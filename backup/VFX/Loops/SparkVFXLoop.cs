using System.Collections;
using UnityEngine;

namespace ProjectSpark.UI.VFX
{
    /// <summary>
    /// Continuous VFX loop controller.
    ///
    /// Supports:
    /// - SparkVFXController
    /// - SparkTMPVFXController
    ///
    /// Resolution:
    ///
    /// SparkVFXLoop
    ///      |
    ///      v
    /// SparkVFXTarget
    ///      |
    ///      v
    /// ISparkVFXController
    ///
    /// The loop only communicates with the controller
    /// through ISparkVFXController.
    ///
    /// This allows the same loop system to work with
    /// both Image and TMP VFX.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkVFXLoop
        : MonoBehaviour
    {
        // ============================================================
        // REFERENCES
        // ============================================================

        [Header("References")]

        [Tooltip(
            "Central VFX target resolver. " +
            "This resolves SparkVFXController or " +
            "SparkTMPVFXController."
        )]
        [SerializeField]
        private SparkVFXTarget target;


        // ============================================================
        // CONTROLLER
        // ============================================================

        private ISparkVFXController controller;


        // ============================================================
        // LOOP SETTINGS
        // ============================================================

        [Header("Loop Settings")]

        [Min(0.01f)]
        [SerializeField]
        private float speed = 1f;


        [SerializeField]
        private bool useUnscaledTime = true;


        [Range(0f, 5f)]
        [SerializeField]
        private float glowAmount = 0.8f;


        [Range(0f, 5f)]
        [SerializeField]
        private float scanAmount = 0f;


        [Range(0f, 5f)]
        [SerializeField]
        private float sweepAmount = 0f;


        [Range(0f, 5f)]
        [SerializeField]
        private float flashAmount = 0f;


        [Range(0f, 1f)]
        [SerializeField]
        private float glitchAmount = 0f;


        [Range(0f, 1f)]
        [SerializeField]
        private float flickerAmount = 0f;


        [Range(0f, 1f)]
        [SerializeField]
        private float revealAmount = 1f;


        [Range(0f, 1f)]
        [SerializeField]
        private float dissolveAmount = 0f;


        // ============================================================
        // ANIMATION SETTINGS
        // ============================================================

        [Header("Animation")]

        [SerializeField]
        private bool animateSweepPosition = true;


        [SerializeField]
        private bool pingPongSweep = true;


        [Min(0f)]
        [SerializeField]
        private float sweepMin = 0f;


        [Min(0f)]
        [SerializeField]
        private float sweepMax = 1f;


        [SerializeField]
        private AnimationCurve sweepCurve =
            AnimationCurve.EaseInOut(
                0f,
                0f,
                1f,
                1f
            );


        // ============================================================
        // RUNTIME STATE
        // ============================================================

        private Coroutine loopRoutine;

        private bool initialized;

        private bool resolving;

        private bool playing;


        private float currentTime;


        // ============================================================
        // PUBLIC PROPERTIES
        // ============================================================

        public bool IsPlaying
        {
            get
            {
                return playing;
            }
        }


        public bool IsInitialized
        {
            get
            {
                return initialized;
            }
        }


        public ISparkVFXController Controller
        {
            get
            {
                if (!EnsureReady())
                {
                    return null;
                }

                return controller;
            }
        }


        public SparkVFXTarget Target
        {
            get
            {
                ResolveTarget();

                return target;
            }
        }


        // ============================================================
        // UNITY AWAKE
        // ============================================================

        private void Awake()
        {
            Initialize();
        }


        // ============================================================
        // UNITY ENABLE
        // ============================================================

        private void OnEnable()
        {
            if (!initialized)
            {
                Initialize();
            }
        }


        // ============================================================
        // INITIALIZE
        // ============================================================

        public void Initialize()
        {
            if (initialized)
            {
                return;
            }


            if (resolving)
            {
                return;
            }


            resolving =
                true;


            // --------------------------------------------------------
            // RESOLVE TARGET
            // --------------------------------------------------------

            ResolveTarget();


            if (target == null)
            {
                Debug.LogError(
                    "[SparkVFXLoop] " +
                    "No SparkVFXTarget found. " +
                    "Add SparkVFXTarget to this GameObject, " +
                    "a parent, or a child.",
                    this
                );


                resolving =
                    false;

                return;
            }


            // --------------------------------------------------------
            // RESOLVE CONTROLLER
            // --------------------------------------------------------

            controller =
                target.Controller;


            if (controller == null)
            {
                Debug.LogError(
                    "[SparkVFXLoop] " +
                    "SparkVFXTarget could not resolve a compatible " +
                    "ISparkVFXController. " +
                    "Add SparkVFXController or " +
                    "SparkTMPVFXController.",
                    this
                );


                resolving =
                    false;

                return;
            }


            // --------------------------------------------------------
            // INITIALIZE CONTROLLER
            // --------------------------------------------------------

            controller.Initialize();


            // --------------------------------------------------------
            // COMPLETE
            // --------------------------------------------------------

            initialized =
                true;


            resolving =
                false;
        }


        // ============================================================
        // RESOLVE TARGET
        // ============================================================

        private void ResolveTarget()
        {
            if (target != null)
            {
                return;
            }


            // --------------------------------------------------------
            // SAME GAMEOBJECT
            // --------------------------------------------------------

            target =
                GetComponent<
                    SparkVFXTarget
                >();


            if (target != null)
            {
                return;
            }


            // --------------------------------------------------------
            // PARENT
            // --------------------------------------------------------

            target =
                GetComponentInParent<
                    SparkVFXTarget
                >(
                    true
                );


            if (target != null)
            {
                return;
            }


            // --------------------------------------------------------
            // CHILDREN
            // --------------------------------------------------------

            target =
                GetComponentInChildren<
                    SparkVFXTarget
                >(
                    true
                );
        }


        // ============================================================
        // ENSURE READY
        // ============================================================

        private bool EnsureReady()
        {
            if (
                initialized &&
                controller != null
            )
            {
                return true;
            }


            if (resolving)
            {
                return false;
            }


            Initialize();


            return
                initialized &&
                controller != null;
        }


        // ============================================================
        // PLAY
        // ============================================================

        public void Play()
        {
            if (!EnsureReady())
            {
                return;
            }


            // --------------------------------------------------------
            // ALREADY PLAYING
            // --------------------------------------------------------

            if (playing)
            {
                return;
            }


            // --------------------------------------------------------
            // START
            // --------------------------------------------------------

            playing =
                true;


            currentTime =
                0f;


            ApplyStaticValues();


            if (loopRoutine != null)
            {
                StopCoroutine(
                    loopRoutine
                );
            }


            loopRoutine =
                StartCoroutine(
                    LoopRoutine()
                );
        }


        // ============================================================
        // PLAY RESTART
        // ============================================================

        public void Restart()
        {
            if (!EnsureReady())
            {
                return;
            }


            Stop();


            Play();
        }


        // ============================================================
        // STOP
        // ============================================================

        public void Stop()
        {
            playing =
                false;


            currentTime =
                0f;


            if (loopRoutine != null)
            {
                StopCoroutine(
                    loopRoutine
                );


                loopRoutine =
                    null;
            }
        }


        // ============================================================
        // PAUSE
        // ============================================================

        public void Pause()
        {
            if (!playing)
            {
                return;
            }


            playing =
                false;


            if (loopRoutine != null)
            {
                StopCoroutine(
                    loopRoutine
                );


                loopRoutine =
                    null;
            }
        }


        // ============================================================
        // RESUME
        // ============================================================

        public void Resume()
        {
            if (playing)
            {
                return;
            }


            if (!EnsureReady())
            {
                return;
            }


            playing =
                true;


            if (loopRoutine != null)
            {
                StopCoroutine(
                    loopRoutine
                );
            }


            loopRoutine =
                StartCoroutine(
                    LoopRoutine()
                );
        }


        // ============================================================
        // LOOP ROUTINE
        // ============================================================

        private IEnumerator LoopRoutine()
        {
            while (playing)
            {
                if (controller == null)
                {
                    controller =
                        target != null
                            ? target.Controller
                            : null;
                }


                if (controller == null)
                {
                    playing =
                        false;

                    loopRoutine =
                        null;

                    yield break;
                }


                float deltaTime;


                if (useUnscaledTime)
                {
                    deltaTime =
                        Time.unscaledDeltaTime;
                }
                else
                {
                    deltaTime =
                        Time.deltaTime;
                }


                currentTime +=
                    deltaTime *
                    Mathf.Max(
                        0.01f,
                        speed
                    );


                ApplyLoopValues(
                    currentTime
                );


                yield return null;
            }


            loopRoutine =
                null;
        }


        // ============================================================
        // APPLY STATIC VALUES
        // ============================================================

        private void ApplyStaticValues()
        {
            if (controller == null)
            {
                return;
            }


            controller.SetGlowValue(
                glowAmount
            );


            controller.SetScanValue(
                scanAmount
            );


            controller.SetSweepValue(
                sweepAmount
            );


            controller.SetFlashValue(
                flashAmount
            );


            controller.SetGlitchValue(
                glitchAmount
            );


            controller.SetFlickerValue(
                flickerAmount
            );


            controller.SetRevealValue(
                revealAmount
            );


            controller.SetDissolveValue(
                dissolveAmount
            );
        }


        // ============================================================
        // APPLY LOOP VALUES
        // ============================================================

        private void ApplyLoopValues(
            float time)
        {
            if (controller == null)
            {
                return;
            }


            // --------------------------------------------------------
            // STATIC EFFECT VALUES
            // --------------------------------------------------------

            controller.SetGlowValue(
                glowAmount
            );


            controller.SetScanValue(
                scanAmount
            );


            controller.SetSweepValue(
                sweepAmount
            );


            controller.SetFlashValue(
                flashAmount
            );


            controller.SetGlitchValue(
                glitchAmount
            );


            controller.SetFlickerValue(
                flickerAmount
            );


            controller.SetRevealValue(
                revealAmount
            );


            controller.SetDissolveValue(
                dissolveAmount
            );


            // --------------------------------------------------------
            // SWEEP POSITION
            // --------------------------------------------------------

            if (!animateSweepPosition)
            {
                controller.SetSweepPositionValue(
                    sweepMin
                );

                return;
            }


            float normalized;


            if (pingPongSweep)
            {
                normalized =
                    Mathf.PingPong(
                        time,
                        1f
                    );
            }
            else
            {
                normalized =
                    Mathf.Repeat(
                        time,
                        1f
                    );
            }


            // --------------------------------------------------------
            // CURVE
            // --------------------------------------------------------

            if (sweepCurve != null)
            {
                normalized =
                    sweepCurve.Evaluate(
                        normalized
                    );
            }


            // --------------------------------------------------------
            // RANGE
            // --------------------------------------------------------

            float position =
                Mathf.Lerp(
                    sweepMin,
                    sweepMax,
                    normalized
                );


            // --------------------------------------------------------
            // APPLY
            // --------------------------------------------------------

            controller.SetSweepPositionValue(
                position
            );
        }


        // ============================================================
        // CONFIGURE
        // ============================================================

        public void Configure(
            float newSpeed,
            bool newAnimateSweepPosition,
            float newGlow,
            float newSweep,
            float newScan,
            float newFlicker)
        {
            speed =
                Mathf.Max(
                    0.01f,
                    newSpeed
                );


            animateSweepPosition =
                newAnimateSweepPosition;


            glowAmount =
                Mathf.Clamp(
                    newGlow,
                    0f,
                    5f
                );


            sweepAmount =
                Mathf.Clamp(
                    newSweep,
                    0f,
                    5f
                );


            scanAmount =
                Mathf.Clamp(
                    newScan,
                    0f,
                    5f
                );


            flickerAmount =
                Mathf.Clamp01(
                    newFlicker
                );


            if (playing)
            {
                ApplyStaticValues();
            }
        }


        // ============================================================
        // CONFIGURE FULL
        // ============================================================

        public void ConfigureFull(
            float newSpeed,
            bool newUseUnscaledTime,
            bool newAnimateSweepPosition,
            bool newPingPongSweep,
            float newGlow,
            float newScan,
            float newSweep,
            float newFlash,
            float newGlitch,
            float newFlicker,
            float newReveal,
            float newDissolve)
        {
            speed =
                Mathf.Max(
                    0.01f,
                    newSpeed
                );


            useUnscaledTime =
                newUseUnscaledTime;


            animateSweepPosition =
                newAnimateSweepPosition;


            pingPongSweep =
                newPingPongSweep;


            glowAmount =
                Mathf.Clamp(
                    newGlow,
                    0f,
                    5f
                );


            scanAmount =
                Mathf.Clamp(
                    newScan,
                    0f,
                    5f
                );


            sweepAmount =
                Mathf.Clamp(
                    newSweep,
                    0f,
                    5f
                );


            flashAmount =
                Mathf.Clamp(
                    newFlash,
                    0f,
                    5f
                );


            glitchAmount =
                Mathf.Clamp01(
                    newGlitch
                );


            flickerAmount =
                Mathf.Clamp01(
                    newFlicker
                );


            revealAmount =
                Mathf.Clamp01(
                    newReveal
                );


            dissolveAmount =
                Mathf.Clamp01(
                    newDissolve
                );


            if (playing)
            {
                ApplyStaticValues();
            }
        }


        // ============================================================
        // SET SPEED
        // ============================================================

        public void SetSpeed(
            float value)
        {
            speed =
                Mathf.Max(
                    0.01f,
                    value
                );
        }


        // ============================================================
        // SET SWEEP RANGE
        // ============================================================

        public void SetSweepRange(
            float min,
            float max)
        {
            sweepMin =
                min;


            sweepMax =
                max;
        }


        // ============================================================
        // SET CONTROLLER
        // ============================================================

        public void SetTarget(
            SparkVFXTarget newTarget)
        {
            Stop();


            target =
                newTarget;


            controller =
                null;


            initialized =
                false;


            resolving =
                false;


            Initialize();
        }


        // ============================================================
        // REFRESH
        // ============================================================

        public void Refresh()
        {
            Stop();


            target =
                null;


            controller =
                null;


            initialized =
                false;


            resolving =
                false;


            Initialize();
        }


        // ============================================================
        // RESET
        // ============================================================

        public void ResetLoop()
        {
            Stop();


            if (!EnsureReady())
            {
                return;
            }


            controller.ResetVFX();
        }


        // ============================================================
        // ON DISABLE
        // ============================================================

        private void OnDisable()
        {
            Stop();
        }


        // ============================================================
        // VALIDATION
        // ============================================================

        private void OnValidate()
        {
            speed =
                Mathf.Max(
                    0.01f,
                    speed
                );


            sweepMin =
                Mathf.Max(
                    0f,
                    sweepMin
                );


            sweepMax =
                Mathf.Max(
                    sweepMin,
                    sweepMax
                );


            if (target == null)
            {
                target =
                    GetComponent<
                        SparkVFXTarget
                    >();
            }
        }
    }
}