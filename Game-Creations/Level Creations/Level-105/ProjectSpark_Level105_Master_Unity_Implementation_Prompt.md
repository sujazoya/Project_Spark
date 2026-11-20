# PROJECT SPARK — LEVEL 105: EXPANSION NEXUS
## MASTER UNITY IMPLEMENTATION PROMPT — SOURCE-ALIGNED

**Board:** ENX-13 • **Endpoint card:** ECX-4 • **Category:** Computer Motherboard Mastery • **Output:** Fully playable, evidence-certified Unity level • **Design targets:** 100 components, 132 routes, 38 vias, six objectives, three recoverable faults and removable W133.

> **Instruction to the implementing agent:** Act as a senior Unity gameplay, embedded-electronics simulation and technical-art engineer. Implement a production-oriented, integrated Level 105 using the existing Project Spark codebase and the actual supplied Level 105 art/data package. **Inspect the real project APIs before writing integrations**; do not invent methods on `LevelGamePlayManager`, `SparkTerminal`, solver components or the repair system. Preserve Levels 1–104 and all existing shop, shelf, throw, circuit and UI behavior. Report precisely which parts compile and pass Play Mode, and which remain blocked by source-design issues.

**Important distinction:** This is an implementation specification, **not a claim that the included art kit already contains playable circuits**. The kit has actual 3D models and planning data, but no verified pin-level ENX-13/ECX-4 schematic. Fail closed on missing authority or missing connectivity, never auto-certify design-stage traces.

## 01 — Authoritative source inventory and limitations

Use `ProjectSpark_Level105_ExpansionNexus_BoardOnly_Realistic_Complete.zip` as the art/data source. It contains `Level105_ENX13/Models_FBX`, `Models_GLB`, `Textures`, `Data`, `Unity`, and `Documentation` (an extracted equivalent folder may be used).

- **128 modular FBXs and 128 matching GLBs**, plus `129_L105_AssembledMainboardReference.fbx` and a matching assembled reference GLB.
- **100 installable component models**, 94 on ENX-13 and six on ECX-4 per `ComponentBOM.csv`; **28 board/structural/repair models**.
- Main motherboard 270 × 190 mm; expansion card 80 × 35 mm; model vertices in **meters, Unity Y-up** per README.
- **16 PBR starter material families**, **49 texture maps** including 2K silkscreen reference.
- `CopperRoutes_132_REQUIRES_SCHEMATIC.csv`: all source/destination pads and net IDs are `UNASSIGNED`; `certifiable=FALSE`.
- `ViaTasks_38_REQUIRES_SCHEMATIC.csv`: layer start/end `TBD`, net ID `UNASSIGNED`, `verified=FALSE`.
- `ComponentPlacements.csv`: `installed_at_start=FALSE`, `footprint_verified=FALSE` for all entries. These coordinates are art-placement hints, **not manufacturer-certified footprints**.
- W133 source is `L105_LinkTrainingMonitor_U12.TRUSTED_EXPANSION_READY_OUT`; destination is `L105_ExpansionSafetySupervisor_U13.TRUSTED_EXPANSION_READY_IN`. These are **logical labels**, not approved chip pins.
- Two existing Editor scripts build empty art / reference and create starter materials. They do not implement gameplay, routing, link training or objective completion.

**Detected blocker in shipped script:** `SparkL105MaterialBuilder.cs` contains `Replace('\','/')` incorrectly escaped in its actual file, written as `Replace('','/')` with a single backslash character in the C# source. Correct all occurrences to `Replace('\', '/')` (the C# code must contain two backslashes inside the character literal) before compilation. Verify with Unity Compiler, not string matching alone. Prefer `Path` normalization or `.Replace('\', '/')` with correct source escaping and robust asset discovery, avoiding hardcoded install paths.

The BOM contains `U12` and `U13` as **generic IC descriptions**, not verified monitor/supervisor part numbers. Resolve their exact functional identities and physical pins during schematic approval, or leave W133 *Not Certifiable*.

## 02 — Mandatory preflight and project-preservation rules

1. Back up the Unity project or use a clean version-control branch; preserve scenes, prefabs, GUIDs, gameplay managers, and all Levels 1–104.
2. Enumerate existing assembly/routing/simulation classes and actual method signatures. Search for `LevelGamePlayManager`, `SparkTerminal`, relevant electric-network APIs, component/PCB placement systems and the existing AAA world-space virtual repair UI. Document which services exist.
3. Import the exact Level 105 FBXs; validate mesh hierarchies, units, pivots, material indices, normals, UVs, render-pipeline compatibility and collider targets. Do not assume custom-exported FBXs are Unity-tested.
4. Validate `ComponentBOM`, `ComponentPlacements` and `ModelManifest` one-to-one; use ref IDs as stable keys, with board identity (`ENX-13` / `ECX-4`) as part of lookup.
5. Repair the supplied material-builder syntax issue and run Unity compilation before any new runtime scripts.
6. Create an isolated Level 105 scene and prefabs under the project's established level conventions; do not alter global prefabs except by additive, backwards-compatible adapters.
7. If authoritative schematic/footprint data is absent, create clearly tagged **design-preview mode** for geometry and unscored interaction, while retaining **Certification Disabled** for all electrically dependent objectives. No fabricated netlists/pins.

**Deliverable at this gate:** an import/preflight report with exact project paths, FBX load and material status, compile status and remaining design data blockers.

## 03 — Scene assembly and hierarchy

Build two independent physical art roots, for ENX-13 and ECX-4, plus a separate world-space virtual instrumentation canvas. Recommended hierarchy (adapt names to existing project):

```text
Level105_ENX13
  MissionCoordinator
  AuthorityAdapters
  ElectricalTopology / PowerClockReset / PCIeSimulation / Faults / Objectives
  Board_Main_ENX13
    BareBoardVisual
    ComponentFootprintAnchors
    CopperLayerView_L1_to_L8
    RouteInteractionTargets
    ViaInteractionTargets
    PCIeSlot_ContactBank
    W133_PhysicalJumperSocket
  Expansion_ECX4
    BareEndpointVisual
    ComponentFootprintAnchors
    EdgeContactBank
    CardInsertionPivot
  VirtualRepairCanvas_WorldSpace
    ToolDeck / LogicAnalyzer / LinkTrainer / LaneMonitor / EyeAnalyzer
    CardInsertionGuide / SafetyOverlay / Telemetry
  DiagnosticsAndLogs
  ART_REFERENCE_NOT_GAMEPLAY (disabled, editor-only)
```

The card is a child of the card-interaction root, *not* permanently fused into the motherboard. Its insertion pivot follows the connector's mechanical axis and only becomes electrically joined when engagement, power contact, ground contact, presence and required signal contacts pass the modeled criteria. The slot and card contacts must be real named interaction targets derived from approved contact IDs, not just a single boolean collision.

## 04 — Source-unit, coordinates, and component handling

- Main board physical size: `0.270 × 0.190 m`; ECX-4 card: `0.080 × 0.035 m`. Scale uniformly for game visibility via an isolated art-scale parent, while preserving solver physical units and relative geometry in a separate coordinate basis.
- Respect meter-based FBX vertices; avoid repeated meter-to-millimeter conversion. Require an automatic dimension assertion on import.
- Placement coordinates in `ComponentPlacements.csv` are initial art hints. Final footprints require pitch/orientation/pin-one verification before snap placement or solder certification.
- Every installable component must have a unique `ref`, owner board, installed state, oriented footprint, accessible target collider (including contact-related subtargets), component health and immutable source manifest lookup.
- No component can be installed merely by importing a completed-reference prefab. Material remapping and mesh instantiation are visual operations; they must not set gameplay flags.
- Track 100 installable components across the main board and endpoint; a separate ECX-4 board/socket structure is not implicitly a 101st installable BOM item.
- W133 is one of the 100 BOM components and is a physically detachable jumper, not an additional count added to 100.

## 05 — Eight-layer PCB and electrical graph authority

Before route completion is enabled, obtain an approved, explicit netlist schema comprising `component_ref`, `pad_id`, `package_pin`, `net_id`, `layer_id`, `via_span`, physical contact identity and allowed electrical equivalences. Require board-level distinction for the slot and endpoint card and a cross-board contact bridge.

For each route W001–W132:

- Bind **both exact terminal IDs**, electrical net, copper layer, segment geometry, net-class constraints and design-rule requirements.
- Only accept user input if the route is among valid connections and the solver confirms conductive graph changes. A visual colored path cannot mark a route completed.
- Enforce short-circuit checks, open pads, pin direction, power-domain isolation, trace reachability and layer legality.
- Differentiate factory-fixed traces (always present in design), player-built traces (initially missing) and art-only copper (no electrical authority).
- A player's 132 route actions are selected gameplay tasks, **not** the entire physical conductor count of an eight-layer PCIe system.

For vias V01–V38:

- Require explicit `net_id`, `start_layer`, `end_layer`, type (through/blind/buried if supported), allowed span and drilled/interconnect validation.
- Never let a via connect all eight layers automatically. Require continuity through the correct plated geometry and check internal-plane clearance.
- Update UI 0/38 only for genuinely verified structures.

**Source gate:** All 132 supplied routes and 38 supplied vias are unassigned. Until amended by an approved schematic, their certifiable state must remain `FALSE`, and Objective 2 remains blocked.

## 06 — Power, connector, reset and clock commissioning

- Model protected 12 V input, auxiliary 3.3 V supply, ECX-4 endpoint power availability, shared reference and return continuity. Pin-count and slot power capability are left for schematic approval; do not infer that a visual pad corresponds to a real PCIe pin.
- Implement independent card-present detection, ground-first/contact-engagement sequence, endpoint supply qualification, 100 MHz differential reference clock and reset state.
- Simulate controlled power-up and valid reset release only after power/clock conditions qualify. Expose these distinct states to virtual multimeter and oscilloscope readouts.
- A partially seated card may report card presence or power while high-speed electrical connectivity remains incomplete. A simulated bad latch/contact must alter the contact graph, not just the UI text.
- Reject training when endpoint power, reference clock, reset prerequisites or mandatory lane/sideband contacts fail.
- Preserve real source/target timing semantics: the clock must be modeled as a differential waveform or qualified timing signal with jitter, not just a constant “100 MHz” label.

## 07 — PCIe Gen2-style bounded educational protocol

Implement a deterministic, inspectable link-training model inspired by PCIe Gen2, not a complete or standards-certified PCIe protocol stack. It should include well-defined states:

`PoweredOff → Detect → Polling → Configuration → L0 → Recovery` with `Disabled / Fault / Reset` paths.

- `Detect`: qualified endpoint electrical presence / receiver detect.
- `Polling`: clock/reset requirements and modeled training-pattern exchange.
- `Configuration`: lane alignment, negotiation of width and speed subject to device/board capability and active lane mapping.
- `L0`: usable data link, preserving actual negotiated speed, lane mapping, signal margin and error counts.
- `Recovery`: retrain on degraded margins, speed change or link errors; include budgeted retries, event reason and stop conditions.
- Initial qualification may run at a lower supported mode before 5.0 GT/s target. Both root complex and ECX-4 must support requested mode in the approved model.
- Full Objective 3 requires **5.0 GT/s per lane and valid x4 link in L0**; a fallback x1 or x2 link can be a diagnostic outcome but **must not pass**.
- Show live per-lane TX/RX health, AC coupling condition, receiver detection, polarity, loss, skew, jitter and lane-specific training evidence. All results derive from simulated state and actual topology.
- PCIe terminology must be labeled “Gen2-style educational simulation,” never manufacturer/steward certification.

## 08 — Four differential lanes, AC coupling and signal quality

The PCIe-style x4 link has four bidirectional lanes, each containing a TX and an RX differential pair. Its detailed net definitions require approved pins, connector IDs, AC-coupling device location and return paths.

Simulation inputs per lane: drive waveform, receiver threshold, differential impedance, segment length, insertion loss, pair skew, reference plane continuity, via discontinuity, AC-coupling presence/value, electrical contact quality and common clock quality. Provide a scalable fast model for gameplay rather than a heavyweight full SPICE engine.

Output: eye opening, timing margin, signal margin, lane availability, recoveries, symbol/packet integrity and diagnostic reason codes. Qualification needs **fresh measured evidence** after any repair; cached healthy samples must not be reused.

- Lane 2 series AC-coupling failure opens or degrades the affected TX path, as defined by the approved schematic. Its repair changes actual component/contact state and link-training outcome.
- Clock-integrity fault changes differential reference-clock behavior or distribution, propagating to lane timing and training.
- Lane 3 contact failure changes slot-to-card electrical continuity (possibly intermittently with a deterministic disturbance schedule), influencing negotiated width or link stability.

Do not force a particular reduced width for a given failed lane without an explicit modeled lane mapping and valid negotiation rule.

## 09 — Endpoint discovery, enumeration and traffic testing

After successful `L0`, implement a bounded enumeration path with endpoint identity, configuration header, resource requests, virtual address assignment and readiness. Use an accurate enough state model for a player to learn: **presence ≠ link trained ≠ device enumerated ≠ successful data transfer**.

- Model a real ECX-4 endpoint object and a separate root complex.
- Record enumeration events, device ID supplied by design data, BAR-like allocations (fictional/educational), errors and timestamps.
- Transfer test: bi-directional known payloads, sequence numbers, hashes/checksums, receive confirmation, retry/error counters, and deterministic traffic load.
- Observe link-down/recovery mid-transfer: halt or retry according to the bounded protocol and do not award stale complete results.
- Objective 4 requires a fresh valid endpoint enumeration, current x4 L0 state and test evidence with required transfer integrity; a card UI icon appearing is insufficient.

## 10 — W133 safety feedback: enforce true disconnection and recovery

Authoritative source (`W133_Recovery.json`):

- Source logical endpoint: `L105_LinkTrainingMonitor_U12.TRUSTED_EXPANSION_READY_OUT`.
- Destination logical endpoint: `L105_ExpansionSafetySupervisor_U13.TRUSTED_EXPANSION_READY_IN`.
- Initial state `OPEN`; physical pins require schematic reconciliation; original source sets `certifiable=false`.
- Recovery sequence: reconnect jumper → retrain x4 to L0 → new enumeration → fresh transfer tests → explicit reset.

**Safety design:** W133 is a trusted link-ready digital feedback, **not** the data lanes or main power rail. Its removal must open the actual logical-to-physical feedback path, invalidate health authority, latch the fault and inhibit protected operations. It must not fictitiously zero power or erase unrelated diagnostic evidence. Validate freshness/transition protocol and missing/stuck signal detection, not simply pin HIGH.

Stage Objective 6 only after the player has completed the earlier tasks and a controlled test arms a valid prior trusted status. Require recorded disconnection, safety inhibition, reconnection, new x4 L0 training, new endpoint enumeration, successful post-repair traffic, explicit reset, and renewed independently verified authorization. If U12/U13 remain generic BOM ICs with unresolved physical pins, keep this objective blocked and disclose the blocker in the build report.

## 11 — Three authoritative recoverable faults

| Fault | Underlying failure | Expected solver/protocol consequence | Player tools and proof |
|---|---|---|---|
| F1 — Lane 2 AC coupling capacitor open | Specific capacitor contact or internal capacitor opens | Lane TX path invalid; lane training/degradation follows modeled mapping | Multimeter/oscilloscope/net tracer; remove/replace or approved solder repair; repeat lane and link tests |
| F2 — 100 MHz reference-clock integrity | Clock distribution component, contact or waveform quality degrades | Clock not qualified or jitter/timing failures; Polling/Recovery can result | Differential virtual oscilloscope, clock analysis, permitted component/connection repair; prove fresh clean reference |
| F3 — Lane 3 intermittent slot contact | Explicit card-edge or socket contact intermittently opens under deterministic disturbance | Lane integrity degraded, link fallback/Recovery/drop depending model | Connector inspection and contact measurement, reseat/clean/replace; repeated mechanical and data qualification |

Faults are staged after relevant base certification. All fault injection, diagnosis, repair and re-qualification must be serializable, repeatable and attributable to distinct fault IDs. Never award F1/F2/F3 because of an animation or generic “repair complete” click. Repair is authorized only for a valid target, tool type, electrical precondition and actual state mutation. Show errors for incompatible operations.

## 12 — AAA virtual inspection & click/touch repair integration

Use the already existing **World-Space Virtual PCB Repair UI** rather than importing physical multimeter, oscilloscope, soldering iron, analyzer or additional room models. Add/extend virtual panels for a PCIe-style link analyzer, lane-status view, virtual oscilloscope, eye-diagram display, net tracer, layer viewer, expansion-card contact inspector, telemetry and safety logs.

Interaction contract:

1. User selects an instrument or repair mode on world-space Canvas.
2. User clicks/touches a valid collider on the motherboard, card, pin/pad, capacitor, W133 jumper or slot contact.
3. Resolve an unambiguous component/pad/contact target ID and validate the tool and authority.
4. Acquire solver measurement or perform an authorized repair transaction; never update objective states directly from UI.
5. Spawn restrained contact-localized VFX (micro-sparks, solder glow, contact-clean pulse, highlighting) at the actual hit location; separately report operation status.
6. Re-evaluate topology, clock, LTSSM, negotiated link and fresh qualification evidence before objective state can change.

Support mouse/touch, reasonable UI hit areas, correct physics raycast layers and camera/Canvas sorting order. Decorative widgets must never block all 3D target interaction. Demo-generated waveforms must be marked illustrative until bound to authentic simulation data. Reuse existing adapter APIs if present, not a parallel incompatible inventory of virtual tools.

## 13 — Six authoritative objective contracts

| Objective | Required fresh source authority | Must not pass if |
|---|---|---|
| O1 — Assemble 100 | Per-BOM placement, alignment, orientation, solder state and verified footprints across ENX-13/ECX-4 | Missing component, wrong part or placeholder footprint |
| O2 — 132 routes + 38 vias | Approved pin-level nets, solver graph, PCB design rules, valid layers/contact graph and W133 installed for commissioning | Route/via remains `UNASSIGNED`, short/open, invalid via span or decorative copper |
| O3 — Commission x4 link | Independent power/clock/reset readiness and LTSSM training with 5.0 GT/s ×4 at L0 | Only presence, x1/x2 fallback, failed clock, failed contact or cached state |
| O4 — Enumerate and transfer | New endpoint enumeration, valid resource setup and bidirectional finite-pattern data results | No L0, stale results, CRC/payload failure, inconsistent endpoint |
| O5 — Repair three faults | F1/F2/F3 diagnosis, authoritative repairs, fresh clock/link/data re-qualification | VFX-only repair, any active fault, earlier cached tests |
| O6 — W133 recovery | Actual open event and inhibition; physical repair; fresh x4 L0, enumeration, transfers and explicit reset | W133 still open, feedback stuck, missing pins or no fresh evidence |

The game starts 0/6 and each target transitions only after the authoritative subsystem reports certified evidence to the **existing `LevelGamePlayManager`**. Inspect the manager's real API rather than inventing an event or mutating displayed 0/6 counters. Historical objective progress may remain visible after later fault injection, while current certification health and requalification status must be independently reflected.

## 14 — Mission phases and player-facing experience

**Phase A: Inspect and build.** Show ENX-13 and unseated ECX-4 board with footprints, transparent layer viewer and BOM tray. Highlight selected pads, allow deliberate placement and solder validation. No auto-installs from model import.

**Phase B: Route and insert.** Player completes the selected 132 top/internal/bottom route tasks, 38 vias, W133 and appropriate card seating. Provide progress counts separate from certified continuity and visual trace counts.

**Phase C: Commission.** Turn on protected rails, inspect 100 MHz ref clock, endpoint presence, PERST-like reset, receiver detect and LTSSM. Lane status explains failure reason and current width.

**Phase D: Transfer and enumerate.** Read endpoint configuration, assign resources, perform diagnostic payload transfers and expose signal-quality overlays. Do not declare success from a placeholder “Card Detected” status alone.

**Phase E: Diagnose and recover.** Inject staged faults one by one. Use appropriate virtual inspections and actual repairs; rerun full acceptance after all repairs.

**Phase F: W133 critical test.** Supervised removal, latch/authorization check, repair, retrain, enumerate, transfer and explicit reset. Deliver final `6/6` only from gameplay manager certification.

Provide accessibility-appropriate contrast, readable tooltips, screen-space-safe resolution, restrained sci-fi glows and user-friendly highlights. All UI display should update from real game state where available.

## 15 — Performance and rendering goals

- Board-only meshes; no new scene environment or standalone instrument meshes.
- Use URP if current project uses it, otherwise adapt without imposing pipeline migration. Respect metallic/roughness/smoothness packing differences, normal maps and sRGB/linear import settings.
- Use batching, instancing and LOD for repeated ceramic passives and dense contacts, but keep independent colliders/semantic IDs for installable gameplay items.
- Prefer local/world-space Canvas UI with pooled effects. VFX should be localized and budgeted so touch interactions remain responsive.
- Simulate high-speed protocol/data at a bounded deterministic level, not by physically stepping every one of 5 billion symbols each second. Maintain dimensional correctness in presented delays, timing and transfer metrics.
- Architect fast update frequencies separately for UI, solver snapshots, electrical events and art. Avoid `FindObjects...` per frame.
- Allow QA injection of deterministic fault seeds and replayable action logs.

## 16 — Proposed C# service boundaries (not existing API claims)

| Responsibility | Proposed class |
|---|---|
| Scenario coordination | `SparkLevel105Coordinator` |
| Source manifest import | `SparkLevel105ManifestImporter` |
| Route/via acceptance | `SparkLevel105BoardTopologyAdapter` |
| Card insertion & contacts | `SparkPCBExpansionCardInteractor` |
| 100 MHz reference clock | `SparkPCBPCIeReferenceClock` |
| Per-lane quality | `SparkPCBPCIeLaneModel` |
| LTSSM | `SparkPCBLinkTrainingStateMachine` |
| Negotiation | `SparkPCBLinkNegotiator` |
| Endpoint enumeration | `SparkPCBEndpointEnumerator` |
| Transfer tests | `SparkPCBExpansionDataTester` |
| Fault management | `SparkLevel105FaultController` |
| W133 recovery | `SparkLevel105W133Recovery` |
| Six objectives | `SparkLevel105ObjectiveTracker` |
| World-space UI adaptation | `SparkLevel105VirtualRepairAdapter` |

Keep these under the existing Project Spark namespace conventions after inspecting actual code. Use interfaces or adapters for the existing electrical solver and manager. Avoid creating a second, hidden electrical truth in the Level 105 controller.

## 17 — Unity Editor automation

**Source-existing menus** (do not claim these create complete gameplay):

- `Tools/Project Spark/Levels/105/1. Build EMPTY ENX-13 Board`
- `Tools/Project Spark/Levels/105/2. Open Assembled ART Reference`
- `Tools/Project Spark/Levels/105/3. Generate Starter PBR Materials`

**Required new one-click playable builder target:** `Tools/Project Spark/Levels/Create Level 105`.

Builder responsibilities: validate imports/source manifests; initialize scene and art roots; assemble component footpoint anchors; register correct board ownership; create empty route/via action targets from **verified** netlist only; create ECX-4 insertion and connector contacts; attach simulation/mission adapters; create virtual tool deck; configure safety and faults; create Level 105 scene/prefab assets using existing conventions; report unresolved gate failures. Should be idempotent and avoid duplicate roots and material assets on repeated execution.

Do not let material generation fail just because the user extracted the ZIP under `Assets/My_Assets/...` rather than the assumed root. Use `AssetDatabase.FindAssets` with reliable exact-path resolution and normalized directory paths. Fix the provided invalid backslash literal first.

## 18 — State, saves, and anti-bypass validation

Persist per-board installed component state, approved copper route geometries, via net/span assignments, endpoint insertion/contact states, W133 jumper status, fault injection and repair history, training state and evidence-generation IDs. Save file format must include a version and manifest hash.

Prevent: assembled-reference auto-completion, UI count spoofing, visually covering an unconnected pad, installing the wrong component value, bypassing missing lanes with factory art, routing the wrong net, reusing old healthy measurements, falsely accepting a W133 reconnect without a new data test, and accidentally earning six objectives through inspector defaults or Unity Editor preview.

When the active graph or component health changes, invalidate only dependent fresh evidence, not user history. Report the difference between mission milestones already earned and current qualified operational status.

## 19 — Electrical schematic and component-reconciliation deliverables

Create (or request from the owner) an approved source-of-truth set before turning on authoritative certification:

1. Main-board/endpoint symbol library and pin-mapped BOM, including verified physical identities for U12/U13.
2. ENX-13↔J1↔ECX-4 electrical connector table with each contact ID and pin map.
3. Full power, refclock, reset, presence, differential TX/RX, sideband and W133 netlist.
4. Schematic-level AC-coupling capacitor refs and values; exact F1 affected lane 2 TX/RX path.
5. Reference clock source/distribution pins and defined F2 fault location.
6. Lane 3 connector-contact IDs, engagement behavior and F3 failure mapping.
7. Player-route W001–W132 exact source/destination pads and net IDs; factory copper flags; clearances, copper widths and signal layer references.
8. Via V01–V38 exact net and layer-span assignments, plus geometry and type.
9. Real component footprint geometry, orientation and solderable pad maps; U12/U13 identities.
10. Configurable performance acceptance rules for Gen2-style lane margins, data-test duration, x4 width, error rates, reference clock conditions and safety response.

When these are missing, implement the UI/art shell and declare **BLOCKED: UNSIGNED ELECTRICAL SCHEMATIC**. Do not generate arbitrary connections, mark placeholder routes as certifiable or invent package pins in code.

## 20 — Test plan and result format

Implement automated EditMode and PlayMode acceptance checks. Define each test in an automated framework (NUnit or existing suite), with expected state, steps, evidence and pass/fail. The list below is a proposal; adapt to real APIs and approved electrical source files. Any test that depends on absent schematic data should be explicitly `BLOCKED`, not silently marked PASS.

### Import and art — 10 checks

- [ ] **T001** — FBX 128 models load with no missing paths.
- [ ] **T002** — GLB count matches 128 modular models.
- [ ] **T003** — reference FBX remains disabled in gameplay.
- [ ] **T004** — board physical size 270 x 190 mm.
- [ ] **T005** — ECX-4 size 80 x 35 mm.
- [ ] **T006** — component manifest count 100 exact.
- [ ] **T007** — main-board ownership count 94.
- [ ] **T008** — endpoint ownership count six.
- [ ] **T009** — import pivots/normals/UVs valid.
- [ ] **T010** — material generator compiles with corrected path code.

### Assembly and seating — 10 checks

- [ ] **T011** — start with 0 installed components.
- [ ] **T012** — install correct main-board part.
- [ ] **T013** — install ECX-4 part at proper anchor.
- [ ] **T014** — reject wrong footprint.
- [ ] **T015** — reject wrong orientation/pin-one.
- [ ] **T016** — reject wrong value/package.
- [ ] **T017** — reject placement onto other board.
- [ ] **T018** — contact solder repair requires authority.
- [ ] **T019** — partial card insertion is not seated.
- [ ] **T020** — fully seated card registers legitimate contacts.

### Electrical topology — 10 checks

- [ ] **T021** — all 132 routes have approved endpoints before certification.
- [ ] **T022** — all routes start incomplete.
- [ ] **T023** — unassigned route never certifies.
- [ ] **T024** — wrong net route rejected.
- [ ] **T025** — correct net route changes solver connectivity.
- [ ] **T026** — factory copper never bypasses user task.
- [ ] **T027** — short circuit detected and rejected.
- [ ] **T028** — all 38 via tasks have spans.
- [ ] **T029** — invalid via span is rejected.
- [ ] **T030** — all required valid nets pass graph verification.

### Power and clock — 10 checks

- [ ] **T031** — 12 V supply qualifies independently.
- [ ] **T032** — 3.3 V auxiliary qualifies independently.
- [ ] **T033** — missing return causes power fault.
- [ ] **T034** — endpoint unpowered blocks training.
- [ ] **T035** — 100 MHz reference qualifies at approved limits.
- [ ] **T036** — missing reference blocks training.
- [ ] **T037** — clock jitter fault changes sampling/training.
- [ ] **T038** — reset held prevents training.
- [ ] **T039** — reset release follows power/clock policy.
- [ ] **T040** — card present does not imply trained.

### Lane model and LTSSM — 10 checks

- [ ] **T041** — start in Detect with no endpoint.
- [ ] **T042** — Polling requires receiver detect.
- [ ] **T043** — Configuration requires training data.
- [ ] **T044** — L0 reached under healthy approved link.
- [ ] **T045** — x4 speed 5 GT/s certifies objective 3.
- [ ] **T046** — x1 fallback cannot certify x4.
- [ ] **T047** — x2 fallback cannot certify x4.
- [ ] **T048** — Lane 2 open changes link availability.
- [ ] **T049** — Lane 3 contact intermittency provokes retraining.
- [ ] **T050** — Recovery budget and retry reasons recorded.

### Enumeration and traffic — 10 checks

- [ ] **T051** — no enumeration before L0.
- [ ] **T052** — endpoint configuration readable in L0.
- [ ] **T053** — resource assignment matches declared endpoint.
- [ ] **T054** — bidirectional traffic validates payload integrity.
- [ ] **T055** — sequence error increments diagnostic counter.
- [ ] **T056** — transfer error blocks acceptance.
- [ ] **T057** — link drop invalidates in-flight transfer.
- [ ] **T058** — recovery permits fresh enumeration.
- [ ] **T059** — stale enumeration does not certify.
- [ ] **T060** — fresh sustained x4 transfer qualifies objective 4.

### Fault F1 — 10 checks

- [ ] **T061** — F1 injects open in specified lane 2 AC cap.
- [ ] **T062** — F1 alters conductive graph.
- [ ] **T063** — scope identifies affected lane.
- [ ] **T064** — wrong tool target cannot repair F1.
- [ ] **T065** — authorized component replacement clears F1.
- [ ] **T066** — repair VFX alone does not clear F1.
- [ ] **T067** — training before F1 repair remains non-qualified.
- [ ] **T068** — fresh training after F1 repair required.
- [ ] **T069** — post-repair lane quality recalculated.
- [ ] **T070** — F1 recorded separately from other faults.

### Fault F2 and F3 — 10 checks

- [ ] **T071** — F2 changes clock distribution quality.
- [ ] **T072** — F2 reports clock fault origin.
- [ ] **T073** — F2 repairs require real component state.
- [ ] **T074** — post-F2 clock measurement fresh.
- [ ] **T075** — F3 changes specified lane 3 contact.
- [ ] **T076** — F3 mechanical disturbance is deterministic.
- [ ] **T077** — F3 card reseat alone if inadequate does not falsely fix.
- [ ] **T078** — authorized connector repair restores continuity.
- [ ] **T079** — F2/F3 require fresh x4 training.
- [ ] **T080** — three-fault objective requires all faults cleared and requalified.

### W133 safety — 10 checks

- [ ] **T081** — W133 initially OPEN.
- [ ] **T082** — generic U12/U13 identities cannot certify pins.
- [ ] **T083** — W133 normal closed can carry validated feedback.
- [ ] **T084** — W133 removal opens authority path.
- [ ] **T085** — W133 fault inhibits protected operations.
- [ ] **T086** — W133 fault does not zero unrelated rails.
- [ ] **T087** — W133 stuck-high fails freshness validation.
- [ ] **T088** — W133 reconnect alone does not clear latch.
- [ ] **T089** — new L0/enumeration/transfers plus reset required.
- [ ] **T090** — objective 6 only after actual full recovery.

### Objectives, UI and regression — 10 checks

- [ ] **T091** — initial objective counter 0/6.
- [ ] **T092** — six objectives map to authoritative manager.
- [ ] **T093** — UI indicators cannot write objective authority.
- [ ] **T094** — demo waveform clearly marked illustrative.
- [ ] **T095** — click and touch resolve correct contact target.
- [ ] **T096** — VFX emitted only at accepted target.
- [ ] **T097** — save/load preserves state and evidence generations.
- [ ] **T098** — repeat builder is idempotent.
- [ ] **T099** — previous Levels 1-104 regression checks pass.
- [ ] **T100** — completed objective 6 and 6/6 requires fresh complete evidence.

## 21 — Recommended implementation order and delivery checklist

**Milestone 1 — Art import:** correct editor script compilation; verify 128 models, ECX-4 independent card, realistic scale and material slots. Deliver import report and scene containing empty art.

**Milestone 2 — Schema reconciliation:** obtain verified electrical source package, resolve U12/U13 and W133 physical pads, write route/via import validator and tests. Until this phase passes, keep certification disabled.

**Milestone 3 — Board and connector gameplay:** integrate 100 individual components, real route/via interactions, card seating/contact graph and shop/shelf flows without breaking previous systems.

**Milestone 4 — Simulator:** implement realistic educational power/clock/reset requirements, four-lane channel characteristics, LTSSM, speed/width negotiation, enumeration and transfer tests.

**Milestone 5 — Faults and safety:** implement F1/F2/F3, W133 authority path, click/touch world-space repair UI and VFX.

**Milestone 6 — QA:** all six objectives and 100 acceptance tests, build report with proven status, Unity console clear, no warnings hidden, previous-level smoke tests.

**Final handoff report must include:** exact scene/prefab paths, new/changed scripts, compilation results, Play Mode proof, verified schematic revision/hash, model/material QA, each objective's source authority, results of the 100 tests (PASS/FAIL/BLOCKED), and explicitly listed unfinished items. Never claim a playable, electrically certified Level 105 solely on the basis of an art-builder output.

---
# SOURCE ANNEXES — VERBATIM MANIFEST CONTENT (FOR RECONCILIATION)

The tables below are transcribed from the supplied Level 105 package, not generated electrical pin assignments. Keep `UNASSIGNED`, `TBD`, and false verification flags intact until source owners approve a verified schematic.

## ANNEX A — Full model manifest (128 modular assets)

**Source:** `Data/ModelManifest.csv`

| index | asset_id | type | board | fbx | glb | x_m | y_m | z_m | installable |
|---|---|---|---|---|---|---|---|---|---|
| 1 | L105_U1 | root | ENX-13 | 001_L105_U1.fbx | 001_L105_U1.glb | -0.046 | 0.00087 | 0.018 | True |
| 2 | L105_U2 | endpoint | ECX-4 | 002_L105_U2.fbx | 002_L105_U2.glb | -0.011 | 0.00087 | 0.006 | True |
| 3 | L105_U3 | ic | ENX-13 | 003_L105_U3.fbx | 003_L105_U3.glb | -0.12 | 0.00087 | 0.075 | True |
| 4 | L105_U4 | ic | ENX-13 | 004_L105_U4.fbx | 004_L105_U4.glb | -0.104 | 0.00087 | 0.075 | True |
| 5 | L105_U5 | ic | ENX-13 | 005_L105_U5.fbx | 005_L105_U5.glb | -0.087 | 0.00087 | 0.075 | True |
| 6 | L105_U6 | ic | ENX-13 | 006_L105_U6.fbx | 006_L105_U6.glb | -0.069 | 0.00087 | 0.075 | True |
| 7 | L105_U7 | ic | ENX-13 | 007_L105_U7.fbx | 007_L105_U7.glb | -0.051 | 0.00087 | 0.075 | True |
| 8 | L105_U8 | ic | ENX-13 | 008_L105_U8.fbx | 008_L105_U8.glb | -0.033 | 0.00087 | 0.075 | True |
| 9 | L105_U9 | ic | ENX-13 | 009_L105_U9.fbx | 009_L105_U9.glb | -0.014 | 0.00087 | 0.075 | True |
| 10 | L105_U10 | ic | ECX-4 | 010_L105_U10.fbx | 010_L105_U10.glb | -0.03 | 0.00087 | 0.012 | True |
| 11 | L105_U11 | ic | ECX-4 | 011_L105_U11.fbx | 011_L105_U11.glb | -0.019 | 0.00087 | 0.012 | True |
| 12 | L105_U12 | ic | ENX-13 | 012_L105_U12.fbx | 012_L105_U12.glb | 0.005 | 0.00087 | 0.075 | True |
| 13 | L105_U13 | ic | ENX-13 | 013_L105_U13.fbx | 013_L105_U13.glb | 0.023 | 0.00087 | 0.075 | True |
| 14 | L105_U14 | ic | ENX-13 | 014_L105_U14.fbx | 014_L105_U14.glb | 0.041 | 0.00087 | 0.075 | True |
| 15 | L105_U15 | ic | ENX-13 | 015_L105_U15.fbx | 015_L105_U15.glb | 0.059 | 0.00087 | 0.075 | True |
| 16 | L105_R1 | resistor | ENX-13 | 016_L105_R1.fbx | 016_L105_R1.glb | 0.077 | 0.00087 | 0.075 | True |
| 17 | L105_R2 | resistor | ENX-13 | 017_L105_R2.fbx | 017_L105_R2.glb | 0.095 | 0.00087 | 0.075 | True |
| 18 | L105_R3 | resistor | ENX-13 | 018_L105_R3.fbx | 018_L105_R3.glb | 0.115 | 0.00087 | 0.075 | True |
| 19 | L105_R4 | resistor | ENX-13 | 019_L105_R4.fbx | 019_L105_R4.glb | -0.12 | 0.00087 | 0.064 | True |
| 20 | L105_R5 | resistor | ENX-13 | 020_L105_R5.fbx | 020_L105_R5.glb | -0.069 | 0.00087 | 0.064 | True |
| 21 | L105_R6 | resistor | ENX-13 | 021_L105_R6.fbx | 021_L105_R6.glb | -0.051 | 0.00087 | 0.064 | True |
| 22 | L105_R7 | resistor | ENX-13 | 022_L105_R7.fbx | 022_L105_R7.glb | -0.033 | 0.00087 | 0.064 | True |
| 23 | L105_R8 | resistor | ENX-13 | 023_L105_R8.fbx | 023_L105_R8.glb | -0.014 | 0.00087 | 0.064 | True |
| 24 | L105_R9 | resistor | ENX-13 | 024_L105_R9.fbx | 024_L105_R9.glb | 0.005 | 0.00087 | 0.064 | True |
| 25 | L105_R10 | resistor | ENX-13 | 025_L105_R10.fbx | 025_L105_R10.glb | 0.023 | 0.00087 | 0.064 | True |
| 26 | L105_R11 | resistor | ENX-13 | 026_L105_R11.fbx | 026_L105_R11.glb | 0.041 | 0.00087 | 0.064 | True |
| 27 | L105_R12 | resistor | ENX-13 | 027_L105_R12.fbx | 027_L105_R12.glb | 0.059 | 0.00087 | 0.064 | True |
| 28 | L105_R13 | resistor | ENX-13 | 028_L105_R13.fbx | 028_L105_R13.glb | 0.077 | 0.00087 | 0.064 | True |
| 29 | L105_R14 | resistor | ENX-13 | 029_L105_R14.fbx | 029_L105_R14.glb | 0.095 | 0.00087 | 0.064 | True |
| 30 | L105_R15 | resistor | ENX-13 | 030_L105_R15.fbx | 030_L105_R15.glb | 0.115 | 0.00087 | 0.064 | True |
| 31 | L105_R16 | resistor | ENX-13 | 031_L105_R16.fbx | 031_L105_R16.glb | -0.12 | 0.00087 | 0.052 | True |
| 32 | L105_R17 | resistor | ENX-13 | 032_L105_R17.fbx | 032_L105_R17.glb | -0.104 | 0.00087 | 0.052 | True |
| 33 | L105_R18 | resistor | ENX-13 | 033_L105_R18.fbx | 033_L105_R18.glb | -0.069 | 0.00087 | 0.052 | True |
| 34 | L105_R19 | resistor | ENX-13 | 034_L105_R19.fbx | 034_L105_R19.glb | -0.051 | 0.00087 | 0.052 | True |
| 35 | L105_R20 | resistor | ENX-13 | 035_L105_R20.fbx | 035_L105_R20.glb | -0.033 | 0.00087 | 0.052 | True |
| 36 | L105_R21 | resistor | ENX-13 | 036_L105_R21.fbx | 036_L105_R21.glb | -0.014 | 0.00087 | 0.052 | True |
| 37 | L105_R22 | resistor | ENX-13 | 037_L105_R22.fbx | 037_L105_R22.glb | 0.005 | 0.00087 | 0.052 | True |
| 38 | L105_R23 | resistor | ENX-13 | 038_L105_R23.fbx | 038_L105_R23.glb | 0.023 | 0.00087 | 0.052 | True |
| 39 | L105_R24 | resistor | ENX-13 | 039_L105_R24.fbx | 039_L105_R24.glb | 0.041 | 0.00087 | 0.052 | True |
| 40 | L105_C1 | capacitor | ENX-13 | 040_L105_C1.fbx | 040_L105_C1.glb | 0.059 | 0.00087 | 0.052 | True |
| 41 | L105_C2 | capacitor | ENX-13 | 041_L105_C2.fbx | 041_L105_C2.glb | 0.077 | 0.00087 | 0.052 | True |
| 42 | L105_C3 | capacitor | ENX-13 | 042_L105_C3.fbx | 042_L105_C3.glb | 0.095 | 0.00087 | 0.052 | True |
| 43 | L105_C4 | capacitor | ENX-13 | 043_L105_C4.fbx | 043_L105_C4.glb | 0.115 | 0.00087 | 0.052 | True |
| 44 | L105_C5 | capacitor | ENX-13 | 044_L105_C5.fbx | 044_L105_C5.glb | -0.12 | 0.00087 | 0.041 | True |
| 45 | L105_C6 | capacitor | ENX-13 | 045_L105_C6.fbx | 045_L105_C6.glb | -0.104 | 0.00087 | 0.041 | True |
| 46 | L105_C7 | capacitor | ENX-13 | 046_L105_C7.fbx | 046_L105_C7.glb | -0.087 | 0.00087 | 0.041 | True |
| 47 | L105_C8 | capacitor | ENX-13 | 047_L105_C8.fbx | 047_L105_C8.glb | -0.069 | 0.00087 | 0.041 | True |
| 48 | L105_C9 | capacitor | ENX-13 | 048_L105_C9.fbx | 048_L105_C9.glb | -0.051 | 0.00087 | 0.041 | True |
| 49 | L105_C10 | capacitor | ENX-13 | 049_L105_C10.fbx | 049_L105_C10.glb | -0.033 | 0.00087 | 0.041 | True |
| 50 | L105_C11 | capacitor | ENX-13 | 050_L105_C11.fbx | 050_L105_C11.glb | -0.014 | 0.00087 | 0.041 | True |
| 51 | L105_C12 | capacitor | ENX-13 | 051_L105_C12.fbx | 051_L105_C12.glb | 0.005 | 0.00087 | 0.041 | True |
| 52 | L105_C13 | capacitor | ENX-13 | 052_L105_C13.fbx | 052_L105_C13.glb | 0.023 | 0.00087 | 0.041 | True |
| 53 | L105_C14 | capacitor | ENX-13 | 053_L105_C14.fbx | 053_L105_C14.glb | 0.041 | 0.00087 | 0.041 | True |
| 54 | L105_C15 | capacitor | ENX-13 | 054_L105_C15.fbx | 054_L105_C15.glb | 0.059 | 0.00087 | 0.041 | True |
| 55 | L105_C16 | capacitor | ENX-13 | 055_L105_C16.fbx | 055_L105_C16.glb | 0.077 | 0.00087 | 0.041 | True |
| 56 | L105_C17 | capacitor | ENX-13 | 056_L105_C17.fbx | 056_L105_C17.glb | 0.095 | 0.00087 | 0.041 | True |
| 57 | L105_C18 | capacitor | ENX-13 | 057_L105_C18.fbx | 057_L105_C18.glb | 0.115 | 0.00087 | 0.041 | True |
| 58 | L105_C19 | capacitor | ENX-13 | 058_L105_C19.fbx | 058_L105_C19.glb | -0.12 | 0.00087 | 0.03 | True |
| 59 | L105_C20 | capacitor | ENX-13 | 059_L105_C20.fbx | 059_L105_C20.glb | -0.104 | 0.00087 | 0.03 | True |
| 60 | L105_C21 | capacitor | ENX-13 | 060_L105_C21.fbx | 060_L105_C21.glb | -0.087 | 0.00087 | 0.03 | True |
| 61 | L105_C22 | capacitor | ENX-13 | 061_L105_C22.fbx | 061_L105_C22.glb | -0.069 | 0.00087 | 0.03 | True |
| 62 | L105_C23 | capacitor | ENX-13 | 062_L105_C23.fbx | 062_L105_C23.glb | -0.051 | 0.00087 | 0.03 | True |
| 63 | L105_C24 | capacitor | ENX-13 | 063_L105_C24.fbx | 063_L105_C24.glb | -0.033 | 0.00087 | 0.03 | True |
| 64 | L105_C25 | capacitor | ENX-13 | 064_L105_C25.fbx | 064_L105_C25.glb | -0.014 | 0.00087 | 0.03 | True |
| 65 | L105_C26 | capacitor | ENX-13 | 065_L105_C26.fbx | 065_L105_C26.glb | 0.005 | 0.00087 | 0.03 | True |
| 66 | L105_C27 | capacitor | ENX-13 | 066_L105_C27.fbx | 066_L105_C27.glb | 0.023 | 0.00087 | 0.03 | True |
| 67 | L105_C28 | capacitor | ENX-13 | 067_L105_C28.fbx | 067_L105_C28.glb | 0.041 | 0.00087 | 0.03 | True |
| 68 | L105_C29 | capacitor | ENX-13 | 068_L105_C29.fbx | 068_L105_C29.glb | 0.059 | 0.00087 | 0.03 | True |
| 69 | L105_FB1 | ferrite | ENX-13 | 069_L105_FB1.fbx | 069_L105_FB1.glb | 0.077 | 0.00087 | 0.03 | True |
| 70 | L105_FB2 | ferrite | ENX-13 | 070_L105_FB2.fbx | 070_L105_FB2.glb | 0.095 | 0.00087 | 0.03 | True |
| 71 | L105_FB3 | ferrite | ENX-13 | 071_L105_FB3.fbx | 071_L105_FB3.glb | 0.115 | 0.00087 | 0.03 | True |
| 72 | L105_FB4 | ferrite | ENX-13 | 072_L105_FB4.fbx | 072_L105_FB4.glb | -0.12 | 0.00087 | 0.019 | True |
| 73 | L105_FB5 | ferrite | ENX-13 | 073_L105_FB5.fbx | 073_L105_FB5.glb | -0.104 | 0.00087 | 0.019 | True |
| 74 | L105_FB6 | ferrite | ENX-13 | 074_L105_FB6.fbx | 074_L105_FB6.glb | -0.087 | 0.00087 | 0.019 | True |
| 75 | L105_Y1 | clock | ENX-13 | 075_L105_Y1.fbx | 075_L105_Y1.glb | -0.085 | 0.00087 | 0.059 | True |
| 76 | L105_Y2 | clock | ENX-13 | 076_L105_Y2.fbx | 076_L105_Y2.glb | -0.069 | 0.00087 | 0.019 | True |
| 77 | L105_J1 | connector | ENX-13 | 077_L105_J1.fbx | 077_L105_J1.glb | 0.032 | 0.00087 | -0.018 | True |
| 78 | L105_J2 | connector | ENX-13 | 078_L105_J2.fbx | 078_L105_J2.glb | -0.11 | 0.00087 | 0.065 | True |
| 79 | L105_J3 | connector | ECX-4 | 079_L105_J3.fbx | 079_L105_J3.glb | -0.008 | 0.00087 | 0.012 | True |
| 80 | L105_J4 | connector | ENX-13 | 080_L105_J4.fbx | 080_L105_J4.glb | -0.014 | 0.00087 | 0.019 | True |
| 81 | L105_J5 | connector | ENX-13 | 081_L105_J5.fbx | 081_L105_J5.glb | 0.005 | 0.00087 | 0.019 | True |
| 82 | L105_J6 | connector | ENX-13 | 082_L105_J6.fbx | 082_L105_J6.glb | 0.023 | 0.00087 | 0.019 | True |
| 83 | L105_J7 | connector | ECX-4 | 083_L105_J7.fbx | 083_L105_J7.glb | 0.003 | 0.00087 | 0.012 | True |
| 84 | L105_LED1 | led | ENX-13 | 084_L105_LED1.fbx | 084_L105_LED1.glb | 0.041 | 0.00087 | 0.019 | True |
| 85 | L105_LED2 | led | ENX-13 | 085_L105_LED2.fbx | 085_L105_LED2.glb | 0.059 | 0.00087 | 0.019 | True |
| 86 | L105_LED3 | led | ENX-13 | 086_L105_LED3.fbx | 086_L105_LED3.glb | 0.077 | 0.00087 | 0.019 | True |
| 87 | L105_LED4 | led | ENX-13 | 087_L105_LED4.fbx | 087_L105_LED4.glb | 0.095 | 0.00087 | 0.019 | True |
| 88 | L105_LED5 | led | ECX-4 | 088_L105_LED5.fbx | 088_L105_LED5.glb | 0.014 | 0.00087 | 0.012 | True |
| 89 | L105_SW1 | switch | ENX-13 | 089_L105_SW1.fbx | 089_L105_SW1.glb | 0.115 | 0.00087 | 0.019 | True |
| 90 | L105_SW2 | switch | ENX-13 | 090_L105_SW2.fbx | 090_L105_SW2.glb | -0.12 | 0.00087 | 0.006 | True |
| 91 | L105_SW3 | switch | ENX-13 | 091_L105_SW3.fbx | 091_L105_SW3.glb | -0.104 | 0.00087 | 0.006 | True |
| 92 | L105_D1 | protection | ENX-13 | 092_L105_D1.fbx | 092_L105_D1.glb | -0.087 | 0.00087 | 0.006 | True |
| 93 | L105_D2 | protection | ENX-13 | 093_L105_D2.fbx | 093_L105_D2.glb | -0.069 | 0.00087 | 0.006 | True |
| 94 | L105_D3 | protection | ENX-13 | 094_L105_D3.fbx | 094_L105_D3.glb | -0.051 | 0.00087 | 0.006 | True |
| 95 | L105_D4 | protection | ENX-13 | 095_L105_D4.fbx | 095_L105_D4.glb | 0.023 | 0.00087 | 0.006 | True |
| 96 | L105_D5 | protection | ENX-13 | 096_L105_D5.fbx | 096_L105_D5.glb | 0.041 | 0.00087 | 0.006 | True |
| 97 | L105_SN1 | sensor | ENX-13 | 097_L105_SN1.fbx | 097_L105_SN1.glb | 0.059 | 0.00087 | 0.006 | True |
| 98 | L105_SN2 | sensor | ENX-13 | 098_L105_SN2.fbx | 098_L105_SN2.glb | 0.077 | 0.00087 | 0.006 | True |
| 99 | L105_SN3 | sensor | ENX-13 | 099_L105_SN3.fbx | 099_L105_SN3.glb | 0.095 | 0.00087 | 0.006 | True |
| 100 | L105_W133 | jumper | ENX-13 | 100_L105_W133.fbx | 100_L105_W133.glb | 0.108 | 0.00087 | -0.065 | True |
| 101 | PCB_ENX13_270x190mm | board | ENX-13 | 101_L105_PCB_ENX13_270x190mm.fbx | 101_L105_PCB_ENX13_270x190mm.glb | 0 | 0 | 0 | False |
| 102 | L1_CopperReference | layer | ENX-13 | 102_L105_L1_CopperReference.fbx | 102_L105_L1_CopperReference.glb | 0 | 0 | 0 | False |
| 103 | L2_CopperReference | layer | ENX-13 | 103_L105_L2_CopperReference.fbx | 103_L105_L2_CopperReference.glb | 0 | 0 | 0 | False |
| 104 | L3_CopperReference | layer | ENX-13 | 104_L105_L3_CopperReference.fbx | 104_L105_L3_CopperReference.glb | 0 | 0 | 0 | False |
| 105 | L4_CopperReference | layer | ENX-13 | 105_L105_L4_CopperReference.fbx | 105_L105_L4_CopperReference.glb | 0 | 0 | 0 | False |
| 106 | L5_CopperReference | layer | ENX-13 | 106_L105_L5_CopperReference.fbx | 106_L105_L5_CopperReference.glb | 0 | 0 | 0 | False |
| 107 | L6_CopperReference | layer | ENX-13 | 107_L105_L6_CopperReference.fbx | 107_L105_L6_CopperReference.glb | 0 | 0 | 0 | False |
| 108 | L7_CopperReference | layer | ENX-13 | 108_L105_L7_CopperReference.fbx | 108_L105_L7_CopperReference.glb | 0 | 0 | 0 | False |
| 109 | L8_CopperReference | layer | ENX-13 | 109_L105_L8_CopperReference.fbx | 109_L105_L8_CopperReference.glb | 0 | 0 | 0 | False |
| 110 | ECX4_RemovableCard | card | ECX-4 | 110_L105_ECX4_RemovableCard.fbx | 110_L105_ECX4_RemovableCard.glb | 0 | 0 | 0 | False |
| 111 | PCIeX4_ConnectorContacts | slot | ENX-13 | 111_L105_PCIeX4_ConnectorContacts.fbx | 111_L105_PCIeX4_ConnectorContacts.glb | 0 | 0 | 0 | False |
| 112 | ECX4_GoldEdgeFingers | fingers | ENX-13 | 112_L105_ECX4_GoldEdgeFingers.fbx | 112_L105_ECX4_GoldEdgeFingers.glb | 0 | 0 | 0 | False |
| 113 | PCIe_SpringContacts | contacts | ENX-13 | 113_L105_PCIe_SpringContacts.fbx | 113_L105_PCIe_SpringContacts.glb | 0 | 0 | 0 | False |
| 114 | ConnectorRetentionLatch | latch | ENX-13 | 114_L105_ConnectorRetentionLatch.fbx | 114_L105_ConnectorRetentionLatch.glb | 0 | 0 | 0 | False |
| 115 | ECX4_CardDockingAnchor | latch | ENX-13 | 115_L105_ECX4_CardDockingAnchor.fbx | 115_L105_ECX4_CardDockingAnchor.glb | 0 | 0 | 0 | False |
| 116 | PowerContactPads | pads | ENX-13 | 116_L105_PowerContactPads.fbx | 116_L105_PowerContactPads.glb | 0 | 0 | 0 | False |
| 117 | LanePairs_CopperDetail | trace | ENX-13 | 117_L105_LanePairs_CopperDetail.fbx | 117_L105_LanePairs_CopperDetail.glb | 0 | 0 | 0 | False |
| 118 | ClockPair_CopperDetail | trace | ENX-13 | 118_L105_ClockPair_CopperDetail.fbx | 118_L105_ClockPair_CopperDetail.glb | 0 | 0 | 0 | False |
| 119 | GroundVia | via | ENX-13 | 119_L105_GroundVia.fbx | 119_L105_GroundVia.glb | 0 | 0 | 0 | False |
| 120 | BlindVia | via | ENX-13 | 120_L105_BlindVia.fbx | 120_L105_BlindVia.glb | 0 | 0 | 0 | False |
| 121 | PowerVia | via | ENX-13 | 121_L105_PowerVia.fbx | 121_L105_PowerVia.glb | 0 | 0 | 0 | False |
| 122 | LaneTestPads | pads | ENX-13 | 122_L105_LaneTestPads.fbx | 122_L105_LaneTestPads.glb | 0 | 0 | 0 | False |
| 123 | Fault_Lane2_ACCap | fault | ENX-13 | 123_L105_Fault_Lane2_ACCap.fbx | 123_L105_Fault_Lane2_ACCap.glb | 0 | 0 | 0 | False |
| 124 | Fault_ReferenceClock | fault | ENX-13 | 124_L105_Fault_ReferenceClock.fbx | 124_L105_Fault_ReferenceClock.glb | 0 | 0 | 0 | False |
| 125 | Fault_Lane3Contact | fault | ENX-13 | 125_L105_Fault_Lane3Contact.fbx | 125_L105_Fault_Lane3Contact.glb | 0 | 0 | 0 | False |
| 126 | ConnectorRepairState | fault | ENX-13 | 126_L105_ConnectorRepairState.fbx | 126_L105_ConnectorRepairState.glb | 0 | 0 | 0 | False |
| 127 | ENX13_Silkscreen | mark | ENX-13 | 127_L105_ENX13_Silkscreen.fbx | 127_L105_ENX13_Silkscreen.glb | 0 | 0 | 0 | False |
| 128 | ECX4_Silkscreen | mark | ENX-13 | 128_L105_ECX4_Silkscreen.fbx | 128_L105_ECX4_Silkscreen.glb | 0 | 0 | 0 | False |

## ANNEX B — Full component BOM (100)

**Source:** `Data/ComponentBOM.csv`

| ref | kind | description | board |
|---|---|---|---|
| U1 | root | PCIe Gen2-style virtual root complex | ENX-13 |
| U2 | endpoint | ECX-4 endpoint control IC | ECX-4 |
| U3 | ic | Clock, safety, protection or control IC 3 | ENX-13 |
| U4 | ic | Clock, safety, protection or control IC 4 | ENX-13 |
| U5 | ic | Clock, safety, protection or control IC 5 | ENX-13 |
| U6 | ic | Clock, safety, protection or control IC 6 | ENX-13 |
| U7 | ic | Clock, safety, protection or control IC 7 | ENX-13 |
| U8 | ic | Clock, safety, protection or control IC 8 | ENX-13 |
| U9 | ic | Clock, safety, protection or control IC 9 | ENX-13 |
| U10 | ic | Clock, safety, protection or control IC 10 | ECX-4 |
| U11 | ic | Clock, safety, protection or control IC 11 | ECX-4 |
| U12 | ic | Clock, safety, protection or control IC 12 | ENX-13 |
| U13 | ic | Clock, safety, protection or control IC 13 | ENX-13 |
| U14 | ic | Clock, safety, protection or control IC 14 | ENX-13 |
| U15 | ic | Clock, safety, protection or control IC 15 | ENX-13 |
| R1 | resistor | Signal/power resistor 1 | ENX-13 |
| R2 | resistor | Signal/power resistor 2 | ENX-13 |
| R3 | resistor | Signal/power resistor 3 | ENX-13 |
| R4 | resistor | Signal/power resistor 4 | ENX-13 |
| R5 | resistor | Signal/power resistor 5 | ENX-13 |
| R6 | resistor | Signal/power resistor 6 | ENX-13 |
| R7 | resistor | Signal/power resistor 7 | ENX-13 |
| R8 | resistor | Signal/power resistor 8 | ENX-13 |
| R9 | resistor | Signal/power resistor 9 | ENX-13 |
| R10 | resistor | Signal/power resistor 10 | ENX-13 |
| R11 | resistor | Signal/power resistor 11 | ENX-13 |
| R12 | resistor | Signal/power resistor 12 | ENX-13 |
| R13 | resistor | Signal/power resistor 13 | ENX-13 |
| R14 | resistor | Signal/power resistor 14 | ENX-13 |
| R15 | resistor | Signal/power resistor 15 | ENX-13 |
| R16 | resistor | Signal/power resistor 16 | ENX-13 |
| R17 | resistor | Signal/power resistor 17 | ENX-13 |
| R18 | resistor | Signal/power resistor 18 | ENX-13 |
| R19 | resistor | Signal/power resistor 19 | ENX-13 |
| R20 | resistor | Signal/power resistor 20 | ENX-13 |
| R21 | resistor | Signal/power resistor 21 | ENX-13 |
| R22 | resistor | Signal/power resistor 22 | ENX-13 |
| R23 | resistor | Signal/power resistor 23 | ENX-13 |
| R24 | resistor | Signal/power resistor 24 | ENX-13 |
| C1 | capacitor | AC coupling or decoupling capacitor 1 | ENX-13 |
| C2 | capacitor | AC coupling or decoupling capacitor 2 | ENX-13 |
| C3 | capacitor | AC coupling or decoupling capacitor 3 | ENX-13 |
| C4 | capacitor | AC coupling or decoupling capacitor 4 | ENX-13 |
| C5 | capacitor | AC coupling or decoupling capacitor 5 | ENX-13 |
| C6 | capacitor | AC coupling or decoupling capacitor 6 | ENX-13 |
| C7 | capacitor | AC coupling or decoupling capacitor 7 | ENX-13 |
| C8 | capacitor | AC coupling or decoupling capacitor 8 | ENX-13 |
| C9 | capacitor | AC coupling or decoupling capacitor 9 | ENX-13 |
| C10 | capacitor | AC coupling or decoupling capacitor 10 | ENX-13 |
| C11 | capacitor | AC coupling or decoupling capacitor 11 | ENX-13 |
| C12 | capacitor | AC coupling or decoupling capacitor 12 | ENX-13 |
| C13 | capacitor | AC coupling or decoupling capacitor 13 | ENX-13 |
| C14 | capacitor | AC coupling or decoupling capacitor 14 | ENX-13 |
| C15 | capacitor | AC coupling or decoupling capacitor 15 | ENX-13 |
| C16 | capacitor | AC coupling or decoupling capacitor 16 | ENX-13 |
| C17 | capacitor | AC coupling or decoupling capacitor 17 | ENX-13 |
| C18 | capacitor | AC coupling or decoupling capacitor 18 | ENX-13 |
| C19 | capacitor | AC coupling or decoupling capacitor 19 | ENX-13 |
| C20 | capacitor | AC coupling or decoupling capacitor 20 | ENX-13 |
| C21 | capacitor | AC coupling or decoupling capacitor 21 | ENX-13 |
| C22 | capacitor | AC coupling or decoupling capacitor 22 | ENX-13 |
| C23 | capacitor | AC coupling or decoupling capacitor 23 | ENX-13 |
| C24 | capacitor | AC coupling or decoupling capacitor 24 | ENX-13 |
| C25 | capacitor | AC coupling or decoupling capacitor 25 | ENX-13 |
| C26 | capacitor | AC coupling or decoupling capacitor 26 | ENX-13 |
| C27 | capacitor | AC coupling or decoupling capacitor 27 | ENX-13 |
| C28 | capacitor | AC coupling or decoupling capacitor 28 | ENX-13 |
| C29 | capacitor | AC coupling or decoupling capacitor 29 | ENX-13 |
| FB1 | ferrite | Power filter ferrite 1 | ENX-13 |
| FB2 | ferrite | Power filter ferrite 2 | ENX-13 |
| FB3 | ferrite | Power filter ferrite 3 | ENX-13 |
| FB4 | ferrite | Power filter ferrite 4 | ENX-13 |
| FB5 | ferrite | Power filter ferrite 5 | ENX-13 |
| FB6 | ferrite | Power filter ferrite 6 | ENX-13 |
| Y1 | clock | Reference oscillator 1 | ENX-13 |
| Y2 | clock | Reference oscillator 2 | ENX-13 |
| J1 | connector | Expansion or debug connector 1 | ENX-13 |
| J2 | connector | Expansion or debug connector 2 | ENX-13 |
| J3 | connector | Expansion or debug connector 3 | ECX-4 |
| J4 | connector | Expansion or debug connector 4 | ENX-13 |
| J5 | connector | Expansion or debug connector 5 | ENX-13 |
| J6 | connector | Expansion or debug connector 6 | ENX-13 |
| J7 | connector | Expansion or debug connector 7 | ECX-4 |
| LED1 | led | Link/power indicator 1 | ENX-13 |
| LED2 | led | Link/power indicator 2 | ENX-13 |
| LED3 | led | Link/power indicator 3 | ENX-13 |
| LED4 | led | Link/power indicator 4 | ENX-13 |
| LED5 | led | Link/power indicator 5 | ECX-4 |
| SW1 | switch | Service switch 1 | ENX-13 |
| SW2 | switch | Service switch 2 | ENX-13 |
| SW3 | switch | Service switch 3 | ENX-13 |
| D1 | protection | Protection diode 1 | ENX-13 |
| D2 | protection | Protection diode 2 | ENX-13 |
| D3 | protection | Protection diode 3 | ENX-13 |
| D4 | protection | Protection diode 4 | ENX-13 |
| D5 | protection | Protection diode 5 | ENX-13 |
| SN1 | sensor | Board monitor 1 | ENX-13 |
| SN2 | sensor | Board monitor 2 | ENX-13 |
| SN3 | sensor | Board monitor 3 | ENX-13 |
| W133 | jumper | Removable trusted expansion-link readiness jumper | ENX-13 |

## ANNEX C — Component placements (100)

**Source:** `Data/ComponentPlacements.csv`

| ref | board | x_m | y_m | z_m | installed_at_start | footprint_verified |
|---|---|---|---|---|---|---|
| U1 | ENX-13 | -0.046 | 0.00087 | 0.018 | FALSE | FALSE |
| U2 | ECX-4 | -0.011 | 0.00087 | 0.006 | FALSE | FALSE |
| U3 | ENX-13 | -0.12 | 0.00087 | 0.075 | FALSE | FALSE |
| U4 | ENX-13 | -0.104 | 0.00087 | 0.075 | FALSE | FALSE |
| U5 | ENX-13 | -0.087 | 0.00087 | 0.075 | FALSE | FALSE |
| U6 | ENX-13 | -0.069 | 0.00087 | 0.075 | FALSE | FALSE |
| U7 | ENX-13 | -0.051 | 0.00087 | 0.075 | FALSE | FALSE |
| U8 | ENX-13 | -0.033 | 0.00087 | 0.075 | FALSE | FALSE |
| U9 | ENX-13 | -0.014 | 0.00087 | 0.075 | FALSE | FALSE |
| U10 | ECX-4 | -0.03 | 0.00087 | 0.012 | FALSE | FALSE |
| U11 | ECX-4 | -0.019 | 0.00087 | 0.012 | FALSE | FALSE |
| U12 | ENX-13 | 0.005 | 0.00087 | 0.075 | FALSE | FALSE |
| U13 | ENX-13 | 0.023 | 0.00087 | 0.075 | FALSE | FALSE |
| U14 | ENX-13 | 0.041 | 0.00087 | 0.075 | FALSE | FALSE |
| U15 | ENX-13 | 0.059 | 0.00087 | 0.075 | FALSE | FALSE |
| R1 | ENX-13 | 0.077 | 0.00087 | 0.075 | FALSE | FALSE |
| R2 | ENX-13 | 0.095 | 0.00087 | 0.075 | FALSE | FALSE |
| R3 | ENX-13 | 0.115 | 0.00087 | 0.075 | FALSE | FALSE |
| R4 | ENX-13 | -0.12 | 0.00087 | 0.064 | FALSE | FALSE |
| R5 | ENX-13 | -0.069 | 0.00087 | 0.064 | FALSE | FALSE |
| R6 | ENX-13 | -0.051 | 0.00087 | 0.064 | FALSE | FALSE |
| R7 | ENX-13 | -0.033 | 0.00087 | 0.064 | FALSE | FALSE |
| R8 | ENX-13 | -0.014 | 0.00087 | 0.064 | FALSE | FALSE |
| R9 | ENX-13 | 0.005 | 0.00087 | 0.064 | FALSE | FALSE |
| R10 | ENX-13 | 0.023 | 0.00087 | 0.064 | FALSE | FALSE |
| R11 | ENX-13 | 0.041 | 0.00087 | 0.064 | FALSE | FALSE |
| R12 | ENX-13 | 0.059 | 0.00087 | 0.064 | FALSE | FALSE |
| R13 | ENX-13 | 0.077 | 0.00087 | 0.064 | FALSE | FALSE |
| R14 | ENX-13 | 0.095 | 0.00087 | 0.064 | FALSE | FALSE |
| R15 | ENX-13 | 0.115 | 0.00087 | 0.064 | FALSE | FALSE |
| R16 | ENX-13 | -0.12 | 0.00087 | 0.052 | FALSE | FALSE |
| R17 | ENX-13 | -0.104 | 0.00087 | 0.052 | FALSE | FALSE |
| R18 | ENX-13 | -0.069 | 0.00087 | 0.052 | FALSE | FALSE |
| R19 | ENX-13 | -0.051 | 0.00087 | 0.052 | FALSE | FALSE |
| R20 | ENX-13 | -0.033 | 0.00087 | 0.052 | FALSE | FALSE |
| R21 | ENX-13 | -0.014 | 0.00087 | 0.052 | FALSE | FALSE |
| R22 | ENX-13 | 0.005 | 0.00087 | 0.052 | FALSE | FALSE |
| R23 | ENX-13 | 0.023 | 0.00087 | 0.052 | FALSE | FALSE |
| R24 | ENX-13 | 0.041 | 0.00087 | 0.052 | FALSE | FALSE |
| C1 | ENX-13 | 0.059 | 0.00087 | 0.052 | FALSE | FALSE |
| C2 | ENX-13 | 0.077 | 0.00087 | 0.052 | FALSE | FALSE |
| C3 | ENX-13 | 0.095 | 0.00087 | 0.052 | FALSE | FALSE |
| C4 | ENX-13 | 0.115 | 0.00087 | 0.052 | FALSE | FALSE |
| C5 | ENX-13 | -0.12 | 0.00087 | 0.041 | FALSE | FALSE |
| C6 | ENX-13 | -0.104 | 0.00087 | 0.041 | FALSE | FALSE |
| C7 | ENX-13 | -0.087 | 0.00087 | 0.041 | FALSE | FALSE |
| C8 | ENX-13 | -0.069 | 0.00087 | 0.041 | FALSE | FALSE |
| C9 | ENX-13 | -0.051 | 0.00087 | 0.041 | FALSE | FALSE |
| C10 | ENX-13 | -0.033 | 0.00087 | 0.041 | FALSE | FALSE |
| C11 | ENX-13 | -0.014 | 0.00087 | 0.041 | FALSE | FALSE |
| C12 | ENX-13 | 0.005 | 0.00087 | 0.041 | FALSE | FALSE |
| C13 | ENX-13 | 0.023 | 0.00087 | 0.041 | FALSE | FALSE |
| C14 | ENX-13 | 0.041 | 0.00087 | 0.041 | FALSE | FALSE |
| C15 | ENX-13 | 0.059 | 0.00087 | 0.041 | FALSE | FALSE |
| C16 | ENX-13 | 0.077 | 0.00087 | 0.041 | FALSE | FALSE |
| C17 | ENX-13 | 0.095 | 0.00087 | 0.041 | FALSE | FALSE |
| C18 | ENX-13 | 0.115 | 0.00087 | 0.041 | FALSE | FALSE |
| C19 | ENX-13 | -0.12 | 0.00087 | 0.03 | FALSE | FALSE |
| C20 | ENX-13 | -0.104 | 0.00087 | 0.03 | FALSE | FALSE |
| C21 | ENX-13 | -0.087 | 0.00087 | 0.03 | FALSE | FALSE |
| C22 | ENX-13 | -0.069 | 0.00087 | 0.03 | FALSE | FALSE |
| C23 | ENX-13 | -0.051 | 0.00087 | 0.03 | FALSE | FALSE |
| C24 | ENX-13 | -0.033 | 0.00087 | 0.03 | FALSE | FALSE |
| C25 | ENX-13 | -0.014 | 0.00087 | 0.03 | FALSE | FALSE |
| C26 | ENX-13 | 0.005 | 0.00087 | 0.03 | FALSE | FALSE |
| C27 | ENX-13 | 0.023 | 0.00087 | 0.03 | FALSE | FALSE |
| C28 | ENX-13 | 0.041 | 0.00087 | 0.03 | FALSE | FALSE |
| C29 | ENX-13 | 0.059 | 0.00087 | 0.03 | FALSE | FALSE |
| FB1 | ENX-13 | 0.077 | 0.00087 | 0.03 | FALSE | FALSE |
| FB2 | ENX-13 | 0.095 | 0.00087 | 0.03 | FALSE | FALSE |
| FB3 | ENX-13 | 0.115 | 0.00087 | 0.03 | FALSE | FALSE |
| FB4 | ENX-13 | -0.12 | 0.00087 | 0.019 | FALSE | FALSE |
| FB5 | ENX-13 | -0.104 | 0.00087 | 0.019 | FALSE | FALSE |
| FB6 | ENX-13 | -0.087 | 0.00087 | 0.019 | FALSE | FALSE |
| Y1 | ENX-13 | -0.085 | 0.00087 | 0.059 | FALSE | FALSE |
| Y2 | ENX-13 | -0.069 | 0.00087 | 0.019 | FALSE | FALSE |
| J1 | ENX-13 | 0.032 | 0.00087 | -0.018 | FALSE | FALSE |
| J2 | ENX-13 | -0.11 | 0.00087 | 0.065 | FALSE | FALSE |
| J3 | ECX-4 | -0.008 | 0.00087 | 0.012 | FALSE | FALSE |
| J4 | ENX-13 | -0.014 | 0.00087 | 0.019 | FALSE | FALSE |
| J5 | ENX-13 | 0.005 | 0.00087 | 0.019 | FALSE | FALSE |
| J6 | ENX-13 | 0.023 | 0.00087 | 0.019 | FALSE | FALSE |
| J7 | ECX-4 | 0.003 | 0.00087 | 0.012 | FALSE | FALSE |
| LED1 | ENX-13 | 0.041 | 0.00087 | 0.019 | FALSE | FALSE |
| LED2 | ENX-13 | 0.059 | 0.00087 | 0.019 | FALSE | FALSE |
| LED3 | ENX-13 | 0.077 | 0.00087 | 0.019 | FALSE | FALSE |
| LED4 | ENX-13 | 0.095 | 0.00087 | 0.019 | FALSE | FALSE |
| LED5 | ECX-4 | 0.014 | 0.00087 | 0.012 | FALSE | FALSE |
| SW1 | ENX-13 | 0.115 | 0.00087 | 0.019 | FALSE | FALSE |
| SW2 | ENX-13 | -0.12 | 0.00087 | 0.006 | FALSE | FALSE |
| SW3 | ENX-13 | -0.104 | 0.00087 | 0.006 | FALSE | FALSE |
| D1 | ENX-13 | -0.087 | 0.00087 | 0.006 | FALSE | FALSE |
| D2 | ENX-13 | -0.069 | 0.00087 | 0.006 | FALSE | FALSE |
| D3 | ENX-13 | -0.051 | 0.00087 | 0.006 | FALSE | FALSE |
| D4 | ENX-13 | 0.023 | 0.00087 | 0.006 | FALSE | FALSE |
| D5 | ENX-13 | 0.041 | 0.00087 | 0.006 | FALSE | FALSE |
| SN1 | ENX-13 | 0.059 | 0.00087 | 0.006 | FALSE | FALSE |
| SN2 | ENX-13 | 0.077 | 0.00087 | 0.006 | FALSE | FALSE |
| SN3 | ENX-13 | 0.095 | 0.00087 | 0.006 | FALSE | FALSE |
| W133 | ENX-13 | 0.108 | 0.00087 | -0.065 | FALSE | FALSE |

## ANNEX D — 132 original route-planning rows

**Source:** `Data/CopperRoutes_132_REQUIRES_SCHEMATIC.csv`

| id | domain | source_pad | dest_pad | net_id | certifiable |
|---|---|---|---|---|---|
| W001 | PowerGround | UNASSIGNED | UNASSIGNED | UNASSIGNED | FALSE |
| W002 | PowerGround | UNASSIGNED | UNASSIGNED | UNASSIGNED | FALSE |
| W003 | PowerGround | UNASSIGNED | UNASSIGNED | UNASSIGNED | FALSE |
| W004 | PowerGround | UNASSIGNED | UNASSIGNED | UNASSIGNED | FALSE |
| W005 | PowerGround | UNASSIGNED | UNASSIGNED | UNASSIGNED | FALSE |
| W006 | PowerGround | UNASSIGNED | UNASSIGNED | UNASSIGNED | FALSE |
| W007 | PowerGround | UNASSIGNED | UNASSIGNED | UNASSIGNED | FALSE |
| W008 | PowerGround | UNASSIGNED | UNASSIGNED | UNASSIGNED | FALSE |
| W009 | PowerGround | UNASSIGNED | UNASSIGNED | UNASSIGNED | FALSE |
| W010 | PowerGround | UNASSIGNED | UNASSIGNED | UNASSIGNED | FALSE |
| W011 | PowerGround | UNASSIGNED | UNASSIGNED | UNASSIGNED | FALSE |
| W012 | PowerGround | UNASSIGNED | UNASSIGNED | UNASSIGNED | FALSE |
| W013 | PowerGround | UNASSIGNED | UNASSIGNED | UNASSIGNED | FALSE |
| W014 | PowerGround | UNASSIGNED | UNASSIGNED | UNASSIGNED | FALSE |
| W015 | PowerGround | UNASSIGNED | UNASSIGNED | UNASSIGNED | FALSE |
| W016 | PowerGround | UNASSIGNED | UNASSIGNED | UNASSIGNED | FALSE |
| W017 | PowerGround | UNASSIGNED | UNASSIGNED | UNASSIGNED | FALSE |
| W018 | PowerGround | UNASSIGNED | UNASSIGNED | UNASSIGNED | FALSE |
| W019 | ClockReset | UNASSIGNED | UNASSIGNED | UNASSIGNED | FALSE |
| W020 | ClockReset | UNASSIGNED | UNASSIGNED | UNASSIGNED | FALSE |
| W021 | ClockReset | UNASSIGNED | UNASSIGNED | UNASSIGNED | FALSE |
| W022 | ClockReset | UNASSIGNED | UNASSIGNED | UNASSIGNED | FALSE |
| W023 | ClockReset | UNASSIGNED | UNASSIGNED | UNASSIGNED | FALSE |
| W024 | ClockReset | UNASSIGNED | UNASSIGNED | UNASSIGNED | FALSE |
| W025 | ClockReset | UNASSIGNED | UNASSIGNED | UNASSIGNED | FALSE |
| W026 | ClockReset | UNASSIGNED | UNASSIGNED | UNASSIGNED | FALSE |
| W027 | ClockReset | UNASSIGNED | UNASSIGNED | UNASSIGNED | FALSE |
| W028 | ClockReset | UNASSIGNED | UNASSIGNED | UNASSIGNED | FALSE |
| W029 | ClockReset | UNASSIGNED | UNASSIGNED | UNASSIGNED | FALSE |
| W030 | ClockReset | UNASSIGNED | UNASSIGNED | UNASSIGNED | FALSE |
| W031 | ClockReset | UNASSIGNED | UNASSIGNED | UNASSIGNED | FALSE |
| W032 | ClockReset | UNASSIGNED | UNASSIGNED | UNASSIGNED | FALSE |
| W033 | ClockReset | UNASSIGNED | UNASSIGNED | UNASSIGNED | FALSE |
| W034 | ClockReset | UNASSIGNED | UNASSIGNED | UNASSIGNED | FALSE |
| W035 | ClockReset | UNASSIGNED | UNASSIGNED | UNASSIGNED | FALSE |
| W036 | ClockReset | UNASSIGNED | UNASSIGNED | UNASSIGNED | FALSE |
| W037 | ClockReset | UNASSIGNED | UNASSIGNED | UNASSIGNED | FALSE |
| W038 | ClockReset | UNASSIGNED | UNASSIGNED | UNASSIGNED | FALSE |
| W039 | ClockReset | UNASSIGNED | UNASSIGNED | UNASSIGNED | FALSE |
| W040 | ClockReset | UNASSIGNED | UNASSIGNED | UNASSIGNED | FALSE |
| W041 | Lane01 | UNASSIGNED | UNASSIGNED | UNASSIGNED | FALSE |
| W042 | Lane01 | UNASSIGNED | UNASSIGNED | UNASSIGNED | FALSE |
| W043 | Lane01 | UNASSIGNED | UNASSIGNED | UNASSIGNED | FALSE |
| W044 | Lane01 | UNASSIGNED | UNASSIGNED | UNASSIGNED | FALSE |
| W045 | Lane01 | UNASSIGNED | UNASSIGNED | UNASSIGNED | FALSE |
| W046 | Lane01 | UNASSIGNED | UNASSIGNED | UNASSIGNED | FALSE |
| W047 | Lane01 | UNASSIGNED | UNASSIGNED | UNASSIGNED | FALSE |
| W048 | Lane01 | UNASSIGNED | UNASSIGNED | UNASSIGNED | FALSE |
| W049 | Lane01 | UNASSIGNED | UNASSIGNED | UNASSIGNED | FALSE |
| W050 | Lane01 | UNASSIGNED | UNASSIGNED | UNASSIGNED | FALSE |
| W051 | Lane01 | UNASSIGNED | UNASSIGNED | UNASSIGNED | FALSE |
| W052 | Lane01 | UNASSIGNED | UNASSIGNED | UNASSIGNED | FALSE |
| W053 | Lane01 | UNASSIGNED | UNASSIGNED | UNASSIGNED | FALSE |
| W054 | Lane01 | UNASSIGNED | UNASSIGNED | UNASSIGNED | FALSE |
| W055 | Lane01 | UNASSIGNED | UNASSIGNED | UNASSIGNED | FALSE |
| W056 | Lane01 | UNASSIGNED | UNASSIGNED | UNASSIGNED | FALSE |
| W057 | Lane01 | UNASSIGNED | UNASSIGNED | UNASSIGNED | FALSE |
| W058 | Lane01 | UNASSIGNED | UNASSIGNED | UNASSIGNED | FALSE |
| W059 | Lane01 | UNASSIGNED | UNASSIGNED | UNASSIGNED | FALSE |
| W060 | Lane01 | UNASSIGNED | UNASSIGNED | UNASSIGNED | FALSE |
| W061 | Lane01 | UNASSIGNED | UNASSIGNED | UNASSIGNED | FALSE |
| W062 | Lane01 | UNASSIGNED | UNASSIGNED | UNASSIGNED | FALSE |
| W063 | Lane01 | UNASSIGNED | UNASSIGNED | UNASSIGNED | FALSE |
| W064 | Lane01 | UNASSIGNED | UNASSIGNED | UNASSIGNED | FALSE |
| W065 | Lane23 | UNASSIGNED | UNASSIGNED | UNASSIGNED | FALSE |
| W066 | Lane23 | UNASSIGNED | UNASSIGNED | UNASSIGNED | FALSE |
| W067 | Lane23 | UNASSIGNED | UNASSIGNED | UNASSIGNED | FALSE |
| W068 | Lane23 | UNASSIGNED | UNASSIGNED | UNASSIGNED | FALSE |
| W069 | Lane23 | UNASSIGNED | UNASSIGNED | UNASSIGNED | FALSE |
| W070 | Lane23 | UNASSIGNED | UNASSIGNED | UNASSIGNED | FALSE |
| W071 | Lane23 | UNASSIGNED | UNASSIGNED | UNASSIGNED | FALSE |
| W072 | Lane23 | UNASSIGNED | UNASSIGNED | UNASSIGNED | FALSE |
| W073 | Lane23 | UNASSIGNED | UNASSIGNED | UNASSIGNED | FALSE |
| W074 | Lane23 | UNASSIGNED | UNASSIGNED | UNASSIGNED | FALSE |
| W075 | Lane23 | UNASSIGNED | UNASSIGNED | UNASSIGNED | FALSE |
| W076 | Lane23 | UNASSIGNED | UNASSIGNED | UNASSIGNED | FALSE |
| W077 | Lane23 | UNASSIGNED | UNASSIGNED | UNASSIGNED | FALSE |
| W078 | Lane23 | UNASSIGNED | UNASSIGNED | UNASSIGNED | FALSE |
| W079 | Lane23 | UNASSIGNED | UNASSIGNED | UNASSIGNED | FALSE |
| W080 | Lane23 | UNASSIGNED | UNASSIGNED | UNASSIGNED | FALSE |
| W081 | Lane23 | UNASSIGNED | UNASSIGNED | UNASSIGNED | FALSE |
| W082 | Lane23 | UNASSIGNED | UNASSIGNED | UNASSIGNED | FALSE |
| W083 | Lane23 | UNASSIGNED | UNASSIGNED | UNASSIGNED | FALSE |
| W084 | Lane23 | UNASSIGNED | UNASSIGNED | UNASSIGNED | FALSE |
| W085 | Lane23 | UNASSIGNED | UNASSIGNED | UNASSIGNED | FALSE |
| W086 | Lane23 | UNASSIGNED | UNASSIGNED | UNASSIGNED | FALSE |
| W087 | Lane23 | UNASSIGNED | UNASSIGNED | UNASSIGNED | FALSE |
| W088 | Lane23 | UNASSIGNED | UNASSIGNED | UNASSIGNED | FALSE |
| W089 | Connector | UNASSIGNED | UNASSIGNED | UNASSIGNED | FALSE |
| W090 | Connector | UNASSIGNED | UNASSIGNED | UNASSIGNED | FALSE |
| W091 | Connector | UNASSIGNED | UNASSIGNED | UNASSIGNED | FALSE |
| W092 | Connector | UNASSIGNED | UNASSIGNED | UNASSIGNED | FALSE |
| W093 | Connector | UNASSIGNED | UNASSIGNED | UNASSIGNED | FALSE |
| W094 | Connector | UNASSIGNED | UNASSIGNED | UNASSIGNED | FALSE |
| W095 | Connector | UNASSIGNED | UNASSIGNED | UNASSIGNED | FALSE |
| W096 | Connector | UNASSIGNED | UNASSIGNED | UNASSIGNED | FALSE |
| W097 | Connector | UNASSIGNED | UNASSIGNED | UNASSIGNED | FALSE |
| W098 | Connector | UNASSIGNED | UNASSIGNED | UNASSIGNED | FALSE |
| W099 | Connector | UNASSIGNED | UNASSIGNED | UNASSIGNED | FALSE |
| W100 | Connector | UNASSIGNED | UNASSIGNED | UNASSIGNED | FALSE |
| W101 | Connector | UNASSIGNED | UNASSIGNED | UNASSIGNED | FALSE |
| W102 | Connector | UNASSIGNED | UNASSIGNED | UNASSIGNED | FALSE |
| W103 | Connector | UNASSIGNED | UNASSIGNED | UNASSIGNED | FALSE |
| W104 | Connector | UNASSIGNED | UNASSIGNED | UNASSIGNED | FALSE |
| W105 | Connector | UNASSIGNED | UNASSIGNED | UNASSIGNED | FALSE |
| W106 | Connector | UNASSIGNED | UNASSIGNED | UNASSIGNED | FALSE |
| W107 | Diagnostics | UNASSIGNED | UNASSIGNED | UNASSIGNED | FALSE |
| W108 | Diagnostics | UNASSIGNED | UNASSIGNED | UNASSIGNED | FALSE |
| W109 | Diagnostics | UNASSIGNED | UNASSIGNED | UNASSIGNED | FALSE |
| W110 | Diagnostics | UNASSIGNED | UNASSIGNED | UNASSIGNED | FALSE |
| W111 | Diagnostics | UNASSIGNED | UNASSIGNED | UNASSIGNED | FALSE |
| W112 | Diagnostics | UNASSIGNED | UNASSIGNED | UNASSIGNED | FALSE |
| W113 | Diagnostics | UNASSIGNED | UNASSIGNED | UNASSIGNED | FALSE |
| W114 | Diagnostics | UNASSIGNED | UNASSIGNED | UNASSIGNED | FALSE |
| W115 | Diagnostics | UNASSIGNED | UNASSIGNED | UNASSIGNED | FALSE |
| W116 | Diagnostics | UNASSIGNED | UNASSIGNED | UNASSIGNED | FALSE |
| W117 | Diagnostics | UNASSIGNED | UNASSIGNED | UNASSIGNED | FALSE |
| W118 | Diagnostics | UNASSIGNED | UNASSIGNED | UNASSIGNED | FALSE |
| W119 | Diagnostics | UNASSIGNED | UNASSIGNED | UNASSIGNED | FALSE |
| W120 | Diagnostics | UNASSIGNED | UNASSIGNED | UNASSIGNED | FALSE |
| W121 | Safety | UNASSIGNED | UNASSIGNED | UNASSIGNED | FALSE |
| W122 | Safety | UNASSIGNED | UNASSIGNED | UNASSIGNED | FALSE |
| W123 | Safety | UNASSIGNED | UNASSIGNED | UNASSIGNED | FALSE |
| W124 | Safety | UNASSIGNED | UNASSIGNED | UNASSIGNED | FALSE |
| W125 | Safety | UNASSIGNED | UNASSIGNED | UNASSIGNED | FALSE |
| W126 | Safety | UNASSIGNED | UNASSIGNED | UNASSIGNED | FALSE |
| W127 | Safety | UNASSIGNED | UNASSIGNED | UNASSIGNED | FALSE |
| W128 | Safety | UNASSIGNED | UNASSIGNED | UNASSIGNED | FALSE |
| W129 | Safety | UNASSIGNED | UNASSIGNED | UNASSIGNED | FALSE |
| W130 | Safety | UNASSIGNED | UNASSIGNED | UNASSIGNED | FALSE |
| W131 | Safety | UNASSIGNED | UNASSIGNED | UNASSIGNED | FALSE |
| W132 | Safety | UNASSIGNED | UNASSIGNED | UNASSIGNED | FALSE |

## ANNEX E — 38 original via tasks

**Source:** `Data/ViaTasks_38_REQUIRES_SCHEMATIC.csv`

| id | start_layer | end_layer | net_id | verified |
|---|---|---|---|---|
| V01 | TBD | TBD | UNASSIGNED | FALSE |
| V02 | TBD | TBD | UNASSIGNED | FALSE |
| V03 | TBD | TBD | UNASSIGNED | FALSE |
| V04 | TBD | TBD | UNASSIGNED | FALSE |
| V05 | TBD | TBD | UNASSIGNED | FALSE |
| V06 | TBD | TBD | UNASSIGNED | FALSE |
| V07 | TBD | TBD | UNASSIGNED | FALSE |
| V08 | TBD | TBD | UNASSIGNED | FALSE |
| V09 | TBD | TBD | UNASSIGNED | FALSE |
| V10 | TBD | TBD | UNASSIGNED | FALSE |
| V11 | TBD | TBD | UNASSIGNED | FALSE |
| V12 | TBD | TBD | UNASSIGNED | FALSE |
| V13 | TBD | TBD | UNASSIGNED | FALSE |
| V14 | TBD | TBD | UNASSIGNED | FALSE |
| V15 | TBD | TBD | UNASSIGNED | FALSE |
| V16 | TBD | TBD | UNASSIGNED | FALSE |
| V17 | TBD | TBD | UNASSIGNED | FALSE |
| V18 | TBD | TBD | UNASSIGNED | FALSE |
| V19 | TBD | TBD | UNASSIGNED | FALSE |
| V20 | TBD | TBD | UNASSIGNED | FALSE |
| V21 | TBD | TBD | UNASSIGNED | FALSE |
| V22 | TBD | TBD | UNASSIGNED | FALSE |
| V23 | TBD | TBD | UNASSIGNED | FALSE |
| V24 | TBD | TBD | UNASSIGNED | FALSE |
| V25 | TBD | TBD | UNASSIGNED | FALSE |
| V26 | TBD | TBD | UNASSIGNED | FALSE |
| V27 | TBD | TBD | UNASSIGNED | FALSE |
| V28 | TBD | TBD | UNASSIGNED | FALSE |
| V29 | TBD | TBD | UNASSIGNED | FALSE |
| V30 | TBD | TBD | UNASSIGNED | FALSE |
| V31 | TBD | TBD | UNASSIGNED | FALSE |
| V32 | TBD | TBD | UNASSIGNED | FALSE |
| V33 | TBD | TBD | UNASSIGNED | FALSE |
| V34 | TBD | TBD | UNASSIGNED | FALSE |
| V35 | TBD | TBD | UNASSIGNED | FALSE |
| V36 | TBD | TBD | UNASSIGNED | FALSE |
| V37 | TBD | TBD | UNASSIGNED | FALSE |
| V38 | TBD | TBD | UNASSIGNED | FALSE |

## ANNEX F — W133 recovery record

```json
{
  "id": "W133",
  "logical_source": "L105_LinkTrainingMonitor_U12.TRUSTED_EXPANSION_READY_OUT",
  "logical_destination": "L105_ExpansionSafetySupervisor_U13.TRUSTED_EXPANSION_READY_IN",
  "reconcile_physical_pins": true,
  "initial": "OPEN",
  "certifiable": false,
  "recovery": [
    "reconnect jumper",
    "retrain x4 to L0",
    "new enumeration",
    "fresh transfer tests",
    "explicit reset"
  ]
}
```

## ANNEX G — fault plan

```json
{
  "faults": [
    {
      "id": "F1",
      "title": "Lane 2 AC coupling capacitor open"
    },
    {
      "id": "F2",
      "title": "100 MHz reference-clock integrity"
    },
    {
      "id": "F3",
      "title": "Lane 3 intermittent slot contact"
    }
  ]
}
```

## ANNEX H — objectives and initial state

```json
{
  "level": 105,
  "name": "EXPANSION NEXUS",
  "objectives": [
    "Assemble 100 components",
    "Complete 132 copper and 38 vias",
    "Commission 5GT/s x4 link",
    "Enumerate endpoint and test transfers",
    "Recover 3 faults and requalify",
    "W133 removal and recovery"
  ],
  "initial": {
    "components": 0,
    "routes": 0,
    "vias": 0,
    "W133": "OPEN",
    "card_inserted": false
  },
  "implemented": false
}
```

## ANNEX I — export summary

```json
{
  "level": 105,
  "model": "ENX-13",
  "endpoint_card": "ECX-4",
  "modular_fbx": 128,
  "modular_glb": 128,
  "assembled_fbx": 1,
  "installable_components": 100,
  "board_extra": 28,
  "PBR_families": 16,
  "texture_maps": 49,
  "routes_unverified": 132,
  "vias_unverified": 38,
  "unity_tested": false
}
```

## ANNEX J — Original art-board builder script

**Source:** `Unity/Editor/SparkL105BoardBuilder.cs`

```csharp
#if UNITY_EDITOR
using System; using System.IO; using UnityEditor; using UnityEngine;
namespace ProjectSpark.Level105.Editor { public static class SparkL105BoardBuilder {
 static string Find(string filename){foreach(var guid in AssetDatabase.FindAssets(Path.GetFileNameWithoutExtension(filename))){var p=AssetDatabase.GUIDToAssetPath(guid);if(p.EndsWith(filename,StringComparison.OrdinalIgnoreCase))return p;}return null;}
 [MenuItem("Tools/Project Spark/Levels/105/1. Build EMPTY ENX-13 Board")]
 static void Build(){var path=Find("101_L105_PCB_ENX13_270x190mm.fbx");if(path==null){Debug.LogError("[L105] Missing board FBX.");return;}var root=new GameObject("L105_ENX13_EmptyArt");var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(path);var child=(GameObject)PrefabUtility.InstantiatePrefab(prefab);child.transform.SetParent(root.transform,false);child.name="PCB_VISUAL_ONLY";Selection.activeGameObject=root;Debug.Log("[L105] Board art only. Gameplay unassembled and not certified.");}
 [MenuItem("Tools/Project Spark/Levels/105/2. Open Assembled ART Reference")]
 static void Reference(){var p=Find("129_L105_AssembledMainboardReference.fbx");if(p==null){Debug.LogError("Missing reference FBX");return;}var go=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(p));go.name="ART_REFERENCE_NOT_GAMEPLAY";Selection.activeGameObject=go;}
} }
#endif

```

## ANNEX K — Original material builder (contains source syntax error)

**Source:** `Unity/Editor/SparkL105MaterialBuilder.cs`

```csharp
#if UNITY_EDITOR
using System; using System.IO; using UnityEditor; using UnityEngine;
namespace ProjectSpark.Level105.Editor { public static class SparkL105MaterialBuilder {
 [MenuItem("Tools/Project Spark/Levels/105/3. Generate Starter PBR Materials")]
 static void Generate(){string root=null;foreach(var g in AssetDatabase.FindAssets("pcb_Albedo t:Texture2D")){var p=AssetDatabase.GUIDToAssetPath(g);if(p.Contains("Level105")||p.Contains("ExpansionNexus")){root=Path.GetDirectoryName(p).Replace('\','/');break;}}if(root==null){Debug.LogError("[L105] Textures missing.");return;}
 var dest=Path.GetDirectoryName(root).Replace('\','/')+"/UnityGeneratedMaterials";if(!AssetDatabase.IsValidFolder(dest))AssetDatabase.CreateFolder(Path.GetDirectoryName(dest).Replace('\','/'),"UnityGeneratedMaterials");
 string[] ids={"pcb","mask","copper","gold","silver","tin","black","chip","ceramic","resistor","green","red","white","core","dielectric","solder"};foreach(var id in ids){var shader=Shader.Find("Universal Render Pipeline/Lit")??Shader.Find("Standard");if(shader==null)continue;var mat=new Material(shader);mat.name="L105_"+id;var tex=AssetDatabase.LoadAssetAtPath<Texture2D>(root+"/"+id+"_Albedo.png");if(mat.HasProperty("_BaseMap"))mat.SetTexture("_BaseMap",tex);if(mat.HasProperty("_MainTex"))mat.SetTexture("_MainTex",tex);if(mat.HasProperty("_Metallic"))mat.SetFloat("_Metallic",(id=="gold"||id=="silver"||id=="copper"||id=="tin"||id=="solder")?0.88f:0f);if(mat.HasProperty("_Smoothness"))mat.SetFloat("_Smoothness",0.5f);var path=dest+"/"+mat.name+".mat";if(AssetDatabase.LoadAssetAtPath<Material>(path)==null)AssetDatabase.CreateAsset(mat,path);else UnityEngine.Object.DestroyImmediate(mat);}AssetDatabase.SaveAssets();Debug.Log("[L105] Starter materials ready; FBX slot mapping and URP masks need checking.");}
} }
#endif

```

## ANNEX L — Source package paths and verification constraints

```text
Level105_ENX13/
  Models_FBX/ (128 modular FBXs + 129_L105_AssembledMainboardReference.fbx)
  Models_GLB/ (128 modular GLBs + 129_L105_AssembledMainboardReference.glb)
  Textures/
  Data/ (original CSV and JSON manifests included above)
  Unity/Editor/ (two art-only helper scripts included above)
  Documentation/README_FIRST.md
```

Source package explicitly states “procedural art foundation, not hand-authored AAA final meshes or manufacturer-qualified PCIe footprints.” Do not misrepresent the kit as validated electrical hardware. All implementation decisions requiring exact pinout, package dimensions, power sequencing or link timing remain subject to approved engineering data.

**MASTER PROMPT END — Level 105: EXPANSION NEXUS**

