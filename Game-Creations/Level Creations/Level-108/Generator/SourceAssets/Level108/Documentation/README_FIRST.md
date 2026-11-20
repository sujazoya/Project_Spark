# PROJECT SPARK — LEVEL 108: GHOST IN THE CIRCUIT
## DFX-16 Diagnostic Fusion — Board-Only Procedural Asset Kit

This package provides **real mesh files**, not another image: 152 separate FBX assets + corresponding GLBs, 118 individually placeable components, 34 structural/diagnostic assets, and one assembled-reference asset in each format.

### Contents
- `Models_FBX/`: separate board and electronic models; one assembled FBX visual reference.
- `Models_GLB/`: matching GLBs and a composed assembled scene with three diagnostic indicators.
- `Textures/`: inherited basic PBR texture references plus new Level 108 2K silkscreen.
- `Data/`: 118 part placements, 156 planned routes, 50 vias, 3 faults, 6 objectives and W157 feedback.
- `Unity/Editor/`: an empty-board art builder and starter material generator.

### Scope and quality
This asset kit **adapts the procedural 3D mesh library from Level 107**. The shared CPU, RAM, regulator, connector and passive geometries are reused under Level 108 asset identities. Additional diagnostics and fault-overlay objects are created from compatible existing meshes; they have not been re-sculpted into unique manufacturer-accurate forms. The GLB assembled reference includes colored diagnostic markers. It is a layout reference, not the completed playable state. The FBX reference retains its inherited source board composition and will require adjustment to exactly match the Level 108 reference GLB and plan.

This is a reusable **realistic-style production foundation**, not hand-finished AAA artwork. Models require final art direction, pivots, UVs, materials, manufacturer footprints and Unity import verification.

### Target PCB size
DFX-16 design target = **300 x 220 mm**, ten layers. The inherited bare-board mesh is approximately **285 x 205 mm**; Unity empty-board art builder applies a display scale adjustment. Component, testpad and copper positions must be reconciled against the actual modified mesh before meaningful use. Model source axes and Unity scale may need manual correction.

### Electrical validity
- The **156 routes** are only named planning tasks; ALL pins/nets/layers are UNASSIGNED and `NOT_CERTIFIABLE`.
- The **50 vias** lack physical layer spans and net assignments and are `NOT_CERTIFIABLE`.
- W157's logical U16-U17 path is defined, but exact manufacturer components and physical pins remain UNASSIGNED.
- Neither a functional power solver nor thermal memory/clock faults nor a real diagnostic engine is included.
- All art models must remain separate from gameplay authority. Never auto-complete objectives from visual mesh presence.

### Unity import
1. Extract package under `Assets/ProjectSpark/Level108/` preserving the folder structure.
2. Let Unity import the FBX files and inspect scale/axes and generated materials.
3. Run `Tools/Project Spark/Levels/108/1. Build EMPTY DFX-16 Board`.
4. Optionally run `Tools/Project Spark/Levels/108/2. Generate PBR Starter Materials`.
5. Manually apply texture maps and validate the assembled reference in your target render pipeline.
6. Create verified pin-level schematic and integrate with existing Project Spark PCB repair, electrical graph and LevelGamePlayManager.

### Start of gameplay
0/118 installed, 0/156 routes, 0/50 vias, W157 disconnected, 0/6 objectives.

### Fault definitions
- F1: leaky standby ceramic capacitor resulting in current limiting and no-power behavior.
- F2: U3 DDR4 BGA contact opens as the simulated board warms.
- F3: PCIe reference-clock contact intermittently fails, degrading link reliability.

**No rooms, furniture, or physical diagnostic instruments are included. Virtual instruments remain World-Space Canvas UI.**
