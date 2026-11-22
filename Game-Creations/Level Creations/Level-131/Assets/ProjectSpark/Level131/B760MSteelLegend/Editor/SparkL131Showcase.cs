using UnityEditor;
using UnityEngine;
using System.IO;
namespace ProjectSpark.Level131.Editor {
 public static class SparkL131Showcase {
  const string Root="Assets/ProjectSpark/Level131/B760MSteelLegend";
  [MenuItem("Tools/Project Spark/Automatic Level Generator/131/3. Create assembled preview prefab")]
  static void Assembled()=>Create("ASSEMBLED");
  [MenuItem("Tools/Project Spark/Automatic Level Generator/131/4. Create batched preview prefab")]
  static void Batched()=>Create("BATCHED");
  static void Create(string id) { var source=AssetDatabase.LoadAssetAtPath<GameObject>(Root+"/Models_Native/L131_"+id+".spark131mesh"); if(!source) throw new InvalidDataException("Reimport native meshes first"); var go=(GameObject)PrefabUtility.InstantiatePrefab(source); try { SparkL131MaterialTools.ApplyTo(go,true); if(!AssetDatabase.IsValidFolder(Root+"/Generated")) AssetDatabase.CreateFolder(Root,"Generated"); PrefabUtility.SaveAsPrefabAsset(go,Root+"/Generated/L131_"+id+".prefab"); } finally { Object.DestroyImmediate(go); } }
 }
}
