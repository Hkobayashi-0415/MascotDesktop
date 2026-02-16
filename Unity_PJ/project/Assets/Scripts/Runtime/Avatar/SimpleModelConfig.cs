using UnityEngine;

namespace MascotDesktop.Runtime.Avatar
{
    public sealed class SimpleModelConfig : MonoBehaviour
    {
        [Header("Asset Path")]
        [Tooltip("Relative path from Unity_PJ/data/assets_user or StreamingAssets.")]
        public string modelRelativePath = "characters/amane_kanata/official_v1/mmd_pkg/mmd/amane_kanata.pmx";

        [Header("Policy")]
        public bool forbidLegacyPath = true;
        public bool warnOnNonAsciiPath = true;

        [Header("Lighting")]
        [Tooltip("If false, SimpleModelBootstrap does not override scene light settings.")]
        public bool autoConfigureSceneLight = false;
        [Tooltip("When autoConfigureSceneLight is false, disable currently enabled scene lights at startup.")]
        public bool disableExistingSceneLightsWhenAutoConfigOff = true;
        [Tooltip("If true and no light exists, create SimpleModelLight before applying lighting settings.")]
        public bool createSceneLightIfMissing = false;

        [Header("Fallback View")]
        public Color fallbackColor = new Color(0.10f, 0.62f, 1.0f, 1.0f);
        public Vector3 fallbackScale = new Vector3(1.6f, 2.2f, 1.0f);
    }
}

