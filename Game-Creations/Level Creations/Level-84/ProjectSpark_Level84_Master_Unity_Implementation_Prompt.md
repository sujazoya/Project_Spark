# PROJECT SPARK — LEVEL 84 MASTER UNITY IMPLEMENTATION PROMPT

## LEVEL 84 — DEFY THE TUMBLE
### Advanced AAA Spacecraft Attitude Determination & Control • Four Reaction Wheels • Three-Axis Magnetorquers • Gyroscope + Star-Tracker Sensor Fusion • Quaternion Flight Dynamics • Momentum Desaturation • Optical Ground-Station Pointing • Critical W097 Feedback Recovery

> **SOURCE-GROUNDED IMPLEMENTATION SPECIFICATION.** This document is constructed from `ProjectSpark_Level84_HD_AAA_Models.zip` and follows the comprehensive engineering, Unity integration, testing and source-appendix format of the supplied Level 82 and Level 83 Master Unity Implementation Prompts. The asset names, exact source manifests, art scale, pivot records, conceptual wiring rows, six planned objective titles, and W097 route are based on the Level 84 ZIP. Gameplay physics, algorithms, numerical acceptance thresholds, adapters, tests and Editor builder are **requirements to implement**, not evidence of existing compiled gameplay.

---

## 01. PRIMARY IMPLEMENTATION MANDATE

Act as my **senior Unity C# gameplay engineer, Project Spark electrical system integration architect, spacecraft ADCS and guidance/control engineer, rigid-body dynamics and numerical simulation engineer, inertial navigation and sensor-fusion engineer, reaction-wheel/magnetic-torquer actuator engineer, optical communications and ground-station pointing engineer, safety-PLC and mission-autonomy designer, AAA technical artist, and Unity Editor automation developer**.

**IMPLEMENT A COMPLETE, FUNCTIONAL, PLAYABLE LEVEL 84 — DEFY THE TUMBLE — IN MY EXISTING PROJECT SPARK UNITY GAME.** This is not a request to redesign the already delivered 3D asset kit. Import the 76 supplied modular models and implement genuine 142 player-made electrical/signal connections, spacecraft attitude dynamics, realistic-but-accessible actuator control, calibrated and time-stamped measurements, integrated safety, three physical disturbances, final trusted W097 failure and recovery, six authoritative objectives, one-click level creation, and verification.

The player's final target is an actual **`LevelGamePlayManager` 6/6** reached by executing the real mission sequence. **Preserve Levels 1–83**. Do not report implementation or Play Mode success unless actually demonstrated inside the Unity project.

## 02. INSPECT EXACT ZIP CONTENTS FIRST

Extract and read real packaged files before generating scenes/scripts. Source of truth:

- `Model_Manifest.json`, `Art_Assembly.csv`, `Component_Roles.csv`, `Verification_Report.json`.
- `Animation_Rigs.csv` for **58** mesh-owning pivot parents and their precise axes and positions.
- `Gameplay_Wiring_Concept_W001_W142.csv`, `Gameplay_Wiring_Budget.csv` for **142** conceptual connections.
- `Terminal_Anchor_Plan_218.csv`, `Visual_Terminal_Sockets_218.csv` for **218** art-only sockets.
- `Objective_Certification_Plan_6.csv`, `W097_Critical_Feedback_Recovery.md`, `Gameplay_Integration_Plan.md`.
- `Texture_Manifest.json`, `AAA_Art_Delivery_Notes.md`, `README_Unity.txt`, `Checksums_SHA256.txt`.
- `Unity_Optional/SparkLevel84ArtPlacementEditor.cs`, `SparkLevel84AssetValidatorEditor.cs`, `SparkLevel84MaterialUpgradeEditor.cs`, and `SparkLevel84VisualRig.cs`.

The appendices below reproduce every model, pivot, planned wire, original art socket and objective. **Prefer actual imported package files if they ever differ from this text**; report any mismatch instead of inventing names or methods.

## 03. VERIFIED SOURCE BASELINE; WHAT IS NOT ALREADY BUILT

The manifest/verification files report **76 numbered modular binary FBX 7.4 meshes; 76 corresponding GLBs; one assembled showcase FBX and GLB; 171,408 modular equipment triangles; 1,125 mesh parts; 58 real animation pivot parents; 218 `SOCKET_P###` visual socket meshes; 142 conceptual W001–W142 connections; six objective design records; 12 procedural 1024×1024 PBR reference material sets and 36 PNG maps**.

Source bounds: minimum **(-13.5, 0, -7.225) m**, maximum **(13.5, 10.472, 7.225) m**, approximately **27.0 m wide / 10.472 m tall**. Geometry/structure was checked, **not** Unity compile or Play Mode. The ZIP says `actual_player_wires_included=false`, `functional_spark_terminals_included=false`, `adcs_rigidbody_solver_included=false`, and `six_authoritative_objectives_implemented=false`. It contains a technical art kit and planning files, not a ready-to-play scene. The UVs/bakes/LOD/collider/lighting production finish remains to be authored.

## 04. INSPECT ACTUAL PROJECT SPARK CODE AND INTERFACES

Find and read actual code for `SparkElectricalSolver`, `SparkCircuitSystem`, `SparkElectricalComponent`, `SparkTerminal`, `SparkSwitch`, `SparkLevelDefinition`, `SparkLevelEvaluator`, `SparkLevelSceneBindings`, `SparkLevelWorkstationBindingBridge`, `SparkShelfSlot`, `SparkObjectShelf`, `SparkLevelLayoutLoader`, `SparkLevel`, and `LevelGamePlayManager`, when present. Discover true method signatures, namespaces, actual power/return semantics, events on wire creation/disconnect, wire ownership, terminal fan-out/junction rules, scene/prefab conventions, shelf/shop purchasing, serialization, UI and level evaluator callbacks.

Study reusable real implementations from prior Levels 58–83, notably physical fault recovery, sensor freshness, PLC, independent safety, motion, qualification run IDs and historical objective completion. **Do not treat earlier planning prompts as proof that a method exists**. Use thin testable adapters for genuinely missing APIs; never silently replace the global solver with an unrelated fake system.

## 05. ABSOLUTE GAMEPLAY RULES

1. **All 142 puzzle connections must be placed by the player** and reflected in live Spark circuit topology; do not prewire them or count decorative splines.
2. **218 sockets are art-only anchors**, not already functional `SparkTerminal` components. Their proposed mapping must be checked and reconciled before use.
3. Keep educational 5 V control circuits, simulated spacecraft actuator-power physics, and independent safety logic separate; never push flight motor energy through invented 5 V signal conductors.
4. Simulate actual spacecraft free-body quaternion and angular velocity. **Three-axis test-gimbal graphics never supply free flight torque or zero momentum.**
5. Reaction wheels exchange angular momentum with the body, are subject to finite inertia/torque/speed/thermal limits, and saturate.
6. Magnetorquer torque follows **`tau = m × B`**, limited by field geometry and current; it cannot make torque parallel to B, and it cannot desaturate at arbitrary rate.
7. Commands, actual states, sensor readings, estimated states, and certified evidence must remain distinct.
8. Gyroscopes drift and require calibration; a star tracker needs line of sight, baffle, angular-rate and observation/cadence conditions; stale data is invalid.
9. A secondary **diagnostic** tracker or SCADA display cannot replace the trusted **W097** route without a separately designed and certified authority path.
10. Optical lock depends on actual pointing, jitter, tracking window, payload alignment and simulated link quality; a static green indicator is not evidence.
11. Two qualifying ground-station passes must be distinct scheduled windows with actual spacecraft dynamics and link samples, not two button clicks.
12. Each of the three disturbances modifies real authoritative plant/sensor state; repairs must change physics, not flags.
13. W097 disconnection changes live graph continuity and expires trusted quaternion samples. It must not teleport attitude to identity or stop genuine angular momentum.
14. Fine-pointing authorization must withdraw on W097 loss; independent safety can bound actuator drive and command a coarse Sun-safe fallback.
15. On W097 repair: reacquire fresh stars, validate gyro/absolute attitude agreement, realign/calibrate, explicitly reset the latch, and complete a fresh two-pass optical mission.
16. Objectives 1–5 remain **historically certified** during the deliberately induced final W097 fault; Objective 6 must use new post-repair evidence.
17. One authoritative state owner per subsystem; visual rigs/SCADA consume state read-only.
18. The level must be playable, readable, challenging and deterministic enough to test, not only cinematic.
19. Preserve Levels 1–83 and all project-customized content outside a replaceable visual art root.
20. Never invent passing tests, runtime scripts, physical safety certificates or Unity compilation results.

## 06. LEVEL METADATA AND EXPERIENCE

`level_id = level_84`; display title **Defy the Tumble**; subtitle **Three-Axis Spacecraft Attitude and Precision Optical Pointing**; advanced/expert/master difficulty; 6 certified objectives; **142 player connections**, **218 original socket locations**, **76 art models**, **58 animation parents**. One-click editor entry **`Tools/Project Spark/Levels/Create Level 84`**.

Narrative: having established a functioning orbital power system in Level 83, the player must now make the spacecraft *point*. The simulated spacecraft tumbles, its star tracker cannot initially lock, and precision optical downlink is impossible. Commission a valid ADCS, kill rotation, acquire stars, perform three-axis slews, conserve momentum, execute two ground-station optical passes, recover three increasingly complex faults, then survive deliberate loss and repair of the critical trusted star-tracker feedback.

Central gameplay loop: **wire → power/safety → calibrate → detumble → acquire stars → estimate quaternion → control body → unload wheel momentum → acquire/hold optical link → troubleshoot three faults → W097 disconnect/reconnect → new two-pass qualification → actual 6/6**.

## 07. MODEL IMPORT AND ASSEMBLED COORDINATES

Import the 76 numbered files `00_...fbx` to `75_...fbx` as individually addressable modular assets. Their FBX numeric positions/vertices are **centimetres** with FBX `UnitScaleFactor=1`; the GLBs are in **metres**. After correct importer conversion, **one Unity unit = one metre**. Do not scale the whole kit twice. **Y up, X width, Z front**. All numbered models use common origin, identity local rotation, `1,1,1` local scale after importer conversion.

Create `L84_HD_VISUAL_ART` as a replaceable child of the Level 84 layout and instantiate each modular model once beneath it. Verify workstation width ~27.0 m and height ~10.472 m. The separately assembled `Models/Level84_Complete_ArtKit_Showcase.fbx` / `GLB/Level84_Complete_ArtKit_Showcase.glb` are **reference/alternate assemblies only**, not additional geometry to load alongside the modular set. Keep colliders, functional terminals, wire markers, physics authority, PLC, objectives, interaction hotspots and scenario state **outside** the disposable art root.

## 08. REUSE ACTUAL PACKAGED ART EDITOR MENUS

The kit ships tools in namespace `ProjectSpark.EditorTools`:

- `SparkLevel84ArtPlacementEditor`: **`Tools/Project Spark/Levels/84/Place HD Art Under Selected Layout`**; creates/replaces `L84_HD_VISUAL_ART`.
- `SparkLevel84MaterialUpgradeEditor`: **`Tools/Project Spark/Levels/84/Create & Apply HD Materials To Selected Art`**; materials in `Assets/ProjectSpark/Level84/Materials`.
- `SparkLevel84AssetValidatorEditor`: **`Tools/Project Spark/Levels/84/Validate Imported HD Art`**.
- Runtime component `ProjectSpark.Visuals.SparkLevel84VisualRig` is an **art and animation consumer**, not an attitude flight solver.

Place `*Editor.cs` in a Unity Editor-only assembly/folder and the runtime visual component outside Editor. Validate current Unity/render-pipeline compatibility. Do not confuse these three art menus with the new, full Level 84 gameplay builder.

## 09. AAA ART, MATERIALS, OPTICS, EFFECTS AND PERFORMANCE

Use 12 PBR reference sets / 36 maps: albedo, normal and packed `MetalRoughAO` (**R metallic, G roughness, B AO**). Repack for URP/HDRP/Standard shader expectations, converting roughness to smoothness where needed. Production UV authoring, normal baking, lightmapping, LODs, colliders, emissive surfaces and GPU instancing remain required. Use premium titanium, carbon composite, ceramic, realistic copper coil windings, star-tracker coated glass, gold-rimmed electrical sockets, instrument displays and clear protective enclosures.

Art direction: large dark aerospace lab with suspended nested gimbal rings; authentic mechanical reaction wheels; glowing magenta/cyan magnetic and quaternion vector graphics; distant Earth, navigable star catalog, restrained blue beam targeting; instrument panels showing actual measurement data. Disturbance feedback: gyro bias produces growing estimator residual; wheel saturation produces amber momentum bars; flexible solar wings display body-coupled jitter; W097 loss produces red trusted-measurement warning without freezing the spacecraft. Optimize for real-time through static batching/instancing, art LOD groups, pooled VFX, screen-space culling, clean transparent sorting and minimal dynamic material allocations.

## 10. VERIFIED VISUAL RIG PUBLIC API — USE EXACT SIGNATURES

The delivered runtime class is `ProjectSpark.Visuals.SparkLevel84VisualRig`. Its **verified** methods are:

```csharp
public void SetActualAttitude(Quaternion actualBodyToWorld, Vector3 measuredBodyRatesDegreesPerSecond);
public void SetActualReactionWheels(float[] actualRpm, float[] measuredMomentumUtilization01, float[] measuredTemperaturesC);
public void SetActualMagneticSystem(Vector3 actualMagneticDipoleBody, Vector3 measuredLocalMagneticFieldBody, bool actualMomentumUnloading);
public void SetActualNavigationSensors(float measuredStarMatchConfidence01, float measuredEstimatorUncertaintyDegrees,
    bool trustedW097StarTrackerMeasurementValid, bool actualStarCatalogMatchQualified,
    float measuredGyroBiasSeverity01, float actualTrackerBaffleOpen01, float actualCalibrationProgress01);
public void SetActualOpticalPointing(float measuredPointingErrorDegrees, float measuredJitterRmsDegrees,
    bool actualPointingControllerQualified, bool actualOpticalDownlinkLocked,
    float measuredTransmittedDataActivity01, int actualQualifiedPassCount);
public void SetActualSafetyAndFaults(bool actualCriticalW097FeedbackValid, bool actualCriticalFaultIsLatched,
    bool actualCoarseSunSafeMode, float actualGyroBiasDisturbance01,
    float actualWheelSaturationDisturbance01, float actualSolarWingFlexibleMotion01);
public void SetActualMissionState(float normalizedMissionTime01, float targetTrackingProgress01, float actualCertificationProgress01);
```

Map actual authority/measured gameplay values into these methods via an adapter. **Pass array lengths of four** for all reaction-wheel arrays, preserve quaternion ordering/handedness, degrees and normalized 0–1 channels exactly as declared. The rig drives the 58 `ANIM_*` parents and optional displays in `LateUpdate`. Do not create nonexistent animation setters or call these methods to fabricate physical state. If additional effects are needed, add independent read-only VFX binding components.

## 11. REAL 58-PIVOT HIERARCHY AND RIG SAFETY

Read `Animation_Rigs.csv` for each exact `Pivot`, `Model`, `Kind`, `Axis`, `X_m`, `Y_m`, `Z_m`, `Group`. Annex B prints all 58. Preserve local home transforms and actual physical hierarchy. The three concentric test-rig rings can visualize body attitude but must not mechanically apply torque to the simulated free body. Wheel rotors animate from actual individual wheel RPM, momentum bars from actual stored wheel angular momentum, magnetic coils/field lines from modeled dipole/current, star-tracker baffle and calibration motion from actual mechanical states. `Kind=indicator` uses UI/emission/particles without inventing a physical axis. Reject missing or duplicate pivot names and incorrect axes in validation.

## 12. EXACT 142 PLAYER-WIRE BUDGET

Use the original `Gameplay_Wiring_Concept_W001_W142.csv` for exact source and target equipment, proposed terminal names, visual anchors, signal domain, purpose and critical flag. Annex C reproduces all rows. The groups are:

| Planned IDs | Purpose | Count |
|---|---|---:|
| W001–W010 | Educational 5 V control power | 10 |
| W011–W032 | Reaction wheels, motors and feedback | 22 |
| W033–W052 | Magnetic torquers and magnetic sensing | 20 |
| W053–W074 | Gyroscopes, star trackers and Sun sensors | 22 |
| W075–W094 | Attitude estimation and pointing control | 20 |
| W095–W110 | Critical feedback and motion safety | 16 |
| W111–W130 | Optical pointing and mission functions | 20 |
| W131–W142 | PLC, SCADA and final certification | 12 |
| **TOTAL** | | **142** |

Each declared wire must be connected by the player through real project wiring tools. Validate exact IDs, owner models, domain, direction, voltage compatibility, permissible fanout and required reference/return. A wire's `connected` state is only valid when the actual Spark circuit solver finds that correct live edge. Creating a LineRenderer or drawing an animation **is not a player wire**. Integrate proper disconnect and reconnect events; at least W097 must be physically removable after initial certification.

## 13. ART SOCKETS VS REAL FUNCTIONAL PORTS — SOURCE-VERIFIED CONFLICT

There are **218 decorative socket anchors P001–P218** but **142 × 2 = 284 planned connection endpoint occurrences**. The present wiring CSV references **199 distinct** anchor IDs; **47** anchors are reused across different proposed functional port names. The source reuses, for example, **P001** for ten separate control-supply outlets; **P012** for multiple wheel command outputs; **P035** for multiple distinct analog measurements; and **P064** for magnetic, gyro, Earth-horizon and the trusted W097 absolute-attitude estimator input. Although this source wiring plan does not mix the `SignalKind` label on a given reused anchor, **same-category signals are still not necessarily the same electrical net**. This is a real integration issue; do not short gyro time validity, magnetic vectors, and W097 quaternion together just because the same gold socket location appears in the art.

Implement deterministic reconciliation mapping `(WireID, side, source/destination model, source art anchor, proposed functional port name, signal kind, direction, domain)` → real compatible `SparkTerminal` and physical contact. Allow a shared bus **only if electrically/semantically intentional**, else create distinct labeled subports around/within the original socket visual, or report necessary additional physical connectors. Preserve all 218 original P IDs and planned W IDs. If the engine cannot express the required 284 endpoint occurrences within 218 actual distinct terminals, create extra functional terminals explicitly with IDs and a `Level84_TerminalReconciliationReport.csv`; do not claim exact 218 functional `SparkTerminal` objects without proving no shorts. Ensure W097 maps to a **dedicated trusted-feedback input**, despite P064's other reused analog labels.

## 14. THE UNIQUE CRITICAL W097 ROUTE — EXACT ZIP DATA

- **Wire:** `W097`, `Critical=YES`, `SignalKind=ANALOG_FEEDBACK` in the source concept CSV.
- **Source model:** `L84_PrimaryStarTracker`.
- **Source visual socket:** `P139`.
- **Source logical terminal:** `STAR_TRACKER_QUATERNION_OUT`.
- **Destination model:** `L84_AttitudeStateEstimator`.
- **Destination visual socket:** `P064`.
- **Destination logical terminal:** `ADCS_ABSOLUTE_ATTITUDE_IN`.
- **Intended signal:** continuously timestamped, calibrated **absolute attitude quaternion**, star-match confidence, validity and heartbeat.

W097 must be one real, physically disconnectable graph edge; do not infer trusted continuity from a visual cable, cached quaternion, an estimator's `health` flag, SCADA or the secondary diagnostic tracker. A quaternion is a structured sample, even though the conceptual CSV classifies its transport as `ANALOG_FEEDBACK`; implement suitable typed data/signal serialization using verified project APIs, while preserving source ID and intent. A loss of W097 creates **unknown/stale absolute attitude** rather than a false zero quaternion or perfect estimate.

## 15. THREE ENGINEERING DOMAINS AND AUTHORITY BOUNDARIES

**A. Real educational 5 V wiring:** Project Spark circuit solver, player-created wires, control supplies and returns, logic and sensor ports, PLC inputs and signal continuity. **B. Fictional attitude-control plant:** rigid-body attitude, wheel rotor momentum, magnetorquer dipole and B-field, sensor capture, optical beam and power/thermal constraints; simulated separately from tiny 5 V wiring. **C. Independent safety:** emergency stop, motor-current/speed limits, torquer limits, faulty sensor gating, latched safe-mode controller and explicit reset. A mission-control algorithm cannot bypass the independent safety station through SCADA or a stale data path.

## 16. ONE AUTHORITATIVE ADCS STATE MODEL

Maintain one consistent simulation record for: spacecraft body-frame inertia tensor and center of mass; quaternion `q_body_to_inertial`; body angular rates; wheel axes/inertia/RPM/current/temperature/angular momentum and allocated torque; local inertial magnetic field and commanded/actual dipoles; environmental disturbance torques; gyro truth/bias/noise/sample times; Sun/Earth sensors; star catalog, tracker attitude samples and uncertainty; state-estimation quaternion/covariance; planned target trajectory; actual control torque vs requested torque; solar-wing vibration and structural modes; optical ground-station geometry and data packets; safety states; full player wiring topology; mission evidence and objective history. Use explicit units **rad, rad/s, N·m, kg·m², A·m², tesla, seconds** internally; convert to degrees only for UI/rig methods. If fictional normalized values replace SI units, clearly document every mapping.

## 17. DETERMINISTIC FIXED-STEP ORDER

Recommended authoritative `FixedUpdate`-driven or dedicated fixed-timestep sequence:

1. Read actual Spark graph, commands and user wiring changes.
2. Evaluate 5 V logic/returns and independent safety gates.
3. Advance scenario/orbit clock, inertial target, ground pass windows, Sun and magnetic environment.
4. Advance motor-drive commands, wheel current/thermal constraints and torquer dipole response.
5. Compute actual reaction-wheel body torque and external magnetic/environment torque.
6. Integrate full body **angular momentum**, angular velocity and normalized unit quaternion.
7. Evolve flexing solar-wing modes, structural vibration and optical boresight jitter.
8. Acquire gyro, magnetometer, Sun/horizon and star-tracker observations on distinct clocks.
9. Check calibration, power, sampling age, confidence, visibility and W097 graph continuity.
10. Propagate/update state estimator quaternion and covariance without future-sample leaks.
11. Compute trajectory reference and closed-loop control, bounded by safety and wheel saturation.
12. Evaluate real optical line-of-sight/link samples during actual scheduled passes.
13. Process faults/diagnosis/repair, update safety latch and mission state machine.
14. Capture time-stamped objective evidence and run-scoped certification history.
15. Push read-only measurements to SCADA, indicators, VFX and the verified visual rig.

Log actual step size, deterministic fault seed and simulation `runId`; guard against frame-rate-dependent results, fixed-step overflow and invalid quaternion normalization.

## 18. RIGID-BODY QUATERNION ATTITUDE DYNAMICS

Use a **real, finite-inertia, torque-driven** 3D educational rigid body. For example, body-frame angular momentum includes stored wheel momentum, and with consistent sign conventions satisfies an Euler-style relation `dH_body/dt + omega × H_body = tau_external`; reaction-wheel drive exchanges momentum internally. For nonzero external magnetic/gravity-gradient/drag/solar-pressure torques, the vehicle's total angular momentum can change accordingly. Quaternion integration follows the chosen documented body-to-world convention, e.g. `q_dot = 0.5 * q ⊗ (0, omega_body)` under a consistent right-multiplication convention; normalize after integration, handle ±q equivalence and shortest-arc attitude errors. Use bounded numerical integrators and reproducible unit tests for torque-free, constant-torque, spin reversal and zero-rate stability. Never blend the spacecraft to the commanded target by animation smoothing alone.

## 19. FREE TUMBLE, INERTIA AND DETUMBLING

Initialize a nonzero three-axis angular rate and an arbitrary nontrivial orientation. The attitude estimate initially degrades until sensors and actuators are correctly wired and calibrated. With only reaction wheels, the player can redistribute body-wheel momentum subject to limits; sustainable long-term desaturation needs legitimate external torque from magnetorquers in an appropriate magnetic field. Create an accessible coarse detumble stage, then transition to fine inertial pointing after actual angular-rate and star-lock criteria pass. Show true angular rate, estimated rate, rate residual and wheel state separately.

## 20. FOUR INDEPENDENT REACTION WHEEL MODELS

Model X/Y/Z orthogonal wheels plus **skew backup** with measured wheel-axis geometry from configuration. Each wheel stores rotor inertia, RPM, actual spin direction, torque constant proxy, current, friction/drag, maximum acceleration/deceleration, max absolute RPM, motor temperature, current limit, fault state and tachometer sample age. Drive actual motion with finite response and sign-correct body reaction torque. Build a true **4-wheel torque allocation** (weighted least-squares or damped pseudoinverse) rather than assigning identical torque to all wheels; handle skew-wheel redundancy, failed wheel, sign changes, remaining speed authority, torque/thermal saturation and residual torque when commands cannot be met. Report residuals to the controller and do not fabricate full 3-axis authority after wheel failure.

## 21. MOTOR DRIVERS, FEEDBACK AND WHEEL THERMAL STATE

Source equipment includes `L84_ReactionWheelMotorDriverRack`, `L84_ReactionWheelTachometerArray`, `L84_ReactionWheelMotorCurrentMonitor`, `L84_ReactionWheelThermalControlModule`, wheel cluster/housing and saturation alarms. Integrate bounded motor voltage/current proxy and heating/cooling. Commands must not instantly equal measured RPM. Require real current/speed feedback for qualification, with calibration timestamps and missed-sample detection. Safety must independently inhibit out-of-range driver commands; the safe controller should handle a wheel already spinning rather than teleporting its speed to zero.

## 22. MAGNETORQUERS AND PHYSICAL MOMENTUM UNLOADING

The X/Y/Z rods generate commanded magnetic dipole vectors with bounded current/dipole/thermal dynamics. Torque is `tau_magnetic = m_body × B_body`. Derive `B_body` from modeled orbital field and actual quaternion, not from a decorative field line. Include weak/null torque geometries, saturation, drive delays, field measurement cadence and control reorientation opportunities. The `L84_MomentumDesaturationController`/`L84_ReactionWheelDesaturationSupervisor` must choose torquer commands that relieve wheel momentum **only when geometrically feasible**, with realistic response and measured evidence. Do not directly reset wheel RPM to zero or subtract stored momentum without external torque.

## 23. MAGNETIC FIELD, ORBITAL ENVIRONMENT AND DISTURBANCE TORQUES

Use configurable fictional but coherent orbit/environment: position/phase, inertial Sun vector, magnetic field vector, ground-station line of sight and occlusion windows. `L84_OrbitalMagneticFieldEmulator`, `L84_GravityGradientTorqueEmulator`, `L84_OrbitalDragTorqueEmulator`, `L84_SolarRadiationPressureTorqueModel`, `L84_ExternalDisturbanceTorqueInjector` modify actual external torque. Scale illustrative torque amplitudes to the simulated inertia and time budget; keep them bounded, seeded and reproducible. Do not claim high-fidelity astrodynamic or spaceflight-qualified orbital prediction from an educational scenario. Expose measurable torque budgets and demonstrate equilibrium/control margins under active disturbances.

## 24. GYROSCOPE/IMU — BIAS, CALIBRATION AND PROPAGATION

Simulate a tri-axis gyroscope providing actual body rate plus finite rate bias, scale-factor error, measurement noise, thermal drift, sample latency and optional saturation. `L84_GyroscopeCalibrationStation` needs a real calibration interaction/stationary-reference condition and produces timestamped calibration validities. `L84_GyroBiasThermalFaultEmulator` introduces an actual drifting bias, not merely a red gauge. The estimator integrates gyro samples and accumulates orientation uncertainty between absolute observations; recalibration must use fresh comparison and a finite settling/sampling period. Detect gyro/absolute-attitude residuals; avoid using a ground-truth quaternion secretly when sensor data is invalid.

## 25. STAR TRACKER, BATTLE WITH THE STARS, CATALOG MATCHING

`L84_PrimaryStarTracker`, `L84_StarTrackerOpticalBaffle`, `L84_SecondaryDiagnosticStarTracker` and `L84_StarCatalogPatternMatcher` must have distinguishable modeled roles. Generate a synthetic star catalog/visible-star field or geometric reference consistent with displayed navigation dome; calculate visibility based on boresight, field of view, obstruction, baffle state, angular rate, exposure/settling, background stray light and sample quality. Acquire several valid pattern matches before declaring star lock; output calibrated, timestamped quaternion with confidence and covariance. High body rate prevents fine tracking until detumbled. Secondary tracker offers diagnostics/consistency, not automatic trust substitution for W097. Stale cached quaternion must not be accepted as a new star observation.

## 26. SUN SENSORS, EARTH HORIZON, MAGNETOMETER

`L84_CoarseSunSensorCluster`, `L84_EarthHorizonSensor`, `L84_TriaxialMagnetometer` provide limited, noisy body-frame direction/field measurements. A single Sun vector cannot resolve full yaw about itself; magnetic vectors may offer extra information unless aligned/weak. Earth-horizon visibility depends on geometry. Calibrate sensor-to-body misalignment, timing and confidence; reject impossible observations or stale samples. Enable coarse safe attitude with bounded certainty when W097 is absent, while **never** labeling coarse estimates as W097-quality fine absolute attitude.

## 27. QUATERNION ESTIMATION AND SENSOR FUSION

`L84_AttitudeStateEstimator`, `L84_ComplementaryFusionDisplay`, `L84_EstimatorUncertaintyDisplay`, `L84_HolographicQuaternionAttitudeSphere` must reflect an actual estimative process. Implement a documented educational complementary filter, error-state EKF or equivalent filter: propagate with gyroscope measurements/bias, apply fresh star-tracker quaternion residual updates, optionally incorporate Sun/field/horizon vectors where observable, track estimator covariance/uncertainty and innovation residuals, handle quaternion sign ambiguity and out-of-order samples, and expose clear confidence states. A loss of absolute observation grows uncertainty even if gyro integration continues; a UI spinner is not a fusion algorithm. Prevent directly copying simulated true quaternion into the estimator.

## 28. THREE-AXIS FLIGHT CONTROLLER, SLEW AND SETTLING

Use `L84_AttitudeFlightController`, `L84_ThreeAxisTorqueAllocationComputer`, `L84_SlewTrajectoryPlanner`, `L84_PrecisionPointingErrorAnalyzer`, `L84_InertialReferenceClock`. For target quaternion `q_ref`, compute shortest rotation error and rate error and request bounded body torque (e.g. PD/PID with antiwindup and torque compensation). Use finite velocity/acceleration/rate-limited reference trajectories; avoid instantaneous orientation changes. Require actual measured settle and overshoot bounds, wheel headroom, valid estimator and no forbidden safe-mode before fine pointing. Display commanded angle, true measured angle, estimated angle, residual, body rates, actuator residual and predicted settle time.

## 29. FOUR-WHEEL ALLOCATION, FAULT RECONFIGURATION AND AUTHORITY

Map desired 3D body torque to four wheel motor torques with known wheel-axis matrix; solve under constraints. If skew backup is enabled after a wheel defect, maintain control only if surviving axes retain sufficient rank and authority. Detect windup when a wheel reaches RPM/current/thermal limit and command a legitimate momentum unloading maneuver. Distinguish nominal torque allocation from reconfigured failed-wheel mode. Certification must fail if unrealizable torque requests are silently clipped without flagging accuracy degradation.

## 30. STRUCTURAL FLEXIBILITY AND LINE-OF-SIGHT JITTER

`L84_PortSolarArrayWing`, `L84_StarboardSolarArrayWing`, `L84_SolarWingFlexibleModeEmulator`, `L84_StructuralVibrationSensor`, `L84_ActivePointingVibrationFilter` provide a bounded simplified flexible-mode plant. Model one or more damped oscillator modes coupled to body torque/slew and solar-wing dynamics. The flexible-wing disturbance increases actual boresight pointing jitter and accelerometer/vibration measurements; the filtered controller has finite bandwidth, phase lag and tradeoffs. A vibration suppression feature must not produce zero jitter instantly or hide true beam error; aggressive filtering can slow slews or cause lag. Visual solar-wing shaking follows actual mode amplitudes.

## 31. OPTICAL PAYLOAD AND BORESIGHT CALIBRATION

The spacecraft has `L84_CoPackagedOpticalTransmitter`, `L84_MissionPayloadBoresightReference`, `L84_OpticalLineOfSightSensor`, `L84_OpticalLinkAcquisitionFixture`, `L84_OpticalSignalQualityTelemetry`. Simulate an optical boresight distinct from spacecraft body-forward axis with a calibrated payload alignment transform. Require a valid boresight calibration and target line-of-sight prediction, actual attitude slews, beam acquisition cone, tracking fine-error/jitter thresholds, and finite actual signal/data transfer. A spacecraft pointed near Earth is not equivalent to an aligned optical downlink; track transmitter enable, link margin, line-of-sight validity and delivered data independently.

## 32. TWO ACTUAL GROUND-STATION PASSES

Use a fictional reproducible orbit/ground-station scenario with at least two **non-overlapping** scheduled visibility windows separated by a meaningful period. `L84_GroundStationTargetProjector`, `L84_OpticalGroundStationAimReticle`, `L84_OrbitalEphemerisDisplayRail` show **predicted and actual** line of sight based on scenario time and spacecraft pose. A pass only qualifies if the spacecraft has valid trusted estimator/W097, fine controller authorized, payload aligned, stable body motion, sustained optical lock, bounded RMS jitter and sufficient successfully transferred data **during that actual pass**. Count unique pass IDs, not starts/stops or repeated frames in one window. When target is geometrically blocked, the beam cannot score. Build a two-pass mission certificate with timestamps, delivered data and worst pointing error.

## 33. MISSION CLOCK, ENERGY AND POWER COUPLING

Level 84 should respect energy continuity with the preceding orbital-power theme. If an actual reusable Level 83 energy interface exists, consume it through a verified adapter; otherwise supply an explicitly **fictional bounded ADCS actuator-energy budget** that accounts for wheel motor, torquer drive, sensors and optical transmitter demand. Do not claim working reuse without integration. Low-power mode may reduce actuator authority and transmitter availability. Do not change level 83 itself. Use actual mission clock, pass deadlines and power availability checks to prevent an infinite-time all-systems-off shortcut.

## 34. INITIAL GAME STATE — 0/6

The rig and spacecraft art are staged, but none of the 142 required player wires are connected. 5 V controls unqualified; inertial and absolute sensors not commissioned; gyro calibration pending; primary star tracker has not acquired an acceptable star fix; four wheels are idle/uncalibrated; magnetorquer axes uncommissioned; body has genuine nonzero tumble; optical link disabled; power and safety contact checks incomplete; scheduled passes are pending; W097 exists only as a planned endpoint until the player wires it. No later faults start already certified. The actual level manager reads **0/6**.

## 35. PLC STATES AND REAL MISSION SEQUENCER

Recommended explicit mode states: `OFF`, `WIRING_CHECK`, `CONTROL_POWER_OK`, `INDEPENDENT_SAFETY_CHECK`, `WHEEL_COMMISSIONING`, `MAGNETIC_CALIBRATION`, `GYRO_CALIBRATION`, `DETUMBLE_ACTIVE`, `COARSE_ATTITUDE_ESTIMATE`, `STAR_ACQUISITION`, `TRACKER_CALIBRATION`, `FINE_ESTIMATOR_READY`, `THREE_AXIS_SLEW`, `POINTING_SETTLING`, `PRECISION_POINTING_READY`, `OPTICAL_PASS_A_ACQUISITION`, `OPTICAL_PASS_A_TRACKING`, `PASS_B_PREPARATION`, `OPTICAL_PASS_B_TRACKING`, `TWO_PASS_QUALIFIED`, `DISTURBANCE_DIAGNOSIS`, `DISTURBANCE_RECOVERY`, `W097_FEEDBACK_LOST`, `COARSE_SUN_SAFE`, `CRITICAL_FAULT_LATCHED`, `RESET_REQUIRED`, `POST_REPAIR_RECALIBRATION`, `POST_REPAIR_TWO_PASS_QUALIFICATION`, `FINAL_CERTIFIED`. Never transition to qualified solely on an elapsed timer or animation completion; require topology, real dynamics and time-stamped evidence. State machine must support abort, retry, save/load and fault-injection provenance.

## 36. PLAYER-FACING REAL INTERACTIONS

Implement usable equipment interactions and/or SCADA controls: `MAIN 5V`, `VALIDATE WIRING`, `SAFETY SELF TEST`, `COMMISSION WHEEL X/Y/Z/SKEW`, `CALIBRATE TACHOMETERS`, `CHECK MOTOR CURRENT`, `CONFIGURE WHEEL ALLOCATION`, `COMMISSION TORQUERS`, `CALIBRATE MAGNETOMETER`, `HOME GIMBAL VISUAL`, `CALIBRATE GYRO`, `START DETUMBLE`, `CHECK BODY RATE`, `OPEN STAR TRACKER BAFFLE`, `ACQUIRE STARS`, `MATCH CATALOG`, `CALIBRATE TRACKER BORESIGHT`, `ENABLE FUSION`, `SLEW TO TARGET`, `SETTLE / HOLD`, `START DESATURATION`, `CALIBRATE OPTICAL BORESIGHT`, `TARGET GROUND STATION`, `START PASS A`, `START PASS B`, `START LINK`, `REVIEW TELEMETRY`, `INJECT GYRO BIAS`, `INJECT WHEEL SATURATION`, `INJECT SOLAR FLEX`, `DIAGNOSE / REPAIR`, `PHYSICALLY DISCONNECT W097`, `PHYSICALLY RECONNECT W097`, `REACQUIRE STARS`, `RECALIBRATE`, `EXPLICIT SAFETY RESET`, `FRESH TWO-PASS QUALIFICATION`. Each button must call a real verified project input/action and may be disabled with clear cause. Never allow a UI-only disconnect flag to pass the physical wire objective.

## 37. INDEPENDENT MOTION SAFETY AND AUTHORIZATION

`L84_IndependentMotionSafetyStation`, `L84_EmergencyAttitudeStopStation`, `L84_AttitudeSafeModeSupervisor`, `L84_SunSafeAttitudeController`, `L84_ReactionWheelSaturationAlarm` and related equipment enforce independent drive limits. Protect against excessive wheel RPM/current/thermal load, torquer current, untrusted sample ages, estimator divergence, unsafe actuator commands and emergency stop. On W097 loss, withdraw **fine-pointing and optical mission authorization**, inhibit unbounded wheel/torquer requests, permit bounded coarse stabilization if physically supported and keep independent E-stop authoritative. Never force actual body angular momentum or state estimates to zero. A safe-mode command may take finite settling time; monitor residual rates and report inability to reach safe attitude if conditions are insufficient.

## 38. CRITICAL W097 LOSS — PHYSICAL DISCONNECT

After historical Objectives 1–5 are certified, the player physically unplugs **W097 P139 → P064** via the real project wire interaction. The circuit graph must lose that edge and stop delivering new **trusted** absolute-attitude quaternion samples. Frozen last-good observations become stale based on timestamps; estimated covariance continues evolving from valid coarse/gyro data; perfect absolute orientation is unavailable. The supervisor withdraws fine-pointing permission, aborts/holds optical transmission, initiates bounded safe-mode control, applies independent actuator limits, latches a critical feedback fault and records causal event IDs. No SCADA cache, W096 freshness diagnostic, secondary diagnostic tracker or previous objective certificate may silently stand in for the trusted W097 stream. **The spacecraft keeps physically rotating under its actual state and torques.** Objectives 1–5 stay historically certified but live readiness may turn red.

## 39. W097 RECONNECTION, CALIBRATION AND EXPLICIT RESET

Physical repair steps: reconnect the correct real source/destination ports, verify solver connectivity and signal compatibility, verify 5 V supply/reference to the sensor, receive fresh *new* star observations and catalog matches with valid confidence/timestamps, check gyro vs tracker residual, recalibrate tracker-to-body alignment, confirm estimator convergence/covariance and coarse sensor readiness, validate all independent motion safety conditions and **request explicit operator reset**. Do not self-clear the latch on reconnection or merely on receipt of one sample. Resume fine control gradually, settle body rates, re-establish boresight alignment, start a **new post-repair mission `runId`** and qualify two **fresh non-overlapping optical passes**; old Pass A/B data cannot satisfy Objective 6.

## 40. THREE DISTINCT PLANT DISTURBANCES

**A — Thermal gyro bias:** `L84_GyroBiasThermalFaultEmulator` adds a time-dependent actual gyro bias, causing estimator drift/innovation failure, increased attitude error and loss of confidence. Diagnose against fresh star and other reference observations; cool/repair/calibrate, reacquire and demonstrate convergence and real angular-rate/pointing recovery.

**B — Reaction wheel momentum saturation:** spin and disturbance history drive one or more real wheel momentum/RPM states toward the limit; allocated torque clips, pointing performance drops, alarm triggers. Diagnose tachometer, torque/current and angular momentum logs; perform field-dependent magnetic desaturation while maintaining bounded spacecraft attitude. **No instant RPM reset.**

**C — Flexible solar-wing jitter:** apply a seeded excitation to flex modes, physically increasing boresight jitter and possibly degrading optical downlink; diagnose vibration sensor, flexible-mode spectrum and line-of-sight error; tune/enable physically limited active damping and/or alter slew profile; demonstrate verified reduction without invalid timing or mission performance.

Each fault must be triggered, measured in actual plant/sensor variables, diagnosed with causal evidence, repaired by real interactions, and followed by a **new complete two-pass optical qualification** after the final disturbance.

## 41. DISTURBANCE REPAIR AND DATA LOGGING CONTRACT

Store each disturbance's unique `faultId`, start time, affected true state, measured symptom, diagnostic sample references, chosen repair, post-repair state, safety reset if applicable, and run-scoped qualification. An alarm acknowledgment does not repair gyro bias, desaturate a wheel or damp a flex mode. Replaying the same fault label without a new plant mutation must not count. Make deliberately incorrect repairs instructive (e.g., gyro gain tweak while sensor bias persists); show relevant physics and stable retries. Prevent exploits by pausing simulation, reloading cached valid samples or advancing mission pass counters without time/geometry.

## 42. OBJECTIVE 1 — BUILD THE ATTITUDE CONTROL NETWORK

**Exact source title:** `Build the Attitude Control Network`.

Certify **1/6** only after all **142 required player-created wire connections** are graph-valid with no cross-shorted logical ports; all original 218 art-anchor records are preserved/reconciled; educational 5 V/returns and PLC are valid; four reaction wheels, three torquers, gyro, magnetometer, Sun/horizon sensors, primary star tracker, state estimator, safe-mode station and optical payload are wired/recognized; independent E-stop works; and real W097 is connected and initially qualified for appropriate sampling. Capture topology hash, functional port mapping revision, safety proof and commissioning timestamps. Do not count art sockets alone as actual `SparkTerminal` objects.

## 43. OBJECTIVE 2 — STOP THE TUMBLE AND FIND THE STARS

**Exact source title:** `Stop the Tumble and Find the Stars`.

Certify **2/6** when the true three-axis body-rate norm decreases below a configured detumble criterion for a sustained dwell, gyro/magnetometer and reference sensors are calibrated, actuator torque is genuinely applied without violating limits, and the primary star tracker acquires a verified fresh absolute quaternion via real W097 with sufficient star-pattern confidence and valid estimator covariance. Prevent an initial static spacecraft, toggling a detumble button or resetting the quaternion from passing.

## 44. OBJECTIVE 3 — ACHIEVE PRECISION THREE-AXIS POINTING

**Exact source title:** `Achieve Precision Three-Axis Pointing`.

Certify **3/6** with all four wheel models configured, control allocation computed under real limits, one or more actual nontrivial 3-axis target slews performed with finite acceleration, held true quaternion/estimated pointing error below the scenario tolerance, bounded rate and jitter, sufficient wheel momentum headroom, trusted estimator samples and safety authorization. Show real settling behavior and prove backup-wheel/torquer functions where required; neither moving ring art nor simply setting a reference target qualifies.

## 45. OBJECTIVE 4 — HOLD THE OPTICAL BEAM

**Exact source title:** `Hold the Optical Beam`.

Certify **4/6** after two **distinct scheduled ground-station passes** with valid geometric visibility, actual line-of-sight optical acquisition/lock, fine pointing and RMS jitter margins maintained for configured continuous dwell, legitimate transmitted data volume, valid 5 V and modeled payload power, trusted W097, safe reaction-wheel momentum and qualified magnetic desaturation where required. Record pass IDs, timestamps, target alignment, packet/data totals, true and estimated pointing margins and link stability. A downlink signal displayed outside a pass window does not count.

## 46. OBJECTIVE 5 — SURVIVE ATTITUDE-CONTROL DISTURBANCES

**Exact source title:** `Survive Attitude-Control Disturbances`.

Certify **5/6** only after the actual **gyro bias**, **reaction-wheel saturation**, and **solar-wing flexible-mode jitter** faults have each been deliberately exercised, caused measurable changes, been diagnosed, and been physically/algorithmically recovered. Then a new independent **two-pass optical qualification run** must succeed with no unresolved critical fault and stable body, estimator, momentum, vibration and link margins. Fault flags alone cannot count as repaired.

## 47. OBJECTIVE 6 — RESTORE CRITICAL STAR-TRACKER FEEDBACK

**Exact source title:** `Restore Critical Star-Tracker Feedback`.

Requires historical 1–5, actual W097 topology break, trusted quaternion sample expiration, safe-mode authorization withdrawal, bounded coarse Sun-safe operation and latched critical fault; then physically reconnect W097, obtain truly fresh stars/catalog data, recalibrate tracker alignment/estimator, verify gyro agreement and independent safety, explicitly reset and **complete another brand-new two-pass optical mission**. Objective 6 must **not** award on reconnect/reset alone. The only success is authoritative actual `LevelGamePlayManager` **6/6**, proven by running the integrated Unity game.

## 48. CERTIFICATION EVIDENCE, HISTORICAL STATE AND RUN IDS

Track `ObjectiveCertifiedAt`, `objectiveId`, `runId`, `faultId`, `wiringTopologyHash`, functional terminal mapping version, sensor calibration version, sample times, pass IDs, actual angular rates, quaternion residuals, covariance/uncertainty, wheel momentum headroom, torquer actuation, jitter RMS, link data totals and safety status. Never let an animation or SCADA field set an objective directly. Keep the historical Objective 1–5 records on W097 fault while the **current** ADCS live readiness becomes unqualified. Require `postRepairTwoPassRunId` strictly newer than the W097 fault event and all Pass A/B samples newly generated within that run. Preserve legitimate progress across save/load, but invalidate replayed/stale qualification data.

## 49. LIVE SCADA, INSTRUMENTS AND PLAYER DIAGNOSTICS

`L84_HolographicAttitudeControlSCADA`, `L84_AttitudeFlightDataRecorder`, `L84_ADCSCertificationBeacon` and holograms display: actual and estimated quaternion/Euler presentation; per-axis body rate and angular momentum; wheel actual RPM/current/thermal/load/headroom; requested vs applied torque and allocation residual; local B-field and vector torquer authority; gyro bias/covariance/calibration; star-tracker visible matches/heartbeats/W097 graph continuity/timestamp; Sun/horizon sensor validity; 3-axis pointing and settle trace; solar-wing vibration spectrum/RMS; optical boresight, predicted pass windows, actual pass lock and data volume; PLC safety state, independent inhibit/E-stop, latched fault, and objective 0/6–6/6. Distinguish **TRUE**, **MEASURED**, **ESTIMATED**, **COMMANDED**, **QUALIFIED**, and **STALE**. Use appropriate world-space UI/flat HUD readability, not a sea of decorative unreadable numbers.

## 50. SCENARIO TUNING — ALL VALUES FICTIONAL AND EDITABLE

Use a `ScriptableObject` scenario profile for spacecraft inertia, initial angular rates, wheel speed/torque limits, sensor cadence/noise, gyro bias, orbital B field, time scaling, optical pass geometry, motor thermal response, flex natural frequency/damping, estimator covariance and safety thresholds. Example **illustrative game-scale defaults only**: initial tumble roughly 1–3°/s by axis, coarse capture under 0.8°/s, fine capture under 0.1°/s sustained, fine pointing within 0.25° mean error and 0.05° RMS jitter, two distinct visible pass windows each demanding multi-second continuous lock. These are **not** real spacecraft qualification standards and must be tuned via deterministic tests to remain achievable. Never gate on only a single scalar `adcsProgress`.

## 51. PROPOSED C# RUNTIME COMPONENTS — ADAPT TO VERIFIED CODEBASE

Create only where missing: `SparkLevel84Coordinator`, `SparkLevel84ProcessState`, `SparkLevel84ElectricalBindings`, `SparkLevel84TerminalMapper`, `SparkLevel84QuaternionRigidBody`, `SparkLevel84OrbitalReference`, `SparkLevel84DisturbanceTorqueModel`, `SparkLevel84ReactionWheelModel`, `SparkLevel84WheelAllocation`, `SparkLevel84WheelMotorDrivers`, `SparkLevel84MagnetorquerModel`, `SparkLevel84MomentumDesaturation`, `SparkLevel84GyroIMU`, `SparkLevel84MagnetometerSensor`, `SparkLevel84CoarseSunSensor`, `SparkLevel84EarthHorizonSensor`, `SparkLevel84StarTracker`, `SparkLevel84StarCatalogMatcher`, `SparkLevel84AttitudeEstimator`, `SparkLevel84SlewPlanner`, `SparkLevel84AttitudeFlightController`, `SparkLevel84SolarWingFlexibleModes`, `SparkLevel84OpticalBoresight`, `SparkLevel84GroundStationPassScheduler`, `SparkLevel84OpticalLinkModel`, `SparkLevel84MotionSafetyPLC`, `SparkLevel84SunSafeMode`, `SparkLevel84DisturbanceController`, `SparkLevel84W097Recovery`, `SparkLevel84ObjectiveTracker`, `SparkLevel84SCADA`, `SparkLevel84VisualRigAdapter`. Use interfaces and central state authority rather than dozens of independent contradictory MonoBehaviours. Names are suggestions, not guaranteed existing classes.

## 52. ONE-CLICK UNITY LEVEL 84 BUILDER

Create **`Tools/Project Spark/Levels/Create Level 84`** with idempotent behavior. The builder must inspect actual Project Spark APIs and:

1. Validate original ZIP/package structure, model files, art scaling and 58 pivots.
2. Create/update actual `Level_84.asset` and `Level_84_Layout.prefab` using **real project schemas** and registration.
3. Place exactly 76 common-origin modular FBXs under `L84_HD_VISUAL_ART`; do not double-instantiate the assembled showcase.
4. Import/map 12 procedural PBR sets and 36 texture maps; warn about unfinished UV/LODs.
5. Preserve 218 original visual sockets and reconcile 284 planned wire endpoints to safe functional child ports.
6. Create 142 **unconnected** puzzle wire declarations and live terminal/network observers.
7. Set up 5 V control domain, distinct actuator energy model and independent motion safety.
8. Instantiate/configure rigid-body quaternion plant, wheel models, magnetorquers, sensors, estimator, flight controller, time/target geometry, optical link and three faults.
9. Provide real W097 trust boundary, physical disconnect/reconnect, stale sampling, fault latch, recalibration/reset and post-repair two-pass mission.
10. Bind six exact source-objective titles to the real `LevelGamePlayManager` with evidence-based evaluator logic.
11. Bind actual-state channels to existing `SparkLevel84VisualRig`, SCADA, animation/VFX, interactables and collision proxies.
12. Run static structural validation, dependency/reference checks and generate clear CSV/JSON/report outputs.
13. Save new prefabs/assets without duplicating solvers, functional terminals, level definitions or custom gameplay on rerun.
14. Preserve Levels 1–83, back up any migration-sensitive content, and report all changed files.

The three included art-only menus remain available, but may not replace this full builder. Use Undo/Editor transactions, deterministic object IDs, predictable asset paths, scoped updates and a dry-run/validate option.

## 53. ACCEPTANCE TESTS — ART, UNITY AND TERMINAL MAPPING

Run tests in the actual Unity project where accessible and record **PASS / FAIL / NOT RUN** truthfully:

1. 76 numbered binary FBXs and 76 GLBs present and readable.
2. One assembled showcase FBX/GLB exists but is not duplicated with the modular set.
3. Correct centimetre-to-metre conversion performed once; ~27.0 m width, ~10.472 m height.
4. Exactly 58 valid unique mesh-owning `ANIM_*` parents bound to expected equipment/axis.
5. 218 source `SOCKET_P###` visual anchors preserved and verified.
6. 142 unique wire IDs/rows match the authoritative CSV, and all start player-disconnected.
7. All 284 planned endpoints reconciled; 47 repeated-art-anchor cases receive safe functional mapping.
8. P064 has a dedicated trusted W097 endpoint; magnetic, gyro and Earth-horizon inputs are not shorted to it.
9. Real SparkTerminal directions, fanout, 5 V power/returns and distinct signal domains verified.
10. W097 connection/removal mutates actual electrical graph topology.
11. Builder rerun does not duplicate models, terminals, objects, solvers, PLCs or six objectives.
12. Levels 1–83 still load and their definitions remain intact.
13. Optional art helper methods/menus are compiled/validated for the installed Unity version.
14. Art root replacement leaves gameplay, connectors and scenario state untouched.

## 54. ACCEPTANCE TESTS — RIGID BODY AND ACTUATORS

15. True body attitude evolves under angular velocity even if visuals are hidden.
16. Torque-free evolution conserves angular momentum within documented integrator tolerance.
17. Reaction-wheel body/rotor momentum exchange preserves total momentum absent external torque.
18. Each X/Y/Z/skew rotor follows finite acceleration and measured RPM.
19. Current/speed/temperature limits clip commands and produce visible diagnostic residuals.
20. Four-wheel torque allocation produces expected 3D torque and handles redundant wheel/saturation.
21. Skew backup restores authority only when available wheel axes remain controllable.
22. B-field rotates correctly between inertial and body frames.
23. Magnetorquer torque equals `m × B` and is zero along the field direction.
24. Momentum unload transfers angular momentum through nonzero external magnetic torque; no instant wheel reset.
25. Disturbance torques change actual body motion, not only SCADA labels.
26. Test-gimbal ring animation never writes rigid-body torque or quaternion truth.

## 55. ACCEPTANCE TESTS — SENSORS, ESTIMATION, POINTING

27. Gyro samples include actual bias/noise/timestamps, with reproducible seeded fault behavior.
28. Stationary/reference calibration reduces measured bias within configured criteria.
29. Star tracker cannot lock during unsuitable high body rates or invalid baffle/visibility.
30. Star-pattern matches are required, time-stamped and confidence-qualified.
31. Stale star samples increase estimator uncertainty rather than remaining valid forever.
32. Quaternion normalization/±q sign handling/shortest-arc errors are correct.
33. Sun/horizon/magnetometer provide their proper limited observability, not perfect ground truth.
34. Desired vs actual vs estimated states remain distinct in telemetry and controller inputs.
35. Three-axis slews obey finite torque/rate/acceleration and settling requirements.
36. A saturated actuator cannot instantly achieve a requested attitude.
37. Fine pointing fails when W097 or estimator confidence is unqualified.
38. Solar flex creates real boresight jitter and damping reduces it with finite dynamics.

## 56. ACCEPTANCE TESTS — TWO-PASS OPTICAL MISSION

39. Ground-station geometry generates two actual distinct scheduled windows.
40. No lock/data credit outside a valid line-of-sight interval.
41. Actual spacecraft boresight, not just reference target, controls acquisition and measured error.
42. Required link dwell, RMS jitter, transmitted data and power are actually measured.
43. Replaying one pass cannot count as two independent passes.
44. Momentum safety and estimator freshness are enforced throughout each pass.
45. Missing payload alignment / excessive flex jitter / unavailable power blocks qualification.
46. Two-pass qualification stores unique pass IDs, time windows and measurement evidence.

## 57. ACCEPTANCE TESTS — THREE DISTURBANCES, W097 AND 6/6

47. Gyro bias fault changes real measured rate/estimator residual and is physically recalibrated.
48. Wheel saturation fault changes RPM/momentum headroom; magnetic desaturation obeys B-field physics.
49. Solar-wing jitter fault changes actual optical error and is fixed without fake state jumps.
50. Fresh two-pass qualification after all three repairs is mandatory for Objective 5.
51. Physical W097 disconnect stops *new* trusted quaternion measurements and ages old samples.
52. Loss of trusted feedback withdraws fine-pointing authority and triggers latched safe mode.
53. Physical body attitude/rates/momentum remain continuous on W097 loss.
54. Secondary tracker, SCADA cache and diagnostic W096 cannot bypass W097 trust.
55. Independent E-stop/current/thermal protection remains active throughout loss/recovery.
56. W097 reset refused before valid topology, fresh catalog samples, calibration and safety checks.
57. Reconnection alone does not clear fault, enable fine pointing or award Objective 6.
58. Explicit reset after proper checks enables qualified recommissioning; no retroactive data.
59. Objectives 1–5 historical records persist after deliberate W097 fault.
60. Brand-new two-pass run with new IDs and data after repair is required for Objective 6.
61. Actual `LevelGamePlayManager` reaches 6/6 only after the full validated sequence.
62. Saving/loading and rebuilding cannot counterfeit 6/6 or replay stale samples.
63. Compilation, Editor/Play Mode, wiring, fault, scene and regression results are reported with evidence.

## 58. ERROR MESSAGES, FAILURES AND RECOVERY DIAGNOSTICS

Emit actionable errors for: missing FBX/GLB, wrong import scale, duplicated rig/asset root, missing animation pivot, P-anchor not found, repeated socket ambiguous, terminal wrong direction/signal domain, 5 V return absent, unsafe fanout, wrong W097 destination, invalid topology, wheel current/RPM/thermal overrange, torque allocation rank deficiency, no B-field for unload, magnetic field sensor stale, gyroscope bias and calibration expired, star-tracker baffle blocked, angular rate too high for catalog match, low star confidence, quaternion invalid/stale, estimator divergence, safe-mode active, pointing jitter high, boresight miscalibration, missed pass window, insufficient data transfer, unresolved flex resonance, fault not actually repaired, W097 topology broken, external reset requested prematurely, stale post-repair certificate, historical objectives unexpectedly cleared, older levels changed. Report which exact measured value, model, terminal, wire or `runId` caused failure rather than a generic red `FAIL`.

## 59. COMPLETE REQUIRED PLAYER GAMEPLAY FLOW

**CONNECT 142 REAL WIRES → RECONCILE 218 ART SOCKET LOCATIONS / 284 LOGICAL ENDPOINT OCCURRENCES → QUALIFY 5 V SUPPLY AND ACTUAL RETURNS → VERIFY INDEPENDENT MOTION SAFETY AND ESTOP → COMMISSION FOUR WHEELS AND TACHOMETERS/CURRENT SENSORS → COMMISSION THREE MAGNETIC TORQUERS AND MAGNETOMETER → CALIBRATE GYROSCOPE → START PHYSICALLY SIMULATED DETUMBLE → MEASURE ACTUAL ANGULAR RATE AND WHEEL MOMENTUM → ACQUIRE COARSE SUN/MAGNETIC/HORIZON DIRECTIONS → OPEN PRIMARY STAR-TRACKER BAFFLE → CAPTURE FRESH STARS → MATCH CATALOG → VALIDATE PHYSICAL W097 QUATERNION → CALIBRATE TRACKER AND ESTIMATOR → ENABLE THREE-AXIS QUATERNION FLIGHT CONTROLLER → SLEW TO TARGET WITH FINITE TORQUE → SETTLE TRUE POINTING/JITTER → MANAGE/UNLOAD REACTION WHEEL MOMENTUM THROUGH ACTUAL m×B → ALIGN OPTICAL PAYLOAD BORESIGHT → WAIT FOR REAL GROUND-STATION PASS A → ACQUIRE AND HOLD BEAM → TRANSMIT VALID DATA → MAINTAIN MOMENTUM RESERVE → EXECUTE SEPARATE PASS B → CERTIFY TWO PASSES → INJECT GYRO BIAS → DIAGNOSE/CALIBRATE/REQUALIFY → SATURATE WHEEL → DIAGNOSE/DESATURATE WITH MAGNETORQUER → INJECT FLEXIBLE SOLAR-WING JITTER → MEASURE/REPAIR ACTIVE DAMPING → COMPLETE FRESH TWO-PASS QUALIFICATION → PHYSICALLY DISCONNECT W097 → EXPIRE TRUSTED QUATERNION → WITHDRAW FINE OPTICAL CONTROL → LATCH CRITICAL FAULT → ENTER BOUNDED COARSE SUN-SAFE MODE → PHYSICALLY RECONNECT W097 → ACQUIRE NEW STAR CATALOG SAMPLES → RECALIBRATE TRACKER/ESTIMATOR → VERIFY INDEPENDENT SAFETY → EXPLICIT RESET → RESUME FINE CONTROL → EXECUTE AN ENTIRELY NEW PASS A AND PASS B WITH REAL OPTICAL LINK/DATA → AUTHORITATIVE `LevelGamePlayManager` 6/6.**

## 60. DELIVERABLES, REPORTS AND PROJECT HANDOFF

Provide/importable real Unity runtime source, one-click Level 84 Editor builder, `Level_84.asset`, `Level_84_Layout.prefab`, 76 correct modular art instances, 58 bound pivots, 218 preserved art anchors, `Level84_TerminalReconciliationReport.csv` mapping split functional ports, 142 live player-wire declarations, proper Spark terminals/returns, authoritative rigid-body/wheel/magnetorquer dynamics, real sensors/fusion/PLC, optical mission/pass scheduler, three disturbances and repairs, W097 physical fault sequence, six evidence-based objective evaluators, SCADA and actual-state VFX, editor and gameplay tests, and honest PASS/FAIL/NOT RUN results. Preserve Levels 1–83.

## 61. FINAL ACCEPTANCE / COMPLETION CONTRACT

**Level 84 is complete only when the actual Unity game has been compiled and run**, a player can wire the ADCS, detumble genuine rigid-body motion, acquire trusted star-tracker attitude, control all three axes, desaturate wheels without violating magnetic torque physics, hold real optical beam lock in two scheduled windows, recover the three distinct disturbances, physically remove and restore W097 with coarse safe-mode and explicit reset, and complete **new** two-pass qualification after the critical feedback repair, with the real `LevelGamePlayManager` showing **6/6**. Otherwise describe precisely which functionality is implemented and which remains untested or incomplete.

**PROJECT SPARK — LEVEL 84: DEFY THE TUMBLE. CONTROL THE MOMENTUM. FIND THE STARS. HOLD THE BEAM. RESTORE TRUSTED ATTITUDE.**

---

# SOURCE-VERIFIED ANNEXES (PRESERVE EXACT SOURCE NAMES AND IDs)

**Reference:** Every record below is pulled from the provided Level 84 ZIP. These are **art assets, pivot nodes, conceptual wire definitions, and proposal anchors**, not pre-existing Unity gameplay components. Proposed algorithms in the main body are implementation requirements.

## ANNEX A — EXACT 76 MODULAR MODEL FILENAMES, COUNTS AND ROLE

| # | Exact FBX | Exact GLB | Triangles | Mesh parts |
|---:|---|---|---:|---:|
| 00 | `00_L84_CelestialDynamicsArenaBase.fbx` | `00_L84_CelestialDynamicsArenaBase.glb` | 3,868 | 24 |
| 01 | `01_L84_PanoramicStarfieldSphere.fbx` | `01_L84_PanoramicStarfieldSphere.glb` | 10,472 | 37 |
| 02 | `02_L84_ThreeAxisSpacecraftTestRig.fbx` | `02_L84_ThreeAxisSpacecraftTestRig.glb` | 17,440 | 17 |
| 03 | `03_L84_RotationalTestRigPedestal.fbx` | `03_L84_RotationalTestRigPedestal.glb` | 3,820 | 10 |
| 04 | `04_L84_SpacecraftAttitudeTestVehicle.fbx` | `04_L84_SpacecraftAttitudeTestVehicle.glb` | 424 | 4 |
| 05 | `05_L84_ProtectedAvionicsChassis.fbx` | `05_L84_ProtectedAvionicsChassis.glb` | 444 | 8 |
| 06 | `06_L84_FourChipletMissionPayload.fbx` | `06_L84_FourChipletMissionPayload.glb` | 444 | 8 |
| 07 | `07_L84_CoPackagedOpticalTransmitter.fbx` | `07_L84_CoPackagedOpticalTransmitter.glb` | 1,380 | 13 |
| 08 | `08_L84_PortSolarArrayWing.fbx` | `08_L84_PortSolarArrayWing.glb` | 4,188 | 28 |
| 09 | `09_L84_StarboardSolarArrayWing.fbx` | `09_L84_StarboardSolarArrayWing.glb` | 4,284 | 30 |
| 10 | `10_L84_HolographicEarthReferenceSphere.fbx` | `10_L84_HolographicEarthReferenceSphere.glb` | 4,288 | 3 |
| 11 | `11_L84_StellarNavigationCatalogDome.fbx` | `11_L84_StellarNavigationCatalogDome.glb` | 3,360 | 20 |
| 12 | `12_L84_InertialReferenceTargetHologram.fbx` | `12_L84_InertialReferenceTargetHologram.glb` | 3,888 | 3 |
| 13 | `13_L84_OpticalGroundStationAimReticle.fbx` | `13_L84_OpticalGroundStationAimReticle.glb` | 3,900 | 4 |
| 14 | `14_L84_OrbitalEphemerisDisplayRail.fbx` | `14_L84_OrbitalEphemerisDisplayRail.glb` | 456 | 6 |
| 15 | `15_L84_AngularMomentumVectorHologram.fbx` | `15_L84_AngularMomentumVectorHologram.glb` | 3,900 | 4 |
| 16 | `16_L84_SpacecraftBodyAxesHologram.fbx` | `16_L84_SpacecraftBodyAxesHologram.glb` | 3,984 | 5 |
| 17 | `17_L84_ReactionWheelClusterHousing.fbx` | `17_L84_ReactionWheelClusterHousing.glb` | 2,904 | 2 |
| 18 | `18_L84_ReactionWheelX.fbx` | `18_L84_ReactionWheelX.glb` | 3,380 | 16 |
| 19 | `19_L84_ReactionWheelY.fbx` | `19_L84_ReactionWheelY.glb` | 3,284 | 14 |
| 20 | `20_L84_ReactionWheelZ.fbx` | `20_L84_ReactionWheelZ.glb` | 3,380 | 16 |
| 21 | `21_L84_SkewBackupReactionWheel.fbx` | `21_L84_SkewBackupReactionWheel.glb` | 3,284 | 14 |
| 22 | `22_L84_ReactionWheelMotorDriverRack.fbx` | `22_L84_ReactionWheelMotorDriverRack.glb` | 1,128 | 24 |
| 23 | `23_L84_ReactionWheelTachometerArray.fbx` | `23_L84_ReactionWheelTachometerArray.glb` | 924 | 19 |
| 24 | `24_L84_ReactionWheelMotorCurrentMonitor.fbx` | `24_L84_ReactionWheelMotorCurrentMonitor.glb` | 804 | 15 |
| 25 | `25_L84_ReactionWheelThermalControlModule.fbx` | `25_L84_ReactionWheelThermalControlModule.glb` | 720 | 14 |
| 26 | `26_L84_MagneticTorqueRodX.fbx` | `26_L84_MagneticTorqueRodX.glb` | 7,516 | 9 |
| 27 | `27_L84_MagneticTorqueRodY.fbx` | `27_L84_MagneticTorqueRodY.glb` | 7,516 | 9 |
| 28 | `28_L84_MagneticTorqueRodZ.fbx` | `28_L84_MagneticTorqueRodZ.glb` | 7,516 | 9 |
| 29 | `29_L84_MagneticTorqueRodDriveController.fbx` | `29_L84_MagneticTorqueRodDriveController.glb` | 1,380 | 27 |
| 30 | `30_L84_TriaxialMagnetometer.fbx` | `30_L84_TriaxialMagnetometer.glb` | 1,560 | 19 |
| 31 | `31_L84_OrbitalMagneticFieldEmulator.fbx` | `31_L84_OrbitalMagneticFieldEmulator.glb` | 1,356 | 14 |
| 32 | `32_L84_HolographicMagneticFieldLines.fbx` | `32_L84_HolographicMagneticFieldLines.glb` | 4,128 | 4 |
| 33 | `33_L84_MomentumDesaturationController.fbx` | `33_L84_MomentumDesaturationController.glb` | 1,296 | 26 |
| 34 | `34_L84_PrecisionGyroscopeIMU.fbx` | `34_L84_PrecisionGyroscopeIMU.glb` | 1,124 | 19 |
| 35 | `35_L84_GyroscopeCalibrationStation.fbx` | `35_L84_GyroscopeCalibrationStation.glb` | 752 | 12 |
| 36 | `36_L84_GyroBiasThermalFaultEmulator.fbx` | `36_L84_GyroBiasThermalFaultEmulator.glb` | 552 | 13 |
| 37 | `37_L84_PrimaryStarTracker.fbx` | `37_L84_PrimaryStarTracker.glb` | 1,700 | 16 |
| 38 | `38_L84_StarTrackerOpticalBaffle.fbx` | `38_L84_StarTrackerOpticalBaffle.glb` | 1,316 | 8 |
| 39 | `39_L84_SecondaryDiagnosticStarTracker.fbx` | `39_L84_SecondaryDiagnosticStarTracker.glb` | 1,412 | 10 |
| 40 | `40_L84_StarCatalogPatternMatcher.fbx` | `40_L84_StarCatalogPatternMatcher.glb` | 720 | 16 |
| 41 | `41_L84_CoarseSunSensorCluster.fbx` | `41_L84_CoarseSunSensorCluster.glb` | 1,412 | 10 |
| 42 | `42_L84_EarthHorizonSensor.fbx` | `42_L84_EarthHorizonSensor.glb` | 1,508 | 12 |
| 43 | `43_L84_AttitudeStateEstimator.fbx` | `43_L84_AttitudeStateEstimator.glb` | 1,188 | 25 |
| 44 | `44_L84_ComplementaryFusionDisplay.fbx` | `44_L84_ComplementaryFusionDisplay.glb` | 708 | 15 |
| 45 | `45_L84_HolographicQuaternionAttitudeSphere.fbx` | `45_L84_HolographicQuaternionAttitudeSphere.glb` | 3,996 | 6 |
| 46 | `46_L84_EstimatorUncertaintyDisplay.fbx` | `46_L84_EstimatorUncertaintyDisplay.glb` | 4,284 | 12 |
| 47 | `47_L84_AttitudeFlightController.fbx` | `47_L84_AttitudeFlightController.glb` | 1,872 | 40 |
| 48 | `48_L84_ThreeAxisTorqueAllocationComputer.fbx` | `48_L84_ThreeAxisTorqueAllocationComputer.glb` | 720 | 16 |
| 49 | `49_L84_SlewTrajectoryPlanner.fbx` | `49_L84_SlewTrajectoryPlanner.glb` | 1,020 | 20 |
| 50 | `50_L84_PrecisionPointingErrorAnalyzer.fbx` | `50_L84_PrecisionPointingErrorAnalyzer.glb` | 924 | 18 |
| 51 | `51_L84_InertialReferenceClock.fbx` | `51_L84_InertialReferenceClock.glb` | 516 | 11 |
| 52 | `52_L84_ExternalDisturbanceTorqueInjector.fbx` | `52_L84_ExternalDisturbanceTorqueInjector.glb` | 816 | 15 |
| 53 | `53_L84_GravityGradientTorqueEmulator.fbx` | `53_L84_GravityGradientTorqueEmulator.glb` | 624 | 11 |
| 54 | `54_L84_OrbitalDragTorqueEmulator.fbx` | `54_L84_OrbitalDragTorqueEmulator.glb` | 624 | 11 |
| 55 | `55_L84_SolarRadiationPressureTorqueModel.fbx` | `55_L84_SolarRadiationPressureTorqueModel.glb` | 720 | 13 |
| 56 | `56_L84_SolarWingFlexibleModeEmulator.fbx` | `56_L84_SolarWingFlexibleModeEmulator.glb` | 648 | 13 |
| 57 | `57_L84_StructuralVibrationSensor.fbx` | `57_L84_StructuralVibrationSensor.glb` | 912 | 17 |
| 58 | `58_L84_ActivePointingVibrationFilter.fbx` | `58_L84_ActivePointingVibrationFilter.glb` | 732 | 14 |
| 59 | `59_L84_OpticalLineOfSightSensor.fbx` | `59_L84_OpticalLineOfSightSensor.glb` | 1,700 | 16 |
| 60 | `60_L84_GroundStationTargetProjector.fbx` | `60_L84_GroundStationTargetProjector.glb` | 1,508 | 12 |
| 61 | `61_L84_OpticalLinkAcquisitionFixture.fbx` | `61_L84_OpticalLinkAcquisitionFixture.glb` | 1,508 | 12 |
| 62 | `62_L84_OpticalSignalQualityTelemetry.fbx` | `62_L84_OpticalSignalQualityTelemetry.glb` | 912 | 20 |
| 63 | `63_L84_MissionPayloadBoresightReference.fbx` | `63_L84_MissionPayloadBoresightReference.glb` | 1,304 | 7 |
| 64 | `64_L84_SunSafeAttitudeController.fbx` | `64_L84_SunSafeAttitudeController.glb` | 720 | 16 |
| 65 | `65_L84_AttitudeSafeModeSupervisor.fbx` | `65_L84_AttitudeSafeModeSupervisor.glb` | 900 | 19 |
| 66 | `66_L84_ReactionWheelSaturationAlarm.fbx` | `66_L84_ReactionWheelSaturationAlarm.glb` | 636 | 14 |
| 67 | `67_L84_ReactionWheelDesaturationSupervisor.fbx` | `67_L84_ReactionWheelDesaturationSupervisor.glb` | 708 | 15 |
| 68 | `68_L84_CriticalAttitudeFeedbackStation.fbx` | `68_L84_CriticalAttitudeFeedbackStation.glb` | 912 | 20 |
| 69 | `69_L84_IntegratedADCSPLCRack.fbx` | `69_L84_IntegratedADCSPLCRack.glb` | 1,476 | 30 |
| 70 | `70_L84_Regulated5VControlSupply.fbx` | `70_L84_Regulated5VControlSupply.glb` | 720 | 15 |
| 71 | `71_L84_IndependentMotionSafetyStation.fbx` | `71_L84_IndependentMotionSafetyStation.glb` | 1,104 | 23 |
| 72 | `72_L84_HolographicAttitudeControlSCADA.fbx` | `72_L84_HolographicAttitudeControlSCADA.glb` | 1,008 | 22 |
| 73 | `73_L84_ADCSCertificationBeacon.fbx` | `73_L84_ADCSCertificationBeacon.glb` | 1,520 | 6 |
| 74 | `74_L84_EmergencyAttitudeStopStation.fbx` | `74_L84_EmergencyAttitudeStopStation.glb` | 636 | 14 |
| 75 | `75_L84_AttitudeFlightDataRecorder.fbx` | `75_L84_AttitudeFlightDataRecorder.glb` | 720 | 16 |

Assembled reference **only**: `Models/Level84_Complete_ArtKit_Showcase.fbx` and `GLB/Level84_Complete_ArtKit_Showcase.glb`. Do not instantiate both reference and modular assets together.

## ANNEX B — ALL 58 VERIFIED ANIMATION PIVOT PARENTS

| # | Pivot parent | Owning model | Kind | Axis | World XYZ (m) |
|---:|---|---|---|---|---|
| 01 | `ANIM_OUTER_YAW_RING` | `L84_ThreeAxisSpacecraftTestRig` | rotation | `y` | `(0, 5.06, -1.25)` |
| 02 | `ANIM_MIDDLE_PITCH_RING` | `L84_ThreeAxisSpacecraftTestRig` | rotation | `x` | `(0, 5.06, -1.25)` |
| 03 | `ANIM_INNER_ROLL_RING` | `L84_ThreeAxisSpacecraftTestRig` | rotation | `z` | `(0, 5.06, -1.25)` |
| 04 | `ANIM_SPACECRAFT_BODY_ATTITUDE` | `L84_SpacecraftAttitudeTestVehicle` | rotation | `quaternion` | `(0, 5.06, -1.25)` |
| 05 | `ANIM_EARTH_REFERENCE_ROTATION` | `L84_HolographicEarthReferenceSphere` | rotation | `y` | `(8.0, 7.55, -5.3)` |
| 06 | `ANIM_STARFIELD_DOME` | `L84_PanoramicStarfieldSphere` | rotation | `y` | `(0, 6.4, -5.2)` |
| 07 | `ANIM_GROUND_STATION_TARGET` | `L84_OpticalGroundStationAimReticle` | rotation | `y` | `(9.5, 7.6, -5.0)` |
| 08 | `ANIM_ORBIT_REFERENCE_GRID` | `L84_OrbitalEphemerisDisplayRail` | translation | `x` | `(0, 2.4, -5.28)` |
| 09 | `ANIM_ANGULAR_MOMENTUM_VECTOR` | `L84_AngularMomentumVectorHologram` | indicator | `none` | `(0, 7.5, -4.5)` |
| 10 | `ANIM_TEST_RIG_LOCK` | `L84_RotationalTestRigPedestal` | indicator | `none` | `(0, 1.3, -1.25)` |
| 11 | `ANIM_WHEEL_X_ROTOR` | `L84_ReactionWheelX` | rotation | `z` | `(-11.55, 3.15, 0.15)` |
| 12 | `ANIM_WHEEL_Y_ROTOR` | `L84_ReactionWheelY` | rotation | `z` | `(-10.18, 3.15, 0.15)` |
| 13 | `ANIM_WHEEL_Z_ROTOR` | `L84_ReactionWheelZ` | rotation | `z` | `(-8.8, 3.15, 0.15)` |
| 14 | `ANIM_WHEEL_SKEW_ROTOR` | `L84_SkewBackupReactionWheel` | rotation | `z` | `(-7.43, 3.15, 0.15)` |
| 15 | `ANIM_TORQUE_ROD_X_VECTOR` | `L84_MagneticTorqueRodX` | indicator | `none` | `(-2.8, 2.0, -4.8)` |
| 16 | `ANIM_TORQUE_ROD_Y_VECTOR` | `L84_MagneticTorqueRodY` | indicator | `none` | `(0, 2.0, -4.8)` |
| 17 | `ANIM_TORQUE_ROD_Z_VECTOR` | `L84_MagneticTorqueRodZ` | indicator | `none` | `(2.8, 2.0, -4.8)` |
| 18 | `ANIM_WHEEL_DRIVER_X_PANEL` | `L84_ReactionWheelMotorDriverRack` | rotation | `y` | `(-11.65, 1.55, 4.0)` |
| 19 | `ANIM_WHEEL_DRIVER_Y_PANEL` | `L84_ReactionWheelMotorDriverRack` | rotation | `y` | `(-11.2, 1.55, 4.0)` |
| 20 | `ANIM_WHEEL_DRIVER_Z_PANEL` | `L84_ReactionWheelMotorDriverRack` | rotation | `y` | `(-10.75, 1.55, 4.0)` |
| 21 | `ANIM_WHEEL_TACHOMETER_SCAN` | `L84_ReactionWheelTachometerArray` | indicator | `none` | `(-9.95, 1.55, 4.0)` |
| 22 | `ANIM_WHEEL_COOLING_FAN` | `L84_ReactionWheelThermalControlModule` | translation | `x` | `(-7.45, 1.55, 4.0)` |
| 23 | `ANIM_PRIMARY_STAR_TRACKER_APERTURE` | `L84_PrimaryStarTracker` | rotation | `y` | `(9.1, 4.2, 0.3)` |
| 24 | `ANIM_STAR_TRACKER_BAFFLE` | `L84_StarTrackerOpticalBaffle` | rotation | `y` | `(10.4, 4.12, 0.3)` |
| 25 | `ANIM_SECONDARY_TRACKER` | `L84_SecondaryDiagnosticStarTracker` | translation | `x` | `(11.55, 4.2, 0.3)` |
| 26 | `ANIM_GYRO_CALIBRATION_SPINDLE` | `L84_GyroscopeCalibrationStation` | rotation | `y` | `(8.7, 2.3, 3.85)` |
| 27 | `ANIM_GYRO_BIAS_DISPLAY` | `L84_GyroBiasThermalFaultEmulator` | indicator | `none` | `(10.0, 2.3, 3.85)` |
| 28 | `ANIM_SUN_SENSOR_SCAN` | `L84_CoarseSunSensorCluster` | indicator | `none` | `(10.45, 2.6, 0.4)` |
| 29 | `ANIM_EARTH_HORIZON_SCAN` | `L84_EarthHorizonSensor` | indicator | `none` | `(11.65, 2.6, 0.4)` |
| 30 | `ANIM_MAGNETOMETER_SCAN` | `L84_TriaxialMagnetometer` | indicator | `none` | `(-5.2, 3.6, -4.84)` |
| 31 | `ANIM_STAR_CATALOG_MATCH` | `L84_StarCatalogPatternMatcher` | translation | `x` | `(9.15, 2.6, 0.4)` |
| 32 | `ANIM_ATTITUDE_QUATERNION` | `L84_HolographicQuaternionAttitudeSphere` | translation | `x` | `(3.75, 7.45, -5.12)` |
| 33 | `ANIM_ESTIMATOR_UNCERTAINTY_CONE` | `L84_EstimatorUncertaintyDisplay` | indicator | `none` | `(-2.95, 1.65, 4.7)` |
| 34 | `ANIM_OPTICAL_LINE_OF_SIGHT` | `L84_OpticalLineOfSightSensor` | indicator | `none` | `(7.9, 3.75, -5.0)` |
| 35 | `ANIM_FLIGHT_CONTROLLER_HOLOGRAM` | `L84_AttitudeFlightController` | indicator | `none` | `(-1.65, 1.65, 4.7)` |
| 36 | `ANIM_SLEW_TRAJECTORY_DISPLAY` | `L84_SlewTrajectoryPlanner` | indicator | `none` | `(0.95, 1.65, 4.7)` |
| 37 | `ANIM_TORQUE_ALLOCATION_DISPLAY` | `L84_ThreeAxisTorqueAllocationComputer` | indicator | `none` | `(-0.35, 1.65, 4.7)` |
| 38 | `ANIM_REACTION_WHEEL_MOMENTUM_BARS` | `L84_ReactionWheelTachometerArray` | indicator | `none` | `(-9.95, 1.68, 4.28)` |
| 39 | `ANIM_MOMENTUM_DESAT_ARC` | `L84_MomentumDesaturationController` | indicator | `none` | `(2.15, 3.5, -5.1)` |
| 40 | `ANIM_MAGNETIC_FIELD_LINES` | `L84_HolographicMagneticFieldLines` | indicator | `none` | `(0, 7.9, -5.3)` |
| 41 | `ANIM_POINTING_TARGET_RETICLE` | `L84_PrecisionPointingErrorAnalyzer` | indicator | `none` | `(2.25, 1.65, 4.7)` |
| 42 | `ANIM_OPTICAL_LINK_LOCK` | `L84_OpticalLinkAcquisitionFixture` | indicator | `none` | `(10.35, 3.75, -5.0)` |
| 43 | `ANIM_STRUCTURAL_MODE_TRACE` | `L84_SolarWingFlexibleModeEmulator` | indicator | `none` | `(4.25, 3.75, -5.0)` |
| 44 | `ANIM_VIBRATION_FILTER_DISPLAY` | `L84_ActivePointingVibrationFilter` | indicator | `none` | `(6.65, 3.75, -5.0)` |
| 45 | `ANIM_GYRO_BIAS_FAULT` | `L84_GyroBiasThermalFaultEmulator` | indicator | `none` | `(10.0, 2.3, 3.85)` |
| 46 | `ANIM_WHEEL_SATURATION_FAULT` | `L84_ReactionWheelSaturationAlarm` | indicator | `none` | `(-11.22, 4.25, -0.55)` |
| 47 | `ANIM_SOLAR_WING_FLEX_FAULT` | `L84_SolarWingFlexibleModeEmulator` | indicator | `none` | `(4.25, 3.75, -5.0)` |
| 48 | `ANIM_SUN_SAFE_ACQUISITION` | `L84_SunSafeAttitudeController` | indicator | `none` | `(-5.42, 3.65, -4.8)` |
| 49 | `ANIM_EMERGENCY_ATTITUDE_STOP` | `L84_EmergencyAttitudeStopStation` | rotation | `y` | `(11.15, 1.52, 4.85)` |
| 50 | `ANIM_CRITICAL_W097_INDICATOR` | `L84_CriticalAttitudeFeedbackStation` | translation | `x` | `(10.3, 1.64, 4.7)` |
| 51 | `ANIM_INDEPENDENT_SAFETY_PANEL` | `L84_IndependentMotionSafetyStation` | rotation | `y` | `(7.6, 1.65, 4.7)` |
| 52 | `ANIM_FLIGHT_DATA_RECORDER` | `L84_AttitudeFlightDataRecorder` | translation | `x` | `(8.95, 1.65, 4.7)` |
| 53 | `ANIM_OPTICAL_DOWNLINK_DISPLAY` | `L84_OpticalSignalQualityTelemetry` | indicator | `none` | `(11.55, 3.75, -5.0)` |
| 54 | `ANIM_MISSION_TIMELINE` | `L84_OrbitalEphemerisDisplayRail` | indicator | `none` | `(0, 2.4, -5.28)` |
| 55 | `ANIM_GROUND_PASS_WINDOW` | `L84_GroundStationTargetProjector` | indicator | `none` | `(9.1, 3.75, -5.0)` |
| 56 | `ANIM_POWER_RESERVE_DISPLAY` | `L84_Regulated5VControlSupply` | indicator | `none` | `(6.3, 1.65, 4.7)` |
| 57 | `ANIM_ADCS_SCADA` | `L84_HolographicAttitudeControlSCADA` | indicator | `none` | `(0, 7.8, -5.3)` |
| 58 | `ANIM_ADCS_CERTIFICATION_BEACON` | `L84_ADCSCertificationBeacon` | indicator | `none` | `(0, 8.6, -5.2)` |

`Kind=indicator` means a read-only display/emissive anchor, not a real actuator; all pivots are mesh-owning parent nodes.

## ANNEX C — COMPLETE W001–W142 PLANNED PLAYER WIRING (SOURCE EXACT)

| Wire | Source model / art socket / proposed port | Destination model / art socket / proposed port | Kind | Source concept purpose |
|---|---|---|---|---|
| **W001** | `L84_Regulated5VControlSupply` / **P001** / `ADCS_PLC_LOGIC_POWER_OUT` | `L84_IntegratedADCSPLCRack` / **P002** / `ADCS_PLC_LOGIC_POWER_IN` | `POWER_CONTROL` | ADCS PLC logic power |
| **W002** | `L84_Regulated5VControlSupply` / **P001** / `FLIGHT_CONTROLLER_5V_SUPPLY_OUT` | `L84_AttitudeFlightController` / **P003** / `FLIGHT_CONTROLLER_5V_SUPPLY_IN` | `POWER_CONTROL` | Flight controller 5V supply |
| **W003** | `L84_Regulated5VControlSupply` / **P001** / `STATE_ESTIMATOR_5V_LOGIC_OUT` | `L84_AttitudeStateEstimator` / **P004** / `STATE_ESTIMATOR_5V_LOGIC_IN` | `POWER_CONTROL` | State estimator 5V logic |
| **W004** | `L84_Regulated5VControlSupply` / **P001** / `WHEEL_MOTOR_DRIVER_CONTROL_LOGIC_OUT` | `L84_ReactionWheelMotorDriverRack` / **P005** / `WHEEL_MOTOR_DRIVER_CONTROL_LOGIC_IN` | `POWER_CONTROL` | Wheel motor driver control logic |
| **W005** | `L84_Regulated5VControlSupply` / **P001** / `MAGNETORQUER_DRIVER_CONTROL_LOGIC_OUT` | `L84_MagneticTorqueRodDriveController` / **P006** / `MAGNETORQUER_DRIVER_CONTROL_LOGIC_IN` | `POWER_CONTROL` | Magnetorquer driver control logic |
| **W006** | `L84_Regulated5VControlSupply` / **P001** / `GYROSCOPE_SIGNAL_CONDITIONING_OUT` | `L84_PrecisionGyroscopeIMU` / **P007** / `GYROSCOPE_SIGNAL_CONDITIONING_IN` | `POWER_CONTROL` | Gyroscope signal conditioning |
| **W007** | `L84_Regulated5VControlSupply` / **P001** / `PRIMARY_STAR_TRACKER_SIGNAL_CONDITIONING_OUT` | `L84_PrimaryStarTracker` / **P008** / `PRIMARY_STAR_TRACKER_SIGNAL_CONDITIONING_IN` | `POWER_CONTROL` | Primary star tracker signal conditioning |
| **W008** | `L84_Regulated5VControlSupply` / **P001** / `SECONDARY_STAR_TRACKER_LOGIC_OUT` | `L84_SecondaryDiagnosticStarTracker` / **P009** / `SECONDARY_STAR_TRACKER_LOGIC_IN` | `POWER_CONTROL` | Secondary star tracker logic |
| **W009** | `L84_Regulated5VControlSupply` / **P001** / `HOLOGRAPHIC_SCADA_LOGIC_POWER_OUT` | `L84_HolographicAttitudeControlSCADA` / **P010** / `HOLOGRAPHIC_SCADA_LOGIC_POWER_IN` | `POWER_CONTROL` | Holographic SCADA logic power |
| **W010** | `L84_Regulated5VControlSupply` / **P001** / `INDEPENDENT_MOTION_SAFETY_LOGIC_POWER_OUT` | `L84_IndependentMotionSafetyStation` / **P011** / `INDEPENDENT_MOTION_SAFETY_LOGIC_POWER_IN` | `POWER_CONTROL` | Independent motion safety logic power |
| **W011** | `L84_AttitudeFlightController` / **P012** / `WHEEL_X_REQUESTED_TORQUE_COMMAND_OUT` | `L84_ReactionWheelMotorDriverRack` / **P013** / `WHEEL_X_REQUESTED_TORQUE_COMMAND_IN` | `ACTUATOR_COMMAND` | Wheel X requested torque command |
| **W012** | `L84_ReactionWheelMotorDriverRack` / **P014** / `WHEEL_X_ACTUAL_MOTOR_DRIVE_OUT` | `L84_ReactionWheelX` / **P015** / `WHEEL_X_ACTUAL_MOTOR_DRIVE_IN` | `ACTUATOR_COMMAND` | Wheel X actual motor drive |
| **W013** | `L84_ReactionWheelX` / **P016** / `WHEEL_X_SHAFT_VELOCITY_FEEDBACK_OUT` | `L84_ReactionWheelTachometerArray` / **P017** / `WHEEL_X_SHAFT_VELOCITY_FEEDBACK_IN` | `ANALOG_FEEDBACK` | Wheel X shaft velocity feedback |
| **W014** | `L84_ReactionWheelX` / **P018** / `WHEEL_X_ELECTRICAL_CURRENT_FEEDBACK_OUT` | `L84_ReactionWheelMotorCurrentMonitor` / **P019** / `WHEEL_X_ELECTRICAL_CURRENT_FEEDBACK_IN` | `ANALOG_FEEDBACK` | Wheel X electrical current feedback |
| **W015** | `L84_AttitudeFlightController` / **P020** / `WHEEL_Y_REQUESTED_TORQUE_COMMAND_OUT` | `L84_ReactionWheelMotorDriverRack` / **P021** / `WHEEL_Y_REQUESTED_TORQUE_COMMAND_IN` | `ACTUATOR_COMMAND` | Wheel Y requested torque command |
| **W016** | `L84_ReactionWheelMotorDriverRack` / **P022** / `WHEEL_Y_ACTUAL_MOTOR_DRIVE_OUT` | `L84_ReactionWheelY` / **P023** / `WHEEL_Y_ACTUAL_MOTOR_DRIVE_IN` | `ACTUATOR_COMMAND` | Wheel Y actual motor drive |
| **W017** | `L84_ReactionWheelY` / **P024** / `WHEEL_Y_SHAFT_VELOCITY_FEEDBACK_OUT` | `L84_ReactionWheelTachometerArray` / **P025** / `WHEEL_Y_SHAFT_VELOCITY_FEEDBACK_IN` | `ANALOG_FEEDBACK` | Wheel Y shaft velocity feedback |
| **W018** | `L84_ReactionWheelY` / **P026** / `WHEEL_Y_ELECTRICAL_CURRENT_FEEDBACK_OUT` | `L84_ReactionWheelMotorCurrentMonitor` / **P027** / `WHEEL_Y_ELECTRICAL_CURRENT_FEEDBACK_IN` | `ANALOG_FEEDBACK` | Wheel Y electrical current feedback |
| **W019** | `L84_AttitudeFlightController` / **P012** / `WHEEL_Z_REQUESTED_TORQUE_COMMAND_OUT` | `L84_ReactionWheelMotorDriverRack` / **P013** / `WHEEL_Z_REQUESTED_TORQUE_COMMAND_IN` | `ACTUATOR_COMMAND` | Wheel Z requested torque command |
| **W020** | `L84_ReactionWheelMotorDriverRack` / **P014** / `WHEEL_Z_ACTUAL_MOTOR_DRIVE_OUT` | `L84_ReactionWheelZ` / **P028** / `WHEEL_Z_ACTUAL_MOTOR_DRIVE_IN` | `ACTUATOR_COMMAND` | Wheel Z actual motor drive |
| **W021** | `L84_ReactionWheelZ` / **P029** / `WHEEL_Z_SHAFT_VELOCITY_FEEDBACK_OUT` | `L84_ReactionWheelTachometerArray` / **P017** / `WHEEL_Z_SHAFT_VELOCITY_FEEDBACK_IN` | `ANALOG_FEEDBACK` | Wheel Z shaft velocity feedback |
| **W022** | `L84_ReactionWheelZ` / **P030** / `WHEEL_Z_ELECTRICAL_CURRENT_FEEDBACK_OUT` | `L84_ReactionWheelMotorCurrentMonitor` / **P019** / `WHEEL_Z_ELECTRICAL_CURRENT_FEEDBACK_IN` | `ANALOG_FEEDBACK` | Wheel Z electrical current feedback |
| **W023** | `L84_AttitudeFlightController` / **P020** / `WHEEL_S_REQUESTED_TORQUE_COMMAND_OUT` | `L84_ReactionWheelMotorDriverRack` / **P021** / `WHEEL_S_REQUESTED_TORQUE_COMMAND_IN` | `ACTUATOR_COMMAND` | Wheel S requested torque command |
| **W024** | `L84_ReactionWheelMotorDriverRack` / **P022** / `WHEEL_S_ACTUAL_MOTOR_DRIVE_OUT` | `L84_SkewBackupReactionWheel` / **P031** / `WHEEL_S_ACTUAL_MOTOR_DRIVE_IN` | `ACTUATOR_COMMAND` | Wheel S actual motor drive |
| **W025** | `L84_SkewBackupReactionWheel` / **P032** / `WHEEL_S_SHAFT_VELOCITY_FEEDBACK_OUT` | `L84_ReactionWheelTachometerArray` / **P025** / `WHEEL_S_SHAFT_VELOCITY_FEEDBACK_IN` | `ANALOG_FEEDBACK` | Wheel S shaft velocity feedback |
| **W026** | `L84_SkewBackupReactionWheel` / **P033** / `WHEEL_S_ELECTRICAL_CURRENT_FEEDBACK_OUT` | `L84_ReactionWheelMotorCurrentMonitor` / **P027** / `WHEEL_S_ELECTRICAL_CURRENT_FEEDBACK_IN` | `ANALOG_FEEDBACK` | Wheel S electrical current feedback |
| **W027** | `L84_ReactionWheelTachometerArray` / **P034** / `WHEEL_CLUSTER_SPEED_LIMIT_FEEDBACK_OUT` | `L84_AttitudeFlightController` / **P035** / `WHEEL_CLUSTER_SPEED_LIMIT_FEEDBACK_IN` | `ANALOG_FEEDBACK` | Wheel cluster speed-limit feedback |
| **W028** | `L84_ReactionWheelMotorCurrentMonitor` / **P036** / `WHEEL_MOTOR_CURRENT_PROTECTION_FEEDBACK_OUT` | `L84_AttitudeFlightController` / **P037** / `WHEEL_MOTOR_CURRENT_PROTECTION_FEEDBACK_IN` | `ANALOG_FEEDBACK` | Wheel motor-current protection feedback |
| **W029** | `L84_ReactionWheelThermalControlModule` / **P038** / `WHEEL_THERMAL_HEALTH_FEEDBACK_OUT` | `L84_AttitudeFlightController` / **P039** / `WHEEL_THERMAL_HEALTH_FEEDBACK_IN` | `DIGITAL_STATUS` | Wheel thermal health feedback |
| **W030** | `L84_AttitudeFlightController` / **P040** / `WHEEL_THERMAL_REGULATION_REQUEST_OUT` | `L84_ReactionWheelThermalControlModule` / **P041** / `WHEEL_THERMAL_REGULATION_REQUEST_IN` | `DIGITAL_CONTROL` | Wheel thermal regulation request |
| **W031** | `L84_ReactionWheelSaturationAlarm` / **P042** / `WHEEL_SATURATION_WARNING_OUT` | `L84_AttitudeFlightController` / **P043** / `WHEEL_SATURATION_WARNING_IN` | `SAFETY_CONTROL` | Wheel saturation warning |
| **W032** | `L84_ReactionWheelDesaturationSupervisor` / **P044** / `WHEEL_DESATURATION_REQUEST_OUT` | `L84_AttitudeFlightController` / **P045** / `WHEEL_DESATURATION_REQUEST_IN` | `DIGITAL_CONTROL` | Wheel desaturation request |
| **W033** | `L84_MomentumDesaturationController` / **P046** / `X_MAGNETORQUER_DIPOLE_REQUEST_OUT` | `L84_MagneticTorqueRodDriveController` / **P047** / `X_MAGNETORQUER_DIPOLE_REQUEST_IN` | `ACTUATOR_COMMAND` | X magnetorquer dipole request |
| **W034** | `L84_MagneticTorqueRodDriveController` / **P048** / `X_TORQUE_ROD_DRIVE_CURRENT_OUT` | `L84_MagneticTorqueRodX` / **P049** / `X_TORQUE_ROD_DRIVE_CURRENT_IN` | `ACTUATOR_COMMAND` | X torque rod drive current |
| **W035** | `L84_MagneticTorqueRodX` / **P050** / `X_ACTUAL_MAGNETIC_DIPOLE_RESPONSE_OUT` | `L84_MomentumDesaturationController` / **P051** / `X_ACTUAL_MAGNETIC_DIPOLE_RESPONSE_IN` | `ANALOG_FEEDBACK` | X actual magnetic dipole response |
| **W036** | `L84_MomentumDesaturationController` / **P052** / `Y_MAGNETORQUER_DIPOLE_REQUEST_OUT` | `L84_MagneticTorqueRodDriveController` / **P053** / `Y_MAGNETORQUER_DIPOLE_REQUEST_IN` | `ACTUATOR_COMMAND` | Y magnetorquer dipole request |
| **W037** | `L84_MagneticTorqueRodDriveController` / **P054** / `Y_TORQUE_ROD_DRIVE_CURRENT_OUT` | `L84_MagneticTorqueRodY` / **P055** / `Y_TORQUE_ROD_DRIVE_CURRENT_IN` | `ACTUATOR_COMMAND` | Y torque rod drive current |
| **W038** | `L84_MagneticTorqueRodY` / **P056** / `Y_ACTUAL_MAGNETIC_DIPOLE_RESPONSE_OUT` | `L84_MomentumDesaturationController` / **P057** / `Y_ACTUAL_MAGNETIC_DIPOLE_RESPONSE_IN` | `ANALOG_FEEDBACK` | Y actual magnetic dipole response |
| **W039** | `L84_MomentumDesaturationController` / **P046** / `Z_MAGNETORQUER_DIPOLE_REQUEST_OUT` | `L84_MagneticTorqueRodDriveController` / **P047** / `Z_MAGNETORQUER_DIPOLE_REQUEST_IN` | `ACTUATOR_COMMAND` | Z magnetorquer dipole request |
| **W040** | `L84_MagneticTorqueRodDriveController` / **P048** / `Z_TORQUE_ROD_DRIVE_CURRENT_OUT` | `L84_MagneticTorqueRodZ` / **P058** / `Z_TORQUE_ROD_DRIVE_CURRENT_IN` | `ACTUATOR_COMMAND` | Z torque rod drive current |
| **W041** | `L84_MagneticTorqueRodZ` / **P059** / `Z_ACTUAL_MAGNETIC_DIPOLE_RESPONSE_OUT` | `L84_MomentumDesaturationController` / **P051** / `Z_ACTUAL_MAGNETIC_DIPOLE_RESPONSE_IN` | `ANALOG_FEEDBACK` | Z actual magnetic dipole response |
| **W042** | `L84_TriaxialMagnetometer` / **P060** / `CALIBRATED_LOCAL_MAGNETIC_FIELD_VECTOR_OUT` | `L84_MomentumDesaturationController` / **P057** / `CALIBRATED_LOCAL_MAGNETIC_FIELD_VECTOR_IN` | `ANALOG_FEEDBACK` | Calibrated local magnetic field vector |
| **W043** | `L84_OrbitalMagneticFieldEmulator` / **P061** / `ORBITAL_FIELD_ENVIRONMENT_REFERENCE_OUT` | `L84_TriaxialMagnetometer` / **P062** / `ORBITAL_FIELD_ENVIRONMENT_REFERENCE_IN` | `DATA_STREAM` | Orbital field environment reference |
| **W044** | `L84_TriaxialMagnetometer` / **P063** / `MAGNETIC_VECTOR_MEASUREMENT_TO_ESTIMATOR_OUT` | `L84_AttitudeStateEstimator` / **P064** / `MAGNETIC_VECTOR_MEASUREMENT_TO_ESTIMATOR_IN` | `ANALOG_FEEDBACK` | Magnetic vector measurement to estimator |
| **W045** | `L84_AttitudeStateEstimator` / **P065** / `ATTITUDE_AND_FIELD_DESATURATION_COORDINATION_OUT` | `L84_MomentumDesaturationController` / **P066** / `ATTITUDE_AND_FIELD_DESATURATION_COORDINATION_IN` | `DIGITAL_CONTROL` | Attitude and field desaturation coordination |
| **W046** | `L84_MomentumDesaturationController` / **P067** / `ACCUMULATED_WHEEL_MOMENTUM_DUMP_STATUS_OUT` | `L84_ReactionWheelDesaturationSupervisor` / **P068** / `ACCUMULATED_WHEEL_MOMENTUM_DUMP_STATUS_IN` | `DIGITAL_STATUS` | Accumulated wheel momentum dump status |
| **W047** | `L84_ReactionWheelDesaturationSupervisor` / **P069** / `PROTECTED_MOMENTUM_UNLOADING_AUTHORIZATION_OUT` | `L84_MagneticTorqueRodDriveController` / **P070** / `PROTECTED_MOMENTUM_UNLOADING_AUTHORIZATION_IN` | `SAFETY_CONTROL` | Protected momentum-unloading authorization |
| **W048** | `L84_IndependentMotionSafetyStation` / **P071** / `MAGNETIC_TORQUER_SAFETY_INTERLOCK_OUT` | `L84_MagneticTorqueRodDriveController` / **P072** / `MAGNETIC_TORQUER_SAFETY_INTERLOCK_IN` | `SAFETY_CONTROL` | Magnetic torquer safety interlock |
| **W049** | `L84_IntegratedADCSPLCRack` / **P073** / `PLC_TORQUE_MODE_SELECTION_OUT` | `L84_MomentumDesaturationController` / **P074** / `PLC_TORQUE_MODE_SELECTION_IN` | `DIGITAL_CONTROL` | PLC torque mode selection |
| **W050** | `L84_MagneticTorqueRodDriveController` / **P075** / `MAGNETORQUER_AMPLIFIER_READY_FEEDBACK_OUT` | `L84_IntegratedADCSPLCRack` / **P076** / `MAGNETORQUER_AMPLIFIER_READY_FEEDBACK_IN` | `DIGITAL_STATUS` | Magnetorquer amplifier ready feedback |
| **W051** | `L84_TriaxialMagnetometer` / **P077** / `MAGNETOMETER_MEASUREMENT_HEALTH_OUT` | `L84_IntegratedADCSPLCRack` / **P078** / `MAGNETOMETER_MEASUREMENT_HEALTH_IN` | `DIGITAL_STATUS` | Magnetometer measurement health |
| **W052** | `L84_MomentumDesaturationController` / **P067** / `DESATURATION_STATE_SCADA_FEEDBACK_OUT` | `L84_HolographicAttitudeControlSCADA` / **P079** / `DESATURATION_STATE_SCADA_FEEDBACK_IN` | `DIGITAL_STATUS` | Desaturation state SCADA feedback |
| **W053** | `L84_PrecisionGyroscopeIMU` / **P080** / `RAW_GYROSCOPE_ANGULAR_RATE_MEASUREMENT_OUT` | `L84_AttitudeStateEstimator` / **P081** / `RAW_GYROSCOPE_ANGULAR_RATE_MEASUREMENT_IN` | `ANALOG_FEEDBACK` | Raw gyroscope angular-rate measurement |
| **W054** | `L84_GyroscopeCalibrationStation` / **P082** / `GYROSCOPE_ZERO_RATE_CALIBRATION_COMMAND_OUT` | `L84_PrecisionGyroscopeIMU` / **P083** / `GYROSCOPE_ZERO_RATE_CALIBRATION_COMMAND_IN` | `DIGITAL_STATUS` | Gyroscope zero-rate calibration command |
| **W055** | `L84_GyroBiasThermalFaultEmulator` / **P084** / `GYRO_TEMPERATURE_BIAS_DISTURBANCE_INPUT_OUT` | `L84_PrecisionGyroscopeIMU` / **P085** / `GYRO_TEMPERATURE_BIAS_DISTURBANCE_INPUT_IN` | `DIGITAL_STATUS` | Gyro temperature bias disturbance input |
| **W056** | `L84_PrecisionGyroscopeIMU` / **P086** / `GYRO_MEASUREMENT_TIMESTAMP_VALIDITY_OUT` | `L84_AttitudeStateEstimator` / **P064** / `GYRO_MEASUREMENT_TIMESTAMP_VALIDITY_IN` | `ANALOG_FEEDBACK` | Gyro measurement timestamp/validity |
| **W057** | `L84_PrimaryStarTracker` / **P087** / `PRIMARY_TRACKER_STAR_FRAME_PATTERN_OUT` | `L84_StarCatalogPatternMatcher` / **P088** / `PRIMARY_TRACKER_STAR_FRAME_PATTERN_IN` | `DIGITAL_STATUS` | Primary tracker star-frame pattern |
| **W058** | `L84_StarCatalogPatternMatcher` / **P089** / `CATALOG_MATCH_ACCEPTANCE_OUT` | `L84_PrimaryStarTracker` / **P090** / `CATALOG_MATCH_ACCEPTANCE_IN` | `DIGITAL_STATUS` | Catalog match acceptance |
| **W059** | `L84_PrimaryStarTracker` / **P087** / `PRIMARY_TRACKER_DIAGNOSTIC_ATTITUDE_TELEMETRY_OUT` | `L84_AttitudeStateEstimator` / **P091** / `PRIMARY_TRACKER_DIAGNOSTIC_ATTITUDE_TELEMETRY_IN` | `DIGITAL_STATUS` | Primary tracker diagnostic attitude telemetry |
| **W060** | `L84_StarTrackerOpticalBaffle` / **P092** / `PRIMARY_TRACKER_BAFFLE_STATE_OUT` | `L84_PrimaryStarTracker` / **P093** / `PRIMARY_TRACKER_BAFFLE_STATE_IN` | `DIGITAL_STATUS` | Primary tracker baffle state |
| **W061** | `L84_SecondaryDiagnosticStarTracker` / **P094** / `SECONDARY_DIAGNOSTIC_TRACKER_STAR_PATTERN_OUT` | `L84_StarCatalogPatternMatcher` / **P095** / `SECONDARY_DIAGNOSTIC_TRACKER_STAR_PATTERN_IN` | `DIGITAL_STATUS` | Secondary diagnostic tracker star pattern |
| **W062** | `L84_SecondaryDiagnosticStarTracker` / **P094** / `SECONDARY_TRACKER_DIAGNOSTIC_CONFIDENCE_OUT` | `L84_AttitudeStateEstimator` / **P096** / `SECONDARY_TRACKER_DIAGNOSTIC_CONFIDENCE_IN` | `DIGITAL_STATUS` | Secondary tracker diagnostic confidence |
| **W063** | `L84_CoarseSunSensorCluster` / **P097** / `COARSE_SUN_DIRECTION_MEASUREMENT_OUT` | `L84_AttitudeStateEstimator` / **P081** / `COARSE_SUN_DIRECTION_MEASUREMENT_IN` | `ANALOG_FEEDBACK` | Coarse Sun-direction measurement |
| **W064** | `L84_EarthHorizonSensor` / **P098** / `EARTH_HORIZON_DIRECTION_MEASUREMENT_OUT` | `L84_AttitudeStateEstimator` / **P064** / `EARTH_HORIZON_DIRECTION_MEASUREMENT_IN` | `ANALOG_FEEDBACK` | Earth horizon direction measurement |
| **W065** | `L84_TriaxialMagnetometer` / **P060** / `MAGNETOMETER_TRI_AXIS_FIELD_MEASUREMENT_OUT` | `L84_AttitudeStateEstimator` / **P081** / `MAGNETOMETER_TRI_AXIS_FIELD_MEASUREMENT_IN` | `ANALOG_FEEDBACK` | Magnetometer tri-axis field measurement |
| **W066** | `L84_InertialReferenceClock` / **P099** / `INERTIAL_CLOCK_SYNCHRONIZATION_OUT` | `L84_AttitudeStateEstimator` / **P091** / `INERTIAL_CLOCK_SYNCHRONIZATION_IN` | `DIGITAL_STATUS` | Inertial clock synchronization |
| **W067** | `L84_AttitudeStateEstimator` / **P100** / `ESTIMATOR_COVARIANCE_UNCERTAINTY_OUTPUT_OUT` | `L84_EstimatorUncertaintyDisplay` / **P101** / `ESTIMATOR_COVARIANCE_UNCERTAINTY_OUTPUT_IN` | `DIGITAL_STATUS` | Estimator covariance/uncertainty output |
| **W068** | `L84_AttitudeStateEstimator` / **P100** / `COMPLEMENTARY_FUSION_STATUS_OUT` | `L84_ComplementaryFusionDisplay` / **P102** / `COMPLEMENTARY_FUSION_STATUS_IN` | `DIGITAL_STATUS` | Complementary fusion status |
| **W069** | `L84_StarCatalogPatternMatcher` / **P089** / `STAR_CATALOG_VALIDATION_SUMMARY_OUT` | `L84_ComplementaryFusionDisplay` / **P103** / `STAR_CATALOG_VALIDATION_SUMMARY_IN` | `DIGITAL_STATUS` | Star catalog validation summary |
| **W070** | `L84_GyroscopeCalibrationStation` / **P082** / `GYROSCOPE_CALIBRATION_REPORT_OUT` | `L84_IntegratedADCSPLCRack` / **P076** / `GYROSCOPE_CALIBRATION_REPORT_IN` | `DIGITAL_STATUS` | Gyroscope calibration report |
| **W071** | `L84_PrimaryStarTracker` / **P087** / `PRIMARY_TRACKER_OPERATIONAL_HEALTH_OUT` | `L84_IntegratedADCSPLCRack` / **P078** / `PRIMARY_TRACKER_OPERATIONAL_HEALTH_IN` | `DIGITAL_STATUS` | Primary tracker operational health |
| **W072** | `L84_SecondaryDiagnosticStarTracker` / **P094** / `SECONDARY_TRACKER_OPERATIONAL_HEALTH_OUT` | `L84_IntegratedADCSPLCRack` / **P076** / `SECONDARY_TRACKER_OPERATIONAL_HEALTH_IN` | `DIGITAL_STATUS` | Secondary tracker operational health |
| **W073** | `L84_CoarseSunSensorCluster` / **P104** / `SUN_SENSOR_VALIDITY_OUT` | `L84_IntegratedADCSPLCRack` / **P078** / `SUN_SENSOR_VALIDITY_IN` | `DIGITAL_STATUS` | Sun sensor validity |
| **W074** | `L84_EarthHorizonSensor` / **P105** / `HORIZON_SENSOR_VALIDITY_OUT` | `L84_IntegratedADCSPLCRack` / **P076** / `HORIZON_SENSOR_VALIDITY_IN` | `DIGITAL_STATUS` | Horizon sensor validity |
| **W075** | `L84_AttitudeStateEstimator` / **P106** / `ESTIMATED_QUATERNION_TO_FLIGHT_CONTROLLER_OUT` | `L84_AttitudeFlightController` / **P107** / `ESTIMATED_QUATERNION_TO_FLIGHT_CONTROLLER_IN` | `DATA_STREAM` | Estimated quaternion to flight controller |
| **W076** | `L84_EstimatorUncertaintyDisplay` / **P108** / `ESTIMATOR_CONFIDENCE_GATE_OUT` | `L84_AttitudeFlightController` / **P035** / `ESTIMATOR_CONFIDENCE_GATE_IN` | `ANALOG_FEEDBACK` | Estimator confidence gate |
| **W077** | `L84_SlewTrajectoryPlanner` / **P109** / `SLEW_TRAJECTORY_COMMAND_OUT` | `L84_AttitudeFlightController` / **P110** / `SLEW_TRAJECTORY_COMMAND_IN` | `DIGITAL_CONTROL` | Slew trajectory command |
| **W078** | `L84_AttitudeFlightController` / **P012** / `DESIRED_SPACECRAFT_BODY_TORQUE_OUT` | `L84_ThreeAxisTorqueAllocationComputer` / **P111** / `DESIRED_SPACECRAFT_BODY_TORQUE_IN` | `ACTUATOR_COMMAND` | Desired spacecraft body torque |
| **W079** | `L84_ThreeAxisTorqueAllocationComputer` / **P112** / `FOUR_WHEEL_TORQUE_ALLOCATION_OUT` | `L84_ReactionWheelMotorDriverRack` / **P013** / `FOUR_WHEEL_TORQUE_ALLOCATION_IN` | `ACTUATOR_COMMAND` | Four-wheel torque allocation |
| **W080** | `L84_AttitudeFlightController` / **P113** / `REQUESTED_TARGET_QUATERNION_OUT` | `L84_SlewTrajectoryPlanner` / **P114** / `REQUESTED_TARGET_QUATERNION_IN` | `DIGITAL_CONTROL` | Requested target quaternion |
| **W081** | `L84_SlewTrajectoryPlanner` / **P115** / `PREDICTED_SLEW_ENDPOINT_OUT` | `L84_PrecisionPointingErrorAnalyzer` / **P116** / `PREDICTED_SLEW_ENDPOINT_IN` | `DATA_STREAM` | Predicted slew endpoint |
| **W082** | `L84_PrecisionPointingErrorAnalyzer` / **P117** / `MEASURED_POINTING_ERROR_MAGNITUDE_OUT` | `L84_AttitudeFlightController` / **P037** / `MEASURED_POINTING_ERROR_MAGNITUDE_IN` | `ANALOG_FEEDBACK` | Measured pointing-error magnitude |
| **W083** | `L84_StructuralVibrationSensor` / **P118** / `MEASURED_STRUCTURAL_VIBRATION_RESPONSE_OUT` | `L84_ActivePointingVibrationFilter` / **P119** / `MEASURED_STRUCTURAL_VIBRATION_RESPONSE_IN` | `ANALOG_FEEDBACK` | Measured structural vibration response |
| **W084** | `L84_ActivePointingVibrationFilter` / **P120** / `QUALIFIED_JITTER_FILTER_OUTPUT_OUT` | `L84_AttitudeFlightController` / **P035** / `QUALIFIED_JITTER_FILTER_OUTPUT_IN` | `ANALOG_FEEDBACK` | Qualified jitter filter output |
| **W085** | `L84_ExternalDisturbanceTorqueInjector` / **P121** / `EXTERNAL_DISTURBANCE_TORQUE_ESTIMATE_OUT` | `L84_AttitudeFlightController` / **P037** / `EXTERNAL_DISTURBANCE_TORQUE_ESTIMATE_IN` | `ANALOG_FEEDBACK` | External disturbance torque estimate |
| **W086** | `L84_GravityGradientTorqueEmulator` / **P122** / `GRAVITY_GRADIENT_TORQUE_CONTRIBUTION_OUT` | `L84_ExternalDisturbanceTorqueInjector` / **P123** / `GRAVITY_GRADIENT_TORQUE_CONTRIBUTION_IN` | `DATA_STREAM` | Gravity-gradient torque contribution |
| **W087** | `L84_OrbitalDragTorqueEmulator` / **P124** / `ORBITAL_DRAG_TORQUE_CONTRIBUTION_OUT` | `L84_ExternalDisturbanceTorqueInjector` / **P125** / `ORBITAL_DRAG_TORQUE_CONTRIBUTION_IN` | `DATA_STREAM` | Orbital drag torque contribution |
| **W088** | `L84_SolarRadiationPressureTorqueModel` / **P126** / `SOLAR_PRESSURE_TORQUE_CONTRIBUTION_OUT` | `L84_ExternalDisturbanceTorqueInjector` / **P123** / `SOLAR_PRESSURE_TORQUE_CONTRIBUTION_IN` | `DATA_STREAM` | Solar pressure torque contribution |
| **W089** | `L84_SolarWingFlexibleModeEmulator` / **P127** / `SOLAR_WING_FLEXIBLE_MODE_AMPLITUDE_OUT` | `L84_StructuralVibrationSensor` / **P128** / `SOLAR_WING_FLEXIBLE_MODE_AMPLITUDE_IN` | `ANALOG_FEEDBACK` | Solar wing flexible-mode amplitude |
| **W090** | `L84_InertialReferenceClock` / **P099** / `ATTITUDE_CONTROLLER_PRECISE_TIMING_REFERENCE_OUT` | `L84_AttitudeFlightController` / **P129** / `ATTITUDE_CONTROLLER_PRECISE_TIMING_REFERENCE_IN` | `DIGITAL_STATUS` | Attitude controller precise timing reference |
| **W091** | `L84_HolographicQuaternionAttitudeSphere` / **P130** / `QUATERNION_ORIENTATION_VISUALIZATION_OUT` | `L84_HolographicAttitudeControlSCADA` / **P131** / `QUATERNION_ORIENTATION_VISUALIZATION_IN` | `DIGITAL_STATUS` | Quaternion orientation visualization |
| **W092** | `L84_EstimatorUncertaintyDisplay` / **P132** / `ESTIMATOR_UNCERTAINTY_VISUALIZATION_OUT` | `L84_HolographicAttitudeControlSCADA` / **P079** / `ESTIMATOR_UNCERTAINTY_VISUALIZATION_IN` | `DIGITAL_STATUS` | Estimator uncertainty visualization |
| **W093** | `L84_PrecisionPointingErrorAnalyzer` / **P133** / `POINTING_ACCURACY_TELEMETRY_OUT` | `L84_HolographicAttitudeControlSCADA` / **P134** / `POINTING_ACCURACY_TELEMETRY_IN` | `ANALOG_FEEDBACK` | Pointing accuracy telemetry |
| **W094** | `L84_ThreeAxisTorqueAllocationComputer` / **P135** / `TORQUE_ALLOCATION_TELEMETRY_OUT` | `L84_HolographicAttitudeControlSCADA` / **P131** / `TORQUE_ALLOCATION_TELEMETRY_IN` | `DIGITAL_STATUS` | Torque-allocation telemetry |
| **W095** | `L84_IndependentMotionSafetyStation` / **P136** / `INDEPENDENT_ATTITUDE_MOTION_SAFETY_PERMISSION_OUT` | `L84_AttitudeSafeModeSupervisor` / **P137** / `INDEPENDENT_ATTITUDE_MOTION_SAFETY_PERMISSION_IN` | `SAFETY_CONTROL` | Independent attitude motion safety permission |
| **W096** | `L84_PrimaryStarTracker` / **P087** / `STAR_TRACKER_SAMPLE_FRESHNESS_DIAGNOSTIC_OUT` | `L84_CriticalAttitudeFeedbackStation` / **P138** / `STAR_TRACKER_SAMPLE_FRESHNESS_DIAGNOSTIC_IN` | `DIGITAL_STATUS` | Star tracker sample freshness diagnostic |
| **W097** **CRITICAL** | `L84_PrimaryStarTracker` / **P139** / `STAR_TRACKER_QUATERNION_OUT` | `L84_AttitudeStateEstimator` / **P064** / `ADCS_ABSOLUTE_ATTITUDE_IN` | `ANALOG_FEEDBACK` | Primary star tracker trusted quaternion absolute-attitude feedback |
| **W098** | `L84_CriticalAttitudeFeedbackStation` / **P140** / `CRITICAL_ABSOLUTE_ATTITUDE_FEEDBACK_QUALIFICATION_OUT` | `L84_AttitudeSafeModeSupervisor` / **P141** / `CRITICAL_ABSOLUTE_ATTITUDE_FEEDBACK_QUALIFICATION_IN` | `SAFETY_CONTROL` | Critical absolute-attitude feedback qualification |
| **W099** | `L84_AttitudeSafeModeSupervisor` / **P142** / `ATTITUDE_SAFETY_MODE_AUTHORIZATION_OUT` | `L84_AttitudeFlightController` / **P143** / `ATTITUDE_SAFETY_MODE_AUTHORIZATION_IN` | `SAFETY_CONTROL` | Attitude safety mode authorization |
| **W100** | `L84_AttitudeSafeModeSupervisor` / **P144** / `SUN_SAFE_ACQUISITION_REQUEST_OUT` | `L84_SunSafeAttitudeController` / **P145** / `SUN_SAFE_ACQUISITION_REQUEST_IN` | `SAFETY_CONTROL` | Sun-safe acquisition request |
| **W101** | `L84_SunSafeAttitudeController` / **P146** / `VALIDATED_COARSE_SUN_SAFE_TARGET_OUT` | `L84_AttitudeFlightController` / **P045** / `VALIDATED_COARSE_SUN_SAFE_TARGET_IN` | `DIGITAL_CONTROL` | Validated coarse Sun-safe target |
| **W102** | `L84_CriticalAttitudeFeedbackStation` / **P147** / `ABSOLUTE_FEEDBACK_LOSS_CRITICAL_EVENT_OUT` | `L84_IntegratedADCSPLCRack` / **P148** / `ABSOLUTE_FEEDBACK_LOSS_CRITICAL_EVENT_IN` | `SAFETY_CONTROL` | Absolute feedback loss critical event |
| **W103** | `L84_IntegratedADCSPLCRack` / **P149** / `ADCS_PLC_PROTECTED_MOTION_RESET_REQUEST_OUT` | `L84_IndependentMotionSafetyStation` / **P150** / `ADCS_PLC_PROTECTED_MOTION_RESET_REQUEST_IN` | `SAFETY_CONTROL` | ADCS PLC protected motion reset request |
| **W104** | `L84_IndependentMotionSafetyStation` / **P071** / `INDEPENDENT_WHEEL_DRIVER_INHIBIT_OUT` | `L84_ReactionWheelMotorDriverRack` / **P151** / `INDEPENDENT_WHEEL_DRIVER_INHIBIT_IN` | `SAFETY_CONTROL` | Independent wheel driver inhibit |
| **W105** | `L84_IndependentMotionSafetyStation` / **P136** / `INDEPENDENT_TORQUER_CURRENT_INHIBIT_OUT` | `L84_MagneticTorqueRodDriveController` / **P070** / `INDEPENDENT_TORQUER_CURRENT_INHIBIT_IN` | `SAFETY_CONTROL` | Independent torquer current inhibit |
| **W106** | `L84_EmergencyAttitudeStopStation` / **P152** / `EMERGENCY_STOP_CONTACT_STATE_OUT` | `L84_IndependentMotionSafetyStation` / **P153** / `EMERGENCY_STOP_CONTACT_STATE_IN` | `SAFETY_CONTROL` | Emergency stop contact state |
| **W107** | `L84_IndependentMotionSafetyStation` / **P154** / `EMERGENCY_STOP_RESET_READINESS_OUT` | `L84_EmergencyAttitudeStopStation` / **P155** / `EMERGENCY_STOP_RESET_READINESS_IN` | `DIGITAL_STATUS` | Emergency stop reset readiness |
| **W108** | `L84_ReactionWheelSaturationAlarm` / **P156** / `WHEEL_SATURATION_PROTECTION_THRESHOLD_OUT` | `L84_AttitudeSafeModeSupervisor` / **P137** / `WHEEL_SATURATION_PROTECTION_THRESHOLD_IN` | `SAFETY_CONTROL` | Wheel saturation protection threshold |
| **W109** | `L84_AttitudeStateEstimator` / **P100** / `ATTITUDE_ESTIMATOR_STATUS_SUMMARY_NON_AUTHORITATIVE_OUT` | `L84_CriticalAttitudeFeedbackStation` / **P157** / `ATTITUDE_ESTIMATOR_STATUS_SUMMARY_NON_AUTHORITATIVE_IN` | `DIGITAL_STATUS` | Attitude estimator status summary non-authoritative |
| **W110** | `L84_AttitudeSafeModeSupervisor` / **P158** / `LATCHED_ATTITUDE_SAFE_MODE_SCADA_ALARM_OUT` | `L84_HolographicAttitudeControlSCADA` / **P079** / `LATCHED_ATTITUDE_SAFE_MODE_SCADA_ALARM_IN` | `DIGITAL_STATUS` | Latched attitude safe-mode SCADA alarm |
| **W111** | `L84_GroundStationTargetProjector` / **P159** / `GROUND_STATION_PREDICTED_LINE_OF_SIGHT_OUT` | `L84_SlewTrajectoryPlanner` / **P160** / `GROUND_STATION_PREDICTED_LINE_OF_SIGHT_IN` | `DATA_STREAM` | Ground-station predicted line of sight |
| **W112** | `L84_SlewTrajectoryPlanner` / **P161** / `OPTICAL_TARGET_SLEW_REQUEST_OUT` | `L84_AttitudeFlightController` / **P110** / `OPTICAL_TARGET_SLEW_REQUEST_IN` | `DIGITAL_CONTROL` | Optical target slew request |
| **W113** | `L84_AttitudeFlightController` / **P040** / `OPTICAL_POINTING_STABILIZATION_ENABLE_OUT` | `L84_OpticalLineOfSightSensor` / **P162** / `OPTICAL_POINTING_STABILIZATION_ENABLE_IN` | `DIGITAL_CONTROL` | Optical pointing stabilization enable |
| **W114** | `L84_OpticalLineOfSightSensor` / **P163** / `OPTICAL_LINE_OF_SIGHT_MEASUREMENT_OUT` | `L84_OpticalLinkAcquisitionFixture` / **P164** / `OPTICAL_LINE_OF_SIGHT_MEASUREMENT_IN` | `ANALOG_FEEDBACK` | Optical line-of-sight measurement |
| **W115** | `L84_MissionPayloadBoresightReference` / **P165** / `MISSION_PAYLOAD_BORESIGHT_REFERENCE_OUT` | `L84_OpticalLineOfSightSensor` / **P166** / `MISSION_PAYLOAD_BORESIGHT_REFERENCE_IN` | `DATA_STREAM` | Mission payload boresight reference |
| **W116** | `L84_OpticalLinkAcquisitionFixture` / **P167** / `ACQUIRED_OPTICAL_SIGNAL_QUALITY_OUT` | `L84_OpticalSignalQualityTelemetry` / **P168** / `ACQUIRED_OPTICAL_SIGNAL_QUALITY_IN` | `ANALOG_FEEDBACK` | Acquired optical signal quality |
| **W117** | `L84_OpticalSignalQualityTelemetry` / **P169** / `OPTICAL_LOCK_QUALIFICATION_FEEDBACK_OUT` | `L84_AttitudeFlightController` / **P035** / `OPTICAL_LOCK_QUALIFICATION_FEEDBACK_IN` | `ANALOG_FEEDBACK` | Optical lock qualification feedback |
| **W118** | `L84_OpticalSignalQualityTelemetry` / **P170** / `DOWNLINK_RECEIVE_QUALITY_TELEMETRY_OUT` | `L84_HolographicAttitudeControlSCADA` / **P131** / `DOWNLINK_RECEIVE_QUALITY_TELEMETRY_IN` | `DIGITAL_STATUS` | Downlink receive quality telemetry |
| **W119** | `L84_GroundStationTargetProjector` / **P171** / `MOVING_GROUND_STATION_TARGET_TELEMETRY_OUT` | `L84_HolographicAttitudeControlSCADA` / **P079** / `MOVING_GROUND_STATION_TARGET_TELEMETRY_IN` | `DIGITAL_STATUS` | Moving ground station target telemetry |
| **W120** | `L84_OpticalLinkAcquisitionFixture` / **P172** / `OPTICAL_TRANSMITTER_ACQUISITION_PERMIT_OUT` | `L84_CoPackagedOpticalTransmitter` / **P173** / `OPTICAL_TRANSMITTER_ACQUISITION_PERMIT_IN` | `DIGITAL_CONTROL` | Optical transmitter acquisition permit |
| **W121** | `L84_CoPackagedOpticalTransmitter` / **P174** / `OPTICAL_TRANSMITTER_ACTUAL_EMISSION_STATUS_OUT` | `L84_OpticalSignalQualityTelemetry` / **P175** / `OPTICAL_TRANSMITTER_ACTUAL_EMISSION_STATUS_IN` | `DIGITAL_STATUS` | Optical transmitter actual emission status |
| **W122** | `L84_FourChipletMissionPayload` / **P176** / `MISSION_CHIPLET_OUTGOING_DATA_PACKETS_OUT` | `L84_CoPackagedOpticalTransmitter` / **P177** / `MISSION_CHIPLET_OUTGOING_DATA_PACKETS_IN` | `DATA_STREAM` | Mission chiplet outgoing data packets |
| **W123** | `L84_ProtectedAvionicsChassis` / **P178** / `AVIONICS_SUPPLY_AND_HEALTH_STATUS_OUT` | `L84_FourChipletMissionPayload` / **P179** / `AVIONICS_SUPPLY_AND_HEALTH_STATUS_IN` | `DIGITAL_STATUS` | Avionics supply and health status |
| **W124** | `L84_OpticalSignalQualityTelemetry` / **P170** / `VERIFIED_OPTICAL_DOWNLINK_PACKET_DELIVERY_OUT` | `L84_IntegratedADCSPLCRack` / **P078** / `VERIFIED_OPTICAL_DOWNLINK_PACKET_DELIVERY_IN` | `DIGITAL_STATUS` | Verified optical downlink packet delivery |
| **W125** | `L84_IntegratedADCSPLCRack` / **P180** / `TWO_PASS_OPTICAL_SCHEDULE_AUTHORIZATION_OUT` | `L84_GroundStationTargetProjector` / **P181** / `TWO_PASS_OPTICAL_SCHEDULE_AUTHORIZATION_IN` | `DIGITAL_CONTROL` | Two-pass optical schedule authorization |
| **W126** | `L84_OpticalLineOfSightSensor` / **P182** / `BEAM_POINTING_ERROR_TO_ANALYZER_OUT` | `L84_PrecisionPointingErrorAnalyzer` / **P183** / `BEAM_POINTING_ERROR_TO_ANALYZER_IN` | `ANALOG_FEEDBACK` | Beam pointing error to analyzer |
| **W127** | `L84_StructuralVibrationSensor` / **P184** / `STRUCTURAL_JITTER_OPTICAL_QUALITY_IMPACT_OUT` | `L84_OpticalSignalQualityTelemetry` / **P185** / `STRUCTURAL_JITTER_OPTICAL_QUALITY_IMPACT_IN` | `ANALOG_FEEDBACK` | Structural jitter optical quality impact |
| **W128** | `L84_SunSafeAttitudeController` / **P186** / `SUN_SAFE_PORT_ARRAY_POINTING_REQUEST_OUT` | `L84_PortSolarArrayWing` / **P187** / `SUN_SAFE_PORT_ARRAY_POINTING_REQUEST_IN` | `DIGITAL_CONTROL` | Sun-safe port array pointing request |
| **W129** | `L84_SunSafeAttitudeController` / **P146** / `SUN_SAFE_STARBOARD_ARRAY_POINTING_REQUEST_OUT` | `L84_StarboardSolarArrayWing` / **P188** / `SUN_SAFE_STARBOARD_ARRAY_POINTING_REQUEST_IN` | `DIGITAL_CONTROL` | Sun-safe starboard array pointing request |
| **W130** | `L84_OpticalSignalQualityTelemetry` / **P170** / `OPTICAL_COMMUNICATION_QUALIFICATION_LOG_OUT` | `L84_AttitudeFlightDataRecorder` / **P189** / `OPTICAL_COMMUNICATION_QUALIFICATION_LOG_IN` | `DIGITAL_STATUS` | Optical communication qualification log |
| **W131** | `L84_IntegratedADCSPLCRack` / **P190** / `ADCS_PLC_MODE_AND_SEQUENCING_TELEMETRY_OUT` | `L84_HolographicAttitudeControlSCADA` / **P131** / `ADCS_PLC_MODE_AND_SEQUENCING_TELEMETRY_IN` | `DIGITAL_STATUS` | ADCS PLC mode and sequencing telemetry |
| **W132** | `L84_HolographicAttitudeControlSCADA` / **P191** / `OPERATOR_MODE_REQUEST_OUT` | `L84_IntegratedADCSPLCRack` / **P192** / `OPERATOR_MODE_REQUEST_IN` | `DIGITAL_CONTROL` | Operator mode request |
| **W133** | `L84_AttitudeFlightController` / **P193** / `THREE_AXIS_CONTROL_STABILITY_QUALIFICATION_OUT` | `L84_IntegratedADCSPLCRack` / **P076** / `THREE_AXIS_CONTROL_STABILITY_QUALIFICATION_IN` | `DIGITAL_STATUS` | Three-axis control stability qualification |
| **W134** | `L84_AttitudeStateEstimator` / **P100** / `ESTIMATOR_READINESS_AND_CONFIDENCE_OUT` | `L84_IntegratedADCSPLCRack` / **P078** / `ESTIMATOR_READINESS_AND_CONFIDENCE_IN` | `DIGITAL_STATUS` | Estimator readiness and confidence |
| **W135** | `L84_ReactionWheelTachometerArray` / **P194** / `WHEEL_SPEED_MARGIN_CERTIFICATE_OUT` | `L84_IntegratedADCSPLCRack` / **P076** / `WHEEL_SPEED_MARGIN_CERTIFICATE_IN` | `DIGITAL_STATUS` | Wheel speed margin certificate |
| **W136** | `L84_MomentumDesaturationController` / **P067** / `MAGNETIC_MOMENTUM_DUMPING_CERTIFICATE_OUT` | `L84_IntegratedADCSPLCRack` / **P078** / `MAGNETIC_MOMENTUM_DUMPING_CERTIFICATE_IN` | `DIGITAL_STATUS` | Magnetic momentum dumping certificate |
| **W137** | `L84_AttitudeSafeModeSupervisor` / **P142** / `ATTITUDE_SAFE_MODE_AND_ALARM_STATUS_OUT` | `L84_IntegratedADCSPLCRack` / **P195** / `ATTITUDE_SAFE_MODE_AND_ALARM_STATUS_IN` | `SAFETY_CONTROL` | Attitude-safe mode and alarm status |
| **W138** | `L84_IntegratedADCSPLCRack` / **P073** / `CERTIFICATION_BEACON_COMMAND_OUT` | `L84_ADCSCertificationBeacon` / **P196** / `CERTIFICATION_BEACON_COMMAND_IN` | `DIGITAL_CONTROL` | Certification beacon command |
| **W139** | `L84_ADCSCertificationBeacon` / **P197** / `SIX_OBJECTIVE_CERTIFICATE_STATE_OUT` | `L84_HolographicAttitudeControlSCADA` / **P079** / `SIX_OBJECTIVE_CERTIFICATE_STATE_IN` | `DIGITAL_STATUS` | Six-objective certificate state |
| **W140** | `L84_AttitudeFlightDataRecorder` / **P198** / `TIMESTAMPED_FLIGHT_RECORDER_EVENT_TELEMETRY_OUT` | `L84_HolographicAttitudeControlSCADA` / **P131** / `TIMESTAMPED_FLIGHT_RECORDER_EVENT_TELEMETRY_IN` | `DIGITAL_STATUS` | Timestamped flight recorder event telemetry |
| **W141** | `L84_IntegratedADCSPLCRack` / **P180** / `FLIGHT_EVENT_RECORDING_REQUEST_OUT` | `L84_AttitudeFlightDataRecorder` / **P199** / `FLIGHT_EVENT_RECORDING_REQUEST_IN` | `DIGITAL_CONTROL` | Flight event recording request |
| **W142** | `L84_IndependentMotionSafetyStation` / **P071** / `INDEPENDENT_MOTION_PROTECTION_STATUS_OUT` | `L84_IntegratedADCSPLCRack` / **P148** / `INDEPENDENT_MOTION_PROTECTION_STATUS_IN` | `SAFETY_CONTROL` | Independent motion protection status |

**Remember:** `Status=CONCEPT_ONLY_NOT_YET_PLAYER_WIRE` for all source rows. These rows must be created as real player-placeable wires; none is already an operational Spark electrical edge. W097 is the only marked critical row.

## ANNEX D — EXACT 218 DECORATIVE SOCKET ANCHOR LOCATIONS

| Anchor | Owner model | World XYZ (m) | Status |
|---|---|---|---|
| `P001` | `L84_Regulated5VControlSupply` | `(6.07, 1.44, 5.09)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P002` | `L84_IntegratedADCSPLCRack` | `(4.77, 1.44, 5.09)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P003` | `L84_AttitudeFlightController` | `(-1.88, 1.44, 5.09)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P004` | `L84_AttitudeStateEstimator` | `(-5.68, 1.44, 5.09)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P005` | `L84_ReactionWheelMotorDriverRack` | `(-11.43, 1.34, 4.39)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P006` | `L84_MagneticTorqueRodDriveController` | `(-5.53, 1.89, -4.47)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P007` | `L84_PrecisionGyroscopeIMU` | `(7.17, 2.09, 4.24)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P008` | `L84_PrimaryStarTracker` | `(8.87, 3.99, 0.69)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P009` | `L84_SecondaryDiagnosticStarTracker` | `(11.32, 3.99, 0.69)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P010` | `L84_HolographicAttitudeControlSCADA` | `(-0.23, 7.59, -4.91)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P011` | `L84_IndependentMotionSafetyStation` | `(7.37, 1.44, 5.09)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P012` | `L84_AttitudeFlightController` | `(-1.765, 1.44, 5.09)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P013` | `L84_ReactionWheelMotorDriverRack` | `(-11.315, 1.34, 4.39)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P014` | `L84_ReactionWheelMotorDriverRack` | `(-11.2, 1.34, 4.39)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P015` | `L84_ReactionWheelX` | `(-11.78, 2.94, 0.85)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P016` | `L84_ReactionWheelX` | `(-11.665, 2.94, 0.85)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P017` | `L84_ReactionWheelTachometerArray` | `(-10.18, 1.34, 4.39)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P018` | `L84_ReactionWheelX` | `(-11.55, 2.94, 0.85)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P019` | `L84_ReactionWheelMotorCurrentMonitor` | `(-8.93, 1.34, 4.39)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P020` | `L84_AttitudeFlightController` | `(-1.65, 1.44, 5.09)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P021` | `L84_ReactionWheelMotorDriverRack` | `(-11.085, 1.34, 4.39)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P022` | `L84_ReactionWheelMotorDriverRack` | `(-10.97, 1.34, 4.39)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P023` | `L84_ReactionWheelY` | `(-10.41, 2.94, 0.85)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P024` | `L84_ReactionWheelY` | `(-10.295, 2.94, 0.85)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P025` | `L84_ReactionWheelTachometerArray` | `(-10.065, 1.34, 4.39)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P026` | `L84_ReactionWheelY` | `(-10.18, 2.94, 0.85)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P027` | `L84_ReactionWheelMotorCurrentMonitor` | `(-8.815, 1.34, 4.39)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P028` | `L84_ReactionWheelZ` | `(-9.03, 2.94, 0.85)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P029` | `L84_ReactionWheelZ` | `(-8.915, 2.94, 0.85)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P030` | `L84_ReactionWheelZ` | `(-8.8, 2.94, 0.85)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P031` | `L84_SkewBackupReactionWheel` | `(-7.66, 2.94, 0.85)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P032` | `L84_SkewBackupReactionWheel` | `(-7.545, 2.94, 0.85)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P033` | `L84_SkewBackupReactionWheel` | `(-7.43, 2.94, 0.85)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P034` | `L84_ReactionWheelTachometerArray` | `(-9.95, 1.34, 4.39)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P035` | `L84_AttitudeFlightController` | `(-1.535, 1.44, 5.09)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P036` | `L84_ReactionWheelMotorCurrentMonitor` | `(-8.7, 1.34, 4.39)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P037` | `L84_AttitudeFlightController` | `(-1.42, 1.44, 5.09)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P038` | `L84_ReactionWheelThermalControlModule` | `(-7.68, 1.34, 4.39)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P039` | `L84_AttitudeFlightController` | `(-1.88, 1.545, 5.09)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P040` | `L84_AttitudeFlightController` | `(-1.765, 1.545, 5.09)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P041` | `L84_ReactionWheelThermalControlModule` | `(-7.565, 1.34, 4.39)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P042` | `L84_ReactionWheelSaturationAlarm` | `(-11.45, 4.04, -0.16)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P043` | `L84_AttitudeFlightController` | `(-1.65, 1.545, 5.09)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P044` | `L84_ReactionWheelDesaturationSupervisor` | `(-10.43, 4.04, -0.16)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P045` | `L84_AttitudeFlightController` | `(-1.535, 1.545, 5.09)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P046` | `L84_MomentumDesaturationController` | `(1.92, 3.29, -4.71)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P047` | `L84_MagneticTorqueRodDriveController` | `(-5.415, 1.89, -4.47)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P048` | `L84_MagneticTorqueRodDriveController` | `(-5.3, 1.89, -4.47)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P049` | `L84_MagneticTorqueRodX` | `(-3.03, 1.79, -4.41)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P050` | `L84_MagneticTorqueRodX` | `(-2.915, 1.79, -4.41)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P051` | `L84_MomentumDesaturationController` | `(2.035, 3.29, -4.71)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P052` | `L84_MomentumDesaturationController` | `(2.15, 3.29, -4.71)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P053` | `L84_MagneticTorqueRodDriveController` | `(-5.185, 1.89, -4.47)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P054` | `L84_MagneticTorqueRodDriveController` | `(-5.07, 1.89, -4.47)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P055` | `L84_MagneticTorqueRodY` | `(-0.23, 1.79, -4.41)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P056` | `L84_MagneticTorqueRodY` | `(-0.115, 1.79, -4.41)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P057` | `L84_MomentumDesaturationController` | `(2.265, 3.29, -4.71)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P058` | `L84_MagneticTorqueRodZ` | `(2.57, 1.79, -4.41)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P059` | `L84_MagneticTorqueRodZ` | `(2.685, 1.79, -4.41)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P060` | `L84_TriaxialMagnetometer` | `(-5.43, 3.39, -4.45)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P061` | `L84_OrbitalMagneticFieldEmulator` | `(-2.23, 3.39, -4.76)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P062` | `L84_TriaxialMagnetometer` | `(-5.315, 3.39, -4.45)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P063` | `L84_TriaxialMagnetometer` | `(-5.2, 3.39, -4.45)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P064` | `L84_AttitudeStateEstimator` | `(-5.565, 1.44, 5.09)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P065` | `L84_AttitudeStateEstimator` | `(-5.45, 1.44, 5.09)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P066` | `L84_MomentumDesaturationController` | `(2.38, 3.29, -4.71)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P067` | `L84_MomentumDesaturationController` | `(1.92, 3.395, -4.71)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P068` | `L84_ReactionWheelDesaturationSupervisor` | `(-10.315, 4.04, -0.16)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P069` | `L84_ReactionWheelDesaturationSupervisor` | `(-10.2, 4.04, -0.16)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P070` | `L84_MagneticTorqueRodDriveController` | `(-5.53, 1.995, -4.47)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P071` | `L84_IndependentMotionSafetyStation` | `(7.485, 1.44, 5.09)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P072` | `L84_MagneticTorqueRodDriveController` | `(-5.415, 1.995, -4.47)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P073` | `L84_IntegratedADCSPLCRack` | `(4.885, 1.44, 5.09)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P074` | `L84_MomentumDesaturationController` | `(2.035, 3.395, -4.71)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P075` | `L84_MagneticTorqueRodDriveController` | `(-5.3, 1.995, -4.47)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P076` | `L84_IntegratedADCSPLCRack` | `(5.0, 1.44, 5.09)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P077` | `L84_TriaxialMagnetometer` | `(-5.085, 3.39, -4.45)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P078` | `L84_IntegratedADCSPLCRack` | `(5.115, 1.44, 5.09)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P079` | `L84_HolographicAttitudeControlSCADA` | `(-0.115, 7.59, -4.91)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P080` | `L84_PrecisionGyroscopeIMU` | `(7.285, 2.09, 4.24)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P081` | `L84_AttitudeStateEstimator` | `(-5.335, 1.44, 5.09)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P082` | `L84_GyroscopeCalibrationStation` | `(8.47, 2.09, 4.24)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P083` | `L84_PrecisionGyroscopeIMU` | `(7.4, 2.09, 4.24)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P084` | `L84_GyroBiasThermalFaultEmulator` | `(9.77, 2.09, 4.24)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P085` | `L84_PrecisionGyroscopeIMU` | `(7.515, 2.09, 4.24)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P086` | `L84_PrecisionGyroscopeIMU` | `(7.63, 2.09, 4.24)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P087` | `L84_PrimaryStarTracker` | `(8.985, 3.99, 0.69)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P088` | `L84_StarCatalogPatternMatcher` | `(8.92, 2.39, 0.79)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P089` | `L84_StarCatalogPatternMatcher` | `(9.035, 2.39, 0.79)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P090` | `L84_PrimaryStarTracker` | `(9.1, 3.99, 0.69)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P091` | `L84_AttitudeStateEstimator` | `(-5.22, 1.44, 5.09)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P092` | `L84_StarTrackerOpticalBaffle` | `(10.17, 3.91, 0.69)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P093` | `L84_PrimaryStarTracker` | `(9.215, 3.99, 0.69)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P094` | `L84_SecondaryDiagnosticStarTracker` | `(11.435, 3.99, 0.69)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P095` | `L84_StarCatalogPatternMatcher` | `(9.15, 2.39, 0.79)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P096` | `L84_AttitudeStateEstimator` | `(-5.68, 1.545, 5.09)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P097` | `L84_CoarseSunSensorCluster` | `(10.22, 2.39, 0.79)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P098` | `L84_EarthHorizonSensor` | `(11.42, 2.39, 0.79)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P099` | `L84_InertialReferenceClock` | `(3.32, 1.44, 5.09)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P100` | `L84_AttitudeStateEstimator` | `(-5.565, 1.545, 5.09)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P101` | `L84_EstimatorUncertaintyDisplay` | `(-3.18, 1.44, 5.09)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P102` | `L84_ComplementaryFusionDisplay` | `(-4.41, 1.44, 5.09)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P103` | `L84_ComplementaryFusionDisplay` | `(-4.295, 1.44, 5.09)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P104` | `L84_CoarseSunSensorCluster` | `(10.335, 2.39, 0.79)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P105` | `L84_EarthHorizonSensor` | `(11.535, 2.39, 0.79)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P106` | `L84_AttitudeStateEstimator` | `(-5.45, 1.545, 5.09)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P107` | `L84_AttitudeFlightController` | `(-1.42, 1.545, 5.09)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P108` | `L84_EstimatorUncertaintyDisplay` | `(-3.065, 1.44, 5.09)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P109` | `L84_SlewTrajectoryPlanner` | `(0.72, 1.44, 5.09)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P110` | `L84_AttitudeFlightController` | `(-1.88, 1.65, 5.09)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P111` | `L84_ThreeAxisTorqueAllocationComputer` | `(-0.58, 1.44, 5.09)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P112` | `L84_ThreeAxisTorqueAllocationComputer` | `(-0.465, 1.44, 5.09)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P113` | `L84_AttitudeFlightController` | `(-1.765, 1.65, 5.09)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P114` | `L84_SlewTrajectoryPlanner` | `(0.835, 1.44, 5.09)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P115` | `L84_SlewTrajectoryPlanner` | `(0.95, 1.44, 5.09)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P116` | `L84_PrecisionPointingErrorAnalyzer` | `(2.02, 1.44, 5.09)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P117` | `L84_PrecisionPointingErrorAnalyzer` | `(2.135, 1.44, 5.09)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P118` | `L84_StructuralVibrationSensor` | `(5.22, 3.54, -4.61)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P119` | `L84_ActivePointingVibrationFilter` | `(6.42, 3.54, -4.61)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P120` | `L84_ActivePointingVibrationFilter` | `(6.535, 3.54, -4.61)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P121` | `L84_ExternalDisturbanceTorqueInjector` | `(-4.33, 3.39, -4.41)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P122` | `L84_GravityGradientTorqueEmulator` | `(-3.23, 3.54, -4.61)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P123` | `L84_ExternalDisturbanceTorqueInjector` | `(-4.215, 3.39, -4.41)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P124` | `L84_OrbitalDragTorqueEmulator` | `(-2.13, 3.54, -4.61)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P125` | `L84_ExternalDisturbanceTorqueInjector` | `(-4.1, 3.39, -4.41)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P126` | `L84_SolarRadiationPressureTorqueModel` | `(-0.98, 3.54, -4.61)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P127` | `L84_SolarWingFlexibleModeEmulator` | `(4.02, 3.54, -4.61)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P128` | `L84_StructuralVibrationSensor` | `(5.335, 3.54, -4.61)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P129` | `L84_AttitudeFlightController` | `(-1.65, 1.65, 5.09)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P130` | `L84_HolographicQuaternionAttitudeSphere` | `(3.52, 7.24, -4.73)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P131` | `L84_HolographicAttitudeControlSCADA` | `(0.0, 7.59, -4.91)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P132` | `L84_EstimatorUncertaintyDisplay` | `(-2.95, 1.44, 5.09)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P133` | `L84_PrecisionPointingErrorAnalyzer` | `(2.25, 1.44, 5.09)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P134` | `L84_HolographicAttitudeControlSCADA` | `(0.115, 7.59, -4.91)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P135` | `L84_ThreeAxisTorqueAllocationComputer` | `(-0.35, 1.44, 5.09)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P136` | `L84_IndependentMotionSafetyStation` | `(7.6, 1.44, 5.09)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P137` | `L84_AttitudeSafeModeSupervisor` | `(-6.63, 1.44, 5.09)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P138` | `L84_CriticalAttitudeFeedbackStation` | `(10.07, 1.43, 5.09)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P139` | `L84_PrimaryStarTracker` | `(9.33, 3.99, 0.69)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P140` | `L84_CriticalAttitudeFeedbackStation` | `(10.185, 1.43, 5.09)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P141` | `L84_AttitudeSafeModeSupervisor` | `(-6.515, 1.44, 5.09)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P142` | `L84_AttitudeSafeModeSupervisor` | `(-6.4, 1.44, 5.09)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P143` | `L84_AttitudeFlightController` | `(-1.535, 1.65, 5.09)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P144` | `L84_AttitudeSafeModeSupervisor` | `(-6.285, 1.44, 5.09)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P145` | `L84_SunSafeAttitudeController` | `(-5.65, 3.44, -4.41)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P146` | `L84_SunSafeAttitudeController` | `(-5.535, 3.44, -4.41)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P147` | `L84_CriticalAttitudeFeedbackStation` | `(10.3, 1.43, 5.09)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P148` | `L84_IntegratedADCSPLCRack` | `(5.23, 1.44, 5.09)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P149` | `L84_IntegratedADCSPLCRack` | `(4.77, 1.545, 5.09)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P150` | `L84_IndependentMotionSafetyStation` | `(7.715, 1.44, 5.09)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P151` | `L84_ReactionWheelMotorDriverRack` | `(-11.43, 1.445, 4.39)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P152` | `L84_EmergencyAttitudeStopStation` | `(10.92, 1.31, 5.24)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P153` | `L84_IndependentMotionSafetyStation` | `(7.83, 1.44, 5.09)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P154` | `L84_IndependentMotionSafetyStation` | `(7.37, 1.545, 5.09)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P155` | `L84_EmergencyAttitudeStopStation` | `(11.035, 1.31, 5.24)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P156` | `L84_ReactionWheelSaturationAlarm` | `(-11.335, 4.04, -0.16)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P157` | `L84_CriticalAttitudeFeedbackStation` | `(10.415, 1.43, 5.09)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P158` | `L84_AttitudeSafeModeSupervisor` | `(-6.17, 1.44, 5.09)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P159` | `L84_GroundStationTargetProjector` | `(8.87, 3.54, -4.61)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P160` | `L84_SlewTrajectoryPlanner` | `(1.065, 1.44, 5.09)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P161` | `L84_SlewTrajectoryPlanner` | `(1.18, 1.44, 5.09)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P162` | `L84_OpticalLineOfSightSensor` | `(7.67, 3.54, -4.61)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P163` | `L84_OpticalLineOfSightSensor` | `(7.785, 3.54, -4.61)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P164` | `L84_OpticalLinkAcquisitionFixture` | `(10.12, 3.54, -4.61)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P165` | `L84_MissionPayloadBoresightReference` | `(3.27, 5.39, -1.01)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P166` | `L84_OpticalLineOfSightSensor` | `(7.9, 3.54, -4.61)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P167` | `L84_OpticalLinkAcquisitionFixture` | `(10.235, 3.54, -4.61)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P168` | `L84_OpticalSignalQualityTelemetry` | `(11.32, 3.54, -4.61)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P169` | `L84_OpticalSignalQualityTelemetry` | `(11.435, 3.54, -4.61)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P170` | `L84_OpticalSignalQualityTelemetry` | `(11.55, 3.54, -4.61)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P171` | `L84_GroundStationTargetProjector` | `(8.985, 3.54, -4.61)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P172` | `L84_OpticalLinkAcquisitionFixture` | `(10.35, 3.54, -4.61)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P173` | `L84_CoPackagedOpticalTransmitter` | `(-0.23, 5.64, 0.09)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P174` | `L84_CoPackagedOpticalTransmitter` | `(-0.115, 5.64, 0.09)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P175` | `L84_OpticalSignalQualityTelemetry` | `(11.665, 3.54, -4.61)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P176` | `L84_FourChipletMissionPayload` | `(-0.23, 4.91, -0.13)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P177` | `L84_CoPackagedOpticalTransmitter` | `(0.0, 5.64, 0.09)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P178` | `L84_ProtectedAvionicsChassis` | `(-0.23, 4.79, -0.41)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P179` | `L84_FourChipletMissionPayload` | `(-0.115, 4.91, -0.13)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P180` | `L84_IntegratedADCSPLCRack` | `(4.885, 1.545, 5.09)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P181` | `L84_GroundStationTargetProjector` | `(9.1, 3.54, -4.61)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P182` | `L84_OpticalLineOfSightSensor` | `(8.015, 3.54, -4.61)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P183` | `L84_PrecisionPointingErrorAnalyzer` | `(2.365, 1.44, 5.09)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P184` | `L84_StructuralVibrationSensor` | `(5.45, 3.54, -4.61)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P185` | `L84_OpticalSignalQualityTelemetry` | `(11.78, 3.54, -4.61)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P186` | `L84_SunSafeAttitudeController` | `(-5.42, 3.44, -4.41)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P187` | `L84_PortSolarArrayWing` | `(-5.23, 4.86, -1.0)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P188` | `L84_StarboardSolarArrayWing` | `(4.77, 4.86, -1.0)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P189` | `L84_AttitudeFlightDataRecorder` | `(8.72, 1.44, 5.09)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P190` | `L84_IntegratedADCSPLCRack` | `(5.0, 1.545, 5.09)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P191` | `L84_HolographicAttitudeControlSCADA` | `(0.23, 7.59, -4.91)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P192` | `L84_IntegratedADCSPLCRack` | `(5.115, 1.545, 5.09)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P193` | `L84_AttitudeFlightController` | `(-1.42, 1.65, 5.09)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P194` | `L84_ReactionWheelTachometerArray` | `(-9.835, 1.34, 4.39)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P195` | `L84_IntegratedADCSPLCRack` | `(5.23, 1.545, 5.09)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P196` | `L84_ADCSCertificationBeacon` | `(-0.23, 8.39, -4.81)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P197` | `L84_ADCSCertificationBeacon` | `(-0.115, 8.39, -4.81)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P198` | `L84_AttitudeFlightDataRecorder` | `(8.835, 1.44, 5.09)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P199` | `L84_AttitudeFlightDataRecorder` | `(8.95, 1.44, 5.09)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P200` | `L84_RotationalTestRigPedestal` | `(-0.23, 1.09, -1.05)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P201` | `L84_SpacecraftBodyAxesHologram` | `(-0.23, 5.99, -1.0)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P202` | `L84_MagneticTorqueRodDriveController` | `(-5.185, 1.995, -4.47)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P203` | `L84_EarthHorizonSensor` | `(11.65, 2.39, 0.79)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P204` | `L84_SolarRadiationPressureTorqueModel` | `(-0.865, 3.54, -4.61)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P205` | `L84_CriticalAttitudeFeedbackStation` | `(10.53, 1.43, 5.09)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P206` | `L84_ProtectedAvionicsChassis` | `(-0.115, 4.79, -0.41)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P207` | `L84_ReactionWheelX` | `(-11.435, 2.94, 0.85)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P208` | `L84_OrbitalMagneticFieldEmulator` | `(-2.115, 3.39, -4.76)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P209` | `L84_ComplementaryFusionDisplay` | `(-4.18, 1.44, 5.09)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P210` | `L84_StructuralVibrationSensor` | `(5.565, 3.54, -4.61)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P211` | `L84_Regulated5VControlSupply` | `(6.185, 1.44, 5.09)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P212` | `L84_CoPackagedOpticalTransmitter` | `(0.115, 5.64, 0.09)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P213` | `L84_ReactionWheelZ` | `(-8.685, 2.94, 0.85)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P214` | `L84_MomentumDesaturationController` | `(2.15, 3.395, -4.71)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P215` | `L84_EstimatorUncertaintyDisplay` | `(-2.835, 1.44, 5.09)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P216` | `L84_OpticalLineOfSightSensor` | `(8.13, 3.54, -4.61)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P217` | `L84_HolographicAttitudeControlSCADA` | `(-0.23, 7.695, -4.91)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P218` | `L84_StarboardSolarArrayWing` | `(4.885, 4.86, -1.0)` | `ART_ANCHOR_NOT_SPARKTERMINAL` |

Preserve original P001–P218 decorative positions and create a separate allocation report for each conflicting logical terminal. Do not use a mesh as proof that a SparkTerminal is registered.

## ANNEX E — SOURCE OBJECTIVE CERTIFICATION RECORDS

| # | Source exact objective title | Source authority | Status |
|---:|---|---|---|
| 1 | **Build the Attitude Control Network** | `REAL_GRAPH_AND_DEVICE_COMMISSIONING` | `DESIGN_ONLY_NOT_IMPLEMENTED` |
| 2 | **Stop the Tumble and Find the Stars** | `DYNAMICS_AND_SENSOR_EVIDENCE` | `DESIGN_ONLY_NOT_IMPLEMENTED` |
| 3 | **Achieve Precision Three-Axis Pointing** | `REAL_ATTITUDE_AND_ACTUATOR_EVIDENCE` | `DESIGN_ONLY_NOT_IMPLEMENTED` |
| 4 | **Hold the Optical Beam** | `TWO_PASS_QUALIFICATION` | `DESIGN_ONLY_NOT_IMPLEMENTED` |
| 5 | **Survive Attitude-Control Disturbances** | `THREE_RECOVERIES_AND_FRESH_MISSION` | `DESIGN_ONLY_NOT_IMPLEMENTED` |
| 6 | **Restore Critical Star-Tracker Feedback** | `LATCHED_FAULT_AND_FRESH_POST_REPAIR_QUALIFICATION` | `DESIGN_ONLY_NOT_IMPLEMENTED` |

Original `RequiredEvidence` fields (verbatim):

- **1. Build the Attitude Control Network:** 142 actual player wires, 218 functional ports, valid power/sensors/actuators/safety and trusted W097 feedback
- **2. Stop the Tumble and Find the Stars:** Calibrated gyro/magnetic coarse estimate, actual angular rate under threshold, valid primary star-tracker fix
- **3. Achieve Precision Three-Axis Pointing:** Four wheel models allocated; controlled target slew, fine pointing, estimator confidence, momentum and jitter margin
- **4. Hold the Optical Beam:** Two real scheduled ground-station passes, actual beam lock, valid power, tracking/jitter margin and qualified desaturation
- **5. Survive Attitude-Control Disturbances:** Actual gyro bias, wheel saturation and flexible solar-wing jitter faults repaired followed by fresh two-pass qualification
- **6. Restore Critical Star-Tracker Feedback:** Deliberate real W097 removal, stale quaternion fault, safe coarse-mode, physical reconnect, fresh stars/calibration, explicit reset and new two-pass mission

## ANNEX F — SOURCE RECONCILIATION SUMMARY AND KNOWN AMBIGUITIES

- **Original art anchors:** 218 P IDs.
- **Proposed wire rows:** 142 distinct W IDs → 284 endpoint uses.
- **Art anchors actually referenced by wiring CSV:** 199; **unreferenced art anchors:** 19.
- **Art anchors reused by multiple planned wires:** 47. Each such anchor corresponds to multiple differently named proposed logical ports and requires an explicit reconciliation decision.
- **Same-anchor cross-`SignalKind` conflict count in the concept CSV:** 0; **this does not guarantee compatible electrical nets** because distinct same-kind measurement/control signals must usually stay isolated.
- **P001:** ten 5 V source names share a visual anchor; if configured as a legitimate 5 V bus, verify current/fanout and circuit returns.
- **P064:** magnetic estimator vector, gyro sample validity, horizon direction and W097 quaternion are different logical inputs despite sharing an analog-labeled art location. **They must not be shorted.**
- **P139:** dedicated original art socket for W097 source; `P064` requires a distinct functional subport for W097 destination.
- **W097:** `L84_PrimaryStarTracker` `STAR_TRACKER_QUATERNION_OUT` (`P139`) → `L84_AttitudeStateEstimator` `ADCS_ABSOLUTE_ATTITUDE_IN` (`P064`), conceptual `ANALOG_FEEDBACK`, only `Critical=YES` connection.
- **Status of simulations and tests in ZIP:** no actual player wires, no functional SparkTerminal components, no real ADCS plant, no built objectives, no Unity compile/Play Mode tests. Editor code provided is art tooling only.

**MANDATORY FOR IMPLEMENTER:** issue `Level84_TerminalReconciliationReport.csv`, `Level84_ImportedAssetValidation.json`, `Level84_WiringValidation.json`, `Level84_ScenarioTestReport.md` and a truthful Play Mode completion status; label any unrun test NOT RUN.

---

**FINAL TASK TO THE UNITY IMPLEMENTER:** Open the real Project Spark code and supplied ZIP; generate project-compatible runnable C# and editor assets, assemble Level 84, implement truthful physics, wiring and safety, execute actual tests where possible, and only claim complete after the in-game `LevelGamePlayManager` verifies real **6/6**.

