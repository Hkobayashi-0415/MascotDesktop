using System;
using System.Collections.Generic;
using MascotDesktop.Runtime.Diagnostics;
using UnityEngine;

namespace MascotDesktop.Runtime.Core
{
    [DefaultExecutionOrder(-2100)]
    public sealed class CoreOrchestrator : MonoBehaviour
    {
        private sealed class ScheduledReminder
        {
            public string RequestId;
            public string Message;
            public DateTimeOffset DueAtUtc;
        }

        private readonly List<ScheduledReminder> _scheduledReminders = new List<ScheduledReminder>();

        [SerializeField] private string defaultAvatarState = "idle";
        [SerializeField] private float reminderPollIntervalSeconds = 0.25f;

        private float _nextReminderPollAt;

        public event Action<string, string> AvatarStateChanged;
        public event Action<string, string> MotionSlotRequested;
        public event Action<string, string> ReminderTriggered;

        public string CurrentAvatarState { get; private set; } = "idle";

        private void Awake()
        {
            CurrentAvatarState = NormalizeState(defaultAvatarState);
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
    }
}
