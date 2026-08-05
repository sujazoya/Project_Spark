using ProjectSpark.UI.Core;
using UnityEngine;

namespace ProjectSpark.UI.Screens
{
    public sealed class SettingsScreen :
        UIScreen
    {
        public void Back()
        {
            UIManager.Instance.ShowScreen(
                UIScreenIds.MainMenu);
        }

        public void OpenControls()
        {
            Debug.Log(
                "Controls requested.");
        }

        public void ApplySettings()
        {
            Debug.Log(
                "Settings apply requested.");
        }
    }
}