using System.Linq;
using MascotDesktop.Runtime.Core;
using MascotDesktop.Runtime.Diagnostics;
using NUnit.Framework;
using UnityEngine;

namespace MascotDesktop.Tests.EditMode
{
    /// <summary>
    /// U5-T4 Phase C: CoreOrchestrator STT統合のEditModeテスト。
    /// partial/final 区別、request_id 伝搬、retryable 記録、空final無視を検証する。
    /// </summary>
    public sealed class CoreOrchestratorSttIntegrationTests
    {
        private GameObject _gameObject;
        private CoreOrchestrator _orchestrator;

        [SetUp]
        public void SetUp()
        {
            _gameObject = new GameObject("CoreOrchestratorSttIntegrationTests");
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
        public void SendSttWithBridgeResult_Partial_DoesNotApplyState()
        {
            const string requestId = "req-stt-partial-001";
            var stateChanged = false;
            _orchestrator.AvatarStateChanged += (_, __) => stateChanged = true;

            var returnedRid = _orchestrator.SendSttWithBridgeResult(
                "i am",
                requestId,
                isFinal: false,
                bridgeSucceeded: true,
                bridgeErrorCode: string.Empty,
                bridgeRetryable: false);

            Assert.That(returnedRid, Is.EqualTo(requestId));
            Assert.That(stateChanged, Is.False);
            Assert.That(_orchestrator.CurrentAvatarState, Is.EqualTo("idle"));

            RuntimeLog.Flush(2000);
            var entries = RuntimeLog.SnapshotRecentEntries(64);
            Assert.That(entries.Any(e => e.event_name == "core.stt.partial_accepted" && e.request_id == requestId), Is.True);
        }

        [Test]
        public void SendSttWithBridgeResult_FinalSuccess_AppliesStateWithSameRequestId()
        {
            const string requestId = "req-stt-final-002";
            string capturedRid = null;
            string capturedState = null;
            _orchestrator.AvatarStateChanged += (rid, state) =>
            {
                capturedRid = rid;
                capturedState = state;
            };

            var returnedRid = _orchestrator.SendSttWithBridgeResult(
                "i am happy",
                requestId,
                isFinal: true,
                bridgeSucceeded: true,
                bridgeErrorCode: string.Empty,
                bridgeRetryable: false);

            Assert.That(returnedRid, Is.EqualTo(requestId));
            Assert.That(_orchestrator.CurrentAvatarState, Is.EqualTo("happy"));
            Assert.That(capturedRid, Is.EqualTo(requestId));
            Assert.That(capturedState, Is.EqualTo("happy"));
        }

        [Test]
        public void SendSttWithBridgeResult_FinalFailedRetryable_RecordsRetryableAndFallsBackViaChat()
        {
            UnityEngine.TestTools.LogAssert.Expect(
                LogType.Warning,
                new System.Text.RegularExpressions.Regex(".*core\\.stt\\.bridge_fallback.*"));
            UnityEngine.TestTools.LogAssert.Expect(
                LogType.Warning,
                new System.Text.RegularExpressions.Regex(".*core\\.chat\\.bridge_fallback.*"));

            const string requestId = "req-stt-fallback-003";
            const string errorCode = "CORE.STT.TIMEOUT";

            var returnedRid = _orchestrator.SendSttWithBridgeResult(
                "i am happy",
                requestId,
                isFinal: true,
                bridgeSucceeded: false,
                bridgeErrorCode: errorCode,
                bridgeRetryable: true);

            Assert.That(returnedRid, Is.EqualTo(requestId));
            Assert.That(_orchestrator.CurrentAvatarState, Is.EqualTo("happy"));

            RuntimeLog.Flush(2000);
            var entries = RuntimeLog.SnapshotRecentEntries(64);
            var sttFallback = entries.FirstOrDefault(e =>
                e.event_name == "core.stt.bridge_fallback" &&
                e.request_id == requestId);

            Assert.That(sttFallback, Is.Not.Null);
            Assert.That(sttFallback.error_code, Is.EqualTo(errorCode));
            Assert.That(sttFallback.message, Does.Contain("retryable=True"));
        }

        [Test]
        public void SendSttWithBridgeResult_EmptyFinal_IgnoredWithoutStateBreak()
        {
            UnityEngine.TestTools.LogAssert.Expect(
                LogType.Warning,
                new System.Text.RegularExpressions.Regex(".*core\\.stt\\.final_ignored.*"));

            const string requestId = "req-stt-empty-final-004";
            _orchestrator.ApplyAvatarState("happy", "req-init-happy");
            RuntimeLog.ClearRecentEntries();

            var returnedRid = _orchestrator.SendSttWithBridgeResult(
                string.Empty,
                requestId,
                isFinal: true,
                bridgeSucceeded: true,
                bridgeErrorCode: string.Empty,
                bridgeRetryable: false);

            Assert.That(returnedRid, Is.EqualTo(requestId));
            Assert.That(_orchestrator.CurrentAvatarState, Is.EqualTo("happy"));

            RuntimeLog.Flush(2000);
            var entries = RuntimeLog.SnapshotRecentEntries(64);
            Assert.That(entries.Any(e => e.event_name == "core.chat.bridge_success" && e.request_id == requestId), Is.False);
        }
    }
}
