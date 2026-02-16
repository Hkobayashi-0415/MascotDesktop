using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MascotDesktop.Runtime.Config;
using MascotDesktop.Runtime.Diagnostics;
using UnityEngine;

namespace MascotDesktop.Runtime.Ipc
{
    public sealed class LoopbackHttpResult
    {
        public bool Success;
        public string RequestId;
        public int StatusCode;
        public string Body;
        public string ErrorCode;
        public string Message;
    }

    public sealed class LoopbackHttpClient : MonoBehaviour
    {
        [SerializeField] private RuntimeConfig runtimeConfig;

        private HttpClient _httpClient;

        private void Awake()
        {
            if (runtimeConfig == null)
            {
                runtimeConfig = GetComponent<RuntimeConfig>();
            }

            if (runtimeConfig == null)
            {
                runtimeConfig = FindFirstObjectByType<RuntimeConfig>();
            }

            _httpClient = new HttpClient();
        }

        private void OnDestroy()
        {
            _httpClient?.Dispose();
        }

        public async Task<LoopbackHttpResult> PostJsonAsync(
            string relativePath,
            string requestId,
            string jsonBody,
            CancellationToken cancellationToken = default)
        {
            var rid = string.IsNullOrWhiteSpace(requestId) ? RuntimeLog.NewRequestId() : requestId;
            if (!CanRunBridge(rid))
            {
                return new LoopbackHttpResult
                {
                    Success = false,
                    RequestId = rid,
                    StatusCode = 0,
                    Body = string.Empty,
                    ErrorCode = "IPC.HTTP.DISABLED",
                    Message = "loopback bridge is disabled"
                };
            }

            var baseUrl = NormalizeBaseUrl(runtimeConfig.loopbackBaseUrl);
            var targetUrl = BuildTargetUrl(baseUrl, relativePath);
            var body = EnsureRequestIdInBody(jsonBody, rid);
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            linkedCts.CancelAfter(Math.Max(200, runtimeConfig.httpTimeoutMs));

            RuntimeLog.Info(
                "ipc",
                "ipc.http.request",
                rid,
                $"POST {targetUrl}",
                relativePath ?? string.Empty,
                "loopback_http");

            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Post, targetUrl);
                request.Headers.Add("X-Request-Id", rid);
                request.Content = new StringContent(body, Encoding.UTF8, "application/json");

                using var response = await _httpClient.SendAsync(request, linkedCts.Token);
                var responseBody = await response.Content.ReadAsStringAsync();
                var status = (int)response.StatusCode;
                var success = response.IsSuccessStatusCode;

                if (!success)
                {
                    RuntimeLog.Error(
                        "ipc",
                        "ipc.http.response_failed",
                        rid,
                        "IPC.HTTP.NON_SUCCESS",
                        $"non-success response: {status}",
                        relativePath ?? string.Empty,
                        "loopback_http");
                }
                else
                {
                    RuntimeLog.Info(
                        "ipc",
                        "ipc.http.response",
                        rid,
                        $"response status: {status}",
                        relativePath ?? string.Empty,
                        "loopback_http");
                }

                return new LoopbackHttpResult
                {
                    Success = success,
                    RequestId = rid,
                    StatusCode = status,
                    Body = responseBody ?? string.Empty,
                    ErrorCode = success ? string.Empty : "IPC.HTTP.NON_SUCCESS",
                    Message = success ? "ok" : "non-success response"
                };
            }
            catch (Exception ex)
            {
                RuntimeLog.Error(
                    "ipc",
                    "ipc.http.request_failed",
                    rid,
                    "IPC.HTTP.REQUEST_FAILED",
                    "loopback http request failed",
                    relativePath ?? string.Empty,
                    "loopback_http",
                    ex);

                return new LoopbackHttpResult
                {
                    Success = false,
                    RequestId = rid,
                    StatusCode = 0,
                    Body = string.Empty,
                    ErrorCode = "IPC.HTTP.REQUEST_FAILED",
                    Message = ex.GetBaseException().Message
                };
            }
        }

        private bool CanRunBridge(string requestId)
        {
            if (runtimeConfig == null)
            {
                RuntimeLog.Warn(
                    "ipc",
                    "ipc.http.config_missing",
                    requestId,
                    "IPC.HTTP.CONFIG_MISSING",
                    "runtime config is missing",
                    string.Empty,
                    "loopback_http");
                return false;
            }

            if (!runtimeConfig.enableHttpBridge)
            {
                RuntimeLog.Info(
                    "ipc",
                    "ipc.http.disabled",
                    requestId,
                    "loopback bridge is disabled",
                    string.Empty,
                    "loopback_http");
                return false;
            }

            return true;
        }

        private static string NormalizeBaseUrl(string baseUrl)
        {
            var normalized = string.IsNullOrWhiteSpace(baseUrl) ? "http://127.0.0.1:18080" : baseUrl.Trim();
            return normalized.TrimEnd('/');
        }

        private static string BuildTargetUrl(string baseUrl, string relativePath)
        {
            var path = string.IsNullOrWhiteSpace(relativePath) ? "/" : relativePath.Trim();
            if (!path.StartsWith("/"))
            {
                path = "/" + path;
            }

            return baseUrl + path;
        }

        private static string EnsureRequestIdInBody(string body, string requestId)
        {
            if (string.IsNullOrWhiteSpace(body))
            {
                return "{\"request_id\":\"" + EscapeJson(requestId) + "\"}";
            }

            var trimmed = body.Trim();
            if (trimmed.Contains("\"request_id\""))
            {
                return trimmed;
            }

            if (trimmed.StartsWith("{") || trimmed.StartsWith("["))
            {
                return "{\"request_id\":\"" + EscapeJson(requestId) + "\",\"payload\":" + trimmed + "}";
            }

            return "{\"request_id\":\"" + EscapeJson(requestId) + "\",\"payload_text\":\"" + EscapeJson(trimmed) + "\"}";
        }

        private static string EscapeJson(string value)
        {
            return (value ?? string.Empty)
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"")
                .Replace("\r", "\\r")
                .Replace("\n", "\\n");
        }
    }
}
