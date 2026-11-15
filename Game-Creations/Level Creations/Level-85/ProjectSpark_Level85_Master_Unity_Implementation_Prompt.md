# PROJECT SPARK — LEVEL 85 MASTER UNITY IMPLEMENTATION PROMPT

## LEVEL 85 — THE LAST METER

**Advanced AAA Orbital Rendezvous • Relative Navigation & Sensor Fusion • 6-DOF Guidance • Finite RCS Propulsion • Collision Avoidance • Contact Dynamics • Six-Latch Docking • Critical W123 Trust Recovery**

> **SOURCE-GROUNDED DELIVERY:** Based directly on the supplied local `ProjectSpark_Level85_HD_AAA_Models.zip` files, not on a concept image. Model counts, filenames, pivot axes/positions, socket IDs, logical contact names and W123 route are source-derived. Physics models, thresholds, gameplay APIs and testing procedures in the main specification are **requirements to implement**, not claims of existing runtime behavior.


# 01. PRIMARY MANDATE

Act as my **senior Unity C# gameplay engineer, Project Spark electrical-circuit integration architect, spacecraft guidance/navigation/control engineer, astrodynamics and relative-orbit simulation engineer, spacecraft RCS/propulsion engineer, sensor fusion and metrology engineer, docking mechanism/mechatronics engineer, autonomous systems functional-safety engineer, industrial PLC programmer, AAA technical artist, and Unity Editor automation developer**.

**IMPLEMENT LEVEL 85 — THE LAST METER — AS A COMPLETE, ACTUALLY PLAYABLE LEVEL INSIDE MY EXISTING PROJECT SPARK UNITY GAME.** This is an implementation specification for the already-generated Level 85 geometry, **not** a request to redraw the concept, generate new pictures, or arbitrarily replace the supplied mesh files. Import and use the exact `ProjectSpark_Level85_HD_AAA_Models.zip` models/manifests and adapt to the project's real API signatures.

Implement **146 real player-created electrical connections**, a physically coherent educational orbital rendezvous and six-degree-of-freedom relative-navigation simulator, finite thruster force/propellant/valve response, sensor acquisition and fusion, safe approach corridor/hold points, actual docking contact mechanics, six latches, interface/seal checks, three genuine disturbances, and **W123** trusted closing-rate feedback loss/recovery. Create all six evidence-based objectives through actual `LevelGamePlayManager` integration. Preserve **Levels 1–84**. Final success is **6/6 through real player actions and validated simulation**, not a button, decorative animation, prewired screenshot, or editor-only progress flag.


# 02. READ THE EXACT SOURCE PACKAGE FIRST

Unzip and inspect the actual files, using the ZIP as the primary data authority:

- `Model_Manifest.json`, `Verification_Report.json`, `Art_Assembly.csv`, `Component_Roles.csv`.
- `Animation_Rigs.csv` (**60** `ANIM_*` parent records with owners, axes and XYZ coordinates).
- `Gameplay_Wiring_Concept_W001_W146.csv`, `Gameplay_Wiring_Budget.csv` (**146** proposed wires, their actual planned endpoints, types and purposes).
- `Terminal_Anchor_Plan_224.csv`, `Visual_Terminal_Sockets_224.csv` (**224** decorative socket locations).
- `Terminal_Logical_Port_Reconciliation_292.csv` (**292** individual source/destination logical-port proposals).
- `Objective_Certification_Plan_6.csv`, `W123_Critical_Closing_Rate_Recovery.md`, `Gameplay_Integration_Plan.md`.
- `Unity_Optional/SparkLevel85ArtPlacementEditor.cs`, `SparkLevel85AssetValidatorEditor.cs`, `SparkLevel85MaterialUpgradeEditor.cs`, `SparkLevel85VisualRig.cs`.
- `Texture_Manifest.json`, `README_Unity.txt`, `AAA_Art_Delivery_Notes.md`, `Checksums_SHA256.txt`.

Annexes include the source-derived filenames, pivots, exact wire descriptions, anchor positions and every logical endpoint. **The actual imported artifacts remain authoritative if a transcription differs.** Don't mistake concept CSV data for live player circuits or an art script for an actual gameplay builder.


# 03. VERIFIED ASSET BASELINE — NO FALSE IMPLEMENTATION CLAIMS

The supplied Level 85 source reports **78 modular FBX 7400 meshes + 78 corresponding GLBs**, one separately assembled showcase FBX/GLB, **200,132 modular triangles**, **1,017 mesh parts**, **60 mesh-owning animation parent groups**, **224 modeled visual sockets**, **146 conceptual connection rows**, and **292 proposed individual logical contact endpoints**. There are **12 procedural reference material sets and 36 PNG maps**. The nominal workstation bounds are **X −14.6 to +14.6 m, Y 0 to 10.128 m, Z −7.85 to +7.85 m**, i.e. about **29.2 m wide × 10.128 m high × 15.7 m deep**.

The structural `Verification_Report.json` says geometry/planning were parsed and inspected. It explicitly does **not** establish Unity import, Unity compilation, Play Mode, gameplay circuitry, orbital dynamics, real mechanical docking, final production UVs, or finished artist-authored AAA assets. The optional rig is visual-only. Generated procedural PBR maps are **reference textures requiring correct UV/shader work**. Use the term **AAA-inspired procedural art kit**, then finish the actual production work; do not claim fully finished AAA fidelity based solely on file size or model count.


# 04. LEVEL IDENTITY AND UNIQUE GAMEPLAY

- Level ID: `level_85`.
- Title: **The Last Meter**.
- Subtitle: **Advanced Orbital Rendezvous and Spacecraft Docking**.
- Difficulty: expert/master.
- Progress: six authoritative objectives; historical certificates and new run-scoped evidence.
- Critical final edge: **W123**, source **`L85_RelativeNavigationFusionComputer/P148`**, target **`L85_DockingSafetyPLC/P046`**.
- Proposed level-specific game builder: **`Tools/Project Spark/Levels/Create Level 85`**.
- Art-only import menu provided by the ZIP: **`Tools/Project Spark/Levels/85/Place HD Art Under Selected Layout`**.

This level follows Level 84's attitude pointing by introducing **relative translational orbital motion, powered trajectory maneuvers, imperfect range/range-rate estimation, real docking contact, six independent latch confirmations, pressure/interface verification and autonomous abort logic**. It must not recycle Level 84's fine pointing objective as a substitute for actual orbital rendezvous. The user should experience the engineering workstation, spacecraft navigation hologram, close-up docking camera and external inspection views as one coherent live simulation.


# 05. INSPECT REAL PROJECT SPARK GAMEPLAY APIS

Search the user's real Unity codebase, inspect public and serialized signatures, and use what's actually there: `SparkElectricalSolver`, `SparkCircuitSystem`, `SparkElectricalComponent`, `SparkTerminal`, `SparkSwitch`, `SparkLevelDefinition`, `SparkLevelEvaluator`, `SparkLevelSceneBindings`, `SparkLevelWorkstationBindingBridge`, `SparkShelfSlot`, `SparkObjectShelf`, `SparkLevelLayoutLoader`, `SparkLevel`, `LevelGamePlayManager`, shop/purchase logic, wire creation/removal callbacks, safety latch/reset and persistence. Identify which earlier-level interfaces actually exist; reuse validated Level 84 attitude/IMU/reaction-wheel code if present. Do not assume design prompts from Levels 82–84 prove those classes compiled. Create minimal adapters with explicit interfaces where actual APIs differ. Keep all new Level 85 scene, scripts and assets namespaced and isolated from Levels 1–84.


# 06. ABSOLUTE IMPLEMENTATION RULES

1. Preserve Levels 1–84 and don't globally overwrite player inventory, prefabs, solvers or level definitions.
2. Start with puzzle wiring **unconnected**; 146 conceptual rows become 146 player-created, solver-validated edges.
3. Keep 224 decorative `SOCKET_P###` transform markers distinct from real electrical `SparkTerminal` nodes.
4. Provide 292 separate logical endpoints (or a validated equivalent graph with documented electrical equivalence); don't short independent data just because one art anchor is reused.
5. Educational 5 V logic, fictional spacecraft thruster/actuator power, station docked interface and independent safety authority must stay domain-separated.
6. Use true plant state, sensor samples with timestamps, estimated state, commands, and visual state as **different** concepts.
7. Actual target-relative vehicle momentum and attitude cannot teleport when the player changes a setpoint or presses HOLD.
8. Finite propellant, bounded thruster acceleration, real actuator delay and coupling; no perpetual thrust or instantaneous fuel reset.
9. A single renderer overlap, green HUD light, near-zero displayed distance, or countdown does not prove docking.
10. Safe abort depends on physical available fuel/control authority and independent sensor availability; never assert guaranteed rescue when that is not true.
11. If already mechanically hard-docked, W123 loss must not unhook an intact certified docking mechanism.
12. W124 display speed and W121 diagnostic health are **not** substitutes for W123 trusted continuous feedback.
13. Three disturbances must change actual plant/measurement/actuator state and require diagnosis, repair, then **fresh** mission evidence.
14. Historical Objectives 1–5 remain earned during the deliberate last fault; Objective 6 must use a new post-repair run ID.
15. Visual rig, VFX, panels and cameras display real state only; their values never determine solver truth or objective PASS.
16. Never claim real-world flight qualification or physical pressure-vessel safety; this is a bounded fictional educational model.
17. Don't claim Unity compilation or Play Mode tests passed until actually executed.


# 07. INITIAL GAMEPLAY STATE — 0/6

At launch, the station and service spacecraft are visibly separated. Service vehicle at the designed nominal pose near **X −4.8 m, Y 5.05 m, Z −1.45 m** in the stage. Real mission range is initialized by the **relative-orbit state**, not inferred from arbitrary geometry spacing in the stage. Docking corridor has virtual 100 m / 30 m / 5 m approach stages and a low-speed terminal phase, all **adjustable game-scale parameters**.

Control power, safety PLC and sensors are not fully commissioned; navigation instruments require calibration; RCS pulses and estimates are not qualified; docking rings/latches/couplers are disengaged; no successful mission certificate exists; W123 must be created by the player like every other wire before normal approach. Three simulated faults are available but are not initially running. Historical objective count **0/6**.


# 08. UNITY FBX/GLB IMPORT AND TRUE SCALE

The package authoring conventions are explicit: **binary FBX mesh vertices and pivot translations are numerically centimetres** with `UnitScaleFactor = 1`; matching GLB geometry is numerically **metres**. The world is Y-up, X station approach, Z depth. The importer must convert FBX centimetres → Unity metres **exactly once** and validate bounds against ~29.2 × 10.128 × 15.7 m. Avoid a second ×0.01 when Unity has already applied conversion.

Import every `Models/00_...fbx` through `Models/77_...fbx` at a shared local origin `(0,0,0)`, identity rotation and post-imported unit scale. Parent as coordinated models under a dedicated replaceable **`L85_HD_VISUAL_ART`**. Do not instantiate `Models/Level85_Complete_ArtKit_Showcase.fbx` or `GLB/Level85_Complete_ArtKit_Showcase.glb` at the same time: those contain a duplicate assembled art reference.

Build runtime colliders, gameplay anchors, constraints, `SparkTerminal` components and scene controller objects **outside** the disposable art root. Never use mesh renderer bounds as your sole collision/contact detector.


# 09. USE AND VERIFY EXISTING ART-ONLY UNITY MENUS

The ZIP includes three Unity Editor helpers in namespace `ProjectSpark.EditorTools`:

- `SparkLevel85ArtPlacementEditor`: **`Tools/Project Spark/Levels/85/Place HD Art Under Selected Layout`**; checks 78 numbered FBXs and creates/replaces art root.
- `SparkLevel85AssetValidatorEditor`: **`Tools/Project Spark/Levels/85/Validate Imported HD Art`**.
- `SparkLevel85MaterialUpgradeEditor`: **`Tools/Project Spark/Levels/85/Create & Apply HD Materials To Selected Art`**; material output path `Assets/ProjectSpark/Level85/Materials`.

Runtime visual class `ProjectSpark.Visuals.SparkLevel85VisualRig` must **not** go under an `Editor` folder. Existing `*Editor.cs` scripts belong in editor-only compilation. The artwork menu does not implement physics, terminals, objective tracker, safety or gameplay builder. New menu **Create Level 85** must be implemented separately and be safe to re-run.


# 10. MOVEABLE SPACECRAFT ART HIERARCHY AND POSE

The provided art placement helper creates a special **`L85_DYNAMIC_SERVICE_SPACECRAFT`** child at local `(-4.8, 5.05, -1.45)` and reparents ship modules indices **18–31 and 35–42** using world-position preservation (excluding bench equipment 32–34). Preserve this actual grouping if using the supplied importer. The visual rig looks for that exact transform name on `Awake`. The vehicle's art is already authored at world-space positions before grouping: don't offset it twice.

Bind the rigid spacecraft's authoritative relative position and body attitude to the root through `SetActualServiceSpacecraftPose`. Keep a **clearly documented transformation** between orbital LVLH physical-state coordinates, the station-centered X-approach stage, and the dynamic model's local rendering transform. An absolute orbital 100 m range must not accidentally send the art 100 metres outside a 29.2 m diorama; choose a well-documented visualization scale, while collision/certification uses unscaled physical relative-state metres.

Group hull-mounted sensors and thruster VFX with the spacecraft; don't move benchtop navigation racks when the spacecraft translates. Add attachment validation tests and ensure spring/constraint motion does not violate intended module parenting.


# 11. PBR MATERIALS, LIGHTING, AAA FINISH AND PERFORMANCE

The 12 sets/36 PNGs are **1024² procedural reference maps**: albedo, normal, packed MetalRoughAO where **R=metallic, G=roughness, B=ambient occlusion**. For URP/HDRP shader inputs, correctly repack channels (notably roughness → smoothness inversion when required), use proper sRGB/linear flags and normal-map import mode. **UV production is not finished**: unwrap/bake before asserting these maps will match actual model surfaces.

Target polished station hull composites, brushed titanium, machined rings and hinge pins, gold socket contacts, insulated harnesses, anti-reflective optics, credible docking seal elastomer, emission materials only where physically useful, starfield/Earth background, and legible workshop light. Add practical authored wear, optimized LODs, colliders and motion constraints. Profile 1,017 meshes for draw calls, batching/instancing, translucent holograms, and narrow-beam sensor effects. Build URP/HDRP VFX for scanner sweeps, real thruster pulses, alignment guides, red safety faults, latch/umbilical movement, and certification—none should decide physics.


# 12. VERIFIED LEVEL 85 VISUAL RIG PUBLIC API

The package contains **`ProjectSpark.Visuals.SparkLevel85VisualRig`**. Verified public method signatures:

```csharp
public void SetActualServiceSpacecraftPose(Vector3 actualTranslationMetres, Quaternion actualBodyAttitude);
public void SetActualRelativeNavigation(float measuredRangeMetres, float measuredClosingRateMps,
    float measuredLateralErrorMetres, float measuredAttitudeErrorDegrees,
    bool trustedClosingRateW123Valid);
public void SetActualLiDARScanHeadAngles(float actualAdeg, float actualBdeg);
public void SetActualThrusterPulseLevels(float plusX, float minusX,
    float plusY, float minusY, float plusZ, float minusZ);
public void SetActualDockingMechanisms(float actualSoftCapture01,
    float actualSixLatchLockFraction01, float actualSealCompression01,
    float actualElectricalCouplerExtension01, bool physicallyContacting,
    bool actualStructuralPressureAndInterfaceQualified);
public void SetActualSafetyState(bool actualW123FeedbackValid,
    bool actualCriticalFaultLatched, bool actualAbortModeActive);
```

The original visual rig caches **60** `ANIM_*` transforms; on `LateUpdate` it currently directly animates **selected** pivots (service dock probe, soft-capture ring, latch bank, pressure seal, electrical umbilical, capture petals, two LiDAR scanner heads), root vehicle pose, warning/certification lights and optional thruster particle systems. **Do not claim it animates all 60 pivots automatically**. Implement measured-state animation for additional relevant pivots and verify pivot uniqueness, correct owner and axis. The light's baked .15 m/.05 m visual warning tolerances are **presentation thresholds** and must not be mistaken for configurable mission authorization constraints.


# 13. 60 EXACT ART PIVOTS — TYPES AND AXES

Read all 60 original records in `Animation_Rigs.csv` and Annex B. `rotation` records use the provided authored local axis, `translation` records have a finite authored mechanical path, `indicator` records must not be treated as physical force actuators, and `quaternion` for `ANIM_SERVICE_SPACECRAFT_BODY` represents orientation. Preserve original pivot hierarchy and home transform; use measured position/velocity for motion and appropriate interpolation for visuals. Never multiply a movement parent and child rotation blindly. Capture an automated report listing missing/duplicated names, wrong type/owner, and transforms outside expected bounds.


# 14. EXACT 146-WIRE PLAYER CONNECTION BUDGET

Use the original `Gameplay_Wiring_Concept_W001_W146.csv` without renaming WIDs or inventing routes. The **planned** group counts:

| IDs | Engineering group | Count |
|---|---|---:|
| W001–W010 | Educational 5 V control power and return verification | 10 |
| W011–W034 | Sensor acquisition and autonomous navigation | 24 |
| W035–W058 | Reaction control propulsion and valve feedback | 24 |
| W059–W078 | Relative trajectory planning and 6DOF guidance | 20 |
| W079–W096 | Docking approach, beacons and collision safety | 18 |
| W097–W114 | Mechanical soft capture, locks and pressure seal | 18 |
| W115–W130 | Independent docking safety, abort and W123 recovery | 16 |
| W131–W146 | SCADA, diagnostics and final certification | 16 |
| **Total** | | **146** |

Each row contains source model, source visual anchor, exact proposed logical source terminal, target model, target visual anchor, proposed logical target terminal, signal kind, purpose and critical flag. Source row statuses are **`CONCEPT_ONLY_NOT_PLAYER_CREATED_WIRE`**. Convert them into genuinely player-connectable, graph-validated circuit edges with real compatible `SparkTerminal` components. Refuse incorrect direction/type, nonexistent power returns, out-of-range voltage and invalid safety bypass. Count only correct live graph edges, not prefab decorative cables or saved counters. W123 is the only row marked `Critical=YES`.


# 15. 224 VISUAL SOCKETS VS 292 LOGICAL CONTACTS — RECONCILE SAFELY

`Terminal_Anchor_Plan_224.csv` has **P001–P224** gold-rimmed **art-only** socket locations; `Terminal_Logical_Port_Reconciliation_292.csv` contains **292** proposed independently named endpoint contacts, exactly two per conceptual wire. The 292 mappings use **223** unique `(Model, VisualAnchorID)` pairs with **41 reused** art anchors. In this particular kit, reused anchors happen to keep the same broad signal kind and domain, but **same kind does not mean same net**: distinct analog feedback channels may still carry unrelated values. Examples: **`L85_RelativeNavigationFusionComputer/P148` is used by W080 (approach confidence) and W123 (trusted closing rate)**. If shorted into one node, a perfectly typed analog wire would still be semantically incorrect.

Implement a terminal reconciliation pass with **one explicit logical endpoint per planned wire side** unless a verified, deliberate shared-bus/junction topology supports sharing. Bind each to the parent model and original decorative P ID, add offset child connector/label/breakout points if necessary, validate direction/type/domain and record `WireID → Model → P### → LogicalTerminalID → actual SparkTerminal`. Don't claim **224 distinct real SparkTerminals** can represent **292 isolated logical port contacts** without a designed mapping. Keep all original 224 art markers intact; an unused P224 is allowed as a source spare. Output a machine-readable `Level85_TerminalReconciliationReport.csv`. Reject incorrect shortcuts that conflate W080 and W123.


# 16. CONTROL-POWER, PROPULSION AND SAFETY DOMAINS

Domain A: Project Spark's real educational **5 V control-circuit graph** for players' wires, low-power instrumentation, digital/analog signal permits, input/returns and true physical disconnection. Domain B: **fictional spacecraft propulsion/electrical plant** with actual thruster forces, valves, limited propellant, actuator power budget, thermal proxy and station interface power state. Domain C: **independent safety authority** capable of withdrawing docking permission, commanding a feasible hold/retreat, preserving latched diagnostics and enforcing independent keep-out/collision rules.

The file's W001–W010 rows are nominal 5 V logic supplies; do **not** assume the conceptual rows individually enumerate every explicit ground-return conductor required by the existing solver. Inspect the real circuit architecture and add valid return paths/terminal blocks or report the necessary reconciliation. A `DATA_STREAM` wire is not a hydraulic propellant feed; a modeled RCS force is not direct current in an ordinary 5 V wire. Couple real control graph continuity to physical plant effects via device models and explicit adapters.


# 17. W123 TRUSTED FEEDBACK — EXACT SOURCE ROW

The only critical row in the actual CSV is:

- ID: **`W123`**; purpose: **`CRITICAL trusted relative closing-rate feedback`**.
- Kind: **`ANALOG_FEEDBACK`**; `Critical=YES`.
- Source: **`L85_RelativeNavigationFusionComputer`**, visual anchor **`P148`**, proposed source terminal **`TRUSTED_RANGE_RATE_OUT`**.
- Target: **`L85_DockingSafetyPLC`**, visual anchor **`P046`**, proposed target terminal **`TRUSTED_RANGE_RATE_IN`**.
- The trusted message includes consistent signed closing-rate estimate, sequence number, measurement timestamp/age, confidence, calibration version and validity—not an unchanging green boolean.

**W124** is `L85_RelativePositionVelocityEstimator/P162 → L85_HolographicRendezvousSCADA/P069`, display-only relative-speed telemetry; **W121** is calibration-health diagnostic, **W122** a raw calibration channel. None can serve as the W123 normal safety feed. **W125** authorizes escape/retreat, **W126** returns measured abort completion, **W128** is explicit operator reset, and **W129** is safety status telemetry. Use these source routes for proper separation of authority.


# 18. TRUSTED NAVIGATION AND INDEPENDENT SAFETY BOUNDARY

Do not treat the absence of a W123 sample as **zero closing speed**. W123 validity requires real wire continuity, powered source and receiver, compatible calibrated analog channel, monotonic sample sequence, appropriate freshness window, sensor-fusion confidence and consistency checks. On stale/missing feedback, withdraw autonomous docking/hard-capture permits independently of SCADA display. W124 can remain visible as a *diagnostic* even while W123 is broken; it must not authorize new dock approach.

Independent keep-out and collision safety uses its own validated measures/rules and can protect against failed normal feedback; it should not silently synthesize a fake W123 pass. If independent measurements also fail, indicate loss of safety assurance and enter a controlled degraded mode. Explain the difference between **current plant safe state**, **valid sensor trust**, and **previous historical mission certification**.


# 19. ONE AUTHORITATIVE ORBITAL RENDEZVOUS PROCESS STATE

Represent at least: station orbit/reference frame and gravitational parameter/game mean motion; true relative position `r`, velocity `v`, spacecraft quaternion `q`, angular velocity, mass/inertia, propellant, per-valve request/actual, thrust/torque and failure; sensor true outputs/samples/timestamps/calibration/covariance; fused estimated state and confidence; guidance target/hold point/collision envelope; control/electrical graph authorization; soft-contact pose/speed and compression; six independent latch states/contact forces; coupler continuity; fictional seal/leak integrity; independent safety and fault latch; abort/run IDs; full mission evidence and historical objective state.

Single authoritative physical state. Sensor modules sample it; estimators consume sensor samples; guidance consumes estimates; actuator models receive bounded commands; safety monitors physical and validated measured data; UI and visual rig observe these results. Never duplicate the range/velocity estimate independently inside the SCADA, flight controller and PLC with contradictory ownership.


# 20. FIXED-STEP SIMULATION ORDER AND UNITS

Suggested deterministic fixed-step process (adapt to actual game's event/update ownership):

1. Read real wire topology, power, returns, player controls and safety state.
2. Decode valid control commands and reject unsafe modes.
3. Advance station/orbit reference and target kinematics.
4. Update valve drive dynamics, individual thruster delivered forces and propellant mass.
5. Integrate service craft translation, attitude and rotation with proper frame mapping.
6. Detect station/corridor/capture contact and compute contact responses.
7. Update actual capture petals, damping, latches, coupler and seal/leak states.
8. Sample LiDAR, radar, stereo, IMU, optical patterns and clocks with noise/faults.
9. Run sensor association/fusion, timestamp checks and uncertainty prediction.
10. Check safety/abort/keep-out, authorize limited guidance and docking commands.
11. Advance PLC/mode sequencing; capture run-scoped evidence and diagnostics.
12. Certify objectives from evidence; publish read-only SCADA and visual-rig inputs.

Use SI metres/seconds/newtons/kg/radians consistently internally, convert to display degrees where needed, and clamp physically invalid mass/energy/impulses. Use a fixed timestep with documented integration stability and deterministic fault seeds; never let variable graphical frame time alter docking success.


# 21. RELATIVE ORBIT DYNAMICS — REAL MOVEMENT, NOT KEYFRAME TELEPORT

Implement a **clearly labeled educational** target-relative orbital model. For a circular target reference, a configurable Clohessy–Wiltshire/Hill approximation is sufficient for the intended game stage (with appropriate LVLH axis convention):

`x¨ − 2n y˙ − 3n² x = a_x`, `y¨ + 2n x˙ = a_y`, `z¨ + n² z = a_z`.

These equations use **x radial, y along-track, z cross-track** in LVLH; **do not confuse this physics frame** with the art stage's **X = station approach**. Implement an explicit rotation/mapping from LVLH to the station-centered docking port frame. For the near-terminal approach, use bounded translational relative pose and contact dynamics; do not add an unphysical pointwise acceleration just to make the cinematic vehicle line up. Include a documented alternative simplified mode if CW integration is too expensive, but make its limitations explicit and keep momentum real. Model target rotation/docking-port axis as needed; hard-docked craft share the target's state in a defined attached-body mode only after real capture.


# 22. SIX-DEGREE-OF-FREEDOM VEHICLE DYNAMICS

State should include `r, v, q, ω`, mass, inertia tensor and accumulated forces/torques. At each timestep, compute delivered thruster force vectors and moment arms: `F_total = Σ F_i`, `τ_total = Σ (r_i × F_i)`, transform into the chosen inertial/relative frame, integrate translation/rotation, and normalize quaternion. Add finite angular-rate/docking misalignment constraints and reaction-wheel/attitude control reuse **only if real Level 84 code or Level 85 physical actuators support it**.

**Six labeled ±X/±Y/±Z thruster assets do not automatically imply six independent controllable degrees of freedom.** Determine actual nozzle directions and moment arms. If the geometry supplies only translational force along body axes, give attitude to a validated separate actuator system/reused Level 84 ADCS; otherwise create and document an additional gameplay actuator with the correct art and controls. Don't manufacture yaw/pitch/roll torque from engine count or let a six-axis 'controller' output abstract perfect torque without installed authority. Include saturations and axis-coupling.


# 23. RCS THRUSTER PULSE AND VALVE MODELS

Model `L85_RCSPlusXThruster`, `MinusX`, `PlusY`, `MinusY`, `PlusZ`, `MinusZ`, `L85_ThrusterValveDriverRack`, `L85_ReactionControlPropellantManifold`, `L85_ThrusterFlowAndPressureMonitor`, and `L85_PrecisionThrusterCalibrationFixture` as separate controls/physical device states. Each valve has open/close lag, finite response, maximum/minimum stable impulse, hysteresis/stiction, commanded duty, actual measured drive/pressure/flow, available supply and fault state.

Thruster force depends on actual valve opening and pressure/availability rather than an instant command. Avoid highly technical real propulsion handling procedures; all model values are fictional controlled game parameters. Show thruster pulse VFX only when modeled impulse is delivered. Implement stuck valve as actual valve state mismatch that changes force, trajectory, fuel and diagnostic readings.


# 24. PROPELLANT, MANIFOLD AND FUEL-RESERVE SAFETY

Model Pack A/B propellant vessels, available propellant mass, mass distribution if useful, regulator/valve flow proxy, pressure response, finite thrust duration, consumption and reserve forecasts. A simplified coherent depletion law `m_next = max(0, m − m_dot*dt)` is acceptable, with `m_dot ≥ 0` and documented force/flow relation. A pressure fault changes thrust rather than merely blinking red. Do not permit negative fuel, unlimited emergency impulses, or 'restore full tank' via warning reset.

`L85_FuelReserveAndAbortPredictor` should estimate minimum feasible retreat Δv/impulse including current distance, closing rate, attitude and uncertainty, with reserve guard-band. If a feasible ABORT cannot be executed, fail safely by withdrawing fresh dock authorization, log shortage, and use the best physically feasible attitude/hold response; never claim it performed a 30-metre retreat with an empty tank.


# 25. THRUSTER CHARACTERIZATION AND ALLOCATION

Before approach, commission every propulsion channel: command a bounded short pulse in the game calibration fixture, observe delivered force/response and measured telemetry, compare expected vs actual, estimate response lag and allocation effectiveness. Compute a bounded allocation for desired acceleration/torque using the real installed actuators and reachable constraints (e.g. constrained least squares/pseudoinverse with saturation). A missing/stuck thruster must reduce the feasible wrench set, not magically redistribute infinite force. Make trim compensation and diagnostic corrections measurable and time-limited. An invalid flow sensor or missing wiring refuses full closed-loop qualification.


# 26. DUAL LIDAR, STEREO, RADAR AND BEACON SENSORS

Implement physically distinct synthetic sensing paths from the source models:

- `L85_PrimaryScanningLiDAR`, `L85_SecondaryScanningLiDAR`: range, scan head actual angle, field-of-view, surface reflectivity/blind spot, range confidence, timestamp and dropout.
- `L85_StereoNavigationCameraLeft/Right`: calibrated baseline, feature extraction/triangulation proxy, illumination/occlusion sensitivity, target marker association and lateral pose error.
- `L85_ShortRangeRendezvousRadar`: range/range-rate estimates and independent acquisition/uncertainty appropriate to fictional stage.
- `L85_DockingBeaconReceiver`, `L85_OpticalTargetPatternTracker`: identifiable target marker sequence and bearing quality.
- `L85_SpacecraftInertialMeasurementUnit`: acceleration/gyro bias, noise, calibration and drift; don't mistake inertial integration for absolute relative position.

Every sensor sample must be derived from true relative geometry/pose and current fault conditions, with timestamp, calibrated range/error bounds and confidence. Generate deterministic noise/bias seeds for reproducible QA; don't produce always-perfect exact true state disguised as LiDAR.


# 27. SENSOR CALIBRATION, LATENCY AND MEASUREMENT TRUST

Require actual user calibration tasks: camera baseline and boresight, primary/secondary LiDAR distance and scan home, radar zero/range bias, IMU alignment and bias, docking beacon ID/clock sync, and thruster response verification. A 'CALIBRATE' button alone is not enough: perform an instrument procedure against known fictional references, record offsets/residuals, expiration and calibration version. Sampling has independent rates/latencies; stale LiDAR cannot be quietly consumed as fresh. A dropout need not mean true range changed. Distinguish **true target range**, **raw measurement**, **filtered estimate** and **PLC-trusted W123 reading** on diagnostic UI.


# 28. RELATIVE STATE ESTIMATOR AND COVARIANCE

`L85_RelativePositionVelocityEstimator` and `L85_RelativeNavigationFusionComputer` should fuse asynchronous sensor samples into estimated position, velocity, orientation and covariance/quality. An educational extended/complementary Kalman-style filter is suitable: propagate the orbit/IMU model, measure sensor residuals, gate outliers, update when valid samples arrive, and increase uncertainty during dropout. Make the estimator detect range-rate sign and units; log which sensors contribute. `L85_NavigationUncertaintyVolumeDisplay` should show the estimator confidence envelope, **not** an artist-chosen green sphere independent of covariance.

The closing-rate sensor must be defined consistently (for example positive closing speed `−d(range)/dt`, if used); W123 carries a signed, time-stamped, trusted derived measurement. Nonclosing receding velocity or a stale sample must not accidentally qualify 'zero speed' at capture.


# 29. TRAJECTORY PLANNING, GUIDANCE AND ACTUAL FLIGHT CONTROL

`L85_RelativeTrajectoryPlanningComputer` computes feasible relative waypoints, Δv budget, hold points, line-of-sight corridor and predicted stopping distance using estimated state and available actuator authority. `L85_ProximityOperationsGuidanceComputer` generates bounded desired translational/rotational acceleration, `L85_SixDegreeFreedomFlightController` executes actual feedback control, and `L85_ThrusterForceAllocationComputer` maps the desired wrench to real individual actuators. Distinguish generated **trajectory**, **requested command**, **delivered force**, **measured vehicle state** and **certified hold point**.

Make player engineering choices consequential: aggressive closing saves time but raises fuel demand and overshoot risk; conservative braking preserves capture safety but may miss time/fuel constraints. Do not create autopilot success solely when progress along X reaches the desired position.


# 30. APPROACH CORRIDOR AND HOLD POINTS

Use game-scale, tuneable approach gates rather than real spacecraft standards: optional nominal holds at **100 m**, **30 m**, **5 m**, then **0 m docking contact**. The 3D corridor requires correct relative station-frame position and lateral/cross-track error, heading/alignment, valid trustworthy sensor fusion, feasible stopping distance, fuel reserve, low angular rate, safe no-go/keep-out geometry and independent safety OK.

Holding point achievement requires actual velocity to settle within limits for a minimum confidence/stability dwell; touching a waypoint while flying past at high speed fails. Stay within a configurable station exclusion envelope except via the authorized path. A new qualification run must observe actual required state transitions; a cinematic scrubber/progress button cannot mint hold-point certificates.


# 31. COLLISION PREDICTION AND ABORT BEHAVIOR

`L85_DockingCorridorSafetyGuardian`, `L85_CollisionAvoidancePredictionMonitor`, `L85_ProtectedAutonomousAbortController`, and `L85_EmergencyApproachHoldStation` must calculate predicted relative intersection/closest approach, closing speed, available braking distance/Δv and maneuver authority. Implement independent safety trip conditions for corridor violation, missing trusted feedback, excessive uncertainty, physical collision risk, stuck-on thruster, missing fuel reserve, PLC control power loss and wrong docking port target.

When safe-retreat fuel/thrusters/navigation are available, issue a finite actual retreat, verify measured response with W126 and enter safe hold. When not feasible, report the risk and physically integrate actual drift rather than teleporting to neutral. For a fully hard-docked vehicle, prevent unintended unlatching/undocking during sensor fault; respond in a dedicated attached safe state.


# 32. APPROACH ACCEPTANCE VALUES — FICTIONAL AND CONFIGURABLE

Example adjustable game scenario targets: contact closing speed **≤0.05 m/s**, lateral alignment **≤0.15 m**, attitude axis error **≤1.5°**, trustworthy recent sensor data, finite reserve for abort, and mandatory hold-point tolerance/dwell. These are **game defaults**, not a claim of real spacecraft procedures or qualification standards. Use actual signed closing speed magnitude and actual relative angular velocity at contact, not just screen text values. Allow level designer to configure by ScriptableObject and report active values in UI/QA. Hard-gate unsafe contact and record explicit margins: measured speed, lateral offset, axis angular error, angular rate, contact loads, reserve and estimated uncertainty.


# 33. DOCKING CONTACT DETECTION AND SOFT CAPTURE

Model `L85_StationPrimaryDockingPort`, `L85_DockingOuterGuideRing`, `L85_DockingPassiveCaptureCone`, `L85_ActiveSoftCaptureRing`, `L85_MotorizedCaptureGuidePetals`, `L85_ServiceSpacecraftExtendableDockingProbe`, `L85_SixAxisContactForceSensor` and `L85_CompliantContactDampingActuator`. Detect mating-interface contact using physics colliders/convex simplified capture shapes and actual relative pose/speed; compute normal/tangential contact response (bounded spring–damper proxy) and six-axis contact force/moment. Contact can bounce, fail, or require retreat when too energetic/off-axis. A shape intersection in a visual asset is never a pass condition.

Soft capture occurs only within alignment/velocity/force limits and after compliant mechanical engagement. Transition from free-flight to captured relative mechanical state without discontinuously erasing momentum; damping must dissipate kinetic energy over time and generate measurable contact loads. Keep target and service craft coherent in a single reference frame.


# 34. SIX LATCH ACTUATORS AND HARD DOCK CERTIFICATION

Model six individual latch actuator states using `L85_SixActuatorDockingLatchArray` and `L85_DockingMechanismSequencingPLC`, including command, actual position, travel time, limit switch, contact/latch load and fault. The model's visual rig currently has a single summary latch fraction; **physical certification must examine all six**, not the summary alone. A single stuck/misaligned latch creates asymmetric force/load and must block hard dock until repaired.

Hard dock requires all six safely positioned/locked under valid structural load, no residual unacceptable relative motion, seal seat alignment and system safety authorization. `L85_HardDockStructuralLoadMonitor` reads actual load. No direct setter `hardDock=true` on play, no one-click GREEN from LATCH button without measured feedback.


# 35. ELECTRICAL UMBILICAL, SEAL AND PRESSURE INTERFACE

`L85_DockingElectricalUmbilicalCoupler` must physically extend/engage only after valid latch/pose conditions, and its connection status must influence a **separate bounded docked-interface state** (station-vs-vehicle control and power handshake). Never treat a 5 V player cable as an unprotected power umbilical. `L85_PressureInterfaceSealGasket` and `L85_PressureLeakQualificationController` should represent a safe **fictional** pressure seal and leak-check game model: seat compression, modeled pressure differential, bounded leak-rate measurement, sensor calibration and elapsed test window. Do not provide real-world pressure-system operation instructions or imply flightworthiness.

Objective 4 needs stable soft capture, six independent locked latches, validated coupler continuity, measured seal qualification and no active safety fault—all on the same verified run and attached configuration.


# 36. CONTROLLED UNDOCK AND TEST RESTART

Objectives 4 and 5 require first and second complete dock runs; Objective 6's W123 deliberate loss must be tested **while the spacecraft is safely free or at an authorized hold point**, not by ripping a critical connection during an in-progress unsafe capture. Provide a safe, evidence-based controlled undock: confirm interface isolated, pressure-seal game test complete, latches unloaded, docking permission, thruster and escape reserve; retract umbilical, release latches and move to a defined safe separation using actual dynamics. If safe undock cannot be met, prohibit it and explain why. Preserve historical certification and start a **new runId** for the final W123 fault test. If W123 is unplugged while hard docked, keep attachment intact and inhibit any new undock/approach operation.


# 37. DISTURBANCE A — LIDAR BLIND SPOT

`L85_LiDARBlindSpotFaultEmulator` deliberately changes the real primary sensor measurement model for a subset of target geometry/angles/reflectance. Show dropout, stale samples, missing returns or biased range—not a decorative WARNING. The player compares primary/secondary LiDAR, stereo target pattern and radar range/rate; notices residual/covariance deterioration; stops approach, adjusts scanning geometry or selects safe valid fallback, recalibrates and verifies fresh full navigation. The fault must not change true spacecraft range. Log fault trigger, effected sensor samples, diagnosis, repair and a new safe docking run.


# 38. DISTURBANCE B — STUCK THRUSTER VALVE

`L85_StuckThrusterValveFaultEmulator` changes an actual valve's mechanical opening state (e.g. stuck-open or partly-open) independent of command. Effects: unexpected force/torque, drift, fuel flow/pressure change, estimator disagreement, target approach risk, reserve loss. Diagnosis uses requested vs actual valve, flow monitor, IMU acceleration and trajectory. Recovery: command protected isolation, confirm delivered thrust ceases as physically modeled, repair/replace fictional valve and recalibrate thrust allocation with available controls; then re-run complete qualified rendezvous. No invisible fuel or teleport to original pose. Make the recovery possible under the designed game scenario and reject impossible states with clear diagnostics.


# 39. DISTURBANCE C — ASYMMETRIC DOCKING LATCH

`L85_AsymmetricLatchFaultEmulator` changes **one actual latch travel/force/limit state**, creating mechanically unbalanced capture. PLC sees unequal latch feedback and contact forces; stops hard-capture sequencing and preserves soft capture/controlled release if safe. Diagnose using six-latch status, six-axis force sensor and structural load monitor. Repair/reset the physical latch with the mechanism de-energized/isolated in the simulated game state, rehome/test stroke, and carry out new soft-capture, six-lock, coupler, seal and full fresh docking qualification. Do not hide one broken latch behind a high average latch fraction.


# 40. MAIN PLC AND SAFETY MODE STATE MACHINE

Recommended states: `OFF`, `CONTROL_WIRING`, `SAFETY_POWER_CHECK`, `NAV_SENSOR_CALIBRATION`, `THRUSTER_CALIBRATION`, `RELATIVE_ESTIMATE_READY`, `RENDEZVOUS_PLANNING`, `RENDEZVOUS_APPROACH`, `HOLD_100M`, `HOLD_30M`, `FINAL_HOLD_5M`, `FINAL_APPROACH`, `CONTACT_DETECTED`, `SOFT_CAPTURE`, `LATCH_SEQUENCE`, `HARD_DOCKED`, `UMBILICAL_MATED`, `SEAL_QUALIFIED`, `DOCKED_CERTIFIED`, `SAFE_UNDOCK`, `DISTURBANCE_DIAGNOSIS`, `CONTROLLED_ABORT`, `SAFE_HOLD`, `CRITICAL_FEEDBACK_LOST`, `CRITICAL_FAULT_LATCHED`, `RESET_REQUIRED`, `POST_REPAIR_QUALIFICATION`, `FINAL_CERTIFIED`.

Every transition gates on actual topology, sensor quality/freshness, real pose/speed, actuator response, fuel/stopping margin, true contact state, six latch readings and safety acknowledgement. Commands may request transitions but cannot set physical conditions instantly. Design power-off/fault entry/exit with safe semantics; camera/SCADA progress is nonauthoritative.


# 41. MANDATORY W123 DISCONNECTION EVENT

Only after Objectives 1–5 are historically certified, perform a scenario-authorized deliberate final fault test. If docked, first conduct a **safe controlled undock** and move to a defined certified test hold; do not forcibly detach a spacecraft solely to exercise the wire. Then:

1. Player physically disconnects **real graph edge W123** using actual Project Spark wire UI.
2. Live circuit graph mutates and W123 continuity becomes false.
3. Trusted closing-rate samples expire or immediately invalidate via graph connectivity; cached data remains historical only.
4. `L85_DockingSafetyPLC` withdraws autonomous final-approach, capture and hard-latch authorization.
5. Protected hold/abort controller chooses physically feasible bounded response using independent sensor safety and remaining fuel, logs inability to retreat if actually impossible.
6. If craft already hard-docked, keep six latches/umbilical safe and block inappropriate undock commands rather than automatically pulling apart the structure.
7. Verify commands and **actual** motion/valve state, not just an abort-request flag.
8. Latch critical feedback loss and record distinct physical fault event ID, pre/post true pose/speed, independent safety and W126 measured abort/hold result.
9. Maintain historical Objectives 1–5 while displaying current mission unsafe/unqualified.
10. Prevent W124 SCADA, W121 health or W122 calibration channel from substituting for W123.


# 42. W123 REPAIR, FRESH CALIBRATION AND EXPLICIT RESET

Player reconnects the **same genuine W123** source and destination, rebuilding live solver continuity. Require valid source/receiver 5 V and returns, fresh nonreplayed timestamp/sequence, proper calibrated range-rate estimation and confidence, sensor fusion cross-check against radar/LiDAR under valid scene conditions, and separate independent safety/fuel reserve OK. A wire reconnected while the vehicle is drifting unsafely is **not** instant safe status.

Fault latch remains until the user commands explicit reset via source-planned W128 request and the PLC verifies all reset preconditions. Do not automatically clear critical fault merely because wiring is restored. Start a **brand-new mission runId** and require complete sensor commissioning (as configured), trajectory plan, all holds, controlled approach, true soft capture, six latches, coupler and seal qualification. **Objective 6 may pass only after this full fresh sequence.** Previous Objective 3–5 run events cannot be replayed.


# 43. OBJECTIVE 1 — CONSTRUCT THE RENDEZVOUS CONTROL NETWORK

From the exact source title: **Construct the Rendezvous Control Network**. Certify `1/6` only when all **146 real player-created edges** match the CSV's intended logical endpoints and live solver graph, each functional port is reconciled against 224 visual socket art/292 proposed logical contacts, correct 5 V domain and returns are present, navigation/thruster/capture/PLC control interfaces are powered, W123 is actively connected/valid, and independent safety is verified. Capture topology hash, required device health, invalid-connection count and reason codes. A circuit mockup with 146 preconnected glowing wires must fail.


# 44. OBJECTIVE 2 — CALIBRATE THE AUTONOMOUS NAVIGATION SYSTEM

**Calibrate the Autonomous Navigation System**. Certify `2/6` only after both LiDAR channels, stereo cameras, radar, IMU, beacon/target, synchronization and fusion confidence pass actual sampled calibration; relative position/velocity and uncertainty are trustworthy; six propulsion channels are calibrated with finite observed impulses and correct valve feedback. Test at least one validated finite drive maneuver and estimate its effect without overshoot. Keep calibration versions and time-stamped instrument certificates, not a green UI switch.


# 45. OBJECTIVE 3 — EXECUTE PRECISION ORBITAL RENDEZVOUS

**Execute Precision Orbital Rendezvous**. Certify `3/6` after a fresh run enters the 100 m/30 m/5 m (or currently configured) holds with valid dwell/velocity/uncertainty conditions, stays in the safe approach corridor, follows a physically integrated trajectory with finite propellant, detects any drift/overshoot and satisfies final approach gates. Use true/estimated relative state with documented tolerances and measured timestamps. No waypoint teleport, no moving visual ring into the craft, no instant `success=true` when HUD shows 0 m.


# 46. OBJECTIVE 4 — COMPLETE SOFT CAPTURE AND HARD DOCKING

**Complete Soft Capture and Hard Docking**. Certify `4/6` only when true contact pose/speed and loads are within fictional constraints, compliant capture actually engages/damps relative motion, **all six** individual latches are locked with valid independent feedback, hard-dock structural loads are acceptable, electrical umbilical actually mates/continuity checks, pressure seal proxy passes the measured test window, and the safety PLC grants authorization. Capture real run ID and each latch's evidence. A virtual animation of lock petals cannot certify hard docking alone.


# 47. OBJECTIVE 5 — SURVIVE THREE DOCKING-SYSTEM FAILURES

**Survive Three Docking-System Failures**. Each disturbance (LiDAR blindspot, stuck thruster valve, asymmetric latch) must have actually changed the plant/sensor state, produced relevant diagnosis evidence, been addressed through a real repair interaction and passed its subsystem verification. Then perform a **new full successful rendezvous-and-docking mission** after the last disturbance, not merely a 5-second 'all green' timer. Save physical state fingerprints, event IDs, calibration versions and the new run ID. Historical Objective 4 stays earned if a later fault temporarily makes current dock unsafe.


# 48. OBJECTIVE 6 — RESTORE CRITICAL CLOSING-RATE FEEDBACK

**Restore Critical Closing-Rate Feedback**. After historical `1–5/6`, safely arrange authorized free-flight fault conditions; intentionally unplug **actual W123**, demonstrate graph discontinuity → trusted sample invalid/stale → approach permission withdrawn → controlled physically feasible hold/abort → latched fault. Reconnect W123 through real wire tool, obtain new calibrated and cross-checked measurements, verify PLC/safety/propulsion, explicitly reset, and conduct a **completely new** full rendezvous/capture/latch/umbilical/seal run. Certify only if `postRepairRunId > faultRunId`, trusted W123 stayed good throughout qualification, no critical fault remains, and real `LevelGamePlayManager` records **6/6**.


# 49. HISTORICAL OBJECTIVE EVIDENCE, SAVE/LOAD AND RUN IDS

Keep separate `historicallyCertified`, `currentlyQualified`, `currentProcessState`, `faultLatched`, `runId`, `missionEvidenceSequence`, sensor calibration versions, topology fingerprints and each certification's time/event IDs. A deliberate last fault must not erase previously earned 1–5/6, but must make current approach unsafe and block Objective 6 until a fresh cycle. Persist through save/reload using the real project framework, refusing replayed stale sensor events or artificially advanced time. Idempotent editor reruns cannot award objectives or preconnect player edges. Include deterministic game-level retry/reset and reproducible fault seeds.


# 50. AAA SCADA AND COCKPIT — LIVE READINGS, NOT SUCCESS AUTHORITIES

`L85_HolographicRendezvousSCADA` should display true vs measured vs estimated relative range, range-rate sign, lateral/cross-track displacement, attitude/axis error, angular rate, covariance/confidence, sensor freshness/IDs, 100/30/5 m hold states, predicted stopping distance, station keep-out geometry, target and actual guidance, per-axis command, six valve actual open fractions, delivered thrust, propellant A/B mass/pressure, fuel abort reserve, six independent latch positions/contact force/limits, contact damper compression, coupler continuity, seal pressure/leak test, independent safety status, active fault evidence, W123 topology/sample age/trust, W124 display-only status, objective history and current 0/6–6/6 progress.

Use actual graph/process state. Provide timeline plots of relative range/range-rate, valve pulses, fuel reserve, covariance, contact force/latch engagement and fault event causal order. Distinguish command vs actual and valid vs stale readings visually. Do not let a glowing green meter or a cinematic camera authorize the next state.


# 51. PLAYER INTERACTIONS AND CONTROL PANELS

Player controls should include: `CONNECT WIRE`, `DISCONNECT`, `MAIN 5V`, `VERIFY RETURNS`, `SAFETY SELF TEST`, `CALIBRATE IMU`, `CALIBRATE STEREO`, `SCAN LiDAR A/B`, `RADAR ACQUIRE`, `TARGET BEACON LOCK`, `SYNC CLOCK`, `SENSOR FUSION CHECK`, `VALVE TEST`, `THRUSTER CALIBRATION`, `SET HOLD TARGET`, `GENERATE TRAJECTORY`, `COMPUTE FUEL RESERVE`, `ARM GUIDANCE`, `EXECUTE BURN`, `STOP / HOLD`, `SAFE ABORT`, `AUTH 100M`, `AUTH 30M`, `FINAL 5M`, `CONTACT MODE`, `SOFT CAPTURE`, `SEQUENCE SIX LATCHES`, `CONNECT UMBILICAL`, `SEAL CHECK`, `SAFE UNDOCK`, three targeted fault injectors, `DIAGNOSE`, `ISOLATE / REPAIR`, `REQUALIFY`, deliberate `W123 UNPLUG / RECONNECT` via **actual wire tool**, `CALIBRATE TRUSTED RATE`, `EXPLICIT SAFETY RESET`, `START FRESH DOCK RUN`, and `VIEW CERTIFICATION`.

Every control must call a real stateful operation, support permission/feedback, and visibly explain why a command is refused. No fake one-click mission complete actions.


# 52. AAA VISUAL DESIGN — THE LAST METER EXPERIENCE

Cinematic focal composition: service spacecraft in the left half of a vast orbital viewport, huge machined station docking port on the right, Earth and starfield backdrop, holographic green docking vector, physically limited RCS pulse VFX and close-up moving active/passive capture mechanisms. Foreground laboratory has accessible navigation, thruster, capture, safety PLC and power panels, real player wire paths and gold-rimmed sockets, premium dark titanium/satin aluminum/ceramic, muted cyan/amber/red emission and convincing shadowing.

Phase changes visually: blue tracking scans with honest occlusion; amber hold-point warnings; pulses only while real valves deliver thrust; a growing nav uncertainty cone during LiDAR fault; dynamic predicted collision arc; soft-contact mechanical compliance; six numbered latch LEDs from their actual feedback; red W123 fault with abort path; clean final docking qualification animation only after true objective evidence. Separate live external inspection camera, dock-port alignment camera and orbit trajectory hologram. Don't bake success statuses into textures.


# 53. SUGGESTED C# RUNTIME COMPONENT RESPONSIBILITIES

Implement/reuse only as needed after inspecting actual project code. Proposed names are **not claims of current classes**:

- `SparkLevel85Coordinator`, `SparkLevel85ProcessState`, `SparkLevel85ElectricalBindings`, `SparkLevel85TerminalMapper`, `SparkLevel85RunEvidence`.
- `SparkLevel85OrbitReference`, `SparkLevel85RelativeDynamics`, `SparkLevel85AttitudeDynamics`, `SparkLevel85ReferenceFrameAdapter`, `SparkLevel85VehicleMassProperties`.
- `SparkLevel85RCSActuators`, `SparkLevel85PropellantModel`, `SparkLevel85ValveDynamics`, `SparkLevel85ThrusterCalibration`, `SparkLevel85ThrusterAllocator`.
- `SparkLevel85LiDARModel`, `SparkLevel85StereoNavigation`, `SparkLevel85RendezvousRadar`, `SparkLevel85IMU`, `SparkLevel85BeaconTracking`, `SparkLevel85SensorClock`, `SparkLevel85RelativeStateEstimator`.
- `SparkLevel85TrajectoryPlanner`, `SparkLevel85GuidanceController`, `SparkLevel85ApproachCorridor`, `SparkLevel85CollisionPredictor`, `SparkLevel85IndependentDockSafety`, `SparkLevel85AbortController`.
- `SparkLevel85ContactDynamics`, `SparkLevel85SoftCapture`, `SparkLevel85SixLatchMechanism`, `SparkLevel85DockedUmbilical`, `SparkLevel85SealIntegrity`, `SparkLevel85DockSequencePLC`.
- `SparkLevel85DisturbanceController`, `SparkLevel85W123Recovery`, `SparkLevel85ObjectiveTracker`, `SparkLevel85RendezvousSCADA`, `SparkLevel85VisualRigAdapter`.

Prefer small testable interfaces and deterministic pure step functions for physics/measurement; one authoritative state owner. Don't make these 30 independent `Update()` components compete to set the same pose or objective completion.


# 54. ONE-CLICK FULL LEVEL 85 UNITY EDITOR BUILDER

Create **`Tools/Project Spark/Levels/Create Level 85`** (distinct from existing art-only menu) that inspects actual APIs and idempotently creates/updates `Level_85.asset`, `Level_85_Layout.prefab`, objectives and station/vehicle runtime hierarchy. It must:

1. Confirm all 78 exact numbered FBXs and convert scale once.
2. Place origin-aligned modular models under `L85_HD_VISUAL_ART`, using the existing art placement helper rather than silently generating new geometry.
3. Form `L85_DYNAMIC_SERVICE_SPACECRAFT` with correct modules, preserved transforms and no duplicated art showcase.
4. Bind/validate 60 actual pivot parents and their declared owner, type, axis.
5. Import 224 art anchors; reconcile into 292 source-derived logical terminal contacts; report 41 reused P location groups.
6. Create 146 legitimate player-wirable declarations and real `SparkTerminal` components, unconnected at start, with signal direction/type/domain and power/return checks.
7. Set up separate educational 5 V controls, fictional propulsion plant power, independent safety PLC and station umbilical power handshake.
8. Set up orbital-relative dynamics, state frame conversion, thruster/propellant, sensor simulation and fusion.
9. Build approach corridor, 100/30/5 holds, collision avoidance, independent abort and contact dynamics.
10. Build six-lock docking mechanism, pressure seal and electrical coupler, matching real interactions.
11. Add three plant-mutating disturbances and repairs, W123 disconnect/recalibration/reset and fresh run sequence.
12. Implement actual `LevelGamePlayManager` objective bindings and save/history evidence.
13. Bind read-only SCADA, user controls, camera switching, visual rig and VFX to real process values.
14. Save scene/prefab/assets with Undo where relevant, avoid duplicate solvers/ports and preserve customized gameplay objects outside disposable art root.
15. Run structural validation, wiring reconciliation and any available EditMode/PlayMode tests; write an honest report with PASS/FAIL/NOT_RUN.


# 55. ASSET AND WIRING ACCEPTANCE TESTS

Where executable in the actual Unity installation, verify:

1. Exactly 78 distinct numbered FBXs and 78 corresponding GLBs in source; one assembled alternate reference.
2. Art scale and overall stage bounds ~29.2 × 10.128 × 15.7 m after single conversion.
3. Exactly 60 unique `ANIM_*` pivot names with correct owner/type/axis, no duplicate transforms.
4. Source 224 `SOCKET_P###` art markers and all 292 planned logical contacts reconciled.
5. All 41 reused art anchors mapped to independent safe logical contacts; specifically W080 and W123 source at P148 not accidentally shorted.
6. 146 unique correct player-connectable wire definitions, no puzzle wires preinstalled.
7. Correct 5 V supply/returns and type/direction checks; actual graph connection affects machine operation.
8. W123 real detachable critical circuit edge and no alternate diagnostic/SCADA authority.
9. Existing optional art placement/material/asset validator menus work only as documented.
10. Art rerun preserves gameplay objects, 5 V solver, save state and terminal definitions.
11. No double FBX scale or duplicate assembled showcase meshes.
12. Levels 1–84 continue loading and executing unmodified.

Mark **NOT_RUN** if Unity project testing is unavailable. ZIP structural verification alone is not proof of these Unity tests.


# 56. RELATIVE FLIGHT, THRUSTER AND NAVIGATION ACCEPTANCE TESTS

13. True station-relative pose and velocity integrate with time and controlled forces; no instant commanded pose.
14. LVLH physics convention is distinct from stage X-approach mapping and tested across all axes.
15. Zero thrust with nonzero velocity continues coasting; STOP does not instantaneously zero momentum.
16. ±X/Y/Z thruster deliveries match real nozzle vectors and allocation, including any actual torque authority constraints.
17. Valve actual position lags command and stuck-on valve causes unintended force/fuel loss.
18. Propellant mass cannot be negative; abort uses physically available reserve.
19. Sensor LiDAR range reacts to actual geometry, scan angle and optical occlusion.
20. Primary/secondary LiDAR, stereo, radar and IMU have distinct uncertainty/dropout models and timestamps.
21. IMU bias/drift and calibration affect estimates, not true plant pose.
22. Sensor fusion uncertainty grows during dropout, recovers only on valid measured samples.
23. W123 sequence and age check fail for replayed/stale samples.
24. A 100/30/5 hold is not achieved by high-speed waypoint crossing.
25. Keep-out and stopping-distance tests respond to actual speed, mass, fuel and uncertainty.
26. Infeasible thruster/fuel conditions prevent unauthorized approach and are reported, not silently corrected.


# 57. DOCKING, FAULT AND CRITICAL SAFETY ACCEPTANCE TESTS

27. Contact requires actual relative pose, speed, collision shape and physical force response.
28. Too-fast/off-axis contact does not magically soft-capture or hard-dock.
29. Compliant capture dissipates relative kinetic energy over finite time.
30. Six individual latches have independent measured travel/lock/load evidence; one stuck latch blocks hard dock.
31. Structural load monitor reflects actual modeled contact/capture forces.
32. Electrical coupler continuity is earned only after proper mating authorization and physical extension.
33. Fictional pressure seal requires sustained qualified measurement; a cosmetic ring contact fails.
34. Safe undock actually releases/retreats through permitted sequence and preserves previous objective history.
35. LiDAR blindspot causes real invalid/biased samples, diagnosable by independent instruments.
36. Stuck valve causes real command-vs-force mismatch and is recoverable only after actual isolation/repair.
37. Asymmetric latch changes at least one real latch state and contact-load response.
38. A new full docking run after all three repairs qualifies Objective 5; no timer-only shortcut.
39. Disconnect actual W123 through player wire UI; graph continuity becomes false.
40. Lost/stale W123 withdraws final approach/hard-capture permission; W124/W121/W122 cannot bypass it.
41. Critical fault latch stays active during missing W123 and rejects reset.
42. If free in flight, controlled hold/abort uses real remaining thrust and fuel; no teleport.
43. If already hard-docked, W123 loss does not automatically release locks or undock.
44. Reconnect W123, fresh sequence/timestamp, recalibrate and cross-check; explicit operator reset mandatory.
45. Reconnection alone never certifies Objective 6.
46. Fresh post-repair run includes new hold-point, approach, soft capture, all six locks, coupler and seal evidence.
47. Historical Objectives 1–5 remain certified across fault/reload; current machine health may be unsafe.
48. Actual `LevelGamePlayManager` progresses 0/6 → 6/6 only through valid gameplay actions.
49. All Unity compiler, editor and Play Mode results are reported accurately.


# 58. DIAGNOSTIC MESSAGES AND VALIDATION REPORT

Report actionable errors: missing model/FBX scale, wrong pivot owner/type, duplicated `SOCKET_P###`, unbound spark terminal, logical port conflation at W080/W123, missing 5 V return, invalid wire direction/type, expired sensor calibration, failed LiDAR/IMU/radar sample, unsynchronized beacon, sensor covariance too high, wrong range-rate sign, no target capture, out-of-corridor, dangerous closing speed, lateral/angular error, depleted fuel/abort impossible, thruster valve stuck, wrong thrust axis/insufficient torque, unsafe keep-out approach, mechanical collision, high contact force, one of six latches open, coupler incomplete, seal leak test failed, lost W123, stale/cached W123, reset preconditions incomplete, false W124 substitute, abort not physically achieved, wrong run ID, objectives stuck at 0/6 or 5/6 and prior-level regression. Write a source/import report, terminal reconciliation CSV, circuit topology report, deterministic mission trace and executed test matrix. **Never invent green test results.**


# 59. FULL REQUIRED GAMEPLAY WALKTHROUGH

**IMPORT 78 REAL MODULAR FBXS → VERIFY 60 PIVOTS + 224 SOCKET MARKERS → RECONCILE 292 LOGICAL CONTACTS → CONNECT 146 PLAYER-CREATED REAL WIRES → VERIFY 5 V CONTROL RETURNS + INDEPENDENT SAFETY → CALIBRATE IMU → CALIBRATE STEREO PAIR → HOME BOTH LIDARS → QUALIFY RADAR AND TARGET BEACONS → CLOCK SYNC → SENSOR FUSION / COVARIANCE CHECK → CALIBRATE SIX THRUSTERS AND VALVES → PLAN RELATIVE ORBIT TRAJECTORY → COMPUTE FEASIBLE FUEL/ABORT RESERVE → START REAL RUN → REACH AND STABILIZE 100 m HOLD → 30 m HOLD → 5 m HOLD → CONFIRM TRUSTED W123 → ENTER APPROACH CORRIDOR → BRAKE REAL VELOCITY → VERIFY LATERAL/ANGULAR POSE → CONTACT → COMPLIANT SOFT CAPTURE → LOCK SIX MEASURED LATCHES → MATE ELECTRICAL UMBILICAL → VERIFY FICTIONAL PRESSURE SEAL → CERTIFY FIRST DOCK → INJECT LIDAR BLINDSPOT / DIAGNOSE / REPAIR → INJECT STUCK THRUSTER VALVE / ISOLATE / REPAIR → INJECT ASYMMETRIC LATCH / RELEASE / REPAIR → COMPLETE FRESH FULL DOCK RUN → CERTIFY 5/6 → CONTROLLED SAFE UNDOCK → MOVE TO AUTHORIZED FAULT-TEST HOLD → PHYSICALLY DISCONNECT W123 → STALE CRITICAL CLOSING RATE → APPROACH INHIBIT → REAL PHYSICALLY FEASIBLE SAFE HOLD/ABORT → CRITICAL FAULT LATCH → PHYSICALLY RECONNECT W123 → COLLECT NEW CALIBRATED MEASUREMENTS → VERIFY INDEPENDENT SAFETY AND FUEL → EXPLICIT SAFETY RESET → START NEW POST-REPAIR MISSION RUN → NEW 100/30/5 HOLDS → COMPLETE FRESH APPROACH → REAL SOFT CAPTURE → SIX LATCHES → UMBILICAL/SEAL → AUTHORITATIVE LEVEL MANAGER 6/6.**


# 60. DELIVERABLES AND HONEST FINAL SUCCESS CONTRACT

Deliver working/validated Unity C# source and actual project assets (subject to access to the user's Unity project): `Level_85.asset`, `Level_85_Layout.prefab`, unique art visual root, 78 modular models with correct import scale and authorable materials, 60 validated pivot bindings, 224 decorative anchors with 292 reconciled logical terminal interfaces, 146 genuine player wire declarations (initially unconnected), complete 5 V controls and returns, relative-orbit/vehicle dynamics, propulsion/sensor fusion/control/abort, physical capture/six latches/umbilical/seal, three disturbances, real W123 physical disconnection/reconnection and explicit reset, objective evaluator/save history/SCADA, one-click Editor builder, QA reports and performance assessment.

A final Unity implementation may be called **Level 85 complete** only when actual compilation and Play Mode demonstrate that the human player can follow the electrical/safety/docking workflow, deliberate W123 disconnect truly inhibits approach, repair/reset enables fresh full requalification, and **the authoritative `LevelGamePlayManager` displays 6/6** without modifying Levels 1–84. If the actual Unity project isn't available, deliver the source/scripts/technical plan clearly labeled **uncompiled / not Play Mode tested**, not a fake completed game.

**PROJECT SPARK — LEVEL 85: THE LAST METER. TWO SPACECRAFT. ONE ORBIT. ZERO ROOM FOR ERROR. NAVIGATE. APPROACH. CAPTURE. DOCK.**


---

# SOURCE-DERIVED ANNEXES — EXACT ASSET/CONNECTION RECORDS

These annexes reproduce data read from the generated Level 85 package. The original JSON/CSV files remain authoritative; statuses such as MESH_ONLY and CONCEPT_ONLY mean **not yet functional Unity gameplay**.


## ANNEX A — ALL 78 EXACT MODULAR FBX/GLB FILENAMES AND MESH METRICS

| # | Exact FBX file | Corresponding GLB | Triangles | Mesh parts |
|---:|---|---|---:|---:|
| 00 | `00_L85_OrbitalDockingArenaBase.fbx` | `00_L85_OrbitalDockingArenaBase.glb` | 4460 | 29 |
| 01 | `01_L85_PanoramicEarthAndStarfieldViewport.fbx` | `01_L85_PanoramicEarthAndStarfieldViewport.glb` | 7556 | 27 |
| 02 | `02_L85_StationOrbitalTrussSpine.fbx` | `02_L85_StationOrbitalTrussSpine.glb` | 10528 | 6 |
| 03 | `03_L85_StationPrimaryDockingPort.fbx` | `03_L85_StationPrimaryDockingPort.glb` | 6960 | 8 |
| 04 | `04_L85_DockingOuterGuideRing.fbx` | `04_L85_DockingOuterGuideRing.glb` | 5680 | 7 |
| 05 | `05_L85_DockingPassiveCaptureCone.fbx` | `05_L85_DockingPassiveCaptureCone.glb` | 1392 | 2 |
| 06 | `06_L85_StationOpticalTargetMarkerArray.fbx` | `06_L85_StationOpticalTargetMarkerArray.glb` | 928 | 6 |
| 07 | `07_L85_StationSolarWingModule.fbx` | `07_L85_StationSolarWingModule.glb` | 908 | 8 |
| 08 | `08_L85_StationThermalRadiatorWing.fbx` | `08_L85_StationThermalRadiatorWing.glb` | 908 | 7 |
| 09 | `09_L85_StationApproachBeaconCluster.fbx` | `09_L85_StationApproachBeaconCluster.glb` | 1248 | 5 |
| 10 | `10_L85_StationServiceAirlockModule.fbx` | `10_L85_StationServiceAirlockModule.glb` | 1652 | 4 |
| 11 | `11_L85_StationDockingPowerInterface.fbx` | `11_L85_StationDockingPowerInterface.glb` | 888 | 11 |
| 12 | `12_L85_StationCaptureStructuralFrame.fbx` | `12_L85_StationCaptureStructuralFrame.glb` | 5532 | 7 |
| 13 | `13_L85_ThreeDimensionalApproachCorridorHologram.fbx` | `13_L85_ThreeDimensionalApproachCorridorHologram.glb` | 16484 | 7 |
| 14 | `14_L85_RelativeOrbitTrajectoryHologram.fbx` | `14_L85_RelativeOrbitTrajectoryHologram.glb` | 5340 | 6 |
| 15 | `15_L85_EarthOrbitalReferenceHologram.fbx` | `15_L85_EarthOrbitalReferenceHologram.glb` | 3800 | 2 |
| 16 | `16_L85_ExternalInspectionCameraGantry.fbx` | `16_L85_ExternalInspectionCameraGantry.glb` | 2444 | 9 |
| 17 | `17_L85_StationApproachIlluminationRig.fbx` | `17_L85_StationApproachIlluminationRig.glb` | 2444 | 9 |
| 18 | `18_L85_AutonomousServiceSpacecraftBody.fbx` | `18_L85_AutonomousServiceSpacecraftBody.glb` | 7656 | 8 |
| 19 | `19_L85_ServiceSpacecraftExtendableDockingProbe.fbx` | `19_L85_ServiceSpacecraftExtendableDockingProbe.glb` | 1232 | 3 |
| 20 | `20_L85_ProtectedServiceVehicleAvionics.fbx` | `20_L85_ProtectedServiceVehicleAvionics.glb` | 300 | 4 |
| 21 | `21_L85_ServiceSpacecraftPortSolarWing.fbx` | `21_L85_ServiceSpacecraftPortSolarWing.glb` | 832 | 8 |
| 22 | `22_L85_ServiceSpacecraftStarboardSolarWing.fbx` | `22_L85_ServiceSpacecraftStarboardSolarWing.glb` | 832 | 8 |
| 23 | `23_L85_ReactionControlPropellantManifold.fbx` | `23_L85_ReactionControlPropellantManifold.glb` | 832 | 11 |
| 24 | `24_L85_RCSPlusXThruster.fbx` | `24_L85_RCSPlusXThruster.glb` | 1676 | 9 |
| 25 | `25_L85_RCSMinusXThruster.fbx` | `25_L85_RCSMinusXThruster.glb` | 1676 | 9 |
| 26 | `26_L85_RCSPlusYThruster.fbx` | `26_L85_RCSPlusYThruster.glb` | 1676 | 9 |
| 27 | `27_L85_RCSMinusYThruster.fbx` | `27_L85_RCSMinusYThruster.glb` | 1676 | 9 |
| 28 | `28_L85_RCSPlusZThruster.fbx` | `28_L85_RCSPlusZThruster.glb` | 1676 | 9 |
| 29 | `29_L85_RCSMinusZThruster.fbx` | `29_L85_RCSMinusZThruster.glb` | 1676 | 9 |
| 30 | `30_L85_PropellantTankA.fbx` | `30_L85_PropellantTankA.glb` | 2980 | 5 |
| 31 | `31_L85_PropellantTankB.fbx` | `31_L85_PropellantTankB.glb` | 2980 | 5 |
| 32 | `32_L85_ThrusterValveDriverRack.fbx` | `32_L85_ThrusterValveDriverRack.glb` | 1704 | 26 |
| 33 | `33_L85_ThrusterFlowAndPressureMonitor.fbx` | `33_L85_ThrusterFlowAndPressureMonitor.glb` | 1320 | 18 |
| 34 | `34_L85_PrecisionThrusterCalibrationFixture.fbx` | `34_L85_PrecisionThrusterCalibrationFixture.glb` | 1020 | 12 |
| 35 | `35_L85_SpacecraftInertialMeasurementUnit.fbx` | `35_L85_SpacecraftInertialMeasurementUnit.glb` | 2580 | 12 |
| 36 | `36_L85_StereoNavigationCameraLeft.fbx` | `36_L85_StereoNavigationCameraLeft.glb` | 2388 | 8 |
| 37 | `37_L85_StereoNavigationCameraRight.fbx` | `37_L85_StereoNavigationCameraRight.glb` | 2388 | 8 |
| 38 | `38_L85_PrimaryScanningLiDAR.fbx` | `38_L85_PrimaryScanningLiDAR.glb` | 4828 | 15 |
| 39 | `39_L85_SecondaryScanningLiDAR.fbx` | `39_L85_SecondaryScanningLiDAR.glb` | 4732 | 13 |
| 40 | `40_L85_ShortRangeRendezvousRadar.fbx` | `40_L85_ShortRangeRendezvousRadar.glb` | 2436 | 11 |
| 41 | `41_L85_DockingBeaconReceiver.fbx` | `41_L85_DockingBeaconReceiver.glb` | 2580 | 12 |
| 42 | `42_L85_OpticalTargetPatternTracker.fbx` | `42_L85_OpticalTargetPatternTracker.glb` | 2868 | 18 |
| 43 | `43_L85_RelativePositionVelocityEstimator.fbx` | `43_L85_RelativePositionVelocityEstimator.glb` | 1224 | 21 |
| 44 | `44_L85_RelativeNavigationFusionComputer.fbx` | `44_L85_RelativeNavigationFusionComputer.glb` | 1896 | 35 |
| 45 | `45_L85_NavigationUncertaintyVolumeDisplay.fbx` | `45_L85_NavigationUncertaintyVolumeDisplay.glb` | 3936 | 9 |
| 46 | `46_L85_PrecisionFlightTimeReferenceClock.fbx` | `46_L85_PrecisionFlightTimeReferenceClock.glb` | 828 | 13 |
| 47 | `47_L85_RelativeTrajectoryPlanningComputer.fbx` | `47_L85_RelativeTrajectoryPlanningComputer.glb` | 1692 | 20 |
| 48 | `48_L85_ProximityOperationsGuidanceComputer.fbx` | `48_L85_ProximityOperationsGuidanceComputer.glb` | 2172 | 30 |
| 49 | `49_L85_SixDegreeFreedomFlightController.fbx` | `49_L85_SixDegreeFreedomFlightController.glb` | 2268 | 31 |
| 50 | `50_L85_ThrusterForceAllocationComputer.fbx` | `50_L85_ThrusterForceAllocationComputer.glb` | 1980 | 25 |
| 51 | `51_L85_DockingCorridorSafetyGuardian.fbx` | `51_L85_DockingCorridorSafetyGuardian.glb` | 2076 | 28 |
| 52 | `52_L85_CollisionAvoidancePredictionMonitor.fbx` | `52_L85_CollisionAvoidancePredictionMonitor.glb` | 1500 | 16 |
| 53 | `53_L85_FuelReserveAndAbortPredictor.fbx` | `53_L85_FuelReserveAndAbortPredictor.glb` | 1692 | 20 |
| 54 | `54_L85_ProtectedAutonomousAbortController.fbx` | `54_L85_ProtectedAutonomousAbortController.glb` | 1788 | 22 |
| 55 | `55_L85_ActiveSoftCaptureRing.fbx` | `55_L85_ActiveSoftCaptureRing.glb` | 6304 | 11 |
| 56 | `56_L85_MotorizedCaptureGuidePetals.fbx` | `56_L85_MotorizedCaptureGuidePetals.glb` | 1248 | 6 |
| 57 | `57_L85_SixActuatorDockingLatchArray.fbx` | `57_L85_SixActuatorDockingLatchArray.glb` | 768 | 7 |
| 58 | `58_L85_SixAxisContactForceSensor.fbx` | `58_L85_SixAxisContactForceSensor.glb` | 672 | 5 |
| 59 | `59_L85_CompliantContactDampingActuator.fbx` | `59_L85_CompliantContactDampingActuator.glb` | 1216 | 6 |
| 60 | `60_L85_DockingElectricalUmbilicalCoupler.fbx` | `60_L85_DockingElectricalUmbilicalCoupler.glb` | 844 | 7 |
| 61 | `61_L85_PressureInterfaceSealGasket.fbx` | `61_L85_PressureInterfaceSealGasket.glb` | 3072 | 6 |
| 62 | `62_L85_PressureLeakQualificationController.fbx` | `62_L85_PressureLeakQualificationController.glb` | 924 | 15 |
| 63 | `63_L85_HardDockStructuralLoadMonitor.fbx` | `63_L85_HardDockStructuralLoadMonitor.glb` | 5776 | 11 |
| 64 | `64_L85_DockingMechanismSequencingPLC.fbx` | `64_L85_DockingMechanismSequencingPLC.glb` | 1788 | 31 |
| 65 | `65_L85_PrecisionDockingCameraMonitor.fbx` | `65_L85_PrecisionDockingCameraMonitor.glb` | 1320 | 24 |
| 66 | `66_L85_LiDARBlindSpotFaultEmulator.fbx` | `66_L85_LiDARBlindSpotFaultEmulator.glb` | 1044 | 18 |
| 67 | `67_L85_StuckThrusterValveFaultEmulator.fbx` | `67_L85_StuckThrusterValveFaultEmulator.glb` | 948 | 16 |
| 68 | `68_L85_AsymmetricLatchFaultEmulator.fbx` | `68_L85_AsymmetricLatchFaultEmulator.glb` | 852 | 14 |
| 69 | `69_L85_CriticalClosingRateFeedbackStation.fbx` | `69_L85_CriticalClosingRateFeedbackStation.glb` | 1044 | 16 |
| 70 | `70_L85_DockingSafetyPLC.fbx` | `70_L85_DockingSafetyPLC.glb` | 2100 | 36 |
| 71 | `71_L85_Protected5VControlSupply.fbx` | `71_L85_Protected5VControlSupply.glb` | 1032 | 14 |
| 72 | `72_L85_DockingPowerDistributionCabinet.fbx` | `72_L85_DockingPowerDistributionCabinet.glb` | 840 | 10 |
| 73 | `73_L85_HolographicRendezvousSCADA.fbx` | `73_L85_HolographicRendezvousSCADA.glb` | 4668 | 21 |
| 74 | `74_L85_ElectricalTerminalDiagnosticRack.fbx` | `74_L85_ElectricalTerminalDiagnosticRack.glb` | 1224 | 18 |
| 75 | `75_L85_EmergencyApproachHoldStation.fbx` | `75_L85_EmergencyApproachHoldStation.glb` | 864 | 12 |
| 76 | `76_L85_DockingMissionCertificationBeacon.fbx` | `76_L85_DockingMissionCertificationBeacon.glb` | 1804 | 9 |
| 77 | `77_L85_PrecisionFlightEventDataRecorder.fbx` | `77_L85_PrecisionFlightEventDataRecorder.glb` | 1032 | 15 |

Assembled **reference only**, not to instantiate with the 78 modules: `Models/Level85_Complete_ArtKit_Showcase.fbx` and `GLB/Level85_Complete_ArtKit_Showcase.glb`.


## ANNEX B — ALL 60 ACTUAL ANIMATION PIVOTS WITH OWNER/AXIS/POSITION

| # | Original `ANIM_*` pivot | Owning model | Kind | Axis | XYZ (metres) |
|---:|---|---|---|:---:|---|
| 01 | `ANIM_STATION_DOCK_RING_GLOW` | `L85_StationPrimaryDockingPort` | indicator | none | `(3.35, 5.05, -1.45)` |
| 02 | `ANIM_OUTER_DOCK_GUIDE` | `L85_DockingOuterGuideRing` | rotation | x | `(3.15, 5.05, -1.45)` |
| 03 | `ANIM_PASSIVE_CAPTURE_CONE` | `L85_DockingPassiveCaptureCone` | translation | x | `(2.92, 5.05, -1.45)` |
| 04 | `ANIM_TARGET_MARKER_SCAN` | `L85_StationOpticalTargetMarkerArray` | indicator | none | `(3.0, 5.05, -1.45)` |
| 05 | `ANIM_STATION_SOLAR_TRACK` | `L85_StationSolarWingModule` | rotation | y | `(8.1, 5.6, -1.3)` |
| 06 | `ANIM_RADIATOR_DEPLOY` | `L85_StationThermalRadiatorWing` | rotation | x | `(10.5, 3.65, -1.3)` |
| 07 | `ANIM_STATION_BEACON_ROTATION` | `L85_StationApproachBeaconCluster` | rotation | x | `(5.8, 5.05, -1.4)` |
| 08 | `ANIM_STATION_SERVICE_HATCH` | `L85_StationServiceAirlockModule` | rotation | y | `(8.0, 5.05, -1.5)` |
| 09 | `ANIM_POWER_UMBILICAL_PORT` | `L85_StationDockingPowerInterface` | translation | x | `(5.2, 3.8, 0.15)` |
| 10 | `ANIM_STATION_CAPTURE_FRAME` | `L85_StationCaptureStructuralFrame` | indicator | none | `(4.2, 5.05, -1.45)` |
| 11 | `ANIM_APPROACH_CORRIDOR_VECTOR` | `L85_ThreeDimensionalApproachCorridorHologram` | indicator | none | `(0, 5.05, -1.45)` |
| 12 | `ANIM_RELATIVE_TRAJECTORY` | `L85_RelativeOrbitTrajectoryHologram` | indicator | none | `(0, 7.8, -5.3)` |
| 13 | `ANIM_EARTH_REFERENCE` | `L85_EarthOrbitalReferenceHologram` | rotation | y | `(-9.8, 7.65, -5.8)` |
| 14 | `ANIM_INSPECTION_CAMERA_PAN` | `L85_ExternalInspectionCameraGantry` | rotation | y | `(5.8, 7.1, -0.55)` |
| 15 | `ANIM_INSPECTION_LIGHT_FLOOD` | `L85_StationApproachIlluminationRig` | rotation | x | `(1.0, 7.1, -3.1)` |
| 16 | `ANIM_SERVICE_SPACECRAFT_BODY` | `L85_AutonomousServiceSpacecraftBody` | quaternion | xyz | `(-4.8, 5.05, -1.45)` |
| 17 | `ANIM_SERVICE_DOCK_PROBE` | `L85_ServiceSpacecraftExtendableDockingProbe` | translation | x | `(-2.83, 5.05, -1.45)` |
| 18 | `ANIM_SERVICE_PORT_SOLAR` | `L85_ServiceSpacecraftPortSolarWing` | rotation | x | `(-5.3, 6.35, -1.4)` |
| 19 | `ANIM_SERVICE_STARBOARD_SOLAR` | `L85_ServiceSpacecraftStarboardSolarWing` | rotation | x | `(-5.3, 3.75, -1.4)` |
| 20 | `ANIM_RCS_PLUS_X` | `L85_RCSPlusXThruster` | indicator | none | `(-4.2, 5.77, -0.5)` |
| 21 | `ANIM_RCS_MINUS_X` | `L85_RCSMinusXThruster` | indicator | none | `(-5.7, 4.38, -0.5)` |
| 22 | `ANIM_RCS_PLUS_Y` | `L85_RCSPlusYThruster` | indicator | none | `(-4.8, 5.95, -1.6)` |
| 23 | `ANIM_RCS_MINUS_Y` | `L85_RCSMinusYThruster` | indicator | none | `(-4.8, 4.12, -1.6)` |
| 24 | `ANIM_RCS_PLUS_Z` | `L85_RCSPlusZThruster` | indicator | none | `(-5.45, 5.05, 0.25)` |
| 25 | `ANIM_RCS_MINUS_Z` | `L85_RCSMinusZThruster` | indicator | none | `(-4.25, 5.05, -2.9)` |
| 26 | `ANIM_THRUSTER_VALVE_BANK` | `L85_ThrusterValveDriverRack` | translation | x | `(-11.4, 1.72, 4.6)` |
| 27 | `ANIM_FLOW_MONITOR` | `L85_ThrusterFlowAndPressureMonitor` | indicator | none | `(-10.03, 1.72, 4.6)` |
| 28 | `ANIM_THRUST_CALIBRATION_RIG` | `L85_PrecisionThrusterCalibrationFixture` | rotation | y | `(-8.63, 1.72, 4.6)` |
| 29 | `ANIM_IMU_REFERENCE` | `L85_SpacecraftInertialMeasurementUnit` | rotation | z | `(-3.75, 4.55, -0.92)` |
| 30 | `ANIM_STEREO_CAMERA_LEFT` | `L85_StereoNavigationCameraLeft` | rotation | y | `(-3.25, 5.37, -0.9)` |
| 31 | `ANIM_STEREO_CAMERA_RIGHT` | `L85_StereoNavigationCameraRight` | rotation | y | `(-3.25, 4.75, -0.9)` |
| 32 | `ANIM_LIDAR_A_SCANNER` | `L85_PrimaryScanningLiDAR` | rotation | y | `(-2.9, 5.57, -1.05)` |
| 33 | `ANIM_LIDAR_B_SCANNER` | `L85_SecondaryScanningLiDAR` | rotation | y | `(-2.9, 4.55, -1.05)` |
| 34 | `ANIM_RADAR_DISH` | `L85_ShortRangeRendezvousRadar` | rotation | y | `(-3.2, 5.05, -2.15)` |
| 35 | `ANIM_BEACON_RECEIVER` | `L85_DockingBeaconReceiver` | indicator | none | `(-2.3, 6.2, -1.3)` |
| 36 | `ANIM_TARGET_PATTERN_TRACK` | `L85_OpticalTargetPatternTracker` | indicator | none | `(-2.33, 4.7, -1.3)` |
| 37 | `ANIM_RELATIVE_VELOCITY_VECTOR` | `L85_RelativePositionVelocityEstimator` | indicator | none | `(-7.25, 1.72, 4.6)` |
| 38 | `ANIM_NAV_SENSOR_FUSION` | `L85_RelativeNavigationFusionComputer` | indicator | none | `(-5.91, 1.72, 4.6)` |
| 39 | `ANIM_NAV_UNCERTAINTY_CONE` | `L85_NavigationUncertaintyVolumeDisplay` | indicator | none | `(-8.2, 6.6, -5.7)` |
| 40 | `ANIM_TRAJECTORY_PLANNER` | `L85_RelativeTrajectoryPlanningComputer` | indicator | none | `(-3.22, 1.72, 4.6)` |
| 41 | `ANIM_GUIDANCE_SOLUTION` | `L85_ProximityOperationsGuidanceComputer` | indicator | none | `(-1.87, 1.72, 4.6)` |
| 42 | `ANIM_SIX_DOF_CONTROL` | `L85_SixDegreeFreedomFlightController` | indicator | none | `(-0.52, 1.72, 4.6)` |
| 43 | `ANIM_THRUSTER_ALLOCATION` | `L85_ThrusterForceAllocationComputer` | indicator | none | `(0.83, 1.72, 4.6)` |
| 44 | `ANIM_COLLISION_BOUNDARY` | `L85_CollisionAvoidancePredictionMonitor` | indicator | none | `(3.53, 1.72, 4.6)` |
| 45 | `ANIM_ABORT_TRAJECTORY` | `L85_ProtectedAutonomousAbortController` | indicator | none | `(6.23, 1.72, 4.6)` |
| 46 | `ANIM_ACTIVE_SOFT_CAPTURE_RING` | `L85_ActiveSoftCaptureRing` | translation | x | `(2.66, 5.05, -1.45)` |
| 47 | `ANIM_CAPTURE_GUIDE_PETALS` | `L85_MotorizedCaptureGuidePetals` | rotation | x | `(2.66, 5.05, -1.45)` |
| 48 | `ANIM_DOCKING_LATCH_BANK` | `L85_SixActuatorDockingLatchArray` | translation | x | `(2.66, 5.05, -1.45)` |
| 49 | `ANIM_CONTACT_DAMPING` | `L85_CompliantContactDampingActuator` | translation | x | `(2.6, 5.05, -1.45)` |
| 50 | `ANIM_ELECTRICAL_UMBILICAL` | `L85_DockingElectricalUmbilicalCoupler` | translation | x | `(2.6, 5.05, -1.45)` |
| 51 | `ANIM_PRESSURE_SEAL` | `L85_PressureInterfaceSealGasket` | translation | x | `(2.65, 5.05, -1.45)` |
| 52 | `ANIM_DOCK_CAMERA_FEED` | `L85_PrecisionDockingCameraMonitor` | indicator | none | `(10.1, 4.2, -5.5)` |
| 53 | `ANIM_LIDAR_BLINDSPOT_FAULT` | `L85_LiDARBlindSpotFaultEmulator` | translation | x | `(10.32, 1.72, 4.6)` |
| 54 | `ANIM_STUCK_VALVE_FAULT` | `L85_StuckThrusterValveFaultEmulator` | translation | x | `(11.68, 1.72, 4.6)` |
| 55 | `ANIM_ASYMMETRIC_LATCH_FAULT` | `L85_AsymmetricLatchFaultEmulator` | translation | x | `(8.35, 3.72, 4.6)` |
| 56 | `ANIM_W123_TRUSTED_FEEDBACK` | `L85_CriticalClosingRateFeedbackStation` | indicator | none | `(-11.34, 3.6, 4.6)` |
| 57 | `ANIM_INDEPENDENT_DOCK_SAFETY` | `L85_DockingSafetyPLC` | indicator | none | `(-10.0, 3.6, 4.6)` |
| 58 | `ANIM_APPROACH_EMERGENCY_HOLD` | `L85_EmergencyApproachHoldStation` | translation | z | `(-4.62, 3.6, 4.6)` |
| 59 | `ANIM_RENDEZVOUS_SCADA` | `L85_HolographicRendezvousSCADA` | indicator | none | `(0, 8.5, -5.72)` |
| 60 | `ANIM_DOCK_CERTIFICATION` | `L85_DockingMissionCertificationBeacon` | indicator | none | `(0, 9.2, -5.75)` |

Every pivot row is `REAL_MESH_OWNING_FBX_AND_GLB_PIVOT`. Indicator/visual nodes are not physical state setters. The original art rig currently animates a subset of pivots.


## ANNEX C — ALL 146 SOURCE-AUTHORED WIRE ROUTES AND PURPOSES

| Wire ID | Planned source model / art anchor | Planned destination model / art anchor | Signal kind | Exact planned purpose |
|---|---|---|---|---|
| `W001` | `L85_Protected5VControlSupply` / `P114` | `L85_DockingSafetyPLC` / `P050` | `POWER_CONTROL` | Docking safety PLC control power |
| `W002` | `L85_Protected5VControlSupply` / `P115` | `L85_DockingMechanismSequencingPLC` / `P035` | `POWER_CONTROL` | Capture sequencing PLC 5V logic |
| `W003` | `L85_Protected5VControlSupply` / `P114` | `L85_RelativeNavigationFusionComputer` / `P160` | `POWER_CONTROL` | Navigation fusion computer 5V |
| `W004` | `L85_Protected5VControlSupply` / `P115` | `L85_SixDegreeFreedomFlightController` / `P190` | `POWER_CONTROL` | Six degree flight control logic 5V |
| `W005` | `L85_Protected5VControlSupply` / `P114` | `L85_ThrusterForceAllocationComputer` / `P214` | `POWER_CONTROL` | Thruster allocation control 5V |
| `W006` | `L85_Protected5VControlSupply` / `P115` | `L85_ThrusterValveDriverRack` / `P222` | `POWER_CONTROL` | Valve controller 5V command power |
| `W007` | `L85_Protected5VControlSupply` / `P114` | `L85_PrimaryScanningLiDAR` / `P111` | `POWER_CONTROL` | LiDAR signal conditioning 5V |
| `W008` | `L85_Protected5VControlSupply` / `P115` | `L85_PrecisionDockingCameraMonitor` / `P096` | `POWER_CONTROL` | Docking camera monitor 5V |
| `W009` | `L85_Protected5VControlSupply` / `P114` | `L85_HolographicRendezvousSCADA` / `P074` | `POWER_CONTROL` | SCADA control and visualization 5V |
| `W010` | `L85_Protected5VControlSupply` / `P115` | `L85_ElectricalTerminalDiagnosticRack` / `P057` | `POWER_CONTROL` | Terminal diagnostic monitor 5V |
| `W011` | `L85_SpacecraftInertialMeasurementUnit` / `P192` | `L85_RelativeNavigationFusionComputer` / `P154` | `ANALOG_FEEDBACK` | IMU measured acceleration and angular rate |
| `W012` | `L85_StereoNavigationCameraLeft` / `P198` | `L85_OpticalTargetPatternTracker` / `P087` | `DATA_STREAM` | Left navigation camera image samples |
| `W013` | `L85_StereoNavigationCameraRight` / `P199` | `L85_OpticalTargetPatternTracker` / `P088` | `DATA_STREAM` | Right navigation camera image samples |
| `W014` | `L85_OpticalTargetPatternTracker` / `P085` | `L85_RelativeNavigationFusionComputer` / `P156` | `DATA_STREAM` | Stereo triangulated optical target pose |
| `W015` | `L85_PrimaryScanningLiDAR` / `P108` | `L85_RelativePositionVelocityEstimator` / `P166` | `ANALOG_FEEDBACK` | Primary LiDAR measured range scan |
| `W016` | `L85_SecondaryScanningLiDAR` / `P173` | `L85_RelativePositionVelocityEstimator` / `P167` | `ANALOG_FEEDBACK` | Secondary LiDAR measured range scan |
| `W017` | `L85_ShortRangeRendezvousRadar` / `P176` | `L85_RelativePositionVelocityEstimator` / `P166` | `ANALOG_FEEDBACK` | Radar independent relative range rate |
| `W018` | `L85_DockingBeaconReceiver` / `P014` | `L85_RelativeNavigationFusionComputer` / `P158` | `DIGITAL_STATUS` | Dock target beacon identity validation |
| `W019` | `L85_OpticalTargetPatternTracker` / `P084` | `L85_RelativeNavigationFusionComputer` / `P155` | `ANALOG_FEEDBACK` | Docking fiducial pattern confidence |
| `W020` | `L85_RelativePositionVelocityEstimator` / `P164` | `L85_RelativeNavigationFusionComputer` / `P157` | `DATA_STREAM` | Estimated relative position and speed |
| `W021` | `L85_SpacecraftInertialMeasurementUnit` / `P193` | `L85_RelativePositionVelocityEstimator` / `P167` | `ANALOG_FEEDBACK` | Inertial relative-state propagation samples |
| `W022` | `L85_PrecisionFlightTimeReferenceClock` / `P100` | `L85_RelativeNavigationFusionComputer` / `P159` | `DIGITAL_STATUS` | Navigation timestamp and clock synchronization |
| `W023` | `L85_RelativeNavigationFusionComputer` / `P150` | `L85_NavigationUncertaintyVolumeDisplay` / `P082` | `DATA_STREAM` | Navigation covariance and confidence ellipsoid |
| `W024` | `L85_PrimaryScanningLiDAR` / `P109` | `L85_LiDARBlindSpotFaultEmulator` / `P077` | `DIGITAL_STATUS` | Primary LiDAR scan validity diagnostic |
| `W025` | `L85_SecondaryScanningLiDAR` / `P174` | `L85_LiDARBlindSpotFaultEmulator` / `P078` | `DIGITAL_STATUS` | Secondary LiDAR sensor redundancy diagnostic |
| `W026` | `L85_LiDARBlindSpotFaultEmulator` / `P075` | `L85_RelativeNavigationFusionComputer` / `P158` | `DIGITAL_STATUS` | Injected LiDAR blind spot signal quality |
| `W027` | `L85_RelativeNavigationFusionComputer` / `P152` | `L85_PrimaryScanningLiDAR` / `P110` | `DIGITAL_CONTROL` | Primary LiDAR timed scan trigger |
| `W028` | `L85_RelativeNavigationFusionComputer` / `P153` | `L85_SecondaryScanningLiDAR` / `P175` | `DIGITAL_CONTROL` | Secondary LiDAR timed scan trigger |
| `W029` | `L85_RelativeNavigationFusionComputer` / `P152` | `L85_ShortRangeRendezvousRadar` / `P178` | `DIGITAL_CONTROL` | Rendezvous radar mode request |
| `W030` | `L85_RelativeNavigationFusionComputer` / `P153` | `L85_OpticalTargetPatternTracker` / `P089` | `DIGITAL_CONTROL` | Target optical acquisition command |
| `W031` | `L85_DockingBeaconReceiver` / `P013` | `L85_OpticalTargetPatternTracker` / `P087` | `DATA_STREAM` | Dock beacon association to optical markers |
| `W032` | `L85_RelativePositionVelocityEstimator` / `P165` | `L85_HolographicRendezvousSCADA` / `P070` | `DATA_STREAM` | Relative range and velocity telemetry display |
| `W033` | `L85_NavigationUncertaintyVolumeDisplay` / `P081` | `L85_HolographicRendezvousSCADA` / `P071` | `DATA_STREAM` | Covariance visualization and quality trace |
| `W034` | `L85_SpacecraftInertialMeasurementUnit` / `P194` | `L85_HolographicRendezvousSCADA` / `P072` | `DIGITAL_STATUS` | Raw spacecraft IMU health telemetry |
| `W035` | `L85_SixDegreeFreedomFlightController` / `P182` | `L85_ThrusterForceAllocationComputer` / `P211` | `ACTUATOR_COMMAND` | Requested six degree thruster wrench |
| `W036` | `L85_ThrusterForceAllocationComputer` / `P209` | `L85_ThrusterValveDriverRack` / `P220` | `ACTUATOR_COMMAND` | Thruster command allocation and pulse train |
| `W037` | `L85_ThrusterValveDriverRack` / `P216` | `L85_RCSPlusXThruster` / `P139` | `ACTUATOR_COMMAND` | RCS thruster X valve pulse command |
| `W038` | `L85_ThrusterValveDriverRack` / `P217` | `L85_RCSMinusXThruster` / `P133` | `ACTUATOR_COMMAND` | RCS thruster Y valve pulse command |
| `W039` | `L85_ThrusterValveDriverRack` / `P216` | `L85_RCSPlusYThruster` / `P141` | `ACTUATOR_COMMAND` | RCS thruster Z valve pulse command |
| `W040` | `L85_ThrusterValveDriverRack` / `P217` | `L85_RCSMinusYThruster` / `P135` | `ACTUATOR_COMMAND` | RCS thruster x valve pulse command |
| `W041` | `L85_ThrusterValveDriverRack` / `P216` | `L85_RCSPlusZThruster` / `P143` | `ACTUATOR_COMMAND` | RCS thruster y valve pulse command |
| `W042` | `L85_ThrusterValveDriverRack` / `P217` | `L85_RCSMinusZThruster` / `P137` | `ACTUATOR_COMMAND` | RCS thruster z valve pulse command |
| `W043` | `L85_RCSPlusXThruster` / `P138` | `L85_ThrusterFlowAndPressureMonitor` / `P207` | `ANALOG_FEEDBACK` | RCS thruster X actual pulse feedback |
| `W044` | `L85_RCSMinusXThruster` / `P132` | `L85_ThrusterFlowAndPressureMonitor` / `P208` | `ANALOG_FEEDBACK` | RCS thruster Y actual pulse feedback |
| `W045` | `L85_RCSPlusYThruster` / `P140` | `L85_ThrusterFlowAndPressureMonitor` / `P207` | `ANALOG_FEEDBACK` | RCS thruster Z actual pulse feedback |
| `W046` | `L85_RCSMinusYThruster` / `P134` | `L85_ThrusterFlowAndPressureMonitor` / `P208` | `ANALOG_FEEDBACK` | RCS thruster x actual pulse feedback |
| `W047` | `L85_RCSPlusZThruster` / `P142` | `L85_ThrusterFlowAndPressureMonitor` / `P207` | `ANALOG_FEEDBACK` | RCS thruster y actual pulse feedback |
| `W048` | `L85_RCSMinusZThruster` / `P136` | `L85_ThrusterFlowAndPressureMonitor` / `P208` | `ANALOG_FEEDBACK` | RCS thruster z actual pulse feedback |
| `W049` | `L85_ThrusterFlowAndPressureMonitor` / `P205` | `L85_SixDegreeFreedomFlightController` / `P189` | `ANALOG_FEEDBACK` | Actual delivered impulse and pressure samples |
| `W050` | `L85_PropellantTankA` / `P112` | `L85_ReactionControlPropellantManifold` / `P146` | `ANALOG_FEEDBACK` | Propellant tank A available feed pressure |
| `W051` | `L85_PropellantTankB` / `P113` | `L85_ReactionControlPropellantManifold` / `P147` | `ANALOG_FEEDBACK` | Propellant tank B available feed pressure |
| `W052` | `L85_ReactionControlPropellantManifold` / `P145` | `L85_ThrusterValveDriverRack` / `P221` | `DIGITAL_STATUS` | Propellant manifold delivery readiness |
| `W053` | `L85_PrecisionThrusterCalibrationFixture` / `P102` | `L85_ThrusterForceAllocationComputer` / `P213` | `DATA_STREAM` | Measured thruster calibration thrust table |
| `W054` | `L85_StuckThrusterValveFaultEmulator` / `P201` | `L85_ThrusterValveDriverRack` / `P223` | `SAFETY_CONTROL` | Stuck thruster valve malfunction injection |
| `W055` | `L85_ThrusterValveDriverRack` / `P218` | `L85_StuckThrusterValveFaultEmulator` / `P202` | `DIGITAL_CONTROL` | Fault injector configured isolation readiness |
| `W056` | `L85_FuelReserveAndAbortPredictor` / `P062` | `L85_ThrusterForceAllocationComputer` / `P215` | `SAFETY_CONTROL` | Abort fuel reserve thrust allocation limit |
| `W057` | `L85_ThrusterForceAllocationComputer` / `P210` | `L85_FuelReserveAndAbortPredictor` / `P063` | `ANALOG_FEEDBACK` | Predicted commanded thruster fuel usage |
| `W058` | `L85_ReactionControlPropellantManifold` / `P144` | `L85_ThrusterFlowAndPressureMonitor` / `P207` | `ANALOG_FEEDBACK` | Propellant feed pressure transducer response |
| `W059` | `L85_RelativeNavigationFusionComputer` / `P151` | `L85_RelativeTrajectoryPlanningComputer` / `P171` | `DATA_STREAM` | Fused relative state for orbital trajectory planning |
| `W060` | `L85_RelativeTrajectoryPlanningComputer` / `P168` | `L85_ProximityOperationsGuidanceComputer` / `P129` | `DATA_STREAM` | Planned rendezvous trajectory and corridor |
| `W061` | `L85_ProximityOperationsGuidanceComputer` / `P122` | `L85_SixDegreeFreedomFlightController` / `P187` | `ACTUATOR_COMMAND` | Desired six degree translational guidance vector |
| `W062` | `L85_SixDegreeFreedomFlightController` / `P184` | `L85_ProximityOperationsGuidanceComputer` / `P127` | `ANALOG_FEEDBACK` | Actual translational control tracking error |
| `W063` | `L85_RelativePositionVelocityEstimator` / `P162` | `L85_ProximityOperationsGuidanceComputer` / `P128` | `ANALOG_FEEDBACK` | Measured range-rate and stopping distance |
| `W064` | `L85_PrecisionFlightTimeReferenceClock` / `P101` | `L85_RelativeTrajectoryPlanningComputer` / `P172` | `DIGITAL_STATUS` | Trajectory propagation time reference |
| `W065` | `L85_FuelReserveAndAbortPredictor` / `P060` | `L85_RelativeTrajectoryPlanningComputer` / `P170` | `ANALOG_FEEDBACK` | Propellant and abort margin constraint |
| `W066` | `L85_RelativeTrajectoryPlanningComputer` / `P169` | `L85_FuelReserveAndAbortPredictor` / `P064` | `DATA_STREAM` | Forecasted rendezvous impulse budget |
| `W067` | `L85_DockingCorridorSafetyGuardian` / `P018` | `L85_ProximityOperationsGuidanceComputer` / `P130` | `SAFETY_CONTROL` | Approach hold-point permission and mode |
| `W068` | `L85_CollisionAvoidancePredictionMonitor` / `P006` | `L85_ProximityOperationsGuidanceComputer` / `P131` | `SAFETY_CONTROL` | Predicted collision-free maneuver certificate |
| `W069` | `L85_ProximityOperationsGuidanceComputer` / `P126` | `L85_ProtectedAutonomousAbortController` / `P120` | `SAFETY_CONTROL` | Guidance protective abort request |
| `W070` | `L85_ProtectedAutonomousAbortController` / `P116` | `L85_SixDegreeFreedomFlightController` / `P188` | `ACTUATOR_COMMAND` | Protected autonomous retreat guidance |
| `W071` | `L85_SixDegreeFreedomFlightController` / `P186` | `L85_ProtectedAutonomousAbortController` / `P119` | `DIGITAL_STATUS` | Propulsion ability and attitude control feedback |
| `W072` | `L85_ProximityOperationsGuidanceComputer` / `P123` | `L85_RelativeOrbitTrajectoryHologram` / `P161` | `DATA_STREAM` | Predicted orbital approach overlay |
| `W073` | `L85_RelativeNavigationFusionComputer` / `P150` | `L85_NavigationUncertaintyVolumeDisplay` / `P083` | `DATA_STREAM` | Estimated positional uncertainty distribution |
| `W074` | `L85_ProximityOperationsGuidanceComputer` / `P125` | `L85_HolographicRendezvousSCADA` / `P073` | `DIGITAL_STATUS` | Guidance controller state and tracking trend |
| `W075` | `L85_RelativeTrajectoryPlanningComputer` / `P168` | `L85_HolographicRendezvousSCADA` / `P070` | `DATA_STREAM` | Trajectory delta-v forecast telemetry |
| `W076` | `L85_FuelReserveAndAbortPredictor` / `P061` | `L85_HolographicRendezvousSCADA` / `P068` | `ANALOG_FEEDBACK` | Fuel reserve and retreat envelope |
| `W077` | `L85_ProtectedAutonomousAbortController` / `P117` | `L85_HolographicRendezvousSCADA` / `P072` | `DIGITAL_STATUS` | Abort controller actual mode status |
| `W078` | `L85_SixDegreeFreedomFlightController` / `P183` | `L85_ThrusterForceAllocationComputer` / `P212` | `ACTUATOR_COMMAND` | Real-time microthruster body-wrench update |
| `W079` | `L85_ShortRangeRendezvousRadar` / `P177` | `L85_DockingCorridorSafetyGuardian` / `P020` | `ANALOG_FEEDBACK` | Short-range approaching-object radial distance |
| `W080` | `L85_RelativeNavigationFusionComputer` / `P148` | `L85_DockingCorridorSafetyGuardian` / `P021` | `ANALOG_FEEDBACK` | Navigation estimation confidence for approach |
| `W081` | `L85_RelativePositionVelocityEstimator` / `P163` | `L85_DockingCorridorSafetyGuardian` / `P020` | `ANALOG_FEEDBACK` | Relative target approach speed estimate |
| `W082` | `L85_CollisionAvoidancePredictionMonitor` / `P007` | `L85_DockingCorridorSafetyGuardian` / `P024` | `SAFETY_CONTROL` | Predictive keep-out-zone intersection status |
| `W083` | `L85_DockingCorridorSafetyGuardian` / `P019` | `L85_ProximityOperationsGuidanceComputer` / `P130` | `SAFETY_CONTROL` | Collision boundary clearance authorization |
| `W084` | `L85_PrecisionDockingCameraMonitor` / `P090` | `L85_DockingCorridorSafetyGuardian` / `P022` | `DIGITAL_STATUS` | Live final docking camera status |
| `W085` | `L85_OpticalTargetPatternTracker` / `P086` | `L85_PrecisionDockingCameraMonitor` / `P092` | `DATA_STREAM` | Optical docking target video alignment |
| `W086` | `L85_StationOpticalTargetMarkerArray` / `P197` | `L85_OpticalTargetPatternTracker` / `P088` | `DATA_STREAM` | Station target fiducial coordinates |
| `W087` | `L85_StationApproachBeaconCluster` / `P195` | `L85_DockingBeaconReceiver` / `P015` | `DATA_STREAM` | Dock beacon coded acquisition signal |
| `W088` | `L85_ExternalInspectionCameraGantry` / `P059` | `L85_PrecisionDockingCameraMonitor` / `P093` | `DATA_STREAM` | Inspection gantry monitoring pose |
| `W089` | `L85_StationApproachIlluminationRig` / `P196` | `L85_PrecisionDockingCameraMonitor` / `P094` | `DIGITAL_STATUS` | Docking approach lighting status |
| `W090` | `L85_ProximityOperationsGuidanceComputer` / `P124` | `L85_ThreeDimensionalApproachCorridorHologram` / `P203` | `DATA_STREAM` | Commanded approach corridor overlay |
| `W091` | `L85_DockingCorridorSafetyGuardian` / `P016` | `L85_ThreeDimensionalApproachCorridorHologram` / `P204` | `DATA_STREAM` | Actual proximity warning boundary visual |
| `W092` | `L85_DockingCorridorSafetyGuardian` / `P017` | `L85_HolographicRendezvousSCADA` / `P073` | `DIGITAL_STATUS` | Approach hold point readiness report |
| `W093` | `L85_CollisionAvoidancePredictionMonitor` / `P005` | `L85_HolographicRendezvousSCADA` / `P071` | `DATA_STREAM` | Collision avoidance prediction timeline |
| `W094` | `L85_DockingCorridorSafetyGuardian` / `P018` | `L85_ProtectedAutonomousAbortController` / `P121` | `SAFETY_CONTROL` | Emergency hold/retreat arbitration request |
| `W095` | `L85_ProtectedAutonomousAbortController` / `P118` | `L85_DockingCorridorSafetyGuardian` / `P023` | `DIGITAL_STATUS` | Measured retreat maneuver executed status |
| `W096` | `L85_DockingCorridorSafetyGuardian` / `P019` | `L85_DockingMechanismSequencingPLC` / `P036` | `SAFETY_CONTROL` | Final docking contact authorization |
| `W097` | `L85_DockingMechanismSequencingPLC` / `P027` | `L85_ActiveSoftCaptureRing` / `P002` | `ACTUATOR_COMMAND` | Active soft capture ring actuation request |
| `W098` | `L85_ActiveSoftCaptureRing` / `P001` | `L85_DockingMechanismSequencingPLC` / `P032` | `ANALOG_FEEDBACK` | Measured active ring extension status |
| `W099` | `L85_DockingMechanismSequencingPLC` / `P028` | `L85_MotorizedCaptureGuidePetals` / `P080` | `ACTUATOR_COMMAND` | Guide petal positioning request |
| `W100` | `L85_MotorizedCaptureGuidePetals` / `P079` | `L85_DockingMechanismSequencingPLC` / `P033` | `ANALOG_FEEDBACK` | Measured guide petal alignment feedback |
| `W101` | `L85_DockingMechanismSequencingPLC` / `P027` | `L85_SixActuatorDockingLatchArray` / `P180` | `ACTUATOR_COMMAND` | Six independent hard-latch actuation permits |
| `W102` | `L85_SixActuatorDockingLatchArray` / `P179` | `L85_DockingMechanismSequencingPLC` / `P032` | `ANALOG_FEEDBACK` | Six hard-latch actual lock feedback channels |
| `W103` | `L85_SixAxisContactForceSensor` / `P181` | `L85_DockingMechanismSequencingPLC` / `P033` | `ANALOG_FEEDBACK` | Six axis captured contact force feedback |
| `W104` | `L85_DockingMechanismSequencingPLC` / `P028` | `L85_CompliantContactDampingActuator` / `P009` | `ACTUATOR_COMMAND` | Contact damping request under safe loads |
| `W105` | `L85_CompliantContactDampingActuator` / `P008` | `L85_DockingMechanismSequencingPLC` / `P032` | `ANALOG_FEEDBACK` | Actual compliant damper displacement |
| `W106` | `L85_DockingMechanismSequencingPLC` / `P027` | `L85_DockingElectricalUmbilicalCoupler` / `P026` | `ACTUATOR_COMMAND` | Qualified docking umbilical mate command |
| `W107` | `L85_DockingElectricalUmbilicalCoupler` / `P025` | `L85_DockingMechanismSequencingPLC` / `P033` | `ANALOG_FEEDBACK` | Actual umbilical continuity verification |
| `W108` | `L85_DockingMechanismSequencingPLC` / `P028` | `L85_PressureInterfaceSealGasket` / `P104` | `ACTUATOR_COMMAND` | Pressure gasket compression authorization |
| `W109` | `L85_PressureInterfaceSealGasket` / `P103` | `L85_PressureLeakQualificationController` / `P107` | `ANALOG_FEEDBACK` | Measured gasket compression and seal health |
| `W110` | `L85_PressureLeakQualificationController` / `P105` | `L85_DockingMechanismSequencingPLC` / `P034` | `DIGITAL_STATUS` | Completed fictional pressure leak test results |
| `W111` | `L85_HardDockStructuralLoadMonitor` / `P065` | `L85_DockingMechanismSequencingPLC` / `P032` | `ANALOG_FEEDBACK` | Docked structure load distribution monitor |
| `W112` | `L85_DockingMechanismSequencingPLC` / `P030` | `L85_PrecisionDockingCameraMonitor` / `P095` | `DIGITAL_STATUS` | Docking mechanism step-by-step camera status |
| `W113` | `L85_AsymmetricLatchFaultEmulator` / `P004` | `L85_DockingMechanismSequencingPLC` / `P037` | `SAFETY_CONTROL` | Asymmetric mechanical latch fault injection |
| `W114` | `L85_DockingMechanismSequencingPLC` / `P031` | `L85_HolographicRendezvousSCADA` / `P072` | `DIGITAL_STATUS` | Soft capture to hard docking state events |
| `W115` | `L85_Protected5VControlSupply` / `P114` | `L85_DockingSafetyPLC` / `P051` | `POWER_CONTROL` | Independent docking safety PLC protected control supply |
| `W116` | `L85_EmergencyApproachHoldStation` / `P058` | `L85_DockingSafetyPLC` / `P052` | `SAFETY_CONTROL` | Manual emergency approach hold physical contact |
| `W117` | `L85_DockingSafetyPLC` / `P043` | `L85_SixDegreeFreedomFlightController` / `P191` | `SAFETY_CONTROL` | Independent thruster inhibit and authorization |
| `W118` | `L85_DockingSafetyPLC` / `P044` | `L85_DockingMechanismSequencingPLC` / `P036` | `SAFETY_CONTROL` | Independent hard capture inhibit output |
| `W119` | `L85_FuelReserveAndAbortPredictor` / `P060` | `L85_DockingSafetyPLC` / `P045` | `ANALOG_FEEDBACK` | Minimum physical fuel reserve for safe abort |
| `W120` | `L85_CollisionAvoidancePredictionMonitor` / `P006` | `L85_DockingSafetyPLC` / `P053` | `SAFETY_CONTROL` | Independent collision prediction stop request |
| `W121` | `L85_CriticalClosingRateFeedbackStation` / `P010` | `L85_DockingSafetyPLC` / `P048` | `DIGITAL_STATUS` | Closing rate monitor calibration health diagnostic |
| `W122` | `L85_RelativeNavigationFusionComputer` / `P149` | `L85_CriticalClosingRateFeedbackStation` / `P011` | `ANALOG_FEEDBACK` | Navigation fusion raw range rate calibration channel |
| **W123 — CRITICAL** | `L85_RelativeNavigationFusionComputer` / `P148` | `L85_DockingSafetyPLC` / `P046` | `ANALOG_FEEDBACK` | CRITICAL trusted relative closing-rate feedback |
| `W124` | `L85_RelativePositionVelocityEstimator` / `P162` | `L85_HolographicRendezvousSCADA` / `P069` | `ANALOG_FEEDBACK` | Display-only relative speed telemetry (NOT W123) |
| `W125` | `L85_DockingSafetyPLC` / `P043` | `L85_ProtectedAutonomousAbortController` / `P120` | `SAFETY_CONTROL` | Safety-authorized escape/retreat enable |
| `W126` | `L85_ProtectedAutonomousAbortController` / `P117` | `L85_DockingSafetyPLC` / `P049` | `DIGITAL_STATUS` | Measured safe abort completion feedback |
| `W127` | `L85_DockingSafetyPLC` / `P044` | `L85_CriticalClosingRateFeedbackStation` / `P012` | `SAFETY_CONTROL` | Critical W123 feedback state and fault latch |
| `W128` | `L85_ElectricalTerminalDiagnosticRack` / `P054` | `L85_DockingSafetyPLC` / `P047` | `DIGITAL_CONTROL` | Operator explicit safety reset request |
| `W129` | `L85_DockingSafetyPLC` / `P042` | `L85_HolographicRendezvousSCADA` / `P073` | `DIGITAL_STATUS` | Critical safety status and event telemetry |
| `W130` | `L85_DockingSafetyPLC` / `P043` | `L85_DockingMissionCertificationBeacon` / `P040` | `SAFETY_CONTROL` | Protected docking mission release authorization |
| `W131` | `L85_RelativeNavigationFusionComputer` / `P151` | `L85_HolographicRendezvousSCADA` / `P070` | `DATA_STREAM` | Fused position and velocity state SCADA |
| `W132` | `L85_RelativePositionVelocityEstimator` / `P163` | `L85_HolographicRendezvousSCADA` / `P068` | `ANALOG_FEEDBACK` | Measured target closing rate display summary |
| `W133` | `L85_SixDegreeFreedomFlightController` / `P185` | `L85_HolographicRendezvousSCADA` / `P071` | `DATA_STREAM` | Six degree translation actual control telemetry |
| `W134` | `L85_ThrusterValveDriverRack` / `P219` | `L85_HolographicRendezvousSCADA` / `P072` | `DIGITAL_STATUS` | Thruster valve commanded versus actual status |
| `W135` | `L85_ThrusterFlowAndPressureMonitor` / `P206` | `L85_HolographicRendezvousSCADA` / `P069` | `ANALOG_FEEDBACK` | Propellant flow and pulse history trend |
| `W136` | `L85_LiDARBlindSpotFaultEmulator` / `P076` | `L85_ElectricalTerminalDiagnosticRack` / `P055` | `DIGITAL_STATUS` | LiDAR dropout event diagnostic fingerprint |
| `W137` | `L85_StuckThrusterValveFaultEmulator` / `P200` | `L85_ElectricalTerminalDiagnosticRack` / `P056` | `DIGITAL_STATUS` | Stuck valve fault and isolation report |
| `W138` | `L85_AsymmetricLatchFaultEmulator` / `P003` | `L85_ElectricalTerminalDiagnosticRack` / `P055` | `DIGITAL_STATUS` | Asymmetric latch abnormality diagnosis |
| `W139` | `L85_PressureLeakQualificationController` / `P106` | `L85_HolographicRendezvousSCADA` / `P073` | `DIGITAL_STATUS` | Pressure seal post docking integrity report |
| `W140` | `L85_HardDockStructuralLoadMonitor` / `P066` | `L85_HolographicRendezvousSCADA` / `P068` | `ANALOG_FEEDBACK` | Captured structural load safety report |
| `W141` | `L85_PrecisionDockingCameraMonitor` / `P091` | `L85_HolographicRendezvousSCADA` / `P072` | `DIGITAL_STATUS` | Synchronized approach camera feed health |
| `W142` | `L85_DockingMechanismSequencingPLC` / `P029` | `L85_PrecisionFlightEventDataRecorder` / `P098` | `DATA_STREAM` | Docking state transitions flight recorder |
| `W143` | `L85_DockingSafetyPLC` / `P041` | `L85_PrecisionFlightEventDataRecorder` / `P099` | `DATA_STREAM` | Latched safety alarm flight recorder |
| `W144` | `L85_PrecisionFlightEventDataRecorder` / `P097` | `L85_HolographicRendezvousSCADA` / `P070` | `DATA_STREAM` | Signed fresh mission run event log |
| `W145` | `L85_HolographicRendezvousSCADA` / `P067` | `L85_DockingMissionCertificationBeacon` / `P039` | `DIGITAL_STATUS` | Six-objective authorized certification display |
| `W146` | `L85_DockingMissionCertificationBeacon` / `P038` | `L85_HolographicRendezvousSCADA` / `P073` | `DIGITAL_STATUS` | Docking mission 6 of 6 objective certificate |

Every row in Annex C is **concept only**; no source-provided actual connected player wire. Critical W123 logical terminals are `TRUSTED_RANGE_RATE_OUT` → `TRUSTED_RANGE_RATE_IN`, P148 → P046. Full terminal names for every other wire are in Annex E and the original CSV.


## ANNEX D — ALL 224 MODELLED VISUAL SOCKET ANCHORS

| Art socket | Owning model | World XYZ (m) | Source role | Signal hint |
|---|---|---|---|---|
| `P001` | `L85_ActiveSoftCaptureRing` | `(2.3875, 4.84, -1.2)` | source | `ANALOG_FEEDBACK` |
| `P002` | `L85_ActiveSoftCaptureRing` | `(2.4925, 4.84, -1.2)` | target | `ACTUATOR_COMMAND` |
| `P003` | `L85_AsymmetricLatchFaultEmulator` | `(8.0875, 3.51, 4.98)` | source | `DIGITAL_STATUS` |
| `P004` | `L85_AsymmetricLatchFaultEmulator` | `(8.1925, 3.51, 4.98)` | source | `SAFETY_CONTROL` |
| `P005` | `L85_CollisionAvoidancePredictionMonitor` | `(3.2675, 1.51, 4.98)` | source | `DATA_STREAM` |
| `P006` | `L85_CollisionAvoidancePredictionMonitor` | `(3.3725, 1.51, 4.98)` | source | `SAFETY_CONTROL` |
| `P007` | `L85_CollisionAvoidancePredictionMonitor` | `(3.4775, 1.51, 4.98)` | source | `SAFETY_CONTROL` |
| `P008` | `L85_CompliantContactDampingActuator` | `(2.3375, 4.84, -1.2)` | source | `ANALOG_FEEDBACK` |
| `P009` | `L85_CompliantContactDampingActuator` | `(2.4425, 4.84, -1.2)` | target | `ACTUATOR_COMMAND` |
| `P010` | `L85_CriticalClosingRateFeedbackStation` | `(-11.6025, 3.39, 4.98)` | source | `DIGITAL_STATUS` |
| `P011` | `L85_CriticalClosingRateFeedbackStation` | `(-11.4975, 3.39, 4.98)` | target | `ANALOG_FEEDBACK` |
| `P012` | `L85_CriticalClosingRateFeedbackStation` | `(-11.3925, 3.39, 4.98)` | target | `SAFETY_CONTROL` |
| `P013` | `L85_DockingBeaconReceiver` | `(-2.5625, 5.99, -1.05)` | source | `DATA_STREAM` |
| `P014` | `L85_DockingBeaconReceiver` | `(-2.4575, 5.99, -1.05)` | source | `DIGITAL_STATUS` |
| `P015` | `L85_DockingBeaconReceiver` | `(-2.3525, 5.99, -1.05)` | target | `DATA_STREAM` |
| `P016` | `L85_DockingCorridorSafetyGuardian` | `(1.9175, 1.51, 4.98)` | source | `DATA_STREAM` |
| `P017` | `L85_DockingCorridorSafetyGuardian` | `(2.0225, 1.51, 4.98)` | source | `DIGITAL_STATUS` |
| `P018` | `L85_DockingCorridorSafetyGuardian` | `(2.1275, 1.51, 4.98)` | source | `SAFETY_CONTROL` |
| `P019` | `L85_DockingCorridorSafetyGuardian` | `(2.2325, 1.51, 4.98)` | source | `SAFETY_CONTROL` |
| `P020` | `L85_DockingCorridorSafetyGuardian` | `(2.3375, 1.51, 4.98)` | target | `ANALOG_FEEDBACK` |
| `P021` | `L85_DockingCorridorSafetyGuardian` | `(2.4425, 1.51, 4.98)` | target | `ANALOG_FEEDBACK` |
| `P022` | `L85_DockingCorridorSafetyGuardian` | `(1.9175, 1.615, 4.98)` | target | `DIGITAL_STATUS` |
| `P023` | `L85_DockingCorridorSafetyGuardian` | `(2.0225, 1.615, 4.98)` | target | `DIGITAL_STATUS` |
| `P024` | `L85_DockingCorridorSafetyGuardian` | `(2.1275, 1.615, 4.98)` | target | `SAFETY_CONTROL` |
| `P025` | `L85_DockingElectricalUmbilicalCoupler` | `(2.3375, 4.84, -1.2)` | source | `ANALOG_FEEDBACK` |
| `P026` | `L85_DockingElectricalUmbilicalCoupler` | `(2.4425, 4.84, -1.2)` | target | `ACTUATOR_COMMAND` |
| `P027` | `L85_DockingMechanismSequencingPLC` | `(8.7375, 1.51, 4.98)` | source | `ACTUATOR_COMMAND` |
| `P028` | `L85_DockingMechanismSequencingPLC` | `(8.8425, 1.51, 4.98)` | source | `ACTUATOR_COMMAND` |
| `P029` | `L85_DockingMechanismSequencingPLC` | `(8.9475, 1.51, 4.98)` | source | `DATA_STREAM` |
| `P030` | `L85_DockingMechanismSequencingPLC` | `(9.0525, 1.51, 4.98)` | source | `DIGITAL_STATUS` |
| `P031` | `L85_DockingMechanismSequencingPLC` | `(9.1575, 1.51, 4.98)` | source | `DIGITAL_STATUS` |
| `P032` | `L85_DockingMechanismSequencingPLC` | `(9.2625, 1.51, 4.98)` | target | `ANALOG_FEEDBACK` |
| `P033` | `L85_DockingMechanismSequencingPLC` | `(8.7375, 1.615, 4.98)` | target | `ANALOG_FEEDBACK` |
| `P034` | `L85_DockingMechanismSequencingPLC` | `(8.8425, 1.615, 4.98)` | target | `DIGITAL_STATUS` |
| `P035` | `L85_DockingMechanismSequencingPLC` | `(8.9475, 1.615, 4.98)` | target | `POWER_CONTROL` |
| `P036` | `L85_DockingMechanismSequencingPLC` | `(9.0525, 1.615, 4.98)` | target | `SAFETY_CONTROL` |
| `P037` | `L85_DockingMechanismSequencingPLC` | `(9.1575, 1.615, 4.98)` | target | `SAFETY_CONTROL` |
| `P038` | `L85_DockingMissionCertificationBeacon` | `(-0.2625, 8.99, -5.37)` | source | `DIGITAL_STATUS` |
| `P039` | `L85_DockingMissionCertificationBeacon` | `(-0.1575, 8.99, -5.37)` | target | `DIGITAL_STATUS` |
| `P040` | `L85_DockingMissionCertificationBeacon` | `(-0.0525, 8.99, -5.37)` | target | `SAFETY_CONTROL` |
| `P041` | `L85_DockingSafetyPLC` | `(-10.2625, 3.39, 4.98)` | source | `DATA_STREAM` |
| `P042` | `L85_DockingSafetyPLC` | `(-10.1575, 3.39, 4.98)` | source | `DIGITAL_STATUS` |
| `P043` | `L85_DockingSafetyPLC` | `(-10.0525, 3.39, 4.98)` | source | `SAFETY_CONTROL` |
| `P044` | `L85_DockingSafetyPLC` | `(-9.9475, 3.39, 4.98)` | source | `SAFETY_CONTROL` |
| `P045` | `L85_DockingSafetyPLC` | `(-9.8425, 3.39, 4.98)` | target | `ANALOG_FEEDBACK` |
| `P046` | `L85_DockingSafetyPLC` | `(-9.7375, 3.39, 4.98)` | target | `ANALOG_FEEDBACK` |
| `P047` | `L85_DockingSafetyPLC` | `(-10.2625, 3.495, 4.98)` | target | `DIGITAL_CONTROL` |
| `P048` | `L85_DockingSafetyPLC` | `(-10.1575, 3.495, 4.98)` | target | `DIGITAL_STATUS` |
| `P049` | `L85_DockingSafetyPLC` | `(-10.0525, 3.495, 4.98)` | target | `DIGITAL_STATUS` |
| `P050` | `L85_DockingSafetyPLC` | `(-9.9475, 3.495, 4.98)` | target | `POWER_CONTROL` |
| `P051` | `L85_DockingSafetyPLC` | `(-9.8425, 3.495, 4.98)` | target | `POWER_CONTROL` |
| `P052` | `L85_DockingSafetyPLC` | `(-9.7375, 3.495, 4.98)` | target | `SAFETY_CONTROL` |
| `P053` | `L85_DockingSafetyPLC` | `(-10.2625, 3.6, 4.98)` | target | `SAFETY_CONTROL` |
| `P054` | `L85_ElectricalTerminalDiagnosticRack` | `(-6.2325, 3.39, 4.98)` | source | `DIGITAL_CONTROL` |
| `P055` | `L85_ElectricalTerminalDiagnosticRack` | `(-6.1275, 3.39, 4.98)` | target | `DIGITAL_STATUS` |
| `P056` | `L85_ElectricalTerminalDiagnosticRack` | `(-6.0225, 3.39, 4.98)` | target | `DIGITAL_STATUS` |
| `P057` | `L85_ElectricalTerminalDiagnosticRack` | `(-5.9175, 3.39, 4.98)` | target | `POWER_CONTROL` |
| `P058` | `L85_EmergencyApproachHoldStation` | `(-4.8825, 3.39, 4.98)` | source | `SAFETY_CONTROL` |
| `P059` | `L85_ExternalInspectionCameraGantry` | `(5.5375, 6.89, -0.3)` | source | `DATA_STREAM` |
| `P060` | `L85_FuelReserveAndAbortPredictor` | `(4.6175, 1.51, 4.98)` | source | `ANALOG_FEEDBACK` |
| `P061` | `L85_FuelReserveAndAbortPredictor` | `(4.7225, 1.51, 4.98)` | source | `ANALOG_FEEDBACK` |
| `P062` | `L85_FuelReserveAndAbortPredictor` | `(4.8275, 1.51, 4.98)` | source | `SAFETY_CONTROL` |
| `P063` | `L85_FuelReserveAndAbortPredictor` | `(4.9325, 1.51, 4.98)` | target | `ANALOG_FEEDBACK` |
| `P064` | `L85_FuelReserveAndAbortPredictor` | `(5.0375, 1.51, 4.98)` | target | `DATA_STREAM` |
| `P065` | `L85_HardDockStructuralLoadMonitor` | `(2.4875, 4.84, -1.2)` | source | `ANALOG_FEEDBACK` |
| `P066` | `L85_HardDockStructuralLoadMonitor` | `(2.5925, 4.84, -1.2)` | source | `ANALOG_FEEDBACK` |
| `P067` | `L85_HolographicRendezvousSCADA` | `(-0.2625, 8.29, -5.34)` | source | `DIGITAL_STATUS` |
| `P068` | `L85_HolographicRendezvousSCADA` | `(-0.1575, 8.29, -5.34)` | target | `ANALOG_FEEDBACK` |
| `P069` | `L85_HolographicRendezvousSCADA` | `(-0.0525, 8.29, -5.34)` | target | `ANALOG_FEEDBACK` |
| `P070` | `L85_HolographicRendezvousSCADA` | `(0.0525, 8.29, -5.34)` | target | `DATA_STREAM` |
| `P071` | `L85_HolographicRendezvousSCADA` | `(0.1575, 8.29, -5.34)` | target | `DATA_STREAM` |
| `P072` | `L85_HolographicRendezvousSCADA` | `(0.2625, 8.29, -5.34)` | target | `DIGITAL_STATUS` |
| `P073` | `L85_HolographicRendezvousSCADA` | `(-0.2625, 8.395, -5.34)` | target | `DIGITAL_STATUS` |
| `P074` | `L85_HolographicRendezvousSCADA` | `(-0.1575, 8.395, -5.34)` | target | `POWER_CONTROL` |
| `P075` | `L85_LiDARBlindSpotFaultEmulator` | `(10.0575, 1.51, 4.98)` | source | `DIGITAL_STATUS` |
| `P076` | `L85_LiDARBlindSpotFaultEmulator` | `(10.1625, 1.51, 4.98)` | source | `DIGITAL_STATUS` |
| `P077` | `L85_LiDARBlindSpotFaultEmulator` | `(10.2675, 1.51, 4.98)` | target | `DIGITAL_STATUS` |
| `P078` | `L85_LiDARBlindSpotFaultEmulator` | `(10.3725, 1.51, 4.98)` | target | `DIGITAL_STATUS` |
| `P079` | `L85_MotorizedCaptureGuidePetals` | `(2.3375, 4.84, -1.2)` | source | `ANALOG_FEEDBACK` |
| `P080` | `L85_MotorizedCaptureGuidePetals` | `(2.4425, 4.84, -1.2)` | target | `ACTUATOR_COMMAND` |
| `P081` | `L85_NavigationUncertaintyVolumeDisplay` | `(-8.4625, 6.39, -5.45)` | source | `DATA_STREAM` |
| `P082` | `L85_NavigationUncertaintyVolumeDisplay` | `(-8.3575, 6.39, -5.45)` | target | `DATA_STREAM` |
| `P083` | `L85_NavigationUncertaintyVolumeDisplay` | `(-8.2525, 6.39, -5.45)` | target | `DATA_STREAM` |
| `P084` | `L85_OpticalTargetPatternTracker` | `(-2.5925, 4.49, -1.05)` | source | `ANALOG_FEEDBACK` |
| `P085` | `L85_OpticalTargetPatternTracker` | `(-2.4875, 4.49, -1.05)` | source | `DATA_STREAM` |
| `P086` | `L85_OpticalTargetPatternTracker` | `(-2.3825, 4.49, -1.05)` | source | `DATA_STREAM` |
| `P087` | `L85_OpticalTargetPatternTracker` | `(-2.2775, 4.49, -1.05)` | target | `DATA_STREAM` |
| `P088` | `L85_OpticalTargetPatternTracker` | `(-2.1725, 4.49, -1.05)` | target | `DATA_STREAM` |
| `P089` | `L85_OpticalTargetPatternTracker` | `(-2.0675, 4.49, -1.05)` | target | `DIGITAL_CONTROL` |
| `P090` | `L85_PrecisionDockingCameraMonitor` | `(9.8375, 3.99, -5.12)` | source | `DIGITAL_STATUS` |
| `P091` | `L85_PrecisionDockingCameraMonitor` | `(9.9425, 3.99, -5.12)` | source | `DIGITAL_STATUS` |
| `P092` | `L85_PrecisionDockingCameraMonitor` | `(10.0475, 3.99, -5.12)` | target | `DATA_STREAM` |
| `P093` | `L85_PrecisionDockingCameraMonitor` | `(10.1525, 3.99, -5.12)` | target | `DATA_STREAM` |
| `P094` | `L85_PrecisionDockingCameraMonitor` | `(10.2575, 3.99, -5.12)` | target | `DIGITAL_STATUS` |
| `P095` | `L85_PrecisionDockingCameraMonitor` | `(10.3625, 3.99, -5.12)` | target | `DIGITAL_STATUS` |
| `P096` | `L85_PrecisionDockingCameraMonitor` | `(9.8375, 4.095, -5.12)` | target | `POWER_CONTROL` |
| `P097` | `L85_PrecisionFlightEventDataRecorder` | `(-3.5325, 3.39, 4.98)` | source | `DATA_STREAM` |
| `P098` | `L85_PrecisionFlightEventDataRecorder` | `(-3.4275, 3.39, 4.98)` | target | `DATA_STREAM` |
| `P099` | `L85_PrecisionFlightEventDataRecorder` | `(-3.3225, 3.39, 4.98)` | target | `DATA_STREAM` |
| `P100` | `L85_PrecisionFlightTimeReferenceClock` | `(-4.8325, 1.51, 4.98)` | source | `DIGITAL_STATUS` |
| `P101` | `L85_PrecisionFlightTimeReferenceClock` | `(-4.7275, 1.51, 4.98)` | source | `DIGITAL_STATUS` |
| `P102` | `L85_PrecisionThrusterCalibrationFixture` | `(-8.8925, 1.51, 4.98)` | source | `DATA_STREAM` |
| `P103` | `L85_PressureInterfaceSealGasket` | `(2.3875, 4.84, -1.2)` | source | `ANALOG_FEEDBACK` |
| `P104` | `L85_PressureInterfaceSealGasket` | `(2.4925, 4.84, -1.2)` | target | `ACTUATOR_COMMAND` |
| `P105` | `L85_PressureLeakQualificationController` | `(7.3575, 1.51, 4.98)` | source | `DIGITAL_STATUS` |
| `P106` | `L85_PressureLeakQualificationController` | `(7.4625, 1.51, 4.98)` | source | `DIGITAL_STATUS` |
| `P107` | `L85_PressureLeakQualificationController` | `(7.5675, 1.51, 4.98)` | target | `ANALOG_FEEDBACK` |
| `P108` | `L85_PrimaryScanningLiDAR` | `(-3.1625, 5.36, -0.8)` | source | `ANALOG_FEEDBACK` |
| `P109` | `L85_PrimaryScanningLiDAR` | `(-3.0575, 5.36, -0.8)` | source | `DIGITAL_STATUS` |
| `P110` | `L85_PrimaryScanningLiDAR` | `(-2.9525, 5.36, -0.8)` | target | `DIGITAL_CONTROL` |
| `P111` | `L85_PrimaryScanningLiDAR` | `(-2.8475, 5.36, -0.8)` | target | `POWER_CONTROL` |
| `P112` | `L85_PropellantTankA` | `(-5.9425, 4.84, -1.65)` | source | `ANALOG_FEEDBACK` |
| `P113` | `L85_PropellantTankB` | `(-5.3625, 4.84, -1.65)` | source | `ANALOG_FEEDBACK` |
| `P114` | `L85_Protected5VControlSupply` | `(-8.9225, 3.39, 4.98)` | source | `POWER_CONTROL` |
| `P115` | `L85_Protected5VControlSupply` | `(-8.8175, 3.39, 4.98)` | source | `POWER_CONTROL` |
| `P116` | `L85_ProtectedAutonomousAbortController` | `(5.9675, 1.51, 4.98)` | source | `ACTUATOR_COMMAND` |
| `P117` | `L85_ProtectedAutonomousAbortController` | `(6.0725, 1.51, 4.98)` | source | `DIGITAL_STATUS` |
| `P118` | `L85_ProtectedAutonomousAbortController` | `(6.1775, 1.51, 4.98)` | source | `DIGITAL_STATUS` |
| `P119` | `L85_ProtectedAutonomousAbortController` | `(6.2825, 1.51, 4.98)` | target | `DIGITAL_STATUS` |
| `P120` | `L85_ProtectedAutonomousAbortController` | `(6.3875, 1.51, 4.98)` | target | `SAFETY_CONTROL` |
| `P121` | `L85_ProtectedAutonomousAbortController` | `(6.4925, 1.51, 4.98)` | target | `SAFETY_CONTROL` |
| `P122` | `L85_ProximityOperationsGuidanceComputer` | `(-2.1325, 1.51, 4.98)` | source | `ACTUATOR_COMMAND` |
| `P123` | `L85_ProximityOperationsGuidanceComputer` | `(-2.0275, 1.51, 4.98)` | source | `DATA_STREAM` |
| `P124` | `L85_ProximityOperationsGuidanceComputer` | `(-1.9225, 1.51, 4.98)` | source | `DATA_STREAM` |
| `P125` | `L85_ProximityOperationsGuidanceComputer` | `(-1.8175, 1.51, 4.98)` | source | `DIGITAL_STATUS` |
| `P126` | `L85_ProximityOperationsGuidanceComputer` | `(-1.7125, 1.51, 4.98)` | source | `SAFETY_CONTROL` |
| `P127` | `L85_ProximityOperationsGuidanceComputer` | `(-1.6075, 1.51, 4.98)` | target | `ANALOG_FEEDBACK` |
| `P128` | `L85_ProximityOperationsGuidanceComputer` | `(-2.1325, 1.615, 4.98)` | target | `ANALOG_FEEDBACK` |
| `P129` | `L85_ProximityOperationsGuidanceComputer` | `(-2.0275, 1.615, 4.98)` | target | `DATA_STREAM` |
| `P130` | `L85_ProximityOperationsGuidanceComputer` | `(-1.9225, 1.615, 4.98)` | target | `SAFETY_CONTROL` |
| `P131` | `L85_ProximityOperationsGuidanceComputer` | `(-1.8175, 1.615, 4.98)` | target | `SAFETY_CONTROL` |
| `P132` | `L85_RCSMinusXThruster` | `(-5.9625, 4.17, -0.25)` | source | `ANALOG_FEEDBACK` |
| `P133` | `L85_RCSMinusXThruster` | `(-5.8575, 4.17, -0.25)` | target | `ACTUATOR_COMMAND` |
| `P134` | `L85_RCSMinusYThruster` | `(-5.0625, 3.91, -1.35)` | source | `ANALOG_FEEDBACK` |
| `P135` | `L85_RCSMinusYThruster` | `(-4.9575, 3.91, -1.35)` | target | `ACTUATOR_COMMAND` |
| `P136` | `L85_RCSMinusZThruster` | `(-4.5125, 4.84, -2.65)` | source | `ANALOG_FEEDBACK` |
| `P137` | `L85_RCSMinusZThruster` | `(-4.4075, 4.84, -2.65)` | target | `ACTUATOR_COMMAND` |
| `P138` | `L85_RCSPlusXThruster` | `(-4.4625, 5.56, -0.25)` | source | `ANALOG_FEEDBACK` |
| `P139` | `L85_RCSPlusXThruster` | `(-4.3575, 5.56, -0.25)` | target | `ACTUATOR_COMMAND` |
| `P140` | `L85_RCSPlusYThruster` | `(-5.0625, 5.74, -1.35)` | source | `ANALOG_FEEDBACK` |
| `P141` | `L85_RCSPlusYThruster` | `(-4.9575, 5.74, -1.35)` | target | `ACTUATOR_COMMAND` |
| `P142` | `L85_RCSPlusZThruster` | `(-5.7125, 4.84, 0.5)` | source | `ANALOG_FEEDBACK` |
| `P143` | `L85_RCSPlusZThruster` | `(-5.6075, 4.84, 0.5)` | target | `ACTUATOR_COMMAND` |
| `P144` | `L85_ReactionControlPropellantManifold` | `(-5.7625, 4.44, -0.15)` | source | `ANALOG_FEEDBACK` |
| `P145` | `L85_ReactionControlPropellantManifold` | `(-5.6575, 4.44, -0.15)` | source | `DIGITAL_STATUS` |
| `P146` | `L85_ReactionControlPropellantManifold` | `(-5.5525, 4.44, -0.15)` | target | `ANALOG_FEEDBACK` |
| `P147` | `L85_ReactionControlPropellantManifold` | `(-5.4475, 4.44, -0.15)` | target | `ANALOG_FEEDBACK` |
| `P148` | `L85_RelativeNavigationFusionComputer` | `(-6.1725, 1.51, 4.98)` | source | `ANALOG_FEEDBACK` |
| `P149` | `L85_RelativeNavigationFusionComputer` | `(-6.0675, 1.51, 4.98)` | source | `ANALOG_FEEDBACK` |
| `P150` | `L85_RelativeNavigationFusionComputer` | `(-5.9625, 1.51, 4.98)` | source | `DATA_STREAM` |
| `P151` | `L85_RelativeNavigationFusionComputer` | `(-5.8575, 1.51, 4.98)` | source | `DATA_STREAM` |
| `P152` | `L85_RelativeNavigationFusionComputer` | `(-5.7525, 1.51, 4.98)` | source | `DIGITAL_CONTROL` |
| `P153` | `L85_RelativeNavigationFusionComputer` | `(-5.6475, 1.51, 4.98)` | source | `DIGITAL_CONTROL` |
| `P154` | `L85_RelativeNavigationFusionComputer` | `(-6.1725, 1.615, 4.98)` | target | `ANALOG_FEEDBACK` |
| `P155` | `L85_RelativeNavigationFusionComputer` | `(-6.0675, 1.615, 4.98)` | target | `ANALOG_FEEDBACK` |
| `P156` | `L85_RelativeNavigationFusionComputer` | `(-5.9625, 1.615, 4.98)` | target | `DATA_STREAM` |
| `P157` | `L85_RelativeNavigationFusionComputer` | `(-5.8575, 1.615, 4.98)` | target | `DATA_STREAM` |
| `P158` | `L85_RelativeNavigationFusionComputer` | `(-5.7525, 1.615, 4.98)` | target | `DIGITAL_STATUS` |
| `P159` | `L85_RelativeNavigationFusionComputer` | `(-5.6475, 1.615, 4.98)` | target | `DIGITAL_STATUS` |
| `P160` | `L85_RelativeNavigationFusionComputer` | `(-6.1725, 1.72, 4.98)` | target | `POWER_CONTROL` |
| `P161` | `L85_RelativeOrbitTrajectoryHologram` | `(-0.2625, 7.59, -5.05)` | target | `DATA_STREAM` |
| `P162` | `L85_RelativePositionVelocityEstimator` | `(-7.5125, 1.51, 4.98)` | source | `ANALOG_FEEDBACK` |
| `P163` | `L85_RelativePositionVelocityEstimator` | `(-7.4075, 1.51, 4.98)` | source | `ANALOG_FEEDBACK` |
| `P164` | `L85_RelativePositionVelocityEstimator` | `(-7.3025, 1.51, 4.98)` | source | `DATA_STREAM` |
| `P165` | `L85_RelativePositionVelocityEstimator` | `(-7.1975, 1.51, 4.98)` | source | `DATA_STREAM` |
| `P166` | `L85_RelativePositionVelocityEstimator` | `(-7.0925, 1.51, 4.98)` | target | `ANALOG_FEEDBACK` |
| `P167` | `L85_RelativePositionVelocityEstimator` | `(-6.9875, 1.51, 4.98)` | target | `ANALOG_FEEDBACK` |
| `P168` | `L85_RelativeTrajectoryPlanningComputer` | `(-3.4825, 1.51, 4.98)` | source | `DATA_STREAM` |
| `P169` | `L85_RelativeTrajectoryPlanningComputer` | `(-3.3775, 1.51, 4.98)` | source | `DATA_STREAM` |
| `P170` | `L85_RelativeTrajectoryPlanningComputer` | `(-3.2725, 1.51, 4.98)` | target | `ANALOG_FEEDBACK` |
| `P171` | `L85_RelativeTrajectoryPlanningComputer` | `(-3.1675, 1.51, 4.98)` | target | `DATA_STREAM` |
| `P172` | `L85_RelativeTrajectoryPlanningComputer` | `(-3.0625, 1.51, 4.98)` | target | `DIGITAL_STATUS` |
| `P173` | `L85_SecondaryScanningLiDAR` | `(-3.1625, 4.34, -0.8)` | source | `ANALOG_FEEDBACK` |
| `P174` | `L85_SecondaryScanningLiDAR` | `(-3.0575, 4.34, -0.8)` | source | `DIGITAL_STATUS` |
| `P175` | `L85_SecondaryScanningLiDAR` | `(-2.9525, 4.34, -0.8)` | target | `DIGITAL_CONTROL` |
| `P176` | `L85_ShortRangeRendezvousRadar` | `(-3.4625, 4.84, -1.9)` | source | `ANALOG_FEEDBACK` |
| `P177` | `L85_ShortRangeRendezvousRadar` | `(-3.3575, 4.84, -1.9)` | source | `ANALOG_FEEDBACK` |
| `P178` | `L85_ShortRangeRendezvousRadar` | `(-3.2525, 4.84, -1.9)` | target | `DIGITAL_CONTROL` |
| `P179` | `L85_SixActuatorDockingLatchArray` | `(2.3875, 4.84, -1.2)` | source | `ANALOG_FEEDBACK` |
| `P180` | `L85_SixActuatorDockingLatchArray` | `(2.4925, 4.84, -1.2)` | target | `ACTUATOR_COMMAND` |
| `P181` | `L85_SixAxisContactForceSensor` | `(2.4575, 4.84, -1.2)` | source | `ANALOG_FEEDBACK` |
| `P182` | `L85_SixDegreeFreedomFlightController` | `(-0.7825, 1.51, 4.98)` | source | `ACTUATOR_COMMAND` |
| `P183` | `L85_SixDegreeFreedomFlightController` | `(-0.6775, 1.51, 4.98)` | source | `ACTUATOR_COMMAND` |
| `P184` | `L85_SixDegreeFreedomFlightController` | `(-0.5725, 1.51, 4.98)` | source | `ANALOG_FEEDBACK` |
| `P185` | `L85_SixDegreeFreedomFlightController` | `(-0.4675, 1.51, 4.98)` | source | `DATA_STREAM` |
| `P186` | `L85_SixDegreeFreedomFlightController` | `(-0.3625, 1.51, 4.98)` | source | `DIGITAL_STATUS` |
| `P187` | `L85_SixDegreeFreedomFlightController` | `(-0.2575, 1.51, 4.98)` | target | `ACTUATOR_COMMAND` |
| `P188` | `L85_SixDegreeFreedomFlightController` | `(-0.7825, 1.615, 4.98)` | target | `ACTUATOR_COMMAND` |
| `P189` | `L85_SixDegreeFreedomFlightController` | `(-0.6775, 1.615, 4.98)` | target | `ANALOG_FEEDBACK` |
| `P190` | `L85_SixDegreeFreedomFlightController` | `(-0.5725, 1.615, 4.98)` | target | `POWER_CONTROL` |
| `P191` | `L85_SixDegreeFreedomFlightController` | `(-0.4675, 1.615, 4.98)` | target | `SAFETY_CONTROL` |
| `P192` | `L85_SpacecraftInertialMeasurementUnit` | `(-4.0125, 4.34, -0.67)` | source | `ANALOG_FEEDBACK` |
| `P193` | `L85_SpacecraftInertialMeasurementUnit` | `(-3.9075, 4.34, -0.67)` | source | `ANALOG_FEEDBACK` |
| `P194` | `L85_SpacecraftInertialMeasurementUnit` | `(-3.8025, 4.34, -0.67)` | source | `DIGITAL_STATUS` |
| `P195` | `L85_StationApproachBeaconCluster` | `(5.5375, 4.84, -1.15)` | source | `DATA_STREAM` |
| `P196` | `L85_StationApproachIlluminationRig` | `(0.7375, 6.89, -2.85)` | source | `DIGITAL_STATUS` |
| `P197` | `L85_StationOpticalTargetMarkerArray` | `(2.7375, 4.84, -1.2)` | source | `DATA_STREAM` |
| `P198` | `L85_StereoNavigationCameraLeft` | `(-3.5125, 5.16, -0.65)` | source | `DATA_STREAM` |
| `P199` | `L85_StereoNavigationCameraRight` | `(-3.5125, 4.54, -0.65)` | source | `DATA_STREAM` |
| `P200` | `L85_StuckThrusterValveFaultEmulator` | `(11.4175, 1.51, 4.98)` | source | `DIGITAL_STATUS` |
| `P201` | `L85_StuckThrusterValveFaultEmulator` | `(11.5225, 1.51, 4.98)` | source | `SAFETY_CONTROL` |
| `P202` | `L85_StuckThrusterValveFaultEmulator` | `(11.6275, 1.51, 4.98)` | target | `DIGITAL_CONTROL` |
| `P203` | `L85_ThreeDimensionalApproachCorridorHologram` | `(-0.2625, 4.84, -1.2)` | target | `DATA_STREAM` |
| `P204` | `L85_ThreeDimensionalApproachCorridorHologram` | `(-0.1575, 4.84, -1.2)` | target | `DATA_STREAM` |
| `P205` | `L85_ThrusterFlowAndPressureMonitor` | `(-10.2925, 1.51, 4.98)` | source | `ANALOG_FEEDBACK` |
| `P206` | `L85_ThrusterFlowAndPressureMonitor` | `(-10.1875, 1.51, 4.98)` | source | `ANALOG_FEEDBACK` |
| `P207` | `L85_ThrusterFlowAndPressureMonitor` | `(-10.0825, 1.51, 4.98)` | target | `ANALOG_FEEDBACK` |
| `P208` | `L85_ThrusterFlowAndPressureMonitor` | `(-9.9775, 1.51, 4.98)` | target | `ANALOG_FEEDBACK` |
| `P209` | `L85_ThrusterForceAllocationComputer` | `(0.5675, 1.51, 4.98)` | source | `ACTUATOR_COMMAND` |
| `P210` | `L85_ThrusterForceAllocationComputer` | `(0.6725, 1.51, 4.98)` | source | `ANALOG_FEEDBACK` |
| `P211` | `L85_ThrusterForceAllocationComputer` | `(0.7775, 1.51, 4.98)` | target | `ACTUATOR_COMMAND` |
| `P212` | `L85_ThrusterForceAllocationComputer` | `(0.8825, 1.51, 4.98)` | target | `ACTUATOR_COMMAND` |
| `P213` | `L85_ThrusterForceAllocationComputer` | `(0.9875, 1.51, 4.98)` | target | `DATA_STREAM` |
| `P214` | `L85_ThrusterForceAllocationComputer` | `(1.0925, 1.51, 4.98)` | target | `POWER_CONTROL` |
| `P215` | `L85_ThrusterForceAllocationComputer` | `(0.5675, 1.615, 4.98)` | target | `SAFETY_CONTROL` |
| `P216` | `L85_ThrusterValveDriverRack` | `(-11.6625, 1.51, 4.98)` | source | `ACTUATOR_COMMAND` |
| `P217` | `L85_ThrusterValveDriverRack` | `(-11.5575, 1.51, 4.98)` | source | `ACTUATOR_COMMAND` |
| `P218` | `L85_ThrusterValveDriverRack` | `(-11.4525, 1.51, 4.98)` | source | `DIGITAL_CONTROL` |
| `P219` | `L85_ThrusterValveDriverRack` | `(-11.3475, 1.51, 4.98)` | source | `DIGITAL_STATUS` |
| `P220` | `L85_ThrusterValveDriverRack` | `(-11.2425, 1.51, 4.98)` | target | `ACTUATOR_COMMAND` |
| `P221` | `L85_ThrusterValveDriverRack` | `(-11.1375, 1.51, 4.98)` | target | `DIGITAL_STATUS` |
| `P222` | `L85_ThrusterValveDriverRack` | `(-11.6625, 1.615, 4.98)` | target | `POWER_CONTROL` |
| `P223` | `L85_ThrusterValveDriverRack` | `(-11.5575, 1.615, 4.98)` | target | `SAFETY_CONTROL` |
| `P224` | `L85_SixAxisContactForceSensor` | `(2.5625, 4.84, -1.2)` | unused spare art socket | `UNASSIGNED` |

All 224 records are `MESH_ONLY_SOCKET_NOT_SPARKTERMINAL`. They are decorative placement coordinates. P224 is a spare art socket. Do not merge logical ports merely because a single art socket is their shared display point.


## ANNEX E — ALL 292 EXACT PROPOSED LOGICAL ENDPOINT CONTACTS

| Wire ID | Model | Visual anchor | Exact proposed logical `SparkTerminal` identity | Dir. | Signal kind | Electrical domain |
|---|---|---|---|---|---|---|
| `W001` | `L85_Protected5VControlSupply` | `P114` | `W001_DOCKING_SAFETY_PLC_CONTROL_POWER_OUT` | OUTPUT | `POWER_CONTROL` | `CONTROL_5V` |
| `W001` | `L85_DockingSafetyPLC` | `P050` | `W001_DOCKING_SAFETY_PLC_CONTROL_POWER_IN` | INPUT | `POWER_CONTROL` | `CONTROL_5V` |
| `W002` | `L85_Protected5VControlSupply` | `P115` | `W002_CAPTURE_SEQUENCING_PLC_5V_LOGIC_OUT` | OUTPUT | `POWER_CONTROL` | `CONTROL_5V` |
| `W002` | `L85_DockingMechanismSequencingPLC` | `P035` | `W002_CAPTURE_SEQUENCING_PLC_5V_LOGIC_IN` | INPUT | `POWER_CONTROL` | `CONTROL_5V` |
| `W003` | `L85_Protected5VControlSupply` | `P114` | `W003_NAVIGATION_FUSION_COMPUTER_5V_OUT` | OUTPUT | `POWER_CONTROL` | `CONTROL_5V` |
| `W003` | `L85_RelativeNavigationFusionComputer` | `P160` | `W003_NAVIGATION_FUSION_COMPUTER_5V_IN` | INPUT | `POWER_CONTROL` | `CONTROL_5V` |
| `W004` | `L85_Protected5VControlSupply` | `P115` | `W004_SIX_DEGREE_FLIGHT_CONTROL_LOGIC_5V_OUT` | OUTPUT | `POWER_CONTROL` | `CONTROL_5V` |
| `W004` | `L85_SixDegreeFreedomFlightController` | `P190` | `W004_SIX_DEGREE_FLIGHT_CONTROL_LOGIC_5V_IN` | INPUT | `POWER_CONTROL` | `CONTROL_5V` |
| `W005` | `L85_Protected5VControlSupply` | `P114` | `W005_THRUSTER_ALLOCATION_CONTROL_5V_OUT` | OUTPUT | `POWER_CONTROL` | `CONTROL_5V` |
| `W005` | `L85_ThrusterForceAllocationComputer` | `P214` | `W005_THRUSTER_ALLOCATION_CONTROL_5V_IN` | INPUT | `POWER_CONTROL` | `CONTROL_5V` |
| `W006` | `L85_Protected5VControlSupply` | `P115` | `W006_VALVE_CONTROLLER_5V_COMMAND_POWER_OUT` | OUTPUT | `POWER_CONTROL` | `CONTROL_5V` |
| `W006` | `L85_ThrusterValveDriverRack` | `P222` | `W006_VALVE_CONTROLLER_5V_COMMAND_POWER_IN` | INPUT | `POWER_CONTROL` | `CONTROL_5V` |
| `W007` | `L85_Protected5VControlSupply` | `P114` | `W007_LIDAR_SIGNAL_CONDITIONING_5V_OUT` | OUTPUT | `POWER_CONTROL` | `CONTROL_5V` |
| `W007` | `L85_PrimaryScanningLiDAR` | `P111` | `W007_LIDAR_SIGNAL_CONDITIONING_5V_IN` | INPUT | `POWER_CONTROL` | `CONTROL_5V` |
| `W008` | `L85_Protected5VControlSupply` | `P115` | `W008_DOCKING_CAMERA_MONITOR_5V_OUT` | OUTPUT | `POWER_CONTROL` | `CONTROL_5V` |
| `W008` | `L85_PrecisionDockingCameraMonitor` | `P096` | `W008_DOCKING_CAMERA_MONITOR_5V_IN` | INPUT | `POWER_CONTROL` | `CONTROL_5V` |
| `W009` | `L85_Protected5VControlSupply` | `P114` | `W009_SCADA_CONTROL_AND_VISUALIZATION_5V_OUT` | OUTPUT | `POWER_CONTROL` | `CONTROL_5V` |
| `W009` | `L85_HolographicRendezvousSCADA` | `P074` | `W009_SCADA_CONTROL_AND_VISUALIZATION_5V_IN` | INPUT | `POWER_CONTROL` | `CONTROL_5V` |
| `W010` | `L85_Protected5VControlSupply` | `P115` | `W010_TERMINAL_DIAGNOSTIC_MONITOR_5V_OUT` | OUTPUT | `POWER_CONTROL` | `CONTROL_5V` |
| `W010` | `L85_ElectricalTerminalDiagnosticRack` | `P057` | `W010_TERMINAL_DIAGNOSTIC_MONITOR_5V_IN` | INPUT | `POWER_CONTROL` | `CONTROL_5V` |
| `W011` | `L85_SpacecraftInertialMeasurementUnit` | `P192` | `W011_IMU_MEASURED_ACCELERATION_AND_ANGULAR_RATE_OUT` | OUTPUT | `ANALOG_FEEDBACK` | `ISOLATED_CONTROL_SIGNAL` |
| `W011` | `L85_RelativeNavigationFusionComputer` | `P154` | `W011_IMU_MEASURED_ACCELERATION_AND_ANGULAR_RATE_IN` | INPUT | `ANALOG_FEEDBACK` | `ISOLATED_CONTROL_SIGNAL` |
| `W012` | `L85_StereoNavigationCameraLeft` | `P198` | `W012_LEFT_NAVIGATION_CAMERA_IMAGE_SAMPLES_OUT` | OUTPUT | `DATA_STREAM` | `ISOLATED_CONTROL_SIGNAL` |
| `W012` | `L85_OpticalTargetPatternTracker` | `P087` | `W012_LEFT_NAVIGATION_CAMERA_IMAGE_SAMPLES_IN` | INPUT | `DATA_STREAM` | `ISOLATED_CONTROL_SIGNAL` |
| `W013` | `L85_StereoNavigationCameraRight` | `P199` | `W013_RIGHT_NAVIGATION_CAMERA_IMAGE_SAMPLES_OUT` | OUTPUT | `DATA_STREAM` | `ISOLATED_CONTROL_SIGNAL` |
| `W013` | `L85_OpticalTargetPatternTracker` | `P088` | `W013_RIGHT_NAVIGATION_CAMERA_IMAGE_SAMPLES_IN` | INPUT | `DATA_STREAM` | `ISOLATED_CONTROL_SIGNAL` |
| `W014` | `L85_OpticalTargetPatternTracker` | `P085` | `W014_STEREO_TRIANGULATED_OPTICAL_TARGET_POSE_OUT` | OUTPUT | `DATA_STREAM` | `ISOLATED_CONTROL_SIGNAL` |
| `W014` | `L85_RelativeNavigationFusionComputer` | `P156` | `W014_STEREO_TRIANGULATED_OPTICAL_TARGET_POSE_IN` | INPUT | `DATA_STREAM` | `ISOLATED_CONTROL_SIGNAL` |
| `W015` | `L85_PrimaryScanningLiDAR` | `P108` | `W015_PRIMARY_LIDAR_MEASURED_RANGE_SCAN_OUT` | OUTPUT | `ANALOG_FEEDBACK` | `ISOLATED_CONTROL_SIGNAL` |
| `W015` | `L85_RelativePositionVelocityEstimator` | `P166` | `W015_PRIMARY_LIDAR_MEASURED_RANGE_SCAN_IN` | INPUT | `ANALOG_FEEDBACK` | `ISOLATED_CONTROL_SIGNAL` |
| `W016` | `L85_SecondaryScanningLiDAR` | `P173` | `W016_SECONDARY_LIDAR_MEASURED_RANGE_SCAN_OUT` | OUTPUT | `ANALOG_FEEDBACK` | `ISOLATED_CONTROL_SIGNAL` |
| `W016` | `L85_RelativePositionVelocityEstimator` | `P167` | `W016_SECONDARY_LIDAR_MEASURED_RANGE_SCAN_IN` | INPUT | `ANALOG_FEEDBACK` | `ISOLATED_CONTROL_SIGNAL` |
| `W017` | `L85_ShortRangeRendezvousRadar` | `P176` | `W017_RADAR_INDEPENDENT_RELATIVE_RANGE_RATE_OUT` | OUTPUT | `ANALOG_FEEDBACK` | `ISOLATED_CONTROL_SIGNAL` |
| `W017` | `L85_RelativePositionVelocityEstimator` | `P166` | `W017_RADAR_INDEPENDENT_RELATIVE_RANGE_RATE_IN` | INPUT | `ANALOG_FEEDBACK` | `ISOLATED_CONTROL_SIGNAL` |
| `W018` | `L85_DockingBeaconReceiver` | `P014` | `W018_DOCK_TARGET_BEACON_IDENTITY_VALIDATION_OUT` | OUTPUT | `DIGITAL_STATUS` | `ISOLATED_CONTROL_SIGNAL` |
| `W018` | `L85_RelativeNavigationFusionComputer` | `P158` | `W018_DOCK_TARGET_BEACON_IDENTITY_VALIDATION_IN` | INPUT | `DIGITAL_STATUS` | `ISOLATED_CONTROL_SIGNAL` |
| `W019` | `L85_OpticalTargetPatternTracker` | `P084` | `W019_DOCKING_FIDUCIAL_PATTERN_CONFIDENCE_OUT` | OUTPUT | `ANALOG_FEEDBACK` | `ISOLATED_CONTROL_SIGNAL` |
| `W019` | `L85_RelativeNavigationFusionComputer` | `P155` | `W019_DOCKING_FIDUCIAL_PATTERN_CONFIDENCE_IN` | INPUT | `ANALOG_FEEDBACK` | `ISOLATED_CONTROL_SIGNAL` |
| `W020` | `L85_RelativePositionVelocityEstimator` | `P164` | `W020_ESTIMATED_RELATIVE_POSITION_AND_SPEED_OUT` | OUTPUT | `DATA_STREAM` | `ISOLATED_CONTROL_SIGNAL` |
| `W020` | `L85_RelativeNavigationFusionComputer` | `P157` | `W020_ESTIMATED_RELATIVE_POSITION_AND_SPEED_IN` | INPUT | `DATA_STREAM` | `ISOLATED_CONTROL_SIGNAL` |
| `W021` | `L85_SpacecraftInertialMeasurementUnit` | `P193` | `W021_INERTIAL_RELATIVE_STATE_PROPAGATION_SAMPLES_OUT` | OUTPUT | `ANALOG_FEEDBACK` | `ISOLATED_CONTROL_SIGNAL` |
| `W021` | `L85_RelativePositionVelocityEstimator` | `P167` | `W021_INERTIAL_RELATIVE_STATE_PROPAGATION_SAMPLES_IN` | INPUT | `ANALOG_FEEDBACK` | `ISOLATED_CONTROL_SIGNAL` |
| `W022` | `L85_PrecisionFlightTimeReferenceClock` | `P100` | `W022_NAVIGATION_TIMESTAMP_AND_CLOCK_SYNCHRONIZATIO_OUT` | OUTPUT | `DIGITAL_STATUS` | `ISOLATED_CONTROL_SIGNAL` |
| `W022` | `L85_RelativeNavigationFusionComputer` | `P159` | `W022_NAVIGATION_TIMESTAMP_AND_CLOCK_SYNCHRONIZATIO_IN` | INPUT | `DIGITAL_STATUS` | `ISOLATED_CONTROL_SIGNAL` |
| `W023` | `L85_RelativeNavigationFusionComputer` | `P150` | `W023_NAVIGATION_COVARIANCE_AND_CONFIDENCE_ELLIPSOI_OUT` | OUTPUT | `DATA_STREAM` | `ISOLATED_CONTROL_SIGNAL` |
| `W023` | `L85_NavigationUncertaintyVolumeDisplay` | `P082` | `W023_NAVIGATION_COVARIANCE_AND_CONFIDENCE_ELLIPSOI_IN` | INPUT | `DATA_STREAM` | `ISOLATED_CONTROL_SIGNAL` |
| `W024` | `L85_PrimaryScanningLiDAR` | `P109` | `W024_PRIMARY_LIDAR_SCAN_VALIDITY_DIAGNOSTIC_OUT` | OUTPUT | `DIGITAL_STATUS` | `ISOLATED_CONTROL_SIGNAL` |
| `W024` | `L85_LiDARBlindSpotFaultEmulator` | `P077` | `W024_PRIMARY_LIDAR_SCAN_VALIDITY_DIAGNOSTIC_IN` | INPUT | `DIGITAL_STATUS` | `ISOLATED_CONTROL_SIGNAL` |
| `W025` | `L85_SecondaryScanningLiDAR` | `P174` | `W025_SECONDARY_LIDAR_SENSOR_REDUNDANCY_DIAGNOSTIC_OUT` | OUTPUT | `DIGITAL_STATUS` | `ISOLATED_CONTROL_SIGNAL` |
| `W025` | `L85_LiDARBlindSpotFaultEmulator` | `P078` | `W025_SECONDARY_LIDAR_SENSOR_REDUNDANCY_DIAGNOSTIC_IN` | INPUT | `DIGITAL_STATUS` | `ISOLATED_CONTROL_SIGNAL` |
| `W026` | `L85_LiDARBlindSpotFaultEmulator` | `P075` | `W026_INJECTED_LIDAR_BLIND_SPOT_SIGNAL_QUALITY_OUT` | OUTPUT | `DIGITAL_STATUS` | `ISOLATED_CONTROL_SIGNAL` |
| `W026` | `L85_RelativeNavigationFusionComputer` | `P158` | `W026_INJECTED_LIDAR_BLIND_SPOT_SIGNAL_QUALITY_IN` | INPUT | `DIGITAL_STATUS` | `ISOLATED_CONTROL_SIGNAL` |
| `W027` | `L85_RelativeNavigationFusionComputer` | `P152` | `W027_PRIMARY_LIDAR_TIMED_SCAN_TRIGGER_OUT` | OUTPUT | `DIGITAL_CONTROL` | `ISOLATED_CONTROL_SIGNAL` |
| `W027` | `L85_PrimaryScanningLiDAR` | `P110` | `W027_PRIMARY_LIDAR_TIMED_SCAN_TRIGGER_IN` | INPUT | `DIGITAL_CONTROL` | `ISOLATED_CONTROL_SIGNAL` |
| `W028` | `L85_RelativeNavigationFusionComputer` | `P153` | `W028_SECONDARY_LIDAR_TIMED_SCAN_TRIGGER_OUT` | OUTPUT | `DIGITAL_CONTROL` | `ISOLATED_CONTROL_SIGNAL` |
| `W028` | `L85_SecondaryScanningLiDAR` | `P175` | `W028_SECONDARY_LIDAR_TIMED_SCAN_TRIGGER_IN` | INPUT | `DIGITAL_CONTROL` | `ISOLATED_CONTROL_SIGNAL` |
| `W029` | `L85_RelativeNavigationFusionComputer` | `P152` | `W029_RENDEZVOUS_RADAR_MODE_REQUEST_OUT` | OUTPUT | `DIGITAL_CONTROL` | `ISOLATED_CONTROL_SIGNAL` |
| `W029` | `L85_ShortRangeRendezvousRadar` | `P178` | `W029_RENDEZVOUS_RADAR_MODE_REQUEST_IN` | INPUT | `DIGITAL_CONTROL` | `ISOLATED_CONTROL_SIGNAL` |
| `W030` | `L85_RelativeNavigationFusionComputer` | `P153` | `W030_TARGET_OPTICAL_ACQUISITION_COMMAND_OUT` | OUTPUT | `DIGITAL_CONTROL` | `ISOLATED_CONTROL_SIGNAL` |
| `W030` | `L85_OpticalTargetPatternTracker` | `P089` | `W030_TARGET_OPTICAL_ACQUISITION_COMMAND_IN` | INPUT | `DIGITAL_CONTROL` | `ISOLATED_CONTROL_SIGNAL` |
| `W031` | `L85_DockingBeaconReceiver` | `P013` | `W031_DOCK_BEACON_ASSOCIATION_TO_OPTICAL_MARKERS_OUT` | OUTPUT | `DATA_STREAM` | `ISOLATED_CONTROL_SIGNAL` |
| `W031` | `L85_OpticalTargetPatternTracker` | `P087` | `W031_DOCK_BEACON_ASSOCIATION_TO_OPTICAL_MARKERS_IN` | INPUT | `DATA_STREAM` | `ISOLATED_CONTROL_SIGNAL` |
| `W032` | `L85_RelativePositionVelocityEstimator` | `P165` | `W032_RELATIVE_RANGE_AND_VELOCITY_TELEMETRY_DISPLAY_OUT` | OUTPUT | `DATA_STREAM` | `ISOLATED_CONTROL_SIGNAL` |
| `W032` | `L85_HolographicRendezvousSCADA` | `P070` | `W032_RELATIVE_RANGE_AND_VELOCITY_TELEMETRY_DISPLAY_IN` | INPUT | `DATA_STREAM` | `ISOLATED_CONTROL_SIGNAL` |
| `W033` | `L85_NavigationUncertaintyVolumeDisplay` | `P081` | `W033_COVARIANCE_VISUALIZATION_AND_QUALITY_TRACE_OUT` | OUTPUT | `DATA_STREAM` | `ISOLATED_CONTROL_SIGNAL` |
| `W033` | `L85_HolographicRendezvousSCADA` | `P071` | `W033_COVARIANCE_VISUALIZATION_AND_QUALITY_TRACE_IN` | INPUT | `DATA_STREAM` | `ISOLATED_CONTROL_SIGNAL` |
| `W034` | `L85_SpacecraftInertialMeasurementUnit` | `P194` | `W034_RAW_SPACECRAFT_IMU_HEALTH_TELEMETRY_OUT` | OUTPUT | `DIGITAL_STATUS` | `ISOLATED_CONTROL_SIGNAL` |
| `W034` | `L85_HolographicRendezvousSCADA` | `P072` | `W034_RAW_SPACECRAFT_IMU_HEALTH_TELEMETRY_IN` | INPUT | `DIGITAL_STATUS` | `ISOLATED_CONTROL_SIGNAL` |
| `W035` | `L85_SixDegreeFreedomFlightController` | `P182` | `W035_REQUESTED_SIX_DEGREE_THRUSTER_WRENCH_OUT` | OUTPUT | `ACTUATOR_COMMAND` | `ISOLATED_CONTROL_SIGNAL` |
| `W035` | `L85_ThrusterForceAllocationComputer` | `P211` | `W035_REQUESTED_SIX_DEGREE_THRUSTER_WRENCH_IN` | INPUT | `ACTUATOR_COMMAND` | `ISOLATED_CONTROL_SIGNAL` |
| `W036` | `L85_ThrusterForceAllocationComputer` | `P209` | `W036_THRUSTER_COMMAND_ALLOCATION_AND_PULSE_TRAIN_OUT` | OUTPUT | `ACTUATOR_COMMAND` | `ISOLATED_CONTROL_SIGNAL` |
| `W036` | `L85_ThrusterValveDriverRack` | `P220` | `W036_THRUSTER_COMMAND_ALLOCATION_AND_PULSE_TRAIN_IN` | INPUT | `ACTUATOR_COMMAND` | `ISOLATED_CONTROL_SIGNAL` |
| `W037` | `L85_ThrusterValveDriverRack` | `P216` | `W037_RCS_THRUSTER_X_VALVE_PULSE_COMMAND_OUT` | OUTPUT | `ACTUATOR_COMMAND` | `ISOLATED_CONTROL_SIGNAL` |
| `W037` | `L85_RCSPlusXThruster` | `P139` | `W037_RCS_THRUSTER_X_VALVE_PULSE_COMMAND_IN` | INPUT | `ACTUATOR_COMMAND` | `ISOLATED_CONTROL_SIGNAL` |
| `W038` | `L85_ThrusterValveDriverRack` | `P217` | `W038_RCS_THRUSTER_Y_VALVE_PULSE_COMMAND_OUT` | OUTPUT | `ACTUATOR_COMMAND` | `ISOLATED_CONTROL_SIGNAL` |
| `W038` | `L85_RCSMinusXThruster` | `P133` | `W038_RCS_THRUSTER_Y_VALVE_PULSE_COMMAND_IN` | INPUT | `ACTUATOR_COMMAND` | `ISOLATED_CONTROL_SIGNAL` |
| `W039` | `L85_ThrusterValveDriverRack` | `P216` | `W039_RCS_THRUSTER_Z_VALVE_PULSE_COMMAND_OUT` | OUTPUT | `ACTUATOR_COMMAND` | `ISOLATED_CONTROL_SIGNAL` |
| `W039` | `L85_RCSPlusYThruster` | `P141` | `W039_RCS_THRUSTER_Z_VALVE_PULSE_COMMAND_IN` | INPUT | `ACTUATOR_COMMAND` | `ISOLATED_CONTROL_SIGNAL` |
| `W040` | `L85_ThrusterValveDriverRack` | `P217` | `W040_RCS_THRUSTER_X_VALVE_PULSE_COMMAND_OUT` | OUTPUT | `ACTUATOR_COMMAND` | `ISOLATED_CONTROL_SIGNAL` |
| `W040` | `L85_RCSMinusYThruster` | `P135` | `W040_RCS_THRUSTER_X_VALVE_PULSE_COMMAND_IN` | INPUT | `ACTUATOR_COMMAND` | `ISOLATED_CONTROL_SIGNAL` |
| `W041` | `L85_ThrusterValveDriverRack` | `P216` | `W041_RCS_THRUSTER_Y_VALVE_PULSE_COMMAND_OUT` | OUTPUT | `ACTUATOR_COMMAND` | `ISOLATED_CONTROL_SIGNAL` |
| `W041` | `L85_RCSPlusZThruster` | `P143` | `W041_RCS_THRUSTER_Y_VALVE_PULSE_COMMAND_IN` | INPUT | `ACTUATOR_COMMAND` | `ISOLATED_CONTROL_SIGNAL` |
| `W042` | `L85_ThrusterValveDriverRack` | `P217` | `W042_RCS_THRUSTER_Z_VALVE_PULSE_COMMAND_OUT` | OUTPUT | `ACTUATOR_COMMAND` | `ISOLATED_CONTROL_SIGNAL` |
| `W042` | `L85_RCSMinusZThruster` | `P137` | `W042_RCS_THRUSTER_Z_VALVE_PULSE_COMMAND_IN` | INPUT | `ACTUATOR_COMMAND` | `ISOLATED_CONTROL_SIGNAL` |
| `W043` | `L85_RCSPlusXThruster` | `P138` | `W043_RCS_THRUSTER_X_ACTUAL_PULSE_FEEDBACK_OUT` | OUTPUT | `ANALOG_FEEDBACK` | `ISOLATED_CONTROL_SIGNAL` |
| `W043` | `L85_ThrusterFlowAndPressureMonitor` | `P207` | `W043_RCS_THRUSTER_X_ACTUAL_PULSE_FEEDBACK_IN` | INPUT | `ANALOG_FEEDBACK` | `ISOLATED_CONTROL_SIGNAL` |
| `W044` | `L85_RCSMinusXThruster` | `P132` | `W044_RCS_THRUSTER_Y_ACTUAL_PULSE_FEEDBACK_OUT` | OUTPUT | `ANALOG_FEEDBACK` | `ISOLATED_CONTROL_SIGNAL` |
| `W044` | `L85_ThrusterFlowAndPressureMonitor` | `P208` | `W044_RCS_THRUSTER_Y_ACTUAL_PULSE_FEEDBACK_IN` | INPUT | `ANALOG_FEEDBACK` | `ISOLATED_CONTROL_SIGNAL` |
| `W045` | `L85_RCSPlusYThruster` | `P140` | `W045_RCS_THRUSTER_Z_ACTUAL_PULSE_FEEDBACK_OUT` | OUTPUT | `ANALOG_FEEDBACK` | `ISOLATED_CONTROL_SIGNAL` |
| `W045` | `L85_ThrusterFlowAndPressureMonitor` | `P207` | `W045_RCS_THRUSTER_Z_ACTUAL_PULSE_FEEDBACK_IN` | INPUT | `ANALOG_FEEDBACK` | `ISOLATED_CONTROL_SIGNAL` |
| `W046` | `L85_RCSMinusYThruster` | `P134` | `W046_RCS_THRUSTER_X_ACTUAL_PULSE_FEEDBACK_OUT` | OUTPUT | `ANALOG_FEEDBACK` | `ISOLATED_CONTROL_SIGNAL` |
| `W046` | `L85_ThrusterFlowAndPressureMonitor` | `P208` | `W046_RCS_THRUSTER_X_ACTUAL_PULSE_FEEDBACK_IN` | INPUT | `ANALOG_FEEDBACK` | `ISOLATED_CONTROL_SIGNAL` |
| `W047` | `L85_RCSPlusZThruster` | `P142` | `W047_RCS_THRUSTER_Y_ACTUAL_PULSE_FEEDBACK_OUT` | OUTPUT | `ANALOG_FEEDBACK` | `ISOLATED_CONTROL_SIGNAL` |
| `W047` | `L85_ThrusterFlowAndPressureMonitor` | `P207` | `W047_RCS_THRUSTER_Y_ACTUAL_PULSE_FEEDBACK_IN` | INPUT | `ANALOG_FEEDBACK` | `ISOLATED_CONTROL_SIGNAL` |
| `W048` | `L85_RCSMinusZThruster` | `P136` | `W048_RCS_THRUSTER_Z_ACTUAL_PULSE_FEEDBACK_OUT` | OUTPUT | `ANALOG_FEEDBACK` | `ISOLATED_CONTROL_SIGNAL` |
| `W048` | `L85_ThrusterFlowAndPressureMonitor` | `P208` | `W048_RCS_THRUSTER_Z_ACTUAL_PULSE_FEEDBACK_IN` | INPUT | `ANALOG_FEEDBACK` | `ISOLATED_CONTROL_SIGNAL` |
| `W049` | `L85_ThrusterFlowAndPressureMonitor` | `P205` | `W049_ACTUAL_DELIVERED_IMPULSE_AND_PRESSURE_SAMPLES_OUT` | OUTPUT | `ANALOG_FEEDBACK` | `ISOLATED_CONTROL_SIGNAL` |
| `W049` | `L85_SixDegreeFreedomFlightController` | `P189` | `W049_ACTUAL_DELIVERED_IMPULSE_AND_PRESSURE_SAMPLES_IN` | INPUT | `ANALOG_FEEDBACK` | `ISOLATED_CONTROL_SIGNAL` |
| `W050` | `L85_PropellantTankA` | `P112` | `W050_PROPELLANT_TANK_A_AVAILABLE_FEED_PRESSURE_OUT` | OUTPUT | `ANALOG_FEEDBACK` | `ISOLATED_CONTROL_SIGNAL` |
| `W050` | `L85_ReactionControlPropellantManifold` | `P146` | `W050_PROPELLANT_TANK_A_AVAILABLE_FEED_PRESSURE_IN` | INPUT | `ANALOG_FEEDBACK` | `ISOLATED_CONTROL_SIGNAL` |
| `W051` | `L85_PropellantTankB` | `P113` | `W051_PROPELLANT_TANK_B_AVAILABLE_FEED_PRESSURE_OUT` | OUTPUT | `ANALOG_FEEDBACK` | `ISOLATED_CONTROL_SIGNAL` |
| `W051` | `L85_ReactionControlPropellantManifold` | `P147` | `W051_PROPELLANT_TANK_B_AVAILABLE_FEED_PRESSURE_IN` | INPUT | `ANALOG_FEEDBACK` | `ISOLATED_CONTROL_SIGNAL` |
| `W052` | `L85_ReactionControlPropellantManifold` | `P145` | `W052_PROPELLANT_MANIFOLD_DELIVERY_READINESS_OUT` | OUTPUT | `DIGITAL_STATUS` | `ISOLATED_CONTROL_SIGNAL` |
| `W052` | `L85_ThrusterValveDriverRack` | `P221` | `W052_PROPELLANT_MANIFOLD_DELIVERY_READINESS_IN` | INPUT | `DIGITAL_STATUS` | `ISOLATED_CONTROL_SIGNAL` |
| `W053` | `L85_PrecisionThrusterCalibrationFixture` | `P102` | `W053_MEASURED_THRUSTER_CALIBRATION_THRUST_TABLE_OUT` | OUTPUT | `DATA_STREAM` | `ISOLATED_CONTROL_SIGNAL` |
| `W053` | `L85_ThrusterForceAllocationComputer` | `P213` | `W053_MEASURED_THRUSTER_CALIBRATION_THRUST_TABLE_IN` | INPUT | `DATA_STREAM` | `ISOLATED_CONTROL_SIGNAL` |
| `W054` | `L85_StuckThrusterValveFaultEmulator` | `P201` | `W054_STUCK_THRUSTER_VALVE_MALFUNCTION_INJECTION_OUT` | OUTPUT | `SAFETY_CONTROL` | `ISOLATED_CONTROL_SIGNAL` |
| `W054` | `L85_ThrusterValveDriverRack` | `P223` | `W054_STUCK_THRUSTER_VALVE_MALFUNCTION_INJECTION_IN` | INPUT | `SAFETY_CONTROL` | `ISOLATED_CONTROL_SIGNAL` |
| `W055` | `L85_ThrusterValveDriverRack` | `P218` | `W055_FAULT_INJECTOR_CONFIGURED_ISOLATION_READINESS_OUT` | OUTPUT | `DIGITAL_CONTROL` | `ISOLATED_CONTROL_SIGNAL` |
| `W055` | `L85_StuckThrusterValveFaultEmulator` | `P202` | `W055_FAULT_INJECTOR_CONFIGURED_ISOLATION_READINESS_IN` | INPUT | `DIGITAL_CONTROL` | `ISOLATED_CONTROL_SIGNAL` |
| `W056` | `L85_FuelReserveAndAbortPredictor` | `P062` | `W056_ABORT_FUEL_RESERVE_THRUST_ALLOCATION_LIMIT_OUT` | OUTPUT | `SAFETY_CONTROL` | `ISOLATED_CONTROL_SIGNAL` |
| `W056` | `L85_ThrusterForceAllocationComputer` | `P215` | `W056_ABORT_FUEL_RESERVE_THRUST_ALLOCATION_LIMIT_IN` | INPUT | `SAFETY_CONTROL` | `ISOLATED_CONTROL_SIGNAL` |
| `W057` | `L85_ThrusterForceAllocationComputer` | `P210` | `W057_PREDICTED_COMMANDED_THRUSTER_FUEL_USAGE_OUT` | OUTPUT | `ANALOG_FEEDBACK` | `ISOLATED_CONTROL_SIGNAL` |
| `W057` | `L85_FuelReserveAndAbortPredictor` | `P063` | `W057_PREDICTED_COMMANDED_THRUSTER_FUEL_USAGE_IN` | INPUT | `ANALOG_FEEDBACK` | `ISOLATED_CONTROL_SIGNAL` |
| `W058` | `L85_ReactionControlPropellantManifold` | `P144` | `W058_PROPELLANT_FEED_PRESSURE_TRANSDUCER_RESPONSE_OUT` | OUTPUT | `ANALOG_FEEDBACK` | `ISOLATED_CONTROL_SIGNAL` |
| `W058` | `L85_ThrusterFlowAndPressureMonitor` | `P207` | `W058_PROPELLANT_FEED_PRESSURE_TRANSDUCER_RESPONSE_IN` | INPUT | `ANALOG_FEEDBACK` | `ISOLATED_CONTROL_SIGNAL` |
| `W059` | `L85_RelativeNavigationFusionComputer` | `P151` | `W059_FUSED_RELATIVE_STATE_FOR_ORBITAL_TRAJECTORY_P_OUT` | OUTPUT | `DATA_STREAM` | `ISOLATED_CONTROL_SIGNAL` |
| `W059` | `L85_RelativeTrajectoryPlanningComputer` | `P171` | `W059_FUSED_RELATIVE_STATE_FOR_ORBITAL_TRAJECTORY_P_IN` | INPUT | `DATA_STREAM` | `ISOLATED_CONTROL_SIGNAL` |
| `W060` | `L85_RelativeTrajectoryPlanningComputer` | `P168` | `W060_PLANNED_RENDEZVOUS_TRAJECTORY_AND_CORRIDOR_OUT` | OUTPUT | `DATA_STREAM` | `ISOLATED_CONTROL_SIGNAL` |
| `W060` | `L85_ProximityOperationsGuidanceComputer` | `P129` | `W060_PLANNED_RENDEZVOUS_TRAJECTORY_AND_CORRIDOR_IN` | INPUT | `DATA_STREAM` | `ISOLATED_CONTROL_SIGNAL` |
| `W061` | `L85_ProximityOperationsGuidanceComputer` | `P122` | `W061_DESIRED_SIX_DEGREE_TRANSLATIONAL_GUIDANCE_VEC_OUT` | OUTPUT | `ACTUATOR_COMMAND` | `ISOLATED_CONTROL_SIGNAL` |
| `W061` | `L85_SixDegreeFreedomFlightController` | `P187` | `W061_DESIRED_SIX_DEGREE_TRANSLATIONAL_GUIDANCE_VEC_IN` | INPUT | `ACTUATOR_COMMAND` | `ISOLATED_CONTROL_SIGNAL` |
| `W062` | `L85_SixDegreeFreedomFlightController` | `P184` | `W062_ACTUAL_TRANSLATIONAL_CONTROL_TRACKING_ERROR_OUT` | OUTPUT | `ANALOG_FEEDBACK` | `ISOLATED_CONTROL_SIGNAL` |
| `W062` | `L85_ProximityOperationsGuidanceComputer` | `P127` | `W062_ACTUAL_TRANSLATIONAL_CONTROL_TRACKING_ERROR_IN` | INPUT | `ANALOG_FEEDBACK` | `ISOLATED_CONTROL_SIGNAL` |
| `W063` | `L85_RelativePositionVelocityEstimator` | `P162` | `W063_MEASURED_RANGE_RATE_AND_STOPPING_DISTANCE_OUT` | OUTPUT | `ANALOG_FEEDBACK` | `ISOLATED_CONTROL_SIGNAL` |
| `W063` | `L85_ProximityOperationsGuidanceComputer` | `P128` | `W063_MEASURED_RANGE_RATE_AND_STOPPING_DISTANCE_IN` | INPUT | `ANALOG_FEEDBACK` | `ISOLATED_CONTROL_SIGNAL` |
| `W064` | `L85_PrecisionFlightTimeReferenceClock` | `P101` | `W064_TRAJECTORY_PROPAGATION_TIME_REFERENCE_OUT` | OUTPUT | `DIGITAL_STATUS` | `ISOLATED_CONTROL_SIGNAL` |
| `W064` | `L85_RelativeTrajectoryPlanningComputer` | `P172` | `W064_TRAJECTORY_PROPAGATION_TIME_REFERENCE_IN` | INPUT | `DIGITAL_STATUS` | `ISOLATED_CONTROL_SIGNAL` |
| `W065` | `L85_FuelReserveAndAbortPredictor` | `P060` | `W065_PROPELLANT_AND_ABORT_MARGIN_CONSTRAINT_OUT` | OUTPUT | `ANALOG_FEEDBACK` | `ISOLATED_CONTROL_SIGNAL` |
| `W065` | `L85_RelativeTrajectoryPlanningComputer` | `P170` | `W065_PROPELLANT_AND_ABORT_MARGIN_CONSTRAINT_IN` | INPUT | `ANALOG_FEEDBACK` | `ISOLATED_CONTROL_SIGNAL` |
| `W066` | `L85_RelativeTrajectoryPlanningComputer` | `P169` | `W066_FORECASTED_RENDEZVOUS_IMPULSE_BUDGET_OUT` | OUTPUT | `DATA_STREAM` | `ISOLATED_CONTROL_SIGNAL` |
| `W066` | `L85_FuelReserveAndAbortPredictor` | `P064` | `W066_FORECASTED_RENDEZVOUS_IMPULSE_BUDGET_IN` | INPUT | `DATA_STREAM` | `ISOLATED_CONTROL_SIGNAL` |
| `W067` | `L85_DockingCorridorSafetyGuardian` | `P018` | `W067_APPROACH_HOLD_POINT_PERMISSION_AND_MODE_OUT` | OUTPUT | `SAFETY_CONTROL` | `ISOLATED_CONTROL_SIGNAL` |
| `W067` | `L85_ProximityOperationsGuidanceComputer` | `P130` | `W067_APPROACH_HOLD_POINT_PERMISSION_AND_MODE_IN` | INPUT | `SAFETY_CONTROL` | `ISOLATED_CONTROL_SIGNAL` |
| `W068` | `L85_CollisionAvoidancePredictionMonitor` | `P006` | `W068_PREDICTED_COLLISION_FREE_MANEUVER_CERTIFICATE_OUT` | OUTPUT | `SAFETY_CONTROL` | `ISOLATED_CONTROL_SIGNAL` |
| `W068` | `L85_ProximityOperationsGuidanceComputer` | `P131` | `W068_PREDICTED_COLLISION_FREE_MANEUVER_CERTIFICATE_IN` | INPUT | `SAFETY_CONTROL` | `ISOLATED_CONTROL_SIGNAL` |
| `W069` | `L85_ProximityOperationsGuidanceComputer` | `P126` | `W069_GUIDANCE_PROTECTIVE_ABORT_REQUEST_OUT` | OUTPUT | `SAFETY_CONTROL` | `ISOLATED_CONTROL_SIGNAL` |
| `W069` | `L85_ProtectedAutonomousAbortController` | `P120` | `W069_GUIDANCE_PROTECTIVE_ABORT_REQUEST_IN` | INPUT | `SAFETY_CONTROL` | `ISOLATED_CONTROL_SIGNAL` |
| `W070` | `L85_ProtectedAutonomousAbortController` | `P116` | `W070_PROTECTED_AUTONOMOUS_RETREAT_GUIDANCE_OUT` | OUTPUT | `ACTUATOR_COMMAND` | `ISOLATED_CONTROL_SIGNAL` |
| `W070` | `L85_SixDegreeFreedomFlightController` | `P188` | `W070_PROTECTED_AUTONOMOUS_RETREAT_GUIDANCE_IN` | INPUT | `ACTUATOR_COMMAND` | `ISOLATED_CONTROL_SIGNAL` |
| `W071` | `L85_SixDegreeFreedomFlightController` | `P186` | `W071_PROPULSION_ABILITY_AND_ATTITUDE_CONTROL_FEEDB_OUT` | OUTPUT | `DIGITAL_STATUS` | `ISOLATED_CONTROL_SIGNAL` |
| `W071` | `L85_ProtectedAutonomousAbortController` | `P119` | `W071_PROPULSION_ABILITY_AND_ATTITUDE_CONTROL_FEEDB_IN` | INPUT | `DIGITAL_STATUS` | `ISOLATED_CONTROL_SIGNAL` |
| `W072` | `L85_ProximityOperationsGuidanceComputer` | `P123` | `W072_PREDICTED_ORBITAL_APPROACH_OVERLAY_OUT` | OUTPUT | `DATA_STREAM` | `ISOLATED_CONTROL_SIGNAL` |
| `W072` | `L85_RelativeOrbitTrajectoryHologram` | `P161` | `W072_PREDICTED_ORBITAL_APPROACH_OVERLAY_IN` | INPUT | `DATA_STREAM` | `ISOLATED_CONTROL_SIGNAL` |
| `W073` | `L85_RelativeNavigationFusionComputer` | `P150` | `W073_ESTIMATED_POSITIONAL_UNCERTAINTY_DISTRIBUTION_OUT` | OUTPUT | `DATA_STREAM` | `ISOLATED_CONTROL_SIGNAL` |
| `W073` | `L85_NavigationUncertaintyVolumeDisplay` | `P083` | `W073_ESTIMATED_POSITIONAL_UNCERTAINTY_DISTRIBUTION_IN` | INPUT | `DATA_STREAM` | `ISOLATED_CONTROL_SIGNAL` |
| `W074` | `L85_ProximityOperationsGuidanceComputer` | `P125` | `W074_GUIDANCE_CONTROLLER_STATE_AND_TRACKING_TREND_OUT` | OUTPUT | `DIGITAL_STATUS` | `ISOLATED_CONTROL_SIGNAL` |
| `W074` | `L85_HolographicRendezvousSCADA` | `P073` | `W074_GUIDANCE_CONTROLLER_STATE_AND_TRACKING_TREND_IN` | INPUT | `DIGITAL_STATUS` | `ISOLATED_CONTROL_SIGNAL` |
| `W075` | `L85_RelativeTrajectoryPlanningComputer` | `P168` | `W075_TRAJECTORY_DELTA_V_FORECAST_TELEMETRY_OUT` | OUTPUT | `DATA_STREAM` | `ISOLATED_CONTROL_SIGNAL` |
| `W075` | `L85_HolographicRendezvousSCADA` | `P070` | `W075_TRAJECTORY_DELTA_V_FORECAST_TELEMETRY_IN` | INPUT | `DATA_STREAM` | `ISOLATED_CONTROL_SIGNAL` |
| `W076` | `L85_FuelReserveAndAbortPredictor` | `P061` | `W076_FUEL_RESERVE_AND_RETREAT_ENVELOPE_OUT` | OUTPUT | `ANALOG_FEEDBACK` | `ISOLATED_CONTROL_SIGNAL` |
| `W076` | `L85_HolographicRendezvousSCADA` | `P068` | `W076_FUEL_RESERVE_AND_RETREAT_ENVELOPE_IN` | INPUT | `ANALOG_FEEDBACK` | `ISOLATED_CONTROL_SIGNAL` |
| `W077` | `L85_ProtectedAutonomousAbortController` | `P117` | `W077_ABORT_CONTROLLER_ACTUAL_MODE_STATUS_OUT` | OUTPUT | `DIGITAL_STATUS` | `ISOLATED_CONTROL_SIGNAL` |
| `W077` | `L85_HolographicRendezvousSCADA` | `P072` | `W077_ABORT_CONTROLLER_ACTUAL_MODE_STATUS_IN` | INPUT | `DIGITAL_STATUS` | `ISOLATED_CONTROL_SIGNAL` |
| `W078` | `L85_SixDegreeFreedomFlightController` | `P183` | `W078_REAL_TIME_MICROTHRUSTER_BODY_WRENCH_UPDATE_OUT` | OUTPUT | `ACTUATOR_COMMAND` | `ISOLATED_CONTROL_SIGNAL` |
| `W078` | `L85_ThrusterForceAllocationComputer` | `P212` | `W078_REAL_TIME_MICROTHRUSTER_BODY_WRENCH_UPDATE_IN` | INPUT | `ACTUATOR_COMMAND` | `ISOLATED_CONTROL_SIGNAL` |
| `W079` | `L85_ShortRangeRendezvousRadar` | `P177` | `W079_SHORT_RANGE_APPROACHING_OBJECT_RADIAL_DISTANC_OUT` | OUTPUT | `ANALOG_FEEDBACK` | `ISOLATED_CONTROL_SIGNAL` |
| `W079` | `L85_DockingCorridorSafetyGuardian` | `P020` | `W079_SHORT_RANGE_APPROACHING_OBJECT_RADIAL_DISTANC_IN` | INPUT | `ANALOG_FEEDBACK` | `ISOLATED_CONTROL_SIGNAL` |
| `W080` | `L85_RelativeNavigationFusionComputer` | `P148` | `W080_NAVIGATION_ESTIMATION_CONFIDENCE_FOR_APPROACH_OUT` | OUTPUT | `ANALOG_FEEDBACK` | `ISOLATED_CONTROL_SIGNAL` |
| `W080` | `L85_DockingCorridorSafetyGuardian` | `P021` | `W080_NAVIGATION_ESTIMATION_CONFIDENCE_FOR_APPROACH_IN` | INPUT | `ANALOG_FEEDBACK` | `ISOLATED_CONTROL_SIGNAL` |
| `W081` | `L85_RelativePositionVelocityEstimator` | `P163` | `W081_RELATIVE_TARGET_APPROACH_SPEED_ESTIMATE_OUT` | OUTPUT | `ANALOG_FEEDBACK` | `ISOLATED_CONTROL_SIGNAL` |
| `W081` | `L85_DockingCorridorSafetyGuardian` | `P020` | `W081_RELATIVE_TARGET_APPROACH_SPEED_ESTIMATE_IN` | INPUT | `ANALOG_FEEDBACK` | `ISOLATED_CONTROL_SIGNAL` |
| `W082` | `L85_CollisionAvoidancePredictionMonitor` | `P007` | `W082_PREDICTIVE_KEEP_OUT_ZONE_INTERSECTION_STATUS_OUT` | OUTPUT | `SAFETY_CONTROL` | `ISOLATED_CONTROL_SIGNAL` |
| `W082` | `L85_DockingCorridorSafetyGuardian` | `P024` | `W082_PREDICTIVE_KEEP_OUT_ZONE_INTERSECTION_STATUS_IN` | INPUT | `SAFETY_CONTROL` | `ISOLATED_CONTROL_SIGNAL` |
| `W083` | `L85_DockingCorridorSafetyGuardian` | `P019` | `W083_COLLISION_BOUNDARY_CLEARANCE_AUTHORIZATION_OUT` | OUTPUT | `SAFETY_CONTROL` | `ISOLATED_CONTROL_SIGNAL` |
| `W083` | `L85_ProximityOperationsGuidanceComputer` | `P130` | `W083_COLLISION_BOUNDARY_CLEARANCE_AUTHORIZATION_IN` | INPUT | `SAFETY_CONTROL` | `ISOLATED_CONTROL_SIGNAL` |
| `W084` | `L85_PrecisionDockingCameraMonitor` | `P090` | `W084_LIVE_FINAL_DOCKING_CAMERA_STATUS_OUT` | OUTPUT | `DIGITAL_STATUS` | `ISOLATED_CONTROL_SIGNAL` |
| `W084` | `L85_DockingCorridorSafetyGuardian` | `P022` | `W084_LIVE_FINAL_DOCKING_CAMERA_STATUS_IN` | INPUT | `DIGITAL_STATUS` | `ISOLATED_CONTROL_SIGNAL` |
| `W085` | `L85_OpticalTargetPatternTracker` | `P086` | `W085_OPTICAL_DOCKING_TARGET_VIDEO_ALIGNMENT_OUT` | OUTPUT | `DATA_STREAM` | `ISOLATED_CONTROL_SIGNAL` |
| `W085` | `L85_PrecisionDockingCameraMonitor` | `P092` | `W085_OPTICAL_DOCKING_TARGET_VIDEO_ALIGNMENT_IN` | INPUT | `DATA_STREAM` | `ISOLATED_CONTROL_SIGNAL` |
| `W086` | `L85_StationOpticalTargetMarkerArray` | `P197` | `W086_STATION_TARGET_FIDUCIAL_COORDINATES_OUT` | OUTPUT | `DATA_STREAM` | `ISOLATED_CONTROL_SIGNAL` |
| `W086` | `L85_OpticalTargetPatternTracker` | `P088` | `W086_STATION_TARGET_FIDUCIAL_COORDINATES_IN` | INPUT | `DATA_STREAM` | `ISOLATED_CONTROL_SIGNAL` |
| `W087` | `L85_StationApproachBeaconCluster` | `P195` | `W087_DOCK_BEACON_CODED_ACQUISITION_SIGNAL_OUT` | OUTPUT | `DATA_STREAM` | `ISOLATED_CONTROL_SIGNAL` |
| `W087` | `L85_DockingBeaconReceiver` | `P015` | `W087_DOCK_BEACON_CODED_ACQUISITION_SIGNAL_IN` | INPUT | `DATA_STREAM` | `ISOLATED_CONTROL_SIGNAL` |
| `W088` | `L85_ExternalInspectionCameraGantry` | `P059` | `W088_INSPECTION_GANTRY_MONITORING_POSE_OUT` | OUTPUT | `DATA_STREAM` | `ISOLATED_CONTROL_SIGNAL` |
| `W088` | `L85_PrecisionDockingCameraMonitor` | `P093` | `W088_INSPECTION_GANTRY_MONITORING_POSE_IN` | INPUT | `DATA_STREAM` | `ISOLATED_CONTROL_SIGNAL` |
| `W089` | `L85_StationApproachIlluminationRig` | `P196` | `W089_DOCKING_APPROACH_LIGHTING_STATUS_OUT` | OUTPUT | `DIGITAL_STATUS` | `ISOLATED_CONTROL_SIGNAL` |
| `W089` | `L85_PrecisionDockingCameraMonitor` | `P094` | `W089_DOCKING_APPROACH_LIGHTING_STATUS_IN` | INPUT | `DIGITAL_STATUS` | `ISOLATED_CONTROL_SIGNAL` |
| `W090` | `L85_ProximityOperationsGuidanceComputer` | `P124` | `W090_COMMANDED_APPROACH_CORRIDOR_OVERLAY_OUT` | OUTPUT | `DATA_STREAM` | `ISOLATED_CONTROL_SIGNAL` |
| `W090` | `L85_ThreeDimensionalApproachCorridorHologram` | `P203` | `W090_COMMANDED_APPROACH_CORRIDOR_OVERLAY_IN` | INPUT | `DATA_STREAM` | `ISOLATED_CONTROL_SIGNAL` |
| `W091` | `L85_DockingCorridorSafetyGuardian` | `P016` | `W091_ACTUAL_PROXIMITY_WARNING_BOUNDARY_VISUAL_OUT` | OUTPUT | `DATA_STREAM` | `ISOLATED_CONTROL_SIGNAL` |
| `W091` | `L85_ThreeDimensionalApproachCorridorHologram` | `P204` | `W091_ACTUAL_PROXIMITY_WARNING_BOUNDARY_VISUAL_IN` | INPUT | `DATA_STREAM` | `ISOLATED_CONTROL_SIGNAL` |
| `W092` | `L85_DockingCorridorSafetyGuardian` | `P017` | `W092_APPROACH_HOLD_POINT_READINESS_REPORT_OUT` | OUTPUT | `DIGITAL_STATUS` | `ISOLATED_CONTROL_SIGNAL` |
| `W092` | `L85_HolographicRendezvousSCADA` | `P073` | `W092_APPROACH_HOLD_POINT_READINESS_REPORT_IN` | INPUT | `DIGITAL_STATUS` | `ISOLATED_CONTROL_SIGNAL` |
| `W093` | `L85_CollisionAvoidancePredictionMonitor` | `P005` | `W093_COLLISION_AVOIDANCE_PREDICTION_TIMELINE_OUT` | OUTPUT | `DATA_STREAM` | `ISOLATED_CONTROL_SIGNAL` |
| `W093` | `L85_HolographicRendezvousSCADA` | `P071` | `W093_COLLISION_AVOIDANCE_PREDICTION_TIMELINE_IN` | INPUT | `DATA_STREAM` | `ISOLATED_CONTROL_SIGNAL` |
| `W094` | `L85_DockingCorridorSafetyGuardian` | `P018` | `W094_EMERGENCY_HOLD_RETREAT_ARBITRATION_REQUEST_OUT` | OUTPUT | `SAFETY_CONTROL` | `ISOLATED_CONTROL_SIGNAL` |
| `W094` | `L85_ProtectedAutonomousAbortController` | `P121` | `W094_EMERGENCY_HOLD_RETREAT_ARBITRATION_REQUEST_IN` | INPUT | `SAFETY_CONTROL` | `ISOLATED_CONTROL_SIGNAL` |
| `W095` | `L85_ProtectedAutonomousAbortController` | `P118` | `W095_MEASURED_RETREAT_MANEUVER_EXECUTED_STATUS_OUT` | OUTPUT | `DIGITAL_STATUS` | `ISOLATED_CONTROL_SIGNAL` |
| `W095` | `L85_DockingCorridorSafetyGuardian` | `P023` | `W095_MEASURED_RETREAT_MANEUVER_EXECUTED_STATUS_IN` | INPUT | `DIGITAL_STATUS` | `ISOLATED_CONTROL_SIGNAL` |
| `W096` | `L85_DockingCorridorSafetyGuardian` | `P019` | `W096_FINAL_DOCKING_CONTACT_AUTHORIZATION_OUT` | OUTPUT | `SAFETY_CONTROL` | `ISOLATED_CONTROL_SIGNAL` |
| `W096` | `L85_DockingMechanismSequencingPLC` | `P036` | `W096_FINAL_DOCKING_CONTACT_AUTHORIZATION_IN` | INPUT | `SAFETY_CONTROL` | `ISOLATED_CONTROL_SIGNAL` |
| `W097` | `L85_DockingMechanismSequencingPLC` | `P027` | `W097_ACTIVE_SOFT_CAPTURE_RING_ACTUATION_REQUEST_OUT` | OUTPUT | `ACTUATOR_COMMAND` | `ISOLATED_CONTROL_SIGNAL` |
| `W097` | `L85_ActiveSoftCaptureRing` | `P002` | `W097_ACTIVE_SOFT_CAPTURE_RING_ACTUATION_REQUEST_IN` | INPUT | `ACTUATOR_COMMAND` | `ISOLATED_CONTROL_SIGNAL` |
| `W098` | `L85_ActiveSoftCaptureRing` | `P001` | `W098_MEASURED_ACTIVE_RING_EXTENSION_STATUS_OUT` | OUTPUT | `ANALOG_FEEDBACK` | `ISOLATED_CONTROL_SIGNAL` |
| `W098` | `L85_DockingMechanismSequencingPLC` | `P032` | `W098_MEASURED_ACTIVE_RING_EXTENSION_STATUS_IN` | INPUT | `ANALOG_FEEDBACK` | `ISOLATED_CONTROL_SIGNAL` |
| `W099` | `L85_DockingMechanismSequencingPLC` | `P028` | `W099_GUIDE_PETAL_POSITIONING_REQUEST_OUT` | OUTPUT | `ACTUATOR_COMMAND` | `ISOLATED_CONTROL_SIGNAL` |
| `W099` | `L85_MotorizedCaptureGuidePetals` | `P080` | `W099_GUIDE_PETAL_POSITIONING_REQUEST_IN` | INPUT | `ACTUATOR_COMMAND` | `ISOLATED_CONTROL_SIGNAL` |
| `W100` | `L85_MotorizedCaptureGuidePetals` | `P079` | `W100_MEASURED_GUIDE_PETAL_ALIGNMENT_FEEDBACK_OUT` | OUTPUT | `ANALOG_FEEDBACK` | `ISOLATED_CONTROL_SIGNAL` |
| `W100` | `L85_DockingMechanismSequencingPLC` | `P033` | `W100_MEASURED_GUIDE_PETAL_ALIGNMENT_FEEDBACK_IN` | INPUT | `ANALOG_FEEDBACK` | `ISOLATED_CONTROL_SIGNAL` |
| `W101` | `L85_DockingMechanismSequencingPLC` | `P027` | `W101_SIX_INDEPENDENT_HARD_LATCH_ACTUATION_PERMITS_OUT` | OUTPUT | `ACTUATOR_COMMAND` | `ISOLATED_CONTROL_SIGNAL` |
| `W101` | `L85_SixActuatorDockingLatchArray` | `P180` | `W101_SIX_INDEPENDENT_HARD_LATCH_ACTUATION_PERMITS_IN` | INPUT | `ACTUATOR_COMMAND` | `ISOLATED_CONTROL_SIGNAL` |
| `W102` | `L85_SixActuatorDockingLatchArray` | `P179` | `W102_SIX_HARD_LATCH_ACTUAL_LOCK_FEEDBACK_CHANNELS_OUT` | OUTPUT | `ANALOG_FEEDBACK` | `ISOLATED_CONTROL_SIGNAL` |
| `W102` | `L85_DockingMechanismSequencingPLC` | `P032` | `W102_SIX_HARD_LATCH_ACTUAL_LOCK_FEEDBACK_CHANNELS_IN` | INPUT | `ANALOG_FEEDBACK` | `ISOLATED_CONTROL_SIGNAL` |
| `W103` | `L85_SixAxisContactForceSensor` | `P181` | `W103_SIX_AXIS_CAPTURED_CONTACT_FORCE_FEEDBACK_OUT` | OUTPUT | `ANALOG_FEEDBACK` | `ISOLATED_CONTROL_SIGNAL` |
| `W103` | `L85_DockingMechanismSequencingPLC` | `P033` | `W103_SIX_AXIS_CAPTURED_CONTACT_FORCE_FEEDBACK_IN` | INPUT | `ANALOG_FEEDBACK` | `ISOLATED_CONTROL_SIGNAL` |
| `W104` | `L85_DockingMechanismSequencingPLC` | `P028` | `W104_CONTACT_DAMPING_REQUEST_UNDER_SAFE_LOADS_OUT` | OUTPUT | `ACTUATOR_COMMAND` | `ISOLATED_CONTROL_SIGNAL` |
| `W104` | `L85_CompliantContactDampingActuator` | `P009` | `W104_CONTACT_DAMPING_REQUEST_UNDER_SAFE_LOADS_IN` | INPUT | `ACTUATOR_COMMAND` | `ISOLATED_CONTROL_SIGNAL` |
| `W105` | `L85_CompliantContactDampingActuator` | `P008` | `W105_ACTUAL_COMPLIANT_DAMPER_DISPLACEMENT_OUT` | OUTPUT | `ANALOG_FEEDBACK` | `ISOLATED_CONTROL_SIGNAL` |
| `W105` | `L85_DockingMechanismSequencingPLC` | `P032` | `W105_ACTUAL_COMPLIANT_DAMPER_DISPLACEMENT_IN` | INPUT | `ANALOG_FEEDBACK` | `ISOLATED_CONTROL_SIGNAL` |
| `W106` | `L85_DockingMechanismSequencingPLC` | `P027` | `W106_QUALIFIED_DOCKING_UMBILICAL_MATE_COMMAND_OUT` | OUTPUT | `ACTUATOR_COMMAND` | `ISOLATED_CONTROL_SIGNAL` |
| `W106` | `L85_DockingElectricalUmbilicalCoupler` | `P026` | `W106_QUALIFIED_DOCKING_UMBILICAL_MATE_COMMAND_IN` | INPUT | `ACTUATOR_COMMAND` | `ISOLATED_CONTROL_SIGNAL` |
| `W107` | `L85_DockingElectricalUmbilicalCoupler` | `P025` | `W107_ACTUAL_UMBILICAL_CONTINUITY_VERIFICATION_OUT` | OUTPUT | `ANALOG_FEEDBACK` | `ISOLATED_CONTROL_SIGNAL` |
| `W107` | `L85_DockingMechanismSequencingPLC` | `P033` | `W107_ACTUAL_UMBILICAL_CONTINUITY_VERIFICATION_IN` | INPUT | `ANALOG_FEEDBACK` | `ISOLATED_CONTROL_SIGNAL` |
| `W108` | `L85_DockingMechanismSequencingPLC` | `P028` | `W108_PRESSURE_GASKET_COMPRESSION_AUTHORIZATION_OUT` | OUTPUT | `ACTUATOR_COMMAND` | `ISOLATED_CONTROL_SIGNAL` |
| `W108` | `L85_PressureInterfaceSealGasket` | `P104` | `W108_PRESSURE_GASKET_COMPRESSION_AUTHORIZATION_IN` | INPUT | `ACTUATOR_COMMAND` | `ISOLATED_CONTROL_SIGNAL` |
| `W109` | `L85_PressureInterfaceSealGasket` | `P103` | `W109_MEASURED_GASKET_COMPRESSION_AND_SEAL_HEALTH_OUT` | OUTPUT | `ANALOG_FEEDBACK` | `ISOLATED_CONTROL_SIGNAL` |
| `W109` | `L85_PressureLeakQualificationController` | `P107` | `W109_MEASURED_GASKET_COMPRESSION_AND_SEAL_HEALTH_IN` | INPUT | `ANALOG_FEEDBACK` | `ISOLATED_CONTROL_SIGNAL` |
| `W110` | `L85_PressureLeakQualificationController` | `P105` | `W110_COMPLETED_FICTIONAL_PRESSURE_LEAK_TEST_RESULT_OUT` | OUTPUT | `DIGITAL_STATUS` | `ISOLATED_CONTROL_SIGNAL` |
| `W110` | `L85_DockingMechanismSequencingPLC` | `P034` | `W110_COMPLETED_FICTIONAL_PRESSURE_LEAK_TEST_RESULT_IN` | INPUT | `DIGITAL_STATUS` | `ISOLATED_CONTROL_SIGNAL` |
| `W111` | `L85_HardDockStructuralLoadMonitor` | `P065` | `W111_DOCKED_STRUCTURE_LOAD_DISTRIBUTION_MONITOR_OUT` | OUTPUT | `ANALOG_FEEDBACK` | `ISOLATED_CONTROL_SIGNAL` |
| `W111` | `L85_DockingMechanismSequencingPLC` | `P032` | `W111_DOCKED_STRUCTURE_LOAD_DISTRIBUTION_MONITOR_IN` | INPUT | `ANALOG_FEEDBACK` | `ISOLATED_CONTROL_SIGNAL` |
| `W112` | `L85_DockingMechanismSequencingPLC` | `P030` | `W112_DOCKING_MECHANISM_STEP_BY_STEP_CAMERA_STATUS_OUT` | OUTPUT | `DIGITAL_STATUS` | `ISOLATED_CONTROL_SIGNAL` |
| `W112` | `L85_PrecisionDockingCameraMonitor` | `P095` | `W112_DOCKING_MECHANISM_STEP_BY_STEP_CAMERA_STATUS_IN` | INPUT | `DIGITAL_STATUS` | `ISOLATED_CONTROL_SIGNAL` |
| `W113` | `L85_AsymmetricLatchFaultEmulator` | `P004` | `W113_ASYMMETRIC_MECHANICAL_LATCH_FAULT_INJECTION_OUT` | OUTPUT | `SAFETY_CONTROL` | `ISOLATED_CONTROL_SIGNAL` |
| `W113` | `L85_DockingMechanismSequencingPLC` | `P037` | `W113_ASYMMETRIC_MECHANICAL_LATCH_FAULT_INJECTION_IN` | INPUT | `SAFETY_CONTROL` | `ISOLATED_CONTROL_SIGNAL` |
| `W114` | `L85_DockingMechanismSequencingPLC` | `P031` | `W114_SOFT_CAPTURE_TO_HARD_DOCKING_STATE_EVENTS_OUT` | OUTPUT | `DIGITAL_STATUS` | `ISOLATED_CONTROL_SIGNAL` |
| `W114` | `L85_HolographicRendezvousSCADA` | `P072` | `W114_SOFT_CAPTURE_TO_HARD_DOCKING_STATE_EVENTS_IN` | INPUT | `DIGITAL_STATUS` | `ISOLATED_CONTROL_SIGNAL` |
| `W115` | `L85_Protected5VControlSupply` | `P114` | `W115_INDEPENDENT_DOCKING_SAFETY_PLC_PROTECTED_CONT_OUT` | OUTPUT | `POWER_CONTROL` | `CONTROL_5V` |
| `W115` | `L85_DockingSafetyPLC` | `P051` | `W115_INDEPENDENT_DOCKING_SAFETY_PLC_PROTECTED_CONT_IN` | INPUT | `POWER_CONTROL` | `CONTROL_5V` |
| `W116` | `L85_EmergencyApproachHoldStation` | `P058` | `W116_MANUAL_EMERGENCY_APPROACH_HOLD_PHYSICAL_CONTA_OUT` | OUTPUT | `SAFETY_CONTROL` | `ISOLATED_CONTROL_SIGNAL` |
| `W116` | `L85_DockingSafetyPLC` | `P052` | `W116_MANUAL_EMERGENCY_APPROACH_HOLD_PHYSICAL_CONTA_IN` | INPUT | `SAFETY_CONTROL` | `ISOLATED_CONTROL_SIGNAL` |
| `W117` | `L85_DockingSafetyPLC` | `P043` | `W117_INDEPENDENT_THRUSTER_INHIBIT_AND_AUTHORIZATIO_OUT` | OUTPUT | `SAFETY_CONTROL` | `ISOLATED_CONTROL_SIGNAL` |
| `W117` | `L85_SixDegreeFreedomFlightController` | `P191` | `W117_INDEPENDENT_THRUSTER_INHIBIT_AND_AUTHORIZATIO_IN` | INPUT | `SAFETY_CONTROL` | `ISOLATED_CONTROL_SIGNAL` |
| `W118` | `L85_DockingSafetyPLC` | `P044` | `W118_INDEPENDENT_HARD_CAPTURE_INHIBIT_OUTPUT_OUT` | OUTPUT | `SAFETY_CONTROL` | `ISOLATED_CONTROL_SIGNAL` |
| `W118` | `L85_DockingMechanismSequencingPLC` | `P036` | `W118_INDEPENDENT_HARD_CAPTURE_INHIBIT_OUTPUT_IN` | INPUT | `SAFETY_CONTROL` | `ISOLATED_CONTROL_SIGNAL` |
| `W119` | `L85_FuelReserveAndAbortPredictor` | `P060` | `W119_MINIMUM_PHYSICAL_FUEL_RESERVE_FOR_SAFE_ABORT_OUT` | OUTPUT | `ANALOG_FEEDBACK` | `ISOLATED_CONTROL_SIGNAL` |
| `W119` | `L85_DockingSafetyPLC` | `P045` | `W119_MINIMUM_PHYSICAL_FUEL_RESERVE_FOR_SAFE_ABORT_IN` | INPUT | `ANALOG_FEEDBACK` | `ISOLATED_CONTROL_SIGNAL` |
| `W120` | `L85_CollisionAvoidancePredictionMonitor` | `P006` | `W120_INDEPENDENT_COLLISION_PREDICTION_STOP_REQUEST_OUT` | OUTPUT | `SAFETY_CONTROL` | `ISOLATED_CONTROL_SIGNAL` |
| `W120` | `L85_DockingSafetyPLC` | `P053` | `W120_INDEPENDENT_COLLISION_PREDICTION_STOP_REQUEST_IN` | INPUT | `SAFETY_CONTROL` | `ISOLATED_CONTROL_SIGNAL` |
| `W121` | `L85_CriticalClosingRateFeedbackStation` | `P010` | `W121_CLOSING_RATE_MONITOR_CALIBRATION_HEALTH_DIAGN_OUT` | OUTPUT | `DIGITAL_STATUS` | `ISOLATED_CONTROL_SIGNAL` |
| `W121` | `L85_DockingSafetyPLC` | `P048` | `W121_CLOSING_RATE_MONITOR_CALIBRATION_HEALTH_DIAGN_IN` | INPUT | `DIGITAL_STATUS` | `ISOLATED_CONTROL_SIGNAL` |
| `W122` | `L85_RelativeNavigationFusionComputer` | `P149` | `W122_NAVIGATION_FUSION_RAW_RANGE_RATE_CALIBRATION__OUT` | OUTPUT | `ANALOG_FEEDBACK` | `ISOLATED_CONTROL_SIGNAL` |
| `W122` | `L85_CriticalClosingRateFeedbackStation` | `P011` | `W122_NAVIGATION_FUSION_RAW_RANGE_RATE_CALIBRATION__IN` | INPUT | `ANALOG_FEEDBACK` | `ISOLATED_CONTROL_SIGNAL` |
| `W123` | `L85_RelativeNavigationFusionComputer` | `P148` | `TRUSTED_RANGE_RATE_OUT` | OUTPUT | `ANALOG_FEEDBACK` | `ISOLATED_CONTROL_SIGNAL` |
| `W123` | `L85_DockingSafetyPLC` | `P046` | `TRUSTED_RANGE_RATE_IN` | INPUT | `ANALOG_FEEDBACK` | `ISOLATED_CONTROL_SIGNAL` |
| `W124` | `L85_RelativePositionVelocityEstimator` | `P162` | `W124_DISPLAY_ONLY_RELATIVE_SPEED_TELEMETRY_NOT_W12_OUT` | OUTPUT | `ANALOG_FEEDBACK` | `ISOLATED_CONTROL_SIGNAL` |
| `W124` | `L85_HolographicRendezvousSCADA` | `P069` | `W124_DISPLAY_ONLY_RELATIVE_SPEED_TELEMETRY_NOT_W12_IN` | INPUT | `ANALOG_FEEDBACK` | `ISOLATED_CONTROL_SIGNAL` |
| `W125` | `L85_DockingSafetyPLC` | `P043` | `W125_SAFETY_AUTHORIZED_ESCAPE_RETREAT_ENABLE_OUT` | OUTPUT | `SAFETY_CONTROL` | `ISOLATED_CONTROL_SIGNAL` |
| `W125` | `L85_ProtectedAutonomousAbortController` | `P120` | `W125_SAFETY_AUTHORIZED_ESCAPE_RETREAT_ENABLE_IN` | INPUT | `SAFETY_CONTROL` | `ISOLATED_CONTROL_SIGNAL` |
| `W126` | `L85_ProtectedAutonomousAbortController` | `P117` | `W126_MEASURED_SAFE_ABORT_COMPLETION_FEEDBACK_OUT` | OUTPUT | `DIGITAL_STATUS` | `ISOLATED_CONTROL_SIGNAL` |
| `W126` | `L85_DockingSafetyPLC` | `P049` | `W126_MEASURED_SAFE_ABORT_COMPLETION_FEEDBACK_IN` | INPUT | `DIGITAL_STATUS` | `ISOLATED_CONTROL_SIGNAL` |
| `W127` | `L85_DockingSafetyPLC` | `P044` | `W127_CRITICAL_W123_FEEDBACK_STATE_AND_FAULT_LATCH_OUT` | OUTPUT | `SAFETY_CONTROL` | `ISOLATED_CONTROL_SIGNAL` |
| `W127` | `L85_CriticalClosingRateFeedbackStation` | `P012` | `W127_CRITICAL_W123_FEEDBACK_STATE_AND_FAULT_LATCH_IN` | INPUT | `SAFETY_CONTROL` | `ISOLATED_CONTROL_SIGNAL` |
| `W128` | `L85_ElectricalTerminalDiagnosticRack` | `P054` | `W128_OPERATOR_EXPLICIT_SAFETY_RESET_REQUEST_OUT` | OUTPUT | `DIGITAL_CONTROL` | `ISOLATED_CONTROL_SIGNAL` |
| `W128` | `L85_DockingSafetyPLC` | `P047` | `W128_OPERATOR_EXPLICIT_SAFETY_RESET_REQUEST_IN` | INPUT | `DIGITAL_CONTROL` | `ISOLATED_CONTROL_SIGNAL` |
| `W129` | `L85_DockingSafetyPLC` | `P042` | `W129_CRITICAL_SAFETY_STATUS_AND_EVENT_TELEMETRY_OUT` | OUTPUT | `DIGITAL_STATUS` | `ISOLATED_CONTROL_SIGNAL` |
| `W129` | `L85_HolographicRendezvousSCADA` | `P073` | `W129_CRITICAL_SAFETY_STATUS_AND_EVENT_TELEMETRY_IN` | INPUT | `DIGITAL_STATUS` | `ISOLATED_CONTROL_SIGNAL` |
| `W130` | `L85_DockingSafetyPLC` | `P043` | `W130_PROTECTED_DOCKING_MISSION_RELEASE_AUTHORIZATI_OUT` | OUTPUT | `SAFETY_CONTROL` | `ISOLATED_CONTROL_SIGNAL` |
| `W130` | `L85_DockingMissionCertificationBeacon` | `P040` | `W130_PROTECTED_DOCKING_MISSION_RELEASE_AUTHORIZATI_IN` | INPUT | `SAFETY_CONTROL` | `ISOLATED_CONTROL_SIGNAL` |
| `W131` | `L85_RelativeNavigationFusionComputer` | `P151` | `W131_FUSED_POSITION_AND_VELOCITY_STATE_SCADA_OUT` | OUTPUT | `DATA_STREAM` | `ISOLATED_CONTROL_SIGNAL` |
| `W131` | `L85_HolographicRendezvousSCADA` | `P070` | `W131_FUSED_POSITION_AND_VELOCITY_STATE_SCADA_IN` | INPUT | `DATA_STREAM` | `ISOLATED_CONTROL_SIGNAL` |
| `W132` | `L85_RelativePositionVelocityEstimator` | `P163` | `W132_MEASURED_TARGET_CLOSING_RATE_DISPLAY_SUMMARY_OUT` | OUTPUT | `ANALOG_FEEDBACK` | `ISOLATED_CONTROL_SIGNAL` |
| `W132` | `L85_HolographicRendezvousSCADA` | `P068` | `W132_MEASURED_TARGET_CLOSING_RATE_DISPLAY_SUMMARY_IN` | INPUT | `ANALOG_FEEDBACK` | `ISOLATED_CONTROL_SIGNAL` |
| `W133` | `L85_SixDegreeFreedomFlightController` | `P185` | `W133_SIX_DEGREE_TRANSLATION_ACTUAL_CONTROL_TELEMET_OUT` | OUTPUT | `DATA_STREAM` | `ISOLATED_CONTROL_SIGNAL` |
| `W133` | `L85_HolographicRendezvousSCADA` | `P071` | `W133_SIX_DEGREE_TRANSLATION_ACTUAL_CONTROL_TELEMET_IN` | INPUT | `DATA_STREAM` | `ISOLATED_CONTROL_SIGNAL` |
| `W134` | `L85_ThrusterValveDriverRack` | `P219` | `W134_THRUSTER_VALVE_COMMANDED_VERSUS_ACTUAL_STATUS_OUT` | OUTPUT | `DIGITAL_STATUS` | `ISOLATED_CONTROL_SIGNAL` |
| `W134` | `L85_HolographicRendezvousSCADA` | `P072` | `W134_THRUSTER_VALVE_COMMANDED_VERSUS_ACTUAL_STATUS_IN` | INPUT | `DIGITAL_STATUS` | `ISOLATED_CONTROL_SIGNAL` |
| `W135` | `L85_ThrusterFlowAndPressureMonitor` | `P206` | `W135_PROPELLANT_FLOW_AND_PULSE_HISTORY_TREND_OUT` | OUTPUT | `ANALOG_FEEDBACK` | `ISOLATED_CONTROL_SIGNAL` |
| `W135` | `L85_HolographicRendezvousSCADA` | `P069` | `W135_PROPELLANT_FLOW_AND_PULSE_HISTORY_TREND_IN` | INPUT | `ANALOG_FEEDBACK` | `ISOLATED_CONTROL_SIGNAL` |
| `W136` | `L85_LiDARBlindSpotFaultEmulator` | `P076` | `W136_LIDAR_DROPOUT_EVENT_DIAGNOSTIC_FINGERPRINT_OUT` | OUTPUT | `DIGITAL_STATUS` | `ISOLATED_CONTROL_SIGNAL` |
| `W136` | `L85_ElectricalTerminalDiagnosticRack` | `P055` | `W136_LIDAR_DROPOUT_EVENT_DIAGNOSTIC_FINGERPRINT_IN` | INPUT | `DIGITAL_STATUS` | `ISOLATED_CONTROL_SIGNAL` |
| `W137` | `L85_StuckThrusterValveFaultEmulator` | `P200` | `W137_STUCK_VALVE_FAULT_AND_ISOLATION_REPORT_OUT` | OUTPUT | `DIGITAL_STATUS` | `ISOLATED_CONTROL_SIGNAL` |
| `W137` | `L85_ElectricalTerminalDiagnosticRack` | `P056` | `W137_STUCK_VALVE_FAULT_AND_ISOLATION_REPORT_IN` | INPUT | `DIGITAL_STATUS` | `ISOLATED_CONTROL_SIGNAL` |
| `W138` | `L85_AsymmetricLatchFaultEmulator` | `P003` | `W138_ASYMMETRIC_LATCH_ABNORMALITY_DIAGNOSIS_OUT` | OUTPUT | `DIGITAL_STATUS` | `ISOLATED_CONTROL_SIGNAL` |
| `W138` | `L85_ElectricalTerminalDiagnosticRack` | `P055` | `W138_ASYMMETRIC_LATCH_ABNORMALITY_DIAGNOSIS_IN` | INPUT | `DIGITAL_STATUS` | `ISOLATED_CONTROL_SIGNAL` |
| `W139` | `L85_PressureLeakQualificationController` | `P106` | `W139_PRESSURE_SEAL_POST_DOCKING_INTEGRITY_REPORT_OUT` | OUTPUT | `DIGITAL_STATUS` | `ISOLATED_CONTROL_SIGNAL` |
| `W139` | `L85_HolographicRendezvousSCADA` | `P073` | `W139_PRESSURE_SEAL_POST_DOCKING_INTEGRITY_REPORT_IN` | INPUT | `DIGITAL_STATUS` | `ISOLATED_CONTROL_SIGNAL` |
| `W140` | `L85_HardDockStructuralLoadMonitor` | `P066` | `W140_CAPTURED_STRUCTURAL_LOAD_SAFETY_REPORT_OUT` | OUTPUT | `ANALOG_FEEDBACK` | `ISOLATED_CONTROL_SIGNAL` |
| `W140` | `L85_HolographicRendezvousSCADA` | `P068` | `W140_CAPTURED_STRUCTURAL_LOAD_SAFETY_REPORT_IN` | INPUT | `ANALOG_FEEDBACK` | `ISOLATED_CONTROL_SIGNAL` |
| `W141` | `L85_PrecisionDockingCameraMonitor` | `P091` | `W141_SYNCHRONIZED_APPROACH_CAMERA_FEED_HEALTH_OUT` | OUTPUT | `DIGITAL_STATUS` | `ISOLATED_CONTROL_SIGNAL` |
| `W141` | `L85_HolographicRendezvousSCADA` | `P072` | `W141_SYNCHRONIZED_APPROACH_CAMERA_FEED_HEALTH_IN` | INPUT | `DIGITAL_STATUS` | `ISOLATED_CONTROL_SIGNAL` |
| `W142` | `L85_DockingMechanismSequencingPLC` | `P029` | `W142_DOCKING_STATE_TRANSITIONS_FLIGHT_RECORDER_OUT` | OUTPUT | `DATA_STREAM` | `ISOLATED_CONTROL_SIGNAL` |
| `W142` | `L85_PrecisionFlightEventDataRecorder` | `P098` | `W142_DOCKING_STATE_TRANSITIONS_FLIGHT_RECORDER_IN` | INPUT | `DATA_STREAM` | `ISOLATED_CONTROL_SIGNAL` |
| `W143` | `L85_DockingSafetyPLC` | `P041` | `W143_LATCHED_SAFETY_ALARM_FLIGHT_RECORDER_OUT` | OUTPUT | `DATA_STREAM` | `ISOLATED_CONTROL_SIGNAL` |
| `W143` | `L85_PrecisionFlightEventDataRecorder` | `P099` | `W143_LATCHED_SAFETY_ALARM_FLIGHT_RECORDER_IN` | INPUT | `DATA_STREAM` | `ISOLATED_CONTROL_SIGNAL` |
| `W144` | `L85_PrecisionFlightEventDataRecorder` | `P097` | `W144_SIGNED_FRESH_MISSION_RUN_EVENT_LOG_OUT` | OUTPUT | `DATA_STREAM` | `ISOLATED_CONTROL_SIGNAL` |
| `W144` | `L85_HolographicRendezvousSCADA` | `P070` | `W144_SIGNED_FRESH_MISSION_RUN_EVENT_LOG_IN` | INPUT | `DATA_STREAM` | `ISOLATED_CONTROL_SIGNAL` |
| `W145` | `L85_HolographicRendezvousSCADA` | `P067` | `W145_SIX_OBJECTIVE_AUTHORIZED_CERTIFICATION_DISPLA_OUT` | OUTPUT | `DIGITAL_STATUS` | `ISOLATED_CONTROL_SIGNAL` |
| `W145` | `L85_DockingMissionCertificationBeacon` | `P039` | `W145_SIX_OBJECTIVE_AUTHORIZED_CERTIFICATION_DISPLA_IN` | INPUT | `DIGITAL_STATUS` | `ISOLATED_CONTROL_SIGNAL` |
| `W146` | `L85_DockingMissionCertificationBeacon` | `P038` | `W146_DOCKING_MISSION_6_OF_6_OBJECTIVE_CERTIFICATE_OUT` | OUTPUT | `DIGITAL_STATUS` | `ISOLATED_CONTROL_SIGNAL` |
| `W146` | `L85_HolographicRendezvousSCADA` | `P073` | `W146_DOCKING_MISSION_6_OF_6_OBJECTIVE_CERTIFICATE_IN` | INPUT | `DIGITAL_STATUS` | `ISOLATED_CONTROL_SIGNAL` |

All 292 records are `PLANNED_NOT_ACTUAL_SPARKTERMINAL` and carry `NeedsSeparateFunctionalPort=YES`. Implement reconciled functional contacts and document deliberate shared buses explicitly. Preserve the art P IDs and the exact W IDs.


## ANNEX F — ALL SIX ORIGINAL SOURCE OBJECTIVE RECORDS

| # | Exact source objective title | Required source evidence | Design authority |
|---:|---|---|---|
| 1 | **Construct the Rendezvous Control Network** | 146 genuine player-created electrical connections and reconciled functional SparkTerminals, real 5 V control and independent safety | `LIVE_ELECTRICAL_GRAPH_AND_DEVICE_HEALTH` |
| 2 | **Calibrate the Autonomous Navigation System** | Calibrated stereo/LiDAR/radar/IMU, fresh relative navigation data, estimated position velocity, finite thruster calibration | `REAL_SENSORS_AND_ACTUATOR_CALIBRATION` |
| 3 | **Execute Precision Orbital Rendezvous** | Relative-motion physics, commanded and actual position/velocity, safe keep-out zones, hold points and fuel margin | `RELATIVE_STATE_AND_MISSION_RUN_EVIDENCE` |
| 4 | **Complete Soft Capture and Hard Docking** | Measured contact speed and pose, physically verified soft capture and six locks, mate status and pressure seal validation | `MECHANICAL_CAPTURE_AND_SEAL_EVIDENCE` |
| 5 | **Survive Three Docking-System Failures** | LiDAR dropout, stuck propulsion valve, asymmetric latch diagnosed/changed and repaired; fresh full docking requalified | `THREE_REAL_FAULT_REPAIRS_AND_FRESH_MISSION` |
| 6 | **Restore Critical Closing-Rate Feedback** | Deliberate actual W123 disconnection, stale critical measurement, safe inhibit/abort or hold, fault latch; reconnect and fresh sampling, explicit reset, new full docking | `W123_RECOVERY_AND_NEW_RUN_QUALIFICATION` |

All six original rows are `DESIGN_ONLY_NOT_IMPLEMENTED`. The implementer must create actual runtime evaluation and get authoritative `LevelGamePlayManager` 6/6.


## ANNEX G — SOURCE VALIDATION SNAPSHOT AND RECONCILIATION SAFEGUARDS

- Model manifest: `level_85`, `The Last Meter`, 78 modular FBX + 78 GLB, 200,132 triangles, 1,017 mesh parts.
- Geometry verification: `GEOMETRY_AND_SOURCE_PLANNING_STRUCTURALLY_VERIFIED`; 60 unique actual pivot names, 224 modeled sockets, 146 planned wires, 292 planned logical endpoints.
- `Terminal_Logical_Port_Reconciliation_292.csv`: **223** distinct `(model, visual-anchor)` pairs, **41** reused pairs. Preserve independent logical signal meaning even on same-type contacts.
- Important exact collision of visual locations: **W080** `L85_RelativeNavigationFusionComputer/P148` → `L85_DockingCorridorSafetyGuardian/P021` is navigation estimation confidence, while **W123** uses the **same P148 source visual socket** to send *trusted closing rate* to `L85_DockingSafetyPLC/P046`. The two analog values must not accidentally become one electrical node.
- Geometry layout: `Y-up / X station approach / Z depth`; all modular assets origin-aligned. Stage bounds roughly **29.2 × 10.128 × 15.7 m**.
- FBX numeric units centimetres; GLB numeric units metres. Convert FBX **once** in Unity.
- `actual_player_wires_included=false`, `functional_sparkterminals_included=false`, `actual_spacecraft_orbit_solver_included=false`, `six_runtime_objectives_implemented=false`, `unity_compilation_tested=false`, `unity_playmode_tested=false`.
- Model set is genuine procedural editable mesh geometry, but final production UVs, hand-authored AAA finish, runtime collisions/LOD/VFX, compiled electrical simulation and actual Play Mode are unverified or absent. Do not misrepresent the art package as the final Unity game.


---

**FINAL IMPLEMENTATION STANDARD: 146 REAL PLAYER WIRES • 292 SAFELY RECONCILED LOGICAL PORTS AT 224 ART MARKERS • 60 REAL PIVOTS • ACTUAL MOMENTUM/THRUSTER/FUEL/ESTIMATOR/DOCKING PHYSICS • THREE MUTATING FAULTS • W123 TRUST AND INDEPENDENT SAFETY • SIX EVIDENCE-BASED OBJECTIVES • NO REGRESSION TO LEVELS 1–84 • ONLY CLAIM 6/6 AFTER UNITY TESTING.**

