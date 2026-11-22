#if UNITY_EDITOR
using System;using System.IO;using System.Linq;using UnityEditor;using UnityEngine;
[InitializeOnLoad]static class SparkLevel104ImportScan{
 static SparkLevel104ImportScan(){EditorApplication.update+=Poll;}
 static void Poll(){if(!File.Exists("Logs/SparkLevel104Import.request")||EditorApplication.isCompiling||EditorApplication.isUpdating||EditorApplication.isPlayingOrWillChangePlaymode)return;File.Delete("Logs/SparkLevel104Import.request");var lines=new System.Collections.Generic.List<string>();foreach(var path in Directory.GetFiles("Assets/ProjectSpark/Level104/Models_Rebuilt","*.fbx")){var model=AssetDatabase.LoadAssetAtPath<GameObject>(path);if(!model){lines.Add(path+" MISSING");continue;}var go=(GameObject)PrefabUtility.InstantiatePrefab(model);try{var rs=go.GetComponentsInChildren<Renderer>(true);var b=rs.Length>0?rs[0].bounds:new Bounds();foreach(var r in rs)b.Encapsulate(r.bounds);lines.Add(Path.GetFileName(path)+" renderers="+rs.Length+" rotation="+go.transform.eulerAngles+" scale="+go.transform.localScale+" bounds="+b+" materials="+string.Join("|",rs.SelectMany(r=>r.sharedMaterials).Where(m=>m).Select(m=>m.name).Distinct()));}finally{UnityEngine.Object.DestroyImmediate(go);}}File.WriteAllLines("Logs/SparkLevel104Import.txt",lines);}
}
#endif

