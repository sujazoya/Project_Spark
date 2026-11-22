using System;
using System.IO;
using System.Text;
using UnityEditor.AssetImporters;
using UnityEngine;
using UnityEngine.Rendering;
namespace ProjectSpark.Level131.Editor {
 [ScriptedImporter(1,"spark131mesh")]
 public sealed class SparkL131NativeImporter : ScriptedImporter {
  static string ReadName(BinaryReader b) { int n=b.ReadUInt16(); return Encoding.UTF8.GetString(b.ReadBytes(n)); }
  public override void OnImportAsset(AssetImportContext ctx) {
   var root=new GameObject(Path.GetFileNameWithoutExtension(ctx.assetPath));
   using(var b=new BinaryReader(File.OpenRead(ctx.assetPath))) {
    if(Encoding.ASCII.GetString(b.ReadBytes(8))!="SPK131M1") throw new InvalidDataException("Invalid native mesh signature");
    int groups=b.ReadInt32(); if(groups<1||groups>10000) throw new InvalidDataException("Invalid group count");
    for(int g=0;g<groups;g++) {
     string name=ReadName(b), material=ReadName(b); int n=b.ReadInt32(); if(n<3||n%3!=0||n>3000000) throw new InvalidDataException("Invalid vertex count");
     var vertices=new Vector3[n];var uv=new Vector2[n];var indices=new int[n];
     for(int i=0;i<n;i++) { float x=b.ReadSingle(),y=b.ReadSingle(),z=b.ReadSingle(); vertices[i]=new Vector3(-x,y,z); }
     for(int i=0;i<n;i++) uv[i]=new Vector2(b.ReadSingle(),b.ReadSingle());
     for(int i=0;i<n;i+=3) { indices[i]=i;indices[i+1]=i+2;indices[i+2]=i+1; }
     var mesh=new Mesh { name=name, indexFormat=n>65535?IndexFormat.UInt32:IndexFormat.UInt16 };mesh.vertices=vertices;mesh.uv=uv;mesh.triangles=indices;mesh.RecalculateNormals();mesh.RecalculateTangents();mesh.RecalculateBounds();
     var child=new GameObject(name);child.transform.SetParent(root.transform,false);child.AddComponent<MeshFilter>().sharedMesh=mesh;child.AddComponent<MeshRenderer>().sharedMaterial=UnityEditor.AssetDatabase.GetBuiltinExtraResource<Material>("Default-Material.mat");ctx.AddObjectToAsset("mesh_"+g,mesh);
    }
    if(b.BaseStream.Position!=b.BaseStream.Length) throw new InvalidDataException("Unexpected trailing bytes");
   }
   ctx.AddObjectToAsset("root",root);ctx.SetMainObject(root);
  }
 }
}
