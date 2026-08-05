using System;

namespace ProjectSpark.Gameplay.Grid
{
    [Serializable]
    public readonly struct GridCoordinate : IEquatable<GridCoordinate>
    {
        public readonly int X;
        public readonly int Y;

        public GridCoordinate(int x, int y)
        {
            X = x;
            Y = y;
        }

        public bool Equals(GridCoordinate other)
        {
            return X == other.X && Y == other.Y;
        }

        public override bool Equals(object obj)
        {
            return obj is GridCoordinate other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y);
        }

        public override string ToString()
        {
            return $"({X}, {Y})";
        }

        public static bool operator ==(GridCoordinate a, GridCoordinate b) => a.Equals(b);
        public static bool operator !=(GridCoordinate a, GridCoordinate b) => !a.Equals(b);
    }
}
