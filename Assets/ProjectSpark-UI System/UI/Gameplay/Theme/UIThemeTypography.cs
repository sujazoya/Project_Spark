using System;
using TMPro;
using UnityEngine;

namespace ProjectSpark.UI.Theme
{
    [Serializable]
    public struct UIThemeTypography
    {
        public TMP_FontAsset primaryFont;

        public TMP_FontAsset secondaryFont;

        [Header("Sizes")]

        public float displaySize;

        public float headingSize;

        public float titleSize;

        public float bodySize;

        public float smallSize;

        public float captionSize;

        [Header("Spacing")]

        public float headingSpacing;

        public float bodySpacing;
    }
}