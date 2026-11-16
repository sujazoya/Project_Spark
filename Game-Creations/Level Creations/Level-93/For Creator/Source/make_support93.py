from pathlib import Path
import csv,json
import build_level93 as b
R=Path(__file__).resolve().parents[1]
M=json.loads((R/'Model_Manifest.json').read_text());T=json.loads((R/'Texture_Manifest.json').read_text())
(R/'README_FIRST.md').write_text(f'''# PROJECT SPARK — LEVEL 93: THE ELEMENTAL ARCHIVE

## ACTUAL MODULAR 3D ASSET PACKAGE — ART, NOT A COMPLETED GAME LEVEL

- **{M['modular_fbx_count']} individually importable binary FBX 7400 files**, with UV and vertex-normal layers, named PBR material slots and independent named animation pivots.
- **{M['modular_glb_count']} matching glTF GLB meshes**, including PBR material metadata and UV coordinates.
- One assembled showcase FBX and GLB, **{M['triangle_count']:,} modular triangles**.
- **18 shared PBR material sets / 108 PNG maps**, including albedo, normal, metal-rough-AO, Unity metallic-smoothness, occlusion, emission.
- **Two shader source files** (URP and Built-in hologram) plus PBR Unity material importer for URP/HDRP/Built-in Lit.
- **Four optional C# art helper scripts**, named pivot rig (92 pivots), 272 mesh-only socket positions.
- **194 conceptual connection records**, 388 independent logical endpoint records and 6 gameplay objective specs.
- Proposed critical **W179**: `L93_AnalyticalVacuumPressureTransducer` -> `L93_AnalyticalSafetyPLC`.

### Unity quick start

1. Import all folders into your Unity project, preferably within `Assets/ProjectSpark/Level93/`.
2. Ensure the three Editor helpers are inside an `Editor` folder (or keep their `#if UNITY_EDITOR` guards). Keep `SparkLevel93VisualRig.cs` outside Editor.
3. Choose `Tools/Project Spark/Levels/93/1. Generate HD PBR Materials And Shaders` to create actual editable `.mat` assets. This handles supported pipelines using source maps; for HDRP transparent glass settings may need a project-specific pass.
4. Select a **Level 93 layout GameObject**; choose `Tools/Project Spark/Levels/93/2. Place 100 Modular FBX Models Under Selected Layout`.
5. Choose `Tools/Project Spark/Levels/93/3. Apply Generated PBR Materials To Selected Art` and `4. Validate Imported HD Art`.
6. Add your own scene lighting, colliders, LODs, visual rigs, real SparkTerminals, science simulation and objective evaluator.

### What was intentionally not implemented

There are NO functioning player wires, physics/electrical circuits, vacuum/radiation safety, X-ray instrument mechanics, tomography reconstruction algorithms, Raman/XRF spectra, scientific evidence processor, Play Mode tested components, or 6/6 mission completion in this ZIP.

**The 272 modeled sockets are VISUAL geometry, not SparkTerminal components.** The 194 wiring rows are an illustrative engineering plan; 388 logical endpoints require an actual, independent mapping to the game's real electrical topology. Reused visual anchors and mixed signal kinds must not be blindly shorted together.

### Important asset caveats

- This is **original procedurally generated AAA-inspired technical geometry**, not authored/baked final AAA production art; visual finishing and Unity runtime tests remain necessary.
- FBX numerical vertex coordinates are in centimetres with centimetre unit metadata; GLB geometry is in metres. Compare imported room dimensions to the manifest before choosing scale settings.
- All modular meshes share a **common world origin**; place each at local (0,0,0) under the same art root to reproduce the scene. Do NOT apply the nominal world positions again (they are already baked into geometry).
- UVs are automatically generated planar per chunk (not artist-authored atlas unwraps), so some fine surfaces need UV refinements and final texture bakes.
- PBR map sources are included, and the Unity C# material installer creates editable native .mat files; native Unity material asset serialization is project/pipeline-dependent.
- Never place the assembled showcase and all 100 individual art assets in the same active scene because that duplicates all geometry.
- This package is independent of the unrelated image uploaded by accident earlier in the project.

### Directories

- `Models/` — 100 individual binary `.fbx` files + a combined showcase
- `GLB/` — matching `.glb` files + combined showcase
- `Textures/`, `Texture_Manifest.json`, `Materials/` — source PBR library
- `Shaders/` — hand-authored Unity shader source for holograms
- `Unity_Optional/` — visual-only art import, materials, validation and rig
- `Animation_Rigs.csv`, `Art_Assembly.csv`, `Component_Roles.csv`
- `Gameplay_Wiring_*`, `Terminal_*`, `Visual_Terminal_Sockets_272.csv`
- `W179_Critical_Analytical_Vacuum_Pressure_Recovery.md`
- `Objective_Certification_Plan_6.csv`, `Physical_Analytical_Process_Concept.csv`
- `Collision_LOD_Production_Plan.csv` and polish notes
- `Source/` contains the source exporters for repeatability
''')
(R/'Gameplay_Integration_Plan.md').write_text('''# Project Spark Level 93 — Unity gameplay implementation plan

This is an art-only source package. Inspect actual Project Spark classes and integrate using their real APIs, rather than assuming helper methods exist.

1. Use only meshes/materials under a dedicated `L93_HD_VISUAL_ART` root; import models at a shared origin without altering previous levels or LevelGamePlayManager.
2. Provide actual `SparkTerminal` components for the 194 planned wire routes. Reconcile 388 logical endpoints with 272 decorative visual positions. Never join different signal or power domains just because their decorative art sockets coincide.
3. Model one authoritative finite KHEPRI-7 geological core composition and internal structure. Samples consumed for mass analysis must remain consumed after retries; reserve a fresh fragment for post-repair tests.
4. Implement protected CT with realistic finite-stage movement and scan coverage; Raman/IR/XRF with calibrated and noisy spectral measurements; mass-spectrometry/thermal vacuum pressure and resource constraints; specimen custody and contamination witness.
5. Separate real-world process values, sensor readings, controller commands, safety-qualified conditions, displays and animation. A green UI indicator or successfully played animation cannot certify an objective.
6. Provide real three-fault recovery: inert purge failure, CT stage encoder slip and mass detector gain drift.
7. W179 needs actual wire topology loss, time-invalid pressure signal, protected heating/analysis shutdown, reconnection, fresh calibration, explicit safety reset and new qualification run ID.
8. Award six objectives using independently validated evidence and `LevelGamePlayManager`. Preserve levels 1–92 and do not use fake objective-completion flags.
9. Add real occlusion/colliders/LODs, audio, URP/HDRP lighting, baked occlusion and lightmaps, VFX authoring and professional art tuning.
10. Test a new Unity project import before runtime; a structural FBX/GLB check is NOT a Unity compile or Play Mode test.

### Suggested Editor command
`Tools/Project Spark/Levels/Create Level 93` — to be implemented by the gameplay team; the package only includes the `Levels/93/1...4` optional ART menus.
''')
(R/'AAA_Polish_And_Lighting_Plan.md').write_text('''# Level 93 — Professional Art Finishing Plan

## Target visual identity
Clean scientific science-fiction aesthetic: white ceramic shell, brushed titanium, sealed glass archive cylinder, gilded pressure clamps, violet spectra and cyan volumetric reconstruction. The core is visually dominant; stations line both sides with safe player circulation.

## Suggested lighting (author in Unity)
- Cinematic contrast: warm key on sample vault, cool indirect fill on instrument racks, subtle violet on Raman and XRF heads.
- Realtime area-style highlights for specimen vault and science console; prefer baked lighting for static surfaces and URP/HDRP probes for reflective metal.
- Use volumetric beams only as bounded aesthetic holograms; X-ray radiation itself is not visible.
- Controlled Bloom for cyan/violet indicators, no overexposed floor, readable status text.

## Mesh finishing
- Keep hard mechanical silhouettes; add bevel normals, realistic bolted interfaces, improved UV unwrapping/texture atlas and secondary lightmap UVs.
- Apply actual URP/HDRP glass configuration including refraction where supported.
- Improve LOD and colliders; consider transparent glass double-sorting and shadow settings.
- Use consistent texel density, trim sheets and selective decals. Keep major labels in 2D Unity TMPro for legibility.

## Engine targets
- Full physical PBR using supplied maps as a starting point, not a claim of artist-authored baking.
- All 92 named moving assemblies should be driven from actual measured state, never from objective-complete booleans.
- Include performance budget, culling, batched static props, LODs and optional URP/HDRP advanced effects.
''')
(R/'Visual_Rig_Integration_Notes.md').write_text('''# Level 93 visual-pivot integration

`SparkLevel93VisualRig` is only an animation facade. It resolves `ANIM_` pivots by name and can accept measured angles or offsets from future validated gameplay code. It does not create circuits, sample dynamics, protected instruments or objectives.

The 92 pivot records in `Animation_Rigs.csv` include parent model, nominal world coordinates and intended axis. All pivots are imported as parent nodes with independent mesh children. Authoritative runtime scripts should map actual stage position, gantry revolution, glovebox latch, sample cartridge, vacuum pump rotor and analyzer status to these art transforms.

When replacing visual art, call `Rebind()`. Never drive `LevelGamePlayManager` from this component.
''')
modelrows=[]
for m in M['models']:
 ix=m['index'];cat=m['category'];crit='High' if ix in (7,8,9,18,19,23,35,52,53,61,70,73,85,94) else 'Medium'
 modelrows.append(dict(Model=m['name'],Category=cat,Triangles=m['triangles'],Collision='Box/capsule composites with detailed surface colliders where contacts matter',
   LOD0='Use supplied imported geometry',LOD1='Reduce 45%-65% geometry while keeping mechanical silhouette',LOD2='Reduce 75%-85% or fade detail at distance',
   StaticBatching='Yes' if ix<18 else 'Selective',GameplayCritical='YES' if crit=='High' else 'NO',Implemented='PLAN_ONLY'))
with (R/'Collision_LOD_Production_Plan.csv').open('w',newline='') as f:
 w=csv.DictWriter(f,fieldnames=list(modelrows[0]));w.writeheader();w.writerows(modelrows)
process=[
('Inert archive','Pristine core and sealed glovebox','Finite core sample + inert gas condition','Custody state, transfer locks, atmosphere contamination witness','No conversion to vacuum without an actual valid transfer'),
('Tomography','Core sample in X-ray shielded stage','Stage positions, detector projections, calibration','3D reconstruction coverage and spatial uncertainty','Missing angles or encoder drift degrade accuracy'),
('Raman IR XRF','Small selected surface regions','Calibration sources + specimen regions','Raman/IR/XRF spectra with noise/confidence','Elemental peaks alone not a mineral certification'),
('Vacuum analysis','Separate micro sample heated within chamber','Finite micro sample, power, vacuum pressure','Gas release, ion detector response, thermal state','Do not consume archive main core; finite material conservation'),
('Science fusion','All independent instrument measurements','Timestamped covariance-aware evidence','Qualified cross-instrument interpretation','Display success not evidence'),
('Safety W179','Analytical pressure sensor to independent PLC','Live real feedback not decorative socket continuity','New fault, reset and postrepair test evidence','Safe shutdown when feedback goes stale')]
with (R/'Physical_Analytical_Process_Concept.csv').open('w',newline='') as f:
 w=csv.writer(f);w.writerow(['Process','Plant','Inputs','Outputs','Invariant']);w.writerows(process)
print('Created README, integration and AAA notes, process CSV, 100 collision/LOD plans')
