using System;
using UnityEngine;

namespace AAAUI
{
    [Serializable]
    public sealed class ScaleTrack : UIAnimationTrack
    {
        [SerializeField] private Vector3 from = new Vector3(0.98f, 0.98f, 0.98f);
        [SerializeField] private Vector3 to = Vector3.one;

        public Vector3 From => from;
        public Vector3 To => to;
        public override string DisplayName => "Scale";
    }
}