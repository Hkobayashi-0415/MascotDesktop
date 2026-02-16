using System;
using System.Runtime.InteropServices;
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
            var hwnd = GetActiveWindow();
            if (hwnd == IntPtr.Zero)
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

            var target = enable ? HWND_TOPMOST : HWND_NOTOPMOST;
            var ok = SetWindowPos(hwnd, target, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW);
            if (!ok)
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
            var hwnd = GetActiveWindow();
            if (hwnd == IntPtr.Zero)
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

            ReleaseCapture();
            SendMessage(hwnd, WM_NCLBUTTONDOWN, HTCAPTION, 0);
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
            var hwnd = GetActiveWindow();
            if (hwnd == IntPtr.Zero)
            {
                return;
            }

            if (!GetWindowRect(hwnd, out var rect))
            {
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
            var hwnd = GetActiveWindow();
            if (hwnd == IntPtr.Zero)
            {
                hwnd = GetForegroundWindow();
            }

            if (hwnd == IntPtr.Zero)
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

            var style = GetWindowLongPtr(hwnd, GWL_STYLE).ToInt64();
            var newStyle = style & ~WS_CAPTION & ~WS_THICKFRAME & ~WS_MINIMIZEBOX & ~WS_MAXIMIZEBOX;
            if (newStyle == style)
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

            SetWindowLongPtr(hwnd, GWL_STYLE, new IntPtr(newStyle));
            SetWindowPos(hwnd, IntPtr.Zero, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOZORDER | SWP_FRAMECHANGED | SWP_SHOWWINDOW);

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
            var hwnd = GetActiveWindow();
            if (hwnd == IntPtr.Zero)
            {
                return;
            }

            var x = PlayerPrefs.GetInt(PrefX);
            var y = PlayerPrefs.GetInt(PrefY);
            var w = Math.Max(640, PlayerPrefs.GetInt(PrefW));
            var h = Math.Max(360, PlayerPrefs.GetInt(PrefH));

            var ok = SetWindowPos(hwnd, IntPtr.Zero, x, y, w, h, SWP_NOZORDER | SWP_SHOWWINDOW);
            if (ok)
            {
                RuntimeLog.Info(
                    "window",
                    "window.rect.restored",
                    requestId,
                    "window rectangle restored",
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

#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
        private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        private static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);
        private const int GWL_STYLE = -16;
        private const long WS_CAPTION = 0x00C00000L;
        private const long WS_THICKFRAME = 0x00040000L;
        private const long WS_MINIMIZEBOX = 0x00020000L;
        private const long WS_MAXIMIZEBOX = 0x00010000L;
        private const int SWP_NOSIZE = 0x0001;
        private const int SWP_NOMOVE = 0x0002;
        private const int SWP_NOZORDER = 0x0004;
        private const int SWP_SHOWWINDOW = 0x0040;
        private const int SWP_FRAMECHANGED = 0x0020;
        private const int WM_NCLBUTTONDOWN = 0x00A1;
        private const int HTCAPTION = 0x0002;

        [StructLayout(LayoutKind.Sequential)]
        private struct Rect
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [DllImport("user32.dll")]
        private static extern IntPtr GetActiveWindow();

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetWindowPos(
            IntPtr hWnd,
            IntPtr hWndInsertAfter,
            int x,
            int y,
            int cx,
            int cy,
            int uFlags);

        [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr", SetLastError = true)]
        private static extern IntPtr GetWindowLongPtr64(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr", SetLastError = true)]
        private static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("user32.dll", EntryPoint = "GetWindowLong", SetLastError = true)]
        private static extern IntPtr GetWindowLong32(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "SetWindowLong", SetLastError = true)]
        private static extern IntPtr SetWindowLong32(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        private static IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex)
        {
            return IntPtr.Size == 8 ? GetWindowLongPtr64(hWnd, nIndex) : GetWindowLong32(hWnd, nIndex);
        }

        private static IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong)
        {
            return IntPtr.Size == 8 ? SetWindowLongPtr64(hWnd, nIndex, dwNewLong) : SetWindowLong32(hWnd, nIndex, dwNewLong);
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool GetWindowRect(IntPtr hWnd, out Rect lpRect);

        [DllImport("user32.dll")]
        private static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);
#endif
    }
}
