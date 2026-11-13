PROJECT SPARK — LEVEL 54 "CROSS THE CRITICAL"
VISUAL AAA-INSPIRED SEMI-HD MODEL COLLECTION

WHAT'S IN THE PACKAGE
---------------------
Models/              17 individual FBX (Autodesk FBX 7.4 binary), plus a full assembled showcase FBX.
GLB/                 Matching GLB preview/backup mesh exports.
Previews/            Rendered assembly, detail view, top-down view and a 17-model contact sheet.
Unity_Optional/      Visual-only C# adapter, optional one-click Editor model placement, and a setup guide.
Source/              Reproducible Python procedural mesh builder, FBX exporter and preview renderer.
Terminal_Anchors.csv All 36 unique socket anchor transforms; local + assembled coordinates.
Exact_Wiring.csv     W01-W22 exact proposed wiring map.
Animation_Rigs.csv   Named animation pivot transforms and driver inputs.
Model_Manifest.json  Paths, part counts, triangle counts, pivots and socket IDs.

17 MODELS
---------
00 IndustrialBase          09 ActiveDamper
01 DriveMotor              10 DamperDriver
02 FlexibleShaft           11 MotorDriver
03 TestRotor               12 CriticalSpeedController
04 LeftFlexureBearing      13 OrbitSpectrumMonitor
05 RightFlexureBearing     14 SafetyGuard
06 DisplacementProbeX      15 SensorCalibrationStand
07 DisplacementProbeY      16 5VSupply
08 EncoderABZ

PROJECT SPARK ART INTEGRATION
-----------------------------
1. Import the individual FBX models into Unity.
2. Select the Level 54 Layout, then run Tools > Project Spark > Levels > 54 > Place AAA Visual Models.
3. Copy SparkLevel54VisualRig.cs into runtime Scripts and add it to L54_VisualArt.
4. Resolve/assign the pivots and connect ApplyState() to your existing genuine mechanical simulation.
5. Create real SparkTerminal components and register IDs using your SparkLevelSceneBindings implementation.
   The socket meshes/empties in the FBX are artist anchors only, not electrical objects.
6. Configure actual electrical and digital signal paths for W01–W22 from Exact_Wiring.csv.
7. Integrate the six staged objectives with the authoritative SparkLevelEvaluator and LevelGamePlayManager.
8. Apply transparent rendering to the guard glass, and add live TextMeshPro to the blank screens.
9. Test in Unity Play Mode at 45, ~60 and 75 RPM plus the deliberate Y sensor disconnection.

QUALITY / SCOPE
---------------
These are polygonal 3D art assets created for a semi-HD stylized industrial game. Named
shaft rotation, rotor, bearing, probe, damper, and safety door nodes are separate.

No Blender/Unity compilation was performed here. Structural FBX and GLB validation is reported
only if the final integrity report passes. AAA-inspired appearance is an art direction, not a
claim that a production-ready Unity level is already implemented.
