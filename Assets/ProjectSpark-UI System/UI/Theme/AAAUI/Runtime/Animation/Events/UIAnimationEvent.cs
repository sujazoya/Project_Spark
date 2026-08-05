using System;
using UnityEngine;

namespace AAAUI
{
    public enum UIAnimationEventType
    {
        Marker,
        Start,
        Complete,
        ReverseComplete
    }

    [Serializable]
    public sealed class UIAnimationEvent
    {
        [SerializeField, Min(0f)]
        private float time;

        [SerializeField]
        private UIAnimationEventType type =
            UIAnimationEventType.Marker;

        [SerializeField]
        private string id = "Marker";

        public float Time => time;
        public UIAnimationEventType Type => type;
        public string Id => id;

        public void SetTime(float value)
        {
            time = Mathf.Max(0f, value);
        }

        public void SetType(UIAnimationEventType value)
        {
            type = value;
        }

        public void SetId(string value)
        {
            id = value ?? string.Empty;
        }
    }
}