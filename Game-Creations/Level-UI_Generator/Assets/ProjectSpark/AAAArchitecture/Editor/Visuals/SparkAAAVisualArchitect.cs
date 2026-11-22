using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
namespace ProjectSpark.AAAArchitecture.Editor
{
    public static class SparkAAAVisualArchitect
    {
        public const string Menu="Tools/Project Spark/AAA Level Visual Architect/";
        internal const string Root=SparkAAAArchitect.Generated;
        private const string SettingsPath=Root+"/Editor/SparkVisualSettings.asset";
        private const string ReportPath=Root+"/VisualAudit.txt";
        private const string Version="Studio-1.0.0";
        internal static bool Busy {get;private set;}
        private static readonly string[] Names={"Thumbnail","Banner","Portrait","LoadingScreen","Architecture"};
        private static readonly int[] W={1024,1920,1024,1920,1920},H={576,1080,1024,1080,1080};
        private static readonly Vector3[] Angles={new Vector3(1,1.15f,-1.4f),new Vector3(1.1f,.85f,-1.6f),new Vector3(-1,1.1f,-1.3f),new Vector3(-1.4f,.55f,1.2f),new Vector3(0,1,.36f)};
        internal static SparkAAAVisualSettings Settings(bool create=true)
        {
            var s=AssetDatabase.LoadAssetAtPath<SparkAAAVisualSettings>(SettingsPath);if(s!=null||!create)return s;
            if(AssetDatabase.LoadMainAssetAtPath(SettingsPath)!=null)throw new InvalidOperationException("Foreign settings asset at "+SettingsPath);
            Folder(Root+"/Editor");s=ScriptableObject.CreateInstance<SparkAAAVisualSettings>();AssetDatabase.CreateAsset(s,SettingsPath);return s;
        }
        private static void Folder(string path){if(AssetDatabase.IsValidFolder(path))return;Folder(Path.GetDirectoryName(path).Replace('\\','/'));AssetDatabase.CreateFolder(Path.GetDirectoryName(path).Replace('\\','/'),Path.GetFileName(path));}
        internal static SparkAAACatalog Catalog()=>AssetDatabase.LoadAssetAtPath<SparkAAACatalog>(SparkAAAArchitect.CatalogPath);
        internal static SparkAAAVisualBinding Find(SparkAAAVisualSettings s,string key){foreach(var b in s.bindings)if(b.key==key)return b;return null;}
        private static bool Eligible(SparkAAALevel e)=>e!=null&&e.Present&&!e.blocked&&!string.IsNullOrEmpty(e.guid);
        internal static bool ValidPrefab(GameObject obj){if(obj==null||!EditorUtility.IsPersistent(obj))return false;string p=AssetDatabase.GetAssetPath(obj);if(string.IsNullOrEmpty(p)||p.StartsWith(Root+"/",StringComparison.Ordinal)||AssetDatabase.LoadAssetAtPath<GameObject>(p)!=obj)return false;return obj.GetComponentInChildren<MeshFilter>(true)!=null;}
        internal static void Bind(SparkAAAVisualSettings s,SparkAAALevel e,GameObject prefab)
        {
            if(!Eligible(e)||!ValidPrefab(prefab))throw new InvalidOperationException("Select a valid, nonblocked level and a persistent prefab or model with meshes.");
            var b=Find(s,e.key);if(b==null){b=new SparkAAAVisualBinding{key=e.key};s.bindings.Add(b);}
            b.definitionGuid=e.guid;b.prefab=prefab;b.lastSignature="";EditorUtility.SetDirty(s);AssetDatabase.SaveAssetIfDirty(s);
        }
        // Candidate matching is intentionally conservative: exact level token and semantic hardware name.
        private static readonly Regex LevelToken=new Regex(@"(?:^|[/_ -])(?:Level|L|PS-L)[_ -]*0*(\d+)(?=$|[/_ .-])",RegexOptions.IgnoreCase|RegexOptions.CultureInvariant);
        private static readonly Regex HardwareName=new Regex(@"(?:hardware|motherboard|workshop|completeboard|assembly|visual|layout|circuit)",RegexOptions.IgnoreCase|RegexOptions.CultureInvariant);
        internal static Dictionary<int,List<GameObject>> Discover()
        {
            var map=new Dictionary<int,List<GameObject>>();
            foreach(string guid in AssetDatabase.FindAssets("t:Prefab",new[]{"Assets"})){
                string p=AssetDatabase.GUIDToAssetPath(guid);if(p.StartsWith(Root+"/",StringComparison.Ordinal)||!HardwareName.IsMatch(Path.GetFileNameWithoutExtension(p)))continue;
                int number=-1;bool conflict=false;foreach(Match m in LevelToken.Matches(p)){if(!int.TryParse(m.Groups[1].Value,out int n)||n<1)continue;if(number!=-1&&number!=n)conflict=true;number=n;}
                if(conflict||number<1)continue;
                var go=AssetDatabase.LoadAssetAtPath<GameObject>(p);if(!ValidPrefab(go))continue;
                if(!map.TryGetValue(number,out var list)){list=new List<GameObject>();map[number]=list;}list.Add(go);
            }
            foreach(var list in map.Values)list.Sort((a,b)=>string.CompareOrdinal(AssetDatabase.GetAssetPath(a),AssetDatabase.GetAssetPath(b)));return map;
        }
        internal static bool Approved(SparkAAALevel e,SparkAAAVisualBinding b)=>Eligible(e)&&b!=null&&b.definitionGuid==e.guid&&ValidPrefab(b.prefab);
        private static string PathFor(string key,int i)=>i==0?Root+"/Thumbnails/"+key+".png":i==1?Root+"/Resources/SparkAAAVisuals/"+key+"/Banner.png":Root+"/Visuals/"+key+"/"+Names[i]+".png";
        private static string Hash(byte[] bytes){using(var sha=SHA256.Create())return BitConverter.ToString(sha.ComputeHash(bytes)).Replace("-","");}
        private static string FileHash(string p)=>File.Exists(p)?Hash(File.ReadAllBytes(p)):"";
        private static string Signature(SparkAAAVisualSettings s,SparkAAALevel e,SparkAAAVisualBinding b)
        {
            var pipeline=GraphicsSettings.currentRenderPipeline;string pp=pipeline==null?"BuiltIn":AssetDatabase.GetAssetPath(pipeline);
            string raw=Version+"|"+Application.unityVersion+"|"+QualitySettings.activeColorSpace+"|"+e.guid+"|"+AssetDatabase.GetAssetDependencyHash(e.sourcePath)+"|"+AssetDatabase.GetAssetPath(b.prefab)+"|"+AssetDatabase.GetAssetDependencyHash(AssetDatabase.GetAssetPath(b.prefab))+"|"+pp+"|"+(pipeline==null?"":AssetDatabase.GetAssetDependencyHash(pp).ToString())+"|"+JsonUtility.ToJson(new VisualConfig{padding=s.padding,background=s.background,fullSize=s.fullSizeThumbnails});
            return Hash(Encoding.UTF8.GetBytes(raw));
        }
        [Serializable] private sealed class VisualConfig { public float padding;public Color background;public bool fullSize; }
        private static bool Current(SparkAAAVisualBinding b,string sig){if(b.lastSignature!=sig||b.outputHashes==null||b.outputHashes.Count!=Names.Length)return false;for(int i=0;i<Names.Length;i++)if(FileHash(PathFor(b.key,i))!=b.outputHashes[i]||AssetDatabase.LoadAssetAtPath<Sprite>(PathFor(b.key,i))==null)return false;return true;}
        private static void Import(string path,int index,bool fullSize)
        {
            AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport|ImportAssetOptions.ForceUpdate);
            var t=AssetImporter.GetAtPath(path) as TextureImporter;if(t==null)throw new InvalidOperationException("No texture importer: "+path);
            t.textureType=TextureImporterType.Sprite;t.spriteImportMode=SpriteImportMode.Single;t.sRGBTexture=true;t.mipmapEnabled=false;t.isReadable=false;t.alphaIsTransparency=false;t.npotScale=TextureImporterNPOTScale.None;t.maxTextureSize=index==0&&!fullSize?512:2048;t.textureCompression=TextureImporterCompression.CompressedHQ;t.filterMode=FilterMode.Bilinear;t.wrapMode=TextureWrapMode.Clamp;t.SaveAndReimport();
            if(AssetDatabase.LoadAssetAtPath<Sprite>(path)==null)throw new InvalidOperationException("Sprite import failed: "+path);
        }
        private static void RenderLevel(SparkAAAVisualSettings s,SparkAAALevel e,SparkAAAVisualBinding b,string sig,bool replaceForeign,StringBuilder report)
        {
            var paths=new string[Names.Length];var bytes=new byte[Names.Length][];var originals=new byte[Names.Length][];var metas=new byte[Names.Length][];
            for(int i=0;i<Names.Length;i++){
                string p=paths[i]=PathFor(e.key,i);if(File.Exists(p)){
                    string old=FileHash(p);bool owned=b.outputHashes!=null&&b.outputHashes.Count==Names.Length&&old==b.outputHashes[i];
                    if(!owned&&!replaceForeign)throw new InvalidOperationException("Existing unowned or edited image: "+p+". Enable explicit replacement to continue.");
                    originals[i]=File.ReadAllBytes(p);if(File.Exists(p+".meta"))metas[i]=File.ReadAllBytes(p+".meta");
                }else if(File.Exists(p+".meta"))throw new InvalidOperationException("Orphan metadata: "+p);
            }
            using(var studio=new SparkAAAStudio(b.prefab,s)){
                for(int i=0;i<Names.Length;i++)bytes[i]=studio.Capture(W[i],H[i],Angles[i]);
                report.AppendLine($"  Mesh renderers: {studio.MeshCount}; skipped skinned objects: {studio.SkippedSkinned}");
            }
            // Back up prior images and import settings before touching output. No source model or scene changes.
            string backup=Path.Combine(Directory.GetParent(Application.dataPath).FullName,"SparkAAA_VisualBackups",DateTime.UtcNow.ToString("yyyyMMdd_HHmmss_fffffff"),e.key);
            for(int i=0;i<Names.Length;i++)if(originals[i]!=null){Directory.CreateDirectory(backup);File.WriteAllBytes(Path.Combine(backup,Names[i]+".png"),originals[i]);if(metas[i]!=null)File.WriteAllBytes(Path.Combine(backup,Names[i]+".png.meta"),metas[i]);}
            int touched=0;try{
                for(int i=0;i<Names.Length;i++){Folder(Path.GetDirectoryName(paths[i]).Replace('\\','/'));touched=i+1;File.WriteAllBytes(paths[i],bytes[i]);Import(paths[i],i,s.fullSizeThumbnails);}
            }catch{
                for(int i=0;i<touched;i++){if(originals[i]==null){AssetDatabase.DeleteAsset(paths[i]);if(File.Exists(paths[i]))File.Delete(paths[i]);if(File.Exists(paths[i]+".meta"))File.Delete(paths[i]+".meta");}else{File.WriteAllBytes(paths[i],originals[i]);if(metas[i]!=null)File.WriteAllBytes(paths[i]+".meta",metas[i]);}}
                AssetDatabase.Refresh();throw;
            }
            b.outputHashes=new List<string>();foreach(var data in bytes)b.outputHashes.Add(Hash(data));b.lastSignature=sig;EditorUtility.SetDirty(s);AssetDatabase.SaveAssetIfDirty(s);
        }
        internal static void Build(bool force=false,bool replaceForeign=false,int only=0,bool rescan=true)
        {
            if(Busy||EditorApplication.isPlayingOrWillChangePlaymode||EditorApplication.isCompiling||EditorApplication.isUpdating)throw new InvalidOperationException("Visual Architect cannot render in the current editor state.");
            Busy=true;var report=new StringBuilder("PROJECT SPARK / VISUAL LIBRARY AUDIT\n"+DateTime.UtcNow.ToString("O")+"\n");int rendered=0,current=0,blocked=0,failed=0;bool cancelled=false;
            try{
                var c=rescan?SparkAAAArchitect.Scan():Catalog();if(c==null)throw new InvalidOperationException("Build or audit campaign first.");var s=Settings();
                for(int i=0;i<c.levels.Count;i++){
                    var e=c.levels[i];if(only>0&&e.number!=only)continue;
                    if(EditorUtility.DisplayCancelableProgressBar("Spark AAA Visual Architect",e.key+" / "+e.title,(float)i/Mathf.Max(1,c.levels.Count))){cancelled=true;break;}
                    var b=Find(s,e.key);if(!Approved(e,b)){blocked++;report.AppendLine(e.key+" BLOCKED: "+(!Eligible(e)?"Missing or blocked definition":"No approved matching prefab"));continue;}
                    try{string sig=Signature(s,e,b);if(!force&&Current(b,sig)){current++;report.AppendLine(e.key+" CURRENT");continue;}RenderLevel(s,e,b,sig,replaceForeign,report);rendered++;report.AppendLine(e.key+" GENERATED: "+AssetDatabase.GetAssetPath(b.prefab));}
                    catch(Exception ex){failed++;report.AppendLine(e.key+" FAILED: "+ex.Message);Debug.LogWarning("[Spark Visuals] "+e.key+": "+ex.Message);}
                }
                SparkAAAArchitect.Scan();
            }finally{
                EditorUtility.ClearProgressBar();Busy=false;report.AppendLine($"Rendered {rendered}; current {current}; blocked {blocked}; failed {failed}; cancelled {cancelled}.");report.AppendLine("Architecture is a top-angle render, not a verified exploded view. Visual QA and Play Mode are required.");Folder(Root);File.WriteAllText(ReportPath,report.ToString());AssetDatabase.ImportAsset(ReportPath);Debug.Log("[Spark Visuals] "+report.ToString());
            }
        }
        [MenuItem(Menu+"02 Generate missing or changed visuals")]
        public static void Incremental()=>Build();
        [MenuItem(Menu+"03 Validate visual library")]
        public static void Validate()
        {
            var c=Catalog();var s=Settings(false);if(c==null||s==null)throw new InvalidOperationException("Missing catalog or visual settings.");var report=new StringBuilder("VISUAL LIBRARY VALIDATION\n");int valid=0,invalid=0;
            foreach(var e in c.levels){var b=Find(s,e.key);bool ok=Approved(e,b)&&Current(b,Signature(s,e,b));if(ok)valid++;else invalid++;report.AppendLine(e.key+" "+(ok?"CURRENT":"MISSING / STALE / UNAPPROVED"));}
            report.AppendLine($"Valid {valid}; not ready {invalid}. Visual quality not automatically certified.");File.WriteAllText(ReportPath,report.ToString());AssetDatabase.ImportAsset(ReportPath);Debug.Log(report.ToString());
        }
        [MenuItem(Menu+"04 Open visual report")]
        public static void OpenReport(){Selection.activeObject=AssetDatabase.LoadAssetAtPath<TextAsset>(ReportPath);}
    }
}
