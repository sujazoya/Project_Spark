using UnityEngine;

namespace ProjectSpark.Gameplay.Grid
{
    public sealed class GridManager : MonoBehaviour
    {
        [SerializeField]
        private BoardDefinition boardDefinition;

        public GridBoard Board { get; private set; }

        private void Awake()
        {
            Board = new GridBoard(boardDefinition);
        }

        public GridCell GetCell(GridCoordinate coordinate)
        {
            return Board.Get(coordinate);
        }

        public Vector3 GridToWorld(GridCoordinate coordinate)
        {
            return Board.GridToWorld(coordinate);
        }
    }
}
