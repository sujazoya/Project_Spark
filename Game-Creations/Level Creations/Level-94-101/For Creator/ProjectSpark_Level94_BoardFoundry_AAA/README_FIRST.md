# PROJECT SPARK — LEVEL 94: THE BOARD FOUNDRY
## Complete procedural 3D art & board-engineering implementation kit

**This archive contains actual editable modular 3D models, not a presentation image.** The PCB is modeled and exported with real geometry, UV coordinates, normals, material slot names and correctly named placement pivots. All source and Unity integration scripts are included. It is a procedural **AAA-inspired production foundation**, not a claim of a finished artist-authored AAA level or tested electrical game.

### Contents

- **48 original modular binary FBX 7400 models** (`Models/00_...fbx` through `Models/47_...fbx`), plus `L94_BoardFoundry_CompleteShowcase.fbx`.
- **48 matching glTF GLB 2.0 models**, plus one assembled GLB. Individual GLBs have real mesh geometry, local normals, generated UVs and named PBR materials.
- **20 PBR material sets** / **120 source PNG textures**, 1024×1024: BaseColor, Normal, Metallic-Roughness-AO, UnityMetallicSmoothness/AO mask, Occlusion, Emission.
- **2 original holographic interface shaders**, for URP and Built-in (HDRP uses a native shader fallback).
- **6 Unity C# scripts**, four Editor-only and two runtime/data components, for native materials, art assembly, art inspection and CSV planning asset generation.
- **27 separate installable component models** (U1–U4, Q1, D1, three LEDs, R1–R8, C1–C7, reset switch, power connector and programming header).
- Soldermask, FR-4 body, independent pad arrays, silkscreen, mechanical clamp equipment and a **removable W035 service jumper** as separate modular objects.
- **34 initially missing player-routing tasks**, **6 initially missing via tasks** and **127 physical pad/terminal planning anchors**. These are data specifications, **not already-connected circuit edges**.
- Six evidence-based gameplay objective definitions, three distinct failure scenarios and **56 proposed Unity engineering acceptance tests** (not executed).
- Complete source code for regenerating geometry, textures, wiring data and Unity helpers.

### Workspace and scale

The board's **physical reference size is 100×70×1.6 mm**. The FBX art intentionally uses a **14× presentation scale** so it is inspectable on Project Spark's large futuristic table. This changes **only visual presentation**, not engineering dimensions, electrical parameters or gameplay distances. The board center and instruments share a common room-space origin, so import every modular FBX at `(0,0,0)` with identity rotation and unit scale.

The package has its own compact instrument bench, but the models can also be used on the existing Project Spark room table. In the room's visual-only preview mode, use the supplied assembled visual FBX; do not clone level solvers, objectives, SparkTerminal objects, or gameplay prefabs into the room-preview root.

### Import into Unity

1. Extract `ProjectSpark_Level94_BoardFoundry_AAA/` into `Assets/ProjectSpark/Level94/` **without flattening the folder layout**. For example: `Assets/ProjectSpark/Level94/Models/` and `/Textures/`.
2. Put **Editor-only C# scripts** (`SparkLevel94PBRMaterialEditor.cs`, `SparkLevel94ArtAssembler.cs`, `SparkLevel94PlanningDataEditor.cs`, `SparkLevel94ArtValidator.cs`) into `Assets/ProjectSpark/Level94/Editor/`. Leave `SparkLevel94PCBDefinition.cs` and `SparkLevel94BoardVisualController.cs` outside `Editor/`.
3. Import FBX models and textures, and import the `Shaders/` files in a normal Unity shader folder.
4. Open `Tools / Project Spark / Levels / 94 / 1. Create Native PBR Materials`. This generates actual editable Unity `.mat` files in `Assets/ProjectSpark/Level94/GeneratedMaterials/`, choosing URP Lit, HDRP Lit or Built-in Standard based on the project.
5. Make an empty Level 94 layout root and select it. Open `Tools / Project Spark / Levels / 94 / 2. Assemble 48 Modular FBXs Under Selected Layout`. Art files use the same origin and will be grouped into bench, board, 27 components, and test instruments.
6. Run menu **3. Apply PBR To Selected Art**, if material slots need a refresh.
7. Run menu **4. Create PCB Planning Data Asset**. This uses imported CSV files to create a `SparkLevel94PCBDefinition` ScriptableObject for the team to wire into existing project systems.
8. Run menu **5. Validate Imported Assets And Plans**. This checks art/source presence, **not** circuit correctness or Unity Play Mode.

**Do not place all the FBX and GLB counterparts in the same scene**; they are alternative formats, not additional objects.

### Gameplay implementation requirements

The FBX art is shown with the parts at their *target footprint locations* as a showcase. **The actual gameplay begins with the 27 parts uninstalled and 34 traces plus 6 vias missing.** The game must use the source `Component_BOM_27.csv`, `Copper_Trace_Tasks_W001_W034.csv`, `Required_Via_Placements_V01_V06.csv`, `Logical_Pad_Locations.csv` and `Factory_Existing_Nets.csv` to create real active component/trace/terminal state.

The `W035` removable service jumper physically separates `U4.CURRENT_OK_OUT` from `U1.CURRENT_OK_IN` via `J3.A`/`J3.B`. Removal must open an authoritative electrical connection, invalidate trusted load feedback, inhibit the Q1 load driver and latch a fault. Repair requires a new measurement, explicit reset and a new full board qualification. No visual-only animation, popup or shader may award LevelGamePlayManager 6/6.

### Shader/material details

- PBR albedo uses sRGB BaseColor textures and white material tint.
- `*_UnityMaskMap.png` channels: **R=metallic, G=occlusion, A=smoothness**.
- `*_MetalRoughAO.png` channels: **R=metallic, G=roughness, B=occlusion**.
- `*_Normal.png`: original procedural surface normal references; imported as **Normal Map** by the supplied Unity Editor script.
- `*_Emission.png`: source emission references for LEDs, SCADA and accent lights. For believable visual quality, use project-specific exposure and bloom without blowing out silkscreen/readability.
- The hologram shaders are **optional**. HDRP uses native HDRP/Unlit fallback for the visual hologram material. Shader compilation must be checked in the actual Unity pipeline.

### Current verification and limits

Offline structural checks and archive-integrity tests are reported in `Reports/`. Neither Unity Editor nor a real Project Spark gameplay build was used in the offline exporter.

**Not included as completed systems:** live SparkTerminal topology, integrated PCB electrical solver, solder/trace editing tools, physically measured waveforms, ObjectiveTracker/LevelGamePlayManager wiring, full production hand-authored UV atlas, final collision meshes, LODs, Unity shader compilation and Play Mode verification.

The package is deliberately organized so future PCB mastery levels can reuse the same component/footprint and shader conventions.
