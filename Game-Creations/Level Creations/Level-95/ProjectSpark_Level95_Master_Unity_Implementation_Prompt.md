# PROJECT SPARK — LEVEL 95 MASTER UNITY IMPLEMENTATION PROMPT

## PULSE ARCHITECT — Advanced AAA Precision Timing & PWM Controller

**Authoritative implementation contract | Unity gameplay + PCB source data + six objective certification**

**Important provenance:** Source details are drawn from the provided `ProjectSpark_Level95_PulseArchitect_AAA_Complete.zip` (the project-mounted extracted folder `ProjectSpark_Level95_PulseArchitect_AAA/`). This is a future implementation instruction, **not an assertion that the supplied files already include playable circuits or a working Level 95**. Original source tables are reproduced unmodified as annexes for auditability. Where source design statements disagree, the main sections flag the discrepancy rather than silently changing the data.

---

# 01. IMPLEMENTATION MANDATE — BUILD AN ACTUAL PLAYABLE LEVEL

You are the Unity engineering agent implementing **PROJECT SPARK LEVEL 95: PULSE ARCHITECT** in the user's real Project Spark Unity project, using the supplied `ProjectSpark_Level95_PulseArchitect_AAA_Complete.zip` art-and-data package. Deliver working Level 95 gameplay, not a separate prototype and not a promotional render. Maintain the existing Level 94 reusable PCB interaction framework where compatible; preserve Levels 1–94.

**Hard outcome:** The player constructs a 120 × 80 mm two-layer Precision Timing & PWM Controller, installs 36 correct electronic items, creates 48 explicitly required copper route tasks, places 10 real vias, commissions W049, powers and calibrates real simulated timing, verifies measured 1024/256/2 Hz signals and 25/50/75% PWM, repairs all three faults, removes and restores W049 under safety supervision, and obtains a genuine **6/6** through the current authoritative `LevelGamePlayManager`.

Never implement a parallel fake solver, count source plan rows as player completed actions, spawn a preassembled board as playable, substitute animated signal lines for electrical edges, or set an objective complete merely because an animation, button or test timer finished. The source kit supplies geometry, visual pivots, planning data and art tools; none is a fully certified runtime PCB simulation.

# 02. SOURCE PACKAGE — READ EXACT FILES AND PRESERVE NAMES

Unpack the Level 95 ZIP under `Assets/ProjectSpark/Level95/` with its relative paths intact. Read `Docs/README_FIRST.md`, `Docs/Level95_Gameplay_Integration_Plan.md`, `Docs/W049_Critical_Output_Feedback_Recovery.md`, `Data/PCB_Physical_Design.json`, `Data/Circuit_Graph_Contract.json`, and every CSV in `Data/` before changing gameplay code. Inspect `Models/`, `GLB/`, `Materials/`, `Textures/`, `Shaders/`, `Unity/` and `Unity/Editor/` and use the source filenames in the annexes. Keep binary FBX and GLB source files unchanged where possible; generated `.prefab`, `.mat`, `.asset`, scenes and C# components belong to separate Level 95-owned locations.

The package includes 54 modular FBXs, 54 corresponding GLBs, the two assembled showcase files, 21 shared PBR material families and 126 PNG maps. Inspect the *real* Project Spark code before binding runtime components; this prompt is a contract, not proof that suggested APIs already exist.

# 03. SOURCE-VERIFIED BASELINE — DISTINCT COUNTS

Asset counts are verified from the source package: **54 modular art models** (8 bench/fixture, 6 board/pad, 4 instruments, 36 installable components), **36 BOM placements**, **150 logical pad and test-point records**, **48 player-created route IDs W001–W048**, **10 via IDs V01–V10**, **one separate removable jumper W049**, **35 factory-fabricated branch records**, **48 animation pivots**, **six objective records**, **three fault scenarios**, and **72 tests L95-T001–L95-T072**. These are intentionally different kinds of things and must not be summed or substituted.

The two copper layers are `F.Cu` and `B.Cu`. The native PCB measures **120 mm × 80 mm × 1.6 mm**. The imported artistic presentation uses **12× scale** solely to make components selectable on the game table. The protected supply is nominal **5.0 V**, with a nominal **3.3 V monitoring rail**; target clocks are 1024 Hz master, 256 Hz divided and 2 Hz divided. PWM is nominal 256 Hz and must be proved at **25%, 50% and 75% duty**. Source tolerances are *not specified as numeric acceptance thresholds*: implement scenario-configured tolerances, label them game targets, and test them.

# 04. PRESERVE ALL PREVIOUS LEVELS AND EXISTING APIS

Make no destructive changes to Levels 1–94, existing level data, assets, scene roots, shop inventory IDs or the Project Spark electrical system. Before writing adapters, locate the real definitions and call sites for `SparkElectricalSolver`, `SparkCircuitSystem`, `SparkTerminal`, `LevelGamePlayManager`, the existing level evaluator, player wire/drag actions, shop and shelf, and the Level 94 PCB model. Document namespace, method signatures, serialization conventions, event ordering, and null/missing-service behavior. Use actual APIs, not methods invented by this prompt.

If Level 94 reusable PCB systems are absent or have different contracts, implement an adapter or reusable missing capability without breaking the previous level. Any source discrepancy must produce a build-time reconciliation issue instead of being silently overwritten.

# 05. LEVEL 95 STORY, LEARNING GOAL AND INITIAL STATE

Level 95 is the **Pulse Architect** stage of the PCB-to-Motherboard Mastery series. The player graduates from Level 94's basic controller assembly to frequency-generation and time-domain diagnostics. The hero object is a dark-blue two-layer timing PCB with TLC555-style master oscillator, 74HC14 conditioning, CD4040B-style binary counter, LM393 PWM comparator, protected MOSFET driver and independent timing safety monitor. Use the same large, tidy Project Spark futuristic game room.

On first enter: level progression `0/6`; all **36 BOM objects have `InitialState=NOT_INSTALLED`**; all W001–W048 have `InitialState=NOT_ROUTED`; all V01–V10 have `InitialState=NOT_PLACED`; W049 has `InitialInstalled=NO`; bench power is off; instrument probes are not attached; neither fault-recovery objective is pre-awarded. The completed-board FBX and GLB are **showcase references only**. The first playable level must never instantiate the showcase as the electrically populated board.

# 06. BOARD PHYSICAL UNITS VERSUS 12× VISUAL SCALE

Use a single explicit millimetre-to-board-local transform for `(Xmm,Zmm)` from CSVs, and a separate visual-frame transform. Do not multiply component positions by 12 twice or apply a display-scale multiplier inside current, capacitance, frequency, clearance, via diameter or physics calculations. The source pad table provides `Xmm`, `Zmm`, `ArtX`, `ArtY`, `ArtZ`: preserve both coordinate representations and validate importer conversion with known pads.

The educational board is nominally 120×80×1.6 mm. At 12× presentation scale, model positions are a view convenience, not a different board footprint. Colliders, raycasts, camera focus and UI snapping must align with the rendered board while DRC continues using physical millimetres.

# 07. INTEGRATE WITH THE EXISTING FUTURISTIC ROOM TABLE

Reuse the Project Spark Game Room and its large 9.9 m central preview table. Keep the room sparse: use the **existing** room chairs, dustbin, lighting and optional decorations. The Level 95 first eight FBXs include an optional workbench/fixtures; do not duplicate an entire bench inside another bench unless the user intentionally chooses the standalone kit preview. Allow `00_Bench` / `BenchAndFixtures` to be hidden while retaining the board clamp, ESD top, instruments and parts tray as appropriate.

Game-room showcase mode may clone only mesh/material/transform data. It must never clone active `SparkTerminal`s, live circuit simulations, inventory registrations or objective trackers. Play mode must create exactly one Level 95 authoritative PCB runtime graph.

# 08. SCENE HIERARCHY, STABLE IDS AND REBUILD IDEMPOTENCE

Recommended logical tree: `Level_95_Layout/L95_RUNTIME/{BoardDefinition,Footprints,LivePads,PlacedComponents,CopperSegments,Vias,FactoryEdges,Power,Oscillator,Divider,PWM,OutputProtection,Instruments,Faults,W049Safety,Objectives,SCADA}`. Recommended art tree: `Level_95_Layout/L95_VISUALS/{BoardAndPads,Fixture,Instruments,UninstalledInventory,InstalledMeshes,TraceOverlay,WaveformDisplays,OptionalBench}`. Place game-state components under the persistent runtime root, never under a replaceable FBX hierarchy.

The builder must be idempotent: repeat execution may refresh Level 95-owned generated art without duplicating terminals, tests, component IDs, colliders or objectives. Protect user-authored placements and all previous levels. Record a data schema revision, import manifest checksum and reconciliation log.

# 09. EXACT SOURCE MODEL OWNERSHIP AND ASSET IMPORT

Use the 54 `Data/Model_Manifest_54.csv` entries verbatim, including numeric FBX prefixes. Model groups are exactly `BenchAndFixtures` (8), `BarePCBAndPads` (6), `TestInstruments` (4), and `PlayerInstallableComponents` (36). Retain reference designators U1–U7, R1–R12, C1–C9, LED1–LED2, Q1, D1, RV1, W049, J1, J2; a similar-looking capacitor is not interchangeable with another unless game inventory explicitly allows the correct part and value.

Keep `L95_PulseArchitect_CompleteShowcase` solely for art preview and screenshots. Do not make the numbered modular model 51 `L95_W049_DetachableMonitorJumper` cosmetically installed at game start. Validate geometry import scaling, local bounds, material slots and pivot naming. Verify assembly art against `Data/Model_Manifest_54.csv` rather than inferring from filenames.

# 10. PBR MATERIALS — 21 FAMILIES, 126 SHARED MAPS

Generate editable native Unity `.mat` materials using `Materials/PBR_Material_Bindings.json`, `Materials/Material_Reference_Map.csv` and `Materials/PBR_Texture_Manifest.csv`. The 21 families provide six 1024×1024 map types per material: BaseColor, Normal, MetalRoughAO, UnityMaskMap, Occlusion and Emission. Use the source material slot names and map paths; no fragile renderer-index-only assignment. Choose native URP Lit, HDRP Lit or Built-in Standard based on the actual Unity project pipeline.

Ensure color versus data maps use the proper sRGB flags, mark normal maps as normals, and interpret the documented Unity mask channel convention correctly. Blue-black soldermask should be subtle, copper/gold pads clearly distinguishable, IC markings legible, and status LED emission driven by the runtime, not baked "success" colors. The maps are procedural **reference quality** and still need artist-authored UV/bakes for final AAA signoff.

# 11. HOLOGRAM SHADERS, LIGHTING AND RENDER PIPELINES

The kit includes `Shaders/ProjectSpark95_PulseHologramURP.shader` and `Shaders/ProjectSpark95_PulseHologramBuiltIn.shader`. Confirm actual shader import names with the files. HDRP should use native Lit/emissive equivalents or an explicitly tested HDRP version; do not claim unsupported shader code automatically works in all pipelines. Suppress transparent overdraw on tiny PCB pads, cap bloom, keep text/test points readable, and test with real pipeline project settings.

Use cool cyan for measurement and routing visualization, warm amber for PWM/attention, green only for actually valid monitored states, and red only for real faults. No screen UI, oscilloscope texture or volumetric holo effect may independently award gameplay evidence.

# 12. COLLISION, LEVEL OF DETAIL, PICKING AND PERFORMANCE

`Data/Collision_LOD_Production_Plan.csv` contains recommendations only. Create fit-for-purpose colliders: a board-body collider; individually addressable footprint pick targets; probe snap targets; grabbable W049; optional fixture interaction; and large instrument hit surfaces. Avoid collisions that prevent the player selecting nearby SOIC pins. Use focus zoom, snapped placement and keyboard/controller alternatives to pixel-perfect drag.

Produce real collision proxies/LOD assets as required by target hardware; source comments are not generated LOD geometry. Use static batching appropriately and profile draw calls, renderers, shader variants, transparency and signal overlays on the existing game-room stage. Document measured performance in the actual project rather than promising untested frame rates.

# 13. PIVOTS — ALL 48 NAMES AND MOVEMENT AXES

`Data/Animation_Pivots.csv` lists 48 source pivots: **36 installation pivots** and twelve other fixtures/instrument presentation pivots. Axis distribution is source-specific: 42 `y`, 4 `z` and 2 `x`; do not rotate all around a hardcoded Y axis. Read `Model`, `Pivot`, `X`, `Y`, `Z`, `Axis`, `Movement`, `Description` and verify every named source node exists after FBX import.

`ANIM_W049_INSTALL` moves a visual model but must never change electrical continuity unless the real W049 placement action succeeds. Animating a display bezel or turning a knob changes the artistic pose only; the live measured data comes from the circuit simulation and instrument samples.

# 14. COMPONENT BILL OF MATERIALS — 36 UNIQUE REFERENCES

Parse `Data/Component_BOM_36.csv` and `Data/Component_Placement_36.csv` exactly. The BOM includes seven U devices, twelve resistors, nine capacitors, two indicator LEDs, one MOSFET, one protection diode, one duty trimmer, the W049 removable service jumper and J1/J2 connectors. All rows begin `NOT_INSTALLED`. Preserve designated values and `RotationDegrees`, `CorrectOrientationRequired`, `PolarityCheck`, footprint dimensions and named FBX owner.

The original BOM includes **W049 as one of the 36 installables**, while Objective 02 explicitly requires W049 installed for network certification. The implementation should not create a 37th required part or count W049 twice. Confirm whether Objective 01's "all 36" demands jumper installation at assembly or intentionally defers it to Objective 02; whichever ordering is chosen must preserve BOM ownership and Objective 02's safety prerequisite, be recorded in the certification plan, and avoid an impossible objective sequence.

# 15. PLACEMENT, SOLDER, SHOP/SHELF AND PART RECYCLING

Footprint snap validates component Ref, size, nominal position, rotation, IC pin-1 marker, polarity and correct value. Keep physical placement evidence separate from solder electrical-contact evidence. Use inspectable joint states (`Open`, `Good`, `Bridge`, optionally `Intermittent`) and their corresponding graph edges. The player's failed part install cannot satisfy the net merely by hiding the preview mesh.

Reuse existing Project Spark shop/shelf/throw semantics only after reading the real code. Connected/powered parts must not be removed or recycled through a loophole. A faulty C1 replacement must be specifically `C1`, with real component value state, not a visual recolor. A user should be able to undo mistaken placements while maintaining graph integrity, inventory counts and objective evidence invalidation rules.

# 16. 150 LOGICAL PADS AND TEST POINTS — DECORATIVE IS NOT LIVE

Parse every row of `Data/Logical_Pad_And_Contact_Map.csv` using `PadID`, `OwnerRef`, `LogicalSignal`, `PadIndex`, `Side`, `(Xmm,Zmm)`, `ArtX/Y/Z`, `OwnerFBX`, `FBXSocketIsDecorative` and `PinoutsRequireRealComponentDatasheetReview`. Each logical pad must have a unique stable ID such as `(L95,PadID)` and a compatible real circuit terminal or pad-graph representative. Do not create two electrically connected pads merely because the mesh/socket positions overlap.

The source flags **all FBX socket nodes as decorative** and indicates physical pinout review is required. Educational logical aliases (for example `U4.RAMP_SYNC`) are not a manufacturer-approved LM393 package pinout. Make the live pad/functional signal mapping explicit and fail validation for unresolved pin assignments; do not claim the generated data can be fabricated directly without a corrected schematic.

# 17. W001–W048 PLAYER ROUTING — EXPLICIT TASK EVIDENCE

Use all rows in `Data/Copper_Trace_Tasks_W001_W048.csv` with original `WireID`, `SourcePad`, `DestinationPad`, `Net`, `SignalDomain`, `RequiredLayer`, `RequiredVia`, width and clearance fields. Every W task starts `NOT_ROUTED`. Player routing must produce actual copper geometry, a route-owned graph edge and recorded DRC/ERC evidence. A route may not complete just because the source and destination happen to be connected through *different existing factory branches*.

Route groups in the source are W001–W015 power/ground, W016–W027 oscillator/filters, W028–W031 clock conditioning/division, W032–W039 PWM/output, W040–W048 indicators/duty. W049 is explicitly **not** the forty-ninth copper route: it is a removable physical safety jumper. Keep its insertion/removal state separate from the 48 trace progress counter.

# 18. V01–V10 — REAL TWO-LAYER VIA GEOMETRY

Import ten `Data/Required_Vias_V01_V10.csv` rows and bind their required routes exactly: V01→W004, V02→W010, V03→W016, V04→W020, V05→W024, V06→W028, V07→W032, V08→W036, V09→W041 and V10→W046. They are all initially `NOT_PLACED`, with source `DrillMM=0.35` and `AnnularPadMM=0.78` and an F.Cu↔B.Cu connection.

A via is an actual plated interlayer electrical transition, not a colored circle. Require valid drill and annulus geometry, layer connection, route ownership and DRC. Removing V06 in the fault challenge must break its actual net path and stop dependent clock qualification. Avoid duplicate via ownership or automatic replacement through visualization regeneration.

# 19. 35 FACTORY CONNECTION BRANCHES — NEVER PRE-ROUTE PLAYER TASKS

`Data/Factory_Net_And_Testpoint_Branches.csv` has 35 records marked `FACTORY_CONNECTED`, `NOT_PLAYER_ROUTING`. These represent manufacturing/internal branches and test-point taps, not player work. Import them as explicit graph edges with `Factory` ownership; do not count them toward W001–W048 progress.

Some routes may appear electrically redundant through factory branches or wider net connectivity. Preserve original branch rows and separately prove that every player W ID was drawn, routed on permitted layer(s), with required via(s), and DRC validated. Log all alternate paths in a reconciliation report. Do not silently delete a factory branch, convert W IDs to visual-only lines, or award a missing route by whole-net continuity.

# 20. GRAPH TOPOLOGY, EDGE OWNERSHIP AND REVISIONS

Represent pad contacts, component internal edges, factory copper edges, player copper segments, vias, removable W049 contacts and probe attachments with distinguishable ownership and lifecycle. Every topology-changing action increments a network/graph revision and invalidates all stale derived measurements and pending tests. Probe connections do not change the PCB routing requirements unless the actual Project Spark electrical system models probe impedance explicitly.

Use atomic graph transactions for soldering, copper completion, via placement, desolder/rework and W049 extraction. Roll back malformed actions. Keep graph IDs reproducible after save/load. Serialize the original source identity for each edge and preserve task-level proof, even when two graph nodes remain connected by another route.

# 21. DESIGN RULE CHECKS, ELECTRICAL RULE CHECKS, NEGATIVE TESTS

DRC must check trace width, configured minimum clearance, copper-layer membership, pad landing, unintended same-layer crossings, exact via rules, board boundaries and shorted net geometry. ERC must detect missing rails/returns, invalid power-source connections, cross-net shorts, reversed polar components, absent component pins, incomplete jumper contacts and unsafe short/load states. For each W task, combine the required physical geometry and actual electrical graph result.

Tests must include: a missing one of 48 traces; each of ten missing vias; a route placed on a wrong layer; a self-intersection; trace touching an unrelated pad; swapped C1/C2; incorrect value or reversed D1; uninstalled J1; a complete-board showcase substituted for the live board; and a previously passing test reused after graph revision. None may result in six objectives being marked complete.

# 22. PROTECTED SUPPLIES — 5 V AND 3.3 V MONITOR RAIL

Model the actual protected J1 5 V input (through D1/protection as represented), ground return, loads, rail ramp, simple source impedance, current limiting and U5 monitored 3.3 V regulator response. This is fictional low-voltage educational control power, not a high-voltage physical bench wiring tutorial. Do not mark circuits powered if their rail is visually green but the electrical graph is open.

Electronics need valid VDD/VCC and ground conditions; a missing protected input, a missing return, an overcurrent trip or bad U5 decoupling must produce measurable state changes. Require fresh rail measurements before accepting timing or PWM evidence.

# 23. U1 TLC555-STYLE MASTER OSCILLATOR — 1024 HZ

Use R1 14.7 kΩ, R2 63 kΩ and C1 10 nF source values for the representative nominal educational astable oscillator. A simple approximation is `f ≈ 1.44 / ((R1+2R2)C1)`, producing about 1023.45 Hz for this baseline. This is a modeled clock target, not a claim of exact real-device behavior. Include state for timing-capacitor charge/discharge, reset pin permission, output high/low, edge timestamps, supply validity and component tolerances; deterministic pseudonoise is acceptable only if it is seeded and traceable.

Missing/incorrect R1, R2, C1, U1 VDD/GND/RESET or routing must affect the generated waveform. C1=22nF fault must lower the actual oscillator frequency and propagate to all divided outputs. Do not independently author three unrelated frequency constants.

# 24. U2 SCHMITT CONDITIONING AND EDGE INTEGRITY

U2 is a 74HC14-style Schmitt input conditioning stage. Signal flow W028 U1.OUT → U2.IN1, then W029 U2.OUT1 → U3.CLK. Include propagation delay, threshold/hysteresis proxy, invalid-level/edge handling and an honest timing output. In the source fault F02, V06 breaks W028: the conditioned clock should become stale/unavailable, while U1 may remain oscillating at its own output. Distinguish raw oscillator, conditioned signal and downstream counter states in SCADA.

# 25. U3 CD4040B-STYLE DIVIDER — /4 AND /512

Model a 12-stage counter receiving valid conditioned clock edges. Source net routing is W030 U3.Q2 → U4.RAMP_SYNC (nominal /4 = 256 Hz) and W031 U3.Q9 → U7.SLOW_CLK (nominal /512 = 2 Hz). Use actual edge/state progression and reset behavior. Measured frequencies should derive from U1's edges, then the valid U2 and U3 clock chain, not from hardcoded independent timers.

Require valid counter power, reset state and graph connections; if W028/V06 or W029 is open, the 256/2 Hz channels must not stay artificially valid because previous values were cached.

# 26. U4 PWM MODULATOR — RAMP, COMPARATOR AND PULL-UP

The kit treats U4 as an LM393-style educational PWM comparator with functional aliases `RAMP_SYNC`, `RAMP_OUT`, `DUTY_REF`, `PWM_OUT`. Implement synchronized ramp generation and threshold comparison in the gameplay model. The source has R4/C7 local branches for ramp conditioning and R10 pull-up to protected power. An LM393-like open-collector output cannot self-drive high without appropriate pull-up; power, ground and pull-up invalidity must affect the output.

This source is **not** a manufacturer-accurate LM393 package/PCB pinout. Do not claim fabrication readiness. Verify logic-voltage thresholds, real device limitations and functional mappings in a separate engineering signoff before using this art kit for real electronics.

# 27. RV1 CONTROLS PWM DUTY — NOT MASTER FREQUENCY

The source BOM explicitly identifies `L95_RV1_PWMDutyTrimPotentiometer`, and routes W046 `RV1.WIPER → U4.DUTY_REF`, W047 `D1.K → RV1.HI`, and W048 `RV1.LO → J1.GND`. Therefore RV1 changes the PWM comparator's duty reference **only**. It must not directly alter U1 oscillator timing or CD4040 division.

The source fault plan F01 says “Replace C1 with correct value and calibrate trimmer,” which conflicts with the explicit stated PWM-only RV1 role if interpreted as clock calibration. **Surface this as a source ambiguity:** implement C1 value restoration and remeasure oscillator frequency for clock recovery; treat any RV1 retuning as a separate PWM-duty verification rather than silently using RV1 to change U1. Report the resolution in the source reconciliation log.

# 28. U6 LOGIC GATE, U7 SAFETY SUPERVISOR AND Q1 OUTPUT

Source W032 feeds U6.IN_A from U4.PWM_OUT, W033 gates U6.IN_B with U7.SAFE_ENABLE, W034 sends U6.OUT to R7.A, W035 R7.B to Q1.G, W036 Q1.S through sense shunt R9, W037 R9.B to ground, W038 Q1.D to J2.LOAD_NEG and W039 protected supply to J2.LOAD_POS. Preserve route identity and actual electrical dependencies. The MOSFET/load current model must have finite load, source/shunt voltage response, temperature or overload proxy, and correct disabled behavior.

U7 is an *educational modeled safety monitor*, not a guaranteed physical safety PLC. Its gate permission must require valid W049 monitored feedback, qualified supply, safe current and no latched faults. A colorful LED cannot bypass U7. Connected parts must have meaningful failure and measurement behavior.

# 29. REAL PWM OPERATING MODES AND MEASUREMENT CRITERIA

Require three separately executed and recorded tests at nominal 256 Hz: **25%, 50%, 75% duty**. Each must measure actual period, high time, low time, voltage levels, load current and gating response. For ideal 256 Hz, period is approximately 3.90625 ms and ideal high times are 0.97656 ms, 1.953125 ms and 2.92969 ms. Treat these as expected educational math, and use source-configured tolerance, sampling resolution and plausible noise in actual acceptance.

Changing duty setting must not alter the divider output period. Output tests must be newly sampled after graph edits and independent at each requested duty. A nonfunctional MOSFET that produces visually animated PWM cannot qualify.

# 30. REAL OSCILLOSCOPE, MULTIMETER AND LOGIC ANALYZER

Provide probes that snap to the 150 logical pad/test-point IDs and sample the authoritative electrical graph. A four-channel oscilloscope should measure U1 raw 1024 Hz, U3 divided 256/2 Hz and PWM/load voltage with correct trigger, timebase, waveform cache expiration and measurement flags. The DMM measures the actual 5 V, 3.3 V, ground, resistance/continuity with safe power-state enforcement. The logic analyzer timestamps raw/conditioned/slow/PWM edges and displays invalid/stale channels honestly.

Never create a passing waveform from a preauthored UI image or preview model. A disconnected probe should display missing data, not a zero-valued valid signal. Include measurement provenance in objective evidence: `sourcePadID`, timestamp, networkRevision, calibrationRevision, valid/invalid reason and test-session ID.

# 31. EVENT TIMESTAMPS, SAMPLE VALIDITY AND RETEST RULES

Use `Data/Circuit_Graph_Contract.json` validity fields (`measuredValue`, `timestamp`, `sensorState`, `calibrationRevision`, `networkRevision`) as mandatory contract fields; add test-session, instrument ID and provenance as needed. All measurements become stale after relevant wiring/component/calibration changes. Display `INVALID`, `STALE`, `OVER_RANGE`, `NOT_POWERED` or `DISCONNECTED` as appropriate rather than generating fake zeros.

Objective 03 and 04 require fresh independent captures after successful commissioning; Objective 05 requires a full new post-fault test; Objective 06 requires another full post-W049-repair run. Do not recycle pre-fault waveforms or pre-insertion W049 samples.

# 32. FAULT INJECTION — PHYSICAL/ELECTRICAL EFFECT, NOT UI FLAG

Import `Data/Three_Faults_And_Recovery.csv` exactly. Fault sequence must be deterministic and distinct, with injected condition, actual graph/parameter change, detectable measured symptom, appropriate safe mode, player correction, and new test evidence. The fault controller must never simply show an alarm then mark it repaired when clicked.

Keep conditions safe for a low-energy educational game: require virtual power isolation before simulated solder bridge repair and avoid physical high-energy instructions. A fresh production retest must explicitly prove no current/clock/PWM error remains.

# 33. FAULT F01 — C1 TIMING CAPACITOR MISVALUE

Fault F01 changes C1 from intended **10 nF** to **22 nF** in the authoritative board model. Recalculate actual master frequency and downstream 256/2 Hz. Expect lower measured clock frequencies. The player must inspect waveform period, identify C1 and replace/reassign the correct value while in a safe electrical state. Calibration of a PWM duty trimmer is *not* a substitute for fixing oscillator C1; the source's "calibrate trimmer" wording is logged separately as noted above.

Proof: new U1 and U3 oscilloscope captures with valid graph revision and test-session ID; wrong part still installed must fail.

# 34. FAULT F02 — V06 / W028 OPEN CONDITIONED CLOCK

Fault F02 removes the physical V06 continuity of required route **W028**, which links U1.OUT and U2.IN1. U1 may still have valid raw 1024 Hz at its source node, but U2 and U3 downstream valid clocks must disappear or become stale. Actual route connectivity and DRC must fail until the player rebuilds V06. Fixing a decorative hole, hitting a reset button or toggling a counter output must not restore the graph.

Proof: actual V06 interlayer continuity, W028 route evidence and new correct 256 Hz and 2 Hz captures.

# 35. FAULT F03 — Q1 GATE–DRAIN SOLDER BRIDGE

Fault F03 adds an unintended cross-net solder edge from the Q1 gate network to the Q1 drain/output network. The actual load output/current must become abnormal under modeled safe limits; U7 must inhibit output as designed. Player action requires de-energizing the affected simulated output, removing the unwanted edge, inspecting the resulting nets and remeasuring switching and load behavior.

Proof: DRC/ERC report no bridge, measured MOSFET switching returns and W049 monitored feedback is fresh and valid.

# 36. OBJECTIVE 01 — INSPECT AND ASSEMBLE ALL 36

Read and identify PCB/schematic nets, footprint/reference locations, main power/clock/PWM paths and jumper contacts. Install all 36 BOM references with proper value, polarity, orientation, solder and placement state according to source. Preserve the BOM W049 accounting nuance: if Objective 01 truly requires all 36, W049 must be inserted by this point and subsequently kept installed for Objective 02; if the approved learning design stages jumper installation in Objective 02, track 35 fixed placements and a separate W049 requirement while explicitly documenting the source-plan interpretation. Never require 36 fixed parts **plus** a 37th jumper.

The objective passes only on authority-backed component/footprint/solder evidence, not mesh visibility. No preinstalled showcase parts.

# 37. OBJECTIVE 02 — 48 COPPER ROUTES, TEN VIAS, W049

Require all W001–W048 source route tasks explicitly completed and DRC/ERC-valid, V01–V10 present and electrically connected, W049 installed with qualified contact quality, no forbidden cross-net short, and reliable true supply/return paths. Maintain separate counters: component placements, copper task IDs, vias, physical W049 and net health. Factory edges do not count as player work.

Do not pass this objective when an alternative factory branch happens to connect source/destination but player route geometry is absent. Prove both exact route ownership and actual powered net connectivity.

# 38. OBJECTIVE 03 — COMMISSION AND CALIBRATE MASTER/DIVIDED CLOCKS

Require valid protected 5V and regulated 3.3V monitoring state, valid W049, U1 raw oscillator near 1024 Hz, U2 conditioned input, and actual U3 Q2/Q9 near 256 Hz and 2 Hz. Acquire fresh measured timing samples with configurable tolerances and sufficient observation duration for the slow 2 Hz channel. Test reset/enable as source permits and confirm no invalid logic levels.

Calibrate by proper physical timing network component correction; RV1 adjusts PWM duty only. No frequency target may pass while C1 is wrong or W028/V06 open.

# 39. OBJECTIVE 04 — VERIFY THREE PWM DUTIES UNDER LOAD

Perform three sequential, independent and time-stamped output tests at 25, 50 and 75% requested duty. Each requires nominal 256 Hz PWM frequency, measured period and high time, healthy safety-monitor feedback through W049, current-limited MOSFET/load response, no unsupported voltage states, and independent evidence for all settings.

The passing output UI cannot substitute for actual oscilloscope/current readings. Any new graph revision or latched fault invalidates unqualified captures; do not award on setting a UI slider alone.

# 40. OBJECTIVE 05 — THREE RECOVERIES AND POST-FAULT RETEST

Stage C1=22nF, V06 missing and Q1 gate–drain bridge as three authentic faults. Track an evidence token for each: fault injected, measurable fault manifestation, correct safety/diagnosis action, real repair and fresh individual proof. After all recoveries, rerun the complete 1024/256/2 Hz timing and 25/50/75% duty production qualification. Objective 05 passes only after those new samples exist in a separate valid run.

Earlier objective certifications remain historically earned, even if a fault temporarily makes the live board unqualified. Use explicit current state versus historical evidence rather than a single boolean.

# 41. OBJECTIVE 06 — PHYSICAL W049 DISCONNECT AND FULL RECOVERY

After Objectives 1–5 are historically certified, require a supervised low-energy test. The player physically removes the **same original W049** from J3.A/J3.B. Graph connectivity becomes open. U7 invalidates trusted PWM output feedback, latches a critical fault and inhibits the PWM output load. Clock-only diagnostics may remain powered when safe. Cached readings are not eligible.

The player then reinstalls W049 with valid contacts, obtains new measured output-monitor samples, explicitly commands safety reset once permitted and completes an entirely fresh full-board timing and PWM/load qualification. Award Objective 06 **only** when the actual `LevelGamePlayManager` certifies the entire sequence, reaching 6/6. Never implement a "Mark W049 Completed" button.

# 42. W049 SAFETY STATE MACHINE AND FAULT LATCH

Recommended transitions: `NotInstalled → InstalledUnqualified → FeedbackQualified → Armed → FaultLatchedOpen → ReinstalledNeedsCalibration → ReadyForManualReset → ResetAuthorized → RequalificationRunning → Qualified`. Guard all transitions by real topology and measurement data. Removing W049 at any time invalidates monitor feedback, not just during Objective 06. For the final supervised objective, store a particular qualifying fault-event ID and ensure it occurred after Objective 05's certification.

The physical source contact is `J3.A` fed via a factory branch from `J2.LOAD_NEG`; destination `J3.B` is branched to `U7.TRUSTED_PWM_MONITOR_IN`. The logical processing path is `L95_PWMOutputFeedbackMonitor.TRUSTED_PWM_MONITOR_OUT` → `L95_TimingSafetyMonitor_U7.TRUSTED_PWM_MONITOR_IN`. Both representations must map to the *same real graph event*, not two independent fictitious circuits.

# 43. INDEPENDENT PROTECTION AND FAIL-SAFE BEHAVIOR

U7 should supervise valid signal freshness, safe load range, power-good, output command/response consistency and fault latch. A lost W049 must not be interpreted as valid 0% duty/zero current; rather, its trusted channel is absent/invalid. Disable the protected output using modeled finite transitions, while keeping diagnostic oscillator observation active when still electrically safe. Reject reset on open W049, invalid feedback, new graph revision without rechecks, overcurrent, or unacknowledged fault.

No visual input, SCADA chart, test-bypass setting or editor debug flag may directly energize the load or award progression. Emergency and routine power isolation commands must remain independent of the six-objective UI.

# 44. OBJECTIVE AUTHORITY, EVIDENCE HISTORY AND REPLAY

Source `Data/Objective_Certification_Plan_6.csv` defines six objectives initially `INCOMPLETE`, authority `Actual Project Spark LevelGamePlayManager; NOT art scripts`. Implement explicit observations, provenance, stored historical certification events and current live system state separately. A historical pass persists after a supervised fault, but its physical readings must not be reused for a newly required test.

Maintain run/session counters: initial commissioning, F01/F02/F03 repair trials, post-fault complete test, W049 removed event, post-W049 recovery. Every captured reading includes networkRevision and calibrationRevision, so source changes automatically stale old results. Level complete requires all six evidence records, not `Count>=6` alone.

# 45. FIXED-STEP ELECTRONICS MODEL AND DETERMINISTIC REPRODUCIBILITY

Provide a fixed-step or event-driven simulator that can resolve ~1024 Hz edges without losing transitions at ordinary frame rates. Keep physics/electronics time separate from render frame time; buffer waveform events and downsample for scopes. For slow 2 Hz verification, sample a meaningful observation window. Seed any stochastic jitter and instrument noise so QA can reproduce exact test behavior. Do not simulate output clocks by driving sine waves on visual meshes.

The model should be accurate enough for game objectives, not an unsupported claim of circuit-grade SPICE accuracy. Log model approximations, declared thresholds and domain limits. Prevent stale simulation coroutines or duplicate solvers when scene gets rebuilt.

# 46. SCHEMATIC RECONCILIATION — LOGICAL ALIASES VERSUS DATASHEETS

Source pad names are educational role aliases. The board art does not supply a complete production-quality manufacturer-verified schematic and does not automatically guarantee real TLC555/CD4040B/LM393/74HC14/74HC08 pin correctness. Build a clearly documented functional pin-to-simulation-port mapping that retains source PadIDs while distinguishing physical pin numbers, simulated logic ports and signal domains.

Specifically flag the LM393-like `RAMP_SYNC`/`RAMP_OUT` aliases; make a separate internal ramp generator model rather than pretending they are official chip pins. Record all unresolved contradictions as build report errors or explicit educational abstractions, not invisible "repairs" of the original sources.

# 47. PROJECT SPARK HUD, SCHEMATIC AND SCADA READABILITY

Provide `Assembly`, `Routing`, `Layers`, `Testing`, `Diagnostics`, `Repair`, and `Certification` modes. Use actual board nets to highlight complete/incomplete tasks. Display separate source clocks, conditioned clocks, divided clocks, PWM duty, measured 5 V/3.3 V, shunt/load response, jumper signal validity, U7 latch reason and measurement age.

Every UI value should bind to a typed read-only view model from the authoritative simulation; no self-awarding UI status. For a failing objective, show a specific actionable blocker (e.g., `V06 missing`, `W028 stale`, `C1 value mismatch`, `W049 absent`, `2 Hz observation too short`) rather than a generic red symbol. Avoid dense decorative HUD that blocks component selection.

# 48. ACCESSIBLE PLAYER INTERACTION AND UNDO/REDO

Support component inventory filtering by Ref/value, snap previews, rotation and polarity clues, click/drag or controller placement, route waypoints, layer toggle, via insertion, inspect and clear errors, instrument-probe assignment, guarded fault repair, and a visible confirmation for high-impact disconnected-parts changes. Keep a tutorial path for clock chain and a separate test for each PWM duty.

Undo/redo must preserve circuit graph integrity and invalidate dependent measurements; restoration should not duplicate a physical resistor, consume two replacements or leave ghost routes. For test instruments, allow clear probe target labels even at 12× scale.

# 49. ANIMATION SYSTEM — STATE-DRIVEN ONLY

The 48 named pivots drive only visual placements, fixture motions and instrument knobs. When a correctly installed component is registered, play placement animation and reveal that component's mesh. When repaired, show its actual new physical state. W049's jumper animation follows the same graph event that changes trusted feedback. An oscilloscope UI should render actual samples, not a canned GIF.

Maintain distinction between show-mode assembled board and game-mode unassembled board. A change to the board's visual scale or refresh of art assets must not mutate source physical coordinates or complete PCB tasks.

# 50. SAVED GAME, RESTART, PARTIAL PROGRESS AND DESERIALIZATION

Persist BOM placements and values, solder states, route segment geometry and IDs, via positions, W049 physical/contact state, power/fault states, controller calibration, test-session IDs, historical objective certifications and pending UI mode. Store source-manifest version/checksum for migration. On load rebuild the graph from serialized true state rather than blindly trusting a saved `isComplete` flag.

Restart from zero must restore all 36 uninstalled, 48 routes missing, 10 vias absent and W049 out. Replay after 6/6 should not duplicate reward events or create extra purchases.

# 51. ONE-CLICK LEVEL 95 EDITOR BUILDER — TARGET MENU

Build a genuine project-side editor command:

`Tools/Project Spark/Levels/Create Level 95`

It creates or refreshes `Level_95.asset`, `Level_95_Layout.prefab`, Level 95-owned import settings/material references, gameplay runtime hierarchy, source data assets, functional pad terminals, component instances, task definitions, simulated plant, instruments, fault controller, W049 safety and six objectives. It must be undoable/idempotent, log conflicts and maintain existing Level IDs, scenes and assets. It should not silently regenerate previous levels, modify global singleton scene management, or create an isolated fake gameplay runtime.

# 52. SOURCE UNITY HELPERS — EXACT MENU PATHS AND LIMITATIONS

The package contains six C# scripts. Four are Editor-only: `SparkLevel95PBRMaterialEditor.cs`, `SparkLevel95ArtAssembler.cs`, `SparkLevel95PlanningDataEditor.cs`, `SparkLevel95ArtValidator.cs`. Two are non-Editor: `SparkLevel95PCBDefinition.cs` (data-only ScriptableObject) and `SparkLevel95BoardVisualController.cs` (visual-only controller).

Existing source Editor menus are exactly:
1. `Tools/Project Spark/Levels/95/1. Create Native PBR Materials`
2. `Tools/Project Spark/Levels/95/2. Assemble 54 Modular FBXs Under Selected Layout`
3. `Tools/Project Spark/Levels/95/3. Apply PBR To Selected Art`
4. `Tools/Project Spark/Levels/95/4. Create PCB Planning Data Asset`
5. `Tools/Project Spark/Levels/95/5. Validate Imported Assets And Plans`

Do not confuse those with the *future* one-click gameplay builder. Art assembly and planning asset creation do not make actual live copper routes or pass objectives. Run each in a throwaway Level 95 staging scene and verify exceptions/materials before linking runtime.

# 53. PROPOSED SCRIPT RESPONSIBILITIES — ADAPT TO REAL APIS

Suggested new Level 95 components **only where Level 94 lacks equivalents**: `SparkLevel95Coordinator`, `SparkLevel95PCBBoardAdapter`, `SparkLevel95TraceTaskTracker`, `SparkLevel95ViaValidator`, `SparkLevel95OscillatorModel`, `SparkLevel95ClockConditioner`, `SparkLevel95BinaryDivider`, `SparkLevel95PWMRampComparator`, `SparkLevel95MOSFETLoad`, `SparkLevel95InstrumentSampler`, `SparkLevel95W049Safety`, `SparkLevel95FaultSequencer`, `SparkLevel95EvidenceTracker`, `SparkLevel95VisualBindings` and an Editor builder/validator.

These names are *suggestions*, not existing source APIs. Prefer integrating reused Level 94 PCB systems and real Project Spark classes; do not duplicate board serialization, pad graph or generic inventory logic. Unit test each component at an independent subsystem boundary before full scene integration.

# 54. IMPLEMENT SOURCE QA TESTS L95-T001–T072

Import and execute all 72 `Data/Engineering_Acceptance_Tests_72.csv` entries. Their groups are: 12 `Art & Materials`, 12 `PCB Assembly`, 16 `Routing/DRC`, 11 `Clock/PWM`, 9 `Fault injection`, 7 `W049 feedback`, and 5 `Unity Regression`. Each source row begins `NOT_RUN`; do not change that to PASSED simply because a test class exists. Bind every test to an executable Unity Edit Mode or Play Mode assertion and capture the result, environment and reason on failure.

Expand coverage with negative tests for premature 6/6, stale waveform certification, W049 disconnected at startup, wrong BOM count, double-route alternate-path exploits, electrically isolated pad rendering, wrong via layer, save/reload, multiplayer duplicate interactions if relevant and bypassing safety through developer inspector toggles. Source 72 is the minimum acceptance catalog, not a claim tests currently run.

# 55. NON-REGRESSION, CI AND TARGET UNITY PROJECT VERIFICATION

Test Unity import, shader compile, physics/performance, the complete six-objective route and prior-level regression on the actual Project Spark project. Run Edit Mode tests for parsing, uniqueness, correct net graph, PBR import and idempotent builder. Run Play Mode tests for component drag, trace routing, via creation, instrument measurements, all 3 faults, W049 extraction, explicit safety reset and 6/6. Test with existing room/table preview and with standalone preview bench if supported.

Deliver test logs showing exact Unity version/render pipeline, build target, failures and fixes. Never equate an offline ZIP-integrity or FBX header pass with successful Unity Play Mode, functional electrical simulation or production-ready AAA polishing.

# 56. SOURCE AUDIT FINDINGS — REPORT AMBIGUITIES WITHOUT HIDING THEM

At minimum record and resolve these with explicit design decisions:

1. **RV1 role ambiguity:** the source BOM/routes say PWM duty, but F01 recovery says "calibrate trimmer". Do not use RV1 for oscillator frequency; document the separation.
2. **BOM/W049 sequencing:** the 36 BOM count includes W049, while its commissioning is highlighted in Objective 02. Ensure the six-objective ordering is satisfiable without double-counting.
3. **Educational aliases:** LM393-like `RAMP_SYNC`, `RAMP_OUT`, `PWM_OUT` are functional model names, not proven datasheet pin numbers.
4. **Factory alternate paths:** preserve all 35 factory branches and still require individual W-route geometry and evidence.
5. **Visual scale:** 12× art coordinates must not affect true board dimensions or DRC.
6. **Art pivot directions:** use source X/Y/Z axis metadata; not uniform Y.
7. **Future gameplay claim:** source contains no functional PCB solver, production pinout validation, real Unity Play Mode or approved machine fabrication files.

Make findings visible in an implementation report and stop certification if a critical source ambiguity remains unresolved.

# 57. AAA POLISH AND FINAL PLAYER EXPERIENCE

The new PCB should appear dense and premium without distracting from precision interactions. Use realistic metallic pads, clean silkscreen identifiers, properly shaded packages and subtle control panel emissive effects. Provide graceful transition from schematic inspection to placement, layer routing, via drilling, diagnostic measurement, repair and final certification. The same game room should host increasingly complex boards in later levels.

A professional completion cinematic may illuminate clock/status indicators and show measured scope waveforms, but this is an *effect of genuine qualification*, not the cause of it. Do not promise custom AAA artist finishing absent verified UV atlases, collision proxies, bakes and target frame tests.

# 58. COMPLETE PLAYER PATH — ZERO TO SIX OBJECTIVES

Start at 0/6 with bare board, 36 inventory parts, no traces, no vias, W049 out and power disabled. Inspect the schematic and references. Place correct parts and solder contacts. Draw W001–W048 including correct layer transitions, add V01–V10, insert W049 for commissioning, and pass ERC/DRC. Energize regulated control rails. Observe U1's 1024 Hz raw signal, U2 conditioning and U3's 256 Hz / 2 Hz outputs. Calibrate PWM duty and collect three fresh under-load tests. Demonstrate C1 wrong value, V06 open and Q1 bridge faults; repair each and rerun full production tests. After Objectives 1–5, remove W049 under supervision, verify fault latch/output inhibit, physically reinstall, acquire new monitor samples, explicitly reset and run a fresh clock/PWM qualification. Only the actual `LevelGamePlayManager` may certify Level 95 complete at 6/6.

# 59. REQUIRED DELIVERABLES AND HANDOFF

Provide the new/updated Unity C# files, native `.mat` files, correctly integrated level data asset, `Level_95_Layout.prefab`, scene/editor builder, true live terminals/pad graph, PCB routing interaction, full objective and fault runtime, save/load, instrument UI, source-reconciliation report, actual test scripts/results and a short import/use guide. Document every change to shared Project Spark code and show why it is backward compatible. Confirm in-game no other level was overwritten.

The Level 95 asset ZIP remains an art-and-planning prerequisite. Do not claim Level 95 is playable before real electrical tests have run and all six objectives have been earned in Play Mode.

# 60. FINAL ACCEPTANCE CONTRACT

Acceptance requires:
- 54 source assets imported, art materials assigned and pivots mapped without wrong axes.
- 36 real component instances with correct placement/value/polarity and no preinstallation.
- W001–W048 geometry/graph connected by the player; V01–V10 real; W049 distinct.
- 150 logical pads reconciled with safe/real terminal types, 35 factory connections preserved.
- No alternate-path or decorative-scene completion exploit.
- Actual 5 V and 3.3 V rails, a measured 1024 Hz oscillator, 256 Hz and 2 Hz derived clock, and 256 Hz PWM proven at all requested duties under simulated load.
- The C1, V06 and Q1 faults genuinely fail the system and are recoverable.
- W049 removal genuinely invalidates feedback and disables the protected output; physical reinstall + fresh calibration + explicit reset + new full production qualification are mandatory.
- Six objectives completed through the authoritative manager, and no save/load/rebuild path can award 6/6 early.
- Every L95-T001–T072 test is evaluated and reported; all regressions are resolved on the real Unity project.

**No fake wires. No pre-completed copper. No disconnected demo solver. No editor-only certification. No assertion of Unity test success without actually running it.**

---

# TECHNICAL SOURCE ANNEXES — EXACT FILE CONTENTS

Every annex below preserves its original source text; generated game code must parse and validate these records, not treat their mere presence as in-game progress. The tables are not independent electrical proof or fabrication signoff.

## ANNEX A — MODEL MANIFEST — 54 EXACT FBX/GLB REFERENCES

**Source path:** `Data/Model_Manifest_54.csv`

```csv
Index,Model,Group,FBX,GLB,MeshParts,Triangles,Pivots,ArtScale,GameplayReady
0,L95_PrecisionWorkBenchFrame,BenchAndFixtures,Models/00_L95_PrecisionWorkBenchFrame.fbx,GLB/00_L95_PrecisionWorkBenchFrame.glb,6,408,0,12.0,NO - visual art only
1,L95_ESDAntiStaticWorktop,BenchAndFixtures,Models/01_L95_ESDAntiStaticWorktop.fbx,GLB/01_L95_ESDAntiStaticWorktop.glb,3,48,0,12.0,NO - visual art only
2,L95_TimingPCBFixtureBase,BenchAndFixtures,Models/02_L95_TimingPCBFixtureBase.fbx,GLB/02_L95_TimingPCBFixtureBase.glb,4,480,0,12.0,NO - visual art only
3,L95_ClampingJawLeft,BenchAndFixtures,Models/03_L95_ClampingJawLeft.fbx,GLB/03_L95_ClampingJawLeft.glb,3,120,1,12.0,NO - visual art only
4,L95_ClampingJawRight,BenchAndFixtures,Models/04_L95_ClampingJawRight.fbx,GLB/04_L95_ClampingJawRight.glb,3,120,1,12.0,NO - visual art only
5,L95_InspectionMicroscopeGantry,BenchAndFixtures,Models/05_L95_InspectionMicroscopeGantry.fbx,GLB/05_L95_InspectionMicroscopeGantry.glb,5,186,1,12.0,NO - visual art only
6,L95_MicroscopeFocusLens,BenchAndFixtures,Models/06_L95_MicroscopeFocusLens.fbx,GLB/06_L95_MicroscopeFocusLens.glb,4,1296,1,12.0,NO - visual art only
7,L95_PartsAndReworkTray,BenchAndFixtures,Models/07_L95_PartsAndReworkTray.fbx,GLB/07_L95_PartsAndReworkTray.glb,5,732,0,12.0,NO - visual art only
8,L95_FR4PrecisionBoardCore,BarePCBAndPads,Models/08_L95_FR4PrecisionBoardCore.fbx,GLB/08_L95_FR4PrecisionBoardCore.glb,3,588,0,12.0,NO - visual art only
9,L95_TopNavySoldermask,BarePCBAndPads,Models/09_L95_TopNavySoldermask.fbx,GLB/09_L95_TopNavySoldermask.glb,2,84,0,12.0,NO - visual art only
10,L95_BottomNavySoldermask,BarePCBAndPads,Models/10_L95_BottomNavySoldermask.fbx,GLB/10_L95_BottomNavySoldermask.glb,1,12,0,12.0,NO - visual art only
11,L95_TopGoldSMDPads,BarePCBAndPads,Models/11_L95_TopGoldSMDPads.fbx,GLB/11_L95_TopGoldSMDPads.glb,6,1656,0,12.0,NO - visual art only
12,L95_BottomCopperPadTargets,BarePCBAndPads,Models/12_L95_BottomCopperPadTargets.fbx,GLB/12_L95_BottomCopperPadTargets.glb,1,720,0,12.0,NO - visual art only
13,L95_SilkscreenMountsAndTestFiducials,BarePCBAndPads,Models/13_L95_SilkscreenMountsAndTestFiducials.fbx,GLB/13_L95_SilkscreenMountsAndTestFiducials.glb,2,744,0,12.0,NO - visual art only
14,L95_FourChannelOscilloscope,TestInstruments,Models/14_L95_FourChannelOscilloscope.fbx,GLB/14_L95_FourChannelOscilloscope.glb,8,668,1,12.0,NO - visual art only
15,L95_Protected5VBenchSupply,TestInstruments,Models/15_L95_Protected5VBenchSupply.fbx,GLB/15_L95_Protected5VBenchSupply.glb,8,596,1,12.0,NO - visual art only
16,L95_PrecisionDigitalMultimeter,TestInstruments,Models/16_L95_PrecisionDigitalMultimeter.fbx,GLB/16_L95_PrecisionDigitalMultimeter.glb,6,452,1,12.0,NO - visual art only
17,L95_DigitalLogicAnalyzer,TestInstruments,Models/17_L95_DigitalLogicAnalyzer.fbx,GLB/17_L95_DigitalLogicAnalyzer.glb,7,476,1,12.0,NO - visual art only
18,L95_U1_TLC555CMOSTimer_SOIC8,PlayerInstallableComponents,Models/18_L95_U1_TLC555CMOSTimer_SOIC8.fbx,GLB/18_L95_U1_TLC555CMOSTimer_SOIC8.glb,5,288,1,12.0,NO - visual art only
19,L95_U2_74HC14_Schmitt_SOIC14,PlayerInstallableComponents,Models/19_L95_U2_74HC14_Schmitt_SOIC14.fbx,GLB/19_L95_U2_74HC14_Schmitt_SOIC14.glb,5,432,1,12.0,NO - visual art only
20,L95_U3_CD4040B_BinaryCounter_SOIC16,PlayerInstallableComponents,Models/20_L95_U3_CD4040B_BinaryCounter_SOIC16.fbx,GLB/20_L95_U3_CD4040B_BinaryCounter_SOIC16.glb,5,480,1,12.0,NO - visual art only
21,L95_U4_LM393_PWMComparator_SOIC8,PlayerInstallableComponents,Models/21_L95_U4_LM393_PWMComparator_SOIC8.fbx,GLB/21_L95_U4_LM393_PWMComparator_SOIC8.glb,5,288,2,12.0,NO - visual art only
22,L95_U5_3V3LDO_SOT223,PlayerInstallableComponents,Models/22_L95_U5_3V3LDO_SOT223.fbx,GLB/22_L95_U5_3V3LDO_SOT223.glb,2,60,1,12.0,NO - visual art only
23,L95_U6_74HC08_LogicGate_SOIC14,PlayerInstallableComponents,Models/23_L95_U6_74HC08_LogicGate_SOIC14.fbx,GLB/23_L95_U6_74HC08_LogicGate_SOIC14.glb,5,432,1,12.0,NO - visual art only
24,L95_U7_TimingSafetySupervisor_SOIC8,PlayerInstallableComponents,Models/24_L95_U7_TimingSafetySupervisor_SOIC8.fbx,GLB/24_L95_U7_TimingSafetySupervisor_SOIC8.glb,5,288,1,12.0,NO - visual art only
25,L95_R1_RCCharge_14K7,PlayerInstallableComponents,Models/25_L95_R1_RCCharge_14K7.fbx,GLB/25_L95_R1_RCCharge_14K7.glb,3,48,1,12.0,NO - visual art only
26,L95_R2_RCTiming_63K,PlayerInstallableComponents,Models/26_L95_R2_RCTiming_63K.fbx,GLB/26_L95_R2_RCTiming_63K.glb,3,48,1,12.0,NO - visual art only
27,L95_R3_DividerBias_10K,PlayerInstallableComponents,Models/27_L95_R3_DividerBias_10K.fbx,GLB/27_L95_R3_DividerBias_10K.glb,3,48,1,12.0,NO - visual art only
28,L95_R4_RampCharge_4K7,PlayerInstallableComponents,Models/28_L95_R4_RampCharge_4K7.fbx,GLB/28_L95_R4_RampCharge_4K7.glb,3,48,1,12.0,NO - visual art only
29,L95_R5_ClockLED_1K,PlayerInstallableComponents,Models/29_L95_R5_ClockLED_1K.fbx,GLB/29_L95_R5_ClockLED_1K.glb,3,48,1,12.0,NO - visual art only
30,L95_R6_StatusLED_330R,PlayerInstallableComponents,Models/30_L95_R6_StatusLED_330R.fbx,GLB/30_L95_R6_StatusLED_330R.glb,3,48,1,12.0,NO - visual art only
31,L95_R7_MOSFETGate_100R,PlayerInstallableComponents,Models/31_L95_R7_MOSFETGate_100R.fbx,GLB/31_L95_R7_MOSFETGate_100R.glb,3,48,1,12.0,NO - visual art only
32,L95_R8_GatePulldown_100K,PlayerInstallableComponents,Models/32_L95_R8_GatePulldown_100K.fbx,GLB/32_L95_R8_GatePulldown_100K.glb,3,48,1,12.0,NO - visual art only
33,L95_R9_CurrentShunt_0R1,PlayerInstallableComponents,Models/33_L95_R9_CurrentShunt_0R1.fbx,GLB/33_L95_R9_CurrentShunt_0R1.glb,3,48,1,12.0,NO - visual art only
34,L95_R10_PWMPullup_4K7,PlayerInstallableComponents,Models/34_L95_R10_PWMPullup_4K7.fbx,GLB/34_L95_R10_PWMPullup_4K7.glb,3,48,1,12.0,NO - visual art only
35,L95_R11_SenseFilter_1K,PlayerInstallableComponents,Models/35_L95_R11_SenseFilter_1K.fbx,GLB/35_L95_R11_SenseFilter_1K.glb,3,48,1,12.0,NO - visual art only
36,L95_R12_EnableBias_10K,PlayerInstallableComponents,Models/36_L95_R12_EnableBias_10K.fbx,GLB/36_L95_R12_EnableBias_10K.glb,3,48,1,12.0,NO - visual art only
37,L95_C1_RCTiming_10nF,PlayerInstallableComponents,Models/37_L95_C1_RCTiming_10nF.fbx,GLB/37_L95_C1_RCTiming_10nF.glb,2,36,1,12.0,NO - visual art only
38,L95_C2_ControlFilter_10nF,PlayerInstallableComponents,Models/38_L95_C2_ControlFilter_10nF.fbx,GLB/38_L95_C2_ControlFilter_10nF.glb,2,36,1,12.0,NO - visual art only
39,L95_C3_PowerInput_10uF,PlayerInstallableComponents,Models/39_L95_C3_PowerInput_10uF.fbx,GLB/39_L95_C3_PowerInput_10uF.glb,2,36,1,12.0,NO - visual art only
40,L95_C4_LDOOutput_22uF,PlayerInstallableComponents,Models/40_L95_C4_LDOOutput_22uF.fbx,GLB/40_L95_C4_LDOOutput_22uF.glb,2,36,1,12.0,NO - visual art only
41,L95_C5_TimerDecoupling_100nF,PlayerInstallableComponents,Models/41_L95_C5_TimerDecoupling_100nF.fbx,GLB/41_L95_C5_TimerDecoupling_100nF.glb,2,36,1,12.0,NO - visual art only
42,L95_C6_CounterDecoupling_100nF,PlayerInstallableComponents,Models/42_L95_C6_CounterDecoupling_100nF.fbx,GLB/42_L95_C6_CounterDecoupling_100nF.glb,2,36,1,12.0,NO - visual art only
43,L95_C7_RampFilter_1nF,PlayerInstallableComponents,Models/43_L95_C7_RampFilter_1nF.fbx,GLB/43_L95_C7_RampFilter_1nF.glb,2,36,1,12.0,NO - visual art only
44,L95_C8_PWMFilter_100nF,PlayerInstallableComponents,Models/44_L95_C8_PWMFilter_100nF.fbx,GLB/44_L95_C8_PWMFilter_100nF.glb,2,36,1,12.0,NO - visual art only
45,L95_C9_SafetyDecoupling_100nF,PlayerInstallableComponents,Models/45_L95_C9_SafetyDecoupling_100nF.fbx,GLB/45_L95_C9_SafetyDecoupling_100nF.glb,2,36,1,12.0,NO - visual art only
46,L95_LED1_ClockStatusGreen,PlayerInstallableComponents,Models/46_L95_LED1_ClockStatusGreen.fbx,GLB/46_L95_LED1_ClockStatusGreen.glb,3,48,1,12.0,NO - visual art only
47,L95_LED2_OutputStatusAmber,PlayerInstallableComponents,Models/47_L95_LED2_OutputStatusAmber.fbx,GLB/47_L95_LED2_OutputStatusAmber.glb,3,48,1,12.0,NO - visual art only
48,L95_Q1_OutputMOSFET_SOT23,PlayerInstallableComponents,Models/48_L95_Q1_OutputMOSFET_SOT23.fbx,GLB/48_L95_Q1_OutputMOSFET_SOT23.glb,2,48,2,12.0,NO - visual art only
49,L95_D1_InputProtectionSMA,PlayerInstallableComponents,Models/49_L95_D1_InputProtectionSMA.fbx,GLB/49_L95_D1_InputProtectionSMA.glb,3,48,1,12.0,NO - visual art only
50,L95_RV1_PWMDutyTrimPotentiometer,PlayerInstallableComponents,Models/50_L95_RV1_PWMDutyTrimPotentiometer.fbx,GLB/50_L95_RV1_PWMDutyTrimPotentiometer.glb,4,132,2,12.0,NO - visual art only
51,L95_W049_DetachableMonitorJumper,PlayerInstallableComponents,Models/51_L95_W049_DetachableMonitorJumper.fbx,GLB/51_L95_W049_DetachableMonitorJumper.glb,4,192,2,12.0,NO - visual art only
52,L95_J1_Protected5VTerminal,PlayerInstallableComponents,Models/52_L95_J1_Protected5VTerminal.fbx,GLB/52_L95_J1_Protected5VTerminal.glb,3,300,1,12.0,NO - visual art only
53,L95_J2_PWMAndClockOutputHeader,PlayerInstallableComponents,Models/53_L95_J2_PWMAndClockOutputHeader.fbx,GLB/53_L95_J2_PWMAndClockOutputHeader.glb,2,108,1,12.0,NO - visual art only
```

## ANNEX B — BOM — 36 EXACT PLAYER-INSTALLED COMPONENTS

**Source path:** `Data/Component_BOM_36.csv`

```csv
Ref,Model,Kind,Value,Xmm,Zmm,WidthMM,DepthMM,RotationDegrees,InitialState,CorrectOrientationRequired,PolarityCheck,Placement,ComponentSimulatedAs
U1,L95_U1_TLC555CMOSTimer_SOIC8,timer,TLC555_5V,-29,1,8,7,0,NOT_INSTALLED,YES,YES,SnapToMatchedFootprint,Electrical component with designated port aliases
U2,L95_U2_74HC14_Schmitt_SOIC14,logic14,74HC14_5V,-7,5,12,8,0,NOT_INSTALLED,YES,YES,SnapToMatchedFootprint,Electrical component with designated port aliases
U3,L95_U3_CD4040B_BinaryCounter_SOIC16,logic16,CD4040B_5V,18,8,13,9,0,NOT_INSTALLED,YES,YES,SnapToMatchedFootprint,Electrical component with designated port aliases
U4,L95_U4_LM393_PWMComparator_SOIC8,soic,LM393_5V,40,-14,9,7,0,NOT_INSTALLED,YES,YES,SnapToMatchedFootprint,Electrical component with designated port aliases
U5,L95_U5_3V3LDO_SOT223,ldo,LDO3V3,-43,19,10,7,0,NOT_INSTALLED,YES,YES,SnapToMatchedFootprint,Electrical component with designated port aliases
U6,L95_U6_74HC08_LogicGate_SOIC14,logic14,74HC08_5V,11,-19,11,8,0,NOT_INSTALLED,YES,YES,SnapToMatchedFootprint,Electrical component with designated port aliases
U7,L95_U7_TimingSafetySupervisor_SOIC8,soic,SAFETY_SUPERVISOR,-21,-23,8,7,0,NOT_INSTALLED,YES,YES,SnapToMatchedFootprint,Electrical component with designated port aliases
R1,L95_R1_RCCharge_14K7,res,14K7,-47,10,3.8,2,0,NOT_INSTALLED,YES,NO,SnapToMatchedFootprint,Electrical component with designated port aliases
R2,L95_R2_RCTiming_63K,res,63K,-42,0,3.8,2,0,NOT_INSTALLED,YES,NO,SnapToMatchedFootprint,Electrical component with designated port aliases
R3,L95_R3_DividerBias_10K,res,10K,-32,21,3.8,2,0,NOT_INSTALLED,YES,NO,SnapToMatchedFootprint,Electrical component with designated port aliases
R4,L95_R4_RampCharge_4K7,res,4K7,34,13,3.8,2,0,NOT_INSTALLED,YES,NO,SnapToMatchedFootprint,Electrical component with designated port aliases
R5,L95_R5_ClockLED_1K,res,1K,-28,-34,3.8,2,0,NOT_INSTALLED,YES,NO,SnapToMatchedFootprint,Electrical component with designated port aliases
R6,L95_R6_StatusLED_330R,res,330R,39,-27,3.8,2,0,NOT_INSTALLED,YES,NO,SnapToMatchedFootprint,Electrical component with designated port aliases
R7,L95_R7_MOSFETGate_100R,res,100R,29,-25,3.8,2,0,NOT_INSTALLED,YES,NO,SnapToMatchedFootprint,Electrical component with designated port aliases
R8,L95_R8_GatePulldown_100K,res,100K,42,-33,3.8,2,0,NOT_INSTALLED,YES,NO,SnapToMatchedFootprint,Electrical component with designated port aliases
R9,L95_R9_CurrentShunt_0R1,res,0R1,22,-31,3.8,2,0,NOT_INSTALLED,YES,NO,SnapToMatchedFootprint,Electrical component with designated port aliases
R10,L95_R10_PWMPullup_4K7,res,4K7,50,4,3.8,2,0,NOT_INSTALLED,YES,NO,SnapToMatchedFootprint,Electrical component with designated port aliases
R11,L95_R11_SenseFilter_1K,res,1K,50,-3,3.8,2,0,NOT_INSTALLED,YES,NO,SnapToMatchedFootprint,Electrical component with designated port aliases
R12,L95_R12_EnableBias_10K,res,10K,-7,-31,3.8,2,0,NOT_INSTALLED,YES,NO,SnapToMatchedFootprint,Electrical component with designated port aliases
C1,L95_C1_RCTiming_10nF,cap,10n,-50,18,4.6,2.7,0,NOT_INSTALLED,YES,NO,SnapToMatchedFootprint,Electrical component with designated port aliases
C2,L95_C2_ControlFilter_10nF,cap,10n,-24,13,4.6,2.7,0,NOT_INSTALLED,YES,NO,SnapToMatchedFootprint,Electrical component with designated port aliases
C3,L95_C3_PowerInput_10uF,cap,10u,-43,31,4.6,2.7,0,NOT_INSTALLED,YES,NO,SnapToMatchedFootprint,Electrical component with designated port aliases
C4,L95_C4_LDOOutput_22uF,cap,22u,-30,32,4.6,2.7,0,NOT_INSTALLED,YES,NO,SnapToMatchedFootprint,Electrical component with designated port aliases
C5,L95_C5_TimerDecoupling_100nF,cap,100n,-26,-10,3.7,2,0,NOT_INSTALLED,YES,NO,SnapToMatchedFootprint,Electrical component with designated port aliases
C6,L95_C6_CounterDecoupling_100nF,cap,100n,20,24,3.7,2,0,NOT_INSTALLED,YES,NO,SnapToMatchedFootprint,Electrical component with designated port aliases
C7,L95_C7_RampFilter_1nF,cap,1n,34,-3,3.7,2,0,NOT_INSTALLED,YES,NO,SnapToMatchedFootprint,Electrical component with designated port aliases
C8,L95_C8_PWMFilter_100nF,cap,100n,51,-25,3.7,2,0,NOT_INSTALLED,YES,NO,SnapToMatchedFootprint,Electrical component with designated port aliases
C9,L95_C9_SafetyDecoupling_100nF,cap,100n,-21,-33,3.7,2,0,NOT_INSTALLED,YES,NO,SnapToMatchedFootprint,Electrical component with designated port aliases
LED1,L95_LED1_ClockStatusGreen,led,GREEN,-46,-28,4,2.8,0,NOT_INSTALLED,YES,YES,SnapToMatchedFootprint,Electrical component with designated port aliases
LED2,L95_LED2_OutputStatusAmber,led,AMBER,44,29,4,2.8,0,NOT_INSTALLED,YES,YES,SnapToMatchedFootprint,Electrical component with designated port aliases
Q1,L95_Q1_OutputMOSFET_SOT23,sot,LOW_SIDE_NMOS,29,-34,5,4,0,NOT_INSTALLED,YES,YES,SnapToMatchedFootprint,Electrical component with designated port aliases
D1,L95_D1_InputProtectionSMA,diode,SMA_SCHOTTKY,-52,27,6,3.6,0,NOT_INSTALLED,YES,YES,SnapToMatchedFootprint,Electrical component with designated port aliases
RV1,L95_RV1_PWMDutyTrimPotentiometer,trimmer,PWM_DUTY_ADJUST,-3,29,7,7,0,NOT_INSTALLED,YES,NO,SnapToMatchedFootprint,Electrical component with designated port aliases
W049,L95_W049_DetachableMonitorJumper,jumper,TRUSTED_PWM_FB,47,-20,8,7,0,NOT_INSTALLED,YES,NO,SnapToMatchedFootprint,Electrical component with designated port aliases
J1,L95_J1_Protected5VTerminal,terminal,5V_INPUT,-54,-1,8,11,0,NOT_INSTALLED,YES,YES,SnapToMatchedFootprint,Electrical component with designated port aliases
J2,L95_J2_PWMAndClockOutputHeader,header,PWM_CLK_OUTPUT,53,22,7,10,0,NOT_INSTALLED,YES,NO,SnapToMatchedFootprint,Electrical component with designated port aliases
```

## ANNEX C — PLACEMENT — 36 POSITIONS AND FOOTPRINT SIZES

**Source path:** `Data/Component_Placement_36.csv`

```csv
Ref,Kind,Xmm,Zmm,WidthMM,DepthMM,Value,Model
U1,timer,-29,1,8,7,TLC555_5V,L95_U1_TLC555CMOSTimer_SOIC8
U2,logic14,-7,5,12,8,74HC14_5V,L95_U2_74HC14_Schmitt_SOIC14
U3,logic16,18,8,13,9,CD4040B_5V,L95_U3_CD4040B_BinaryCounter_SOIC16
U4,soic,40,-14,9,7,LM393_5V,L95_U4_LM393_PWMComparator_SOIC8
U5,ldo,-43,19,10,7,LDO3V3,L95_U5_3V3LDO_SOT223
U6,logic14,11,-19,11,8,74HC08_5V,L95_U6_74HC08_LogicGate_SOIC14
U7,soic,-21,-23,8,7,SAFETY_SUPERVISOR,L95_U7_TimingSafetySupervisor_SOIC8
R1,res,-47,10,3.8,2,14K7,L95_R1_RCCharge_14K7
R2,res,-42,0,3.8,2,63K,L95_R2_RCTiming_63K
R3,res,-32,21,3.8,2,10K,L95_R3_DividerBias_10K
R4,res,34,13,3.8,2,4K7,L95_R4_RampCharge_4K7
R5,res,-28,-34,3.8,2,1K,L95_R5_ClockLED_1K
R6,res,39,-27,3.8,2,330R,L95_R6_StatusLED_330R
R7,res,29,-25,3.8,2,100R,L95_R7_MOSFETGate_100R
R8,res,42,-33,3.8,2,100K,L95_R8_GatePulldown_100K
R9,res,22,-31,3.8,2,0R1,L95_R9_CurrentShunt_0R1
R10,res,50,4,3.8,2,4K7,L95_R10_PWMPullup_4K7
R11,res,50,-3,3.8,2,1K,L95_R11_SenseFilter_1K
R12,res,-7,-31,3.8,2,10K,L95_R12_EnableBias_10K
C1,cap,-50,18,4.6,2.7,10n,L95_C1_RCTiming_10nF
C2,cap,-24,13,4.6,2.7,10n,L95_C2_ControlFilter_10nF
C3,cap,-43,31,4.6,2.7,10u,L95_C3_PowerInput_10uF
C4,cap,-30,32,4.6,2.7,22u,L95_C4_LDOOutput_22uF
C5,cap,-26,-10,3.7,2,100n,L95_C5_TimerDecoupling_100nF
C6,cap,20,24,3.7,2,100n,L95_C6_CounterDecoupling_100nF
C7,cap,34,-3,3.7,2,1n,L95_C7_RampFilter_1nF
C8,cap,51,-25,3.7,2,100n,L95_C8_PWMFilter_100nF
C9,cap,-21,-33,3.7,2,100n,L95_C9_SafetyDecoupling_100nF
LED1,led,-46,-28,4,2.8,GREEN,L95_LED1_ClockStatusGreen
LED2,led,44,29,4,2.8,AMBER,L95_LED2_OutputStatusAmber
Q1,sot,29,-34,5,4,LOW_SIDE_NMOS,L95_Q1_OutputMOSFET_SOT23
D1,diode,-52,27,6,3.6,SMA_SCHOTTKY,L95_D1_InputProtectionSMA
RV1,trimmer,-3,29,7,7,PWM_DUTY_ADJUST,L95_RV1_PWMDutyTrimPotentiometer
W049,jumper,47,-20,8,7,TRUSTED_PWM_FB,L95_W049_DetachableMonitorJumper
J1,terminal,-54,-1,8,11,5V_INPUT,L95_J1_Protected5VTerminal
J2,header,53,22,7,10,PWM_CLK_OUTPUT,L95_J2_PWMAndClockOutputHeader
```

## ANNEX D — PAD MAP — ALL 150 LOGICAL CONTACTS

**Source path:** `Data/Logical_Pad_And_Contact_Map.csv`

```csv
PadID,OwnerRef,LogicalSignal,PadIndex,Side,Xmm,Zmm,ArtX,ArtY,ArtZ,OwnerFBX,FBXSocketIsDecorative,PinoutsRequireRealComponentDatasheetReview
U1.GND,U1,GND,1,TOP,-32.76,-1.765,-0.39312,1.277,-0.02118,L95_U1_TLC555CMOSTimer_SOIC8,YES,YES
U1.TRIG,U1,TRIG,2,TOP,-32.76,0.0783,-0.39312,1.277,0.00094,L95_U1_TLC555CMOSTimer_SOIC8,YES,YES
U1.OUT,U1,OUT,3,TOP,-32.76,1.9217,-0.39312,1.277,0.02306,L95_U1_TLC555CMOSTimer_SOIC8,YES,YES
U1.RESET,U1,RESET,4,TOP,-32.76,3.765,-0.39312,1.277,0.04518,L95_U1_TLC555CMOSTimer_SOIC8,YES,YES
U1.CTRL,U1,CTRL,5,TOP,-25.24,-1.765,-0.30288,1.277,-0.02118,L95_U1_TLC555CMOSTimer_SOIC8,YES,YES
U1.THRESH,U1,THRESH,6,TOP,-25.24,0.0783,-0.30288,1.277,0.00094,L95_U1_TLC555CMOSTimer_SOIC8,YES,YES
U1.DISCH,U1,DISCH,7,TOP,-25.24,1.9217,-0.30288,1.277,0.02306,L95_U1_TLC555CMOSTimer_SOIC8,YES,YES
U1.VDD,U1,VDD,8,TOP,-25.24,3.765,-0.30288,1.277,0.04518,L95_U1_TLC555CMOSTimer_SOIC8,YES,YES
U2.IN1,U2,IN1,1,TOP,-12.64,1.84,-0.15168,1.277,0.02208,L95_U2_74HC14_Schmitt_SOIC14,YES,YES
U2.OUT1,U2,OUT1,2,TOP,-12.64,2.8933,-0.15168,1.277,0.03472,L95_U2_74HC14_Schmitt_SOIC14,YES,YES
U2.VCC,U2,VCC,3,TOP,-12.64,3.9467,-0.15168,1.277,0.04736,L95_U2_74HC14_Schmitt_SOIC14,YES,YES
U2.GND,U2,GND,4,TOP,-12.64,5.0,-0.15168,1.277,0.06,L95_U2_74HC14_Schmitt_SOIC14,YES,YES
U2.IN2,U2,IN2,5,TOP,-12.64,6.0533,-0.15168,1.277,0.07264,L95_U2_74HC14_Schmitt_SOIC14,YES,YES
U2.OUT2,U2,OUT2,6,TOP,-12.64,7.1067,-0.15168,1.277,0.08528,L95_U2_74HC14_Schmitt_SOIC14,YES,YES
U2.IN3,U2,IN3,7,TOP,-12.64,8.16,-0.15168,1.277,0.09792,L95_U2_74HC14_Schmitt_SOIC14,YES,YES
U2.OUT3,U2,OUT3,8,TOP,-1.36,1.84,-0.01632,1.277,0.02208,L95_U2_74HC14_Schmitt_SOIC14,YES,YES
U2.IN4,U2,IN4,9,TOP,-1.36,2.8933,-0.01632,1.277,0.03472,L95_U2_74HC14_Schmitt_SOIC14,YES,YES
U2.OUT4,U2,OUT4,10,TOP,-1.36,3.9467,-0.01632,1.277,0.04736,L95_U2_74HC14_Schmitt_SOIC14,YES,YES
U2.IN5,U2,IN5,11,TOP,-1.36,5.0,-0.01632,1.277,0.06,L95_U2_74HC14_Schmitt_SOIC14,YES,YES
U2.OUT5,U2,OUT5,12,TOP,-1.36,6.0533,-0.01632,1.277,0.07264,L95_U2_74HC14_Schmitt_SOIC14,YES,YES
U2.IN6,U2,IN6,13,TOP,-1.36,7.1067,-0.01632,1.277,0.08528,L95_U2_74HC14_Schmitt_SOIC14,YES,YES
U2.OUT6,U2,OUT6,14,TOP,-1.36,8.16,-0.01632,1.277,0.09792,L95_U2_74HC14_Schmitt_SOIC14,YES,YES
U3.CLK,U3,CLK,1,TOP,11.89,4.445,0.14268,1.277,0.05334,L95_U3_CD4040B_BinaryCounter_SOIC16,YES,YES
U3.RESET,U3,RESET,2,TOP,11.89,5.4607,0.14268,1.277,0.065529,L95_U3_CD4040B_BinaryCounter_SOIC16,YES,YES
U3.Q1,U3,Q1,3,TOP,11.89,6.4764,0.14268,1.277,0.077717,L95_U3_CD4040B_BinaryCounter_SOIC16,YES,YES
U3.Q2,U3,Q2,4,TOP,11.89,7.4921,0.14268,1.277,0.089906,L95_U3_CD4040B_BinaryCounter_SOIC16,YES,YES
U3.Q3,U3,Q3,5,TOP,11.89,8.5079,0.14268,1.277,0.102094,L95_U3_CD4040B_BinaryCounter_SOIC16,YES,YES
U3.Q4,U3,Q4,6,TOP,11.89,9.5236,0.14268,1.277,0.114283,L95_U3_CD4040B_BinaryCounter_SOIC16,YES,YES
U3.Q5,U3,Q5,7,TOP,11.89,10.5393,0.14268,1.277,0.126471,L95_U3_CD4040B_BinaryCounter_SOIC16,YES,YES
U3.Q6,U3,Q6,8,TOP,11.89,11.555,0.14268,1.277,0.13866,L95_U3_CD4040B_BinaryCounter_SOIC16,YES,YES
U3.Q7,U3,Q7,9,TOP,24.11,4.445,0.28932,1.277,0.05334,L95_U3_CD4040B_BinaryCounter_SOIC16,YES,YES
U3.Q8,U3,Q8,10,TOP,24.11,5.4607,0.28932,1.277,0.065529,L95_U3_CD4040B_BinaryCounter_SOIC16,YES,YES
U3.Q9,U3,Q9,11,TOP,24.11,6.4764,0.28932,1.277,0.077717,L95_U3_CD4040B_BinaryCounter_SOIC16,YES,YES
U3.Q10,U3,Q10,12,TOP,24.11,7.4921,0.28932,1.277,0.089906,L95_U3_CD4040B_BinaryCounter_SOIC16,YES,YES
U3.Q11,U3,Q11,13,TOP,24.11,8.5079,0.28932,1.277,0.102094,L95_U3_CD4040B_BinaryCounter_SOIC16,YES,YES
U3.Q12,U3,Q12,14,TOP,24.11,9.5236,0.28932,1.277,0.114283,L95_U3_CD4040B_BinaryCounter_SOIC16,YES,YES
U3.VDD,U3,VDD,15,TOP,24.11,10.5393,0.28932,1.277,0.126471,L95_U3_CD4040B_BinaryCounter_SOIC16,YES,YES
U3.GND,U3,GND,16,TOP,24.11,11.555,0.28932,1.277,0.13866,L95_U3_CD4040B_BinaryCounter_SOIC16,YES,YES
U4.VCC,U4,VCC,1,TOP,35.77,-16.765,0.42924,1.277,-0.20118,L95_U4_LM393_PWMComparator_SOIC8,YES,YES
U4.GND,U4,GND,2,TOP,35.77,-14.9217,0.42924,1.277,-0.17906,L95_U4_LM393_PWMComparator_SOIC8,YES,YES
U4.RAMP_SYNC,U4,RAMP_SYNC,3,TOP,35.77,-13.0783,0.42924,1.277,-0.15694,L95_U4_LM393_PWMComparator_SOIC8,YES,YES
U4.RAMP_OUT,U4,RAMP_OUT,4,TOP,35.77,-11.235,0.42924,1.277,-0.13482,L95_U4_LM393_PWMComparator_SOIC8,YES,YES
U4.DUTY_REF,U4,DUTY_REF,5,TOP,44.23,-16.765,0.53076,1.277,-0.20118,L95_U4_LM393_PWMComparator_SOIC8,YES,YES
U4.PWM_OUT,U4,PWM_OUT,6,TOP,44.23,-14.9217,0.53076,1.277,-0.17906,L95_U4_LM393_PWMComparator_SOIC8,YES,YES
U4.COMPARATOR_REF,U4,COMPARATOR_REF,7,TOP,44.23,-13.0783,0.53076,1.277,-0.15694,L95_U4_LM393_PWMComparator_SOIC8,YES,YES
U4.SPARE,U4,SPARE,8,TOP,44.23,-11.235,0.53076,1.277,-0.13482,L95_U4_LM393_PWMComparator_SOIC8,YES,YES
U5.VIN,U5,VIN,1,TOP,-46.6,16.55,-0.5592,1.277,0.1986,L95_U5_3V3LDO_SOT223,YES,YES
U5.GND,U5,GND,2,TOP,-39.4,16.55,-0.4728,1.277,0.1986,L95_U5_3V3LDO_SOT223,YES,YES
U5.VOUT,U5,VOUT,3,TOP,-46.6,21.45,-0.5592,1.277,0.2574,L95_U5_3V3LDO_SOT223,YES,YES
U5.TAB_GND,U5,TAB_GND,4,TOP,-39.4,21.45,-0.4728,1.277,0.2574,L95_U5_3V3LDO_SOT223,YES,YES
U6.VCC,U6,VCC,1,TOP,5.83,-22.16,0.06996,1.277,-0.26592,L95_U6_74HC08_LogicGate_SOIC14,YES,YES
U6.GND,U6,GND,2,TOP,5.83,-21.1067,0.06996,1.277,-0.25328,L95_U6_74HC08_LogicGate_SOIC14,YES,YES
U6.IN_A,U6,IN_A,3,TOP,5.83,-20.0533,0.06996,1.277,-0.24064,L95_U6_74HC08_LogicGate_SOIC14,YES,YES
U6.IN_B,U6,IN_B,4,TOP,5.83,-19.0,0.06996,1.277,-0.228,L95_U6_74HC08_LogicGate_SOIC14,YES,YES
U6.OUT,U6,OUT,5,TOP,5.83,-17.9467,0.06996,1.277,-0.21536,L95_U6_74HC08_LogicGate_SOIC14,YES,YES
U6.AUX_A,U6,AUX_A,6,TOP,5.83,-16.8933,0.06996,1.277,-0.20272,L95_U6_74HC08_LogicGate_SOIC14,YES,YES
U6.AUX_B,U6,AUX_B,7,TOP,5.83,-15.84,0.06996,1.277,-0.19008,L95_U6_74HC08_LogicGate_SOIC14,YES,YES
U6.AUX_OUT,U6,AUX_OUT,8,TOP,16.17,-22.16,0.19404,1.277,-0.26592,L95_U6_74HC08_LogicGate_SOIC14,YES,YES
U6.B_A,U6,B_A,9,TOP,16.17,-21.1067,0.19404,1.277,-0.25328,L95_U6_74HC08_LogicGate_SOIC14,YES,YES
U6.B_B,U6,B_B,10,TOP,16.17,-20.0533,0.19404,1.277,-0.24064,L95_U6_74HC08_LogicGate_SOIC14,YES,YES
U6.B_OUT,U6,B_OUT,11,TOP,16.17,-19.0,0.19404,1.277,-0.228,L95_U6_74HC08_LogicGate_SOIC14,YES,YES
U6.C_A,U6,C_A,12,TOP,16.17,-17.9467,0.19404,1.277,-0.21536,L95_U6_74HC08_LogicGate_SOIC14,YES,YES
U6.C_B,U6,C_B,13,TOP,16.17,-16.8933,0.19404,1.277,-0.20272,L95_U6_74HC08_LogicGate_SOIC14,YES,YES
U6.C_OUT,U6,C_OUT,14,TOP,16.17,-15.84,0.19404,1.277,-0.19008,L95_U6_74HC08_LogicGate_SOIC14,YES,YES
U7.VDD,U7,VDD,1,TOP,-24.76,-25.765,-0.29712,1.277,-0.30918,L95_U7_TimingSafetySupervisor_SOIC8,YES,YES
U7.GND,U7,GND,2,TOP,-24.76,-23.9217,-0.29712,1.277,-0.28706,L95_U7_TimingSafetySupervisor_SOIC8,YES,YES
U7.SLOW_CLK,U7,SLOW_CLK,3,TOP,-24.76,-22.0783,-0.29712,1.277,-0.26494,L95_U7_TimingSafetySupervisor_SOIC8,YES,YES
U7.SAFE_ENABLE,U7,SAFE_ENABLE,4,TOP,-24.76,-20.235,-0.29712,1.277,-0.24282,L95_U7_TimingSafetySupervisor_SOIC8,YES,YES
U7.STAT_OUT,U7,STAT_OUT,5,TOP,-17.24,-25.765,-0.20688,1.277,-0.30918,L95_U7_TimingSafetySupervisor_SOIC8,YES,YES
U7.TRUSTED_PWM_MONITOR_IN,U7,TRUSTED_PWM_MONITOR_IN,6,TOP,-17.24,-23.9217,-0.20688,1.277,-0.28706,L95_U7_TimingSafetySupervisor_SOIC8,YES,YES
U7.CURRENT_SENSE,U7,CURRENT_SENSE,7,TOP,-17.24,-22.0783,-0.20688,1.277,-0.26494,L95_U7_TimingSafetySupervisor_SOIC8,YES,YES
U7.RESET_CMD,U7,RESET_CMD,8,TOP,-17.24,-20.235,-0.20688,1.277,-0.24282,L95_U7_TimingSafetySupervisor_SOIC8,YES,YES
R1.A,R1,A,1,TOP,-48.71,10,-0.58452,1.277,0.12,L95_R1_RCCharge_14K7,YES,YES
R1.B,R1,B,2,TOP,-45.29,10,-0.54348,1.277,0.12,L95_R1_RCCharge_14K7,YES,YES
R2.A,R2,A,1,TOP,-43.71,0,-0.52452,1.277,0.0,L95_R2_RCTiming_63K,YES,YES
R2.B,R2,B,2,TOP,-40.29,0,-0.48348,1.277,0.0,L95_R2_RCTiming_63K,YES,YES
R3.A,R3,A,1,TOP,-33.71,21,-0.40452,1.277,0.252,L95_R3_DividerBias_10K,YES,YES
R3.B,R3,B,2,TOP,-30.29,21,-0.36348,1.277,0.252,L95_R3_DividerBias_10K,YES,YES
R4.A,R4,A,1,TOP,32.29,13,0.38748,1.277,0.156,L95_R4_RampCharge_4K7,YES,YES
R4.B,R4,B,2,TOP,35.71,13,0.42852,1.277,0.156,L95_R4_RampCharge_4K7,YES,YES
R5.A,R5,A,1,TOP,-29.71,-34,-0.35652,1.277,-0.408,L95_R5_ClockLED_1K,YES,YES
R5.B,R5,B,2,TOP,-26.29,-34,-0.31548,1.277,-0.408,L95_R5_ClockLED_1K,YES,YES
R6.A,R6,A,1,TOP,37.29,-27,0.44748,1.277,-0.324,L95_R6_StatusLED_330R,YES,YES
R6.B,R6,B,2,TOP,40.71,-27,0.48852,1.277,-0.324,L95_R6_StatusLED_330R,YES,YES
R7.A,R7,A,1,TOP,27.29,-25,0.32748,1.277,-0.3,L95_R7_MOSFETGate_100R,YES,YES
R7.B,R7,B,2,TOP,30.71,-25,0.36852,1.277,-0.3,L95_R7_MOSFETGate_100R,YES,YES
R8.A,R8,A,1,TOP,40.29,-33,0.48348,1.277,-0.396,L95_R8_GatePulldown_100K,YES,YES
R8.B,R8,B,2,TOP,43.71,-33,0.52452,1.277,-0.396,L95_R8_GatePulldown_100K,YES,YES
R9.A,R9,A,1,TOP,20.29,-31,0.24348,1.277,-0.372,L95_R9_CurrentShunt_0R1,YES,YES
R9.B,R9,B,2,TOP,23.71,-31,0.28452,1.277,-0.372,L95_R9_CurrentShunt_0R1,YES,YES
R10.A,R10,A,1,TOP,48.29,4,0.57948,1.277,0.048,L95_R10_PWMPullup_4K7,YES,YES
R10.B,R10,B,2,TOP,51.71,4,0.62052,1.277,0.048,L95_R10_PWMPullup_4K7,YES,YES
R11.A,R11,A,1,TOP,48.29,-3,0.57948,1.277,-0.036,L95_R11_SenseFilter_1K,YES,YES
R11.B,R11,B,2,TOP,51.71,-3,0.62052,1.277,-0.036,L95_R11_SenseFilter_1K,YES,YES
R12.A,R12,A,1,TOP,-8.71,-31,-0.10452,1.277,-0.372,L95_R12_EnableBias_10K,YES,YES
R12.B,R12,B,2,TOP,-5.29,-31,-0.06348,1.277,-0.372,L95_R12_EnableBias_10K,YES,YES
C1.POS,C1,POS,1,TOP,-52.07,18,-0.62484,1.277,0.216,L95_C1_RCTiming_10nF,YES,YES
C1.NEG,C1,NEG,2,TOP,-47.93,18,-0.57516,1.277,0.216,L95_C1_RCTiming_10nF,YES,YES
C2.POS,C2,POS,1,TOP,-26.07,13,-0.31284,1.277,0.156,L95_C2_ControlFilter_10nF,YES,YES
C2.NEG,C2,NEG,2,TOP,-21.93,13,-0.26316,1.277,0.156,L95_C2_ControlFilter_10nF,YES,YES
C3.POS,C3,POS,1,TOP,-45.07,31,-0.54084,1.277,0.372,L95_C3_PowerInput_10uF,YES,YES
C3.NEG,C3,NEG,2,TOP,-40.93,31,-0.49116,1.277,0.372,L95_C3_PowerInput_10uF,YES,YES
C4.POS,C4,POS,1,TOP,-32.07,32,-0.38484,1.277,0.384,L95_C4_LDOOutput_22uF,YES,YES
C4.NEG,C4,NEG,2,TOP,-27.93,32,-0.33516,1.277,0.384,L95_C4_LDOOutput_22uF,YES,YES
C5.POS,C5,POS,1,TOP,-27.665,-10,-0.33198,1.277,-0.12,L95_C5_TimerDecoupling_100nF,YES,YES
C5.NEG,C5,NEG,2,TOP,-24.335,-10,-0.29202,1.277,-0.12,L95_C5_TimerDecoupling_100nF,YES,YES
C6.POS,C6,POS,1,TOP,18.335,24,0.22002,1.277,0.288,L95_C6_CounterDecoupling_100nF,YES,YES
C6.NEG,C6,NEG,2,TOP,21.665,24,0.25998,1.277,0.288,L95_C6_CounterDecoupling_100nF,YES,YES
C7.POS,C7,POS,1,TOP,32.335,-3,0.38802,1.277,-0.036,L95_C7_RampFilter_1nF,YES,YES
C7.NEG,C7,NEG,2,TOP,35.665,-3,0.42798,1.277,-0.036,L95_C7_RampFilter_1nF,YES,YES
C8.POS,C8,POS,1,TOP,49.335,-25,0.59202,1.277,-0.3,L95_C8_PWMFilter_100nF,YES,YES
C8.NEG,C8,NEG,2,TOP,52.665,-25,0.63198,1.277,-0.3,L95_C8_PWMFilter_100nF,YES,YES
C9.POS,C9,POS,1,TOP,-22.665,-33,-0.27198,1.277,-0.396,L95_C9_SafetyDecoupling_100nF,YES,YES
C9.NEG,C9,NEG,2,TOP,-19.335,-33,-0.23202,1.277,-0.396,L95_C9_SafetyDecoupling_100nF,YES,YES
LED1.A,LED1,A,1,TOP,-47.8,-28,-0.5736,1.277,-0.336,L95_LED1_ClockStatusGreen,YES,YES
LED1.C,LED1,C,2,TOP,-44.2,-28,-0.5304,1.277,-0.336,L95_LED1_ClockStatusGreen,YES,YES
LED2.A,LED2,A,1,TOP,42.2,29,0.5064,1.277,0.348,L95_LED2_OutputStatusAmber,YES,YES
LED2.C,LED2,C,2,TOP,45.8,29,0.5496,1.277,0.348,L95_LED2_OutputStatusAmber,YES,YES
Q1.G,Q1,G,1,TOP,27.1,-35.4,0.3252,1.277,-0.4248,L95_Q1_OutputMOSFET_SOT23,YES,YES
Q1.S,Q1,S,2,TOP,30.9,-35.4,0.3708,1.277,-0.4248,L95_Q1_OutputMOSFET_SOT23,YES,YES
Q1.D,Q1,D,3,TOP,29,-32.4,0.348,1.277,-0.3888,L95_Q1_OutputMOSFET_SOT23,YES,YES
D1.A,D1,A,1,TOP,-54.7,27,-0.6564,1.277,0.324,L95_D1_InputProtectionSMA,YES,YES
D1.K,D1,K,2,TOP,-49.3,27,-0.5916,1.277,0.324,L95_D1_InputProtectionSMA,YES,YES
RV1.HI,RV1,HI,1,TOP,-5.66,26.55,-0.06792,1.277,0.3186,L95_RV1_PWMDutyTrimPotentiometer,YES,YES
RV1.WIPER,RV1,WIPER,2,TOP,-0.34,26.55,-0.00408,1.277,0.3186,L95_RV1_PWMDutyTrimPotentiometer,YES,YES
RV1.LO,RV1,LO,3,TOP,-3,31.8,-0.036,1.277,0.3816,L95_RV1_PWMDutyTrimPotentiometer,YES,YES
W049.A,W049,A,1,TOP,43.4,-20,0.5208,1.277,-0.24,L95_W049_DetachableMonitorJumper,YES,YES
W049.B,W049,B,2,TOP,50.6,-20,0.6072,1.277,-0.24,L95_W049_DetachableMonitorJumper,YES,YES
J1.5V_IN,J1,5V_IN,1,TOP,-57.6,-1,-0.6912,1.277,-0.012,L95_J1_Protected5VTerminal,YES,YES
J1.GND,J1,GND,2,TOP,-50.4,-1,-0.6048,1.277,-0.012,L95_J1_Protected5VTerminal,YES,YES
J2.LOAD_POS,J2,LOAD_POS,1,TOP,49.71,18.05,0.59652,1.277,0.2166,L95_J2_PWMAndClockOutputHeader,YES,YES
J2.LOAD_NEG,J2,LOAD_NEG,2,TOP,49.71,20.6833,0.59652,1.277,0.2482,L95_J2_PWMAndClockOutputHeader,YES,YES
J2.CLK1024,J2,CLK1024,3,TOP,49.71,23.3167,0.59652,1.277,0.2798,L95_J2_PWMAndClockOutputHeader,YES,YES
J2.CLK256,J2,CLK256,4,TOP,49.71,25.95,0.59652,1.277,0.3114,L95_J2_PWMAndClockOutputHeader,YES,YES
J2.CLK2,J2,CLK2,5,TOP,56.29,18.05,0.67548,1.277,0.2166,L95_J2_PWMAndClockOutputHeader,YES,YES
J2.PWM,J2,PWM,6,TOP,56.29,20.6833,0.67548,1.277,0.2482,L95_J2_PWMAndClockOutputHeader,YES,YES
J2.GND,J2,GND,7,TOP,56.29,23.3167,0.67548,1.277,0.2798,L95_J2_PWMAndClockOutputHeader,YES,YES
J2.3V3,J2,3V3,8,TOP,56.29,25.95,0.67548,1.277,0.3114,L95_J2_PWMAndClockOutputHeader,YES,YES
J3.A,W049,W049_A,1,TOP,45.24,-20,0.54288,1.277,-0.24,L95_W049_DetachableMonitorJumper,YES,YES
J3.B,W049,W049_B,2,TOP,48.76,-20,0.58512,1.277,-0.24,L95_W049_DetachableMonitorJumper,YES,YES
TP01.PROBE,TP01,TEST_POINT,1,TOP,-55,-35,-0.66,1.277,-0.42,L95_TopGoldSMDPads,YES,NO
TP02.PROBE,TP02,TEST_POINT,2,TOP,-49,36,-0.588,1.277,0.432,L95_TopGoldSMDPads,YES,NO
TP03.PROBE,TP03,TEST_POINT,3,TOP,-15,34,-0.18,1.277,0.408,L95_TopGoldSMDPads,YES,NO
TP04.PROBE,TP04,TEST_POINT,4,TOP,9,35,0.108,1.277,0.42,L95_TopGoldSMDPads,YES,NO
TP05.PROBE,TP05,TEST_POINT,5,TOP,29,34,0.348,1.277,0.408,L95_TopGoldSMDPads,YES,NO
TP06.PROBE,TP06,TEST_POINT,6,TOP,55,35,0.66,1.277,0.42,L95_TopGoldSMDPads,YES,NO
TP07.PROBE,TP07,TEST_POINT,7,TOP,-54,-35,-0.648,1.277,-0.42,L95_TopGoldSMDPads,YES,NO
TP08.PROBE,TP08,TEST_POINT,8,TOP,-12,-35,-0.144,1.277,-0.42,L95_TopGoldSMDPads,YES,NO
TP09.PROBE,TP09,TEST_POINT,9,TOP,19,-35,0.228,1.277,-0.42,L95_TopGoldSMDPads,YES,NO
TP10.PROBE,TP10,TEST_POINT,10,TOP,54,-35,0.648,1.277,-0.42,L95_TopGoldSMDPads,YES,NO
```

## ANNEX E — PLAYER COPPER TRACE REQUIREMENTS — W001–W048

**Source path:** `Data/Copper_Trace_Tasks_W001_W048.csv`

```csv
WireID,SourcePad,DestinationPad,Net,SignalDomain,RequiredLayer,RequiredVia,TraceWidthMM,MinClearanceMM,InitialState,NoShortsRequired,Qualification,VisualCopperTraceModel
W001,J1.5V_IN,D1.A,VIN_RAW,power,F.Cu,,0.45,0.2,NOT_ROUTED,YES,Independent route-ID evidence + actual electrical connectivity,RUNTIME_GENERATED_AFTER_PLAYER_ROUTING
W002,D1.K,U1.VDD,+5V_PROT,power,F.Cu,,0.45,0.2,NOT_ROUTED,YES,Independent route-ID evidence + actual electrical connectivity,RUNTIME_GENERATED_AFTER_PLAYER_ROUTING
W003,D1.K,U2.VCC,+5V_PROT,power,F.Cu,,0.45,0.2,NOT_ROUTED,YES,Independent route-ID evidence + actual electrical connectivity,RUNTIME_GENERATED_AFTER_PLAYER_ROUTING
W004,D1.K,U3.VDD,+5V_PROT,power,F.Cu+B.Cu,V01,0.45,0.2,NOT_ROUTED,YES,Independent route-ID evidence + actual electrical connectivity,RUNTIME_GENERATED_AFTER_PLAYER_ROUTING
W005,D1.K,U4.VCC,+5V_PROT,power,F.Cu,,0.45,0.2,NOT_ROUTED,YES,Independent route-ID evidence + actual electrical connectivity,RUNTIME_GENERATED_AFTER_PLAYER_ROUTING
W006,D1.K,U5.VIN,+5V_PROT,power,F.Cu,,0.45,0.2,NOT_ROUTED,YES,Independent route-ID evidence + actual electrical connectivity,RUNTIME_GENERATED_AFTER_PLAYER_ROUTING
W007,D1.K,U6.VCC,+5V_PROT,power,F.Cu,,0.45,0.2,NOT_ROUTED,YES,Independent route-ID evidence + actual electrical connectivity,RUNTIME_GENERATED_AFTER_PLAYER_ROUTING
W008,D1.K,U7.VDD,+5V_PROT,power,F.Cu,,0.45,0.2,NOT_ROUTED,YES,Independent route-ID evidence + actual electrical connectivity,RUNTIME_GENERATED_AFTER_PLAYER_ROUTING
W009,J1.GND,U1.GND,GND,ground,F.Cu,,0.45,0.2,NOT_ROUTED,YES,Independent route-ID evidence + actual electrical connectivity,RUNTIME_GENERATED_AFTER_PLAYER_ROUTING
W010,J1.GND,U2.GND,GND,ground,F.Cu+B.Cu,V02,0.45,0.2,NOT_ROUTED,YES,Independent route-ID evidence + actual electrical connectivity,RUNTIME_GENERATED_AFTER_PLAYER_ROUTING
W011,J1.GND,U3.GND,GND,ground,F.Cu,,0.45,0.2,NOT_ROUTED,YES,Independent route-ID evidence + actual electrical connectivity,RUNTIME_GENERATED_AFTER_PLAYER_ROUTING
W012,J1.GND,U4.GND,GND,ground,F.Cu,,0.45,0.2,NOT_ROUTED,YES,Independent route-ID evidence + actual electrical connectivity,RUNTIME_GENERATED_AFTER_PLAYER_ROUTING
W013,J1.GND,U5.GND,GND,ground,F.Cu,,0.45,0.2,NOT_ROUTED,YES,Independent route-ID evidence + actual electrical connectivity,RUNTIME_GENERATED_AFTER_PLAYER_ROUTING
W014,J1.GND,U6.GND,GND,ground,F.Cu,,0.45,0.2,NOT_ROUTED,YES,Independent route-ID evidence + actual electrical connectivity,RUNTIME_GENERATED_AFTER_PLAYER_ROUTING
W015,J1.GND,U7.GND,GND,ground,F.Cu,,0.45,0.2,NOT_ROUTED,YES,Independent route-ID evidence + actual electrical connectivity,RUNTIME_GENERATED_AFTER_PLAYER_ROUTING
W016,U5.VOUT,C4.POS,+3V3_MON,power,F.Cu+B.Cu,V03,0.45,0.2,NOT_ROUTED,YES,Independent route-ID evidence + actual electrical connectivity,RUNTIME_GENERATED_AFTER_PLAYER_ROUTING
W017,C4.NEG,J1.GND,GND,ground,F.Cu,,0.45,0.2,NOT_ROUTED,YES,Independent route-ID evidence + actual electrical connectivity,RUNTIME_GENERATED_AFTER_PLAYER_ROUTING
W018,D1.K,R1.A,OSC_CHARGE,power,F.Cu,,0.45,0.2,NOT_ROUTED,YES,Independent route-ID evidence + actual electrical connectivity,RUNTIME_GENERATED_AFTER_PLAYER_ROUTING
W019,R1.B,U1.DISCH,OSC_RA_NODE,analog,F.Cu,,0.22,0.2,NOT_ROUTED,YES,Independent route-ID evidence + actual electrical connectivity,RUNTIME_GENERATED_AFTER_PLAYER_ROUTING
W020,U1.DISCH,R2.A,OSC_RB_NODE,analog,F.Cu+B.Cu,V04,0.22,0.2,NOT_ROUTED,YES,Independent route-ID evidence + actual electrical connectivity,RUNTIME_GENERATED_AFTER_PLAYER_ROUTING
W021,R2.B,U1.THRESH,OSC_CAP_NODE,analog,F.Cu,,0.22,0.2,NOT_ROUTED,YES,Independent route-ID evidence + actual electrical connectivity,RUNTIME_GENERATED_AFTER_PLAYER_ROUTING
W022,U1.THRESH,U1.TRIG,OSC_CAP_NODE,analog,F.Cu,,0.22,0.2,NOT_ROUTED,YES,Independent route-ID evidence + actual electrical connectivity,RUNTIME_GENERATED_AFTER_PLAYER_ROUTING
W023,U1.TRIG,C1.POS,OSC_CAP_NODE,analog,F.Cu,,0.22,0.2,NOT_ROUTED,YES,Independent route-ID evidence + actual electrical connectivity,RUNTIME_GENERATED_AFTER_PLAYER_ROUTING
W024,C1.NEG,J1.GND,GND,ground,F.Cu+B.Cu,V05,0.45,0.2,NOT_ROUTED,YES,Independent route-ID evidence + actual electrical connectivity,RUNTIME_GENERATED_AFTER_PLAYER_ROUTING
W025,U1.RESET,D1.K,OSC_ENABLE,digital,F.Cu,,0.22,0.2,NOT_ROUTED,YES,Independent route-ID evidence + actual electrical connectivity,RUNTIME_GENERATED_AFTER_PLAYER_ROUTING
W026,U1.CTRL,C2.POS,OSC_CTRL_FILTER,analog,F.Cu,,0.22,0.2,NOT_ROUTED,YES,Independent route-ID evidence + actual electrical connectivity,RUNTIME_GENERATED_AFTER_PLAYER_ROUTING
W027,C2.NEG,J1.GND,GND,ground,F.Cu,,0.45,0.2,NOT_ROUTED,YES,Independent route-ID evidence + actual electrical connectivity,RUNTIME_GENERATED_AFTER_PLAYER_ROUTING
W028,U1.OUT,U2.IN1,CLK1024_RAW,digital,F.Cu+B.Cu,V06,0.22,0.2,NOT_ROUTED,YES,Independent route-ID evidence + actual electrical connectivity,RUNTIME_GENERATED_AFTER_PLAYER_ROUTING
W029,U2.OUT1,U3.CLK,CLK1024_COND,digital,F.Cu,,0.22,0.2,NOT_ROUTED,YES,Independent route-ID evidence + actual electrical connectivity,RUNTIME_GENERATED_AFTER_PLAYER_ROUTING
W030,U3.Q2,U4.RAMP_SYNC,CLK256,digital,F.Cu,,0.22,0.2,NOT_ROUTED,YES,Independent route-ID evidence + actual electrical connectivity,RUNTIME_GENERATED_AFTER_PLAYER_ROUTING
W031,U3.Q9,U7.SLOW_CLK,CLK2,digital,F.Cu,,0.22,0.2,NOT_ROUTED,YES,Independent route-ID evidence + actual electrical connectivity,RUNTIME_GENERATED_AFTER_PLAYER_ROUTING
W032,U4.PWM_OUT,U6.IN_A,PWM_RAW,digital,F.Cu+B.Cu,V07,0.22,0.2,NOT_ROUTED,YES,Independent route-ID evidence + actual electrical connectivity,RUNTIME_GENERATED_AFTER_PLAYER_ROUTING
W033,U7.SAFE_ENABLE,U6.IN_B,PWM_GATE_PERMISSION,digital,F.Cu,,0.22,0.2,NOT_ROUTED,YES,Independent route-ID evidence + actual electrical connectivity,RUNTIME_GENERATED_AFTER_PLAYER_ROUTING
W034,U6.OUT,R7.A,GATED_PWM,digital,F.Cu,,0.22,0.2,NOT_ROUTED,YES,Independent route-ID evidence + actual electrical connectivity,RUNTIME_GENERATED_AFTER_PLAYER_ROUTING
W035,R7.B,Q1.G,MOSFET_GATE,digital,F.Cu,,0.22,0.2,NOT_ROUTED,YES,Independent route-ID evidence + actual electrical connectivity,RUNTIME_GENERATED_AFTER_PLAYER_ROUTING
W036,Q1.S,R9.A,SHUNT_SENSE,analog,F.Cu+B.Cu,V08,0.22,0.2,NOT_ROUTED,YES,Independent route-ID evidence + actual electrical connectivity,RUNTIME_GENERATED_AFTER_PLAYER_ROUTING
W037,R9.B,J1.GND,GND,ground,F.Cu,,0.45,0.2,NOT_ROUTED,YES,Independent route-ID evidence + actual electrical connectivity,RUNTIME_GENERATED_AFTER_PLAYER_ROUTING
W038,Q1.D,J2.LOAD_NEG,LOAD_SWITCHED,power,F.Cu,,0.45,0.2,NOT_ROUTED,YES,Independent route-ID evidence + actual electrical connectivity,RUNTIME_GENERATED_AFTER_PLAYER_ROUTING
W039,D1.K,J2.LOAD_POS,LOAD_POSITIVE,power,F.Cu,,0.45,0.2,NOT_ROUTED,YES,Independent route-ID evidence + actual electrical connectivity,RUNTIME_GENERATED_AFTER_PLAYER_ROUTING
W040,U3.Q2,R5.A,CLK256_LED_DRV,digital,F.Cu,,0.22,0.2,NOT_ROUTED,YES,Independent route-ID evidence + actual electrical connectivity,RUNTIME_GENERATED_AFTER_PLAYER_ROUTING
W041,R5.B,LED1.A,CLK_LED_SERIES,power,F.Cu+B.Cu,V09,0.45,0.2,NOT_ROUTED,YES,Independent route-ID evidence + actual electrical connectivity,RUNTIME_GENERATED_AFTER_PLAYER_ROUTING
W042,LED1.C,J1.GND,GND,ground,F.Cu,,0.45,0.2,NOT_ROUTED,YES,Independent route-ID evidence + actual electrical connectivity,RUNTIME_GENERATED_AFTER_PLAYER_ROUTING
W043,U7.STAT_OUT,R6.A,STATUS_LED_DRV,digital,F.Cu,,0.22,0.2,NOT_ROUTED,YES,Independent route-ID evidence + actual electrical connectivity,RUNTIME_GENERATED_AFTER_PLAYER_ROUTING
W044,R6.B,LED2.A,STATUS_LED_SERIES,power,F.Cu,,0.45,0.2,NOT_ROUTED,YES,Independent route-ID evidence + actual electrical connectivity,RUNTIME_GENERATED_AFTER_PLAYER_ROUTING
W045,LED2.C,J1.GND,GND,ground,F.Cu,,0.45,0.2,NOT_ROUTED,YES,Independent route-ID evidence + actual electrical connectivity,RUNTIME_GENERATED_AFTER_PLAYER_ROUTING
W046,RV1.WIPER,U4.DUTY_REF,DUTY_REFERENCE,analog,F.Cu+B.Cu,V10,0.22,0.2,NOT_ROUTED,YES,Independent route-ID evidence + actual electrical connectivity,RUNTIME_GENERATED_AFTER_PLAYER_ROUTING
W047,D1.K,RV1.HI,DUTY_SUPPLY,power,F.Cu,,0.45,0.2,NOT_ROUTED,YES,Independent route-ID evidence + actual electrical connectivity,RUNTIME_GENERATED_AFTER_PLAYER_ROUTING
W048,RV1.LO,J1.GND,GND,ground,F.Cu,,0.45,0.2,NOT_ROUTED,YES,Independent route-ID evidence + actual electrical connectivity,RUNTIME_GENERATED_AFTER_PLAYER_ROUTING
```

## ANNEX F — REQUIRED INTERLAYER VIAS — V01–V10

**Source path:** `Data/Required_Vias_V01_V10.csv`

```csv
ViaID,RouteID,Xmm,Zmm,DrillMM,AnnularPadMM,LayerTop,LayerBottom,InitialState,PhysicalViaRequired
V01,W004,-48,24,0.35,0.78,F.Cu,B.Cu,NOT_PLACED,YES
V02,W010,-35,-10,0.35,0.78,F.Cu,B.Cu,NOT_PLACED,YES
V03,W016,-27,27,0.35,0.78,F.Cu,B.Cu,NOT_PLACED,YES
V04,W020,-6,19,0.35,0.78,F.Cu,B.Cu,NOT_PLACED,YES
V05,W024,11,-31,0.35,0.78,F.Cu,B.Cu,NOT_PLACED,YES
V06,W028,8,27,0.35,0.78,F.Cu,B.Cu,NOT_PLACED,YES
V07,W032,29,-5,0.35,0.78,F.Cu,B.Cu,NOT_PLACED,YES
V08,W036,39,-37,0.35,0.78,F.Cu,B.Cu,NOT_PLACED,YES
V09,W041,-38,-29,0.35,0.78,F.Cu,B.Cu,NOT_PLACED,YES
V10,W046,49,12,0.35,0.78,F.Cu,B.Cu,NOT_PLACED,YES
```

## ANNEX G — FACTORY BRANCHES AND TEST POINTS — 35 SOURCE EDGES

**Source path:** `Data/Factory_Net_And_Testpoint_Branches.csv`

```csv
EndpointA,EndpointB,InitialState,TaskType,MayBypassRequiredRoute
C3.POS,D1.K,FACTORY_CONNECTED,NOT_PLAYER_ROUTING,NO; player route IDs also verified independently
C3.NEG,J1.GND,FACTORY_CONNECTED,NOT_PLAYER_ROUTING,NO; player route IDs also verified independently
C5.POS,U1.VDD,FACTORY_CONNECTED,NOT_PLAYER_ROUTING,NO; player route IDs also verified independently
C5.NEG,U1.GND,FACTORY_CONNECTED,NOT_PLAYER_ROUTING,NO; player route IDs also verified independently
C6.POS,U3.VDD,FACTORY_CONNECTED,NOT_PLAYER_ROUTING,NO; player route IDs also verified independently
C6.NEG,U3.GND,FACTORY_CONNECTED,NOT_PLAYER_ROUTING,NO; player route IDs also verified independently
C9.POS,U7.VDD,FACTORY_CONNECTED,NOT_PLAYER_ROUTING,NO; player route IDs also verified independently
C9.NEG,U7.GND,FACTORY_CONNECTED,NOT_PLAYER_ROUTING,NO; player route IDs also verified independently
C7.POS,U4.RAMP_OUT,FACTORY_CONNECTED,NOT_PLAYER_ROUTING,NO; player route IDs also verified independently
C7.NEG,U4.GND,FACTORY_CONNECTED,NOT_PLAYER_ROUTING,NO; player route IDs also verified independently
C8.POS,U7.CURRENT_SENSE,FACTORY_CONNECTED,NOT_PLAYER_ROUTING,NO; player route IDs also verified independently
C8.NEG,U7.GND,FACTORY_CONNECTED,NOT_PLAYER_ROUTING,NO; player route IDs also verified independently
R3.A,U3.RESET,FACTORY_CONNECTED,NOT_PLAYER_ROUTING,NO; player route IDs also verified independently
R3.B,U3.GND,FACTORY_CONNECTED,NOT_PLAYER_ROUTING,NO; player route IDs also verified independently
R4.A,U4.RAMP_OUT,FACTORY_CONNECTED,NOT_PLAYER_ROUTING,NO; player route IDs also verified independently
R4.B,U4.RAMP_SYNC,FACTORY_CONNECTED,NOT_PLAYER_ROUTING,NO; player route IDs also verified independently
R8.A,Q1.G,FACTORY_CONNECTED,NOT_PLAYER_ROUTING,NO; player route IDs also verified independently
R8.B,J1.GND,FACTORY_CONNECTED,NOT_PLAYER_ROUTING,NO; player route IDs also verified independently
R10.A,D1.K,FACTORY_CONNECTED,NOT_PLAYER_ROUTING,NO; player route IDs also verified independently
R10.B,U4.PWM_OUT,FACTORY_CONNECTED,NOT_PLAYER_ROUTING,NO; player route IDs also verified independently
R11.A,Q1.S,FACTORY_CONNECTED,NOT_PLAYER_ROUTING,NO; player route IDs also verified independently
R11.B,U7.CURRENT_SENSE,FACTORY_CONNECTED,NOT_PLAYER_ROUTING,NO; player route IDs also verified independently
R12.A,U7.SAFE_ENABLE,FACTORY_CONNECTED,NOT_PLAYER_ROUTING,NO; player route IDs also verified independently
R12.B,J1.GND,FACTORY_CONNECTED,NOT_PLAYER_ROUTING,NO; player route IDs also verified independently
U1.OUT,TP01.PROBE,FACTORY_CONNECTED,NOT_PLAYER_ROUTING,NO; player route IDs also verified independently
U3.Q2,TP02.PROBE,FACTORY_CONNECTED,NOT_PLAYER_ROUTING,NO; player route IDs also verified independently
U3.Q9,TP03.PROBE,FACTORY_CONNECTED,NOT_PLAYER_ROUTING,NO; player route IDs also verified independently
U4.PWM_OUT,TP04.PROBE,FACTORY_CONNECTED,NOT_PLAYER_ROUTING,NO; player route IDs also verified independently
U6.OUT,TP05.PROBE,FACTORY_CONNECTED,NOT_PLAYER_ROUTING,NO; player route IDs also verified independently
U5.VOUT,TP06.PROBE,FACTORY_CONNECTED,NOT_PLAYER_ROUTING,NO; player route IDs also verified independently
U7.STAT_OUT,TP07.PROBE,FACTORY_CONNECTED,NOT_PLAYER_ROUTING,NO; player route IDs also verified independently
U1.TRIG,TP08.PROBE,FACTORY_CONNECTED,NOT_PLAYER_ROUTING,NO; player route IDs also verified independently
J2.LOAD_NEG,TP09.PROBE,FACTORY_CONNECTED,NOT_PLAYER_ROUTING,NO; player route IDs also verified independently
U7.TRUSTED_PWM_MONITOR_IN,J3.B,FACTORY_CONNECTED,NOT_PLAYER_ROUTING,NO; player route IDs also verified independently
J2.LOAD_NEG,J3.A,FACTORY_CONNECTED,NOT_PLAYER_ROUTING,NO; player route IDs also verified independently
```

## ANNEX H — SOURCE ROUTE GROUPS

**Source path:** `Data/Copper_Route_Groups.csv`

```csv
Range,Subsystem,Count
W001-W015,Power ground and protected rails,15
W016-W027,Oscillator RC and LDO filters,12
W028-W031,Conditioning and frequency divider,4
W032-W039,PWM and MOSFET driver,8
W040-W048,Indicators and duty control,9
W049,Removable critical feedback jumper,1
```

## ANNEX I — CRITICAL W049 SOURCE MAPPING

**Source path:** `Data/Critical_W049_Feedback.csv`

```csv
WireID,PhysicalComponentRef,PhysicalModel,Signal,FromLogicalDevice,FromPort,FromContact,ToLogicalDevice,ToPort,ToContact,InitialInstalled,InstallForObjectives,RemoveAtObjective,DisconnectionReaction,Restoration,NeverInterpretOpenAs
W049,W049,L95_W049_DetachableMonitorJumper,TRUSTED_PWM_MONITOR,L95_PWMOutputFeedbackMonitor,TRUSTED_PWM_MONITOR_OUT,J3.A,L95_TimingSafetyMonitor_U7,TRUSTED_PWM_MONITOR_IN,J3.B,NO,"02,03,04,05",06,"PWM_LOAD_DISABLED, fault latched; keep diagnostic clock available",Reinstall physical jumper + fresh validated feedback + explicit safety reset + full new qualification,Valid zero duty/zero current
```

## ANNEX J — SIX OBJECTIVE DEFINITIONS

**Source path:** `Data/Objective_Certification_Plan_6.csv`

```csv
ObjectiveID,Title,QualificationEvidence,InitialStatus,Authority
01,Inspect and assemble timing board,"All 36 correct placeable components with value, polarity, placement and solder verification",INCOMPLETE,Actual Project Spark LevelGamePlayManager; NOT art scripts
02,Complete PCB routing and vias,"48 distinct required copper routes, ten physical vias and installed W049, no cross-net shorts",INCOMPLETE,Actual Project Spark LevelGamePlayManager; NOT art scripts
03,Calibrate master and divided clocks,"Measured 1024 Hz oscillator, 256 Hz (/4), 2 Hz (/512), valid actual power and timing tolerance",INCOMPLETE,Actual Project Spark LevelGamePlayManager; NOT art scripts
04,Commission precision PWM driver,"3 independent output tests 25%,50%,75% at nominal 256 Hz; actual load response and measured current",INCOMPLETE,Actual Project Spark LevelGamePlayManager; NOT art scripts
05,Recover three board faults,"Wrong C1, missing V06, solder bridge Q1; correct repair and independent fresh full-board production retest",INCOMPLETE,Actual Project Spark LevelGamePlayManager; NOT art scripts
06,Recover the critical W049 feedback,"Real W049 disconnection, fault and output inhibit, reconnection, fresh evidence, explicit reset and new test",INCOMPLETE,Actual Project Spark LevelGamePlayManager; NOT art scripts
```

## ANNEX K — THREE FAULT SCENARIOS

**Source path:** `Data/Three_Faults_And_Recovery.csv`

```csv
FaultID,Name,Affected,Injection,RealEffect,Recovery,Proof
F01,Timing capacitor misvalue,C1,10 nF intended -> 22 nF faulty,"Master and divided frequencies fall, frequency measurement invalid",Replace C1 with correct value and calibrate trimmer,New scope captures measured frequencies
F02,Missing divided-clock via,V06 / W028,Remove physical via continuity,Clock conditioning fails between U1 and U3; output timing unavailable,Rebuild correct via under clearance rules,Continuity and two valid divided clock captures
F03,PWM stage solder bridge,Q1 Gate-Drain,Cross-net solder bridge causes non-commanded output,Output monitor fault and abnormal load current,"De-energize, remove short, retest current and PWM",W049 trusted monitor with actual safe load behavior
```

## ANNEX L — 72 SOURCE ACCEPTANCE TEST DEFINITIONS

**Source path:** `Data/Engineering_Acceptance_Tests_72.csv`

```csv
TestID,Group,AcceptanceRequirement,Required,TestState,MustUse
L95-T001,Art & Materials,Asset + UV + material assignment for object group 1,YES,NOT_RUN,Unity actual project tests and real gameplay authority
L95-T002,Art & Materials,Asset + UV + material assignment for object group 2,YES,NOT_RUN,Unity actual project tests and real gameplay authority
L95-T003,Art & Materials,Asset + UV + material assignment for object group 3,YES,NOT_RUN,Unity actual project tests and real gameplay authority
L95-T004,Art & Materials,Asset + UV + material assignment for object group 4,YES,NOT_RUN,Unity actual project tests and real gameplay authority
L95-T005,Art & Materials,Asset + UV + material assignment for object group 5,YES,NOT_RUN,Unity actual project tests and real gameplay authority
L95-T006,Art & Materials,Asset + UV + material assignment for object group 6,YES,NOT_RUN,Unity actual project tests and real gameplay authority
L95-T007,Art & Materials,Asset + UV + material assignment for object group 7,YES,NOT_RUN,Unity actual project tests and real gameplay authority
L95-T008,Art & Materials,Asset + UV + material assignment for object group 8,YES,NOT_RUN,Unity actual project tests and real gameplay authority
L95-T009,Art & Materials,Asset + UV + material assignment for object group 9,YES,NOT_RUN,Unity actual project tests and real gameplay authority
L95-T010,Art & Materials,Asset + UV + material assignment for object group 10,YES,NOT_RUN,Unity actual project tests and real gameplay authority
L95-T011,Art & Materials,Asset + UV + material assignment for object group 11,YES,NOT_RUN,Unity actual project tests and real gameplay authority
L95-T012,Art & Materials,Asset + UV + material assignment for object group 12,YES,NOT_RUN,Unity actual project tests and real gameplay authority
L95-T013,PCB Assembly,"Real component placement, polarity, pad contact, no ghost pre-install for case 13",YES,NOT_RUN,Unity actual project tests and real gameplay authority
L95-T014,PCB Assembly,"Real component placement, polarity, pad contact, no ghost pre-install for case 14",YES,NOT_RUN,Unity actual project tests and real gameplay authority
L95-T015,PCB Assembly,"Real component placement, polarity, pad contact, no ghost pre-install for case 15",YES,NOT_RUN,Unity actual project tests and real gameplay authority
L95-T016,PCB Assembly,"Real component placement, polarity, pad contact, no ghost pre-install for case 16",YES,NOT_RUN,Unity actual project tests and real gameplay authority
L95-T017,PCB Assembly,"Real component placement, polarity, pad contact, no ghost pre-install for case 17",YES,NOT_RUN,Unity actual project tests and real gameplay authority
L95-T018,PCB Assembly,"Real component placement, polarity, pad contact, no ghost pre-install for case 18",YES,NOT_RUN,Unity actual project tests and real gameplay authority
L95-T019,PCB Assembly,"Real component placement, polarity, pad contact, no ghost pre-install for case 19",YES,NOT_RUN,Unity actual project tests and real gameplay authority
L95-T020,PCB Assembly,"Real component placement, polarity, pad contact, no ghost pre-install for case 20",YES,NOT_RUN,Unity actual project tests and real gameplay authority
L95-T021,PCB Assembly,"Real component placement, polarity, pad contact, no ghost pre-install for case 21",YES,NOT_RUN,Unity actual project tests and real gameplay authority
L95-T022,PCB Assembly,"Real component placement, polarity, pad contact, no ghost pre-install for case 22",YES,NOT_RUN,Unity actual project tests and real gameplay authority
L95-T023,PCB Assembly,"Real component placement, polarity, pad contact, no ghost pre-install for case 23",YES,NOT_RUN,Unity actual project tests and real gameplay authority
L95-T024,PCB Assembly,"Real component placement, polarity, pad contact, no ghost pre-install for case 24",YES,NOT_RUN,Unity actual project tests and real gameplay authority
L95-T025,Routing/DRC,"Real trace topology, via, clearance and shorted-net rejection case 25",YES,NOT_RUN,Unity actual project tests and real gameplay authority
L95-T026,Routing/DRC,"Real trace topology, via, clearance and shorted-net rejection case 26",YES,NOT_RUN,Unity actual project tests and real gameplay authority
L95-T027,Routing/DRC,"Real trace topology, via, clearance and shorted-net rejection case 27",YES,NOT_RUN,Unity actual project tests and real gameplay authority
L95-T028,Routing/DRC,"Real trace topology, via, clearance and shorted-net rejection case 28",YES,NOT_RUN,Unity actual project tests and real gameplay authority
L95-T029,Routing/DRC,"Real trace topology, via, clearance and shorted-net rejection case 29",YES,NOT_RUN,Unity actual project tests and real gameplay authority
L95-T030,Routing/DRC,"Real trace topology, via, clearance and shorted-net rejection case 30",YES,NOT_RUN,Unity actual project tests and real gameplay authority
L95-T031,Routing/DRC,"Real trace topology, via, clearance and shorted-net rejection case 31",YES,NOT_RUN,Unity actual project tests and real gameplay authority
L95-T032,Routing/DRC,"Real trace topology, via, clearance and shorted-net rejection case 32",YES,NOT_RUN,Unity actual project tests and real gameplay authority
L95-T033,Routing/DRC,"Real trace topology, via, clearance and shorted-net rejection case 33",YES,NOT_RUN,Unity actual project tests and real gameplay authority
L95-T034,Routing/DRC,"Real trace topology, via, clearance and shorted-net rejection case 34",YES,NOT_RUN,Unity actual project tests and real gameplay authority
L95-T035,Routing/DRC,"Real trace topology, via, clearance and shorted-net rejection case 35",YES,NOT_RUN,Unity actual project tests and real gameplay authority
L95-T036,Routing/DRC,"Real trace topology, via, clearance and shorted-net rejection case 36",YES,NOT_RUN,Unity actual project tests and real gameplay authority
L95-T037,Routing/DRC,"Real trace topology, via, clearance and shorted-net rejection case 37",YES,NOT_RUN,Unity actual project tests and real gameplay authority
L95-T038,Routing/DRC,"Real trace topology, via, clearance and shorted-net rejection case 38",YES,NOT_RUN,Unity actual project tests and real gameplay authority
L95-T039,Routing/DRC,"Real trace topology, via, clearance and shorted-net rejection case 39",YES,NOT_RUN,Unity actual project tests and real gameplay authority
L95-T040,Routing/DRC,"Real trace topology, via, clearance and shorted-net rejection case 40",YES,NOT_RUN,Unity actual project tests and real gameplay authority
L95-T041,Clock/PWM,"Real 1024/256/2 Hz and PWM waveform, power and load measurement case 41",YES,NOT_RUN,Unity actual project tests and real gameplay authority
L95-T042,Clock/PWM,"Real 1024/256/2 Hz and PWM waveform, power and load measurement case 42",YES,NOT_RUN,Unity actual project tests and real gameplay authority
L95-T043,Clock/PWM,"Real 1024/256/2 Hz and PWM waveform, power and load measurement case 43",YES,NOT_RUN,Unity actual project tests and real gameplay authority
L95-T044,Clock/PWM,"Real 1024/256/2 Hz and PWM waveform, power and load measurement case 44",YES,NOT_RUN,Unity actual project tests and real gameplay authority
L95-T045,Clock/PWM,"Real 1024/256/2 Hz and PWM waveform, power and load measurement case 45",YES,NOT_RUN,Unity actual project tests and real gameplay authority
L95-T046,Clock/PWM,"Real 1024/256/2 Hz and PWM waveform, power and load measurement case 46",YES,NOT_RUN,Unity actual project tests and real gameplay authority
L95-T047,Clock/PWM,"Real 1024/256/2 Hz and PWM waveform, power and load measurement case 47",YES,NOT_RUN,Unity actual project tests and real gameplay authority
L95-T048,Clock/PWM,"Real 1024/256/2 Hz and PWM waveform, power and load measurement case 48",YES,NOT_RUN,Unity actual project tests and real gameplay authority
L95-T049,Clock/PWM,"Real 1024/256/2 Hz and PWM waveform, power and load measurement case 49",YES,NOT_RUN,Unity actual project tests and real gameplay authority
L95-T050,Clock/PWM,"Real 1024/256/2 Hz and PWM waveform, power and load measurement case 50",YES,NOT_RUN,Unity actual project tests and real gameplay authority
L95-T051,Clock/PWM,"Real 1024/256/2 Hz and PWM waveform, power and load measurement case 51",YES,NOT_RUN,Unity actual project tests and real gameplay authority
L95-T052,Fault injection,"Real C1/V06/Q1 change, fault detection and repair evidence case 52",YES,NOT_RUN,Unity actual project tests and real gameplay authority
L95-T053,Fault injection,"Real C1/V06/Q1 change, fault detection and repair evidence case 53",YES,NOT_RUN,Unity actual project tests and real gameplay authority
L95-T054,Fault injection,"Real C1/V06/Q1 change, fault detection and repair evidence case 54",YES,NOT_RUN,Unity actual project tests and real gameplay authority
L95-T055,Fault injection,"Real C1/V06/Q1 change, fault detection and repair evidence case 55",YES,NOT_RUN,Unity actual project tests and real gameplay authority
L95-T056,Fault injection,"Real C1/V06/Q1 change, fault detection and repair evidence case 56",YES,NOT_RUN,Unity actual project tests and real gameplay authority
L95-T057,Fault injection,"Real C1/V06/Q1 change, fault detection and repair evidence case 57",YES,NOT_RUN,Unity actual project tests and real gameplay authority
L95-T058,Fault injection,"Real C1/V06/Q1 change, fault detection and repair evidence case 58",YES,NOT_RUN,Unity actual project tests and real gameplay authority
L95-T059,Fault injection,"Real C1/V06/Q1 change, fault detection and repair evidence case 59",YES,NOT_RUN,Unity actual project tests and real gameplay authority
L95-T060,Fault injection,"Real C1/V06/Q1 change, fault detection and repair evidence case 60",YES,NOT_RUN,Unity actual project tests and real gameplay authority
L95-T061,W049 feedback,"Physical W049 removal, open feedback, safe inhibit, recalibrate, reset case 61",YES,NOT_RUN,Unity actual project tests and real gameplay authority
L95-T062,W049 feedback,"Physical W049 removal, open feedback, safe inhibit, recalibrate, reset case 62",YES,NOT_RUN,Unity actual project tests and real gameplay authority
L95-T063,W049 feedback,"Physical W049 removal, open feedback, safe inhibit, recalibrate, reset case 63",YES,NOT_RUN,Unity actual project tests and real gameplay authority
L95-T064,W049 feedback,"Physical W049 removal, open feedback, safe inhibit, recalibrate, reset case 64",YES,NOT_RUN,Unity actual project tests and real gameplay authority
L95-T065,W049 feedback,"Physical W049 removal, open feedback, safe inhibit, recalibrate, reset case 65",YES,NOT_RUN,Unity actual project tests and real gameplay authority
L95-T066,W049 feedback,"Physical W049 removal, open feedback, safe inhibit, recalibrate, reset case 66",YES,NOT_RUN,Unity actual project tests and real gameplay authority
L95-T067,W049 feedback,"Physical W049 removal, open feedback, safe inhibit, recalibrate, reset case 67",YES,NOT_RUN,Unity actual project tests and real gameplay authority
L95-T068,Unity Regression,"Builder rerun, compile, preserve levels 1-94 and 6/6 case 68",YES,NOT_RUN,Unity actual project tests and real gameplay authority
L95-T069,Unity Regression,"Builder rerun, compile, preserve levels 1-94 and 6/6 case 69",YES,NOT_RUN,Unity actual project tests and real gameplay authority
L95-T070,Unity Regression,"Builder rerun, compile, preserve levels 1-94 and 6/6 case 70",YES,NOT_RUN,Unity actual project tests and real gameplay authority
L95-T071,Unity Regression,"Builder rerun, compile, preserve levels 1-94 and 6/6 case 71",YES,NOT_RUN,Unity actual project tests and real gameplay authority
L95-T072,Unity Regression,"Builder rerun, compile, preserve levels 1-94 and 6/6 case 72",YES,NOT_RUN,Unity actual project tests and real gameplay authority
```

## ANNEX M — ANIMATION PIVOTS — 48 EXACT POSITIONS AND AXES

**Source path:** `Data/Animation_Pivots.csv`

```csv
Model,Pivot,X,Y,Z,Axis,Movement,Description
L95_U1_TLC555CMOSTimer_SOIC8,ANIM_U1_INSTALL,-0.348,1.259,0.012,y,placement,U1 INSTALL
L95_U2_74HC14_Schmitt_SOIC14,ANIM_U2_INSTALL,-0.084,1.259,0.06,y,placement,U2 INSTALL
L95_U3_CD4040B_BinaryCounter_SOIC16,ANIM_U3_INSTALL,0.216,1.259,0.096,y,placement,U3 INSTALL
L95_U4_LM393_PWMComparator_SOIC8,ANIM_U4_INSTALL,0.48,1.259,-0.168,y,placement,U4 INSTALL
L95_U5_3V3LDO_SOT223,ANIM_U5_INSTALL,-0.516,1.259,0.228,y,placement,U5 INSTALL
L95_U6_74HC08_LogicGate_SOIC14,ANIM_U6_INSTALL,0.132,1.259,-0.228,y,placement,U6 INSTALL
L95_U7_TimingSafetySupervisor_SOIC8,ANIM_U7_INSTALL,-0.252,1.259,-0.276,y,placement,U7 INSTALL
L95_R1_RCCharge_14K7,ANIM_R1_INSTALL,-0.564,1.259,0.12,y,placement,R1 INSTALL
L95_R2_RCTiming_63K,ANIM_R2_INSTALL,-0.504,1.259,0.0,y,placement,R2 INSTALL
L95_R3_DividerBias_10K,ANIM_R3_INSTALL,-0.384,1.259,0.252,y,placement,R3 INSTALL
L95_R4_RampCharge_4K7,ANIM_R4_INSTALL,0.408,1.259,0.156,y,placement,R4 INSTALL
L95_R5_ClockLED_1K,ANIM_R5_INSTALL,-0.336,1.259,-0.408,y,placement,R5 INSTALL
L95_R6_StatusLED_330R,ANIM_R6_INSTALL,0.468,1.259,-0.324,y,placement,R6 INSTALL
L95_R7_MOSFETGate_100R,ANIM_R7_INSTALL,0.348,1.259,-0.3,y,placement,R7 INSTALL
L95_R8_GatePulldown_100K,ANIM_R8_INSTALL,0.504,1.259,-0.396,y,placement,R8 INSTALL
L95_R9_CurrentShunt_0R1,ANIM_R9_INSTALL,0.264,1.259,-0.372,y,placement,R9 INSTALL
L95_R10_PWMPullup_4K7,ANIM_R10_INSTALL,0.6,1.259,0.048,y,placement,R10 INSTALL
L95_R11_SenseFilter_1K,ANIM_R11_INSTALL,0.6,1.259,-0.036,y,placement,R11 INSTALL
L95_R12_EnableBias_10K,ANIM_R12_INSTALL,-0.084,1.259,-0.372,y,placement,R12 INSTALL
L95_C1_RCTiming_10nF,ANIM_C1_INSTALL,-0.6,1.259,0.216,y,placement,C1 INSTALL
L95_C2_ControlFilter_10nF,ANIM_C2_INSTALL,-0.288,1.259,0.156,y,placement,C2 INSTALL
L95_C3_PowerInput_10uF,ANIM_C3_INSTALL,-0.516,1.259,0.372,y,placement,C3 INSTALL
L95_C4_LDOOutput_22uF,ANIM_C4_INSTALL,-0.36,1.259,0.384,y,placement,C4 INSTALL
L95_C5_TimerDecoupling_100nF,ANIM_C5_INSTALL,-0.312,1.259,-0.12,y,placement,C5 INSTALL
L95_C6_CounterDecoupling_100nF,ANIM_C6_INSTALL,0.24,1.259,0.288,y,placement,C6 INSTALL
L95_C7_RampFilter_1nF,ANIM_C7_INSTALL,0.408,1.259,-0.036,y,placement,C7 INSTALL
L95_C8_PWMFilter_100nF,ANIM_C8_INSTALL,0.612,1.259,-0.3,y,placement,C8 INSTALL
L95_C9_SafetyDecoupling_100nF,ANIM_C9_INSTALL,-0.252,1.259,-0.396,y,placement,C9 INSTALL
L95_LED1_ClockStatusGreen,ANIM_LED1_INSTALL,-0.552,1.259,-0.336,y,placement,LED1 INSTALL
L95_LED2_OutputStatusAmber,ANIM_LED2_INSTALL,0.528,1.259,0.348,y,placement,LED2 INSTALL
L95_Q1_OutputMOSFET_SOT23,ANIM_Q1_INSTALL,0.348,1.259,-0.408,y,placement,Q1 INSTALL
L95_D1_InputProtectionSMA,ANIM_D1_INSTALL,-0.624,1.259,0.324,y,placement,D1 INSTALL
L95_RV1_PWMDutyTrimPotentiometer,ANIM_RV1_INSTALL,-0.036,1.259,0.348,y,placement,RV1 INSTALL
L95_W049_DetachableMonitorJumper,ANIM_W049_INSTALL,0.564,1.259,-0.24,y,placement,W049 INSTALL
L95_J1_Protected5VTerminal,ANIM_J1_INSTALL,-0.648,1.259,-0.012,y,placement,J1 INSTALL
L95_J2_PWMAndClockOutputHeader,ANIM_J2_INSTALL,0.636,1.259,0.264,y,placement,J2 INSTALL
L95_ClampingJawLeft,ANIM_LEFT_CLAMP_RELEASE,-0.86,1.24,0,x,translation,LEFT CLAMP RELEASE
L95_ClampingJawRight,ANIM_RIGHT_CLAMP_RELEASE,0.86,1.24,0,x,translation,RIGHT CLAMP RELEASE
L95_InspectionMicroscopeGantry,ANIM_MICROSCOPE_ARM_ROTATE,0.39,2.15,-0.51,y,rotation,MICROSCOPE ARM ROTATE
L95_MicroscopeFocusLens,ANIM_MICROSCOPE_FOCUS,0.39,1.95,-0.51,y,translation,MICROSCOPE FOCUS
L95_FourChannelOscilloscope,ANIM_SCOPE_CHANNEL_KNOB,-1.83,1.51,0.28,z,rotation,SCOPE CHANNEL KNOB
L95_Protected5VBenchSupply,ANIM_SUPPLY_SETPOINT,-1.92,1.29,0.72,z,rotation,SUPPLY SETPOINT
L95_PrecisionDigitalMultimeter,ANIM_METER_DIAL,1.75,1.52,0.32,z,rotation,METER DIAL
L95_DigitalLogicAnalyzer,ANIM_ANALYZER_PROBE,1.8,1.4,0.75,z,translation,ANALYZER PROBE
L95_Q1_OutputMOSFET_SOT23,ANIM_Q1_HEATSINK,0.348,1.259,-0.408,y,rotation,Q1 HEATSINK
L95_RV1_PWMDutyTrimPotentiometer,ANIM_RV1_DUTY_TRIM_SCREW,-0.036,1.259,0.348,y,rotation,RV1 DUTY TRIM SCREW
L95_W049_DetachableMonitorJumper,ANIM_W049_REMOVAL,0.564,1.259,-0.24,y,rotation,W049 REMOVAL
L95_U4_LM393_PWMComparator_SOIC8,ANIM_U4_PWM_CALIBRATION,0.48,1.259,-0.168,y,rotation,U4 PWM CALIBRATION
```

## ANNEX N — COLLISION AND LOD PRODUCTION PLAN — 54 ENTRIES

**Source path:** `Data/Collision_LOD_Production_Plan.csv`

```csv
Model,Group,CollisionRecommendation,LOD0,LOD1,LOD2,Static,UVReviewNeeded,BakedNormalsNeeded,UnityPlayModeVerified
L95_PrecisionWorkBenchFrame,BenchAndFixtures,Simple machine collision proxy,Original mesh,Plan: 40-65% triangles,Plan: billboard or hide < 4px,True,YES,FINAL PRODUCTION,NO
L95_ESDAntiStaticWorktop,BenchAndFixtures,Simple machine collision proxy,Original mesh,Plan: 40-65% triangles,Plan: billboard or hide < 4px,True,YES,FINAL PRODUCTION,NO
L95_TimingPCBFixtureBase,BenchAndFixtures,Simple machine collision proxy,Original mesh,Plan: 40-65% triangles,Plan: billboard or hide < 4px,True,YES,FINAL PRODUCTION,NO
L95_ClampingJawLeft,BenchAndFixtures,Simple machine collision proxy,Original mesh,Plan: 40-65% triangles,Plan: billboard or hide < 4px,True,YES,FINAL PRODUCTION,NO
L95_ClampingJawRight,BenchAndFixtures,Simple machine collision proxy,Original mesh,Plan: 40-65% triangles,Plan: billboard or hide < 4px,True,YES,FINAL PRODUCTION,NO
L95_InspectionMicroscopeGantry,BenchAndFixtures,Simple machine collision proxy,Original mesh,Plan: 40-65% triangles,Plan: billboard or hide < 4px,True,YES,FINAL PRODUCTION,NO
L95_MicroscopeFocusLens,BenchAndFixtures,Simple machine collision proxy,Original mesh,Plan: 40-65% triangles,Plan: billboard or hide < 4px,True,YES,FINAL PRODUCTION,NO
L95_PartsAndReworkTray,BenchAndFixtures,Simple machine collision proxy,Original mesh,Plan: 40-65% triangles,Plan: billboard or hide < 4px,True,YES,FINAL PRODUCTION,NO
L95_FR4PrecisionBoardCore,BarePCBAndPads,Static box proxy or skip; pads are decorative,Original mesh,Plan: 40-65% triangles,Plan: billboard or hide < 4px,True,YES,FINAL PRODUCTION,NO
L95_TopNavySoldermask,BarePCBAndPads,Static box proxy or skip; pads are decorative,Original mesh,Plan: 40-65% triangles,Plan: billboard or hide < 4px,True,YES,FINAL PRODUCTION,NO
L95_BottomNavySoldermask,BarePCBAndPads,Static box proxy or skip; pads are decorative,Original mesh,Plan: 40-65% triangles,Plan: billboard or hide < 4px,True,YES,FINAL PRODUCTION,NO
L95_TopGoldSMDPads,BarePCBAndPads,Static box proxy or skip; pads are decorative,Original mesh,Plan: 40-65% triangles,Plan: billboard or hide < 4px,True,YES,FINAL PRODUCTION,NO
L95_BottomCopperPadTargets,BarePCBAndPads,Static box proxy or skip; pads are decorative,Original mesh,Plan: 40-65% triangles,Plan: billboard or hide < 4px,True,YES,FINAL PRODUCTION,NO
L95_SilkscreenMountsAndTestFiducials,BarePCBAndPads,Static box proxy or skip; pads are decorative,Original mesh,Plan: 40-65% triangles,Plan: billboard or hide < 4px,True,YES,FINAL PRODUCTION,NO
L95_FourChannelOscilloscope,TestInstruments,Simple instrument case proxy,Original mesh,Plan: 40-65% triangles,Plan: billboard or hide < 4px,False,YES,FINAL PRODUCTION,NO
L95_Protected5VBenchSupply,TestInstruments,Simple instrument case proxy,Original mesh,Plan: 40-65% triangles,Plan: billboard or hide < 4px,False,YES,FINAL PRODUCTION,NO
L95_PrecisionDigitalMultimeter,TestInstruments,Simple instrument case proxy,Original mesh,Plan: 40-65% triangles,Plan: billboard or hide < 4px,False,YES,FINAL PRODUCTION,NO
L95_DigitalLogicAnalyzer,TestInstruments,Simple instrument case proxy,Original mesh,Plan: 40-65% triangles,Plan: billboard or hide < 4px,False,YES,FINAL PRODUCTION,NO
L95_U1_TLC555CMOSTimer_SOIC8,PlayerInstallableComponents,No physics collider for fine SMD; use snap-footprint hitbox,Original mesh,Plan: 40-65% triangles,Plan: billboard or hide < 4px,False,YES,FINAL PRODUCTION,NO
L95_U2_74HC14_Schmitt_SOIC14,PlayerInstallableComponents,No physics collider for fine SMD; use snap-footprint hitbox,Original mesh,Plan: 40-65% triangles,Plan: billboard or hide < 4px,False,YES,FINAL PRODUCTION,NO
L95_U3_CD4040B_BinaryCounter_SOIC16,PlayerInstallableComponents,No physics collider for fine SMD; use snap-footprint hitbox,Original mesh,Plan: 40-65% triangles,Plan: billboard or hide < 4px,False,YES,FINAL PRODUCTION,NO
L95_U4_LM393_PWMComparator_SOIC8,PlayerInstallableComponents,No physics collider for fine SMD; use snap-footprint hitbox,Original mesh,Plan: 40-65% triangles,Plan: billboard or hide < 4px,False,YES,FINAL PRODUCTION,NO
L95_U5_3V3LDO_SOT223,PlayerInstallableComponents,No physics collider for fine SMD; use snap-footprint hitbox,Original mesh,Plan: 40-65% triangles,Plan: billboard or hide < 4px,False,YES,FINAL PRODUCTION,NO
L95_U6_74HC08_LogicGate_SOIC14,PlayerInstallableComponents,No physics collider for fine SMD; use snap-footprint hitbox,Original mesh,Plan: 40-65% triangles,Plan: billboard or hide < 4px,False,YES,FINAL PRODUCTION,NO
L95_U7_TimingSafetySupervisor_SOIC8,PlayerInstallableComponents,No physics collider for fine SMD; use snap-footprint hitbox,Original mesh,Plan: 40-65% triangles,Plan: billboard or hide < 4px,False,YES,FINAL PRODUCTION,NO
L95_R1_RCCharge_14K7,PlayerInstallableComponents,No physics collider for fine SMD; use snap-footprint hitbox,Original mesh,Plan: 40-65% triangles,Plan: billboard or hide < 4px,False,YES,FINAL PRODUCTION,NO
L95_R2_RCTiming_63K,PlayerInstallableComponents,No physics collider for fine SMD; use snap-footprint hitbox,Original mesh,Plan: 40-65% triangles,Plan: billboard or hide < 4px,False,YES,FINAL PRODUCTION,NO
L95_R3_DividerBias_10K,PlayerInstallableComponents,No physics collider for fine SMD; use snap-footprint hitbox,Original mesh,Plan: 40-65% triangles,Plan: billboard or hide < 4px,False,YES,FINAL PRODUCTION,NO
L95_R4_RampCharge_4K7,PlayerInstallableComponents,No physics collider for fine SMD; use snap-footprint hitbox,Original mesh,Plan: 40-65% triangles,Plan: billboard or hide < 4px,False,YES,FINAL PRODUCTION,NO
L95_R5_ClockLED_1K,PlayerInstallableComponents,No physics collider for fine SMD; use snap-footprint hitbox,Original mesh,Plan: 40-65% triangles,Plan: billboard or hide < 4px,False,YES,FINAL PRODUCTION,NO
L95_R6_StatusLED_330R,PlayerInstallableComponents,No physics collider for fine SMD; use snap-footprint hitbox,Original mesh,Plan: 40-65% triangles,Plan: billboard or hide < 4px,False,YES,FINAL PRODUCTION,NO
L95_R7_MOSFETGate_100R,PlayerInstallableComponents,No physics collider for fine SMD; use snap-footprint hitbox,Original mesh,Plan: 40-65% triangles,Plan: billboard or hide < 4px,False,YES,FINAL PRODUCTION,NO
L95_R8_GatePulldown_100K,PlayerInstallableComponents,No physics collider for fine SMD; use snap-footprint hitbox,Original mesh,Plan: 40-65% triangles,Plan: billboard or hide < 4px,False,YES,FINAL PRODUCTION,NO
L95_R9_CurrentShunt_0R1,PlayerInstallableComponents,No physics collider for fine SMD; use snap-footprint hitbox,Original mesh,Plan: 40-65% triangles,Plan: billboard or hide < 4px,False,YES,FINAL PRODUCTION,NO
L95_R10_PWMPullup_4K7,PlayerInstallableComponents,No physics collider for fine SMD; use snap-footprint hitbox,Original mesh,Plan: 40-65% triangles,Plan: billboard or hide < 4px,False,YES,FINAL PRODUCTION,NO
L95_R11_SenseFilter_1K,PlayerInstallableComponents,No physics collider for fine SMD; use snap-footprint hitbox,Original mesh,Plan: 40-65% triangles,Plan: billboard or hide < 4px,False,YES,FINAL PRODUCTION,NO
L95_R12_EnableBias_10K,PlayerInstallableComponents,No physics collider for fine SMD; use snap-footprint hitbox,Original mesh,Plan: 40-65% triangles,Plan: billboard or hide < 4px,False,YES,FINAL PRODUCTION,NO
L95_C1_RCTiming_10nF,PlayerInstallableComponents,No physics collider for fine SMD; use snap-footprint hitbox,Original mesh,Plan: 40-65% triangles,Plan: billboard or hide < 4px,False,YES,FINAL PRODUCTION,NO
L95_C2_ControlFilter_10nF,PlayerInstallableComponents,No physics collider for fine SMD; use snap-footprint hitbox,Original mesh,Plan: 40-65% triangles,Plan: billboard or hide < 4px,False,YES,FINAL PRODUCTION,NO
L95_C3_PowerInput_10uF,PlayerInstallableComponents,No physics collider for fine SMD; use snap-footprint hitbox,Original mesh,Plan: 40-65% triangles,Plan: billboard or hide < 4px,False,YES,FINAL PRODUCTION,NO
L95_C4_LDOOutput_22uF,PlayerInstallableComponents,No physics collider for fine SMD; use snap-footprint hitbox,Original mesh,Plan: 40-65% triangles,Plan: billboard or hide < 4px,False,YES,FINAL PRODUCTION,NO
L95_C5_TimerDecoupling_100nF,PlayerInstallableComponents,No physics collider for fine SMD; use snap-footprint hitbox,Original mesh,Plan: 40-65% triangles,Plan: billboard or hide < 4px,False,YES,FINAL PRODUCTION,NO
L95_C6_CounterDecoupling_100nF,PlayerInstallableComponents,No physics collider for fine SMD; use snap-footprint hitbox,Original mesh,Plan: 40-65% triangles,Plan: billboard or hide < 4px,False,YES,FINAL PRODUCTION,NO
L95_C7_RampFilter_1nF,PlayerInstallableComponents,No physics collider for fine SMD; use snap-footprint hitbox,Original mesh,Plan: 40-65% triangles,Plan: billboard or hide < 4px,False,YES,FINAL PRODUCTION,NO
L95_C8_PWMFilter_100nF,PlayerInstallableComponents,No physics collider for fine SMD; use snap-footprint hitbox,Original mesh,Plan: 40-65% triangles,Plan: billboard or hide < 4px,False,YES,FINAL PRODUCTION,NO
L95_C9_SafetyDecoupling_100nF,PlayerInstallableComponents,No physics collider for fine SMD; use snap-footprint hitbox,Original mesh,Plan: 40-65% triangles,Plan: billboard or hide < 4px,False,YES,FINAL PRODUCTION,NO
L95_LED1_ClockStatusGreen,PlayerInstallableComponents,No physics collider for fine SMD; use snap-footprint hitbox,Original mesh,Plan: 40-65% triangles,Plan: billboard or hide < 4px,False,YES,FINAL PRODUCTION,NO
L95_LED2_OutputStatusAmber,PlayerInstallableComponents,No physics collider for fine SMD; use snap-footprint hitbox,Original mesh,Plan: 40-65% triangles,Plan: billboard or hide < 4px,False,YES,FINAL PRODUCTION,NO
L95_Q1_OutputMOSFET_SOT23,PlayerInstallableComponents,No physics collider for fine SMD; use snap-footprint hitbox,Original mesh,Plan: 40-65% triangles,Plan: billboard or hide < 4px,False,YES,FINAL PRODUCTION,NO
L95_D1_InputProtectionSMA,PlayerInstallableComponents,No physics collider for fine SMD; use snap-footprint hitbox,Original mesh,Plan: 40-65% triangles,Plan: billboard or hide < 4px,False,YES,FINAL PRODUCTION,NO
L95_RV1_PWMDutyTrimPotentiometer,PlayerInstallableComponents,No physics collider for fine SMD; use snap-footprint hitbox,Original mesh,Plan: 40-65% triangles,Plan: billboard or hide < 4px,False,YES,FINAL PRODUCTION,NO
L95_W049_DetachableMonitorJumper,PlayerInstallableComponents,No physics collider for fine SMD; use snap-footprint hitbox,Original mesh,Plan: 40-65% triangles,Plan: billboard or hide < 4px,False,YES,FINAL PRODUCTION,NO
L95_J1_Protected5VTerminal,PlayerInstallableComponents,No physics collider for fine SMD; use snap-footprint hitbox,Original mesh,Plan: 40-65% triangles,Plan: billboard or hide < 4px,False,YES,FINAL PRODUCTION,NO
L95_J2_PWMAndClockOutputHeader,PlayerInstallableComponents,No physics collider for fine SMD; use snap-footprint hitbox,Original mesh,Plan: 40-65% triangles,Plan: billboard or hide < 4px,False,YES,FINAL PRODUCTION,NO
```

## ANNEX O — PHYSICAL BOARD DESIGN JSON

**Source path:** `Data/PCB_Physical_Design.json`

```json
{
  "Level": 95,
  "BoardName": "Precision Timing & PWM Controller",
  "WidthMM": 120,
  "HeightMM": 80,
  "ThicknessMM": 1.6,
  "BoardLayers": [
    "F.Cu",
    "B.Cu"
  ],
  "InputDCV": 5.0,
  "MonitorRailV": 3.3,
  "MasterClockHz": 1024,
  "DividedClockHz": [
    256,
    2
  ],
  "PWMOutputHz": 256,
  "ValidPWMDutyPercents": [
    25,
    50,
    75
  ],
  "RequiredComponentPlacements": 36,
  "RequiredRouteTasks": 48,
  "RequiredVias": 10,
  "CriticalServiceJumper": "W049",
  "VisualScaleMultiplier": 12,
  "GameStartComponentsInstalled": 0,
  "GameStartRoutesCompleted": 0,
  "GameStartViasCompleted": 0,
  "GameStartCriticalJumperConnected": false,
  "CircuitIsEducational": "YES: role aliases are not physical manufacturer datasheet pin numbers",
  "AllRequiredTestTolerancesRequireGameplayConfig": true
}
```

## ANNEX P — CIRCUIT GRAPH CONTRACT JSON

**Source path:** `Data/Circuit_Graph_Contract.json`

```json
{
  "player_wires": 48,
  "copper_layer_count": 2,
  "player_vias": 10,
  "required_components": 36,
  "critical_jumper": "W049",
  "pad_count": 150,
  "live_topology_must_interact_with": "Existing SparkElectricalSolver, SparkTerminal and SparkCircuitSystem APIs - inspect project first",
  "sample_validity_fields": [
    "measuredValue",
    "timestamp",
    "sensorState",
    "calibrationRevision",
    "networkRevision"
  ],
  "safe_state": "PWM output disabled; oscillator diagnostics continue",
  "projected_scripts_only": true
}
```

## ANNEX Q — PBR MATERIAL BINDINGS JSON

**Source path:** `Materials/PBR_Material_Bindings.json`

```json
{
  "source_material_map": {
    "frame": "TitaniumBench",
    "steel": "TitaniumBench",
    "aluminum": "TitaniumBench",
    "graphite": "AnodizedGraphite",
    "rubber": "ESDSafeRubber",
    "fr4": "FiberglassFR4",
    "mask": "DeepNavyPCBMask",
    "maskdark": "DeepNavyPCBMask",
    "copper": "RolledCopper",
    "gold": "ImmersionGoldENIG",
    "solder": "LeadFreeSolder",
    "ceramic": "TechnicalWhiteCeramic",
    "whiteink": "LaserSilkscreenWhite",
    "resistor": "CarbonFilmResistor",
    "capacitor": "MultiLayerCeramicCapacitor",
    "ic": "ElectronicBlackMold",
    "black": "ElectronicBlackMold",
    "glass": "PrecisionCoatedGlass",
    "panel": "BlueBlackScreenGlass",
    "cyan": "InspectionCyanLED",
    "green": "StatusGreenLED",
    "amber": "StatusAmberLED",
    "red": "StatusRedLED",
    "blue": "StatusBlueLED",
    "violet": "CalibrationVioletLED",
    "holo": "InspectionCyanLED"
  },
  "material_sets": [
    {
      "MaterialSet": "TitaniumBench",
      "AlbedoRGB": "#7D8B9A",
      "Metallic": 0.83,
      "Roughness": 0.3,
      "Pattern": "brushed",
      "Resolution": 1024,
      "TextureCount": 6,
      "UsedBy": "frame;steel;aluminum"
    },
    {
      "MaterialSet": "AnodizedGraphite",
      "AlbedoRGB": "#1E2839",
      "Metallic": 0.64,
      "Roughness": 0.43,
      "Pattern": "brushed",
      "Resolution": 1024,
      "TextureCount": 6,
      "UsedBy": "graphite"
    },
    {
      "MaterialSet": "ESDSafeRubber",
      "AlbedoRGB": "#161D26",
      "Metallic": 0.02,
      "Roughness": 0.74,
      "Pattern": "rubber",
      "Resolution": 1024,
      "TextureCount": 6,
      "UsedBy": "rubber"
    },
    {
      "MaterialSet": "FiberglassFR4",
      "AlbedoRGB": "#5C693B",
      "Metallic": 0.03,
      "Roughness": 0.72,
      "Pattern": "fiberglass",
      "Resolution": 1024,
      "TextureCount": 6,
      "UsedBy": "fr4"
    },
    {
      "MaterialSet": "DeepNavyPCBMask",
      "AlbedoRGB": "#091F5C",
      "Metallic": 0.03,
      "Roughness": 0.28,
      "Pattern": "pcbmask",
      "Resolution": 1024,
      "TextureCount": 6,
      "UsedBy": "mask;maskdark"
    },
    {
      "MaterialSet": "RolledCopper",
      "AlbedoRGB": "#DA712B",
      "Metallic": 0.89,
      "Roughness": 0.34,
      "Pattern": "brushed",
      "Resolution": 1024,
      "TextureCount": 6,
      "UsedBy": "copper"
    },
    {
      "MaterialSet": "ImmersionGoldENIG",
      "AlbedoRGB": "#E8BE4E",
      "Metallic": 0.96,
      "Roughness": 0.21,
      "Pattern": "brushed",
      "Resolution": 1024,
      "TextureCount": 6,
      "UsedBy": "gold"
    },
    {
      "MaterialSet": "LeadFreeSolder",
      "AlbedoRGB": "#BCC9D0",
      "Metallic": 0.85,
      "Roughness": 0.26,
      "Pattern": "brushed",
      "Resolution": 1024,
      "TextureCount": 6,
      "UsedBy": "solder"
    },
    {
      "MaterialSet": "TechnicalWhiteCeramic",
      "AlbedoRGB": "#E6E7DC",
      "Metallic": 0.09,
      "Roughness": 0.56,
      "Pattern": "ceramic",
      "Resolution": 1024,
      "TextureCount": 6,
      "UsedBy": "ceramic"
    },
    {
      "MaterialSet": "LaserSilkscreenWhite",
      "AlbedoRGB": "#E7F0F1",
      "Metallic": 0,
      "Roughness": 0.78,
      "Pattern": "ink",
      "Resolution": 1024,
      "TextureCount": 6,
      "UsedBy": "whiteink"
    },
    {
      "MaterialSet": "CarbonFilmResistor",
      "AlbedoRGB": "#493125",
      "Metallic": 0.05,
      "Roughness": 0.6,
      "Pattern": "carbon",
      "Resolution": 1024,
      "TextureCount": 6,
      "UsedBy": "resistor"
    },
    {
      "MaterialSet": "MultiLayerCeramicCapacitor",
      "AlbedoRGB": "#A39159",
      "Metallic": 0.12,
      "Roughness": 0.42,
      "Pattern": "ceramic",
      "Resolution": 1024,
      "TextureCount": 6,
      "UsedBy": "capacitor"
    },
    {
      "MaterialSet": "ElectronicBlackMold",
      "AlbedoRGB": "#11141E",
      "Metallic": 0.05,
      "Roughness": 0.47,
      "Pattern": "carbon",
      "Resolution": 1024,
      "TextureCount": 6,
      "UsedBy": "ic;black"
    },
    {
      "MaterialSet": "PrecisionCoatedGlass",
      "AlbedoRGB": "#2F80AD",
      "Metallic": 0.03,
      "Roughness": 0.15,
      "Pattern": "glass",
      "Resolution": 1024,
      "TextureCount": 6,
      "UsedBy": "glass"
    },
    {
      "MaterialSet": "BlueBlackScreenGlass",
      "AlbedoRGB": "#071A37",
      "Metallic": 0.02,
      "Roughness": 0.29,
      "Pattern": "holo",
      "Resolution": 1024,
      "TextureCount": 6,
      "UsedBy": "panel"
    },
    {
      "MaterialSet": "InspectionCyanLED",
      "AlbedoRGB": "#12B7F4",
      "Metallic": 0,
      "Roughness": 0.2,
      "Pattern": "holo",
      "Resolution": 1024,
      "TextureCount": 6,
      "UsedBy": "cyan;holo"
    },
    {
      "MaterialSet": "StatusGreenLED",
      "AlbedoRGB": "#14E353",
      "Metallic": 0,
      "Roughness": 0.23,
      "Pattern": "holo",
      "Resolution": 1024,
      "TextureCount": 6,
      "UsedBy": "green"
    },
    {
      "MaterialSet": "StatusAmberLED",
      "AlbedoRGB": "#F6911A",
      "Metallic": 0,
      "Roughness": 0.24,
      "Pattern": "holo",
      "Resolution": 1024,
      "TextureCount": 6,
      "UsedBy": "amber"
    },
    {
      "MaterialSet": "StatusRedLED",
      "AlbedoRGB": "#F03530",
      "Metallic": 0,
      "Roughness": 0.22,
      "Pattern": "holo",
      "Resolution": 1024,
      "TextureCount": 6,
      "UsedBy": "red"
    },
    {
      "MaterialSet": "StatusBlueLED",
      "AlbedoRGB": "#2375F1",
      "Metallic": 0,
      "Roughness": 0.24,
      "Pattern": "holo",
      "Resolution": 1024,
      "TextureCount": 6,
      "UsedBy": "blue"
    },
    {
      "MaterialSet": "CalibrationVioletLED",
      "AlbedoRGB": "#914BF1",
      "Metallic": 0,
      "Roughness": 0.21,
      "Pattern": "holo",
      "Resolution": 1024,
      "TextureCount": 6,
      "UsedBy": "violet"
    }
  ],
  "unity_mask_channels": "R metallic, G occlusion, A smoothness",
  "shader_targets": [
    "URP Lit",
    "HDRP Lit",
    "Built-in Standard"
  ],
  "normal_map_import": "Set TextureImporter.textureType=NormalMap",
  "glb_embedded_textures": false,
  "notes": "Reference PBR patterns, artist UV unwrap and finishing recommended"
}
```

## ANNEX R — PBR MATERIAL FAMILY REFERENCE MAP

**Source path:** `Materials/Material_Reference_Map.csv`

```csv
FBXMaterial,UnityMaterialSet,Meaning
PS_aluminum,TitaniumBench,Original procedural PBR reference material
PS_amber,StatusAmberLED,Original procedural PBR reference material
PS_black,ElectronicBlackMold,Original procedural PBR reference material
PS_blue,StatusBlueLED,Original procedural PBR reference material
PS_capacitor,MultiLayerCeramicCapacitor,Original procedural PBR reference material
PS_ceramic,TechnicalWhiteCeramic,Original procedural PBR reference material
PS_copper,RolledCopper,Original procedural PBR reference material
PS_cyan,InspectionCyanLED,Original procedural PBR reference material
PS_fr4,FiberglassFR4,Original procedural PBR reference material
PS_frame,TitaniumBench,Original procedural PBR reference material
PS_glass,PrecisionCoatedGlass,Original procedural PBR reference material
PS_gold,ImmersionGoldENIG,Original procedural PBR reference material
PS_graphite,AnodizedGraphite,Original procedural PBR reference material
PS_green,StatusGreenLED,Original procedural PBR reference material
PS_holo,InspectionCyanLED,Original procedural PBR reference material
PS_ic,ElectronicBlackMold,Original procedural PBR reference material
PS_mask,DeepNavyPCBMask,Original procedural PBR reference material
PS_maskdark,DeepNavyPCBMask,Original procedural PBR reference material
PS_panel,BlueBlackScreenGlass,Original procedural PBR reference material
PS_red,StatusRedLED,Original procedural PBR reference material
PS_resistor,CarbonFilmResistor,Original procedural PBR reference material
PS_rubber,ESDSafeRubber,Original procedural PBR reference material
PS_solder,LeadFreeSolder,Original procedural PBR reference material
PS_steel,TitaniumBench,Original procedural PBR reference material
PS_violet,CalibrationVioletLED,Original procedural PBR reference material
PS_whiteink,LaserSilkscreenWhite,Original procedural PBR reference material
```

## ANNEX S — PBR TEXTURE MANIFEST

**Source path:** `Materials/PBR_Texture_Manifest.csv`

```csv
MaterialSet,AlbedoRGB,Metallic,Roughness,Pattern,Resolution,TextureCount,UsedBy
TitaniumBench,#7D8B9A,0.83,0.3,brushed,1024,6,frame;steel;aluminum
AnodizedGraphite,#1E2839,0.64,0.43,brushed,1024,6,graphite
ESDSafeRubber,#161D26,0.02,0.74,rubber,1024,6,rubber
FiberglassFR4,#5C693B,0.03,0.72,fiberglass,1024,6,fr4
DeepNavyPCBMask,#091F5C,0.03,0.28,pcbmask,1024,6,mask;maskdark
RolledCopper,#DA712B,0.89,0.34,brushed,1024,6,copper
ImmersionGoldENIG,#E8BE4E,0.96,0.21,brushed,1024,6,gold
LeadFreeSolder,#BCC9D0,0.85,0.26,brushed,1024,6,solder
TechnicalWhiteCeramic,#E6E7DC,0.09,0.56,ceramic,1024,6,ceramic
LaserSilkscreenWhite,#E7F0F1,0,0.78,ink,1024,6,whiteink
CarbonFilmResistor,#493125,0.05,0.6,carbon,1024,6,resistor
MultiLayerCeramicCapacitor,#A39159,0.12,0.42,ceramic,1024,6,capacitor
ElectronicBlackMold,#11141E,0.05,0.47,carbon,1024,6,ic;black
PrecisionCoatedGlass,#2F80AD,0.03,0.15,glass,1024,6,glass
BlueBlackScreenGlass,#071A37,0.02,0.29,holo,1024,6,panel
InspectionCyanLED,#12B7F4,0,0.2,holo,1024,6,cyan;holo
StatusGreenLED,#14E353,0,0.23,holo,1024,6,green
StatusAmberLED,#F6911A,0,0.24,holo,1024,6,amber
StatusRedLED,#F03530,0,0.22,holo,1024,6,red
StatusBlueLED,#2375F1,0,0.24,holo,1024,6,blue
CalibrationVioletLED,#914BF1,0,0.21,holo,1024,6,violet
```

## ANNEX T — ORIGINAL README FIRST

**Source path:** `Docs/README_FIRST.md`

```markdown
# PROJECT SPARK — LEVEL 95: PULSE ARCHITECT

**Original procedural AAA-inspired 3D artwork, source maps and PCB planning kit — not a playable level.**

## Contents
- `Models/`: 54 separate binary FBX 7400 models + one `L95_PulseArchitect_CompleteShowcase.fbx`.
- `GLB/`: 54 corresponding glTF 2.0 GLBs + one assembled GLB reference.
- `Textures/`: 21 shared PBR texture families × 6 maps = 126 maps, 1024px each.
- `Materials/`: source material names and PBR texture manifest.
- `Shaders/`: Built-in and URP original ShaderLab/HLSL hologram shaders; HDRP supported through native Lit materials.
- `Unity/Editor/`: native material installer, FBX assembler, planning-data importer and art/data validator.
- `Unity/`: optional board visual controller and data-only ScriptableObject.
- `Data/`: board physical data, BOM36, logical pad map, 48 copper tasks, ten vias, W049, objectives, 3 faults, factory branches and 72 planned acceptance tests.
- `Source/`: reproducible Python mesh, texture, data and packaging generators.

## Unity import
1. Extract this top-level folder into `Assets/ProjectSpark/Level95/` (ensure assets remain beneath an `Assets/` folder).
2. Keep `Unity/Editor/*.cs` in a folder literally named `Editor`. Keep runtime `.cs` outside the Editor folder.
3. Use menu **Tools > Project Spark > Levels > 95 > 1. Create Native PBR Materials**.
4. Select a Level 95 layout GameObject and use **2. Assemble 54 Modular FBXs Under Selected Layout**.
5. Optionally run **3. Apply PBR To Selected Art**, **4. Create PCB Planning Data Asset** and **5. Validate Imported Assets And Plans**.
6. For Project Spark's preexisting futuristic room/table, hide the optional `00_Bench` group and use the `01_BareBoard`, `02_TestInstruments`, and `03_Components_36` art roots.
7. In normal gameplay, the 36 components begin not installed and the artwork-only assembler hides them until a real placement system is connected. For visual review, use the visual controller's `ShowAssembledBoardPreview(true)` or the reference assembled model separately.

## Circuit task summary
- 120×80×1.6 mm; 2-layer board, visually presented 12× larger.
- 36 player-installed electronic parts.
- W001-W048 are **unrouted** at level start; ten required vias **missing**.
- W049 is one **separate removable safety feedback jumper**, initially uninstalled.
- Nominal protected 5 V and regulated monitoring 3.3 V, 1024 Hz master, 256 Hz and 2 Hz divided, 256 Hz PWM with 25/50/75% duty.
- Three fault scenarios: C1 wrong value, V06 open, Q1 gate-drain solder bridge.
- All six gameplay objectives require later integration with actual Project Spark electrical/interaction/level systems.

## Disclaimer
These are **real editable mesh assets**, not image illustrations or a promised full playable game. Normalized procedural UV mapping and PBR reference materials are included, not film-grade artist-authored UV atlases or baked high-poly details. Actual Unity shader compilation, materials preview, collision meshes, runtime board simulation, traces, authentic component pinouts and LevelGamePlayManager tests remain to be performed.
```

## ANNEX U — ORIGINAL GAMEPLAY INTEGRATION NOTES

**Source path:** `Docs/Level95_Gameplay_Integration_Plan.md`

```markdown
# PROJECT SPARK — LEVEL 95 / PULSE ARCHITECT
## Future playable implementation contract (not yet implemented)

Goal: a new advanced two-layer precision timing and PWM PCB puzzle integrated into existing Project Spark runtime systems.
Preserve Levels 1–94; never replace SparkElectricalSolver, SparkCircuitSystem, SparkTerminal or LevelGamePlayManager with a disconnected mock.
Inspect actual API signatures from the user's Project before writing gameplay scripts.

## Art and manufacturing planning inputs
- 54 independent FBXs + 54 matching GLBs, plus assembled visual showcases.
- 36 placeable component FBXs; **all 36 begin not installed**. The assembled showcase is reference geometry only.
- 48 required route IDs W001–W048, all **initially NOT_ROUTED**; their copper geometry must be generated by player interaction, not taken from the decorative pad assets.
- 10 player-placed vias V01–V10, initially NOT_PLACED.
- W049 detachable physical jumper, initially NOT_INSTALLED, required for normal commissioning and removed only for the Objective 6 fault exercise.
- 150 physical/logical pad and test-point records (the W049 service contacts J3.A/B have dedicated logical identifiers).
- Components are scaled visually 12× from the 120×80 mm simulation PCB; all gameplay widths, distances, electrical parameters and DRC stay in millimetres.

## A six-objective evidence model
1. BOM/schematic inspection and verified physical assembly of all 36 components.
2. Correct 48 route IDs, ten vias, and W049 installed, backed by actual topology, clearance and short tests.
3. Calibrate U1 1024 Hz; U3 produces /4 (256 Hz) and /512 (2 Hz) with measured frequency and amplitude.
4. Measure PWM at 256 Hz with 25, 50 and 75% duty under load with fresh samples.
5. Faults C1 misvalue, V06 open connection, Q1 gate-drain bridge must alter actual circuit state; repair and rerun tests.
6. Deliberately remove W049, fault latches and driver disables, physically reconnect, recalibrate, explicitly reset and pass a fresh full sequence.

## Simulation fidelity rules
- The 5 V input is a protected low-voltage *gameplay* supply. The 3.3 V rail is a secondary regulator output.
- U1 is modeled as a TLC555-style RC oscillator. For representative nominal R1=14.7kΩ, R2=63kΩ and C1=10nF, ideal f≈1023.45 Hz; permit explicit tolerances.
- U3 is a CD4040B-style 12-stage binary counter. /4 (Q2) and /512 (Q9) are timing channels, NOT two unrelated arbitrary clocks.
- U4's 'RAMP_SYNC', 'RAMP_OUT' and 'DUTY_REF' names are educational **functional aliases**. Do not treat this source plan as a manufacturer-accurate LM393 pin-number drawing.
- The fitted RV1 adjustment controls PWM **duty**, not master clock. Master frequency is set by R1, R2, C1 and optional educational calibration/replacement.
- The LM393 open-collector result needs a pull-up; R10 is included in fixed local branches.
- The counter, comparator and logic gate must share compatible voltage domains in the scenario and have valid ground.
- All test instruments must sample actual electrical nodes (with timestamps, validity and calibration); never render fake certifying waveforms.
- Unknown / disconnected W049 = **invalid feedback**, never interpreted as 0% PWM or zero current.
- The user-visible schematic, placeholder IC pin names, footprints and topology must be reviewed for physical datasheet correctness before any real-world PCB fabrication claim.
- Distinguish actual net connectivity from *individual trace task evidence*. A detour along factory copper must not silently complete an unrouted W ID.

## Unity / project compatibility
Under `Tools/Project Spark/Levels/95/`:
1. Create Native PBR Materials
2. Assemble 54 Modular FBXs Under Selected Layout
3. Apply PBR To Selected Art
4. Create PCB Planning Data Asset
5. Validate Imported Assets And Plans

Art-only helper scripts do not create a live PCB solver or six-objective gameplay. The future Level 95 builder should live at `Tools/Project Spark/Levels/Create Level 95` after actual source/API inspection.

## Avoid duplicating the existing game room
The first eight FBXs are *optional bench furniture*. The user already has a futuristic Project Spark studio and can disable `00_Bench` and display only the board, fixture, instruments and parts tray on the 9.9m level-presentation table. Adjust model placement visually without multiplying the PCB's 120×80mm physical dimensions.
```

## ANNEX V — ORIGINAL CRITICAL W049 RECOVERY DOCUMENT

**Source path:** `Docs/W049_Critical_Output_Feedback_Recovery.md`

```markdown
# W049 — TRUSTED PWM FEEDBACK AND PROTECTED OUTPUT RECOVERY

- Physical service part: `L95_W049_DetachableMonitorJumper`.
- Contact source: `J3.A`, branch fed from actual J2.LOAD_NEG output-monitor path.
- Contact destination: `J3.B`, branch to `U7.TRUSTED_PWM_MONITOR_IN`.
- Functional block source: `L95_PWMOutputFeedbackMonitor.TRUSTED_PWM_MONITOR_OUT`.
- Functional safety input: `L95_TimingSafetyMonitor_U7.TRUSTED_PWM_MONITOR_IN`.
- Initial state: **W049 is physically uninstalled**. It must be installed to commission normal operation.

### Certified fault sequence (Objective 6, after completed Objectives 1–5)
1. Under a guarded low-energy test, player removes the *real* W049 component.
2. The authoritative circuit/connection graph reports J3.A -> J3.B **open**.
3. Any cached PWM monitor reading is invalidated; independently supervised protected output is disabled.
4. A critical feedback fault latches. Diagnostic oscillator and oscilloscope reading remain available where electrically safe.
5. Player reinstalls the physical jumper with proper contact quality and matching orientation.
6. New measured monitor samples and cross-checks must agree with actual output behavior. Previously cached samples are ineligible.
7. The safety controller accepts an **explicit operator reset** only after all prerequisites qualify.
8. Fresh independent commissioning verifies 1024/256/2 Hz, 25/50/75% PWM and safe load behavior.
9. Authoritative `LevelGamePlayManager` reaches 6/6. Never bypass the actual objective evaluator.

### No false claims
The supplied FBX contains the shunt geometry and contact planning data only. Functional topology, current sensing, PLC logic and gameplay must be implemented in the real Project Spark Unity project.
```

## ANNEX W — ORIGINAL VISUAL RIG INTEGRATION NOTES

**Source path:** `Docs/Visual_Rig_Integration_Notes.md`

```markdown
# L95 Visual Rig — Unity binding guidance

All 48 named pivot nodes are present in the FBXs and GLBs, including 36 `ANIM_<REF>_INSTALL` component presentation transforms.
The 48 pivots are *art transforms*, not animated GameObjects or gameplay controllers. Every pivot axis is recorded in `Data/Animation_Pivots.csv`. Do not hardcode all rotation about Y.

### Installation / runtime synchronization
- Board art is enlarged 12× while virtual physical positions remain 120×80mm. Component installation must snap to physical footprint coordinates, then drive visual `ANIM_<REF>_INSTALL` without changing net coordinates.
- Initial assembled full-scene GLB and FBX are **showcase** art, not the initial gameplay state.
- The Unity art assembler hides `03_Components_36` initially. A future component placement system should show / move each selected mesh from inventory when actual component placement succeeds.
- `ANIM_W049_INSTALL` is purely a visual pivot: the actual critical connection must be changed in Spark electrical topology.
- Test instruments can animate knobs, but only measured voltage/frequency/duty/current may populate SCADA.
- A probe attachment must affect which actual net is sampled; waveform display assets do not imply an active probe.
```

## ANNEX X — SOURCE UNITY HELPER SCRIPTS AND EDITOR COMMANDS

The source kit contains these six code files (read and inspect them before integration):

```text
Unity/SparkLevel95PCBDefinition.cs
Unity/SparkLevel95BoardVisualController.cs
Unity/Editor/SparkLevel95ArtAssembler.cs
Unity/Editor/SparkLevel95ArtValidator.cs
Unity/Editor/SparkLevel95PBRMaterialEditor.cs
Unity/Editor/SparkLevel95PlanningDataEditor.cs
Shaders/ProjectSpark95_PulseHologramURP.shader
Shaders/ProjectSpark95_PulseHologramBuiltIn.shader
```

**Exact existing Editor menu paths:**

```text
Tools/Project Spark/Levels/95/1. Create Native PBR Materials
Tools/Project Spark/Levels/95/2. Assemble 54 Modular FBXs Under Selected Layout
Tools/Project Spark/Levels/95/3. Apply PBR To Selected Art
Tools/Project Spark/Levels/95/4. Create PCB Planning Data Asset
Tools/Project Spark/Levels/95/5. Validate Imported Assets And Plans
```

**Target new runtime builder:** `Tools/Project Spark/Levels/Create Level 95`.

Do not claim these existing helpers generate live terminals, PWM physics or six-objective game logic.

---

# COMMAND TO THE IMPLEMENTING UNITY AGENT

**Implement, compile, test and demonstrate Project Spark Level 95: Pulse Architect with the supplied Level 95 assets and source tables.** Inspect the actual Project Spark Unity project first, preserve Levels 1–94, reconcile the source-specific caveats above, create a non-destructive one-click builder, implement genuine PCB trace/via/component topology and measurement-based timing/PWM gameplay, recover all three real faults and W049 safely, and show all 72 source acceptance tests plus a full 0/6→6/6 Play Mode run through the authoritative `LevelGamePlayManager`. Report any unavailable existing APIs or unresolved source contradictions openly rather than inventing successful integration.
