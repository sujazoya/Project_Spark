#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ProjectSpark.RoomStudio.EditorTools
{
    /// <summary>
    /// Non-destructive, idempotent Project Spark room builder.
    /// It does NOT find, instantiate, reparent or mutate existing level gameplay prefabs.
    /// Only _PS_ROOM_VISUAL_ART and _PS_ROOM_LIGHTS may be replaced.
    /// </summary>
    public static class PSRoomStudioBuilder
    {
        private const string SceneRoot = "PS_PRO_GAME_ROOM";
        private const string ArtRoot = "_PS_ROOM_VISUAL_ART";
        private const string LightRoot = "_PS_ROOM_LIGHTS";
        private const string PreviewHost = "PS_TABLE_LEVEL_PREVIEW_HOST";

        public static string FindPackageRoot()
        {
            string[] guids = AssetDatabase.FindAssets("00_PS_Studio_FloorSlab");
            foreach (string guid in guids)
            {
                string p = AssetDatabase.GUIDToAssetPath(guid);
                if (Path.GetFileName(p).Equals("00_PS_Studio_FloorSlab.fbx", StringComparison.OrdinalIgnoreCase))
                {
                    return Path.GetDirectoryName(Path.GetDirectoryName(p)).Replace('\\', '/');
                }
            }
            return null;
        }

        [MenuItem("Tools/Project Spark/Room Studio/2. Build or Refresh Full Room")]
        public static void BuildRoom()
        {
            string package = FindPackageRoot();
            if (string.IsNullOrEmpty(package)) { Debug.LogError("[PS Room] Import the complete kit under Assets first."); return; }
            Dictionary<string, Material> materials = PSRoomMaterialInstaller.CreateAll(package);
            TextAsset layout = AssetDatabase.LoadAssetAtPath<TextAsset>(package + "/Docs/Scene_Placement.csv");
            if (layout == null) { Debug.LogError("[PS Room] Missing Scene_Placement.csv"); return; }
            var root = GameObject.Find(SceneRoot);
            if (root == null)
            {
                root = new GameObject(SceneRoot);
                Undo.RegisterCreatedObjectUndo(root, "Create Project Spark Room");
            }
            Transform art = RecreateChild(root.transform, ArtRoot);
            int instances = 0;
            int missing = 0;
            foreach (string line in layout.text.Replace("\r", "").Split('\n'))
            {
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("Model,")) continue;
                string[] cells = line.Split(',');
                if (cells.Length < 6) continue;
                string name = cells[0].Trim();
                var pref = FindModel(package, name);
                if (pref == null) { Debug.LogWarning("[PS Room] Missing FBX model " + name); missing++; continue; }
                GameObject obj = PrefabUtility.InstantiatePrefab(pref, art) as GameObject;
                if (obj == null) { missing++; continue; }
                obj.name = cells[1].Trim() == "Base" ? name : (name + "__" + cells[1].Trim());
                obj.transform.localPosition = new Vector3(Parse(cells[2]), Parse(cells[3]), Parse(cells[4]));
                obj.transform.localRotation = Quaternion.Euler(0, Parse(cells[5]), 0);
                // Preserve model importer scale, which handles centimetre FBX units.
                PSRoomMaterialInstaller.ApplyMaterials(obj, materials);
                instances++;
            }
            AddSimpleColliders(root.transform);
            AddLighting(root.transform);
            EnsurePreviewHost(root.transform);
            Selection.activeGameObject = root;
            EditorSceneManager.MarkSceneDirty(root.scene);
            Debug.Log($"[PS Room] Built {instances} modular visual instances ({missing} missing). 18.2m x 14.4m room, 9.9m table, three chairs, one dustbin, one planter. " +
                "Table previews accept arbitrary Project Spark level art without gameplay scripts.");
        }

        [MenuItem("Tools/Project Spark/Room Studio/3. Save Current Room As Prefab")]
        public static void SaveRoomPrefab()
        {
            GameObject room = GameObject.Find(SceneRoot);
            if (room == null) { Debug.LogWarning("Build the room first."); return; }
            string path = "Assets/ProjectSpark/RoomStudio/PS_ProGameRoom.prefab";
            PSRoomMaterialInstaller.EnsureFolder("Assets/ProjectSpark/RoomStudio");
            PrefabUtility.SaveAsPrefabAsset(room, path);
            Debug.Log("[PS Room] Saved prefab: " + path);
        }

        [MenuItem("Tools/Project Spark/Room Studio/4. Preview Selected Level / Full Table")]
        public static void StageSelectedFull() { StageSelected(-1); }
        [MenuItem("Tools/Project Spark/Room Studio/5. Preview Selected Level / Left Bay")]
        public static void StageSelectedLeft() { StageSelected(0); }
        [MenuItem("Tools/Project Spark/Room Studio/6. Preview Selected Level / Center Bay")]
        public static void StageSelectedCenter() { StageSelected(1); }
        [MenuItem("Tools/Project Spark/Room Studio/7. Preview Selected Level / Right Bay")]
        public static void StageSelectedRight() { StageSelected(2); }

        private static void StageSelected(int bay)
        {
            GameObject room = GameObject.Find(SceneRoot);
            if (room == null) { Debug.LogWarning("Build the room first."); return; }
            var host = room.GetComponentInChildren<PSRoomPreviewHost>();
            if (host == null) { Debug.LogWarning("Room preview host not found."); return; }
            GameObject selected = Selection.activeGameObject;
            if (selected == null || selected == room || selected.transform.IsChildOf(room.transform))
            {
                Debug.LogWarning("[PS Room] First select a level's assembled ART prefab, FBX or visual GameObject outside the room, then run this menu item.");
                return;
            }
            if (bay < 0) host.SetSingleLevel(selected);
            else host.SetBayLevel(bay, selected);
            EditorUtility.SetDirty(host);
            EditorSceneManager.MarkSceneDirty(room.scene);
            Debug.Log("[PS Room] Level preview was generated from static mesh renderers only. No electrical or gameplay components were cloned.");
        }

        private static float Parse(string s)
        {
            return float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out var f) ? f : 0;
        }
        private static GameObject FindModel(string root, string stem)
        {
            string path = root + "/Models/";
            // Avoid depending on exact number; art manifest carries the model names.
            string[] guids = AssetDatabase.FindAssets(stem + " t:Model", new[] { path });
            foreach (string guid in guids)
            {
                string p = AssetDatabase.GUIDToAssetPath(guid);
                if (Path.GetFileNameWithoutExtension(p).EndsWith("_" + stem, StringComparison.OrdinalIgnoreCase)
                    && p.EndsWith(".fbx",StringComparison.OrdinalIgnoreCase))
                    return AssetDatabase.LoadAssetAtPath<GameObject>(p);
            }
            // Fallback is exact directory scan; no other level folders are searched.
            foreach (string p in Directory.GetFiles(path, "*.fbx"))
            {
                string projectPath = p.Replace('\\','/');
                if (Path.GetFileNameWithoutExtension(projectPath).EndsWith("_"+stem, StringComparison.OrdinalIgnoreCase))
                    return AssetDatabase.LoadAssetAtPath<GameObject>(projectPath);
            }
            return null;
        }

        private static Transform RecreateChild(Transform parent, string name)
        {
            Transform old = parent.Find(name);
            if (old != null) Undo.DestroyObjectImmediate(old.gameObject);
            var go = new GameObject(name);
            Undo.RegisterCreatedObjectUndo(go, "Build Room Visuals");
            go.transform.SetParent(parent, false);
            return go.transform;
        }

        private static void AddSimpleColliders(Transform root)
        {
            Transform node = RecreateChild(root,"_PS_ROOM_WALKABLE_COLLIDERS");
            AddCollider(node, "Floor", new Vector3(0,-.27f,0),new Vector3(18.2f,.48f,14.4f));
            AddCollider(node, "CentralTable",new Vector3(0,.61f,.30f),new Vector3(9.8f,1.22f,5.3f));
            AddCollider(node, "LeftChair",new Vector3(-6.06f,.67f,3.86f),new Vector3(1.0f,1.34f,1.05f));
            AddCollider(node, "RightChair",new Vector3(6.10f,.67f,3.90f),new Vector3(1.0f,1.34f,1.05f));
            AddCollider(node, "RearChair",new Vector3(0,.67f,-4.35f),new Vector3(1.05f,1.34f,1.0f));
            AddCollider(node, "WasteBin",new Vector3(7.85f,.36f,-3.50f),new Vector3(.56f,.72f,.56f));
            // No high-poly mesh colliders; preview meshes remain visual-only.
        }
        private static void AddCollider(Transform parent,string name,Vector3 center,Vector3 size)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent,false);
            go.transform.localPosition=center;
            var box=go.AddComponent<BoxCollider>();
            box.size=size;
        }
        private static void EnsurePreviewHost(Transform root)
        {
            Transform node = root.Find(PreviewHost);
            if (node == null)
            {
                var go = new GameObject(PreviewHost);
                go.transform.SetParent(root,false);
                go.AddComponent<PSRoomPreviewHost>();
            }
            else if (node.GetComponent<PSRoomPreviewHost>() == null)
                node.gameObject.AddComponent<PSRoomPreviewHost>();
        }
        private static void AddLighting(Transform root)
        {
            Transform lights = RecreateChild(root,LightRoot);
            MakeLight(lights,"Room_Key_Area",LightType.Spot,new Vector3(-4,5.75f,0),new Vector3(0,1.1f,0),new Color(0.9f,0.94f,1f),3.6f,11f,105f);
            MakeLight(lights,"Room_Fill_Area",LightType.Spot,new Vector3(4,5.75f,0),new Vector3(0,1.1f,0),new Color(.70f,.87f,1f),2.9f,11f,105f);
            MakeLight(lights,"Table_Warm_Fill",LightType.Spot,new Vector3(0,5.4f,3.5f),new Vector3(0,1.1f,0),new Color(1f,.72f,.46f),2.0f,8f,92f);
            MakeLight(lights,"Table_Cool_Fill",LightType.Spot,new Vector3(0,5.4f,-3.5f),new Vector3(0,1.1f,0),new Color(.25f,.81f,1f),1.5f,8f,92f);
            MakeLight(lights,"Window_Light",LightType.Directional,new Vector3(0,5f,-5f),new Vector3(0,0,0),new Color(.72f,.84f,1f),.6f,0,0);
        }
        private static void MakeLight(Transform parent,string name,LightType type,Vector3 origin,Vector3 target,Color color,float intensity,float range,float angle)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent,false);
            go.transform.localPosition=origin;
            go.transform.rotation=Quaternion.LookRotation((parent.TransformPoint(target)-go.transform.position).normalized);
            var l=go.AddComponent<Light>();
            l.type=type; l.color=color; l.intensity=intensity;
            if (type==LightType.Spot) { l.range=range; l.spotAngle=angle; }
            l.shadows=LightShadows.Soft;
            l.shadowStrength=.55f;
        }
    }
}
#endif
