using System;
using System.Collections.Generic;
using MascotDesktop.Runtime.Diagnostics;
using UnityEngine;

namespace MascotDesktop.Runtime.Avatar
{
    [Serializable]
    public sealed class MotionSlotDefinition
    {
        public string slotName;
        public string fallbackSlot;
    }

    public sealed class MotionPlayResult
    {
        public bool Success;
        public string ResolvedSlot;
        public string ErrorCode;
        public string Message;
    }

    public sealed class MotionSlotPlayer : MonoBehaviour
    {
        [SerializeField] private List<MotionSlotDefinition> configuredSlots = new List<MotionSlotDefinition>
        {
            new MotionSlotDefinition { slotName = "idle", fallbackSlot = string.Empty },
            new MotionSlotDefinition { slotName = "wave", fallbackSlot = "idle" },
            new MotionSlotDefinition { slotName = "happy", fallbackSlot = "idle" },
            new MotionSlotDefinition { slotName = "sleepy", fallbackSlot = "idle" }
        };

        private readonly Dictionary<string, MotionSlotDefinition> _slotMap = new Dictionary<string, MotionSlotDefinition>(StringComparer.OrdinalIgnoreCase);

        public string CurrentSlot { get; private set; } = "idle";

        private void Awake()
        {
            RebuildSlotMap();
        }

        public void RebuildSlotMap()
        {
            _slotMap.Clear();
            foreach (var definition in configuredSlots)
            {
                var slot = Normalize(definition.slotName);
                if (string.IsNullOrWhiteSpace(slot))
                {
                    continue;
                }

                _slotMap[slot] = new MotionSlotDefinition
                {
                    slotName = slot,
                    fallbackSlot = Normalize(definition.fallbackSlot)
                };
            }
        }

        public void RegisterSlot(string slotName, string fallbackSlot = null)
        {
            var slot = Normalize(slotName);
            if (string.IsNullOrWhiteSpace(slot))
            {
                return;
            }

            _slotMap[slot] = new MotionSlotDefinition
            {
                slotName = slot,
                fallbackSlot = Normalize(fallbackSlot)
            };
        }

        public MotionPlayResult PlaySlot(string requestedSlot, string requestId = null)
        {
            var rid = string.IsNullOrWhiteSpace(requestId) ? RuntimeLog.NewRequestId() : requestId;
            var slot = Normalize(requestedSlot);
            if (string.IsNullOrWhiteSpace(slot))
            {
                return Fail(
                    rid,
                    "AVATAR.MOTION.SLOT_EMPTY",
                    "requested slot is empty");
            }

            if (_slotMap.ContainsKey(slot))
            {
                CurrentSlot = slot;
                RuntimeLog.Info(
                    "avatar",
                    "avatar.motion.slot_played",
                    rid,
                    "motion slot played",
                    slot,
                    "state_machine");

                return new MotionPlayResult
                {
                    Success = true,
                    ResolvedSlot = slot,
                    ErrorCode = string.Empty,
                    Message = "ok"
                };
            }

            var fallback = ResolveFallbackSlot(slot);
            if (!string.IsNullOrWhiteSpace(fallback))
            {
                CurrentSlot = fallback;
                RuntimeLog.Warn(
                    "avatar",
                    "avatar.motion.slot_fallback_used",
                    rid,
                    "AVATAR.MOTION.SLOT_FALLBACK",
                    $"fallback slot used: {fallback}",
                    slot,
                    "state_machine");

                return new MotionPlayResult
                {
                    Success = true,
                    ResolvedSlot = fallback,
                    ErrorCode = "AVATAR.MOTION.SLOT_FALLBACK",
                    Message = "fallback used"
                };
            }

            return Fail(
                rid,
                "AVATAR.MOTION.SLOT_NOT_FOUND",
                $"slot not found: {slot}");
        }

        public string ResolveFallbackSlot(string requestedSlot)
        {
            var slot = Normalize(requestedSlot);
            if (_slotMap.TryGetValue(slot, out var definition) &&
                !string.IsNullOrWhiteSpace(definition.fallbackSlot) &&
                _slotMap.ContainsKey(definition.fallbackSlot))
            {
                return definition.fallbackSlot;
            }

            return _slotMap.ContainsKey("idle") ? "idle" : string.Empty;
        }

        private static string Normalize(string value)
        {
            var normalized = (value ?? string.Empty).Trim().ToLowerInvariant();
            return string.IsNullOrWhiteSpace(normalized) ? string.Empty : normalized;
        }

        private static MotionPlayResult Fail(string requestId, string errorCode, string message)
        {
            RuntimeLog.Error(
                "avatar",
                "avatar.motion.slot_play_failed",
                requestId,
                errorCode,
                message,
                string.Empty,
                "state_machine");

            return new MotionPlayResult
            {
                Success = false,
                ResolvedSlot = string.Empty,
                ErrorCode = errorCode,
                Message = message
            };
        }
    }
}
