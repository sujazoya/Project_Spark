using System.Collections;
using ProjectSpark.UI.Core;
using UnityEngine;

namespace ProjectSpark.UI.Screens
{
    public sealed class SplashScreen : UIScreen
    {
        [Header("Splash Settings")]
        [SerializeField]
        [Min(0f)]
        private float displayDuration = 2f;

        [SerializeField]
        private string nextScreenId = UIScreenIds.MainMenu;

        private Coroutine transitionRoutine;

        protected override void OnOpen()
        {
            base.OnOpen();

            StopTransitionRoutine();
            transitionRoutine = StartCoroutine(ShowSplashRoutine());
        }

        protected override void OnClose()
        {
            StopTransitionRoutine();
            base.OnClose();
        }

        private IEnumerator ShowSplashRoutine()
        {
            yield return new WaitForSecondsRealtime(displayDuration);

            transitionRoutine = null;

            if (UIManager.Instance == null)
            {
                yield break;
            }

            if (string.IsNullOrWhiteSpace(nextScreenId))
            {
                Debug.LogWarning(
                    $"{nameof(SplashScreen)} on '{name}' has no next screen ID.",
                    this);
                yield break;
            }

            UIManager.Instance.ShowScreen(nextScreenId);
        }

        private void StopTransitionRoutine()
        {
            if (transitionRoutine == null)
            {
                return;
            }

            StopCoroutine(transitionRoutine);
            transitionRoutine = null;
        }
    }
}
