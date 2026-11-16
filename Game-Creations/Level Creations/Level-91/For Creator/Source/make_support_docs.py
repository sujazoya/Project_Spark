from pathlib import Path
import csv,json
from build_level91 import NAMES,P,PV,WIRES,CONFLICTS,OBJECTIVES,ANCHORS,logical
ROOT=Path('/mnt/data/ProjectSpark_Level91_HD_AAA')
def dumpcsv(file,rows):
 with (ROOT/file).open('w',newline='') as f:
  w=csv.DictWriter(f,fieldnames=list(rows[0]));w.writeheader();w.writerows(rows)
collision=[]
for i,n in enumerate(NAMES):
 group='environment' if i<16 else 'telescopes' if i<34 else 'timekeeping' if i<52 else 'processor' if i<70 else 'command' if i<82 else 'diagnostics'
 collision.append(dict(Model=n,Category=group,
  CollisionRecommendation='Simple boxes for floor/walkways' if i<16 else 'Primitive capsules/boxes for equipment; no per-fastener mesh colliders',
  LOD0='Source modular FBX' if i in range(16,36) else 'Source mesh',
  LOD1='Artist-authored 50% triangle proxy required',LOD2='Artist-authored 15% proxy required',
  UV1='Unwrapped source UV still required',UV2='Baked lightmap UV still required',
  Physics='Static visuals only unless actual gameplay component attached',
  Notes='DO NOT animate simulated physics by reading visual transforms'))
dumpcsv('Collision_LOD_Production_Plan.csv',collision)
with (ROOT/'AAA_Art_Delivery_Notes.md').open('w') as f:
 f.write(f'''# PROJECT SPARK LEVEL 91 — THE COSMIC CLOCK
## Original procedural 3D art kit • Imported assets are NOT playable gameplay

### Verified intended file structure
- `Models/` contains {len(NAMES)} numbered original binary FBX 7400 models and a separate assembled reference FBX.
- `GLB/` contains {len(NAMES)} glTF 2.0 binary equivalents plus a separate assembled reference GLB.
- `Textures/` contains 12 × 3 = 36 1024² procedural PBR **reference** textures.
- `Animation_Rigs.csv`: {len(PV)} `ANIM_*` art pivot parents. They are mesh-owning transforms, not complete real animations.
- `Visual_Terminal_Sockets_260.csv`: {len(ANCHORS)} gold-rimmed visual sockets; these are not functional `SparkTerminal` objects.
- `Gameplay_Wiring_Concept_W001_W182.csv`: {len(WIRES)} *proposed* connections, not preinstalled player wires.
- `Terminal_Logical_Port_Reconciliation_364.csv`: {len(logical)} separate planned endpoint identities.
- `Terminal_Anchor_Signal_Conflict_Report.csv`: {len(CONFLICTS)} reused art socket positions; {sum(bool(q['MixedSignalKinds']) for q in CONFLICTS)} mixed-signal reuses that must not be shorted together.
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
''')
(ROOT/'Gameplay_Integration_Plan.md').write_text('''# Level 91 Unity gameplay integration plan (NOT included at runtime)

1. Inspect real `SparkElectricalSolver`, `SparkCircuitSystem`, `SparkTerminal`, `SparkElectricalComponent`, `SparkLevelDefinition`, `SparkLevelEvaluator`, `SparkLevel`, `SparkLevelSceneBindings`, `SparkObjectShelf` and `LevelGamePlayManager` code before assuming any method exists.
2. Import 96 individual FBXs as the visual-only layout. Verify centimetre FBX conversion exactly once; keep the showcase separate.
3. Reconcile the 260 decorative sockets with 364 conceptual independent logical contacts: split mixed-signal uses into distinct real terminal children, maintain domain separation, and document any additional physical terminals. Model control-power return paths as actually required by the project API.
4. Register W001–W182 as real **player-required but initially disconnected** circuit graph edges; validate direction, type, voltage/reference, age and topology before activating devices.
5. Maintain independent authoritative simulator states: spacecraft motion/orbit, four detector photon streams and backgrounds, atomic clock bias/drift/temperature, telescope encoder positions, observation timestamps, four stored pulsar time-models, pulse phase fitting, position/velocity covariance, propulsion correction, safety and objective history. Visuals read but never determine those states.
6. Implement repeatable photon counting (Poisson arrival events, pulse profile and background) at scaled game time and measured timestamp accumulation. Simulate a valid four-pulsar geometry with a full-rank navigation measurement model; do not treat 'four samples' as instant qualification.
7. Support three true disturbances: high photon background; atomic clock thermal drift; corrupt ephemeris. Each mutates authoritative state, requires diagnosis and repair, and then fresh time/position qualification.
8. Implement trusted W163 loss based on the **real wire graph**; loss does not mean zero clock bias. Inhibit new autonomous correction burns, preserve independent protection, require actual reconnection, fresh calibration, explicit reset and a fresh four-pulsar correction test.
9. Six objectives from the CSV are historical evidence certifications. Objectives 1–5 remain historically completed during deliberate final W163 fault; only after fresh postrepair full mission can Objective 6 drive actual `LevelGamePlayManager` to 6/6.
10. Add an idempotent gameplay editor menu `Tools/Project Spark/Levels/Create Level 91`, using real inspected Project Spark APIs. The three included art editor menus do NOT replace that game builder.

Test before claiming completion: import/FBX scaling, pivot count, terminal mapping, electrical graph, injection and fault recovery, fresh timestamp/covariance, protected burn authorization, actual objective history, Unity compilation and Play Mode.
''')
(ROOT/'AAA_Polish_And_Lighting_Plan.md').write_text('''# Level 91 — final AAA polish and lighting requirements

**Target:** high-precision cosmic observatory: cold titanium, tungsten-toned gold X-ray mirrors, deep graphite and black instrument electronics, violet celestial map accents, ice-blue clock indicators, warm gold safety rails. Maintain contrast and readability for player electrical interaction.

- Bake authored UV maps; inspect the source procedural PBR reference maps. Packed MRAO channels: R metallic, G roughness, B AO. URP/HDRP metallic workflow may need texture repacking and roughness→smoothness inversion.
- Nested X-ray concentrator shells must look like separate precisely polished annuli. Use segmented reflection probes/reflections, crisp normal maps, restrained roughness and no translucent lens implying ordinary optical microscopy. X-ray telescope visuals are grounded in grazing-incidence concentrator geometry, but not manufactured optical specifications.
- Original material sources in FBX/GLB are vertex-independent preview materials. Transparent `glass` and `holo` may need explicit shader mode, depth write and draw sorting in Unity.
- The four telescope gimbals require a **serial transform hierarchy** for real azimuth/elevation kinematics and movable sunshade; the provided named pivots are mesh-owning art groups, not guaranteed prebuilt game mechanics.
- Add selected-area Light references for gold atomic-clock glow, blue X-ray detector event activity, violet four-pulsar hologram and red W163 fault. No actual X-ray beams should be rendered as physically visible through the lab.
- Final HDRP/URP grading: realistic dark/cold polished metals, path-traced-style reflections where viable, local warm task lights and gentle cyan holograms. Avoid overbright bloom that washes out gold sockets.
- Author collision proxies separately: deck/walkways, cabinet boxes, telescope service rigs; no costly mesh colliders for tiny optical shells, bolts, grating or socket visuals.
- Create 50% and 15% LOD variants, GPU instance repeating optics/mirror shells, batch stationary structural objects and use material atlas for repeated instrument status icons.
- Provide responsive VFX for measured photon activity and uncertainty contraction, not synthetic 'pass' animations. Never derive circuit, navigation, clock, protection or objectives from art animations or light colors.
''')
(ROOT/'Visual_Rig_Integration_Notes.md').write_text('''# Level 91 — Read-only visual rig notes

- `Unity_Optional/SparkLevel91VisualRig.cs` can bind `ANIM_*` children and animate visual telescope positions, sunshades, photon quality indicators and actual fault/lock lights.
- It is a façade only: it does not measure actual photon events, synchronize time, create motion physics, certify navigation, detect a wire disconnect or advance an objective.
- `SetActualTelescopeAngles` must receive actual measured gimbal pose, not user intent.
- `SetActualAcquisition` must receive measured normalized photon signal and actual sunshade extension (not just bright UI).
- `SetActualTimingAndNavigation` must receive authoritative clock/position confidence and independently certified W163 safety state. The light flags must not alter authoritative safety.
- Preserve each pivot's local initial orientation and position. If gimbal yaw/elevation should be physically serial, attach a separate gameplay transform hierarchy and map named art groups to that hierarchy rather than assuming groups are nested.
''')
(ROOT/'README_Unity.txt').write_text('''PROJECT SPARK LEVEL 91: THE COSMIC CLOCK — REAL 3D ART KIT

STEP 1: Extract ZIP. Copy Models/*.fbx to Assets/ProjectSpark/Level91/Models/ and Textures/ if wanted.
STEP 2: Copy Unity_Optional/*Editor.cs to a Unity Editor folder, VisualRig.cs to a non-Editor folder.
STEP 3: SELECT your Level 91 layout root in Unity Hierarchy.
STEP 4: Tools/Project Spark/Levels/91/Place HD Art Under Selected Layout.
STEP 5: Tools/Project Spark/Levels/91/Create & Apply HD Materials To Selected Art.
STEP 6: Tools/Project Spark/Levels/91/Validate Imported HD Art.

NOTE: FBX numeric vertices authored in CENTIMETRES; correct importer conversion to Unity metres ONCE. GLB in metres.
DO NOT instantiate assembled showcase on top of numbered FBXs.

CANNOT CLAIM AAA PRODUCTION COMPLETE: meshes are original detailed procedural art, but no handcrafted final UV bake, LODs, colliders, functional SparkTerminals, live wires, pulsar physics or Unity gameplay/certifications. This project was not Unity Play Mode tested.
''')
print('DOCS created',len(NAMES),'models',len(OBJECTIVES),'objectives',len(CONFLICTS),'shared sockets')
