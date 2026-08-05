using System;
using UnityEngine;

namespace AAAUI
{
    [Serializable]
    public sealed class MaterialColorTrack : UIAnimationTrack
    {
        [Header("Material Property")]
        [SerializeField]
        private string property = "_Color";

        [Header("Color")]
        [SerializeField]
        private Color from = Color.white;

        [SerializeField]
        private Color to = Color.white;

        public string Property => property;

        public Color From => from;

        public Color To => to;

        public int PropertyId =>
            UIPropertyRegistry.GetId(property);

        public override string DisplayName =>
            "Material Color";

        public void SetProperty(string value)
        {
            property = value ?? string.Empty;
        }

        public void SetValues(
            Color start,
            Color end)
        {
            from = start;
            to = end;
        }
    }
}