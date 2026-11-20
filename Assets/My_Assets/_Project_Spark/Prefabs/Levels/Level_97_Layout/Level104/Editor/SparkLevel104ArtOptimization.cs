#if UNITY_EDITOR
using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEditor;using UnityEngine;
namespace ProjectSpark.EditorTools
{
 static class SparkLevel104ArtOptimization
 {
  internal static void Combine(string root,Dictionary<string,GameObject> modules)
  {
   string folder=root+"/Meshes";if(!AssetDatabase.IsValidFolder(folder))AssetDatabase.CreateFolder(root,"Meshes");
   var report=new List<string>{"Model,SourceTriangles,SourceRenderers,RenderedBatches"};
   AssetDatabase.StartAssetEditing(); try { foreach(var pair in modules)
   {
    var go=pair.Value;var original=go.GetComponentsInChildren<MeshRenderer>(true);int triangles=0,batches=0;
    foreach(var r in original){var f=r.GetComponent<MeshFilter>();if(f&&f.sharedMesh)triangles+=f.sharedMesh.triangles.Length/3;}
    foreach(var group in original.Where(r=>r.sharedMaterial&&r.GetComponent<MeshFilter>()).GroupBy(r=>r.sharedMaterial))
    {
     var meshes=group.Select(r=>new CombineInstance{mesh=r.GetComponent<MeshFilter>().sharedMesh,transform=go.transform.worldToLocalMatrix*r.transform.localToWorldMatrix}).ToArray();
     var combined=new Mesh{name=go.name+"_"+group.Key.name};combined.indexFormat=UnityEngine.Rendering.IndexFormat.UInt32;combined.CombineMeshes(meshes,true,true);
     string path=folder+"/"+combined.name+".asset";var previous=AssetDatabase.LoadAssetAtPath<Mesh>(path);
     if(previous){EditorUtility.CopySerialized(combined,previous);UnityEngine.Object.DestroyImmediate(combined);combined=previous;}else AssetDatabase.CreateAsset(combined,path);
     var node=new GameObject("Render batch / "+group.Key.name);node.transform.SetParent(go.transform,false);node.AddComponent<MeshFilter>().sharedMesh=combined;node.AddComponent<MeshRenderer>().sharedMaterial=group.Key;
     foreach(var r in group)r.enabled=false;batches++;
    }
    report.Add($"{go.name},{triangles},{original.Length},{batches}");
   }
   } finally {AssetDatabase.StopAssetEditing();} File.WriteAllLines(root+"/Reports/Imported_Render_Batches.csv",report);
  }
 }
}
#endif

