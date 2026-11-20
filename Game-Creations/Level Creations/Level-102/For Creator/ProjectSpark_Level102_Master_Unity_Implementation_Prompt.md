# PROJECT SPARK — LEVEL 102: SIGNAL PHANTOM
## MASTER UNITY IMPLEMENTATION PROMPT — SOURCE-GROUNDED PRODUCTION SPECIFICATION

**Mission:** HSX-10 High-Speed Signal Integrity Evaluation Board  
**Scope:** Board-only Unity gameplay and scene builder, no room/furniture/physical instrumentation  
**Source archive:** `ProjectSpark_Level102_SignalPhantom_BoardOnly_Realistic_Complete.zip`  
**Status:** Implementation instructions, NOT a compiled or tested Unity level  
**Authority:** Existing Project Spark runtime electrical graph and `LevelGamePlayManager`; never make visual/UI state authoritative.

> **MASTER TASK FOR UNITY IMPLEMENTATION ASSISTANT** — Inspect the actual Unity project, the attached Level 102 source archive and Levels 94–101 before writing integration scripts. Implement a physically coherent but computationally bounded educational high-speed PCB simulation, build the level from the supplied board-only assets, integrate virtual instruments and repair tools, then make all six objectives pass only upon real measured proof. No fabricated success checks, hard-coded faux measurements, bypasses or cross-level changes.

### Source-of-truth hierarchy
1. Actual imported Unity code and authoritative solver APIs, inspected locally, control runtime integration.
2. The supplied archive's `Data/*` and named FBXs control asset identity, positions and source-design task counts.
3. The archive's `GameplayPlan.json` governs the named faults, W109 endpoints and objective categories.
4. This specification governs requirements for systems **not supplied** in the archive; proposed classes, thresholds and tests are implementation targets, not claims of existing code.
5. Where proposed behavior conflicts with verified electronic pinout or real solver capability, document the conflict and implement explicit validated adapters rather than silent guesses.

**Do not mark any Level 102 route/via certifiable until its pad endpoints and net are assigned and confirmed.** The 108 source route rows contain `UNASSIGNED` endpoints and `UNVERIFIED` nets, and all 28 via tasks are unassigned. Their `certifiable=false` flags are intentional and must be retained until an authorized engineering reconciliation is complete.

## 01 — Exact packaged asset summary

| Asset/data | Source count | Interpretation |
|---|---:|---|
| Modular FBXs + corresponding GLBs | 104 each | 82 installable + 22 board detail |
| Reference FBX and GLB | 1 each | Showcase only, NEVER playable board |
| Modular mesh triangles | 30,796 | Source art budget, not final production quality |
| Modular mesh vertices | 19,242 | Source art budget |
| PBR material families | 18 | Reference materials |
| PBR maps | 90 | Five maps per family |
| Silkscreen references | 1 | Art reference |
| Player copper route tasks | 108 | ALL UNVERIFIED |
| Via placement tasks | 28 | ALL UNVERIFIED |
| PCB physical size | 250 × 175 mm | Preserve real-world units |
| Copper layers | 6 | Layer aware topology required |
| Initial W109 state | disconnected | Removable digital health path |

**Art limitation:** FBXs are procedurally created, with reference PBR maps, not manufacturer-certified CAD or production-grade textured bakes. Verify FBX import settings, UVs, scale, normals, material assignment, alpha/metallic/smoothness packing, collider selection, and package dimensions inside Unity. Avoid claiming “AAA finished” based on the asset label.

## 02 — Hard design rules and scope

- Use only the HSX-10 board and component FBXs for physical Level 102 art. Do not generate another room, chair, bench, tools, oscilloscope or UI-as-FBX.
- Reuse Project Spark's existing world-space virtual PCB inspection/repair UI for multimeter, oscilloscope, TDR, eye analyzer, net tracer, solder/desolder, via repair, component replacement, differential lane test and calibration.
- The world-space dashboard is presentation and interaction; the solver is the authority for voltages, topology, state, connection validity, fault repair and objective proof.
- Existing Levels 1–101 remain structurally and behaviorally unchanged. Level 102 code must be namespaced and non-invasive.
- Never replace the original electrical simulation with a graph displayed only as copper art.
- Never mix simulated voltage domains with cosmetic safety indicators or substitute arbitrary target counts for real validation.
- Keep real PCB physical dimensions separate from optional world-space visual enlargement; gameplay calculations use metres/mm as appropriate, never presentation scale.
- Make repeatable results possible with deterministic seeded noise and fixed-step state simulation. Explicitly label approximations and bounds.
- Support PC mouse and mobile touch. Tool selection must work from World-Space Canvas; picking must use valid board targets, not nearest decorative meshes.
- Logging and error feedback must identify missing source data, invalid connector types, unavailable integration APIs and rejected repairs.

## 03 — First inspect Project Spark APIs — no invented integration signatures

Locate the real definitions and references for `LevelGamePlayManager`, `SparkTerminal`, circuit solver / topology management, PCB footprint/trace/via classes, item placement/shop integration, board viewer, touch raycasters, repair/authority interface, fault controller and UI panel tool selection. Inspect the actual namespaces, field names, lifecycle hooks, save mechanisms and events before authoring adapters.

Produce a short integration report: known type names and files, canonical connectivity API, unique stable ID patterns, how a player creates/removes copper traces, how vias are represented, how the completed task is verified, how objective state is reported, and how audio/VFX and gameplay are bound. If an API is missing, add a Level-102-only documented adapter or common reusable system without breaking earlier levels. Do not edit legacy scripts unless indispensable; include regression tests for each changed shared API.

### Stage gates
- **G0**: actual Unity APIs inspected and documented.
- **G1**: pin-verified schematic/netlist resolves all 108 routes, all 28 vias and W109 contacts.
- **G2**: empty board FBXs import at the correct scale and all component footprints bind.
- **G3**: electrical graph, safety controls, dynamic lane measurements and UI interaction wired up.
- **G4**: six objectives pass/fail from runtime evidence; negative tests reject shortcuts.
- **G5**: compilation, Play Mode and regression tests pass in the real project.

Do not jump from G0 directly to declaring G5 success.

## 04 — One-click scene builder and folder organization

**Desired menu:** `Tools/Project Spark/Levels/Create Level 102`

Builder creates or updates `L102_SignalPhantom` under the existing game-room level host; never duplicates the room. Maintain stable IDs and idempotency. Suggested internal roots:

```text
L102_SignalPhantom
  GameplayAuthority
    BoardElectricalGraph
    LayerStackAndNets
    FootprintPlacementAuthority
    TraceRoutingAuthority
    ViaPlacementAuthority
    HSX10PowerClockAndControls
    LaneA_TransmissionModel
    LaneB_TransmissionModel
    SignalMeasurementAuthority
    SafetySupervisor_W109
    FaultAuthority
    ObjectiveCertification
  Visuals
    PCB_Base
    InstalledComponentVisuals (empty initially)
    CopperView (factory copper only)
    DynamicTraceVisuals (empty)
    DynamicViaVisuals (empty)
    FaultOverlays (off initially)
    LayerInspectionOverlay (art only)
  Interaction
    BoardPickTargets
    InspectionAndRepairBridge
    WorkstationHUD (existing world-space virtual UI adapter)
  Testing
    HiddenSimulationTestHooks (Editor/testing only; disable in build)
```

Use `PrefabUtility.InstantiatePrefab` in Editor, Undo operations and stable serialized references. Re-running builder must not destroy player save data, duplicate authorities or create precompleted trace objects. Restore existing selection/scene references where feasible.

## 05 — Initial level state and component installation

Gameplay state at first load: `installedComponents=0/82`, `completedCopperRoutes=0/108`, `installedViaTasks=0/28`, `W109Connected=false`, `certifiedObjectives=0/6`. This does NOT mean the physically required permanent factory copper is absent; explicitly model immutable factory nets, and ensure they cannot bypass a required player route.

For every component row in `ComponentBOM.csv`, create a unique stable component identity, package/footprint association, nominal electrical value and permitted orientation. Use source `ComponentPlacements.csv` for artwork local positions, but derive true pad/land patterns from reviewed manufacturer-style pin geometry rather than blindly assuming the model mesh supplies electrical pin identities. The asset `L102_Assembled_Reference.fbx` must exist only as a disabled showcase or separate preview, never an interactive gameplay child. Component selection from the shop/shelf should follow the existing item acquisition logic without granting installation unless the actual board authority accepted it.

Check for polarity, pin-one orientation, component value, solder contact quality, missing leads, shorted leads and electrical domain compatibility. A component is installed only after an authorized placement action. Replacing a component must remove/restore the correct pad connections and invalidate any measurements that depend on that part.

## 06 — Pin-level schematic and pad-net reconciliation (mandatory)

**Source fact:** Every row `W001–W108` has `source_pad=UNASSIGNED`, `destination_pad=UNASSIGNED`, `net_id=UNVERIFIED`, `layer=UNASSIGNED`, `certifiable=false`; every via `V01–V28` is likewise unverified. Do not invent certified pin numbers from rendered leads.

Engineering deliverables required before functional gameplay:

1. `L102_ApprovedSchematic.json` or equivalent reviewed document listing all MCU/transceiver/clock/termination/monitor package pins.
2. `L102_PadAndFootprints.csv`: component ref, manufacturer-style package, pad index, local position, net membership, orientation and electrical type.
3. `L102_Nets.csv`: unique net ID, voltage domain, behavior, I/O direction, DC and optional frequency-dependent constraints.
4. `L102_FactoryCopper.csv`: permanent source traces, geometry, layer, termination and true connectivity.
5. `L102_PlayerRoutes_108.csv`: exact pair/branches, segment geometry/layer allowed, clearance, routing rules, cost and required completion state.
6. `L102_Vias_28.csv`: exact net and layer span, manufacturing kind, drill/barrel and installation positions.
7. `L102_ApprovedStackup.csv`: six layers, copper thickness, dielectric thickness/permittivity, controlled-impedance geometry and reference planes.
8. `L102_SafetyW109.json`: true monitor output pad, detachable test fixture and independent supervisor input pad.
9. Independent consistency validator refusing duplicate pin assignments, undefined nets, cross-net shorting, nonphysical via spans, factory bypass and missing required branches.

If exact information is absent, preserve source rows as `UNVERIFIED`, show a configuration error to engineers, and prevent objective certification. Do not auto-fill missing nets using generic alternating power/signal patterns.

## 07 — Six-layer stack, vias and actual topology

Use L1 and L6 outer signal/component layers; L2/L5 ground reference and L3/L4 signal/power as defined only by the approved stack-up. A via connects exactly its declared conductive spans (through, blind or buried as approved), not all layers by default. Validate pad/via clearance, plated barrel connection, antipads, grounded stitching and plated ground reference. Distinguish a **via stub** on an otherwise conductive path from an **open via**; the F3 reflection fault is not a broken continuity defect.

Require route overlap and component attachment only on intended copper regions. A decorative layer cutaway must not change layer topology. An art-only model may be visible when a route is pending but must never expose a functional connection unless that route actually exists in the graph.

## 08 — Power, control and high-speed channel initialization

Separate protected 5 V input, 3.3 V logic supply and 1.8 V transceiver supply. Model power-up time, regulator tolerance, decoupling, correct ground return, reset and oscillator availability. Require appropriate qualification before link training or full-speed tests. Preserve the MCU as the **control processor**, not necessarily a real 1 Gb/s bit-emitting hardware emulation; the two separate transceiver devices own the high-speed data path under the fictional training protocol.

Use actual U7 for Lane A, U8 for Lane B, U9 link quality analyzer, U11 independent safety supervisor per source BOM and `GameplayPlan.json`. U1 controls tests and pattern selection. Keep U2 timing/pattern function, U10 PLL buffer, memory U3/U4 and voltage regulators U5/U6 as source-named parts until schematic defines their actual pins. Distinguish source chip *role* from functional authority, avoid inserting unsupported IC behaviors.

## 09 — High-speed transmission-line physics

Implement a bounded educational distributed-channel representation for two nominal 100 Ω differential links at a 1.0 Gb/s test rate. Choose a consistent simulation time base or event-driven sampled response; do **not** run GHz per-bit calculations in Unity Update for every bit of a long acceptance test. The model needs at least:

- Copper trace/electrical path length and effective propagation delay based on reviewed dielectric geometry.
- Characteristic impedance and frequency-dependent distributed losses (simplified RLGC or piecewise impulse response).
- Discontinuity reflections including impedance mismatches, via stubs, pad/coupling parasitics.
- Independent Lane A and B launch and receive states.
- Differential pair skew, asymmetry and reference-plane transitions.
- Adjacent aggressor/victim crosstalk parameterized by physical routing.
- Thermal/noise/jitter contribution controlled by stable deterministic seed.
- Legitimate transmitter equalization/receiver training if the training protocol supports it, never as a generic “fix all” button.

Use \(\Gamma=(Z_L-Z_0)/(Z_L+Z_0)\) at a simple real boundary, but distinguish single-ended vs differential impedance and reject misuse of the equation as a full-channel model. A nominal 100 Ω differential termination is **not** necessarily 100 Ω on each half to ground. Avoid double-counting discontinuities and contact resistances. Provide a documented fidelity switch (fast production vs detailed bench mode) without changing mission pass rules.

## 10 — TDR and time/frequency-domain measurements

Generate a TDR waveform from a simulated incident edge and the actual current approved path topology. The measured return time must correspond to the discontinuity distance and propagation velocity (round trip), not a hard-coded fault cursor. The UI should show trace ID, source/termination conditions, model assumptions, calculated impedance estimate, valid measurement range and uncertainty. A short, open, wrong-value termination and via stub should produce distinguishable responses within the chosen model's resolution.

Model probe bandwidth, pulse width and noise. A 1 Gb/s signal-integrity inspection needs an appropriate simulated instrument front-end; the gameplay does not require an actual physical TDR model. Reject results if the probe is not logically attached to a valid test port, if there is no reference/return connection, or if the instrument is measuring an obsolete circuit version.

## 11 — Eye diagram, jitter and BER

The eye analyzer reads actual sampled receiver outputs from the lane model over a finite set of generated data patterns. Show UI height/width in units and volts, measured random/deterministic jitter, inter-symbol interference, skew and estimated BER confidence where supported by the model. PRBS-style patterns should use fixed seeded generators, specified polynomial/seed and correct bit alignment. Allow Lane A and Lane B to fail independently.

Do not assert a physically proven BER of zero merely because no bit errors occurred during a finite test. Report `0 observed / N tested`, plus a confidence upper bound if desired. For example, with 0 observed errors, a rough 95% Poisson upper bound is about `3/N`; **label this as an approximation**. Define test length, pattern, conditions and valid signal measurement timestamps. A screenshot of a pretty eye cannot certify the objective.

Design targets (proposed, not source-certified): nominal differential impedance 100 Ω ±10%, lane pair skew ≤15 ps, rate 1.0 Gb/s, eye height ≥20% of nominal differential swing, eye width ≥45% unit interval, zero observed bits in a declared acceptance interval, valid safety feedback. Confirm/adjust physically coherent thresholds against the eventual approved stackup.

## 12 — Crosstalk model and differential lane integrity

Associate actual copper path segments and neighboring aggressor traces, then estimate capacitive/inductive coupling according to parallel length, spacing, dielectric and edge rate. A virtual routing repair must change the real lane geometry model, return reference continuity and regenerated eye/BER samples. Include a separate aggressor-off control during diagnosis to prove correlation. Provide wrong-correction negative tests: rerouting unrelated decorative segments or reducing UI noise slider must not repair the fault.

Differential pair handling must preserve which net is P and N, check polarity, skew and receiver common-mode limits, and distinguish basic DC continuity from valid 1 Gb/s channel behavior.

## 13 — Three source-defined recoverable faults

The source `GameplayPlan.json` names exactly:

**F1 — R1 wrong differential termination:** source fault describes **150 Ω instead of 100 Ω**. The player must measure actual termination effect using TDR/eye data, isolate the safe repair procedure, choose the appropriate virtual replacement component, authorize a real replace action at R1, update the component/electrical model and run fresh tests. Reject a repair if the wrong resistance or footprint is chosen.

**F2 — `Diff_Pair_A_Copper` excessive coupling:** player uses crosstalk analyzer to locate a designated physically relevant adjacent routing segment, uses supported trace rework action, changes actual geometrical spacing/route parameters within DRC constraints and demonstrates lower crosstalk. The source does not specify exact coordinates of the fault, so the approved route geometry must define them first.

**F3 — `Via_Barrels` unterminated via stub:** use TDR and layer inspection to identify the conductive excess stub; repair via approved section rework/backdrilling/board-section replacement (model supported choice) without allowing a surface soldering click to magically remove a buried stub. Verify impedance improvement and full link qualification. The source identifies only a generic visual target, so exact via/task/net must be reconciled.

For each fault: independent fault ID, root cause, affected nets, physical parameter before/after, visible effect, allowed tools, prerequisites, repair action, VFX binding, measurement invalidation, accepted evidence and regression tests. Restore fault without resetting permanently certified historical objectives; fresh post-fault testing remains mandatory.

## 14 — W109 critical feedback and safety recovery

The source explicitly states:

```text
L102_LinkQualityMonitor_U9.TRUSTED_LINK_QUALITY_OUT
  -> W109 (detachable validity signal; STARTS DISCONNECTED)
  -> L102_HighSpeedSafetySupervisor_U11.TRUSTED_LINK_QUALITY_IN
```

W109 is **not** the actual Lane A or Lane B differential pair and must not short the analog rails. During initial commissioning the player installs W109; only after objectives 1–5 are properly certified, Objective 6 asks the player to deliberately remove it under supervised conditions.

Disconnection must invalidate trusted link-quality feedback in the actual graph. A constant HIGH, replayed frame, out-of-date sample or floating input cannot qualify as fresh health. The independent supervisor withdraws protected transmission authorization and latches the expected safety fault while leaving permitted diagnostics intact. Model bounded shutdown transitions rather than instantly erasing energy or the MCU firmware. The W109 test cannot be passed by manually toggling a UI status indicator.

Recovery protocol: real physical reconnection → verify continuity and input type → fresh heartbeat/quality messages → calibrate relevant sensors → explicit fault reset → reinitialize both lanes → collect a wholly new PRBS/eye/TDR/BER acceptance run → award Objective 6 only if all criteria are true. Objectives 1–5 may retain historical certification but must not substitute for that new run.

## 15 — Six authoritative objectives and fresh-evidence semantics

| Objective | Condition | Persistent historical certification | Fresh evidence |
|---|---|---|---|
| O1 — Assembly | All 82 approved component placements with valid values/solder states | yes | recheck affected replacements |
| O2 — Copper routing | 108 valid approved routes, 28 valid vias, installed W109 for commissioning, actual DRC/ERC success | yes | topology version tied |
| O3 — Link commissioning | protected supplies, clock/reset, Lane A+B training, legitimate measured impedance and link checks | yes | commissioning run ID |
| O4 — Integrity | TDR/eye/skew/crosstalk and finite PRBS tests satisfy both lane margins | yes | measurement run ID |
| O5 — Fault recovery | all F1/F2/F3 physical/parametric faults truly repaired and full fresh post-repair validation | yes | fault-history + run ID |
| O6 — W109 recovery | real disconnect/inhibition/reconnect/reset, full independent fresh both-lane acceptance | completion | post-fault new run ID |

Certification must be done by the established `LevelGamePlayManager` mechanism rather than a locally fabricated completed-count property. Define versioned evidence records: source netlist hash, circuit topology revision, component installed revision, fault state revision, safety latch revision, time/simulation tick, run ID, result/uncertainty and issuing subsystem. Any route, via, value, fault or safety change invalidates measurements dependent on it. Enforce monotonic historical awards but current-safe failure states; no reusing stale success records.

## 16 — AAA World-Space Canvas instrument integration

Reuse the existing virtual PCB UI (not physical FBX tools). Support: inspect, assemble, route, layer view, test, repair and calibrate; multimeter, oscilloscope, logic analyzer, TDR analyzer, eye analyzer, impedance analyzer, crosstalk analyzer, differential pair tool, net tracer, via repair, solder, desolder, component replace. Virtual panels should show honest live or paused data, measurement source IDs and connection requirements.

On PC click or mobile touch: use Unity UI raycast priority for world-space instrument buttons and physics/raycast picking for actual PCB targets. When a virtual tool is armed, require a compatible target (pad/component/trace/via/test point) and use the exact collision hit coordinate for VFX. Keep input debounced; avoid simultaneous PCB repair and camera orbit. Tool target eligibility comes from functional defect state, not renderer name.

UI design: polished cyan/graphite/amber palette, readable text in perspective, dynamic waveform samples from the measurement service, signal path highlighting tied to real net IDs, safe tooltips for difficult tasks, optional first-connection tutorial. Explicitly label virtual instruments as simulations and maintain consistent visuals with earlier Project Spark levels.

## 17 — Click/touch localized VFX and repair authority

At contact point create lightweight GPU/particle-compatible solder glow, small sparks, transient heat shimmer, localized scan pulse, net highlight and short UI confirmation. Effects vary by tool and approved state; e.g. thermal scan emits no molten solder. Use object pooling, camera-distance LOD and mobile quality presets. If the electrical authority rejects the tool action, show a warning and no repaired-success effect; never set fault flags from a VFX event. The actual lifecycle is `SelectTool -> ResolveTarget -> ValidateSafety -> AuthorizeRepair -> UpdateTopology/Component -> Recompute -> VFX -> FreshMeasurement -> ObjectiveAssessment`.

Account for no physical tools: all models in the scene are electronic board assets. A touch repair may simulate a safe controlled manufacturing operation via animation and local effects, but cannot justify physically impossible fixes such as “soldering away” a buried via stub. For fault F3 require supported board-section replacement or validated precision rework.

## 18 — Material and importer quality contract

Use source FBX and GLB only as *art*. The imported FBX coordinates are in metres; physical board 0.250 m × 0.175 m. Preserve orientation/scale and validate in Unity. Generate material assets from 18 families with 90 provided reference maps, pack roughness into smoothness only as required by active URP/HDRP/Built-in pipeline, honor sRGB flags for albedo vs linear normal/masks, configure normal texture import and alpha modes correctly. Remap imported material slots by actual source material name; avoid creating duplicate `.mat` assets on re-run. The supplied `SparkL102MaterialBuilder.cs` currently assigns albedo maps only; improving it is a separate actual implementation step.

Pick colliders appropriate for interaction, not full high-poly collider on tiny solder geometry. Use footprint/pad trigger proxies or generated hit zones anchored to the validated pad manifest. Keep scene render quality configurable without touching physics/electrical math. Diagnose missing FBXs, missing textures and mismatched material-slot assignments with an Editor validation report.

## 19 — Save, load, reset and determinism

Save component installation, route/via geometry and net identifiers, connected W109 state, fault status/history, current circuit revisions, calibration freshness, verified run results and objective certification. On load reconstruct the authoritative graph first, then bind visuals and UI, then replay any permitted continuing simulation state. Never deserialize `certified=true` solely from an art file. Provide a clean reset command that restores the empty playable board and clears Level 102 only. Protect other levels and avoid statics accidentally leaking circuit state.

Use a deterministic test seed for acceptance and a separate optional stochastic presentation mode. Offline tests should be able to inject a fault, execute test stimuli and compare normalized measurement metrics reproducibly. Keep current/voltage and heat approximations stable under variable framerate.

## 20 — Performance, accessibility and stability

Target smooth play on mobile and desktop: low-frequency board simulation outside high-speed acquisition, cached impulse-response lookups, pooled VFX, no generating tens of millions of per-bit GameObjects or huge polyline meshes, no per-frame GC allocations in hot paths. Adjustable UI contrast, color-independent fault indicators, scalable text and touch hit sizes; tooltips for impedance/skew concepts. Use structured logs under an optional debug flag.

Protect against null refs, duplicate singleton/scene roots, missing target colliders, unloaded material assets, stale Canvas cameras, scene re-entry and simultaneous two-finger actions. Ensure editor menu paths are idempotent and do not change levels 1–101.

## 21 — Proposed runtime responsibilities (not confirmed existing classes)

| Responsibility | Proposed implementation name |
|---|---|
| Level coordination | `SparkLevel102Coordinator` |
| Approved schematic and pad mapping | `SparkL102SchematicRegistry` |
| Six-layer source graph adapter | `SparkL102PCBConnectivityAdapter` |
| Component and factory copper | `SparkL102ComponentAuthority` |
| Dynamic routed copper | `SparkL102RouteAuthority` |
| Via span rules | `SparkL102ViaAuthority` |
| Power and initialization | `SparkL102PowerClockPlant` |
| Differential signal path | `SparkL102DifferentialChannel` |
| TDR acquisition | `SparkL102TDRAnalyzer` |
| Eye/PRBS measurements | `SparkL102EyeAndBERAnalyzer` |
| Crosstalk coupling | `SparkL102CrosstalkAnalyzer` |
| F1/F2/F3 controller | `SparkL102FaultAuthority` |
| W109 independent supervisor | `SparkL102W109SafetyAuthority` |
| Existing virtual UI adapter | `SparkL102VirtualToolBridge` |
| Six objective evidence gateway | `SparkL102ObjectiveEvaluator` |
| One-click builder and validation | `SparkL102SceneBuilder` |

Prefer real Project Spark existing types; proposed names should not cause duplicates of systems already present. All authority adapters should have explicit lifecycle and dependency injection points.

## 22 — Player experience and objective sequencing

1. Load empty 250 × 175 mm HSX-10 board on existing table.
2. Inspect schematic and select correct component inventory.
3. Place and solder 82 individual components.
4. Draw 108 approved copper route tasks across six layers; install 28 approved vias; install W109 for normal operation.
5. Power up 5 V, 3.3 V and 1.8 V domains; initialize clock and test firmware.
6. Train both 1.0 Gb/s fictional differential lanes.
7. Acquire valid TDR, eye and finite-pattern BER measurement for each lane.
8. Investigate F1 wrong-value R1; replace R1 and retest.
9. Investigate F2 crosstalk route; correct approved geometry and retest.
10. Investigate F3 via stub; perform approved board-section rework/replacement and retest.
11. Certify O1–O5 with authentic evidence.
12. Perform supervised W109 removal: detect trusted feedback loss and verify safety inhibition.
13. Reconnect, calibrate, explicitly reset, rerun fully independent two-lane tests.
14. Award O6 via actual `LevelGamePlayManager`, display `6/6` only if all required evidence exists.

No scripted win triggered by clicking a single dashboard button, toggling art colors, showing completed route meshes or editing a local boolean.

## 23 — Proposed engineering acceptance suite (90 tests)

These are **new target tests**, NOT tests already supplied or passed by the source archive. Implement them as Edit Mode, Play Mode and deterministic simulation tests. Run all inside the actual Unity project and provide a machine-readable results report.

### Source and import

- [ ] **L102-T001** — 104 modular FBXs uniquely identified.
- [ ] **L102-T002** — 104 GLB pairs mapped.
- [ ] **L102-T003** — reference FBX kept outside playable root.
- [ ] **L102-T004** — real board dimensions match 250 x 175 mm.
- [ ] **L102-T005** — all 82 component identities unique.
- [ ] **L102-T006** — all 22 secondary models recognized.
- [ ] **L102-T007** — missing FBX reported clearly.
- [ ] **L102-T008** — wrong scale reported clearly.
- [ ] **L102-T009** — material generator idempotent.
- [ ] **L102-T010** — empty art builder idempotent.

### Approved schematic

- [ ] **L102-T011** — all 108 rows initially uncertifiable.
- [ ] **L102-T012** — all 28 via rows initially uncertifiable.
- [ ] **L102-T013** — no route accepted without endpoints.
- [ ] **L102-T014** — no via accepted without layer span.
- [ ] **L102-T015** — pin ID collisions detected.
- [ ] **L102-T016** — missing power net rejected.
- [ ] **L102-T017** — factory copper bypass detected.
- [ ] **L102-T018** — cross-net short rejected.
- [ ] **L102-T019** — floating test pad rejected.
- [ ] **L102-T020** — W109 input and output approved before certification.

### Assembly and topology

- [ ] **L102-T021** — start 0/82 components.
- [ ] **L102-T022** — wrong component value rejected.
- [ ] **L102-T023** — wrong pin-one rotation rejected.
- [ ] **L102-T024** — damaged solder joint blocks continuity.
- [ ] **L102-T025** — valid install adds correct pad edges.
- [ ] **L102-T026** — removal invalidates dependent measurements.
- [ ] **L102-T027** — start 0/108 routes.
- [ ] **L102-T028** — wrong layer route rejected.
- [ ] **L102-T029** — cross-net route blocked.
- [ ] **L102-T030** — all 28 task vias required.

### Power and clock

- [ ] **L102-T031** — no 3.3 V without supply topology.
- [ ] **L102-T032** — 1.8 V transceiver power required.
- [ ] **L102-T033** — ground return required.
- [ ] **L102-T034** — clock initialization required.
- [ ] **L102-T035** — invalid reset prevents training.
- [ ] **L102-T036** — supply tolerance propagates into noise.
- [ ] **L102-T037** — power cycling clears volatile measurements.
- [ ] **L102-T038** — power dropout interrupts live run.
- [ ] **L102-T039** — status UI mirrors authority.
- [ ] **L102-T040** — diagnostic mode remains safe.

### Transmission and TDR

- [ ] **L102-T041** — 100 ohm nominal line matched.
- [ ] **L102-T042** — 150 ohm R1 mismatch reflected.
- [ ] **L102-T043** — reflection polarity correct for high load.
- [ ] **L102-T044** — short/open signatures differ.
- [ ] **L102-T045** — round-trip TDR delay scales with length.
- [ ] **L102-T046** — probe must be connected.
- [ ] **L102-T047** — instrument bandwidth limits resolution.
- [ ] **L102-T048** — lane A and lane B independent.
- [ ] **L102-T049** — via stub reflection separate from open via.
- [ ] **L102-T050** — stale TDR cannot qualify.

### Eye and PRBS

- [ ] **L102-T051** — PRBS sequence deterministic.
- [ ] **L102-T052** — training requires live lane.
- [ ] **L102-T053** — receive bits depend on channel response.
- [ ] **L102-T054** — finite error count recorded.
- [ ] **L102-T055** — zero observed errors not infinite BER proof.
- [ ] **L102-T056** — lane-specific eye width evaluated.
- [ ] **L102-T057** — lane-specific eye height evaluated.
- [ ] **L102-T058** — jitter tolerance evaluated.
- [ ] **L102-T059** — pair skew affects eye.
- [ ] **L102-T060** — stale PRBS results rejected.

### Crosstalk and repairs

- [ ] **L102-T061** — F1 fault changes actual termination.
- [ ] **L102-T062** — F1 wrong replacement rejected.
- [ ] **L102-T063** — F1 proper repair invalidates old run.
- [ ] **L102-T064** — F2 aggressor influences victim.
- [ ] **L102-T065** — F2 unrelated routing change does not fix.
- [ ] **L102-T066** — F2 approved spacing rework improves coupling.
- [ ] **L102-T067** — F3 via stub causes frequency-dependent effect.
- [ ] **L102-T068** — F3 surface solder click cannot cure.
- [ ] **L102-T069** — F3 approved section replacement cures.
- [ ] **L102-T070** — all three faults require fresh qualification.

### W109 safety

- [ ] **L102-T071** — W109 initially detached.
- [ ] **L102-T072** — initial installation enables health only after tests.
- [ ] **L102-T073** — constant high rejected as stale.
- [ ] **L102-T074** — actual removal changes topology.
- [ ] **L102-T075** — loss triggers inhibition.
- [ ] **L102-T076** — diagnostics remain available.
- [ ] **L102-T077** — reconnect alone insufficient.
- [ ] **L102-T078** — explicit reset required.
- [ ] **L102-T079** — fresh link evidence after reconnect.
- [ ] **L102-T080** — new post-fault run required.

### UI and regression

- [ ] **L102-T081** — mouse click tool targets correct pad.
- [ ] **L102-T082** — touch tool targets correct component.
- [ ] **L102-T083** — wrong tool shows rejection without repair.
- [ ] **L102-T084** — VFX follows actual hit point.
- [ ] **L102-T085** — UI never awards objective alone.
- [ ] **L102-T086** — six objectives only via gameplay manager.
- [ ] **L102-T087** — save/load maintains real graph state.
- [ ] **L102-T088** — reset restores level 102 only.
- [ ] **L102-T089** — earlier levels remain unchanged.
- [ ] **L102-T090** — no Unity compilation errors.

## 24 — Acceptance and handoff deliverables

Deliver: approved netlist and stack-up, completed Unity C# implementations, no missing compile references, imported and bound FBXs with production-ready native materials, working world-space inspection/repair dashboard, working click/touch targets and VFX, safety supervisor, six-objective evaluator, one-click scene builder, 90-test report, save/load verification, per-level performance report and list of known limitations. Produce both a gameplay screenshot and a debugger readout from **actual Unity Play Mode** if Unity is available. If Unity is not executable, state that unequivocally; do not claim successful compilation or 6/6 from offline CSV checks.

A production-level completion claim needs a run with real player interactions, correct electrical/communication outcomes and replayable fresh W109 fault recovery. A source-only art kit is insufficient.

## ANNEX A — Original model manifest (104 modular meshes)

**Archive path:** `Level102_HSX10/Data/ModelManifest.csv`. Source-preserved as provided; these tables are not auto-corrected.

```csv
index,model,category,fbx,glb,x_m,y_m,z_m,triangles
1,U1,installable_component,001_L102_U1.fbx,001_L102_U1.glb,0,0,0.0008,3560
2,U2,installable_component,002_L102_U2.fbx,002_L102_U2.glb,-0.096,0.046,0.0008,488
3,U3,installable_component,003_L102_U3.fbx,003_L102_U3.glb,-0.064,0.046,0.0008,488
4,U4,installable_component,004_L102_U4.fbx,004_L102_U4.glb,-0.032,0.046,0.0008,488
5,U5,installable_component,005_L102_U5.fbx,005_L102_U5.glb,0.0,0.046,0.0008,488
6,U6,installable_component,006_L102_U6.fbx,006_L102_U6.glb,0.032,0.046,0.0008,488
7,U7,installable_component,007_L102_U7.fbx,007_L102_U7.glb,0.064,0.046,0.0008,872
8,U8,installable_component,008_L102_U8.fbx,008_L102_U8.glb,0.096,0.046,0.0008,872
9,U9,installable_component,009_L102_U9.fbx,009_L102_U9.glb,-0.096,-0.040999999999999995,0.0008,872
10,U10,installable_component,010_L102_U10.fbx,010_L102_U10.glb,-0.064,-0.040999999999999995,0.0008,488
11,U11,installable_component,011_L102_U11.fbx,011_L102_U11.glb,-0.032,-0.040999999999999995,0.0008,872
12,U12,installable_component,012_L102_U12.fbx,012_L102_U12.glb,0.0,-0.040999999999999995,0.0008,488
13,U13,installable_component,013_L102_U13.fbx,013_L102_U13.glb,0.032,-0.040999999999999995,0.0008,488
14,U14,installable_component,014_L102_U14.fbx,014_L102_U14.glb,0.064,-0.040999999999999995,0.0008,488
15,R1,installable_component,015_L102_R1.fbx,015_L102_R1.glb,-0.107,-0.064,0.0008,60
16,R2,installable_component,016_L102_R2.fbx,016_L102_R2.glb,-0.0907,-0.064,0.0008,60
17,R3,installable_component,017_L102_R3.fbx,017_L102_R3.glb,-0.0744,-0.064,0.0008,60
18,R4,installable_component,018_L102_R4.fbx,018_L102_R4.glb,-0.0581,-0.064,0.0008,60
19,R5,installable_component,019_L102_R5.fbx,019_L102_R5.glb,-0.0418,-0.064,0.0008,60
20,R6,installable_component,020_L102_R6.fbx,020_L102_R6.glb,-0.0255,-0.064,0.0008,60
21,R7,installable_component,021_L102_R7.fbx,021_L102_R7.glb,-0.0092,-0.064,0.0008,60
22,R8,installable_component,022_L102_R8.fbx,022_L102_R8.glb,0.0071,-0.064,0.0008,60
23,R9,installable_component,023_L102_R9.fbx,023_L102_R9.glb,0.0234,-0.064,0.0008,60
24,R10,installable_component,024_L102_R10.fbx,024_L102_R10.glb,0.0397,-0.064,0.0008,60
25,R11,installable_component,025_L102_R11.fbx,025_L102_R11.glb,0.056,-0.064,0.0008,60
26,R12,installable_component,026_L102_R12.fbx,026_L102_R12.glb,0.0723,-0.064,0.0008,60
27,R13,installable_component,027_L102_R13.fbx,027_L102_R13.glb,0.0886,-0.064,0.0008,60
28,R14,installable_component,028_L102_R14.fbx,028_L102_R14.glb,0.1049,-0.064,0.0008,60
29,R15,installable_component,029_L102_R15.fbx,029_L102_R15.glb,-0.107,-0.0477,0.0008,60
30,R16,installable_component,030_L102_R16.fbx,030_L102_R16.glb,-0.0907,-0.0477,0.0008,60
31,R17,installable_component,031_L102_R17.fbx,031_L102_R17.glb,-0.0744,-0.0477,0.0008,60
32,R18,installable_component,032_L102_R18.fbx,032_L102_R18.glb,-0.0581,-0.0477,0.0008,60
33,R19,installable_component,033_L102_R19.fbx,033_L102_R19.glb,-0.0418,-0.0477,0.0008,60
34,R20,installable_component,034_L102_R20.fbx,034_L102_R20.glb,-0.0255,-0.0477,0.0008,60
35,R21,installable_component,035_L102_R21.fbx,035_L102_R21.glb,-0.0092,-0.0477,0.0008,60
36,R22,installable_component,036_L102_R22.fbx,036_L102_R22.glb,0.0071,-0.0477,0.0008,60
37,R23,installable_component,037_L102_R23.fbx,037_L102_R23.glb,0.0234,-0.0477,0.0008,60
38,R24,installable_component,038_L102_R24.fbx,038_L102_R24.glb,0.0397,-0.0477,0.0008,60
39,C1,installable_component,039_L102_C1.fbx,039_L102_C1.glb,0.056,-0.0477,0.0008,60
40,C2,installable_component,040_L102_C2.fbx,040_L102_C2.glb,0.0723,-0.0477,0.0008,60
41,C3,installable_component,041_L102_C3.fbx,041_L102_C3.glb,0.0886,-0.0477,0.0008,60
42,C4,installable_component,042_L102_C4.fbx,042_L102_C4.glb,0.1049,-0.0477,0.0008,60
43,C5,installable_component,043_L102_C5.fbx,043_L102_C5.glb,-0.107,-0.0314,0.0008,60
44,C6,installable_component,044_L102_C6.fbx,044_L102_C6.glb,-0.0907,-0.0314,0.0008,60
45,C7,installable_component,045_L102_C7.fbx,045_L102_C7.glb,-0.0744,-0.0314,0.0008,60
46,C8,installable_component,046_L102_C8.fbx,046_L102_C8.glb,-0.0581,-0.0314,0.0008,60
47,C9,installable_component,047_L102_C9.fbx,047_L102_C9.glb,-0.0418,-0.0314,0.0008,60
48,C10,installable_component,048_L102_C10.fbx,048_L102_C10.glb,-0.0255,-0.0314,0.0008,60
49,C11,installable_component,049_L102_C11.fbx,049_L102_C11.glb,-0.0092,-0.0314,0.0008,60
50,C12,installable_component,050_L102_C12.fbx,050_L102_C12.glb,0.0071,-0.0314,0.0008,60
51,C13,installable_component,051_L102_C13.fbx,051_L102_C13.glb,0.0234,-0.0314,0.0008,60
52,C14,installable_component,052_L102_C14.fbx,052_L102_C14.glb,0.0397,-0.0314,0.0008,60
53,C15,installable_component,053_L102_C15.fbx,053_L102_C15.glb,0.056,-0.0314,0.0008,60
54,C16,installable_component,054_L102_C16.fbx,054_L102_C16.glb,0.0723,-0.0314,0.0008,60
55,C17,installable_component,055_L102_C17.fbx,055_L102_C17.glb,0.0886,-0.0314,0.0008,60
56,C18,installable_component,056_L102_C18.fbx,056_L102_C18.glb,0.1049,-0.0314,0.0008,60
57,C19,installable_component,057_L102_C19.fbx,057_L102_C19.glb,-0.107,-0.0151,0.0008,60
58,C20,installable_component,058_L102_C20.fbx,058_L102_C20.glb,-0.0907,-0.0151,0.0008,60
59,C21,installable_component,059_L102_C21.fbx,059_L102_C21.glb,-0.0744,-0.0151,0.0008,60
60,C22,installable_component,060_L102_C22.fbx,060_L102_C22.glb,-0.0581,-0.0151,0.0008,60
61,Y1,installable_component,061_L102_Y1.fbx,061_L102_Y1.glb,-0.059,0.021,0.0008,72
62,Y2,installable_component,062_L102_Y2.fbx,062_L102_Y2.glb,-0.067,-0.004,0.0008,72
63,J1,installable_component,063_L102_J1.fbx,063_L102_J1.glb,-0.111,0.05,0.0008,24
64,J2,installable_component,064_L102_J2.fbx,064_L102_J2.glb,0.111,0.038,0.0008,372
65,J3,installable_component,065_L102_J3.fbx,065_L102_J3.glb,0.111,-0.038,0.0008,372
66,J4,installable_component,066_L102_J4.fbx,066_L102_J4.glb,-0.105,-0.043,0.0008,132
67,J5,installable_component,067_L102_J5.fbx,067_L102_J5.glb,0.105,0.06,0.0008,132
68,LED1,installable_component,068_L102_LED1.fbx,068_L102_LED1.glb,0.0723,-0.0151,0.0008,72
69,LED2,installable_component,069_L102_LED2.fbx,069_L102_LED2.glb,0.0886,-0.0151,0.0008,72
70,LED3,installable_component,070_L102_LED3.fbx,070_L102_LED3.glb,0.1049,-0.0151,0.0008,72
71,LED4,installable_component,071_L102_LED4.fbx,071_L102_LED4.glb,-0.107,0.0012,0.0008,72
72,D1,installable_component,072_L102_D1.fbx,072_L102_D1.glb,-0.0907,0.0012,0.0008,60
73,D2,installable_component,073_L102_D2.fbx,073_L102_D2.glb,-0.0744,0.0012,0.0008,60
74,D3,installable_component,074_L102_D3.fbx,074_L102_D3.glb,-0.0581,0.0012,0.0008,60
75,FB1,installable_component,075_L102_FB1.fbx,075_L102_FB1.glb,-0.0418,0.0012,0.0008,60
76,FB2,installable_component,076_L102_FB2.fbx,076_L102_FB2.glb,-0.0255,0.0012,0.0008,60
77,FB3,installable_component,077_L102_FB3.fbx,077_L102_FB3.glb,-0.0092,0.0312,0.0008,60
78,FB4,installable_component,078_L102_FB4.fbx,078_L102_FB4.glb,0.0071,0.0312,0.0008,60
79,SW1,installable_component,079_L102_SW1.fbx,079_L102_SW1.glb,-0.105,-0.063,0.0008,132
80,SW2,installable_component,080_L102_SW2.fbx,080_L102_SW2.glb,0.0397,0.0012,0.0008,132
81,SW3,installable_component,081_L102_SW3.fbx,081_L102_SW3.glb,0.056,0.0012,0.0008,132
82,W109,installable_component,082_L102_W109.fbx,082_L102_W109.glb,0.089,-0.053,0.0008,168
83,PCB_Base,board_detail,083_L102_PCB_Base.fbx,083_L102_PCB_Base.glb,0,0,0,1308
84,L1_Layer_Top,board_detail,084_L102_L1_Layer_Top.fbx,084_L102_L1_Layer_Top.glb,0,0,0,720
85,L2_Layer_GND,board_detail,085_L102_L2_Layer_GND.fbx,085_L102_L2_Layer_GND.glb,0,0,0,12
86,L3_Layer_Signal,board_detail,086_L102_L3_Layer_Signal.fbx,086_L102_L3_Layer_Signal.glb,0,0,0,720
87,L4_Layer_Power,board_detail,087_L102_L4_Layer_Power.fbx,087_L102_L4_Layer_Power.glb,0,0,0,720
88,L5_Layer_GND,board_detail,088_L102_L5_Layer_GND.fbx,088_L102_L5_Layer_GND.glb,0,0,0,12
89,L6_Layer_Bottom,board_detail,089_L102_L6_Layer_Bottom.fbx,089_L102_L6_Layer_Bottom.glb,0,0,0,720
90,Diff_Pair_A_Copper,board_detail,090_L102_Diff_Pair_A_Copper.fbx,090_L102_Diff_Pair_A_Copper.glb,0,0,0,612
91,Diff_Pair_B_Copper,board_detail,091_L102_Diff_Pair_B_Copper.fbx,091_L102_Diff_Pair_B_Copper.glb,0,0,0,612
92,Test_Pads,board_detail,092_L102_Test_Pads.fbx,092_L102_Test_Pads.glb,0,0,0,720
93,Via_Barrels,board_detail,093_L102_Via_Barrels.fbx,093_L102_Via_Barrels.glb,0,0,0,1728
94,Ground_Stitching,board_detail,094_L102_Ground_Stitching.fbx,094_L102_Ground_Stitching.glb,0,0,0,1728
95,Termination_Fault,board_detail,095_L102_Termination_Fault.fbx,095_L102_Termination_Fault.glb,0,0,0,84
96,Crosstalk_Fault,board_detail,096_L102_Crosstalk_Fault.fbx,096_L102_Crosstalk_Fault.glb,0,0,0,84
97,Via_Stub_Fault,board_detail,097_L102_Via_Stub_Fault.fbx,097_L102_Via_Stub_Fault.glb,0,0,0,1728
98,Trace_Repair,board_detail,098_L102_Trace_Repair.fbx,098_L102_Trace_Repair.glb,0,0,0,612
99,Via_Repair,board_detail,099_L102_Via_Repair.fbx,099_L102_Via_Repair.glb,0,0,0,1728
100,HSX10_Silkscreen,board_detail,100_L102_HSX10_Silkscreen.fbx,100_L102_HSX10_Silkscreen.glb,0,0,0,60
101,Board_ID,board_detail,101_L102_Board_ID.fbx,101_L102_Board_ID.glb,0,0,0,60
102,Signal_Reference,board_detail,102_L102_Signal_Reference.fbx,102_L102_Signal_Reference.glb,0,0,0,60
103,Termination_Reference,board_detail,103_L102_Termination_Reference.fbx,103_L102_Termination_Reference.glb,0,0,0,60
104,Diagnostic_Reference,board_detail,104_L102_Diagnostic_Reference.fbx,104_L102_Diagnostic_Reference.glb,0,0,0,60
```

## ANNEX B — Original 82-part BOM

**Archive path:** `Level102_HSX10/Data/ComponentBOM.csv`. Source-preserved as provided; these tables are not auto-corrected.

```csv
ref,type,description
U1,IC,Cortex-M MCU controller
U2,IC,Pattern and clock generator
U3,IC,SPI flash
U4,IC,RAM controller
U5,IC,3.3 V LDO
U6,IC,1.8 V LDO
U7,IC,HS Transceiver Lane A
U8,IC,HS Transceiver Lane B
U9,IC,Link quality analyzer
U10,IC,PLL clock buffer
U11,IC,Independent safety supervisor
U12,IC,Receiver test IC
U13,IC,Signal equalizer
U14,IC,Input protection controller
R1,R,SMD resistor differential termination
R2,R,SMD resistor differential termination
R3,R,SMD resistor differential termination
R4,R,SMD resistor differential termination
R5,R,SMD resistor
R6,R,SMD resistor
R7,R,SMD resistor
R8,R,SMD resistor
R9,R,SMD resistor
R10,R,SMD resistor
R11,R,SMD resistor
R12,R,SMD resistor
R13,R,SMD resistor
R14,R,SMD resistor
R15,R,SMD resistor
R16,R,SMD resistor
R17,R,SMD resistor
R18,R,SMD resistor
R19,R,SMD resistor
R20,R,SMD resistor
R21,R,SMD resistor
R22,R,SMD resistor
R23,R,SMD resistor
R24,R,SMD resistor
C1,C,MLCC decoupling capacitor
C2,C,MLCC decoupling capacitor
C3,C,MLCC decoupling capacitor
C4,C,MLCC decoupling capacitor
C5,C,MLCC decoupling capacitor
C6,C,MLCC decoupling capacitor
C7,C,MLCC decoupling capacitor
C8,C,MLCC decoupling capacitor
C9,C,MLCC decoupling capacitor
C10,C,MLCC decoupling capacitor
C11,C,MLCC decoupling capacitor
C12,C,MLCC decoupling capacitor
C13,C,MLCC decoupling capacitor
C14,C,MLCC decoupling capacitor
C15,C,MLCC decoupling capacitor
C16,C,MLCC decoupling capacitor
C17,C,MLCC decoupling capacitor
C18,C,MLCC decoupling capacitor
C19,C,MLCC decoupling capacitor
C20,C,MLCC decoupling capacitor
C21,C,MLCC decoupling capacitor
C22,C,MLCC decoupling capacitor
Y1,CRYSTAL,Precision oscillator
Y2,CRYSTAL,Precision oscillator
J1,CONNECTOR,USB-C connector
J2,CONNECTOR,SMA precision port
J3,CONNECTOR,SMA precision port
J4,CONNECTOR,Debug header
J5,CONNECTOR,Debug header
LED1,LED,Board diagnostic indicator
LED2,LED,Board diagnostic indicator
LED3,LED,Board diagnostic indicator
LED4,LED,Board diagnostic indicator
D1,D,ESD/TVS protection
D2,D,ESD/TVS protection
D3,D,ESD/TVS protection
FB1,FB,Ferrite bead
FB2,FB,Ferrite bead
FB3,FB,Ferrite bead
FB4,FB,Ferrite bead
SW1,SW,Tactile configuration switch
SW2,SW,Tactile configuration switch
SW3,SW,Tactile configuration switch
W109,JUMPER,Removable trusted link-health jumper
```

## ANNEX C — Source component placements

**Archive path:** `Level102_HSX10/Data/ComponentPlacements.csv`. Source-preserved as provided; these tables are not auto-corrected.

```csv
ref,footprint,x_m,y_m,board_side,initial_installed
U1,IC,0,0,Top,False
U2,IC,-0.096,0.046,Top,False
U3,IC,-0.064,0.046,Top,False
U4,IC,-0.032,0.046,Top,False
U5,IC,0.0,0.046,Top,False
U6,IC,0.032,0.046,Top,False
U7,IC,0.064,0.046,Top,False
U8,IC,0.096,0.046,Top,False
U9,IC,-0.096,-0.040999999999999995,Top,False
U10,IC,-0.064,-0.040999999999999995,Top,False
U11,IC,-0.032,-0.040999999999999995,Top,False
U12,IC,0.0,-0.040999999999999995,Top,False
U13,IC,0.032,-0.040999999999999995,Top,False
U14,IC,0.064,-0.040999999999999995,Top,False
R1,R,-0.107,-0.064,Top,False
R2,R,-0.0907,-0.064,Top,False
R3,R,-0.0744,-0.064,Top,False
R4,R,-0.0581,-0.064,Top,False
R5,R,-0.0418,-0.064,Top,False
R6,R,-0.0255,-0.064,Top,False
R7,R,-0.0092,-0.064,Top,False
R8,R,0.0071,-0.064,Top,False
R9,R,0.0234,-0.064,Top,False
R10,R,0.0397,-0.064,Top,False
R11,R,0.056,-0.064,Top,False
R12,R,0.0723,-0.064,Top,False
R13,R,0.0886,-0.064,Top,False
R14,R,0.1049,-0.064,Top,False
R15,R,-0.107,-0.0477,Top,False
R16,R,-0.0907,-0.0477,Top,False
R17,R,-0.0744,-0.0477,Top,False
R18,R,-0.0581,-0.0477,Top,False
R19,R,-0.0418,-0.0477,Top,False
R20,R,-0.0255,-0.0477,Top,False
R21,R,-0.0092,-0.0477,Top,False
R22,R,0.0071,-0.0477,Top,False
R23,R,0.0234,-0.0477,Top,False
R24,R,0.0397,-0.0477,Top,False
C1,C,0.056,-0.0477,Top,False
C2,C,0.0723,-0.0477,Top,False
C3,C,0.0886,-0.0477,Top,False
C4,C,0.1049,-0.0477,Top,False
C5,C,-0.107,-0.0314,Top,False
C6,C,-0.0907,-0.0314,Top,False
C7,C,-0.0744,-0.0314,Top,False
C8,C,-0.0581,-0.0314,Top,False
C9,C,-0.0418,-0.0314,Top,False
C10,C,-0.0255,-0.0314,Top,False
C11,C,-0.0092,-0.0314,Top,False
C12,C,0.0071,-0.0314,Top,False
C13,C,0.0234,-0.0314,Top,False
C14,C,0.0397,-0.0314,Top,False
C15,C,0.056,-0.0314,Top,False
C16,C,0.0723,-0.0314,Top,False
C17,C,0.0886,-0.0314,Top,False
C18,C,0.1049,-0.0314,Top,False
C19,C,-0.107,-0.0151,Top,False
C20,C,-0.0907,-0.0151,Top,False
C21,C,-0.0744,-0.0151,Top,False
C22,C,-0.0581,-0.0151,Top,False
Y1,CRYSTAL,-0.059,0.021,Top,False
Y2,CRYSTAL,-0.067,-0.004,Top,False
J1,CONNECTOR,-0.111,0.05,Top,False
J2,CONNECTOR,0.111,0.038,Top,False
J3,CONNECTOR,0.111,-0.038,Top,False
J4,CONNECTOR,-0.105,-0.043,Top,False
J5,CONNECTOR,0.105,0.06,Top,False
LED1,LED,0.0723,-0.0151,Top,False
LED2,LED,0.0886,-0.0151,Top,False
LED3,LED,0.1049,-0.0151,Top,False
LED4,LED,-0.107,0.0012,Top,False
D1,D,-0.0907,0.0012,Top,False
D2,D,-0.0744,0.0012,Top,False
D3,D,-0.0581,0.0012,Top,False
FB1,FB,-0.0418,0.0012,Top,False
FB2,FB,-0.0255,0.0012,Top,False
FB3,FB,-0.0092,0.0312,Top,False
FB4,FB,0.0071,0.0312,Top,False
SW1,SW,-0.105,-0.063,Top,False
SW2,SW,0.0397,0.0012,Top,False
SW3,SW,0.056,0.0012,Top,False
W109,JUMPER,0.089,-0.053,Top,False
```

## ANNEX D — All 108 unverified route task records

**Archive path:** `Level102_HSX10/Data/RouteTasks_108_REQUIRES_NETLIST.csv`. Source-preserved as provided; these tables are not auto-corrected.

```csv
route_id,subsystem,source_pad,destination_pad,net_id,layer,starts_complete,certifiable
W001,Power/Ground,UNASSIGNED,UNASSIGNED,UNVERIFIED,UNASSIGNED,false,false
W002,Power/Ground,UNASSIGNED,UNASSIGNED,UNVERIFIED,UNASSIGNED,false,false
W003,Power/Ground,UNASSIGNED,UNASSIGNED,UNVERIFIED,UNASSIGNED,false,false
W004,Power/Ground,UNASSIGNED,UNASSIGNED,UNVERIFIED,UNASSIGNED,false,false
W005,Power/Ground,UNASSIGNED,UNASSIGNED,UNVERIFIED,UNASSIGNED,false,false
W006,Power/Ground,UNASSIGNED,UNASSIGNED,UNVERIFIED,UNASSIGNED,false,false
W007,Power/Ground,UNASSIGNED,UNASSIGNED,UNVERIFIED,UNASSIGNED,false,false
W008,Power/Ground,UNASSIGNED,UNASSIGNED,UNVERIFIED,UNASSIGNED,false,false
W009,Power/Ground,UNASSIGNED,UNASSIGNED,UNVERIFIED,UNASSIGNED,false,false
W010,Power/Ground,UNASSIGNED,UNASSIGNED,UNVERIFIED,UNASSIGNED,false,false
W011,Power/Ground,UNASSIGNED,UNASSIGNED,UNVERIFIED,UNASSIGNED,false,false
W012,Power/Ground,UNASSIGNED,UNASSIGNED,UNVERIFIED,UNASSIGNED,false,false
W013,Power/Ground,UNASSIGNED,UNASSIGNED,UNVERIFIED,UNASSIGNED,false,false
W014,Power/Ground,UNASSIGNED,UNASSIGNED,UNVERIFIED,UNASSIGNED,false,false
W015,Power/Ground,UNASSIGNED,UNASSIGNED,UNVERIFIED,UNASSIGNED,false,false
W016,Power/Ground,UNASSIGNED,UNASSIGNED,UNVERIFIED,UNASSIGNED,false,false
W017,MCU/Clock,UNASSIGNED,UNASSIGNED,UNVERIFIED,UNASSIGNED,false,false
W018,MCU/Clock,UNASSIGNED,UNASSIGNED,UNVERIFIED,UNASSIGNED,false,false
W019,MCU/Clock,UNASSIGNED,UNASSIGNED,UNVERIFIED,UNASSIGNED,false,false
W020,MCU/Clock,UNASSIGNED,UNASSIGNED,UNVERIFIED,UNASSIGNED,false,false
W021,MCU/Clock,UNASSIGNED,UNASSIGNED,UNVERIFIED,UNASSIGNED,false,false
W022,MCU/Clock,UNASSIGNED,UNASSIGNED,UNVERIFIED,UNASSIGNED,false,false
W023,MCU/Clock,UNASSIGNED,UNASSIGNED,UNVERIFIED,UNASSIGNED,false,false
W024,MCU/Clock,UNASSIGNED,UNASSIGNED,UNVERIFIED,UNASSIGNED,false,false
W025,MCU/Clock,UNASSIGNED,UNASSIGNED,UNVERIFIED,UNASSIGNED,false,false
W026,MCU/Clock,UNASSIGNED,UNASSIGNED,UNVERIFIED,UNASSIGNED,false,false
W027,MCU/Clock,UNASSIGNED,UNASSIGNED,UNVERIFIED,UNASSIGNED,false,false
W028,MCU/Clock,UNASSIGNED,UNASSIGNED,UNVERIFIED,UNASSIGNED,false,false
W029,MCU/Clock,UNASSIGNED,UNASSIGNED,UNVERIFIED,UNASSIGNED,false,false
W030,MCU/Clock,UNASSIGNED,UNASSIGNED,UNVERIFIED,UNASSIGNED,false,false
W031,MCU/Clock,UNASSIGNED,UNASSIGNED,UNVERIFIED,UNASSIGNED,false,false
W032,MCU/Clock,UNASSIGNED,UNASSIGNED,UNVERIFIED,UNASSIGNED,false,false
W033,MCU/Clock,UNASSIGNED,UNASSIGNED,UNVERIFIED,UNASSIGNED,false,false
W034,MCU/Clock,UNASSIGNED,UNASSIGNED,UNVERIFIED,UNASSIGNED,false,false
W035,MCU/Clock,UNASSIGNED,UNASSIGNED,UNVERIFIED,UNASSIGNED,false,false
W036,MCU/Clock,UNASSIGNED,UNASSIGNED,UNVERIFIED,UNASSIGNED,false,false
W037,Differential Lanes,UNASSIGNED,UNASSIGNED,UNVERIFIED,UNASSIGNED,false,false
W038,Differential Lanes,UNASSIGNED,UNASSIGNED,UNVERIFIED,UNASSIGNED,false,false
W039,Differential Lanes,UNASSIGNED,UNASSIGNED,UNVERIFIED,UNASSIGNED,false,false
W040,Differential Lanes,UNASSIGNED,UNASSIGNED,UNVERIFIED,UNASSIGNED,false,false
W041,Differential Lanes,UNASSIGNED,UNASSIGNED,UNVERIFIED,UNASSIGNED,false,false
W042,Differential Lanes,UNASSIGNED,UNASSIGNED,UNVERIFIED,UNASSIGNED,false,false
W043,Differential Lanes,UNASSIGNED,UNASSIGNED,UNVERIFIED,UNASSIGNED,false,false
W044,Differential Lanes,UNASSIGNED,UNASSIGNED,UNVERIFIED,UNASSIGNED,false,false
W045,Differential Lanes,UNASSIGNED,UNASSIGNED,UNVERIFIED,UNASSIGNED,false,false
W046,Differential Lanes,UNASSIGNED,UNASSIGNED,UNVERIFIED,UNASSIGNED,false,false
W047,Differential Lanes,UNASSIGNED,UNASSIGNED,UNVERIFIED,UNASSIGNED,false,false
W048,Differential Lanes,UNASSIGNED,UNASSIGNED,UNVERIFIED,UNASSIGNED,false,false
W049,Differential Lanes,UNASSIGNED,UNASSIGNED,UNVERIFIED,UNASSIGNED,false,false
W050,Differential Lanes,UNASSIGNED,UNASSIGNED,UNVERIFIED,UNASSIGNED,false,false
W051,Differential Lanes,UNASSIGNED,UNASSIGNED,UNVERIFIED,UNASSIGNED,false,false
W052,Differential Lanes,UNASSIGNED,UNASSIGNED,UNVERIFIED,UNASSIGNED,false,false
W053,Differential Lanes,UNASSIGNED,UNASSIGNED,UNVERIFIED,UNASSIGNED,false,false
W054,Differential Lanes,UNASSIGNED,UNASSIGNED,UNVERIFIED,UNASSIGNED,false,false
W055,Differential Lanes,UNASSIGNED,UNASSIGNED,UNVERIFIED,UNASSIGNED,false,false
W056,Differential Lanes,UNASSIGNED,UNASSIGNED,UNVERIFIED,UNASSIGNED,false,false
W057,Differential Lanes,UNASSIGNED,UNASSIGNED,UNVERIFIED,UNASSIGNED,false,false
W058,Differential Lanes,UNASSIGNED,UNASSIGNED,UNVERIFIED,UNASSIGNED,false,false
W059,Differential Lanes,UNASSIGNED,UNASSIGNED,UNVERIFIED,UNASSIGNED,false,false
W060,Differential Lanes,UNASSIGNED,UNASSIGNED,UNVERIFIED,UNASSIGNED,false,false
W061,Termination/Reference,UNASSIGNED,UNASSIGNED,UNVERIFIED,UNASSIGNED,false,false
W062,Termination/Reference,UNASSIGNED,UNASSIGNED,UNVERIFIED,UNASSIGNED,false,false
W063,Termination/Reference,UNASSIGNED,UNASSIGNED,UNVERIFIED,UNASSIGNED,false,false
W064,Termination/Reference,UNASSIGNED,UNASSIGNED,UNVERIFIED,UNASSIGNED,false,false
W065,Termination/Reference,UNASSIGNED,UNASSIGNED,UNVERIFIED,UNASSIGNED,false,false
W066,Termination/Reference,UNASSIGNED,UNASSIGNED,UNVERIFIED,UNASSIGNED,false,false
W067,Termination/Reference,UNASSIGNED,UNASSIGNED,UNVERIFIED,UNASSIGNED,false,false
W068,Termination/Reference,UNASSIGNED,UNASSIGNED,UNVERIFIED,UNASSIGNED,false,false
W069,Termination/Reference,UNASSIGNED,UNASSIGNED,UNVERIFIED,UNASSIGNED,false,false
W070,Termination/Reference,UNASSIGNED,UNASSIGNED,UNVERIFIED,UNASSIGNED,false,false
W071,Termination/Reference,UNASSIGNED,UNASSIGNED,UNVERIFIED,UNASSIGNED,false,false
W072,Termination/Reference,UNASSIGNED,UNASSIGNED,UNVERIFIED,UNASSIGNED,false,false
W073,Termination/Reference,UNASSIGNED,UNASSIGNED,UNVERIFIED,UNASSIGNED,false,false
W074,Termination/Reference,UNASSIGNED,UNASSIGNED,UNVERIFIED,UNASSIGNED,false,false
W075,Termination/Reference,UNASSIGNED,UNASSIGNED,UNVERIFIED,UNASSIGNED,false,false
W076,Termination/Reference,UNASSIGNED,UNASSIGNED,UNVERIFIED,UNASSIGNED,false,false
W077,Termination/Reference,UNASSIGNED,UNASSIGNED,UNVERIFIED,UNASSIGNED,false,false
W078,Termination/Reference,UNASSIGNED,UNASSIGNED,UNVERIFIED,UNASSIGNED,false,false
W079,Termination/Reference,UNASSIGNED,UNASSIGNED,UNVERIFIED,UNASSIGNED,false,false
W080,Termination/Reference,UNASSIGNED,UNASSIGNED,UNVERIFIED,UNASSIGNED,false,false
W081,Measurement,UNASSIGNED,UNASSIGNED,UNVERIFIED,UNASSIGNED,false,false
W082,Measurement,UNASSIGNED,UNASSIGNED,UNVERIFIED,UNASSIGNED,false,false
W083,Measurement,UNASSIGNED,UNASSIGNED,UNVERIFIED,UNASSIGNED,false,false
W084,Measurement,UNASSIGNED,UNASSIGNED,UNVERIFIED,UNASSIGNED,false,false
W085,Measurement,UNASSIGNED,UNASSIGNED,UNVERIFIED,UNASSIGNED,false,false
W086,Measurement,UNASSIGNED,UNASSIGNED,UNVERIFIED,UNASSIGNED,false,false
W087,Measurement,UNASSIGNED,UNASSIGNED,UNVERIFIED,UNASSIGNED,false,false
W088,Measurement,UNASSIGNED,UNASSIGNED,UNVERIFIED,UNASSIGNED,false,false
W089,Measurement,UNASSIGNED,UNASSIGNED,UNVERIFIED,UNASSIGNED,false,false
W090,Measurement,UNASSIGNED,UNASSIGNED,UNVERIFIED,UNASSIGNED,false,false
W091,Measurement,UNASSIGNED,UNASSIGNED,UNVERIFIED,UNASSIGNED,false,false
W092,Measurement,UNASSIGNED,UNASSIGNED,UNVERIFIED,UNASSIGNED,false,false
W093,Measurement,UNASSIGNED,UNASSIGNED,UNVERIFIED,UNASSIGNED,false,false
W094,Measurement,UNASSIGNED,UNASSIGNED,UNVERIFIED,UNASSIGNED,false,false
W095,Measurement,UNASSIGNED,UNASSIGNED,UNVERIFIED,UNASSIGNED,false,false
W096,Measurement,UNASSIGNED,UNASSIGNED,UNVERIFIED,UNASSIGNED,false,false
W097,Safety,UNASSIGNED,UNASSIGNED,UNVERIFIED,UNASSIGNED,false,false
W098,Safety,UNASSIGNED,UNASSIGNED,UNVERIFIED,UNASSIGNED,false,false
W099,Safety,UNASSIGNED,UNASSIGNED,UNVERIFIED,UNASSIGNED,false,false
W100,Safety,UNASSIGNED,UNASSIGNED,UNVERIFIED,UNASSIGNED,false,false
W101,Safety,UNASSIGNED,UNASSIGNED,UNVERIFIED,UNASSIGNED,false,false
W102,Safety,UNASSIGNED,UNASSIGNED,UNVERIFIED,UNASSIGNED,false,false
W103,Safety,UNASSIGNED,UNASSIGNED,UNVERIFIED,UNASSIGNED,false,false
W104,Safety,UNASSIGNED,UNASSIGNED,UNVERIFIED,UNASSIGNED,false,false
W105,Safety,UNASSIGNED,UNASSIGNED,UNVERIFIED,UNASSIGNED,false,false
W106,Safety,UNASSIGNED,UNASSIGNED,UNVERIFIED,UNASSIGNED,false,false
W107,Safety,UNASSIGNED,UNASSIGNED,UNVERIFIED,UNASSIGNED,false,false
W108,Safety,UNASSIGNED,UNASSIGNED,UNVERIFIED,UNASSIGNED,false,false
```

## ANNEX E — All 28 unverified via task records

**Archive path:** `Level102_HSX10/Data/ViaTasks_28_REQUIRES_NETLIST.csv`. Source-preserved as provided; these tables are not auto-corrected.

```csv
via_id,net_id,start_layer,end_layer,manufacturing_type,starts_installed,certifiable
V01,UNVERIFIED,UNASSIGNED,UNASSIGNED,REQUIRES_STACKUP,false,false
V02,UNVERIFIED,UNASSIGNED,UNASSIGNED,REQUIRES_STACKUP,false,false
V03,UNVERIFIED,UNASSIGNED,UNASSIGNED,REQUIRES_STACKUP,false,false
V04,UNVERIFIED,UNASSIGNED,UNASSIGNED,REQUIRES_STACKUP,false,false
V05,UNVERIFIED,UNASSIGNED,UNASSIGNED,REQUIRES_STACKUP,false,false
V06,UNVERIFIED,UNASSIGNED,UNASSIGNED,REQUIRES_STACKUP,false,false
V07,UNVERIFIED,UNASSIGNED,UNASSIGNED,REQUIRES_STACKUP,false,false
V08,UNVERIFIED,UNASSIGNED,UNASSIGNED,REQUIRES_STACKUP,false,false
V09,UNVERIFIED,UNASSIGNED,UNASSIGNED,REQUIRES_STACKUP,false,false
V10,UNVERIFIED,UNASSIGNED,UNASSIGNED,REQUIRES_STACKUP,false,false
V11,UNVERIFIED,UNASSIGNED,UNASSIGNED,REQUIRES_STACKUP,false,false
V12,UNVERIFIED,UNASSIGNED,UNASSIGNED,REQUIRES_STACKUP,false,false
V13,UNVERIFIED,UNASSIGNED,UNASSIGNED,REQUIRES_STACKUP,false,false
V14,UNVERIFIED,UNASSIGNED,UNASSIGNED,REQUIRES_STACKUP,false,false
V15,UNVERIFIED,UNASSIGNED,UNASSIGNED,REQUIRES_STACKUP,false,false
V16,UNVERIFIED,UNASSIGNED,UNASSIGNED,REQUIRES_STACKUP,false,false
V17,UNVERIFIED,UNASSIGNED,UNASSIGNED,REQUIRES_STACKUP,false,false
V18,UNVERIFIED,UNASSIGNED,UNASSIGNED,REQUIRES_STACKUP,false,false
V19,UNVERIFIED,UNASSIGNED,UNASSIGNED,REQUIRES_STACKUP,false,false
V20,UNVERIFIED,UNASSIGNED,UNASSIGNED,REQUIRES_STACKUP,false,false
V21,UNVERIFIED,UNASSIGNED,UNASSIGNED,REQUIRES_STACKUP,false,false
V22,UNVERIFIED,UNASSIGNED,UNASSIGNED,REQUIRES_STACKUP,false,false
V23,UNVERIFIED,UNASSIGNED,UNASSIGNED,REQUIRES_STACKUP,false,false
V24,UNVERIFIED,UNASSIGNED,UNASSIGNED,REQUIRES_STACKUP,false,false
V25,UNVERIFIED,UNASSIGNED,UNASSIGNED,REQUIRES_STACKUP,false,false
V26,UNVERIFIED,UNASSIGNED,UNASSIGNED,REQUIRES_STACKUP,false,false
V27,UNVERIFIED,UNASSIGNED,UNASSIGNED,REQUIRES_STACKUP,false,false
V28,UNVERIFIED,UNASSIGNED,UNASSIGNED,REQUIRES_STACKUP,false,false
```

## ANNEX F — Source gameplay plan

**Archive path:** `Level102_HSX10/Data/GameplayPlan.json`. Source-preserved as provided; these tables are not auto-corrected.

```json
{
  "level": 102,
  "name": "SIGNAL PHANTOM",
  "board": "HSX-10",
  "size_mm": [
    250,
    175
  ],
  "copper_layers": 6,
  "player_routes": 108,
  "via_tasks": 28,
  "components": 82,
  "critical_wire": {
    "id": "W109",
    "from": "L102_LinkQualityMonitor_U9.TRUSTED_LINK_QUALITY_OUT",
    "to": "L102_HighSpeedSafetySupervisor_U11.TRUSTED_LINK_QUALITY_IN",
    "kind": "detachable_validity_signal",
    "starts_connected": false
  },
  "faults": [
    {
      "id": "F1",
      "description": "150 Ohm differential receiver termination instead of 100 Ohm",
      "target": "R1",
      "repair": "Replace termination resistor"
    },
    {
      "id": "F2",
      "description": "Excessive coupling between adjacent lane traces",
      "target": "Diff_Pair_A_Copper",
      "repair": "Rework controlled trace separation"
    },
    {
      "id": "F3",
      "description": "Unterminated transmission via stub",
      "target": "Via_Barrels",
      "repair": "Approved section rework or board replacement"
    }
  ],
  "objectives": [
    "Assembly",
    "Routing",
    "Commissioning",
    "Signal integrity",
    "Three fault recovery",
    "W109 safety recovery"
  ],
  "netlist_status": "UNVERIFIED - placeholder route and via source tables",
  "asset_quality": "procedurally detailed asset kit, not manufacturer-certified CAD"
}
```

## ANNEX G — Source asset summary

**Archive path:** `Level102_HSX10/Data/ExportSummary.json`. Source-preserved as provided; these tables are not auto-corrected.

```json
{
  "FBX": 105,
  "GLB": 105,
  "modular": 104,
  "components": 82,
  "secondary": 22,
  "triangle_total_modular": 30796,
  "vertex_total_modular": 19242,
  "pbr_material_families": 18,
  "pbr_maps": 90,
  "silkscreen_maps": 1,
  "routes_unverified": 108,
  "vias_unverified": 28,
  "offline_checks": "passed preliminary file and counts, no Unity import certification"
}
```

## ANNEX H — Original integration guide

**Archive path:** `Level102_HSX10/Documentation/INTEGRATION_GUIDE.md`. Source-preserved as provided; these tables are not auto-corrected.

```markdown
# PROJECT SPARK — LEVEL 102 SIGNAL PHANTOM

## Scope
Board-only procedural 3D mesh assets, not a complete Unity level. No environment, instrument FBXs, or UI meshes.

## Source of truth
- ModelManifest.csv contains 104 modular models; 82 are component placements.
- ComponentBOM.csv and ComponentPlacements.csv supply local-space placement references. Board physical size = **250 x 175 mm**. All 3D coordinates use metres.
- Assembled_Reference model is **showcase only**. Do not include it in the playable board.
- Start with 82 uninstalled parts, 108 incomplete copper routes, 28 vias missing, W109 detached.
- The 108 route/via tasks are planning placeholders, NOT a pin-level electrical schematic. Reconcile authentic nets, package pins and pad IDs before permitting gameplay certification.
- W109 carries a freshness-checked link-health validity signal, NOT physical differential data.

## Unity
Import FBXs. Invoke `Tools > Project Spark > Levels > 102 > 1. Build EMPTY HSX-10 Board`. Run material tool as a starting point, then assign lit materials based on imported material slots. The textures are reference PBR maps; actual mask packing and FBX mapping need validation. For accurate scale ensure Unity treats the FBX metre coordinate values as metres.

## Required gameplay logic (not supplied)
Real pad/net topology, 108 player copper routes, 28 vias, device dynamics, actual two differential channels, TDR, eye analyzer, BER qualification, impedance and parasitic model, separate safety supervisory loop, fault repair authority, virtual PCB repair UI integration, six objectives via `LevelGamePlayManager`.

## Quality limits
Procedurally detailed packages, leads, pads, connectors, reference copper, textures. Not manufacturer-accurate land patterns or artist-baked AAA material maps. FBX binary structure is generated and checked offline, but Unity import and shader compilation are not verified.
```

## ANNEX I — Original board builder source

**Archive path:** `Level102_HSX10/Unity/Editor/SparkL102BoardBuilder.cs`. Source-preserved as provided; these tables are not auto-corrected.

```csharp
#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.Level102.Editor
{
    public static class SparkL102BoardBuilder
    {
        [MenuItem("Tools/Project Spark/Levels/102/1. Build EMPTY HSX-10 Board")]
        public static void BuildEmpty()
        {
            string[] matches = AssetDatabase.FindAssets("t:Model 083_L102_PCB_Base");
            if (matches.Length == 0) { Debug.LogError("[L102] Import the FBX Models_FBX directory first."); return; }
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(matches[0]));
            if (!prefab) { Debug.LogError("[L102] Board FBX not found."); return; }
            var root = new GameObject("L102_HSX10_EMPTY_GAMEPLAY_ART");
            Undo.RegisterCreatedObjectUndo(root,"Create L102 empty art");
            var child = (GameObject)PrefabUtility.InstantiatePrefab(prefab,root.scene);
            Undo.SetTransformParent(child.transform,root.transform,"Parent board");
            child.transform.localPosition=Vector3.zero;
            child.transform.localRotation=Quaternion.identity;
            child.transform.localScale=Vector3.one;
            Selection.activeGameObject=root;
            Debug.Log("[L102] Created empty visual board only. No placed parts, completed routes or working game logic.");
        }
        [MenuItem("Tools/Project Spark/Levels/102/2. Validate Imported Models")]
        public static void ValidateModels()
        {
            int count=AssetDatabase.FindAssets("t:Model").Count(g=>AssetDatabase.GUIDToAssetPath(g).Contains("L102_"));
            Debug.Log($"[L102] Imported L102-prefixed FBX assets found: {count}. Expected 104 modular + 1 reference. This is ART validation only.");
        }
    }
}
#endif
```

## ANNEX J — Original materials builder source

**Archive path:** `Level102_HSX10/Unity/Editor/SparkL102MaterialBuilder.cs`. Source-preserved as provided; these tables are not auto-corrected.

```csharp
#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
namespace ProjectSpark.Level102.Editor
{
    public static class SparkL102MaterialBuilder
    {
        [MenuItem("Tools/Project Spark/Levels/102/3. Create Native PBR Materials")]
        public static void Build()
        {
            var guids=AssetDatabase.FindAssets("t:Texture2D");
            const string outdir="Assets/ProjectSparkGenerated/Level102/Materials";
            Directory.CreateDirectory(outdir);
            int count=0;
            foreach(var guid in guids)
            {
                var path=AssetDatabase.GUIDToAssetPath(guid);
                if(!path.EndsWith("_Albedo.png") || !path.Contains("Level102")) continue;
                var matname=Path.GetFileNameWithoutExtension(path).Replace("_Albedo","");
                var shader=Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("HDRP/Lit") ?? Shader.Find("Standard");
                if(!shader) {Debug.LogError("[L102] No supported lit shader found.");return;}
                var material=new Material(shader){name="L102_"+matname};
                var albedo=AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                if(material.HasProperty("_BaseMap")) material.SetTexture("_BaseMap",albedo);
                if(material.HasProperty("_MainTex")) material.SetTexture("_MainTex",albedo);
                var roughPath=path.Replace("_Albedo.png","_Roughness.png");
                // Roughness source is retained. Unity metallic/smoothness packing requires pipeline-specific conversion.
                AssetDatabase.CreateAsset(material,outdir+"/L102_"+matname+".mat");count++;
            }
            AssetDatabase.SaveAssets();AssetDatabase.Refresh();
            Debug.Log($"[L102] Created {count} base-color lit materials. Assign material slots and pack pipeline masks before final production use.");
        }
    }
}
#endif
```

## ANNEX K — Original art marker source

**Archive path:** `Level102_HSX10/Unity/Runtime/SparkL102VisualMarker.cs`. Source-preserved as provided; these tables are not auto-corrected.

```csharp
using UnityEngine;
namespace ProjectSpark.Level102
{
    public sealed class SparkL102VisualMarker : MonoBehaviour
    {
        [Tooltip("Art-only model or footprint reference, never an electrical net identifier.")]
        public string assetReference;
        [Tooltip("True only after your authoritative PCB placement service confirms installation.")]
        public bool installedVisualState;
    }
}
```

