using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.VFX
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(CanvasGroup))]
    public sealed class SparkScanOverlay : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private RectTransform monitorParent;
        [SerializeField] private Image overlayImage;

        [Header("Settings")]
        [SerializeField] private float scanWidth = 80f;

        [SerializeField]
        private float scanDuration = 4f;     // Always finishes in 4 seconds

        [SerializeField]
        private bool loop = false;

        [SerializeField]
        private bool fadeWithProgress = true;

        [Range(0f, 1f)]
        [SerializeField]
        private float maxAlpha = 0.35f;

        private RectTransform rect;
        private CanvasGroup canvasGroup;

        private float progress;
        private bool startScan;

        private void Awake()
        {
            rect = GetComponent<RectTransform>();
            canvasGroup = GetComponent<CanvasGroup>();

            if (overlayImage == null)
                overlayImage = GetComponent<Image>();

            SetWidth(scanWidth);
        }

        private void OnEnable()
        {
            RestartScan();
        }

        private void OnDisable()
        {
            progress = 0f;
            startScan = false;
            SetProgress(0f);
        }

        private void Update()
        {
            if (!startScan)
                return;

            progress += Time.deltaTime / scanDuration;

            if (progress >= 1f)
            {
                if (loop)
                {
                    progress = 0f;
                }
                else
                {
                    progress = 1f;
                    startScan = false;
                }
            }

            SetProgress(progress);
        }

        public void RestartScan()
        {
            progress = 0f;
            startScan = true;
            SetProgress(0f);
        }

        public void Scan(bool play)
        {
            if (play)
            {
                RestartScan();
            }
            else
            {
                startScan = false;
            }
        }

        public void SetProgress(float progress)
        {
            if (monitorParent == null)
                return;

            progress = Mathf.Clamp01(progress);

            float width = monitorParent.rect.width;

            // Travel completely across the monitor
            float left = -(width + scanWidth) * 0.01f;
            float right = (width + scanWidth) * 0.25f;

            Vector2 pos = rect.anchoredPosition;
            pos.x = Mathf.Lerp(left, right, progress);
            rect.anchoredPosition = pos;

            if (fadeWithProgress)
                canvasGroup.alpha = maxAlpha;
        }

        public void SetWidth(float width)
        {
            scanWidth = Mathf.Max(4f, width);

            Vector2 size = rect.sizeDelta;
            size.x = scanWidth;
            rect.sizeDelta = size;
        }

        public void SetColor(Color color)
        {
            if (overlayImage != null)
                overlayImage.color = color;
        }

        public void Show()
        {
            canvasGroup.alpha = maxAlpha;
        }

        public void Hide()
        {
            canvasGroup.alpha = 0f;
        }

        public void SetAlpha(float alpha)
        {
            canvasGroup.alpha = Mathf.Clamp01(alpha);
        }

        public void SetMaxAlpha(float alpha)
        {
            maxAlpha = Mathf.Clamp01(alpha);
        }
    }
}