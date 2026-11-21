using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectSpark.Level120
{
    // Authoring-only records. No invented electrical solver/gameplay API calls.
    public enum SparkL120TaskType { CopperRoute, Via }

    [Serializable]
    public sealed class SparkL120ComponentEntry
    {
        public string referenceId;
        public string assetId;
        public string kind;
        public Vector3 physicalLocationMetres;
        public bool footprintVerified;
        public bool installedAtStart;
    }

    [Serializable]
    public sealed class SparkL120RouteEntry
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
    public sealed class SparkL120ViaEntry
    {
        public string id;
        public string netId;
        public string fromLayer;
        public string toLayer;
        public bool verified;
    }

    [CreateAssetMenu(fileName = "L120_SourceBlueprint", menuName = "Project Spark/Levels/120 Source Blueprint")]
    public sealed class SparkL120SourceBlueprint : ScriptableObject
    {
        [SerializeField] private string boardId = "RAX-28";
        [SerializeField] private Vector2 physicalSizeMetres = new Vector2(.520f, .380f);
        [SerializeField] private int copperLayerCount = 32;
        [SerializeField] private bool schematicApproved = false;
        [SerializeField] private bool objectiveCertificationEnabled = false;
        [SerializeField] private string unresolvedSafetyLink = "W353: U47.TRUSTED_SIX_AXIS_READY_OUT -> U48.TRUSTED_SIX_AXIS_READY_IN (physical pins UNASSIGNED)";
        [SerializeField] private List<SparkL120ComponentEntry> components = new List<SparkL120ComponentEntry>();
        [SerializeField] private List<SparkL120RouteEntry> copperTasks = new List<SparkL120RouteEntry>();
        [SerializeField] private List<SparkL120ViaEntry> viaTasks = new List<SparkL120ViaEntry>();
        public string BoardId => boardId;
        public Vector2 PhysicalSizeMetres => physicalSizeMetres;
        public int CopperLayerCount => copperLayerCount;
        public bool SchematicApproved => schematicApproved;
        public bool ObjectiveCertificationEnabled => objectiveCertificationEnabled;
        public IReadOnlyList<SparkL120ComponentEntry> Components => components;
        public IReadOnlyList<SparkL120RouteEntry> CopperTasks => copperTasks;
        public IReadOnlyList<SparkL120ViaEntry> ViaTasks => viaTasks;
        public void SetSourceData(List<SparkL120ComponentEntry> partRecords,
            List<SparkL120RouteEntry> routeRecords, List<SparkL120ViaEntry> viaRecords)
        {
            components = partRecords ?? new List<SparkL120ComponentEntry>();
            copperTasks = routeRecords ?? new List<SparkL120RouteEntry>();
            viaTasks = viaRecords ?? new List<SparkL120ViaEntry>();
            schematicApproved = false;
            objectiveCertificationEnabled = false;
            unresolvedSafetyLink = "W353: U47.TRUSTED_SIX_AXIS_READY_OUT -> U48.TRUSTED_SIX_AXIS_READY_IN (physical pins UNASSIGNED)";
        }
    }

    [DisallowMultipleComponent]
    public sealed class SparkL120VisualOnly : MonoBehaviour
    {
        [TextArea(2, 4)] public string notice =
            "VISUAL ONLY / NOT CERTIFIABLE. Artwork is not verified power-phase wiring, torque feedback, STO qualification or objective authority.";
        public bool IsAuthoritative => false;
    }

    [DisallowMultipleComponent]
    public sealed class SparkL120LayoutMarker : MonoBehaviour
    {
        public const int LevelNumber = 120;
        public bool IsAuthorityBound => false;
    }

    [DisallowMultipleComponent]
    public sealed class SparkL120PlacementSlot : MonoBehaviour
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
    public sealed class SparkL120BlockedTask : MonoBehaviour
    {
        [SerializeField] private SparkL120TaskType type;
        [SerializeField] private string taskId;
        [SerializeField] private string domain;
        [SerializeField] private string blocker = "UNASSIGNED net/pads or 32-layer via span";
        public string TaskId => taskId;
        public bool CanCertify => false;
        public void Configure(SparkL120TaskType taskType, string identifier, string taskDomain)
        {
            type = taskType;
            taskId = identifier;
            domain = taskDomain;
        }
    }

    [DisallowMultipleComponent]
    public sealed class SparkL120IntegrationAnchor : MonoBehaviour
    {
        [SerializeField] private string requiredProjectService;
        public string RequiredService => requiredProjectService;
        public bool IsBound => false;
        public void Configure(string service) { requiredProjectService = service; }
    }
}
