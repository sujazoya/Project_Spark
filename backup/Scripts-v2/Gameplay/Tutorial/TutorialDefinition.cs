using System.Collections.Generic;
using UnityEngine;

namespace ProjectSpark.Gameplay.Tutorial
{
    [CreateAssetMenu(
        menuName="Project Spark/Tutorial/Tutorial")]
    public class TutorialDefinition : ScriptableObject
    {
        public List<TutorialStepDefinition> Steps =
            new();
    }
}
