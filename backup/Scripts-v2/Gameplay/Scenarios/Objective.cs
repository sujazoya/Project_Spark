using System;

namespace ProjectSpark.Gameplay.Scenarios
{
    [Serializable]
    public class Objective
    {
        public string Id;

        public string Title;

        public string Description;

        public ObjectiveState State;
    }
}
