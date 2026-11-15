#!/usr/bin/env python3
"""Level 83 source-derived conceptual wiring, terminal and Unity art helpers. NOT a gameplay implementation."""
import json,csv,math,re,shutil
from pathlib import Path
from collections import defaultdict
R=Path('/mnt/data/ProjectSpark_Level83_HD_AAA'); U=R/'Unity_Optional';S=R/'Source';U.mkdir(exist_ok=True);S.mkdir(exist_ok=True)
man=json.loads((R/'Model_Manifest.json').read_text());N=[r['name'] for r in man['models']]
def M(i):return N[i]
G=[
('Educational 5V control power and return',[
(68,66,'5V EPS safety PLC power'),(68,67,'5V energy management supervisor supply'),(68,23,'5V port MPPT controller logic'),(68,24,'5V starboard MPPT controller logic'),(68,40,'5V battery management logic'),(68,49,'5V distribution controller logic'),(68,50,'5V priority load controller logic'),(68,69,'5V SCADA logic supply'),(68,65,'5V independent undervoltage protector logic'),(68,30,'5V bus transducer conditioning feed')]),
('Solar wing deployment and tracking',[
(67,7,'Port wing deploy command'),(7,67,'Port wing deployed limit feedback'),(67,8,'Starboard wing deploy command'),(8,67,'Starboard wing deployed limit feedback'),(67,9,'Port azimuth tracking request'),(9,67,'Port array actual azimuth'),(67,10,'Starboard azimuth tracking request'),(10,67,'Starboard array actual azimuth'),(11,67,'Solar tracking encoder measured position'),(67,11,'Tracking encoder calibration enable'),(13,67,'Modeled Sun vector and irradiance diagnostic'),(14,67,'Orbital eclipse shadow boundary status'),(15,67,'Simulated orbital phase status'),(67,62,'Solar array jam fault arm'),(62,67,'Port gimbal jam diagnostic'),(67,16,'Solar string inspection request'),(16,67,'Solar cell string distribution feedback'),(67,20,'Solar combiner isolation command'),(20,67,'Solar combiner ready feedback'),(9,66,'Port gimbal motion safety status')]),
('PV generation and MPPT power conversion',[
(21,23,'Port solar measured array current'),(22,23,'Port solar measured array voltage'),(21,24,'Starboard solar measured current'),(22,24,'Starboard solar measured voltage'),(67,23,'Port MPPT enable request'),(67,24,'Starboard MPPT enable request'),(23,27,'Port MPPT converter duty request'),(24,28,'Starboard MPPT converter duty request'),(27,67,'Port converter measured DC output'),(28,67,'Starboard converter measured DC output'),(23,67,'Port measured MPPT tracking quality'),(24,67,'Starboard measured MPPT tracking quality'),(17,23,'Port IV curve sample data'),(18,24,'Starboard IV curve sample data'),(19,23,'Bypass diode array health'),(20,27,'Solar combined port output monitoring'),(20,28,'Solar combined starboard output monitoring'),(67,27,'Port converter protected enable'),(67,28,'Starboard converter protected enable'),(27,29,'Port converter DC bus power interface'),(28,29,'Starboard converter DC bus power interface'),(25,69,'MPPT perturb observe diagnostic telemetry')]),
('Battery packs, cell sensing and BMS',[
(34,40,'Battery A module presence and pack state'),(35,40,'Battery B module presence and pack state'),(37,40,'Battery individual cell-group voltages'),(38,40,'Battery measured pack current'),(39,40,'Battery pack thermistor array feedback'),(40,41,'BMS cell balancing command'),(41,40,'Cell balancing progress and diagnostic'),(40,42,'BMS safe charge authorization'),(40,43,'BMS safe discharge authorization'),(40,44,'BMS isolation contactor drive'),(44,40,'Actual isolation contactor feedback'),(40,45,'Safe precharge resistor switching request'),(45,40,'Precharge measured completion'),(42,34,'Pack A charge controller output'),(42,35,'Pack B charge controller output'),(34,43,'Battery A available discharge capacity'),(35,43,'Battery B available discharge capacity'),(39,47,'Thermal battery cooling plate control feedback'),(47,40,'Battery thermal plate state'),(40,48,'Battery radiator position request'),(48,40,'Battery radiator actual state'),(63,40,'Controlled cell imbalance fault state')]),
('Main DC bus and distribution',[
(29,30,'Main bus physical potential measurement sense'),(29,31,'Main bus actual shunt current measurement'),(29,32,'Bus bulk capacitor energy interface'),(32,67,'Bus transient ride through measurement'),(29,49,'Regulated DC bus distribution input'),(49,50,'Power distribution priority switching command feed'),(50,51,'P0 essential bus supply permit'),(50,52,'P1 attitude control load switch'),(50,53,'P0 mission computer load switch'),(50,54,'P2 optical telemetry load switch'),(50,55,'P3 science payload load switch'),(50,56,'P1 thermal heater load switch'),(67,49,'Distribution power limit request'),(49,67,'Actual distribution branch current status'),(46,29,'Supercapacitor discharge support status'),(67,46,'Bounded supercapacitor ride-through command'),(65,49,'Independent bus undervoltage distribution inhibit'),(29,65,'Independent main bus voltage protector sense')]),
('Energy measurements and diagnostics',[
(33,67,'Bus transient recorder measured ripple diagnostic'),(30,33,'Bus voltage acquisition to transient recorder'),(31,33,'Shunt current acquisition to transient recorder'),(30,69,'Display only main bus voltage telemetry'),(31,69,'Main bus current trend telemetry'),(40,69,'Battery cell health and SOC diagnostic telemetry'),(21,69,'Solar port current telemetry'),(22,69,'Solar port voltage telemetry'),(58,67,'Mission energy reserve forecast result'),(59,67,'Predicted eclipse entry and duration'),(60,67,'Sunlight return battery recharge plan'),
(30,67,'CRITICAL trusted DC bus voltage control feedback'),
(30,67,'Bus voltage transducer calibration health (diagnostic only)'),(65,67,'Independent under voltage protection status'),(67,33,'Bus transient capture trigger'),(67,58,'Energy budget computation request'),(61,67,'Contingency power fault diagnostic status'),(33,69,'Transient waveform SCADA display only')]),
('Mission loads and eclipse scheduling',[
(57,67,'Scheduled mission task demand'),(67,57,'Approved mission task energy budget'),(58,57,'Eclipse energy reserve prediction'),(59,57,'Eclipse phase transition forecast'),(60,57,'Sunlight return charge opportunity'),(67,50,'Mission priority load schedule'),(52,57,'Attitude control required energy demand'),(53,57,'Mission computer required electrical demand'),(54,57,'Optical telemetry downlink power demand'),(55,57,'Science payload optional power demand'),(56,57,'Thermal heater required demand'),(57,50,'Priority shedding request'),(64,54,'Optical burst surge fault injection state'),(54,67,'Actual optical transient demand feedback'),(67,64,'Optical burst disturbance activation'),(57,69,'Mission load schedule visualization telemetry')]),
('EPS safety, controls and SCADA',[
(66,67,'EPS safety authorization and interlock permit'),(66,49,'Independent priority load safe-state command'),(65,66,'Hardwired independent undervoltage trip status'),(72,66,'Emergency power isolation actuator status'),(66,44,'Battery isolation interlock override'),(67,66,'Supervisor power system fault/status'),(67,69,'Authoritative mission energy objective telemetry'),(69,67,'Operator requested mission load schedule'),(66,69,'EPS safety fault latch and alarm'),(67,66,'Operator explicit safety reset request'),(40,66,'Battery thermal and current protection health'),(67,71,'Final orbital power qualification beacon')])]
assert [len(v) for _,v in G]==[10,20,22,22,18,18,16,12], [len(v) for _,v in G]
assert sum(map(lambda g:len(g[1]),G))==138
socks=list(csv.DictReader((R/'Visual_Terminal_Sockets_212.csv').open()))
by_model=defaultdict(list)
for s in socks:by_model[s['Model']].append(s['AnchorID'])
rot=defaultdict(int);rows=[]
for category,items in G:
 for a,b,purpose in items:
  a=int(a);b=int(b);src=M(a);dst=M(b)
  assert by_model[src] and by_model[dst],(src,dst,purpose)
  rot[src]+=1;rot[dst]+=1
  sa=by_model[src][(rot[src]-1)%len(by_model[src])];da=by_model[dst][(rot[dst]-1)%len(by_model[dst])]
  ix=len(rows)+1
  kind='POWER_CONTROL' if ix<=10 else 'ANALOG_FEEDBACK' if any(x in purpose.lower() for x in ('actual','measured','monitor','feedback','telemetry','sense','volt','current','temperature','soc','health','status','diagnostic','position','irradiance','ripple','sample','ready')) else 'DIGITAL_CONTROL'
  rows.append({'WireID':f'W{ix:03d}','Group':category,'SourceModel':src,'SourceVisualAnchor':sa,'TargetModel':dst,'TargetVisualAnchor':da,'SignalPurpose':purpose,'SignalKind':kind,'Critical':'YES' if ix==104 else 'NO','Status':'CONCEPT_ONLY_NO_PLAYER_WIRE'})
assert rows[103]['WireID']=='W104' and rows[103]['SourceModel']=='L83_BusVoltageTransducer' and rows[103]['TargetModel']=='L83_MainBusEnergyManagementController',rows[103]
with (R/'Gameplay_Wiring_Concept_W001_W138.csv').open('w',newline='') as f:
 w=csv.DictWriter(f,fieldnames=rows[0].keys());w.writeheader();w.writerows(rows)
with (R/'Gameplay_Wiring_Budget.csv').open('w',newline='') as f:
 w=csv.writer(f);w.writerow(['Group','FirstWire','LastWire','PlannedWires','Status']);a=1
 for cat,items in G:
  w.writerow([cat,f'W{a:03d}',f'W{a+len(items)-1:03d}',len(items),'DESIGN_ONLY']);a+=len(items)
(R/'Terminal_Anchor_Plan_212.csv').write_text((R/'Visual_Terminal_Sockets_212.csv').read_text())
objectives=[
('1','Build the Orbital Energy Network','138 actual player-created wires; 212 valid functional terminals; valid control power, converters, BMS, mission loads, independent protection and W104'),
('2','Capture and Regulate Solar Energy','Deployed and calibrated solar wings; actual solar incidence and PV IV measurements; independent MPPT controllers; bus precharge and stable DC regulation'),
('3','Qualify Battery Storage and Distribution','Cell group sense, protected pack current, BMS, balancing, charge/discharge, contactors, thermal control and priority load switching passed'),
('4','Survive the First Complete Eclipse','Fresh sunlight/eclipsed/sunlight cycle with physically conserved energy, essential bus stability, validated mission task deadlines and battery reserve'),
('5','Recover Three Power-System Disturbances','Gimbal jam, cell imbalance and optical burst demand each physically activate and are repaired; fresh orbital cycle passes all mission and safety metrics'),
('6','Restore Critical DC Bus Feedback','Real W104 physically disconnected, measurement stale, supervised power-safe state and latched fault; reconnect, calibrate, explicit reset, then new full orbital EPS certification')]
with (R/'Objective_Certification_Plan_6.csv').open('w',newline='') as f:
 w=csv.writer(f);w.writerow(['Objective','Title','AuthoritativeEvidence','Status'])
 for row in objectives:w.writerow([*row,'DESIGN_ONLY_NOT_IMPLEMENTED'])
w104=rows[103]
(R/'W104_Critical_Feedback_Recovery.md').write_text(f'''# Level 83 — W104 critical DC bus feedback loss and power-safe recovery

## Proposed real electrical endpoints
- **W104** (actual player-created wire in future game)
- Source: `{w104['SourceModel']}` terminal `MAIN_DC_BUS_VOLTAGE_OUT`, visual anchor `{w104['SourceVisualAnchor']}`
- Destination: `{w104['TargetModel']}` terminal `EPS_TRUSTED_BUS_VOLTAGE_IN`, visual anchor `{w104['TargetVisualAnchor']}`
- Signal: trusted, continuously sampled DC bus voltage with calibration, range validity and timestamp.

## Fault semantics
- Physically disconnect W104: update actual circuit topology; trusted bus feedback invalidates as samples age. The real bus continues to obey energy conservation and does not instantly drop to zero. Last-known voltage/SCADA telemetry, W105 diagnostic health, and the separate independent protector do **not** substitute for W104 closed-loop qualification.
- Power safety PLC withdraws normal high-power permits, sheds optional loads and commands protected fallback. Maintain essential power only if the available physical energy permits. Separate independent undervoltage protector remains active. Latch critical fault.
- Reconnect W104, reacquire fresh samples, verify calibration, independent protection, converter/current limits, and explicit operator reset.
- Run **fresh full sunlight → eclipse → sunlight** qualification, PV/MPPT, pack temperature/current/SOC, load priorities, bus stability and mission deadlines. Only then certify Objective 6 (actual LevelGamePlayManager 6/6). Objectives 1–5 remain historically certified.

## Scope
This package contains **art plus conceptual wiring** only: no actual Unity `SparkTerminal`, circuit solver bindings, EPS physics, playable fault state machine or objectives.
''')
(R/'AAA_Art_Delivery_Notes.md').write_text('''# Level 83: Beyond the Shadow — HD AAA-inspired original procedural geometry

74 individual FBX 7400 binary models, 74 matching GLBs, assembled FBX/GLB, 56 real named mesh-owning pivot parents, 212 modeled gold-rimmed socket visuals. Original modeled: independently framed solar wings with thousands of discrete PV cell meshes, solar hinges and gimbals, spacecraft electronics bus, orbital theater, 2 MPPT racks, 2 battery vaults with cell modules, DC bus and PDU, priority loads, solar/eclipse holograms and power-safe controls.

This is **AAA-inspired procedural art**; not a final studio-authored AAA mesh/UV/LOD/texture production set. The procedural textures are preview tileable PBR maps. Import models without assembling the separate showcase simultaneously. FBX numeric units centimeters, GLB meters, Y-up; nominal dimensions 26.2m wide and 9.29m tall. Very high mesh part count: use GPU instancing, static batching and LOD optimization.

Planned 138 wires and 212 socket anchors are **not** actual functional Unity electrical connections. No actual orbit, PV, MPPT, battery, converter, simulation or Unity Play Mode are implemented or tested.
''')
(R/'Gameplay_Integration_Plan.md').write_text('''# Project Spark Level 83 — Unity Gameplay Integration Plan

## Existing package
Original 74 modular FBX+GLB pairs + assembled scene, 56 real ANIM parent pivots, material sets, 212 gold-rimmed art socket meshes, Unity art-only Editor and visual rig helpers. `Model_Manifest.json` contains exact names and origin coordinates.

## Architectural requirements
1. Inspect existing SparkElectricalSolver, SparkCircuitSystem, SparkTerminal, SparkLevelDefinition and LevelGamePlayManager APIs. **Do not invent Project Spark APIs.**
2. Build exactly 138 actual player-created wires using `Gameplay_Wiring_Concept_W001_W138.csv`. Reconcile socket reuse: a single visual anchor can host multiple compatible logical connections only through legitimate electrical junctions or a clearly separated physical socket/port. No cross-domain shorts. Build 212 functional SparkTerminal interfaces compatible with the 138 electrical connection definitions.
3. Implement separate electrical control signal (5V), fictional spacecraft energy/power, and independent safety domains. No free energy. Real dynamic PV generation must depend on sunlight, incidence and panel/MPPT state; batteries must use power/time integrators, thermal/current limits and SOC boundaries; bus voltage and load shed reflect stored energy and converter dynamics.
4. Three eclipse-cycle phases, mission task deadlines, optical communication load schedule, gimbal jam, battery cell imbalance, transmitter sudden demand.
5. W104 real physical disconnect: trusted bus voltage stale, power-safe fallback, independent undervoltage protection, latch, reconnection, calibration, explicit reset, and **fresh complete orbital cycle**. Never bypass with display telemetry. Objectives 1–5 remain historically complete.
6. All six objectives certified through actual LevelGamePlayManager. Do not award PASS based on animations.

## Unity art assembly
- Import all 74 numbered FBXs with centimeter-to-meter conversion **once**. All modular model transforms identity at common origin under `L83_HD_VISUAL_ART`. Do not simultaneously instantiate assembled showcase.
- Run art placement, material upgrade, validator from `Tools/Project Spark/Levels/83/...`.
- Attach SparkLevel83VisualRig to common art root and call actual-state setter functions. It only renders; no physics/wiring/safety/certification.

## Fidelity limits
The orbital visuals and physics are educational game abstractions. Actual spacecraft flight qualification and battery safety are not claimed. Unity compile/Play Mode not tested.
''')
# Imported C# art placement and validator based on previously validated layout-specific tools.
old=Path('/mnt/data/ProjectSpark_Level82_HD_AAA/Unity_Optional')
for fn in ('SparkLevel82ArtPlacementEditor.cs','SparkLevel82AssetValidatorEditor.cs'):
 s=(old/fn).read_text().replace('Level82','Level83').replace('Level 82','Level 83').replace('L82_','L83_').replace('_L82_','_L83_').replace('Levels/82/','Levels/83/').replace('/82/','/83/').replace('>= 72','>= 74').replace('< 72','< 74').replace('i < 72','i < 74').replace('72;', '74;').replace('72\n','74\n').replace('/72','/74').replace('54\n','56\n').replace('/54','/56').replace('25.40','26.20').replace('25.4','26.2')
 (U/fn.replace('82','83')).write_text(s)
# Robust custom shader/material palette mapping; shader textures remain preview and do not claim baked UVs.
pal=json.loads((R/'Material_Palette.json').read_text());cs=[]
for name,p in pal.items():
 col=p['rgba_8bit'];cs.append('            {"%s", new Color(%.6ff, %.6ff, %.6ff, %.6ff)},'%(name,*[x/255 for x in col]))
source=r'''#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace ProjectSpark.EditorTools
{
 public static class SparkLevel83MaterialUpgradeEditor
 {
  const string Menu="Tools/Project Spark/Levels/83/Create & Apply HD Materials To Selected Art";
  const string Folder="Assets/ProjectSpark/Level83/Materials";
  static readonly Dictionary<string,Color> Palette = new Dictionary<string,Color>() {
__PALETTE__
  };
  static void EnsureFolders(){
    if(!AssetDatabase.IsValidFolder("Assets/ProjectSpark"))AssetDatabase.CreateFolder("Assets","ProjectSpark");
    if(!AssetDatabase.IsValidFolder("Assets/ProjectSpark/Level83"))AssetDatabase.CreateFolder("Assets/ProjectSpark","Level83");
    if(!AssetDatabase.IsValidFolder(Folder))AssetDatabase.CreateFolder("Assets/ProjectSpark/Level83","Materials");
  }
  static string MapSet(string key){
    if(key=="solar"||key=="solarblue"||key=="solarcell"||key=="cellglass") return "SolarCells";
    if(key=="gold"||key=="palegold"||key=="orbitgold"||key=="sun"||key=="amber")return "GoldContact";
    if(key=="copper") return "CopperBus";
    if(key=="battery"||key=="batteryblue"||key=="green")return "BatteryCells";
    if(key=="metallic"||key=="steel"||key=="silver"||key=="light"||key=="titanium")return "BrushedTitanium";
    if(key=="glass"||key=="frost"||key=="hologram"||key=="ice")return "OpticalGlass";
    if(key=="navy"||key=="space"||key=="shadow"||key=="powerrack")return "SpacecraftAlloy";
    if(key=="black"||key=="dark"||key=="rubber")return "BlackCeramic";
    if(key=="teal"||key=="circuit")return "ElectronicsPCB";
    if(key=="violet"||key=="purpleglass"||key=="orbitviolet")return "OrbitDisplay";
    if(key=="warm"||key=="orange"||key=="red"||key=="safety")return "SafetyCoating";
    return "InstrumentPanel";
  }
  static Material GetOrCreate(string key,Color col){
    string path=Folder+"/PS_"+key+".mat";
    Material m=AssetDatabase.LoadAssetAtPath<Material>(path);if(m!=null)return m;
    Shader s=Shader.Find("Universal Render Pipeline/Lit");
    if(s==null)s=Shader.Find("HDRP/Lit");if(s==null)s=Shader.Find("Standard");
    if(s==null)throw new InvalidOperationException("No compatible Lit shader");
    m=new Material(s);m.name="PS_"+key;
    if(m.HasProperty("_BaseColor"))m.SetColor("_BaseColor",col);
    if(m.HasProperty("_Color"))m.SetColor("_Color",col);
    bool metal=key=="silver"||key=="copper"||key=="gold"||key=="titanium"||key=="metallic"||key=="steel";
    if(m.HasProperty("_Metallic"))m.SetFloat("_Metallic",metal?0.85f:0.12f);
    if(m.HasProperty("_Smoothness"))m.SetFloat("_Smoothness",0.64f);
    string pre="Assets/ProjectSpark/Level83/Textures/"+MapSet(key);
    Texture2D colorMap=AssetDatabase.LoadAssetAtPath<Texture2D>(pre+"_Albedo.png");
    if(colorMap!=null){if(m.HasProperty("_BaseMap"))m.SetTexture("_BaseMap",colorMap);if(m.HasProperty("_MainTex"))m.SetTexture("_MainTex",colorMap);}
    string norm=pre+"_Normal.png";
    TextureImporter imp=AssetImporter.GetAtPath(norm) as TextureImporter;
    if(imp!=null && imp.textureType!=TextureImporterType.NormalMap){imp.textureType=TextureImporterType.NormalMap;imp.SaveAndReimport();}
    Texture2D n=AssetDatabase.LoadAssetAtPath<Texture2D>(norm);
    if(n!=null){if(m.HasProperty("_BumpMap"))m.SetTexture("_BumpMap",n);if(m.HasProperty("_NormalMap"))m.SetTexture("_NormalMap",n);m.EnableKeyword("_NORMALMAP");}
    if(col.a<0.99f){if(m.HasProperty("_Surface"))m.SetFloat("_Surface",1);if(m.HasProperty("_Mode"))m.SetFloat("_Mode",3);m.renderQueue=3000;}
    AssetDatabase.CreateAsset(m,path);return m;
  }
  [MenuItem(Menu)]public static void Apply(){
    GameObject root=Selection.activeGameObject;
    if(root==null){EditorUtility.DisplayDialog("Level 83 Materials","Select L83_HD_VISUAL_ART","OK");return;}
    EnsureFolders();var lookup=new Dictionary<string,Material>();
    foreach(var p in Palette)lookup[p.Key]=GetOrCreate(p.Key,p.Value);
    int changed=0;
    foreach(Renderer r in root.GetComponentsInChildren<Renderer>(true)){
      Material[] assigned=r.sharedMaterials;bool found=false;
      for(int i=0;i<assigned.Length;i++){
        if(assigned[i]==null)continue;
        string key=assigned[i].name;
        if(key.StartsWith("PS_",StringComparison.OrdinalIgnoreCase))key=key.Substring(3);
        int suffix=key.IndexOf(" (Instance)",StringComparison.Ordinal);
        if(suffix>=0)key=key.Substring(0,suffix);
        Material target;
        if(!lookup.TryGetValue(key.ToLowerInvariant(),out target))continue;
        assigned[i]=target;found=true;
      }
      if(found){Undo.RecordObject(r,"Level 83 Preview Material Assignment");r.sharedMaterials=assigned;changed++;}
    }
    AssetDatabase.SaveAssets();Debug.Log("[Level 83] Assigned preview materials to "+changed+" renderers. Packed roughness/metal/AO maps need shader-specific mapping.");
  }
  [MenuItem(Menu,true)]static bool ValidSelection()=>Selection.activeGameObject!=null;
 }
}
#endif
'''
(U/'SparkLevel83MaterialUpgradeEditor.cs').write_text(source.replace('__PALETTE__','\n'.join(cs)))
(R/'README_Unity.txt').write_text('''PROJECT SPARK — LEVEL 83: BEYOND THE SHADOW — UNITY ART INSTALL

1. Extract the complete archive and import all 74 numbered FBX models from Models into Assets/ProjectSpark/Level83/Models.
2. Import scale: FBX numeric coordinates are CENTIMETRES; the GLBs are METRES. Configure Unity to convert exactly once so workstation bounds are about 26.20 m wide x 9.29 m tall. One Unity unit = one metre.
3. Put the three *Editor.cs scripts in an Assets/**/Editor/ folder; put SparkLevel83VisualRig.cs in any runtime folder. Copy Textures/ to Assets/ProjectSpark/Level83/Textures.
4. Select an empty Level 83 layout root. Run Tools/Project Spark/Levels/83/Place HD Art Under Selected Layout. This creates L83_HD_VISUAL_ART with the 74 common-origin modular models.
5. Select L83_HD_VISUAL_ART and run Tools/Project Spark/Levels/83/Create & Apply HD Materials To Selected Art.
6. Run Tools/Project Spark/Levels/83/Validate Imported HD Art; confirm 74 models, 56 pivot parents and expected world dimensions.
7. Optional: use Level83_Complete_ArtKit_Showcase.fbx or .glb for an alternate static showcase, NOT alongside all modular models.
8. The visual rig is an art-only adapter. The 138 wiring CSV entries and 212 socket art anchors are design plans, not instantiated SparkTerminals or gameplay circuits. Build the EPS and power safety simulation using your actual Project Spark APIs.
9. W104 future trusted bus-voltage connection: L83_BusVoltageTransducer -> L83_MainBusEnergyManagementController. Do not bypass with SCADA or stale samples. See W104_Critical_Feedback_Recovery.md.
10. Preview PBR maps: R metallic, G roughness, B ambient occlusion. Map separately to your URP/HDRP shader channel expectations. Transparent regions and thousands of cell meshes require LOD and draw-call optimization.
11. No Unity compilation, Play Mode testing, battery safety testing or real spacecraft flight certification has been performed.
''')
print('Level83 SUPPORT READY',len(rows),'wires',len(socks),'anchors','Critical',w104,'Unity Editor 3 files')
