using TMPro;
using UnityEngine;
namespace ProjectSpark.AAAArchitecture
{
    // Local presentation preferences. Existing global theme and saves are untouched.
    public sealed class SparkProfessionalSettings : ScriptableObject
    {
        [Tooltip("Optional project-owned TMP display font. Existing font is used if empty.")]
        public TMP_FontAsset displayFont;
        [Tooltip("Optional project-owned TMP body font. Existing font is used if empty.")]
        public TMP_FontAsset bodyFont;
        [Tooltip("Optional text-free 16:9 artwork. Assign your own licensed or original environment.")]
        public Sprite environment;
        [Tooltip("Optional text-free background for the equipment spotlight.")]
        public Sprite inspectionBackground;
        [Tooltip("Optional V7 showcase. Off by default: V8 restores the full mission grid.")]
        public bool useShowcaseLayout;
        [Header("V8 Neon Edition")]
        public bool neonEnabled = true;
        [Tooltip("Different engineering colors on the chapter rail.")]
        public bool colorfulChapters = true;
        [Tooltip("Different colors on individual mission cards, without altering their artwork or access state.")]
        public bool colorfulMissions = true;
        [Range(.15f,1f)] public float neonIntensity = .88f;
    }
}
