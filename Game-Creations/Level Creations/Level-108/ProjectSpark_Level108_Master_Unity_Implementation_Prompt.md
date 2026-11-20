# PROJECT SPARK — LEVEL 108: GHOST IN THE CIRCUIT

## MASTER UNITY IMPLEMENTATION PROMPT — SOURCE-GROUNDED / ENGINEERING AUTHORITY REQUIRED
**Project:** Project Spark  | **Level:** 108  | **Board:** DFX-16 Diagnostic Fusion  | **Document type:** implementation brief and AI coding-agent task  | **Status:** specification only, NOT compiled or tested.
**Source basis:** `ProjectSpark_Level108_GhostInCircuit_BoardOnly_Realistic_Complete.zip`, inspected directly. All source annexes below are transcribed from the package without silently assigning unknown electrical connections. This prompt is intended to be handed to a Unity coding agent that has the full Project Spark project available.

## 0. Mandatory agent instructions — read before writing code

You are the lead Unity gameplay and embedded systems engineer for Project Spark. Your task is to integrate **Level 108 — Ghost in the Circuit** into the existing Unity game as a complete, deterministic, testable fault-investigation and repair level. Work against the actual project, not a new isolated prototype. Inspect code, serialized prefabs, existing Scenes, asmdefs, input system, `LevelGamePlayManager`, shelf/purchase mechanisms, board tools, routing, electrical simulation, level builders, level progression, UI and VFX before modifying anything. Reuse established APIs when they exist; proposed class names in this document are NOT authoritative APIs. Do not delete, rename, bypass or regress Levels 1–107. Implement in incremental, compile-tested changes. No external web downloads, proprietary SDKs or third-party dependencies are assumed. Do not claim implementation success until compiling, Play Mode testing and the acceptance tests succeed.

**Non-negotiable validity rule:** All 156 copper-route records and all 50 via records in the supplied ZIP are marked `NOT_CERTIFIABLE`, with nets, pins, layers and spans `UNASSIGNED`. No automatic route/via completion or objective certification is allowed until a verified pin-level schematic is resolved. If that design information is unavailable, build the art/UI and safe scaffolding with explicit blocking validation and report what remains unresolved. Never invent physical pin numbers or silently map arbitrary connections as if electrically correct. The W157 logic endpoints exist, but pin-level mapping is also unassigned. Keep renders, diagnostic hints, VFX and the assembled showcase strictly non-authoritative.

## 1. Exact package contents and required integration

| Source | Exact contents |
| --- | --- |
| Modular geometry | 152 `.fbx` + 152 `.glb` |
| Completed visual reference | 1 `.fbx` + 1 `.glb` (reference only) |
| Installable parts | 118 individually placeable component rows |
| Supporting meshes | 34 board, diagnostic, fault, copper and via art rows |
| Copper tasks | 156 planned; all unspecified/not certifiable |
| Via tasks | 50 planned; all unspecified/not certifiable |
| Objectives | 6 source records |
| Faults | F1 standby leakage, F2 warm DDR4 BGA, F3 intermittent PCIe clock |
| Critical jumper | W157: U16 diagnostic monitor → U17 independent safety supervisor |
| Target PCB | 300 × 220 mm, 10 layers |
| Textures | Inherited starter PBR references + Level 108 2K silkscreen |
| Unity Editor helpers | `SparkL108BoardArtBuilder.cs`, `SparkL108StarterMaterials.cs` |

Package install root: `Assets/ProjectSpark/Level108/` (preserve relative folders). Place scripts requiring `UnityEditor` beneath an `Editor` folder or editor-only assembly. The package has a completed GLB with diagnostic markers and an inherited assembled FBX reference. Neither is playable. Its base board FBX uses approximately 285 × 205 mm legacy geometry, while the Level 108 concept requires 300 × 220 mm: correct physical geometry and inspect transforms rather than trusting the art-builder display scale as an electrical layout. Validate Unity FBX axis orientation, units, pivots, UVs, normals, collider bounds and material slots.

## 2. Working acceptance contract

**Completion:** Only the game's authoritative `LevelGamePlayManager` may certify six objectives after all required prerequisites, active fault states and fresh measurements pass. Completion is **0/6** at level entry. A visual component placed without a real footprint/contact remains uninstalled for purposes of certification. A route drawn on a board without an approved net and verified connectivity stays unqualified. The same applies to vias. The component, copper and via progress counters MUST derive from committed authoritative records, not UI button taps.

**Starting state:** `Installed = 0/118`, `Routes = 0/156`, `Vias = 0/50`, `W157 = DISCONNECTED`, `Objectives = 0/6`, diagnostic notebook empty, no repair signed off, power controlled off. Retain a separate assembled visual reference for previews only.

**Role of the player:** Investigate evidence before repairs, avoid unsafe energized continuity checks, use current-limited power when appropriate, reproduce warm and intermittent failures through deterministic heat/load conditions, select actual repair targets, and prove the fault no longer recurs under the same qualifying conditions. Simulator warnings must not reveal the exact guilty component before appropriate evidence is gathered.

## 3. Scene composition and visual structure

Create `L108_DFX16_GameplayRoot` with separate children `BoardArt`, `AuthorizedFootprints`, `ElectricalTopology`, `CopperTasks`, `ViaTasks`, `ToolHitTargets`, `SystemSimulation`, `EvidenceService`, `Faults`, `VirtualUI`, `RepairVFX`, `Objectives`, `Audio`, `DebugOnly`. Keep authoring/scaffolding isolated from runtime ownership. Every installed component uses a unique stable source ref, not array index. Each tool target maps to an approved electrical node/contact or named component location and has accessible touch/click collider interaction. No rooms, tables, chairs or physical meters are created; reuse the existing Project Spark large engineering room and world-space Canvas UI. Model-only art transformations cannot move electrical pin authority.

PCB stack has ten intended copper layers. Player layer view must be a non-destructive visual cross-section and must never alter real circuit connectivity. For fault overlays, display red/orange diagnostic markers only when appropriate evidence has been collected or the player's selected diagnostic mode warrants it; do not make F1–F3 visually obvious before investigation.

## 4. Electrical schematic reconciliation / hard validation gate

Before activating Objectives 2–6, create or import a reviewed schematic mapping each physical component pad (reference, manufacturer package, footprint pad number) to an electrical net; each requested route must have valid source and destination terminals, net ID, allowed PCB layer(s) and isolation checks. Each via must specify net, start/end layer, via type, position and drill/hole coverage. Resolve W157 physical pins on source U16 and destination U17 as actual schematic terminals. Validate no shorts between protected rails, valid connected components, solder condition, and that factory copper cannot bypass player tasks. Include a dedicated verification report with unresolved/missing/duplicate pin references and hard fail on any unknowns. **Never fabricate approved netlists to meet the target task counts.**

At present, all source routes have `net/source_ref/source_pin/destination_ref/destination_pin/layer = UNASSIGNED`; all via `net/start_layer/end_layer/via_type/position = UNASSIGNED`. Treat those as nonfunctional authoring placeholders. Use clear UI statuses `DESIGN DATA REQUIRED`, not `PASS` or `READY`. If design approval is unavailable, it is acceptable to demonstrate art/UI and isolated simulation test fixtures but not to claim a certifiable or fully playable Level 108.

## 5. Fault simulation contracts

### F1 — Leaky standby ceramic capacitor

**Source subsystem:** STANDBY_POWER  | **Model kind:** `LEAKAGE_AND_CURRENT_LIMITING`
**Source symptoms:** No power/abnormal standby current.  **Required evidence:** current measurement + isolated branch comparison.  **Approved repair:** replace leaking capacitor.
Implement a standby MLCC leakage-to-ground path with controllable leakage resistance/current as a function of condition. At initial power-on the current-limited supply may collapse the 3.3 V standby rail, creating no-power symptoms. Secondary CPU/POST failure must be a consequence, not an independent secret fault. Authorize branch isolation only at predefined service links; when isolated, the measured supply/branch current changes in a causally appropriate way. Do not use a simplistic UI flag that immediately sets 3.3 V to zero on any short. Require simulated power-off and discharge before continuity or component replacement. Re-test supply voltage, input current, branch current and sequencer readiness.

### F2 — Intermittent U3 BGA contact

**Source subsystem:** DDR4_MEMORY  | **Model kind:** `TEMPERATURE_DEPENDENT_CONTACT`
**Source symptoms:** warm memory training errors.  **Required evidence:** cold/warm lane test + BGA inspection.  **Approved repair:** controlled BGA rework or replacement.
Attach a temperature-dependent impedance/open-contact model to a specific approved U3 BGA net. During cold tests it may pass, while at a deterministic defined thermal condition warm memory training or data pattern comparison fails. Correlate failures with byte-lane/pin evidence only when electrical mapping is known. Thermal VFX is non-authoritative. Require cold-versus-warm comparative evidence and BGA X-ray/contact inspection. Permit qualified virtual rework/replacement with controlled preheat and safety gating; after repair perform both cold and warm qualification, including repeated transitions.

### F3 — Intermittent PCIe reference-clock contact

**Source subsystem:** PCIE_CLOCK  | **Model kind:** `THERMALLY_INTERMITTENT_CLOCK`
**Source symptoms:** endpoint disappears / L0 to Recovery.  **Required evidence:** source clock compared with endpoint waveform.  **Approved repair:** clock solder contact restoration.
Model a marginal clock-buffer solder/contact affecting the PCIe reference-clock branch; source oscillator may be healthy while endpoint-side differential clock intermittently degrades with the defined stress/thermal schedule. The endpoint reports Recovery/failed retrain/disappearance only as a downstream effect of that clock impairment. Require simultaneous source-vs-endpoint clock captures, not just a generic PCIe error code. Correct repair restores contact; re-test link startup, stable L0 operation and sustained transfers over an entire deterministic stress window.

## 6. Diagnosing without spoilers: evidence notebook and hypothesis lifecycle

Build a persistent per-level investigation notebook (saved by the project’s existing save approach): hypothesis ID, tested subsystem, tool, target terminal/net, test configuration, power state, board temperature, load state, simulated time, measured value with units, outcome, screenshot/trace reference if available, and player conclusion. Values must originate from the shared simulation. Capture first vs last successful POST state, and differentiate *symptom observed* from *root cause validated*. No direct display of F1/F2/F3 component identity until measurements support it. Support contradictory evidence and hypothesis revision. Expose `NotTested`, `Suspected`, `Supported`, `Confirmed`, `Repaired`, `Requalified` states; evidence and repairs are separately tracked. Persist the minimum evidence needed for Objective 4 while not forcing arbitrary tool-use quotas.

Where the simulator is intentionally simplified, surface its assumptions (current limit, selected thermal thresholds, clock quality thresholds, repeat duration) in engineering configuration assets. Never present educational signal estimates as device-manufacturer certified measurements. Deterministic seeded behavior must reproduce on reruns and automated tests; no uncontrolled per-frame random fault triggers.

## 7. World-Space Canvas virtual instruments

Reuse and extend Project Spark’s existing virtual PCB UI. Instruments: virtual multimeter, current-limited power source, oscilloscope, POST analyzer, DDR4 trainer, BGA X-ray, thermal scan, PCIe link analyzer, clock comparison, net tracer, ten-layer viewer, safe subsystem isolator, evidence notebook, solder/desolder, controlled BGA rework, component replacement and calibration. Use only click/touch to arm tool and select real board targets. Differentiate powered voltage/current captures from power-off resistance/continuity captures; block the latter on energized circuits. Pointer raycast and world-space canvas interactions must work with mouse and touch without camera/UI event conflict. Instrument data refreshes from simulation snapshots with timestamps and stale-data indicators.

Localized VFX should include subtle contact probe glow, short solder sparks, temperature shimmer, scan pulses and separate failure/repaired states, optimized for Unity real-time playback. VFX cannot change the circuit until repair authorization commits. SFX and haptics optional. UI's dynamic measurements and labels must not claim `OPEN` or `FIXED` without authoritative evidence.

## 8. Boot and subsystem dependency engine

Implement dependency chain `STANDBY → MAIN_POWER_REQUEST → VRM_AND_RESET → MCU/SOC_INIT → SPI_FIRMWARE → DDR4_TRAINING → PCIe_LINK → BOOT_STORAGE → SYSTEM_READY`, allowing the actual platform to choose optional peripherals where applicable; SFX/DFX game objectives may require PCIe diagnostic endpoint as a certification condition. The first blocking stage is derived from actual readiness and relevant faults. For F1 no power/reset release, F2 warm memory may fail while healthy cold boot succeeds, and F3 PCIe may drop after successful POST. Do not incorrectly claim that all three failures necessarily happen simultaneously at level entry. Controlled commissioning should expose them sequentially as appropriate to power state and environmental conditions.

## 9. Detailed objective authority

### Objective 01
**Exact source requirement:** Assemble 118 components.
**Implementation gate:** Commit 118 correct footprint/component placements with correct orientation and solder contacts. Never count placeholders or the assembled showcase.

### Objective 02
**Exact source requirement:** Complete 156 certified copper tasks and 50 vias.
**Implementation gate:** Require all approved 156 player-route task IDs and 50 approved via IDs; enforce pin-level netlist, electrical continuity and no shorts. Install W157 only for initial commissioning; no shortcut by visual wire overlay.

### Objective 03
**Exact source requirement:** Capture initial diagnostic baseline.
**Implementation gate:** Establish valid diagnostic baseline: supply and power limit measurement, reset/clock status, preliminary POST and dated notebook evidence with stated conclusions.

### Objective 04
**Exact source requirement:** Prove root causes using evidence and isolation.
**Implementation gate:** Reproduce all three conditional faults at the right test conditions; support actual isolation evidence and root-cause confirmation without omniscient highlighting.

### Objective 05
**Exact source requirement:** Repair F1-F3 and requalify cold/warm/PCIe.
**Implementation gate:** Confirm F1–F3 with approved repair authority, then repeat cold boot, warm DDR4 stress and PCIe stability/transfer qualification under the same windows.

### Objective 06
**Exact source requirement:** Disconnect/restore W157; fresh safety qualification.
**Implementation gate:** Disconnect actual W157 link, assert independent inhibition, reconnect true terminals, collect fresh complete diagnostic and boot evidence, explicitly reset latch, then certify 6/6 through LevelGamePlayManager.

**Revocation and history:** Preserve historical objective success where gameplay design calls for it, but invalidate stale measurement certificates after new repair, topology or safety state changes. A past successful cold boot cannot qualify the board after W157 disconnection or a subsequent fault. Only the corresponding latest complete evidence bundle can pass Objective 6.

## 10. W157 independent safety lifecycle

Source-defined logical jumper:

```text
L108_SystemDiagnosticMonitor_U16.TRUSTED_DIAGNOSTIC_READY_OUT
      │
      ├── W157 REMOVABLE FEEDBACK JUMPER
      │
L108_IndependentSafetySupervisor_U17.TRUSTED_DIAGNOSTIC_READY_IN
```

Both U16 and U17 occur in the 118-component manifest, but no physical pin identifiers are given. Map only after schematic review. `W157` must carry dynamic, freshness-valid diagnostic health, **not** direct CPU/standby power, clock or a permanently HIGH bit. Sequence: (1) start commissioned/healthy, (2) open physical jumper graph edge under supervised test, (3) independently latch protected test inhibition, (4) maintain physical power states according to the solver, (5) close actual jumper edge, (6) acquire new meaningful health transitions and complete all applicable board requalification, (7) explicit safe fault reset, (8) final independent proof and `6/6`. No automatic safety reset on reconnection and no UI-only state toggles.

## 11. Persistence, performance and safe implementation

Use ScriptableObject authoring for approved stable definitions and runtime structs/classes for mutable simulation/evidence. Work with the existing save system and avoid serialization of scene references. Stable IDs must survive prefab regeneration. Avoid `FindFirstObjectByType` per frame, event reentrancy, cross-level singleton leakage and memory allocation inside sampling loops. Support a deterministic fixed-step simulation (as appropriate) separate from frame-rate visuals, with bounded trace history and downsampled UI plots. Provide runtime diagnostics for counts, invalid pins, shorted rails, stale measurements, failed tool authority and missing dependencies. No runtime Editor calls. All new public interfaces and namespaces should follow existing Project Spark conventions; isolate new Level 108 scripts behind level-specific namespaces/assemblies when possible. Keep side effects localized.

## 12. One-click builder, migration and verification

**Existing menus in the source kit** (art-only, NOT full gameplay):

`Tools/Project Spark/Levels/108/1. Build EMPTY DFX-16 Board`

`Tools/Project Spark/Levels/108/2. Generate PBR Starter Materials`

**Requested gameplay builder:** `Tools/Project Spark/Levels/Create Level 108`.

The complete builder should: validate package/scene assets; check netlist reconciliation; construct a fresh, undoable Level 108 root (or update an existing tagged root idempotently); bind one authoritative topology to physical target colliders; create component instances as *uninstalled shelf/placement entries*; initialize 156/50 tasks as missing; connect virtual tools; configure the safe power/POST/fault simulator; initialize 6 objective gates; add W157 as disconnected; create a reference-only preview separately; validate and save prefab/scene artifacts only after no hard errors. If input data remains UNASSIGNED, make a clearly marked **ART/DIAGNOSTIC PREVIEW ONLY** build and block functional certification, rather than filling in arbitrary terminals.

Use stable Editor Undo and `AssetDatabase` calls only inside editor assembly. Do not auto-edit existing levels, project settings, render pipeline or user scenes without a scoped need. After import run actual Unity compilation and automated tests, then Play Mode and touch/mouse UI checks. State exactly which steps are verified versus untested.

## 13. Implementation deliverables

Deliver: (a) readable Unity C# gameplay implementation and integrations, (b) DFX-16 scene/prefab with empty playable state and separate reference preview, (c) validated ScriptableObject/csv-driven component data, (d) verified and approved netlist or **explicit blocking report** if absent, (e) deterministic F1/F2/F3 fault simulations and repair authority, (f) instrument UI plus evidence notebook, (g) W157 supervisor and safety state machine, (h) objective management via real LevelGamePlayManager, (i) one-click editor builder, (j) unit/integration/Play Mode acceptance tests, (k) onboarding/readme and runbook, (l) transparent remaining gaps and source-data reconciliation decisions.

## 14. 100 proposed engineering acceptance tests

### Source / imports

- [ ] **T001** — Correct source ZIP and all relative folders import without destructive renaming.
- [ ] **T002** — 152 modular FBX models resolvable.
- [ ] **T003** — 152 modular GLBs resolvable.
- [ ] **T004** — assembled visual reference FBX/GLB treated as non-authoritative.
- [ ] **T005** — 118 component records parse with unique stable references.
- [ ] **T006** — 34 structural models not counted as installable.
- [ ] **T007** — PCB 300×220 mm target vs inherited 285×205 mm detected and flagged.
- [ ] **T008** — ten-layer art viewer corresponds to validated layer stack.
- [ ] **T009** — Unity units, axis and pivot alignment inspected.
- [ ] **T010** — basic PBR shader compatibility and graceful missing-map warning.

### Electrical authority

- [ ] **T011** — All 156 source routes are initially NOT_CERTIFIABLE.
- [ ] **T012** — All 50 source vias are initially NOT_CERTIFIABLE.
- [ ] **T013** — missing source route pad prevents electrical certification.
- [ ] **T014** — missing destination route pad prevents electrical certification.
- [ ] **T015** — missing net ID prevents route certification.
- [ ] **T016** — wrong copper layer prevents route certification.
- [ ] **T017** — unsupported via span rejected.
- [ ] **T018** — cross-net short detected.
- [ ] **T019** — unverified visual reference never closes live connectivity.
- [ ] **T020** — approved topology change invalidates stale evidence.

### Gameplay assembly

- [ ] **T021** — initial component count is 0/118.
- [ ] **T022** — initial route count is 0/156.
- [ ] **T023** — initial via count is 0/50.
- [ ] **T024** — W157 initially disconnected.
- [ ] **T025** — initial objectives are 0/6.
- [ ] **T026** — correct part installation creates actual footprint contacts.
- [ ] **T027** — wrong part/orientation rejected.
- [ ] **T028** — incorrect solder contact inhibits certification.
- [ ] **T029** — shelf/recycle existing rules preserved.
- [ ] **T030** — level reload restores deterministic assembly state.

### F1 standby fault

- [ ] **T031** — F1 increases modeled standby current.
- [ ] **T032** — current limiting lowers rail under defined load.
- [ ] **T033** — F1 creates downstream power/reset inhibition.
- [ ] **T034** — off-board source measurements distinguish supply from load.
- [ ] **T035** — authorized isolation changes downstream current.
- [ ] **T036** — energized resistance probing blocked.
- [ ] **T037** — F1 replacement removes modeled leakage.
- [ ] **T038** — post-repair supply qualification succeeds.
- [ ] **T039** — false repair target leaves F1 active.
- [ ] **T040** — F1 report contains current and isolation evidence.

### F2 BGA thermal fault

- [ ] **T041** — U3 fault inactive or limited under cold model conditions.
- [ ] **T042** — heating transitions contact to faulty state deterministically.
- [ ] **T043** — warm DDR4 training/data test exposes failure.
- [ ] **T044** — temperature readout matches simulated thermal model.
- [ ] **T045** — cold/warm evidence retained separately.
- [ ] **T046** — BGA X-ray requires selected supported target.
- [ ] **T047** — ordinary surface solder action cannot repair hidden BGA.
- [ ] **T048** — authorized BGA rework updates contact model.
- [ ] **T049** — warm qualification passes after legitimate repair.
- [ ] **T050** — reloading deterministic seed reproduces failure.

### F3 PCIe clock fault

- [ ] **T051** — healthy source clock remains measurable.
- [ ] **T052** — affected endpoint clock degrades during fault.
- [ ] **T053** — link transitions L0 to Recovery as appropriate.
- [ ] **T054** — unrelated PCIe lane contacts remain unchanged.
- [ ] **T055** — clock vs link data distinguishes root cause.
- [ ] **T056** — thermal stress reproducibly triggers condition.
- [ ] **T057** — wrong connector repair does not fix clock contact.
- [ ] **T058** — authorized clock solder repair fixes physical link.
- [ ] **T059** — retrain achieves required x4 operational condition.
- [ ] **T060** — sustained endpoint transfer test passes after repair.

### Virtual diagnostics UI

- [ ] **T061** — mouse board selection targets exact contact.
- [ ] **T062** — touch board selection targets exact contact.
- [ ] **T063** — World-Space Canvas input does not block tool interaction.
- [ ] **T064** — powered voltage tool reads live solver value.
- [ ] **T065** — de-energized continuity tool reads valid graph.
- [ ] **T066** — power-off rule enforced for resistance mode.
- [ ] **T067** — waveform snapshots include timestamp/settings.
- [ ] **T068** — evidence notebook logs test point and result.
- [ ] **T069** — untested regions remain unknown instead of green.
- [ ] **T070** — VFX animation never writes repair success.

### Objectives, boot, safety

- [ ] **T071** — Objective 1 requires 118 verified parts.
- [ ] **T072** — Objective 2 blocked on unresolved netlist.
- [ ] **T073** — Objective 3 requires real baseline evidence.
- [ ] **T074** — Objective 4 requires fault-specific root-cause evidence.
- [ ] **T075** — Objective 5 requires three real repairs and requalification.
- [ ] **T076** — Objective 6 blocked until physical W157 is mapped.
- [ ] **T077** — W157 unplug forces independent protected-operation inhibition.
- [ ] **T078** — W157 reconnect without reset remains inhibited.
- [ ] **T079** — fresh diagnostics and explicit reset permit final certification.
- [ ] **T080** — only LevelGamePlayManager can authorize 6/6.

### Nonregression / lifecycle

- [ ] **T081** — Levels 1–107 remain loadable.
- [ ] **T082** — previous scene objects and prefabs unmodified.
- [ ] **T083** — level builder safe to run twice.
- [ ] **T084** — old Level108 root is not duplicated on rebuild.
- [ ] **T085** — missing FBX import gives actionable error.
- [ ] **T086** — missing PBR texture gives actionable warning.
- [ ] **T087** — no UnityEditor assembly references in runtime.
- [ ] **T088** — objective reset/reload never fabricates evidence.
- [ ] **T089** — rapid tool switching cannot race repair state.
- [ ] **T090** — diagnostic logs include deterministic simulation seed.

### Final validation / documentation

- [ ] **T091** — Unity scripts compile without C# errors.
- [ ] **T092** — EditMode tests verify core topology and safeguards.
- [ ] **T093** — PlayMode tests reproduce and repair F1.
- [ ] **T094** — PlayMode tests reproduce and repair F2.
- [ ] **T095** — PlayMode tests reproduce and repair F3.
- [ ] **T096** — safety/W157 manual flow passes after pin verification.
- [ ] **T097** — cold/warm power-up and restart checks pass.
- [ ] **T098** — user input and responsive UI tested at gameplay resolution.
- [ ] **T099** — README identifies source-derived data vs unverified assumptions.
- [ ] **T100** — final handoff lists confirmed tests and outstanding blockers.

## 15. Source annex A — exact 152-model manifest

These are **source records**, not newly verified electrical components. `source_geometry_asset` documents inherited procedural geometry. Import names and positions faithfully; do not infer nets from this table.
| Model | Type | Source geometry | X mm | Y mm | Installable |
| --- | --- | --- | --- | --- | --- |
| 001_L108_U1 | CPU | 001_L107_U1 | 0.0 | 0.0 | True |
| 002_L108_U2 | DDR | 002_L107_U2 | 87.0 | 50.0 | True |
| 003_L108_U3 | DDR | 003_L107_U3 | 87.0 | 23.0 | True |
| 004_L108_IC1 | IC | 004_L107_IC1 | -51.0 | 52.0 | True |
| 005_L108_U5_StandbyPMIC | PMIC | 005_L107_U5_RecoveryController | -85.0 | -47.0 | True |
| 006_L108_U6_EvidenceController | DIAGNOSTIC_CONTROLLER | 006_L107_U6_PrimarySPIFlash | -58.0 | -47.0 | True |
| 007_L108_U7_PrimarySPIFlash | SPI_FLASH | 007_L107_U7_RecoverySPIFlash | -24.0 | -49.0 | True |
| 008_L108_IC5 | IC | 008_L107_IC5 | 106.0 | 65.0 | True |
| 009_L108_U10_BootStorage | EMMC | 009_L107_U7 | 46.0 | -57.0 | True |
| 010_L108_Y1 | OSC | 010_L107_Y1 | -33.0 | 75.0 | True |
| 011_L108_U6 | FLASH | 011_L107_U6 | -50.0 | -55.0 | True |
| 012_L108_U11_EmbeddedController | EC | 012_L107_U5 | -105.0 | -57.0 | True |
| 013_L108_U8 | IO | 013_L107_U8 | 68.0 | -26.0 | True |
| 014_L108_U9_PCIeClockBuffer | PCIE_CLOCK_BUFFER | 014_L107_U9 | 85.0 | -48.0 | True |
| 015_L108_REG1 | REG | 015_L107_REG1 | -76.0 | -17.0 | True |
| 016_L108_REG2 | REG | 016_L107_REG2 | 68.0 | -71.0 | True |
| 017_L108_U15 | SAFETY | 017_L107_U15 | 75.0 | -35.0 | True |
| 018_L108_TEMP1 | TEMP | 018_L107_TEMP1 | -25.0 | 71.0 | True |
| 019_L108_RES1 | RES | 019_L107_RES1 | 116.84 | -32.698 | True |
| 020_L108_RES2 | RES | 020_L107_RES2 | 48.49 | -55.554 | True |
| 021_L108_RES3 | RES | 021_L107_RES3 | 111.199 | 70.863 | True |
| 022_L108_RES4 | RES | 022_L107_RES4 | -67.594 | -81.658 | True |
| 023_L108_RES5 | RES | 023_L107_RES5 | 4.154 | 54.934 | True |
| 024_L108_RES6 | RES | 024_L107_RES6 | 62.017 | -34.962 | True |
| 025_L108_RES7 | RES | 025_L107_RES7 | -14.08 | 56.828 | True |
| 026_L108_RES8 | RES | 026_L107_RES8 | -53.466 | 71.105 | True |
| 027_L108_RES9 | RES | 027_L107_RES9 | 2.736 | 82.905 | True |
| 028_L108_RES10 | RES | 028_L107_RES10 | -7.85 | 43.487 | True |
| 029_L108_RES11 | RES | 029_L107_RES11 | -119.747 | 1.571 | True |
| 030_L108_RES12 | RES | 030_L107_RES12 | 91.23 | -34.954 | True |
| 031_L108_RES13 | RES | 031_L107_RES13 | 101.928 | 6.888 | True |
| 032_L108_RES14 | RES | 032_L107_RES14 | -66.132 | 62.698 | True |
| 033_L108_RES15 | RES | 033_L107_RES15 | -115.959 | -7.956 | True |
| 034_L108_RES16 | RES | 034_L107_RES16 | -33.139 | -41.004 | True |
| 035_L108_RES17 | RES | 035_L107_RES17 | -6.409 | 82.141 | True |
| 036_L108_RES18 | RES | 036_L107_RES18 | 92.559 | -22.654 | True |
| 037_L108_RES19 | RES | 037_L107_RES19 | -110.948 | 41.087 | True |
| 038_L108_RES20 | RES | 038_L107_RES20 | -68.392 | -63.593 | True |
| 039_L108_RES21 | RES | 039_L107_RES21 | 13.774 | -53.781 | True |
| 040_L108_RES22 | RES | 040_L107_RES22 | -58.873 | -11.738 | True |
| 041_L108_RES23 | RES | 041_L107_RES23 | -109.21 | -70.07 | True |
| 042_L108_RES24 | RES | 042_L107_RES24 | 30.614 | 19.717 | True |
| 043_L108_RES25 | RES | 043_L107_RES25 | 55.544 | 60.566 | True |
| 044_L108_RES26 | RES | 044_L107_RES26 | -79.121 | -47.915 | True |
| 045_L108_RES27 | RES | 045_L107_RES27 | -46.512 | 46.277 | True |
| 046_L108_RES28 | RES | 046_L107_RES28 | 118.315 | 17.856 | True |
| 047_L108_CAP1 | CAP | 047_L107_CAP1 | -115.872 | -61.12 | True |
| 048_L108_CAP2 | CAP | 048_L107_CAP2 | 43.533 | 5.789 | True |
| 049_L108_CAP3 | CAP | 049_L107_CAP3 | -7.903 | 50.467 | True |
| 050_L108_CAP4 | CAP | 050_L107_CAP4 | -16.058 | 43.553 | True |
| 051_L108_CAP5 | CAP | 051_L107_CAP5 | 110.695 | -80.095 | True |
| 052_L108_CAP6 | CAP | 052_L107_CAP6 | 34.366 | 8.669 | True |
| 053_L108_CAP7 | CAP | 053_L107_CAP7 | -101.293 | -27.657 | True |
| 054_L108_CAP8 | CAP | 054_L107_CAP8 | 34.781 | 76.07 | True |
| 055_L108_CAP9 | CAP | 055_L107_CAP9 | 33.523 | -57.739 | True |
| 056_L108_CAP10 | CAP | 056_L107_CAP10 | 35.601 | 53.221 | True |
| 057_L108_CAP11 | CAP | 057_L107_CAP11 | 37.758 | 65.638 | True |
| 058_L108_CAP12 | CAP | 058_L107_CAP12 | -55.554 | -34.148 | True |
| 059_L108_CAP13 | CAP | 059_L107_CAP13 | -45.387 | 79.482 | True |
| 060_L108_CAP14 | CAP | 060_L107_CAP14 | 6.747 | -42.045 | True |
| 061_L108_CAP15 | CAP | 061_L107_CAP15 | 43.548 | -30.754 | True |
| 062_L108_CAP16 | CAP | 062_L107_CAP16 | -40.076 | -44.856 | True |
| 063_L108_CAP17 | CAP | 063_L107_CAP17 | -116.986 | -25.441 | True |
| 064_L108_CAP18 | CAP | 064_L107_CAP18 | -96.274 | -52.99 | True |
| 065_L108_CAP19 | CAP | 065_L107_CAP19 | 61.452 | 11.852 | True |
| 066_L108_CAP20 | CAP | 066_L107_CAP20 | -20.224 | -58.869 | True |
| 067_L108_CAP21 | CAP | 067_L107_CAP21 | 33.773 | 28.953 | True |
| 068_L108_CAP22 | CAP | 068_L107_CAP22 | 110.104 | -7.008 | True |
| 069_L108_CAP23 | CAP | 069_L107_CAP23 | -85.536 | -61.108 | True |
| 070_L108_CAP24 | CAP | 070_L107_CAP24 | 113.783 | 32.553 | True |
| 071_L108_CAP25 | CAP | 071_L107_CAP25 | -69.696 | -3.355 | True |
| 072_L108_CAP26 | CAP | 072_L107_CAP26 | 64.014 | 31.889 | True |
| 073_L108_CAP27 | CAP | 073_L107_CAP27 | -96.5 | -21.431 | True |
| 074_L108_CAP28 | CAP | 074_L107_CAP28 | 68.81 | -57.279 | True |
| 075_L108_CAP29 | CAP | 075_L107_CAP29 | 3.791 | 76.72 | True |
| 076_L108_CAP30 | CAP | 076_L107_CAP30 | -7.891 | -44.846 | True |
| 077_L108_INDUCTOR1 | INDUCTOR | 077_L107_INDUCTOR1 | -95.0 | 67.0 | True |
| 078_L108_INDUCTOR2 | INDUCTOR | 078_L107_INDUCTOR2 | -95.0 | 47.0 | True |
| 079_L108_INDUCTOR3 | INDUCTOR | 079_L107_INDUCTOR3 | -95.0 | 27.0 | True |
| 080_L108_INDUCTOR4 | INDUCTOR | 080_L107_INDUCTOR4 | -95.0 | 7.0 | True |
| 081_L108_INDUCTOR5 | INDUCTOR | 081_L107_INDUCTOR5 | -61.0 | 72.0 | True |
| 082_L108_INDUCTOR6 | INDUCTOR | 082_L107_INDUCTOR6 | -61.0 | 52.0 | True |
| 083_L108_MOSFET1 | MOSFET | 083_L107_MOSFET1 | -115.0 | 71.0 | True |
| 084_L108_MOSFET2 | MOSFET | 084_L107_MOSFET2 | -114.0 | 55.0 | True |
| 085_L108_MOSFET3 | MOSFET | 085_L107_MOSFET3 | -115.0 | 36.0 | True |
| 086_L108_MOSFET4 | MOSFET | 086_L107_MOSFET4 | -114.0 | 18.0 | True |
| 087_L108_MOSFET5 | MOSFET | 087_L107_MOSFET5 | -76.0 | 81.0 | True |
| 088_L108_MOSFET6 | MOSFET | 088_L107_MOSFET6 | -48.0 | 81.0 | True |
| 089_L108_CONNECTOR1 | CONNECTOR | 089_L107_CONNECTOR1 | -121.0 | 45.0 | True |
| 090_L108_CONNECTOR2 | CONNECTOR | 090_L107_CONNECTOR2 | -122.0 | -67.0 | True |
| 091_L108_CONNECTOR3 | CONNECTOR | 091_L107_CONNECTOR3 | 122.0 | 81.0 | True |
| 092_L108_CONNECTOR4 | CONNECTOR | 092_L107_CONNECTOR4 | 124.0 | 11.0 | True |
| 093_L108_J1 | PCIE | 093_L107_J1 | 18.0 | -79.0 | True |
| 094_L108_LED1 | LED | 094_L107_LED1 | 55.0 | -50.0 | True |
| 095_L108_LED2 | LED | 095_L107_LED2 | 68.0 | -50.0 | True |
| 096_L108_LED3 | LED | 096_L107_LED3 | 81.0 | -50.0 | True |
| 097_L108_LED4 | LED | 097_L107_LED4 | 94.0 | -50.0 | True |
| 098_L108_SWITCH1 | SWITCH | 098_L107_SWITCH1 | -117.0 | -78.0 | True |
| 099_L108_SWITCH2 | SWITCH | 099_L107_SWITCH2 | -78.0 | -77.0 | True |
| 100_L108_SWITCH3 | SWITCH | 100_L107_SWITCH3 | 126.0 | -81.0 | True |
| 101_L108_DIODE1 | DIODE | 101_L107_DIODE1 | 68.1 | 56.239 | True |
| 102_L108_DIODE2 | DIODE | 102_L107_DIODE2 | 83.001 | -12.68 | True |
| 103_L108_DIODE3 | DIODE | 103_L107_DIODE3 | -79.132 | -32.474 | True |
| 104_L108_FERRITE1 | FERRITE | 104_L107_FERRITE1 | 95.747 | -7.488 | True |
| 105_L108_FERRITE2 | FERRITE | 105_L107_FERRITE2 | 108.766 | -43.932 | True |
| 106_L108_W157_DiagnosticTrustJumper | JUMPER | 106_L107_W149 | 123.0 | -53.0 | True |
| 107_L108_S107_FlashBusArbiter | FLASH_ARBITER | 107_L107_S107_FlashBusArbiter | -57.0 | -34.0 | True |
| 108_L108_S108_RecoveryInputPullup | RECOVERY_INPUT_RESISTOR | 108_L107_S108_RecoveryInputPullup | 47.0 | -54.0 | True |
| 109_L108_S109_RecoveryButton | RECOVERY_SWITCH | 109_L107_S109_RecoveryButton | 59.0 | -49.0 | True |
| 110_L108_S110_WriteProtectSwitch | FLASH_WRITE_PROTECT | 110_L107_S110_WriteProtectSwitch | 80.0 | -49.0 | True |
| 111_L108_S111_FirmwareServiceHeader | SERVICE_HEADER | 111_L107_S111_FirmwareServiceHeader | -67.0 | -72.0 | True |
| 112_L108_S112_BootPolicySelector | BOOT_SELECTOR | 112_L107_S112_BootPolicySelector | 82.0 | -72.0 | True |
| 113_L108_TP_STBY_CurrentSense | CURRENT_SENSE | 073_L107_CAP27 | -119.0 | -68.0 | True |
| 114_L108_J_DIAG_IsolationLink | ISOLATION_LINK | 093_L107_J1 | -82.0 | -76.0 | True |
| 115_L108_TP_DDR_ThermalPad | THERMAL_TEST_PAD | 130_L107_Memory_TraceDetail_1 | 70.0 | 66.0 | True |
| 116_L108_J_CLK_SourceProbe | CLOCK_PROBE_HEADER | 092_L107_CONNECTOR4 | 105.0 | -69.0 | True |
| 117_L108_U16_DiagnosticMonitor | DIAGNOSTIC_MONITOR | 008_L107_IC5 | -61.0 | -72.0 | True |
| 118_L108_U17_IndependentSafety | SAFETY_SUPERVISOR | 017_L107_U15 | 76.0 | -76.0 | True |
| 119_L108_DFX16_BarePCB | BOARD | 113_L107_PFX15_BarePCB | 0.0 | 0.0 | False |
| 120_L108_Copper_L01_Reference | REFERENCE_LAYER | 114_L107_Copper_L01_Reference | 0.0 | 0.0 | False |
| 121_L108_Copper_L02_Reference | REFERENCE_LAYER | 115_L107_Copper_L02_Reference | 0.0 | 0.0 | False |
| 122_L108_Copper_L03_Reference | REFERENCE_LAYER | 116_L107_Copper_L03_Reference | 0.0 | 0.0 | False |
| 123_L108_Copper_L04_Reference | REFERENCE_LAYER | 117_L107_Copper_L04_Reference | 0.0 | 0.0 | False |
| 124_L108_Copper_L05_Reference | REFERENCE_LAYER | 118_L107_Copper_L05_Reference | 0.0 | 0.0 | False |
| 125_L108_Copper_L06_Reference | REFERENCE_LAYER | 119_L107_Copper_L06_Reference | 0.0 | 0.0 | False |
| 126_L108_Copper_L07_Reference | REFERENCE_LAYER | 120_L107_Copper_L07_Reference | 0.0 | 0.0 | False |
| 127_L108_Copper_L08_Reference | REFERENCE_LAYER | 121_L107_Copper_L08_Reference | 0.0 | 0.0 | False |
| 128_L108_Copper_L09_Reference | REFERENCE_LAYER | 122_L107_Copper_L09_Reference | 0.0 | 0.0 | False |
| 129_L108_Copper_L10_Reference | REFERENCE_LAYER | 123_L107_Copper_L10_Reference | 0.0 | 0.0 | False |
| 130_L108_CPU_Socket_Frame | SOCKET | 124_L107_CPU_Socket_Frame | 0.0 | 0.0 | False |
| 131_L108_CPU_HeatSpreader | CPU_HARDWARE | 125_L107_CPU_HeatSpreader | 0.0 | 0.0 | False |
| 132_L108_PCIe_Socket_Detail | CONNECTOR_DETAIL | 126_L107_PCIe_Socket_Detail | 18.0 | -79.0 | False |
| 133_L108_DDR4_Footprint_U2 | BGA_FOOTPRINT | 127_L107_DDR4_Footprint_U2 | 87.0 | 50.0 | False |
| 134_L108_DDR4_Footprint_U3 | BGA_FOOTPRINT | 128_L107_DDR4_Footprint_U3 | 87.0 | 23.0 | False |
| 135_L108_SPI_Flash_Footprint | FLASH_FOOTPRINT | 129_L107_SPI_Flash_Footprint | -50.0 | -55.0 | False |
| 136_L108_Memory_TraceDetail_1 | DECORATIVE | 130_L107_Memory_TraceDetail_1 | 0.0 | 0.0 | False |
| 137_L108_Memory_TraceDetail_2 | DECORATIVE | 131_L107_Memory_TraceDetail_2 | 0.0 | 0.0 | False |
| 138_L108_Memory_TraceDetail_3 | DECORATIVE | 132_L107_Memory_TraceDetail_3 | 0.0 | 0.0 | False |
| 139_L108_GroundVia | VIA_REFERENCE | 133_L107_GroundVia | 0.0 | 0.0 | False |
| 140_L108_PowerVia | VIA_REFERENCE | 134_L107_PowerVia | 0.0 | 0.0 | False |
| 141_L108_SignalVia | VIA_REFERENCE | 135_L107_SignalVia | 0.0 | 0.0 | False |
| 142_L108_CPU_PowerGood_Fault | FAULT_OVERLAY | 136_L107_CPU_PowerGood_Fault | 0.0 | 0.0 | False |
| 143_L108_SPI_SolderBridge_Fault | FAULT_OVERLAY | 137_L107_SPI_SolderBridge_Fault | 0.0 | 0.0 | False |
| 144_L108_Storage_Contact_Fault | FAULT_OVERLAY | 138_L107_Storage_Contact_Fault | 0.0 | 0.0 | False |
| 145_L108_12V_Input_TestPad | TESTPAD | 139_L107_12V_Input_TestPad | 0.0 | 0.0 | False |
| 146_L108_VCORE_TestPad | TESTPAD | 140_L107_VCORE_TestPad | 0.0 | 0.0 | False |
| 147_L108_DDR_VDD_TestPad | TESTPAD | 141_L107_DDR_VDD_TestPad | 0.0 | 0.0 | False |
| 148_L108_W157_Safety_TestPad | TESTPAD | 142_L107_W149_Safety_TestPad | 0.0 | 0.0 | False |
| 149_L108_DualFlash_ServicePad_Reference | TEST_PAD_REFERENCE | 143_L107_DualFlash_ServicePad_Reference | -9.0 | -70.0 | False |
| 150_L108_RecoveryBoot_TraceReference | COPPER_REFERENCE | 144_L107_RecoveryBoot_TraceReference | 35.0 | -63.0 | False |
| 151_L108_StandbyLeak_ThermalOverlay | FAULT_OVERLAY | 143_L107_DualFlash_ServicePad_Reference | -119.0 | -68.0 | False |
| 152_L108_PCIeClock_IntermittentOverlay | FAULT_OVERLAY | 144_L107_RecoveryBoot_TraceReference | 105.0 | -69.0 | False |

## 16. Source annex B — exact 118 installable component records

The source uses placeholder footprints; every `electrical_netlist` field is `NOT_CERTIFIABLE`.
| Model | Ref | Part type | X mm | Y mm | Footprint status |
| --- | --- | --- | --- | --- | --- |
| 001_L108_U1 | U1 | CPU | 0.0 | 0.0 | ART_PLACEHOLDER |
| 002_L108_U2 | U2 | DDR | 87.0 | 50.0 | ART_PLACEHOLDER |
| 003_L108_U3 | U3 | DDR | 87.0 | 23.0 | ART_PLACEHOLDER |
| 004_L108_IC1 | IC1 | IC | -51.0 | 52.0 | ART_PLACEHOLDER |
| 005_L108_U5_StandbyPMIC | U5 | PMIC | -85.0 | -47.0 | ART_PLACEHOLDER |
| 006_L108_U6_EvidenceController | U6 | DIAGNOSTIC_CONTROLLER | -58.0 | -47.0 | ART_PLACEHOLDER |
| 007_L108_U7_PrimarySPIFlash | U7 | SPI_FLASH | -24.0 | -49.0 | ART_PLACEHOLDER |
| 008_L108_IC5 | IC5 | IC | 106.0 | 65.0 | ART_PLACEHOLDER |
| 009_L108_U10_BootStorage | U10 | EMMC | 46.0 | -57.0 | ART_PLACEHOLDER |
| 010_L108_Y1 | Y1 | OSC | -33.0 | 75.0 | ART_PLACEHOLDER |
| 011_L108_U6 | U6 | FLASH | -50.0 | -55.0 | ART_PLACEHOLDER |
| 012_L108_U11_EmbeddedController | U11 | EC | -105.0 | -57.0 | ART_PLACEHOLDER |
| 013_L108_U8 | U8 | IO | 68.0 | -26.0 | ART_PLACEHOLDER |
| 014_L108_U9_PCIeClockBuffer | U9 | PCIE_CLOCK_BUFFER | 85.0 | -48.0 | ART_PLACEHOLDER |
| 015_L108_REG1 | REG1 | REG | -76.0 | -17.0 | ART_PLACEHOLDER |
| 016_L108_REG2 | REG2 | REG | 68.0 | -71.0 | ART_PLACEHOLDER |
| 017_L108_U15 | U15 | SAFETY | 75.0 | -35.0 | ART_PLACEHOLDER |
| 018_L108_TEMP1 | TEMP1 | TEMP | -25.0 | 71.0 | ART_PLACEHOLDER |
| 019_L108_RES1 | RES1 | RES | 116.84 | -32.698 | ART_PLACEHOLDER |
| 020_L108_RES2 | RES2 | RES | 48.49 | -55.554 | ART_PLACEHOLDER |
| 021_L108_RES3 | RES3 | RES | 111.199 | 70.863 | ART_PLACEHOLDER |
| 022_L108_RES4 | RES4 | RES | -67.594 | -81.658 | ART_PLACEHOLDER |
| 023_L108_RES5 | RES5 | RES | 4.154 | 54.934 | ART_PLACEHOLDER |
| 024_L108_RES6 | RES6 | RES | 62.017 | -34.962 | ART_PLACEHOLDER |
| 025_L108_RES7 | RES7 | RES | -14.08 | 56.828 | ART_PLACEHOLDER |
| 026_L108_RES8 | RES8 | RES | -53.466 | 71.105 | ART_PLACEHOLDER |
| 027_L108_RES9 | RES9 | RES | 2.736 | 82.905 | ART_PLACEHOLDER |
| 028_L108_RES10 | RES10 | RES | -7.85 | 43.487 | ART_PLACEHOLDER |
| 029_L108_RES11 | RES11 | RES | -119.747 | 1.571 | ART_PLACEHOLDER |
| 030_L108_RES12 | RES12 | RES | 91.23 | -34.954 | ART_PLACEHOLDER |
| 031_L108_RES13 | RES13 | RES | 101.928 | 6.888 | ART_PLACEHOLDER |
| 032_L108_RES14 | RES14 | RES | -66.132 | 62.698 | ART_PLACEHOLDER |
| 033_L108_RES15 | RES15 | RES | -115.959 | -7.956 | ART_PLACEHOLDER |
| 034_L108_RES16 | RES16 | RES | -33.139 | -41.004 | ART_PLACEHOLDER |
| 035_L108_RES17 | RES17 | RES | -6.409 | 82.141 | ART_PLACEHOLDER |
| 036_L108_RES18 | RES18 | RES | 92.559 | -22.654 | ART_PLACEHOLDER |
| 037_L108_RES19 | RES19 | RES | -110.948 | 41.087 | ART_PLACEHOLDER |
| 038_L108_RES20 | RES20 | RES | -68.392 | -63.593 | ART_PLACEHOLDER |
| 039_L108_RES21 | RES21 | RES | 13.774 | -53.781 | ART_PLACEHOLDER |
| 040_L108_RES22 | RES22 | RES | -58.873 | -11.738 | ART_PLACEHOLDER |
| 041_L108_RES23 | RES23 | RES | -109.21 | -70.07 | ART_PLACEHOLDER |
| 042_L108_RES24 | RES24 | RES | 30.614 | 19.717 | ART_PLACEHOLDER |
| 043_L108_RES25 | RES25 | RES | 55.544 | 60.566 | ART_PLACEHOLDER |
| 044_L108_RES26 | RES26 | RES | -79.121 | -47.915 | ART_PLACEHOLDER |
| 045_L108_RES27 | RES27 | RES | -46.512 | 46.277 | ART_PLACEHOLDER |
| 046_L108_RES28 | RES28 | RES | 118.315 | 17.856 | ART_PLACEHOLDER |
| 047_L108_CAP1 | CAP1 | CAP | -115.872 | -61.12 | ART_PLACEHOLDER |
| 048_L108_CAP2 | CAP2 | CAP | 43.533 | 5.789 | ART_PLACEHOLDER |
| 049_L108_CAP3 | CAP3 | CAP | -7.903 | 50.467 | ART_PLACEHOLDER |
| 050_L108_CAP4 | CAP4 | CAP | -16.058 | 43.553 | ART_PLACEHOLDER |
| 051_L108_CAP5 | CAP5 | CAP | 110.695 | -80.095 | ART_PLACEHOLDER |
| 052_L108_CAP6 | CAP6 | CAP | 34.366 | 8.669 | ART_PLACEHOLDER |
| 053_L108_CAP7 | CAP7 | CAP | -101.293 | -27.657 | ART_PLACEHOLDER |
| 054_L108_CAP8 | CAP8 | CAP | 34.781 | 76.07 | ART_PLACEHOLDER |
| 055_L108_CAP9 | CAP9 | CAP | 33.523 | -57.739 | ART_PLACEHOLDER |
| 056_L108_CAP10 | CAP10 | CAP | 35.601 | 53.221 | ART_PLACEHOLDER |
| 057_L108_CAP11 | CAP11 | CAP | 37.758 | 65.638 | ART_PLACEHOLDER |
| 058_L108_CAP12 | CAP12 | CAP | -55.554 | -34.148 | ART_PLACEHOLDER |
| 059_L108_CAP13 | CAP13 | CAP | -45.387 | 79.482 | ART_PLACEHOLDER |
| 060_L108_CAP14 | CAP14 | CAP | 6.747 | -42.045 | ART_PLACEHOLDER |
| 061_L108_CAP15 | CAP15 | CAP | 43.548 | -30.754 | ART_PLACEHOLDER |
| 062_L108_CAP16 | CAP16 | CAP | -40.076 | -44.856 | ART_PLACEHOLDER |
| 063_L108_CAP17 | CAP17 | CAP | -116.986 | -25.441 | ART_PLACEHOLDER |
| 064_L108_CAP18 | CAP18 | CAP | -96.274 | -52.99 | ART_PLACEHOLDER |
| 065_L108_CAP19 | CAP19 | CAP | 61.452 | 11.852 | ART_PLACEHOLDER |
| 066_L108_CAP20 | CAP20 | CAP | -20.224 | -58.869 | ART_PLACEHOLDER |
| 067_L108_CAP21 | CAP21 | CAP | 33.773 | 28.953 | ART_PLACEHOLDER |
| 068_L108_CAP22 | CAP22 | CAP | 110.104 | -7.008 | ART_PLACEHOLDER |
| 069_L108_CAP23 | CAP23 | CAP | -85.536 | -61.108 | ART_PLACEHOLDER |
| 070_L108_CAP24 | CAP24 | CAP | 113.783 | 32.553 | ART_PLACEHOLDER |
| 071_L108_CAP25 | CAP25 | CAP | -69.696 | -3.355 | ART_PLACEHOLDER |
| 072_L108_CAP26 | CAP26 | CAP | 64.014 | 31.889 | ART_PLACEHOLDER |
| 073_L108_CAP27 | CAP27 | CAP | -96.5 | -21.431 | ART_PLACEHOLDER |
| 074_L108_CAP28 | CAP28 | CAP | 68.81 | -57.279 | ART_PLACEHOLDER |
| 075_L108_CAP29 | CAP29 | CAP | 3.791 | 76.72 | ART_PLACEHOLDER |
| 076_L108_CAP30 | CAP30 | CAP | -7.891 | -44.846 | ART_PLACEHOLDER |
| 077_L108_INDUCTOR1 | INDUCTOR1 | INDUCTOR | -95.0 | 67.0 | ART_PLACEHOLDER |
| 078_L108_INDUCTOR2 | INDUCTOR2 | INDUCTOR | -95.0 | 47.0 | ART_PLACEHOLDER |
| 079_L108_INDUCTOR3 | INDUCTOR3 | INDUCTOR | -95.0 | 27.0 | ART_PLACEHOLDER |
| 080_L108_INDUCTOR4 | INDUCTOR4 | INDUCTOR | -95.0 | 7.0 | ART_PLACEHOLDER |
| 081_L108_INDUCTOR5 | INDUCTOR5 | INDUCTOR | -61.0 | 72.0 | ART_PLACEHOLDER |
| 082_L108_INDUCTOR6 | INDUCTOR6 | INDUCTOR | -61.0 | 52.0 | ART_PLACEHOLDER |
| 083_L108_MOSFET1 | MOSFET1 | MOSFET | -115.0 | 71.0 | ART_PLACEHOLDER |
| 084_L108_MOSFET2 | MOSFET2 | MOSFET | -114.0 | 55.0 | ART_PLACEHOLDER |
| 085_L108_MOSFET3 | MOSFET3 | MOSFET | -115.0 | 36.0 | ART_PLACEHOLDER |
| 086_L108_MOSFET4 | MOSFET4 | MOSFET | -114.0 | 18.0 | ART_PLACEHOLDER |
| 087_L108_MOSFET5 | MOSFET5 | MOSFET | -76.0 | 81.0 | ART_PLACEHOLDER |
| 088_L108_MOSFET6 | MOSFET6 | MOSFET | -48.0 | 81.0 | ART_PLACEHOLDER |
| 089_L108_CONNECTOR1 | CONNECTOR1 | CONNECTOR | -121.0 | 45.0 | ART_PLACEHOLDER |
| 090_L108_CONNECTOR2 | CONNECTOR2 | CONNECTOR | -122.0 | -67.0 | ART_PLACEHOLDER |
| 091_L108_CONNECTOR3 | CONNECTOR3 | CONNECTOR | 122.0 | 81.0 | ART_PLACEHOLDER |
| 092_L108_CONNECTOR4 | CONNECTOR4 | CONNECTOR | 124.0 | 11.0 | ART_PLACEHOLDER |
| 093_L108_J1 | J1 | PCIE | 18.0 | -79.0 | ART_PLACEHOLDER |
| 094_L108_LED1 | LED1 | LED | 55.0 | -50.0 | ART_PLACEHOLDER |
| 095_L108_LED2 | LED2 | LED | 68.0 | -50.0 | ART_PLACEHOLDER |
| 096_L108_LED3 | LED3 | LED | 81.0 | -50.0 | ART_PLACEHOLDER |
| 097_L108_LED4 | LED4 | LED | 94.0 | -50.0 | ART_PLACEHOLDER |
| 098_L108_SWITCH1 | SWITCH1 | SWITCH | -117.0 | -78.0 | ART_PLACEHOLDER |
| 099_L108_SWITCH2 | SWITCH2 | SWITCH | -78.0 | -77.0 | ART_PLACEHOLDER |
| 100_L108_SWITCH3 | SWITCH3 | SWITCH | 126.0 | -81.0 | ART_PLACEHOLDER |
| 101_L108_DIODE1 | DIODE1 | DIODE | 68.1 | 56.239 | ART_PLACEHOLDER |
| 102_L108_DIODE2 | DIODE2 | DIODE | 83.001 | -12.68 | ART_PLACEHOLDER |
| 103_L108_DIODE3 | DIODE3 | DIODE | -79.132 | -32.474 | ART_PLACEHOLDER |
| 104_L108_FERRITE1 | FERRITE1 | FERRITE | 95.747 | -7.488 | ART_PLACEHOLDER |
| 105_L108_FERRITE2 | FERRITE2 | FERRITE | 108.766 | -43.932 | ART_PLACEHOLDER |
| 106_L108_W157_DiagnosticTrustJumper | W157 | JUMPER | 123.0 | -53.0 | ART_PLACEHOLDER |
| 107_L108_S107_FlashBusArbiter | S107 | FLASH_ARBITER | -57.0 | -34.0 | ART_PLACEHOLDER |
| 108_L108_S108_RecoveryInputPullup | S108 | RECOVERY_INPUT_RESISTOR | 47.0 | -54.0 | ART_PLACEHOLDER |
| 109_L108_S109_RecoveryButton | S109 | RECOVERY_SWITCH | 59.0 | -49.0 | ART_PLACEHOLDER |
| 110_L108_S110_WriteProtectSwitch | S110 | FLASH_WRITE_PROTECT | 80.0 | -49.0 | ART_PLACEHOLDER |
| 111_L108_S111_FirmwareServiceHeader | S111 | SERVICE_HEADER | -67.0 | -72.0 | ART_PLACEHOLDER |
| 112_L108_S112_BootPolicySelector | S112 | BOOT_SELECTOR | 82.0 | -72.0 | ART_PLACEHOLDER |
| 113_L108_TP_STBY_CurrentSense | TP | CURRENT_SENSE | -119.0 | -68.0 | ART_PLACEHOLDER |
| 114_L108_J_DIAG_IsolationLink | J | ISOLATION_LINK | -82.0 | -76.0 | ART_PLACEHOLDER |
| 115_L108_TP_DDR_ThermalPad | TP | THERMAL_TEST_PAD | 70.0 | 66.0 | ART_PLACEHOLDER |
| 116_L108_J_CLK_SourceProbe | J | CLOCK_PROBE_HEADER | 105.0 | -69.0 | ART_PLACEHOLDER |
| 117_L108_U16_DiagnosticMonitor | U16 | DIAGNOSTIC_MONITOR | -61.0 | -72.0 | ART_PLACEHOLDER |
| 118_L108_U17_IndependentSafety | U17 | SAFETY_SUPERVISOR | 76.0 | -76.0 | ART_PLACEHOLDER |

## 17. Source annex C — 156 routing task records

All actual net, pin and layer fields are **UNASSIGNED** in the package. The table below does not attempt to fill them.
| Route | Subsystem | Source net | Certification |
| --- | --- | --- | --- |
| W001 | STANDBY_POWER | UNASSIGNED | NOT_CERTIFIABLE |
| W002 | STANDBY_POWER | UNASSIGNED | NOT_CERTIFIABLE |
| W003 | STANDBY_POWER | UNASSIGNED | NOT_CERTIFIABLE |
| W004 | STANDBY_POWER | UNASSIGNED | NOT_CERTIFIABLE |
| W005 | STANDBY_POWER | UNASSIGNED | NOT_CERTIFIABLE |
| W006 | STANDBY_POWER | UNASSIGNED | NOT_CERTIFIABLE |
| W007 | STANDBY_POWER | UNASSIGNED | NOT_CERTIFIABLE |
| W008 | STANDBY_POWER | UNASSIGNED | NOT_CERTIFIABLE |
| W009 | STANDBY_POWER | UNASSIGNED | NOT_CERTIFIABLE |
| W010 | STANDBY_POWER | UNASSIGNED | NOT_CERTIFIABLE |
| W011 | STANDBY_POWER | UNASSIGNED | NOT_CERTIFIABLE |
| W012 | STANDBY_POWER | UNASSIGNED | NOT_CERTIFIABLE |
| W013 | STANDBY_POWER | UNASSIGNED | NOT_CERTIFIABLE |
| W014 | STANDBY_POWER | UNASSIGNED | NOT_CERTIFIABLE |
| W015 | STANDBY_POWER | UNASSIGNED | NOT_CERTIFIABLE |
| W016 | STANDBY_POWER | UNASSIGNED | NOT_CERTIFIABLE |
| W017 | STANDBY_POWER | UNASSIGNED | NOT_CERTIFIABLE |
| W018 | STANDBY_POWER | UNASSIGNED | NOT_CERTIFIABLE |
| W019 | STANDBY_POWER | UNASSIGNED | NOT_CERTIFIABLE |
| W020 | STANDBY_POWER | UNASSIGNED | NOT_CERTIFIABLE |
| W021 | STANDBY_POWER | UNASSIGNED | NOT_CERTIFIABLE |
| W022 | STANDBY_POWER | UNASSIGNED | NOT_CERTIFIABLE |
| W023 | CPU_VRM_RESET | UNASSIGNED | NOT_CERTIFIABLE |
| W024 | CPU_VRM_RESET | UNASSIGNED | NOT_CERTIFIABLE |
| W025 | CPU_VRM_RESET | UNASSIGNED | NOT_CERTIFIABLE |
| W026 | CPU_VRM_RESET | UNASSIGNED | NOT_CERTIFIABLE |
| W027 | CPU_VRM_RESET | UNASSIGNED | NOT_CERTIFIABLE |
| W028 | CPU_VRM_RESET | UNASSIGNED | NOT_CERTIFIABLE |
| W029 | CPU_VRM_RESET | UNASSIGNED | NOT_CERTIFIABLE |
| W030 | CPU_VRM_RESET | UNASSIGNED | NOT_CERTIFIABLE |
| W031 | CPU_VRM_RESET | UNASSIGNED | NOT_CERTIFIABLE |
| W032 | CPU_VRM_RESET | UNASSIGNED | NOT_CERTIFIABLE |
| W033 | CPU_VRM_RESET | UNASSIGNED | NOT_CERTIFIABLE |
| W034 | CPU_VRM_RESET | UNASSIGNED | NOT_CERTIFIABLE |
| W035 | CPU_VRM_RESET | UNASSIGNED | NOT_CERTIFIABLE |
| W036 | CPU_VRM_RESET | UNASSIGNED | NOT_CERTIFIABLE |
| W037 | CPU_VRM_RESET | UNASSIGNED | NOT_CERTIFIABLE |
| W038 | CPU_VRM_RESET | UNASSIGNED | NOT_CERTIFIABLE |
| W039 | CPU_VRM_RESET | UNASSIGNED | NOT_CERTIFIABLE |
| W040 | CPU_VRM_RESET | UNASSIGNED | NOT_CERTIFIABLE |
| W041 | CPU_VRM_RESET | UNASSIGNED | NOT_CERTIFIABLE |
| W042 | CPU_VRM_RESET | UNASSIGNED | NOT_CERTIFIABLE |
| W043 | CPU_VRM_RESET | UNASSIGNED | NOT_CERTIFIABLE |
| W044 | CPU_VRM_RESET | UNASSIGNED | NOT_CERTIFIABLE |
| W045 | DDR4_MEMORY | UNASSIGNED | NOT_CERTIFIABLE |
| W046 | DDR4_MEMORY | UNASSIGNED | NOT_CERTIFIABLE |
| W047 | DDR4_MEMORY | UNASSIGNED | NOT_CERTIFIABLE |
| W048 | DDR4_MEMORY | UNASSIGNED | NOT_CERTIFIABLE |
| W049 | DDR4_MEMORY | UNASSIGNED | NOT_CERTIFIABLE |
| W050 | DDR4_MEMORY | UNASSIGNED | NOT_CERTIFIABLE |
| W051 | DDR4_MEMORY | UNASSIGNED | NOT_CERTIFIABLE |
| W052 | DDR4_MEMORY | UNASSIGNED | NOT_CERTIFIABLE |
| W053 | DDR4_MEMORY | UNASSIGNED | NOT_CERTIFIABLE |
| W054 | DDR4_MEMORY | UNASSIGNED | NOT_CERTIFIABLE |
| W055 | DDR4_MEMORY | UNASSIGNED | NOT_CERTIFIABLE |
| W056 | DDR4_MEMORY | UNASSIGNED | NOT_CERTIFIABLE |
| W057 | DDR4_MEMORY | UNASSIGNED | NOT_CERTIFIABLE |
| W058 | DDR4_MEMORY | UNASSIGNED | NOT_CERTIFIABLE |
| W059 | DDR4_MEMORY | UNASSIGNED | NOT_CERTIFIABLE |
| W060 | DDR4_MEMORY | UNASSIGNED | NOT_CERTIFIABLE |
| W061 | DDR4_MEMORY | UNASSIGNED | NOT_CERTIFIABLE |
| W062 | DDR4_MEMORY | UNASSIGNED | NOT_CERTIFIABLE |
| W063 | DDR4_MEMORY | UNASSIGNED | NOT_CERTIFIABLE |
| W064 | DDR4_MEMORY | UNASSIGNED | NOT_CERTIFIABLE |
| W065 | DDR4_MEMORY | UNASSIGNED | NOT_CERTIFIABLE |
| W066 | DDR4_MEMORY | UNASSIGNED | NOT_CERTIFIABLE |
| W067 | DDR4_MEMORY | UNASSIGNED | NOT_CERTIFIABLE |
| W068 | DDR4_MEMORY | UNASSIGNED | NOT_CERTIFIABLE |
| W069 | FIRMWARE_EC | UNASSIGNED | NOT_CERTIFIABLE |
| W070 | FIRMWARE_EC | UNASSIGNED | NOT_CERTIFIABLE |
| W071 | FIRMWARE_EC | UNASSIGNED | NOT_CERTIFIABLE |
| W072 | FIRMWARE_EC | UNASSIGNED | NOT_CERTIFIABLE |
| W073 | FIRMWARE_EC | UNASSIGNED | NOT_CERTIFIABLE |
| W074 | FIRMWARE_EC | UNASSIGNED | NOT_CERTIFIABLE |
| W075 | FIRMWARE_EC | UNASSIGNED | NOT_CERTIFIABLE |
| W076 | FIRMWARE_EC | UNASSIGNED | NOT_CERTIFIABLE |
| W077 | FIRMWARE_EC | UNASSIGNED | NOT_CERTIFIABLE |
| W078 | FIRMWARE_EC | UNASSIGNED | NOT_CERTIFIABLE |
| W079 | FIRMWARE_EC | UNASSIGNED | NOT_CERTIFIABLE |
| W080 | FIRMWARE_EC | UNASSIGNED | NOT_CERTIFIABLE |
| W081 | FIRMWARE_EC | UNASSIGNED | NOT_CERTIFIABLE |
| W082 | FIRMWARE_EC | UNASSIGNED | NOT_CERTIFIABLE |
| W083 | FIRMWARE_EC | UNASSIGNED | NOT_CERTIFIABLE |
| W084 | FIRMWARE_EC | UNASSIGNED | NOT_CERTIFIABLE |
| W085 | FIRMWARE_EC | UNASSIGNED | NOT_CERTIFIABLE |
| W086 | FIRMWARE_EC | UNASSIGNED | NOT_CERTIFIABLE |
| W087 | FIRMWARE_EC | UNASSIGNED | NOT_CERTIFIABLE |
| W088 | FIRMWARE_EC | UNASSIGNED | NOT_CERTIFIABLE |
| W089 | FIRMWARE_EC | UNASSIGNED | NOT_CERTIFIABLE |
| W090 | FIRMWARE_EC | UNASSIGNED | NOT_CERTIFIABLE |
| W091 | PCIE_CLOCK | UNASSIGNED | NOT_CERTIFIABLE |
| W092 | PCIE_CLOCK | UNASSIGNED | NOT_CERTIFIABLE |
| W093 | PCIE_CLOCK | UNASSIGNED | NOT_CERTIFIABLE |
| W094 | PCIE_CLOCK | UNASSIGNED | NOT_CERTIFIABLE |
| W095 | PCIE_CLOCK | UNASSIGNED | NOT_CERTIFIABLE |
| W096 | PCIE_CLOCK | UNASSIGNED | NOT_CERTIFIABLE |
| W097 | PCIE_CLOCK | UNASSIGNED | NOT_CERTIFIABLE |
| W098 | PCIE_CLOCK | UNASSIGNED | NOT_CERTIFIABLE |
| W099 | PCIE_CLOCK | UNASSIGNED | NOT_CERTIFIABLE |
| W100 | PCIE_CLOCK | UNASSIGNED | NOT_CERTIFIABLE |
| W101 | PCIE_CLOCK | UNASSIGNED | NOT_CERTIFIABLE |
| W102 | PCIE_CLOCK | UNASSIGNED | NOT_CERTIFIABLE |
| W103 | PCIE_CLOCK | UNASSIGNED | NOT_CERTIFIABLE |
| W104 | PCIE_CLOCK | UNASSIGNED | NOT_CERTIFIABLE |
| W105 | PCIE_CLOCK | UNASSIGNED | NOT_CERTIFIABLE |
| W106 | PCIE_CLOCK | UNASSIGNED | NOT_CERTIFIABLE |
| W107 | PCIE_CLOCK | UNASSIGNED | NOT_CERTIFIABLE |
| W108 | PCIE_CLOCK | UNASSIGNED | NOT_CERTIFIABLE |
| W109 | PCIE_CLOCK | UNASSIGNED | NOT_CERTIFIABLE |
| W110 | PCIE_CLOCK | UNASSIGNED | NOT_CERTIFIABLE |
| W111 | PCIE_CLOCK | UNASSIGNED | NOT_CERTIFIABLE |
| W112 | PCIE_CLOCK | UNASSIGNED | NOT_CERTIFIABLE |
| W113 | PCIE_CLOCK | UNASSIGNED | NOT_CERTIFIABLE |
| W114 | PCIE_CLOCK | UNASSIGNED | NOT_CERTIFIABLE |
| W115 | DIAGNOSTIC_ISOLATION | UNASSIGNED | NOT_CERTIFIABLE |
| W116 | DIAGNOSTIC_ISOLATION | UNASSIGNED | NOT_CERTIFIABLE |
| W117 | DIAGNOSTIC_ISOLATION | UNASSIGNED | NOT_CERTIFIABLE |
| W118 | DIAGNOSTIC_ISOLATION | UNASSIGNED | NOT_CERTIFIABLE |
| W119 | DIAGNOSTIC_ISOLATION | UNASSIGNED | NOT_CERTIFIABLE |
| W120 | DIAGNOSTIC_ISOLATION | UNASSIGNED | NOT_CERTIFIABLE |
| W121 | DIAGNOSTIC_ISOLATION | UNASSIGNED | NOT_CERTIFIABLE |
| W122 | DIAGNOSTIC_ISOLATION | UNASSIGNED | NOT_CERTIFIABLE |
| W123 | DIAGNOSTIC_ISOLATION | UNASSIGNED | NOT_CERTIFIABLE |
| W124 | DIAGNOSTIC_ISOLATION | UNASSIGNED | NOT_CERTIFIABLE |
| W125 | DIAGNOSTIC_ISOLATION | UNASSIGNED | NOT_CERTIFIABLE |
| W126 | DIAGNOSTIC_ISOLATION | UNASSIGNED | NOT_CERTIFIABLE |
| W127 | DIAGNOSTIC_ISOLATION | UNASSIGNED | NOT_CERTIFIABLE |
| W128 | DIAGNOSTIC_ISOLATION | UNASSIGNED | NOT_CERTIFIABLE |
| W129 | DIAGNOSTIC_ISOLATION | UNASSIGNED | NOT_CERTIFIABLE |
| W130 | DIAGNOSTIC_ISOLATION | UNASSIGNED | NOT_CERTIFIABLE |
| W131 | DIAGNOSTIC_ISOLATION | UNASSIGNED | NOT_CERTIFIABLE |
| W132 | DIAGNOSTIC_ISOLATION | UNASSIGNED | NOT_CERTIFIABLE |
| W133 | DIAGNOSTIC_ISOLATION | UNASSIGNED | NOT_CERTIFIABLE |
| W134 | DIAGNOSTIC_ISOLATION | UNASSIGNED | NOT_CERTIFIABLE |
| W135 | DIAGNOSTIC_ISOLATION | UNASSIGNED | NOT_CERTIFIABLE |
| W136 | DIAGNOSTIC_ISOLATION | UNASSIGNED | NOT_CERTIFIABLE |
| W137 | SAFETY_TESTPOINTS | UNASSIGNED | NOT_CERTIFIABLE |
| W138 | SAFETY_TESTPOINTS | UNASSIGNED | NOT_CERTIFIABLE |
| W139 | SAFETY_TESTPOINTS | UNASSIGNED | NOT_CERTIFIABLE |
| W140 | SAFETY_TESTPOINTS | UNASSIGNED | NOT_CERTIFIABLE |
| W141 | SAFETY_TESTPOINTS | UNASSIGNED | NOT_CERTIFIABLE |
| W142 | SAFETY_TESTPOINTS | UNASSIGNED | NOT_CERTIFIABLE |
| W143 | SAFETY_TESTPOINTS | UNASSIGNED | NOT_CERTIFIABLE |
| W144 | SAFETY_TESTPOINTS | UNASSIGNED | NOT_CERTIFIABLE |
| W145 | SAFETY_TESTPOINTS | UNASSIGNED | NOT_CERTIFIABLE |
| W146 | SAFETY_TESTPOINTS | UNASSIGNED | NOT_CERTIFIABLE |
| W147 | SAFETY_TESTPOINTS | UNASSIGNED | NOT_CERTIFIABLE |
| W148 | SAFETY_TESTPOINTS | UNASSIGNED | NOT_CERTIFIABLE |
| W149 | SAFETY_TESTPOINTS | UNASSIGNED | NOT_CERTIFIABLE |
| W150 | SAFETY_TESTPOINTS | UNASSIGNED | NOT_CERTIFIABLE |
| W151 | SAFETY_TESTPOINTS | UNASSIGNED | NOT_CERTIFIABLE |
| W152 | SAFETY_TESTPOINTS | UNASSIGNED | NOT_CERTIFIABLE |
| W153 | SAFETY_TESTPOINTS | UNASSIGNED | NOT_CERTIFIABLE |
| W154 | SAFETY_TESTPOINTS | UNASSIGNED | NOT_CERTIFIABLE |
| W155 | SAFETY_TESTPOINTS | UNASSIGNED | NOT_CERTIFIABLE |
| W156 | SAFETY_TESTPOINTS | UNASSIGNED | NOT_CERTIFIABLE |

## 18. Source annex D — all 50 via records

| Via | Net | Start layer | End layer | Via type | Certification |
| --- | --- | --- | --- | --- | --- |
| V01 | UNASSIGNED | UNASSIGNED | UNASSIGNED | UNASSIGNED | NOT_CERTIFIABLE |
| V02 | UNASSIGNED | UNASSIGNED | UNASSIGNED | UNASSIGNED | NOT_CERTIFIABLE |
| V03 | UNASSIGNED | UNASSIGNED | UNASSIGNED | UNASSIGNED | NOT_CERTIFIABLE |
| V04 | UNASSIGNED | UNASSIGNED | UNASSIGNED | UNASSIGNED | NOT_CERTIFIABLE |
| V05 | UNASSIGNED | UNASSIGNED | UNASSIGNED | UNASSIGNED | NOT_CERTIFIABLE |
| V06 | UNASSIGNED | UNASSIGNED | UNASSIGNED | UNASSIGNED | NOT_CERTIFIABLE |
| V07 | UNASSIGNED | UNASSIGNED | UNASSIGNED | UNASSIGNED | NOT_CERTIFIABLE |
| V08 | UNASSIGNED | UNASSIGNED | UNASSIGNED | UNASSIGNED | NOT_CERTIFIABLE |
| V09 | UNASSIGNED | UNASSIGNED | UNASSIGNED | UNASSIGNED | NOT_CERTIFIABLE |
| V10 | UNASSIGNED | UNASSIGNED | UNASSIGNED | UNASSIGNED | NOT_CERTIFIABLE |
| V11 | UNASSIGNED | UNASSIGNED | UNASSIGNED | UNASSIGNED | NOT_CERTIFIABLE |
| V12 | UNASSIGNED | UNASSIGNED | UNASSIGNED | UNASSIGNED | NOT_CERTIFIABLE |
| V13 | UNASSIGNED | UNASSIGNED | UNASSIGNED | UNASSIGNED | NOT_CERTIFIABLE |
| V14 | UNASSIGNED | UNASSIGNED | UNASSIGNED | UNASSIGNED | NOT_CERTIFIABLE |
| V15 | UNASSIGNED | UNASSIGNED | UNASSIGNED | UNASSIGNED | NOT_CERTIFIABLE |
| V16 | UNASSIGNED | UNASSIGNED | UNASSIGNED | UNASSIGNED | NOT_CERTIFIABLE |
| V17 | UNASSIGNED | UNASSIGNED | UNASSIGNED | UNASSIGNED | NOT_CERTIFIABLE |
| V18 | UNASSIGNED | UNASSIGNED | UNASSIGNED | UNASSIGNED | NOT_CERTIFIABLE |
| V19 | UNASSIGNED | UNASSIGNED | UNASSIGNED | UNASSIGNED | NOT_CERTIFIABLE |
| V20 | UNASSIGNED | UNASSIGNED | UNASSIGNED | UNASSIGNED | NOT_CERTIFIABLE |
| V21 | UNASSIGNED | UNASSIGNED | UNASSIGNED | UNASSIGNED | NOT_CERTIFIABLE |
| V22 | UNASSIGNED | UNASSIGNED | UNASSIGNED | UNASSIGNED | NOT_CERTIFIABLE |
| V23 | UNASSIGNED | UNASSIGNED | UNASSIGNED | UNASSIGNED | NOT_CERTIFIABLE |
| V24 | UNASSIGNED | UNASSIGNED | UNASSIGNED | UNASSIGNED | NOT_CERTIFIABLE |
| V25 | UNASSIGNED | UNASSIGNED | UNASSIGNED | UNASSIGNED | NOT_CERTIFIABLE |
| V26 | UNASSIGNED | UNASSIGNED | UNASSIGNED | UNASSIGNED | NOT_CERTIFIABLE |
| V27 | UNASSIGNED | UNASSIGNED | UNASSIGNED | UNASSIGNED | NOT_CERTIFIABLE |
| V28 | UNASSIGNED | UNASSIGNED | UNASSIGNED | UNASSIGNED | NOT_CERTIFIABLE |
| V29 | UNASSIGNED | UNASSIGNED | UNASSIGNED | UNASSIGNED | NOT_CERTIFIABLE |
| V30 | UNASSIGNED | UNASSIGNED | UNASSIGNED | UNASSIGNED | NOT_CERTIFIABLE |
| V31 | UNASSIGNED | UNASSIGNED | UNASSIGNED | UNASSIGNED | NOT_CERTIFIABLE |
| V32 | UNASSIGNED | UNASSIGNED | UNASSIGNED | UNASSIGNED | NOT_CERTIFIABLE |
| V33 | UNASSIGNED | UNASSIGNED | UNASSIGNED | UNASSIGNED | NOT_CERTIFIABLE |
| V34 | UNASSIGNED | UNASSIGNED | UNASSIGNED | UNASSIGNED | NOT_CERTIFIABLE |
| V35 | UNASSIGNED | UNASSIGNED | UNASSIGNED | UNASSIGNED | NOT_CERTIFIABLE |
| V36 | UNASSIGNED | UNASSIGNED | UNASSIGNED | UNASSIGNED | NOT_CERTIFIABLE |
| V37 | UNASSIGNED | UNASSIGNED | UNASSIGNED | UNASSIGNED | NOT_CERTIFIABLE |
| V38 | UNASSIGNED | UNASSIGNED | UNASSIGNED | UNASSIGNED | NOT_CERTIFIABLE |
| V39 | UNASSIGNED | UNASSIGNED | UNASSIGNED | UNASSIGNED | NOT_CERTIFIABLE |
| V40 | UNASSIGNED | UNASSIGNED | UNASSIGNED | UNASSIGNED | NOT_CERTIFIABLE |
| V41 | UNASSIGNED | UNASSIGNED | UNASSIGNED | UNASSIGNED | NOT_CERTIFIABLE |
| V42 | UNASSIGNED | UNASSIGNED | UNASSIGNED | UNASSIGNED | NOT_CERTIFIABLE |
| V43 | UNASSIGNED | UNASSIGNED | UNASSIGNED | UNASSIGNED | NOT_CERTIFIABLE |
| V44 | UNASSIGNED | UNASSIGNED | UNASSIGNED | UNASSIGNED | NOT_CERTIFIABLE |
| V45 | UNASSIGNED | UNASSIGNED | UNASSIGNED | UNASSIGNED | NOT_CERTIFIABLE |
| V46 | UNASSIGNED | UNASSIGNED | UNASSIGNED | UNASSIGNED | NOT_CERTIFIABLE |
| V47 | UNASSIGNED | UNASSIGNED | UNASSIGNED | UNASSIGNED | NOT_CERTIFIABLE |
| V48 | UNASSIGNED | UNASSIGNED | UNASSIGNED | UNASSIGNED | NOT_CERTIFIABLE |
| V49 | UNASSIGNED | UNASSIGNED | UNASSIGNED | UNASSIGNED | NOT_CERTIFIABLE |
| V50 | UNASSIGNED | UNASSIGNED | UNASSIGNED | UNASSIGNED | NOT_CERTIFIABLE |

## 19. Source annex E — full objective and fault source records

| ID | Requirement | Source status |
| --- | --- | --- |
| 01 | Assemble 118 components | NOT_STARTED |
| 02 | Complete 156 certified copper tasks and 50 vias | NOT_STARTED |
| 03 | Capture initial diagnostic baseline | NOT_STARTED |
| 04 | Prove root causes using evidence and isolation | NOT_STARTED |
| 05 | Repair F1-F3 and requalify cold/warm/PCIe | NOT_STARTED |
| 06 | Disconnect/restore W157; fresh safety qualification | NOT_STARTED |

| ID | Subsystem | Name | Model | Symptoms | Required evidence | Approved repair |
| --- | --- | --- | --- | --- | --- | --- |
| F1 | STANDBY_POWER | Leaky standby ceramic capacitor | LEAKAGE_AND_CURRENT_LIMITING | No power/abnormal standby current | current measurement + isolated branch comparison | replace leaking capacitor |
| F2 | DDR4_MEMORY | Intermittent U3 BGA contact | TEMPERATURE_DEPENDENT_CONTACT | warm memory training errors | cold/warm lane test + BGA inspection | controlled BGA rework or replacement |
| F3 | PCIE_CLOCK | Intermittent PCIe reference-clock contact | THERMALLY_INTERMITTENT_CLOCK | endpoint disappears / L0 to Recovery | source clock compared with endpoint waveform | clock solder contact restoration |

## 20. Source annex F — W157 original safety JSON

```json
{
  "jumper": "W157",
  "source": "L108_SystemDiagnosticMonitor_U16.TRUSTED_DIAGNOSTIC_READY_OUT",
  "destination": "L108_IndependentSafetySupervisor_U17.TRUSTED_DIAGNOSTIC_READY_IN",
  "physical_pin_mapping": "UNASSIGNED",
  "netlist_status": "NOT_CERTIFIABLE",
  "sequence": [
    "disconnection",
    "independent inhibition",
    "physical reconnection",
    "fresh evidence",
    "explicit safety reset",
    "final qualification"
  ]
}
```

## 21. Source annex G — supplied Unity Editor art builder (exact source)

```csharp
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
namespace ProjectSpark.Level108.Editor
{
    public static class SparkL108BoardArtBuilder
    {
        [MenuItem("Tools/Project Spark/Levels/108/1. Build EMPTY DFX-16 Board")]
        public static void BuildEmptyBoard()
        {
            var guids = AssetDatabase.FindAssets("119_L108_DFX16_BarePCB t:Model");
            if (guids.Length == 0)
            {
                Debug.LogError("[L108] Bare board model not found. Import Models_FBX first.");
                return;
            }
            var path = AssetDatabase.GUIDToAssetPath(guids[0]);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (!prefab) { Debug.LogError("[L108] FBX import failed: " + path); return; }
            var root = new GameObject("L108_DFX16_EMPTY_BOARD_ART");
            Undo.RegisterCreatedObjectUndo(root, "Create Level 108 empty board");
            var board = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            if (!board) { Object.DestroyImmediate(root); return; }
            Undo.SetTransformParent(board.transform, root.transform, "Parent DFX-16 board");
            board.name = "DFX16_BarePCB_ArtOnly";
            // FBX reuses the 285 x 205 mm source geometry.
            // Scale only the visual footprint here, not the authoritative netlist.
            board.transform.localScale = new Vector3(300f/285f, 220f/205f, 1f);
            Selection.activeGameObject = root;
            Debug.Log("[L108] Empty BOARD ART created. No components/routes/vias/objectives are certified.");
        }
    }
}
#endif

```

## 22. Source annex H — supplied Unity Editor starter material generator (exact source)

```csharp
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;
namespace ProjectSpark.Level108.Editor
{
    public static class SparkL108StarterMaterials
    {
        [MenuItem("Tools/Project Spark/Levels/108/2. Generate PBR Starter Materials")]
        public static void Create()
        {
            const string folder = "Assets/ProjectSpark/Level108/Materials_Generated";
            Directory.CreateDirectory(folder);
            string[,] defs = {
                {"PCB_Diagnostic_Blue", "#0B2846", "0.05"},
                {"IC_Epoxy", "#171E26", "0.0"},
                {"Gold_Pads", "#BFA255", "0.85"},
                {"Copper", "#B97E39", "0.8"},
                {"Solder", "#A8B4C1", "0.72"},
                {"Aluminum", "#8E9CA6", "0.8"},
                {"Ceramic", "#D3C9B4", "0.0"},
                {"FR4_Core", "#9B8053", "0.0"},
                {"Diagnostic_Orange", "#E88C42", "0.1"},
                {"Fault_Red", "#BB4554", "0.1"},
                {"Diagnostic_Cyan", "#38ADD0", "0.1"}
            };
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            if (shader == null) { Debug.LogError("[L108] No supported shader found"); return; }
            for (int i=0;i<defs.GetLength(0);i++)
            {
                string path = folder+"/"+defs[i,0]+".mat";
                if (AssetDatabase.LoadAssetAtPath<Material>(path)!=null) continue;
                var m = new Material(shader);
                if (ColorUtility.TryParseHtmlString(defs[i,1], out var col))
                {
                    if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", col);
                    if (m.HasProperty("_Color")) m.SetColor("_Color", col);
                }
                float met=float.Parse(defs[i,2], System.Globalization.CultureInfo.InvariantCulture);
                if(m.HasProperty("_Metallic"))m.SetFloat("_Metallic",met);
                if(m.HasProperty("_Smoothness"))m.SetFloat("_Smoothness",0.45f);
                AssetDatabase.CreateAsset(m,path);
            }
            AssetDatabase.SaveAssets();
            Debug.Log("[L108] Art-only starter materials created; assign actual PBR maps and validate FBX slots.");
        }
    }
}
#endif

```

## 23. Source annex I — package README (exact source)

```markdown
# PROJECT SPARK — LEVEL 108: GHOST IN THE CIRCUIT
## DFX-16 Diagnostic Fusion — Board-Only Procedural Asset Kit

This package provides **real mesh files**, not another image: 152 separate FBX assets + corresponding GLBs, 118 individually placeable components, 34 structural/diagnostic assets, and one assembled-reference asset in each format.

### Contents
- `Models_FBX/`: separate board and electronic models; one assembled FBX visual reference.
- `Models_GLB/`: matching GLBs and a composed assembled scene with three diagnostic indicators.
- `Textures/`: inherited basic PBR texture references plus new Level 108 2K silkscreen.
- `Data/`: 118 part placements, 156 planned routes, 50 vias, 3 faults, 6 objectives and W157 feedback.
- `Unity/Editor/`: an empty-board art builder and starter material generator.

### Scope and quality
This asset kit **adapts the procedural 3D mesh library from Level 107**. The shared CPU, RAM, regulator, connector and passive geometries are reused under Level 108 asset identities. Additional diagnostics and fault-overlay objects are created from compatible existing meshes; they have not been re-sculpted into unique manufacturer-accurate forms. The GLB assembled reference includes colored diagnostic markers. It is a layout reference, not the completed playable state. The FBX reference retains its inherited source board composition and will require adjustment to exactly match the Level 108 reference GLB and plan.

This is a reusable **realistic-style production foundation**, not hand-finished AAA artwork. Models require final art direction, pivots, UVs, materials, manufacturer footprints and Unity import verification.

### Target PCB size
DFX-16 design target = **300 x 220 mm**, ten layers. The inherited bare-board mesh is approximately **285 x 205 mm**; Unity empty-board art builder applies a display scale adjustment. Component, testpad and copper positions must be reconciled against the actual modified mesh before meaningful use. Model source axes and Unity scale may need manual correction.

### Electrical validity
- The **156 routes** are only named planning tasks; ALL pins/nets/layers are UNASSIGNED and `NOT_CERTIFIABLE`.
- The **50 vias** lack physical layer spans and net assignments and are `NOT_CERTIFIABLE`.
- W157's logical U16-U17 path is defined, but exact manufacturer components and physical pins remain UNASSIGNED.
- Neither a functional power solver nor thermal memory/clock faults nor a real diagnostic engine is included.
- All art models must remain separate from gameplay authority. Never auto-complete objectives from visual mesh presence.

### Unity import
1. Extract package under `Assets/ProjectSpark/Level108/` preserving the folder structure.
2. Let Unity import the FBX files and inspect scale/axes and generated materials.
3. Run `Tools/Project Spark/Levels/108/1. Build EMPTY DFX-16 Board`.
4. Optionally run `Tools/Project Spark/Levels/108/2. Generate PBR Starter Materials`.
5. Manually apply texture maps and validate the assembled reference in your target render pipeline.
6. Create verified pin-level schematic and integrate with existing Project Spark PCB repair, electrical graph and LevelGamePlayManager.

### Start of gameplay
0/118 installed, 0/156 routes, 0/50 vias, W157 disconnected, 0/6 objectives.

### Fault definitions
- F1: leaky standby ceramic capacitor resulting in current limiting and no-power behavior.
- F2: U3 DDR4 BGA contact opens as the simulated board warms.
- F3: PCIe reference-clock contact intermittently fails, degrading link reliability.

**No rooms, furniture, or physical diagnostic instruments are included. Virtual instruments remain World-Space Canvas UI.**

```

## 24. Final requirement / definition of done

Level 108 is complete **only** when the approved actual circuit topology exists, the board initializes from the empty state, all fault scenarios reproduce predictably under appropriate tests, each approved repair changes actual simulation, W157 safety behaves independently, all six objectives are legitimately certified, and the Unity project compiles and passes the test suite with no regression to Levels 1–107. If the electrical-design inputs are absent, submit the functioning art/UI/fault-model scaffolding plus a blocking schematic report—do **not** claim a fully playable level or a passed 6/6. Aim for production-quality AAA presentation after actual material/geometry review, not through inflated quality labels.

**PROJECT SPARK — LEVEL 108: GHOST IN THE CIRCUIT**  
**OBSERVE. ISOLATE. DIAGNOSE. PROVE. REPAIR.**

