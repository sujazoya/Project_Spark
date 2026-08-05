using System.Collections;
using UnityEngine;

namespace ProjectSpark.UI.VFX
{
    [DisallowMultipleComponent]
    public sealed class SparkAdvancedGlowController
        : MonoBehaviour
    {
        // ============================================================
        // STATE
        // ============================================================

        public enum GlowState
        {
            Normal,
            Hover,
            Selected,
            Target,
            Warning,
            Error,
            Disabled
        }


        // ============================================================
        // RENDERER
        // ============================================================

        [Header("Renderer")]

        [SerializeField]
        private Renderer targetRenderer;


        [SerializeField]
        private bool findRendererAutomatically = true;


        // ============================================================
        // STATE
        // ============================================================

        [Header("State")]

        [SerializeField]
        private GlowState defaultState =
            GlowState.Normal;


        [SerializeField]
        private float transitionDuration =
            0.25f;


        [SerializeField]
        private AnimationCurve transitionCurve =
            AnimationCurve.EaseInOut(
                0f,
                0f,
                1f,
                1f
            );


        // ============================================================
        // BASE
        // ============================================================

        [Header("Base Glow")]

        [ColorUsage(true, true)]
        [SerializeField]
        private Color glowColor =
            new Color(
                0f,
                0.8f,
                1f,
                1f
            );


        [SerializeField]
        [Range(0f, 20f)]
        private float glowIntensity = 4f;


        // ============================================================
        // FRESNEL
        // ============================================================

        [Header("Fresnel")]

        [SerializeField]
        private bool fresnelEnabled = true;


        [ColorUsage(true, true)]
        [SerializeField]
        private Color fresnelColor =
            new Color(
                0f,
                0.5f,
                1f,
                1f
            );


        [SerializeField]
        [Range(0.1f, 10f)]
        private float fresnelPower = 3f;


        [SerializeField]
        [Range(0f, 20f)]
        private float fresnelIntensity = 3f;


        // ============================================================
        // PULSE
        // ============================================================

        [Header("Pulse")]

        [SerializeField]
        private float pulseIntensity = 3f;


        [SerializeField]
        private float pulseDuration = 0.35f;


        [SerializeField]
        private AnimationCurve pulseCurve =
            AnimationCurve.EaseInOut(
                0f,
                0f,
                1f,
                1f
            );


        // ============================================================
        // SCAN
        // ============================================================

        [Header("Scan")]

        [SerializeField]
        private bool scanEnabled = true;


        [ColorUsage(true, true)]
        [SerializeField]
        private Color scanColor =
            Color.cyan;


        [SerializeField]
        [Range(-10f, 10f)]
        private float scanSpeed = 2f;


        [SerializeField]
        [Range(1f, 100f)]
        private float scanScale = 20f;


        [SerializeField]
        [Range(0.001f, 1f)]
        private float scanWidth = 0.12f;


        [SerializeField]
        [Range(0f, 20f)]
        private float scanIntensity = 4f;


        // ============================================================
        // SWEEP
        // ============================================================

        [Header("Energy Sweep")]

        [SerializeField]
        private bool sweepEnabled = true;


        [ColorUsage(true, true)]
        [SerializeField]
        private Color sweepColor =
            Color.white;


        [SerializeField]
        [Range(0.01f, 1f)]
        private float sweepWidth = 0.2f;


        [SerializeField]
        [Range(0f, 20f)]
        private float sweepIntensity = 5f;


        [SerializeField]
        [Range(-10f, 10f)]
        private float sweepSpeed = 1f;


        // ============================================================
        // NOISE
        // ============================================================

        [Header("Noise")]

        [SerializeField]
        private bool noiseEnabled;


        [SerializeField]
        private Texture2D noiseTexture;


        [SerializeField]
        [Range(0.1f, 30f)]
        private float noiseScale = 5f;


        [SerializeField]
        [Range(-10f, 10f)]
        private float noiseSpeed = 1f;


        [SerializeField]
        [Range(0f, 1f)]
        private float noiseStrength = 0.25f;


        [ColorUsage(true, true)]
        [SerializeField]
        private Color noiseColor =
            new Color(
                0f,
                0.5f,
                1f,
                1f
            );


        [SerializeField]
        [Range(0f, 20f)]
        private float noiseIntensity = 2f;


        // ============================================================
        // DISSOLVE
        // ============================================================

        [Header("Dissolve")]

        [SerializeField]
        private bool dissolveEnabled;


        [SerializeField]
        private Texture2D dissolveTexture;


        [SerializeField]
        [Range(0f, 1f)]
        private float dissolveAmount;


        [SerializeField]
        [Range(0.001f, 0.5f)]
        private float dissolveEdgeWidth = 0.05f;


        [ColorUsage(true, true)]
        [SerializeField]
        private Color dissolveEdgeColor =
            Color.cyan;


        [SerializeField]
        [Range(0f, 30f)]
        private float dissolveEdgeIntensity = 8f;


        // ============================================================
        // RUNTIME
        // ============================================================

        private Material runtimeMaterial;

        private GlowState currentState;

        private Coroutine transitionRoutine;

        private Coroutine pulseRoutine;


        // ============================================================
        // SHADER IDS
        // ============================================================

        private static readonly int GlowColorID =
            Shader.PropertyToID(
                "_GlowColor"
            );

        private static readonly int GlowIntensityID =
            Shader.PropertyToID(
                "_GlowIntensity"
            );

        private static readonly int FresnelEnabledID =
            Shader.PropertyToID(
                "_FresnelEnabled"
            );

        private static readonly int FresnelColorID =
            Shader.PropertyToID(
                "_FresnelColor"
            );

        private static readonly int FresnelPowerID =
            Shader.PropertyToID(
                "_FresnelPower"
            );

        private static readonly int FresnelIntensityID =
            Shader.PropertyToID(
                "_FresnelIntensity"
            );

        private static readonly int PulseID =
            Shader.PropertyToID(
                "_Pulse"
            );

        private static readonly int PulseIntensityID =
            Shader.PropertyToID(
                "_PulseIntensity"
            );

        private static readonly int ScanEnabledID =
            Shader.PropertyToID(
                "_ScanEnabled"
            );

        private static readonly int ScanColorID =
            Shader.PropertyToID(
                "_ScanColor"
            );

        private static readonly int ScanSpeedID =
            Shader.PropertyToID(
                "_ScanSpeed"
            );

        private static readonly int ScanScaleID =
            Shader.PropertyToID(
                "_ScanScale"
            );

        private static readonly int ScanWidthID =
            Shader.PropertyToID(
                "_ScanWidth"
            );

        private static readonly int ScanIntensityID =
            Shader.PropertyToID(
                "_ScanIntensity"
            );

        private static readonly int SweepEnabledID =
            Shader.PropertyToID(
                "_SweepEnabled"
            );

        private static readonly int SweepColorID =
            Shader.PropertyToID(
                "_SweepColor"
            );

        private static readonly int SweepWidthID =
            Shader.PropertyToID(
                "_SweepWidth"
            );

        private static readonly int SweepIntensityID =
            Shader.PropertyToID(
                "_SweepIntensity"
            );

        private static readonly int SweepSpeedID =
            Shader.PropertyToID(
                "_SweepSpeed"
            );

        private static readonly int NoiseEnabledID =
            Shader.PropertyToID(
                "_NoiseEnabled"
            );

        private static readonly int NoiseMapID =
            Shader.PropertyToID(
                "_NoiseMap"
            );

        private static readonly int NoiseScaleID =
            Shader.PropertyToID(
                "_NoiseScale"
            );

        private static readonly int NoiseSpeedID =
            Shader.PropertyToID(
                "_NoiseSpeed"
            );

        private static readonly int NoiseStrengthID =
            Shader.PropertyToID(
                "_NoiseStrength"
            );

        private static readonly int NoiseColorID =
            Shader.PropertyToID(
                "_NoiseColor"
            );

        private static readonly int NoiseIntensityID =
            Shader.PropertyToID(
                "_NoiseIntensity"
            );

        private static readonly int DissolveEnabledID =
            Shader.PropertyToID(
                "_DissolveEnabled"
            );

        private static readonly int DissolveMapID =
            Shader.PropertyToID(
                "_DissolveMap"
            );

        private static readonly int DissolveAmountID =
            Shader.PropertyToID(
                "_DissolveAmount"
            );

        private static readonly int DissolveEdgeWidthID =
            Shader.PropertyToID(
                "_DissolveEdgeWidth"
            );

        private static readonly int DissolveEdgeColorID =
            Shader.PropertyToID(
                "_DissolveEdgeColor"
            );

        private static readonly int DissolveEdgeIntensityID =
            Shader.PropertyToID(
                "_DissolveEdgeIntensity"
            );


        // ============================================================
        // AWAKE
        // ============================================================

        private void Awake()
        {
            ResolveRenderer();

            if (targetRenderer == null)
            {
                Debug.LogError(
                    "[SparkAdvancedGlowController] " +
                    "Renderer not found.",
                    this
                );

                enabled = false;

                return;
            }

            runtimeMaterial =
                targetRenderer.material;

            if (runtimeMaterial == null)
            {
                Debug.LogError(
                    "[SparkAdvancedGlowController] " +
                    "Material not found.",
                    this
                );

                enabled = false;

                return;
            }

            ApplyAllProperties();

            currentState =
                defaultState;

            ApplyStateInstant(
                currentState
            );
        }


        // ============================================================
        // RESOLVE RENDERER
        // ============================================================

        private void ResolveRenderer()
        {
            if (
                targetRenderer != null
            )
            {
                return;
            }

            if (
                !findRendererAutomatically
            )
            {
                return;
            }

            targetRenderer =
                GetComponent<Renderer>();

            if (
                targetRenderer == null
            )
            {
                targetRenderer =
                    GetComponentInChildren<
                        Renderer
                    >();
            }
        }


        // ============================================================
        // APPLY ALL PROPERTIES
        // ============================================================

        private void ApplyAllProperties()
        {
            runtimeMaterial.SetColor(
                GlowColorID,
                glowColor
            );

            runtimeMaterial.SetFloat(
                GlowIntensityID,
                glowIntensity
            );


            runtimeMaterial.SetFloat(
                FresnelEnabledID,
                fresnelEnabled
                    ? 1f
                    : 0f
            );

            runtimeMaterial.SetColor(
                FresnelColorID,
                fresnelColor
            );

            runtimeMaterial.SetFloat(
                FresnelPowerID,
                fresnelPower
            );

            runtimeMaterial.SetFloat(
                FresnelIntensityID,
                fresnelIntensity
            );


            runtimeMaterial.SetFloat(
                PulseIntensityID,
                pulseIntensity
            );


            runtimeMaterial.SetFloat(
                ScanEnabledID,
                scanEnabled
                    ? 1f
                    : 0f
            );

            runtimeMaterial.SetColor(
                ScanColorID,
                scanColor
            );

            runtimeMaterial.SetFloat(
                ScanSpeedID,
                scanSpeed
            );

            runtimeMaterial.SetFloat(
                ScanScaleID,
                scanScale
            );

            runtimeMaterial.SetFloat(
                ScanWidthID,
                scanWidth
            );

            runtimeMaterial.SetFloat(
                ScanIntensityID,
                scanIntensity
            );


            runtimeMaterial.SetFloat(
                SweepEnabledID,
                sweepEnabled
                    ? 1f
                    : 0f
            );

            runtimeMaterial.SetColor(
                SweepColorID,
                sweepColor
            );

            runtimeMaterial.SetFloat(
                SweepWidthID,
                sweepWidth
            );

            runtimeMaterial.SetFloat(
                SweepIntensityID,
                sweepIntensity
            );

            runtimeMaterial.SetFloat(
                SweepSpeedID,
                sweepSpeed
            );


            runtimeMaterial.SetFloat(
                NoiseEnabledID,
                noiseEnabled
                    ? 1f
                    : 0f
            );

            if (
                noiseTexture != null
            )
            {
                runtimeMaterial.SetTexture(
                    NoiseMapID,
                    noiseTexture
                );
            }

            runtimeMaterial.SetFloat(
                NoiseScaleID,
                noiseScale
            );

            runtimeMaterial.SetFloat(
                NoiseSpeedID,
                noiseSpeed
            );

            runtimeMaterial.SetFloat(
                NoiseStrengthID,
                noiseStrength
            );

            runtimeMaterial.SetColor(
                NoiseColorID,
                noiseColor
            );

            runtimeMaterial.SetFloat(
                NoiseIntensityID,
                noiseIntensity
            );


            runtimeMaterial.SetFloat(
                DissolveEnabledID,
                dissolveEnabled
                    ? 1f
                    : 0f
            );

            if (
                dissolveTexture != null
            )
            {
                runtimeMaterial.SetTexture(
                    DissolveMapID,
                    dissolveTexture
                );
            }

            runtimeMaterial.SetFloat(
                DissolveAmountID,
                dissolveAmount
            );

            runtimeMaterial.SetFloat(
                DissolveEdgeWidthID,
                dissolveEdgeWidth
            );

            runtimeMaterial.SetColor(
                DissolveEdgeColorID,
                dissolveEdgeColor
            );

            runtimeMaterial.SetFloat(
                DissolveEdgeIntensityID,
                dissolveEdgeIntensity
            );


            runtimeMaterial.SetFloat(
                PulseID,
                0f
            );
        }


        // ============================================================
        // CURRENT STATE
        // ============================================================

        public GlowState CurrentState
        {
            get
            {
                return currentState;
            }
        }


        // ============================================================
        // SET STATE
        // ============================================================

        public void SetState(
            GlowState newState)
        {
            if (
                currentState ==
                newState
            )
            {
                return;
            }

            currentState =
                newState;

            ApplyState(
                newState
            );

            PlayPulse();
        }


        // ============================================================
        // APPLY STATE
        // ============================================================

        private void ApplyState(
            GlowState state)
        {
            if (
                transitionRoutine != null
            )
            {
                StopCoroutine(
                    transitionRoutine
                );
            }

            transitionRoutine =
                StartCoroutine(
                    TransitionState(
                        state
                    )
                );
        }


        // ============================================================
        // APPLY STATE INSTANT
        // ============================================================

        private void ApplyStateInstant(
            GlowState state)
        {
            Color color;

            float intensity;

            GetStateValues(
                state,
                out color,
                out intensity
            );

            runtimeMaterial.SetColor(
                GlowColorID,
                color
            );

            runtimeMaterial.SetFloat(
                GlowIntensityID,
                intensity
            );

            runtimeMaterial.SetFloat(
                PulseID,
                0f
            );
        }


        // ============================================================
        // STATE TRANSITION
        // ============================================================

        private IEnumerator TransitionState(
            GlowState state)
        {
            Color startColor =
                runtimeMaterial.GetColor(
                    GlowColorID
                );

            float startIntensity =
                runtimeMaterial.GetFloat(
                    GlowIntensityID
                );


            Color targetColor;

            float targetIntensity;


            GetStateValues(
                state,
                out targetColor,
                out targetIntensity
            );


            float time = 0f;


            while (
                time <
                transitionDuration
            )
            {
                time +=
                    Time.deltaTime;


                float t =
                    Mathf.Clamp01(
                        time /
                        transitionDuration
                    );


                t =
                    transitionCurve.Evaluate(
                        t
                    );


                runtimeMaterial.SetColor(
                    GlowColorID,
                    Color.Lerp(
                        startColor,
                        targetColor,
                        t
                    )
                );


                runtimeMaterial.SetFloat(
                    GlowIntensityID,
                    Mathf.Lerp(
                        startIntensity,
                        targetIntensity,
                        t
                    )
                );


                yield return null;
            }


            runtimeMaterial.SetColor(
                GlowColorID,
                targetColor
            );


            runtimeMaterial.SetFloat(
                GlowIntensityID,
                targetIntensity
            );


            transitionRoutine =
                null;
        }


        // ============================================================
        // STATE VALUES
        // ============================================================

        private void GetStateValues(
            GlowState state,
            out Color color,
            out float intensity)
        {
            color =
                glowColor;

            intensity =
                glowIntensity;


            switch (state)
            {
                case GlowState.Normal:

                    break;


                case GlowState.Hover:

                    color =
                        new Color(
                            0f,
                            0.8f,
                            1f,
                            1f
                        );

                    intensity =
                        glowIntensity *
                        1.5f;

                    break;


                case GlowState.Selected:

                    color =
                        new Color(
                            0f,
                            1f,
                            1f,
                            1f
                        );

                    intensity =
                        glowIntensity *
                        2f;

                    break;


                case GlowState.Target:

                    color =
                        new Color(
                            1f,
                            0.85f,
                            0.1f,
                            1f
                        );

                    intensity =
                        glowIntensity *
                        2.5f;

                    break;


                case GlowState.Warning:

                    color =
                        new Color(
                            1f,
                            0.35f,
                            0.02f,
                            1f
                        );

                    intensity =
                        glowIntensity *
                        3f;

                    break;


                case GlowState.Error:

                    color =
                        Color.red;

                    intensity =
                        glowIntensity *
                        4f;

                    break;


                case GlowState.Disabled:

                    color =
                        Color.gray;

                    intensity =
                        glowIntensity *
                        0.15f;

                    break;
            }
        }


        // ============================================================
        // PULSE
        // ============================================================

        public void PlayPulse()
        {
            if (
                pulseRoutine != null
            )
            {
                StopCoroutine(
                    pulseRoutine
                );
            }

            pulseRoutine =
                StartCoroutine(
                    PulseRoutine()
                );
        }


        private IEnumerator PulseRoutine()
        {
            float time = 0f;


            while (
                time <
                pulseDuration
            )
            {
                time +=
                    Time.deltaTime;


                float t =
                    Mathf.Clamp01(
                        time /
                        pulseDuration
                    );


                float value =
                    pulseCurve.Evaluate(
                        t
                    );


                runtimeMaterial.SetFloat(
                    PulseID,
                    value
                );


                yield return null;
            }


            runtimeMaterial.SetFloat(
                PulseID,
                0f
            );


            pulseRoutine =
                null;
        }


        // ============================================================
        // PUBLIC STATE API
        // ============================================================

        public void SetNormal()
        {
            SetState(
                GlowState.Normal
            );
        }


        public void SetHover()
        {
            SetState(
                GlowState.Hover
            );
        }


        public void SetSelected()
        {
            SetState(
                GlowState.Selected
            );
        }


        public void SetTarget()
        {
            SetState(
                GlowState.Target
            );
        }


        public void SetWarning()
        {
            SetState(
                GlowState.Warning
            );
        }


        public void SetError()
        {
            SetState(
                GlowState.Error
            );
        }


        public void SetDisabled()
        {
            SetState(
                GlowState.Disabled
            );
        }


        // ============================================================
        // DISSOLVE API
        // ============================================================

        public void SetDissolve(
            float value)
        {
            dissolveAmount =
                Mathf.Clamp01(
                    value
                );


            if (
                runtimeMaterial != null
            )
            {
                runtimeMaterial.SetFloat(
                    DissolveAmountID,
                    dissolveAmount
                );
            }
        }


        // ============================================================
        // CLEANUP
        // ============================================================

        private void OnDestroy()
        {
            if (
                transitionRoutine != null
            )
            {
                StopCoroutine(
                    transitionRoutine
                );
            }


            if (
                pulseRoutine != null
            )
            {
                StopCoroutine(
                    pulseRoutine
                );
            }
        }
    }
}