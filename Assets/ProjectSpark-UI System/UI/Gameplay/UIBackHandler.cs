using UnityEngine;

namespace ProjectSpark.UI.Input
{
    public sealed class UIBackHandler :
        MonoBehaviour
    {
        [SerializeField]
        private Navigation.ScreenManager
            screenManager;

        [SerializeField]
        private Navigation.PopupManager
            popupManager;

        [SerializeField]
        private Navigation.ModalManager
            modalManager;

        public void HandleBack()
        {
            if (modalManager != null &&
                modalManager.IsOpen)
            {
                modalManager.CloseCurrent();

                return;
            }

            if (popupManager != null)
            {
                popupManager.CloseCurrent();

                return;
            }

            if (screenManager != null)
            {
                screenManager.CloseCurrent();
            }
        }
    }
}