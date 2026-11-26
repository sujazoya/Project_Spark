using ProjectSpark.Circuit;
using UnityEngine;

using ProjectSpark.Gameplay;
namespace ProjectSpark.SparkV5.Pro
{
    /// <summary>
    /// Native bridge contract. Implementation MUST invoke the same real cable/plug/terminal
    /// workflow used by the player, not a standalone LineRenderer or a graph-only shortcut.
    /// The controller never claims success until the actual SparkCircuitSystem sees the link.
    /// </summary>
    public abstract class SparkAINativeWirePort : MonoBehaviour
    {
        public abstract bool IsConfigured { get; }
        public abstract bool Begin(SparkTerminal source, SparkTerminal target, out string error);
        public abstract bool AttachSource(SparkTerminal source, out string error);
        public abstract bool MoveLooseEnd(Vector3 worldPosition, out string error);
        public abstract bool AttachDestination(SparkTerminal destination, out string error);
        public abstract void Abort();
        public abstract bool UndoLastCompletedWire(out string error);
    }
}
