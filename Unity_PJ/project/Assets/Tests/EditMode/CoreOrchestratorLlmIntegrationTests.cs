using MascotDesktop.Runtime.Core;
using MascotDesktop.Runtime.Diagnostics;
using NUnit.Framework;
using System.Linq;
using UnityEngine;

namespace MascotDesktop.Tests.EditMode
{
    /// <summary>
    /// U5-T4 Phase A: CoreOrchestrator LLM統合のEditModeテスト。
    /// bridge成功/失敗/ログ記録の3ケースを検証する。
    /// </summary>
    public sealed class CoreOrchestratorLlmIntegrationTests
    {
        private GameObject _gameObject;
        private CoreOrchestrator _orchestrator;

        [SetUp]
        public void SetUp()
        {
            _gameObject = new GameObject("CoreOrchestratorLlmIntegrationTests");
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

        /// <summary>
        /// bridge成功時: テキストから解析したstateへ遷移することを確認する。
        /// </summary>
        [Test]
        public void SendChatWithBridgeResult_BridgeSuccess_AppliesStateDerivedFromText()
        {
            const string requestId = "req-llm-success-001";

            _orchestrator.SendChatWithBridgeResult(
                "I am happy",
                requestId,
                bridgeSucceeded: true,
                bridgeErrorCode: string.Empty,
                bridgeRetryable: false);

            Assert.That(_orchestrator.CurrentAvatarState, Is.EqualTo("happy"),
                "bridge成功時はtext解析によるstateへ遷移すること");
        }

        /// <summary>
        /// bridge失敗時: ローカルルールfallbackでstateへ遷移することを確認する。
        /// </summary>
        [Test]
        public void SendChatWithBridgeResult_BridgeFailed_FallsBackToLocalRule()
        {
            UnityEngine.TestTools.LogAssert.Expect(
                LogType.Warning,
                new System.Text.RegularExpressions.Regex(".*core\\.chat\\.bridge_fallback.*"));

            const string requestId = "req-llm-fallback-002";

            _orchestrator.SendChatWithBridgeResult(
                "I am happy",
                requestId,
                bridgeSucceeded: false,
                bridgeErrorCode: "IPC.HTTP.REQUEST_FAILED",
                bridgeRetryable: true);

            Assert.That(_orchestrator.CurrentAvatarState, Is.EqualTo("happy"),
                "bridge失敗時のfallbackでローカルルール（happy->happy遷移）が動作すること");
        }

        /// <summary>
        /// request_id / error_code / retryable のログ記録を確認する。
        /// bridge失敗時にcore.chat.bridge_fallbackイベントが記録されていること。
        /// </summary>
        [Test]
        public void SendChatWithBridgeResult_RecordsRequestIdErrorCodeRetryable_ViaRuntimeLog()
        {
            UnityEngine.TestTools.LogAssert.Expect(
                LogType.Warning,
                new System.Text.RegularExpressions.Regex(".*core\\.chat\\.bridge_fallback.*"));

            const string requestId = "req-llm-log-003";
            const string errorCode = "CORE.TIMEOUT";

            RuntimeLog.ClearRecentEntries();

            _orchestrator.SendChatWithBridgeResult(
                "hello",
                requestId,
                bridgeSucceeded: false,
                bridgeErrorCode: errorCode,
                bridgeRetryable: true);

            RuntimeLog.Flush(2000);

            var entries = RuntimeLog.SnapshotRecentEntries(64);

            // core.chat.bridge_fallback エントリが存在すること
            var fallbackEntry = entries.FirstOrDefault(e =>
                e.event_name == "core.chat.bridge_fallback" &&
                e.request_id == requestId);

            Assert.That(fallbackEntry, Is.Not.Null,
                "core.chat.bridge_fallback イベントがRuntimeLogに記録されること");
            Assert.That(fallbackEntry.error_code, Is.EqualTo(errorCode),
                "error_codeが記録されること");
        }
        /// <summary>
        /// [Low 強化] bridge成功時: AvatarStateChanged イベントの rid が入力 requestId と一致することを確認する。
        /// </summary>
        [Test]
        public void SendChatWithBridgeResult_BridgeSuccess_AvatarStateChanged_RidMatchesRequestId()
        {
            const string requestId = "req-llm-rid-success-004";

            string capturedRid = null;
            _orchestrator.AvatarStateChanged += (rid, _) => capturedRid = rid;

            _orchestrator.SendChatWithBridgeResult(
                "I am happy",
                requestId,
                bridgeSucceeded: true,
                bridgeErrorCode: string.Empty,
                bridgeRetryable: false);

            Assert.That(capturedRid, Is.EqualTo(requestId),
                "bridge成功時にAvatarStateChangedで発行されるridが入力requestIdと一致すること");
        }

        /// <summary>
        /// [Low 強化] bridge失敗時: AvatarStateChanged イベントの rid が入力 requestId と一致し、
        /// かつ core.chat.bridge_fallback が記録されることを確認する。
        /// </summary>
        [Test]
        public void SendChatWithBridgeResult_BridgeFailed_AvatarStateChanged_RidMatchesRequestId_AndFallbackLogged()
        {
            UnityEngine.TestTools.LogAssert.Expect(
                LogType.Warning,
                new System.Text.RegularExpressions.Regex(".*core\\.chat\\.bridge_fallback.*"));

            const string requestId = "req-llm-rid-fallback-005";
            const string errorCode = "IPC.HTTP.REQUEST_FAILED";

            string capturedRid = null;
            _orchestrator.AvatarStateChanged += (rid, _) => capturedRid = rid;

            RuntimeLog.ClearRecentEntries();

            _orchestrator.SendChatWithBridgeResult(
                "I am happy",
                requestId,
                bridgeSucceeded: false,
                bridgeErrorCode: errorCode,
                bridgeRetryable: true);

            // AvatarStateChanged の rid が requestId と一致すること
            Assert.That(capturedRid, Is.EqualTo(requestId),
                "bridge失敗時のfallbackでもAvatarStateChangedのridが入力requestIdと一致すること");

            // core.chat.bridge_fallback が記録されていること
            RuntimeLog.Flush(2000);
            var entries = RuntimeLog.SnapshotRecentEntries(64);
            var fallbackEntry = entries.FirstOrDefault(e =>
                e.event_name == "core.chat.bridge_fallback" &&
                e.request_id == requestId);

            Assert.That(fallbackEntry, Is.Not.Null,
                "bridge失敗時にcore.chat.bridge_fallbackイベントが記録されること");
        }
    }
}
