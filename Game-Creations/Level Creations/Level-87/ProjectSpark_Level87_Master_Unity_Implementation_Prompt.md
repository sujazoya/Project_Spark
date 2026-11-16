# PROJECT SPARK — LEVEL 87 MASTER UNITY IMPLEMENTATION PROMPT

## LEVEL 87 — THE GRAVITY ENGINE

**Advanced AAA Artificial Gravity • Orbital Habitat Centrifuge • Eight-Station Active Magnetic Bearings • Rim Motor Drives • Rotor Dynamics & Angular Momentum • Resonance and Balancing • Contactless Rotary Power/Data • Independent Safety • Critical W137 Trusted Radial Gap Feedback Recovery**

> **SOURCE-GROUNDED IMPLEMENTATION SPECIFICATION:** This document was assembled from the actual supplied `ProjectSpark_Level87_HD_AAA_Models.zip` and its extracted `Model_Manifest.json`, verification data, 86 model records, 68 animation pivots, W001–W158 wiring CSV, 236 art socket records, 316 logical endpoint declarations, 96 reused-anchor records, source objective plans and shipped Unity art helpers. Tables in the annexes preserve source IDs and fields. **Everything described as physics, gameplay, solver integration, safety logic, production-quality finishing, Unity compilation, or objective completion is an implementation requirement, NOT something already functioning in the art kit.** The ZIP is the existing art source of truth; the project source code is the API source of truth.

# 01. PRIMARY MANDATE

Act as my **senior Unity C# gameplay engineer, Project Spark circuit/terminal integration architect, rotating machinery and rotor dynamics engineer, magnetic bearing/levitation controls specialist, industrial power electronics and inverter engineer, vibration and resonance analyst, adaptive balancing-controls engineer, spacecraft momentum-management engineer, spacecraft power and telemetry engineer, sensor calibration/metrology engineer, independent safety/PLC engineer, physical educational simulator designer, AAA technical artist, and Unity Editor automation engineer**.

**IMPLEMENT A COMPLETE, PLAYABLE LEVEL 87 — THE GRAVITY ENGINE — IN MY EXISTING PROJECT SPARK UNITY PROJECT.** Do not redesign the existing art. Import the supplied **86 numbered modular FBX models**; preserve exact names/actual pivots; make **158 actual player-created wires** into working circuit topology; reconcile the **236 decorative socket locations against 316 separate proposed functional endpoints**; implement a dynamic active-bearing centrifuge and rotating habitat energy/momentum systems; simulate three causal faults and repairs; implement **the actual detachable W137** trust/safety sequence; and award six evidence-based objectives through the real `LevelGamePlayManager`.

**Success means genuine measured 6/6 after a fresh post-W137 levitation → controlled spin-up → sustained gravity ring qualification.** An animation, countdown, SetComplete call made from a debug button, prefabricated 158 cables, or static UI text does not constitute success. Preserve Levels 1–86 and normal Project Spark shop/shelf/wiring/save behaviors.

# 02. READ THESE SOURCE FILES FIRST

Open the supplied Level 87 ZIP and inspect these exact contents before coding:

- `Model_Manifest.json`, `Verification_Report.json`, `Art_Assembly.csv`, `Component_Roles.csv`, `README_Unity.txt`, `Checksums_SHA256.txt`.
- `Animation_Rigs.csv`: **68** actual named mesh-owning `ANIM_*` transform parents; real axes, kinds and coordinates.
- `Gameplay_Wiring_Concept_W001_W158.csv` and `Gameplay_Wiring_Budget.csv`: **158 design-only** WIDs, owners, art anchor IDs, purposes, signal classes, critical flags.
- `Terminal_Anchor_Plan_236.csv` and `Visual_Terminal_Sockets_236.csv`: **236 visual SOCKET_P### nodes**, NOT game terminals.
- `Terminal_Logical_Port_Reconciliation_316.csv`: **316** separate proposed logical contacts, one endpoint per source/target WID.
- `Terminal_Anchor_Signal_Conflict_Report.csv`: **96** art locations reused by more than one contact; **69** with different signal kinds. These require careful electrical separation.
- `Objective_Certification_Plan_6.csv`, `W137_Critical_Radial_Feedback_Recovery.md`, `Gameplay_Integration_Plan.md`.
- `Texture_Manifest.json`, `Material_Palette.json`, `AAA_Art_Delivery_Notes.md`, `AAA_Polish_And_Lighting_Plan.md`, `Collision_LOD_Production_Plan.csv`.
- `Unity_Optional/SparkLevel87ArtPlacementEditor.cs`, `SparkLevel87MaterialUpgradeEditor.cs`, `SparkLevel87AssetValidatorEditor.cs`, `SparkLevel87VisualRig.cs`.

The **appendices at the end reproduce the manifest-derived exact model filenames, pivot axes/locations, all W001–W158 conceptual routes, all P001–P236 visual sockets, all 316 logical endpoint mappings, all 96 conflict records and the original six objective acceptance rows**. Use original package CSV/JSON for the builder; do not silently replace their facts with freeform text from this prompt.

# 03. VERIFIED ART BASELINE AND LIMITATIONS

The source manifest reports **86 modular binary FBX 7400 files**, **86 matching GLBs**, **one alternative assembled showcase FBX and GLB**, **184,948 modular triangles**, **1,208 mesh parts**, **68 named animation pivot groups**, **236 visual socket anchor nodes**, **158 conceptual wire records**, **316 proposed independent logical wire endpoints**, **96 reused anchors**, and **12 procedural material sets / 36 reference maps**. Recorded bounds: **min (−15.0, −3.255, −23.0)** to **max (+15.0, +13.835, +10.1)** metres, or **30.0 m width × 17.09 m height × 33.1 m depth**. Bounds include scenic/stage elements; do not assume all space is traversable.

The source records **visual art-ring radius 5.2 m** and a distinct **fictional gameplay physical-scenario radius 45 m**, with example target **3 RPM**. Do **NOT** treat 5.2 m as the real game physics radius; do not visually scale the entire 30 m art layout by 45/5.2. The physical centrifuge is a deliberately scaled educational representation. Preserve clear labels explaining this abstraction.

The structural `Verification_Report.json` records **24/24 local package checks**, including 86 GLBs independently loadable and FBX/GLB structure. It explicitly does NOT establish actual Unity compilation, Play Mode testing, physical magnetic-bearing physics, energy simulations, functioning SparkTerminals, connected wires, or implemented runtime objectives. `actual_player_wires_included=false`, `functional_sparkterminals_included=false`, `actual_magnetic_bearing_physics_included=false`, `actual_gravity_ring_simulator_included=false`, `six_runtime_objectives_implemented=false`, `unity_compilation_tested=false`, `unity_playmode_tested=false`. Do not describe the procedural texture references as final production UV/bake work.

# 04. INSPECT ACTUAL PROJECT SPARK APIS

Before any integration code, inspect real namespaces, concrete signatures, serialized fields, events and data flows for `SparkElectricalSolver`, `SparkCircuitSystem`, `SparkElectricalComponent`, `SparkTerminal`, `SparkSwitch`, `SparkLevelDefinition`, `SparkLevelEvaluator`, `SparkLevelSceneBindings`, `SparkLevelWorkstationBindingBridge`, `SparkShelfSlot`, `SparkObjectShelf`, `SparkLevelLayoutLoader`, `SparkLevel`, and `LevelGamePlayManager` **where they exist**. Trace how real player wires are added and removed, how powered terminals and returns qualify, how objectives are registered and persisted, how shop/shelf components are purchased and placed, and how prior level prefabs/builders are authored.

Reuse suitable real code from previous implemented Levels 1–86 when verified. A prior **prompt** does not prove its proposed Unity class was ever created. Never write a method call based solely on assumption. Implement small adapters for missing events/status APIs; do not fork a second electrical solver or overwrite global game infrastructure.

# 05. ABSOLUTE NON-NEGOTIABLE RULES

1. Preserve all Levels 1–86, common scenes, project-wide systems and existing purchased inventory behavior.
2. No preconnected puzzle wires; player action must establish and remove the solver graph edges.
3. Real `SparkTerminal` nodes, correct signal classes, direction/voltage/return semantics and connection capacity—not merely gold art sockets.
4. Keep **5 V educational control circuits**, **fictional higher-power rotor/bearing/rotary plant**, and **independent protective safety** distinctly modeled. A 5 V signal cable cannot directly transmit megawatt plant energy.
5. One authoritative simulator owns actual rotor energy, RPM, angular phase, bearing gaps, current, thermal state, vibration, power and faults.
6. Requested drive speed, coil current, balance-mass position, isolator state and touchdown engagement **must not instantly equal** measured physical state.
7. Conserve energy: real rotor inertia/kinetic energy, finite drive source power, converter losses, motor torque, thermal losses, station counter-momentum and finite emergency braking capacity.
8. Magnetic levitation has finite force, control bandwidth, current/temperature limits, sensor noise/latency; backup touchdown bearings have finite engagement and cannot teleport a spinning ring to rest.
9. A gravity-ring timer cannot certify an all-loads-off machine. Habitat essential-load duty, power transfer and instrument validity must remain real mission gates.
10. A sensor declaring healthy via SCADA, W136 patch status or W135 backup diagnostic must never substitute for missing **trusted W137** in normal bearing authorization.
11. W137 loss invalidates fresh/cached trusted feedback and enters a **physically plausible** safe response; it must not zero rotor angular velocity or magnetic gap by fiat.
12. Independent overspeed/E-stop/touchdown protection must remain functional if the ordinary bearing controller fails.
13. Historical Objectives 1–5 persist through the final supervised fault; Objective 6 uses a new post-repair run and cannot complete on reconnect/reset alone.
14. Visual pivots, indicator emissions, SCADA numbers and decorative cables are **presentation consumers**, not physical or certification truth.
15. No real centrifuge/mechanical/electrical safety certification is asserted: these are educational game-model thresholds.
16. Report actual tests honestly: unsupported Unity compilation, missing source code, lacking mesh UV/LOD, and incomplete Play Mode work must remain UNTESTED/OPEN.

# 06. LEVEL CONFIGURATION AND OPENING STATE

Create level ID **`level_87`**, title **The Gravity Engine**, subtitle **Artificial Gravity, Magnetic Bearings and Rotation Control**, expert/master difficulty; **6 genuine objectives**. Build a fictional Earth-orbit magnetic-bearing gravity-ring qualification lab. Initial state: visual ring present but stationary or safely supported, touchdown mechanics available, most control circuits disconnected, inverters isolated, coils not commissioned, radial sensors uncalibrated, balance masses at known home, rotary power transfer offline, habitat mission loads staged and E-stop operational, all objectives **0/6**.

Set game-scale tunable parameters separately from display scene dimensions. Example radius **45 m**, demonstration speed **3 RPM**, which implies approximately **0.453 g** at the rim; not a real target for crew habitability. Use conservative adjustable educational limits for vibration, gap margin, coil current, thermal limits, acceleration, power-transfer efficiency, essential-load duty and mission interval. Label them as Level 87 scenario defaults.

# 07. FBX IMPORT, AXES, COORDINATES AND IDENTITY

The source FBX numeric mesh vertices and named pivot data are in **centimetres**, with source `UnitScaleFactor=1`; GLB geometry uses **metres**. In Unity convert centimetres → metres **once** and inspect actual imported renderer bounds. Y is up, X across, Z forward. Models share the common world-origin layout: instantiate all 86 numbered FBXs under `L87_HD_VISUAL_ART` with identity local transforms in the authored assembly frame. Do not additionally offset each module to an origin or rescale the mesh a second time.

The alternative assembled showcase `Models/Level87_Complete_ArtKit_Showcase.fbx` and `GLB/Level87_Complete_ArtKit_Showcase.glb` are **reference previews only**. Never instantiate showcase alongside the 86 modular pieces, or the entire environment will double. Keep gameplay roots, functional terminal children, colliders, solver objects, objectives, saved user placement, simulation authority and VFX controllers **outside any disposable visual-art root**. Rebuild must be undoable, idempotent, and safe against users' customizations.

# 08. ACTUAL SHIPPED UNITY ART TOOLS — DO NOT CONFUSE WITH GAMEPLAY BUILDER

The ZIP includes editor helpers in namespace `ProjectSpark.EditorTools`:

- `SparkLevel87ArtPlacementEditor`: `Tools/Project Spark/Levels/87/Place HD Art Under Selected Layout`.
- `SparkLevel87MaterialUpgradeEditor`: `Tools/Project Spark/Levels/87/Create & Apply HD Materials To Selected Art`.
- `SparkLevel87AssetValidatorEditor`: `Tools/Project Spark/Levels/87/Validate Imported HD Art`.
- Runtime `ProjectSpark.Visuals.SparkLevel87VisualRig` is a presentation component, NOT a rotor solver.

Place `*Editor.cs` under a Unity `Editor` directory; keep `SparkLevel87VisualRig.cs` in a runtime assembly. The above menus work only at the ART stage. **You must implement a separate gameplay editor command** at `Tools/Project Spark/Levels/Create Level 87` that constructs terminals, wire definitions, simulation, objectives, scene layout and tests.

# 09. AAA PBR FINISH, GPU OPTIMIZATION AND COLLISIONS

Use the source **12 material sets / 36 PNG maps** as reference textures: albedo, normal and packed `MetalRoughAO` where R=metallic, G=roughness, B=ambient occlusion. Repack for the actual URP/HDRP/Standard shader; convert roughness to smoothness where needed; use actual UV unwraps and bake/author production normals/AO before describing AAA-final quality. Treat metallic copper coils, ceramic rotor shell, brushed titanium, carbon composites, emissive field visuals, warning marks, structural fasteners, glass and instrument panels distinctly.

Plan collider simplification for the central workstation and moving ring, no raw high-density mesh colliders on spinning subassemblies. Use static batching on stationary structure; instancing/repeated mesh reuse and LODs on habitat sections/magnet coils. Avoid double-rendered showcases, expensive per-mesh Update scripts and fake material-state gameplay. For a ring rotating around visual +Z, configure dynamic bounds/culling correctly. Use the supplied `Collision_LOD_Production_Plan.csv` as guidance, not completed collision meshes.

# 10. VERIFIED SPARKLEVEL87VISUALRIG API

The shipped class is `ProjectSpark.Visuals.SparkLevel87VisualRig`. Verified public methods, **exact signatures**:

```csharp
public void BindVisuals();
public void SetActualRingDynamics(float actualAngleDegrees,
    float actualCountermomentAngleDegrees,
    float measuredRadialGapQuality01,
    float measuredVibrationSeverity01);
public void SetActualBearingMechanisms(float measuredWeightA01,
    float measuredWeightB01,
    float actualTouchdownBearingEngagement01);
public void SetActualSafety(bool measuredLevitationQualified,
    bool measuredDriveQualified,
    bool measuredW137FreshAndValid,
    bool actualCriticalFaultLatched);
```

The visual script looks up named `ANIM_*` children in its `artRoot`, preserves base local rotations/positions, turns the ring, spokes, six segments, pressure shell and rotor magnets about **local +Z**, turns the counter-torque flywheel about local X, translates balancing weights and emergency touchdown support, and updates three optional lamps. Values marked `01` are clamped to [0,1]; `actualAngleDegrees` is a measured integrated angle, not RPM. `SetActualSafety` takes bool measurements. A named pivot's existence does **not** mean this shipped visual script animates all 68 pivots. Extend carefully for new behaviors; do not invent setter methods.

Only the authoritative game update should call the setters. Use visual interpolation as a view, and never read localEulerAngles, light status, particle emission or a UI RPM field as actual physical truth. Rebind after art-root replacement; validate duplicate pivot IDs.

# 11. 68 SOURCE-AUTHORED PIVOTS, RING ROTATION HIERARCHY

Read all 68 records from `Animation_Rigs.csv` and Annex B. Most rotating habitat pieces have **the identical world pivot `(0,7.25,−7.2)`** and `Axis=z`, even when physically stored as separate FBXs. Drive them from the **same integrated rotor angle** to prevent visual drift and clipping; if a grouped parent makes sense, preserve world poses, art model ownership and correct local centers. Do not rotate the stationary observation viewport, consoles, fixed truss or support bearings with the rotor.

`Kind=indicator` pivots indicate display/effects and should not receive arbitrary physical rotations. Some `Kind=translation` pivots represent samples, sliders, contactors, weights and safety mechanisms. Preserve source authored X/Y/Z axes and home offsets. Special pivots include `ANIM_GRAVITY_RING_MAIN_ROTATION`, `ANIM_GRAVITY_RING_SPOKES`, `ANIM_HABITAT_SEGMENT_A..F`, `ANIM_HABITAT_PRESSURE_SHELL`, `ANIM_ROTOR_MAGNET_ARRAY`, `ANIM_COUNTERTORQUE_FLYWHEEL`, `ANIM_BALANCE_WEIGHT_A/B`, `ANIM_EMERGENCY_TOUCHDOWN_RACE`, `ANIM_MOTOR_PRECHARGE_CONTACTOR`, `ANIM_ROTARY_POWER_CONTACTOR`, `ANIM_PRIMARY_RADIAL_SENSOR`, `ANIM_SECONDARY_RADIAL_SENSOR`, `ANIM_GRAVITY_MASTER_SCADA`, and `ANIM_FINAL_CERTIFICATION`.

# 12. TWO PHYSICS DOMAINS, ONE REAL CIRCUIT GRAPH, INDEPENDENT SAFETY

**A. Real player control graph:** 158 player-installed Spark wire edges and compatible functional nodes, backed by actual solver connectivity and state. Use genuine educational regulated 5 V control, returns, logical/sampled analog and digital signal transmission.

**B. Fictional station power/rotor plant:** drive converters/inverters, eight active magnetic bearings, ring motion and kinetic energy, structural modes, rotating power couplers and essential simulated habitat demand. This cannot be realistically powered by tiny 5 V puzzle wires. Plant devices have explicit couplings to player-wired permits, commands and feedback.

**C. Independent protective domain:** hardwired safety permissions, E-stop, overspeed, touchdown support and isolation logic. Model protection as separate trusted state and actuators; don't let the SCADA controller override a latched fault.

Separate **true plant state**, **requested commands**, **sensor samples**, **qualified validity**, **historical mission evidence**, and **visual scene states** to prevent contradictions.

# 13. EXACT W001–W158 WIRE BUDGET

Use the actual `Gameplay_Wiring_Concept_W001_W158.csv`; preserve its WIDs and exact source/target owners, P anchors, purpose, signal-kind text and Critical flags. Group counts from `Gameplay_Wiring_Budget.csv`:

| IDs | Source group | Count |
|---|---|---:|
| W001–W012 | Protected educational 5 V and returns verification | 12 |
| W013–W038 | Eight active magnetic bearings and levitation sensing | 26 |
| W039–W060 | Rim motor inverters and angular speed regulation | 22 |
| W061–W080 | Rotor vibration, resonance and dynamic balance | 20 |
| W081–W102 | Noncontact rotating power and data transfer | 22 |
| W103–W122 | Thermal management and simulated habitat mission loads | 20 |
| W123–W142 | Independent safety, W137 and three disturbances | 20 |
| W143–W158 | SCADA, diagnostics and six-objective certification | 16 |
| **TOTAL** | | **158** |

Every existing source row status is `CONCEPT_ONLY_NOT_PLAYER_WIRE`: the kit contains no real runtime player wires. Build each wire as a **required detachable edge declaration**, not preconnected cable instances. Keep unique stable IDs, correct endpoints, source/return qualification, persisted topology and runtime connect/disconnect events. A wire PASS is only an actual solver-recognized edge with compatible terminals and powered devices. Never award Objective 1 simply because 158 decorative splines are drawn.

# 14. ART SOCKETS ARE NOT ELECTRICAL CONTACTS — REQUIRED 236→316 RECONCILIATION

`Terminal_Anchor_Plan_236.csv` specifies **P001–P236** as source-authored `SOCKET_P###` art locations. Each is a visible connector marker, not an installed `SparkTerminal`. The **316 endpoint occurrences** require 316 separately identified source/target contact identities (`W###_OUT_...` and `W###_IN_...`). The conflict report contains **96 reused art anchors**, of which **69** are used by multiple `SignalKind` categories. Examples include mixed power/analog/data/safety classes on supervisory PLC art anchors. These cannot all be one circuit node.

Implement a deterministic reconciler that maps `{WireID, Endpoint, SourceModel/TargetModel, ArtAnchor, FunctionalPortID, Direction, SignalKind, domain}`. Determine legal multi-contact bus compatibility. If one anchor is used by incompatible signals, split it into multiple visual subcontacts or an explicit labeled multipin terminal block with separate `SparkTerminal` children, keeping the original P### art intact. Respect the existing terminal API's pin count and multiple connections; don't invent power returns. Where the W001–W012 plan does not sufficiently define electrical return conductors, make an **explicit audited return/junction design** compatible with the source budget and real solver, and report changes rather than assuming chassis art carries return current.

Validate topology programmatically: wrong-direction edges, analog-to-power shorts, cross-domain shorts, multiple drivers, duplicated ports, intentionally shared actual buses, orphaned returns, incompatible references and false source/target IDs. Export a `Level87_TerminalReconciliationReport.csv`. Preserve all 236 original anchor IDs and all 158 required WIDs even where more physical contact instances are necessary.

# 15. TRUSTED W137 — EXACT VERIFIED SOURCE ROW

The **only** `Critical=YES` wire in the actual source CSV is:

- `WireID`: **`W137`**.
- **Source model**: `L87_PrimaryRadialGapSensor`.
- **Source decorative socket**: **`P019`**.
- **Destination model**: `L87_MagneticBearingSafetyPLC`.
- **Destination decorative socket**: **`P135`**.
- `SignalKind`: **`ANALOG_FEEDBACK`**.
- `SignalPurpose`: **`CRITICAL trusted radial displacement feedback W137`**.
- Source planned functional port: `W137_OUT_ANALOG_FEEDBACK`; destination: `W137_IN_ANALOG_FEEDBACK` in the reconciliation CSV. The recovery notes also label their *semantic* roles `TRUSTED_RADIAL_GAP_OUT` / `TRUSTED_RADIAL_GAP_IN`.
- Original status: `CONCEPT_ONLY_NOT_PLAYER_WIRE`.

Make W137 a real physically disconnectable **player wire** in the circuit graph. Validate both topology and powered sampling/correct return to permit trusted feedback. The sample carries an actual modeled radial displacement (calibration, timestamp, range, validity), not the SCADA visual meter setting. An invalid W137 signal is **unknown/untrusted**—never substitute 0 mm error or an old last-good sample. Do not confuse the decorative P019/P135 nodes with functional electrical nodes.

# 16. W137 ADJACENT SAFETY WIRES AND NO-BYPASS TRUST BOUNDARY

Important source routes around W137:

- **W123** hardwired E-stop station → supervisory PLC.
- **W125** independent overspeed protector → supervisory PLC.
- **W126** speed encoder → independent overspeed protector.
- **W130** bearing safety PLC → bearing control PLC safe-mode command.
- **W134** bearing safety PLC → emergency touchdown bearing.
- **W135** secondary radial gap sensor → bearing safety PLC: **redundant diagnostic/cross-check**, not normal W137 trusted replacement.
- **W136** W137 patch station → bearing safety PLC: **continuity diagnostic only**, not actual displacement measurement.
- **W138** bearing safety PLC → supervisory PLC: critical fault latch status.
- **W140** supervisory PLC → W137 patch station: recovery status display.
- **W141** event recorder → supervisory PLC: explicit operator fault-reset request.
- **W149** bearing safety PLC → master SCADA: trusted feedback timestamp/status only.
- **W155** patch station → master SCADA: nonauthoritative diagnostic telemetry.

Do not accept W135/W136/W149/W155, a stale time series, UI switch or independent overspeed relay as a substitute **for W137 normal closed-loop qualification**. W135 may participate in an approved bounded emergency control/rundown mode as an independent measurement, but it may not silently heal W137 normal operation. W125/E-stop remain operative even during lost or corrupt W137.

# 17. AUTHORITATIVE SIMULATION STATE

Maintain one `Level87ProcessState` (or actual project-compatible equivalent) including: ring angular phase/velocity/acceleration; scenario radius=45 m; inertia and rotor kinetic energy; commanded/actual torque, speed and inverter current; DC bus/energy supply constraints; all eight bearing actual radial/axial displacements, coil currents and force limits; amplifier temperature and limit/fault state; calibrated measured primary/secondary gap signals with age; vibration modal displacement, velocity, phase, spectra and resonance regions; distributed mass offsets and balancing actuator positions; touchdown support displacement/contact/heat; station reaction torque and counter-momentum actuator state; rotating power coupler input/output energy and transfer efficiency; essential habitat power/thermal loads and deadlines; safety interlock/fault latch; W137 topology and sensor validity; disturbance activation/repair; source command and sampled-measurement timestamps; evidence event IDs, history and run IDs.

**No separate authority in visual rig, PLC, SCADA or UI**. Expose read-only sampled displays and strictly validated commands from those systems; preserve history without faking physical truth.

# 18. DETERMINISTIC FIXED-STEP UPDATE SEQUENCE

Implement one coherent fixed-step simulation (e.g. `FixedUpdate` with actual project timestep), explicitly ordered:

1. Ingest live Spark graph changes, 5 V powered controls, operator requests and independent protective inputs.
2. Validate W137, other sensor conditioning/returns, fresh sample status and current safety latches.
3. Resolve allowed mode: off, supported touchdown, levitation, spin-up, steady running, dynamic faults, rundown, reset.
4. Apply protected drive/coil power permission and finite precharge/contactor/inverter response.
5. Compute bearing commanded current to actual coil current/force with bandwidth/limit and amplifier temperature.
6. Integrate rotor radial/axial dynamics, mass eccentricity, structural vibration and touchdown contact.
7. Integrate drive torque, kinetic energy, losses, reaction torque and counter-momentum state.
8. Integrate rotating power transfer, habitat loads, source energy draw, efficiencies and heat.
9. Update actual balancing actuators and motor thermal/cooling dynamics.
10. Sample primary/secondary radial gaps, axial probes, speed encoder, vibration and temperatures with validity/calibration/time.
11. Reevaluate independent overspeed, E-stop, vibration/clearance/thermal and touchdown protections.
12. Advance scenario PLC, fault diagnosis and qualification evidence from real sampled plant state.
13. Persist event/`runId`/objective evidence and telemetry history.
14. Push read-only normalized actual values to SCADA and `SparkLevel87VisualRig`.

Use consistent seconds, radians, metres, N, N·m, amperes, watts, joules and kelvin/Celsius where displayed. Handle numerical stability and fixed-step interpolation; clamp nonphysical negative energy without silently concealing a numerical error.

# 19. GRAVITY, RING ANGULAR SPEED AND MECHANICAL ENERGY

Use the scenario radius **r=45 m**, with **angular velocity ω in rad/s**:

```text
omega_rad_s = rpm * (2*pi / 60)
a_rim_m_s2 = omega_rad_s^2 * radius_m
g_rim = a_rim_m_s2 / 9.80665
E_rotor_J = 0.5 * inertia_kg_m2 * omega_rad_s^2
angularMomentum_Nms = inertia_kg_m2 * omega_rad_s
I * d(omega)/dt = torque_drive - torque_losses - torque_load
```

At **3 RPM and 45 m**, calculated g is approximately **0.453**, not 1 g. Show artificial gravity as centrifugal acceleration measured in the rotating reference frame, not a force-generator magic switch. Inertia must be calibrated to plausible fictional gameplay scale and all units documented. Do not mistake the art radius **5.2 m** for the physical simulation radius or allow 5.2 m to generate mismatched g.

Angular momentum must be stored and transferred: station hub reaction torque, counter-torque flywheel capacity and permitted attitude disturbances should respond to motor acceleration. Do not allow an arbitrary unlimited counter-momentum sink.

# 20. ROTOR / STATION COUPLED DYNAMICS AND SAFE SPIN-UP

Represent ring angle, angular speed, inertia, drive torque, bearing/drag losses, station torque reaction and optional elastic coupling. The spin-up controller commands finite torque/current with slope limits; motor inverter must settle with response lag, not instant RPM. Run a low-speed test before crossing any modeled resonance bands; stop/hold or modify ramp profile when excessive vibration or gap error is observed. If power fails, rotor coasts/decays according to modeled losses/braking, not instant stopping.

Configure safe test modes and an explicit overspeed monitor separate from the normal speed controller. Qualify speed stability from measured actual ω over a valid time window and real load variations, not from a commanded target.

# 21. EIGHT ACTIVE MAGNETIC BEARING STATIONS

The source contains exactly `L87_MagneticBearingStationA` through `...H`, each with display/pivot, alongside sensor and amplifier equipment. Implement **eight station states** with coil current, force direction, commanded vs actual force, radial clearance, axial coupling where applicable, temperature and health. Use a bounded educational electromagnetic force relationship (locally linearized spring/damping or nonlinear gap/current model with a safe minimum-gap cutoff). Force must be proportional to available power and actual current/clearance; don't make LEDs control the rotor.

Commission all eight stations after sensor calibration, feedback confidence, independent safety and current-limit proof. Enter levitated status only if actual clearance is within qualified window and the protected touchdown bearing is not carrying normal rotor load. One defective station should create local displacement/vibration, not an instant globally green rotor.

# 22. RADIAL/AXIAL GAP SENSOR METROLOGY

Primary sensor `L87_PrimaryRadialGapSensor`, secondary `L87_SecondaryRadialGapSensor`, axial `L87_AxialGapDisplacementSensor`, gap-calibration stage and coil current monitor are actual named assets. Model sensor range, resolution, bias, scaling, noise, acquisition rate, calibration version, range overrun, powered state, timestamp, stale-sample age and separate electrical connectivity. Sample actual modeled gap **after** physics update. A calibration button must perform a bounded calibration procedure against known offsets, not erase live physical displacement.

Require W137 trust for the normal contactless-bearing safety decision, cross-check W135 diagnostic sample and other independent protections. Show disagreement flags and confidence; never treat 'bad sensor' as healthy because it reads near zero.

# 23. BEARING AMPLIFIER, COIL ELECTRICAL AND THERMAL DYNAMICS

`L87_BearingPowerAmplifierCabinet`, `L87_MagneticBearingCoilCurrentMonitor` and `L87_BearingHousingTemperatureSensor` are relevant components. Model finite amplifier voltage/current, lag and I²R/cooling heat behavior. A thermal fault reduces permissible coil drive as the amplifier warms, making actual gap/vibration worse. A requested coil current can exceed allowed limit and should be clamped with diagnostic evidence; it must not create free stabilizing force. Thermal recovery requires finite cooling time or an allowed fictional maintenance action with actual system safety state; clearing an alarm alone is not repair.

# 24. TOUCHDOWN AND SAFE SUPPORT MECHANICS

`L87_EmergencyTouchdownBearing` must protect the rotor when contactless levitation is untrusted or unavailable. Model actual engagement position, finite movement and contact force, speed-dependent friction/heat, rotor clearance and optional surface degradation. The touchdown bearing does not magically arrest angular velocity or erase vibration; a physically spinning ring may touch down and coast while monitored. Make independent permission/safety actions fail safely, and block fresh normal levitation authorization until a clean reset/calibration is performed.

# 25. TWO RIM DRIVE INVERTERS AND STATOR/MAGNET SYSTEM

Use the source `L87_SegmentedRimMotorStator`, `L87_RimDriveRotorMagnetArray`, `L87_MotorInverterDriveA`, `L87_MotorInverterDriveB`, current sensor, torque estimator, speed encoder, precharge contactor and spin-up sequencer. Represent two independently limited inverters with proper actual request/response, precharge, bus readiness, efficiency and thermal load. `torque_available` must depend on current/power/temperature/speed and fault state. The segmented motor gives smooth torque only with valid converter operation; torque ripple may excite vibration at relevant harmonics.

# 26. MOTOR PRECHARGE, ROTARY ELECTRICAL TRANSFER AND POWER BALANCE

Precharge and main isolation contactors are real staged states; do not permit abrupt enabling without qualified precharge/safety. Compute motor power and losses, bearing power, rotary coupler efficiency and station load power coherently: `P_source >= P_drive_electrical + P_bearings + P_rotary_inputs + P_aux + P_losses` where physically supportable; actual rotor energy rate derives from net torque. Contactless coupler losses become heat; rotated side output cannot exceed its allowable input times η. Treat the game power plant as a separate fictional domain with clear instrumented interfaces to the 5 V control graph.

# 27. ROTATING OPTICAL DATA AND QUALITY

Source models include `L87_RotaryOpticalTelemetryLink` and `L87_RotatingDataQualityMonitor`. Model a rotating-side telemetry channel with finite bandwidth, latency, packet loss/misalignment, timestamp freshness and safety-critical data restrictions. Data transfer quality may vary with misalignment or faults, but should not act as an alternative W137 trusted safety signal. Cache/SCADA data does not satisfy fresh safety PLC measurement.

# 28. ROTATING HABITAT MISSION LOADS — NO ALL-OFF SHORTCUT

`L87_HabitatElectricalLoadEmulator`, `L87_HabitatEmergencyLightingLoad`, `L87_HabitatThermalControlLoad`, power distribution PLC, environment hologram and budget analyzer support meaningful load scheduling. Maintain P0 essential safety/lighting/compute, required thermal duty and optional training/science load tiers. Differentiate requested/actual load power, power-transfer efficiency, recovery time and task/deadline completion. A machine that spins empty with all loads off **does not pass** sustained gravity mission; must supply essential electric/thermal function and valid data through the required interval.

# 29. THERMAL SYSTEM AND HABITAT CONDITIONS

Track ring electrical load heat, inverter/bearing/coupler losses, cooling capacity and radiator/environment proxy, with bounded thermal masses and plausible finite response. Cooling cannot instantly remove energy from a hot component. The simulated habitat temperature must meet mission stability conditions at appropriate sustained load, not just during brief spin-up. Display actual current temperatures and cooling margins.

# 30. STRUCTURAL MODES, IMBALANCE AND RESONANCE

Use a reproducible low-order flexible-rotor model; store at least one or two damped modes with natural frequency, damping, excitation from rotor imbalance and motor torque ripple. Vibration depends on ω, phase, mass offset, support stiffness/damping and active control. Do not map an abstract 'vibration level' slider directly to PASS. For example an educational single mode can use `m*x'' + c*x' + k*x = F_imbalance(omega,t)` coupled to ring/bearing states. Use safe integrators and avoid unrealistic real structural qualification claims.

# 31. VIBRATION MEASUREMENT, SPECTRUM, ORBIT PLOTS

`L87_MultiAxisVibrationSensorArray`, `L87_VibrationFrequencySpectrumAnalyzer`, `L87_RotorShaftOrbitProbe`, `L87_RotorImbalanceVectorDisplay`, `L87_DynamicRotorHealthRecorder` and `L87_PrecisionGapAndVibrationSCADA` should derive amplitude/phase and frequency bins from actual simulated time-series history. Use real acquisition windows, sample rates, calibration validity and saturation flags. A frequency sweep only qualifies covered speeds/frequencies and requires valid rotor hold/measurement conditions. A mere static heatmap cannot prove a full scan.

# 32. TWO BALANCE WEIGHTS AND ADAPTIVE DAMPING

Source has `L87_PrecisionBalanceWeightActuatorA/B` with actual slide pivots and `L87_AdaptiveVibrationDampingController`. Implement finite actuator travel, velocity and position feedback; adjusting weights changes rotor mass distribution/inertia and synchronous imbalance vector. Calibrated phased measurements guide changes. Moving balance weights while high-speed/unsafe must be interlocked; require a safe servicing state where appropriate. Active damping consumes power and has finite control authority. A balancing button must not instantly suppress every vibration mode.

# 33. STATION MOMENTUM COMPENSATION

`L87_RingAngularMomentumCompensationUnit` may be represented as a limited counter-torque/flywheel mechanism. Track its angular velocity, stored momentum, torque and saturation. Ring spin-up creates reaction torque on station; counter-momentum device cannot absorb arbitrary angular momentum forever. Design fictional acceptance margins and distinguish visual counter-flywheel rotation from actual stored momentum. If the compensator saturates, speed ramp must pause or adjust; never silently ignore the station's reaction.

# 34. SAFETY PLC, INDEPENDENT PROTECTION AND MODE HIERARCHY

Implement `L87_GravitySafetySupervisoryPLC`, `L87_MagneticBearingSafetyPLC`, `L87_MagneticBearingControlPLC`, `L87_RotorMotorControlPLC`, `L87_IndependentOverspeedProtector`, `L87_RingEmergencyStopStation`, and `L87_StationSafetyIsolationRelay`. The protection PLC must have explicit states, instrument freshness, E-stop, overspeed, low clearance, excessive vibration, amplifier heat, active W137 trust and touchdown readiness. The independent overspeed path cannot be disabled simply because the normal PLC reports safe.

Actions must be physically plausible: inhibit new power, ramp down drive where possible, preserve bearing stabilization while energy permits, and engage protective touchdown when required. Loss of W137 must not cause a physically impossible zero-speed jump. Safety reset only after operator approval and independently verified prerequisites.

# 35. EXPLICIT PLC STATE MACHINE

Recommended design states: `OFF`, `CONTROL_WIRING`, `PROTECTED_5V_READY`, `E_STOP_SELF_TEST`, `SENSOR_CALIBRATION`, `BEARING_POWER_PRECHARGE`, `BEARING_FORCE_TEST`, `LEVITATION_CAPTURE`, `LEVITATED_READY`, `MOTOR_PRECHARGE`, `LOW_SPEED_TRIAL`, `RESONANCE_SCAN`, `BALANCE_ADJUSTMENT`, `CONTROLLED_SPINUP`, `TARGET_SPEED_HOLD`, `ROTARY_POWER_COMMISSIONING`, `HABITAT_LOAD_TEST`, `SUSTAINED_GRAVITY_QUALIFY`, `DISTURBANCE_DIAGNOSIS`, `PROTECTED_RUNDOWN`, `TOUCHDOWN_SUPPORT`, `CRITICAL_FAULT_LATCHED`, `W137_REPAIR_VERIFICATION`, `RESET_REQUIRED`, `POST_REPAIR_REQUALIFICATION`, `FINAL_CERTIFIED`.

Every transition must check true motion, sensor validity and physical safety. An elapsed-time trigger may advance a simulation clock but cannot certify levitation/turning if sensors, power, vibration and task evidence are invalid.

# 36. FIRST FULL GRAVITY QUALIFICATION RUN

Generate a fresh `runId` and start with measured qualified bearing levitation, safe actual precharge and legal control commands. Perform controlled low-speed operation, pass the selected resonance test, accelerate to target RPM with proper torque/momentum budget, reach steady-state gravity within configured tolerance, commission rotary power/data, serve essential loads and thermal duty for a sustained configurable run. Capture timestamps, actual ω, radial gap/window, vibration spectra, amplifier limits, power and thermal margins, essential-load completion, independent safety health and dynamic evidence; no skipped steps or 'set RPM' teleport may pass.

# 37. DISTURBANCE A — PRIMARY RADIAL GAP SENSOR DRIFT

Activate `L87_RadialSensorDriftFaultInjector` so the **measured sensor value** changes bias/drift while true rotor gap does not instantaneously move. Normal bearing controller can initially respond incorrectly, leading to real force/position changes. Compare independent W135, raw calibration reference, coil currents and shaft probe to identify error. Recovery: safe mode/hold or controlled rundown, repair/recalibrate fictional measurement chain, fresh samples and return to valid sensor agreement. Don't clear fault via a single green UI toggle; record before/after plant and measurement evidence.

# 38. DISTURBANCE B — BEARING AMPLIFIER HEAT / CURRENT LIMIT

Activate `L87_BearingAmplifierHeatFaultInjector`; increase a selected amplifier's heat/loss or reduce cooling effectiveness. Actual amplifier temperature rises with finite dynamics; current limit reduces achievable force and increases rotor displacement/vibration as operating speed rises. Player diagnoses supply/current/temperature telemetry and conducts protected cooling or module repair. Retest at operating load with all safety margins and controller response qualified. Never 'repair' by disabling over-temperature protection.

# 39. DISTURBANCE C — HABITAT MASS DISTRIBUTION IMBALANCE

Activate `L87_MassImbalanceFaultInjector` or source load-distribution simulator; shift a cargo mass vector or eccentricity. Model a genuine synchronous vibration response, changing spectrum/phase and bearing loads. Player diagnoses amplitude/phase, commands safe rundown/hold if needed, recalibrates scan and adjusts balance masses with finite travel. Requalify vibration at relevant speeds, ring bearings, station momentum and habitat mission load continuity. An aesthetic red wobble alone does not count.

# 40. THREE-DISTURBANCE RECOVERY CERTIFICATE

All three faults must be genuinely activated, causally change authoritative plant or sensor dynamics, be diagnosed using proper measurements, repaired through corresponding constrained interactions and verified by clean new evidence. Following the **last** repair, require a **new full levitation → spin-up → sustained mission cycle** and no critical fault. Record fault injection IDs, initial state, diagnostic samples, repair actions, balance/thermal results and full runId. Objective 5 cannot pass by three `Repair()` calls if mechanical results are still unhealthy.

# 41. W137 FINAL DISCONNECTION — REAL CIRCUIT TOPOLOGY

After Objectives 1–5 have historically certified, enter a supervised low-speed, independently protected fault exercise. The player physically disconnects the **real** W137 wire `P019 → P135` using the existing wire UI; actual Spark graph continuity becomes false. Mark W137 trusted radial gap sample invalid immediately, and reject previously cached values; do not copy W135 diagnostic status into W137 trust. Withdraw normal high-speed and contactless-bearing control permissions, latch a critical fault and log events. Command a bounded rundown if independent measurements, inverter/touchdown mechanisms and power remain viable, otherwise execute defensible protected fallback. Rotor continues moving and consuming/dissipating energy according to actual dynamics.

Independent overspeed, E-stop, safety isolation and touchdown protections remain active. Existing historical 1–5 objective completion must remain stored; current machine safety may be red. Never call objective 6 complete on the disconnect event.

# 42. W137 RECONNECT, FRESH CALIBRATION, EXPLICIT RESET

Require the player to reconnect the actual correct W137 functional terminals. Verify solver continuity, permitted analog node class, sensor 5 V conditioning and return, calibrated sample with **post-reconnection timestamp**, valid range/age, primary vs secondary cross-check and independent protective health. Check actual rotor speed, magnetic bearing temperatures/gaps, touchdown engagement, source/drive isolation and safe restart conditions. The safety fault latch persists until a **separate explicit operator reset** request (source W141) succeeds; failed reset must produce actionable reasons. Re-energize with finite precharge, recomission levitation and controlled ramp.

Create fresh qualification `runId` **after** fault and reset. Pre-fault objective evidence and earlier runId cannot satisfy Objective 6's new cycle. Reconnection or sensor recalibration alone must never award 6/6.

# 43. SOURCE OBJECTIVE 1 — BUILD THE GRAVITY CONTROL NETWORK

Exactly source title **Build the Gravity Control Network**. Qualify only when all 158 actual required wires are correctly installed and graph-verified; 236 art sockets reconciled into compatible functional contacts; source/returns validated; control PLCs and independent protection genuinely powered; W137 connected, fresh and trusted. Record terminal mapping version, graph topology hash, correct WID set, supply/return measurements and safety readiness. Award **1/6** only through real evaluator API.

# 44. SOURCE OBJECTIVE 2 — COMMISSION THE MAGNETIC LEVITATION SYSTEM

Exact title **Commission the Magnetic Levitation System**. Calibrate radial/axial sensors, stage amplifiers, release from protected touchdown support into true contactless levitation of the simulated rotor, and prove all eight bearing stations maintain bounded measured clearance, coil current, amplifier temperature and fresh sensor health over a qualified interval. Must be physically dynamic and safe, not 'LEVITATED' lamp state alone. Award **2/6**.

# 45. SOURCE OBJECTIVE 3 — ACCELERATE THE ARTIFICIAL GRAVITY RING

Exact title **Accelerate the Artificial Gravity Ring**. Prove inverter precharge, actual drive torque and bounded acceleration, speed-encoder validity, resonance/vibration safe passage, preserved radial clearance, independent overspeed and limited station reaction momentum; reach stable measured target RPM. Store actual kinetic energy and speed trace. Award **3/6** only on physical and safety evidence.

# 46. SOURCE OBJECTIVE 4 — MAINTAIN STABLE ARTIFICIAL GRAVITY

Exact title **Maintain Stable Artificial Gravity**. Run a fresh sustained interval at qualified g/RPM under changing habitat load distribution, with actual rotary power/data transfer, essential lighting/compute/thermal duty, bearing temperatures/gap/vibration and adequate power budget. Reject all-loads-off, invalid sensor samples, temporary unsafe bypasses and time-jump shortcuts. Store mission task and energy certificates. Award **4/6**.

# 47. SOURCE OBJECTIVE 5 — RECOVER THREE CRITICAL ROTATION FAILURES

Exact title **Recover Three Critical Rotation Failures**. Confirm real radial sensor drift, amplifier thermal limitation and mass imbalance; perform diagnosis, physically consequential repair and remeasurement for each, and then complete a **new full** levitation/spin-up/stable essential-load qualification with no unresolved fault. Historical Objectives 1–4 persist. Award **5/6** with run-scoped evidence only.

# 48. SOURCE OBJECTIVE 6 — RESTORE CRITICAL MAGNETIC BEARING FEEDBACK

Exact title **Restore Critical Magnetic Bearing Feedback**. Preconditions: 1–5 historically certified. A true player W137 disconnection causes graph break, trusted data loss, permission withdrawal, safe physical response and critical latch. Physically reconnect, validate fresh sensor samples/return/reference, cross-check independent protective systems, explicitly reset, conduct an entirely new full levitation → spin-up → sustained-load interval, confirm safety and physics. Only then let the existing `LevelGamePlayManager` report **6/6**. Reconnection alone, SCADA patch-only status, W135 substitution and stale pre-fault run must fail.

# 49. HISTORICAL CERTIFICATES, RUN IDs, SAVE/LOAD

Persist each objective's `CertifiedAt`, actual circuit topology hash, calibration revision IDs, physical snapshots and telemetry sample validity, machine safety state, event IDs, fault sequence IDs and source runId. Progress should be historically latched (1–5 remain completed during deliberate W137 test), but machine health is live and can change. Objective 6 new run must satisfy `postRepairRunId > faultRunId` plus all gates. Upon scene reload, preserve correct physical and objective state without duplicating fault evidence or accidentally marking the ring qualified because it was qualified in an older run.

# 50. SCADA — ACTUAL MEASURED ENGINEERING STATUS

Build `L87_GravityEngineeringMasterSCADA`, source gap/vibration SCADA and digital twin dashboards showing: actual ring angle/RPM/g and acceleration; angular momentum and counter-torque/flywheel saturation; eight bearing radial gaps, coil currents, controller errors, amplifier temperatures and touch-state; primary vs secondary sensor comparison, sample age and calibration status; motor inverter A/B requested/actual currents, torque, losses and precharge; harmonic/vibration magnitude/phase/spectra and dangerous bands; balance weight positions; rotary coupler power/efficiency, data quality, actual habitat essential loads, thermal budgets; independent E-stop/overspeed state; W137 connected/sample freshness/fault latch/reset readiness; event history and authoritative objective status 0/6–6/6. Chart sample times/units, source validity and uncertainty where useful.

No graph from the visual rig or hardcoded green labels may certify a physical condition. SCADA may request operator controls but must not bypass underlying PLC decisions.

# 51. USER INTERACTION CONTROLS AND FEEDBACK

Expose usable safe interaction commands including: `5V CONTROL POWER`, `E-STOP SELF-TEST`, `CALIBRATE RADIAL A/B`, `CALIBRATE AXIAL`, `CHECK GAP SENSOR AGE`, `PRECHARGE BEARING POWER`, `ENERGIZE BEARINGS`, `VERIFY TOUCHDOWN SUPPORT`, `REQUEST LEVITATION`, `VERIFY ALL 8 COILS`, `MOTOR PRECHARGE`, `INVERTER A/B ENABLE`, `SET TARGET RPM`, `START LOW-SPEED TRIAL`, `MEASURE VIBRATION SPECTRUM`, `RUN RESONANCE CHECK`, `ADJUST BALANCE A/B`, `TUNE DAMPING`, `START SPINUP`, `HOLD RPM`, `ENGAGE ROTARY COUPLER`, `VERIFY OPTICAL DATA`, `SET HABITAT LOADS`, `RUN STABILITY QUALIFICATION`, `INJECT SENSOR DRIFT`, `INJECT AMPLIFIER HEAT`, `INJECT MASS IMBALANCE`, `DIAGNOSE`, `REPAIR`, `RUN FRESH REQUALIFICATION`, `W137 DISCONNECT` and `W137 RECONNECT` through the real wire interaction tool, `SENSOR RECALIBRATION`, `EXPLICIT SAFETY RESET`, `FINAL FULL-CYCLE CERTIFY`.

Separate REQUESTED / MOVING / MEASURED / QUALIFIED. Disable controls through actual interlocks. Provide diagnostic reasons rather than guessing why a state failed.

# 52. AAA VISUALS, MECHANICS AND GAMEPLAY PRESENTATION

Show a prominent ring rotating against Earth, luminous stator and bearing-field rings, eight individually active bearing highlights, moving balance weights, touching/support protection animations, motor power-flow VFX, ring vibration/tiny offsets, thermal overheat visual warnings, 3D orbital gravity vector hologram and a spectrogram. Maintain gold/copper coil and cyan normal-state lighting, amber temperature/imbalance, red fault/overspeed, violet holographic gravity vectors. Visual amplitude should remain readable even when educational physics displacement is sub-millimetre; label visual exaggeration separately and never fake actual metric values.

Use the shipped visual rig's exact supported calls; extend it with new independent art-only setters if necessary, preserving compatibility. Scene should remain efficient: ring art is modular and repeated components should be batched/instanced and LODed. Do not mistake a final completion beacon animating for real `LevelGamePlayManager` 6/6.

# 53. RECOMMENDED IMPLEMENTATION CLASSES — ONLY IF MISSING

Use existing functional equivalents where verified; suggested new responsibilities: `SparkLevel87Coordinator`, `SparkLevel87ProcessState`, `SparkLevel87ElectricalBindings`, `SparkLevel87TerminalMapper`, `SparkLevel87RotorDynamics`, `SparkLevel87GravityCalculator`, `SparkLevel87BearingStationModel`, `SparkLevel87BearingController`, `SparkLevel87GapSensorModel`, `SparkLevel87BearingAmplifierThermal`, `SparkLevel87TouchdownSupport`, `SparkLevel87DriveInverter`, `SparkLevel87MotorPrecharge`, `SparkLevel87StationMomentumCompensator`, `SparkLevel87VibrationModes`, `SparkLevel87VibrationInstrument`, `SparkLevel87BalanceActuator`, `SparkLevel87AdaptiveDamping`, `SparkLevel87RotaryPowerTransfer`, `SparkLevel87RotaryTelemetry`, `SparkLevel87HabitatLoadScheduler`, `SparkLevel87IndependentOverspeed`, `SparkLevel87SafetyPLC`, `SparkLevel87FaultController`, `SparkLevel87W137Recovery`, `SparkLevel87ObjectiveTracker`, `SparkLevel87SCADA`, `SparkLevel87VisualRigAdapter`, `SparkLevel87Diagnostics`, `SparkLevel87BuilderEditor`, and tests. Avoid unnecessary class proliferation/duplicate state; compose interfaces cleanly and make actual source API bindings compile.

# 54. ONE-CLICK UNITY LEVEL 87 EDITOR BUILDER

Create **`Tools/Project Spark/Levels/Create Level 87`** (NOT one of the shipped art-only menus). Builder must:

1. Inspect actual Project Spark source and previous-level Editor builder conventions.
2. Import 86 actual numbered FBX meshes and verify one cm→m conversion, actual bounds and model hierarchy.
3. Create/update `Level_87.asset`, `Level_87_Layout.prefab` and real six-objective definitions with project schemas.
4. Place 86 modular models at common origin under replaceable `L87_HD_VISUAL_ART`; avoid combined showcase duplication.
5. Bind/check 68 pivot IDs, especially the ten coordinated Z-axis ring transforms, and preserve stationary art.
6. Preserve 236 source decorative anchors, create/attach functional terminals for all 316 W endpoints with documented bus decisions.
7. Produce and validate actual typed, player-connectable W001–W158 definitions **without connecting them**.
8. Reconcile mixed signal/return/conflict issues; export conflict and change reports; fail on unsafe shorting.
9. Create 5 V educational control, fictional motor/bearing/rotary plant and independent protection systems.
10. Construct rotor/bearing/inverter/vibration/thermal/power/mission simulation with genuine time stepping.
11. Bind calibrated sensor snapshots, W137 source/target circuit trust, fault latch, redundancy/safety limits and operator reset.
12. Add three disturbances and corresponding measured recovery mechanisms.
13. Add six authoritative objectives, fresh run/evidence persistence and LevelGamePlayManager hooks.
14. Bind SCADA and visual rig as read-only consumers of authoritative state.
15. Provide inspector defaults, safety limits, clear diagnostics, smoke-test and acceptance-test scripts.
16. Run idempotently, preserve user gameplay children and older levels, and produce a truthful validation report.

# 55. PACKAGE/IMPORT ACCEPTANCE TESTS

Perform and record actual test result: PASS/FAIL/UNTESTED, Unity version, renderer pipeline, relevant scene, and evidence. Structural tests are NOT Unity compilation evidence.

1. 86 distinct numbered binary FBXs found with expected names and no duplicates.
2. 86 matching GLBs exist; assembled showcase exists but is not double-instantiated.
3. FBX cm→Unity m applied once, GLB metres checked, expected renderer bounds compared.
4. 184,948 modular triangles / 1,208 mesh parts match the source manifest (before Unity optimization).
5. All 68 unique `ANIM_*` pivots present and actually bind to owning models.
6. Ten main coordinated ring pivots rotate about authored +Z and preserve common center.
7. Fixed structure, support bearings, workstations and viewport do not rotate with habitat.
8. All 236 visual art socket IDs present with correct owners/positions.
9. 316 logical endpoints mapped; 96 reused-anchor records investigated; 69 mixed-signal cases are separated.
10. Correct P019/P135 W137 endpoint mapping, no cross-domain short.
11. 12/36 material references imported correctly; UV/bake gaps identified rather than falsely passed.
12. Visual root rerun idempotent and does not destroy gameplay/terminals or older levels.

# 56. CIRCUIT AND SAFETY ACCEPTANCE TESTS

13. Every W001–W158 is unique with correct owner/anchor/purpose/signal classification.
14. All wires begin unconnected; player can connect via existing wire interaction.
15. Solver reports correct actual topology, power/return and supported junctions.
16. Mixed-type socket conflicts do not short 5 V, analog, actuator, data or safety signals.
17. All real terminal source/target directions and reuse capacity pass.
18. Disconnection changes machine/sensor state, not merely visual cable.
19. W137 true player edge is removable and restores correctly.
20. W135 backup and W136 patch diagnostic cannot substitute W137 normal control trust.
21. Independent overspeed/E-stop works with W137 missing and normal PLC disabled.
22. Fault reset fails when disconnected/stale/calibration/safety prerequisites are invalid.
23. Historical objectives survive fault and avoid accidental completion from wire topology alone.

# 57. DYNAMICS, ENERGY, LEVITATION AND VIBRATION TESTS

24. Computed rim g uses physical scenario r=45 m, not visual r=5.2 m.
25. 3 RPM produces approximately 0.453 g with configured scenario radius.
26. Rotor angular acceleration responds to net torque/inertia rather than immediately equaling target RPM.
27. Rotational kinetic energy and angular momentum integrate consistently.
28. Station reaction torque and counter-momentum saturate at meaningful limits.
29. Drive inverter A/B precharge, torque/current/speed/thermal limits enforced.
30. Actual bearing coil current and force respond with finite dynamics.
31. All eight bearing stations report separate actual gap/temperature/force values.
32. Primary and secondary sensor calibration, bias, freshness and validity behave independently.
33. True radial gap and sensor measurement remain distinct under injected drift.
34. Touchdown support engages with finite motion/contact loss and does not teleport ring to rest.
35. Motor/bearing/coupler power transfer respects finite source power and losses.
36. Vibration grows under eccentric mass and can show a resonance band.
37. Spectrum/phase/shaft orbit comes from sampled time history, not a hardcoded UI trace.
38. Balance masses move with bounded speed and measurably alter vibration force.
39. Amplifier overheat changes actual current capacity and bearing performance.
40. Rotating optical data quality has lag/fault/validity and cannot replace W137.
41. Essential habitat load power/thermal duty genuinely consumes energy.
42. All-loads-off approach fails sustained mission utility.
43. Stable interval requires authentic levitation/speed/thermal/power/safety sample evidence.

# 58. DISTURBANCE, W137 AND OBJECTIVES ACCEPTANCE TESTS

44. Sensor drift injected: actual sensor bias and controller response change, diagnose/repair/recalibrate.
45. Thermal amplifier fault injected: finite heat rise and current degradation, diagnose and repair.
46. Mass imbalance injected: actual vibration amplitude/phase change, balance and requalify.
47. Each repair changes the underlying plant rather than just a screen flag.
48. Objective 5 only after all three causal recoveries and **new full operating run**.
49. W137 physically disconnected after 1–5; Spark graph changes.
50. Missing W137 sample becomes invalid, not zero and not stale-cached-good.
51. Normal high-speed/bearing authorization withdraws and critical latch activates.
52. A physically spinning rotor follows finite rundown/touchdown dynamics.
53. E-stop/overspeed protections remain independent during feedback loss.
54. W135/W136/W149/W155 cannot bypass trusted W137.
55. Reconnect requires correct type/owner/actual live graph, fresh calibration and valid sample timestamps.
56. Reset cannot be accepted before independent safety checks and explicit operator command.
57. Objective 6 cannot pass on disconnect, reconnect, SCADA flag or reset alone.
58. New post-repair full levitation→spinup→load cycle produces fresh evidence and no remaining fault.
59. Objectives 1–5 remain historically certified while live safety status changes.
60. The **real `LevelGamePlayManager` reaches 6/6** only after actual full sequence.
61. Save/load and builder rerun do not duplicate wire connections, objectives or fault evidence.
62. Older Levels 1–86 still load and operate; true Unity compilation and Play Mode evidence reported.

# 59. FULL GAMEPLAY WALKTHROUGH — REQUIRED REAL SEQUENCE

**CONNECT 158 REAL WIRES → RECONCILE 236 VISUAL SOCKETS TO 316 FUNCTIONAL CONTACTS → VERIFY 5 V POWER AND ACTUAL RETURNS → E-STOP/INDEPENDENT OVERSPEED TEST → CALIBRATE PRIMARY/SECONDARY RADIAL SENSORS, AXIAL SENSOR, SPEED ENCODER, VIBRATION PROBES → PRECHARGE BEARING AMPLIFIER → VERIFY ALL EIGHT BEARING STATIONS → ENGAGE TRUE CONTACTLESS LEVITATION → COMMISSION MOTOR PRECHARGE AND INVERTERS → RUN LOW-SPEED ROTOR TEST → ACQUIRE VIBRATION SPECTRUM AND RESONANCE DATA → BALANCE MASSES AND VERIFY DAMPING → EXECUTE FINITE-TORQUE SPIN-UP → REACH TRUE 3-RPM EXAMPLE TARGET AND VALIDATE RIM g → QUALIFY STATION MOMENTUM COMPENSATION → ENABLE ROTARY POWER/DATA TRANSFER → SUPPLY ESSENTIAL LIGHTING AND THERMAL/HABITAT LOADS → HOLD STABLE GRAVITY FOR FRESH QUALIFICATION INTERVAL → INJECT SENSOR DRIFT → DIAGNOSE AND REPAIR/CALIBRATE → INJECT AMPLIFIER OVERHEAT → DIAGNOSE AND RESTORE COOLING/CURRENT CAPACITY → INJECT MASS IMBALANCE → MEASURE PHASE AND REBALANCE → EXECUTE FRESH FULL SPIN QUALIFICATION → AT SAFE SUPERVISED LOW SPEED PHYSICALLY DISCONNECT W137 → TRUSTED GAP FEEDBACK LOST → WITHDRAW HIGH-SPEED PERMIT / CONTROLLED RUNDOWN OR PROTECTED TOUCHDOWN → LATCH CRITICAL FAULT → RECONNECT REAL W137 → VALIDATE FRESH MEASUREMENTS AND INDEPENDENT SAFETY → EXPLICIT RESET → RUN A BRAND-NEW BEARING COMMISSION → CONTROLLED SPIN-UP → SUSTAINED ESSENTIAL-LOAD GRAVITY CERTIFICATION → ACTUAL LEVEL GAMEPLAY MANAGER 6/6.**

# 60. ERRORS, DIAGNOSTICS AND DELIVERABLES

Produce actionable fault diagnostics for: missing model/texture, incorrect FBX scale, duplicate pivot, wrong ring axis, art showcase double-place, missing P ID, reused socket short, missing return conductor, incorrect source/target terminal, swapped analog/power domain, drive precharge incomplete, inverter saturation, nonexistent bearing power, stale radial gap sample, sensor drift, amplifier current/temperature limit, rotor not levitated, excessive gap, dangerous vibration/resonance, counter-momentum saturation, insufficient power transfer, dropped optical telemetry, inadequate essential load duty, overspeed, E-stop, contactless bearing unsafe, touchdown heat, W137 disconnected/untrusted, W135/W136 bypass attempt, reset refused, stale run evidence, objectives stuck at 0/6 or 5/6, accidental destruction of previous-level scenes, and Unity compilation not tested.

Deliver importable C# runtime code and editor scripts, `Level_87.asset`, `Level_87_Layout.prefab`, 86 source art FBX bindings, repaired/optimized PBR and collider setup, 68 pivot bindings, all 236 original visual anchors, a rigorously reconciled functional interface for the 316 proposed endpoints, 158 genuine player-connectable wires, separate control and power/safety plant, gravity-ring rotor, bearing, motor, resonance, thermal, rotary-transfer, independent safety and W137 modules, SCADA, VFX, six objective evaluations, run/fault persistence, code/tests and a truthful validation report.

**Do NOT report Level 87 complete unless the real Unity project compiles, the gameplay was executed, the full post-W137 stable gravity qualification ran and the authoritative `LevelGamePlayManager` reads 6/6. Preserve all earlier levels.**

**PROJECT SPARK — LEVEL 87: THE GRAVITY ENGINE. BUILD THE FIELD. STABILIZE THE RING. MASTER THE MOMENTUM. BRING GRAVITY ONLINE.**

---

# SOURCE-DERIVED ANNEXES — EXACT PACKAGE RECORDS

> The annex data below was converted mechanically from source JSON/CSV in `ProjectSpark_Level87_HD_AAA_Models.zip`, not invented. These remain planning/art records, not proof of functioning Unity objects. Builder should consume the original CSV/JSON files.

## ANNEX A — ALL 86 EXACT MODULAR MODEL FILENAMES AND MESH COUNTS

| # | Exact FBX | Exact GLB | Triangles | Mesh parts |
|---:|---|---|---:|---:|
| 00 | `00_L87_GravityQualificationArenaBase.fbx` | `00_L87_GravityQualificationArenaBase.glb` | 888 | 6 |
| 01 | `01_L87_PanoramicOrbitalObservationViewport.fbx` | `01_L87_PanoramicOrbitalObservationViewport.glb` | 4,928 | 12 |
| 02 | `02_L87_RotatingHabitatRingMainRotor.fbx` | `02_L87_RotatingHabitatRingMainRotor.glb` | 12,608 | 11 |
| 03 | `03_L87_RotatingHabitatSpokeAssembly.fbx` | `03_L87_RotatingHabitatSpokeAssembly.glb` | 1,344 | 4 |
| 04 | `04_L87_StationaryGravityHub.fbx` | `04_L87_StationaryGravityHub.glb` | 4,944 | 4 |
| 05 | `05_L87_HabitatSegmentA.fbx` | `05_L87_HabitatSegmentA.glb` | 1,648 | 4 |
| 06 | `06_L87_HabitatSegmentB.fbx` | `06_L87_HabitatSegmentB.glb` | 1,648 | 4 |
| 07 | `07_L87_HabitatSegmentC.fbx` | `07_L87_HabitatSegmentC.glb` | 1,648 | 4 |
| 08 | `08_L87_HabitatSegmentD.fbx` | `08_L87_HabitatSegmentD.glb` | 1,648 | 4 |
| 09 | `09_L87_HabitatSegmentE.fbx` | `09_L87_HabitatSegmentE.glb` | 1,648 | 4 |
| 10 | `10_L87_HabitatSegmentF.fbx` | `10_L87_HabitatSegmentF.glb` | 1,648 | 4 |
| 11 | `11_L87_HabitatPressureShellRing.fbx` | `11_L87_HabitatPressureShellRing.glb` | 7,488 | 7 |
| 12 | `12_L87_GravityRingStructuralGuideTrack.fbx` | `12_L87_GravityRingStructuralGuideTrack.glb` | 5,132 | 3 |
| 13 | `13_L87_GravityStationFixedSupportFrame.fbx` | `13_L87_GravityStationFixedSupportFrame.glb` | 132 | 4 |
| 14 | `14_L87_GravityStationIsolationTruss.fbx` | `14_L87_GravityStationIsolationTruss.glb` | 600 | 2 |
| 15 | `15_L87_HabitatObservationDeck.fbx` | `15_L87_HabitatObservationDeck.glb` | 160 | 4 |
| 16 | `16_L87_MagneticBearingStationA.fbx` | `16_L87_MagneticBearingStationA.glb` | 3,996 | 9 |
| 17 | `17_L87_MagneticBearingStationB.fbx` | `17_L87_MagneticBearingStationB.glb` | 3,996 | 9 |
| 18 | `18_L87_MagneticBearingStationC.fbx` | `18_L87_MagneticBearingStationC.glb` | 3,996 | 9 |
| 19 | `19_L87_MagneticBearingStationD.fbx` | `19_L87_MagneticBearingStationD.glb` | 3,996 | 9 |
| 20 | `20_L87_MagneticBearingStationE.fbx` | `20_L87_MagneticBearingStationE.glb` | 3,996 | 9 |
| 21 | `21_L87_MagneticBearingStationF.fbx` | `21_L87_MagneticBearingStationF.glb` | 3,996 | 9 |
| 22 | `22_L87_MagneticBearingStationG.fbx` | `22_L87_MagneticBearingStationG.glb` | 3,996 | 9 |
| 23 | `23_L87_MagneticBearingStationH.fbx` | `23_L87_MagneticBearingStationH.glb` | 3,996 | 9 |
| 24 | `24_L87_PrimaryRadialGapSensor.fbx` | `24_L87_PrimaryRadialGapSensor.glb` | 1,356 | 19 |
| 25 | `25_L87_SecondaryRadialGapSensor.fbx` | `25_L87_SecondaryRadialGapSensor.glb` | 1,260 | 17 |
| 26 | `26_L87_AxialGapDisplacementSensor.fbx` | `26_L87_AxialGapDisplacementSensor.glb` | 1,164 | 15 |
| 27 | `27_L87_MagneticBearingCoilCurrentMonitor.fbx` | `27_L87_MagneticBearingCoilCurrentMonitor.glb` | 1,032 | 12 |
| 28 | `28_L87_MagneticFluxVisualizationHologram.fbx` | `28_L87_MagneticFluxVisualizationHologram.glb` | 4,008 | 6 |
| 29 | `29_L87_EmergencyTouchdownBearing.fbx` | `29_L87_EmergencyTouchdownBearing.glb` | 1,968 | 6 |
| 30 | `30_L87_BearingPowerAmplifierCabinet.fbx` | `30_L87_BearingPowerAmplifierCabinet.glb` | 1,536 | 23 |
| 31 | `31_L87_BearingGapCalibrationStage.fbx` | `31_L87_BearingGapCalibrationStage.glb` | 1,032 | 12 |
| 32 | `32_L87_SegmentedRimMotorStator.fbx` | `32_L87_SegmentedRimMotorStator.glb` | 3,504 | 7 |
| 33 | `33_L87_RimDriveRotorMagnetArray.fbx` | `33_L87_RimDriveRotorMagnetArray.glb` | 3,912 | 6 |
| 34 | `34_L87_MotorInverterDriveA.fbx` | `34_L87_MotorInverterDriveA.glb` | 1,296 | 20 |
| 35 | `35_L87_MotorInverterDriveB.fbx` | `35_L87_MotorInverterDriveB.glb` | 1,296 | 20 |
| 36 | `36_L87_MotorPhaseCurrentSensor.fbx` | `36_L87_MotorPhaseCurrentSensor.glb` | 1,128 | 14 |
| 37 | `37_L87_MotorTorqueEstimator.fbx` | `37_L87_MotorTorqueEstimator.glb` | 1,044 | 15 |
| 38 | `38_L87_RotorSpeedOpticalEncoder.fbx` | `38_L87_RotorSpeedOpticalEncoder.glb` | 1,236 | 19 |
| 39 | `39_L87_SafeSpinUpSequencer.fbx` | `39_L87_SafeSpinUpSequencer.glb` | 1,104 | 16 |
| 40 | `40_L87_MotorPrechargeContactor.fbx` | `40_L87_MotorPrechargeContactor.glb` | 1,088 | 15 |
| 41 | `41_L87_DriveThermalCoolingManifold.fbx` | `41_L87_DriveThermalCoolingManifold.glb` | 4,276 | 7 |
| 42 | `42_L87_MotorAccelerationProfileConsole.fbx` | `42_L87_MotorAccelerationProfileConsole.glb` | 996 | 12 |
| 43 | `43_L87_RingAngularMomentumCompensationUnit.fbx` | `43_L87_RingAngularMomentumCompensationUnit.glb` | 4,180 | 5 |
| 44 | `44_L87_MultiAxisVibrationSensorArray.fbx` | `44_L87_MultiAxisVibrationSensorArray.glb` | 1,044 | 15 |
| 45 | `45_L87_VibrationFrequencySpectrumAnalyzer.fbx` | `45_L87_VibrationFrequencySpectrumAnalyzer.glb` | 1,524 | 25 |
| 46 | `46_L87_RotorShaftOrbitProbe.fbx` | `46_L87_RotorShaftOrbitProbe.glb` | 1,164 | 15 |
| 47 | `47_L87_RotorImbalanceVectorDisplay.fbx` | `47_L87_RotorImbalanceVectorDisplay.glb` | 4,296 | 12 |
| 48 | `48_L87_AdaptiveVibrationDampingController.fbx` | `48_L87_AdaptiveVibrationDampingController.glb` | 1,236 | 19 |
| 49 | `49_L87_PrecisionBalanceWeightActuatorA.fbx` | `49_L87_PrecisionBalanceWeightActuatorA.glb` | 216 | 5 |
| 50 | `50_L87_PrecisionBalanceWeightActuatorB.fbx` | `50_L87_PrecisionBalanceWeightActuatorB.glb` | 216 | 5 |
| 51 | `51_L87_RotorMassDistributionScanner.fbx` | `51_L87_RotorMassDistributionScanner.glb` | 1,332 | 21 |
| 52 | `52_L87_ResonanceSweepTestFixture.fbx` | `52_L87_ResonanceSweepTestFixture.glb` | 1,044 | 15 |
| 53 | `53_L87_BearingHousingTemperatureSensor.fbx` | `53_L87_BearingHousingTemperatureSensor.glb` | 1,044 | 14 |
| 54 | `54_L87_DynamicRotorHealthRecorder.fbx` | `54_L87_DynamicRotorHealthRecorder.glb` | 1,416 | 20 |
| 55 | `55_L87_PrecisionGapAndVibrationSCADA.fbx` | `55_L87_PrecisionGapAndVibrationSCADA.glb` | 1,044 | 15 |
| 56 | `56_L87_VibrationFaultCalibrationInjector.fbx` | `56_L87_VibrationFaultCalibrationInjector.glb` | 1,044 | 14 |
| 57 | `57_L87_ContactlessRotaryPowerCouplerPrimary.fbx` | `57_L87_ContactlessRotaryPowerCouplerPrimary.glb` | 2,116 | 8 |
| 58 | `58_L87_ContactlessRotaryPowerCouplerBackup.fbx` | `58_L87_ContactlessRotaryPowerCouplerBackup.glb` | 2,116 | 8 |
| 59 | `59_L87_RotaryOpticalTelemetryLink.fbx` | `59_L87_RotaryOpticalTelemetryLink.glb` | 1,584 | 14 |
| 60 | `60_L87_RotaryPowerIsolationContactor.fbx` | `60_L87_RotaryPowerIsolationContactor.glb` | 1,184 | 17 |
| 61 | `61_L87_HabitatElectricalLoadEmulator.fbx` | `61_L87_HabitatElectricalLoadEmulator.glb` | 1,116 | 16 |
| 62 | `62_L87_HabitatEmergencyLightingLoad.fbx` | `62_L87_HabitatEmergencyLightingLoad.glb` | 1,212 | 17 |
| 63 | `63_L87_HabitatThermalControlLoad.fbx` | `63_L87_HabitatThermalControlLoad.glb` | 1,212 | 18 |
| 64 | `64_L87_RotatingDataQualityMonitor.fbx` | `64_L87_RotatingDataQualityMonitor.glb` | 1,044 | 15 |
| 65 | `65_L87_RotaryPowerTransferMonitor.fbx` | `65_L87_RotaryPowerTransferMonitor.glb` | 1,428 | 23 |
| 66 | `66_L87_HabitatLoadDistributionSimulator.fbx` | `66_L87_HabitatLoadDistributionSimulator.glb` | 1,236 | 19 |
| 67 | `67_L87_GravityHabitatEnvironmentHologram.fbx` | `67_L87_GravityHabitatEnvironmentHologram.glb` | 4,104 | 8 |
| 68 | `68_L87_OrbitalRingPowerBudgetAnalyzer.fbx` | `68_L87_OrbitalRingPowerBudgetAnalyzer.glb` | 1,620 | 27 |
| 69 | `69_L87_GravitySafetySupervisoryPLC.fbx` | `69_L87_GravitySafetySupervisoryPLC.glb` | 2,256 | 40 |
| 70 | `70_L87_MagneticBearingSafetyPLC.fbx` | `70_L87_MagneticBearingSafetyPLC.glb` | 1,584 | 26 |
| 71 | `71_L87_IndependentOverspeedProtector.fbx` | `71_L87_IndependentOverspeedProtector.glb` | 1,200 | 18 |
| 72 | `72_L87_RingEmergencyStopStation.fbx` | `72_L87_RingEmergencyStopStation.glb` | 1,020 | 13 |
| 73 | `73_L87_Protected5VControlSupply.fbx` | `73_L87_Protected5VControlSupply.glb` | 1,764 | 28 |
| 74 | `74_L87_MagneticBearingControlPLC.fbx` | `74_L87_MagneticBearingControlPLC.glb` | 3,012 | 54 |
| 75 | `75_L87_RotorMotorControlPLC.fbx` | `75_L87_RotorMotorControlPLC.glb` | 2,148 | 36 |
| 76 | `76_L87_HabitatPowerDistributionPLC.fbx` | `76_L87_HabitatPowerDistributionPLC.glb` | 2,436 | 42 |
| 77 | `77_L87_RadialSensorDriftFaultInjector.fbx` | `77_L87_RadialSensorDriftFaultInjector.glb` | 1,020 | 13 |
| 78 | `78_L87_BearingAmplifierHeatFaultInjector.fbx` | `78_L87_BearingAmplifierHeatFaultInjector.glb` | 1,020 | 13 |
| 79 | `79_L87_MassImbalanceFaultInjector.fbx` | `79_L87_MassImbalanceFaultInjector.glb` | 1,020 | 13 |
| 80 | `80_L87_W137CriticalFeedbackPatchStation.fbx` | `80_L87_W137CriticalFeedbackPatchStation.glb` | 1,500 | 18 |
| 81 | `81_L87_GravityEngineeringMasterSCADA.fbx` | `81_L87_GravityEngineeringMasterSCADA.glb` | 4,920 | 39 |
| 82 | `82_L87_RingCertificationBeacon.fbx` | `82_L87_RingCertificationBeacon.glb` | 1,672 | 9 |
| 83 | `83_L87_GravityMachineEventRecorder.fbx` | `83_L87_GravityMachineEventRecorder.glb` | 1,380 | 20 |
| 84 | `84_L87_GravityEngineInspectionCamera.fbx` | `84_L87_GravityEngineInspectionCamera.glb` | 912 | 10 |
| 85 | `85_L87_StationSafetyIsolationRelay.fbx` | `85_L87_StationSafetyIsolationRelay.glb` | 996 | 12 |

**Total modular triangles: 184,948; mesh parts: 1,208.** Alternative assembled showcase files: `Models/Level87_Complete_ArtKit_Showcase.fbx` and `GLB/Level87_Complete_ArtKit_Showcase.glb` (do not instantiate both showcase and 86 modular pieces).

## ANNEX B — ALL 68 SOURCE PIVOT PARENTS, AXES AND COORDINATES

`Kind=indicator` denotes display/VFX, not an automatically actuated machine axis. Position is from CSV in metres; FBX importer scale conversion still applies.

| # | Pivot | Model owner | Kind | Axis | X m | Y m | Z m |
|---:|---|---|---|:---:|---:|---:|---:|
| 1 | `ANIM_GRAVITY_RING_MAIN_ROTATION` | `L87_RotatingHabitatRingMainRotor` | rotation | `z` | 0 | 7.25 | -7.2 |
| 2 | `ANIM_GRAVITY_RING_SPOKES` | `L87_RotatingHabitatSpokeAssembly` | rotation | `z` | 0 | 7.25 | -7.2 |
| 3 | `ANIM_HABITAT_SEGMENT_A` | `L87_HabitatSegmentA` | rotation | `z` | 0 | 7.25 | -7.2 |
| 4 | `ANIM_HABITAT_SEGMENT_B` | `L87_HabitatSegmentB` | rotation | `z` | 0 | 7.25 | -7.2 |
| 5 | `ANIM_HABITAT_SEGMENT_C` | `L87_HabitatSegmentC` | rotation | `z` | 0 | 7.25 | -7.2 |
| 6 | `ANIM_HABITAT_SEGMENT_D` | `L87_HabitatSegmentD` | rotation | `z` | 0 | 7.25 | -7.2 |
| 7 | `ANIM_HABITAT_SEGMENT_E` | `L87_HabitatSegmentE` | rotation | `z` | 0 | 7.25 | -7.2 |
| 8 | `ANIM_HABITAT_SEGMENT_F` | `L87_HabitatSegmentF` | rotation | `z` | 0 | 7.25 | -7.2 |
| 9 | `ANIM_HABITAT_PRESSURE_SHELL` | `L87_HabitatPressureShellRing` | rotation | `z` | 0 | 7.25 | -7.2 |
| 10 | `ANIM_STATION_HUB_FLYWHEEL` | `L87_StationaryGravityHub` | rotation | `z` | 0 | 7.25 | -7.15 |
| 11 | `ANIM_BEARING_A_FIELD` | `L87_MagneticBearingStationA` | indicator | `none` | 5.85 | 7.249999999999997 | -6.65 |
| 12 | `ANIM_BEARING_B_FIELD` | `L87_MagneticBearingStationB` | indicator | `none` | 4.136574669941301 | 11.386574669941304 | -6.65 |
| 13 | `ANIM_BEARING_C_FIELD` | `L87_MagneticBearingStationC` | indicator | `none` | 3.223882698755407e-15 | 13.1 | -6.65 |
| 14 | `ANIM_BEARING_D_FIELD` | `L87_MagneticBearingStationD` | indicator | `none` | -4.136574669941297 | 11.386574669941309 | -6.65 |
| 15 | `ANIM_BEARING_E_FIELD` | `L87_MagneticBearingStationE` | indicator | `none` | -5.85 | 7.2500000000000036 | -6.65 |
| 16 | `ANIM_BEARING_F_FIELD` | `L87_MagneticBearingStationF` | indicator | `none` | -4.136574669941302 | 3.1134253300586963 | -6.65 |
| 17 | `ANIM_BEARING_G_FIELD` | `L87_MagneticBearingStationG` | indicator | `none` | -1.4331988586748073e-14 | 1.4000000000000004 | -6.65 |
| 18 | `ANIM_BEARING_H_FIELD` | `L87_MagneticBearingStationH` | indicator | `none` | 4.136574669941296 | 3.11342533005869 | -6.65 |
| 19 | `ANIM_PRIMARY_RADIAL_SENSOR` | `L87_PrimaryRadialGapSensor` | translation | `x` | -12.3 | 1.75 | 2.2 |
| 20 | `ANIM_SECONDARY_RADIAL_SENSOR` | `L87_SecondaryRadialGapSensor` | translation | `x` | -11.0 | 1.75 | 2.2 |
| 21 | `ANIM_AXIAL_SENSOR` | `L87_AxialGapDisplacementSensor` | translation | `z` | -9.7 | 1.75 | 2.2 |
| 22 | `ANIM_MAGNETIC_FIELD_LINES` | `L87_MagneticFluxVisualizationHologram` | indicator | `none` | -2.1 | 7.5 | -4.1 |
| 23 | `ANIM_EMERGENCY_TOUCHDOWN_RACE` | `L87_EmergencyTouchdownBearing` | translation | `x` | -11.7 | 4.2 | -5.6 |
| 24 | `ANIM_BEARING_AMPLIFIER_COOLING` | `L87_BearingPowerAmplifierCabinet` | rotation | `z` | -7.1 | 1.8 | 2.2 |
| 25 | `ANIM_SEGMENTED_RIM_DRIVE` | `L87_SegmentedRimMotorStator` | indicator | `none` | 0 | 7.25 | -6.9 |
| 26 | `ANIM_ROTOR_MAGNET_ARRAY` | `L87_RimDriveRotorMagnetArray` | rotation | `z` | 0 | 7.25 | -7.2 |
| 27 | `ANIM_DRIVE_A_INVERTER` | `L87_MotorInverterDriveA` | indicator | `none` | -4.5 | 1.75 | 2.2 |
| 28 | `ANIM_DRIVE_B_INVERTER` | `L87_MotorInverterDriveB` | indicator | `none` | -3.2 | 1.75 | 2.2 |
| 29 | `ANIM_TORQUE_ESTIMATOR` | `L87_MotorTorqueEstimator` | indicator | `none` | -0.6 | 1.75 | 2.2 |
| 30 | `ANIM_SPEED_ENCODER` | `L87_RotorSpeedOpticalEncoder` | rotation | `z` | 0.7 | 1.75 | 2.2 |
| 31 | `ANIM_SAFE_SPINUP_PROFILE` | `L87_SafeSpinUpSequencer` | indicator | `none` | 2.0 | 1.75 | 2.2 |
| 32 | `ANIM_MOTOR_PRECHARGE_CONTACTOR` | `L87_MotorPrechargeContactor` | translation | `x` | 3.3 | 1.75 | 2.2 |
| 33 | `ANIM_DRIVE_COOLING_IMPELLER` | `L87_DriveThermalCoolingManifold` | rotation | `x` | 4.6 | 1.75 | 2.2 |
| 34 | `ANIM_COUNTERTORQUE_FLYWHEEL` | `L87_RingAngularMomentumCompensationUnit` | rotation | `x` | 7.2 | 1.75 | 2.2 |
| 35 | `ANIM_VIBRATION_PROBE_AXES` | `L87_MultiAxisVibrationSensorArray` | indicator | `none` | -12.3 | 1.75 | 4.9 |
| 36 | `ANIM_SPECTRAL_ANALYZER` | `L87_VibrationFrequencySpectrumAnalyzer` | indicator | `none` | -11 | 1.75 | 4.9 |
| 37 | `ANIM_SHAFT_ORBIT_PROBE` | `L87_RotorShaftOrbitProbe` | translation | `z` | -9.7 | 1.75 | 4.9 |
| 38 | `ANIM_IMBALANCE_VECTOR` | `L87_RotorImbalanceVectorDisplay` | indicator | `none` | -8.4 | 1.75 | 4.9 |
| 39 | `ANIM_ADAPTIVE_DAMPING` | `L87_AdaptiveVibrationDampingController` | indicator | `none` | -7.1 | 1.75 | 4.9 |
| 40 | `ANIM_BALANCE_WEIGHT_A` | `L87_PrecisionBalanceWeightActuatorA` | translation | `x` | -6.5 | 7.5 | -4.8 |
| 41 | `ANIM_BALANCE_WEIGHT_B` | `L87_PrecisionBalanceWeightActuatorB` | translation | `x` | 6.5 | 7.5 | -4.8 |
| 42 | `ANIM_MASS_SCAN` | `L87_RotorMassDistributionScanner` | indicator | `none` | -5.8 | 1.75 | 4.9 |
| 43 | `ANIM_RESONANCE_SWEEP` | `L87_ResonanceSweepTestFixture` | indicator | `none` | -4.5 | 1.75 | 4.9 |
| 44 | `ANIM_BEARING_TEMPERATURE` | `L87_BearingHousingTemperatureSensor` | indicator | `none` | -3.2 | 1.75 | 4.9 |
| 45 | `ANIM_VIBRATION_SCADA` | `L87_PrecisionGapAndVibrationSCADA` | indicator | `none` | -0.6 | 1.75 | 4.9 |
| 46 | `ANIM_FAULT_CALIBRATION` | `L87_VibrationFaultCalibrationInjector` | translation | `x` | 0.7 | 1.75 | 4.9 |
| 47 | `ANIM_PRIMARY_POWER_COUPLER` | `L87_ContactlessRotaryPowerCouplerPrimary` | rotation | `z` | 8.35 | 2.85 | 1.7 |
| 48 | `ANIM_BACKUP_POWER_COUPLER` | `L87_ContactlessRotaryPowerCouplerBackup` | rotation | `z` | 9.7 | 2.85 | 1.7 |
| 49 | `ANIM_ROTARY_OPTICAL_LINK` | `L87_RotaryOpticalTelemetryLink` | indicator | `none` | 11.05 | 2.85 | 1.7 |
| 50 | `ANIM_ROTARY_POWER_CONTACTOR` | `L87_RotaryPowerIsolationContactor` | translation | `x` | 12.4 | 2.85 | 1.7 |
| 51 | `ANIM_HABITAT_LOAD_A` | `L87_HabitatElectricalLoadEmulator` | indicator | `none` | 8.35 | 1.7 | 4.65 |
| 52 | `ANIM_HABITAT_EMERGENCY_LIGHT` | `L87_HabitatEmergencyLightingLoad` | indicator | `none` | 9.7 | 1.7 | 4.65 |
| 53 | `ANIM_HABITAT_THERMAL_LOAD` | `L87_HabitatThermalControlLoad` | indicator | `none` | 11.05 | 1.7 | 4.65 |
| 54 | `ANIM_ROTARY_DATA_QUALITY` | `L87_RotatingDataQualityMonitor` | indicator | `none` | 12.4 | 1.7 | 4.65 |
| 55 | `ANIM_ROTARY_POWER_MONITOR` | `L87_RotaryPowerTransferMonitor` | indicator | `none` | 8.35 | 3.95 | 4.65 |
| 56 | `ANIM_LOAD_DISTURBANCE_SLIDER` | `L87_HabitatLoadDistributionSimulator` | translation | `x` | 9.7 | 3.95 | 4.65 |
| 57 | `ANIM_ARTIFICIAL_GRAVITY_HOLOGRAM` | `L87_GravityHabitatEnvironmentHologram` | indicator | `none` | 2.4 | 8.5 | -3.7 |
| 58 | `ANIM_POWER_BUDGET` | `L87_OrbitalRingPowerBudgetAnalyzer` | indicator | `none` | 11.05 | 3.95 | 4.65 |
| 59 | `ANIM_SAFETY_SUPERVISOR` | `L87_GravitySafetySupervisoryPLC` | indicator | `none` | -11.7 | 3.75 | 7.2 |
| 60 | `ANIM_TRUSTED_RADIAL_SAFETY` | `L87_MagneticBearingSafetyPLC` | indicator | `none` | -10.3 | 3.75 | 7.2 |
| 61 | `ANIM_INDEPENDENT_OVERSPEED` | `L87_IndependentOverspeedProtector` | indicator | `none` | -8.9 | 3.75 | 7.2 |
| 62 | `ANIM_EMERGENCY_STOP` | `L87_RingEmergencyStopStation` | translation | `z` | -7.5 | 3.75 | 7.2 |
| 63 | `ANIM_GAP_SENSOR_DRIFT_FAULT` | `L87_RadialSensorDriftFaultInjector` | translation | `x` | -0.5 | 3.75 | 7.2 |
| 64 | `ANIM_AMP_OVERHEAT_FAULT` | `L87_BearingAmplifierHeatFaultInjector` | translation | `y` | 0.9 | 3.75 | 7.2 |
| 65 | `ANIM_MASS_IMBALANCE_FAULT` | `L87_MassImbalanceFaultInjector` | translation | `x` | 2.3 | 3.75 | 7.2 |
| 66 | `ANIM_W137_CRITICAL_FEEDBACK` | `L87_W137CriticalFeedbackPatchStation` | indicator | `none` | 3.7 | 3.75 | 7.2 |
| 67 | `ANIM_GRAVITY_MASTER_SCADA` | `L87_GravityEngineeringMasterSCADA` | indicator | `none` | 0 | 11.8 | -8.7 |
| 68 | `ANIM_FINAL_CERTIFICATION` | `L87_RingCertificationBeacon` | indicator | `none` | 0 | 12.75 | -8.0 |

## ANNEX C — ALL 158 SOURCE WIRE ROUTES (CONCEPTUAL, NOT YET GAME WIRES)

**Use all identifiers unchanged.** Each source row is `CONCEPT_ONLY_NOT_PLAYER_WIRE`. `Source/Target` names and anchor IDs are art-derived and must be reconciled to real typed SparkTerminals; `SignalKind` is the source classification, which may need an audited functional interpretation (do not silently rewrite it).

| WID | Source model (socket) | Target model (socket) | Source SignalKind | Intended purpose | Critical |
|---|---|---|---|---|:---:|
| **W001** | `L87_Protected5VControlSupply` (`P146`) | `L87_GravitySafetySupervisoryPLC` (`P121`) | `POWER_CONTROL` | Safety supervisory PLC regulated logic supply | NO |
| **W002** | `L87_Protected5VControlSupply` (`P150`) | `L87_MagneticBearingSafetyPLC` (`P136`) | `POWER_CONTROL` | Bearing safety PLC regulated logic supply | NO |
| **W003** | `L87_Protected5VControlSupply` (`P145`) | `L87_MagneticBearingControlPLC` (`P157`) | `POWER_CONTROL` | Magnetic bearing control PLC supply | NO |
| **W004** | `L87_Protected5VControlSupply` (`P149`) | `L87_RotorMotorControlPLC` (`P180`) | `POWER_CONTROL` | Main motor control PLC supply | NO |
| **W005** | `L87_Protected5VControlSupply` (`P153`) | `L87_HabitatPowerDistributionPLC` (`P194`) | `POWER_CONTROL` | Habitat electrical distribution PLC supply | NO |
| **W006** | `L87_Protected5VControlSupply` (`P148`) | `L87_GravityEngineeringMasterSCADA` (`P217`) | `POWER_CONTROL` | SCADA readout logic supply | NO |
| **W007** | `L87_Protected5VControlSupply` (`P152`) | `L87_IndependentOverspeedProtector` (`P142`) | `POWER_CONTROL` | Independent overspeed logic interface | NO |
| **W008** | `L87_Protected5VControlSupply` (`P147`) | `L87_BearingPowerAmplifierCabinet` (`P030`) | `POWER_CONTROL` | Bearing amplifier controller supply | NO |
| **W009** | `L87_Protected5VControlSupply` (`P151`) | `L87_MotorInverterDriveA` (`P037`) | `POWER_CONTROL` | Motor inverter A control logic supply | NO |
| **W010** | `L87_Protected5VControlSupply` (`P146`) | `L87_MotorInverterDriveB` (`P042`) | `POWER_CONTROL` | Motor inverter B control logic supply | NO |
| **W011** | `L87_Protected5VControlSupply` (`P150`) | `L87_W137CriticalFeedbackPatchStation` (`P210`) | `POWER_CONTROL` | Critical W137 patch monitor supply | NO |
| **W012** | `L87_Protected5VControlSupply` (`P145`) | `L87_GravityMachineEventRecorder` (`P233`) | `POWER_CONTROL` | Mission event recorder digital supply | NO |
| **W013** | `L87_MagneticBearingControlPLC` (`P170`) | `L87_MagneticBearingStationA` (`P002`) | `ACTUATOR_COMMAND` | Bearing 1 magnetic current request | NO |
| **W014** | `L87_MagneticBearingStationA` (`P002`) | `L87_MagneticBearingControlPLC` (`P174`) | `ANALOG_FEEDBACK` | Bearing 1 measured displacement and field | NO |
| **W015** | `L87_MagneticBearingControlPLC` (`P156`) | `L87_MagneticBearingStationB` (`P004`) | `ACTUATOR_COMMAND` | Bearing 2 magnetic current request | NO |
| **W016** | `L87_MagneticBearingStationB` (`P004`) | `L87_MagneticBearingControlPLC` (`P160`) | `ANALOG_FEEDBACK` | Bearing 2 measured displacement and field | NO |
| **W017** | `L87_MagneticBearingControlPLC` (`P164`) | `L87_MagneticBearingStationC` (`P006`) | `ACTUATOR_COMMAND` | Bearing 3 magnetic current request | NO |
| **W018** | `L87_MagneticBearingStationC` (`P006`) | `L87_MagneticBearingControlPLC` (`P168`) | `ANALOG_FEEDBACK` | Bearing 3 measured displacement and field | NO |
| **W019** | `L87_MagneticBearingControlPLC` (`P172`) | `L87_MagneticBearingStationD` (`P008`) | `ACTUATOR_COMMAND` | Bearing 4 magnetic current request | NO |
| **W020** | `L87_MagneticBearingStationD` (`P008`) | `L87_MagneticBearingControlPLC` (`P154`) | `ANALOG_FEEDBACK` | Bearing 4 measured displacement and field | NO |
| **W021** | `L87_MagneticBearingControlPLC` (`P158`) | `L87_MagneticBearingStationE` (`P010`) | `ACTUATOR_COMMAND` | Bearing 5 magnetic current request | NO |
| **W022** | `L87_MagneticBearingStationE` (`P010`) | `L87_MagneticBearingControlPLC` (`P162`) | `ANALOG_FEEDBACK` | Bearing 5 measured displacement and field | NO |
| **W023** | `L87_MagneticBearingControlPLC` (`P166`) | `L87_MagneticBearingStationF` (`P012`) | `ACTUATOR_COMMAND` | Bearing 6 magnetic current request | NO |
| **W024** | `L87_MagneticBearingStationF` (`P012`) | `L87_MagneticBearingControlPLC` (`P170`) | `ANALOG_FEEDBACK` | Bearing 6 measured displacement and field | NO |
| **W025** | `L87_MagneticBearingControlPLC` (`P174`) | `L87_MagneticBearingStationG` (`P014`) | `ACTUATOR_COMMAND` | Bearing 7 magnetic current request | NO |
| **W026** | `L87_MagneticBearingStationG` (`P014`) | `L87_MagneticBearingControlPLC` (`P156`) | `ANALOG_FEEDBACK` | Bearing 7 measured displacement and field | NO |
| **W027** | `L87_MagneticBearingControlPLC` (`P160`) | `L87_MagneticBearingStationH` (`P016`) | `ACTUATOR_COMMAND` | Bearing 8 magnetic current request | NO |
| **W028** | `L87_MagneticBearingStationH` (`P016`) | `L87_MagneticBearingControlPLC` (`P164`) | `ANALOG_FEEDBACK` | Bearing 8 measured displacement and field | NO |
| **W029** | `L87_PrimaryRadialGapSensor` (`P019`) | `L87_MagneticBearingControlPLC` (`P168`) | `ANALOG_FEEDBACK` | Primary calibrated radial gap feedback | NO |
| **W030** | `L87_SecondaryRadialGapSensor` (`P020`) | `L87_MagneticBearingControlPLC` (`P172`) | `ANALOG_FEEDBACK` | Redundant radial gap comparison | NO |
| **W031** | `L87_AxialGapDisplacementSensor` (`P022`) | `L87_MagneticBearingControlPLC` (`P154`) | `ANALOG_FEEDBACK` | Axial gap displacement feedback | NO |
| **W032** | `L87_MagneticBearingCoilCurrentMonitor` (`P023`) | `L87_MagneticBearingControlPLC` (`P158`) | `ANALOG_FEEDBACK` | Measured stator coil current | NO |
| **W033** | `L87_MagneticBearingControlPLC` (`P162`) | `L87_BearingPowerAmplifierCabinet` (`P028`) | `ACTUATOR_COMMAND` | Bearing amplifier setpoint | NO |
| **W034** | `L87_BearingPowerAmplifierCabinet` (`P027`) | `L87_MagneticBearingControlPLC` (`P166`) | `ANALOG_FEEDBACK` | Bearing amplifier actual current limits | NO |
| **W035** | `L87_MagneticBearingControlPLC` (`P170`) | `L87_EmergencyTouchdownBearing` (`P026`) | `SAFETY_CONTROL` | Touchdown bearing actuation command | NO |
| **W036** | `L87_EmergencyTouchdownBearing` (`P026`) | `L87_MagneticBearingSafetyPLC` (`P138`) | `DIGITAL_STATUS` | Touchdown bearing engaged state | NO |
| **W037** | `L87_BearingGapCalibrationStage` (`P032`) | `L87_MagneticBearingControlPLC` (`P175`) | `ANALOG_FEEDBACK` | Magnetic gap home-reference calibration | NO |
| **W038** | `L87_MagneticBearingControlPLC` (`P157`) | `L87_MagneticFluxVisualizationHologram` (`P024`) | `DATA_STREAM` | Magnetic field vector visualization telemetry | NO |
| **W039** | `L87_RotorMotorControlPLC` (`P179`) | `L87_MotorInverterDriveA` (`P038`) | `ACTUATOR_COMMAND` | Inverter A torque phase modulation request | NO |
| **W040** | `L87_RotorMotorControlPLC` (`P183`) | `L87_MotorInverterDriveB` (`P043`) | `ACTUATOR_COMMAND` | Inverter B torque phase modulation request | NO |
| **W041** | `L87_MotorInverterDriveA` (`P039`) | `L87_SegmentedRimMotorStator` (`P034`) | `ACTUATOR_COMMAND` | Motor A stator phase winding request | NO |
| **W042** | `L87_MotorInverterDriveB` (`P040`) | `L87_SegmentedRimMotorStator` (`P034`) | `ACTUATOR_COMMAND` | Motor B stator phase winding request | NO |
| **W043** | `L87_RimDriveRotorMagnetArray` (`P035`) | `L87_RotorSpeedOpticalEncoder` (`P048`) | `ANALOG_FEEDBACK` | Rotor magnet encoder reference | NO |
| **W044** | `L87_RotorSpeedOpticalEncoder` (`P049`) | `L87_RotorMotorControlPLC` (`P177`) | `ANALOG_FEEDBACK` | Actual rotating ring angular velocity | NO |
| **W045** | `L87_MotorPhaseCurrentSensor` (`P045`) | `L87_RotorMotorControlPLC` (`P181`) | `ANALOG_FEEDBACK` | Three phase motor current samples | NO |
| **W046** | `L87_MotorTorqueEstimator` (`P046`) | `L87_RotorMotorControlPLC` (`P185`) | `ANALOG_FEEDBACK` | Measured motor torque estimator output | NO |
| **W047** | `L87_RotorMotorControlPLC` (`P176`) | `L87_MotorPrechargeContactor` (`P052`) | `ACTUATOR_COMMAND` | Drive precharge contactor request | NO |
| **W048** | `L87_MotorPrechargeContactor` (`P052`) | `L87_RotorMotorControlPLC` (`P180`) | `ANALOG_FEEDBACK` | Drive precharge actual feedback | NO |
| **W049** | `L87_DriveThermalCoolingManifold` (`P054`) | `L87_MotorInverterDriveA` (`P038`) | `ANALOG_FEEDBACK` | Drive cooling manifold health and cooling flow | NO |
| **W050** | `L87_DriveThermalCoolingManifold` (`P054`) | `L87_MotorInverterDriveB` (`P043`) | `ANALOG_FEEDBACK` | Inverter B cooling capacity | NO |
| **W051** | `L87_SafeSpinUpSequencer` (`P051`) | `L87_RotorMotorControlPLC` (`P186`) | `DIGITAL_CONTROL` | Authorized jerk-limited speed profile | NO |
| **W052** | `L87_MotorAccelerationProfileConsole` (`P055`) | `L87_RotorMotorControlPLC` (`P177`) | `DATA_STREAM` | Actual versus requested acceleration profile | NO |
| **W053** | `L87_RingAngularMomentumCompensationUnit` (`P056`) | `L87_GravitySafetySupervisoryPLC` (`P120`) | `ANALOG_FEEDBACK` | Station reaction-torque balance telemetry | NO |
| **W054** | `L87_RotorMotorControlPLC` (`P182`) | `L87_RingAngularMomentumCompensationUnit` (`P056`) | `ACTUATOR_COMMAND` | Countermoment flywheel command | NO |
| **W055** | `L87_MotorInverterDriveA` (`P039`) | `L87_MotorPhaseCurrentSensor` (`P044`) | `ANALOG_FEEDBACK` | Motor A inverter output phase-current readout | NO |
| **W056** | `L87_MotorInverterDriveB` (`P040`) | `L87_MotorPhaseCurrentSensor` (`P044`) | `ANALOG_FEEDBACK` | Motor B inverter output phase-current readout | NO |
| **W057** | `L87_RotorMotorControlPLC` (`P188`) | `L87_SafeSpinUpSequencer` (`P050`) | `DIGITAL_CONTROL` | Motor speed ramp request | NO |
| **W058** | `L87_RotorSpeedOpticalEncoder` (`P048`) | `L87_SafeSpinUpSequencer` (`P050`) | `ANALOG_FEEDBACK` | Spinup reference measurement timestamps | NO |
| **W059** | `L87_MotorInverterDriveA` (`P038`) | `L87_RotorMotorControlPLC` (`P180`) | `DIGITAL_STATUS` | Motor A inverter fault and temperature status | NO |
| **W060** | `L87_MotorInverterDriveB` (`P043`) | `L87_RotorMotorControlPLC` (`P184`) | `DIGITAL_STATUS` | Motor B inverter fault and temperature status | NO |
| **W061** | `L87_MultiAxisVibrationSensorArray` (`P057`) | `L87_VibrationFrequencySpectrumAnalyzer` (`P059`) | `ANALOG_FEEDBACK` | Actual multi-axis vibration samples | NO |
| **W062** | `L87_RotorShaftOrbitProbe` (`P064`) | `L87_VibrationFrequencySpectrumAnalyzer` (`P063`) | `ANALOG_FEEDBACK` | Rotor shaft orbit probe trace | NO |
| **W063** | `L87_VibrationFrequencySpectrumAnalyzer` (`P061`) | `L87_RotorImbalanceVectorDisplay` (`P068`) | `DATA_STREAM` | Measured vibration magnitude and synchronous phase | NO |
| **W064** | `L87_RotorImbalanceVectorDisplay` (`P068`) | `L87_AdaptiveVibrationDampingController` (`P070`) | `DATA_STREAM` | Imbalance force vector estimate | NO |
| **W065** | `L87_AdaptiveVibrationDampingController` (`P071`) | `L87_MagneticBearingControlPLC` (`P165`) | `ACTUATOR_COMMAND` | Adaptive damping bearing current correction | NO |
| **W066** | `L87_AdaptiveVibrationDampingController` (`P069`) | `L87_RotorMotorControlPLC` (`P180`) | `SAFETY_CONTROL` | Resonance-aware motor torque limit request | NO |
| **W067** | `L87_RotorMassDistributionScanner` (`P077`) | `L87_RotorImbalanceVectorDisplay` (`P066`) | `ANALOG_FEEDBACK` | Measured rotating mass asymmetry | NO |
| **W068** | `L87_ResonanceSweepTestFixture` (`P078`) | `L87_VibrationFrequencySpectrumAnalyzer` (`P063`) | `DIGITAL_CONTROL` | Vibration-frequency excitation test request | NO |
| **W069** | `L87_VibrationFrequencySpectrumAnalyzer` (`P061`) | `L87_ResonanceSweepTestFixture` (`P078`) | `DATA_STREAM` | Calibrated frequency sweep measurement | NO |
| **W070** | `L87_RotorImbalanceVectorDisplay` (`P068`) | `L87_PrecisionBalanceWeightActuatorA` (`P072`) | `ACTUATOR_COMMAND` | Balance weight A calibrated position request | NO |
| **W071** | `L87_RotorImbalanceVectorDisplay` (`P068`) | `L87_PrecisionBalanceWeightActuatorB` (`P073`) | `ACTUATOR_COMMAND` | Balance weight B calibrated position request | NO |
| **W072** | `L87_PrecisionBalanceWeightActuatorA` (`P072`) | `L87_DynamicRotorHealthRecorder` (`P082`) | `ANALOG_FEEDBACK` | Balance A achieved actuator position | NO |
| **W073** | `L87_PrecisionBalanceWeightActuatorB` (`P073`) | `L87_DynamicRotorHealthRecorder` (`P081`) | `ANALOG_FEEDBACK` | Balance B achieved actuator position | NO |
| **W074** | `L87_BearingHousingTemperatureSensor` (`P079`) | `L87_DynamicRotorHealthRecorder` (`P080`) | `ANALOG_FEEDBACK` | Bearing housing temperature history | NO |
| **W075** | `L87_MultiAxisVibrationSensorArray` (`P057`) | `L87_DynamicRotorHealthRecorder` (`P084`) | `ANALOG_FEEDBACK` | Axial vibration cross-check telemetry | NO |
| **W076** | `L87_DynamicRotorHealthRecorder` (`P083`) | `L87_PrecisionGapAndVibrationSCADA` (`P085`) | `DATA_STREAM` | Rotor diagnostic validity and run evidence | NO |
| **W077** | `L87_VibrationFaultCalibrationInjector` (`P086`) | `L87_VibrationFrequencySpectrumAnalyzer` (`P060`) | `DIGITAL_STATUS` | Controlled vibration instrumentation fault diagnostic | NO |
| **W078** | `L87_VibrationFrequencySpectrumAnalyzer` (`P058`) | `L87_GravitySafetySupervisoryPLC` (`P120`) | `SAFETY_CONTROL` | Independent vibration overlimit status | NO |
| **W079** | `L87_DynamicRotorHealthRecorder` (`P084`) | `L87_GravityEngineeringMasterSCADA` (`P225`) | `DATA_STREAM` | Rotating machine vibration trend telemetry | NO |
| **W080** | `L87_AdaptiveVibrationDampingController` (`P071`) | `L87_GravityEngineeringMasterSCADA` (`P212`) | `DATA_STREAM` | Damping controller effectiveness status | NO |
| **W081** | `L87_HabitatPowerDistributionPLC` (`P193`) | `L87_ContactlessRotaryPowerCouplerPrimary` (`P088`) | `DIGITAL_CONTROL` | Primary contactless habitat power authorization | NO |
| **W082** | `L87_HabitatPowerDistributionPLC` (`P197`) | `L87_ContactlessRotaryPowerCouplerBackup` (`P089`) | `DIGITAL_CONTROL` | Backup contactless power authorization | NO |
| **W083** | `L87_ContactlessRotaryPowerCouplerPrimary` (`P087`) | `L87_HabitatPowerDistributionPLC` (`P201`) | `ANALOG_FEEDBACK` | Primary rotating power transfer measured efficiency | NO |
| **W084** | `L87_ContactlessRotaryPowerCouplerBackup` (`P090`) | `L87_HabitatPowerDistributionPLC` (`P189`) | `ANALOG_FEEDBACK` | Backup rotating power transfer measured efficiency | NO |
| **W085** | `L87_RotaryOpticalTelemetryLink` (`P091`) | `L87_RotatingDataQualityMonitor` (`P102`) | `DATA_STREAM` | Optical rotary telemetry good packets | NO |
| **W086** | `L87_RotatingDataQualityMonitor` (`P102`) | `L87_HabitatPowerDistributionPLC` (`P194`) | `DATA_STREAM` | Rotary signal timestamp integrity | NO |
| **W087** | `L87_HabitatPowerDistributionPLC` (`P198`) | `L87_RotaryPowerIsolationContactor` (`P093`) | `ACTUATOR_COMMAND` | Contactless supply isolation contactor request | NO |
| **W088** | `L87_RotaryPowerIsolationContactor` (`P093`) | `L87_HabitatPowerDistributionPLC` (`P202`) | `ANALOG_FEEDBACK` | Actual isolation mechanism feedback | NO |
| **W089** | `L87_HabitatElectricalLoadEmulator` (`P095`) | `L87_HabitatPowerDistributionPLC` (`P190`) | `ANALOG_FEEDBACK` | Habitat real electrical load demand | NO |
| **W090** | `L87_HabitatEmergencyLightingLoad` (`P096`) | `L87_HabitatPowerDistributionPLC` (`P194`) | `SAFETY_CONTROL` | Emergency lighting continuity status | NO |
| **W091** | `L87_HabitatThermalControlLoad` (`P100`) | `L87_HabitatPowerDistributionPLC` (`P198`) | `ANALOG_FEEDBACK` | Thermal circulation and habitat HVAC load | NO |
| **W092** | `L87_HabitatLoadDistributionSimulator` (`P110`) | `L87_HabitatPowerDistributionPLC` (`P202`) | `ANALOG_FEEDBACK` | Habitat cargo/passenger load redistribution | NO |
| **W093** | `L87_HabitatPowerDistributionPLC` (`P190`) | `L87_RotaryPowerTransferMonitor` (`P106`) | `DIGITAL_CONTROL` | Rotary power data acquisition request | NO |
| **W094** | `L87_RotaryPowerTransferMonitor` (`P105`) | `L87_HabitatPowerDistributionPLC` (`P194`) | `ANALOG_FEEDBACK` | Measured rotary power V I P quality | NO |
| **W095** | `L87_OrbitalRingPowerBudgetAnalyzer` (`P117`) | `L87_HabitatPowerDistributionPLC` (`P198`) | `DATA_STREAM` | Predicted load margin and transfer efficiency | NO |
| **W096** | `L87_HabitatPowerDistributionPLC` (`P202`) | `L87_OrbitalRingPowerBudgetAnalyzer` (`P114`) | `DIGITAL_CONTROL` | Power budget recalculation request | NO |
| **W097** | `L87_HabitatPowerDistributionPLC` (`P190`) | `L87_HabitatElectricalLoadEmulator` (`P094`) | `DIGITAL_CONTROL` | Habitat equipment permitted load state | NO |
| **W098** | `L87_HabitatPowerDistributionPLC` (`P194`) | `L87_HabitatEmergencyLightingLoad` (`P098`) | `SAFETY_CONTROL` | Critical habitat lighting permission | NO |
| **W099** | `L87_HabitatPowerDistributionPLC` (`P198`) | `L87_HabitatThermalControlLoad` (`P099`) | `DIGITAL_CONTROL` | Habitat thermal load permission | NO |
| **W100** | `L87_RotaryOpticalTelemetryLink` (`P091`) | `L87_HabitatPowerDistributionPLC` (`P202`) | `DATA_STREAM` | Secondary optical rotary communications link | NO |
| **W101** | `L87_HabitatLoadDistributionSimulator` (`P110`) | `L87_OrbitalRingPowerBudgetAnalyzer` (`P115`) | `DATA_STREAM` | Cargo shift and predicted mission demand | NO |
| **W102** | `L87_GravityHabitatEnvironmentHologram` (`P111`) | `L87_GravityEngineeringMasterSCADA` (`P220`) | `DATA_STREAM` | Artificial gravity live radius and RPM hologram | NO |
| **W103** | `L87_BearingHousingTemperatureSensor` (`P079`) | `L87_BearingPowerAmplifierCabinet` (`P029`) | `ANALOG_FEEDBACK` | Bearing temperature amplifier derating signal | NO |
| **W104** | `L87_BearingPowerAmplifierCabinet` (`P028`) | `L87_GravitySafetySupervisoryPLC` (`P121`) | `ANALOG_FEEDBACK` | Bearing amplifier thermal condition | NO |
| **W105** | `L87_DriveThermalCoolingManifold` (`P054`) | `L87_GravitySafetySupervisoryPLC` (`P125`) | `ANALOG_FEEDBACK` | Drive cooling actual flow and availability | NO |
| **W106** | `L87_HabitatElectricalLoadEmulator` (`P094`) | `L87_OrbitalRingPowerBudgetAnalyzer` (`P116`) | `ANALOG_FEEDBACK` | Habitat essential equipment electrical demand | NO |
| **W107** | `L87_HabitatEmergencyLightingLoad` (`P098`) | `L87_OrbitalRingPowerBudgetAnalyzer` (`P113`) | `ANALOG_FEEDBACK` | Emergency lighting branch power demand | NO |
| **W108** | `L87_HabitatThermalControlLoad` (`P099`) | `L87_OrbitalRingPowerBudgetAnalyzer` (`P117`) | `ANALOG_FEEDBACK` | Habitat thermal controller measured demand | NO |
| **W109** | `L87_HabitatLoadDistributionSimulator` (`P109`) | `L87_RotorMassDistributionScanner` (`P074`) | `DIGITAL_CONTROL` | Dynamic cargo mass distribution scan request | NO |
| **W110** | `L87_RotorMassDistributionScanner` (`P074`) | `L87_HabitatLoadDistributionSimulator` (`P110`) | `DATA_STREAM` | Measured mass distribution time series | NO |
| **W111** | `L87_OrbitalRingPowerBudgetAnalyzer` (`P116`) | `L87_RotorMotorControlPLC` (`P176`) | `DATA_STREAM` | Motor drive power reserve forecast | NO |
| **W112** | `L87_OrbitalRingPowerBudgetAnalyzer` (`P113`) | `L87_GravitySafetySupervisoryPLC` (`P121`) | `DATA_STREAM` | Thermal and electrical system safety margin | NO |
| **W113** | `L87_RotaryPowerTransferMonitor` (`P107`) | `L87_GravityEngineeringMasterSCADA` (`P217`) | `DATA_STREAM` | Power transfer operational reserve status | NO |
| **W114** | `L87_HabitatThermalControlLoad` (`P099`) | `L87_GravityEngineeringMasterSCADA` (`P221`) | `DATA_STREAM` | Habitat thermal load historical sample | NO |
| **W115** | `L87_HabitatEmergencyLightingLoad` (`P097`) | `L87_GravityEngineeringMasterSCADA` (`P225`) | `DATA_STREAM` | Critical habitat lighting status | NO |
| **W116** | `L87_BearingPowerAmplifierCabinet` (`P028`) | `L87_GravityEngineeringMasterSCADA` (`P212`) | `DATA_STREAM` | Amplifier current and temperature SCADA telemetry | NO |
| **W117** | `L87_ContactlessRotaryPowerCouplerPrimary` (`P088`) | `L87_RotaryPowerTransferMonitor` (`P104`) | `ANALOG_FEEDBACK` | Primary coupler output energy measurement | NO |
| **W118** | `L87_ContactlessRotaryPowerCouplerBackup` (`P089`) | `L87_RotaryPowerTransferMonitor` (`P103`) | `ANALOG_FEEDBACK` | Backup coupler output energy measurement | NO |
| **W119** | `L87_RotaryPowerIsolationContactor` (`P093`) | `L87_RotaryPowerTransferMonitor` (`P107`) | `DIGITAL_STATUS` | Actual contactor transmission connection | NO |
| **W120** | `L87_GravityHabitatEnvironmentHologram` (`P112`) | `L87_GravityEngineeringMasterSCADA` (`P219`) | `DATA_STREAM` | Gravity field radial display update | NO |
| **W121** | `L87_OrbitalRingPowerBudgetAnalyzer` (`P118`) | `L87_GravityMachineEventRecorder` (`P235`) | `DATA_STREAM` | Power thermal mission interval certificate | NO |
| **W122** | `L87_RotorMassDistributionScanner` (`P077`) | `L87_GravityMachineEventRecorder` (`P234`) | `DATA_STREAM` | Load profile and mass perturbation event evidence | NO |
| **W123** | `L87_RingEmergencyStopStation` (`P144`) | `L87_GravitySafetySupervisoryPLC` (`P121`) | `SAFETY_CONTROL` | Emergency stop physical wired safety status | NO |
| **W124** | `L87_StationSafetyIsolationRelay` (`P236`) | `L87_GravitySafetySupervisoryPLC` (`P125`) | `SAFETY_CONTROL` | Independent isolation relay operational state | NO |
| **W125** | `L87_IndependentOverspeedProtector` (`P143`) | `L87_GravitySafetySupervisoryPLC` (`P129`) | `SAFETY_CONTROL` | Independent overspeed status | NO |
| **W126** | `L87_RotorSpeedOpticalEncoder` (`P047`) | `L87_IndependentOverspeedProtector` (`P141`) | `ANALOG_FEEDBACK` | Independent angular speed sample | NO |
| **W127** | `L87_GravitySafetySupervisoryPLC` (`P120`) | `L87_RotorMotorControlPLC` (`P182`) | `SAFETY_CONTROL` | Main drive safety permission | NO |
| **W128** | `L87_GravitySafetySupervisoryPLC` (`P124`) | `L87_MagneticBearingControlPLC` (`P165`) | `SAFETY_CONTROL` | Magnetic bearing stabilization safety permit | NO |
| **W129** | `L87_GravitySafetySupervisoryPLC` (`P128`) | `L87_HabitatPowerDistributionPLC` (`P202`) | `SAFETY_CONTROL` | Rotary power safety isolation authorization | NO |
| **W130** | `L87_MagneticBearingSafetyPLC` (`P137`) | `L87_MagneticBearingControlPLC` (`P170`) | `SAFETY_CONTROL` | Bearing safe-mode command and measured readiness | NO |
| **W131** | `L87_RadialSensorDriftFaultInjector` (`P205`) | `L87_PrimaryRadialGapSensor` (`P019`) | `DIGITAL_CONTROL` | Injected primary radial sensor drift bias | NO |
| **W132** | `L87_BearingAmplifierHeatFaultInjector` (`P206`) | `L87_BearingPowerAmplifierCabinet` (`P027`) | `DIGITAL_CONTROL` | Injected bearing amplifier thermal fault | NO |
| **W133** | `L87_MassImbalanceFaultInjector` (`P207`) | `L87_RotorMassDistributionScanner` (`P075`) | `DIGITAL_CONTROL` | Injected rotating cargo imbalance | NO |
| **W134** | `L87_MagneticBearingSafetyPLC` (`P137`) | `L87_EmergencyTouchdownBearing` (`P025`) | `SAFETY_CONTROL` | Touchdown support protective engagement | NO |
| **W135** | `L87_SecondaryRadialGapSensor` (`P020`) | `L87_MagneticBearingSafetyPLC` (`P134`) | `ANALOG_FEEDBACK` | Redundant sensor cross-check diagnostic | NO |
| **W136** | `L87_W137CriticalFeedbackPatchStation` (`P209`) | `L87_MagneticBearingSafetyPLC` (`P138`) | `DIGITAL_STATUS` | Critical feedback patch continuity monitor ONLY | NO |
| **W137** | `L87_PrimaryRadialGapSensor` (`P019`) | `L87_MagneticBearingSafetyPLC` (`P135`) | `ANALOG_FEEDBACK` | CRITICAL trusted radial displacement feedback W137 | YES |
| **W138** | `L87_MagneticBearingSafetyPLC` (`P139`) | `L87_GravitySafetySupervisoryPLC` (`P126`) | `DIGITAL_STATUS` | Safety critical feedback fault latch status | NO |
| **W139** | `L87_GravitySafetySupervisoryPLC` (`P130`) | `L87_GravityMachineEventRecorder` (`P234`) | `DATA_STREAM` | Safety-critical fault and event sequence record | NO |
| **W140** | `L87_GravitySafetySupervisoryPLC` (`P120`) | `L87_W137CriticalFeedbackPatchStation` (`P210`) | `DIGITAL_STATUS` | W137 recovery safety qualification display | NO |
| **W141** | `L87_GravityMachineEventRecorder` (`P234`) | `L87_GravitySafetySupervisoryPLC` (`P124`) | `SAFETY_CONTROL` | Operator explicit fault reset request | NO |
| **W142** | `L87_MagneticBearingSafetyPLC` (`P139`) | `L87_RingCertificationBeacon` (`P229`) | `DIGITAL_STATUS` | Independent bearing safety final qualified status | NO |
| **W143** | `L87_GravitySafetySupervisoryPLC` (`P129`) | `L87_GravityEngineeringMasterSCADA` (`P211`) | `DATA_STREAM` | Gravity operating state and safety telemetry | NO |
| **W144** | `L87_MagneticBearingControlPLC` (`P165`) | `L87_GravityEngineeringMasterSCADA` (`P215`) | `DATA_STREAM` | Magnetic bearing actual gap currents and quality | NO |
| **W145** | `L87_RotorMotorControlPLC` (`P177`) | `L87_GravityEngineeringMasterSCADA` (`P219`) | `DATA_STREAM` | Motor RPM torque acceleration live status | NO |
| **W146** | `L87_VibrationFrequencySpectrumAnalyzer` (`P063`) | `L87_GravityEngineeringMasterSCADA` (`P223`) | `DATA_STREAM` | Rotor vibration spectrum and phase traces | NO |
| **W147** | `L87_RotaryPowerTransferMonitor` (`P103`) | `L87_GravityEngineeringMasterSCADA` (`P227`) | `DATA_STREAM` | Rotary transfer power and efficiency readout | NO |
| **W148** | `L87_OrbitalRingPowerBudgetAnalyzer` (`P113`) | `L87_GravityEngineeringMasterSCADA` (`P214`) | `DATA_STREAM` | Habitat thermal and power budget | NO |
| **W149** | `L87_MagneticBearingSafetyPLC` (`P135`) | `L87_GravityEngineeringMasterSCADA` (`P218`) | `DIGITAL_STATUS` | W137 trusted feedback timestamp and safety validity | NO |
| **W150** | `L87_IndependentOverspeedProtector` (`P141`) | `L87_GravityEngineeringMasterSCADA` (`P222`) | `DIGITAL_STATUS` | Independent overspeed shutdown status | NO |
| **W151** | `L87_GravityEngineeringMasterSCADA` (`P226`) | `L87_GravityMachineEventRecorder` (`P232`) | `DATA_STREAM` | Mission-wide qualified event and run recording | NO |
| **W152** | `L87_GravityMachineEventRecorder` (`P231`) | `L87_GravitySafetySupervisoryPLC` (`P127`) | `DATA_STREAM` | Fresh full qualification run evidence fingerprint | NO |
| **W153** | `L87_GravitySafetySupervisoryPLC` (`P131`) | `L87_RingCertificationBeacon` (`P228`) | `DIGITAL_STATUS` | Final mission permission after all safety passes | NO |
| **W154** | `L87_RingCertificationBeacon` (`P229`) | `L87_GravityEngineeringMasterSCADA` (`P215`) | `DIGITAL_STATUS` | Certification beacon actual status telemetry | NO |
| **W155** | `L87_W137CriticalFeedbackPatchStation` (`P210`) | `L87_GravityEngineeringMasterSCADA` (`P219`) | `DATA_STREAM` | W137 display patch diagnostic only, no bypass | NO |
| **W156** | `L87_DynamicRotorHealthRecorder` (`P084`) | `L87_GravityEngineeringMasterSCADA` (`P223`) | `DATA_STREAM` | Rotor recorder health and sample timeline | NO |
| **W157** | `L87_GravityHabitatEnvironmentHologram` (`P112`) | `L87_GravityEngineeringMasterSCADA` (`P227`) | `DATA_STREAM` | Gravity digital twin radial field visualization | NO |
| **W158** | `L87_GravityEngineeringMasterSCADA` (`P214`) | `L87_RingCertificationBeacon` (`P230`) | `DATA_STREAM` | Six-authoritative-objective status display | NO |

## ANNEX D — ALL 236 ORIGINAL DECORATIVE SOCKET ANCHORS

These are visual art records, **not** `SparkTerminal` components. Preserve original IDs and owner models; if further contacts are needed, create named subports instead of overwriting the `SOCKET_P###` mesh.

| AnchorID | Model | WorldX_m | WorldY_m | WorldZ_m | Status |
|---|---|---|---|---|---|
| `P001` | `L87_MagneticBearingStationA` | 5.51 | 7.061 | -6.44 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P002` | `L87_MagneticBearingStationA` | 5.595 | 7.061 | -6.44 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P003` | `L87_MagneticBearingStationB` | 3.79657 | 11.19757 | -6.44 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P004` | `L87_MagneticBearingStationB` | 3.88157 | 11.19757 | -6.44 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P005` | `L87_MagneticBearingStationC` | -0.34 | 12.911 | -6.44 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P006` | `L87_MagneticBearingStationC` | -0.255 | 12.911 | -6.44 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P007` | `L87_MagneticBearingStationD` | -4.47657 | 11.19757 | -6.44 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P008` | `L87_MagneticBearingStationD` | -4.39157 | 11.19757 | -6.44 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P009` | `L87_MagneticBearingStationE` | -6.19 | 7.061 | -6.44 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P010` | `L87_MagneticBearingStationE` | -6.105 | 7.061 | -6.44 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P011` | `L87_MagneticBearingStationF` | -4.47657 | 2.92443 | -6.44 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P012` | `L87_MagneticBearingStationF` | -4.39157 | 2.92443 | -6.44 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P013` | `L87_MagneticBearingStationG` | -0.34 | 1.211 | -6.44 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P014` | `L87_MagneticBearingStationG` | -0.255 | 1.211 | -6.44 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P015` | `L87_MagneticBearingStationH` | 3.79657 | 2.92443 | -6.44 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P016` | `L87_MagneticBearingStationH` | 3.88157 | 2.92443 | -6.44 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P017` | `L87_PrimaryRadialGapSensor` | -12.72 | 1.561 | 2.6 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P018` | `L87_PrimaryRadialGapSensor` | -12.615 | 1.561 | 2.6 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P019` | `L87_PrimaryRadialGapSensor` | -12.51 | 1.561 | 2.6 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P020` | `L87_SecondaryRadialGapSensor` | -11.42 | 1.561 | 2.6 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P021` | `L87_SecondaryRadialGapSensor` | -11.315 | 1.561 | 2.6 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P022` | `L87_AxialGapDisplacementSensor` | -10.12 | 1.561 | 2.6 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P023` | `L87_MagneticBearingCoilCurrentMonitor` | -8.82 | 1.561 | 2.6 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P024` | `L87_MagneticFluxVisualizationHologram` | -2.52 | 7.311 | -3.7 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P025` | `L87_EmergencyTouchdownBearing` | -12.12 | 4.011 | -5.2 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P026` | `L87_EmergencyTouchdownBearing` | -12.015 | 4.011 | -5.2 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P027` | `L87_BearingPowerAmplifierCabinet` | -7.52 | 1.611 | 2.6 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P028` | `L87_BearingPowerAmplifierCabinet` | -7.415 | 1.611 | 2.6 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P029` | `L87_BearingPowerAmplifierCabinet` | -7.31 | 1.611 | 2.6 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P030` | `L87_BearingPowerAmplifierCabinet` | -7.205 | 1.611 | 2.6 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P031` | `L87_BearingPowerAmplifierCabinet` | -7.1 | 1.611 | 2.6 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P032` | `L87_BearingGapCalibrationStage` | -6.22 | 1.611 | 2.6 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P033` | `L87_SegmentedRimMotorStator` | -0.42 | 7.061 | -6.69 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P034` | `L87_SegmentedRimMotorStator` | -0.315 | 7.061 | -6.69 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P035` | `L87_RimDriveRotorMagnetArray` | -0.42 | 7.061 | -6.99 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P036` | `L87_MotorInverterDriveA` | -4.92 | 1.561 | 2.6 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P037` | `L87_MotorInverterDriveA` | -4.815 | 1.561 | 2.6 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P038` | `L87_MotorInverterDriveA` | -4.71 | 1.561 | 2.6 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P039` | `L87_MotorInverterDriveA` | -4.605 | 1.561 | 2.6 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P040` | `L87_MotorInverterDriveB` | -3.62 | 1.561 | 2.6 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P041` | `L87_MotorInverterDriveB` | -3.515 | 1.561 | 2.6 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P042` | `L87_MotorInverterDriveB` | -3.41 | 1.561 | 2.6 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P043` | `L87_MotorInverterDriveB` | -3.305 | 1.561 | 2.6 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P044` | `L87_MotorPhaseCurrentSensor` | -2.32 | 1.561 | 2.6 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P045` | `L87_MotorPhaseCurrentSensor` | -2.215 | 1.561 | 2.6 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P046` | `L87_MotorTorqueEstimator` | -1.02 | 1.561 | 2.6 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P047` | `L87_RotorSpeedOpticalEncoder` | 0.28 | 1.561 | 2.6 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P048` | `L87_RotorSpeedOpticalEncoder` | 0.385 | 1.561 | 2.6 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P049` | `L87_RotorSpeedOpticalEncoder` | 0.49 | 1.561 | 2.6 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P050` | `L87_SafeSpinUpSequencer` | 1.58 | 1.561 | 2.6 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P051` | `L87_SafeSpinUpSequencer` | 1.685 | 1.561 | 2.6 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P052` | `L87_MotorPrechargeContactor` | 2.88 | 1.561 | 2.6 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P053` | `L87_DriveThermalCoolingManifold` | 4.18 | 1.561 | 2.6 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P054` | `L87_DriveThermalCoolingManifold` | 4.285 | 1.561 | 2.6 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P055` | `L87_MotorAccelerationProfileConsole` | 5.48 | 1.561 | 2.6 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P056` | `L87_RingAngularMomentumCompensationUnit` | 6.78 | 1.561 | 2.6 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P057` | `L87_MultiAxisVibrationSensorArray` | -12.72 | 1.561 | 5.3 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P058` | `L87_VibrationFrequencySpectrumAnalyzer` | -11.42 | 1.561 | 5.3 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P059` | `L87_VibrationFrequencySpectrumAnalyzer` | -11.315 | 1.561 | 5.3 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P060` | `L87_VibrationFrequencySpectrumAnalyzer` | -11.21 | 1.561 | 5.3 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P061` | `L87_VibrationFrequencySpectrumAnalyzer` | -11.105 | 1.561 | 5.3 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P062` | `L87_VibrationFrequencySpectrumAnalyzer` | -11.0 | 1.561 | 5.3 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P063` | `L87_VibrationFrequencySpectrumAnalyzer` | -10.895 | 1.561 | 5.3 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P064` | `L87_RotorShaftOrbitProbe` | -10.12 | 1.561 | 5.3 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P065` | `L87_RotorImbalanceVectorDisplay` | -8.82 | 1.561 | 5.3 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P066` | `L87_RotorImbalanceVectorDisplay` | -8.715 | 1.561 | 5.3 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P067` | `L87_RotorImbalanceVectorDisplay` | -8.61 | 1.561 | 5.3 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P068` | `L87_RotorImbalanceVectorDisplay` | -8.505 | 1.561 | 5.3 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P069` | `L87_AdaptiveVibrationDampingController` | -7.52 | 1.561 | 5.3 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P070` | `L87_AdaptiveVibrationDampingController` | -7.415 | 1.561 | 5.3 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P071` | `L87_AdaptiveVibrationDampingController` | -7.31 | 1.561 | 5.3 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P072` | `L87_PrecisionBalanceWeightActuatorA` | -6.92 | 7.311 | -4.59 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P073` | `L87_PrecisionBalanceWeightActuatorB` | 6.08 | 7.311 | -4.59 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P074` | `L87_RotorMassDistributionScanner` | -6.22 | 1.561 | 5.3 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P075` | `L87_RotorMassDistributionScanner` | -6.115 | 1.561 | 5.3 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P076` | `L87_RotorMassDistributionScanner` | -6.01 | 1.561 | 5.3 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P077` | `L87_RotorMassDistributionScanner` | -5.905 | 1.561 | 5.3 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P078` | `L87_ResonanceSweepTestFixture` | -4.92 | 1.561 | 5.3 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P079` | `L87_BearingHousingTemperatureSensor` | -3.62 | 1.561 | 5.3 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P080` | `L87_DynamicRotorHealthRecorder` | -2.32 | 1.561 | 5.3 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P081` | `L87_DynamicRotorHealthRecorder` | -2.215 | 1.561 | 5.3 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P082` | `L87_DynamicRotorHealthRecorder` | -2.11 | 1.561 | 5.3 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P083` | `L87_DynamicRotorHealthRecorder` | -2.005 | 1.561 | 5.3 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P084` | `L87_DynamicRotorHealthRecorder` | -1.9 | 1.561 | 5.3 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P085` | `L87_PrecisionGapAndVibrationSCADA` | -1.02 | 1.561 | 5.3 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P086` | `L87_VibrationFaultCalibrationInjector` | 0.28 | 1.561 | 5.3 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P087` | `L87_ContactlessRotaryPowerCouplerPrimary` | 7.93 | 2.661 | 1.91 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P088` | `L87_ContactlessRotaryPowerCouplerPrimary` | 8.035 | 2.661 | 1.91 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P089` | `L87_ContactlessRotaryPowerCouplerBackup` | 9.28 | 2.661 | 1.91 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P090` | `L87_ContactlessRotaryPowerCouplerBackup` | 9.385 | 2.661 | 1.91 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P091` | `L87_RotaryOpticalTelemetryLink` | 10.63 | 2.661 | 2.1 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P092` | `L87_RotaryPowerIsolationContactor` | 11.98 | 2.661 | 2.1 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P093` | `L87_RotaryPowerIsolationContactor` | 12.085 | 2.661 | 2.1 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P094` | `L87_HabitatElectricalLoadEmulator` | 7.93 | 1.511 | 5.05 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P095` | `L87_HabitatElectricalLoadEmulator` | 8.035 | 1.511 | 5.05 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P096` | `L87_HabitatEmergencyLightingLoad` | 9.28 | 1.511 | 5.05 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P097` | `L87_HabitatEmergencyLightingLoad` | 9.385 | 1.511 | 5.05 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P098` | `L87_HabitatEmergencyLightingLoad` | 9.49 | 1.511 | 5.05 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P099` | `L87_HabitatThermalControlLoad` | 10.63 | 1.511 | 5.05 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P100` | `L87_HabitatThermalControlLoad` | 10.735 | 1.511 | 5.05 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P101` | `L87_HabitatThermalControlLoad` | 10.84 | 1.511 | 5.05 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P102` | `L87_RotatingDataQualityMonitor` | 11.98 | 1.511 | 5.05 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P103` | `L87_RotaryPowerTransferMonitor` | 7.93 | 3.761 | 5.05 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P104` | `L87_RotaryPowerTransferMonitor` | 8.035 | 3.761 | 5.05 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P105` | `L87_RotaryPowerTransferMonitor` | 8.14 | 3.761 | 5.05 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P106` | `L87_RotaryPowerTransferMonitor` | 8.245 | 3.761 | 5.05 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P107` | `L87_RotaryPowerTransferMonitor` | 8.35 | 3.761 | 5.05 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P108` | `L87_HabitatLoadDistributionSimulator` | 9.28 | 3.761 | 5.05 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P109` | `L87_HabitatLoadDistributionSimulator` | 9.385 | 3.761 | 5.05 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P110` | `L87_HabitatLoadDistributionSimulator` | 9.49 | 3.761 | 5.05 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P111` | `L87_GravityHabitatEnvironmentHologram` | 1.98 | 8.311 | -3.3 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P112` | `L87_GravityHabitatEnvironmentHologram` | 2.085 | 8.311 | -3.3 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P113` | `L87_OrbitalRingPowerBudgetAnalyzer` | 10.63 | 3.761 | 5.05 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P114` | `L87_OrbitalRingPowerBudgetAnalyzer` | 10.735 | 3.761 | 5.05 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P115` | `L87_OrbitalRingPowerBudgetAnalyzer` | 10.84 | 3.761 | 5.05 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P116` | `L87_OrbitalRingPowerBudgetAnalyzer` | 10.945 | 3.761 | 5.05 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P117` | `L87_OrbitalRingPowerBudgetAnalyzer` | 11.05 | 3.761 | 5.05 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P118` | `L87_OrbitalRingPowerBudgetAnalyzer` | 11.155 | 3.761 | 5.05 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P119` | `L87_OrbitalRingPowerBudgetAnalyzer` | 11.26 | 3.761 | 5.05 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P120` | `L87_GravitySafetySupervisoryPLC` | -12.12 | 3.561 | 7.6 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P121` | `L87_GravitySafetySupervisoryPLC` | -12.015 | 3.561 | 7.6 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P122` | `L87_GravitySafetySupervisoryPLC` | -11.91 | 3.561 | 7.6 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P123` | `L87_GravitySafetySupervisoryPLC` | -11.805 | 3.561 | 7.6 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P124` | `L87_GravitySafetySupervisoryPLC` | -11.7 | 3.561 | 7.6 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P125` | `L87_GravitySafetySupervisoryPLC` | -11.595 | 3.561 | 7.6 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P126` | `L87_GravitySafetySupervisoryPLC` | -11.49 | 3.561 | 7.6 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P127` | `L87_GravitySafetySupervisoryPLC` | -11.385 | 3.561 | 7.6 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P128` | `L87_GravitySafetySupervisoryPLC` | -11.28 | 3.561 | 7.6 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P129` | `L87_GravitySafetySupervisoryPLC` | -12.12 | 3.666 | 7.6 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P130` | `L87_GravitySafetySupervisoryPLC` | -12.015 | 3.666 | 7.6 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P131` | `L87_GravitySafetySupervisoryPLC` | -11.91 | 3.666 | 7.6 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P132` | `L87_GravitySafetySupervisoryPLC` | -11.805 | 3.666 | 7.6 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P133` | `L87_GravitySafetySupervisoryPLC` | -11.7 | 3.666 | 7.6 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P134` | `L87_MagneticBearingSafetyPLC` | -10.72 | 3.561 | 7.6 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P135` | `L87_MagneticBearingSafetyPLC` | -10.615 | 3.561 | 7.6 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P136` | `L87_MagneticBearingSafetyPLC` | -10.51 | 3.561 | 7.6 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P137` | `L87_MagneticBearingSafetyPLC` | -10.405 | 3.561 | 7.6 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P138` | `L87_MagneticBearingSafetyPLC` | -10.3 | 3.561 | 7.6 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P139` | `L87_MagneticBearingSafetyPLC` | -10.195 | 3.561 | 7.6 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P140` | `L87_MagneticBearingSafetyPLC` | -10.09 | 3.561 | 7.6 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P141` | `L87_IndependentOverspeedProtector` | -9.32 | 3.561 | 7.6 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P142` | `L87_IndependentOverspeedProtector` | -9.215 | 3.561 | 7.6 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P143` | `L87_IndependentOverspeedProtector` | -9.11 | 3.561 | 7.6 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P144` | `L87_RingEmergencyStopStation` | -7.92 | 3.561 | 7.6 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P145` | `L87_Protected5VControlSupply` | -6.52 | 3.561 | 7.6 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P146` | `L87_Protected5VControlSupply` | -6.415 | 3.561 | 7.6 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P147` | `L87_Protected5VControlSupply` | -6.31 | 3.561 | 7.6 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P148` | `L87_Protected5VControlSupply` | -6.205 | 3.561 | 7.6 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P149` | `L87_Protected5VControlSupply` | -6.1 | 3.561 | 7.6 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P150` | `L87_Protected5VControlSupply` | -5.995 | 3.561 | 7.6 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P151` | `L87_Protected5VControlSupply` | -5.89 | 3.561 | 7.6 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P152` | `L87_Protected5VControlSupply` | -5.785 | 3.561 | 7.6 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P153` | `L87_Protected5VControlSupply` | -5.68 | 3.561 | 7.6 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P154` | `L87_MagneticBearingControlPLC` | -5.12 | 3.561 | 7.6 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P155` | `L87_MagneticBearingControlPLC` | -5.015 | 3.561 | 7.6 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P156` | `L87_MagneticBearingControlPLC` | -4.91 | 3.561 | 7.6 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P157` | `L87_MagneticBearingControlPLC` | -4.805 | 3.561 | 7.6 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P158` | `L87_MagneticBearingControlPLC` | -4.7 | 3.561 | 7.6 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P159` | `L87_MagneticBearingControlPLC` | -4.595 | 3.561 | 7.6 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P160` | `L87_MagneticBearingControlPLC` | -4.49 | 3.561 | 7.6 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P161` | `L87_MagneticBearingControlPLC` | -4.385 | 3.561 | 7.6 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P162` | `L87_MagneticBearingControlPLC` | -4.28 | 3.561 | 7.6 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P163` | `L87_MagneticBearingControlPLC` | -5.12 | 3.666 | 7.6 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P164` | `L87_MagneticBearingControlPLC` | -5.015 | 3.666 | 7.6 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P165` | `L87_MagneticBearingControlPLC` | -4.91 | 3.666 | 7.6 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P166` | `L87_MagneticBearingControlPLC` | -4.805 | 3.666 | 7.6 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P167` | `L87_MagneticBearingControlPLC` | -4.7 | 3.666 | 7.6 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P168` | `L87_MagneticBearingControlPLC` | -4.595 | 3.666 | 7.6 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P169` | `L87_MagneticBearingControlPLC` | -4.49 | 3.666 | 7.6 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P170` | `L87_MagneticBearingControlPLC` | -4.385 | 3.666 | 7.6 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P171` | `L87_MagneticBearingControlPLC` | -4.28 | 3.666 | 7.6 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P172` | `L87_MagneticBearingControlPLC` | -5.12 | 3.771 | 7.6 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P173` | `L87_MagneticBearingControlPLC` | -5.015 | 3.771 | 7.6 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P174` | `L87_MagneticBearingControlPLC` | -4.91 | 3.771 | 7.6 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P175` | `L87_MagneticBearingControlPLC` | -4.805 | 3.771 | 7.6 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P176` | `L87_RotorMotorControlPLC` | -3.72 | 3.561 | 7.6 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P177` | `L87_RotorMotorControlPLC` | -3.615 | 3.561 | 7.6 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P178` | `L87_RotorMotorControlPLC` | -3.51 | 3.561 | 7.6 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P179` | `L87_RotorMotorControlPLC` | -3.405 | 3.561 | 7.6 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P180` | `L87_RotorMotorControlPLC` | -3.3 | 3.561 | 7.6 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P181` | `L87_RotorMotorControlPLC` | -3.195 | 3.561 | 7.6 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P182` | `L87_RotorMotorControlPLC` | -3.09 | 3.561 | 7.6 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P183` | `L87_RotorMotorControlPLC` | -2.985 | 3.561 | 7.6 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P184` | `L87_RotorMotorControlPLC` | -2.88 | 3.561 | 7.6 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P185` | `L87_RotorMotorControlPLC` | -3.72 | 3.666 | 7.6 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P186` | `L87_RotorMotorControlPLC` | -3.615 | 3.666 | 7.6 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P187` | `L87_RotorMotorControlPLC` | -3.51 | 3.666 | 7.6 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P188` | `L87_RotorMotorControlPLC` | -3.405 | 3.666 | 7.6 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P189` | `L87_HabitatPowerDistributionPLC` | -2.32 | 3.561 | 7.6 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P190` | `L87_HabitatPowerDistributionPLC` | -2.215 | 3.561 | 7.6 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P191` | `L87_HabitatPowerDistributionPLC` | -2.11 | 3.561 | 7.6 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P192` | `L87_HabitatPowerDistributionPLC` | -2.005 | 3.561 | 7.6 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P193` | `L87_HabitatPowerDistributionPLC` | -1.9 | 3.561 | 7.6 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P194` | `L87_HabitatPowerDistributionPLC` | -1.795 | 3.561 | 7.6 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P195` | `L87_HabitatPowerDistributionPLC` | -1.69 | 3.561 | 7.6 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P196` | `L87_HabitatPowerDistributionPLC` | -1.585 | 3.561 | 7.6 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P197` | `L87_HabitatPowerDistributionPLC` | -1.48 | 3.561 | 7.6 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P198` | `L87_HabitatPowerDistributionPLC` | -2.32 | 3.666 | 7.6 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P199` | `L87_HabitatPowerDistributionPLC` | -2.215 | 3.666 | 7.6 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P200` | `L87_HabitatPowerDistributionPLC` | -2.11 | 3.666 | 7.6 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P201` | `L87_HabitatPowerDistributionPLC` | -2.005 | 3.666 | 7.6 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P202` | `L87_HabitatPowerDistributionPLC` | -1.9 | 3.666 | 7.6 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P203` | `L87_HabitatPowerDistributionPLC` | -1.795 | 3.666 | 7.6 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P204` | `L87_HabitatPowerDistributionPLC` | -1.69 | 3.666 | 7.6 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P205` | `L87_RadialSensorDriftFaultInjector` | -0.92 | 3.561 | 7.6 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P206` | `L87_BearingAmplifierHeatFaultInjector` | 0.48 | 3.561 | 7.6 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P207` | `L87_MassImbalanceFaultInjector` | 1.88 | 3.561 | 7.6 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P208` | `L87_W137CriticalFeedbackPatchStation` | 3.28 | 3.561 | 7.6 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P209` | `L87_W137CriticalFeedbackPatchStation` | 3.385 | 3.561 | 7.6 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P210` | `L87_W137CriticalFeedbackPatchStation` | 3.49 | 3.561 | 7.6 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P211` | `L87_GravityEngineeringMasterSCADA` | -0.42 | 11.611 | -8.3 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P212` | `L87_GravityEngineeringMasterSCADA` | -0.315 | 11.611 | -8.3 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P213` | `L87_GravityEngineeringMasterSCADA` | -0.21 | 11.611 | -8.3 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P214` | `L87_GravityEngineeringMasterSCADA` | -0.105 | 11.611 | -8.3 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P215` | `L87_GravityEngineeringMasterSCADA` | 0.0 | 11.611 | -8.3 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P216` | `L87_GravityEngineeringMasterSCADA` | 0.105 | 11.611 | -8.3 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P217` | `L87_GravityEngineeringMasterSCADA` | 0.21 | 11.611 | -8.3 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P218` | `L87_GravityEngineeringMasterSCADA` | 0.315 | 11.611 | -8.3 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P219` | `L87_GravityEngineeringMasterSCADA` | 0.42 | 11.611 | -8.3 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P220` | `L87_GravityEngineeringMasterSCADA` | -0.42 | 11.716 | -8.3 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P221` | `L87_GravityEngineeringMasterSCADA` | -0.315 | 11.716 | -8.3 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P222` | `L87_GravityEngineeringMasterSCADA` | -0.21 | 11.716 | -8.3 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P223` | `L87_GravityEngineeringMasterSCADA` | -0.105 | 11.716 | -8.3 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P224` | `L87_GravityEngineeringMasterSCADA` | 0.0 | 11.716 | -8.3 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P225` | `L87_GravityEngineeringMasterSCADA` | 0.105 | 11.716 | -8.3 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P226` | `L87_GravityEngineeringMasterSCADA` | 0.21 | 11.716 | -8.3 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P227` | `L87_GravityEngineeringMasterSCADA` | 0.315 | 11.716 | -8.3 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P228` | `L87_RingCertificationBeacon` | -0.42 | 12.561 | -7.6 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P229` | `L87_RingCertificationBeacon` | -0.315 | 12.561 | -7.6 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P230` | `L87_RingCertificationBeacon` | -0.21 | 12.561 | -7.6 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P231` | `L87_GravityMachineEventRecorder` | 4.68 | 3.561 | 7.6 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P232` | `L87_GravityMachineEventRecorder` | 4.785 | 3.561 | 7.6 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P233` | `L87_GravityMachineEventRecorder` | 4.89 | 3.561 | 7.6 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P234` | `L87_GravityMachineEventRecorder` | 4.995 | 3.561 | 7.6 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P235` | `L87_GravityMachineEventRecorder` | 5.1 | 3.561 | 7.6 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P236` | `L87_StationSafetyIsolationRelay` | 6.08 | 3.561 | 7.6 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |

## ANNEX E — ALL 316 PROPOSED LOGICAL WIRE ENDPOINT CONTACTS

Each contact is a **proposed functional interface**, not a preexisting Unity `SparkTerminal`. Many contacts share a single visual anchor location; they may not share the same electrical net without proven compatibility.

| WID | End | Model | Visual socket | Proposed FunctionalPortID | Kind | Direction |
|---|---|---|---|---|---|---|
| W001 | Source | `L87_Protected5VControlSupply` | `P146` | `W001_OUT_POWER_CONTROL` | `POWER_CONTROL` | OUTPUT |
| W001 | Target | `L87_GravitySafetySupervisoryPLC` | `P121` | `W001_IN_POWER_CONTROL` | `POWER_CONTROL` | INPUT |
| W002 | Source | `L87_Protected5VControlSupply` | `P150` | `W002_OUT_POWER_CONTROL` | `POWER_CONTROL` | OUTPUT |
| W002 | Target | `L87_MagneticBearingSafetyPLC` | `P136` | `W002_IN_POWER_CONTROL` | `POWER_CONTROL` | INPUT |
| W003 | Source | `L87_Protected5VControlSupply` | `P145` | `W003_OUT_POWER_CONTROL` | `POWER_CONTROL` | OUTPUT |
| W003 | Target | `L87_MagneticBearingControlPLC` | `P157` | `W003_IN_POWER_CONTROL` | `POWER_CONTROL` | INPUT |
| W004 | Source | `L87_Protected5VControlSupply` | `P149` | `W004_OUT_POWER_CONTROL` | `POWER_CONTROL` | OUTPUT |
| W004 | Target | `L87_RotorMotorControlPLC` | `P180` | `W004_IN_POWER_CONTROL` | `POWER_CONTROL` | INPUT |
| W005 | Source | `L87_Protected5VControlSupply` | `P153` | `W005_OUT_POWER_CONTROL` | `POWER_CONTROL` | OUTPUT |
| W005 | Target | `L87_HabitatPowerDistributionPLC` | `P194` | `W005_IN_POWER_CONTROL` | `POWER_CONTROL` | INPUT |
| W006 | Source | `L87_Protected5VControlSupply` | `P148` | `W006_OUT_POWER_CONTROL` | `POWER_CONTROL` | OUTPUT |
| W006 | Target | `L87_GravityEngineeringMasterSCADA` | `P217` | `W006_IN_POWER_CONTROL` | `POWER_CONTROL` | INPUT |
| W007 | Source | `L87_Protected5VControlSupply` | `P152` | `W007_OUT_POWER_CONTROL` | `POWER_CONTROL` | OUTPUT |
| W007 | Target | `L87_IndependentOverspeedProtector` | `P142` | `W007_IN_POWER_CONTROL` | `POWER_CONTROL` | INPUT |
| W008 | Source | `L87_Protected5VControlSupply` | `P147` | `W008_OUT_POWER_CONTROL` | `POWER_CONTROL` | OUTPUT |
| W008 | Target | `L87_BearingPowerAmplifierCabinet` | `P030` | `W008_IN_POWER_CONTROL` | `POWER_CONTROL` | INPUT |
| W009 | Source | `L87_Protected5VControlSupply` | `P151` | `W009_OUT_POWER_CONTROL` | `POWER_CONTROL` | OUTPUT |
| W009 | Target | `L87_MotorInverterDriveA` | `P037` | `W009_IN_POWER_CONTROL` | `POWER_CONTROL` | INPUT |
| W010 | Source | `L87_Protected5VControlSupply` | `P146` | `W010_OUT_POWER_CONTROL` | `POWER_CONTROL` | OUTPUT |
| W010 | Target | `L87_MotorInverterDriveB` | `P042` | `W010_IN_POWER_CONTROL` | `POWER_CONTROL` | INPUT |
| W011 | Source | `L87_Protected5VControlSupply` | `P150` | `W011_OUT_POWER_CONTROL` | `POWER_CONTROL` | OUTPUT |
| W011 | Target | `L87_W137CriticalFeedbackPatchStation` | `P210` | `W011_IN_POWER_CONTROL` | `POWER_CONTROL` | INPUT |
| W012 | Source | `L87_Protected5VControlSupply` | `P145` | `W012_OUT_POWER_CONTROL` | `POWER_CONTROL` | OUTPUT |
| W012 | Target | `L87_GravityMachineEventRecorder` | `P233` | `W012_IN_POWER_CONTROL` | `POWER_CONTROL` | INPUT |
| W013 | Source | `L87_MagneticBearingControlPLC` | `P170` | `W013_OUT_ACTUATOR_COMMAND` | `ACTUATOR_COMMAND` | OUTPUT |
| W013 | Target | `L87_MagneticBearingStationA` | `P002` | `W013_IN_ACTUATOR_COMMAND` | `ACTUATOR_COMMAND` | INPUT |
| W014 | Source | `L87_MagneticBearingStationA` | `P002` | `W014_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W014 | Target | `L87_MagneticBearingControlPLC` | `P174` | `W014_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W015 | Source | `L87_MagneticBearingControlPLC` | `P156` | `W015_OUT_ACTUATOR_COMMAND` | `ACTUATOR_COMMAND` | OUTPUT |
| W015 | Target | `L87_MagneticBearingStationB` | `P004` | `W015_IN_ACTUATOR_COMMAND` | `ACTUATOR_COMMAND` | INPUT |
| W016 | Source | `L87_MagneticBearingStationB` | `P004` | `W016_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W016 | Target | `L87_MagneticBearingControlPLC` | `P160` | `W016_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W017 | Source | `L87_MagneticBearingControlPLC` | `P164` | `W017_OUT_ACTUATOR_COMMAND` | `ACTUATOR_COMMAND` | OUTPUT |
| W017 | Target | `L87_MagneticBearingStationC` | `P006` | `W017_IN_ACTUATOR_COMMAND` | `ACTUATOR_COMMAND` | INPUT |
| W018 | Source | `L87_MagneticBearingStationC` | `P006` | `W018_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W018 | Target | `L87_MagneticBearingControlPLC` | `P168` | `W018_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W019 | Source | `L87_MagneticBearingControlPLC` | `P172` | `W019_OUT_ACTUATOR_COMMAND` | `ACTUATOR_COMMAND` | OUTPUT |
| W019 | Target | `L87_MagneticBearingStationD` | `P008` | `W019_IN_ACTUATOR_COMMAND` | `ACTUATOR_COMMAND` | INPUT |
| W020 | Source | `L87_MagneticBearingStationD` | `P008` | `W020_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W020 | Target | `L87_MagneticBearingControlPLC` | `P154` | `W020_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W021 | Source | `L87_MagneticBearingControlPLC` | `P158` | `W021_OUT_ACTUATOR_COMMAND` | `ACTUATOR_COMMAND` | OUTPUT |
| W021 | Target | `L87_MagneticBearingStationE` | `P010` | `W021_IN_ACTUATOR_COMMAND` | `ACTUATOR_COMMAND` | INPUT |
| W022 | Source | `L87_MagneticBearingStationE` | `P010` | `W022_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W022 | Target | `L87_MagneticBearingControlPLC` | `P162` | `W022_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W023 | Source | `L87_MagneticBearingControlPLC` | `P166` | `W023_OUT_ACTUATOR_COMMAND` | `ACTUATOR_COMMAND` | OUTPUT |
| W023 | Target | `L87_MagneticBearingStationF` | `P012` | `W023_IN_ACTUATOR_COMMAND` | `ACTUATOR_COMMAND` | INPUT |
| W024 | Source | `L87_MagneticBearingStationF` | `P012` | `W024_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W024 | Target | `L87_MagneticBearingControlPLC` | `P170` | `W024_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W025 | Source | `L87_MagneticBearingControlPLC` | `P174` | `W025_OUT_ACTUATOR_COMMAND` | `ACTUATOR_COMMAND` | OUTPUT |
| W025 | Target | `L87_MagneticBearingStationG` | `P014` | `W025_IN_ACTUATOR_COMMAND` | `ACTUATOR_COMMAND` | INPUT |
| W026 | Source | `L87_MagneticBearingStationG` | `P014` | `W026_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W026 | Target | `L87_MagneticBearingControlPLC` | `P156` | `W026_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W027 | Source | `L87_MagneticBearingControlPLC` | `P160` | `W027_OUT_ACTUATOR_COMMAND` | `ACTUATOR_COMMAND` | OUTPUT |
| W027 | Target | `L87_MagneticBearingStationH` | `P016` | `W027_IN_ACTUATOR_COMMAND` | `ACTUATOR_COMMAND` | INPUT |
| W028 | Source | `L87_MagneticBearingStationH` | `P016` | `W028_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W028 | Target | `L87_MagneticBearingControlPLC` | `P164` | `W028_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W029 | Source | `L87_PrimaryRadialGapSensor` | `P019` | `W029_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W029 | Target | `L87_MagneticBearingControlPLC` | `P168` | `W029_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W030 | Source | `L87_SecondaryRadialGapSensor` | `P020` | `W030_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W030 | Target | `L87_MagneticBearingControlPLC` | `P172` | `W030_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W031 | Source | `L87_AxialGapDisplacementSensor` | `P022` | `W031_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W031 | Target | `L87_MagneticBearingControlPLC` | `P154` | `W031_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W032 | Source | `L87_MagneticBearingCoilCurrentMonitor` | `P023` | `W032_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W032 | Target | `L87_MagneticBearingControlPLC` | `P158` | `W032_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W033 | Source | `L87_MagneticBearingControlPLC` | `P162` | `W033_OUT_ACTUATOR_COMMAND` | `ACTUATOR_COMMAND` | OUTPUT |
| W033 | Target | `L87_BearingPowerAmplifierCabinet` | `P028` | `W033_IN_ACTUATOR_COMMAND` | `ACTUATOR_COMMAND` | INPUT |
| W034 | Source | `L87_BearingPowerAmplifierCabinet` | `P027` | `W034_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W034 | Target | `L87_MagneticBearingControlPLC` | `P166` | `W034_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W035 | Source | `L87_MagneticBearingControlPLC` | `P170` | `W035_OUT_SAFETY_CONTROL` | `SAFETY_CONTROL` | OUTPUT |
| W035 | Target | `L87_EmergencyTouchdownBearing` | `P026` | `W035_IN_SAFETY_CONTROL` | `SAFETY_CONTROL` | INPUT |
| W036 | Source | `L87_EmergencyTouchdownBearing` | `P026` | `W036_OUT_DIGITAL_STATUS` | `DIGITAL_STATUS` | OUTPUT |
| W036 | Target | `L87_MagneticBearingSafetyPLC` | `P138` | `W036_IN_DIGITAL_STATUS` | `DIGITAL_STATUS` | INPUT |
| W037 | Source | `L87_BearingGapCalibrationStage` | `P032` | `W037_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W037 | Target | `L87_MagneticBearingControlPLC` | `P175` | `W037_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W038 | Source | `L87_MagneticBearingControlPLC` | `P157` | `W038_OUT_DATA_STREAM` | `DATA_STREAM` | OUTPUT |
| W038 | Target | `L87_MagneticFluxVisualizationHologram` | `P024` | `W038_IN_DATA_STREAM` | `DATA_STREAM` | INPUT |
| W039 | Source | `L87_RotorMotorControlPLC` | `P179` | `W039_OUT_ACTUATOR_COMMAND` | `ACTUATOR_COMMAND` | OUTPUT |
| W039 | Target | `L87_MotorInverterDriveA` | `P038` | `W039_IN_ACTUATOR_COMMAND` | `ACTUATOR_COMMAND` | INPUT |
| W040 | Source | `L87_RotorMotorControlPLC` | `P183` | `W040_OUT_ACTUATOR_COMMAND` | `ACTUATOR_COMMAND` | OUTPUT |
| W040 | Target | `L87_MotorInverterDriveB` | `P043` | `W040_IN_ACTUATOR_COMMAND` | `ACTUATOR_COMMAND` | INPUT |
| W041 | Source | `L87_MotorInverterDriveA` | `P039` | `W041_OUT_ACTUATOR_COMMAND` | `ACTUATOR_COMMAND` | OUTPUT |
| W041 | Target | `L87_SegmentedRimMotorStator` | `P034` | `W041_IN_ACTUATOR_COMMAND` | `ACTUATOR_COMMAND` | INPUT |
| W042 | Source | `L87_MotorInverterDriveB` | `P040` | `W042_OUT_ACTUATOR_COMMAND` | `ACTUATOR_COMMAND` | OUTPUT |
| W042 | Target | `L87_SegmentedRimMotorStator` | `P034` | `W042_IN_ACTUATOR_COMMAND` | `ACTUATOR_COMMAND` | INPUT |
| W043 | Source | `L87_RimDriveRotorMagnetArray` | `P035` | `W043_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W043 | Target | `L87_RotorSpeedOpticalEncoder` | `P048` | `W043_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W044 | Source | `L87_RotorSpeedOpticalEncoder` | `P049` | `W044_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W044 | Target | `L87_RotorMotorControlPLC` | `P177` | `W044_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W045 | Source | `L87_MotorPhaseCurrentSensor` | `P045` | `W045_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W045 | Target | `L87_RotorMotorControlPLC` | `P181` | `W045_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W046 | Source | `L87_MotorTorqueEstimator` | `P046` | `W046_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W046 | Target | `L87_RotorMotorControlPLC` | `P185` | `W046_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W047 | Source | `L87_RotorMotorControlPLC` | `P176` | `W047_OUT_ACTUATOR_COMMAND` | `ACTUATOR_COMMAND` | OUTPUT |
| W047 | Target | `L87_MotorPrechargeContactor` | `P052` | `W047_IN_ACTUATOR_COMMAND` | `ACTUATOR_COMMAND` | INPUT |
| W048 | Source | `L87_MotorPrechargeContactor` | `P052` | `W048_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W048 | Target | `L87_RotorMotorControlPLC` | `P180` | `W048_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W049 | Source | `L87_DriveThermalCoolingManifold` | `P054` | `W049_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W049 | Target | `L87_MotorInverterDriveA` | `P038` | `W049_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W050 | Source | `L87_DriveThermalCoolingManifold` | `P054` | `W050_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W050 | Target | `L87_MotorInverterDriveB` | `P043` | `W050_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W051 | Source | `L87_SafeSpinUpSequencer` | `P051` | `W051_OUT_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | OUTPUT |
| W051 | Target | `L87_RotorMotorControlPLC` | `P186` | `W051_IN_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | INPUT |
| W052 | Source | `L87_MotorAccelerationProfileConsole` | `P055` | `W052_OUT_DATA_STREAM` | `DATA_STREAM` | OUTPUT |
| W052 | Target | `L87_RotorMotorControlPLC` | `P177` | `W052_IN_DATA_STREAM` | `DATA_STREAM` | INPUT |
| W053 | Source | `L87_RingAngularMomentumCompensationUnit` | `P056` | `W053_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W053 | Target | `L87_GravitySafetySupervisoryPLC` | `P120` | `W053_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W054 | Source | `L87_RotorMotorControlPLC` | `P182` | `W054_OUT_ACTUATOR_COMMAND` | `ACTUATOR_COMMAND` | OUTPUT |
| W054 | Target | `L87_RingAngularMomentumCompensationUnit` | `P056` | `W054_IN_ACTUATOR_COMMAND` | `ACTUATOR_COMMAND` | INPUT |
| W055 | Source | `L87_MotorInverterDriveA` | `P039` | `W055_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W055 | Target | `L87_MotorPhaseCurrentSensor` | `P044` | `W055_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W056 | Source | `L87_MotorInverterDriveB` | `P040` | `W056_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W056 | Target | `L87_MotorPhaseCurrentSensor` | `P044` | `W056_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W057 | Source | `L87_RotorMotorControlPLC` | `P188` | `W057_OUT_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | OUTPUT |
| W057 | Target | `L87_SafeSpinUpSequencer` | `P050` | `W057_IN_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | INPUT |
| W058 | Source | `L87_RotorSpeedOpticalEncoder` | `P048` | `W058_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W058 | Target | `L87_SafeSpinUpSequencer` | `P050` | `W058_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W059 | Source | `L87_MotorInverterDriveA` | `P038` | `W059_OUT_DIGITAL_STATUS` | `DIGITAL_STATUS` | OUTPUT |
| W059 | Target | `L87_RotorMotorControlPLC` | `P180` | `W059_IN_DIGITAL_STATUS` | `DIGITAL_STATUS` | INPUT |
| W060 | Source | `L87_MotorInverterDriveB` | `P043` | `W060_OUT_DIGITAL_STATUS` | `DIGITAL_STATUS` | OUTPUT |
| W060 | Target | `L87_RotorMotorControlPLC` | `P184` | `W060_IN_DIGITAL_STATUS` | `DIGITAL_STATUS` | INPUT |
| W061 | Source | `L87_MultiAxisVibrationSensorArray` | `P057` | `W061_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W061 | Target | `L87_VibrationFrequencySpectrumAnalyzer` | `P059` | `W061_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W062 | Source | `L87_RotorShaftOrbitProbe` | `P064` | `W062_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W062 | Target | `L87_VibrationFrequencySpectrumAnalyzer` | `P063` | `W062_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W063 | Source | `L87_VibrationFrequencySpectrumAnalyzer` | `P061` | `W063_OUT_DATA_STREAM` | `DATA_STREAM` | OUTPUT |
| W063 | Target | `L87_RotorImbalanceVectorDisplay` | `P068` | `W063_IN_DATA_STREAM` | `DATA_STREAM` | INPUT |
| W064 | Source | `L87_RotorImbalanceVectorDisplay` | `P068` | `W064_OUT_DATA_STREAM` | `DATA_STREAM` | OUTPUT |
| W064 | Target | `L87_AdaptiveVibrationDampingController` | `P070` | `W064_IN_DATA_STREAM` | `DATA_STREAM` | INPUT |
| W065 | Source | `L87_AdaptiveVibrationDampingController` | `P071` | `W065_OUT_ACTUATOR_COMMAND` | `ACTUATOR_COMMAND` | OUTPUT |
| W065 | Target | `L87_MagneticBearingControlPLC` | `P165` | `W065_IN_ACTUATOR_COMMAND` | `ACTUATOR_COMMAND` | INPUT |
| W066 | Source | `L87_AdaptiveVibrationDampingController` | `P069` | `W066_OUT_SAFETY_CONTROL` | `SAFETY_CONTROL` | OUTPUT |
| W066 | Target | `L87_RotorMotorControlPLC` | `P180` | `W066_IN_SAFETY_CONTROL` | `SAFETY_CONTROL` | INPUT |
| W067 | Source | `L87_RotorMassDistributionScanner` | `P077` | `W067_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W067 | Target | `L87_RotorImbalanceVectorDisplay` | `P066` | `W067_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W068 | Source | `L87_ResonanceSweepTestFixture` | `P078` | `W068_OUT_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | OUTPUT |
| W068 | Target | `L87_VibrationFrequencySpectrumAnalyzer` | `P063` | `W068_IN_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | INPUT |
| W069 | Source | `L87_VibrationFrequencySpectrumAnalyzer` | `P061` | `W069_OUT_DATA_STREAM` | `DATA_STREAM` | OUTPUT |
| W069 | Target | `L87_ResonanceSweepTestFixture` | `P078` | `W069_IN_DATA_STREAM` | `DATA_STREAM` | INPUT |
| W070 | Source | `L87_RotorImbalanceVectorDisplay` | `P068` | `W070_OUT_ACTUATOR_COMMAND` | `ACTUATOR_COMMAND` | OUTPUT |
| W070 | Target | `L87_PrecisionBalanceWeightActuatorA` | `P072` | `W070_IN_ACTUATOR_COMMAND` | `ACTUATOR_COMMAND` | INPUT |
| W071 | Source | `L87_RotorImbalanceVectorDisplay` | `P068` | `W071_OUT_ACTUATOR_COMMAND` | `ACTUATOR_COMMAND` | OUTPUT |
| W071 | Target | `L87_PrecisionBalanceWeightActuatorB` | `P073` | `W071_IN_ACTUATOR_COMMAND` | `ACTUATOR_COMMAND` | INPUT |
| W072 | Source | `L87_PrecisionBalanceWeightActuatorA` | `P072` | `W072_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W072 | Target | `L87_DynamicRotorHealthRecorder` | `P082` | `W072_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W073 | Source | `L87_PrecisionBalanceWeightActuatorB` | `P073` | `W073_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W073 | Target | `L87_DynamicRotorHealthRecorder` | `P081` | `W073_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W074 | Source | `L87_BearingHousingTemperatureSensor` | `P079` | `W074_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W074 | Target | `L87_DynamicRotorHealthRecorder` | `P080` | `W074_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W075 | Source | `L87_MultiAxisVibrationSensorArray` | `P057` | `W075_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W075 | Target | `L87_DynamicRotorHealthRecorder` | `P084` | `W075_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W076 | Source | `L87_DynamicRotorHealthRecorder` | `P083` | `W076_OUT_DATA_STREAM` | `DATA_STREAM` | OUTPUT |
| W076 | Target | `L87_PrecisionGapAndVibrationSCADA` | `P085` | `W076_IN_DATA_STREAM` | `DATA_STREAM` | INPUT |
| W077 | Source | `L87_VibrationFaultCalibrationInjector` | `P086` | `W077_OUT_DIGITAL_STATUS` | `DIGITAL_STATUS` | OUTPUT |
| W077 | Target | `L87_VibrationFrequencySpectrumAnalyzer` | `P060` | `W077_IN_DIGITAL_STATUS` | `DIGITAL_STATUS` | INPUT |
| W078 | Source | `L87_VibrationFrequencySpectrumAnalyzer` | `P058` | `W078_OUT_SAFETY_CONTROL` | `SAFETY_CONTROL` | OUTPUT |
| W078 | Target | `L87_GravitySafetySupervisoryPLC` | `P120` | `W078_IN_SAFETY_CONTROL` | `SAFETY_CONTROL` | INPUT |
| W079 | Source | `L87_DynamicRotorHealthRecorder` | `P084` | `W079_OUT_DATA_STREAM` | `DATA_STREAM` | OUTPUT |
| W079 | Target | `L87_GravityEngineeringMasterSCADA` | `P225` | `W079_IN_DATA_STREAM` | `DATA_STREAM` | INPUT |
| W080 | Source | `L87_AdaptiveVibrationDampingController` | `P071` | `W080_OUT_DATA_STREAM` | `DATA_STREAM` | OUTPUT |
| W080 | Target | `L87_GravityEngineeringMasterSCADA` | `P212` | `W080_IN_DATA_STREAM` | `DATA_STREAM` | INPUT |
| W081 | Source | `L87_HabitatPowerDistributionPLC` | `P193` | `W081_OUT_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | OUTPUT |
| W081 | Target | `L87_ContactlessRotaryPowerCouplerPrimary` | `P088` | `W081_IN_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | INPUT |
| W082 | Source | `L87_HabitatPowerDistributionPLC` | `P197` | `W082_OUT_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | OUTPUT |
| W082 | Target | `L87_ContactlessRotaryPowerCouplerBackup` | `P089` | `W082_IN_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | INPUT |
| W083 | Source | `L87_ContactlessRotaryPowerCouplerPrimary` | `P087` | `W083_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W083 | Target | `L87_HabitatPowerDistributionPLC` | `P201` | `W083_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W084 | Source | `L87_ContactlessRotaryPowerCouplerBackup` | `P090` | `W084_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W084 | Target | `L87_HabitatPowerDistributionPLC` | `P189` | `W084_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W085 | Source | `L87_RotaryOpticalTelemetryLink` | `P091` | `W085_OUT_DATA_STREAM` | `DATA_STREAM` | OUTPUT |
| W085 | Target | `L87_RotatingDataQualityMonitor` | `P102` | `W085_IN_DATA_STREAM` | `DATA_STREAM` | INPUT |
| W086 | Source | `L87_RotatingDataQualityMonitor` | `P102` | `W086_OUT_DATA_STREAM` | `DATA_STREAM` | OUTPUT |
| W086 | Target | `L87_HabitatPowerDistributionPLC` | `P194` | `W086_IN_DATA_STREAM` | `DATA_STREAM` | INPUT |
| W087 | Source | `L87_HabitatPowerDistributionPLC` | `P198` | `W087_OUT_ACTUATOR_COMMAND` | `ACTUATOR_COMMAND` | OUTPUT |
| W087 | Target | `L87_RotaryPowerIsolationContactor` | `P093` | `W087_IN_ACTUATOR_COMMAND` | `ACTUATOR_COMMAND` | INPUT |
| W088 | Source | `L87_RotaryPowerIsolationContactor` | `P093` | `W088_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W088 | Target | `L87_HabitatPowerDistributionPLC` | `P202` | `W088_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W089 | Source | `L87_HabitatElectricalLoadEmulator` | `P095` | `W089_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W089 | Target | `L87_HabitatPowerDistributionPLC` | `P190` | `W089_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W090 | Source | `L87_HabitatEmergencyLightingLoad` | `P096` | `W090_OUT_SAFETY_CONTROL` | `SAFETY_CONTROL` | OUTPUT |
| W090 | Target | `L87_HabitatPowerDistributionPLC` | `P194` | `W090_IN_SAFETY_CONTROL` | `SAFETY_CONTROL` | INPUT |
| W091 | Source | `L87_HabitatThermalControlLoad` | `P100` | `W091_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W091 | Target | `L87_HabitatPowerDistributionPLC` | `P198` | `W091_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W092 | Source | `L87_HabitatLoadDistributionSimulator` | `P110` | `W092_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W092 | Target | `L87_HabitatPowerDistributionPLC` | `P202` | `W092_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W093 | Source | `L87_HabitatPowerDistributionPLC` | `P190` | `W093_OUT_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | OUTPUT |
| W093 | Target | `L87_RotaryPowerTransferMonitor` | `P106` | `W093_IN_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | INPUT |
| W094 | Source | `L87_RotaryPowerTransferMonitor` | `P105` | `W094_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W094 | Target | `L87_HabitatPowerDistributionPLC` | `P194` | `W094_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W095 | Source | `L87_OrbitalRingPowerBudgetAnalyzer` | `P117` | `W095_OUT_DATA_STREAM` | `DATA_STREAM` | OUTPUT |
| W095 | Target | `L87_HabitatPowerDistributionPLC` | `P198` | `W095_IN_DATA_STREAM` | `DATA_STREAM` | INPUT |
| W096 | Source | `L87_HabitatPowerDistributionPLC` | `P202` | `W096_OUT_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | OUTPUT |
| W096 | Target | `L87_OrbitalRingPowerBudgetAnalyzer` | `P114` | `W096_IN_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | INPUT |
| W097 | Source | `L87_HabitatPowerDistributionPLC` | `P190` | `W097_OUT_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | OUTPUT |
| W097 | Target | `L87_HabitatElectricalLoadEmulator` | `P094` | `W097_IN_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | INPUT |
| W098 | Source | `L87_HabitatPowerDistributionPLC` | `P194` | `W098_OUT_SAFETY_CONTROL` | `SAFETY_CONTROL` | OUTPUT |
| W098 | Target | `L87_HabitatEmergencyLightingLoad` | `P098` | `W098_IN_SAFETY_CONTROL` | `SAFETY_CONTROL` | INPUT |
| W099 | Source | `L87_HabitatPowerDistributionPLC` | `P198` | `W099_OUT_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | OUTPUT |
| W099 | Target | `L87_HabitatThermalControlLoad` | `P099` | `W099_IN_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | INPUT |
| W100 | Source | `L87_RotaryOpticalTelemetryLink` | `P091` | `W100_OUT_DATA_STREAM` | `DATA_STREAM` | OUTPUT |
| W100 | Target | `L87_HabitatPowerDistributionPLC` | `P202` | `W100_IN_DATA_STREAM` | `DATA_STREAM` | INPUT |
| W101 | Source | `L87_HabitatLoadDistributionSimulator` | `P110` | `W101_OUT_DATA_STREAM` | `DATA_STREAM` | OUTPUT |
| W101 | Target | `L87_OrbitalRingPowerBudgetAnalyzer` | `P115` | `W101_IN_DATA_STREAM` | `DATA_STREAM` | INPUT |
| W102 | Source | `L87_GravityHabitatEnvironmentHologram` | `P111` | `W102_OUT_DATA_STREAM` | `DATA_STREAM` | OUTPUT |
| W102 | Target | `L87_GravityEngineeringMasterSCADA` | `P220` | `W102_IN_DATA_STREAM` | `DATA_STREAM` | INPUT |
| W103 | Source | `L87_BearingHousingTemperatureSensor` | `P079` | `W103_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W103 | Target | `L87_BearingPowerAmplifierCabinet` | `P029` | `W103_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W104 | Source | `L87_BearingPowerAmplifierCabinet` | `P028` | `W104_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W104 | Target | `L87_GravitySafetySupervisoryPLC` | `P121` | `W104_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W105 | Source | `L87_DriveThermalCoolingManifold` | `P054` | `W105_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W105 | Target | `L87_GravitySafetySupervisoryPLC` | `P125` | `W105_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W106 | Source | `L87_HabitatElectricalLoadEmulator` | `P094` | `W106_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W106 | Target | `L87_OrbitalRingPowerBudgetAnalyzer` | `P116` | `W106_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W107 | Source | `L87_HabitatEmergencyLightingLoad` | `P098` | `W107_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W107 | Target | `L87_OrbitalRingPowerBudgetAnalyzer` | `P113` | `W107_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W108 | Source | `L87_HabitatThermalControlLoad` | `P099` | `W108_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W108 | Target | `L87_OrbitalRingPowerBudgetAnalyzer` | `P117` | `W108_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W109 | Source | `L87_HabitatLoadDistributionSimulator` | `P109` | `W109_OUT_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | OUTPUT |
| W109 | Target | `L87_RotorMassDistributionScanner` | `P074` | `W109_IN_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | INPUT |
| W110 | Source | `L87_RotorMassDistributionScanner` | `P074` | `W110_OUT_DATA_STREAM` | `DATA_STREAM` | OUTPUT |
| W110 | Target | `L87_HabitatLoadDistributionSimulator` | `P110` | `W110_IN_DATA_STREAM` | `DATA_STREAM` | INPUT |
| W111 | Source | `L87_OrbitalRingPowerBudgetAnalyzer` | `P116` | `W111_OUT_DATA_STREAM` | `DATA_STREAM` | OUTPUT |
| W111 | Target | `L87_RotorMotorControlPLC` | `P176` | `W111_IN_DATA_STREAM` | `DATA_STREAM` | INPUT |
| W112 | Source | `L87_OrbitalRingPowerBudgetAnalyzer` | `P113` | `W112_OUT_DATA_STREAM` | `DATA_STREAM` | OUTPUT |
| W112 | Target | `L87_GravitySafetySupervisoryPLC` | `P121` | `W112_IN_DATA_STREAM` | `DATA_STREAM` | INPUT |
| W113 | Source | `L87_RotaryPowerTransferMonitor` | `P107` | `W113_OUT_DATA_STREAM` | `DATA_STREAM` | OUTPUT |
| W113 | Target | `L87_GravityEngineeringMasterSCADA` | `P217` | `W113_IN_DATA_STREAM` | `DATA_STREAM` | INPUT |
| W114 | Source | `L87_HabitatThermalControlLoad` | `P099` | `W114_OUT_DATA_STREAM` | `DATA_STREAM` | OUTPUT |
| W114 | Target | `L87_GravityEngineeringMasterSCADA` | `P221` | `W114_IN_DATA_STREAM` | `DATA_STREAM` | INPUT |
| W115 | Source | `L87_HabitatEmergencyLightingLoad` | `P097` | `W115_OUT_DATA_STREAM` | `DATA_STREAM` | OUTPUT |
| W115 | Target | `L87_GravityEngineeringMasterSCADA` | `P225` | `W115_IN_DATA_STREAM` | `DATA_STREAM` | INPUT |
| W116 | Source | `L87_BearingPowerAmplifierCabinet` | `P028` | `W116_OUT_DATA_STREAM` | `DATA_STREAM` | OUTPUT |
| W116 | Target | `L87_GravityEngineeringMasterSCADA` | `P212` | `W116_IN_DATA_STREAM` | `DATA_STREAM` | INPUT |
| W117 | Source | `L87_ContactlessRotaryPowerCouplerPrimary` | `P088` | `W117_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W117 | Target | `L87_RotaryPowerTransferMonitor` | `P104` | `W117_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W118 | Source | `L87_ContactlessRotaryPowerCouplerBackup` | `P089` | `W118_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W118 | Target | `L87_RotaryPowerTransferMonitor` | `P103` | `W118_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W119 | Source | `L87_RotaryPowerIsolationContactor` | `P093` | `W119_OUT_DIGITAL_STATUS` | `DIGITAL_STATUS` | OUTPUT |
| W119 | Target | `L87_RotaryPowerTransferMonitor` | `P107` | `W119_IN_DIGITAL_STATUS` | `DIGITAL_STATUS` | INPUT |
| W120 | Source | `L87_GravityHabitatEnvironmentHologram` | `P112` | `W120_OUT_DATA_STREAM` | `DATA_STREAM` | OUTPUT |
| W120 | Target | `L87_GravityEngineeringMasterSCADA` | `P219` | `W120_IN_DATA_STREAM` | `DATA_STREAM` | INPUT |
| W121 | Source | `L87_OrbitalRingPowerBudgetAnalyzer` | `P118` | `W121_OUT_DATA_STREAM` | `DATA_STREAM` | OUTPUT |
| W121 | Target | `L87_GravityMachineEventRecorder` | `P235` | `W121_IN_DATA_STREAM` | `DATA_STREAM` | INPUT |
| W122 | Source | `L87_RotorMassDistributionScanner` | `P077` | `W122_OUT_DATA_STREAM` | `DATA_STREAM` | OUTPUT |
| W122 | Target | `L87_GravityMachineEventRecorder` | `P234` | `W122_IN_DATA_STREAM` | `DATA_STREAM` | INPUT |
| W123 | Source | `L87_RingEmergencyStopStation` | `P144` | `W123_OUT_SAFETY_CONTROL` | `SAFETY_CONTROL` | OUTPUT |
| W123 | Target | `L87_GravitySafetySupervisoryPLC` | `P121` | `W123_IN_SAFETY_CONTROL` | `SAFETY_CONTROL` | INPUT |
| W124 | Source | `L87_StationSafetyIsolationRelay` | `P236` | `W124_OUT_SAFETY_CONTROL` | `SAFETY_CONTROL` | OUTPUT |
| W124 | Target | `L87_GravitySafetySupervisoryPLC` | `P125` | `W124_IN_SAFETY_CONTROL` | `SAFETY_CONTROL` | INPUT |
| W125 | Source | `L87_IndependentOverspeedProtector` | `P143` | `W125_OUT_SAFETY_CONTROL` | `SAFETY_CONTROL` | OUTPUT |
| W125 | Target | `L87_GravitySafetySupervisoryPLC` | `P129` | `W125_IN_SAFETY_CONTROL` | `SAFETY_CONTROL` | INPUT |
| W126 | Source | `L87_RotorSpeedOpticalEncoder` | `P047` | `W126_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W126 | Target | `L87_IndependentOverspeedProtector` | `P141` | `W126_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W127 | Source | `L87_GravitySafetySupervisoryPLC` | `P120` | `W127_OUT_SAFETY_CONTROL` | `SAFETY_CONTROL` | OUTPUT |
| W127 | Target | `L87_RotorMotorControlPLC` | `P182` | `W127_IN_SAFETY_CONTROL` | `SAFETY_CONTROL` | INPUT |
| W128 | Source | `L87_GravitySafetySupervisoryPLC` | `P124` | `W128_OUT_SAFETY_CONTROL` | `SAFETY_CONTROL` | OUTPUT |
| W128 | Target | `L87_MagneticBearingControlPLC` | `P165` | `W128_IN_SAFETY_CONTROL` | `SAFETY_CONTROL` | INPUT |
| W129 | Source | `L87_GravitySafetySupervisoryPLC` | `P128` | `W129_OUT_SAFETY_CONTROL` | `SAFETY_CONTROL` | OUTPUT |
| W129 | Target | `L87_HabitatPowerDistributionPLC` | `P202` | `W129_IN_SAFETY_CONTROL` | `SAFETY_CONTROL` | INPUT |
| W130 | Source | `L87_MagneticBearingSafetyPLC` | `P137` | `W130_OUT_SAFETY_CONTROL` | `SAFETY_CONTROL` | OUTPUT |
| W130 | Target | `L87_MagneticBearingControlPLC` | `P170` | `W130_IN_SAFETY_CONTROL` | `SAFETY_CONTROL` | INPUT |
| W131 | Source | `L87_RadialSensorDriftFaultInjector` | `P205` | `W131_OUT_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | OUTPUT |
| W131 | Target | `L87_PrimaryRadialGapSensor` | `P019` | `W131_IN_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | INPUT |
| W132 | Source | `L87_BearingAmplifierHeatFaultInjector` | `P206` | `W132_OUT_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | OUTPUT |
| W132 | Target | `L87_BearingPowerAmplifierCabinet` | `P027` | `W132_IN_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | INPUT |
| W133 | Source | `L87_MassImbalanceFaultInjector` | `P207` | `W133_OUT_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | OUTPUT |
| W133 | Target | `L87_RotorMassDistributionScanner` | `P075` | `W133_IN_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | INPUT |
| W134 | Source | `L87_MagneticBearingSafetyPLC` | `P137` | `W134_OUT_SAFETY_CONTROL` | `SAFETY_CONTROL` | OUTPUT |
| W134 | Target | `L87_EmergencyTouchdownBearing` | `P025` | `W134_IN_SAFETY_CONTROL` | `SAFETY_CONTROL` | INPUT |
| W135 | Source | `L87_SecondaryRadialGapSensor` | `P020` | `W135_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W135 | Target | `L87_MagneticBearingSafetyPLC` | `P134` | `W135_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W136 | Source | `L87_W137CriticalFeedbackPatchStation` | `P209` | `W136_OUT_DIGITAL_STATUS` | `DIGITAL_STATUS` | OUTPUT |
| W136 | Target | `L87_MagneticBearingSafetyPLC` | `P138` | `W136_IN_DIGITAL_STATUS` | `DIGITAL_STATUS` | INPUT |
| W137 | Source | `L87_PrimaryRadialGapSensor` | `P019` | `W137_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W137 | Target | `L87_MagneticBearingSafetyPLC` | `P135` | `W137_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W138 | Source | `L87_MagneticBearingSafetyPLC` | `P139` | `W138_OUT_DIGITAL_STATUS` | `DIGITAL_STATUS` | OUTPUT |
| W138 | Target | `L87_GravitySafetySupervisoryPLC` | `P126` | `W138_IN_DIGITAL_STATUS` | `DIGITAL_STATUS` | INPUT |
| W139 | Source | `L87_GravitySafetySupervisoryPLC` | `P130` | `W139_OUT_DATA_STREAM` | `DATA_STREAM` | OUTPUT |
| W139 | Target | `L87_GravityMachineEventRecorder` | `P234` | `W139_IN_DATA_STREAM` | `DATA_STREAM` | INPUT |
| W140 | Source | `L87_GravitySafetySupervisoryPLC` | `P120` | `W140_OUT_DIGITAL_STATUS` | `DIGITAL_STATUS` | OUTPUT |
| W140 | Target | `L87_W137CriticalFeedbackPatchStation` | `P210` | `W140_IN_DIGITAL_STATUS` | `DIGITAL_STATUS` | INPUT |
| W141 | Source | `L87_GravityMachineEventRecorder` | `P234` | `W141_OUT_SAFETY_CONTROL` | `SAFETY_CONTROL` | OUTPUT |
| W141 | Target | `L87_GravitySafetySupervisoryPLC` | `P124` | `W141_IN_SAFETY_CONTROL` | `SAFETY_CONTROL` | INPUT |
| W142 | Source | `L87_MagneticBearingSafetyPLC` | `P139` | `W142_OUT_DIGITAL_STATUS` | `DIGITAL_STATUS` | OUTPUT |
| W142 | Target | `L87_RingCertificationBeacon` | `P229` | `W142_IN_DIGITAL_STATUS` | `DIGITAL_STATUS` | INPUT |
| W143 | Source | `L87_GravitySafetySupervisoryPLC` | `P129` | `W143_OUT_DATA_STREAM` | `DATA_STREAM` | OUTPUT |
| W143 | Target | `L87_GravityEngineeringMasterSCADA` | `P211` | `W143_IN_DATA_STREAM` | `DATA_STREAM` | INPUT |
| W144 | Source | `L87_MagneticBearingControlPLC` | `P165` | `W144_OUT_DATA_STREAM` | `DATA_STREAM` | OUTPUT |
| W144 | Target | `L87_GravityEngineeringMasterSCADA` | `P215` | `W144_IN_DATA_STREAM` | `DATA_STREAM` | INPUT |
| W145 | Source | `L87_RotorMotorControlPLC` | `P177` | `W145_OUT_DATA_STREAM` | `DATA_STREAM` | OUTPUT |
| W145 | Target | `L87_GravityEngineeringMasterSCADA` | `P219` | `W145_IN_DATA_STREAM` | `DATA_STREAM` | INPUT |
| W146 | Source | `L87_VibrationFrequencySpectrumAnalyzer` | `P063` | `W146_OUT_DATA_STREAM` | `DATA_STREAM` | OUTPUT |
| W146 | Target | `L87_GravityEngineeringMasterSCADA` | `P223` | `W146_IN_DATA_STREAM` | `DATA_STREAM` | INPUT |
| W147 | Source | `L87_RotaryPowerTransferMonitor` | `P103` | `W147_OUT_DATA_STREAM` | `DATA_STREAM` | OUTPUT |
| W147 | Target | `L87_GravityEngineeringMasterSCADA` | `P227` | `W147_IN_DATA_STREAM` | `DATA_STREAM` | INPUT |
| W148 | Source | `L87_OrbitalRingPowerBudgetAnalyzer` | `P113` | `W148_OUT_DATA_STREAM` | `DATA_STREAM` | OUTPUT |
| W148 | Target | `L87_GravityEngineeringMasterSCADA` | `P214` | `W148_IN_DATA_STREAM` | `DATA_STREAM` | INPUT |
| W149 | Source | `L87_MagneticBearingSafetyPLC` | `P135` | `W149_OUT_DIGITAL_STATUS` | `DIGITAL_STATUS` | OUTPUT |
| W149 | Target | `L87_GravityEngineeringMasterSCADA` | `P218` | `W149_IN_DIGITAL_STATUS` | `DIGITAL_STATUS` | INPUT |
| W150 | Source | `L87_IndependentOverspeedProtector` | `P141` | `W150_OUT_DIGITAL_STATUS` | `DIGITAL_STATUS` | OUTPUT |
| W150 | Target | `L87_GravityEngineeringMasterSCADA` | `P222` | `W150_IN_DIGITAL_STATUS` | `DIGITAL_STATUS` | INPUT |
| W151 | Source | `L87_GravityEngineeringMasterSCADA` | `P226` | `W151_OUT_DATA_STREAM` | `DATA_STREAM` | OUTPUT |
| W151 | Target | `L87_GravityMachineEventRecorder` | `P232` | `W151_IN_DATA_STREAM` | `DATA_STREAM` | INPUT |
| W152 | Source | `L87_GravityMachineEventRecorder` | `P231` | `W152_OUT_DATA_STREAM` | `DATA_STREAM` | OUTPUT |
| W152 | Target | `L87_GravitySafetySupervisoryPLC` | `P127` | `W152_IN_DATA_STREAM` | `DATA_STREAM` | INPUT |
| W153 | Source | `L87_GravitySafetySupervisoryPLC` | `P131` | `W153_OUT_DIGITAL_STATUS` | `DIGITAL_STATUS` | OUTPUT |
| W153 | Target | `L87_RingCertificationBeacon` | `P228` | `W153_IN_DIGITAL_STATUS` | `DIGITAL_STATUS` | INPUT |
| W154 | Source | `L87_RingCertificationBeacon` | `P229` | `W154_OUT_DIGITAL_STATUS` | `DIGITAL_STATUS` | OUTPUT |
| W154 | Target | `L87_GravityEngineeringMasterSCADA` | `P215` | `W154_IN_DIGITAL_STATUS` | `DIGITAL_STATUS` | INPUT |
| W155 | Source | `L87_W137CriticalFeedbackPatchStation` | `P210` | `W155_OUT_DATA_STREAM` | `DATA_STREAM` | OUTPUT |
| W155 | Target | `L87_GravityEngineeringMasterSCADA` | `P219` | `W155_IN_DATA_STREAM` | `DATA_STREAM` | INPUT |
| W156 | Source | `L87_DynamicRotorHealthRecorder` | `P084` | `W156_OUT_DATA_STREAM` | `DATA_STREAM` | OUTPUT |
| W156 | Target | `L87_GravityEngineeringMasterSCADA` | `P223` | `W156_IN_DATA_STREAM` | `DATA_STREAM` | INPUT |
| W157 | Source | `L87_GravityHabitatEnvironmentHologram` | `P112` | `W157_OUT_DATA_STREAM` | `DATA_STREAM` | OUTPUT |
| W157 | Target | `L87_GravityEngineeringMasterSCADA` | `P227` | `W157_IN_DATA_STREAM` | `DATA_STREAM` | INPUT |
| W158 | Source | `L87_GravityEngineeringMasterSCADA` | `P214` | `W158_OUT_DATA_STREAM` | `DATA_STREAM` | OUTPUT |
| W158 | Target | `L87_RingCertificationBeacon` | `P230` | `W158_IN_DATA_STREAM` | `DATA_STREAM` | INPUT |

## ANNEX F — ALL 96 REUSED VISUAL SOCKET / SIGNAL RECONCILIATION RECORDS

**69** of these 96 anchor records involve more than one source SignalKind. A reused anchor is *not automatically an electrical junction*. Verify domain/direction/return and separate contacts as required.

| Art anchor | Model | Logical contacts | Kinds | Mixed kinds? | Source status |
|---|---|---:|---|:---:|---|
| `P146` | `L87_Protected5VControlSupply` | 2 | `POWER_CONTROL` | False | MUST_RECONCILE_SEPARATE_COMPATIBLE_PORTS |
| `P121` | `L87_GravitySafetySupervisoryPLC` | 4 | `ANALOG_FEEDBACK;DATA_STREAM;POWER_CONTROL;SAFETY_CONTROL` | True | MUST_RECONCILE_SEPARATE_COMPATIBLE_PORTS |
| `P150` | `L87_Protected5VControlSupply` | 2 | `POWER_CONTROL` | False | MUST_RECONCILE_SEPARATE_COMPATIBLE_PORTS |
| `P145` | `L87_Protected5VControlSupply` | 2 | `POWER_CONTROL` | False | MUST_RECONCILE_SEPARATE_COMPATIBLE_PORTS |
| `P157` | `L87_MagneticBearingControlPLC` | 2 | `DATA_STREAM;POWER_CONTROL` | True | MUST_RECONCILE_SEPARATE_COMPATIBLE_PORTS |
| `P180` | `L87_RotorMotorControlPLC` | 4 | `ANALOG_FEEDBACK;DIGITAL_STATUS;POWER_CONTROL;SAFETY_CONTROL` | True | MUST_RECONCILE_SEPARATE_COMPATIBLE_PORTS |
| `P194` | `L87_HabitatPowerDistributionPLC` | 5 | `ANALOG_FEEDBACK;DATA_STREAM;POWER_CONTROL;SAFETY_CONTROL` | True | MUST_RECONCILE_SEPARATE_COMPATIBLE_PORTS |
| `P217` | `L87_GravityEngineeringMasterSCADA` | 2 | `DATA_STREAM;POWER_CONTROL` | True | MUST_RECONCILE_SEPARATE_COMPATIBLE_PORTS |
| `P210` | `L87_W137CriticalFeedbackPatchStation` | 3 | `DATA_STREAM;DIGITAL_STATUS;POWER_CONTROL` | True | MUST_RECONCILE_SEPARATE_COMPATIBLE_PORTS |
| `P170` | `L87_MagneticBearingControlPLC` | 4 | `ACTUATOR_COMMAND;ANALOG_FEEDBACK;SAFETY_CONTROL` | True | MUST_RECONCILE_SEPARATE_COMPATIBLE_PORTS |
| `P002` | `L87_MagneticBearingStationA` | 2 | `ACTUATOR_COMMAND;ANALOG_FEEDBACK` | True | MUST_RECONCILE_SEPARATE_COMPATIBLE_PORTS |
| `P174` | `L87_MagneticBearingControlPLC` | 2 | `ACTUATOR_COMMAND;ANALOG_FEEDBACK` | True | MUST_RECONCILE_SEPARATE_COMPATIBLE_PORTS |
| `P156` | `L87_MagneticBearingControlPLC` | 2 | `ACTUATOR_COMMAND;ANALOG_FEEDBACK` | True | MUST_RECONCILE_SEPARATE_COMPATIBLE_PORTS |
| `P004` | `L87_MagneticBearingStationB` | 2 | `ACTUATOR_COMMAND;ANALOG_FEEDBACK` | True | MUST_RECONCILE_SEPARATE_COMPATIBLE_PORTS |
| `P160` | `L87_MagneticBearingControlPLC` | 2 | `ACTUATOR_COMMAND;ANALOG_FEEDBACK` | True | MUST_RECONCILE_SEPARATE_COMPATIBLE_PORTS |
| `P164` | `L87_MagneticBearingControlPLC` | 2 | `ACTUATOR_COMMAND;ANALOG_FEEDBACK` | True | MUST_RECONCILE_SEPARATE_COMPATIBLE_PORTS |
| `P006` | `L87_MagneticBearingStationC` | 2 | `ACTUATOR_COMMAND;ANALOG_FEEDBACK` | True | MUST_RECONCILE_SEPARATE_COMPATIBLE_PORTS |
| `P168` | `L87_MagneticBearingControlPLC` | 2 | `ANALOG_FEEDBACK` | False | MUST_RECONCILE_SEPARATE_COMPATIBLE_PORTS |
| `P172` | `L87_MagneticBearingControlPLC` | 2 | `ACTUATOR_COMMAND;ANALOG_FEEDBACK` | True | MUST_RECONCILE_SEPARATE_COMPATIBLE_PORTS |
| `P008` | `L87_MagneticBearingStationD` | 2 | `ACTUATOR_COMMAND;ANALOG_FEEDBACK` | True | MUST_RECONCILE_SEPARATE_COMPATIBLE_PORTS |
| `P154` | `L87_MagneticBearingControlPLC` | 2 | `ANALOG_FEEDBACK` | False | MUST_RECONCILE_SEPARATE_COMPATIBLE_PORTS |
| `P158` | `L87_MagneticBearingControlPLC` | 2 | `ACTUATOR_COMMAND;ANALOG_FEEDBACK` | True | MUST_RECONCILE_SEPARATE_COMPATIBLE_PORTS |
| `P010` | `L87_MagneticBearingStationE` | 2 | `ACTUATOR_COMMAND;ANALOG_FEEDBACK` | True | MUST_RECONCILE_SEPARATE_COMPATIBLE_PORTS |
| `P162` | `L87_MagneticBearingControlPLC` | 2 | `ACTUATOR_COMMAND;ANALOG_FEEDBACK` | True | MUST_RECONCILE_SEPARATE_COMPATIBLE_PORTS |
| `P166` | `L87_MagneticBearingControlPLC` | 2 | `ACTUATOR_COMMAND;ANALOG_FEEDBACK` | True | MUST_RECONCILE_SEPARATE_COMPATIBLE_PORTS |
| `P012` | `L87_MagneticBearingStationF` | 2 | `ACTUATOR_COMMAND;ANALOG_FEEDBACK` | True | MUST_RECONCILE_SEPARATE_COMPATIBLE_PORTS |
| `P014` | `L87_MagneticBearingStationG` | 2 | `ACTUATOR_COMMAND;ANALOG_FEEDBACK` | True | MUST_RECONCILE_SEPARATE_COMPATIBLE_PORTS |
| `P016` | `L87_MagneticBearingStationH` | 2 | `ACTUATOR_COMMAND;ANALOG_FEEDBACK` | True | MUST_RECONCILE_SEPARATE_COMPATIBLE_PORTS |
| `P019` | `L87_PrimaryRadialGapSensor` | 3 | `ANALOG_FEEDBACK;DIGITAL_CONTROL` | True | MUST_RECONCILE_SEPARATE_COMPATIBLE_PORTS |
| `P020` | `L87_SecondaryRadialGapSensor` | 2 | `ANALOG_FEEDBACK` | False | MUST_RECONCILE_SEPARATE_COMPATIBLE_PORTS |
| `P028` | `L87_BearingPowerAmplifierCabinet` | 3 | `ACTUATOR_COMMAND;ANALOG_FEEDBACK;DATA_STREAM` | True | MUST_RECONCILE_SEPARATE_COMPATIBLE_PORTS |
| `P027` | `L87_BearingPowerAmplifierCabinet` | 2 | `ANALOG_FEEDBACK;DIGITAL_CONTROL` | True | MUST_RECONCILE_SEPARATE_COMPATIBLE_PORTS |
| `P026` | `L87_EmergencyTouchdownBearing` | 2 | `DIGITAL_STATUS;SAFETY_CONTROL` | True | MUST_RECONCILE_SEPARATE_COMPATIBLE_PORTS |
| `P138` | `L87_MagneticBearingSafetyPLC` | 2 | `DIGITAL_STATUS` | False | MUST_RECONCILE_SEPARATE_COMPATIBLE_PORTS |
| `P038` | `L87_MotorInverterDriveA` | 3 | `ACTUATOR_COMMAND;ANALOG_FEEDBACK;DIGITAL_STATUS` | True | MUST_RECONCILE_SEPARATE_COMPATIBLE_PORTS |
| `P043` | `L87_MotorInverterDriveB` | 3 | `ACTUATOR_COMMAND;ANALOG_FEEDBACK;DIGITAL_STATUS` | True | MUST_RECONCILE_SEPARATE_COMPATIBLE_PORTS |
| `P039` | `L87_MotorInverterDriveA` | 2 | `ACTUATOR_COMMAND;ANALOG_FEEDBACK` | True | MUST_RECONCILE_SEPARATE_COMPATIBLE_PORTS |
| `P034` | `L87_SegmentedRimMotorStator` | 2 | `ACTUATOR_COMMAND` | False | MUST_RECONCILE_SEPARATE_COMPATIBLE_PORTS |
| `P040` | `L87_MotorInverterDriveB` | 2 | `ACTUATOR_COMMAND;ANALOG_FEEDBACK` | True | MUST_RECONCILE_SEPARATE_COMPATIBLE_PORTS |
| `P048` | `L87_RotorSpeedOpticalEncoder` | 2 | `ANALOG_FEEDBACK` | False | MUST_RECONCILE_SEPARATE_COMPATIBLE_PORTS |
| `P177` | `L87_RotorMotorControlPLC` | 3 | `ANALOG_FEEDBACK;DATA_STREAM` | True | MUST_RECONCILE_SEPARATE_COMPATIBLE_PORTS |
| `P176` | `L87_RotorMotorControlPLC` | 2 | `ACTUATOR_COMMAND;DATA_STREAM` | True | MUST_RECONCILE_SEPARATE_COMPATIBLE_PORTS |
| `P052` | `L87_MotorPrechargeContactor` | 2 | `ACTUATOR_COMMAND;ANALOG_FEEDBACK` | True | MUST_RECONCILE_SEPARATE_COMPATIBLE_PORTS |
| `P054` | `L87_DriveThermalCoolingManifold` | 3 | `ANALOG_FEEDBACK` | False | MUST_RECONCILE_SEPARATE_COMPATIBLE_PORTS |
| `P056` | `L87_RingAngularMomentumCompensationUnit` | 2 | `ACTUATOR_COMMAND;ANALOG_FEEDBACK` | True | MUST_RECONCILE_SEPARATE_COMPATIBLE_PORTS |
| `P120` | `L87_GravitySafetySupervisoryPLC` | 4 | `ANALOG_FEEDBACK;DIGITAL_STATUS;SAFETY_CONTROL` | True | MUST_RECONCILE_SEPARATE_COMPATIBLE_PORTS |
| `P182` | `L87_RotorMotorControlPLC` | 2 | `ACTUATOR_COMMAND;SAFETY_CONTROL` | True | MUST_RECONCILE_SEPARATE_COMPATIBLE_PORTS |
| `P044` | `L87_MotorPhaseCurrentSensor` | 2 | `ANALOG_FEEDBACK` | False | MUST_RECONCILE_SEPARATE_COMPATIBLE_PORTS |
| `P050` | `L87_SafeSpinUpSequencer` | 2 | `ANALOG_FEEDBACK;DIGITAL_CONTROL` | True | MUST_RECONCILE_SEPARATE_COMPATIBLE_PORTS |
| `P057` | `L87_MultiAxisVibrationSensorArray` | 2 | `ANALOG_FEEDBACK` | False | MUST_RECONCILE_SEPARATE_COMPATIBLE_PORTS |
| `P063` | `L87_VibrationFrequencySpectrumAnalyzer` | 3 | `ANALOG_FEEDBACK;DATA_STREAM;DIGITAL_CONTROL` | True | MUST_RECONCILE_SEPARATE_COMPATIBLE_PORTS |
| `P061` | `L87_VibrationFrequencySpectrumAnalyzer` | 2 | `DATA_STREAM` | False | MUST_RECONCILE_SEPARATE_COMPATIBLE_PORTS |
| `P068` | `L87_RotorImbalanceVectorDisplay` | 4 | `ACTUATOR_COMMAND;DATA_STREAM` | True | MUST_RECONCILE_SEPARATE_COMPATIBLE_PORTS |
| `P071` | `L87_AdaptiveVibrationDampingController` | 2 | `ACTUATOR_COMMAND;DATA_STREAM` | True | MUST_RECONCILE_SEPARATE_COMPATIBLE_PORTS |
| `P165` | `L87_MagneticBearingControlPLC` | 3 | `ACTUATOR_COMMAND;DATA_STREAM;SAFETY_CONTROL` | True | MUST_RECONCILE_SEPARATE_COMPATIBLE_PORTS |
| `P077` | `L87_RotorMassDistributionScanner` | 2 | `ANALOG_FEEDBACK;DATA_STREAM` | True | MUST_RECONCILE_SEPARATE_COMPATIBLE_PORTS |
| `P078` | `L87_ResonanceSweepTestFixture` | 2 | `DATA_STREAM;DIGITAL_CONTROL` | True | MUST_RECONCILE_SEPARATE_COMPATIBLE_PORTS |
| `P072` | `L87_PrecisionBalanceWeightActuatorA` | 2 | `ACTUATOR_COMMAND;ANALOG_FEEDBACK` | True | MUST_RECONCILE_SEPARATE_COMPATIBLE_PORTS |
| `P073` | `L87_PrecisionBalanceWeightActuatorB` | 2 | `ACTUATOR_COMMAND;ANALOG_FEEDBACK` | True | MUST_RECONCILE_SEPARATE_COMPATIBLE_PORTS |
| `P079` | `L87_BearingHousingTemperatureSensor` | 2 | `ANALOG_FEEDBACK` | False | MUST_RECONCILE_SEPARATE_COMPATIBLE_PORTS |
| `P084` | `L87_DynamicRotorHealthRecorder` | 3 | `ANALOG_FEEDBACK;DATA_STREAM` | True | MUST_RECONCILE_SEPARATE_COMPATIBLE_PORTS |
| `P225` | `L87_GravityEngineeringMasterSCADA` | 2 | `DATA_STREAM` | False | MUST_RECONCILE_SEPARATE_COMPATIBLE_PORTS |
| `P212` | `L87_GravityEngineeringMasterSCADA` | 2 | `DATA_STREAM` | False | MUST_RECONCILE_SEPARATE_COMPATIBLE_PORTS |
| `P088` | `L87_ContactlessRotaryPowerCouplerPrimary` | 2 | `ANALOG_FEEDBACK;DIGITAL_CONTROL` | True | MUST_RECONCILE_SEPARATE_COMPATIBLE_PORTS |
| `P089` | `L87_ContactlessRotaryPowerCouplerBackup` | 2 | `ANALOG_FEEDBACK;DIGITAL_CONTROL` | True | MUST_RECONCILE_SEPARATE_COMPATIBLE_PORTS |
| `P091` | `L87_RotaryOpticalTelemetryLink` | 2 | `DATA_STREAM` | False | MUST_RECONCILE_SEPARATE_COMPATIBLE_PORTS |
| `P102` | `L87_RotatingDataQualityMonitor` | 2 | `DATA_STREAM` | False | MUST_RECONCILE_SEPARATE_COMPATIBLE_PORTS |
| `P198` | `L87_HabitatPowerDistributionPLC` | 4 | `ACTUATOR_COMMAND;ANALOG_FEEDBACK;DATA_STREAM;DIGITAL_CONTROL` | True | MUST_RECONCILE_SEPARATE_COMPATIBLE_PORTS |
| `P093` | `L87_RotaryPowerIsolationContactor` | 3 | `ACTUATOR_COMMAND;ANALOG_FEEDBACK;DIGITAL_STATUS` | True | MUST_RECONCILE_SEPARATE_COMPATIBLE_PORTS |
| `P202` | `L87_HabitatPowerDistributionPLC` | 5 | `ANALOG_FEEDBACK;DATA_STREAM;DIGITAL_CONTROL;SAFETY_CONTROL` | True | MUST_RECONCILE_SEPARATE_COMPATIBLE_PORTS |
| `P190` | `L87_HabitatPowerDistributionPLC` | 3 | `ANALOG_FEEDBACK;DIGITAL_CONTROL` | True | MUST_RECONCILE_SEPARATE_COMPATIBLE_PORTS |
| `P110` | `L87_HabitatLoadDistributionSimulator` | 3 | `ANALOG_FEEDBACK;DATA_STREAM` | True | MUST_RECONCILE_SEPARATE_COMPATIBLE_PORTS |
| `P117` | `L87_OrbitalRingPowerBudgetAnalyzer` | 2 | `ANALOG_FEEDBACK;DATA_STREAM` | True | MUST_RECONCILE_SEPARATE_COMPATIBLE_PORTS |
| `P094` | `L87_HabitatElectricalLoadEmulator` | 2 | `ANALOG_FEEDBACK;DIGITAL_CONTROL` | True | MUST_RECONCILE_SEPARATE_COMPATIBLE_PORTS |
| `P098` | `L87_HabitatEmergencyLightingLoad` | 2 | `ANALOG_FEEDBACK;SAFETY_CONTROL` | True | MUST_RECONCILE_SEPARATE_COMPATIBLE_PORTS |
| `P099` | `L87_HabitatThermalControlLoad` | 3 | `ANALOG_FEEDBACK;DATA_STREAM;DIGITAL_CONTROL` | True | MUST_RECONCILE_SEPARATE_COMPATIBLE_PORTS |
| `P125` | `L87_GravitySafetySupervisoryPLC` | 2 | `ANALOG_FEEDBACK;SAFETY_CONTROL` | True | MUST_RECONCILE_SEPARATE_COMPATIBLE_PORTS |
| `P116` | `L87_OrbitalRingPowerBudgetAnalyzer` | 2 | `ANALOG_FEEDBACK;DATA_STREAM` | True | MUST_RECONCILE_SEPARATE_COMPATIBLE_PORTS |
| `P113` | `L87_OrbitalRingPowerBudgetAnalyzer` | 3 | `ANALOG_FEEDBACK;DATA_STREAM` | True | MUST_RECONCILE_SEPARATE_COMPATIBLE_PORTS |
| `P074` | `L87_RotorMassDistributionScanner` | 2 | `DATA_STREAM;DIGITAL_CONTROL` | True | MUST_RECONCILE_SEPARATE_COMPATIBLE_PORTS |
| `P107` | `L87_RotaryPowerTransferMonitor` | 2 | `DATA_STREAM;DIGITAL_STATUS` | True | MUST_RECONCILE_SEPARATE_COMPATIBLE_PORTS |
| `P103` | `L87_RotaryPowerTransferMonitor` | 2 | `ANALOG_FEEDBACK;DATA_STREAM` | True | MUST_RECONCILE_SEPARATE_COMPATIBLE_PORTS |
| `P112` | `L87_GravityHabitatEnvironmentHologram` | 2 | `DATA_STREAM` | False | MUST_RECONCILE_SEPARATE_COMPATIBLE_PORTS |
| `P219` | `L87_GravityEngineeringMasterSCADA` | 3 | `DATA_STREAM` | False | MUST_RECONCILE_SEPARATE_COMPATIBLE_PORTS |
| `P234` | `L87_GravityMachineEventRecorder` | 3 | `DATA_STREAM;SAFETY_CONTROL` | True | MUST_RECONCILE_SEPARATE_COMPATIBLE_PORTS |
| `P129` | `L87_GravitySafetySupervisoryPLC` | 2 | `DATA_STREAM;SAFETY_CONTROL` | True | MUST_RECONCILE_SEPARATE_COMPATIBLE_PORTS |
| `P141` | `L87_IndependentOverspeedProtector` | 2 | `ANALOG_FEEDBACK;DIGITAL_STATUS` | True | MUST_RECONCILE_SEPARATE_COMPATIBLE_PORTS |
| `P124` | `L87_GravitySafetySupervisoryPLC` | 2 | `SAFETY_CONTROL` | False | MUST_RECONCILE_SEPARATE_COMPATIBLE_PORTS |
| `P137` | `L87_MagneticBearingSafetyPLC` | 2 | `SAFETY_CONTROL` | False | MUST_RECONCILE_SEPARATE_COMPATIBLE_PORTS |
| `P135` | `L87_MagneticBearingSafetyPLC` | 2 | `ANALOG_FEEDBACK;DIGITAL_STATUS` | True | MUST_RECONCILE_SEPARATE_COMPATIBLE_PORTS |
| `P139` | `L87_MagneticBearingSafetyPLC` | 2 | `DIGITAL_STATUS` | False | MUST_RECONCILE_SEPARATE_COMPATIBLE_PORTS |
| `P229` | `L87_RingCertificationBeacon` | 2 | `DIGITAL_STATUS` | False | MUST_RECONCILE_SEPARATE_COMPATIBLE_PORTS |
| `P215` | `L87_GravityEngineeringMasterSCADA` | 2 | `DATA_STREAM;DIGITAL_STATUS` | True | MUST_RECONCILE_SEPARATE_COMPATIBLE_PORTS |
| `P223` | `L87_GravityEngineeringMasterSCADA` | 2 | `DATA_STREAM` | False | MUST_RECONCILE_SEPARATE_COMPATIBLE_PORTS |
| `P227` | `L87_GravityEngineeringMasterSCADA` | 2 | `DATA_STREAM` | False | MUST_RECONCILE_SEPARATE_COMPATIBLE_PORTS |
| `P214` | `L87_GravityEngineeringMasterSCADA` | 2 | `DATA_STREAM` | False | MUST_RECONCILE_SEPARATE_COMPATIBLE_PORTS |

## ANNEX G — ORIGINAL SIX OBJECTIVE PLAN RECORDS

**All six source records say `Implemented=NO`.** The runtime evaluator, six objectives and test evidence are requests to implement.

| # | Source title | Acceptance criteria from package | Planned evidence | Implemented in art kit? |
|---:|---|---|---|:---:|
| 1 | **Build the Gravity Control Network** | 158 actual player-created correct topology connections with logical socket reconciliation; 5 V and real returns; safety valid; W137 trusted. | Real Spark electrical graph, sources and returns, correct terminal domains, evidence hash | NO |
| 2 | **Commission the Magnetic Levitation System** | Eight bearing stations, radial/axial sensing, amplifier limits and touchdown transition physically stable. | Measured actual air gaps, coil currents, qualified temperature and valid timestamped calibration | NO |
| 3 | **Accelerate the Artificial Gravity Ring** | Controlled motor torque and angular dynamics, acceptable vibration, independent overspeed check, momentum protection and target RPM. | Actual simulated angular velocity, drive torque and stored momentum history | NO |
| 4 | **Maintain Stable Artificial Gravity** | Sustained stable ring speed, load distribution, bearing thermal health, rotary data and mission essential loads preserved. | Fresh complete sustained runID, physics and power balance, load deadlines, vibration/temperature checks | NO |
| 5 | **Recover Three Critical Rotation Failures** | Real sensor bias, amplifier overheating and mass imbalance disturbances each diagnosed and repaired; new full stabilization run. | Causal fault/repair record; fresh spin-up, bearing and sustained mission proof | NO |
| 6 | **Restore Critical Magnetic Bearing Feedback** | Physical W137 disconnect after 1-5 historical certifications, safe control response, reconnect, fresh calibration, explicit reset, new full operating cycle. | Real topology change; latched fault; independent protection; new runID; actual LevelGamePlayManager 6/6 | NO |

## ANNEX H — EXACT W137 AND NEIGHBORING ROUTE TRUST MATRIX

The following rows are lifted from the source conceptual wiring table. Preserve the exact route and treat diagnostic/telemetry routes as nonauthoritative for normal control.

| WID | Source | Destination | Kind | Purpose |
|---|---|---|---|---|
| **W123** | `L87_RingEmergencyStopStation` `P144` | `L87_GravitySafetySupervisoryPLC` `P121` | `SAFETY_CONTROL` | Emergency stop physical wired safety status |
| **W124** | `L87_StationSafetyIsolationRelay` `P236` | `L87_GravitySafetySupervisoryPLC` `P125` | `SAFETY_CONTROL` | Independent isolation relay operational state |
| **W125** | `L87_IndependentOverspeedProtector` `P143` | `L87_GravitySafetySupervisoryPLC` `P129` | `SAFETY_CONTROL` | Independent overspeed status |
| **W126** | `L87_RotorSpeedOpticalEncoder` `P047` | `L87_IndependentOverspeedProtector` `P141` | `ANALOG_FEEDBACK` | Independent angular speed sample |
| **W127** | `L87_GravitySafetySupervisoryPLC` `P120` | `L87_RotorMotorControlPLC` `P182` | `SAFETY_CONTROL` | Main drive safety permission |
| **W128** | `L87_GravitySafetySupervisoryPLC` `P124` | `L87_MagneticBearingControlPLC` `P165` | `SAFETY_CONTROL` | Magnetic bearing stabilization safety permit |
| **W129** | `L87_GravitySafetySupervisoryPLC` `P128` | `L87_HabitatPowerDistributionPLC` `P202` | `SAFETY_CONTROL` | Rotary power safety isolation authorization |
| **W130** | `L87_MagneticBearingSafetyPLC` `P137` | `L87_MagneticBearingControlPLC` `P170` | `SAFETY_CONTROL` | Bearing safe-mode command and measured readiness |
| **W131** | `L87_RadialSensorDriftFaultInjector` `P205` | `L87_PrimaryRadialGapSensor` `P019` | `DIGITAL_CONTROL` | Injected primary radial sensor drift bias |
| **W132** | `L87_BearingAmplifierHeatFaultInjector` `P206` | `L87_BearingPowerAmplifierCabinet` `P027` | `DIGITAL_CONTROL` | Injected bearing amplifier thermal fault |
| **W133** | `L87_MassImbalanceFaultInjector` `P207` | `L87_RotorMassDistributionScanner` `P075` | `DIGITAL_CONTROL` | Injected rotating cargo imbalance |
| **W134** | `L87_MagneticBearingSafetyPLC` `P137` | `L87_EmergencyTouchdownBearing` `P025` | `SAFETY_CONTROL` | Touchdown support protective engagement |
| **W135** | `L87_SecondaryRadialGapSensor` `P020` | `L87_MagneticBearingSafetyPLC` `P134` | `ANALOG_FEEDBACK` | Redundant sensor cross-check diagnostic |
| **W136** | `L87_W137CriticalFeedbackPatchStation` `P209` | `L87_MagneticBearingSafetyPLC` `P138` | `DIGITAL_STATUS` | Critical feedback patch continuity monitor ONLY |
| **W137** | `L87_PrimaryRadialGapSensor` `P019` | `L87_MagneticBearingSafetyPLC` `P135` | `ANALOG_FEEDBACK` | CRITICAL trusted radial displacement feedback W137 |
| **W138** | `L87_MagneticBearingSafetyPLC` `P139` | `L87_GravitySafetySupervisoryPLC` `P126` | `DIGITAL_STATUS` | Safety critical feedback fault latch status |
| **W139** | `L87_GravitySafetySupervisoryPLC` `P130` | `L87_GravityMachineEventRecorder` `P234` | `DATA_STREAM` | Safety-critical fault and event sequence record |
| **W140** | `L87_GravitySafetySupervisoryPLC` `P120` | `L87_W137CriticalFeedbackPatchStation` `P210` | `DIGITAL_STATUS` | W137 recovery safety qualification display |
| **W141** | `L87_GravityMachineEventRecorder` `P234` | `L87_GravitySafetySupervisoryPLC` `P124` | `SAFETY_CONTROL` | Operator explicit fault reset request |
| **W142** | `L87_MagneticBearingSafetyPLC` `P139` | `L87_RingCertificationBeacon` `P229` | `DIGITAL_STATUS` | Independent bearing safety final qualified status |
| **W149** | `L87_MagneticBearingSafetyPLC` `P135` | `L87_GravityEngineeringMasterSCADA` `P218` | `DIGITAL_STATUS` | W137 trusted feedback timestamp and safety validity |
| **W155** | `L87_W137CriticalFeedbackPatchStation` `P210` | `L87_GravityEngineeringMasterSCADA` `P219` | `DATA_STREAM` | W137 display patch diagnostic only, no bypass |

## ANNEX I — SOURCE MODEL'S SPATIAL / FILE / TEST CONVENTIONS

| Field | Source-manifest value |
|---|---|
| `level_id` | `"level_87"` |
| `display_name` | `"The Gravity Engine"` |
| `subtitle` | `"Artificial Gravity, Magnetic Bearings and Rotation Control"` |
| `individual_models` | `86` |
| `individual_glbs` | `86` |
| `assembled_fbx` | `"Models/Level87_Complete_ArtKit_Showcase.fbx"` |
| `assembled_glb` | `"GLB/Level87_Complete_ArtKit_Showcase.glb"` |
| `individual_triangles` | `184948` |
| `individual_mesh_parts` | `1208` |
| `animation_parent_pivot_groups` | `68` |
| `planned_player_wires` | `158` |
| `planned_visual_terminal_sockets` | `236` |
| `proposed_independent_logical_wire_endpoints` | `316` |
| `anchor_reconciliation_conflicts` | `96` |
| `visual_ring_radius_m` | `5.2` |
| `gameplay_proposed_full_ring_radius_m` | `45` |
| `proposed_demo_target_rpm` | `3` |
| `workstation_world_bounds_m` | `[[-15.0, -3.255, -23.0], [15.0, 13.835, 10.1]]` |
| `fbx_numeric_units` | `"centimetres"` |
| `fbx_unit_scale_factor` | `1` |
| `glb_numeric_units` | `"metres"` |
| `axes` | `"Y-up X-width Z-forward"` |
| `assembly_origin` | `[0, 0, 0]` |
| `actual_player_wires_included` | `false` |
| `functional_sparkterminals_included` | `false` |
| `actual_magnetic_bearing_physics_included` | `false` |
| `actual_gravity_ring_simulator_included` | `false` |
| `six_runtime_objectives_implemented` | `false` |
| `unity_compilation_tested` | `false` |
| `unity_playmode_tested` | `false` |

## ANNEX J — SHIPPED EDITOR AND VISUAL COMPONENT CONTRACT

- `Unity_Optional/SparkLevel87ArtPlacementEditor.cs` (namespace `ProjectSpark.EditorTools`): **Tools/Project Spark/Levels/87/Place HD Art Under Selected Layout**.
- `Unity_Optional/SparkLevel87MaterialUpgradeEditor.cs` (namespace `ProjectSpark.EditorTools`): **Tools/Project Spark/Levels/87/Create & Apply HD Materials To Selected Art**.
- `Unity_Optional/SparkLevel87AssetValidatorEditor.cs` (namespace `ProjectSpark.EditorTools`): **Tools/Project Spark/Levels/87/Validate Imported HD Art**.
- `Unity_Optional/SparkLevel87VisualRig.cs` (namespace `ProjectSpark.Visuals`): `BindVisuals`, `SetActualRingDynamics`, `SetActualBearingMechanisms`, `SetActualSafety` exactly as specified in section 10.

The art-only scripts must not be mistaken for a complete editor-level builder, functional terminals or dynamics.

## ANNEX K — MATERIAL / TEXTURE REFERENCE FILES

| Material set | Resolution | Maps | Source use |
|---|---|---|---|
| `OrbitalHabitatCeramicHull` | 1024×1024 | Albedo, Normal, MetalRoughAO | OPTIONAL_REFERENCE_MAPS_REQUIRING_UV_AND_SHADER_CHANNEL_MAPPING |
| `PrecisionBearingTitanium` | 1024×1024 | Albedo, Normal, MetalRoughAO | OPTIONAL_REFERENCE_MAPS_REQUIRING_UV_AND_SHADER_CHANNEL_MAPPING |
| `ElectromagneticCopperWindings` | 1024×1024 | Albedo, Normal, MetalRoughAO | OPTIONAL_REFERENCE_MAPS_REQUIRING_UV_AND_SHADER_CHANNEL_MAPPING |
| `RotatingHabitatGoldAnodized` | 1024×1024 | Albedo, Normal, MetalRoughAO | OPTIONAL_REFERENCE_MAPS_REQUIRING_UV_AND_SHADER_CHANNEL_MAPPING |
| `CarbonCompositeRotor` | 1024×1024 | Albedo, Normal, MetalRoughAO | OPTIONAL_REFERENCE_MAPS_REQUIRING_UV_AND_SHADER_CHANNEL_MAPPING |
| `ElectroMagnetPoleSteel` | 1024×1024 | Albedo, Normal, MetalRoughAO | OPTIONAL_REFERENCE_MAPS_REQUIRING_UV_AND_SHADER_CHANNEL_MAPPING |
| `MagneticFieldCyanHologram` | 1024×1024 | Albedo, Normal, MetalRoughAO | OPTIONAL_REFERENCE_MAPS_REQUIRING_UV_AND_SHADER_CHANNEL_MAPPING |
| `VerticalStationGlass` | 1024×1024 | Albedo, Normal, MetalRoughAO | OPTIONAL_REFERENCE_MAPS_REQUIRING_UV_AND_SHADER_CHANNEL_MAPPING |
| `GravityControlInstrumentPanel` | 1024×1024 | Albedo, Normal, MetalRoughAO | OPTIONAL_REFERENCE_MAPS_REQUIRING_UV_AND_SHADER_CHANNEL_MAPPING |
| `OrbitalMachineServiceFloor` | 1024×1024 | Albedo, Normal, MetalRoughAO | OPTIONAL_REFERENCE_MAPS_REQUIRING_UV_AND_SHADER_CHANNEL_MAPPING |
| `ActiveSafetyAmberWarning` | 1024×1024 | Albedo, Normal, MetalRoughAO | OPTIONAL_REFERENCE_MAPS_REQUIRING_UV_AND_SHADER_CHANNEL_MAPPING |
| `EmergencyRedSafetyHousing` | 1024×1024 | Albedo, Normal, MetalRoughAO | OPTIONAL_REFERENCE_MAPS_REQUIRING_UV_AND_SHADER_CHANNEL_MAPPING |

`MetalRoughAO` packing per source: **R=Metallic, G=Roughness, B=Ambient Occlusion**. UV authoring and appropriate shader channels must be completed; source states `uv_authoring_required=true`.

## FINAL AUTHORITY REMINDER

The package supplies **3D art and design/planning data**. It does **not** supply connected player wiring, working magnetic bearing levitation, validated rotor physics, full PBR art production, six certified runtime objectives or a compiled Unity game. Implement in the actual Project Spark project, test in Play Mode, and report the result without embellishment. The **authoritative** success is the real `LevelGamePlayManager` showing **6/6** after the W137 recovery and completely new qualification run.
