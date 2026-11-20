# PROJECT SPARK — LEVEL 97 MASTER UNITY IMPLEMENTATION PROMPT

**VOLTAGE GUARDIAN | VGR-5 Intelligent Power Board | Board-Only AAA Implementation**

> **Use alongside** `ProjectSpark_Level97_VoltageGuardian_Realistic_BoardOnly_Complete.zip`. This document is a development specification and source annex, **not a claim that the Unity level is already playable**. Source CSVs are reproduced without rewriting the unresolved electrical definitions.

**Implementation goals:** 50 installable components; 66 genuine copper tasks; 14 vias; four PCB layers; three power rails (5.0/3.3/1.8 V); R7/V05/Q1 repairs; W067 feedback recovery; actual `LevelGamePlayManager` 6/6.


## 01. Mission and immutable requirements

**LEVEL 97 — VOLTAGE GUARDIAN**, the VGR-5 intelligent multi-rail power board. Implement a fully playable, board-only PCB level in the existing Project Spark game room using its World-Space Canvas virtual instrumentation and click/touch repair UI. Do not generate an additional room, tables, chairs, physical oscilloscopes, soldering tools, or irrelevant scenery. Preserve Levels 1–96 and all current shared systems. Make progress authoritative, deterministic, inspectable and impossible to earn by toggling cosmetic UI.

**Source lock:** The accompanying Level 97 archive is the source of truth for exported model file names, placement positions, conceptual routes, via plan, faults and objective names. Its route rows are *not* a validated schematic; do not quietly invent pin assignments and treat them as verified. The completed showcase is art-only and must never be instantiated as the gameplay graph.

## 02. Exact source scope and counts

Archive: `ProjectSpark_Level97_VoltageGuardian_Realistic_BoardOnly_Complete.zip`. Verified source includes **68 modular FBX models, 69 total FBXs including the assembled reference, 50 installable BOM parts, 66 conceptual copper routes, 14 planned vias, 6 objectives and 3 faults**. Asset summary reports 37,372 modular triangles, 21 material families and 105 shared PBR texture maps. This is imported art + planning data, not playable circuitry. Normalize any differences between narrative and source in an explicit discrepancy report, never silently.

## 03. Board scale and coordinate contract

Physical board: **0.160 m X × 0.110 m Z × 0.0016 m Y**, Y-up, board top approximately **Y=0.0008 m**. BOM entries give component local X and Z in metres; source README recommends component contact reference Y=0.00092 m. Any aesthetic scaling for the 9.9 m Project Spark room table must be under a visual-only preview root. Never scale solver resistances, electrical topology, true board units, hit tolerance or thermal conductivity with preview scaling.

## 04. Startup gameplay state

Spawn only `00_L97_VGR5_Empty_PCB_160x110mm.fbx` under `L97_VGR5_GAMEPLAY_EMPTY`. At start: 50 BOM parts uninstalled, 66 route tasks INCOMPLETE, 14 via tasks MISSING, W067 DISCONNECTED, all six objectives unearned. Factory pad/reference artwork is non-conductive unless an explicitly verified factory netlist defines a copper branch. Do not place the assembled reference prefab into the playable hierarchy. A reference may appear in an isolated art preview with all colliders, PCB interactions and objective providers disabled.

## 05. Existing API discovery before writing Unity integration

Inspect real code for `SparkElectricalSolver`, `SparkCircuitSystem`, `SparkTerminal`, PCB placement/routing/via classes, `LevelGamePlayManager`, `SparkLevelEvaluator`, World-Space Canvas tools and input/repair bridge. List actual namespaces, public methods, events, persistence routes and any unavailable interfaces. Create adapters only after this scan; never assume proposed class names exist. Respect assembly definitions, render pipeline, Unity version, Input System/legacy input status, object shelf/shop and existing level loading patterns.

## 06. Board art import

Retain all 68 individual FBXs and their matching GLBs; prefer FBX in Unity. Use the package manifest for exact bindings. Map imported art materials by actual FBX material names rather than numeric slot order. Keep editable component parts separately addressable via BOM IDs. Imported mesh models cannot themselves be authoritative installed components. Give the PCB and each part separate art and electrical state owners.

## 07. Material and shader hookup

The provided PBR reference textures are not guaranteed to be Unity-ready without packing. Support Built-in Standard / URP Lit / HDRP Lit with explicit normal import, albedo sRGB, metallic/smoothness channel pack and occlusion semantics. Exclude shader-graph or render-pipeline features not installed. Create deterministic generated `.mat` paths; never overwrite unrelated assets. Ensure copper/tinned metal, FR4, soldermask, silkscreen, black IC epoxy, ceramic, gold pads, LEDs, coil windings and plastic connectors read as realistic rather than exaggerated bloom.

## 08. Board-only presentation and camera

Use current Project Spark room and existing virtual studio UI. Camera should zoom to solder joints, gold pads, four copper layers, and fine IC leads. Offer top/bottom camera presets, orbit, inspect focus and layer reveal (L1 top signal, L2 GND, L3 power, L4 bottom signal). World-space Canvas scales with comfortable text sizes; touch targets need larger invisible hit colliders without changing geometric pads. Avoid UI obstructing the PCB while dragging.

## 09. Virtual-tool integration

Reuse existing World-Space Canvas virtual multimeter, oscilloscope, current probe, thermal scanner, net tracer, solder, desolder, via repair, component replacement, calibration and diagnostics. Flow: select tool → raycast/click/touch PCB target → inspect verified authority state → validate action prerequisites → request transaction → emit VFX at actual hit location → receive authority result → update instruments/objectives. Do not construct physical FBX instruments or physical tool models.

## 10. Input and hit targets

Support mouse and touch with the project's existing input abstractions. Register component IDs (U1–U8, Q1–Q4, L1–L3, R1–R13, C1–C14, D1–D3, J1–J2, LED1–LED2, W067), pad IDs, net IDs, route IDs, via IDs, and selectable layer IDs. Reject clicks from hidden/back layers without an active X-ray/layer tool. Always separate the hit mesh from authoritative component record; never mark a route complete because a decorative mesh was clicked.

## 11. BOM assembly and occupancy

Use the 50 BOM rows verbatim. A footprint record needs reference, expected footprint/package/value/orientation, pad numbers, local origin, permitted height and contact anchor. The BOM supplies component family/role and position, **not complete manufacturer/package specifications**; derive or author verified footprint definitions and record assumptions. Placing a part must check correct reference, rotation, polarity, collision/occupancy, solder contacts and electrical pin graph. The W067 part counts once in the 50 installables; do not double-count it.

## 12. Electrical domain separation

Define the protected physical 12 V input, main 5.0 V output, 3.3 V secondary, 1.8 V secondary, ground/power ground, switching nodes and low-voltage logic monitor nodes. An electrical solver must not merge them just because art shares a visual copper pad or a proximity collider. W067 carries **logical trusted power-good feedback**, never the real 5 V load current. Do not energize any rail if required ground return, protection or verified topology is absent.

## 13. Four-layer copper and physical via model

Use explicit stackup L1 top/L2 GND/L3 power/L4 bottom. Each copper segment must have geometry, net, layer, endpoints, width, clearance, and state. A via must have start/end layer and valid layer-span semantics; check electrical connection only among allowed copper layers. The source via plan assigns V01–V14 L1→L4 and `verified_net=UNASSIGNED`, so do **not** assume any via is already logically correct. Detect shorts between unlike nets, unconnected required pads, crossing without isolation, coincident non-connected lines and power-plane interactions.

## 14. Pin-verified netlist reconciliation — mandatory blocker

**STOP before electrical certification until the source concept is reconciled.** The `Copper_Routing_Concept_W001_W066.csv` rows identify route group, logical_net, and placeholder pads such as `MAIN_BUCK_SOURCE_01`; each `is_verified_pin_netlist=NO`. Do not infer missing genuine pin numbers by cycling nets or nearest-neighbor geometry. Author a schematic/footprint reconciliation record with device part, pin number, electrical pin type, signal domain, net name, pad local position, intended PCB layer, and route. Independently validate it via electrical netlist/ERC plus PCB DRC. Make the builder report `BLOCKED_UNVERIFIED_NETLIST` until these rows are resolved. The appendix preserves the original 66 rows untouched.

## 15. Player copper task state machine

States: Unrouted → Previewing → RoutePlaced → DRCValid → ElectricallyConnected → Qualified. Removing a route rolls back graph edges and any dependent live evidence. Each W001–W066 remains a distinct player task even if its net joins a larger factory copper node. Factory copper must never bypass required player tasks and award progress. The 66-route count excludes W067, which is one separate installable removable jumper.

## 16. Power-stage model

Implement a safe, educational multi-rail plant with stateful input voltage/current, enable sequencing, switching duty, inductor current, output capacitor voltage, feedback network, regulation-loop error, ripple, losses, temperature and transient loads. Prefer a physically coherent averaged synchronous buck model rather than pretending a click directly sets output voltage. Derive actual 5 V regulation under finite available input and component conditions. Include reverse-polarity, overcurrent, overvoltage, undervoltage, thermal protection and load disconnect. Game limits are configurable scenario parameters, not manufacturer ratings.

## 17. Secondary 3.3 V and 1.8 V rails

Do not instantly power the secondary rails by setting green UI flags. Simulate dependencies on the preceding rail, finite startup delays, soft-start, load, LDO/converter losses where applicable, thermal rise and power-good. Require measured stable 5.0 V, 3.3 V and 1.8 V (proposal ±3% steady-state) before Objective 3. Configure exact tolerances in scenario data rather than assuming bare numbers prove hardware safety.

## 18. Regulator feedback R7 fault

Source F1: wrong resistance at `R7`, causing main buck output setpoint deviation. The wrong value must alter feedback divider voltage and plant output until the safety monitor responds. A correct component replacement must check removal/power-off guard and valid replacement value, actual solder reattachment, new startup sequence and three measured load operating points. A green UI checkmark alone is not a repair.

## 19. Open via V05 fault

Source F2: `V05` is the open supply via. This fault must remove its graph edge and degrade downstream rail continuity. An authentic via repair must restore allowed layer-span conductivity and DRC; never set the downstream voltage directly. Post-repair test: continuity, isolation and power sequencing. Verify that `V05` already exists as a physically placed gameplay via when fault injection begins; inject the open state at the appropriate objective stage rather than before it has ever been installed.

## 20. Q1 high-loss MOSFET fault

Source F3: `Q1` with excessive conduction/switching loss and temperature rise. Calculate dissipated loss with bounded plant parameters, model thermal time constants and trigger protected derating/shutdown. Diagnose with current probe, heat map and efficiency changes. Repair by removing and replacing the Q1 component (with authorization), maintaining solder geometry, then retest efficiency and thermal recovery.

## 21. Critical W067 path

Source declares exactly `L97_MainRailSupervisor_U7.TRUSTED_MAIN_POWER_GOOD_OUT` → **W067** → `L97_PowerSafetyController_U8.TRUSTED_MAIN_POWER_GOOD_IN`. In normal commissioning the player installs the jumper before attempting to pass the rail and load tests. For Objective 6, after history 1–5, the player physically removes it during a controlled test. A loss of trusted signal is invalid/unknown, not a valid measured power-good LOW and not a forced 5 V rail collapse. Disable safety-dependent loads via modeled shutdown, retain independent diagnostics, latch the fault. Restore by physical reconnection, new rail readings, explicit reset and **new qualification**. Never let a fake wire UI, cached sample, reconnect-only event or test shortcut complete the objective.

## 22. Telemetry and instruments

Virtual multimeter reads actual node potential from electrical authority; oscilloscope reads time-series output voltage/ripple; current probe reads actual current; thermal scan reads modeled component temperatures; net tracer queries the conductive graph. Every sample needs timestamp, validity, measured physical quantity and calibration state. If missing data, show `UNKNOWN/INVALID`, not a plausible preset readout. Unpowered displays cannot misleadingly claim safe live voltage.

## 23. Load and protection qualification

Run light, nominal and peak dynamic load cases. Verify voltage accuracy, ripple, load step droop, settling, efficiency, temperatures and safety state. Proposed game targets: main 5 V within ±3%, secondary rails ±3%, 5 V ripple under 60 mV p-p during declared load, nominal efficiency ≥85%; these are gameplay tuning defaults, not validated engineering specs. Set exact durations, load levels, thresholds and evidence freshness in a level configuration asset and save the measurement traces.

## 24. Procedural VFX and AAA UI

Use selected virtual tool's cursor/touch position as VFX origin. Solder produces short bright micro-sparks and local molten-metal glow, trace and via repair produces restrained localized conductive highlight, thermal inspection uses diagnostic color overlay, current paths use optional short cyan pulses. Clamp VFX intensity and particle lifetimes for mobile performance, avoid full-screen effects. VFX must subscribe to true state changes; no VFX, shader or animation can change circuit authority.

## 25. Objective 1 — assembly

Award only after all 50 unique BOM IDs are installed, every expected footprint/rotation/value/polarity and solder connection is verified, and no wrong-value part remains. Preserve evidence and timestamps. A visual reference board cannot count. Initial W067 must be installed for normal commissioning and is included once in the 50 component placements.

## 26. Objective 2 — routing

Require all 66 planned copper routes and 14 vias **after** pin-level mapping is validated. Check electrical net topology, cross-layer connectivity, short isolation, required route ownership, and DRC. Refuse certification if conceptual source pads, unassigned via nets or unresolved component pin definitions remain.

## 27. Objective 3 — multi-rail startup

Require actual 12 V protected source and verified power/ground graph, correct W067 trusted path, deliberate enable sequence and stable measured 5.0, 3.3 and 1.8 V rails. Account for startup transients and errors. A simulated steady voltage without proper provenance cannot qualify.

## 28. Objective 4 — load and efficiency

Require separately logged light, normal and peak load regimes with valid voltage, ripple, efficiency, current and thermal evidence. No acceptance through UI preset values or a single timer. On interruption, restart the full test window.

## 29. Objective 5 — fault recovery

Source `Faults_3.csv` gives F1=R7 feedback divider mismatch, F2=V05 open via, F3=Q1 high losses. After each repair, verify actual topology/component/plant correction, then execute a fresh full multi-rail and dynamic-load qualification. Fault history is durable, even if the current board subsequently degrades.

## 30. Objective 6 — W067 critical recovery

Require Objectives 1–5 historically done, W067 previously installed and trusted, a controlled disconnection, observed feedback invalidation, safe dependent-load inhibition, fault latch, physical repair, independent fresh measurements and explicit reset. Only after a **new** complete rail/load/thermal qualification may the manager award six of six. User must not be allowed to skip disconnection or repeatedly press a reset button to pass.

## 31. Save, reload and anti-cheat integrity

Persist board topology, installed BOM IDs, per-route geometry/state, vias, faults, W067 lifecycle, calibration, objective evidence epochs and last tested run ID. Rebuild the graph on load from authoritative records; never trust serialized green/red colors or a cached `targets=6`. On load, invalidate stale live sample certification until sensors and rails have been rechecked.

## 32. Safety, fault recovery and undo

Allow a virtual power-off/isolate mode before desolder, trace repair or component removal. Block unsafe tool actions during high modeled power, or require explicit isolation. Provide undo for an incomplete route under safe rules; do not undo objective history or bypass fault recording. Fail gracefully when a target is unavailable or an action is canceled mid-drag.

## 33. Editor builder — single action target

Menu: `Tools/Project Spark/Levels/Create Level 97`. The builder must locate imported manifest by AssetDatabase discovery instead of hardcoded `Assets/ProjectSpark/...` prefix, create `Level_97.asset`, `Level_97_Layout.prefab`, scene roots for electrical authority/visual-only art/tool integration and six objective evaluators. Integrate with real level IDs and menu practices discovered in project; if APIs are missing emit a clear validation report rather than generating uncompilable guesses. Avoid duplicated menu items and preserve existing levels.

## 34. Asset import menu compatibility

The supplied source file `SparkLevel97BoardOnlyImporter.cs` exposes `Tools/Project Spark/Levels/97/Board Only/1. Create EMPTY VGR-5 PCB`, a visual-only reference action, PBR material creation and validation. Reuse or extend these without clobbering. Source `SparkLevel97BoardTarget.cs` is merely a clickable hit-target data bridge; do not interpret it as circuit/safety authority. Avoid hardcoded subfolders: importer already searches for `Asset_Manifest_68.csv`.

## 35. Performance and rendering

Target smooth interaction on desktop and mobile. Pool VFX, avoid creating one Canvas for each solder pad, use shared world-space virtual-tool panel, minimize GC allocations in every-frame measurement updates, batch static factory pads but preserve individual interactive targets, and reduce unnecessary overdraw/bloom. Only create LOD/collider derivatives where necessary. Report triangle counts and draw calls after Unity import; offline triangle count is not evidence of GPU performance.

## 36. Logging and diagnostics

Provide typed, throttled diagnostic messages for missing asset, unresolved net, illegal connection, wrong voltage domain, false via short, measurement stale, invalid W067 feedback, and blocked objectives. Generate `Level97_Netlist_Reconciliation_Report.csv` and `Level97_Validation_Report.md` with all source-to-runtime mappings and unresolved TODOs. Do not mark validation green while blockers remain.

## 37. Integration and regression

Test all existing Levels 1–96 for scene/menu/script namespace conflicts, shop and object shelf compatibility, direct wire and PCB trace interactions, shared UI resource references and `LevelGamePlayManager` objective persistence. The Level 97 board-only preview must work on the existing table and have no unrelated furniture or instrument FBXs. All previous level assets and evaluator logic are immutable unless an explicit shared fix is required and regression-tested.

## 38. Acceptance test framework

Use Unity Edit Mode for import, DRC, state-machine and data validation; Play Mode for actual circuitry, touch interactions, saving and real safety responses. Maintain authoritative test IDs `L97-T001` onward; attach reproducible setup, stimulus, expected state, measured state and pass/fail evidence. A test passes only after actual code executes, not because the prompt lists it. Exact 80 proposed cases appear below.

## 39. Completion and handoff

Deliver compiling C# sources, generated scenes/prefabs, verified reconciled netlist or an explicit blocker report, complete trace/via pin mapping, source manifest bindings, virtual UI adapters, objective evaluators, fault/state tests, how-to-play first-wire tutorial, screen capture verification checklist and regression report. Do not claim a playable 6/6 unless it truly reaches LevelGamePlayManager in Unity Play Mode.

## Engineering acceptance tests — 80 proposed cases

| ID | Domain | Required verification |
|---|---|---|
| L97-T001 | Assets | 68 modular FBXs discovered |
| L97-T002 | Assets | 68 modular GLBs matching |
| L97-T003 | Assets | reference art imported in isolated hierarchy |
| L97-T004 | Assets | board dimensions match 160x110mm |
| L97-T005 | Assets | BOM coordinates interpreted in metres |
| L97-T006 | Assets | 20/21 material family discrepancy documented |
| L97-T007 | Assets | texture channels/pipeline verified |
| L97-T008 | Assets | no physical instrument FBXs generated |
| L97-T009 | Assembly | empty board at start |
| L97-T010 | Assembly | all 50 components initially uninstalled |
| L97-T011 | Assembly | correct BOM count no duplicate W067 |
| L97-T012 | Assembly | component footprint occupancy check |
| L97-T013 | Assembly | wrong value rejected |
| L97-T014 | Assembly | wrong orientation rejected |
| L97-T015 | Assembly | wrong footprint rejected |
| L97-T016 | Assembly | power-on desolder blocked |
| L97-T017 | Netlist | 66 placeholder routes reported unverified |
| L97-T018 | Netlist | no objective progress with unresolved pin mapping |
| L97-T019 | Netlist | 14 via net placeholders reported unassigned |
| L97-T020 | Netlist | verified supply domains separate |
| L97-T021 | Netlist | factory copper cannot bypass route |
| L97-T022 | Netlist | cross-layer via span validated |
| L97-T023 | Netlist | wrong-net short rejected |
| L97-T024 | Netlist | missing return cannot energize load |
| L97-T025 | Routing | route placement legal L1 |
| L97-T026 | Routing | route placement legal L4 |
| L97-T027 | Routing | L2 ground plane semantics |
| L97-T028 | Routing | L3 power plane semantics |
| L97-T029 | Routing | trace clearance DRC |
| L97-T030 | Routing | via count 14 required |
| L97-T031 | Routing | route removal invalidates downstream evidence |
| L97-T032 | Routing | all 66 routes tracked uniquely |
| L97-T033 | Power | 12 V protection absent/open blocks startup |
| L97-T034 | Power | main buck starts with ramp |
| L97-T035 | Power | 5 V nominal qualified |
| L97-T036 | Power | 3.3 V nominal qualified |
| L97-T037 | Power | 1.8 V nominal qualified |
| L97-T038 | Power | power-good dependent on measurements |
| L97-T039 | Power | wrong sequence blocks certification |
| L97-T040 | Power | ground disconnection destabilizes system |
| L97-T041 | Load | light-load qualification |
| L97-T042 | Load | nominal-load qualification |
| L97-T043 | Load | peak-load qualification |
| L97-T044 | Load | ripple measured not hardcoded |
| L97-T045 | Load | load-step droop recorded |
| L97-T046 | Load | settling evaluated over time |
| L97-T047 | Load | efficiency derived from input/output power |
| L97-T048 | Load | heat follows losses |
| L97-T049 | Faults | F1 R7 fault degrades output |
| L97-T050 | Faults | R7 correct replacement requalifies |
| L97-T051 | Faults | F2 V05 open breaks topology |
| L97-T052 | Faults | V05 repair reconnects only allowed layers |
| L97-T053 | Faults | F3 Q1 loss raises temperature |
| L97-T054 | Faults | Q1 replacement improves efficiency |
| L97-T055 | Faults | faults cannot clear from VFX alone |
| L97-T056 | Faults | fresh post-fault test required |
| L97-T057 | W067 | W067 starts physically disconnected |
| L97-T058 | W067 | normal commissioning requires install |
| L97-T059 | W067 | critical test requires prior trusted state |
| L97-T060 | W067 | disconnect marks quality invalid |
| L97-T061 | W067 | no false zero rail on feedback loss |
| L97-T062 | W067 | dependent loads inhibited and fault latched |
| L97-T063 | W067 | reconnect alone cannot clear fault |
| L97-T064 | W067 | reset and new qualification unlock 6/6 |
| L97-T065 | UI | world-space UI selection works |
| L97-T066 | UI | mouse click hit mapping |
| L97-T067 | UI | touch hit mapping |
| L97-T068 | UI | multimeter reads authoritative node |
| L97-T069 | UI | oscilloscope samples waveform history |
| L97-T070 | UI | thermal scan reads thermal model |
| L97-T071 | UI | repair VFX at hit location |
| L97-T072 | UI | decorative panel cannot grant objective |
| L97-T073 | Persistence | save/reload installed BOM |
| L97-T074 | Persistence | save/reload route/via states |
| L97-T075 | Persistence | save/reload faults and W067 events |
| L97-T076 | Persistence | stale measurements rejected on reload |
| L97-T077 | Persistence | six-objective manager integration |
| L97-T078 | Persistence | edit-mode compile |
| L97-T079 | Persistence | play-mode 6/6 verified |
| L97-T080 | Persistence | levels 1–96 regression preserved |


## Source discrepancies, authority, and unknowns

- `Asset_Summary.json` reports 21 material families and 105 maps; README says 20 PBR families and other reference maps. Record and reconcile the actual imported asset names, without altering source.
- `Copper_Routing_Concept_W001_W066.csv` uses **unverified placeholder pads**. The provided package does not contain a manufacturer-verified schematic or board-level electrical implementation.
- `Via_Plan_14.csv` marks all via nets `UNASSIGNED`. Do not manufacture net truth from a generic via pattern.
- The BOM provides component reference/role/position, but not a full electrical footprint/pin mapping or verified component specifications.
- Source Unity scripts are import/art helpers, not power-system simulation or objective evaluators.
- None of these source files prove Unity compilation, live wiring or playable completion.


## Technical annex 01 — `Asset_Manifest_68.csv` (verbatim source)

```csv
asset_id,asset_name,category,bom_ref,x_m,y_m,z_m,mesh_parts,triangles,description
0,00_L97_VGR5_Empty_PCB_160x110mm,Board,,0.0,0,0.0,237,5084,"Empty board with manufacturing pads, silkscreen UV and nonfunctional factory detail"
1,01_L97_U1_IC,Installable,U1,-0.03,0.0008,0.014,61,744,Integrated power controller / regulator / monitor
2,02_L97_U2_IC,Installable,U2,0.012,0.0008,0.014,51,624,Integrated power controller / regulator / monitor
3,03_L97_U3_IC,Installable,U3,0.044,0.0008,0.014,49,600,Integrated power controller / regulator / monitor
4,04_L97_U4_IC,Installable,U4,-0.064,0.0008,-0.005,50,612,Integrated power controller / regulator / monitor
5,05_L97_U5_IC,Installable,U5,-0.04,0.0008,-0.024,50,612,Integrated power controller / regulator / monitor
6,06_L97_U6_IC,Installable,U6,-0.01,0.0008,-0.027,52,636,Integrated power controller / regulator / monitor
7,07_L97_U7_IC,Installable,U7,0.021,0.0008,-0.024,48,588,Integrated power controller / regulator / monitor
8,08_L97_U8_IC,Installable,U8,0.046,0.0008,-0.024,66,804,Integrated power controller / regulator / monitor
9,09_L97_Q1_MOSFET,Installable,Q1,-0.044,0.0008,0.03,23,276,Protected power switch
10,10_L97_Q2_MOSFET,Installable,Q2,-0.004,0.0008,0.018,25,300,Protected power switch
11,11_L97_Q3_MOSFET,Installable,Q3,0.034,0.0008,0.018,23,276,Protected power switch
12,12_L97_Q4_MOSFET,Installable,Q4,0.061,0.0008,-0.01,24,288,Protected power switch
13,13_L97_L1_Inductor,Installable,L1,-0.012,0.0008,0.032,32,6500,Switching magnetic component
14,14_L97_L2_Inductor,Installable,L2,0.027,0.0008,0.032,34,6524,Switching magnetic component
15,15_L97_L3_Inductor,Installable,L3,0.056,0.0008,0.032,32,6500,Switching magnetic component
16,16_L97_R1_Resistor,Installable,R1,-0.066,0.0008,-0.034,5,60,Precision / current-sense resistor
17,17_L97_R2_Resistor,Installable,R2,-0.0585,0.0008,-0.034,5,60,Precision / current-sense resistor
18,18_L97_R3_Resistor,Installable,R3,-0.051000000000000004,0.0008,-0.034,5,60,Precision / current-sense resistor
19,19_L97_R4_Resistor,Installable,R4,-0.043500000000000004,0.0008,-0.034,5,60,Precision / current-sense resistor
20,20_L97_R5_Resistor,Installable,R5,-0.036000000000000004,0.0008,-0.034,5,60,Precision / current-sense resistor
21,21_L97_R6_Resistor,Installable,R6,-0.028500000000000004,0.0008,-0.034,5,60,Precision / current-sense resistor
22,22_L97_R7_Resistor,Installable,R7,-0.021000000000000005,0.0008,-0.034,5,60,Precision / current-sense resistor
23,23_L97_R8_Resistor,Installable,R8,-0.066,0.0008,-0.01,5,60,Precision / current-sense resistor
24,24_L97_R9_Resistor,Installable,R9,-0.0585,0.0008,-0.01,5,60,Precision / current-sense resistor
25,25_L97_R10_Resistor,Installable,R10,-0.051000000000000004,0.0008,-0.01,5,60,Precision / current-sense resistor
26,26_L97_R11_Resistor,Installable,R11,-0.043500000000000004,0.0008,-0.01,5,60,Precision / current-sense resistor
27,27_L97_R12_Resistor,Installable,R12,-0.036000000000000004,0.0008,-0.01,5,60,Precision / current-sense resistor
28,28_L97_R13_Resistor,Installable,R13,-0.028500000000000004,0.0008,-0.01,5,60,Precision / current-sense resistor
29,29_L97_C1_Capacitor,Installable,C1,-0.065,0.0008,0.044,3,288,MLCC / bulk output capacitor
30,30_L97_C2_Capacitor,Installable,C2,-0.049,0.0008,0.044,3,288,MLCC / bulk output capacitor
31,31_L97_C3_Capacitor,Installable,C3,-0.033,0.0008,0.044,5,60,MLCC / bulk output capacitor
32,32_L97_C4_Capacitor,Installable,C4,-0.017,0.0008,0.044,5,60,MLCC / bulk output capacitor
33,33_L97_C5_Capacitor,Installable,C5,-0.0010000000000000009,0.0008,0.044,5,60,MLCC / bulk output capacitor
34,34_L97_C6_Capacitor,Installable,C6,0.015,0.0008,0.044,5,60,MLCC / bulk output capacitor
35,35_L97_C7_Capacitor,Installable,C7,0.031,0.0008,0.044,5,60,MLCC / bulk output capacitor
36,36_L97_C8_Capacitor,Installable,C8,-0.065,0.0008,-0.041,3,288,MLCC / bulk output capacitor
37,37_L97_C9_Capacitor,Installable,C9,-0.049,0.0008,-0.041,5,60,MLCC / bulk output capacitor
38,38_L97_C10_Capacitor,Installable,C10,-0.033,0.0008,-0.041,5,60,MLCC / bulk output capacitor
39,39_L97_C11_Capacitor,Installable,C11,-0.017,0.0008,-0.041,5,60,MLCC / bulk output capacitor
40,40_L97_C12_Capacitor,Installable,C12,-0.0010000000000000009,0.0008,-0.041,5,60,MLCC / bulk output capacitor
41,41_L97_C13_Capacitor,Installable,C13,0.015,0.0008,-0.041,5,60,MLCC / bulk output capacitor
42,42_L97_C14_Capacitor,Installable,C14,0.031,0.0008,-0.041,5,60,MLCC / bulk output capacitor
43,43_L97_D1_Diode,Installable,D1,-0.048,0.0008,-0.004,6,72,Protection / TVS diode
44,44_L97_D2_Diode,Installable,D2,-0.013999999999999999,0.0008,-0.004,6,72,Protection / TVS diode
45,45_L97_D3_Diode,Installable,D3,0.020000000000000004,0.0008,-0.004,6,72,Protection / TVS diode
46,46_L97_J1_Connector,Installable,J1,-0.068,0.0008,0.036,14,168,Power connector
47,47_L97_J2_Connector,Installable,J2,0.069,0.0008,0.026,14,168,Power connector
48,48_L97_LED1_LED,Installable,LED1,-0.012,0.0008,-0.047,4,48,Rail status LED
49,49_L97_LED2_LED,Installable,LED2,0.003,0.0008,-0.047,4,48,Rail status LED
50,50_L97_W067_Jumper,Installable,W067,0.063,0.0008,-0.038,4,48,Removable main power-good feedback jumper
51,51_L97_L1_Top_Copper_Reference,Layer,,0.0,0,0.0,22,264,Optional board-detail and fault/inspection asset; not a playable-route completion
52,52_L97_L2_Internal_Ground_Plane_Reference,Layer,,0.0,0,0.0,1,12,Optional board-detail and fault/inspection asset; not a playable-route completion
53,53_L97_L3_Internal_Power_Planes_Reference,Layer,,0.0,0,0.0,3,36,Optional board-detail and fault/inspection asset; not a playable-route completion
54,54_L97_L4_Bottom_Copper_Reference,Layer,,0.0,0,0.0,22,264,Optional board-detail and fault/inspection asset; not a playable-route completion
55,55_L97_Soldermask_And_Silkscreen_Reference,Layer,,0.0,0,0.0,1,12,Optional board-detail and fault/inspection asset; not a playable-route completion
56,56_L97_Via_Through_Hole,Copper,,0.0,0,0.0,3,512,Optional board-detail and fault/inspection asset; not a playable-route completion
57,57_L97_Via_Blind_L1_L2,Copper,,0.0,0,0.0,3,512,Optional board-detail and fault/inspection asset; not a playable-route completion
58,58_L97_Via_Buried_L2_L3,Copper,,0.0,0,0.0,3,512,Optional board-detail and fault/inspection asset; not a playable-route completion
59,59_L97_Copper_Trace_Coupon,Copper,,0.0,0,0.0,1,12,Optional board-detail and fault/inspection asset; not a playable-route completion
60,60_L97_ENIG_Pad_Grid,Copper,,0.0,0,0.0,15,180,Optional board-detail and fault/inspection asset; not a playable-route completion
61,61_L97_Open_Power_Via_Fault,Fault,,0.0,0,0.0,3,512,Optional board-detail and fault/inspection asset; not a playable-route completion
62,62_L97_Solder_Bridge_Coupon,Fault,,0.0,0,0.0,3,36,Optional board-detail and fault/inspection asset; not a playable-route completion
63,63_L97_Repair_Strap,Fault,,0.0,0,0.0,1,12,Optional board-detail and fault/inspection asset; not a playable-route completion
64,64_L97_Replaced_MOSFET_Pad,Fault,,0.0,0,0.0,3,36,Optional board-detail and fault/inspection asset; not a playable-route completion
65,65_L97_5V_3V3_1V8_Rail_Markers,Annotation,,0.0,0,0.0,3,192,Optional board-detail and fault/inspection asset; not a playable-route completion
66,66_L97_W067_Header_Base,Annotation,,0.0,0,0.0,3,108,Optional board-detail and fault/inspection asset; not a playable-route completion
67,67_L97_Thermal_Inspection_Pads,Annotation,,0.0,0,0.0,4,192,Optional board-detail and fault/inspection asset; not a playable-route completion
```


## Technical annex 02 — `Component_BOM_50.csv` (verbatim source)

```csv
reference,component_family,role,asset_file,x_m,z_m,initially_installed
U1,IC,Integrated power controller / regulator / monitor,01_L97_U1_IC.fbx,-0.03,0.014,false
U2,IC,Integrated power controller / regulator / monitor,02_L97_U2_IC.fbx,0.012,0.014,false
U3,IC,Integrated power controller / regulator / monitor,03_L97_U3_IC.fbx,0.044,0.014,false
U4,IC,Integrated power controller / regulator / monitor,04_L97_U4_IC.fbx,-0.064,-0.005,false
U5,IC,Integrated power controller / regulator / monitor,05_L97_U5_IC.fbx,-0.04,-0.024,false
U6,IC,Integrated power controller / regulator / monitor,06_L97_U6_IC.fbx,-0.01,-0.027,false
U7,IC,Integrated power controller / regulator / monitor,07_L97_U7_IC.fbx,0.021,-0.024,false
U8,IC,Integrated power controller / regulator / monitor,08_L97_U8_IC.fbx,0.046,-0.024,false
Q1,MOSFET,Protected power switch,09_L97_Q1_MOSFET.fbx,-0.044,0.03,false
Q2,MOSFET,Protected power switch,10_L97_Q2_MOSFET.fbx,-0.004,0.018,false
Q3,MOSFET,Protected power switch,11_L97_Q3_MOSFET.fbx,0.034,0.018,false
Q4,MOSFET,Protected power switch,12_L97_Q4_MOSFET.fbx,0.061,-0.01,false
L1,Inductor,Switching magnetic component,13_L97_L1_Inductor.fbx,-0.012,0.032,false
L2,Inductor,Switching magnetic component,14_L97_L2_Inductor.fbx,0.027,0.032,false
L3,Inductor,Switching magnetic component,15_L97_L3_Inductor.fbx,0.056,0.032,false
R1,Resistor,Precision / current-sense resistor,16_L97_R1_Resistor.fbx,-0.066,-0.034,false
R2,Resistor,Precision / current-sense resistor,17_L97_R2_Resistor.fbx,-0.0585,-0.034,false
R3,Resistor,Precision / current-sense resistor,18_L97_R3_Resistor.fbx,-0.051000000000000004,-0.034,false
R4,Resistor,Precision / current-sense resistor,19_L97_R4_Resistor.fbx,-0.043500000000000004,-0.034,false
R5,Resistor,Precision / current-sense resistor,20_L97_R5_Resistor.fbx,-0.036000000000000004,-0.034,false
R6,Resistor,Precision / current-sense resistor,21_L97_R6_Resistor.fbx,-0.028500000000000004,-0.034,false
R7,Resistor,Precision / current-sense resistor,22_L97_R7_Resistor.fbx,-0.021000000000000005,-0.034,false
R8,Resistor,Precision / current-sense resistor,23_L97_R8_Resistor.fbx,-0.066,-0.01,false
R9,Resistor,Precision / current-sense resistor,24_L97_R9_Resistor.fbx,-0.0585,-0.01,false
R10,Resistor,Precision / current-sense resistor,25_L97_R10_Resistor.fbx,-0.051000000000000004,-0.01,false
R11,Resistor,Precision / current-sense resistor,26_L97_R11_Resistor.fbx,-0.043500000000000004,-0.01,false
R12,Resistor,Precision / current-sense resistor,27_L97_R12_Resistor.fbx,-0.036000000000000004,-0.01,false
R13,Resistor,Precision / current-sense resistor,28_L97_R13_Resistor.fbx,-0.028500000000000004,-0.01,false
C1,Capacitor,MLCC / bulk output capacitor,29_L97_C1_Capacitor.fbx,-0.065,0.044,false
C2,Capacitor,MLCC / bulk output capacitor,30_L97_C2_Capacitor.fbx,-0.049,0.044,false
C3,Capacitor,MLCC / bulk output capacitor,31_L97_C3_Capacitor.fbx,-0.033,0.044,false
C4,Capacitor,MLCC / bulk output capacitor,32_L97_C4_Capacitor.fbx,-0.017,0.044,false
C5,Capacitor,MLCC / bulk output capacitor,33_L97_C5_Capacitor.fbx,-0.0010000000000000009,0.044,false
C6,Capacitor,MLCC / bulk output capacitor,34_L97_C6_Capacitor.fbx,0.015,0.044,false
C7,Capacitor,MLCC / bulk output capacitor,35_L97_C7_Capacitor.fbx,0.031,0.044,false
C8,Capacitor,MLCC / bulk output capacitor,36_L97_C8_Capacitor.fbx,-0.065,-0.041,false
C9,Capacitor,MLCC / bulk output capacitor,37_L97_C9_Capacitor.fbx,-0.049,-0.041,false
C10,Capacitor,MLCC / bulk output capacitor,38_L97_C10_Capacitor.fbx,-0.033,-0.041,false
C11,Capacitor,MLCC / bulk output capacitor,39_L97_C11_Capacitor.fbx,-0.017,-0.041,false
C12,Capacitor,MLCC / bulk output capacitor,40_L97_C12_Capacitor.fbx,-0.0010000000000000009,-0.041,false
C13,Capacitor,MLCC / bulk output capacitor,41_L97_C13_Capacitor.fbx,0.015,-0.041,false
C14,Capacitor,MLCC / bulk output capacitor,42_L97_C14_Capacitor.fbx,0.031,-0.041,false
D1,Diode,Protection / TVS diode,43_L97_D1_Diode.fbx,-0.048,-0.004,false
D2,Diode,Protection / TVS diode,44_L97_D2_Diode.fbx,-0.013999999999999999,-0.004,false
D3,Diode,Protection / TVS diode,45_L97_D3_Diode.fbx,0.020000000000000004,-0.004,false
J1,Connector,Power connector,46_L97_J1_Connector.fbx,-0.068,0.036,false
J2,Connector,Power connector,47_L97_J2_Connector.fbx,0.069,0.026,false
LED1,LED,Rail status LED,48_L97_LED1_LED.fbx,-0.012,-0.047,false
LED2,LED,Rail status LED,49_L97_LED2_LED.fbx,0.003,-0.047,false
W067,Jumper,Removable main power-good feedback jumper,50_L97_W067_Jumper.fbx,0.063,-0.038,false
```


## Technical annex 03 — `Copper_Routing_Concept_W001_W066.csv` (verbatim source)

```csv
route_id,group,logical_net,proposed_source_pad,proposed_dest_pad,initial_state,is_verified_pin_netlist,notes
W001,INPUT_PROTECTION,GND,INPUT_PROTECTION_SOURCE_01,INPUT_PROTECTION_DEST_01,INCOMPLETE,NO,Conceptual route task; map to real footprint pins and copper graph before gameplay
W002,INPUT_PROTECTION,VIN12,INPUT_PROTECTION_SOURCE_02,INPUT_PROTECTION_DEST_02,INCOMPLETE,NO,Conceptual route task; map to real footprint pins and copper graph before gameplay
W003,INPUT_PROTECTION,VIN12,INPUT_PROTECTION_SOURCE_03,INPUT_PROTECTION_DEST_03,INCOMPLETE,NO,Conceptual route task; map to real footprint pins and copper graph before gameplay
W004,INPUT_PROTECTION,GND,INPUT_PROTECTION_SOURCE_04,INPUT_PROTECTION_DEST_04,INCOMPLETE,NO,Conceptual route task; map to real footprint pins and copper graph before gameplay
W005,INPUT_PROTECTION,VIN12,INPUT_PROTECTION_SOURCE_05,INPUT_PROTECTION_DEST_05,INCOMPLETE,NO,Conceptual route task; map to real footprint pins and copper graph before gameplay
W006,INPUT_PROTECTION,VIN12,INPUT_PROTECTION_SOURCE_06,INPUT_PROTECTION_DEST_06,INCOMPLETE,NO,Conceptual route task; map to real footprint pins and copper graph before gameplay
W007,INPUT_PROTECTION,GND,INPUT_PROTECTION_SOURCE_07,INPUT_PROTECTION_DEST_07,INCOMPLETE,NO,Conceptual route task; map to real footprint pins and copper graph before gameplay
W008,INPUT_PROTECTION,VIN12,INPUT_PROTECTION_SOURCE_08,INPUT_PROTECTION_DEST_08,INCOMPLETE,NO,Conceptual route task; map to real footprint pins and copper graph before gameplay
W009,INPUT_PROTECTION,VIN12,INPUT_PROTECTION_SOURCE_09,INPUT_PROTECTION_DEST_09,INCOMPLETE,NO,Conceptual route task; map to real footprint pins and copper graph before gameplay
W010,INPUT_PROTECTION,GND,INPUT_PROTECTION_SOURCE_10,INPUT_PROTECTION_DEST_10,INCOMPLETE,NO,Conceptual route task; map to real footprint pins and copper graph before gameplay
W011,INPUT_PROTECTION,VIN12,INPUT_PROTECTION_SOURCE_11,INPUT_PROTECTION_DEST_11,INCOMPLETE,NO,Conceptual route task; map to real footprint pins and copper graph before gameplay
W012,INPUT_PROTECTION,VIN12,INPUT_PROTECTION_SOURCE_12,INPUT_PROTECTION_DEST_12,INCOMPLETE,NO,Conceptual route task; map to real footprint pins and copper graph before gameplay
W013,MAIN_BUCK,PGND,MAIN_BUCK_SOURCE_01,MAIN_BUCK_DEST_01,INCOMPLETE,NO,Conceptual route task; map to real footprint pins and copper graph before gameplay
W014,MAIN_BUCK,SWITCH_5V,MAIN_BUCK_SOURCE_02,MAIN_BUCK_DEST_02,INCOMPLETE,NO,Conceptual route task; map to real footprint pins and copper graph before gameplay
W015,MAIN_BUCK,SWITCH_5V,MAIN_BUCK_SOURCE_03,MAIN_BUCK_DEST_03,INCOMPLETE,NO,Conceptual route task; map to real footprint pins and copper graph before gameplay
W016,MAIN_BUCK,PGND,MAIN_BUCK_SOURCE_04,MAIN_BUCK_DEST_04,INCOMPLETE,NO,Conceptual route task; map to real footprint pins and copper graph before gameplay
W017,MAIN_BUCK,SWITCH_5V,MAIN_BUCK_SOURCE_05,MAIN_BUCK_DEST_05,INCOMPLETE,NO,Conceptual route task; map to real footprint pins and copper graph before gameplay
W018,MAIN_BUCK,SWITCH_5V,MAIN_BUCK_SOURCE_06,MAIN_BUCK_DEST_06,INCOMPLETE,NO,Conceptual route task; map to real footprint pins and copper graph before gameplay
W019,MAIN_BUCK,PGND,MAIN_BUCK_SOURCE_07,MAIN_BUCK_DEST_07,INCOMPLETE,NO,Conceptual route task; map to real footprint pins and copper graph before gameplay
W020,MAIN_BUCK,SWITCH_5V,MAIN_BUCK_SOURCE_08,MAIN_BUCK_DEST_08,INCOMPLETE,NO,Conceptual route task; map to real footprint pins and copper graph before gameplay
W021,MAIN_BUCK,SWITCH_5V,MAIN_BUCK_SOURCE_09,MAIN_BUCK_DEST_09,INCOMPLETE,NO,Conceptual route task; map to real footprint pins and copper graph before gameplay
W022,MAIN_BUCK,PGND,MAIN_BUCK_SOURCE_10,MAIN_BUCK_DEST_10,INCOMPLETE,NO,Conceptual route task; map to real footprint pins and copper graph before gameplay
W023,MAIN_BUCK,SWITCH_5V,MAIN_BUCK_SOURCE_11,MAIN_BUCK_DEST_11,INCOMPLETE,NO,Conceptual route task; map to real footprint pins and copper graph before gameplay
W024,MAIN_BUCK,SWITCH_5V,MAIN_BUCK_SOURCE_12,MAIN_BUCK_DEST_12,INCOMPLETE,NO,Conceptual route task; map to real footprint pins and copper graph before gameplay
W025,MAIN_BUCK,PGND,MAIN_BUCK_SOURCE_13,MAIN_BUCK_DEST_13,INCOMPLETE,NO,Conceptual route task; map to real footprint pins and copper graph before gameplay
W026,MAIN_BUCK,SWITCH_5V,MAIN_BUCK_SOURCE_14,MAIN_BUCK_DEST_14,INCOMPLETE,NO,Conceptual route task; map to real footprint pins and copper graph before gameplay
W027,MAIN_BUCK,SWITCH_5V,MAIN_BUCK_SOURCE_15,MAIN_BUCK_DEST_15,INCOMPLETE,NO,Conceptual route task; map to real footprint pins and copper graph before gameplay
W028,MAIN_BUCK,PGND,MAIN_BUCK_SOURCE_16,MAIN_BUCK_DEST_16,INCOMPLETE,NO,Conceptual route task; map to real footprint pins and copper graph before gameplay
W029,SECONDARY_RAILS,GND,SECONDARY_RAILS_SOURCE_01,SECONDARY_RAILS_DEST_01,INCOMPLETE,NO,Conceptual route task; map to real footprint pins and copper graph before gameplay
W030,SECONDARY_RAILS,V3V3_V1V8,SECONDARY_RAILS_SOURCE_02,SECONDARY_RAILS_DEST_02,INCOMPLETE,NO,Conceptual route task; map to real footprint pins and copper graph before gameplay
W031,SECONDARY_RAILS,V3V3_V1V8,SECONDARY_RAILS_SOURCE_03,SECONDARY_RAILS_DEST_03,INCOMPLETE,NO,Conceptual route task; map to real footprint pins and copper graph before gameplay
W032,SECONDARY_RAILS,GND,SECONDARY_RAILS_SOURCE_04,SECONDARY_RAILS_DEST_04,INCOMPLETE,NO,Conceptual route task; map to real footprint pins and copper graph before gameplay
W033,SECONDARY_RAILS,V3V3_V1V8,SECONDARY_RAILS_SOURCE_05,SECONDARY_RAILS_DEST_05,INCOMPLETE,NO,Conceptual route task; map to real footprint pins and copper graph before gameplay
W034,SECONDARY_RAILS,V3V3_V1V8,SECONDARY_RAILS_SOURCE_06,SECONDARY_RAILS_DEST_06,INCOMPLETE,NO,Conceptual route task; map to real footprint pins and copper graph before gameplay
W035,SECONDARY_RAILS,GND,SECONDARY_RAILS_SOURCE_07,SECONDARY_RAILS_DEST_07,INCOMPLETE,NO,Conceptual route task; map to real footprint pins and copper graph before gameplay
W036,SECONDARY_RAILS,V3V3_V1V8,SECONDARY_RAILS_SOURCE_08,SECONDARY_RAILS_DEST_08,INCOMPLETE,NO,Conceptual route task; map to real footprint pins and copper graph before gameplay
W037,SECONDARY_RAILS,V3V3_V1V8,SECONDARY_RAILS_SOURCE_09,SECONDARY_RAILS_DEST_09,INCOMPLETE,NO,Conceptual route task; map to real footprint pins and copper graph before gameplay
W038,SECONDARY_RAILS,GND,SECONDARY_RAILS_SOURCE_10,SECONDARY_RAILS_DEST_10,INCOMPLETE,NO,Conceptual route task; map to real footprint pins and copper graph before gameplay
W039,SECONDARY_RAILS,V3V3_V1V8,SECONDARY_RAILS_SOURCE_11,SECONDARY_RAILS_DEST_11,INCOMPLETE,NO,Conceptual route task; map to real footprint pins and copper graph before gameplay
W040,SECONDARY_RAILS,V3V3_V1V8,SECONDARY_RAILS_SOURCE_12,SECONDARY_RAILS_DEST_12,INCOMPLETE,NO,Conceptual route task; map to real footprint pins and copper graph before gameplay
W041,CURRENT_AND_LOAD,GND,CURRENT_AND_LOAD_SOURCE_01,CURRENT_AND_LOAD_DEST_01,INCOMPLETE,NO,Conceptual route task; map to real footprint pins and copper graph before gameplay
W042,CURRENT_AND_LOAD,IMON,CURRENT_AND_LOAD_SOURCE_02,CURRENT_AND_LOAD_DEST_02,INCOMPLETE,NO,Conceptual route task; map to real footprint pins and copper graph before gameplay
W043,CURRENT_AND_LOAD,IMON,CURRENT_AND_LOAD_SOURCE_03,CURRENT_AND_LOAD_DEST_03,INCOMPLETE,NO,Conceptual route task; map to real footprint pins and copper graph before gameplay
W044,CURRENT_AND_LOAD,GND,CURRENT_AND_LOAD_SOURCE_04,CURRENT_AND_LOAD_DEST_04,INCOMPLETE,NO,Conceptual route task; map to real footprint pins and copper graph before gameplay
W045,CURRENT_AND_LOAD,IMON,CURRENT_AND_LOAD_SOURCE_05,CURRENT_AND_LOAD_DEST_05,INCOMPLETE,NO,Conceptual route task; map to real footprint pins and copper graph before gameplay
W046,CURRENT_AND_LOAD,IMON,CURRENT_AND_LOAD_SOURCE_06,CURRENT_AND_LOAD_DEST_06,INCOMPLETE,NO,Conceptual route task; map to real footprint pins and copper graph before gameplay
W047,CURRENT_AND_LOAD,GND,CURRENT_AND_LOAD_SOURCE_07,CURRENT_AND_LOAD_DEST_07,INCOMPLETE,NO,Conceptual route task; map to real footprint pins and copper graph before gameplay
W048,CURRENT_AND_LOAD,IMON,CURRENT_AND_LOAD_SOURCE_08,CURRENT_AND_LOAD_DEST_08,INCOMPLETE,NO,Conceptual route task; map to real footprint pins and copper graph before gameplay
W049,CURRENT_AND_LOAD,IMON,CURRENT_AND_LOAD_SOURCE_09,CURRENT_AND_LOAD_DEST_09,INCOMPLETE,NO,Conceptual route task; map to real footprint pins and copper graph before gameplay
W050,CURRENT_AND_LOAD,GND,CURRENT_AND_LOAD_SOURCE_10,CURRENT_AND_LOAD_DEST_10,INCOMPLETE,NO,Conceptual route task; map to real footprint pins and copper graph before gameplay
W051,CURRENT_AND_LOAD,IMON,CURRENT_AND_LOAD_SOURCE_11,CURRENT_AND_LOAD_DEST_11,INCOMPLETE,NO,Conceptual route task; map to real footprint pins and copper graph before gameplay
W052,CURRENT_AND_LOAD,IMON,CURRENT_AND_LOAD_SOURCE_12,CURRENT_AND_LOAD_DEST_12,INCOMPLETE,NO,Conceptual route task; map to real footprint pins and copper graph before gameplay
W053,SAFETY_TELEMETRY,GND,SAFETY_TELEMETRY_SOURCE_01,SAFETY_TELEMETRY_DEST_01,INCOMPLETE,NO,Conceptual route task; map to real footprint pins and copper graph before gameplay
W054,SAFETY_TELEMETRY,POWER_GOOD,SAFETY_TELEMETRY_SOURCE_02,SAFETY_TELEMETRY_DEST_02,INCOMPLETE,NO,Conceptual route task; map to real footprint pins and copper graph before gameplay
W055,SAFETY_TELEMETRY,POWER_GOOD,SAFETY_TELEMETRY_SOURCE_03,SAFETY_TELEMETRY_DEST_03,INCOMPLETE,NO,Conceptual route task; map to real footprint pins and copper graph before gameplay
W056,SAFETY_TELEMETRY,GND,SAFETY_TELEMETRY_SOURCE_04,SAFETY_TELEMETRY_DEST_04,INCOMPLETE,NO,Conceptual route task; map to real footprint pins and copper graph before gameplay
W057,SAFETY_TELEMETRY,POWER_GOOD,SAFETY_TELEMETRY_SOURCE_05,SAFETY_TELEMETRY_DEST_05,INCOMPLETE,NO,Conceptual route task; map to real footprint pins and copper graph before gameplay
W058,SAFETY_TELEMETRY,POWER_GOOD,SAFETY_TELEMETRY_SOURCE_06,SAFETY_TELEMETRY_DEST_06,INCOMPLETE,NO,Conceptual route task; map to real footprint pins and copper graph before gameplay
W059,SAFETY_TELEMETRY,GND,SAFETY_TELEMETRY_SOURCE_07,SAFETY_TELEMETRY_DEST_07,INCOMPLETE,NO,Conceptual route task; map to real footprint pins and copper graph before gameplay
W060,SAFETY_TELEMETRY,POWER_GOOD,SAFETY_TELEMETRY_SOURCE_08,SAFETY_TELEMETRY_DEST_08,INCOMPLETE,NO,Conceptual route task; map to real footprint pins and copper graph before gameplay
W061,SAFETY_TELEMETRY,POWER_GOOD,SAFETY_TELEMETRY_SOURCE_09,SAFETY_TELEMETRY_DEST_09,INCOMPLETE,NO,Conceptual route task; map to real footprint pins and copper graph before gameplay
W062,SAFETY_TELEMETRY,GND,SAFETY_TELEMETRY_SOURCE_10,SAFETY_TELEMETRY_DEST_10,INCOMPLETE,NO,Conceptual route task; map to real footprint pins and copper graph before gameplay
W063,SAFETY_TELEMETRY,POWER_GOOD,SAFETY_TELEMETRY_SOURCE_11,SAFETY_TELEMETRY_DEST_11,INCOMPLETE,NO,Conceptual route task; map to real footprint pins and copper graph before gameplay
W064,SAFETY_TELEMETRY,POWER_GOOD,SAFETY_TELEMETRY_SOURCE_12,SAFETY_TELEMETRY_DEST_12,INCOMPLETE,NO,Conceptual route task; map to real footprint pins and copper graph before gameplay
W065,SAFETY_TELEMETRY,GND,SAFETY_TELEMETRY_SOURCE_13,SAFETY_TELEMETRY_DEST_13,INCOMPLETE,NO,Conceptual route task; map to real footprint pins and copper graph before gameplay
W066,SAFETY_TELEMETRY,POWER_GOOD,SAFETY_TELEMETRY_SOURCE_14,SAFETY_TELEMETRY_DEST_14,INCOMPLETE,NO,Conceptual route task; map to real footprint pins and copper graph before gameplay
```


## Technical annex 04 — `Via_Plan_14.csv` (verbatim source)

```csv
via_id,x_m,z_m,start_layer,end_layer,initial_state,verified_net
V01,-0.05,-0.025,1,4,MISSING,UNASSIGNED
V02,-0.034,-0.025,1,4,MISSING,UNASSIGNED
V03,-0.018,-0.025,1,4,MISSING,UNASSIGNED
V04,-0.002,-0.025,1,4,MISSING,UNASSIGNED
V05,0.014,-0.025,1,4,MISSING,UNASSIGNED
V06,0.03,-0.025,1,4,MISSING,UNASSIGNED
V07,0.046,-0.025,1,4,MISSING,UNASSIGNED
V08,-0.05,0.025,1,4,MISSING,UNASSIGNED
V09,-0.034,0.025,1,4,MISSING,UNASSIGNED
V10,-0.018,0.025,1,4,MISSING,UNASSIGNED
V11,-0.002,0.025,1,4,MISSING,UNASSIGNED
V12,0.014,0.025,1,4,MISSING,UNASSIGNED
V13,0.03,0.025,1,4,MISSING,UNASSIGNED
V14,0.046,0.025,1,4,MISSING,UNASSIGNED
```


## Technical annex 05 — `Objectives_6.csv` (verbatim source)

```csv
objective,title,must_verify
1,Install 50 components,Exact placement/value/orientation and solder contacts
2,Complete 66 routes and 14 vias,Actual conductive topology and DRC
3,Commission 5/3.3/1.8 V rails,Measured stable startup and power-good
4,Qualify dynamic loading,Ripple/droop/efficiency/thermal checks
5,Repair three seeded faults,Real repair followed by fresh full test
6,W067 disconnect and restore,"Loss of trusted feedback, safe inhibition, repair/reset/new qualification"
```


## Technical annex 06 — `Faults_3.csv` (verbatim source)

```csv
fault_id,name,affected_component,physical_consequence,virtual_repair,fresh_evidence
F1,Feedback divider wrong resistance,R7,Main buck output setpoint deviates,Component replacement,Regulation at 3 loads
F2,Open supply via,V05,Downstream rail loses continuity,Via repair,Continuity and startup sequencing
F3,Switching MOSFET high losses,Q1,Extra losses and temperature rise,Component replacement,Thermal and efficiency test
```


## Technical annex 07 — `Asset_Summary.json` (verbatim source)

```json
{
  "level": 97,
  "title": "Voltage Guardian",
  "board_dimensions_m": [
    0.16,
    0.0016,
    0.11
  ],
  "modular_assets": 68,
  "installables": 50,
  "fbx_including_showcase": 69,
  "glb_including_showcase": 69,
  "triangles_modular": 37372,
  "material_families": 21,
  "shared_pbr_maps": 105,
  "player_route_tasks": 66,
  "functional_vias": 14,
  "critical_jumper": "W067",
  "playable_circuits_included": false,
  "unity_playmode_tested": false
}
```


## Technical annex 08 — `W067_Critical_Recovery.json` (verbatim source)

```json
{
  "wire": "W067",
  "type": "removable_logic_feedback_jumper",
  "source": "L97_MainRailSupervisor_U7.TRUSTED_MAIN_POWER_GOOD_OUT",
  "destination": "L97_PowerSafetyController_U8.TRUSTED_MAIN_POWER_GOOD_IN",
  "initial": "DISCONNECTED",
  "condition_for_objective_6": "historical objectives 1-5 completed; disconnect after normal setup and prove safe inhibition",
  "fault_response": [
    "invalidate feedback quality",
    "disable dependent loads",
    "retain independent diagnostics",
    "latch fault"
  ],
  "recovery": [
    "reinstall real jumper",
    "fresh rail observations",
    "explicit reset",
    "new qualification"
  ]
}
```


## Technical annex 09 — `Material_Palette.json` (verbatim source)

```json
{
  "PCB_FR4": {
    "base_color": "#244438",
    "metallic": 0.15,
    "roughness": 0.82
  },
  "Soldermask_Green": {
    "base_color": "#075a3d",
    "metallic": 0.08,
    "roughness": 0.32
  },
  "Copper": {
    "base_color": "#b26b36",
    "metallic": 0.75,
    "roughness": 0.32
  },
  "ENIG_Gold": {
    "base_color": "#ddae63",
    "metallic": 0.85,
    "roughness": 0.22
  },
  "Solder_Silver": {
    "base_color": "#9eb0b6",
    "metallic": 0.86,
    "roughness": 0.27
  },
  "IC_Black": {
    "base_color": "#171b21",
    "metallic": 0.09,
    "roughness": 0.5
  },
  "Ceramic_Ivory": {
    "base_color": "#dfd6c0",
    "metallic": 0.05,
    "roughness": 0.65
  },
  "Resistor_Charcoal": {
    "base_color": "#303239",
    "metallic": 0.05,
    "roughness": 0.76
  },
  "Inductor_Core": {
    "base_color": "#3c454b",
    "metallic": 0.12,
    "roughness": 0.73
  },
  "Coil_Copper": {
    "base_color": "#b76a2a",
    "metallic": 0.8,
    "roughness": 0.28
  },
  "Connector_Black": {
    "base_color": "#242c37",
    "metallic": 0.13,
    "roughness": 0.58
  },
  "Capacitor_Aluminum": {
    "base_color": "#a7b7c6",
    "metallic": 0.8,
    "roughness": 0.33
  },
  "LED_Green": {
    "base_color": "#2ce06c",
    "metallic": 0.0,
    "roughness": 0.3
  },
  "LED_Amber": {
    "base_color": "#ffb73b",
    "metallic": 0.0,
    "roughness": 0.34
  },
  "Diode_Black": {
    "base_color": "#1d252b",
    "metallic": 0.12,
    "roughness": 0.62
  },
  "Silkscreen_White": {
    "base_color": "#e0ebd8",
    "metallic": 0.03,
    "roughness": 0.84
  },
  "Fault_Red": {
    "base_color": "#c43128",
    "metallic": 0.05,
    "roughness": 0.5
  },
  "Via_Dark": {
    "base_color": "#213542",
    "metallic": 0.4,
    "roughness": 0.5
  },
  "Copper_Plane": {
    "base_color": "#9c773b",
    "metallic": 0.67,
    "roughness": 0.36
  },
  "Jumper_Red": {
    "base_color": "#d6322e",
    "metallic": 0.05,
    "roughness": 0.51
  },
  "Glass_Dark": {
    "base_color": "#172834",
    "metallic": 0.2,
    "roughness": 0.28
  }
}
```


## Technical annex 10 — `README_FIRST.md` (verbatim source)

```markdown
# PROJECT SPARK LEVEL 97 — VOLTAGE GUARDIAN BOARD-ONLY KIT

Actual procedural binary FBX 7400 + GLB geometry. Exactly 68 modular assets (1 empty board, 50 installable parts, 17 extra board-only details) and a separate assembled showcase reference. Designed for a 160×110 mm, four-layer board. 20 PBR texture families with shared maps plus a silkscreen and four reference layer textures. No room or physical UI instruments.

## Import
Import the entire folder under Assets/ (any subfolder), keep Models_FBX, Textures, Data, and Unity intact. Put the C# Editor file in a Unity Editor folder as supplied. Menu: Tools > Project Spark > Levels > 97 > Board Only > 1. Create EMPTY VGR-5 PCB. Then generate native materials and assign them to matching imported FBX material slots. The reference FBX is visual-only and intentionally populated. Unity and render-pipeline import have not been tested here.

## Critical distinction
The artwork is not a validated electrical netlist. `Copper_Routing_Concept_W001_W066.csv` is a route budget with placeholder pads, not a proven schematic; reconcile every pin, supply domain and layer via with Project Spark's real circuit graph. All components, 66 player routes, 14 vias and removable W067 start incomplete/uninstalled. Displayed static factory detail cannot complete player routing. W067 is a signal-validity jumper, not the 5 V power output.

## Physical metrics
FBX and GLB vertices are authored in meters, with Y up, X board width (0.160 m), Z depth (0.110 m). Component local geometry origins are at the board-side contact reference; board top = 0.0008 m. Place each component using `Component_BOM_50.csv`: world local position (x_m, 0.00092, z_m). For table displays, scale only a mesh-only preview, not electrical simulation.

## Production limits
Procedurally modeled realistic-style parts with separate metal pins and contacts, SMDs, copper turns on inductors, plated pad features, board layering and repair coupons. Manufacturing DRC, UV baking, collision/LOD, actual PCB simulation and Unity Play Mode require development. Textures are reference materials; manually confirm slots and metal/smoothness conventions under your Unity pipeline.
```


## Technical annex 11 — `Gameplay_Integration.md` (verbatim source)

```markdown
# Gameplay and virtual tools
- Reuse existing World Space Canvas virtual PCB tool system; no physical instrument meshes are included.
- Use Component_BOM_50.csv for placement IDs and expected component types.
- Treat FBX parts as art; add colliders and component targets for click/touch inspection and repair.
- 66 route tasks and 14 vias begin unmade and are validated by conductive topology, not by visible strips.
- Faults: feedback resistor mismatch R7, open via V05, MOSFET high loss Q1.
- W067 trust feedback path MainRailSupervisor_U7 -> PowerSafetyController_U8, initially missing, installed for commissioning, deliberately disconnected in Objective 6.
- Repair VFX should appear at hit location, and only the authoritative electrical component can mark the fault corrected.
- Requirements for 6/6 objectives in Objectives_6.csv. Preserve all existing levels.
```


## Technical annex 12 — `SparkLevel97BoardOnlyImporter.cs` (source code reference)

```csharp
#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public static class SparkLevel97BoardOnlyImporter
{
    const string MenuBase = "Tools/Project Spark/Levels/97/Board Only/";
    static string FindRoot()
    {
        var guids=AssetDatabase.FindAssets("t:TextAsset Asset_Manifest_68");
        foreach(var guid in guids)
        {
            string path=AssetDatabase.GUIDToAssetPath(guid);
            if(path.EndsWith("Asset_Manifest_68.csv",StringComparison.OrdinalIgnoreCase))
                return Path.GetDirectoryName(Path.GetDirectoryName(path)).Replace('\\','/');
        }
        // avoid assuming a hard-coded install directory
        return null;
    }
    [MenuItem(MenuBase + "1. Create EMPTY VGR-5 PCB")]
    public static void BuildEmpty()
    {
        var root=FindRoot(); if(string.IsNullOrEmpty(root)){Debug.LogError("Level 97 manifest missing. Import entire kit under Assets first.");return;}
        string model=root+"/Models_FBX/00_L97_VGR5_Empty_PCB_160x110mm.fbx";
        var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(model);
        if(!prefab){Debug.LogError("FBX not imported: "+model);return;}
        GameObject parent=new GameObject("L97_VGR5_GAMEPLAY_EMPTY");
        var board=(GameObject)PrefabUtility.InstantiatePrefab(prefab);
        board.name="Physical_Empty_PCB_Factory_Pads_Only";
        board.transform.SetParent(parent.transform,false);
        Selection.activeGameObject=parent;
        Debug.Log("Level 97 EMPTY board spawned. No components, player routes, vias, or W067 are certified. Setup real components via your PCB framework.");
    }
    [MenuItem(MenuBase + "2. Create VISUAL REFERENCE (not gameplay)")]
    public static void BuildReference()
    {
        var root=FindRoot();if(string.IsNullOrEmpty(root)){Debug.LogError("Manifest missing");return;}
        var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(root+"/Models_FBX/L97_COMPLETE_ASSEMBLED_VISUAL_REFERENCE_ONLY.fbx");
        if(!prefab){Debug.LogError("Showcase FBX missing");return;}
        var go=(GameObject)PrefabUtility.InstantiatePrefab(prefab);
        go.name="L97_VISUAL_REFERENCE_ONLY_DO_NOT_CERTIFY";
        foreach(var c in go.GetComponentsInChildren<Collider>(true)) UnityEngine.Object.DestroyImmediate(c);
        Selection.activeGameObject=go;
    }
    [MenuItem(MenuBase + "3. Create Native PBR Materials")]
    public static void Materials()
    {
        var root=FindRoot();if(string.IsNullOrEmpty(root)){Debug.LogError("Manifest missing");return;}
        string output=root+"/GeneratedMaterials";if(!AssetDatabase.IsValidFolder(output))AssetDatabase.CreateFolder(root,"GeneratedMaterials");
        var texGuids=AssetDatabase.FindAssets("t:Texture2D",new[]{root+"/Textures"});
        var texPaths=texGuids.Select(AssetDatabase.GUIDToAssetPath).ToArray();
        foreach(string albedo in texPaths.Where(x=>x.EndsWith("_BaseColor.png",StringComparison.OrdinalIgnoreCase)))
        {
            string matName=Path.GetFileNameWithoutExtension(albedo).Replace("_BaseColor","");
            string assetPath=output+"/"+matName+".mat";
            var shader=Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("HDRP/Lit") ?? Shader.Find("Standard");
            if(!shader){Debug.LogError("No Lit shader found");return;}
            Material mat=AssetDatabase.LoadAssetAtPath<Material>(assetPath);
            if(!mat){mat=new Material(shader);AssetDatabase.CreateAsset(mat,assetPath);}
            else mat.shader=shader;
            var baseTex=AssetDatabase.LoadAssetAtPath<Texture2D>(albedo);
            if(mat.HasProperty("_BaseMap"))mat.SetTexture("_BaseMap",baseTex);
            if(mat.HasProperty("_MainTex"))mat.SetTexture("_MainTex",baseTex);
            var normalPath=albedo.Replace("_BaseColor.png","_Normal.png");
            var normal=AssetDatabase.LoadAssetAtPath<Texture2D>(normalPath);
            if(normal)
            {
                var importer=AssetImporter.GetAtPath(normalPath) as TextureImporter;
                if(importer && importer.textureType!=TextureImporterType.NormalMap){importer.textureType=TextureImporterType.NormalMap;importer.SaveAndReimport();normal=AssetDatabase.LoadAssetAtPath<Texture2D>(normalPath);}
                if(mat.HasProperty("_BumpMap"))mat.SetTexture("_BumpMap",normal);
                if(mat.HasProperty("_NormalMap"))mat.SetTexture("_NormalMap",normal);
                mat.EnableKeyword("_NORMALMAP");
            }
            var ao=AssetDatabase.LoadAssetAtPath<Texture2D>(albedo.Replace("_BaseColor.png","_AO.png"));
            if(ao && mat.HasProperty("_OcclusionMap"))mat.SetTexture("_OcclusionMap",ao);
            var emission=AssetDatabase.LoadAssetAtPath<Texture2D>(albedo.Replace("_BaseColor.png","_Emission.png"));
            if(emission && matName.StartsWith("LED_"))
            {
                if(mat.HasProperty("_EmissionMap"))mat.SetTexture("_EmissionMap",emission);
                if(mat.HasProperty("_EmissionColor"))mat.SetColor("_EmissionColor",Color.white);
                mat.EnableKeyword("_EMISSION");
            }
            EditorUtility.SetDirty(mat);
        }
        AssetDatabase.SaveAssets();AssetDatabase.Refresh();
        Debug.Log("Level 97 native materials created. Assign to matching FBX material slots by name; verify normal maps and metallic/smoothness masks in active pipeline.");
    }
    [MenuItem(MenuBase + "4. Validate Imported Art")]
    public static void ValidateArt()
    {
        var root=FindRoot();if(string.IsNullOrEmpty(root)){Debug.LogError("Manifest missing");return;}
        int found=0;foreach(string g in AssetDatabase.FindAssets("t:Model",new[]{root+"/Models_FBX"}))
        {if(AssetDatabase.GUIDToAssetPath(g).EndsWith(".fbx"))found++;}
        Debug.Log("Level 97 model assets found: "+found+" expected 69 including showcase. This checks art only, NOT live PCB electrical correctness.");
    }
}
#endif
```


## Technical annex 13 — `SparkLevel97BoardTarget.cs` (source code reference)

```csharp
using UnityEngine;
namespace ProjectSpark.Level97 {
    // Optional clickable hit target for existing world-space virtual repair UI.
    // Never grants gameplay completion or bypasses the electrical solver.
    public sealed class SparkLevel97BoardTarget : MonoBehaviour {
        public string componentId; public string padId; public string targetType;
        public Vector3 VfxContactPoint => transform.position;
    }
}
```


## Final instruction to Unity implementation agent

**Implement working code and test it; do not just produce a plan.** First inspect existing Project Spark source and solve the pin-netlist reconciliation blockers with verified records. Then build a non-destructive Level 97 integrated level with playable component placement, copper routing, vias, real rail physics, virtual touch repair, R7/V05/Q1 faults and W067 safety recovery. Preserve old levels and provide objective and regression evidence. No fabricated component values, green status toggles, or ornamental geometry can substitute for authoritative simulation.
