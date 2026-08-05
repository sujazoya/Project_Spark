using System;
using UnityEngine;

namespace AAAUI
{
    [Serializable]
    public sealed class GlitchTrack : UIAnimationTrack
    {
        [SerializeField]
        private string property = "_GlitchIntensity";

        [SerializeField]
        private float from;

        [SerializeField]
        private float to = 1f;

        [NonSerialized]
        private int propertyId = -1;

        public string Property => property;

        public float From => from;

        public float To => to;

        public override string DisplayName => "Glitch";

        public int PropertyId
        {
            get
            {
                if (propertyId < 0)
                    propertyId = Shader.PropertyToID(property);

                return propertyId;
            }
        }
    }
}