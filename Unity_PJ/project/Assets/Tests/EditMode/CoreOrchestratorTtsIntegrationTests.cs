using System.Linq;
using MascotDesktop.Runtime.Core;
using MascotDesktop.Runtime.Diagnostics;
using NUnit.Framework;
using UnityEngine;

namespace MascotDesktop.Tests.EditMode
{
    /// <summary>
    /// U5-T4 Phase B: CoreOrchestrator TTS統合のEditModeテスト。
    /// bridge成功/失敗（retryable=true/false）の3ケースを検証する。
    /// </summary>
    public sealed class CoreOrchestratorTtsIntegrationTests
    {
        private GameObject _gameObject;
        private CoreOrchestrator _orchestrator;

        [SetUp]
        public void SetUp()
        {
            _gameObject = new GameObject("CoreOrchestratorTtsIntegrationTests");
            _orchestrator = _gameObject.AddComponent<CoreOrchestrator>();
            RuntimeLog.ClearRecentEntries();
        }

        [TearDown]
        public void TearDown()
        {
            RuntimeLog.ClearRecentEntries();
            if (_gameObject != null)
            {
                Object.DestroyImmediate(_gameObject);
            }
        }

        [Test]
        public void SendTtsWithBridgeResult_BridgeSuccess_RequestsWaveMotionWithSameRequestId()
        {
            const string requestId = "req-tts-success-001";
            string capturedRid = null;
            string capturedSlot = null;
            _orchestrator.MotionSlotRequested += (rid, slot) =>
            {
                capturedRid = rid;
                capturedSlot = slot;
            };

            var returnedRid = _orchestrator.SendTtsWithBridgeResult(
                "hello",
                requestId,
                bridgeSucceeded: true,
                bridgeErrorCode: string.Empty,
                bridgeRetryable: false);

            Assert.That(returnedRid, Is.EqualTo(requestId));
            Assert.That(capturedRid, Is.EqualTo(requestId));
            Assert.That(capturedSlot, Is.EqualTo("wave"));
        }

        [Test]
        public void SendTtsWithBridgeResult_BridgeFailedRetryableTrue_UsesIdleFallbackAndRecordsRetryableTrue()
        {
            UnityEngine.TestTools.LogAssert.Expect(
                LogType.Warning,
                new System.Text.RegularExpressions.Regex(".*core\\.tts\\.bridge_fallback.*"));

            const string requestId = "req-tts-fallback-002";
            const string errorCode = "CORE.TTS.UNAVAILABLE";
            string capturedRid = null;
            string capturedSlot = null;
            _orchestrator.MotionSlotRequested += (rid, slot) =>
            {
                capturedRid = rid;
                capturedSlot = slot;
            };

            RuntimeLog.ClearRecentEntries();
            var returnedRid = _orchestrator.SendTtsWithBridgeResult(
                "hello",
                requestId,
                bridgeSucceeded: false,
                bridgeErrorCode: errorCode,
                bridgeRetryable: true);

            Assert.That(returnedRid, Is.EqualTo(requestId));
            Assert.That(capturedRid, Is.EqualTo(requestId));
            Assert.That(capturedSlot, Is.EqualTo("idle"));

            RuntimeLog.Flush(2000);
            var fallbackEntry = RuntimeLog.SnapshotRecentEntries(64).FirstOrDefault(e =>
                e.event_name == "core.tts.bridge_fallback" &&
                e.request_id == requestId);

            Assert.That(fallbackEntry, Is.Not.Null);
            Assert.That(fallbackEntry.error_code, Is.EqualTo(errorCode));
            Assert.That(fallbackEntry.message, Does.Contain("retryable=True"));
        }

        [Test]
        public void SendTtsWithBridgeResult_BridgeFailedRetryableFalse_UsesIdleFallbackAndRecordsRetryableFalse()
        {
            UnityEngine.TestTools.LogAssert.Expect(
                LogType.Warning,
                new System.Text.RegularExpressions.Regex(".*core\\.tts\\.bridge_fallback.*"));

            const string requestId = "req-tts-fallback-003";
            const string errorCode = "CORE.TTS.NON_RETRYABLE";
            string capturedRid = null;
            string capturedSlot = null;
            _orchestrator.MotionSlotRequested += (rid, slot) =>
            {
                capturedRid = rid;
                capturedSlot = slot;
            };

            RuntimeLog.ClearRecentEntries();
            var returnedRid = _orchestrator.SendTtsWithBridgeResult(
                "hello",
                requestId,
                bridgeSucceeded: false,
                bridgeErrorCode: errorCode,
                bridgeRetryable: false);

            Assert.That(returnedRid, Is.EqualTo(requestId));
            Assert.That(capturedRid, Is.EqualTo(requestId));
            Assert.That(capturedSlot, Is.EqualTo("idle"));

            RuntimeLog.Flush(2000);
            var fallbackEntry = RuntimeLog.SnapshotRecentEntries(64).FirstOrDefault(e =>
                e.event_name == "core.tts.bridge_fallback" &&
                e.request_id == requestId);

            Assert.That(fallbackEntry, Is.Not.Null);
            Assert.That(fallbackEntry.error_code, Is.EqualTo(errorCode));
            Assert.That(fallbackEntry.message, Does.Contain("retryable=False"));
        }
    }
}
