using System;
using System.IO;
using System.Text.RegularExpressions;
using MascotDesktop.Runtime.Assets;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace MascotDesktop.Tests.EditMode
{
    public sealed class AssetPathResolverTests
    {
        private string _tempRoot;
        private string _canonicalRoot;
        private string _streamingRoot;

        [SetUp]
        public void SetUp()
        {
            _tempRoot = Path.Combine(Path.GetTempPath(), "MascotDesktop_AssetPathResolverTests_" + Guid.NewGuid().ToString("N"));
            _canonicalRoot = Path.Combine(_tempRoot, "canonical");
            _streamingRoot = Path.Combine(_tempRoot, "streaming");

            Directory.CreateDirectory(_canonicalRoot);
            Directory.CreateDirectory(_streamingRoot);

            CreateFile(_canonicalRoot, "characters/demo/state.png");
            CreateFile(_canonicalRoot, "characters/ナース/state.png");
            CreateFile(_streamingRoot, "characters/fallback/stream.png");
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_tempRoot))
            {
                Directory.Delete(_tempRoot, true);
            }
        }

        [Test]
        public void ResolveRelative_ResolvesFromCanonical()
        {
            var resolver = CreateResolver();
            var result = resolver.ResolveRelative("characters/demo/state.png", "req-test-001");

            Assert.That(result.Success, Is.True);
            Assert.That(result.SourceTier, Is.EqualTo("assets_user"));
            Assert.That(result.ResolvedPath, Does.Contain("characters"));
        }

        [Test]
        public void ResolveRelative_FallsBackToStreamingAssets()
        {
            var resolver = CreateResolver();
            var result = resolver.ResolveRelative("characters/fallback/stream.png", "req-test-002");

            Assert.That(result.Success, Is.True);
            Assert.That(result.SourceTier, Is.EqualTo("streaming_assets"));
        }

        [Test]
        public void ResolveRelative_RejectsAbsolutePath()
        {
            var resolver = CreateResolver();
            var absolute = Path.Combine(_canonicalRoot, "characters/demo/state.png");
            ExpectErrorCode("ASSET.PATH.ABSOLUTE_FORBIDDEN");

            var result = resolver.ResolveRelative(absolute, "req-test-003");

            Assert.That(result.Success, Is.False);
            Assert.That(result.ErrorCode, Is.EqualTo("ASSET.PATH.ABSOLUTE_FORBIDDEN"));
        }

        [Test]
        public void ResolveRelative_RejectsTraversal()
        {
            var resolver = CreateResolver();
            ExpectErrorCode("ASSET.PATH.TRAVERSAL_FORBIDDEN");
            var result = resolver.ResolveRelative("../secrets/token.txt", "req-test-004");

            Assert.That(result.Success, Is.False);
            Assert.That(result.ErrorCode, Is.EqualTo("ASSET.PATH.TRAVERSAL_FORBIDDEN"));
        }

        [Test]
        public void ResolveRelative_RejectsLegacyLikePath_WhenForbidden()
        {
            var resolver = CreateResolver();
            ExpectErrorCode("ASSET.PATH.LEGACY_FORBIDDEN");
            var result = resolver.ResolveRelative("workspace/data/assets_user/characters/a.png", "req-test-005");

            Assert.That(result.Success, Is.False);
            Assert.That(result.ErrorCode, Is.EqualTo("ASSET.PATH.LEGACY_FORBIDDEN"));
        }

        [Test]
        public void ResolveRelative_AddsWarningCode_ForNonAsciiSegment()
        {
            var resolver = CreateResolver();
            ExpectWarningCode("ASSET.PATH.NON_ASCII_WARN");
            var result = resolver.ResolveRelative("characters/ナース/state.png", "req-test-006");

            Assert.That(result.Success, Is.True);
            Assert.That(result.WarningCode, Is.EqualTo("ASSET.PATH.NON_ASCII_WARN"));
        }

        private AssetPathResolver CreateResolver()
        {
            return new AssetPathResolver(
                new AssetPathResolverOptions
                {
                    CanonicalAssetsRoot = _canonicalRoot,
                    StreamingAssetsRoot = _streamingRoot,
                    ForbidLegacyRoot = true,
                    WarnOnNonAscii = true
                });
        }

        private static void CreateFile(string root, string relativePath)
        {
            var fullPath = Path.Combine(root, relativePath.Replace('/', Path.DirectorySeparatorChar));
            var dir = Path.GetDirectoryName(fullPath);
            if (!string.IsNullOrWhiteSpace(dir))
            {
                Directory.CreateDirectory(dir);
            }

            File.WriteAllText(fullPath, "dummy");
        }

        private static void ExpectErrorCode(string code)
        {
            LogAssert.Expect(LogType.Error, new Regex($"\"error_code\":\"{Regex.Escape(code)}\""));
        }

        private static void ExpectWarningCode(string code)
        {
            LogAssert.Expect(LogType.Warning, new Regex($"\"error_code\":\"{Regex.Escape(code)}\""));
        }
    }
}
