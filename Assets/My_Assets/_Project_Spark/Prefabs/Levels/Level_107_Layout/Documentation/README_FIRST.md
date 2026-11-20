# PROJECT SPARK — LEVEL 107: PHOENIX PROTOCOL
## PFX-15 realistic-style board-only model package

**WHAT THIS IS:** A Level 107 art and engineering-planning package. It is NOT a production-proven electrical circuit, gameplay implementation, commercial motherboard, or hand-finished AAA model set.

### Content
- 144 modular FBX models and 144 equivalent GLB previews.
- 112 independent, installable electronic components and 32 board-only structural/repair assets.
- Separate assembled visual reference GLB and FBX.
- Starter PBR maps, including a new PFX-15 2K firmware silkscreen.
- Component placement, proposed BOM, 148 unassigned route tasks, 46 unassigned via tasks, 3 fault definitions, 6 objectives and W149 recovery requirements.
- Unity Editor scripts for an art-only empty board and starter materials.

### Source/art lineage and accuracy
This package extends the supplied Project Spark Level 106 SFX-14 procedural mesh library. Compatible common CPU, VRM, DDR, connectors and passives were reused; six additional firmware/control component meshes and two board-only reference meshes were derived from existing electronic package geometry. The PFX-15-specific component manifest, dual-flash assignment, firmware-zone layout, new silkscreen, recovery data, W149 data and composed GLB reference are authored for Level 107.

FBX models are source-library binary FBX geometry, not newly sculpted artist-unique assets; the preview reference FBX retains reused geometry. The assembled GLB includes additional colored firmware-region indication and a modest XY scale adjustment, and may not exactly match the FBX reference. Import settings, component pivots, footprint accuracy and production materials must be verified in Unity.

### IMPORTANT: use source model dimensions correctly
The underlying inherited bare-board art measures 285 x 205 mm. The Level 107 target is 290 x 210 mm. The Unity Art Builder applies an XY footprint adjustment to the bare-board presentation. Reconcile source axes/scale and exact pad coordinates before a functional board prefab is built.

### Safety and electrical authority
- `Routes_148_PLANNING.csv` endpoints are UNASSIGNED and NOT_CERTIFIABLE.
- `Vias_46_PLANNING.csv` net identities and layer spans are UNASSIGNED and NOT_CERTIFIABLE.
- W149 physical pins and supervisor component assignments are NOT CERTIFIED.
- No real firmware authentication or update execution is included.
- The completed reference is artwork ONLY. Never convert it directly to gameplay starting state.
- A genuine real-time electrical topology and gameplay authority must be implemented separately and tested.

### Unity setup
1. Extract directory into `Assets/ProjectSpark/Level107/`.
2. Ensure Editor scripts live under `/Unity/Editor/`.
3. Open `Tools/Project Spark/Levels/107/1. Build EMPTY PFX-15 Board`.
4. Run `Tools/Project Spark/Levels/107/3. Create PBR Starter Materials`.
5. Assign and validate mesh materials, UVs and transforms.
6. Build true electrical netlist, assembly and routes using actual project gameplay APIs.

Initial playable conditions: 0/112 components, 0/148 routes, 0/46 vias, W149 DISCONNECTED, objectives 0/6.

### Level 107 recovery faults
- F1: U6 primary SPI chip-select contact
- F2: corrupted primary image
- F3: recovery request input/pull-up fault

### W149 recovery
Disconnect, independently inhibit, physically reconnect, reauthenticate firmware, fresh POST and boot, explicit safety reset, independent qualification.

**Do not claim AAA completion or Unity compatibility without actual engine import/Play Mode review.**
