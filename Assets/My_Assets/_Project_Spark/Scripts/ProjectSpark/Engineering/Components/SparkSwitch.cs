using UnityEngine;

namespace ProjectSpark.Gameplay
{
    public enum SparkSwitchState { Open, Closed }

    public sealed class SparkSwitch : SparkElectricalComponent
    {
        [SerializeField] private SparkSwitchState state = SparkSwitchState.Open;
        public SparkSwitchState State => state;
        public bool IsConducting => state == SparkSwitchState.Closed;

        public SparkResult Toggle(in SparkInteractionContext context)
        {
            if (!CanInteract(context, out var reason)) return SparkResult.Rejected(reason);
            state = state == SparkSwitchState.Open ? SparkSwitchState.Closed : SparkSwitchState.Open;
            NotifyElectricalConfigurationChanged();
            return SparkResult.Success();
        }

        public void SetState(SparkSwitchState newState)
        {
            if (state == newState) return;
            state = newState;
            NotifyElectricalConfigurationChanged();
        }

        public override SparkResult BeginInteraction(in SparkInteractionContext context)
        {
            return context.InteractionType == SparkInteractionType.Toggle ? Toggle(context) : base.BeginInteraction(context);
        }

    }
}
