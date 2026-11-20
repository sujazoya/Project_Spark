# PROJECT SPARK — LEVEL 98 MASTER UNITY IMPLEMENTATION PROMPT
**TRANSIENT SHIELD | PIS-6 Power Integrity Stabilizer | Board-Only Advanced AAA Unity Implementation**

> **Developer execution contract.** Use the accompanying `ProjectSpark_Level98_TransientShield_BoardOnly_Realistic_Complete.zip` as the immutable source for filenames, component placement, conceptual route and via IDs, faults and objectives. Build a complete playable Level 98 through verified Project Spark APIs. Source data is **not** a validated pin-level circuit. Do not claim gameplay functionality, Unity compilation or electrical certification until genuinely tested.

**Source-verified totals:** 74 modular FBXs + 74 GLBs; one assembled showcase FBX/GLB; 56 BOM parts; 74 conceptual player copper routes; 16 conceptual vias; three faults; six objectives. PIS-6 PCB is 180 × 120 × 1.6 mm. Source README lists 21 PBR material families and 127 texture maps, which must be reconciled with actual files during import. W075 is a separate detachable feedback jumper and is included once in the 56 BOM placements.

## 01. Mission and architectural boundaries

Implement **Level 98 — Transient Shield** as a realistic PIS-6 power-integrity PCB engineering challenge. The player assembles 56 parts, routes 74 copper tasks, installs 16 vias, commissions 12 V input / 5 V main / 1.8 V load / 3.3 V monitor domains, measures time-varying voltage and current, repairs three physical circuit failures, and qualifies W075 critical feedback recovery. Reuse the current Project Spark futuristic game room, large preview table and existing world-space virtual PCB tools. **NO room, furniture, physical instrument FBXs, imported UI screenshots, or unrelated level scenery.** Preserve Levels 1–97.

## 02. First inspect real project APIs

Before coding, inspect the actual project source for `SparkElectricalSolver`, `SparkCircuitSystem`, `SparkTerminal`, PCB placement / pad / via / trace systems, level evaluator, `LevelGamePlayManager`, World-Space Canvas tooling, click/touch raycast bridge, shop/shelf and save/load systems. Record namespaces, events, return types, assembly definitions, Unity version and active render pipeline. Do not invent extension signatures or overwrite working classes. Use adapters only after the real signatures are confirmed.

## 03. Immutable source-data handling

Import all six source CSV/MD files by their exact names. Keep an original-source copy and a separately generated **verified** schematic/netlist. Display `SOURCE_CONCEPT_NOT_VERIFIED` until approved endpoint mapping exists. Do not convert a placeholder `UNASSIGNED` into a plausible real terminal by inference, nearest-neighbor coordinates or numbering. Report missing or mismatched assets rather than inventing replacements.

## 04. Scale and coordinate system

The physical PCB is X=0.180 m, Z=0.120 m, thickness=0.0016 m, with model units in metres and Y-up. Source placement records store X/Y in **millimetres**; map their `y_mm` field to Unity local Z and component height independently to Unity local Y. Verify the board centre/orientation against the source assembly rather than assuming the sign of Z. Any enlarged level-table representation belongs to a visual-only host; never multiply physical electrical values or collision/routing tolerances by the display scale.

## 05. Empty playable board

Create `Level_98.asset` and `Level_98_Layout.prefab`. Instantiate the empty board substrate and decorative factory-only geometry only. Spawn no placed components initially; the authoritative counts must read `0/56`, `0/74`, `0/16`, objectives `0/6`, and W075 **absent**. Hide or isolate the completed showcase from interactive gameplay. Art geometry, reference textures or cached routes must never silently satisfy installation/continuity.

## 06. Model import and bindings

Resolve all 74 FBXs using `Model_Manifest_74.csv` exact filenames and material slots, and bind all 56 installable components from `BOM_56_Component_Placements.csv`. Keep one prefab per component and meaningful anchors/colliders. The showcase is reference only. Support GLB as inspection fallback, not as a secret duplicate interactable component. Validate UVs, normals, units, mirrored transforms, IC lead visibility and component rotations.

## 07. PBR realism and Unity materials

Import the shared textures as editable PBR source maps. Generate deterministic native `.mat` assets for the detected Built-in, URP or HDRP pipeline. Set albedo sRGB, data maps linear, normal textures to normal type, metallic smoothness channel pack as appropriate, and material mapping by **name**, not FBX submesh index. Metallic copper windings, plated gold, solder tin, polymer sleeves, ceramic caps, IC epoxy, black plastic and FR4 should remain legible in neutral lighting. The package textures are basic procedural PBR references; do not promise artist-authored production bakes.

## 08. AAA board art and inspection camera

Provide top, bottom, low-angle macro, component focus, layer X-ray and thermal-overlay views using current room cameras or dedicated child cameras. Minimize excessive emissive glows and camera obstruction. Allow magnified invisible selection proxies without distorting contact geometry. Instrument panels remain World-Space Canvas, with an uncluttered PCB-centric composition.

## 09. Existing virtual tool hookup

Tools: Inspect, multimeter, continuity, oscilloscope, current probe, thermal scan, net tracer, four-layer view, diagnostics, solder, desolder, component replace, via repair, trace repair and calibration. Use a single flow: UI tool selected → valid physics/UI hit → hit target identified → authority prerequisites checked → transaction attempted → state returned → localized VFX/audio → live telemetry/objective refresh. Clicking an unrelated decorative object must not mutate PCB state. Support mouse and mobile touch.

## 10. BOM placement and solder contacts

Each BOM item is independently bought/selected/installed according to the existing Project Spark interactions, occupies exactly one footprint, and checks identity, orientation, required contacts and pin-to-pad mapping. Count W075 **once** as a placed item; never increment both jumper and component counts separately. A solder operation cannot make an incompatible footprint electrically sound. Track empty, aligned, installed, soldered, desoldered, failed and replaced states separately.

## 11. Four-layer physical stack

Explicitly model L1 top copper, L2 ground plane, L3 power plane and L4 bottom copper. Electrical contact between layers exists only through valid via/through-hole structures with defined span, net and insulation rules. Do not merge all copper because it shares a translucent overlay. Maintain DRC checks for clearance, trace overlap, power-to-ground shorts, pad occupancy, through-via annular rings and legitimate internal plane entry.

## 12. Mandatory pin-level netlist reconciliation

**Hard blocker:** all source `W001–W074` copper-route rows have `source_pad=UNASSIGNED`, `destination_pad=UNASSIGNED`, `electrical_verified=NO - MUST DESIGN NETLIST`. The 16 planned vias have `net_name=UNASSIGNED`. Produce a reviewed `L98_Verified_Component_Pin_Map.csv`, `L98_Verified_Nets.csv`, `L98_Verified_Routes.csv`, `L98_Verified_Via_Spans.csv` and ERC/DRC results. Define real component pins, net classes, ground/power domains, actual coordinates, allowed layers, topology edges and factory copper. Until all records resolve, builder returns `BLOCKED_UNVERIFIED_NETLIST`; no route, power or objective certification.

## 13. Routing task state machine

Each of W001–W074 passes through Missing → Previewed → Placed → DRCValid → ElectricallyConnected → Qualified. Store route identity independently of net identity; a shared net does not automatically fulfill a physically missing required player segment. A decorative source trace is **not** factory copper unless a reviewed manifest explicitly owns it. Removing a route invalidates graph edges and dependent fresh measurement qualification, but historical completed objectives may remain in a durable record if the mission design requires.

## 14. Via task state machine

Each V01–V16 must receive a reviewed net assignment and plated-layer span (source provides L1→L4 placeholders). Validate drill/pad geometry, insulation, correct net, continuity and DRC. Repair tool only affects an existing damaged via when authorized. Missing vias cannot be considered `OPEN_FAULT` until installed; scenario fault injection must occur after the corresponding object has been qualified.

## 15. Electrical separation and energy conservation

Keep 12 V protected input, 5 V rail, 1.8 V core/load rail, 3.3 V monitoring rail, ground, switching nodes, low-voltage supervisor signaling and W075 feedback distinct. Non-ideal regulator models track real current flow, stored capacitor energy, inductive energy, voltage and losses. A safety-control Boolean may gate hardware behavior, never instantiate electrical power from nowhere. Define explicit power limits and soft-start.

## 16. Power-integrity plant

Use a stable fixed-step plant integrating a simplified converter and output network: state variables inductor current, bulk capacitor charge, local bypass charge, output voltage, regulator loop state, load current and loss/temperature. Fast load-step updates must produce physically plausible voltage droop, overshoot, finite recovery, ring-down and thermal dissipation. Coefficients / limits belong in `Level98PowerIntegrityConfig` ScriptableObject; include a deterministic seed for noise/faults. No fake oscilloscope animation or instant voltage set command may qualify tests.

## 17. Decoupling and parasitic model

Represent each relevant capacitor bank with capacitance, ESR, ESL, placement/path resistance/inductance and temperature-dependent degradation where reasonable. High-frequency local MLCC and bulk polymer roles must be differentiated. Real trace/via topology alters equivalent network. Keep simplified simulation stable and configurable; record limitations and avoid claiming hardware-accurate EMI analysis.

## 18. High-speed controlled load-step generator

Implement reproducible transitions 0.5 A → 3.0 A, 3.0 A → 0.5 A and repeated pulse loading (fictional mission bounds). A test requires actual authorized connected load, fresh current sensor readings, valid Kelvin feedback and rail-health permission. Integrate waveform over real simulation time; pause/abort correctly on safety faults. Record test ID, stimulus, start/end timestamps, sensor/calibration versions and qualification evidence.

## 19. Kelvin voltage measurement correctness

Distinguish regulator output voltage from measured **at-load** voltage. Kelvin positive/negative returns must be electrically represented and independently verified. A sense-path fault alters measurement or control behavior according to topology, never rewrites the authoritative actual rail to match a UI number. Instrument probes include grounding/location errors and sample invalidity. The oscilloscope should show raw and calibrated traces with timebase and provenance.

## 20. Fault A — degraded capacitor ESR

Source fault A: **Degraded polymer capacitor ESR**. Identify the actual affected component through a reviewed mapping/seeded scenario (the source does not name which CPx). Apply elevated ESR/temperature in the physical plant and demonstrate increased droop/heating. Player uses waveform, temperature and inspect tools; isolate the rail, replace the correct component, restore solder/graph state, then perform fresh qualification. Do not invent a fixed CP reference as source fact.

## 21. Fault B — open ground-return via

Source fault B: **Open ground return via**. The source gives no definite Vxx identity. Choose and document an approved netlisted via during implementation, inject the fault by removing its conductive edge and model ground bounce/ripple. The player must inspect/net trace, safely restore via continuity, pass isolation/DRC and repeat transient tests. Never award via fault repair from a click on a missing/uninstalled via.

## 22. Fault C — Kelvin-sense connection

Source fault C: **Kelvin sense fault**. Source does not specify which exact copper route/pad. Bind to an approved verified sense path and inject a persistent or intermittent electrical defect. Instrument comparison shows divergence between at-load voltage and reported feedback. Repair the real route/contact, validate calibration, reset as required and capture fresh observations.

## 23. W075 authoritative feedback identity

**Exact source path:** `L98_LoadRailSupervisor_U8.TRUSTED_LOAD_RAIL_VALID_OUT` → detachable **W075** → `L98_PowerIntegritySafety_U9.TRUSTED_LOAD_RAIL_VALID_IN`. W075 is a validity signal, **not** 1.8 V supply, a Kelvin voltage conductor, or the load-current path. W075 appears as one installed component and separate connection target; count it once in the 56-installable BOM.

## 24. W075 disconnection state machine

For normal commissioning, player installs W075 and proves trusted rail-health before Objectives 3–5. After 1–5 are historically certified, supervisor starts the explicit safety test: physically remove W075; solver loses actual continuity; mark trusted feedback INVALID (never zero V); inhibit new fast load-step tests, safely disable the electronic load, preserve protected diagnostics and latch a critical fault. Upon physical reinstall, demand fresh rail/sense calibration, explicit operator reset, independent protected start, then a **new full transient qualification**. Reconnect or visual animation alone must not pass Objective 6.

## 25. Virtual UI design and credible telemetry

Keep the current AAA world-space Virtual PCB Repair UI. Have an Inspect/Assemble/Route/Calibrate/Test/Repair navigation strip and essential 56/74/16 + 0/6 counters. Live waveform modules show VCORE, load current, droop Vmax/Vmin, overshoot, ring amplitude, settling, current sense, board temperature and validity/quality. Differentiate `READY`, `UNPOWERED`, `UNKNOWN`, `OUT_OF_RANGE`, `FAULT_LATCHED`, `QUALIFIED` statuses. Read values only from authority snapshots, never UI constants.

## 26. Repair VFX and touch precision

VFX spawn at actual 3D PCB hit: micro-sparks for solder contact, tiny glow for via repair, short scan pulse for net tracing, restrained solder smoke for legitimate heated operations, diagnostic tint for faults. Disable VFX on rejected operations or show clear error feedback. Pool effects; clamp particles and mobile shader cost. Never use a shader/particle callback to set an electrical state or objective flag.

## 27. Level-specific objective 1 — assembly

Certify only once all 56 unique BOM parts including W075 are correctly installed in exact footprint and electrical orientation with required solder/contact and no fatal wrong parts. Count unique canonical component IDs, not current GameObjects or showcase meshes. Save completion evidence, timestamp and board revision.

## 28. Level-specific objective 2 — routing

Require every W001–W074 reviewed route and V01–V16 installed and electrically qualified; confirm full ERC/DRC, short isolation, actual graph/terminal continuity and correct layers. Do not mark passed if any source placeholder or unidentified factory trace remains.

## 29. Level-specific objective 3 — calibration

Require protected power sequence, working 12 V input, regulated 5 V / 1.8 V / 3.3 V, valid W075, calibrated current and load-voltage sense, decoupling health and valid steady-state ripple/temperature conditions. Define test duration and tolerance in ScriptableObject; reject stale and simulated-but-unpowered readings.

## 30. Level-specific objective 4 — transient test

Run separate start-to-peak, peak-to-idle and repetitive load-step profiles. Validate peak droop/overshoot, ringing, settling within tolerances, current waveform, ripple and thermal state. Each trace must tie to actual load test and reference calibration; a predetermined graph cannot pass.

## 31. Level-specific objective 5 — three repairs

Require fault injection and recovery for A ESR degradation, B open ground via and C Kelvin sense. Each requires measured symptom, authorized repair transaction, physical/electrical change, proof of fault disappearance and a fresh complete load-transient test. Persist individual histories, not just a `3/3` UI increment.

## 32. Level-specific objective 6 — critical feedback recovery

Require a W075 normal baseline, documented supervised unplug event, actual safety-state transition, protective load inhibition, latched invalid-feedback fault, correct physical restoration, recalibration, explicit reset and completely new post-repair qualification. Preserve historical Objective 1–5 certification; never pass from a reconnect-only callback.

## 33. `LevelGamePlayManager` authority contract

Use existing `LevelGamePlayManager` and evaluator APIs discovered in the project. Six objective IDs are canonical, persistent and only the evaluator awards 6/6. UI, showcase art and VFX are read-only observers. Restart and save/load must preserve faults, installed parts, route/via state and historical objective evidence without duplicating completion events.

## 34. Source importer and one-click builder

Create a menu entry **`Tools/Project Spark/Levels/Create Level 98`** with idempotent behavior. It imports assets and source tables, creates a known Level98 folder/scene/prefab, sets links to shared tools, places an empty PCB, creates six objective evaluator definitions and a runtime diagnostics root. It must print blockers for missing scripts, missing models, absent netlist, unresolved material remaps and unverified pads. Running twice must not duplicate wires, board objects or GameObjects.

## 35. Art importer menu compatibility

The supplied `SparkLevel98BoardOnlyImporter.cs` should be inspected for exact menus and compile dependencies; treat it as an **art convenience** rather than an electrical builder. If imported under a nested directory, use `AssetDatabase.FindAssets`, GUID/file discovery and proper Unity paths rather than assuming hard-coded `Assets/ProjectSpark/Level98/`. Preserve project asmdefs and do not blindly change shared source scripts.

## 36. Persistence and regression

Save model binding GUIDs, BOM installed/removed state, net graph revision, route/via status, fault state, W075 states, current/thermal plant state or reproducible restart state, calibration evidence and objective history. Round-trip all gameplay state and protect Levels 1–97 from menu collisions, namespace clashes or scene alterations.

## 37. Visual-only large table preview

Create a purely decorative scaled preview copy for the existing Project Spark room table if desired. Strip or disable all colliders, MonoBehaviour gameplay, simulated power, raycast targets, objective providers and save IDs from this copy. Use real-scale playable PCB for electronics. Reference/showcase cannot contribute a single installation or route count.

## 38. Optimized integration and tests

Create Edit Mode tests for parsing, BOM uniqueness, all route IDs, via spans, W075 path, material/mapping, geometry scale and builder idempotency. Add Play Mode tests for power simulation, current profiles, 3 faults, loss-of-feedback, save/load, UI touch input, VFX origin, and authoritative 6/6. Require Unity project compilation and manual visual inspection; do not pretend offline mesh checks establish Play Mode readiness.

## 39. Implementation deliverables

Deliver actual C# runtime and Editor sources, level asset, layout prefab, source-to-verified netlist reconciliation tables, 74 copper route bindings, 16 via bindings, 56 BOM prefab bindings, three faults, W075 implementation, six objective evaluators, virtual-tool adapters, material mappings, test suite and a clear `IMPLEMENTATION_REPORT.md` with pass/fail details. Explicitly report any unresolved blocker rather than fabricating a green validation badge.

## 40. Final acceptance gate

The level is complete only if Unity compiles, the scene loads, the board starts 0/56, 0/74, 0/16, routes/vias are electrically real, rails and transients follow physical states, all three faults change telemetry and can be repaired, W075 response is tested, previous levels still work, and `LevelGamePlayManager` genuinely certifies **6/6**. Separate visual AAA quality from engineering completion.

## 41. Engineering acceptance tests (84 proposed tests)

### Source import and hierarchy

- **AT-001** Source ZIP imports without duplicate nesting.
- **AT-002** All 74 modular FBXs resolve.
- **AT-003** All 74 GLB references resolve or are explicitly optional.
- **AT-004** Assembled FBX is tagged art-only.
- **AT-005** All 56 BOM entries map to unique prefab identities.
- **AT-006** Board measures 180 by 120 mm.
- **AT-007** Board thickness is 1.6 mm.
- **AT-008** Millimetre placement coordinates map correctly to Unity XZ.
- **AT-009** All source file names match case sensitively.
- **AT-010** BOM W075 is counted exactly once.
- **AT-011** No extra physical test instrument spawned.
- **AT-012** All loaded materials bind by matching source name.

### Electrical schematic and routing

- **AT-013** Unverified source routes block certification.
- **AT-014** All 74 source route IDs are stable.
- **AT-015** Original placeholder pad fields remain unchanged in annex.
- **AT-016** Verified route endpoints have actual component pin numbers.
- **AT-017** Each pad has an electrical pin type and net domain.
- **AT-018** Factory copper cannot bypass required player routes.
- **AT-019** Placing copper creates actual graph edge.
- **AT-020** Removing copper invalidates graph edge.
- **AT-021** Incompatible net crossover triggers DRC.
- **AT-022** Correct layer spans preserve insulation.
- **AT-023** Ground plane not globally shorted to supply.
- **AT-024** Power plane not globally shorted to ground.
- **AT-025** All 16 via IDs are preserved.
- **AT-026** Unassigned via nets block certification.
- **AT-027** Via repair restores actual conductive edge.
- **AT-028** Missing via never counts as installed.

### Component and tools

- **AT-029** All 56 parts initially uninstalled.
- **AT-030** Wrong footprint placement rejected.
- **AT-031** Wrong polarity/orientation rejected.
- **AT-032** Solder only allowed at valid contacts.
- **AT-033** Desolder removes electrical continuity.
- **AT-034** Component replacement checks values.
- **AT-035** Macro inspection focuses correct target.
- **AT-036** Mouse tool selection works.
- **AT-037** Touch tool selection works.
- **AT-038** Hidden layer hit rejected unless allowed.
- **AT-039** VFX at contact point with pooling.
- **AT-040** Rejected repair does not apply success VFX.

### Power and transient physics

- **AT-041** Disconnected input leaves rails unpowered.
- **AT-042** Correct protected source starts softly.
- **AT-043** 5 V rail reaches governed steady state.
- **AT-044** 1.8 V load rail depends on converter state.
- **AT-045** 3.3 V monitoring rail has own validity.
- **AT-046** Output capacitor charge persists across transitions.
- **AT-047** Inductor current remains continuous across timestep.
- **AT-048** Load step induces finite droop.
- **AT-049** Load release can induce overshoot.
- **AT-050** ESR change modifies transient.
- **AT-051** ESL/path inductance modifies spike/ringing.
- **AT-052** Regulator settles over finite time.
- **AT-053** Input power versus loss accounting bounded.
- **AT-054** Thermal state responds to dissipation.
- **AT-055** Oscilloscope trace derives from solver history.
- **AT-056** Unpowered probes report invalid measurements.

### Faults and feedback

- **AT-057** Fault A ESR affects waveform and heating.
- **AT-058** Fault A needs authorized component repair.
- **AT-059** Fault B opens real verified via.
- **AT-060** Fault B repair restores graph/DRC.
- **AT-061** Fault C breaks real Kelvin sensing path.
- **AT-062** Fault C degrades measurement integrity.
- **AT-063** Fault C requires fresh calibration.
- **AT-064** W075 path matches source identities.
- **AT-065** W075 starts disconnected.
- **AT-066** W075 commissioning requires physical install.
- **AT-067** W075 removal invalidates safety feedback.
- **AT-068** W075 removal does not force rail to zero.
- **AT-069** Loss of W075 safely inhibits new load steps.
- **AT-070** Critical feedback loss latches fault.
- **AT-071** Reinstall alone cannot clear latch.
- **AT-072** Explicit operator reset is mandatory.
- **AT-073** Fresh measurements after reset mandatory.
- **AT-074** Fresh complete transient mission required.

### Objectives, persistence and regression

- **AT-075** Initial objective count is 0/6.
- **AT-076** Assembly requires 56 distinct verified placements.
- **AT-077** Routing requires 74 routes and 16 vias.
- **AT-078** Calibration rejects stale readings.
- **AT-079** Transient qualification uses recorded stimuli.
- **AT-080** Three repairs require three independent evidence records.
- **AT-081** W075 scenario cannot skip supervised disconnect.
- **AT-082** Final 6/6 comes from LevelGamePlayManager.
- **AT-083** Showcase prefab cannot affect objective count.
- **AT-084** Saving and loading preserves installed parts.
- **AT-085** Saving and loading preserves routed copper.
- **AT-086** Saving and loading preserves via/fault state.
- **AT-087** Saving and loading preserves historical objective evidence.
- **AT-088** Builder is idempotent.
- **AT-089** Levels 1–97 remain accessible.
- **AT-090** No duplicate Editor menu paths or assembly collisions.

**Acceptance suite count: 90.** Each must have an Edit Mode, Play Mode, import validation, or manual verification mapping and a recorded observed result.

## Annex A — 74 original model records (verbatim source)

`Data/Model_Manifest_74.csv`

```csv
index,name,type,file,glb,x_mm,y_mm,rotation_deg,triangle_count
1,U1,IC,01_L98_U1.fbx,01_L98_U1.glb,-53,6,0,408
2,U2,IC,02_L98_U2.fbx,02_L98_U2.glb,-12,6,0,408
3,U3,IC,03_L98_U3.fbx,03_L98_U3.glb,29,6,0,408
4,U4,IC,04_L98_U4.fbx,04_L98_U4.glb,-53,21,0,408
5,U5,IC,05_L98_U5.fbx,05_L98_U5.glb,-12,21,0,408
6,U6,IC,06_L98_U6.fbx,06_L98_U6.glb,29,21,0,408
7,U7,IC,07_L98_U7.fbx,07_L98_U7.glb,-53,36,0,408
8,U8,IC,08_L98_U8.fbx,08_L98_U8.glb,-12,36,0,408
9,U9,IC,09_L98_U9.fbx,09_L98_U9.glb,29,36,0,408
10,Q1,MOSFET,10_L98_Q1.fbx,10_L98_Q1.glb,21,11,0,72
11,Q2,MOSFET,11_L98_Q2.fbx,11_L98_Q2.glb,35,11,0,72
12,Q3,MOSFET,12_L98_Q3.fbx,12_L98_Q3.glb,49,11,0,72
13,Q4,MOSFET,13_L98_Q4.fbx,13_L98_Q4.glb,21,30,0,72
14,Q5,MOSFET,14_L98_Q5.fbx,14_L98_Q5.glb,35,30,0,72
15,L1,Inductor,15_L98_L1.fbx,15_L98_L1.glb,-43,41,0,2296
16,L2,Inductor,16_L98_L2.fbx,16_L98_L2.glb,8,41,0,2296
17,L3,Inductor,17_L98_L3.fbx,17_L98_L3.glb,55,41,0,2296
18,CP1,Polymer,18_L98_CP1.fbx,18_L98_CP1.glb,-65,-31,0,240
19,CP2,Polymer,19_L98_CP2.fbx,19_L98_CP2.glb,-51,-31,0,240
20,CP3,Polymer,20_L98_CP3.fbx,20_L98_CP3.glb,-37,-31,0,240
21,CP4,Polymer,21_L98_CP4.fbx,21_L98_CP4.glb,-65,-14,0,240
22,CP5,Polymer,22_L98_CP5.fbx,22_L98_CP5.glb,-51,-14,0,240
23,CP6,Polymer,23_L98_CP6.fbx,23_L98_CP6.glb,-37,-14,0,240
24,C1,MLCC,24_L98_C1.fbx,24_L98_C1.glb,-18,-7,0,36
25,C2,MLCC,25_L98_C2.fbx,25_L98_C2.glb,-8,-7,0,36
26,C3,MLCC,26_L98_C3.fbx,26_L98_C3.glb,2,-7,0,36
27,C4,MLCC,27_L98_C4.fbx,27_L98_C4.glb,12,-7,0,36
28,C5,MLCC,28_L98_C5.fbx,28_L98_C5.glb,22,-7,0,36
29,C6,MLCC,29_L98_C6.fbx,29_L98_C6.glb,-18,3,0,36
30,C7,MLCC,30_L98_C7.fbx,30_L98_C7.glb,-8,3,0,36
31,C8,MLCC,31_L98_C8.fbx,31_L98_C8.glb,2,3,0,36
32,C9,MLCC,32_L98_C9.fbx,32_L98_C9.glb,12,3,0,36
33,C10,MLCC,33_L98_C10.fbx,33_L98_C10.glb,22,3,0,36
34,C11,MLCC,34_L98_C11.fbx,34_L98_C11.glb,-18,13,0,36
35,C12,MLCC,35_L98_C12.fbx,35_L98_C12.glb,-8,13,0,36
36,C13,MLCC,36_L98_C13.fbx,36_L98_C13.glb,2,13,0,36
37,C14,MLCC,37_L98_C14.fbx,37_L98_C14.glb,12,13,0,36
38,C15,MLCC,38_L98_C15.fbx,38_L98_C15.glb,22,13,0,36
39,R1,Resistor,39_L98_R1.fbx,39_L98_R1.glb,-55,-10,0,48
40,R2,Resistor,40_L98_R2.fbx,40_L98_R2.glb,-38,-10,0,48
41,R3,Resistor,41_L98_R3.fbx,41_L98_R3.glb,-21,-10,0,48
42,R4,Resistor,42_L98_R4.fbx,42_L98_R4.glb,-4,-10,0,48
43,R5,Resistor,43_L98_R5.fbx,43_L98_R5.glb,13,-10,0,48
44,R6,Resistor,44_L98_R6.fbx,44_L98_R6.glb,30,-10,0,48
45,R7,Resistor,45_L98_R7.fbx,45_L98_R7.glb,-55,-22,0,48
46,R8,Resistor,46_L98_R8.fbx,46_L98_R8.glb,-38,-22,0,48
47,R9,Resistor,47_L98_R9.fbx,47_L98_R9.glb,-21,-22,0,48
48,R10,Resistor,48_L98_R10.fbx,48_L98_R10.glb,-4,-22,0,48
49,R11,Resistor,49_L98_R11.fbx,49_L98_R11.glb,13,-22,0,48
50,J1,Connector,50_L98_J1.fbx,50_L98_J1.glb,-78,0,0,220
51,J2,Connector,51_L98_J2.fbx,51_L98_J2.glb,0,0,0,220
52,J3,Connector,52_L98_J3.fbx,52_L98_J3.glb,78,0,0,324
53,LED1,LED,53_L98_LED1.fbx,53_L98_LED1.glb,-24,-51,0,48
54,LED2,LED,54_L98_LED2.fbx,54_L98_LED2.glb,-14,-51,0,48
55,W075,Jumper,55_L98_W075.fbx,55_L98_W075.glb,62,-30,0,120
56,D,Protection,56_L98_D.fbx,56_L98_D.glb,-72,21,0,36
57,PIS6_Empty_FourLayer_PCB,Board,57_L98_PIS6_Empty_FourLayer_PCB.fbx,57_L98_PIS6_Empty_FourLayer_PCB.glb,0,0,0,3556
58,L1_Top_Copper_Reference,Plane,58_L98_L1_Top_Copper_Reference.fbx,58_L98_L1_Top_Copper_Reference.glb,0,0,0,12
59,L2_Ground_Plane_Reference,Plane,59_L98_L2_Ground_Plane_Reference.fbx,59_L98_L2_Ground_Plane_Reference.glb,0,0,0,12
60,L3_Power_Plane_Reference,Plane,60_L98_L3_Power_Plane_Reference.fbx,60_L98_L3_Power_Plane_Reference.glb,0,0,0,12
61,L4_Bottom_Copper_Reference,Plane,61_L98_L4_Bottom_Copper_Reference.fbx,61_L98_L4_Bottom_Copper_Reference.glb,0,0,0,12
62,Plated_Through_Via,Via,62_L98_Plated_Through_Via.fbx,62_L98_Plated_Through_Via.glb,0,0,0,88
63,Blind_Via_L1_L2,Via,63_L98_Blind_Via_L1_L2.fbx,63_L98_Blind_Via_L1_L2.glb,0,0,0,88
64,Buried_Via_L2_L3,Via,64_L98_Buried_Via_L2_L3.fbx,64_L98_Buried_Via_L2_L3.glb,0,0,0,88
65,Factory_Copper_Route_Reference,Trace,65_L98_Factory_Copper_Route_Reference.fbx,65_L98_Factory_Copper_Route_Reference.glb,0,0,0,84
66,Kelvin_Sense_Pad,TestPad,66_L98_Kelvin_Sense_Pad.fbx,66_L98_Kelvin_Sense_Pad.glb,0,0,0,144
67,Load_Current_Test_Pad,TestPad,67_L98_Load_Current_Test_Pad.fbx,67_L98_Load_Current_Test_Pad.glb,0,0,0,144
68,Ground_Probe_Pad,TestPad,68_L98_Ground_Probe_Pad.fbx,68_L98_Ground_Probe_Pad.glb,0,0,0,144
69,Decoupling_Repair_Strap,Repair,69_L98_Decoupling_Repair_Strap.fbx,69_L98_Decoupling_Repair_Strap.glb,0,0,0,212
70,Via_Repair_Contact,Repair,70_L98_Via_Repair_Contact.fbx,70_L98_Via_Repair_Contact.glb,0,0,0,212
71,PCB_Reference_Silkscreen,Silk,71_L98_PCB_Reference_Silkscreen.fbx,71_L98_PCB_Reference_Silkscreen.glb,0,0,0,60
72,Degraded_ESR_Fault_Visual,Fault,72_L98_Degraded_ESR_Fault_Visual.fbx,72_L98_Degraded_ESR_Fault_Visual.glb,0,0,0,24
73,Open_Ground_Via_Fault_Visual,Fault,73_L98_Open_Ground_Via_Fault_Visual.fbx,73_L98_Open_Ground_Via_Fault_Visual.glb,0,0,0,24
74,Kelvin_Sense_Fault_Visual,Fault,74_L98_Kelvin_Sense_Fault_Visual.fbx,74_L98_Kelvin_Sense_Fault_Visual.glb,0,0,0,24
```

## Annex B — 56 original component placements (verbatim source)

`Data/BOM_56_Component_Placements.csv`

```csv
name,type,region,x_mm,y_mm,rotation_deg,fbx
U1,IC,Control,-53,6,0,01_L98_U1.fbx
U2,IC,Control,-12,6,0,02_L98_U2.fbx
U3,IC,Control,29,6,0,03_L98_U3.fbx
U4,IC,Control,-53,21,0,04_L98_U4.fbx
U5,IC,Control,-12,21,0,05_L98_U5.fbx
U6,IC,Control,29,21,0,06_L98_U6.fbx
U7,IC,Control,-53,36,0,07_L98_U7.fbx
U8,IC,Control,-12,36,0,08_L98_U8.fbx
U9,IC,Control,29,36,0,09_L98_U9.fbx
Q1,MOSFET,Load,21,11,0,10_L98_Q1.fbx
Q2,MOSFET,Load,35,11,0,11_L98_Q2.fbx
Q3,MOSFET,Load,49,11,0,12_L98_Q3.fbx
Q4,MOSFET,Load,21,30,0,13_L98_Q4.fbx
Q5,MOSFET,Load,35,30,0,14_L98_Q5.fbx
L1,Inductor,Power,-43,41,0,15_L98_L1.fbx
L2,Inductor,Power,8,41,0,16_L98_L2.fbx
L3,Inductor,Power,55,41,0,17_L98_L3.fbx
CP1,Polymer,Decoupling,-65,-31,0,18_L98_CP1.fbx
CP2,Polymer,Decoupling,-51,-31,0,19_L98_CP2.fbx
CP3,Polymer,Decoupling,-37,-31,0,20_L98_CP3.fbx
CP4,Polymer,Decoupling,-65,-14,0,21_L98_CP4.fbx
CP5,Polymer,Decoupling,-51,-14,0,22_L98_CP5.fbx
CP6,Polymer,Decoupling,-37,-14,0,23_L98_CP6.fbx
C1,MLCC,Decoupling,-18,-7,0,24_L98_C1.fbx
C2,MLCC,Decoupling,-8,-7,0,25_L98_C2.fbx
C3,MLCC,Decoupling,2,-7,0,26_L98_C3.fbx
C4,MLCC,Decoupling,12,-7,0,27_L98_C4.fbx
C5,MLCC,Decoupling,22,-7,0,28_L98_C5.fbx
C6,MLCC,Decoupling,-18,3,0,29_L98_C6.fbx
C7,MLCC,Decoupling,-8,3,0,30_L98_C7.fbx
C8,MLCC,Decoupling,2,3,0,31_L98_C8.fbx
C9,MLCC,Decoupling,12,3,0,32_L98_C9.fbx
C10,MLCC,Decoupling,22,3,0,33_L98_C10.fbx
C11,MLCC,Decoupling,-18,13,0,34_L98_C11.fbx
C12,MLCC,Decoupling,-8,13,0,35_L98_C12.fbx
C13,MLCC,Decoupling,2,13,0,36_L98_C13.fbx
C14,MLCC,Decoupling,12,13,0,37_L98_C14.fbx
C15,MLCC,Decoupling,22,13,0,38_L98_C15.fbx
R1,Resistor,Signal,-55,-10,0,39_L98_R1.fbx
R2,Resistor,Signal,-38,-10,0,40_L98_R2.fbx
R3,Resistor,Signal,-21,-10,0,41_L98_R3.fbx
R4,Resistor,Signal,-4,-10,0,42_L98_R4.fbx
R5,Resistor,Signal,13,-10,0,43_L98_R5.fbx
R6,Resistor,Signal,30,-10,0,44_L98_R6.fbx
R7,Resistor,Signal,-55,-22,0,45_L98_R7.fbx
R8,Resistor,Signal,-38,-22,0,46_L98_R8.fbx
R9,Resistor,Signal,-21,-22,0,47_L98_R9.fbx
R10,Resistor,Signal,-4,-22,0,48_L98_R10.fbx
R11,Resistor,Signal,13,-22,0,49_L98_R11.fbx
J1,Connector,Connector,-78,0,0,50_L98_J1.fbx
J2,Connector,Connector,0,0,0,51_L98_J2.fbx
J3,Connector,Connector,78,0,0,52_L98_J3.fbx
LED1,LED,Status,-24,-51,0,53_L98_LED1.fbx
LED2,LED,Status,-14,-51,0,54_L98_LED2.fbx
W075,Jumper,Safety,62,-30,0,55_L98_W075.fbx
D,Protection,Input,-72,21,0,56_L98_D.fbx
```

## Annex C — 74 conceptual copper tasks — NOT verified (verbatim source)

`Data/Copper_Routing_Tasks_74_CONCEPT_ONLY.csv`

```csv
route_id,subsystem,source_pad,destination_pad,status,electrical_verified
W001,InputPower,UNASSIGNED,UNASSIGNED,INCOMPLETE,NO - MUST DESIGN NETLIST
W002,InputPower,UNASSIGNED,UNASSIGNED,INCOMPLETE,NO - MUST DESIGN NETLIST
W003,InputPower,UNASSIGNED,UNASSIGNED,INCOMPLETE,NO - MUST DESIGN NETLIST
W004,InputPower,UNASSIGNED,UNASSIGNED,INCOMPLETE,NO - MUST DESIGN NETLIST
W005,InputPower,UNASSIGNED,UNASSIGNED,INCOMPLETE,NO - MUST DESIGN NETLIST
W006,InputPower,UNASSIGNED,UNASSIGNED,INCOMPLETE,NO - MUST DESIGN NETLIST
W007,InputPower,UNASSIGNED,UNASSIGNED,INCOMPLETE,NO - MUST DESIGN NETLIST
W008,InputPower,UNASSIGNED,UNASSIGNED,INCOMPLETE,NO - MUST DESIGN NETLIST
W009,InputPower,UNASSIGNED,UNASSIGNED,INCOMPLETE,NO - MUST DESIGN NETLIST
W010,InputPower,UNASSIGNED,UNASSIGNED,INCOMPLETE,NO - MUST DESIGN NETLIST
W011,InputPower,UNASSIGNED,UNASSIGNED,INCOMPLETE,NO - MUST DESIGN NETLIST
W012,InputPower,UNASSIGNED,UNASSIGNED,INCOMPLETE,NO - MUST DESIGN NETLIST
W013,MainConversion,UNASSIGNED,UNASSIGNED,INCOMPLETE,NO - MUST DESIGN NETLIST
W014,MainConversion,UNASSIGNED,UNASSIGNED,INCOMPLETE,NO - MUST DESIGN NETLIST
W015,MainConversion,UNASSIGNED,UNASSIGNED,INCOMPLETE,NO - MUST DESIGN NETLIST
W016,MainConversion,UNASSIGNED,UNASSIGNED,INCOMPLETE,NO - MUST DESIGN NETLIST
W017,MainConversion,UNASSIGNED,UNASSIGNED,INCOMPLETE,NO - MUST DESIGN NETLIST
W018,MainConversion,UNASSIGNED,UNASSIGNED,INCOMPLETE,NO - MUST DESIGN NETLIST
W019,MainConversion,UNASSIGNED,UNASSIGNED,INCOMPLETE,NO - MUST DESIGN NETLIST
W020,MainConversion,UNASSIGNED,UNASSIGNED,INCOMPLETE,NO - MUST DESIGN NETLIST
W021,MainConversion,UNASSIGNED,UNASSIGNED,INCOMPLETE,NO - MUST DESIGN NETLIST
W022,MainConversion,UNASSIGNED,UNASSIGNED,INCOMPLETE,NO - MUST DESIGN NETLIST
W023,MainConversion,UNASSIGNED,UNASSIGNED,INCOMPLETE,NO - MUST DESIGN NETLIST
W024,MainConversion,UNASSIGNED,UNASSIGNED,INCOMPLETE,NO - MUST DESIGN NETLIST
W025,MainConversion,UNASSIGNED,UNASSIGNED,INCOMPLETE,NO - MUST DESIGN NETLIST
W026,MainConversion,UNASSIGNED,UNASSIGNED,INCOMPLETE,NO - MUST DESIGN NETLIST
W027,MainConversion,UNASSIGNED,UNASSIGNED,INCOMPLETE,NO - MUST DESIGN NETLIST
W028,MainConversion,UNASSIGNED,UNASSIGNED,INCOMPLETE,NO - MUST DESIGN NETLIST
W029,Decoupling,UNASSIGNED,UNASSIGNED,INCOMPLETE,NO - MUST DESIGN NETLIST
W030,Decoupling,UNASSIGNED,UNASSIGNED,INCOMPLETE,NO - MUST DESIGN NETLIST
W031,Decoupling,UNASSIGNED,UNASSIGNED,INCOMPLETE,NO - MUST DESIGN NETLIST
W032,Decoupling,UNASSIGNED,UNASSIGNED,INCOMPLETE,NO - MUST DESIGN NETLIST
W033,Decoupling,UNASSIGNED,UNASSIGNED,INCOMPLETE,NO - MUST DESIGN NETLIST
W034,Decoupling,UNASSIGNED,UNASSIGNED,INCOMPLETE,NO - MUST DESIGN NETLIST
W035,Decoupling,UNASSIGNED,UNASSIGNED,INCOMPLETE,NO - MUST DESIGN NETLIST
W036,Decoupling,UNASSIGNED,UNASSIGNED,INCOMPLETE,NO - MUST DESIGN NETLIST
W037,Decoupling,UNASSIGNED,UNASSIGNED,INCOMPLETE,NO - MUST DESIGN NETLIST
W038,Decoupling,UNASSIGNED,UNASSIGNED,INCOMPLETE,NO - MUST DESIGN NETLIST
W039,Decoupling,UNASSIGNED,UNASSIGNED,INCOMPLETE,NO - MUST DESIGN NETLIST
W040,Decoupling,UNASSIGNED,UNASSIGNED,INCOMPLETE,NO - MUST DESIGN NETLIST
W041,Decoupling,UNASSIGNED,UNASSIGNED,INCOMPLETE,NO - MUST DESIGN NETLIST
W042,Decoupling,UNASSIGNED,UNASSIGNED,INCOMPLETE,NO - MUST DESIGN NETLIST
W043,Decoupling,UNASSIGNED,UNASSIGNED,INCOMPLETE,NO - MUST DESIGN NETLIST
W044,Decoupling,UNASSIGNED,UNASSIGNED,INCOMPLETE,NO - MUST DESIGN NETLIST
W045,HighSpeedLoad,UNASSIGNED,UNASSIGNED,INCOMPLETE,NO - MUST DESIGN NETLIST
W046,HighSpeedLoad,UNASSIGNED,UNASSIGNED,INCOMPLETE,NO - MUST DESIGN NETLIST
W047,HighSpeedLoad,UNASSIGNED,UNASSIGNED,INCOMPLETE,NO - MUST DESIGN NETLIST
W048,HighSpeedLoad,UNASSIGNED,UNASSIGNED,INCOMPLETE,NO - MUST DESIGN NETLIST
W049,HighSpeedLoad,UNASSIGNED,UNASSIGNED,INCOMPLETE,NO - MUST DESIGN NETLIST
W050,HighSpeedLoad,UNASSIGNED,UNASSIGNED,INCOMPLETE,NO - MUST DESIGN NETLIST
W051,HighSpeedLoad,UNASSIGNED,UNASSIGNED,INCOMPLETE,NO - MUST DESIGN NETLIST
W052,HighSpeedLoad,UNASSIGNED,UNASSIGNED,INCOMPLETE,NO - MUST DESIGN NETLIST
W053,HighSpeedLoad,UNASSIGNED,UNASSIGNED,INCOMPLETE,NO - MUST DESIGN NETLIST
W054,HighSpeedLoad,UNASSIGNED,UNASSIGNED,INCOMPLETE,NO - MUST DESIGN NETLIST
W055,HighSpeedLoad,UNASSIGNED,UNASSIGNED,INCOMPLETE,NO - MUST DESIGN NETLIST
W056,HighSpeedLoad,UNASSIGNED,UNASSIGNED,INCOMPLETE,NO - MUST DESIGN NETLIST
W057,MeasurementsProtection,UNASSIGNED,UNASSIGNED,INCOMPLETE,NO - MUST DESIGN NETLIST
W058,MeasurementsProtection,UNASSIGNED,UNASSIGNED,INCOMPLETE,NO - MUST DESIGN NETLIST
W059,MeasurementsProtection,UNASSIGNED,UNASSIGNED,INCOMPLETE,NO - MUST DESIGN NETLIST
W060,MeasurementsProtection,UNASSIGNED,UNASSIGNED,INCOMPLETE,NO - MUST DESIGN NETLIST
W061,MeasurementsProtection,UNASSIGNED,UNASSIGNED,INCOMPLETE,NO - MUST DESIGN NETLIST
W062,MeasurementsProtection,UNASSIGNED,UNASSIGNED,INCOMPLETE,NO - MUST DESIGN NETLIST
W063,MeasurementsProtection,UNASSIGNED,UNASSIGNED,INCOMPLETE,NO - MUST DESIGN NETLIST
W064,MeasurementsProtection,UNASSIGNED,UNASSIGNED,INCOMPLETE,NO - MUST DESIGN NETLIST
W065,MeasurementsProtection,UNASSIGNED,UNASSIGNED,INCOMPLETE,NO - MUST DESIGN NETLIST
W066,MeasurementsProtection,UNASSIGNED,UNASSIGNED,INCOMPLETE,NO - MUST DESIGN NETLIST
W067,MeasurementsProtection,UNASSIGNED,UNASSIGNED,INCOMPLETE,NO - MUST DESIGN NETLIST
W068,MeasurementsProtection,UNASSIGNED,UNASSIGNED,INCOMPLETE,NO - MUST DESIGN NETLIST
W069,MeasurementsProtection,UNASSIGNED,UNASSIGNED,INCOMPLETE,NO - MUST DESIGN NETLIST
W070,MeasurementsProtection,UNASSIGNED,UNASSIGNED,INCOMPLETE,NO - MUST DESIGN NETLIST
W071,MeasurementsProtection,UNASSIGNED,UNASSIGNED,INCOMPLETE,NO - MUST DESIGN NETLIST
W072,MeasurementsProtection,UNASSIGNED,UNASSIGNED,INCOMPLETE,NO - MUST DESIGN NETLIST
W073,MeasurementsProtection,UNASSIGNED,UNASSIGNED,INCOMPLETE,NO - MUST DESIGN NETLIST
W074,MeasurementsProtection,UNASSIGNED,UNASSIGNED,INCOMPLETE,NO - MUST DESIGN NETLIST
```

## Annex D — 16 conceptual via tasks — nets unassigned (verbatim source)

`Data/Via_Plan_16_CONCEPT_ONLY.csv`

```csv
via_id,start_layer,end_layer,net_name,state
V01,L1,L4,UNASSIGNED,MISSING
V02,L1,L4,UNASSIGNED,MISSING
V03,L1,L4,UNASSIGNED,MISSING
V04,L1,L4,UNASSIGNED,MISSING
V05,L1,L4,UNASSIGNED,MISSING
V06,L1,L4,UNASSIGNED,MISSING
V07,L1,L4,UNASSIGNED,MISSING
V08,L1,L4,UNASSIGNED,MISSING
V09,L1,L4,UNASSIGNED,MISSING
V10,L1,L4,UNASSIGNED,MISSING
V11,L1,L4,UNASSIGNED,MISSING
V12,L1,L4,UNASSIGNED,MISSING
V13,L1,L4,UNASSIGNED,MISSING
V14,L1,L4,UNASSIGNED,MISSING
V15,L1,L4,UNASSIGNED,MISSING
V16,L1,L4,UNASSIGNED,MISSING
```

## Annex E — Three original fault definitions (verbatim source)

`Data/Faults_3.csv`

```csv
id,fault,effect,repair
A,Degraded polymer capacitor ESR,Worse voltage droop and heating,Replace faulted capacitor; retest
B,Open ground return via,Ground bounce and ripple,Restore actual via connectivity
C,Kelvin sense fault,Incorrect measured load rail,Repair sensor trace; recalibrate
```

## Annex F — Six original objective definitions (verbatim source)

`Data/Six_Objectives.csv`

```csv
objective,name,authoritative_pass
1,Assemble,All 56 correct placements
2,Route,"74 route tasks, 16 vias, verified netlist"
3,Calibrate,"Protected rails, voltage and current calibration"
4,Transient,Qualified droop overshoot recovery
5,Repair,"ESR, open via and Kelvin faults repaired then new run"
6,W075,"Physical disconnection, safe inhibit, repair, explicit reset and fresh qualification"
```

## Annex G — W075 source-critical safety contract (verbatim source)

`Data/W075_Critical_Feedback.md`

```markdown
# W075 — trusted load rail validity

L98_LoadRailSupervisor_U8.TRUSTED_LOAD_RAIL_VALID_OUT -> detachable W075 -> L98_PowerIntegritySafety_U9.TRUSTED_LOAD_RAIL_VALID_IN.

W075 is a logical feedback jumper, never the power rail itself. Its geometry is included as a separately placeable component. Initial game state: uninstalled. Objective 6: after prior normal certification remove W075, reject stale measurements, inhibit load steps, latch fault, physically reconnect, recalibrate, explicitly reset and complete fresh transient qualification.

No supplied file certifies a real pin-level circuit. Implement a verified netlist and safety state machine before enabling objectives.
```

## Annex H — Package README (verbatim source)
```markdown
# PROJECT SPARK — LEVEL 98 TRANSIENT SHIELD
## Board-only realistic 3D geometry

Actual geometry: 74 separate FBX + 74 GLB modular files; 56 separately placeable components; 18 board-only structural and diagnostic assets; assembled visualization FBX + GLB. All units use METRES; PCB 0.180 x 0.120 x 0.0016 m.

### Scope
Only PCB substrate, components, copper layers, solder contacts, vias, repair details, W075. NO room, furniture, test instrument FBXs, UI, or game mechanics. Use existing Project Spark world-space tool interfaces.

### Unity import
Copy package content under Assets/ProjectSpark/Level98/. Ensure Editor script is in an Editor folder. Run Tools > Project Spark > Levels > 98 > Board Only > 1. Build EMPTY PIS-6 Board. Then generate native materials. If the FBX import reports geometry or axis issues, use the matching GLBs as reference and inspect Unity's Model Import settings.

### Crucial gameplay guard
The assembled showcase is VISUAL ONLY. Actual gameplay starts with 56 components UNINSTALLED, 74 routes INCOMPLETE, 16 vias MISSING, W075 UNINSTALLED. Some factory copper pads and paths are decorative; do not use them to certify circuit topology. The route and via planning CSV files do NOT constitute a pin-verified netlist.

### Quality limits
Original procedural realistic-style meshes and basic authored PBR maps. FBX export is FBX Binary version 7400 with geometry, UV layers and material name slots. Offline validation checks file structures and GLB loading, not Unity FBX import, assembly fit, Play Mode, manufacturer footprints or authoritative electrical operation.
```

## Annex I — Actual board-only FBX/GLB filenames

- `01_L98_U1.fbx` | `01_L98_U1.glb` | IC | U1
- `02_L98_U2.fbx` | `02_L98_U2.glb` | IC | U2
- `03_L98_U3.fbx` | `03_L98_U3.glb` | IC | U3
- `04_L98_U4.fbx` | `04_L98_U4.glb` | IC | U4
- `05_L98_U5.fbx` | `05_L98_U5.glb` | IC | U5
- `06_L98_U6.fbx` | `06_L98_U6.glb` | IC | U6
- `07_L98_U7.fbx` | `07_L98_U7.glb` | IC | U7
- `08_L98_U8.fbx` | `08_L98_U8.glb` | IC | U8
- `09_L98_U9.fbx` | `09_L98_U9.glb` | IC | U9
- `10_L98_Q1.fbx` | `10_L98_Q1.glb` | MOSFET | Q1
- `11_L98_Q2.fbx` | `11_L98_Q2.glb` | MOSFET | Q2
- `12_L98_Q3.fbx` | `12_L98_Q3.glb` | MOSFET | Q3
- `13_L98_Q4.fbx` | `13_L98_Q4.glb` | MOSFET | Q4
- `14_L98_Q5.fbx` | `14_L98_Q5.glb` | MOSFET | Q5
- `15_L98_L1.fbx` | `15_L98_L1.glb` | Inductor | L1
- `16_L98_L2.fbx` | `16_L98_L2.glb` | Inductor | L2
- `17_L98_L3.fbx` | `17_L98_L3.glb` | Inductor | L3
- `18_L98_CP1.fbx` | `18_L98_CP1.glb` | Polymer | CP1
- `19_L98_CP2.fbx` | `19_L98_CP2.glb` | Polymer | CP2
- `20_L98_CP3.fbx` | `20_L98_CP3.glb` | Polymer | CP3
- `21_L98_CP4.fbx` | `21_L98_CP4.glb` | Polymer | CP4
- `22_L98_CP5.fbx` | `22_L98_CP5.glb` | Polymer | CP5
- `23_L98_CP6.fbx` | `23_L98_CP6.glb` | Polymer | CP6
- `24_L98_C1.fbx` | `24_L98_C1.glb` | MLCC | C1
- `25_L98_C2.fbx` | `25_L98_C2.glb` | MLCC | C2
- `26_L98_C3.fbx` | `26_L98_C3.glb` | MLCC | C3
- `27_L98_C4.fbx` | `27_L98_C4.glb` | MLCC | C4
- `28_L98_C5.fbx` | `28_L98_C5.glb` | MLCC | C5
- `29_L98_C6.fbx` | `29_L98_C6.glb` | MLCC | C6
- `30_L98_C7.fbx` | `30_L98_C7.glb` | MLCC | C7
- `31_L98_C8.fbx` | `31_L98_C8.glb` | MLCC | C8
- `32_L98_C9.fbx` | `32_L98_C9.glb` | MLCC | C9
- `33_L98_C10.fbx` | `33_L98_C10.glb` | MLCC | C10
- `34_L98_C11.fbx` | `34_L98_C11.glb` | MLCC | C11
- `35_L98_C12.fbx` | `35_L98_C12.glb` | MLCC | C12
- `36_L98_C13.fbx` | `36_L98_C13.glb` | MLCC | C13
- `37_L98_C14.fbx` | `37_L98_C14.glb` | MLCC | C14
- `38_L98_C15.fbx` | `38_L98_C15.glb` | MLCC | C15
- `39_L98_R1.fbx` | `39_L98_R1.glb` | Resistor | R1
- `40_L98_R2.fbx` | `40_L98_R2.glb` | Resistor | R2
- `41_L98_R3.fbx` | `41_L98_R3.glb` | Resistor | R3
- `42_L98_R4.fbx` | `42_L98_R4.glb` | Resistor | R4
- `43_L98_R5.fbx` | `43_L98_R5.glb` | Resistor | R5
- `44_L98_R6.fbx` | `44_L98_R6.glb` | Resistor | R6
- `45_L98_R7.fbx` | `45_L98_R7.glb` | Resistor | R7
- `46_L98_R8.fbx` | `46_L98_R8.glb` | Resistor | R8
- `47_L98_R9.fbx` | `47_L98_R9.glb` | Resistor | R9
- `48_L98_R10.fbx` | `48_L98_R10.glb` | Resistor | R10
- `49_L98_R11.fbx` | `49_L98_R11.glb` | Resistor | R11
- `50_L98_J1.fbx` | `50_L98_J1.glb` | Connector | J1
- `51_L98_J2.fbx` | `51_L98_J2.glb` | Connector | J2
- `52_L98_J3.fbx` | `52_L98_J3.glb` | Connector | J3
- `53_L98_LED1.fbx` | `53_L98_LED1.glb` | LED | LED1
- `54_L98_LED2.fbx` | `54_L98_LED2.glb` | LED | LED2
- `55_L98_W075.fbx` | `55_L98_W075.glb` | Jumper | W075
- `56_L98_D.fbx` | `56_L98_D.glb` | Protection | D
- `57_L98_PIS6_Empty_FourLayer_PCB.fbx` | `57_L98_PIS6_Empty_FourLayer_PCB.glb` | Board | PIS6_Empty_FourLayer_PCB
- `58_L98_L1_Top_Copper_Reference.fbx` | `58_L98_L1_Top_Copper_Reference.glb` | Plane | L1_Top_Copper_Reference
- `59_L98_L2_Ground_Plane_Reference.fbx` | `59_L98_L2_Ground_Plane_Reference.glb` | Plane | L2_Ground_Plane_Reference
- `60_L98_L3_Power_Plane_Reference.fbx` | `60_L98_L3_Power_Plane_Reference.glb` | Plane | L3_Power_Plane_Reference
- `61_L98_L4_Bottom_Copper_Reference.fbx` | `61_L98_L4_Bottom_Copper_Reference.glb` | Plane | L4_Bottom_Copper_Reference
- `62_L98_Plated_Through_Via.fbx` | `62_L98_Plated_Through_Via.glb` | Via | Plated_Through_Via
- `63_L98_Blind_Via_L1_L2.fbx` | `63_L98_Blind_Via_L1_L2.glb` | Via | Blind_Via_L1_L2
- `64_L98_Buried_Via_L2_L3.fbx` | `64_L98_Buried_Via_L2_L3.glb` | Via | Buried_Via_L2_L3
- `65_L98_Factory_Copper_Route_Reference.fbx` | `65_L98_Factory_Copper_Route_Reference.glb` | Trace | Factory_Copper_Route_Reference
- `66_L98_Kelvin_Sense_Pad.fbx` | `66_L98_Kelvin_Sense_Pad.glb` | TestPad | Kelvin_Sense_Pad
- `67_L98_Load_Current_Test_Pad.fbx` | `67_L98_Load_Current_Test_Pad.glb` | TestPad | Load_Current_Test_Pad
- `68_L98_Ground_Probe_Pad.fbx` | `68_L98_Ground_Probe_Pad.glb` | TestPad | Ground_Probe_Pad
- `69_L98_Decoupling_Repair_Strap.fbx` | `69_L98_Decoupling_Repair_Strap.glb` | Repair | Decoupling_Repair_Strap
- `70_L98_Via_Repair_Contact.fbx` | `70_L98_Via_Repair_Contact.glb` | Repair | Via_Repair_Contact
- `71_L98_PCB_Reference_Silkscreen.fbx` | `71_L98_PCB_Reference_Silkscreen.glb` | Silk | PCB_Reference_Silkscreen
- `72_L98_Degraded_ESR_Fault_Visual.fbx` | `72_L98_Degraded_ESR_Fault_Visual.glb` | Fault | Degraded_ESR_Fault_Visual
- `73_L98_Open_Ground_Via_Fault_Visual.fbx` | `73_L98_Open_Ground_Via_Fault_Visual.glb` | Fault | Open_Ground_Via_Fault_Visual
- `74_L98_Kelvin_Sense_Fault_Visual.fbx` | `74_L98_Kelvin_Sense_Fault_Visual.glb` | Fault | Kelvin_Sense_Fault_Visual

## Annex J — Real project API discovery checklist

- [ ] Unity version / render pipeline / Input System
- [ ] Existing PCB board, footprint, pad, copper-route and via APIs
- [ ] Existing `SparkTerminal` and electrical graph APIs
- [ ] `LevelGamePlayManager` evaluation interface
- [ ] Virtual PCB repair UI tool-selection and contact-point raycast bridge
- [ ] AssetDatabase importer path and Unity asmdef boundaries
- [ ] Persistence, scene loading and regression suite

## Annex K — Minimum verified pin/net reconciliation columns

`component_ref, manufacturer_or_simulated_part, package, pin_number, pin_function, pin_electrical_type, pad_id, pad_local_x_mm, pad_local_z_mm, net_name, voltage_domain, allowed_copper_layers, source_task_id, review_status, reviewer`

**STOP GATE:** Until these columns are populated for every required conductive endpoint and the verified netlist passes ERC/DRC, report `BLOCKED_UNVERIFIED_NETLIST` and do not certify connection progress.

## Annex L — Unity handoff and limitations

- Import original art; preserve the FBX naming and separate completed showcase.
- Generate verified schematic and footprint pin map **before** implementing objective 2.
- Integrate virtual UI with authoritative solver, not mock UI button flags.
- Run actual Unity compile/Play Mode; no such claims are made by this prompt.
- Package changes as additive Level 98 files; do not overwrite Levels 1–97.

