using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace ProjectSpark.EditorTools
{
    internal static class SparkLevel110Art
    {
        [Serializable] private sealed class Glb { public Node[] nodes; public Geometry[] meshes; public Accessor[] accessors; public BufferView[] bufferViews; }
        [Serializable] private sealed class Node { public string name; public int mesh = -1; public float[] translation, rotation, scale, matrix; }
        [Serializable] private sealed class Geometry { public Primitive[] primitives; }
        [Serializable] private sealed class Primitive { public Attributes attributes; public int indices, material, mode; }
        [Serializable] private sealed class Attributes { public int POSITION; }
        [Serializable] private sealed class Accessor { public int count, byteOffset, bufferView, componentType; public string type; }
        [Serializable] private sealed class BufferView { public int byteOffset, byteStride; }

        internal static GameObject Build(string path, Transform parent, Vector3 anchor, string meshFolder, string materialFolder)
        {
            byte[] bytes = File.ReadAllBytes(path);
            using var stream = new BinaryReader(new MemoryStream(bytes));
            if (stream.ReadUInt32() != 0x46546C67 || stream.ReadUInt32() != 2 || stream.ReadUInt32() != bytes.Length)
                throw new InvalidDataException($"Invalid GLB header for {path}");
            int length = stream.ReadInt32();
            if (stream.ReadUInt32() != 0x4E4F534A) throw new InvalidDataException($"JSON chunk missing in {path}");
            var input = JsonUtility.FromJson<Glb>(System.Text.Encoding.UTF8.GetString(stream.ReadBytes(length)));
            stream.ReadInt32();
            if (stream.ReadUInt32() != 0x004E4942) throw new InvalidDataException($"Geometry chunk missing in {path}");
            long start = stream.BaseStream.Position;

            var vertices = new List<Vector3>();
            var submeshes = new List<int[]>();
            var materials = new List<Material>();
            foreach (var node in input.nodes)
            {
                if (node.name == "world") continue;
                if (node.mesh < 0 || node.mesh >= input.meshes.Length || node.translation != null || node.rotation != null
                    || node.scale != null || node.matrix != null) throw new InvalidDataException($"Unsupported transform on {node.name} in {path}");
                foreach (var primitive in input.meshes[node.mesh].primitives)
                {
                    if (primitive.mode != 4) throw new InvalidDataException($"Expected triangle geometry in {path}");
                    var positions = input.accessors[primitive.attributes.POSITION];
                    var view = input.bufferViews[positions.bufferView];
                    if (positions.componentType != 5126 || positions.type != "VEC3") throw new InvalidDataException($"Unsupported vertices in {path}");
                    int baseVertex = vertices.Count;
                    for (int i = 0; i < positions.count; i++)
                    {
                        stream.BaseStream.Position = start + view.byteOffset + positions.byteOffset + i * (view.byteStride == 0 ? 12 : view.byteStride);
                        float x = stream.ReadSingle(), y = stream.ReadSingle(), z = stream.ReadSingle();
                        vertices.Add(new Vector3(x, z, y) * .001f - anchor);
                    }

                    var index = input.accessors[primitive.indices];
                    stream.BaseStream.Position = start + input.bufferViews[index.bufferView].byteOffset + index.byteOffset;
                    var triangles = new int[index.count];
                    for (int i = 0; i < triangles.Length; i++)
                    {
                        int value = index.componentType switch
                        {
                            5125 => stream.ReadInt32(), 5123 => stream.ReadUInt16(), 5121 => stream.ReadByte(),
                            _ => throw new InvalidDataException($"Unsupported indices in {path}")
                        };
                        if (value < 0 || value >= positions.count) throw new InvalidDataException($"Index outside vertex array in {path}");
                        triangles[i] = baseVertex + value;
                    }
                    for (int i = 0; i < triangles.Length; i += 3) (triangles[i + 1], triangles[i + 2]) = (triangles[i + 2], triangles[i + 1]);
                    submeshes.Add(triangles);
                    string materialName = node.name.Split('_').Last();
                    var material = AssetDatabase.LoadAssetAtPath<Material>($"{materialFolder}/{materialName}_AutoPBR.mat");
                    if (!material) material = AssetDatabase.LoadAssetAtPath<Material>($"{materialFolder}/EPOXY_AutoPBR.mat");
                    materials.Add(material);
                }
            }
            if (vertices.Count == 0) throw new InvalidDataException($"No mesh geometry in {path}");
            string name = Path.GetFileNameWithoutExtension(path);
            string asset = $"{meshFolder}/{name}.asset";
            var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(asset);
            bool existing = mesh;
            if (mesh) mesh.Clear();
            else mesh = new Mesh { name = name };
            mesh.indexFormat = vertices.Count > 65535 ? IndexFormat.UInt32 : IndexFormat.UInt16;
            mesh.SetVertices(vertices);
            mesh.subMeshCount = submeshes.Count;
            for (int i = 0; i < submeshes.Count; i++) mesh.SetTriangles(submeshes[i], i, false);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            if (existing) EditorUtility.SetDirty(mesh);
            else AssetDatabase.CreateAsset(mesh, asset);

            var result = new GameObject(name, typeof(MeshFilter), typeof(MeshRenderer));
            result.transform.SetParent(parent, false);
            result.transform.localPosition = anchor;
            result.GetComponent<MeshFilter>().sharedMesh = mesh;
            result.GetComponent<MeshRenderer>().sharedMaterials = materials.ToArray();
            return result;
        }
    }
}
