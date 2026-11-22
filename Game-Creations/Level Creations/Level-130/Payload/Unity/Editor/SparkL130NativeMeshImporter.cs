using System;
using System.IO;
using System.Text;
using UnityEditor.AssetImporters;
using UnityEngine;
using UnityEngine.Rendering;

namespace ProjectSpark.Level130.Editor
{
    // Reads the verified Level 130 source geometry, already converted from mm to metres.
    [ScriptedImporter(1, "spark130mesh")]
    public sealed class SparkL130NativeMeshImporter : ScriptedImporter
    {
        public override void OnImportAsset(AssetImportContext ctx)
        {
            GameObject root = null;
            var meshes = new System.Collections.Generic.List<Mesh>();
            try
            {
                using (var reader = new BinaryReader(File.OpenRead(ctx.assetPath), Encoding.UTF8))
                {
                    if (Encoding.ASCII.GetString(reader.ReadBytes(8)) != "SPK130M1")
                        throw new InvalidDataException("Invalid Level 130 native mesh signature.");
                    int count = reader.ReadInt32();
                    if (count < 1 || count > 10000) throw new InvalidDataException("Invalid node count.");
                    var nodes = new GameObject[count];
                    for (int i = 0; i < count; ++i)
                    {
                        int parent = reader.ReadInt32();
                        int nameLength = reader.ReadInt32();
                        if ((i == 0 ? parent != -1 : parent < 0 || parent >= i) || nameLength < 1 || nameLength > 4096)
                            throw new InvalidDataException("Invalid native mesh hierarchy.");
                        byte[] bytes = reader.ReadBytes(nameLength);
                        if (bytes.Length != nameLength) throw new EndOfStreamException();
                        var go = new GameObject(Encoding.UTF8.GetString(bytes));
                        nodes[i] = go;
                        if (i == 0) root = go;
                        else go.transform.SetParent(nodes[parent].transform, false);
                        go.transform.localPosition = ReadVector3(reader);
                        int vertexCount = reader.ReadInt32();
                        if (vertexCount < 0 || vertexCount > 3000000 || vertexCount % 3 != 0 ||
                            (long)vertexCount * 32 > reader.BaseStream.Length - reader.BaseStream.Position)
                            throw new InvalidDataException("Invalid native mesh vertex count.");
                        if (vertexCount == 0) continue;
                        var vertices = new Vector3[vertexCount];
                        var normals = new Vector3[vertexCount];
                        var uv = new Vector2[vertexCount];
                        var triangles = new int[vertexCount];
                        for (int j = 0; j < vertexCount; ++j)
                        {
                            vertices[j] = ReadVector3(reader);
                            normals[j] = ReadVector3(reader);
                            uv[j] = new Vector2(reader.ReadSingle(), reader.ReadSingle());
                            triangles[j] = j;
                        }
                        var mesh = new Mesh { name = go.name, indexFormat = vertexCount > 65535 ? IndexFormat.UInt32 : IndexFormat.UInt16 };
                        meshes.Add(mesh);
                        mesh.vertices = vertices;
                        mesh.normals = normals;
                        mesh.uv = uv;
                        mesh.triangles = triangles;
                        mesh.RecalculateBounds();
                        mesh.RecalculateTangents();
                        go.AddComponent<MeshFilter>().sharedMesh = mesh;
                        go.AddComponent<MeshRenderer>(); // URP materials are assigned by the existing Level 130 builder.
                    }
                    if (reader.BaseStream.Position != reader.BaseStream.Length || meshes.Count == 0)
                        throw new InvalidDataException("Native mesh data incomplete or has trailing bytes.");
                }
            }
            catch
            {
                if (root != null) DestroyImmediate(root);
                foreach (Mesh mesh in meshes) if (mesh != null) DestroyImmediate(mesh);
                throw;
            }
            for (int i = 0; i < meshes.Count; ++i) ctx.AddObjectToAsset("mesh_" + i, meshes[i]);
            ctx.AddObjectToAsset("root", root);
            ctx.SetMainObject(root);
        }
        private static Vector3 ReadVector3(BinaryReader reader)
        {
            return new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
        }
    }
}
