using System;
using System.Collections.Generic;
using System.Linq;
using ProjectSpark.Gameplay.AxisForge;
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.EditorTools.AxisForge
{
    internal static partial class SparkLevel120Builder
    {
        private static void BuildFootprintLegend(SparkLevel120Process board)
        {
            string output = Kit + "/Unity/Art";
            if (!AssetDatabase.IsValidFolder(output)) AssetDatabase.CreateFolder(Kit + "/Unity", "Art");
            string materialPath = output + "/RAX28_FootprintLegend.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if (!material)
            {
                var shader = Shader.Find("Universal Render Pipeline/Unlit");
                if (!shader) throw new InvalidOperationException("The footprint legend requires the project URP Unlit shader.");
                material = new Material(shader) { name = "RAX-28 Footprint Legend", color = new Color(.32f, .51f, .55f) };
                AssetDatabase.CreateAsset(material, materialPath);
            }
            PaintLegend(board.fixture, board.parts, material, output + "/RAX28_FootprintLegend.asset",
                new Vector2(.260f, .190f), .00142f, "RAX-28 placement legend");
        }

        private static void PaintLegend(Transform parent, IEnumerable<SparkLevel120Process.Part> parts, Material material,
            string path, Vector2 halfSize, float surface, string name)
        {
            var vertices = new List<Vector3>();
            var triangles = new List<int>();
            foreach (var part in parts)
            {
                if (part.label == "W353") continue;
                var center = parent.InverseTransformPoint(part.footprint.position);
                Vector2 size = part.label switch
                {
                    "U1" or "U2" or "U3" or "U4" => new Vector2(.038f, .038f),
                    _ => VisualFootprint(parent, part)
                };
                const float edge = .0009f;
                float minX = Mathf.Clamp(center.x - size.x * .5f, -halfSize.x + edge, halfSize.x - edge);
                float maxX = Mathf.Clamp(center.x + size.x * .5f, -halfSize.x + edge, halfSize.x - edge);
                float minZ = Mathf.Clamp(center.z - size.y * .5f, -halfSize.y + edge, halfSize.y - edge);
                float maxZ = Mathf.Clamp(center.z + size.y * .5f, -halfSize.y + edge, halfSize.y - edge);
                const float line = .00038f;
                float xTip = Mathf.Min(.0041f, (maxX - minX) * .45f);
                float zTip = Mathf.Min(.0035f, (maxZ - minZ) * .68f);
                if (xTip < line || zTip < line) continue;
                Quad(minX, minZ, minX + xTip, minZ + line);
                Quad(minX, minZ, minX + line, minZ + zTip);
                Quad(maxX - xTip, maxZ - line, maxX, maxZ);
                Quad(maxX - line, maxZ - zTip, maxX, maxZ);
                if (part.label.StartsWith("U", StringComparison.Ordinal))
                    Quad(minX + .001f, minZ + .001f, minX + .0017f, minZ + .0017f);
            }

            var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            bool exists = mesh;
            if (!exists) mesh = new Mesh { name = name };
            mesh.Clear();
            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            if (!exists) AssetDatabase.CreateAsset(mesh, path);
            else EditorUtility.SetDirty(mesh);
            var node = parent.Find(name);
            if (!node)
            {
                node = Child(parent, name);
                node.gameObject.AddComponent<MeshFilter>();
                node.gameObject.AddComponent<MeshRenderer>();
            }
            node.GetComponent<MeshFilter>().sharedMesh = mesh;
            node.GetComponent<MeshRenderer>().sharedMaterial = material;

            void Quad(float left, float bottom, float right, float top)
            {
                int index = vertices.Count;
                vertices.Add(new Vector3(left, surface, bottom));
                vertices.Add(new Vector3(left, surface, top));
                vertices.Add(new Vector3(right, surface, top));
                vertices.Add(new Vector3(right, surface, bottom));
                triangles.Add(index);
                triangles.Add(index + 1);
                triangles.Add(index + 2);
                triangles.Add(index);
                triangles.Add(index + 2);
                triangles.Add(index + 3);
            }
        }

        private static Vector2 VisualFootprint(Transform fixture, SparkLevel120Process.Part part)
        {
            float halfWidth = .0014f, halfDepth = .0013f;
            foreach (var filter in part.model.GetComponentsInChildren<MeshFilter>(true))
            {
                if (!filter.sharedMesh) continue;
                var shape = filter.sharedMesh.bounds;
                var matrix = fixture.worldToLocalMatrix * filter.transform.localToWorldMatrix;
                var center = fixture.InverseTransformPoint(part.footprint.position);
                foreach (float x in new[] { shape.min.x, shape.max.x })
                foreach (float y in new[] { shape.min.y, shape.max.y })
                foreach (float z in new[] { shape.min.z, shape.max.z })
                {
                    Vector3 local = matrix.MultiplyPoint3x4(new Vector3(x, y, z)) - center;
                    halfWidth = Mathf.Max(halfWidth, Mathf.Abs(local.x));
                    halfDepth = Mathf.Max(halfDepth, Mathf.Abs(local.z));
                }
            }
            return new Vector2(halfWidth * 2f + .0015f, halfDepth * 2f + .0015f);
        }
    }
}
