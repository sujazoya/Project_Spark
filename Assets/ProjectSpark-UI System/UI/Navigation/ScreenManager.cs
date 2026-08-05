using System.Collections.Generic;
using UnityEngine;

namespace ProjectSpark.UI.Navigation
{
    /// <summary>
    /// Manages full-screen UI navigation and screen history.
    /// </summary>
    public sealed class ScreenManager :
        MonoBehaviour
    {
        private readonly Dictionary<
            string,
            ScreenBase> screens =
            new();

        private readonly Stack<
            ScreenBase> history =
            new();

        private ScreenBase currentScreen;

        public ScreenBase CurrentScreen =>
            currentScreen;

        public bool HasHistory =>
            history.Count > 0;

        public void Register(
            ScreenBase screen)
        {
            if (screen == null)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(
                screen.ScreenId))
            {
                Debug.LogWarning(
                    "Cannot register screen " +
                    "without an ID.",
                    screen);

                return;
            }

            if (screens.ContainsKey(
                screen.ScreenId))
            {
                Debug.LogWarning(
                    $"Duplicate screen ID: " +
                    $"{screen.ScreenId}",
                    screen);

                return;
            }

            screens.Add(
                screen.ScreenId,
                screen);

            screen.Initialize();
        }

        public bool Open(
            string screenId)
        {
            if (!screens.TryGetValue(
                screenId,
                out ScreenBase nextScreen))
            {
                Debug.LogWarning(
                    $"Screen '{screenId}' " +
                    "was not registered.",
                    this);

                return false;
            }

            if (currentScreen == nextScreen)
            {
                return true;
            }

            if (currentScreen != null)
            {
                history.Push(
                    currentScreen);

                currentScreen.Close();
            }

            currentScreen =
                nextScreen;

            currentScreen.Open();

            return true;
        }

        public bool Back()
        {
            if (history.Count == 0)
            {
                return false;
            }

            ScreenBase previous =
                history.Pop();

            if (currentScreen != null)
            {
                currentScreen.Close();
            }

            currentScreen =
                previous;

            currentScreen.Open();

            return true;
        }

        public void CloseCurrent()
        {
            if (currentScreen == null)
            {
                return;
            }

            currentScreen.Close();

            currentScreen = null;
        }

        public void ClearHistory()
        {
            history.Clear();
        }

        public bool TryGet(
            string screenId,
            out ScreenBase screen)
        {
            return screens.TryGetValue(
                screenId,
                out screen);
        }
    }
}