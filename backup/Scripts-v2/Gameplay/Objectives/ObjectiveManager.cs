using System.Collections.Generic;
using UnityEngine;

namespace ProjectSpark.Gameplay.Objectives
{
    public sealed class ObjectiveManager
        : MonoBehaviour
    {
        [SerializeField]
        private List<Objective> objectives =
            new();

        private readonly ObjectiveContext
            context =
                new();

        public void StartObjectives()
        {
            foreach (Objective objective
                in objectives)
            {
                objective.StartObjective(
                    context);

                ObjectiveEvents.RaiseStarted(
                    objective);
            }
        }

        private void Update()
        {
            foreach (Objective objective
                in objectives)
            {
                if (objective.Status !=
                    ObjectiveStatus.Active)
                    continue;

                objective.Evaluate(context);
            }
        }
    }
}
