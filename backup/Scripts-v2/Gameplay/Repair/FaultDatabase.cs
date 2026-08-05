using System.Collections.Generic;
using UnityEngine;

namespace ProjectSpark.Gameplay.Repair
{
    [CreateAssetMenu(
        menuName="Project Spark/Fault Database")]
    public sealed class FaultDatabase
        : ScriptableObject
    {
        public List<Fault> Faults =
            new();
    }
}
