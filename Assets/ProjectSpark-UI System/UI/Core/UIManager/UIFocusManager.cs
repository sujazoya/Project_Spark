using UnityEngine;
using UnityEngine.EventSystems;

namespace ProjectSpark.UI.Input
{
    public sealed class UIFocusManager :
        MonoBehaviour
    {
        public static UIFocusManager Instance
        {
            get;
            private set;
        }

        private GameObject previousFocus;

        private void Awake()
        {
            if (Instance != null &&
                Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        public void Focus(
            GameObject target)
        {
            if (target == null)
            {
                return;
            }

            previousFocus =
                EventSystem.current
                    ?.currentSelectedGameObject;

            EventSystem.current
                ?.SetSelectedGameObject(
                    target);
        }

        public void RestorePreviousFocus()
        {
            if (previousFocus == null)
            {
                return;
            }

            EventSystem.current
                ?.SetSelectedGameObject(
                    previousFocus);
        }

        public void ClearFocus()
        {
            EventSystem.current
                ?.SetSelectedGameObject(
                    null);
        }
    }
}