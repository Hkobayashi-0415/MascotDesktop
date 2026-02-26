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
        public string CoreRequestId;
        public int StatusCode;
        public string Body;
        public string ErrorCode;
        public string ErrorName;
        public string Message;
        public bool Retryable;
        public int Attempt;
    }

    public sealed class LoopbackHttpClient : MonoBehaviour
    {
        [SerializeField] private RuntimeConfig runtimeConfig;

        private HttpClient _httpClient;
        private const float RetryBackoffBaseSeconds = 0.5f;
        private const float RetryBackoffFactor = 2.0f;
        private const float RetryBackoffMaxSeconds = 8.0f;
        private const float RetryJitterMin = 0.8f;
        private const float RetryJitterMax = 1.2f;

        [Serializable]
        private sealed class ResponseEnvelope
        {
            public string status;
            public string request_id;
            public string core_request_id;
            public string error_code;
            public string error_name;
            public string message;
            public bool retryable;
            public int attempt;
        }

        private sealed class ParsedResponseInfo
        {
            public string Status = string.Empty;
            public string RequestId = string.Empty;
            public string CoreRequestId = string.Empty;
            public string ErrorCode = string.Empty;
            public string ErrorName = string.Empty;
            public string Message = string.Empty;
            public bool Retryable;
            public bool HasRetryable;
            public int Attempt;
            public bool HasAttempt;
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
                    CoreRequestId = string.Empty,
                    StatusCode = 0,
                    Body = string.Empty,
                    ErrorCode = "IPC.HTTP.DISABLED",
                    ErrorName = "IPC_HTTP_DISABLED",
                    Message = "loopback bridge is disabled",
                    Retryable = false,
                    Attempt = 0
                };
            }

            var baseUrl = runtimeConfig.ResolveBridgeBaseUrl();
            var targetUrl = BuildTargetUrl(baseUrl, relativePath);
            var body = EnsureRequestIdInBody(jsonBody, rid);
            var timeoutMs = runtimeConfig.GetTimeoutMsForPath(relativePath);
            var maxRetries = runtimeConfig.GetMaxRetriesForPath(relativePath);
            var attempt = 0;

            while (true)
            {
                using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                linkedCts.CancelAfter(timeoutMs);

                RuntimeLog.Info(
                    "ipc",
                    "ipc.http.request",
                    rid,
                    $"POST {targetUrl}; attempt={attempt}",
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
                            CoreRequestId = parsed.CoreRequestId,
                            StatusCode = status,
                            Body = responseBody ?? string.Empty,
                            ErrorCode = "IPC.HTTP.REQUEST_ID_MISMATCH",
                            ErrorName = "IPC_HTTP_REQUEST_ID_MISMATCH",
                            Message = "response request_id mismatch",
                            Retryable = false,
                            Attempt = attempt
                        };
                    }

                    var payloadError = string.Equals(parsed.Status, "error", StringComparison.OrdinalIgnoreCase);
                    var success = response.IsSuccessStatusCode && !payloadError;
                    var errorCode = string.Empty;
                    var errorName = string.Empty;
                    var message = "ok";
                    var retryable = false;

                    if (!success)
                    {
                        errorCode = ResolveErrorCode(response.IsSuccessStatusCode, parsed);
                        errorName = ResolveErrorName(errorCode, parsed);
                        message = ResolveErrorMessage(response.IsSuccessStatusCode, parsed, status);
                        retryable = parsed.HasRetryable && parsed.Retryable;
                    }

                    if (success)
                    {
                        RuntimeLog.Info(
                            "ipc",
                            "ipc.http.response",
                            rid,
                            $"response status: {status}; attempt={attempt}",
                            relativePath ?? string.Empty,
                            "loopback_http");

                        return new LoopbackHttpResult
                        {
                            Success = true,
                            RequestId = rid,
                            ResponseRequestId = parsed.RequestId,
                            CoreRequestId = parsed.CoreRequestId,
                            StatusCode = status,
                            Body = responseBody ?? string.Empty,
                            ErrorCode = string.Empty,
                            ErrorName = string.Empty,
                            Message = "ok",
                            Retryable = false,
                            Attempt = parsed.HasAttempt ? parsed.Attempt : attempt
                        };
                    }

                    if (ShouldAutoRetry(status, errorCode) && attempt < maxRetries)
                    {
                        var retryAttempt = attempt + 1;
                        RuntimeLog.Warn(
                            "ipc",
                            "ipc.http.retry",
                            rid,
                            errorCode,
                            $"retry scheduled; path={relativePath}; attempt={retryAttempt}/{maxRetries}",
                            relativePath ?? string.Empty,
                            "loopback_http");

                        await DelayBeforeRetryAsync(retryAttempt, cancellationToken);
                        attempt = retryAttempt;
                        continue;
                    }

                    RuntimeLog.Error(
                        "ipc",
                        "ipc.http.response_failed",
                        rid,
                        errorCode,
                        $"{message}; attempt={attempt}",
                        relativePath ?? string.Empty,
                        "loopback_http");

                    return new LoopbackHttpResult
                    {
                        Success = false,
                        RequestId = rid,
                        ResponseRequestId = parsed.RequestId,
                        CoreRequestId = parsed.CoreRequestId,
                        StatusCode = status,
                        Body = responseBody ?? string.Empty,
                        ErrorCode = errorCode,
                        ErrorName = errorName,
                        Message = message,
                        Retryable = retryable,
                        Attempt = parsed.HasAttempt ? parsed.Attempt : attempt
                    };
                }
                catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested)
                {
                    const string timeoutCode = "IPC.HTTP.REQUEST_TIMEOUT";
                    if (attempt < maxRetries)
                    {
                        var retryAttempt = attempt + 1;
                        RuntimeLog.Warn(
                            "ipc",
                            "ipc.http.retry",
                            rid,
                            timeoutCode,
                            $"retry scheduled after timeout; path={relativePath}; attempt={retryAttempt}/{maxRetries}",
                            relativePath ?? string.Empty,
                            "loopback_http");

                        await DelayBeforeRetryAsync(retryAttempt, cancellationToken);
                        attempt = retryAttempt;
                        continue;
                    }

                    RuntimeLog.Error(
                        "ipc",
                        "ipc.http.request_failed",
                        rid,
                        timeoutCode,
                        "loopback http request timed out",
                        relativePath ?? string.Empty,
                        "loopback_http",
                        ex);

                    return new LoopbackHttpResult
                    {
                        Success = false,
                        RequestId = rid,
                        ResponseRequestId = string.Empty,
                        CoreRequestId = string.Empty,
                        StatusCode = 0,
                        Body = string.Empty,
                        ErrorCode = timeoutCode,
                        ErrorName = "IPC_HTTP_REQUEST_TIMEOUT",
                        Message = "request timeout",
                        Retryable = true,
                        Attempt = attempt
                    };
                }
                catch (Exception ex)
                {
                    const string failedCode = "IPC.HTTP.REQUEST_FAILED";
                    if (attempt < maxRetries && IsRetryableTransportException(ex))
                    {
                        var retryAttempt = attempt + 1;
                        RuntimeLog.Warn(
                            "ipc",
                            "ipc.http.retry",
                            rid,
                            failedCode,
                            $"retry scheduled after transport failure; path={relativePath}; attempt={retryAttempt}/{maxRetries}",
                            relativePath ?? string.Empty,
                            "loopback_http");

                        await DelayBeforeRetryAsync(retryAttempt, cancellationToken);
                        attempt = retryAttempt;
                        continue;
                    }

                    RuntimeLog.Error(
                        "ipc",
                        "ipc.http.request_failed",
                        rid,
                        failedCode,
                        "loopback http request failed",
                        relativePath ?? string.Empty,
                        "loopback_http",
                        ex);

                    return new LoopbackHttpResult
                    {
                        Success = false,
                        RequestId = rid,
                        ResponseRequestId = string.Empty,
                        CoreRequestId = string.Empty,
                        StatusCode = 0,
                        Body = string.Empty,
                        ErrorCode = failedCode,
                        ErrorName = "IPC_HTTP_REQUEST_FAILED",
                        Message = ex.GetBaseException().Message,
                        Retryable = true,
                        Attempt = attempt
                    };
                }
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
                parsed.CoreRequestId = envelope.core_request_id ?? string.Empty;
                parsed.ErrorCode = envelope.error_code ?? string.Empty;
                parsed.ErrorName = envelope.error_name ?? string.Empty;
                parsed.Message = envelope.message ?? string.Empty;
                parsed.Retryable = envelope.retryable;
                parsed.Attempt = envelope.attempt;
                parsed.HasRetryable = responseBody.IndexOf("\"retryable\"", StringComparison.OrdinalIgnoreCase) >= 0;
                parsed.HasAttempt = responseBody.IndexOf("\"attempt\"", StringComparison.OrdinalIgnoreCase) >= 0;
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

        private static string ResolveErrorName(string errorCode, ParsedResponseInfo parsed)
        {
            if (!string.IsNullOrWhiteSpace(parsed.ErrorName))
            {
                return parsed.ErrorName;
            }

            switch (errorCode)
            {
                case "IPC.HTTP.PAYLOAD_ERROR":
                    return "IPC_HTTP_PAYLOAD_ERROR";
                case "IPC.HTTP.NON_SUCCESS":
                    return "IPC_HTTP_NON_SUCCESS";
                case "IPC.HTTP.REQUEST_FAILED":
                    return "IPC_HTTP_REQUEST_FAILED";
                case "IPC.HTTP.REQUEST_TIMEOUT":
                    return "IPC_HTTP_REQUEST_TIMEOUT";
                default:
                    return string.Empty;
            }
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

        private static bool ShouldAutoRetry(int statusCode, string errorCode)
        {
            if (statusCode >= 500 && statusCode <= 599)
            {
                return true;
            }

            return string.Equals(errorCode, "IPC.HTTP.REQUEST_TIMEOUT", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(errorCode, "IPC.HTTP.REQUEST_FAILED", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsRetryableTransportException(Exception ex)
        {
            return ex is HttpRequestException ||
                   ex is TaskCanceledException ||
                   ex is TimeoutException;
        }

        private static async Task DelayBeforeRetryAsync(int retryAttempt, CancellationToken cancellationToken)
        {
            var exponent = Mathf.Pow((float)RetryBackoffFactor, Mathf.Max(0, retryAttempt - 1));
            var baseSeconds = RetryBackoffBaseSeconds * exponent;
            var jitter = UnityEngine.Random.Range(RetryJitterMin, RetryJitterMax);
            var delaySeconds = Mathf.Min(RetryBackoffMaxSeconds, baseSeconds) * jitter;
            var delayMs = Mathf.Max(1, Mathf.RoundToInt(delaySeconds * 1000f));
            await Task.Delay(delayMs, cancellationToken);
        }
    }
}
