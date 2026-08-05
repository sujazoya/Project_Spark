using UnityEngine;

namespace ProjectSpark.Domain.Simulation.Solver
{
    [CreateAssetMenu(menuName="Project Spark/Solver Settings")]
    public sealed class SolverSettings
        : ScriptableObject
    {
        public int MaxIterations = 100;

        public double Tolerance = 1e-9;

        public bool UsePivoting = true;
    }
}
