using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MascotDesktop.Runtime.Config;
using MascotDesktop.Runtime.Ipc;
using NUnit.Framework;
using UnityEngine;

namespace MascotDesktop.Tests.EditMode
{
    public sealed class LoopbackHttpClientTests
    {
        private GameObject _gameObject;
        private RuntimeConfig _runtimeConfig;
        private LoopbackHttpClient _client;

        [SetUp]
        public void SetUp()
        {
            _gameObject = new GameObject("LoopbackHttpClientTests");
            _runtimeConfig = _gameObject.AddComponent<RuntimeConfig>();
            _runtimeConfig.enableHttpBridge = false;
            _runtimeConfig.loopbackBaseUrl = "http://127.0.0.1:18080";
            _runtimeConfig.httpTimeoutMs = 2000;
            _client = _gameObject.AddComponent<LoopbackHttpClient>();
            InjectRuntimeConfig(_runtimeConfig);
        }

        [TearDown]
        public void TearDown()
        {
            if (_gameObject != null)
            {
                Object.DestroyImmediate(_gameObject);
            }
        }

        [Test]
        public async Task PostJsonAsync_DisabledBridge_ReturnsDisabledError()
        {
            var result = await _client.PostJsonAsync("/health", "req-bridge-001", "{\"probe\":true}");

            Assert.That(result.Success, Is.False);
            Assert.That(result.RequestId, Is.EqualTo("req-bridge-001"));
            Assert.That(result.ErrorCode, Is.EqualTo("IPC.HTTP.DISABLED"));
            Assert.That(result.StatusCode, Is.EqualTo(0));
        }

        [Test]
        public async Task PostJsonAsync_InjectsRequestIdIntoHeaderAndBody_WhenMissingInBody()
        {
            _runtimeConfig.enableHttpBridge = true;
            var handler = new CapturingHandler(HttpStatusCode.OK, "{\"status\":\"ok\"}");
            ReplaceHttpClient(handler);

            const string requestId = "req-bridge-002";
            var result = await _client.PostJsonAsync("/health", requestId, "{\"dto_version\":\"1.0.0\"}");

            Assert.That(result.Success, Is.True);
            Assert.That(result.RequestId, Is.EqualTo(requestId));
            Assert.That(handler.LastMethod, Is.EqualTo(HttpMethod.Post));
            Assert.That(handler.LastUrl, Is.EqualTo("http://127.0.0.1:18080/health"));
            Assert.That(handler.LastRequestIdHeader, Is.EqualTo(requestId));
            Assert.That(handler.LastBody, Does.Contain("\"request_id\":\"req-bridge-002\""));
            Assert.That(handler.LastBody, Does.Contain("\"payload\""));
        }

        [Test]
        public async Task PostJsonAsync_GeneratesRequestId_WhenMissing()
        {
            _runtimeConfig.enableHttpBridge = true;
            var handler = new CapturingHandler(HttpStatusCode.OK, "{\"status\":\"ok\"}");
            ReplaceHttpClient(handler);

            var result = await _client.PostJsonAsync("/health", null, "{}");

            Assert.That(result.Success, Is.True);
            Assert.That(string.IsNullOrWhiteSpace(result.RequestId), Is.False);
            Assert.That(handler.LastRequestIdHeader, Is.EqualTo(result.RequestId));
            Assert.That(handler.LastBody, Does.Contain("\"request_id\":\"" + result.RequestId + "\""));
        }

        [Test]
        public async Task PostJsonAsync_NonSuccess_UsesErrorCodeAndRetryableFromResponseBody()
        {
            _runtimeConfig.enableHttpBridge = true;
            const string requestId = "req-bridge-003";
            var handler = new CapturingHandler(
                HttpStatusCode.ServiceUnavailable,
                "{\"status\":\"error\",\"request_id\":\"req-bridge-003\",\"error_code\":\"CORE.TIMEOUT\",\"message\":\"upstream timeout\",\"retryable\":true}");
            ReplaceHttpClient(handler);

            UnityEngine.TestTools.LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex(".*CORE\\.TIMEOUT.*"));
            var result = await _client.PostJsonAsync("/v1/chat/send", requestId, "{\"payload\":{\"text\":\"hello\"}}");

            Assert.That(result.Success, Is.False);
            Assert.That(result.RequestId, Is.EqualTo(requestId));
            Assert.That(result.ResponseRequestId, Is.EqualTo(requestId));
            Assert.That(result.ErrorCode, Is.EqualTo("CORE.TIMEOUT"));
            Assert.That(result.Message, Is.EqualTo("upstream timeout"));
            Assert.That(result.Retryable, Is.True);
        }

        [Test]
        public async Task PostJsonAsync_ResponseRequestIdMismatch_ReturnsMismatchError()
        {
            _runtimeConfig.enableHttpBridge = true;
            const string requestId = "req-bridge-004";
            var handler = new CapturingHandler(
                HttpStatusCode.OK,
                "{\"status\":\"ok\",\"request_id\":\"req-bridge-other\"}");
            ReplaceHttpClient(handler);

            UnityEngine.TestTools.LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex(".*IPC\\.HTTP\\.REQUEST_ID_MISMATCH.*"));
            var result = await _client.PostJsonAsync("/health", requestId, "{\"probe\":true}");

            Assert.That(result.Success, Is.False);
            Assert.That(result.RequestId, Is.EqualTo(requestId));
            Assert.That(result.ResponseRequestId, Is.EqualTo("req-bridge-other"));
            Assert.That(result.ErrorCode, Is.EqualTo("IPC.HTTP.REQUEST_ID_MISMATCH"));
            Assert.That(result.Retryable, Is.False);
        }

        private void ReplaceHttpClient(CapturingHandler handler)
        {
            var field = typeof(LoopbackHttpClient).GetField("_httpClient", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null);

            var replacement = new HttpClient(handler);
            field.SetValue(_client, replacement);
        }

        private void InjectRuntimeConfig(RuntimeConfig config)
        {
            var field = typeof(LoopbackHttpClient).GetField("runtimeConfig", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null);
            field.SetValue(_client, config);
        }

        private sealed class CapturingHandler : HttpMessageHandler
        {
            private readonly HttpStatusCode _statusCode;
            private readonly string _body;

            public HttpMethod LastMethod { get; private set; }
            public string LastUrl { get; private set; } = string.Empty;
            public string LastRequestIdHeader { get; private set; } = string.Empty;
            public string LastBody { get; private set; } = string.Empty;

            public CapturingHandler(HttpStatusCode statusCode, string body)
            {
                _statusCode = statusCode;
                _body = body ?? string.Empty;
            }

            protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                LastMethod = request.Method;
                LastUrl = request.RequestUri?.ToString() ?? string.Empty;
                LastRequestIdHeader = request.Headers.Contains("X-Request-Id")
                    ? string.Join(",", request.Headers.GetValues("X-Request-Id"))
                    : string.Empty;
                LastBody = request.Content == null ? string.Empty : await request.Content.ReadAsStringAsync();

                return new HttpResponseMessage(_statusCode)
                {
                    Content = new StringContent(_body, Encoding.UTF8, "application/json")
                };
            }
        }
    }
}
