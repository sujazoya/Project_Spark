using System.Collections.Generic;

namespace ProjectSpark.Gameplay.Wiring
{
    public sealed class WireRouter
    {
        private readonly WirePathFinder
            _pathFinder = new();

        public List<WireGridNode> Route(
            WireGrid grid,
            WireGridNode start,
            WireGridNode end)
        {
            return _pathFinder.FindPath(
                grid,
                start,
                end);
        }
    }
}
