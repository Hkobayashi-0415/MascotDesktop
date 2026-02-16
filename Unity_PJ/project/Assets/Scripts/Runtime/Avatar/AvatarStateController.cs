using System;
using System.Collections.Generic;
using MascotDesktop.Runtime.Core;
using MascotDesktop.Runtime.Diagnostics;
using UnityEngine;

namespace MascotDesktop.Runtime.Avatar
{
    [Serializable]
    public sealed class AvatarStateSlotMap
    {
        public string stateName;
        public string slotName;
    }

    [DefaultExecutionOrder(-2050)]
    public sealed class AvatarStateController : MonoBehaviour
    {
        [SerializeField] private CoreOrchestrator orchestrator;
        [SerializeField] private MotionSlotPlayer motionSlotPlayer;
        [SerializeField] private List<AvatarStateSlotMap> stateToSlotMaps = new List<AvatarStateSlotMap>
        {
            new AvatarStateSlotMap { stateName = "idle", slotName = "idle" },
            new AvatarStateSlotMap { stateName = "happy", slotName = "happy" },
            new AvatarStateSlotMap { stateName = "sleepy", slotName = "sleepy" },
            new AvatarStateSlotMap { stateName = "sad", slotName = "idle" }
        };

        private readonly Dictionary<string, string> _stateToSlot = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public string CurrentState { get; private set; } = "idle";

        private void Awake()
        {
            CacheDependencies();
            RebuildStateMap();
            SubscribeEvents();
            ApplyState("idle", RuntimeLog.NewRequestId());
        }

        private void OnDestroy()
        {
            UnsubscribeEvents();
        }

        public void SetDependencies(CoreOrchestrator newOrchestrator, MotionSlotPlayer newMotionSlotPlayer)
        {
            UnsubscribeEvents();
            orchestrator = newOrchestrator;
            motionSlotPlayer = newMotionSlotPlayer;
            SubscribeEvents();
        }

        public void RebuildStateMap()
        {
            _stateToSlot.Clear();
            foreach (var map in stateToSlotMaps)
            {
                var state = Normalize(map.stateName);
                var slot = Normalize(map.slotName);
                if (string.IsNullOrWhiteSpace(state) || string.IsNullOrWhiteSpace(slot))
                {
                    continue;
                }

                _stateToSlot[state] = slot;
            }
        }

        public string ResolveSlotForState(string state)
        {
            var normalized = Normalize(state);
            if (_stateToSlot.TryGetValue(normalized, out var mappedSlot))
            {
                return mappedSlot;
            }

            return "idle";
        }

        public MotionPlayResult ApplyState(string state, string requestId = null)
        {
            var rid = string.IsNullOrWhiteSpace(requestId) ? RuntimeLog.NewRequestId() : requestId;
            var normalizedState = Normalize(state);
            if (string.IsNullOrWhiteSpace(normalizedState))
            {
                normalizedState = "idle";
            }

            CurrentState = normalizedState;
            var targetSlot = ResolveSlotForState(normalizedState);

            RuntimeLog.Info(
                "avatar",
                "avatar.state.transitioned",
                rid,
                $"avatar state changed to {normalizedState}",
                targetSlot,
                "state_machine");

            if (motionSlotPlayer == null)
            {
                RuntimeLog.Warn(
                    "avatar",
                    "avatar.state.transition_failed",
                    rid,
                    "AVATAR.STATE.MOTION_PLAYER_MISSING",
                    "motion slot player is missing",
                    targetSlot,
                    "state_machine");

                return new MotionPlayResult
                {
                    Success = false,
                    ResolvedSlot = string.Empty,
                    ErrorCode = "AVATAR.STATE.MOTION_PLAYER_MISSING",
                    Message = "motion slot player is missing"
                };
            }

            return motionSlotPlayer.PlaySlot(targetSlot, rid);
        }

        private void OnAvatarStateChanged(string requestId, string state)
        {
            ApplyState(state, requestId);
        }

        private void OnMotionSlotRequested(string requestId, string slot)
        {
            if (motionSlotPlayer != null)
            {
                motionSlotPlayer.PlaySlot(slot, requestId);
            }
        }

        private void CacheDependencies()
        {
            if (orchestrator == null)
            {
                orchestrator = FindFirstObjectByType<CoreOrchestrator>();
            }

            if (motionSlotPlayer == null)
            {
                motionSlotPlayer = GetComponent<MotionSlotPlayer>();
            }

            if (motionSlotPlayer == null)
            {
                motionSlotPlayer = FindFirstObjectByType<MotionSlotPlayer>();
            }
        }

        private void SubscribeEvents()
        {
            if (orchestrator != null)
            {
                orchestrator.AvatarStateChanged += OnAvatarStateChanged;
                orchestrator.MotionSlotRequested += OnMotionSlotRequested;
            }
        }

        private void UnsubscribeEvents()
        {
            if (orchestrator != null)
            {
                orchestrator.AvatarStateChanged -= OnAvatarStateChanged;
                orchestrator.MotionSlotRequested -= OnMotionSlotRequested;
            }
        }

        private static string Normalize(string value)
        {
            var normalized = (value ?? string.Empty).Trim().ToLowerInvariant();
            return string.IsNullOrWhiteSpace(normalized) ? string.Empty : normalized;
        }
    }
}
