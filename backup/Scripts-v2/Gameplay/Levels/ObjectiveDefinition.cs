using UnityEngine;

namespace ProjectSpark.Gameplay.Levels
{
    [CreateAssetMenu(
        menuName="Project Spark/Levels/Objective")]
    public class ObjectiveDefinition : ScriptableObject
    {
        public string Id;

        public string Title;

        [TextArea]
        public string Description;

        public bool Required = true;
    }
}
