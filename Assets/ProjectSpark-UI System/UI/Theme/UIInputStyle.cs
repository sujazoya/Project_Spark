using System;
using UnityEngine;

namespace ProjectSpark.UI.Theme
{
    [Serializable]
    public struct UIInputStyle
    {
        [Header("Background")]
        public Color normalBackground;
        public Color focusedBackground;
        public Color disabledBackground;
        public Color errorBackground;

        [Header("Border")]
        public Color normalBorder;
        public Color focusedBorder;
        public Color errorBorder;

        [Header("Text")]
        public Color textColor;
        public Color placeholderColor;
        public Color disabledTextColor;
        public Color errorTextColor;

        [Header("Layout")]
        public float borderWidth;
        public float cornerRadius;
    }
}