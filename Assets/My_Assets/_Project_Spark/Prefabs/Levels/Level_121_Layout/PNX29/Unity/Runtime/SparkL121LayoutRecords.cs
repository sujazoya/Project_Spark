using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectSpark.Level121
{
    // Authoring-only records. No invented electrical solver/gameplay API calls.
    public enum SparkL121TaskType { CopperRoute, Via }

    [Serializable]
    public sealed class SparkL121ComponentEntry
    {
        public string referenceId;
        public string assetId;
        public string kind;
        public Vector3 physicalLocationMetres;
        public bool footprintVerified;
        public bool installedAtStart;
    }

    [Serializable]
    public sealed class SparkL121RouteEntry
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
    public sealed class SparkL121ViaEntry
    {
        public string id;
        public string netId;
        public string fromLayer;
        public string toLayer;
        public bool verified;
    }

    [CreateAssetMenu(fileName = "L121_SourceBlueprint", menuName = "Project Spark/Levels/121 Source Blueprint")]
    public sealed class SparkL121SourceBlueprint : ScriptableObject
    {
        [SerializeField] private string boardId = "PNX-29";
        [SerializeField] private Vector2 physicalSizeMetres = new Vector2(.540f, .400f);
        [SerializeField] private int copperLayerCount = 34;
        [SerializeField] private bool schematicApproved = false;
        [SerializeField] private bool objectiveCertificationEnabled = false;
        [SerializeField] private string unresolvedSafetyLink = "W369: U5.TRUSTED_PERCEPTION_READY_OUT -> U6.TRUSTED_PERCEPTION_READY_IN (physical pins UNASSIGNED)";
        [SerializeField] private List<SparkL121ComponentEntry> components = new List<SparkL121ComponentEntry>();
        [SerializeField] private List<SparkL121RouteEntry> copperTasks = new List<SparkL121RouteEntry>();
        [SerializeField] private List<SparkL121ViaEntry> viaTasks = new List<SparkL121ViaEntry>();
        public string BoardId => boardId;
        public Vector2 PhysicalSizeMetres => physicalSizeMetres;
        public int CopperLayerCount => copperLayerCount;
        public bool SchematicApproved => schematicApproved;
        public bool ObjectiveCertificationEnabled => objectiveCertificationEnabled;
        public IReadOnlyList<SparkL121ComponentEntry> Components => components;
        public IReadOnlyList<SparkL121RouteEntry> CopperTasks => copperTasks;
        public IReadOnlyList<SparkL121ViaEntry> ViaTasks => viaTasks;
        public void SetSourceData(List<SparkL121ComponentEntry> partRecords,
            List<SparkL121RouteEntry> routeRecords, List<SparkL121ViaEntry> viaRecords)
        {
            components = partRecords ?? new List<SparkL121ComponentEntry>();
            copperTasks = routeRecords ?? new List<SparkL121RouteEntry>();
            viaTasks = viaRecords ?? new List<SparkL121ViaEntry>();
            schematicApproved = false;
            objectiveCertificationEnabled = false;
            unresolvedSafetyLink = "W369: U5.TRUSTED_PERCEPTION_READY_OUT -> U6.TRUSTED_PERCEPTION_READY_IN (physical pins UNASSIGNED)";
        }
    }

    [DisallowMultipleComponent]
    public sealed class SparkL121VisualOnly : MonoBehaviour
    {
        [TextArea(2, 4)] public string notice =
            "VISUAL ONLY / NOT CERTIFIABLE. Artwork is not verified approved sensor electrical connectivity, perception evidence, safety qualification or objective authority.";
        public bool IsAuthoritative => false;
    }

    [DisallowMultipleComponent]
    public sealed class SparkL121LayoutMarker : MonoBehaviour
    {
        public const int LevelNumber = 121;
        public bool IsAuthorityBound => false;
    }

    [DisallowMultipleComponent]
    public sealed class SparkL121PlacementSlot : MonoBehaviour
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
    public sealed class SparkL121BlockedTask : MonoBehaviour
    {
        [SerializeField] private SparkL121TaskType type;
        [SerializeField] private string taskId;
        [SerializeField] private string domain;
        [SerializeField] private string blocker = "UNASSIGNED net/pads or 34-layer via span";
        public string TaskId => taskId;
        public bool CanCertify => false;
        public void Configure(SparkL121TaskType taskType, string identifier, string taskDomain)
        {
            type = taskType;
            taskId = identifier;
            domain = taskDomain;
        }
    }

    [DisallowMultipleComponent]
    public sealed class SparkL121IntegrationAnchor : MonoBehaviour
    {
        [SerializeField] private string requiredProjectService;
        public string RequiredService => requiredProjectService;
        public bool IsBound => false;
        public void Configure(string service) { requiredProjectService = service; }
    }
}
