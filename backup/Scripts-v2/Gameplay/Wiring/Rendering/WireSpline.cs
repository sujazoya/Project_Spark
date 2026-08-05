using System.Collections.Generic;

namespace ProjectSpark.Gameplay.Wiring.Rendering
{
    public sealed class WireSpline
    {
        private readonly List<WireSplinePoint> _points = new();

        public IReadOnlyList<WireSplinePoint> Points => _points;

        public void Add(WireSplinePoint point)
        {
            _points.Add(point);
        }

        public void Clear()
        {
            _points.Clear();
        }
    }
}
