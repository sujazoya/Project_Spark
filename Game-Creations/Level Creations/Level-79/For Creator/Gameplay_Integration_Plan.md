# Level 79 — Unity Gameplay Integration Plan

**Scope:** true modular 3D ART ONLY; 66 FBXs/GLBs and 48 pivot parents. W001–W122 and P001–P188 files are planning guides, NOT functional Spark terminals, wires, physics, or gameplay. Unity compile/Play Mode is not tested.

## Real Project Spark integration
Inspect `SparkCircuitSystem`, `SparkTerminal`, `SparkElectricalComponent`, `SparkLevelDefinition`, `SparkLevelEvaluator`, `SparkLevelSceneBindings`, `SparkLevelWorkstationBindingBridge` and `LevelGamePlayManager` before generating gameplay. Keep electrical control power at 5 V and separate simulated industrial motor and bonded-chiplet test domains. Physical contact, robot motion, thermal integration, optical registration, material/particle state, bond front, inspection and contact graph must have authoritative models.

## Geometry and animation
Import the 66 numbered FBXs. Check combined width ~22.45 m. FBX numeric vertices are cm and FBX UnitScaleFactor=1; GLB numeric vertices are metres. Place under `L79_HD_VISUAL_ART` with common-origin 0,0,0 transforms and importer scale conversion once. Use `Animation_Rigs.csv` for actual pivot parent names, axis and 3D coordinates. Optional `SparkLevel79VisualRig` is ART ONLY; do not derive objective truth from it. The 48 named movable pivot groups are real mesh-parent nodes in both formats; no baked animation clips included.

## Six distinct processes
(1) real electrical/player wiring, (2) known-good die and clean bond-ready interface plus six-axis calibration, (3) Logic and Memory hybrid contact front and void inspection, (4) I/O+Sensor bonding, thermal consolidation and four-chip communication graph, (5) three independent assembly disturbances and fresh certification, (6) deliberate W037 feedback wire disconnect, motion safe hold, reconnect/rehome/recalibrate/reset and a fresh four-chiplet assembly to 6/6.

## Physical truth vs simulated metrology
Use six-axis pose and per-pad registration, surface readiness and particle locations, contact-front propagation and voids, thermomechanical stress, copper pad connection resistance, cross-die signal routing. Test conditions are educational normalized approximations rather than real chiplet manufacturing specs. Chiplets cannot be simply re-positioned after committed bonding; bad bonds require quarantine/replacement according to scenario rules.

## Objective history
Historical Objectives 1–5 persist while user deliberately faults W037. Reconnection alone never grants Objective 6. No simulated sensor data may bypass actual missing W037 signal.
