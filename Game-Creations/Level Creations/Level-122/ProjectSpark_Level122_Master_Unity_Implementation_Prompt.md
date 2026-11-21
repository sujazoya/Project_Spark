# PROJECT SPARK — LEVEL 122: THE AUTONOMY SENTINEL
## ANX-30 — MASTER UNITY 6.3 URP IMPLEMENTATION PROMPT

**Category:** Advanced Robotics / Autonomous Navigation, Sensor Trust & Independent Motion Safety  
**Level:** 122 | **Board:** ANX-30 | **Physical PCB:** 560 × 420 × 2.4 mm | **Inspection stack:** 36 layers  
**Source art:** 352 independent procedural OBJ/GLB model pairs, plus combined assembled reference (353rd pair), 296 component models, 56 structural/inspection models, 42 procedural PBR families.  
**Design tasks:** 384 player copper-route plans, 148 via plans, 6 objectives, 3 hardware faults and one removable safety-feedback jumper **W385**.  
**Status:** **ART KIT + IMPLEMENTATION SPECIFICATION ONLY**, NOT a finished Unity gameplay project. No physical pin-mapped schematic or Unity source APIs were provided to this package author. All routes/vias remain unverified and certifications are initially disabled.

> **Directive to the implementing Unity/Codex agent:** Act as a senior Unity 6.3 LTS URP developer, game designer, robotics engineer and technical artist. Implement a genuinely playable, evidence-gated Level 122 inside the *existing* Project Spark project. Inspect the actual existing `LevelGamePlayManager`, `SparkCircuitSystem`, `SparkTerminal`, electrical solve context, selection/mover, prefab/level/spawn systems, shop/shelf/throw interaction systems, and existing world-space virtual repair UI **before** writing adapters. Never invent API names or claim Unity tests passed unless you executed them. Preserve all Levels 1–121 and assets/GUIDs. Reject missing schematic authority instead of turning illustration or UI values into progress.

## 01 — Mission identity & player promise

**Name:** The Autonomy Sentinel — *When confident navigation becomes dangerous*.  
A safety-critical autonomous inspection robot is supposed to navigate a cluttered industrial facility while integrating dual GNSS (for outdoor transitions), radar ranging, redundant inertial sensing, map localization and independent obstacle supervision. A visually healthy motherboard may still make hazardous motion decisions because a clock, RF feed or inertial reference is wrong. The mission teaches the difference between **sensing, trusting, localization confidence, route planning and independent motion authorization**.

**Player starts:** 0/296 physically installed, 0/384 routes complete, 0/148 vias complete, W385 physically OPEN, GNSS A/B unqualified, radar A/B unqualified, IMU channels unqualified, safety-motion permit FALSE and objective counter **0/6**. The separate assembled reference is editor/showcase art only and is NEVER a shortcut to install components or qualify sensors.

**Educational scope:** bounded deterministic models of navigation, uncertainty, obstacle margins and watchdog behavior, not a real autonomous-driving stack, aviation safety controller, ISO-certified robotics system, RF design, or manufacturer-approved PCB.

## 02 — 3D import contract & generated assets

- The original board model is `Models_OBJ/297_L122_PCB_Main.obj`, geometry in **metres, Y-up** and outer bounds **0.560 × 0.420 m** on X/Z. Substrate thickness is **0.0024 m**. Do not infer measured PCB stack-up from the 36-layer illustration meshes.
- There are **352 manifest rows**, with unique `asset_id`; 296 `installable=TRUE` component entries and 56 `FALSE` structural/inspection items. The 36 layer-reference meshes are **disabled during gameplay**.
- The 353rd standalone pair `353_L122_AssembledReference.obj` / `.glb` is an actually combined **complete motherboard visual**, including all 296 component meshes, four heatsinks, six antenna reference structures, four shield covers and inspection artwork. Keep that assembled asset **inactive or showcase-only** in the gameplay scene.
- Each OBJ has its own `.mtl` file and planar UV coordinates; generated `_BaseColor`, `_Normal`, `_AO`, `_MetalRoughAO` and `_URP_MetallicSmoothness` maps use 42 material families. Review actual Unity import normals/UVs/sRGB/metallic-alpha mapping, shadows and LODs.
- Do not manufacture OBJ/FBX from imaginary filenames such as `NOT_INCLUDED`. Resolve **exact `.obj` filenames from `Data/ModelManifest.csv`**. The level uses no FBX files. Never reuse a count from L112/L114/L121.
- The board is large at physical scale. For visual showcase ONLY, scale uniformly to approximately **10.13 × 7.60 Unity units** on the existing **15.79 × 8.54-unit tabletop**. Keep all solver/sensor units physical in a separate coordinate basis.
- All physical component anchor coordinates in `ComponentPlacements.csv` are **procedural placement hints**; manufacturer footprints, ball-map pin IDs, thermal limits, shield RF effects and actual pad coordinates are NOT certified.

## 03 — Content hierarchy & scene assembly

```text
L122_ANX30_THE_AUTONOMY_SENTINEL
  VisualArtRoot_Metres
    EmptyPCB_Physical_560x420mm
    36_InspectionLayerReferences_DISABLED
    OtherUnverifiedFactoryArtwork_DISABLED
  GameplayAuthoringRoot_NONCERTIFIABLE
    296_UninstalledComponentTargets
    384_Routes_UNASSIGNED_BLOCKED
    148_Vias_UNASSIGNED_BLOCKED
    AutonomyAcquisitionZones_UNBOUND
    InventoryPrefabs_AsAssets_NotYetInstalled
  ProjectSparkSystemBindings_REQUIRED_UNBOUND
  OPTIONAL_AutonomyLocalization_Teaching_DEMO_NONCERTIFIABLE
  Assembled_ART_REFERENCE_DISABLED_NEVER_GAMEPLAY
```

Required playable additions, **after real APIs and approved pins are available**: mission coordinator, component install authority, validated netlist importer, GNSS/radar/IMU/clock/power adapters, bounded localization engine, motion planner/obstacle verifier, independent safety supervisor and objective-evidence bridge. Keep all functional adapters additive and Level-122-scoped. Do not modify global project scripts for convenience when an existing adapter or level binding will work.

## 04 — Electrical authority, model and placement validation

Define approved schema fields `board_id`, `component_ref`, `manufacturer_part`, `package`, `footprint_id`, `pin_1_orientation`, `pad_id`, `physical_package_pin`, `net_id`, `layer_id`, `via_span`, `net_class`, `clearance`, `trace_width`, `copper_geometry`, `power_domain`, and `source_revision_sha256`. Verify all one-to-one against source manifests.

Every component must have unique stable ref ID, footprint tolerances, orientation, individual socket/terminal targets, damage/repair state and installation evidence. Installation requires correct inventory part, position, orientation, contact and solder/connection authority; importing finished artwork cannot set install state. Do not award Objective 1 because 296 child transforms exist.

For **W001–W384**, require approved exact source/destination pins, net, legal layer, connection class and an actual graph change. Each remains `UNASSIGNED` in the provided source. For **V001–V148**, require approved net ID, start/end layer and true via type; each remains `TBD`/`UNASSIGNED`. A decorative trace or copper-layer reference cannot complete any route or via. Reject shorts, bad ground reference, power domain bridging, unauthorized feedback bypass, invalid differential/RF impedance and unsafe layer transitions.

### Explicit current blocker

`CopperRoutes_384_REQUIRES_SCHEMATIC.csv` is **384/384 unapproved**, and `ViaTasks_148_REQUIRES_SCHEMATIC.csv` is **148/148 unapproved**. The complete board is a plausible visual teaching prop, not a fabricated circuit schematic. Maintain `certifiable=false` and `verified=false` until schematic review; Objective 2 **BLOCKED**.

## 05 — Functional motherboard zones

1. **U1 / U2 / U3:** navigation accelerator, time-alignment FPGA and path-planner CPU; separate measurement, motion and planning evidence.
2. **U6 / U7:** redundant GNSS receivers; independent signal quality, RF integrity, time validity and spoof/jam suspicion.
3. **U8 / U9:** radar acquisition processors; independent range confidence, PLL phase noise, radar-contact validity and obstacle gating.
4. **U10 / U11:** primary and backup inertial references; measured gyro bias, acceleration, timestamp monotonicity and divergence handling.
5. **U17 / U18:** reference clock distribution and PLL with explicit drift/jitter evidence; never qualify a signal from a static text label.
6. **U4 / U5 / U13 / U14 / U24:** independent safety supervisor, trusted readiness feedback, two watchdogs and safe-motion arbiter. Protected motion is vetoable independently of the planner.
7. **U15 / U16:** controller communication interfaces; validated command age, watchdog timeouts and remote-stop handling.
8. **Four heatsink and six antenna reference structures:** visually independent, not presumed active RF/thermal devices until the approved parts and contacts are known.

## 06 — Navigation, sensor data and bounded deterministic physics

Use one authoritative snapshot timeline with deterministic time steps; do not run real-time physics at radar/GNSS radio sample rates. Suggested states: `Disabled → PowerCheck → SensorWarmup → TimeSync → LocalizationValid → PlanningReady → MotionAuthorized → Degraded → SafeStop → FaultLatched`, with deliberate arming and reset transitions.

### Dual GNSS

Model per receiver: antenna/contact continuity, RF signal availability, received power proxy, timestamp, fix status, satellite geometry proxy, error bound/covariance, innovation against inertial/radar measurements, stale-sample limit, spoof/jam flags. Dual agreement is required by policy; identical reported coordinates do not prove authenticity. A GNSS failure may degrade outdoor localization, but must not arbitrarily destroy unrelated indoor radar/IMU evidence.

### Radar

Track PLL qualification, phase noise/jitter proxy, chirp timing, cross-channel range consistency, probability-of-detection proxy and bounded false-positive/false-negative probabilities. A radar fault must alter the modeled source state and safety assessment, never just display a red icon.

### Redundant IMU

Track gyro bias, accelerometer bias, drift, temperature proxy, sampling integrity, monotonic timestamps and A/B disagreement. Fuse only current qualified measurements. Recalculate uncertainty after repairs; never reuse stale healthy covariance. Divergence triggers the independent safety gate as required by an approved policy.

### SLAM and obstacle response

Use a deterministic educational filter (e.g. simplified EKF-like update) with explicit process/measurement variance and revisioned inputs. Publish position estimate, velocity, heading, uncertainty ellipse, tracked obstacles, projected stopping distance, minimum safe clearance, stale sample age and reason codes. Include mixed indoor/outdoor paths, GNSS loss, temporary radar occlusion, multiple obstacle speeds, command latency and safe geofence boundary handling. **Independent safety authorization shall never be inferred from a plan visualization alone.**

Define user-visible messages for `RF_FEED_OPEN`, `RADAR_PLL_UNQUALIFIED`, `IMU_DIVERGENCE`, `SENSOR_STALE`, `LOCALIZATION_UNCERTAIN`, `STOPPING_DISTANCE_EXCEEDED`, `GEOFENCE_VIOLATION` and `SAFETY_LINK_OPEN`.

## 07 — Protected power, clock and independent safety

Model approved power rails, grounds, power-on sequencing and qualified clocks as separate authoritative states. Sensor presence ≠ clean power ≠ valid measurements ≠ good localization ≠ motion permit. The independent safety supervisor must use current qualified evidence and veto motion when samples are stale, uncertainty is high, obstacle clearance is inadequate, feedback is open, or watchdog age expires.

A safe-stop outcome must inhibit the **motion authorization path**, not fictitiously remove every supply rail or wipe previous measurements. Require explicit system-level resume policy after service; do not auto-resume on a single green UI status. Every safety transition must be timestamped, replayable and bound to a revision of the topology and measurement evidence.

## 08 — Three recoverable hardware faults

**F1 — Radar PLL phase-noise drift.** This changes an approved oscillator, distribution connection or clock-conditioning element, increasing measured phase noise and degrading radar range quality. Diagnose using the existing virtual oscilloscope/frequency monitor and radar confidence panel. Repair the exact authorized hardware target, obtain new clock evidence, radar traces, localization and obstacle-stop tests.

**F2 — GNSS-A RF-feed via intermittency.** A specific approved RF via/connectivity segment develops deterministic contact dropout. Diagnose through layer view, continuity and GNSS A/B disagreement or RF proxy. Repair the *actual via/contact state*, then rerun fix integrity, spoof/jam guard and positioning tests; GNSS B cannot magically authorize motion alone.

**F3 — Redundant IMU reference mismatch.** A verified physical reference/supply or I2C/SPI timing defect causes disagreement between the independent inertial channels. Diagnose both sensor traces and independent reference measurements; repair the exact cause, then requalify inertial agreement, dynamic motion and obstacle safety. Do not pass because a filtered graph smooths out the divergence.

Fault injection must be staged only after relevant baseline qualification; deterministic seed, specific source revision, discovery, repair target ID, tool used, actual electrical mutation, fresh verification generation and objective history are recorded. No direct “Repair Complete” UI button can clear any fault by itself.

## 09 — W385 critical safety feedback

`Data/W385_Recovery.json` defines the logical path **U5.TRUSTED_LOCALIZATION_READY_OUT → U4.TRUSTED_LOCALIZATION_READY_IN**, initially **OPEN**. Its physical package pins and net ID are **UNASSIGNED**. U5 and U4 roles are a proposed functional design, not manufacturer-certified part numbers.

Once the player has completed the preceding valid tasks, stage the controlled exercise: arm a previously qualified trusted status → physically remove W385 → safety graph detects loss of trusted feedback and independently inhibits motion → inspect actual open circuit → reinstall/repair authorized jumper → repeat new sensor, localization and obstacle-response evidence → explicitly reset supervisor → verify renewed authorizations and qualify Objective 6. Validate a transition/freshness handshake and stuck-high faults, not just logic `HIGH`.

If W385 physical mapping remains unresolved, objective 6 is blocked. The model must not award a certificate for removing and replacing a decorative jumper.

## 10 — Existing world-space virtual repair UI, no duplicate room equipment

Reuse Project Spark’s existing world-space virtual PCB repair UI rather than importing separate physical instrument objects. Extend inspector panels with **GNSS A/B integrity, radar sweep confidence, inertial comparison, clock/PLL trace, localization uncertainty, obstacle clearance, motion-safety state, layer viewer, and fault/event journal**. A selected tool and a click/touch raycast must resolve an exact contact/part/trace target and enforce a compatible operation. Only an approved state mutation triggers VFX and subsequent solver re-evaluation.

Keep holographic overlays restrained and readable under the existing theme. Instrument traces from the classroom demo must be clearly labeled **ILLUSTRATIVE / NOT CONNECTED TO PROJECT SPARK SOLVER**. Rendered board art must never block actionable 3D colliders, and decorative Canvas panels cannot intercept all pointer input.

## 11 — Six authoritative objective contracts

| Objective | Evidence source to earn it | Must not pass if |
|---|---|---|
| O1 — Precision Assembly | All 296 independently installed, verified parts/footprints/pin-one/orientation/contacts | Assembled reference was simply activated; wrong part, unsoldered pins or generic footprint |
| O2 — 36-layer routing | 384 approved routes + 148 approved legal vias, verified solver continuity and design rules | `UNASSIGNED` pad/net, `TBD` via spans, decorative copper or short |
| O3 — Sensor commissioning | Clean protected power, clocks, dual GNSS, dual radar, dual IMU and safety watchdogs | Presence alone, stale samples, unapproved clock or single sensor success |
| O4 — Safe navigation certification | Fresh localization uncertainty, timed obstacle stop, geofence, command-age and independent safety trials | A pretty path, static confidence value, outdated trials or unsafe braking margin |
| O5 — Recover three faults | Distinct F1/F2/F3 authoritative repair history, new sensor/localization/obstacle evidence | Cosmetic repair, unresolved hardware or cached clean traces |
| O6 — W385 independent recovery | Actual feedback open/latched inhibit, repaired continuity, fresh full evidence, explicit reset | W385 OPEN, pins unresolved, stuck-high, missing new safety proof |

Start at **0/6**. Integrate into the existing authoritative `LevelGamePlayManager` only through its *actual inspected* interfaces; never directly mutate an objective label or invent a completion API. Past earned milestones and current qualified operating health are separate values.

## 12 — Mission flow / pacing

**A. Inspection & assembly** — New bare ANX-30 on tabletop; BOM tray, component highlights, repair UI. Each of 296 parts must be installed deliberately. Showcase model in separate inactive root.

**B. Routing & vias** — 36 transparent inspection layers, 384 selected copper tasks, 148 via targets. Only validated net/pad data may drive active UI interactions; preview placeholders remain locked.

**C. Sensor startup** — Bring up protected supplies, PLL, GNSS, radar and redundant IMUs with clearly differentiated power/signal/fix statuses.

**D. Navigation trials** — Outbound path, intermittent satellite view, radar obstacle crossing and geofence. Compare covariance against real simulated movement; timed stop trials at different speeds.

**E. Three separate repairs** — F1 radar clock, F2 GNSS via, F3 redundant IMU reference; require actual hardware mutation and new independent tests after each.

**F. W385 critical safety** — Controlled feedback removal, guaranteed protected motion inhibit, repair, fresh sensor/navigation evidence, supervisor reset. Only verified evidence permits 6/6.

## 13 — Save/load, determinism, anti-bypass

Save format `L122_v1` with source manifest SHA-256, approved schematic revision, component install/contact states, actual electrical topology graph, approved route/via geometry, sensor fault seeds and contact schedules, simulator time, localization map revision, evidence-generation counters, safety latch and W385 event history. On load reject unexpected schema changes and stale objective evidence; allow history display but require requalification for current health when graph or sensor generation changes. Reject fake UI increments, unchanged graphical repairs, wrong hardware target, static HIGH W385 spoof and assembled-reference activation.

## 14 — Unity integration/performance requirements

Use Unity 6.3 LTS URP with uGUI/TextMeshPro and New Input System consistent with existing project. Avoid `FindObjects...`, LINQ allocations or project-wide scans in Update. Cache actual service references, use event-driven invalidation and fixed deterministic bounded simulation ticks. Keep UI refresh independent of solver tick rate, pool localized VFX, use instancing and LOD without sharing independent safety colliders. Keep input mouse/touch and PC/mobile supported. Do not change other level's assemblies or assets. Build menu needs to be idempotent and save stable project-local paths.

**Editor menus supplied by this kit** (visual only):

- `Tools/Project Spark/Automatic Level Generator/122/3. Validate Sources and Report Blockers`
- `Tools/Project Spark/Automatic Level Generator/122/4. Rebuild URP PBR Materials`
- `Tools/Project Spark/Automatic Level Generator/122/5. Create COMPLETE assembled motherboard (VISUAL ONLY)`
- `Tools/Project Spark/Automatic Level Generator/122/1. Build FULL Level_122_Layout`
- `Tools/Project Spark/Automatic Level Generator/122/2. Place assembled VISUAL preview on SELECTED tabletop`
- `Tools/Project Spark/Levels/Create Level 122` (visual authoring only; does not create the complete gameplay)

**Output paths:** `Assets/ProjectSpark/AutoGeneratedLevels/Level122/Level_122_Layout.unity`, `Level_122_Layout.prefab`, `L122_CompleteMotherboard_VISUAL_ONLY.prefab`, `L122_SourceBlueprint.asset`, `URP_PBR_Materials`, `ComponentVisualPrefabs` and `L122_Layout_Validation.txt`.

## 15 — Approved external engineering inputs required

1. All component package choices, actual values, manufacturer pin maps, physical footprint and orientation specifications.
2. GNSS RF antenna connector and feed contacts, approved clock, return and per-receiver measurement authority.
3. Dual radar front-end, PLL, antenna and signal-conditioning pin-level design; approved phase-noise fault location.
4. Redundant IMU and isolated safety reference mappings, sampling protocol and physical F3 repair target.
5. Protection, independent safe-motion gate and W385 full physical schematic including known source/destination pins.
6. Exact W001–W384 source/destination pads, net/layer/class/geometry, factory vs player copper requirements.
7. Exact V001–V148 net IDs, layer spans, via types and design-rule tests.
8. Bounded environment / map / obstacle scenario datasets, motion-stop policy and acceptable risk margins.
9. Sensor uncertainty, timeouts, fusion tuning and independent safety criteria approved for the educational game.
10. Actual Project Spark `LevelGamePlayManager` and repair/circuit/inventory APIs and scene binding contracts.

Until supplied, **BLOCKED: UNSIGNED NAVIGATION/SAFETY ELECTRICAL SCHEMATIC**; do not fabricate missing source pins or grant certificates.

## 16 — Required QA evidence & final report

Deliver a compile and import log, exact scene/prefab paths, actual ModelManifest to asset mapping, Unity EditMode tests, Unity PlayMode tests, 100 acceptance test results, objective authority source revision/hash, each fault's hardware state proof and recovery, W385 safety proof, performance numbers on PC/mobile, regression smoke tests for Levels 1–121, unresolved blockers, and demo-vs-real evidence labels. An offline source-validator PASS is **not** a Unity compile or Play Mode PASS.

---
## ANNEX — Original Level 122 artifact inventory

`Data/ModelManifest.csv`, `ComponentBOM.csv`, `ComponentPlacements.csv`, `CopperRoutes_384_REQUIRES_SCHEMATIC.csv`, `ViaTasks_148_REQUIRES_SCHEMATIC.csv`, `W385_Recovery.json`, `FaultPlan.json`, `Objectives.json`, `CertificationGates.json`, `ExportSummary.json`; 352 modular OBJ/GLB pairs and one assembled pair; 42 material families with BaseColor, Normal, AO, MetalRoughAO and URP metallic/smoothness masks; four level-scoped Unity C# scripts, independent preview renders and offline QA manifest.

