using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.Scanner
{
    [DisallowMultipleComponent]
    public sealed class ScannerStateTransition
        : MonoBehaviour
    {
        private static readonly int
            TransitionProgressID =
            Shader.PropertyToID(
                "_TransitionProgress");

        private static readonly int
            TransitionActiveID =
            Shader.PropertyToID(
                "_TransitionActive");

        private static readonly int
            TransitionDirectionID =
            Shader.PropertyToID(
                "_TransitionDirection");

        [SerializeField]
        private Graphic transitionGraphic;

        [SerializeField]
        private float duration = 0.35f;

        private Material transitionMaterial;

        private float progress;
        private float startTime;

        private bool running;

        private void Awake()
        {
            if (transitionGraphic == null)
                transitionGraphic =
                    GetComponent<Graphic>();

            if (transitionGraphic == null)
                return;

            transitionMaterial =
                transitionGraphic.material;

            DisableImmediate();
        }

        private void Update()
        {
            if (!running)
                return;

            if (duration <= 0f)
            {
                progress = 1f;
            }
            else
            {
                progress =
                    Mathf.Clamp01(
                        (Time.unscaledTime -
                         startTime) /
                        duration);
            }

            ApplyProgress(progress);

            if (progress >= 1f)
                running = false;
        }

        public void PlayForward()
        {
            Play(1f);
        }

        public void PlayReverse()
        {
            Play(-1f);
        }

        public void Play(
            float direction)
        {
            if (transitionGraphic == null)
                return;

            if (transitionMaterial == null)
                transitionMaterial =
                    transitionGraphic.material;

            direction =
                direction >= 0f
                    ? 1f
                    : -1f;

            progress = 0f;
            startTime =
                Time.unscaledTime;

            running = true;

            transitionGraphic.enabled = true;

            transitionMaterial.SetFloat(
                TransitionDirectionID,
                direction);

            transitionMaterial.SetFloat(
                TransitionActiveID,
                1f);

            ApplyProgress(0f);
        }

        public void SetProgress(
            float value)
        {
            if (transitionGraphic == null)
                return;

            progress =
                Mathf.Clamp01(value);

            transitionGraphic.enabled =
                progress > 0f;

            ApplyProgress(progress);
        }

        public void Stop()
        {
            running = false;

            DisableImmediate();
        }

        public void DisableImmediate()
        {
            running = false;
            progress = 0f;

            if (transitionMaterial != null)
            {
                transitionMaterial.SetFloat(
                    TransitionProgressID,
                    0f);

                transitionMaterial.SetFloat(
                    TransitionActiveID,
                    0f);
            }

            if (transitionGraphic != null)
                transitionGraphic.enabled = false;
        }

        private void ApplyProgress(
            float value)
        {
            if (transitionMaterial == null)
                return;

            transitionMaterial.SetFloat(
                TransitionProgressID,
                value);

            transitionMaterial.SetFloat(
                TransitionActiveID,
                1f);
        }
    }
}