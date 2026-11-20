# PROJECT SPARK — LEVEL 103: THE POWER CORE
## MASTER UNITY IMPLEMENTATION PROMPT — SOURCE-LOCKED ENGINEERING SPECIFICATION

**Target:** Unity project, Level 103 CPX-11 eight-layer CPU power board. **Deliverable goal:** a fully playable, measured, repairable four-phase VRM level with six authoritative objectives, while preserving Levels 1–102. **Status:** implementation instructions only, not a claim of completed functional code or Unity Play Mode validation.

> **PRIMARY DIRECTIVE FOR IMPLEMENTING AI / ENGINEER**  
> Inspect the actual Project Spark codebase before implementing. Use the supplied Level 103 ZIP as immutable source-art and planning-data input. Preserve existing game architecture, UI design, level numbers, shop, shelf, actual electrical solver, wire interactions and `LevelGamePlayManager`. Implement real functionality rather than mock completion flags. Do not invent class methods or infer exact schematic pins from the placeholder CSVs. If required APIs are missing, create minimal adapters after inspecting their real signatures, and report blocked certification honestly.

## 01 — Verified package baseline

Source archive: `ProjectSpark_Level103_PowerCore_BoardOnly_Realistic_Complete.zip`. Source root: `Level103_CPX11/`.

| Item | Source value |
|---|---:|
| Modular FBX and GLB | 112 each |
| Assembled reference FBX and GLB | 1 each |
| Installable component references | 88 |
| Separate board/socket/layer/repair/reference art assets | 24 |
| Physical PCB | 240 × 180 mm |
| Copper layers | 8 |
| Planned routes | 116 |
| Planned vias | 30 |
| Planned fault definitions | 3 |
| Planned objectives | 6 |
| Modular triangle total (source summary) | 27,900 |

**File semantics:** GLB coordinates are in metres, whereas FBX geometry uses centimetres with `UnitScaleFactor 1`. The project is Y-up; PCB spans XZ. Verify imported board bounding box measures **0.240 × 0.180 metres** in Unity after model scale and transform. Never silently multiply world-space placement coordinates from the CSV twice. Component geometry is local to its FBX; use `ComponentPlacements.csv` for intended physical poses.

**Hard limit:** Routes and via records contain `UNASSIGNED`, `REQUIRES_PIN_SCHEMATIC`, and `NOT_CERTIFIABLE`. No electrically functional or objective-certified level can be claimed from the asset ZIP alone. A schematic and verified netlist are prerequisites.

## 02 — Non-negotiable production boundaries

1. **Board only**: create no new room, chairs, game table, standalone oscilloscopes, solder irons, or physical diagnostic instruments. Reuse the existing room and **AAA Virtual PCB Repair UI** (world-space Canvas, click/touch VFX).
2. **Art/gameplay separation**: the assembled showcase is reference-only and must never satisfy placement, routing, power, repair or objective requirements. Build a clean empty gameplay board with all 88 parts uninstalled, 116 routes unfinished, 30 vias absent, W117 disconnected.
3. **Topology authority**: real solder/pad/via/trace connectivity must live in one graph used by the electrical solver, repair system, measurement tools and objective evaluator; visual-only nets are not authoritative.
4. **Protected simulation**: model 12 V supply, nominal 1.00 V VCORE, 2–20 A CPU load, and four 400 kHz interleaved power phases. No hardware control of real power supplies is required or authorized.
5. **Strict compatibility**: preserve Levels 1–102, their asset GUIDs, prefabs, scene settings, menu items and scripts. Namespaced or versioned new systems must avoid project-wide replacement.
6. **Evidence and freshness**: cached readings, fake status LEDs, teleported wires, premade showcase geometry and UI VFX cannot award objective completion.
7. **No invented inputs**: external pin numbers, precise copper geometry, thermal coefficients, phase device datasheets, CPU power specification, per-phase inductor/capacitance values and detailed control-loop coefficients are currently unspecified. Store these as configurable engineering defaults with validation status, never describe them as source-certified data.

## 03 — Mandatory reconnaissance in user's Unity repository

Before coding, locate and inspect the real definitions for `LevelGamePlayManager`, `SparkTerminal`, electrical graph/solver types, wire creation and disconnect APIs, component purchase/placement, repair authority, world-space `PCBVirtualStudio`, level generator menu, objective save/load and render pipeline. Build a dependency map with actual file paths, namespaces, public methods/events and initialization order. Create one integration contract documenting which actor is authoritative for each signal. Do not call proposed interfaces as if they already exist.

Generate a compilation-safe, idempotent level builder; re-running it must not duplicate levels, terminals, materials, systems or scene instances. Disallow implicit creation of working terminal objects for unresolved pin-level data. Keep blocked features visible in diagnostics rather than silently accepting them.

## 04 — Target system and scene hierarchy

```text
Level103_CPX11
  ArtRoot
    EmptyPCBVisual
    FactoryOnlyArt
    EightLayerInspectionArt (visual only)
    CPU_Socket (board-only, not counted again in 88 BOM)
    InstalledComponentVisuals (empty at start)
    RouteVisuals (empty at start)
    ViaVisuals (empty at start)
    AssembledShowcaseReference (disabled and detached from gameplay)
  ElectricalRoot
    VerifiedPadsAndPins (blocked until schematic approval)
    FactoryConductiveGraph
    PlayerCopperGraph
    ViaGraph
    W117FunctionalJumper
  SimulationRoot
    Protected12VInput
    AuxiliarySupplies
    FourPhaseVRM
    CPUVirtualLoad
    PowerAndThermalPlant
    IndependentSafetyMonitor
  GameplayRoot
    PlacementController
    RouteAndViaController
    FaultInjectionAndRepairAuthority
    ObjectiveEvidenceTracker
    LevelGamePlayManagerAdapter
  UIAdapters
    ExistingWorldSpaceInspectionAndRepair
    WaveformAndCurrentDisplays
    ThermalPhaseOverlay
    LayerViewer
  Diagnostics
    StartupAndNetlistAudit
    TestReportExporter
```

Board-only art root is attached to an existing Project Spark table/level mount; preserve measured PCB scale in physics. If the game requires presentation enlargement, implement a visual proxy scale independent of all electrical dimensions.

## 05 — Verified schematic / netlist authoring gate

Build a machine-readable approved schematic schema: `component_ref`, `package`, `datasheet_or_educational_model`, `pin_number`, `pin_name`, `pad_id`, `net_id`, `signal_domain`, `rated_voltage`, `function`, `source_of_truth`. Each board trace record must resolve to valid source/destination pads on the same `net_id`, with copper layer, geometry, width, clearance, current rating and player-required status. Each via must resolve to `net_id`, `(x,z)` location, barrel span, drill, annulus, layer connectivity and manufacturing type. W117 is a separate removable logical signal on its own source/sink port definitions.

Validation gates before setting `NetlistApproved=true`: component/BOM cross-reference, all 88 unique part refs, all required power grounds, all MOSFET gate/control nodes, 4 phase switch nodes, VCORE bus continuity, no false direct 12V-to-VCORE short, legitimate CPU socket power pins, safety inputs with sensible pull behavior, all 116 routes mapped, all 30 vias mapped, W117 distinct from the actual high-current VCORE rail, no duplicate unintended route IDs, correct branch membership, no electrical bypass through factory traces. Reject unresolved or conflicting assignments with diagnostics and block Objectives 2–6; do not automatically generate placeholder terminal names as live terminals.

## 06 — Component placement and soldering authority

Load `ComponentBOM.csv` and `ComponentPlacements.csv` exactly. Every component starts uninstalled in gameplay regardless of assembled showcase. Each item has `RequiredPartRef`, footprint, material and package, acceptable orientation, component value, polarity where relevant, socket compatibility, solder-pad list and independent validation state.

Interaction sequence: select item via existing shelf/shop; hover over footprint; snap and rotate; check exact ref, package, orientation; place; use virtual solder tool by click/touch with target-specific VFX; apply contact state to authoritative PCB graph only after validated solder operation. Wrong part, wrong value, rotated IC, reversed diode and bridged pins must produce honest electrical consequences. W117 is included in 88 BOM and must not be counted as an extra 89th placement.

For CPU load/socket, source manifest includes its separate socket geometry; verify whether the socket is an art-only part or has its own approved functional pad set before wiring it. Never mark 88/88 solely from mesh instances or a single prefab flag.

## 07 — Eight-layer routing, power planes and 30 via tasks

Layer stack proposal from Level 103 design (pending manufacture approval): L1 components/local routing; L2 ground plane; L3 12V distribution; L4 VRM control/phase-sensing; L5 VCORE plane; L6 ground plane; L7 monitors/control; L8 bottom routing. This is a gameplay design proposal, not geometry proven by the ZIP.

The route tool must snap to approved pads and existing copper nodes; check electrical net identity, clearance, layer, width/copper current constraints and DRC. A visual glowing line does not join nets without solver acceptance. Via tool must implement the selected layer span, validate net, forbid illegal shorting of power planes, and create conductive edges only after approved placement/repair.

Source route group counts: Input/Protection 16; VRM PH1/2 24; VRM PH3/4 24; VCORE 18; Sensing/Thermal 18; Safety 16. All route endpoints and via nets remain unresolved in the source and must be populated from an approved schematic first. No arbitrary automatic connecting of 'Terminal 1 to Terminal 2'.

## 08 — Four-phase VRM — authoritative educational physics

Implement a coherent deterministic time-stepped simulation, not independently animated voltages. Inputs: protected `Vin=12V`, phase commands with nominal frequency 400 kHz and 90° offsets, phase inductors with series resistance, high/low MOSFET conduction and switching losses, common output capacitance/ESR, load profile 2–20 A, feedback measurements, thermal coupling and protection.

For each phase `i`, represent state `(iL_i, switchState_i, driverValid_i, currentSense_i, temperature_i)`; common state `(VCORE, outputCapCharge, CPU_Load, PowerGood)`. Approximate inductor dynamics by `di/dt = (Vswitch_i - VCORE - R_L*iL_i)/L_i` and capacitor dynamics by `dVCORE/dt = (sum_i iL_i - I_CPU)/C_out`, accounting for ESR and controller restrictions. Choose stable integration substeps or a suitably averaged model; do not run 400 kHz switching edge simulation directly at Unity frame rate. Preserve energy and realistic finite transition time.

Phase offsets must be 0°, 90°, 180°, 270° when all four phases healthy. Their outputs are paralleled only after inductors, with true shared VCORE and common ground. Current-balance correction must depend on sensors; wrong sensor gain may lead to misleading control behavior. The CPU virtual load changes with a specified workload test profile. Configure realistic but explicitly fictional sensor/noise, thermal and protection parameters for repeatable gameplay; store separately from source manifests.

## 09 — VCORE control, load line and ripple

Nominal VCORE setpoint is 1.00V; configure an educational droop slope (for example, 2.5mΩ) as a tunable gameplay parameter, not a sourced CPU requirement. Compute actual VCORE using VRM dynamics and current; compare against **load-dependent permitted envelope**, not rigid exactly 1.000V at all loads. At 20A, a 2.5mΩ intentional load-line corresponds to 50mV steady-state decrease. Treat overshoot, ripple and recovery-time as separate measured properties.

Capture and display actual per-phase currents, sum/current-balance error, output voltage min/max, peak-to-peak ripple, efficiency, MOSFET junction and inductor temperatures, thermal guard state, CPU reset/power-good. Phase current sum should physically track output capacitor charging and CPU demand. A 'good' UI icon with bad VCORE must fail certification.

## 10 — Commissioning and sequencing

Design explicit states `UNASSEMBLED`, `ROUTING_INCOMPLETE`, `DRC_READY`, `INPUT_PROTECTED`, `AUX_RAILS_OK`, `VRM_STARTING`, `VCORE_SETTLING`, `VCORE_QUALIFIED`, `CPU_RELEASED`, `FAULT_LATCHED`, `RECOVERY_PENDING`, `REQUALIFYING`. Gate each transition on fresh graph and measurement evidence.

For power-on: validate no short before energizing; check 12V input and auxiliary supplies; verify drivers and sensing; enable phase PWM in managed order; regulate VCORE and wait for stable limits; assert trusted VCORE-good if genuinely qualified; only then release virtual CPU reset. Loss of safety feedback must inhibit CPU authorization even if actual VCORE remains momentarily charged.

## 11 — Objective 1: PCB assembly

Goal: 88/88 exactly matched installed component references with verified footprint, value, orientation, solder. The socket/load mesh in board-detail assets is not counted as an extra installable part unless explicitly linked to a BOM reference. Reject auto-install from showcase visual art. Record objective event with full component state and time; if faults are later injected, preserve historical completion without falsely ignoring new repair conditions.

## 12 — Objective 2: electrical construction

Goal: all 116 approved player copper routes completed and 30 assigned via tasks installed, with DRC, net continuity and power-safety checks. Requires `NetlistApproved=true`. Visual connection count alone cannot pass. The separate W117 jumper should be installed for normal commissioning after validating its two real ports and must not increment the 116 copper route counter.

## 13 — Objective 3: four-phase VRM commissioning

Goal: protected 12V input, valid auxiliary rail conditions, four operating phase drivers and current sensors, near-90-degree interleaving, shared nominal VCORE regulation, low-load startup, stable rail power-good and independent safety authorization. Demonstrate measured current contributions from PH1–PH4 and valid feedback. Require a fresh commission test run with machine-readable evidence and an actual verified electrical graph.

## 14 — Objective 4: CPU load and thermal stability

Run reproducible named test profiles `IDLE_2A`, `NOMINAL_10A`, `PEAK_20A` and `TRANSIENT_2_TO_20_TO_2_A`, with documented dwell intervals, sample rate, pass/fail thresholds and reset conditions. Evaluate real VCORE envelope under load line, voltage ripple, settling, phase sharing, total power, thermal rise, and protection status; record per-run trace data. Failing one profile means no certification, even if the CPU status indicator looks green.

## 15 — Objective 5: three recoverable faults

- **F1 — Phase 2 current-sense drift:** alter actual Phase 2 sensor gain/offset in simulation, causing sensed/actual phase current discrepancy and possible unbalance. Correct with targeted component/sense repair and fresh calibration; verify Phase 2 measurement matches true simulated current.
- **F2 — Phase 3 inductor-output open via:** remove the specified via conductive edge on approved `VCORE` phase-output path; Phase 3 contribution fails or is reduced, other phases carry extra load; tool diagnosis identifies location. Repair via/approved rework and rerun continuity, current balance and VCORE tests.
- **F3 — Phase 4 MOSFET thermal loss:** increase modeled Phase 4 device losses (on-resistance or switching loss); current/heat and thermal protection respond. Disable output safely, use virtual inspection/replacement and re-test thermal stability.

Each fault must be deterministic, individually identifiable, repairable only through relevant authority and representable in electrical measurements. Repaired VFX alone is not proof. Three fault repairs require a **fresh complete** post-repair power qualification before Objective 5 passes. Do not inject all faults invisibly without a discovery mechanic.

## 16 — Objective 6: W117 supervised critical feedback

From ZIP: `L103_VcoreSupervisor_U10.TRUSTED_VCORE_GOOD_OUT` -> **W117** -> `L103_CPUSafetySupervisor_U11.TRUSTED_VCORE_GOOD_IN`. Status: **design stage**, exact package pin assignments missing. The jumper is a **logical VCORE-good wire, not the high-current CPU supply**. It begins disconnected, must be connected and validated for initial commissioning, then disconnected after Objectives 1–5 during a supervised test.

On disconnection: authoritative graph contact opens; the independent safety supervisor detects missing trusted feedback, inhibits processor authorization, latches a diagnostic fault, and triggers a controlled VRM/CPU safe transition according to the engineering plant. The output capacitors and VCORE cannot teleport to zero; independent diagnostic power and actual state remain coherent. On reconnect: require correct terminal restoration, new VCORE and per-phase current evidence, explicit user-commanded safety reset, then a *new* complete four-phase + CPU workload and thermal qualification. Objective 6 must not pass through cached measurements, an old flag or decorative reconnect animation. After passing, authoritative `LevelGamePlayManager` reports 6/6.

## 17 — Virtual repair UI: world-space Canvas, no FBX tools

Integrate the existing Project Spark Virtual PCB Repair UI, not a duplicate tool deck. Virtual tools: voltage/current probe, oscilloscope, four-phase VRM analyzer, power analyzer, thermal scan, net tracer, eight-layer viewer, controlled load tester, continuity check, solder, desolder, component replacement, via repair and calibration.

Interaction: select virtual tool -> click/touch a valid 3D PCB collider pad/trace/via/component -> resolve authoritative target ID -> validate permissions, power-safe state, fault and tool compatibility -> show localized contact VFX at raycast point -> request atomic graph/component repair -> update electrical plant and measurement cache -> reevaluate objectives. Wrong tool/target should generate a clear UI rejection, not a hidden success. Electrical repairs that touch a powered high-current rail should require safe isolation before virtual repair.

VFX: local solder glow, subtle heating, short sparks, diagnostic scan pulses and thermal gradients. Particle effects are presentations only and never change simulator states by themselves. On mobile, prevent touches on world-space tool UI from also striking the PCB underneath.

## 18 — PBR, FBX scale and reference-assembly handling

Use `ModelManifest.csv` file paths and placement values as source. Check Y-up/XZ board orientation; import FBX centimetre geometry with an explicit once-only conversion and compare final world bounding box to 0.240 × 0.180 m. Do not apply 100x compensation twice. GLB in metres may be used as an independent sanity reference.

Keep material lookup relative to the imported kit's actual root (not hardcoded nested `Assets/ProjectSpark` path). Starter material generator prefers URP Lit, falls back to Standard. The included installer currently applies only BaseColor/MainTex and requires final metallic, smoothness, normal, occlusion, transparency and emission handling. Support active project render pipeline with explicit errors instead of silent incompatible materials. Don't present starter maps as artist-authored final AAA production textures.

Ensure real interactive object colliders, separate renderers and stable IDs; high-density mesh art should not create hundreds of expensive unnecessary colliders. Layer art and showcase should not be raycastable for gameplay or conductive by default.

## 19 — Performance and platform acceptance

Target smooth Play Mode on the user's actual Unity platform: use pooled procedural VFX, batch decorative art where appropriate, avoid per-frame allocations in waveform UI, cache spatial lookups, and separate simulation fixed update from render/UI refresh. Provide deterministic replays with controlled simulation timestep and random seeds. Support desktop mouse and mobile touch. Persist only validated component, net, fault and objective states; on load recompute solver authority before reporting green states.

## 20 — One-click level builder, menus and isolation

**Existing source menus (must retain as-is):**

- `Tools/Project Spark/Levels/103/1. Build EMPTY CPX-11 Board`
- `Tools/Project Spark/Levels/103/2. Create Starter Materials`

**New desired convenience menu**: `Tools/Project Spark/Levels/Create Level 103`. This is a proposed integration feature, not present in the source scripts. It should orchestrate art import, root setup, warnings and verified data registration, with a dry-run report. If netlist is missing, build **art and blocked diagnostics**, not fake working power circuitry. Re-running the builder must not duplicate the level.

## 21 — Suggested implementation classes (PROPOSALS ONLY)

`SparkL103Coordinator`, `SparkL103VerifiedNetlistAsset`, `SparkL103PadAndViaResolver`, `SparkL103PlacementAdapter`, `SparkL103FourPhaseVRMPlant`, `SparkL103PhaseDynamics`, `SparkL103VcoreController`, `SparkL103CPUVirtualLoad`, `SparkL103ThermalPlant`, `SparkL103PowerSafetySupervisor`, `SparkL103W117Recovery`, `SparkL103FaultAuthority`, `SparkL103ObjectiveEvidence`, `SparkL103WorldSpaceToolAdapter`, `SparkL103Builder`, `SparkL103ValidationSuite`. Do not presume these exist. Bind to real Project Spark namespaces and component lifecycle after codebase inspection. Prefer reusable shared PCB/VRM components over unique level-only monoliths.

## 22 — Data and save formats

Generate approved assets only after schema reconciliation: `ApprovedSchematic.json`, `ApprovedPinNetlist.json`, `ApprovedRouteGeometry.csv`, `ApprovedViaSpans.csv`, `ComponentFootprintMap.json`, `VRMElectricalParameters.asset`, `FaultScenarioConfig.asset`, `ObjectiveEvidencePolicy.asset`, `W117SafetyContract.asset`. Keep originals in `SourceData` unchanged. Persist: component installation/solder state, conductive graph deltas, via states, controlled fault states, calibration version/time, sequence progress, safety latch and objective evidence references. Recompute actual voltages on load and invalidate stale readings.

## 23 — Implementer delivery checklist

Deliver Unity compilation-clean scripts and assets; exact file tree; menu usage and repeatability; prefab/scene configuration; real netlist and its provenance; material mapping report; solver and objective integration references; deterministic functional tests and their results; known limitations. Explicitly distinguish: **art imported**, **netlist approved**, **simulation functional**, **Play Mode tested**, **6/6 achieved**. Do not declare a stage complete until verified in that Unity project.

## 24 — Engineering acceptance tests (100 proposed, not yet executed)

Every test needs `ID`, setup, action, expected outcome, actual outcome, run date, Unity version and evidence location. Test failures must not be relabeled as passes. Source ZIP structural validation is not equivalent to any of these runtime tests.

| ID | Category | Required test |
|---|---|---|
| AT-001 | Art/Import | Zip contents match declared 112 modular and one showcase FBX |
| AT-002 | Art/Import | Each modular FBX resolves by manifest filename |
| AT-003 | Art/Import | Each modular GLB resolves by manifest filename |
| AT-004 | Art/Import | All 88 BOM refs are unique |
| AT-005 | Art/Import | All 88 placements join to BOM |
| AT-006 | Art/Import | Socket model exists and is not double-counted |
| AT-007 | Art/Import | Board imports to 240 × 180 mm within tolerance |
| AT-008 | Art/Import | Y-up and XZ board orientation validated |
| AT-009 | Art/Import | FBX centimeters and GLB meters agree after conversion |
| AT-010 | Art/Import | Showcase does not affect gameplay authority |
| AT-011 | Art/Import | Source material textures load relative to actual install path |
| AT-012 | Art/Import | Native materials compile in active render pipeline |
| AT-013 | Art/Import | Rerunning empty builder does not duplicate root |
| AT-014 | Art/Import | Rerunning convenience builder does not duplicate system |
| AT-015 | Art/Import | Previous Levels 1–102 remain unchanged |
| AT-016 | Schematic/Topology | All 116 route net IDs resolved and source-approved |
| AT-017 | Schematic/Topology | All 116 route endpoint pins resolved |
| AT-018 | Schematic/Topology | All 30 via net IDs and layer spans resolved |
| AT-019 | Schematic/Topology | Exact eight-layer stack-up approved |
| AT-020 | Schematic/Topology | All required ground returns pass DRC |
| AT-021 | Schematic/Topology | No accidental 12 V to VCORE short |
| AT-022 | Schematic/Topology | All four phase switch nodes distinct |
| AT-023 | Schematic/Topology | All inductor outputs correctly join common VCORE |
| AT-024 | Schematic/Topology | No factory copper bypasses missing gameplay route |
| AT-025 | Schematic/Topology | W117 valid source/sink physically identified |
| AT-026 | Schematic/Topology | W117 never carries high-current VCORE |
| AT-027 | Schematic/Topology | Netlist approval gates electrical solver |
| AT-028 | Schematic/Topology | Invalid/unapproved nets cannot certify objectives |
| AT-029 | Schematic/Topology | Wrong net routing rejected and explained |
| AT-030 | Schematic/Topology | Wrong via layer span rejected and explained |
| AT-031 | Placement/Routes | 88 component starting states are uninstalled |
| AT-032 | Placement/Routes | W117 counted once in BOM |
| AT-033 | Placement/Routes | All 116 routes start incomplete |
| AT-034 | Placement/Routes | All 30 vias start absent |
| AT-035 | Placement/Routes | W117 initially disconnected |
| AT-036 | Placement/Routes | Wrong component value rejected |
| AT-037 | Placement/Routes | Wrong IC orientation rejected |
| AT-038 | Placement/Routes | Reversed polar part rejected |
| AT-039 | Placement/Routes | Component install requires validated footprint |
| AT-040 | Placement/Routes | Solder contact must join appropriate pad |
| AT-041 | Placement/Routes | Solder bridge creates a real short or is prevented |
| AT-042 | Placement/Routes | Completed showcase cannot change count |
| AT-043 | Placement/Routes | Correctly installed 88/88 passes Objective 1 |
| AT-044 | Placement/Routes | Actual 116/116 and 30/30 passes Objective 2 |
| AT-045 | Placement/Routes | Objective 2 blocked without approved netlist |
| AT-046 | VRM/Power | Input off => no VRM output |
| AT-047 | VRM/Power | 12 V input protection acts on incorrect polarity |
| AT-048 | VRM/Power | Auxiliary supplies gate VRM start |
| AT-049 | VRM/Power | Four phases are interleaved at 90 degree intervals |
| AT-050 | VRM/Power | 400 kHz phase setting is respected by dynamics |
| AT-051 | VRM/Power | One phase disabled reduces available output |
| AT-052 | VRM/Power | Each phase current is measured independently |
| AT-053 | VRM/Power | Sum of phase currents follows capacitor/load dynamics |
| AT-054 | VRM/Power | VCORE reaches configured 1.00 V nominal at qualified state |
| AT-055 | VRM/Power | Output capacitor prevents teleporting VCORE |
| AT-056 | VRM/Power | 2 A idle load passes healthy run |
| AT-057 | VRM/Power | 10 A nominal load passes healthy run |
| AT-058 | VRM/Power | 20 A peak load passes healthy run |
| AT-059 | VRM/Power | 2-to-20 A transient creates finite response |
| AT-060 | VRM/Power | Load-line is applied to envelope not false fault |
| AT-061 | VRM/Power | Losses produce temperature rise |
| AT-062 | VRM/Power | Thermal protection inhibits unsafe operation |
| AT-063 | VRM/Power | CPU reset remains asserted until qualified |
| AT-064 | VRM/Power | Safety-power-good uses fresh measurement |
| AT-065 | VRM/Power | VCORE deviation causes true qualification failure |
| AT-066 | Faults | F1 changes measured versus actual Phase 2 current |
| AT-067 | Faults | F1 observable on virtual current probe |
| AT-068 | Faults | F1 repair requires correct target and tool |
| AT-069 | Faults | F1 recalibration restores phase balance |
| AT-070 | Faults | F2 actually opens Phase 3 conductive path |
| AT-071 | Faults | F2 affects current contribution |
| AT-072 | Faults | F2 via repair restores authoritative net |
| AT-073 | Faults | F3 modifies Phase 4 loss parameters |
| AT-074 | Faults | F3 causes appropriate thermal response |
| AT-075 | Faults | F3 qualified replacement restores behavior |
| AT-076 | Faults | All three repaired faults require fresh complete retest |
| AT-077 | Faults | Repair VFX alone never certifies fault recovery |
| AT-078 | W117/Safety | W117 connected for first commissioning |
| AT-079 | W117/Safety | W117 manual disconnect physically opens graph |
| AT-080 | W117/Safety | Independent supervisor detects lost valid signal |
| AT-081 | W117/Safety | Constant/stale feedback cannot qualify |
| AT-082 | W117/Safety | CPU operation authorization inhibited |
| AT-083 | W117/Safety | Unrelated diagnostics remain possible |
| AT-084 | W117/Safety | VCORE discharges physically, not instantly |
| AT-085 | W117/Safety | Safety fault latches until explicit reset |
| AT-086 | W117/Safety | Reconnect alone does not clear latch |
| AT-087 | W117/Safety | Fresh phase/rail evidence required after reconnect |
| AT-088 | W117/Safety | New complete CPU workload qualification required |
| AT-089 | W117/Safety | Objective 6 cannot reuse old test evidence |
| AT-090 | W117/Safety | Only game manager authority reports six objectives |
| AT-091 | UI/Persistence | Mouse click hits correct PCB target |
| AT-092 | UI/Persistence | Touch selection hits correct PCB target |
| AT-093 | UI/Persistence | UI pointer capture prevents pass-through |
| AT-094 | UI/Persistence | Invalid tool target rejects visibly |
| AT-095 | UI/Persistence | VFX plays at real contact world coordinate |
| AT-096 | UI/Persistence | Instrument displays actual simulated node measurement |
| AT-097 | UI/Persistence | Thermal UI reflects actual device model |
| AT-098 | UI/Persistence | Layer viewer does not create conductance |
| AT-099 | UI/Persistence | Save/reload preserves authoritative states |
| AT-100 | UI/Persistence | Reload invalidates stale measurement cache |

## 25 — Source conflicts / unresolved engineering issues

1. All route/via records explicitly remain `NOT_CERTIFIABLE` with placeholder endpoints and net IDs; this blocks actual topology until schematic is produced.
2. The W117 source/sink use `U10/U11` labels in safety recovery JSON. Confirm those labels against the approved BOM, footprints and netlist (do not silently remap by name similarity).
3. A distinct socket/CPU load 3D art asset exists outside the 88-part BOM. Do not inflate player install counts or silently assume its electrical contacts.
4. The 8-layer geometry is visualization; no manufacturing stack dimensions, copper weight or blind-via spans are guaranteed.
5. The physical model is a virtual educational CPU load and a nominal 1.00V/20A regulator target, not a datasheet-certified CPU motherboard.
6. Bundled material generation is starter-only; verify UVs and PBR remapping separately, and do not claim final AAA texture quality.
7. Existing script is an **art builder**, not a game-level builder. The new one-click playable-level menu is an implementation deliverable.

## Annex A — Exact model manifest — all 112 modular assets

**Exact source path:** `Level103_CPX11/Data/ModelManifest.csv`

```csv
index,ref,kind,description,group,fbx,glb,x_m,y_m,z_m,triangles
1,U1,ic,4-phase PWM controller,IC,001_L103_U1.fbx,001_L103_U1.glb,-0.092,0.0014,0.005,816
2,U2,ic,Phase driver A,IC,002_L103_U2.fbx,002_L103_U2.glb,-0.031,0.0014,0.005,336
3,U3,ic,Phase driver B,IC,003_L103_U3.fbx,003_L103_U3.glb,0.03,0.0014,0.005,336
4,U4,ic,Phase driver C,IC,004_L103_U4.fbx,004_L103_U4.glb,0.091,0.0014,0.005,336
5,U5,ic,Phase driver D,IC,005_L103_U5.fbx,005_L103_U5.glb,-0.092,0.0014,-0.027,336
6,U6,ic,Control 3.3V regulator,IC,006_L103_U6.fbx,006_L103_U6.glb,-0.031,0.0014,-0.027,336
7,U7,ic,Control 1.8V regulator,IC,007_L103_U7.fbx,007_L103_U7.glb,0.03,0.0014,-0.027,336
8,U8,ic,Input protection,IC,008_L103_U8.fbx,008_L103_U8.glb,0.091,0.0014,-0.027,336
9,U9,ic,Phase-current processing,IC,009_L103_U9.fbx,009_L103_U9.glb,-0.092,0.0014,-0.059,336
10,U10,ic,VCORE-good supervisor,IC,010_L103_U10.fbx,010_L103_U10.glb,-0.031,0.0014,-0.059,336
11,U11,ic,Independent CPU safety supervisor,IC,011_L103_U11.fbx,011_L103_U11.glb,0.03,0.0014,-0.059,336
12,U12,ic,Voltage telemetry interface,IC,012_L103_U12.fbx,012_L103_U12.glb,0.091,0.0014,-0.059,336
13,Q1,mosfet,Power MOSFET phase 1 high side,MOSFET,013_L103_Q1.fbx,013_L103_Q1.glb,-0.073,0.0014,0.037,96
14,Q2,mosfet,Power MOSFET phase 1 low side,MOSFET,014_L103_Q2.fbx,014_L103_Q2.glb,-0.073,0.0014,0.025,96
15,Q3,mosfet,Power MOSFET phase 2 high side,MOSFET,015_L103_Q3.fbx,015_L103_Q3.glb,-0.025,0.0014,0.037,96
16,Q4,mosfet,Power MOSFET phase 2 low side,MOSFET,016_L103_Q4.fbx,016_L103_Q4.glb,-0.025,0.0014,0.025,96
17,Q5,mosfet,Power MOSFET phase 3 high side,MOSFET,017_L103_Q5.fbx,017_L103_Q5.glb,0.023,0.0014,0.037,96
18,Q6,mosfet,Power MOSFET phase 3 low side,MOSFET,018_L103_Q6.fbx,018_L103_Q6.glb,0.023,0.0014,0.025,96
19,Q7,mosfet,Power MOSFET phase 4 high side,MOSFET,019_L103_Q7.fbx,019_L103_Q7.glb,0.071,0.0014,0.037,96
20,Q8,mosfet,Power MOSFET phase 4 low side,MOSFET,020_L103_Q8.fbx,020_L103_Q8.glb,0.071,0.0014,0.025,96
21,L1,inductor,VRM power phase 1 magnetic inductor,Inductor,021_L103_L1.fbx,021_L103_L1.glb,-0.073,0.0014,0.063,2736
22,L2,inductor,VRM power phase 2 magnetic inductor,Inductor,022_L103_L2.fbx,022_L103_L2.glb,-0.025,0.0014,0.063,2736
23,L3,inductor,VRM power phase 3 magnetic inductor,Inductor,023_L103_L3.fbx,023_L103_L3.glb,0.023,0.0014,0.063,2736
24,L4,inductor,VRM power phase 4 magnetic inductor,Inductor,024_L103_L4.fbx,024_L103_L4.glb,0.071,0.0014,0.063,2736
25,R1,resistor,VRM feedback or precision sense resistor 1,Resistor,025_L103_R1.fbx,025_L103_R1.glb,-0.098,0.0014,-0.05,36
26,R2,resistor,VRM feedback or precision sense resistor 2,Resistor,026_L103_R2.fbx,026_L103_R2.glb,-0.082,0.0014,-0.05,36
27,R3,resistor,VRM feedback or precision sense resistor 3,Resistor,027_L103_R3.fbx,027_L103_R3.glb,-0.066,0.0014,-0.05,36
28,R4,resistor,VRM feedback or precision sense resistor 4,Resistor,028_L103_R4.fbx,028_L103_R4.glb,-0.05,0.0014,-0.05,36
29,R5,resistor,VRM feedback or precision sense resistor 5,Resistor,029_L103_R5.fbx,029_L103_R5.glb,-0.034,0.0014,-0.05,36
30,R6,resistor,VRM feedback or precision sense resistor 6,Resistor,030_L103_R6.fbx,030_L103_R6.glb,-0.018,0.0014,-0.05,36
31,R7,resistor,VRM feedback or precision sense resistor 7,Resistor,031_L103_R7.fbx,031_L103_R7.glb,-0.002,0.0014,-0.05,36
32,R8,resistor,VRM feedback or precision sense resistor 8,Resistor,032_L103_R8.fbx,032_L103_R8.glb,0.014,0.0014,-0.05,36
33,R9,resistor,VRM feedback or precision sense resistor 9,Resistor,033_L103_R9.fbx,033_L103_R9.glb,0.03,0.0014,-0.05,36
34,R10,resistor,VRM feedback or precision sense resistor 10,Resistor,034_L103_R10.fbx,034_L103_R10.glb,0.046,0.0014,-0.05,36
35,R11,resistor,VRM feedback or precision sense resistor 11,Resistor,035_L103_R11.fbx,035_L103_R11.glb,0.062,0.0014,-0.05,36
36,R12,resistor,VRM feedback or precision sense resistor 12,Resistor,036_L103_R12.fbx,036_L103_R12.glb,0.078,0.0014,-0.05,36
37,R13,resistor,VRM feedback or precision sense resistor 13,Resistor,037_L103_R13.fbx,037_L103_R13.glb,0.094,0.0014,-0.05,36
38,R14,resistor,VRM feedback or precision sense resistor 14,Resistor,038_L103_R14.fbx,038_L103_R14.glb,-0.098,0.0014,-0.063,36
39,R15,resistor,VRM feedback or precision sense resistor 15,Resistor,039_L103_R15.fbx,039_L103_R15.glb,-0.082,0.0014,-0.063,36
40,R16,resistor,VRM feedback or precision sense resistor 16,Resistor,040_L103_R16.fbx,040_L103_R16.glb,-0.066,0.0014,-0.063,36
41,R17,resistor,VRM feedback or precision sense resistor 17,Resistor,041_L103_R17.fbx,041_L103_R17.glb,-0.05,0.0014,-0.063,36
42,R18,resistor,VRM feedback or precision sense resistor 18,Resistor,042_L103_R18.fbx,042_L103_R18.glb,-0.034,0.0014,-0.063,36
43,R19,resistor,VRM feedback or precision sense resistor 19,Resistor,043_L103_R19.fbx,043_L103_R19.glb,-0.018,0.0014,-0.063,36
44,R20,resistor,VRM feedback or precision sense resistor 20,Resistor,044_L103_R20.fbx,044_L103_R20.glb,-0.002,0.0014,-0.063,36
45,R21,resistor,VRM feedback or precision sense resistor 21,Resistor,045_L103_R21.fbx,045_L103_R21.glb,0.014,0.0014,-0.063,36
46,R22,resistor,VRM feedback or precision sense resistor 22,Resistor,046_L103_R22.fbx,046_L103_R22.glb,0.03,0.0014,-0.063,36
47,R23,resistor,VRM feedback or precision sense resistor 23,Resistor,047_L103_R23.fbx,047_L103_R23.glb,0.046,0.0014,-0.063,36
48,R24,resistor,VRM feedback or precision sense resistor 24,Resistor,048_L103_R24.fbx,048_L103_R24.glb,0.062,0.0014,-0.063,36
49,R25,resistor,VRM feedback or precision sense resistor 25,Resistor,049_L103_R25.fbx,049_L103_R25.glb,0.078,0.0014,-0.063,36
50,C1,bulkcap,Bulk polymer capacitor 1,Capacitor,050_L103_C1.fbx,050_L103_C1.glb,-0.096,0.0014,-0.018,216
51,C2,bulkcap,Bulk polymer capacitor 2,Capacitor,051_L103_C2.fbx,051_L103_C2.glb,-0.0788,0.0014,-0.018,216
52,C3,bulkcap,Bulk polymer capacitor 3,Capacitor,052_L103_C3.fbx,052_L103_C3.glb,-0.0616,0.0014,-0.018,216
53,C4,bulkcap,Bulk polymer capacitor 4,Capacitor,053_L103_C4.fbx,053_L103_C4.glb,-0.0444,0.0014,-0.018,216
54,C5,bulkcap,Bulk polymer capacitor 5,Capacitor,054_L103_C5.fbx,054_L103_C5.glb,-0.0272,0.0014,-0.018,216
55,C6,bulkcap,Bulk polymer capacitor 6,Capacitor,055_L103_C6.fbx,055_L103_C6.glb,-0.01,0.0014,-0.018,216
56,C7,capacitor,Ceramic decoupling capacitor 7,Capacitor,056_L103_C7.fbx,056_L103_C7.glb,0.0072,0.0014,-0.018,36
57,C8,capacitor,Ceramic decoupling capacitor 8,Capacitor,057_L103_C8.fbx,057_L103_C8.glb,0.0244,0.0014,-0.018,36
58,C9,capacitor,Ceramic decoupling capacitor 9,Capacitor,058_L103_C9.fbx,058_L103_C9.glb,0.0416,0.0014,-0.018,36
59,C10,capacitor,Ceramic decoupling capacitor 10,Capacitor,059_L103_C10.fbx,059_L103_C10.glb,0.0588,0.0014,-0.018,36
60,C11,capacitor,Ceramic decoupling capacitor 11,Capacitor,060_L103_C11.fbx,060_L103_C11.glb,0.076,0.0014,-0.018,36
61,C12,capacitor,Ceramic decoupling capacitor 12,Capacitor,061_L103_C12.fbx,061_L103_C12.glb,0.0932,0.0014,-0.018,36
62,C13,capacitor,Ceramic decoupling capacitor 13,Capacitor,062_L103_C13.fbx,062_L103_C13.glb,-0.096,0.0014,-0.033,36
63,C14,capacitor,Ceramic decoupling capacitor 14,Capacitor,063_L103_C14.fbx,063_L103_C14.glb,-0.0788,0.0014,-0.033,36
64,C15,capacitor,Ceramic decoupling capacitor 15,Capacitor,064_L103_C15.fbx,064_L103_C15.glb,-0.0616,0.0014,-0.033,36
65,C16,capacitor,Ceramic decoupling capacitor 16,Capacitor,065_L103_C16.fbx,065_L103_C16.glb,-0.0444,0.0014,-0.033,36
66,C17,capacitor,Ceramic decoupling capacitor 17,Capacitor,066_L103_C17.fbx,066_L103_C17.glb,-0.0272,0.0014,-0.033,36
67,C18,capacitor,Ceramic decoupling capacitor 18,Capacitor,067_L103_C18.fbx,067_L103_C18.glb,-0.01,0.0014,-0.033,36
68,C19,capacitor,Ceramic decoupling capacitor 19,Capacitor,068_L103_C19.fbx,068_L103_C19.glb,0.0072,0.0014,-0.033,36
69,C20,capacitor,Ceramic decoupling capacitor 20,Capacitor,069_L103_C20.fbx,069_L103_C20.glb,0.0244,0.0014,-0.033,36
70,C21,capacitor,Ceramic decoupling capacitor 21,Capacitor,070_L103_C21.fbx,070_L103_C21.glb,0.0416,0.0014,-0.033,36
71,C22,capacitor,Ceramic decoupling capacitor 22,Capacitor,071_L103_C22.fbx,071_L103_C22.glb,0.0588,0.0014,-0.033,36
72,C23,capacitor,Ceramic decoupling capacitor 23,Capacitor,072_L103_C23.fbx,072_L103_C23.glb,0.076,0.0014,-0.033,36
73,C24,capacitor,Ceramic decoupling capacitor 24,Capacitor,073_L103_C24.fbx,073_L103_C24.glb,0.0932,0.0014,-0.033,36
74,J1,connector,12V CPU power connector,Connector,074_L103_J1.fbx,074_L103_J1.glb,-0.095,0.0014,-0.078,120
75,J2,connector,CPU load sense connector,Connector,075_L103_J2.fbx,075_L103_J2.glb,-0.033,0.0014,-0.078,120
76,J3,connector,Debug and telemetry,Connector,076_L103_J3.fbx,076_L103_J3.glb,0.029,0.0014,-0.078,120
77,J4,connector,Power test header,Connector,077_L103_J4.fbx,077_L103_J4.glb,0.091,0.0014,-0.078,120
78,LED1,led,Status LED 1,LED,078_L103_LED1.fbx,078_L103_LED1.glb,-0.08,0.0014,-0.065,24
79,LED2,led,Status LED 2,LED,079_L103_LED2.fbx,079_L103_LED2.glb,-0.027,0.0014,-0.065,24
80,LED3,led,Status LED 3,LED,080_L103_LED3.fbx,080_L103_LED3.glb,0.026,0.0014,-0.065,24
81,LED4,led,Status LED 4,LED,081_L103_LED4.fbx,081_L103_LED4.glb,0.079,0.0014,-0.065,24
82,T1,sensor,Temperature or current sensor 1,Sensor,082_L103_T1.fbx,082_L103_T1.glb,0.07,0.0014,-0.048,288
83,T2,sensor,Temperature or current sensor 2,Sensor,083_L103_T2.fbx,083_L103_T2.glb,0.07,0.0014,-0.027,288
84,T3,sensor,Temperature or current sensor 3,Sensor,084_L103_T3.fbx,084_L103_T3.glb,0.07,0.0014,-0.006,288
85,D1,diode,Input protection diode 1,Diode,085_L103_D1.fbx,085_L103_D1.glb,0.093,0.0014,-0.006,36
86,D2,diode,Input protection diode 2,Diode,086_L103_D2.fbx,086_L103_D2.glb,0.093,0.0014,0.008,36
87,W117,jumper,Removable VCORE-good monitored feedback test jumper,Jumper,087_L103_W117.fbx,087_L103_W117.glb,0.091,0.0014,0.008,72
88,Y1,clock,VRM PWM reference oscillator,Clock,088_L103_Y1.fbx,088_L103_Y1.glb,-0.093,0.0014,0.018,24
89,PCB_Main,pcb,240x180mm eight-layer main board,Board,089_L103_PCB_Main.fbx,089_L103_PCB_Main.glb,0,0,0,4804
90,L1_Reference,layer,Internal copper layer L1 reference art,BoardLayer,090_L103_L1_Reference.fbx,090_L103_L1_Reference.glb,0,0,0,12
91,L2_Reference,layer,Internal copper layer L2 reference art,BoardLayer,091_L103_L2_Reference.fbx,091_L103_L2_Reference.glb,0,0,0,12
92,L3_Reference,layer,Internal copper layer L3 reference art,BoardLayer,092_L103_L3_Reference.fbx,092_L103_L3_Reference.glb,0,0,0,12
93,L4_Reference,layer,Internal copper layer L4 reference art,BoardLayer,093_L103_L4_Reference.fbx,093_L103_L4_Reference.glb,0,0,0,12
94,L5_Reference,layer,Internal copper layer L5 reference art,BoardLayer,094_L103_L5_Reference.fbx,094_L103_L5_Reference.glb,0,0,0,12
95,L6_Reference,layer,Internal copper layer L6 reference art,BoardLayer,095_L103_L6_Reference.fbx,095_L103_L6_Reference.glb,0,0,0,12
96,L7_Reference,layer,Internal copper layer L7 reference art,BoardLayer,096_L103_L7_Reference.fbx,096_L103_L7_Reference.glb,0,0,0,12
97,L8_Reference,layer,Internal copper layer L8 reference art,BoardLayer,097_L103_L8_Reference.fbx,097_L103_L8_Reference.glb,0,0,0,12
98,VRM_Plane_1,plane,Copper plane diagnostic reference,BoardDetail,098_L103_VRM_Plane_1.fbx,098_L103_VRM_Plane_1.glb,0,0,0,12
99,VRM_Plane_2,plane,Copper plane diagnostic reference,BoardDetail,099_L103_VRM_Plane_2.fbx,099_L103_VRM_Plane_2.glb,0,0,0,12
100,VRM_Plane_3,plane,Copper plane diagnostic reference,BoardDetail,100_L103_VRM_Plane_3.fbx,100_L103_VRM_Plane_3.glb,0,0,0,12
101,VRM_Plane_4,plane,Copper plane diagnostic reference,BoardDetail,101_L103_VRM_Plane_4.fbx,101_L103_VRM_Plane_4.glb,0,0,0,12
102,CPU_Socket_And_Load,cpu,"Detailed CPU socket, contacts and virtual CPU load module",Socket,102_L103_CPU_Socket_And_Load.fbx,102_L103_CPU_Socket_And_Load.glb,0,0,0,1916
103,ViaRepair_1,via,Via or copper repair visual state,Repair,103_L103_ViaRepair_1.fbx,103_L103_ViaRepair_1.glb,0,0,0,80
104,ViaRepair_2,via,Via or copper repair visual state,Repair,104_L103_ViaRepair_2.fbx,104_L103_ViaRepair_2.glb,0,0,0,80
105,ViaRepair_3,via,Via or copper repair visual state,Repair,105_L103_ViaRepair_3.fbx,105_L103_ViaRepair_3.glb,0,0,0,80
106,ViaRepair_4,repair,Via or copper repair visual state,Repair,106_L103_ViaRepair_4.fbx,106_L103_ViaRepair_4.glb,0,0,0,24
107,ViaRepair_5,repair,Via or copper repair visual state,Repair,107_L103_ViaRepair_5.fbx,107_L103_ViaRepair_5.glb,0,0,0,24
108,ViaRepair_6,repair,Via or copper repair visual state,Repair,108_L103_ViaRepair_6.fbx,108_L103_ViaRepair_6.glb,0,0,0,24
109,ID_Marker_1,silkscreen,Board identification decal/reference,Label,109_L103_ID_Marker_1.fbx,109_L103_ID_Marker_1.glb,0,0,0,12
110,ID_Marker_2,silkscreen,Board identification decal/reference,Label,110_L103_ID_Marker_2.fbx,110_L103_ID_Marker_2.glb,0,0,0,12
111,ID_Marker_3,silkscreen,Board identification decal/reference,Label,111_L103_ID_Marker_3.fbx,111_L103_ID_Marker_3.glb,0,0,0,12
112,ID_Marker_4,silkscreen,Board identification decal/reference,Label,112_L103_ID_Marker_4.fbx,112_L103_ID_Marker_4.glb,0,0,0,12
```

## Annex B — Component BOM — all 88 components

**Exact source path:** `Level103_CPX11/Data/ComponentBOM.csv`

```csv
ref,kind,description,group
U1,ic,4-phase PWM controller,IC
U2,ic,Phase driver A,IC
U3,ic,Phase driver B,IC
U4,ic,Phase driver C,IC
U5,ic,Phase driver D,IC
U6,ic,Control 3.3V regulator,IC
U7,ic,Control 1.8V regulator,IC
U8,ic,Input protection,IC
U9,ic,Phase-current processing,IC
U10,ic,VCORE-good supervisor,IC
U11,ic,Independent CPU safety supervisor,IC
U12,ic,Voltage telemetry interface,IC
Q1,mosfet,Power MOSFET phase 1 high side,MOSFET
Q2,mosfet,Power MOSFET phase 1 low side,MOSFET
Q3,mosfet,Power MOSFET phase 2 high side,MOSFET
Q4,mosfet,Power MOSFET phase 2 low side,MOSFET
Q5,mosfet,Power MOSFET phase 3 high side,MOSFET
Q6,mosfet,Power MOSFET phase 3 low side,MOSFET
Q7,mosfet,Power MOSFET phase 4 high side,MOSFET
Q8,mosfet,Power MOSFET phase 4 low side,MOSFET
L1,inductor,VRM power phase 1 magnetic inductor,Inductor
L2,inductor,VRM power phase 2 magnetic inductor,Inductor
L3,inductor,VRM power phase 3 magnetic inductor,Inductor
L4,inductor,VRM power phase 4 magnetic inductor,Inductor
R1,resistor,VRM feedback or precision sense resistor 1,Resistor
R2,resistor,VRM feedback or precision sense resistor 2,Resistor
R3,resistor,VRM feedback or precision sense resistor 3,Resistor
R4,resistor,VRM feedback or precision sense resistor 4,Resistor
R5,resistor,VRM feedback or precision sense resistor 5,Resistor
R6,resistor,VRM feedback or precision sense resistor 6,Resistor
R7,resistor,VRM feedback or precision sense resistor 7,Resistor
R8,resistor,VRM feedback or precision sense resistor 8,Resistor
R9,resistor,VRM feedback or precision sense resistor 9,Resistor
R10,resistor,VRM feedback or precision sense resistor 10,Resistor
R11,resistor,VRM feedback or precision sense resistor 11,Resistor
R12,resistor,VRM feedback or precision sense resistor 12,Resistor
R13,resistor,VRM feedback or precision sense resistor 13,Resistor
R14,resistor,VRM feedback or precision sense resistor 14,Resistor
R15,resistor,VRM feedback or precision sense resistor 15,Resistor
R16,resistor,VRM feedback or precision sense resistor 16,Resistor
R17,resistor,VRM feedback or precision sense resistor 17,Resistor
R18,resistor,VRM feedback or precision sense resistor 18,Resistor
R19,resistor,VRM feedback or precision sense resistor 19,Resistor
R20,resistor,VRM feedback or precision sense resistor 20,Resistor
R21,resistor,VRM feedback or precision sense resistor 21,Resistor
R22,resistor,VRM feedback or precision sense resistor 22,Resistor
R23,resistor,VRM feedback or precision sense resistor 23,Resistor
R24,resistor,VRM feedback or precision sense resistor 24,Resistor
R25,resistor,VRM feedback or precision sense resistor 25,Resistor
C1,bulkcap,Bulk polymer capacitor 1,Capacitor
C2,bulkcap,Bulk polymer capacitor 2,Capacitor
C3,bulkcap,Bulk polymer capacitor 3,Capacitor
C4,bulkcap,Bulk polymer capacitor 4,Capacitor
C5,bulkcap,Bulk polymer capacitor 5,Capacitor
C6,bulkcap,Bulk polymer capacitor 6,Capacitor
C7,capacitor,Ceramic decoupling capacitor 7,Capacitor
C8,capacitor,Ceramic decoupling capacitor 8,Capacitor
C9,capacitor,Ceramic decoupling capacitor 9,Capacitor
C10,capacitor,Ceramic decoupling capacitor 10,Capacitor
C11,capacitor,Ceramic decoupling capacitor 11,Capacitor
C12,capacitor,Ceramic decoupling capacitor 12,Capacitor
C13,capacitor,Ceramic decoupling capacitor 13,Capacitor
C14,capacitor,Ceramic decoupling capacitor 14,Capacitor
C15,capacitor,Ceramic decoupling capacitor 15,Capacitor
C16,capacitor,Ceramic decoupling capacitor 16,Capacitor
C17,capacitor,Ceramic decoupling capacitor 17,Capacitor
C18,capacitor,Ceramic decoupling capacitor 18,Capacitor
C19,capacitor,Ceramic decoupling capacitor 19,Capacitor
C20,capacitor,Ceramic decoupling capacitor 20,Capacitor
C21,capacitor,Ceramic decoupling capacitor 21,Capacitor
C22,capacitor,Ceramic decoupling capacitor 22,Capacitor
C23,capacitor,Ceramic decoupling capacitor 23,Capacitor
C24,capacitor,Ceramic decoupling capacitor 24,Capacitor
J1,connector,12V CPU power connector,Connector
J2,connector,CPU load sense connector,Connector
J3,connector,Debug and telemetry,Connector
J4,connector,Power test header,Connector
LED1,led,Status LED 1,LED
LED2,led,Status LED 2,LED
LED3,led,Status LED 3,LED
LED4,led,Status LED 4,LED
T1,sensor,Temperature or current sensor 1,Sensor
T2,sensor,Temperature or current sensor 2,Sensor
T3,sensor,Temperature or current sensor 3,Sensor
D1,diode,Input protection diode 1,Diode
D2,diode,Input protection diode 2,Diode
W117,jumper,Removable VCORE-good monitored feedback test jumper,Jumper
Y1,clock,VRM PWM reference oscillator,Clock
```

## Annex C — Component placement coordinates

**Exact source path:** `Level103_CPX11/Data/ComponentPlacements.csv`

```csv
ref,x_m,y_m,z_m,yaw_deg,initial_state
U1,-0.092,0.0014,0.005,0,Uninstalled
U2,-0.031,0.0014,0.005,0,Uninstalled
U3,0.03,0.0014,0.005,0,Uninstalled
U4,0.091,0.0014,0.005,0,Uninstalled
U5,-0.092,0.0014,-0.027,0,Uninstalled
U6,-0.031,0.0014,-0.027,0,Uninstalled
U7,0.03,0.0014,-0.027,0,Uninstalled
U8,0.091,0.0014,-0.027,0,Uninstalled
U9,-0.092,0.0014,-0.059,0,Uninstalled
U10,-0.031,0.0014,-0.059,0,Uninstalled
U11,0.03,0.0014,-0.059,0,Uninstalled
U12,0.091,0.0014,-0.059,0,Uninstalled
Q1,-0.073,0.0014,0.037,0,Uninstalled
Q2,-0.073,0.0014,0.025,0,Uninstalled
Q3,-0.025,0.0014,0.037,0,Uninstalled
Q4,-0.025,0.0014,0.025,0,Uninstalled
Q5,0.023,0.0014,0.037,0,Uninstalled
Q6,0.023,0.0014,0.025,0,Uninstalled
Q7,0.071,0.0014,0.037,0,Uninstalled
Q8,0.071,0.0014,0.025,0,Uninstalled
L1,-0.073,0.0014,0.063,0,Uninstalled
L2,-0.025,0.0014,0.063,0,Uninstalled
L3,0.023,0.0014,0.063,0,Uninstalled
L4,0.071,0.0014,0.063,0,Uninstalled
R1,-0.098,0.0014,-0.05,0,Uninstalled
R2,-0.082,0.0014,-0.05,0,Uninstalled
R3,-0.066,0.0014,-0.05,0,Uninstalled
R4,-0.05,0.0014,-0.05,0,Uninstalled
R5,-0.034,0.0014,-0.05,0,Uninstalled
R6,-0.018,0.0014,-0.05,0,Uninstalled
R7,-0.002,0.0014,-0.05,0,Uninstalled
R8,0.014,0.0014,-0.05,0,Uninstalled
R9,0.03,0.0014,-0.05,0,Uninstalled
R10,0.046,0.0014,-0.05,0,Uninstalled
R11,0.062,0.0014,-0.05,0,Uninstalled
R12,0.078,0.0014,-0.05,0,Uninstalled
R13,0.094,0.0014,-0.05,0,Uninstalled
R14,-0.098,0.0014,-0.063,0,Uninstalled
R15,-0.082,0.0014,-0.063,0,Uninstalled
R16,-0.066,0.0014,-0.063,0,Uninstalled
R17,-0.05,0.0014,-0.063,0,Uninstalled
R18,-0.034,0.0014,-0.063,0,Uninstalled
R19,-0.018,0.0014,-0.063,0,Uninstalled
R20,-0.002,0.0014,-0.063,0,Uninstalled
R21,0.014,0.0014,-0.063,0,Uninstalled
R22,0.03,0.0014,-0.063,0,Uninstalled
R23,0.046,0.0014,-0.063,0,Uninstalled
R24,0.062,0.0014,-0.063,0,Uninstalled
R25,0.078,0.0014,-0.063,0,Uninstalled
C1,-0.096,0.0014,-0.018,0,Uninstalled
C2,-0.0788,0.0014,-0.018,0,Uninstalled
C3,-0.0616,0.0014,-0.018,0,Uninstalled
C4,-0.0444,0.0014,-0.018,0,Uninstalled
C5,-0.0272,0.0014,-0.018,0,Uninstalled
C6,-0.01,0.0014,-0.018,0,Uninstalled
C7,0.0072,0.0014,-0.018,0,Uninstalled
C8,0.0244,0.0014,-0.018,0,Uninstalled
C9,0.0416,0.0014,-0.018,0,Uninstalled
C10,0.0588,0.0014,-0.018,0,Uninstalled
C11,0.076,0.0014,-0.018,0,Uninstalled
C12,0.0932,0.0014,-0.018,0,Uninstalled
C13,-0.096,0.0014,-0.033,0,Uninstalled
C14,-0.0788,0.0014,-0.033,0,Uninstalled
C15,-0.0616,0.0014,-0.033,0,Uninstalled
C16,-0.0444,0.0014,-0.033,0,Uninstalled
C17,-0.0272,0.0014,-0.033,0,Uninstalled
C18,-0.01,0.0014,-0.033,0,Uninstalled
C19,0.0072,0.0014,-0.033,0,Uninstalled
C20,0.0244,0.0014,-0.033,0,Uninstalled
C21,0.0416,0.0014,-0.033,0,Uninstalled
C22,0.0588,0.0014,-0.033,0,Uninstalled
C23,0.076,0.0014,-0.033,0,Uninstalled
C24,0.0932,0.0014,-0.033,0,Uninstalled
J1,-0.095,0.0014,-0.078,0,Uninstalled
J2,-0.033,0.0014,-0.078,0,Uninstalled
J3,0.029,0.0014,-0.078,0,Uninstalled
J4,0.091,0.0014,-0.078,0,Uninstalled
LED1,-0.08,0.0014,-0.065,0,Uninstalled
LED2,-0.027,0.0014,-0.065,0,Uninstalled
LED3,0.026,0.0014,-0.065,0,Uninstalled
LED4,0.079,0.0014,-0.065,0,Uninstalled
T1,0.07,0.0014,-0.048,0,Uninstalled
T2,0.07,0.0014,-0.027,0,Uninstalled
T3,0.07,0.0014,-0.006,0,Uninstalled
D1,0.093,0.0014,-0.006,0,Uninstalled
D2,0.093,0.0014,0.008,0,Uninstalled
W117,0.091,0.0014,0.008,0,Uninstalled
Y1,-0.093,0.0014,0.018,0,Uninstalled
```

## Annex D — 116 source copper-route records — unassigned

**Exact source path:** `Level103_CPX11/Data/CopperRoutes_116_REQUIRES_SCHEMATIC.csv`

```csv
id,group,net_id,source_pin,destination_pin,layer_span,validation_status
W001,Input/Protection,UNASSIGNED,REQUIRES_PIN_SCHEMATIC,REQUIRES_PIN_SCHEMATIC,UNASSIGNED,NOT_CERTIFIABLE
W002,Input/Protection,UNASSIGNED,REQUIRES_PIN_SCHEMATIC,REQUIRES_PIN_SCHEMATIC,UNASSIGNED,NOT_CERTIFIABLE
W003,Input/Protection,UNASSIGNED,REQUIRES_PIN_SCHEMATIC,REQUIRES_PIN_SCHEMATIC,UNASSIGNED,NOT_CERTIFIABLE
W004,Input/Protection,UNASSIGNED,REQUIRES_PIN_SCHEMATIC,REQUIRES_PIN_SCHEMATIC,UNASSIGNED,NOT_CERTIFIABLE
W005,Input/Protection,UNASSIGNED,REQUIRES_PIN_SCHEMATIC,REQUIRES_PIN_SCHEMATIC,UNASSIGNED,NOT_CERTIFIABLE
W006,Input/Protection,UNASSIGNED,REQUIRES_PIN_SCHEMATIC,REQUIRES_PIN_SCHEMATIC,UNASSIGNED,NOT_CERTIFIABLE
W007,Input/Protection,UNASSIGNED,REQUIRES_PIN_SCHEMATIC,REQUIRES_PIN_SCHEMATIC,UNASSIGNED,NOT_CERTIFIABLE
W008,Input/Protection,UNASSIGNED,REQUIRES_PIN_SCHEMATIC,REQUIRES_PIN_SCHEMATIC,UNASSIGNED,NOT_CERTIFIABLE
W009,Input/Protection,UNASSIGNED,REQUIRES_PIN_SCHEMATIC,REQUIRES_PIN_SCHEMATIC,UNASSIGNED,NOT_CERTIFIABLE
W010,Input/Protection,UNASSIGNED,REQUIRES_PIN_SCHEMATIC,REQUIRES_PIN_SCHEMATIC,UNASSIGNED,NOT_CERTIFIABLE
W011,Input/Protection,UNASSIGNED,REQUIRES_PIN_SCHEMATIC,REQUIRES_PIN_SCHEMATIC,UNASSIGNED,NOT_CERTIFIABLE
W012,Input/Protection,UNASSIGNED,REQUIRES_PIN_SCHEMATIC,REQUIRES_PIN_SCHEMATIC,UNASSIGNED,NOT_CERTIFIABLE
W013,Input/Protection,UNASSIGNED,REQUIRES_PIN_SCHEMATIC,REQUIRES_PIN_SCHEMATIC,UNASSIGNED,NOT_CERTIFIABLE
W014,Input/Protection,UNASSIGNED,REQUIRES_PIN_SCHEMATIC,REQUIRES_PIN_SCHEMATIC,UNASSIGNED,NOT_CERTIFIABLE
W015,Input/Protection,UNASSIGNED,REQUIRES_PIN_SCHEMATIC,REQUIRES_PIN_SCHEMATIC,UNASSIGNED,NOT_CERTIFIABLE
W016,Input/Protection,UNASSIGNED,REQUIRES_PIN_SCHEMATIC,REQUIRES_PIN_SCHEMATIC,UNASSIGNED,NOT_CERTIFIABLE
W017,VRM PH1/2,UNASSIGNED,REQUIRES_PIN_SCHEMATIC,REQUIRES_PIN_SCHEMATIC,UNASSIGNED,NOT_CERTIFIABLE
W018,VRM PH1/2,UNASSIGNED,REQUIRES_PIN_SCHEMATIC,REQUIRES_PIN_SCHEMATIC,UNASSIGNED,NOT_CERTIFIABLE
W019,VRM PH1/2,UNASSIGNED,REQUIRES_PIN_SCHEMATIC,REQUIRES_PIN_SCHEMATIC,UNASSIGNED,NOT_CERTIFIABLE
W020,VRM PH1/2,UNASSIGNED,REQUIRES_PIN_SCHEMATIC,REQUIRES_PIN_SCHEMATIC,UNASSIGNED,NOT_CERTIFIABLE
W021,VRM PH1/2,UNASSIGNED,REQUIRES_PIN_SCHEMATIC,REQUIRES_PIN_SCHEMATIC,UNASSIGNED,NOT_CERTIFIABLE
W022,VRM PH1/2,UNASSIGNED,REQUIRES_PIN_SCHEMATIC,REQUIRES_PIN_SCHEMATIC,UNASSIGNED,NOT_CERTIFIABLE
W023,VRM PH1/2,UNASSIGNED,REQUIRES_PIN_SCHEMATIC,REQUIRES_PIN_SCHEMATIC,UNASSIGNED,NOT_CERTIFIABLE
W024,VRM PH1/2,UNASSIGNED,REQUIRES_PIN_SCHEMATIC,REQUIRES_PIN_SCHEMATIC,UNASSIGNED,NOT_CERTIFIABLE
W025,VRM PH1/2,UNASSIGNED,REQUIRES_PIN_SCHEMATIC,REQUIRES_PIN_SCHEMATIC,UNASSIGNED,NOT_CERTIFIABLE
W026,VRM PH1/2,UNASSIGNED,REQUIRES_PIN_SCHEMATIC,REQUIRES_PIN_SCHEMATIC,UNASSIGNED,NOT_CERTIFIABLE
W027,VRM PH1/2,UNASSIGNED,REQUIRES_PIN_SCHEMATIC,REQUIRES_PIN_SCHEMATIC,UNASSIGNED,NOT_CERTIFIABLE
W028,VRM PH1/2,UNASSIGNED,REQUIRES_PIN_SCHEMATIC,REQUIRES_PIN_SCHEMATIC,UNASSIGNED,NOT_CERTIFIABLE
W029,VRM PH1/2,UNASSIGNED,REQUIRES_PIN_SCHEMATIC,REQUIRES_PIN_SCHEMATIC,UNASSIGNED,NOT_CERTIFIABLE
W030,VRM PH1/2,UNASSIGNED,REQUIRES_PIN_SCHEMATIC,REQUIRES_PIN_SCHEMATIC,UNASSIGNED,NOT_CERTIFIABLE
W031,VRM PH1/2,UNASSIGNED,REQUIRES_PIN_SCHEMATIC,REQUIRES_PIN_SCHEMATIC,UNASSIGNED,NOT_CERTIFIABLE
W032,VRM PH1/2,UNASSIGNED,REQUIRES_PIN_SCHEMATIC,REQUIRES_PIN_SCHEMATIC,UNASSIGNED,NOT_CERTIFIABLE
W033,VRM PH1/2,UNASSIGNED,REQUIRES_PIN_SCHEMATIC,REQUIRES_PIN_SCHEMATIC,UNASSIGNED,NOT_CERTIFIABLE
W034,VRM PH1/2,UNASSIGNED,REQUIRES_PIN_SCHEMATIC,REQUIRES_PIN_SCHEMATIC,UNASSIGNED,NOT_CERTIFIABLE
W035,VRM PH1/2,UNASSIGNED,REQUIRES_PIN_SCHEMATIC,REQUIRES_PIN_SCHEMATIC,UNASSIGNED,NOT_CERTIFIABLE
W036,VRM PH1/2,UNASSIGNED,REQUIRES_PIN_SCHEMATIC,REQUIRES_PIN_SCHEMATIC,UNASSIGNED,NOT_CERTIFIABLE
W037,VRM PH1/2,UNASSIGNED,REQUIRES_PIN_SCHEMATIC,REQUIRES_PIN_SCHEMATIC,UNASSIGNED,NOT_CERTIFIABLE
W038,VRM PH1/2,UNASSIGNED,REQUIRES_PIN_SCHEMATIC,REQUIRES_PIN_SCHEMATIC,UNASSIGNED,NOT_CERTIFIABLE
W039,VRM PH1/2,UNASSIGNED,REQUIRES_PIN_SCHEMATIC,REQUIRES_PIN_SCHEMATIC,UNASSIGNED,NOT_CERTIFIABLE
W040,VRM PH1/2,UNASSIGNED,REQUIRES_PIN_SCHEMATIC,REQUIRES_PIN_SCHEMATIC,UNASSIGNED,NOT_CERTIFIABLE
W041,VRM PH3/4,UNASSIGNED,REQUIRES_PIN_SCHEMATIC,REQUIRES_PIN_SCHEMATIC,UNASSIGNED,NOT_CERTIFIABLE
W042,VRM PH3/4,UNASSIGNED,REQUIRES_PIN_SCHEMATIC,REQUIRES_PIN_SCHEMATIC,UNASSIGNED,NOT_CERTIFIABLE
W043,VRM PH3/4,UNASSIGNED,REQUIRES_PIN_SCHEMATIC,REQUIRES_PIN_SCHEMATIC,UNASSIGNED,NOT_CERTIFIABLE
W044,VRM PH3/4,UNASSIGNED,REQUIRES_PIN_SCHEMATIC,REQUIRES_PIN_SCHEMATIC,UNASSIGNED,NOT_CERTIFIABLE
W045,VRM PH3/4,UNASSIGNED,REQUIRES_PIN_SCHEMATIC,REQUIRES_PIN_SCHEMATIC,UNASSIGNED,NOT_CERTIFIABLE
W046,VRM PH3/4,UNASSIGNED,REQUIRES_PIN_SCHEMATIC,REQUIRES_PIN_SCHEMATIC,UNASSIGNED,NOT_CERTIFIABLE
W047,VRM PH3/4,UNASSIGNED,REQUIRES_PIN_SCHEMATIC,REQUIRES_PIN_SCHEMATIC,UNASSIGNED,NOT_CERTIFIABLE
W048,VRM PH3/4,UNASSIGNED,REQUIRES_PIN_SCHEMATIC,REQUIRES_PIN_SCHEMATIC,UNASSIGNED,NOT_CERTIFIABLE
W049,VRM PH3/4,UNASSIGNED,REQUIRES_PIN_SCHEMATIC,REQUIRES_PIN_SCHEMATIC,UNASSIGNED,NOT_CERTIFIABLE
W050,VRM PH3/4,UNASSIGNED,REQUIRES_PIN_SCHEMATIC,REQUIRES_PIN_SCHEMATIC,UNASSIGNED,NOT_CERTIFIABLE
W051,VRM PH3/4,UNASSIGNED,REQUIRES_PIN_SCHEMATIC,REQUIRES_PIN_SCHEMATIC,UNASSIGNED,NOT_CERTIFIABLE
W052,VRM PH3/4,UNASSIGNED,REQUIRES_PIN_SCHEMATIC,REQUIRES_PIN_SCHEMATIC,UNASSIGNED,NOT_CERTIFIABLE
W053,VRM PH3/4,UNASSIGNED,REQUIRES_PIN_SCHEMATIC,REQUIRES_PIN_SCHEMATIC,UNASSIGNED,NOT_CERTIFIABLE
W054,VRM PH3/4,UNASSIGNED,REQUIRES_PIN_SCHEMATIC,REQUIRES_PIN_SCHEMATIC,UNASSIGNED,NOT_CERTIFIABLE
W055,VRM PH3/4,UNASSIGNED,REQUIRES_PIN_SCHEMATIC,REQUIRES_PIN_SCHEMATIC,UNASSIGNED,NOT_CERTIFIABLE
W056,VRM PH3/4,UNASSIGNED,REQUIRES_PIN_SCHEMATIC,REQUIRES_PIN_SCHEMATIC,UNASSIGNED,NOT_CERTIFIABLE
W057,VRM PH3/4,UNASSIGNED,REQUIRES_PIN_SCHEMATIC,REQUIRES_PIN_SCHEMATIC,UNASSIGNED,NOT_CERTIFIABLE
W058,VRM PH3/4,UNASSIGNED,REQUIRES_PIN_SCHEMATIC,REQUIRES_PIN_SCHEMATIC,UNASSIGNED,NOT_CERTIFIABLE
W059,VRM PH3/4,UNASSIGNED,REQUIRES_PIN_SCHEMATIC,REQUIRES_PIN_SCHEMATIC,UNASSIGNED,NOT_CERTIFIABLE
W060,VRM PH3/4,UNASSIGNED,REQUIRES_PIN_SCHEMATIC,REQUIRES_PIN_SCHEMATIC,UNASSIGNED,NOT_CERTIFIABLE
W061,VRM PH3/4,UNASSIGNED,REQUIRES_PIN_SCHEMATIC,REQUIRES_PIN_SCHEMATIC,UNASSIGNED,NOT_CERTIFIABLE
W062,VRM PH3/4,UNASSIGNED,REQUIRES_PIN_SCHEMATIC,REQUIRES_PIN_SCHEMATIC,UNASSIGNED,NOT_CERTIFIABLE
W063,VRM PH3/4,UNASSIGNED,REQUIRES_PIN_SCHEMATIC,REQUIRES_PIN_SCHEMATIC,UNASSIGNED,NOT_CERTIFIABLE
W064,VRM PH3/4,UNASSIGNED,REQUIRES_PIN_SCHEMATIC,REQUIRES_PIN_SCHEMATIC,UNASSIGNED,NOT_CERTIFIABLE
W065,VCORE,UNASSIGNED,REQUIRES_PIN_SCHEMATIC,REQUIRES_PIN_SCHEMATIC,UNASSIGNED,NOT_CERTIFIABLE
W066,VCORE,UNASSIGNED,REQUIRES_PIN_SCHEMATIC,REQUIRES_PIN_SCHEMATIC,UNASSIGNED,NOT_CERTIFIABLE
W067,VCORE,UNASSIGNED,REQUIRES_PIN_SCHEMATIC,REQUIRES_PIN_SCHEMATIC,UNASSIGNED,NOT_CERTIFIABLE
W068,VCORE,UNASSIGNED,REQUIRES_PIN_SCHEMATIC,REQUIRES_PIN_SCHEMATIC,UNASSIGNED,NOT_CERTIFIABLE
W069,VCORE,UNASSIGNED,REQUIRES_PIN_SCHEMATIC,REQUIRES_PIN_SCHEMATIC,UNASSIGNED,NOT_CERTIFIABLE
W070,VCORE,UNASSIGNED,REQUIRES_PIN_SCHEMATIC,REQUIRES_PIN_SCHEMATIC,UNASSIGNED,NOT_CERTIFIABLE
W071,VCORE,UNASSIGNED,REQUIRES_PIN_SCHEMATIC,REQUIRES_PIN_SCHEMATIC,UNASSIGNED,NOT_CERTIFIABLE
W072,VCORE,UNASSIGNED,REQUIRES_PIN_SCHEMATIC,REQUIRES_PIN_SCHEMATIC,UNASSIGNED,NOT_CERTIFIABLE
W073,VCORE,UNASSIGNED,REQUIRES_PIN_SCHEMATIC,REQUIRES_PIN_SCHEMATIC,UNASSIGNED,NOT_CERTIFIABLE
W074,VCORE,UNASSIGNED,REQUIRES_PIN_SCHEMATIC,REQUIRES_PIN_SCHEMATIC,UNASSIGNED,NOT_CERTIFIABLE
W075,VCORE,UNASSIGNED,REQUIRES_PIN_SCHEMATIC,REQUIRES_PIN_SCHEMATIC,UNASSIGNED,NOT_CERTIFIABLE
W076,VCORE,UNASSIGNED,REQUIRES_PIN_SCHEMATIC,REQUIRES_PIN_SCHEMATIC,UNASSIGNED,NOT_CERTIFIABLE
W077,VCORE,UNASSIGNED,REQUIRES_PIN_SCHEMATIC,REQUIRES_PIN_SCHEMATIC,UNASSIGNED,NOT_CERTIFIABLE
W078,VCORE,UNASSIGNED,REQUIRES_PIN_SCHEMATIC,REQUIRES_PIN_SCHEMATIC,UNASSIGNED,NOT_CERTIFIABLE
W079,VCORE,UNASSIGNED,REQUIRES_PIN_SCHEMATIC,REQUIRES_PIN_SCHEMATIC,UNASSIGNED,NOT_CERTIFIABLE
W080,VCORE,UNASSIGNED,REQUIRES_PIN_SCHEMATIC,REQUIRES_PIN_SCHEMATIC,UNASSIGNED,NOT_CERTIFIABLE
W081,VCORE,UNASSIGNED,REQUIRES_PIN_SCHEMATIC,REQUIRES_PIN_SCHEMATIC,UNASSIGNED,NOT_CERTIFIABLE
W082,VCORE,UNASSIGNED,REQUIRES_PIN_SCHEMATIC,REQUIRES_PIN_SCHEMATIC,UNASSIGNED,NOT_CERTIFIABLE
W083,Sensing/Thermal,UNASSIGNED,REQUIRES_PIN_SCHEMATIC,REQUIRES_PIN_SCHEMATIC,UNASSIGNED,NOT_CERTIFIABLE
W084,Sensing/Thermal,UNASSIGNED,REQUIRES_PIN_SCHEMATIC,REQUIRES_PIN_SCHEMATIC,UNASSIGNED,NOT_CERTIFIABLE
W085,Sensing/Thermal,UNASSIGNED,REQUIRES_PIN_SCHEMATIC,REQUIRES_PIN_SCHEMATIC,UNASSIGNED,NOT_CERTIFIABLE
W086,Sensing/Thermal,UNASSIGNED,REQUIRES_PIN_SCHEMATIC,REQUIRES_PIN_SCHEMATIC,UNASSIGNED,NOT_CERTIFIABLE
W087,Sensing/Thermal,UNASSIGNED,REQUIRES_PIN_SCHEMATIC,REQUIRES_PIN_SCHEMATIC,UNASSIGNED,NOT_CERTIFIABLE
W088,Sensing/Thermal,UNASSIGNED,REQUIRES_PIN_SCHEMATIC,REQUIRES_PIN_SCHEMATIC,UNASSIGNED,NOT_CERTIFIABLE
W089,Sensing/Thermal,UNASSIGNED,REQUIRES_PIN_SCHEMATIC,REQUIRES_PIN_SCHEMATIC,UNASSIGNED,NOT_CERTIFIABLE
W090,Sensing/Thermal,UNASSIGNED,REQUIRES_PIN_SCHEMATIC,REQUIRES_PIN_SCHEMATIC,UNASSIGNED,NOT_CERTIFIABLE
W091,Sensing/Thermal,UNASSIGNED,REQUIRES_PIN_SCHEMATIC,REQUIRES_PIN_SCHEMATIC,UNASSIGNED,NOT_CERTIFIABLE
W092,Sensing/Thermal,UNASSIGNED,REQUIRES_PIN_SCHEMATIC,REQUIRES_PIN_SCHEMATIC,UNASSIGNED,NOT_CERTIFIABLE
W093,Sensing/Thermal,UNASSIGNED,REQUIRES_PIN_SCHEMATIC,REQUIRES_PIN_SCHEMATIC,UNASSIGNED,NOT_CERTIFIABLE
W094,Sensing/Thermal,UNASSIGNED,REQUIRES_PIN_SCHEMATIC,REQUIRES_PIN_SCHEMATIC,UNASSIGNED,NOT_CERTIFIABLE
W095,Sensing/Thermal,UNASSIGNED,REQUIRES_PIN_SCHEMATIC,REQUIRES_PIN_SCHEMATIC,UNASSIGNED,NOT_CERTIFIABLE
W096,Sensing/Thermal,UNASSIGNED,REQUIRES_PIN_SCHEMATIC,REQUIRES_PIN_SCHEMATIC,UNASSIGNED,NOT_CERTIFIABLE
W097,Sensing/Thermal,UNASSIGNED,REQUIRES_PIN_SCHEMATIC,REQUIRES_PIN_SCHEMATIC,UNASSIGNED,NOT_CERTIFIABLE
W098,Sensing/Thermal,UNASSIGNED,REQUIRES_PIN_SCHEMATIC,REQUIRES_PIN_SCHEMATIC,UNASSIGNED,NOT_CERTIFIABLE
W099,Sensing/Thermal,UNASSIGNED,REQUIRES_PIN_SCHEMATIC,REQUIRES_PIN_SCHEMATIC,UNASSIGNED,NOT_CERTIFIABLE
W100,Sensing/Thermal,UNASSIGNED,REQUIRES_PIN_SCHEMATIC,REQUIRES_PIN_SCHEMATIC,UNASSIGNED,NOT_CERTIFIABLE
W101,Safety,UNASSIGNED,REQUIRES_PIN_SCHEMATIC,REQUIRES_PIN_SCHEMATIC,UNASSIGNED,NOT_CERTIFIABLE
W102,Safety,UNASSIGNED,REQUIRES_PIN_SCHEMATIC,REQUIRES_PIN_SCHEMATIC,UNASSIGNED,NOT_CERTIFIABLE
W103,Safety,UNASSIGNED,REQUIRES_PIN_SCHEMATIC,REQUIRES_PIN_SCHEMATIC,UNASSIGNED,NOT_CERTIFIABLE
W104,Safety,UNASSIGNED,REQUIRES_PIN_SCHEMATIC,REQUIRES_PIN_SCHEMATIC,UNASSIGNED,NOT_CERTIFIABLE
W105,Safety,UNASSIGNED,REQUIRES_PIN_SCHEMATIC,REQUIRES_PIN_SCHEMATIC,UNASSIGNED,NOT_CERTIFIABLE
W106,Safety,UNASSIGNED,REQUIRES_PIN_SCHEMATIC,REQUIRES_PIN_SCHEMATIC,UNASSIGNED,NOT_CERTIFIABLE
W107,Safety,UNASSIGNED,REQUIRES_PIN_SCHEMATIC,REQUIRES_PIN_SCHEMATIC,UNASSIGNED,NOT_CERTIFIABLE
W108,Safety,UNASSIGNED,REQUIRES_PIN_SCHEMATIC,REQUIRES_PIN_SCHEMATIC,UNASSIGNED,NOT_CERTIFIABLE
W109,Safety,UNASSIGNED,REQUIRES_PIN_SCHEMATIC,REQUIRES_PIN_SCHEMATIC,UNASSIGNED,NOT_CERTIFIABLE
W110,Safety,UNASSIGNED,REQUIRES_PIN_SCHEMATIC,REQUIRES_PIN_SCHEMATIC,UNASSIGNED,NOT_CERTIFIABLE
W111,Safety,UNASSIGNED,REQUIRES_PIN_SCHEMATIC,REQUIRES_PIN_SCHEMATIC,UNASSIGNED,NOT_CERTIFIABLE
W112,Safety,UNASSIGNED,REQUIRES_PIN_SCHEMATIC,REQUIRES_PIN_SCHEMATIC,UNASSIGNED,NOT_CERTIFIABLE
W113,Safety,UNASSIGNED,REQUIRES_PIN_SCHEMATIC,REQUIRES_PIN_SCHEMATIC,UNASSIGNED,NOT_CERTIFIABLE
W114,Safety,UNASSIGNED,REQUIRES_PIN_SCHEMATIC,REQUIRES_PIN_SCHEMATIC,UNASSIGNED,NOT_CERTIFIABLE
W115,Safety,UNASSIGNED,REQUIRES_PIN_SCHEMATIC,REQUIRES_PIN_SCHEMATIC,UNASSIGNED,NOT_CERTIFIABLE
W116,Safety,UNASSIGNED,REQUIRES_PIN_SCHEMATIC,REQUIRES_PIN_SCHEMATIC,UNASSIGNED,NOT_CERTIFIABLE
```

## Annex E — 30 source via records — unassigned

**Exact source path:** `Level103_CPX11/Data/ViaTasks_30_REQUIRES_SCHEMATIC.csv`

```csv
id,group,net_id,source_pin,destination_pin,layer_span,validation_status
V001,Layer Transition,UNASSIGNED,REQUIRES_PIN_SCHEMATIC,REQUIRES_PIN_SCHEMATIC,UNASSIGNED,NOT_CERTIFIABLE
V002,Layer Transition,UNASSIGNED,REQUIRES_PIN_SCHEMATIC,REQUIRES_PIN_SCHEMATIC,UNASSIGNED,NOT_CERTIFIABLE
V003,Layer Transition,UNASSIGNED,REQUIRES_PIN_SCHEMATIC,REQUIRES_PIN_SCHEMATIC,UNASSIGNED,NOT_CERTIFIABLE
V004,Layer Transition,UNASSIGNED,REQUIRES_PIN_SCHEMATIC,REQUIRES_PIN_SCHEMATIC,UNASSIGNED,NOT_CERTIFIABLE
V005,Layer Transition,UNASSIGNED,REQUIRES_PIN_SCHEMATIC,REQUIRES_PIN_SCHEMATIC,UNASSIGNED,NOT_CERTIFIABLE
V006,Layer Transition,UNASSIGNED,REQUIRES_PIN_SCHEMATIC,REQUIRES_PIN_SCHEMATIC,UNASSIGNED,NOT_CERTIFIABLE
V007,Layer Transition,UNASSIGNED,REQUIRES_PIN_SCHEMATIC,REQUIRES_PIN_SCHEMATIC,UNASSIGNED,NOT_CERTIFIABLE
V008,Layer Transition,UNASSIGNED,REQUIRES_PIN_SCHEMATIC,REQUIRES_PIN_SCHEMATIC,UNASSIGNED,NOT_CERTIFIABLE
V009,Layer Transition,UNASSIGNED,REQUIRES_PIN_SCHEMATIC,REQUIRES_PIN_SCHEMATIC,UNASSIGNED,NOT_CERTIFIABLE
V010,Layer Transition,UNASSIGNED,REQUIRES_PIN_SCHEMATIC,REQUIRES_PIN_SCHEMATIC,UNASSIGNED,NOT_CERTIFIABLE
V011,Layer Transition,UNASSIGNED,REQUIRES_PIN_SCHEMATIC,REQUIRES_PIN_SCHEMATIC,UNASSIGNED,NOT_CERTIFIABLE
V012,Layer Transition,UNASSIGNED,REQUIRES_PIN_SCHEMATIC,REQUIRES_PIN_SCHEMATIC,UNASSIGNED,NOT_CERTIFIABLE
V013,Layer Transition,UNASSIGNED,REQUIRES_PIN_SCHEMATIC,REQUIRES_PIN_SCHEMATIC,UNASSIGNED,NOT_CERTIFIABLE
V014,Layer Transition,UNASSIGNED,REQUIRES_PIN_SCHEMATIC,REQUIRES_PIN_SCHEMATIC,UNASSIGNED,NOT_CERTIFIABLE
V015,Layer Transition,UNASSIGNED,REQUIRES_PIN_SCHEMATIC,REQUIRES_PIN_SCHEMATIC,UNASSIGNED,NOT_CERTIFIABLE
V016,Layer Transition,UNASSIGNED,REQUIRES_PIN_SCHEMATIC,REQUIRES_PIN_SCHEMATIC,UNASSIGNED,NOT_CERTIFIABLE
V017,Layer Transition,UNASSIGNED,REQUIRES_PIN_SCHEMATIC,REQUIRES_PIN_SCHEMATIC,UNASSIGNED,NOT_CERTIFIABLE
V018,Layer Transition,UNASSIGNED,REQUIRES_PIN_SCHEMATIC,REQUIRES_PIN_SCHEMATIC,UNASSIGNED,NOT_CERTIFIABLE
V019,Layer Transition,UNASSIGNED,REQUIRES_PIN_SCHEMATIC,REQUIRES_PIN_SCHEMATIC,UNASSIGNED,NOT_CERTIFIABLE
V020,Layer Transition,UNASSIGNED,REQUIRES_PIN_SCHEMATIC,REQUIRES_PIN_SCHEMATIC,UNASSIGNED,NOT_CERTIFIABLE
V021,Layer Transition,UNASSIGNED,REQUIRES_PIN_SCHEMATIC,REQUIRES_PIN_SCHEMATIC,UNASSIGNED,NOT_CERTIFIABLE
V022,Layer Transition,UNASSIGNED,REQUIRES_PIN_SCHEMATIC,REQUIRES_PIN_SCHEMATIC,UNASSIGNED,NOT_CERTIFIABLE
V023,Layer Transition,UNASSIGNED,REQUIRES_PIN_SCHEMATIC,REQUIRES_PIN_SCHEMATIC,UNASSIGNED,NOT_CERTIFIABLE
V024,Layer Transition,UNASSIGNED,REQUIRES_PIN_SCHEMATIC,REQUIRES_PIN_SCHEMATIC,UNASSIGNED,NOT_CERTIFIABLE
V025,Layer Transition,UNASSIGNED,REQUIRES_PIN_SCHEMATIC,REQUIRES_PIN_SCHEMATIC,UNASSIGNED,NOT_CERTIFIABLE
V026,Layer Transition,UNASSIGNED,REQUIRES_PIN_SCHEMATIC,REQUIRES_PIN_SCHEMATIC,UNASSIGNED,NOT_CERTIFIABLE
V027,Layer Transition,UNASSIGNED,REQUIRES_PIN_SCHEMATIC,REQUIRES_PIN_SCHEMATIC,UNASSIGNED,NOT_CERTIFIABLE
V028,Layer Transition,UNASSIGNED,REQUIRES_PIN_SCHEMATIC,REQUIRES_PIN_SCHEMATIC,UNASSIGNED,NOT_CERTIFIABLE
V029,Layer Transition,UNASSIGNED,REQUIRES_PIN_SCHEMATIC,REQUIRES_PIN_SCHEMATIC,UNASSIGNED,NOT_CERTIFIABLE
V030,Layer Transition,UNASSIGNED,REQUIRES_PIN_SCHEMATIC,REQUIRES_PIN_SCHEMATIC,UNASSIGNED,NOT_CERTIFIABLE
```

## Annex F — W117 safety recovery JSON

**Exact source path:** `Level103_CPX11/Data/W117_SafetyRecovery.json`

```json
{
  "wire": "W117",
  "source": "L103_VcoreSupervisor_U10.TRUSTED_VCORE_GOOD_OUT",
  "sink": "L103_CPUSafetySupervisor_U11.TRUSTED_VCORE_GOOD_IN",
  "purpose": "monitored power-good signal, not high-current CPU VCORE",
  "initial": "Disconnected",
  "recovery": [
    "Install for initial commissioning",
    "After Objectives 1-5 disconnect during supervised test",
    "Independent supervisor inhibits CPU authorization",
    "Reconnect real functional terminals",
    "Acquire fresh rail health and phase current evidence",
    "Explicit safety reset",
    "Repeat complete power qualification"
  ],
  "status": "Design stage; pin-level schematic required"
}
```

## Annex G — Three fault definitions JSON

**Exact source path:** `Level103_CPX11/Data/FaultPlan.json`

```json
[
  {
    "id": "F1",
    "fault": "Phase 2 current sense drift",
    "affects": "measured phase current and balancing"
  },
  {
    "id": "F2",
    "fault": "Phase 3 inductor-output open via",
    "affects": "phase contribution and common VCORE"
  },
  {
    "id": "F3",
    "fault": "Phase 4 MOSFET thermal loss",
    "affects": "conduction losses, heat and protection"
  }
]
```

## Annex H — Six objective definitions JSON

**Exact source path:** `Level103_CPX11/Data/Objectives.json`

```json
[
  {
    "id": 1,
    "title": "Assemble 88 components"
  },
  {
    "id": 2,
    "title": "Build 116 copper routes and 30 vias"
  },
  {
    "id": 3,
    "title": "Commission four-phase VRM"
  },
  {
    "id": 4,
    "title": "Master CPU load stability"
  },
  {
    "id": 5,
    "title": "Recover 3 real power faults"
  },
  {
    "id": 6,
    "title": "Restore W117 and requalify"
  }
]
```

## Annex I — Export summary JSON

**Exact source path:** `Level103_CPX11/Data/ExportSummary.json`

```json
{
  "level": 103,
  "name": "The Power Core",
  "board_size_mm": [
    240,
    180
  ],
  "layers": 8,
  "modular_fbx": 112,
  "installable_components": 88,
  "board_detail_models": 24,
  "modular_triangles": 27900,
  "route_planning_records": 116,
  "via_planning_records": 30,
  "units": "GLB meters; FBX vertices centimeters with UnitScaleFactor 1",
  "coordinate_system": "Y up, board XZ",
  "simulated_connection_status": "No verified pin-level netlist"
}
```

## Annex J — Source gameplay implementation notes

**Exact source path:** `Level103_CPX11/Documentation/GameplayImplementationNotes.md`

```markdown
# Functional gameplay requirements

Objective 1: install 88 correctly oriented/value parts. Objective 2: 116 verified routes and 30 vias. Objective 3: 4-phase interleaved nominal 400 kHz phase offset 90 degrees, 1.00V target and stable feedback. Objective 4: 2-20 A load and thermal stability. Objective 5: faults phase 2 current sense, phase 3 via, phase 4 MOSFET. Objective 6: W117 unplug/replug, fresh calibration, explicit safety reset and independent qualification.

No component rendering implies electrical continuity. CPU load simulator and PCB solver must use the same authoritative netlist.
```

## Annex K — Source README

**Exact source path:** `Level103_CPX11/Documentation/README_FIRST.md`

```markdown
# Project Spark — Level 103: THE POWER CORE

REAL EDITABLE BOARD-ONLY FBX/GLB ASSETS. No UI screenshots or rooms.

- 112 modular FBX + 112 GLB, with one assembled FBX and GLB reference.
- 88 installable components + 24 board detail models, including an independent CPU socket/load mesh.
- 240 x 180 mm, eight copper layers (reference visualization).
- 116 copper routes and 30 via tasks are UNASSIGNED planning records and NOT a verified schematic.
- W117 is a removable logical VCORE-good jumper, not CPU current path.
- Component FBXs have local geometry, manifest specifies design positions in meters; assembly reference places them.
- FBX geometry uses centimeters (FBX UnitScaleFactor 1); GLB in meters. Verify Unity import scale (the PCB should be 0.240 x 0.180 m).
- This kit uses procedural geometry and starter PBR maps; artist-finished AAA baking/LOD and Unity validation are not claimed.

## Install
Copy this root to Unity Assets. Run Tools > Project Spark > Levels > 103 > 1. Build EMPTY CPX-11 Board. Then 2. Create Starter Materials.

Do not award any objective from showcase visuals. Verify pins/netlist before running electric solver or simulated VRM.
```

## Annex L — Material guide JSON

**Exact source path:** `Level103_CPX11/Materials/MaterialGuide.json`

```json
{
  "PCB_Navy": {
    "base_color": "#092a47",
    "metallic": 0.0,
    "roughness": 0.68,
    "texture_prefix": "PCB_Navy"
  },
  "PCB_Fiberglass": {
    "base_color": "#594b2c",
    "metallic": 0.0,
    "roughness": 0.88,
    "texture_prefix": "PCB_Fiberglass"
  },
  "Copper": {
    "base_color": "#b87936",
    "metallic": 0.82,
    "roughness": 0.28,
    "texture_prefix": "Copper"
  },
  "Gold_ENIG": {
    "base_color": "#dbb55e",
    "metallic": 0.88,
    "roughness": 0.2,
    "texture_prefix": "Gold_ENIG"
  },
  "Solder_Tin": {
    "base_color": "#abb3bc",
    "metallic": 0.75,
    "roughness": 0.29,
    "texture_prefix": "Solder_Tin"
  },
  "IC_Mold": {
    "base_color": "#1b222c",
    "metallic": 0.07,
    "roughness": 0.54,
    "texture_prefix": "IC_Mold"
  },
  "IC_LaserMark": {
    "base_color": "#9ca9b6",
    "metallic": 0.0,
    "roughness": 0.78,
    "texture_prefix": "IC_LaserMark"
  },
  "Ceramic": {
    "base_color": "#d0ba8e",
    "metallic": 0.02,
    "roughness": 0.5,
    "texture_prefix": "Ceramic"
  },
  "Resistor_Black": {
    "base_color": "#363534",
    "metallic": 0.04,
    "roughness": 0.7,
    "texture_prefix": "Resistor_Black"
  },
  "Cap_Aluminum": {
    "base_color": "#b5bac4",
    "metallic": 0.79,
    "roughness": 0.27,
    "texture_prefix": "Cap_Aluminum"
  },
  "Cap_Top": {
    "base_color": "#d5dae0",
    "metallic": 0.4,
    "roughness": 0.34,
    "texture_prefix": "Cap_Top"
  },
  "Ferrite": {
    "base_color": "#39414b",
    "metallic": 0.04,
    "roughness": 0.73,
    "texture_prefix": "Ferrite"
  },
  "Plastic_Black": {
    "base_color": "#1b222a",
    "metallic": 0.05,
    "roughness": 0.55,
    "texture_prefix": "Plastic_Black"
  },
  "Plastic_Grey": {
    "base_color": "#5a626e",
    "metallic": 0.05,
    "roughness": 0.62,
    "texture_prefix": "Plastic_Grey"
  },
  "Copper_Winding": {
    "base_color": "#c86b28",
    "metallic": 0.73,
    "roughness": 0.27,
    "texture_prefix": "Copper_Winding"
  },
  "Inductor_Core": {
    "base_color": "#4c535b",
    "metallic": 0.09,
    "roughness": 0.7,
    "texture_prefix": "Inductor_Core"
  },
  "LED_Green": {
    "base_color": "#35f05d",
    "metallic": 0.06,
    "roughness": 0.21,
    "texture_prefix": "LED_Green"
  },
  "LED_Amber": {
    "base_color": "#ffbc52",
    "metallic": 0.06,
    "roughness": 0.22,
    "texture_prefix": "LED_Amber"
  },
  "Connector_Metal": {
    "base_color": "#c5c9d0",
    "metallic": 0.8,
    "roughness": 0.26,
    "texture_prefix": "Connector_Metal"
  },
  "Warning_Red": {
    "base_color": "#be2731",
    "metallic": 0.1,
    "roughness": 0.32,
    "texture_prefix": "Warning_Red"
  },
  "Thermal_Pad": {
    "base_color": "#63717d",
    "metallic": 0.12,
    "roughness": 0.76,
    "texture_prefix": "Thermal_Pad"
  },
  "Rubber": {
    "base_color": "#191c20",
    "metallic": 0.0,
    "roughness": 0.94,
    "texture_prefix": "Rubber"
  },
  "Glass": {
    "base_color": "#8bb6c8",
    "metallic": 0.15,
    "roughness": 0.15,
    "texture_prefix": "Glass"
  }
}
```

## Annex M — Unity empty board art builder

**Exact source path:** `Level103_CPX11/Unity/Editor/SparkL103BoardArtBuilder.cs`

```csharp
#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;
public static class SparkL103BoardArtBuilder
{
    [MenuItem("Tools/Project Spark/Levels/103/1. Build EMPTY CPX-11 Board")]
    public static void BuildEmpty()
    {
        string[] guids = AssetDatabase.FindAssets("001_L103_U1 t:Model");
        // Find package root via any ModelManifest, not hardcoded Assets path.
        string root = null;
        foreach (var g in AssetDatabase.FindAssets("ModelManifest"))
        {
            string p=AssetDatabase.GUIDToAssetPath(g).Replace('\\','/');
            if (p.Contains("Level103") || p.Contains("PowerCore")) { root=Path.GetDirectoryName(Path.GetDirectoryName(p)).Replace('\\','/'); break; }
        }
        if (root == null) { Debug.LogError("[L103] Level 103 Data/ModelManifest.csv not found.");return; }
        var model=AssetDatabase.LoadAssetAtPath<GameObject>(root+"/Models_FBX/089_L103_PCB_Main.fbx");
        if (!model) { Debug.LogError("[L103] Missing PCB_Main FBX. Check ModelManifest.csv.");return; }
        var parent=new GameObject("L103_CPX11_EMPTY_GAMEPLAY_ART");
        var go=(GameObject)PrefabUtility.InstantiatePrefab(model);go.transform.SetParent(parent.transform,false);
        go.name="VisualOnly_PCB_Main";
        Selection.activeGameObject=parent;
        Debug.Log("[L103] Empty board created. 88 components, 116 routes, 30 vias and W117 remain NOT installed. Attach Project Spark gameplay systems separately.");
    }
}
#endif
```

## Annex N — Unity starter material generator

**Exact source path:** `Level103_CPX11/Unity/Editor/SparkL103MaterialGenerator.cs`

```csharp
#if UNITY_EDITOR
using System.IO;
using UnityEngine;
using UnityEditor;
public static class SparkL103MaterialGenerator
{
 [MenuItem("Tools/Project Spark/Levels/103/2. Create Starter Materials")]
 public static void Build()
 {
  string folder=null;
  foreach (var guid in AssetDatabase.FindAssets("PCB_Navy_BaseColor t:Texture2D"))
  {
   var path=AssetDatabase.GUIDToAssetPath(guid).Replace('\\','/');
   if(path.Contains("Level103") || path.Contains("PowerCore")) {folder=Path.GetDirectoryName(path).Replace('\\','/');break;}
  }
  if(folder==null){Debug.LogError("[L103] Texture folder not found. Keep Textures with this package.");return;}
  var dest=Path.GetDirectoryName(folder).Replace('\\','/')+"/UnityMaterials";
  if(!AssetDatabase.IsValidFolder(dest))AssetDatabase.CreateFolder(Path.GetDirectoryName(dest).Replace('\\','/'),"UnityMaterials");
  string[] names={"PCB_Navy","PCB_Fiberglass","Copper","Gold_ENIG","Solder_Tin","IC_Mold","IC_LaserMark","Ceramic","Resistor_Black","Cap_Aluminum","Cap_Top","Ferrite","Plastic_Black","Plastic_Grey","Copper_Winding","Inductor_Core","LED_Green","LED_Amber","Connector_Metal","Warning_Red","Thermal_Pad","Rubber","Glass"};
  foreach(var n in names)
  {
   Shader shader=Shader.Find("Universal Render Pipeline/Lit"); if(shader==null) shader=Shader.Find("Standard");
   if(shader==null){Debug.LogError("[L103] Compatible shader not found");return;}
   Material mat=new Material(shader){name=n};
   var col=AssetDatabase.LoadAssetAtPath<Texture2D>(folder+"/"+n+"_BaseColor.png");
   if(mat.HasProperty("_BaseMap"))mat.SetTexture("_BaseMap",col);
   if(mat.HasProperty("_MainTex"))mat.SetTexture("_MainTex",col);
   var save=dest+"/"+n+".mat";
   if(AssetDatabase.LoadAssetAtPath<Material>(save)==null)AssetDatabase.CreateAsset(mat,save);else Object.DestroyImmediate(mat);
  }
  AssetDatabase.SaveAssets(); AssetDatabase.Refresh();
  Debug.Log("[L103] Starter materials generated. FBX slot remapping and pipeline-specific normal/metallic packing require review.");
 }
}
#endif
```

## Annex O — Unity visual-only runtime marker

**Exact source path:** `Level103_CPX11/Unity/Runtime/SparkL103VisualOnly.cs`

```csharp
using UnityEngine;
[DisallowMultipleComponent] public sealed class SparkL103VisualOnly : MonoBehaviour { [Tooltip("Visual geometry only: never awards gameplay objectives.")] public bool referenceOnly=true; }
```

## Final implementing instruction

Do not modify existing project gameplay by guessing. Start by inspecting the actual APIs and reconciling the approved schematic, then deliver the full playable level step by step with tests. A successful result is a board-only Level 103 assembled and repaired through virtual click/touch tools, four physically coherent VRM phases supplying a virtual CPU, credible safety response to W117, verified evidence for all six objectives, and no regressions in Levels 1–102. If source information is insufficient, block the affected certification and explicitly record the missing input; never fabricate a pin-level netlist or declare Play Mode validation that has not occurred.

