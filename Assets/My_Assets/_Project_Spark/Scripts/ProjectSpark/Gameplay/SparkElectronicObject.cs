using System;
using UnityEngine;

namespace ProjectSpark.Gameplay
{
    public enum SparkOperationalState { Standby, Active, Fault, Disabled }

    [DisallowMultipleComponent]
    public class SparkElectronicObject : MonoBehaviour, ISparkInteractable
    {
        [Header("Identity")]
        [SerializeField] private string objectId;
        [SerializeField] private string displayName;

        [Header("Interaction")]
        [SerializeField] private bool interactionsEnabled = true;
        [SerializeField] private SparkOperationalState operationalState = SparkOperationalState.Standby;

        public string ObjectId => objectId;
        public string DisplayName => displayName;
        public bool InteractionsEnabled => interactionsEnabled;
        public SparkOperationalState OperationalState => operationalState;
        public bool IsHovered { get; private set; }
        public bool IsSelected { get; private set; }

        public event Action<SparkElectronicObject> HoverChanged;
        public event Action<SparkElectronicObject> SelectionChanged;
        public event Action<SparkOperationalState> OperationalStateChanged;

        protected virtual void Awake()
        {
            EnsureIdentity();
        }

        protected virtual void OnValidate()
        {
            EnsureIdentity();
            if (string.IsNullOrWhiteSpace(displayName)) displayName = name;
        }

        private void EnsureIdentity()
        {
            if (string.IsNullOrWhiteSpace(objectId)) objectId = Guid.NewGuid().ToString("N");
            if (string.IsNullOrWhiteSpace(displayName)) displayName = name;
        }

        public void SetHovered(bool value)
        {
            if (IsHovered == value) return;
            IsHovered = value;
            HoverChanged?.Invoke(this);
        }

        public void SetSelected(bool value)
        {
            if (IsSelected == value) return;
            IsSelected = value;
            SelectionChanged?.Invoke(this);
        }

        public bool SetOperationalState(SparkOperationalState state)
        {
            if (operationalState == state) return false;
            operationalState = state;
            OperationalStateChanged?.Invoke(state);
            return true;
        }

        public void SetInteractionsEnabled(bool value)
        {
            interactionsEnabled = value;
            if (!value) { SetHovered(false); SetSelected(false); }
        }

        public virtual bool CanInteract(in SparkInteractionContext context, out string reason)
        {
            if (!isActiveAndEnabled || !interactionsEnabled)
            { reason = "Object interaction is disabled."; return false; }
            if (operationalState == SparkOperationalState.Disabled)
            { reason = "Object is disabled."; return false; }
            if (operationalState == SparkOperationalState.Fault &&
                context.InteractionType != SparkInteractionType.Select &&
                context.InteractionType != SparkInteractionType.Inspect &&
                context.InteractionType != SparkInteractionType.Scan)
            { reason = "Object is faulted."; return false; }
            reason = null; return true;
        }

        public virtual SparkResult BeginInteraction(in SparkInteractionContext context) => SparkResult.Success();
        public virtual SparkResult UpdateInteraction(in SparkInteractionContext context) => SparkResult.Success();
        public virtual SparkResult EndInteraction(in SparkInteractionContext context) => SparkResult.Success();
        public virtual SparkResult CancelInteraction(in SparkInteractionContext context) => SparkResult.Cancelled();

        public virtual SparkResult Inspect(in SparkInteractionContext context) =>
            CanInteract(context, out var reason) ? SparkResult.Success() : SparkResult.Rejected(reason);
    }
}
