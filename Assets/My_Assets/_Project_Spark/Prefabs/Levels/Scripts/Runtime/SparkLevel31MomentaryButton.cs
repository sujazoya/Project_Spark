using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ProjectSpark.Gameplay
{
    [DisallowMultipleComponent]
    public sealed class SparkLevel31MomentaryButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public SparkLevel31SealIn controller;
        public bool isStop;
        public Image face;
        private bool pressed;
        private int pointerId;
        private Color idle;

        private void Awake() { if (face != null) idle = face.color; }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (pressed || eventData.button != PointerEventData.InputButton.Left) return;
            pointerId = eventData.pointerId;
            SetPressed(true);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (pressed && eventData.pointerId == pointerId) SetPressed(false);
        }

        private void OnDisable() { if (pressed) SetPressed(false); }
        private void OnApplicationFocus(bool focused) { if (!focused && pressed) SetPressed(false); }

        private void SetPressed(bool value)
        {
            pressed = value;
            if (face != null) face.color = value ? Color.Lerp(idle, Color.white, .35f) : idle;
            if (controller == null || !controller.isActiveAndEnabled) return;
            if (isStop) controller.SetStopPressed(value);
            else controller.SetStartPressed(value);
        }
    }
}
