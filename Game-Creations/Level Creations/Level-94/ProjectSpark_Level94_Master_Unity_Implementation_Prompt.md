# PROJECT SPARK — LEVEL 94 MASTER UNITY IMPLEMENTATION PROMPT

## LEVEL 94 — THE BOARD FOUNDRY

### Advanced AAA Two-Layer PCB Controller Assembly, Component Footprints, Player-Routed Copper, Real Via Topology, Protected 5 V/3.3 V Electronics, Instrument-Based Validation, Three Repair Faults, and Critical W035 Current-Feedback Recovery

**ROLE:** You are my senior Unity C# architect, Project Spark electrical systems integration engineer, educational PCB simulation engineer, electronic assembly and inspection engineer, digital/analog instrumentation engineer, AAA environment technical artist, Unity Editor tooling engineer, safety/PLC engineer, and QA automation specialist.

**PRIMARY IMPLEMENTATION REQUEST:** In my *existing* Project Spark Unity game, **IMPLEMENT A COMPLETE, ACTUALLY PLAYABLE, REBUILDABLE LEVEL 94 — THE BOARD FOUNDRY**, using the original `ProjectSpark_Level94_BoardFoundry_AAA_Complete.zip` assets as the unmodified art/source baseline. The player must inspect a two-layer diagnostic PCB, place the correct 27 components, complete 34 distinct missing copper-route tasks and six required vias, commission realistic educational electronics, identify and fix three circuit faults, disconnect and restore W035 and earn **six verifiable objectives (6/6)** from the **real `LevelGamePlayManager`**. **Do not substitute an image for the delivered FBXs, do not create a parallel fake game or award scripted completion flags.** Preserve all Levels 1–93, the shop, shelf, throw/recycle system, existing large futuristic game room and current electrical interaction UX.

**SOURCE PROVENANCE & TRUST:** This prompt is grounded in the supplied Level 94 complete ZIP and its CSV/JSON/Unity source, not hypothetical board counts. The package is an **art and planning kit**, not a finished circuit or compiled level. Its offline report says **41/41 structural checks passed**, but Unity compilation, actual electrical integration and Play Mode are **not** verified. The *new* Unity classes and architecture below are **implementation targets**, not existing API declarations: inspect real Project Spark code before coding or using names.

**EXPECTED RESULT:** an accessible and polished AAA-inspired game-board level containing real interactive part placement, copper graph editing, continuity/short checks, measurement tools, reliable safety inhibition, procedural fault recovery and hard evidence-based certification. Deliver compile-tested C# source, prefabs, editor builder, wiring/net data, board presentation and test reports. Never claim compilation or testing without running it in the actual project.

---

# 01. IMPLEMENTATION MANDATE — ONE GENUINE BOARD, ONE ELECTRICAL AUTHORITY

Implement the entire sequence **inspect → install 27 parts → connect W001–W034 and V01–V06 → validate DRC + netlist → install/qualify W035 for normal operation → protected 5 V and 3.3 V commissioning → test MCU 2 Hz, I²C U3 and Q1 load/current → inject and fix F01–F03 → fresh post-fault test → supervised W035 disconnection → fail-safe load inhibit → physical reconnection, new measurement, explicit reset → fresh complete production test → `LevelGamePlayManager` 6/6**.

Every observable outcome must be derived from authoritative circuit/component data. Frontend holograms, shader emissions, model transforms, checkbox states, simulated rack lights and button presses may visualize a result but **may not create one**. All initial objective records must start incomplete, and each certified objective needs tamper-resistant evidence from the owning simulation.

# 02. SOURCE PACKAGE — INSPECT EXACT FILES BEFORE EDITING

Use `ProjectSpark_Level94_BoardFoundry_AAA_Complete.zip` directly. Inspect `README_FIRST.md`, `Docs/Level94_Gameplay_Integration_Contract.md`, the 15 Data files, `Materials/PBR_Material_Bindings.json`, model/material manifests, `Reports/Offline_Structural_Validation.json`, all six Unity `.cs` source files, two `Shaders` variants and all numbered FBXs.

Do **not** reconstruct filenames or endpoint IDs from screenshots or generated prose; parse the CSV/JSON. Do not import the assembled FBX *and* all 48 numbered FBXs into the same playable layout (duplicate geometry). The assembled model is for visual inspection/showcase only. If any source records are inconsistent, retain the original as provenance, emit a diagnostic, and only introduce an approved mapping in a separate reconciled asset rather than silently changing source data.

# 03. SOURCE-VERIFIED BASELINE — COUNTS AND CURRENT LIMITS

Source-verified: **48 individual binary FBX 7400 models, 48 matching GLBs**, one assembled FBX and one assembled GLB; **159 mesh parts / 12,312 mesh triangles** across modular files; **27 installable components; 37 named pivots; 127 logical pad positions; 34 missing player copper routes (W001–W034); six missing vias (V01–V06); one separate removable safety jumper (W035); 23 prefabricated net definitions; 19 electrical net-domain records; six objective definitions; three faults; 56 *not-yet-executed* Unity acceptance-test definitions**.

PBR art includes **20 material families, 120 PNG maps at 1024×1024, six maps per family**, two hologram shaders and six C# helper/data scripts. The ZIP's `Offline_Structural_Validation.json` reports **41 passed, 0 failed** structural checks, but `unity_playmode_tested=false` and `electrical_solver_connected=false`. No functional pads, solder joints, routed live copper, actual oscilloscope or automatic gameplay completion are present yet.

# 04. READ ACTUAL PROJECT SPARK C# APIS — NO FABRICATED METHODS

Before modifying gameplay, search project assemblies/source for `SparkElectricalSolver`, `SparkCircuitSystem`, `SparkTerminal`, the player-wire placement/removal system, `SparkObjectShelf`, existing shop item registration, `SparkThrowSystem`, `SparkLevelEvaluator` if present and `LevelGamePlayManager`. Record each actual namespace, public method signature, lifecycle event, serialization path, ownership rule and circuit graph mutation hook.

Create a short `L94_API_Compatibility.md` referencing exact source locations; implement adapters only after the signatures are known. Reuse existing systems rather than a cloned circuit solver. Any net-graph API missing from the project must be explicitly bridged with a bounded, testable adapter; do not claim a guessed class is real. Respect that the user may have other important scripts and prefabs modified in their local Unity project.

# 05. NON-NEGOTIABLE GAMEPLAY SAFETY AND ANTI-CHEAT RULES

The board uses an **educational protected virtual 5 V input** and a simulated regulated 3.3 V rail, never real mains or real soldering/hot-work instructions. External player probes must not cause real-world hazardous guidance. Block power when net shorts or safety faults are unresolved; model the power supply's current limit, not infinite current.

Never preconnect player trace routes, auto-solder components simply by displaying them, treat pads drawn on FBX as live `SparkTerminal`s, bypass safety on reset, or infer successful inspection from tool VFX. The actual `LevelGamePlayManager` is the sole authority that grants 6/6, and certification needs unique evidence IDs, timestamps/version counters and current integrity states.

# 06. LEVEL 94 IDENTITY, STORY, INITIAL SCENE AND 0/6

**Level number:** 94. **Name:** The Board Foundry. **Module:** first level of the dedicated PCB & Motherboard Engineering Mastery series. The board is a **Board Diagnostic Controller** intended for later embedded-control board commissioning. The player enters the existing clean futuristic room with a single central board fixture, a combined instrument rack and one compact component/tool tray. Avoid unrelated scene clutter.

Start with the educational schematic unlocked but uninspected, 27 part slots empty, W001–W034 routes absent, all V01–V06 vias absent, W035 service jumper absent, the power source disabled and no objective certified. Visible target-arrangement art from the sample showcase must be hidden/deactivated in active play and replaced by empty footprints until player action qualifies real components.

# 07. BOARD PHYSICAL UNITS VS 14× VISUAL PRESENTATION

Source JSON: **100 mm × 70 mm × 1.6 mm** physical PCB, two layers `F.Cu` and `B.Cu`, input **5 V**, logic **3.3 V**, diagnostic **2 Hz**, `ArtScaleMultiplier=14.0`. Store physical dimensions, trace widths, via drill/pad sizes and electrical simulation in **mm and calibrated SI units**. Keep the original art at a shared world origin and `14×` preview/presentation size only for comfortable table interaction.

Create a single explicit `BoardToWorld` transform that converts source `(Xmm,Zmm)` to visual world coordinates once; do not multiply by 14 in both importer and prefab. Test component footprint snapping and pad positions against the 127 source `WorldX/WorldY/WorldZ` values without changing physical capacitance, voltage, resistance or delays.

# 08. PROJECT SPARK ROOM TABLE INTEGRATION

Mount the main PCB fixture on the existing large futuristic room table, allowing unobstructed overhead and close inspection. Use the compatible game-room art root, a dedicated camera focus/framing system and an interaction-safe layer. Keep one chair, dustbin and the preexisting sparse decor; **do not spawn duplicate rooms or dozens of furniture props**.

The room's *preview mode* may clone only MeshFilter/MeshRenderer, static display transforms and safe visual materials. It must never clone live `SparkTerminal`, circuit solver, objective tracker, shop or interaction components. When entering actual Level 94, instantiate the unique gameplay layout with only one board graph and one active objective authority.

# 09. SCENE HIERARCHY, MODEL IMPORT AND REBUILD IDEMPOTENCE

Recommended visual hierarchy: `Level_94_Layout / L94_BOARD_FOUNDRY_VISUALS / {BenchAndFixture, BoardAndPads, ComponentPreviews, Instruments}`. Recommended functional hierarchy: `Level_94_Layout / L94_RUNTIME / {PCBState, PadTerminals, ComponentInstances, Routes, ViaEdges, Instruments, Safety, Objectives, UI}`. Runtime components must not be parented to a replaceable pure-art FBX where regeneration would delete state.

Builder must be idempotent: find stable generated roots, reuse or replace only Level 94-owned children, maintain serialized references and protect user-authored placements and other level assets. Set model root at `(0,0,0)`, identity rotation and original scale as source notes specify. Record model import scale conversion explicitly and verify assembled world bounds.

# 10. SOURCE MODELS — EXACT NAMES AND CLASSIFICATION

Import exactly the 48 `Data/Model_Manifest_48.csv` rows under `Models/`. Key art includes `L94_FR4BoardSubstrate`, top/bottom soldermask, separate copper pad sets, `L94_SilkscreenAndTestFiducials`, `L94_W035DetachableFeedbackJumper`, U1–U4, Q1, D1, LED1–3, R1–R8, C1–C7, SW1, J1, J2, bench instruments and `L94_PCBAssemblySCADAPanel`.

Do not substitute guessed components or generate a prettier but incompatible board. Each numbered model must retain its reference ID and ownership: a user-selected resistor R2 cannot accidentally instantiate R1 or be assigned to a visually similar footprint.

# 11. PBR MATERIALS — 20 FAMILIES, SIX MAPS EACH

Generate native `.mat` objects from `Materials/PBR_Material_Bindings.json` and `Materials/Material_Reference_Map.csv`. Use authored names/material slots, the supplied 20 shared material families, and source textures; never bind materials by renderer array index alone without validating source slot name. Keep FR-4 substrate matte, soldermask smooth but not mirror-like, copper/gold pads metallic, IC packages dark and readable, glass display surfaces controlled and emitter cores emissive.

The PBR source provides for each family `BaseColor`, `Normal`, `MetalRoughAO`, `UnityMaskMap`, `Occlusion`, and `Emission` maps. Use correct sRGB vs linear import flags, normal map importer types and smoothness channel. Do not create opaque normal-map-looking albedo surfaces or double-apply AO.

# 12. RENDER PIPELINES, HOLOGRAM SHADERS AND QA

Existing art editor supports native Lit materials for URP, HDRP and Built-in. Use `Shaders/PS_Level94HologramURP.shader` when compatible with URP, `PS_Level94HologramBuiltin.shader` for Built-in; create a documented native HDRP/Unlit fallback where needed. Check compile/errors and frame debugger in **the actual target Unity render pipeline**. Do not claim the shader works just because text files exist.

Keep holograms optional and presentation-only. Use gentle blue/cyan emissive lines, warm amber fault indicators and readable PCB silkscreen. Avoid unbounded real-time lights per LED, expensive transparent overdraw on every pad, and visual bloom clipping small components.

# 13. LOD, COLLIDERS, PICKING, ACCESSIBILITY AND PERFORMANCE

Build simple correct colliders for the board, component trays, probe targets and fixture clamps. Source FBX triangle budget is small; still audit draw calls, collider scaling, transparent overlays and text readability. Use one selectable interaction surface per footprint, not a giant overlapping collider mask. Fine-pitch parts should remain targetable through zoomed interaction and keyboard/controller alternatives.

Player visual placement of a QFN must not require realistic microscopic mouse accuracy: provide snapped footprint slots, clear reference designators, rotation preview, mismatch warnings and an undo action. Establish stable framerate goals consistent with the current game project and test in its chosen URP/HDRP/Built-in configuration.

# 14. 37 NAMED PIVOTS — PRESERVE AND DRIVE THE RIGHT AXES

Source `Data/Animation_Pivots.csv` defines **37 named pivots** with `Model`, `Pivot`, `LocalHome`, `Axis`, `Kind` and description. Twenty-seven are associated with component installation; the rest support clamps, microscope, W035 and instrument display/interaction motions. Parse and validate the **exact pivot names**, axes and local homes.

Component placement animation should follow actual `Uninstalled → Positioned → VerifiedInstalled` state, not drive the electrical pin mapping. Axes may be `x`, `y`, or `z`; never rotate all pivots about a single hardcoded axis or use cosmetic transforms to imply physical electrical continuity.

# 15. COMPONENT BOM — ALL 27 START UNINSTALLED

Source `Data/Component_BOM_27.csv` is authoritative for reference designators, package kinds, values, required orientation, physical footprint sizes, nominal positions and `InitialInstalled=NO`. The actual component distribution is **U1–U4 (4 ICs), Q1 MOSFET, D1 Schottky, LED1–LED3, R1–R8, C1–C7, SW1 reset switch, J1 5 V power connector and J2 debug header**.

Preserve source values including **R1=4K7 SCL**, **R2=4K7 SDA**, **R3=100R gate series**, **R4=100K gate pull-down**, **R8=0R1 shunt**, **C1=10 µF**, **C2=22 µF**, **C3–C5=100 nF**, **C6=10 nF**, **C7=1 µF** where defined in BOM. Always source these constants from the exact data rows, not this convenient summary. Explicitly reject a wrong replacement value and wrong polarized orientation.

# 16. SHOP/SHELF REUSE, PART OWNERSHIP AND RECYCLING

Integrate existing shop registration and shelf placement semantics as actually implemented. For Level 94, make each component instance unique by component Ref (U1, R2, etc.); a stocked replacement should not silently satisfy the installed reference without a valid placement event. Use existing throw/recycle rules for rejected *disconnected* parts; refuse removal/recycling of connected elements unless the current project's safety workflow supports power isolation and graph disconnection.

Respect the game's existing purchase/economy model when present. Do not monetize components twice, delete prefab IDs, or invent `SparkObjectShelf` methods. Persist component orientation, value, installed state and replacement history.

# 17. FOOTPRINT SNAP, PIN-1, VALUE AND SOLDER JOINT VALIDATION

Create physical footprint records from `Component_Placement_27.csv` and `Component_BOM_27.csv`. On drag/selection, calculate snap target, rotation and package compatibility; show a ghost preview before committing. Only a verified install creates the appropriate **component pin ↔ board pad** graph relationships. Displaced, rotated or incorrectly valued components must not count.

Represent solder-joint state separately from “component visible”: `Open`, `Valid`, `Bridge`, `Cold/Intermittent` if modeled. The game may simplify soldering into safe virtual actions, but joint defects must be tied to real conduction edges. Source files do not supply a complete manufacturing pin/footprint schematic: produce a documented reconciliation/functional mapping for actual gameplay and flag uncertain pins instead of guessing connections.

# 18. 127 LOGICAL PADS — SOURCE GEOMETRY VS LIVE TERMINALS

Parse all **127** `Data/Logical_Pad_Locations.csv` rows. Each has `PadID`, owner component, pad number, signal name, copper layer, physical position `(Xmm,Zmm)`, preview world position, `OwnerModel`, decorative flag and logical type. `IsDecorativeMeshOnly=YES` is a warning, not a functional pad. Create stable live pad/terminal instances or a compatible pad-graph representation and map them to the actual `SparkTerminal` architecture.

Pads must be individually addressable, unique by `(Level94,PadID)`, accessible for trace routing and probes, and properly resolved across component installation state. Never assume pad mesh overlap means two electrical nets are connected; use the explicit net graph and via constraints.

# 19. FACTORY-PREFABRICATED NETS — DO NOT CHARGE THE PLAYER

Import **23 rows** from `Data/Factory_Existing_Nets.csv` as immutable premanufactured copper/internal interfaces. They may become conductive when their owning component is correctly installed and the physical path exists. Factory rows are not player route tasks, and should not be deleted to artificially increase difficulty.

**Important source overlap:** the factory table contains `LED2.C → GND` even though player trace W033 requests `LED2.C → J1.GND`, and it includes other net-level ties that may produce alternate paths for a required task. Keep this as a **source design issue to reconcile** in the generated functional netlist: even if whole-net continuity happens through another allowed route, a player routing task passes only if **its own required geometry and route record** are completed and DRC-valid. Emit an audit entry for every such redundancy; do not silently remove factory records or auto-award W033.

# 20. W001–W034 SOURCE ROUTES — ALL MISSING AT START

Import exactly **34** source tasks from `Data/Copper_Trace_Tasks_W001_W034.csv`, with `FromPad`, `ToPad`, `Net`, `SignalType`, `RequiredLayer`, `ViaID`, clearance, target width and initial state. Every row declares `InitialState=MISSING`. Assign each an independent persistent task ID and a series of player-created trace geometry segments, recorded along with current DRC status.

Completing one net graph with a shortcut is not enough: the *declared route* must be player-laid and geometrically qualified (or transparently reconcilable if the project allows equivalent alternate copper paths). Do not spawn preconnected gold wires or route splines that already create live edges. Mark a task done only after actual pad contact, legal layer usage, valid via endpoints, no illegal crossings and successful routing validation.

# 21. COPPER ROUTING INTERACTION AND UNDO/REDO

Implement top/bottom edit mode, start-pad selection, intermediate corner/path drawing, snapping to a scalable grid, endpoint validation, live clearance indicators and route preview. A new copper segment commits to the real graph only after a valid transaction; partial drawings must not become conductive. Provide edit, undo, delete and reroute actions. Use the CSV's `RequiredLayer`, `TargetWidthMM` and `RequiredClearanceMM` as exact source values.

Crossing two incompatible nets on the **same layer** is an electrical short; crossing on **different insulated layers** is not, unless a legal via/pad through-hole connects them. Multiple wires of one net may intentionally branch, but this must be modeled as a validated net connection, not inferred from screen-space overlaps. Preserve player route geometry and graph state when saving/loading.

# 22. V01–V06 — SIX REAL THROUGH-LAYER CONNECTION TASKS

Source `Required_Via_Placements_V01_V06.csv` defines six missing vias: **V01/W014 (-42,-24), V02/W018 (-20,-12), V03/W021 (0,25), V04/W025 (21,9), V05/W030 (41,-18), V06/W034 (12,-30)**, in physical mm coordinates. Each joins `F.Cu` to `B.Cu`, with source drill `0.35 mm`, pad `0.75 mm`, `MustBePlayerPlaced=YES` and `InitialState=MISSING`.

A via is a typed physical-through-layer graph vertex, not just a bright circular marker. A trace requiring a via is incomplete until the exact valid via is placed and connected; deleting it breaks that route. V05 is also the site of one of the three later repair faults, so it must support actual physical interruption and reconstruction.

# 23. DESIGN-RULE CHECKING (DRC) AND ELECTRICAL RULE CHECKING (ERC)

Build DRC checks for 2-layer clearance, minimum track width, forbidden same-layer shorting, copper-to-pad ownership, invalid via placement, board-edge violations, crossing semantics, unconnected stubs and source-specified requirements. Use efficient geometry tests in **physical mm**, not 14× world-scale units.

ERC must report open required nets, missing/invalid components, wrong polarity, floating control inputs, supply-ground shorts, invalid I²C pull-ups, undriven MOSFET gates, missing measurement returns and W035 unsafe conditions. DRC + ERC findings must be actionable in SCADA and on the board, and no certification may use “no known errors” when a required validation has not actually run.

# 24. TRACE GRAPH RECONCILIATION — COMPONENTS, PADS, COPPER, VIAS

Define an authoritative circuit graph with distinct categories: (a) permanently fabricated copper edges, (b) player trace segments, (c) player vias, (d) component internal/terminal connectivity, (e) detachable W035 service jumper, (f) temporary test probes (high impedance). Avoid treating every equal textual net label as physically connected before the player has laid required copper.

Cross-check the 34 row endpoints against 127 pads. Treat source `Net` values as intended design constraints, **not instant conductive global buses**; otherwise every `+3V3` or `GND` edge could be auto-connected and bypass gameplay. Commit graph mutations transactionally and invalidate dependent sensor samples/objective evidence whenever an affected edge changes.

# 25. W035 — REAL DETACHABLE LOAD-HEALTH JUMPER

Use `Data/Critical_W035_Feedback.csv` exactly: **W035**, `DetachableServiceJumper`, source **U4 / CURRENT_OK_OUT**, through **J3.A → J3.B**, destination **U1 / CURRENT_OK_IN**, visual model `L94_W035DetachableFeedbackJumper`, initial **DISCONNECTED**, signal `TRUSTED_CURRENT_OK`, safe state `LOAD_GATE_DISABLED`.

Treat W035 as the only detachable feedback-link object with its own physical service/jumper contacts and persisted edge. In normal commissioning (Objectives 1–5), allow player installation/qualification of W035 before enabling Q1. For Objective 6, require a new **verified disconnect event after O1–O5 certification**; earlier initial absence must not count. On removal, mark sample invalid, latch the error and inhibit controlled load. A cosmetic jumper animation cannot satisfy the fault test. Reinstall/repair, obtain fresh qualified current sample, explicitly reset, and rerun a complete board qualification.

# 26. PROTECTED CONTROL POWER DOMAIN

Model the bench PSU as a virtual, regulated, **5.0 V** supply with power-enable state, current limit, output resistance/transient response and measurement terminals. Do not instruct players to attach real mains circuitry. The protected source through J1 and D1 creates `VIN_RAW`/`+5V_PROT` as graph-dependent rails. Shorts trip a meaningful simulated overcurrent/protection state rather than producing infinite current or negative voltages.

Real power/ground routing must be established before component simulation can run. Distinguish requested supply state from actual voltage/current at each IC. The power LED only illuminates from computed electrical conditions through its resistor path, not an objective checkbox.

# 27. 3.3 V LDO AND DECOUPLING RESPONSE

Model U2 LDO with its input/protected rail condition, regulation threshold, bounded output current, transient response, finite dropout/headroom and stability-related decoupling checks. Simulated C1=10 µF input, C2=22 µF output and C3–C5=100 nF local decoupling must have some causal effect or at least fail deterministically if missing/incorrect per educational abstraction.

The physical 3.3 V rail must be measured by probes. The board cannot report successful U1/U3/U4 operation when the actual logic rail is unpowered, an open return exists or a disallowed short is present. All tuning values not specified in the ZIP belong in a clearly documented fictional educational settings asset, not unmarked aerospace/production specifications.

# 28. U1 MICROCONTROLLER MODEL — 2 HZ DIAGNOSTIC OUTPUT

U1 is a generic QFN48 MCU with power/ground, reset, GPIO, ADC, I²C and safety-feedback interfaces. For educational gameplay implement deterministic firmware-state behavior: boot only with valid rails/reset, periodic nominal **2 Hz** output with bounded jitter/tolerance, status output, sensor polling and Q1 load control permission.

The 2 Hz waveform must be generated by simulation time and recorded as timestamped signal edges. Do not fake a sine/square trace via a UI animation independently of the board's actual rail or firmware state. The debug header J2 may expose observability and programming status, but keep in-game firmware changes bounded and coherent.

# 29. U3 I²C TEMPERATURE SENSOR — REAL DIGITAL EXCHANGE

Simulate U3 as an addressable I²C sensor on `I2C_SCL` and `I2C_SDA` nets with timing, pull-up behavior and acknowledgment. The instrument should return a modeled temperature reading only when power/ground, bus wiring, pull-ups, start/address/ACK, and timing are valid. Missing SDA, missing SCL or incorrect R2 must visibly change logic-analyzer capture and communication outcomes.

Use a simplified bus waveform model with pull-up RC and digital thresholds; no need for cycle-perfect silicon emulation unless the current Project Spark architecture already supports it. Truth state, detector samples and displayed temperature must remain separate objects with freshness and validity flags.

# 30. MOSFET Q1 LOAD DRIVER AND VIRTUAL LOAD

Q1 MOSFET must respond to U1's `GPIO_LOAD` through R3, gate bias via R4, the actual controlled load through LED3/R7 and current sense resistor R8. Simulate sufficient source/gate/drain relationships for gate charge/logic enable and load current. The virtual load must not activate when trusted W035 feedback is invalid or the safety PLC/controller has an active fault.

The F03 solder bridge between Q1 gate and drain must add a **real unwanted graph edge** and change transistor/load behavior, not merely turn the LED amber. After repair, current and switching behavior should return to qualified values as measured by real probes.

# 31. U4 CURRENT MONITOR AND R8 SHUNT FEEDBACK

U4 monitors the current-shunt circuit using source pads including `U4.SENSE_POS` and `U4.SENSE_NEG`; R8 has source value `0R1`. Compute a physically consistent sense voltage such as `Vshunt = I_load × R8` and apply bounded measurement noise/offset, timestamps and validity. W035 carries U4's trusted “current OK” condition to U1, not simply a display label.

If U4 is disconnected, unpowered, stale, miscalibrated or its trusted W035 route is broken, represent `Sample.Valid=false` and remove load enable. **Never replace invalid measurement with zero current**, because a detached sensor cannot prove a safe load.

# 32. DMM — PROBES ON REAL ELECTRICAL NODES

The bench digital multimeter must require player selection of measurement function and real probe target nodes/returns. Read the authoritative circuit state for DC voltage, continuity, resistance (only in safe unpowered conditions), and current where suitable. Measurements must reflect open nets, shorts, regulator dropout and shunt voltage.

Probe UI overlays must identify the actual `PadID` and layer; a meter pointed at decorative gold art without a mapped live pad must return “invalid probe”, not a fabricated number. Avoid probe mechanisms that inadvertently short two nodes through a fictitious zero-resistance meter.

# 33. OSCILLOSCOPE — MEASURED 2 HZ, DUTY AND TRANSIENTS

Implement actual signal sampling for U1 diagnostic GPIO, I²C SCL/SDA and load gate nodes, with appropriate sample clock, horizontal scale and trigger. The scope must show measured time-series points from simulation, not a texture that animates independently. Missing power, wrong routing, failed clock or an open route must alter the waveform.

Expose nominal **2.0 Hz** expectation, measured frequency/period, amplitude, edge timing, and clear warning when the signal is absent, out of tolerance or stale. Scope calibration and input-probe grounding must be checked in the simulation.

# 34. LOGIC ANALYZER — SDA, SCL, OUT, FEEDBACK

Use the four-channel logic analyzer for digital waveforms: `SCL`, `SDA`, diagnostic `OUT`, and `FB` from actual sample/logic state. Show START/STOP/ACK and possible NACK when the sensor cannot respond. Capture the wrong R2=47K fault as degraded SDA rise timing and possible communication failure under the chosen board model.

The analyzer may sample simplified digital events with timestamped edges; it must not automatically decode I²C from the expected design when no valid edges are present. Status overlays and cursor readings use the measured signal history.

# 35. FAULT F01 — OPEN GROUND VIA V05 / W030

Inject **F01** only at the designated testing stage. Fault location from source: **V05 on W030 return path**. The open via breaks its actual through-layer graph connection and changes the expected power-LED/ground return behavior. The player should use continuity, layer inspection and DRC to localize the fault and rebuild V05.

Do not auto-award the fix when the visual hole gets a green circle. Require real reestablishment of layer conduction, no short, a fresh unpowered continuity result and a powered system test. Preserve the original separate V05 installation requirement before fault injection.

# 36. FAULT F02 — R2 WRONG VALUE 47K VS 4K7

Inject **F02** by replacing or corrupting **R2 SDA pull-up** with **47K** rather than the required **4K7**. Keep R1 SCL pull-up independent. This must increase SDA rise time via an educational pull-up/line-capacitance model and may cause I²C NACK/bit failures with chosen tolerances.

Repair requires removing the defective value through existing shelf/repair interactions and installing a correct R2=4K7 part, then observing new measured SCL/SDA behavior and valid I²C acknowledgment. Merely editing SCADA text “R2 OK” is not acceptable.

# 37. FAULT F03 — Q1 GATE/DRAIN SOLDER BRIDGE

Inject **F03** as a **real unwanted conductive edge between neighboring Q1 gate and drain pads**. It may cause unwanted coupling and load-control malfunction. Keep net graph electrical equivalence and geometric error tracking separate, so any illegal edge appears both in DRC and actual circuit response.

Repair must remove the bridge physically through the in-game PCB repair workflow, eliminate the unwanted graph edge and pass fresh safe load switching/current-feedback tests. Do not reset Q1 current directly to a correct value when the joint is removed.

# 38. FAULT CONTROLLER — DISTINCT TIMES, REPAIRS AND EVIDENCE

Provide a deterministic fault controller that injects F01–F03 at explicit objective stages, one at a time or as permitted by the authored scenario. Use stable seeds and replayable trials. Each fault has its own component/graph action, detector changes, diagnosis/repair action and unique qualification ID.

Fault certification must require observed bad behavior, a proper repair event and a new passing electrical test. Prevent bypass by replacing the whole prefab, reloading an old save, pre-awarding faults, resetting the board before acknowledgment or hiding the error. Ensure fault recovery is educational rather than an unbounded free-form soldering simulation.

# 39. OBJECTIVE 01 — INSPECT BOARD SCHEMATIC AND NETS

Source objective O1 is `Inspect PCB schematic and identify all nets`. Require the player to inspect top/bottom layer views, read net names/footprints, identify protected power, logic and ground domains, demonstrate correct pad-to-net associations and record an actual inspection/quiz state, not simply open a UI panel.

PASS: the level evaluator receives an objective-specific `InspectionEvidence` record verifying real required interactions and source net comprehension; no routing is credited as already complete. FAIL on skipping required inspection or selecting wrong net classifications.

# 40. OBJECTIVE 02 — INSTALL 27 CORRECT COMPONENTS

Source O2 requires `27 real parts in matched footprint, correct value/orientation/polarity`. Each of 27 BOM rows must have a unique runtime instance, a qualifying footprint, correct reference, physical placement state, orientation and modeled solder-joint validity. Use the source CSV, not a manually typed list.

PASS: an independently calculated placed/qualified component count equals 27 with no mismatch, no missing part, no bridge/cold joint and clear install evidence. An image of all components in place in the source assembled FBX does **not** count as player assembly.

# 41. OBJECTIVE 03 — COMPLETE ALL 34 COPPER ROUTES AND SIX VIAS

Source O3 requires **W001–W034** plus **V01–V06**. Each route must be player-laid, properly layered, legal in width/clearance, correctly connected to source pads, and present as a real graph edge. All six vias must be player-placed and conductive in the intended route. DRC and ERC must both pass after routing.

PASS: each of the 34 task records has unique evidence and real route geometry; each of six vias is present and passes its association test. Reject a missing route even if some global net is electrically reachable by an accidental or factory bypass. Store full graph checksum for this certification.

# 42. OBJECTIVE 04 — SAFE POWER COMMISSION AND INSTRUMENT TEST

Source O4 requires **protected 5 V**, measured **3.3 V regulator**, functional MCU **2 Hz** output, valid U3 I²C communication, controlled Q1 load and trusted current samples. Normal operating permission requires **W035 installed and qualified** at this stage even though the archive initially ships it disconnected.

PASS: actual power rails and current within configured educational tolerance, measured 2 Hz edges over a nonzero interval, valid U3 transactions, Q1 responding to commands, fresh U4 shunt/current feedback, independent safety permission. All certificates must be backed by instrument traces and graph-state version.

# 43. OBJECTIVE 05 — THREE FAULT REPAIRS AND COMPLETE RETEST

Source O5 requires fault F01 **open V05/W030**, F02 **wrong R2 (47K vs 4K7)**, and F03 **Q1 gate/drain bridge**, each with detected symptoms, correct physical repair, and measured fresh evidence. Include DRC/continuity, I²C logic timing and MOSFET load response as appropriate.

PASS: three independent `FaultRecoveryEvidence` records, no unresolved defects, a complete successful new production test with W035 still functioning. Do not count a fault as repaired solely because the user pressed “Repair” or its indicator turned green.

# 44. OBJECTIVE 06 — SUPERVISED W035 DISCONNECT AND RECOVERY

O6 becomes available **after Objectives 1–5 are historically certified**. Begin in a verified qualified load test. Player physically removes W035, graph connectivity breaks, the trusted sample invalidates, Q1 load permission is removed and the independent safety fault latches. Require evidence of the unsafe-but-protected state (e.g. load drive inhibited, diagnostic rail may remain powered).

Next player repairs W035, generates new live U4 measurements and explicitly resets the controller; the system validates current topology and safe rails. Finally run a **new full board qualification** (routing integrity, 5 V/3.3 V, 2 Hz, I²C, Q1 switching/current feedback) with evidence timestamps after the fault/repair. Only then award Objective 6 and overall `LevelGamePlayManager` **6/6**. No self-reset on reconnection and no stale certificate reuse.

# 45. CERTIFICATION EVIDENCE, HISTORICAL RETENTION AND NO FORCED PASS

Define specific evidence records: `SchematicInspectionEvidence`, `ComponentAssemblyEvidence`, `CopperDRCEvidence`, `BoardCommissioningEvidence`, `ThreeFaultRepairEvidence`, `W035RecoveryEvidence`. Include objective ID, run ID, source net-graph version, measurement epoch, instrument IDs, sample freshness, result and authorized evaluator source.

After intentional O6 break, the current board becomes unsafe but O1–O5 remain **historically certified**. A new post-fault “run epoch” prevents previously collected samples from satisfying O6. Let the existing level manager own the real objective-completion bitset. If the manager API differs, write a thin adapter to its observed public interface and document the mapping.

# 46. RUN STATE MACHINE AND INVALID TRANSITION REJECTION

Suggested Level 94 states: `AwaitSchematic`, `Assembly`, `CopperRouting`, `PrePowerDRC`, `PowerCommissioning`, `FaultTrials`, `PostFaultQualification`, `QualifiedForW035Trial`, `W035OpenLatched`, `W035RepairedAwaitMeasurement`, `AwaitExplicitReset`, `FreshFinalQualification`, `Completed`.

Transitions require authoritative evidence and safe prerequisites, not time delays or visual sequences. Prevent unpowered resistance measurement during active supply, energizing a shorted board, moving a component while its nets are live without a safe isolation operation, injecting O6 before O5, or reusing stale traces after any graph mutation.

# 47. FIXED-STEP ELECTRONICS SIMULATION AND DETERMINISTIC TIME

Use a fixed simulation step or timestamped event scheduler consistent with Project Spark's electrical solver. Track rail voltage/current, R/C simplified time-domain response, digital thresholds, MCU periodic output, I²C bus states, MOSFET conduction, shunt monitor signals and protection events. UI update cadence can be slower than electrical simulation; don't make diagnosis depend on FPS.

For educational behavior a mixed analog/digital approximation is acceptable; document assumptions and limits. Use deterministic reproducible configurations for faults and tests. Never fabricate 3.300 V at the DMM from the target design if U2's actual output is invalid.

# 48. SIMULATION AUTHORITY VS VIEW/PROBE PRESENTATION

Separate authoritative `PCBGraph` and circuit state from transient views. Instruments must **read** from the graph, physics and signal histories; they may not set these values except through validated player controls. Material/LED emissive states are derived snapshots from the simulation.

Add strict validity states (`Disconnected`, `Unpowered`, `Stale`, `OutOfRange`, `Fault`, `Calibrating`, `Valid`) to telemetry. An invalid sensor reading should be shown as unavailable (e.g. `—`) with a reason, **never zero** when zero might imply safe current.

# 49. LEVEL INTERACTION UX — HIGH PRECISION WITHOUT FRUSTRATION

Provide 6 work modes: **Inspect, Assembly, Routing, Layers, Electrical Test, Repair**. The fixed large table shows the PCB and compact instruments; other game room elements remain sparse. Each mode should explain current goal, highlight only relevant pads/components, support snap/rotate/undo and always allow switching back to schematic.

Use actual magnification and camera pan/zoom for 100×70 mm physical board. Show the current layer, route width, clearance and via count in unobtrusive UI. Build accessible colorblind alternatives to green/amber/red, readable labels at high zoom, and controller/keyboard options for fine action selection.

# 50. SCADA AND INSTRUMENT UI — ONLY SOURCE-BACKED VALUES

Display correct live supply values, board temperature, U1 diagnostic frequency/duty, I²C state/ACK, Q1 load duty/current, U4 sample freshness, W035 continuity, DRC/short findings, 27/27 placement, 34/34 routing, 6/6 vias, objective progression and fault history. All instrument readouts must use the same graph data.

Plot oscilloscope and logic analyzer traces with timestamps, not a fixed picture. Add a schematic/board net highlight that can show which conductive paths and vias connect the selected node. Keep failure messages explainable: “SDA rise-time too slow; inspect R2”, “Ground return open at V05”, “W035 feedback invalid; load disabled”.

# 51. SAVED GAME, RESTART, REPLAY AND SCHEMA VERSIONING

Persist placed component Ref/value/rotation/joint state, every player segment's polyline/layer, valid vias, factory net version, W035 installed/removed and fault latch state, supply status, diagnostic run IDs and objective historical evidence. Load/save must rebuild **real graph edges** from the save and re-evaluate safety on loading, not restore renderer colors as truth.

Support save migration by source manifest version and consistent IDs. A previously successful O6 cannot be replayed through a stale cached certificate in a new/reset run. Provide restart level, undo limited player actions and an explicit full reset to original unassembled board state.

# 52. SCENE BUILDER — ONE-CLICK LEVEL 94

Implement `Tools/Project Spark/Levels/Create Level 94` as an actual idempotent Editor command. It must safely create/update `Level_94.asset`, `Level_94_Layout.prefab`, physical art, runtime board graph, 127 functional pad bindings, component inventory, factory nets, 34 route tasks, six via tasks, W035 jumper, instruments, safety, six objectives and test helpers. Make a dry-run validation report before mutation.

A generated scene should start at **0/6**, all components uninstalled, routes/vias missing and W035 absent. Do not delete the base Project Spark game room, rewrite other levels' assets, introduce namespace collisions or duplicate a `LevelGamePlayManager`. Use `Undo`, prefab-aware asset editing and explicit generated-root ownership.

# 53. SOURCE UNITY HELPER MENUS — EXACT IMPORT ORDER

The supplied art-only helper menus are, in order: `Tools/Project Spark/Levels/94/1. Create Native PBR Materials`; `.../2. Assemble 48 Modular FBXs Under Selected Layout`; `.../3. Apply PBR To Selected Art`; `.../4. Create PCB Planning Data Asset`; `.../5. Validate Imported Assets And Plans`. The supplied sources: `SparkLevel94PBRMaterialEditor.cs`, `SparkLevel94ArtAssembler.cs`, `SparkLevel94PlanningDataEditor.cs`, `SparkLevel94ArtValidator.cs` belong in an `Editor` folder, while `SparkLevel94PCBDefinition.cs` and `SparkLevel94BoardVisualController.cs` stay runtime-side.

Their outputs create art/materials/planning assets only. The new one-click playable level builder is separate. Correct imported paths based on real project layout; if paths or menu selections are invalid, fail with a concrete diagnostic rather than defaulting to a global asset with a similar name.

# 54. SCRIPT ARCHITECTURE — CONCRETE RESPONSIBILITIES, NOT ASSUMED APIS

Recommended *new* Level 94 responsibilities: `SparkLevel94Coordinator` (run lifecycle), `SparkPCBBoard` (physical/topology model), `SparkPCBPad` and `SparkPCBFootprint`, `SparkPCBPartPlacement`, `SparkPCBCopperRoute`, `SparkPCBVia`, `SparkPCBDesignRuleValidator`, `SparkPCBElectricalGraphAdapter`, `SparkPCBVirtualSupply`, `SparkPCBControllerModel`, `SparkPCBI2CBusModel`, `SparkPCBLoadModel`, `SparkPCBCircuitInstrument` subclasses, `SparkLevel94FaultController`, `SparkLevel94W035Safety`, `SparkLevel94ObjectiveEvaluator`, `SparkLevel94VisualAdapter`.

Do not copy these guessed names into existing namespaces without checking collisions. Use serialized references where practical; publish immutable measured snapshots/events to UI. Keep every safety-critical decision inside runtime simulation/evaluator, never in an FBX-related script.

# 55. CIRCUIT GRAPH CHANGE TRANSACTIONS AND EDGE OWNERSHIP

Every graph mutation—part installation/removal, trace creation/removal, via insertion/removal, solder bridge injection/repair, W035 plug/unplug—should be one transaction with preconditions, commit/rollback, graph-version increment, dependent-sample invalidation and state change event. Track edge owner `FactoryCopper`, `PlayerTrace(W###)`, `PlayerVia(V##)`, `ComponentPin`, `FaultEdge(F##)`, `ServiceJumper(W035)`.

This prevents a removed W035 from leaving a hidden safety connection behind, and prevents a player from restoring a short simply by hiding its geometry. The same graph used by DRC must drive voltage/current and objective evidence through the project's real solver or its validated adapter.

# 56. NETLIST AMBIGUITIES — FLAG, DO NOT SILENTLY FIX

The source planning CSV is not KiCad output or a verified electrical design, and its factory-net records have intentional/possible net-level overlap with player tasks. For example `LED2.C → GND` is premanufactured while W033 demands a route from LED2.C to J1.GND. Document whether that is a redundant copper route, deliberate challenge requirement, or source authoring mistake. Either way: do not award W033 simply because a factory edge creates equivalent net-level continuity.

Additional missing implementation specifics may include exact QFN48 pinout, pull-up line capacitance, device register map and protective thresholds. These must be represented as explicitly configurable *educational abstractions* with assumptions and tests. Maintain the original CSV as the source record; add a separate reviewed reconciliation file when needed.

# 57. UNITY TESTS — MECHANICAL, ELECTRICAL AND REGRESSION

Implement the **56 source `Engineering_Acceptance_Tests_56.csv` definitions** as real Edit Mode, Play Mode and/or deterministic simulation harness tests. Do not confuse the source test rows labeled `NOT TESTED - UNITY IMPLEMENTATION REQUIRED` with passed tests. Where Unity testing cannot be run, record `NOT_RUN`, environment and blocker, never mark PASS.

High-priority automated cases: missing W001 prevents 5 V path; no 3.3 V when U2 supply invalid; all vias absent initially; cross-layer crossing does not short; same-layer incompatible crossing fails; V05 fault physically breaks W030; R2=47K degrades SDA; Q1 bridge changes load behavior; open W035 invalidates feedback; explicit reset alone fails without repair/fresh sample; no fake 6/6; save/load faithfully restores route graph. Test actual stage order and disabled unsafe commands.

# 58. NON-REGRESSION — PRESERVE PROJECT SPARK LEVELS 1–93

Ensure generated code is isolated to Level 94 except for generic opt-in PCB abstractions. Existing levels may use wire gameplay, electrical physics and object-shelf interactions; new PCB graph adapters must not change their solver behavior by default. Use assembly-definition boundaries and unique menu paths to avoid class collisions or Editor helper duplication.

Regression suite includes loading an earlier level, placing/throwing appropriate shelf objects, connecting/disconnecting its existing wires, reading its level manager state, and returning to Level 94. No scene Asset GUID rewrites, no modified previous level prefab overrides, no changed shop identifiers and no stealth changes to physics layers.

# 59. AAA PRODUCTION QUALITY, SHADER AND BOARD READABILITY

The base art is procedurally generated and should be polished in Unity with grounded PBR, correct normals/UVs, detailed but restrained silkscreen, visible solder fillets/pads, realistic antistatic surface and fine metallic traces. Use the supplied material textures and imported shader files as a starting point. High-quality materials do not mean postprocess blur; keep R/C designators, measurement displays and trace paths legible at close zoom.

Optimize with combined static meshes for fixed support structures, batched pad material, controlled translucent hologram passes, editor static lighting where appropriate and targeted real-time lights only on the active PCB. Deliver collision/LOD checklists and actual frame profiling once a Unity project target is available.

# 60. PLAYER EDUCATION AND TUTORIAL HINTS

Objective 1 should teach the difference between schematic *intent*, copper *geometry* and live electrical continuity. Highlight what a via does, why a 4.7 kΩ pull-up matters, why a bad ground return alters LEDs, and why an open feedback wire must be treated as unknown rather than zero. Provide optional stepwise hints, but do not auto-route copper without recording player action and legitimate objective evidence.

The level is the introduction to later motherboard-mastery levels. The reusable pad/footprint/traces/instrument abstractions should be generic, while this board's test IDs, W035, component values and failures remain Level 94 specific.

# 61. FULL GAMEPLAY WALKTHROUGH — ZERO TO SIX OBJECTIVES

1. Start a fresh Level 94 scene; player sees empty PCB footprints, a component tray, unpowered test rack and 0/6.
2. Inspect top/bottom copper, identify schematic nets and basic safety rules; certify O1.
3. Place U1–U4, Q1, D1, LED1–3, R1–R8, C1–C7, SW1, J1, J2 with correct packages/orientations; inspect solder state; certify O2.
4. Route W001–W034; insert V01–V06; run DRC/ERC; verify real continuity and no wrong-net shorts; certify O3.
5. Install/qualify W035 for normal operation, activate protected 5 V, measure 3.3 V regulator, 2 Hz clock, I²C ACK, Q1 current feedback; certify O4.
6. Inject and diagnose F01 open V05/W030, F02 incorrect R2=47K, F03 Q1 solder bridge; repair each and rerun full board test; certify O5.
7. Begin deliberate supervised W035 service test after O1–O5 are recorded; pull physical jumper, observe actual graph break, invalid trusted current and protected load inhibit.
8. Restore W035, gather new valid U4 samples, perform explicit reset, rerun full production test with a new run ID.
9. Only real objective authority certifies O6 and all 6/6; show a cinematic board-qualified display driven by this result.

# 62. DELIVERABLES — CODE, ASSETS, REPORTS AND HANDOFF

Deliver the real Unity C# implementations (runtime, Editor and tests), generated scene/prefabs/assets, native PBR material assets, authoritative netlist/pad mappings, a conflict/reconciliation report, 34 route task definitions, six via tasks, 27 component footprints, W035 fault event controller, instrument panel bindings, save/load schema, test suite and a concise human walkthrough.

Generate a `L94_Source_To_Runtime_Mapping.csv` containing `SourceRef, SourceFile, SceneObject, RuntimeNodeID, Component/RouteState, ValidationResult`; and `L94_Run_QA_Report.md` with Unity version, pipeline, import/compile/test statuses, measured FPS if available and remaining blockers. Present all results honestly: *not tested* is not *passed*.

# 63. FINAL ACCEPTANCE CONTRACT — REAL, PLAYABLE, NO PRETENSE

The result qualifies as fully implemented only if Level 94 loads in the actual Project Spark project, starts at 0/6 with 27 uninstalled parts, 34 unconnected routes, six missing vias, and absent W035; validates player-driven component placement and 2-layer routing; produces computed power/signal/instrument readings; enforces three real circuit faults and repairs; inhibits the MOSFET load when W035 is disconnected; requires real reconnection/fresh current evidence/explicit reset; and **`LevelGamePlayManager` actually records 6/6** after the fresh final production test.

All **56 source acceptance tests** must be attempted and reported with PASS/FAIL/NOT_RUN and evidence; there must be no regressions in Levels 1–93 or in the Project Spark futuristic game room. The source ZIP never by itself proves any of these runtime achievements. Never claim success until the target Unity project compiles and Play Mode behavior is observed.

---

# TECHNICAL SOURCE ANNEXES — ORIGINAL PACKAGE DATA (AUDIT COPIES)

The records below are **verbatim content** of the source archive (normalized to LF line endings). They are source-authored planning/art data; the Unity developer should import the corresponding original CSV/JSON files programmatically rather than parsing this Markdown. Do not label these as already integrated or independently verified manufacturing data.

## ANNEX A — Exact 48 Model Manifest

Source: `ProjectSpark_Level94_BoardFoundry_AAA/Data/Model_Manifest_48.csv`.

```csv
Index,Model,Group,FBX,GLB,MeshParts,Triangles,FBXBytes,GLBBytes,SceneScale,PhysicalBoardMM,GameplayReady
0,L94_AssemblyBenchFrame,BenchAndFixture,Models/00_L94_AssemblyBenchFrame.fbx,GLB/00_L94_AssemblyBenchFrame.glb,8,468,28567,19172,14.0,100 x 70,"NO, visual only"
1,L94_AntiStaticWorkSurface,BenchAndFixture,Models/01_L94_AntiStaticWorkSurface.fbx,GLB/01_L94_AntiStaticWorkSurface.glb,3,48,10368,4872,14.0,100 x 70,"NO, visual only"
2,L94_PrecisionFixtureBase,BenchAndFixture,Models/02_L94_PrecisionFixtureBase.fbx,GLB/02_L94_PrecisionFixtureBase.glb,4,848,22026,29152,14.0,100 x 70,"NO, visual only"
3,L94_FixtureClampLeft,BenchAndFixture,Models/03_L94_FixtureClampLeft.fbx,GLB/03_L94_FixtureClampLeft.glb,3,128,12117,8084,14.0,100 x 70,"NO, visual only"
4,L94_FixtureClampRight,BenchAndFixture,Models/04_L94_FixtureClampRight.fbx,GLB/04_L94_FixtureClampRight.glb,3,128,12125,8092,14.0,100 x 70,"NO, visual only"
5,L94_InspectionMicroscopeArm,BenchAndFixture,Models/05_L94_InspectionMicroscopeArm.fbx,GLB/05_L94_InspectionMicroscopeArm.glb,5,168,18467,12180,14.0,100 x 70,"NO, visual only"
6,L94_MicroscopeLensAndRingLight,BenchAndFixture,Models/06_L94_MicroscopeLensAndRingLight.fbx,GLB/06_L94_MicroscopeLensAndRingLight.glb,4,1152,29628,37068,14.0,100 x 70,"NO, visual only"
7,L94_ComponentRepairTray,BenchAndFixture,Models/07_L94_ComponentRepairTray.fbx,GLB/07_L94_ComponentRepairTray.glb,4,588,19186,21732,14.0,100 x 70,"NO, visual only"
8,L94_FR4BoardSubstrate,BarePCBAndPads,Models/08_L94_FR4BoardSubstrate.fbx,GLB/08_L94_FR4BoardSubstrate.glb,3,652,17256,18908,14.0,100 x 70,"NO, visual only"
9,L94_TopGreenSoldermask,BarePCBAndPads,Models/09_L94_TopGreenSoldermask.fbx,GLB/09_L94_TopGreenSoldermask.glb,2,72,7980,5248,14.0,100 x 70,"NO, visual only"
10,L94_BottomGreenSoldermask,BarePCBAndPads,Models/10_L94_BottomGreenSoldermask.fbx,GLB/10_L94_BottomGreenSoldermask.glb,1,12,4564,1988,14.0,100 x 70,"NO, visual only"
11,L94_TopSMDGoldPads,BarePCBAndPads,Models/11_L94_TopSMDGoldPads.fbx,GLB/11_L94_TopSMDGoldPads.glb,6,2060,44253,48400,14.0,100 x 70,"NO, visual only"
12,L94_BottomCopperPadSet,BarePCBAndPads,Models/12_L94_BottomCopperPadSet.fbx,GLB/12_L94_BottomCopperPadSet.glb,1,288,8613,10000,14.0,100 x 70,"NO, visual only"
13,L94_SilkscreenAndTestFiducials,BarePCBAndPads,Models/13_L94_SilkscreenAndTestFiducials.fbx,GLB/13_L94_SilkscreenAndTestFiducials.glb,2,768,16784,25188,14.0,100 x 70,"NO, visual only"
14,L94_CopperEdgeAndMountBosses,BarePCBAndPads,Models/14_L94_CopperEdgeAndMountBosses.fbx,GLB/14_L94_CopperEdgeAndMountBosses.glb,1,48,5064,3184,14.0,100 x 70,"NO, visual only"
15,L94_W035DetachableFeedbackJumper,BarePCBAndPads,Models/15_L94_W035DetachableFeedbackJumper.fbx,GLB/15_L94_W035DetachableFeedbackJumper.glb,4,208,15694,11160,14.0,100 x 70,"NO, visual only"
16,L94_U1_MCU_QFN48,BoardComponents,Models/16_L94_U1_MCU_QFN48.fbx,GLB/16_L94_U1_MCU_QFN48.glb,5,680,23531,21776,14.0,100 x 70,"NO, visual only"
17,L94_U2_LDO_SOT223,BoardComponents,Models/17_L94_U2_LDO_SOT223.fbx,GLB/17_L94_U2_LDO_SOT223.glb,3,164,12478,9312,14.0,100 x 70,"NO, visual only"
18,L94_U3_I2CTemperatureSensor_SOIC8,BoardComponents,Models/18_L94_U3_I2CTemperatureSensor_SOIC8.fbx,GLB/18_L94_U3_I2CTemperatureSensor_SOIC8.glb,3,188,12832,10208,14.0,100 x 70,"NO, visual only"
19,L94_U4_CurrentMonitor_SOIC8,BoardComponents,Models/19_L94_U4_CurrentMonitor_SOIC8.fbx,GLB/19_L94_U4_CurrentMonitor_SOIC8.glb,3,188,12766,10172,14.0,100 x 70,"NO, visual only"
20,L94_Q1_LoadMOSFET_SOT23,BoardComponents,Models/20_L94_Q1_LoadMOSFET_SOT23.fbx,GLB/20_L94_Q1_LoadMOSFET_SOT23.glb,2,48,8170,4588,14.0,100 x 70,"NO, visual only"
21,L94_D1_InputSchottkySMA,BoardComponents,Models/21_L94_D1_InputSchottkySMA.fbx,GLB/21_L94_D1_InputSchottkySMA.glb,3,48,10787,5224,14.0,100 x 70,"NO, visual only"
22,L94_LED1_PowerGreen,BoardComponents,Models/22_L94_LED1_PowerGreen.fbx,GLB/22_L94_LED1_PowerGreen.glb,3,48,10781,4968,14.0,100 x 70,"NO, visual only"
23,L94_LED2_StatusBlue,BoardComponents,Models/23_L94_LED2_StatusBlue.fbx,GLB/23_L94_LED2_StatusBlue.glb,3,48,10779,4948,14.0,100 x 70,"NO, visual only"
24,L94_LED3_LoadAmber,BoardComponents,Models/24_L94_LED3_LoadAmber.fbx,GLB/24_L94_LED3_LoadAmber.glb,3,48,10755,4944,14.0,100 x 70,"NO, visual only"
25,L94_R1_SCLPullup_4K7,BoardComponents,Models/25_L94_R1_SCLPullup_4K7.fbx,GLB/25_L94_R1_SCLPullup_4K7.glb,3,48,10769,5192,14.0,100 x 70,"NO, visual only"
26,L94_R2_SDAPullup_4K7,BoardComponents,Models/26_L94_R2_SDAPullup_4K7.fbx,GLB/26_L94_R2_SDAPullup_4K7.glb,3,48,10795,5188,14.0,100 x 70,"NO, visual only"
27,L94_R3_GateSeries_100R,BoardComponents,Models/27_L94_R3_GateSeries_100R.fbx,GLB/27_L94_R3_GateSeries_100R.glb,3,48,10820,5204,14.0,100 x 70,"NO, visual only"
28,L94_R4_GatePulldown_100K,BoardComponents,Models/28_L94_R4_GatePulldown_100K.fbx,GLB/28_L94_R4_GatePulldown_100K.glb,3,48,10784,5232,14.0,100 x 70,"NO, visual only"
29,L94_R5_PowerLED_1K,BoardComponents,Models/29_L94_R5_PowerLED_1K.fbx,GLB/29_L94_R5_PowerLED_1K.glb,3,48,10771,5176,14.0,100 x 70,"NO, visual only"
30,L94_R6_StatusLED_330R,BoardComponents,Models/30_L94_R6_StatusLED_330R.fbx,GLB/30_L94_R6_StatusLED_330R.glb,3,48,10763,5200,14.0,100 x 70,"NO, visual only"
31,L94_R7_LoadLED_330R,BoardComponents,Models/31_L94_R7_LoadLED_330R.fbx,GLB/31_L94_R7_LoadLED_330R.glb,3,48,10749,5184,14.0,100 x 70,"NO, visual only"
32,L94_R8_Shunt_0R1,BoardComponents,Models/32_L94_R8_Shunt_0R1.fbx,GLB/32_L94_R8_Shunt_0R1.glb,3,48,10751,4944,14.0,100 x 70,"NO, visual only"
33,L94_C1_InputBulk_10u,BoardComponents,Models/33_L94_C1_InputBulk_10u.fbx,GLB/33_L94_C1_InputBulk_10u.glb,2,36,7982,4168,14.0,100 x 70,"NO, visual only"
34,L94_C2_OutputBulk_22u,BoardComponents,Models/34_L94_C2_OutputBulk_22u.fbx,GLB/34_L94_C2_OutputBulk_22u.glb,2,36,7976,4176,14.0,100 x 70,"NO, visual only"
35,L94_C3_MCUDecoupling_100n,BoardComponents,Models/35_L94_C3_MCUDecoupling_100n.fbx,GLB/35_L94_C3_MCUDecoupling_100n.glb,2,36,8034,4200,14.0,100 x 70,"NO, visual only"
36,L94_C4_SensorDecoupling_100n,BoardComponents,Models/36_L94_C4_SensorDecoupling_100n.fbx,GLB/36_L94_C4_SensorDecoupling_100n.glb,2,36,8012,4224,14.0,100 x 70,"NO, visual only"
37,L94_C5_CurrentMonitorDecoupling_100n,BoardComponents,Models/37_L94_C5_CurrentMonitorDecoupling_100n.fbx,GLB/37_L94_C5_CurrentMonitorDecoupling_100n.glb,2,36,8049,4236,14.0,100 x 70,"NO, visual only"
38,L94_C6_ResetFilter_10n,BoardComponents,Models/38_L94_C6_ResetFilter_10n.fbx,GLB/38_L94_C6_ResetFilter_10n.glb,2,36,7980,4180,14.0,100 x 70,"NO, visual only"
39,L94_C7_AnalogFilter_1u,BoardComponents,Models/39_L94_C7_AnalogFilter_1u.fbx,GLB/39_L94_C7_AnalogFilter_1u.glb,2,36,7992,4196,14.0,100 x 70,"NO, visual only"
40,L94_SW1_TactileReset,BoardComponents,Models/40_L94_SW1_TactileReset.fbx,GLB/40_L94_SW1_TactileReset.glb,3,140,12018,8512,14.0,100 x 70,"NO, visual only"
41,L94_J1_Protected5VTerminal,BoardComponents,Models/41_L94_J1_Protected5VTerminal.fbx,GLB/41_L94_J1_Protected5VTerminal.glb,3,332,15288,11792,14.0,100 x 70,"NO, visual only"
42,L94_J2_DebugProgrammingHeader,BoardComponents,Models/42_L94_J2_DebugProgrammingHeader.fbx,GLB/42_L94_J2_DebugProgrammingHeader.glb,2,132,9054,7416,14.0,100 x 70,"NO, visual only"
43,L94_BenchOscilloscope,TestInstruments,Models/43_L94_BenchOscilloscope.fbx,GLB/43_L94_BenchOscilloscope.glb,8,608,32007,23696,14.0,100 x 70,"NO, visual only"
44,L94_BenchDigitalMultimeter,TestInstruments,Models/44_L94_BenchDigitalMultimeter.fbx,GLB/44_L94_BenchDigitalMultimeter.glb,6,368,23023,17444,14.0,100 x 70,"NO, visual only"
45,L94_ProtectedBenchPowerSupply,TestInstruments,Models/45_L94_ProtectedBenchPowerSupply.fbx,GLB/45_L94_ProtectedBenchPowerSupply.glb,6,448,24250,19792,14.0,100 x 70,"NO, visual only"
46,L94_FourChannelLogicAnalyzer,TestInstruments,Models/46_L94_FourChannelLogicAnalyzer.fbx,GLB/46_L94_FourChannelLogicAnalyzer.glb,6,384,23333,18376,14.0,100 x 70,"NO, visual only"
47,L94_PCBAssemblySCADAPanel,TestInstruments,Models/47_L94_PCBAssemblySCADAPanel.fbx,GLB/47_L94_PCBAssemblySCADAPanel.glb,5,168,17669,10064,14.0,100 x 70,"NO, visual only"
```

## ANNEX B — Component BOM — All 27 Installable Parts

Source: `ProjectSpark_Level94_BoardFoundry_AAA/Data/Component_BOM_27.csv`.

```csv
Ref,PackageKind,Value,Model,FootprintWidthMM,FootprintLengthMM,Xmm,Zmm,RotationDegrees,InitialInstalled,CorrectPolarityRequired,PlacementKind,Qualification
U1,mcu,MCU_QFN48,L94_U1_MCU_QFN48,10,10,0,0,0,NO,NO,SnapToFootprint,Inspect value+orientation+joint quality
U2,ldo,3V3_LDO,L94_U2_LDO_SOT223,7,7,-25,10,0,NO,YES,SnapToFootprint,Inspect value+orientation+joint quality
U3,soic,I2C_SENSOR,L94_U3_I2CTemperatureSensor_SOIC8,7,5,26,9,0,NO,YES,SnapToFootprint,Inspect value+orientation+joint quality
U4,soic,CURRENT_MONITOR,L94_U4_CurrentMonitor_SOIC8,7,5,28,-12,0,NO,YES,SnapToFootprint,Inspect value+orientation+joint quality
Q1,sot,MOSFET,L94_Q1_LoadMOSFET_SOT23,4,3,12,-22,0,NO,YES,SnapToFootprint,Inspect value+orientation+joint quality
D1,diode,SCHOTTKY,L94_D1_InputSchottkySMA,5,3,-40,20,0,NO,YES,SnapToFootprint,Inspect value+orientation+joint quality
LED1,led,LED_GREEN,L94_LED1_PowerGreen,3,2,-37,-12,0,NO,YES,SnapToFootprint,Inspect value+orientation+joint quality
LED2,led,LED_BLUE,L94_LED2_StatusBlue,3,2,-35,-23,0,NO,YES,SnapToFootprint,Inspect value+orientation+joint quality
LED3,led,LED_AMBER,L94_LED3_LoadAmber,3,2,40,-27,0,NO,YES,SnapToFootprint,Inspect value+orientation+joint quality
R1,res,4K7,L94_R1_SCLPullup_4K7,3,1.7,-7,22,0,NO,NO,SnapToFootprint,Inspect value+orientation+joint quality
R2,res,4K7,L94_R2_SDAPullup_4K7,3,1.7,1,22,0,NO,NO,SnapToFootprint,Inspect value+orientation+joint quality
R3,res,100R,L94_R3_GateSeries_100R,3,1.7,9,-18,0,NO,NO,SnapToFootprint,Inspect value+orientation+joint quality
R4,res,100K,L94_R4_GatePulldown_100K,3,1.7,21,-25,0,NO,NO,SnapToFootprint,Inspect value+orientation+joint quality
R5,res,1K,L94_R5_PowerLED_1K,3,1.7,-30,-9,0,NO,NO,SnapToFootprint,Inspect value+orientation+joint quality
R6,res,330R,L94_R6_StatusLED_330R,3,1.7,-27,-19,0,NO,NO,SnapToFootprint,Inspect value+orientation+joint quality
R7,res,330R,L94_R7_LoadLED_330R,3,1.7,33,-24,0,NO,NO,SnapToFootprint,Inspect value+orientation+joint quality
R8,res,0R1,L94_R8_Shunt_0R1,3.8,2.2,21,-13,0,NO,NO,SnapToFootprint,Inspect value+orientation+joint quality
C1,cap,10u,L94_C1_InputBulk_10u,4,2.8,-33,15,0,NO,NO,SnapToFootprint,Inspect value+orientation+joint quality
C2,cap,22u,L94_C2_OutputBulk_22u,4,2.8,-16,13,0,NO,NO,SnapToFootprint,Inspect value+orientation+joint quality
C3,cap,100n,L94_C3_MCUDecoupling_100n,3,1.7,-9,-1,0,NO,NO,SnapToFootprint,Inspect value+orientation+joint quality
C4,cap,100n,L94_C4_SensorDecoupling_100n,3,1.7,27,17,0,NO,NO,SnapToFootprint,Inspect value+orientation+joint quality
C5,cap,100n,L94_C5_CurrentMonitorDecoupling_100n,3,1.7,36,-7,0,NO,NO,SnapToFootprint,Inspect value+orientation+joint quality
C6,cap,10n,L94_C6_ResetFilter_10n,3,1.7,0,-19,0,NO,NO,SnapToFootprint,Inspect value+orientation+joint quality
C7,cap,1u,L94_C7_AnalogFilter_1u,3,1.7,13,17,0,NO,NO,SnapToFootprint,Inspect value+orientation+joint quality
SW1,switch,RESET,L94_SW1_TactileReset,6,6,-22,-24,0,NO,NO,SnapToFootprint,Inspect value+orientation+joint quality
J1,terminal,5V_INPUT,L94_J1_Protected5VTerminal,8,12,-45,4,0,NO,NO,SnapToFootprint,Inspect value+orientation+joint quality
J2,header,DEBUG_HEADER,L94_J2_DebugProgrammingHeader,7,12,43,21,0,NO,NO,SnapToFootprint,Inspect value+orientation+joint quality
```

## ANNEX C — Component Placement Coordinates

Source: `ProjectSpark_Level94_BoardFoundry_AAA/Data/Component_Placement_27.csv`.

```csv
Reference,Kind,Xmm,Zmm,WidthMM,DepthMM,Value,Model
U1,mcu,0,0,10,10,MCU_QFN48,L94_U1_MCU_QFN48
U2,ldo,-25,10,7,7,3V3_LDO,L94_U2_LDO_SOT223
U3,soic,26,9,7,5,I2C_SENSOR,L94_U3_I2CTemperatureSensor_SOIC8
U4,soic,28,-12,7,5,CURRENT_MONITOR,L94_U4_CurrentMonitor_SOIC8
Q1,sot,12,-22,4,3,MOSFET,L94_Q1_LoadMOSFET_SOT23
D1,diode,-40,20,5,3,SCHOTTKY,L94_D1_InputSchottkySMA
LED1,led,-37,-12,3,2,LED_GREEN,L94_LED1_PowerGreen
LED2,led,-35,-23,3,2,LED_BLUE,L94_LED2_StatusBlue
LED3,led,40,-27,3,2,LED_AMBER,L94_LED3_LoadAmber
R1,res,-7,22,3,1.7,4K7,L94_R1_SCLPullup_4K7
R2,res,1,22,3,1.7,4K7,L94_R2_SDAPullup_4K7
R3,res,9,-18,3,1.7,100R,L94_R3_GateSeries_100R
R4,res,21,-25,3,1.7,100K,L94_R4_GatePulldown_100K
R5,res,-30,-9,3,1.7,1K,L94_R5_PowerLED_1K
R6,res,-27,-19,3,1.7,330R,L94_R6_StatusLED_330R
R7,res,33,-24,3,1.7,330R,L94_R7_LoadLED_330R
R8,res,21,-13,3.8,2.2,0R1,L94_R8_Shunt_0R1
C1,cap,-33,15,4,2.8,10u,L94_C1_InputBulk_10u
C2,cap,-16,13,4,2.8,22u,L94_C2_OutputBulk_22u
C3,cap,-9,-1,3,1.7,100n,L94_C3_MCUDecoupling_100n
C4,cap,27,17,3,1.7,100n,L94_C4_SensorDecoupling_100n
C5,cap,36,-7,3,1.7,100n,L94_C5_CurrentMonitorDecoupling_100n
C6,cap,0,-19,3,1.7,10n,L94_C6_ResetFilter_10n
C7,cap,13,17,3,1.7,1u,L94_C7_AnalogFilter_1u
SW1,switch,-22,-24,6,6,RESET,L94_SW1_TactileReset
J1,terminal,-45,4,8,12,5V_INPUT,L94_J1_Protected5VTerminal
J2,header,43,21,7,12,DEBUG_HEADER,L94_J2_DebugProgrammingHeader
```

## ANNEX D — All 34 Copper Tasks W001–W034

Source: `ProjectSpark_Level94_BoardFoundry_AAA/Data/Copper_Trace_Tasks_W001_W034.csv`.

```csv
WireID,FromPad,ToPad,Net,SignalType,RequiredLayer,ViaID,RequiredClearanceMM,TargetWidthMM,InitialState,SimulationAuthority,QualificationStage
W001,J1.5V_IN,D1.A,VIN_RAW,power,F.Cu,,.2,.35,MISSING,SparkPCBNetlistValidator+SparkElectricalSolver,03 Copper Routing
W002,D1.K,U2.VIN,+5V_PROT,power,F.Cu,,.2,.35,MISSING,SparkPCBNetlistValidator+SparkElectricalSolver,03 Copper Routing
W003,U2.VOUT,U1.VDD,+3V3,power,F.Cu,,.2,.35,MISSING,SparkPCBNetlistValidator+SparkElectricalSolver,03 Copper Routing
W004,J1.GND,U2.GND,GND,ground,F.Cu,,.2,.35,MISSING,SparkPCBNetlistValidator+SparkElectricalSolver,03 Copper Routing
W005,U2.VOUT,C2.POS,+3V3,power,F.Cu,,.2,.35,MISSING,SparkPCBNetlistValidator+SparkElectricalSolver,03 Copper Routing
W006,C2.NEG,J1.GND,GND,ground,F.Cu,,.2,.35,MISSING,SparkPCBNetlistValidator+SparkElectricalSolver,03 Copper Routing
W007,U2.VIN,C1.POS,+5V_PROT,power,F.Cu,,.2,.35,MISSING,SparkPCBNetlistValidator+SparkElectricalSolver,03 Copper Routing
W008,C1.NEG,J1.GND,GND,ground,F.Cu,,.2,.35,MISSING,SparkPCBNetlistValidator+SparkElectricalSolver,03 Copper Routing
W009,U1.VDD,C3.POS,+3V3,power,F.Cu,,.2,.35,MISSING,SparkPCBNetlistValidator+SparkElectricalSolver,03 Copper Routing
W010,C3.NEG,J1.GND,GND,ground,F.Cu,,.2,.35,MISSING,SparkPCBNetlistValidator+SparkElectricalSolver,03 Copper Routing
W011,U3.VDD,U1.VDD,+3V3,power,F.Cu,,.2,.35,MISSING,SparkPCBNetlistValidator+SparkElectricalSolver,03 Copper Routing
W012,U3.GND,J1.GND,GND,ground,F.Cu,,.2,.35,MISSING,SparkPCBNetlistValidator+SparkElectricalSolver,03 Copper Routing
W013,U1.SCL,U3.SCL,I2C_SCL,digital,F.Cu,,.2,.2,MISSING,SparkPCBNetlistValidator+SparkElectricalSolver,03 Copper Routing
W014,U1.SDA,U3.SDA,I2C_SDA,digital,F.Cu+B.Cu,V01,.2,.2,MISSING,SparkPCBNetlistValidator+SparkElectricalSolver,03 Copper Routing
W015,U1.SCL,R1.A,I2C_SCL,digital,F.Cu,,.2,.2,MISSING,SparkPCBNetlistValidator+SparkElectricalSolver,03 Copper Routing
W016,R1.B,U2.VOUT,+3V3,power,F.Cu,,.2,.35,MISSING,SparkPCBNetlistValidator+SparkElectricalSolver,03 Copper Routing
W017,U1.SDA,R2.A,I2C_SDA,digital,F.Cu,,.2,.2,MISSING,SparkPCBNetlistValidator+SparkElectricalSolver,03 Copper Routing
W018,R2.B,U2.VOUT,+3V3,power,F.Cu+B.Cu,V02,.2,.35,MISSING,SparkPCBNetlistValidator+SparkElectricalSolver,03 Copper Routing
W019,U1.GPIO_LOAD,R3.A,LOAD_GATE_DRIVE,digital,F.Cu,,.2,.2,MISSING,SparkPCBNetlistValidator+SparkElectricalSolver,03 Copper Routing
W020,R3.B,Q1.G,MOSFET_GATE,digital,F.Cu,,.2,.2,MISSING,SparkPCBNetlistValidator+SparkElectricalSolver,03 Copper Routing
W021,Q1.G,R4.A,MOSFET_GATE,digital,F.Cu+B.Cu,V03,.2,.2,MISSING,SparkPCBNetlistValidator+SparkElectricalSolver,03 Copper Routing
W022,R4.B,J1.GND,GND,ground,F.Cu,,.2,.35,MISSING,SparkPCBNetlistValidator+SparkElectricalSolver,03 Copper Routing
W023,Q1.S,R8.B,LOAD_SHUNT,analog,F.Cu,,.2,.2,MISSING,SparkPCBNetlistValidator+SparkElectricalSolver,03 Copper Routing
W024,R8.A,J1.GND,GND,ground,F.Cu,,.2,.35,MISSING,SparkPCBNetlistValidator+SparkElectricalSolver,03 Copper Routing
W025,Q1.D,LED3.C,LOAD_SWITCHED,power,F.Cu+B.Cu,V04,.2,.35,MISSING,SparkPCBNetlistValidator+SparkElectricalSolver,03 Copper Routing
W026,LED3.A,R7.B,LOAD_DRIVE,power,F.Cu,,.2,.35,MISSING,SparkPCBNetlistValidator+SparkElectricalSolver,03 Copper Routing
W027,R7.A,D1.K,+5V_PROT,power,F.Cu,,.2,.35,MISSING,SparkPCBNetlistValidator+SparkElectricalSolver,03 Copper Routing
W028,LED1.A,R5.B,PWR_LED_SERIES,power,F.Cu,,.2,.35,MISSING,SparkPCBNetlistValidator+SparkElectricalSolver,03 Copper Routing
W029,R5.A,U2.VOUT,+3V3,power,F.Cu,,.2,.35,MISSING,SparkPCBNetlistValidator+SparkElectricalSolver,03 Copper Routing
W030,LED1.C,J1.GND,GND,ground,F.Cu+B.Cu,V05,.2,.35,MISSING,SparkPCBNetlistValidator+SparkElectricalSolver,03 Copper Routing
W031,U1.GPIO_STAT,R6.A,STATUS_DRIVE,digital,F.Cu,,.2,.2,MISSING,SparkPCBNetlistValidator+SparkElectricalSolver,03 Copper Routing
W032,R6.B,LED2.A,STATUS_LED_SERIES,power,F.Cu,,.2,.35,MISSING,SparkPCBNetlistValidator+SparkElectricalSolver,03 Copper Routing
W033,LED2.C,J1.GND,GND,ground,F.Cu,,.2,.35,MISSING,SparkPCBNetlistValidator+SparkElectricalSolver,03 Copper Routing
W034,R8.B,U4.SENSE_POS,SHUNT_SENSE_POS,analog,F.Cu+B.Cu,V06,.2,.2,MISSING,SparkPCBNetlistValidator+SparkElectricalSolver,03 Copper Routing
```

## ANNEX E — All Six Via Placement Tasks V01–V06

Source: `ProjectSpark_Level94_BoardFoundry_AAA/Data/Required_Via_Placements_V01_V06.csv`.

```csv
ViaID,RouteID,Xmm,Zmm,TopLayer,BottomLayer,HoleDiameterMM,PadDiameterMM,MustBePlayerPlaced,InitialState
V01,W014,-42,-24,F.Cu,B.Cu,0.35,0.75,YES,MISSING
V02,W018,-20,-12,F.Cu,B.Cu,0.35,0.75,YES,MISSING
V03,W021,0,25,F.Cu,B.Cu,0.35,0.75,YES,MISSING
V04,W025,21,9,F.Cu,B.Cu,0.35,0.75,YES,MISSING
V05,W030,41,-18,F.Cu,B.Cu,0.35,0.75,YES,MISSING
V06,W034,12,-30,F.Cu,B.Cu,0.35,0.75,YES,MISSING
```

## ANNEX F — All 127 Logical Pad Locations

Source: `ProjectSpark_Level94_BoardFoundry_AAA/Data/Logical_Pad_Locations.csv`.

```csv
PadID,Ref,PadNumber,SignalName,Layer,Xmm,Zmm,WorldX,WorldY,WorldZ,OwnerModel,IsDecorativeMeshOnly,LogicalType
U1.VDD,U1,1,VDD,F.Cu,-4.8,-3.9,-0.0672,1.27,-0.0546,L94_U1_MCU_QFN48,YES,FUNCTIONAL_PAD_TO_CREATE_IN_UNITY
U1.VSS,U1,2,VSS,F.Cu,-4.8,-3.1909,-0.0672,1.27,-0.044673,L94_U1_MCU_QFN48,YES,FUNCTIONAL_PAD_TO_CREATE_IN_UNITY
U1.SCL,U1,3,SCL,F.Cu,-4.8,-2.4818,-0.0672,1.27,-0.034745,L94_U1_MCU_QFN48,YES,FUNCTIONAL_PAD_TO_CREATE_IN_UNITY
U1.SDA,U1,4,SDA,F.Cu,-4.8,-1.7727,-0.0672,1.27,-0.024818,L94_U1_MCU_QFN48,YES,FUNCTIONAL_PAD_TO_CREATE_IN_UNITY
U1.GPIO_LOAD,U1,5,GPIO_LOAD,F.Cu,-4.8,-1.0636,-0.0672,1.27,-0.014891,L94_U1_MCU_QFN48,YES,FUNCTIONAL_PAD_TO_CREATE_IN_UNITY
U1.GPIO_STAT,U1,6,GPIO_STAT,F.Cu,-4.8,-0.3545,-0.0672,1.27,-0.004964,L94_U1_MCU_QFN48,YES,FUNCTIONAL_PAD_TO_CREATE_IN_UNITY
U1.CURRENT_OK_IN,U1,7,CURRENT_OK_IN,F.Cu,-4.8,0.3545,-0.0672,1.27,0.004964,L94_U1_MCU_QFN48,YES,FUNCTIONAL_PAD_TO_CREATE_IN_UNITY
U1.RESET_N,U1,8,RESET_N,F.Cu,-4.8,1.0636,-0.0672,1.27,0.014891,L94_U1_MCU_QFN48,YES,FUNCTIONAL_PAD_TO_CREATE_IN_UNITY
U1.SWDIO,U1,9,SWDIO,F.Cu,-4.8,1.7727,-0.0672,1.27,0.024818,L94_U1_MCU_QFN48,YES,FUNCTIONAL_PAD_TO_CREATE_IN_UNITY
U1.SWCLK,U1,10,SWCLK,F.Cu,-4.8,2.4818,-0.0672,1.27,0.034745,L94_U1_MCU_QFN48,YES,FUNCTIONAL_PAD_TO_CREATE_IN_UNITY
U1.ADC_IN,U1,11,ADC_IN,F.Cu,-4.8,3.1909,-0.0672,1.27,0.044673,L94_U1_MCU_QFN48,YES,FUNCTIONAL_PAD_TO_CREATE_IN_UNITY
U1.VDDIO,U1,12,VDDIO,F.Cu,-4.8,3.9,-0.0672,1.27,0.0546,L94_U1_MCU_QFN48,YES,FUNCTIONAL_PAD_TO_CREATE_IN_UNITY
U1.GND,U1,13,GND,F.Cu,-3.9,-4.8,-0.0546,1.27,-0.0672,L94_U1_MCU_QFN48,YES,FUNCTIONAL_PAD_TO_CREATE_IN_UNITY
U1.NC_14,U1,14,NC_14,F.Cu,-3.1909,-4.8,-0.044673,1.27,-0.0672,L94_U1_MCU_QFN48,YES,FUNCTIONAL_PAD_TO_CREATE_IN_UNITY
U1.NC_15,U1,15,NC_15,F.Cu,-2.4818,-4.8,-0.034745,1.27,-0.0672,L94_U1_MCU_QFN48,YES,FUNCTIONAL_PAD_TO_CREATE_IN_UNITY
U1.NC_16,U1,16,NC_16,F.Cu,-1.7727,-4.8,-0.024818,1.27,-0.0672,L94_U1_MCU_QFN48,YES,FUNCTIONAL_PAD_TO_CREATE_IN_UNITY
U1.NC_17,U1,17,NC_17,F.Cu,-1.0636,-4.8,-0.014891,1.27,-0.0672,L94_U1_MCU_QFN48,YES,FUNCTIONAL_PAD_TO_CREATE_IN_UNITY
U1.NC_18,U1,18,NC_18,F.Cu,-0.3545,-4.8,-0.004964,1.27,-0.0672,L94_U1_MCU_QFN48,YES,FUNCTIONAL_PAD_TO_CREATE_IN_UNITY
U1.NC_19,U1,19,NC_19,F.Cu,0.3545,-4.8,0.004964,1.27,-0.0672,L94_U1_MCU_QFN48,YES,FUNCTIONAL_PAD_TO_CREATE_IN_UNITY
U1.NC_20,U1,20,NC_20,F.Cu,1.0636,-4.8,0.014891,1.27,-0.0672,L94_U1_MCU_QFN48,YES,FUNCTIONAL_PAD_TO_CREATE_IN_UNITY
U1.NC_21,U1,21,NC_21,F.Cu,1.7727,-4.8,0.024818,1.27,-0.0672,L94_U1_MCU_QFN48,YES,FUNCTIONAL_PAD_TO_CREATE_IN_UNITY
U1.NC_22,U1,22,NC_22,F.Cu,2.4818,-4.8,0.034745,1.27,-0.0672,L94_U1_MCU_QFN48,YES,FUNCTIONAL_PAD_TO_CREATE_IN_UNITY
U1.NC_23,U1,23,NC_23,F.Cu,3.1909,-4.8,0.044673,1.27,-0.0672,L94_U1_MCU_QFN48,YES,FUNCTIONAL_PAD_TO_CREATE_IN_UNITY
U1.NC_24,U1,24,NC_24,F.Cu,3.9,-4.8,0.0546,1.27,-0.0672,L94_U1_MCU_QFN48,YES,FUNCTIONAL_PAD_TO_CREATE_IN_UNITY
U1.NC_25,U1,25,NC_25,F.Cu,4.8,-3.9,0.0672,1.27,-0.0546,L94_U1_MCU_QFN48,YES,FUNCTIONAL_PAD_TO_CREATE_IN_UNITY
U1.NC_26,U1,26,NC_26,F.Cu,4.8,-3.1909,0.0672,1.27,-0.044673,L94_U1_MCU_QFN48,YES,FUNCTIONAL_PAD_TO_CREATE_IN_UNITY
U1.NC_27,U1,27,NC_27,F.Cu,4.8,-2.4818,0.0672,1.27,-0.034745,L94_U1_MCU_QFN48,YES,FUNCTIONAL_PAD_TO_CREATE_IN_UNITY
U1.NC_28,U1,28,NC_28,F.Cu,4.8,-1.7727,0.0672,1.27,-0.024818,L94_U1_MCU_QFN48,YES,FUNCTIONAL_PAD_TO_CREATE_IN_UNITY
U1.NC_29,U1,29,NC_29,F.Cu,4.8,-1.0636,0.0672,1.27,-0.014891,L94_U1_MCU_QFN48,YES,FUNCTIONAL_PAD_TO_CREATE_IN_UNITY
U1.NC_30,U1,30,NC_30,F.Cu,4.8,-0.3545,0.0672,1.27,-0.004964,L94_U1_MCU_QFN48,YES,FUNCTIONAL_PAD_TO_CREATE_IN_UNITY
U1.NC_31,U1,31,NC_31,F.Cu,4.8,0.3545,0.0672,1.27,0.004964,L94_U1_MCU_QFN48,YES,FUNCTIONAL_PAD_TO_CREATE_IN_UNITY
U1.NC_32,U1,32,NC_32,F.Cu,4.8,1.0636,0.0672,1.27,0.014891,L94_U1_MCU_QFN48,YES,FUNCTIONAL_PAD_TO_CREATE_IN_UNITY
U1.NC_33,U1,33,NC_33,F.Cu,4.8,1.7727,0.0672,1.27,0.024818,L94_U1_MCU_QFN48,YES,FUNCTIONAL_PAD_TO_CREATE_IN_UNITY
U1.NC_34,U1,34,NC_34,F.Cu,4.8,2.4818,0.0672,1.27,0.034745,L94_U1_MCU_QFN48,YES,FUNCTIONAL_PAD_TO_CREATE_IN_UNITY
U1.NC_35,U1,35,NC_35,F.Cu,4.8,3.1909,0.0672,1.27,0.044673,L94_U1_MCU_QFN48,YES,FUNCTIONAL_PAD_TO_CREATE_IN_UNITY
U1.NC_36,U1,36,NC_36,F.Cu,4.8,3.9,0.0672,1.27,0.0546,L94_U1_MCU_QFN48,YES,FUNCTIONAL_PAD_TO_CREATE_IN_UNITY
U1.NC_37,U1,37,NC_37,F.Cu,-3.9,4.8,-0.0546,1.27,0.0672,L94_U1_MCU_QFN48,YES,FUNCTIONAL_PAD_TO_CREATE_IN_UNITY
U1.NC_38,U1,38,NC_38,F.Cu,-3.1909,4.8,-0.044673,1.27,0.0672,L94_U1_MCU_QFN48,YES,FUNCTIONAL_PAD_TO_CREATE_IN_UNITY
U1.NC_39,U1,39,NC_39,F.Cu,-2.4818,4.8,-0.034745,1.27,0.0672,L94_U1_MCU_QFN48,YES,FUNCTIONAL_PAD_TO_CREATE_IN_UNITY
U1.NC_40,U1,40,NC_40,F.Cu,-1.7727,4.8,-0.024818,1.27,0.0672,L94_U1_MCU_QFN48,YES,FUNCTIONAL_PAD_TO_CREATE_IN_UNITY
U1.NC_41,U1,41,NC_41,F.Cu,-1.0636,4.8,-0.014891,1.27,0.0672,L94_U1_MCU_QFN48,YES,FUNCTIONAL_PAD_TO_CREATE_IN_UNITY
U1.NC_42,U1,42,NC_42,F.Cu,-0.3545,4.8,-0.004964,1.27,0.0672,L94_U1_MCU_QFN48,YES,FUNCTIONAL_PAD_TO_CREATE_IN_UNITY
U1.NC_43,U1,43,NC_43,F.Cu,0.3545,4.8,0.004964,1.27,0.0672,L94_U1_MCU_QFN48,YES,FUNCTIONAL_PAD_TO_CREATE_IN_UNITY
U1.NC_44,U1,44,NC_44,F.Cu,1.0636,4.8,0.014891,1.27,0.0672,L94_U1_MCU_QFN48,YES,FUNCTIONAL_PAD_TO_CREATE_IN_UNITY
U1.NC_45,U1,45,NC_45,F.Cu,1.7727,4.8,0.024818,1.27,0.0672,L94_U1_MCU_QFN48,YES,FUNCTIONAL_PAD_TO_CREATE_IN_UNITY
U1.NC_46,U1,46,NC_46,F.Cu,2.4818,4.8,0.034745,1.27,0.0672,L94_U1_MCU_QFN48,YES,FUNCTIONAL_PAD_TO_CREATE_IN_UNITY
U1.NC_47,U1,47,NC_47,F.Cu,3.1909,4.8,0.044673,1.27,0.0672,L94_U1_MCU_QFN48,YES,FUNCTIONAL_PAD_TO_CREATE_IN_UNITY
U1.NC_48,U1,48,NC_48,F.Cu,3.9,4.8,0.0546,1.27,0.0672,L94_U1_MCU_QFN48,YES,FUNCTIONAL_PAD_TO_CREATE_IN_UNITY
U2.VIN,U2,1,VIN,F.Cu,-27.94,10.0,-0.39116,1.27,0.14,L94_U2_LDO_SOT223,YES,FUNCTIONAL_PAD_TO_CREATE_IN_UNITY
U2.GND,U2,2,GND,F.Cu,-22.06,10.0,-0.30884,1.27,0.14,L94_U2_LDO_SOT223,YES,FUNCTIONAL_PAD_TO_CREATE_IN_UNITY
U2.VOUT,U2,3,VOUT,F.Cu,-27.94,10.0,-0.39116,1.27,0.14,L94_U2_LDO_SOT223,YES,FUNCTIONAL_PAD_TO_CREATE_IN_UNITY
U2.TAB_GND,U2,4,TAB_GND,F.Cu,-22.06,10.0,-0.30884,1.27,0.14,L94_U2_LDO_SOT223,YES,FUNCTIONAL_PAD_TO_CREATE_IN_UNITY
U3.SCL,U3,1,SCL,F.Cu,22.64,7.05,0.31696,1.27,0.0987,L94_U3_I2CTemperatureSensor_SOIC8,YES,FUNCTIONAL_PAD_TO_CREATE_IN_UNITY
U3.SDA,U3,2,SDA,F.Cu,22.64,8.35,0.31696,1.27,0.1169,L94_U3_I2CTemperatureSensor_SOIC8,YES,FUNCTIONAL_PAD_TO_CREATE_IN_UNITY
U3.VDD,U3,3,VDD,F.Cu,22.64,9.65,0.31696,1.27,0.1351,L94_U3_I2CTemperatureSensor_SOIC8,YES,FUNCTIONAL_PAD_TO_CREATE_IN_UNITY
U3.GND,U3,4,GND,F.Cu,22.64,10.95,0.31696,1.27,0.1533,L94_U3_I2CTemperatureSensor_SOIC8,YES,FUNCTIONAL_PAD_TO_CREATE_IN_UNITY
U3.DEVICE_ADDR,U3,5,DEVICE_ADDR,F.Cu,29.36,7.05,0.41104,1.27,0.0987,L94_U3_I2CTemperatureSensor_SOIC8,YES,FUNCTIONAL_PAD_TO_CREATE_IN_UNITY
U3.NC_6,U3,6,NC_6,F.Cu,29.36,8.35,0.41104,1.27,0.1169,L94_U3_I2CTemperatureSensor_SOIC8,YES,FUNCTIONAL_PAD_TO_CREATE_IN_UNITY
U3.NC_7,U3,7,NC_7,F.Cu,29.36,9.65,0.41104,1.27,0.1351,L94_U3_I2CTemperatureSensor_SOIC8,YES,FUNCTIONAL_PAD_TO_CREATE_IN_UNITY
U3.NC_8,U3,8,NC_8,F.Cu,29.36,10.95,0.41104,1.27,0.1533,L94_U3_I2CTemperatureSensor_SOIC8,YES,FUNCTIONAL_PAD_TO_CREATE_IN_UNITY
U4.VDD,U4,1,VDD,F.Cu,24.64,-13.95,0.34496,1.27,-0.1953,L94_U4_CurrentMonitor_SOIC8,YES,FUNCTIONAL_PAD_TO_CREATE_IN_UNITY
U4.GND,U4,2,GND,F.Cu,24.64,-12.65,0.34496,1.27,-0.1771,L94_U4_CurrentMonitor_SOIC8,YES,FUNCTIONAL_PAD_TO_CREATE_IN_UNITY
U4.SENSE_POS,U4,3,SENSE_POS,F.Cu,24.64,-11.35,0.34496,1.27,-0.1589,L94_U4_CurrentMonitor_SOIC8,YES,FUNCTIONAL_PAD_TO_CREATE_IN_UNITY
U4.SENSE_NEG,U4,4,SENSE_NEG,F.Cu,24.64,-10.05,0.34496,1.27,-0.1407,L94_U4_CurrentMonitor_SOIC8,YES,FUNCTIONAL_PAD_TO_CREATE_IN_UNITY
U4.CURRENT_OK_OUT,U4,5,CURRENT_OK_OUT,F.Cu,31.36,-13.95,0.43904,1.27,-0.1953,L94_U4_CurrentMonitor_SOIC8,YES,FUNCTIONAL_PAD_TO_CREATE_IN_UNITY
U4.NC_6,U4,6,NC_6,F.Cu,31.36,-12.65,0.43904,1.27,-0.1771,L94_U4_CurrentMonitor_SOIC8,YES,FUNCTIONAL_PAD_TO_CREATE_IN_UNITY
U4.NC_7,U4,7,NC_7,F.Cu,31.36,-11.35,0.43904,1.27,-0.1589,L94_U4_CurrentMonitor_SOIC8,YES,FUNCTIONAL_PAD_TO_CREATE_IN_UNITY
U4.NC_8,U4,8,NC_8,F.Cu,31.36,-10.05,0.43904,1.27,-0.1407,L94_U4_CurrentMonitor_SOIC8,YES,FUNCTIONAL_PAD_TO_CREATE_IN_UNITY
Q1.G,Q1,1,G,F.Cu,10.32,-22.0,0.14448,1.27,-0.308,L94_Q1_LoadMOSFET_SOT23,YES,FUNCTIONAL_PAD_TO_CREATE_IN_UNITY
Q1.S,Q1,2,S,F.Cu,13.68,-22.0,0.19152,1.27,-0.308,L94_Q1_LoadMOSFET_SOT23,YES,FUNCTIONAL_PAD_TO_CREATE_IN_UNITY
Q1.D,Q1,3,D,F.Cu,10.32,-22.0,0.14448,1.27,-0.308,L94_Q1_LoadMOSFET_SOT23,YES,FUNCTIONAL_PAD_TO_CREATE_IN_UNITY
D1.A,D1,1,A,F.Cu,-42.1,20.0,-0.5894,1.27,0.28,L94_D1_InputSchottkySMA,YES,FUNCTIONAL_PAD_TO_CREATE_IN_UNITY
D1.K,D1,2,K,F.Cu,-37.9,20.0,-0.5306,1.27,0.28,L94_D1_InputSchottkySMA,YES,FUNCTIONAL_PAD_TO_CREATE_IN_UNITY
LED1.A,LED1,1,A,F.Cu,-38.26,-12.0,-0.53564,1.27,-0.168,L94_LED1_PowerGreen,YES,FUNCTIONAL_PAD_TO_CREATE_IN_UNITY
LED1.C,LED1,2,C,F.Cu,-35.74,-12.0,-0.50036,1.27,-0.168,L94_LED1_PowerGreen,YES,FUNCTIONAL_PAD_TO_CREATE_IN_UNITY
LED2.A,LED2,1,A,F.Cu,-36.26,-23.0,-0.50764,1.27,-0.322,L94_LED2_StatusBlue,YES,FUNCTIONAL_PAD_TO_CREATE_IN_UNITY
LED2.C,LED2,2,C,F.Cu,-33.74,-23.0,-0.47236,1.27,-0.322,L94_LED2_StatusBlue,YES,FUNCTIONAL_PAD_TO_CREATE_IN_UNITY
LED3.A,LED3,1,A,F.Cu,38.74,-27.0,0.54236,1.27,-0.378,L94_LED3_LoadAmber,YES,FUNCTIONAL_PAD_TO_CREATE_IN_UNITY
LED3.C,LED3,2,C,F.Cu,41.26,-27.0,0.57764,1.27,-0.378,L94_LED3_LoadAmber,YES,FUNCTIONAL_PAD_TO_CREATE_IN_UNITY
R1.A,R1,1,A,F.Cu,-8.26,22.0,-0.11564,1.27,0.308,L94_R1_SCLPullup_4K7,YES,FUNCTIONAL_PAD_TO_CREATE_IN_UNITY
R1.B,R1,2,B,F.Cu,-5.74,22.0,-0.08036,1.27,0.308,L94_R1_SCLPullup_4K7,YES,FUNCTIONAL_PAD_TO_CREATE_IN_UNITY
R2.A,R2,1,A,F.Cu,-0.26,22.0,-0.00364,1.27,0.308,L94_R2_SDAPullup_4K7,YES,FUNCTIONAL_PAD_TO_CREATE_IN_UNITY
R2.B,R2,2,B,F.Cu,2.26,22.0,0.03164,1.27,0.308,L94_R2_SDAPullup_4K7,YES,FUNCTIONAL_PAD_TO_CREATE_IN_UNITY
R3.A,R3,1,A,F.Cu,7.74,-18.0,0.10836,1.27,-0.252,L94_R3_GateSeries_100R,YES,FUNCTIONAL_PAD_TO_CREATE_IN_UNITY
R3.B,R3,2,B,F.Cu,10.26,-18.0,0.14364,1.27,-0.252,L94_R3_GateSeries_100R,YES,FUNCTIONAL_PAD_TO_CREATE_IN_UNITY
R4.A,R4,1,A,F.Cu,19.74,-25.0,0.27636,1.27,-0.35,L94_R4_GatePulldown_100K,YES,FUNCTIONAL_PAD_TO_CREATE_IN_UNITY
R4.B,R4,2,B,F.Cu,22.26,-25.0,0.31164,1.27,-0.35,L94_R4_GatePulldown_100K,YES,FUNCTIONAL_PAD_TO_CREATE_IN_UNITY
R5.A,R5,1,A,F.Cu,-31.26,-9.0,-0.43764,1.27,-0.126,L94_R5_PowerLED_1K,YES,FUNCTIONAL_PAD_TO_CREATE_IN_UNITY
R5.B,R5,2,B,F.Cu,-28.74,-9.0,-0.40236,1.27,-0.126,L94_R5_PowerLED_1K,YES,FUNCTIONAL_PAD_TO_CREATE_IN_UNITY
R6.A,R6,1,A,F.Cu,-28.26,-19.0,-0.39564,1.27,-0.266,L94_R6_StatusLED_330R,YES,FUNCTIONAL_PAD_TO_CREATE_IN_UNITY
R6.B,R6,2,B,F.Cu,-25.74,-19.0,-0.36036,1.27,-0.266,L94_R6_StatusLED_330R,YES,FUNCTIONAL_PAD_TO_CREATE_IN_UNITY
R7.A,R7,1,A,F.Cu,31.74,-24.0,0.44436,1.27,-0.336,L94_R7_LoadLED_330R,YES,FUNCTIONAL_PAD_TO_CREATE_IN_UNITY
R7.B,R7,2,B,F.Cu,34.26,-24.0,0.47964,1.27,-0.336,L94_R7_LoadLED_330R,YES,FUNCTIONAL_PAD_TO_CREATE_IN_UNITY
R8.A,R8,1,A,F.Cu,19.404,-13.0,0.271656,1.27,-0.182,L94_R8_Shunt_0R1,YES,FUNCTIONAL_PAD_TO_CREATE_IN_UNITY
R8.B,R8,2,B,F.Cu,22.596,-13.0,0.316344,1.27,-0.182,L94_R8_Shunt_0R1,YES,FUNCTIONAL_PAD_TO_CREATE_IN_UNITY
C1.POS,C1,1,POS,F.Cu,-34.68,15.0,-0.48552,1.27,0.21,L94_C1_InputBulk_10u,YES,FUNCTIONAL_PAD_TO_CREATE_IN_UNITY
C1.NEG,C1,2,NEG,F.Cu,-31.32,15.0,-0.43848,1.27,0.21,L94_C1_InputBulk_10u,YES,FUNCTIONAL_PAD_TO_CREATE_IN_UNITY
C2.POS,C2,1,POS,F.Cu,-17.68,13.0,-0.24752,1.27,0.182,L94_C2_OutputBulk_22u,YES,FUNCTIONAL_PAD_TO_CREATE_IN_UNITY
C2.NEG,C2,2,NEG,F.Cu,-14.32,13.0,-0.20048,1.27,0.182,L94_C2_OutputBulk_22u,YES,FUNCTIONAL_PAD_TO_CREATE_IN_UNITY
C3.POS,C3,1,POS,F.Cu,-10.26,-1.0,-0.14364,1.27,-0.014,L94_C3_MCUDecoupling_100n,YES,FUNCTIONAL_PAD_TO_CREATE_IN_UNITY
C3.NEG,C3,2,NEG,F.Cu,-7.74,-1.0,-0.10836,1.27,-0.014,L94_C3_MCUDecoupling_100n,YES,FUNCTIONAL_PAD_TO_CREATE_IN_UNITY
C4.POS,C4,1,POS,F.Cu,25.74,17.0,0.36036,1.27,0.238,L94_C4_SensorDecoupling_100n,YES,FUNCTIONAL_PAD_TO_CREATE_IN_UNITY
C4.NEG,C4,2,NEG,F.Cu,28.26,17.0,0.39564,1.27,0.238,L94_C4_SensorDecoupling_100n,YES,FUNCTIONAL_PAD_TO_CREATE_IN_UNITY
C5.POS,C5,1,POS,F.Cu,34.74,-7.0,0.48636,1.27,-0.098,L94_C5_CurrentMonitorDecoupling_100n,YES,FUNCTIONAL_PAD_TO_CREATE_IN_UNITY
C5.NEG,C5,2,NEG,F.Cu,37.26,-7.0,0.52164,1.27,-0.098,L94_C5_CurrentMonitorDecoupling_100n,YES,FUNCTIONAL_PAD_TO_CREATE_IN_UNITY
C6.POS,C6,1,POS,F.Cu,-1.26,-19.0,-0.01764,1.27,-0.266,L94_C6_ResetFilter_10n,YES,FUNCTIONAL_PAD_TO_CREATE_IN_UNITY
C6.NEG,C6,2,NEG,F.Cu,1.26,-19.0,0.01764,1.27,-0.266,L94_C6_ResetFilter_10n,YES,FUNCTIONAL_PAD_TO_CREATE_IN_UNITY
C7.POS,C7,1,POS,F.Cu,11.74,17.0,0.16436,1.27,0.238,L94_C7_AnalogFilter_1u,YES,FUNCTIONAL_PAD_TO_CREATE_IN_UNITY
C7.NEG,C7,2,NEG,F.Cu,14.26,17.0,0.19964,1.27,0.238,L94_C7_AnalogFilter_1u,YES,FUNCTIONAL_PAD_TO_CREATE_IN_UNITY
SW1.NO1,SW1,1,NO1,F.Cu,-24.4,-26.4,-0.3416,1.27,-0.3696,L94_SW1_TactileReset,YES,FUNCTIONAL_PAD_TO_CREATE_IN_UNITY
SW1.NO2,SW1,2,NO2,F.Cu,-19.6,-26.4,-0.2744,1.27,-0.3696,L94_SW1_TactileReset,YES,FUNCTIONAL_PAD_TO_CREATE_IN_UNITY
SW1.NC1,SW1,3,NC1,F.Cu,-24.4,-21.6,-0.3416,1.27,-0.3024,L94_SW1_TactileReset,YES,FUNCTIONAL_PAD_TO_CREATE_IN_UNITY
SW1.NC2,SW1,4,NC2,F.Cu,-19.6,-21.6,-0.2744,1.27,-0.3024,L94_SW1_TactileReset,YES,FUNCTIONAL_PAD_TO_CREATE_IN_UNITY
J1.5V_IN,J1,1,5V_IN,F.Cu,-48.36,4.0,-0.67704,1.27,0.056,L94_J1_Protected5VTerminal,YES,FUNCTIONAL_PAD_TO_CREATE_IN_UNITY
J1.GND,J1,2,GND,F.Cu,-41.64,4.0,-0.58296,1.27,0.056,L94_J1_Protected5VTerminal,YES,FUNCTIONAL_PAD_TO_CREATE_IN_UNITY
J2.SWDIO,J2,1,SWDIO,F.Cu,41.25,17.16,0.5775,1.27,0.24024,L94_J2_DebugProgrammingHeader,YES,FUNCTIONAL_PAD_TO_CREATE_IN_UNITY
J2.SWCLK,J2,2,SWCLK,F.Cu,41.25,19.08,0.5775,1.27,0.26712,L94_J2_DebugProgrammingHeader,YES,FUNCTIONAL_PAD_TO_CREATE_IN_UNITY
J2.GND,J2,3,GND,F.Cu,41.25,21.0,0.5775,1.27,0.294,L94_J2_DebugProgrammingHeader,YES,FUNCTIONAL_PAD_TO_CREATE_IN_UNITY
J2.3V3,J2,4,3V3,F.Cu,41.25,22.92,0.5775,1.27,0.32088,L94_J2_DebugProgrammingHeader,YES,FUNCTIONAL_PAD_TO_CREATE_IN_UNITY
J2.RESET,J2,5,RESET,F.Cu,41.25,24.84,0.5775,1.27,0.34776,L94_J2_DebugProgrammingHeader,YES,FUNCTIONAL_PAD_TO_CREATE_IN_UNITY
J2.UART_TX,J2,6,UART_TX,F.Cu,44.75,17.16,0.6265,1.27,0.24024,L94_J2_DebugProgrammingHeader,YES,FUNCTIONAL_PAD_TO_CREATE_IN_UNITY
J2.UART_RX,J2,7,UART_RX,F.Cu,44.75,19.08,0.6265,1.27,0.26712,L94_J2_DebugProgrammingHeader,YES,FUNCTIONAL_PAD_TO_CREATE_IN_UNITY
J2.KEY,J2,8,KEY,F.Cu,44.75,21.0,0.6265,1.27,0.294,L94_J2_DebugProgrammingHeader,YES,FUNCTIONAL_PAD_TO_CREATE_IN_UNITY
J2.GND2,J2,9,GND2,F.Cu,44.75,22.92,0.6265,1.27,0.32088,L94_J2_DebugProgrammingHeader,YES,FUNCTIONAL_PAD_TO_CREATE_IN_UNITY
J2.VREF,J2,10,VREF,F.Cu,44.75,24.84,0.6265,1.27,0.34776,L94_J2_DebugProgrammingHeader,YES,FUNCTIONAL_PAD_TO_CREATE_IN_UNITY
J3.A,J3,1,A,F.Cu,38,-9,0.532,1.2699999999999998,-0.126,L94_W035DetachableFeedbackJumper,YES,SERVICE_JUMPER_CONTACT
J3.B,J3,2,B,F.Cu,44,-9,0.616,1.2699999999999998,-0.126,L94_W035DetachableFeedbackJumper,YES,SERVICE_JUMPER_CONTACT
```

## ANNEX G — 23 Prefabricated Factory Nets

Source: `ProjectSpark_Level94_BoardFoundry_AAA/Data/Factory_Existing_Nets.csv`.

```csv
EndpointA,EndpointB,Status,Gameplay
U4.VDD,+3V3,PREFABRICATED,NOT A PLAYER ROUTING TASK
U4.GND,GND,PREFABRICATED,NOT A PLAYER ROUTING TASK
U4.SENSE_NEG,GND,PREFABRICATED,NOT A PLAYER ROUTING TASK
U4.CURRENT_OK_OUT,J3.A,PREFABRICATED,NOT A PLAYER ROUTING TASK
J3.B,U1.CURRENT_OK_IN,PREFABRICATED,NOT A PLAYER ROUTING TASK
U3.DEVICE_ADDR,GND,PREFABRICATED,NOT A PLAYER ROUTING TASK
U1.VSS,GND,PREFABRICATED,NOT A PLAYER ROUTING TASK
U1.RESET_N,SW1.NO1,PREFABRICATED,NOT A PLAYER ROUTING TASK
SW1.NO2,GND,PREFABRICATED,NOT A PLAYER ROUTING TASK
U1.SWDIO,J2.SWDIO,PREFABRICATED,NOT A PLAYER ROUTING TASK
U1.SWCLK,J2.SWCLK,PREFABRICATED,NOT A PLAYER ROUTING TASK
J2.GND,GND,PREFABRICATED,NOT A PLAYER ROUTING TASK
J2.3V3,+3V3,PREFABRICATED,NOT A PLAYER ROUTING TASK
C4.POS,+3V3,PREFABRICATED,NOT A PLAYER ROUTING TASK
C4.NEG,GND,PREFABRICATED,NOT A PLAYER ROUTING TASK
C5.POS,+3V3,PREFABRICATED,NOT A PLAYER ROUTING TASK
C5.NEG,GND,PREFABRICATED,NOT A PLAYER ROUTING TASK
C6.POS,U1.RESET_N,PREFABRICATED,NOT A PLAYER ROUTING TASK
C6.NEG,GND,PREFABRICATED,NOT A PLAYER ROUTING TASK
C7.POS,U1.ADC_IN,PREFABRICATED,NOT A PLAYER ROUTING TASK
C7.NEG,GND,PREFABRICATED,NOT A PLAYER ROUTING TASK
LED2.C,GND,PREFABRICATED,NOT A PLAYER ROUTING TASK
U1.VDDIO,+3V3,PREFABRICATED,NOT A PLAYER ROUTING TASK
```

## ANNEX H — Electrical Net Domains

Source: `ProjectSpark_Level94_BoardFoundry_AAA/Data/Electrical_Net_Domains.csv`.

```csv
Net,SignalKind,ControlVoltage,ProtectedDomain
+3V3,power,3.3V,YES
+5V_PROT,power,5V,YES
GND,ground,0V,NO
I2C_SCL,digital,SIGNAL,NO
I2C_SDA,digital,SIGNAL,NO
LOAD_DRIVE,power,SIGNAL,YES
LOAD_GATE_DRIVE,digital,SIGNAL,NO
LOAD_SHUNT,analog,SIGNAL,NO
LOAD_SWITCHED,power,SIGNAL,YES
MOSFET_GATE,digital,SIGNAL,NO
PWR_LED_SERIES,power,SIGNAL,YES
SHUNT_SENSE_POS,analog,SIGNAL,NO
STATUS_DRIVE,digital,SIGNAL,NO
STATUS_LED_SERIES,power,SIGNAL,YES
TRUSTED_CURRENT_OK,digital,SIGNAL,NO
U1.ADC_IN,analog,SIGNAL,NO
U1.RESET_N,digital,SIGNAL,NO
VDDIO,power,SIGNAL,YES
VIN_RAW,power,SIGNAL,YES
```

## ANNEX I — 37 Named Animation Pivots

Source: `ProjectSpark_Level94_BoardFoundry_AAA/Data/Animation_Pivots.csv`.

```csv
Model,Pivot,LocalHome,Axis,Kind,Description
L94_U1_MCU_QFN48,ANIM_U1_COMPONENT_PLACEMENT,"(0.0, 1.257, 0.0)",y,installation,U1 Component Placement
L94_U2_LDO_SOT223,ANIM_U2_COMPONENT_PLACEMENT,"(-0.35000000000000003, 1.257, 0.14)",y,installation,U2 Component Placement
L94_U3_I2CTemperatureSensor_SOIC8,ANIM_U3_COMPONENT_PLACEMENT,"(0.364, 1.257, 0.126)",y,installation,U3 Component Placement
L94_U4_CurrentMonitor_SOIC8,ANIM_U4_COMPONENT_PLACEMENT,"(0.392, 1.257, -0.168)",y,installation,U4 Component Placement
L94_Q1_LoadMOSFET_SOT23,ANIM_Q1_COMPONENT_PLACEMENT,"(0.168, 1.257, -0.308)",y,installation,Q1 Component Placement
L94_D1_InputSchottkySMA,ANIM_D1_COMPONENT_PLACEMENT,"(-0.56, 1.257, 0.28)",y,installation,D1 Component Placement
L94_LED1_PowerGreen,ANIM_LED1_COMPONENT_PLACEMENT,"(-0.518, 1.257, -0.168)",y,installation,Led1 Component Placement
L94_LED2_StatusBlue,ANIM_LED2_COMPONENT_PLACEMENT,"(-0.49, 1.257, -0.322)",y,installation,Led2 Component Placement
L94_LED3_LoadAmber,ANIM_LED3_COMPONENT_PLACEMENT,"(0.56, 1.257, -0.378)",y,installation,Led3 Component Placement
L94_R1_SCLPullup_4K7,ANIM_R1_COMPONENT_PLACEMENT,"(-0.098, 1.257, 0.308)",y,installation,R1 Component Placement
L94_R2_SDAPullup_4K7,ANIM_R2_COMPONENT_PLACEMENT,"(0.014, 1.257, 0.308)",y,installation,R2 Component Placement
L94_R3_GateSeries_100R,ANIM_R3_COMPONENT_PLACEMENT,"(0.126, 1.257, -0.252)",y,installation,R3 Component Placement
L94_R4_GatePulldown_100K,ANIM_R4_COMPONENT_PLACEMENT,"(0.294, 1.257, -0.35000000000000003)",y,installation,R4 Component Placement
L94_R5_PowerLED_1K,ANIM_R5_COMPONENT_PLACEMENT,"(-0.42, 1.257, -0.126)",y,installation,R5 Component Placement
L94_R6_StatusLED_330R,ANIM_R6_COMPONENT_PLACEMENT,"(-0.378, 1.257, -0.266)",y,installation,R6 Component Placement
L94_R7_LoadLED_330R,ANIM_R7_COMPONENT_PLACEMENT,"(0.462, 1.257, -0.336)",y,installation,R7 Component Placement
L94_R8_Shunt_0R1,ANIM_R8_COMPONENT_PLACEMENT,"(0.294, 1.257, -0.182)",y,installation,R8 Component Placement
L94_C1_InputBulk_10u,ANIM_C1_COMPONENT_PLACEMENT,"(-0.462, 1.257, 0.21)",y,installation,C1 Component Placement
L94_C2_OutputBulk_22u,ANIM_C2_COMPONENT_PLACEMENT,"(-0.224, 1.257, 0.182)",y,installation,C2 Component Placement
L94_C3_MCUDecoupling_100n,ANIM_C3_COMPONENT_PLACEMENT,"(-0.126, 1.257, -0.014)",y,installation,C3 Component Placement
L94_C4_SensorDecoupling_100n,ANIM_C4_COMPONENT_PLACEMENT,"(0.378, 1.257, 0.23800000000000002)",y,installation,C4 Component Placement
L94_C5_CurrentMonitorDecoupling_100n,ANIM_C5_COMPONENT_PLACEMENT,"(0.504, 1.257, -0.098)",y,installation,C5 Component Placement
L94_C6_ResetFilter_10n,ANIM_C6_COMPONENT_PLACEMENT,"(0.0, 1.257, -0.266)",y,installation,C6 Component Placement
L94_C7_AnalogFilter_1u,ANIM_C7_COMPONENT_PLACEMENT,"(0.182, 1.257, 0.23800000000000002)",y,installation,C7 Component Placement
L94_SW1_TactileReset,ANIM_SW1_COMPONENT_PLACEMENT,"(-0.308, 1.257, -0.336)",y,installation,Sw1 Component Placement
L94_J1_Protected5VTerminal,ANIM_J1_COMPONENT_PLACEMENT,"(-0.63, 1.257, 0.056)",y,installation,J1 Component Placement
L94_J2_DebugProgrammingHeader,ANIM_J2_COMPONENT_PLACEMENT,"(0.602, 1.257, 0.294)",y,installation,J2 Component Placement
L94_FixtureClampLeft,ANIM_LEFT_CLAMP_RELEASE,"(-0.82, 1.26, 0)",x,rotation,Left Clamp Release
L94_FixtureClampRight,ANIM_RIGHT_CLAMP_RELEASE,"(0.82, 1.26, 0)",x,rotation,Right Clamp Release
L94_InspectionMicroscopeArm,ANIM_MICROSCOPE_ARM_RAISE,"(0.36, 2.2, -0.48)",y,rotation,Microscope Arm Raise
L94_MicroscopeLensAndRingLight,ANIM_LENS_FOCUS,"(0.36, 2.1, -0.48)",y,rotation,Lens Focus
L94_W035DetachableFeedbackJumper,ANIM_W035_JUMPER_REMOVE,"(0.57, 1.35, -0.13)",y,rotation,W035 Jumper Remove
L94_BenchOscilloscope,ANIM_OSCILLOSCOPE_CONTROL,"(-1.7, 1.75, 0.35)",z,rotation,Oscilloscope Control
L94_BenchDigitalMultimeter,ANIM_MULTIMETER_DIAL,"(1.84, 1.65, 0.08)",z,rotation,Multimeter Dial
L94_ProtectedBenchPowerSupply,ANIM_SUPPLY_KNOB,"(-1.75, 1.4, 0.42)",z,rotation,Supply Knob
L94_FourChannelLogicAnalyzer,ANIM_LOGIC_ANALYZER_STATUS,"(1.7, 1.47, 0.42)",z,rotation,Logic Analyzer Status
L94_PCBAssemblySCADAPanel,ANIM_SCADA_DISPLAY,"(0, 2.0, -1.23)",z,rotation,Scada Display
```

## ANNEX J — Critical W035 Jumper Route

Source: `ProjectSpark_Level94_BoardFoundry_AAA/Data/Critical_W035_Feedback.csv`.

```csv
WireID,Type,FromComponent,FromPort,ThroughPadA,ThroughPadB,ToComponent,ToPort,VisualModel,InitialState,FaultTest,Signal,SafeState,Reset,Safety
W035,DetachableServiceJumper,U4,CURRENT_OK_OUT,J3.A,J3.B,U1,CURRENT_OK_IN,L94_W035DetachableFeedbackJumper,DISCONNECTED,Disconnect after objectives 1-5 certified,TRUSTED_CURRENT_OK,LOAD_GATE_DISABLED,Fresh measurement + explicit operator reset,Never interpret open-feedback as zero current
```

## ANNEX K — All Six Objective Certification Definitions

Source: `ProjectSpark_Level94_BoardFoundry_AAA/Data/Objective_Certification_Plan_6.csv`.

```csv
ObjectiveID,Title,RealEvidence,Authority,InitialStatus
01,Inspect PCB schematic and identify all nets,A valid schematic/netlist inspection state is recorded and no routing is pre-awarded,LevelGamePlayManager via Project Spark actual evaluator,INCOMPLETE
02,Install all 27 parts,"27 real parts in matched footprint, correct value/orientation/polarity; no fake placement",LevelGamePlayManager via Project Spark actual evaluator,INCOMPLETE
03,Route copper tasks W001-W034 and vias V01-V06,"Valid graph connectivity, 2-layer DRC, no shorted nets or open required route",LevelGamePlayManager via Project Spark actual evaluator,INCOMPLETE
04,Commission board and test,"Protected 5 V, valid 3.3 V regulator, microcontroller clock 2 Hz, sensor I2C, load driver and current samples",LevelGamePlayManager via Project Spark actual evaluator,INCOMPLETE
05,Repair 3 injected faults,"Open ground via, incorrect R2 value, solder bridge on Q1; then independent full circuit retest",LevelGamePlayManager via Project Spark actual evaluator,INCOMPLETE
06,Disconnect and restore critical W035,"Real topology break, load inhibit, new current measurement, explicit reset, new full production qualification",LevelGamePlayManager via Project Spark actual evaluator,INCOMPLETE
```

## ANNEX L — Three Fault Repair Scenarios

Source: `ProjectSpark_Level94_BoardFoundry_AAA/Data/Three_Fault_Repair_Scenarios.csv`.

```csv
FaultID,Name,Location,ElectricalEffect,Repair,Validation
F01,Open Ground Via,"V05, W030 return path",Open return / false LED operation,Rebuild actual via connection,Net continuity + powered test
F02,Incorrect Resistor Value,R2 SDA pull-up,47K instead of required 4K7; slower I2C rise time,Replace with correct 4K7 component,Measure I2C edges and acknowledge
F03,MOSFET Solder Bridge,Q1 Gate-to-Drain neighboring pads,Unwanted coupling and load-control malfunction,"Remove bridged solder defect, run DRC",Correct controlled switching and safety cutoff
```

## ANNEX M — All 56 Engineering Acceptance Test Definitions

Source: `ProjectSpark_Level94_BoardFoundry_AAA/Data/Engineering_Acceptance_Tests_56.csv`.

```csv
TestID,Requirement,Result
L94-AT-001,FBX model imports with nonempty meshes,NOT TESTED - UNITY IMPLEMENTATION REQUIRED
L94-AT-002,GLB alternative opens without missing meshes,NOT TESTED - UNITY IMPLEMENTATION REQUIRED
L94-AT-003,Material slots mapped to native Lit material,NOT TESTED - UNITY IMPLEMENTATION REQUIRED
L94-AT-004,Normal maps are imported as normal maps,NOT TESTED - UNITY IMPLEMENTATION REQUIRED
L94-AT-005,Copper pad positions match logical pad CSV,NOT TESTED - UNITY IMPLEMENTATION REQUIRED
L94-AT-006,Board display scale does not change physical dimensions,NOT TESTED - UNITY IMPLEMENTATION REQUIRED
L94-AT-007,PCB layer controls affect visuals only,NOT TESTED - UNITY IMPLEMENTATION REQUIRED
L94-AT-008,Component prefab snaps to correct footprint,NOT TESTED - UNITY IMPLEMENTATION REQUIRED
L94-AT-009,Wrong part value rejected,NOT TESTED - UNITY IMPLEMENTATION REQUIRED
L94-AT-010,Wrong polarized part orientation rejected,NOT TESTED - UNITY IMPLEMENTATION REQUIRED
L94-AT-011,Missing part prevents objective,NOT TESTED - UNITY IMPLEMENTATION REQUIRED
L94-AT-012,Pad-to-pad connection creates real solver graph edge,NOT TESTED - UNITY IMPLEMENTATION REQUIRED
L94-AT-013,Invalid circuit-net connection rejected,NOT TESTED - UNITY IMPLEMENTATION REQUIRED
L94-AT-014,Same-layer incompatible trace crossing rejected,NOT TESTED - UNITY IMPLEMENTATION REQUIRED
L94-AT-015,Via joins intended layers only,NOT TESTED - UNITY IMPLEMENTATION REQUIRED
L94-AT-016,Missing via invalidates route,NOT TESTED - UNITY IMPLEMENTATION REQUIRED
L94-AT-017,Each route begins unconnected,NOT TESTED - UNITY IMPLEMENTATION REQUIRED
L94-AT-018,All 34 routes have distinct IDs,NOT TESTED - UNITY IMPLEMENTATION REQUIRED
L94-AT-019,All 6 via IDs exist,NOT TESTED - UNITY IMPLEMENTATION REQUIRED
L94-AT-020,W035 is a detachable real graph edge,NOT TESTED - UNITY IMPLEMENTATION REQUIRED
L94-AT-021,Open W035 invalidates trusted feedback,NOT TESTED - UNITY IMPLEMENTATION REQUIRED
L94-AT-022,Open W035 never reports zero current,NOT TESTED - UNITY IMPLEMENTATION REQUIRED
L94-AT-023,Open W035 inhibits load enable,NOT TESTED - UNITY IMPLEMENTATION REQUIRED
L94-AT-024,W035 repair requires fresh sample,NOT TESTED - UNITY IMPLEMENTATION REQUIRED
L94-AT-025,W035 repair requires explicit safety reset,NOT TESTED - UNITY IMPLEMENTATION REQUIRED
L94-AT-026,5 V protected input monitors real state,NOT TESTED - UNITY IMPLEMENTATION REQUIRED
L94-AT-027,3.3 V regulator responds to load,NOT TESTED - UNITY IMPLEMENTATION REQUIRED
L94-AT-028,MCU 2 Hz output reflects power conditions,NOT TESTED - UNITY IMPLEMENTATION REQUIRED
L94-AT-029,U3 I2C exchange requires real SDA/SCL,NOT TESTED - UNITY IMPLEMENTATION REQUIRED
L94-AT-030,R1/R2 pull-ups affect rise time,NOT TESTED - UNITY IMPLEMENTATION REQUIRED
L94-AT-031,Q1 MOSFET control changes load current,NOT TESTED - UNITY IMPLEMENTATION REQUIRED
L94-AT-032,R8 current measurement updates from circuit,NOT TESTED - UNITY IMPLEMENTATION REQUIRED
L94-AT-033,Voltmeter probe reflects actual circuit,NOT TESTED - UNITY IMPLEMENTATION REQUIRED
L94-AT-034,"Scope measures signal, not animation",NOT TESTED - UNITY IMPLEMENTATION REQUIRED
L94-AT-035,Logic analyzer captures digital edges,NOT TESTED - UNITY IMPLEMENTATION REQUIRED
L94-AT-036,Open ground via actually breaks net,NOT TESTED - UNITY IMPLEMENTATION REQUIRED
L94-AT-037,Incorrect R2 value degrades signal,NOT TESTED - UNITY IMPLEMENTATION REQUIRED
L94-AT-038,Q1 bridge produces physical short,NOT TESTED - UNITY IMPLEMENTATION REQUIRED
L94-AT-039,Three faults have unique recovery evidence,NOT TESTED - UNITY IMPLEMENTATION REQUIRED
L94-AT-040,Objective 1 inspection evidence validated,NOT TESTED - UNITY IMPLEMENTATION REQUIRED
L94-AT-041,Objective 2 placement evidence validated,NOT TESTED - UNITY IMPLEMENTATION REQUIRED
L94-AT-042,Objective 3 routing evidence validated,NOT TESTED - UNITY IMPLEMENTATION REQUIRED
L94-AT-043,Objective 4 power test evidence validated,NOT TESTED - UNITY IMPLEMENTATION REQUIRED
L94-AT-044,Objective 5 fault repair evidence validated,NOT TESTED - UNITY IMPLEMENTATION REQUIRED
L94-AT-045,Objective 6 fresh test evidence validated,NOT TESTED - UNITY IMPLEMENTATION REQUIRED
L94-AT-046,Only LevelGamePlayManager awards 6/6,NOT TESTED - UNITY IMPLEMENTATION REQUIRED
L94-AT-047,Historical certifications retained after final fault,NOT TESTED - UNITY IMPLEMENTATION REQUIRED
L94-AT-048,Unity room table preview clones only visual meshes,NOT TESTED - UNITY IMPLEMENTATION REQUIRED
L94-AT-049,Preview never clones SparkTerminal or solvers,NOT TESTED - UNITY IMPLEMENTATION REQUIRED
L94-AT-050,Gameplay layout preserves Levels 1-93,NOT TESTED - UNITY IMPLEMENTATION REQUIRED
L94-AT-051,No fake preconnected wires on level start,NOT TESTED - UNITY IMPLEMENTATION REQUIRED
L94-AT-052,Safety interlocks remain effective after fault,NOT TESTED - UNITY IMPLEMENTATION REQUIRED
L94-AT-053,Runtime materials load in URP,NOT TESTED - UNITY IMPLEMENTATION REQUIRED
L94-AT-054,Runtime materials load in HDRP or documented fallback,NOT TESTED - UNITY IMPLEMENTATION REQUIRED
L94-AT-055,Runtime materials load in Built-in,NOT TESTED - UNITY IMPLEMENTATION REQUIRED
L94-AT-056,Save/load retains actual component and route states,NOT TESTED - UNITY IMPLEMENTATION REQUIRED
```

## ANNEX N — PCB Physical Design and Scale

Source: `ProjectSpark_Level94_BoardFoundry_AAA/Data/PCB_Physical_Design.json`.

```json
{
  "Level": 94,
  "BoardName": "Board Diagnostic Controller",
  "BoardWidthMM": 100,
  "BoardLengthMM": 70,
  "ThicknessMM": 1.6,
  "Layers": [
    "F.Cu",
    "B.Cu"
  ],
  "InputV": 5,
  "LogicV": 3.3,
  "LogicHz": 2.0,
  "ArtScaleMultiplier": 14.0,
  "ArtIsGameplayAuthority": false,
  "DeclaredCopperTasks": 34,
  "DeclaredVias": 6,
  "RequiredComponentPlacements": 27,
  "CriticalDisconnect": "W035",
  "ElectricalNotes": "Power supply is a virtual protected 5 V source. Higher-energy real tools and hardware are not exposed."
}
```

## ANNEX O — Circuit Graph Interface Contract

Source: `ProjectSpark_Level94_BoardFoundry_AAA/Data/Circuit_Graph_Interface_Contract.json`.

```json
{
  "required_interfaces": [
    "SparkTerminal",
    "SparkCircuitSystem",
    "SparkElectricalSolver",
    "LevelGamePlayManager"
  ],
  "source_status": "INTEGRATION PLAN ONLY - inspect actual user Project Spark assemblies before compiling",
  "nodes": 127,
  "player_required_routes": 34,
  "physical_critical_jumper": "W035",
  "shared_nets": "Connections on same logical net may join only through compatible solver rules",
  "do_not_use": [
    "decorative wire as circuit",
    "gameplay state from renderers",
    "fake 6/6 result"
  ],
  "instruments": [
    "DMM",
    "Oscilloscope",
    "Logic Analyzer",
    "Protected Supply"
  ],
  "safety": [
    "No hot work simulation",
    "Protected 5 V educational input",
    "Independent feedback validation"
  ]
}
```

## ANNEX P — Existing Unity Material Bindings

Source: `ProjectSpark_Level94_BoardFoundry_AAA/Materials/PBR_Material_Bindings.json`.

```json
{
  "source_material_map": {
    "frame": "TitaniumBench",
    "steel": "TitaniumBench",
    "aluminum": "TitaniumBench",
    "graphite": "AnodizedGraphite",
    "rubber": "ESDPolymer",
    "fr4": "FiberglassFR4",
    "mask": "DeepGreenSoldermask",
    "maskdark": "DeepGreenSoldermask",
    "copper": "RolledCopper",
    "gold": "ENIGGold",
    "solder": "LeadFreeSolder",
    "ceramic": "CeramicWhite",
    "whiteink": "SilkscreenWhite",
    "resistor": "ResistorCarbon",
    "capacitor": "MLCCDielectric",
    "ic": "ICBlackMold",
    "black": "ICBlackMold",
    "glass": "GlassLens",
    "panel": "SCADADisplay",
    "cyan": "NeonCyan",
    "holo": "NeonCyan",
    "green": "StatusGreen",
    "amber": "StatusAmber",
    "orange": "StatusAmber",
    "red": "StatusRed",
    "blue": "StatusBlue"
  },
  "sets": [
    {
      "MaterialSet": "TitaniumBench",
      "AlbedoRGB": "#84919F",
      "Metallic": 0.84,
      "Roughness": 0.3,
      "Pattern": "brushed",
      "TextureResolution": 1024,
      "Textures": 6,
      "UsedBy": "frame;steel;aluminum"
    },
    {
      "MaterialSet": "AnodizedGraphite",
      "AlbedoRGB": "#222D37",
      "Metallic": 0.65,
      "Roughness": 0.43,
      "Pattern": "brushed",
      "TextureResolution": 1024,
      "Textures": 6,
      "UsedBy": "graphite"
    },
    {
      "MaterialSet": "ESDPolymer",
      "AlbedoRGB": "#171D21",
      "Metallic": 0.02,
      "Roughness": 0.76,
      "Pattern": "rubber",
      "TextureResolution": 1024,
      "Textures": 6,
      "UsedBy": "rubber"
    },
    {
      "MaterialSet": "FiberglassFR4",
      "AlbedoRGB": "#5C6A3B",
      "Metallic": 0.03,
      "Roughness": 0.73,
      "Pattern": "fiberglass",
      "TextureResolution": 1024,
      "Textures": 6,
      "UsedBy": "fr4"
    },
    {
      "MaterialSet": "DeepGreenSoldermask",
      "AlbedoRGB": "#0D6938",
      "Metallic": 0.03,
      "Roughness": 0.3,
      "Pattern": "pcbmask",
      "TextureResolution": 1024,
      "Textures": 6,
      "UsedBy": "mask;maskdark"
    },
    {
      "MaterialSet": "RolledCopper",
      "AlbedoRGB": "#D47730",
      "Metallic": 0.91,
      "Roughness": 0.34,
      "Pattern": "brushed",
      "TextureResolution": 1024,
      "Textures": 6,
      "UsedBy": "copper"
    },
    {
      "MaterialSet": "ENIGGold",
      "AlbedoRGB": "#E5B745",
      "Metallic": 0.97,
      "Roughness": 0.2,
      "Pattern": "brushed",
      "TextureResolution": 1024,
      "Textures": 6,
      "UsedBy": "gold"
    },
    {
      "MaterialSet": "LeadFreeSolder",
      "AlbedoRGB": "#BEC6CA",
      "Metallic": 0.87,
      "Roughness": 0.27,
      "Pattern": "brushed",
      "TextureResolution": 1024,
      "Textures": 6,
      "UsedBy": "solder"
    },
    {
      "MaterialSet": "CeramicWhite",
      "AlbedoRGB": "#E1E2CF",
      "Metallic": 0.1,
      "Roughness": 0.58,
      "Pattern": "ceramic",
      "TextureResolution": 1024,
      "Textures": 6,
      "UsedBy": "ceramic"
    },
    {
      "MaterialSet": "SilkscreenWhite",
      "AlbedoRGB": "#E5F0E1",
      "Metallic": 0.0,
      "Roughness": 0.78,
      "Pattern": "ink",
      "TextureResolution": 1024,
      "Textures": 6,
      "UsedBy": "whiteink"
    },
    {
      "MaterialSet": "ResistorCarbon",
      "AlbedoRGB": "#432B20",
      "Metallic": 0.05,
      "Roughness": 0.61,
      "Pattern": "carbon",
      "TextureResolution": 1024,
      "Textures": 6,
      "UsedBy": "resistor"
    },
    {
      "MaterialSet": "MLCCDielectric",
      "AlbedoRGB": "#9F8E56",
      "Metallic": 0.13,
      "Roughness": 0.42,
      "Pattern": "ceramic",
      "TextureResolution": 1024,
      "Textures": 6,
      "UsedBy": "capacitor"
    },
    {
      "MaterialSet": "ICBlackMold",
      "AlbedoRGB": "#12151A",
      "Metallic": 0.06,
      "Roughness": 0.47,
      "Pattern": "carbon",
      "TextureResolution": 1024,
      "Textures": 6,
      "UsedBy": "ic;black"
    },
    {
      "MaterialSet": "GlassLens",
      "AlbedoRGB": "#379EC0",
      "Metallic": 0.04,
      "Roughness": 0.14,
      "Pattern": "glass",
      "TextureResolution": 1024,
      "Textures": 6,
      "UsedBy": "glass"
    },
    {
      "MaterialSet": "SCADADisplay",
      "AlbedoRGB": "#092040",
      "Metallic": 0.02,
      "Roughness": 0.28,
      "Pattern": "holo",
      "TextureResolution": 1024,
      "Textures": 6,
      "UsedBy": "panel"
    },
    {
      "MaterialSet": "NeonCyan",
      "AlbedoRGB": "#15B5F8",
      "Metallic": 0.0,
      "Roughness": 0.2,
      "Pattern": "holo",
      "TextureResolution": 1024,
      "Textures": 6,
      "UsedBy": "cyan;holo"
    },
    {
      "MaterialSet": "StatusGreen",
      "AlbedoRGB": "#1FE45A",
      "Metallic": 0.0,
      "Roughness": 0.2,
      "Pattern": "holo",
      "TextureResolution": 1024,
      "Textures": 6,
      "UsedBy": "green"
    },
    {
      "MaterialSet": "StatusAmber",
      "AlbedoRGB": "#F4841C",
      "Metallic": 0.0,
      "Roughness": 0.27,
      "Pattern": "holo",
      "TextureResolution": 1024,
      "Textures": 6,
      "UsedBy": "amber;orange"
    },
    {
      "MaterialSet": "StatusRed",
      "AlbedoRGB": "#EC352B",
      "Metallic": 0.0,
      "Roughness": 0.24,
      "Pattern": "holo",
      "TextureResolution": 1024,
      "Textures": 6,
      "UsedBy": "red"
    },
    {
      "MaterialSet": "StatusBlue",
      "AlbedoRGB": "#177BF0",
      "Metallic": 0.0,
      "Roughness": 0.21,
      "Pattern": "holo",
      "TextureResolution": 1024,
      "Textures": 6,
      "UsedBy": "blue"
    }
  ],
  "unity_shader_compatibility": [
    "Built-in Standard",
    "URP Lit",
    "HDRP Lit"
  ],
  "unity_masks": "Red=Metallic Green=AO Alpha=Smoothness",
  "normal_maps": "Tangent style procedural reference; final production baking recommended",
  "textures_are_embedded_in_glbs": false
}
```

## ANNEX Q — Source 20-Material Reference Map

Source: `ProjectSpark_Level94_BoardFoundry_AAA/Materials/Material_Reference_Map.csv`.

```csv
SourceFBXMaterial,PBRMaterialSet,PhysicalMeaning
PS_aluminum,TitaniumBench,Original PBR material reference
PS_amber,StatusAmber,Original PBR material reference
PS_black,ICBlackMold,Original PBR material reference
PS_blue,StatusBlue,Original PBR material reference
PS_capacitor,MLCCDielectric,Original PBR material reference
PS_ceramic,CeramicWhite,Original PBR material reference
PS_copper,RolledCopper,Original PBR material reference
PS_cyan,NeonCyan,Original PBR material reference
PS_fr4,FiberglassFR4,Original PBR material reference
PS_frame,TitaniumBench,Original PBR material reference
PS_glass,GlassLens,Original PBR material reference
PS_gold,ENIGGold,Original PBR material reference
PS_graphite,AnodizedGraphite,Original PBR material reference
PS_green,StatusGreen,Original PBR material reference
PS_holo,NeonCyan,Original PBR material reference
PS_ic,ICBlackMold,Original PBR material reference
PS_mask,DeepGreenSoldermask,Original PBR material reference
PS_maskdark,DeepGreenSoldermask,Original PBR material reference
PS_orange,StatusAmber,Original PBR material reference
PS_panel,SCADADisplay,Original PBR material reference
PS_red,StatusRed,Original PBR material reference
PS_resistor,ResistorCarbon,Original PBR material reference
PS_rubber,ESDPolymer,Original PBR material reference
PS_solder,LeadFreeSolder,Original PBR material reference
PS_steel,TitaniumBench,Original PBR material reference
PS_whiteink,SilkscreenWhite,Original PBR material reference
```

## ANNEX R — Source Texture Manifest

Source: `ProjectSpark_Level94_BoardFoundry_AAA/Materials/PBR_Texture_Manifest.csv`.

```csv
MaterialSet,AlbedoRGB,Metallic,Roughness,Pattern,TextureResolution,Textures,UsedBy
TitaniumBench,#84919F,0.84,0.3,brushed,1024,6,frame;steel;aluminum
AnodizedGraphite,#222D37,0.65,0.43,brushed,1024,6,graphite
ESDPolymer,#171D21,0.02,0.76,rubber,1024,6,rubber
FiberglassFR4,#5C6A3B,0.03,0.73,fiberglass,1024,6,fr4
DeepGreenSoldermask,#0D6938,0.03,0.3,pcbmask,1024,6,mask;maskdark
RolledCopper,#D47730,0.91,0.34,brushed,1024,6,copper
ENIGGold,#E5B745,0.97,0.2,brushed,1024,6,gold
LeadFreeSolder,#BEC6CA,0.87,0.27,brushed,1024,6,solder
CeramicWhite,#E1E2CF,0.1,0.58,ceramic,1024,6,ceramic
SilkscreenWhite,#E5F0E1,0.0,0.78,ink,1024,6,whiteink
ResistorCarbon,#432B20,0.05,0.61,carbon,1024,6,resistor
MLCCDielectric,#9F8E56,0.13,0.42,ceramic,1024,6,capacitor
ICBlackMold,#12151A,0.06,0.47,carbon,1024,6,ic;black
GlassLens,#379EC0,0.04,0.14,glass,1024,6,glass
SCADADisplay,#092040,0.02,0.28,holo,1024,6,panel
NeonCyan,#15B5F8,0.0,0.2,holo,1024,6,cyan;holo
StatusGreen,#1FE45A,0.0,0.2,holo,1024,6,green
StatusAmber,#F4841C,0.0,0.27,holo,1024,6,amber;orange
StatusRed,#EC352B,0.0,0.24,holo,1024,6,red
StatusBlue,#177BF0,0.0,0.21,holo,1024,6,blue
```

## ANNEX S1 — Existing Art Assembler (Art Only)

Original source (NOT a functioning circuit solver) from `ProjectSpark_Level94_BoardFoundry_AAA/Unity/Editor/SparkLevel94ArtAssembler.cs`. If it conflicts with existing user code, reconcile explicitly.

```csharp
#if UNITY_EDITOR
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
namespace ProjectSpark.Level94.EditorTools
{
    /// <summary>Art-only assembly of modular binary FBX files. Never adds or modifies gameplay components.</summary>
    public static class SparkLevel94ArtAssembler
    {
        public const string RootName="L94_BOARD_FOUNDRY_VISUALS";
        public static readonly string[] Names = {
            "L94_AssemblyBenchFrame",
            "L94_AntiStaticWorkSurface",
            "L94_PrecisionFixtureBase",
            "L94_FixtureClampLeft",
            "L94_FixtureClampRight",
            "L94_InspectionMicroscopeArm",
            "L94_MicroscopeLensAndRingLight",
            "L94_ComponentRepairTray",
            "L94_FR4BoardSubstrate",
            "L94_TopGreenSoldermask",
            "L94_BottomGreenSoldermask",
            "L94_TopSMDGoldPads",
            "L94_BottomCopperPadSet",
            "L94_SilkscreenAndTestFiducials",
            "L94_CopperEdgeAndMountBosses",
            "L94_W035DetachableFeedbackJumper",
            "L94_U1_MCU_QFN48",
            "L94_U2_LDO_SOT223",
            "L94_U3_I2CTemperatureSensor_SOIC8",
            "L94_U4_CurrentMonitor_SOIC8",
            "L94_Q1_LoadMOSFET_SOT23",
            "L94_D1_InputSchottkySMA",
            "L94_LED1_PowerGreen",
            "L94_LED2_StatusBlue",
            "L94_LED3_LoadAmber",
            "L94_R1_SCLPullup_4K7",
            "L94_R2_SDAPullup_4K7",
            "L94_R3_GateSeries_100R",
            "L94_R4_GatePulldown_100K",
            "L94_R5_PowerLED_1K",
            "L94_R6_StatusLED_330R",
            "L94_R7_LoadLED_330R",
            "L94_R8_Shunt_0R1",
            "L94_C1_InputBulk_10u",
            "L94_C2_OutputBulk_22u",
            "L94_C3_MCUDecoupling_100n",
            "L94_C4_SensorDecoupling_100n",
            "L94_C5_CurrentMonitorDecoupling_100n",
            "L94_C6_ResetFilter_10n",
            "L94_C7_AnalogFilter_1u",
            "L94_SW1_TactileReset",
            "L94_J1_Protected5VTerminal",
            "L94_J2_DebugProgrammingHeader",
            "L94_BenchOscilloscope",
            "L94_BenchDigitalMultimeter",
            "L94_ProtectedBenchPowerSupply",
            "L94_FourChannelLogicAnalyzer",
            "L94_PCBAssemblySCADAPanel"
        };
        [MenuItem("Tools/Project Spark/Levels/94/2. Assemble 48 Modular FBXs Under Selected Layout")]
        public static void Assemble()
        {
            GameObject selected=Selection.activeGameObject;
            if(selected==null){Debug.LogError("[Level94] Select a Level94 layout GameObject.");return;}
            var source=new Dictionary<string,string>(StringComparer.OrdinalIgnoreCase);
            foreach(var guid in AssetDatabase.FindAssets("t:Model"))
            {
                string path=AssetDatabase.GUIDToAssetPath(guid);
                if(!path.EndsWith(".fbx",StringComparison.OrdinalIgnoreCase))continue;
                string stem=Path.GetFileNameWithoutExtension(path);
                int cut=stem.IndexOf("_L94_",StringComparison.OrdinalIgnoreCase);
                if(cut<0)continue;
                string name=stem.Substring(cut+1);
                if(!source.ContainsKey(name))source[name]=path;
            }
            var missing=Names.Where(n=>!source.ContainsKey(n)).ToArray();
            if(missing.Length>0){Debug.LogError("[Level94] Missing "+missing.Length+" FBXs; examples: "+String.Join(", ",missing.Take(6).ToArray()));return;}
            Transform existing=selected.transform.Find(RootName);
            if(existing!=null)Undo.DestroyObjectImmediate(existing.gameObject);
            GameObject root=new GameObject(RootName);
            Undo.RegisterCreatedObjectUndo(root,"Create Level94 visual root");root.transform.SetParent(selected.transform,false);
            root.transform.localPosition=Vector3.zero;root.transform.localRotation=Quaternion.identity;root.transform.localScale=Vector3.one;
            var groups=new Dictionary<string,Transform>();
            foreach(string gn in new[]{"00_Bench","01_BareBoard","02_Components_27","03_TestInstruments"})
            {
                var grp=new GameObject(gn);Undo.RegisterCreatedObjectUndo(grp,"Create Level94 group");
                grp.transform.SetParent(root.transform,false);groups[gn]=grp.transform;
            }
            for(int i=0;i<Names.Length;i++)
            {
                var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(source[Names[i]]);
                if(prefab==null){Debug.LogError("[Level94] Could not load "+Names[i]);continue;}
                var part=(GameObject)PrefabUtility.InstantiatePrefab(prefab);
                if(part==null)continue;
                var group=groups[i<8?"00_Bench":i<16?"01_BareBoard":i<43?"02_Components_27":"03_TestInstruments"];
                part.transform.SetParent(group,false);part.transform.localPosition=Vector3.zero;
                part.transform.localRotation=Quaternion.identity;part.transform.localScale=Vector3.one;
                part.name=Names[i];
            }
            SparkLevel94PBRMaterialEditor.ApplyMaterials(root);
            Selection.activeGameObject=root;
            Debug.Log("[Level94] Assembled 48 modular FBX visual models. This scene does not create actual electrical circuits, component placement tasks or objectives.");
        }
    }
}
#endif
```

## ANNEX S2 — Existing Native Material Generator

Original source (NOT a functioning circuit solver) from `ProjectSpark_Level94_BoardFoundry_AAA/Unity/Editor/SparkLevel94PBRMaterialEditor.cs`. If it conflicts with existing user code, reconcile explicitly.

```csharp
#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
namespace ProjectSpark.Level94.EditorTools
{
    /// <summary>Converts the original Level94 PBR maps to editable native Unity materials.
    /// No gameplay state is read or changed. URP/HDRP/Built-In Lit shader chosen at import time.</summary>
    public static class SparkLevel94PBRMaterialEditor
    {
        public const string OutputPath="Assets/ProjectSpark/Level94/GeneratedMaterials";
        private static readonly Dictionary<string,string> Sets = new Dictionary<string,string>(StringComparer.OrdinalIgnoreCase) {
            {"PS_aluminum","TitaniumBench"},
            {"PS_amber","StatusAmber"},
            {"PS_black","ICBlackMold"},
            {"PS_blue","StatusBlue"},
            {"PS_capacitor","MLCCDielectric"},
            {"PS_ceramic","CeramicWhite"},
            {"PS_copper","RolledCopper"},
            {"PS_cyan","NeonCyan"},
            {"PS_fr4","FiberglassFR4"},
            {"PS_frame","TitaniumBench"},
            {"PS_glass","GlassLens"},
            {"PS_gold","ENIGGold"},
            {"PS_graphite","AnodizedGraphite"},
            {"PS_green","StatusGreen"},
            {"PS_holo","NeonCyan"},
            {"PS_ic","ICBlackMold"},
            {"PS_mask","DeepGreenSoldermask"},
            {"PS_maskdark","DeepGreenSoldermask"},
            {"PS_orange","StatusAmber"},
            {"PS_panel","SCADADisplay"},
            {"PS_red","StatusRed"},
            {"PS_resistor","ResistorCarbon"},
            {"PS_rubber","ESDPolymer"},
            {"PS_solder","LeadFreeSolder"},
            {"PS_steel","TitaniumBench"},
            {"PS_whiteink","SilkscreenWhite"}
        };
        private static readonly Dictionary<string,Material> Cache = new Dictionary<string,Material>(StringComparer.OrdinalIgnoreCase);
        public static void EnsureFolder(string folder)
        {
            if (folder=="Assets" || AssetDatabase.IsValidFolder(folder)) return;
            int sep=folder.LastIndexOf('/');
            if (sep<1) throw new Exception("Expected Assets/ path");
            string parent=folder.Substring(0,sep);EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent,folder.Substring(sep+1));
        }
        public static string FindPathExact(string filename)
        {
            string stem=Path.GetFileNameWithoutExtension(filename);
            foreach(string guid in AssetDatabase.FindAssets(stem))
            {
                string p=AssetDatabase.GUIDToAssetPath(guid);
                if(Path.GetFileName(p).Equals(filename,StringComparison.OrdinalIgnoreCase)
                    && p.IndexOf("Level94",StringComparison.OrdinalIgnoreCase)>=0) return p;
            }
            return null;
        }
        public static Texture2D LoadMap(string name,bool isNormal=false)
        {
            string p=FindPathExact(name+".png"); if(String.IsNullOrEmpty(p))return null;
            var importer=AssetImporter.GetAtPath(p) as TextureImporter;
            if(isNormal && importer!=null && importer.textureType!=TextureImporterType.NormalMap)
            {
                importer.textureType=TextureImporterType.NormalMap;
                importer.SaveAndReimport();
            }
            return AssetDatabase.LoadAssetAtPath<Texture2D>(p);
        }
        static string Pipeline()
        {
            var a=GraphicsSettings.currentRenderPipeline;
            if(a==null)return "BuiltIn";
            string n=a.GetType().Name.ToLowerInvariant();
            return n.Contains("hd") ? "HDRP" : "URP";
        }
        static void SetTex(Material m, string key, Texture2D tex)
        {
            if(tex!=null && m.HasProperty(key))m.SetTexture(key,tex);
        }
        public static Dictionary<string,Material> EnsureMaterials()
        {
            EnsureFolder(OutputPath);
            Cache.Clear();
            string pipe=Pipeline();
            string shaderName=pipe=="URP"?"Universal Render Pipeline/Lit":pipe=="HDRP"?"HDRP/Lit":"Standard";
            var lit=Shader.Find(shaderName);
            if(lit==null) { Debug.LogError("[Level94] No native Lit shader: "+shaderName);return Cache; }
            foreach(var kv in Sets)
            {
                string source=kv.Key, set=kv.Value;
                string target=OutputPath+"/"+source+".mat";
                var m=AssetDatabase.LoadAssetAtPath<Material>(target);
                if(m==null) { m=new Material(lit){name=source};AssetDatabase.CreateAsset(m,target); }
                var hue=source.ToLowerInvariant();
                bool glass=(hue=="ps_glass");
                bool emissive=hue=="ps_cyan"||hue=="ps_holo"||hue=="ps_green"||hue=="ps_amber"||hue=="ps_orange"||hue=="ps_red"||hue=="ps_blue";
                Shader desired=lit;
                if(hue=="ps_holo")
                {
                    string custom=pipe=="URP"?"ProjectSpark/Level94/HologramURP":pipe=="BuiltIn"?"ProjectSpark/Level94/HologramBuiltin":"HDRP/Unlit";
                    var sh=Shader.Find(custom); if(sh!=null)desired=sh;
                }
                m.shader=desired;
                Texture2D albedo=LoadMap(set+"_BaseColor");
                Texture2D normal=LoadMap(set+"_Normal",true);
                Texture2D mask=LoadMap(set+"_UnityMaskMap");
                Texture2D ao=LoadMap(set+"_Occlusion");
                Texture2D em=LoadMap(set+"_Emission");
                if(pipe=="URP")
                {
                    SetTex(m,"_BaseMap",albedo);SetTex(m,"_BumpMap",normal);
                    SetTex(m,"_MetallicGlossMap",mask);SetTex(m,"_OcclusionMap",ao);SetTex(m,"_EmissionMap",em);
                    if(m.HasProperty("_Surface"))m.SetFloat("_Surface",glass?1:0);
                    if(m.HasProperty("_Smoothness"))m.SetFloat("_Smoothness",1);
                }
                else if(pipe=="HDRP")
                {
                    SetTex(m,"_BaseColorMap",albedo);SetTex(m,"_NormalMap",normal);
                    SetTex(m,"_MaskMap",mask);SetTex(m,"_EmissiveColorMap",em);
                    if(m.HasProperty("_SurfaceType"))m.SetFloat("_SurfaceType",glass?1:0);
                }
                else
                {
                    SetTex(m,"_MainTex",albedo);SetTex(m,"_BumpMap",normal);
                    SetTex(m,"_MetallicGlossMap",mask);SetTex(m,"_OcclusionMap",ao);SetTex(m,"_EmissionMap",em);
                }
                if(m.HasProperty("_BaseColor"))m.SetColor("_BaseColor",Color.white);
                if(m.HasProperty("_Color"))m.SetColor("_Color",Color.white);
                if(m.HasProperty("_Tint"))m.SetColor("_Tint",new Color(.12f,.88f,1f,.55f));
                if(normal!=null)m.EnableKeyword("_NORMALMAP");
                if(mask!=null)m.EnableKeyword("_METALLICGLOSSMAP");
                if(emissive) {
                    if(m.HasProperty("_EmissionColor"))m.SetColor("_EmissionColor",Color.white*2.0f);
                    if(m.HasProperty("_EmissiveColor"))m.SetColor("_EmissiveColor",Color.white*2.0f);
                    m.EnableKeyword("_EMISSION");
                }
                if(glass) { m.renderQueue=(int)RenderQueue.Transparent; m.SetOverrideTag("RenderType","Transparent"); }
                EditorUtility.SetDirty(m);Cache[source]=m;
            }
            AssetDatabase.SaveAssets();
            Debug.Log("[Level94] Created/updated "+Cache.Count+" native PBR materials for "+pipe+". Actual render outcome must be verified in Unity.");
            return Cache;
        }
        [MenuItem("Tools/Project Spark/Levels/94/1. Create Native PBR Materials")]
        public static void CreateFromMenu(){EnsureMaterials();}
        public static void ApplyMaterials(GameObject target)
        {
            if(target==null)return;
            var map=EnsureMaterials();
            int applied=0,missing=0;
            foreach(var render in target.GetComponentsInChildren<Renderer>(true))
            {
                var arr=render.sharedMaterials;bool changed=false;
                for(int i=0;i<arr.Length;i++)
                {
                    if(arr[i]==null)continue;
                    string key=arr[i].name.Replace(" (Instance)","").Trim();
                    if(map.TryGetValue(key,out var m)) {arr[i]=m;applied++;changed=true;} else missing++;
                }
                if(changed)render.sharedMaterials=arr;
            }
            Debug.Log("[Level94] Bound "+applied+" PBR material slots; unmatched "+missing+" (check FBX material names).");
        }
        [MenuItem("Tools/Project Spark/Levels/94/3. Apply PBR To Selected Art")]
        public static void ApplySelected(){if(Selection.activeGameObject!=null)ApplyMaterials(Selection.activeGameObject);else Debug.LogError("[Level94] Select Level94 art first.");}
    }
}
#endif
```

## ANNEX S3 — Existing PCB Planning-Data Importer

Original source (NOT a functioning circuit solver) from `ProjectSpark_Level94_BoardFoundry_AAA/Unity/Editor/SparkLevel94PlanningDataEditor.cs`. If it conflicts with existing user code, reconcile explicitly.

```csharp
#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;
namespace ProjectSpark.Level94.EditorTools
{
    public static class SparkLevel94PlanningDataEditor
    {
        private static readonly string Out="Assets/ProjectSpark/Level94/Data/L94_PCBDefinition.asset";
        private static TextAsset Find(string filename)
        {
            foreach(var guid in AssetDatabase.FindAssets(Path.GetFileNameWithoutExtension(filename)))
            {
                string path=AssetDatabase.GUIDToAssetPath(guid);
                if(Path.GetFileName(path).Equals(filename,StringComparison.OrdinalIgnoreCase) && path.Contains("Level94"))
                    return AssetDatabase.LoadAssetAtPath<TextAsset>(path);
            }
            return null;
        }
        [MenuItem("Tools/Project Spark/Levels/94/4. Create PCB Planning Data Asset")]
        public static void Create()
        {
            SparkLevel94PBRMaterialEditor.EnsureFolder("Assets/ProjectSpark/Level94/Data");
            var asset=AssetDatabase.LoadAssetAtPath<ProjectSpark.Level94.SparkLevel94PCBDefinition>(Out);
            if(asset==null){asset=ScriptableObject.CreateInstance<ProjectSpark.Level94.SparkLevel94PCBDefinition>();AssetDatabase.CreateAsset(asset,Out);}
            asset.componentBOM=Find("Component_BOM_27.csv");
            asset.copperTraceTasks=Find("Copper_Trace_Tasks_W001_W034.csv");
            asset.viaTasks=Find("Required_Via_Placements_V01_V06.csv");
            asset.logicalPadAnchors=Find("Logical_Pad_Locations.csv");
            asset.factoryNetlist=Find("Factory_Existing_Nets.csv");
            asset.w035CriticalJumper=Find("Critical_W035_Feedback.csv");
            asset.objectiveRequirements=Find("Objective_Certification_Plan_6.csv");
            asset.faultScenarios=Find("Three_Fault_Repair_Scenarios.csv");
            asset.testPlan=Find("Engineering_Acceptance_Tests_56.csv");
            EditorUtility.SetDirty(asset);AssetDatabase.SaveAssets();Selection.activeObject=asset;
            Debug.Log("[Level94] Created PCB source-data asset. CSVs describe intended component/trace tasks, not live gameplay connections.");
        }
    }
}
#endif
```

## ANNEX S4 — Existing Art/Source Validator

Original source (NOT a functioning circuit solver) from `ProjectSpark_Level94_BoardFoundry_AAA/Unity/Editor/SparkLevel94ArtValidator.cs`. If it conflicts with existing user code, reconcile explicitly.

```csharp
#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
namespace ProjectSpark.Level94.EditorTools
{
    public static class SparkLevel94ArtValidator
    {
        [MenuItem("Tools/Project Spark/Levels/94/5. Validate Imported Assets And Plans")]
        public static void Validate()
        {
            int modelCount=0,invalid=0;
            foreach(string guid in AssetDatabase.FindAssets("t:Model"))
            {
                string path=AssetDatabase.GUIDToAssetPath(guid);
                if(!Path.GetFileNameWithoutExtension(path).Contains("_L94_"))continue;
                if(path.EndsWith(".fbx",StringComparison.OrdinalIgnoreCase))modelCount++;
            }
            if(modelCount!=48){Debug.LogError("[Level94] Expected 48 modular FBXs; found "+modelCount);invalid++;}
            string[] required={"Component_BOM_27.csv","Copper_Trace_Tasks_W001_W034.csv","Required_Via_Placements_V01_V06.csv","Logical_Pad_Locations.csv","Critical_W035_Feedback.csv","Objective_Certification_Plan_6.csv"};
            foreach(string file in required)
            {
                if(SparkLevel94PBRMaterialEditor.FindPathExact(file)==null){Debug.LogError("[Level94] Missing "+file);invalid++;}
            }
            var root=Selection.activeGameObject;
            if(root!=null)
            {
                int meshes=root.GetComponentsInChildren<MeshFilter>(true).Count(m=>m.sharedMesh!=null);
                Debug.Log("[Level94] Selected visual root contains "+meshes+" mesh filters.");
            }
            if(invalid==0)Debug.Log("[Level94] Imported art/data presence checks passed. This DOES NOT validate PCB electrical gameplay or Unity Play Mode.");
            else Debug.LogError("[Level94] Validation failed with "+invalid+" missing/incorrect asset groups.");
        }
    }
}
#endif
```

## ANNEX S5 — Existing Runtime PCB Definition Asset

Original source (NOT a functioning circuit solver) from `ProjectSpark_Level94_BoardFoundry_AAA/Unity/SparkLevel94PCBDefinition.cs`. If it conflicts with existing user code, reconcile explicitly.

```csharp
using UnityEngine;
namespace ProjectSpark.Level94
{
    /// <summary>Text assets authored in CSV, assigned manually or by Editor importer.
    /// Source-of-truth planning data only; actual SparkTerminal and circuit graph must be built separately.</summary>
    [CreateAssetMenu(fileName="L94_PCBDefinition",menuName="Project Spark/Level94/PCB Planning Data")]
    public sealed class SparkLevel94PCBDefinition : ScriptableObject
    {
        public Vector2 physicalBoardSizeMm=new Vector2(100,70);
        public float physicalThicknessMm=1.6f;
        public int copperLayers=2;
        public float visualPresentationScale=14f;
        public TextAsset componentBOM;
        public TextAsset copperTraceTasks;
        public TextAsset viaTasks;
        public TextAsset logicalPadAnchors;
        public TextAsset factoryNetlist;
        public TextAsset w035CriticalJumper;
        public TextAsset objectiveRequirements;
        public TextAsset faultScenarios;
        public TextAsset testPlan;
    }
}
```

## ANNEX S6 — Existing Visual-Only Board Controller

Original source (NOT a functioning circuit solver) from `ProjectSpark_Level94_BoardFoundry_AAA/Unity/SparkLevel94BoardVisualController.cs`. If it conflicts with existing user code, reconcile explicitly.

```csharp
using UnityEngine;
namespace ProjectSpark.Level94
{
    /// <summary>Visual-only toggles for the board's inspection modes.
    /// No authority over gameplay, component placement, copper nets, voltage or objective success.</summary>
    [DisallowMultipleComponent]
    public sealed class SparkLevel94BoardVisualController : MonoBehaviour
    {
        [SerializeField] private Transform visualRoot;
        [SerializeField] private bool showTop = true;
        [SerializeField] private bool showBottom = true;
        [SerializeField] private bool showPlacedComponentPreview = true;
        [SerializeField, Range(.5f,1.5f)] private float brightness = 1.0f;
        private Transform FindUnderRoot(string n)
        {
            if(visualRoot==null)return null;
            foreach(var t in visualRoot.GetComponentsInChildren<Transform>(true))
                if(t.name==n)return t;
            return null;
        }
        public void SetVisualLayers(bool top,bool bottom)
        {showTop=top;showBottom=bottom;ApplyVisualState();}
        public void ShowAssembledBoardPreview(bool visible)
        {showPlacedComponentPreview=visible;ApplyVisualState();}
        [ContextMenu("Refresh Visual-Only Board Display")]
        public void ApplyVisualState()
        {
            var a=FindUnderRoot("L94_TopGreenSoldermask"); if(a!=null)a.gameObject.SetActive(showTop);
            a=FindUnderRoot("L94_TopSMDGoldPads");if(a!=null)a.gameObject.SetActive(showTop);
            a=FindUnderRoot("L94_BottomGreenSoldermask");if(a!=null)a.gameObject.SetActive(showBottom);
            a=FindUnderRoot("L94_BottomCopperPadSet");if(a!=null)a.gameObject.SetActive(showBottom);
            a=FindUnderRoot("02_Components_27");if(a!=null)a.gameObject.SetActive(showPlacedComponentPreview);
        }
        private void OnValidate(){brightness=Mathf.Clamp(brightness,.5f,1.5f);}
    }
}
```

---

# COMMAND TO IMPLEMENTING UNITY AGENT

**Implement this specification in my actual existing Project Spark project, starting by inspecting the supplied ZIP and project code.** Use the exact manifests, honor the source caveats and highlighted netlist redundancy, implement real transactional player-created electrical topology, use my actual APIs, keep only one level manager, preserve previous levels and the game room, and make the six-objective evidence path true. Do not simply output a new design or another image. Deliver source changes, idempotent one-click Editor builder, play-tested level scene, test evidence and any documented limitations.
