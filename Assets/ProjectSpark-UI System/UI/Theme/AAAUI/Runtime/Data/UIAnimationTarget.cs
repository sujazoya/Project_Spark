using System;
using UnityEngine;
using UnityEngine.UI;

namespace AAAUI
{
    [Serializable]
    public sealed class UIAnimationTarget
    {
        [SerializeField] private Transform transform;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Graphic graphic;
        [SerializeField] private Renderer renderer;

        public Transform Transform => transform;
        public CanvasGroup CanvasGroup => canvasGroup;
        public Graphic Graphic => ResolveGraphic();
        public Renderer Renderer => ResolveRenderer();

        public bool IsAssigned =>
            transform != null ||
            canvasGroup != null ||
            graphic != null ||
            renderer != null;

        public void Assign(Transform value)
        {
            transform = value;

            if (value == null)
            {
                canvasGroup = null;
                graphic = null;
                renderer = null;
                return;
            }

            canvasGroup =
                value.GetComponent<CanvasGroup>();

            graphic =
                value.GetComponent<Graphic>();

            renderer =
                value.GetComponent<Renderer>();
        }

        private Graphic ResolveGraphic()
        {
            if (graphic != null)
                return graphic;

            if (transform != null)
            {
                graphic =
                    transform.GetComponent<Graphic>();
            }

            return graphic;
        }

        private Renderer ResolveRenderer()
        {
            if (renderer != null)
                return renderer;

            if (transform != null)
            {
                renderer =
                    transform.GetComponent<Renderer>();
            }

            return renderer;
        }
    }
}