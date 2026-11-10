#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using ProjectSpark.Gameplay;

namespace ProjectSpark.EditorTools
{
    public static class SparkLevel29Setup
    {
        [MenuItem("Tools/Project Spark/Level 29/Prepare Selected Layout Gates")]
        private static void Prepare()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode) { Debug.LogError("Exit Play Mode first."); return; }
            var go=Selection.activeGameObject;
            var layout=go!=null ? go.GetComponent<SparkLevelLayout>() : null;
            if(layout==null || layout.LevelDefinition==null || layout.LevelDefinition.LevelId!="level_29")
            { Debug.LogError("Select the Level_29_Layout ROOT in Prefab Mode, assigned to Level_29.asset."); return; }
            var bindings=go.GetComponent<SparkLevelSceneBindings>();
            if(bindings==null) { Debug.LogError("The selected layout root needs SparkLevelSceneBindings."); return; }
            Undo.RecordObject(bindings,"Declare Level 29 runtime bindings");
            var so=new SerializedObject(bindings);
            var ids=so.FindProperty("runtimeComponentIds");
            if(ids==null || !ids.isArray) { Debug.LogError("Binding schema changed: runtimeComponentIds not found."); return; }
            foreach(var id in SparkLevel29AutomaticBackup.Gates) Add(ids,id);
            foreach(var id in new[]{"D_MAIN_Component","D_CHARGE_Component","R_CHARGE_Component","C_STORE_Component","R_LED_Component","D_BACKUP_Component"}) Add(ids,id);
            var terminals=so.FindProperty("runtimeTerminalIds");
            if(terminals==null || !terminals.isArray) { Debug.LogError("Binding schema changed: runtimeTerminalIds not found."); return; }
            foreach(var id in new[]{"D_MAIN_Anode","D_MAIN_Cathode","D_CHARGE_Anode","D_CHARGE_Cathode","R_CHARGE_Input","R_CHARGE_Output","C_STORE_Positive","C_STORE_Negative","R_LED_Input","R_LED_Output","D_BACKUP_Anode","D_BACKUP_Cathode"}) Add(terminals,id);
            so.ApplyModifiedProperties();
            if(go.GetComponent<SparkLevel29AutomaticBackup>()==null) Undo.AddComponent<SparkLevel29AutomaticBackup>(go);
            EditorUtility.SetDirty(bindings);
            Debug.Log("[Level 29] Declared bindings and added observer. Save the prefab. Configure slots, part values and wiring using the guide; this tool does not build or wire the circuit.",go);
        }
        private static void Add(SerializedProperty array,string id)
        {
            for(int i=0;i<array.arraySize;i++) if(array.GetArrayElementAtIndex(i).stringValue==id) return;
            int n=array.arraySize; array.InsertArrayElementAtIndex(n); array.GetArrayElementAtIndex(n).stringValue=id;
        }
    }
}
#endif
