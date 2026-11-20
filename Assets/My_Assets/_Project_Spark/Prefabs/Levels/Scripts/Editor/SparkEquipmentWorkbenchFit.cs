#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProjectSpark.EditorTools
{
    internal static class SparkEquipmentWorkbenchFit
    {
        internal const float SourceArtUnit = .096f;

        internal static BoxCollider FindTable(Scene scene) => scene.GetRootGameObjects()
            .SelectMany(item => item.GetComponentsInChildren<BoxCollider>(true))
            .Single(item => item.name == "CentralTable" && item.transform.parent.name == "_PS_ROOM_WALKABLE_COLLIDERS");

        internal static Bounds MeasureArt(Transform root, Matrix4x4 worldToSpace)
        {
            Bounds bounds = default;
            bool first = true;
            foreach (var filter in root.GetComponentsInChildren<MeshFilter>(true))
            {
                if (!filter.sharedMesh) continue;
                var mesh = filter.sharedMesh.bounds;
                var matrix = worldToSpace * filter.transform.localToWorldMatrix;
                for (int i = 0; i < 8; i++)
                {
                    var corner = new Vector3(
                        (i & 1) == 0 ? mesh.min.x : mesh.max.x,
                        (i & 2) == 0 ? mesh.min.y : mesh.max.y,
                        (i & 4) == 0 ? mesh.min.z : mesh.max.z);
                    var point = matrix.MultiplyPoint3x4(corner);
                    if (first) { bounds = new Bounds(point, Vector3.zero); first = false; }
                    else bounds.Encapsulate(point);
                }
            }
            if (first) throw new InvalidOperationException("The equipment fixture has no model geometry.");
            return bounds;
        }

        internal static Transform[] SeatMotionPivots(Transform assembly, TextAsset data)
        {
            if (!data) throw new InvalidOperationException("The original animation pivots are missing.");
            var pivots = new List<Transform>();
            foreach (string row in data.text.Split('\n').Skip(1))
            {
                if (string.IsNullOrWhiteSpace(row)) continue;
                string[] values = row.TrimEnd('\r').Split(',');
                var module = assembly.Find(values[1]);
                var pivot = module.GetComponentsInChildren<Transform>(true)
                    .Single(part => part.name.EndsWith("_" + values[0], StringComparison.Ordinal));

                // Some FBX pivot translations were imported in centimeters. Keep the resting mesh
                // pose and place the rotation axis at the source kit's physical hinge or dial.
                var parts = pivot.Cast<Transform>().ToArray();
                foreach (var part in parts) part.SetParent(module, true);
                pivot.SetPositionAndRotation(assembly.TransformPoint(SourcePosition(values)), assembly.rotation);
                foreach (var part in parts) part.SetParent(pivot, true);
                pivots.Add(pivot);
            }
            return pivots.ToArray();
        }

        internal static Vector3 SourcePosition(string[] values)
        {
            float Coordinate(int index) => float.Parse(values[index], NumberStyles.Float, CultureInfo.InvariantCulture);
            return new Vector3(Coordinate(2), Coordinate(3), -Coordinate(4)) * SourceArtUnit;
        }

    }
}
#endif
