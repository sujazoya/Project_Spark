# Project Spark — Level 103: THE POWER CORE

REAL EDITABLE BOARD-ONLY FBX/GLB ASSETS. No UI screenshots or rooms.

- 112 modular FBX + 112 GLB, with one assembled FBX and GLB reference.
- 88 installable components + 24 board detail models, including an independent CPU socket/load mesh.
- 240 x 180 mm, eight copper layers (reference visualization).
- 116 copper routes and 30 via tasks are UNASSIGNED planning records and NOT a verified schematic.
- W117 is a removable logical VCORE-good jumper, not CPU current path.
- Component FBXs have local geometry, manifest specifies design positions in meters; assembly reference places them.
- FBX geometry uses centimeters (FBX UnitScaleFactor 1); GLB in meters. Verify Unity import scale (the PCB should be 0.240 x 0.180 m).
- This kit uses procedural geometry and starter PBR maps; artist-finished AAA baking/LOD and Unity validation are not claimed.

## Install
Copy this root to Unity Assets. Run Tools > Project Spark > Levels > 103 > 1. Build EMPTY CPX-11 Board. Then 2. Create Starter Materials.

Do not award any objective from showcase visuals. Verify pins/netlist before running electric solver or simulated VRM.
