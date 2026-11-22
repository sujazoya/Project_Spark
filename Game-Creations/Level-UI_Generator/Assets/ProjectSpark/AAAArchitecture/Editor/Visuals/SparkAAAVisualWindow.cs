using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace ProjectSpark.AAAArchitecture.Editor
{
    public sealed class SparkAAAVisualWindow:EditorWindow
    {
        private Vector2 scroll;
        private int levelNumber=1;
        private GameObject selectedPrefab;
        private Dictionary<int,List<GameObject>> candidates;
        private bool force,replace;
        [MenuItem(SparkAAAVisualArchitect.Menu+"01 Open Visual Architect",false,0)]
        public static void Open()=>GetWindow<SparkAAAVisualWindow>("AAA Visual Architect");
        private static void Run(Action a){try{a();}catch(Exception e){Debug.LogException(e);}}
        private void OnGUI()
        {
            scroll=EditorGUILayout.BeginScrollView(scroll);
            EditorGUILayout.LabelField("PROJECT SPARK / AAA VISUAL ARCHITECT",EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Render actual approved static mesh assets. No gameplay scripts or existing scenes are instantiated. Missing/multiple sources stay blocked; review each match before approval.",MessageType.Info);
            var catalog=SparkAAAVisualArchitect.Catalog();var s=SparkAAAVisualArchitect.Settings(false);
            using(new EditorGUI.DisabledScope(EditorApplication.isPlayingOrWillChangePlaymode||SparkAAAVisualArchitect.Busy)){
                if(GUILayout.Button("SCAN CAMPAIGN AND DISCOVER PREFAB CANDIDATES",GUILayout.Height(35)))Run(()=>{catalog=SparkAAAArchitect.Scan();s=SparkAAAVisualArchitect.Settings();candidates=SparkAAAVisualArchitect.Discover();});
                if(catalog==null||s==null){EditorGUILayout.EndScrollView();return;}
                int approved=0,unique=0,ambiguous=0;foreach(var e in catalog.levels){if(SparkAAAVisualArchitect.Approved(e,SparkAAAVisualArchitect.Find(s,e.key)))approved++;if(candidates!=null&&candidates.TryGetValue(e.number,out var l)){if(l.Count==1)unique++;else ambiguous++;}}
                EditorGUILayout.LabelField("Catalog",$"{catalog.levels.Count} slots / {approved} approved sources");EditorGUILayout.LabelField("Candidate scan",candidates==null?"Not scanned":$"{unique} single / {ambiguous} multiple");
                if(candidates!=null&&GUILayout.Button("Review and approve unique matches"))Run(()=>ApproveUnique(catalog,s));
                EditorGUILayout.Space();levelNumber=EditorGUILayout.IntField("Level number",levelNumber);var entry=catalog.Find($"PS-L{levelNumber:000}");
                if(entry!=null){EditorGUILayout.LabelField(entry.title);var binding=SparkAAAVisualArchitect.Find(s,entry.key);EditorGUILayout.ObjectField("Approved source",binding?.prefab,typeof(GameObject),false);
                    if(candidates!=null&&candidates.TryGetValue(levelNumber,out var list))foreach(var go in list){EditorGUILayout.BeginHorizontal();EditorGUILayout.ObjectField(go,typeof(GameObject),false);if(GUILayout.Button("Choose",GUILayout.Width(65)))selectedPrefab=go;EditorGUILayout.EndHorizontal();}
                    selectedPrefab=(GameObject)EditorGUILayout.ObjectField("Source to approve",selectedPrefab,typeof(GameObject),false);
                    if(GUILayout.Button("Approve source for selected level"))Run(()=>{if(EditorUtility.DisplayDialog("Approve visual source",entry.key+" / "+entry.title+"\n"+AssetDatabase.GetAssetPath(selectedPrefab),"Approve","Cancel"))SparkAAAVisualArchitect.Bind(s,entry,selectedPrefab);});
                }
                EditorGUILayout.Space();EditorGUI.BeginChangeCheck();s.padding=EditorGUILayout.Slider("Camera padding",s.padding,1.08f,2f);s.background=EditorGUILayout.ColorField("Studio background",s.background);s.fullSizeThumbnails=EditorGUILayout.Toggle("Import thumbnails at 1024",s.fullSizeThumbnails);s.autoRefresh=EditorGUILayout.Toggle("Auto render changed approved sources",s.autoRefresh);if(EditorGUI.EndChangeCheck()){EditorUtility.SetDirty(s);AssetDatabase.SaveAssetIfDirty(s);}
                force=EditorGUILayout.Toggle("Force recapture",force);replace=EditorGUILayout.Toggle("Permit replacing unowned/edited PNGs",replace);
                if(GUILayout.Button("GENERATE SELECTED LEVEL",GUILayout.Height(32)))Run(()=>Generate(levelNumber));
                if(GUILayout.Button("BUILD COMPLETE AAA VISUAL LIBRARY",GUILayout.Height(45)))Run(()=>Generate(0));
                if(GUILayout.Button("Validate visual library"))Run(SparkAAAVisualArchitect.Validate);if(GUILayout.Button("Open visual audit"))SparkAAAVisualArchitect.OpenReport();
            }
            EditorGUILayout.EndScrollView();
        }
        private void Generate(int n){if(replace&&!EditorUtility.DisplayDialog("Replace existing artwork?","Existing unowned or externally edited images will be backed up and overwritten for eligible approved levels.","Replace","Cancel"))return;SparkAAAVisualArchitect.Build(force,replace,n);}
        private void ApproveUnique(SparkAAACatalog c,SparkAAAVisualSettings s)
        {
            var pending=new List<SparkAAALevel>();var text=new System.Text.StringBuilder();foreach(var e in c.levels){if(!e.Present||e.blocked||SparkAAAVisualArchitect.Find(s,e.key)!=null||!candidates.TryGetValue(e.number,out var l)||l.Count!=1)continue;pending.Add(e);text.AppendLine(e.key+" / "+e.title+" => "+AssetDatabase.GetAssetPath(l[0]));}
            if(pending.Count==0){EditorUtility.DisplayDialog("No new unique matches","Assign unresolved sources manually.","OK");return;}
            // Full list is available for inspection, rather than silently approving names.
            string report=SparkAAAVisualArchitect.Root+"/VisualCandidateReview.txt";System.IO.File.WriteAllText(report,text.ToString());AssetDatabase.ImportAsset(report);Selection.activeObject=AssetDatabase.LoadAssetAtPath<TextAsset>(report);
            SparkAAAVisualReviewWindow.Open(c,s,pending,candidates);
        }
    }
    internal sealed class SparkAAAVisualReviewWindow:EditorWindow
    {
        private SparkAAACatalog catalog;private SparkAAAVisualSettings settings;private List<SparkAAALevel> entries;private Dictionary<int,List<GameObject>> matches;private bool[] approved;private Vector2 scroll;
        internal static void Open(SparkAAACatalog c,SparkAAAVisualSettings s,List<SparkAAALevel> e,Dictionary<int,List<GameObject>> m){var w=CreateInstance<SparkAAAVisualReviewWindow>();w.titleContent=new GUIContent("Review visual sources");w.catalog=c;w.settings=s;w.entries=e;w.matches=m;w.approved=new bool[e.Count];w.ShowUtility();}
        private void OnGUI(){if(entries==null){Close();return;}EditorGUILayout.HelpBox("Review hardware identity, check confirmed rows, then approve. Filename matching is not proof of correct hardware.",MessageType.Warning);if(GUILayout.Button("Check all rows (only after review)"))for(int i=0;i<approved.Length;i++)approved[i]=true;scroll=EditorGUILayout.BeginScrollView(scroll);for(int i=0;i<entries.Count;i++){var e=entries[i];approved[i]=EditorGUILayout.ToggleLeft(e.key+" / "+e.title,approved[i]);EditorGUILayout.ObjectField(matches[e.number][0],typeof(GameObject),false);EditorGUILayout.LabelField(AssetDatabase.GetAssetPath(matches[e.number][0]),EditorStyles.wordWrappedMiniLabel);}EditorGUILayout.EndScrollView();if(GUILayout.Button("Approve checked sources")){try{for(int i=0;i<entries.Count;i++)if(approved[i]){var e=catalog.Find(entries[i].key);if(e!=null&&e.guid==entries[i].guid&&SparkAAAVisualArchitect.Find(settings,e.key)==null)SparkAAAVisualArchitect.Bind(settings,e,matches[entries[i].number][0]);}Close();}catch(Exception ex){Debug.LogException(ex);}}}
    }
    internal sealed class SparkAAAVisualWatcher:AssetPostprocessor
    {
        private static bool queued;
        private static bool Candidate(string path)=>path.StartsWith("Assets/",StringComparison.Ordinal)&&!path.StartsWith(SparkAAAVisualArchitect.Root+"/",StringComparison.Ordinal)&&!path.StartsWith(SparkAAAArchitect.Root+"/Editor/",StringComparison.Ordinal)&&!path.EndsWith(".cs",StringComparison.OrdinalIgnoreCase);
        private static void OnPostprocessAllAssets(string[] imported,string[] deleted,string[] moved,string[] movedFrom){if(queued||SparkAAAVisualArchitect.Busy||SparkAAAArchitect.Busy)return;var s=SparkAAAVisualArchitect.Settings(false);if(s==null||!s.autoRefresh)return;foreach(var group in new[]{imported,deleted,moved,movedFrom})foreach(string p in group)if(Candidate(p)){queued=true;EditorApplication.delayCall+=Refresh;return;}}
        private static void Refresh(){queued=false;if(EditorApplication.isPlayingOrWillChangePlaymode)return;if(EditorApplication.isCompiling||EditorApplication.isUpdating||SparkAAAArchitect.Busy||SparkAAAVisualArchitect.Busy){queued=true;EditorApplication.delayCall+=Refresh;return;}var s=SparkAAAVisualArchitect.Settings(false);if(s==null||!s.autoRefresh)return;try{SparkAAAVisualArchitect.Build();}catch(Exception ex){Debug.LogException(ex);}}
    }
}
