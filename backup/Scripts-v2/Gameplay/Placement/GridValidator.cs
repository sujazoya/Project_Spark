using UnityEngine;

namespace ProjectSpark.Gameplay.Placement
{
    public sealed class GridValidator
    {
        public bool IsInsideBoard(
            Vector2Int gridPosition,
            Vector2Int boardSize)
        {
            return
                gridPosition.x >= 0 &&
                gridPosition.y >= 0 &&
                gridPosition.x < boardSize.x &&
                gridPosition.y < boardSize.y;
        }
    }
}
