using System;
using UnityEngine;

namespace ProjectSpark.UI.Theme
{
    [Serializable]
    public struct UIButtonStyle
    {
        public Color normal;

        public Color hover;

        public Color pressed;

        public Color selected;

        public Color disabled;

        public Color textNormal;

        public Color textDisabled;

        public float height;

        public float radius;

        public float borderWidth;
    }
}