# PROJECT SPARK — LEVEL 124: THE ORBITAL BASTION
## Complete AAA Gameplay Implementation Specification — Unity 6.3 URP

**Board:** OBX-32 • **Domain:** Spacecraft Avionics, Attitude Determination & Control (ADCS), Fault Detection/Isolation/Recovery (FDIR) • **Physical PCB:** 600 × 460 × 2.8 mm • **Visual inspection layers:** 40 • **Modular models:** 384 OBJ/GLB pairs (328 installable + 56 structural/inspection) • **Separate assembled visual motherboard:** `385_L124_AssembledReference.obj/.glb` • **PBR material families:** 46 • **Construction:** W001–W416, V001–V164 • **Removable trusted link:** W417 • **Objectives:** six • **Faults:** F1/F2/F3.

> **Directive to the Unity implementation engineer:** Build an excellent, production-oriented Project Spark Level 124 using the **actual existing Unity repository and services**. Inspect and document the real signatures of `LevelGamePlayManager`, `SparkCircuitSystem`, `SparkTerminal`, `SparkElectricalNetworkGraph`, existing power supply and test instruments, selection/placement/shop/shelf/throw/wire tools and world-space repair UI before adding adapters. Preserve Levels 1–123. Never invent pre-existing APIs, hardwire the goal counter, or imply that a mesh is an electrical connection. All electrical, actuator and spacecraft safety facts must be backed by authoritative, versioned source data and executable proof.

> **Package boundary:** This deliverable contains real procedural OBJ/GLB meshes, UVs, MTLs, textures, a separate completely assembled motherboard model, Unity visual-generation scripts, manifests, non-scoring teaching code and QA plans. It does **not** provide a qualified pin-level 40-layer schematic, validated footprints, hardware flight models, space-grade component selections, approved spacecraft dynamics or Unity Play Mode evidence. The technical art and level concept are provided; **full authoritative AAA gameplay is blocked until those sources are approved and integrated**.

## 01. Preservation, source inspection and acceptance gate

1. Create version-control branch or complete project backup. Install exclusively under `Assets/ProjectSpark/Level124/OBX32`, preserve existing `.meta` GUIDs and all previous scenes and materials.
2. Validate 384 manifest records (and 385 GLB, 385 OBJ and 385 MTL files including the assembled reference); exact lowercase `.obj` and `.glb` names from CSV, no `NOT_INCLUDED` placeholders.
3. Validate 328 one-to-one BOM and placement records, each `installed_at_start=FALSE` and `footprint_verified=FALSE`, and 56 noninstallable source structures; W417 is part of 328 and is **not** an extra component.
4. Assert physical mesh footprint X=0.600 m, Z=0.460 m and substrate thickness Y=0.0028 m. Import Y-up, metre units. Assert same X/Z bounds for the combined reference, which excludes the 40 inspection-layer meshes.
5. Validate 46 BaseColor/Normal/AO/MetalRough material sets and 46 URP metallic/smoothness maps. Remap actual material slots, not random colors by object name.
6. Validate all 416 route tasks have `UNASSIGNED` source/destination/net and `certifiable=FALSE`; all 164 vias have net `UNASSIGNED`, `TBD` layer spans and `verified=FALSE`.
7. Treat W417 logical endpoints `L124_U5.TRUSTED_ATTITUDE_READY_OUT` and `L124_U3.TRUSTED_ATTITUDE_READY_IN` as *planning labels*. They are **not manufacturer-qualified pins**. Default `OPEN`, 0/6 objectives and actuator authorization disabled.
8. Generate a source audit and Unity console import report, without falsely reporting compilation or Play Mode success.

## 02. Complete mainboard + empty gameplay scene

The finished visual board is the principal showcase asset. Its assembled mesh includes all 328 component visuals and all eight thermal-spreader structures, plus visible surface test/connector, shielding and copper artwork. Keep it separate from the unassembled mission shell to avoid bypassing construction objectives.

```text
L124_OBX32_THE_ORBITAL_BASTION
  VisualArtRoot_Metres
    EmptyPCB_Physical_600x460mm
    40_InspectionLayerReferences_DISABLED
    OtherUnverifiedFactoryArtwork_DISABLED
  GameplayAuthoringRoot_NONCERTIFIABLE
    328_UninstalledComponentTargets
    416_Routes_UNASSIGNED_BLOCKED
    164_Vias_UNASSIGNED_BLOCKED
    OrbitalAvionicsAndAttitudeZones_UNBOUND
    InventoryPrefabs_AsAssets_NotYetInstalled
  ProjectSparkSystemBindings_REQUIRED_UNBOUND
  OPTIONAL_AttitudeControl_Teaching_DEMO_NONCERTIFIABLE
  Assembled_ART_REFERENCE_DISABLED_NEVER_GAMEPLAY
```

`L124_CompleteMotherboard_VISUAL_ONLY.prefab` is a separate artist reference. It must never report installed components or certified nets. Keep the source at physical 600 × 460 mm; create an **isolated presentation-scale preview** at around 10.13 × 7.76 Unity units on the existing workshop tabletop (15.79 × 8.54 units). Do not rescale solver physical units.

Visual direction: dark aerospace-blue solder mask, satin anodized aluminum spreaders, controlled gold interconnects, fine copper and white silkscreen, readable power/compute/ADCS zoning, radiation-shield cans, cool teal telemetry indicators, restrained warning amber. Consistent physically plausible packages, subtle roughness, adjustable bloom, high-quality surface normals, premium lighting baked only for art where appropriate. Existing room environment and virtual instrument canvas are reused.

## 03. 328-component installed-state contract

**BOM inventory:** 94 capacitor, 9 clock, 21 connector, 15 diode, 30 ic, 22 inductor, 1 jumper, 1 led, 32 mosfet, 91 resistor, 8 sensor, 4 switch.

Each component requires a unique `ref`, board ID, type, approved manufacturer/package/part rating and footprint/rotation, verified solder/pad identities, health, install/uninstall event, target collider and inventory provenance. Only the actual project placement/connection system may grant completion; visual-only prefabs cannot change state. Reject wrong value, package, orientation, pin one, board, power rating, incorrect solder or conflicting terminal. Do not manufacture package footprints from position hints. W417 remains physically disconnected at scenario start.

## 04. 40-layer topology authority and PCB QA

Provide revisioned data for `component_ref`, `package_pin`, `pad_id`, `terminal_id`, `net_id`, `layer_id`, `net_class`, `voltage_domain`, `segment_geometry`, `is_factory_copper`, `allowed_via_span`, `clearance`, `return_plane`, and `has_electrical_authority`. Validate every selected route **W001–W416** and via **V001–V164** against those definitions and the existing solver. A decorative copper trace or via mesh has no topology effect. Short/open/insufficient return continuity, illegal blind/buried span, power-domain isolation and undesired actuator enable must be rejected and logged. Do not falsely require 416 to equal every conductor on the board; these are player-selected tasks only.

## 05. Spacecraft avionics electrical architecture

Subsystem roles (proposed, not physical pin facts): U1 primary command/data computer, U2 redundant computer, U3 independent safety/actuator inhibit supervisor, U4 voting/cross-strapping FPGA, U5 trusted attitude-ready feedback monitor, U6/U7 independent star-tracker controllers, U8 sun sensor fusion, U9 reaction-wheel controller, U10 magnetorquer interface, U11 propulsion valve authorization and U12 clock supervisor. Retain the rest of the BOM roles in `Data/ComponentBOM.csv`.

Model independent spacecraft avionics power and actuation supply eligibility: protected battery bus, solar charge/power transfer, regulated compute/trackers, isolated wheel drive rails, isolated propulsion valve controls and telemetry-only safe-mode power. Require current/voltage ranges and component ratings from approved design data; invent no real spacecraft pinout. A computer's assertion of healthy status must not override an independent safety latch. Do not energize an actuator without all prerequisites qualified.

## 06. ADCS mission design and deterministic teaching model

Provide two redundant star-tracker pipelines, gyro/inertial sensing, sun sensors, attitude estimation with documented quaternion/frame convention, four reaction wheels and magnetorquer desaturation; a thruster interface is optional and independently inhibited. Compare **sensor detection → power → valid synchronized frames → calibrated state estimate → safety authorization → bounded actuator response** as distinct states.

Implement a deterministic numerical educational model at a bounded time step for attitude acquisition, slewing to target, reaction-wheel saturation/desaturation, disturbance torque, sun/eclipse changes, pointing confidence, wheel currents and safe-mode recovery. Expose input conditions and event-generation IDs. No simulated physical events may control actual Unity inventory, terminal graph or authoritative objectives. A visual star-tracker confidence gauge is not verified sensor evidence; a wheel spinning animation is not verified torque. Label all such outputs *illustrative* until connected to approved simulation and independent evidence.

## 07. ECC, radiation upset and watchdog policy

Model recoverable single-event-upset teaching events through explicit error detection, ECC correction limits, scrubbing, watchdog expiry and reset policy. Differentiate a transient corrected bit upset from permanent high-current latch-up; the latter requires power-path isolation and component health treatment. Do not imply an educational model is a radiation/spaceflight certified design. Control software requires source generation/hash, fault replay seeds and safe-mode evidence; no false claim of radiation-tested parts.

## 08. FDIR and three fault challenges

**F1 — Reaction-wheel 3 overcurrent / latch-up:** A schematic-approved wheel-drive or power cross-strap fault makes a real electrical overcurrent condition. The independent current monitor latches actuation off; diagnose with the virtual multimeter/current trace and isolate the faulted component. Clear only after an approved physical-state repair, power requalification and fresh wheel tests. Never simply animate a power surge or pretend an arbitrary Q ref is the fault.

**F2 — Star-tracker B reference-clock via intermittency:** An approved via loses continuity according to a deterministic disturbance pattern; tracking frames become stale or desynchronized. Diagnose with the virtual scope/net tracer, repair the specified plated structure, compare against tracker A, and require new timestamped samples and attitude qualification. Do not guess the actual via ID from planning rows.

**F3 — Reaction-wheel tachometer bias / stale feedback:** A defined wheel sensor/reference path reports inconsistent speed or aged telemetry. Detect by independent estimates/current and appropriate timestamps; require authorized sensor/connection repair and fresh wheel/pointing evidence. Do not auto-repair from a generic calibration button without certified conditions.

Faults have independent IDs, deterministic seeds, serialization, real component/contact state mutation, allowed tools, evidence invalidation and exact repair target. Fault VFX and UI hints are independent of solver state. Keep all three faults BLOCKED until physical targets are mapped.

## 09. W417 independent safety disconnect/recovery

W417 is **trusted attitude-control-ready feedback** between logical U5 and U3 planning labels; it is not a reaction wheel phase, the main battery rail or the independent emergency inhibit itself. Start OPEN and actuators INHIBITED. After a legitimately qualified earlier state, supervised disconnection must break the approved physical signal path, make readiness invalid, latch protected actuator inhibition and preserve unrelated telemetry power. Reconnect through the approved component/contact graph, collect fresh power, star-tracker, wheel and watchdog evidence, rerun pointing/safe-mode tests, then perform explicit supervised reset and verify a new safety generation. Stuck-high and stale tokens must fail. No 6/6 without W417 approved package pins and fresh independent proof.

## 10. AAA workshop UI, interaction and effects

Reuse existing world-space virtual PCB repair UI, selection/rotate/wire/measure/scan tools and input system. Add virtual ADCS state panel, star tracker timing graph, wheel current/speed display, FDIR/event console, ECC/watchdog recorder, thermal/power tracer, net/layer viewer and safety-latch panel. All measurements use real data when bound; otherwise clearly show **NO VERIFIED DATA**. Provide tooltips, high-contrast object picking, accurate raycast target IDs, selection outline hierarchy, mobile touch hit areas and keyboard support. VFX micro-arcs, continuity glow and localized warnings only accompany validated transactions. Do not import physical handheld scopes, extra room props or a fake flight environment.

## 11. Six authoritative objectives

| Objective | Requirement | Cannot pass when |
|---|---|---|
| O1 — Assemble 328 | Every installed component has correct approved footprint, orientation, contact and health | only visual model or unverified footprint |
| O2 — Route 416 + Via 164 | Exact solver topology, correct net/layer/via and verified return paths | UNASSIGNED, TBD, short or art-only trace |
| O3 — Avionics commission | Power, independent reset/watchdog, trackers, wheel controls and safety eligible | missing approved pins or open W417 |
| O4 — Attitude & safe mode | Fresh bounded acquisition, pointing hold, desaturation and emergency inhibit | demo-only model, stale tracker or simulated green icon |
| O5 — Three fault recoveries | F1/F2/F3 independently diagnosed, repaired and requalified | a fault active or stale proof |
| O6 — W417 controlled recovery | Actual open/inhibit, repair, fresh independent tests, explicit reset | unresolved U3/U5 pins, stuck feedback or cached authority |

Start at `0/328`, `0/416`, `0/164`, `0/6`, W417 OPEN, actuators disabled. Existing `LevelGamePlayManager` exclusively awards objectives through its *actual* inspected API. Keep immutable historical achievements separate from current qualified health.

## 12. Save/load, anti-bypass and determinism

Version saves with board/manifest hash, netlist revision, solver graph generation, mission phase, installed parts, approved routes/vias, measured star-tracker/wheel states, corrected ECC events, watchdog reset counts, FDIR repair histories, W417 latch generation and evidence IDs. On load recompute authority and invalidate stale dependent proof. Prevent prefab-showcase installation, arbitrary route completion, duplicate component ref, VFX-only repair, stale sensor evidence, untrusted calibration and inspector changes from granting objectives. Preserve prior-level event subscriptions and unsubscribe correctly.

## 13. Rendering/performance and accessibility

URP Lit uses BaseColor sRGB, NormalMap import and linear metallic/smoothness packed maps (metallic in R, smoothness in A), AO in its channel. Materials must bind to OBJ material slots. Use instancing for numerous passives, pooled localized effects, incremental dashboard updates, layer inspection on demand, and cached references. Avoid large per-frame searches and unnecessary Update loops. Preserve visual/solver scale separation. Acceptable on PC and mobile with configurable graphics quality and readable safety text.

## 14. Unity generator and file paths

Run **Tools → Project Spark → Automatic Level Generator → 124**:

1. `1. Build FULL Level_124_Layout` — validates exact model paths and counts, creates real-scale empty board, 328 uninstalled targets, 416 blocked routes, 164 blocked vias, visual inventory prefabs, inactive inspection artwork, disabled assembled reference, source ScriptableObject, isolated `.prefab` and `.unity` scene.
2. `2. Place assembled VISUAL preview on SELECTED tabletop` — select existing `Table_top`; creates an unscored, uniformly scaled complete 3D board preview.
3. `3. Validate Sources and Report Blockers` — source consistency and explicit physical-certification blockers.
4. `4. Rebuild URP PBR Materials` — creates/rebuilds all 46 URP Lit material assets and binds material families.
5. `5. Create COMPLETE assembled motherboard (VISUAL ONLY)` — saves visual-only prefab with all 328 component visuals and eight heat spreaders, without changing gameplay.

Also `Tools → Project Spark → Levels → Create Level 124`. Expected assets under `Assets/ProjectSpark/AutoGeneratedLevels/Level124/`. Preserve all existing project files and GUIDs. Prompt before abandoning unsaved open scenes. Import preflight must reject missing models and incorrect dimensions instead of creating a generic substitute.

## 15. Required schematic reconciliation package

A) 40-layer approved stack-up; B) 328 component package/footprint/values with pin-one and orientation; C) exact source/destination pads/net/layer for W001–W416; D) via V001–V164 drilled/plated geometry and legal spans; E) verified U3/U5 W417 physical pins; F) reaction-wheel power driver/current monitor actual pins and F1 fault; G) star-tracker B reference path and F2 via ID; H) tachometer channels and F3; I) protected battery/solar/telemetry rails; J) independent inhibit paths, actuator authority rules and bounded ADCS acceptance thresholds; K) existing real Project Spark C# API signatures. Until signed, report **BLOCKED: UNSIGNED ELECTRICAL SCHEMATIC**, never invent connectivity.

## 16. QA and release quality gates

Stage 1: Unity import and compiler clean. Stage 2: signed schematic and 40-layer physical validation. Stage 3: placement, solver, topology and actual virtual tool interactions. Stage 4: bounded ADCS/electrical/FDIR behavior with independent safety evidence. Stage 5: F1/F2/F3 and W417 recovery anti-bypass tests. Stage 6: all six objectives and Levels 1–123 regression, plus PC/mobile rendering QA. Log Unity version, exact source asset paths, manifest hash, test evidence and all open blockers. The supplied 100-case acceptance matrix begins `NOT_RUN` / `BLOCKED` and is not claimed as Unity-executed.
