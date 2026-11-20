using System;
using UnityEngine;

namespace ProjectSpark.Level111
{
    // Metadata-only components. They NEVER grant electrical connectivity or objectives.
    [DisallowMultipleComponent]
    public sealed class SparkL111LayoutMarker : MonoBehaviour
    {
        [SerializeField] private int levelNumber = 111;
        [SerializeField] private string levelCode = "SIX-19";
        [SerializeField, TextArea] private string certification =
            "SCHEMATIC_REQUIRED - no approved board graph; no gameplay objective authority.";
        public int LevelNumber { get { return levelNumber; } }
        public string LevelCode { get { return levelCode; } }
        public bool ElectricallyCertified { get { return false; } }
    }

    [DisallowMultipleComponent]
    public sealed class SparkL111PlacementSlot : MonoBehaviour
    {
        [SerializeField] private string partReference;
        [SerializeField] private string partKind;
        [SerializeField] private string sourceAssetId;
        [SerializeField] private bool footprintApproved;
        [SerializeField] private bool installed;
        public string PartReference { get { return partReference; } }
        public bool FootprintApproved { get { return footprintApproved; } }
        public bool Installed { get { return installed; } }
        public void Configure(string reference, string kind, string assetId)
        {
            partReference = reference;
            partKind = kind;
            sourceAssetId = assetId;
            footprintApproved = false;
            installed = false;
        }
        // No public completion setter by design. A future adapter must use the actual solver and solder authority.
    }

    public enum SparkL111TaskType { CopperRoute, Via }

    [DisallowMultipleComponent]
    public sealed class SparkL111BlockedTask : MonoBehaviour
    {
        [SerializeField] private SparkL111TaskType taskType;
        [SerializeField] private string taskId;
        [SerializeField] private string domain;
        [SerializeField] private string blockedReason = "UNASSIGNED_NET_AND_PINS";
        [SerializeField] private bool certifiable;
        public bool Certifiable { get { return false; } }
        public string TaskId { get { return taskId; } }
        public void Configure(SparkL111TaskType type, string id, string group)
        {
            taskType = type;
            taskId = id;
            domain = group;
            blockedReason = "SCHEMATIC_REQUIRED";
            certifiable = false;
        }
    }

    [DisallowMultipleComponent]
    public sealed class SparkL111VisualOnly : MonoBehaviour
    {
        [SerializeField] private bool referenceOnly = true;
        [SerializeField, TextArea] private string notice =
            "Art only - cannot install parts, connect pins, energize circuits or award objectives.";
        public bool IsReferenceOnly { get { return referenceOnly; } }
    }

    [DisallowMultipleComponent]
    public sealed class SparkL111IntegrationAnchor : MonoBehaviour
    {
        [SerializeField] private string requiredService;
        [SerializeField] private bool serviceBound;
        [SerializeField, TextArea] private string notice = "UNBOUND: inspect actual Project Spark API.";
        public bool IsBound { get { return serviceBound; } }
        public void Configure(string serviceName)
        {
            requiredService = serviceName;
            serviceBound = false;
            notice = "UNBOUND: requires verified project service contract and approved netlist.";
        }
    }
}
