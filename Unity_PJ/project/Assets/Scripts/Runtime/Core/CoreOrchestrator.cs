using System;
using System.Collections.Generic;
using MascotDesktop.Runtime.Diagnostics;
using UnityEngine;

namespace MascotDesktop.Runtime.Core
{
    [DefaultExecutionOrder(-2100)]
    public sealed class CoreOrchestrator : MonoBehaviour
    {
        private enum CoreFeature
        {
            Core,
            Llm,
            Tts,
            Stt
        }

        private sealed class ScheduledReminder
        {
            public string RequestId;
            public string Message;
            public DateTimeOffset DueAtUtc;
        }

        private sealed class FeatureHealthState
        {
            public bool IsDegraded;
            public int ConsecutiveSuccessCount;
            public string LastErrorCode = string.Empty;
            public string LastRequestId = string.Empty;
        }

        private readonly List<ScheduledReminder> _scheduledReminders = new List<ScheduledReminder>();
        private readonly Dictionary<CoreFeature, FeatureHealthState> _featureHealthStates = new Dictionary<CoreFeature, FeatureHealthState>();
        private const int DegradedRecoverySuccessThreshold = 3;

        [SerializeField] private string defaultAvatarState = "idle";
        [SerializeField] private float reminderPollIntervalSeconds = 0.25f;

        private float _nextReminderPollAt;

        public event Action<string, string> AvatarStateChanged;
        public event Action<string, string> MotionSlotRequested;
        public event Action<string, string> ReminderTriggered;

        public string CurrentAvatarState { get; private set; } = "idle";
        public bool IsAnyFeatureDegraded { get; private set; }
        public string DegradedSummary { get; private set; } = "none";

        private void Awake()
        {
            CurrentAvatarState = NormalizeState(defaultAvatarState);
            EnsureFeatureStates();
        }

        private void Update()
        {
            if (Time.unscaledTime < _nextReminderPollAt)
            {
                return;
            }

            _nextReminderPollAt = Time.unscaledTime + Mathf.Max(0.05f, reminderPollIntervalSeconds);
            ProcessDueReminders(DateTimeOffset.UtcNow);
        }

        public string SendChat(string text, string requestId = null)
        {
            var rid = string.IsNullOrWhiteSpace(requestId) ? RuntimeLog.NewRequestId() : requestId;
            var message = text ?? string.Empty;

            RuntimeLog.Info(
                "core",
                "core.chat.received",
                rid,
                message,
                string.Empty,
                "core");

            var derivedState = DetermineStateFromChat(message);
            ApplyAvatarState(derivedState, rid);
            return rid;
        }

        /// <summary>
        /// Phase A (LLM Integration): bridge result を受け取り、state遷移とログ記録を行う。
        /// bridge成功時はtextからstateを解析して遷移（将来はCore応答payloadを参照予定）。
        /// bridge失敗時はローカルルールでfallbackし、error_code/retryableをログに記録する。
        /// </summary>
        public string SendChatWithBridgeResult(
            string text,
            string requestId,
            bool bridgeSucceeded,
            string bridgeErrorCode,
            bool bridgeRetryable,
            string bridgeErrorName = "",
            string coreRequestId = "",
            int attempt = 0)
        {
            var rid = string.IsNullOrWhiteSpace(requestId) ? RuntimeLog.NewRequestId() : requestId;
            var message = text ?? string.Empty;
            var errorCode = bridgeErrorCode ?? string.Empty;
            var errorName = bridgeErrorName ?? string.Empty;
            var coreRid = coreRequestId ?? string.Empty;

            if (bridgeSucceeded)
            {
                UpdateFeatureHealth(CoreFeature.Llm, true, rid, string.Empty);
                RuntimeLog.Info(
                    "core",
                    "core.chat.bridge_success",
                    rid,
                    $"bridge chat succeeded; text_length={message.Length}; core_request_id={coreRid}; attempt={attempt}",
                    "/v1/chat/send",
                    "core");

                var derivedState = DetermineStateFromChat(message);
                ApplyAvatarState(derivedState, rid);
            }
            else
            {
                UpdateFeatureHealth(CoreFeature.Llm, false, rid, errorCode);
                RuntimeLog.Warn(
                    "core",
                    "core.chat.bridge_fallback",
                    rid,
                    string.IsNullOrWhiteSpace(errorCode) ? errorName : errorCode,
                    $"bridge chat failed (retryable={bridgeRetryable}); fallback to local rule; core_request_id={coreRid}; attempt={attempt}",
                    "/v1/chat/send",
                    "core");

                var derivedState = DetermineStateFromChat(message);
                ApplyAvatarState(derivedState, rid);
            }

            return rid;
        }

        /// <summary>
        /// Phase B (TTS Integration): bridge result を受け取り、再生可否に応じた motion を適用する。
        /// 成功時は再生モーション、失敗時は fallback モーションを要求し、
        /// error_code/retryable をログに記録する。
        /// </summary>
        public string SendTtsWithBridgeResult(
            string text,
            string requestId,
            bool bridgeSucceeded,
            string bridgeErrorCode,
            bool bridgeRetryable,
            string bridgeErrorName = "",
            string coreRequestId = "",
            int attempt = 0)
        {
            var rid = string.IsNullOrWhiteSpace(requestId) ? RuntimeLog.NewRequestId() : requestId;
            var message = text ?? string.Empty;
            var errorCode = bridgeErrorCode ?? string.Empty;
            var errorName = bridgeErrorName ?? string.Empty;
            var coreRid = coreRequestId ?? string.Empty;

            if (bridgeSucceeded)
            {
                UpdateFeatureHealth(CoreFeature.Tts, true, rid, string.Empty);
                RuntimeLog.Info(
                    "core",
                    "core.tts.bridge_success",
                    rid,
                    $"bridge tts succeeded; text_length={message.Length}; core_request_id={coreRid}; attempt={attempt}",
                    "/v1/tts/play",
                    "core");

                RequestMotionSlot("wave", rid);
            }
            else
            {
                UpdateFeatureHealth(CoreFeature.Tts, false, rid, errorCode);
                RuntimeLog.Warn(
                    "core",
                    "core.tts.bridge_fallback",
                    rid,
                    string.IsNullOrWhiteSpace(errorCode) ? errorName : errorCode,
                    $"bridge tts failed (retryable={bridgeRetryable}); fallback motion; core_request_id={coreRid}; attempt={attempt}",
                    "/v1/tts/play",
                    "core");

                RequestMotionSlot("idle", rid);
            }

            return rid;
        }

        /// <summary>
        /// Phase C (STT Integration): bridge result を受け取り、partial/final を区別して処理する。
        /// partial は状態遷移させず、final のみ chat 処理へ連結する。
        /// 空finalは誤認識として無視し、状態破綻を回避する。
        /// </summary>
        public string SendSttWithBridgeResult(
            string text,
            string requestId,
            bool isFinal,
            bool bridgeSucceeded,
            string bridgeErrorCode,
            bool bridgeRetryable,
            string bridgeErrorName = "",
            string coreRequestId = "",
            int attempt = 0)
        {
            var rid = string.IsNullOrWhiteSpace(requestId) ? RuntimeLog.NewRequestId() : requestId;
            var transcript = text ?? string.Empty;
            var errorCode = bridgeErrorCode ?? string.Empty;
            var errorName = bridgeErrorName ?? string.Empty;
            var coreRid = coreRequestId ?? string.Empty;

            if (bridgeSucceeded)
            {
                UpdateFeatureHealth(CoreFeature.Stt, true, rid, string.Empty);
                RuntimeLog.Info(
                    "core",
                    "core.stt.bridge_success",
                    rid,
                    $"bridge stt event received; is_final={isFinal}; text_length={transcript.Length}; core_request_id={coreRid}; attempt={attempt}",
                    "/v1/stt/event",
                    "core");
            }
            else
            {
                UpdateFeatureHealth(CoreFeature.Stt, false, rid, errorCode);
                RuntimeLog.Warn(
                    "core",
                    "core.stt.bridge_fallback",
                    rid,
                    string.IsNullOrWhiteSpace(errorCode) ? errorName : errorCode,
                    $"bridge stt failed (retryable={bridgeRetryable}); is_final={isFinal}; core_request_id={coreRid}; attempt={attempt}",
                    "/v1/stt/event",
                    "core");
            }

            if (!isFinal)
            {
                RuntimeLog.Info(
                    "core",
                    "core.stt.partial_accepted",
                    rid,
                    "partial transcript accepted",
                    "/v1/stt/event",
                    "core");
                return rid;
            }

            if (string.IsNullOrWhiteSpace(transcript))
            {
                RuntimeLog.Warn(
                    "core",
                    "core.stt.final_ignored",
                    rid,
                    "CORE.STT.EMPTY_FINAL",
                    "empty final transcript ignored",
                    "/v1/stt/event",
                    "core");
                return rid;
            }

            SendChatWithBridgeResult(transcript, rid, bridgeSucceeded, errorCode, bridgeRetryable);
            return rid;
        }

        public void ApplyAvatarState(string state, string requestId = null)
        {
            var rid = string.IsNullOrWhiteSpace(requestId) ? RuntimeLog.NewRequestId() : requestId;
            var normalized = NormalizeState(state);
            CurrentAvatarState = normalized;

            RuntimeLog.Info(
                "core",
                "core.avatar.state_applied",
                rid,
                "avatar state updated",
                normalized,
                "core");

            AvatarStateChanged?.Invoke(rid, normalized);
        }

        public void RequestMotionSlot(string slotName, string requestId = null)
        {
            var rid = string.IsNullOrWhiteSpace(requestId) ? RuntimeLog.NewRequestId() : requestId;
            var normalizedSlot = NormalizeSlot(slotName);

            RuntimeLog.Info(
                "core",
                "core.avatar.motion_requested",
                rid,
                "motion slot requested",
                normalizedSlot,
                "core");

            MotionSlotRequested?.Invoke(rid, normalizedSlot);
        }

        public string ScheduleReminder(string message, double delaySeconds, string requestId = null)
        {
            var rid = string.IsNullOrWhiteSpace(requestId) ? RuntimeLog.NewRequestId() : requestId;
            var dueAt = DateTimeOffset.UtcNow.AddSeconds(Math.Max(0.0, delaySeconds));
            _scheduledReminders.Add(
                new ScheduledReminder
                {
                    RequestId = rid,
                    Message = message ?? string.Empty,
                    DueAtUtc = dueAt
                });

            RuntimeLog.Info(
                "core",
                "core.reminder.scheduled",
                rid,
                $"reminder scheduled for {dueAt:O}",
                string.Empty,
                "core");

            return rid;
        }

        public void ProcessDueRemindersForTests(DateTimeOffset nowUtc)
        {
            ProcessDueReminders(nowUtc);
        }

        public void ReportHealthStatus(
            bool coreUp,
            bool llmUp,
            bool ttsUp,
            bool sttUp,
            string requestId = null,
            string errorCode = "")
        {
            var rid = string.IsNullOrWhiteSpace(requestId) ? RuntimeLog.NewRequestId() : requestId;
            UpdateFeatureHealth(CoreFeature.Core, coreUp, rid, errorCode);
            UpdateFeatureHealth(CoreFeature.Llm, llmUp, rid, errorCode);
            UpdateFeatureHealth(CoreFeature.Tts, ttsUp, rid, errorCode);
            UpdateFeatureHealth(CoreFeature.Stt, sttUp, rid, errorCode);
        }

        public bool IsFeatureDegraded(string featureName)
        {
            if (!TryResolveFeature(featureName, out var feature))
            {
                return false;
            }

            EnsureFeatureStates();
            return _featureHealthStates[feature].IsDegraded;
        }

        private void ProcessDueReminders(DateTimeOffset nowUtc)
        {
            for (var i = _scheduledReminders.Count - 1; i >= 0; i--)
            {
                var item = _scheduledReminders[i];
                if (item.DueAtUtc > nowUtc)
                {
                    continue;
                }

                RuntimeLog.Info(
                    "core",
                    "core.reminder.triggered",
                    item.RequestId,
                    item.Message,
                    string.Empty,
                    "core");

                ReminderTriggered?.Invoke(item.RequestId, item.Message);
                _scheduledReminders.RemoveAt(i);
            }
        }

        private static string DetermineStateFromChat(string text)
        {
            var normalized = (text ?? string.Empty).Trim().ToLowerInvariant();
            if (normalized.Contains("happy"))
            {
                return "happy";
            }

            if (normalized.Contains("sad"))
            {
                return "sad";
            }

            if (normalized.Contains("sleep") || normalized.Contains("tired"))
            {
                return "sleepy";
            }

            return "idle";
        }

        private static string NormalizeState(string state)
        {
            var normalized = (state ?? string.Empty).Trim().ToLowerInvariant();
            return string.IsNullOrWhiteSpace(normalized) ? "idle" : normalized;
        }

        private static string NormalizeSlot(string slotName)
        {
            var normalized = (slotName ?? string.Empty).Trim().ToLowerInvariant();
            return string.IsNullOrWhiteSpace(normalized) ? "idle" : normalized;
        }

        private void EnsureFeatureStates()
        {
            EnsureFeatureState(CoreFeature.Core);
            EnsureFeatureState(CoreFeature.Llm);
            EnsureFeatureState(CoreFeature.Tts);
            EnsureFeatureState(CoreFeature.Stt);
            RebuildDegradedSummary();
        }

        private void EnsureFeatureState(CoreFeature feature)
        {
            if (_featureHealthStates.ContainsKey(feature))
            {
                return;
            }

            _featureHealthStates[feature] = new FeatureHealthState();
        }

        private void UpdateFeatureHealth(CoreFeature feature, bool success, string requestId, string errorCode)
        {
            EnsureFeatureState(feature);
            var state = _featureHealthStates[feature];

            if (success)
            {
                state.ConsecutiveSuccessCount++;
                if (state.IsDegraded && state.ConsecutiveSuccessCount >= DegradedRecoverySuccessThreshold)
                {
                    state.IsDegraded = false;
                    state.LastErrorCode = string.Empty;
                    RuntimeLog.Info(
                        "core",
                        "core.degraded_exit",
                        requestId,
                        $"feature recovered: {feature.ToString().ToLowerInvariant()}",
                        feature.ToString().ToLowerInvariant(),
                        "core");
                }
            }
            else
            {
                state.ConsecutiveSuccessCount = 0;
                state.LastErrorCode = errorCode ?? string.Empty;
                if (!state.IsDegraded)
                {
                    state.IsDegraded = true;
                    RuntimeLog.Info(
                        "core",
                        "core.degraded_enter",
                        requestId,
                        $"feature degraded: {feature.ToString().ToLowerInvariant()}",
                        feature.ToString().ToLowerInvariant(),
                        "core");
                }
            }

            state.LastRequestId = requestId ?? string.Empty;
            RebuildDegradedSummary();
        }

        private void RebuildDegradedSummary()
        {
            var degradedFeatures = new List<string>();
            foreach (var item in _featureHealthStates)
            {
                if (item.Value.IsDegraded)
                {
                    degradedFeatures.Add(item.Key.ToString().ToLowerInvariant());
                }
            }

            IsAnyFeatureDegraded = degradedFeatures.Count > 0;
            DegradedSummary = degradedFeatures.Count == 0
                ? "none"
                : string.Join(",", degradedFeatures);
        }

        private static bool TryResolveFeature(string featureName, out CoreFeature feature)
        {
            var normalized = (featureName ?? string.Empty).Trim().ToLowerInvariant();
            switch (normalized)
            {
                case "core":
                    feature = CoreFeature.Core;
                    return true;
                case "llm":
                case "chat":
                    feature = CoreFeature.Llm;
                    return true;
                case "tts":
                    feature = CoreFeature.Tts;
                    return true;
                case "stt":
                    feature = CoreFeature.Stt;
                    return true;
                default:
                    feature = CoreFeature.Core;
                    return false;
            }
        }
    }
}
