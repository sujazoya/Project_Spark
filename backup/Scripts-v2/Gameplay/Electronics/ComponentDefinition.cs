using UnityEngine;

namespace ProjectSpark.Gameplay.Electronics
{
    [CreateAssetMenu(
        menuName="Project Spark/Electronics/Component")]
    public sealed class ComponentDefinition
        : ScriptableObject
    {
        public string Id;

        public string DisplayName;

        public ComponentType Type;

        public GameObject Prefab;

        public Sprite Icon;

        public float DefaultVoltage;

        public float DefaultResistance;

        public float MaxCurrent;
    }
}
