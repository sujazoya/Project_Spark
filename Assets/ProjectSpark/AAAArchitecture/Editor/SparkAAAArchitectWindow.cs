using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
namespace ProjectSpark.AAAArchitecture.Editor
{
    public sealed class SparkAAAArchitectWindow:EditorWindow
    {
        private GameObject previewPrefab;
        private int levelNumber=1;
        private Vector2 scroll;
        [MenuItem(SparkAAAArchitect.Menu+"Open Master Architect",false,0)]
        public static void Open()=>GetWindow<SparkAAAArchitectWindow>("Spark AAA Architect");
        private void OnGUI()
        {
            scroll=EditorGUILayout.BeginScrollView(scroll);
            EditorGUILayout.Space();EditorGUILayout.LabelField("PROJECT SPARK  /  AAA LEVEL ARCHITECT",EditorStyles.boldLabel);
            var data=AssetDatabase.LoadAssetAtPath<SparkAAACatalog>(SparkAAAArchitect.CatalogPath);
            int present=0,targets=0,blocked=0;if(data!=null)foreach(var level in data.levels){if(level.Present)present++;targets+=level.targets.Count;if(level.blocked)blocked++;}
            EditorGUILayout.LabelField("Generated catalog",data==null?"Not built":$"{data.levels.Count} slots / {present} mapped / {targets} targets / {blocked} blocked");
            EditorGUILayout.HelpBox("Original levels are read-only. Generated presentation is local; gameplay, save and unlock authority stay with the existing systems.",MessageType.Info);
            using(new EditorGUI.DisabledScope(EditorApplication.isPlayingOrWillChangePlaymode)){
                if(GUILayout.Button("AUDIT & REFRESH CAMPAIGN",GUILayout.Height(34)))Run(()=>SparkAAAArchitect.Scan());
                if(GUILayout.Button("BUILD COMPLETE AAA PRESENTATION",GUILayout.Height(42)))Run(SparkAAAArchitect.BuildMenu);
                if(GUILayout.Button("OPEN GENERATED PREVIEW"))Run(SparkAAAArchitect.OpenScene);
                if(GUILayout.Button("VALIDATE GENERATED ASSETS"))Run(Validate);
                SparkAAAArchitect.AutoRefresh=EditorGUILayout.Toggle("Auto-refresh on asset changes",SparkAAAArchitect.AutoRefresh);
                EditorGUILayout.Space();EditorGUILayout.LabelField("Approved hardware thumbnail",EditorStyles.boldLabel);
                EditorGUILayout.HelpBox("Assign a real prefab to a specific level. Unity's asset preview is not a full-resolution studio render. Use a pre-rendered Sprite PNG instead for release.",MessageType.None);
                levelNumber=EditorGUILayout.IntField("Level number",levelNumber);
                previewPrefab=(GameObject)EditorGUILayout.ObjectField("Hardware prefab",previewPrefab,typeof(GameObject),false);
                if(GUILayout.Button("Capture assigned prefab thumbnail"))Run(Capture);
            }
            EditorGUILayout.EndScrollView();
        }
        private static void Run(Action a){try{a();}catch(Exception e){Debug.LogException(e);}}
        private void Capture()
        {
            var catalog=AssetDatabase.LoadAssetAtPath<SparkAAACatalog>(SparkAAAArchitect.CatalogPath);
            var e=catalog!=null?catalog.Find($"PS-L{levelNumber:000}"):null;
            if(e==null||!e.Present||e.blocked||previewPrefab==null||!EditorUtility.IsPersistent(previewPrefab)){Debug.LogWarning("Select a valid mapped level and persistent prefab first.");return;}
            var source=AssetPreview.GetAssetPreview(previewPrefab);if(source==null){Debug.LogWarning("Unity asset preview is not ready. Click again after the Inspector preview appears.");return;}
            var path=SparkAAAArchitect.Generated+"/Thumbnails/"+e.key+".png";
            if(File.Exists(path)&&!EditorUtility.DisplayDialog("Replace thumbnail?",path,"Replace","Cancel"))return;
            var old=RenderTexture.active;var rt=RenderTexture.GetTemporary(source.width,source.height,0,RenderTextureFormat.ARGB32);Texture2D copy=null;
            try{Graphics.Blit(source,rt);RenderTexture.active=rt;copy=new Texture2D(rt.width,rt.height,TextureFormat.RGBA32,false);copy.ReadPixels(new Rect(0,0,rt.width,rt.height),0,0);copy.Apply();File.WriteAllBytes(path,copy.EncodeToPNG());}
            finally{RenderTexture.active=old;RenderTexture.ReleaseTemporary(rt);if(copy!=null)DestroyImmediate(copy);}
            AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);var importer=AssetImporter.GetAtPath(path) as TextureImporter;
            if(importer!=null){importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;importer.mipmapEnabled=false;importer.SaveAndReimport();}
            SparkAAAArchitect.Scan();
        }
        [MenuItem(SparkAAAArchitect.Menu+"06 Validate generated assets")]
        public static void Validate()
        {
            var c=AssetDatabase.LoadAssetAtPath<SparkAAACatalog>(SparkAAAArchitect.CatalogPath);
            var card=AssetDatabase.LoadAssetAtPath<GameObject>(SparkAAAArchitect.CardPath);var panel=AssetDatabase.LoadAssetAtPath<GameObject>(SparkAAAArchitect.PanelPath);
            if(c==null||card==null||panel==null||!File.Exists(SparkAAAArchitect.ScenePath))throw new InvalidOperationException("Build is incomplete. Catalog, card, panel and preview scene required.");
            if(card.GetComponent<SparkAAALevelCard>()==null||panel.GetComponent<SparkAAAMissionPanel>()==null||panel.GetComponentsInChildren<TMP_Text>(true).Length==0||panel.GetComponentsInChildren<Button>(true).Length==0)throw new InvalidOperationException("Generated prefab missing components.");
            var keys=new System.Collections.Generic.HashSet<string>();foreach(var e in c.levels)if(!keys.Add(e.key))throw new InvalidOperationException("Duplicate campaign key: "+e.key);
            var so=new SerializedObject(panel.GetComponent<SparkAAAMissionPanel>());
            foreach(var name in new[]{"catalog","cardPrefab","grid","gridLayout","gridScroll","detailScroll","action","search","chapterNext","detailTitle","detailBody"})if(so.FindProperty(name)?.objectReferenceValue==null)throw new InvalidOperationException("Missing panel reference: "+name);
            Debug.Log("[Spark AAA] Generated references and unique campaign keys passed. Runtime behavior and actual gameplay remain untested by this check.");
        }
    }
}
