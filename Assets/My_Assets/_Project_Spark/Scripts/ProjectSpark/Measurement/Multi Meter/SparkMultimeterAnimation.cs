using System.Collections;
using TMPro;
using UnityEngine;

namespace ProjectSpark.Measurement
{
    [DisallowMultipleComponent]
    public sealed class SparkMultimeterAnimation : MonoBehaviour
    {
        [Header("References")]

        [SerializeField]
        private Transform displayTransform;

        [SerializeField]
        private TMP_Text valueText;

        [SerializeField]
        private CanvasGroup displayCanvas;

        [Header("Display Animation")]

        [SerializeField, Min(0.01f)]
        private float valuePulseDuration = 0.12f;

        [SerializeField, Min(0f)]
        private float valuePulseScale = 0.035f;

        [SerializeField, Min(0.01f)]
        private float powerOnDuration = 0.25f;

        [Header("Mode Animation")]

        [SerializeField, Min(0.01f)]
        private float modeFlashDuration = 0.12f;

        [SerializeField, Min(0f)]
        private float modeFlashScale = 0.05f;

        private Vector3 displayOriginalScale;
        private Vector3 valueOriginalScale;

        private Coroutine valueAnimation;
        private Coroutine powerAnimation;
        private Coroutine modeAnimation;

        private void Awake()
        {
            if (displayTransform != null)
            {
                displayOriginalScale =
                    displayTransform.localScale;
            }

            if (valueText != null)
            {
                valueOriginalScale =
                    valueText.transform.localScale;
            }
        }

        public void PlayReading(
            SparkMultimeterReading reading)
        {
            if (!reading.Valid)
            {
                return;
            }

            if (valueText == null)
            {
                return;
            }

            if (valueAnimation != null)
            {
                StopCoroutine(
                    valueAnimation);
            }

            valueAnimation =
                StartCoroutine(
                    AnimateValue());
        }

        public void PlayPowerOn()
        {
            if (powerAnimation != null)
            {
                StopCoroutine(
                    powerAnimation);
            }

            powerAnimation =
                StartCoroutine(
                    AnimatePowerOn());
        }

        public void PlayPowerOff()
        {
            StopAllAnimations();

            if (displayCanvas != null)
            {
                displayCanvas.alpha = 0.35f;
            }

            if (displayTransform != null)
            {
                displayTransform.localScale =
                    displayOriginalScale;
            }

            if (valueText != null)
            {
                valueText.transform.localScale =
                    valueOriginalScale;
            }
        }

        public void PlayModeChange()
        {
            if (modeAnimation != null)
            {
                StopCoroutine(
                    modeAnimation);
            }

            modeAnimation =
                StartCoroutine(
                    AnimateMode());
        }

        private IEnumerator AnimateValue()
        {
            Transform target =
                valueText.transform;

            Vector3 baseScale =
                valueOriginalScale;

            Vector3 peakScale =
                baseScale *
                (1f + valuePulseScale);

            float elapsed = 0f;

            while (elapsed <
                   valuePulseDuration)
            {
                elapsed +=
                    Time.unscaledDeltaTime;

                float t =
                    Mathf.Clamp01(
                        elapsed /
                        valuePulseDuration);

                float eased =
                    Mathf.Sin(
                        t *
                        Mathf.PI);

                target.localScale =
                    Vector3.LerpUnclamped(
                        baseScale,
                        peakScale,
                        eased);

                yield return null;
            }

            target.localScale =
                baseScale;

            valueAnimation = null;
        }

        private IEnumerator AnimatePowerOn()
        {
            if (displayCanvas != null)
            {
                displayCanvas.alpha = 0f;
            }

            if (displayTransform != null)
            {
                displayTransform.localScale =
                    displayOriginalScale *
                    0.94f;
            }

            float elapsed = 0f;

            while (elapsed <
                   powerOnDuration)
            {
                elapsed +=
                    Time.unscaledDeltaTime;

                float t =
                    Mathf.Clamp01(
                        elapsed /
                        powerOnDuration);

                float eased =
                    1f -
                    Mathf.Pow(
                        1f - t,
                        3f);

                if (displayCanvas != null)
                {
                    displayCanvas.alpha =
                        Mathf.Lerp(
                            0f,
                            1f,
                            eased);
                }

                if (displayTransform != null)
                {
                    displayTransform.localScale =
                        Vector3.Lerp(
                            displayOriginalScale *
                            0.94f,
                            displayOriginalScale,
                            eased);
                }

                yield return null;
            }

            if (displayCanvas != null)
            {
                displayCanvas.alpha = 1f;
            }

            if (displayTransform != null)
            {
                displayTransform.localScale =
                    displayOriginalScale;
            }

            powerAnimation = null;
        }

        private IEnumerator AnimateMode()
        {
            if (valueText == null)
            {
                yield break;
            }

            Transform target =
                valueText.transform;

            Vector3 baseScale =
                valueOriginalScale;

            Vector3 peakScale =
                baseScale *
                (1f + modeFlashScale);

            float elapsed = 0f;

            while (elapsed <
                   modeFlashDuration)
            {
                elapsed +=
                    Time.unscaledDeltaTime;

                float t =
                    Mathf.Clamp01(
                        elapsed /
                        modeFlashDuration);

                float eased =
                    Mathf.Sin(
                        t *
                        Mathf.PI);

                target.localScale =
                    Vector3.LerpUnclamped(
                        baseScale,
                        peakScale,
                        eased);

                yield return null;
            }

            target.localScale =
                baseScale;

            modeAnimation = null;
        }

        private void StopAllAnimations()
        {
            if (valueAnimation != null)
            {
                StopCoroutine(
                    valueAnimation);

                valueAnimation = null;
            }

            if (powerAnimation != null)
            {
                StopCoroutine(
                    powerAnimation);

                powerAnimation = null;
            }

            if (modeAnimation != null)
            {
                StopCoroutine(
                    modeAnimation);

                modeAnimation = null;
            }
        }

        private void OnDisable()
        {
            StopAllAnimations();
        }
    }
}