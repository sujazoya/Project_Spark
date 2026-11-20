using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectSpark.Level101
{
    [Serializable] public sealed class SparkL101ComponentRecord
    {
        public string referenceDesignator, kind, variant, sourceFbx;
        public Vector3 physicalPositionMeters;
        public bool startsInstalled, hasApprovedElectricalFootprint;
    }
    [Serializable] public sealed class SparkL101CopperTaskRecord
    {
        public string id, subsystem, sourcePad, destinationPad, netId;
        public bool startsComplete, certifiable;
    }
    [Serializable] public sealed class SparkL101ViaTaskRecord
    {
        public string id, netId, fromLayer, toLayer;
        public Vector3 physicalPositionMeters;
        public bool startsInstalled, certifiable;
    }
    [CreateAssetMenu(fileName="L101_SourceBlueprint",menuName="Project Spark/Level 101/Source Blueprint (Unverified)")]
    public sealed class SparkL101SourceBlueprint : ScriptableObject
    {
        public const string UnverifiedStatus="BLOCKED_UNVERIFIED_NETLIST_AND_W101_ROLE_CONFLICT";
        public int level=101;
        public string boardId="MLX-9";
        public string levelTitle="The Hidden Layers";
        public Vector2 physicalBoardMeters=new Vector2(.230f,.160f);
        public int copperLayers=6;
        public string electricalStatus=UnverifiedStatus;
        public string safetyJumper="W101";
        [TextArea(2,3)]public string safetyPinConflict="Source W101 designates U10 link monitor and U11 safety, but BOM lists U10=LDO, U11=INTERFACE, U7=LINK_MON, U8=SAFETY. Canonical pin owners must be approved.";
        public string sourceTrustedOutput="L101_LinkIntegrityMonitor_U10.TRUSTED_LINK_HEALTH_OUT";
        public string sourceTrustedInput="L101_MultilayerSafetySupervisor_U11.TRUSTED_LINK_HEALTH_IN";
        public int certifiedObjectives=0;
        public List<SparkL101ComponentRecord> components=new List<SparkL101ComponentRecord>();
        public List<SparkL101CopperTaskRecord> routes=new List<SparkL101CopperTaskRecord>();
        public List<SparkL101ViaTaskRecord> vias=new List<SparkL101ViaTaskRecord>();
    }
    [DisallowMultipleComponent]
    public sealed class SparkL101FootprintArtTarget : MonoBehaviour
    {
        [Tooltip("Visual footprint only, not a solder connection or an approved electrical pin map.")]
        public string referenceDesignator, modelFbx;
        public Vector3 originalPhysicalPositionMeters;
        public GameObject inventoryVisualPrefab;
        public bool startsInstalled=false;
        public bool hasApprovedPinMapping=false;
    }
    [DisallowMultipleComponent]
    public sealed class SparkL101UnverifiedPlanningTask : MonoBehaviour
    {
        public string sourceId,category,subsystem;
        [TextArea(2,3)]public string reason=SparkL101SourceBlueprint.UnverifiedStatus;
        public bool certifiable=false;
    }
    [DisallowMultipleComponent]
    public sealed class SparkL101PreviewArtOnly : MonoBehaviour
    {
        [TextArea(2,4)] public string warning="REFERENCE SHOWCASE ONLY. Enlarged visual board with colliders stripped. No electrical or game-play authority.";
        public float displayScale;
    }
}
