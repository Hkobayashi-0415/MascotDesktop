using System;
using System.Runtime.InteropServices;
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
            var hwnd = GetActiveWindow();
            if (hwnd == IntPtr.Zero)
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

            ShowWindow(hwnd, SW_MINIMIZE);
#else
            Application.runInBackground = true;
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
            var hwnd = GetActiveWindow();
            if (hwnd == IntPtr.Zero)
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

            ShowWindow(hwnd, SW_RESTORE);
#else
            Application.runInBackground = false;
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

#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
        private const int SW_RESTORE = 9;
        private const int SW_MINIMIZE = 6;

        [DllImport("user32.dll")]
        private static extern IntPtr GetActiveWindow();

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
#endif
    }
}
