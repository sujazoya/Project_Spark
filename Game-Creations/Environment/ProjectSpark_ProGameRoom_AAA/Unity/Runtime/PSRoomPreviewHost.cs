using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace ProjectSpark.RoomStudio
{
    /// <summary>
    /// PREVIEW ONLY. Copies MeshFilter/MeshRenderer geometry into optimized render meshes.
    /// No source MonoBehaviours, electrical terminals, colliders, Rigidbody, VFX,
    /// game state, objectives or level managers are cloned or activated.
    /// Compatible with source prefab assets AND scene GameObjects.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PSRoomPreviewHost : MonoBehaviour
    {
        [Header("Mode")]
        public bool threeBayMode = false;
        [Header("Single large table showcase")]
        public GameObject singleLevelPrefab;
        [Header("Three separate table bays")]
        public GameObject leftLevelPrefab;
        public GameObject centerLevelPrefab;
        public GameObject rightLevelPrefab;
        [Header("Performance and fit")]
        [Range(0.5f, 0.99f)] public float fitPadding = 0.91f;
        public int maxSourceMeshFilters = 16000;
        public int maxVerticesPerBatch = 180000;
        public bool combineByMaterial = true;

        // Relative to room origin. The front-stage physical table top is at ~1.30 m.
        private static readonly Vector3 BigCenter = new Vector3(0f, 1.34f, 0.30f);
        private static readonly Vector3 BigMax = new Vector3(8.55f, 2.3f, 4.06f);
        private static readonly Vector3 SmallMax = new Vector3(2.60f, 1.75f, 3.75f);

        [ContextMenu("Rebuild visual-only level previews")]
        public void RebuildPreviews()
        {
            ClearGenerated();
            if (threeBayMode)
            {
                BuildSlot("Left", leftLevelPrefab, new Vector3(-2.91f, 1.34f, 0.30f), SmallMax);
                BuildSlot("Center", centerLevelPrefab, BigCenter, SmallMax);
                BuildSlot("Right", rightLevelPrefab, new Vector3(2.91f, 1.34f, 0.30f), SmallMax);
            }
            else
            {
                BuildSlot("FullTable", singleLevelPrefab, BigCenter, BigMax);
            }
        }

        public void SetSingleLevel(GameObject levelArt)
        {
            threeBayMode = false;
            singleLevelPrefab = levelArt;
            RebuildPreviews();
        }

        public void SetBayLevel(int bay, GameObject levelArt)
        {
            threeBayMode = true;
            if (bay == 0) leftLevelPrefab = levelArt;
            else if (bay == 1) centerLevelPrefab = levelArt;
            else if (bay == 2) rightLevelPrefab = levelArt;
            else throw new ArgumentOutOfRangeException(nameof(bay));
            RebuildPreviews();
        }

        private void ClearGenerated()
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                var child = transform.GetChild(i);
                if (!child.name.StartsWith("_PS_LevelPreview_", StringComparison.Ordinal)) continue;
                var filters = child.GetComponentsInChildren<MeshFilter>(true);
                foreach (var filter in filters)
                {
                    var mesh = filter.sharedMesh;
                    if (mesh != null && mesh.name.StartsWith("_PS_COMBINED_", StringComparison.Ordinal))
                    {
                        if (Application.isPlaying) Destroy(mesh); else DestroyImmediate(mesh);
                    }
                }
                if (Application.isPlaying) Destroy(child.gameObject); else DestroyImmediate(child.gameObject);
            }
        }

        private struct Part
        {
            public Mesh mesh;
            public int submesh;
            public Material material;
            public Matrix4x4 localMatrix;
        }

        private static void EncapsulateMeshBounds(ref Bounds bounds, ref bool any, Bounds meshBounds, Matrix4x4 tx)
        {
            Vector3 c = meshBounds.center;
            Vector3 e = meshBounds.extents;
            for (int x = -1; x <= 1; x += 2)
            for (int y = -1; y <= 1; y += 2)
            for (int z = -1; z <= 1; z += 2)
            {
                Vector3 p = tx.MultiplyPoint3x4(c + Vector3.Scale(e, new Vector3(x,y,z)));
                if (!any) { bounds = new Bounds(p, Vector3.zero); any = true; }
                else bounds.Encapsulate(p);
            }
        }

        private void BuildSlot(string slot, GameObject source, Vector3 center, Vector3 available)
        {
            if (source == null) return;
            var sourceFilters = source.GetComponentsInChildren<MeshFilter>(true);
            if (sourceFilters.Length > maxSourceMeshFilters)
            {
                Debug.LogWarning($"[PS Room] {source.name}: {sourceFilters.Length} meshes exceeds configured preview budget {maxSourceMeshFilters}. Increase budget or use a simplified visual-only prefab.");
                return;
            }

            var parts = new List<Part>(sourceFilters.Length);
            Bounds modelBounds = new Bounds();
            bool hasBounds = false;
            Matrix4x4 toSource = source.transform.worldToLocalMatrix;
            foreach (var mf in sourceFilters)
            {
                Mesh mesh = mf.sharedMesh;
                MeshRenderer renderer = mf.GetComponent<MeshRenderer>();
                if (mesh == null || renderer == null || !renderer.enabled) continue;
                Matrix4x4 relative = toSource * mf.transform.localToWorldMatrix;
                EncapsulateMeshBounds(ref modelBounds, ref hasBounds, mesh.bounds, relative);
                Material[] mats = renderer.sharedMaterials;
                for (int sub = 0; sub < mesh.subMeshCount; sub++)
                {
                    Material mat = mats.Length > sub ? mats[sub] : (mats.Length > 0 ? mats[0] : null);
                    parts.Add(new Part { mesh = mesh, submesh = sub, material = mat, localMatrix = relative });
                }
            }
            if (!hasBounds || parts.Count == 0)
            {
                Debug.LogWarning($"[PS Room] No static MeshRenderer geometry on {source.name}. Supply an art FBX or visual-only prefab.");
                return;
            }

            float fit = Mathf.Min(available.x / Mathf.Max(.01f, modelBounds.size.x),
                                  available.y / Mathf.Max(.01f, modelBounds.size.y),
                                  available.z / Mathf.Max(.01f, modelBounds.size.z)) * fitPadding;
            var holder = new GameObject("_PS_LevelPreview_" + slot);
            holder.transform.SetParent(transform, false);
            holder.transform.localPosition = center;
            var content = new GameObject("Art_Only_NoGameplay");
            content.transform.SetParent(holder.transform, false);
            content.transform.localScale = Vector3.one * fit;
            content.transform.localPosition = new Vector3(-modelBounds.center.x * fit,
                                                         -modelBounds.min.y * fit,
                                                         -modelBounds.center.z * fit);
            if (combineByMaterial)
            {
                try { BuildCombined(parts, content.transform); }
                catch (Exception ex)
                {
                    Debug.LogWarning("[PS Room] Some source meshes may be non-readable. Falling back to individual mesh renderers: " + ex.Message);
                    for (int j = content.transform.childCount-1; j >= 0; j--)
                    {
                        var child = content.transform.GetChild(j);
                        var filter = child.GetComponent<MeshFilter>();
                        if (filter != null && filter.sharedMesh != null && filter.sharedMesh.name.StartsWith("_PS_COMBINED_", StringComparison.Ordinal))
                        {
                            if (Application.isPlaying) Destroy(filter.sharedMesh); else DestroyImmediate(filter.sharedMesh);
                        }
                        if (Application.isPlaying) Destroy(child.gameObject); else DestroyImmediate(child.gameObject);
                    }
                    BuildIndividual(parts, content.transform);
                }
            }
            else BuildIndividual(parts, content.transform);
            Debug.Log($"[PS Room] Staged VISUAL ONLY: {source.name} in {slot}, scale {fit:F4}, {parts.Count} source submeshes. Gameplay objects untouched.");
        }

        private void BuildIndividual(List<Part> parts, Transform parent)
        {
            int index = 0;
            foreach (var part in parts)
            {
                var obj = new GameObject("PreviewMesh_" + (index++).ToString("D5"));
                obj.transform.SetParent(parent, false);
                Vector3 pos = part.localMatrix.GetColumn(3);
                obj.transform.localPosition = pos;
                obj.transform.localRotation = part.localMatrix.rotation;
                obj.transform.localScale = part.localMatrix.lossyScale;
                var filter = obj.AddComponent<MeshFilter>();
                filter.sharedMesh = part.mesh;
                var renderer = obj.AddComponent<MeshRenderer>();
                renderer.sharedMaterial = part.material;
                // If source mesh had multiple submeshes, use a temporary single-submesh copy if needed.
                // Most generated Project Spark models have one material per MeshFilter.
            }
        }

        private void BuildCombined(List<Part> parts, Transform parent)
        {
            var groups = new Dictionary<Material, List<Part>>();
            foreach (var p in parts)
            {
                if (!groups.TryGetValue(p.material, out var list))
                {
                    list = new List<Part>();
                    groups[p.material] = list;
                }
                list.Add(p);
            }
            int batchNo = 0;
            foreach (var group in groups)
            {
                var instances = new List<CombineInstance>();
                int vertices = 0;
                foreach (Part p in group.Value)
                {
                    if (vertices + p.mesh.vertexCount > Mathf.Max(10000, maxVerticesPerBatch) && instances.Count > 0)
                    {
                        EmitCombined(parent, group.Key, instances, batchNo++);
                        instances.Clear(); vertices = 0;
                    }
                    instances.Add(new CombineInstance { mesh = p.mesh, subMeshIndex = p.submesh, transform = p.localMatrix });
                    vertices += p.mesh.vertexCount;
                }
                if (instances.Count > 0) EmitCombined(parent, group.Key, instances, batchNo++);
            }
        }

        private static void EmitCombined(Transform parent, Material material, List<CombineInstance> batch, int index)
        {
            var mesh = new Mesh { name = "_PS_COMBINED_" + index.ToString("D3"), indexFormat = IndexFormat.UInt32 };
            mesh.CombineMeshes(batch.ToArray(), true, true, false);
            var obj = new GameObject("OptimizedVisualBatch_" + index.ToString("D3"));
            obj.transform.SetParent(parent, false);
            obj.AddComponent<MeshFilter>().sharedMesh = mesh;
            obj.AddComponent<MeshRenderer>().sharedMaterial = material;
        }
    }
}
