#!/usr/bin/env python3
"""Level 90 readable production notes and process concept, grounded in exported model + wire plans."""
from pathlib import Path
from build_level90 import NAMES,P,PIVOTS,WIRES,ANCHORS,LOGICAL,CONFLICTS,core
import json,csv
ROOT=Path(__file__).resolve().parent.parent
def put(name,s):(ROOT/name).write_text(s.strip()+'\n',encoding='utf8')
materials=[]
for k,c in core.COLORS.items():
 materials.append(dict(name=k,base_rgba=c,visual_role=('emissive_preview' if k in ('cyan','plasma','blue','violet','amber','green','red','orange') else 'physical_material_preview'),UV_Baked=False))
(ROOT/'Material_Palette.json').write_text(json.dumps(dict(art_source='original_proc_generated',level='90',materials=materials,production_roughness_maps_are_reference_only=True),indent=2))
put('README_Unity.txt',f'''PROJECT SPARK — LEVEL 90 — THE LONG BURN
ORIGINAL EDITABLE MODULAR 3D ART ASSET DELIVERY

WHAT YOU HAVE
- 94 individual binary FBX 7400 files in Models/, 94 matching glTF GLB 2.0 in GLB/.
- One assembled showcase FBX and GLB for reference ONLY; do not instantiate alongside all 94 modular files.
- 80 named pivot-parent groups ANIM_*, 254 decorative gold visual socket markers.
- 12 procedural preview material sets / 36 PNG reference maps.
- Authored 176 conceptual wire definitions (W001..W176), 352 proposed independent logical ports, six objective design records.
- Optional Unity C# art placer, material palette upgrade, structure validator, and read-only visual rig.

IMPORT
1. Extract the ZIP, copy Models/ to Assets/ProjectSpark/Level90/Models/.
2. Copy optional Unity_Optional/*Editor.cs to a Unity Editor folder (e.g. Assets/ProjectSpark/Level90/Editor/).
3. Put SparkLevel90VisualRig.cs in a runtime scripts directory outside Editor/.
4. Select an existing/new Level 90 layout GameObject.
5. Tools/Project Spark/Levels/90/Place HD Art Under Selected Layout.
6. Tools/Project Spark/Levels/90/Create & Apply HD Materials To Selected Art.
7. Tools/Project Spark/Levels/90/Validate Imported HD Art.

SCALING IMPORTANT
FBX numeric vertices and animation pivot positions are in CENTIMETRES with FBX UnitScaleFactor=1;
GLB numeric geometry and node positions are in METRES. Ensure Unity's FBX importer converts cm -> metres EXACTLY ONCE.
All modular models were authored with their equipment geometry already laid out at common assembly origin (0,0,0).
Instantiate each numbered FBX once under L90_HD_VISUAL_ART at identity local transforms.
Do not also instantiate Models/Level90_Complete_ArtKit_Showcase.fbx.

NOT INCLUDED / NOT CLAIMED
- No actual SparkTerminal components or player-created physical wires; 254 SOCKET_P... are decorative.
- No circuit solver or PPU/plasma/xenon/trajectory safety process model.
- No authoritative 6/6 gameplay, fault recovery or scene builder.
- Unity compilation, import and Play Mode have NOT been tested.
- Procedural reference textures need UV unwrap, baking, packing, LOD and production shading.
- Generated emitter/plasma shapes are only visual geometry, not VFX or physical ion propulsion.

CRITICAL W157
L90_NeutralizerCurrentTransducer {WIRES[156]['SourceVisualAnchor']} -> L90_PropulsionSafetyPLC {WIRES[156]['TargetVisualAnchor']}.
See W157_Critical_Neutralizer_Current_Recovery.md. Never confuse the decorative anchor with a functional SparkTerminal.

This package supplies the art and authoring plan for a future Unity implementation only.
''')
put('AAA_Art_Delivery_Notes.md',f'''# Level 90 — The Long Burn — 3D Art Delivery Notes

- **94** separately importable binary FBXs / **94** matching GLB files.
- **80** named pivot parents including four three-part Hall-thruster rigs (yaw / pitch / discharge indicator), twin solar wings and xenon valve controls.
- **254** modeled decorative gold-socket marker positions.
- **176** planned, NOT created player wires and **352** independent logical contact roles.
- **{len(CONFLICTS)}** reused visual anchor locations. Some use incompatible signal classifications. NEVER make these one electrical node.
- **12** separate procedural reference PBR texture materials (36 maps, 1024 square).
- AURORA-1 spacecraft body and deployable solar wings with separate FBX components, four annular Hall thrusters, cathode systems, high-pressure xenon tanks, magnetic coils, power modules, protected command electronics, sensor racks and control bay.

## Real deliverables and remaining work
FBX/glTF files contain actual, editable triangle meshes and spatial pivot nodes. Visual-tech-art quality is **AAA-inspired procedural**; it is NOT a final film-asset, hand-sculpted production AAA model, mesh-optimized game-ready LOD, or validated flight hardware model. Shader authorship, full UV unwrapping, collisions, lighting, emissive VFX, animation rigging hierarchy review and performance profiling remain to be done.

Planned wires, SCADA data, plasma behavior, thermal dynamics, propulsion thrust, finite xenon mass, safety latching and LevelGamePlayManager objective logic **do not exist** in this art package.
''')
put('AAA_Polish_And_Lighting_Plan.md','''# Level 90 — AAA Finish and Lighting Plan

## Art direction
Blue-violet Hall-thruster plasma, white/titanium spacecraft armor, deep blue solar cells, warm gold industrial mechanics and cyan scientific holograms. Keep the spacecraft central, maintain clear player circulation around racks, and distinguish actual engine-plasma emission from unrelated decorative highlights.

## PBR production
1. Create authored UV sets for every reusable modular mesh. Current generated PNGs are reference maps, not complete baked PBR material application.
2. Repack MetalRoughAO (R metal / G roughness / B AO) correctly for the project's actual URP/HDRP shader. Invert roughness to smoothness where needed.
3. Bake curvature, AO and detailed normal maps; author wear/edge treatment only where industrially plausible.
4. Use physically plausible translucent displays/optical windows, double-sided glass only where needed and carefully controlled alpha sorting.
5. Convert static trim to modular atlases, instanced detail for repeating cell/busbar geometry and cut LODs for long-distance ship views.
6. Add measured-state-driven runtime VFX: low-power discharge initiation, steady blue annular emission, hot surfaces, status LEDs and alerts. No animated geometry is evidence of live propulsion.

## Lighting
- HDR-space environment with gold practical workstation light; keep cockpit and precision control instruments readable.
- Engine glow must be enabled only from authoritative discharge state, fade on shutdown and respect safety inhibition.
- Restrained blue rimlight on spacecraft, real-time contact shadows on movable engine rigs, baked static fixtures.
- Limit cockpit emissive textures to avoid bloom washing out signal labels and hazard warnings.

## Performance
Separate near/far ship views and lab views. Use LODGroups and combine stationary hull trim. Profile draw calls and transparency; do not assume procedural mesh density automatically equals AAA quality.
''')
put('Gameplay_Integration_Plan.md',f'''# Project Spark Level 90 Gameplay Integration Plan — Proposed Work

1. Inspect actual existing SparkCircuitSystem, SparkElectricalSolver, SparkTerminal, SparkLevelEvaluator, LevelGamePlayManager, inventory and wire-disconnect API definitions.
2. Import 94 modular FBXs and the 80 exact ANIM_* parent groups; create gameplay colliders and electrical functional ports separate from disposable L90_HD_VISUAL_ART.
3. Reconcile {len(CONFLICTS)} shared decorative socket locations. Keep 254 art anchors unchanged, create 352 distinct logical endpoint mappings, merge only provably compatible bus contacts. Record mapping and new contact counts.
4. Register 176 player-connectable wire declarations, but do not instantiate puzzle wires connected on load. All must update real circuit graph topology.
5. Model three separate domains: real educational 5 V control graph; fictional protected energy + xenon + plasma plant; independent safety authorizations.
6. Physically coherent finite propellant: tank pressure/mass state, regulators, actual valves, measured mass flows, measured PPU powers/losses and finite thermal dynamics.
7. Four propulsion channels: protected precharge, cathode neutralizer, magnet current, ion beam/discharge, actual thrust, gimbal feedback, thermal constraints and power availability.
8. Orbital guidance: commanded vs measured gimbal angles, finite thrust/propellant, full trajectory propagation, flight departure constraints and proper safe separation from the docking station.
9. Disturbances: xenon flow restriction, coil overheating, gimbal feedback drift; require diagnosis, physical repair and new qualified mission run.
10. Critical real W157 disconnect changes topology, invalidates trusted neutralizer feedback, inhibits ignition and initiates physical safe shutdown; reconnection + fresh measurement + calibration + independent checks + explicit reset + new full departure run.
11. Persist historical Objective 1–5 evidence even while current live health is invalid; Objective 6 requires evidence generated strictly after W157 repair. Connect only via actual LevelGamePlayManager APIs.
12. Wire read-only SCADA and visual rig to *actual authoritative measured state*, never to command widgets or fake timers. Perform Unity compilation, Play Mode, integration and previous-level regression tests in the actual user's Unity project.

**One-click gameplay builder target (NOT included here):** Tools/Project Spark/Levels/Create Level 90.

**Safety:** fictional bounded classroom game simulator, not physical propulsion system instructions or NASA qualification claims.
''')
put('Visual_Rig_Integration_Notes.md','''# Level 90 Read-only Visual Rig Notes

SparkLevel90VisualRig.cs provides these *art-only* entry points:
- SetActualEngineStates(A, B, C, D, measured PPU powers A–D)
- SetActualGimbals(actual yaw/pitch of each A–D engine)
- SetActualPropellantAndMechanisms(tank A/B fractions, port/starboard solar deploy, coolant pump speed)
- SetActualMissionAndSafety(departure progress, measured deltaV normalized for display, trusted W157 validity, fault latch, mission qualification)

Pivots are looked up from the imported 80 ANIM_* nodes. They drive mechanical pieces or decorative indicators. The visual rig never calculates thrust, plasma current, xenon flow, mission success, fault reset, nor manipulates the electrical solver.

Dedicated adapter required in actual Project Spark: physical state -> properly scaled visual parameters. Unity compilation and assembly import remain unverified.
''')
rows=[]
for i,name in enumerate(NAMES):
 if i<16:kind='static' if i not in (3,7,10,11,13) else 'mechanical'
 elif i<32:kind='mechanical' if i in (16,17,18,19,20,21,22,24,25) else 'static'
 elif i<62:kind='mechanical' if i in (32,33,34,35,37,46,47,51,52,53,54,55,56,57,58,59) else 'static'
 else:kind='mechanical' if i in (67,70,76,77,78,79,81,85,87,88,89) else 'static'
 rows.append(dict(Model=name,ColliderPlan=('simple_convex_parts' if kind=='mechanical' else 'proxy_box_capsule'),
   RecommendedLOD='LOD0+LOD1+LOD2',MovablePartsMustRemainSeparate=kind=='mechanical',
   PhysicsVsVisual='SIMULATION_DRIVES_VISUAL_NEVER_REVERSE',AuthorStatus='PLANNED_NOT_BUILT'))
with (ROOT/'Collision_LOD_Production_Plan.csv').open('w',newline='',encoding='utf8') as f:
 w=csv.DictWriter(f,fieldnames=list(rows[0]));w.writeheader();w.writerows(rows)
flow=[
 ('Control','5V_ControlSupply','Ignition_Sequencer','power/return','Educational logic only'),
 ('Propellant','TankA','XenonFeedManifold','finite mass & pressure','Modeled protected plant state'),
 ('Propellant','TankB','XenonFeedManifold','finite mass & pressure','Modeled protected plant state'),
 ('Propellant','XenonFeedManifold','FourActualFlowControllers','mass flow and valve lag','Actual flow separate from command'),
 ('Energy','ProtectedDCBus','PPU_A_B_C_D','bounded electric power','Conversion losses, precharge, max current'),
 ('Energy','PPU_A_B_C_D','HallThruster_A_B_C_D','discharge power','Must respect magnet and cathode limits'),
 ('Plasma','Neutralizers_A_B_C_D','IonBeam_A_B_C_D','neutralization status','W157 trusted current is critical control feedback'),
 ('Dynamics','HallThruster_A_B_C_D','Aurora1SpacecraftState','force / mass / deltaV','Actual trajectory integrated over time'),
 ('Thermal','PPU_HallThrusterHeat','ThermalControlPumpAndRadiator','joules and heat','Requires measured cooling capacity'),
 ('Safety','PrimaryNeutralizerCurrent','PropulsionSafetyPLC','W157 real topology','Cannot be bypassed by SCADA cached display'),
 ('Safety','IndependentProtection','PPU_Inhibit','safe shutdown','Independent action remains available')]
with (ROOT/'Physical_Propulsion_Process_Concept.csv').open('w',newline='',encoding='utf8') as f:
 w=csv.writer(f);w.writerow(['Domain','From','To','Flow','Constraint']);w.writerows(flow)
print('Level90 support docs generated:',len(rows),'LOD rows and',len(flow),'process flows')
