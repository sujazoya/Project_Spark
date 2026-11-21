using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectSpark.Level117
{
    // Authoring-only records. No invented electrical solver/gameplay API calls.
    public enum SparkL117TaskType { CopperRoute, Via }

    [Serializable]
    public sealed class SparkL117ComponentEntry
    {
        public string referenceId;
        public string assetId;
        public string kind;
        public Vector3 physicalLocationMetres;
        public bool footprintVerified;
        public bool installedAtStart;
    }

    [Serializable]
    public sealed class SparkL117RouteEntry
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
    public sealed class SparkL117ViaEntry
    {
        public string id;
        public string netId;
        public string fromLayer;
        public string toLayer;
        public bool verified;
    }

    [CreateAssetMenu(fileName = "L117_SourceBlueprint", menuName = "Project Spark/Levels/117 Source Blueprint")]
    public sealed class SparkL117SourceBlueprint : ScriptableObject
    {
        [SerializeField] private string boardId = "NFX-25";
        [SerializeField] private Vector2 physicalSizeMetres = new Vector2(.460f, .320f);
        [SerializeField] private int copperLayerCount = 26;
        [SerializeField] private bool schematicApproved = false;
        [SerializeField] private bool objectiveCertificationEnabled = false;
        [SerializeField] private string unresolvedSafetyLink = "W305: U35.TRUSTED_NETWORK_READY_OUT -> U36.TRUSTED_NETWORK_READY_IN (physical pins UNASSIGNED)";
        [SerializeField] private List<SparkL117ComponentEntry> components = new List<SparkL117ComponentEntry>();
        [SerializeField] private List<SparkL117RouteEntry> copperTasks = new List<SparkL117RouteEntry>();
        [SerializeField] private List<SparkL117ViaEntry> viaTasks = new List<SparkL117ViaEntry>();
        public string BoardId => boardId;
        public Vector2 PhysicalSizeMetres => physicalSizeMetres;
        public int CopperLayerCount => copperLayerCount;
        public bool SchematicApproved => schematicApproved;
        public bool ObjectiveCertificationEnabled => objectiveCertificationEnabled;
        public IReadOnlyList<SparkL117ComponentEntry> Components => components;
        public IReadOnlyList<SparkL117RouteEntry> CopperTasks => copperTasks;
        public IReadOnlyList<SparkL117ViaEntry> ViaTasks => viaTasks;
        public void SetSourceData(List<SparkL117ComponentEntry> partRecords,
            List<SparkL117RouteEntry> routeRecords, List<SparkL117ViaEntry> viaRecords)
        {
            components = partRecords ?? new List<SparkL117ComponentEntry>();
            copperTasks = routeRecords ?? new List<SparkL117RouteEntry>();
            viaTasks = viaRecords ?? new List<SparkL117ViaEntry>();
            schematicApproved = false;
            objectiveCertificationEnabled = false;
            unresolvedSafetyLink = "W305: U35.TRUSTED_NETWORK_READY_OUT -> U36.TRUSTED_NETWORK_READY_IN (physical pins UNASSIGNED)";
        }
    }

    [DisallowMultipleComponent]
    public sealed class SparkL117VisualOnly : MonoBehaviour
    {
        [TextArea(2, 4)] public string notice =
            "VISUAL ONLY / NOT CERTIFIABLE. Artwork is not optical link wiring, solver continuity, CRC/PTP evidence or objective authority.";
        public bool IsAuthoritative => false;
    }

    [DisallowMultipleComponent]
    public sealed class SparkL117LayoutMarker : MonoBehaviour
    {
        public const int LevelNumber = 117;
        public bool IsAuthorityBound => false;
    }

    [DisallowMultipleComponent]
    public sealed class SparkL117PlacementSlot : MonoBehaviour
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
    public sealed class SparkL117BlockedTask : MonoBehaviour
    {
        [SerializeField] private SparkL117TaskType type;
        [SerializeField] private string taskId;
        [SerializeField] private string domain;
        [SerializeField] private string blocker = "UNASSIGNED net/pads or 26-layer via span";
        public string TaskId => taskId;
        public bool CanCertify => false;
        public void Configure(SparkL117TaskType taskType, string identifier, string taskDomain)
        {
            type = taskType;
            taskId = identifier;
            domain = taskDomain;
        }
    }

    [DisallowMultipleComponent]
    public sealed class SparkL117IntegrationAnchor : MonoBehaviour
    {
        [SerializeField] private string requiredProjectService;
        public string RequiredService => requiredProjectService;
        public bool IsBound => false;
        public void Configure(string service) { requiredProjectService = service; }
    }
}
