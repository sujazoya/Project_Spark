using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

namespace ProjectSpark.AAAArchitecture.Editor
{
    public static class SparkAAAArchitect
    {
        public const string Root="Assets/ProjectSpark/AAAArchitecture";
        public const string Generated=Root+"/Generated";
        public const string CatalogPath=Generated+"/SparkCampaignCatalog.asset";
        public const string CardPath=Generated+"/Prefabs/SparkLevelButton.prefab";
        public const string PanelPath=Generated+"/Prefabs/SparkLevelSelectionPanel.prefab";
        public const string ScenePath=Generated+"/Scenes/SparkMissionControl.unity";
        public const string Menu="Tools/Project Spark/AAA Level Architect/";
        private const string OriginalType="ProjectSpark.Gameplay.SparkLevelDefinition";
        private static readonly Regex NumberPattern=new Regex(@"^Level[_ -]*0*(\d+)$",RegexOptions.IgnoreCase|RegexOptions.CultureInvariant);
        private static string PrefKey=>"Spark.AAA.AutoRefresh."+Application.dataPath;
        public static bool AutoRefresh { get=>EditorPrefs.GetBool(PrefKey,false); set=>EditorPrefs.SetBool(PrefKey,value); }
        internal static bool Busy {get;private set;}
        private static void Folder(string path)
        { if(AssetDatabase.IsValidFolder(path))return;var parent=Path.GetDirectoryName(path).Replace('\\','/');Folder(parent);AssetDatabase.CreateFolder(parent,Path.GetFileName(path)); }
        private static void Folders(){Folder(Generated);Folder(Generated+"/Prefabs");Folder(Generated+"/Scenes");Folder(Generated+"/Thumbnails");}
        private static string Str(SerializedObject so,string k)=>so.FindProperty(k)?.stringValue??"";
        private static int Int(SerializedObject so,string k)=>so.FindProperty(k)?.intValue??0;
        private static bool Bool(SerializedObject so,string k)=>so.FindProperty(k)?.boolValue??false;
        private static string Str(SerializedProperty o,string k)=>o.FindPropertyRelative(k)?.stringValue??"";
        private static float Float(SerializedProperty o,string k)=>o.FindPropertyRelative(k)?.floatValue??0;
        private static int Int(SerializedProperty o,string k)=>o.FindPropertyRelative(k)?.intValue??0;
        private static void Issue(SparkAAALevel e,string text,bool block=false){e.issue=string.IsNullOrEmpty(e.issue)?text:e.issue+"; "+text; e.blocked|=block;}
        private static List<SparkAAAChapter> Chapters()=>new List<SparkAAAChapter>{
            new SparkAAAChapter(1,1,20,"Electrical Foundations"),new SparkAAAChapter(2,21,29,"Electronic Components"),new SparkAAAChapter(3,30,42,"Instrumentation & Control"),new SparkAAAChapter(4,43,50,"Industrial Manufacturing"),new SparkAAAChapter(5,51,56,"Machine Dynamics"),new SparkAAAChapter(6,57,64,"Power Systems"),new SparkAAAChapter(7,65,70,"Process Engineering"),new SparkAAAChapter(8,71,82,"Semiconductor Engineering"),new SparkAAAChapter(9,83,93,"Space Systems"),new SparkAAAChapter(10,94,105,"PCB Engineering"),new SparkAAAChapter(11,106,117,"Hardware Qualification"),new SparkAAAChapter(12,118,127,"Mission-Critical Systems"),new SparkAAAChapter(13,128,130,"Commercial Motherboards"),new SparkAAAChapter(14,131,int.MaxValue,"Future Missions / Review")};
        private static SparkAAALevel Slot(int n,List<SparkAAAChapter> chapters)
        { var e=new SparkAAALevel{number=n,key=$"PS-L{n:000}",title=$"Level {n:000}",description="",issue=""};foreach(var c in chapters)if(n>=c.first&&n<=c.last){e.chapter=c.id;break;}return e; }
        private static SparkAAALevel Read(int n,string path,string guid,ScriptableObject asset,List<SparkAAAChapter> chapters)
        {
            var e=Slot(n,chapters);e.definition=asset;e.guid=guid;e.sourcePath=path;
            var s=new SerializedObject(asset);e.legacyId=Str(s,"levelId");e.title=Str(s,"displayName");if(string.IsNullOrWhiteSpace(e.title))e.title=asset.name;e.description=Str(s,"description");e.requiredTargets=Int(s,"requiredTargetCount");e.rawCompletionMode=Int(s,"completionMode");e.defaultUnlocked=Bool(s,"unlockedByDefault");e.autoUnlock=Bool(s,"autoUnlockNextLevel");e.autoAdvance=Bool(s,"autoAdvanceOnCompletion");e.unlockNext=Bool(s,"unlockNextLevelOnCompletion");
            foreach(string field in new[]{"levelId","displayName","description","targets","requiredTargetCount","completionMode","unlockedByDefault","autoUnlockNextLevel","autoAdvanceOnCompletion","unlockNextLevelOnCompletion"})if(s.FindProperty(field)==null)Issue(e,"Missing serialized field: "+field,true);
            if(string.IsNullOrWhiteSpace(e.legacyId))Issue(e,"Empty legacy ID",true);
            var legacy=Regex.Match(e.legacyId??"",@"^level_0*(\d+)(?:_|$)",RegexOptions.IgnoreCase);if(legacy.Success&&int.TryParse(legacy.Groups[1].Value,out int id)&&id!=n)Issue(e,"Legacy ID differs from filename; confirm mapping",true);
            var a=s.FindProperty("targets");var targetIds=new HashSet<string>(StringComparer.Ordinal);
            if(a!=null&&a.isArray)for(int i=0;i<a.arraySize;i++){
                var t=a.GetArrayElementAtIndex(i);var dto=new SparkAAATarget{id=Str(t,"targetId"),title=Str(t,"displayName"),description=Str(t,"description"),componentId=Str(t,"targetComponentId"),terminalId=Str(t,"targetTerminalId"),positiveId=Str(t,"requiredPositiveTerminalId"),negativeId=Str(t,"requiredNegativeTerminalId"),rawType=Int(t,"targetType"),minimumVoltage=Float(t,"minimumVoltage"),minimumCurrent=Float(t,"minimumCurrent"),minimumPower=Float(t,"minimumPower")};
                if(string.IsNullOrEmpty(dto.title))dto.title=dto.id;if(string.IsNullOrEmpty(dto.id)||!targetIds.Add(dto.id))Issue(e,"Empty or duplicate target ID at index "+i+" (legacy behavior unchanged)");e.targets.Add(dto);
            }
            else Issue(e,"Targets are not an array",true);
            if(e.requiredTargets<0||e.requiredTargets>e.targets.Count)Issue(e,"Required target count outside configured range",true);else if(e.requiredTargets!=e.targets.Count)Issue(e,"Required count differs from configured count; preserve legacy completion semantics");
            return e;
        }
        private static Sprite Thumbnail(SparkAAALevel e)
        { return AssetDatabase.LoadAssetAtPath<Sprite>(Generated+"/Thumbnails/"+e.key+".png"); }
        public static SparkAAACatalog Scan()
        {
            if(Busy)throw new InvalidOperationException("Architect is busy."); Busy=true;
            try{
                Folders();var chapters=Chapters();var byNumber=new SortedDictionary<int,List<SparkAAALevel>>();var report=new StringBuilder("PROJECT SPARK / CAMPAIGN SOURCE AUDIT\n");
                foreach(string guid in AssetDatabase.FindAssets("t:SparkLevelDefinition",new[]{"Assets"})){
                    string path=AssetDatabase.GUIDToAssetPath(guid);var asset=AssetDatabase.LoadMainAssetAtPath(path) as ScriptableObject;
                    if(asset==null||asset.GetType().FullName!=OriginalType){report.AppendLine("SKIPPED incompatible source: "+path);continue;}
                    var match=NumberPattern.Match(Path.GetFileNameWithoutExtension(path));if(!match.Success)match=NumberPattern.Match(asset.name);
                    if(!match.Success||!int.TryParse(match.Groups[1].Value,out int num)||num<1){report.AppendLine("UNMAPPED source (requires explicit naming review): "+path);continue;}
                    var e=Read(num,path,guid,asset,chapters);if(!byNumber.TryGetValue(num,out var list)){list=new List<SparkAAALevel>();byNumber.Add(num,list);}list.Add(e);
                }
                var entries=new List<SparkAAALevel>();int sources=0;int targets=0;foreach(var pair in byNumber){sources+=pair.Value.Count;foreach(var e in pair.Value)targets+=e.targets.Count;}
                for(int n=1;n<=130;n++)entries.Add(Resolve(n,chapters,byNumber));foreach(var pair in byNumber)if(pair.Key>130)entries.Add(Resolve(pair.Key,chapters,byNumber));
                var legacyIds=new Dictionary<string,SparkAAALevel>(StringComparer.OrdinalIgnoreCase);foreach(var e in entries){if(!e.Present)continue;if(legacyIds.TryGetValue(e.legacyId,out var other)){Issue(e,"Duplicate legacy ID",true);Issue(other,"Duplicate legacy ID",true);}else legacyIds.Add(e.legacyId,e);}
                if(AssetDatabase.LoadMainAssetAtPath(CatalogPath)!=null && !(AssetDatabase.LoadMainAssetAtPath(CatalogPath) is SparkAAACatalog))throw new InvalidOperationException("Foreign asset at catalog path.");
                var catalog=AssetDatabase.LoadAssetAtPath<SparkAAACatalog>(CatalogPath);if(catalog==null){catalog=ScriptableObject.CreateInstance<SparkAAACatalog>();AssetDatabase.CreateAsset(catalog,CatalogPath);}
                var visualSettings=SparkAAAVisualArchitect.Settings(false);
                foreach(var e in entries){
                    var binding=visualSettings==null?null:SparkAAAVisualArchitect.Find(visualSettings,e.key);
                    bool showArtwork=e.Present&&(binding==null||SparkAAAVisualArchitect.Approved(e,binding));
                    e.thumbnail=showArtwork?Thumbnail(e):null;
                    string resource="SparkAAAVisuals/"+e.key+"/Banner";
                    e.bannerResourcePath=showArtwork&&AssetDatabase.LoadAssetAtPath<Sprite>(Generated+"/Resources/"+resource+".png")!=null?resource:"";
                }
                catalog.levels=entries;catalog.chapters=chapters;catalog.scannedUtc=DateTime.UtcNow.ToString("O");catalog.schemaVersion=SparkAAACatalog.CurrentSchema;EditorUtility.SetDirty(catalog);AssetDatabase.SaveAssetIfDirty(catalog);
                report.AppendLine($"Sources parsed: {sources} | Configured targets: {targets} | Slots: {entries.Count}");
                int blocked=0;foreach(var e in entries){if(e.blocked)blocked++;if(!string.IsNullOrEmpty(e.issue))report.AppendLine($"{e.key} {(e.blocked?"BLOCK":"REVIEW")}: {e.issue}");}
                report.AppendLine("Blocked slots: "+blocked);report.AppendLine("Scene, solver, legacy progression and runtime launch are NOT verified by this audit.");
                File.WriteAllText(Generated+"/CampaignAudit.txt",report.ToString());AssetDatabase.ImportAsset(Generated+"/CampaignAudit.txt");Debug.Log("[Spark AAA] "+sources+" source definitions; "+targets+" targets; "+blocked+" blocked slots. See CampaignAudit.txt.");
                return catalog;
            }finally{Busy=false;}
        }
        private static SparkAAALevel Resolve(int n,List<SparkAAAChapter> chapters,SortedDictionary<int,List<SparkAAALevel>> map)
        {
            if(!map.TryGetValue(n,out var list)){var missing=Slot(n,chapters);missing.title="Content unavailable";Issue(missing,"No source definition discovered",true);return missing;}
            if(list.Count==1)return list[0];var e=Slot(n,chapters);e.title="Conflicting level definitions";foreach(var item in list)Issue(e,"Duplicate candidate: "+item.sourcePath,true);return e;
        }
        [MenuItem(Menu+"01 Audit and refresh campaign")]
        public static void RefreshMenu()=>Scan();
        [MenuItem(Menu+"02 Build COMPLETE AAA presentation")]
        public static void BuildMenu()
        {
            if(!EditorUtility.DisplayDialog("Build AAA presentation","Regenerate the owned catalog, panel, card and preview scene under AAAArchitecture/Generated? Existing level assets and scenes are not modified. Do not keep manual changes inside generated prefabs.","Build","Cancel"))return;
            Build();
        }
        // Safe for command line -executeMethod; opens no existing project scene.
        public static void Build()
        {
            if(TMP_Settings.defaultFontAsset==null)throw new InvalidOperationException("Import TMP Essential Resources before building.");
            if(SceneManager.GetSceneByPath(ScenePath).isLoaded)throw new InvalidOperationException("Close generated preview scene before rebuilding.");
            var catalog=Scan(); Folders();
            RequireOwnedPrefab<SparkAAALevelCard>(CardPath);RequireOwnedPrefab<SparkAAAMissionPanel>(PanelPath);
            var card=SparkAAALevelCard.Create();GameObject cardAsset;
            try{cardAsset=PrefabUtility.SaveAsPrefabAsset(card.gameObject,CardPath);if(cardAsset==null)throw new IOException("Card prefab save failed");}finally{UnityEngine.Object.DestroyImmediate(card.gameObject);}
            var panel=SparkAAAMissionPanel.Create(catalog,cardAsset.GetComponent<SparkAAALevelCard>());GameObject panelAsset;
            try{panelAsset=PrefabUtility.SaveAsPrefabAsset(panel.gameObject,PanelPath);if(panelAsset==null)throw new IOException("Panel prefab save failed");}finally{UnityEngine.Object.DestroyImmediate(panel.gameObject);}
            CreatePreviewScene(panelAsset);Debug.Log("[Spark AAA] Generated presentation and preview scene. Gameplay bridge remains unapproved.");
        }
        private static void RequireOwnedPrefab<T>(string path) where T:Component
        { var existing=AssetDatabase.LoadMainAssetAtPath(path);if(existing!=null&&(!(existing is GameObject go)||go.GetComponent<T>()==null))throw new InvalidOperationException("Foreign asset at "+path); }
        private static void CreatePreviewScene(GameObject panelAsset)
        {
            if(SceneManager.GetSceneByPath(ScenePath).isLoaded)throw new InvalidOperationException("Close the generated preview scene before rebuilding it.");
            // Existing generated scene is overwritten only by explicit build. No scene build settings are altered.
            var previous=SceneManager.GetActiveScene();var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Additive);
            try{
                SceneManager.SetActiveScene(scene);
                var canvasObject=new GameObject("AAA Mission Control Canvas",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
                var canvas=canvasObject.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;var scaler=canvasObject.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1920,1080);scaler.matchWidthOrHeight=.5f;
                var panel=(GameObject)PrefabUtility.InstantiatePrefab(panelAsset,scene);panel.transform.SetParent(canvasObject.transform,false);SparkAAAUIFactory.Stretch((RectTransform)panel.transform);
                var gateway=panel.AddComponent<SparkAAAEventGateway>();var so=new SerializedObject(panel.GetComponent<SparkAAAMissionPanel>());so.FindProperty("gateway").objectReferenceValue=gateway;so.ApplyModifiedPropertiesWithoutUndo();
                var input=new GameObject("EventSystem",typeof(EventSystem));var inputType=Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");
                if(inputType!=null)input.AddComponent(inputType);else input.AddComponent<StandaloneInputModule>();
                if(!EditorSceneManager.SaveScene(scene,ScenePath))throw new IOException("Scene save failed: "+ScenePath);
            }finally{if(previous.IsValid()&&previous.isLoaded)SceneManager.SetActiveScene(previous);EditorSceneManager.CloseScene(scene,true);}
        }
        [MenuItem(Menu+"03 Open generated preview scene")]
        public static void OpenScene(){if(!File.Exists(ScenePath)){Debug.LogError("Build presentation first.");return;}if(EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())EditorSceneManager.OpenScene(ScenePath,OpenSceneMode.Single);}
        [MenuItem(Menu+"04 Toggle automatic catalog refresh")]
        public static void Toggle(){AutoRefresh=!AutoRefresh;Debug.Log("[Spark AAA] Automatic catalog refresh: "+AutoRefresh);}
        [MenuItem(Menu+"04 Toggle automatic catalog refresh",true)]
        private static bool ToggleCheck(){UnityEditor.Menu.SetChecked(Menu+"04 Toggle automatic catalog refresh",AutoRefresh);return true;}
        [MenuItem(Menu+"05 Open source audit")]
        public static void OpenReport(){var a=AssetDatabase.LoadAssetAtPath<TextAsset>(Generated+"/CampaignAudit.txt");if(a!=null)Selection.activeObject=a;else Debug.LogWarning("Run source audit first.");}
    }
    internal sealed class SparkAAAAssetWatcher:AssetPostprocessor
    {
        private static bool queued;
        private static bool Candidate(string path)=>path.StartsWith("Assets/",StringComparison.Ordinal)&&!path.StartsWith(SparkAAAArchitect.Generated+"/",StringComparison.Ordinal)&&path.EndsWith(".asset",StringComparison.OrdinalIgnoreCase);
        private static void OnPostprocessAllAssets(string[] imported,string[] deleted,string[] moved,string[] movedFrom)
        {
            if(!SparkAAAArchitect.AutoRefresh||SparkAAAArchitect.Busy||queued)return;bool any=false;
            foreach(var group in new[]{imported,deleted,moved,movedFrom})foreach(string path in group)if(Candidate(path)){any=true;break;}
            if(!any)return;queued=true;EditorApplication.delayCall+=Refresh;
        }
        private static void Refresh(){queued=false;if(EditorApplication.isPlayingOrWillChangePlaymode)return;if(EditorApplication.isCompiling||EditorApplication.isUpdating||SparkAAAArchitect.Busy){queued=true;EditorApplication.delayCall+=Refresh;return;}try{SparkAAAArchitect.Scan();}catch(Exception ex){Debug.LogException(ex);}}
    }
}
