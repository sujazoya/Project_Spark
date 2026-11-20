using UnityEngine;

namespace ProjectSpark.AutoLevelGenerator
{
    /// <summary>Inspector-only source identity for Level 107 generated objects. No electrical authority.</summary>
    [DisallowMultipleComponent]
    public sealed class SparkAutoL107LayoutRecord : MonoBehaviour
    {
        [SerializeField] private string sourceId;
        [SerializeField] private string category;
        [SerializeField] private string status;
        [SerializeField, TextArea(1, 4)] private string notes;
        [SerializeField] private bool referenceOnly;
        [SerializeField] private bool requiresSchematic;
        public string SourceId => sourceId;
        public string Category => category;
        public string Status => status;
        public bool ReferenceOnly => referenceOnly;
        public bool RequiresSchematic => requiresSchematic;
        public void Configure(string id, string kind, string state, string detail, bool reference = false, bool schematic = false)
        {
            sourceId = id ?? "";
            category = kind ?? "";
            status = state ?? "";
            notes = detail ?? "";
            referenceOnly = reference;
            requiresSchematic = schematic;
        }
    }
}
