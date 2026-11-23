using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.AAAArchitecture
{
    // Deterministic sliced UI sprites and shared cinematic materials.
    public static class SparkCinematicUIArchitect
    {
        private const string Menu = "Tools/Project Spark/AAA Cinematic UI/";
        private const string Root = "Assets/ProjectSpark/AAAArchitecture/Generated/Resources/SparkCinematicUI";
        private const string ArtRoot = "Assets/ProjectSpark/AAAArchitecture/Editor/StudioArt";
        private const string ShaderPath = "Assets/ProjectSpark/AAAArchitecture/Shaders/SparkCinematicSurface.shader";
        private const string ReducedMotionPref = "ProjectSpark.CinematicUI.ReducedMotion";
        private static readonly string[] Names = { "Backdrop", "Panel", "Well", "Card", "Frame", "Button", "Primary", "Badge", "Hero", "Grid", "Scan" };
        [MenuItem(Menu + "01 Build cinematic skin")]
        public static void BuildSkin()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode) { Debug.LogWarning("Exit Play Mode before generating assets."); return; }
            Directory.CreateDirectory(Root);
            // Validate the complete source set before replacing any generated asset.
            foreach (string name in Names) VerifySource(name);
            foreach (string name in Names) Generate(name);
            BuildMaterials();
            SparkCinematicSkin.ClearCache();
            AssetDatabase.SaveAssets();
            Debug.Log("[Spark Cinematic] 11 Studio HD skin sprites and shared shader materials built (see validation). Existing campaign applies these automatically on next initialization.");
        }
        [MenuItem(Menu + "02 Build complete AAA presentation")]
        public static void BuildComplete()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (!EditorUtility.DisplayDialog("Build cinematic interface", "Regenerate the owned AAA preview assets and cinematic skin? The existing gameplay campaign reads the new skin at runtime. Original levels and scenes are untouched.", "Build", "Cancel")) return;
            BuildSkin();
            // Use existing architect, retaining the catalog and gateway. Reflection avoids dependence on its editor namespace.
            Type architect = null;
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                architect = asm.GetType("ProjectSpark.AAAArchitecture.Editor.SparkAAAArchitect") ?? asm.GetType("ProjectSpark.AAAArchitecture.SparkAAAArchitect");
                if (architect != null) break;
            }
            var build = architect?.GetMethod("Build", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static, null, Type.EmptyTypes, null);
            if (build == null) { Debug.LogWarning("[Spark Cinematic] Skin ready. Run the existing AAA Level Architect build manually."); return; }
            build.Invoke(null, null);
            StylePreviewPrefab();
            Validate();
        }
        private static void StylePreviewPrefab()
        {
            string path = "Assets/ProjectSpark/AAAArchitecture/Generated/Prefabs/SparkLevelSelectionPanel.prefab";
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (!asset || !asset.GetComponent<SparkAAAMissionPanel>()) return;
            var root = PrefabUtility.LoadPrefabContents(path);
            try { SparkCinematicSkin.StyleGeneratedPresentation(root); PrefabUtility.SaveAsPrefabAsset(root, path); }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }
        [MenuItem(Menu + "03 Validate cinematic assets")]
        public static void Validate()
        {
            int good = 0;
            foreach (string name in Names)
            {
                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(Root + "/" + name + ".png");
                if (sprite && sprite.texture.width == sprite.texture.height && (sprite.texture.width == SizeFor(name) || (SizeFor(name) == 1024 && sprite.texture.width == 512))) good++;
                else Debug.LogError("[Spark Cinematic] Missing or invalid sprite: " + name);
            }
            int materialCount = 0;
            var shader = AssetDatabase.LoadAssetAtPath<Shader>(ShaderPath);
            foreach(string role in SparkCinematicSkin.ShaderRoles)
            {
                var mat=AssetDatabase.LoadAssetAtPath<Material>(Root+"/Materials/"+role+".mat");
                if(mat && shader && mat.shader==shader && shader.isSupported) materialCount++;
                else Debug.LogError("[Spark Cinematic] Missing/unsupported shader material: " + role);
            }
            Debug.Log($"[Spark Cinematic] {good}/{Names.Length} sprites and {materialCount}/{SparkCinematicSkin.ShaderRoles.Length} materials validated. Inspect shader compiler messages and verify in Play Mode.");
        }

        [MenuItem(Menu + "04 Build shader materials only")]
        public static void BuildMaterials()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode) return;
            var shader = AssetDatabase.LoadAssetAtPath<Shader>(ShaderPath);
            if (!shader || !shader.isSupported)
            {
                Debug.LogError("[Spark Cinematic] Shader missing or unsupported; check Shader Inspector. Sprite fallback remains available.");
                return;
            }
            string folder=Root+"/Materials";
            Directory.CreateDirectory(folder);
            AssetDatabase.Refresh();
            bool reduced=EditorPrefs.GetBool(ReducedMotionPref,false);
            foreach (string role in SparkCinematicSkin.ShaderRoles)
            {
                string path=folder+"/"+role+".mat";
                var mat=AssetDatabase.LoadAssetAtPath<Material>(path);
                if(!mat) { mat=new Material(shader); AssetDatabase.CreateAsset(mat,path); }
                else mat.shader=shader;
                bool frame=role=="Frame", backdrop=role=="Backdrop", hero=role=="Hero"||role=="Well", gold=role=="Primary";
                mat.SetFloat("_Role",frame?1:backdrop?2:hero?3:0);
                mat.SetFloat("_Motion",reduced?0:1);
                mat.SetFloat("_Glow",frame?1.70f:.7f);
                mat.SetFloat("_Metal",gold?1f:role=="Card"?.78f:.55f);
                mat.SetFloat("_Grid",backdrop?.9f:.46f);
                mat.SetColor("_Accent",gold?new Color(1,.63f,.22f):new Color(.20f,.77f,1));
                EditorUtility.SetDirty(mat);
            }
            AssetDatabase.SaveAssets();
            SparkCinematicSkin.ClearCache();
        }
        [MenuItem(Menu + "05 Toggle reduced shader motion")]
        public static void ToggleMotion()
        {
            bool state=!EditorPrefs.GetBool(ReducedMotionPref,false);
            EditorPrefs.SetBool(ReducedMotionPref,state);
            BuildMaterials();
            Debug.Log("[Spark Cinematic] Shader motion " + (state?"reduced":"enabled") + ". Card hover animation is unchanged.");
        }
        [MenuItem(Menu + "05 Toggle reduced shader motion",true)]
        private static bool CheckMotion()
        {
            UnityEditor.Menu.SetChecked(Menu + "05 Toggle reduced shader motion",EditorPrefs.GetBool(ReducedMotionPref,false));
            return !EditorApplication.isPlayingOrWillChangePlaymode;
        }
        private static int SizeFor(string name) => name == "Grid" || name == "Scan" ? 256 : 1024;
        private static void VerifySource(string name)
        {
            var source = AssetDatabase.LoadAssetAtPath<TextAsset>(ArtRoot + "/" + name + ".bytes");
            if (!source) throw new InvalidOperationException("Missing Studio V5 artwork: " + name);
            var probe = new Texture2D(2, 2);
            try
            {
                if (!ImageConversion.LoadImage(probe, source.bytes, true) || probe.width != SizeFor(name) || probe.height != SizeFor(name))
                    throw new InvalidOperationException("Invalid Studio V5 artwork dimensions: " + name);
            }
            finally { UnityEngine.Object.DestroyImmediate(probe); }
        }
        private static void Generate(string name)
        {
            var source = AssetDatabase.LoadAssetAtPath<TextAsset>(ArtRoot + "/" + name + ".bytes");
            string path = Root + "/" + name + ".png";
            File.WriteAllBytes(path, source.bytes);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            if (!importer) throw new InvalidOperationException("Failed to import " + path);
            importer.textureType=TextureImporterType.Sprite; importer.spriteImportMode=SpriteImportMode.Single;
            var settings = new TextureImporterSettings(); importer.ReadTextureSettings(settings);
            settings.spriteMeshType=SpriteMeshType.FullRect; importer.SetTextureSettings(settings);
            importer.spritePixelsPerUnit=SizeFor(name) == 1024 ? 400 : 100;
            importer.spriteBorder = name == "Backdrop" || name == "Grid" || name == "Scan" ? Vector4.zero : new Vector4(128,128,128,128);
            importer.mipmapEnabled=false; importer.alphaIsTransparency=true; importer.sRGBTexture=true;
            importer.wrapMode=TextureWrapMode.Clamp; importer.filterMode=FilterMode.Bilinear;
            importer.textureCompression=TextureImporterCompression.CompressedHQ; importer.maxTextureSize=SizeFor(name);
            // Explicit platform override avoids stale V4 texture limits; retain platform-specific format selection.
            foreach (string platform in new[] { "Android", "iPhone", "Standalone" })
            {
                var ps = importer.GetPlatformTextureSettings(platform);
                if (platform == "Standalone" && !ps.overridden) continue;
                if (!ps.overridden) { ps.overridden = true; ps.format = TextureImporterFormat.Automatic; }
                ps.maxTextureSize = platform == "Standalone" ? SizeFor(name) : Mathf.Min(512,SizeFor(name));
                importer.SetPlatformTextureSettings(ps);
            }
            importer.SaveAndReimport();
        }
    }
}
