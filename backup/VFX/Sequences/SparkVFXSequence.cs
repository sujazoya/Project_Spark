
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectSpark.UI.VFX
{
    [CreateAssetMenu(
        fileName = "SparkVFXSequence",
        menuName = "Project Spark/UI VFX/VFX Sequence"
    )]
    public sealed class SparkVFXSequence
        : ScriptableObject
    {
        // ============================================================
        // IDENTITY
        // ============================================================

        [Header("Sequence")]

        [SerializeField]
        private string sequenceID =
            "New Sequence";


        // ============================================================
        // TIMELINE
        // ============================================================

        [Header("Timeline")]

        [Min(0.01f)]
        [SerializeField]
        private float duration =
            0.5f;


        [SerializeField]
        private AnimationCurve sequenceCurve =
            AnimationCurve.EaseInOut(
                0f,
                0f,
                1f,
                1f
            );


        // ============================================================
        // KEYFRAMES
        // ============================================================

        [Header("Keyframes")]

        [SerializeField]
        private List<SparkVFXKeyframe> keyframes =
            new List<SparkVFXKeyframe>();


        // ============================================================
        // PUBLIC ACCESS
        // ============================================================

        public string SequenceID
        {
            get
            {
                return sequenceID;
            }
        }


        public float Duration
        {
            get
            {
                return duration;
            }
        }


        public AnimationCurve SequenceCurve
        {
            get
            {
                return sequenceCurve;
            }
        }


        public IReadOnlyList<
            SparkVFXKeyframe
        > Keyframes
        {
            get
            {
                return keyframes;
            }
        }


        // ============================================================
        // EVALUATE
        // ============================================================

        /// <summary>
        /// Evaluates the sequence at the specified playback time.
        ///
        /// Time is expressed in seconds.
        ///
        /// The sequence:
        /// 1. Converts time to normalized time.
        /// 2. Applies the sequence animation curve.
        /// 3. Finds the surrounding keyframes.
        /// 4. Interpolates all VFX values.
        /// </summary>
        public SparkVFXKeyframe Evaluate(
            float time)
        {
            if (
                keyframes == null ||
                keyframes.Count == 0
            )
            {
                return null;
            }


            // --------------------------------------------------------
            // GET VALID KEYFRAMES
            // --------------------------------------------------------

            SparkVFXKeyframe first =
                null;


            SparkVFXKeyframe last =
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


                if (first == null)
                {
                    first =
                        candidate;
                }


                last =
                    candidate;
            }


            if (
                first == null ||
                last == null
            )
            {
                return null;
            }


            // --------------------------------------------------------
            // NORMALIZE TIME
            // --------------------------------------------------------

            float normalizedTime =
                duration <= 0f
                    ? 0f
                    : Mathf.Clamp01(
                        time /
                        duration
                    );


            // --------------------------------------------------------
            // APPLY SEQUENCE CURVE
            // --------------------------------------------------------

            float evaluatedTime =
                sequenceCurve != null
                    ? sequenceCurve.Evaluate(
                        normalizedTime
                    )
                    : normalizedTime;


            evaluatedTime =
                Mathf.Clamp01(
                    evaluatedTime
                );


            // --------------------------------------------------------
            // FIND FIRST / LAST VALID KEYFRAME
            // --------------------------------------------------------

            first =
                GetFirstValidKeyframe();


            last =
                GetLastValidKeyframe();


            if (
                first == null ||
                last == null
            )
            {
                return null;
            }


            // --------------------------------------------------------
            // BEFORE FIRST KEYFRAME
            // --------------------------------------------------------

            if (
                evaluatedTime <=
                first.time
            )
            {
                return CreateEvaluatedKeyframe(
                    first
                );
            }


            // --------------------------------------------------------
            // AFTER LAST KEYFRAME
            // --------------------------------------------------------

            if (
                evaluatedTime >=
                last.time
            )
            {
                return CreateEvaluatedKeyframe(
                    last
                );
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
                    candidate.time <=
                    evaluatedTime
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
            // CALCULATE INTERPOLATION
            // --------------------------------------------------------

            float range =
                next.time -
                previous.time;


            float interpolation =
                range <=
                Mathf.Epsilon
                    ? 0f
                    : (
                        evaluatedTime -
                        previous.time
                    ) /
                    range;


            interpolation =
                Mathf.Clamp01(
                    interpolation
                );


            // --------------------------------------------------------
            // INTERPOLATE
            // --------------------------------------------------------

            return InterpolateKeyframes(
                previous,
                next,
                interpolation
            );
        }


        // ============================================================
        // GET FIRST VALID KEYFRAME
        // ============================================================

        private SparkVFXKeyframe
            GetFirstValidKeyframe()
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
        // GET LAST VALID KEYFRAME
        // ============================================================

        private SparkVFXKeyframe
            GetLastValidKeyframe()
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
        // CREATE EVALUATED KEYFRAME
        // ============================================================

        private SparkVFXKeyframe
            CreateEvaluatedKeyframe(
                SparkVFXKeyframe source)
        {
            if (source == null)
            {
                return null;
            }


            return new SparkVFXKeyframe
            {
                time =
                    source.time,

                glow =
                    source.glow,

                scan =
                    source.scan,

                sweep =
                    source.sweep,

                flash =
                    source.flash,

                glitch =
                    source.glitch,

                flicker =
                    source.flicker,

                reveal =
                    source.reveal,

                dissolve =
                    source.dissolve,

                sweepPosition =
                    source.sweepPosition
            };
        }


        // ============================================================
        // INTERPOLATE KEYFRAMES
        // ============================================================

        private SparkVFXKeyframe
            InterpolateKeyframes(
                SparkVFXKeyframe from,
                SparkVFXKeyframe to,
                float t)
        {
            if (from == null)
            {
                return CreateEvaluatedKeyframe(
                    to
                );
            }


            if (to == null)
            {
                return CreateEvaluatedKeyframe(
                    from
                );
            }


            t =
                Mathf.Clamp01(
                    t
                );


            return new SparkVFXKeyframe
            {
                time =
                    Mathf.Lerp(
                        from.time,
                        to.time,
                        t
                    ),

                glow =
                    Mathf.Lerp(
                        from.glow,
                        to.glow,
                        t
                    ),

                scan =
                    Mathf.Lerp(
                        from.scan,
                        to.scan,
                        t
                    ),

                sweep =
                    Mathf.Lerp(
                        from.sweep,
                        to.sweep,
                        t
                    ),

                flash =
                    Mathf.Lerp(
                        from.flash,
                        to.flash,
                        t
                    ),

                glitch =
                    Mathf.Lerp(
                        from.glitch,
                        to.glitch,
                        t
                    ),

                flicker =
                    Mathf.Lerp(
                        from.flicker,
                        to.flicker,
                        t
                    ),

                reveal =
                    Mathf.Lerp(
                        from.reveal,
                        to.reveal,
                        t
                    ),

                dissolve =
                    Mathf.Lerp(
                        from.dissolve,
                        to.dissolve,
                        t
                    ),

                sweepPosition =
                    Mathf.Lerp(
                        from.sweepPosition,
                        to.sweepPosition,
                        t
                    )
            };
        }
    }


    // ================================================================
    // KEYFRAME
    // ================================================================

    [Serializable]
    public sealed class SparkVFXKeyframe
    {
        // ============================================================
        // TIME
        // ============================================================

        [Range(0f, 1f)]
        public float time;


        // ============================================================
        // VFX VALUES
        // ============================================================

        [Header("VFX Values")]

        [Range(0f, 5f)]
        public float glow;


        [Range(0f, 5f)]
        public float scan;


        [Range(0f, 5f)]
        public float sweep;


        [Range(0f, 5f)]
        public float flash;


        [Range(0f, 1f)]
        public float glitch;


        [Range(0f, 1f)]
        public float flicker;


        [Range(0f, 1f)]
        public float reveal =
            1f;


        [Range(0f, 1f)]
        public float dissolve;


        [Range(-1f, 2f)]
        public float sweepPosition =
            0.5f;
    }
}
