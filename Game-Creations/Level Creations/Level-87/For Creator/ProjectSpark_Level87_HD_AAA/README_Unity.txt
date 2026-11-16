PROJECT SPARK — LEVEL 87: THE GRAVITY ENGINE

ACTUAL 3D ART, not a finished playable Unity level.
86 separate real binary FBX 7400 meshes + 86 matching glTF2 GLB meshes.
1 assembled FBX and GLB alternative reference — do not instantiate with modular set.
68 named mesh-owning ANIM_* pivot parents, 236 visual SOCKET_P### geometry parent nodes.
158 planned W001..W158 conceptual wire specifications and 316 independent logical port identities.
96 reused socket anchors documented; incompatible electrical domains MUST NOT be shorted.
12 procedural material sets, 36 reference PNG maps (UV unwrap and proper shader-channel adaptation still needed).

IMPORT
1. Extract archive and copy Models/ into Assets/ProjectSpark/Level87/Models.
2. Import Textures/ into Assets/ProjectSpark/Level87/Textures. Place optional *Editor.cs in Unity Editor directory.
3. Put SparkLevel87VisualRig.cs outside Editor assemblies, under runtime scripts.
4. Select Level 87 layout root GameObject in Unity Hierarchy.
5. Tools/Project Spark/Levels/87/Place HD Art Under Selected Layout.
6. Tools/Project Spark/Levels/87/Validate Imported HD Art.
7. Optionally Tools/Project Spark/Levels/87/Create & Apply HD Materials To Selected Art.
8. Implement actual electrical, solver, rotor, bearing, safety, UI, objective and persistence systems separately.

SCALE / PIVOTS
FBX numeric geometry and named pivots in CENTIMETRES, FBX UnitScaleFactor=1, GLBs in METRES.
Convert FBX cm -> Unity m exactly ONCE. Y-up, X lateral, Z front. All modular model root transforms share origin.
Ring visual model reference radius = 5.2 m; game-design physical scenario radius = 45 m, a deliberately scaled representation.
Critical player wire W137: L87_PrimaryRadialGapSensor P019 -> L87_MagneticBearingSafetyPLC P135, analog feedback.
It is DESIGN-ONLY until game wiring and compatible SparkTerminal objects are authored.

NO UNITY PLAY MODE, COMPILATION OR REAL SUSTAINED PHYSICAL SIMULATION TESTING WAS PERFORMED.
