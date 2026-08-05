using UnityEngine;
using ProjectSpark.UI.Context;
using ProjectSpark.UI.Navigation;

namespace ProjectSpark.UI.Core
{
    public sealed class UIBootstrap :
        MonoBehaviour
    {
        [SerializeField]
        private ScreenManager screenManager;

        [SerializeField]
        private ScreenBase initialScreen;

        private void Awake()
        {
            RegisterScreens();
        }

        private void Start()
        {
            OpenInitialScreen();
        }

        private void RegisterScreens()
        {
            UIScreen[] screens =
                GetComponentsInChildren<UIScreen>(true);

            foreach (UIScreen screen in screens)
            {
                screenManager.Register(screen);
            }
        }

        private void OpenInitialScreen()
        {
            if (initialScreen == null)
            {
                return;
            }

            screenManager.Show(
                initialScreen.ScreenId
                );
        }
    }
}