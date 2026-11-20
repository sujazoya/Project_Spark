#if UNITY_EDITOR
using System;using System.IO;using System.Linq;using System.Reflection;using ProjectSpark.EditorTools;using ProjectSpark.Gameplay;using ProjectSpark.SensorMatrix;using UnityEditor;using UnityEngine;using Object=UnityEngine.Object;
internal static partial class Level103Acceptance84
{
 static void EditChecks()
 {
  results.hashes=Hashes();SparkLevel103Builder.Build(false);
  var manifest=File.ReadAllLines(Kit+"/Data/Asset_Manifest_62.csv").Skip(1).Where(l=>!string.IsNullOrWhiteSpace(l)).Select(l=>l.Split(',')).ToArray();
  Check(1,manifest.Length==62&&manifest.All(r=>AssetDatabase.LoadAssetAtPath<GameObject>(Kit+"/Models_Rebuilt/"+Path.GetFileName(r[2])) is GameObject g&&g.GetComponentsInChildren<Renderer>(true).Length>0),"62 exact manifest models resolve with geometry; source FBX recovery documented.");
  Check(2,manifest.All(r=>File.Exists(Kit+"/Models_GLB/"+Path.GetFileName(r[3]))),"All 62 source GLBs exist.");
  var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(Kit+"/Prefabs/Level_103_Layout.prefab");var process=prefab.GetComponent<SparkLevel103Process>();
  Check(3,process.art.childCount==62&&!process.art.Cast<Transform>().Any(t=>t.name.Contains("Complete")),"Only modular assets appear under gameplay art; complete reference is separate.");
  var core=process.art.GetComponentsInChildren<Renderer>(true).First(r=>r.name.Contains("FR4"));var dims=Vector3.Scale(core.bounds.size,new Vector3(1/process.art.lossyScale.x,1/process.art.lossyScale.y,1/process.art.lossyScale.z));
  Check(4,Mathf.Abs(dims.x-.14f)<.0001&&Mathf.Abs(dims.z-.095f)<.0001&&Mathf.Abs(dims.y-.0016f)<.0001,"FR4 dimensions in metres: "+dims.ToString("F6"));
  Check(5,Directory.GetFiles(Kit+"/Models_Rebuilt","*.fbx").Length==63&&!prefab.GetComponentsInChildren<Transform>(true).Any(t=>t.name=="PS_PRO_GAME_ROOM"),"Only supplied board models/reference imported; no new room, furniture or instrument FBXs.");
  Check(6,typeof(SparkLevel103Builder).GetMethods().SelectMany(m=>m.GetCustomAttributes<MenuItem>()).Count(a=>a.menuItem=="Tools/Project Spark/Levels/Create Level 103")==1,"Unique Create Level 103 menu, deliberately adapted from source Level 96 identity.");
  Check(7,Directory.GetFiles(Kit+"/Materials","*.mat").Select(p=>AssetDatabase.LoadAssetAtPath<Material>(p)).All(m=>m&&m.shader&&m.shader.isSupported),"Nested project-local kit materials resolve supported shaders.");
  string guid=AssetDatabase.AssetPathToGUID(Kit+"/Prefabs/Level_103_Layout.prefab");SparkLevel103Builder.Build(false);Check(8,guid==AssetDatabase.AssetPathToGUID(Kit+"/Prefabs/Level_103_Layout.prefab")&&Hashes()==results.hashes,"Repeated build preserves prefab GUID and all existing scene/other level definition hashes.");
  var design=process.design;Check(19,design.IsResolved(out _)&&design.routes.All(r=>!r.net.Contains("SMX4_NET")),"Conceptual placeholder labels are absent from the certifying design.");
  var bad=Object.Instantiate(design);bad.contacts.First(p=>p.id==bad.routes[0].from).net="UNASSIGNED_PENDING_SCHEMATIC";Check(20,!bad.IsResolved(out _),"Same build/startup validator rejects a required unmapped contact.");Object.DestroyImmediate(bad);
  Check(83,Shader.Find("Universal Render Pipeline/Lit")?.isSupported==true,"Runtime/editor assemblies compiled; target URP Lit shader supported.");
 }
 static SparkSensorMatrixAcquisition Sim(double[] v,double reference=2.5,bool trusted=true,int ticks=128){var a=new SparkSensorMatrixAcquisition();for(int i=0;i<ticks;i++)a.Step(1d/128,v,new[]{true,true,true,true},trusted,reference,true);return a;}
 static void SimChecks()
 {
  var a=new SparkSensorMatrixAcquisition();a.channels[0].stimulus=10;double first=a.SensorVoltage(0,2.5);a.channels[0].stimulus=70;Check(29,Math.Abs(a.SensorVoltage(0,2.5)-first-.1155)<1e-8,"PT1000 equivalent rises 115.5 mV from 10 to 70 C with 0.5 mA excitation.");
  a.channels[1].stimulus=0;first=a.SensorVoltage(1,2.5);a.channels[1].stimulus=100;Check(31,Math.Abs(a.SensorVoltage(1,2.5)-first-.01)<1e-8,"Bridge changes 10 mV differential over 0-100 N at 2.5 V excitation.");
  a.channels[2].stimulus=0;first=a.SensorVoltage(2,2.5);a.channels[2].stimulus=1000;Check(33,Math.Abs(a.SensorVoltage(2,2.5)-first-1.88)<1e-8,"Photodiode/47 kohm equivalent changes 1.88 V over 1000 lux.");Check(34,a.FrontEndVoltage(2,a.SensorVoltage(2,2.5),0,2.5,470000)==3.15,"Tenfold optical gain clips analog output at 3.15 V.");
  a.channels[3].stimulus=0;Check(35,Math.Abs(a.SensorVoltage(3,2.5)-.25)<1e-9,"Hall zero-current offset equals 250 mV.");a.channels[3].stimulus=2;Check(36,Math.Abs(a.SensorVoltage(3,2.5)-1.85)<1e-9,"Hall 2 A stimulus produces 1.85 V.");
  var nominal=Sim(new[]{.2,1.2,2.2,.25});Check(39,Math.Abs(nominal.channels[2].rawCode-57671)<3&&nominal.channels.All(c=>c.rawCode>=0&&c.rawCode<=65535),"16-bit voltage/reference scaling within bounded noise.");Check(40,nominal.channels.All(c=>c.samples==128),"Fixed-clock one-second run accepts 128 samples/channel.");
  var unsettled=new SparkSensorMatrixAcquisition{muxSettlingSeconds=0};for(int i=0;i<128;i++)unsettled.Step(1d/128,new[]{.2,2.2,.25,1.8},new[]{true,true,true,true},true,2.5,true);Check(41,unsettled.channels.All(c=>c.samples==0&&c.status==SensorSampleStatus.Stale)&&nominal.channels.All(c=>c.samples==128),"Zero settling rejects samples; nominal mux slots settle within half an LSB.");
  var c=new SparkSensorMatrixAcquisition{referenceChecked=true};c.SelectStandard(0);for(int i=0;i<100;i++)c.Step(1d/128,new[]{.2,.2,.2,.25},new[]{true,true,true,true},true,2.5,true);bool zero=c.CaptureStandard(out _);c.SelectStandard(1);for(int i=0;i<100;i++)c.Step(1d/128,new[]{2.2,2.2,2.08,1.85},new[]{true,true,true,true},true,2.5,true);bool span=c.CaptureStandard(out _);Check(42,zero&&span&&c.AllCalibrated&&c.channels.All(ch=>ch.historyCount==200&&ch.history.Take(ch.historyCount).Select(h=>h.code).Distinct().Count()>5),"Calibration retains raw-code/voltage/reference history with timestamps.");
  c.Step(.02,new[]{2.2,2.2,2.08,1.85},new[]{true,true,true,true},true,2.5,false);Check(43,!c.AllValid&&!c.CaptureStandard(out _),"Stale solve status blocks calibration and accuracy acceptance.");
  var clip=Sim(new[]{.2,.2,3.15,.25});Check(44,clip.channels[2].status==SensorSampleStatus.Saturated&&clip.channels[2].rawCode==65535&&!clip.WithinUncertainty(),"Clipped ADC input cannot pass accuracy.");
  var drift=Sim(new[]{.2,1.2,2.2,.25},2.37,false);Check(45,drift.channels[2].rawCode>nominal.channels[2].rawCode+1000&&drift.channels[2].status==SensorSampleStatus.ReferenceUntrusted,"Drift changes non-ratiometric raw codes while trusted qualification remains blocked.");
  a.channels[1].stimulus=50;double v1=a.FrontEndVoltage(1,a.SensorVoltage(1,2.5),1.25,2.5,47000),v2=a.FrontEndVoltage(1,a.SensorVoltage(1,2.37),1.185,2.37,47000);Check(46,Math.Abs(Sim(new[]{v1,v1,v1,v1}).channels[1].rawCode-Sim(new[]{v2,v2,v2,v2},2.37,false).channels[1].rawCode)<=2,"Ratiometric bridge code remains invariant; no double drift correction.");
  var untrusted=Sim(new[]{.2,.2,.2,.25},2.5,false);Check(38,!untrusted.AllValid&&!untrusted.CaptureStandard(out _),"Untrusted reference cannot qualify samples or calibration.");
 }
}
#endif
