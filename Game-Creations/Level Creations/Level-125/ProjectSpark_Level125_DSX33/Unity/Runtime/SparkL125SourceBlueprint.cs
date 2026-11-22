using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectSpark.Level125
{
    [CreateAssetMenu(fileName = "L125_SourceBlueprint", menuName = "Project Spark/Levels/125 Source Blueprint")]
    public sealed class SparkL125SourceBlueprint : ScriptableObject
    {
        [SerializeField] private string boardId = "DSX-33";
        [SerializeField] private Vector2 physicalSizeMetres = new Vector2(.620f, .480f);
        [SerializeField] private int copperLayerCount = 42;
        [SerializeField] private bool schematicApproved = false;
        [SerializeField] private bool objectiveCertificationEnabled = false;
        [SerializeField] private string unresolvedSafetyLink = "W433: U5.TRUSTED_TX_READY_OUT -> U3.TRUSTED_TX_READY_IN (physical pins UNASSIGNED)";
        [SerializeField] private List<SparkL125ComponentEntry> components = new List<SparkL125ComponentEntry>();
        [SerializeField] private List<SparkL125RouteEntry> copperTasks = new List<SparkL125RouteEntry>();
        [SerializeField] private List<SparkL125ViaEntry> viaTasks = new List<SparkL125ViaEntry>();
        public string BoardId => boardId;
        public Vector2 PhysicalSizeMetres => physicalSizeMetres;
        public int CopperLayerCount => copperLayerCount;
        public bool SchematicApproved => schematicApproved;
        public bool ObjectiveCertificationEnabled => objectiveCertificationEnabled;
        public IReadOnlyList<SparkL125ComponentEntry> Components => components;
        public IReadOnlyList<SparkL125RouteEntry> CopperTasks => copperTasks;
        public IReadOnlyList<SparkL125ViaEntry> ViaTasks => viaTasks;
        public void SetSourceData(List<SparkL125ComponentEntry> partRecords,
            List<SparkL125RouteEntry> routeRecords, List<SparkL125ViaEntry> viaRecords)
        {
            components = partRecords ?? new List<SparkL125ComponentEntry>();
            copperTasks = routeRecords ?? new List<SparkL125RouteEntry>();
            viaTasks = viaRecords ?? new List<SparkL125ViaEntry>();
            schematicApproved = false;
            objectiveCertificationEnabled = false;
            unresolvedSafetyLink = "W433: U5.TRUSTED_TX_READY_OUT -> U3.TRUSTED_TX_READY_IN (physical pins UNASSIGNED)";
        }
    }
}
