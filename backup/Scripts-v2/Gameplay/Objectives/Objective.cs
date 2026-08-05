using UnityEngine;

namespace ProjectSpark.Gameplay.Objectives
{
    public abstract class Objective : ScriptableObject
    {
        [Header("Objective Info")]
        [SerializeField]
        private string objectiveName;

        [SerializeField]
        [TextArea]
        private string description;

        [SerializeField]
        private bool optional;

        public string ObjectiveName => objectiveName;
        public string Description => description;
        public bool Optional => optional;

        public ObjectiveStatus Status
        {
            get;
            protected set;
        } = ObjectiveStatus.Locked;

        public virtual void StartObjective(ObjectiveContext context)
        {
            Status = ObjectiveStatus.Active;
        }

        /// <summary>
        /// Called every frame (or when needed) to evaluate objective progress.
        /// </summary>
        public abstract ObjectiveResult Evaluate(ObjectiveContext context);

        protected void Complete()
        {
            if (Status == ObjectiveStatus.Completed)
                return;

            Status = ObjectiveStatus.Completed;
            ObjectiveEvents.RaiseCompleted(this);
        }

        protected void Fail()
        {
            if (Status == ObjectiveStatus.Failed)
                return;

            Status = ObjectiveStatus.Failed;
        }
    }
}