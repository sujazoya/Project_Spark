using System;
using UnityEngine;

namespace AAAUI
{
    [Serializable]
    public sealed class SlideTrack : UIAnimationTrack
    {
        [SerializeField] private Vector3 from = new Vector3(-32f, 0f, 0f);
        [SerializeField] private Vector3 to = Vector3.zero;

        public Vector3 From => from;
        public Vector3 To => to;
        public override string DisplayName => "Slide";
    }
}