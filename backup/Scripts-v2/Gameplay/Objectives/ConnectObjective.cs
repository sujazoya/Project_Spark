using UnityEngine;

namespace ProjectSpark.Gameplay.Objectives
{
    [CreateAssetMenu(
        menuName = "Project Spark/Objectives/Connect")]
    public sealed class ConnectObjective : Objective
    {
        [SerializeField]
        private string componentA;

        [SerializeField]
        private string componentB;

        public override ObjectiveResult Evaluate(
            ObjectiveContext context)
        {
            bool connected = false;

            // TODO:
            // connected = context.Circuit.IsConnected(componentA, componentB);

            if (connected)
            {
                Complete();
            }

            return new ObjectiveResult(
                Status == ObjectiveStatus.Completed,
                connected ? 1f : 0f);
        }
    }
}