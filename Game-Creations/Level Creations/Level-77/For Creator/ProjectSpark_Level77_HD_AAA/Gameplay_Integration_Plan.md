# PROJECT SPARK — LEVEL 77: REVEAL THE CIRCUIT

## Source of truth and scope

This pack contains **real procedural 3D mesh art** only: 62 individual binary FBX files, 62 corresponding GLB backups, and assembled showroom versions in both formats. 44 named pivot parent groups are embedded in the real binary FBX and GLB hierarchies. No Level 77 circuit, CMP physics, playable PLC, `SparkTerminal`, player wires, or actual objectives are included.

## Critical machine systems to build in the Unity project

1. **Inspect existing Project Spark code** before writing against `SparkElectricalSolver`, `SparkCircuitSystem`, `SparkElectricalComponent`, `SparkTerminal`, `SparkLevelDefinition`, `SparkLevelEvaluator`, `LevelGamePlayManager`, shelf and layout loader APIs.
2. Import 62 numbered FBXs as one common-origin modular installation. Use `L77_HD_VISUAL_ART` as visual root. FBX numeric units are centimetres, UnitScaleFactor=1; GLB numeric units are metres. Check combined width approximately 20.4 m. Do not duplicate the assembled showcase.
3. Create **114 real player-created circuit wires**, indexed W001–W114, and **176 registered functional terminal GameObjects**. The included CSVs are design guides only. Shared terminals, signal and safety domains must be engineered using real circuit API; non-electrical process-fluid relationships must not be turned into conductive gameplay wires.
4. Build three independently controlled CMP stations: A bulk copper, B soft landing, C selective barrier clear. Model actual motor RPM, downforce, contact and pad states, slurry/finishing-fluid flow, wafer transfer, measurement validity and safety.
5. Build a wafer with 16 regional material maps and multiple representative copper trenches/vias; material fields track copper, barrier/liner, dielectric, copper overburden, copper dishing, dielectric erosion, residual metal bridges, corrosion and local defects. Use physically inspired, bounded material-selective rates. No instant perfect wafer.
6. Implement real sensor-derived endpoints: eddy-current copper thickness, optical bulk clearance, dual-wavelength barrier endpoint, motor torque, optical viewport health; measurement timestamps, staleness, calibration and invalidation.
7. Implement post-CMP brush/megasonic/rinse/dry processes, corrosion protection and electrical metrology: via Kelvin continuity, interline leakage isolation, dielectric integrity. Unpolished blanket copper from Level 76 must be removed from dielectric field regions; preserve filled vias and lines.
8. Industrial disturbances: optical viewport fouling; selectivity drift; post-CMP corrosion. Each changes actual process behavior and requires repair/recalibration and a new certified wafer.
9. Final real player wire **W055**: `L77_DualWavelengthBarrierEndpointSpectrometer` → `L77_ThreeStageCMPProcessController`, conceptual signal `CRITICAL barrier optical endpoint spectral feedback`. On physical wire removal, invalidate optical measurement (not just its indicator), enter safe stop, decelerate platens, release carrier pressure, lift/retract wafer, latch fault, require wire reconnection, fresh optical calibration, explicit reset and a *new* fully measured quality wafer.
10. Integrate six authoritative objectives through real `SparkLevelEvaluator` / `LevelGamePlayManager`: (1) 114 wires/176 terminals, (2) three CMP stations commissioning, (3) bulk copper removal + soft landing, (4) selective barrier clear + isolation metrology, (5) three disturbances recovery + new wafer, (6) real W055 fault recovery + new wafer. Objectives 1–5 remain historically certified during deliberate W055 disconnection, final progress **6/6**.

## Design quality targets (fictional teaching scenario)

- Residual blanket copper below the calibrated clearance threshold.
- Exposed-field barrier residue below the calibrated threshold.
- Copper dishing ≤5 nm, dielectric erosion ≤4 nm; wafer nonuniformity ≤2.5%.
- Continuity passes for required vias; interline leakage/isolation passes across adjacent lines.
- Surface corrosion and particles within configured limits.

## AAA visual art status

The included geometry is original procedural HD/AAA-inspired modular art with material-region colors. It is **not** final artist-authored production PBR, baked animations, LODs, UV/texture atlas optimization, a functioning chemical recipe or a Unity-tested level. The visual rig is an optional scene adapter that should only consume authoritative simulation state. Preview colors and glass alpha require pipeline-specific inspection.
