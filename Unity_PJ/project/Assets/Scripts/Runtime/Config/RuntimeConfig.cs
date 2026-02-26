using UnityEngine;

namespace MascotDesktop.Runtime.Config
{
    public sealed class RuntimeConfig : MonoBehaviour
    {
        public enum RuntimeMode
        {
            Loopback,
            Core
        }

        [Header("HTTP Bridge")]
        [Tooltip("Enable outgoing HTTP bridge calls.")]
        public bool enableHttpBridge;

        [Tooltip("Runtime mode switch. Loopback uses loopbackBaseUrl, Core uses coreBaseUrl.")]
        public RuntimeMode runtimeMode = RuntimeMode.Loopback;

        [Tooltip("Loopback base URL for local dummy/minimal core.")]
        public string loopbackBaseUrl = "http://127.0.0.1:18080";

        [Tooltip("Full core base URL.")]
        public string coreBaseUrl = "http://127.0.0.1:8769";

        [Tooltip("Legacy fallback timeout in milliseconds when endpoint-specific values are not set.")]
        public int httpTimeoutMs = 1500;

        [Header("Endpoint Timeouts (ms)")]
        public int healthTimeoutMs = 3000;
        public int llmTimeoutMs = 45000;
        public int ttsTimeoutMs = 25000;
        public int sttTimeoutMs = 15000;
        public int configTimeoutMs = 5000;

        [Header("Endpoint Retries")]
        public int healthMaxRetries = 0;
        public int llmMaxRetries = 5;
        public int ttsMaxRetries = 3;
        public int sttMaxRetries = 2;
        public int configMaxRetries = 0;

        [Header("Operations")]
        [Tooltip("Show detailed degraded status in HUD when enabled.")]
        public bool adminDebugMode;

        [Header("Runtime Defaults")]
        public string defaultAvatarState = "idle";
        public string defaultMotionSlot = "idle";

        public bool IsCoreMode => runtimeMode == RuntimeMode.Core;

        public string ResolveBridgeBaseUrl()
        {
            if (IsCoreMode)
            {
                return NormalizeBaseUrl(string.IsNullOrWhiteSpace(coreBaseUrl) ? loopbackBaseUrl : coreBaseUrl);
            }

            return NormalizeBaseUrl(loopbackBaseUrl);
        }

        public int GetTimeoutMsForPath(string relativePath)
        {
            var path = (relativePath ?? string.Empty).Trim().ToLowerInvariant();
            if (path == "/health")
            {
                return ClampTimeoutMs(healthTimeoutMs);
            }

            if (path.StartsWith("/v1/chat/"))
            {
                return ClampTimeoutMs(llmTimeoutMs);
            }

            if (path.StartsWith("/v1/tts/"))
            {
                return ClampTimeoutMs(ttsTimeoutMs);
            }

            if (path.StartsWith("/v1/stt/"))
            {
                return ClampTimeoutMs(sttTimeoutMs);
            }

            if (path.StartsWith("/v1/config/"))
            {
                return ClampTimeoutMs(configTimeoutMs);
            }

            return ClampTimeoutMs(httpTimeoutMs);
        }

        public int GetMaxRetriesForPath(string relativePath)
        {
            var path = (relativePath ?? string.Empty).Trim().ToLowerInvariant();
            if (path == "/health")
            {
                return ClampRetries(healthMaxRetries);
            }

            if (path.StartsWith("/v1/chat/"))
            {
                return ClampRetries(llmMaxRetries);
            }

            if (path.StartsWith("/v1/tts/"))
            {
                return ClampRetries(ttsMaxRetries);
            }

            if (path.StartsWith("/v1/stt/"))
            {
                return ClampRetries(sttMaxRetries);
            }

            if (path.StartsWith("/v1/config/"))
            {
                return ClampRetries(configMaxRetries);
            }

            return 0;
        }

        private static string NormalizeBaseUrl(string baseUrl)
        {
            var normalized = string.IsNullOrWhiteSpace(baseUrl) ? "http://127.0.0.1:18080" : baseUrl.Trim();
            return normalized.TrimEnd('/');
        }

        private static int ClampTimeoutMs(int timeoutMs)
        {
            return Mathf.Max(200, timeoutMs);
        }

        private static int ClampRetries(int retries)
        {
            return Mathf.Max(0, retries);
        }
    }
}
