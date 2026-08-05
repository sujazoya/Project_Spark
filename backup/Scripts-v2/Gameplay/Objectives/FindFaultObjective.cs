using UnityEngine;

namespace ProjectSpark.Gameplay.Objectives
{
    [CreateAssetMenu(
        menuName = "Project Spark/Objectives/Find Fault")]
    public sealed class FindFaultObjective : Objective
    {
        public override ObjectiveResult Evaluate(
            ObjectiveContext context)
        {
            // TODO:
            // Replace with your diagnostics system.
            bool faultFound = false;

            if (faultFound)
            {
                Complete();
            }

            return new ObjectiveResult(
                Status == ObjectiveStatus.Completed,
                faultFound ? 1f : 0f);
        }
    }
}