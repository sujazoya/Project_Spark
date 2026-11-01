using UnityEngine;

namespace ProjectSpark.UI.VisualTeaching
{
    /// <summary>
    /// Place on a collider that represents a real electrical terminal.
    /// For component voltage, PositiveReference/NegativeReference must match
    /// the exact sign convention used by the authoritative solver reading.
    /// For arbitrary node voltage, terminalId must match the verified node feed.
    /// </summary>
    public enum SparkMeterTerminalEnd { Unspecified, PositiveReference, NegativeReference }

    [DisallowMultipleComponent]
    public sealed class SparkMeterProbeTerminal : MonoBehaviour
    {
        [SerializeField] private string terminalId;
        [SerializeField] private string componentAnchorId;
        [SerializeField] private SparkMeterTerminalEnd referenceEnd = SparkMeterTerminalEnd.Unspecified;

        public string TerminalId => terminalId;
        public string ComponentAnchorId => componentAnchorId;
        public SparkMeterTerminalEnd ReferenceEnd => referenceEnd;
    }
}
