using System;
using System.Runtime.InteropServices;

namespace MascotDesktop.Runtime.Windowing
{
    public struct WindowNativeRect
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    public static class WindowNativeGateway
    {
        private static readonly IntPtr HwndTopmost = new IntPtr(-1);
        private static readonly IntPtr HwndNotTopmost = new IntPtr(-2);
        private const int SwRestore = 9;
        private const int SwMinimize = 6;
        private const int GwlStyle = -16;
        private const long WsCaption = 0x00C00000L;
        private const long WsThickFrame = 0x00040000L;
        private const long WsMinimizeBox = 0x00020000L;
        private const long WsMaximizeBox = 0x00010000L;
        private const long FramelessMask = WsCaption | WsThickFrame | WsMinimizeBox | WsMaximizeBox;
        private const int SwpNoSize = 0x0001;
        private const int SwpNoMove = 0x0002;
        private const int SwpNoZOrder = 0x0004;
        private const int SwpShowWindow = 0x0040;
        private const int SwpFrameChanged = 0x0020;
        private const int WmNclButtonDown = 0x00A1;
        private const int HtCaption = 0x0002;

        public static bool TryGetTargetWindowHandle(out IntPtr hwnd)
        {
            hwnd = IntPtr.Zero;
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
            hwnd = GetActiveWindow();
            if (hwnd == IntPtr.Zero)
            {
                hwnd = GetForegroundWindow();
            }
#endif
            return hwnd != IntPtr.Zero;
        }

        public static bool TrySetTopmost(IntPtr hwnd, bool enable)
        {
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
            var target = enable ? HwndTopmost : HwndNotTopmost;
            return SetWindowPos(hwnd, target, 0, 0, 0, 0, SwpNoMove | SwpNoSize | SwpShowWindow);
#else
            return false;
#endif
        }

        public static bool TryMinimizeWindow(IntPtr hwnd)
        {
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
            return ShowWindow(hwnd, SwMinimize);
#else
            return false;
#endif
        }

        public static bool TryRestoreWindow(IntPtr hwnd)
        {
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
            return ShowWindow(hwnd, SwRestore);
#else
            return false;
#endif
        }

        public static bool TryApplyFramelessStyle(IntPtr hwnd, out bool alreadyFrameless)
        {
            alreadyFrameless = false;
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
            var style = GetWindowLongPtr(hwnd, GwlStyle).ToInt64();
            var newStyle = style & ~WsCaption & ~WsThickFrame & ~WsMinimizeBox & ~WsMaximizeBox;
            if (newStyle == style)
            {
                alreadyFrameless = true;
                return true;
            }

            SetWindowLongPtr(hwnd, GwlStyle, new IntPtr(newStyle));
            var appliedStyle = GetWindowLongPtr(hwnd, GwlStyle).ToInt64();
            return (appliedStyle & FramelessMask) == 0;
#else
            return false;
#endif
        }

        public static bool TryRefreshWindowFrame(IntPtr hwnd)
        {
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
            return SetWindowPos(hwnd, IntPtr.Zero, 0, 0, 0, 0, SwpNoMove | SwpNoSize | SwpNoZOrder | SwpFrameChanged | SwpShowWindow);
#else
            return false;
#endif
        }

        public static bool TryGetWindowRect(IntPtr hwnd, out WindowNativeRect rect)
        {
            rect = default(WindowNativeRect);
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
            if (!GetWindowRect(hwnd, out var nativeRect))
            {
                return false;
            }

            rect = new WindowNativeRect
            {
                Left = nativeRect.Left,
                Top = nativeRect.Top,
                Right = nativeRect.Right,
                Bottom = nativeRect.Bottom
            };
            return true;
#else
            return false;
#endif
        }

        public static bool TrySetWindowRect(IntPtr hwnd, int x, int y, int width, int height)
        {
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
            return SetWindowPos(hwnd, IntPtr.Zero, x, y, width, height, SwpNoZOrder | SwpShowWindow);
#else
            return false;
#endif
        }

        public static bool TryBeginDrag(IntPtr hwnd)
        {
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
            if (!ReleaseCapture())
            {
                return false;
            }

            SendMessage(hwnd, WmNclButtonDown, HtCaption, 0);
            return true;
#else
            return false;
#endif
        }

#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
        [StructLayout(LayoutKind.Sequential)]
        private struct NativeRect
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

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool GetWindowRect(IntPtr hWnd, out NativeRect lpRect);

        [DllImport("user32.dll")]
        private static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private static IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex)
        {
            return IntPtr.Size == 8 ? GetWindowLongPtr64(hWnd, nIndex) : GetWindowLong32(hWnd, nIndex);
        }

        private static IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong)
        {
            return IntPtr.Size == 8 ? SetWindowLongPtr64(hWnd, nIndex, dwNewLong) : SetWindowLong32(hWnd, nIndex, dwNewLong);
        }
#endif
    }
}
