using System;
using UnityEngine;

namespace AAAUI
{
    [Serializable]
    public sealed class FlashTrack : UIAnimationTrack
    {
        [SerializeField] private Color from = Color.clear;
        [SerializeField] private Color to = Color.white;

        public Color From => from;
        public Color To => to;
        public override string DisplayName => "Flash";
    }
}