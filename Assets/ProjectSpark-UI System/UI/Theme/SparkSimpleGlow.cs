using System.Collections;
using UnityEngine;

namespace ProjectSpark.UI.VFX
{
    /// <summary>
    /// Simple visible VFX test.
    ///
    /// Attach this to any GameObject with a Renderer.
    ///
    /// Automatically:
    /// - Finds the Renderer.
    /// - Creates a material instance.
    /// - Controls the glow shader.
    /// - Pulses the glow.
    ///
    /// No state machine required.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkSimpleGlow
        : MonoBehaviour
    {
        // ============================================================
        // SETTINGS
        // ============================================================

        [Header("Glow")]

        [SerializeField]
        private Color glowColor =
            new Color(
                0f,
                0.8f,
                1f,
                1f
            );


        [SerializeField]
        [Range(0f, 10f)]
        private float glowIntensity =
            4f;


        [Header("Pulse")]

        [SerializeField]
        private float pulseDuration =
            0.35f;


        [SerializeField]
        private float holdDuration =
            0.15f;


        [SerializeField]
        private AnimationCurve pulseCurve =
            AnimationCurve.EaseInOut(
                0f,
                0f,
                1f,
                1f
            );


        [Header("Startup")]

        [SerializeField]
        private bool playOnStart =
            true;


        [SerializeField]
        private bool loop =
            true;


        [SerializeField]
        private float loopDelay =
            0.5f;


        // ============================================================
        // RUNTIME
        // ============================================================

        private Renderer targetRenderer;

        private Material runtimeMaterial;

        private Coroutine pulseRoutine;


        private static readonly int GlowColorID =
            Shader.PropertyToID(
                "_GlowColor"
            );


        private static readonly int GlowIntensityID =
            Shader.PropertyToID(
                "_GlowIntensity"
            );


        private static readonly int PulseID =
            Shader.PropertyToID(
                "_Pulse"
            );


        // ============================================================
        // AWAKE
        // ============================================================

        private void Awake()
        {
            targetRenderer =
                GetComponent<Renderer>();


            if (targetRenderer == null)
            {
                Debug.LogError(
                    "[SparkSimpleGlow] " +
                    "No Renderer found on " +
                    gameObject.name,
                    this
                );

                return;
            }


            runtimeMaterial =
                targetRenderer.material;


            if (
                runtimeMaterial == null
            )
            {
                Debug.LogError(
                    "[SparkSimpleGlow] " +
                    "Could not create material instance.",
                    this
                );

                return;
            }


            runtimeMaterial.SetColor(
                GlowColorID,
                glowColor
            );


            runtimeMaterial.SetFloat(
                GlowIntensityID,
                glowIntensity
            );


            runtimeMaterial.SetFloat(
                PulseID,
                0f
            );
        }


        // ============================================================
        // START
        // ============================================================

        private void Start()
        {
            if (!playOnStart)
            {
                return;
            }


            Play();
        }


        // ============================================================
        // PLAY
        // ============================================================

        public void Play()
        {
            if (
                runtimeMaterial == null
            )
            {
                return;
            }


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


        // ============================================================
        // PULSE
        // ============================================================

        private IEnumerator PulseRoutine()
        {
            do
            {
                yield return
                    PulseUp();


                yield return
                    new WaitForSeconds(
                        holdDuration
                    );


                yield return
                    PulseDown();


                if (loop)
                {
                    yield return
                        new WaitForSeconds(
                            loopDelay
                        );
                }

            }
            while (loop);


            pulseRoutine =
                null;
        }


        // ============================================================
        // PULSE UP
        // ============================================================

        private IEnumerator PulseUp()
        {
            float time =
                0f;


            while (
                time <
                pulseDuration
            )
            {
                time +=
                    Time.deltaTime;


                float normalized =
                    Mathf.Clamp01(
                        time /
                        pulseDuration
                    );


                float value =
                    pulseCurve.Evaluate(
                        normalized
                    );


                runtimeMaterial.SetFloat(
                    PulseID,
                    value
                );


                yield return null;
            }


            runtimeMaterial.SetFloat(
                PulseID,
                1f
            );
        }


        // ============================================================
        // PULSE DOWN
        // ============================================================

        private IEnumerator PulseDown()
        {
            float time =
                0f;


            while (
                time <
                pulseDuration
            )
            {
                time +=
                    Time.deltaTime;


                float normalized =
                    Mathf.Clamp01(
                        time /
                        pulseDuration
                    );


                float value =
                    1f -
                    pulseCurve.Evaluate(
                        normalized
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
        }


        // ============================================================
        // STOP
        // ============================================================

        public void Stop()
        {
            if (
                pulseRoutine != null
            )
            {
                StopCoroutine(
                    pulseRoutine
                );


                pulseRoutine =
                    null;
            }


            if (
                runtimeMaterial != null
            )
            {
                runtimeMaterial.SetFloat(
                    PulseID,
                    0f
                );
            }
        }


        // ============================================================
        // SET GLOW COLOR
        // ============================================================

        public void SetGlowColor(
            Color color)
        {
            glowColor =
                color;


            if (
                runtimeMaterial != null
            )
            {
                runtimeMaterial.SetColor(
                    GlowColorID,
                    glowColor
                );
            }
        }


        // ============================================================
        // SET INTENSITY
        // ============================================================

        public void SetGlowIntensity(
            float intensity)
        {
            glowIntensity =
                Mathf.Max(
                    0f,
                    intensity
                );


            if (
                runtimeMaterial != null
            )
            {
                runtimeMaterial.SetFloat(
                    GlowIntensityID,
                    glowIntensity
                );
            }
        }


        // ============================================================
        // CLEANUP
        // ============================================================

        private void OnDestroy()
        {
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