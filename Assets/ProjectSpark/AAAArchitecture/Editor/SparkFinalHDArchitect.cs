using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using ProjectSpark.AAAArchitecture.Editor;

namespace ProjectSpark.AAAArchitecture
{
    public static class SparkFinalHDArchitect
    {
        private const string Menu = "Tools/Project Spark/AAA Final HD/";
        private const string Root = "Assets/ProjectSpark/AAAArchitecture/Generated/Resources/SparkFinalHD";
        private const string StudioMenu = "Tools/Project Spark/AAA Studio V5/";
        private const string StudioArt = "Assets/ProjectSpark/AAAArchitecture/Editor/StudioArt/LaboratoryHD.bytes";
        private const string SettingsPath = Root + "/Settings.asset";
        [MenuItem(StudioMenu + "01 Build complete Studio V5")]
        public static void BuildStudio() => Build();
        [MenuItem(StudioMenu + "02 Build Studio skin only")]
        public static void BuildStudioSkin() => BuildVisuals();
        [MenuItem(StudioMenu + "03 Configure Studio appearance")]
        public static void ConfigureStudio() => Configure();
        [MenuItem(StudioMenu + "04 Validate Studio assets")]
        public static void ValidateStudio() => Validate();
        [MenuItem(Menu + "01 Build COMPLETE Final HD")]
        public static void Build()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode) return;
            if(!EditorUtility.DisplayDialog("Project Spark Final HD", "Regenerate owned cinematic skin, chapter icons, background and approved live-preview manifests? Original levels, progress and gameplay scenes are not edited.","Build","Cancel")) return;
            SparkLivePreviewArchitect.Build();
            BuildVisuals();
            Validate();
        }
        [MenuItem(Menu + "02 Build HD skin and icons only")]
        public static void BuildVisuals()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode) return;
            SparkCinematicUIArchitect.BuildSkin();
            Directory.CreateDirectory(Root+"/Icons"); AssetDatabase.Refresh();
            var s=Settings();
            if(s.chapterIcons==null || s.chapterIcons.Length!=13) Array.Resize(ref s.chapterIcons,13);
            for(int i=0;i<13;i++)
            {
                string path=Root+"/Icons/Chapter_"+(i+1).ToString("00")+".png";
                GenerateIcon(i+1,path);
                if(!s.chapterIcons[i])s.chapterIcons[i]=AssetDatabase.LoadAssetAtPath<Sprite>(path);
            }
            string backdrop=Root+"/LaboratoryHD.png";
            if(!s.laboratoryBackdrop || AssetDatabase.GetAssetPath(s.laboratoryBackdrop) == backdrop)
            {
                GenerateBackdrop(backdrop); s.laboratoryBackdrop=AssetDatabase.LoadAssetAtPath<Sprite>(backdrop);
            }
            EditorUtility.SetDirty(s); AssetDatabase.SaveAssets();
            Debug.Log("[Spark Final HD] HD sprites, 13 chapter symbols and backdrop ready. Custom settings retained.");
        }
        private static SparkFinalPresentationSettings Settings()
        {
            var s=AssetDatabase.LoadAssetAtPath<SparkFinalPresentationSettings>(SettingsPath);
            if(s)return s;
            if(AssetDatabase.LoadMainAssetAtPath(SettingsPath))throw new InvalidOperationException("Final HD settings path occupied.");
            Directory.CreateDirectory(Root);AssetDatabase.Refresh();s=ScriptableObject.CreateInstance<SparkFinalPresentationSettings>();AssetDatabase.CreateAsset(s,SettingsPath);return s;
        }
        [MenuItem(Menu+"03 Configure Final HD")]
        public static void Configure(){if(!EditorApplication.isPlayingOrWillChangePlaymode)Selection.activeObject=Settings();}
        [MenuItem(Menu+"04 Validate Final HD")]
        public static void Validate()
        {
            SparkCinematicUIArchitect.Validate();
            var s=AssetDatabase.LoadAssetAtPath<SparkFinalPresentationSettings>(SettingsPath);
            if(!s){Debug.LogError("[Spark Final HD] Missing settings; build first.");return;}
            int count=0;
            if(s.chapterIcons!=null)foreach(var icon in s.chapterIcons)if(icon)count++;
            if(count!=13||!s.laboratoryBackdrop)Debug.LogError("[Spark Final HD] Missing icon or backdrop.");
            else Debug.Log("[Spark Final HD] 13 chapter icons and laboratory backdrop validated. Test the actual campaign in Play Mode.");
        }
        // A small vector rasterizer for tintable, anti-aliased symbols. No fonts or third-party artwork required.
        private static void GenerateIcon(int kind,string path)
        {
            const int n=128; var pixels=new Color32[n*n];
            for(int y=0;y<n;y++)for(int x=0;x<n;x++)
            {
                Vector2 p=new Vector2((x+.5f)/n,(y+.5f)/n);float d=100;
                void Line(float ax,float ay,float bx,float by){Vector2 a=new Vector2(ax,ay),b=new Vector2(bx,by),v=b-a;float t=Mathf.Clamp01(Vector2.Dot(p-a,v)/Mathf.Max(.0001f,v.sqrMagnitude));d=Mathf.Min(d,(p-a-v*t).magnitude);}
                void Ring(float cx,float cy,float radius){d=Mathf.Min(d,Mathf.Abs((p-new Vector2(cx,cy)).magnitude-radius));}
                void Rect(float l,float b,float r,float t){Line(l,b,r,b);Line(r,b,r,t);Line(r,t,l,t);Line(l,t,l,b);}
                void Chip(){Rect(.27f,.27f,.73f,.73f);Rect(.38f,.38f,.62f,.62f);for(int i=0;i<5;i++){float q=.3f+i*.1f;Line(q,.17f,q,.27f);Line(q,.73f,q,.83f);Line(.17f,q,.27f,q);Line(.73f,q,.83f,q);}}
                switch(kind)
                {
                    case 1: Line(.55f,.88f,.25f,.47f);Line(.25f,.47f,.48f,.47f);Line(.48f,.47f,.38f,.12f);Line(.38f,.12f,.76f,.61f);Line(.76f,.61f,.53f,.61f);Line(.53f,.61f,.55f,.88f);break;
                    case 2: Line(.12f,.5f,.27f,.5f);Line(.27f,.5f,.33f,.65f);Line(.33f,.65f,.43f,.35f);Line(.43f,.35f,.53f,.65f);Line(.53f,.65f,.63f,.35f);Line(.63f,.35f,.73f,.5f);Line(.73f,.5f,.88f,.5f);break;
                    case 3: Ring(.5f,.5f,.33f);Line(.5f,.5f,.70f,.72f);Ring(.5f,.5f,.045f);Line(.5f,.77f,.5f,.84f);Line(.18f,.5f,.26f,.5f);Line(.74f,.5f,.82f,.5f);break;
                    case 4: Rect(.17f,.16f,.83f,.26f);Ring(.35f,.36f,.08f);Ring(.62f,.70f,.08f);Line(.35f,.36f,.45f,.57f);Line(.45f,.57f,.62f,.70f);Line(.62f,.70f,.78f,.57f);Line(.78f,.57f,.78f,.44f);break;
                    case 5: Ring(.5f,.5f,.25f);Ring(.5f,.5f,.10f);for(int j=0;j<8;j++){float a=j*Mathf.PI/4;Line(.5f+.25f*Mathf.Cos(a),.5f+.25f*Mathf.Sin(a),.5f+.37f*Mathf.Cos(a),.5f+.37f*Mathf.Sin(a));}break;
                    case 6: Line(.5f,.9f,.25f,.13f);Line(.5f,.9f,.75f,.13f);Line(.22f,.66f,.78f,.66f);Line(.15f,.46f,.85f,.46f);Line(.31f,.32f,.69f,.32f);Line(.29f,.16f,.71f,.32f);Line(.71f,.16f,.29f,.32f);break;
                    case 7: Rect(.15f,.17f,.85f,.55f);Rect(.20f,.55f,.32f,.84f);Line(.32f,.55f,.50f,.72f);Line(.50f,.72f,.50f,.55f);Line(.50f,.55f,.69f,.72f);Line(.69f,.72f,.69f,.55f);Rect(.28f,.25f,.40f,.39f);Rect(.57f,.25f,.69f,.39f);break;
                    case 8: Chip();break;
                    case 9: Rect(.38f,.38f,.62f,.62f);Rect(.12f,.42f,.32f,.58f);Rect(.68f,.42f,.88f,.58f);Line(.32f,.5f,.38f,.5f);Line(.62f,.5f,.68f,.5f);Line(.5f,.62f,.5f,.80f);Ring(.5f,.81f,.06f);break;
                    case 10: Rect(.13f,.13f,.87f,.87f);Ring(.28f,.28f,.06f);Ring(.72f,.72f,.06f);Ring(.28f,.72f,.06f);Line(.28f,.34f,.28f,.55f);Line(.28f,.55f,.60f,.55f);Line(.60f,.55f,.72f,.66f);Line(.34f,.72f,.48f,.72f);Line(.48f,.72f,.48f,.27f);Line(.48f,.27f,.73f,.27f);break;
                    case 11: Line(.5f,.87f,.17f,.72f);Line(.17f,.72f,.24f,.31f);Line(.24f,.31f,.5f,.12f);Line(.5f,.12f,.76f,.31f);Line(.76f,.31f,.83f,.72f);Line(.83f,.72f,.5f,.87f);Line(.34f,.5f,.46f,.37f);Line(.46f,.37f,.68f,.64f);break;
                    case 12: for(int j=0;j<3;j++){float h=.22f+j*.19f;Line(.15f,h+.1f,.5f,h+.28f);Line(.5f,h+.28f,.85f,h+.1f);Line(.85f,h+.1f,.5f,h-.02f);Line(.5f,h-.02f,.15f,h+.1f);}break;
                    case 13: Rect(.13f,.13f,.87f,.87f);Rect(.24f,.47f,.51f,.74f);Rect(.58f,.32f,.63f,.77f);Rect(.69f,.32f,.74f,.77f);Line(.25f,.30f,.51f,.30f);Line(.25f,.22f,.51f,.22f);break;
                }
                byte alpha=(byte)Mathf.RoundToInt(255*Mathf.Clamp01((.023f-d)*n+.5f));pixels[y*n+x]=new Color32(255,255,255,alpha);
            }
            WriteSprite(path,n,n,pixels);
        }
        // Baked original studio illustration, separate from the UI and mission artwork.
        private static void GenerateBackdrop(string path)
        {
            var data = AssetDatabase.LoadAssetAtPath<TextAsset>(StudioArt);
            if (!data) throw new InvalidOperationException("Missing Studio V5 laboratory artwork.");
            var t = new Texture2D(2,2);
            try
            {
                if (!ImageConversion.LoadImage(t,data.bytes) || t.width != 1920 || t.height != 512)
                    throw new InvalidOperationException("Invalid Studio V5 laboratory artwork.");
                WriteSprite(path,t.width,t.height,t.GetPixels32());
            }
            finally { UnityEngine.Object.DestroyImmediate(t); }
        }
        private static void WriteSprite(string path,int w,int h,Color32[] data)
        {
            var t=new Texture2D(w,h,TextureFormat.RGBA32,false);
            try { t.SetPixels32(data);t.Apply();File.WriteAllBytes(path,t.EncodeToPNG()); }
            finally { UnityEngine.Object.DestroyImmediate(t); }
            AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
            var imp=AssetImporter.GetAtPath(path) as TextureImporter;
            if(!imp)throw new InvalidOperationException("Cannot import "+path);
            imp.textureType=TextureImporterType.Sprite;imp.spriteImportMode=SpriteImportMode.Single;imp.mipmapEnabled=false;imp.alphaIsTransparency=true;imp.maxTextureSize=2048;imp.npotScale=TextureImporterNPOTScale.None;imp.wrapMode=TextureWrapMode.Clamp;imp.filterMode=FilterMode.Bilinear;imp.textureCompression=TextureImporterCompression.Uncompressed;imp.SaveAndReimport();
        }
    }
}
