using System;
using UnityEngine;

namespace ProjectSpark.UI.Theme
{
    [Serializable]
    public struct UIThemeColors
    {
        [Header("Foundation")]

        public Color background;

        public Color surface;

        public Color surfaceElevated;

        public Color surfaceInteractive;

        public Color border;

        public Color borderStrong;

        [Header("Text")]

        public Color textPrimary;

        public Color textSecondary;

        public Color textMuted;

        public Color textDisabled;

        [Header("Accent")]

        public Color accent;

        public Color accentMuted;

        public Color accentSoft;

        [Header("Status")]

        public Color success;

        public Color warning;

        public Color error;

        public Color info;

        [Header("Overlay")]

        public Color overlay;

        public Color modalOverlay;
    }
}