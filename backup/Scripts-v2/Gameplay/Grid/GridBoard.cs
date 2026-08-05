using UnityEngine;

namespace ProjectSpark.Gameplay.Grid
{
    public sealed class GridBoard
    {
        private readonly GridCell[,] _cells;

        public int Width { get; }

        public int Height { get; }

        public float CellSize { get; }

        public GridBoard(BoardDefinition definition)
        {
            Width = definition.Width;
            Height = definition.Height;
            CellSize = definition.CellSize;

            _cells = new GridCell[Width, Height];

            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    _cells[x, y] =
                        new GridCell(new GridCoordinate(x, y));
                }
            }
        }

        public bool IsValid(GridCoordinate c)
        {
            return c.X >= 0 &&
                   c.Y >= 0 &&
                   c.X < Width &&
                   c.Y < Height;
        }

        public GridCell Get(GridCoordinate c)
        {
            if (!IsValid(c))
                return null;

            return _cells[c.X, c.Y];
        }

        public Vector3 GridToWorld(GridCoordinate c)
        {
            return new Vector3(
                c.X * CellSize,
                0,
                c.Y * CellSize);
        }
    }
}
