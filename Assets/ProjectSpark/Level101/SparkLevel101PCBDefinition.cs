using UnityEngine;
namespace ProjectSpark.Level101
{
    /// <summary>Text assets authored in CSV, assigned manually or by Editor importer.
    /// Source-of-truth planning data only; actual SparkTerminal and circuit graph must be built separately.</summary>
    [CreateAssetMenu(fileName="L94_PCBDefinition",menuName="Project Spark/Level101/PCB Planning Data")]
    public sealed class SparkLevel101PCBDefinition : ScriptableObject
    {
        public Vector2 physicalBoardSizeMm=new Vector2(100,70);
        public float physicalThicknessMm=1.6f;
        public int copperLayers=2;
        public float visualPresentationScale=14f;
        public TextAsset componentBOM;
        public TextAsset copperTraceTasks;
        public TextAsset viaTasks;
        public TextAsset logicalPadAnchors;
        public TextAsset factoryNetlist;
        public TextAsset w035CriticalJumper;
        public TextAsset objectiveRequirements;
        public TextAsset faultScenarios;
        public TextAsset testPlan;
    }
}

