using System.Collections.Generic;

namespace ProjectSpark.Gameplay.Scenarios
{
    public class ObjectiveManager
    {
        private readonly List<Objective>
            objectives =
                new();

        public void Initialize(
            List<Objective> list)
        {
            objectives.Clear();

            objectives.AddRange(list);
        }

        public void Complete(
            string id)
        {
            foreach(var obj in objectives)
            {
                if(obj.Id!=id)
                    continue;

                obj.State =
                    ObjectiveState.Completed;
            }
        }
    }
}
