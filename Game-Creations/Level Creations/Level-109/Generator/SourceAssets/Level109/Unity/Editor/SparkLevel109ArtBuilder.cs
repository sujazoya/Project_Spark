#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.Editor
{
    public static class SparkLevel109ArtBuilder
    {
        [Serializable] private class ModelRow { public string model, type, fbx; public float x_mm, y_mm, z_mm; public bool installable; }
        // Export the CSV into this same package directory under Assets and run the menu action.
        [MenuItem("Tools/Project Spark/Levels/109/Build TRX-17 Visual Reference")]
        public static void Build()
        {
            string[] guids = AssetDatabase.FindAssets("L109_Models_160 t:TextAsset");
            if (guids.Length == 0) { Debug.LogError("L109_Models_160.csv not found in Assets."); return; }
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            string dir = Path.GetDirectoryName(Path.GetDirectoryName(path));
            var go = new GameObject("L109_TRX17_VISUAL_REFERENCE_NOT_GAMEPLAY");
            string[] lines = File.ReadAllLines(path);
            if (lines.Length < 2) { UnityEngine.Object.DestroyImmediate(go); return; }
            var h = lines[0].Split(',');
            int column(string key) { for(int i=0;i<h.Length;i++) if(h[i]==key) return i; return -1; }
            int nameIdx=column("model"), fbxIdx=column("fbx"), xIdx=column("x_mm"), yIdx=column("y_mm"), zIdx=column("z_mm");
            for (int line=1;line<lines.Length;line++)
            {
                var v=lines[line].Split(',');
                if(v.Length<h.Length) continue;
                var prefab=AssetDatabase.LoadAssetAtPath<GameObject>((dir+"/"+v[fbxIdx]).Replace('\\','/'));
                if(prefab==null){Debug.LogWarning("Missing model: "+v[nameIdx]);continue;}
                GameObject obj=(GameObject)PrefabUtility.InstantiatePrefab(prefab);
                obj.name=v[nameIdx];obj.transform.SetParent(go.transform,false);
                // FBX authoring units are mm. Unity FBX default importer scale may require adjustment.
                if(float.TryParse(v[xIdx],System.Globalization.NumberStyles.Float,System.Globalization.CultureInfo.InvariantCulture,out float x) &&
                   float.TryParse(v[yIdx],System.Globalization.NumberStyles.Float,System.Globalization.CultureInfo.InvariantCulture,out float y) &&
                   float.TryParse(v[zIdx],System.Globalization.NumberStyles.Float,System.Globalization.CultureInfo.InvariantCulture,out float z))
                    obj.transform.localPosition=new Vector3(x*.001f,z*.001f,y*.001f);
                // Importer Z-up/Y-up rotations must be inspected for your Unity version.
            }
            Selection.activeGameObject=go;
            Debug.Log("Level 109 ART reference created; does not install electrical parts, progress or objectives.");
        }
    }
}
#endif
