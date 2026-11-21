using System;
using System.Collections.Generic;
using System.Linq;
using ProjectSpark.Gameplay.SignalCrucible;
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.EditorTools.SignalCrucible
{
    internal static partial class SparkLevel111Builder
    {
        private static void BuildFootprintLegend(SparkLevel111Process board)
        {
            string output = Folder + "/SIX19/Unity/Art";
            if (!AssetDatabase.IsValidFolder(output)) AssetDatabase.CreateFolder(Folder + "/SIX19/Unity", "Art");
            string materialPath = output + "/SIX19_FootprintLegend.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if (!material)
            {
                var shader = Shader.Find("Universal Render Pipeline/Unlit");
                if (!shader) throw new InvalidOperationException("The footprint legend requires the project URP Unlit shader.");
                material = new Material(shader) { name = "SIX-19 Footprint Legend", color = new Color(.32f, .51f, .55f) };
                AssetDatabase.CreateAsset(material, materialPath);
            }
            PaintLegend(board.fixture, board.parts, material, output + "/SIX19_FootprintLegend.asset",
                new Vector2(.165f, .120f), .00178f, "SIX-19 placement legend");
        }

        private static void PaintLegend(Transform parent, IEnumerable<SparkLevel111Process.Part> parts, Material material,
            string path, Vector2 halfSize, float surface, string name)
        {
            var vertices = new List<Vector3>();
            var triangles = new List<int>();
            foreach (var part in parts)
            {
                if (part.label == "W185") continue;
                var center = part.footprint.localPosition;
                Vector2 size = part.label switch
                {
                    "U1" => new Vector2(.024f, .024f),
                    "U2" => new Vector2(.015f, .015f),
                    _ => VisualFootprint(part)
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

        private static Vector2 VisualFootprint(SparkLevel111Process.Part part)
        {
            float halfWidth = .0014f, halfDepth = .0013f;
            foreach (var filter in part.model.GetComponentsInChildren<MeshFilter>(true))
            {
                if (!filter.sharedMesh) continue;
                var shape = filter.sharedMesh.bounds;
                var matrix = part.footprint.worldToLocalMatrix * filter.transform.localToWorldMatrix;
                foreach (float x in new[] { shape.min.x, shape.max.x })
                foreach (float z in new[] { shape.min.z, shape.max.z })
                {
                    Vector3 local = matrix.MultiplyPoint3x4(new Vector3(x, shape.center.y, z));
                    halfWidth = Mathf.Max(halfWidth, Mathf.Abs(local.x));
                    halfDepth = Mathf.Max(halfDepth, Mathf.Abs(local.z));
                }
            }
            return new Vector2(halfWidth * 2f + .0015f, halfDepth * 2f + .0015f);
        }
    }
}
