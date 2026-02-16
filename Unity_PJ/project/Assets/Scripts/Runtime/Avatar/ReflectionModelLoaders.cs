using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace MascotDesktop.Runtime.Avatar
{
    public sealed class ModelLoadAttemptResult
    {
        public bool Success;
        public GameObject Root;
        public string ErrorCode;
        public string Message;
    }

    public sealed class ImageLoadAttemptResult
    {
        public bool Success;
        public Texture Texture;
        public string ErrorCode;
        public string Message;
    }

    public static class ReflectionModelLoaders
    {
        public static ImageLoadAttemptResult TryLoadImageTexture(string absolutePath)
        {
            if (!File.Exists(absolutePath))
            {
                return ImageFail("ASSET.READ.FILE_NOT_FOUND", "image file not found");
            }

            var ext = Path.GetExtension(absolutePath)?.ToLowerInvariant();
            try
            {
                Texture texture = null;
                if (string.Equals(ext, ".bmp", StringComparison.OrdinalIgnoreCase))
                {
                    texture = TryLoadBmpViaLibMmd(absolutePath);
                }

                if (texture == null)
                {
                    texture = TryLoadTextureWithUnity(absolutePath);
                }

                if (texture == null)
                {
                    return ImageFail("ASSET.READ.DECODE_FAILED", $"failed to decode image: ext={ext ?? "(none)"}");
                }

                return ImageOk(texture);
            }
            catch (Exception ex)
            {
                return ImageFail("ASSET.READ.DECODE_FAILED", ex.GetBaseException().Message);
            }
        }

        public static ModelLoadAttemptResult TryLoadVrm(string absolutePath)
        {
            if (!File.Exists(absolutePath))
            {
                return Fail("ASSET.READ.FILE_NOT_FOUND", "vrm file not found");
            }

            // UniVRM 0.x path: VRM.VRMImporterContext
            var vrmImporterType = FindType("VRM.VRMImporterContext");
            if (vrmImporterType != null)
            {
                try
                {
                    var bytes = File.ReadAllBytes(absolutePath);
                    var context = Activator.CreateInstance(vrmImporterType);
                    var parseGlb = vrmImporterType.GetMethod("ParseGlb", BindingFlags.Public | BindingFlags.Instance, null, new[] { typeof(byte[]) }, null);
                    var load = vrmImporterType.GetMethod("Load", BindingFlags.Public | BindingFlags.Instance, null, Type.EmptyTypes, null);

                    if (parseGlb == null || load == null)
                    {
                        return Fail("AVATAR.VRM.API_MISMATCH", "VRM importer API mismatch");
                    }

                    parseGlb.Invoke(context, new object[] { bytes });
                    load.Invoke(context, null);

                    var showMeshes = vrmImporterType.GetMethod("ShowMeshes", BindingFlags.Public | BindingFlags.Instance, null, Type.EmptyTypes, null);
                    showMeshes?.Invoke(context, null);

                    var root = ExtractGameObject(context, vrmImporterType, "Root", "RootGameObject");
                    if (root == null)
                    {
                        return Fail("AVATAR.VRM.ROOT_NOT_FOUND", "VRM importer did not provide root object");
                    }

                    return Ok(root);
                }
                catch (Exception ex)
                {
                    return Fail("AVATAR.VRM.LOAD_FAILED", ex.GetBaseException().Message);
                }
            }

            return Fail("AVATAR.VRM.LOADER_NOT_FOUND", "VRM loader package is not installed");
        }

        public static ModelLoadAttemptResult TryLoadPmx(string absolutePath)
        {
            if (!File.Exists(absolutePath))
            {
                return Fail("ASSET.READ.FILE_NOT_FOUND", "pmx file not found");
            }

            // Primary path: LibMMD.Unity3D.MmdGameObject
            try
            {
                var mmdGameObjectType = FindType("LibMMD.Unity3D.MmdGameObject");
                if (mmdGameObjectType != null)
                {
                    var createMethod = mmdGameObjectType.GetMethod("CreateGameObject", BindingFlags.Public | BindingFlags.Static);
                    if (createMethod != null)
                    {
                        var gameObject = createMethod.Invoke(null, new object[] { "PMXModel" }) as GameObject;
                        if (gameObject != null)
                        {
                            var mmdComponent = gameObject.GetComponent(mmdGameObjectType);
                            if (mmdComponent != null)
                            {
                                var loadModelMethod = mmdGameObjectType.GetMethod("LoadModel", BindingFlags.Public | BindingFlags.Instance);
                                if (loadModelMethod != null)
                                {
                                    var result = loadModelMethod.Invoke(mmdComponent, new object[] { absolutePath });
                                    if (result is bool success && success)
                                    {
                                        return Ok(gameObject);
                                    }
                                    else
                                    {
                                        UnityEngine.Object.Destroy(gameObject);
                                        return Fail("AVATAR.PMX.LOAD_FAILED", "MmdGameObject.LoadModel returned false");
                                    }
                                }
                            }
                            UnityEngine.Object.Destroy(gameObject);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return Fail("AVATAR.PMX.LOAD_FAILED", ex.GetBaseException().Message);
            }

            // Fallback: reflection-based search for any PMX loader
            try
            {
                var candidates = AppDomain.CurrentDomain
                    .GetAssemblies()
                    .SelectMany(a =>
                    {
                        try
                        {
                            return a.GetTypes();
                        }
                        catch (ReflectionTypeLoadException e)
                        {
                            return e.Types.Where(t => t != null);
                        }
                    })
                    .Where(t =>
                    {
                        if (t == null)
                        {
                            return false;
                        }

                        var fullName = t.FullName ?? string.Empty;
                        return t.Name.IndexOf("pmx", StringComparison.OrdinalIgnoreCase) >= 0 ||
                               fullName.IndexOf("pmx", StringComparison.OrdinalIgnoreCase) >= 0;
                    })
                    .ToArray();

                foreach (var type in candidates)
                {
                    // Candidate: static loader method like Load(string path) => GameObject
                    var staticMethods = type.GetMethods(BindingFlags.Public | BindingFlags.Static);
                    foreach (var method in staticMethods)
                    {
                        var parameters = method.GetParameters();
                        if (parameters.Length != 1 || parameters[0].ParameterType != typeof(string))
                        {
                            continue;
                        }

                        var raw = method.Invoke(null, new object[] { absolutePath });
                        var root = ConvertToGameObject(raw);
                        if (root != null)
                        {
                            return Ok(root);
                        }
                    }
                }

                return Fail("AVATAR.PMX.LOADER_NOT_FOUND", "PMX loader package is not installed");
            }
            catch (Exception ex)
            {
                return Fail("AVATAR.PMX.LOAD_FAILED", ex.GetBaseException().Message);
            }
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

        private static ModelLoadAttemptResult Fail(string code, string message)
        {
            return new ModelLoadAttemptResult
            {
                Success = false,
                ErrorCode = code,
                Message = message
            };
        }

        private static ImageLoadAttemptResult ImageFail(string code, string message)
        {
            return new ImageLoadAttemptResult
            {
                Success = false,
                ErrorCode = code,
                Message = message
            };
        }

        private static Type FindType(string fullName)
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                var type = asm.GetType(fullName, throwOnError: false);
                if (type != null)
                {
                    return type;
                }
            }

            return null;
        }

        private static Texture TryLoadBmpViaLibMmd(string absolutePath)
        {
            var textureLoaderType = FindType("LibMMD.Unity3D.TextureLoader");
            if (textureLoaderType == null)
            {
                return null;
            }

            var loadBmp = textureLoaderType.GetMethod("LoadBmp", BindingFlags.NonPublic | BindingFlags.Static);
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

            UnityEngine.Object.Destroy(texture);
            return null;
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
