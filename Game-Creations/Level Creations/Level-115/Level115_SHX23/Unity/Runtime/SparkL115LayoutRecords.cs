using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectSpark.Level115
{
    // Authoring data only. These types deliberately do not duplicate SparkCircuitSystem
    // or call LevelGamePlayManager until its actual project API has been inspected.
    public enum SparkL115TaskType { CopperRoute, Via }

    [Serializable]
    public sealed class SparkL115ComponentEntry
    {
        public string referenceId;
        public string assetId;
        public string kind;
        public Vector3 physicalLocationMetres;
        public bool footprintVerified;
        public bool installedAtStart;
    }

    [Serializable]
    public sealed class SparkL115RouteEntry
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
    public sealed class SparkL115ViaEntry
    {
        public string id;
        public string netId;
        public string fromLayer;
        public string toLayer;
        public bool verified;
    }

    [CreateAssetMenu(fileName = "L115_SourceBlueprint", menuName = "Project Spark/Levels/115 Source Blueprint")]
    public sealed class SparkL115SourceBlueprint : ScriptableObject
    {
        [SerializeField] private string boardId = "SHX-23";
        [SerializeField] private Vector2 physicalSizeMetres = new Vector2(0.420f, 0.300f);
        [SerializeField] private int copperLayerCount = 22;
        [SerializeField] private bool schematicApproved;
        [SerializeField] private bool objectiveCertificationEnabled;
        [SerializeField] private string unresolvedSafetyLink = "W273 U7/U8 physical package pins UNASSIGNED";
        [SerializeField] private List<SparkL115ComponentEntry> components = new List<SparkL115ComponentEntry>();
        [SerializeField] private List<SparkL115RouteEntry> copperTasks = new List<SparkL115RouteEntry>();
        [SerializeField] private List<SparkL115ViaEntry> viaTasks = new List<SparkL115ViaEntry>();
        public string BoardId => boardId;
        public Vector2 PhysicalSizeMetres => physicalSizeMetres;
        public int CopperLayerCount => copperLayerCount;
        public bool SchematicApproved => schematicApproved;
        public bool ObjectiveCertificationEnabled => objectiveCertificationEnabled;
        public IReadOnlyList<SparkL115ComponentEntry> Components => components;
        public IReadOnlyList<SparkL115RouteEntry> CopperTasks => copperTasks;
        public IReadOnlyList<SparkL115ViaEntry> ViaTasks => viaTasks;

        public void SetSourceData(List<SparkL115ComponentEntry> partRecords,
            List<SparkL115RouteEntry> routeRecords, List<SparkL115ViaEntry> viaRecords)
        {
            components = partRecords ?? new List<SparkL115ComponentEntry>();
            copperTasks = routeRecords ?? new List<SparkL115RouteEntry>();
            viaTasks = viaRecords ?? new List<SparkL115ViaEntry>();
            // NEVER infer electrical approval from these manifest counts.
            schematicApproved = false;
            objectiveCertificationEnabled = false;
            unresolvedSafetyLink = "W273 U7/U8 physical package pins UNASSIGNED";
        }
    }

    [DisallowMultipleComponent]
    public sealed class SparkL115VisualOnly : MonoBehaviour
    {
        [TextArea(2, 4)] public string notice =
            "VISUAL ONLY / NOT CERTIFIABLE. Artwork is not solder, copper connectivity, real cryptographic verification, or gameplay evidence.";
        public bool IsAuthoritative => false;
    }

    [DisallowMultipleComponent]
    public sealed class SparkL115LayoutMarker : MonoBehaviour
    {
        public const int LevelNumber = 115;
        public bool IsAuthorityBound => false;
    }

    [DisallowMultipleComponent]
    public sealed class SparkL115PlacementSlot : MonoBehaviour
    {
        [SerializeField] private string componentRef;
        [SerializeField] private string assetId;
        [SerializeField] private string componentKind;
        [SerializeField] private bool physicalFootprintVerified;
        [SerializeField] private bool installed;
        public string ComponentRef => componentRef;
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
    public sealed class SparkL115BlockedTask : MonoBehaviour
    {
        [SerializeField] private SparkL115TaskType type;
        [SerializeField] private string taskId;
        [SerializeField] private string domain;
        [SerializeField] private string blocker = "UNASSIGNED electrical net/pads or layer span";
        public string TaskId => taskId;
        public bool CanCertify => false;
        public void Configure(SparkL115TaskType taskType, string identifier, string taskDomain)
        {
            type = taskType;
            taskId = identifier;
            domain = taskDomain;
        }
    }

    [DisallowMultipleComponent]
    public sealed class SparkL115IntegrationAnchor : MonoBehaviour
    {
        [SerializeField] private string requiredProjectService;
        public bool IsBound => false;
        public void Configure(string service) { requiredProjectService = service; }
    }
}
