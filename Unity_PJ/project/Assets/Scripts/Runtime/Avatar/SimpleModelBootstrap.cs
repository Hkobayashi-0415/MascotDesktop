using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MascotDesktop.Runtime.Assets;
using MascotDesktop.Runtime.Config;
using MascotDesktop.Runtime.Core;
using MascotDesktop.Runtime.Diagnostics;
using MascotDesktop.Runtime.Ipc;
using MascotDesktop.Runtime.UI;
using MascotDesktop.Runtime.Windowing;
using UnityEngine;
using UnityEngine.Rendering;

namespace MascotDesktop.Runtime.Avatar
{
    [DefaultExecutionOrder(-2000)]
    public sealed class SimpleModelBootstrap : MonoBehaviour
    {
        private const string BootstrapObjectName = "SimpleModelBootstrap";
        private const string FallbackErrorCode = "AVATAR.MODEL.UNSUPPORTED_OR_READ_FAILED";
        private const string ConfigSourceTier = "config_relative";
        private const string RenderSourceTier = "render";
        private const float ModelTargetHeight = 2.4f;
        private const float ModelTargetZ = 2.5f;
        private const float CameraFieldOfView = 24f;
        private const float MinModelScale = 0.05f;
        private const float MaxModelScale = 12f;
        private const float CameraFitPadding = 0.88f;
        private const float MinVisibleMaterialAlpha = 0.02f;
        private const float MinSkinnedBoundsSize = 0.05f;
        private const float FallbackSkinnedBoundsSize = 8f;
        private const float HighShininessThreshold = 32f;
        private const float HighSpecularColorThreshold = 0.9f;
        private const float BrightDiffuseColorThreshold = 0.95f;
        private const int MaterialDiagnosticSamplesLimit = 8;
        private const int ModelPlacementStabilizationFrames = 120;
        private const int MinAntiAliasingSamples = 4;
        private const float DefaultKeyLightShadowStrength = 0.7f;
        private const int MaxSelectedImagesPerCharacter = 4;
        private const string MainTextureStatusTag = "MASCOT_MAIN_TEX_STATUS";
        private const string MainTextureStatusMissingSpec = "missing_spec";
        private const string MainTextureStatusMissingResolve = "missing_resolve";
        private const string TextureStatusNotUsed = "not_used";
        private const string ToonTextureStatusTag = "MASCOT_TOON_TEX_STATUS";
        private const string SphereAddTextureStatusTag = "MASCOT_SPHERE_ADD_TEX_STATUS";
        private const string SphereMulTextureStatusTag = "MASCOT_SPHERE_MUL_TEX_STATUS";
        private const string TransparentReasonTag = "MASCOT_TRANSPARENT_REASON";
        private const string MmdMaterialNameTag = "MASCOT_MMD_MATERIAL_NAME";
        private const string TransparentReasonOpaque = "opaque";
        private const string TransparentReasonDiffuseAlpha = "diffuse_alpha";
        private const string TransparentReasonEdgeAlpha = "edge_alpha";
        private const string TransparentReasonTextureAlpha = "texture_alpha";

        private static readonly string[] AllModelExtensions = { ".pmx", ".pmd", ".vrm" };
        private static readonly string[] CandidateImageExtensions = { ".png", ".jpg", ".jpeg", ".bmp" };
        private static readonly HashSet<string> TextureDiagnosticExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".png",
            ".jpg",
            ".jpeg",
            ".bmp",
            ".tga",
            ".dds",
            ".spa",
            ".sph"
        };

        private readonly struct RenderFactorPreset
        {
            public RenderFactorPreset(string displayName, float keyLightIntensity, Color ambientLightColor, float albedoMultiplier)
            {
                DisplayName = displayName;
                KeyLightIntensity = keyLightIntensity;
                AmbientLightColor = ambientLightColor;
                AlbedoMultiplier = albedoMultiplier;
            }

            public string DisplayName { get; }
            public float KeyLightIntensity { get; }
            public Color AmbientLightColor { get; }
            public float AlbedoMultiplier { get; }
        }

        private static readonly RenderFactorPreset[] RenderFactorPresets =
        {
            new RenderFactorPreset("F0 Baseline", 0.58f, new Color(0.10f, 0.10f, 0.12f, 1f), 1.00f),
            new RenderFactorPreset("F1 Lower Key Light", 0.45f, new Color(0.08f, 0.08f, 0.10f, 1f), 1.00f),
            new RenderFactorPreset("F2 Lower Light + Albedo95", 0.45f, new Color(0.08f, 0.08f, 0.10f, 1f), 0.95f),
            new RenderFactorPreset("F3 Lower Light + Albedo88", 0.38f, new Color(0.07f, 0.07f, 0.09f, 1f), 0.88f)
        };

        private SimpleModelConfig _config;
        private GameObject _activeModelRoot;
        private int _renderFactorIndex;
        private int _placementStabilizationFramesRemaining;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void EnsureBootstrapObject()
        {
            var existing = FindFirstObjectByType<SimpleModelBootstrap>();
            if (existing != null)
            {
                EnsureRuntimeComponents(existing.gameObject);
                return;
            }

            var go = new GameObject(BootstrapObjectName);
            DontDestroyOnLoad(go);
            EnsureRuntimeComponents(go);
            go.AddComponent<SimpleModelBootstrap>();
        }

        private static void EnsureRuntimeComponents(GameObject go)
        {
            EnsureComponent<RuntimeConfig>(go);
            EnsureComponent<CoreOrchestrator>(go);
            EnsureComponent<MotionSlotPlayer>(go);
            EnsureComponent<AvatarStateController>(go);
            EnsureComponent<WindowController>(go);
            EnsureComponent<ResidentController>(go);
            EnsureComponent<LoopbackHttpClient>(go);
            EnsureComponent<RuntimeDebugHud>(go);
            EnsureComponent<SimpleModelConfig>(go);
        }

        private static void EnsureComponent<T>(GameObject go) where T : Component
        {
            if (go.GetComponent<T>() == null)
            {
                go.AddComponent<T>();
            }
        }

        private void Awake()
        {
            _config = GetComponent<SimpleModelConfig>();
            if (_config == null)
            {
                _config = gameObject.AddComponent<SimpleModelConfig>();
            }

            EnsureCameraAndLight();
            BuildSimpleModelView();
        }

        private void LateUpdate()
        {
            if (_placementStabilizationFramesRemaining <= 0 || _activeModelRoot == null)
            {
                return;
            }

            _placementStabilizationFramesRemaining--;
            StabilizeActiveModelPlacement();
        }

        public string[] DiscoverModelCandidates()
        {
            return BuildModelCandidatesFromRelativePaths(DiscoverAllRelativeAssetPaths());
        }

        public string[] DiscoverImageCandidates()
        {
            return BuildImageCandidatesFromRelativePaths(DiscoverAllRelativeAssetPaths());
        }

        public static string[] BuildModelCandidatesFromRelativePaths(IEnumerable<string> allRelativePaths)
        {
            var normalizedRelativePaths = NormalizeRelativePaths(allRelativePaths);
            return normalizedRelativePaths
                .Where(path => AllModelExtensions.Contains(Path.GetExtension(path), StringComparer.OrdinalIgnoreCase))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }

        public static string[] BuildImageCandidatesFromRelativePaths(IEnumerable<string> allRelativePaths)
        {
            var normalizedRelativePaths = NormalizeRelativePaths(allRelativePaths);
            return normalizedRelativePaths
                .Where(path => CandidateImageExtensions.Contains(Path.GetExtension(path), StringComparer.OrdinalIgnoreCase))
                .GroupBy(GetCandidateGroupKey, StringComparer.OrdinalIgnoreCase)
                .SelectMany(group => group
                    .OrderBy(GetImageSelectionRank)
                    .ThenBy(path => path, StringComparer.OrdinalIgnoreCase)
                    .Take(MaxSelectedImagesPerCharacter))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }

        private string[] DiscoverAllRelativeAssetPaths()
        {
            var requestId = RuntimeLog.NewRequestId();
            var (_, _, unityPjRoot) = ResolveProjectRoots(requestId, logResolvedInfo: false);
            if (string.IsNullOrWhiteSpace(unityPjRoot))
            {
                return Array.Empty<string>();
            }

            var assetsRoot = Path.Combine(unityPjRoot, "data", "assets_user");
            if (!Directory.Exists(assetsRoot))
            {
                return Array.Empty<string>();
            }

            return Directory
                .EnumerateFiles(assetsRoot, "*.*", SearchOption.AllDirectories)
                .Select(path => path.Substring(assetsRoot.Length + 1).Replace('\\', '/'))
                .ToArray();
        }

        private static string[] NormalizeRelativePaths(IEnumerable<string> allRelativePaths)
        {
            if (allRelativePaths == null)
            {
                return Array.Empty<string>();
            }

            return allRelativePaths
                .Where(path => !string.IsNullOrWhiteSpace(path))
                .Select(path => path.Trim().Replace('\\', '/'))
                .ToArray();
        }

        private static string GetCandidateGroupKey(string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
            {
                return "__misc__";
            }

            var normalized = relativePath.Replace('\\', '/');
            var segments = normalized.Split('/');
            if (segments.Length >= 2)
            {
                return $"{segments[0]}/{segments[1]}";
            }

            return segments[0];
        }

        private static int GetImageSelectionRank(string relativePath)
        {
            var name = Path.GetFileNameWithoutExtension(relativePath)?.ToLowerInvariant() ?? string.Empty;
            if (name.Contains("face"))
            {
                return 0;
            }

            if (name.Contains("body") || name.Contains("skin"))
            {
                return 1;
            }

            if (name.Contains("hair"))
            {
                return 2;
            }

            if (name.Contains("main") || name.Contains("diffuse") || name.Contains("albedo") || name.Contains("base"))
            {
                return 3;
            }

            if (name.Contains("tex"))
            {
                return 4;
            }

            return 10;
        }

        public void ReloadModel()
        {
            BuildSimpleModelView();
        }

        public void LoadModelByRelativePath(string modelRelativePath)
        {
            if (_config == null)
            {
                _config = GetComponent<SimpleModelConfig>() ?? gameObject.AddComponent<SimpleModelConfig>();
            }

            _config.modelRelativePath = modelRelativePath ?? string.Empty;
            ReloadModel();
        }

        public string[] GetRenderFactorNames()
        {
            return RenderFactorPresets.Select(preset => preset.DisplayName).ToArray();
        }

        public int GetRenderFactorIndex()
        {
            return _renderFactorIndex;
        }

        public void SetRenderFactorIndex(int index)
        {
            var normalized = NormalizeRenderFactorIndex(index);
            if (_renderFactorIndex == normalized)
            {
                return;
            }

            _renderFactorIndex = normalized;
            var requestId = RuntimeLog.NewRequestId();
            RuntimeLog.Info(
                "avatar",
                "avatar.render.factor.changed",
                requestId,
                $"render factor switched to {GetRenderFactorPreset().DisplayName}",
                string.Empty,
                RenderSourceTier);

            EnsureCameraAndLight();
            ReloadModel();
        }

        private void BuildSimpleModelView()
        {
            var requestId = RuntimeLog.NewRequestId();
            var relativePath = _config != null ? _config.modelRelativePath : string.Empty;

            var (unityProjectRoot, repoRoot, unityPjRoot) = ResolveProjectRoots(requestId);
            if (string.IsNullOrWhiteSpace(unityProjectRoot))
            {
                RuntimeLog.Error(
                    "avatar",
                    "avatar.model.bootstrap_failed",
                    requestId,
                    "AVATAR.PATHS.ROOT_EMPTY",
                    "failed to resolve project roots",
                    relativePath,
                    ConfigSourceTier);
                CreateFallbackPrimitive(requestId, "failed to resolve project roots", relativePath, ConfigSourceTier, "AVATAR.PATHS.ROOT_EMPTY");
                return;
            }

            var resolver = new AssetPathResolver(
                new AssetPathResolverOptions
                {
                    CanonicalAssetsRoot = Path.Combine(unityPjRoot, "data", "assets_user"),
                    StreamingAssetsRoot = Application.streamingAssetsPath,
                    ForbidLegacyRoot = _config.forbidLegacyPath,
                    WarnOnNonAscii = _config.warnOnNonAsciiPath
                });

            var result = resolver.ResolveRelative(_config.modelRelativePath, requestId);
            if (!result.Success)
            {
                var resolveError = string.IsNullOrWhiteSpace(result.ErrorCode) ? "ASSET.PATH.RESOLVE_FAILED" : result.ErrorCode;
                var resolveSourceTier = SafeSourceTier(result.SourceTier, ConfigSourceTier);
                RuntimeLog.Error(
                    "avatar",
                    "avatar.model.resolve_failed",
                    requestId,
                    resolveError,
                    result.Message,
                    relativePath,
                    resolveSourceTier);
                CreateFallbackPrimitive(requestId, $"resolve failed: {resolveError}", relativePath, resolveSourceTier, resolveError);
                return;
            }

            if (!TryDisplayModel(result.ResolvedPath, requestId, result.SourceTier, out var loadErrorCode, out var loadMessage))
            {
                var fallbackErrorCode = loadErrorCode ?? FallbackErrorCode;
                var resolvedSourceTier = SafeSourceTier(result.SourceTier, "resolved_asset");
                RuntimeLog.Warn(
                    "avatar",
                    "avatar.model.fallback_used",
                    requestId,
                    fallbackErrorCode,
                    loadMessage ?? "model load failed, fallback primitive displayed",
                    result.ResolvedPath,
                    resolvedSourceTier);
                CreateFallbackPrimitive(
                    requestId,
                    $"fallback due to: {fallbackErrorCode}",
                    result.ResolvedPath,
                    resolvedSourceTier,
                    fallbackErrorCode);
            }
        }

        private static (string UnityProjectRoot, string RepoRoot, string UnityPjRoot) ResolveProjectRoots(string requestId, bool logResolvedInfo = true)
        {
            try
            {
                // Application.dataPath: <repo>/Unity_PJ/project/Assets
                var assetsDir = Path.GetFullPath(Application.dataPath);
                var projectRoot = Directory.GetParent(assetsDir)?.FullName;
                var unityPjRoot = Directory.GetParent(projectRoot ?? string.Empty)?.FullName;
                var repoRoot = Directory.GetParent(unityPjRoot ?? string.Empty)?.FullName;

                if (logResolvedInfo)
                {
                    RuntimeLog.Info(
                        "avatar",
                        "avatar.paths.resolved",
                        requestId,
                        "resolved runtime roots",
                        projectRoot,
                        "project");
                }

                return (projectRoot, repoRoot, unityPjRoot);
            }
            catch (Exception ex)
            {
                RuntimeLog.Error(
                    "avatar",
                    "avatar.paths.resolve_failed",
                    requestId,
                    "AVATAR.PATHS.RESOLVE_FAILED",
                    "failed to resolve runtime roots",
                    Application.dataPath,
                    "project",
                    ex);
                return (null, null, null);
            }
        }

        private bool TryDisplayModel(string absolutePath, string requestId, string sourceTier, out string errorCode, out string message)
        {
            var kind = ModelFormatRouter.Classify(absolutePath);
            RuntimeLog.Info(
                "avatar",
                "avatar.model.loader_selected",
                requestId,
                $"selected loader kind: {kind}",
                absolutePath,
                sourceTier);

            switch (kind)
            {
                case ModelAssetKind.Image:
                    return TryDisplayAsImagePlane(absolutePath, requestId, sourceTier, out errorCode, out message);
                case ModelAssetKind.Vrm:
                    return TryDisplayVrm(absolutePath, requestId, sourceTier, out errorCode, out message);
                case ModelAssetKind.Pmx:
                    return TryDisplayPmx(absolutePath, requestId, sourceTier, out errorCode, out message);
                default:
                    errorCode = ModelFormatRouter.UnsupportedExtensionErrorCode(absolutePath);
                    message = "unsupported extension for runtime model loader";
                    RuntimeLog.Warn(
                        "avatar",
                        "avatar.model.unsupported_extension",
                        requestId,
                        errorCode,
                        message,
                        absolutePath,
                        sourceTier);
                    return false;
            }
        }

        private bool TryDisplayAsImagePlane(string absolutePath, string requestId, string sourceTier, out string errorCode, out string message)
        {
            try
            {
                var loadResult = ReflectionModelLoaders.TryLoadImageTexture(absolutePath, requestId, sourceTier);
                if (!loadResult.Success || loadResult.Texture == null)
                {
                    var detail = loadResult.Error;
                    errorCode = detail?.ErrorCode ?? loadResult.ErrorCode ?? "ASSET.READ.DECODE_FAILED";
                    message = BuildLoaderFailureMessage(loadResult.Message ?? "failed to decode image bytes", detail);
                    RuntimeLog.Error(
                        "avatar",
                        "avatar.model.texture_decode_failed",
                        requestId,
                        errorCode,
                        message,
                        absolutePath,
                        sourceTier,
                        ex: null,
                        exceptionType: detail?.ExceptionType,
                        exceptionMessage: detail?.ExceptionMessage);
                    return false;
                }

                var tex = loadResult.Texture;
                var plane = GameObject.CreatePrimitive(PrimitiveType.Quad);
                plane.name = "SimpleModelImagePlane";
                plane.transform.position = new Vector3(0f, 0f, 3f);
                plane.transform.localScale = ComputeImagePlaneScale(tex.width, tex.height, _config.fallbackScale);

                var renderer = plane.GetComponent<Renderer>();
                var unlitShader = Shader.Find("Unlit/Texture") ?? Shader.Find("Standard");
                var material = new Material(unlitShader);
                material.mainTexture = tex;
                renderer.material = material;
                ApplyRenderFactorToModel(plane);
                RegisterActiveModelRoot(plane, enablePlacementStabilization: false);

                RuntimeLog.Info(
                    "avatar",
                    "avatar.model.displayed",
                    requestId,
                    "image model displayed",
                    absolutePath,
                    sourceTier);
                errorCode = null;
                message = null;
                return true;
            }
            catch (Exception ex)
            {
                errorCode = FallbackErrorCode;
                message = "failed to display model image";
                RuntimeLog.Error(
                    "avatar",
                    "avatar.model.display_failed",
                    requestId,
                    errorCode,
                    message,
                    absolutePath,
                    sourceTier,
                    ex);
                return false;
            }
        }

        public static Vector3 ComputeImagePlaneScale(int textureWidth, int textureHeight, Vector3 fallbackScale)
        {
            if (textureWidth <= 0 || textureHeight <= 0)
            {
                return fallbackScale;
            }

            var safeWidth = Mathf.Max(0.01f, Mathf.Abs(fallbackScale.x));
            var safeHeight = Mathf.Max(0.01f, Mathf.Abs(fallbackScale.y));
            var textureAspect = (float)textureWidth / textureHeight;
            var fallbackAspect = safeWidth / safeHeight;

            var scale = fallbackScale;
            if (textureAspect >= fallbackAspect)
            {
                scale.x = safeWidth;
                scale.y = safeWidth / textureAspect;
            }
            else
            {
                scale.x = safeHeight * textureAspect;
                scale.y = safeHeight;
            }

            return scale;
        }

        private bool TryDisplayVrm(string absolutePath, string requestId, string sourceTier, out string errorCode, out string message)
        {
            var result = ReflectionModelLoaders.TryLoadVrm(absolutePath, requestId, sourceTier);
            if (!result.Success || result.Root == null)
            {
                var detail = result.Error;
                errorCode = detail?.ErrorCode ?? result.ErrorCode ?? "AVATAR.VRM.LOAD_FAILED";
                message = BuildLoaderFailureMessage(result.Message ?? "vrm load failed", detail);
                RuntimeLog.Error(
                    "avatar",
                    "avatar.model.vrm_load_failed",
                    requestId,
                    errorCode,
                    message,
                    absolutePath,
                    sourceTier,
                    ex: null,
                    exceptionType: detail?.ExceptionType,
                    exceptionMessage: detail?.ExceptionMessage);
                return false;
            }

            NormalizeLoadedModelRenderState(result.Root);
            NormalizeLoadedModelTransform(result.Root);
            ApplyRenderFactorToModel(result.Root);
            LogModelRenderDiagnostics(result.Root, requestId, absolutePath, sourceTier, "vrm", GetRenderFactorPreset().DisplayName);
            RegisterActiveModelRoot(result.Root, enablePlacementStabilization: true);
            RuntimeLog.Info(
                "avatar",
                "avatar.model.displayed",
                requestId,
                "vrm model displayed",
                absolutePath,
                sourceTier);
            errorCode = null;
            message = null;
            return true;
        }

        private bool TryDisplayPmx(string absolutePath, string requestId, string sourceTier, out string errorCode, out string message)
        {
            LogModelPathStructureDiagnostics(absolutePath, requestId, sourceTier);
            var result = ReflectionModelLoaders.TryLoadPmx(absolutePath, requestId, sourceTier);
            if (!result.Success || result.Root == null)
            {
                var detail = result.Error;
                errorCode = detail?.ErrorCode ?? result.ErrorCode ?? "AVATAR.PMX.LOAD_FAILED";
                message = BuildLoaderFailureMessage(result.Message ?? "pmx load failed", detail);
                RuntimeLog.Error(
                    "avatar",
                    "avatar.model.pmx_load_failed",
                    requestId,
                    errorCode,
                    message,
                    absolutePath,
                    sourceTier,
                    ex: null,
                    exceptionType: detail?.ExceptionType,
                    exceptionMessage: detail?.ExceptionMessage);
                return false;
            }

            NormalizeLoadedModelRenderState(result.Root);
            NormalizeLoadedModelTransform(result.Root);
            ApplyRenderFactorToModel(result.Root);
            LogModelRenderDiagnostics(result.Root, requestId, absolutePath, sourceTier, "pmx", GetRenderFactorPreset().DisplayName);
            RegisterActiveModelRoot(result.Root, enablePlacementStabilization: true);
            RuntimeLog.Info(
                "avatar",
                "avatar.model.displayed",
                requestId,
                "pmx model displayed",
                absolutePath,
                sourceTier);
            errorCode = null;
            message = null;
            return true;
        }

        private static string BuildLoaderFailureMessage(string fallbackMessage, RuntimeErrorDetail detail)
        {
            if (detail == null)
            {
                return fallbackMessage ?? string.Empty;
            }

            var baseMessage = string.IsNullOrWhiteSpace(detail.Message)
                ? (fallbackMessage ?? string.Empty)
                : detail.Message;
            var stage = string.IsNullOrWhiteSpace(detail.Stage) ? "unknown" : detail.Stage;
            var cause = string.IsNullOrWhiteSpace(detail.Cause) ? "unknown" : detail.Cause;
            if (string.IsNullOrWhiteSpace(detail.ExceptionType) && string.IsNullOrWhiteSpace(detail.ExceptionMessage))
            {
                return $"{baseMessage} (stage={stage}, cause={cause})";
            }

            return $"{baseMessage} (stage={stage}, cause={cause}, exception={detail.ExceptionType}: {detail.ExceptionMessage})";
        }

        private static void NormalizeLoadedModelRenderState(GameObject modelRoot)
        {
            if (modelRoot == null)
            {
                return;
            }

            var renderers = modelRoot.GetComponentsInChildren<Renderer>(true);
            foreach (var renderer in renderers)
            {
                if (renderer == null)
                {
                    continue;
                }

                if (!renderer.gameObject.activeSelf)
                {
                    renderer.gameObject.SetActive(true);
                }

                renderer.forceRenderingOff = false;
                renderer.enabled = true;

                if (renderer is SkinnedMeshRenderer skinnedRenderer)
                {
                    skinnedRenderer.updateWhenOffscreen = true;
                    EnsureSkinnedRendererBounds(skinnedRenderer);
                }

                EnsureVisibleMaterialSettings(renderer);
            }
        }

        private void NormalizeLoadedModelTransform(GameObject modelRoot)
        {
            modelRoot.transform.position = Vector3.zero;
            modelRoot.transform.rotation = Quaternion.identity;
            modelRoot.transform.localScale = Vector3.one;

            if (!TryGetModelBounds(modelRoot, out var bounds))
            {
                modelRoot.transform.position = new Vector3(0f, 0f, ModelTargetZ);
                return;
            }

            var scaleFactor = ComputeModelScaleFactor(bounds.size.y, ModelTargetHeight, MinModelScale, MaxModelScale);
            modelRoot.transform.localScale = Vector3.one * scaleFactor;

            if (!TryGetModelBounds(modelRoot, out bounds))
            {
                modelRoot.transform.position = new Vector3(0f, 0f, ModelTargetZ);
                return;
            }

            CenterModelAtTargetDepth(modelRoot, bounds, ModelTargetZ);

            if (!TryGetModelBounds(modelRoot, out bounds))
            {
                return;
            }

            FitModelToCamera(modelRoot, bounds);
        }

        public static float ComputeModelScaleFactor(float currentHeight, float targetHeight, float minScale, float maxScale)
        {
            var safeHeight = Mathf.Max(0.01f, currentHeight);
            var safeTarget = Mathf.Max(0.01f, targetHeight);
            var unclamped = safeTarget / safeHeight;
            var safeMin = Mathf.Max(0.001f, minScale);
            var safeMax = Mathf.Max(safeMin, maxScale);
            return Mathf.Clamp(unclamped, safeMin, safeMax);
        }

        public static float ComputeCameraFitScale(float modelHalfWidth, float modelHalfHeight, float cameraDistance, float cameraFieldOfView, float cameraAspect, float padding)
        {
            var safeDistance = Mathf.Max(0.01f, cameraDistance);
            var safeHalfWidth = Mathf.Max(0.001f, modelHalfWidth);
            var safeHalfHeight = Mathf.Max(0.001f, modelHalfHeight);
            var safeAspect = Mathf.Max(0.1f, cameraAspect);
            var safePadding = Mathf.Clamp(padding, 0.1f, 1f);

            var halfVertical = Mathf.Tan(cameraFieldOfView * 0.5f * Mathf.Deg2Rad) * safeDistance * safePadding;
            var halfHorizontal = halfVertical * safeAspect;
            if (halfVertical <= 0f || halfHorizontal <= 0f)
            {
                return 1f;
            }

            var heightScale = halfVertical / safeHalfHeight;
            var widthScale = halfHorizontal / safeHalfWidth;
            var fitScale = Mathf.Min(heightScale, widthScale);
            return Mathf.Clamp(fitScale, MinModelScale, 1f);
        }

        private static void CenterModelAtTargetDepth(GameObject modelRoot, Bounds bounds, float targetDepth)
        {
            modelRoot.transform.position = new Vector3(-bounds.center.x, -bounds.center.y, targetDepth - bounds.center.z);
        }

        private void FitModelToCamera(GameObject modelRoot, Bounds bounds)
        {
            var camera = Camera.main;
            if (camera == null)
            {
                camera = FindFirstObjectByType<Camera>();
            }

            if (camera == null)
            {
                return;
            }

            var forward = camera.transform.forward;
            var toCenter = bounds.center - camera.transform.position;
            var distance = Vector3.Dot(toCenter, forward);
            var minDistance = camera.nearClipPlane + 0.5f;
            if (distance < minDistance)
            {
                var pushForward = minDistance - distance;
                modelRoot.transform.position += forward * pushForward;
                if (!TryGetModelBounds(modelRoot, out bounds))
                {
                    return;
                }
                toCenter = bounds.center - camera.transform.position;
                distance = Vector3.Dot(toCenter, forward);
            }

            var fitScale = ComputeCameraFitScale(
                bounds.extents.x,
                bounds.extents.y,
                distance,
                camera.fieldOfView,
                camera.aspect,
                CameraFitPadding);
            if (fitScale >= 0.999f)
            {
                return;
            }

            modelRoot.transform.localScale *= fitScale;
            if (!TryGetModelBounds(modelRoot, out bounds))
            {
                return;
            }

            CenterModelAtTargetDepth(modelRoot, bounds, ModelTargetZ);
        }

        private static bool TryGetModelBounds(GameObject modelRoot, out Bounds bounds)
        {
            var renderers = modelRoot.GetComponentsInChildren<Renderer>(true);
            if (TryGetValidBounds(renderers, includeDisabledRenderers: false, out bounds))
            {
                return true;
            }

            if (TryGetValidBounds(renderers, includeDisabledRenderers: true, out bounds))
            {
                return true;
            }

            bounds = new Bounds();
            return false;
        }

        private static bool TryGetValidBounds(Renderer[] renderers, bool includeDisabledRenderers, out Bounds bounds)
        {
            bounds = default;
            var hasBounds = false;
            foreach (var renderer in renderers)
            {
                if (renderer == null)
                {
                    continue;
                }

                if (!includeDisabledRenderers && !renderer.enabled)
                {
                    continue;
                }

                var rendererBounds = renderer.bounds;
                if (!IsFiniteBounds(rendererBounds))
                {
                    continue;
                }

                if (!hasBounds)
                {
                    bounds = rendererBounds;
                    hasBounds = true;
                }
                else
                {
                    bounds.Encapsulate(rendererBounds);
                }
            }

            return hasBounds;
        }

        private static bool IsFiniteBounds(Bounds bounds)
        {
            return IsFiniteVector(bounds.center) &&
                   IsFiniteVector(bounds.extents) &&
                   bounds.size.x > 0.0001f &&
                   bounds.size.y > 0.0001f &&
                   bounds.size.z > 0.0001f;
        }

        private static bool IsFiniteVector(Vector3 value)
        {
            return !float.IsNaN(value.x) &&
                   !float.IsNaN(value.y) &&
                   !float.IsNaN(value.z) &&
                   !float.IsInfinity(value.x) &&
                   !float.IsInfinity(value.y) &&
                   !float.IsInfinity(value.z);
        }

        private static void EnsureSkinnedRendererBounds(SkinnedMeshRenderer renderer)
        {
            if (renderer == null)
            {
                return;
            }

            var bounds = renderer.localBounds;
            if (!IsFiniteBounds(bounds) || !HasValidBoundsSize(bounds, MinSkinnedBoundsSize))
            {
                bounds = renderer.sharedMesh != null ? renderer.sharedMesh.bounds : default;
            }

            if (!IsFiniteBounds(bounds) || !HasValidBoundsSize(bounds, MinSkinnedBoundsSize))
            {
                bounds = new Bounds(Vector3.zero, Vector3.one * FallbackSkinnedBoundsSize);
            }
            else
            {
                bounds.Expand(Mathf.Max(MinSkinnedBoundsSize, bounds.extents.magnitude * 0.15f));
            }

            renderer.localBounds = bounds;
        }

        private static bool HasValidBoundsSize(Bounds bounds, float minSize)
        {
            return bounds.size.x >= minSize &&
                   bounds.size.y >= minSize &&
                   bounds.size.z >= minSize;
        }

        private static void EnsureVisibleMaterialSettings(Renderer renderer)
        {
            if (renderer == null)
            {
                return;
            }

            var materials = renderer.materials;
            for (var i = 0; i < materials.Length; i++)
            {
                var material = materials[i];
                if (material == null)
                {
                    continue;
                }

                if (material.HasProperty("_Color"))
                {
                    var color = material.color;
                    if (color.a <= MinVisibleMaterialAlpha)
                    {
                        color.a = 1f;
                        material.color = color;
                    }
                }

                if (material.HasProperty("_Opacity"))
                {
                    var opacity = material.GetFloat("_Opacity");
                    if (opacity <= MinVisibleMaterialAlpha)
                    {
                        material.SetFloat("_Opacity", 1f);
                    }
                }
            }
        }

        private static void LogModelPathStructureDiagnostics(string absolutePath, string requestId, string sourceTier)
        {
            if (string.IsNullOrWhiteSpace(absolutePath))
            {
                return;
            }

            var modelDirectory = Path.GetDirectoryName(absolutePath);
            if (string.IsNullOrWhiteSpace(modelDirectory))
            {
                return;
            }

            var textureDirectory = Path.Combine(modelDirectory, "texture");
            var texturesDirectory = Path.Combine(modelDirectory, "textures");
            var textureDirectoryExists = Directory.Exists(textureDirectory);
            var texturesDirectoryExists = Directory.Exists(texturesDirectory);

            var topLevelTextureFiles = 0;
            var nestedTextureFiles = 0;
            try
            {
                foreach (var filePath in Directory.EnumerateFiles(modelDirectory, "*.*", SearchOption.TopDirectoryOnly))
                {
                    if (IsTextureFilePath(filePath))
                    {
                        topLevelTextureFiles++;
                    }
                }
            }
            catch (Exception ex)
            {
                topLevelTextureFiles = -1;
                RuntimeLog.Warn(
                    "avatar",
                    "avatar.model.path_structure_scan_failed",
                    requestId,
                    "AVATAR.MODEL.PATH_SCAN_TOPLEVEL_FAILED",
                    "failed to enumerate top-level texture files",
                    absolutePath,
                    sourceTier,
                    ex);
            }

            try
            {
                foreach (var filePath in Directory.EnumerateFiles(modelDirectory, "*.*", SearchOption.AllDirectories))
                {
                    if (IsTextureFilePath(filePath))
                    {
                        nestedTextureFiles++;
                    }
                }
            }
            catch (Exception ex)
            {
                nestedTextureFiles = -1;
                RuntimeLog.Warn(
                    "avatar",
                    "avatar.model.path_structure_scan_failed",
                    requestId,
                    "AVATAR.MODEL.PATH_SCAN_NESTED_FAILED",
                    "failed to enumerate nested texture files",
                    absolutePath,
                    sourceTier,
                    ex);
            }

            RuntimeLog.Info(
                "avatar",
                "avatar.model.path_structure",
                requestId,
                $"modelDir={modelDirectory}, textureDir={textureDirectoryExists}, texturesDir={texturesDirectoryExists}, textureFilesTop={topLevelTextureFiles}, textureFilesNested={nestedTextureFiles}",
                absolutePath,
                sourceTier);
        }

        private static bool IsTextureFilePath(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                return false;
            }

            var extension = Path.GetExtension(filePath);
            return !string.IsNullOrWhiteSpace(extension) && TextureDiagnosticExtensions.Contains(extension);
        }

        private static void LogModelRenderDiagnostics(
            GameObject modelRoot,
            string requestId,
            string absolutePath,
            string sourceTier,
            string assetKind,
            string renderFactorName)
        {
            if (modelRoot == null)
            {
                return;
            }

            var renderers = modelRoot.GetComponentsInChildren<Renderer>(true);
            var rendererCount = 0;
            var enabledRendererCount = 0;
            var activeRendererCount = 0;
            var skinnedRendererCount = 0;
            var missingMainTextureMaterials = 0;
            var missingMainTextureSpecMaterials = 0;
            var missingMainTextureResolveMaterials = 0;
            var missingMainTextureUnknownMaterials = 0;
            var missingMainTextureSamples = new List<string>();
            var missingMainTextureSpecSamples = new List<string>();
            var lowAlphaMaterials = 0;
            var materialCount = 0;
            var missingToonTextureMaterials = 0;
            var missingToonTextureSpecMaterials = 0;
            var missingToonTextureResolveMaterials = 0;
            var missingToonTextureUnknownMaterials = 0;
            var missingSphereAddTextureMaterials = 0;
            var missingSphereAddTextureSpecMaterials = 0;
            var missingSphereAddTextureResolveMaterials = 0;
            var missingSphereAddTextureUnknownMaterials = 0;
            var sphereAddTextureNotUsedMaterials = 0;
            var missingSphereMulTextureMaterials = 0;
            var missingSphereMulTextureSpecMaterials = 0;
            var missingSphereMulTextureResolveMaterials = 0;
            var missingSphereMulTextureUnknownMaterials = 0;
            var sphereMulTextureNotUsedMaterials = 0;
            var highShininessMaterials = 0;
            var highSpecularColorMaterials = 0;
            var brightDiffuseColorMaterials = 0;
            var transparentShaderMaterials = 0;
            var transparentByDiffuseAlphaMaterials = 0;
            var transparentByEdgeAlphaMaterials = 0;
            var transparentByTextureAlphaMaterials = 0;
            var transparentByMultiReasonMaterials = 0;
            var transparentReasonUnknownMaterials = 0;
            var materialDiagnosticSamples = new List<string>();
            var safeAssetKind = string.IsNullOrWhiteSpace(assetKind) ? "unknown" : assetKind;
            var safeRenderFactorName = string.IsNullOrWhiteSpace(renderFactorName) ? "unknown" : renderFactorName;

            foreach (var renderer in renderers)
            {
                if (renderer == null)
                {
                    continue;
                }

                rendererCount++;
                if (renderer.enabled)
                {
                    enabledRendererCount++;
                }

                if (renderer.gameObject.activeInHierarchy)
                {
                    activeRendererCount++;
                }

                if (renderer is SkinnedMeshRenderer)
                {
                    skinnedRendererCount++;
                }

                if (ShouldSkipRendererForMissingMainTextureDiagnostics(renderer, modelRoot))
                {
                    continue;
                }

                var materials = renderer.materials;
                for (var i = 0; i < materials.Length; i++)
                {
                    var material = materials[i];
                    if (material == null)
                    {
                        continue;
                    }

                    var shaderName = material.shader != null ? material.shader.name : string.Empty;
                    var isTransparentShader = !string.IsNullOrEmpty(shaderName) &&
                                              shaderName.IndexOf("MMD/Transparent/", StringComparison.OrdinalIgnoreCase) >= 0;
                    if (isTransparentShader)
                    {
                        transparentShaderMaterials++;
                    }

                    materialCount++;
                    var textureStatus = material.GetTag(MainTextureStatusTag, false, string.Empty);
                    var transparentReason = material.GetTag(TransparentReasonTag, false, string.Empty);
                    var toonTextureStatus = material.GetTag(ToonTextureStatusTag, false, string.Empty);
                    var sphereAddTextureStatus = material.GetTag(SphereAddTextureStatusTag, false, string.Empty);
                    var sphereMulTextureStatus = material.GetTag(SphereMulTextureStatusTag, false, string.Empty);
                    var displayMaterialName = GetMaterialDiagnosticName(material);
                    if (ShouldInspectMainTextureMaterial(material, textureStatus) && material.mainTexture == null)
                    {
                        missingMainTextureMaterials++;
                        if (string.Equals(textureStatus, MainTextureStatusMissingSpec, StringComparison.OrdinalIgnoreCase))
                        {
                            missingMainTextureSpecMaterials++;
                            if (missingMainTextureSpecSamples.Count < 12)
                            {
                                var shaderLabel = string.IsNullOrWhiteSpace(shaderName) ? "none" : shaderName;
                                var reasonLabel = string.IsNullOrWhiteSpace(transparentReason) ? TransparentReasonOpaque : transparentReason;
                                missingMainTextureSpecSamples.Add($"{displayMaterialName}(shader={shaderLabel}, reason={reasonLabel})");
                            }
                        }
                        else if (string.Equals(textureStatus, MainTextureStatusMissingResolve, StringComparison.OrdinalIgnoreCase))
                        {
                            missingMainTextureResolveMaterials++;
                        }
                        else
                        {
                            missingMainTextureUnknownMaterials++;
                            if (string.IsNullOrWhiteSpace(textureStatus))
                            {
                                textureStatus = "unknown";
                            }
                        }

                        if (missingMainTextureSamples.Count < 8)
                        {
                            missingMainTextureSamples.Add($"{displayMaterialName}:{textureStatus}");
                        }
                    }

                    if (material.HasProperty("_Color") && material.color.a <= MinVisibleMaterialAlpha)
                    {
                        lowAlphaMaterials++;
                    }

                    if (material.HasProperty("_Opacity") && material.GetFloat("_Opacity") <= MinVisibleMaterialAlpha)
                    {
                        lowAlphaMaterials++;
                    }

                    var toonMissing = material.HasProperty("_ToonTex") && material.GetTexture("_ToonTex") == null;
                    if (toonMissing)
                    {
                        missingToonTextureMaterials++;
                        CountMissingTextureStatus(
                            toonTextureStatus,
                            ref missingToonTextureSpecMaterials,
                            ref missingToonTextureResolveMaterials,
                            ref missingToonTextureUnknownMaterials);
                    }

                    var sphereAddNotUsed = IsTextureStatus(sphereAddTextureStatus, TextureStatusNotUsed);
                    if (sphereAddNotUsed)
                    {
                        sphereAddTextureNotUsedMaterials++;
                    }

                    var sphereAddMissing = material.HasProperty("_SphereAddTex") &&
                                           material.GetTexture("_SphereAddTex") == null &&
                                           !sphereAddNotUsed;
                    if (sphereAddMissing)
                    {
                        missingSphereAddTextureMaterials++;
                        CountMissingTextureStatus(
                            sphereAddTextureStatus,
                            ref missingSphereAddTextureSpecMaterials,
                            ref missingSphereAddTextureResolveMaterials,
                            ref missingSphereAddTextureUnknownMaterials);
                    }

                    var sphereMulNotUsed = IsTextureStatus(sphereMulTextureStatus, TextureStatusNotUsed);
                    if (sphereMulNotUsed)
                    {
                        sphereMulTextureNotUsedMaterials++;
                    }

                    var sphereMulMissing = material.HasProperty("_SphereMulTex") &&
                                           material.GetTexture("_SphereMulTex") == null &&
                                           !sphereMulNotUsed;
                    if (sphereMulMissing)
                    {
                        missingSphereMulTextureMaterials++;
                        CountMissingTextureStatus(
                            sphereMulTextureStatus,
                            ref missingSphereMulTextureSpecMaterials,
                            ref missingSphereMulTextureResolveMaterials,
                            ref missingSphereMulTextureUnknownMaterials);
                    }

                    var highShininess = material.HasProperty("_Shininess") && material.GetFloat("_Shininess") >= HighShininessThreshold;
                    if (highShininess)
                    {
                        highShininessMaterials++;
                    }

                    var highSpecular = false;
                    if (material.HasProperty("_SpecularColor"))
                    {
                        var specularColor = material.GetColor("_SpecularColor");
                        highSpecular = MaxColorChannel(specularColor) >= HighSpecularColorThreshold;
                        if (highSpecular)
                        {
                            highSpecularColorMaterials++;
                        }
                    }

                    var brightDiffuse = false;
                    if (material.HasProperty("_Color"))
                    {
                        var diffuseColor = material.GetColor("_Color");
                        brightDiffuse = MaxColorChannel(diffuseColor) >= BrightDiffuseColorThreshold;
                        if (brightDiffuse)
                        {
                            brightDiffuseColorMaterials++;
                        }
                    }

                    if (isTransparentShader)
                    {
                        var hasDiffuseAlphaReason = HasTransparentReason(transparentReason, TransparentReasonDiffuseAlpha);
                        if (hasDiffuseAlphaReason)
                        {
                            transparentByDiffuseAlphaMaterials++;
                        }

                        var hasEdgeAlphaReason = HasTransparentReason(transparentReason, TransparentReasonEdgeAlpha);
                        if (hasEdgeAlphaReason)
                        {
                            transparentByEdgeAlphaMaterials++;
                        }

                        var hasTextureAlphaReason = HasTransparentReason(transparentReason, TransparentReasonTextureAlpha);
                        if (hasTextureAlphaReason)
                        {
                            transparentByTextureAlphaMaterials++;
                        }

                        var reasonCount = 0;
                        if (hasDiffuseAlphaReason)
                        {
                            reasonCount++;
                        }

                        if (hasEdgeAlphaReason)
                        {
                            reasonCount++;
                        }

                        if (hasTextureAlphaReason)
                        {
                            reasonCount++;
                        }

                        if (reasonCount >= 2)
                        {
                            transparentByMultiReasonMaterials++;
                        }

                        if (reasonCount == 0)
                        {
                            transparentReasonUnknownMaterials++;
                        }
                    }

                    if (materialDiagnosticSamples.Count < MaterialDiagnosticSamplesLimit &&
                        (toonMissing || sphereAddMissing || sphereMulMissing || highShininess || highSpecular || brightDiffuse || isTransparentShader))
                    {
                        materialDiagnosticSamples.Add(BuildMaterialDiagnosticSample(
                            material,
                            textureStatus,
                            transparentReason,
                            toonTextureStatus,
                            sphereAddTextureStatus,
                            sphereMulTextureStatus,
                            toonMissing,
                            sphereAddMissing,
                            sphereMulMissing));
                    }
                }
            }

            var hasBounds = TryGetModelBounds(modelRoot, out var bounds);
            var boundsText = hasBounds
                ? $"{bounds.size.x:0.###}x{bounds.size.y:0.###}x{bounds.size.z:0.###}"
                : "none";

            RuntimeLog.Info(
                "avatar",
                "avatar.model.render_diagnostics",
                requestId,
                $"renderers={rendererCount}, enabled={enabledRendererCount}, active={activeRendererCount}, skinned={skinnedRendererCount}, missingMainTexMats={missingMainTextureMaterials}, missingMainTexSpecMats={missingMainTextureSpecMaterials}, missingMainTexResolveMats={missingMainTextureResolveMaterials}, missingMainTexUnknownMats={missingMainTextureUnknownMaterials}, lowAlphaMats={lowAlphaMaterials}, bounds={boundsText}",
                absolutePath,
                sourceTier);

            var materialSampleText = materialDiagnosticSamples.Count == 0
                ? "none"
                : string.Join(" | ", materialDiagnosticSamples);
            RuntimeLog.Info(
                "avatar",
                "avatar.model.material_diagnostics",
                requestId,
                $"assetKind={safeAssetKind}, renderFactor={safeRenderFactorName}, materials={materialCount}, transparentMats={transparentShaderMaterials}, transparentByDiffuseAlphaMats={transparentByDiffuseAlphaMaterials}, transparentByEdgeAlphaMats={transparentByEdgeAlphaMaterials}, transparentByTextureAlphaMats={transparentByTextureAlphaMaterials}, transparentByMultiReasonMats={transparentByMultiReasonMaterials}, transparentReasonUnknownMats={transparentReasonUnknownMaterials}, toonMissingMats={missingToonTextureMaterials}, toonMissingSpecMats={missingToonTextureSpecMaterials}, toonMissingResolveMats={missingToonTextureResolveMaterials}, toonMissingUnknownMats={missingToonTextureUnknownMaterials}, sphereAddMissingMats={missingSphereAddTextureMaterials}, sphereAddMissingSpecMats={missingSphereAddTextureSpecMaterials}, sphereAddMissingResolveMats={missingSphereAddTextureResolveMaterials}, sphereAddMissingUnknownMats={missingSphereAddTextureUnknownMaterials}, sphereAddNotUsedMats={sphereAddTextureNotUsedMaterials}, sphereMulMissingMats={missingSphereMulTextureMaterials}, sphereMulMissingSpecMats={missingSphereMulTextureSpecMaterials}, sphereMulMissingResolveMats={missingSphereMulTextureResolveMaterials}, sphereMulMissingUnknownMats={missingSphereMulTextureUnknownMaterials}, sphereMulNotUsedMats={sphereMulTextureNotUsedMaterials}, highShininessMats={highShininessMaterials}, highSpecularMats={highSpecularColorMaterials}, brightDiffuseMats={brightDiffuseColorMaterials}, colorSpace={QualitySettings.activeColorSpace}, samples={materialSampleText}",
                absolutePath,
                sourceTier);

            var transparentRatio = ComputeRatio(transparentShaderMaterials, materialCount);
            var textureAlphaShare = ComputeRatio(transparentByTextureAlphaMaterials, transparentShaderMaterials);
            var edgeAlphaShare = ComputeRatio(transparentByEdgeAlphaMaterials, transparentShaderMaterials);
            var highShininessRatio = ComputeRatio(highShininessMaterials, materialCount);
            var brightDiffuseRatio = ComputeRatio(brightDiffuseColorMaterials, materialCount);
            var missingResolveTotal = missingMainTextureResolveMaterials +
                                      missingToonTextureResolveMaterials +
                                      missingSphereAddTextureResolveMaterials +
                                      missingSphereMulTextureResolveMaterials;
            var remediationHint = BuildRemediationHint(
                materialCount,
                transparentShaderMaterials,
                transparentByTextureAlphaMaterials,
                transparentByEdgeAlphaMaterials,
                highShininessMaterials,
                brightDiffuseColorMaterials,
                missingResolveTotal);
            RuntimeLog.Info(
                "avatar",
                "avatar.model.remediation_hint",
                requestId,
                $"assetKind={safeAssetKind}, renderFactor={safeRenderFactorName}, hint={remediationHint}, transparentRatio={transparentRatio:0.###}, textureAlphaShare={textureAlphaShare:0.###}, edgeAlphaShare={edgeAlphaShare:0.###}, highShininessRatio={highShininessRatio:0.###}, brightDiffuseRatio={brightDiffuseRatio:0.###}, missingResolveTotal={missingResolveTotal}",
                absolutePath,
                sourceTier);

            if (missingMainTextureMaterials > 0)
            {
                var sampleText = missingMainTextureSamples.Count == 0
                    ? "none"
                    : string.Join(" | ", missingMainTextureSamples);

                RuntimeLog.Info(
                    "avatar",
                    "avatar.model.missing_main_textures",
                    requestId,
                    $"missingMainTexMats={missingMainTextureMaterials}, missingMainTexSpecMats={missingMainTextureSpecMaterials}, missingMainTexResolveMats={missingMainTextureResolveMaterials}, missingMainTexUnknownMats={missingMainTextureUnknownMaterials}, samples={sampleText}",
                    absolutePath,
                    sourceTier);
            }

            if (missingMainTextureSpecMaterials > 0)
            {
                var sampleText = missingMainTextureSpecSamples.Count == 0
                    ? "none"
                    : string.Join(" | ", missingMainTextureSpecSamples);

                RuntimeLog.Info(
                    "avatar",
                    "avatar.model.missing_spec_materials",
                    requestId,
                    $"missingMainTexSpecMats={missingMainTextureSpecMaterials}, samples={sampleText}",
                    absolutePath,
                    sourceTier);
            }

            if (enabledRendererCount == 0 || !hasBounds)
            {
                RuntimeLog.Warn(
                    "avatar",
                    "avatar.model.render_suspect",
                    requestId,
                    "AVATAR.RENDER.SUSPECT_STATE",
                    $"possible invisible model state: enabledRenderers={enabledRendererCount}, hasBounds={hasBounds}",
                    absolutePath,
                    sourceTier);
            }
        }

        private static bool ShouldSkipRendererForMissingMainTextureDiagnostics(Renderer renderer, GameObject modelRoot)
        {
            if (renderer == null || modelRoot == null || renderer.transform != modelRoot.transform)
            {
                return false;
            }

            var skinnedRenderer = renderer as SkinnedMeshRenderer;
            if (skinnedRenderer == null)
            {
                return false;
            }

            var sharedMesh = skinnedRenderer.sharedMesh;
            return sharedMesh == null || sharedMesh.vertexCount <= 0;
        }

        private static bool ShouldInspectMainTextureMaterial(Material material, string textureStatus)
        {
            if (material == null)
            {
                return false;
            }

            if (material.HasProperty("_MainTex"))
            {
                return true;
            }

            return !string.IsNullOrWhiteSpace(textureStatus);
        }

        private static string GetMaterialDiagnosticName(Material material)
        {
            if (material == null)
            {
                return "null_material";
            }

            var taggedName = material.GetTag(MmdMaterialNameTag, false, string.Empty);
            if (!string.IsNullOrWhiteSpace(taggedName))
            {
                return taggedName;
            }

            return string.IsNullOrWhiteSpace(material.name) ? "unnamed_material" : material.name;
        }

        private static float MaxColorChannel(Color color)
        {
            return Mathf.Max(color.r, Mathf.Max(color.g, color.b));
        }

        private static float ComputeRatio(int numerator, int denominator)
        {
            if (denominator <= 0 || numerator <= 0)
            {
                return 0f;
            }

            return numerator / (float)denominator;
        }

        private static string BuildRemediationHint(
            int materialCount,
            int transparentShaderMaterials,
            int transparentByTextureAlphaMaterials,
            int transparentByEdgeAlphaMaterials,
            int highShininessMaterials,
            int brightDiffuseMaterials,
            int missingResolveTotal)
        {
            if (materialCount <= 0)
            {
                return "insufficient_data";
            }

            if (missingResolveTotal > 0)
            {
                return "asset_resolution_first";
            }

            var textureAlphaShare = ComputeRatio(transparentByTextureAlphaMaterials, transparentShaderMaterials);
            var edgeAlphaShare = ComputeRatio(transparentByEdgeAlphaMaterials, transparentShaderMaterials);
            var highShininessRatio = ComputeRatio(highShininessMaterials, materialCount);
            var brightDiffuseRatio = ComputeRatio(brightDiffuseMaterials, materialCount);

            if (textureAlphaShare >= 0.5f && edgeAlphaShare < 0.5f)
            {
                return "materialloader_threshold_candidate";
            }

            if (edgeAlphaShare >= 0.5f || highShininessRatio >= 0.4f || brightDiffuseRatio >= 0.7f)
            {
                return "shader_lighting_candidate";
            }

            return "mixed_followup";
        }

        private static bool HasTransparentReason(string transparentReasonTag, string expectedReason)
        {
            if (string.IsNullOrWhiteSpace(transparentReasonTag) || string.IsNullOrWhiteSpace(expectedReason))
            {
                return false;
            }

            var parts = transparentReasonTag.Split('+');
            for (var i = 0; i < parts.Length; i++)
            {
                if (string.Equals(parts[i].Trim(), expectedReason, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsTextureStatus(string textureStatus, string expectedStatus)
        {
            return string.Equals(textureStatus, expectedStatus, StringComparison.OrdinalIgnoreCase);
        }

        private static void CountMissingTextureStatus(
            string textureStatus,
            ref int missingSpecCount,
            ref int missingResolveCount,
            ref int missingUnknownCount)
        {
            if (IsTextureStatus(textureStatus, MainTextureStatusMissingSpec))
            {
                missingSpecCount++;
            }
            else if (IsTextureStatus(textureStatus, MainTextureStatusMissingResolve))
            {
                missingResolveCount++;
            }
            else
            {
                missingUnknownCount++;
            }
        }

        private static string BuildMaterialDiagnosticSample(
            Material material,
            string textureStatus,
            string transparentReason,
            string toonTextureStatus,
            string sphereAddTextureStatus,
            string sphereMulTextureStatus,
            bool toonMissing,
            bool sphereAddMissing,
            bool sphereMulMissing)
        {
            var shininess = material.HasProperty("_Shininess") ? material.GetFloat("_Shininess") : -1f;
            var specularMax = material.HasProperty("_SpecularColor")
                ? MaxColorChannel(material.GetColor("_SpecularColor"))
                : -1f;
            var diffuseMax = material.HasProperty("_Color")
                ? MaxColorChannel(material.GetColor("_Color"))
                : -1f;

            var safeTransparentReason = string.IsNullOrWhiteSpace(transparentReason) ? TransparentReasonOpaque : transparentReason;
            var safeToonStatus = string.IsNullOrWhiteSpace(toonTextureStatus) ? "none" : toonTextureStatus;
            var safeSphereAddStatus = string.IsNullOrWhiteSpace(sphereAddTextureStatus) ? "none" : sphereAddTextureStatus;
            var safeSphereMulStatus = string.IsNullOrWhiteSpace(sphereMulTextureStatus) ? "none" : sphereMulTextureStatus;
            var shaderName = material.shader != null ? material.shader.name : "none";
            var materialName = GetMaterialDiagnosticName(material);
            return $"{materialName}(shader={shaderName}, status={(string.IsNullOrWhiteSpace(textureStatus) ? "none" : textureStatus)}, transparentReason={safeTransparentReason}, mainTex={(material.mainTexture != null)}, toonMissing={toonMissing}, toonStatus={safeToonStatus}, sphereAddMissing={sphereAddMissing}, sphereAddStatus={safeSphereAddStatus}, sphereMulMissing={sphereMulMissing}, sphereMulStatus={safeSphereMulStatus}, shininess={shininess:0.##}, specMax={specularMax:0.##}, diffMax={diffuseMax:0.##})";
        }

        private void ApplyRenderFactorToModel(GameObject modelRoot)
        {
            if (modelRoot == null)
            {
                return;
            }

            var preset = GetRenderFactorPreset();
            if (Mathf.Approximately(preset.AlbedoMultiplier, 1f))
            {
                return;
            }

            var renderers = modelRoot.GetComponentsInChildren<Renderer>(true);
            foreach (var renderer in renderers)
            {
                var materials = renderer.materials;
                for (var i = 0; i < materials.Length; i++)
                {
                    var material = materials[i];
                    if (material == null || !material.HasProperty("_Color"))
                    {
                        continue;
                    }

                    var color = material.color;
                    color.r *= preset.AlbedoMultiplier;
                    color.g *= preset.AlbedoMultiplier;
                    color.b *= preset.AlbedoMultiplier;
                    material.color = color;
                }
            }
        }

        private void CreateFallbackPrimitive(
            string requestId,
            string reason,
            string path,
            string sourceTier,
            string errorCode)
        {
            var capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            capsule.name = "SimpleModelFallback";
            capsule.transform.position = new Vector3(0f, 0f, 3f);
            capsule.transform.localScale = _config.fallbackScale;

            var renderer = capsule.GetComponent<Renderer>();
            var fallbackShader = Shader.Find("Standard") ?? Shader.Find("Unlit/Color");
            renderer.material = new Material(fallbackShader)
            {
                color = _config.fallbackColor
            };
            ApplyRenderFactorToModel(capsule);
            RegisterActiveModelRoot(capsule, enablePlacementStabilization: false);

            RuntimeLog.Warn(
                "avatar",
                "avatar.model.placeholder_displayed",
                requestId,
                string.IsNullOrWhiteSpace(errorCode) ? "ASSET.PLACEHOLDER.USED" : errorCode,
                reason,
                path,
                SafeSourceTier(sourceTier, "placeholder"));
        }

        private void RegisterActiveModelRoot(GameObject root, bool enablePlacementStabilization)
        {
            if (root == null)
            {
                return;
            }

            var previousRoot = _activeModelRoot;
            _activeModelRoot = root;
            if (root.transform.parent != transform)
            {
                root.transform.SetParent(transform, true);
            }

            if (previousRoot != null && previousRoot != root)
            {
                Destroy(previousRoot);
            }

            _placementStabilizationFramesRemaining = enablePlacementStabilization
                ? ModelPlacementStabilizationFrames
                : 0;
        }

        private void StabilizeActiveModelPlacement()
        {
            if (_activeModelRoot == null || !TryGetModelBounds(_activeModelRoot, out var bounds))
            {
                return;
            }

            var camera = Camera.main;
            if (camera == null)
            {
                camera = FindFirstObjectByType<Camera>();
            }

            if (camera == null)
            {
                return;
            }

            var viewportCenter = camera.WorldToViewportPoint(bounds.center);
            var isOutside =
                viewportCenter.z < camera.nearClipPlane + 0.1f ||
                viewportCenter.x < -0.15f || viewportCenter.x > 1.15f ||
                viewportCenter.y < -0.15f || viewportCenter.y > 1.15f;

            if (!isOutside)
            {
                return;
            }

            CenterModelAtTargetDepth(_activeModelRoot, bounds, ModelTargetZ);
            if (!TryGetModelBounds(_activeModelRoot, out bounds))
            {
                return;
            }

            FitModelToCamera(_activeModelRoot, bounds);
        }

        private int NormalizeRenderFactorIndex(int index)
        {
            if (RenderFactorPresets.Length == 0)
            {
                return 0;
            }

            if (index < 0)
            {
                return RenderFactorPresets.Length - 1;
            }

            if (index >= RenderFactorPresets.Length)
            {
                return 0;
            }

            return index;
        }

        private RenderFactorPreset GetRenderFactorPreset()
        {
            if (RenderFactorPresets.Length == 0)
            {
                return new RenderFactorPreset("Fallback", 0.58f, new Color(0.10f, 0.10f, 0.12f, 1f), 1f);
            }

            _renderFactorIndex = NormalizeRenderFactorIndex(_renderFactorIndex);
            return RenderFactorPresets[_renderFactorIndex];
        }

        private static string SafeSourceTier(string sourceTier, string fallback)
        {
            return string.IsNullOrWhiteSpace(sourceTier) ? fallback : sourceTier;
        }

        private void EnsureCameraAndLight()
        {
            ApplyRuntimeRenderQuality();

            var camera = Camera.main;
            var createdCamera = false;
            if (camera == null)
            {
                camera = FindFirstObjectByType<Camera>();
            }

            if (camera == null)
            {
                var cameraObject = new GameObject("SimpleModelCamera");
                cameraObject.tag = "MainCamera";
                camera = cameraObject.AddComponent<Camera>();
                createdCamera = true;
            }

            ConfigureCamera(camera, createdCamera);

            var light = FindFirstObjectByType<Light>();
            var autoConfigureSceneLight = _config != null && _config.autoConfigureSceneLight;
            var disableExistingSceneLightsWhenAutoConfigOff =
                _config != null && _config.disableExistingSceneLightsWhenAutoConfigOff;
            var createSceneLightIfMissing = _config != null && _config.createSceneLightIfMissing;
            if (!autoConfigureSceneLight)
            {
                var disabledSceneLightsCount = 0;
                var bootstrapLightDisabledByBootstrap = false;
                if (disableExistingSceneLightsWhenAutoConfigOff)
                {
                    var lights = FindObjectsByType<Light>(FindObjectsSortMode.None);
                    foreach (var sceneLight in lights)
                    {
                        if (sceneLight == null || !sceneLight.enabled)
                        {
                            continue;
                        }

                        sceneLight.enabled = false;
                        disabledSceneLightsCount++;
                    }
                }
                else
                {
                    var bootstrapLightObject = GameObject.Find("SimpleModelLight");
                    var bootstrapLight = bootstrapLightObject != null ? bootstrapLightObject.GetComponent<Light>() : null;
                    if (bootstrapLight != null && bootstrapLight.enabled)
                    {
                        bootstrapLight.enabled = false;
                        bootstrapLightDisabledByBootstrap = true;
                    }
                }

                var requestId = RuntimeLog.NewRequestId();
                RuntimeLog.Info(
                    "avatar",
                    "avatar.render.light.autoconfig_skipped",
                    requestId,
                    $"autoConfigureSceneLight=false, disableExistingSceneLightsWhenAutoConfigOff={disableExistingSceneLightsWhenAutoConfigOff}, existingLight={(light != null ? light.name : "none")}, disabledSceneLightsCount={disabledSceneLightsCount}, simpleModelLightDisabledByBootstrap={bootstrapLightDisabledByBootstrap}",
                    string.Empty,
                    RenderSourceTier);
            }
            else
            {
                if (light == null && createSceneLightIfMissing)
                {
                    var lightObj = new GameObject("SimpleModelLight");
                    light = lightObj.AddComponent<Light>();
                }

                ConfigureLighting(light);
            }

            LogRenderEnvironmentDiagnostics(camera, light);
        }

        private static void ConfigureCamera(Camera camera, bool createdCamera)
        {
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.08f, 0.08f, 0.1f, 1f);
            camera.fieldOfView = CameraFieldOfView;
            camera.allowMSAA = true;
            camera.allowHDR = false;
            camera.allowDynamicResolution = false;
            camera.nearClipPlane = 0.03f;
            camera.farClipPlane = 200f;

            if (createdCamera)
            {
                camera.transform.position = new Vector3(0f, 0f, -6f);
                camera.transform.rotation = Quaternion.identity;
            }
        }

        private void ConfigureLighting(Light light)
        {
            var preset = GetRenderFactorPreset();
            if (light != null)
            {
                light.type = LightType.Directional;
                light.intensity = preset.KeyLightIntensity;
                light.color = Color.white;
                light.shadows = LightShadows.Soft;
                light.shadowStrength = DefaultKeyLightShadowStrength;
                light.transform.rotation = Quaternion.Euler(45f, -35f, 0f);
            }

            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = preset.AmbientLightColor;

            var requestId = RuntimeLog.NewRequestId();
            RuntimeLog.Info(
                "avatar",
                "avatar.render.factor.applied",
                requestId,
                $"render factor applied: {preset.DisplayName}, key={preset.KeyLightIntensity:0.##}, albedo={preset.AlbedoMultiplier:0.##}",
                string.Empty,
                RenderSourceTier);
        }

        private static void ApplyRuntimeRenderQuality()
        {
#pragma warning disable CS0618
            QualitySettings.masterTextureLimit = 0;
#pragma warning restore CS0618
            QualitySettings.maximumLODLevel = 0;
            QualitySettings.lodBias = Mathf.Max(QualitySettings.lodBias, 2f);
            QualitySettings.anisotropicFiltering = AnisotropicFiltering.ForceEnable;
            QualitySettings.antiAliasing = Mathf.Max(QualitySettings.antiAliasing, MinAntiAliasingSamples);
            QualitySettings.streamingMipmapsActive = false;

            var requestId = RuntimeLog.NewRequestId();
            RuntimeLog.Info(
                "avatar",
                "avatar.render.quality.applied",
                requestId,
                $"render quality applied: level={QualitySettings.names[QualitySettings.GetQualityLevel()]}, aa={QualitySettings.antiAliasing}, lodBias={QualitySettings.lodBias:0.##}",
                string.Empty,
                "render");
        }

        private static void LogRenderEnvironmentDiagnostics(Camera camera, Light light)
        {
            if (camera == null)
            {
                return;
            }

            var requestId = RuntimeLog.NewRequestId();
            var lightInfo = light == null
                ? "none"
                : $"{light.type},intensity={light.intensity:0.##},shadow={light.shadowStrength:0.##}";
            RuntimeLog.Info(
                "avatar",
                "avatar.render.environment_diagnostics",
                requestId,
                $"colorSpace={QualitySettings.activeColorSpace}, quality={QualitySettings.names[QualitySettings.GetQualityLevel()]}, aa={QualitySettings.antiAliasing}, lodBias={QualitySettings.lodBias:0.##}, cameraHdr={camera.allowHDR}, cameraMsaa={camera.allowMSAA}, near={camera.nearClipPlane:0.###}, far={camera.farClipPlane:0.###}, ambient={RenderSettings.ambientLight.r:0.##}/{RenderSettings.ambientLight.g:0.##}/{RenderSettings.ambientLight.b:0.##}, light={lightInfo}",
                string.Empty,
                RenderSourceTier);
        }
    }
}
