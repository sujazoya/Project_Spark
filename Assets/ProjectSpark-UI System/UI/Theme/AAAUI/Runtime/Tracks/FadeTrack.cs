using System;
using UnityEngine;

namespace AAAUI
{
    [Serializable]
    public sealed class FadeTrack : UIAnimationTrack
    {
        [SerializeField, Range(0f, 1f)] private float from = 0f;
        [SerializeField, Range(0f, 1f)] private float to = 1f;

        public float From => from;
        public float To => to;
        public override string DisplayName => "Fade";
    }
}