#!/usr/bin/env python3
"""Level 79 conceptual electrical/terminal plans and scope documents (NOT runtime gameplay)."""
import csv,json
from pathlib import Path
R=Path('/mnt/data/ProjectSpark_Level79_HD_AAA')
m=json.loads((R/'Model_Manifest.json').read_text())
names=[x['name'] for x in m['models']]
def S(x):return names[x]
# Each triple: source FBX index, destination index, conceptual electrical signal purpose.
G=[
('Educational 5V control power and returns',[
(64,60,'PLC 5 V supply'),(64,61,'Analog IO power'),(64,63,'SCADA logic supply'),(64,62,'Motion safety logic supply'),(64,18,'Fine alignment controller supply'),(64,30,'Interferometer conditioning electronics supply'),(64,27,'Camera A power'),(64,28,'Camera B power'),(64,26,'Force sensing interface power'),(64,52,'Kelvin meter interface power')]),
('Gantry hexapod and robotic motion',[
(60,14,'Gantry overall motion enable'),(60,15,'X rail servo command'),(15,61,'X travel position feedback'),(60,16,'Y crossbeam command'),(16,61,'Y travel position feedback'),(60,17,'Z vertical servo command'),(17,61,'Z travel position feedback'),(60,18,'Hexapod pose command'),(18,61,'Hexapod actuators ready'),(18,19,'Nano X command'),(19,61,'Nano X actual state'),(18,20,'Nano Y command'),(20,61,'Nano Y actual state'),(18,21,'Theta correction command'),(21,61,'Theta position feedback'),(18,22,'Tip tilt command'),(22,61,'Tip tilt position feedback'),(61,60,'Precision pose interlock satisfied')]),
('Optical interferometric alignment',[
(27,61,'Fiducial camera A overlay measurement'),(28,61,'Fiducial camera B overlay measurement'),(29,61,'Through silicon IR registration measurement'),(31,61,'Moire overlay diagnostic'),(32,61,'Wafer bow measurement'),(27,18,'Camera A alignment residual'),(28,18,'Camera B alignment residual'),(61,18,'Camera agreement validity'),
(30,18,'CRITICAL fine-position laser interferometer feedback'),(30,63,'Laser position telemetry to SCADA'),(60,30,'Laser measurement enable'),(30,62,'Laser signal health to safety'),
(30,61,'Laser interferometer calibration health'), # W041
(18,60,'Closed loop pose qualified'),(61,31,'Overlay projection display control'),(29,60,'Infrared registration certificate'),(32,60,'Warpage compensation authorization'),(61,62,'Optical positioning safety validity')]),
('Contact force and bonding mechanics',[
(60,23,'Upper die chuck grip command'),(23,24,'Chuck release condition monitoring'),(24,61,'Chuck release ready feedback'),(60,25,'Base wafer chuck command'),(25,61,'Lower chuck holding confirmation'),(60,26,'Microforce acquisition enable'),(26,61,'Measured contact force array'),(61,39,'Contact initiation permit'),(39,61,'Contact actuator position feedback'),(42,61,'Interface gap monitor'),(40,61,'Bond wave optical signal'),(61,60,'Bond-front monitoring certificate'),(60,39,'Contact initiation request'),(62,39,'Safety retract override'),(39,62,'Approach state safe feedback'),(61,23,'Chuck residual force calibration')]),
('Surface preparation and cleanliness',[
(10,61,'Known good die certificate result'),(33,61,'Darkfield particle count'),(34,61,'Surface flatness metrology'),(35,61,'Surface energy inspection result'),(60,36,'Dry surface cleaning enable'),(36,61,'Cleaning actuator completion'),(60,37,'Surface activation enable'),(37,61,'Activation process completion'),(59,62,'ESD guarding status'),(58,60,'Reject quarantine occupancy'),(60,58,'Reject bin accept command'),(13,61,'Base wafer alignment ready'),(61,60,'Surface readiness certificate'),(11,61,'Loadlock condition safe')]),
('Thermal consolidation and stress',[
(60,43,'Consolidation chamber enable'),(60,44,'Bond heater setpoint command'),(44,61,'Actual multi zone heater state'),(45,61,'Measured bond temperature'),(46,61,'Differential expansion telemetry'),(47,61,'Bond interface stress measurement'),(60,45,'Temperature sampling request'),(61,60,'Thermal profile qualification'),(60,56,'Thermal cycle screening command'),(56,61,'Thermal cycle completion status'),(57,61,'Stack warpage scan'),(43,62,'Thermal enclosure interlock'),(61,44,'Heater safe disable'),(47,60,'Stress threshold certification')]),
('Bond inspection and electrical tests',[
(60,48,'Acoustic scan request'),(48,61,'Acoustic void map'),(60,49,'Infrared tomography request'),(49,61,'IR void map'),(50,61,'Bond seam inspection'),(51,61,'Witness coupon adhesion result'),(52,61,'Cross die Kelvin resistance data'),(53,61,'Vertical daisy chain continuity'),(54,61,'Chiplet routing topology result'),(55,61,'High speed BER jitter test result'),(60,55,'System link test enable'),(61,60,'Integrated electrical qualification'),(61,63,'Bond health inspection telemetry'),(61,63,'Chiplet link test telemetry')]),
('PLC motion safety and SCADA',[
(62,60,'Emergency stop input'),(62,14,'Emergency gantry inhibit'),(62,18,'Precision hexapod hold'),(62,23,'Chuck release safety inhibit'),(62,39,'Bond approach safety inhibit'),(61,62,'Motion axis status'),(61,60,'Precise alignment status'),(60,63,'PLC operational state'),(63,60,'SCADA operator command'),(60,63,'Six axis pose telemetry'),(61,63,'Overlay quality map'),(61,63,'Void and bond map'),(61,63,'Inter chiplet contact graph'),(61,63,'Six objective progress'),(60,62,'Motion reset request'),(62,60,'Critical latch status'),(60,63,'W037 feedback loss alarm'),(60,63,'Final four chip certification')]),
]
assert [len(rows) for _,rows in G]==[10,18,18,16,14,14,14,18]
rows=[]
for group,entries in G:
  for a,b,purpose in entries:
    rows.append({'WireID':f'W{len(rows)+1:03d}','Group':group,'SourceModel':S(a),'TargetModel':S(b),'SignalPurpose':purpose,'Status':'CONCEPTUAL_ONLY_NO_SPARKWIRE_IN_SCENE'})
assert len(rows)==122
assert rows[36]['WireID']=='W037' and rows[36]['SourceModel']==S(30) and rows[36]['TargetModel']==S(18),rows[36]
with (R/'Gameplay_Wiring_Concept_W001_W122.csv').open('w',newline='',encoding='utf8') as f:
    w=csv.DictWriter(f,fieldnames=list(rows[0]));w.writeheader();w.writerows(rows)
with (R/'Gameplay_Wiring_Budget.csv').open('w',newline='',encoding='utf8') as f:
    w=csv.writer(f);w.writerow(['Group','PlannedPlayerWires','Start','End','Status'])
    start=1
    for g,ent in G:
        w.writerow([g,len(ent),f'W{start:03d}',f'W{start+len(ent)-1:03d}','Conceptual; requires real electrical mapping']);start+=len(ent)
# 2 anchors per module, plus 56 additional instruments / control anchors => exactly 188 unique proposed positions
anchors=[]
for i,model in enumerate(m['models']):
    lo=model['bbox_min'];hi=model['bbox_max']
    cx=(lo[0]+hi[0])/2;cz=(lo[2]+hi[2])/2
    # Front side sockets with a deliberate horizontal offset for player access.
    for j in range(2+(1 if 10<=i<=65 else 0)):
        if j==0:x=lo[0]+.22*(hi[0]-lo[0]);y=max(.9,lo[1]+.37*(hi[1]-lo[1]));z=hi[2]+.10
        elif j==1:x=hi[0]-.22*(hi[0]-lo[0]);y=max(.9,lo[1]+.37*(hi[1]-lo[1]));z=hi[2]+.10
        else:x=cx;y=max(.9,lo[1]+.62*(hi[1]-lo[1]));z=hi[2]+.10
        anchors.append({'PlannedAnchorID':f'P{len(anchors)+1:03d}','Model':model['name'],'WorldX_m':round(x,4),'WorldY_m':round(y,4),'WorldZ_m':round(z,4),'Purpose':f'Proposed terminal mounting point {j+1}; assign electrical domain from actual Unity signal mapping','Status':'PROPOSED_ONLY_NO_SPARKTERMINAL_OBJECT'})
assert len(anchors)==188
with (R/'Terminal_Anchor_Plan_188.csv').open('w',newline='',encoding='utf8') as f:
    w=csv.DictWriter(f,fieldnames=list(anchors[0]));w.writeheader();w.writerows(anchors)
objs=[
('1','Build Precision Bonding Network','122 actual wires connected and 188 functional SparkTerminals registered; qualified control, motion, vision, safety and test systems'),
('2','Qualify Surfaces and Six Axis Alignment','Known-good chiplets, bond-ready wafer, surface/particle scans, camera and interferometer alignment calibrated'),
('3','Bond Logic and Memory','Real six-axis placement, safe bond-front initiation, acceptable interface coverage, acoustic/IR void inspection for first two chiplets'),
('4','Complete and Test Four Chiplets','I/O and Sensor bonds accepted, thermal consolidation passed, all contacts and inter-chip test paths qualified'),
('5','Survive Assembly Disturbances','Camera calibration drift, chuck-release anomaly and particle contamination repaired; fresh complete 4-chiplet assembly certified'),
('6','Restore Critical Laser Feedback','Physically disconnect W037, detect stale precision feedback and latch motion hold; reconnect/rehome/recalibrate/reset; build and certify fresh four-chiplet system')]
with (R/'Objective_Certification_Plan_6.csv').open('w',newline='',encoding='utf8') as f:
    w=csv.writer(f);w.writerow(['Objective','Title','AuthoritativeEvidence','Status'])
    for number,title,body in objs:w.writerow([number,title,body,'DESIGN_NOT_IMPLEMENTED'])
(R/'W037_Critical_Feedback_Recovery.md').write_text('''# Level 79 — W037 critical laser interferometer feedback recovery

**Exact conceptual entry:** W037, `L79_LaserDisplacementInterferometer` → `L79_HexapodFineAlignmentStage`, `CRITICAL fine-position laser interferometer feedback`.

**Not already a physical Unity wire.** During implementation define two actual compatible `SparkTerminal`s, e.g. `LASER_INTERFEROMETER_POSITION_OUT` → `HEXAPOD_FINE_POSITION_IN`, and ensure a real `SparkCircuitSystem` wire-graph mutation occurs on player disconnection.

**Safe fault transition:** topology changes → current sample path invalid → previous samples stale → precision approach inhibited → fine axes commanded controlled hold → no unverified contact initiation → fault latched. Independent cameras help safely diagnose/withdraw but do not bypass missing precision feedback. Already bonded chiplets retain state, not erased by the fault.

**Recovery:** reconnect actual W037; fresh valid samples; laser position feedback recalibration; six-axis homing; independent camera/interferometer agreement; explicit operator safety reset; new prepared wafer and FOUR fresh known-good chiplets; all interfaces bonded and inspected; thermal consolidation; cross-die Kelvin/daisy-chain/routing/BER tests; authoritative `LevelGamePlayManager` 6/6. Objectives 1–5 remain historically certified. Reconnection alone cannot complete Objective 6.

**Safety:** educational abstract robotics; no real microfabrication automation scripts or hazardous procedure parameters. Testing of functional faults requires real Unity project source and Play Mode.
''',encoding='utf8')
(R/'Gameplay_Integration_Plan.md').write_text('''# Level 79 — Unity Gameplay Integration Plan

**Scope:** true modular 3D ART ONLY; 66 FBXs/GLBs and 48 pivot parents. W001–W122 and P001–P188 files are planning guides, NOT functional Spark terminals, wires, physics, or gameplay. Unity compile/Play Mode is not tested.

## Real Project Spark integration
Inspect `SparkCircuitSystem`, `SparkTerminal`, `SparkElectricalComponent`, `SparkLevelDefinition`, `SparkLevelEvaluator`, `SparkLevelSceneBindings`, `SparkLevelWorkstationBindingBridge` and `LevelGamePlayManager` before generating gameplay. Keep electrical control power at 5 V and separate simulated industrial motor and bonded-chiplet test domains. Physical contact, robot motion, thermal integration, optical registration, material/particle state, bond front, inspection and contact graph must have authoritative models.

## Geometry and animation
Import the 66 numbered FBXs. Check combined width ~22.45 m. FBX numeric vertices are cm and FBX UnitScaleFactor=1; GLB numeric vertices are metres. Place under `L79_HD_VISUAL_ART` with common-origin 0,0,0 transforms and importer scale conversion once. Use `Animation_Rigs.csv` for actual pivot parent names, axis and 3D coordinates. Optional `SparkLevel79VisualRig` is ART ONLY; do not derive objective truth from it. The 48 named movable pivot groups are real mesh-parent nodes in both formats; no baked animation clips included.

## Six distinct processes
(1) real electrical/player wiring, (2) known-good die and clean bond-ready interface plus six-axis calibration, (3) Logic and Memory hybrid contact front and void inspection, (4) I/O+Sensor bonding, thermal consolidation and four-chip communication graph, (5) three independent assembly disturbances and fresh certification, (6) deliberate W037 feedback wire disconnect, motion safe hold, reconnect/rehome/recalibrate/reset and a fresh four-chiplet assembly to 6/6.

## Physical truth vs simulated metrology
Use six-axis pose and per-pad registration, surface readiness and particle locations, contact-front propagation and voids, thermomechanical stress, copper pad connection resistance, cross-die signal routing. Test conditions are educational normalized approximations rather than real chiplet manufacturing specs. Chiplets cannot be simply re-positioned after committed bonding; bad bonds require quarantine/replacement according to scenario rules.

## Objective history
Historical Objectives 1–5 persist while user deliberately faults W037. Reconnection alone never grants Objective 6. No simulated sensor data may bypass actual missing W037 signal.
''',encoding='utf8')
(R/'AAA_Art_Delivery_Notes.md').write_text('''# Project Spark Level 79 — AAA-Inspired Art Delivery

Unique change of concept: vertically dominant six-axis die-to-wafer bonding tower rather than horizontal process reactors. Four **distinct** fabricated chiplets, multi-axis gantry and hexapod, alignment cameras and laser interferometer, contact-front hologram, acoustic and IR buried-void inspection, thermal consolidation, electrical test and chiplet reject handling.

Actual procedural meshes with split moving parts, embedded material color regions, named pivots, common-origin coordinate system, binary FBX 7400 and hierarchical GLB. Geometry is not final artist-authored photorealistic baked PBR, LOD, UV texture atlas, or finished animation clips. Preview materials are upgradeable in Unity URP/HDRP and visible GLB vertex colors are included.

The copper pad patterns are visually representative and do not encode the functional circuit graph. W037 fault and all 122 proposed player wires still require actual Unity electrical implementation. No Unity compile or Play Mode run was performed.
''',encoding='utf8')
(R/'README_Unity.txt').write_text('''PROJECT SPARK — LEVEL 79: BOND THE WORLDS
66 modular binary FBX and matching GLB models; one combined FBX and GLB showcase; 48 real named animation pivots.

INSTALL
1. Import all 66 numbered FBXs from Models/ into Unity Assets.
2. Verify combined equipment width approximately 22.45 metres with Y-up. FBX numeric units are cm (FBX UnitScaleFactor=1). GLB numeric units are metres. Convert FBX exactly once in Unity importer.
3. Put SparkLevel79ArtPlacementEditor.cs, SparkLevel79MaterialUpgradeEditor.cs, SparkLevel79AssetValidatorEditor.cs under a Unity Editor folder.
4. Put SparkLevel79VisualRig.cs under a normal runtime scripts folder.
5. Select Level 79 root and run Tools/Project Spark/Levels/79/Place HD Art Under Selected Layout. Creates L79_HD_VISUAL_ART and 66 origin-aligned modular model instances.
6. Apply preview materials via Tools/Project Spark/Levels/79/Create & Apply HD Materials To Selected Art.
7. Validate with Tools/Project Spark/Levels/79/Validate Imported HD Art (66 FBX, 48 pivots, ~22.45m).
8. Do not also instantiate combined `Level79_Complete_ArtKit_Showcase.fbx`; it duplicates the entire kit.

SCOPE: 122 wire descriptions and 188 proposed anchors are planning files. They are not functioning player wires, `SparkTerminal` components or playable objectives. W037 is a proposed critical laser feedback wire requiring real circuit integration. Optional visual rig is art-only. Unity compile and Play Mode have NOT been tested.
''',encoding='utf8')
print('PLANNING OK',len(rows),'wires',len(anchors),'anchors',len(objs),'objectives')
