#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using ProjectSpark.Electrical;

namespace ProjectSpark.EditorTools
{
    public static class SparkAdvancedSolverInstaller
    {
        [MenuItem("Tools/Project Spark/Electrical/Install Advanced Solver Definitions")]
        public static void Install()
        {
            string[] guids = AssetDatabase.FindAssets("t:SparkElectricalSolverRegistry");
            if (guids.Length != 1)
            {
                EditorUtility.DisplayDialog("Project Spark Solvers",
                    "Expected exactly one registry asset; found " + guids.Length +
                    ". Select your actual registry manually before proceeding. Nothing changed.", "OK");
                return;
            }
            string registryPath = AssetDatabase.GUIDToAssetPath(guids[0]);
            var registry = AssetDatabase.LoadAssetAtPath<SparkElectricalSolverRegistry>(registryPath);
            if (registry == null) return;
            var serialized = new SerializedObject(registry);
            var entries = serialized.FindProperty("definitions");
            if (entries == null || !entries.isArray)
            {
                Debug.LogError("[Advanced Solvers] Registry definitions field not found. Nothing changed.");
                return;
            }
            Type[] types = {
                typeof(SparkCapacitorSolverDefinition), typeof(SparkDiodeSolverDefinition),
                typeof(SparkLDRSolverDefinition), typeof(SparkTransistorSolverDefinition),
                typeof(SparkRelaySolverDefinition)
            };
            string directory = System.IO.Path.GetDirectoryName(registryPath).Replace('\\','/');
            if (!EditorUtility.DisplayDialog("Install Project Spark Solver Definitions",
                "Target registry: " + registryPath + "\n\nThis will preserve existing definitions, add missing definitions (" +
                string.Join(", ",types.Select(t=>t.Name)) + ") and create only missing assets beside the registry.",
                "Install", "Cancel")) return;
            Undo.RecordObject(registry, "Install Advanced Solver Definitions");
            foreach (var type in types)
            {
                bool present = false;
                for (int i=0;i<entries.arraySize;i++)
                {
                    var existing = entries.GetArrayElementAtIndex(i).objectReferenceValue as SparkElectricalSolverDefinition;
                    if (existing != null && existing.GetType()==type) { present = true; break; }
                }
                if (present) continue;
                string[] candidates = AssetDatabase.FindAssets("t:"+type.Name);
                SparkElectricalSolverDefinition definition = null;
                if (candidates.Length==1)
                    definition = AssetDatabase.LoadAssetAtPath<SparkElectricalSolverDefinition>(AssetDatabase.GUIDToAssetPath(candidates[0]));
                else if (candidates.Length>1)
                {
                    Debug.LogError("[Advanced Solvers] Multiple assets for " + type.Name + ". Choose one manually; skipping.");
                    continue;
                }
                if (definition==null)
                {
                    definition = ScriptableObject.CreateInstance(type) as SparkElectricalSolverDefinition;
                    string path = AssetDatabase.GenerateUniqueAssetPath(directory+"/"+type.Name+".asset");
                    AssetDatabase.CreateAsset(definition,path);
                }
                entries.InsertArrayElementAtIndex(entries.arraySize);
                entries.GetArrayElementAtIndex(entries.arraySize-1).objectReferenceValue=definition;
                Debug.Log("[Advanced Solvers] Registered " + type.Name);
            }
            serialized.ApplyModifiedProperties();
            EditorUtility.SetDirty(registry);
            AssetDatabase.SaveAssets();
            Selection.activeObject=registry;
            EditorGUIUtility.PingObject(registry);
            Debug.Log("[Advanced Solvers] Review solverRegistry reference in the active SparkElectricalSolver, then test each device in Play Mode.");
        }
    }
}
#endif
