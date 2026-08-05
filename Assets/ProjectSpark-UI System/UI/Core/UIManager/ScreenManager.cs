using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectSpark.UI.Core
{
    /// <summary>
    /// Manages full-screen UI navigation.
    /// </summary>
    public sealed class ScreenManager : MonoBehaviour
    {
        [SerializeField]
        private Transform screenRoot;

        private readonly Dictionary<string, UIScreen> screens =
            new Dictionary<string, UIScreen>();

        private UIScreen currentScreen;

        public UIScreen CurrentScreen => currentScreen;

        public event Action<UIScreen, UIScreen> ScreenChanged;

        public void Initialize()
        {
            screens.Clear();

            if (screenRoot == null)
            {
                Debug.LogError(
                    "ScreenManager requires a Screen Root.",
                    this);

                return;
            }

            UIScreen[] discoveredScreens =
                screenRoot.GetComponentsInChildren<UIScreen>(true);

            foreach (UIScreen screen in discoveredScreens)
            {
                Register(screen);
            }
        }

        public void Register(UIScreen screen)
        {
            if (screen == null)
                return;

            if (string.IsNullOrWhiteSpace(screen.ScreenId))
            {
                Debug.LogWarning(
                    $"UIScreen '{screen.name}' has no Screen ID.",
                    screen);

                return;
            }

            if (screens.ContainsKey(screen.ScreenId))
            {
                Debug.LogError(
                    $"Duplicate UI Screen ID: {screen.ScreenId}",
                    screen);

                return;
            }

            screens.Add(screen.ScreenId, screen);
        }

        public bool Show(string screenId)
        {
            if (!screens.TryGetValue(screenId, out UIScreen nextScreen))
            {
                Debug.LogError(
                    $"UI Screen '{screenId}' was not found.",
                    this);

                return false;
            }

            if (currentScreen == nextScreen)
                return true;

            UIScreen previousScreen = currentScreen;

            if (previousScreen != null)
            {
                previousScreen.Close();
            }

            currentScreen = nextScreen;
            currentScreen.Open();

            ScreenChanged?.Invoke(
                previousScreen,
                currentScreen);

            return true;
        }

        public bool HideCurrent()
        {
            if (currentScreen == null)
                return false;

            UIScreen previousScreen = currentScreen;

            currentScreen.Close();
            currentScreen = null;

            ScreenChanged?.Invoke(
                previousScreen,
                null);

            return true;
        }

        public bool TryGet(
            string screenId,
            out UIScreen screen)
        {
            return screens.TryGetValue(
                screenId,
                out screen);
        }
    }
}