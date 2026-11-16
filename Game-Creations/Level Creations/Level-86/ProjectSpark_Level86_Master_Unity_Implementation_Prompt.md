# PROJECT SPARK — LEVEL 86 MASTER UNITY IMPLEMENTATION PROMPT

## LEVEL 86 — THE ORBITAL FORGE

**Advanced AAA Space Robotics • Seven-Axis Orbital Manipulation • Sensor Fusion & Six-Axis Force-Torque Feedback • Compliant Contact • Flexible Radiator Servicing • Thermal Loop Restoration • Independent Safety • Critical W129 Trust Recovery**

> **SOURCE-GROUNDED IMPLEMENTATION SPECIFICATION:** Prepared against the actual supplied `ProjectSpark_Level86_HD_AAA_Models.zip`, manifests, CSVs and included Unity art-helper source. Exact model names, pivot axes and positions, planned wire source/destination pairs, socket identifiers, reconciliation counts and W129 route are extracted from the existing package. All dynamic simulation, gameplay integrations, thresholds, editor-generation requirements and proposed runtime architecture below are **work to implement**, not functionality already included in the art kit. Do not substitute renders or images for the source FBX meshes.

# 01. PRIMARY MANDATE

Act as my **senior Unity C# gameplay engineer, Project Spark electrical-system integration specialist, seven-degree-of-freedom robotics engineer, inverse-kinematics and manipulation-control architect, servo and motor-drive engineer, mechatronics and contact-dynamics specialist, optical metrology / sensor-fusion engineer, flexible-payload and structural-vibration engineer, spacecraft thermal-control engineer, fluid-loop educational simulator designer, industrial PLC and functional-safety engineer, high-fidelity gameplay systems designer, AAA technical artist, and Unity Editor automation developer**.

**IMPLEMENT THE COMPLETE, FUNCTIONAL, PLAYABLE LEVEL 86 — THE ORBITAL FORGE — INSIDE THE EXISTING PROJECT SPARK UNITY PROJECT.** Import the supplied **82 actual numbered FBXs**, use their existing names and pivot hierarchy, and integrate them with real player-created electrical circuits, independently simulated robot and thermal processes, actual inspection / extraction / replacement interactions, six evidence-backed objectives, three physically consequential disturbances and final **W129** trusted force/torque feedback disconnection and repair. Do not redesign the assets or manufacture a new rendered image.

**Final success is only a genuine `LevelGamePlayManager` result of 6/6 after real player operations, fresh post-W129 qualification and actual Unity runtime testing. Preserve Levels 1–85.** No editor button, cosmetic animation, fake terminal, sample SCADA number, preconnected cable, or time-elapsed trigger may substitute for those conditions.

# 02. READ THESE EXACT PACKAGE SOURCES FIRST

Open `ProjectSpark_Level86_HD_AAA_Models.zip` and inspect its actual files before implementing anything:

- `Model_Manifest.json`, `Verification_Report.json`, `Art_Assembly.csv`, `Component_Roles.csv`, `README_Unity.txt`.
- `Animation_Rigs.csv` (all **64** actual art pivot parents), `Robot_Joint_Articulation_Chain.csv` (seven serial joints and rail parent).
- `Gameplay_Wiring_Concept_W001_W152.csv`, `Gameplay_Wiring_Budget.csv` (**152** planned player connections).
- `Terminal_Anchor_Plan_230.csv`, `Visual_Terminal_Sockets_230.csv` (**230** modeled but decorative socket anchors).
- `Terminal_Logical_Port_Reconciliation_304.csv` (**304** proposed independent endpoints); `Terminal_Anchor_Signal_Conflict_Report.csv` (**88** reused/multi-contact anchor records).
- `Objective_Certification_Plan_6.csv`, `W129_Critical_Contact_Force_Recovery.md`, `Gameplay_Integration_Plan.md`.
- `AAA_Art_Delivery_Notes.md`, `AAA_Polish_And_Lighting_Plan.md`, `Collision_LOD_Production_Plan.csv`.
- `Texture_Manifest.json`, `Material_Palette.json`, `Checksums_SHA256.txt`.
- `Unity_Optional/SparkLevel86ArtPlacementEditor.cs`, `SparkLevel86MaterialUpgradeEditor.cs`, `SparkLevel86AssetValidatorEditor.cs`, `SparkLevel86VisualRig.cs`.

Annexes below reproduce the important **source-derived** tables. Use the original CSV/JSON package as the master data at build time; investigate discrepancies instead of silently trusting a manually transcribed table.

# 03. VERIFIED ART BASELINE — WHAT EXISTS AND WHAT DOES NOT

The actual source manifest describes **82 modular FBX 7400 meshes**, **82 corresponding GLBs**, **1 assembled showcase FBX + 1 showcase GLB**, **122,186 total modular triangles**, **993 mesh parts**, **64 named animation pivots**, **230 `SOCKET_P###` decorative art locations**, **152 planned conceptual W-wire entries**, **304 distinct proposed endpoint contacts**, **88 reported reused-anchor reconciliation records**, **six objective design rows**, and **12 procedural reference PBR material sets / 36 PNG maps**. The recorded world bounds (metres) are **min (−15.0, −2.45, −17.15)** and **max (+15.55265, +12.74089, +8.4)**, giving approximately **30.55265 m width × 15.19089 m height × 25.55 m depth**. The lower Y includes background/stage geometry; do not assume the gameplay walking floor is at −2.45 m.

The package verification is a *structural art* check, not Unity behavior verification. Its flags explicitly report `actual_player_wires_included=false`, `functional_sparkterminals_included=false`, `actual_robot_dynamics_solver_included=false`, `actual_thermal_simulator_included=false`, `six_runtime_objectives_implemented=false`, `unity_compilation_tested=false`, and `unity_playmode_tested=false`. The supplied meshes have attractive procedural industrial detailing but are **AAA-inspired production blockout/technical art**, not finalized artist-authored, UV-baked, LOD/collider-finished assets. Acknowledge that honestly.

# 04. LEVEL IDENTITY, CONTINUITY AND PLAYER EXPERIENCE

- Level key: `level_86`; main title **The Orbital Forge**; subtitle **Advanced Space Robotics and Thermal Station Restoration**; expert/master difficulty.
- Narrative: After Level 85 docking, the player must restore the station's failed external thermal radiator using a seven-axis remote robotic system and tools carried in the docked spacecraft.
- Six objective certifications with **historical completion** separated from real-time safety readiness; initial progress is **0/6**.
- Actual source-critical row: **W129**, `L86_SixAxisWristForceTorqueSensor/P045` → `L86_RoboticContactSafetyPLC/P119`, `ANALOG_FEEDBACK`.
- New full gameplay builder menu target: `Tools/Project Spark/Levels/Create Level 86`.
- Existing art-only placement menu: `Tools/Project Spark/Levels/86/Place HD Art Under Selected Layout`.

Level 86 must feel different from Level 85: it is not another spacecraft trajectory or docking test. It focuses on serial-link motion, real contact and grasp constraints, structure/payload interactions, fastening, fluid/thermal restoration, and contact-safe fault recovery.

# 05. INSPECT THE EXISTING PROJECT SPARK APIS AND LEVEL INFRASTRUCTURE

Search the actual Unity codebase and inspect signatures, serialized fields, interfaces and event sequencing of `SparkElectricalSolver`, `SparkCircuitSystem`, `SparkElectricalComponent`, `SparkTerminal`, `SparkSwitch`, `SparkLevelDefinition`, `SparkLevelEvaluator`, `SparkLevelSceneBindings`, `SparkLevelWorkstationBindingBridge`, `SparkShelfSlot`, `SparkObjectShelf`, `SparkLevelLayoutLoader`, `SparkLevel`, `LevelGamePlayManager`, level/shop/shelf/purchase/throw systems, wire placement/removal, PLC, safety latch/reset, interaction and save/load. Treat *previous Level 82–85 prompts* as design references, not proof that a method or runtime class exists.

Inspect the actually implemented Lessons/Levels 1–85 for reusable solvers, event history, sensor timebase, motor interpolation, object placement and thermal systems. Use actual namespaces and method signatures. Build slim adapters when needed and do not overwrite earlier level assets, shared prefabs, level progression or editor menu scripts.

# 06. NON-NEGOTIABLE IMPLEMENTATION RULES

1. Preserve Levels **1–85**, level unlock/progress data, object-shelf inventory, shop/purchases, circuits and shared level managers.
2. Begin with the 152 puzzle wires **unconnected**. Each is a genuine player-created edge accepted by the real Spark electrical topology solver, never an already-connected decorative mesh.
3. Preserve the 230 source art anchors even when creating **304 independent functional contacts**; their counts represent *different things*. Distinct electrical signals cannot be electrically shorted merely because the visual art reuses one socket.
4. Distinguish **5 V educational control and returns**, **fictional robot motor/actuator energy**, **fictional thermal/coolant system**, and **independent safety authority**. Avoid physically impossible high-power currents through 5 V logic wires.
5. True plant state, measured sensor samples, controller estimates, user commands, visual animations and objective evidence are distinct and should have explicit owners.
6. Seven actual joints plus rail move with finite velocities, acceleration, torque, motor limits, encoder bias and brake states; requested pose never automatically becomes actual pose.
7. Zero gravity does **not** mean zero mass or inertia. Heavy radiator panels retain translational/rotational inertia and can excite flexing or collisions.
8. Independent robot-to-payload contact state is authoritative; an apparent overlap or gripper-green light does not prove a stable force-balanced grasp.
9. Never open grippers, disable brakes, power off necessary holding mechanisms or release a radiator while it is unsupported, including during W129 loss.
10. Coolant isolation, pressure relief, electrical isolation and leak checks must be real fictional system states, not animation shortcuts.
11. Heat is conserved in a coherent game-scale model; replacing a radiator does not instantly reset station temperature.
12. W129 loss makes trusted six-axis contact feedback **invalid/stale**, not zero force/torque. Old samples, patch diagnostics and SCADA telemetry are not an acceptable fallback for normal contact permission.
13. Independent limits, collision detection, safe brakes, emergency stop and gripper-support logic remain effective even if W129 is absent.
14. Three disturbances must mutate real simulation state and require diagnosis/repair and new test evidence.
15. Historical Objectives 1–5 remain certified during final deliberate W129 fault. Objective 6 needs a **new post-repair runId** and all required test evidence.
16. Cinematic cameras, robot pivots, indicator lights and SCADA display only authoritative state. They never directly mark objectives PASS.
17. Educational thresholds are configurable fictional game rules, **not real spacecraft servicing certification or physical coolant/pressure-vessel safety instructions**.
18. Never report Unity compilation, Play Mode, 6/6, robotics safety or thermal restoration as passed if not actually exercised.

# 07. INITIAL LEVEL STATE — REAL 0/6

Set the station's failed radiator under an actual modeled heat-load stress; the player has a usable but uncommissioned robot, calibrated references unavailable, tools staged, the defective panel still bolted into place, and a replacement panel inside the docked spacecraft. External service zone is restricted until the robot safety PLC and electrical network are commissioned. Thermal isolation valves and pump must have safe start states; pressure/temperature values must be physically consistent with those states. W129 is **not** connected until the player builds the required network, and it must be valid before commissioning contact tasks. No severe injected test fault is active at scene start. Objective count is 0/6.

# 08. ART IMPORT, SCALE AND WORLD COORDINATES

All **82 numbered modular `Models/*.fbx`** have authored world-baked shared-origin geometry. Their FBX numeric positions are in **centimetres** with `UnitScaleFactor=1`; matching GLBs encode **metres**. Y up, X width and Z forward. Convert centimetres to Unity metres **exactly once**, test a known structural dimension, and ensure pivots/anchors transform consistently with renderer geometry. Keep imported modular copies at local position `(0,0,0)`, identity rotation and unit GameObject scale *after correct importer scaling*. Make **`L86_HD_VISUAL_ART`** a disposable/rebuildable art-only root under the Level 86 layout.

Do **not** instantiate `Models/Level86_Complete_ArtKit_Showcase.fbx` or `GLB/Level86_Complete_ArtKit_Showcase.glb` alongside the 82 modular pieces. Do not add actual `SparkTerminal`, gameplay colliders, source sensors, simulation controllers or permanent interaction scripts beneath an art root that can be replaced by the supplied Editor helper. Prefer separate `L86_GAMEPLAY`, `L86_ELECTRICAL`, `L86_PHYSICS`, `L86_SAFETY`, and `L86_UI` roots with explicit stable bindings. Use Undo / prefab transactions and repeatable editor updates.

# 09. REUSE THE EXACT PROVIDED UNITY ART HELPERS

The package supplies actual C# in `ProjectSpark.EditorTools`:

- `SparkLevel86ArtPlacementEditor.cs` menu: **`Tools/Project Spark/Levels/86/Place HD Art Under Selected Layout`**; authoring root `L86_HD_VISUAL_ART`.
- `SparkLevel86MaterialUpgradeEditor.cs` menu: **`Tools/Project Spark/Levels/86/Create & Apply HD Materials To Selected Art`**; assigns reference PBR color/material properties, not fully baked authored production maps.
- `SparkLevel86AssetValidatorEditor.cs` menu: **`Tools/Project Spark/Levels/86/Validate Imported HD Art`**; checks imported FBXs, the 64 named pivots, 230 decorative sockets and renderer count.
- Runtime class `ProjectSpark.Visuals.SparkLevel86VisualRig` is a visual-only animator and light/status bridge.

Place `*Editor.cs` under a Unity `Editor` folder and the visual rig outside Editor assemblies. These are **not** the requested full level gameplay builder. Do not invent a menu name beyond these verified art menus when describing what already exists.

# 10. AAA FINISH, MATERIALS, LIGHTING, LOD AND COLLISION PRODUCTION

The source has **12 procedural texture sets × 3 maps = 36 PNGs** at 1024²; each set includes Albedo, Normal and packed `MetalRoughAO` where **R=Metallic, G=Roughness, B=Ambient Occlusion**. Adjust URP/HDRP shader packing (invert roughness to smoothness as necessary). Do not claim correct maps are already UV-authored on the final meshes. Add production UV0/UV1, tangent normals, suitable authored edge-wear and AO, high-quality chamfers, controlled metal anisotropy, material atlasing, emissive safety decal materials, and lightmap UVs where applicable. Finish the seven joints, gripper pads, bolt drivers, radiator manifold and inspection camera with correct appearance.

Make logical LOD0/LOD1/LOD2, static batching for structural trusses, GPU instancing for repeated fins/fasteners, dynamic link meshes following the correct robot transforms, simplified compound colliders for robot links, convex tool-gripper-radiator collision proxies, interaction colliders for buttons/sockets and broadphase keep-out zones. Use `Collision_LOD_Production_Plan.csv` as a production guide, not proof colliders already exist. Warm amber hazard strips, cool cyan instrument glow and neutral station work lighting should expose connectors/bolts clearly; physical safety state determines light FX.

# 11. VERIFIED `SparkLevel86VisualRig` API — EXACT PUBLIC SIGNATURES

The package's actual visual component is **`ProjectSpark.Visuals.SparkLevel86VisualRig`**. Its provided public calls are:

```csharp
public void BindVisuals();
public void SetActualRobotJointAngles(float j1, float j2, float j3, float j4,
    float j5, float j6, float j7, float railMeters);
public void SetActualServicingMechanisms(float jawOpen01, float releasedPanel01,
    float liftPanel01, float actualCoolantValveDegrees);
public void SetActualSafetyState(bool trustedW129Fresh, bool criticalFaultLatched,
    bool radiatorCoolingQualified);
```

`SetActualRobotJointAngles` takes **degrees** for all joints, and rail displacement in **metres**; the visual script currently clamps that rail to `[-2.5,+2.5]`. Gripper/failed panel/replacement panel parameters are normalized 0–1; coolant valve uses degrees clamped 0–90. It drives selected pivot animations in `LateUpdate` and three optional `Light` fields. It does **not** implement inverse kinematics, joint drives, physics, force measurement, battery/thermal simulation, PLC or objective completion. Make a tested state-to-rig adapter, and never call nonexistent methods. Where more animations are needed, extend the visual implementation carefully without overwriting an existing working project version.

# 12. SEVEN-JOINT VISUAL PARENT CHAIN — CRITICAL INTEGRATION

The seven joint models are separate world-baked FBXs; leaving their animation parents as siblings will rotate disconnected links. Read `Robot_Joint_Articulation_Chain.csv` and, after import, construct a **serial wrist chain** preserving world transforms: `ANIM_LINEAR_RAIL_CARRIAGE` → `ANIM_ROBOT_BASE_ROTATE` → `ANIM_ROBOT_SHOULDER_PITCH` → `ANIM_ROBOT_UPPERARM_PITCH` → `ANIM_ROBOT_ELBOW_PITCH` → `ANIM_ROBOT_FOREARM_ROLL` → `ANIM_ROBOT_WRIST_PITCH` → `ANIM_ROBOT_WRIST_ROLL`, with wrist tools parented to the actual wrist transform. Use source joint axes **Y, Z, Z, Z, X, Z, X** respectively. The package visual rig attempts this reparenting in `BindVisuals()`; validate actual rendering hierarchy and preservation of the original per-module mesh ownership, avoid adding duplicate pivots and avoid double-reparenting on builder reruns.

**Separate kinematic/physics authority from this visual chain.** Use a validated joint reference frame model and corrected pivots for full spatial forward kinematics, collision and contact. The visual hierarchy serves only as a presentation of measured joint angles. Never derive true joint angle, end-effector transform or safety qualification from rendered links. The 64 pivots are source-authored animation parents, including display-only `Kind=indicator` records; don't rotate indicators as physical actuators.

# 13. TWO INTERACTING SYSTEMS PLUS INDEPENDENT SAFETY

**Real player electrical topology:** Spark solver, 152 detachable connections, functional terminals, 5 V supplies/returns and typed analog/digital/status/command/data signals. **Robot and thermal process plant:** authoritative 7-DoF joint dynamics and kinematics, tools, contacts, radiator structure, coolant and thermal energy, faults and measurements. **Independent safety domain:** PLC, gripper/payload-preserving safe-hold, brakes, over-torque limits, emergency stop, collision inhibition, coolant isolation and W129 trust boundary. Supervisory SCADA is a display/control interface, never an alternate sensor authority. Validate every cross-domain interface explicitly.

# 14. EXACT 152 PLAYER-WIRE BUDGET

Use `Gameplay_Wiring_Concept_W001_W152.csv` as the source of exact model pairs, purpose, `SignalKind`, anchor IDs and `Critical` flags. Do not create generic wires instead. The budget is:

| WIDs | Source group | Required wires |
|---|---|---:|
| W001–W012 | Educational 5 V control network and independent return checks | 12 |
| W013–W036 | Seven-axis joint servos, encoders and motor torque feedback | 24 |
| W037–W056 | Stereo vision, depth metrology, kinematics and collision avoidance | 20 |
| W057–W076 | Six-axis force-torque, compliant control and flexible payload dynamics | 20 |
| W077–W094 | End effector, gripper, tool changer and fastening | 18 |
| W095–W112 | Radiator thermal interface, coolant and leak-test system | 18 |
| W113–W136 | Independent safety, fault recovery and critical W129 | 24 |
| W137–W152 | SCADA, event recording and six-objective certification | 16 |
| **Total** | | **152** |

The `Status` field in every source wire row is `CONCEPT_ONLY_NOT_PLAYER_CREATED`. Build declarations for exactly these 152 required edges but let the **player actually connect** them using the existing Project Spark wire placement interaction. Support disconnect/reconnect at runtime. Validate source/target model, exact proposed logical ports, input/output role, signal domain, common return/power/reference, current electrical solver continuity, duplicate/junction constraints and live connection evidence. Wire IDs must remain stable across save/load.

# 15. 230 DECORATIVE SOCKETS VS. 304 FUNCTIONAL CONTACTS

The 230 source `SOCKET_P001…SOCKET_P230` meshes are **visual art**, not `SparkTerminal`. The 152 conceptual wires have 304 endpoint occurrences, each separately named in `Terminal_Logical_Port_Reconciliation_304.csv`. The conflict CSV contains **88 reused-anchor records**, not 88 prebuilt electrical shorts. Some reuse is electrically benign; other anchors share *incompatible* signal classes. Reconcile in a deterministic pass:

1. Validate each `P###` exists on its listed owner model and is correctly scaled to metres.
2. For each endpoint, create/identify its unique proposed `FunctionalPortID`, correct `Direction` and `SignalKind`.
3. Group contacts by `ArtAnchor` and owner; only share a real electrical node if voltage/signal domain, direction, multiple-connection capacity and intended bus semantics allow it.
4. For mixed/specific contacts, create distinct functional child terminals or a legitimate multi-contact junction/pin block with small visually readable local offset, preserving the original art socket name and origin.
5. Keep all 152 wire IDs and 230 decorative sockets intact, plus the 304 endpoint mapping records. If the native Spark API cannot support some required typed signal, implement a small explicit adapter and report the compromise; do **not** bypass the solver.
6. Export a machine-readable terminal reconciliation report and fail validation if any unrelated power, status, safety, analog or data net is shorted.

Source example: `P045` on the wrist sensor participates in W129 as trusted `ANALOG_FEEDBACK`; `P119` at the contact PLC also hosts other routes, and must not collapse those connections into one mixed-role terminal. **The real W129 route must remain individually removable** despite shared art locations.

# 16. EXACT W129 TRUSTED CONTACT SIGNAL — ART SOURCE

`W129` is the **only** source row with `Critical=YES` and has exactly:

- **Source model:** `L86_SixAxisWristForceTorqueSensor`; **source art socket:** `P045`.
- **Target model:** `L86_RoboticContactSafetyPLC`; **target art socket:** `P119`.
- **Source logical ID:** `W129_OUT_ANALOG_FEEDBACK`.
- **Target logical ID:** `W129_IN_ANALOG_FEEDBACK`.
- **SignalKind:** `ANALOG_FEEDBACK`.
- **Purpose:** `CRITICAL trusted six-axis wrist contact wrench W129`.
- **Original source status:** `CONCEPT_ONLY_NOT_PLAYER_CREATED`.

**W129 is one real detachable gameplay circuit edge that gates a trustworthy six-component sampled wrench** (`Fx,Fy,Fz,Tx,Ty,Tz`) with calibration, range and age. If the existing Spark electrical system only carries scalar analog channels, design a documented *single edge with explicitly modeled multiplexed/bundled measurement semantics* and verify the six measurements cannot be faked by client UI; do not silently pretend a raw single voltage is six independent analog signals. Maintain any source conditioning / power / return requirements via actual modeled device interfaces. Contact-protection truth comes from valid live W129 topology plus measured safety state, not from a display approximation.

# 17. W129 NO-BYPASS RULES AND NEIGHBORING SIGNALS

Nearby planned rows have distinct roles:

- **W128:** `L86_ContactForceSignalConditioner` → `L86_W129CriticalFeedbackPatchStation`: conditioned wrist-load diagnostic patch. Not a replacement for W129 trusted PLC input.
- **W130:** patch station → contact safety PLC: patch continuity **diagnostic only**; cannot substitute six-axis force samples.
- **W131:** safety PLC → interlock diagnostic rack: W129 freshness/fault **status**, not the trusted contact measurement.
- **W132:** diagnostic rack → safety PLC: **explicit operator reset request**, accepted only after requalification prerequisites.
- **W152:** patch station → holographic SCADA: diagnostic **display only**.

Use actual CSV classifications for these WIDs. No cache, mirrored SCADA measurement, pre-fault sample, alarm flag or trusted-wire indicator may make a missing W129 look valid. Independent limits and interlocks remain capable of stopping risky motion, but cannot silently serve as an ordinary W129 substitute.

# 18. AUTHORITATIVE PROCESS DATA MODEL

Maintain one authoritative Level 86 process state including: full joint calibration and commanded/measured angle, velocity, acceleration and torque; seven drive currents/limits/brake states and rail position; kinematic frames/tool TCP and Jacobian quality; workspace obstacles and clearances; optical calibration/time-stamped stereo/depth observations and pose confidence; tool changer lock/grip/mount status; individual jaw position, pressure and contact geometry; true vs measured six-axis wrist wrench, sample range/freshness/bias/overload; gripped payload ownership/mass/inertia and flex modal displacement; actual fastener/connector engagement; panel mount/disposal/training cassette state; radiator coolant ports/valves, pressure, flow, thermal load/capacity and heat rejection; safety interlocks/latches/W129; faults, event history, evidence `runId`, objective historical certifications and save/load identities. Never let independent UI panels each invent a private authoritative temperature or torque.

# 19. DETERMINISTIC UPDATE ORDER

Use a stable simulation tick (e.g. game-scale `FixedUpdate` or an explicitly stepped service), documented SI-style units and seeded fault replay. Recommended dependency order:

1. Read actual Spark electrical graph and player commands.
2. Validate control supplies, returns, sensor power, independent safety and fault latches.
3. Filter motion/fastening/isolation commands through the safety arbiter.
4. Advance motor/rail/actuator/brake state with bounded dynamics.
5. Solve 7-axis link transforms and joint/contact constraints, including actual payload attachment.
6. Detect collisions/contact, update gripper grasp/fastener/interface interactions.
7. Integrate payload rigid and flexible motion and station reaction loads.
8. Integrate coolant valve/pump behavior, pressure/flow and thermal energy.
9. Sample sensors with calibration, delay, noise, timestamp and wire continuity.
10. Update estimator / compliant controller / health monitor / independent protection from fresh measurements.
11. Update sequencing PLC; capture ordered safety, procedure and test events.
12. Evaluate objective evidence and historical certification.
13. Publish read-only snapshots to SCADA, holograms, lights, VFX and visual rig.

If exact causality requires feedback from the previous tick, document it; do not form an unstable instantaneous algebraic loop between commanded torque and measured contact.

# 20. SEVEN-DEGREE-OF-FREEDOM KINEMATICS

Implement joint-local transform chains using accurate joint centers and axes from the source hierarchy. Calculate forward kinematics `T_world_tcp(q, rail)`, end-effector pose, Jacobian and workspace envelope from **real kinematic frames**, not just the imported animation transform after interpolation. A redundant 7-DoF arm may have multiple inverse solutions: choose solutions using joint-limit avoidance, obstacle clearance, manipulability, rail position, service bay reachable space and minimal reconfiguration. Prevent false success when inverse kinematics fails or a singularity makes commanded contact motion unsafe. Offer manual jog and constrained guided planning with measured Cartesian pose and joint-space feedback.

# 21. FINITE SERVO MOTION, DRIVE TORQUE AND BRAKES

For each axis represent requested position/velocity/torque and actual motor effort, inertia, friction/backlash proxy, speed/acceleration/jerk caps, torque/current/thermal derating, measured encoder and brake state. Use an educational bounded servo control with stable discretization and explicit saturations; avoid infinite torque to hit a setpoint. `L86_JointServoDriveRack`, `L86_JointEncoderFeedbackArray`, `L86_JointTorqueCurrentSensorRack`, `L86_RobotBrakeAndTorqueLimiter` and `L86_RoboticHomingCalibrationFixture` must have meaningful actual-state interactions. Brake confirmation differs from brake command. A powered-down motor with a gripped unsupported mass must preserve a defined fail-safe holding state where physically supportable; otherwise report unsafe support honestly.

# 22. RAIL CARRIAGE, DOCKED SPACECRAFT AND FRAME REGISTRATION

The robotic arm's base moves on `L86_RobotLinearTraverseRail` while the station and docked cargo spacecraft define different reference frames. Track requested/actual rail displacement, rail drive limits, homing sensors, tool-to-base transform, station mounting frame, replacement-panel cradle pose and inspection-camera calibration. Treat the cargo spacecraft as already docked to the station from Level 85. Do not interpret the fixed Stage art coordinates as true orbital mechanics or invent a second independent rendezvous mission. Rail movement and arm kinematics must remain consistent with the visual rig's ±2.5 m rail display clamp or the rig adapter must be updated and documented.

# 23. MACHINE VISION, STEREO CALIBRATION AND METROLOGY

Implement left/right camera calibration, baseline/extrinsic orientation, lens distortion proxy, pixel/object correspondence uncertainty, depth scanner range/field-of-view, station fiducial observations, occlusion, lighting/glare effects and timestamped pose estimates. Use `L86_StereoMachineVisionCameraLeft`, `L86_StereoMachineVisionCameraRight`, `L86_StructuredLightDepthScanner`, `L86_OpticalFiducialTargetArray`, `L86_PrecisionVisionCalibrationBoard`, `L86_LaserAlignmentMetrologyUnit` and `L86_StereoVisionFusionComputer`. Calibration must produce real changed sensor quality; repeated pressing CALIBRATE cannot pass without proper target observation. Occluded or stale measurements invalidate precision insertion authorization, while a calibrated control observer may continue limited safe motion where explicitly permitted.

# 24. COLLISION-AWARE TRAJECTORY PLANNING

Construct a 3D collision map with station trusses, service hatch, docked spacecraft, radiator panels, coolant ports, tool magazine and no-go human-analog service zones. Plan joint/Cartesian trajectories with continuous clearance checks, joint limits, tool geometry, payload inflated bounds and uncertainty margin. Support `L86_SevenAxisReachEnvelopeHologram`, `L86_CollisionProximityScanner` and `L86_RobotTrajectorySafetySupervisor` as visual/logic devices; don't treat a holographic safe green region as a solver substitute. Abort/reroute when actual state deviates or an obstacle emerges; do not teleport through station structure.

# 25. SIX-AXIS FORCE/TORQUE SENSOR AND CONTACT PHYSICS

Represent wrist **forces** in Newton-style game units and torques in N·m-style game units in a declared tool frame, with calibration offsets, cross-axis coupling proxy, noise, range limits, overload flags, latency and sample freshness. The true contact wrench derives from contact geometry and payload inertial/elastic loads, not from the HUD. `L86_SixAxisWristForceTorqueSensor`, `L86_ForceTorqueCalibrationFixture`, `L86_ContactForceSignalConditioner`, `L86_CompliantInsertionController`, `L86_SixAxisContactForceHologram` should connect through the actual planned signal routes. A sensor disconnected by W129 may still be physically loaded; mark the *measurement* invalid and constrain motion, do not clear the true load.

# 26. COMPLIANT INSERTION AND VIRTUAL IMPEDANCE

Use controlled compliance/impedance in the small insertion envelope so the gripper can align a rigid radiator interface within safe force/torque margins; implement bounded virtual stiffness/damping and actuator effort with measured sensor age/limits. Contact force should rise when a connector is misaligned rather than being magically resolved by an ALIGN button. Insertion succeeds only if actual pose, contact geometry, alignment, depth and force/time profiles satisfy the procedure; excessive moment or sideways force can damage a fictional connector and require inspection/replacement. Never let a visual animation alone snap two mechanically incompatible faces together.

# 27. GRIPPER PHYSICS, CONTACT DISTRIBUTION AND PAYLOAD SUPPORT

Model two independent jaw pads with commanded vs achieved separation, pressure/holding force, pad contact geometry, grasp center error, actual gripper mass and supported payload reference. Calculate a simplified friction/contact-wrench margin for panel inertia. `L86_AdaptiveRadiatorHandlingGripper`, `L86_DualJawGripperPressureSensorArray`, `L86_RadiatorGrappleHandleFixture`, `L86_DualJawGripperPressureSensorArray` need inspectable state. Do not allow the player to carry a radiator because a collider intersects the gripper; require actual secure clamp and tool-change latch. A W129 fault while holding a panel should inhibit risky motion and preserve a valid mechanical grasp if available; do not automatically unclamp. Log support loss if no physically plausible holding strategy remains.

# 28. TOOL CHANGER, TORQUE DRIVER AND FASTENER CERTIFICATION

Use `L86_AutomaticEndEffectorToolChanger`, `L86_PrecisionTorqueFasteningDriver`, `L86_RoboticToolDockAndCalibrationNest` and fastening cassette assets to implement distinct tool acquire/release, positive lock, tool TCP offset and identity verification. Define finite bolt engagement progress with actual speed/torque angle, wrong tool/position rejection, cross-thread/misalignment proxy, tightening sequence, final clamp-load proxy, and duplicate bolt identification. Every bolt must be fully released before a radiator can be removed. At installation, a uniform fastening animation cannot certify all fasteners; each location needs its own valid evidence.

# 29. FAILED RADIATOR REMOVAL PROCEDURE

The defective module remains fixed until independently verified: station thermal load is managed, coolant path isolated, pump state safe, pressure relieved, connectors disconnected or protected, attachment fasteners released, payload gripped and truss clearance planned. Only then permit `L86_FailedThermalRadiatorModule` to detach from mount. Move it using actual arm/payload dynamics and verified trajectory to `L86_FailedRadiatorDisposalCradle` (use exact model name from Annex A if source differs). Treat the disposal cradle as a mechanically restrained receiving station with engagement confirmation. Accidental release/collision invalidates Objective 3 even if the model ends up visually near the cradle.

# 30. REPLACEMENT RADIATOR HANDLING AND INSTALLATION

The `L86_ReplacementThermalRadiatorModule` is staged in the docked cargo ship. Require real hatch/rail access, compatible tool and grip, payload pickup, movement with inertia and oscillation constraints, fiducial target acquisition, alignment at mount, compliant contact, supported load transfer and fastener engagement. Transfer ownership from robot to station mount only when latches/bolts and load support truly qualify. Do not clip the panel into place by setting its transform or immediately mark all interfaces connected. Use actual model names/positions from manifest/assembly and create the missing dedicated gameplay colliders/anchors separately.

# 31. FLEXIBLE RADIATOR STRUCTURE AND OSCILLATION

Use an educational low-order flex model (e.g. two bending modes or a spring-mass-damper proxy) coupled to acceleration and applied contact forces. Track modal amplitude, frequency, damping, stress proxy and held payload orientation. Sudden robot acceleration should excite visible radiator motion and add measured wrist forces/moments; passive damping reduces them over time. Motion-shaping and active damping should have bounded authority and time, not instantly zero vibration. `L86_FlexiblePayloadModalSensor` and `L86_ActiveVibrationDampingController` should read real modeled state. Distinguish rigid main-body motion from visual flex, and do not use a skinning offset to calculate structural safety.

# 32. COOLANT SYSTEM — ISOLATION, PRESSURE AND FLOW

Implement a bounded fictional station coolant-loop plant with reservoir/line pressure, pump commands and actual rpm, valve positions, independent isolation, temperature-dependent transport, pressure loss, leaks and measured flow. Models include the source radiator interfaces, coolant valves, connectors, flow/pressure instrumentation, pump and seal/leak analyzer. **Do not provide real-world pressure-system operating instructions**; use game-labeled normalized units and conservative configurable limits. The defective radiator must be truly isolated and depressurized in the *simulated plant* before disconnecting ports. After installation, run a new interface leak check and gradual re-pressurization before enabling normal heat rejection. A display reading '0 L/min' cannot independently prove zero pressure; a commanded valve CLOSED is not actual valve-closed feedback.

# 33. THERMAL ENERGY CONSERVATION — RESTORE COOLING, NOT A FLAG

Model station avionics and thermal loop as finite thermal capacitances with a consistent energy balance such as `C_total * dT/dt = Q_avionics + Q_pump_loss − Q_radiator − Q_other_loss`. A radiator should reject heat according to actual fluid coupling, flow, radiator surface effectiveness, orientation/space sink proxy and current temperature. Pump electrical consumption and pressure/flow constraints must influence cooling. A failed/removed radiator has reduced or absent effective heat rejection, so station temperature changes gradually. A restored module with no flow or uncoupled quick connector cannot suddenly remove full heat load. Qualification needs sustained valid operation and measured thermal trend/reserve over a complete test window, not a single instant threshold reading.

# 34. COOLANT CONNECTOR AND LEAK-TEST INTERACTIONS

Represent two separately identifiable quick connectors/ports with connector insertion depth, alignment, sealing status, mechanical lock, valve isolation and leak test. Detect a loose seal, reversed connection or incomplete latch through actual modeled pressure decay/flow deviation rather than only a red graphic. `L86_CoolantQuickDisconnectA/B`, `L86_CoolantValveManifold`, `L86_RadiatorLeakTestController` and associated instrumentation must integrate with the station thermal model. Source filenames in Annex A are authoritative; do not rename game objects without explicit binding mapping.

# 35. INDEPENDENT SAFETY AND ARBITRATION

Create a separate, explicit safety arbiter/PLC state machine controlling robot-drive enable, brakes, contact permission, gripper release, tool change, fastening, radiator detach, coolant isolation and restoration. Monitor emergency stop, joint encoder integrity, speed/torque limits, real contact load, collision distance, vision validity, tool lock, payload support and independent contact relay; enforce a critical latched state on W129 loss. The `L86_RoboticContactSafetyPLC`, `L86_RobotTrajectorySafetySupervisor`, `L86_IndependentContactSafetyRelay`, `L86_HardwareInterlockDiagnosticRack`, `L86_ManipulatorEmergencyStopStation` and service PLC may issue requests but protections should not be bypassed by ordinary SCADA. Preserve safe holding controls when a gripped panel still needs support.

# 36. SEQUENCING PLC — CLEAR PHYSICAL STATE TRANSITIONS

Implement explicit phases such as `OFF`, `CONTROL_WIRING`, `SELF_TEST`, `ROBOT_HOMING`, `SERVO_CALIBRATION`, `VISION_CALIBRATION`, `FORCE_CALIBRATION`, `REACH_VERIFY`, `TOOL_READY`, `COOLANT_ISOLATION`, `RADIATOR_FASTENER_RELEASE`, `PAYLOAD_GRASP`, `FAILED_RADIATOR_EXTRACTION`, `DISPOSAL_CAPTURE`, `REPLACEMENT_RETRIEVAL`, `FLEX_SETTLE`, `ALIGNMENT`, `COMPLIANT_INSERTION`, `FASTENING`, `CONNECTOR_CHECK`, `LEAK_TEST`, `PUMP_START`, `THERMAL_STABILIZE`, `DISTURBANCE_RECOVERY`, `CONTACT_SAFE_HOLD`, `CRITICAL_FAULT_LATCHED`, `RESET_REQUIRED`, `SERVICE_CASSETTE_REQUALIFICATION`, `FINAL_CERTIFIED`.

State transitions must consume actual terminal continuity, measurements, physics and procedural evidence. Use explicit re-entry, abort, interrupted-contact and save/load semantics. The PLC must not create instant physical success by advancing a phase.

# 37. DISTURBANCE A — JOINT ENCODER DRIFT

Fault injector modifies one joint's **measured** encoder offset/stability while true mechanical angle follows physical drive. Result: estimator pose diverges from real tool pose; relative tool alignment, reach envelope and contact safety may degrade. Diagnose using independent reference, commanded/actual joint disagreement, laser/stereo metrology and torque measurements. Recovery requires motion inhibit if hazardous, genuine encoder reference procedure, fresh calibration and a successful accuracy trial. The player cannot clear this disturbance by changing an indicator field. Record pre/post errors and test events.

# 38. DISTURBANCE B — FLEXIBLE PAYLOAD VIBRATION

The fault injector excites an actual flexible radiator mode during gripped movement. Model displacement/strain proxy and added wrist load/torque; the motion planner should detect and reduce acceleration, change trajectory shaping and/or enable bounded damping. Require measured amplitude decay, maintained secure gripper contact, no station collision and a fresh removal/installation test. A vibration particle effect is not evidence; it must alter the plant and sensor state.

# 39. DISTURBANCE C — ASYMMETRIC GRIPPER CONTACT

The fault injector degrades one jaw's pad force/position/sensing or actual contact geometry. The grasp becomes off-centre and produces a nonzero moment that affects the wrist sensor and payload stability. Diagnose individual jaw pressure, grip/contact map, wrench direction and motion response. Recovery requires safely supporting the payload, repairing/exchanging the pad/sensor assembly, balanced new grip and proof of holding margin before moving. Resetting a warning without restoring physical contact quality is rejected.

# 40. FRESH POST-DISTURBANCE SERVICE CASSETTE TRIAL

After all three distinct disturbances have been activated, correctly diagnosed and repaired, execute a **new qualification run** on a repeatable service/training cassette. This must not destroy the already restored station radiator merely to re-run Objective 5 or Objective 6. The cassette provides representative panel grasp, tool change, fastener, connector, compliant insertion, leak/thermal-loop proxy tests tied to the same authoritative simulator and safety rules. Record independent acceptance evidence with `runId` after the last repair. No reuse of pre-fault samples or a previous Objective 4 installation result.

# 41. W129 PHYSICAL DISCONNECTION — FINAL TRUST-BOUNDARY FAULT

Only after historical Objectives 1–5 have certified, deliberately **physically disconnect W129** through the normal Project Spark player-wire tool. Verify the actual graph changed from connected → disconnected. Invalidate the source's trusted six-axis samples immediately or after a documented bounded sensor age timeout; do **not** turn a missing measurement into zero contact force or zero torque. Withdraw new contact-motion, insertion and fastening authority; enter controlled safe-hold with actual motor/brake/gripper support as allowed by present physical state; keep coolant system within independently safe conditions; latch a critical fault and log the exact event. If the physical load is in danger despite safety action, report a failed safe hold rather than inventing impossible brakes. Auxiliary W128/W130/W131/W152 data never substitutes for W129.

# 42. W129 REPAIR, RECALIBRATION, EXPLICIT RESET

Reconnect the **same real W129 wire** using player gameplay interaction. Validate live matching source/destination ports and compatible graph, sensor 5 V/returns, signal conditioner health (if applicable), sample clock, sensor range, six independent axes of measured wrench, bias and calibration fixture agreement. Verify independent overload protection, arm stationary/controlled, gripper/payload safe, coolant isolated or confirmed safe as relevant, no E-stop, no unresolved collision. Keep the latched fault until the player requests **explicit** reset through the W132 path; accept reset only if all prerequisites pass. Recommission operations gradually; do not teleport the arm or overwrite force values. Create a new run ID *after successful reset* and require fresh complete service-cassette manipulation + interface + thermal qualification before Objective 6.

# 43. SIX EXACT SOURCE OBJECTIVES — USE THESE TITLES

The source `Objective_Certification_Plan_6.csv` declares:

1. **Build the Orbital Robotics Control Network**.
2. **Commission and Calibrate the Seven-Axis Robot**.
3. **Extract the Failed Radiator**.
4. **Install and Restore the Thermal Radiator**.
5. **Recover Three Robotic Servicing Failures**.
6. **Restore Critical Contact-Force Feedback**.

Register them using the actual Project Spark objective/evaluator schema and event hooks. Store timestamps, source identifiers, topology hashes, equipment state snapshot/fingerprint, calibration versions and `runId` for each certification. Distinguish immutable historic progress from mutable live system health.

# 44. OBJECTIVE 1 — BUILD THE ORBITAL ROBOTICS CONTROL NETWORK

Grant **1/6** only after the player has made all **152 exact W001–W152** connections in the actual Spark solver graph, 230 art socket IDs are preserved, all 304 proposed endpoints are correctly reconciled to compatible real functional terminals, 5 V control and returns are valid, servo/vision/thermal/service/independent safety devices report powered healthy states, and actual W129 trusted feedback is connected and sampled validly. Reject prewired cables, duplicate shorted signals, missing returns, invalid port directions and fake visual-only socket checks. Capture a stable topology hash and electrical validation certificate.

# 45. OBJECTIVE 2 — COMMISSION AND CALIBRATE THE SEVEN-AXIS ROBOT

Grant **2/6** only after all seven joints and rail are genuinely homed, measured encoder offsets are qualified, actual motor/brake/torque states are within constraints, vision/depth stereo target calibration is successful, wrist six-axis wrench is zeroed/verified on the real simulated calibration fixture, tool TCP is calibrated, workspace/collision sensors are healthy and a complete collision-free trial path is executed using finite actual servo motion. Must distinguish feedback freshness and pose-estimation uncertainty from green visuals.

# 46. OBJECTIVE 3 — EXTRACT THE FAILED RADIATOR

Grant **3/6** only after true fictional coolant isolation/depressurization, safe thermal/electrical state, all radiator attachment fasteners released, actual gripper capture with sufficient holding margin, measured wrist force/torque and payload stress within limits, collision-free extraction and physical restraint in the disposal cradle. Save event order and completion proof; an extracted visual transform or scripted hatch animation is not sufficient.

# 47. OBJECTIVE 4 — INSTALL AND RESTORE THE THERMAL RADIATOR

Grant **4/6** only after real replacement pickup, vision-guided alignment, oscillation control, compliant insertion with bounded wrench, all required mechanical fasteners secured, coolant/electrical connectors engaged, pressure integrity verified by new test measurements, correct pump/valve/flow response and a sustained valid thermal operating period with Q_load/Q_radiator balance. Do not award if pump has no power, seal leaks, no heat is rejected, temperatures are fake or station thermal load was simply switched off.

# 48. OBJECTIVE 5 — RECOVER THREE ROBOTIC SERVICING FAILURES

Grant **5/6** only if encoder drift, flexible payload vibration and asymmetric gripper fault were **each actually induced into authoritative plant state**, diagnosed using independent telemetry, repaired via actual player interactions, and then a **new complete service-cassette manipulation/contact/fastening/leak/thermal qualification** passed. Verify no unresolved critical safety faults and document deterministic fault seeds and pre-/post-test evidence. A single generic RESET cannot qualify all three disturbances.

# 49. OBJECTIVE 6 — RESTORE CRITICAL CONTACT-FORCE FEEDBACK

Grant **6/6** only after historical Objectives 1–5, real W129 electrical topology break, trusted wrench invalidation, protected motion inhibition and latched fault; physical reconnection at **P045→P119** to correct split logical ports; true fresh six-axis samples and recalibration; independent contact/safety checks; **explicit reset through supported gameplay**; and a **brand-new full service-cassette qualification** with actual contact, fastening, coolant/leak/thermal evidence after repair. W129 reconnect or RESET alone must never complete 6/6. The real `LevelGamePlayManager` must reflect all six certificates and previous five history retained.

# 50. OBJECTIVE HISTORY, EVIDENCE AND SAVE/LOAD

Maintain `ObjectiveCertifiedAt`, `runId`, process-step event IDs, topology hash (including W129 continuity), sensor calibration version, measured contact/clearance margins, payload/thermal state snapshots and fault recovery milestones. Historic objectives are stored separately from current live readiness: during the deliberate W129 break, objectives 1–5 stay earned even though the machine is in a critical state. After repairs, Objective 6 requires `postRepairRunId > faultRunId` (or an equivalent strictly ordered event/run identity) and all constituent evidence newer than the accepted reset. Ensure save/load cannot duplicate certification, reset a critical latch invisibly or reuse old test data.

# 51. LIVE SCADA, VISUAL HOLOGRAMS AND TECHNICAL UI

Provide a comprehensive live SCADA display: all seven joint requested/actual angles, measured velocity/torque/current and brake status; rail commanded/actual, TCP position/orientation, singularity/clearance margin and collision map; stereo/depth calibration quality, confidence and sample age; six-axis force/torque vectors and uncertainty; W129 real graph continuity, trusted sample age, range and fault latch; gripper jaw pressure/contact distribution, tool identity/locking, individual fastener progress/torque; payload mass, flex modal amplitude/damping; panel mount/disposal/training-cassette state; coolant isolation, actual valve states, flow/pressure/leak rate, heat load/radiator rejection, station temperature trend; PLC mode, operator reset readiness, current and historic objectives 0/6→6/6. UI must clearly differentiate **command**, **actual**, **measured**, **valid**, and **historically certified**. SCADA may issue authorized requests, never inject a PASS or set temperatures/forces directly.

# 52. PLAYER CONTROLS AND REAL INTERACTIONS

Implement actual operable controls such as `MAIN 5V`, `VERIFY RETURNS`, `SAFETY SELF TEST`, `HOME RAIL`, `HOME J1..J7`, `ENCODER VERIFY`, `DRIVE ENABLE`, `BRAKE TEST`, `CALIBRATE STEREO`, `DEPTH SCAN`, `FIDUCIAL LOCK`, `CALIBRATE WRIST F/T`, `VERIFY W129`, `TOOL SELECT`, `CHANGE TOOL`, `OPEN/CLOSE JAWS`, `GRASP TEST`, `SET SAFE SPEED`, `PLAN PATH`, `JOG JOINT`, `MOVE TCP`, `STOP MOTION`, `ISOLATE COOLANT`, `VERIFY PRESSURE`, `RELEASE BOLTS`, `EXTRACT FAILED PANEL`, `STOW PANEL`, `FETCH REPLACEMENT`, `DAMP VIBRATION`, `ALIGN`, `COMPLIANT INSERT`, `FASTEN BOLTS`, `CONNECT PORT A/B`, `LEAK TEST`, `START PUMP`, `THERMAL TREND`, `INJECT ENCODER DRIFT`, `INJECT FLEX VIBRATION`, `INJECT JAW ASYMMETRY`, `DIAGNOSE/REPAIR`, `SERVICE CASSETTE RUN`, real player-wire `DISCONNECT/RECONNECT W129`, `RECALIBRATE`, `EXPLICIT RESET`, `FINAL QUALIFY`. Every command must interact with real modeled state and obey safety prerequisites; disable controls with actionable explanations when not permitted.

# 53. AAA VISUALS, PHYSICALLY GROUNDED VFX AND CAMERA PLAN

Visual identity: panoramic Earth and docked cargo spacecraft beyond a wide station service bay; a seven-joint white/gold industrial space arm; dark titanium and finely machined wrist fixtures; black finned failed/replacement radiators; cyan alignment vectors; amber hazardous-thermal status; red critical safety; transparent in-world holograms for joint envelope, force vector and temperature. Service cameras: orbital establishing view, close wrist contact camera, tool-changer closeup, station radiator exterior, coolant interface macro and SCADA. Animate actual joint and rail motion through the source pivot chain, valve angle, gripper jaw state, radiator extraction/replacement lift and fault lamps from authority. If needed, extend rig to animate other source pivots in `Animation_Rigs.csv`; never leave actual source pivots out of placement due to names guessed from the concept image. Keep all spectacular cinematics non-authoritative.

# 54. PROPOSED C# RUNTIME COMPONENTS — ADAPT TO REAL APIs

Create new Level 86 classes **only where the project has no equivalent**; names below are proposed rather than existing API claims:

`SparkLevel86Coordinator`, `SparkLevel86ProcessState`, `SparkLevel86ElectricalBindings`, `SparkLevel86TerminalReconciler`, `SparkLevel86RobotModel`, `SparkLevel86JointServoPlant`, `SparkLevel86RailDrive`, `SparkLevel86ForwardKinematics`, `SparkLevel86InverseKinematics`, `SparkLevel86TrajectoryPlanner`, `SparkLevel86CollisionWorld`, `SparkLevel86VisionSensorModel`, `SparkLevel86VisionPoseEstimator`, `SparkLevel86WristWrenchSensor`, `SparkLevel86CompliantContactModel`, `SparkLevel86GripperAndToolManager`, `SparkLevel86FastenerModel`, `SparkLevel86PayloadFlexModel`, `SparkLevel86PanelHandlingModel`, `SparkLevel86CoolantLoop`, `SparkLevel86RadiatorThermalModel`, `SparkLevel86IndependentContactSafety`, `SparkLevel86ServicingPLC`, `SparkLevel86DisturbanceController`, `SparkLevel86W129Recovery`, `SparkLevel86ObjectiveTracker`, `SparkLevel86TelemetryHistory`, `SparkLevel86SCADA`, `SparkLevel86VisualRigAdapter`, `SparkLevel86EditorBuilder`, `SparkLevel86AssetValidator`.

Use explicit interfaces for `IElectricalContinuity`, `IJointMeasurement`, `IToolWrenchMeasurement`, `ICoolantTelemetry`, `ISafetyPermissions`, `IObjectiveEvidence` where useful. Keep only one authoritative simulation owner; do not duplicate state into the artist rig or prefab inspector fields.

# 55. ONE-CLICK LEVEL 86 UNITY EDITOR BUILDER

Author the **new** menu `Tools/Project Spark/Levels/Create Level 86`. It must:

1. Inspect real Project Spark API signatures and find level-definition/prefab patterns.
2. Confirm all exact **82** FBXs/optionally GLBs, correct scene scale, identity-root placement and materials.
3. Create or safely update `Level_86.asset`, `Level_86_Layout.prefab`, namespaced Level 86 assets and six objective records.
4. Place all modular art under disposable `L86_HD_VISUAL_ART` without duplicating the showcase.
5. Validate all **64** unique pivot parents and construct the seven-link serial animation chain.
6. Preserve all **230** source art socket nodes and allocate **304** correctly typed functional logical contacts, resolving the **88** reported shared-anchor records.
7. Generate **152 W-wire declarations** but leave all puzzle wires unconnected; connect player wire placement/removal to real topology.
8. Create separate control/actuation/thermal/independent-safety simulated domains and explicit sensor/PLC adapters.
9. Bind tool changer, radiator panels, fastener interactions, gripper, coolant loops and physically meaningful colliders.
10. Implement robot servo/IK/collision/force/contact and station thermal process, with deterministic fault handling.
11. Implement all six evidence-based objectives, history, fresh-run and final W129 recovery logic.
12. Attach SCADA, visual rig, indicators and cameras to read-only state.
13. Emit collision/LOD, scaling, port conflict and runtime-test reports.
14. Save prefab and level assets idempotently with safe backups/Undo; don't erase customized gameplay objects or any prior level.

Do not claim existing art-placement menu fulfills this new gameplay builder. Rerunning this builder must not duplicate a circuit solver, pivot, `SparkTerminal`, objective or safety latch.

# 56. ACCEPTANCE TESTS — ASSETS, PIVOTS AND ELECTRICAL GRAPH

Perform these in the real project where available and report PASS/FAIL/NOT RUN with evidence, never inferred PASS:

1. Exactly 82 numbered FBX models imported and 82 GLBs available as alternatives.
2. Showcase asset not co-instantiated with modular source.
3. FBX centimetre→metre scaling occurs exactly once; scene bounds agree with source within import tolerances.
4. Exactly 64 unique `ANIM_*` parents, each correct owner/pivot coordinates/axis; no duplicates.
5. Serial seven-joint chain preserves link positions and proper parent-child following; rail follows measured displacement.
6. All 230 `SOCKET_P###` art transforms accounted for.
7. All 304 distinct logical contacts mapped, with 88 reused-anchor records reviewed.
8. No mixed signal kinds / invalid supply shorts / unsupported multi-node junctions.
9. Exactly 152 unique player-wire definition IDs; no preconnected puzzle wires.
10. Every wire can be created/removed by real player tools; solver detects actual continuity.
11. W129 is a real detachable `P045` sensor→`P119` safety PLC edge, not a display connection.
12. 5 V supplies, valid returns and independent safety authorization operate correctly.
13. Shop/shelf/throw UI and Levels 1–85 remain functional after import/builder rerun.
14. Optional reference textures map correctly if UV finishing performed; unmade UVs/LODs/colliders reported.

# 57. ACCEPTANCE TESTS — ROBOTICS, VISION AND CONTACT

15. All seven joints home separately and respect joint and drive limits.
16. Joint requested/actual pose differ appropriately during acceleration and settle.
17. Encoder drift produces real estimate error without teleporting true joint angles.
18. Motor current/torque saturation changes achieved motion and safety response.
19. Rail movement changes true base frame and robot reach consistently.
20. Forward kinematics reaches a known reference target within specified game tolerance.
21. IK rejects unreachable/unsafe targets and avoids collisions where physically feasible.
22. Scene obstacles and tool/payload boundaries prevent passage through station structure.
23. Stereo/depth calibration changes actual measured quality and sample confidence.
24. Occlusion/stale sensor readings cannot qualify precision insertion.
25. True six-axis wrench responds to contact geometry and payload inertia.
26. Wrist force sensor calibration has finite/traceable bias correction and timestamps.
27. Gripper jaw pressure/contact margin determines whether payload is securely held.
28. Tool changer and torque driver require real lock identity/position and finite torque progress.
29. Panel bolts must be released before extraction and secured before mount certificate.
30. Flexible radiator vibration excites and damps with actual motion inputs.
31. Payload cannot be released unsupported during W129 fault or emergency stop.

# 58. ACCEPTANCE TESTS — COOLANT, THERMAL, FAULTS AND W129

32. Coolant isolation and pressure response have independent actual measured states.
33. Failed radiator cannot be disconnected with live unqualified coolant interface.
34. Leak check detects intentionally unsealed connector and denies certification.
35. Pump/valve response changes real flow; UI command cannot create flow without energy/path.
36. Thermal energy balance changes station temperature gradually, not instant reset.
37. Replacement radiator improves actual heat rejection only when installed and circulating.
38. A permanent zero-heat-load shortcut does not satisfy required station mission utility.
39. All three injected disturbances materially affect the intended physical state.
40. Diagnoses and repairs produce fresh measurable evidence, not just cleared alarms.
41. Disturbance requalification uses a newly initiated service-cassette run.
42. Disconnect W129 through actual player tool and confirm solver topology mutation.
43. Trusted wrench becomes invalid/stale; true physical contact force is not overwritten.
44. W128 diagnostic, W130 continuity status, W131 fault indicator and W152 SCADA display cannot bypass W129.
45. Contact operations and risky motion are inhibited; existing payload support preserved where physically possible.
46. Independent collision/torque/E-stop protections still operate during W129 loss.
47. Critical fault latches and reset fails while W129 remains disconnected.
48. Reconnect W129; prove fresh six-axis calibrated samples and correct control references.
49. Explicit W132 reset accepted only after all safety prerequisites.
50. Reconnection or reset alone cannot complete Objective 6.
51. Objectives 1–5 remain historically certified throughout final fault/recovery.
52. New post-repair cassette trial proves actual handling, contact, fastening and coolant/thermal conditions.
53. Actual `LevelGamePlayManager` records **6/6** only after all required evidence.
54. Unity compilation/Play Mode, earlier-level regression and reproducibility results reported accurately.

# 59. COMPLETE PLAYER GAMEPLAY WALKTHROUGH — MUST BE REAL

**CONNECT ALL 152 PLAYER WIRES → RECONCILE 230 ART SOCKETS WITH 304 INDEPENDENT TERMINAL CONTACTS → VERIFY 5 V SUPPLY/RETURNS AND INDEPENDENT SAFETY → HOME RAIL AND SEVEN JOINTS → VALIDATE BRAKES/CURRENT/TORQUE → CALIBRATE STEREO AND DEPTH VISION → CALIBRATE SIX-AXIS WRIST FORCE/TORQUE AND TRUSTED W129 → LEARN SAFE REACH ENVELOPE → CHANGE TO ROBOTIC GRIPPER/DRIVER → ISOLATE AND VERIFY COOLANT SAFE → SCAN FAILED RADIATOR → RELEASE EACH ATTACHMENT FASTENER → GRASP THE FAILED PANEL → REMOVE USING BOUNDED COMPLIANT MOTION → PLACE IN RESTRAINED DISPOSAL CRADLE → OPEN CARGO HATCH → FETCH REPLACEMENT RADIATOR → PLAN COLLISION-FREE ROBOT TRAJECTORY → DAMP FLEX OSCILLATION → MEASURE FIDUCIAL ALIGNMENT → PERFORM COMPLIANT INSERTION → FASTEN ALL MOUNTS → ENGAGE COOLANT CONNECTORS → RUN LEAK TEST → START PUMP AND VERIFY FLOW → OBSERVE REAL STATION COOLING TREND → CERTIFY INITIAL REPAIR → INJECT ENCODER DRIFT AND RECALIBRATE → INJECT PAYLOAD VIBRATION AND SHAPE/DAMP MOTION → INJECT GRIPPER ASYMMETRY AND REPAIR CONTACT → RUN FRESH SERVICE-CASSETTE QUALIFICATION → PHYSICALLY DISCONNECT W129 → DETECT STALE CONTACT WRENCH → SAFELY INHIBIT CONTACT MOTION, PRESERVE HOLD, LATCH FAULT → RECONNECT REAL W129 → ACQUIRE NEW SIX-AXIS SAMPLES AND RECALIBRATE → VERIFY INDEPENDENT PROTECTION → EXPLICITLY RESET SAFETY → RUN BRAND-NEW COMPLETE CASSETTE MANIPULATION + FASTENING + LEAK/THERMAL TEST → ACTUAL `LevelGamePlayManager` REACHES 6/6.**

# 60. DIAGNOSTICS, FAILURE MESSAGES AND VALIDATION REPORT

Report actionable faults: missing FBX/GLB, incorrect import scaling, duplicate pivot, broken serial chain, bad joint axis, missing socket, unsupported terminal direction/type, shorted analog-power node, duplicate wire ID, unpowered controller, absent 5 V return, stale stereo/encoder/wrench measurements, W129 open, W128/W130 attempted bypass, gripper unsupported, overload/torque limit, joint singularity, actual-vs-command deviation, collision risk, uncalibrated tool TCP, unstable payload, wrong fastener, loose mounting bolt, coolant valve commanded-but-not-actual, insufficient pressure relief, pump stalled, leak check failed, hot station or inadequate thermal trend, disturbance still active, rejected explicit reset, fake completion, stale pre-fault qualification, objectives stuck at 0/6 or 5/6, broken save/load and previous-level regression. Emit **level import report**, **port reconciliation CSV**, **wiring validation report**, **process simulation test evidence**, **objective run history** and **Unity compilation/Play Mode test status**. Never manufacture passing results.

# 61. DELIVERABLES

Deliver real Project Spark-compatible Unity source components, a Level 86 scene/prefab and `Level_86.asset`, one-click gameplay builder, full 82-FBX modular binding, 64 visual pivots with correct serial robot chain, all 230 source decorative anchors plus 304 reconciled logical functional ports, 152 actual player-created wire definitions/validation, robot joint servo and kinematic solver, vision calibration/pose estimation, six-axis wrench/contact, gripper/tool changer, payload flex, fasteners, finite thermal/coolant plant, contact safety PLC, three genuine faults, W129 disconnect/repair/reset logic, six authoritative objectives, SCADA and controlled cinematic VFX, repeatable tests and honest reports. Provide importable files, not pseudocode-only claims. If access to the Unity project is not available, deliver working source modules and mark compilation/runtime testing **NOT RUN**, without claiming a playable integrated scene.

# 62. FINAL SUCCESS CONTRACT

**Only mark “LEVEL 86 COMPLETE” when an actual Unity project import and Play Mode mission demonstrates genuine wiring, seven-joint calibration, safe removal of the failed radiator, properly fastened replacement, sustained cooling, three physical disturbance recoveries, real W129 trust-boundary failure, physical reconnection, fresh six-axis calibration, explicit safety reset and a brand-new service-cassette test, with actual `LevelGamePlayManager` displaying 6/6.** Levels 1–85 remain unaffected.

> **THE STATION IS OVERHEATING. THE ROBOT IS YOUR HAND. CONTACT IS YOUR TRUTH. REPAIR THE RADIATOR. RESTORE THE ORBITAL FORGE.**

---

# SOURCE-DERIVED ANNEXES — EXACT PACKAGE RECORDS

The following annexes are generated from the supplied Level 86 CSV/JSON files and are **design/art data, not pre-existing Unity runtime electrical or robotics implementations**. Keep these source spellings/IDs authoritative unless the import reveals a package discrepancy.


## ANNEX A — ALL 82 EXACT MODULAR FBX AND GLB FILENAMES

Source: `Model_Manifest.json`. Counts and names below are exact; triangles refer to the modular triangulated geometry.

| # | Model name | Exact FBX filename | Exact GLB filename | Triangles | Mesh parts |
|---|---|---|---|---|---|
| 00 | `L86_OrbitalForgeArenaBase` | `00_L86_OrbitalForgeArenaBase.fbx` | `00_L86_OrbitalForgeArenaBase.glb` | 492 | 6 |
| 01 | `L86_PanoramicEarthMaintenanceViewport` | `01_L86_PanoramicEarthMaintenanceViewport.fbx` | `01_L86_PanoramicEarthMaintenanceViewport.glb` | 5120 | 15 |
| 02 | `L86_StationExteriorServiceTruss` | `02_L86_StationExteriorServiceTruss.fbx` | `02_L86_StationExteriorServiceTruss.glb` | 402 | 4 |
| 03 | `L86_StationRobotServiceBayFrame` | `03_L86_StationRobotServiceBayFrame.fbx` | `03_L86_StationRobotServiceBayFrame.glb` | 402 | 4 |
| 04 | `L86_DockedCargoSpacecraft` | `04_L86_DockedCargoSpacecraft.fbx` | `04_L86_DockedCargoSpacecraft.glb` | 7968 | 4 |
| 05 | `L86_CargoServiceBayHatch` | `05_L86_CargoServiceBayHatch.fbx` | `05_L86_CargoServiceBayHatch.glb` | 9260 | 3 |
| 06 | `L86_RobotLinearTraverseRail` | `06_L86_RobotLinearTraverseRail.fbx` | `06_L86_RobotLinearTraverseRail.glb` | 792 | 7 |
| 07 | `L86_SevenAxisRobotBase` | `07_L86_SevenAxisRobotBase.fbx` | `07_L86_SevenAxisRobotBase.glb` | 1068 | 2 |
| 08 | `L86_RobotJoint1BaseYaw` | `08_L86_RobotJoint1BaseYaw.fbx` | `08_L86_RobotJoint1BaseYaw.glb` | 2162 | 10 |
| 09 | `L86_RobotJoint2Shoulder` | `09_L86_RobotJoint2Shoulder.fbx` | `09_L86_RobotJoint2Shoulder.glb` | 2162 | 10 |
| 10 | `L86_RobotJoint3UpperArm` | `10_L86_RobotJoint3UpperArm.fbx` | `10_L86_RobotJoint3UpperArm.glb` | 2162 | 10 |
| 11 | `L86_RobotJoint4Elbow` | `11_L86_RobotJoint4Elbow.fbx` | `11_L86_RobotJoint4Elbow.glb` | 2162 | 10 |
| 12 | `L86_RobotJoint5Forearm` | `12_L86_RobotJoint5Forearm.fbx` | `12_L86_RobotJoint5Forearm.glb` | 2162 | 10 |
| 13 | `L86_RobotJoint6WristPitch` | `13_L86_RobotJoint6WristPitch.fbx` | `13_L86_RobotJoint6WristPitch.glb` | 2162 | 10 |
| 14 | `L86_RobotJoint7WristRoll` | `14_L86_RobotJoint7WristRoll.fbx` | `14_L86_RobotJoint7WristRoll.glb` | 1964 | 10 |
| 15 | `L86_RobotCounterbalanceModule` | `15_L86_RobotCounterbalanceModule.fbx` | `15_L86_RobotCounterbalanceModule.glb` | 276 | 2 |
| 16 | `L86_JointServoDriveRack` | `16_L86_JointServoDriveRack.fbx` | `16_L86_JointServoDriveRack.glb` | 1212 | 13 |
| 17 | `L86_JointEncoderFeedbackArray` | `17_L86_JointEncoderFeedbackArray.fbx` | `17_L86_JointEncoderFeedbackArray.glb` | 1980 | 29 |
| 18 | `L86_JointTorqueCurrentSensorRack` | `18_L86_JointTorqueCurrentSensorRack.fbx` | `18_L86_JointTorqueCurrentSensorRack.glb` | 1692 | 23 |
| 19 | `L86_RobotBrakeAndTorqueLimiter` | `19_L86_RobotBrakeAndTorqueLimiter.fbx` | `19_L86_RobotBrakeAndTorqueLimiter.glb` | 1308 | 15 |
| 20 | `L86_SevenAxisReachEnvelopeHologram` | `20_L86_SevenAxisReachEnvelopeHologram.fbx` | `20_L86_SevenAxisReachEnvelopeHologram.glb` | 3822 | 5 |
| 21 | `L86_RoboticHomingCalibrationFixture` | `21_L86_RoboticHomingCalibrationFixture.fbx` | `21_L86_RoboticHomingCalibrationFixture.glb` | 460 | 4 |
| 22 | `L86_StereoMachineVisionCameraLeft` | `22_L86_StereoMachineVisionCameraLeft.fbx` | `22_L86_StereoMachineVisionCameraLeft.glb` | 1068 | 8 |
| 23 | `L86_StereoMachineVisionCameraRight` | `23_L86_StereoMachineVisionCameraRight.fbx` | `23_L86_StereoMachineVisionCameraRight.glb` | 1068 | 8 |
| 24 | `L86_StructuredLightDepthScanner` | `24_L86_StructuredLightDepthScanner.fbx` | `24_L86_StructuredLightDepthScanner.glb` | 1068 | 9 |
| 25 | `L86_OpticalFiducialTargetArray` | `25_L86_OpticalFiducialTargetArray.fbx` | `25_L86_OpticalFiducialTargetArray.glb` | 1032 | 9 |
| 26 | `L86_PrecisionVisionCalibrationBoard` | `26_L86_PrecisionVisionCalibrationBoard.fbx` | `26_L86_PrecisionVisionCalibrationBoard.glb` | 1032 | 9 |
| 27 | `L86_LaserAlignmentMetrologyUnit` | `27_L86_LaserAlignmentMetrologyUnit.fbx` | `27_L86_LaserAlignmentMetrologyUnit.glb` | 1068 | 9 |
| 28 | `L86_SixAxisWristForceTorqueSensor` | `28_L86_SixAxisWristForceTorqueSensor.fbx` | `28_L86_SixAxisWristForceTorqueSensor.glb` | 1600 | 9 |
| 29 | `L86_ForceTorqueCalibrationFixture` | `29_L86_ForceTorqueCalibrationFixture.fbx` | `29_L86_ForceTorqueCalibrationFixture.glb` | 636 | 4 |
| 30 | `L86_CompliantInsertionController` | `30_L86_CompliantInsertionController.fbx` | `30_L86_CompliantInsertionController.glb` | 636 | 10 |
| 31 | `L86_SixAxisContactForceHologram` | `31_L86_SixAxisContactForceHologram.fbx` | `31_L86_SixAxisContactForceHologram.glb` | 3822 | 5 |
| 32 | `L86_RobotJointPathPlanningComputer` | `32_L86_RobotJointPathPlanningComputer.fbx` | `32_L86_RobotJointPathPlanningComputer.glb` | 1164 | 19 |
| 33 | `L86_InverseKinematicsSolverDisplay` | `33_L86_InverseKinematicsSolverDisplay.fbx` | `33_L86_InverseKinematicsSolverDisplay.glb` | 876 | 13 |
| 34 | `L86_CollisionProximityScanner` | `34_L86_CollisionProximityScanner.fbx` | `34_L86_CollisionProximityScanner.glb` | 1068 | 9 |
| 35 | `L86_RobotTrajectorySafetySupervisor` | `35_L86_RobotTrajectorySafetySupervisor.fbx` | `35_L86_RobotTrajectorySafetySupervisor.glb` | 1068 | 17 |
| 36 | `L86_FlexiblePayloadModalSensor` | `36_L86_FlexiblePayloadModalSensor.fbx` | `36_L86_FlexiblePayloadModalSensor.glb` | 692 | 9 |
| 37 | `L86_ActiveVibrationDampingController` | `37_L86_ActiveVibrationDampingController.fbx` | `37_L86_ActiveVibrationDampingController.glb` | 788 | 11 |
| 38 | `L86_AdaptiveRadiatorHandlingGripper` | `38_L86_AdaptiveRadiatorHandlingGripper.fbx` | `38_L86_AdaptiveRadiatorHandlingGripper.glb` | 604 | 16 |
| 39 | `L86_AutomaticEndEffectorToolChanger` | `39_L86_AutomaticEndEffectorToolChanger.fbx` | `39_L86_AutomaticEndEffectorToolChanger.glb` | 448 | 6 |
| 40 | `L86_PrecisionTorqueFasteningDriver` | `40_L86_PrecisionTorqueFasteningDriver.fbx` | `40_L86_PrecisionTorqueFasteningDriver.glb` | 1168 | 9 |
| 41 | `L86_CaptiveFastenerAndBoltRack` | `41_L86_CaptiveFastenerAndBoltRack.fbx` | `41_L86_CaptiveFastenerAndBoltRack.glb` | 908 | 5 |
| 42 | `L86_VacuumCompatibleInspectionProbe` | `42_L86_VacuumCompatibleInspectionProbe.fbx` | `42_L86_VacuumCompatibleInspectionProbe.glb` | 600 | 6 |
| 43 | `L86_RoboticToolDockAndCalibrationNest` | `43_L86_RoboticToolDockAndCalibrationNest.fbx` | `43_L86_RoboticToolDockAndCalibrationNest.glb` | 668 | 5 |
| 44 | `L86_DualJawGripperPressureSensorArray` | `44_L86_DualJawGripperPressureSensorArray.fbx` | `44_L86_DualJawGripperPressureSensorArray.glb` | 588 | 11 |
| 45 | `L86_RadiatorGrappleHandleFixture` | `45_L86_RadiatorGrappleHandleFixture.fbx` | `45_L86_RadiatorGrappleHandleFixture.glb` | 684 | 4 |
| 46 | `L86_FailedThermalRadiatorModule` | `46_L86_FailedThermalRadiatorModule.fbx` | `46_L86_FailedThermalRadiatorModule.glb` | 1162 | 9 |
| 47 | `L86_ReplacementThermalRadiatorModule` | `47_L86_ReplacementThermalRadiatorModule.fbx` | `47_L86_ReplacementThermalRadiatorModule.glb` | 1162 | 8 |
| 48 | `L86_RadiatorMountingSpaceTruss` | `48_L86_RadiatorMountingSpaceTruss.fbx` | `48_L86_RadiatorMountingSpaceTruss.glb` | 246 | 4 |
| 49 | `L86_RadiatorThermalInterfacePlate` | `49_L86_RadiatorThermalInterfacePlate.fbx` | `49_L86_RadiatorThermalInterfacePlate.glb` | 84 | 2 |
| 50 | `L86_CoolantQuickDisconnectA` | `50_L86_CoolantQuickDisconnectA.fbx` | `50_L86_CoolantQuickDisconnectA.glb` | 1340 | 5 |
| 51 | `L86_CoolantQuickDisconnectB` | `51_L86_CoolantQuickDisconnectB.fbx` | `51_L86_CoolantQuickDisconnectB.glb` | 1340 | 5 |
| 52 | `L86_CoolantValveManifold` | `52_L86_CoolantValveManifold.fbx` | `52_L86_CoolantValveManifold.glb` | 630 | 8 |
| 53 | `L86_CoolantPressureTransducer` | `53_L86_CoolantPressureTransducer.fbx` | `53_L86_CoolantPressureTransducer.glb` | 792 | 12 |
| 54 | `L86_CoolantFlowMeter` | `54_L86_CoolantFlowMeter.fbx` | `54_L86_CoolantFlowMeter.glb` | 792 | 12 |
| 55 | `L86_RadiatorTemperatureSensorArray` | `55_L86_RadiatorTemperatureSensorArray.fbx` | `55_L86_RadiatorTemperatureSensorArray.glb` | 792 | 12 |
| 56 | `L86_StationAvionicsHeatLoadEmulator` | `56_L86_StationAvionicsHeatLoadEmulator.fbx` | `56_L86_StationAvionicsHeatLoadEmulator.glb` | 792 | 12 |
| 57 | `L86_ThermalLoopPumpSimulator` | `57_L86_ThermalLoopPumpSimulator.fbx` | `57_L86_ThermalLoopPumpSimulator.glb` | 630 | 8 |
| 58 | `L86_RadiatorLeakTestController` | `58_L86_RadiatorLeakTestController.fbx` | `58_L86_RadiatorLeakTestController.glb` | 1080 | 18 |
| 59 | `L86_CoolantIsolationAndVentStation` | `59_L86_CoolantIsolationAndVentStation.fbx` | `59_L86_CoolantIsolationAndVentStation.glb` | 630 | 8 |
| 60 | `L86_RadiatorFastenerActuatorDriver` | `60_L86_RadiatorFastenerActuatorDriver.fbx` | `60_L86_RadiatorFastenerActuatorDriver.glb` | 792 | 12 |
| 61 | `L86_StationThermalTrendHologram` | `61_L86_StationThermalTrendHologram.fbx` | `61_L86_StationThermalTrendHologram.glb` | 3822 | 5 |
| 62 | `L86_RadiatorThermalPerformanceAnalyzer` | `62_L86_RadiatorThermalPerformanceAnalyzer.fbx` | `62_L86_RadiatorThermalPerformanceAnalyzer.glb` | 1068 | 17 |
| 63 | `L86_FailedRadiatorDisposalCradle` | `63_L86_FailedRadiatorDisposalCradle.fbx` | `63_L86_FailedRadiatorDisposalCradle.glb` | 204 | 5 |
| 64 | `L86_PayloadVibrationFaultInjector` | `64_L86_PayloadVibrationFaultInjector.fbx` | `64_L86_PayloadVibrationFaultInjector.glb` | 816 | 12 |
| 65 | `L86_JointEncoderDriftFaultInjector` | `65_L86_JointEncoderDriftFaultInjector.fbx` | `65_L86_JointEncoderDriftFaultInjector.glb` | 816 | 12 |
| 66 | `L86_AsymmetricGripperFaultInjector` | `66_L86_AsymmetricGripperFaultInjector.fbx` | `66_L86_AsymmetricGripperFaultInjector.glb` | 816 | 12 |
| 67 | `L86_RoboticContactSafetyPLC` | `67_L86_RoboticContactSafetyPLC.fbx` | `67_L86_RoboticContactSafetyPLC.glb` | 2820 | 51 |
| 68 | `L86_ManipulatorEmergencyStopStation` | `68_L86_ManipulatorEmergencyStopStation.fbx` | `68_L86_ManipulatorEmergencyStopStation.glb` | 900 | 12 |
| 69 | `L86_Protected5VControlSupply` | `69_L86_Protected5VControlSupply.fbx` | `69_L86_Protected5VControlSupply.glb` | 1644 | 28 |
| 70 | `L86_RobotMotorPowerDistributionUnit` | `70_L86_RobotMotorPowerDistributionUnit.fbx` | `70_L86_RobotMotorPowerDistributionUnit.glb` | 876 | 12 |
| 71 | `L86_IndependentContactSafetyRelay` | `71_L86_IndependentContactSafetyRelay.fbx` | `71_L86_IndependentContactSafetyRelay.glb` | 1188 | 17 |
| 72 | `L86_SevenAxisJointServoController` | `72_L86_SevenAxisJointServoController.fbx` | `72_L86_SevenAxisJointServoController.glb` | 2604 | 47 |
| 73 | `L86_ContactForceSignalConditioner` | `73_L86_ContactForceSignalConditioner.fbx` | `73_L86_ContactForceSignalConditioner.glb` | 1452 | 23 |
| 74 | `L86_StereoVisionFusionComputer` | `74_L86_StereoVisionFusionComputer.fbx` | `74_L86_StereoVisionFusionComputer.glb` | 1944 | 34 |
| 75 | `L86_RadiatorServiceSequencingPLC` | `75_L86_RadiatorServiceSequencingPLC.fbx` | `75_L86_RadiatorServiceSequencingPLC.glb` | 2808 | 53 |
| 76 | `L86_HolographicOrbitalRoboticsSCADA` | `76_L86_HolographicOrbitalRoboticsSCADA.fbx` | `76_L86_HolographicOrbitalRoboticsSCADA.glb` | 5160 | 36 |
| 77 | `L86_W129CriticalFeedbackPatchStation` | `77_L86_W129CriticalFeedbackPatchStation.fbx` | `77_L86_W129CriticalFeedbackPatchStation.glb` | 1320 | 17 |
| 78 | `L86_HardwareInterlockDiagnosticRack` | `78_L86_HardwareInterlockDiagnosticRack.fbx` | `78_L86_HardwareInterlockDiagnosticRack.glb` | 1068 | 16 |
| 79 | `L86_ServicingMissionCertificationBeacon` | `79_L86_ServicingMissionCertificationBeacon.fbx` | `79_L86_ServicingMissionCertificationBeacon.glb` | 1360 | 7 |
| 80 | `L86_ExternalInspectionCameraGantry` | `80_L86_ExternalInspectionCameraGantry.fbx` | `80_L86_ExternalInspectionCameraGantry.glb` | 912 | 5 |
| 81 | `L86_RoboticServicingEventRecorder` | `81_L86_RoboticServicingEventRecorder.fbx` | `81_L86_RoboticServicingEventRecorder.glb` | 780 | 11 |

Separate assembled previews: `Models/Level86_Complete_ArtKit_Showcase.fbx` and `GLB/Level86_Complete_ArtKit_Showcase.glb`; do not combine them with the modular set.



## ANNEX B — ALL 64 VERIFIED `ANIM_*` PIVOT PARENTS

Source: `Animation_Rigs.csv`. World XYZ values are in **metres**, and `Kind=indicator` means a visual indicator anchor, not a mechanical joint.

| # | Exact pivot parent | Owner model | Kind | Axis | X m | Y m | Z m | Group |
|---|---|---|---|---|---|---|---|---|
| 1 | `ANIM_CARGO_HATCH` | `L86_CargoServiceBayHatch` | rotation | y | -7.4 | 5.0 | -3.0 | Mechanical and instrument visual |
| 2 | `ANIM_LINEAR_RAIL_CARRIAGE` | `L86_RobotLinearTraverseRail` | translation | x | -4.55 | 1.17 | -1.52 | Mechanical and instrument visual |
| 3 | `ANIM_ROBOT_BASE_ROTATE` | `L86_RobotJoint1BaseYaw` | rotation | y | -4.55 | 2.52 | -1.52 | Robot articulation |
| 4 | `ANIM_ROBOT_SHOULDER_PITCH` | `L86_RobotJoint2Shoulder` | rotation | z | -4.15 | 3.03 | -1.52 | Robot articulation |
| 5 | `ANIM_ROBOT_UPPERARM_PITCH` | `L86_RobotJoint3UpperArm` | rotation | z | -3.3 | 3.67 | -1.52 | Robot articulation |
| 6 | `ANIM_ROBOT_ELBOW_PITCH` | `L86_RobotJoint4Elbow` | rotation | z | -2.35 | 4.22 | -1.52 | Robot articulation |
| 7 | `ANIM_ROBOT_FOREARM_ROLL` | `L86_RobotJoint5Forearm` | rotation | x | -1.3 | 4.74 | -1.52 | Robot articulation |
| 8 | `ANIM_ROBOT_WRIST_PITCH` | `L86_RobotJoint6WristPitch` | rotation | z | -0.55 | 5.15 | -1.52 | Robot articulation |
| 9 | `ANIM_ROBOT_WRIST_ROLL` | `L86_RobotJoint7WristRoll` | rotation | x | 0.25 | 5.42 | -1.52 | Robot articulation |
| 10 | `ANIM_COUNTERBALANCE_SLIDER` | `L86_RobotCounterbalanceModule` | translation | x | -4.96 | 3.27 | -1.98 | Mechanical and instrument visual |
| 11 | `ANIM_SERVO_DRIVE_BANK` | `L86_JointServoDriveRack` | indicator | none | -12.35 | 1.71 | 5.55 | Mechanical and instrument visual |
| 12 | `ANIM_ENCODER_SCAN` | `L86_JointEncoderFeedbackArray` | indicator | none | -11.1 | 1.71 | 5.55 | Mechanical and instrument visual |
| 13 | `ANIM_MOTOR_TORQUE_GAUGE` | `L86_JointTorqueCurrentSensorRack` | indicator | none | -9.85 | 1.71 | 5.55 | Mechanical and instrument visual |
| 14 | `ANIM_JOINT_BRAKE_ACTUATOR` | `L86_RobotBrakeAndTorqueLimiter` | translation | x | -8.6 | 1.71 | 5.55 | Mechanical and instrument visual |
| 15 | `ANIM_REACH_ENVELOPE` | `L86_SevenAxisReachEnvelopeHologram` | indicator | none | -3.1 | 7.65 | -5.9 | Mechanical and instrument visual |
| 16 | `ANIM_HOMING_PROBE` | `L86_RoboticHomingCalibrationFixture` | translation | z | -5.95 | 1.7 | 4.25 | Mechanical and instrument visual |
| 17 | `ANIM_STEREO_LEFT_PAN` | `L86_StereoMachineVisionCameraLeft` | rotation | y | -1.2 | 6.3 | -1.1 | Mechanical and instrument visual |
| 18 | `ANIM_STEREO_RIGHT_PAN` | `L86_StereoMachineVisionCameraRight` | rotation | y | -0.45 | 6.3 | -1.1 | Mechanical and instrument visual |
| 19 | `ANIM_DEPTH_SCANNER` | `L86_StructuredLightDepthScanner` | rotation | y | 1.05 | 6.25 | -1.7 | Mechanical and instrument visual |
| 20 | `ANIM_ALIGNMENT_LASER` | `L86_LaserAlignmentMetrologyUnit` | rotation | y | 2.4 | 6.5 | -2.75 | Mechanical and instrument visual |
| 21 | `ANIM_WRIST_FORCE_SENSOR` | `L86_SixAxisWristForceTorqueSensor` | indicator | none | 0.9 | 5.3 | -1.52 | Mechanical and instrument visual |
| 22 | `ANIM_COMPLIANT_INSERTION` | `L86_CompliantInsertionController` | translation | x | -0.7 | 1.7 | 5.45 | Mechanical and instrument visual |
| 23 | `ANIM_CONTACT_WRENCH_VECTOR` | `L86_SixAxisContactForceHologram` | indicator | none | 3.4 | 7.32 | -5.95 | Mechanical and instrument visual |
| 24 | `ANIM_COLLISION_SENSOR_SWEEP` | `L86_CollisionProximityScanner` | rotation | y | 3.06 | 1.7 | 5.45 | Mechanical and instrument visual |
| 25 | `ANIM_FLEX_MODAL_RESPONSE` | `L86_FlexiblePayloadModalSensor` | indicator | none | 1.15 | 5.55 | -1.65 | Mechanical and instrument visual |
| 26 | `ANIM_ACTIVE_DAMPING` | `L86_ActiveVibrationDampingController` | indicator | none | 5.56 | 1.7 | 5.45 | Mechanical and instrument visual |
| 27 | `ANIM_GRIPPER_JAW_LEFT` | `L86_AdaptiveRadiatorHandlingGripper` | translation | y | 1.46 | 5.22 | -1.18 | Mechanical and instrument visual |
| 28 | `ANIM_GRIPPER_JAW_RIGHT` | `L86_AdaptiveRadiatorHandlingGripper` | translation | y | 1.46 | 5.22 | -1.86 | Mechanical and instrument visual |
| 29 | `ANIM_TOOL_CHANGER_ROTARY` | `L86_AutomaticEndEffectorToolChanger` | rotation | x | 0.85 | 5.18 | -1.52 | Mechanical and instrument visual |
| 30 | `ANIM_FASTENING_DRIVER_SPINDLE` | `L86_PrecisionTorqueFasteningDriver` | rotation | x | 2.32 | 5.1 | -1.52 | Mechanical and instrument visual |
| 31 | `ANIM_FASTENER_CASSETTE` | `L86_CaptiveFastenerAndBoltRack` | translation | x | -5.2 | 1.45 | 5.35 | Mechanical and instrument visual |
| 32 | `ANIM_INSPECTION_PROBE` | `L86_VacuumCompatibleInspectionProbe` | translation | x | 0.93 | 5.56 | -1.45 | Mechanical and instrument visual |
| 33 | `ANIM_TOOL_DOCK_INDEXER` | `L86_RoboticToolDockAndCalibrationNest` | rotation | y | -5.65 | 1.68 | 3.82 | Mechanical and instrument visual |
| 34 | `ANIM_GRIP_CONTACT_SCAN` | `L86_DualJawGripperPressureSensorArray` | indicator | none | 1.6 | 5.1 | -1.5 | Mechanical and instrument visual |
| 35 | `ANIM_PAYLOAD_GRAPPLE_LOCK` | `L86_RadiatorGrappleHandleFixture` | rotation | x | 6.8 | 5.5 | -2.17 | Mechanical and instrument visual |
| 36 | `ANIM_FAILED_RADIATOR_RELEASE` | `L86_FailedThermalRadiatorModule` | translation | x | 8.15 | 5.2 | -2.35 | Mechanical and instrument visual |
| 37 | `ANIM_REPLACEMENT_RADIATOR_LIFT` | `L86_ReplacementThermalRadiatorModule` | translation | y | 8.25 | 1.39 | 4.05 | Mechanical and instrument visual |
| 38 | `ANIM_RADIATOR_TRUSS_JOINT` | `L86_RadiatorMountingSpaceTruss` | indicator | none | 9.25 | 5.35 | -2.4 | Mechanical and instrument visual |
| 39 | `ANIM_THERMAL_PLATE_INTERFACE` | `L86_RadiatorThermalInterfacePlate` | translation | x | 8.37 | 5.25 | -2.25 | Mechanical and instrument visual |
| 40 | `ANIM_COOLANT_CONNECTOR_A` | `L86_CoolantQuickDisconnectA` | translation | x | 7.0 | 4.6 | -1.85 | Mechanical and instrument visual |
| 41 | `ANIM_COOLANT_CONNECTOR_B` | `L86_CoolantQuickDisconnectB` | translation | x | 9.4 | 4.6 | -1.85 | Mechanical and instrument visual |
| 42 | `ANIM_COOLANT_VALVE` | `L86_CoolantValveManifold` | rotation | x | 9.15 | 1.64 | 5.45 | Mechanical and instrument visual |
| 43 | `ANIM_COOLANT_PRESSURE_INDICATOR` | `L86_CoolantPressureTransducer` | indicator | none | 10.35 | 1.64 | 5.45 | Mechanical and instrument visual |
| 44 | `ANIM_COOLANT_FLOW_INDICATOR` | `L86_CoolantFlowMeter` | indicator | none | 11.55 | 1.64 | 5.45 | Mechanical and instrument visual |
| 45 | `ANIM_RADIATOR_SENSOR_GRID` | `L86_RadiatorTemperatureSensorArray` | indicator | none | 9.9 | 5.2 | -2.8 | Mechanical and instrument visual |
| 46 | `ANIM_AVIONICS_HEAT_LOAD` | `L86_StationAvionicsHeatLoadEmulator` | indicator | none | -2.7 | 6.2 | -3.35 | Mechanical and instrument visual |
| 47 | `ANIM_PUMP_ROTOR` | `L86_ThermalLoopPumpSimulator` | rotation | x | 7.05 | 1.67 | 5.5 | Mechanical and instrument visual |
| 48 | `ANIM_LEAK_TEST_DIAGNOSTIC` | `L86_RadiatorLeakTestController` | indicator | none | 8.4 | 1.66 | 5.52 | Mechanical and instrument visual |
| 49 | `ANIM_COOLANT_ISOLATION_VALVE` | `L86_CoolantIsolationAndVentStation` | rotation | x | 10.1 | 3.3 | 5.16 | Mechanical and instrument visual |
| 50 | `ANIM_FASTENER_MOTOR_BANK` | `L86_RadiatorFastenerActuatorDriver` | rotation | x | 7.14 | 5.65 | -2.7 | Mechanical and instrument visual |
| 51 | `ANIM_THERMAL_TREND` | `L86_StationThermalTrendHologram` | indicator | none | 0.0 | 8.65 | -6.1 | Mechanical and instrument visual |
| 52 | `ANIM_DISPOSAL_CRADLE_LATCH` | `L86_FailedRadiatorDisposalCradle` | rotation | y | 9.25 | 1.1 | 3.2 | Mechanical and instrument visual |
| 53 | `ANIM_PAYLOAD_OSCILLATION_FAULT` | `L86_PayloadVibrationFaultInjector` | translation | y | 6.75 | 3.35 | 5.65 | Mechanical and instrument visual |
| 54 | `ANIM_ENCODER_DRIFT_FAULT` | `L86_JointEncoderDriftFaultInjector` | rotation | z | -7.12 | 3.35 | 5.65 | Mechanical and instrument visual |
| 55 | `ANIM_GRIPPER_ASYMMETRY_FAULT` | `L86_AsymmetricGripperFaultInjector` | translation | x | -5.85 | 3.35 | 5.65 | Mechanical and instrument visual |
| 56 | `ANIM_CONTACT_SAFETY_PLC` | `L86_RoboticContactSafetyPLC` | indicator | none | -11.1 | 3.5 | 5.52 | Mechanical and instrument visual |
| 57 | `ANIM_EMERGENCY_ROBOT_STOP` | `L86_ManipulatorEmergencyStopStation` | translation | z | -9.85 | 3.5 | 5.52 | Mechanical and instrument visual |
| 58 | `ANIM_INDEPENDENT_CONTACT_RELAY` | `L86_IndependentContactSafetyRelay` | indicator | none | -6.1 | 3.5 | 5.52 | Mechanical and instrument visual |
| 59 | `ANIM_VISION_FUSION` | `L86_StereoVisionFusionComputer` | indicator | none | -2.35 | 3.5 | 5.52 | Mechanical and instrument visual |
| 60 | `ANIM_SERVICE_SEQUENCE_PLC` | `L86_RadiatorServiceSequencingPLC` | indicator | none | -1.1 | 3.5 | 5.52 | Mechanical and instrument visual |
| 61 | `ANIM_ROBOTICS_SCADA` | `L86_HolographicOrbitalRoboticsSCADA` | indicator | none | 0.0 | 8.8 | -6.33 | Mechanical and instrument visual |
| 62 | `ANIM_W129_TRUSTED_FORCE_FEEDBACK` | `L86_W129CriticalFeedbackPatchStation` | indicator | none | 0.15 | 3.5 | 5.52 | Mechanical and instrument visual |
| 63 | `ANIM_FINAL_SERVICING_CERTIFICATION` | `L86_ServicingMissionCertificationBeacon` | indicator | none | 0.0 | 10.3 | -6.32 | Mechanical and instrument visual |
| 64 | `ANIM_INSPECTION_CAMERA_GANTRY` | `L86_ExternalInspectionCameraGantry` | rotation | y | 7.1 | 7.6 | -0.8 | Mechanical and instrument visual |



## ANNEX C — EVERY PROPOSED PLAYER WIRE W001–W152

Source: `Gameplay_Wiring_Concept_W001_W152.csv`. The following records are **conceptual**, not live wires. Parenthesized `P###` values are decorative art anchors awaiting functional contact reconciliation. **W129** is the only source-marked critical connection.

| Wire | Source model / art anchor | Target model / art anchor | Signal kind | Purpose | Critical |
|---|---|---|---|---|---|
| W001 | `L86_Protected5VControlSupply` (`P135`) | `L86_RoboticContactSafetyPLC` (`P114`) | POWER_CONTROL | 5 V robotic contact safety PLC logic supply | NO |
| W002 | `L86_Protected5VControlSupply` (`P139`) | `L86_RadiatorServiceSequencingPLC` (`P187`) | POWER_CONTROL | 5 V servicing sequencing PLC supply | NO |
| W003 | `L86_Protected5VControlSupply` (`P134`) | `L86_SevenAxisJointServoController` (`P150`) | POWER_CONTROL | 5 V seven-axis joint controller power | NO |
| W004 | `L86_Protected5VControlSupply` (`P138`) | `L86_StereoVisionFusionComputer` (`P177`) | POWER_CONTROL | 5 V stereo vision fusion controller supply | NO |
| W005 | `L86_Protected5VControlSupply` (`P142`) | `L86_ContactForceSignalConditioner` (`P171`) | POWER_CONTROL | 5 V force-torque signal conditioning power | NO |
| W006 | `L86_Protected5VControlSupply` (`P137`) | `L86_JointServoDriveRack` (`P015`) | POWER_CONTROL | 5 V joint motor drive control interface supply | NO |
| W007 | `L86_Protected5VControlSupply` (`P141`) | `L86_HolographicOrbitalRoboticsSCADA` (`P213`) | POWER_CONTROL | 5 V live robotics SCADA supply | NO |
| W008 | `L86_Protected5VControlSupply` (`P136`) | `L86_IndependentContactSafetyRelay` (`P146`) | POWER_CONTROL | 5 V independent contact relay logic power | NO |
| W009 | `L86_Protected5VControlSupply` (`P140`) | `L86_RadiatorLeakTestController` (`P099`) | POWER_CONTROL | 5 V coolant leak test controller supply | NO |
| W010 | `L86_Protected5VControlSupply` (`P135`) | `L86_RobotJointPathPlanningComputer` (`P054`) | POWER_CONTROL | 5 V robotic path planner logic supply | NO |
| W011 | `L86_Protected5VControlSupply` (`P139`) | `L86_HardwareInterlockDiagnosticRack` (`P227`) | POWER_CONTROL | 5 V hardware interlock diagnostic supply | NO |
| W012 | `L86_Protected5VControlSupply` (`P134`) | `L86_W129CriticalFeedbackPatchStation` (`P222`) | POWER_CONTROL | 5 V critical W129 patch signal station supply | NO |
| W013 | `L86_SevenAxisJointServoController` (`P163`) | `L86_RobotJoint1BaseYaw` (`P002`) | ACTUATOR_COMMAND | Joint 1 commanded servo position/torque | NO |
| W014 | `L86_RobotJoint1BaseYaw` (`P002`) | `L86_JointEncoderFeedbackArray` (`P021`) | ANALOG_FEEDBACK | Joint 1 actual encoder and homing reference | NO |
| W015 | `L86_JointTorqueCurrentSensorRack` (`P028`) | `L86_SevenAxisJointServoController` (`P149`) | ANALOG_FEEDBACK | Joint 1 measured motor current and torque | NO |
| W016 | `L86_SevenAxisJointServoController` (`P153`) | `L86_RobotJoint2Shoulder` (`P003`) | ACTUATOR_COMMAND | Joint 2 commanded servo position/torque | NO |
| W017 | `L86_RobotJoint2Shoulder` (`P003`) | `L86_JointEncoderFeedbackArray` (`P018`) | ANALOG_FEEDBACK | Joint 2 actual encoder and homing reference | NO |
| W018 | `L86_JointTorqueCurrentSensorRack` (`P028`) | `L86_SevenAxisJointServoController` (`P158`) | ANALOG_FEEDBACK | Joint 2 measured motor current and torque | NO |
| W019 | `L86_SevenAxisJointServoController` (`P162`) | `L86_RobotJoint3UpperArm` (`P006`) | ACTUATOR_COMMAND | Joint 3 commanded servo position/torque | NO |
| W020 | `L86_RobotJoint3UpperArm` (`P006`) | `L86_JointEncoderFeedbackArray` (`P024`) | ANALOG_FEEDBACK | Joint 3 actual encoder and homing reference | NO |
| W021 | `L86_JointTorqueCurrentSensorRack` (`P028`) | `L86_SevenAxisJointServoController` (`P148`) | ANALOG_FEEDBACK | Joint 3 measured motor current and torque | NO |
| W022 | `L86_SevenAxisJointServoController` (`P152`) | `L86_RobotJoint4Elbow` (`P007`) | ACTUATOR_COMMAND | Joint 4 commanded servo position/torque | NO |
| W023 | `L86_RobotJoint4Elbow` (`P007`) | `L86_JointEncoderFeedbackArray` (`P021`) | ANALOG_FEEDBACK | Joint 4 actual encoder and homing reference | NO |
| W024 | `L86_JointTorqueCurrentSensorRack` (`P028`) | `L86_SevenAxisJointServoController` (`P157`) | ANALOG_FEEDBACK | Joint 4 measured motor current and torque | NO |
| W025 | `L86_SevenAxisJointServoController` (`P161`) | `L86_RobotJoint5Forearm` (`P010`) | ACTUATOR_COMMAND | Joint 5 commanded servo position/torque | NO |
| W026 | `L86_RobotJoint5Forearm` (`P010`) | `L86_JointEncoderFeedbackArray` (`P018`) | ANALOG_FEEDBACK | Joint 5 actual encoder and homing reference | NO |
| W027 | `L86_JointTorqueCurrentSensorRack` (`P028`) | `L86_SevenAxisJointServoController` (`P147`) | ANALOG_FEEDBACK | Joint 5 measured motor current and torque | NO |
| W028 | `L86_SevenAxisJointServoController` (`P151`) | `L86_RobotJoint6WristPitch` (`P011`) | ACTUATOR_COMMAND | Joint 6 commanded servo position/torque | NO |
| W029 | `L86_RobotJoint6WristPitch` (`P011`) | `L86_JointEncoderFeedbackArray` (`P024`) | ANALOG_FEEDBACK | Joint 6 actual encoder and homing reference | NO |
| W030 | `L86_JointTorqueCurrentSensorRack` (`P028`) | `L86_SevenAxisJointServoController` (`P156`) | ANALOG_FEEDBACK | Joint 6 measured motor current and torque | NO |
| W031 | `L86_SevenAxisJointServoController` (`P160`) | `L86_RobotJoint7WristRoll` (`P014`) | ACTUATOR_COMMAND | Joint 7 commanded servo position/torque | NO |
| W032 | `L86_RobotJoint7WristRoll` (`P014`) | `L86_JointEncoderFeedbackArray` (`P021`) | ANALOG_FEEDBACK | Joint 7 actual encoder and homing reference | NO |
| W033 | `L86_JointTorqueCurrentSensorRack` (`P028`) | `L86_SevenAxisJointServoController` (`P165`) | ANALOG_FEEDBACK | Joint 7 measured motor current and torque | NO |
| W034 | `L86_JointEncoderFeedbackArray` (`P017`) | `L86_SevenAxisJointServoController` (`P150`) | DATA_STREAM | Fused seven-joint angle validity and timestamps | NO |
| W035 | `L86_SevenAxisJointServoController` (`P154`) | `L86_RobotBrakeAndTorqueLimiter` (`P032`) | SAFETY_CONTROL | Protective joint brake release request | NO |
| W036 | `L86_RobotBrakeAndTorqueLimiter` (`P032`) | `L86_RoboticContactSafetyPLC` (`P132`) | DIGITAL_STATUS | Measured joint brake mechanism actual state | NO |
| W037 | `L86_StereoMachineVisionCameraLeft` (`P036`) | `L86_StereoVisionFusionComputer` (`P177`) | DATA_STREAM | Left stereo camera frames and hardware timestamps | NO |
| W038 | `L86_StereoMachineVisionCameraRight` (`P037`) | `L86_StereoVisionFusionComputer` (`P181`) | DATA_STREAM | Right stereo camera frames and hardware timestamps | NO |
| W039 | `L86_StructuredLightDepthScanner` (`P040`) | `L86_StereoVisionFusionComputer` (`P173`) | DATA_STREAM | Depth scanner measured structured-light points | NO |
| W040 | `L86_OpticalFiducialTargetArray` (`P041`) | `L86_StereoVisionFusionComputer` (`P177`) | DATA_STREAM | Optical fiducial location and identity | NO |
| W041 | `L86_PrecisionVisionCalibrationBoard` (`P042`) | `L86_StereoVisionFusionComputer` (`P181`) | DATA_STREAM | Calibration board geometric reference | NO |
| W042 | `L86_LaserAlignmentMetrologyUnit` (`P043`) | `L86_StereoVisionFusionComputer` (`P173`) | ANALOG_FEEDBACK | Laser alignment actual offset measurement | NO |
| W043 | `L86_StereoVisionFusionComputer` (`P177`) | `L86_StereoMachineVisionCameraLeft` (`P035`) | DIGITAL_CONTROL | Left camera timed acquisition request | NO |
| W044 | `L86_StereoVisionFusionComputer` (`P181`) | `L86_StereoMachineVisionCameraRight` (`P038`) | DIGITAL_CONTROL | Right camera timed acquisition request | NO |
| W045 | `L86_StereoVisionFusionComputer` (`P173`) | `L86_StructuredLightDepthScanner` (`P039`) | DIGITAL_CONTROL | Depth scanner ranging trigger | NO |
| W046 | `L86_StereoVisionFusionComputer` (`P177`) | `L86_LaserAlignmentMetrologyUnit` (`P044`) | DIGITAL_CONTROL | Laser alignment measurement request | NO |
| W047 | `L86_StereoVisionFusionComputer` (`P181`) | `L86_RobotJointPathPlanningComputer` (`P054`) | DATA_STREAM | Fused target relative position and orientation | NO |
| W048 | `L86_StereoVisionFusionComputer` (`P173`) | `L86_InverseKinematicsSolverDisplay` (`P059`) | DATA_STREAM | Robot tool calibration covariance | NO |
| W049 | `L86_CollisionProximityScanner` (`P062`) | `L86_RobotTrajectorySafetySupervisor` (`P064`) | ANALOG_FEEDBACK | Proximity collision risk and range measurement | NO |
| W050 | `L86_RobotJointPathPlanningComputer` (`P055`) | `L86_RobotTrajectorySafetySupervisor` (`P064`) | DATA_STREAM | Commanded collision-aware trajectory candidates | NO |
| W051 | `L86_RobotTrajectorySafetySupervisor` (`P064`) | `L86_SevenAxisJointServoController` (`P154`) | SAFETY_CONTROL | Validated joint trajectory motion permit | NO |
| W052 | `L86_InverseKinematicsSolverDisplay` (`P060`) | `L86_RobotJointPathPlanningComputer` (`P055`) | DATA_STREAM | Inverse kinematic joint candidate solution | NO |
| W053 | `L86_RobotJointPathPlanningComputer` (`P054`) | `L86_SevenAxisReachEnvelopeHologram` (`P033`) | DATA_STREAM | Robot predicted reach envelope visualization | NO |
| W054 | `L86_RoboticHomingCalibrationFixture` (`P034`) | `L86_SevenAxisJointServoController` (`P160`) | DIGITAL_STATUS | Seven-axis reference homing completion | NO |
| W055 | `L86_StereoVisionFusionComputer` (`P183`) | `L86_HolographicOrbitalRoboticsSCADA` (`P216`) | DATA_STREAM | Vision pose and calibration live telemetry | NO |
| W056 | `L86_CollisionProximityScanner` (`P062`) | `L86_HolographicOrbitalRoboticsSCADA` (`P220`) | DATA_STREAM | Collision clearance and obstacle sensor status | NO |
| W057 | `L86_SixAxisWristForceTorqueSensor` (`P045`) | `L86_ContactForceSignalConditioner` (`P170`) | ANALOG_FEEDBACK | Six-axis wrist load raw strain gauge measurement | NO |
| W058 | `L86_ContactForceSignalConditioner` (`P167`) | `L86_CompliantInsertionController` (`P051`) | ANALOG_FEEDBACK | Conditioned measured contact wrench for insertion control | NO |
| W059 | `L86_CompliantInsertionController` (`P051`) | `L86_SevenAxisJointServoController` (`P149`) | ACTUATOR_COMMAND | Compliant contact motion correction request | NO |
| W060 | `L86_ForceTorqueCalibrationFixture` (`P048`) | `L86_ContactForceSignalConditioner` (`P172`) | ANALOG_FEEDBACK | Wrist six-axis load calibration reference | NO |
| W061 | `L86_ContactForceSignalConditioner` (`P169`) | `L86_SixAxisContactForceHologram` (`P053`) | DATA_STREAM | Live force torque wrench holographic vector | NO |
| W062 | `L86_ContactForceSignalConditioner` (`P166`) | `L86_RobotTrajectorySafetySupervisor` (`P066`) | SAFETY_CONTROL | Force torque contact overload warning | NO |
| W063 | `L86_IndependentContactSafetyRelay` (`P144`) | `L86_CompliantInsertionController` (`P050`) | SAFETY_CONTROL | Independent contact motion enable | NO |
| W064 | `L86_SevenAxisJointServoController` (`P157`) | `L86_CompliantInsertionController` (`P050`) | ANALOG_FEEDBACK | Joint servo response for compliant insertion | NO |
| W065 | `L86_CompliantInsertionController` (`P050`) | `L86_RadiatorServiceSequencingPLC` (`P190`) | DIGITAL_STATUS | Contact insertion state and alignment error | NO |
| W066 | `L86_FlexiblePayloadModalSensor` (`P067`) | `L86_ActiveVibrationDampingController` (`P069`) | ANALOG_FEEDBACK | Actual flexible radiator vibration sensor response | NO |
| W067 | `L86_ActiveVibrationDampingController` (`P070`) | `L86_RobotJointPathPlanningComputer` (`P056`) | ACTUATOR_COMMAND | Recommended shaped-trajectory acceleration limit | NO |
| W068 | `L86_PayloadVibrationFaultInjector` (`P110`) | `L86_FlexiblePayloadModalSensor` (`P068`) | SAFETY_CONTROL | Controlled flexible payload vibration disturbance | NO |
| W069 | `L86_JointEncoderDriftFaultInjector` (`P111`) | `L86_JointEncoderFeedbackArray` (`P019`) | SAFETY_CONTROL | Injected joint encoder drift measurement bias | NO |
| W070 | `L86_JointEncoderFeedbackArray` (`P023`) | `L86_RobotTrajectorySafetySupervisor` (`P065`) | ANALOG_FEEDBACK | Crosschecked individual joint-angle discrepancy | NO |
| W071 | `L86_DualJawGripperPressureSensorArray` (`P088`) | `L86_ContactForceSignalConditioner` (`P171`) | ANALOG_FEEDBACK | Independent gripper pressure cell array health | NO |
| W072 | `L86_ContactForceSignalConditioner` (`P168`) | `L86_RadiatorServiceSequencingPLC` (`P200`) | DIGITAL_STATUS | Grasp-load balance qualification result | NO |
| W073 | `L86_SixAxisWristForceTorqueSensor` (`P046`) | `L86_HolographicOrbitalRoboticsSCADA` (`P208`) | DATA_STREAM | Non-authoritative force torque SCADA stream | NO |
| W074 | `L86_FlexiblePayloadModalSensor` (`P067`) | `L86_HolographicOrbitalRoboticsSCADA` (`P212`) | ANALOG_FEEDBACK | Payload flex dominant mode response telemetry | NO |
| W075 | `L86_ActiveVibrationDampingController` (`P069`) | `L86_SevenAxisJointServoController` (`P152`) | ACTUATOR_COMMAND | Mode damping joint acceleration filter | NO |
| W076 | `L86_IndependentContactSafetyRelay` (`P145`) | `L86_RoboticContactSafetyPLC` (`P115`) | DIGITAL_STATUS | Independent contact permission relay diagnostic | NO |
| W077 | `L86_RadiatorServiceSequencingPLC` (`P187`) | `L86_AdaptiveRadiatorHandlingGripper` (`P073`) | ACTUATOR_COMMAND | Robot gripper grasp/release command | NO |
| W078 | `L86_AdaptiveRadiatorHandlingGripper` (`P073`) | `L86_DualJawGripperPressureSensorArray` (`P086`) | ANALOG_FEEDBACK | Measured jaw surface pressure and contact state | NO |
| W079 | `L86_RadiatorServiceSequencingPLC` (`P192`) | `L86_AutomaticEndEffectorToolChanger` (`P077`) | ACTUATOR_COMMAND | Automatic tool changer selected tool request | NO |
| W080 | `L86_AutomaticEndEffectorToolChanger` (`P077`) | `L86_RadiatorServiceSequencingPLC` (`P196`) | DIGITAL_STATUS | Actual end-effector lock confirmation | NO |
| W081 | `L86_RadiatorServiceSequencingPLC` (`P200`) | `L86_PrecisionTorqueFasteningDriver` (`P078`) | ACTUATOR_COMMAND | Motorized fastener tightening torque request | NO |
| W082 | `L86_PrecisionTorqueFasteningDriver` (`P079`) | `L86_RadiatorServiceSequencingPLC` (`P204`) | ANALOG_FEEDBACK | Measured fastener torque and turn count | NO |
| W083 | `L86_CaptiveFastenerAndBoltRack` (`P081`) | `L86_PrecisionTorqueFasteningDriver` (`P080`) | DIGITAL_STATUS | Captive bolt stock and insertion status | NO |
| W084 | `L86_RadiatorServiceSequencingPLC` (`P188`) | `L86_VacuumCompatibleInspectionProbe` (`P082`) | DIGITAL_CONTROL | Borescope inspection sequence trigger | NO |
| W085 | `L86_VacuumCompatibleInspectionProbe` (`P082`) | `L86_StereoVisionFusionComputer` (`P180`) | DATA_STREAM | Connector bore-scope image evidence | NO |
| W086 | `L86_RoboticToolDockAndCalibrationNest` (`P084`) | `L86_AutomaticEndEffectorToolChanger` (`P076`) | DIGITAL_STATUS | Tool changer index/homing identity feedback | NO |
| W087 | `L86_RadiatorGrappleHandleFixture` (`P089`) | `L86_AdaptiveRadiatorHandlingGripper` (`P073`) | ANALOG_FEEDBACK | Payload grapple fixture captured status | NO |
| W088 | `L86_AdaptiveRadiatorHandlingGripper` (`P073`) | `L86_RoboticContactSafetyPLC` (`P130`) | SAFETY_CONTROL | Gripper hold continuity protective status | NO |
| W089 | `L86_AsymmetricGripperFaultInjector` (`P112`) | `L86_DualJawGripperPressureSensorArray` (`P088`) | SAFETY_CONTROL | Asymmetric gripper pad contact fault condition | NO |
| W090 | `L86_DualJawGripperPressureSensorArray` (`P088`) | `L86_RadiatorServiceSequencingPLC` (`P197`) | ANALOG_FEEDBACK | Gripper contact imbalance fault diagnostic | NO |
| W091 | `L86_RadiatorFastenerActuatorDriver` (`P103`) | `L86_PrecisionTorqueFasteningDriver` (`P079`) | DIGITAL_STATUS | Fastening driver actuator readiness and limit | NO |
| W092 | `L86_RadiatorServiceSequencingPLC` (`P202`) | `L86_RadiatorFastenerActuatorDriver` (`P103`) | ACTUATOR_COMMAND | Fastener driver motion enable command | NO |
| W093 | `L86_FailedRadiatorDisposalCradle` (`P109`) | `L86_RadiatorServiceSequencingPLC` (`P185`) | DIGITAL_STATUS | Defective radiator waste cradle secured state | NO |
| W094 | `L86_AdaptiveRadiatorHandlingGripper` (`P074`) | `L86_HolographicOrbitalRoboticsSCADA` (`P219`) | DATA_STREAM | Gripper actual jaw positions and tool health | NO |
| W095 | `L86_RadiatorServiceSequencingPLC` (`P190`) | `L86_CoolantIsolationAndVentStation` (`P102`) | ACTUATOR_COMMAND | Radiator line isolation and vent request | NO |
| W096 | `L86_CoolantIsolationAndVentStation` (`P102`) | `L86_RadiatorServiceSequencingPLC` (`P194`) | ANALOG_FEEDBACK | Measured safe cooled line depressurization | NO |
| W097 | `L86_RadiatorServiceSequencingPLC` (`P198`) | `L86_CoolantQuickDisconnectA` (`P090`) | ACTUATOR_COMMAND | Coolant connector A safe mating command | NO |
| W098 | `L86_RadiatorServiceSequencingPLC` (`P202`) | `L86_CoolantQuickDisconnectB` (`P091`) | ACTUATOR_COMMAND | Coolant connector B safe mating command | NO |
| W099 | `L86_CoolantQuickDisconnectA` (`P090`) | `L86_RadiatorServiceSequencingPLC` (`P185`) | ANALOG_FEEDBACK | Connector A actual sealed latch feedback | NO |
| W100 | `L86_CoolantQuickDisconnectB` (`P091`) | `L86_RadiatorServiceSequencingPLC` (`P189`) | ANALOG_FEEDBACK | Connector B actual sealed latch feedback | NO |
| W101 | `L86_RadiatorServiceSequencingPLC` (`P193`) | `L86_CoolantValveManifold` (`P092`) | ACTUATOR_COMMAND | Cooling valve controlled-open command | NO |
| W102 | `L86_CoolantValveManifold` (`P092`) | `L86_RadiatorServiceSequencingPLC` (`P197`) | ANALOG_FEEDBACK | Actual coolant valve position measurement | NO |
| W103 | `L86_CoolantPressureTransducer` (`P093`) | `L86_RadiatorLeakTestController` (`P100`) | ANALOG_FEEDBACK | Main coolant circuit measured pressure | NO |
| W104 | `L86_CoolantFlowMeter` (`P094`) | `L86_RadiatorLeakTestController` (`P100`) | ANALOG_FEEDBACK | Main coolant circuit measured flow rate | NO |
| W105 | `L86_RadiatorTemperatureSensorArray` (`P095`) | `L86_RadiatorThermalPerformanceAnalyzer` (`P106`) | ANALOG_FEEDBACK | Radiator temperature sensor array samples | NO |
| W106 | `L86_StationAvionicsHeatLoadEmulator` (`P096`) | `L86_RadiatorThermalPerformanceAnalyzer` (`P106`) | ANALOG_FEEDBACK | Real station avionics heat demand telemetry | NO |
| W107 | `L86_ThermalLoopPumpSimulator` (`P097`) | `L86_RadiatorThermalPerformanceAnalyzer` (`P106`) | ANALOG_FEEDBACK | Coolant pump actual duty and throughput | NO |
| W108 | `L86_RadiatorLeakTestController` (`P099`) | `L86_RadiatorServiceSequencingPLC` (`P185`) | DIGITAL_STATUS | Closed-loop coolant qualification leak status | NO |
| W109 | `L86_RadiatorThermalPerformanceAnalyzer` (`P107`) | `L86_RadiatorServiceSequencingPLC` (`P189`) | DATA_STREAM | Integrated radiator heat rejection qualification | NO |
| W110 | `L86_RadiatorServiceSequencingPLC` (`P193`) | `L86_ThermalLoopPumpSimulator` (`P097`) | ACTUATOR_COMMAND | Qualified pump startup and control request | NO |
| W111 | `L86_RadiatorThermalPerformanceAnalyzer` (`P108`) | `L86_StationThermalTrendHologram` (`P104`) | DATA_STREAM | Live station thermal trend and cooling rate | NO |
| W112 | `L86_RadiatorTemperatureSensorArray` (`P095`) | `L86_HolographicOrbitalRoboticsSCADA` (`P208`) | DATA_STREAM | Radiator inlet outlet thermistor SCADA data | NO |
| W113 | `L86_ManipulatorEmergencyStopStation` (`P133`) | `L86_RoboticContactSafetyPLC` (`P118`) | SAFETY_CONTROL | Operator emergency manipulator stop hardwired status | NO |
| W114 | `L86_IndependentContactSafetyRelay` (`P144`) | `L86_RoboticContactSafetyPLC` (`P122`) | SAFETY_CONTROL | Independent contact safety protective relay state | NO |
| W115 | `L86_CollisionProximityScanner` (`P062`) | `L86_RoboticContactSafetyPLC` (`P126`) | SAFETY_CONTROL | Independent collision interlock risk status | NO |
| W116 | `L86_RobotTrajectorySafetySupervisor` (`P066`) | `L86_RoboticContactSafetyPLC` (`P130`) | SAFETY_CONTROL | Robot trajectory safe-motion qualification | NO |
| W117 | `L86_JointEncoderFeedbackArray` (`P019`) | `L86_RoboticContactSafetyPLC` (`P114`) | SAFETY_CONTROL | Joint encoder validity disagreement interlock | NO |
| W118 | `L86_DualJawGripperPressureSensorArray` (`P087`) | `L86_RoboticContactSafetyPLC` (`P118`) | SAFETY_CONTROL | Independent gripper hold pressure safety | NO |
| W119 | `L86_RadiatorServiceSequencingPLC` (`P205`) | `L86_RoboticContactSafetyPLC` (`P122`) | SAFETY_CONTROL | Contact and tooling motion authorization request | NO |
| W120 | `L86_RoboticContactSafetyPLC` (`P126`) | `L86_SevenAxisJointServoController` (`P162`) | SAFETY_CONTROL | Protected joint drive and brake enable | NO |
| W121 | `L86_RoboticContactSafetyPLC` (`P130`) | `L86_AdaptiveRadiatorHandlingGripper` (`P072`) | SAFETY_CONTROL | Protective gripper hold/release inhibit | NO |
| W122 | `L86_RoboticContactSafetyPLC` (`P114`) | `L86_CompliantInsertionController` (`P050`) | SAFETY_CONTROL | Protected compliant contact permit | NO |
| W123 | `L86_RoboticContactSafetyPLC` (`P118`) | `L86_RadiatorServiceSequencingPLC` (`P191`) | SAFETY_CONTROL | Independent service sequence emergency inhibit | NO |
| W124 | `L86_JointEncoderDriftFaultInjector` (`P111`) | `L86_RoboticContactSafetyPLC` (`P122`) | DIGITAL_STATUS | Encoder drift fault injection diagnostic | NO |
| W125 | `L86_PayloadVibrationFaultInjector` (`P110`) | `L86_RoboticContactSafetyPLC` (`P126`) | DIGITAL_STATUS | Flex oscillation overload safety status | NO |
| W126 | `L86_AsymmetricGripperFaultInjector` (`P112`) | `L86_RoboticContactSafetyPLC` (`P130`) | DIGITAL_STATUS | Asymmetric gripper contact safety status | NO |
| W127 | `L86_RoboticContactSafetyPLC` (`P114`) | `L86_HardwareInterlockDiagnosticRack` (`P226`) | DIGITAL_STATUS | Safety interlock fault latch diagnostic | NO |
| W128 | `L86_ContactForceSignalConditioner` (`P171`) | `L86_W129CriticalFeedbackPatchStation` (`P224`) | ANALOG_FEEDBACK | Wrist conditioned load sample diagnostic patch | NO |
| **W129** | `L86_SixAxisWristForceTorqueSensor` (`P045`) | `L86_RoboticContactSafetyPLC` (`P119`) | ANALOG_FEEDBACK | CRITICAL trusted six-axis wrist contact wrench W129 | YES |
| W130 | `L86_W129CriticalFeedbackPatchStation` (`P223`) | `L86_RoboticContactSafetyPLC` (`P123`) | DIGITAL_STATUS | Critical W129 patch continuity diagnostic only | NO |
| W131 | `L86_RoboticContactSafetyPLC` (`P127`) | `L86_HardwareInterlockDiagnosticRack` (`P227`) | DIGITAL_STATUS | W129 trusted-sample freshness and fault status | NO |
| W132 | `L86_HardwareInterlockDiagnosticRack` (`P225`) | `L86_RoboticContactSafetyPLC` (`P131`) | SAFETY_CONTROL | Operator explicit fault reset request | NO |
| W133 | `L86_RoboticContactSafetyPLC` (`P115`) | `L86_RadiatorServiceSequencingPLC` (`P204`) | SAFETY_CONTROL | Post-repair safety recommissioning clearance | NO |
| W134 | `L86_RobotMotorPowerDistributionUnit` (`P143`) | `L86_SevenAxisJointServoController` (`P160`) | SAFETY_CONTROL | Isolated fictional joint motor plant power permit | NO |
| W135 | `L86_RoboticContactSafetyPLC` (`P120`) | `L86_HolographicOrbitalRoboticsSCADA` (`P218`) | DIGITAL_STATUS | Critical fault latch and motion permission telemetry | NO |
| W136 | `L86_RoboticContactSafetyPLC` (`P124`) | `L86_ServicingMissionCertificationBeacon` (`P228`) | DIGITAL_STATUS | Safety-qualified mission completion certificate | NO |
| W137 | `L86_RadiatorServiceSequencingPLC` (`P190`) | `L86_HolographicOrbitalRoboticsSCADA` (`P207`) | DATA_STREAM | Servicing mission objective evidence state | NO |
| W138 | `L86_SevenAxisJointServoController` (`P148`) | `L86_HolographicOrbitalRoboticsSCADA` (`P211`) | DATA_STREAM | Real seven-axis servo position and error trace | NO |
| W139 | `L86_JointEncoderFeedbackArray` (`P017`) | `L86_HolographicOrbitalRoboticsSCADA` (`P215`) | DATA_STREAM | Encoder positions timestamp and health stream | NO |
| W140 | `L86_JointTorqueCurrentSensorRack` (`P030`) | `L86_HolographicOrbitalRoboticsSCADA` (`P219`) | DATA_STREAM | Motor winding current and load live trace | NO |
| W141 | `L86_ContactForceSignalConditioner` (`P166`) | `L86_HolographicOrbitalRoboticsSCADA` (`P207`) | DATA_STREAM | Conditioned wrist wrench diagnostic telemetry | NO |
| W142 | `L86_StereoVisionFusionComputer` (`P180`) | `L86_HolographicOrbitalRoboticsSCADA` (`P211`) | DATA_STREAM | Machine vision calibration and target pose | NO |
| W143 | `L86_RadiatorThermalPerformanceAnalyzer` (`P107`) | `L86_HolographicOrbitalRoboticsSCADA` (`P215`) | DATA_STREAM | Radiator pressure flow energy balance summary | NO |
| W144 | `L86_RadiatorLeakTestController` (`P098`) | `L86_HolographicOrbitalRoboticsSCADA` (`P219`) | DATA_STREAM | Cooling leak test results and sample time | NO |
| W145 | `L86_RobotJointPathPlanningComputer` (`P057`) | `L86_HolographicOrbitalRoboticsSCADA` (`P207`) | DATA_STREAM | Tool path collision clearance predictions | NO |
| W146 | `L86_ActiveVibrationDampingController` (`P071`) | `L86_HolographicOrbitalRoboticsSCADA` (`P211`) | DATA_STREAM | Flexible load oscillation estimate | NO |
| W147 | `L86_RoboticContactSafetyPLC` (`P118`) | `L86_HolographicOrbitalRoboticsSCADA` (`P215`) | DIGITAL_STATUS | Independent contact safety system live status | NO |
| W148 | `L86_HolographicOrbitalRoboticsSCADA` (`P219`) | `L86_RoboticServicingEventRecorder` (`P230`) | DATA_STREAM | Mission event recorder chronology and run IDs | NO |
| W149 | `L86_RoboticServicingEventRecorder` (`P230`) | `L86_RadiatorServiceSequencingPLC` (`P205`) | DATA_STREAM | Requalification run evidence fingerprint | NO |
| W150 | `L86_RadiatorServiceSequencingPLC` (`P188`) | `L86_ServicingMissionCertificationBeacon` (`P229`) | DIGITAL_STATUS | Six-objective final completion authorization | NO |
| W151 | `L86_ServicingMissionCertificationBeacon` (`P229`) | `L86_HolographicOrbitalRoboticsSCADA` (`P209`) | DIGITAL_STATUS | Mission qualification beacon animation status | NO |
| W152 | `L86_W129CriticalFeedbackPatchStation` (`P224`) | `L86_HolographicOrbitalRoboticsSCADA` (`P213`) | DIGITAL_STATUS | W129 critical patch diagnostic display only | NO |



## ANNEX D — ALL 230 MODELED DECORATIVE TERMINAL ANCHORS

Source: `Terminal_Anchor_Plan_230.csv`. Each `P###` location belongs to an art-owner model, not to a functioning Spark electrical node. Preserve all 230 markers; scale from source file coordinates carefully.

| Anchor ID | Owner model | WorldX_m | WorldY_m | WorldZ_m | Status |
|---|---|---|---|---|---|
| P001 | L86_RobotJoint1BaseYaw | -4.97 | 2.3688 | -1.3 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P002 | L86_RobotJoint1BaseYaw | -4.83 | 2.3688 | -1.3 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P003 | L86_RobotJoint2Shoulder | -4.57 | 2.8788 | -1.3 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P004 | L86_RobotJoint2Shoulder | -4.43 | 2.8788 | -1.3 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P005 | L86_RobotJoint3UpperArm | -3.72 | 3.5188 | -1.3 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P006 | L86_RobotJoint3UpperArm | -3.58 | 3.5188 | -1.3 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P007 | L86_RobotJoint4Elbow | -2.77 | 4.0688 | -1.3 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P008 | L86_RobotJoint4Elbow | -2.63 | 4.0688 | -1.3 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P009 | L86_RobotJoint5Forearm | -1.72 | 4.5888 | -1.3 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P010 | L86_RobotJoint5Forearm | -1.58 | 4.5888 | -1.3 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P011 | L86_RobotJoint6WristPitch | -0.97 | 4.9988 | -1.3 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P012 | L86_RobotJoint6WristPitch | -0.83 | 4.9988 | -1.3 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P013 | L86_RobotJoint7WristRoll | -0.17 | 5.2688 | -1.3 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P014 | L86_RobotJoint7WristRoll | -0.03 | 5.2688 | -1.3 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P015 | L86_JointServoDriveRack | -12.77 | 1.5588 | 6.01 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P016 | L86_JointEncoderFeedbackArray | -11.52 | 1.5588 | 6.01 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P017 | L86_JointEncoderFeedbackArray | -11.38 | 1.5588 | 6.01 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P018 | L86_JointEncoderFeedbackArray | -11.24 | 1.5588 | 6.01 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P019 | L86_JointEncoderFeedbackArray | -11.1 | 1.5588 | 6.01 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P020 | L86_JointEncoderFeedbackArray | -10.96 | 1.5588 | 6.01 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P021 | L86_JointEncoderFeedbackArray | -10.82 | 1.5588 | 6.01 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P022 | L86_JointEncoderFeedbackArray | -10.68 | 1.5588 | 6.01 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P023 | L86_JointEncoderFeedbackArray | -11.52 | 1.6668 | 6.01 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P024 | L86_JointEncoderFeedbackArray | -11.38 | 1.6668 | 6.01 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P025 | L86_JointTorqueCurrentSensorRack | -10.27 | 1.5588 | 6.01 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P026 | L86_JointTorqueCurrentSensorRack | -10.13 | 1.5588 | 6.01 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P027 | L86_JointTorqueCurrentSensorRack | -9.99 | 1.5588 | 6.01 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P028 | L86_JointTorqueCurrentSensorRack | -9.85 | 1.5588 | 6.01 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P029 | L86_JointTorqueCurrentSensorRack | -9.71 | 1.5588 | 6.01 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P030 | L86_JointTorqueCurrentSensorRack | -9.57 | 1.5588 | 6.01 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P031 | L86_RobotBrakeAndTorqueLimiter | -9.02 | 1.5588 | 6.01 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P032 | L86_RobotBrakeAndTorqueLimiter | -8.88 | 1.5588 | 6.01 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P033 | L86_SevenAxisReachEnvelopeHologram | -3.52 | 7.4988 | -5.44 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P034 | L86_RoboticHomingCalibrationFixture | -6.37 | 1.5488 | 4.71 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P035 | L86_StereoMachineVisionCameraLeft | -1.62 | 6.1488 | -0.64 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P036 | L86_StereoMachineVisionCameraLeft | -1.48 | 6.1488 | -0.64 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P037 | L86_StereoMachineVisionCameraRight | -0.87 | 6.1488 | -0.64 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P038 | L86_StereoMachineVisionCameraRight | -0.73 | 6.1488 | -0.64 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P039 | L86_StructuredLightDepthScanner | 0.63 | 6.0988 | -1.24 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P040 | L86_StructuredLightDepthScanner | 0.77 | 6.0988 | -1.24 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P041 | L86_OpticalFiducialTargetArray | 7.08 | 5.6988 | -2.19 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P042 | L86_PrecisionVisionCalibrationBoard | -3.62 | 1.5388 | 4.82 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P043 | L86_LaserAlignmentMetrologyUnit | 1.98 | 6.3488 | -2.29 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P044 | L86_LaserAlignmentMetrologyUnit | 2.12 | 6.3488 | -2.29 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P045 | L86_SixAxisWristForceTorqueSensor | 0.48 | 5.1488 | -1.3 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P046 | L86_SixAxisWristForceTorqueSensor | 0.62 | 5.1488 | -1.3 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P047 | L86_SixAxisWristForceTorqueSensor | 0.76 | 5.1488 | -1.3 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P048 | L86_ForceTorqueCalibrationFixture | -2.37 | 1.5488 | 5.91 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P049 | L86_CompliantInsertionController | -1.12 | 1.5488 | 5.91 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P050 | L86_CompliantInsertionController | -0.98 | 1.5488 | 5.91 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P051 | L86_CompliantInsertionController | -0.84 | 1.5488 | 5.91 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P052 | L86_CompliantInsertionController | -0.7 | 1.5488 | 5.91 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P053 | L86_SixAxisContactForceHologram | 2.98 | 7.1688 | -5.49 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P054 | L86_RobotJointPathPlanningComputer | 0.14 | 1.5488 | 5.91 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P055 | L86_RobotJointPathPlanningComputer | 0.28 | 1.5488 | 5.91 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P056 | L86_RobotJointPathPlanningComputer | 0.42 | 1.5488 | 5.91 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P057 | L86_RobotJointPathPlanningComputer | 0.56 | 1.5488 | 5.91 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P058 | L86_RobotJointPathPlanningComputer | 0.7 | 1.5488 | 5.91 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P059 | L86_InverseKinematicsSolverDisplay | 1.39 | 1.5488 | 5.91 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P060 | L86_InverseKinematicsSolverDisplay | 1.53 | 1.5488 | 5.91 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P061 | L86_CollisionProximityScanner | 2.64 | 1.5488 | 5.91 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P062 | L86_CollisionProximityScanner | 2.78 | 1.5488 | 5.91 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P063 | L86_RobotTrajectorySafetySupervisor | 3.89 | 1.5488 | 5.91 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P064 | L86_RobotTrajectorySafetySupervisor | 4.03 | 1.5488 | 5.91 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P065 | L86_RobotTrajectorySafetySupervisor | 4.17 | 1.5488 | 5.91 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P066 | L86_RobotTrajectorySafetySupervisor | 4.31 | 1.5488 | 5.91 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P067 | L86_FlexiblePayloadModalSensor | 0.73 | 5.3988 | -1.19 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P068 | L86_FlexiblePayloadModalSensor | 0.87 | 5.3988 | -1.19 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P069 | L86_ActiveVibrationDampingController | 5.14 | 1.5488 | 5.91 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P070 | L86_ActiveVibrationDampingController | 5.28 | 1.5488 | 5.91 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P071 | L86_ActiveVibrationDampingController | 5.42 | 1.5488 | 5.91 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P072 | L86_AdaptiveRadiatorHandlingGripper | 1.08 | 5.0688 | -1.3 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P073 | L86_AdaptiveRadiatorHandlingGripper | 1.22 | 5.0688 | -1.3 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P074 | L86_AdaptiveRadiatorHandlingGripper | 1.36 | 5.0688 | -1.3 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P075 | L86_AdaptiveRadiatorHandlingGripper | 1.5 | 5.0688 | -1.3 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P076 | L86_AutomaticEndEffectorToolChanger | 0.43 | 5.0288 | -1.06 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P077 | L86_AutomaticEndEffectorToolChanger | 0.57 | 5.0288 | -1.06 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P078 | L86_PrecisionTorqueFasteningDriver | 1.9 | 4.9488 | -1.06 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P079 | L86_PrecisionTorqueFasteningDriver | 2.04 | 4.9488 | -1.06 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P080 | L86_PrecisionTorqueFasteningDriver | 2.18 | 4.9488 | -1.06 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P081 | L86_CaptiveFastenerAndBoltRack | -5.62 | 1.2988 | 5.81 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P082 | L86_VacuumCompatibleInspectionProbe | 0.51 | 5.4088 | -0.99 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P083 | L86_VacuumCompatibleInspectionProbe | 0.65 | 5.4088 | -0.99 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P084 | L86_RoboticToolDockAndCalibrationNest | -6.07 | 1.5288 | 4.28 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P085 | L86_DualJawGripperPressureSensorArray | 1.18 | 4.9488 | -1.04 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P086 | L86_DualJawGripperPressureSensorArray | 1.32 | 4.9488 | -1.04 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P087 | L86_DualJawGripperPressureSensorArray | 1.46 | 4.9488 | -1.04 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P088 | L86_DualJawGripperPressureSensorArray | 1.6 | 4.9488 | -1.04 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P089 | L86_RadiatorGrappleHandleFixture | 6.38 | 5.3488 | -1.71 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P090 | L86_CoolantQuickDisconnectA | 6.58 | 4.4488 | -1.39 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P091 | L86_CoolantQuickDisconnectB | 8.98 | 4.4488 | -1.39 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P092 | L86_CoolantValveManifold | 8.73 | 1.4888 | 5.91 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P093 | L86_CoolantPressureTransducer | 9.93 | 1.4888 | 5.91 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P094 | L86_CoolantFlowMeter | 11.13 | 1.4888 | 5.91 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P095 | L86_RadiatorTemperatureSensorArray | 9.48 | 5.0488 | -2.34 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P096 | L86_StationAvionicsHeatLoadEmulator | -3.12 | 6.0488 | -2.89 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P097 | L86_ThermalLoopPumpSimulator | 6.63 | 1.5188 | 5.96 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P098 | L86_RadiatorLeakTestController | 7.98 | 1.5088 | 5.98 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P099 | L86_RadiatorLeakTestController | 8.12 | 1.5088 | 5.98 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P100 | L86_RadiatorLeakTestController | 8.26 | 1.5088 | 5.98 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P101 | L86_RadiatorLeakTestController | 8.4 | 1.5088 | 5.98 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P102 | L86_CoolantIsolationAndVentStation | 9.68 | 3.1488 | 5.62 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P103 | L86_RadiatorFastenerActuatorDriver | 6.72 | 5.4988 | -2.24 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P104 | L86_StationThermalTrendHologram | -0.42 | 8.4988 | -5.64 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P105 | L86_RadiatorThermalPerformanceAnalyzer | 10.86 | 3.1488 | 5.62 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P106 | L86_RadiatorThermalPerformanceAnalyzer | 11.0 | 3.1488 | 5.62 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P107 | L86_RadiatorThermalPerformanceAnalyzer | 11.14 | 3.1488 | 5.62 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P108 | L86_RadiatorThermalPerformanceAnalyzer | 11.28 | 3.1488 | 5.62 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P109 | L86_FailedRadiatorDisposalCradle | 8.83 | 0.9488 | 3.66 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P110 | L86_PayloadVibrationFaultInjector | 6.33 | 3.1988 | 6.11 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P111 | L86_JointEncoderDriftFaultInjector | -7.54 | 3.1988 | 6.11 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P112 | L86_AsymmetricGripperFaultInjector | -6.27 | 3.1988 | 6.11 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P113 | L86_RoboticContactSafetyPLC | -11.52 | 3.3488 | 5.98 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P114 | L86_RoboticContactSafetyPLC | -11.38 | 3.3488 | 5.98 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P115 | L86_RoboticContactSafetyPLC | -11.24 | 3.3488 | 5.98 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P116 | L86_RoboticContactSafetyPLC | -11.1 | 3.3488 | 5.98 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P117 | L86_RoboticContactSafetyPLC | -10.96 | 3.3488 | 5.98 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P118 | L86_RoboticContactSafetyPLC | -10.82 | 3.3488 | 5.98 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P119 | L86_RoboticContactSafetyPLC | -10.68 | 3.3488 | 5.98 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P120 | L86_RoboticContactSafetyPLC | -11.52 | 3.4568 | 5.98 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P121 | L86_RoboticContactSafetyPLC | -11.38 | 3.4568 | 5.98 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P122 | L86_RoboticContactSafetyPLC | -11.24 | 3.4568 | 5.98 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P123 | L86_RoboticContactSafetyPLC | -11.1 | 3.4568 | 5.98 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P124 | L86_RoboticContactSafetyPLC | -10.96 | 3.4568 | 5.98 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P125 | L86_RoboticContactSafetyPLC | -10.82 | 3.4568 | 5.98 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P126 | L86_RoboticContactSafetyPLC | -10.68 | 3.4568 | 5.98 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P127 | L86_RoboticContactSafetyPLC | -11.52 | 3.5648 | 5.98 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P128 | L86_RoboticContactSafetyPLC | -11.38 | 3.5648 | 5.98 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P129 | L86_RoboticContactSafetyPLC | -11.24 | 3.5648 | 5.98 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P130 | L86_RoboticContactSafetyPLC | -11.1 | 3.5648 | 5.98 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P131 | L86_RoboticContactSafetyPLC | -10.96 | 3.5648 | 5.98 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P132 | L86_RoboticContactSafetyPLC | -10.82 | 3.5648 | 5.98 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P133 | L86_ManipulatorEmergencyStopStation | -10.27 | 3.3488 | 5.98 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P134 | L86_Protected5VControlSupply | -9.02 | 3.3488 | 5.98 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P135 | L86_Protected5VControlSupply | -8.88 | 3.3488 | 5.98 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P136 | L86_Protected5VControlSupply | -8.74 | 3.3488 | 5.98 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P137 | L86_Protected5VControlSupply | -8.6 | 3.3488 | 5.98 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P138 | L86_Protected5VControlSupply | -8.46 | 3.3488 | 5.98 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P139 | L86_Protected5VControlSupply | -8.32 | 3.3488 | 5.98 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P140 | L86_Protected5VControlSupply | -8.18 | 3.3488 | 5.98 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P141 | L86_Protected5VControlSupply | -9.02 | 3.4568 | 5.98 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P142 | L86_Protected5VControlSupply | -8.88 | 3.4568 | 5.98 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P143 | L86_RobotMotorPowerDistributionUnit | -7.77 | 3.3488 | 5.98 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P144 | L86_IndependentContactSafetyRelay | -6.52 | 3.3488 | 5.98 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P145 | L86_IndependentContactSafetyRelay | -6.38 | 3.3488 | 5.98 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P146 | L86_IndependentContactSafetyRelay | -6.24 | 3.3488 | 5.98 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P147 | L86_SevenAxisJointServoController | -5.27 | 3.3488 | 5.98 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P148 | L86_SevenAxisJointServoController | -5.13 | 3.3488 | 5.98 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P149 | L86_SevenAxisJointServoController | -4.99 | 3.3488 | 5.98 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P150 | L86_SevenAxisJointServoController | -4.85 | 3.3488 | 5.98 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P151 | L86_SevenAxisJointServoController | -4.71 | 3.3488 | 5.98 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P152 | L86_SevenAxisJointServoController | -4.57 | 3.3488 | 5.98 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P153 | L86_SevenAxisJointServoController | -4.43 | 3.3488 | 5.98 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P154 | L86_SevenAxisJointServoController | -5.27 | 3.4568 | 5.98 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P155 | L86_SevenAxisJointServoController | -5.13 | 3.4568 | 5.98 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P156 | L86_SevenAxisJointServoController | -4.99 | 3.4568 | 5.98 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P157 | L86_SevenAxisJointServoController | -4.85 | 3.4568 | 5.98 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P158 | L86_SevenAxisJointServoController | -4.71 | 3.4568 | 5.98 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P159 | L86_SevenAxisJointServoController | -4.57 | 3.4568 | 5.98 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P160 | L86_SevenAxisJointServoController | -4.43 | 3.4568 | 5.98 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P161 | L86_SevenAxisJointServoController | -5.27 | 3.5648 | 5.98 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P162 | L86_SevenAxisJointServoController | -5.13 | 3.5648 | 5.98 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P163 | L86_SevenAxisJointServoController | -4.99 | 3.5648 | 5.98 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P164 | L86_SevenAxisJointServoController | -4.85 | 3.5648 | 5.98 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P165 | L86_SevenAxisJointServoController | -4.71 | 3.5648 | 5.98 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P166 | L86_ContactForceSignalConditioner | -4.02 | 3.3488 | 5.98 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P167 | L86_ContactForceSignalConditioner | -3.88 | 3.3488 | 5.98 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P168 | L86_ContactForceSignalConditioner | -3.74 | 3.3488 | 5.98 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P169 | L86_ContactForceSignalConditioner | -3.6 | 3.3488 | 5.98 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P170 | L86_ContactForceSignalConditioner | -3.46 | 3.3488 | 5.98 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P171 | L86_ContactForceSignalConditioner | -3.32 | 3.3488 | 5.98 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P172 | L86_ContactForceSignalConditioner | -3.18 | 3.3488 | 5.98 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P173 | L86_StereoVisionFusionComputer | -2.77 | 3.3488 | 5.98 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P174 | L86_StereoVisionFusionComputer | -2.63 | 3.3488 | 5.98 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P175 | L86_StereoVisionFusionComputer | -2.49 | 3.3488 | 5.98 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P176 | L86_StereoVisionFusionComputer | -2.35 | 3.3488 | 5.98 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P177 | L86_StereoVisionFusionComputer | -2.21 | 3.3488 | 5.98 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P178 | L86_StereoVisionFusionComputer | -2.07 | 3.3488 | 5.98 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P179 | L86_StereoVisionFusionComputer | -1.93 | 3.3488 | 5.98 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P180 | L86_StereoVisionFusionComputer | -2.77 | 3.4568 | 5.98 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P181 | L86_StereoVisionFusionComputer | -2.63 | 3.4568 | 5.98 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P182 | L86_StereoVisionFusionComputer | -2.49 | 3.4568 | 5.98 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P183 | L86_StereoVisionFusionComputer | -2.35 | 3.4568 | 5.98 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P184 | L86_StereoVisionFusionComputer | -2.21 | 3.4568 | 5.98 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P185 | L86_RadiatorServiceSequencingPLC | -1.52 | 3.3488 | 5.98 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P186 | L86_RadiatorServiceSequencingPLC | -1.38 | 3.3488 | 5.98 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P187 | L86_RadiatorServiceSequencingPLC | -1.24 | 3.3488 | 5.98 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P188 | L86_RadiatorServiceSequencingPLC | -1.1 | 3.3488 | 5.98 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P189 | L86_RadiatorServiceSequencingPLC | -0.96 | 3.3488 | 5.98 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P190 | L86_RadiatorServiceSequencingPLC | -0.82 | 3.3488 | 5.98 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P191 | L86_RadiatorServiceSequencingPLC | -0.68 | 3.3488 | 5.98 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P192 | L86_RadiatorServiceSequencingPLC | -1.52 | 3.4568 | 5.98 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P193 | L86_RadiatorServiceSequencingPLC | -1.38 | 3.4568 | 5.98 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P194 | L86_RadiatorServiceSequencingPLC | -1.24 | 3.4568 | 5.98 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P195 | L86_RadiatorServiceSequencingPLC | -1.1 | 3.4568 | 5.98 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P196 | L86_RadiatorServiceSequencingPLC | -0.96 | 3.4568 | 5.98 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P197 | L86_RadiatorServiceSequencingPLC | -0.82 | 3.4568 | 5.98 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P198 | L86_RadiatorServiceSequencingPLC | -0.68 | 3.4568 | 5.98 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P199 | L86_RadiatorServiceSequencingPLC | -1.52 | 3.5648 | 5.98 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P200 | L86_RadiatorServiceSequencingPLC | -1.38 | 3.5648 | 5.98 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P201 | L86_RadiatorServiceSequencingPLC | -1.24 | 3.5648 | 5.98 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P202 | L86_RadiatorServiceSequencingPLC | -1.1 | 3.5648 | 5.98 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P203 | L86_RadiatorServiceSequencingPLC | -0.96 | 3.5648 | 5.98 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P204 | L86_RadiatorServiceSequencingPLC | -0.82 | 3.5648 | 5.98 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P205 | L86_RadiatorServiceSequencingPLC | -0.68 | 3.5648 | 5.98 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P206 | L86_HolographicOrbitalRoboticsSCADA | -0.42 | 8.6488 | -5.87 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P207 | L86_HolographicOrbitalRoboticsSCADA | -0.28 | 8.6488 | -5.87 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P208 | L86_HolographicOrbitalRoboticsSCADA | -0.14 | 8.6488 | -5.87 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P209 | L86_HolographicOrbitalRoboticsSCADA | 0.0 | 8.6488 | -5.87 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P210 | L86_HolographicOrbitalRoboticsSCADA | 0.14 | 8.6488 | -5.87 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P211 | L86_HolographicOrbitalRoboticsSCADA | 0.28 | 8.6488 | -5.87 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P212 | L86_HolographicOrbitalRoboticsSCADA | 0.42 | 8.6488 | -5.87 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P213 | L86_HolographicOrbitalRoboticsSCADA | -0.42 | 8.7568 | -5.87 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P214 | L86_HolographicOrbitalRoboticsSCADA | -0.28 | 8.7568 | -5.87 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P215 | L86_HolographicOrbitalRoboticsSCADA | -0.14 | 8.7568 | -5.87 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P216 | L86_HolographicOrbitalRoboticsSCADA | 0.0 | 8.7568 | -5.87 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P217 | L86_HolographicOrbitalRoboticsSCADA | 0.14 | 8.7568 | -5.87 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P218 | L86_HolographicOrbitalRoboticsSCADA | 0.28 | 8.7568 | -5.87 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P219 | L86_HolographicOrbitalRoboticsSCADA | 0.42 | 8.7568 | -5.87 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P220 | L86_HolographicOrbitalRoboticsSCADA | -0.42 | 8.8648 | -5.87 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P221 | L86_HolographicOrbitalRoboticsSCADA | -0.28 | 8.8648 | -5.87 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P222 | L86_W129CriticalFeedbackPatchStation | -0.27 | 3.3488 | 5.98 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P223 | L86_W129CriticalFeedbackPatchStation | -0.13 | 3.3488 | 5.98 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P224 | L86_W129CriticalFeedbackPatchStation | 0.01 | 3.3488 | 5.98 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P225 | L86_HardwareInterlockDiagnosticRack | 0.98 | 3.3488 | 5.98 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P226 | L86_HardwareInterlockDiagnosticRack | 1.12 | 3.3488 | 5.98 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P227 | L86_HardwareInterlockDiagnosticRack | 1.26 | 3.3488 | 5.98 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P228 | L86_ServicingMissionCertificationBeacon | -0.42 | 10.1488 | -5.86 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P229 | L86_ServicingMissionCertificationBeacon | -0.28 | 10.1488 | -5.86 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |
| P230 | L86_RoboticServicingEventRecorder | 2.23 | 3.3488 | 5.98 | DECORATIVE_3D_SOCKET_NOT_SPARKTERMINAL |



## ANNEX E — ALL 304 RECONCILED LOGICAL ENDPOINT RECORDS

Source: `Terminal_Logical_Port_Reconciliation_304.csv`. There are exactly two proposed independent logical contacts per planned player wire. Each row retains owner, original art marker, explicit functional ID, `SignalKind` and direction. Use this table as an allocation plan, not proof any `SparkTerminal` is already in the ZIP.

| Wire | Endpoint | Model | Art anchor | FunctionalPortID | SignalKind | Direction | Critical |
|---|---|---|---|---|---|---|---|
| W001 | Source | `L86_Protected5VControlSupply` | P135 | `W001_OUT_POWER_CONTROL` | POWER_CONTROL | OUTPUT | NO |
| W001 | Target | `L86_RoboticContactSafetyPLC` | P114 | `W001_IN_POWER_CONTROL` | POWER_CONTROL | INPUT | NO |
| W002 | Source | `L86_Protected5VControlSupply` | P139 | `W002_OUT_POWER_CONTROL` | POWER_CONTROL | OUTPUT | NO |
| W002 | Target | `L86_RadiatorServiceSequencingPLC` | P187 | `W002_IN_POWER_CONTROL` | POWER_CONTROL | INPUT | NO |
| W003 | Source | `L86_Protected5VControlSupply` | P134 | `W003_OUT_POWER_CONTROL` | POWER_CONTROL | OUTPUT | NO |
| W003 | Target | `L86_SevenAxisJointServoController` | P150 | `W003_IN_POWER_CONTROL` | POWER_CONTROL | INPUT | NO |
| W004 | Source | `L86_Protected5VControlSupply` | P138 | `W004_OUT_POWER_CONTROL` | POWER_CONTROL | OUTPUT | NO |
| W004 | Target | `L86_StereoVisionFusionComputer` | P177 | `W004_IN_POWER_CONTROL` | POWER_CONTROL | INPUT | NO |
| W005 | Source | `L86_Protected5VControlSupply` | P142 | `W005_OUT_POWER_CONTROL` | POWER_CONTROL | OUTPUT | NO |
| W005 | Target | `L86_ContactForceSignalConditioner` | P171 | `W005_IN_POWER_CONTROL` | POWER_CONTROL | INPUT | NO |
| W006 | Source | `L86_Protected5VControlSupply` | P137 | `W006_OUT_POWER_CONTROL` | POWER_CONTROL | OUTPUT | NO |
| W006 | Target | `L86_JointServoDriveRack` | P015 | `W006_IN_POWER_CONTROL` | POWER_CONTROL | INPUT | NO |
| W007 | Source | `L86_Protected5VControlSupply` | P141 | `W007_OUT_POWER_CONTROL` | POWER_CONTROL | OUTPUT | NO |
| W007 | Target | `L86_HolographicOrbitalRoboticsSCADA` | P213 | `W007_IN_POWER_CONTROL` | POWER_CONTROL | INPUT | NO |
| W008 | Source | `L86_Protected5VControlSupply` | P136 | `W008_OUT_POWER_CONTROL` | POWER_CONTROL | OUTPUT | NO |
| W008 | Target | `L86_IndependentContactSafetyRelay` | P146 | `W008_IN_POWER_CONTROL` | POWER_CONTROL | INPUT | NO |
| W009 | Source | `L86_Protected5VControlSupply` | P140 | `W009_OUT_POWER_CONTROL` | POWER_CONTROL | OUTPUT | NO |
| W009 | Target | `L86_RadiatorLeakTestController` | P099 | `W009_IN_POWER_CONTROL` | POWER_CONTROL | INPUT | NO |
| W010 | Source | `L86_Protected5VControlSupply` | P135 | `W010_OUT_POWER_CONTROL` | POWER_CONTROL | OUTPUT | NO |
| W010 | Target | `L86_RobotJointPathPlanningComputer` | P054 | `W010_IN_POWER_CONTROL` | POWER_CONTROL | INPUT | NO |
| W011 | Source | `L86_Protected5VControlSupply` | P139 | `W011_OUT_POWER_CONTROL` | POWER_CONTROL | OUTPUT | NO |
| W011 | Target | `L86_HardwareInterlockDiagnosticRack` | P227 | `W011_IN_POWER_CONTROL` | POWER_CONTROL | INPUT | NO |
| W012 | Source | `L86_Protected5VControlSupply` | P134 | `W012_OUT_POWER_CONTROL` | POWER_CONTROL | OUTPUT | NO |
| W012 | Target | `L86_W129CriticalFeedbackPatchStation` | P222 | `W012_IN_POWER_CONTROL` | POWER_CONTROL | INPUT | NO |
| W013 | Source | `L86_SevenAxisJointServoController` | P163 | `W013_OUT_ACTUATOR_COMMAND` | ACTUATOR_COMMAND | OUTPUT | NO |
| W013 | Target | `L86_RobotJoint1BaseYaw` | P002 | `W013_IN_ACTUATOR_COMMAND` | ACTUATOR_COMMAND | INPUT | NO |
| W014 | Source | `L86_RobotJoint1BaseYaw` | P002 | `W014_OUT_ANALOG_FEEDBACK` | ANALOG_FEEDBACK | OUTPUT | NO |
| W014 | Target | `L86_JointEncoderFeedbackArray` | P021 | `W014_IN_ANALOG_FEEDBACK` | ANALOG_FEEDBACK | INPUT | NO |
| W015 | Source | `L86_JointTorqueCurrentSensorRack` | P028 | `W015_OUT_ANALOG_FEEDBACK` | ANALOG_FEEDBACK | OUTPUT | NO |
| W015 | Target | `L86_SevenAxisJointServoController` | P149 | `W015_IN_ANALOG_FEEDBACK` | ANALOG_FEEDBACK | INPUT | NO |
| W016 | Source | `L86_SevenAxisJointServoController` | P153 | `W016_OUT_ACTUATOR_COMMAND` | ACTUATOR_COMMAND | OUTPUT | NO |
| W016 | Target | `L86_RobotJoint2Shoulder` | P003 | `W016_IN_ACTUATOR_COMMAND` | ACTUATOR_COMMAND | INPUT | NO |
| W017 | Source | `L86_RobotJoint2Shoulder` | P003 | `W017_OUT_ANALOG_FEEDBACK` | ANALOG_FEEDBACK | OUTPUT | NO |
| W017 | Target | `L86_JointEncoderFeedbackArray` | P018 | `W017_IN_ANALOG_FEEDBACK` | ANALOG_FEEDBACK | INPUT | NO |
| W018 | Source | `L86_JointTorqueCurrentSensorRack` | P028 | `W018_OUT_ANALOG_FEEDBACK` | ANALOG_FEEDBACK | OUTPUT | NO |
| W018 | Target | `L86_SevenAxisJointServoController` | P158 | `W018_IN_ANALOG_FEEDBACK` | ANALOG_FEEDBACK | INPUT | NO |
| W019 | Source | `L86_SevenAxisJointServoController` | P162 | `W019_OUT_ACTUATOR_COMMAND` | ACTUATOR_COMMAND | OUTPUT | NO |
| W019 | Target | `L86_RobotJoint3UpperArm` | P006 | `W019_IN_ACTUATOR_COMMAND` | ACTUATOR_COMMAND | INPUT | NO |
| W020 | Source | `L86_RobotJoint3UpperArm` | P006 | `W020_OUT_ANALOG_FEEDBACK` | ANALOG_FEEDBACK | OUTPUT | NO |
| W020 | Target | `L86_JointEncoderFeedbackArray` | P024 | `W020_IN_ANALOG_FEEDBACK` | ANALOG_FEEDBACK | INPUT | NO |
| W021 | Source | `L86_JointTorqueCurrentSensorRack` | P028 | `W021_OUT_ANALOG_FEEDBACK` | ANALOG_FEEDBACK | OUTPUT | NO |
| W021 | Target | `L86_SevenAxisJointServoController` | P148 | `W021_IN_ANALOG_FEEDBACK` | ANALOG_FEEDBACK | INPUT | NO |
| W022 | Source | `L86_SevenAxisJointServoController` | P152 | `W022_OUT_ACTUATOR_COMMAND` | ACTUATOR_COMMAND | OUTPUT | NO |
| W022 | Target | `L86_RobotJoint4Elbow` | P007 | `W022_IN_ACTUATOR_COMMAND` | ACTUATOR_COMMAND | INPUT | NO |
| W023 | Source | `L86_RobotJoint4Elbow` | P007 | `W023_OUT_ANALOG_FEEDBACK` | ANALOG_FEEDBACK | OUTPUT | NO |
| W023 | Target | `L86_JointEncoderFeedbackArray` | P021 | `W023_IN_ANALOG_FEEDBACK` | ANALOG_FEEDBACK | INPUT | NO |
| W024 | Source | `L86_JointTorqueCurrentSensorRack` | P028 | `W024_OUT_ANALOG_FEEDBACK` | ANALOG_FEEDBACK | OUTPUT | NO |
| W024 | Target | `L86_SevenAxisJointServoController` | P157 | `W024_IN_ANALOG_FEEDBACK` | ANALOG_FEEDBACK | INPUT | NO |
| W025 | Source | `L86_SevenAxisJointServoController` | P161 | `W025_OUT_ACTUATOR_COMMAND` | ACTUATOR_COMMAND | OUTPUT | NO |
| W025 | Target | `L86_RobotJoint5Forearm` | P010 | `W025_IN_ACTUATOR_COMMAND` | ACTUATOR_COMMAND | INPUT | NO |
| W026 | Source | `L86_RobotJoint5Forearm` | P010 | `W026_OUT_ANALOG_FEEDBACK` | ANALOG_FEEDBACK | OUTPUT | NO |
| W026 | Target | `L86_JointEncoderFeedbackArray` | P018 | `W026_IN_ANALOG_FEEDBACK` | ANALOG_FEEDBACK | INPUT | NO |
| W027 | Source | `L86_JointTorqueCurrentSensorRack` | P028 | `W027_OUT_ANALOG_FEEDBACK` | ANALOG_FEEDBACK | OUTPUT | NO |
| W027 | Target | `L86_SevenAxisJointServoController` | P147 | `W027_IN_ANALOG_FEEDBACK` | ANALOG_FEEDBACK | INPUT | NO |
| W028 | Source | `L86_SevenAxisJointServoController` | P151 | `W028_OUT_ACTUATOR_COMMAND` | ACTUATOR_COMMAND | OUTPUT | NO |
| W028 | Target | `L86_RobotJoint6WristPitch` | P011 | `W028_IN_ACTUATOR_COMMAND` | ACTUATOR_COMMAND | INPUT | NO |
| W029 | Source | `L86_RobotJoint6WristPitch` | P011 | `W029_OUT_ANALOG_FEEDBACK` | ANALOG_FEEDBACK | OUTPUT | NO |
| W029 | Target | `L86_JointEncoderFeedbackArray` | P024 | `W029_IN_ANALOG_FEEDBACK` | ANALOG_FEEDBACK | INPUT | NO |
| W030 | Source | `L86_JointTorqueCurrentSensorRack` | P028 | `W030_OUT_ANALOG_FEEDBACK` | ANALOG_FEEDBACK | OUTPUT | NO |
| W030 | Target | `L86_SevenAxisJointServoController` | P156 | `W030_IN_ANALOG_FEEDBACK` | ANALOG_FEEDBACK | INPUT | NO |
| W031 | Source | `L86_SevenAxisJointServoController` | P160 | `W031_OUT_ACTUATOR_COMMAND` | ACTUATOR_COMMAND | OUTPUT | NO |
| W031 | Target | `L86_RobotJoint7WristRoll` | P014 | `W031_IN_ACTUATOR_COMMAND` | ACTUATOR_COMMAND | INPUT | NO |
| W032 | Source | `L86_RobotJoint7WristRoll` | P014 | `W032_OUT_ANALOG_FEEDBACK` | ANALOG_FEEDBACK | OUTPUT | NO |
| W032 | Target | `L86_JointEncoderFeedbackArray` | P021 | `W032_IN_ANALOG_FEEDBACK` | ANALOG_FEEDBACK | INPUT | NO |
| W033 | Source | `L86_JointTorqueCurrentSensorRack` | P028 | `W033_OUT_ANALOG_FEEDBACK` | ANALOG_FEEDBACK | OUTPUT | NO |
| W033 | Target | `L86_SevenAxisJointServoController` | P165 | `W033_IN_ANALOG_FEEDBACK` | ANALOG_FEEDBACK | INPUT | NO |
| W034 | Source | `L86_JointEncoderFeedbackArray` | P017 | `W034_OUT_DATA_STREAM` | DATA_STREAM | OUTPUT | NO |
| W034 | Target | `L86_SevenAxisJointServoController` | P150 | `W034_IN_DATA_STREAM` | DATA_STREAM | INPUT | NO |
| W035 | Source | `L86_SevenAxisJointServoController` | P154 | `W035_OUT_SAFETY_CONTROL` | SAFETY_CONTROL | OUTPUT | NO |
| W035 | Target | `L86_RobotBrakeAndTorqueLimiter` | P032 | `W035_IN_SAFETY_CONTROL` | SAFETY_CONTROL | INPUT | NO |
| W036 | Source | `L86_RobotBrakeAndTorqueLimiter` | P032 | `W036_OUT_DIGITAL_STATUS` | DIGITAL_STATUS | OUTPUT | NO |
| W036 | Target | `L86_RoboticContactSafetyPLC` | P132 | `W036_IN_DIGITAL_STATUS` | DIGITAL_STATUS | INPUT | NO |
| W037 | Source | `L86_StereoMachineVisionCameraLeft` | P036 | `W037_OUT_DATA_STREAM` | DATA_STREAM | OUTPUT | NO |
| W037 | Target | `L86_StereoVisionFusionComputer` | P177 | `W037_IN_DATA_STREAM` | DATA_STREAM | INPUT | NO |
| W038 | Source | `L86_StereoMachineVisionCameraRight` | P037 | `W038_OUT_DATA_STREAM` | DATA_STREAM | OUTPUT | NO |
| W038 | Target | `L86_StereoVisionFusionComputer` | P181 | `W038_IN_DATA_STREAM` | DATA_STREAM | INPUT | NO |
| W039 | Source | `L86_StructuredLightDepthScanner` | P040 | `W039_OUT_DATA_STREAM` | DATA_STREAM | OUTPUT | NO |
| W039 | Target | `L86_StereoVisionFusionComputer` | P173 | `W039_IN_DATA_STREAM` | DATA_STREAM | INPUT | NO |
| W040 | Source | `L86_OpticalFiducialTargetArray` | P041 | `W040_OUT_DATA_STREAM` | DATA_STREAM | OUTPUT | NO |
| W040 | Target | `L86_StereoVisionFusionComputer` | P177 | `W040_IN_DATA_STREAM` | DATA_STREAM | INPUT | NO |
| W041 | Source | `L86_PrecisionVisionCalibrationBoard` | P042 | `W041_OUT_DATA_STREAM` | DATA_STREAM | OUTPUT | NO |
| W041 | Target | `L86_StereoVisionFusionComputer` | P181 | `W041_IN_DATA_STREAM` | DATA_STREAM | INPUT | NO |
| W042 | Source | `L86_LaserAlignmentMetrologyUnit` | P043 | `W042_OUT_ANALOG_FEEDBACK` | ANALOG_FEEDBACK | OUTPUT | NO |
| W042 | Target | `L86_StereoVisionFusionComputer` | P173 | `W042_IN_ANALOG_FEEDBACK` | ANALOG_FEEDBACK | INPUT | NO |
| W043 | Source | `L86_StereoVisionFusionComputer` | P177 | `W043_OUT_DIGITAL_CONTROL` | DIGITAL_CONTROL | OUTPUT | NO |
| W043 | Target | `L86_StereoMachineVisionCameraLeft` | P035 | `W043_IN_DIGITAL_CONTROL` | DIGITAL_CONTROL | INPUT | NO |
| W044 | Source | `L86_StereoVisionFusionComputer` | P181 | `W044_OUT_DIGITAL_CONTROL` | DIGITAL_CONTROL | OUTPUT | NO |
| W044 | Target | `L86_StereoMachineVisionCameraRight` | P038 | `W044_IN_DIGITAL_CONTROL` | DIGITAL_CONTROL | INPUT | NO |
| W045 | Source | `L86_StereoVisionFusionComputer` | P173 | `W045_OUT_DIGITAL_CONTROL` | DIGITAL_CONTROL | OUTPUT | NO |
| W045 | Target | `L86_StructuredLightDepthScanner` | P039 | `W045_IN_DIGITAL_CONTROL` | DIGITAL_CONTROL | INPUT | NO |
| W046 | Source | `L86_StereoVisionFusionComputer` | P177 | `W046_OUT_DIGITAL_CONTROL` | DIGITAL_CONTROL | OUTPUT | NO |
| W046 | Target | `L86_LaserAlignmentMetrologyUnit` | P044 | `W046_IN_DIGITAL_CONTROL` | DIGITAL_CONTROL | INPUT | NO |
| W047 | Source | `L86_StereoVisionFusionComputer` | P181 | `W047_OUT_DATA_STREAM` | DATA_STREAM | OUTPUT | NO |
| W047 | Target | `L86_RobotJointPathPlanningComputer` | P054 | `W047_IN_DATA_STREAM` | DATA_STREAM | INPUT | NO |
| W048 | Source | `L86_StereoVisionFusionComputer` | P173 | `W048_OUT_DATA_STREAM` | DATA_STREAM | OUTPUT | NO |
| W048 | Target | `L86_InverseKinematicsSolverDisplay` | P059 | `W048_IN_DATA_STREAM` | DATA_STREAM | INPUT | NO |
| W049 | Source | `L86_CollisionProximityScanner` | P062 | `W049_OUT_ANALOG_FEEDBACK` | ANALOG_FEEDBACK | OUTPUT | NO |
| W049 | Target | `L86_RobotTrajectorySafetySupervisor` | P064 | `W049_IN_ANALOG_FEEDBACK` | ANALOG_FEEDBACK | INPUT | NO |
| W050 | Source | `L86_RobotJointPathPlanningComputer` | P055 | `W050_OUT_DATA_STREAM` | DATA_STREAM | OUTPUT | NO |
| W050 | Target | `L86_RobotTrajectorySafetySupervisor` | P064 | `W050_IN_DATA_STREAM` | DATA_STREAM | INPUT | NO |
| W051 | Source | `L86_RobotTrajectorySafetySupervisor` | P064 | `W051_OUT_SAFETY_CONTROL` | SAFETY_CONTROL | OUTPUT | NO |
| W051 | Target | `L86_SevenAxisJointServoController` | P154 | `W051_IN_SAFETY_CONTROL` | SAFETY_CONTROL | INPUT | NO |
| W052 | Source | `L86_InverseKinematicsSolverDisplay` | P060 | `W052_OUT_DATA_STREAM` | DATA_STREAM | OUTPUT | NO |
| W052 | Target | `L86_RobotJointPathPlanningComputer` | P055 | `W052_IN_DATA_STREAM` | DATA_STREAM | INPUT | NO |
| W053 | Source | `L86_RobotJointPathPlanningComputer` | P054 | `W053_OUT_DATA_STREAM` | DATA_STREAM | OUTPUT | NO |
| W053 | Target | `L86_SevenAxisReachEnvelopeHologram` | P033 | `W053_IN_DATA_STREAM` | DATA_STREAM | INPUT | NO |
| W054 | Source | `L86_RoboticHomingCalibrationFixture` | P034 | `W054_OUT_DIGITAL_STATUS` | DIGITAL_STATUS | OUTPUT | NO |
| W054 | Target | `L86_SevenAxisJointServoController` | P160 | `W054_IN_DIGITAL_STATUS` | DIGITAL_STATUS | INPUT | NO |
| W055 | Source | `L86_StereoVisionFusionComputer` | P183 | `W055_OUT_DATA_STREAM` | DATA_STREAM | OUTPUT | NO |
| W055 | Target | `L86_HolographicOrbitalRoboticsSCADA` | P216 | `W055_IN_DATA_STREAM` | DATA_STREAM | INPUT | NO |
| W056 | Source | `L86_CollisionProximityScanner` | P062 | `W056_OUT_DATA_STREAM` | DATA_STREAM | OUTPUT | NO |
| W056 | Target | `L86_HolographicOrbitalRoboticsSCADA` | P220 | `W056_IN_DATA_STREAM` | DATA_STREAM | INPUT | NO |
| W057 | Source | `L86_SixAxisWristForceTorqueSensor` | P045 | `W057_OUT_ANALOG_FEEDBACK` | ANALOG_FEEDBACK | OUTPUT | NO |
| W057 | Target | `L86_ContactForceSignalConditioner` | P170 | `W057_IN_ANALOG_FEEDBACK` | ANALOG_FEEDBACK | INPUT | NO |
| W058 | Source | `L86_ContactForceSignalConditioner` | P167 | `W058_OUT_ANALOG_FEEDBACK` | ANALOG_FEEDBACK | OUTPUT | NO |
| W058 | Target | `L86_CompliantInsertionController` | P051 | `W058_IN_ANALOG_FEEDBACK` | ANALOG_FEEDBACK | INPUT | NO |
| W059 | Source | `L86_CompliantInsertionController` | P051 | `W059_OUT_ACTUATOR_COMMAND` | ACTUATOR_COMMAND | OUTPUT | NO |
| W059 | Target | `L86_SevenAxisJointServoController` | P149 | `W059_IN_ACTUATOR_COMMAND` | ACTUATOR_COMMAND | INPUT | NO |
| W060 | Source | `L86_ForceTorqueCalibrationFixture` | P048 | `W060_OUT_ANALOG_FEEDBACK` | ANALOG_FEEDBACK | OUTPUT | NO |
| W060 | Target | `L86_ContactForceSignalConditioner` | P172 | `W060_IN_ANALOG_FEEDBACK` | ANALOG_FEEDBACK | INPUT | NO |
| W061 | Source | `L86_ContactForceSignalConditioner` | P169 | `W061_OUT_DATA_STREAM` | DATA_STREAM | OUTPUT | NO |
| W061 | Target | `L86_SixAxisContactForceHologram` | P053 | `W061_IN_DATA_STREAM` | DATA_STREAM | INPUT | NO |
| W062 | Source | `L86_ContactForceSignalConditioner` | P166 | `W062_OUT_SAFETY_CONTROL` | SAFETY_CONTROL | OUTPUT | NO |
| W062 | Target | `L86_RobotTrajectorySafetySupervisor` | P066 | `W062_IN_SAFETY_CONTROL` | SAFETY_CONTROL | INPUT | NO |
| W063 | Source | `L86_IndependentContactSafetyRelay` | P144 | `W063_OUT_SAFETY_CONTROL` | SAFETY_CONTROL | OUTPUT | NO |
| W063 | Target | `L86_CompliantInsertionController` | P050 | `W063_IN_SAFETY_CONTROL` | SAFETY_CONTROL | INPUT | NO |
| W064 | Source | `L86_SevenAxisJointServoController` | P157 | `W064_OUT_ANALOG_FEEDBACK` | ANALOG_FEEDBACK | OUTPUT | NO |
| W064 | Target | `L86_CompliantInsertionController` | P050 | `W064_IN_ANALOG_FEEDBACK` | ANALOG_FEEDBACK | INPUT | NO |
| W065 | Source | `L86_CompliantInsertionController` | P050 | `W065_OUT_DIGITAL_STATUS` | DIGITAL_STATUS | OUTPUT | NO |
| W065 | Target | `L86_RadiatorServiceSequencingPLC` | P190 | `W065_IN_DIGITAL_STATUS` | DIGITAL_STATUS | INPUT | NO |
| W066 | Source | `L86_FlexiblePayloadModalSensor` | P067 | `W066_OUT_ANALOG_FEEDBACK` | ANALOG_FEEDBACK | OUTPUT | NO |
| W066 | Target | `L86_ActiveVibrationDampingController` | P069 | `W066_IN_ANALOG_FEEDBACK` | ANALOG_FEEDBACK | INPUT | NO |
| W067 | Source | `L86_ActiveVibrationDampingController` | P070 | `W067_OUT_ACTUATOR_COMMAND` | ACTUATOR_COMMAND | OUTPUT | NO |
| W067 | Target | `L86_RobotJointPathPlanningComputer` | P056 | `W067_IN_ACTUATOR_COMMAND` | ACTUATOR_COMMAND | INPUT | NO |
| W068 | Source | `L86_PayloadVibrationFaultInjector` | P110 | `W068_OUT_SAFETY_CONTROL` | SAFETY_CONTROL | OUTPUT | NO |
| W068 | Target | `L86_FlexiblePayloadModalSensor` | P068 | `W068_IN_SAFETY_CONTROL` | SAFETY_CONTROL | INPUT | NO |
| W069 | Source | `L86_JointEncoderDriftFaultInjector` | P111 | `W069_OUT_SAFETY_CONTROL` | SAFETY_CONTROL | OUTPUT | NO |
| W069 | Target | `L86_JointEncoderFeedbackArray` | P019 | `W069_IN_SAFETY_CONTROL` | SAFETY_CONTROL | INPUT | NO |
| W070 | Source | `L86_JointEncoderFeedbackArray` | P023 | `W070_OUT_ANALOG_FEEDBACK` | ANALOG_FEEDBACK | OUTPUT | NO |
| W070 | Target | `L86_RobotTrajectorySafetySupervisor` | P065 | `W070_IN_ANALOG_FEEDBACK` | ANALOG_FEEDBACK | INPUT | NO |
| W071 | Source | `L86_DualJawGripperPressureSensorArray` | P088 | `W071_OUT_ANALOG_FEEDBACK` | ANALOG_FEEDBACK | OUTPUT | NO |
| W071 | Target | `L86_ContactForceSignalConditioner` | P171 | `W071_IN_ANALOG_FEEDBACK` | ANALOG_FEEDBACK | INPUT | NO |
| W072 | Source | `L86_ContactForceSignalConditioner` | P168 | `W072_OUT_DIGITAL_STATUS` | DIGITAL_STATUS | OUTPUT | NO |
| W072 | Target | `L86_RadiatorServiceSequencingPLC` | P200 | `W072_IN_DIGITAL_STATUS` | DIGITAL_STATUS | INPUT | NO |
| W073 | Source | `L86_SixAxisWristForceTorqueSensor` | P046 | `W073_OUT_DATA_STREAM` | DATA_STREAM | OUTPUT | NO |
| W073 | Target | `L86_HolographicOrbitalRoboticsSCADA` | P208 | `W073_IN_DATA_STREAM` | DATA_STREAM | INPUT | NO |
| W074 | Source | `L86_FlexiblePayloadModalSensor` | P067 | `W074_OUT_ANALOG_FEEDBACK` | ANALOG_FEEDBACK | OUTPUT | NO |
| W074 | Target | `L86_HolographicOrbitalRoboticsSCADA` | P212 | `W074_IN_ANALOG_FEEDBACK` | ANALOG_FEEDBACK | INPUT | NO |
| W075 | Source | `L86_ActiveVibrationDampingController` | P069 | `W075_OUT_ACTUATOR_COMMAND` | ACTUATOR_COMMAND | OUTPUT | NO |
| W075 | Target | `L86_SevenAxisJointServoController` | P152 | `W075_IN_ACTUATOR_COMMAND` | ACTUATOR_COMMAND | INPUT | NO |
| W076 | Source | `L86_IndependentContactSafetyRelay` | P145 | `W076_OUT_DIGITAL_STATUS` | DIGITAL_STATUS | OUTPUT | NO |
| W076 | Target | `L86_RoboticContactSafetyPLC` | P115 | `W076_IN_DIGITAL_STATUS` | DIGITAL_STATUS | INPUT | NO |
| W077 | Source | `L86_RadiatorServiceSequencingPLC` | P187 | `W077_OUT_ACTUATOR_COMMAND` | ACTUATOR_COMMAND | OUTPUT | NO |
| W077 | Target | `L86_AdaptiveRadiatorHandlingGripper` | P073 | `W077_IN_ACTUATOR_COMMAND` | ACTUATOR_COMMAND | INPUT | NO |
| W078 | Source | `L86_AdaptiveRadiatorHandlingGripper` | P073 | `W078_OUT_ANALOG_FEEDBACK` | ANALOG_FEEDBACK | OUTPUT | NO |
| W078 | Target | `L86_DualJawGripperPressureSensorArray` | P086 | `W078_IN_ANALOG_FEEDBACK` | ANALOG_FEEDBACK | INPUT | NO |
| W079 | Source | `L86_RadiatorServiceSequencingPLC` | P192 | `W079_OUT_ACTUATOR_COMMAND` | ACTUATOR_COMMAND | OUTPUT | NO |
| W079 | Target | `L86_AutomaticEndEffectorToolChanger` | P077 | `W079_IN_ACTUATOR_COMMAND` | ACTUATOR_COMMAND | INPUT | NO |
| W080 | Source | `L86_AutomaticEndEffectorToolChanger` | P077 | `W080_OUT_DIGITAL_STATUS` | DIGITAL_STATUS | OUTPUT | NO |
| W080 | Target | `L86_RadiatorServiceSequencingPLC` | P196 | `W080_IN_DIGITAL_STATUS` | DIGITAL_STATUS | INPUT | NO |
| W081 | Source | `L86_RadiatorServiceSequencingPLC` | P200 | `W081_OUT_ACTUATOR_COMMAND` | ACTUATOR_COMMAND | OUTPUT | NO |
| W081 | Target | `L86_PrecisionTorqueFasteningDriver` | P078 | `W081_IN_ACTUATOR_COMMAND` | ACTUATOR_COMMAND | INPUT | NO |
| W082 | Source | `L86_PrecisionTorqueFasteningDriver` | P079 | `W082_OUT_ANALOG_FEEDBACK` | ANALOG_FEEDBACK | OUTPUT | NO |
| W082 | Target | `L86_RadiatorServiceSequencingPLC` | P204 | `W082_IN_ANALOG_FEEDBACK` | ANALOG_FEEDBACK | INPUT | NO |
| W083 | Source | `L86_CaptiveFastenerAndBoltRack` | P081 | `W083_OUT_DIGITAL_STATUS` | DIGITAL_STATUS | OUTPUT | NO |
| W083 | Target | `L86_PrecisionTorqueFasteningDriver` | P080 | `W083_IN_DIGITAL_STATUS` | DIGITAL_STATUS | INPUT | NO |
| W084 | Source | `L86_RadiatorServiceSequencingPLC` | P188 | `W084_OUT_DIGITAL_CONTROL` | DIGITAL_CONTROL | OUTPUT | NO |
| W084 | Target | `L86_VacuumCompatibleInspectionProbe` | P082 | `W084_IN_DIGITAL_CONTROL` | DIGITAL_CONTROL | INPUT | NO |
| W085 | Source | `L86_VacuumCompatibleInspectionProbe` | P082 | `W085_OUT_DATA_STREAM` | DATA_STREAM | OUTPUT | NO |
| W085 | Target | `L86_StereoVisionFusionComputer` | P180 | `W085_IN_DATA_STREAM` | DATA_STREAM | INPUT | NO |
| W086 | Source | `L86_RoboticToolDockAndCalibrationNest` | P084 | `W086_OUT_DIGITAL_STATUS` | DIGITAL_STATUS | OUTPUT | NO |
| W086 | Target | `L86_AutomaticEndEffectorToolChanger` | P076 | `W086_IN_DIGITAL_STATUS` | DIGITAL_STATUS | INPUT | NO |
| W087 | Source | `L86_RadiatorGrappleHandleFixture` | P089 | `W087_OUT_ANALOG_FEEDBACK` | ANALOG_FEEDBACK | OUTPUT | NO |
| W087 | Target | `L86_AdaptiveRadiatorHandlingGripper` | P073 | `W087_IN_ANALOG_FEEDBACK` | ANALOG_FEEDBACK | INPUT | NO |
| W088 | Source | `L86_AdaptiveRadiatorHandlingGripper` | P073 | `W088_OUT_SAFETY_CONTROL` | SAFETY_CONTROL | OUTPUT | NO |
| W088 | Target | `L86_RoboticContactSafetyPLC` | P130 | `W088_IN_SAFETY_CONTROL` | SAFETY_CONTROL | INPUT | NO |
| W089 | Source | `L86_AsymmetricGripperFaultInjector` | P112 | `W089_OUT_SAFETY_CONTROL` | SAFETY_CONTROL | OUTPUT | NO |
| W089 | Target | `L86_DualJawGripperPressureSensorArray` | P088 | `W089_IN_SAFETY_CONTROL` | SAFETY_CONTROL | INPUT | NO |
| W090 | Source | `L86_DualJawGripperPressureSensorArray` | P088 | `W090_OUT_ANALOG_FEEDBACK` | ANALOG_FEEDBACK | OUTPUT | NO |
| W090 | Target | `L86_RadiatorServiceSequencingPLC` | P197 | `W090_IN_ANALOG_FEEDBACK` | ANALOG_FEEDBACK | INPUT | NO |
| W091 | Source | `L86_RadiatorFastenerActuatorDriver` | P103 | `W091_OUT_DIGITAL_STATUS` | DIGITAL_STATUS | OUTPUT | NO |
| W091 | Target | `L86_PrecisionTorqueFasteningDriver` | P079 | `W091_IN_DIGITAL_STATUS` | DIGITAL_STATUS | INPUT | NO |
| W092 | Source | `L86_RadiatorServiceSequencingPLC` | P202 | `W092_OUT_ACTUATOR_COMMAND` | ACTUATOR_COMMAND | OUTPUT | NO |
| W092 | Target | `L86_RadiatorFastenerActuatorDriver` | P103 | `W092_IN_ACTUATOR_COMMAND` | ACTUATOR_COMMAND | INPUT | NO |
| W093 | Source | `L86_FailedRadiatorDisposalCradle` | P109 | `W093_OUT_DIGITAL_STATUS` | DIGITAL_STATUS | OUTPUT | NO |
| W093 | Target | `L86_RadiatorServiceSequencingPLC` | P185 | `W093_IN_DIGITAL_STATUS` | DIGITAL_STATUS | INPUT | NO |
| W094 | Source | `L86_AdaptiveRadiatorHandlingGripper` | P074 | `W094_OUT_DATA_STREAM` | DATA_STREAM | OUTPUT | NO |
| W094 | Target | `L86_HolographicOrbitalRoboticsSCADA` | P219 | `W094_IN_DATA_STREAM` | DATA_STREAM | INPUT | NO |
| W095 | Source | `L86_RadiatorServiceSequencingPLC` | P190 | `W095_OUT_ACTUATOR_COMMAND` | ACTUATOR_COMMAND | OUTPUT | NO |
| W095 | Target | `L86_CoolantIsolationAndVentStation` | P102 | `W095_IN_ACTUATOR_COMMAND` | ACTUATOR_COMMAND | INPUT | NO |
| W096 | Source | `L86_CoolantIsolationAndVentStation` | P102 | `W096_OUT_ANALOG_FEEDBACK` | ANALOG_FEEDBACK | OUTPUT | NO |
| W096 | Target | `L86_RadiatorServiceSequencingPLC` | P194 | `W096_IN_ANALOG_FEEDBACK` | ANALOG_FEEDBACK | INPUT | NO |
| W097 | Source | `L86_RadiatorServiceSequencingPLC` | P198 | `W097_OUT_ACTUATOR_COMMAND` | ACTUATOR_COMMAND | OUTPUT | NO |
| W097 | Target | `L86_CoolantQuickDisconnectA` | P090 | `W097_IN_ACTUATOR_COMMAND` | ACTUATOR_COMMAND | INPUT | NO |
| W098 | Source | `L86_RadiatorServiceSequencingPLC` | P202 | `W098_OUT_ACTUATOR_COMMAND` | ACTUATOR_COMMAND | OUTPUT | NO |
| W098 | Target | `L86_CoolantQuickDisconnectB` | P091 | `W098_IN_ACTUATOR_COMMAND` | ACTUATOR_COMMAND | INPUT | NO |
| W099 | Source | `L86_CoolantQuickDisconnectA` | P090 | `W099_OUT_ANALOG_FEEDBACK` | ANALOG_FEEDBACK | OUTPUT | NO |
| W099 | Target | `L86_RadiatorServiceSequencingPLC` | P185 | `W099_IN_ANALOG_FEEDBACK` | ANALOG_FEEDBACK | INPUT | NO |
| W100 | Source | `L86_CoolantQuickDisconnectB` | P091 | `W100_OUT_ANALOG_FEEDBACK` | ANALOG_FEEDBACK | OUTPUT | NO |
| W100 | Target | `L86_RadiatorServiceSequencingPLC` | P189 | `W100_IN_ANALOG_FEEDBACK` | ANALOG_FEEDBACK | INPUT | NO |
| W101 | Source | `L86_RadiatorServiceSequencingPLC` | P193 | `W101_OUT_ACTUATOR_COMMAND` | ACTUATOR_COMMAND | OUTPUT | NO |
| W101 | Target | `L86_CoolantValveManifold` | P092 | `W101_IN_ACTUATOR_COMMAND` | ACTUATOR_COMMAND | INPUT | NO |
| W102 | Source | `L86_CoolantValveManifold` | P092 | `W102_OUT_ANALOG_FEEDBACK` | ANALOG_FEEDBACK | OUTPUT | NO |
| W102 | Target | `L86_RadiatorServiceSequencingPLC` | P197 | `W102_IN_ANALOG_FEEDBACK` | ANALOG_FEEDBACK | INPUT | NO |
| W103 | Source | `L86_CoolantPressureTransducer` | P093 | `W103_OUT_ANALOG_FEEDBACK` | ANALOG_FEEDBACK | OUTPUT | NO |
| W103 | Target | `L86_RadiatorLeakTestController` | P100 | `W103_IN_ANALOG_FEEDBACK` | ANALOG_FEEDBACK | INPUT | NO |
| W104 | Source | `L86_CoolantFlowMeter` | P094 | `W104_OUT_ANALOG_FEEDBACK` | ANALOG_FEEDBACK | OUTPUT | NO |
| W104 | Target | `L86_RadiatorLeakTestController` | P100 | `W104_IN_ANALOG_FEEDBACK` | ANALOG_FEEDBACK | INPUT | NO |
| W105 | Source | `L86_RadiatorTemperatureSensorArray` | P095 | `W105_OUT_ANALOG_FEEDBACK` | ANALOG_FEEDBACK | OUTPUT | NO |
| W105 | Target | `L86_RadiatorThermalPerformanceAnalyzer` | P106 | `W105_IN_ANALOG_FEEDBACK` | ANALOG_FEEDBACK | INPUT | NO |
| W106 | Source | `L86_StationAvionicsHeatLoadEmulator` | P096 | `W106_OUT_ANALOG_FEEDBACK` | ANALOG_FEEDBACK | OUTPUT | NO |
| W106 | Target | `L86_RadiatorThermalPerformanceAnalyzer` | P106 | `W106_IN_ANALOG_FEEDBACK` | ANALOG_FEEDBACK | INPUT | NO |
| W107 | Source | `L86_ThermalLoopPumpSimulator` | P097 | `W107_OUT_ANALOG_FEEDBACK` | ANALOG_FEEDBACK | OUTPUT | NO |
| W107 | Target | `L86_RadiatorThermalPerformanceAnalyzer` | P106 | `W107_IN_ANALOG_FEEDBACK` | ANALOG_FEEDBACK | INPUT | NO |
| W108 | Source | `L86_RadiatorLeakTestController` | P099 | `W108_OUT_DIGITAL_STATUS` | DIGITAL_STATUS | OUTPUT | NO |
| W108 | Target | `L86_RadiatorServiceSequencingPLC` | P185 | `W108_IN_DIGITAL_STATUS` | DIGITAL_STATUS | INPUT | NO |
| W109 | Source | `L86_RadiatorThermalPerformanceAnalyzer` | P107 | `W109_OUT_DATA_STREAM` | DATA_STREAM | OUTPUT | NO |
| W109 | Target | `L86_RadiatorServiceSequencingPLC` | P189 | `W109_IN_DATA_STREAM` | DATA_STREAM | INPUT | NO |
| W110 | Source | `L86_RadiatorServiceSequencingPLC` | P193 | `W110_OUT_ACTUATOR_COMMAND` | ACTUATOR_COMMAND | OUTPUT | NO |
| W110 | Target | `L86_ThermalLoopPumpSimulator` | P097 | `W110_IN_ACTUATOR_COMMAND` | ACTUATOR_COMMAND | INPUT | NO |
| W111 | Source | `L86_RadiatorThermalPerformanceAnalyzer` | P108 | `W111_OUT_DATA_STREAM` | DATA_STREAM | OUTPUT | NO |
| W111 | Target | `L86_StationThermalTrendHologram` | P104 | `W111_IN_DATA_STREAM` | DATA_STREAM | INPUT | NO |
| W112 | Source | `L86_RadiatorTemperatureSensorArray` | P095 | `W112_OUT_DATA_STREAM` | DATA_STREAM | OUTPUT | NO |
| W112 | Target | `L86_HolographicOrbitalRoboticsSCADA` | P208 | `W112_IN_DATA_STREAM` | DATA_STREAM | INPUT | NO |
| W113 | Source | `L86_ManipulatorEmergencyStopStation` | P133 | `W113_OUT_SAFETY_CONTROL` | SAFETY_CONTROL | OUTPUT | NO |
| W113 | Target | `L86_RoboticContactSafetyPLC` | P118 | `W113_IN_SAFETY_CONTROL` | SAFETY_CONTROL | INPUT | NO |
| W114 | Source | `L86_IndependentContactSafetyRelay` | P144 | `W114_OUT_SAFETY_CONTROL` | SAFETY_CONTROL | OUTPUT | NO |
| W114 | Target | `L86_RoboticContactSafetyPLC` | P122 | `W114_IN_SAFETY_CONTROL` | SAFETY_CONTROL | INPUT | NO |
| W115 | Source | `L86_CollisionProximityScanner` | P062 | `W115_OUT_SAFETY_CONTROL` | SAFETY_CONTROL | OUTPUT | NO |
| W115 | Target | `L86_RoboticContactSafetyPLC` | P126 | `W115_IN_SAFETY_CONTROL` | SAFETY_CONTROL | INPUT | NO |
| W116 | Source | `L86_RobotTrajectorySafetySupervisor` | P066 | `W116_OUT_SAFETY_CONTROL` | SAFETY_CONTROL | OUTPUT | NO |
| W116 | Target | `L86_RoboticContactSafetyPLC` | P130 | `W116_IN_SAFETY_CONTROL` | SAFETY_CONTROL | INPUT | NO |
| W117 | Source | `L86_JointEncoderFeedbackArray` | P019 | `W117_OUT_SAFETY_CONTROL` | SAFETY_CONTROL | OUTPUT | NO |
| W117 | Target | `L86_RoboticContactSafetyPLC` | P114 | `W117_IN_SAFETY_CONTROL` | SAFETY_CONTROL | INPUT | NO |
| W118 | Source | `L86_DualJawGripperPressureSensorArray` | P087 | `W118_OUT_SAFETY_CONTROL` | SAFETY_CONTROL | OUTPUT | NO |
| W118 | Target | `L86_RoboticContactSafetyPLC` | P118 | `W118_IN_SAFETY_CONTROL` | SAFETY_CONTROL | INPUT | NO |
| W119 | Source | `L86_RadiatorServiceSequencingPLC` | P205 | `W119_OUT_SAFETY_CONTROL` | SAFETY_CONTROL | OUTPUT | NO |
| W119 | Target | `L86_RoboticContactSafetyPLC` | P122 | `W119_IN_SAFETY_CONTROL` | SAFETY_CONTROL | INPUT | NO |
| W120 | Source | `L86_RoboticContactSafetyPLC` | P126 | `W120_OUT_SAFETY_CONTROL` | SAFETY_CONTROL | OUTPUT | NO |
| W120 | Target | `L86_SevenAxisJointServoController` | P162 | `W120_IN_SAFETY_CONTROL` | SAFETY_CONTROL | INPUT | NO |
| W121 | Source | `L86_RoboticContactSafetyPLC` | P130 | `W121_OUT_SAFETY_CONTROL` | SAFETY_CONTROL | OUTPUT | NO |
| W121 | Target | `L86_AdaptiveRadiatorHandlingGripper` | P072 | `W121_IN_SAFETY_CONTROL` | SAFETY_CONTROL | INPUT | NO |
| W122 | Source | `L86_RoboticContactSafetyPLC` | P114 | `W122_OUT_SAFETY_CONTROL` | SAFETY_CONTROL | OUTPUT | NO |
| W122 | Target | `L86_CompliantInsertionController` | P050 | `W122_IN_SAFETY_CONTROL` | SAFETY_CONTROL | INPUT | NO |
| W123 | Source | `L86_RoboticContactSafetyPLC` | P118 | `W123_OUT_SAFETY_CONTROL` | SAFETY_CONTROL | OUTPUT | NO |
| W123 | Target | `L86_RadiatorServiceSequencingPLC` | P191 | `W123_IN_SAFETY_CONTROL` | SAFETY_CONTROL | INPUT | NO |
| W124 | Source | `L86_JointEncoderDriftFaultInjector` | P111 | `W124_OUT_DIGITAL_STATUS` | DIGITAL_STATUS | OUTPUT | NO |
| W124 | Target | `L86_RoboticContactSafetyPLC` | P122 | `W124_IN_DIGITAL_STATUS` | DIGITAL_STATUS | INPUT | NO |
| W125 | Source | `L86_PayloadVibrationFaultInjector` | P110 | `W125_OUT_DIGITAL_STATUS` | DIGITAL_STATUS | OUTPUT | NO |
| W125 | Target | `L86_RoboticContactSafetyPLC` | P126 | `W125_IN_DIGITAL_STATUS` | DIGITAL_STATUS | INPUT | NO |
| W126 | Source | `L86_AsymmetricGripperFaultInjector` | P112 | `W126_OUT_DIGITAL_STATUS` | DIGITAL_STATUS | OUTPUT | NO |
| W126 | Target | `L86_RoboticContactSafetyPLC` | P130 | `W126_IN_DIGITAL_STATUS` | DIGITAL_STATUS | INPUT | NO |
| W127 | Source | `L86_RoboticContactSafetyPLC` | P114 | `W127_OUT_DIGITAL_STATUS` | DIGITAL_STATUS | OUTPUT | NO |
| W127 | Target | `L86_HardwareInterlockDiagnosticRack` | P226 | `W127_IN_DIGITAL_STATUS` | DIGITAL_STATUS | INPUT | NO |
| W128 | Source | `L86_ContactForceSignalConditioner` | P171 | `W128_OUT_ANALOG_FEEDBACK` | ANALOG_FEEDBACK | OUTPUT | NO |
| W128 | Target | `L86_W129CriticalFeedbackPatchStation` | P224 | `W128_IN_ANALOG_FEEDBACK` | ANALOG_FEEDBACK | INPUT | NO |
| W129 | Source | `L86_SixAxisWristForceTorqueSensor` | P045 | `W129_OUT_ANALOG_FEEDBACK` | ANALOG_FEEDBACK | OUTPUT | YES |
| W129 | Target | `L86_RoboticContactSafetyPLC` | P119 | `W129_IN_ANALOG_FEEDBACK` | ANALOG_FEEDBACK | INPUT | YES |
| W130 | Source | `L86_W129CriticalFeedbackPatchStation` | P223 | `W130_OUT_DIGITAL_STATUS` | DIGITAL_STATUS | OUTPUT | NO |
| W130 | Target | `L86_RoboticContactSafetyPLC` | P123 | `W130_IN_DIGITAL_STATUS` | DIGITAL_STATUS | INPUT | NO |
| W131 | Source | `L86_RoboticContactSafetyPLC` | P127 | `W131_OUT_DIGITAL_STATUS` | DIGITAL_STATUS | OUTPUT | NO |
| W131 | Target | `L86_HardwareInterlockDiagnosticRack` | P227 | `W131_IN_DIGITAL_STATUS` | DIGITAL_STATUS | INPUT | NO |
| W132 | Source | `L86_HardwareInterlockDiagnosticRack` | P225 | `W132_OUT_SAFETY_CONTROL` | SAFETY_CONTROL | OUTPUT | NO |
| W132 | Target | `L86_RoboticContactSafetyPLC` | P131 | `W132_IN_SAFETY_CONTROL` | SAFETY_CONTROL | INPUT | NO |
| W133 | Source | `L86_RoboticContactSafetyPLC` | P115 | `W133_OUT_SAFETY_CONTROL` | SAFETY_CONTROL | OUTPUT | NO |
| W133 | Target | `L86_RadiatorServiceSequencingPLC` | P204 | `W133_IN_SAFETY_CONTROL` | SAFETY_CONTROL | INPUT | NO |
| W134 | Source | `L86_RobotMotorPowerDistributionUnit` | P143 | `W134_OUT_SAFETY_CONTROL` | SAFETY_CONTROL | OUTPUT | NO |
| W134 | Target | `L86_SevenAxisJointServoController` | P160 | `W134_IN_SAFETY_CONTROL` | SAFETY_CONTROL | INPUT | NO |
| W135 | Source | `L86_RoboticContactSafetyPLC` | P120 | `W135_OUT_DIGITAL_STATUS` | DIGITAL_STATUS | OUTPUT | NO |
| W135 | Target | `L86_HolographicOrbitalRoboticsSCADA` | P218 | `W135_IN_DIGITAL_STATUS` | DIGITAL_STATUS | INPUT | NO |
| W136 | Source | `L86_RoboticContactSafetyPLC` | P124 | `W136_OUT_DIGITAL_STATUS` | DIGITAL_STATUS | OUTPUT | NO |
| W136 | Target | `L86_ServicingMissionCertificationBeacon` | P228 | `W136_IN_DIGITAL_STATUS` | DIGITAL_STATUS | INPUT | NO |
| W137 | Source | `L86_RadiatorServiceSequencingPLC` | P190 | `W137_OUT_DATA_STREAM` | DATA_STREAM | OUTPUT | NO |
| W137 | Target | `L86_HolographicOrbitalRoboticsSCADA` | P207 | `W137_IN_DATA_STREAM` | DATA_STREAM | INPUT | NO |
| W138 | Source | `L86_SevenAxisJointServoController` | P148 | `W138_OUT_DATA_STREAM` | DATA_STREAM | OUTPUT | NO |
| W138 | Target | `L86_HolographicOrbitalRoboticsSCADA` | P211 | `W138_IN_DATA_STREAM` | DATA_STREAM | INPUT | NO |
| W139 | Source | `L86_JointEncoderFeedbackArray` | P017 | `W139_OUT_DATA_STREAM` | DATA_STREAM | OUTPUT | NO |
| W139 | Target | `L86_HolographicOrbitalRoboticsSCADA` | P215 | `W139_IN_DATA_STREAM` | DATA_STREAM | INPUT | NO |
| W140 | Source | `L86_JointTorqueCurrentSensorRack` | P030 | `W140_OUT_DATA_STREAM` | DATA_STREAM | OUTPUT | NO |
| W140 | Target | `L86_HolographicOrbitalRoboticsSCADA` | P219 | `W140_IN_DATA_STREAM` | DATA_STREAM | INPUT | NO |
| W141 | Source | `L86_ContactForceSignalConditioner` | P166 | `W141_OUT_DATA_STREAM` | DATA_STREAM | OUTPUT | NO |
| W141 | Target | `L86_HolographicOrbitalRoboticsSCADA` | P207 | `W141_IN_DATA_STREAM` | DATA_STREAM | INPUT | NO |
| W142 | Source | `L86_StereoVisionFusionComputer` | P180 | `W142_OUT_DATA_STREAM` | DATA_STREAM | OUTPUT | NO |
| W142 | Target | `L86_HolographicOrbitalRoboticsSCADA` | P211 | `W142_IN_DATA_STREAM` | DATA_STREAM | INPUT | NO |
| W143 | Source | `L86_RadiatorThermalPerformanceAnalyzer` | P107 | `W143_OUT_DATA_STREAM` | DATA_STREAM | OUTPUT | NO |
| W143 | Target | `L86_HolographicOrbitalRoboticsSCADA` | P215 | `W143_IN_DATA_STREAM` | DATA_STREAM | INPUT | NO |
| W144 | Source | `L86_RadiatorLeakTestController` | P098 | `W144_OUT_DATA_STREAM` | DATA_STREAM | OUTPUT | NO |
| W144 | Target | `L86_HolographicOrbitalRoboticsSCADA` | P219 | `W144_IN_DATA_STREAM` | DATA_STREAM | INPUT | NO |
| W145 | Source | `L86_RobotJointPathPlanningComputer` | P057 | `W145_OUT_DATA_STREAM` | DATA_STREAM | OUTPUT | NO |
| W145 | Target | `L86_HolographicOrbitalRoboticsSCADA` | P207 | `W145_IN_DATA_STREAM` | DATA_STREAM | INPUT | NO |
| W146 | Source | `L86_ActiveVibrationDampingController` | P071 | `W146_OUT_DATA_STREAM` | DATA_STREAM | OUTPUT | NO |
| W146 | Target | `L86_HolographicOrbitalRoboticsSCADA` | P211 | `W146_IN_DATA_STREAM` | DATA_STREAM | INPUT | NO |
| W147 | Source | `L86_RoboticContactSafetyPLC` | P118 | `W147_OUT_DIGITAL_STATUS` | DIGITAL_STATUS | OUTPUT | NO |
| W147 | Target | `L86_HolographicOrbitalRoboticsSCADA` | P215 | `W147_IN_DIGITAL_STATUS` | DIGITAL_STATUS | INPUT | NO |
| W148 | Source | `L86_HolographicOrbitalRoboticsSCADA` | P219 | `W148_OUT_DATA_STREAM` | DATA_STREAM | OUTPUT | NO |
| W148 | Target | `L86_RoboticServicingEventRecorder` | P230 | `W148_IN_DATA_STREAM` | DATA_STREAM | INPUT | NO |
| W149 | Source | `L86_RoboticServicingEventRecorder` | P230 | `W149_OUT_DATA_STREAM` | DATA_STREAM | OUTPUT | NO |
| W149 | Target | `L86_RadiatorServiceSequencingPLC` | P205 | `W149_IN_DATA_STREAM` | DATA_STREAM | INPUT | NO |
| W150 | Source | `L86_RadiatorServiceSequencingPLC` | P188 | `W150_OUT_DIGITAL_STATUS` | DIGITAL_STATUS | OUTPUT | NO |
| W150 | Target | `L86_ServicingMissionCertificationBeacon` | P229 | `W150_IN_DIGITAL_STATUS` | DIGITAL_STATUS | INPUT | NO |
| W151 | Source | `L86_ServicingMissionCertificationBeacon` | P229 | `W151_OUT_DIGITAL_STATUS` | DIGITAL_STATUS | OUTPUT | NO |
| W151 | Target | `L86_HolographicOrbitalRoboticsSCADA` | P209 | `W151_IN_DIGITAL_STATUS` | DIGITAL_STATUS | INPUT | NO |
| W152 | Source | `L86_W129CriticalFeedbackPatchStation` | P224 | `W152_OUT_DIGITAL_STATUS` | DIGITAL_STATUS | OUTPUT | NO |
| W152 | Target | `L86_HolographicOrbitalRoboticsSCADA` | P213 | `W152_IN_DIGITAL_STATUS` | DIGITAL_STATUS | INPUT | NO |



## ANNEX F — REUSED ANCHOR CONFLICTS (ALL 88 SOURCE ROWS)

Source: `Terminal_Anchor_Signal_Conflict_Report.csv`. These report **candidate** multi-contact source-art anchor groups, not actual Unity shorts; split incompatible electrical contacts in the target implementation and document legitimate shared junctions.

| Anchor | Model | Contact count | Signal kinds | Different kinds? | Source status |
|---|---|---|---|---|---|
| P135 | `L86_Protected5VControlSupply` | 2 | POWER_CONTROL | False | REQUIRES_SEPARATE_FUNCTIONAL_SUBPORTS |
| P114 | `L86_RoboticContactSafetyPLC` | 4 | DIGITAL_STATUS;POWER_CONTROL;SAFETY_CONTROL | True | REQUIRES_SEPARATE_FUNCTIONAL_SUBPORTS |
| P139 | `L86_Protected5VControlSupply` | 2 | POWER_CONTROL | False | REQUIRES_SEPARATE_FUNCTIONAL_SUBPORTS |
| P187 | `L86_RadiatorServiceSequencingPLC` | 2 | ACTUATOR_COMMAND;POWER_CONTROL | True | REQUIRES_SEPARATE_FUNCTIONAL_SUBPORTS |
| P134 | `L86_Protected5VControlSupply` | 2 | POWER_CONTROL | False | REQUIRES_SEPARATE_FUNCTIONAL_SUBPORTS |
| P150 | `L86_SevenAxisJointServoController` | 2 | DATA_STREAM;POWER_CONTROL | True | REQUIRES_SEPARATE_FUNCTIONAL_SUBPORTS |
| P177 | `L86_StereoVisionFusionComputer` | 5 | DATA_STREAM;DIGITAL_CONTROL;POWER_CONTROL | True | REQUIRES_SEPARATE_FUNCTIONAL_SUBPORTS |
| P171 | `L86_ContactForceSignalConditioner` | 3 | ANALOG_FEEDBACK;POWER_CONTROL | True | REQUIRES_SEPARATE_FUNCTIONAL_SUBPORTS |
| P213 | `L86_HolographicOrbitalRoboticsSCADA` | 2 | DIGITAL_STATUS;POWER_CONTROL | True | REQUIRES_SEPARATE_FUNCTIONAL_SUBPORTS |
| P099 | `L86_RadiatorLeakTestController` | 2 | DIGITAL_STATUS;POWER_CONTROL | True | REQUIRES_SEPARATE_FUNCTIONAL_SUBPORTS |
| P054 | `L86_RobotJointPathPlanningComputer` | 3 | DATA_STREAM;POWER_CONTROL | True | REQUIRES_SEPARATE_FUNCTIONAL_SUBPORTS |
| P227 | `L86_HardwareInterlockDiagnosticRack` | 2 | DIGITAL_STATUS;POWER_CONTROL | True | REQUIRES_SEPARATE_FUNCTIONAL_SUBPORTS |
| P002 | `L86_RobotJoint1BaseYaw` | 2 | ACTUATOR_COMMAND;ANALOG_FEEDBACK | True | REQUIRES_SEPARATE_FUNCTIONAL_SUBPORTS |
| P021 | `L86_JointEncoderFeedbackArray` | 3 | ANALOG_FEEDBACK | False | REQUIRES_SEPARATE_FUNCTIONAL_SUBPORTS |
| P028 | `L86_JointTorqueCurrentSensorRack` | 7 | ANALOG_FEEDBACK | False | REQUIRES_SEPARATE_FUNCTIONAL_SUBPORTS |
| P149 | `L86_SevenAxisJointServoController` | 2 | ACTUATOR_COMMAND;ANALOG_FEEDBACK | True | REQUIRES_SEPARATE_FUNCTIONAL_SUBPORTS |
| P003 | `L86_RobotJoint2Shoulder` | 2 | ACTUATOR_COMMAND;ANALOG_FEEDBACK | True | REQUIRES_SEPARATE_FUNCTIONAL_SUBPORTS |
| P018 | `L86_JointEncoderFeedbackArray` | 2 | ANALOG_FEEDBACK | False | REQUIRES_SEPARATE_FUNCTIONAL_SUBPORTS |
| P162 | `L86_SevenAxisJointServoController` | 2 | ACTUATOR_COMMAND;SAFETY_CONTROL | True | REQUIRES_SEPARATE_FUNCTIONAL_SUBPORTS |
| P006 | `L86_RobotJoint3UpperArm` | 2 | ACTUATOR_COMMAND;ANALOG_FEEDBACK | True | REQUIRES_SEPARATE_FUNCTIONAL_SUBPORTS |
| P024 | `L86_JointEncoderFeedbackArray` | 2 | ANALOG_FEEDBACK | False | REQUIRES_SEPARATE_FUNCTIONAL_SUBPORTS |
| P148 | `L86_SevenAxisJointServoController` | 2 | ANALOG_FEEDBACK;DATA_STREAM | True | REQUIRES_SEPARATE_FUNCTIONAL_SUBPORTS |
| P152 | `L86_SevenAxisJointServoController` | 2 | ACTUATOR_COMMAND | False | REQUIRES_SEPARATE_FUNCTIONAL_SUBPORTS |
| P007 | `L86_RobotJoint4Elbow` | 2 | ACTUATOR_COMMAND;ANALOG_FEEDBACK | True | REQUIRES_SEPARATE_FUNCTIONAL_SUBPORTS |
| P157 | `L86_SevenAxisJointServoController` | 2 | ANALOG_FEEDBACK | False | REQUIRES_SEPARATE_FUNCTIONAL_SUBPORTS |
| P010 | `L86_RobotJoint5Forearm` | 2 | ACTUATOR_COMMAND;ANALOG_FEEDBACK | True | REQUIRES_SEPARATE_FUNCTIONAL_SUBPORTS |
| P011 | `L86_RobotJoint6WristPitch` | 2 | ACTUATOR_COMMAND;ANALOG_FEEDBACK | True | REQUIRES_SEPARATE_FUNCTIONAL_SUBPORTS |
| P160 | `L86_SevenAxisJointServoController` | 3 | ACTUATOR_COMMAND;DIGITAL_STATUS;SAFETY_CONTROL | True | REQUIRES_SEPARATE_FUNCTIONAL_SUBPORTS |
| P014 | `L86_RobotJoint7WristRoll` | 2 | ACTUATOR_COMMAND;ANALOG_FEEDBACK | True | REQUIRES_SEPARATE_FUNCTIONAL_SUBPORTS |
| P017 | `L86_JointEncoderFeedbackArray` | 2 | DATA_STREAM | False | REQUIRES_SEPARATE_FUNCTIONAL_SUBPORTS |
| P154 | `L86_SevenAxisJointServoController` | 2 | SAFETY_CONTROL | False | REQUIRES_SEPARATE_FUNCTIONAL_SUBPORTS |
| P032 | `L86_RobotBrakeAndTorqueLimiter` | 2 | DIGITAL_STATUS;SAFETY_CONTROL | True | REQUIRES_SEPARATE_FUNCTIONAL_SUBPORTS |
| P181 | `L86_StereoVisionFusionComputer` | 4 | DATA_STREAM;DIGITAL_CONTROL | True | REQUIRES_SEPARATE_FUNCTIONAL_SUBPORTS |
| P173 | `L86_StereoVisionFusionComputer` | 4 | ANALOG_FEEDBACK;DATA_STREAM;DIGITAL_CONTROL | True | REQUIRES_SEPARATE_FUNCTIONAL_SUBPORTS |
| P062 | `L86_CollisionProximityScanner` | 3 | ANALOG_FEEDBACK;DATA_STREAM;SAFETY_CONTROL | True | REQUIRES_SEPARATE_FUNCTIONAL_SUBPORTS |
| P064 | `L86_RobotTrajectorySafetySupervisor` | 3 | ANALOG_FEEDBACK;DATA_STREAM;SAFETY_CONTROL | True | REQUIRES_SEPARATE_FUNCTIONAL_SUBPORTS |
| P055 | `L86_RobotJointPathPlanningComputer` | 2 | DATA_STREAM | False | REQUIRES_SEPARATE_FUNCTIONAL_SUBPORTS |
| P045 | `L86_SixAxisWristForceTorqueSensor` | 2 | ANALOG_FEEDBACK | False | REQUIRES_SEPARATE_FUNCTIONAL_SUBPORTS |
| P051 | `L86_CompliantInsertionController` | 2 | ACTUATOR_COMMAND;ANALOG_FEEDBACK | True | REQUIRES_SEPARATE_FUNCTIONAL_SUBPORTS |
| P166 | `L86_ContactForceSignalConditioner` | 2 | DATA_STREAM;SAFETY_CONTROL | True | REQUIRES_SEPARATE_FUNCTIONAL_SUBPORTS |
| P066 | `L86_RobotTrajectorySafetySupervisor` | 2 | SAFETY_CONTROL | False | REQUIRES_SEPARATE_FUNCTIONAL_SUBPORTS |
| P144 | `L86_IndependentContactSafetyRelay` | 2 | SAFETY_CONTROL | False | REQUIRES_SEPARATE_FUNCTIONAL_SUBPORTS |
| P050 | `L86_CompliantInsertionController` | 4 | ANALOG_FEEDBACK;DIGITAL_STATUS;SAFETY_CONTROL | True | REQUIRES_SEPARATE_FUNCTIONAL_SUBPORTS |
| P190 | `L86_RadiatorServiceSequencingPLC` | 3 | ACTUATOR_COMMAND;DATA_STREAM;DIGITAL_STATUS | True | REQUIRES_SEPARATE_FUNCTIONAL_SUBPORTS |
| P067 | `L86_FlexiblePayloadModalSensor` | 2 | ANALOG_FEEDBACK | False | REQUIRES_SEPARATE_FUNCTIONAL_SUBPORTS |
| P069 | `L86_ActiveVibrationDampingController` | 2 | ACTUATOR_COMMAND;ANALOG_FEEDBACK | True | REQUIRES_SEPARATE_FUNCTIONAL_SUBPORTS |
| P110 | `L86_PayloadVibrationFaultInjector` | 2 | DIGITAL_STATUS;SAFETY_CONTROL | True | REQUIRES_SEPARATE_FUNCTIONAL_SUBPORTS |
| P111 | `L86_JointEncoderDriftFaultInjector` | 2 | DIGITAL_STATUS;SAFETY_CONTROL | True | REQUIRES_SEPARATE_FUNCTIONAL_SUBPORTS |
| P019 | `L86_JointEncoderFeedbackArray` | 2 | SAFETY_CONTROL | False | REQUIRES_SEPARATE_FUNCTIONAL_SUBPORTS |
| P088 | `L86_DualJawGripperPressureSensorArray` | 3 | ANALOG_FEEDBACK;SAFETY_CONTROL | True | REQUIRES_SEPARATE_FUNCTIONAL_SUBPORTS |
| P200 | `L86_RadiatorServiceSequencingPLC` | 2 | ACTUATOR_COMMAND;DIGITAL_STATUS | True | REQUIRES_SEPARATE_FUNCTIONAL_SUBPORTS |
| P208 | `L86_HolographicOrbitalRoboticsSCADA` | 2 | DATA_STREAM | False | REQUIRES_SEPARATE_FUNCTIONAL_SUBPORTS |
| P115 | `L86_RoboticContactSafetyPLC` | 2 | DIGITAL_STATUS;SAFETY_CONTROL | True | REQUIRES_SEPARATE_FUNCTIONAL_SUBPORTS |
| P073 | `L86_AdaptiveRadiatorHandlingGripper` | 4 | ACTUATOR_COMMAND;ANALOG_FEEDBACK;SAFETY_CONTROL | True | REQUIRES_SEPARATE_FUNCTIONAL_SUBPORTS |
| P077 | `L86_AutomaticEndEffectorToolChanger` | 2 | ACTUATOR_COMMAND;DIGITAL_STATUS | True | REQUIRES_SEPARATE_FUNCTIONAL_SUBPORTS |
| P079 | `L86_PrecisionTorqueFasteningDriver` | 2 | ANALOG_FEEDBACK;DIGITAL_STATUS | True | REQUIRES_SEPARATE_FUNCTIONAL_SUBPORTS |
| P204 | `L86_RadiatorServiceSequencingPLC` | 2 | ANALOG_FEEDBACK;SAFETY_CONTROL | True | REQUIRES_SEPARATE_FUNCTIONAL_SUBPORTS |
| P188 | `L86_RadiatorServiceSequencingPLC` | 2 | DIGITAL_CONTROL;DIGITAL_STATUS | True | REQUIRES_SEPARATE_FUNCTIONAL_SUBPORTS |
| P082 | `L86_VacuumCompatibleInspectionProbe` | 2 | DATA_STREAM;DIGITAL_CONTROL | True | REQUIRES_SEPARATE_FUNCTIONAL_SUBPORTS |
| P180 | `L86_StereoVisionFusionComputer` | 2 | DATA_STREAM | False | REQUIRES_SEPARATE_FUNCTIONAL_SUBPORTS |
| P130 | `L86_RoboticContactSafetyPLC` | 4 | DIGITAL_STATUS;SAFETY_CONTROL | True | REQUIRES_SEPARATE_FUNCTIONAL_SUBPORTS |
| P112 | `L86_AsymmetricGripperFaultInjector` | 2 | DIGITAL_STATUS;SAFETY_CONTROL | True | REQUIRES_SEPARATE_FUNCTIONAL_SUBPORTS |
| P197 | `L86_RadiatorServiceSequencingPLC` | 2 | ANALOG_FEEDBACK | False | REQUIRES_SEPARATE_FUNCTIONAL_SUBPORTS |
| P103 | `L86_RadiatorFastenerActuatorDriver` | 2 | ACTUATOR_COMMAND;DIGITAL_STATUS | True | REQUIRES_SEPARATE_FUNCTIONAL_SUBPORTS |
| P202 | `L86_RadiatorServiceSequencingPLC` | 2 | ACTUATOR_COMMAND | False | REQUIRES_SEPARATE_FUNCTIONAL_SUBPORTS |
| P185 | `L86_RadiatorServiceSequencingPLC` | 3 | ANALOG_FEEDBACK;DIGITAL_STATUS | True | REQUIRES_SEPARATE_FUNCTIONAL_SUBPORTS |
| P219 | `L86_HolographicOrbitalRoboticsSCADA` | 4 | DATA_STREAM | False | REQUIRES_SEPARATE_FUNCTIONAL_SUBPORTS |
| P102 | `L86_CoolantIsolationAndVentStation` | 2 | ACTUATOR_COMMAND;ANALOG_FEEDBACK | True | REQUIRES_SEPARATE_FUNCTIONAL_SUBPORTS |
| P090 | `L86_CoolantQuickDisconnectA` | 2 | ACTUATOR_COMMAND;ANALOG_FEEDBACK | True | REQUIRES_SEPARATE_FUNCTIONAL_SUBPORTS |
| P091 | `L86_CoolantQuickDisconnectB` | 2 | ACTUATOR_COMMAND;ANALOG_FEEDBACK | True | REQUIRES_SEPARATE_FUNCTIONAL_SUBPORTS |
| P189 | `L86_RadiatorServiceSequencingPLC` | 2 | ANALOG_FEEDBACK;DATA_STREAM | True | REQUIRES_SEPARATE_FUNCTIONAL_SUBPORTS |
| P193 | `L86_RadiatorServiceSequencingPLC` | 2 | ACTUATOR_COMMAND | False | REQUIRES_SEPARATE_FUNCTIONAL_SUBPORTS |
| P092 | `L86_CoolantValveManifold` | 2 | ACTUATOR_COMMAND;ANALOG_FEEDBACK | True | REQUIRES_SEPARATE_FUNCTIONAL_SUBPORTS |
| P100 | `L86_RadiatorLeakTestController` | 2 | ANALOG_FEEDBACK | False | REQUIRES_SEPARATE_FUNCTIONAL_SUBPORTS |
| P095 | `L86_RadiatorTemperatureSensorArray` | 2 | ANALOG_FEEDBACK;DATA_STREAM | True | REQUIRES_SEPARATE_FUNCTIONAL_SUBPORTS |
| P106 | `L86_RadiatorThermalPerformanceAnalyzer` | 3 | ANALOG_FEEDBACK | False | REQUIRES_SEPARATE_FUNCTIONAL_SUBPORTS |
| P097 | `L86_ThermalLoopPumpSimulator` | 2 | ACTUATOR_COMMAND;ANALOG_FEEDBACK | True | REQUIRES_SEPARATE_FUNCTIONAL_SUBPORTS |
| P107 | `L86_RadiatorThermalPerformanceAnalyzer` | 2 | DATA_STREAM | False | REQUIRES_SEPARATE_FUNCTIONAL_SUBPORTS |
| P118 | `L86_RoboticContactSafetyPLC` | 4 | DIGITAL_STATUS;SAFETY_CONTROL | True | REQUIRES_SEPARATE_FUNCTIONAL_SUBPORTS |
| P122 | `L86_RoboticContactSafetyPLC` | 3 | DIGITAL_STATUS;SAFETY_CONTROL | True | REQUIRES_SEPARATE_FUNCTIONAL_SUBPORTS |
| P126 | `L86_RoboticContactSafetyPLC` | 3 | DIGITAL_STATUS;SAFETY_CONTROL | True | REQUIRES_SEPARATE_FUNCTIONAL_SUBPORTS |
| P205 | `L86_RadiatorServiceSequencingPLC` | 2 | DATA_STREAM;SAFETY_CONTROL | True | REQUIRES_SEPARATE_FUNCTIONAL_SUBPORTS |
| P224 | `L86_W129CriticalFeedbackPatchStation` | 2 | ANALOG_FEEDBACK;DIGITAL_STATUS | True | REQUIRES_SEPARATE_FUNCTIONAL_SUBPORTS |
| P207 | `L86_HolographicOrbitalRoboticsSCADA` | 3 | DATA_STREAM | False | REQUIRES_SEPARATE_FUNCTIONAL_SUBPORTS |
| P211 | `L86_HolographicOrbitalRoboticsSCADA` | 3 | DATA_STREAM | False | REQUIRES_SEPARATE_FUNCTIONAL_SUBPORTS |
| P215 | `L86_HolographicOrbitalRoboticsSCADA` | 3 | DATA_STREAM;DIGITAL_STATUS | True | REQUIRES_SEPARATE_FUNCTIONAL_SUBPORTS |
| P230 | `L86_RoboticServicingEventRecorder` | 2 | DATA_STREAM | False | REQUIRES_SEPARATE_FUNCTIONAL_SUBPORTS |
| P229 | `L86_ServicingMissionCertificationBeacon` | 2 | DIGITAL_STATUS | False | REQUIRES_SEPARATE_FUNCTIONAL_SUBPORTS |



## ANNEX G — EXACT 7-AXIS ARTICULATION CHAIN

Source: `Robot_Joint_Articulation_Chain.csv`. This chain is visual source-art geometry with world pivot centers; the physics/IK robot model must independently validate the link frames, joint limits, tool center and motion.

| Order | Model | Pivot | Axis | JointCenterX_m | JointCenterY_m | JointCenterZ_m | ParentPivot | Note |
|---|---|---|---|---|---|---|---|---|
| 1 | L86_RobotJoint1BaseYaw | ANIM_ROBOT_BASE_ROTATE | y | -4.55 | 2.52 | -1.52 | ANIM_LINEAR_RAIL_CARRIAGE | Actual visual pivot; reparent at runtime preserving world transform; drive by measured actual axis angle |
| 2 | L86_RobotJoint2Shoulder | ANIM_ROBOT_SHOULDER_PITCH | z | -4.15 | 3.03 | -1.52 | ANIM_ROBOT_BASE_ROTATE | Actual visual pivot; reparent at runtime preserving world transform; drive by measured actual axis angle |
| 3 | L86_RobotJoint3UpperArm | ANIM_ROBOT_UPPERARM_PITCH | z | -3.3 | 3.67 | -1.52 | ANIM_ROBOT_SHOULDER_PITCH | Actual visual pivot; reparent at runtime preserving world transform; drive by measured actual axis angle |
| 4 | L86_RobotJoint4Elbow | ANIM_ROBOT_ELBOW_PITCH | z | -2.35 | 4.22 | -1.52 | ANIM_ROBOT_UPPERARM_PITCH | Actual visual pivot; reparent at runtime preserving world transform; drive by measured actual axis angle |
| 5 | L86_RobotJoint5Forearm | ANIM_ROBOT_FOREARM_ROLL | x | -1.3 | 4.74 | -1.52 | ANIM_ROBOT_ELBOW_PITCH | Actual visual pivot; reparent at runtime preserving world transform; drive by measured actual axis angle |
| 6 | L86_RobotJoint6WristPitch | ANIM_ROBOT_WRIST_PITCH | z | -0.55 | 5.15 | -1.52 | ANIM_ROBOT_FOREARM_ROLL | Actual visual pivot; reparent at runtime preserving world transform; drive by measured actual axis angle |
| 7 | L86_RobotJoint7WristRoll | ANIM_ROBOT_WRIST_ROLL | x | 0.25 | 5.42 | -1.52 | ANIM_ROBOT_WRIST_PITCH | Actual visual pivot; reparent at runtime preserving world transform; drive by measured actual axis angle |



## ANNEX H — ALL SIX EXACT OBJECTIVE-DESIGN ROWS

Source: `Objective_Certification_Plan_6.csv`. `PLANNED_NOT_IMPLEMENTED` is not a tested or satisfied runtime state.

| Objective | Exact title | Required evidence | Evidence capture | Status |
|---|---|---|---|---|
| 1 | Build the Orbital Robotics Control Network | All 152 real player wires and real functionally separated SparkTerminal contacts; proper 5V supplies, returns and independent interlocks; W129 valid | Player-connected electrical graph, fresh control inputs, correctly typed ports | PLANNED_NOT_IMPLEMENTED |
| 2 | Commission and Calibrate the Seven-Axis Robot | All seven axes homed; encoder offsets verified; wrist force/torque calibrated; stereo/depth vision aligned; collision-free trial | Physical joint actual states, timestamped calibration record, completed commanded path | PLANNED_NOT_IMPLEMENTED |
| 3 | Extract the Failed Radiator | Coolant isolated and pressure reduced; all bolts disengaged; safe gripper contact; controlled extraction; faulty radiator cradled | Electrical and thermal isolation logs, actual jaw/torque/pose state, safe extraction evidence | PLANNED_NOT_IMPLEMENTED |
| 4 | Install and Restore the Thermal Radiator | Replacement fetched, oscillation damped, insertion aligned and fastened; connectors secured; pump and radiator reject sufficient heat | Real geometry/contact state, fastening evidence, pressure/flow/temperature histories | PLANNED_NOT_IMPLEMENTED |
| 5 | Recover Three Robotic Servicing Failures | Real induced encoder drift, flexible payload vibration, and asymmetric gripper; diagnosis, recovery and fresh service-cassette trial | Deterministic fault logs and new qualified post-repair test run | PLANNED_NOT_IMPLEMENTED |
| 6 | Restore Critical Contact-Force Feedback | After historical 1-5: real W129 disconnect -> stale trusted wrench -> protected stop -> latched fault; reconnect, recalibrate, explicit reset, fresh whole manipulation and cooling qualification | W129 topology evidence, safety event sequence, new runId and actual LevelGamePlayManager 6/6 | PLANNED_NOT_IMPLEMENTED |



## ANNEX I — ART/COMPONENT PLACEMENT AND ROLE REFERENCE

Source: `Art_Assembly.csv` and `Component_Roles.csv`. Keep the actual package as authoritative for exact placement, role and workbench allocation. Following is the art assembly table for immediate integration review.

| Index | Model | FBX | GLB | HomeX_m | HomeY_m | HomeZ_m | GeometryPositionSpace |
|---|---|---|---|---|---|---|---|
| 0 | L86_OrbitalForgeArenaBase | 00_L86_OrbitalForgeArenaBase.fbx | 00_L86_OrbitalForgeArenaBase.glb | 0 | 0.1 | 0 | SHARED_WORLD_ORIGIN |
| 1 | L86_PanoramicEarthMaintenanceViewport | 01_L86_PanoramicEarthMaintenanceViewport.fbx | 01_L86_PanoramicEarthMaintenanceViewport.glb | 0 | 5.8 | -7.4 | SHARED_WORLD_ORIGIN |
| 2 | L86_StationExteriorServiceTruss | 02_L86_StationExteriorServiceTruss.fbx | 02_L86_StationExteriorServiceTruss.glb | 9.9 | 5.4 | -3.2 | SHARED_WORLD_ORIGIN |
| 3 | L86_StationRobotServiceBayFrame | 03_L86_StationRobotServiceBayFrame.fbx | 03_L86_StationRobotServiceBayFrame.glb | 0 | 5.0 | -2.25 | SHARED_WORLD_ORIGIN |
| 4 | L86_DockedCargoSpacecraft | 04_L86_DockedCargoSpacecraft.fbx | 04_L86_DockedCargoSpacecraft.glb | -9.1 | 5.0 | -3.1 | SHARED_WORLD_ORIGIN |
| 5 | L86_CargoServiceBayHatch | 05_L86_CargoServiceBayHatch.fbx | 05_L86_CargoServiceBayHatch.glb | -7.4 | 5.0 | -3.0 | SHARED_WORLD_ORIGIN |
| 6 | L86_RobotLinearTraverseRail | 06_L86_RobotLinearTraverseRail.fbx | 06_L86_RobotLinearTraverseRail.glb | -4.55 | 1.17 | -1.52 | SHARED_WORLD_ORIGIN |
| 7 | L86_SevenAxisRobotBase | 07_L86_SevenAxisRobotBase.fbx | 07_L86_SevenAxisRobotBase.glb | -4.55 | 2.08 | -1.52 | SHARED_WORLD_ORIGIN |
| 8 | L86_RobotJoint1BaseYaw | 08_L86_RobotJoint1BaseYaw.fbx | 08_L86_RobotJoint1BaseYaw.glb | -4.55 | 2.52 | -1.52 | SHARED_WORLD_ORIGIN |
| 9 | L86_RobotJoint2Shoulder | 09_L86_RobotJoint2Shoulder.fbx | 09_L86_RobotJoint2Shoulder.glb | -4.15 | 3.03 | -1.52 | SHARED_WORLD_ORIGIN |
| 10 | L86_RobotJoint3UpperArm | 10_L86_RobotJoint3UpperArm.fbx | 10_L86_RobotJoint3UpperArm.glb | -3.3 | 3.67 | -1.52 | SHARED_WORLD_ORIGIN |
| 11 | L86_RobotJoint4Elbow | 11_L86_RobotJoint4Elbow.fbx | 11_L86_RobotJoint4Elbow.glb | -2.35 | 4.22 | -1.52 | SHARED_WORLD_ORIGIN |
| 12 | L86_RobotJoint5Forearm | 12_L86_RobotJoint5Forearm.fbx | 12_L86_RobotJoint5Forearm.glb | -1.3 | 4.74 | -1.52 | SHARED_WORLD_ORIGIN |
| 13 | L86_RobotJoint6WristPitch | 13_L86_RobotJoint6WristPitch.fbx | 13_L86_RobotJoint6WristPitch.glb | -0.55 | 5.15 | -1.52 | SHARED_WORLD_ORIGIN |
| 14 | L86_RobotJoint7WristRoll | 14_L86_RobotJoint7WristRoll.fbx | 14_L86_RobotJoint7WristRoll.glb | 0.25 | 5.42 | -1.52 | SHARED_WORLD_ORIGIN |
| 15 | L86_RobotCounterbalanceModule | 15_L86_RobotCounterbalanceModule.fbx | 15_L86_RobotCounterbalanceModule.glb | -4.96 | 3.27 | -1.98 | SHARED_WORLD_ORIGIN |
| 16 | L86_JointServoDriveRack | 16_L86_JointServoDriveRack.fbx | 16_L86_JointServoDriveRack.glb | -12.35 | 1.71 | 5.55 | SHARED_WORLD_ORIGIN |
| 17 | L86_JointEncoderFeedbackArray | 17_L86_JointEncoderFeedbackArray.fbx | 17_L86_JointEncoderFeedbackArray.glb | -11.1 | 1.71 | 5.55 | SHARED_WORLD_ORIGIN |
| 18 | L86_JointTorqueCurrentSensorRack | 18_L86_JointTorqueCurrentSensorRack.fbx | 18_L86_JointTorqueCurrentSensorRack.glb | -9.85 | 1.71 | 5.55 | SHARED_WORLD_ORIGIN |
| 19 | L86_RobotBrakeAndTorqueLimiter | 19_L86_RobotBrakeAndTorqueLimiter.fbx | 19_L86_RobotBrakeAndTorqueLimiter.glb | -8.6 | 1.71 | 5.55 | SHARED_WORLD_ORIGIN |
| 20 | L86_SevenAxisReachEnvelopeHologram | 20_L86_SevenAxisReachEnvelopeHologram.fbx | 20_L86_SevenAxisReachEnvelopeHologram.glb | -3.1 | 7.65 | -5.9 | SHARED_WORLD_ORIGIN |
| 21 | L86_RoboticHomingCalibrationFixture | 21_L86_RoboticHomingCalibrationFixture.fbx | 21_L86_RoboticHomingCalibrationFixture.glb | -5.95 | 1.7 | 4.25 | SHARED_WORLD_ORIGIN |
| 22 | L86_StereoMachineVisionCameraLeft | 22_L86_StereoMachineVisionCameraLeft.fbx | 22_L86_StereoMachineVisionCameraLeft.glb | -1.2 | 6.3 | -1.1 | SHARED_WORLD_ORIGIN |
| 23 | L86_StereoMachineVisionCameraRight | 23_L86_StereoMachineVisionCameraRight.fbx | 23_L86_StereoMachineVisionCameraRight.glb | -0.45 | 6.3 | -1.1 | SHARED_WORLD_ORIGIN |
| 24 | L86_StructuredLightDepthScanner | 24_L86_StructuredLightDepthScanner.fbx | 24_L86_StructuredLightDepthScanner.glb | 1.05 | 6.25 | -1.7 | SHARED_WORLD_ORIGIN |
| 25 | L86_OpticalFiducialTargetArray | 25_L86_OpticalFiducialTargetArray.fbx | 25_L86_OpticalFiducialTargetArray.glb | 7.5 | 5.85 | -2.65 | SHARED_WORLD_ORIGIN |
| 26 | L86_PrecisionVisionCalibrationBoard | 26_L86_PrecisionVisionCalibrationBoard.fbx | 26_L86_PrecisionVisionCalibrationBoard.glb | -3.2 | 1.69 | 4.36 | SHARED_WORLD_ORIGIN |
| 27 | L86_LaserAlignmentMetrologyUnit | 27_L86_LaserAlignmentMetrologyUnit.fbx | 27_L86_LaserAlignmentMetrologyUnit.glb | 2.4 | 6.5 | -2.75 | SHARED_WORLD_ORIGIN |
| 28 | L86_SixAxisWristForceTorqueSensor | 28_L86_SixAxisWristForceTorqueSensor.fbx | 28_L86_SixAxisWristForceTorqueSensor.glb | 0.9 | 5.3 | -1.52 | SHARED_WORLD_ORIGIN |
| 29 | L86_ForceTorqueCalibrationFixture | 29_L86_ForceTorqueCalibrationFixture.fbx | 29_L86_ForceTorqueCalibrationFixture.glb | -1.95 | 1.7 | 5.45 | SHARED_WORLD_ORIGIN |
| 30 | L86_CompliantInsertionController | 30_L86_CompliantInsertionController.fbx | 30_L86_CompliantInsertionController.glb | -0.7 | 1.7 | 5.45 | SHARED_WORLD_ORIGIN |
| 31 | L86_SixAxisContactForceHologram | 31_L86_SixAxisContactForceHologram.fbx | 31_L86_SixAxisContactForceHologram.glb | 3.4 | 7.32 | -5.95 | SHARED_WORLD_ORIGIN |
| 32 | L86_RobotJointPathPlanningComputer | 32_L86_RobotJointPathPlanningComputer.fbx | 32_L86_RobotJointPathPlanningComputer.glb | 0.56 | 1.7 | 5.45 | SHARED_WORLD_ORIGIN |
| 33 | L86_InverseKinematicsSolverDisplay | 33_L86_InverseKinematicsSolverDisplay.fbx | 33_L86_InverseKinematicsSolverDisplay.glb | 1.81 | 1.7 | 5.45 | SHARED_WORLD_ORIGIN |
| 34 | L86_CollisionProximityScanner | 34_L86_CollisionProximityScanner.fbx | 34_L86_CollisionProximityScanner.glb | 3.06 | 1.7 | 5.45 | SHARED_WORLD_ORIGIN |
| 35 | L86_RobotTrajectorySafetySupervisor | 35_L86_RobotTrajectorySafetySupervisor.fbx | 35_L86_RobotTrajectorySafetySupervisor.glb | 4.31 | 1.7 | 5.45 | SHARED_WORLD_ORIGIN |
| 36 | L86_FlexiblePayloadModalSensor | 36_L86_FlexiblePayloadModalSensor.fbx | 36_L86_FlexiblePayloadModalSensor.glb | 1.15 | 5.55 | -1.65 | SHARED_WORLD_ORIGIN |
| 37 | L86_ActiveVibrationDampingController | 37_L86_ActiveVibrationDampingController.fbx | 37_L86_ActiveVibrationDampingController.glb | 5.56 | 1.7 | 5.45 | SHARED_WORLD_ORIGIN |
| 38 | L86_AdaptiveRadiatorHandlingGripper | 38_L86_AdaptiveRadiatorHandlingGripper.fbx | 38_L86_AdaptiveRadiatorHandlingGripper.glb | 1.5 | 5.22 | -1.52 | SHARED_WORLD_ORIGIN |
| 39 | L86_AutomaticEndEffectorToolChanger | 39_L86_AutomaticEndEffectorToolChanger.fbx | 39_L86_AutomaticEndEffectorToolChanger.glb | 0.85 | 5.18 | -1.52 | SHARED_WORLD_ORIGIN |
| 40 | L86_PrecisionTorqueFasteningDriver | 40_L86_PrecisionTorqueFasteningDriver.fbx | 40_L86_PrecisionTorqueFasteningDriver.glb | 2.32 | 5.1 | -1.52 | SHARED_WORLD_ORIGIN |
| 41 | L86_CaptiveFastenerAndBoltRack | 41_L86_CaptiveFastenerAndBoltRack.fbx | 41_L86_CaptiveFastenerAndBoltRack.glb | -5.2 | 1.45 | 5.35 | SHARED_WORLD_ORIGIN |
| 42 | L86_VacuumCompatibleInspectionProbe | 42_L86_VacuumCompatibleInspectionProbe.fbx | 42_L86_VacuumCompatibleInspectionProbe.glb | 0.93 | 5.56 | -1.45 | SHARED_WORLD_ORIGIN |
| 43 | L86_RoboticToolDockAndCalibrationNest | 43_L86_RoboticToolDockAndCalibrationNest.fbx | 43_L86_RoboticToolDockAndCalibrationNest.glb | -5.65 | 1.68 | 3.82 | SHARED_WORLD_ORIGIN |
| 44 | L86_DualJawGripperPressureSensorArray | 44_L86_DualJawGripperPressureSensorArray.fbx | 44_L86_DualJawGripperPressureSensorArray.glb | 1.6 | 5.1 | -1.5 | SHARED_WORLD_ORIGIN |
| 45 | L86_RadiatorGrappleHandleFixture | 45_L86_RadiatorGrappleHandleFixture.fbx | 45_L86_RadiatorGrappleHandleFixture.glb | 6.8 | 5.5 | -2.17 | SHARED_WORLD_ORIGIN |
| 46 | L86_FailedThermalRadiatorModule | 46_L86_FailedThermalRadiatorModule.fbx | 46_L86_FailedThermalRadiatorModule.glb | 8.15 | 5.2 | -2.35 | SHARED_WORLD_ORIGIN |
| 47 | L86_ReplacementThermalRadiatorModule | 47_L86_ReplacementThermalRadiatorModule.fbx | 47_L86_ReplacementThermalRadiatorModule.glb | 8.25 | 1.39 | 4.05 | SHARED_WORLD_ORIGIN |
| 48 | L86_RadiatorMountingSpaceTruss | 48_L86_RadiatorMountingSpaceTruss.fbx | 48_L86_RadiatorMountingSpaceTruss.glb | 9.25 | 5.35 | -2.4 | SHARED_WORLD_ORIGIN |
| 49 | L86_RadiatorThermalInterfacePlate | 49_L86_RadiatorThermalInterfacePlate.fbx | 49_L86_RadiatorThermalInterfacePlate.glb | 8.37 | 5.25 | -2.25 | SHARED_WORLD_ORIGIN |
| 50 | L86_CoolantQuickDisconnectA | 50_L86_CoolantQuickDisconnectA.fbx | 50_L86_CoolantQuickDisconnectA.glb | 7.0 | 4.6 | -1.85 | SHARED_WORLD_ORIGIN |
| 51 | L86_CoolantQuickDisconnectB | 51_L86_CoolantQuickDisconnectB.fbx | 51_L86_CoolantQuickDisconnectB.glb | 9.4 | 4.6 | -1.85 | SHARED_WORLD_ORIGIN |
| 52 | L86_CoolantValveManifold | 52_L86_CoolantValveManifold.fbx | 52_L86_CoolantValveManifold.glb | 9.15 | 1.64 | 5.45 | SHARED_WORLD_ORIGIN |
| 53 | L86_CoolantPressureTransducer | 53_L86_CoolantPressureTransducer.fbx | 53_L86_CoolantPressureTransducer.glb | 10.35 | 1.64 | 5.45 | SHARED_WORLD_ORIGIN |
| 54 | L86_CoolantFlowMeter | 54_L86_CoolantFlowMeter.fbx | 54_L86_CoolantFlowMeter.glb | 11.55 | 1.64 | 5.45 | SHARED_WORLD_ORIGIN |
| 55 | L86_RadiatorTemperatureSensorArray | 55_L86_RadiatorTemperatureSensorArray.fbx | 55_L86_RadiatorTemperatureSensorArray.glb | 9.9 | 5.2 | -2.8 | SHARED_WORLD_ORIGIN |
| 56 | L86_StationAvionicsHeatLoadEmulator | 56_L86_StationAvionicsHeatLoadEmulator.fbx | 56_L86_StationAvionicsHeatLoadEmulator.glb | -2.7 | 6.2 | -3.35 | SHARED_WORLD_ORIGIN |
| 57 | L86_ThermalLoopPumpSimulator | 57_L86_ThermalLoopPumpSimulator.fbx | 57_L86_ThermalLoopPumpSimulator.glb | 7.05 | 1.67 | 5.5 | SHARED_WORLD_ORIGIN |
| 58 | L86_RadiatorLeakTestController | 58_L86_RadiatorLeakTestController.fbx | 58_L86_RadiatorLeakTestController.glb | 8.4 | 1.66 | 5.52 | SHARED_WORLD_ORIGIN |
| 59 | L86_CoolantIsolationAndVentStation | 59_L86_CoolantIsolationAndVentStation.fbx | 59_L86_CoolantIsolationAndVentStation.glb | 10.1 | 3.3 | 5.16 | SHARED_WORLD_ORIGIN |
| 60 | L86_RadiatorFastenerActuatorDriver | 60_L86_RadiatorFastenerActuatorDriver.fbx | 60_L86_RadiatorFastenerActuatorDriver.glb | 7.14 | 5.65 | -2.7 | SHARED_WORLD_ORIGIN |
| 61 | L86_StationThermalTrendHologram | 61_L86_StationThermalTrendHologram.fbx | 61_L86_StationThermalTrendHologram.glb | 0.0 | 8.65 | -6.1 | SHARED_WORLD_ORIGIN |
| 62 | L86_RadiatorThermalPerformanceAnalyzer | 62_L86_RadiatorThermalPerformanceAnalyzer.fbx | 62_L86_RadiatorThermalPerformanceAnalyzer.glb | 11.28 | 3.3 | 5.16 | SHARED_WORLD_ORIGIN |
| 63 | L86_FailedRadiatorDisposalCradle | 63_L86_FailedRadiatorDisposalCradle.fbx | 63_L86_FailedRadiatorDisposalCradle.glb | 9.25 | 1.1 | 3.2 | SHARED_WORLD_ORIGIN |
| 64 | L86_PayloadVibrationFaultInjector | 64_L86_PayloadVibrationFaultInjector.fbx | 64_L86_PayloadVibrationFaultInjector.glb | 6.75 | 3.35 | 5.65 | SHARED_WORLD_ORIGIN |
| 65 | L86_JointEncoderDriftFaultInjector | 65_L86_JointEncoderDriftFaultInjector.fbx | 65_L86_JointEncoderDriftFaultInjector.glb | -7.12 | 3.35 | 5.65 | SHARED_WORLD_ORIGIN |
| 66 | L86_AsymmetricGripperFaultInjector | 66_L86_AsymmetricGripperFaultInjector.fbx | 66_L86_AsymmetricGripperFaultInjector.glb | -5.85 | 3.35 | 5.65 | SHARED_WORLD_ORIGIN |
| 67 | L86_RoboticContactSafetyPLC | 67_L86_RoboticContactSafetyPLC.fbx | 67_L86_RoboticContactSafetyPLC.glb | -11.1 | 3.5 | 5.52 | SHARED_WORLD_ORIGIN |
| 68 | L86_ManipulatorEmergencyStopStation | 68_L86_ManipulatorEmergencyStopStation.fbx | 68_L86_ManipulatorEmergencyStopStation.glb | -9.85 | 3.5 | 5.52 | SHARED_WORLD_ORIGIN |
| 69 | L86_Protected5VControlSupply | 69_L86_Protected5VControlSupply.fbx | 69_L86_Protected5VControlSupply.glb | -8.6 | 3.5 | 5.52 | SHARED_WORLD_ORIGIN |
| 70 | L86_RobotMotorPowerDistributionUnit | 70_L86_RobotMotorPowerDistributionUnit.fbx | 70_L86_RobotMotorPowerDistributionUnit.glb | -7.35 | 3.5 | 5.52 | SHARED_WORLD_ORIGIN |
| 71 | L86_IndependentContactSafetyRelay | 71_L86_IndependentContactSafetyRelay.fbx | 71_L86_IndependentContactSafetyRelay.glb | -6.1 | 3.5 | 5.52 | SHARED_WORLD_ORIGIN |
| 72 | L86_SevenAxisJointServoController | 72_L86_SevenAxisJointServoController.fbx | 72_L86_SevenAxisJointServoController.glb | -4.85 | 3.5 | 5.52 | SHARED_WORLD_ORIGIN |
| 73 | L86_ContactForceSignalConditioner | 73_L86_ContactForceSignalConditioner.fbx | 73_L86_ContactForceSignalConditioner.glb | -3.6 | 3.5 | 5.52 | SHARED_WORLD_ORIGIN |
| 74 | L86_StereoVisionFusionComputer | 74_L86_StereoVisionFusionComputer.fbx | 74_L86_StereoVisionFusionComputer.glb | -2.35 | 3.5 | 5.52 | SHARED_WORLD_ORIGIN |
| 75 | L86_RadiatorServiceSequencingPLC | 75_L86_RadiatorServiceSequencingPLC.fbx | 75_L86_RadiatorServiceSequencingPLC.glb | -1.1 | 3.5 | 5.52 | SHARED_WORLD_ORIGIN |
| 76 | L86_HolographicOrbitalRoboticsSCADA | 76_L86_HolographicOrbitalRoboticsSCADA.fbx | 76_L86_HolographicOrbitalRoboticsSCADA.glb | 0.0 | 8.8 | -6.33 | SHARED_WORLD_ORIGIN |
| 77 | L86_W129CriticalFeedbackPatchStation | 77_L86_W129CriticalFeedbackPatchStation.fbx | 77_L86_W129CriticalFeedbackPatchStation.glb | 0.15 | 3.5 | 5.52 | SHARED_WORLD_ORIGIN |
| 78 | L86_HardwareInterlockDiagnosticRack | 78_L86_HardwareInterlockDiagnosticRack.fbx | 78_L86_HardwareInterlockDiagnosticRack.glb | 1.4 | 3.5 | 5.52 | SHARED_WORLD_ORIGIN |
| 79 | L86_ServicingMissionCertificationBeacon | 79_L86_ServicingMissionCertificationBeacon.fbx | 79_L86_ServicingMissionCertificationBeacon.glb | 0.0 | 10.3 | -6.32 | SHARED_WORLD_ORIGIN |
| 80 | L86_ExternalInspectionCameraGantry | 80_L86_ExternalInspectionCameraGantry.fbx | 80_L86_ExternalInspectionCameraGantry.glb | 7.1 | 7.6 | -0.8 | SHARED_WORLD_ORIGIN |
| 81 | L86_RoboticServicingEventRecorder | 81_L86_RoboticServicingEventRecorder.fbx | 81_L86_RoboticServicingEventRecorder.glb | 2.65 | 3.5 | 5.52 | SHARED_WORLD_ORIGIN |



## ANNEX J — TEXTURE SETS AND RENDER IMPORT RULES

Source: `Texture_Manifest.json`. Files below are optional procedural maps; production UVs and shader packing remain to be authored.

| Set | Resolution | Albedo file | Normal file | Packed M/R/AO file |
|---|---|---|---|---|
| RoboticArmCeramicCoating | 1024×1024 | Textures/RoboticArmCeramicCoating_Albedo.png | Textures/RoboticArmCeramicCoating_Normal.png | Textures/RoboticArmCeramicCoating_MetalRoughAO.png |
| MachinedTitaniumLink | 1024×1024 | Textures/MachinedTitaniumLink_Albedo.png | Textures/MachinedTitaniumLink_Normal.png | Textures/MachinedTitaniumLink_MetalRoughAO.png |
| ArmActuatorGoldAnodized | 1024×1024 | Textures/ArmActuatorGoldAnodized_Albedo.png | Textures/ArmActuatorGoldAnodized_Normal.png | Textures/ArmActuatorGoldAnodized_MetalRoughAO.png |
| StationDarkCarbonComposite | 1024×1024 | Textures/StationDarkCarbonComposite_Albedo.png | Textures/StationDarkCarbonComposite_Normal.png | Textures/StationDarkCarbonComposite_MetalRoughAO.png |
| RadiatorOpticalBlackFins | 1024×1024 | Textures/RadiatorOpticalBlackFins_Albedo.png | Textures/RadiatorOpticalBlackFins_Normal.png | Textures/RadiatorOpticalBlackFins_MetalRoughAO.png |
| RadiatorThermalCopperHeatPipe | 1024×1024 | Textures/RadiatorThermalCopperHeatPipe_Albedo.png | Textures/RadiatorThermalCopperHeatPipe_Normal.png | Textures/RadiatorThermalCopperHeatPipe_MetalRoughAO.png |
| CoolantPortSteelCeramic | 1024×1024 | Textures/CoolantPortSteelCeramic_Albedo.png | Textures/CoolantPortSteelCeramic_Normal.png | Textures/CoolantPortSteelCeramic_MetalRoughAO.png |
| RobotOpticalSensorGlass | 1024×1024 | Textures/RobotOpticalSensorGlass_Albedo.png | Textures/RobotOpticalSensorGlass_Normal.png | Textures/RobotOpticalSensorGlass_MetalRoughAO.png |
| SpaceStationServiceFloor | 1024×1024 | Textures/SpaceStationServiceFloor_Albedo.png | Textures/SpaceStationServiceFloor_Normal.png | Textures/SpaceStationServiceFloor_MetalRoughAO.png |
| AvionicsInstrumentPanel | 1024×1024 | Textures/AvionicsInstrumentPanel_Albedo.png | Textures/AvionicsInstrumentPanel_Normal.png | Textures/AvionicsInstrumentPanel_MetalRoughAO.png |
| RoboticSafetyRedWarning | 1024×1024 | Textures/RoboticSafetyRedWarning_Albedo.png | Textures/RoboticSafetyRedWarning_Normal.png | Textures/RoboticSafetyRedWarning_MetalRoughAO.png |
| OrbitalThermalHologram | 1024×1024 | Textures/OrbitalThermalHologram_Albedo.png | Textures/OrbitalThermalHologram_Normal.png | Textures/OrbitalThermalHologram_MetalRoughAO.png |

Packed channel meanings: **R metallic, G roughness, B ambient occlusion**. Convert to correct project shader convention before use.



## ANNEX K — ART DELIVERABLE STATUS AND NON-RUNTIME VALIDATION

Source: `Verification_Report.json`. Its checks are structural only. Exact source check summary:

| Check | Source reported result | Details |
|---|---|---|
| 82 independent FBX files | PASS | 82 |
| 82 corresponding GLB files | PASS | 82 |
| one assembled alternative FBX and GLB | PASS | — |
| manifest matches the exact 82 files | PASS | — |
| binary FBX 7400 structural headers and objects | PASS | 83/83 |
| all GLB 2 geometry imports and exact triangle totals | PASS | 82/82, 122186 tris |
| nonempty triangulated meshes in every GLB part | PASS | — |
| manifest individual triangle sum correct | PASS | 122186 |
| 64 unique announced animation pivots | PASS | — |
| all 64 pivots physically present as GLB nodes | PASS | — |
| 230 unique decorative socket art anchors | PASS | — |
| all 230 SOCKET_P### art transforms present in GLB files | PASS | — |
| 152 sequential, unique conceptual WIDs | PASS | — |
| 304 uniquely named independent wire-end contacts | PASS | — |
| wire art anchor references exist | PASS | — |
| W129 trusted feedback exact model pair and analog signal | PASS | — |
| only W129 marked critical | PASS | — |
| 6 exact planned objective definitions | PASS | — |
| 36 procedural reference PNG textures | PASS | — |
| 4 optional Unity C# art helper source files | PASS | — |
| no runtime or Unity-Play-Mode testing claim | PASS | — |
| full showcase GLB geometry import and total triangles | PASS | 122186 |

**Not run / not included by source:** Unity compile, Play Mode, functional circuitry, SparkTerminal creation, robot dynamics, coolant simulation, authoritative objective evaluator, final AAA UV/collider/LOD production.



---

**FINAL INSTRUCTION TO IMPLEMENTING ENGINEER:** Build the real level from the actual imported project and package source. On completion provide source/prefab assets, the generated reconciliation and test reports, the known limitations and exact PASS/FAIL/NOT-RUN status. Never describe procedural art manifests as proof of functional robotics. Only the verified real `LevelGamePlayManager` 6/6 state after a correct full W129 recovery qualifies the level as complete.
