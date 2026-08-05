using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.UI.Theme
{
    public sealed class UIThemedButton :
        UIThemeElement
    {
        [SerializeField]
        private Button button;

        [SerializeField]
        private Image background;

        [SerializeField]
        private UIButtonStyle style;

        protected override void ApplyTheme()
        {
            if (Theme == null)
            {
                return;
            }

            style =
                Theme.components.primaryButton;

            if (background != null)
            {
                background.color =
                    style.normal;
            }
        }
    }
}