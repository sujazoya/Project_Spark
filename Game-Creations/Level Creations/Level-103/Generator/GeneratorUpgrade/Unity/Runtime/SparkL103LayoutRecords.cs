using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectSpark.Level103
{
    [Serializable]
    public sealed class SparkL103ComponentRecord
    {
        public string referenceDesignator;
        public string kind;
        public string description;
        public string sourceFbx;
        public Vector3 physicalPositionMeters;
        public float yawDegrees;
        public bool startsInstalled;
    }

    [Serializable]
    public sealed class SparkL103CopperTaskRecord
    {
        public string id;
        public string subsystem;
        public string netId;
        public string sourcePin;
        public string destinationPin;
        public string layerSpan;
        public bool certifiable;
    }

    [Serializable]
    public sealed class SparkL103ViaTaskRecord
    {
        public string id;
        public string subsystem;
        public string netId;
        public string sourcePin;
        public string destinationPin;
        public string layerSpan;
        public bool certifiable;
    }

    /// <summary>
    /// IMPORT/ART metadata only. An approved schematic, actual project circuit solver and
    /// LevelGamePlayManager evaluator MUST take ownership before any live certification.
    /// </summary>
    [CreateAssetMenu(fileName = "L103_SourceBlueprint", menuName = "Project Spark/Level 103/Source Blueprint (Unverified)")]
    public sealed class SparkL103SourceBlueprint : ScriptableObject
    {
        public const string UnverifiedStatus = "BLOCKED_UNVERIFIED_NETLIST";
        public int level = 103;
        public string boardId = "CPX-11";
        public string levelTitle = "The Power Core";
        public Vector2 physicalBoardMeters = new Vector2(0.240f, 0.180f);
        public int copperLayers = 8;
        public string electricalStatus = UnverifiedStatus;
        public string safetyJumper = "W117";
        public string trustedOutput = "L103_VcoreSupervisor_U10.TRUSTED_VCORE_GOOD_OUT";
        public string trustedInput = "L103_CPUSafetySupervisor_U11.TRUSTED_VCORE_GOOD_IN";
        public int certifiedObjectives = 0;
        public List<SparkL103ComponentRecord> components = new List<SparkL103ComponentRecord>();
        public List<SparkL103CopperTaskRecord> routes = new List<SparkL103CopperTaskRecord>();
        public List<SparkL103ViaTaskRecord> vias = new List<SparkL103ViaTaskRecord>();
    }

    [DisallowMultipleComponent]
    public sealed class SparkL103FootprintArtTarget : MonoBehaviour
    {
        [Tooltip("This marks a physical art/placement coordinate. It is NOT a soldered part or approved pin map.")]
        public string referenceDesignator;
        public string modelFbx;
        public Vector3 originalPhysicalPositionMeters;
        public GameObject inventoryVisualPrefab;
        public bool startsInstalled = false;
        public bool hasApprovedPinMapping = false;
    }

    [DisallowMultipleComponent]
    public sealed class SparkL103UnverifiedPlanningTask : MonoBehaviour
    {
        public string sourceId;
        public string category;
        public string subsystem;
        [TextArea(2,3)] public string reason = SparkL103SourceBlueprint.UnverifiedStatus;
        public bool certifiable = false;
    }

    [DisallowMultipleComponent]
    public sealed class SparkL103PreviewArtOnly : MonoBehaviour
    {
        [TextArea(2,4)]
        public string warning = "DISPLAY ONLY. Scaled completed-board art; no colliders, electrical graph, terminals, installation, objective progress or gameplay authority.";
        public float displayScale;
    }
}
