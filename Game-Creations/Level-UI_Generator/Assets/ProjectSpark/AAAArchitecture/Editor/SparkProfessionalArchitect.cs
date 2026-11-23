using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.AAAArchitecture
{
    public static class SparkProfessionalArchitect
    {
        private const string Menu = "Tools/Project Spark/AAA Professional V8/";
        private const string Source = "Assets/ProjectSpark/AAAArchitecture/Editor/ProArtV8/";
        private const string Target = "Assets/ProjectSpark/AAAArchitecture/Generated/Resources/SparkProfessional/";
        private static readonly string[] Roles = { "Shell", "Chamber", "Header", "Rail", "Brief", "Tab", "ActiveTab", "GalleryCard", "Index", "Notice", "Objective", "Action", "Selection", "Fade", "CardFooter", "CardBezel", "LockedIcon", "ReadyIcon", "CompleteIcon", "UnknownIcon", "HeaderPanorama" };
        private static Vector2Int Dimensions(string role) => role == "HeaderPanorama" ? new Vector2Int(1920,240) : role == "Shell" ? new Vector2Int(1920,1080) : role == "Chamber" ? new Vector2Int(1600,900) : role.EndsWith("Icon", StringComparison.Ordinal) ? new Vector2Int(128,128) : new Vector2Int(1024,1024);
        private static bool Simple(string role) => role == "Shell" || role == "Chamber" || role == "Fade" || role == "HeaderPanorama" || role.EndsWith("Icon", StringComparison.Ordinal);
        private static Vector4 Border(string role) => Simple(role) ? Vector4.zero : (role == "Selection" || role == "CardBezel") ? new Vector4(132,132,132,132) : new Vector4(54,54,54,54);
        [MenuItem(Menu + "01 Build complete V8 presentation")]
        public static void Build()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) { Debug.LogWarning("Exit Play Mode before building Spark artwork."); return; }
            // Preflight every input and output before touching any generated asset.
            foreach (string role in Roles)
            {
                var source = AssetDatabase.LoadAssetAtPath<TextAsset>(Source + role + ".bytes");
                if (!source) throw new InvalidOperationException("Missing artwork: " + role);
                var existing = AssetDatabase.LoadMainAssetAtPath(Target + "SkinV8/" + role + ".png");
                if (existing && !(existing is Texture2D)) throw new InvalidOperationException("Occupied output for " + role);
                var check = new Texture2D(2,2);
                try { if (!check.LoadImage(source.bytes) || new Vector2Int(check.width,check.height) != Dimensions(role)) throw new InvalidDataException("Invalid artwork dimensions: " + role); }
                finally { UnityEngine.Object.DestroyImmediate(check); }
            }
            Directory.CreateDirectory(Target + "SkinV8"); AssetDatabase.Refresh();
            foreach (string role in Roles)
            {
                string path = Target + "SkinV8/" + role + ".png";
                var source = AssetDatabase.LoadAssetAtPath<TextAsset>(Source + role + ".bytes");
                File.WriteAllBytes(path, source.bytes);
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (!importer) throw new InvalidOperationException("Cannot import " + path);
                importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Single;
                importer.spritePixelsPerUnit = 320; importer.spriteMeshType = SpriteMeshType.FullRect; importer.spriteBorder = Border(role);
                importer.alphaIsTransparency = true; importer.sRGBTexture = true; importer.mipmapEnabled = false; importer.isReadable = false;
                importer.wrapMode = TextureWrapMode.Clamp; importer.filterMode = FilterMode.Bilinear; importer.npotScale = TextureImporterNPOTScale.None;
                importer.maxTextureSize = 2048; importer.textureCompression = TextureImporterCompression.Uncompressed;
                foreach (string platform in new[] { "Android", "iPhone" })
                {
                    var p = importer.GetPlatformTextureSettings(platform); p.overridden = true; p.maxTextureSize = role.EndsWith("Icon", StringComparison.Ordinal) ? 128 : (Simple(role) && role != "Fade" ? 1024 : 512);
                    p.format = TextureImporterFormat.ASTC_6x6; importer.SetPlatformTextureSettings(p);
                }
                importer.SaveAndReimport();
            }
            EnsureSettings(); AssetDatabase.SaveAssets(); Validate();
        }
        private static SparkProfessionalSettings EnsureSettings()
        {
            string path = Target + "Settings.asset";
            var value = AssetDatabase.LoadAssetAtPath<SparkProfessionalSettings>(path);
            if (value) return value;
            if (AssetDatabase.LoadMainAssetAtPath(path)) throw new InvalidOperationException("Settings path is occupied.");
            Directory.CreateDirectory(Target); AssetDatabase.Refresh();
            value = ScriptableObject.CreateInstance<SparkProfessionalSettings>(); AssetDatabase.CreateAsset(value,path); return value;
        }
        [MenuItem(Menu + "02 Configure professional art and typography")]
        public static void Configure()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            Selection.activeObject = EnsureSettings(); EditorGUIUtility.PingObject(Selection.activeObject);
        }
        [MenuItem(Menu + "03 Validate professional assets")]
        public static void Validate()
        {
            int errors = 0;
            foreach (string role in Roles)
            {
                string path = Target + "SkinV8/" + role + ".png";
                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (!sprite || !importer || importer.spriteImportMode != SpriteImportMode.Single || importer.spriteBorder != Border(role)) { errors++; Debug.LogError("[Spark V8] Invalid sprite: " + path); }
            }
            if (!AssetDatabase.LoadAssetAtPath<SparkProfessionalSettings>(Target+"Settings.asset")) { errors++; Debug.LogError("[Spark V8] Missing settings."); }
            if (errors == 0) Debug.Log("[Spark V8] 21 artwork roles and settings are ready. Validate real appearance in Play Mode.");
        }
        [MenuItem(Menu + "04 Capture actual Game view (Play Mode)")]
        public static void Capture()
        {
            if (!EditorApplication.isPlaying) { Debug.LogWarning("Enter Play Mode, open the campaign and select a mission first."); return; }
            // No camera overrides or artificial data: capture the real rendered Game view.
            string folder = Path.Combine(Directory.GetParent(Application.dataPath).FullName,"Spark_Visual_Verification");
            Directory.CreateDirectory(folder);
            string path = Path.Combine(folder,"Campaign_" + DateTime.Now.ToString("yyyyMMdd_HHmmss_fff") + ".png");
            ScreenCapture.CaptureScreenshot(path,1);
            Debug.Log("[Spark V8] Capture requested for next rendered frame: " + path + ". Check the file after the frame renders.");
        }
    }
}
