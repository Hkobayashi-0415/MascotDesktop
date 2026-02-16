using System;
using System.Linq;
using System.Reflection;
using LibMMD.Material;
using MascotDesktop.Runtime.Avatar;
using NUnit.Framework;
using UnityEngine;

namespace MascotDesktop.Tests.EditMode
{
    public sealed class SimpleModelBootstrapTests
    {
        [Test]
        public void BuildModelCandidatesFromRelativePaths_IncludesModelFormatsOnly()
        {
            var paths = new[]
            {
                "characters/demo/mmd/avatar.pmx",
                "characters/demo/mmd/avatar_legacy.pmd",
                "characters/demo/tex/facetoon.bmp",
                "characters/demo/tex/readme.txt"
            };

            var candidates = SimpleModelBootstrap.BuildModelCandidatesFromRelativePaths(paths);

            Assert.That(candidates, Does.Contain("characters/demo/mmd/avatar.pmx"));
            Assert.That(candidates, Does.Contain("characters/demo/mmd/avatar_legacy.pmd"));
            Assert.That(candidates, Does.Not.Contain("characters/demo/tex/facetoon.bmp"));
        }

        [Test]
        public void BuildModelCandidatesFromRelativePaths_ReturnsSortedDistinctList()
        {
            var paths = new[]
            {
                "characters/demo/mmd/Zeta.vrm",
                "characters/demo/mmd/avatar.pmx",
                "characters/demo/mmd/avatar.pmx",
                "characters/demo/mmd/Alpha.pmd",
                "characters/demo/images/face_main.png"
            };

            var candidates = SimpleModelBootstrap.BuildModelCandidatesFromRelativePaths(paths);

            Assert.That(candidates, Is.EqualTo(new[]
            {
                "characters/demo/mmd/Alpha.pmd",
                "characters/demo/mmd/avatar.pmx",
                "characters/demo/mmd/Zeta.vrm"
            }));
        }

        [Test]
        public void BuildImageCandidatesFromRelativePaths_LimitsImagesPerGroupToFour()
        {
            var paths = new[]
            {
                "characters/demo/images/face_main.png",
                "characters/demo/images/body_main.png",
                "characters/demo/images/hair_main.png",
                "characters/demo/images/main_albedo.png",
                "characters/demo/images/tex_extra.png",
                "characters/demo/images/zzz_other.png"
            };

            var candidates = SimpleModelBootstrap.BuildImageCandidatesFromRelativePaths(paths);
            var selectedImages = candidates.Where(p => p.StartsWith("characters/demo/", System.StringComparison.OrdinalIgnoreCase)).ToArray();

            Assert.That(selectedImages.Length, Is.EqualTo(4));
            Assert.That(selectedImages, Does.Contain("characters/demo/images/face_main.png"));
            Assert.That(selectedImages, Does.Contain("characters/demo/images/body_main.png"));
            Assert.That(selectedImages, Does.Contain("characters/demo/images/hair_main.png"));
            Assert.That(selectedImages, Does.Contain("characters/demo/images/main_albedo.png"));
            Assert.That(selectedImages, Does.Not.Contain("characters/demo/images/tex_extra.png"));
        }

        [Test]
        public void ComputeImagePlaneScale_FitsLandscapeIntoFallbackBounds()
        {
            var fallback = new Vector3(1.6f, 2.2f, 1f);

            var scale = SimpleModelBootstrap.ComputeImagePlaneScale(1600, 900, fallback);

            Assert.That(scale.x, Is.EqualTo(1.6f).Within(0.0001f));
            Assert.That(scale.y, Is.EqualTo(0.9f).Within(0.0001f));
            Assert.That(scale.z, Is.EqualTo(1f).Within(0.0001f));
        }

        [Test]
        public void ComputeImagePlaneScale_FitsPortraitIntoFallbackBounds()
        {
            var fallback = new Vector3(1.6f, 2.2f, 1f);

            var scale = SimpleModelBootstrap.ComputeImagePlaneScale(900, 1600, fallback);

            Assert.That(scale.x, Is.EqualTo(1.2375f).Within(0.0001f));
            Assert.That(scale.y, Is.EqualTo(2.2f).Within(0.0001f));
            Assert.That(scale.z, Is.EqualTo(1f).Within(0.0001f));
        }

        [Test]
        public void ComputeImagePlaneScale_ReturnsFallback_WhenTextureSizeInvalid()
        {
            var fallback = new Vector3(1.6f, 2.2f, 1f);

            var scale = SimpleModelBootstrap.ComputeImagePlaneScale(0, 1600, fallback);

            Assert.That(scale, Is.EqualTo(fallback));
        }

        [Test]
        public void ComputeModelScaleFactor_ClampsToConfiguredRange()
        {
            var tooSmall = SimpleModelBootstrap.ComputeModelScaleFactor(500f, 2.4f, 0.05f, 12f);
            var tooLarge = SimpleModelBootstrap.ComputeModelScaleFactor(0.001f, 2.4f, 0.05f, 12f);

            Assert.That(tooSmall, Is.EqualTo(0.05f).Within(0.0001f));
            Assert.That(tooLarge, Is.EqualTo(12f).Within(0.0001f));
        }

        [Test]
        public void ComputeCameraFitScale_ReturnsOne_WhenModelAlreadyFits()
        {
            var scale = SimpleModelBootstrap.ComputeCameraFitScale(
                modelHalfWidth: 0.5f,
                modelHalfHeight: 0.8f,
                cameraDistance: 8.5f,
                cameraFieldOfView: 24f,
                cameraAspect: 16f / 9f,
                padding: 0.88f);

            Assert.That(scale, Is.EqualTo(1f).Within(0.0001f));
        }

        [Test]
        public void ComputeCameraFitScale_ReturnsLessThanOne_WhenModelExceedsView()
        {
            var scale = SimpleModelBootstrap.ComputeCameraFitScale(
                modelHalfWidth: 10f,
                modelHalfHeight: 10f,
                cameraDistance: 8.5f,
                cameraFieldOfView: 24f,
                cameraAspect: 16f / 9f,
                padding: 0.88f);

            Assert.That(scale, Is.LessThan(1f));
            Assert.That(scale, Is.GreaterThanOrEqualTo(0.05f));
        }

        [Test]
        public void DiagnosticsHelpers_SkipRootPlaceholderSkinnedRenderer()
        {
            var modelRoot = new GameObject("ModelRoot");
            var rootRenderer = modelRoot.AddComponent<SkinnedMeshRenderer>();
            var child = new GameObject("Child");
            child.transform.SetParent(modelRoot.transform, false);
            var childRenderer = child.AddComponent<SkinnedMeshRenderer>();

            try
            {
                Assert.That(InvokeShouldSkipRendererForDiagnostics(rootRenderer, modelRoot), Is.True);
                Assert.That(InvokeShouldSkipRendererForDiagnostics(childRenderer, modelRoot), Is.False);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(child);
                UnityEngine.Object.DestroyImmediate(modelRoot);
            }
        }

        [Test]
        public void DiagnosticsHelpers_SkipsMaterialsWithoutMainTexAndStatusTag()
        {
            var colorShader = Shader.Find("Unlit/Color");
            Assert.That(colorShader, Is.Not.Null, "Unlit/Color shader is required for this test.");

            var textureShader = Shader.Find("Unlit/Texture") ?? Shader.Find("Standard");
            Assert.That(textureShader, Is.Not.Null, "Texture-capable shader is required for this test.");

            var colorOnlyMaterial = new Material(colorShader);
            var texturedMaterial = new Material(textureShader);
            try
            {
                Assert.That(InvokeShouldInspectMainTextureMaterial(colorOnlyMaterial, string.Empty), Is.False);
                Assert.That(InvokeShouldInspectMainTextureMaterial(colorOnlyMaterial, "missing_resolve"), Is.True);
                Assert.That(InvokeShouldInspectMainTextureMaterial(texturedMaterial, string.Empty), Is.True);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(colorOnlyMaterial);
                UnityEngine.Object.DestroyImmediate(texturedMaterial);
            }
        }

        [Test]
        public void TextureLoaderScore_PrefersModelNearbyCandidate()
        {
            const string modelBaseDir = @"D:\assets\characters\nurse\model";
            const string requestedPath = @"texture\body.png";
            const string nearbyCandidate = @"D:\assets\characters\nurse\texture\body.png";
            const string farCandidate = @"D:\assets\characters\other\texture\body.png";

            var nearbyScore = InvokeTextureLoaderScore(nearbyCandidate, modelBaseDir, requestedPath);
            var farScore = InvokeTextureLoaderScore(farCandidate, modelBaseDir, requestedPath);

            Assert.That(nearbyScore, Is.LessThan(farScore));
        }

        [Test]
        public void TextureLoaderScore_PrefersTextureFolderWhenDistanceMatches()
        {
            const string modelBaseDir = @"D:\assets\characters\nurse\model";
            const string requestedPath = @"textures\body.png";
            const string texturesCandidate = @"D:\assets\characters\nurse\textures\body.png";
            const string miscCandidate = @"D:\assets\characters\nurse\misc\body.png";

            var texturesScore = InvokeTextureLoaderScore(texturesCandidate, modelBaseDir, requestedPath);
            var miscScore = InvokeTextureLoaderScore(miscCandidate, modelBaseDir, requestedPath);

            Assert.That(texturesScore, Is.LessThan(miscScore));
        }

        [Test]
        public void DiagnosticsHelpers_IsTextureFilePath_DetectsKnownExtensions()
        {
            Assert.That(InvokeIsTextureFilePath(@"D:\a\b\c.png"), Is.True);
            Assert.That(InvokeIsTextureFilePath(@"D:\a\b\c.sph"), Is.True);
            Assert.That(InvokeIsTextureFilePath(@"D:\a\b\c.txt"), Is.False);
            Assert.That(InvokeIsTextureFilePath(string.Empty), Is.False);
        }

        [Test]
        public void MaterialLoaderTransparencyHeuristic_IgnoresSparseAlphaNoise()
        {
            var mostlyOpaque = Enumerable.Repeat(new Color(1f, 1f, 1f, 1f), 100).ToArray();
            mostlyOpaque[0] = new Color(1f, 1f, 1f, 0.90f);
            Assert.That(InvokeMaterialLoaderIsTransparentByPixels(mostlyOpaque), Is.False);

            var mostlyTransparent = Enumerable.Repeat(new Color(1f, 1f, 1f, 1f), 100).ToArray();
            for (var i = 0; i < 20; i++)
            {
                mostlyTransparent[i] = new Color(1f, 1f, 1f, 0.40f);
            }
            Assert.That(InvokeMaterialLoaderIsTransparentByPixels(mostlyTransparent), Is.True);
        }

        [Test]
        public void MaterialLoaderTransparencyHeuristic_UsesSemiTransparentRatioThreshold()
        {
            var belowThreshold = Enumerable.Repeat(new Color(1f, 1f, 1f, 1f), 100).ToArray();
            for (var i = 0; i < 9; i++)
            {
                belowThreshold[i] = new Color(1f, 1f, 1f, 0.59f);
            }
            Assert.That(InvokeMaterialLoaderIsTransparentByPixels(belowThreshold), Is.False);

            var atThreshold = Enumerable.Repeat(new Color(1f, 1f, 1f, 1f), 100).ToArray();
            for (var i = 0; i < 10; i++)
            {
                atThreshold[i] = new Color(1f, 1f, 1f, 0.59f);
            }
            Assert.That(InvokeMaterialLoaderIsTransparentByPixels(atThreshold), Is.True);
        }

        [Test]
        public void MaterialLoaderTransparencyHeuristic_UsesStrongTransparentRatioThreshold()
        {
            var belowStrongThreshold = Enumerable.Repeat(new Color(1f, 1f, 1f, 1f), 1000).ToArray();
            for (var i = 0; i < 31; i++)
            {
                belowStrongThreshold[i] = new Color(1f, 1f, 1f, 0.10f);
            }
            Assert.That(InvokeMaterialLoaderIsTransparentByPixels(belowStrongThreshold), Is.False);

            var atStrongThreshold = Enumerable.Repeat(new Color(1f, 1f, 1f, 1f), 1000).ToArray();
            for (var i = 0; i < 32; i++)
            {
                atStrongThreshold[i] = new Color(1f, 1f, 1f, 0.10f);
            }
            Assert.That(InvokeMaterialLoaderIsTransparentByPixels(atStrongThreshold), Is.True);
        }

        [Test]
        public void MaterialLoaderTransparencyHeuristic_DoesNotTriggerOnLargeTextureSparseStrongAlpha()
        {
            var largeSparseStrong = Enumerable.Repeat(new Color(1f, 1f, 1f, 1f), 10000).ToArray();
            for (var i = 0; i < 40; i++)
            {
                largeSparseStrong[i] = new Color(1f, 1f, 1f, 0.05f);
            }

            Assert.That(InvokeMaterialLoaderIsTransparentByPixels(largeSparseStrong), Is.False);
        }

        [Test]
        public void MaterialLoaderTransparencyHeuristic_CalculatesRequiredPixelsAtBoundary()
        {
            Assert.That(InvokeMaterialLoaderRequiredTransparentPixels(100, 0.10f), Is.EqualTo(10));
            Assert.That(InvokeMaterialLoaderRequiredTransparentPixels(1000, 0.032f), Is.EqualTo(32));
        }

        [Test]
        public void MaterialLoaderTransparentReasonTag_BuildsExpectedValues()
        {
            Assert.That(InvokeMaterialLoaderBuildTransparentReasonTag(false, false, false), Is.EqualTo("opaque"));
            Assert.That(InvokeMaterialLoaderBuildTransparentReasonTag(true, false, false), Is.EqualTo("diffuse_alpha"));
            Assert.That(InvokeMaterialLoaderBuildTransparentReasonTag(false, true, false), Is.EqualTo("edge_alpha"));
            Assert.That(InvokeMaterialLoaderBuildTransparentReasonTag(false, false, true), Is.EqualTo("texture_alpha"));
            Assert.That(InvokeMaterialLoaderBuildTransparentReasonTag(true, false, true), Is.EqualTo("diffuse_alpha+texture_alpha"));
            Assert.That(InvokeMaterialLoaderBuildTransparentReasonTag(true, true, true), Is.EqualTo("diffuse_alpha+edge_alpha+texture_alpha"));
        }

        [Test]
        public void DiagnosticsHelpers_HasTransparentReason_SplitsCombinedTag()
        {
            const string combined = "diffuse_alpha+texture_alpha";
            Assert.That(InvokeHasTransparentReason(combined, "diffuse_alpha"), Is.True);
            Assert.That(InvokeHasTransparentReason(combined, "texture_alpha"), Is.True);
            Assert.That(InvokeHasTransparentReason(combined, "edge_alpha"), Is.False);
            Assert.That(InvokeHasTransparentReason("opaque", "diffuse_alpha"), Is.False);
        }

        [Test]
        public void DiagnosticsHelpers_CountMissingTextureStatus_ClassifiesSpecResolveUnknown()
        {
            var spec = InvokeCountMissingTextureStatus("missing_spec");
            Assert.That(spec.Spec, Is.EqualTo(1));
            Assert.That(spec.Resolve, Is.EqualTo(0));
            Assert.That(spec.Unknown, Is.EqualTo(0));

            var resolve = InvokeCountMissingTextureStatus("missing_resolve");
            Assert.That(resolve.Spec, Is.EqualTo(0));
            Assert.That(resolve.Resolve, Is.EqualTo(1));
            Assert.That(resolve.Unknown, Is.EqualTo(0));

            var unknown = InvokeCountMissingTextureStatus("not_used");
            Assert.That(unknown.Spec, Is.EqualTo(0));
            Assert.That(unknown.Resolve, Is.EqualTo(0));
            Assert.That(unknown.Unknown, Is.EqualTo(1));
        }

        [Test]
        public void DiagnosticsHelpers_IsTextureStatus_MatchesIgnoreCase()
        {
            Assert.That(InvokeIsTextureStatus("not_used", "not_used"), Is.True);
            Assert.That(InvokeIsTextureStatus("Not_Used", "not_used"), Is.True);
            Assert.That(InvokeIsTextureStatus("missing_spec", "not_used"), Is.False);
        }

        [Test]
        public void DiagnosticsHelpers_BuildRemediationHint_PrefersAssetResolutionWhenResolveMissingExists()
        {
            var hint = InvokeBuildRemediationHint(
                materialCount: 16,
                transparentShaderMaterials: 8,
                transparentByTextureAlphaMaterials: 4,
                transparentByEdgeAlphaMaterials: 4,
                highShininessMaterials: 8,
                brightDiffuseMaterials: 12,
                missingResolveTotal: 1);

            Assert.That(hint, Is.EqualTo("asset_resolution_first"));
        }

        [Test]
        public void DiagnosticsHelpers_BuildRemediationHint_PrefersMaterialLoaderWhenTextureAlphaDominates()
        {
            var hint = InvokeBuildRemediationHint(
                materialCount: 20,
                transparentShaderMaterials: 10,
                transparentByTextureAlphaMaterials: 8,
                transparentByEdgeAlphaMaterials: 1,
                highShininessMaterials: 1,
                brightDiffuseMaterials: 3,
                missingResolveTotal: 0);

            Assert.That(hint, Is.EqualTo("materialloader_threshold_candidate"));
        }

        [Test]
        public void DiagnosticsHelpers_BuildRemediationHint_PrefersShaderLightingWhenEdgeAlphaDominates()
        {
            var hint = InvokeBuildRemediationHint(
                materialCount: 20,
                transparentShaderMaterials: 10,
                transparentByTextureAlphaMaterials: 2,
                transparentByEdgeAlphaMaterials: 8,
                highShininessMaterials: 0,
                brightDiffuseMaterials: 2,
                missingResolveTotal: 0);

            Assert.That(hint, Is.EqualTo("shader_lighting_candidate"));
        }

        [Test]
        public void DiagnosticsHelpers_BuildRemediationHint_PrefersShaderLightingWhenBrightDiffuseIsHigh()
        {
            var hint = InvokeBuildRemediationHint(
                materialCount: 10,
                transparentShaderMaterials: 1,
                transparentByTextureAlphaMaterials: 0,
                transparentByEdgeAlphaMaterials: 0,
                highShininessMaterials: 1,
                brightDiffuseMaterials: 8,
                missingResolveTotal: 0);

            Assert.That(hint, Is.EqualTo("shader_lighting_candidate"));
        }

        [Test]
        public void DiagnosticsHelpers_BuildRemediationHint_ReturnsMixedWhenNoDominantSignal()
        {
            var hint = InvokeBuildRemediationHint(
                materialCount: 10,
                transparentShaderMaterials: 4,
                transparentByTextureAlphaMaterials: 1,
                transparentByEdgeAlphaMaterials: 1,
                highShininessMaterials: 1,
                brightDiffuseMaterials: 2,
                missingResolveTotal: 0);

            Assert.That(hint, Is.EqualTo("mixed_followup"));
        }

        [Test]
        public void MaterialLoaderShaderCaps_UsesStrongerSpecularClampForHighShininess()
        {
            var highShininess = new MmdMaterial { Shiness = 50f };
            var lowShininess = new MmdMaterial { Shiness = 5f };

            var highCap = InvokeMaterialLoaderSpecularContributionCap(highShininess);
            var lowCap = InvokeMaterialLoaderSpecularContributionCap(lowShininess);

            Assert.That(highCap, Is.EqualTo(0.32f).Within(0.0001f));
            Assert.That(lowCap, Is.EqualTo(0.45f).Within(0.0001f));
        }

        [Test]
        public void MaterialLoaderShaderCaps_UsesStrongerEdgeClampWhenEdgeAlphaIsLow()
        {
            var lowEdgeAlpha = new MmdMaterial { EdgeColor = new Color(0f, 0f, 0f, 0.7f) };
            var opaqueEdge = new MmdMaterial { EdgeColor = new Color(0f, 0f, 0f, 1f) };

            var lowAlphaCap = InvokeMaterialLoaderEdgeContributionCap(lowEdgeAlpha);
            var opaqueCap = InvokeMaterialLoaderEdgeContributionCap(opaqueEdge);

            Assert.That(lowAlphaCap, Is.EqualTo(0.55f).Within(0.0001f));
            Assert.That(opaqueCap, Is.EqualTo(0.75f).Within(0.0001f));
        }

        [Test]
        public void MaterialLoaderWhiteFallback_DoesNotUseOpaqueMissingSpecFallback()
        {
            var material = new MmdMaterial
            {
                Name = "Body",
                DiffuseColor = new Color(1f, 1f, 1f, 1f)
            };

            var useFallback = InvokeMaterialLoaderShouldUseWhiteFallbackMainTexture(material, "opaque");

            Assert.That(useFallback, Is.False);
        }

        [Test]
        public void MaterialLoaderWhiteFallback_DoesNotUseOpaqueFallbackForTransparentMaterial()
        {
            var material = new MmdMaterial
            {
                Name = "Hair",
                DiffuseColor = new Color(1f, 1f, 1f, 0.6f)
            };

            var useFallback = InvokeMaterialLoaderShouldUseWhiteFallbackMainTexture(material, "diffuse_alpha");

            Assert.That(useFallback, Is.False);
        }

        [Test]
        public void MaterialLoaderWhiteFallback_KeepsShadowFallbackForLowAlphaShadowMesh()
        {
            var material = new MmdMaterial
            {
                Name = "self_shadow_plane",
                DiffuseColor = new Color(1f, 1f, 1f, 0.2f)
            };

            var useFallback = InvokeMaterialLoaderShouldUseWhiteFallbackMainTexture(material, "diffuse_alpha");

            Assert.That(useFallback, Is.True);
        }

        private static bool InvokeShouldSkipRendererForDiagnostics(Renderer renderer, GameObject modelRoot)
        {
            var method = typeof(SimpleModelBootstrap).GetMethod(
                "ShouldSkipRendererForMissingMainTextureDiagnostics",
                BindingFlags.NonPublic | BindingFlags.Static);
            Assert.That(method, Is.Not.Null);
            return (bool)method.Invoke(null, new object[] { renderer, modelRoot });
        }

        private static bool InvokeShouldInspectMainTextureMaterial(Material material, string textureStatus)
        {
            var method = typeof(SimpleModelBootstrap).GetMethod(
                "ShouldInspectMainTextureMaterial",
                BindingFlags.NonPublic | BindingFlags.Static);
            Assert.That(method, Is.Not.Null);
            return (bool)method.Invoke(null, new object[] { material, textureStatus });
        }

        private static int InvokeTextureLoaderScore(string candidatePath, string modelBaseDir, string requestedPath)
        {
            var textureLoaderType = AppDomain.CurrentDomain
                .GetAssemblies()
                .Select(assembly => assembly.GetType("LibMMD.Unity3D.TextureLoader", false))
                .FirstOrDefault(type => type != null);
            Assert.That(textureLoaderType, Is.Not.Null, "LibMMD.Unity3D.TextureLoader type not found.");

            var method = textureLoaderType.GetMethod(
                "ScoreTexturePathForRequest",
                BindingFlags.NonPublic | BindingFlags.Static);
            Assert.That(method, Is.Not.Null);

            return (int)method.Invoke(null, new object[] { candidatePath, modelBaseDir, requestedPath });
        }

        private static bool InvokeIsTextureFilePath(string path)
        {
            var method = typeof(SimpleModelBootstrap).GetMethod(
                "IsTextureFilePath",
                BindingFlags.NonPublic | BindingFlags.Static);
            Assert.That(method, Is.Not.Null);
            return (bool)method.Invoke(null, new object[] { path });
        }

        private static bool InvokeMaterialLoaderIsTransparentByPixels(Color[] pixels)
        {
            var materialLoaderType = AppDomain.CurrentDomain
                .GetAssemblies()
                .Select(assembly => assembly.GetType("LibMMD.Unity3D.MaterialLoader", false))
                .FirstOrDefault(type => type != null);
            Assert.That(materialLoaderType, Is.Not.Null, "LibMMD.Unity3D.MaterialLoader type not found.");

            var method = materialLoaderType.GetMethod(
                "IsTextureTransparentByPixels",
                BindingFlags.NonPublic | BindingFlags.Static);
            Assert.That(method, Is.Not.Null, "IsTextureTransparentByPixels not found.");
            return (bool)method.Invoke(null, new object[] { pixels });
        }

        private static int InvokeMaterialLoaderRequiredTransparentPixels(int totalPixels, float ratioThreshold)
        {
            var materialLoaderType = AppDomain.CurrentDomain
                .GetAssemblies()
                .Select(assembly => assembly.GetType("LibMMD.Unity3D.MaterialLoader", false))
                .FirstOrDefault(type => type != null);
            Assert.That(materialLoaderType, Is.Not.Null, "LibMMD.Unity3D.MaterialLoader type not found.");

            var method = materialLoaderType.GetMethod(
                "CalculateRequiredTransparentPixels",
                BindingFlags.NonPublic | BindingFlags.Static);
            Assert.That(method, Is.Not.Null, "CalculateRequiredTransparentPixels not found.");
            return (int)method.Invoke(null, new object[] { totalPixels, ratioThreshold });
        }

        private static string InvokeMaterialLoaderBuildTransparentReasonTag(bool byDiffuseAlpha, bool byEdgeAlpha, bool byTextureAlpha)
        {
            var materialLoaderType = AppDomain.CurrentDomain
                .GetAssemblies()
                .Select(assembly => assembly.GetType("LibMMD.Unity3D.MaterialLoader", false))
                .FirstOrDefault(type => type != null);
            Assert.That(materialLoaderType, Is.Not.Null, "LibMMD.Unity3D.MaterialLoader type not found.");

            var method = materialLoaderType.GetMethod(
                "BuildTransparentReasonTag",
                BindingFlags.NonPublic | BindingFlags.Static);
            Assert.That(method, Is.Not.Null, "BuildTransparentReasonTag not found.");
            return (string)method.Invoke(null, new object[] { byDiffuseAlpha, byEdgeAlpha, byTextureAlpha });
        }

        private static bool InvokeHasTransparentReason(string transparentReasonTag, string expectedReason)
        {
            var method = typeof(SimpleModelBootstrap).GetMethod(
                "HasTransparentReason",
                BindingFlags.NonPublic | BindingFlags.Static);
            Assert.That(method, Is.Not.Null, "HasTransparentReason not found.");
            return (bool)method.Invoke(null, new object[] { transparentReasonTag, expectedReason });
        }

        private static bool InvokeMaterialLoaderShouldUseWhiteFallbackMainTexture(MmdMaterial material, string transparentReason)
        {
            var materialLoaderType = AppDomain.CurrentDomain
                .GetAssemblies()
                .Select(assembly => assembly.GetType("LibMMD.Unity3D.MaterialLoader", false))
                .FirstOrDefault(type => type != null);
            Assert.That(materialLoaderType, Is.Not.Null, "LibMMD.Unity3D.MaterialLoader type not found.");

            var method = materialLoaderType.GetMethod(
                "ShouldUseWhiteFallbackMainTexture",
                BindingFlags.NonPublic | BindingFlags.Static,
                null,
                new[] { typeof(MmdMaterial), typeof(string) },
                null);
            Assert.That(method, Is.Not.Null, "ShouldUseWhiteFallbackMainTexture not found.");
            return (bool)method.Invoke(null, new object[] { material, transparentReason });
        }

        private static (int Spec, int Resolve, int Unknown) InvokeCountMissingTextureStatus(string textureStatus)
        {
            var method = typeof(SimpleModelBootstrap).GetMethod(
                "CountMissingTextureStatus",
                BindingFlags.NonPublic | BindingFlags.Static);
            Assert.That(method, Is.Not.Null, "CountMissingTextureStatus not found.");

            var args = new object[] { textureStatus, 0, 0, 0 };
            method.Invoke(null, args);
            return ((int)args[1], (int)args[2], (int)args[3]);
        }

        private static bool InvokeIsTextureStatus(string textureStatus, string expectedStatus)
        {
            var method = typeof(SimpleModelBootstrap).GetMethod(
                "IsTextureStatus",
                BindingFlags.NonPublic | BindingFlags.Static);
            Assert.That(method, Is.Not.Null, "IsTextureStatus not found.");
            return (bool)method.Invoke(null, new object[] { textureStatus, expectedStatus });
        }

        private static string InvokeBuildRemediationHint(
            int materialCount,
            int transparentShaderMaterials,
            int transparentByTextureAlphaMaterials,
            int transparentByEdgeAlphaMaterials,
            int highShininessMaterials,
            int brightDiffuseMaterials,
            int missingResolveTotal)
        {
            var method = typeof(SimpleModelBootstrap).GetMethod(
                "BuildRemediationHint",
                BindingFlags.NonPublic | BindingFlags.Static);
            Assert.That(method, Is.Not.Null, "BuildRemediationHint not found.");
            return (string)method.Invoke(null, new object[]
            {
                materialCount,
                transparentShaderMaterials,
                transparentByTextureAlphaMaterials,
                transparentByEdgeAlphaMaterials,
                highShininessMaterials,
                brightDiffuseMaterials,
                missingResolveTotal
            });
        }

        private static float InvokeMaterialLoaderSpecularContributionCap(MmdMaterial material)
        {
            var materialLoaderType = AppDomain.CurrentDomain
                .GetAssemblies()
                .Select(assembly => assembly.GetType("LibMMD.Unity3D.MaterialLoader", false))
                .FirstOrDefault(type => type != null);
            Assert.That(materialLoaderType, Is.Not.Null, "LibMMD.Unity3D.MaterialLoader type not found.");

            var method = materialLoaderType.GetMethod(
                "ResolveSpecularContributionCap",
                BindingFlags.NonPublic | BindingFlags.Static);
            Assert.That(method, Is.Not.Null, "ResolveSpecularContributionCap not found.");
            return (float)method.Invoke(null, new object[] { material });
        }

        private static float InvokeMaterialLoaderEdgeContributionCap(MmdMaterial material)
        {
            var materialLoaderType = AppDomain.CurrentDomain
                .GetAssemblies()
                .Select(assembly => assembly.GetType("LibMMD.Unity3D.MaterialLoader", false))
                .FirstOrDefault(type => type != null);
            Assert.That(materialLoaderType, Is.Not.Null, "LibMMD.Unity3D.MaterialLoader type not found.");

            var method = materialLoaderType.GetMethod(
                "ResolveEdgeContributionCap",
                BindingFlags.NonPublic | BindingFlags.Static);
            Assert.That(method, Is.Not.Null, "ResolveEdgeContributionCap not found.");
            return (float)method.Invoke(null, new object[] { material });
        }
    }
}
