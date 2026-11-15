# PROJECT SPARK — LEVEL 75: WRITE THE SILICON
## Unity gameplay integration plan for the actual 3D asset package

**Status:** This archive contains original procedural visualization assets and design/integration guidance, **not** the playable Level 75 implementation. The 106 player-created circuit connections, 164 `SparkTerminal` GameObjects, industrial safety interlocks, ion implantation physics, PLC process logic, and 6 authoritative objectives must be built inside the existing Unity Project Spark game.

### 1. Scope / asset inventory

- 58 separately importable binary FBX files in `Models/` and 58 corresponding GLB files in `GLB/`.
- One full assembled FBX/GLB showcase for checking alignment (do not instantiate alongside individual models).
- Procedurally modeled ion source, arced magnetic species analyzer, ceramic acceleration column, quadrupole focusing optics, beam steering, XY scanner, retractable Faraday cup, vacuum wafer endstation, photoresist patterned wafer, vacuum and wafer-handling mechanisms, activation chamber, metrology and SCADA.
- A common-origin 19.2 m wide workstation. X is left/right, Y is up, Z points toward the front.
- FBX numeric geometry in centimeters with UnitScaleFactor 1; matching GLB numeric geometry in meters. Check Unity's FBX ModelImporter scale so the overall workstation is ~19.2 Unity units wide, not ~1920.
- Named moving **FBX and GLB parent Null nodes** in `Animation_Rigs.csv`; no baked clips. Preserve the import hierarchy.
- 5 V DC is for **educational control circuitry only**. HV source and beam power are entirely fictional/abstract and not connected directly to the 5 V rail.

### 2. Unity installation

1. Extract the complete pack, import `Models/00_L75_*.fbx` through `Models/57_L75_*.fbx` into an Assets folder.
2. Put `SparkLevel75ArtPlacementEditor.cs` and `SparkLevel75MaterialUpgradeEditor.cs` in a folder called `Editor` so they do not compile into runtime builds.
3. Put `SparkLevel75VisualRig.cs` in the runtime scripts folder.
4. Select the Level 75 scene/layout root and invoke `Tools/Project Spark/Levels/75/Place HD Art Under Selected Layout`.
5. This creates **`L75_HD_VISUAL_ART`** and places 58 FBX prefabs at zero local offset relative to the selected root. The art tool does **not** place gameplay terminals or wires.
6. To prepare basic material previews, select the placed art and choose `Tools/Project Spark/Levels/75/Create & Apply HD Materials To Selected Art`. Verify actual Built-in/URP/HDRP shader transparency and emission.
7. Verify the measured chassis width is near 19.2 m. Do not duplicate an import-scale correction.
8. Verify moving pivot parents in the actual FBX hierarchy before binding the rig.

### 3. Gameplay assembly and separation of concerns

Create the real `Level_75.asset` and `Level_75_Layout.prefab` using the existing Project Spark level and workstation conventions. Inspect the existing API before adding any new types or methods. Do not invent interfaces on `SparkElectricalSolver`, `SparkCircuitSystem`, `SparkTerminal`, `SparkLevelDefinition`, `SparkLevelEvaluator` or `LevelGamePlayManager`.

Every station must be an interactable electrical/functional system where required. Add gameplay colliders, wire ports and terminal anchors as Unity components, not as decorative mesh names. Geometry should never act as a hidden authoritative source of current, force, dose, wafer concentration or completion.

`Gameplay_Wiring_Budget.csv` defines approved **106 planned player connections**. `Gameplay_Wiring_Concept_W01_W106.csv` names conceptual source/target equipment and intent, but **is not a compiled circuit**, lacks actual `SparkTerminal` identifiers, and must be reconciled with Project Spark's real signal/power domain compatibility rules.

`Terminal_Anchor_Plan_164.csv` proposes **164 accessible panel locations** for gameplay terminals; these are **design references only** and are not present as GameObjects inside the FBXs. Move or replace suggested positions where necessary after inspecting the actual art and actual terminal system.

### 4. Six authoritative objectives

**Objective 1 — Build the Ion Implantation Network**: All 106 actual player-created circuit wires connected; exactly 164 unique functional terminals registered; source, beamline, pumps, dosimetry, mask/wafer handling, activation, PLC, SCADA and safety signals valid. Pass 1/6.

**Objective 2 — Commission the Vacuum and Wafer Endstation**: Validate control power, pressure sensors, pumps, throttle and vacuum; transfer patterned wafer through the loadlock; verify wafer mask registration, clamp, cooling and charge protection. Pass 2/6.

**Objective 3 — Qualify the Precision Ion Beam**: Commission the fictional particle source, tune magnetic analyzer species selection, beam focusing and steering, verify blanking, calibrate current measurement, verify beam purity, profile and protection. Pass 3/6.

**Objective 4 — Implant the Perfect Dopant Profile**: Perform actual modeled dose integration over 16 wafer regions; correctly block protected regions, maintain beam current/quality and wafer protections; produce target depth-profile distribution; run virtual post-implant activation; pass simulated sheet-resistance and junction metrology. Pass 4/6.

**Objective 5 — Survive the Beamline Disturbances**: Independently trigger and resolve analyzer field drift, beam-focus instability and charge-neutralization failure. Demonstrate safe beam blanking as required. After recovery, produce and certify a fresh wafer. Pass 5/6.

**Objective 6 — Restore Critical Beam Dosimetry**: Disconnect the actual Faraday-cup feedback wire (conceptual W067). The electrical topology truly changes. The sensor feedback becomes invalid, the dose controller no longer certifies, the blanking actuator stops implantation and the fault latches. Reconnect the wire, recalibrate current sensing, explicitly reset fault, requalify beam and produce a new fully certified wafer including activation and metrology. Pass **6/6** through the *actual* level evaluator/manager.

Objectives 1–5 remain **historically certified** during the final deliberate fault. Reconnection alone must not complete Objective 6.

### 5. Simulation modules to implement in Unity

**Electrical and control power:** 5 V control supply and returns with valid actual player topology; independent abstract high-power authorization; PLC and interlocks; emergency stop and explicit fault reset.

**Vacuum subsystem:** Real evolving chamber/loadlock pressure state, backing and turbo pump dynamics, valve positions, appropriate throughput/leak/load dynamics, calibrated gauge validity. Never instantly set pressure to target.

**Virtual particle source:** Fictional source A and unwanted species B; finite current and source inventory/availability; bounded extraction and transmission. This is not a real high-voltage tuning or toxic dopant handling guide.

**Magnetic analyzer and optics:** Species-dependent transmission and selected mass/charge band, resolving aperture, beam energy category, beam centroid, width, focusing, steering, neutral fraction, protected beam dump and beam losses. Use an efficient reduced-order physical model rather than millions of particles.

**Dose measurement and scan:** Calibrated Faraday cup current reading, timestamp/validity, primary beam current and corner cup readings. A physical selected-beam current can exist without a valid sensor signal; safety must interrupt it through actuator state. Integrate **local dose over time** across 16 patterned regions, respecting mask attenuation and dwell/scan distribution. No unconditional progress-bar dose.

**Wafer state:** Existing Level 74 developed resist mask influences actual implanted regions. Model wafer orientation, region mask coverage, local fluence, protected-region leakage, projected depth, profile width, charge, thermal state and lattice-damage estimate. Keep implanted concentration separate from activated fraction.

**Activation and metrology:** Fictional bounded thermal ramp, damage recovery/activated fraction, sheet resistance estimate, junction depth scan and pass/fail quality gates. Do not fabricate perfect measurements independent of physics.

**Safety:** High-power authorization, vacuum validity, analyzer purity, beam-profile limits, wafer clamp, mask registration, Faraday current validity, charging, temperature, losses and safety enclosure. A critical feedback loss closes the blanker or enters documented safe stop; an HMI warning alone is not sufficient.

### 6. Final feedback wire

Proposed connection: `FARADAY_CUP_CURRENT_OUT -> ION_DOSE_CONTROLLER_BEAM_CURRENT_IN`. `Gameplay_Wiring_Concept_W01_W106.csv` reserves **W067** for this concept. The real W-number and `SparkTerminal` IDs must be verified in the final implemented wiring table.

- Wire disconnected: **real circuit graph mutation**, current sample invalid, stale old sample rejected, dose certification inhibited, beam blanking action physically commanded, safety fault latched.
- Wire reconnected: fresh valid samples, Faraday cup recalibration and bias/zero/reference logic according to fictional instrument, operator-initiated fault reset only when safe.
- Requalification: profile/purity/dose verified again, fresh wafer implantation completed, thermal activation and metrology passed. Final authoritative 6/6.

### 7. Visual rig API (art-only)

`ProjectSpark.Visuals.SparkLevel75VisualRig` exposes:

- `SetActualRPM(float turbo, float backing, float wafer)`
- `SetWaferHandlingPose(float tiltDegrees, float robotBaseDegrees, float robotArmDegrees, float robotToolDegrees)`
- `SetActuatorPositions(float blankerClosed, float loadLockClosed, float faradayCupInserted)`
- `SetBeamlineVisualState(bool sourceActuallyActive, bool beamDeliveryAuthorized, float actualSelectedBeamTransmission01, float scanNormalizedX, float scanNormalizedY, bool criticalFault)`
- `SetProcessVisualization(float actualImplantProgress01, float actualActivationProgress01, float actualMetrologyProgress01)`

It can update named pivots and optional user-configured Unity lights/particle systems. No particle effects or lights are pre-authored into FBX. The source/beam VFX are **optional Unity scene additions** and **must derive from simulation state**.

### 8. AAA production scope

The asset pack is genuine standalone procedural geometry with material color regions and mechanical hierarchy, not a final hand-optimized AAA game asset with baked animations, colliders, hand-painted PBR maps, lighting setup, advanced emissive/refraction shaders or platform LOD chains.

The project team can add HDRP/URP transparent chamber materials, tiled surface microdetails, diagnostic particle-beam VFX, vacuum/thermal sounds, decals, console typography, custom LODs and collision proxies to reach the intended final in-engine visual standard.

### 9. Validation checklist

- Exactly 58 individual FBXs and 58 GLBs; 1 assembled FBX/GLB.
- FBX binary structure and GLB scene geometry load.
- Correct common coordinate origin and ~19.2 m width.
- Every named pivot parent in FBX and GLB matches `Animation_Rigs.csv`.
- 106 **planned** wires and 164 **planned** terminal locations documented (not implemented by this pack).
- No duplicate gameplay terminals after Unity level builder runs.
- Mask, dose, vacuum, optics and safety state uniquely owned in runtime simulation.
- All six objectives registered with authoritative Project Spark evaluator.
- Final wire disconnection truly mutates topology and engages the safety blanker.
- Fresh post-repair wafer passes activation/metrology and the real `LevelGamePlayManager` reaches 6/6.
- Unity compilation and Play Mode must be tested in the user's actual Unity project, not assumed from asset export.
