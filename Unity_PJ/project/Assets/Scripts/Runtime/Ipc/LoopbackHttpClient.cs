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
        public string ResponseRequestId;
        public int StatusCode;
        public string Body;
        public string ErrorCode;
        public string Message;
        public bool Retryable;
    }

    public sealed class LoopbackHttpClient : MonoBehaviour
    {
        [SerializeField] private RuntimeConfig runtimeConfig;

        private HttpClient _httpClient;

        [Serializable]
        private sealed class ResponseEnvelope
        {
            public string status;
            public string request_id;
            public string error_code;
            public string message;
            public bool retryable;
        }

        private sealed class ParsedResponseInfo
        {
            public string Status = string.Empty;
            public string RequestId = string.Empty;
            public string ErrorCode = string.Empty;
            public string Message = string.Empty;
            public bool Retryable;
            public bool HasRetryable;
        }

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
                    ResponseRequestId = string.Empty,
                    StatusCode = 0,
                    Body = string.Empty,
                    ErrorCode = "IPC.HTTP.DISABLED",
                    Message = "loopback bridge is disabled",
                    Retryable = false
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
                var parsed = ParseResponseInfo(responseBody);

                if (!string.IsNullOrWhiteSpace(parsed.RequestId) &&
                    !string.Equals(parsed.RequestId, rid, StringComparison.Ordinal))
                {
                    RuntimeLog.Error(
                        "ipc",
                        "ipc.http.request_id_mismatch",
                        rid,
                        "IPC.HTTP.REQUEST_ID_MISMATCH",
                        $"response request_id mismatch: expected={rid}, actual={parsed.RequestId}",
                        relativePath ?? string.Empty,
                        "loopback_http");

                    return new LoopbackHttpResult
                    {
                        Success = false,
                        RequestId = rid,
                        ResponseRequestId = parsed.RequestId,
                        StatusCode = status,
                        Body = responseBody ?? string.Empty,
                        ErrorCode = "IPC.HTTP.REQUEST_ID_MISMATCH",
                        Message = "response request_id mismatch",
                        Retryable = false
                    };
                }

                var payloadError = string.Equals(parsed.Status, "error", StringComparison.OrdinalIgnoreCase);
                var success = response.IsSuccessStatusCode && !payloadError;
                var errorCode = string.Empty;
                var message = "ok";
                var retryable = false;

                if (!success)
                {
                    errorCode = ResolveErrorCode(response.IsSuccessStatusCode, parsed);
                    message = ResolveErrorMessage(response.IsSuccessStatusCode, parsed, status);
                    retryable = parsed.HasRetryable && parsed.Retryable;
                }

                if (!success)
                {
                    RuntimeLog.Error(
                        "ipc",
                        "ipc.http.response_failed",
                        rid,
                        errorCode,
                        message,
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
                    ResponseRequestId = parsed.RequestId,
                    StatusCode = status,
                    Body = responseBody ?? string.Empty,
                    ErrorCode = success ? string.Empty : errorCode,
                    Message = success ? "ok" : message,
                    Retryable = retryable
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
                    ResponseRequestId = string.Empty,
                    StatusCode = 0,
                    Body = string.Empty,
                    ErrorCode = "IPC.HTTP.REQUEST_FAILED",
                    Message = ex.GetBaseException().Message,
                    Retryable = true
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

        private static ParsedResponseInfo ParseResponseInfo(string responseBody)
        {
            var parsed = new ParsedResponseInfo();
            if (string.IsNullOrWhiteSpace(responseBody))
            {
                return parsed;
            }

            try
            {
                var envelope = JsonUtility.FromJson<ResponseEnvelope>(responseBody);
                if (envelope == null)
                {
                    return parsed;
                }

                parsed.Status = envelope.status ?? string.Empty;
                parsed.RequestId = envelope.request_id ?? string.Empty;
                parsed.ErrorCode = envelope.error_code ?? string.Empty;
                parsed.Message = envelope.message ?? string.Empty;
                parsed.Retryable = envelope.retryable;
                parsed.HasRetryable = responseBody.IndexOf("\"retryable\"", StringComparison.OrdinalIgnoreCase) >= 0;
                return parsed;
            }
            catch
            {
                return parsed;
            }
        }

        private static string ResolveErrorCode(bool httpSuccess, ParsedResponseInfo parsed)
        {
            if (!string.IsNullOrWhiteSpace(parsed.ErrorCode))
            {
                return parsed.ErrorCode;
            }

            if (httpSuccess && string.Equals(parsed.Status, "error", StringComparison.OrdinalIgnoreCase))
            {
                return "IPC.HTTP.PAYLOAD_ERROR";
            }

            return "IPC.HTTP.NON_SUCCESS";
        }

        private static string ResolveErrorMessage(bool httpSuccess, ParsedResponseInfo parsed, int status)
        {
            if (!string.IsNullOrWhiteSpace(parsed.Message))
            {
                return parsed.Message;
            }

            if (httpSuccess && string.Equals(parsed.Status, "error", StringComparison.OrdinalIgnoreCase))
            {
                return "response payload indicated error";
            }

            return $"non-success response: {status}";
        }
    }
}
