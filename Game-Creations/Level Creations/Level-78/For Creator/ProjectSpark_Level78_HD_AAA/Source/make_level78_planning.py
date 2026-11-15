#!/usr/bin/env python3
"""Create truthful Level 78 gameplay design aids from the actual exported Level 78 assets.
NO Unity gameplay, real SparkTerminals, electrical circuit, or physics are implemented.
"""
from pathlib import Path
import csv,json,math
import numpy as np
ROOT=Path('/mnt/data/ProjectSpark_Level78_HD_AAA')
MAN=json.loads((ROOT/'Model_Manifest.json').read_text())
assert MAN['individual_models']==64 and MAN['animation_parent_pivot_groups']==46
names=[m['name'] for m in MAN['models']]
rows=[]; budget=[]

def group(label,items,expected):
 assert len(items)==expected,(label,len(items),expected)
 start=len(rows)+1
 for s,t,purpose in items:
  assert 0<=s<64 and 0<=t<64
  rows.append({'WireID':f'W{len(rows)+1:03d}','Group':label,'SourceModel':names[s], 'TargetModel':names[t], 'SignalPurpose':purpose, 'Status':'CONCEPTUAL_ONLY_NOT_A_UNITY_CONNECTION'})
 budget.append((label,expected,f'W{start:03d}-W{len(rows):03d}'))

group('Educational 5V control power and returns',[
(63,61,'PLC 5V supply'),(63,62,'SCADA logic power'),(63,53,'Reliability controller logic supply'),(63,57,'Emergency isolation logic supply'),(63,23,'Cap MFC logic supply'),(63,36,'Low-k MFC logic supply'),(63,51,'Leakage electrometer interface power'),(63,60,'Pressure gauge power'),(63,45,'Cure temperature sensor power'),(63,56,'RC analyzer control supply')],10)

group('Vacuum wafer transfer and gas handling',[
(61,59,'Backing pump enable'),(61,58,'Turbo pump enable'),(59,58,'Backing pump readiness'),(58,60,'Vacuum achieved diagnostic'),(60,61,'Main chamber pressure feedback'),(61,6,'Vacuum gate valve command'),(6,61,'Valve closed interlock'),(61,5,'Load-lock pump down command'),(5,61,'Load-lock pressure-ready feedback'),(61,8,'Robot move command'),(8,61,'Robot home feedback'),(8,9,'Wafer alignment request'),(9,61,'Wafer registration feedback'),(61,11,'Copper surface preparation request'),(10,61,'Wafer surface condition status'),(11,61,'Precondition complete'),(60,57,'Vacuum safety permit'),(7,61,'Transfer hub enclosure-safe status')],18)

group('Protective cap plasma RF and deposition',[
(61,12,'Cap reactor enable'),(61,13,'Cap reactor lid command'),(13,61,'Lid closed interlock'),(61,23,'Cap flow request'),(23,61,'Actual cap gas flow feedback'),(21,22,'Virtual cap precursor available'),(22,23,'Cap manifold delivery status'),(61,15,'Cap RF plasma request'),(15,16,'Cap RF power to matching network status'),(16,61,'RF match valid feedback'),(61,18,'Cap susceptor temperature command'),(18,61,'Cap wafer temperature ready'),(24,61,'Measured cap film thickness'),(25,61,'Regional cap uniformity'),(26,61,'Cap continuity/pinhole inspection'),(61,19,'Cap exhaust throttle request')],16)

group('Low-k film deposition and precursor control',[
(61,28,'Low-k reactor enable'),(61,29,'Low-k showerhead distribution request'),(61,30,'Low-k RF plasma request'),(30,31,'Low-k matching network input state'),(31,61,'Low-k RF match status'),(61,32,'Low-k susceptor request'),(32,61,'Low-k wafer temperature valid'),(33,35,'Virtual matrix precursor availability'),(34,35,'Virtual porogen availability'),(61,35,'Low-k dual-channel flow command'),(35,36,'Mixed flow demand'),(36,61,'Actual low-k total gas flow'),(37,61,'Ion energy/damage diagnostic'),(38,61,'Surface charging feedback'),(39,61,'FTIR bonding state'),(40,61,'Low-k growth monitor feedback')],16)

group('UV cure and thermal regulation',[
(61,41,'UV cure chamber start'),(41,61,'Cure chamber door safe'),(61,42,'UV lamp bank enable'),(42,61,'Actual UV lamp output feedback'),(61,43,'UV shutter command'),(43,61,'Actual shutter position'),(61,44,'Cure platen rotation request'),(44,61,'Actual cure platen RPM'),(61,45,'Cure thermal target'),(45,61,'Actual cure wafer temperature'),(46,61,'Cure outgassing monitor feedback'),(47,61,'Pore network monitor status'),(61,41,'Cure completion acknowledgement'),(41,57,'UV enclosure-safe signal')],14)

group('Film and dielectric electrical metrology',[
(49,61,'Measured dielectric thickness and optical properties'),(48,61,'Wafer bow/stress measurement'),(55,61,'Surface moisture measurement'),(39,53,'Bonding-property measurement'),(56,53,'Interconnect RC delay measurement'),(50,53,'Interline comb fixture readiness'),(52,53,'Bias stress source status'),(53,52,'Reliability test source command'),
(51,53,'CRITICAL dielectric leakage-current electrometer feedback'), # W083 (75+8)
(51,61,'Leakage instrument health diagnostic'),(53,61,'Reliability test certification telemetry'),(54,62,'TDDB stress graph display data'),(49,62,'Dielectric film thickness telemetry'),(53,57,'Electrical-stress safety permit')],14)

group('Wafer integrity and reliability inspection',[
(10,61,'Incoming wafer surface acceptance'),(26,53,'Cap pinhole-risk inspection'),(25,53,'Cap coverage evidence'),(37,53,'Plasma damage proxy'),(38,53,'Wafer charging warning'),(40,53,'Low-k film growth certificate'),(45,53,'Cure temperature history'),(46,53,'Cure outgassing completion'),(48,53,'Wafer stress health'),(55,53,'Moisture health'),(56,53,'RC delay result'),(51,53,'Electrometer calibration health (not substitute for W083)'),(50,53,'Comb fixture isolation reference'),(53,61,'Wafer reliability quality status')],14)

group('PLC safety and SCADA coordination',[
(57,61,'Emergency isolation input'),(61,57,'Emergency safety heartbeat'),(57,15,'Cap RF safety disable'),(57,30,'Low-k RF safety disable'),(57,42,'UV lamp safety disable'),(57,52,'Bias stress source safety disable'),(57,61,'Latched safety fault status'),(61,62,'PLC machine state telemetry'),(62,61,'Operator command request'),(61,53,'Reliability sequence request'),(53,61,'Critical feedback validity status'),(53,57,'W083 feedback loss protection request'),(61,8,'Wafer transfer safety hold'),(61,62,'Objective progress report'),(53,62,'Test result SCADA telemetry'),(57,62,'Critical alarm telemetry')],16)
assert len(rows)==118
assert rows[82]['WireID']=='W083' and rows[82]['SourceModel']=='L78_DielectricLeakageCurrentElectrometer' and rows[82]['TargetModel']=='L78_DielectricReliabilityController'
with (ROOT/'Gameplay_Wiring_Concept_W001_W118.csv').open('w',newline='',encoding='utf8') as f:
 w=csv.DictWriter(f,fieldnames=list(rows[0]));w.writeheader();w.writerows(rows)
with (ROOT/'Gameplay_Wiring_Budget.csv').open('w',newline='',encoding='utf8') as f:
 w=csv.writer(f);w.writerow(['FunctionalGroup','PlannedPlayerWires','IDs']);w.writerows(budget)

anchors=[]
for m in MAN['models']:
 i=m['index'];n=3 if i<54 else 2
 lo=np.array(m['bbox_min']);hi=np.array(m['bbox_max']);cen=(lo+hi)/2
 for j in range(n):
  # proposed anchor near the visible front/side of each equipment's true geometry
  x=float(cen[0]+(j-1)*min(.34,(hi[0]-lo[0])*.12)); y=float(max(.92,min(4.3,cen[1]+(.20 if j==2 else -.12))))
  z=float(hi[2]+.11+j*.04)
  anchors.append({'PlannedAnchorID':f'P{len(anchors)+1:03d}','Model':m['name'],'WorldX_m':f'{x:.3f}','WorldY_m':f'{y:.3f}','WorldZ_m':f'{z:.3f}','Purpose':['Proposed I/O or shared return','Proposed control/measurement terminal','Proposed secondary function/junction'][j],'Status':'PROPOSED_ONLY_NO_SPARKTERMINAL_OBJECT'})
assert len(anchors)==182
with (ROOT/'Terminal_Anchor_Plan_182.csv').open('w',newline='',encoding='utf8') as f:
 w=csv.DictWriter(f,fieldnames=list(anchors[0]));w.writeheader();w.writerows(anchors)
objectives=[
('1','Build dielectric protection network','118 real player wires connected, 182 unique registered SparkTerminals, valid 5V control, returns and safety','None','1/6'),
('2','Commission vacuum and cap chamber','Wafer inspected, vacuum dynamically achieved, pumps/robots/interlocks/precursor and cap sensors calibrated','Objective 1','2/6'),
('3','Seal the copper','Actual cap PECVD, 16-region coverage >=99.8%, pinholes <=0.2%, diffusion protection and stress accepted','Objective 2','3/6'),
('4','Build and qualify low-k insulation','Actual low-k PECVD, plasma charge control, UV cure, thickness, low-k/RC, moisture and valid monitor-coupon reliability screen','Objective 3','4/6'),
('5','Survive dielectric disturbances','Cap gas flow asymmetry + plasma charging + UV curing faults detected, safely recovered, fresh wafer certified','Objective 4','5/6'),
('6','Restore critical dielectric leakage feedback','Physically disconnect W083, invalidate electrometer, safely inhibit/discharge stress fixture, reconnect/calibrate/reset, qualify new wafer and coupon','Historical Objectives 1-5','6/6')]
with (ROOT/'Objective_Certification_Plan_6.csv').open('w',newline='',encoding='utf8') as f:
 w=csv.writer(f);w.writerow(['Objective','Title','RealCompletionEvidence','Prerequisites','AuthoritativeProgress']);w.writerows(objectives)
(ROOT/'W083_Critical_Feedback_Recovery.md').write_text('''# Level 78 — W083 critical electrometer feedback fault

W083 is a **conceptual** player-connection plan, not an existing Unity wire. Exact pair:

- Source: `L78_DielectricLeakageCurrentElectrometer`
- Destination: `L78_DielectricReliabilityController`
- Signal: `CRITICAL dielectric leakage-current electrometer feedback`

The eventual Project Spark implementation MUST create actual compatible SparkTerminal endpoints with a physically removable W083 wire. On removal the circuit graph must change, the leakage-current measurement becomes INVALID and prior samples STALE. The test source must be inhibited by safety control and the test fixture's stored energy discharged through a modeled safe path. The source and physical electric field must NOT vanish instantaneously merely because the sensor is disconnected. Log and latch the fault; prevent a fake leakage PASS.

After player reconnection require fresh samples, electrometer zero/calibration and explicit reset. Then require a **new** fully processed protective cap and low-k wafer, UV cure, wafer metrology and valid monitor-coupon leakage/reliability screening. Preserve Objectives 1–5 historically certified. Only real SparkLevelEvaluator/LevelGamePlayManager should award 6/6.

Do not treat W084 (electrometer health) or any SCADA status as a substitute for the missing W083 analog feedback path. Electrical testing should use fictional low-energy protected instrumentation, without real hazardous high-voltage procedures.
''',encoding='utf8')
(ROOT/'Gameplay_Integration_Plan.md').write_text('''# PROJECT SPARK — LEVEL 78: SEAL THE SIGNAL — UNITY GAMEPLAY INTEGRATION PLAN

## Verified art package vs. missing gameplay

64 original numbered modular binary FBXs and 64 matching GLBs, one assembled FBX + GLB, 46 named visual animation parent pivots, procedural colored mesh geometry, art installation helpers and metadata. These are genuine 3D models, NOT finished hand-authored AAA PBR. Their hierarchy has NO working SparkTerminals, electrical wiring, process simulation, PLC, safe stress-test source, real leakage feedback or authoritative objectives. Unity has not been compiled or Play Mode tested here.

## Import and art placement

Import the 64 numbered FBXs, confirm approximately 21.2-m width (FBX numeric cm, GLB metres). Select the Level 78 layout and run Tools/Project Spark/Levels/78/Place HD Art Under Selected Layout, then material upgrade and asset validation. Do not duplicate the assembled showcase.

## Required actual Project Spark Unity implementation

1. Inspect real SparkElectricalSolver, SparkCircuitSystem, SparkTerminal, SparkLevelEvaluator, SparkLevelDefinition, SparkLevelSceneBindings, LevelGamePlayManager and existing levels; never invent core API signatures.
2. Create 118 real player-created wires W001–W118 and 182 functional registered terminals. Included CSVs are conceptual plans ONLY. Keep 5V educational controls separate from fictional RF and reliability-test power. Do not turn material or vacuum piping into conducting player wires.
3. Implement chamber pressure/pump dynamics, gas flow, virtual precursor inventory, thermal inertia, RF matching, plasma damage/charge, film thickness/coverage/pinhole/diffusion risk, low-k film/porosity/RC, UV curing/outgassing/moisture, film stress, ellipsometry, wafer-handling, guarded low-energy coupon bias stress, valid leakage measurement and estimated reliability score. Use 16 wafer regions and deterministic numerical updates.
4. Implement three physical disturbances: cap showerhead flow asymmetry; low-k plasma charging; UV cure lamp-zone degradation. Each genuinely changes process state, triggers protection/diagnosis and requires new certified wafer.
5. Reserve W083 exactly as L78_DielectricLeakageCurrentElectrometer -> L78_DielectricReliabilityController. On physical wire disconnect invalidate leakage measurement, require safe bias-test source shutoff and fixture discharge, latch protection. On reconnect require fresh samples, calibration, explicit reset, new fully qualified wafer AND monitor-coupon result. Do not use cached readings or other status wires as substitutes.
6. Create exactly six authoritative objectives with real LevelGamePlayManager progress 0/6..6/6; preserve historic Objectives 1–5 during final deliberate fault.
7. Implement Editor Tools/Project Spark/Levels/Create Level 78 idempotently and without breaking previous levels; use existing art helpers separately. All visual rig SetActual* inputs must be driven by real simulation, not directly from UI or fake timers.

## Fictional education quality thresholds

Cap coverage >=99.8%, cap defects <=0.2%, low-k film thickness variation <=3%, modeled dielectric constant <=3.0, moisture index <=1%, copper diffusion-risk <=0.5 normalized, leakage <=0.10 normalized, stress within configured range. These are NOT industrial process settings. The modeled TDDB score is NOT a real lifetime guarantee.
''',encoding='utf8')
(ROOT/'AAA_Art_Delivery_Notes.md').write_text('''# Level 78 Art Delivery — scope and limitations

The original equipment mesh geometry consists of 64 distinct modular binary FBXs with matching GLBs, shared common-origin geometry and 46 actual named pivot parent hierarchies. Separate assembled FBX and GLB are included only for inspection. Detail includes machined vacuum flanges, bolts, showerhead arrays, RF/cabinet details, chamber layers, UV emitters, ceramic hardware, wafer test coupons, virtual dielectric/copper cross-section visualizations, and instrumentation. All part materials are procedural color groups; glass translucency depends on Unity material settings.

This **HD/AAA-inspired procedural art foundation is NOT artist-authored production AAA**: no hand-painted calibrated PBR textures/UV atlases, LODs, lightmaps, skinning, baked motion clips or runtime physics. Meshes are triangle-based and should be profiled/optimized for production as needed. Verify importer scaling and actual vertex counts in Unity.

No ready-to-use functioning electrical wires, registered SparkTerminals, simulated PECVD/UV physics, PLC safety, six objectives or final stress-test feedback fault are included. Reference W083 and all gameplay CSVs are conceptual integration guides. Unity compilation/Play Mode not tested.
''',encoding='utf8')
(ROOT/'README_Unity.txt').write_text('''PROJECT SPARK — LEVEL 78: SEAL THE SIGNAL
MODULAR FBX+GLB DIELECTRIC CAP / LOW-K PECVD / UV CURE / RELIABILITY ART KIT

CONTENTS: 64 numbered binary FBXs; 64 matching GLBs; 1 assembled reference FBX+GLB; 46 actual named animation parent pivots; manifest and planning CSVs; 4 optional Unity C# scripts; original procedural model source; verification report.

QUICK INSTALL
1. Extract ZIP and import 64 numbered FBX files from Models/ into a Unity Assets folder.
2. Check combined width ~21.2 metres when one Unity unit = one metre. FBX vertex values are numeric centimetres (FBX UnitScaleFactor=1); GLB numeric vertices are metres. Apply importer conversion once only.
3. Put SparkLevel78ArtPlacementEditor.cs, SparkLevel78MaterialUpgradeEditor.cs and SparkLevel78AssetValidatorEditor.cs into a Unity Editor folder. Put SparkLevel78VisualRig.cs into a normal runtime scripts folder.
4. Select Level 78 layout root and run Tools/Project Spark/Levels/78/Place HD Art Under Selected Layout. Creates L78_HD_VISUAL_ART with 64 origin-aligned numbered FBX prefab instances.
5. Select art root, use Tools/Project Spark/Levels/78/Create & Apply HD Materials To Selected Art. Material colors are previews; inspect transparent/emissive appearance in your render pipeline.
6. Run Tools/Project Spark/Levels/78/Validate Imported HD Art. Verify 64 imported models, ~21.2 metres, 46 named pivot transforms.
7. DO NOT also instantiate Models/Level78_Complete_ArtKit_Showcase.fbx; it duplicates all numbered models.

SCOPE: 118 planned player wires and 182 proposed terminal anchor locations are guides, NOT functional gameplay. W083 conceptual critical feedback wire, PECVD, plasma, curing, leakage reliability, PLC protection and six objectives need actual Unity implementation. Visual rig drives only geometry and optional VFX. Unity compile and Play Mode not tested. No baked animation/PBR texture atlases/LOD rigs.
''',encoding='utf8')
print('Planning ready',len(rows),'wires',len(anchors),'anchors','6 objectives',flush=True)
