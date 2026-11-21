using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.EditorTools.ThermalEndurance
{
    internal static partial class SparkLevel109Builder
    {
        private static void BindSurfaceTextures(Dictionary<string, Material> materials)
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (!shader) throw new InvalidOperationException("TRX-17 requires the project's URP Lit shader.");
            foreach (var (family, material) in materials)
            {
                string prefix = "Assets/ProjectSpark/Level109/Textures/L109_" + family;
                var color = AssetDatabase.LoadAssetAtPath<Texture2D>(prefix + "_BaseColor.png");
                string normalPath = prefix + "_Normal.png";
                if (AssetImporter.GetAtPath(normalPath) is TextureImporter importer &&
                    importer.textureType != TextureImporterType.NormalMap)
                {
                    importer.textureType = TextureImporterType.NormalMap;
                    importer.SaveAndReimport();
                }
                var normal = AssetDatabase.LoadAssetAtPath<Texture2D>(normalPath);
                material.shader = shader;
                material.SetTexture("_BaseMap", color);
                material.SetColor("_BaseColor", color ? Color.white : FallbackColor(family));
                material.SetTexture("_BumpMap", normal);
                material.SetFloat("_BumpScale", .42f);
                if (normal) material.EnableKeyword("_NORMALMAP");
                else material.DisableKeyword("_NORMALMAP");
                EditorUtility.SetDirty(material);
                AssetDatabase.SaveAssetIfDirty(material);
            }
        }

        private static Color FallbackColor(string family) => family switch
        {
            "BLUE" => new Color(.09f, .22f, .38f),
            "PURPLE" => new Color(.32f, .21f, .43f),
            "WHITE" => new Color(.85f, .89f, .91f),
            "RUBBER" => new Color(.07f, .075f, .083f),
            _ => new Color(.10f, .11f, .13f)
        };
    }
}
