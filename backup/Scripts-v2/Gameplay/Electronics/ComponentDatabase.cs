using System.Collections.Generic;
using UnityEngine;

namespace ProjectSpark.Gameplay.Electronics
{
    [CreateAssetMenu(
        menuName="Project Spark/Electronics/Database")]
    public sealed class ComponentDatabase
        : ScriptableObject
    {
        [SerializeField]
        private List<ComponentDefinition> components;

        public IReadOnlyList<ComponentDefinition>
            Components => components;
    }
}
