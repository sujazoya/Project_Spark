using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectSpark.Level102
{
    // Source planning data. These records are NOT electrical authority and cannot award objectives.
    [Serializable]
    public sealed class SparkL102PartDefinition
    {
        public string id;
        public string footprint;
        public string visualModelName;
        public Vector3 physicalPositionMeters;
        public bool startsInstalled;
    }

    [Serializable]
    public sealed class SparkL102CopperDefinition
    {
        public string routeId;
        public string subsystem;
        public string sourcePad;
        public string destinationPad;
        public string netId;
        public string layer;
        public bool certifiable;
    }

    [Serializable]
    public sealed class SparkL102ViaDefinition
    {
        public string viaId;
        public string netId;
        public string sourceLayer;
        public string destinationLayer;
        public string manufacturingType;
        public bool certifiable;
    }

    [CreateAssetMenu(fileName = "L102_SourceBlueprint", menuName = "Project Spark/Level 102/Source Blueprint")]
    public sealed class SparkL102SourceBlueprint : ScriptableObject
    {
        public const string UnverifiedStatus = "BLOCKED_UNVERIFIED_NETLIST";
        public int levelNumber = 102;
        public string boardCode = "HSX-10";
        public string levelTitle = "Signal Phantom";
        public Vector2 boardPhysicalMeters = new Vector2(0.250f, 0.175f);
        public int nominalCopperLayers = 6;
        public string validationStatus = UnverifiedStatus;
        public string criticalJumper = "W109";
        public string criticalOutput = "L102_LinkQualityMonitor_U9.TRUSTED_LINK_QUALITY_OUT";
        public string criticalInput = "L102_HighSpeedSafetySupervisor_U11.TRUSTED_LINK_QUALITY_IN";
        public List<SparkL102PartDefinition> parts = new List<SparkL102PartDefinition>();
        public List<SparkL102CopperDefinition> routes = new List<SparkL102CopperDefinition>();
        public List<SparkL102ViaDefinition> vias = new List<SparkL102ViaDefinition>();
    }

    [DisallowMultipleComponent]
    public sealed class SparkL102PlacementTarget : MonoBehaviour
    {
        [Tooltip("Source component ID. Not an installed electrical component.")]
        public string referenceDesignator;
        public string footprint;
        public string sourceFbx;
        public Vector3 physicalLocationMeters;
        public GameObject visualPrefab;
        public bool startsInstalled;
    }

    [DisallowMultipleComponent]
    public sealed class SparkL102UnverifiedTask : MonoBehaviour
    {
        public string taskId;
        public string taskKind;
        public string sourceGroup;
        [TextArea] public string blockReason = SparkL102SourceBlueprint.UnverifiedStatus;
        public bool certifiable;
    }

    [DisallowMultipleComponent]
    public sealed class SparkL102TabletopPreview : MonoBehaviour
    {
        [TextArea(2, 4)]
        public string warning = "VISUAL-ONLY enlarged tabletop preview. Do not use this transform scale for electrical simulation, hit tolerance, routing, or objective certification.";
        public float displayUniformScale = 40.52f;
    }
}
