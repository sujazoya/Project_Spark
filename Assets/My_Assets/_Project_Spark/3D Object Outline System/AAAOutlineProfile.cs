using UnityEngine;

namespace AAAOutline
{
    [System.Serializable]
    public class AAAOutlineProfile
    {
        [Header("Appearance")]

        [Tooltip("HDR color used by the outline.")]
        [ColorUsage(true, true)]
        public Color Color = Color.cyan;

        [Min(0f)]
        [Tooltip("Expansion amount of the inverted hull.")]
        public float Width = 0.02f;

        [Min(0f)]
        [Tooltip("Overall brightness multiplier.")]
        public float Intensity = 1f;


        [Header("Fresnel")]

        [Tooltip("Enables a view-angle based Fresnel effect.")]
        public bool FresnelEnabled = false;

        [Min(0.01f)]
        [Tooltip("Controls the sharpness of the Fresnel effect.")]
        public float FresnelPower = 2f;

        [Min(0f)]
        [Tooltip("Controls Fresnel contribution.")]
        public float FresnelStrength = 1f;


        [Header("Pulse")]

        [Tooltip("Enables animated intensity pulsing.")]
        public bool PulseEnabled = false;

        [Min(0f)]
        [Tooltip("Pulse speed.")]
        public float PulseSpeed = 2f;

        [Min(0f)]
        [Tooltip("Minimum pulse multiplier.")]
        public float PulseMin = 0.75f;

        [Min(0f)]
        [Tooltip("Maximum pulse multiplier.")]
        public float PulseMax = 1.25f;


        [Header("Visibility")]

        [Tooltip("Controls how the outline interacts with scene depth.")]
        public OutlineVisibilityMode VisibilityMode =
            OutlineVisibilityMode.Occluded;


        public AAAOutlineProfile()
        {
            Color = Color.cyan;
            Width = 0.02f;
            Intensity = 1f;

            FresnelEnabled = false;
            FresnelPower = 2f;
            FresnelStrength = 1f;

            PulseEnabled = false;
            PulseSpeed = 2f;
            PulseMin = 0.75f;
            PulseMax = 1.25f;

            VisibilityMode = OutlineVisibilityMode.Occluded;
        }


        public AAAOutlineProfile Clone()
        {
            return new AAAOutlineProfile
            {
                Color = Color,
                Width = Width,
                Intensity = Intensity,

                FresnelEnabled = FresnelEnabled,
                FresnelPower = FresnelPower,
                FresnelStrength = FresnelStrength,

                PulseEnabled = PulseEnabled,
                PulseSpeed = PulseSpeed,
                PulseMin = PulseMin,
                PulseMax = PulseMax,

                VisibilityMode = VisibilityMode
            };
        }


        public void Sanitize()
        {
            Width = Mathf.Max(0f, Width);
            Intensity = Mathf.Max(0f, Intensity);

            FresnelPower = Mathf.Max(0.01f, FresnelPower);
            FresnelStrength = Mathf.Max(0f, FresnelStrength);

            PulseSpeed = Mathf.Max(0f, PulseSpeed);
            PulseMin = Mathf.Max(0f, PulseMin);
            PulseMax = Mathf.Max(PulseMin, PulseMax);
        }
    }
}