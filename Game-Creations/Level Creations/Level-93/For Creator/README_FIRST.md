# PROJECT SPARK — LEVEL 93: THE ELEMENTAL ARCHIVE

## ACTUAL MODULAR 3D ASSET PACKAGE — ART, NOT A COMPLETED GAME LEVEL

- **100 individually importable binary FBX 7400 files**, with UV and vertex-normal layers, named PBR material slots and independent named animation pivots.
- **100 matching glTF GLB meshes**, including PBR material metadata and UV coordinates.
- One assembled showcase FBX and GLB, **194,976 modular triangles**.
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
