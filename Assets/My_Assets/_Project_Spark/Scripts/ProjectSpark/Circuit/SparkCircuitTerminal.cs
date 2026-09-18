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
        public SparkResult ReceiveConnection(SparkTerminal other)
{
    if (other == null)
    {
        return SparkResult.Invalid(
            "Target terminal missing.");
    }

    SparkCircuitSystem system =
        FindFirstObjectByType<SparkCircuitSystem>();

    if (system == null)
    {
        return SparkResult.Unavailable(
            "No SparkCircuitSystem exists.");
    }

    SparkCircuitConnection connection;

    bool created =
        system.TryCreateConnection(
            terminal,
            other,
            SparkConnectionKind.Wire,
            SparkConnectionDirection.Bidirectional,
            out connection);

    if (!created)
    {
        return SparkResult.Rejected(
            $"Unable to connect '{terminal.name}' " +
            $"to '{other.name}'.");
    }

    return SparkResult.Success();
}
    }
}
