using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor.AssetImporters;
using UnityEngine;
using UnityEngine.Rendering;

namespace ProjectSpark.SparkV5.Pro.Editor
{
    // Native Unity model asset import, independent of FBX/Assimp/third-party packages.
    [ScriptedImporter(1, "sparkmesh")]
    public sealed class SparkMeshImporter : ScriptedImporter
    {
        static string ReadName(BinaryReader r)
        {
            int n = r.ReadInt32();
            if (n < 1 || n > 240) throw new InvalidDataException("Invalid Spark node name length.");
            byte[] bytes = r.ReadBytes(n);
            if (bytes.Length != n) throw new EndOfStreamException();
            return Encoding.UTF8.GetString(bytes);
        }
        static float F(BinaryReader r)
        {
            float f = r.ReadSingle();
            if (float.IsNaN(f) || float.IsInfinity(f)) throw new InvalidDataException("Non-finite Spark model data.");
            return f;
        }
        static Vector3 V(BinaryReader r) => new Vector3(F(r), F(r), F(r));
        public override void OnImportAsset(AssetImportContext ctx)
        {
            GameObject root = null;
            var createdMeshes = new List<Mesh>();
            try
            {
                using (var stream = File.OpenRead(ctx.assetPath))
                using (var r = new BinaryReader(stream))
                {
                    if (Encoding.ASCII.GetString(r.ReadBytes(4)) != "SPKM" || r.ReadInt32() != 1)
                        throw new InvalidDataException("Unsupported Spark mesh format.");
                    int count = r.ReadInt32();
                    if (count != 116) throw new InvalidDataException("Expected 48 pivots and 68 mesh nodes.");
                    var nodes = new Transform[count];
                    var names = new HashSet<string>(StringComparer.Ordinal);
                    int meshCount = 0;
                    for (int i = 0; i < count; i++)
                    {
                        string name = ReadName(r);
                        if (!names.Add(name)) throw new InvalidDataException("Duplicate Spark node: " + name);
                        int parent = r.ReadInt32();
                        if (i == 0 ? parent != -1 : parent < 0 || parent >= i)
                            throw new InvalidDataException("Invalid hierarchy at " + name);
                        Vector3 position = V(r);
                        int nv = r.ReadInt32(), ni = r.ReadInt32();
                        if (nv < 0 || nv > 1000000 || ni < 0 || ni > 6000000 || ni % 3 != 0 || (nv == 0) != (ni == 0))
                            throw new InvalidDataException("Invalid mesh counts at " + name);
                        var node = new GameObject(name);
                        if (i == 0) root = node; else node.transform.SetParent(nodes[parent], false);
                        nodes[i] = node.transform;
                        node.transform.localPosition = position;
                        if (nv == 0) continue;
                        var vertices = new Vector3[nv]; var normals = new Vector3[nv]; var uv = new Vector2[nv];
                        var indices = new int[ni];
                        for (int j = 0; j < nv; j++) { vertices[j] = V(r); normals[j] = V(r); uv[j] = new Vector2(F(r), F(r)); }
                        for (int j = 0; j < ni; j++) { int index = r.ReadInt32(); if (index < 0 || index >= nv) throw new InvalidDataException("Index out of range: " + name); indices[j] = index; }
                        var mesh = new Mesh { name = name, indexFormat = nv > 65535 ? IndexFormat.UInt32 : IndexFormat.UInt16 };
                        createdMeshes.Add(mesh);
                        mesh.vertices = vertices; mesh.normals = normals; mesh.uv = uv; mesh.triangles = indices;
                        mesh.RecalculateBounds(); mesh.RecalculateTangents();
                        node.AddComponent<MeshFilter>().sharedMesh = mesh;
                        node.AddComponent<MeshRenderer>(); // Builder supplies the authored URP materials.
                        meshCount++;
                    }
                    if (meshCount != 68 || !names.Contains("Body") || !names.Contains("Head") || stream.Position != stream.Length)
                        throw new InvalidDataException("Incomplete Spark rig or trailing data.");
                    foreach (var mesh in createdMeshes) ctx.AddObjectToAsset("mesh_" + mesh.name, mesh);
                    ctx.AddObjectToAsset("root", root);
                    ctx.SetMainObject(root);
                }
            }
            catch (Exception ex)
            {
                if (root) DestroyImmediate(root);
                foreach (var mesh in createdMeshes) if (mesh) DestroyImmediate(mesh);
                ctx.LogImportError("Spark native model import failed: " + ex.Message);
            }
        }
    }
}
