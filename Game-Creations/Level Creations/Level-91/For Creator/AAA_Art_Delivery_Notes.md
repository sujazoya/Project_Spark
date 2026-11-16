# PROJECT SPARK LEVEL 91 — THE COSMIC CLOCK
## Original procedural 3D art kit • Imported assets are NOT playable gameplay

### Verified intended file structure
- `Models/` contains 96 numbered original binary FBX 7400 models and a separate assembled reference FBX.
- `GLB/` contains 96 glTF 2.0 binary equivalents plus a separate assembled reference GLB.
- `Textures/` contains 12 × 3 = 36 1024² procedural PBR **reference** textures.
- `Animation_Rigs.csv`: 84 `ANIM_*` art pivot parents. They are mesh-owning transforms, not complete real animations.
- `Visual_Terminal_Sockets_260.csv`: 260 gold-rimmed visual sockets; these are not functional `SparkTerminal` objects.
- `Gameplay_Wiring_Concept_W001_W182.csv`: 182 *proposed* connections, not preinstalled player wires.
- `Terminal_Logical_Port_Reconciliation_364.csv`: 364 separate planned endpoint identities.
- `Terminal_Anchor_Signal_Conflict_Report.csv`: 100 reused art socket positions; 67 mixed-signal reuses that must not be shorted together.
- `Objective_Certification_Plan_6.csv`: six design objectives, no runtime `LevelGamePlayManager` objectives implemented.

### Orientation, coordinates and import
- Y is up. X is across the laboratory, Z is forward.
- Each FBX/GLB modular file contains a portion of the shared-origin assembly; instantiate all at local (0,0,0), identity rotation/scale.
- **FBX vertex numerics are centimetres**; FBX UnitScaleFactor is 1. Convert this geometry to Unity metres **once**. GLB numeric vertices are metres.
- Do **not** instantiate `Level91_Complete_ArtKit_Showcase.fbx` on top of the 96 modular FBXs; the showcase is an alternate reference.
- Create a disposable art-only root `L91_HD_VISUAL_ART`, with all electrical terminals, colliders, circuit solver objects, gameplay state, objective history and scene binding objects separate from it.
- Import `.cs` Editor helpers into a Unity `Editor` folder; put `SparkLevel91VisualRig.cs` outside Editor folders.
- Select your Level 91 layout root and use `Tools/Project Spark/Levels/91/Place HD Art Under Selected Layout`.
- Then `Tools/Project Spark/Levels/91/Create & Apply HD Materials To Selected Art`, and `Tools/Project Spark/Levels/91/Validate Imported HD Art`.

### Visual model components
- Four detailed nested concentric X-ray concentrator telescopes and independent gimbal fixtures.
- Four photon-event detector/logic housings with time-tagging display elements.
- Central celestial reference hologram, moving/decorative phase and uncertainty diagrams.
- Atomic clock primary/backup vacuum references with isolation and cooling equipment.
- Event buffers, pulse folding engines, ephemeris store and independent navigation integrity PLC.
- AURORA-1 deep-space vehicle model, protection/motion test equipment and W163 patch station.

### W163 trusted reference
`L91_PrimaryAtomicClockReference` visual `P104` → `L91_NavigationIntegritySafetyPLC` visual `P221`, `TIME_REFERENCE_FEEDBACK`, Critical=YES. Exact functional contacts `TRUSTED_TIME_REFERENCE_OUT` → `TRUSTED_TIME_REFERENCE_IN`.

A player must physically wire and later disconnect W163 in real gameplay to see an electrical continuity change; losing it invalidates fresh trusted timing, not simulated spacecraft momentum. Restore topology, recapture/qualify time, explicitly reset safety, and perform a **brand-new** four-pulsar correction mission before 6/6.

### Important production honesty
This is **procedural AAA-inspired artwork**, not a hand-authored final AAA production pack. It includes no production UV bakes, no collider meshes/LODs, no complete Unity scenes/prefabs, no working electrical/wire systems, no real pulsar observation or orbital dynamics code, no independent safety PLC runtime, and no completed 6/6 objective evaluation. FBX/GLB structure must still be import-tested in the user's actual Unity project.
