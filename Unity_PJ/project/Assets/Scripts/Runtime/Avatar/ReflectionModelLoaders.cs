using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using MascotDesktop.Runtime.Diagnostics;
using UnityEngine;

namespace MascotDesktop.Runtime.Avatar
{
    public sealed class RuntimeErrorDetail
    {
        public string ErrorCode;
        public string Message;
        public string ExceptionType;
        public string ExceptionMessage;
        public string Stage;
        public string Cause;
        public string Path;
        public string SourceTier;
    }

    public sealed class ModelLoadAttemptResult
    {
        public bool Success;
        public GameObject Root;
        public string ErrorCode;
        public string Message;
        public RuntimeErrorDetail Error;
    }

    public sealed class ImageLoadAttemptResult
    {
        public bool Success;
        public Texture Texture;
        public string ErrorCode;
        public string Message;
        public RuntimeErrorDetail Error;
    }

    public static class ReflectionModelLoaders
    {
        private static readonly object ReflectionCacheLock = new object();
        private static readonly Dictionary<string, Type> TypeCache = new Dictionary<string, Type>(StringComparer.Ordinal);
        private static readonly Dictionary<string, MethodInfo> MethodCache = new Dictionary<string, MethodInfo>(StringComparer.Ordinal);
        private static readonly Dictionary<string, MethodInfo[]> WhitelistMethodCache = new Dictionary<string, MethodInfo[]>(StringComparer.Ordinal);

        private static readonly string[] PmxFallbackAssemblyPrefixes =
        {
            "Assembly-CSharp",
            "Assembly-CSharp-Editor",
            "LibMMD",
            "MascotDesktop"
        };

        private static readonly string[] PmxFallbackTypeMarkers =
        {
            "Pmx",
            "PMX",
            "Mmd",
            "ModelLoader"
        };

        private static readonly HashSet<string> PmxFallbackMethodNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Load",
            "LoadPmx",
            "Import",
            "CreateFromFile"
        };

        private static Type[] pmxFallbackTypesCache = Array.Empty<Type>();
        private static bool pmxFallbackTypesCacheReady;
        private static Func<string, Type> typeResolverOverrideForTests;
        private static Func<Type[]> pmxFallbackTypesOverrideForTests;

        public static void SetTypeResolverOverrideForTests(Func<string, Type> resolver)
        {
            lock (ReflectionCacheLock)
            {
                typeResolverOverrideForTests = resolver;
                ClearReflectionCachesLocked();
            }
        }

        public static void SetPmxFallbackTypesOverrideForTests(Func<Type[]> provider)
        {
            lock (ReflectionCacheLock)
            {
                pmxFallbackTypesOverrideForTests = provider;
                ClearReflectionCachesLocked();
            }
        }

        public static void ResetTestingOverrides()
        {
            lock (ReflectionCacheLock)
            {
                typeResolverOverrideForTests = null;
                pmxFallbackTypesOverrideForTests = null;
                ClearReflectionCachesLocked();
            }
        }

        public static ImageLoadAttemptResult TryLoadImageTexture(string absolutePath, string requestId = null, string sourceTier = null)
        {
            requestId = EnsureRequestId(requestId);
            sourceTier = NormalizeSourceTier(sourceTier);

            const string classifyStage = "classify";
            LogStageBegin(requestId, classifyStage, absolutePath, sourceTier, "image_path_validation");
            if (!File.Exists(absolutePath))
            {
                var detail = BuildErrorDetail(
                    "ASSET.READ.FILE_NOT_FOUND",
                    "image file not found",
                    classifyStage,
                    "file_not_found",
                    absolutePath,
                    sourceTier);
                LogStageFail(requestId, detail);
                return ImageFail(detail);
            }

            LogStageEnd(requestId, classifyStage, absolutePath, sourceTier, "image_path_validation");

            var ext = Path.GetExtension(absolutePath)?.ToLowerInvariant();
            const string invokeStage = "reflection_invoke";
            try
            {
                Texture texture = null;
                if (string.Equals(ext, ".bmp", StringComparison.OrdinalIgnoreCase))
                {
                    LogStageBegin(requestId, invokeStage, absolutePath, sourceTier, "libmmd.bmp_loader");
                    texture = TryLoadBmpViaLibMmd(absolutePath);
                    LogStageEnd(requestId, invokeStage, absolutePath, sourceTier, "libmmd.bmp_loader");
                }

                if (texture == null)
                {
                    LogStageBegin(requestId, invokeStage, absolutePath, sourceTier, "unity.texture_loader");
                    texture = TryLoadTextureWithUnity(absolutePath);
                    LogStageEnd(requestId, invokeStage, absolutePath, sourceTier, "unity.texture_loader");
                }

                const string normalizeStage = "post-normalize";
                LogStageBegin(requestId, normalizeStage, absolutePath, sourceTier, "image_decode_result");
                if (texture == null)
                {
                    var decodeDetail = BuildErrorDetail(
                        "ASSET.READ.DECODE_FAILED",
                        $"failed to decode image: ext={ext ?? "(none)"}",
                        normalizeStage,
                        "decode_returned_null",
                        absolutePath,
                        sourceTier);
                    LogStageFail(requestId, decodeDetail);
                    return ImageFail(decodeDetail);
                }

                LogStageEnd(requestId, normalizeStage, absolutePath, sourceTier, "image_decode_result");
                return ImageOk(texture);
            }
            catch (Exception ex)
            {
                var detail = BuildErrorDetail(
                    "ASSET.READ.DECODE_FAILED",
                    "image decode invoke failed",
                    invokeStage,
                    "decode_exception",
                    absolutePath,
                    sourceTier,
                    ex);
                LogStageFail(requestId, detail);
                return ImageFail(detail);
            }
        }

        public static ModelLoadAttemptResult TryLoadVrm(string absolutePath, string requestId = null, string sourceTier = null)
        {
            requestId = EnsureRequestId(requestId);
            sourceTier = NormalizeSourceTier(sourceTier);

            const string classifyStage = "classify";
            LogStageBegin(requestId, classifyStage, absolutePath, sourceTier, "vrm_path_validation");
            if (!File.Exists(absolutePath))
            {
                var detail = BuildErrorDetail(
                    "ASSET.READ.FILE_NOT_FOUND",
                    "vrm file not found",
                    classifyStage,
                    "file_not_found",
                    absolutePath,
                    sourceTier);
                LogStageFail(requestId, detail);
                return Fail(detail);
            }

            LogStageEnd(requestId, classifyStage, absolutePath, sourceTier, "vrm_path_validation");

            byte[] bytes;
            const string resolveStage = "resolve";
            LogStageBegin(requestId, resolveStage, absolutePath, sourceTier, "vrm.read_bytes");
            try
            {
                bytes = File.ReadAllBytes(absolutePath);
            }
            catch (Exception ex)
            {
                var detail = BuildErrorDetail(
                    "ASSET.READ.DECODE_FAILED",
                    "failed to read vrm bytes",
                    resolveStage,
                    "read_all_bytes_failed",
                    absolutePath,
                    sourceTier,
                    ex);
                LogStageFail(requestId, detail);
                return Fail(detail);
            }

            var vrmImporterType = FindType("VRM.VRMImporterContext");
            if (vrmImporterType == null)
            {
                var detail = BuildErrorDetail(
                    "AVATAR.VRM.LOADER_NOT_FOUND",
                    "VRM loader package is not installed",
                    resolveStage,
                    "type_not_found",
                    absolutePath,
                    sourceTier);
                LogStageFail(requestId, detail);
                return Fail(detail);
            }

            LogStageEnd(requestId, resolveStage, absolutePath, sourceTier, "vrm.read_bytes");

            const string instantiateStage = "instantiate";
            LogStageBegin(requestId, instantiateStage, absolutePath, sourceTier, "vrm.create_context");
            object context;
            try
            {
                context = Activator.CreateInstance(vrmImporterType);
            }
            catch (Exception ex)
            {
                var detail = BuildErrorDetail(
                    "AVATAR.VRM.API_MISMATCH",
                    "failed to create VRM importer context",
                    instantiateStage,
                    "context_create_failed",
                    absolutePath,
                    sourceTier,
                    ex);
                LogStageFail(requestId, detail);
                return Fail(detail);
            }

            if (context == null)
            {
                var detail = BuildErrorDetail(
                    "AVATAR.VRM.API_MISMATCH",
                    "VRM importer context is null",
                    instantiateStage,
                    "context_null",
                    absolutePath,
                    sourceTier);
                LogStageFail(requestId, detail);
                return Fail(detail);
            }

            LogStageEnd(requestId, instantiateStage, absolutePath, sourceTier, "vrm.create_context");

            const string invokeStage = "reflection_invoke";
            LogStageBegin(requestId, invokeStage, absolutePath, sourceTier, "vrm.parse_load");
            var parseGlb = GetCachedMethod(vrmImporterType, "ParseGlb", BindingFlags.Public | BindingFlags.Instance, new[] { typeof(byte[]) });
            var load = GetCachedMethod(vrmImporterType, "Load", BindingFlags.Public | BindingFlags.Instance, Type.EmptyTypes);
            if (parseGlb == null || load == null)
            {
                var detail = BuildErrorDetail(
                    "AVATAR.VRM.API_MISMATCH",
                    "VRM importer API mismatch",
                    invokeStage,
                    "required_method_missing",
                    absolutePath,
                    sourceTier);
                LogStageFail(requestId, detail);
                return Fail(detail);
            }

            try
            {
                parseGlb.Invoke(context, new object[] { bytes });
                load.Invoke(context, null);
                var showMeshes = GetCachedMethod(vrmImporterType, "ShowMeshes", BindingFlags.Public | BindingFlags.Instance, Type.EmptyTypes);
                showMeshes?.Invoke(context, null);
            }
            catch (Exception ex)
            {
                var detail = BuildErrorDetail(
                    "AVATAR.VRM.INVOKE_FAILED",
                    "VRM reflection invoke failed",
                    invokeStage,
                    "parse_load_invoke_failed",
                    absolutePath,
                    sourceTier,
                    ex);
                LogStageFail(requestId, detail);
                return Fail(detail);
            }

            LogStageEnd(requestId, invokeStage, absolutePath, sourceTier, "vrm.parse_load");

            const string normalizeStage = "post-normalize";
            LogStageBegin(requestId, normalizeStage, absolutePath, sourceTier, "vrm.extract_root");
            var root = ExtractGameObject(context, vrmImporterType, "Root", "RootGameObject");
            if (root == null)
            {
                var detail = BuildErrorDetail(
                    "AVATAR.VRM.ROOT_NOT_FOUND",
                    "VRM importer did not provide root object",
                    normalizeStage,
                    "null_root",
                    absolutePath,
                    sourceTier);
                LogStageFail(requestId, detail);
                return Fail(detail);
            }

            LogStageEnd(requestId, normalizeStage, absolutePath, sourceTier, "vrm.extract_root");
            return Ok(root);
        }

        public static ModelLoadAttemptResult TryLoadPmx(string absolutePath, string requestId = null, string sourceTier = null)
        {
            requestId = EnsureRequestId(requestId);
            sourceTier = NormalizeSourceTier(sourceTier);

            const string classifyStage = "classify";
            LogStageBegin(requestId, classifyStage, absolutePath, sourceTier, "pmx_path_validation");
            if (!File.Exists(absolutePath))
            {
                var detail = BuildErrorDetail(
                    "ASSET.READ.FILE_NOT_FOUND",
                    "pmx file not found",
                    classifyStage,
                    "file_not_found",
                    absolutePath,
                    sourceTier);
                LogStageFail(requestId, detail);
                return Fail(detail);
            }

            LogStageEnd(requestId, classifyStage, absolutePath, sourceTier, "pmx_path_validation");

            var primaryResult = TryLoadPmxViaPrimaryPath(absolutePath, requestId, sourceTier);
            if (primaryResult.Success || !string.Equals(primaryResult.ErrorCode, "AVATAR.PMX.LOADER_NOT_FOUND", StringComparison.Ordinal))
            {
                return primaryResult;
            }

            return TryLoadPmxViaWhitelistedFallback(absolutePath, requestId, sourceTier);
        }

        private static ModelLoadAttemptResult TryLoadPmxViaPrimaryPath(string absolutePath, string requestId, string sourceTier)
        {
            const string resolveStage = "resolve";
            LogStageBegin(requestId, resolveStage, absolutePath, sourceTier, "pmx.primary_type_lookup");
            var mmdGameObjectType = FindType("LibMMD.Unity3D.MmdGameObject");
            if (mmdGameObjectType == null)
            {
                var detail = BuildErrorDetail(
                    "AVATAR.PMX.LOADER_NOT_FOUND",
                    "primary PMX loader type not found",
                    resolveStage,
                    "primary_type_missing",
                    absolutePath,
                    sourceTier);
                LogStageFail(requestId, detail);
                return Fail(detail);
            }

            LogStageEnd(requestId, resolveStage, absolutePath, sourceTier, "pmx.primary_type_lookup");

            const string instantiateStage = "instantiate";
            LogStageBegin(requestId, instantiateStage, absolutePath, sourceTier, "pmx.primary_create_gameobject");
            var createMethod = GetCachedMethod(mmdGameObjectType, "CreateGameObject", BindingFlags.Public | BindingFlags.Static, new[] { typeof(string) });
            if (createMethod == null)
            {
                var detail = BuildErrorDetail(
                    "AVATAR.PMX.API_MISMATCH",
                    "MmdGameObject.CreateGameObject was not found",
                    instantiateStage,
                    "create_method_missing",
                    absolutePath,
                    sourceTier);
                LogStageFail(requestId, detail);
                return Fail(detail);
            }

            GameObject gameObject;
            try
            {
                gameObject = createMethod.Invoke(null, new object[] { "PMXModel" }) as GameObject;
            }
            catch (Exception ex)
            {
                var detail = BuildErrorDetail(
                    "AVATAR.PMX.INVOKE_FAILED",
                    "failed to invoke CreateGameObject",
                    instantiateStage,
                    "create_invoke_failed",
                    absolutePath,
                    sourceTier,
                    ex);
                LogStageFail(requestId, detail);
                return Fail(detail);
            }

            if (gameObject == null)
            {
                var detail = BuildErrorDetail(
                    "AVATAR.PMX.ROOT_NOT_FOUND",
                    "CreateGameObject returned null",
                    instantiateStage,
                    "create_returned_null",
                    absolutePath,
                    sourceTier);
                LogStageFail(requestId, detail);
                return Fail(detail);
            }

            LogStageEnd(requestId, instantiateStage, absolutePath, sourceTier, "pmx.primary_create_gameobject");

            var component = gameObject.GetComponent(mmdGameObjectType);
            if (component == null)
            {
                UnityEngine.Object.Destroy(gameObject);
                var detail = BuildErrorDetail(
                    "AVATAR.PMX.API_MISMATCH",
                    "MmdGameObject component was not attached",
                    instantiateStage,
                    "component_missing",
                    absolutePath,
                    sourceTier);
                LogStageFail(requestId, detail);
                return Fail(detail);
            }

            const string invokeStage = "reflection_invoke";
            LogStageBegin(requestId, invokeStage, absolutePath, sourceTier, "pmx.primary_loadmodel");
            var loadModelMethod = GetCachedMethod(mmdGameObjectType, "LoadModel", BindingFlags.Public | BindingFlags.Instance, new[] { typeof(string) });
            if (loadModelMethod == null)
            {
                UnityEngine.Object.Destroy(gameObject);
                var detail = BuildErrorDetail(
                    "AVATAR.PMX.API_MISMATCH",
                    "MmdGameObject.LoadModel was not found",
                    invokeStage,
                    "load_method_missing",
                    absolutePath,
                    sourceTier);
                LogStageFail(requestId, detail);
                return Fail(detail);
            }

            try
            {
                var rawResult = loadModelMethod.Invoke(component, new object[] { absolutePath });
                if (rawResult is bool success && success)
                {
                    LogStageEnd(requestId, invokeStage, absolutePath, sourceTier, "pmx.primary_loadmodel");
                    return Ok(gameObject);
                }

                UnityEngine.Object.Destroy(gameObject);
                var falseDetail = BuildErrorDetail(
                    "AVATAR.PMX.LOAD_FAILED",
                    "MmdGameObject.LoadModel returned false",
                    invokeStage,
                    "load_returned_false",
                    absolutePath,
                    sourceTier);
                LogStageFail(requestId, falseDetail);
                return Fail(falseDetail);
            }
            catch (Exception ex)
            {
                UnityEngine.Object.Destroy(gameObject);
                var detail = BuildErrorDetail(
                    "AVATAR.PMX.INVOKE_FAILED",
                    "MmdGameObject.LoadModel invoke failed",
                    invokeStage,
                    "load_invoke_failed",
                    absolutePath,
                    sourceTier,
                    ex);
                LogStageFail(requestId, detail);
                return Fail(detail);
            }
        }

        private static ModelLoadAttemptResult TryLoadPmxViaWhitelistedFallback(string absolutePath, string requestId, string sourceTier)
        {
            const string resolveStage = "resolve";
            LogStageBegin(requestId, resolveStage, absolutePath, sourceTier, "pmx.fallback_whitelist_scan");
            var candidateTypes = GetWhitelistedPmxFallbackTypes();
            LogStageEnd(requestId, resolveStage, absolutePath, sourceTier, "pmx.fallback_whitelist_scan");
            if (candidateTypes.Length == 0)
            {
                var detail = BuildErrorDetail(
                    "AVATAR.PMX.LOADER_NOT_FOUND",
                    "PMX fallback loader is not available",
                    resolveStage,
                    "fallback_type_not_found",
                    absolutePath,
                    sourceTier);
                LogStageFail(requestId, detail);
                return Fail(detail);
            }

            var attemptedMethodCount = 0;
            var nullRootCount = 0;
            const string invokeStage = "reflection_invoke";
            foreach (var type in candidateTypes)
            {
                var methods = GetWhitelistedFallbackMethods(type);
                foreach (var method in methods)
                {
                    attemptedMethodCount++;
                    var invokeCause = $"{type.FullName}.{method.Name}";
                    LogStageBegin(requestId, invokeStage, absolutePath, sourceTier, invokeCause);
                    try
                    {
                        var raw = method.Invoke(null, new object[] { absolutePath });
                        var root = ConvertToGameObject(raw);
                        if (root != null)
                        {
                            LogStageEnd(requestId, invokeStage, absolutePath, sourceTier, invokeCause);
                            return Ok(root);
                        }

                        nullRootCount++;
                        LogStageEnd(requestId, invokeStage, absolutePath, sourceTier, $"{invokeCause}:null_root");
                    }
                    catch (Exception ex)
                    {
                        var detail = BuildErrorDetail(
                            "AVATAR.PMX.INVOKE_FAILED",
                            "whitelisted PMX fallback invoke failed",
                            invokeStage,
                            invokeCause,
                            absolutePath,
                            sourceTier,
                            ex);
                        LogStageFail(requestId, detail);
                        return Fail(detail);
                    }
                }
            }

            if (attemptedMethodCount > 0 && nullRootCount == attemptedMethodCount)
            {
                var rootDetail = BuildErrorDetail(
                    "AVATAR.PMX.ROOT_NOT_FOUND",
                    "whitelisted PMX fallback returned null root",
                    "post-normalize",
                    "fallback_root_null",
                    absolutePath,
                    sourceTier);
                LogStageFail(requestId, rootDetail);
                return Fail(rootDetail);
            }

            var notFoundDetail = BuildErrorDetail(
                "AVATAR.PMX.LOADER_NOT_FOUND",
                "PMX fallback loader package is not installed",
                resolveStage,
                "fallback_method_not_found",
                absolutePath,
                sourceTier);
            LogStageFail(requestId, notFoundDetail);
            return Fail(notFoundDetail);
        }

        private static ModelLoadAttemptResult Ok(GameObject root)
        {
            return new ModelLoadAttemptResult
            {
                Success = true,
                Root = root
            };
        }

        private static ImageLoadAttemptResult ImageOk(Texture texture)
        {
            return new ImageLoadAttemptResult
            {
                Success = true,
                Texture = texture
            };
        }

        private static ModelLoadAttemptResult Fail(RuntimeErrorDetail detail)
        {
            return new ModelLoadAttemptResult
            {
                Success = false,
                ErrorCode = detail?.ErrorCode,
                Message = detail?.Message,
                Error = detail
            };
        }

        private static ImageLoadAttemptResult ImageFail(RuntimeErrorDetail detail)
        {
            return new ImageLoadAttemptResult
            {
                Success = false,
                ErrorCode = detail?.ErrorCode,
                Message = detail?.Message,
                Error = detail
            };
        }

        private static Type FindType(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
            {
                return null;
            }

            lock (ReflectionCacheLock)
            {
                if (TypeCache.ContainsKey(fullName))
                {
                    return TypeCache[fullName];
                }
            }

            Type resolved = null;
            Func<string, Type> overrideResolver;
            lock (ReflectionCacheLock)
            {
                overrideResolver = typeResolverOverrideForTests;
            }

            if (overrideResolver != null)
            {
                resolved = overrideResolver(fullName);
            }
            else
            {
                foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    var type = asm.GetType(fullName, throwOnError: false);
                    if (type != null)
                    {
                        resolved = type;
                        break;
                    }
                }
            }

            lock (ReflectionCacheLock)
            {
                TypeCache[fullName] = resolved;
            }

            return resolved;
        }

        private static MethodInfo GetCachedMethod(Type type, string methodName, BindingFlags bindingFlags, Type[] parameterTypes)
        {
            if (type == null || string.IsNullOrWhiteSpace(methodName))
            {
                return null;
            }

            parameterTypes = parameterTypes ?? Type.EmptyTypes;
            var key = BuildMethodCacheKey(type, methodName, bindingFlags, parameterTypes);
            lock (ReflectionCacheLock)
            {
                if (MethodCache.ContainsKey(key))
                {
                    return MethodCache[key];
                }
            }

            MethodInfo method = null;
            try
            {
                method = type.GetMethod(methodName, bindingFlags, null, parameterTypes, null);
            }
            catch
            {
                method = null;
            }

            lock (ReflectionCacheLock)
            {
                MethodCache[key] = method;
            }

            return method;
        }

        private static string BuildMethodCacheKey(Type type, string methodName, BindingFlags bindingFlags, Type[] parameterTypes)
        {
            var parameterSignature = parameterTypes.Length == 0
                ? "none"
                : string.Join(",", parameterTypes.Select(p => p == null ? "null" : p.FullName));
            return $"{type.AssemblyQualifiedName}|{methodName}|{(int)bindingFlags}|{parameterSignature}";
        }

        private static Type[] GetWhitelistedPmxFallbackTypes()
        {
            Func<Type[]> overrideProvider;
            lock (ReflectionCacheLock)
            {
                overrideProvider = pmxFallbackTypesOverrideForTests;
            }

            if (overrideProvider != null)
            {
                var provided = overrideProvider() ?? Array.Empty<Type>();
                return provided.Where(t => t != null).Distinct().ToArray();
            }

            lock (ReflectionCacheLock)
            {
                if (pmxFallbackTypesCacheReady)
                {
                    return pmxFallbackTypesCache;
                }
            }

            var discoveredTypes = AppDomain.CurrentDomain
                .GetAssemblies()
                .Where(IsWhitelistedFallbackAssembly)
                .SelectMany(GetAssemblyTypesSafely)
                .Where(IsWhitelistedFallbackType)
                .Distinct()
                .ToArray();

            lock (ReflectionCacheLock)
            {
                pmxFallbackTypesCache = discoveredTypes;
                pmxFallbackTypesCacheReady = true;
            }

            return discoveredTypes;
        }

        private static MethodInfo[] GetWhitelistedFallbackMethods(Type type)
        {
            if (type == null)
            {
                return Array.Empty<MethodInfo>();
            }

            var key = type.AssemblyQualifiedName ?? type.FullName ?? type.Name;
            lock (ReflectionCacheLock)
            {
                if (WhitelistMethodCache.TryGetValue(key, out var cached))
                {
                    return cached;
                }
            }

            MethodInfo[] methods;
            try
            {
                methods = type.GetMethods(BindingFlags.Public | BindingFlags.Static)
                    .Where(IsWhitelistedFallbackMethod)
                    .ToArray();
            }
            catch
            {
                methods = Array.Empty<MethodInfo>();
            }

            lock (ReflectionCacheLock)
            {
                WhitelistMethodCache[key] = methods;
            }

            return methods;
        }

        private static bool IsWhitelistedFallbackAssembly(Assembly assembly)
        {
            if (assembly == null)
            {
                return false;
            }

            var name = assembly.GetName().Name ?? string.Empty;
            for (var i = 0; i < PmxFallbackAssemblyPrefixes.Length; i++)
            {
                if (name.StartsWith(PmxFallbackAssemblyPrefixes[i], StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        private static IEnumerable<Type> GetAssemblyTypesSafely(Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                return ex.Types.Where(t => t != null);
            }
            catch
            {
                return Array.Empty<Type>();
            }
        }

        private static bool IsWhitelistedFallbackType(Type type)
        {
            if (type == null)
            {
                return false;
            }

            var fullName = type.FullName ?? string.Empty;
            for (var i = 0; i < PmxFallbackTypeMarkers.Length; i++)
            {
                if (fullName.IndexOf(PmxFallbackTypeMarkers[i], StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsWhitelistedFallbackMethod(MethodInfo method)
        {
            if (method == null)
            {
                return false;
            }

            if (!PmxFallbackMethodNames.Contains(method.Name))
            {
                return false;
            }

            var parameters = method.GetParameters();
            if (parameters.Length != 1 || parameters[0].ParameterType != typeof(string))
            {
                return false;
            }

            var returnType = method.ReturnType;
            return returnType == typeof(GameObject) ||
                   typeof(Component).IsAssignableFrom(returnType) ||
                   returnType == typeof(object);
        }

        private static Texture TryLoadBmpViaLibMmd(string absolutePath)
        {
            var textureLoaderType = FindType("LibMMD.Unity3D.TextureLoader");
            if (textureLoaderType == null)
            {
                return null;
            }

            var loadBmp = GetCachedMethod(textureLoaderType, "LoadBmp", BindingFlags.NonPublic | BindingFlags.Static, new[] { typeof(string) });
            if (loadBmp == null)
            {
                return null;
            }

            return loadBmp.Invoke(null, new object[] { absolutePath }) as Texture;
        }

        private static Texture TryLoadTextureWithUnity(string absolutePath)
        {
            var bytes = File.ReadAllBytes(absolutePath);
            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (texture.LoadImage(bytes))
            {
                return texture;
            }

            if (Application.isPlaying)
            {
                UnityEngine.Object.Destroy(texture);
            }
            else
            {
                UnityEngine.Object.DestroyImmediate(texture);
            }
            return null;
        }

        private static RuntimeErrorDetail BuildErrorDetail(
            string errorCode,
            string message,
            string stage,
            string cause,
            string path,
            string sourceTier,
            Exception ex = null)
        {
            var normalizedException = UnwrapException(ex);
            return new RuntimeErrorDetail
            {
                ErrorCode = string.IsNullOrWhiteSpace(errorCode) ? "RUNTIME.ERROR.UNSPECIFIED" : errorCode,
                Message = message ?? string.Empty,
                ExceptionType = normalizedException?.GetType().FullName ?? string.Empty,
                ExceptionMessage = normalizedException?.Message ?? string.Empty,
                Stage = stage ?? string.Empty,
                Cause = cause ?? string.Empty,
                Path = path ?? string.Empty,
                SourceTier = NormalizeSourceTier(sourceTier)
            };
        }

        private static Exception UnwrapException(Exception ex)
        {
            var current = ex;
            while (current is TargetInvocationException invocation && invocation.InnerException != null)
            {
                current = invocation.InnerException;
            }

            return current;
        }

        private static string EnsureRequestId(string requestId)
        {
            return string.IsNullOrWhiteSpace(requestId) ? RuntimeLog.NewRequestId() : requestId;
        }

        private static string NormalizeSourceTier(string sourceTier)
        {
            return string.IsNullOrWhiteSpace(sourceTier) ? "loader" : sourceTier;
        }

        private static string BuildStageMessage(string stage, string cause, string message = null)
        {
            var safeStage = string.IsNullOrWhiteSpace(stage) ? "unknown" : stage;
            var safeCause = string.IsNullOrWhiteSpace(cause) ? "none" : cause;
            if (string.IsNullOrWhiteSpace(message))
            {
                return $"stage={safeStage};cause={safeCause}";
            }

            return $"stage={safeStage};cause={safeCause};message={message}";
        }

        private static void LogStageBegin(string requestId, string stage, string path, string sourceTier, string cause)
        {
            RuntimeLog.Info(
                "avatar.loader",
                "avatar.loader.stage.begin",
                requestId,
                BuildStageMessage(stage, cause),
                path,
                sourceTier);
        }

        private static void LogStageEnd(string requestId, string stage, string path, string sourceTier, string cause)
        {
            RuntimeLog.Info(
                "avatar.loader",
                "avatar.loader.stage.end",
                requestId,
                BuildStageMessage(stage, cause),
                path,
                sourceTier);
        }

        private static void LogStageFail(string requestId, RuntimeErrorDetail detail)
        {
            if (detail == null)
            {
                return;
            }

            RuntimeLog.Error(
                "avatar.loader",
                "avatar.loader.stage.fail",
                requestId,
                detail.ErrorCode,
                BuildStageMessage(detail.Stage, detail.Cause, detail.Message),
                detail.Path,
                detail.SourceTier,
                ex: null,
                exceptionType: detail.ExceptionType,
                exceptionMessage: detail.ExceptionMessage);
        }

        private static void ClearReflectionCachesLocked()
        {
            TypeCache.Clear();
            MethodCache.Clear();
            WhitelistMethodCache.Clear();
            pmxFallbackTypesCache = Array.Empty<Type>();
            pmxFallbackTypesCacheReady = false;
        }

        private static GameObject ExtractGameObject(object instance, Type type, params string[] memberNames)
        {
            foreach (var name in memberNames)
            {
                var prop = type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
                if (prop != null)
                {
                    var value = prop.GetValue(instance);
                    var root = ConvertToGameObject(value);
                    if (root != null)
                    {
                        return root;
                    }
                }

                var field = type.GetField(name, BindingFlags.Public | BindingFlags.Instance);
                if (field != null)
                {
                    var value = field.GetValue(instance);
                    var root = ConvertToGameObject(value);
                    if (root != null)
                    {
                        return root;
                    }
                }
            }

            return null;
        }

        private static GameObject ConvertToGameObject(object value)
        {
            if (value == null)
            {
                return null;
            }

            if (value is GameObject go)
            {
                return go;
            }

            if (value is Component comp)
            {
                return comp.gameObject;
            }

            return null;
        }
    }
}
