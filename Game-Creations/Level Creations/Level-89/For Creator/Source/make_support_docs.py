#!/usr/bin/env python3
"""Produce source-grounded art package notes and future Unity integration plan, not gameplay code."""
from pathlib import Path
import json,csv
from build_level89 import ROOT,NAMES,P,PIVOTS,WIRES,ANCHORS,LOGICAL,CONFLICTS,core

def write(fn,content): (ROOT/fn).write_text(content,encoding='utf8')
write('README_Unity.txt',f'''PROJECT SPARK — LEVEL 89: THE HELIOS STORM
ORIGINAL PROCEDURAL 3D ASSET KIT — EDITABLE FBX / GLB MESHES

DELIVERED ART AND SOURCE PLANNING
- 92 separately importable numbered binary FBX 7400 scenes under Models/ and 92 corresponding GLB scenes under GLB/
- Assembled reference FBX and GLB (DO NOT instantiate alongside the 92 individual numbered models)
- 76 uniquely named ANIM_ mesh-owning parent pivot groups, including shield cassettes, pump rotors and instrument status pivots
- 248 geometry-modeled gold-rimmed visual socket nodes, not active SparkTerminals
- 170 proposed wiring entries with 340 separate logical endpoints, {len(CONFLICTS)} visually shared anchors needing reconciliation
- 12 procedural texture reference sets / 36 PNGs at 1024x1024, not baked artist-authored UV maps
- Four optional Unity art-import/material/validation/actual-state-only visual rig scripts
- Manifest, assembly map, objective design, W149 planned critical feedback, production plans and structural test report

UNITY IMPORT
1. Create Assets/ProjectSpark/Level89/Models/ and copy Models/00_*.fbx through 91_*.fbx, excluding the assembled showcase FBX.
2. Copy Textures/ into Assets/ProjectSpark/Level89/Textures/ if using the reference palette workflow.
3. Put SparkLevel89*Editor.cs in an `Editor` folder. Put SparkLevel89VisualRig.cs outside all Editor folders.
4. Select the layout GameObject, then run `Tools/Project Spark/Levels/89/Place HD Art Under Selected Layout`.
5. Run `Tools/Project Spark/Levels/89/Create & Apply HD Materials To Selected Art` on the generated art root.
6. Run `Tools/Project Spark/Levels/89/Validate Imported HD Art`. Confirm all 92 FBX models, 76 pivots, 248 decorative socket transforms.
7. Verify imported scale EXACTLY ONCE: FBX numeric vertices are authored in centimetres with UnitScaleFactor=1; GLB vertices are metres. Y up.
8. All modular equipment models are in common-origin global assembly coordinates. Set each placed instance localPosition=(0,0,0), localRotation=identity, localScale=(1,1,1), *after correct importer unit handling*.
9. Keep real SparkTerminals, circuit solvers, PLC logic and gameplay outside L89_HD_VISUAL_ART so replacing visuals cannot erase playability.
10. Use Gameplay_Wiring_Concept_W001_W170.csv, Terminal_Logical_Port_Reconciliation_340.csv, Object_Certification plan and W149 doc for building real gameplay separately.

QUALITY AND HONEST LIMITATIONS
Delivered original hard-surface procedural technical-art preview with distinct physical mesh details and transform pivots.
NOT INCLUDED: final AAA UV unwrap/baked texture production, final art-directed lighting, colliders and authored LOD meshes,
real runtime wire circuits, a radiation transport solver, dose validation, safety PLC gameplay, LevelGamePlayManager 6/6,
Unity compilation, Unity FBX import/Play Mode tests. Structural GLB and binary header validation is separately reported.
These assets are original game fiction, not qualified flight hardware, radiation protection or equipment design.
''')
write('AAA_Art_Delivery_Notes.md',f'''# Project Spark — Level 89: The Helios Storm | Original AAA-inspired 3D art kit

## Delivered geometry and planning

- **92** individual numbered binary FBX + **92** individually addressable matching GLB files
- One optional assembled reference FBX + GLB (do not duplicate with modular set)
- **76** mechanical/display `ANIM_*` pivot groups with mesh-owning transform nodes
- **248** individually modeled decorative socket nodes
- **170** conceptual electrical wire routes `W001`–`W170`, **340** independent planned logical endpoints
- **{len(CONFLICTS)}** reused art sockets explicitly reported; ports must not be electrically shorted because they share a visual position
- **36** original procedural PBR reference PNGs (12 sets) and source generators
- Four optional Unity art-only C# scripts; no actual circuit/safety/runtime qualification included

## Distinct hero 3D models

- `02_L89_OrbitalStationHabitatExterior` — segmented distant station ring with hub, radial truss spokes and window modules
- `03_L89_StormShelterPressureHull` — cylindrical armoured multi-ring protection enclosure with interior deck
- `04_L89_ShelterEntranceShieldedBulkhead` — movable armor hatch and multi-point locking hardware
- `05_L89_SolarWeatherHologramDome` — orbiting particle vector and instrumented visual rings
- `12_L89_SolarEnergeticParticleTelescopeA` / `13_...B` — independent directional radiation instrument gimbals
- `28_L89_ShelterReferenceDosimeter` — critical W149 sensor hardware and original P### visual socket
- `39`–`42` — four physical blue sight-glass water-wall tank modules
- `43_L89_WaterReservoirMain` — finite resource storage reference art
- `45` / `46` — transfer pumps with physically named rotating impeller pivots
- `52`–`55` — four motorized shelter shielding cassettes with own movable mesh pivots
- `57`–`60` — four actual servo actuator rod/pivot components
- `61_L89_ShieldLockingPinArray` — two named multi-pin locking mechanisms
- `82_L89_RadiationDefenseSCADA` / `83_L89_SolarStorm3DParticleHologram` — 3D holographic status meshes

## Reality check

FBX/GLB art and the associated source manifests exist. Electrical wires, sensor samples, particle physics,
exposure calculations, safety control, objective evaluation, authored collision/LODs, Unity compilation and Play Mode
still require implementation and real testing. Animation pivots represent visual state only and must never decide safety.
''')
write('Gameplay_Integration_Plan.md',f'''# Level 89 — Unity engineering integration contract (design requirements, NOT existing gameplay)

1. Preserve Levels 1–88. Inspect actual `SparkElectricalSolver`, `SparkCircuitSystem`, `SparkTerminal`, `SparkLevelDefinition`, `SparkLevelEvaluator`, `SparkLevelSceneBindings`, `SparkLevelLayoutLoader`, `SparkObjectShelf` and `LevelGamePlayManager` APIs before implementation. Do not invent signatures.
2. Add a separate, idempotent `Tools/Project Spark/Levels/Create Level 89` gameplay builder. Current optional `.../89/Place HD Art Under Selected Layout` is **art placement only**.
3. Import 92 FBX visual instances in one `L89_HD_VISUAL_ART` disposable root. Keep terminals, physics, safety and objectives outside this root. Use source FBX importer scale exactly once.
4. Translate the **170** conceptual WIDs to actual player created Spark electrical graph connections, initially unconnected. Preserve all **248** art anchor locations while accommodating **340** independent logical endpoint contacts. Review {len(CONFLICTS)} reused art sockets and split analog/power/digital/safety ports if needed to avoid electrical shorting.
5. Establish distinct educational 5V control/signals domain; fictional particle/energy dose process domain; protected independent safety and habitat essential supply domain.
6. Author a bounded deterministic fictional solar-particle event state with independent storm phases quiet→rise→peak→decay, variable particle energy bins, source intensity, optional uncertainty and seeded faults.
7. Implement instrument calibration, finite dynamic range, detector saturation, counter statistics/noise proxies, sampled sensor timestamps, range validity, redundancy and sample freshness. SCADA is display only.
8. Model radiation shielding geometrically with water-wall module positions, known shielding areal-density proxies, finite resource mass, shield gaps and energy-bin-dependent transmission. Do not use a blanket physical radiation protection certification formula or pretend scalar attenuation is real spacecraft qualification.
9. Actuators have finite speed, acceleration/limits, measured encoder values, jam states, actual shield placement and independent real mechanical locks. Requested shield coverage cannot be certified as actual coverage.
10. Maintain finite water inventory shared with Level 88 life-support abstractions. Pump transfer consumes modeled energy and respects reservoir floors and actuator state. Reserve constraints protect essential ventilation, thermal maintenance, oxygen and electrical distribution.
11. Independently integrated fictional sheltered exposure is computed from actual storm and shield state; do not let an invalid dose sensor set true physical exposure to zero. Protect habitat mission utility (not every load off).
12. Enforce independent safety PLC, shelter door interlock, protective operating mode, measured actuation, emergency isolation, fault latch and explicit reset. No manual button may override invalid W149 trusted feedback for normal certification.
13. Three disturbances: detector measurement saturation, jammed panel hardware, unexpected high-energy particle surge. Each causes measurable authoritative effects, documented repair, then fresh full event for Objective 5.
14. Make W149 a real detachable player wire from `{WIRES[148]['SourceModel']}` (`{WIRES[148]['SourceVisualAnchor']}`) to `{WIRES[148]['TargetModel']}` (`{WIRES[148]['TargetVisualAnchor']}`). Physical disconnect → lost valid sample → normal safety authorization withdrawn → protective hold + latch; reconnect → fresh calibrated sample → explicit reset → fresh full storm → 6/6. Objectives 1–5 remain historically awarded.
15. Update `SparkLevel89VisualRig` through **actual measured states only**, never drive particle process, safety, terminal validity or objective status by reading meshes/animations/lights.
16. Provide compile, Play Mode, actual wiring, independent safety, signed state evidence and regression QA. Report unexecuted tests honestly.

**Note:** This is a fictional game educational simulation, not a validated particle-transport, human radiation dosage, or shelter safety analysis.
''')
write('AAA_Polish_And_Lighting_Plan.md','''# Project Spark L89 — AAA Production Art Polish and Lighting

## Composition
Left: spectacular dual solar-particle telescopes, calibration instrumentation, orange/amber space-weather holograms.
Center: huge 6.2-m class modular shielded storm shelter and door. Right: blue segmented transparent water-wall
tanks and shield-control valve/pump skid. Front: dense gold-rimmed terminals, PLC racks and usable status panels.
Backdrop: stylized station habitat and panorama with luminous orange solar-event warning motifs (a static art environment).

## Required final production work beyond this generated kit
- Author proper UV unwraps, production trim sheets, normals/roughness/AO, decals and glass transparency depth ordering.
- Add bevels/weighted normals and collision meshes, LOD0/LOD1/LOD2 or mesh simplification and GPU instancing where appropriate.
- Bake texture maps from production geometry. The 36 included maps are procedural **reference maps without finished UVs**.
- Build real material variants for stainless/ceramic water shielding, brass contacts, radiation-detector optics and black-polymer seals.
- Add authentic component engraving and readable electrical terminal numbers as authored textures or UI assets.
- Distinguish amber storm warnings, blue water inventory, red fault lamps and cyan valid sampling without overwhelming readable wiring.
- Create and tune artist-authored VFX (sensor acquisition, flow particles, hologram animation), lightmaps/reflection probes and UI.
- Verify station ring background, shelter geometry, telescope direction and control workstation visibility from the actual game camera.

## Performance
Plan scene bounds from `Model_Manifest.json`. Do not duplicate the assembled visual showcase alongside 92 models.
Use instancing for repetitive bolts, optimized glass and shell geometries, appropriate physics layers and simple colliders.
All animation pivots are art read-only consumers of actual state, with `indicator` types not treated as machine actuators.
''')
shield_paths=[
('ExternalParticleFlux','DetectorA/B','Spectral samples, bounded dynamic range'),
('DetectorA/B','NavigationForecast','Measured particle spectrum with sampling validity'),
('Forecast','MissionSequencingPLC','Predicted storm horizon, non-authoritative'),
('ExternalParticleFlux','UnshieldedHabitat','Actual fictional ambient exposure process'),
('WaterReservoir','WaterPumpA/B','Finite water mass transfer'),
('WaterPumpA/B','FourShieldTanks','Only if actual valve flows and pump power'),
('FourShieldTanks','ShieldArealDensity','Physical shielding geometry and per-energy transmission'),
('ShieldCassettes','ShieldCoverage','Actual encoder/lock state, not command state'),
('ShieldArealDensity','ShelterDoseProcess','Areal mass and physical gaps affect protection'),
('UnshieldedHabitat','ShelterDoseProcess','Actual external environment, independent of UI'),
('ShelterDoseProcess','ShelterReferenceDosimeter','Fresh measured sample, possible overrange'),
('ShelterDoseProcess','IndependentDosimeter','Separate protection measurement'),
('ShelterReferenceDosimeter','W149','Critical sampled feedback; must be live circuit edge'),
('W149','HeliosSafetyPLC','Invalid sample inhibits normal safety authorization'),
('HeliosSafetyPLC','ShieldActuators','Bounded protected movement permission'),
('EssentialPowerAndLifeSupport','ShelterMission','Must remain active through storm'),
('ShelterDoseProcess','ObjectiveHistory','Full run-scoped integrated dose evidence')]
csv_rows=[dict(Source=s,Target=t,Meaning=d,ImplementationStatus='PLANNED_NOT_IMPLEMENTED') for s,t,d in shield_paths]
with (ROOT/'Physical_Shielding_Process_Concept.csv').open('w',newline='',encoding='utf8') as f:
 w=csv.DictWriter(f,csv_rows[0].keys());w.writeheader();w.writerows(csv_rows)
with (ROOT/'Collision_LOD_Production_Plan.csv').open('w',newline='',encoding='utf8') as f:
 rows=[]
 for i,n in enumerate(NAMES):
  priority='High' if i in [0,1,2,3,4,5,12,13,28,39,40,41,42,43,52,53,54,55,57,58,59,60,61,82,83] else 'Medium'
  motion='Animated' if any(p['Index']==i and p['Kind']!='indicator' for p in PIVOTS) else 'Static/indicator'
  coll='Compound box/capsule' if i in (3,4,12,13,39,40,41,42,43,52,53,54,55) else 'Simplified box' if i not in (0,1,5,83) else 'Special/batched nonblocking'
  rows.append(dict(Index=i,Model=n,HeroPriority=priority,MotionType=motion,ColliderPlan=coll,LODStatus='REQUIRES_AUTHORED_PRODUCTION_LODS',UVStatus='REQUIRES_UNWRAP',RendererBatching='Instancing review'))
 w=csv.DictWriter(f,rows[0].keys());w.writeheader();w.writerows(rows)
write('Material_Palette.json',json.dumps({'level':'level_89','preview_palette':{k:list(v) for k,v in core.COLORS.items()},'status':'REFERENCE_ONLY_NEEDS_UV_AND_PBR_PRODUCTION'},indent=2))
print('CREATED SOURCE-GROUNDED DOCUMENTS')
