# Project Spark Level 83 — Unity Gameplay Integration Plan

## Existing package
Original 74 modular FBX+GLB pairs + assembled scene, 56 real ANIM parent pivots, material sets, 212 gold-rimmed art socket meshes, Unity art-only Editor and visual rig helpers. `Model_Manifest.json` contains exact names and origin coordinates.

## Architectural requirements
1. Inspect existing SparkElectricalSolver, SparkCircuitSystem, SparkTerminal, SparkLevelDefinition and LevelGamePlayManager APIs. **Do not invent Project Spark APIs.**
2. Build exactly 138 actual player-created wires using `Gameplay_Wiring_Concept_W001_W138.csv`. Reconcile socket reuse: a single visual anchor can host multiple compatible logical connections only through legitimate electrical junctions or a clearly separated physical socket/port. No cross-domain shorts. Build 212 functional SparkTerminal interfaces compatible with the 138 electrical connection definitions.
3. Implement separate electrical control signal (5V), fictional spacecraft energy/power, and independent safety domains. No free energy. Real dynamic PV generation must depend on sunlight, incidence and panel/MPPT state; batteries must use power/time integrators, thermal/current limits and SOC boundaries; bus voltage and load shed reflect stored energy and converter dynamics.
4. Three eclipse-cycle phases, mission task deadlines, optical communication load schedule, gimbal jam, battery cell imbalance, transmitter sudden demand.
5. W104 real physical disconnect: trusted bus voltage stale, power-safe fallback, independent undervoltage protection, latch, reconnection, calibration, explicit reset, and **fresh complete orbital cycle**. Never bypass with display telemetry. Objectives 1–5 remain historically complete.
6. All six objectives certified through actual LevelGamePlayManager. Do not award PASS based on animations.

## Unity art assembly
- Import all 74 numbered FBXs with centimeter-to-meter conversion **once**. All modular model transforms identity at common origin under `L83_HD_VISUAL_ART`. Do not simultaneously instantiate assembled showcase.
- Run art placement, material upgrade, validator from `Tools/Project Spark/Levels/83/...`.
- Attach SparkLevel83VisualRig to common art root and call actual-state setter functions. It only renders; no physics/wiring/safety/certification.

## Fidelity limits
The orbital visuals and physics are educational game abstractions. Actual spacecraft flight qualification and battery safety are not claimed. Unity compile/Play Mode not tested.
