using System;
using UnityEngine;

namespace AAAUI
{
    [Serializable]
    public sealed class GlowTrack : UIAnimationTrack
    {
        [Header("Glow Property")]
        [SerializeField]
        private string property = "_Glow";

        [Header("Glow Value")]
        [SerializeField]
        private float from = 0f;

        [SerializeField]
        private float to = 1f;

        public string Property => property;

        public float From => from;

        public float To => to;

        public int PropertyId =>
            UIPropertyRegistry.GetId(property);

        public override string DisplayName =>
            "Glow";

        public void SetProperty(string value)
        {
            property = value ?? string.Empty;
        }

        public void SetValues(
            float start,
            float end)
        {
            from = start;
            to = end;
        }
    }
}