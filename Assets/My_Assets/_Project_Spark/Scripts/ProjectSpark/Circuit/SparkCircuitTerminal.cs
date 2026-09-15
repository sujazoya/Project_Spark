using ProjectSpark.Gameplay;
using UnityEngine;

namespace ProjectSpark.Circuit
{
    /// <summary>
    /// Compatibility component for code/prefabs that used the previous CircuitTerminal name.
    /// It delegates all terminal authority to SparkTerminal.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SparkTerminal))]
    public sealed class CircuitTerminal : MonoBehaviour
    {
        [SerializeField] private SparkTerminal terminal;
        public SparkTerminal Terminal => terminal;
        private void Awake() { if (terminal == null) terminal = GetComponent<SparkTerminal>(); }
        public SparkResult ReceiveConnection(CircuitTerminal other)
        {
            if (other == null || other.terminal == null) return SparkResult.Invalid("Target terminal missing.");
            var system = FindFirstObjectByType<SparkCircuitSystem>();
            if (system == null) return SparkResult.Unavailable("No SparkCircuitSystem exists.");
            return system.TryCreateConnection(terminal, other.terminal, SparkConnectionKind.Wire,
                SparkConnectionDirection.Bidirectional, out _);
        }
    }
}
