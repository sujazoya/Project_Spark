using UnityEngine;

namespace ProjectSpark.Gameplay.Objectives
{
    [CreateAssetMenu(
        menuName = "Project Spark/Objectives/Voltage")]
    public sealed class VoltageObjective : Objective
    {
        [SerializeField]
        private float targetVoltage;

        [SerializeField]
        private float tolerance = 0.1f;

        public override ObjectiveResult Evaluate(
            ObjectiveContext context)
        {
            float measured = 0f;

            bool success =
                Mathf.Abs(measured - targetVoltage) <= tolerance;

            if (success)
            {
                Complete();
            }

            return new ObjectiveResult(
                success,
                success ? 1f : 0f);
        }
    }
}