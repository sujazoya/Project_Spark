# Project Spark — Level 95: Pulse Architect — Board-Only Realistic FBX Rebuild

This kit is exclusively the **120 × 80 mm PCB and its component system**. It contains **no game room, desk, chairs, 3D measuring instruments, physical repair tools, or UI panels**. Use your separate World-Space Canvas UI for multimeter, scope, microscope, soldering, etc.

## Correct gameplay start
- Import all `Models_FBX/` models as independent art assets.
- Select a scene/layout art root, run **Tools > Project Spark > Levels > 95 > Board Only > 1. Build Empty Precision PCB**.
- 36 electronic components start uninstalled, W001–W048 must be player-routed, 10 vias must be placed, and W049 starts disconnected.
- A `L95_COMPLETE_BOARD_REFERENCE_VISUAL_ONLY.fbx` is supplied for art review only; its Unity builder creates it inactive.
- `FactoryOnly` silkscreen/traces are purely decorative. Never register them as player routes or as electrical connectivity.

## Models / dimensions
- Modeling coordinates are meters. PCB physical size: **0.120 m X 0.080 m** (FR-4 thickness 1.56 mm approx.) and lies on the XZ plane with Y up.
- Component footprints are stored as X/Z millimeters in `Data/Component_Placement_36.csv`; place models at (Xmm*0.001,0,Zmm*0.001) relative to PCB origin. Each prefab/model uses a local origin for reusable positioning.
- The artwork supports optional **12x visual scale** in the giant table preview. NEVER apply this to the electrical dimensions, simulation, component values or gameplay physics.
- `L95_W049_HeaderBase` is installed visual hardware. W049 removable jumper is a separate component, and must start disconnected.

## Materials and shaders
- Each mesh has physically distinct material slots for FR-4, mask, ENIG gold, solder, plastic and ceramic. Generic PBR albedo/normal/metallic-roughness maps and a high-resolution silk-screen texture are included.
- Run **4. Create Native PBR Materials** for URP/HDRP/Built-in materials. Material mapping and built-in lighting still require a Unity-side pass.
- For realistic board markings use `L95_Board_Top_SilkScreen_FactoryOnly.png` as a top decal/base color, not as a replacement for user-created copper geometry.
- Some chip-top laser ID decals are in `L95_IC_LaserEtch_Atlas.png`; source text and exact IC values are in the component CSV.

## Safety and authenticity
- Do not use this art to claim successful PCB wiring, electrical accuracy, running firmware or objective completion.
- Reconcile `Data/Circuit_Graph_Contract.json`, copper route table, 10 via IDs and W049 in the existing SparkElectricalSolver / PCB netlist system.
- Level 95's design keeps RV1 as PWM duty trim; master clock frequency comes from RC resistor/capacitor selection.
- Procedural models are editable premium-inspired 3D starting assets. Artist refinements, real pin pitch and manufacturer footprint checks, proper drilled geometry, atlas UV refinement, collisions, LOD and Unity Play Mode verification remain necessary.

## Preferred material setup
Run **Tools > Project Spark > Levels > 95 > Board Only > 5. Generate ALL Materials + Remap FBX Slots** for full material names including the PCB silk-screen decal and chip atlas. (Tool 4 creates only six generic families.) Actual shader compilation is project/version dependent and is not verified offline.
