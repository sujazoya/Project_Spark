
using UnityEngine;
using TMPro;

namespace ProjectSpark.Gameplay
{
    [DisallowMultipleComponent]
    public sealed class SparkVoltagePopup : MonoBehaviour
    {
        [Header("Text")]
        [SerializeField]
        private TMP_Text voltageText;

        [Header("Animation")]
        [SerializeField, Min(0.05f)]
        private float duration = 0.9f;

        [SerializeField, Min(0f)]
        private float floatDistance = 0.35f;

        [SerializeField, Min(1f)]
        private float overshootScale = 1.35f;

        [SerializeField]
        private AnimationCurve scaleCurve =
            new AnimationCurve(
                new Keyframe(0f, 0.3f),
                new Keyframe(0.15f, 1.35f),
                new Keyframe(0.35f, 1f),
                new Keyframe(1f, 0.9f));

        [Header("Fade")]
        [SerializeField, Range(0f, 1f)]
        private float fadeStart = 0.5f;

        [Header("Camera")]
        [SerializeField]
        private bool faceCamera = true;

        [SerializeField]
        private Camera targetCamera;

        [Header("Timing")]
        [SerializeField]
        private bool useUnscaledTime = true;

        [Header("Lifetime")]
        [SerializeField]
        private bool destroyWhenFinished = true;

        private Vector3 startPosition;

        private Vector3 baseScale;

        private float elapsed;

        private bool playing;

        private Color baseColor;

        private void Awake()
        {
            if (voltageText == null)
            {
                voltageText =
                    GetComponentInChildren<TMP_Text>(true);
            }

            baseScale = transform.localScale;
        }

        public void Play(
            float oldVoltage,
            float newVoltage,
            Color color,
            SparkTerminalIndicator.PopupDisplayMode displayMode,
            Vector3 worldPosition)
        {
            if (voltageText == null)
                return;

            transform.position = worldPosition;

            startPosition = worldPosition;

            elapsed = 0f;

            playing = true;

            if (targetCamera == null)
                targetCamera = Camera.main;

            float difference =
                newVoltage - oldVoltage;

            string voltageString =
                newVoltage.ToString("+0.00;-0.00;0.00") +
                " V";

            string deltaString =
                difference.ToString("+0.00;-0.00;0.00") +
                " V";

            switch (displayMode)
            {
                case SparkTerminalIndicator.PopupDisplayMode.VoltageChange:

                    voltageText.text =
                        deltaString;

                    break;

                case SparkTerminalIndicator.PopupDisplayMode.Both:

                    voltageText.text =
                        voltageString +
                        "\n" +
                        "<size=65%>" +
                        deltaString +
                        "</size>";

                    break;

                default:

                    voltageText.text =
                        voltageString;

                    break;
            }

            baseColor = color;
            baseColor.a = 1f;

            voltageText.color = baseColor;

            transform.localScale =
                baseScale * 0.3f;
        }

        private void LateUpdate()
        {
            if (!playing)
                return;

            float deltaTime = useUnscaledTime
                ? Time.unscaledDeltaTime
                : Time.deltaTime;

            elapsed += deltaTime;

            float t = Mathf.Clamp01(
                elapsed / duration);

            float eased =
                1f - Mathf.Pow(1f - t, 3f);

            transform.position =
                startPosition +
                Vector3.up *
                (floatDistance * eased);

            float scale =
                scaleCurve != null &&
                scaleCurve.length > 0
                    ? scaleCurve.Evaluate(t)
                    : 1f;

            transform.localScale =
                baseScale * scale;

            float fadeT =
                Mathf.InverseLerp(
                    fadeStart,
                    1f,
                    t);

            Color currentColor = baseColor;

            currentColor.a =
                1f -
                Mathf.SmoothStep(
                    0f,
                    1f,
                    fadeT);

            voltageText.color =
                currentColor;

            if (faceCamera)
            {
                if (targetCamera == null)
                    targetCamera = Camera.main;

                if (targetCamera != null)
                {
                    transform.rotation =
                        targetCamera.transform.rotation;
                }
            }

            if (t >= 1f)
            {
                playing = false;

                if (destroyWhenFinished)
                {
                    Destroy(gameObject);
                }
                else
                {
                    gameObject.SetActive(false);
                }
            }
        }
    }
}
