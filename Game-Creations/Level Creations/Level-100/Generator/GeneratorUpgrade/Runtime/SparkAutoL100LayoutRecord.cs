using UnityEngine;
namespace ProjectSpark.AutoLevelGenerator
{
    /// <summary>Source-backed inspector metadata, not an electrical authority.</summary>
    [DisallowMultipleComponent]
    public sealed class SparkAutoL100LayoutRecord : MonoBehaviour
    {
        [SerializeField] private string sourceId;
        [SerializeField] private string category;
        [SerializeField] private string status;
        [SerializeField, TextArea(1, 5)] private string sourceNotes;
        [SerializeField] private bool artOnly;
        [SerializeField] private bool schematicRequired;
        public string SourceId => sourceId;
        public string Category => category;
        public string Status => status;
        public bool ArtOnly => artOnly;
        public bool SchematicRequired => schematicRequired;
        public void Configure(string id, string type, string state, string notes, bool referenceOnly = false, bool requiresSchematic = false)
        {
            sourceId = id ?? string.Empty;
            category = type ?? string.Empty;
            status = state ?? string.Empty;
            sourceNotes = notes ?? string.Empty;
            artOnly = referenceOnly;
            schematicRequired = requiresSchematic;
        }
    }
}
