using System;
using UnityEngine;

namespace ProjectSpark.UI
{
    [DisallowMultipleComponent]
    public sealed class PanelAnimationController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Animator animator;

        [Header("Animation States")]
        [SerializeField] private string panelInState = "Panel In";
        [SerializeField] private string panelOutState = "Panel Out";

        [Header("Behaviour")]
        [SerializeField] private int layer = 0;
        [SerializeField] private bool playInOnEnable = false;
        [SerializeField] private bool hideGameObjectAfterOut = false;

        public bool IsOpen { get; private set; }
        public bool IsPlaying { get; private set; }

        public event Action OpenStarted;
        public event Action OpenCompleted;
        public event Action CloseStarted;
        public event Action CloseCompleted;

        private int panelInHash;
        private int panelOutHash;

        private void Awake()
        {
            if (animator == null)
                animator = GetComponent<Animator>();

            if (animator == null)
            {
                Debug.LogError(
                    $"{nameof(PanelAnimationController)} requires an Animator.",
                    this);

                enabled = false;
                return;
            }

            panelInHash = Animator.StringToHash(panelInState);
            panelOutHash = Animator.StringToHash(panelOutState);
        }

        private void OnEnable()
        {
            if (playInOnEnable)
                PlayIn();
        }

        private void Update()
        {
            if (!IsPlaying)
                return;

            AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(layer);

            if (state.shortNameHash == panelInHash &&
                state.normalizedTime >= 1f)
            {
                IsPlaying = false;
                IsOpen = true;

                OpenCompleted?.Invoke();
            }
            else if (state.shortNameHash == panelOutHash &&
                     state.normalizedTime >= 1f)
            {
                IsPlaying = false;
                IsOpen = false;

                CloseCompleted?.Invoke();

                if (hideGameObjectAfterOut)
                    gameObject.SetActive(false);
            }
        }

        // ---------------------------------------------------------
        // OPEN
        // ---------------------------------------------------------

        public void PlayIn()
        {
            if (!enabled)
                return;

            gameObject.SetActive(true);

            IsPlaying = true;
            IsOpen = false;

            animator.Play(
                panelInHash,
                layer,
                0f);

            animator.Update(0f);

            OpenStarted?.Invoke();
        }

        // ---------------------------------------------------------
        // CLOSE
        // ---------------------------------------------------------

        public void PlayOut()
        {
            if (!enabled)
                return;

            IsPlaying = true;

            animator.Play(
                panelOutHash,
                layer,
                0f);

            animator.Update(0f);

            CloseStarted?.Invoke();
        }

        // ---------------------------------------------------------
        // TOGGLE
        // ---------------------------------------------------------

        public void Toggle()
        {
            if (IsOpen)
                PlayOut();
            else
                PlayIn();
        }

        // ---------------------------------------------------------
        // FORCE STATE
        // ---------------------------------------------------------

        public void ForceOpen()
        {
            gameObject.SetActive(true);

            animator.Play(
                panelInHash,
                layer,
                1f);

            animator.Update(0f);

            IsOpen = true;
            IsPlaying = false;
        }

        public void ForceClosed()
        {
            animator.Play(
                panelOutHash,
                layer,
                1f);

            animator.Update(0f);

            IsOpen = false;
            IsPlaying = false;
        }
    }
}