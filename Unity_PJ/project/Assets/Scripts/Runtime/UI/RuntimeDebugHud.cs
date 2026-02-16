using MascotDesktop.Runtime.Avatar;
using MascotDesktop.Runtime.Config;
using MascotDesktop.Runtime.Core;
using MascotDesktop.Runtime.Windowing;
using System;
using UnityEngine;

namespace MascotDesktop.Runtime.UI
{
    public sealed class RuntimeDebugHud : MonoBehaviour
    {
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
        private SimpleModelBootstrap _simpleModelBootstrap;
        private string[] _modelCandidates = Array.Empty<string>();
        private string[] _imageCandidates = Array.Empty<string>();
        private int _modelIndex = -1;
        private CandidateMode _candidateMode = CandidateMode.Model;
        private string[] _renderFactorNames = Array.Empty<string>();
        private int _renderFactorIndex = -1;

        private void Awake()
        {
            CacheDependencies();
        }

        private void OnGUI()
        {
            if (!showHud)
            {
                return;
            }

            CacheDependencies();

            GUILayout.BeginArea(new Rect(12f, 12f, 420f, 580f), GUI.skin.box);
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
            GUILayout.Label($"HTTP Bridge: {_runtimeConfig?.enableHttpBridge.ToString() ?? "n/a"}");
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
                RescanModelCandidates();
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
                _coreOrchestrator?.SendChat("hello from runtime hud");
            }

            if (GUILayout.Button("Chat: happy", GUILayout.Height(26f)))
            {
                _coreOrchestrator?.SendChat("I am happy");
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
                _coreOrchestrator?.RequestMotionSlot("idle");
                _motionSlotPlayer?.PlaySlot("idle");
            }

            if (GUILayout.Button("Motion: wave", GUILayout.Height(26f)))
            {
                _coreOrchestrator?.RequestMotionSlot("wave");
                _motionSlotPlayer?.PlaySlot("wave");
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

            if (_simpleModelBootstrap == null)
            {
                _simpleModelBootstrap = FindFirstObjectByType<SimpleModelBootstrap>();
            }
        }

        private void EnsureModelCandidates()
        {
            var activeCandidates = GetActiveCandidates();
            if (activeCandidates.Length == 0)
            {
                RescanModelCandidates();
                activeCandidates = GetActiveCandidates();
                if (activeCandidates.Length == 0)
                {
                    _modelIndex = -1;
                    return;
                }
                return;
            }

            if (_modelIndex < 0 || _modelIndex >= activeCandidates.Length)
            {
                SyncCurrentModelIndex();
            }
        }

        private void RescanModelCandidates()
        {
            if (_simpleModelBootstrap == null)
            {
                _modelCandidates = Array.Empty<string>();
                _imageCandidates = Array.Empty<string>();
            }
            else
            {
                _modelCandidates = _simpleModelBootstrap.DiscoverModelCandidates();
                _imageCandidates = _simpleModelBootstrap.DiscoverImageCandidates();
            }

            SyncCurrentModelIndex();
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
    }
}
