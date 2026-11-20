# PROJECT SPARK — LEVEL 109: THERMAL ENDURANCE
## MASTER UNITY IMPLEMENTATION PROMPT — TRX-17 THERMAL RELIABILITY MOTHERBOARD

**Project:** Project Spark | **Level:** 109 | **Board:** TRX-17 | **Engine:** Unity 6.3 LTS / URP | **Platforms:** Windows PC and Android | **Status:** implementation specification, NOT compiled or validated.

**Give this prompt to Codex alongside the complete existing Project Spark Unity project and the Level 109 asset ZIP.** Reference packages: `ProjectSpark_Level109_TRX17_Realistic_Assets_160.zip`, `ProjectSpark_Level109_TRX17_Complete_Board.zip`, and the optional assembled visual-only `ProjectSpark_Level109_TRX17_Complete_Assembled_Board.fbx` / `.glb`. The Level 107 and 108 master prompts establish the desired engineering rigor; Level 109 is a new thermal/reliability mission, not a clone of previous levels.

## 0. PRIMARY AGENT MANDATE — READ BEFORE CHANGING ANY CODE

Act as the senior Unity gameplay, electrical and thermal-systems engineer for Project Spark. Implement **Level 109 — Thermal Endurance**, a deterministic, evidence-based interactive motherboard assembly, thermal diagnosis, repair and endurance qualification level. Integrate with the **existing project**. Before coding, inspect the actual scripts, scenes, prefabs, assemblies, selection/input, camera tools, component placement/shelf/recycle logic, SparkCircuitSystem/SparkElectricalNetworkGraph, SparkElectricalSolveContext, SparkTerminal, existing multimeter and virtual instrumentation, save system and the real `LevelGamePlayManager` APIs. Prefer existing verified systems; all class names below are proposals, not claims that classes exist. Avoid LINQ and per-frame `Find`; cache references, use event-driven UI, deterministic fixed-step simulation and bounded memory. Keep the production URP lab style scientific, realistic and non-cartoonish. Do not modify or break Levels 1–108.

**CRITICAL: ASSET APPEARANCE IS NOT ELECTRICAL AUTHORITY.** An assembled board model is a reference/showcase only, never the player's completed scene or proof of installed parts, routed traces, functional vias, power distribution, temperature readings or an objective. The supplied 164 routing records and 54 via tasks have `UNASSIGNED` nets/pins/layers or spans and `NOT_CERTIFIABLE` validation. The physical pins on W165 are likewise unassigned. Build a safe art/thermal demonstration if genuine net-level approval is unavailable, but block certification of electrical connectivity and final 6/6; report the missing design inputs. Do not fabricate pin identifiers, successful tests, manufacturer specs or completed objectives.

## 1. EXACT LEVEL 109 SCOPE AND ASSET REQUIREMENTS

- Target PCB: **300 × 220 × 1.6 mm**, intended **10-layer board**, dark graphite-blue soldermask, fine copper/gold detail, separate thermal-overlay presentation.
- **160** modular FBX visual models with matching **160** GLB models, comprising **124 installable electronic/component entries** and **36 board/support/thermal visual entries**. Verify actual ZIP contents and data against these targets; do not silently assume file counts imply correct import.
- An additional **fully assembled visual reference FBX/GLB** is provided separately. Inspect import axis, unit scaling, normals, world origin, pivot, child hierarchy, material groups and board dimensions. The assembled export uses **XY as board plane, Z up, authoring coordinates in millimeters**; explicitly reconcile to Unity's units/orientation, avoiding 1000× scale or 90° rotation errors.
- Data records: `Data/L109_Models_160.csv`, `L109_Components_124.csv`, `L109_Routes_164_PLANNING.csv`, `L109_Vias_54_PLANNING.csv`, `L109_Faults_3.csv`, `L109_W165_Safety.json`.
- Gameplay targets: **124 individual placements**, **164 approved routes**, **54 approved vias**, **3 recoverable faults**, **1 removable W165 feedback jumper**, **6 authoritative objectives**.
- Initial state: `0/124` installed; `0/164` routes; `0/54` vias; `W165 DISCONNECTED`; `0/6` objectives; power off; no endurance certificate.
- Source component targets: `L109_U1_CPU_SOC`, `L109_U2_DDR4_A`, `L109_U3_DDR4_B`, `L109_U18_ThermalHealthMonitor`, safety supervisor `L109_U19_SafetySupervisor`, Phase-3 MOSFET `L109_Q05_VRM_MOSFET`, removable CPU heatsink `L109_CPU_RemovableHeatsink`. Verify all exact identifiers directly in the manifest before binding.
- Preserve actual mechanical CPU cooling and VRM spreader parts as selectable/removable 3D components; *all diagnostic instruments must be virtual World-Space Canvas UI*, not standalone room furniture or physical meters.
- The source models are procedural and **not finished AAA art**. Upgrade UV/material/geometry fidelity pragmatically and record unfinished requirements; do not claim hand-finished photorealism.

Place imported assets under `Assets/ProjectSpark/Level109/`, preserving relative folders. Put scripts using `UnityEditor` inside `Editor/` or editor-only asmdefs. Produce separate production scene/prefabs and an isolated optional assembled showcase.

## 2. BOARD ZONES AND VISUAL HIERARCHY

**Zone A — CPU Thermal Core:** central CPU/SoC, accessible die/package, removable realistic finned CPU heatsink, interface layer, retention brackets, junction and heatsink monitoring. **Zone B — Four-Phase VRM Cooling:** four independently modeled phases, MOSFETs, exposed copper inductors, sense elements, removable aluminum VRM spreader, phase temperatures/losses. **Zone C — DDR4 Thermal Reliability:** U2/U3 DDR4 packages, contacts/BGA, thermal sensors and memory lane test targets. **Zone D — Thermal Supervision and Safety:** controller, monitoring, cooling/fan header, W165 trusted health bridge, power, clocks, PCIe and diagnostic pads.

Create an `L109_TRX17_GameplayRoot` scene hierarchy with `BoardBase`, `InstalledComponents`, `PlacementFootprints`, `RemovableCooling`, `ApprovedElectricalTopology`, `RouteTasks`, `ViaTasks`, `ThermalNodes`, `ElectricalHeatCoupling`, `Cooling`, `Workloads`, `TemperatureCycling`, `Faults`, `SafetySupervisor`, `VirtualTools`, `EvidenceAndReports`, `Objectives`, `WorldSpaceUI`, `VFX`, `Audio` and `Debug`. Keep `L109_TRX17_ASSEMBLED_SHOWCASE_ART_ONLY` disabled by default outside runtime authority. No visual copper plane, texture, collider or hologram creates a real graph edge.

Create realistic UV-capable URP/Lit material definitions for graphite soldermask, FR-4, metallic copper, gold pads, plated solder, black epoxy, ceramic passives, thermal interface material and brushed aluminum. Build correct normals, contact surfaces, readable silkscreen, layer viewer, world-space click/touch hit targets, and inspectable details with selectable hover/selected/target/warning outlines consistent with Project Spark selection authority. Visual overlays are diagnostic modes, not persistent painted temperature colors.

## 3. BOARD ASSEMBLY, COPPER AND VIA AUTHORITIES

124 component placement slots require stable ID, source model, local transform, package/footprint match, polarity/pin 1/orientation check, mechanical clearance and solder/contact validity. Count only committed installation actions; buying a component or displaying the completed reference does not count. Use existing shelf/placement, move, rotate, selection and recycle APIs. Respect board power-off and cooling disassembly interlocks.

For each of W001–W164 require **approved** source/destination pad IDs, net ID, allowed copper layer, width/clearance expectations and continuity result. For each V001–V054 require approved net, start/end layers, via position/type/drill and thermal/electrical role. Reject unknown and cross-net shorts. Preserve plan grouping: W001–W022 power/ground; W023–W046 CPU VRM/phase current; W047–W070 CPU/DDR4/high-speed; W071–W094 sensors/cooling; W095–W116 firmware/PCIe/storage; W117–W140 reliability/testpoints; W141–W164 safety/protection. W165 is distinct and must not be counted among those 164 routes.

Run **hard schematic reconciliation** before electrical completion: verify a consistent physical pad-to-net mapping, legal layer transitions, no hidden factory shortcuts, real graph connectivity and nonzero grounding/reference policy. Unknowns stay `DESIGN DATA REQUIRED` and `NOT_CERTIFIABLE`. Show valid UI/scene previews without allowing a fake 164/164 or 54/54.

## 4. DETERMINISTIC ELECTRO-THERMAL ENGINE

Create coupled yet separable electrical power-loss and thermal-state systems. At minimum provide temperature nodes for CPU junction/package/heatsink, each VRM MOSFET/inductor/spreader, U2/U3 memory, PCB regions and ambient. **CPU junction ≠ heatsink ≠ VRM junction ≠ ambient**. Implement a numerically stable explicit or implicit thermal RC network with calibrated fictional capacitances and conductances, e.g. `C_i dT_i/dt = P_i + Σ_j G_ij(T_j−T_i) + G_ia(T_ambient−T_i)`. Guard against invalid resistance/capacity, negative physical power, nan/inf, unstable time steps and runaway simulation. Thermal connections include CPU package→TIM→heatsink→virtual airflow→ambient and MOSFET→pad/spreader/PCB→ambient.

Power sources depend causally on the established electrical model: CPU idle/nominal/peak/burst load, V×I and/or explicit modelled CPU dissipation; VRM conduction losses `I_rms² R_ds(on,T)` plus simplified switching losses; inductor copper loss `I_rms² R_dc(T)`; DDR operation; auxiliary electronics. Keep educational parameters editable in ScriptableObjects. Only the electrical solver owns actual voltage/current; thermal model reads its snapshots and, where supported, returns temperature-dependent resistance/derating parameters in a controlled next-step coupling. Never fake current/voltage graph data from temperature VFX.

Use a deterministic fixed simulation step with bounded accelerated time (substepping across fault thresholds); interpolate visualization only. Model fan-control PWM as **virtual airflow influence**, no off-board physical fan object. Response curves, sensor lag/freshness/precision, cooling installation quality, thermal throttling and overtemperature must affect authentic simulation state. Thresholds, durations, heat-source powers, ambient sweeps and lifetime criteria are **fictional educational limits**, not approved real-world hardware ratings.

## 5. COOLING ASSEMBLY AND CALIBRATION

Provide inspect/remove/install operations on CPU heatsink and VRM spreader using existing 3D selection/manipulation; contact alignment and torque/retention are virtual bounded parameters. TIM quality changes thermal resistance; mounting may require safe power-off. During calibration compare idle sensor readings, ambient baseline, fan PWM demand, response delay, CPU junction↔heatsink delta and VRM per-phase temperatures. Sensor failure or stale data is an explicit unsafe state. Calibrate only from fresh simulation snapshots, no UI-flag bypass.

## 6. WORKLOADS, THERMAL CYCLING AND RELIABILITY

Implement repeatable `Idle`, `Nominal`, `Peak`, `Burst`, `Recovery` workload profiles with power rails, current, frequency/derating, thermal and memory results. A sustained run must record per-node maximum temperature, voltage deviations, fan/cooling demand, throttle transitions, sensor freshness, VRM balance, memory errors and PCIe stability. Controlled simulated ambient sweeping changes thermal boundary conditions. A pass certifies **only the fictional scenario test envelope**, not unlimited lifetime.

Implement repeatable temperature-cycling tests with bounded thermal expansion/contact models: cold↔warm U3 contact resistance opens/recovers at configured thresholds (possible hysteresis); accumulated cycle stress may be modelled but no uncontrolled randomness. Accelerated testing must not skip sensor stale intervals, fault onset, transient trips or state transitions. Give player a virtual BGA X-ray + memory trainer + test report.

## 7. THREE INDEPENDENT REAL FAULTS — NO SPOILERS

**F1 — Phase 3 excess MOSFET power loss.** Source fault row targets `L109_Q05_VRM_MOSFET`: elevated Rds(on) or switching loss -> disproportionately hot Phase 3; other phases need not overheat. Required evidence: temperature, phase current, loss estimation, same-load comparative telemetry. Repair: power-off, choose correct MOSFET, replace qualified part, validate electrical contacts and re-run balanced-load test. Increasing fan speed or selecting another part must NOT mark it repaired.

**F2 — CPU thermal-interface/retention defect.** Fault target `L109_CPU_RemovableHeatsink`: poor contact makes CPU junction rise rapidly while heatsink thermal response lags. Evidence: multi-load junction/heatsink temperature delta, contact inspection and cooling response. Repair: stop workload, remove assembly, correct TIM and retention, remount, check contact quality and re-run original load qualification. Merely lowering ambient or replacing a sensor does not repair TIM.

**F3 — U3 DDR4 BGA temperature-sensitive contact.** Fault target `L109_U3_DDR4_B`: bounded, deterministic temperature-dependent contact impedance and correlated lane errors under cycling. Evidence: cold/warm/repeated memory tests plus supported virtual X-ray and net/probe measurements where a verified schematic exists. Repair: controlled BGA rework or U3 replacement; test both cold and warm and full repeat cycles. Do not invent a specific manufacturer ball/pin or lane if the mapping is not approved.

Reveal locations only after sufficient diagnosis, not immediately via red lights. Keep fault state, evidence state, repair authority and qualification history separate. Faults must be individually reproducible and independently repairable, with no generic `Repair All` bypass.

## 8. W165 INDEPENDENT TRUSTED THERMAL SAFETY

Logical path: `L109_U18_ThermalHealthMonitor.TRUSTED_THERMAL_HEALTH_OUT → W165 removable jumper → L109_U19_SafetySupervisor.TRUSTED_THERMAL_HEALTH_IN`. Exact physical pins remain **UNASSIGNED** in supplied safety JSON: require schematic review before electrical certification. Confirm exact manifest naming because another board art controller label may use a different ID.

W165 carries fresh, changing or protocol-validated thermal-health feedback; it is **NOT** main CPU power, fan power or the raw temperature. Initial gameplay starts disconnected for commissioning. After normal installation and calibration: (1) establish protected-load authorization from recent healthy thermal readings and cooling status, (2) under controlled supervised test disconnect W165 physical link, (3) independent supervisor detects loss/staleness and latches inhibition of protected peak workloads, (4) preserve safe low-power diagnostic access where permitted, (5) reconnect actual terminal continuity, (6) acquire new sensor/cooling/calibration/qualification evidence, (7) explicitly reset latched fault, (8) verify protected authorization and final Objective 6. Reconnection alone never clears inhibition. A permanently HIGH/stale signal is rejected. UI labels and completed art must not override the supervisor.

## 9. SIX AUTHORITATIVE OBJECTIVES

1. **Assemble Thermal Endurance Motherboard:** 124/124 properly placed components and relevant cooling assembly passes footprint/contact/mounting checks.
2. **Build Ten-Layer Power and Thermal Network:** 164/164 approved routes + 54/54 certified vias and W165 commissioning continuity, strictly blocked on unapproved schematic.
3. **Commission Cooling and Thermal Control:** valid cold idle baseline, fresh sensor calibration and functioning airflow/PWM response; proper overtemperature safeguards.
4. **Sustained Load and Thermal Stability:** repeated idle-to-peak test, VRM phase balance, environment sweep, memory/PCIe stable, full telemetry within approved fictional test conditions.
5. **Recover Three Reliability Failures:** F1/F2/F3 independently diagnosed, correctly repaired, and requalified under original load/cycle conditions; fresh endurance evidence required.
6. **Restore Critical W165 Safety:** supervised disconnect actually inhibits peak load; physical reconnect, fresh sensors/cooling/requalification, explicit reset, genuine `LevelGamePlayManager` 6/6.

**Only real `LevelGamePlayManager` authorizes objective completion.** Use adapters after inspecting actual APIs. Never set 6/6 directly from a button, completion screen, mesh presence or mocked test result. Invalidate stale dependent certificates after repair, topology, cooling, calibration or safety revision changes; preserve appropriate historical objective achievement without trusting prior telemetry for final certification.

## 10. AAA VIRTUAL LAB INTERACTION AND UI

Use the Project Spark established high-fidelity scientific World-Space Canvas look; virtual instrument panels are equipment-free UI: multimeter, oscilloscope, phase-current probe, thermal scanner/thermal camera, cooling PWM controller, CPU stress/load tester, environmental/thermal-cycle controller, reliability logger, memory trainer, BGA X-ray scanner, 10-layer viewer, net tracer, solder/desolder, and component/heatsink service tools. The player selects a virtual tool and clicks/touches specific 3D targets, with mobile/PC support and no UI-canvas/raycast interference.

Top HUD: level name, 0/124, 0/164, 0/54, W165 and 0/6; side diagnostics: CPU junction, heatsink, VRM phase temps, U3 memory, ambient, power, cooling PWM, time acceleration; lower panel: workloads, stress traces, report/evidence/notebook. Display measured/simulated units, timestamps, freshness, pass/fail and uncertain/missing-design states; show `DESIGN DATA REQUIRED` where necessary. Thermal-camera overlay uses sampled fields, optional visual shader, not its own invented heat values. No hardwired green indicators or success animations before authoritative tests.

## 11. EVIDENCE, PERSISTENCE AND REPORTS

Maintain time-stamped immutable simulation snapshots with sample tick, topology version, cooling calibration version, fault revision, ambient condition, workload phase, sensor freshness, current/voltage, temperature, event tags and test verdict. Record per-fault hypothesis states `NotTested → Suspected → Supported → Confirmed → Repaired → Requalified`, while supporting contrary evidence and re-testing. Save placements, approved nets/vias, cooling configuration, repairs, faults, W165 latch state, calibration, workload progress, objective history and test epochs using the existing save system. On load reconcile asset/netlist schema versions and reject stale certification. Generate an exportable in-game fictional reliability report, covering conditions, graphs or telemetry, repairs and limitations.

## 12. CODE STRUCTURE — PROPOSED ONLY

Implement modular, testable components, subject to real codebase discovery:

- `SparkLevel109Coordinator` — mission lifecycle/injection and events
- `SparkLevel109BoardManifest` / `SparkLevel109PlacementAuthority` — stable part IDs and footprint contacts
- `SparkLevel109NetlistValidator` — schematic gates for routing/vias/W165
- `SparkPCBThermalNetwork` — fixed-step RC nodes and heat transfer
- `SparkPCBHeatSourceModel` / `SparkPCBCPUThermalModel` — power-to-temperature coupling
- `SparkPCBVRMThermalModel` — phase currents, losses and temperatures
- `SparkPCBHeatsinkAssembly` / `SparkPCBCoolingController` — TIM/retention/PWM
- `SparkPCBEnduranceWorkload` / `SparkPCBThermalCyclingSystem` — deterministic tests
- `SparkPCBReliabilityRecorder` — evidence and qualification
- `SparkLevel109FaultController` — F1–F3 and actual repair authority
- `SparkLevel109W165Recovery` — independent supervisor and latch
- `SparkLevel109ObjectiveTracker` — adapter to real LevelGamePlayManager
- `SparkLevel109VirtualToolBinder` — instrument display/input based on snapshots
- `SparkLevel109Builder` (Editor only) — one-click scene/prefab creation

No cycles of singleton dependencies, no `UnityEditor` runtime references, no constant Update polling where events or sampling loops suffice. Separate edit-time asset assembly from runtime saved state.

## 13. EDITOR BUILDER AND NONREGRESSION

**Required menu:** `Tools/Project Spark/Levels/Create Level 109`. Check exact import file paths, 160 model records, 124 component records, 164 route IDs, 54 vias, 3 faults, unique stable IDs, transform/units, materials, missing colliders, level registration and shader availability. Build the **EMPTY playable state** and separate off-by-default assembled reference prefab. Be Undo-friendly and idempotent when run twice, never duplicate root, never overwrite unrelated assets, levels or project settings. If approval data missing, generate a `SCHEMATIC_REQUIRED` art-and-simulation preview with Objective 2/6 certification blocked; provide report of missing fields rather than inventing a netlist. Test actual Unity import and resolve console errors before claiming complete.

## 14. ENGINEERING ACCEPTANCE TEST MATRIX — 100 REQUIRED CHECKS

Implement as actual EditMode/PlayMode tests or explicitly report those that require manual input or a missing schematic. Every item is an acceptance requirement, not a preexisting pass.

### Asset/scene/import (T001–T010)
- [ ] T001 — modular 160 FBX assets resolvable; file count reconciled to manifest.
- [ ] T002 — optional 160 GLB preview assets handled without duplicate runtime components.
- [ ] T003 — assembled FBX remains art-only, never authoritative.
- [ ] T004 — 124 unique installable component IDs parse correctly.
- [ ] T005 — 36 support model records do not count as installed parts.
- [ ] T006 — 300×220×1.6 mm board geometry validated in Unity units.
- [ ] T007 — XY/Z-up source orientation reconciled with Unity and pivots.
- [ ] T008 — URP materials, normals, UVs and outlines render without console errors.
- [ ] T009 — separate real interactive CPU heatsink, VRM spreader, U3 and W165 targets.
- [ ] T010 — Levels 1–108 and shared source assets remain unchanged.

### Placement/netlist (T011–T020)
- [ ] T011 — level begins 0/124 installed; assembled preview contributes zero.
- [ ] T012 — correct package/orientation/contact required for installation.
- [ ] T013 — wrong component/polarity/footprint rejected.
- [ ] T014 — routes begin 0/164 and remain NOT_CERTIFIABLE without approved netlist.
- [ ] T015 — vias begin 0/54 and remain NOT_CERTIFIABLE without approved spans.
- [ ] T016 — missing net/pin/layer explicitly blocks validation.
- [ ] T017 — cross-net short rejected and reported.
- [ ] T018 — simulated board-layer viewer never changes connectivity.
- [ ] T019 — jumper endpoints cannot be inferred from mere mesh adjacency.
- [ ] T020 — topology revision invalidates dependent qualification.

### Thermal numerical model (T021–T035)
- [ ] T021 — separate CPU junction/heatsink/ambient thermal nodes.
- [ ] T022 — distinct MOSFET and inductor nodes for four VRM phases.
- [ ] T023 — CPU power increase raises junction temperature with physical lag.
- [ ] T024 — increasing TIM resistance raises junction-to-heatsink delta.
- [ ] T025 — improved airflow reduces steady-state temperatures.
- [ ] T026 — heat capacity produces non-instantaneous response.
- [ ] T027 — energy/temperature calculation stays finite for valid test envelope.
- [ ] T028 — invalid RC configuration detected, not silently clamped to success.
- [ ] T029 — deterministic fixed-step outcomes repeat for same config/seed.
- [ ] T030 — accelerated time resolves transient threshold crossings.
- [ ] T031 — sensor readings reflect sampled thermal nodes.
- [ ] T032 — stale sensor reading identified and unsafe authorization blocked.
- [ ] T033 — derating reduces protected workload without claiming total power outage.
- [ ] T034 — VRM losses respond to I²R and changed Rds(on).
- [ ] T035 — CPU junction, heatsink and MOSFET temp not conflated in HUD.

### Cooling and profiles (T036–T045)
- [ ] T036 — heatsink installs only with matching assembly alignment.
- [ ] T037 — VRM spreader contact state affects thermal coupling.
- [ ] T038 — cooling controller interpolates editable PWM curve correctly.
- [ ] T039 — calibration uses fresh actual sample and records revision.
- [ ] T040 — idle→nominal→peak→recovery profiles produce distinct telemetry.
- [ ] T041 — virtual ambient change alters thermal boundary condition.
- [ ] T042 — load ramp can trigger throttling before an overtemperature trip.
- [ ] T043 — protected test is blocked with failed cooling response.
- [ ] T044 — reliability log includes max temperatures and voltage deviations.
- [ ] T045 — report distinguishes scenario qualification from lifetime proof.

### F1 Phase-3 fault (T046–T055)
- [ ] T046 — F1 modifies Phase 3 losses, not all phases equally.
- [ ] T047 — Phase 3 temperature rise correlates with supplied phase current.
- [ ] T048 — other phases remain comparatively balanced under equal load.
- [ ] T049 — defect diagnosis requires measured comparison evidence.
- [ ] T050 — Phase 3 MOSFET model matches manifest target.
- [ ] T051 — wrong MOSFET replacement does not clear fault.
- [ ] T052 — fan speed increase alone does not repair defective MOSFET.
- [ ] T053 — qualified replacement blocked when energized.
- [ ] T054 — replacement changes actual loss parameter/contact state.
- [ ] T055 — original stress test passes only after legitimate repair.

### F2 CPU thermal interface (T056–T065)
- [ ] T056 — poor interface increases modeled CPU thermal resistance.
- [ ] T057 — CPU junction heats faster than heatsink when interface poor.
- [ ] T058 — correct multi-load delta evidence required.
- [ ] T059 — stopping workload is enforced for safe heatsink service.
- [ ] T060 — retightening/retention state influences correct contact.
- [ ] T061 — wrong or absent thermal pad retains defect.
- [ ] T062 — sensor replacement alone does not repair interface.
- [ ] T063 — completed correct remount changes thermal coupling.
- [ ] T064 — post-repair load test uses same configured stress envelope.
- [ ] T065 — stale pre-repair evidence does not qualify repaired board.

### F3 DDR4 thermal contact (T066–T075)
- [ ] T066 — cold U3 memory test behaves per defect configuration.
- [ ] T067 — deterministic heat cycle crosses fault transition.
- [ ] T068 — U3 contact resistance changes as defined, without random flicker.
- [ ] T069 — lane errors correlate with contact and temperatures.
- [ ] T070 — unrelated DDR4 U2 remains electrically separate.
- [ ] T071 — X-ray UI exposes only supportable BGA evidence.
- [ ] T072 — ordinary surface solder cannot fix buried BGA.
- [ ] T073 — valid controlled U3 rework/replace corrects fault parameters.
- [ ] T074 — cold and warm memory tests re-run after repair.
- [ ] T075 — repeated cycles pass through complete original duration.

### W165 and safety (T076–T085)
- [ ] T076 — W165 starts disconnected for commissioning.
- [ ] T077 — electrical qualification blocked until exact W165 pins approved.
- [ ] T078 — removing commissioned W165 inhibits protected workloads.
- [ ] T079 — safe diagnostic functions remain available where defined.
- [ ] T080 — stale or constant HIGH feedback rejected.
- [ ] T081 — independent supervisor latches a safety trip.
- [ ] T082 — reconnecting W165 alone cannot clear the latch.
- [ ] T083 — fresh sensor/cooling health evidence required.
- [ ] T084 — explicit authorized reset required.
- [ ] T085 — final protected-load authorization uses trusted current state.

### UI/objectives/persistence/QA (T086–T100)
- [ ] T086 — mouse selection resolves correct tool hit target.
- [ ] T087 — Android touch resolves correct target without Canvas conflicts.
- [ ] T088 — thermal camera overlay reflects actual sampled temperatures.
- [ ] T089 — timeline stores timestamped thermal/electrical events.
- [ ] T090 — three diagnoses/repairs independently recorded.
- [ ] T091 — saving and loading restores deterministic progress.
- [ ] T092 — old revision certificates invalidated on load.
- [ ] T093 — objective 1 requires 124 real installations.
- [ ] T094 — objective 2 requires 164/54 verified electrical tasks.
- [ ] T095 — objective 3 requires cooling calibration and sensor freshness.
- [ ] T096 — objective 4 requires complete stress profile qualification.
- [ ] T097 — objective 5 requires F1–F3 repair + requalification.
- [ ] T098 — objective 6 requires complete W165 recovery and genuine 6/6.
- [ ] T099 — one-click builder is idempotent; all new C# compiles in Unity 6.3.
- [ ] T100 — PlayMode evidence, validation logs and unresolved blocking inputs supplied.

## 15. REQUIRED DELIVERY FORMAT AND DEFINITION OF DONE

Provide (a) runtime C# source/prefabs/ScriptableObjects, (b) separate empty and showcase board configurations, (c) original component IDs and transform reconciliation, (d) validated electrical-data acceptance report, (e) deterministic thermal CPU/VRM/DDR4 simulation, (f) F1/F2/F3 with repair effects, (g) W165 independence and safe reset, (h) World-Space Canvas virtual tools and report, (i) integration with actual level progression and saves, (j) one-click editor builder, (k) tests and project-specific import steps, (l) exact Unity console failures/remaining blockers.

**Completion is NOT “the board looks finished.”** It is achieved only when the game starts empty, the player installs and verifies required components and legitimate network connections, calibrates cooling, reproduces and repairs the faults, passes a deterministic endurance profile, demonstrates fail-safe W165 feedback interruption and restores protected operation with fresh evidence, and the real LevelGamePlayManager certifies 6/6. Where an approved pin-level schematic or source project API is missing, complete safe scaffolding and report the blocker explicitly rather than fake a passing test.

**PROJECT SPARK — LEVEL 109: THERMAL ENDURANCE**  
**COOL. STRESS. ANALYZE. REPAIR. ENDURE.**
