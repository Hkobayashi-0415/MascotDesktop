using UnityEngine;

namespace MascotDesktop.Runtime.Config
{
    public sealed class RuntimeConfig : MonoBehaviour
    {
        [Header("HTTP Bridge")]
        [Tooltip("Loopback base URL for optional companion services.")]
        public string loopbackBaseUrl = "http://127.0.0.1:18080";

        [Tooltip("Timeout in milliseconds for loopback HTTP calls.")]
        public int httpTimeoutMs = 1500;

        [Tooltip("Enable outgoing loopback HTTP bridge calls.")]
        public bool enableHttpBridge;

        [Header("Runtime Defaults")]
        public string defaultAvatarState = "idle";
        public string defaultMotionSlot = "idle";
    }
}
