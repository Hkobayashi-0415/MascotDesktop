using System;
using MascotDesktop.Runtime.Diagnostics;
using UnityEngine;

namespace MascotDesktop.Runtime.Windowing
{
    [DefaultExecutionOrder(-2200)]
    public sealed class WindowController : MonoBehaviour
    {
        private const string PrefTopmost = "MascotDesktop.Window.Topmost";
        private const string PrefX = "MascotDesktop.Window.X";
        private const string PrefY = "MascotDesktop.Window.Y";
        private const string PrefW = "MascotDesktop.Window.W";
        private const string PrefH = "MascotDesktop.Window.H";

        [SerializeField] private bool startTopmost = true;
        [SerializeField] private bool startFrameless = true;
        [SerializeField] private bool persistWindowRect = true;
        [SerializeField] private Vector2Int defaultWindowSize = new Vector2Int(1280, 720);

        public bool IsTopmost { get; private set; }

        private void Awake()
        {
            ApplyInitialWindowState();
        }

        private void OnApplicationQuit()
        {
            SaveWindowState();
        }

        public void ToggleTopmost(string requestId = null)
        {
            SetTopmost(!IsTopmost, requestId);
        }

        public void SetTopmost(bool enable, string requestId = null)
        {
            var rid = string.IsNullOrWhiteSpace(requestId) ? RuntimeLog.NewRequestId() : requestId;
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
            if (!WindowNativeGateway.TryGetTargetWindowHandle(out var hwnd))
            {
                RuntimeLog.Warn(
                    "window",
                    "window.topmost.change_failed",
                    rid,
                    "WINDOW.HWND.NOT_FOUND",
                    "window handle is unavailable",
                    string.Empty,
                    "window");
                return;
            }

            if (!WindowNativeGateway.TrySetTopmost(hwnd, enable))
            {
                RuntimeLog.Warn(
                    "window",
                    "window.topmost.change_failed",
                    rid,
                    "WINDOW.TOPMOST.SET_FAILED",
                    "failed to update topmost state",
                    string.Empty,
                    "window");
                return;
            }
#endif
#if !UNITY_STANDALONE_WIN || UNITY_EDITOR
            RuntimeLog.Info(
                "window",
                "window.topmost.simulated",
                rid,
                "native topmost is unavailable in this runtime; state is toggled for diagnostics",
                string.Empty,
                "window");
#endif
            IsTopmost = enable;
            PlayerPrefs.SetInt(PrefTopmost, IsTopmost ? 1 : 0);
            RuntimeLog.Info(
                "window",
                "window.topmost.changed",
                rid,
                enable ? "window set topmost" : "window topmost disabled",
                string.Empty,
                "window");
        }

        public void DragWindow(string requestId = null)
        {
            var rid = string.IsNullOrWhiteSpace(requestId) ? RuntimeLog.NewRequestId() : requestId;
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
            if (!WindowNativeGateway.TryGetTargetWindowHandle(out var hwnd))
            {
                RuntimeLog.Warn(
                    "window",
                    "window.drag.failed",
                    rid,
                    "WINDOW.HWND.NOT_FOUND",
                    "window handle is unavailable",
                    string.Empty,
                    "window");
                return;
            }

            if (!WindowNativeGateway.TryBeginDrag(hwnd))
            {
                RuntimeLog.Warn(
                    "window",
                    "window.drag.failed",
                    rid,
                    "WINDOW.DRAG.BEGIN_FAILED",
                    "failed to begin native drag operation",
                    string.Empty,
                    "window");
                return;
            }

            RuntimeLog.Info("window", "window.drag.started", rid, "window drag started", string.Empty, "window");
#else
            RuntimeLog.Info("window", "window.drag.skipped", rid, "window drag is only available on Windows player", string.Empty, "window");
#endif
        }

        public void SaveWindowState()
        {
            if (!persistWindowRect)
            {
                return;
            }

#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
            var rid = RuntimeLog.NewRequestId();
            if (!WindowNativeGateway.TryGetTargetWindowHandle(out var hwnd))
            {
                RuntimeLog.Warn(
                    "window",
                    "window.rect.save_failed",
                    rid,
                    "WINDOW.HWND.NOT_FOUND",
                    "window handle is unavailable",
                    string.Empty,
                    "window");
                return;
            }

            if (!WindowNativeGateway.TryGetWindowRect(hwnd, out var rect))
            {
                RuntimeLog.Warn(
                    "window",
                    "window.rect.save_failed",
                    rid,
                    "WINDOW.RECT.READ_FAILED",
                    "failed to read native window rectangle",
                    string.Empty,
                    "window");
                return;
            }

            PlayerPrefs.SetInt(PrefX, rect.Left);
            PlayerPrefs.SetInt(PrefY, rect.Top);
            PlayerPrefs.SetInt(PrefW, Math.Max(1, rect.Right - rect.Left));
            PlayerPrefs.SetInt(PrefH, Math.Max(1, rect.Bottom - rect.Top));
            PlayerPrefs.Save();
#endif
        }

        private void ApplyInitialWindowState()
        {
            var requestId = RuntimeLog.NewRequestId();

            Screen.fullScreenMode = FullScreenMode.Windowed;
            Screen.SetResolution(defaultWindowSize.x, defaultWindowSize.y, false);

            if (startFrameless)
            {
                ApplyFramelessWindow(requestId);
            }

            TryRestoreWindowRect(requestId);

            var persistedTopmost = PlayerPrefs.GetInt(PrefTopmost, startTopmost ? 1 : 0) == 1;
            SetTopmost(persistedTopmost, requestId);
        }

        private void ApplyFramelessWindow(string requestId)
        {
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
            if (!WindowNativeGateway.TryGetTargetWindowHandle(out var hwnd))
            {
                RuntimeLog.Warn(
                    "window",
                    "window.frameless.failed",
                    requestId,
                    "WINDOW.HWND.NOT_FOUND",
                    "window handle is unavailable",
                    string.Empty,
                    "window");
                return;
            }

            if (!WindowNativeGateway.TryApplyFramelessStyle(hwnd, out var alreadyFrameless))
            {
                RuntimeLog.Warn(
                    "window",
                    "window.frameless.failed",
                    requestId,
                    "WINDOW.FRAMELESS.STYLE_NOT_APPLIED",
                    "failed to apply frameless style flags",
                    string.Empty,
                    "window");
                return;
            }

            if (alreadyFrameless)
            {
                RuntimeLog.Info(
                    "window",
                    "window.frameless.already_set",
                    requestId,
                    "window is already frameless",
                    string.Empty,
                    "window");
                return;
            }

            if (!WindowNativeGateway.TryRefreshWindowFrame(hwnd))
            {
                RuntimeLog.Warn(
                    "window",
                    "window.frameless.failed",
                    requestId,
                    "WINDOW.FRAMELESS.FRAME_UPDATE_FAILED",
                    "failed to refresh window frame after style update",
                    string.Empty,
                    "window");
                return;
            }

            RuntimeLog.Info(
                "window",
                "window.frameless.applied",
                requestId,
                "window set to frameless style",
                string.Empty,
                "window");
#else
            RuntimeLog.Info(
                "window",
                "window.frameless.skipped",
                requestId,
                "frameless window is supported on Windows player",
                string.Empty,
                "window");
#endif
        }

        private void TryRestoreWindowRect(string requestId)
        {
            if (!persistWindowRect)
            {
                return;
            }

            if (!PlayerPrefs.HasKey(PrefX) ||
                !PlayerPrefs.HasKey(PrefY) ||
                !PlayerPrefs.HasKey(PrefW) ||
                !PlayerPrefs.HasKey(PrefH))
            {
                return;
            }

#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
            if (!WindowNativeGateway.TryGetTargetWindowHandle(out var hwnd))
            {
                RuntimeLog.Warn(
                    "window",
                    "window.rect.restore_failed",
                    requestId,
                    "WINDOW.HWND.NOT_FOUND",
                    "window handle is unavailable",
                    string.Empty,
                    "window");
                return;
            }

            var x = PlayerPrefs.GetInt(PrefX);
            var y = PlayerPrefs.GetInt(PrefY);
            var w = Math.Max(640, PlayerPrefs.GetInt(PrefW));
            var h = Math.Max(360, PlayerPrefs.GetInt(PrefH));

            if (WindowNativeGateway.TrySetWindowRect(hwnd, x, y, w, h))
            {
                RuntimeLog.Info(
                    "window",
                    "window.rect.restored",
                    requestId,
                    "window rectangle restored",
                    $"{x},{y},{w},{h}",
                    "window");
            }
            else
            {
                RuntimeLog.Warn(
                    "window",
                    "window.rect.restore_failed",
                    requestId,
                    "WINDOW.RECT.RESTORE_FAILED",
                    "failed to apply native window rectangle",
                    $"{x},{y},{w},{h}",
                    "window");
            }
#else
            RuntimeLog.Info(
                "window",
                "window.rect.restore_skipped",
                requestId,
                "window rectangle restore is supported on Windows player",
                string.Empty,
                "window");
#endif
        }

    }
}
