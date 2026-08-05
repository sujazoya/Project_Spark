using UnityEngine;

namespace ProjectSpark.Gameplay.Objectives
{
    [CreateAssetMenu(
        menuName = "Project Spark/Objectives/Power Component")]
    public sealed class PowerComponentObjective : Objective
    {
        [SerializeField]
        private string componentId;

        public override ObjectiveResult Evaluate(
            ObjectiveContext context)
        {
            // TODO:
            // Query simulation for powered state.
            bool powered = false;

            if (powered)
            {
                Complete();
            }

            return new ObjectiveResult(
                Status == ObjectiveStatus.Completed,
                powered ? 1f : 0f);
        }
    }
}