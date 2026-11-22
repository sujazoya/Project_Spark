using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectSpark.Level127
{
    [CreateAssetMenu(fileName = "L127_SourceBlueprint", menuName = "Project Spark/Levels/127 Source Blueprint")]
    public sealed class SparkL127SourceBlueprint : ScriptableObject
    {
        [SerializeField] private string boardId = "TCX-35";
        [SerializeField] private Vector2 physicalSizeMetres = new Vector2(.660f, .520f);
        [SerializeField] private int copperLayerCount = 46;
        [SerializeField] private bool schematicApproved = false;
        [SerializeField] private bool objectiveCertificationEnabled = false;
        [SerializeField] private string unresolvedSafetyLink = "W465: U6.TRUSTED_THERMAL_READY_OUT -> U3.TRUSTED_THERMAL_READY_IN (physical pins UNASSIGNED)";
        [SerializeField] private List<SparkL127ComponentEntry> components = new List<SparkL127ComponentEntry>();
        [SerializeField] private List<SparkL127RouteEntry> copperTasks = new List<SparkL127RouteEntry>();
        [SerializeField] private List<SparkL127ViaEntry> viaTasks = new List<SparkL127ViaEntry>();
        public string BoardId => boardId;
        public Vector2 PhysicalSizeMetres => physicalSizeMetres;
        public int CopperLayerCount => copperLayerCount;
        public bool SchematicApproved => schematicApproved;
        public bool ObjectiveCertificationEnabled => objectiveCertificationEnabled;
        public IReadOnlyList<SparkL127ComponentEntry> Components => components;
        public IReadOnlyList<SparkL127RouteEntry> CopperTasks => copperTasks;
        public IReadOnlyList<SparkL127ViaEntry> ViaTasks => viaTasks;
        public void SetSourceData(List<SparkL127ComponentEntry> partRecords,
            List<SparkL127RouteEntry> routeRecords, List<SparkL127ViaEntry> viaRecords)
        {
            components = partRecords ?? new List<SparkL127ComponentEntry>();
            copperTasks = routeRecords ?? new List<SparkL127RouteEntry>();
            viaTasks = viaRecords ?? new List<SparkL127ViaEntry>();
            schematicApproved = false;
            objectiveCertificationEnabled = false;
            unresolvedSafetyLink = "W465: U6.TRUSTED_THERMAL_READY_OUT -> U3.TRUSTED_THERMAL_READY_IN (physical pins UNASSIGNED)";
        }
    }
}
