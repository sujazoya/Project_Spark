# PROJECT SPARK — LEVEL 106: FIRST BOOT
## MASTER UNITY GAMEPLAY IMPLEMENTATION PROMPT — SFX-14 Integrated Computer Motherboard
**Source basis:** `ProjectSpark_Level106_FirstBoot_BoardOnly_Realistic_Complete.zip` / `ProjectSpark_Level106_FirstBoot_BoardOnly_Realistic/`  
**Contract:** Implement a fully playable, electrical-authority-driven Unity level using the included board-only art and the existing Project Spark project. Do NOT produce a new image or a new room. Preserve Levels 1–105.

> You are the senior Unity gameplay engineer, simulation programmer, PCB systems engineer, UI engineer, technical artist and QA lead for Project Spark. Inspect the real project APIs before changing code. Produce complete compiling Unity C# and editor integration, not pseudocode. Proceed incrementally, create self-tests and fail closed on unresolved electrical authority. Never claim that code has compiled or Play Mode has passed unless actually tested.

This document is an implementation request, **not an assertion that existing sources already implement electrical gameplay**. Source art is procedural and does not have certified electrical pins or manufacturer-approved footprints.

## 01 — Non-negotiable source facts and scope
- **Board:** SFX-14, 285 × 205 × 1.6 mm, ten-layer design; source geometry uses meters and +Z as board-top in the exported asset coordinate frame. Verify Unity Y-up import orientation and all pivot positions.
- **Art:** 136 modular FBX meshes, 136 corresponding GLBs, one complete assembled visual-reference model in each format; **106 individually installable components** and 30 other board/detail assets.
- **Puzzle:** 140 *planned* player routes, 42 *planned* via tasks, a separate W141 jumper, three fault scenarios, six objectives.
- **Source state:** All 140 routes contain `UNASSIGNED` start/end/net IDs, and all 42 vias have unassigned nets, layers and types; both are `NOT_CERTIFIABLE`. Do not infer actual connectivity from placement or decorative trace geometry. No objective may pass those gates until a reviewed pin-level electrical specification is supplied.
- **W141 source:** `L106_SystemBootMonitor_U14.TRUSTED_SYSTEM_READY_OUT` → W141 → `L106_IndependentSafety_U15.TRUSTED_SYSTEM_READY_IN`. The **U14 device is missing from the provided component BOM**, although U15 is present; exact pins are unassigned. Reconcile with the approved schematic and update BOM/model binding before certification. Do not invent U14 physical pins.
- **Source Unity helpers:** `SparkL106ArtBuilder.cs` and `SparkL106StarterMaterials.cs` are Editor-only art/material utilities, not a gameplay framework. Inspect and fix them if necessary, but do not overwrite existing project systems.
- **Existing project authority:** Existing circuit solver/topology, object shelf/shop systems, world-space virtual PCB repair UI, and `LevelGamePlayManager` remain authoritative wherever their real APIs support those functions.

## 02 — Mandatory audit / integration gate
1. Inventory relevant Unity assemblies, namespaces, package versions, render pipeline and scenes; inspect the actual definitions of `LevelGamePlayManager`, connected electrical graph/net nodes, component placement, route/via systems, fail states, repair system, UI tool interactions and objectives.
2. Create an `IntegrationReport.md` listing real APIs, missing capabilities, assembly-definition boundaries, serializer layouts and planned adapters. Do **not** assume proposed class names below already exist.
3. Resolve duplicate/partial components and conflict-prone references before adding replacements; reuse, extend or adapt existing APIs rather than creating a second electrical authority.
4. Create an `ElectricalAuthorityStatus` gate with states `MissingSource`, `RequiresReview`, `Validated`. Default to `RequiresReview` while any net, pin, layer or W141 endpoint is unresolved. In this state, allow art preview and clearly labeled non-certifying demo interactions only.
5. Preserve Levels 1–105, existing player inventory, save data, existing shop workflows and global scene controls. Add Level 106 assets with `L106_` prefixes and keep gameplay-specific code under a unique namespace.

## 03 — Required source reconciliation deliverables
Produce a reviewed `SFX14_PinNetlist.asset` (and human-readable CSV/JSON) with **every installed component physical pin**, footprint pad, pad position, net ID, voltage domain, direction and connection rules. Create a route authority table mapping each W001–W140 row to valid pin endpoints and net IDs; include permissible layers and width/spacing classes. Create via authority table mapping V01–V42 to net ID, start/end layers, drilled span, via type, manufacturing legality and copper drill/contact geometry. Provide checked footprint origin/orientation information for all 106 parts.

Separate **factory-fixed copper connectivity** from player-buildable routes. Preinstalled decorative copper must never accidentally satisfy W001–W140. Implement actual graph connectivity, voltage propagation and short detection; component visuals cannot stand in for pins. Include `NetlistReviewReport` with counts, missing endpoints, net collisions, illegal via spans, cross-power shorts, orphan nodes, ground continuity, missing protection and BOM mismatches. Report all unresolved links explicitly and stop certification; do not silently auto-assign plausible pins.

Resolve W141 U14 versus BOM before implementing the safety test, and do not silently rename it to another IC. U15 must be physically mapped to its intended pins as well.

## 04 — Asset import and AAA board visualization
Use exact files from `Models_FBX/` and the placement values in `L106_Components_106.csv`. Detect model paths relative to package root, not absolute hardcoded `Assets/ProjectSpark/Level106` assumptions. Read `L106_Models_136.csv` for model identities; treat all assembled reference geometry as **non-gameplay**. Build named hierarchy:
```
Level106_SFX14
  BoardArt/EmptyPCB
  BoardArt/FixedFactoryCopper
  BoardArt/LayerReferences_L01_to_L10  [visual only]
  Components/Installed
  Components/UninstalledSpawnPoints
  PadsAndTerminals
  PlayerRoutes
  PlayerVias
  W141_Jumper
  FaultTargets
  VFXContactAnchors
  WorldSpaceUI
  RuntimeSystems
```
Do not duplicate component visuals from completed showcase in playable hierarchy. Provide mesh normals/tangent and scale verification, URP/HDRP/Standard material compatibility, physically suitable metallic/smoothness mapping, 2K silkscreen alignment, and non-conflicting click/touch colliders. Use realistic solder/metal/FR4 surfaces without over-bright emission. Maintain prefab overrides and support one-click regeneration without duplicates.

## 05 — Installation and initial board state
Initial state: `components 0/106`, `routes 0/140`, `vias 0/42`, `W141 disconnected`, `objectives 0/6`, `POST not started`, safety not authorized. Render empty footprints and usable interactive installation anchors but do not count art-only pads as installed components. User brings each shelf/shop-managed part to the correct footprint. Check footprint ID, approved orientation, pin-one, polarity, value, price/ownership semantics and solder-joint condition. All component placement outcomes must be forwarded to the project's existing authority, including removal/recycling restrictions for electrically connected items.

Upon valid installation: show placement contact snap, limited solder/lighting VFX and updated physical pins; keep the repair status independent of the visuals. Reject wrong part, flipped orientation and collisions with clear UI reasons. Save/load shall preserve state and not silently auto-complete components.

## 06 — Ten-layer PCB routing and vias
Implement L1 through L10 viewing, selecting, pad picking, valid route previews, collision/clearance checks, net highlight, plane membership, cross-layer traversal and through/blind/buried via validation as specified by the **reviewed** stack-up. Need routed topology not merely a counter. W001–W140 is player-facing routing set; other full-board factory links are explicitly reviewed, not player tasks. V01–V42 require individually certified physical layer spans. A wire drawn in scene but shorted to an incorrect net must not count.

World-space UI: route selection, start pad, end pad, copper layer, permitted width, via insertion and undo/redo. All visual route colors are status indicators; the underlying graph owns correctness. Keep unverified source manifests as blocked until reconciliation.

## 07 — Simulated electrical supply and power sequencing
Model, in a bounded deterministic educational simulator, protected 12 V input, standby rail, 3.3 V auxiliary and relevant CPU/SoC 1.00 V nominal, DDR4 VDD ~1.20 V, DDR4 VPP ~2.50 V and other reviewed rails. Do not conflate CPU voltage with PCIe supply or assume voltages whose approved schematic is missing. Include regulator enable dependencies, ramp rates, ripple, tolerance, overcurrent/thermal and a true power-good delay. Represent state through actual solver outputs, not panel animations.

Power state transitions: `OFF -> STANDBY -> POWER_REQUESTED -> RAIL_RAMP -> RAILS_QUALIFIED -> RESET_RELEASED -> CPU_EARLY_BOOT`. Failure or brownout invalidates downstream state with appropriate retention and reset behavior. Expose test points to virtual multimeter/oscilloscope. No boot allowed on insufficient rail evidence.

## 08 — CPU, memory, firmware and storage simulation
Implement an educational **virtual system**, not a full physical 64-bit CPU emulator. CPU startup depends on clock, reset, rail validity and firmware access. DDR4 subsystem: two x16 devices (32-bit channel), four byte lanes, power and reset readiness, bounded read/write training, fault-sensitive training windows, data-pattern tests and temperature/noise influences. Make memory failures block subsequent memory-dependent firmware phases.

Model a verified SPI firmware image and firmware-loader reads with distinct missing contact/bridge/invalid-image failure causes. UEFI-inspired stages: `SEC -> PEI -> DXE -> BDS -> BOOTLOADER_HANDOFF`; these are game-appropriate conceptual stages, not execution of real third-party UEFI blobs. Expose first blocking stage, last successful event, boot transaction history and reset reason.

PCIe-style root complex: reuse Level 105 components if available; validate the diagnostic endpoint's link training and enumeration as a required Level 106 test. eMMC-style boot storage: verify interface contacts, enumerate media, read a game-owned boot image, verify checksum and hand off to a constrained simulated OS desktop in the world-space panel. Distinguish optional external devices from required diagnostic hardware. Do not implement real device drivers or claim compatibility with commercial PC hardware.

## 09 — POST visual console and player controls
Create polished world-space panels: power & rails, boot-stage timeline, POST code/event log, CPU/DDR4 readiness, firmware verification, PCIe enumeration, storage status, faults and diagnostic probes. The UI should never simultaneously show `0/106 installed` and `POST PASS` as if gameplay is complete; screenshot art is concept art, not a valid initial state.

State-driven visual design: dark navy/graphite with cyan accents, high legibility, disabled targets clearly identified, diagnostic waveform graphics driven by actual measurements. Controls: Inspect / Assemble / Route / Layer View / POST / Repair / Calibrate. Subtools should reuse existing UI toolkit: multimeter, oscilloscope, logic analyzer, POST analyzer, firmware programmer, DDR4 trainer, PCIe analyzer, storage tester, thermal scanner, net tracer, solder/desolder and component replacement. Select a tool in UI, tap/click real board contact target, raycast and validate, then run localized VFX; support mouse and mobile touches, pointer-over-UI filtering, camera orbit and unobstructed target selection.

## 10 — Three authoritative repair scenarios
**F1 `F1_CPU_PGOOD_TIMING`:** faulted power-good/reset resistor releases CPU reset before the qualified stable-rail interval. Oscilloscope shows premature edge, CPU early boot fails. Repair by correctly replacing the faulted resistor at the verified footprint; collect fresh sequence proof. Replacing an unrelated resistor or playing solder effects cannot fix it.

**F2 `F2_SPI_FLASH_CONTACT`:** solder bridge/defective pad corrupts firmware flash transaction. CPU power and reset remain valid, but firmware validation fails. User inspects correct SPI pins, uses net tracer and microscope, removes bridge/repairs contact via approved virtual action; the firmware read/verification must then pass.

**F3 `F3_eMMC_CONTACT`:** intermittent storage interface contact; early firmware and DDR4 can succeed but BDS/boot image access fails. User diagnoses contact and valid module presence, repairs/reseats approved storage connection, verifies image checksum and fresh boot.

Faults may be introduced during designated challenge stage after prior commissioning; define deterministic injection order and diagnostic evidence. On repair success, invalidate downstream cached certification where necessary, then require fresh full boot. Never erase unrelated proven historical objectives arbitrarily.

## 11 — Critical W141 feedback and fail-safe authorization
Source definition: `L106_SystemBootMonitor_U14.TRUSTED_SYSTEM_READY_OUT` -> W141 -> `L106_IndependentSafety_U15.TRUSTED_SYSTEM_READY_IN`. **STOP until U14 is mapped to a real BOM device and both pin assignments are reviewed.** W141 is a changing/freshness-checked system-ready signal, not the CPU VCORE path, firmware SPI or eMMC bus. On physical disconnect: graph opens the feedback circuit; trusted health becomes invalid; independent safety latches a `FeedbackLost` fault, withdraws protected workload permission, and retains permitted diagnostics. Actual power-rail voltages continue evolving by electrical rules, not forced to zero.

Recovery: (1) independently detect missing feedback, (2) verify safe inhibition, (3) physically reconnect W141 via the true endpoints, (4) rerun a fresh power/DDR4/firmware/PCIe/storage/boot qualification, (5) evaluate a newly received valid health sequence, (6) require explicit player-authorized reset, (7) perform final independent certification. A stale or stuck HIGH signal must never qualify. Log each transition and evidence timestamp, and handle interruption/re-save/load safely.

## 12 — Six objective predicates and authoritative progression
Objective 1: 106 correct installed components with approved footprints and contacts.
Objective 2: 140 designated routes + 42 vias, approved ten-layer connectivity, no dangerous shorts, W141 present for normal commissioning.
Objective 3: valid power rails/reset release/CPU early initialization and four trained DDR4 byte lanes.
Objective 4: verified virtual firmware/POST, PCIe enumeration and successful virtual eMMC boot handoff.
Objective 5: F1–F3 detected, repaired by approved interactions, and full fresh post-repair boot passes.
Objective 6: supervised W141 disconnection/safety inhibition, physical reconnection, fresh system-health evidence, explicit reset and successful independent final certification.

Only the real `LevelGamePlayManager` (or reviewed game integration adapter) owns the 0/6–6/6 status. Avoid a shadow objective manager that reports success from visuals. Objective evidence includes authoritative source state, test ID, timestamps, fault injection/repair IDs and invalidation reason. All predicates must be deterministic and testable. Do not award objectives while `ElectricalAuthorityStatus != Validated`.

## 13 — Safety, save/load and performance
Save serializable: placements, solder faults, copper segments, vias, jumper physical state, fault repairs, firmware virtual image ID, power/boot state, training evidence, last authorized safety reset, objective history and safe recovery checkpoint. On reload, never recreate live authority from an assembled reference. Validate against component manifest hash and fail safe if netlist revision is incompatible. Do not allow use of unowned items, cross-level item contamination or unsafe disposal of connected parts.

Use pooled contact VFX (sparks, smoke, solder glow, oscilloscope pulses), avoid material instancing leaks, excessive per-frame allocations and repeated FindObject calls. Support target 60 fps on a reasonable desktop and sensible mobile LOD settings. Touch hit targets may be larger than their visible solder pads but must route to exact intended electrical points.

## 14 — Proposed implementation classes (adapt names to actual APIs)
- `SparkLevel106Coordinator`: orchestrate authoritative state changes.
- `SparkLevel106BoardManifest`: source-derived asset/placement tables, content version.
- `SparkLevel106NetlistAuthority`: reviewed copper/pin/via graph binding.
- `SparkLevel106PowerSequencer`: electric rail and reset dependencies.
- `SparkLevel106CpuRuntime`: virtual CPU boot prerequisites.
- `SparkLevel106DdrTrainer`: four byte-lane calibration / memory tests.
- `SparkLevel106FirmwareBoot`: bounded UEFI-inspired stage runner.
- `SparkLevel106PcieAdapter`: diagnostic link + enumeration.
- `SparkLevel106StorageBoot`: eMMC-style virtual storage boot.
- `SparkLevel106FaultController`: F1/F2/F3 state and repair authority.
- `SparkLevel106W141Safety`: supervised feedback and fault latch.
- `SparkLevel106ObjectiveAdapter`: forwards evidence to existing manager.
- `SparkLevel106WorldSpaceUI`: panel view of real state.
- `SparkLevel106Builder`: non-destructive one-click Editor scene/prefab creation.

These names are suggestions, not claims that the project already exposes them. Use MonoBehaviour/ScriptableObject appropriately, assembly definitions and serialized references. For systems with existing equivalents, implement thin adapters instead of duplicating logic.

## 15 — Editor tools and scene production
Preserve and verify provided Editor menus:
- `Tools/Project Spark/Levels/106/1. Build EMPTY SFX-14 Board`
- `Tools/Project Spark/Levels/106/2. Configure FBX Import (Meters)`
- `Tools/Project Spark/Levels/106/3. Create PBR Starter Materials`

Add target complete gameplay builder: `Tools/Project Spark/Levels/Create Level 106`. It must be idempotent, provide dry-run validation results, create the bare board, import model references, generate relevant empty footprint anchors, wire UI references, find active project services safely and report missing dependencies. It may build an `ART_ONLY_UNCERTIFIED` preview when the netlist is unresolved, but it must **refuse gameplay certification**. No editor operation may destructively alter Levels 1–105. Provide Undo for created objects and scene dirty handling.

## 16 — Engineering simulation math and limits
Use stable numerical integration or bounded discrete-time logic; validate power sequencing and protected shutdown. Measurement models must be connected to actual simulated state rather than hardcoded UI values. For regulated rails, include output capacitance and load-dependent behavior in simplified form; expose tolerance values in configuration, not code constants. For DDR4 training, use parameterized eye/timing margins per byte lane with deterministic error injection, not real DDR4 silicon compliance claims. For PCIe, use bounded Gen2-style link state and endpoint enumeration without claiming standards-complete operation. For firmware, run virtual scripted boot stages, not arbitrary external firmware binaries.

Do not invent commercially accurate SFX-14 CPU pinouts: motherboard has no verified manufacturer device choices in source art. Clearly label all synthetic electrical values and simulation simplifications in inspector help text and technical documentation.

## 17 — Required acceptance-test categories
Write play/edit mode automated tests for netlist gates, art import hierarchy, placement, route/via graph, power and reset, firmware/POST, DDR4, PCIe, storage, faults, W141, saves and objectives. Include named tests below as the initial regression matrix; tailor to actual project API names. **Tests do not count as passed until executed**. End-to-end success conditions are all 106 placements installed legally, all 140 validated routes and 42 vias satisfied, all 6 authoritative objectives awarded, 3 fault repairs proven, W141 recovered and a fresh virtual system boot completed. Test no-availability of certification when schematic is unresolved.

## 18 — Required 100 engineering acceptance tests

### Import and source integrity
- **AT-001:** 136 modular model identities resolve.
- **AT-002:** 106 unique component refs.
- **AT-003:** no duplicate object roots on rebuild.
- **AT-004:** real-meter board dimensions respected.
- **AT-005:** Z-up to Unity Y-up conversion reviewed.
- **AT-006:** source art assembled board excluded from gameplay.
- **AT-007:** PBR material generator resolves nested install path.
- **AT-008:** model material slots valid.
- **AT-009:** PCB silkscreen aligned.
- **AT-010:** empty board starts without components.

### Netlist and routing authority
- **AT-011:** route rows blocked while unassigned.
- **AT-012:** via rows blocked while unassigned.
- **AT-013:** unknown net identifiers rejected.
- **AT-014:** orphan pad detection.
- **AT-015:** power-to-ground short rejection.
- **AT-016:** wrong-net connection rejection.
- **AT-017:** wrong layer route rejected.
- **AT-018:** through-via span obeyed.
- **AT-019:** blind via span obeyed.
- **AT-020:** missing via does not certify.
- **AT-021:** factory copper does not bypass player task.
- **AT-022:** route undo invalidates derived evidence.
- **AT-023:** netlist revision invalidates stale save.
- **AT-024:** non-certified decorative trace ignored.
- **AT-025:** W141 U14 missing BOM blocks safety certification.

### Component and assembly
- **AT-026:** correct footprint accepts correct component.
- **AT-027:** wrong footprint rejected.
- **AT-028:** wrong orientation rejected.
- **AT-029:** polarized reversal rejected.
- **AT-030:** disconnected uninstalled component does not conduct.
- **AT-031:** solder bridge creates fault.
- **AT-032:** bad pad causes open circuit.
- **AT-033:** removal of connected component requires authority.
- **AT-034:** ownership/shop constraints honored.
- **AT-035:** all 106 placements required.

### Power and reset
- **AT-036:** unpowered board cannot start.
- **AT-037:** standby rail qualifies separately.
- **AT-038:** 12V input missing inhibits startup.
- **AT-039:** faulted rail trips supervisor.
- **AT-040:** CPU VCORE outside tolerance blocks reset.
- **AT-041:** DDR rail outside tolerance blocks training.
- **AT-042:** reset timing fault recognized.
- **AT-043:** new power cycle refreshes evidence.
- **AT-044:** stored capacitor voltage decays physically.
- **AT-045:** healthy ramp allows reset.

### Firmware / CPU / DDR4
- **AT-046:** bad CPU clock blocks early boot.
- **AT-047:** valid firmware image required.
- **AT-048:** SPI contact failure corrupts firmware read.
- **AT-049:** SEC requires valid early conditions.
- **AT-050:** PEI requires usable memory.
- **AT-051:** DXE not reported before PEI.
- **AT-052:** BDS not reported before DXE.
- **AT-053:** byte lane 0 training varies by fault.
- **AT-054:** byte lane 1 training varies by fault.
- **AT-055:** byte lane 2 training varies by fault.
- **AT-056:** byte lane 3 training varies by fault.
- **AT-057:** memory-error test invalidates qualification.
- **AT-058:** fresh DDR evidence required.

### PCIe / storage / POST
- **AT-059:** unseated endpoint cannot enumerate.
- **AT-060:** bad PCIe lane prevents x4 certification.
- **AT-061:** valid endpoint can enumerate.
- **AT-062:** eMMC missing blocks required boot.
- **AT-063:** eMMC contact intermittence reproduces fault.
- **AT-064:** storage checksum mismatch reported.
- **AT-065:** system handoff requires valid image.
- **AT-066:** POST reports first blocking stage.
- **AT-067:** unrelated subsystem success does not mask failure.
- **AT-068:** full boot requires all required dependencies.

### Fault repair
- **AT-069:** F1 reproducible timing failure.
- **AT-070:** F1 unrelated resistor not accepted.
- **AT-071:** F1 replacement yields fresh valid reset.
- **AT-072:** F2 reproducible SPI failure.
- **AT-073:** F2 visual effect alone cannot repair.
- **AT-074:** F2 approved solder repair restores transactions.
- **AT-075:** F3 storage fault blocks boot image.
- **AT-076:** F3 approved repair restores storage.
- **AT-077:** repair invalidates stale boot evidence.
- **AT-078:** all three repairs logged.

### W141 and safety
- **AT-079:** physical W141 unplug opens actual graph.
- **AT-080:** loss triggers independent safety inhibition.
- **AT-081:** constant high heartbeat rejected.
- **AT-082:** stale feedback rejected.
- **AT-083:** protected workloads blocked while unplugged.
- **AT-084:** replug alone not sufficient.
- **AT-085:** fresh POST after reconnect required.
- **AT-086:** explicit reset required.
- **AT-087:** fault remains latched without reset.
- **AT-088:** W141 state survives save load.

### Objective and robustness
- **AT-089:** objective 1 depends on 106 authorized parts.
- **AT-090:** objective 2 depends on valid routes and vias.
- **AT-091:** objective 3 depends on CPU DDR qualification.
- **AT-092:** objective 4 depends on new boot.
- **AT-093:** objective 5 requires all fault repairs.
- **AT-094:** objective 6 requires W141 recovery.
- **AT-095:** 0/6 initial state.
- **AT-096:** 6/6 impossible with unresolved schematic.
- **AT-097:** UI does not directly award objective.
- **AT-098:** save load preserves authority.
- **AT-099:** repair touch and mouse use same validation.
- **AT-100:** VFX cannot mutate solver on their own.

## 19 — Delivery contract for Unity implementation
Deliver: compiling Unity source files; generated ScriptableObject configs and complete project-compatible scene/prefab; inspector wiring validation; reviewed pin-level schematic and netlist or a prominently blocked uncertified state; full routes/vias and BOM reconciliation report; material/FBX import report; virtual tool integration; POST/DDR4/PCIe/storage UI; all 100 automated tests; actual edit-mode/play-mode logs; QA walkthrough; art-performance notes; precise lists of still-unverified behavior. Prefer complete replaceable source files with paths if modifying existing code. Do not invent successful test results.

Acceptance checkpoint is a **fresh** 6/6 using real `LevelGamePlayManager`, not a decorative screenshot or a fake success event. Do not create another room, a physical instrument set or unrelated images.

## 20 — Immutable source annexes
The following records are included verbatim from the supplied Level 106 archive. Treat them as asset and planning data, not an approved physical schematic.

### Annex A — Complete model manifest (136)
**Source:** `Data/L106_Models_136.csv`

```csv
asset_name,asset_type,assembled_reference_only,gameplay_certifiable,x_m,y_m,z_m
001_L106_U1,CPU,True,False,0,0,0
002_L106_U2,DDR,True,False,0.087,0.05,0
003_L106_U3,DDR,True,False,0.087,0.023,0
004_L106_IC1,IC,True,False,-0.051,0.052,0
005_L106_IC2,IC,True,False,-0.033,-0.052,0
006_L106_IC3,IC,True,False,0.029,0.066,0
007_L106_IC4,IC,True,False,0.038,-0.044,0
008_L106_IC5,IC,True,False,0.106,0.065,0
009_L106_U7,STORAGE,True,False,0.033,-0.043,0
010_L106_Y1,OSC,True,False,-0.033,0.075,0
011_L106_U6,FLASH,True,False,-0.05,-0.055,0
012_L106_U5,EC,True,False,-0.099,-0.038,0
013_L106_U8,IO,True,False,0.068,-0.026,0
014_L106_U9,IO,True,False,0.115,-0.015,0
015_L106_REG1,REG,True,False,-0.076,-0.017,0
016_L106_REG2,REG,True,False,0.068,-0.071,0
017_L106_U15,SAFETY,True,False,0.075,-0.035,0
018_L106_TEMP1,TEMP,True,False,-0.025,0.071,0
019_L106_RES1,RES,True,False,0.11684034824006462,-0.03269846767461562,0
020_L106_RES2,RES,True,False,0.04848974587585267,-0.05555441819948695,0
021_L106_RES3,RES,True,False,0.11119948800105678,0.07086326316325599,0
022_L106_RES4,RES,True,False,-0.06759429573633305,-0.08165816687106617,0
023_L106_RES5,RES,True,False,0.00415358594624346,0.05493377115142582,0
024_L106_RES6,RES,True,False,0.06201660141923873,-0.03496158408005589,0
025_L106_RES7,RES,True,False,-0.014080085807122444,0.056828474110786634,0
026_L106_RES8,RES,True,False,-0.053465546834239,0.0711053706748943,0
027_L106_RES9,RES,True,False,0.0027361027895712875,0.08290466576324913,0
028_L106_RES10,RES,True,False,-0.007849718227046817,0.04348678274308186,0
029_L106_RES11,RES,True,False,-0.11974661097475776,0.0015711155636323193,0
030_L106_RES12,RES,True,False,0.09123014587231548,-0.03495386819741323,0
031_L106_RES13,RES,True,False,0.10192754424988748,0.006887798066809697,0
032_L106_RES14,RES,True,False,-0.0661315375199055,0.06269807328295153,0
033_L106_RES15,RES,True,False,-0.11595850279556187,-0.007955855176626503,0
034_L106_RES16,RES,True,False,-0.03313919780218223,-0.04100440905553853,0
035_L106_RES17,RES,True,False,-0.006409457826104678,0.0821408534751699,0
036_L106_RES18,RES,True,False,0.09255868318407551,-0.022653856353627023,0
037_L106_RES19,RES,True,False,-0.11094769944322269,0.04108722879073505,0
038_L106_RES20,RES,True,False,-0.06839175859872709,-0.06359267108988433,0
039_L106_RES21,RES,True,False,0.013774439932411675,-0.053781152562135955,0
040_L106_RES22,RES,True,False,-0.05887295626879627,-0.01173775217914623,0
041_L106_RES23,RES,True,False,-0.10920988840960172,-0.07007030947816281,0
042_L106_RES24,RES,True,False,0.03061447941696005,0.019716682820752313,0
043_L106_RES25,RES,True,False,0.05554374956431299,0.06056575609704885,0
044_L106_RES26,RES,True,False,-0.07912067181190835,-0.04791527133213175,0
045_L106_RES27,RES,True,False,-0.04651220320863875,0.04627670998783277,0
046_L106_RES28,RES,True,False,0.11831528626013499,0.01785633887610459,0
047_L106_CAP1,CAP,True,False,-0.11587174211405811,-0.06111992348943635,0
048_L106_CAP2,CAP,True,False,0.04353344293534231,0.005788747107114536,0
049_L106_CAP3,CAP,True,False,-0.007903325622432428,0.050466585590433974,0
050_L106_CAP4,CAP,True,False,-0.016058329456701464,0.04355255560357284,0
051_L106_CAP5,CAP,True,False,0.11069489537717511,-0.08009470678215605,0
052_L106_CAP6,CAP,True,False,0.03436583675353655,0.008669005527943405,0
053_L106_CAP7,CAP,True,False,-0.10129311481375744,-0.0276567528861004,0
054_L106_CAP8,CAP,True,False,0.03478122105010342,0.0760695294741318,0
055_L106_CAP9,CAP,True,False,0.03352270325164569,-0.057738554450790756,0
056_L106_CAP10,CAP,True,False,0.035600951505955686,0.05322099510656721,0
057_L106_CAP11,CAP,True,False,0.03775751666568608,0.0656377117562824,0
058_L106_CAP12,CAP,True,False,-0.05555365207342568,-0.03414845414484659,0
059_L106_CAP13,CAP,True,False,-0.0453872694202635,0.07948244240059955,0
060_L106_CAP14,CAP,True,False,0.006746945035329938,-0.04204537222836785,0
061_L106_CAP15,CAP,True,False,0.043547998743472094,-0.030753784469152626,0
062_L106_CAP16,CAP,True,False,-0.04007565890943875,-0.04485601681112693,0
063_L106_CAP17,CAP,True,False,-0.1169864210517808,-0.025440988846807393,0
064_L106_CAP18,CAP,True,False,-0.0962741744565406,-0.05299004657415465,0
065_L106_CAP19,CAP,True,False,0.061451994326046505,0.011852486824441996,0
066_L106_CAP20,CAP,True,False,-0.020223711605708156,-0.05886876546845064,0
067_L106_CAP21,CAP,True,False,0.03377303336057019,0.028952738150106502,0
068_L106_CAP22,CAP,True,False,0.11010404316922598,-0.007008020468944859,0
069_L106_CAP23,CAP,True,False,-0.08553551201469611,-0.06110820693987326,0
070_L106_CAP24,CAP,True,False,0.11378274233103461,0.03255271816263826,0
071_L106_CAP25,CAP,True,False,-0.06969562176308025,-0.003355226141912457,0
072_L106_CAP26,CAP,True,False,0.06401391754540867,0.031889316336322956,0
073_L106_CAP27,CAP,True,False,-0.09650021480329835,-0.021431128352017575,0
074_L106_CAP28,CAP,True,False,0.06881002756117963,-0.05727887315342493,0
075_L106_CAP29,CAP,True,False,0.0037914287958353554,0.07672039108131283,0
076_L106_CAP30,CAP,True,False,-0.00789144256066146,-0.04484616450662805,0
077_L106_INDUCTOR1,INDUCTOR,True,False,-0.095,0.067,0
078_L106_INDUCTOR2,INDUCTOR,True,False,-0.095,0.047,0
079_L106_INDUCTOR3,INDUCTOR,True,False,-0.095,0.027,0
080_L106_INDUCTOR4,INDUCTOR,True,False,-0.095,0.007,0
081_L106_INDUCTOR5,INDUCTOR,True,False,-0.061,0.072,0
082_L106_INDUCTOR6,INDUCTOR,True,False,-0.061,0.052,0
083_L106_MOSFET1,MOSFET,True,False,-0.115,0.071,0
084_L106_MOSFET2,MOSFET,True,False,-0.114,0.055,0
085_L106_MOSFET3,MOSFET,True,False,-0.115,0.036,0
086_L106_MOSFET4,MOSFET,True,False,-0.114,0.018,0
087_L106_MOSFET5,MOSFET,True,False,-0.076,0.081,0
088_L106_MOSFET6,MOSFET,True,False,-0.048,0.081,0
089_L106_CONNECTOR1,CONNECTOR,True,False,-0.121,0.045,0
090_L106_CONNECTOR2,CONNECTOR,True,False,-0.122,-0.067,0
091_L106_CONNECTOR3,CONNECTOR,True,False,0.122,0.081,0
092_L106_CONNECTOR4,CONNECTOR,True,False,0.124,0.011,0
093_L106_J1,PCIE,True,False,0.018,-0.079,0
094_L106_LED1,LED,True,False,0.055,-0.05,0
095_L106_LED2,LED,True,False,0.068,-0.05,0
096_L106_LED3,LED,True,False,0.081,-0.05,0
097_L106_LED4,LED,True,False,0.094,-0.05,0
098_L106_SWITCH1,SWITCH,True,False,-0.117,-0.078,0
099_L106_SWITCH2,SWITCH,True,False,-0.078,-0.077,0
100_L106_SWITCH3,SWITCH,True,False,0.126,-0.081,0
101_L106_DIODE1,DIODE,True,False,0.06809957673434008,0.056238660787534075,0
102_L106_DIODE2,DIODE,True,False,0.08300064664450532,-0.012679925420656116,0
103_L106_DIODE3,DIODE,True,False,-0.07913161083360988,-0.03247422977398018,0
104_L106_FERRITE1,FERRITE,True,False,0.09574740386013686,-0.00748829889022784,0
105_L106_FERRITE2,FERRITE,True,False,0.10876625790436537,-0.04393195932881985,0
106_L106_W141,JUMPER,True,False,0.112,-0.057,0
107_L106_SFX14_BarePCB,BOARD,False,False,0,0,0
108_L106_Copper_L01_Reference,REFERENCE_LAYER,True,False,0,0,0
109_L106_Copper_L02_Reference,REFERENCE_LAYER,True,False,0,0,0
110_L106_Copper_L03_Reference,REFERENCE_LAYER,True,False,0,0,0
111_L106_Copper_L04_Reference,REFERENCE_LAYER,True,False,0,0,0
112_L106_Copper_L05_Reference,REFERENCE_LAYER,True,False,0,0,0
113_L106_Copper_L06_Reference,REFERENCE_LAYER,True,False,0,0,0
114_L106_Copper_L07_Reference,REFERENCE_LAYER,True,False,0,0,0
115_L106_Copper_L08_Reference,REFERENCE_LAYER,True,False,0,0,0
116_L106_Copper_L09_Reference,REFERENCE_LAYER,True,False,0,0,0
117_L106_Copper_L10_Reference,REFERENCE_LAYER,True,False,0,0,0
118_L106_CPU_Socket_Frame,SOCKET,False,False,0,0,0
119_L106_CPU_HeatSpreader,CPU_HARDWARE,False,False,0,0,0
120_L106_PCIe_Socket_Detail,CONNECTOR_DETAIL,True,False,0.018,-0.079,0
121_L106_DDR4_Footprint_U2,BGA_FOOTPRINT,True,False,0.087,0.05,0
122_L106_DDR4_Footprint_U3,BGA_FOOTPRINT,True,False,0.087,0.023,0
123_L106_SPI_Flash_Footprint,FLASH_FOOTPRINT,True,False,-0.05,-0.055,0
124_L106_Memory_TraceDetail_1,DECORATIVE,True,False,0,0,0
125_L106_Memory_TraceDetail_2,DECORATIVE,True,False,0,0,0
126_L106_Memory_TraceDetail_3,DECORATIVE,True,False,0,0,0
127_L106_GroundVia,VIA_REFERENCE,True,False,0,0,0
128_L106_PowerVia,VIA_REFERENCE,True,False,0,0,0
129_L106_SignalVia,VIA_REFERENCE,True,False,0,0,0
130_L106_CPU_PowerGood_Fault,FAULT_OVERLAY,True,False,0,0,0
131_L106_SPI_SolderBridge_Fault,FAULT_OVERLAY,True,False,0,0,0
132_L106_Storage_Contact_Fault,FAULT_OVERLAY,True,False,0,0,0
133_L106_12V_Input_TestPad,TESTPAD,True,False,0,0,0
134_L106_VCORE_TestPad,TESTPAD,True,False,0,0,0
135_L106_DDR_VDD_TestPad,TESTPAD,True,False,0,0,0
136_L106_W141_Safety_TestPad,TESTPAD,True,False,0,0,0
```

### Annex B — Component placement/BOM manifest (106)
**Source:** `Data/L106_Components_106.csv`

```csv
model,ref,kind,board,x_mm,y_mm,z_mm,orientation_deg,installed_at_start,electrical_authority,footprint_status
001_L106_U1,U1,CPU,SFX-14,0,0,0,0,False,UNVERIFIED_PIN_LEVEL,ART_PLACEHOLDER
002_L106_U2,U2,DDR,SFX-14,87.0,50.0,0,0,False,UNVERIFIED_PIN_LEVEL,ART_PLACEHOLDER
003_L106_U3,U3,DDR,SFX-14,87.0,23.0,0,0,False,UNVERIFIED_PIN_LEVEL,ART_PLACEHOLDER
004_L106_IC1,IC1,IC,SFX-14,-51.0,52.0,0,0,False,UNVERIFIED_PIN_LEVEL,ART_PLACEHOLDER
005_L106_IC2,IC2,IC,SFX-14,-33.0,-52.0,0,0,False,UNVERIFIED_PIN_LEVEL,ART_PLACEHOLDER
006_L106_IC3,IC3,IC,SFX-14,29.0,66.0,0,0,False,UNVERIFIED_PIN_LEVEL,ART_PLACEHOLDER
007_L106_IC4,IC4,IC,SFX-14,38.0,-44.0,0,0,False,UNVERIFIED_PIN_LEVEL,ART_PLACEHOLDER
008_L106_IC5,IC5,IC,SFX-14,106.0,65.0,0,0,False,UNVERIFIED_PIN_LEVEL,ART_PLACEHOLDER
009_L106_U7,U7,STORAGE,SFX-14,33.0,-43.0,0,0,False,UNVERIFIED_PIN_LEVEL,ART_PLACEHOLDER
010_L106_Y1,Y1,OSC,SFX-14,-33.0,75.0,0,0,False,UNVERIFIED_PIN_LEVEL,ART_PLACEHOLDER
011_L106_U6,U6,FLASH,SFX-14,-50.0,-55.0,0,0,False,UNVERIFIED_PIN_LEVEL,ART_PLACEHOLDER
012_L106_U5,U5,EC,SFX-14,-99.0,-38.0,0,0,False,UNVERIFIED_PIN_LEVEL,ART_PLACEHOLDER
013_L106_U8,U8,IO,SFX-14,68.0,-26.0,0,0,False,UNVERIFIED_PIN_LEVEL,ART_PLACEHOLDER
014_L106_U9,U9,IO,SFX-14,115.0,-15.0,0,0,False,UNVERIFIED_PIN_LEVEL,ART_PLACEHOLDER
015_L106_REG1,REG1,REG,SFX-14,-76.0,-17.0,0,0,False,UNVERIFIED_PIN_LEVEL,ART_PLACEHOLDER
016_L106_REG2,REG2,REG,SFX-14,68.0,-71.0,0,0,False,UNVERIFIED_PIN_LEVEL,ART_PLACEHOLDER
017_L106_U15,U15,SAFETY,SFX-14,75.0,-35.0,0,0,False,UNVERIFIED_PIN_LEVEL,ART_PLACEHOLDER
018_L106_TEMP1,TEMP1,TEMP,SFX-14,-25.0,71.0,0,0,False,UNVERIFIED_PIN_LEVEL,ART_PLACEHOLDER
019_L106_RES1,RES1,RES,SFX-14,116.84,-32.698,0,0,False,UNVERIFIED_PIN_LEVEL,ART_PLACEHOLDER
020_L106_RES2,RES2,RES,SFX-14,48.49,-55.554,0,0,False,UNVERIFIED_PIN_LEVEL,ART_PLACEHOLDER
021_L106_RES3,RES3,RES,SFX-14,111.199,70.863,0,0,False,UNVERIFIED_PIN_LEVEL,ART_PLACEHOLDER
022_L106_RES4,RES4,RES,SFX-14,-67.594,-81.658,0,0,False,UNVERIFIED_PIN_LEVEL,ART_PLACEHOLDER
023_L106_RES5,RES5,RES,SFX-14,4.154,54.934,0,0,False,UNVERIFIED_PIN_LEVEL,ART_PLACEHOLDER
024_L106_RES6,RES6,RES,SFX-14,62.017,-34.962,0,0,False,UNVERIFIED_PIN_LEVEL,ART_PLACEHOLDER
025_L106_RES7,RES7,RES,SFX-14,-14.08,56.828,0,0,False,UNVERIFIED_PIN_LEVEL,ART_PLACEHOLDER
026_L106_RES8,RES8,RES,SFX-14,-53.466,71.105,0,0,False,UNVERIFIED_PIN_LEVEL,ART_PLACEHOLDER
027_L106_RES9,RES9,RES,SFX-14,2.736,82.905,0,0,False,UNVERIFIED_PIN_LEVEL,ART_PLACEHOLDER
028_L106_RES10,RES10,RES,SFX-14,-7.85,43.487,0,0,False,UNVERIFIED_PIN_LEVEL,ART_PLACEHOLDER
029_L106_RES11,RES11,RES,SFX-14,-119.747,1.571,0,0,False,UNVERIFIED_PIN_LEVEL,ART_PLACEHOLDER
030_L106_RES12,RES12,RES,SFX-14,91.23,-34.954,0,0,False,UNVERIFIED_PIN_LEVEL,ART_PLACEHOLDER
031_L106_RES13,RES13,RES,SFX-14,101.928,6.888,0,0,False,UNVERIFIED_PIN_LEVEL,ART_PLACEHOLDER
032_L106_RES14,RES14,RES,SFX-14,-66.132,62.698,0,0,False,UNVERIFIED_PIN_LEVEL,ART_PLACEHOLDER
033_L106_RES15,RES15,RES,SFX-14,-115.959,-7.956,0,0,False,UNVERIFIED_PIN_LEVEL,ART_PLACEHOLDER
034_L106_RES16,RES16,RES,SFX-14,-33.139,-41.004,0,0,False,UNVERIFIED_PIN_LEVEL,ART_PLACEHOLDER
035_L106_RES17,RES17,RES,SFX-14,-6.409,82.141,0,0,False,UNVERIFIED_PIN_LEVEL,ART_PLACEHOLDER
036_L106_RES18,RES18,RES,SFX-14,92.559,-22.654,0,0,False,UNVERIFIED_PIN_LEVEL,ART_PLACEHOLDER
037_L106_RES19,RES19,RES,SFX-14,-110.948,41.087,0,0,False,UNVERIFIED_PIN_LEVEL,ART_PLACEHOLDER
038_L106_RES20,RES20,RES,SFX-14,-68.392,-63.593,0,0,False,UNVERIFIED_PIN_LEVEL,ART_PLACEHOLDER
039_L106_RES21,RES21,RES,SFX-14,13.774,-53.781,0,0,False,UNVERIFIED_PIN_LEVEL,ART_PLACEHOLDER
040_L106_RES22,RES22,RES,SFX-14,-58.873,-11.738,0,0,False,UNVERIFIED_PIN_LEVEL,ART_PLACEHOLDER
041_L106_RES23,RES23,RES,SFX-14,-109.21,-70.07,0,0,False,UNVERIFIED_PIN_LEVEL,ART_PLACEHOLDER
042_L106_RES24,RES24,RES,SFX-14,30.614,19.717,0,0,False,UNVERIFIED_PIN_LEVEL,ART_PLACEHOLDER
043_L106_RES25,RES25,RES,SFX-14,55.544,60.566,0,0,False,UNVERIFIED_PIN_LEVEL,ART_PLACEHOLDER
044_L106_RES26,RES26,RES,SFX-14,-79.121,-47.915,0,0,False,UNVERIFIED_PIN_LEVEL,ART_PLACEHOLDER
045_L106_RES27,RES27,RES,SFX-14,-46.512,46.277,0,0,False,UNVERIFIED_PIN_LEVEL,ART_PLACEHOLDER
046_L106_RES28,RES28,RES,SFX-14,118.315,17.856,0,0,False,UNVERIFIED_PIN_LEVEL,ART_PLACEHOLDER
047_L106_CAP1,CAP1,CAP,SFX-14,-115.872,-61.12,0,0,False,UNVERIFIED_PIN_LEVEL,ART_PLACEHOLDER
048_L106_CAP2,CAP2,CAP,SFX-14,43.533,5.789,0,0,False,UNVERIFIED_PIN_LEVEL,ART_PLACEHOLDER
049_L106_CAP3,CAP3,CAP,SFX-14,-7.903,50.467,0,0,False,UNVERIFIED_PIN_LEVEL,ART_PLACEHOLDER
050_L106_CAP4,CAP4,CAP,SFX-14,-16.058,43.553,0,0,False,UNVERIFIED_PIN_LEVEL,ART_PLACEHOLDER
051_L106_CAP5,CAP5,CAP,SFX-14,110.695,-80.095,0,0,False,UNVERIFIED_PIN_LEVEL,ART_PLACEHOLDER
052_L106_CAP6,CAP6,CAP,SFX-14,34.366,8.669,0,0,False,UNVERIFIED_PIN_LEVEL,ART_PLACEHOLDER
053_L106_CAP7,CAP7,CAP,SFX-14,-101.293,-27.657,0,0,False,UNVERIFIED_PIN_LEVEL,ART_PLACEHOLDER
054_L106_CAP8,CAP8,CAP,SFX-14,34.781,76.07,0,0,False,UNVERIFIED_PIN_LEVEL,ART_PLACEHOLDER
055_L106_CAP9,CAP9,CAP,SFX-14,33.523,-57.739,0,0,False,UNVERIFIED_PIN_LEVEL,ART_PLACEHOLDER
056_L106_CAP10,CAP10,CAP,SFX-14,35.601,53.221,0,0,False,UNVERIFIED_PIN_LEVEL,ART_PLACEHOLDER
057_L106_CAP11,CAP11,CAP,SFX-14,37.758,65.638,0,0,False,UNVERIFIED_PIN_LEVEL,ART_PLACEHOLDER
058_L106_CAP12,CAP12,CAP,SFX-14,-55.554,-34.148,0,0,False,UNVERIFIED_PIN_LEVEL,ART_PLACEHOLDER
059_L106_CAP13,CAP13,CAP,SFX-14,-45.387,79.482,0,0,False,UNVERIFIED_PIN_LEVEL,ART_PLACEHOLDER
060_L106_CAP14,CAP14,CAP,SFX-14,6.747,-42.045,0,0,False,UNVERIFIED_PIN_LEVEL,ART_PLACEHOLDER
061_L106_CAP15,CAP15,CAP,SFX-14,43.548,-30.754,0,0,False,UNVERIFIED_PIN_LEVEL,ART_PLACEHOLDER
062_L106_CAP16,CAP16,CAP,SFX-14,-40.076,-44.856,0,0,False,UNVERIFIED_PIN_LEVEL,ART_PLACEHOLDER
063_L106_CAP17,CAP17,CAP,SFX-14,-116.986,-25.441,0,0,False,UNVERIFIED_PIN_LEVEL,ART_PLACEHOLDER
064_L106_CAP18,CAP18,CAP,SFX-14,-96.274,-52.99,0,0,False,UNVERIFIED_PIN_LEVEL,ART_PLACEHOLDER
065_L106_CAP19,CAP19,CAP,SFX-14,61.452,11.852,0,0,False,UNVERIFIED_PIN_LEVEL,ART_PLACEHOLDER
066_L106_CAP20,CAP20,CAP,SFX-14,-20.224,-58.869,0,0,False,UNVERIFIED_PIN_LEVEL,ART_PLACEHOLDER
067_L106_CAP21,CAP21,CAP,SFX-14,33.773,28.953,0,0,False,UNVERIFIED_PIN_LEVEL,ART_PLACEHOLDER
068_L106_CAP22,CAP22,CAP,SFX-14,110.104,-7.008,0,0,False,UNVERIFIED_PIN_LEVEL,ART_PLACEHOLDER
069_L106_CAP23,CAP23,CAP,SFX-14,-85.536,-61.108,0,0,False,UNVERIFIED_PIN_LEVEL,ART_PLACEHOLDER
070_L106_CAP24,CAP24,CAP,SFX-14,113.783,32.553,0,0,False,UNVERIFIED_PIN_LEVEL,ART_PLACEHOLDER
071_L106_CAP25,CAP25,CAP,SFX-14,-69.696,-3.355,0,0,False,UNVERIFIED_PIN_LEVEL,ART_PLACEHOLDER
072_L106_CAP26,CAP26,CAP,SFX-14,64.014,31.889,0,0,False,UNVERIFIED_PIN_LEVEL,ART_PLACEHOLDER
073_L106_CAP27,CAP27,CAP,SFX-14,-96.5,-21.431,0,0,False,UNVERIFIED_PIN_LEVEL,ART_PLACEHOLDER
074_L106_CAP28,CAP28,CAP,SFX-14,68.81,-57.279,0,0,False,UNVERIFIED_PIN_LEVEL,ART_PLACEHOLDER
075_L106_CAP29,CAP29,CAP,SFX-14,3.791,76.72,0,0,False,UNVERIFIED_PIN_LEVEL,ART_PLACEHOLDER
076_L106_CAP30,CAP30,CAP,SFX-14,-7.891,-44.846,0,0,False,UNVERIFIED_PIN_LEVEL,ART_PLACEHOLDER
077_L106_INDUCTOR1,INDUCTOR1,INDUCTOR,SFX-14,-95.0,67.0,0,0,False,UNVERIFIED_PIN_LEVEL,ART_PLACEHOLDER
078_L106_INDUCTOR2,INDUCTOR2,INDUCTOR,SFX-14,-95.0,47.0,0,0,False,UNVERIFIED_PIN_LEVEL,ART_PLACEHOLDER
079_L106_INDUCTOR3,INDUCTOR3,INDUCTOR,SFX-14,-95.0,27.0,0,0,False,UNVERIFIED_PIN_LEVEL,ART_PLACEHOLDER
080_L106_INDUCTOR4,INDUCTOR4,INDUCTOR,SFX-14,-95.0,7.0,0,0,False,UNVERIFIED_PIN_LEVEL,ART_PLACEHOLDER
081_L106_INDUCTOR5,INDUCTOR5,INDUCTOR,SFX-14,-61.0,72.0,0,0,False,UNVERIFIED_PIN_LEVEL,ART_PLACEHOLDER
082_L106_INDUCTOR6,INDUCTOR6,INDUCTOR,SFX-14,-61.0,52.0,0,0,False,UNVERIFIED_PIN_LEVEL,ART_PLACEHOLDER
083_L106_MOSFET1,MOSFET1,MOSFET,SFX-14,-115.0,71.0,0,0,False,UNVERIFIED_PIN_LEVEL,ART_PLACEHOLDER
084_L106_MOSFET2,MOSFET2,MOSFET,SFX-14,-114.0,55.0,0,0,False,UNVERIFIED_PIN_LEVEL,ART_PLACEHOLDER
085_L106_MOSFET3,MOSFET3,MOSFET,SFX-14,-115.0,36.0,0,0,False,UNVERIFIED_PIN_LEVEL,ART_PLACEHOLDER
086_L106_MOSFET4,MOSFET4,MOSFET,SFX-14,-114.0,18.0,0,0,False,UNVERIFIED_PIN_LEVEL,ART_PLACEHOLDER
087_L106_MOSFET5,MOSFET5,MOSFET,SFX-14,-76.0,81.0,0,0,False,UNVERIFIED_PIN_LEVEL,ART_PLACEHOLDER
088_L106_MOSFET6,MOSFET6,MOSFET,SFX-14,-48.0,81.0,0,0,False,UNVERIFIED_PIN_LEVEL,ART_PLACEHOLDER
089_L106_CONNECTOR1,CONNECTOR1,CONNECTOR,SFX-14,-121.0,45.0,0,0,False,UNVERIFIED_PIN_LEVEL,ART_PLACEHOLDER
090_L106_CONNECTOR2,CONNECTOR2,CONNECTOR,SFX-14,-122.0,-67.0,0,0,False,UNVERIFIED_PIN_LEVEL,ART_PLACEHOLDER
091_L106_CONNECTOR3,CONNECTOR3,CONNECTOR,SFX-14,122.0,81.0,0,0,False,UNVERIFIED_PIN_LEVEL,ART_PLACEHOLDER
092_L106_CONNECTOR4,CONNECTOR4,CONNECTOR,SFX-14,124.0,11.0,0,0,False,UNVERIFIED_PIN_LEVEL,ART_PLACEHOLDER
093_L106_J1,J1,PCIE,SFX-14,18.0,-79.0,0,0,False,UNVERIFIED_PIN_LEVEL,ART_PLACEHOLDER
094_L106_LED1,LED1,LED,SFX-14,55.0,-50.0,0,0,False,UNVERIFIED_PIN_LEVEL,ART_PLACEHOLDER
095_L106_LED2,LED2,LED,SFX-14,68.0,-50.0,0,0,False,UNVERIFIED_PIN_LEVEL,ART_PLACEHOLDER
096_L106_LED3,LED3,LED,SFX-14,81.0,-50.0,0,0,False,UNVERIFIED_PIN_LEVEL,ART_PLACEHOLDER
097_L106_LED4,LED4,LED,SFX-14,94.0,-50.0,0,0,False,UNVERIFIED_PIN_LEVEL,ART_PLACEHOLDER
098_L106_SWITCH1,SWITCH1,SWITCH,SFX-14,-117.0,-78.0,0,0,False,UNVERIFIED_PIN_LEVEL,ART_PLACEHOLDER
099_L106_SWITCH2,SWITCH2,SWITCH,SFX-14,-78.0,-77.0,0,0,False,UNVERIFIED_PIN_LEVEL,ART_PLACEHOLDER
100_L106_SWITCH3,SWITCH3,SWITCH,SFX-14,126.0,-81.0,0,0,False,UNVERIFIED_PIN_LEVEL,ART_PLACEHOLDER
101_L106_DIODE1,DIODE1,DIODE,SFX-14,68.1,56.239,0,0,False,UNVERIFIED_PIN_LEVEL,ART_PLACEHOLDER
102_L106_DIODE2,DIODE2,DIODE,SFX-14,83.001,-12.68,0,0,False,UNVERIFIED_PIN_LEVEL,ART_PLACEHOLDER
103_L106_DIODE3,DIODE3,DIODE,SFX-14,-79.132,-32.474,0,0,False,UNVERIFIED_PIN_LEVEL,ART_PLACEHOLDER
104_L106_FERRITE1,FERRITE1,FERRITE,SFX-14,95.747,-7.488,0,0,False,UNVERIFIED_PIN_LEVEL,ART_PLACEHOLDER
105_L106_FERRITE2,FERRITE2,FERRITE,SFX-14,108.766,-43.932,0,0,False,UNVERIFIED_PIN_LEVEL,ART_PLACEHOLDER
106_L106_W141,W141,JUMPER,SFX-14,112.0,-57.0,0,0,False,UNVERIFIED_PIN_LEVEL,ART_PLACEHOLDER
```

### Annex C — All 140 proposed routes, uncertified
**Source:** `Data/L106_Routes_140_PLANNING.csv`

```csv
id,subsystem,start_pin,end_pin,net_id,certification,starts_completed
W001,POWER_INPUT_STANDBY,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W002,POWER_INPUT_STANDBY,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W003,POWER_INPUT_STANDBY,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W004,POWER_INPUT_STANDBY,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W005,POWER_INPUT_STANDBY,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W006,POWER_INPUT_STANDBY,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W007,POWER_INPUT_STANDBY,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W008,POWER_INPUT_STANDBY,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W009,POWER_INPUT_STANDBY,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W010,POWER_INPUT_STANDBY,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W011,POWER_INPUT_STANDBY,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W012,POWER_INPUT_STANDBY,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W013,POWER_INPUT_STANDBY,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W014,POWER_INPUT_STANDBY,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W015,POWER_INPUT_STANDBY,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W016,POWER_INPUT_STANDBY,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W017,POWER_INPUT_STANDBY,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W018,POWER_INPUT_STANDBY,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W019,POWER_INPUT_STANDBY,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W020,POWER_INPUT_STANDBY,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W021,CPU_VRM_RESET,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W022,CPU_VRM_RESET,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W023,CPU_VRM_RESET,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W024,CPU_VRM_RESET,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W025,CPU_VRM_RESET,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W026,CPU_VRM_RESET,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W027,CPU_VRM_RESET,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W028,CPU_VRM_RESET,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W029,CPU_VRM_RESET,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W030,CPU_VRM_RESET,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W031,CPU_VRM_RESET,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W032,CPU_VRM_RESET,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W033,CPU_VRM_RESET,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W034,CPU_VRM_RESET,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W035,CPU_VRM_RESET,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W036,CPU_VRM_RESET,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W037,CPU_VRM_RESET,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W038,CPU_VRM_RESET,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W039,CPU_VRM_RESET,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W040,CPU_VRM_RESET,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W041,CPU_VRM_RESET,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W042,CPU_VRM_RESET,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W043,DDR4,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W044,DDR4,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W045,DDR4,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W046,DDR4,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W047,DDR4,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W048,DDR4,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W049,DDR4,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W050,DDR4,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W051,DDR4,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W052,DDR4,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W053,DDR4,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W054,DDR4,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W055,DDR4,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W056,DDR4,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W057,DDR4,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W058,DDR4,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W059,DDR4,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W060,DDR4,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W061,DDR4,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W062,DDR4,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W063,DDR4,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W064,DDR4,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W065,SPI_FIRMWARE_EC,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W066,SPI_FIRMWARE_EC,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W067,SPI_FIRMWARE_EC,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W068,SPI_FIRMWARE_EC,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W069,SPI_FIRMWARE_EC,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W070,SPI_FIRMWARE_EC,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W071,SPI_FIRMWARE_EC,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W072,SPI_FIRMWARE_EC,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W073,SPI_FIRMWARE_EC,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W074,SPI_FIRMWARE_EC,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W075,SPI_FIRMWARE_EC,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W076,SPI_FIRMWARE_EC,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W077,SPI_FIRMWARE_EC,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W078,SPI_FIRMWARE_EC,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W079,SPI_FIRMWARE_EC,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W080,SPI_FIRMWARE_EC,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W081,SPI_FIRMWARE_EC,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W082,SPI_FIRMWARE_EC,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W083,SPI_FIRMWARE_EC,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W084,SPI_FIRMWARE_EC,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W085,SPI_FIRMWARE_EC,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W086,SPI_FIRMWARE_EC,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W087,PCIE,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W088,PCIE,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W089,PCIE,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W090,PCIE,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W091,PCIE,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W092,PCIE,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W093,PCIE,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W094,PCIE,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W095,PCIE,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W096,PCIE,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W097,PCIE,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W098,PCIE,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W099,PCIE,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W100,PCIE,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W101,PCIE,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W102,PCIE,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W103,PCIE,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W104,PCIE,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W105,PCIE,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W106,PCIE,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W107,PCIE,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W108,PCIE,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W109,STORAGE_DIAGNOSTICS,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W110,STORAGE_DIAGNOSTICS,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W111,STORAGE_DIAGNOSTICS,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W112,STORAGE_DIAGNOSTICS,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W113,STORAGE_DIAGNOSTICS,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W114,STORAGE_DIAGNOSTICS,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W115,STORAGE_DIAGNOSTICS,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W116,STORAGE_DIAGNOSTICS,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W117,STORAGE_DIAGNOSTICS,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W118,STORAGE_DIAGNOSTICS,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W119,STORAGE_DIAGNOSTICS,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W120,STORAGE_DIAGNOSTICS,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W121,STORAGE_DIAGNOSTICS,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W122,STORAGE_DIAGNOSTICS,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W123,STORAGE_DIAGNOSTICS,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W124,STORAGE_DIAGNOSTICS,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W125,STORAGE_DIAGNOSTICS,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W126,STORAGE_DIAGNOSTICS,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W127,SYSTEM_SAFETY,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W128,SYSTEM_SAFETY,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W129,SYSTEM_SAFETY,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W130,SYSTEM_SAFETY,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W131,SYSTEM_SAFETY,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W132,SYSTEM_SAFETY,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W133,SYSTEM_SAFETY,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W134,SYSTEM_SAFETY,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W135,SYSTEM_SAFETY,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W136,SYSTEM_SAFETY,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W137,SYSTEM_SAFETY,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W138,SYSTEM_SAFETY,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W139,SYSTEM_SAFETY,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W140,SYSTEM_SAFETY,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
```

### Annex D — All 42 proposed vias, uncertified
**Source:** `Data/L106_Vias_42_PLANNING.csv`

```csv
via_id,net_id,start_layer,end_layer,type,certification
V01,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE
V02,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE
V03,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE
V04,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE
V05,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE
V06,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE
V07,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE
V08,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE
V09,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE
V10,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE
V11,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE
V12,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE
V13,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE
V14,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE
V15,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE
V16,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE
V17,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE
V18,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE
V19,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE
V20,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE
V21,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE
V22,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE
V23,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE
V24,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE
V25,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE
V26,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE
V27,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE
V28,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE
V29,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE
V30,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE
V31,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE
V32,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE
V33,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE
V34,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE
V35,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE
V36,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE
V37,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE
V38,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE
V39,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE
V40,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE
V41,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE
V42,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE
```

### Annex E — Objective declarations
**Source:** `Data/L106_Objectives_6.csv`

```csv
objective,label,required_authority
1,Install 106 components,Authoritative project gameplay manager
2,Route 140 nets and 42 vias,Authoritative project gameplay manager
3,Commission CPU power and DDR4,Authoritative project gameplay manager
4,Complete POST and first boot,Authoritative project gameplay manager
5,Recover three boot failures,Authoritative project gameplay manager
6,Restore W141 and recertify system,Authoritative project gameplay manager
```

### Annex F — Repair fault definitions
**Source:** `Data/L106_RepairFaults_3.csv`

```csv
fault,symptom,required_action,repair_authority
F1_CPU_PGOOD_TIMING,CPU reset released before stabilized power,Replace faulted resistor; repeat startup timing,Solver-approved repair
F2_SPI_FLASH_CONTACT,Invalid SPI firmware reads,Remove solder bridge or restore pad; verify firmware,Solver-approved repair
F3_eMMC_CONTACT,Boot storage inaccessible,Repair storage connection; verify image and repeat boot,Solver-approved repair
```

### Annex G — W141 source safety contract
**Source:** `Data/L106_W141_Safety.json`

```json
{
  "jumper": "W141",
  "source": "L106_SystemBootMonitor_U14.TRUSTED_SYSTEM_READY_OUT",
  "destination": "L106_IndependentSafety_U15.TRUSTED_SYSTEM_READY_IN",
  "exact_pins": "UNASSIGNED",
  "required_steps": [
    "disconnect physical jumper",
    "independent safety inhibition",
    "reconnect jumper",
    "fresh POST and boot qualification",
    "explicit safety reset",
    "rerun final certification"
  ],
  "status": "SCHEMATIC_RECONCILIATION_REQUIRED"
}
```

### Annex H — Source art builder
**Source:** `Unity/Editor/SparkL106ArtBuilder.cs`

```csharp
#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.Level106.Editor
{
    public static class SparkL106ArtBuilder
    {
        [MenuItem("Tools/Project Spark/Levels/106/1. Build EMPTY SFX-14 Board")]
        public static void BuildEmptyBoard()
        {
            var guids = AssetDatabase.FindAssets("107_L106_SFX14_BarePCB t:Model");
            if (guids.Length == 0) { Debug.LogError("[L106] Bare PCB FBX not found; import Models_FBX first."); return; }
            var path = AssetDatabase.GUIDToAssetPath(guids[0]);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (!prefab) { Debug.LogError("[L106] PCB asset import failed: " + path); return; }
            var root = new GameObject("L106_SFX14_EMPTY_ART_ROOT");
            var board = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            board.name = "SFX14_Bare_PCB_ArtOnly"; board.transform.SetParent(root.transform, false);
            Selection.activeGameObject = root;
            Undo.RegisterCreatedObjectUndo(root, "Create Level106 Empty Board");
            Debug.Log("[L106] Art-only board built. No electrical components, vias, routes, or gameplay are certified.");
        }
        [MenuItem("Tools/Project Spark/Levels/106/2. Configure FBX Import (Meters)")]
        public static void ConfigureImport()
        {
            var guids = AssetDatabase.FindAssets("L106 t:Model"); int changed = 0;
            foreach (var g in guids) {
                var p = AssetDatabase.GUIDToAssetPath(g);
                if (!p.EndsWith(".fbx", StringComparison.OrdinalIgnoreCase)) continue;
                var importer = AssetImporter.GetAtPath(p) as ModelImporter;
                if (importer == null) continue;
                importer.globalScale = 1f; importer.importMaterials = true;
                importer.SaveAndReimport(); changed++;
            }
            Debug.Log("[L106] Import settings applied to " + changed + " FBXs. Verify meter dimensions and normals manually.");
        }
    }
}
#endif
```

### Annex I — Source starter-material utility
**Source:** `Unity/Editor/SparkL106StarterMaterials.cs`

```csharp
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;
using System;

namespace ProjectSpark.Level106.Editor
{
    public static class SparkL106StarterMaterials
    {
        [MenuItem("Tools/Project Spark/Levels/106/3. Create PBR Starter Materials")]
        public static void Create()
        {
            var ids = AssetDatabase.FindAssets("L106_PCB_BaseColor t:Texture2D");
            if (ids.Length == 0) { Debug.LogWarning("[L106] Import textures first."); return; }
            string texturePath = AssetDatabase.GUIDToAssetPath(ids[0]);
            string dir = Path.GetDirectoryName(texturePath).Replace('\\','/');
            string output = dir + "/../GeneratedMaterials";
            if (!AssetDatabase.IsValidFolder(output)) {
                var parent = Path.GetDirectoryName(output).Replace('\\','/');
                if (!AssetDatabase.IsValidFolder(parent)) parent = dir;
                AssetDatabase.CreateFolder(parent, "GeneratedMaterials");
                output = parent + "/GeneratedMaterials";
            }
            foreach (string family in new [] {"PCB","GOLD","COPPER","SOLDER","EPOXY","CERAMIC","BLACK","ALUMINUM","POLYMER","PLASTIC","RED","LED_GREEN","LED_BLUE"}) {
                var s = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
                if (s == null) continue;
                var m = new Material(s);
                string tex = dir + "/L106_" + family + "_BaseColor.png";
                var baseTex = AssetDatabase.LoadAssetAtPath<Texture2D>(tex);
                if (baseTex != null) {
                    if (m.HasProperty("_BaseMap")) m.SetTexture("_BaseMap", baseTex);
                    if (m.HasProperty("_MainTex")) m.SetTexture("_MainTex", baseTex);
                }
                AssetDatabase.CreateAsset(m, output + "/L106_" + family + ".mat");
            }
            AssetDatabase.SaveAssets();
            Debug.Log("[L106] Starter materials generated; assign FBX slots and configure metallic/smoothness for your render pipeline.");
        }
    }
}
#endif
```

### Annex J — Source README
**Source:** `Documentation/README_FIRST.md`

```markdown
# PROJECT SPARK — LEVEL 106: FIRST BOOT
## SFX-14 Board-Only Realistic 3D Source Kit

This kit contains 136 modular FBX meshes, 136 GLB counterparts, and one complete assembled visual reference in both formats.

## Contents
- `Models_FBX/` contains geometry for the bare board, 106 independently placeable components, layer, socket and repair details.
- `Models_GLB/` provides corresponding GLB previews and inspection geometry.
- `Textures/` contains PBR starter maps and a 2K silkscreen reference.
- `Data/` contains component placements, 140 *planned* routes, 42 *planned* vias, objectives, faults and W141 safety definition.
- `Unity/Editor/` has non-destructive editor helpers for importing the empty art board and starter materials.

## IMPORTANT: NOT A FULLY PLAYABLE LEVEL
This package provides original editable *procedural* geometry; it is not an artist-finished AAA asset set.
FBX files use a minimal binary 7400 writer; Unity/Blender import, material slot mapping and mesh quality must be verified in the target Unity version.
Component package dimensions and pads are artistic approximations, not manufacturer-certified footprints.

## Real size and coordinates
Units in FBX / GLB geometry are meters. Bare PCB: 0.285 m x 0.205 m x 0.0016 m.
Component model origins are local; `L106_Components_106.csv` contains placement coordinates in millimeters.
Positive Z is board top. Unity Y-up coordinate conversion may require rotating the imported root. Verify before placement.

## Gameplay restrictions
- ALL 140 route definitions and 42 vias are UNASSIGNED and NOT_CERTIFIABLE until reconciled with an authoritative pin-level netlist.
- W141 endpoints U14/U15 are functional planning labels; actual component pins and net IDs require specification.
- Visual geometry, factory-style decorative copper and completed showcase do NOT certify connections.
- The playable board starts with 106 parts uninstalled, 140 missing routes, 42 missing vias and W141 disconnected.
- Do not use the assembled reference to populate playable gameplay state.
- Reuse existing Project Spark virtual UI-based inspection and repair tools, rather than importing physical instruments.
- Preserve Levels 1–105 and use LevelGamePlayManager as the authoritative objective owner.

## Unity art import
1. Extract this folder anywhere under your project's `Assets/` folder.
2. In Unity, let FBX imports finish and inspect normals/materials/scale.
3. Select Tools > Project Spark > Levels > 106 > 1. Build EMPTY SFX-14 Board.
4. Optionally use the other menu options to apply FBX import settings and generate starter materials.
5. Integrate the gameplay solver only after a reviewed schematic and pin-level netlist exist.
```

### Annex K — Source material guide
**Source:** `Documentation/MATERIAL_GUIDE.md`

```markdown
Starter material maps are constant-color or simple PBR reference channels. Production work requires UV unwrapping, baked normal/roughness/occlusion, URP/HDRP setup, and precise FBX material-slot remapping.
```

## Final execution instruction
Read, audit and reconcile before coding. Build art/import first, then electrical authority, then world-space interaction, then power/CPU/DDR4/firmware/PCIe/storage, then three faults/W141, then objective and acceptance QA. Where source is incomplete, generate a concrete missing-data report and keep certification blocked. Preserve Levels 1–105 and generate a working, visually polished Level 106 only on approved real project and reviewed electrical data.

**PROJECT SPARK — LEVEL 106: FIRST BOOT — ASSEMBLE. POWER. INITIALIZE. BOOT.**

