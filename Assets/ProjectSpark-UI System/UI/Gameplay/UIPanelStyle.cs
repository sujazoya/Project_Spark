using System;
using UnityEngine;

namespace ProjectSpark.UI.Theme
{
    [Serializable]
    public struct UIPanelStyle
    {
        public Color background;

        public Color border;

        public float borderWidth;

        public float radius;

        public Vector2 padding;

        public Vector2 shadowOffset;

        public float shadowBlur;

        public float shadowOpacity;
    }
}