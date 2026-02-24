using System;
using MascotDesktop.Runtime.Windowing;
using NUnit.Framework;

namespace MascotDesktop.Tests.EditMode
{
    public sealed class WindowNativeGatewayTests
    {
        [Test]
        public void GatewayMethods_NonNativeRuntime_ReturnDeterministicFallbacks()
        {
            var hasHandle = WindowNativeGateway.TryGetTargetWindowHandle(out var hwnd);
            var setTopmost = WindowNativeGateway.TrySetTopmost(IntPtr.Zero, enable: true);
            var minimize = WindowNativeGateway.TryMinimizeWindow(IntPtr.Zero);
            var restore = WindowNativeGateway.TryRestoreWindow(IntPtr.Zero);
            var applyFrameless = WindowNativeGateway.TryApplyFramelessStyle(IntPtr.Zero, out var alreadyFrameless);
            var refreshFrame = WindowNativeGateway.TryRefreshWindowFrame(IntPtr.Zero);
            var readRect = WindowNativeGateway.TryGetWindowRect(IntPtr.Zero, out var rect);
            var setRect = WindowNativeGateway.TrySetWindowRect(IntPtr.Zero, 0, 0, 100, 100);
            var beginDrag = WindowNativeGateway.TryBeginDrag(IntPtr.Zero);

            Assert.That(hasHandle, Is.False);
            Assert.That(hwnd, Is.EqualTo(IntPtr.Zero));
            Assert.That(setTopmost, Is.False);
            Assert.That(minimize, Is.False);
            Assert.That(restore, Is.False);
            Assert.That(applyFrameless, Is.False);
            Assert.That(alreadyFrameless, Is.False);
            Assert.That(refreshFrame, Is.False);
            Assert.That(readRect, Is.False);
            Assert.That(rect.Left, Is.EqualTo(0));
            Assert.That(rect.Right, Is.EqualTo(0));
            Assert.That(setRect, Is.False);
            Assert.That(beginDrag, Is.False);
        }
    }
}
