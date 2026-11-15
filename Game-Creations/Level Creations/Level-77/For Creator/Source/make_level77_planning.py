#!/usr/bin/env python3
"""Create truthful Level 77 art metadata, 114 conceptual electrical endpoints, and 176 anchor proposals.
Does NOT create or register Unity SparkTerminals, physics, PLC logic, or playable objectives.
"""
from pathlib import Path
import csv,json,math
import numpy as np,trimesh
R=Path('/mnt/data/ProjectSpark_Level77_HD_AAA')
FB=R/'Models'; GB=R/'GLB'
files=sorted(p for p in FB.glob('*.fbx') if p.name[:2].isdigit())
assert len(files)==62
pivots=list(csv.DictReader((R/'Animation_Rigs.csv').open(encoding='utf-8',newline='')))
assert len(pivots)==44
models=[]
for i,p in enumerate(files):
 assert p.name.startswith(f'{i:02d}_L77_'),p.name
 glb=GB/(p.stem+'.glb'); assert glb.is_file()
 sc=trimesh.load(glb,force='scene',process=False)
 bb=np.asarray(sc.bounds);assert bb.shape==(2,3) and np.isfinite(bb).all()
 name='L77_'+p.stem.split('_L77_')[1]
 groups=[r for r in pivots if r['Model']==name]
 models.append(dict(index=i,name=name,file=p.name,glb=glb.name,
     triangle_count=sum(len(m.faces) for m in sc.geometry.values()),
     mesh_parts=len(sc.geometry),bbox_min=np.round(bb[0],5).tolist(),bbox_max=np.round(bb[1],5).tolist(),
     rig_groups=[r['Pivot'] for r in groups],
     animation_pivots={r['Pivot']:[float(r[k]) for k in ('X','Y','Z')] for r in groups}))
manifest=dict(level_id='level_77',display_name='Reveal the Circuit',subtitle='Advanced Copper Damascene Planarization and Electrical Isolation',
 asset_kind='Original procedural HD/AAA-inspired modular meshes, preview material IDs; NOT final artist-authored PBR',
 individual_models=62,individual_glbs=62,
 assembled_models=['Models/Level77_Complete_ArtKit_Showcase.fbx','GLB/Level77_Complete_ArtKit_Showcase.glb'],
 individual_triangle_count=sum(m['triangle_count'] for m in models),
 individual_mesh_parts=sum(m['mesh_parts'] for m in models),
 animation_parent_pivot_groups=len(pivots),
 planned_player_wires=114, planned_unique_functional_terminals=176,
 actual_player_wires_included=False,functional_spark_terminals_included=False,
 selective_cmp_physics_included=False,six_authoritative_objectives_implemented=False,
 unity_compilation_tested=False,unity_play_mode_tested=False,
 common_origin_for_all_individual_models=True,
 fbx_units='numeric centimeters, FBX UnitScaleFactor=1; verify FBX importer produces metres',
 glb_units='numeric metres',axes='X width, Y up, Z front',approx_workstation_width_m=20.4,
 rig_hierarchy='44 actual named animation Null parent nodes, separate FBX and GLB hierarchies, no baked animation clips',
 educational_scope='Fictional normalized CMP chemistry/process; no industrial polishing recipes',models=models)
(R/'Model_Manifest.json').write_text(json.dumps(manifest,indent=2),encoding='utf-8')
by={i:x['name'] for i,x in enumerate(models)}
budget=[
 ('Educational 5 V control power and returns',10,'Control-domain distribution, analog sensors, PLC and safety'),
 ('Three-stage motion and downforce',18,'Platen motors, carrier spin, lift, torque and downforce'),
 ('Process-fluid pumps, flow and cleaning',18,'Slurry delivery, rinse, cleaning and wafer transfer'),
 ('Copper thickness and endpoint sensing',16,'Eddy current, optical, material transition; critical W055'),
 ('Barrier selectivity and process-fluid condition',14,'Selective removal and barrier-control instruments'),
 ('Corrosion protection and cleaning feedback',12,'Wafer protection, residue, cleaning, drying'),
 ('Electrical isolation and wafer metrology',12,'Continuity, Kelvin probe, leakage, defects, residue'),
 ('PLC, safety and SCADA coordination',14,'PLC controls, critical safety inhibit and operator telemetry')]
assert sum(n for _,n,_ in budget)==114
with (R/'Gameplay_Wiring_Budget.csv').open('w',newline='',encoding='utf-8') as f:
 w=csv.writer(f);w.writerow(['Group','PlannedPlayerWires','Purpose','Status'])
 for title,n,desc in budget:w.writerow([title,n,desc,'PLANNING_ONLY_NO_WIRES'])
# Explicit source/target equipment numbers with gameplay SIGNAL purposes.
# Non-electrical fluid/mechanical relationships must not be implemented as fake conductive paths.
groups=[
 [(61,58,'5V PLC power'),(61,57,'5V CMP process controller power'),(61,60,'5V SCADA power'),(61,24,'5V bulk endpoint controller power'),(61,13,'5V bulk force sensor interface power'),(61,17,'5V flow instrument power'),(61,23,'5V eddy current interface power'),(61,38,'5V barrier spectrometer electronics power'),(61,59,'5V emergency-stop logic supply'),(61,54,'5V interline leakage test interface power')],
 [(58,10,'bulk servo RPM request'),(10,58,'bulk servo actual RPM'),(58,11,'bulk carrier spin request'),(11,58,'bulk carrier measured RPM'),(58,12,'bulk carrier lift request'),(12,58,'bulk lift position'),(13,57,'bulk downforce actual feedback'),(57,14,'multi-zone force command'),(14,57,'multi-zone force status'),(58,26,'soft platen speed command'),(26,58,'soft platen speed feedback'),(58,28,'soft carrier spin request'),(29,57,'soft landing contact force feedback'),(58,31,'barrier platen speed command'),(31,58,'barrier platen actual RPM'),(58,33,'barrier carrier motion request'),(33,58,'barrier carrier position feedback'),(42,58,'motor torque load feedback')],
 [(58,16,'bulk slurry pump enable'),(16,58,'bulk slurry pump status'),(17,57,'bulk slurry measured flow'),(58,18,'bulk slurry dispensing arm command'),(19,58,'bulk nozzle status'),(58,20,'bulk pad conditioner enable'),(20,58,'pad conditioner ready'),(58,35,'barrier fluid pump enable'),(35,58,'barrier pump actual drive feedback'),(36,57,'barrier fluid measured flow'),(37,58,'barrier dispense nozzle valid'),(58,30,'soft landing fluid manifold request'),(30,58,'soft landing flow response'),(58,46,'brush cleaner enable'),(46,58,'brush cleaner actual RPM'),(58,47,'megasonic cleaning enable'),(49,58,'rinse supply status'),(50,58,'spin dryer actual RPM')],
 [(23,24,'eddy-current copper thickness'),(22,24,'bulk optical copper clearing'),(13,57,'bulk downforce feedback for endpoint'),(21,57,'bulk pad wear measurement'),(23,57,'wafer conductive thickness telemetry'),(42,57,'platen torque process response'),(43,57,'wafer interface thermal feedback'),(40,57,'barrier clearance tomography status'),
  (38,57,'CRITICAL barrier optical endpoint spectral feedback'),
  (39,38,'optical viewport health validity'),(38,60,'barrier spectral telemetry'),(52,57,'dishing and erosion measurement'),(53,57,'residual conductive copper inspection'),(24,57,'bulk endpoint stage transition'),(38,58,'barrier optical feedback validity safety'),(57,33,'safe barrier carrier retract request')],
 [(57,41,'material selectivity analysis request'),(41,57,'selectivity measurement'),(34,57,'barrier process fluid readiness'),(57,35,'barrier fluid feed command'),(36,41,'actual barrier flow for selectivity'),(30,41,'soft landing fluid state'),(41,58,'process selectivity alarm'),(40,41,'barrier clearance map for selectivity'),(23,41,'copper thickness comparison'),(43,41,'thermal selectivity measurement'),(41,60,'selectivity trend telemetry'),(57,30,'soft landing finishing mode'),(41,57,'copper-barrier-dielectric removal ratio'),(57,31,'barrier stage enable permission')],
 [(44,57,'copper corrosion potential'),(57,45,'surface protection enable'),(45,57,'surface protection state'),(48,57,'residue condition measurement'),(57,46,'brush cleaning job'),(46,57,'brush cleaning completion'),(57,47,'megasonic cleaning job'),(47,57,'megasonic cleaning completion'),(49,57,'rinse delivery availability'),(50,57,'spin drying completion'),(51,57,'final surface defect count'),(57,60,'wafer cleaning and corrosion telemetry')],
 [(54,57,'interline leakage measurement'),(55,57,'via Kelvin continuity measurement'),(56,57,'dielectric integrity test result'),(51,57,'post CMP surface defects'),(52,57,'dishing erosion metrology map'),(53,57,'conductive copper residue map'),(4,57,'incoming plated wafer preinspection'),(55,60,'via resistance telemetry'),(54,60,'adjacent line isolation telemetry'),(56,60,'dielectric quality telemetry'),(57,60,'final geometry/electrical wafer certification'),(58,55,'electrical metrology calibration request')],
 [(59,58,'emergency stop safety input'),(58,10,'bulk polishing motor safety disable'),(58,26,'soft landing motor safety disable'),(58,31,'barrier polishing motor safety disable'),(58,12,'bulk carrier retract safety'),(58,29,'soft carrier retract safety'),(58,33,'barrier carrier retract safety'),(38,58,'critical optical sensing health'),(57,58,'CMP process stage command'),(58,57,'PLC safe state telemetry'),(60,58,'operator command'),(58,60,'PLC state telemetry'),(57,60,'objective state event'),(58,60,'wafer certification result')]
]
assert [len(g) for g in groups]==[b[1] for b in budget],list(map(len,groups))
with (R/'Gameplay_Wiring_Concept_W001_W114.csv').open('w',newline='',encoding='utf-8') as f:
 w=csv.writer(f);w.writerow(['WireID','Group','SourceModel','TargetModel','SignalPurpose','Status'])
 n=0
 for gi,group in enumerate(groups):
  for src,dst,signal in group:
   n+=1;w.writerow([f'W{n:03d}',budget[gi][0],by[src],by[dst],signal,'CONCEPT_ONLY_NO_SPARK_WIRE_OR_ENDPOINT'])
assert n==114
wires=list(csv.DictReader((R/'Gameplay_Wiring_Concept_W001_W114.csv').open()))
assert wires[54]['WireID']=='W055' and wires[54]['SourceModel']==by[38] and wires[54]['TargetModel']==by[57]
# 176 unique *planned positions*. 62*2=124, distribute 52 more based on functional equipment.
weights=[2]*62
priority=[58,57,38,24,23,22,13,14,17,36,30,41,42,43,44,46,47,48,49,50,51,52,53,54,55,56,59,61,
 10,11,12,16,18,20,21,25,26,28,29,31,33,34,35,37,39,40,45,4,5,6,7,60,8,9]
for idx in priority[:52]:weights[idx]+=1
assert sum(weights)==176
with (R/'Terminal_Anchor_Plan_176.csv').open('w',newline='',encoding='utf-8') as f:
 w=csv.writer(f);w.writerow(['PlannedAnchorID','Model','WorldX_m','WorldY_m','WorldZ_m','Purpose','Status'])
 t=0
 for rec,n in zip(models,weights):
  lo=np.array(rec['bbox_min']);hi=np.array(rec['bbox_max']);dif=hi-lo
  for j in range(n):
   t+=1
   x=lo[0]+(j+1)*dif[0]/(n+1)
   y=lo[1]+(.44 if j%2 else .65)*dif[1]
   z=hi[2]+.095
   # references only: choose these based on each model's own mesh bounding box
   w.writerow([f'P{t:03d}',rec['name'],round(float(x),4),round(float(y),4),round(float(z),4),
       'Proposed accessible front-side electrical socket; assign real SparkTerminal type and reconcile wire endpoint usage','PROPOSED_ONLY_NO_SPARKTERMINAL_OBJECT'])
assert t==176
(R/'Gameplay_Integration_Plan.md').write_text('''# PROJECT SPARK — LEVEL 77: REVEAL THE CIRCUIT

## Source of truth and scope

This pack contains **real procedural 3D mesh art** only: 62 individual binary FBX files, 62 corresponding GLB backups, and assembled showroom versions in both formats. 44 named pivot parent groups are embedded in the real binary FBX and GLB hierarchies. No Level 77 circuit, CMP physics, playable PLC, `SparkTerminal`, player wires, or actual objectives are included.

## Critical machine systems to build in the Unity project

1. **Inspect existing Project Spark code** before writing against `SparkElectricalSolver`, `SparkCircuitSystem`, `SparkElectricalComponent`, `SparkTerminal`, `SparkLevelDefinition`, `SparkLevelEvaluator`, `LevelGamePlayManager`, shelf and layout loader APIs.
2. Import 62 numbered FBXs as one common-origin modular installation. Use `L77_HD_VISUAL_ART` as visual root. FBX numeric units are centimetres, UnitScaleFactor=1; GLB numeric units are metres. Check combined width approximately 20.4 m. Do not duplicate the assembled showcase.
3. Create **114 real player-created circuit wires**, indexed W001–W114, and **176 registered functional terminal GameObjects**. The included CSVs are design guides only. Shared terminals, signal and safety domains must be engineered using real circuit API; non-electrical process-fluid relationships must not be turned into conductive gameplay wires.
4. Build three independently controlled CMP stations: A bulk copper, B soft landing, C selective barrier clear. Model actual motor RPM, downforce, contact and pad states, slurry/finishing-fluid flow, wafer transfer, measurement validity and safety.
5. Build a wafer with 16 regional material maps and multiple representative copper trenches/vias; material fields track copper, barrier/liner, dielectric, copper overburden, copper dishing, dielectric erosion, residual metal bridges, corrosion and local defects. Use physically inspired, bounded material-selective rates. No instant perfect wafer.
6. Implement real sensor-derived endpoints: eddy-current copper thickness, optical bulk clearance, dual-wavelength barrier endpoint, motor torque, optical viewport health; measurement timestamps, staleness, calibration and invalidation.
7. Implement post-CMP brush/megasonic/rinse/dry processes, corrosion protection and electrical metrology: via Kelvin continuity, interline leakage isolation, dielectric integrity. Unpolished blanket copper from Level 76 must be removed from dielectric field regions; preserve filled vias and lines.
8. Industrial disturbances: optical viewport fouling; selectivity drift; post-CMP corrosion. Each changes actual process behavior and requires repair/recalibration and a new certified wafer.
9. Final real player wire **W055**: `L77_DualWavelengthBarrierEndpointSpectrometer` → `L77_ThreeStageCMPProcessController`, conceptual signal `CRITICAL barrier optical endpoint spectral feedback`. On physical wire removal, invalidate optical measurement (not just its indicator), enter safe stop, decelerate platens, release carrier pressure, lift/retract wafer, latch fault, require wire reconnection, fresh optical calibration, explicit reset and a *new* fully measured quality wafer.
10. Integrate six authoritative objectives through real `SparkLevelEvaluator` / `LevelGamePlayManager`: (1) 114 wires/176 terminals, (2) three CMP stations commissioning, (3) bulk copper removal + soft landing, (4) selective barrier clear + isolation metrology, (5) three disturbances recovery + new wafer, (6) real W055 fault recovery + new wafer. Objectives 1–5 remain historically certified during deliberate W055 disconnection, final progress **6/6**.

## Design quality targets (fictional teaching scenario)

- Residual blanket copper below the calibrated clearance threshold.
- Exposed-field barrier residue below the calibrated threshold.
- Copper dishing ≤5 nm, dielectric erosion ≤4 nm; wafer nonuniformity ≤2.5%.
- Continuity passes for required vias; interline leakage/isolation passes across adjacent lines.
- Surface corrosion and particles within configured limits.

## AAA visual art status

The included geometry is original procedural HD/AAA-inspired modular art with material-region colors. It is **not** final artist-authored production PBR, baked animations, LODs, UV/texture atlas optimization, a functioning chemical recipe or a Unity-tested level. The visual rig is an optional scene adapter that should only consume authoritative simulation state. Preview colors and glass alpha require pipeline-specific inspection.
''',encoding='utf-8')
(R/'README_Unity.txt').write_text('''PROJECT SPARK — LEVEL 77: REVEAL THE CIRCUIT
Advanced triple-stage copper CMP and interconnect isolation — modular FBX/GLB ART PACK

CONTENTS: 62 numbered binary FBXs; 62 corresponding GLBs; assembled reference FBX+GLB; 44 real animation parent pivots; manifests, CSV integration design, 4 optional Unity scripts, original procedural source, verification report.

UNITY QUICK IMPORT
1. Extract ZIP to a working folder. Import all 62 numbered files from Models/ into your Unity Assets folder.
2. Validate the assembled equipment is ~20.4 Unity metres wide when 1 Unity unit = 1 m. Binary FBX vertex coordinates are numeric centimetres (UnitScaleFactor=1); GLB vertices use numeric metres. Configure the FBX importer conversion exactly once, do NOT multiply by 0.01 again if already converted.
3. Place SparkLevel77ArtPlacementEditor.cs, SparkLevel77MaterialUpgradeEditor.cs and SparkLevel77AssetValidatorEditor.cs in a Unity Editor folder. Place SparkLevel77VisualRig.cs in a runtime scripts folder.
4. Select your Level 77 layout GameObject. Use Tools/Project Spark/Levels/77/Place HD Art Under Selected Layout to instantiate L77_HD_VISUAL_ART (62 individually origin-aligned FBXs).
5. Select the art root, run Tools/Project Spark/Levels/77/Create & Apply HD Materials To Selected Art. These are preview materials only; inspect URP/HDRP/Built-in transparency and emission yourself.
6. Run Tools/Project Spark/Levels/77/Validate Imported HD Art. Check 62 models, ~20.4 m combined width, 44 named pivots.
7. DO NOT place Models/Level77_Complete_ArtKit_Showcase.fbx in the same scene with all numbered FBXs; it duplicates the entire machine.

SCOPE LIMITS
- Gameplay_Wiring_Concept_W001_W114.csv and Terminal_Anchor_Plan_176.csv are planning-only; they are NOT actual circuit wires or SparkTerminal objects.
- W055 is reserved as the critical optical barrier-endpoint feedback wire; must be implemented through real circuit topology, safe stop and recalibration logic.
- Copper/barrier/dielectric material removal, dishing, erosion, cleaning, corrosion, isolation metrology, PLC safety, disturbances and six authoritative objectives are NOT included.
- Unity compilation and Play Mode have NOT been tested. The models use procedural geometry and simple material palette regions, not final production hand-authored PBR, LODs, optimized atlases or baked animation clips.

See Model_Manifest.json, Animation_Rigs.csv, Component_Roles.csv, Art_Assembly.csv, Material_Palette.json, Gameplay_Integration_Plan.md, Source/*.py and Verification_Report.json.
''',encoding='utf-8')
print(f'PLANNED models {len(models)} triangles {manifest["individual_triangle_count"]} parts {manifest["individual_mesh_parts"]} pivots {len(pivots)} conceptual wires {len(wires)} anchors {t}',flush=True)
