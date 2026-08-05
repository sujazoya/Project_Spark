using UnityEngine;

namespace ProjectSpark.Gameplay.Objectives
{
    [CreateAssetMenu(
        menuName = "Project Spark/Objectives/Measure")]
    public sealed class MeasureObjective : Objective
    {
        [SerializeField]
        private float expected;

        [SerializeField]
        private float tolerance = 0.1f;

        public override ObjectiveResult Evaluate(
            ObjectiveContext context)
        {
            // TODO:
            // Replace this with the actual measured value
            // from the Multimeter/ToolManager.
            float measured = 0f;

            bool success =
                Mathf.Abs(measured - expected) <= tolerance;

            if (success)
            {
                Complete();
            }

            float progress = success ? 1f : 0f;

            return new ObjectiveResult(
                success,
                progress);
        }
    }
}