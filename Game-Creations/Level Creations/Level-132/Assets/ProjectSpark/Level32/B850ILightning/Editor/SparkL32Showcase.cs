using UnityEditor;
using UnityEngine;
using System.IO;
namespace ProjectSpark.Level32.B850I.Editor {
 public static class SparkL32Showcase {
  const string Root="Assets/ProjectSpark/Level32/B850ILightning";
  [MenuItem("Tools/Project Spark/Automatic Level Generator/32 B850I/3. Create assembled preview prefab")]
  static void Assembled()=>Create("ASSEMBLED");
  [MenuItem("Tools/Project Spark/Automatic Level Generator/32 B850I/4. Create batched preview prefab")]
  static void Batched()=>Create("BATCHED");
  static void Create(string id) { var source=AssetDatabase.LoadAssetAtPath<GameObject>(Root+"/Models_Native/L32_"+id+".spark32b850i"); if(!source) throw new InvalidDataException("Reimport native meshes first"); var go=(GameObject)PrefabUtility.InstantiatePrefab(source); try { SparkL32MaterialTools.ApplyTo(go,true); if(!AssetDatabase.IsValidFolder(Root+"/Generated")) AssetDatabase.CreateFolder(Root,"Generated"); PrefabUtility.SaveAsPrefabAsset(go,Root+"/Generated/L32_"+id+".prefab"); } finally { Object.DestroyImmediate(go); } }
 }
}
