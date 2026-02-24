using System;
using UnityEngine;

namespace MascotDesktop.Runtime.Avatar
{
    public readonly struct AssetCatalogRequest
    {
        public AssetCatalogRequest(
            Func<string[]> scanPaths,
            float minScanIntervalSeconds,
            float cacheTtlSeconds,
            float emptyScanBaseBackoffSeconds,
            float maxBackoffSeconds)
        {
            ScanPaths = scanPaths;
            MinScanIntervalSeconds = minScanIntervalSeconds;
            CacheTtlSeconds = cacheTtlSeconds;
            EmptyScanBaseBackoffSeconds = emptyScanBaseBackoffSeconds;
            MaxBackoffSeconds = maxBackoffSeconds;
        }

        public Func<string[]> ScanPaths { get; }
        public float MinScanIntervalSeconds { get; }
        public float CacheTtlSeconds { get; }
        public float EmptyScanBaseBackoffSeconds { get; }
        public float MaxBackoffSeconds { get; }
    }

    public readonly struct AssetCatalogSnapshot
    {
        public AssetCatalogSnapshot(
            string[] relativePaths,
            bool didScan,
            bool wasThrottled,
            int consecutiveEmptyScans,
            float nextScanDelaySeconds)
        {
            RelativePaths = relativePaths ?? Array.Empty<string>();
            DidScan = didScan;
            WasThrottled = wasThrottled;
            ConsecutiveEmptyScans = consecutiveEmptyScans;
            NextScanDelaySeconds = Mathf.Max(0f, nextScanDelaySeconds);
        }

        public string[] RelativePaths { get; }
        public bool DidScan { get; }
        public bool WasThrottled { get; }
        public int ConsecutiveEmptyScans { get; }
        public float NextScanDelaySeconds { get; }
    }

    public sealed class AssetCatalogService
    {
        private readonly Func<float> _clockSecondsProvider;
        private string[] _cachedRelativePaths = Array.Empty<string>();
        private float _nextScanAllowedAt;
        private float _cacheExpiresAt;
        private int _consecutiveEmptyScans;
        private bool _initialized;

        public AssetCatalogService(Func<float> clockSecondsProvider = null)
        {
            _clockSecondsProvider = clockSecondsProvider ?? (() => Time.realtimeSinceStartup);
        }

        public AssetCatalogSnapshot GetOrRefresh(AssetCatalogRequest request, bool forceRefresh)
        {
            if (request.ScanPaths == null)
            {
                throw new ArgumentNullException(nameof(request), "ScanPaths delegate is required.");
            }

            var now = _clockSecondsProvider();
            var minScanIntervalSeconds = Mathf.Max(0f, request.MinScanIntervalSeconds);
            var cacheTtlSeconds = Mathf.Max(minScanIntervalSeconds, request.CacheTtlSeconds);
            var emptyScanBaseBackoffSeconds = request.EmptyScanBaseBackoffSeconds > 0f
                ? request.EmptyScanBaseBackoffSeconds
                : 0.5f;
            var maxBackoffSeconds = Mathf.Max(emptyScanBaseBackoffSeconds, request.MaxBackoffSeconds);

            var hasCache = _initialized && _cachedRelativePaths.Length > 0;
            if (!forceRefresh && hasCache && now < _cacheExpiresAt)
            {
                return BuildSnapshot(didScan: false, wasThrottled: false, now);
            }

            if (!forceRefresh && _initialized && now < _nextScanAllowedAt)
            {
                return BuildSnapshot(didScan: false, wasThrottled: true, now);
            }

            var scanned = request.ScanPaths() ?? Array.Empty<string>();
            _cachedRelativePaths = scanned;
            _initialized = true;

            if (scanned.Length <= 0)
            {
                _consecutiveEmptyScans++;
                var backoffSeconds = ComputeEmptyBackoffSeconds(
                    emptyScanBaseBackoffSeconds,
                    maxBackoffSeconds,
                    _consecutiveEmptyScans);
                _nextScanAllowedAt = now + backoffSeconds;
                _cacheExpiresAt = now + backoffSeconds;
            }
            else
            {
                _consecutiveEmptyScans = 0;
                _nextScanAllowedAt = now + minScanIntervalSeconds;
                _cacheExpiresAt = now + cacheTtlSeconds;
            }

            return BuildSnapshot(didScan: true, wasThrottled: false, now);
        }

        public static float ComputeEmptyBackoffSeconds(float baseBackoffSeconds, float maxBackoffSeconds, int consecutiveEmptyScans)
        {
            var safeBaseSeconds = baseBackoffSeconds > 0f ? baseBackoffSeconds : 0.5f;
            var safeMaxSeconds = Mathf.Max(safeBaseSeconds, maxBackoffSeconds);
            var safeConsecutiveScans = Mathf.Max(1, consecutiveEmptyScans);
            var multiplier = Mathf.Pow(2f, safeConsecutiveScans - 1);
            return Mathf.Min(safeMaxSeconds, safeBaseSeconds * multiplier);
        }

        private AssetCatalogSnapshot BuildSnapshot(bool didScan, bool wasThrottled, float now)
        {
            var nextScanDelaySeconds = Mathf.Max(0f, _nextScanAllowedAt - now);
            return new AssetCatalogSnapshot(
                _cachedRelativePaths,
                didScan,
                wasThrottled,
                _consecutiveEmptyScans,
                nextScanDelaySeconds);
        }
    }
}
