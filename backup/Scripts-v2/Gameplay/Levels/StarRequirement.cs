using System;
using UnityEngine;

namespace ProjectSpark.Gameplay.Levels
{
    [Serializable]
    public class StarRequirement
    {
        [TextArea]
        public string Description;

        public int TargetValue;
    }
}
