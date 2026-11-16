#!/usr/bin/env python3
"""Reproducible support documents for the real Level 92 procedural 3D art delivery."""
from pathlib import Path
import csv,json
from build_level92 import ROOT,NAMES,P,WIRES,LOGICAL,ANCHORS,CONFLICTS,PV,OBJECTIVES
manifest=json.loads((ROOT/'Model_Manifest.json').read_text())
CRIT=next(w for w in WIRES if w['WireID']=='W171')
(ROOT/'AAA_Art_Delivery_Notes.md').write_text(f'''# Project Spark — Level 92: THE SILENT CORE
## Procedural HD / AAA-inspired 3D art delivery

### Actual supplied files (source-derived)
- **{manifest['modular_fbx_count']} original modular binary FBX 7400 models** and **{manifest['modular_glb_count']} matching GLB scenes**.
- One **alternative assembled full-scene FBX** and one GLB showcase; do not instantiate the assembled model together with all numbered modular pieces.
- **{manifest['triangle_count']:,} modular triangles**, **{manifest['mesh_chunk_count']:,} material/pivot mesh chunks**, **{manifest['animation_pivots']} animation pivot groups** and **{manifest['decorative_sockets']} gold-rimmed visual sockets**.
- **12 procedural reference PBR material groups**, **36 PNG maps**. Maps have *not* been artist-authored, UV-unwrapped or baked to this geometry.
- **188 proposed player-wire rows**, **376 proposed independent logical contacts**, **{manifest['reused_socket_locations']} reused art socket positions**, including **{manifest['mixed_signal_socket_locations']} mixed-kind contacts requiring explicit reconciliation**.
- **6 planned gameplay objective definitions** and fault specification for **W171**. Neither connectivity nor certificate completion is implemented.

### Specific modeled level equipment
- KHEPRI-7 faceted carbonaceous surface, fractured boulders, geophysical cross-sections and terrain scanning platforms.
- AURORA-1 observation vehicle, six-legged lander central hull, six independently named articulated legs, six separately named microspine anchoring feet.
- Multi-assembly rotary-percussive drill, separately movable spindle, carbide teeth, ball-screw feed, motor, percussive striker, axial load and torque instruments.
- Radar A/B, terrain LiDAR, seismic contact probes, subsurface fusion processors and a five-layer holographic geological display.
- Core capture gripper, two sealed core cartridges, six-pocket sample carousel, seal-test chamber, sample transport and tether/winch retrieval equipment.
- Safety PLC, electrical cabinets, patch panels, SCADA, independent emergency controls and scripted visual fault equipment.

### Import convention
- World arrangement is authored inside the geometry: **all numbered models share one world origin**. Import and instantiate all numbered modular models at local `(0,0,0)`, identity rotation and unit object scale **after conversion to metres**.
- FBX numeric vertices are **centimetres** with `UnitScaleFactor=1`; GLB coordinates are **metres**. Apply FBX conversion **once only**, verify a known meter-size cabinet and compare world bounds to manifest.
- Expected modular bounds metres: `{manifest['bounds_min_m']}` to `{manifest['bounds_max_m']}`. Models follow **Y-up, X-right, Z-forward** coordinate convention.
- Use `L92_HD_VISUAL_ART` as an independently replaceable child of the Level 92 layout. Keep all real terminals, puzzle wiring, gameplay controls and authored objects elsewhere.

### Unity menu tools (art only)
- `Tools/Project Spark/Levels/92/Place HD Art Under Selected Layout`
- `Tools/Project Spark/Levels/92/Create & Apply HD Materials To Selected Art`
- `Tools/Project Spark/Levels/92/Validate Imported HD Art`
- Runtime visual adapter class `ProjectSpark.Visuals.SparkLevel92VisualRig`.

### Important production limitations
These are **original editable procedural mesh assets**, NOT a complete AAA production-quality level. The ZIP does **not** contain production UV texture bakes, proven collision meshes, LODs, advanced shaders/VFX, rig-tested robotics, simulated regolith, functioning SparkTerminals, actual player wires, gameplay PLC, or objective certification. Asset structure is checked offline; Unity import, compilation, and Play Mode are not verified here.
''',encoding='utf8')
(ROOT/'Gameplay_Integration_Plan.md').write_text(f'''# Level 92 — gameplay engineering integration plan (not implemented)

**Goal:** use the actual Project Spark Unity APIs, not invented signatures. Preserve Levels 1–91.

## Three separate authorities
1. **Electrical graph**: player-created wires, educational 5 V control power, returns, compatible functional `SparkTerminal` directions and real disconnect consequences.
2. **Mechanical/process plant**: geophysical survey, six-leg contact physics, microspine actual grip, anchor load redistribution, bit feed/torque/temperature/rock resistance, sample capture/containment and finite tether length.
3. **Independent safety PLC**: critical W171 trusted force path, qualified force margins, interlock inputs, latched faults, confirmed actuator disable, manual reset. Safety must not rely on SCADA visuals.

## Electrical plan
- Source `Gameplay_Wiring_Concept_W001_W188.csv` has **188 conceptual, NOT player-wired edges**. Reconcile **266 decorative P-socket nodes** and **376 independent logical endpoint occurrences**.
- `Terminal_Anchor_Signal_Conflict_Report.csv` lists reused socket locations; never electrically short differing `SignalKind` fields through one decorative anchor. Map to functional multichannel terminal blocks or allocate actual separate subcontacts with recorded validation.
- Keep simulated protected drill power separate from 5 V interface and independent safety channels. Do not attach high-power process values to Spark 5 V control nodes.
- Required critical route: `{CRIT['SourceModel']}` / `{CRIT['SourceVisualAnchor']}` `TRUSTED_ANCHOR_LOAD_OUT` → `{CRIT['TargetModel']}` / `{CRIT['TargetVisualAnchor']}` `TRUSTED_ANCHOR_LOAD_IN`.

## Single authoritative geophysics, landing and coring plant
- Subsurface layered properties: regolith, fractured interface, bedrock inclusion, inferred radar response and uncertainty; deterministic scenario seeds.
- Geophysical instrument measured data must respect calibration, pose, scan coverage, noise, validity and timestamp.
- Lander rigid-body position, orientation, momentum, contact locations and finite tether deployment; no scripted docking/teleporting.
- Per-leg encoder, contact normal, shear, microspine grip and spring behavior; actual operational load boundaries from geometry and measured forces.
- Drilling bit rpm, axial feed, feed acceleration, motor torque/current, bit wear, thermal response, actual depth, debris mass and core integrity.
- Sample capture, tube seal state, material continuity, mass conservation, contamination proxy, cartridge leak measurement and retrieval.
- Mutations from all three faults must affect plant variables, be observable through calibrated instruments, and require physical simulated repair.

## Objective evidence
1. Real 188-wire circuit, valid supply, return and independent safety.
2. Calibrated geophysical scans, adequate coverage and qualified site selection with uncertainty.
3. Deployed lander, six-leg contact, secure anchor and successfully bounded test load.
4. Actual drilled valid core, documented sample depth, cartridge seal and mass confirmation.
5. Real anchor-slip, drill-stall and cartridge-seal disturbances diagnosed, repaired and followed by a fresh complete sampling run.
6. Actual W171 removed from electrical graph -> trusted samples stale -> drilling safely inhibited -> latched fault -> physical reconnect -> fresh calibration -> explicit reset -> new complete core recovery and containment qualification. Historical 1–5 remain certified.

## Exact integration guidance
Inspect real `SparkElectricalSolver`, `SparkCircuitSystem`, `SparkElectricalComponent`, `SparkTerminal`, `SparkLevelDefinition`, `SparkLevelEvaluator`, `SparkLevelSceneBindings`, `SparkObjectShelf`, `SparkLevelLayoutLoader` and `LevelGamePlayManager` classes where present. Use their actual source signatures and existing level-builder patterns. `SparkLevel92VisualRig` is **visual only**; pass authoritative actual positions/speeds/load and safety status to it. It cannot certify any objective.

## Deliverables still required
- Full one-click gameplay Editor menu `Tools/Project Spark/Levels/Create Level 92`, actual Level_92.asset and Level_92_Layout.prefab with real evaluator bindings.
- Real 188 player-connectable edges and reconciled independent terminals.
- Unity simulation, safety PLC, scenario state machine, three repair interactions, measured telemetry, six objective evaluator classes and Play Mode QA.
- Collisions/LODs and production material/VFX polish. Do not report completed implementation until exercised in Unity.
''',encoding='utf8')
(ROOT/'AAA_Polish_And_Lighting_Plan.md').write_text('''# Level 92 — AAA art finishing and lighting plan

## Signature visual direction
Dark porous carbonaceous asteroid terrain with irregular faceted silhouettes, sharp grazing sunlight, cold cyan subsurface scanning overlays, amber mechanical warnings, white titanium lander and gold contact hardware. Retain clean visual contrast between the dark rock zone and interior instrumentation.

## PBR authoring to complete
- Unwrap meshes with sensible texel density, bake high-poly normal/AO details and author production albedo, metallic, roughness and masks (current 36 textures are only 2D procedural reference swatches).
- Add geometric high-frequency details on anchors (hooks, flexures, joints), drilling teeth, cable strain relief, flange bolts, structural lattice and service handholds.
- Build readable decals and safety labels as actual textured surfaces in Unity (do not assume procedural geometry contains finished text).
- Apply relevant wear: rock dust on drill/feet, brushed titanium contact, darkened bit edges, anodized black electronics, glass cartridge containment and clean medical-like lab interior.

## HDRP/URP stage lighting
- Off-axis low sun and cold ambient from deep-space exterior; amber near-drill work lights and cyan scan indicators. Use reflection probes/SSR judiciously.
- Realistic glancing rock roughness and subtle sub-surface geological false-color data visualization. Holograms should have depth and occlusion but never obscure critical terminal sockets.
- Dust/debris must be small and slow-moving in low gravity and driven by actual drilling state, not continuous decorative spraying.
- Real thermal and engine VFX require authoring; included FBX files have only static material regions, not full particles/VFX.

## Geometry optimization
- Static combine/instancing only for nonmoving structure. Never merge a named `ANIM_*` pivot into a static mesh.
- Add collisions only where interaction/obstruction matters; landscape and rock walls need simplified proxy collision rather than per-triangle dynamic MeshColliders.
- Create 3-level LOD for distant boulder clusters, spacecraft, and high-frequency drill details; disable unseen high-detail sample tubes.
- Evaluate CPU and draw calls in Unity Frame Debugger, monitor memory after textures and explicit shader setup.
''',encoding='utf8')
(ROOT/'Visual_Rig_Integration_Notes.md').write_text('''# Level 92 visual rig — authoritative data adapter

`SparkLevel92VisualRig` is in `Unity_Optional/SparkLevel92VisualRig.cs` (runtime folder, never under Editor). Its public methods consume measured feedback:

- `RebindArtPivots()` binds every imported `ANIM_*` once after art placement.
- `SetActualLegPose(index, hipDegrees, kneeDegrees, footDegrees, deployed01, latch01)` for each of six legs; do not feed unachieved commands.
- `SetActualMechanicalState(drillRPM, feedDepthM, winchRPM, carouselDeg, drillTorqueNormalized, anchorConfidenceNormalized)` for measured plant values.
- `SetActualSafetyState(trustedW171Valid, actualLatchedFault, actualMissionQualified)` for actual certified state.

The visual rig does not implement inverse kinematics, collision detection, electrical graph, real six-leg support forces, drill physics, UI, safety PLC or objective completion. It uses named pivot transform modifications and indicator lights only. Do not attach interactions directly to replaceable visual nodes; keep gameplay components under stable layout roots and address art via an adapter.

Unity import conversion: FBX geometric vertices in centimeters; convert to meters once using the selected model importer units. Verify pivot centers and local origin alignment before any locomotion animation.
''',encoding='utf8')
(ROOT/'Physical_Geological_Process_Concept.csv').write_text('Subsystem,Source or input,Simulated process,Required measurements,Fault effect,Qualification evidence\n'
 'GeophysicalMapping,Radar and surface pose,Layered reflectivity and uncertainty,Ground echo and scan coverage,Ambiguous poor returns,Calibrated multi-location survey\n'
 'LanderAnchoring,Contact geometry and per-leg force,Force balance and microspine engagement,Leg displacement and trusted load W171,Anchor slip or shear fault,Valid test loads and stable platform\n'
 'Coring,Feed plus rotation torque and rock properties,Penetration rate bit work and thermal energy,Actual depth motor current torque and bit temperature,Bit stall or excessive feed,Valid stratigraphic core with bounded loads\n'
 'Containment,Core volume and cartridge seal,Core retention indexing compression and test,Sample mass and seal integrity,Failed cartridge closure,Fresh qualified sealed sample\n'
 'TetherRetrieval,Winch motion and spacecraft geometry,Finite length and dynamic tension,Line tension length and attachment state,Overtension or slack,Safe retrieval and contact clearances\n'
 'Safety,True Spark topology and samples,Trusted force validity and independent inhibit,Timestamp calibrated W171 and actuator disable,Loss of trusted feedback,Latched shutdown explicit reset and fresh run\n',encoding='utf8')
rows=[]
for m in manifest['models']:
 i=m['index'];n=m['name'];grp=('rock_environment' if i<16 else 'lander_kinematics' if i<34 else 'drill_process' if i<54 else 'geo_scanner' if i<68 else 'sample_handling' if i<82 else 'electronics')
 rows.append(dict(Model=n,Triangles=m['triangles'],Group=grp,ColliderStrategy='Simplified static collider' if i<16 else 'Articulation proxy collider' if i<34 else 'Bounded primitive collider' if i<82 else 'Instrument shell box collider',
 LOD0='Original procedural mesh',LOD1='Reduce repetitive subparts to ~50%',LOD2='Silhouette proxy ~15%',InteractivePivotCount=len(m['pivot_names']),ProductionReady='NO'))
with (ROOT/'Collision_LOD_Production_Plan.csv').open('w',newline='') as f:
 w=csv.DictWriter(f,fieldnames=list(rows[0]));w.writeheader();w.writerows(rows)
print('SUPPORT DOCS OK',len(rows),flush=True)
