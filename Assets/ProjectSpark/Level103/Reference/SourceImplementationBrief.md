# PROJECT SPARK — LEVEL 96: SENSOR MATRIX
## MASTER UNITY IMPLEMENTATION PROMPT — BOARD-ONLY SMX-4 PRECISION ACQUISITION

**Implementation document grounded in the actual `ProjectSpark_Level96_SensorMatrix_BoardOnly_Complete.zip` source kit.**

**Role:** You are the principal Unity engineer, electronics simulation architect, interactive PCB systems developer, and AAA UI/VFX integrator for Project Spark. Implement a *playable*, technically coherent Level 96, preserve existing Levels 1–95, reuse the working PCB systems introduced in Levels 94–95, and integrate the existing world-space virtual inspection/repair UI. Do not implement the game by simply displaying correct-looking readings, marking routes complete from decorative meshes, or declaring six objectives complete when a player presses a button.

**Non-negotiable boundary:** The provided assets are board-only. **Do not generate a new room, table, instrument FBX, furniture, physical soldering iron, or standalone multimeter/oscilloscope.** Use the existing Project Spark large-table scene and world-space UI repair toolkit. Source code and simulation are required for actual gameplay; source assets are not by themselves a complete level.

### Source-of-truth hierarchy
1. Inspect the live Unity codebase to discover actual `SparkTerminal`, electrical solver, Level GamePlay Manager, PCB placement/routing and touch systems. Use the real APIs and avoid guessing method signatures.
2. Use the **exact Level 96 source CSV/JSON definitions in annexes** for art, placement and proposed tasks; do not silently fabricate new source data.
3. Resolve **conceptual routing labels** into a physically valid schematic/netlist before enabling gameplay certification. If no approved netlist is available, produce a visible blocker report and disable certification of routing/measurement objectives; do not pretend conceptual labels constitute a real circuit.
4. Safety and gameplay truth must be authoritative: actual topology + calibrated measurements + fresh evidence + objective evaluator; UI and VFX are observers.

### Deliverables
- `Level_96.asset`, `Level_96_Layout.prefab`, board-only visual root, and modular component prefabs from the 62 models.
- One-click menu **`Tools/Project Spark/Levels/Create Level 96`** that performs idempotent import/build/validation, uses project-local paths and reports blockers.
- A verified component-pin/netlist reconciliation package (not just the conceptual `SMX4_NET_*` placeholders), pad/trace/via domain model and saved electrical topology.
- Playable assembly, 58 required copper-route tasks, 12 functional vias, 44 installed components, W059 removable feedback, measurement/calibration, three repairs, six objectives.
- Four reproducible analog channel models, 16-bit ADC, 2.500 V monitored reference, 3.3 V logic, 5 V protected input, genuine four-channel acquisition and reference health.
- Integration adapters to the existing world-space virtual repair toolkit (inspection, probe, solder/desolder, via/trace repair, replacement, calibration and contact VFX).
- Editor and Play Mode validation scripts, evidence log, failure cases, automated tests and a final acceptance report.

## 01. Confirmed package and known gaps

The source kit contains **62 modular binary FBXs and 62 GLBs**, plus **one visual-reference complete assembly in each format**. It contains **44 independent component models** and **18 non-component board-only models** (including the empty PCB). The source board is **140 × 95 × 1.6 mm** in **Unity metres** (`0.140 × 0.095 × 0.0016 m`); the supplied summary reports **10,716 triangles** over modular geometry. The asset package contains PBR source textures, three Unity C# helpers, 58 conceptual copper-route tasks, 12 via tasks and six objective rows.

**What the art package does NOT establish:** manufacturer-approved footprints; a verified pin-level schematic; electrically authoritative `SparkTerminal` graph; a working analog front-end; actual ADC converter behavior; gameplay repair mutations; Unity shader compilation; Play Mode success; automatically accepted objective completions. The source `Component_Contact_Plan.csv` includes **187 records** with `UNASSIGNED_PENDING_SCHEMATIC` signals; these are not usable as a finished netlist. Do not infer missing real circuit pins from geometrically adjacent pads.

The Unity-supplied `SparkLevel96VirtualRepairTarget.PlayRepairVisualOnly()` explicitly does not mutate the solver. Its `visuallyRepaired` flag is cosmetic, and must never satisfy an objective.

## 02. Starting state, preview separation, physical scale

The playable scene starts with the **empty SMX-4 PCB**, all **44 component placements uninstalled**, **58 player-created routes missing**, **12 functional vias missing**, and **W059 disconnected**. An assembled showcase exists only for previews, screenshots and art inspection. Do not instantiate its completed traces/parts as playable topology.

Maintain two coordinate spaces: board-manufacturing coordinates in millimetres for pads, trace clearance, footprints and vias; world positions in metres for rendering/interaction. A separate `VisualScaleRoot` can enlarge the board for the giant Project Spark table (propose 10×; parameterize), but solver dimensions and collision logic remain at real scale. Scale raycast/touch hit regions appropriately without changing component electrical values, thermal masses or trace width.

Use named runtime parents: `L96_BoardPhysicalRoot`, `L96_BoardArtRoot`, `L96_ComponentSlots`, `L96_PlayerCopperF_Cu`, `L96_PlayerCopperB_Cu`, `L96_ViaSockets`, `L96_RepairTargets`, `L96_InstrumentationAnchors`, `L96_W059PatchPoint`, `L96_WorldSpaceUIAnchor`, `L96_VFXRoot`. Separate completed showcase from all roots above.

## 03. Electrical truth and connection data

Build a reusable board graph: `Board` -> `Footprint` -> `Pad` -> `Net` -> `CopperTraceSegment` -> `Via` -> `Connector/Probe/Jumper`. Distinguish fixed factory copper, incomplete player copper, solder joints, physical component pins, and W059. An electrical route is a required *functional* connection, not merely a line mesh in the right area.

The supplied `Copper_Routing_Plan_W001_W058.csv` rows currently name generic `SMX4_NET_###_A/B` endpoints. Treat these as *task IDs only* until a trusted schematic assigns actual component pads/nets, layer transitions, clearances and testable circuit conditions. Define a typed mapping table with `TaskID`, `SourceFootprint`, `SourcePad`, `DestinationFootprint`, `DestinationPad`, `NetID`, `Layer`, `ViaIDs`, `FactoryPathPolicy`, `ConnectionCompatibility`, `TraceWidth_mm`, `Clearance_mm`, `ValidationMethod` and `ApprovalStatus`. Validate that preinstalled factory copper does not bypass incomplete player routes. Fail the builder with an actionable report when a required mapping is unresolved, not a fake completed board.

Check conductor polarity/domain, connected component orientation, incompatible net shorts, ground returns, analog/digital rail isolation where required, duplicate branches, layer continuity, solder integrity and via plating. Keep graph mutations atomic, undoable and saveable. A completed-looking trace with an open solder joint must remain electrically open.

## 04. Realistic SMX-4 component architecture

Implement the **44 source BOM entries exactly** (see Annex B). Model U1 as a generic 16-bit four-channel ADC, U2 as RTD excitation, U3 bridge instrumentation amplifier, U4 optical transimpedance amplifier, U5 Hall-signal filter, U6 precision reference, U7 acquisition MCU, U8 reference supervisor, U9 safety controller, plus R/C and board connectors/sensors/diode/LED/trimmer/jumper. Do not identify unspecified generic parts as pin-compatible commercial ICs without verifying datasheets and the source schematic.

Component placement workflow: select a virtual part, inspect orientation/value, tap the corresponding footprint, snap to named footprint anchor, confirm polarity and orientation, apply a soldering interaction if required, and only then add electrical terminal connectivity. Incorrect parts must remain visibly inspectable and electrically meaningful. A component replacement must preserve identity/history and require proper removal/reinstallation, not just change a label.

Source component art should have independent root transforms and local collider proxies. Keep opt-in VFX for solder wetting, contact inspection, desoldering and flux cleaning restrained; localize contact effects to actual pad/touch hit positions.

## 05. Four authoritative sensor signal chains

**CH0: RTD temperature.** Define a physically consistent PT1000-style resistance versus temperature curve for the target 10–70 °C scenario, excitation current, self-heating term, analog gain/offset and anti-alias filtering; produce sensor voltage from simulated actual temperature.

**CH1: force Wheatstone bridge.** Define bridge excitation, differential output versus 0–100 N load, instrumentation amplifier gain, common-mode voltage, CMRR approximation, noise and saturation. A broken bridge via must alter the analog circuit/measurement validity; not just a warning icon.

**CH2: optical photodiode.** Define photodiode current versus simulated illuminance (0–1000 lux educational case), transimpedance gain, output limits, dark current/noise and low-pass response. Over-high gain causes true analog saturation/clipping.

**CH3: Hall current sensor.** Define current stimulus 0–2 A, transfer slope, zero offset, noise, output filtering, current direction policy and voltage headroom.

All displayed values are derived from physical stimuli through the connected signal chains, not read directly from the simulator's hidden truth state.

## 06. ADC and calibration

Define **5 V protected supply**, **3.3 V logic**, **2.500 V nominal precision ADC reference**, **16-bit quantization** and **128 samples/sec/channel** (minimum aggregate conversion throughput 512/sec, plus settling). Confirm the actual chosen ADC input format (unipolar vs differential, single-ended vs multiplexed), sample-and-hold behavior, reference wiring, and timing. Treat `2.5V` as a monitored rail, not universally safe for every analog input without range conditioning.

For illustrative unipolar conversion: `rawCode = clamp(round(Vin / VrefActual * 65535),0,65535)` and `VinReported = rawCode * VrefAssumed / 65535`; actual calibration adds offset/gain and converter noise. Do not claim that 16-bit nominal resolution equals 16-bit absolute accuracy. Implement calibrated zero/span pairs for all four channels with explicit reference standards, timestamps and uncertainty. Sample status must include `VALID`, `STALE`, `SATURATED`, `OPEN_CIRCUIT`, `REFERENCE_UNTRUSTED`, `UNCALIBRATED`, `OUT_OF_RANGE`. Qualification requires fresh valid readings for all four channels and acceptable uncertainty.

For ratiometric bridge measurement, do not double-count reference excitation drift if the same physical excitation is used for both bridge and ADC reference. Store calibration provenance and reject self-referential calibration that silently changes true sensor state.

## 07. Sampling and data quality

Implement a deterministic simulation clock and channel scheduling, sample timestamps, acquisition buffer, digital filters, raw versus calibrated values, measured sample throughput, channel settle time, communication errors and power-up stabilization. Use seeded noise for repeatable tests. The UI shows four current readings, raw ADC code, calibrated physical values, a confidence/quality status, rail/reference health and fault history.

The screen instruments are **world-space Canvas panels**, not FBX objects. Bind the existing virtual multimeter, oscilloscope, logic analyzer, net tracer, thermal view and calibration panels to real project-side instrumentation providers. Display `DEMO / ILLUSTRATIVE` explicitly unless attached to a real compatible solver/measurement adapter. Never substitute procedural waveforms for measured waveforms while claiming a successful hardware test.

## 08. Virtual inspection and click/touch repair integration

Use the previously developed Project Spark AAA Virtual PCB Repair UI, but discover its installed classes, namespaces, menu roots and signal interfaces in the actual Unity project. Do not recreate a parallel UI when the existing one works. Board collider/pad hit -> target identity -> selected virtual tool -> eligibility/permissions -> real action request -> authoritative solver/board state mutation -> confirmed new measurements -> one localized VFX event.

Virtual tool categories: Inspect (microscope, net trace, continuity, voltage probe, scope, thermal, logic); Repair (solder, desolder, trace repair, via plating, cleaning, component replacement); Calibration (known stimulus, reference check, zero/span). Disallow live solder work on energized dangerous traces per simulation protection state; use game-safe guarded operations.

Prevent the accidental screen-space Canvas blocking PCB raycasts: explicit input priority, pointer-over-UI checks, GraphicRaycaster and PhysicsRaycaster separation, transparent images with `raycastTarget=false`, multi-touch gesture handling, tooltip occlusion checks, tool targeting ghost, click debounce and cancellation. Show why an action failed (wrong tool, wrong pad, unsafe power, blocked solver, incompatible route).

## 09. Repair VFX authored procedurally

Reuse existing virtual-tool VFX where present. At the actual clicked pad/trace/component position: small copper/solder glint, controlled spark (only for appropriate fault actions), translucent flux vapor, thermal color pulse, solder bridge removal, via-plating indication, test-probe contact ripple and thin highlighted net path. Keep particle counts and emissions suitable for mobile; pool effects. Never spawn VFX at arbitrary canvas origin or scale with the board's visual expansion. Cosmetic feedback is reported only after a gameplay request is accepted, but a repair is considered complete only once the authoritative state is verified.

## 10. Three fault scenarios (source exact names)

**F1 — Reference voltage drift.** Degrade real U6/reference behavior and measured code scaling; player confirms against an independent qualified reference, replaces/repairs fault, recalibrates and reacquires fresh samples.

**F2 — Open strain bridge via.** Introduce an actual open electrical path on an approved CH1 via; net tracer and continuity reading reveal the break; repair via and retest bridge zero/span and sensor acquisition.

**F3 — Photodiode transimpedance amplifier saturation.** Misconfigure/alter the physical gain path or load/illumination stimulus until U4 output clips; repair hardware/configuration and requalify low/high light points. Filtering a clipped waveform does not fix saturation.

Activate faults only in the designed objective phase or by explicit test injection. The source `Faults_3.csv` marks all three `not active` initially. Require a causal measurement change, diagnosis evidence, appropriate tool actions and new post-repair qualification. Multiple concurrent faults need distinct evidence and safe recovery handling.

## 11. W059 critical feedback and recovery

Use **W059** exactly as specified in source JSON:
- Source: `L96_ReferenceSupervisor_U8.TRUSTED_REF_VALID_OUT`
- Destination: `L96_SafetyController_U9.TRUSTED_REF_VALID_IN`
- `initially_connected = false`
- This is a **digital trusted reference-validity monitor, NOT the 2.500 V reference supply**.

Normal commissioning first requires the player to install W059, obtain valid reference status and complete Objectives 1–5. During a supervised final test, player **physically removes** W059. The solver detects open topology, the safety controller marks trusted reference samples invalid/stale, inhibits safety-dependent output authority and new certification, and latches an explicit fault. Physical sensor temperatures/force/illumination/current remain continuous; sample history is preserved but cannot be falsely reused as fresh trusted proof.

Repair sequence: restore physical W059 connection -> verify electrical continuity -> acquire fresh reference-supervisor samples -> recalibrate reference/ADC as required -> explicitly reset latched safety -> complete a **new** four-channel test sequence -> certify Objective 6. Do not auto-complete from reconnect or UI click. Objectives 1–5 remain historically certified; temporary safety-state regression affects current authorization, not previous accomplishment records.

## 12. Six objective certification contract

1. **Assembly:** all 44 physical placements correct, solder connectivity and BOM validated, no fake installed showcase parts.
2. **Routing:** 58 actual player routes and 12 functional vias verified, all power/return/safety nets satisfy real board graph and W059 is installed for commissioning.
3. **Calibration:** 5 V/3.3 V rails, 2.500 V reference, ADC offset/gain and four sensor calibration steps valid with fresh evidence.
4. **Acquisition:** dynamic four-channel stimulus scenario, correct sampling, filters, no clipping, acceptable uncertainty, stable operation throughout full qualification interval.
5. **Fault recovery:** F1/F2/F3 diagnosed and genuinely repaired, then fresh independent qualification after repairs.
6. **W059:** open-circuit proof, fail-safe response, physical reconnection, fresh calibration, explicit reset and full new four-channel qualification.

Use the actual `LevelGamePlayManager`/objective evaluation APIs discovered in this project. Each objective must accept a typed evidence record with timestamp, topology revision, system revision and measurement interval. No manual success flags, UI counter increment buttons, animations or scripted bypasses. Completion must be durable across save/load and remain correct under subsequent cable removal.

## 13. AAA art, materials, LOD and collisions

Use exact model file names from Annex A. Preserve local origins and board art placement from Annex B. Actual unit coordinates are metres and board geometry is 1.6 mm thick. Repair targets and solder pads require precise individual raycast colliders; visible tiny pads may have slightly larger invisible selection colliders controlled by camera zoom, without altering measured copper sizes. Use mesh normals, proper face orientation, plausible IC lead colors and gold contacts. Remap PBR source assets to Unity pipeline-native materials; glTF metallic-roughness textures do not automatically become valid URP/HDRP mask maps. Validate texture sRGB/non-sRGB import settings and normal-map types.

Do not promise manufacturer-grade footprints or AAA film quality from procedural sources. Treat the mesh as production foundation and add art review gates for solder fillets, leg geometry, pad alignment, realistic silkscreen and zoomed inspection readability. Use the user's existing room/table lighting and avoid new environmental FBXs.

## 14. Scene builder and file organization

Target editor menu `Tools/Project Spark/Levels/Create Level 96`; also retain source art-helper paths:
- `Tools/Project Spark/Levels/96/Board Only/1. Create Empty SMX-4 Board`
- `Tools/Project Spark/Levels/96/Board Only/2. Create Completed Reference (Visual Only)`
- `Tools/Project Spark/Levels/96/Board Only/3. Generate Native PBR Materials`

Create `Assets/ProjectSpark/Levels/Level96/` with `Art`, `Materials`, `Prefabs`, `Data`, `Simulation`, `Interaction`, `Editor`, `Tests`, `Reports`. Discover source directories using `AssetDatabase` IDs or explicit file mapping, not a hardcoded nested `Assets/` path; the user's earlier virtual toolkit had a nested-folder material installer issue. Ensure all generated Editor scripts are in Editor assemblies and runtime scripts are not. Use uniquely named assets and safe idempotent rebuilding; no modifications to other levels or project global settings.

Phases: A) source scan + project API audit, B) art build, C) component/pad schema mapping and schematic approval, D) connection gameplay, E) analog/ADC simulation, F) tool/repair UI integration, G) safety/objectives, H) automated validation, I) Unity Play Mode qualification. Stop and report blockers where interfaces or verified netlists are absent.

## 15. Implementation rules for existing Project Spark code

Do not invent methods on `SparkTerminal`, `SparkElectricalSolver`, `SparkCircuitSystem`, `LevelGamePlayManager` or related code. Inspect exact definitions, discover suitable interfaces and create limited adapters only if necessary. Preserve existing shop, throw/recycle systems, component registration, existing PCB levels, all other level assets and scenes. All generated code must compile in the project's exact Unity version and render pipeline. Avoid searching or replacing arbitrary assets by name when GUID references are available.

Place runtime handlers behind clearly separated authority interfaces: `IBoardTopologyAuthority`, `IComponentPlacementAuthority`, `ISensorStimulusSource`, `IAnalogMeasurementProvider`, `IReferenceStatusAuthority`, `IRepairActionAuthority`, `IObjectiveEvidencePublisher`, `IWorldSpaceToolTarget`. These are design abstractions, not claims about already installed classes. Use explicit adapters, inspector bindings and diagnostics if real systems differ.

## 16. Gameplay tutorial and UX

Create a short step-by-step first-time walkthrough: inspect the bare board -> identify CH0–CH3 zones -> select and place an IC -> solder a pad -> route an F.Cu trace -> add a via -> verify continuity -> install W059 -> calibrate reference -> test sensors -> repair a fault. Tool selection must show a valid hovered-target label and action preview. Display `0/44 COMPONENTS`, `0/58 COPPER`, `0/12 VIAS`, `0/6 OBJECTIVES` at start. The user can disable tutorial highlights and work freely. Every rejected action has a precise readable cause.

At high zoom, allow top/bottom board layer inspection; hide/show silkscreen, copper, solder mask, pads and net highlight independently without modifying real electrical graph. Inconsistent physical axes (`X-right`, `Y-up`, `Z-board`) must be handled consistently during surface interaction and animations.

## 17. System performance and platform behavior

Keep precision simulation in double precision where appropriate and convert to floats for visuals. Do not drive ADC sampling at inconsistent `Update()` frame rate; use a fixed deterministic simulation clock with controlled sample scheduler. Pool all contact VFX and UI graphs; don't allocate arrays in hot paths. Support PC mouse and mobile tap/drag, with an interaction hit proxy for millimetre-scale pads at high zoom. Provide saved topology state, current calibration results, placed component identity, fault state, W059 safety history and objective evidence for robust session persistence.

## 18. Engineering acceptance tests — 84 proposed cases

Each test must execute in Unity Edit Mode or Play Mode as appropriate, with setup, action, expected solver topology/measurement/authority state and failure diagnostics. Test identifiers are normative for this implementation prompt; these tests are not claimed to have been run in Unity.

### Source import & setup
- **L96-AT-001:** All 62 named modular FBXs resolve via exact manifests.
- **L96-AT-002:** All 62 GLBs resolve for reference checks.
- **L96-AT-003:** Complete reference FBX excluded from gameplay roots.
- **L96-AT-004:** Empty PCB scale equals 140x95x1.6mm.
- **L96-AT-005:** No room/furniture/test-instrument FBXs created.
- **L96-AT-006:** Unity menu root uses Levels/96 and remains unique.
- **L96-AT-007:** Project nested-folder import does not break material generation.
- **L96-AT-008:** Art builder is idempotent and preserves unrelated scenes.

### Components & placement
- **L96-AT-009:** 44 BOM parts start uninstalled.
- **L96-AT-010:** Correct footprint accepts correct component.
- **L96-AT-011:** Incorrect package is rejected.
- **L96-AT-012:** Reversed polarized part is diagnosed.
- **L96-AT-013:** Solderless placement does not close circuit.
- **L96-AT-014:** Replacement preserves part and repair history.
- **L96-AT-015:** Part collisions/hit areas correspond to pads.
- **L96-AT-016:** Board visual scaling does not modify physical parameters.

### Topology & routing
- **L96-AT-017:** 58 incomplete tasks start at zero.
- **L96-AT-018:** 12 vias start uninstalled.
- **L96-AT-019:** Conceptual SMX4_NET labels do not certify power.
- **L96-AT-020:** Unmapped contact nets block routing certification.
- **L96-AT-021:** Authorized F.Cu trace connects correct pad net.
- **L96-AT-022:** Authorized B.Cu route connects through proper via.
- **L96-AT-023:** Incompatible nets cannot be shorted silently.
- **L96-AT-024:** Broken solder joint remains electrically open.
- **L96-AT-025:** Factory copper cannot bypass a required player task.
- **L96-AT-026:** Duplicate route attempt does not increment progress.
- **L96-AT-027:** Copper routing persists across save/load.
- **L96-AT-028:** W059 has separate identity from all 58 routes.

### Analog channels
- **L96-AT-029:** RTD output moves correctly with temperature.
- **L96-AT-030:** Open RTD path invalidates reading.
- **L96-AT-031:** Bridge zero/span changes with applied force.
- **L96-AT-032:** Open bridge via invalidates force channel.
- **L96-AT-033:** Optical current changes with illumination.
- **L96-AT-034:** TIA output clips at gain/illumination limit.
- **L96-AT-035:** Hall sensor zero-current offset is measurable.
- **L96-AT-036:** Hall signal follows current stimulus.

### ADC & timing
- **L96-AT-037:** Vref nominal 2.500V with independent measured status.
- **L96-AT-038:** Untrusted reference inhibits qualification.
- **L96-AT-039:** 16-bit quantization honors input range.
- **L96-AT-040:** Sampling throughput meets 128 samples/s/channel.
- **L96-AT-041:** Multiplexing settles before code acceptance.
- **L96-AT-042:** Calibrated values retain raw code history.
- **L96-AT-043:** Stale measurement cannot be accepted.
- **L96-AT-044:** Saturated signal cannot pass channel accuracy test.
- **L96-AT-045:** Reference drift changes non-ratiometric reported reading.
- **L96-AT-046:** Ratiometric scenario avoids double-counting drift.

### World-space UI & interaction
- **L96-AT-047:** World-space instrument panels use authoritative provider.
- **L96-AT-048:** Demo waveform visibly labeled illustrative.
- **L96-AT-049:** Touch PCB pad works with GUI dock visible.
- **L96-AT-050:** UI raycaster does not block board hits improperly.
- **L96-AT-051:** Probe click maps to correct net and position.
- **L96-AT-052:** VFX never changes solver without repair authority.
- **L96-AT-053:** Repair effects spawn at touch hit, not canvas origin.
- **L96-AT-054:** Solder requires valid target and permitted power state.
- **L96-AT-055:** Via repair restores actual board connectivity.
- **L96-AT-056:** Component replacement re-runs calibration requirements.

### Faults & recovery
- **L96-AT-057:** F1 inactive in initial state.
- **L96-AT-058:** F1 alters real reference and reading.
- **L96-AT-059:** F1 repair demands valid independent test.
- **L96-AT-060:** F2 opens physically mapped bridge via.
- **L96-AT-061:** F2 restored via changes actual topology.
- **L96-AT-062:** F3 creates observable clipping.
- **L96-AT-063:** F3 repair removes saturation under test.
- **L96-AT-064:** All three repairs demand fresh qualification.

### W059 & safety
- **L96-AT-065:** W059 starts disconnected.
- **L96-AT-066:** Initial commissioning requires W059 installed.
- **L96-AT-067:** W059 disconnect opens actual trusted feedback.
- **L96-AT-068:** Open W059 does not force physical Vref to zero.
- **L96-AT-069:** Open W059 withdraws certificate authorization.
- **L96-AT-070:** W059 failure latches until explicit reset.
- **L96-AT-071:** Reconnection alone cannot finish Objective 6.
- **L96-AT-072:** Fresh reference and four-channel run required after reset.

### Objectives & regression
- **L96-AT-073:** Objectives 1–6 start incomplete.
- **L96-AT-074:** Assembled preview cannot complete Objective 1.
- **L96-AT-075:** Decorative copper cannot complete Objective 2.
- **L96-AT-076:** Invalid reference cannot complete Objective 3.
- **L96-AT-077:** Stale measurements cannot complete Objective 4.
- **L96-AT-078:** Cosmetic repair cannot complete Objective 5.
- **L96-AT-079:** UI click alone cannot complete Objective 6.
- **L96-AT-080:** Other levels and existing electrical flows remain unchanged.

### Persistence & validation
- **L96-AT-081:** Board state round-trips through save/load.
- **L96-AT-082:** Historical objective evidence survives W059 outage.
- **L96-AT-083:** Unity compilation passes target render pipeline.
- **L96-AT-084:** All 6 objectives pass in full Play Mode scenario.

## 19. Mandatory final execution checklist

1. Inspect the real project structure and all relevant APIs; report references and integration decisions.
2. Import 62 models with correct physical scale, bind 44 component slots, create Level 96 empty board and separate preview.
3. Produce/obtain an approved pin-level schematic and reconcile 187 component contact rows, 58 routing tasks and 12 vias before permitting route certification.
4. Implement true placement/solder/copper/via/W059 state transitions.
5. Integrate real 5 V/3.3 V/2.500 V behavior, CH0–CH3 analog fronts, 16-bit ADC, sampling and calibration.
6. Reuse world-space virtual tools with authoritative measurement and repair adapters; pool local click VFX.
7. Implement source faults F1–F3 and W059 supervised failure/recovery with explicit reset.
8. Bind all six objectives to actual LevelGamePlayManager evidence and save/load.
9. Run Unity compilation and all 84 acceptance tests; report pass/fail individually and fix errors.
10. End only with confirmed Play Mode `6/6` if it actually occurs. Otherwise provide blockers and truthful partial completion; never claim success based solely on generated assets.

## ANNEX A — Exact 62 model asset manifest

Source: `Data/Asset_Manifest_62.csv`. Embedded verbatim for exact task/asset naming; placeholders remain unresolved until real schematic verification.

```csv
Index,Name,FBX,GLB,MeshParts,Triangles,GameplayStart
0,L96_SMX4_PCB_140x95mm_Empty,Models_FBX/00_L96_SMX4_PCB_140x95mm_Empty.fbx,Models_GLB/00_L96_SMX4_PCB_140x95mm_Empty.glb,260,5264,bare board
1,L96_U1_IC,Models_FBX/01_L96_U1_IC.fbx,Models_GLB/01_L96_U1_IC.glb,23,300,uninstalled / hidden
2,L96_U2_IC,Models_FBX/02_L96_U2_IC.fbx,Models_GLB/02_L96_U2_IC.glb,11,156,uninstalled / hidden
3,L96_U3_IC,Models_FBX/03_L96_U3_IC.fbx,Models_GLB/03_L96_U3_IC.glb,11,156,uninstalled / hidden
4,L96_U4_IC,Models_FBX/04_L96_U4_IC.fbx,Models_GLB/04_L96_U4_IC.glb,11,156,uninstalled / hidden
5,L96_U5_IC,Models_FBX/05_L96_U5_IC.fbx,Models_GLB/05_L96_U5_IC.glb,11,156,uninstalled / hidden
6,L96_U6_IC,Models_FBX/06_L96_U6_IC.fbx,Models_GLB/06_L96_U6_IC.glb,11,156,uninstalled / hidden
7,L96_U7_IC,Models_FBX/07_L96_U7_IC.fbx,Models_GLB/07_L96_U7_IC.glb,35,444,uninstalled / hidden
8,L96_U8_IC,Models_FBX/08_L96_U8_IC.fbx,Models_GLB/08_L96_U8_IC.glb,11,156,uninstalled / hidden
9,L96_U9_IC,Models_FBX/09_L96_U9_IC.fbx,Models_GLB/09_L96_U9_IC.glb,11,156,uninstalled / hidden
10,L96_R1_Resistor,Models_FBX/10_L96_R1_Resistor.fbx,Models_GLB/10_L96_R1_Resistor.glb,5,60,uninstalled / hidden
11,L96_R2_Resistor,Models_FBX/11_L96_R2_Resistor.fbx,Models_GLB/11_L96_R2_Resistor.glb,5,60,uninstalled / hidden
12,L96_R3_Resistor,Models_FBX/12_L96_R3_Resistor.fbx,Models_GLB/12_L96_R3_Resistor.glb,5,60,uninstalled / hidden
13,L96_R4_Resistor,Models_FBX/13_L96_R4_Resistor.fbx,Models_GLB/13_L96_R4_Resistor.glb,5,60,uninstalled / hidden
14,L96_R5_Resistor,Models_FBX/14_L96_R5_Resistor.fbx,Models_GLB/14_L96_R5_Resistor.glb,5,60,uninstalled / hidden
15,L96_R6_Resistor,Models_FBX/15_L96_R6_Resistor.fbx,Models_GLB/15_L96_R6_Resistor.glb,5,60,uninstalled / hidden
16,L96_R7_Resistor,Models_FBX/16_L96_R7_Resistor.fbx,Models_GLB/16_L96_R7_Resistor.glb,5,60,uninstalled / hidden
17,L96_R8_Resistor,Models_FBX/17_L96_R8_Resistor.fbx,Models_GLB/17_L96_R8_Resistor.glb,5,60,uninstalled / hidden
18,L96_R9_Resistor,Models_FBX/18_L96_R9_Resistor.fbx,Models_GLB/18_L96_R9_Resistor.glb,5,60,uninstalled / hidden
19,L96_R10_Resistor,Models_FBX/19_L96_R10_Resistor.fbx,Models_GLB/19_L96_R10_Resistor.glb,5,60,uninstalled / hidden
20,L96_R11_Resistor,Models_FBX/20_L96_R11_Resistor.fbx,Models_GLB/20_L96_R11_Resistor.glb,5,60,uninstalled / hidden
21,L96_R12_Resistor,Models_FBX/21_L96_R12_Resistor.fbx,Models_GLB/21_L96_R12_Resistor.glb,5,60,uninstalled / hidden
22,L96_R13_Resistor,Models_FBX/22_L96_R13_Resistor.fbx,Models_GLB/22_L96_R13_Resistor.glb,5,60,uninstalled / hidden
23,L96_R14_Resistor,Models_FBX/23_L96_R14_Resistor.fbx,Models_GLB/23_L96_R14_Resistor.glb,5,60,uninstalled / hidden
24,L96_C1_Capacitor,Models_FBX/24_L96_C1_Capacitor.fbx,Models_GLB/24_L96_C1_Capacitor.glb,5,60,uninstalled / hidden
25,L96_C2_Capacitor,Models_FBX/25_L96_C2_Capacitor.fbx,Models_GLB/25_L96_C2_Capacitor.glb,5,60,uninstalled / hidden
26,L96_C3_Capacitor,Models_FBX/26_L96_C3_Capacitor.fbx,Models_GLB/26_L96_C3_Capacitor.glb,5,60,uninstalled / hidden
27,L96_C4_Capacitor,Models_FBX/27_L96_C4_Capacitor.fbx,Models_GLB/27_L96_C4_Capacitor.glb,5,60,uninstalled / hidden
28,L96_C5_Capacitor,Models_FBX/28_L96_C5_Capacitor.fbx,Models_GLB/28_L96_C5_Capacitor.glb,5,60,uninstalled / hidden
29,L96_C6_Capacitor,Models_FBX/29_L96_C6_Capacitor.fbx,Models_GLB/29_L96_C6_Capacitor.glb,5,60,uninstalled / hidden
30,L96_C7_Capacitor,Models_FBX/30_L96_C7_Capacitor.fbx,Models_GLB/30_L96_C7_Capacitor.glb,5,60,uninstalled / hidden
31,L96_C8_Capacitor,Models_FBX/31_L96_C8_Capacitor.fbx,Models_GLB/31_L96_C8_Capacitor.glb,5,60,uninstalled / hidden
32,L96_C9_Capacitor,Models_FBX/32_L96_C9_Capacitor.fbx,Models_GLB/32_L96_C9_Capacitor.glb,5,60,uninstalled / hidden
33,L96_C10_Capacitor,Models_FBX/33_L96_C10_Capacitor.fbx,Models_GLB/33_L96_C10_Capacitor.glb,5,60,uninstalled / hidden
34,L96_C11_Capacitor,Models_FBX/34_L96_C11_Capacitor.fbx,Models_GLB/34_L96_C11_Capacitor.glb,5,60,uninstalled / hidden
35,L96_S1_Sensor,Models_FBX/35_L96_S1_Sensor.fbx,Models_GLB/35_L96_S1_Sensor.glb,7,144,uninstalled / hidden
36,L96_S2_Sensor,Models_FBX/36_L96_S2_Sensor.fbx,Models_GLB/36_L96_S2_Sensor.glb,8,96,uninstalled / hidden
37,L96_S3_Sensor,Models_FBX/37_L96_S3_Sensor.fbx,Models_GLB/37_L96_S3_Sensor.glb,7,184,uninstalled / hidden
38,L96_S4_Sensor,Models_FBX/38_L96_S4_Sensor.fbx,Models_GLB/38_L96_S4_Sensor.glb,7,168,uninstalled / hidden
39,L96_J1_Connector,Models_FBX/39_L96_J1_Connector.fbx,Models_GLB/39_L96_J1_Connector.glb,5,60,uninstalled / hidden
40,L96_J2_Connector,Models_FBX/40_L96_J2_Connector.fbx,Models_GLB/40_L96_J2_Connector.glb,5,60,uninstalled / hidden
41,L96_RV1_Trimmer,Models_FBX/41_L96_RV1_Trimmer.fbx,Models_GLB/41_L96_RV1_Trimmer.glb,5,120,uninstalled / hidden
42,L96_LED1_LED,Models_FBX/42_L96_LED1_LED.fbx,Models_GLB/42_L96_LED1_LED.glb,4,48,uninstalled / hidden
43,L96_W059_Jumper,Models_FBX/43_L96_W059_Jumper.fbx,Models_GLB/43_L96_W059_Jumper.glb,5,60,uninstalled / hidden
44,L96_D1_Diode,Models_FBX/44_L96_D1_Diode.fbx,Models_GLB/44_L96_D1_Diode.glb,4,48,uninstalled / hidden
45,L96_ENIG_TestPad_Grid,Models_FBX/45_L96_ENIG_TestPad_Grid.fbx,Models_GLB/45_L96_ENIG_TestPad_Grid.glb,4,144,uninstalled / hidden
46,L96_Optical_Fiducials,Models_FBX/46_L96_Optical_Fiducials.fbx,Models_GLB/46_L96_Optical_Fiducials.glb,4,144,uninstalled / hidden
47,L96_W059_HeaderBase,Models_FBX/47_L96_W059_HeaderBase.fbx,Models_GLB/47_L96_W059_HeaderBase.glb,1,12,uninstalled / hidden
48,L96_SMT_Rework_SolderBeads,Models_FBX/48_L96_SMT_Rework_SolderBeads.fbx,Models_GLB/48_L96_SMT_Rework_SolderBeads.glb,1,12,uninstalled / hidden
49,L96_Soldermask_Repair_Patch,Models_FBX/49_L96_Soldermask_Repair_Patch.fbx,Models_GLB/49_L96_Soldermask_Repair_Patch.glb,1,12,uninstalled / hidden
50,L96_Copper_Repair_Strap,Models_FBX/50_L96_Copper_Repair_Strap.fbx,Models_GLB/50_L96_Copper_Repair_Strap.glb,3,36,uninstalled / hidden
51,L96_Via_Repair_Sleeve,Models_FBX/51_L96_Via_Repair_Sleeve.fbx,Models_GLB/51_L96_Via_Repair_Sleeve.glb,4,144,uninstalled / hidden
52,L96_Trace_Open_Fault_Coupon,Models_FBX/52_L96_Trace_Open_Fault_Coupon.fbx,Models_GLB/52_L96_Trace_Open_Fault_Coupon.glb,1,12,uninstalled / hidden
53,L96_SolderBridge_Fault_Coupon,Models_FBX/53_L96_SolderBridge_Fault_Coupon.fbx,Models_GLB/53_L96_SolderBridge_Fault_Coupon.glb,3,36,uninstalled / hidden
54,L96_Reference_Supervisor_TestPads,Models_FBX/54_L96_Reference_Supervisor_TestPads.fbx,Models_GLB/54_L96_Reference_Supervisor_TestPads.glb,4,144,uninstalled / hidden
55,L96_ADC_Analog_Guard_Ring,Models_FBX/55_L96_ADC_Analog_Guard_Ring.fbx,Models_GLB/55_L96_ADC_Analog_Guard_Ring.glb,3,36,uninstalled / hidden
56,L96_Grounding_Land_Pattern,Models_FBX/56_L96_Grounding_Land_Pattern.fbx,Models_GLB/56_L96_Grounding_Land_Pattern.glb,3,36,uninstalled / hidden
57,L96_Sensor_Connector_Contacts,Models_FBX/57_L96_Sensor_Connector_Contacts.fbx,Models_GLB/57_L96_Sensor_Connector_Contacts.glb,1,12,uninstalled / hidden
58,L96_RTD_Probe_Housing,Models_FBX/58_L96_RTD_Probe_Housing.fbx,Models_GLB/58_L96_RTD_Probe_Housing.glb,1,96,uninstalled / hidden
59,L96_Photodiode_Lens_Cover,Models_FBX/59_L96_Photodiode_Lens_Cover.fbx,Models_GLB/59_L96_Photodiode_Lens_Cover.glb,1,96,uninstalled / hidden
60,L96_Strain_Bridge_CalibrationPads,Models_FBX/60_L96_Strain_Bridge_CalibrationPads.fbx,Models_GLB/60_L96_Strain_Bridge_CalibrationPads.glb,4,144,uninstalled / hidden
61,L96_Board_Identification_Plate,Models_FBX/61_L96_Board_Identification_Plate.fbx,Models_GLB/61_L96_Board_Identification_Plate.glb,1,12,uninstalled / hidden
```

## ANNEX B — Exact 44-component BOM and placements

Source: `Data/Component_BOM_44.csv`. Embedded verbatim for exact task/asset naming; placeholders remain unresolved until real schematic verification.

```csv
Ref,Kind,Value,X_mm,Z_mm,Width_mm,Depth_mm,Package,Required
U1,IC,16-bit 4CH ADC,9,2,10,9,TSSOP-20,True
U2,IC,RTD Excitation,-39,25,7.5,5.2,SOIC-8,True
U3,IC,Bridge Amplifier,-39,8,7.5,5.2,SOIC-8,True
U4,IC,Optical TIA,-39,-9,7.5,5.2,SOIC-8,True
U5,IC,Hall Signal Filter,-39,-26,7.5,5.2,SOIC-8,True
U6,IC,2.5V Reference,6,28,7.5,5.2,SOIC-8,True
U7,IC,Acquisition MCU,38,0,11,11,QFP-32,True
U8,IC,Reference Supervisor,34,-25,7.5,5.2,SOIC-8,True
U9,IC,Safety Controller,53,-25,7.5,5.2,SOIC-8,True
R1,Resistor,1kΩ,-21,33,3.2,1.6,0805,True
R2,Resistor,10kΩ,-11,33,3.2,1.6,0805,True
R3,Resistor,100Ω,-1,33,3.2,1.6,0805,True
R4,Resistor,4.7kΩ,9,33,3.2,1.6,0805,True
R5,Resistor,2.2kΩ,19,33,3.2,1.6,0805,True
R6,Resistor,0Ω,29,33,3.2,1.6,0805,True
R7,Resistor,47kΩ,39,33,3.2,1.6,0805,True
R8,Resistor,1kΩ,-21,-36,3.2,1.6,0805,True
R9,Resistor,10kΩ,-11,-36,3.2,1.6,0805,True
R10,Resistor,100Ω,-1,-36,3.2,1.6,0805,True
R11,Resistor,4.7kΩ,9,-36,3.2,1.6,0805,True
R12,Resistor,2.2kΩ,19,-36,3.2,1.6,0805,True
R13,Resistor,0Ω,29,-36,3.2,1.6,0805,True
R14,Resistor,47kΩ,39,-36,3.2,1.6,0805,True
C1,Capacitor,100nF,-22,19,3.0,1.8,0805,True
C2,Capacitor,1uF,-12,19,3.0,1.8,0805,True
C3,Capacitor,10nF,-2,19,3.0,1.8,0805,True
C4,Capacitor,4.7uF,8,19,3.0,1.8,0805,True
C5,Capacitor,10uF,18,19,3.0,1.8,0805,True
C6,Capacitor,100nF,28,19,3.0,1.8,0805,True
C7,Capacitor,1uF,-22,-19,3.0,1.8,0805,True
C8,Capacitor,10nF,-12,-19,3.0,1.8,0805,True
C9,Capacitor,4.7uF,-2,-19,3.0,1.8,0805,True
C10,Capacitor,10uF,8,-19,3.0,1.8,0805,True
C11,Capacitor,100nF,18,-19,3.0,1.8,0805,True
S1,Sensor,PT1000 Module,-57,25,9,8,4-pin Sensor Module,True
S2,Sensor,Strain Bridge Module,-57,8,9,8,4-pin Sensor Module,True
S3,Sensor,Photodiode Module,-57,-9,9,8,4-pin Sensor Module,True
S4,Sensor,Hall Current Module,-57,-26,9,8,4-pin Sensor Module,True
J1,Connector,Protected 5V In,57,33,9,6,Header,True
J2,Connector,Digital Interface,56,15,9,6,Header,True
RV1,Trimmer,Gain Calibration,7,-29,6,6,Trimmer,True
LED1,LED,Status Green,59,-37,3,3,0603,True
W059,Jumper,Reference Valid,40,28,6,4,2-pin Removable,True
D1,Diode,Protection,51,29,3.6,2,SMA,True
```

## ANNEX C — Exact 187 component contact records (unassigned signal placeholders)

Source: `Data/Component_Contact_Plan.csv`. Embedded verbatim for exact task/asset naming; placeholders remain unresolved until real schematic verification.

```csv
Component,Contact,Kind,Signal,Gameplay
U1,U1.P01,IC,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
U1,U1.P02,IC,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
U1,U1.P03,IC,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
U1,U1.P04,IC,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
U1,U1.P05,IC,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
U1,U1.P06,IC,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
U1,U1.P07,IC,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
U1,U1.P08,IC,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
U1,U1.P09,IC,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
U1,U1.P10,IC,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
U1,U1.P11,IC,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
U1,U1.P12,IC,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
U1,U1.P13,IC,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
U1,U1.P14,IC,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
U1,U1.P15,IC,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
U1,U1.P16,IC,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
U1,U1.P17,IC,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
U1,U1.P18,IC,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
U1,U1.P19,IC,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
U1,U1.P20,IC,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
U2,U2.P01,IC,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
U2,U2.P02,IC,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
U2,U2.P03,IC,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
U2,U2.P04,IC,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
U2,U2.P05,IC,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
U2,U2.P06,IC,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
U2,U2.P07,IC,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
U2,U2.P08,IC,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
U3,U3.P01,IC,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
U3,U3.P02,IC,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
U3,U3.P03,IC,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
U3,U3.P04,IC,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
U3,U3.P05,IC,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
U3,U3.P06,IC,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
U3,U3.P07,IC,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
U3,U3.P08,IC,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
U4,U4.P01,IC,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
U4,U4.P02,IC,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
U4,U4.P03,IC,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
U4,U4.P04,IC,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
U4,U4.P05,IC,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
U4,U4.P06,IC,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
U4,U4.P07,IC,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
U4,U4.P08,IC,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
U5,U5.P01,IC,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
U5,U5.P02,IC,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
U5,U5.P03,IC,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
U5,U5.P04,IC,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
U5,U5.P05,IC,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
U5,U5.P06,IC,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
U5,U5.P07,IC,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
U5,U5.P08,IC,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
U6,U6.P01,IC,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
U6,U6.P02,IC,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
U6,U6.P03,IC,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
U6,U6.P04,IC,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
U6,U6.P05,IC,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
U6,U6.P06,IC,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
U6,U6.P07,IC,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
U6,U6.P08,IC,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
U7,U7.P01,IC,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
U7,U7.P02,IC,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
U7,U7.P03,IC,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
U7,U7.P04,IC,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
U7,U7.P05,IC,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
U7,U7.P06,IC,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
U7,U7.P07,IC,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
U7,U7.P08,IC,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
U7,U7.P09,IC,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
U7,U7.P10,IC,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
U7,U7.P11,IC,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
U7,U7.P12,IC,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
U7,U7.P13,IC,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
U7,U7.P14,IC,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
U7,U7.P15,IC,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
U7,U7.P16,IC,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
U7,U7.P17,IC,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
U7,U7.P18,IC,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
U7,U7.P19,IC,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
U7,U7.P20,IC,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
U7,U7.P21,IC,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
U7,U7.P22,IC,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
U7,U7.P23,IC,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
U7,U7.P24,IC,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
U7,U7.P25,IC,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
U7,U7.P26,IC,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
U7,U7.P27,IC,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
U7,U7.P28,IC,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
U7,U7.P29,IC,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
U7,U7.P30,IC,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
U7,U7.P31,IC,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
U7,U7.P32,IC,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
U8,U8.P01,IC,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
U8,U8.P02,IC,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
U8,U8.P03,IC,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
U8,U8.P04,IC,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
U8,U8.P05,IC,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
U8,U8.P06,IC,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
U8,U8.P07,IC,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
U8,U8.P08,IC,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
U9,U9.P01,IC,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
U9,U9.P02,IC,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
U9,U9.P03,IC,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
U9,U9.P04,IC,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
U9,U9.P05,IC,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
U9,U9.P06,IC,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
U9,U9.P07,IC,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
U9,U9.P08,IC,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
R1,R1.P01,Resistor,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
R1,R1.P02,Resistor,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
R2,R2.P01,Resistor,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
R2,R2.P02,Resistor,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
R3,R3.P01,Resistor,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
R3,R3.P02,Resistor,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
R4,R4.P01,Resistor,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
R4,R4.P02,Resistor,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
R5,R5.P01,Resistor,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
R5,R5.P02,Resistor,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
R6,R6.P01,Resistor,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
R6,R6.P02,Resistor,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
R7,R7.P01,Resistor,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
R7,R7.P02,Resistor,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
R8,R8.P01,Resistor,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
R8,R8.P02,Resistor,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
R9,R9.P01,Resistor,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
R9,R9.P02,Resistor,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
R10,R10.P01,Resistor,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
R10,R10.P02,Resistor,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
R11,R11.P01,Resistor,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
R11,R11.P02,Resistor,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
R12,R12.P01,Resistor,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
R12,R12.P02,Resistor,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
R13,R13.P01,Resistor,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
R13,R13.P02,Resistor,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
R14,R14.P01,Resistor,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
R14,R14.P02,Resistor,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
C1,C1.P01,Capacitor,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
C1,C1.P02,Capacitor,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
C2,C2.P01,Capacitor,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
C2,C2.P02,Capacitor,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
C3,C3.P01,Capacitor,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
C3,C3.P02,Capacitor,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
C4,C4.P01,Capacitor,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
C4,C4.P02,Capacitor,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
C5,C5.P01,Capacitor,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
C5,C5.P02,Capacitor,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
C6,C6.P01,Capacitor,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
C6,C6.P02,Capacitor,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
C7,C7.P01,Capacitor,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
C7,C7.P02,Capacitor,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
C8,C8.P01,Capacitor,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
C8,C8.P02,Capacitor,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
C9,C9.P01,Capacitor,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
C9,C9.P02,Capacitor,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
C10,C10.P01,Capacitor,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
C10,C10.P02,Capacitor,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
C11,C11.P01,Capacitor,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
C11,C11.P02,Capacitor,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
S1,S1.P01,Sensor,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
S1,S1.P02,Sensor,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
S1,S1.P03,Sensor,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
S1,S1.P04,Sensor,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
S2,S2.P01,Sensor,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
S2,S2.P02,Sensor,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
S2,S2.P03,Sensor,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
S2,S2.P04,Sensor,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
S3,S3.P01,Sensor,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
S3,S3.P02,Sensor,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
S3,S3.P03,Sensor,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
S3,S3.P04,Sensor,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
S4,S4.P01,Sensor,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
S4,S4.P02,Sensor,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
S4,S4.P03,Sensor,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
S4,S4.P04,Sensor,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
J1,J1.P01,Connector,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
J1,J1.P02,Connector,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
J2,J2.P01,Connector,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
J2,J2.P02,Connector,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
RV1,RV1.P01,Trimmer,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
RV1,RV1.P02,Trimmer,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
RV1,RV1.P03,Trimmer,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
LED1,LED1.P01,LED,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
LED1,LED1.P02,LED,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
W059,W059.P01,Jumper,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
W059,W059.P02,Jumper,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
D1,D1.P01,Diode,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
D1,D1.P02,Diode,UNASSIGNED_PENDING_SCHEMATIC,no automatic connectivity
```

## ANNEX D — Exact 58 conceptual routing tasks

Source: `Data/Copper_Routing_Plan_W001_W058.csv`. Embedded verbatim for exact task/asset naming; placeholders remain unresolved until real schematic verification.

```csv
WireID,Group,TaskIndex,SourcePad,DestinationPad,StartComplete,RequiresRealCircuitValidation,Status
W001,Control power and reference,1,SMX4_NET_001_A,SMX4_NET_001_B,False,True,CONCEPT_ROUTE_NOT_A_VERIFIED_PIN_NETLIST
W002,Control power and reference,2,SMX4_NET_002_A,SMX4_NET_002_B,False,True,CONCEPT_ROUTE_NOT_A_VERIFIED_PIN_NETLIST
W003,Control power and reference,3,SMX4_NET_003_A,SMX4_NET_003_B,False,True,CONCEPT_ROUTE_NOT_A_VERIFIED_PIN_NETLIST
W004,Control power and reference,4,SMX4_NET_004_A,SMX4_NET_004_B,False,True,CONCEPT_ROUTE_NOT_A_VERIFIED_PIN_NETLIST
W005,Control power and reference,5,SMX4_NET_005_A,SMX4_NET_005_B,False,True,CONCEPT_ROUTE_NOT_A_VERIFIED_PIN_NETLIST
W006,Control power and reference,6,SMX4_NET_006_A,SMX4_NET_006_B,False,True,CONCEPT_ROUTE_NOT_A_VERIFIED_PIN_NETLIST
W007,Control power and reference,7,SMX4_NET_007_A,SMX4_NET_007_B,False,True,CONCEPT_ROUTE_NOT_A_VERIFIED_PIN_NETLIST
W008,Control power and reference,8,SMX4_NET_008_A,SMX4_NET_008_B,False,True,CONCEPT_ROUTE_NOT_A_VERIFIED_PIN_NETLIST
W009,Control power and reference,9,SMX4_NET_009_A,SMX4_NET_009_B,False,True,CONCEPT_ROUTE_NOT_A_VERIFIED_PIN_NETLIST
W010,Control power and reference,10,SMX4_NET_010_A,SMX4_NET_010_B,False,True,CONCEPT_ROUTE_NOT_A_VERIFIED_PIN_NETLIST
W011,Control power and reference,11,SMX4_NET_011_A,SMX4_NET_011_B,False,True,CONCEPT_ROUTE_NOT_A_VERIFIED_PIN_NETLIST
W012,Control power and reference,12,SMX4_NET_012_A,SMX4_NET_012_B,False,True,CONCEPT_ROUTE_NOT_A_VERIFIED_PIN_NETLIST
W013,Temperature and bridge AFE,1,SMX4_NET_013_A,SMX4_NET_013_B,False,True,CONCEPT_ROUTE_NOT_A_VERIFIED_PIN_NETLIST
W014,Temperature and bridge AFE,2,SMX4_NET_014_A,SMX4_NET_014_B,False,True,CONCEPT_ROUTE_NOT_A_VERIFIED_PIN_NETLIST
W015,Temperature and bridge AFE,3,SMX4_NET_015_A,SMX4_NET_015_B,False,True,CONCEPT_ROUTE_NOT_A_VERIFIED_PIN_NETLIST
W016,Temperature and bridge AFE,4,SMX4_NET_016_A,SMX4_NET_016_B,False,True,CONCEPT_ROUTE_NOT_A_VERIFIED_PIN_NETLIST
W017,Temperature and bridge AFE,5,SMX4_NET_017_A,SMX4_NET_017_B,False,True,CONCEPT_ROUTE_NOT_A_VERIFIED_PIN_NETLIST
W018,Temperature and bridge AFE,6,SMX4_NET_018_A,SMX4_NET_018_B,False,True,CONCEPT_ROUTE_NOT_A_VERIFIED_PIN_NETLIST
W019,Temperature and bridge AFE,7,SMX4_NET_019_A,SMX4_NET_019_B,False,True,CONCEPT_ROUTE_NOT_A_VERIFIED_PIN_NETLIST
W020,Temperature and bridge AFE,8,SMX4_NET_020_A,SMX4_NET_020_B,False,True,CONCEPT_ROUTE_NOT_A_VERIFIED_PIN_NETLIST
W021,Temperature and bridge AFE,9,SMX4_NET_021_A,SMX4_NET_021_B,False,True,CONCEPT_ROUTE_NOT_A_VERIFIED_PIN_NETLIST
W022,Temperature and bridge AFE,10,SMX4_NET_022_A,SMX4_NET_022_B,False,True,CONCEPT_ROUTE_NOT_A_VERIFIED_PIN_NETLIST
W023,Temperature and bridge AFE,11,SMX4_NET_023_A,SMX4_NET_023_B,False,True,CONCEPT_ROUTE_NOT_A_VERIFIED_PIN_NETLIST
W024,Temperature and bridge AFE,12,SMX4_NET_024_A,SMX4_NET_024_B,False,True,CONCEPT_ROUTE_NOT_A_VERIFIED_PIN_NETLIST
W025,Temperature and bridge AFE,13,SMX4_NET_025_A,SMX4_NET_025_B,False,True,CONCEPT_ROUTE_NOT_A_VERIFIED_PIN_NETLIST
W026,Temperature and bridge AFE,14,SMX4_NET_026_A,SMX4_NET_026_B,False,True,CONCEPT_ROUTE_NOT_A_VERIFIED_PIN_NETLIST
W027,Photodiode and current AFE,1,SMX4_NET_027_A,SMX4_NET_027_B,False,True,CONCEPT_ROUTE_NOT_A_VERIFIED_PIN_NETLIST
W028,Photodiode and current AFE,2,SMX4_NET_028_A,SMX4_NET_028_B,False,True,CONCEPT_ROUTE_NOT_A_VERIFIED_PIN_NETLIST
W029,Photodiode and current AFE,3,SMX4_NET_029_A,SMX4_NET_029_B,False,True,CONCEPT_ROUTE_NOT_A_VERIFIED_PIN_NETLIST
W030,Photodiode and current AFE,4,SMX4_NET_030_A,SMX4_NET_030_B,False,True,CONCEPT_ROUTE_NOT_A_VERIFIED_PIN_NETLIST
W031,Photodiode and current AFE,5,SMX4_NET_031_A,SMX4_NET_031_B,False,True,CONCEPT_ROUTE_NOT_A_VERIFIED_PIN_NETLIST
W032,Photodiode and current AFE,6,SMX4_NET_032_A,SMX4_NET_032_B,False,True,CONCEPT_ROUTE_NOT_A_VERIFIED_PIN_NETLIST
W033,Photodiode and current AFE,7,SMX4_NET_033_A,SMX4_NET_033_B,False,True,CONCEPT_ROUTE_NOT_A_VERIFIED_PIN_NETLIST
W034,Photodiode and current AFE,8,SMX4_NET_034_A,SMX4_NET_034_B,False,True,CONCEPT_ROUTE_NOT_A_VERIFIED_PIN_NETLIST
W035,Photodiode and current AFE,9,SMX4_NET_035_A,SMX4_NET_035_B,False,True,CONCEPT_ROUTE_NOT_A_VERIFIED_PIN_NETLIST
W036,Photodiode and current AFE,10,SMX4_NET_036_A,SMX4_NET_036_B,False,True,CONCEPT_ROUTE_NOT_A_VERIFIED_PIN_NETLIST
W037,Photodiode and current AFE,11,SMX4_NET_037_A,SMX4_NET_037_B,False,True,CONCEPT_ROUTE_NOT_A_VERIFIED_PIN_NETLIST
W038,Photodiode and current AFE,12,SMX4_NET_038_A,SMX4_NET_038_B,False,True,CONCEPT_ROUTE_NOT_A_VERIFIED_PIN_NETLIST
W039,ADC sampling and logic,1,SMX4_NET_039_A,SMX4_NET_039_B,False,True,CONCEPT_ROUTE_NOT_A_VERIFIED_PIN_NETLIST
W040,ADC sampling and logic,2,SMX4_NET_040_A,SMX4_NET_040_B,False,True,CONCEPT_ROUTE_NOT_A_VERIFIED_PIN_NETLIST
W041,ADC sampling and logic,3,SMX4_NET_041_A,SMX4_NET_041_B,False,True,CONCEPT_ROUTE_NOT_A_VERIFIED_PIN_NETLIST
W042,ADC sampling and logic,4,SMX4_NET_042_A,SMX4_NET_042_B,False,True,CONCEPT_ROUTE_NOT_A_VERIFIED_PIN_NETLIST
W043,ADC sampling and logic,5,SMX4_NET_043_A,SMX4_NET_043_B,False,True,CONCEPT_ROUTE_NOT_A_VERIFIED_PIN_NETLIST
W044,ADC sampling and logic,6,SMX4_NET_044_A,SMX4_NET_044_B,False,True,CONCEPT_ROUTE_NOT_A_VERIFIED_PIN_NETLIST
W045,ADC sampling and logic,7,SMX4_NET_045_A,SMX4_NET_045_B,False,True,CONCEPT_ROUTE_NOT_A_VERIFIED_PIN_NETLIST
W046,ADC sampling and logic,8,SMX4_NET_046_A,SMX4_NET_046_B,False,True,CONCEPT_ROUTE_NOT_A_VERIFIED_PIN_NETLIST
W047,ADC sampling and logic,9,SMX4_NET_047_A,SMX4_NET_047_B,False,True,CONCEPT_ROUTE_NOT_A_VERIFIED_PIN_NETLIST
W048,ADC sampling and logic,10,SMX4_NET_048_A,SMX4_NET_048_B,False,True,CONCEPT_ROUTE_NOT_A_VERIFIED_PIN_NETLIST
W049,Diagnostics and protection,1,SMX4_NET_049_A,SMX4_NET_049_B,False,True,CONCEPT_ROUTE_NOT_A_VERIFIED_PIN_NETLIST
W050,Diagnostics and protection,2,SMX4_NET_050_A,SMX4_NET_050_B,False,True,CONCEPT_ROUTE_NOT_A_VERIFIED_PIN_NETLIST
W051,Diagnostics and protection,3,SMX4_NET_051_A,SMX4_NET_051_B,False,True,CONCEPT_ROUTE_NOT_A_VERIFIED_PIN_NETLIST
W052,Diagnostics and protection,4,SMX4_NET_052_A,SMX4_NET_052_B,False,True,CONCEPT_ROUTE_NOT_A_VERIFIED_PIN_NETLIST
W053,Diagnostics and protection,5,SMX4_NET_053_A,SMX4_NET_053_B,False,True,CONCEPT_ROUTE_NOT_A_VERIFIED_PIN_NETLIST
W054,Diagnostics and protection,6,SMX4_NET_054_A,SMX4_NET_054_B,False,True,CONCEPT_ROUTE_NOT_A_VERIFIED_PIN_NETLIST
W055,Diagnostics and protection,7,SMX4_NET_055_A,SMX4_NET_055_B,False,True,CONCEPT_ROUTE_NOT_A_VERIFIED_PIN_NETLIST
W056,Diagnostics and protection,8,SMX4_NET_056_A,SMX4_NET_056_B,False,True,CONCEPT_ROUTE_NOT_A_VERIFIED_PIN_NETLIST
W057,Diagnostics and protection,9,SMX4_NET_057_A,SMX4_NET_057_B,False,True,CONCEPT_ROUTE_NOT_A_VERIFIED_PIN_NETLIST
W058,Diagnostics and protection,10,SMX4_NET_058_A,SMX4_NET_058_B,False,True,CONCEPT_ROUTE_NOT_A_VERIFIED_PIN_NETLIST
```

## ANNEX E — Exact 12 via records

Source: `Data/Required_Vias_12.csv`. Embedded verbatim for exact task/asset naming; placeholders remain unresolved until real schematic verification.

```csv
ViaID,LayerFrom,LayerTo,StartComplete,ElectricalNet
V01,F.Cu,B.Cu,False,TO_BE_VALIDATED_WITH_REAL_SCHEMATIC
V02,F.Cu,B.Cu,False,TO_BE_VALIDATED_WITH_REAL_SCHEMATIC
V03,F.Cu,B.Cu,False,TO_BE_VALIDATED_WITH_REAL_SCHEMATIC
V04,F.Cu,B.Cu,False,TO_BE_VALIDATED_WITH_REAL_SCHEMATIC
V05,F.Cu,B.Cu,False,TO_BE_VALIDATED_WITH_REAL_SCHEMATIC
V06,F.Cu,B.Cu,False,TO_BE_VALIDATED_WITH_REAL_SCHEMATIC
V07,F.Cu,B.Cu,False,TO_BE_VALIDATED_WITH_REAL_SCHEMATIC
V08,F.Cu,B.Cu,False,TO_BE_VALIDATED_WITH_REAL_SCHEMATIC
V09,F.Cu,B.Cu,False,TO_BE_VALIDATED_WITH_REAL_SCHEMATIC
V10,F.Cu,B.Cu,False,TO_BE_VALIDATED_WITH_REAL_SCHEMATIC
V11,F.Cu,B.Cu,False,TO_BE_VALIDATED_WITH_REAL_SCHEMATIC
V12,F.Cu,B.Cu,False,TO_BE_VALIDATED_WITH_REAL_SCHEMATIC
```

## ANNEX F — Exact 3 source fault records

Source: `Data/Faults_3.csv`. Embedded verbatim for exact task/asset naming; placeholders remain unresolved until real schematic verification.

```csv
ID,FaultName,Repair,InitialState
F1,Reference voltage drift,requires qualified touch tool + actual electrical/simulation state,not active
F2,Open strain bridge via,requires qualified touch tool + actual electrical/simulation state,not active
F3,Photodiode transimpedance amplifier saturation,requires qualified touch tool + actual electrical/simulation state,not active
```

## ANNEX G — Exact 6 source objective records

Source: `Data/Objectives_6.csv`. Embedded verbatim for exact task/asset naming; placeholders remain unresolved until real schematic verification.

```csv
ID,Description,Certify,DefaultComplete
01,Install 44 PCB components,Only from real Project Spark simulation,False
02,Route 58 copper tasks + 12 vias and W059,Only from real Project Spark simulation,False
03,Commission 2.500 V reference and calibrate ADC,Only from real Project Spark simulation,False
04,Four-channel live sensor acquisition,Only from real Project Spark simulation,False
05,Recover reference drift bridge via optical saturation,Only from real Project Spark simulation,False
06,Disconnect reconnect recalibrate and requalify W059,Only from real Project Spark simulation,False
```

## ANNEX H — Exact W059 source JSON

```json
{
  "wire_id": "W059",
  "source": "L96_ReferenceSupervisor_U8.TRUSTED_REF_VALID_OUT",
  "target": "L96_SafetyController_U9.TRUSTED_REF_VALID_IN",
  "initially_connected": false,
  "meaning": "digital reference-valid monitor, NOT a 2.500V supply",
  "step_sequence": [
    "initial connection and qualification",
    "supervised disconnection",
    "invalidate certification / protect output",
    "physical reconnection",
    "fresh calibration and measurements",
    "explicit reset",
    "full new qualification"
  ],
  "requires_real_solver": true
}
```

## ANNEX I — Exact source summary

```json
{
  "model_count": 62,
  "fbx_count": 63,
  "glb_count": 63,
  "component_count": 44,
  "routes": 58,
  "vias": 12,
  "board_size_mm": [
    140,
    95,
    1.6
  ],
  "coordinate_system": "Unity X-right Y-up Z-board, units meters",
  "board_only": true,
  "environment_models": 0,
  "total_triangles": 10716
}
```

## ANNEX J — Existing supplied Unity source files and limits

- `Unity/SparkLevel96BoardOnlyArtBuilder.cs`: creates the bare board and a visual-only completed reference; its menu paths are Level 96/Board Only.
- `Unity/SparkLevel96PBRMaterialInstaller.cs`: creates native materials (URP Lit, HDRP Lit or Standard fallback) and expects source textures; glTF metallic/roughness channel conversion and model slot remapping still require validation.
- `Unity/SparkLevel96VirtualRepairTarget.cs`: visual-only effect hook; `PlayRepairVisualOnly` sets a cosmetic `visuallyRepaired` Boolean and **does not mutate electrical authority**. Never use that Boolean for pass/fail.

The supplied scripts are art/interaction helpers. They are not substitutes for the full board, electrical, sampling and objective runtime systems described above.

## ANNEX K — Scope exclusions and source-integrity notes

- No new physical multimeter, oscilloscope, logic analyzer, solder gun, room, chair, dustbin or laboratory furniture FBX.
- No invented model filenames or claimed exact pin-level nets. If additions are required, record them as **new implementation proposals**, not source contents.
- No hidden auto-connected player route. No achievement from completed reference art.
- No W059 conflation with 2.500 V supply; it is a trusted logic feedback path.
- No artificial zero readings on lost sensor reference; mark data invalid.
- No board transform scale leaking into physical/thermal/electrical simulation.
- No fixing existing Project Spark data implicitly without a change log and regression tests.

**Requested final result:** a visually polished, genuinely playable Level 96 SMX-4 Sensor Matrix, with realistic PCB interactions, virtual-touch inspection and repair, credible analog acquisition, three faults, W059 safety recovery, and authoritative **6/6** objective certification—only once validated by actual Unity Play Mode testing.

