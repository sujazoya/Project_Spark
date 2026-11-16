#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.RoomStudio.EditorTools
{
    public static class PSRoomValidator
    {
        [MenuItem("Tools/Project Spark/Room Studio/8. Validate Imported 3D Kit")]
        public static void Validate()
        {
            var notes = new List<string>();
            string root = PSRoomStudioBuilder.FindPackageRoot();
            if (string.IsNullOrEmpty(root)) { Debug.LogError("[PS Room] Package not found."); return; }
            int models = 0, faces = 0, meshParts = 0, missingUV = 0;
            var foundPivots = new HashSet<string>();
            foreach (string path in Directory.GetFiles(root + "/Models", "*.fbx"))
            {
                if (Path.GetFileName(path).Contains("COMPLETE_Showcase")) continue;
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path.Replace('\\','/'));
                if (prefab == null) { notes.Add("MISSING/NOT IMPORTED: " + path); continue; }
                models++;
                foreach (Transform t in prefab.GetComponentsInChildren<Transform>(true))
                    if (t.name.StartsWith("ANIM_")) foundPivots.Add(t.name);
                foreach (var filter in prefab.GetComponentsInChildren<MeshFilter>(true))
                {
                    if (filter.sharedMesh == null) continue;
                    meshParts++;
                    for (int sm = 0; sm < filter.sharedMesh.subMeshCount; sm++)
                        faces += (int)(filter.sharedMesh.GetIndexCount(sm) / 3);
                    try
                    {
                        if (filter.sharedMesh.uv == null || filter.sharedMesh.uv.Length != filter.sharedMesh.vertexCount)
                        {
                            missingUV++;
                            notes.Add("UV missing on: " + path + "/" + filter.name);
                        }
                    }
                    catch (Exception)
                    {
                        notes.Add("Mesh not readable to validator (enable Read/Write for preview batching): " + path);
                    }
                }
            }
            int textures = Directory.GetFiles(root + "/Textures", "*.png").Length;
            int shaders = Directory.GetFiles(root + "/Shaders", "*.shader").Length;
            int mats = AssetDatabase.FindAssets("t:Material",new[]{PSRoomMaterialInstaller.GeneratedFolder}).Length;
            foreach (string p in new[]{"ANIM_STUDIO_HALO_LIGHT","ANIM_STUDIO_ENTRY_DOOR","ANIM_TABLE_ACCESS_LEFT","ANIM_TABLE_ACCESS_RIGHT"})
                if (!foundPivots.Contains(p)) notes.Add("Missing pivot: " + p);
            if (models != 37) notes.Add("Expected 37 numbered FBX models, found " + models);
            if (textures != 85) notes.Add("Expected 85 PBR texture maps, found " + textures);
            if (shaders < 2) notes.Add("Expected Built-in + URP hologram shaders");
            if (mats < 17) notes.Add("Create/upgrade the 17 Unity materials using menu item 1.");
            var room = GameObject.Find("PS_PRO_GAME_ROOM");
            if (room != null && room.GetComponentInChildren<PSRoomPreviewHost>() == null)
                notes.Add("Current room scene lacks preview host");
            Debug.Log($"[PS Room Validator] {models}/37 modular FBX; {meshParts} mesh parts; {faces} triangles; " +
              $"{foundPivots.Count}/4 pivots; {textures}/85 maps; {shaders}/2 shaders; {mats}/17 generated Unity .mat files; {missingUV} UV failures. " +
              (notes.Count == 0 ? "PASS (structural checks only)" : "ACTION REQUIRED: " + string.Join(" | ",notes)));
        }
    }
}
#endif
