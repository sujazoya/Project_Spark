namespace ProjectSpark.Gameplay
{
    public sealed class SparkInteractionSession
    {
        public SparkInteractionType InteractionType { get; }
        public SparkToolType ToolType { get; }
        public SparkElectronicObject Target { get; }
        public SparkTargetHit InitialHit { get; }
        public SparkTargetHit LastHit { get; private set; }
        public bool IsActive { get; private set; }

        public SparkInteractionSession(SparkInteractionType interactionType, SparkToolType toolType,
            SparkElectronicObject target, in SparkTargetHit hit)
        {
            InteractionType = interactionType; ToolType = toolType; Target = target;
            InitialHit = hit; LastHit = hit;
        }

        public void Start() => IsActive = true;
        public void Stop() => IsActive = false;
        public void UpdateHit(in SparkTargetHit hit)
        {
            if (hit.Target == Target) LastHit = hit;
        }
    }
}
