PROJECT SPARK — LEVEL 52 "CANCEL THE SHAKE"
POLISHED SEMI-HD FBX + GLB ART PACK | ACTIVE ROTOR BALANCING
==========================================================

DELIVERED VISUAL ASSETS
-----------------------
Models/          15 individual 3D FBX 7.4 binary files + 15 GLB backups;
                 one complete assembled FBX + one assembled GLB.
Previews/        Rendered exported triangle geometry, including hero,
                 mechanical closeup, all-model contact sheet, individual renders.
Sources/         Reproducible Python model generators and binary FBX writer.
Unity_Optional/  SparkLevel52VisualRig.cs (visual animation adapter ONLY).
Terminal_Anchors.csv           32 local modeled connector sockets.
World_Terminal_Anchors.csv     32 assembled art-reference coordinates.
Exact_Wiring.csv               19 intended player connections W01–W19.
Animation_Rigs.csv            Named FBX groups and their pivot points.
Model_Manifest.json           Detailed model list / meshes / positions / anchors.
Material_Palette.csv          Metallic color/material recommendations.
VALIDATION.txt                Programmatic export and geometry verification.

CRITICAL SCOPE
--------------
This package includes REAL MESH GEOMETRY, separate FBX hierarchies and helper
art script. It DOES NOT implement Unity gameplay, electrical components,
SparkTerminal objects, circuit graph bindings, motor physics, vibration
analysis, encoder Z validity, safety interlock, balance algorithms, or the
LevelGamePlayManager's six objectives. Do not use visual rotations to award
objectives. Those must be connected to verified Project Spark runtime systems.

QUICK UNITY IMPORT
------------------
1. Unzip into a temporary folder. Copy Models/*.fbx to e.g.
   Assets/My_Assets/_Project_Spark/Models/Level52/ in your Unity project.
2. Open Level_52_Layout or create a layout prefab with the existing level
   creation pipeline. Import INDIVIDUAL model files as visual child objects.
3. Keep FBX node hierarchy: named groups are pivots for independent animation.
4. Use 'showcase_positions' in Model_Manifest.json to reproduce the assembly.
   These are model-art LOCAL coordinates (Y-up, front = -Z), NOT exact Unity
   breadboard coordinates. Apply a single uniform scale to the assembled art.
5. Snap actual SparkTerminal components to real workbench holes using the
   existing project's terminal and shelf binding systems; the connector rings
   in these FBXs are GEOMETRY ONLY. CSV positions are placement references.
6. Assign proper Unity URP/HDRP Lit materials with metallic maps, roughness,
   glass alpha and optional emission. Material colors are in the exported
   FBXs/GLBs but appearance depends on your Unity importer and render pipeline.
7. Add TextMeshPro overlays or render textures on the dark blank screen faces.
   The procedural waveform bars on the spectrum monitor are static motifs;
   wire actual live signal graphics to real simulation measurements.
8. Copy Unity_Optional/SparkLevel52VisualRig.cs into an appropriate runtime
   script folder; assign the named FBX group Transform references in Inspector.
   The script only ANIMATES YOUR MEASURED NUMBERS and does not invent them.

15 INDIVIDUAL FBX FILES
------------------------
00_L52_IndustrialBase.fbx      Wide isolation-mounted futuristic lab deck;
                               includes GLOBAL_DRIVETRAIN_SHAFT_ROTATE_X.
01_L52_DriveMotor.fbx          Ribbed brushed-metal 5 V motor, shaft, saddles;
                               includes MOTOR_SHAFT_ROTATE_X.
02_L52_TestRotor.fbx           Machined reference rotor, 360° index markings;
                               includes TEST_ROTOR_ROTATE_X.
03_L52_ImbalanceWeight.fbx     Removable red imbalance weight/locknut/tray;
                               includes IMBALANCE_TEST_WEIGHT_DETACH.
04_L52_ActiveBalanceRing.fbx   Twin separately positioned adjustable weights;
                               includes RING_ROTOR_ROTATE_X and
                               CORRECTION_MASS_A_ORBIT_X,
                               CORRECTION_MASS_B_ORBIT_X.
05_L52_BalanceActuator.fbx     Balancing stator, electromagnetic coil and LEDs.
06_L52_EncoderABZ.fbx          80-count quadrature disk and unique Z index;
                               includes ENCODER_ABZ_DISC_ROTATE_X.
07_L52_VibrationSensor.fbx     Phase-synchronous vibration sensor module.
08_L52_MotorDriver.fbx         PWM motor drive with heatsinks and socket jacks.
09_L52_BalanceDriver.fbx       Smart dual-mass actuator driver.
10_L52_BalanceController.fbx   9-socket control panel, blank display, buttons.
11_L52_SpectrumMonitor.fbx     Large live waveform/polar-display screen area.
12_L52_SafetyGuard.fbx         Clear guard and hinged front access door;
                               includes FRONT_SAFETY_DOOR_HINGE_Y.
13_L52_ServiceTray.fbx         8 individually detachable balance weights/tools.
14_L52_5VSupply.fbx            Brushed-metal regulated lab power supply.

ANIMATION NOTES — PARTICULARLY IMPORTANT
----------------------------------------
The rotor rotates about local +X. Some example meshes are rings/discs oriented
perpendicular to the shaft. To rotate them:
- MOTOR_SHAFT_ROTATE_X
- GLOBAL_DRIVETRAIN_SHAFT_ROTATE_X
- TEST_ROTOR_ROTATE_X
- RING_ROTOR_ROTATE_X
- ENCODER_ABZ_DISC_ROTATE_X
Each of these groups is independent. Drive all from externally observed RPM.

Correction weights were modeled at initial angles A=45° and B=225° from the
ring's local +Y direction toward +Z (looking along the shaft axis). Each
correction weight group has its own centerline +X pivot and is a SIBLING of
the ring rotation group in this exported FBX. Therefore apply shaft rotational
phase PLUS the actuator-relative angular offset to BOTH group rotations.
Example visual-only target:
  A display rotation = ShaftPhase + ActualMassAAngle - 45 degrees
  B display rotation = ShaftPhase + ActualMassBAngle - 225 degrees
Do not rotate a correction group independently without shaft phase, or the
weight will appear to stop spinning while the ring rotates. The included
SparkLevel52VisualRig.cs handles the visual composition automatically.

The movable red fault weight is a separate art model; when installing it,
use a separate child/instance of its IMBALANCE_TEST_WEIGHT_DETACH group at
the proper rotor rim position. Do not drag its complete storage plinth into
the spinning rotor. If your gameplay system supports attaching/detaching
components, make the mesh follow that system's authoritative installation.

The beam/drive axles in the assembled art are visual references only. Since
independent FBX modules each have their own foundation, turn off/simplify
unwanted support plinths or use a shared deck if your specific scene needs it.

PLAYER WIRING — 19 WIRES, 32 UNIQUE ART SOCKETS
------------------------------------------------
W01 Supply +5V           -> Balance Controller +5V
W02 Supply RETURN        -> Controller GND
W03 Supply +5V           -> Motor Driver +5V
W04 Supply RETURN        -> Motor Driver GND
W05 Supply +5V           -> Vibration Sensor +5V
W06 Supply RETURN        -> Vibration Sensor GND
W07 Supply +5V           -> Balance Driver +5V
W08 Supply RETURN        -> Balance Driver GND
W09 Controller ROTOR PWM -> Motor Driver PWM
W10 Motor Driver M+      -> DC Motor +
W11 Motor Driver M-      -> DC Motor -
W12 Vibration OUT        -> Controller VIB IN
W13 Encoder A            -> Controller ENC A
W14 Encoder B            -> Controller ENC B
W15 Guard NC OUT         -> Controller SAFE
W16 Controller BAL CMD   -> Balance Driver COMMAND
W17 Balance Driver OUT+  -> Balance Ring POWER+
W18 Balance Driver OUT-  -> Balance Ring POWER-
W19 Encoder Z            -> Controller ENC Z

Supply + and RETURN are each used by four wires; they must accept multiple
connections through your real electrical wiring topology. 38 wire endpoints
minus six reused supply endpoints = exactly 32 distinct modeled sockets.
Install W19 last so Undo Last removes the Z signal for the objective-6 test.

The SMART BALANCE DRIVER is conceptually a powered module carrying separate
commands to its two internal mass motors, NOT a passive resistor. Use the
actual signal architecture in your Project Spark project for BAL CMD, encoder
A/B/Z, PWM and guard signals. No arbitrary solver voltage is generated here.

RECOMMENDED SIX OBJECTIVES (DESIGN TARGETS, NOT IMPLEMENTED)
------------------------------------------------------------
1/6 Wire all components and obtain real safe supply/circuit status.
2/6 Record true 45 RPM unbalanced vibration over several revolutions.
3/6 Determine correction phase from measured vibration and calibration test.
4/6 Correct phase and amplitude; residual vibration <=30% of baseline.
5/6 Ramp to 60 RPM; verify steady low vibration over four revolutions.
6/6 Disconnect and restore W19 (Z), verify safe fault, reacquire phase,
    reset explicitly, repeat valid balancing and reach authoritative 6/6.
Tolerances must be calibrated against actual measured mechanical state.

TEXT / MATERIALS / PERFORMANCE
------------------------------
Use TextMeshPro for TARGET RPM, ACTUAL RPM, VIBRATION RMS, PHASE ANGLE,
MASS A ANGLE, MASS B ANGLE, RESIDUAL VECTOR, GUARD STATUS, OBJECTIVES X/6.
Do NOT use painted static waveform meshes as numeric gameplay information.
FBX meshes have physically separated parts but final collider/LOD authoring
belongs in Unity. Use simplified colliders (box/capsule) for interaction and
avoid mesh colliders on each of the numerous decorative fasteners.

VALIDATION
----------
The Python builder tests FBX binary headers and internal node pointers,
loads each GLB as actual 3D mesh geometry, validates 15 models / 32 unique
socket IDs / 19 wires, and creates the assembled FBX. Preview images come
from rendering the same procedural triangle geometry, not from image prompts.
This is not a claim of testing the FBXs in Unity, shader compatibility or
compiling a gameplay level. Those require your Project Spark project.
