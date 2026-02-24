using MascotDesktop.Runtime.Diagnostics;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MascotDesktop.Runtime.Windowing
{
    public sealed class ResidentController : MonoBehaviour
    {
        [SerializeField] private Key toggleResidentHotkey = Key.F10;
        [SerializeField] private Key exitHotkey = Key.F12;

        public bool IsHidden { get; private set; }

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null)
            {
                return;
            }

            if (keyboard[toggleResidentHotkey].wasPressedThisFrame)
            {
                ToggleResidentVisibility(RuntimeLog.NewRequestId());
            }

            if (keyboard[exitHotkey].wasPressedThisFrame)
            {
                ExitApplication(RuntimeLog.NewRequestId());
            }
        }

        public void ToggleResidentVisibility(string requestId = null)
        {
            if (IsHidden)
            {
                RestoreFromResident(requestId);
            }
            else
            {
                HideToResident(requestId);
            }
        }

        public void HideToResident(string requestId = null)
        {
            var rid = string.IsNullOrWhiteSpace(requestId) ? RuntimeLog.NewRequestId() : requestId;
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
            if (!WindowNativeGateway.TryGetTargetWindowHandle(out var hwnd))
            {
                RuntimeLog.Warn(
                    "window",
                    "window.resident.hide_failed",
                    rid,
                    "WINDOW.HWND.NOT_FOUND",
                    "window handle is unavailable",
                    string.Empty,
                    "resident");
                return;
            }

            if (!WindowNativeGateway.TryMinimizeWindow(hwnd))
            {
                RuntimeLog.Warn(
                    "window",
                    "window.resident.hide_failed",
                    rid,
                    "WINDOW.RESIDENT.MINIMIZE_FAILED",
                    "failed to minimize window for resident mode",
                    string.Empty,
                    "resident");
                return;
            }
#else
            Application.runInBackground = true;
            RuntimeLog.Info(
                "window",
                "window.resident.hide.simulated",
                rid,
                "native minimize is unavailable in this runtime; resident hide is simulated",
                string.Empty,
                "resident");
#endif
            IsHidden = true;

            RuntimeLog.Info(
                "window",
                "window.resident.hidden",
                rid,
                "window moved to resident mode",
                string.Empty,
                "resident");
        }

        public void RestoreFromResident(string requestId = null)
        {
            var rid = string.IsNullOrWhiteSpace(requestId) ? RuntimeLog.NewRequestId() : requestId;
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
            if (!WindowNativeGateway.TryGetTargetWindowHandle(out var hwnd))
            {
                RuntimeLog.Warn(
                    "window",
                    "window.resident.restore_failed",
                    rid,
                    "WINDOW.HWND.NOT_FOUND",
                    "window handle is unavailable",
                    string.Empty,
                    "resident");
                return;
            }

            if (!WindowNativeGateway.TryRestoreWindow(hwnd))
            {
                RuntimeLog.Warn(
                    "window",
                    "window.resident.restore_failed",
                    rid,
                    "WINDOW.RESIDENT.RESTORE_FAILED",
                    "failed to restore window from resident mode",
                    string.Empty,
                    "resident");
                return;
            }
#else
            Application.runInBackground = false;
            RuntimeLog.Info(
                "window",
                "window.resident.restore.simulated",
                rid,
                "native restore is unavailable in this runtime; resident restore is simulated",
                string.Empty,
                "resident");
#endif
            IsHidden = false;

            RuntimeLog.Info(
                "window",
                "window.resident.restored",
                rid,
                "window restored from resident mode",
                string.Empty,
                "resident");
        }

        public void ExitApplication(string requestId = null)
        {
            var rid = string.IsNullOrWhiteSpace(requestId) ? RuntimeLog.NewRequestId() : requestId;
            RuntimeLog.Info(
                "window",
                "window.application.exit_requested",
                rid,
                "exit requested by resident controller",
                string.Empty,
                "resident");
            Application.Quit();
        }

    }
}
