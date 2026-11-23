using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace ProjectSpark.AAAArchitecture.Editor
{
    public static class SparkLivePreviewArchitect
    {
        private const string Menu="Tools/Project Spark/AAA Cinematic V3/";
        private const string Root=SparkAAAArchitect.Generated+"/Resources/SparkLivePreview";
        private const string SettingsPath=Root+"/Settings.asset";
        private const string LevelsPath=Root+"/Levels";
        private const string BackdropPath=Root+"/LaboratoryBackdrop.png";
        private const string ReportPath=SparkAAAArchitect.Generated+"/LivePreviewAudit.txt";
        private const string Version="SparkLivePreview-1";
        internal static bool Busy { get; private set; }
        private static bool queued;
        private static void Folder(string p)
        {
            if(AssetDatabase.IsValidFolder(p))return;
            string parent=Path.GetDirectoryName(p).Replace('\\','/'); Folder(parent);
            AssetDatabase.CreateFolder(parent,Path.GetFileName(p));
        }
        internal static SparkLivePreviewSettings Settings(bool create)
        {
            var s=AssetDatabase.LoadAssetAtPath<SparkLivePreviewSettings>(SettingsPath);
            if(s||!create)return s;
            if(AssetDatabase.LoadMainAssetAtPath(SettingsPath))throw new InvalidOperationException("Settings path occupied.");
            Folder(Root);s=ScriptableObject.CreateInstance<SparkLivePreviewSettings>();AssetDatabase.CreateAsset(s,SettingsPath);return s;
        }
        [MenuItem(Menu+"01 Build complete V3")]
        public static void Build()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode)return;
            SparkCinematicUIArchitect.BuildSkin();
            var settings=Settings(true); BuildBackdrop(settings);
            if(LayerMask.NameToLayer(SparkLivePreviewSettings.LayerName)<0) ConfigureLayer();
            SparkAAAArchitect.Scan(); Refresh();
            Selection.activeObject=settings;
            Debug.Log("[Spark V3] Open your existing Chapters panel in Play Mode. Review LivePreviewAudit.txt.");
        }
        [MenuItem(Menu+"02 Configure isolated preview layer")]
        public static void ConfigureLayer()
        {
            if(LayerMask.NameToLayer(SparkLivePreviewSettings.LayerName)>=8)return;
            if(!EditorUtility.DisplayDialog("Reserve SparkPreview layer","Reserve one unused user layer for isolated 3D rendering. Keep gameplay objects off this layer; no existing named layer will be changed.","Reserve","Cancel"))return;
            var objects=AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");
            if(objects.Length==0){Debug.LogError("[Spark V3] TagManager unavailable.");return;}
            var tag=new SerializedObject(objects[0]);var layers=tag.FindProperty("layers");
            if(layers==null){Debug.LogError("[Spark V3] Layers unavailable.");return;}
            for(int i=8;i<Mathf.Min(31,layers.arraySize);i++)
            {
                var p=layers.GetArrayElementAtIndex(i);if(!string.IsNullOrEmpty(p.stringValue))continue;
                p.stringValue=SparkLivePreviewSettings.LayerName;tag.ApplyModifiedProperties();AssetDatabase.SaveAssets();
                Debug.Log("[Spark V3] Reserved user layer "+i+". Verify no existing scene object uses it.");return;
            }
            Debug.LogWarning("[Spark V3] No unused layer. Static artwork remains available.");
        }
        [MenuItem(Menu+"03 Refresh approved previews")]
        public static void Refresh()
        {
            if(Busy||EditorApplication.isPlayingOrWillChangePlaymode)return;
            Busy=true;
            try
            {
                Folder(LevelsPath);var config=Settings(true);
                var catalog=SparkAAAVisualArchitect.Catalog();var approved=SparkAAAVisualArchitect.Settings(false);
                if(!catalog){Debug.LogWarning("[Spark V3] Generate campaign catalog first.");return;}
                var active=new HashSet<string>(StringComparer.Ordinal);var frequencies=new Dictionary<int,int>();
                foreach(var e in catalog.levels) if(e!=null){frequencies.TryGetValue(e.number,out int n);frequencies[e.number]=n+1;}
                var report=new StringBuilder("SPARK LIVE PREVIEW AUDIT\n");int generated=0,unchanged=0,fallback=0;
                foreach(var e in catalog.levels)
                {
                    if(e==null)continue;
                    string key="PS-L"+e.number.ToString("000"), path=LevelsPath+"/"+key+".asset";
                    var binding=approved?SparkAAAVisualArchitect.Find(approved,e.key):null;
                    if(e.number<1||e.key!=key||frequencies[e.number]!=1||!SparkAAAVisualArchitect.Approved(e,binding))
                    {report.AppendLine(key+": static — missing, ambiguous, blocked or unapproved source");fallback++;continue;}
                    try
                    {
                        string source=AssetDatabase.GetAssetPath(binding.prefab);
                        string signature=Version+"|"+e.guid+"|"+AssetDatabase.GetAssetDependencyHash(source)+"|"+source;
                        var existing=AssetDatabase.LoadAssetAtPath<SparkLivePreviewAsset>(path);
                        if(!existing&&AssetDatabase.LoadMainAssetAtPath(path))throw new InvalidOperationException("Foreign asset occupies generated path.");
                        if(existing&&!string.IsNullOrEmpty(existing.sourceSignature)&&!existing.sourceSignature.StartsWith(Version+"|",StringComparison.Ordinal))throw new InvalidOperationException("Foreign preview manifest; move it before regenerating.");
                        if(existing&&existing.sourceSignature==signature&&existing.definitionGuid==e.guid&&existing.definition==e.definition&&existing.parts!=null&&existing.parts.Length>0)
                        {active.Add(path);unchanged++;report.AppendLine(key+": current, "+existing.parts.Length+" draws");continue;}
                        var parts=Collect(binding.prefab,out long indices);
                        if(!existing){existing=ScriptableObject.CreateInstance<SparkLivePreviewAsset>();AssetDatabase.CreateAsset(existing,path);}
                        existing.levelNumber=e.number;existing.definitionGuid=e.guid;existing.definition=e.definition;existing.sourceSignature=signature;existing.parts=parts;existing.indexCount=indices;
                        EditorUtility.SetDirty(existing);active.Add(path);generated++;
                        report.AppendLine(key+": generated, "+parts.Length+" draws, "+indices+" indices"+(parts.Length>config.mobileDrawBudget||indices>config.mobileIndexBudget?" — mobile static fallback":"")+(parts.Length>config.desktopDrawBudget||indices>config.desktopIndexBudget?" — desktop static fallback":""));
                    }
                    catch(Exception ex){fallback++;report.AppendLine(key+": static — "+ex.Message);}
                }
                // Clear only our own generated manifest objects; preserve GUIDs and source assets.
                foreach(string guid in AssetDatabase.FindAssets("t:SparkLivePreviewAsset",new[]{LevelsPath}))
                {
                    string path=AssetDatabase.GUIDToAssetPath(guid);if(active.Contains(path))continue;
                    var asset=AssetDatabase.LoadAssetAtPath<SparkLivePreviewAsset>(path);
                    if(!asset||string.IsNullOrEmpty(asset.sourceSignature)||!asset.sourceSignature.StartsWith(Version+"|",StringComparison.Ordinal))continue;
                    if(asset.parts==null||asset.parts.Length==0)continue;
                    asset.parts=Array.Empty<SparkLivePreviewAsset.Part>();asset.indexCount=0;asset.definition=null;asset.sourceSignature=Version+"|revoked";EditorUtility.SetDirty(asset);
                    report.AppendLine(path+": obsolete approval cleared");
                }
                report.AppendLine($"Generated {generated}; unchanged {unchanged}; static {fallback}.");
                File.WriteAllText(ReportPath,report.ToString());AssetDatabase.ImportAsset(ReportPath);
                AssetDatabase.SaveAssets();Debug.Log("[Spark V3] "+report.ToString());
            }
            finally{Busy=false;}
        }
        private static SparkLivePreviewAsset.Part[] Collect(GameObject source,out long indices)
        {
            var excluded=new HashSet<Renderer>();
            foreach(var group in source.GetComponentsInChildren<LODGroup>(true))
            {
                var levels=group.GetLODs();if(levels.Length<2)continue;
                var first=new HashSet<Renderer>(levels[0].renderers);
                for(int i=1;i<levels.Length;i++)foreach(var r in levels[i].renderers)if(r&&!first.Contains(r))excluded.Add(r);
            }
            var parts=new List<SparkLivePreviewAsset.Part>();Bounds bounds=default;bool has=false;indices=0;
            CollectTree(source.transform,Matrix4x4.identity,true,excluded,parts,ref bounds,ref has,ref indices);
            if(!has||parts.Count==0||bounds.extents.magnitude<1e-8f)throw new InvalidOperationException("No usable static geometry.");
            float scale=1f/bounds.extents.magnitude;
            if(float.IsInfinity(scale)||float.IsNaN(scale))throw new InvalidOperationException("Invalid geometry bounds.");
            var normalization=Matrix4x4.Scale(Vector3.one*scale)*Matrix4x4.Translate(-bounds.center);
            for(int i=0;i<parts.Count;i++){var p=parts[i];p.matrix=normalization*p.matrix;parts[i]=p;}
            return parts.ToArray();
        }
        private static void CollectTree(Transform t,Matrix4x4 parent,bool root,HashSet<Renderer> excluded,List<SparkLivePreviewAsset.Part> parts,ref Bounds bounds,ref bool has,ref long indices)
        {
            if(!t.gameObject.activeSelf)return;
            var matrix=parent*Matrix4x4.TRS(root?Vector3.zero:t.localPosition,t.localRotation,t.localScale);
            var skinned=t.GetComponent<SkinnedMeshRenderer>();
            if(skinned&&skinned.enabled&&!excluded.Contains(skinned))throw new InvalidOperationException("Active skinned mesh requires a baked static preview source.");
            var r=t.GetComponent<MeshRenderer>();var f=t.GetComponent<MeshFilter>();
            if(r&&f&&r.enabled&&!r.forceRenderingOff&&!excluded.Contains(r)&&f.sharedMesh)
            {
                var mesh=f.sharedMesh;var materials=r.sharedMaterials;
                if(materials.Length<mesh.subMeshCount)throw new InvalidOperationException("Missing submesh materials: "+t.name);
                for(int i=0;i<mesh.subMeshCount;i++)
                {
                    if(mesh.GetIndexCount(i)==0)continue;
                    var mat=materials[i];if(!mat||!mat.shader||!mat.shader.isSupported)throw new InvalidOperationException("Unsupported material: "+t.name);
                    if(!EditorUtility.IsPersistent(mesh)||!EditorUtility.IsPersistent(mat))throw new InvalidOperationException("Nonpersistent mesh/material: "+t.name);
                    parts.Add(new SparkLivePreviewAsset.Part{mesh=mesh,material=mat,matrix=matrix,submesh=i});indices+=mesh.GetIndexCount(i);
                }
                var b=mesh.bounds;
                for(int i=0;i<8;i++)
                {
                    var corner=b.center+Vector3.Scale(b.extents,new Vector3((i&1)==0?-1:1,(i&2)==0?-1:1,(i&4)==0?-1:1));
                    var p=matrix.MultiplyPoint3x4(corner);
                    if(float.IsNaN(p.x)||float.IsInfinity(p.x)||float.IsNaN(p.y)||float.IsInfinity(p.y)||float.IsNaN(p.z)||float.IsInfinity(p.z))throw new InvalidOperationException("Invalid transform: "+t.name);
                    if(!has){bounds=new Bounds(p,Vector3.zero);has=true;}else bounds.Encapsulate(p);
                }
            }
            for(int i=0;i<t.childCount;i++)CollectTree(t.GetChild(i),matrix,false,excluded,parts,ref bounds,ref has,ref indices);
        }
        [MenuItem(Menu+"04 Settings")]
        public static void OpenSettings()=>Selection.activeObject=Settings(true);
        [MenuItem(Menu+"05 Create laboratory backdrop")]
        public static void Backdrop()=>BuildBackdrop(Settings(true));
        private static void BuildBackdrop(SparkLivePreviewSettings s)
        {
            if(s.laboratoryBackdrop)return; // Preserve imported/custom background.
            Folder(Root);
            if(!File.Exists(BackdropPath))
            {
                const int w=1024,h=256;var tex=new Texture2D(w,h,TextureFormat.RGBA32,false);
                try
                {
                    var pixels=new Color32[w*h];
                    for(int y=0;y<h;y++)for(int x=0;x<w;x++)
                    {
                        float u=(float)x/(w-1),v=(float)y/(h-1);float light=Mathf.Exp(-Mathf.Pow((u-.73f)*3.2f,2)-Mathf.Pow((v-.58f)*2,2));
                        float panel=(x%160<4||y%92<2)?.035f:0;
                        float rail=(Mathf.Abs(v-.2f)<.012f||Mathf.Abs(v-.82f)<.008f)?.06f:0;
                        float vertical=x%160>130&&x%160<135&&v>.27f&&v<.75f?.10f:0;
                        float warm=x%320>295&&v>.36f&&v<.66f?.12f:0;
                        float grid=(x%32==0||y%32==0)?.009f:0;
                        var c=new Color(.018f+light*.025f+panel+warm,.035f+light*.09f+rail+vertical*.7f+grid,.052f+light*.13f+vertical+rail+grid,1);
                        pixels[y*w+x]=c;
                    }
                    tex.SetPixels32(pixels);tex.Apply();File.WriteAllBytes(BackdropPath,tex.EncodeToPNG());
                }finally{UnityEngine.Object.DestroyImmediate(tex);}
            }
            AssetDatabase.ImportAsset(BackdropPath);var importer=AssetImporter.GetAtPath(BackdropPath) as TextureImporter;
            if(importer&&importer.textureType!=TextureImporterType.Sprite){importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;importer.mipmapEnabled=false;importer.SaveAndReimport();}
            s.laboratoryBackdrop=AssetDatabase.LoadAssetAtPath<Sprite>(BackdropPath);EditorUtility.SetDirty(s);AssetDatabase.SaveAssets();
        }
        [MenuItem(Menu+"06 Open preview audit")]
        public static void Audit(){var a=AssetDatabase.LoadAssetAtPath<TextAsset>(ReportPath);if(a)Selection.activeObject=a;else Debug.LogWarning("Refresh previews first.");}
        internal static void Queue()
        {
            if(queued||Busy||EditorApplication.isPlayingOrWillChangePlaymode)return;
            var s=Settings(false);if(!s||!s.autoRefresh)return;queued=true;
            EditorApplication.delayCall+=()=>{queued=false;if(!EditorApplication.isPlayingOrWillChangePlaymode&&!Busy)Refresh();};
        }
    }
    internal sealed class SparkLivePreviewPostprocessor:AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(string[] imported,string[] deleted,string[] moved,string[] movedFrom)
        {
            if(SparkLivePreviewArchitect.Busy)return;
            foreach(var group in new[]{imported,deleted,moved,movedFrom})foreach(var p in group)
            {
                if(p==SparkAAAArchitect.CatalogPath||p==SparkAAAArchitect.Generated+"/Editor/SparkVisualSettings.asset") {SparkLivePreviewArchitect.Queue();return;}
                if(p.StartsWith(SparkAAAArchitect.Generated+"/",StringComparison.Ordinal))continue;
                string ext=Path.GetExtension(p).ToLowerInvariant();
                if(ext==".prefab"||ext==".fbx"||ext==".obj"||ext==".mat"||ext==".asset"||ext==".shader"||ext==".shadergraph"||ext==".png"||ext==".jpg"||ext==".tga") {SparkLivePreviewArchitect.Queue();return;}
            }
        }
    }
}
