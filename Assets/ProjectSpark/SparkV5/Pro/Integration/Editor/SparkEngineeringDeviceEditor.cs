#if UNITY_EDITOR
using System;
using System.IO;
using ProjectSpark.Electrical;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using ProjectSpark.Gameplay;

namespace ProjectSpark.SparkV5.Pro.Editor
{
    public static class SparkEngineeringDeviceEditor
    {
        private const string Menu = "Project Spark/Spark V5 Pro/AAA Laboratory/";
        [MenuItem(Menu + "33 - Bind Physical Device Operator v16 (Prefabs + Scenes)")]
        private static void Bind()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            { EditorUtility.DisplayDialog("Spark Devices", "Exit Play Mode before editing prefabs and scenes.", "OK"); return; }
            if (!EditorUtility.DisplayDialog("Spark Device Operator v16",
                "Bind the safe v16 operator and its panel to SparkV5Pro prefabs and loaded scenes?\n" +
                "Prefab files will be backed up. Device callbacks are NEVER auto-bound.", "Bind", "Cancel")) return;
            string backup = Path.Combine(Path.GetDirectoryName(Application.dataPath), "../AI/SparkEngineering_Backups",
                "DEVICES_v16_" + DateTime.Now.ToString("yyyyMMdd_HHmmss_fff"));
            Directory.CreateDirectory(backup);
            int prefabs = 0, scenes = 0;
            foreach (string guid in AssetDatabase.FindAssets("t:Prefab SparkV5Pro"))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (!path.EndsWith("SparkV5Pro.prefab", StringComparison.OrdinalIgnoreCase)) continue;
                GameObject root = null;
                try
                {
                    root = PrefabUtility.LoadPrefabContents(path);
                    bool dirty = false;
                    foreach (var executive in root.GetComponentsInChildren<SparkProExecutive>(true))
                    {
                        if (!executive.GetComponent<SparkEngineeringControlHub>())
                        { Debug.LogWarning("[Spark Devices v16] v14 hub missing in " + path); continue; }
                        if (!executive.GetComponent<SparkEngineeringDeviceOperator>())
                        { executive.gameObject.AddComponent<SparkEngineeringDeviceOperator>(); dirty = true; }
                        if (!executive.GetComponent<SparkEngineeringDevicePanel>())
                        { executive.gameObject.AddComponent<SparkEngineeringDevicePanel>(); dirty = true; }
                    }
                    if (dirty)
                    {
                        string file = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(Application.dataPath),path));
                        File.Copy(file,Path.Combine(backup,path.Replace('/','_')),false);
                        PrefabUtility.SaveAsPrefabAsset(root,path);
                        prefabs++;
                    }
                }
                catch (Exception ex) { Debug.LogError("[Spark Devices v16] Could not bind prefab " + path + ": " + ex); }
                finally { if (root) PrefabUtility.UnloadPrefabContents(root); }
            }
            foreach (var executive in UnityEngine.Object.FindObjectsByType<SparkProExecutive>(FindObjectsInactive.Include,FindObjectsSortMode.None))
            {
                if (!executive || !executive.gameObject.scene.IsValid() ||
                    !executive.gameObject.scene.isLoaded || EditorUtility.IsPersistent(executive)) continue;
                if (!executive.GetComponent<SparkEngineeringControlHub>()) continue;
                bool dirty = false;
                if (!executive.GetComponent<SparkEngineeringDeviceOperator>())
                { Undo.AddComponent<SparkEngineeringDeviceOperator>(executive.gameObject); dirty = true; }
                if (!executive.GetComponent<SparkEngineeringDevicePanel>())
                { Undo.AddComponent<SparkEngineeringDevicePanel>(executive.gameObject); dirty = true; }
                if (dirty) { EditorSceneManager.MarkSceneDirty(executive.gameObject.scene); scenes++; }
            }
            AssetDatabase.SaveAssets();
            Debug.Log("[Spark Devices v16] Bound prefabs=" + prefabs + " scene instances=" + scenes + ". Backup " + backup + ". Save scene changes manually.");
        }
        [MenuItem(Menu + "34 - Add Native Device Port to Selected Device")]
        private static void CreateSelectedPort()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) { Debug.LogWarning("Exit Play Mode first."); return; }
            GameObject go = Selection.activeGameObject;
            if (!go || EditorUtility.IsPersistent(go))
            { Debug.LogWarning("Select the actual scene-level switch or power supply GameObject (not a prefab asset)."); return; }
            var port = go.GetComponent<SparkEngineeringDevicePort>();
            if (!port) port = Undo.AddComponent<SparkEngineeringDevicePort>(go);
            SparkPowerSupply power = go.GetComponent<SparkPowerSupply>();
            var control = go.GetComponent<SparkSwitch>();
            Undo.RecordObject(port, "Configure typed Spark device control");
            if (power) port.EditorInitializePower(power);
            else if (control) port.EditorInitializeSwitch(control);
            EditorUtility.SetDirty(port);
            EditorSceneManager.MarkSceneDirty(go.scene);
            Selection.activeGameObject = go;
            Debug.Log("[Spark Devices v16] Device port added to " + go.name +
                ". Typed power and switch ports use their native gameplay methods. Other devices need configured callbacks.");
        }
        [MenuItem(Menu + "35 - Diagnose Physical Device Operator v16 (Read-Only)")]
        private static void Diagnose()
        {
            var ops = UnityEngine.Object.FindObjectsByType<SparkEngineeringDeviceOperator>(FindObjectsInactive.Include,FindObjectsSortMode.None);
            var ports = UnityEngine.Object.FindObjectsByType<SparkEngineeringDevicePort>(FindObjectsInactive.Include,FindObjectsSortMode.None);
            var sb = new System.Text.StringBuilder("SPARK DEVICES v16 DIAGNOSTIC — READ ONLY\n");
            sb.Append("PlayMode=").Append(EditorApplication.isPlaying).Append(" operators=").Append(ops.Length)
                .Append(" devicePorts=").Append(ports.Length).AppendLine();
            foreach (var op in ops)
                sb.Append("Operator ").Append(op.name).Append(" busy=").Append(op.IsBusy)
                    .Append(" message=").Append(op.LastStatus).AppendLine();
            foreach (var port in ports)
            {
                if (!port) continue;
                bool ok = port.TryRead(out bool on, out string evidence);
                sb.Append("Port '").Append(port.DeviceId).Append("' ").Append(port.Kind)
                    .Append(" readable=").Append(ok).Append(" on=").Append(on)
                    .Append(" ON callback=").Append(port.HasNativeAction(true))
                    .Append(" OFF callback=").Append(port.HasNativeAction(false))
                    .Append(" evidence=").Append(evidence).AppendLine();
            }
            string output = Path.Combine(Path.GetDirectoryName(Application.dataPath),
                "Logs/SparkEngineering/DeviceOperatorV16_Diagnostic.txt");
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            File.WriteAllText(output,sb.ToString());
            Debug.Log("[Spark Devices v16] Diagnostic: " + output + "\n" + sb);
        }
    }
}
#endif
