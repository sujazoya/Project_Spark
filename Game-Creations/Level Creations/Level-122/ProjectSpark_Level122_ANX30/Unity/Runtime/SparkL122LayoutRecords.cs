using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectSpark.Level122
{
    // Authoring-only records. No invented electrical solver/gameplay API calls.
    public enum SparkL122TaskType { CopperRoute, Via }

    [Serializable]
    public sealed class SparkL122ComponentEntry
    {
        public string referenceId;
        public string assetId;
        public string kind;
        public Vector3 physicalLocationMetres;
        public bool footprintVerified;
        public bool installedAtStart;
    }

    [Serializable]
    public sealed class SparkL122RouteEntry
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
    public sealed class SparkL122ViaEntry
    {
        public string id;
        public string netId;
        public string fromLayer;
        public string toLayer;
        public bool verified;
    }

    [CreateAssetMenu(fileName = "L122_SourceBlueprint", menuName = "Project Spark/Levels/122 Source Blueprint")]
    public sealed class SparkL122SourceBlueprint : ScriptableObject
    {
        [SerializeField] private string boardId = "ANX-30";
        [SerializeField] private Vector2 physicalSizeMetres = new Vector2(.560f, .420f);
        [SerializeField] private int copperLayerCount = 36;
        [SerializeField] private bool schematicApproved = false;
        [SerializeField] private bool objectiveCertificationEnabled = false;
        [SerializeField] private string unresolvedSafetyLink = "W385: U5.TRUSTED_LOCALIZATION_READY_OUT -> U4.TRUSTED_LOCALIZATION_READY_IN (physical pins UNASSIGNED)";
        [SerializeField] private List<SparkL122ComponentEntry> components = new List<SparkL122ComponentEntry>();
        [SerializeField] private List<SparkL122RouteEntry> copperTasks = new List<SparkL122RouteEntry>();
        [SerializeField] private List<SparkL122ViaEntry> viaTasks = new List<SparkL122ViaEntry>();
        public string BoardId => boardId;
        public Vector2 PhysicalSizeMetres => physicalSizeMetres;
        public int CopperLayerCount => copperLayerCount;
        public bool SchematicApproved => schematicApproved;
        public bool ObjectiveCertificationEnabled => objectiveCertificationEnabled;
        public IReadOnlyList<SparkL122ComponentEntry> Components => components;
        public IReadOnlyList<SparkL122RouteEntry> CopperTasks => copperTasks;
        public IReadOnlyList<SparkL122ViaEntry> ViaTasks => viaTasks;
        public void SetSourceData(List<SparkL122ComponentEntry> partRecords,
            List<SparkL122RouteEntry> routeRecords, List<SparkL122ViaEntry> viaRecords)
        {
            components = partRecords ?? new List<SparkL122ComponentEntry>();
            copperTasks = routeRecords ?? new List<SparkL122RouteEntry>();
            viaTasks = viaRecords ?? new List<SparkL122ViaEntry>();
            schematicApproved = false;
            objectiveCertificationEnabled = false;
            unresolvedSafetyLink = "W385: U5.TRUSTED_LOCALIZATION_READY_OUT -> U4.TRUSTED_LOCALIZATION_READY_IN (physical pins UNASSIGNED)";
        }
    }

    [DisallowMultipleComponent]
    public sealed class SparkL122VisualOnly : MonoBehaviour
    {
        [TextArea(2, 4)] public string notice =
            "VISUAL ONLY / NOT CERTIFIABLE. Artwork cannot prove GNSS/radar/IMU electrical continuity, localization integrity, motion safety, or game objective completion.";
        public bool IsAuthoritative => false;
    }

    [DisallowMultipleComponent]
    public sealed class SparkL122LayoutMarker : MonoBehaviour
    {
        public const int LevelNumber = 122;
        public bool IsAuthorityBound => false;
    }

    [DisallowMultipleComponent]
    public sealed class SparkL122PlacementSlot : MonoBehaviour
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
    public sealed class SparkL122BlockedTask : MonoBehaviour
    {
        [SerializeField] private SparkL122TaskType type;
        [SerializeField] private string taskId;
        [SerializeField] private string domain;
        [SerializeField] private string blocker = "UNASSIGNED net/pads or 36-layer via span";
        public string TaskId => taskId;
        public bool CanCertify => false;
        public void Configure(SparkL122TaskType taskType, string identifier, string taskDomain)
        {
            type = taskType;
            taskId = identifier;
            domain = taskDomain;
        }
    }

    [DisallowMultipleComponent]
    public sealed class SparkL122IntegrationAnchor : MonoBehaviour
    {
        [SerializeField] private string requiredProjectService;
        public string RequiredService => requiredProjectService;
        public bool IsBound => false;
        public void Configure(string service) { requiredProjectService = service; }
    }
}
