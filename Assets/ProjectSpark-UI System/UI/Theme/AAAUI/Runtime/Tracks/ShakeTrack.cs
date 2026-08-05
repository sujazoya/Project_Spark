using System;
using UnityEngine;

namespace AAAUI
{
    [Serializable]
    public sealed class ShakeTrack : UIAnimationTrack
    {
        [SerializeField] private Vector3 amplitude = new Vector3(2f, 2f, 0f);
        [SerializeField, Min(0f)] private float frequency = 20f;
        [SerializeField] private int seed = 17;

        public Vector3 Amplitude => amplitude;
        public float Frequency => frequency;
        public int Seed => seed;
        public override string DisplayName => "Shake";
    }
}