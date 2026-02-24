using MascotDesktop.Runtime.Avatar;
using NUnit.Framework;

namespace MascotDesktop.Tests.EditMode
{
    public sealed class AssetCatalogServiceTests
    {
        [Test]
        public void GetOrRefresh_UsesCachedResultsWithinTtl()
        {
            var now = 0f;
            var scanCount = 0;
            var service = new AssetCatalogService(() => now);
            var request = new AssetCatalogRequest(
                scanPaths: () =>
                {
                    scanCount++;
                    return new[] { "characters/demo/mmd/avatar.pmx" };
                },
                minScanIntervalSeconds: 1f,
                cacheTtlSeconds: 5f,
                emptyScanBaseBackoffSeconds: 1f,
                maxBackoffSeconds: 16f);

            var first = service.GetOrRefresh(request, forceRefresh: false);
            now = 0.2f;
            var second = service.GetOrRefresh(request, forceRefresh: false);

            Assert.That(first.DidScan, Is.True);
            Assert.That(second.DidScan, Is.False);
            Assert.That(second.WasThrottled, Is.False);
            Assert.That(scanCount, Is.EqualTo(1));
            Assert.That(second.RelativePaths.Length, Is.EqualTo(1));
        }

        [Test]
        public void GetOrRefresh_ThrottlesConsecutiveEmptyScans()
        {
            var now = 0f;
            var scanCount = 0;
            var service = new AssetCatalogService(() => now);
            var request = new AssetCatalogRequest(
                scanPaths: () =>
                {
                    scanCount++;
                    return System.Array.Empty<string>();
                },
                minScanIntervalSeconds: 1f,
                cacheTtlSeconds: 5f,
                emptyScanBaseBackoffSeconds: 1f,
                maxBackoffSeconds: 16f);

            var first = service.GetOrRefresh(request, forceRefresh: false);
            now = 0.3f;
            var throttled = service.GetOrRefresh(request, forceRefresh: false);
            now = 1.1f;
            var secondScan = service.GetOrRefresh(request, forceRefresh: false);

            Assert.That(first.DidScan, Is.True);
            Assert.That(first.ConsecutiveEmptyScans, Is.EqualTo(1));
            Assert.That(throttled.DidScan, Is.False);
            Assert.That(throttled.WasThrottled, Is.True);
            Assert.That(secondScan.DidScan, Is.True);
            Assert.That(secondScan.ConsecutiveEmptyScans, Is.EqualTo(2));
            Assert.That(scanCount, Is.EqualTo(2));
        }

        [Test]
        public void GetOrRefresh_ForceRefreshBypassesThrottle()
        {
            var now = 0f;
            var scanCount = 0;
            var service = new AssetCatalogService(() => now);
            var request = new AssetCatalogRequest(
                scanPaths: () =>
                {
                    scanCount++;
                    return System.Array.Empty<string>();
                },
                minScanIntervalSeconds: 1f,
                cacheTtlSeconds: 5f,
                emptyScanBaseBackoffSeconds: 1f,
                maxBackoffSeconds: 16f);

            service.GetOrRefresh(request, forceRefresh: false);
            now = 0.2f;
            var forced = service.GetOrRefresh(request, forceRefresh: true);

            Assert.That(forced.DidScan, Is.True);
            Assert.That(scanCount, Is.EqualTo(2));
        }

        [Test]
        public void ComputeEmptyBackoffSeconds_UsesExponentialBackoffWithCap()
        {
            var first = AssetCatalogService.ComputeEmptyBackoffSeconds(1f, 16f, 1);
            var third = AssetCatalogService.ComputeEmptyBackoffSeconds(1f, 16f, 3);
            var capped = AssetCatalogService.ComputeEmptyBackoffSeconds(1f, 4f, 10);

            Assert.That(first, Is.EqualTo(1f).Within(0.0001f));
            Assert.That(third, Is.EqualTo(4f).Within(0.0001f));
            Assert.That(capped, Is.EqualTo(4f).Within(0.0001f));
        }
    }
}
