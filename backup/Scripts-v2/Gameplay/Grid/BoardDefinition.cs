using UnityEngine;

namespace ProjectSpark.Gameplay.Grid
{
    [CreateAssetMenu(
        menuName = "Project Spark/Grid/Board Definition",
        fileName = "BoardDefinition")]
    public class BoardDefinition : ScriptableObject
    {
        [Min(1)]
        public int Width = 8;

        [Min(1)]
        public int Height = 8;

        [Min(0.1f)]
        public float CellSize = 1f;

        public Vector3 Origin = Vector3.zero;
    }
}
