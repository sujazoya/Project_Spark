using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectSpark.Level126
{
    [CreateAssetMenu(fileName = "L126_SourceBlueprint", menuName = "Project Spark/Levels/126 Source Blueprint")]
    public sealed class SparkL126SourceBlueprint : ScriptableObject
    {
        [SerializeField] private string boardId = "EPX-34";
        [SerializeField] private Vector2 physicalSizeMetres = new Vector2(.640f, .500f);
        [SerializeField] private int copperLayerCount = 44;
        [SerializeField] private bool schematicApproved = false;
        [SerializeField] private bool objectiveCertificationEnabled = false;
        [SerializeField] private string unresolvedSafetyLink = "W449: U6.TRUSTED_ENERGY_READY_OUT -> U3.TRUSTED_ENERGY_READY_IN (physical pins UNASSIGNED)";
        [SerializeField] private List<SparkL126ComponentEntry> components = new List<SparkL126ComponentEntry>();
        [SerializeField] private List<SparkL126RouteEntry> copperTasks = new List<SparkL126RouteEntry>();
        [SerializeField] private List<SparkL126ViaEntry> viaTasks = new List<SparkL126ViaEntry>();
        public string BoardId => boardId;
        public Vector2 PhysicalSizeMetres => physicalSizeMetres;
        public int CopperLayerCount => copperLayerCount;
        public bool SchematicApproved => schematicApproved;
        public bool ObjectiveCertificationEnabled => objectiveCertificationEnabled;
        public IReadOnlyList<SparkL126ComponentEntry> Components => components;
        public IReadOnlyList<SparkL126RouteEntry> CopperTasks => copperTasks;
        public IReadOnlyList<SparkL126ViaEntry> ViaTasks => viaTasks;
        public void SetSourceData(List<SparkL126ComponentEntry> partRecords,
            List<SparkL126RouteEntry> routeRecords, List<SparkL126ViaEntry> viaRecords)
        {
            components = partRecords ?? new List<SparkL126ComponentEntry>();
            copperTasks = routeRecords ?? new List<SparkL126RouteEntry>();
            viaTasks = viaRecords ?? new List<SparkL126ViaEntry>();
            schematicApproved = false;
            objectiveCertificationEnabled = false;
            unresolvedSafetyLink = "W449: U6.TRUSTED_ENERGY_READY_OUT -> U3.TRUSTED_ENERGY_READY_IN (physical pins UNASSIGNED)";
        }
    }
}
