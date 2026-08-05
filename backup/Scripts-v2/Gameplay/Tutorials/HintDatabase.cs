using System.Collections.Generic;
using UnityEngine;

namespace ProjectSpark.Gameplay.Tutorials
{
    [CreateAssetMenu(
        menuName="Project Spark/Hints/Database")]
    public sealed class HintDatabase
        : ScriptableObject
    {
        public List<HintData> Hints =
            new();
    }
}
