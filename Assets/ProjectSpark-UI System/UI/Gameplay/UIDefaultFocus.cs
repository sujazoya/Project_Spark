using UnityEngine;

namespace ProjectSpark.UI.Input
{
    public sealed class UIDefaultFocus :
        MonoBehaviour
    {
        [SerializeField]
        private GameObject defaultFocus;

        public void FocusDefault()
        {
            if (UIFocusManager.Instance ==
                null)
            {
                return;
            }

            UIFocusManager.Instance
                .Focus(
                    defaultFocus);
        }
    }
}