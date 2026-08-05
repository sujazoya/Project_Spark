using UnityEngine;

namespace ProjectSpark.Gameplay.Objectives
{
    [CreateAssetMenu(
        menuName = "Project Spark/Objectives/Repair")]
    public sealed class RepairObjective : Objective
    {
        public override ObjectiveResult Evaluate(
            ObjectiveContext context)
        {
            bool repaired = false;

            if (repaired)
            {
                Complete();
            }

            return new ObjectiveResult(
                Status == ObjectiveStatus.Completed,
                repaired ? 1f : 0f);
        }
    }
}