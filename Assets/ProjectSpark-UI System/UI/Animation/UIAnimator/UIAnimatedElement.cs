using UnityEngine;

namespace ProjectSpark.UI.Animation
{
    public sealed class UIAnimatedElement :
        MonoBehaviour
    {
        [SerializeField]
        private RectTransform target;

        [SerializeField]
        private CanvasGroup canvasGroup;

        [SerializeField]
        private UIAnimationService
            animationService;

        [SerializeField]
        private UIAnimationProfile
            openProfile;

        [SerializeField]
        private UIAnimationProfile
            closeProfile;

        private Coroutine currentAnimation;

        public bool IsOpen
        {
            get;
            private set;
        }

        private void Awake()
        {
            if (target == null)
            {
                target =
                    transform as RectTransform;
            }
        }

        public void Open()
        {
            gameObject.SetActive(true);

            Play(
                openProfile);

            IsOpen = true;
        }

        public void Close()
        {
            Play(
                closeProfile);

            IsOpen = false;
        }

        private void Play(
            UIAnimationProfile profile)
        {
            if (animationService == null ||
                profile == null)
            {
                return;
            }

            if (currentAnimation != null)
            {
                animationService.Stop(
                    currentAnimation);
            }

            currentAnimation =
                animationService.Play(
                    target,
                    canvasGroup,
                    profile);
        }
    }
}