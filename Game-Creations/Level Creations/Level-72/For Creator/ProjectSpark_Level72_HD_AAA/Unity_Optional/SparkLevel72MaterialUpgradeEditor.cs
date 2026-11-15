#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace ProjectSpark.EditorTools
{
    /// <summary>Creates editable Unity materials and maps them onto an already placed L72 art root.
    /// Uses available URP Lit or Standard shader. Non-destructive to FBX source files.</summary>
    public static class SparkLevel72MaterialUpgradeEditor
    {
        private const string Folder = "Assets/ProjectSpark/Level72/Materials";
        private static readonly string[] Names = {"navy","steel","light","silver","dark","black","cyan","blue","ice","copper","amber","red","green","white","glass","frost","rubber","gold","violet","plasma","purpleglass","teal","ceramic","iridescent","warning"};
        private static readonly string[] Hex = {"#0E2034","#42657E","#8BB1C3","#B5C7D0","#1B3245","#0D151E","#25D3F6","#1470DA","#85E0F5","#DC7834","#F8A22D","#E23F46","#2FDB8B","#DCEEF8","#5DB2D4","#CDEBF7","#22262A","#E0B044","#AC51F4","#BA49F7","#8A4DD3","#42D5C7","#E6E1DC","#6AC8F2","#FF6947"};
        [MenuItem("Tools/Project Spark/Levels/72/Create & Apply HD Materials To Selected Art")]
        public static void Apply()
        {
            if (Selection.activeGameObject == null)
            {
                EditorUtility.DisplayDialog("Level 72 Materials", "Select L72_HD_VISUAL_ART (or its parent) first.", "OK");
                return;
            }
            EnsureFolder(Folder);
            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            if (shader == null)
            {
                EditorUtility.DisplayDialog("Material Shader Missing", "Neither URP Lit nor Standard shader is available.", "OK");
                return;
            }
            Material[] mats = new Material[Names.Length];
            for (int i = 0; i < Names.Length; i++)
            {
                string path = Folder + "/PS_" + Names[i] + ".mat";
                var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (mat == null)
                {
                    mat = new Material(shader) { name = "PS_" + Names[i] };
                    AssetDatabase.CreateAsset(mat, path);
                }
                Undo.RecordObject(mat, "Set Level 72 material");
                Color color;
                if (!ColorUtility.TryParseHtmlString(Hex[i], out color)) color = Color.gray;
                if (Names[i] == "glass" || Names[i] == "purpleglass" || Names[i] == "plasma") color.a = Names[i] == "plasma" ? .62f : .30f;
                mat.color = color;
                if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
                float metallic = (Names[i] == "steel" || Names[i] == "silver" || Names[i] == "copper" || Names[i] == "gold") ? .82f : .16f;
                if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", metallic);
                if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", .78f);
                if (mat.HasProperty("_Glossiness")) mat.SetFloat("_Glossiness", .78f);
                if (Names[i] == "glass" || Names[i] == "purpleglass" || Names[i] == "plasma")
                {
                    if (mat.HasProperty("_Surface")) mat.SetFloat("_Surface", 1f);
                    if (mat.HasProperty("_Mode")) mat.SetFloat("_Mode", 3f);
                    mat.SetOverrideTag("RenderType", "Transparent");
                    mat.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
                    mat.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
                    mat.SetInt("_ZWrite", 0);
                    mat.EnableKeyword("_ALPHABLEND_ON");
                    mat.renderQueue = (int)RenderQueue.Transparent;
                }
                mats[i] = mat;
                EditorUtility.SetDirty(mat);
            }
            int changed = 0;
            foreach (Renderer r in Selection.activeGameObject.GetComponentsInChildren<Renderer>(true))
            {
                Material[] oldMats = r.sharedMaterials;
                Material[] newMats = new Material[oldMats.Length];
                bool diff = false;
                for (int j = 0; j < oldMats.Length; j++)
                {
                    newMats[j] = oldMats[j];
                    if (oldMats[j] == null) continue;
                    for (int i = 0; i < Names.Length; i++)
                    {
                        if (oldMats[j].name.IndexOf("PS_" + Names[i], StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            if (newMats[j] != mats[i]) { newMats[j] = mats[i]; diff = true; }
                            break;
                        }
                    }
                }
                if (diff) { Undo.RecordObject(r,"Apply Level 72 material"); r.sharedMaterials = newMats; changed++; }
            }
            AssetDatabase.SaveAssets();
            Debug.Log("[L72 Materials] Created/updated " + mats.Length + " editable materials; changed " + changed + " renderers. Glass transparency may need adjustment per render pipeline.");
        }
        private static void EnsureFolder(string path)
        {
            string[] parts = path.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next)) AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }
    }
}
#endif
