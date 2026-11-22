using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectSpark.Level123
{
    // Authoring-only records. No invented electrical solver/gameplay API calls.
    public enum SparkL123TaskType { CopperRoute, Via }

    [Serializable]
    public sealed class SparkL123ComponentEntry
    {
        public string referenceId;
        public string assetId;
        public string kind;
        public Vector3 physicalLocationMetres;
        public bool footprintVerified;
        public bool installedAtStart;
    }

    [Serializable]
    public sealed class SparkL123RouteEntry
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
    public sealed class SparkL123ViaEntry
    {
        public string id;
        public string netId;
        public string fromLayer;
        public string toLayer;
        public bool verified;
    }

    [CreateAssetMenu(fileName = "L123_SourceBlueprint", menuName = "Project Spark/Levels/123 Source Blueprint")]
    public sealed class SparkL123SourceBlueprint : ScriptableObject
    {
        [SerializeField] private string boardId = "AFX-31";
        [SerializeField] private Vector2 physicalSizeMetres = new Vector2(.580f, .440f);
        [SerializeField] private int copperLayerCount = 38;
        [SerializeField] private bool schematicApproved = false;
        [SerializeField] private bool objectiveCertificationEnabled = false;
        [SerializeField] private string unresolvedSafetyLink = "W401: U5.TRUSTED_FLIGHT_READY_OUT -> U3.TRUSTED_FLIGHT_READY_IN (physical pins UNASSIGNED)";
        [SerializeField] private List<SparkL123ComponentEntry> components = new List<SparkL123ComponentEntry>();
        [SerializeField] private List<SparkL123RouteEntry> copperTasks = new List<SparkL123RouteEntry>();
        [SerializeField] private List<SparkL123ViaEntry> viaTasks = new List<SparkL123ViaEntry>();
        public string BoardId => boardId;
        public Vector2 PhysicalSizeMetres => physicalSizeMetres;
        public int CopperLayerCount => copperLayerCount;
        public bool SchematicApproved => schematicApproved;
        public bool ObjectiveCertificationEnabled => objectiveCertificationEnabled;
        public IReadOnlyList<SparkL123ComponentEntry> Components => components;
        public IReadOnlyList<SparkL123RouteEntry> CopperTasks => copperTasks;
        public IReadOnlyList<SparkL123ViaEntry> ViaTasks => viaTasks;
        public void SetSourceData(List<SparkL123ComponentEntry> partRecords,
            List<SparkL123RouteEntry> routeRecords, List<SparkL123ViaEntry> viaRecords)
        {
            components = partRecords ?? new List<SparkL123ComponentEntry>();
            copperTasks = routeRecords ?? new List<SparkL123RouteEntry>();
            viaTasks = viaRecords ?? new List<SparkL123ViaEntry>();
            schematicApproved = false;
            objectiveCertificationEnabled = false;
            unresolvedSafetyLink = "W401: U5.TRUSTED_FLIGHT_READY_OUT -> U3.TRUSTED_FLIGHT_READY_IN (physical pins UNASSIGNED)";
        }
    }

    [DisallowMultipleComponent]
    public sealed class SparkL123VisualOnly : MonoBehaviour
    {
        [TextArea(2, 4)] public string notice =
            "VISUAL ONLY / NOT CERTIFIABLE. Artwork cannot prove flight-controller and motor electrical continuity, flight safety, or game objective completion.";
        public bool IsAuthoritative => false;
    }

    [DisallowMultipleComponent]
    public sealed class SparkL123LayoutMarker : MonoBehaviour
    {
        public const int LevelNumber = 123;
        public bool IsAuthorityBound => false;
    }

    [DisallowMultipleComponent]
    public sealed class SparkL123PlacementSlot : MonoBehaviour
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
    public sealed class SparkL123BlockedTask : MonoBehaviour
    {
        [SerializeField] private SparkL123TaskType type;
        [SerializeField] private string taskId;
        [SerializeField] private string domain;
        [SerializeField] private string blocker = "UNASSIGNED net/pads or 38-layer via span";
        public string TaskId => taskId;
        public bool CanCertify => false;
        public void Configure(SparkL123TaskType taskType, string identifier, string taskDomain)
        {
            type = taskType;
            taskId = identifier;
            domain = taskDomain;
        }
    }

    [DisallowMultipleComponent]
    public sealed class SparkL123IntegrationAnchor : MonoBehaviour
    {
        [SerializeField] private string requiredProjectService;
        public string RequiredService => requiredProjectService;
        public bool IsBound => false;
        public void Configure(string service) { requiredProjectService = service; }
    }
}
