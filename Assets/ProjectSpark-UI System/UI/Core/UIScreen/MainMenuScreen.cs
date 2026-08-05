using ProjectSpark.UI.Core;
using UnityEngine;

namespace ProjectSpark.UI.Screens
{
    public sealed class MainMenuScreen : UIScreen
    {
        public void Continue()
        {
            // Actual save/load integration
            // will be connected in Phase 9.
            Debug.Log(
                "Continue requested.");
        }

        public void NewGame()
        {
            UIManager.Instance.ShowScreen(
                UIScreenIds.LevelSelect);
        }

        public void OpenLevelSelect()
        {
            UIManager.Instance.ShowScreen(
                UIScreenIds.LevelSelect);
        }

        public void OpenSettings()
        {
            UIManager.Instance.ShowScreen(
                UIScreenIds.Settings);
        }

        public void ExitGame()
        {
            UIManager.Instance.OpenModal(
                new UIModalRequest
                {
                    Type =
                        UIModalType.Confirmation,

                    Title =
                        "EXIT PROJECT SPARK?",

                    Message =
                        "Are you sure you want to exit?",

                    PrimaryText =
                        "EXIT",

                    SecondaryText =
                        "CANCEL",

                    ShowSecondaryButton =
                        true,

                    PrimaryAction =
                        ConfirmExit
                });
        }

        private void ConfirmExit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying =
                false;
#else
            Application.Quit();
#endif
        }
    }
}