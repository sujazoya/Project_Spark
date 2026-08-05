
using System;
using System.Collections;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;


namespace ProjectSpark.VFX
{
    /// <summary>
    /// Professional monitor diagnostic scan sequence.
    ///
    /// Designed for Project Spark electronic simulator.
    ///
    /// Sequence:
    ///
    /// Idle
    ///   ↓
    /// Start
    ///   ↓
    /// Scanning
    ///   ↓
    /// Analysis
    ///   ↓
    /// Completion Flash
    ///   ↓
    /// Result Reveal
    ///   ↓
    /// Result Hold
    ///   ↓
    /// Idle
    ///
    /// Responsibilities:
    /// - Controls the timing of a monitor scan.
    /// - Drives SparkVFXController effects.
    /// - Animates sweep progress.
    /// - Coordinates scan, noise, glow and distortion.
    /// - Reveals the final result.
    /// - Supports success/failure states.
    /// - Provides UnityEvents for UI integration.
    ///
    /// This class does NOT own the VFX system.
    /// SparkVFXController remains responsible for VFX.
    ///
    /// This class only orchestrates the sequence.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkMonitorScanSequence
        : MonoBehaviour
    {
        // ============================================================
        // ENUMS
        // ============================================================

        public enum ScanState
        {
            Idle,
            Starting,
            Scanning,
            Analyzing,
            Completing,
            Revealing,
            Holding,
            Returning
        }


        public enum ScanResult
        {
            None,
            Success,
            Failure
        }


        // ============================================================
        // VFX
        // ============================================================

        [Header("VFX")]

        [SerializeField]
        private SparkVFXController vfx;


        // ============================================================
        // RESULT UI
        // ============================================================

        [Header("Result UI")]

        [SerializeField]
        private CanvasGroup resultGroup;

        [SerializeField]
        private TMP_Text resultTitle;

        [SerializeField]
        private TMP_Text resultValue;

        [SerializeField]
        private TMP_Text resultStatus;       


        // ============================================================
        // RESULT TEXT
        // ============================================================

        [Header("Result Text")]

        [SerializeField]
        private string successTitle =
            "ANALYSIS COMPLETE";

        [SerializeField]
        private string successValue =
            "SYSTEM NOMINAL";

        [SerializeField]
        private string successStatus =
            "ALL PARAMETERS WITHIN LIMITS";


        [SerializeField]
        private string failureTitle =
            "ANALYSIS COMPLETE";

        [SerializeField]
        private string failureValue =
            "ANOMALY DETECTED";

        [SerializeField]
        private string failureStatus =
            "SYSTEM REQUIRES ATTENTION";


        // ============================================================
        // TIMING
        // ============================================================

        [Header("Timing")]

        [Min(0f)]
        [SerializeField]
        private float startupDuration =
            0.25f;


        [Min(0.05f)]
        [SerializeField]
        private float scanDuration =
            2.5f;


        [Min(0f)]
        [SerializeField]
        private float analysisDuration =
            0.65f;


        [Min(0f)]
        [SerializeField]
        private float completionDuration =
            0.15f;


        [Min(0f)]
        [SerializeField]
        private float resultRevealDuration =
            0.35f;


        [Min(0f)]
        [SerializeField]
        private float resultHoldDuration =
            2.5f;


        [Min(0f)]
        [SerializeField]
        private float returnDuration =
            0.5f;


        // ============================================================
        // MOTION
        // ============================================================

        [Header("Motion")]

        [Tooltip(
            "Controls the movement of the scanning sweep."
        )]
        [SerializeField]
        private AnimationCurve scanCurve =
            AnimationCurve.EaseInOut(
                0f,
                0f,
                1f,
                1f
            );


        [Tooltip(
            "Controls the result UI reveal."
        )]
        [SerializeField]
        private AnimationCurve revealCurve =
            AnimationCurve.EaseInOut(
                0f,
                0f,
                1f,
                1f
            );


        [Tooltip(
            "Controls the result UI fade-out."
        )]
        [SerializeField]
        private AnimationCurve returnCurve =
            AnimationCurve.EaseInOut(
                0f,
                1f,
                1f,
                0f
            );


        // ============================================================
        // SCAN VFX
        // ============================================================

        [Header("Scan VFX")]

        [Range(0f, 20f)]
        [SerializeField]
        private float scanIntensity =
            4f;


        [Range(0f, 20f)]
        [SerializeField]
        private float sweepIntensity =
            5f;


        [Range(0f, 20f)]
        [SerializeField]
        private float startupGlow =
            3f;


        [Range(0f, 20f)]
        [SerializeField]
        private float scanningGlow =
            5f;


        [Range(0f, 20f)]
        [SerializeField]
        private float analysisGlow =
            2.5f;


        [Range(0f, 20f)]
        [SerializeField]
        private float completionGlow =
            8f;


        // ============================================================
        // NOISE
        // ============================================================

        [Header("Scan Noise")]

        [Range(0f, 20f)]
        [SerializeField]
        private float startupNoise =
            1f;


        [Range(0f, 20f)]
        [SerializeField]
        private float scanningNoise =
            2f;


        [Range(0f, 20f)]
        [SerializeField]
        private float analysisNoise =
            0.5f;


        // ============================================================
        // DISTORTION
        // ============================================================

        [Header("Scan Distortion")]

        [Range(0f, 1f)]
        [SerializeField]
        private float startupDistortion =
            0.04f;


        [Range(0f, 1f)]
        [SerializeField]
        private float scanningDistortion =
            0.02f;


        [Range(0f, 1f)]
        [SerializeField]
        private float analysisDistortion =
            0.005f;


        // ============================================================
        // RESULT
        // ============================================================

        [Header("Result")]

        [SerializeField]
        private bool defaultResultIsSuccess =
            true;


        [SerializeField]
        private bool hideResultOnStart =
            true;


        [SerializeField]
        private bool clearResultTextOnReset =
            false;


        // ============================================================
        // PLAYBACK
        // ============================================================

        [Header("Playback")]

        [SerializeField]
        private bool playOnEnable =
            false;


        [SerializeField]
        private bool useUnscaledTime =
            false;


        [SerializeField]
        private bool loop =
            false;


        [Min(0f)]
        [SerializeField]
        private float loopDelay =
            1f;


        // ============================================================
        // EVENTS
        // ============================================================

        [Header("Events")]

        [Tooltip(
            "Called when the scan begins."
        )]
        [SerializeField]
        private UnityEvent onScanStarted;


        [Tooltip(
            "Called when the actual scanning phase begins."
        )]
        [SerializeField]
        private UnityEvent onScanningStarted;


        [Tooltip(
            "Called when analysis begins."
        )]
        [SerializeField]
        private UnityEvent onAnalysisStarted;


        [Tooltip(
            "Called when the scan has completed."
        )]
        [SerializeField]
        private UnityEvent onScanCompleted;


        [Tooltip(
            "Called when the result starts appearing."
        )]
        [SerializeField]
        private UnityEvent onResultRevealed;


        [Tooltip(
            "Called when the full sequence finishes."
        )]
        [SerializeField]
        private UnityEvent onSequenceFinished;


        [Tooltip(
            "Called when the scan is cancelled."
        )]
        [SerializeField]
        private UnityEvent onScanCancelled;


        // ============================================================
        // RUNTIME STATE
        // ============================================================

        public ScanState State
        {
            get;
            private set;
        }


        public ScanResult Result
        {
            get;
            private set;
        }


        public float Progress
        {
            get;
            private set;
        }


        public bool IsPlaying
        {
            get;
            private set;
        }


        // ============================================================
        // INTERNAL
        // ============================================================

        private float stateTime;

        private float loopTimer;

        private bool resultAssigned;

        private bool initialized;


        // ============================================================
        // UNITY
        // ============================================================

        private void Awake()
        {
            Initialize();
        }


        private void OnEnable()
        {
            if (!initialized)
            {
                Initialize();
            }


            if (playOnEnable)
            {
                StartScan();
            }
        }


        private void Update()
        {
            float deltaTime =
                useUnscaledTime
                    ? Time.unscaledDeltaTime
                    : Time.deltaTime;


            if (!IsPlaying)
            {
                UpdateLoop(
                    deltaTime
                );

                return;
            }


            UpdateSequence(
                deltaTime
            );
        }


        // ============================================================
        // INITIALIZE
        // ============================================================

        private void Initialize()
        {
            initialized =
                true;


            State =
                ScanState.Idle;


            Result =
                ScanResult.None;


            Progress =
                0f;


            IsPlaying =
                false;


            stateTime =
                0f;


            loopTimer =
                0f;


            resultAssigned =
                false;


            if (hideResultOnStart)
            {
                HideResultInstant();
            }
        }


        // ============================================================
        // START
        // ============================================================       

        public void StartScan()
        {
            StartScan(
                defaultResultIsSuccess
                    ? ScanResult.Success
                    : ScanResult.Failure
            );
           
        }


        public void StartSuccessScan()
        {
            StartScan(
                ScanResult.Success
            );
        }


        public void StartFailureScan()
        {
            StartScan(
                ScanResult.Failure
            );
        }


        public void StartScan(
            ScanResult result)
        {
            if (vfx == null)
            {
                return;
            }


            if (IsPlaying)
            {
                CancelScan();
            }


            Result =
                result;


            resultAssigned =
                true;


            IsPlaying =
                true;


            Progress =
                0f;


            loopTimer =
                0f;
            vfx.SetSweepPosition(0f);


            HideResultInstant();


            EnterState(
                ScanState.Starting
            );


            onScanStarted?.Invoke();
        }


        // ============================================================
        // CANCEL
        // ============================================================

        public void CancelScan()
        {
            if (!IsPlaying)
            {
                return;
            }


            IsPlaying =
                false;


            State =
                ScanState.Idle;


            Progress =
                0f;


            stateTime =
                0f;


            loopTimer =
                0f;


            if (vfx != null)
            {
                vfx.ResetVFX();
            }



            HideResultInstant();


            onScanCancelled?.Invoke();
        }


        // ============================================================
        // UPDATE SEQUENCE
        // ============================================================

        private void UpdateSequence(
            float deltaTime)
        {
            stateTime +=
                deltaTime;


            switch (State)
            {
                case ScanState.Starting:

                    UpdateStarting();

                    break;


                case ScanState.Scanning:

                    UpdateScanning();

                    break;


                case ScanState.Analyzing:

                    UpdateAnalyzing();

                    break;


                case ScanState.Completing:

                    UpdateCompleting();

                    break;


                case ScanState.Revealing:

                    UpdateRevealing();

                    break;


                case ScanState.Holding:

                    UpdateHolding();

                    break;


                case ScanState.Returning:

                    UpdateReturning();

                    break;
            }
        }


        // ============================================================
        // STARTING
        // ============================================================

        private void UpdateStarting()
        {
            float normalized =
                NormalizeTime(
                    stateTime,
                    startupDuration
                );


            float eased =
                scanCurve.Evaluate(
                    normalized
                );


            vfx.SetGlow(
                Mathf.Lerp(
                    0f,
                    startupGlow,
                    eased
                )
            );


            vfx.SetNoise(
                Mathf.Lerp(
                    0f,
                    startupNoise,
                    eased
                )
            );


            vfx.SetDistortion(
                Mathf.Lerp(
                    0f,
                    startupDistortion,
                    eased
                )
            );


            if (
                normalized >=
                1f
            )
            {
                EnterState(
                    ScanState.Scanning
                );


                onScanningStarted?.Invoke();
            }
        }


// ============================================================
// SCANNING
// ============================================================

private void UpdateScanning()
        {
            float normalized =
                NormalizeTime(
                    stateTime,
                    scanDuration
                );


            float eased =
                scanCurve.Evaluate(
                    normalized
                );


            Progress =
                normalized;


            // ========================================================
            // MOVE SWEEP FROM LEFT TO RIGHT
            // ========================================================

            vfx.SetSweepPosition(
                eased
            );


            // ========================================================
            // SCAN VFX
            // ========================================================

            vfx.SetScan(
                scanIntensity
            );


            vfx.SetSweep(
                sweepIntensity
            );


            vfx.SetGlow(
                scanningGlow
            );


            vfx.SetNoise(
                scanningNoise
            );


            vfx.SetDistortion(
                scanningDistortion
            );


            // ========================================================
            // COMPLETE
            // ========================================================

            if (
                normalized >=
                1f
            )
            {
                vfx.SetSweepPosition(
                    1f
                );


                EnterState(
                    ScanState.Analyzing
                );


                onAnalysisStarted?.Invoke();
            }
        }




        // ============================================================
        // ANALYZING
        // ============================================================

      
// ============================================================
// ANALYZING
// ============================================================

private void UpdateAnalyzing()
        {
            float normalized =
                NormalizeTime(
                    stateTime,
                    analysisDuration
                );


            float eased =
                1f -
                scanCurve.Evaluate(
                    normalized
                );


            // ========================================================
            // KEEP SWEEP AT RIGHT EDGE
            // ========================================================

            vfx.SetSweepPosition(
                1f
            );


            // ========================================================
            // FADE SCAN
            // ========================================================

            vfx.SetScan(
                scanIntensity *
                eased
            );


            vfx.SetSweep(
                sweepIntensity *
                eased
            );


            // ========================================================
            // REDUCE VFX INTENSITY
            // ========================================================

            vfx.SetGlow(
                Mathf.Lerp(
                    analysisGlow,
                    scanningGlow,
                    eased
                )
            );


            vfx.SetNoise(
                Mathf.Lerp(
                    analysisNoise,
                    scanningNoise,
                    eased
                )
            );


            vfx.SetDistortion(
                Mathf.Lerp(
                    analysisDistortion,
                    scanningDistortion,
                    eased
                )
            );


            if (
                normalized >=
                1f
            )
            {
                EnterState(
                    ScanState.Completing
                );


                onScanCompleted?.Invoke();
            }
        }




        // ============================================================
        // COMPLETING
        // ============================================================

        private void UpdateCompleting()
        {
            float normalized =
                NormalizeTime(
                    stateTime,
                    completionDuration
                );


            float eased =
                1f -
                scanCurve.Evaluate(
                    normalized
                );


            vfx.SetScan(
                scanIntensity *
                eased
            );


            vfx.SetSweep(
                sweepIntensity *
                eased
            );


            vfx.SetGlow(
                Mathf.Lerp(
                    analysisGlow,
                    completionGlow,
                    1f -
                    eased
                )
            );


            if (
                normalized >=
                1f
            )
            {
                vfx.PlayFlash();


                EnterState(
                    ScanState.Revealing
                );


                onResultRevealed?.Invoke();
            }
        }


        // ============================================================
        // REVEALING
        // ============================================================

        private void UpdateRevealing()
        {
            float normalized =
                NormalizeTime(
                    stateTime,
                    resultRevealDuration
                );


            float eased =
                revealCurve.Evaluate(
                    normalized
                );


            SetResultAlpha(
                eased
            );


            if (
                normalized >=
                1f
            )
            {
                EnterState(
                    ScanState.Holding
                );
            }
        }


        // ============================================================
        // HOLDING
        // ============================================================

        private void UpdateHolding()
        {
            SetResultAlpha(
                1f
            );


            if (
                stateTime >=
                resultHoldDuration
            )
            {
                EnterState(
                    ScanState.Returning
                );
            }
        }


        // ============================================================
        // RETURNING
        // ============================================================

        private void UpdateReturning()
        {
            float normalized =
                NormalizeTime(
                    stateTime,
                    returnDuration
                );


            float eased =
                returnCurve.Evaluate(
                    normalized
                );


            SetResultAlpha(
                eased
            );


            vfx.SetGlow(
                Mathf.Lerp(
                    0f,
                    analysisGlow,
                    eased
                )
            );


            vfx.SetNoise(
                Mathf.Lerp(
                    0f,
                    analysisNoise,
                    eased
                )
            );


            if (
                normalized >=
                1f
            )
            {
                FinishSequence();
            }
        }


        // ============================================================
        // STATE
        // ============================================================

        private void EnterState(
            ScanState newState)
        {
            State =
                newState;


            stateTime =
                0f;


            switch (newState)
            {
                case ScanState.Starting:

                    vfx.SetScanEnabled(
                        true
                    );

                    vfx.SetSweepEnabled(
                        true
                    );

                    vfx.SetNoiseEnabled(
                        true
                    );

                    vfx.SetDistortionEnabled(
                        true
                    );

                    break;


                case ScanState.Scanning:

                    vfx.SetScanEnabled(
                        true
                    );

                    vfx.SetSweepEnabled(
                        true
                    );

                    vfx.SetNoiseEnabled(
                        true
                    );

                    vfx.SetDistortionEnabled(
                        true
                    );

                    break;


                case ScanState.Analyzing:

                    vfx.SetScanEnabled(
                        true
                    );

                    vfx.SetSweepEnabled(
                        true
                    );

                    break;


                case ScanState.Completing:

                    vfx.SetSweepEnabled(
                        false
                    );

                    break;


                case ScanState.Revealing:

                    vfx.SetScanEnabled(
                        false
                    );

                    vfx.SetSweepEnabled(
                        false
                    );

                    vfx.SetNoiseEnabled(
                        false
                    );

                    vfx.SetDistortionEnabled(
                        false
                    );

                    ApplyResultText();

                    break;


                case ScanState.Holding:

                    vfx.SetScanEnabled(
                        false
                    );

                    vfx.SetSweepEnabled(
                        false
                    );

                    break;


                case ScanState.Returning:

                    break;


                case ScanState.Idle:

                    break;
            }
        }


        // ============================================================
        // FINISH
        // ============================================================

        private void FinishSequence()
        {
            IsPlaying =
                false;


            State =
                ScanState.Idle;


            Progress =
                1f;


            stateTime =
                0f;


            loopTimer =
                0f;


            onSequenceFinished?.Invoke();


            if (loop)
            {
                loopTimer =
                    loopDelay;
            }
            else
            {
                vfx.ResetVFX();
            }
        }


        // ============================================================
        // LOOP
        // ============================================================

        private void UpdateLoop(
            float deltaTime)
        {
            if (!loop)
            {
                return;
            }


            if (
                loopTimer <=
                0f
            )
            {
                return;
            }


            loopTimer -=
                deltaTime;


            if (
                loopTimer <=
                0f
            )
            {
                StartScan(
                    resultAssigned
                        ? Result
                        : defaultResultIsSuccess
                            ? ScanResult.Success
                            : ScanResult.Failure
                );
            }
        }


        // ============================================================
        // RESULT
        // ============================================================

        private void ApplyResultText()
        {
            if (
                Result ==
                ScanResult.Success
            )
            {
                if (
                    resultTitle != null
                )
                {
                    resultTitle.text =
                        successTitle;
                }


                if (
                    resultValue != null
                )
                {
                    resultValue.text =
                        successValue;
                }


                if (
                    resultStatus != null
                )
                {
                    resultStatus.text =
                        successStatus;
                }
            }
            else
            {
                if (
                    resultTitle != null
                )
                {
                    resultTitle.text =
                        failureTitle;
                }


                if (
                    resultValue != null
                )
                {
                    resultValue.text =
                        failureValue;
                }


                if (
                    resultStatus != null
                )
                {
                    resultStatus.text =
                        failureStatus;
                }
            }
        }


        // ============================================================
        // RESULT ALPHA
        // ============================================================

        private void SetResultAlpha(
            float alpha)
        {
            if (
                resultGroup == null
            )
            {
                return;
            }


            resultGroup.alpha =
                Mathf.Clamp01(
                    alpha
                );


            resultGroup.interactable =
                alpha >
                0.99f;


            resultGroup.blocksRaycasts =
                alpha >
                0.99f;
        }


        // ============================================================
        // HIDE RESULT
        // ============================================================

        private void HideResultInstant()
        {
            if (
                resultGroup != null
            )
            {
                resultGroup.alpha =
                    0f;


                resultGroup.interactable =
                    false;


                resultGroup.blocksRaycasts =
                    false;
            }


            if (
                clearResultTextOnReset
            )
            {
                if (
                    resultTitle != null
                )
                {
                    resultTitle.text =
                        string.Empty;
                }


                if (
                    resultValue != null
                )
                {
                    resultValue.text =
                        string.Empty;
                }


                if (
                    resultStatus != null
                )
                {
                    resultStatus.text =
                        string.Empty;
                }
            }
           // scanOverlay.Hide();
        }


        // ============================================================
        // RESET
        // ============================================================

        public void ResetSequence()
        {
            IsPlaying =
                false;


            State =
                ScanState.Idle;


            Result =
                ScanResult.None;


            Progress =
                0f;


            stateTime =
                0f;


            loopTimer =
                0f;


            resultAssigned =
                false;


            HideResultInstant();


            if (vfx != null)
            {
                vfx.ResetVFX();
            }
        }


        // ============================================================
        // FORCE RESULT
        // ============================================================

        public void ShowSuccessResult()
        {
            Result =
                ScanResult.Success;


            resultAssigned =
                true;


            ApplyResultText();


            SetResultAlpha(
                1f
            );
        }


        public void ShowFailureResult()
        {
            Result =
                ScanResult.Failure;


            resultAssigned =
                true;


            ApplyResultText();


            SetResultAlpha(
                1f
            );
        }


        // ============================================================
        // TIME
        // ============================================================

        private float NormalizeTime(
            float time,
            float duration)
        {
            if (
                duration <=
                0f
            )
            {
                return 1f;
            }


            return Mathf.Clamp01(
                time /
                duration
            );
        }
    }
}

