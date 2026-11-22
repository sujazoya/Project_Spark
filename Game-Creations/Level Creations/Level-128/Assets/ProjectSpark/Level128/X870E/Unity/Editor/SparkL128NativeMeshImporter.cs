using System;
using System.IO;
using System.Text;
using UnityEditor.AssetImporters;
using UnityEngine;
using UnityEngine.Rendering;

namespace ProjectSpark.Level128.Editor
{
    // Level-specific extension; deliberately does not override Unity's FBX importer.
    [ScriptedImporter(1, "spark128mesh")]
    public sealed class SparkL128NativeMeshImporter : ScriptedImporter
    {
        public override void OnImportAsset(AssetImportContext ctx)
        {
            using (var reader = new BinaryReader(File.OpenRead(ctx.assetPath), Encoding.UTF8))
            {
                if (Encoding.ASCII.GetString(reader.ReadBytes(4)) != "SPM1")
                    throw new InvalidDataException("Invalid native mesh header: " + ctx.assetPath);
                int count = reader.ReadInt32();
                if (count < 1 || count > 20000) throw new InvalidDataException("Invalid model node count.");
                var nodes = new GameObject[count];
                GameObject root = null;
                int meshCount = 0;
                for (int i = 0; i < count; i++)
                {
                    int length = reader.ReadInt32();
                    if (length < 1 || length > 1024) throw new InvalidDataException("Invalid node name length.");
                    string nodeName = Encoding.UTF8.GetString(reader.ReadBytes(length));
                    int parent = reader.ReadInt32();
                    if (parent < -1 || parent >= i || (i == 0 ? parent != -1 : parent == -1))
                        throw new InvalidDataException("Invalid node hierarchy.");
                    Vector3 pos = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                    var go = new GameObject(nodeName);
                    nodes[i] = go;
                    if (parent >= 0) go.transform.SetParent(nodes[parent].transform, false);
                    else root = go;
                    go.transform.localPosition = pos;
                    int vertexCount = reader.ReadInt32();
                    if (vertexCount == 0) continue;
                    if (vertexCount < 0 || vertexCount > 3000000 || vertexCount % 3 != 0 || reader.BaseStream.Length - reader.BaseStream.Position < (long)vertexCount * 32)
                        throw new InvalidDataException("Invalid vertex count: " + nodeName);
                    var vertices = new Vector3[vertexCount];
                    var normals = new Vector3[vertexCount];
                    var uvs = new Vector2[vertexCount];
                    var triangles = new int[vertexCount];
                    for (int j = 0; j < vertexCount; j++)
                    {
                        vertices[j] = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                        normals[j] = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                        uvs[j] = new Vector2(reader.ReadSingle(), reader.ReadSingle());
                        triangles[j] = j;
                    }
                    var mesh = new Mesh { name = nodeName, indexFormat = vertexCount > 65535 ? IndexFormat.UInt32 : IndexFormat.UInt16 };
                    mesh.vertices = vertices;
                    mesh.normals = normals;
                    mesh.uv = uvs;
                    mesh.SetTriangles(triangles, 0, true);
                    mesh.RecalculateTangents();
                    go.AddComponent<MeshFilter>().sharedMesh = mesh;
                    go.AddComponent<MeshRenderer>(); // Existing Level 128 builder binds native URP materials by node name.
                    ctx.AddObjectToAsset("mesh_" + i, mesh);
                    meshCount++;
                }
                if (root == null || meshCount == 0 || reader.BaseStream.Position != reader.BaseStream.Length)
                    throw new InvalidDataException("Invalid or empty native model: " + ctx.assetPath);
                ctx.AddObjectToAsset("root", root);
                ctx.SetMainObject(root);
            }
        }
    }
}
