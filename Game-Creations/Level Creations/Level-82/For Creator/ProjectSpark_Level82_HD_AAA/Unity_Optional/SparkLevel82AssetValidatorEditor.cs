#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.EditorTools
{
    public static class SparkLevel82AssetValidatorEditor
    {
        [MenuItem("Tools/Project Spark/Levels/82/Validate Imported HD Art")]
        public static void Validate()
        {
            int count = 0;
            var indices = new HashSet<int>();
            foreach (string guid in AssetDatabase.FindAssets("t:Model"))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (!path.EndsWith(".fbx", StringComparison.OrdinalIgnoreCase)) continue;
                string f = System.IO.Path.GetFileNameWithoutExtension(path);
                if (f.Length<8 || !f.Contains("_L82_") || !int.TryParse(f.Substring(0,2),out int ix) || ix<0 || ix>=72) continue;
                if(f[2]!='_') continue;
                if (AssetDatabase.LoadAssetAtPath<GameObject>(path)==null) continue;
                if(indices.Add(ix))count++;
            }
            Transform root = Selection.activeTransform;
            Transform art = root!=null && root.name=="L82_HD_VISUAL_ART" ? root : root!=null ? root.Find("L82_HD_VISUAL_ART") : null;
            int instantiated = art==null ? 0 : art.childCount;
            var pivotNames = new HashSet<string>();
            if (art != null)
            {
                foreach (Transform t in art.GetComponentsInChildren<Transform>(true))
                    if (t.name.StartsWith("ANIM_", StringComparison.Ordinal)) pivotNames.Add(t.name);
            }
            string msg="FBX model indices found: "+count+"/72\n" +
                       "Models instantiated: "+instantiated+"/72\n"+
                       "Named animation pivots: "+pivotNames.Count+"/54\n"+
                       "Expected assembled width (after importer conversion): approximately 25.40 metres.\n"+
                       "No gameplay wires or SparkTerminals are included in the art package.";
            if(art!=null)
            {
                bool found=false;Bounds b=new Bounds();
                foreach(Renderer r in art.GetComponentsInChildren<Renderer>(true))
                { if(!found){b=r.bounds;found=true;}else b.Encapsulate(r.bounds); }
                if(found) msg+="\nCurrent combined rendered width: "+b.size.x.ToString("F2")+" Unity units.";
            }
            Debug.Log("[Level 82 Art Validator] "+msg);
            EditorUtility.DisplayDialog("Project Spark Level 82 Art Check",msg,"OK");
        }
    }
}
#endif
