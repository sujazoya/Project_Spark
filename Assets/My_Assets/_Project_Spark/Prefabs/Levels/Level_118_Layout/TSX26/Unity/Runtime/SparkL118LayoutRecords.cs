using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectSpark.Level118
{
    // Authoring-only records. No invented electrical solver/gameplay API calls.
    public enum SparkL118TaskType { CopperRoute, Via }

    [Serializable]
    public sealed class SparkL118ComponentEntry
    {
        public string referenceId;
        public string assetId;
        public string kind;
        public Vector3 physicalLocationMetres;
        public bool footprintVerified;
        public bool installedAtStart;
    }

    [Serializable]
    public sealed class SparkL118RouteEntry
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
    public sealed class SparkL118ViaEntry
    {
        public string id;
        public string netId;
        public string fromLayer;
        public string toLayer;
        public bool verified;
    }

    [CreateAssetMenu(fileName = "L118_SourceBlueprint", menuName = "Project Spark/Levels/118 Source Blueprint")]
    public sealed class SparkL118SourceBlueprint : ScriptableObject
    {
        [SerializeField] private string boardId = "TSX-26";
        [SerializeField] private Vector2 physicalSizeMetres = new Vector2(.460f, .320f);
        [SerializeField] private int copperLayerCount = 26;
        [SerializeField] private bool schematicApproved = false;
        [SerializeField] private bool objectiveCertificationEnabled = false;
        [SerializeField] private string unresolvedSafetyLink = "W321: U31.TRUSTED_DETERMINISTIC_READY_OUT -> U32.TRUSTED_DETERMINISTIC_READY_IN (physical pins UNASSIGNED)";
        [SerializeField] private List<SparkL118ComponentEntry> components = new List<SparkL118ComponentEntry>();
        [SerializeField] private List<SparkL118RouteEntry> copperTasks = new List<SparkL118RouteEntry>();
        [SerializeField] private List<SparkL118ViaEntry> viaTasks = new List<SparkL118ViaEntry>();
        public string BoardId => boardId;
        public Vector2 PhysicalSizeMetres => physicalSizeMetres;
        public int CopperLayerCount => copperLayerCount;
        public bool SchematicApproved => schematicApproved;
        public bool ObjectiveCertificationEnabled => objectiveCertificationEnabled;
        public IReadOnlyList<SparkL118ComponentEntry> Components => components;
        public IReadOnlyList<SparkL118RouteEntry> CopperTasks => copperTasks;
        public IReadOnlyList<SparkL118ViaEntry> ViaTasks => viaTasks;
        public void SetSourceData(List<SparkL118ComponentEntry> partRecords,
            List<SparkL118RouteEntry> routeRecords, List<SparkL118ViaEntry> viaRecords)
        {
            components = partRecords ?? new List<SparkL118ComponentEntry>();
            copperTasks = routeRecords ?? new List<SparkL118RouteEntry>();
            viaTasks = viaRecords ?? new List<SparkL118ViaEntry>();
            schematicApproved = false;
            objectiveCertificationEnabled = false;
            unresolvedSafetyLink = "W321: U31.TRUSTED_DETERMINISTIC_READY_OUT -> U32.TRUSTED_DETERMINISTIC_READY_IN (physical pins UNASSIGNED)";
        }
    }

    [DisallowMultipleComponent]
    public sealed class SparkL118VisualOnly : MonoBehaviour
    {
        [TextArea(2, 4)] public string notice =
            "VISUAL ONLY / NOT CERTIFIABLE. Artwork is not optical link wiring, solver continuity, gPTP/TSN evidence or objective authority.";
        public bool IsAuthoritative => false;
    }

    [DisallowMultipleComponent]
    public sealed class SparkL118LayoutMarker : MonoBehaviour
    {
        public const int LevelNumber = 118;
        public bool IsAuthorityBound => false;
    }

    [DisallowMultipleComponent]
    public sealed class SparkL118PlacementSlot : MonoBehaviour
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
    public sealed class SparkL118BlockedTask : MonoBehaviour
    {
        [SerializeField] private SparkL118TaskType type;
        [SerializeField] private string taskId;
        [SerializeField] private string domain;
        [SerializeField] private string blocker = "UNASSIGNED net/pads or 28-layer via span";
        public string TaskId => taskId;
        public bool CanCertify => false;
        public void Configure(SparkL118TaskType taskType, string identifier, string taskDomain)
        {
            type = taskType;
            taskId = identifier;
            domain = taskDomain;
        }
    }

    [DisallowMultipleComponent]
    public sealed class SparkL118IntegrationAnchor : MonoBehaviour
    {
        [SerializeField] private string requiredProjectService;
        public string RequiredService => requiredProjectService;
        public bool IsBound => false;
        public void Configure(string service) { requiredProjectService = service; }
    }
}
