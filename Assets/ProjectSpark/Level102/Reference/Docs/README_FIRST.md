# PROJECT SPARK — LEVEL 95: PULSE ARCHITECT

**Original procedural AAA-inspired 3D artwork, source maps and PCB planning kit — not a playable level.**

## Contents
- `Models/`: 54 separate binary FBX 7400 models + one `L95_PulseArchitect_CompleteShowcase.fbx`.
- `GLB/`: 54 corresponding glTF 2.0 GLBs + one assembled GLB reference.
- `Textures/`: 21 shared PBR texture families × 6 maps = 126 maps, 1024px each.
- `Materials/`: source material names and PBR texture manifest.
- `Shaders/`: Built-in and URP original ShaderLab/HLSL hologram shaders; HDRP supported through native Lit materials.
- `Unity/Editor/`: native material installer, FBX assembler, planning-data importer and art/data validator.
- `Unity/`: optional board visual controller and data-only ScriptableObject.
- `Data/`: board physical data, BOM36, logical pad map, 48 copper tasks, ten vias, W049, objectives, 3 faults, factory branches and 72 planned acceptance tests.
- `Source/`: reproducible Python mesh, texture, data and packaging generators.

## Unity import
1. Extract this top-level folder into `Assets/ProjectSpark/Level95/` (ensure assets remain beneath an `Assets/` folder).
2. Keep `Unity/Editor/*.cs` in a folder literally named `Editor`. Keep runtime `.cs` outside the Editor folder.
3. Use menu **Tools > Project Spark > Levels > 95 > 1. Create Native PBR Materials**.
4. Select a Level 95 layout GameObject and use **2. Assemble 54 Modular FBXs Under Selected Layout**.
5. Optionally run **3. Apply PBR To Selected Art**, **4. Create PCB Planning Data Asset** and **5. Validate Imported Assets And Plans**.
6. For Project Spark's preexisting futuristic room/table, hide the optional `00_Bench` group and use the `01_BareBoard`, `02_TestInstruments`, and `03_Components_36` art roots.
7. In normal gameplay, the 36 components begin not installed and the artwork-only assembler hides them until a real placement system is connected. For visual review, use the visual controller's `ShowAssembledBoardPreview(true)` or the reference assembled model separately.

## Circuit task summary
- 120×80×1.6 mm; 2-layer board, visually presented 12× larger.
- 36 player-installed electronic parts.
- W001-W048 are **unrouted** at level start; ten required vias **missing**.
- W049 is one **separate removable safety feedback jumper**, initially uninstalled.
- Nominal protected 5 V and regulated monitoring 3.3 V, 1024 Hz master, 256 Hz and 2 Hz divided, 256 Hz PWM with 25/50/75% duty.
- Three fault scenarios: C1 wrong value, V06 open, Q1 gate-drain solder bridge.
- All six gameplay objectives require later integration with actual Project Spark electrical/interaction/level systems.

## Disclaimer
These are **real editable mesh assets**, not image illustrations or a promised full playable game. Normalized procedural UV mapping and PBR reference materials are included, not film-grade artist-authored UV atlases or baked high-poly details. Actual Unity shader compilation, materials preview, collision meshes, runtime board simulation, traces, authentic component pinouts and LevelGamePlayManager tests remain to be performed.
