using System;
using ProjectSpark.Circuit;
using UnityEngine;
using UnityEngine.Events;

using ProjectSpark.Gameplay;
namespace ProjectSpark.SparkV5.Pro
{
    /// <summary>
    /// Inspector-bindable bridge to existing PUBLIC methods on the player's wire tool.
    /// Binding a listener which merely displays a line is NOT a valid implementation.
    /// Native Wire Director verifies authoritative connectivity after OnComplete.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkAINativeWireUnityEventPort : SparkAINativeWirePort
    {
        [Serializable] public sealed class TerminalEvent : UnityEvent<SparkTerminal> { }
        [Serializable] public sealed class PointEvent : UnityEvent<Vector3> { }

        [Header("Bind these to actual PLAYER WIRE TOOL actions")]
        [SerializeField] private UnityEvent onCreatePlayerWire = new UnityEvent();
        [SerializeField] private TerminalEvent onAttachSource = new TerminalEvent();
        [SerializeField] private PointEvent onMoveFreeCableEnd = new PointEvent();
        [SerializeField] private TerminalEvent onAttachDestination = new TerminalEvent();
        [SerializeField] private UnityEvent onFinalizePlayerWire = new UnityEvent();
        [SerializeField] private UnityEvent onCancelPlayerWire = new UnityEvent();
        [SerializeField] private UnityEvent onUndoPlayerWire = new UnityEvent();
        [Header("Validation")]
        [SerializeField] private bool validatePersistentListeners = true;

        public SparkTerminal Source { get; private set; }
        public SparkTerminal Destination { get; private set; }
        public Vector3 LooseEndPosition { get; private set; }
        public bool Active { get; private set; }
        public override bool IsConfigured => !validatePersistentListeners ||
            (onCreatePlayerWire.GetPersistentEventCount() > 0 &&
             onAttachSource.GetPersistentEventCount() > 0 &&
             onMoveFreeCableEnd.GetPersistentEventCount() > 0 &&
             onAttachDestination.GetPersistentEventCount() > 0 &&
             onFinalizePlayerWire.GetPersistentEventCount() > 0 &&
             onCancelPlayerWire.GetPersistentEventCount() > 0);

        public override bool Begin(SparkTerminal source, SparkTerminal target, out string error)
        {
            error = string.Empty;
            if (!IsConfigured) { error = "Player Wire Tool event bindings are incomplete."; return false; }
            if (Active) { error = "A native wire is already being manipulated."; return false; }
            if (!source || !target) { error = "The selected terminals are unavailable."; return false; }
            Source = source;
            Destination = target;
            LooseEndPosition = source.transform.position;
            Active = true;
            onCreatePlayerWire.Invoke();
            return true;
        }
        public override bool AttachSource(SparkTerminal source, out string error)
        {
            error = string.Empty;
            if (!Active || source != Source) { error = "Native source state changed."; return false; }
            onAttachSource.Invoke(source);
            return true;
        }
        public override bool MoveLooseEnd(Vector3 position, out string error)
        {
            error = string.Empty;
            if (!Active) { error = "No active player cable."; return false; }
            LooseEndPosition = position;
            onMoveFreeCableEnd.Invoke(position);
            return true;
        }
        public override bool AttachDestination(SparkTerminal destination, out string error)
        {
            error = string.Empty;
            if (!Active || destination != Destination) { error = "Native destination state changed."; return false; }
            onAttachDestination.Invoke(destination);
            onFinalizePlayerWire.Invoke();
            Active = false;
            return true;
        }
        public override void Abort()
        {
            if (Active) onCancelPlayerWire.Invoke();
            Active = false;
            Source = null;
            Destination = null;
        }
        public override bool UndoLastCompletedWire(out string error)
        {
            error = "Native Wire Tool does not expose an undo callback.";
            if (Active || onUndoPlayerWire.GetPersistentEventCount() == 0) return false;
            onUndoPlayerWire.Invoke();
            error = string.Empty;
            return true;
        }
    }
}
