using System.Collections.Generic;
using UnityEngine;

namespace ProjectSpark.Gameplay.Tutorials
{
    [CreateAssetMenu(
        menuName="Project Spark/Tutorial/Sequence")]
    public sealed class TutorialSequence
        : ScriptableObject
    {
        public List<TutorialStep> Steps =
            new();
    }
}
