using System;
using UnityEngine;

namespace ProjectSpark.Gameplay.Tutorial
{
    [Serializable]
    public class TutorialStepDefinition
    {
        public TutorialStepType Type;

        [TextArea]
        public string Message;

        public string TargetId;

        public float Delay;
    }
}
