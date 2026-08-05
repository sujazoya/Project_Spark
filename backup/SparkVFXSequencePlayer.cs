using System.Collections;
using UnityEngine;

namespace ProjectSpark.UI.VFX
{
    [DisallowMultipleComponent]
    [RequireComponent(
        typeof(SparkVFXController))]
    public sealed class SparkVFXSequencePlayer
        : MonoBehaviour
    {
        // ============================================================
        // REFERENCES
        // ============================================================

        [Header("References")]

        [SerializeField]
        private SparkVFXController controller;


        // ============================================================
        // STATE
        // ============================================================

        private Coroutine activeSequence;


        // ============================================================
        // INITIALIZE
        // ============================================================

        private void Awake()
        {
            if (controller == null)
            {
                controller =
                    GetComponent<
                        SparkVFXController>();
            }
        }


        // ============================================================
        // PLAY
        // ============================================================

        public void Play(
            SparkVFXSequence sequence)
        {
            if (sequence == null)
            {
                return;
            }


            if (activeSequence != null)
            {
                StopCoroutine(
                    activeSequence
                );
            }


            activeSequence =
                StartCoroutine(
                    PlaySequence(
                        sequence
                    )
                );
        }


        // ============================================================
        // STOP
        // ============================================================

        public void Stop()
        {
            if (activeSequence == null)
            {
                return;
            }


            StopCoroutine(
                activeSequence
            );


            activeSequence =
                null;
        }


        // ============================================================
        // PLAY SEQUENCE
        // ============================================================

        private IEnumerator PlaySequence(
            SparkVFXSequence sequence)
        {
            if (
                sequence.Keyframes == null ||
                sequence.Keyframes.Count == 0
            )
            {
                yield break;
            }


            float time =
                0f;


            float duration =
                Mathf.Max(
                    0.01f,
                    sequence.Duration
                );


            while (time < duration)
            {
                time +=
                    Time.unscaledDeltaTime;


                float normalizedTime =
                    Mathf.Clamp01(
                        time /
                        duration
                    );


                float curveValue =
                    sequence.SequenceCurve != null
                    ? sequence.SequenceCurve.Evaluate(
                        normalizedTime
                    )
                    : normalizedTime;


                EvaluateSequence(
                    sequence,
                    curveValue
                );


                yield return null;
            }


            EvaluateSequence(
                sequence,
                1f
            );


            activeSequence =
                null;
        }


        // ============================================================
        // EVALUATE
        // ============================================================

        private void EvaluateSequence(
            SparkVFXSequence sequence,
            float normalizedTime)
        {
            SparkVFXKeyframe previous =
                sequence.Keyframes[0];


            SparkVFXKeyframe next =
                sequence.Keyframes[
                    sequence.Keyframes.Count - 1
                ];


            for (
                int i = 0;
                i <
                sequence.Keyframes.Count - 1;
                i++
            )
            {
                SparkVFXKeyframe a =
                    sequence.Keyframes[i];


                SparkVFXKeyframe b =
                    sequence.Keyframes[i + 1];


                if (
                    normalizedTime >= a.time &&
                    normalizedTime <= b.time
                )
                {
                    previous =
                        a;

                    next =
                        b;

                    break;
                }
            }


            float range =
                next.time -
                previous.time;


            float localTime =
                range > 0f
                ? Mathf.InverseLerp(
                    previous.time,
                    next.time,
                    normalizedTime
                )
                : 0f;


            ApplyInterpolatedValues(
                previous,
                next,
                localTime
            );
        }


        // ============================================================
        // INTERPOLATE
        // ============================================================

        private void ApplyInterpolatedValues(
            SparkVFXKeyframe a,
            SparkVFXKeyframe b,
            float t)
        {
            controller.SetGlowValue(
                Mathf.Lerp(
                    a.glow,
                    b.glow,
                    t
                )
            );


            controller.SetScanValue(
                Mathf.Lerp(
                    a.scan,
                    b.scan,
                    t
                )
            );


            controller.SetSweepValue(
                Mathf.Lerp(
                    a.sweep,
                    b.sweep,
                    t
                )
            );


            controller.SetFlashValue(
                Mathf.Lerp(
                    a.flash,
                    b.flash,
                    t
                )
            );


            controller.SetGlitchValue(
                Mathf.Lerp(
                    a.glitch,
                    b.glitch,
                    t
                )
            );


            controller.SetFlickerValue(
                Mathf.Lerp(
                    a.flicker,
                    b.flicker,
                    t
                )
            );


            controller.SetRevealValue(
                Mathf.Lerp(
                    a.reveal,
                    b.reveal,
                    t
                )
            );


            controller.SetDissolveValue(
                Mathf.Lerp(
                    a.dissolve,
                    b.dissolve,
                    t
                )
            );


            controller.SetSweepPositionValue(
                Mathf.Lerp(
                    a.sweepPosition,
                    b.sweepPosition,
                    t
                )
            );
        }
    }
}