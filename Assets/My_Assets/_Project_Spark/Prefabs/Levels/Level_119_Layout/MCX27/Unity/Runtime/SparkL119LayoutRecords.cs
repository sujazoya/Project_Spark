using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectSpark.Level119
{
    // Authoring-only records. No invented electrical solver/gameplay API calls.
    public enum SparkL119TaskType { CopperRoute, Via }

    [Serializable]
    public sealed class SparkL119ComponentEntry
    {
        public string referenceId;
        public string assetId;
        public string kind;
        public Vector3 physicalLocationMetres;
        public bool footprintVerified;
        public bool installedAtStart;
    }

    [Serializable]
    public sealed class SparkL119RouteEntry
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
    public sealed class SparkL119ViaEntry
    {
        public string id;
        public string netId;
        public string fromLayer;
        public string toLayer;
        public bool verified;
    }

    [CreateAssetMenu(fileName = "L119_SourceBlueprint", menuName = "Project Spark/Levels/119 Source Blueprint")]
    public sealed class SparkL119SourceBlueprint : ScriptableObject
    {
        [SerializeField] private string boardId = "MCX-27";
        [SerializeField] private Vector2 physicalSizeMetres = new Vector2(.500f, .360f);
        [SerializeField] private int copperLayerCount = 30;
        [SerializeField] private bool schematicApproved = false;
        [SerializeField] private bool objectiveCertificationEnabled = false;
        [SerializeField] private string unresolvedSafetyLink = "W337: U39.TRUSTED_MOTION_READY_OUT -> U40.TRUSTED_MOTION_READY_IN (physical pins UNASSIGNED)";
        [SerializeField] private List<SparkL119ComponentEntry> components = new List<SparkL119ComponentEntry>();
        [SerializeField] private List<SparkL119RouteEntry> copperTasks = new List<SparkL119RouteEntry>();
        [SerializeField] private List<SparkL119ViaEntry> viaTasks = new List<SparkL119ViaEntry>();
        public string BoardId => boardId;
        public Vector2 PhysicalSizeMetres => physicalSizeMetres;
        public int CopperLayerCount => copperLayerCount;
        public bool SchematicApproved => schematicApproved;
        public bool ObjectiveCertificationEnabled => objectiveCertificationEnabled;
        public IReadOnlyList<SparkL119ComponentEntry> Components => components;
        public IReadOnlyList<SparkL119RouteEntry> CopperTasks => copperTasks;
        public IReadOnlyList<SparkL119ViaEntry> ViaTasks => viaTasks;
        public void SetSourceData(List<SparkL119ComponentEntry> partRecords,
            List<SparkL119RouteEntry> routeRecords, List<SparkL119ViaEntry> viaRecords)
        {
            components = partRecords ?? new List<SparkL119ComponentEntry>();
            copperTasks = routeRecords ?? new List<SparkL119RouteEntry>();
            viaTasks = viaRecords ?? new List<SparkL119ViaEntry>();
            schematicApproved = false;
            objectiveCertificationEnabled = false;
            unresolvedSafetyLink = "W337: U39.TRUSTED_MOTION_READY_OUT -> U40.TRUSTED_MOTION_READY_IN (physical pins UNASSIGNED)";
        }
    }

    [DisallowMultipleComponent]
    public sealed class SparkL119VisualOnly : MonoBehaviour
    {
        [TextArea(2, 4)] public string notice =
            "VISUAL ONLY / NOT CERTIFIABLE. Artwork is not verified power-phase wiring, torque feedback, STO qualification or objective authority.";
        public bool IsAuthoritative => false;
    }

    [DisallowMultipleComponent]
    public sealed class SparkL119LayoutMarker : MonoBehaviour
    {
        public const int LevelNumber = 119;
        public bool IsAuthorityBound => false;
    }

    [DisallowMultipleComponent]
    public sealed class SparkL119PlacementSlot : MonoBehaviour
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
    public sealed class SparkL119BlockedTask : MonoBehaviour
    {
        [SerializeField] private SparkL119TaskType type;
        [SerializeField] private string taskId;
        [SerializeField] private string domain;
        [SerializeField] private string blocker = "UNASSIGNED net/pads or 30-layer via span";
        public string TaskId => taskId;
        public bool CanCertify => false;
        public void Configure(SparkL119TaskType taskType, string identifier, string taskDomain)
        {
            type = taskType;
            taskId = identifier;
            domain = taskDomain;
        }
    }

    [DisallowMultipleComponent]
    public sealed class SparkL119IntegrationAnchor : MonoBehaviour
    {
        [SerializeField] private string requiredProjectService;
        public string RequiredService => requiredProjectService;
        public bool IsBound => false;
        public void Configure(string service) { requiredProjectService = service; }
    }
}
