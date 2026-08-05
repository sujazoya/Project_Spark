using ProjectSpark.Gameplay.Electronics;

namespace ProjectSpark.Gameplay.Grid
{
    public sealed class GridCell
    {
        public GridCoordinate Coordinate { get; }

        public ElectronicComponent Occupant { get; private set; }

        public bool IsOccupied => Occupant != null;

        public GridCell(GridCoordinate coordinate)
        {
            Coordinate = coordinate;
        }

        public bool Place(ElectronicComponent component)
        {
            if (IsOccupied)
                return false;

            Occupant = component;
            return true;
        }

        public void Clear()
        {
            Occupant = null;
        }
    }
}
