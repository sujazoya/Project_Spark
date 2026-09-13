using DG.Tweening;
using UnityEngine;

namespace ProjectSpark.UI.Animation
{
    public sealed class UIModalAnimator :
        MonoBehaviour
    {
        [SerializeField]
        private RectTransform panel;

        [SerializeField]
        private CanvasGroup canvasGroup;

        [SerializeField]
        private RectTransform background;

        [Header("Timing")]

        [SerializeField]
        private float duration = 0.28f;

        private Sequence sequence;

        private Vector3 shownScale;

        public bool IsOpen { get; private set; }

        private void Awake()
        {
            if (panel == null)
                panel =
                    transform as RectTransform;

            if (canvasGroup == null)
                canvasGroup =
                    GetComponent<CanvasGroup>();

            if (canvasGroup == null)
                canvasGroup =
                    gameObject.AddComponent<CanvasGroup>();

            shownScale =
                panel.localScale;
        }

        public void Open()
        {
            sequence?.Kill();

            gameObject.SetActive(true);

            panel.localScale =
                Vector3.one * 0.90f;

            canvasGroup.alpha = 0f;

            sequence =
                DOTween.Sequence()
                .SetTarget(this)
                .SetUpdate(true);

            sequence.Join(
                panel.DOScale(
                    shownScale,
                    duration)
                .SetEase(Ease.OutBack));

            sequence.Join(
                canvasGroup.DOFade(
                    1f,
                    duration * 0.75f)
                .SetEase(Ease.OutQuad));

            sequence.OnComplete(() =>
            {
                IsOpen = true;

                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
            });
        }

        public void Close()
        {
            sequence?.Kill();

            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            sequence =
                DOTween.Sequence()
                .SetTarget(this)
                .SetUpdate(true);

            sequence.Join(
                panel.DOScale(
                    Vector3.one * 0.94f,
                    duration * 0.65f)
                .SetEase(Ease.InCubic));

            sequence.Join(
                canvasGroup.DOFade(
                    0f,
                    duration * 0.55f)
                .SetEase(Ease.InQuad));

            sequence.OnComplete(() =>
            {
                IsOpen = false;
                gameObject.SetActive(false);
            });
        }
    }
}