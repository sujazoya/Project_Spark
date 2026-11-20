#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.Level102.Editor
{
    /// <summary>Compatibility menu, using the checked importer and no duplicate gameplay authority.</summary>
    public static class SparkL102BoardBuilder
    {
        [MenuItem("Tools/Project Spark/Levels/102/1. Build EMPTY HSX-10 Board")]
        public static void BuildEmpty()
        {
            SparkL102FullLayoutBuilder.BuildFullLayout();
        }
        [MenuItem("Tools/Project Spark/Levels/102/2. Validate Imported Models")]
        public static void ValidateModels()
        {
            SparkL102FullLayoutBuilder.ValidateImported();
        }
    }
}
#endif
