using System;
using UnityEngine;

namespace AAAUI
{
    [Serializable]
    public sealed class GlitchTrack : UIAnimationTrack
    {
        [Header("Glitch Amount")]
        [SerializeField] private float amountFrom = 0f;
        [SerializeField] private float amountTo = 0.01f;

        [Header("Glitch Speed")]
        [SerializeField] private float speedFrom = 0f;
        [SerializeField] private float speedTo = 1f;

        [Header("Band Scale")]
        [SerializeField] private float bandScaleFrom = 1f;
        [SerializeField] private float bandScaleTo = 10f;

        [Header("Frequency")]
        [SerializeField] private float frequencyFrom = 0f;
        [SerializeField] private float frequencyTo = 1f;

        [Header("Band Width")]
        [SerializeField] private float bandWidthFrom = 0.1f;
        [SerializeField] private float bandWidthTo = 0.5f;

        public float AmountFrom => amountFrom;
        public float AmountTo => amountTo;

        public float SpeedFrom => speedFrom;
        public float SpeedTo => speedTo;

        public float BandScaleFrom => bandScaleFrom;
        public float BandScaleTo => bandScaleTo;

        public float FrequencyFrom => frequencyFrom;
        public float FrequencyTo => frequencyTo;

        public float BandWidthFrom => bandWidthFrom;
        public float BandWidthTo => bandWidthTo;

        public override string DisplayName => "Glitch";
    }
}