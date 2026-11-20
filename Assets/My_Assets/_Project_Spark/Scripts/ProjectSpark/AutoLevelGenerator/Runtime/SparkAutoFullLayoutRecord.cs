using UnityEngine;

namespace ProjectSpark.AutoLevelGenerator
{
    /// <summary>
    /// Source-grounded inspector metadata for a generated layout entry.
    /// It does NOT provide electrical connectivity, gameplay completion, or a repaired-state bypass.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkAutoFullLayoutRecord : MonoBehaviour
    {
        [SerializeField] private string recordId;
        [SerializeField] private string recordKind;
        [SerializeField] private string sourceStatus;
        [SerializeField, TextArea(1, 3)] private string description;
        [SerializeField] private bool requiresApprovedSchematic;
        [SerializeField] private bool isReferenceOnly;

        public string RecordId => recordId;
        public string RecordKind => recordKind;
        public string SourceStatus => sourceStatus;
        public bool RequiresApprovedSchematic => requiresApprovedSchematic;
        public bool IsReferenceOnly => isReferenceOnly;

        public void SetDefinition(string id, string kind, string status, string detail,
            bool schematicRequired = false, bool reference = false)
        {
            recordId = id ?? string.Empty;
            recordKind = kind ?? string.Empty;
            sourceStatus = status ?? string.Empty;
            description = detail ?? string.Empty;
            requiresApprovedSchematic = schematicRequired;
            isReferenceOnly = reference;
        }
    }
}
