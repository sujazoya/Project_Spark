using System.Collections;
using ProjectSpark.UI.Core;
using UnityEngine;

namespace ProjectSpark.UI.Screens
{
    public sealed class SplashScreen : UIScreen
    {
        [SerializeField]
        private float displayDuration = 2f;

        [SerializeField]
        private string nextScreenId =
            UIScreenIds.MainMenu;

        private Coroutine transitionRoutine;

        protected override void OnOpen()
        {
            base.OnOpen();

            if (transitionRoutine != null)
            {
                StopCoroutine(
                    transitionRoutine);
            }

            transitionRoutine =
                StartCoroutine(
                    ShowSplashRoutine());
        }

        protected override void OnClose()
        {
            if (transitionRoutine != null)
            {
                StopCoroutine(
                    transitionRoutine);

                transitionRoutine = null;
            }

            base.OnClose();
        }

        private IEnumerator ShowSplashRoutine()
        {
            yield return new WaitForSecondsRealtime(
                displayDuration);

            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowScreen(
                    nextScreenId);
            }
        }
    }
}