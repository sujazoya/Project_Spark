using UnityEngine;

namespace ProjectSpark.UI.VFX
{
    [CreateAssetMenu(
        fileName = "SparkVFXProfile",
        menuName = "Project Spark/UI VFX/VFX Profile"
    )]
    public sealed class SparkVFXProfile : ScriptableObject
    {
        // ============================================================
        // IDENTITY
        // ============================================================

        [Header("Profile")]

        [SerializeField]
        private string profileID = "Default";


        // ============================================================
        // THEME
        // ============================================================

        [Header("Theme")]

        [SerializeField]
        private bool useThemeColor = true;


        [SerializeField]
        private Color customColor = Color.white;


        // ============================================================
        // GLOW
        // ============================================================

        [Header("Glow")]

        [Range(0f, 5f)]
        [SerializeField]
        private float glow = 0f;


        // ============================================================
        // SCAN
        // ============================================================

        [Header("Scan")]

        [Range(0f, 5f)]
        [SerializeField]
        private float scan = 0f;


        // ============================================================
        // SWEEP
        // ============================================================

        [Header("Sweep")]

        [Range(0f, 5f)]
        [SerializeField]
        private float sweep = 0f;


        [Range(-1f, 2f)]
        [SerializeField]
        private float sweepPosition = 0f;


        // ============================================================
        // FLASH
        // ============================================================

        [Header("Flash")]

        [Range(0f, 5f)]
        [SerializeField]
        private float flash = 0f;


        // ============================================================
        // GLITCH
        // ============================================================

        [Header("Glitch")]

        [Range(0f, 1f)]
        [SerializeField]
        private float glitch = 0f;


        // ============================================================
        // FLICKER
        // ============================================================

        [Header("Flicker")]

        [Range(0f, 1f)]
        [SerializeField]
        private float flicker = 0f;


        // ============================================================
        // REVEAL
        // ============================================================

        [Header("Reveal")]

        [Range(0f, 1f)]
        [SerializeField]
        private float reveal = 0f;


        // ============================================================
        // DISSOLVE
        // ============================================================

        [Header("Dissolve")]

        [Range(0f, 1f)]
        [SerializeField]
        private float dissolve = 0f;


        // ============================================================
        // ANIMATION
        // ============================================================

        [Header("Animation")]

        [Min(0f)]
        [SerializeField]
        private float transitionDuration = 0.2f;


        [SerializeField]
        private AnimationCurve transitionCurve =
            AnimationCurve.EaseInOut(
                0f,
                0f,
                1f,
                1f
            );


        // ============================================================
        // PUBLIC ACCESS
        // ============================================================

        public string ProfileID
        {
            get
            {
                return profileID;
            }
        }


        public bool UseThemeColor
        {
            get
            {
                return useThemeColor;
            }
        }


        public Color CustomColor
        {
            get
            {
                return customColor;
            }
        }


        public float Glow
        {
            get
            {
                return glow;
            }
        }


        public float Scan
        {
            get
            {
                return scan;
            }
        }


        public float Sweep
        {
            get
            {
                return sweep;
            }
        }


        public float SweepPosition
        {
            get
            {
                return sweepPosition;
            }
        }


        public float Flash
        {
            get
            {
                return flash;
            }
        }


        public float Glitch
        {
            get
            {
                return glitch;
            }
        }


        public float Flicker
        {
            get
            {
                return flicker;
            }
        }


        public float Reveal
        {
            get
            {
                return reveal;
            }
        }


        public float Dissolve
        {
            get
            {
                return dissolve;
            }
        }


        public float TransitionDuration
        {
            get
            {
                return transitionDuration;
            }
        }


        public AnimationCurve TransitionCurve
        {
            get
            {
                return transitionCurve;
            }
        }
    }
}