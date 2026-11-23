using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.AAAArchitecture.Editor
{
    public sealed class SparkEarlyVisualRecoveryWindow : EditorWindow
    {
        private const string Menu=SparkAAAVisualArchitect.Menu+"05 Recover missing level visuals";
        private static readonly Regex Token=new Regex(@"(?:^|[/_ .-])(?:PS-L|Level|L)[_ -]*0*(\d+)(?=$|[/_ .-])", RegexOptions.IgnoreCase|RegexOptions.CultureInvariant);
        private readonly List<Row> rows=new List<Row>();
        private int first=1,last=29;
        private int scannedFirst,scannedLast;
        private Vector2 scroll;
        private SparkAAAVisualSettings settings;
        private bool fullSize=true;
        private sealed class Row
        {
            public SparkAAALevel entry;
            public List<GameObject> candidates=new List<GameObject>();
            public GameObject choice;
            public bool check;
        }
        [MenuItem(Menu)]
        public static void Open()=>GetWindow<SparkEarlyVisualRecoveryWindow>("Missing Level Visuals");
        private static bool Idle=>!EditorApplication.isPlayingOrWillChangePlaymode&&!EditorApplication.isCompiling&&!EditorApplication.isUpdating&&!SparkAAAVisualArchitect.Busy&&!SparkAAAArchitect.Busy;
        private static bool ValidRange(int a,int b)=>a>=1&&b>=a&&b-a<=999;
        private void OnGUI()
        {
            EditorGUILayout.LabelField("RECOVER MISSING LEVEL ARTWORK",EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Find level-specific prefab/model candidates, including exact definition dependencies. Review the source before approval. Generation creates missing PNGs only; existing artwork is preserved. No source scenes are opened.",MessageType.Info);
            first=EditorGUILayout.IntField("First level",first);last=EditorGUILayout.IntField("Last level",last);
            fullSize=EditorGUILayout.Toggle("Import full HD thumbnails (1024)",fullSize);
            bool fresh=rows.Count>0&&first==scannedFirst&&last==scannedLast;
            using(new EditorGUI.DisabledScope(!Idle||!ValidRange(first,last))){
                if(GUILayout.Button("SCAN RANGE AND DISCOVER SOURCES",GUILayout.Height(32)))Run(Scan);
                using(new EditorGUI.DisabledScope(!fresh)){
                    if(GUILayout.Button("Check unique candidates after visual review"))foreach(var row in rows)if(!Approved(row)&&row.candidates.Count==1){row.choice=row.candidates[0];row.check=true;}
                    if(GUILayout.Button("APPROVE CHECKED SOURCES"))Run(Approve);
                    if(GUILayout.Button("GENERATE MISSING VISUALS IN RANGE",GUILayout.Height(36)))Run(Generate);
                }
            }
            if(!fresh){EditorGUILayout.HelpBox("Scan the current range first.",MessageType.Info);return;}
            scroll=EditorGUILayout.BeginScrollView(scroll);
            foreach(var row in rows){
                var e=row.entry;
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField(e.key+" / "+e.title,EditorStyles.boldLabel);
                string thumb=SparkAAAVisualArchitect.Root+"/Thumbnails/"+e.key+".png";
                string banner=SparkAAAVisualArchitect.Root+"/Resources/SparkAAAVisuals/"+e.key+"/Banner.png";
                EditorGUILayout.LabelField("Thumbnail / Banner",(File.Exists(thumb)?"Present":"Missing")+" / "+(File.Exists(banner)?"Present":"Missing"));
                if(!e.Present||e.blocked){EditorGUILayout.HelpBox("Definition blocked: "+e.issue,MessageType.Warning);EditorGUILayout.EndVertical();continue;}
                var binding=SparkAAAVisualArchitect.Find(settings,e.key);
                if(Approved(row))EditorGUILayout.ObjectField("Approved source",binding.prefab,typeof(GameObject),false);
                else{
                    EditorGUILayout.LabelField("Candidates",row.candidates.Count.ToString());
                    foreach(var go in row.candidates){
                        EditorGUILayout.BeginHorizontal();EditorGUILayout.ObjectField(go,typeof(GameObject),false);
                        if(GUILayout.Button("Choose",GUILayout.Width(65)))row.choice=go;
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.LabelField(AssetDatabase.GetAssetPath(go),EditorStyles.wordWrappedMiniLabel);
                    }
                    row.choice=(GameObject)EditorGUILayout.ObjectField("Approve this source",row.choice,typeof(GameObject),false);
                    row.check=EditorGUILayout.ToggleLeft("I verified this is the correct complete level hardware",row.check);
                    if(row.candidates.Count==0)EditorGUILayout.HelpBox("Drag the complete level prefab or imported model here. If assembled in a scene, save the relevant workbench geometry as a prefab first, not the whole scene.",MessageType.None);
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndScrollView();
        }
        private bool Approved(Row row)=>SparkAAAVisualArchitect.Approved(row.entry,SparkAAAVisualArchitect.Find(settings,row.entry.key));
        private static void Run(Action action){try{action();}catch(Exception ex){Debug.LogException(ex);}}
        private void Scan()
        {
            rows.Clear();scannedFirst=0;scannedLast=0;
            var catalog=SparkAAAArchitect.Scan();settings=SparkAAAVisualArchitect.Settings();
            var byNumber=new Dictionary<int,Row>();var definitions=new Dictionary<string,Row>(StringComparer.Ordinal);
            foreach(var e in catalog.levels){if(e.number<first||e.number>last)continue;var row=new Row{entry=e};rows.Add(row);if(!e.Present||e.blocked)continue;byNumber[e.number]=row;if(!string.IsNullOrEmpty(e.sourcePath))definitions[e.sourcePath]=row;}
            var paths=new HashSet<string>(StringComparer.Ordinal);
            foreach(string filter in new[]{"t:Prefab","t:Model"})foreach(string guid in AssetDatabase.FindAssets(filter,new[]{"Assets"})){
                string path=AssetDatabase.GUIDToAssetPath(guid);if(!path.StartsWith(SparkAAAVisualArchitect.Root+"/",StringComparison.Ordinal))paths.Add(path);
            }
            var ordered=new List<string>(paths);ordered.Sort(StringComparer.Ordinal);
            try{
                for(int i=0;i<ordered.Count;i++){
                    string path=ordered[i];if(i%32==0&&EditorUtility.DisplayCancelableProgressBar("Discover early level sources",path,(float)i/Mathf.Max(1,ordered.Count))){rows.Clear();return;}
                    var hits=new HashSet<Row>();
                    int number=-1;bool conflict=false;
                    foreach(Match match in Token.Matches(path))if(int.TryParse(match.Groups[1].Value,out int value)&&value>0){if(number!=-1&&number!=value)conflict=true;number=value;}
                    if(!conflict&&byNumber.TryGetValue(number,out var named))hits.Add(named);
                    // Direct dependencies are candidates, not proof of a complete layout.
                    foreach(string dependency in AssetDatabase.GetDependencies(path,false))if(definitions.TryGetValue(dependency,out var direct))hits.Add(direct);
                    if(hits.Count==0)continue;
                    var go=AssetDatabase.LoadAssetAtPath<GameObject>(path);if(!SparkAAAVisualArchitect.ValidPrefab(go))continue;
                    foreach(var row in hits)row.candidates.Add(go);
                }
            }finally{EditorUtility.ClearProgressBar();}
            foreach(var row in rows)if(!Approved(row)&&row.candidates.Count==1)row.choice=row.candidates[0];
            scannedFirst=first;scannedLast=last;
        }
        private void Approve()
        {
            int count=0;
            foreach(var row in rows)if(row.check&&!Approved(row)){
                if(!SparkAAAVisualArchitect.ValidPrefab(row.choice))throw new InvalidOperationException("Invalid source for "+row.entry.key);
                count++;
            }
            if(count==0)return;
            if(!EditorUtility.DisplayDialog("Approve visual sources","Approve "+count+" reviewed level sources? Existing approved bindings are retained.","Approve","Cancel"))return;
            foreach(var row in rows)if(row.check&&!Approved(row)){SparkAAAVisualArchitect.Bind(settings,row.entry,row.choice);row.check=false;}
        }
        private void Generate()
        {
            if(!EditorUtility.DisplayDialog("Generate missing artwork",$"Levels {first:000}–{last:000}. Existing artwork is not replaced. Unapproved sources are skipped. Continue?","Generate","Cancel"))return;
            settings.fullSizeThumbnails=fullSize;EditorUtility.SetDirty(settings);AssetDatabase.SaveAssetIfDirty(settings);
            SparkAAAVisualArchitect.Build(false,false,0,true,first,last,true);
            SparkAAAVisualArchitect.OpenReport();
        }
    }
}
