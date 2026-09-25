using System.Collections;
using TMPro;
using UnityEngine;

namespace ProjectSpark.HolographicViewer
{
    [DisallowMultipleComponent]
    public sealed class HolographicMeasurementAnimation : MonoBehaviour
    {
        [Header("Measurement Visuals")]
        [SerializeField]
        private LineRenderer measurementLine;

        [SerializeField]
        private Transform pointAVisual;

        [SerializeField]
        private Transform pointBVisual;

        [SerializeField]
        private Transform pointCVisual;

        [Header("HUD")]
        [SerializeField]
        private TMP_Text modeText;

        [SerializeField]
        private TMP_Text valueText;

        [Header("Point Reveal")]
        [SerializeField]
        [Min(0f)]
        private float pointRevealDuration = 0.18f;

        [SerializeField]
        [Min(0f)]
        private float pointPulseDuration = 0.22f;

        [SerializeField]
        [Min(0f)]
        private float pointPulseAmount = 0.12f;

        [Header("Line Reveal")]
        [SerializeField]
        [Min(0f)]
        private float lineRevealDuration = 0.35f;

        [Header("HUD Reveal")]
        [SerializeField]
        [Min(0f)]
        private float hudRevealDuration = 0.18f;

        [SerializeField]
        [Min(0f)]
        private float hudScaleAmount = 0.04f;

        [Header("Animation Curves")]
        [SerializeField]
        private AnimationCurve revealCurve =
            AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        [SerializeField]
        private AnimationCurve pulseCurve =
            AnimationCurve.EaseInOut(0f, 0f, 1f, 0f);

        [Header("Safety")]
        [SerializeField]
        [Min(0.000001f)]
        private float minimumAnimatedLineLength = 0.0001f;

        private Coroutine animationRoutine;

        private Vector3 pointAOriginalScale = Vector3.one;
        private Vector3 pointBOriginalScale = Vector3.one;
        private Vector3 pointCOriginalScale = Vector3.one;

        private Vector3 modeOriginalScale = Vector3.one;
        private Vector3 valueOriginalScale = Vector3.one;

        private bool initialized;
        private bool controllingLine;

        public bool IsAnimating
        {
            get
            {
                return animationRoutine != null;
            }
        }

        public bool IsControllingLine
        {
            get
            {
                return controllingLine;
            }
        }

        private void Awake()
        {
            CacheOriginalScales();
            ConfigureLineRenderer();
            initialized = true;
        }

        private void OnDisable()
        {
            Stop();
        }

        private void OnDestroy()
        {
            Stop();
        }

        public void PlayPointA(Vector3 position)
        {
            EnsureInitialized();

            Stop();

            PreparePoint(
                pointAVisual,
                position);

            HidePoint(pointBVisual);
            HidePoint(pointCVisual);

            ClearAnimatedLine();

            animationRoutine = StartCoroutine(
                AnimatePointASequence());
        }

        public void PlayMeasurement(
            Vector3 pointA,
            Vector3 pointB)
        {
            EnsureInitialized();

            Stop();

            PreparePoint(
                pointAVisual,
                pointA);

            PreparePoint(
                pointBVisual,
                pointB);

            HidePoint(pointCVisual);

            controllingLine = true;

            animationRoutine = StartCoroutine(
                AnimateMeasurementSequence(
                    pointA,
                    pointB));
        }

        public void PlayAngle(
            Vector3 pointA,
            Vector3 vertex,
            Vector3 pointC)
        {
            EnsureInitialized();

            Stop();

            PreparePoint(
                pointAVisual,
                pointA);

            PreparePoint(
                pointBVisual,
                vertex);

            PreparePoint(
                pointCVisual,
                pointC);

            controllingLine = true;

            animationRoutine = StartCoroutine(
                AnimateAngleSequence(
                    pointA,
                    vertex,
                    pointC));
        }

        public void PlayHudReveal()
        {
            EnsureInitialized();

            if (modeText == null && valueText == null)
            {
                return;
            }

            if (animationRoutine != null)
            {
                StopCoroutine(animationRoutine);
                animationRoutine = null;
            }

            animationRoutine = StartCoroutine(
                AnimateHudReveal());
        }

        public void Stop()
        {
            if (animationRoutine != null)
            {
                StopCoroutine(animationRoutine);
                animationRoutine = null;
            }

            controllingLine = false;

            RestorePointScale(
                pointAVisual,
                pointAOriginalScale);

            RestorePointScale(
                pointBVisual,
                pointBOriginalScale);

            RestorePointScale(
                pointCVisual,
                pointCOriginalScale);

            RestoreTextScale(
                modeText,
                modeOriginalScale);

            RestoreTextScale(
                valueText,
                valueOriginalScale);

            ClearAnimatedLine();
        }

        private IEnumerator AnimatePointASequence()
        {
            yield return AnimatePointReveal(
                pointAVisual,
                pointAOriginalScale,
                pointRevealDuration);

            yield return AnimatePointPulse(
                pointAVisual,
                pointAOriginalScale);

            FinishAnimation();
        }

        private IEnumerator AnimateMeasurementSequence(
            Vector3 pointA,
            Vector3 pointB)
        {
            yield return AnimatePointReveal(
                pointAVisual,
                pointAOriginalScale,
                pointRevealDuration);

            yield return AnimatePointReveal(
                pointBVisual,
                pointBOriginalScale,
                pointRevealDuration);

            yield return AnimateLine(
                pointA,
                pointB);

            yield return AnimatePointPulse(
                pointBVisual,
                pointBOriginalScale);

            FinishAnimation();
        }

        private IEnumerator AnimateAngleSequence(
            Vector3 pointA,
            Vector3 vertex,
            Vector3 pointC)
        {
            yield return AnimatePointReveal(
                pointAVisual,
                pointAOriginalScale,
                pointRevealDuration);

            yield return AnimatePointReveal(
                pointBVisual,
                pointBOriginalScale,
                pointRevealDuration);

            yield return AnimatePointReveal(
                pointCVisual,
                pointCOriginalScale,
                pointRevealDuration);

            yield return AnimateAngleLine(
                pointA,
                vertex,
                pointC);

            yield return AnimatePointPulse(
                pointCVisual,
                pointCOriginalScale);

            FinishAnimation();
        }

        private IEnumerator AnimatePointReveal(
            Transform point,
            Vector3 originalScale,
            float duration)
        {
            if (point == null)
            {
                yield break;
            }

            if (duration <= 0f)
            {
                point.localScale = originalScale;
                yield break;
            }

            point.localScale = Vector3.zero;

            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;

                float normalized =
                    Mathf.Clamp01(elapsed / duration);

                float evaluated =
                    EvaluateCurve(
                        revealCurve,
                        normalized);

                point.localScale =
                    Vector3.LerpUnclamped(
                        Vector3.zero,
                        originalScale,
                        evaluated);

                yield return null;
            }

            point.localScale = originalScale;
        }

        private IEnumerator AnimatePointPulse(
            Transform point,
            Vector3 originalScale)
        {
            if (point == null)
            {
                yield break;
            }

            if (pointPulseDuration <= 0f ||
                pointPulseAmount <= 0f)
            {
                point.localScale = originalScale;
                yield break;
            }

            float elapsed = 0f;

            while (elapsed < pointPulseDuration)
            {
                elapsed += Time.unscaledDeltaTime;

                float normalized =
                    Mathf.Clamp01(
                        elapsed / pointPulseDuration);

                float evaluated =
                    EvaluateCurve(
                        pulseCurve,
                        normalized);

                float multiplier =
                    1f +
                    (evaluated * pointPulseAmount);

                point.localScale =
                    originalScale * multiplier;

                yield return null;
            }

            point.localScale = originalScale;
        }

        private IEnumerator AnimateLine(
            Vector3 pointA,
            Vector3 pointB)
        {
            if (measurementLine == null)
            {
                yield break;
            }

            float distance =
                Vector3.Distance(
                    pointA,
                    pointB);

            if (distance <= minimumAnimatedLineLength)
            {
                SetLine(
                    pointA,
                    pointB);

                yield break;
            }

            if (lineRevealDuration <= 0f)
            {
                SetLine(
                    pointA,
                    pointB);

                yield break;
            }

            measurementLine.positionCount = 2;
            measurementLine.enabled = true;

            measurementLine.SetPosition(
                0,
                pointA);

            measurementLine.SetPosition(
                1,
                pointA);

            float elapsed = 0f;

            while (elapsed < lineRevealDuration)
            {
                elapsed += Time.unscaledDeltaTime;

                float normalized =
                    Mathf.Clamp01(
                        elapsed / lineRevealDuration);

                float evaluated =
                    EvaluateCurve(
                        revealCurve,
                        normalized);

                Vector3 currentEnd =
                    Vector3.LerpUnclamped(
                        pointA,
                        pointB,
                        evaluated);

                measurementLine.SetPosition(
                    0,
                    pointA);

                measurementLine.SetPosition(
                    1,
                    currentEnd);

                yield return null;
            }

            SetLine(
                pointA,
                pointB);
        }

        private IEnumerator AnimateAngleLine(
            Vector3 pointA,
            Vector3 vertex,
            Vector3 pointC)
        {
            if (measurementLine == null)
            {
                yield break;
            }

            float firstLength =
                Vector3.Distance(
                    pointA,
                    vertex);

            float secondLength =
                Vector3.Distance(
                    vertex,
                    pointC);

            float totalLength =
                firstLength +
                secondLength;

            if (totalLength <= minimumAnimatedLineLength)
            {
                SetAngleLine(
                    pointA,
                    vertex,
                    pointC);

                yield break;
            }

            if (lineRevealDuration <= 0f)
            {
                SetAngleLine(
                    pointA,
                    vertex,
                    pointC);

                yield break;
            }

            measurementLine.positionCount = 3;
            measurementLine.enabled = true;

            measurementLine.SetPosition(
                0,
                pointA);

            measurementLine.SetPosition(
                1,
                pointA);

            measurementLine.SetPosition(
                2,
                pointA);

            float elapsed = 0f;

            while (elapsed < lineRevealDuration)
            {
                elapsed += Time.unscaledDeltaTime;

                float normalized =
                    Mathf.Clamp01(
                        elapsed / lineRevealDuration);

                float evaluated =
                    EvaluateCurve(
                        revealCurve,
                        normalized);

                float travelledDistance =
                    totalLength * evaluated;

                if (travelledDistance <= firstLength)
                {
                    float firstT =
                        firstLength <= 0f
                            ? 1f
                            : travelledDistance /
                              firstLength;

                    Vector3 currentPoint =
                        Vector3.LerpUnclamped(
                            pointA,
                            vertex,
                            firstT);

                    measurementLine.SetPosition(
                        0,
                        pointA);

                    measurementLine.SetPosition(
                        1,
                        currentPoint);

                    measurementLine.SetPosition(
                        2,
                        currentPoint);
                }
                else
                {
                    float secondDistance =
                        travelledDistance -
                        firstLength;

                    float secondT =
                        secondLength <= 0f
                            ? 1f
                            : secondDistance /
                              secondLength;

                    Vector3 currentPoint =
                        Vector3.LerpUnclamped(
                            vertex,
                            pointC,
                            secondT);

                    measurementLine.SetPosition(
                        0,
                        pointA);

                    measurementLine.SetPosition(
                        1,
                        vertex);

                    measurementLine.SetPosition(
                        2,
                        currentPoint);
                }

                yield return null;
            }

            SetAngleLine(
                pointA,
                vertex,
                pointC);
        }

        private IEnumerator AnimateHudReveal()
        {
            Vector3 modeScale =
                modeText != null
                    ? modeText.transform.localScale
                    : Vector3.one;

            Vector3 valueScale =
                valueText != null
                    ? valueText.transform.localScale
                    : Vector3.one;

            modeOriginalScale = modeScale;
            valueOriginalScale = valueScale;

            float reducedScale =
                Mathf.Clamp01(
                    1f - hudScaleAmount);

            if (modeText != null)
            {
                modeText.transform.localScale =
                    modeScale * reducedScale;
            }

            if (valueText != null)
            {
                valueText.transform.localScale =
                    valueScale * reducedScale;
            }

            if (hudRevealDuration <= 0f)
            {
                RestoreTextScale(
                    modeText,
                    modeScale);

                RestoreTextScale(
                    valueText,
                    valueScale);

                animationRoutine = null;
                yield break;
            }

            float elapsed = 0f;

            while (elapsed < hudRevealDuration)
            {
                elapsed += Time.unscaledDeltaTime;

                float normalized =
                    Mathf.Clamp01(
                        elapsed / hudRevealDuration);

                float evaluated =
                    EvaluateCurve(
                        revealCurve,
                        normalized);

                float multiplier =
                    Mathf.Lerp(
                        reducedScale,
                        1f,
                        evaluated);

                if (modeText != null)
                {
                    modeText.transform.localScale =
                        modeScale * multiplier;
                }

                if (valueText != null)
                {
                    valueText.transform.localScale =
                        valueScale * multiplier;
                }

                yield return null;
            }

            RestoreTextScale(
                modeText,
                modeScale);

            RestoreTextScale(
                valueText,
                valueScale);

            animationRoutine = null;
        }

        private void PreparePoint(
            Transform point,
            Vector3 position)
        {
            if (point == null)
            {
                return;
            }

            point.position = position;
            point.gameObject.SetActive(true);
        }

        private void HidePoint(
            Transform point)
        {
            if (point == null)
            {
                return;
            }

            point.gameObject.SetActive(false);
        }

        private void SetLine(
            Vector3 pointA,
            Vector3 pointB)
        {
            if (measurementLine == null)
            {
                return;
            }

            measurementLine.positionCount = 2;

            measurementLine.SetPosition(
                0,
                pointA);

            measurementLine.SetPosition(
                1,
                pointB);

            measurementLine.enabled = true;
        }

        private void SetAngleLine(
            Vector3 pointA,
            Vector3 vertex,
            Vector3 pointC)
        {
            if (measurementLine == null)
            {
                return;
            }

            measurementLine.positionCount = 3;

            measurementLine.SetPosition(
                0,
                pointA);

            measurementLine.SetPosition(
                1,
                vertex);

            measurementLine.SetPosition(
                2,
                pointC);

            measurementLine.enabled = true;
        }

        private void ClearAnimatedLine()
        {
            if (measurementLine == null)
            {
                return;
            }

            measurementLine.enabled = false;
            measurementLine.positionCount = 0;
        }

        private void RestorePointScale(
            Transform point,
            Vector3 originalScale)
        {
            if (point == null)
            {
                return;
            }

            point.localScale = originalScale;
        }

        private void RestoreTextScale(
            TMP_Text text,
            Vector3 originalScale)
        {
            if (text == null)
            {
                return;
            }

            text.transform.localScale =
                originalScale;
        }

        private void FinishAnimation()
        {
            animationRoutine = null;
            controllingLine = false;
        }

        private void CacheOriginalScales()
        {
            if (pointAVisual != null)
            {
                pointAOriginalScale =
                    pointAVisual.localScale;
            }

            if (pointBVisual != null)
            {
                pointBOriginalScale =
                    pointBVisual.localScale;
            }

            if (pointCVisual != null)
            {
                pointCOriginalScale =
                    pointCVisual.localScale;
            }

            if (modeText != null)
            {
                modeOriginalScale =
                    modeText.transform.localScale;
            }

            if (valueText != null)
            {
                valueOriginalScale =
                    valueText.transform.localScale;
            }
        }

        private void EnsureInitialized()
        {
            if (initialized)
            {
                return;
            }

            CacheOriginalScales();
            ConfigureLineRenderer();
            initialized = true;
        }

        private void ConfigureLineRenderer()
        {
            if (measurementLine == null)
            {
                return;
            }

            measurementLine.useWorldSpace = true;
        }

        private float EvaluateCurve(
            AnimationCurve curve,
            float value)
        {
            if (curve == null ||
                curve.length == 0)
            {
                return value;
            }

            return curve.Evaluate(value);
        }
    }
}