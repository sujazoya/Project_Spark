# PROJECT SPARK — LEVEL 123: THE FLIGHT SOVEREIGN
## Master Unity 6.3 URP Implementation Prompt — AAA Flight Control & Functional Safety

**Board:** AFX-31 • **Classification:** Eight-axis flight-controller / avionics educational motherboard • **Physical dimensions:** 580 × 440 × 2.6 mm • **Inspection layers:** 38 • **Visual kit:** 368 modular OBJ+GLB models (312 installable parts and 56 structural/inspection models), plus `369_L123_AssembledReference` OBJ+GLB • **PBR families:** 44 • **Route tasks:** W001–W400 • **Via tasks:** V001–V156 • **Safety jumper:** W401 • **Objectives:** 6 • **Hardware faults:** 3.

> **Implementation directive:** Act as a senior Unity, embedded controls, circuit simulator, technical-art, robotics and quality engineer. Build a stable, engaging, AAA-quality Level 123 using the **actual existing Project Spark repository**. Before integration, inspect the real signatures of `LevelGamePlayManager`, `SparkCircuitSystem`, `SparkTerminal`, component placement, virtual tool UI, inventory, selection, and repair services. Never invent a pre-existing API. Keep Levels 1–122 intact. Use separate, additive adapters and stage every integration behind compilation/tests.

> **Source/design boundary:** This package provides original procedural meshes, UVs, PBR starter maps, art-placement hints and **unverified** task definitions. It does **not** contain an approved 38-layer pin-level schematic, qualified avionics control software, manufacturer-certified footprints, flight-rated ESC hardware data or a certified fail-safe architecture. All electrical and safety objectives remain **BLOCKED: UNSIGNED ELECTRICAL SCHEMATIC** until approved connectivity and test authority are supplied. The 8-channel flight model is an educational bounded simulation; do not imply real airborne safety certification.

## 01 — Source preflight and preservation

1. Back up Unity project and preserve existing `.meta` GUIDs, scenes, shop/shelf/throw mechanics and Levels 1–122. Source installer touches only `Assets/ProjectSpark/Level123/AFX31`.
2. Validate exactly 368 modular OBJ/GLB pairs; each OBJ has matching MTL, exact filenames per manifest; combined reference is independent, not included in the 368 model count.
3. Validate 312 unique BOM refs, 312 placement rows and 312 `installable=TRUE` source manifest rows. All footprint certifications and installed flags start false.
4. Assert the PCB mesh is X=0.580 m and Z=0.440 m with Y thickness 0.0026 m; Y-up, meters; the complete assembled reference must retain same X/Z footprint.
5. Validate 38 separate inspection-layer artworks; keep inactive in gameplay. The eight motor-stage heatsink objects are **structural**, not added to the 312 installable BOM count.
6. Confirm all 400 routes have `UNASSIGNED` terminals and net IDs with `certifiable=FALSE`; all 156 vias are `TBD` spans and `verified=FALSE`; W401 initially open with unverified physical pins.
7. Import native URP Lit PBR materials; BaseColor sRGB, normal maps NormalMap, AO/metal linear. Ensure material slots and mesh pivots bind correctly in Unity. Never declare Unity compilation or runtime success from Python inspection.

## 02 — Visual composition and scene hierarchy

Create an isolated `Level_123_Layout.unity` with the physical-size empty AFX-31 board; create a separate `L123_CompleteMotherboard_VISUAL_ONLY.prefab` with **all 312** installed model visuals and all eight heatsinks. The reference exists to preview art and must never mutate gameplay truth. The board-on-tabletop preview uses a separate presentation parent scaled uniformly to ~10.13 × 7.68 Unity units and leaves physical solver quantities in metres. No physical handheld instrument meshes or new workshop environment.

```text
L123_AFX31_THE_FLIGHT_SOVEREIGN
  VisualArtRoot_Metres
    EmptyPCB_Physical_580x440mm
    38_InspectionLayerReferences_DISABLED
    OtherUnverifiedFactoryArtwork_DISABLED
  GameplayAuthoringRoot_NONCERTIFIABLE
    312_UninstalledComponentTargets
    400_Routes_UNASSIGNED_BLOCKED
    156_Vias_UNASSIGNED_BLOCKED
    FlightControl8AxisZones_UNBOUND
    InventoryPrefabs_AsAssets_NotYetInstalled
  ProjectSparkSystemBindings_REQUIRED_UNBOUND
  OPTIONAL_8AxisFlightMixing_Teaching_DEMO_NONCERTIFIABLE
  Assembled_ART_REFERENCE_DISABLED_NEVER_GAMEPLAY
```

Use a restrained sci-fi laboratory aesthetic: satin teal solder mask, distinct copper and gold, cool silver connector shells, eight anodized cooling assemblies, subtle safety red, high-contrast pad outlines, localized holographic probing, screen-readable labels, and no exaggerated bloom hiding electrical contacts.

## 03 — Identity-based component assembly

The design includes 28 ICs, 86 resistors, 90 capacitors, 20 inductors, 30 MOSFETs, 14 diodes, eight oscillators, 20 connectors, eight sensors, five switches, two indicators, and one W401 jumper = **312 installable parts**. Use `ref` + `board` as stable keys. Model assets, shop stock, slot anchors and solder authority must be separate state machines. Reject incorrect package, value, board, pin-one orientation, footprint, damaged terminals or unapproved contacts. A visual mesh appearing in a slot is never an installation. Approved footprints and solderable pad geometry remain missing and must be reconciled.

## 04 — 38-layer electrical circuit authority

Require signed revisioned schematic: component ref, package and pin ID, terminal ID, net ID, conductor class, permitted layer, via start/end, plated geometry, return reference, current rating, differential constraints, safety boundary. Factory-fixed copper and player-created route tasks must be distinguishable. Source files intentionally contain 400 unassigned route requests and 156 unverified vias. No routes or vias can be completed with arbitrary visual paths, fake terminals or UI clicks. Electrical topology must report real graph changes through the existing Project Spark solver. Verify power-domain separation, insulation, short-circuit detection, loop closure, return path and permitted via spans.

## 05 — Flight-control hardware architecture

Model two independent flight computers (U1/U2), a separate safety supervisor U3, eight-axis arbitration/timing FPGA U4, and monitor U5. The physical interface concept contains eight motor control outputs J1–J8, high-side/low-side MOSFET and drive stages, current/voltage protection, three independent inertial measurement paths, barometric altitude, dual GNSS, data logging, watchdog timing, redundant command communication and protected rails. These are proposed logical roles, not verified pin mappings. Model load power, controller power, references and returns independently; prohibit gate enable on partial wiring, watchdog disagreement, sensor invalidity or open W401.

Design a **bounded, deterministic eight-motor mixer** for educational use (not actual PWM hardware control). Inputs: target throttle, roll/pitch/yaw, IMU angular rates, modeled inertia, battery state, motor response and wind disturbance; outputs: per-axis effort, current, RPM, attitude, confidence and safety inhibit. Require sign conventions and units, time-step invariance and seeded disturbances; no massive per-frame global object searches. Do not label the demo as flight-stability proof.

## 06 — Sensor fusion and redundancy

Triple IMU model with independent confidence scores, timing alignment, sensor calibration and common-mode failure rejection. A single IMU may disagree without immediately causing fictitious recovery, but authorized control is suspended when the independent voting policy cannot establish trustworthy state. Model pressure altitude and GNSS references independently, avoiding a fake perfect sensor. Measurements should originate in approved component/channel states; absent physical/solver authority = unavailable, not zero, green or pass. Track evidence generations after every repair. The UI must distinguish `Sensor Present`, `Powered`, `Fresh Samples`, `Validated`, and `Used for Control`.

## 07 — Protected power, ESC gating and safety architecture

Power-on sequence: protected battery input -> controller supplies qualified -> reset release -> redundant clocks qualified -> sensor samples fresh -> independent safety vote -> controlled motor gate authorization. The gate-cut safety path must be independent of decorative lights, the main computer's own claimed health, and the demo module. Normal control outputs must not override an independent emergency inhibit. W401 is a **trusted FLIGHT_READY feedback**, not a motor phase wire, not a battery contact and not the emergency-stop switch itself. When W401 opens after arm, the safety latch disables protected motor command authorization; do not artificially zero unrelated supply rails or erase historical data. No physical pin inference until approved schematic exists.

## 08 — Flight-performance certification

Implement deterministic ground-stand or virtual test rig, not real aircraft hardware. Test commanded eight-axis responses, stabilized hover approximation, gust recovery, current limits, overcurrent shutoff, sensor disagreement, controlled descent, landing and emergency cut. Store explicit per-channel sample evidence with source revision and timestamp. A rendered motor spinning does not count as good torque; a green altitude graph does not prove flight dynamics. Acceptance thresholds must be design-authored; do not fabricate standards compliance.

## 09 — Hardware faults and qualified repair

**F1 — Motor channel 5 high-side gate-drive open:** a specified gate-driver/pad or MOSFET contact must physically open the actual drive path, affecting channel 5 current and authority; diagnose with virtual oscilloscope and current probe, repair approved target, retest. Do not guess which of the Q refs is channel 5 without the schematic.

**F2 — IMU B SPI clock via intermittency:** an approved via intermittently opens; sample age and voter confidence change; diagnostic net tracing and time-correlated scope evidence identify it; repair actual via and demand fresh synchronized IMU data.

**F3 — Barometric reference drift:** an approved sensor/reference component drifts, biasing altitude; diagnose against independent sensor evidence, repair or recalibrate only when physically justified; require fresh altitude and flight envelope tests.

All three faults have separate IDs, event generations, reversible authorized repair transactions, saved history and stochastic effects controlled by fixed QA seeds. Art VFX alone cannot change fault or score state.

## 10 — W401 supervised safety recovery

Source planning label `L123_U5.TRUSTED_FLIGHT_READY_OUT`; destination planning label `L123_U3.TRUSTED_FLIGHT_READY_IN`. **These are NOT verified physical pins.** Start W401 `OPEN`, safety not authorized. Objective 6 requires an earlier legitimately certified healthy state; record deliberate removal -> independent all-channel inhibit -> approved physical repair -> fresh power/IMU/altitude/motor tests -> supervised explicit reset -> fresh trusted readiness. A stuck-high static voltage, prior success token, or mere visual reconnection cannot pass. Keep blocked until the exact U3/U5 physical pad mapping is approved.

## 11 — World-space virtual repair UI and interaction

Reuse existing world-space PCB repair Canvas, never add physical instrument meshes. Provide virtual multimeter, 8-channel logic capture, gate and PWM timing scope, simulated attitude and altimeter view, inertial voter comparison, current probe, thermal/power view, trace/layer inspector, fault log, trusted safety status and task history. Mouse/touch rays should hit precise contact colliders; UI should not capture the entire view and hide the PCB. Validate selected tool + target ref/pad + safety preconditions before any repair; local VFX occur only after real state mutation and success. Distinguish illustrative/demo vs true simulation streams.

## 12 — Six objectives (authoritative, fresh evidence)

| Objective | Evidence required | Must remain blocked if |
|---|---|---|
| O1 — Assembly | All 312 verified placements, values, orientation, solder contacts | visual-only parts, unverified footprints |
| O2 — PCB | All 400 exact routes and 156 vias change approved solver graph | missing nets/pins/spans, shorts |
| O3 — Commission | qualified power, reset, clocks, sensors, eight stages and independent interlock | open W401, stale sensors, missing terminals |
| O4 — Flight | fresh bounded attitude, throttle, descent, gust and emergency stop evidence | illustrative-only test or motor animation |
| O5 — Repair | F1/F2/F3 repaired on correct targets, all relevant fresh tests | art-only repairs, stale pre-fault results |
| O6 — Recovery | deliberate W401 open/inhibit, repair, fresh tests and reset | open feedback, stuck signal, unspecified physical pins |

Initial state: `0/312` components; `0/400` routes; `0/156` vias; W401 OPEN; motors INHIBITED; `0/6` objectives. The existing `LevelGamePlayManager` alone owns validated objective completion; do not write new calls before checking real API signatures. Preserve earned historical milestones while reporting current health separately.

## 13 — Save and anti-bypass logic

Version saves with source manifest hash, netlist revision and replay seed. Save BOM installation, motor stage contact health, copper/via authorization, sensor calibration, W401 safety latch, fault history, timestamps, evidence generation and mission phase. On load, reverify current topology and invalidate dependent stale proof without deleting history. Prevent reaching 6/6 via inspector defaults, imported completed-board art, demo mixer, fabricated UI telemetry, disconnected motor stage, stale sensor data or unqualified safety link.

## 14 — Production, accessibility and mobile performance

URP/Lit metallic/smoothness packed textures, instancing for passives, pooled localized repair particles, object ID pick targets, cached references and no LINQ/Find in an Update loop. Physical-scale board art must not be distorted for tabletop presentation. Provide adjustable text, contrast-safe safety warnings, touch hit areas, keyboard navigation, subtle sci-fi interface, predictable state transitions and clear actionable blocker messages. Use 38 layer-reference visuals for inspection only and keep collapsed by default. Present any generated educational flight metrics as illustrative unless tied to approved simulation.

## 15 — Unity builder contract and exact commands

Run `Tools → Project Spark → Automatic Level Generator → 123`:

1. `1. Build FULL Level_123_Layout` — source validation, 44 native URP materials, real board import, 312 empty targets, 400 blocked route records, 156 blocked via records, component visual assets, inactive assembled showcase; writes prefab and scene.
2. `2. Place assembled VISUAL preview on SELECTED tabletop` — selection must be the actual workshop tabletop. Creates one uniformly scaled, unscored preview.
3. `3. Validate Sources and Report Blockers` — verifies 368 model rows and exact paths, 312 BOM and placements, route/via flags and output report.
4. `4. Rebuild URP PBR Materials` — regenerates 44 URP/Lit material assets without overwriting original textures.
5. `5. Create COMPLETE assembled motherboard (VISUAL ONLY)` — outputs `L123_CompleteMotherboard_VISUAL_ONLY.prefab` with all 312 component visuals plus eight heatsinks; **never scores gameplay**.

Also register `Tools/Project Spark/Levels/Create Level 123`. Expected outputs under `Assets/ProjectSpark/AutoGeneratedLevels/Level123/`. Scene builder must be repeatable, not touch Levels 1–122, and must not destroy unsaved open scenes without prompting.

## 16 — Engineering source data required before full AAA certification

Approved schematic and connector pin map; BOM verified electrical values/packages/footprint/pin one; per-motor ESC/motor driver pin and protection maps; full board ground/power/clock/sensor nets; W001–W400 exact pads/nets/layers; V001–V156 legal net/layer spans; safety U3/U5 physical pin IDs and independent inhibit graph; deterministic motor load and sensor models with accepted margins; intended workshop wiring/power sources; real `LevelGamePlayManager` and virtual repair API signatures. Stage unavailable subsystems as `UNBOUND`, not fake passing measurements. This prompt is an implementation design, not a claim of completed playability.

## 17 — Required QA report

Run Unity C# compilation, import test, EditMode NUnit, PlayMode and workshop smoke tests. Verify previous Levels 1–122 and all existing selection, move, rotate, wire, measure, shop/shelf/throw and virtual tools. Classify each acceptance test `PASS`, `FAIL`, `NOT_RUN`, `BLOCKED` with reproducible evidence; include actual console logs and commits. Include exact imported path, material-slot results, unity version, manifest hash, screenshot of generated visual-only prefab, objective authority and unresolved schematic blockers. Never claim 100/100 Unity tests passed unless executed.


## 18 — 100 acceptance cases (individual records in `Tests/L123_100_Acceptance_Checks.csv`)

The source includes 10 groups of 10 tests: art import, assembly, PCB topology, power/ESC, inertial and altitude, flight control, F1, F2/F3, W401 safety, and UI/save/regression. Every test begins `NOT_RUN` or `BLOCKED` rather than PASS. Import into your existing NUnit EditMode/PlayMode suite only after the actual Unity APIs are inspected.

**Deliverable milestone gates:** (1) compile/import; (2) approved schematic; (3) real electrical interaction; (4) bounded flight and sensor simulation; (5) faults and safety; (6) verified objective authority and regression. Full AAA certification must not be declared until all gates have evidence.
