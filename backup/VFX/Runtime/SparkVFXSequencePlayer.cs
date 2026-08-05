
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectSpark.UI.VFX
{
    /// <summary>
    /// Plays SparkVFXSequence assets on any compatible VFX target.
    ///
    /// Architecture:
    ///
    /// SparkVFXSequencePlayer
    ///          |
    ///          v
    ///    SparkVFXTarget
    ///          |
    ///          v
    /// ISparkVFXController
    ///
    /// Responsibilities:
    /// - Sequence playback.
    /// - Forward / reverse playback.
    /// - Looping.
    /// - Pause / resume.
    /// - Timeline evaluation.
    /// - Keyframe interpolation.
    /// - Applying sequence values to the controller.
    ///
    /// This class does NOT:
    /// - Resolve logical states.
    /// - Manage loops from SparkVFXLoop.
    /// - Manage overrides.
    /// - Manage layer priority.
    ///
    /// SparkVFXLayeredStateMachine owns those responsibilities.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkVFXSequencePlayer
        : MonoBehaviour
    {
        // ============================================================
        // REFERENCES
        // ============================================================

        [Header("References")]

        [Tooltip(
            "Central target resolver. " +
            "Finds SparkVFXController or SparkTMPVFXController."
        )]
        [SerializeField]
        private SparkVFXTarget target;


        // ============================================================
        // SEQUENCE
        // ============================================================

        [Header("Sequence")]

        [SerializeField]
        private SparkVFXSequence sequence;


        // ============================================================
        // PLAYBACK
        // ============================================================

        [Header("Playback")]

        [SerializeField]
        private bool playOnEnable =
            false;


        [SerializeField]
        private bool loop =
            false;


        [SerializeField]
        private bool useUnscaledTime =
            true;


        [SerializeField]
        private bool playReverse =
            false;


        // ============================================================
        // INITIAL STATE
        // ============================================================

        [Header("Initial State")]

        [SerializeField]
        private bool applyFirstKeyframeOnInitialize =
            true;

        public event Action SequenceCompleted;

       
// ============================================================
// RAISE COMPLETION
// ============================================================

         private void RaiseSequenceCompleted()
        {
            SequenceCompleted?.Invoke();
        }




        // ============================================================
        // RUNTIME
        // ============================================================

        private ISparkVFXController controller;


        private Coroutine playbackRoutine;


        private bool initialized;


        private bool resolving;


        private bool playing;


        private bool paused;


        private float playbackTime;


        // ============================================================
        // PUBLIC PROPERTIES
        // ============================================================

        public SparkVFXSequence Sequence
        {
            get
            {
                return sequence;
            }

            set
            {
                SetSequence(
                    value
                );
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


        public bool IsPlaying
        {
            get
            {
                return playing;
            }
        }


        public bool IsPaused
        {
            get
            {
                return paused;
            }
        }


        public float PlaybackTime
        {
            get
            {
                return playbackTime;
            }
        }


        public float NormalizedTime
        {
            get
            {
                if (
                    sequence == null ||
                    sequence.Duration <=
                    Mathf.Epsilon
                )
                {
                    return 0f;
                }


                return Mathf.Clamp01(
                    playbackTime /
                    sequence.Duration
                );
            }
        }


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
                Play();
            }
        }


        private void OnDisable()
        {
            Stop();
        }


        private void OnDestroy()
        {
            Stop();
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


            ResolveTarget();


            if (target == null)
            {
                Debug.LogError(
                    "[SparkVFXSequencePlayer] " +
                    "No SparkVFXTarget found. " +
                    "Add SparkVFXTarget to this GameObject, " +
                    "a parent, or a child.",
                    this
                );


                resolving =
                    false;

                return;
            }


            controller =
                target.Controller;


            if (controller == null)
            {
                Debug.LogError(
                    "[SparkVFXSequencePlayer] " +
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


            controller.Initialize();


            initialized =
                true;


            resolving =
                false;


            if (
                applyFirstKeyframeOnInitialize
            )
            {
                ApplyFirstKeyframe();
            }
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


            target =
                GetComponent<
                    SparkVFXTarget
                >();


            if (target != null)
            {
                return;
            }


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


            if (sequence == null)
            {
                Debug.LogWarning(
                    "[SparkVFXSequencePlayer] " +
                    "No SparkVFXSequence assigned.",
                    this
                );

                return;
            }


            if (
                sequence.Keyframes == null ||
                sequence.Keyframes.Count == 0
            )
            {
                Debug.LogWarning(
                    "[SparkVFXSequencePlayer] " +
                    "Sequence contains no keyframes.",
                    this
                );

                return;
            }


            Stop();


            paused =
                false;


            playing =
                true;


            playbackTime =
                playReverse
                    ? Mathf.Max(
                        0f,
                        sequence.Duration
                    )
                    : 0f;


            EvaluateAtTime(
                playbackTime
            );


            playbackRoutine =
                StartCoroutine(
                    PlaybackRoutine()
                );
        }


        // ============================================================
        // RESTART
        // ============================================================

        public void Restart()
        {
            Play();
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


            paused =
                true;
        }


        // ============================================================
        // RESUME
        // ============================================================

        public void Resume()
        {
            if (!playing)
            {
                return;
            }


            paused =
                false;
        }


        // ============================================================
        // STOP
        // ============================================================

        public void Stop()
        {
            playing =
                false;


            paused =
                false;


            if (playbackRoutine != null)
            {
                StopCoroutine(
                    playbackRoutine
                );


                playbackRoutine =
                    null;
            }
        }


        // ============================================================
        // PLAYBACK ROUTINE
        // ============================================================

        
private IEnumerator PlaybackRoutine()
        {
            float duration =
                Mathf.Max(
                    0.01f,
                    sequence.Duration
                );


            while (playing)
            {
                if (paused)
                {
                    yield return null;

                    continue;
                }


                float deltaTime =
                    useUnscaledTime
                        ? Time.unscaledDeltaTime
                        : Time.deltaTime;


                // ========================================================
                // UPDATE PLAYBACK TIME
                // ========================================================

                if (playReverse)
                {
                    playbackTime -=
                        deltaTime;
                }
                else
                {
                    playbackTime +=
                        deltaTime;
                }


                // ========================================================
                // CLAMP PLAYBACK TIME
                // ========================================================

                playbackTime =
                    Mathf.Clamp(
                        playbackTime,
                        0f,
                        duration
                    );


                // ========================================================
                // EVALUATE CURRENT FRAME
                // ========================================================

                EvaluateAtTime(
                    playbackTime
                );


                // ========================================================
                // CHECK END
                // ========================================================

                bool reachedEnd =
                    playReverse
                        ? playbackTime <= 0f
                        : playbackTime >= duration;


                if (reachedEnd)
                {
                    // ====================================================
                    // LOOP
                    // ====================================================

                    if (loop)
                    {
                        playbackTime =
                            playReverse
                                ? duration
                                : 0f;


                        EvaluateAtTime(
                            playbackTime
                        );
                    }
                    else
                    {
                        // ================================================
                        // NATURAL SEQUENCE COMPLETION
                        // ================================================

                        playing =
                            false;


                        paused =
                            false;


                        playbackRoutine =
                            null;


                        // ================================================
                        // NOTIFY LAYERED STATE MACHINE
                        // ================================================

                        RaiseSequenceCompleted();


                        yield break;
                    }
                }


                yield return null;
            }


            // ============================================================
            // SAFETY CLEANUP
            // ============================================================

            playbackRoutine =
                null;
        }



        // ============================================================
        // EVALUATE SEQUENCE
        // ============================================================

        private void EvaluateAtTime(
            float time)
        {
            if (
                controller == null ||
                sequence == null
            )
            {
                return;
            }


            IReadOnlyList<
                SparkVFXKeyframe
            > keyframes =
                sequence.Keyframes;


            if (
                keyframes == null ||
                keyframes.Count == 0
            )
            {
                return;
            }


            SparkVFXKeyframe first =
                GetFirstKeyframe(
                    keyframes
                );


            SparkVFXKeyframe last =
                GetLastKeyframe(
                    keyframes
                );


            if (
                first == null ||
                last == null
            )
            {
                return;
            }


            // --------------------------------------------------------
            // SINGLE KEYFRAME
            // --------------------------------------------------------

            if (
                first == last
            )
            {
                ApplyKeyframe(
                    first
                );

                return;
            }


            // --------------------------------------------------------
            // NORMALIZED TIME
            // --------------------------------------------------------

            float duration =
                Mathf.Max(
                    0.01f,
                    sequence.Duration
                );


            float normalized =
                Mathf.Clamp01(
                    time /
                    duration
                );


            // --------------------------------------------------------
            // SEQUENCE CURVE
            // --------------------------------------------------------

            if (
                sequence.SequenceCurve !=
                null
            )
            {
                normalized =
                    Mathf.Clamp01(
                        sequence.SequenceCurve.Evaluate(
                            normalized
                        )
                    );
            }


            // --------------------------------------------------------
            // BEFORE FIRST
            // --------------------------------------------------------

            if (
                normalized <=
                first.time
            )
            {
                ApplyKeyframe(
                    first
                );

                return;
            }


            // --------------------------------------------------------
            // AFTER LAST
            // --------------------------------------------------------

            if (
                normalized >=
                last.time
            )
            {
                ApplyKeyframe(
                    last
                );

                return;
            }


            // --------------------------------------------------------
            // FIND SURROUNDING KEYFRAMES
            // --------------------------------------------------------

            SparkVFXKeyframe previous =
                first;


            SparkVFXKeyframe next =
                last;


            for (
                int i = 0;
                i <
                keyframes.Count;
                i++
            )
            {
                SparkVFXKeyframe candidate =
                    keyframes[i];


                if (candidate == null)
                {
                    continue;
                }


                if (
                    candidate.time <=
                    normalized
                )
                {
                    previous =
                        candidate;

                    continue;
                }


                next =
                    candidate;

                break;
            }


            // --------------------------------------------------------
            // INTERPOLATION
            // --------------------------------------------------------

            float range =
                next.time -
                previous.time;


            float t =
                range <=
                0.0001f
                    ? 1f
                    : Mathf.InverseLerp(
                        previous.time,
                        next.time,
                        normalized
                    );


            ApplyInterpolatedValues(
                previous,
                next,
                t
            );
        }


        // ============================================================
        // GET FIRST KEYFRAME
        // ============================================================

        private SparkVFXKeyframe
            GetFirstKeyframe(
                IReadOnlyList<
                    SparkVFXKeyframe
                > keyframes)
        {
            SparkVFXKeyframe result =
                null;


            for (
                int i = 0;
                i < keyframes.Count;
                i++
            )
            {
                SparkVFXKeyframe candidate =
                    keyframes[i];


                if (candidate == null)
                {
                    continue;
                }


                if (
                    result == null ||
                    candidate.time <
                    result.time
                )
                {
                    result =
                        candidate;
                }
            }


            return result;
        }


        // ============================================================
        // GET LAST KEYFRAME
        // ============================================================

        private SparkVFXKeyframe
            GetLastKeyframe(
                IReadOnlyList<
                    SparkVFXKeyframe
                > keyframes)
        {
            SparkVFXKeyframe result =
                null;


            for (
                int i = 0;
                i < keyframes.Count;
                i++
            )
            {
                SparkVFXKeyframe candidate =
                    keyframes[i];


                if (candidate == null)
                {
                    continue;
                }


                if (
                    result == null ||
                    candidate.time >
                    result.time
                )
                {
                    result =
                        candidate;
                }
            }


            return result;
        }


        // ============================================================
        // APPLY FIRST KEYFRAME
        // ============================================================

        public void ApplyFirstKeyframe()
        {
            if (!EnsureReady())
            {
                return;
            }


            if (sequence == null)
            {
                return;
            }


            IReadOnlyList<
                SparkVFXKeyframe
            > keyframes =
                sequence.Keyframes;


            if (
                keyframes == null ||
                keyframes.Count == 0
            )
            {
                return;
            }


            SparkVFXKeyframe first =
                GetFirstKeyframe(
                    keyframes
                );


            ApplyKeyframe(
                first
            );
        }


        // ============================================================
        // APPLY LAST KEYFRAME
        // ============================================================

        public void ApplyLastKeyframe()
        {
            if (!EnsureReady())
            {
                return;
            }


            if (sequence == null)
            {
                return;
            }


            IReadOnlyList<
                SparkVFXKeyframe
            > keyframes =
                sequence.Keyframes;


            if (
                keyframes == null ||
                keyframes.Count == 0
            )
            {
                return;
            }


            SparkVFXKeyframe last =
                GetLastKeyframe(
                    keyframes
                );


            ApplyKeyframe(
                last
            );
        }


        // ============================================================
        // APPLY KEYFRAME
        // ============================================================

        private void ApplyKeyframe(
            SparkVFXKeyframe keyframe)
        {
            if (
                keyframe == null ||
                controller == null
            )
            {
                return;
            }


            controller.SetGlowValue(
                keyframe.glow
            );


            controller.SetScanValue(
                keyframe.scan
            );


            controller.SetSweepValue(
                keyframe.sweep
            );


            controller.SetSweepPositionValue(
                keyframe.sweepPosition
            );


            controller.SetFlashValue(
                keyframe.flash
            );


            controller.SetGlitchValue(
                keyframe.glitch
            );


            controller.SetFlickerValue(
                keyframe.flicker
            );


            controller.SetRevealValue(
                keyframe.reveal
            );


            controller.SetDissolveValue(
                keyframe.dissolve
            );
        }


        // ============================================================
        // APPLY INTERPOLATED VALUES
        // ============================================================

        private void ApplyInterpolatedValues(
            SparkVFXKeyframe a,
            SparkVFXKeyframe b,
            float t)
        {
            if (
                a == null ||
                b == null ||
                controller == null
            )
            {
                return;
            }


            t =
                Mathf.Clamp01(
                    t
                );


            controller.SetGlowValue(
                Mathf.LerpUnclamped(
                    a.glow,
                    b.glow,
                    t
                )
            );


            controller.SetScanValue(
                Mathf.LerpUnclamped(
                    a.scan,
                    b.scan,
                    t
                )
            );


            controller.SetSweepValue(
                Mathf.LerpUnclamped(
                    a.sweep,
                    b.sweep,
                    t
                )
            );


            controller.SetSweepPositionValue(
                Mathf.LerpUnclamped(
                    a.sweepPosition,
                    b.sweepPosition,
                    t
                )
            );


            controller.SetFlashValue(
                Mathf.LerpUnclamped(
                    a.flash,
                    b.flash,
                    t
                )
            );


            controller.SetGlitchValue(
                Mathf.LerpUnclamped(
                    a.glitch,
                    b.glitch,
                    t
                )
            );


            controller.SetFlickerValue(
                Mathf.LerpUnclamped(
                    a.flicker,
                    b.flicker,
                    t
                )
            );


            controller.SetRevealValue(
                Mathf.LerpUnclamped(
                    a.reveal,
                    b.reveal,
                    t
                )
            );


            controller.SetDissolveValue(
                Mathf.LerpUnclamped(
                    a.dissolve,
                    b.dissolve,
                    t
                )
            );
        }


        // ============================================================
        // SET SEQUENCE
        // ============================================================

        public void SetSequence(
            SparkVFXSequence newSequence)
        {
            Stop();


            sequence =
                newSequence;


            if (initialized)
            {
                ApplyFirstKeyframe();
            }
        }


        // ============================================================
        // SET TARGET
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
        // SET PLAYBACK TIME
        // ============================================================

        public void SetPlaybackTime(
            float time)
        {
            if (!EnsureReady())
            {
                return;
            }


            if (sequence == null)
            {
                return;
            }


            playbackTime =
                Mathf.Clamp(
                    time,
                    0f,
                    sequence.Duration
                );


            EvaluateAtTime(
                playbackTime
            );
        }


        // ============================================================
        // SET NORMALIZED TIME
        // ============================================================

        public void SetNormalizedTime(
            float normalizedTime)
        {
            if (!EnsureReady())
            {
                return;
            }


            if (sequence == null)
            {
                return;
            }


            float duration =
                Mathf.Max(
                    0.01f,
                    sequence.Duration
                );


            playbackTime =
                Mathf.Clamp01(
                    normalizedTime
                ) *
                duration;


            EvaluateAtTime(
                playbackTime
            );
        }


        // ============================================================
        // PLAY FORWARD
        // ============================================================

        public void PlayForward()
        {
            playReverse =
                false;


            Play();
        }


        // ============================================================
        // PLAY REVERSE
        // ============================================================

        public void PlayReverse()
        {
            playReverse =
                true;


            Play();
        }


        // ============================================================
        // SET LOOP
        // ============================================================

        public void SetLoop(
            bool value)
        {
            loop =
                value;
        }


        // ============================================================
        // SET UNSCALED TIME
        // ============================================================

        public void SetUseUnscaledTime(
            bool value)
        {
            useUnscaledTime =
                value;
        }


        // ============================================================
        // VALIDATION
        // ============================================================

        private void OnValidate()
        {
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

