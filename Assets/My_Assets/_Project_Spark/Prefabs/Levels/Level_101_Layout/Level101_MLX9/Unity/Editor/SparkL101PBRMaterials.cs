#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
public static class SparkL101PBRMaterials {
 [MenuItem("Tools/Project Spark/Levels/101/Create Reference PBR Materials")]
 static void Build() {
  var paths=AssetDatabase.FindAssets("t:Texture2D").Select(AssetDatabase.GUIDToAssetPath).Where(s=>s.Contains("Level101")&&s.EndsWith("_BaseColor.png")).ToArray();
  const string folder="Assets/ProjectSpark_L101_Generated_Materials";
  if(!AssetDatabase.IsValidFolder(folder))AssetDatabase.CreateFolder("Assets","ProjectSpark_L101_Generated_Materials");
  foreach(var path in paths){
   var name=Path.GetFileNameWithoutExtension(path).Replace("_BaseColor","");
   var shader=Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
   if(!shader){Debug.LogError("Missing Lit shader");return;}
   var m=new Material(shader){name=name};
   var baseColor=AssetDatabase.LoadAssetAtPath<Texture2D>(path);
   if(m.HasProperty("_BaseMap"))m.SetTexture("_BaseMap",baseColor);
   if(m.HasProperty("_MainTex"))m.SetTexture("_MainTex",baseColor);
   var op=folder+"/"+name+".mat";
   if(!AssetDatabase.LoadAssetAtPath<Material>(op))AssetDatabase.CreateAsset(m,op);else Object.DestroyImmediate(m);
  }
  AssetDatabase.SaveAssets();
  Debug.Log("[L101] Generated "+paths.Length+" source reference materials. Assign/remap FBX slots; normal/metal/smoothness require pipeline-specific setup.");
 }
}
#endif
