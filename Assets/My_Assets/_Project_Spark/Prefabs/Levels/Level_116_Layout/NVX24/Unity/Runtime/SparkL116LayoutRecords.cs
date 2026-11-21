using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectSpark.Level116
{
    // Authoring-only records. No invented electrical solver/gameplay API calls.
    public enum SparkL116TaskType { CopperRoute, Via }

    [Serializable]
    public sealed class SparkL116ComponentEntry
    {
        public string referenceId;
        public string assetId;
        public string kind;
        public Vector3 physicalLocationMetres;
        public bool footprintVerified;
        public bool installedAtStart;
    }

    [Serializable]
    public sealed class SparkL116RouteEntry
    {
        public string id;
        public string domain;
        public string sourcePad;
        public string destinationPad;
        public string netId;
        public string layer;
        public bool certifiable;
    }

    [Serializable]
    public sealed class SparkL116ViaEntry
    {
        public string id;
        public string netId;
        public string fromLayer;
        public string toLayer;
        public bool verified;
    }

    [CreateAssetMenu(fileName = "L116_SourceBlueprint", menuName = "Project Spark/Levels/116 Source Blueprint")]
    public sealed class SparkL116SourceBlueprint : ScriptableObject
    {
        [SerializeField] private string boardId = "NVX-24";
        [SerializeField] private Vector2 physicalSizeMetres = new Vector2(.440f, .310f);
        [SerializeField] private int copperLayerCount = 24;
        [SerializeField] private bool schematicApproved = false;
        [SerializeField] private bool objectiveCertificationEnabled = false;
        [SerializeField] private string unresolvedSafetyLink = "W289 U31/U30 pin assignments UNASSIGNED";
        [SerializeField] private List<SparkL116ComponentEntry> components = new List<SparkL116ComponentEntry>();
        [SerializeField] private List<SparkL116RouteEntry> copperTasks = new List<SparkL116RouteEntry>();
        [SerializeField] private List<SparkL116ViaEntry> viaTasks = new List<SparkL116ViaEntry>();
        public string BoardId => boardId;
        public Vector2 PhysicalSizeMetres => physicalSizeMetres;
        public int CopperLayerCount => copperLayerCount;
        public bool SchematicApproved => schematicApproved;
        public bool ObjectiveCertificationEnabled => objectiveCertificationEnabled;
        public IReadOnlyList<SparkL116ComponentEntry> Components => components;
        public IReadOnlyList<SparkL116RouteEntry> CopperTasks => copperTasks;
        public IReadOnlyList<SparkL116ViaEntry> ViaTasks => viaTasks;
        public void SetSourceData(List<SparkL116ComponentEntry> partRecords,
            List<SparkL116RouteEntry> routeRecords, List<SparkL116ViaEntry> viaRecords)
        {
            components = partRecords ?? new List<SparkL116ComponentEntry>();
            copperTasks = routeRecords ?? new List<SparkL116RouteEntry>();
            viaTasks = viaRecords ?? new List<SparkL116ViaEntry>();
            schematicApproved = false;
            objectiveCertificationEnabled = false;
            unresolvedSafetyLink = "W289 U31/U30 pin assignments UNASSIGNED";
        }
    }

    [DisallowMultipleComponent]
    public sealed class SparkL116VisualOnly : MonoBehaviour
    {
        [TextArea(2, 4)] public string notice =
            "VISUAL ONLY / NOT CERTIFIABLE. Artwork is not NAND wiring, solver continuity, durable write evidence or objective authority.";
        public bool IsAuthoritative => false;
    }

    [DisallowMultipleComponent]
    public sealed class SparkL116LayoutMarker : MonoBehaviour
    {
        public const int LevelNumber = 116;
        public bool IsAuthorityBound => false;
    }

    [DisallowMultipleComponent]
    public sealed class SparkL116PlacementSlot : MonoBehaviour
    {
        [SerializeField] private string componentRef;
        [SerializeField] private string assetId;
        [SerializeField] private string componentKind;
        [SerializeField] private bool physicalFootprintVerified;
        [SerializeField] private bool installed;
        public string ComponentRef => componentRef;
        public string AssetId => assetId;
        public bool IsInstalled => installed;
        public bool IsPlacementCertifiable => physicalFootprintVerified;
        public void Configure(string referenceId, string manifestAssetId, string kind)
        {
            componentRef = referenceId;
            assetId = manifestAssetId;
            componentKind = kind;
            physicalFootprintVerified = false;
            installed = false;
        }
    }

    [DisallowMultipleComponent]
    public sealed class SparkL116BlockedTask : MonoBehaviour
    {
        [SerializeField] private SparkL116TaskType type;
        [SerializeField] private string taskId;
        [SerializeField] private string domain;
        [SerializeField] private string blocker = "UNASSIGNED net/pads or 24-layer via span";
        public string TaskId => taskId;
        public bool CanCertify => false;
        public void Configure(SparkL116TaskType taskType, string identifier, string taskDomain)
        {
            type = taskType;
            taskId = identifier;
            domain = taskDomain;
        }
    }

    [DisallowMultipleComponent]
    public sealed class SparkL116IntegrationAnchor : MonoBehaviour
    {
        [SerializeField] private string requiredProjectService;
        public string RequiredService => requiredProjectService;
        public bool IsBound => false;
        public void Configure(string service) { requiredProjectService = service; }
    }
}
