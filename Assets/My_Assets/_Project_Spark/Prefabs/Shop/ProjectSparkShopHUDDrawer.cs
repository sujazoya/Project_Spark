
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.UI
{
    [DisallowMultipleComponent]
    public sealed class ProjectSparkShopHUDDrawer : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private RectTransform drawer;
        [SerializeField] private Button toggleButton;
        [SerializeField] private TMP_Text toggleLabel;

        [Header("Animation")]
        [SerializeField] private bool startOpen = false;
        [SerializeField, Min(0.05f)]
        private float duration = 0.35f;
        [SerializeField] private float openRightOffset = 20f;

        private Coroutine animationRoutine;
        private bool isOpen;

        private float OpenX => -openRightOffset;
        private float ClosedX => drawer.rect.width;

        private void Awake()
        {
            if (drawer == null)
                drawer = transform as RectTransform;

            if (toggleButton == null)
                toggleButton = transform.Find("DrawerToggle")
                    ?.GetComponent<Button>();

            if (toggleLabel == null)
                toggleLabel = transform.Find("DrawerToggle/Arrow")
                    ?.GetComponent<TMP_Text>();

            if (toggleButton != null)
                toggleButton.onClick.AddListener(Toggle);
        }

        private void Start()
        {
            RectTransform tab = toggleButton != null
                ? toggleButton.GetComponent<RectTransform>()
                : null;

            if (tab != null)
            {
                tab.anchorMin = new Vector2(0f, .5f);
                tab.anchorMax = new Vector2(0f, .5f);
                tab.pivot = new Vector2(1f, .5f);
                tab.anchoredPosition = Vector2.zero;
            }

            SetOpenImmediate(startOpen);
        }

        private void OnDisable()
        {
            if (animationRoutine != null)
            {
                StopCoroutine(animationRoutine);
                animationRoutine = null;
            }
        }

        private void OnDestroy()
        {
            if (toggleButton != null)
                toggleButton.onClick.RemoveListener(Toggle);
        }

        public void Toggle()
        {
            SetOpen(!isOpen);
        }

        public void Open()
        {
            SetOpen(true);
        }

        public void Close()
        {
            SetOpen(false);
        }

        public void SetOpen(bool open)
        {
            if (drawer == null)
                return;

            isOpen = open;

            if (animationRoutine != null)
            {
                StopCoroutine(animationRoutine);
                animationRoutine = null;
            }

            UpdateToggleLabel();

            animationRoutine = StartCoroutine(
                AnimateDrawer(open ? OpenX : ClosedX));
        }

        private IEnumerator AnimateDrawer(float targetX)
        {
            float startX = drawer.anchoredPosition.x;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;

                float t = Mathf.Clamp01(elapsed / duration);
                t = t * t * (3f - 2f * t);

                SetPosition(Mathf.Lerp(startX, targetX, t));

                yield return null;
            }

            SetPosition(targetX);
            animationRoutine = null;
        }

        private void SetOpenImmediate(bool open)
        {
            if (drawer == null)
                return;

            isOpen = open;

            if (animationRoutine != null)
            {
                StopCoroutine(animationRoutine);
                animationRoutine = null;
            }

            SetPosition(open ? OpenX : ClosedX);
            UpdateToggleLabel();
        }

        private void SetPosition(float x)
        {
            Vector2 position = drawer.anchoredPosition;
            position.x = x;
            drawer.anchoredPosition = position;
        }

        private void UpdateToggleLabel()
        {
            if (toggleLabel != null)
                toggleLabel.text = isOpen ? ">" : "<";
        }
    }
}
