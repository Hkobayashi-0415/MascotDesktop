using MascotDesktop.Runtime.Avatar;
using MascotDesktop.Runtime.Config;
using MascotDesktop.Runtime.Core;
using MascotDesktop.Runtime.Diagnostics;
using MascotDesktop.Runtime.Ipc;
using MascotDesktop.Runtime.Windowing;
using System;
using System.Threading.Tasks;
using UnityEngine;

namespace MascotDesktop.Runtime.UI
{
    public sealed class RuntimeDebugHud : MonoBehaviour
    {
        private const float AutoRescanIntervalSeconds = 0.5f;
        private const float MissingBootstrapLogIntervalSeconds = 2f;
        private const float HudMargin = 12f;
        private const float HudMaxWidth = 420f;
        private const float HudMinHeight = 260f;
        private const float HudFallbackHeight = 700f;

        private enum CandidateMode
        {
            Model,
            Image
        }

        [SerializeField] private bool showHud = true;

        private CoreOrchestrator _coreOrchestrator;
        private MotionSlotPlayer _motionSlotPlayer;
        private AvatarStateController _avatarStateController;
        private SimpleModelConfig _modelConfig;
        private WindowController _windowController;
        private ResidentController _residentController;
        private RuntimeConfig _runtimeConfig;
        private LoopbackHttpClient _loopbackHttpClient;
        private SimpleModelBootstrap _simpleModelBootstrap;
        private string[] _modelCandidates = Array.Empty<string>();
        private string[] _imageCandidates = Array.Empty<string>();
        private int _modelIndex = -1;
        private CandidateMode _candidateMode = CandidateMode.Model;
        private string[] _renderFactorNames = Array.Empty<string>();
        private int _renderFactorIndex = -1;
        private string _lastBridgeStatus = "n/a";
        private string _lastBridgeRequestId = "n/a";
        private float _nextAutoRescanAt;
        private float _nextMissingBootstrapLogAt;
        private Vector2 _scrollPosition = Vector2.zero;
        private int _lastLoggedModelCandidateCount = -1;
        private int _lastLoggedImageCandidateCount = -1;

        private void Awake()
        {
            CacheDependencies();
        }

        private void Update()
        {
            if (!showHud)
            {
                return;
            }

            CacheDependencies();
            EnsureAutoCandidateRescan();
        }

        private void OnGUI()
        {
            if (!showHud)
            {
                return;
            }

            CacheDependencies();

            var areaWidth = Mathf.Min(HudMaxWidth, Mathf.Max(320f, Screen.width - (HudMargin * 2f)));
            var computedHeight = Screen.height - (HudMargin * 2f);
            var areaHeight = computedHeight > 0f ? Mathf.Max(HudMinHeight, computedHeight) : HudFallbackHeight;

            GUILayout.BeginArea(new Rect(HudMargin, HudMargin, areaWidth, areaHeight), GUI.skin.box);
            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, false, true);
            GUILayout.Label("MascotDesktop Runtime HUD");
            GUILayout.Space(8f);
            GUILayout.Label($"Avatar State: {_avatarStateController?.CurrentState ?? "n/a"}");
            GUILayout.Label($"Motion Slot: {_motionSlotPlayer?.CurrentSlot ?? "n/a"}");
            GUILayout.Label($"Topmost: {_windowController?.IsTopmost.ToString() ?? "n/a"}");
            GUILayout.Label($"Resident Hidden: {_residentController?.IsHidden.ToString() ?? "n/a"}");
            GUILayout.Label($"Model Path: {_modelConfig?.modelRelativePath ?? "n/a"}");
            EnsureModelCandidates();
            GUILayout.Label($"Model Candidates: {_modelCandidates.Length}");
            GUILayout.Label($"Image Candidates: {_imageCandidates.Length}");
            GUILayout.Label($"Candidate Mode: {GetCandidateModeLabel()}");
            GUILayout.Label("Rescan updates candidate list only");
            GUILayout.Label($"Model Index: {FormatModelIndex()}");
            EnsureRenderFactors();
            GUILayout.Label($"Render Factor: {FormatRenderFactor()}");
            GUILayout.Label(GetWindowOpsCapabilityLabel());
            GUILayout.Label($"HTTP Bridge: {_runtimeConfig?.enableHttpBridge.ToString() ?? "n/a"}");
            GUILayout.Label($"Runtime Mode: {_runtimeConfig?.runtimeMode.ToString() ?? "n/a"}");
            GUILayout.Label($"Bridge Last: {_lastBridgeStatus}");
            GUILayout.Label($"Bridge RequestId: {_lastBridgeRequestId}");
            GUILayout.Label($"Degraded: {_coreOrchestrator?.DegradedSummary ?? "n/a"}");
            if (IsAdminDebugEnabled())
            {
                GUILayout.Label($"Degraded detail: llm={_coreOrchestrator?.IsFeatureDegraded("llm")}, tts={_coreOrchestrator?.IsFeatureDegraded("tts")}, stt={_coreOrchestrator?.IsFeatureDegraded("stt")}");
            }
            GUILayout.Space(8f);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Model: prev", GUILayout.Height(26f)))
            {
                SwitchModel(-1);
            }

            if (GUILayout.Button("Model: next", GUILayout.Height(26f)))
            {
                SwitchModel(+1);
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Mode: models", GUILayout.Height(26f)))
            {
                SetCandidateMode(CandidateMode.Model);
            }

            if (GUILayout.Button("Mode: images", GUILayout.Height(26f)))
            {
                SetCandidateMode(CandidateMode.Image);
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Model: reload", GUILayout.Height(26f)))
            {
                _simpleModelBootstrap?.ReloadModel();
            }

            if (GUILayout.Button("Model: rescan(list)", GUILayout.Height(26f)))
            {
                RescanModelCandidates(forceLog: true, forceRefresh: true);
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Factor: prev", GUILayout.Height(26f)))
            {
                SwitchRenderFactor(-1);
            }

            if (GUILayout.Button("Factor: next", GUILayout.Height(26f)))
            {
                SwitchRenderFactor(+1);
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(8f);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Chat: hello", GUILayout.Height(26f)))
            {
                TriggerChatWithBridge("hello from runtime hud");
            }

            if (GUILayout.Button("Chat: happy", GUILayout.Height(26f)))
            {
                TriggerChatWithBridge("I am happy");
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("STT: partial", GUILayout.Height(26f)))
            {
                TriggerSttPartial();
            }

            if (GUILayout.Button("STT: final happy", GUILayout.Height(26f)))
            {
                TriggerSttFinalHappy();
            }

            if (GUILayout.Button("STT: final empty", GUILayout.Height(26f)))
            {
                TriggerSttFinalEmpty();
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Bridge: health", GUILayout.Height(26f)))
            {
                TriggerBridgeHealth();
            }

            if (GUILayout.Button("Bridge: config/get", GUILayout.Height(26f)))
            {
                TriggerBridgeConfigGet();
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("State: idle", GUILayout.Height(26f)))
            {
                _coreOrchestrator?.ApplyAvatarState("idle");
            }

            if (GUILayout.Button("State: happy", GUILayout.Height(26f)))
            {
                _coreOrchestrator?.ApplyAvatarState("happy");
            }

            if (GUILayout.Button("State: sleepy", GUILayout.Height(26f)))
            {
                _coreOrchestrator?.ApplyAvatarState("sleepy");
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Motion: idle", GUILayout.Height(26f)))
            {
                TriggerMotionSlot("idle");
            }

            if (GUILayout.Button("Motion: wave", GUILayout.Height(26f)))
            {
                TriggerMotionSlot("wave");
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Toggle Topmost", GUILayout.Height(26f)))
            {
                _windowController?.ToggleTopmost();
            }

            if (GUILayout.Button("Drag Window", GUILayout.Height(26f)))
            {
                _windowController?.DragWindow();
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Hide/Show", GUILayout.Height(26f)))
            {
                _residentController?.ToggleResidentVisibility();
            }

            if (GUILayout.Button("Exit", GUILayout.Height(26f)))
            {
                _residentController?.ExitApplication();
            }
            GUILayout.EndHorizontal();

            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        private void CacheDependencies()
        {
            if (_coreOrchestrator == null)
            {
                _coreOrchestrator = FindFirstObjectByType<CoreOrchestrator>();
            }

            if (_motionSlotPlayer == null)
            {
                _motionSlotPlayer = FindFirstObjectByType<MotionSlotPlayer>();
            }

            if (_avatarStateController == null)
            {
                _avatarStateController = FindFirstObjectByType<AvatarStateController>();
            }

            if (_modelConfig == null)
            {
                _modelConfig = FindFirstObjectByType<SimpleModelConfig>();
            }

            if (_windowController == null)
            {
                _windowController = FindFirstObjectByType<WindowController>();
            }

            if (_residentController == null)
            {
                _residentController = FindFirstObjectByType<ResidentController>();
            }

            if (_runtimeConfig == null)
            {
                _runtimeConfig = FindFirstObjectByType<RuntimeConfig>();
            }

            if (_loopbackHttpClient == null)
            {
                _loopbackHttpClient = FindFirstObjectByType<LoopbackHttpClient>();
            }

            if (_simpleModelBootstrap == null)
            {
                _simpleModelBootstrap = FindFirstObjectByType<SimpleModelBootstrap>();
            }

            if (_simpleModelBootstrap != null)
            {
                var bootstrapObject = _simpleModelBootstrap.gameObject;
                _coreOrchestrator ??= bootstrapObject.GetComponent<CoreOrchestrator>();
                _motionSlotPlayer ??= bootstrapObject.GetComponent<MotionSlotPlayer>();
                _avatarStateController ??= bootstrapObject.GetComponent<AvatarStateController>();
                _modelConfig ??= bootstrapObject.GetComponent<SimpleModelConfig>();
                _windowController ??= bootstrapObject.GetComponent<WindowController>();
                _residentController ??= bootstrapObject.GetComponent<ResidentController>();
                _runtimeConfig ??= bootstrapObject.GetComponent<RuntimeConfig>();
                _loopbackHttpClient ??= bootstrapObject.GetComponent<LoopbackHttpClient>();
            }
        }

        private void EnsureModelCandidates()
        {
            var activeCandidates = GetActiveCandidates();
            if (activeCandidates.Length == 0)
            {
                _modelIndex = -1;
                return;
            }

            if (_modelIndex < 0 || _modelIndex >= activeCandidates.Length)
            {
                SyncCurrentModelIndex();
            }
        }

        private void EnsureAutoCandidateRescan()
        {
            if (Time.unscaledTime < _nextAutoRescanAt)
            {
                return;
            }

            if (GetActiveCandidates().Length > 0)
            {
                return;
            }

            _nextAutoRescanAt = Time.unscaledTime + AutoRescanIntervalSeconds;
            RescanModelCandidates();
        }

        private void RescanModelCandidates(bool forceLog = false, bool forceRefresh = false)
        {
            if (_simpleModelBootstrap == null)
            {
                CacheDependencies();
            }

            if (_simpleModelBootstrap == null)
            {
                _modelCandidates = Array.Empty<string>();
                _imageCandidates = Array.Empty<string>();
                if (Time.unscaledTime >= _nextMissingBootstrapLogAt)
                {
                    _nextMissingBootstrapLogAt = Time.unscaledTime + MissingBootstrapLogIntervalSeconds;
                    RuntimeLog.Warn(
                        "ui",
                        "ui.hud.bootstrap_missing",
                        RuntimeLog.NewRequestId(),
                        "UI.HUD.BOOTSTRAP_MISSING",
                        "simple model bootstrap is missing; candidate discovery skipped",
                        string.Empty,
                        "runtime_hud");
                }
            }
            else
            {
                _modelCandidates = _simpleModelBootstrap.DiscoverModelCandidates(forceRefresh);
                _imageCandidates = _simpleModelBootstrap.DiscoverImageCandidates(forceRefresh);
            }

            SyncCurrentModelIndex();
            var candidateCountsChanged =
                _modelCandidates.Length != _lastLoggedModelCandidateCount ||
                _imageCandidates.Length != _lastLoggedImageCandidateCount;
            if (forceLog || candidateCountsChanged)
            {
                _lastLoggedModelCandidateCount = _modelCandidates.Length;
                _lastLoggedImageCandidateCount = _imageCandidates.Length;
                RuntimeLog.Info(
                    "ui",
                    "ui.hud.model_candidates_rescanned",
                    RuntimeLog.NewRequestId(),
                    $"candidate lists updated: models={_modelCandidates.Length}, images={_imageCandidates.Length}, mode={GetCandidateModeLabel()}",
                    string.Empty,
                    "runtime_hud");
            }
        }

        private void EnsureRenderFactors()
        {
            if (_simpleModelBootstrap == null)
            {
                _renderFactorNames = Array.Empty<string>();
                _renderFactorIndex = -1;
                return;
            }

            if (_renderFactorNames.Length == 0)
            {
                _renderFactorNames = _simpleModelBootstrap.GetRenderFactorNames();
            }

            _renderFactorIndex = _simpleModelBootstrap.GetRenderFactorIndex();
        }

        private void SyncCurrentModelIndex()
        {
            _modelIndex = -1;
            var activeCandidates = GetActiveCandidates();
            if (_modelConfig == null || activeCandidates.Length == 0)
            {
                return;
            }

            var current = (_modelConfig.modelRelativePath ?? string.Empty).Trim();
            for (var i = 0; i < activeCandidates.Length; i++)
            {
                if (string.Equals(activeCandidates[i], current, StringComparison.OrdinalIgnoreCase))
                {
                    _modelIndex = i;
                    return;
                }
            }
        }

        private string FormatModelIndex()
        {
            var activeCandidates = GetActiveCandidates();
            if (activeCandidates.Length == 0 || _modelIndex < 0)
            {
                return "n/a";
            }

            return $"{_modelIndex + 1}/{activeCandidates.Length}";
        }

        private string FormatRenderFactor()
        {
            if (_renderFactorNames.Length == 0 || _renderFactorIndex < 0 || _renderFactorIndex >= _renderFactorNames.Length)
            {
                return "n/a";
            }

            return $"{_renderFactorNames[_renderFactorIndex]} ({_renderFactorIndex + 1}/{_renderFactorNames.Length})";
        }

        private void SwitchModel(int direction)
        {
            var activeCandidates = GetActiveCandidates();
            if (_simpleModelBootstrap == null || activeCandidates.Length == 0)
            {
                RuntimeLog.Warn(
                    "ui",
                    "ui.hud.model_switch_skipped",
                    RuntimeLog.NewRequestId(),
                    "UI.HUD.MODEL_CANDIDATES_EMPTY",
                    "model switch skipped because candidate list is empty",
                    string.Empty,
                    "runtime_hud");
                return;
            }

            if (_modelIndex < 0 || _modelIndex >= activeCandidates.Length)
            {
                SyncCurrentModelIndex();
            }

            if (_modelIndex < 0)
            {
                _modelIndex = direction >= 0 ? 0 : activeCandidates.Length - 1;
                _simpleModelBootstrap.LoadModelByRelativePath(activeCandidates[_modelIndex]);
                RuntimeLog.Info(
                    "ui",
                    "ui.hud.model_switched",
                    RuntimeLog.NewRequestId(),
                    $"model switched to index={_modelIndex + 1}/{activeCandidates.Length}",
                    activeCandidates[_modelIndex],
                    "runtime_hud");
                return;
            }

            var next = _modelIndex + direction;
            if (next < 0)
            {
                next = activeCandidates.Length - 1;
            }
            else if (next >= activeCandidates.Length)
            {
                next = 0;
            }

            _modelIndex = next;
            _simpleModelBootstrap.LoadModelByRelativePath(activeCandidates[_modelIndex]);
            RuntimeLog.Info(
                "ui",
                "ui.hud.model_switched",
                RuntimeLog.NewRequestId(),
                $"model switched to index={_modelIndex + 1}/{activeCandidates.Length}",
                activeCandidates[_modelIndex],
                "runtime_hud");
        }

        private string[] GetActiveCandidates()
        {
            return _candidateMode == CandidateMode.Image ? _imageCandidates : _modelCandidates;
        }

        private string GetCandidateModeLabel()
        {
            return _candidateMode == CandidateMode.Image ? "Image" : "Model";
        }

        private void SetCandidateMode(CandidateMode mode)
        {
            if (_candidateMode == mode)
            {
                return;
            }

            _candidateMode = mode;
            SyncCurrentModelIndex();
            RuntimeLog.Info(
                "ui",
                "ui.hud.candidate_mode_changed",
                RuntimeLog.NewRequestId(),
                $"candidate mode changed to {GetCandidateModeLabel()}",
                string.Empty,
                "runtime_hud");
        }

        private void TriggerMotionSlot(string slotName)
        {
            var requestId = RuntimeLog.NewRequestId();
            if (_coreOrchestrator != null)
            {
                _coreOrchestrator.RequestMotionSlot(slotName, requestId);
                return;
            }

            _motionSlotPlayer?.PlaySlot(slotName, requestId);
        }

        private static string GetWindowOpsCapabilityLabel()
        {
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
            return "Window Ops: native (Windows player)";
#else
            return "Window Ops: editor simulation (native effect requires Windows player)";
#endif
        }

        private void SwitchRenderFactor(int direction)
        {
            if (_simpleModelBootstrap == null)
            {
                return;
            }

            EnsureRenderFactors();
            if (_renderFactorNames.Length == 0)
            {
                return;
            }

            if (_renderFactorIndex < 0 || _renderFactorIndex >= _renderFactorNames.Length)
            {
                _renderFactorIndex = 0;
            }

            var next = _renderFactorIndex + direction;
            if (next < 0)
            {
                next = _renderFactorNames.Length - 1;
            }
            else if (next >= _renderFactorNames.Length)
            {
                next = 0;
            }

            _simpleModelBootstrap.SetRenderFactorIndex(next);
            _renderFactorIndex = next;
        }

        private void TriggerChatWithBridge(string text)
        {
            _ = SendChatWithBridgeAsync(text);
        }

        private void TriggerBridgeHealth()
        {
            const string payload = "{\"dto_version\":\"1.0.0\",\"payload\":{\"probe\":\"runtime_hud\"}}";
            _ = PostBridgeOnlyAsync("/health", payload, "ipc.hud.health");
        }

        private void TriggerBridgeConfigGet()
        {
            const string payload = "{\"dto_version\":\"1.0.0\",\"payload\":{\"keys\":[\"topmost\",\"clickthrough\"]}}";
            _ = PostBridgeOnlyAsync("/v1/config/get", payload, "ipc.hud.config_get");
        }

        private void TriggerSttPartial()
        {
            _ = SendSttEventWithBridgeAsync("i am", isFinal: false);
        }

        private void TriggerSttFinalHappy()
        {
            _ = SendSttEventWithBridgeAsync("i am happy", isFinal: true);
        }

        private void TriggerSttFinalEmpty()
        {
            _ = SendSttEventWithBridgeAsync(string.Empty, isFinal: true);
        }

        private async Task SendChatWithBridgeAsync(string text)
        {
            var requestId = RuntimeLog.NewRequestId();
            var message = text ?? string.Empty;
            var payload = "{\"dto_version\":\"1.0.0\",\"request_id\":\"" + EscapeJson(requestId) +
                          "\",\"payload\":{\"text\":\"" + EscapeJson(message) + "\"}}";

            var bridgeResult = await SendBridgeAsync("/v1/chat/send", requestId, payload, "ipc.hud.chat_bridge");

            // Phase A: すべての経路をSendChatWithBridgeResultに統一し、core.chat.bridge_fallback観測軸を一元化する。
            // bridgeResult == null は結果を取得できなかった場合（DISABLED/CLIENT_MISSING含む）として扱う。
            if (_coreOrchestrator != null)
            {
                bool succeeded;
                string errorCode;
                bool retryable;

                if (bridgeResult == null)
                {
                    // SendBridgeAsync が null を返した（bridge未接続など）
                    succeeded = false;
                    errorCode = "IPC.HUD.BRIDGE_RESULT_NULL";
                    retryable = false;
                }
                else
                {
                    succeeded = bridgeResult.Success;
                    errorCode = bridgeResult.ErrorCode;
                    retryable = bridgeResult.Retryable;
                }

                _coreOrchestrator.SendChatWithBridgeResult(
                    message,
                    requestId,
                    succeeded,
                    errorCode,
                    retryable,
                    bridgeErrorName: bridgeResult?.ErrorName ?? string.Empty,
                    coreRequestId: bridgeResult?.CoreRequestId ?? string.Empty,
                    attempt: bridgeResult?.Attempt ?? 0);
            }

            // Phase B: chat導線からTTS bridgeへ連結する。
            await SendTtsWithBridgeAsync(message, requestId);
        }

        private async Task SendTtsWithBridgeAsync(string text, string requestId)
        {
            var ttsRequestId = string.IsNullOrWhiteSpace(requestId) ? RuntimeLog.NewRequestId() : requestId;
            var ttsText = text ?? string.Empty;
            var payload = "{\"dto_version\":\"1.0.0\",\"request_id\":\"" + EscapeJson(ttsRequestId) +
                          "\",\"payload\":{\"text\":\"" + EscapeJson(ttsText) + "\"}}";

            var ttsBridgeResult = await SendBridgeAsync("/v1/tts/play", ttsRequestId, payload, "ipc.hud.tts_bridge");
            if (_coreOrchestrator == null)
            {
                return;
            }

            bool succeeded;
            string errorCode;
            bool retryable;
            if (ttsBridgeResult == null)
            {
                succeeded = false;
                errorCode = "IPC.HUD.BRIDGE_RESULT_NULL";
                retryable = false;
            }
            else
            {
                succeeded = ttsBridgeResult.Success;
                errorCode = ttsBridgeResult.ErrorCode;
                retryable = ttsBridgeResult.Retryable;
            }

            _coreOrchestrator.SendTtsWithBridgeResult(
                ttsText,
                ttsRequestId,
                succeeded,
                errorCode,
                retryable,
                bridgeErrorName: ttsBridgeResult?.ErrorName ?? string.Empty,
                coreRequestId: ttsBridgeResult?.CoreRequestId ?? string.Empty,
                attempt: ttsBridgeResult?.Attempt ?? 0);
        }

        private async Task SendSttEventWithBridgeAsync(string text, bool isFinal)
        {
            var sttRequestId = RuntimeLog.NewRequestId();
            var transcript = text ?? string.Empty;
            var payload = "{\"dto_version\":\"1.0.0\",\"request_id\":\"" + EscapeJson(sttRequestId) +
                          "\",\"payload\":{\"text\":\"" + EscapeJson(transcript) + "\",\"is_final\":" + (isFinal ? "true" : "false") + "}}";

            var sttBridgeResult = await SendBridgeAsync("/v1/stt/event", sttRequestId, payload, "ipc.hud.stt_bridge");
            if (_coreOrchestrator == null)
            {
                return;
            }

            bool succeeded;
            string errorCode;
            bool retryable;
            if (sttBridgeResult == null)
            {
                succeeded = false;
                errorCode = "IPC.HUD.BRIDGE_RESULT_NULL";
                retryable = false;
            }
            else
            {
                succeeded = sttBridgeResult.Success;
                errorCode = sttBridgeResult.ErrorCode;
                retryable = sttBridgeResult.Retryable;
            }

            _coreOrchestrator.SendSttWithBridgeResult(
                transcript,
                sttRequestId,
                isFinal,
                succeeded,
                errorCode,
                retryable,
                bridgeErrorName: sttBridgeResult?.ErrorName ?? string.Empty,
                coreRequestId: sttBridgeResult?.CoreRequestId ?? string.Empty,
                attempt: sttBridgeResult?.Attempt ?? 0);
        }

        private async Task PostBridgeOnlyAsync(string path, string payload, string eventName)
        {
            var requestId = RuntimeLog.NewRequestId();
            await SendBridgeAsync(path, requestId, payload, eventName);
        }

        private async Task<LoopbackHttpResult> SendBridgeAsync(string path, string requestId, string payload, string eventName)
        {
            CacheDependencies();

            if (_loopbackHttpClient == null)
            {
                var missingResult = new LoopbackHttpResult
                {
                    Success = false,
                    RequestId = requestId,
                    StatusCode = 0,
                    Body = string.Empty,
                    ErrorCode = "IPC.HTTP.CLIENT_MISSING",
                    Message = "loopback http client is missing"
                };
                UpdateBridgeResult(path, missingResult);
                RuntimeLog.Warn("ipc", eventName, requestId, missingResult.ErrorCode, missingResult.Message, path, "runtime_hud");
                return missingResult;
            }

            try
            {
                var result = await _loopbackHttpClient.PostJsonAsync(path, requestId, payload);
                UpdateBridgeResult(path, result);
                if (result.Success)
                {
                    RuntimeLog.Info("ipc", eventName, requestId, "bridge request succeeded", path, "runtime_hud");
                }
                else
                {
                    RuntimeLog.Warn("ipc", eventName, requestId, result.ErrorCode, result.Message, path, "runtime_hud");
                }

                return result;
            }
            catch (Exception ex)
            {
                var failedResult = new LoopbackHttpResult
                {
                    Success = false,
                    RequestId = requestId,
                    StatusCode = 0,
                    Body = string.Empty,
                    ErrorCode = "IPC.HUD.REQUEST_FAILED",
                    Message = ex.GetBaseException().Message
                };
                UpdateBridgeResult(path, failedResult);
                RuntimeLog.Error("ipc", eventName, requestId, failedResult.ErrorCode, "runtime hud bridge request failed", path, "runtime_hud", ex);
                return failedResult;
            }
        }

        private void UpdateBridgeResult(string path, LoopbackHttpResult result)
        {
            if (result == null)
            {
                _lastBridgeStatus = $"{path} [0] n/a";
                _lastBridgeRequestId = "n/a";
                return;
            }

            var state = result.Success ? "ok" : $"ng:{result.ErrorCode}";
            _lastBridgeStatus = $"{path} [{result.StatusCode}] {state}";
            _lastBridgeRequestId = string.IsNullOrWhiteSpace(result.RequestId) ? "n/a" : result.RequestId;
        }

        private bool IsAdminDebugEnabled()
        {
            return _runtimeConfig != null && _runtimeConfig.adminDebugMode;
        }

        private static string EscapeJson(string message)
        {
            return (message ?? string.Empty)
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"")
                .Replace("\r", "\\r")
                .Replace("\n", "\\n");
        }
    }
}
