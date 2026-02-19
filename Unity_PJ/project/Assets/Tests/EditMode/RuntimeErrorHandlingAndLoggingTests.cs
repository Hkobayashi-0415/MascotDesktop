using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MascotDesktop.Runtime.Avatar;
using MascotDesktop.Runtime.Diagnostics;
using NUnit.Framework;
using UnityEngine;

namespace MascotDesktop.Tests.EditMode
{
    public sealed class RuntimeErrorHandlingAndLoggingTests
    {
        private static readonly string[] ManagedEnvVars =
        {
            "MASCOTDESKTOP_RUNTIMELOG_MIN_LEVEL",
            "MASCOTDESKTOP_RUNTIMELOG_INCLUDE_COMPONENTS",
            "MASCOTDESKTOP_RUNTIMELOG_EXCLUDE_COMPONENTS",
            "MASCOTDESKTOP_RUNTIMELOG_INCLUDE_EVENTS",
            "MASCOTDESKTOP_RUNTIMELOG_EXCLUDE_EVENTS",
            "MASCOTDESKTOP_RUNTIMELOG_MAX_FILE_BYTES",
            "MASCOTDESKTOP_RUNTIMELOG_MAX_FILES",
            "MASCOTDESKTOP_RUNTIMELOG_FLUSH_INTERVAL_MS",
            "MASCOTDESKTOP_RUNTIMELOG_MAX_BATCH_SIZE",
            "MASCOTDESKTOP_RUNTIMELOG_MAX_QUEUE_SIZE",
            "MASCOTDESKTOP_RUNTIMELOG_FILE_PREFIX"
        };

        private readonly Dictionary<string, string> _envBackup = new Dictionary<string, string>(StringComparer.Ordinal);
        private readonly List<string> _tempFiles = new List<string>();
        private string _logFilePrefix;

        [SetUp]
        public void SetUp()
        {
            foreach (var env in ManagedEnvVars)
            {
                _envBackup[env] = Environment.GetEnvironmentVariable(env);
                Environment.SetEnvironmentVariable(env, null);
            }

            _logFilePrefix = $"runtime-test-{Guid.NewGuid():N}";
            Environment.SetEnvironmentVariable("MASCOTDESKTOP_RUNTIMELOG_FILE_PREFIX", _logFilePrefix);

            ReflectionModelLoaders.ResetTestingOverrides();
            RuntimeLog.ClearRecentEntries();
        }

        [TearDown]
        public void TearDown()
        {
            RuntimeLog.Flush(2000);
            ReflectionModelLoaders.ResetTestingOverrides();
            RuntimeLog.ClearRecentEntries();

            foreach (var path in _tempFiles)
            {
                TryDeleteFile(path);
            }

            TryDeleteTestLogFiles(_logFilePrefix);
            foreach (var pair in _envBackup)
            {
                Environment.SetEnvironmentVariable(pair.Key, pair.Value);
            }
        }

        [Test]
        public void LoaderFailureClassification_ImageFileNotFound_ReturnsExpectedDetail()
        {
            UnityEngine.TestTools.LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex(".*ASSET.READ.FILE_NOT_FOUND.*"));
            const string requestId = "req-test-file-not-found";
            var missingPath = Path.Combine(Path.GetTempPath(), $"missing-{Guid.NewGuid():N}.png");

            var result = ReflectionModelLoaders.TryLoadImageTexture(missingPath, requestId, "unit_test");

            Assert.That(result.Success, Is.False);
            Assert.That(result.ErrorCode, Is.EqualTo("ASSET.READ.FILE_NOT_FOUND"));
            Assert.That(result.Error, Is.Not.Null);
            Assert.That(result.Error.Stage, Is.EqualTo("classify"));
            Assert.That(result.Error.Cause, Is.EqualTo("file_not_found"));
            Assert.That(result.Error.Path, Is.EqualTo(missingPath));
            Assert.That(result.Error.SourceTier, Is.EqualTo("unit_test"));
        }

        [Test]
        public void LoaderFailureClassification_ImageDecodeFail_ReturnsExpectedDetail()
        {
            UnityEngine.TestTools.LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex(".*ASSET.READ.DECODE_FAILED.*"));
            const string requestId = "req-test-decode-fail";
            var invalidPng = CreateTempFile(".png", new byte[] { 0x00, 0x01, 0x02, 0x03 });

            var result = ReflectionModelLoaders.TryLoadImageTexture(invalidPng, requestId, "unit_test");

            Assert.That(result.Success, Is.False);
            Assert.That(result.ErrorCode, Is.EqualTo("ASSET.READ.DECODE_FAILED"));
            Assert.That(result.Error, Is.Not.Null);
            Assert.That(result.Error.Stage, Is.EqualTo("post-normalize").Or.EqualTo("reflection_invoke"));
            Assert.That(result.Error.Cause, Does.Contain("decode"));
        }

        [Test]
        public void LoaderFailureClassification_InvokeFail_ReturnsExpectedDetail()
        {
            UnityEngine.TestTools.LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex(".*AVATAR.PMX.LOADER_NOT_FOUND.*"));
            UnityEngine.TestTools.LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex(".*AVATAR.PMX.INVOKE_FAILED.*"));
            const string requestId = "req-test-invoke-fail";
            var pmxPath = CreateTempFile(".pmx", new byte[] { 0x50, 0x4D, 0x58 });

            ReflectionModelLoaders.SetTypeResolverOverrideForTests(_ => null);
            ReflectionModelLoaders.SetPmxFallbackTypesOverrideForTests(() => new[] { typeof(ThrowingPmxFallbackLoader) });

            var result = ReflectionModelLoaders.TryLoadPmx(pmxPath, requestId, "unit_test");

            Assert.That(result.Success, Is.False);
            Assert.That(result.ErrorCode, Is.EqualTo("AVATAR.PMX.INVOKE_FAILED"));
            Assert.That(result.Error, Is.Not.Null);
            Assert.That(result.Error.Stage, Is.EqualTo("reflection_invoke"));
            Assert.That(result.Error.ExceptionType, Does.Contain(nameof(InvalidOperationException)));
        }

        [Test]
        public void LoaderFailureClassification_NullRoot_ReturnsExpectedDetail()
        {
            UnityEngine.TestTools.LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex(".*AVATAR.PMX.LOADER_NOT_FOUND.*"));
            UnityEngine.TestTools.LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex(".*AVATAR.PMX.ROOT_NOT_FOUND.*"));
            const string requestId = "req-test-null-root";
            var pmxPath = CreateTempFile(".pmx", new byte[] { 0x50, 0x4D, 0x58, 0x00 });

            ReflectionModelLoaders.SetTypeResolverOverrideForTests(_ => null);
            ReflectionModelLoaders.SetPmxFallbackTypesOverrideForTests(() => new[] { typeof(NullRootPmxFallbackLoader) });

            var result = ReflectionModelLoaders.TryLoadPmx(pmxPath, requestId, "unit_test");

            Assert.That(result.Success, Is.False);
            Assert.That(result.ErrorCode, Is.EqualTo("AVATAR.PMX.ROOT_NOT_FOUND"));
            Assert.That(result.Error, Is.Not.Null);
            Assert.That(result.Error.Stage, Is.EqualTo("post-normalize"));
            Assert.That(result.Error.Cause, Is.EqualTo("fallback_root_null"));
        }

        [Test]
        public void RequestIdPropagation_UsesSingleRequestIdAcrossLoaderStageLogs()
        {
            UnityEngine.TestTools.LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex(".*ASSET.READ.DECODE_FAILED.*"));
            const string requestId = "req-test-propagation";
            Environment.SetEnvironmentVariable("MASCOTDESKTOP_RUNTIMELOG_INCLUDE_EVENTS", "avatar.loader.stage");
            Environment.SetEnvironmentVariable("MASCOTDESKTOP_RUNTIMELOG_MIN_LEVEL", "INFO");

            var invalidPng = CreateTempFile(".png", new byte[] { 0x00, 0x00, 0x00, 0x00 });
            ReflectionModelLoaders.TryLoadImageTexture(invalidPng, requestId, "unit_test");
            RuntimeLog.Flush(2000);

            var entries = RuntimeLog.SnapshotRecentEntries(256)
                .Where(entry => entry.event_name.StartsWith("avatar.loader.stage", StringComparison.Ordinal))
                .ToArray();

            Assert.That(entries.Length, Is.GreaterThan(0));
            Assert.That(entries.All(entry => entry.request_id == requestId), Is.True);
            Assert.That(entries.Any(entry => entry.event_name == "avatar.loader.stage.begin"), Is.True);
            Assert.That(entries.Any(entry => entry.event_name == "avatar.loader.stage.fail"), Is.True);
        }

        [Test]
        public void RuntimeLogPolicy_MinLevelAndFilters_AreApplied()
        {
            Environment.SetEnvironmentVariable("MASCOTDESKTOP_RUNTIMELOG_MIN_LEVEL", "WARN");
            Environment.SetEnvironmentVariable("MASCOTDESKTOP_RUNTIMELOG_INCLUDE_COMPONENTS", "policy.test");
            Environment.SetEnvironmentVariable("MASCOTDESKTOP_RUNTIMELOG_INCLUDE_EVENTS", "policy.event");
            RuntimeLog.ClearRecentEntries();

            RuntimeLog.Info("policy.test", "policy.event", "req-policy", "suppressed info");
            RuntimeLog.Warn("other.component", "policy.event", "req-policy", message: "suppressed by component filter");
            RuntimeLog.Warn("policy.test", "policy.event", "req-policy", "POLICY.WARN", "accepted warn");
            RuntimeLog.Flush(2000);

            var entries = RuntimeLog.SnapshotRecentEntries(64);
            Assert.That(entries.Length, Is.EqualTo(1));
            Assert.That(entries[0].level, Is.EqualTo("WARN"));
            Assert.That(entries[0].component, Is.EqualTo("policy.test"));
            Assert.That(entries[0].event_name, Is.EqualTo("policy.event"));
        }

        [Test]
        public void RuntimeLogPolicy_RotationAndRetention_AreApplied()
        {
            Environment.SetEnvironmentVariable("MASCOTDESKTOP_RUNTIMELOG_MIN_LEVEL", "INFO");
            Environment.SetEnvironmentVariable("MASCOTDESKTOP_RUNTIMELOG_INCLUDE_COMPONENTS", "rotation.test");
            Environment.SetEnvironmentVariable("MASCOTDESKTOP_RUNTIMELOG_MAX_FILE_BYTES", "220");
            Environment.SetEnvironmentVariable("MASCOTDESKTOP_RUNTIMELOG_MAX_FILES", "2");
            Environment.SetEnvironmentVariable("MASCOTDESKTOP_RUNTIMELOG_FLUSH_INTERVAL_MS", "50");
            RuntimeLog.ClearRecentEntries();

            for (var i = 0; i < 80; i++)
            {
                RuntimeLog.Info(
                    "rotation.test",
                    "rotation.event",
                    "req-rotation",
                    $"entry-{i}-{new string('x', 96)}");
            }

            RuntimeLog.Flush(5000);

            var logDir = Path.Combine(Application.persistentDataPath, "logs");
            var files = Directory.Exists(logDir)
                ? Directory.GetFiles(logDir, $"{_logFilePrefix}-*.jsonl", SearchOption.TopDirectoryOnly)
                : Array.Empty<string>();

            Assert.That(files.Length, Is.GreaterThanOrEqualTo(2));
            Assert.That(files.Length, Is.LessThanOrEqualTo(2));
        }

        private string CreateTempFile(string extension, byte[] bytes)
        {
            var path = Path.Combine(Path.GetTempPath(), $"runtime-loader-{Guid.NewGuid():N}{extension}");
            File.WriteAllBytes(path, bytes);
            _tempFiles.Add(path);
            return path;
        }

        private static void TryDeleteFile(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            {
                return;
            }

            try
            {
                File.Delete(path);
            }
            catch
            {
                // best effort cleanup
            }
        }

        private static void TryDeleteTestLogFiles(string prefix)
        {
            if (string.IsNullOrWhiteSpace(prefix))
            {
                return;
            }

            var logDir = Path.Combine(Application.persistentDataPath, "logs");
            if (!Directory.Exists(logDir))
            {
                return;
            }

            foreach (var file in Directory.GetFiles(logDir, $"{prefix}-*.jsonl", SearchOption.TopDirectoryOnly))
            {
                TryDeleteFile(file);
            }
        }

        private static class ThrowingPmxFallbackLoader
        {
            public static object LoadPmx(string path)
            {
                throw new InvalidOperationException($"intentional invoke failure: {path}");
            }
        }

        private static class NullRootPmxFallbackLoader
        {
            public static object LoadPmx(string path)
            {
                return null;
            }
        }
    }
}
