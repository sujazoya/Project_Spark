using ProjectSpark.Domain.Simulation;
using ProjectSpark.Gameplay.Electronics;

namespace ProjectSpark.Gameplay.Objectives
{
    public sealed class ObjectiveContext
    {
        public SimulationContext Simulation;

        public ComponentManager Components;

        public float LevelTime;

        public int Mistakes;

        public int HintsUsed;
    }
}
