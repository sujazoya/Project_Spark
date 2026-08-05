using UnityEngine;
using UnityEngine.EventSystems;

namespace ProjectSpark.UI.Animation
{
    public sealed class UIButtonAnimation :
        MonoBehaviour,
        IPointerEnterHandler,
        IPointerExitHandler,
        IPointerDownHandler,
        IPointerUpHandler
    {
        [SerializeField]
        private RectTransform target;

        [SerializeField]
        private float hoverScale = 1.02f;

        [SerializeField]
        private float pressScale = 0.98f;

        [SerializeField]
        private float speed = 12f;

        private Vector3 baseScale;

        private float targetScale = 1f;

        private void Awake()
        {
            if (target == null)
            {
                target =
                    transform as RectTransform;
            }

            baseScale =
                target.localScale;
        }

        private void Update()
        {
            float scale =
                Mathf.Lerp(
                    target.localScale.x,
                    targetScale,
                    Time.unscaledDeltaTime *
                    speed);

            target.localScale =
                baseScale *
                scale;
        }

        public void OnPointerEnter(
            PointerEventData eventData)
        {
            targetScale =
                hoverScale;
        }

        public void OnPointerExit(
            PointerEventData eventData)
        {
            targetScale =
                1f;
        }

        public void OnPointerDown(
            PointerEventData eventData)
        {
            targetScale =
                pressScale;
        }

        public void OnPointerUp(
            PointerEventData eventData)
        {
            targetScale =
                hoverScale;
        }
    }
}