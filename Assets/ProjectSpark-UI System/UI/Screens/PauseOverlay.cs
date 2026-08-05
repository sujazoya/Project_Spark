using ProjectSpark.UI.Core;
using UnityEngine;

namespace ProjectSpark.UI.Screens
{
    public sealed class PauseOverlay :
        MonoBehaviour
    {
        [SerializeField]
        private GameObject pauseRoot;

        private bool isOpen;

        public bool IsOpen =>
            isOpen;

        public void Open()
        {
            if (isOpen)
            {
                return;
            }

            isOpen = true;

            if (pauseRoot != null)
            {
                pauseRoot.SetActive(true);
            }

            Time.timeScale = 0f;

            UIManager.Instance.Input.SetState(
                UIInputState.UIOnly);
        }

        public void Close()
        {
            if (!isOpen)
            {
                return;
            }

            isOpen = false;

            if (pauseRoot != null)
            {
                pauseRoot.SetActive(false);
            }

            Time.timeScale = 1f;

            UIManager.Instance.Input.SetState(
                UIInputState.Gameplay);
        }

        public void Resume()
        {
            Close();
        }

        public void OpenSettings()
        {
            UIManager.Instance.ShowScreen(
                UIScreenIds.Settings);
        }

        public void ReturnToMainMenu()
        {
            Close();

            UIManager.Instance.ShowScreen(
                UIScreenIds.MainMenu);
        }
    }
}