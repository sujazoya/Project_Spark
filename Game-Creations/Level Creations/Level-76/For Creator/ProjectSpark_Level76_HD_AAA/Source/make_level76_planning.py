#!/usr/bin/env python3
"""Create grounded L76 art manifest, wiring design/terminal anchor spreadsheets and installation notes.
No gameplay SparkTerminals or real wires are created by this script.
"""
from pathlib import Path
import csv,json,collections,math,random
import numpy as np,trimesh
R=Path('/mnt/data/ProjectSpark_Level76_HD_AAA')
models=sorted([p for p in (R/'Models').glob('*.fbx') if p.name[:2].isdigit()])
assert len(models)==60
rigs=list(csv.DictReader((R/'Animation_Rigs.csv').open()))
assert len(rigs)==42,len(rigs)
records=[]
for i,p in enumerate(models):
 assert p.name.startswith(f'{i:02d}_L76_')
 q=R/'GLB'/p.with_suffix('.glb').name
 sc=trimesh.load(q,force='scene',process=False)
 tris=sum(len(m.faces) for m in sc.geometry.values()); parts=len(sc.geometry)
 b=np.asarray(sc.bounds)
 assert b.shape==(2,3) and np.isfinite(b).all()
 piv=[x for x in rigs if x['Model']==('L76_'+p.stem.split('_L76_')[1])]
 records.append(dict(index=i,name='L76_'+p.stem.split('_L76_')[1],file=p.name,glb=q.name,
 triangle_count=tris,mesh_parts=parts,bbox_min=np.round(b[0],4).tolist(),bbox_max=np.round(b[1],4).tolist(),
 rig_groups=[x['Pivot'] for x in piv],animation_pivots={x['Pivot']:[float(x[k]) for k in ('X','Y','Z')] for x in piv}))
manifest={
 'level_id':'level_76','display_name':'Forge the Connections',
 'subtitle':'Intelligent Copper Interconnect Electroplating',
 'asset_kind':'Original procedural HD/AAA-inspired 3D mesh art; not final artist-authored production PBR',
 'individual_models':60,'individual_glbs':60,
 'assembled_models':['Models/Level76_Complete_ArtKit_Showcase.fbx','GLB/Level76_Complete_ArtKit_Showcase.glb'],
 'individual_triangle_count':sum(x['triangle_count'] for x in records),
 'individual_mesh_parts':sum(x['mesh_parts'] for x in records),
 'animation_parent_pivot_groups':len(rigs),
 'planned_player_wires':110,'planned_unique_functional_terminals':170,
 'actual_player_wires_included':False,'functional_spark_terminals_included':False,
 'electroplating_physics_included':False,'six_authoritative_objectives_implemented':False,
 'unity_compilation_tested':False,'unity_play_mode_tested':False,
 'fbx_units':'numeric centimeters, UnitScaleFactor=1; importer must produce metre-scale geometry',
 'glb_units':'numeric metres','axes':'X width, Y up, Z front',
 'approx_workstation_width_m':19.6,'common_origin_for_all_individual_models':True,
 'rig_hierarchy':'Named animated parent Null nodes inside binary FBX and GLB; no baked animation clips',
 'educational_scope':'Fictional nonhazardous plating fluid; normalized educational process controls, not an industrial recipe',
 'models':records}
(R/'Model_Manifest.json').write_text(json.dumps(manifest,indent=2),encoding='utf-8')

budget=[
 ('Educational 5 V control power and returns',10,'PLC, sensors, reference supply, shared return'),
 ('Rectifier, anode and cathode power control',16,'plating rectifier, segmented anode, wafer contact, current distribution'),
 ('Electrolyte pumping, flow and filtration',16,'recirculation pump, filter, manifold, flow sensors, leakage'),
 ('Bath temperature and fictional additive delivery',14,'thermal controller, conductivity, bath concentration, additive metering'),
 ('Plating chamber and wafer handling',14,'chuck, spindle, lift, robot, cassette, prewetting and degassing'),
 ('Current, potential and metrology sensors',14,'critical W076 reference electrode, current, voltage, copper fill, void and resistance'),
 ('Safety, interlocks and fault protection',12,'E-stop, leak relay, overcurrent, potential feedback and bath safe'),
 ('PLC, waveform controller and SCADA',14,'PLC process sequencer, waveform controller, SCADA states and events')]
assert sum(b[1] for b in budget)==110
with (R/'Gameplay_Wiring_Budget.csv').open('w',newline='') as f:
 w=csv.writer(f);w.writerow(['Group','PlannedPlayerWires','Purpose','Status']);w.writerows([(*x,'PLANNING_ONLY_NO_WIRES') for x in budget])

# Meaningful concept connections. These are equipment-to-equipment planning references, not terminal IDs.
byid={i:v['name'] for i,v in enumerate(records)}
group_edges=[
 [(58,54,'5V PLC supply'),(58,55,'5V I/O supply'),(58,56,'5V waveform controller'),(58,57,'5V SCADA supply'),(58,30,'5V potential controller'),(58,27,'5V transducer supply'),(58,29,'5V reference input'),(58,18,'5V flow controller'),(58,52,'5V safety leak relay'),(58,53,'5V E-stop logic')],
 [(56,25,'rectifier current command'),(25,21,'segmented anode power'),(25,7,'wafer cathode power'),(26,27,'current shunt measurement'),(27,56,'current feedback'),(28,56,'cell voltage feedback'),(31,21,'anode zone A command'),(31,21,'anode zone B command'),(31,24,'edge-ring control'),(42,7,'cathode contact impedance'),(56,31,'current distribution request'),(25,54,'power module ready'),(54,25,'plating enable'),(25,54,'overcurrent status'),(7,56,'cathode contact valid'),(31,56,'zone current diagnostic')],
 [(54,17,'recirculation enable'),(17,18,'recirculation pump flow'),(18,54,'flow measured'),(18,19,'flow distribution command'),(19,20,'manifold delivery'),(20,54,'nozzle bank validity'),(16,54,'filtration state'),(54,16,'filter pump enable'),(14,17,'reservoir feed'),(17,14,'return flow'),(13,54,'degassing ready'),(54,13,'degassing enable'),(41,54,'line pressure'),(40,54,'bubble measurement'),(52,54,'electrolyte leak state'),(51,54,'waste reservoir status')],
 [(33,54,'bath temperature measurement'),(54,34,'bath thermal command'),(34,54,'thermal output state'),(32,54,'bath conductivity'),(35,54,'virtual concentration measured'),(54,39,'additive dosing command'),(36,39,'suppressor available'),(37,39,'accelerator available'),(38,39,'leveler available'),(39,54,'additive delivery feedback'),(39,14,'additive delivery to reservoir'),(54,15,'mixer enable'),(15,54,'mixer status'),(34,33,'temperature response')],
 [(54,10,'wafer robot enable'),(10,54,'wafer robot ready'),(11,10,'wafer cassette available'),(10,7,'wafer loading operation'),(54,9,'cathode lift command'),(9,54,'lift actual position'),(54,8,'spindle RPM request'),(8,54,'spindle actual RPM'),(54,12,'wafer prewet command'),(12,54,'prewet complete'),(13,12,'bubble removal ready'),(59,54,'barrier seed continuity'),(54,2,'plating chamber close'),(2,54,'chamber enclosure closed')],
 [(27,30,'actual plating current'),(28,30,'full cell voltage'),(44,54,'copper thickness result'),(30,56,'control-loop feedback valid'),(43,54,'four-point interconnect resistance'),(29,30,'reference electrode potential'),(45,54,'void inspection result'),(46,54,'trench fill tomography'),(47,54,'copper overburden result'),(48,54,'copper roughness result'),(42,54,'wafer contact integrity'),(59,54,'seed continuity preflight'),(46,57,'trench holographic display'),(29,54,'reference electrode validity')],
 [(53,54,'E-stop input'),(52,54,'leak detection input'),(54,25,'rectifier safety disable'),(54,8,'spindle interlock'),(54,9,'lift interlock'),(54,17,'pump stop interlock'),(30,54,'potential safety inhibit'),(27,54,'overcurrent safety input'),(41,54,'overpressure safety input'),(33,54,'overtemperature safety input'),(54,53,'safety annunciator'),(54,57,'latched fault state')],
 [(55,54,'analog input sample'),(54,55,'analog I/O enable'),(56,54,'waveform phase feedback'),(54,56,'waveform start request'),(57,54,'operator command'),(54,57,'PLC status telemetry'),(30,57,'potential feedback telemetry'),(27,57,'current feedback telemetry'),(18,57,'electrolyte flow telemetry'),(46,57,'trench fill map'),(45,57,'void map'),(54,57,'objective state event'),(56,57,'plating waveform telemetry'),(54,57,'certification result')]
]
assert [len(g) for g in group_edges]==[b[1] for b in budget]
with (R/'Gameplay_Wiring_Concept_W001_W110.csv').open('w',newline='') as f:
 w=csv.writer(f);w.writerow(['WireID','Group','SourceModel','TargetModel','SignalPurpose','Status'])
 n=0
 for bi,group in enumerate(group_edges):
  for src,dst,sig in group:
   n+=1;w.writerow([f'W{n:03d}',budget[bi][0],byid[src],byid[dst],sig,'CONCEPT_ONLY_NO_SPARK_WIRE_OR_ENDPOINT'])
assert n==110
# The target concept fault W076 is position in group 6: first five groups =70, then 6th item, which is index 76.
wires=list(csv.DictReader((R/'Gameplay_Wiring_Concept_W001_W110.csv').open()))
assert wires[75]['WireID']=='W076' and wires[75]['SourceModel']==byid[29] and wires[75]['TargetModel']==byid[30],wires[75]

# Proposed terminal anchor locations derived from the individual FBX model bounds via actual GLB counterpart.
# Anchor layout deliberately spreads around display face of each equipment; these are not Unity gameplay terminals.
weights=[2 if i in (0,1,3,4,5,6,23,46,57) else 3 for i in range(60)]
while sum(weights)>170:
 weights[weights.index(3)]=2
while sum(weights)<170:
 ii=next(i for i in range(60) if weights[i]==2 and i not in (0,1,3,4))
 weights[ii]+=1
assert sum(weights)==170
with (R/'Terminal_Anchor_Plan_170.csv').open('w',newline='') as f:
 w=csv.writer(f);w.writerow(['PlannedAnchorID','Model','WorldX_m','WorldY_m','WorldZ_m','Purpose','Status'])
 t=0
 for i,(model,num) in enumerate(zip(records,weights)):
  lo=np.asarray(model['bbox_min']);hi=np.asarray(model['bbox_max'])
  center=(lo+hi)/2
  # All mounts deliberately planned near the accessible front side of module bounds.
  for j in range(num):
   t+=1
   x=lo[0]+(j+1)*(hi[0]-lo[0])/(num+1)
   y=lo[1]+(.42 if j%2 else .62)*(hi[1]-lo[1])
   z=hi[2]+.08
   w.writerow([f'P{t:03d}',model['name'],round(float(x),4),round(float(y),4),round(float(z),4),'Proposed front-facing functional socket; refine collider and access after FBX import','PROPOSED_ONLY_NO_SPARKTERMINAL_OBJECT'])
assert t==170

(R/'Gameplay_Integration_Plan.md').write_text('''# Project Spark — Level 76: FORGE THE CONNECTIONS\n\n## Scope and status\n\nThis is original modular 3D visual art **only**. Geometry is not a Unity gameplay implementation. The existing files provide 60 original binary FBX modular meshes, 60 GLB backups, assembled workstation FBX/GLB, named pivots, equipment naming, educational wiring/terminal plans, and optional Unity art helpers. No Unity Play Mode test was performed.\n\n## Existing asset hierarchy\n\n- Art root `L76_HD_VISUAL_ART`.\n- 60 numbered FBXs share one origin. Keep each at position `(0,0,0)` rotation zero scale one after importer conversion.\n- Approximate workstation width 19.6 metres. FBX coordinates are numeric centimeters with `UnitScaleFactor=1`; GLB coordinates are numeric metres.\n- Do not place the assembled reference FBX alongside the 60 modular files.\n- Named pivots from `Animation_Rigs.csv` are actual FBX/GLB parent nodes, no baked animations.\n\n## Critical Unity systems still required\n\n1. Inspect existing Project Spark Unity circuit, wiring, shelf, level and objective APIs.\n2. Create exactly **170 functional `SparkTerminal` objects** and **110 player wires** using the planning CSVs. Respect real junction/shared-terminal rules. No playable terminals or wires are inside the art files.\n3. Model a fictional nonhazardous electrolyte, reservoir inventory, flow, filtration, temperature, prewetting and bubbles.\n4. Implement cell rectifier with distinct actual current, cell voltage and measured cathode potential; include finite response and fail-safe shutdown.\n5. Implement Faraday-law inspired copper deposition and true regional trench/via fill geometry: seed continuity, bottom and sidewall growth, voids, seams, overburden and electrical measurement.\n6. Implement current-density distribution with finite multi-zone anode controller; fictional accelerator/suppressor/leveler balance.\n7. Implement wafer cleaning, spin drying, thickness/void/topography, and pre-CMP electrical test-structure metrology.\n8. Implement equipment disturbances: wafer contact degradation; manifold flow restriction; additive dosing drift.\n9. Implement the critical **W076** real reference-electrode feedback wire. On disconnection, invalidate potential sensing, inhibit plating, safely disable the rectifier through simulated actuator dynamics, latch protection and require real wire reconnection, recalibration, explicit reset, and a fresh certified wafer.\n10. Integrate the existing Project Spark objective system. Objectives 1–5 stay historically certified during the final intentional disconnection; objective 6 only completes after full repair and successful new wafer; `LevelGamePlayManager` must reach 6/6.\n\n## Planned objectives\n\n1. Build electrochemical network — 110 player connections / 170 functional terminals.\n2. Prepare electrolyte and wafer — circulation, temperature, prewetting, seed continuity, clamp and bubble checks.\n3. Master current and electrochemical control — calibrate current / potential / anode current distribution.\n4. Achieve void-free copper filling — regional deposition, fill, overburden, cleaning and metrology.\n5. Survive three disturbances — contact resistance, flow manifold, virtual additives.\n6. Restore critical reference-electrode feedback — W076 real wire loss, safe disable, reconnection, recalibration, fresh wafer certification.\n\n## Important manufacturing continuity\n\nThe prepared incoming wafer is assumed to have a patterned dielectric, barrier and seed layer after intervening process steps. This level does not claim to plate bare implanted silicon directly. The electroplated overburden remains ready for a later copper CMP level.\n\n## Safety and verification limitations\n\nAll plating electrolyte and additives are fictional and nonhazardous, with normalized educational settings; this art pack is not an operational chemical recipe. Source art does not implement circuit physics, plating chemistry, safety PLC, objectives, LODs, baked clips, final PBR textures, or tested Unity gameplay.\n''',encoding='utf-8')
(R/'README_Unity.txt').write_text('''PROJECT SPARK — LEVEL 76: FORGE THE CONNECTIONS\nHD/AAA-inspired original procedural 3D asset kit — 60 FBX + 60 matching GLBs + assembled FBX/GLB.\n\nQUICK IMPORT\n1. Extract this package and import all 60 numbered FBX files under an Assets folder.\n2. In Unity, verify the entire workstation is approximately 19.6 Unity metres wide (1 Unity unit = 1 metre). FBX numeric geometry is in centimetres with UnitScaleFactor=1; GLBs contain metres. Apply the correct importer conversion exactly once.\n3. Place `SparkLevel76ArtPlacementEditor.cs`, `SparkLevel76MaterialUpgradeEditor.cs`, `SparkLevel76AssetValidatorEditor.cs` in a Unity `Editor` folder. Place `SparkLevel76VisualRig.cs` in a regular runtime scripts folder.\n4. Select the Level 76 layout GameObject and run Tools/Project Spark/Levels/76/Place HD Art Under Selected Layout. This creates L76_HD_VISUAL_ART containing all 60 common-origin models.\n5. Run Tools/Project Spark/Levels/76/Create & Apply HD Materials To Selected Art for preview materials (select L76_HD_VISUAL_ART).\n6. Run Tools/Project Spark/Levels/76/Validate Imported HD Art to inspect model counts and bounding size.\n7. The assembled reference FBX/GLB is for viewing only; do NOT instantiate it in addition to the modular models.\n\nGAMEPLAY NOT YET IMPLEMENTED\nThe 110-connection plan and 170-anchor plan are DESIGN ONLY. They are not registered player wires or real SparkTerminal components. W076 is the proposed critical reference-electrode feedback. Six objectives, electroplating physics, sensors, safety and real player wiring need actual Unity project implementation.\n\nFiles: Model_Manifest.json, Animation_Rigs.csv, Art_Assembly.csv, Component_Roles.csv, Gameplay_Wiring_Budget.csv, Gameplay_Wiring_Concept_W001_W110.csv, Terminal_Anchor_Plan_170.csv, Gameplay_Integration_Plan.md, Material_Palette.json, Verification_Report.json, optional C# helpers, Source/*.py.\n\nNo claim of finished artist-authored PBR, baked animation clips, or Unity Play Mode certification.\n''',encoding='utf-8')
print('PLANNED: models',len(records),'tris',manifest['individual_triangle_count'],'mesh_parts',manifest['individual_mesh_parts'],'pivots',len(rigs),'wires',n,'anchors',t,flush=True)
