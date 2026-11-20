#if UNITY_EDITOR
using System;using System.Collections.Generic;using System.IO;using System.Linq;using System.Reflection;using System.Security.Cryptography;using System.Threading.Tasks;
using AAAUI.VFX;using ProjectSpark.Circuit;using ProjectSpark.EditorTools;using ProjectSpark.Gameplay;using ProjectSpark.PCBVirtualStudio;using ProjectSpark.SensorMatrix;using ProjectSpark.Tools;using ProjectSpark.UIManagement;
using UnityEditor;using UnityEngine;using UnityEngine.EventSystems;using Object=UnityEngine.Object;
[InitializeOnLoad]internal static partial class Level103Acceptance84
{
 const string Kit="Assets/ProjectSpark/Level103",Report="Logs/SparkLevel103_Audit84",Key="Spark103.Audit84";
 [Serializable]internal sealed class Row{public int number;public bool pass;public string evidence;}
 [Serializable]internal sealed class Results{public List<Row> rows=new();public string hashes;}
 static Results results=new();static SparkLevel103Process b;static LevelGamePlayManager manager;
 static Level103Acceptance84(){EditorApplication.update+=Poll;EditorApplication.playModeStateChanged+=State;}
 static void Check(int n,bool condition,string evidence){results.rows.RemoveAll(r=>r.number==n);results.rows.Add(new Row{number=n,pass=condition,evidence=evidence});File.WriteAllText(Report+".json",JsonUtility.ToJson(results,true));File.AppendAllText(Report+".log",$"{(condition?"PASS":"FAIL")} L103-AT-{n:000}: {evidence}\n");}
 static void Assert(bool value,string reason){if(!value)throw new Exception(reason);}
 static bool Pair(SparkTerminal a,SparkTerminal z,SparkTerminal x,SparkTerminal y)=>(a==x&&z==y)||(a==y&&z==x);
 static List<SparkCircuitConnection> Edges(){var list=new List<SparkCircuitConnection>();manager.CircuitSystem.CopyConnections(list);return list;}
 static bool Has(SparkTerminal a,SparkTerminal z)=>Edges().Any(e=>e.Kind==SparkConnectionKind.Wire&&Pair(a,z,e.A,e.B));
 static string Hashes()=>string.Join("\n",AssetDatabase.FindAssets("t:SparkLevelDefinition").Select(AssetDatabase.GUIDToAssetPath).Where(p=>!p.StartsWith(Kit)).Concat(AssetDatabase.FindAssets("t:Scene").Select(AssetDatabase.GUIDToAssetPath)).OrderBy(p=>p).Select(p=>p+":"+Convert.ToBase64String(SHA256.Create().ComputeHash(File.ReadAllBytes(p)))));
 static void Poll(){if(!File.Exists("Logs/SparkLevel103Audit84.request")||EditorApplication.isCompiling||EditorApplication.isUpdating||EditorApplication.isPlayingOrWillChangePlaymode)return;File.Delete("Logs/SparkLevel103Audit84.request");results=new Results();File.WriteAllText(Report+".log","LEVEL 103 / NUMBERED ACCEPTANCE AUDIT\n");try{EditChecks();SimChecks();SessionState.SetBool(Key,true);EditorApplication.EnterPlaymode();}catch(Exception e){File.AppendAllText(Report+".log","SETUP FAILED "+e);Finish();}}
 static async void State(PlayModeStateChange state)
 {
  if(!SessionState.GetBool(Key,false))return;if(state==PlayModeStateChange.EnteredEditMode){SessionState.EraseBool(Key);Finish();return;}if(state!=PlayModeStateChange.EnteredPlayMode)return;
  results=JsonUtility.FromJson<Results>(File.ReadAllText(Report+".json"));var old=Debug.unityLogger.filterLogType;bool bg=Application.runInBackground;Debug.unityLogger.filterLogType=LogType.Error;Application.runInBackground=true;
  const string saveKey="ProjectSpark.Level103.Board.v1";bool hadSave=PlayerPrefs.HasKey(saveKey);string original=PlayerPrefs.GetString(saveKey,"");
  try{await PlayChecks();}catch(Exception e){File.AppendAllText(Report+".log","SCENARIO STOPPED "+e+"\n");}finally{if(hadSave)PlayerPrefs.SetString(saveKey,original);else PlayerPrefs.DeleteKey(saveKey);PlayerPrefs.Save();Debug.unityLogger.filterLogType=old;Application.runInBackground=bg;EditorApplication.ExitPlaymode();}
 }
 static async Task Wait(Func<bool> f,string task){var end=DateTime.UtcNow.AddSeconds(40);while(!f()){if(DateTime.UtcNow>end)throw new Exception(task+" | "+b?.heading.text+" | "+b?.detail.text+" | "+b?.readings.text);await Task.Delay(65);}}
 static Task Stage(int n)=>Wait(()=>b.Stage==n&&manager.SatisfiedTargetCount==n,$"Expected {n}/6");
 static void SelectRoute(int n){for(int i=0;i<60&&b.SelectedRoute!=n;i++)b.NextRoute();Assert(b.SelectedRoute==n,"Route selection");}
 static async Task Cal(){await Task.Delay(1100);b.CheckReference();b.ZeroStandard();await Task.Delay(1100);b.Capture();b.SpanStandard();await Task.Delay(1100);b.Capture();await Wait(()=>b.acquisition.AllCalibrated,"Measured calibration");}
 static void Finish()
 {
  if(File.Exists(Report+".json"))results=JsonUtility.FromJson<Results>(File.ReadAllText(Report+".json"));
  var descriptions=File.ReadAllLines("Game-Creations/Level Creations/Level-96/ProjectSpark_Level96_Master_Unity_Implementation_Prompt.md").Select(l=>System.Text.RegularExpressions.Regex.Match(l,@"L96-AT-(\d{3}):\*\* (.+)")).Where(m=>m.Success).ToDictionary(m=>int.Parse(m.Groups[1].Value),m=>m.Groups[2].Value);
  string Csv(string v)=>"\""+(v??"").Replace("\"","\"\"").Replace("\n"," ").Replace("\r","")+"\"";
  var output=new List<string>{"TestID,SourceTestID,Requirement,Status,Evidence"};for(int i=1;i<=84;i++){var row=results.rows.FirstOrDefault(r=>r.number==i);output.Add(string.Join(",",$"L103-AT-{i:000}",$"L96-AT-{i:000}",Csv(descriptions.GetValueOrDefault(i,"Source requirement")),row==null?"NOT RUN":row.pass?"PASS":"FAIL",Csv(row?.evidence??"Scenario stopped before this check.")));}
  File.WriteAllLines(Report+".csv",output);File.AppendAllText(Report+".log",$"SUMMARY {results.rows.Count(r=>r.pass)} PASS / {results.rows.Count(r=>!r.pass)} FAIL / {84-results.rows.Count} NOT RUN. Returned to Edit Mode.\n");
 }
}
#endif
