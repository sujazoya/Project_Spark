using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectSpark.Level124
{
    // Authoring-only records. No invented electrical solver/gameplay API calls.
    public enum SparkL124TaskType { CopperRoute, Via }

    [Serializable]
    public sealed class SparkL124ComponentEntry
    {
        public string referenceId;
        public string assetId;
        public string kind;
        public Vector3 physicalLocationMetres;
        public bool footprintVerified;
        public bool installedAtStart;
    }

    [Serializable]
    public sealed class SparkL124RouteEntry
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
    public sealed class SparkL124ViaEntry
    {
        public string id;
        public string netId;
        public string fromLayer;
        public string toLayer;
        public bool verified;
    }

    [CreateAssetMenu(fileName = "L124_SourceBlueprint", menuName = "Project Spark/Levels/124 Source Blueprint")]
    public sealed class SparkL124SourceBlueprint : ScriptableObject
    {
        [SerializeField] private string boardId = "OBX-32";
        [SerializeField] private Vector2 physicalSizeMetres = new Vector2(.600f, .460f);
        [SerializeField] private int copperLayerCount = 40;
        [SerializeField] private bool schematicApproved = false;
        [SerializeField] private bool objectiveCertificationEnabled = false;
        [SerializeField] private string unresolvedSafetyLink = "W417: U5.TRUSTED_ATTITUDE_READY_OUT -> U3.TRUSTED_ATTITUDE_READY_IN (physical pins UNASSIGNED)";
        [SerializeField] private List<SparkL124ComponentEntry> components = new List<SparkL124ComponentEntry>();
        [SerializeField] private List<SparkL124RouteEntry> copperTasks = new List<SparkL124RouteEntry>();
        [SerializeField] private List<SparkL124ViaEntry> viaTasks = new List<SparkL124ViaEntry>();
        public string BoardId => boardId;
        public Vector2 PhysicalSizeMetres => physicalSizeMetres;
        public int CopperLayerCount => copperLayerCount;
        public bool SchematicApproved => schematicApproved;
        public bool ObjectiveCertificationEnabled => objectiveCertificationEnabled;
        public IReadOnlyList<SparkL124ComponentEntry> Components => components;
        public IReadOnlyList<SparkL124RouteEntry> CopperTasks => copperTasks;
        public IReadOnlyList<SparkL124ViaEntry> ViaTasks => viaTasks;
        public void SetSourceData(List<SparkL124ComponentEntry> partRecords,
            List<SparkL124RouteEntry> routeRecords, List<SparkL124ViaEntry> viaRecords)
        {
            components = partRecords ?? new List<SparkL124ComponentEntry>();
            copperTasks = routeRecords ?? new List<SparkL124RouteEntry>();
            viaTasks = viaRecords ?? new List<SparkL124ViaEntry>();
            schematicApproved = false;
            objectiveCertificationEnabled = false;
            unresolvedSafetyLink = "W417: U5.TRUSTED_ATTITUDE_READY_OUT -> U3.TRUSTED_ATTITUDE_READY_IN (physical pins UNASSIGNED)";
        }
    }

    [DisallowMultipleComponent]
    public sealed class SparkL124VisualOnly : MonoBehaviour
    {
        [TextArea(2, 4)] public string notice =
            "VISUAL ONLY / NOT CERTIFIABLE. Artwork cannot prove spacecraft avionics and actuator electrical continuity or safety, or game objective completion.";
        public bool IsAuthoritative => false;
    }

    [DisallowMultipleComponent]
    public sealed class SparkL124LayoutMarker : MonoBehaviour
    {
        public const int LevelNumber = 124;
        public bool IsAuthorityBound => false;
    }

    [DisallowMultipleComponent]
    public sealed class SparkL124PlacementSlot : MonoBehaviour
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
    public sealed class SparkL124BlockedTask : MonoBehaviour
    {
        [SerializeField] private SparkL124TaskType type;
        [SerializeField] private string taskId;
        [SerializeField] private string domain;
        [SerializeField] private string blocker = "UNASSIGNED net/pads or 40-layer via span";
        public string TaskId => taskId;
        public bool CanCertify => false;
        public void Configure(SparkL124TaskType taskType, string identifier, string taskDomain)
        {
            type = taskType;
            taskId = identifier;
            domain = taskDomain;
        }
    }

    [DisallowMultipleComponent]
    public sealed class SparkL124IntegrationAnchor : MonoBehaviour
    {
        [SerializeField] private string requiredProjectService;
        public string RequiredService => requiredProjectService;
        public bool IsBound => false;
        public void Configure(string service) { requiredProjectService = service; }
    }
}
