using System;
using UnityEngine;
using ProjectSpark.UI;

namespace ProjectSpark.UI.Theme
{
    [Serializable]
    public struct UIThemeComponents
    {
        public UIButtonStyle primaryButton;

        public UIButtonStyle secondaryButton;

        public UIButtonStyle ghostButton;

        public UIPanelStyle panel;

        public UIPanelStyle popup;

        public UIPanelStyle modal;

        public UIInputStyle input;
    }
}