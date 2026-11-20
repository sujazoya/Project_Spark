# PROJECT SPARK — LEVEL 104: MEMORY MATRIX
## MASTER UNITY IMPLEMENTATION PROMPT — SOURCE-GROUNDED, PRODUCTION CONTRACT

> **Use this entire document as the implementation request for the Project Spark Unity project.** It is an engineering/development specification, not finished Unity runtime code. Inspect actual existing Project Spark classes and scene APIs before writing or replacing code. Do not fabricate evidence of Unity compilation, electrical topology, working DDR4 simulation, or gameplay completion.

### 0. Mission and non-negotiable outcome

Implement **Level 104 / MMX-12 Memory Matrix**, a board-only, eight-layer DDR4 educational motherboard with **94 separately installable components**, **124 selected copper-routing tasks**, **34 via tasks**, and **six authoritative objectives**. Reuse Project Spark's existing large engineering room/table and its **AAA world-space Canvas virtual inspection and repair tools**. Do **not** build a second room, decorative physical oscilloscopes, handheld tools or unrelated environmental FBXs. Preserve all existing **Levels 1–103** and existing shop/object/electrical systems. Only complete this level when independent checks prove that actual component installation, topology, DDR4 power/initialization, four-byte-lane memory training, three recoverable faults and W125 disconnection/recovery worked.

Use the package `ProjectSpark_Level104_MemoryMatrix_BoardOnly_Realistic_Complete.zip` as the source of truth for **art filenames, BOM entries, placement coordinates, source task records and included helper scripts**. Use this document for requirements and identify where source records are placeholders. Do not silently treat user-facing 124 selected route tasks as the full total of DDR4 physical copper segments.

### 1. Source evidence, status and blockers

- `Data/ExportSummary.json`: 0.260 m × 0.185 m PCB, 8 copper layers, 120 modular FBX and GLB models, 94 installable pieces, 26 extra board assets, 16 starter material families and 49 maps.
- `Data/ComponentBOM.csv`: precise source component reference IDs and kind/description; these govern art matching, not verified footprint/pin data.
- `Data/ComponentPlacements.csv`: intended coordinates in meters, all marked not installed and footprints not verified.
- `Data/ModelManifest.csv`: exact FBX/GLB names, placements and installable flags.
- `Data/CopperRoutes_124_REQUIRES_SCHEMATIC.csv`: **all 124 rows have unassigned source/destination pads and net IDs and are marked non-certifiable**.
- `Data/ViaTasks_34_REQUIRES_SCHEMATIC.csv`: **all 34 via rows have unassigned net IDs and TBD layer spans**.
- `Data/W125_Recovery.json`: calls for logical training monitor **U11** → safety supervisor **U12**, but the actual BOM does not include U11/U12. **BLOCK functional W125 until resolved with a pin-level schematic and matching concrete physical parts.** Do not manufacture silent pseudo-components or pretend U10 is one of these two devices.
- `Data/FaultPlan.json`: F1 DQS strobe via open, F2 command/address termination failure, F3 U3 intermittent BGA contact.
- `Data/Objectives.json`: six objectives with initial state installed=0, routesCompleted=0, viasInstalled=0, W125 OPEN. `authoritativeCompletion` is false.
- `Unity/Editor/SparkL104BoardArtBuilder.cs`: builds visual EMPTY board from `095_L104_PCB_Substrate_260x185mm.fbx` and separate art reference `121_L104_AssembledReference.fbx`; it does not create gameplay circuitry.
- `Unity/Editor/SparkL104MaterialStarter.cs`: creates simple URP/Standard starter materials, does **not** complete FBX slot remapping nor correct texture packing.
- `Unity/Runtime/SparkL104VisualOnly.cs`: explicit art-only warning component.

**Hard gate:** If schematic or actual Project Spark APIs are unavailable, build a compile-safe, isolated scaffolding mode with explicit NOT_CERTIFIABLE status; do not wire fake authoritatively completed objectives. Output the unresolved schematic/pin list instead of invented pins. Do not claim fully playable until verified end-to-end tests pass.

### 2. Project integration and preservation

1. Inspect current project assemblies, prefabs, namespaces, installed packages, `SparkTerminal` equivalents, the actual electrical solver, the user's World-Space PCB Virtual Studio, and `LevelGamePlayManager`. Adapt to discovered APIs. Do not invent existing method signatures.
2. Implement as a Level 104 namespace/assembly, with Editor-only generation code under `Editor/` and runtime code separate. Use additive registration rather than rewriting shared gameplay.
3. Unique scene, root and asset IDs. No static initialization that changes earlier levels. Avoid globally changing tags, layers, materials and input settings.
4. Support desktop mouse, mobile touch and keyboard/gamepad navigation of the virtual inspection UI. UI pointer hits must not accidentally route PCB wires underneath the panels.
5. The board is real scale **260 × 185 mm** in Unity meters. Apply a separate presentation-scale root if needed and keep topology and length measurements in physical coordinates; enforce conversion for colliders, camera picking and VFX.
6. All model slots must be bound by **exact source `asset_id`**; forbid using duplicate assembled-showcase geometry to represent installed parts.
7. The board camera and canvas must fit the existing large table without adding new furniture or physical instruments.

### 3. One-click Unity authoring tools

Retain exact menus already supplied:

- `Tools/Project Spark/Levels/104/1. Build EMPTY MMX-12 Board`
- `Tools/Project Spark/Levels/104/2. Show Assembled ART Reference`
- `Tools/Project Spark/Levels/104/3. Create PBR Starter Materials`

Add `Tools/Project Spark/Levels/Create Level 104` as an **idempotent** full-level creator. It must import/validate asset references, create Level104 root, build exactly one empty gameplay PCB, register 94 component placement slots, create defined pad/via placeholders with NOT_CERTIFIABLE status pending schematic approval, set up virtual tools/diagnostics, scenario objectives, fault presets, W125 test panel, cameras and debug displays, and run validation. Re-running must reconcile instead of duplicating objects or breaking older scenes. Use Undo and Editor diagnostics.

### 4. Asset import and materials

Import all source FBXs without changing filenames, GLBs as optional alternate references, and preserve original hierarchy if present. Verify FBX custom Binary 7400 import into the user's actual Unity version and chosen render pipeline; check mesh normals, UVs, pivots, materials, scale and bounds. Treat 49 textures and 16 material families as **starter PBR**, not finished AAA work. Where needed, correctly pack roughness/smoothness and metallic/AO for URP or HDRP, enable normal maps, and remap actual FBX renderer material slots by matching material identities. Report missing maps; do not assert production AAA finishing that was not done. Keep CPU/memory BGA solder balls and board layer references viewable in microscope/X-ray; use instance batching/LODs only where detailed zoom is preserved.

### 5. Gameplay initial state and component placement

Start with `installed=0/94`, `routesCompleted=0/124`, `viasInstalled=0/34`, `W125=OPEN`, objectives `0/6`, controller unpowered or safely supply-disabled and DDR training INVALID. Show separate reference assembly only inside optional Editor preview. Each component has an inventory record, source `ref`, source `asset_id`, expected footprint, orientation, value, polarity if applicable, and a validated snap pose. Components U1, U2 and U3 are BGA packages: mounting and solder-contact verification must be explicit and separate from a superficial mesh overlap check. Use click/touch select → ghost preview → rotate/align → seat → inspect contacts/solder → independent validation. Incorrect package, orientation, footprint, poor solder, missing net connection or wrong component value must prevent qualification. Replacing a component should invalidate affected measurements and objectives until retested.

### 6. Required schematic reconciliation before circuit gameplay

Build/review a pin-level approved DDR4 memory-down schematic and an authoritative PCB stack-up. Define package pin/ball maps for every active IC, power/gnd ball groups, controller-to-U2/U3 byte lanes, DQ/DQS/DM-DBI where applicable, CK/CK#, CA/command/control, Vref/VTT/VPP/VDD, clock, reset, termination, reference networks and W125 physical pins. Create resolved pad database `{componentRef, packagePin, padId, netId, layer, type, physicalLocation}`. Resolve all 124 selected tasks with exact source and destination pad IDs and net IDs; specify true factory-copper segments without bypassing user-required routes. Resolve all 34 vias with actual net, start/end layer, geometry, plated connection and permitted via technology. Use a design-rule checker to detect shorts, missing returns, unconnected balls, invalid copper crossings, impossible via spans, manufacturing keep-out violations, and routes inadvertently pre-connected by the showcase. Refuse to mark any task pass if `UNASSIGNED`, `TBD`, `False`, or other placeholder fields remain.

### 7. Eight-layer electrical model

Implement layer-aware topology, contiguous power/ground regions, via spans and plane isolation; retain the final approved stack-up, not an arbitrary fixed assumption. Respect BGA escape routing. Track actual electrical connections and pads via the solver, and use separate geometric parameters for signal transmission delay, parasitic C/L/R and realistic return paths. Model power/reference contact opens as real faults. Use full physical PCB data for electrical operations and the layer viewer strictly as a visualization of that data. The player completes **selected** route/via tasks while the full board may have many more physical segments.

### 8. DDR4 simulation scope and precision

Implement an **educational, bounded, deterministic DDR4-style model**, not claim to emulate JEDEC-certified hardware. The architecture uses a virtual x32 channel implemented with two x16 DDR4 chips, and four x8 byte lanes at an illustrative **1600 MT/s** target. Finalized component specifications must define ranks, burst behavior, command support, row/bank/address organization, actual timing parameters, device capacity, controller interfaces and on-die termination modes. Implement separate states for powered/unpowered, reset, initialized, trained, ready, degraded and failed. Compute transactions through validated controller/memory electrical paths; a simple on/off sprite is insufficient.

### 9. Power/reset/initialization

Commission protected input, specified controller supplies, DDR4 VDD **1.20 V nominal**, VPP **2.50 V nominal**, VTT **0.60 V nominal where required**, and associated reference circuits with separate modeled functions. Do not incorrectly use VTT as the DRAM's entire data supply or treat VPP as a general logic rail. Validate startup sequence, reset release, stable reference values, commanded initialization and memory mode setup against the finalized model. Brownout, missing power return, unstable termination, overvoltage or an open VPP net must have distinct effects on actual memory readiness. Failed tests must preserve diagnostics while blocking protected CPU workload authorization.

### 10. Memory timing and training

Model differential clock behavior and per-byte-lane DQS/DQ training using actual signal arrival times and sampling margins, not a predetermined pass switch. For each of four byte lanes, maintain read/write timing windows, selected sampling delay, reference settings, signal-quality metrics and test timestamps. Provide configurable write leveling, read alignment, write alignment and reference training consistent with the selected simplified architecture; include controller-to-device asymmetry, package propagation, trace and via delay, thermal/noise effects and termination. Failed strobe or missing memory BGA contact must alter measured training results. Training evidence expires when relevant geometry, fault, reference, power or firmware state changes.

### 11. Memory test and error localization

Implement deterministic write/read verification with address tests, walking-one/zero patterns, pseudorandom data, bursts and variable timing. Explain the exact test duration, number of bits and observed errors in UI. Error-free finite testing is **zero errors observed**, not mathematical proof of BER=0. Collect lane/device/address/bit, expected versus actual data, timestamps and board configuration. During F3, intermittent contact should be conditional on a modeled thermal/mechanical state, not purely random unreproducible failures. Supply seeded deterministic test reproducibility and optional controlled randomness for advanced mode.

### 12. Real world-space virtual inspection and repair

Bind existing user Virtual PCB Studio tools: multimeter, oscilloscope, logic analyzer, memory trainer, eye viewer, net tracer, X-ray BGA scanner, layer viewer, solder/desolder, via repair, component replace, thermal scanner and calibrated test console. Tools are UI panels on a world-space Canvas; use hit-testing on real board mesh colliders, pads, vias, component bodies and BGA target subzones. One selected tool plus a clicked/touched target creates a typed request with tool ID, target ID, position, player/session and safety conditions. Reject invalid tool-target pairs. Procedural VFX at surface contact point: localized solder glow, short sparks, heat halo, smoke only when warranted, layer scan pulses and contact highlights. VFX are response to accepted action, never proof of successful electrical repair. BGA rework requires authorized safe state, simulated thermal profile and post-rework verification—not a single generic solder click.

### 13. Three recoverable fault scenarios

**F1 — DQS strobe via open, byte lane 2.** Open a designated, approved DQS connection in the authoritative graph, affecting that lane's training/transactions. Detect with layer viewer/net tracer and appropriate waveform evidence. Repair only via qualified layer-aware via rework. Confirm restored continuity, new training, new read/write tests.

**F2 — Command/address termination failure.** Install or degrade designated termination component with a documented expected value and net. Introduce waveform/reflection/timing errors supported by the model. Replace value-correct component and verify updated signal integrity and initialization/training tests.

**F3 — U3 intermittent BGA contact.** Select one specific approved memory contact/ball net after schematic reconciliation. The defect changes resistance or continuity under defined conditions and produces attributable errors. Use X-ray/diagnostic evidence, controlled BGA rework or part replacement; then prove contact integrity and retrain/retest. These faults should not all auto-trigger at startup unless progression demands; fault injection and restoration require reproducible snapshots and no shortcuts.

### 14. W125 independent safety logic — MUST RESOLVE REFERENCE MISMATCH

Source `W125_Recovery.json` names:

- `L104_MemoryTrainingMonitor_U11.TRUSTED_MEMORY_READY_OUT`
- `L104_MemorySafetySupervisor_U12.TRUSTED_MEMORY_READY_IN`

BUT the supplied `ComponentBOM.csv` contains neither U11 nor U12. Assign their real components, package pins/balls, footprint and net using an approved schematic **before** constructing W125 in gameplay; otherwise label it `SCHEMATIC_VALIDATION_REQUIRED`. W125 is a removable **memory-ready health signal**, **not DDR4 VDD/VPP/VTT and not a DQ lane**. During commissioned operation, disconnect real conductive W125 path: independent monitor detects missing/frozen/stale validity, blocks protected memory-dependent processor workload, maintains diagnostic supply and latches a safety fault. Reconnection alone does not clear fault. Require physical path restoration → fresh successful four-byte-lane retraining → fresh complete memory data test → explicit safety reset → new complete qualification. A constant HIGH is not sufficient evidence. Objectives 1–5 historical awards may be retained, but their test evidence is not automatically valid for Objective 6.

### 15. Objective system — six genuine targets

1. **Assembly:** 94 validated installed components and solder contacts.
2. **Routing:** 124 resolved required copper connections, 34 resolved via tasks, no unintended shorts, fully valid net graph and W125 connected for commissioning.
3. **Power and initialization:** correct VDD/VPP/VTT and controller supplies, successful DDR device initialization and controller state.
4. **Four-byte-lane training:** real trained margins and reproducible read/write data tests at the defined operating conditions.
5. **Three faults:** F1/F2/F3 separately diagnosed/repaired and a complete fresh training/data test.
6. **W125 recovery:** actual disconnect detection, independent safety inhibition, reconnection, fresh training and data test, explicit reset and full requalification.

Register with the actual `LevelGamePlayManager` APIs discovered in the Unity project. Award a task only from an authoritative verification event, not from a UI click, animation, checkbox, debug flag, object count alone, showcase model or test tool visual. Ensure six final objectives, 0/6 start, stable persistence semantics, reset support and no double award. Display human-readable evidence of why an objective is blocked.

### 16. UX and AAA presentation

Reuse the existing large table and virtual tools; show PCB 3D geometry in the center, six-target tracker, component/route/via counters, current tool, safe-state warnings, DDR4 supplies, device initialization, per-lane read/write training margin, memory-test errors, repair evidence and W125 status. Make labels readable at PC and touch resolutions, provide zoom/pan/rotate and optional x-ray layer isolation. UI telemetry must be bound to live authoritative simulation output; never show 'LIVE' readings from a fixed waveform. Keep UI uncluttered by showing detailed diagnostics only for selected mode/tool. Present PCB PBR materials realistically—no arbitrary emissive overlay obscuring actual solder joints.

### 17. Safety, determinism and stability

All electrical actions are simulated. Block unsafe hot rework or trace repair when the modeled board is energized; provide explicit diagnostic mode for live measurements. Validate numeric ranges and user-supplied tuning values; cap training iteration counts and memory-test work per frame to avoid Unity stalls. Use stable deterministic seeds for repeatable bugs and tests. Avoid allocating unbounded temporary meshes or line renderers for every trace. Preserve performance on target mobile/desktop devices; apply pooling to VFX and board highlights. No actual device programmer or real hardware voltage is controlled.

### 18. Save/load, event ordering and idempotency

Persist component placement/repair state, resolved topology, via installation, simulated failure configuration, firmware/training revision, measurement evidence freshness, safety-latch and objective history. On load, rebuild graph before restoring qualification; do not load stale 'all-pass' evidence across changed topology. Repair transactions must be atomic and undoable in editor testing. Duplicate events or repeated Editor build actions cannot award points twice or create second board copies.

### 19. Definition of done

Deliver a complete Unity package with source `.cs`, Editor menu, prefabs/ScriptableObjects/data validators, materials and scene integration, plus README, schematic reconciliation table, asset-link report, test log and accepted final state screenshot/video from **actual** Unity Play Mode. Require no compile errors and no missing references in a clean project import. Successful actual gameplay: all 94 parts installed, 124 route tasks and 34 vias validated, memory supply/initialization established, four lanes trained, three faults repaired, W125 physically disconnected/reconnected and fully requalified, ending with genuine `LevelGamePlayManager` **6/6**. Without required evidence, report PARTIALLY IMPLEMENTED rather than certify. Maintain no regression in Levels 1–103.

### 20. Proposed implementation classes (NOT existing-API claims)

`SparkLevel104Coordinator`, `SparkLevel104BoardData`, `SparkDDR4PowerState`, `SparkDDR4InitializationMachine`, `SparkDDR4ControllerSim`, `SparkDDR4MemoryDeviceSim`, `SparkMemoryLaneTiming`, `SparkMemoryTrainingEngine`, `SparkMemoryDataTest`, `SparkL104BGAContactFault`, `SparkLevel104FaultController`, `SparkLevel104W125Safety`, `SparkLevel104ObjectiveTracker`, `SparkLevel104VirtualToolBridge`, `SparkLevel104Validator`, `SparkLevel104EditorBuilder`. Replace or adapt only after inspecting real systems. Use interfaces to connect electronics simulation to visual-only existing art.

### 21. Acceptance test suite — required automated regression categories

Acceptance tests must include at minimum the following explicit cases. Each reports arrangement, action, expected circuit state, actual measured values, expected objective changes and a deterministic log or screenshot.

001. **[Assets]** 120 FBX assets import without missing meshes.
002. **[Assets]** 120 GLBs available as alternate source.
003. **[Assets]** assembled reference is not in gameplay root.
004. **[Assets]** board physical dimensions 0.260 x 0.185 m.
005. **[Assets]** visual presentation scale does not alter PCB distance metrics.
006. **[Assets]** all 94 component IDs are unique.
007. **[Assets]** all 94 component prefabs bind correct asset IDs.
008. **[Assets]** all 26 detail meshes are non-installable.
009. **[Assets]** all 8 layer views map to correct copper records.
010. **[Assets]** FBX normals and UVs recognized by importer.
011. **[Assets]** PBR slots remapped with no missing material.
012. **[Assets]** missing texture creates diagnostic not silent default.
013. **[Initial]** exact counters 0/94 0/124 0/34 0/6.
014. **[Initial]** W125 starts OPEN.
015. **[Initial]** power held safe until board qualifies.
016. **[Initial]** reference art does not affect electrical state.
017. **[Placement]** correct U1 BGA orientation required.
018. **[Placement]** incorrect U2 memory package rejected.
019. **[Placement]** U3 BGA incomplete contacts fail assembly.
020. **[Placement]** wrong resistor value blocks assembly.
021. **[Placement]** wrong capacitor rating blocks assembly.
022. **[Placement]** duplicate component placement rejected.
023. **[Placement]** component removal invalidates training evidence.
024. **[Placement]** touch and mouse snap have identical result.
025. **[Routing]** unassigned source pad prevents certification.
026. **[Routing]** unassigned destination pad prevents certification.
027. **[Routing]** unassigned net prevents certification.
028. **[Routing]** factory copper cannot bypass missing route.
029. **[Routing]** wrong-net copper produces short fault.
030. **[Routing]** missing return path blocks qualification.
031. **[Routing]** cross-layer path without valid via rejected.
032. **[Routing]** all selected routes counted exactly once.
033. **[Vias]** TBD layer span prevents certification.
034. **[Vias]** UNASSIGNED via net prevents certification.
035. **[Vias]** invalid plated span rejected.
036. **[Vias]** open via interrupts correct net.
037. **[Vias]** via repairs update graph only after authorization.
038. **[Power]** missing VDD prevents initialization.
039. **[Power]** missing VPP prevents qualified initialization.
040. **[Power]** incorrect VTT fails defined termination scenario.
041. **[Power]** brownout resets memory-readiness state.
042. **[Power]** supply overvoltage triggers protection.
043. **[Power]** power and ground not silently merged.
044. **[Power]** controller supplies are independent from DRAM VDD.
045. **[Initialization]** reset must be held during invalid startup.
046. **[Initialization]** valid supplies allow controlled init.
047. **[Initialization]** incorrect command sequence fails.
048. **[Initialization]** U2 absent blocks channel readiness.
049. **[Initialization]** U3 absent blocks channel readiness.
050. **[Training]** all 4 byte lanes trained separately.
051. **[Training]** DQS open causes associated lane failure.
052. **[Training]** read timing outside window fails.
053. **[Training]** write timing outside window fails.
054. **[Training]** bad Vref reduces measurement margin.
055. **[Training]** training evidence expires after PCB edit.
056. **[Training]** training evidence expires after power rail fault.
057. **[Training]** selected test rate is documented.
058. **[Testing]** walking-one pattern detects stuck bit.
059. **[Testing]** address aliasing detected.
060. **[Testing]** read/write mismatch logs device and lane.
061. **[Testing]** zero observed errors not reported BER zero.
062. **[Testing]** finite test records bit count and duration.
063. **[Testing]** seeded diagnostic run is reproducible.
064. **[F1]** via fault physically opens selected DQS net.
065. **[F1]** wrong-tool repair attempt rejected.
066. **[F1]** qualified via rework restores continuity.
067. **[F1]** post-repair retraining required.
068. **[F2]** wrong termination creates measurable failure.
069. **[F2]** replacement component must have correct value.
070. **[F2]** fresh transaction test required.
071. **[F3]** U3 BGA fault targets schematic-selected ball.
072. **[F3]** surface solder click alone cannot fix hidden BGA.
073. **[F3]** thermal conditions can reveal intermittent fault.
074. **[F3]** qualified rework or replacement restores contact.
075. **[F3]** X-ray visual does not certify independent repair.
076. **[W125]** unresolved U11/U12 references block functional builder.
077. **[W125]** missing physical pin assignment stays NOT_CERTIFIABLE.
078. **[W125]** disconnect causes actual graph open.
079. **[W125]** stuck HIGH cannot impersonate fresh heartbeat.
080. **[W125]** safety inhibits protected CPU memory operations.
081. **[W125]** diagnostic power remains available.
082. **[W125]** reconnect alone does not clear latch.
083. **[W125]** four lanes must retrain after reconnect.
084. **[W125]** fresh data test required after reconnect.
085. **[W125]** explicit reset required.
086. **[Objectives]** no award from UI animation.
087. **[Objectives]** no award from decorative reference mesh.
088. **[Objectives]** no double award from duplicate events.
089. **[Objectives]** LevelGamePlayManager ends exactly 6/6.
090. **[Objectives]** Objectives 1-5 history retained correctly.
091. **[Objectives]** save/load does not reuse stale certification.
092. **[UI]** world-space UI intercepts pointer interactions.
093. **[UI]** contact VFX emitted at clicked target.
094. **[UI]** VFX completion never mutates solver automatically.
095. **[UI]** touch and mouse selection consistency.
096. **[UI]** instrument shows actual measured state.
097. **[Editor]** one-click builder idempotent on repeated run.
098. **[Editor]** existing art builder menu preserved.
099. **[Editor]** starter materials menu preserved.
100. **[Regression]** Level 103 remains unchanged.
101. **[Regression]** all previous Levels 1-102 unaffected.
102. **[Regression]** no new room or physical instrument assets.

### 22. Technical annexes (verbatim source records)

The following source tables are included without synthesizing missing electrical assignments; they MUST be reconciled rather than treated as finished schematic data.


#### Annex A. Export Summary — `Data/ExportSummary.json`

```json
{
  "level": 104,
  "name": "MEMORY MATRIX",
  "board_size_m": [
    0.26,
    0.185
  ],
  "layers": 8,
  "modular_fbx": 120,
  "modular_glb": 120,
  "installable_components": 94,
  "extra_board_assets": 26,
  "complete_reference": 1,
  "starter_material_families": 16,
  "texture_maps": 49,
  "routing_tasks": 124,
  "via_tasks": 34,
  "electrical_certifiable": false,
  "authoritative_gameplay_implemented": false,
  "fbx_format": "FBX Binary 7400 (custom exporter)"
}
```

#### Annex B. Model Manifest — `Data/ModelManifest.csv`

```csv
index,asset_id,type,fbx,glb,x_m,y_m,z_m,installable
1,L104_U1,controller,001_L104_U1.fbx,001_L104_U1.glb,-0.048,0.00088,0.005,True
2,L104_U2,ddr4,002_L104_U2.fbx,002_L104_U2.glb,0.058,0.00088,0.036,True
3,L104_U3,ddr4,003_L104_U3.fbx,003_L104_U3.glb,0.058,0.00088,-0.037,True
4,L104_U4,ic,004_L104_U4.fbx,004_L104_U4.glb,-0.104,0.00088,0.065,True
5,L104_U5,ic,005_L104_U5.fbx,005_L104_U5.glb,-0.082,0.00088,0.065,True
6,L104_U6,ic,006_L104_U6.fbx,006_L104_U6.glb,-0.103,0.00088,-0.059,True
7,L104_U7,ic,007_L104_U7.fbx,007_L104_U7.glb,-0.082,0.00088,-0.059,True
8,L104_U8,ic,008_L104_U8.fbx,008_L104_U8.glb,-0.058,0.00088,0.063,True
9,L104_U9,ic,009_L104_U9.fbx,009_L104_U9.glb,-0.04,0.00088,0.063,True
10,L104_U10,ic,010_L104_U10.fbx,010_L104_U10.glb,-0.02,0.00088,0.063,True
11,L104_RN1,termination,011_L104_RN1.fbx,011_L104_RN1.glb,-0.112,0.00088,0.072,True
12,L104_RN2,termination,012_L104_RN2.fbx,012_L104_RN2.glb,-0.099,0.00088,0.072,True
13,L104_RN3,termination,013_L104_RN3.fbx,013_L104_RN3.glb,-0.071,0.00088,0.072,True
14,L104_RN4,termination,014_L104_RN4.fbx,014_L104_RN4.glb,-0.054,0.00088,0.072,True
15,L104_RN5,termination,015_L104_RN5.fbx,015_L104_RN5.glb,-0.035,0.00088,0.072,True
16,L104_RN6,termination,016_L104_RN6.fbx,016_L104_RN6.glb,-0.018,0.00088,0.072,True
17,L104_RN7,termination,017_L104_RN7.fbx,017_L104_RN7.glb,0.002,0.00088,0.072,True
18,L104_RN8,termination,018_L104_RN8.fbx,018_L104_RN8.glb,0.021,0.00088,0.072,True
19,L104_RN9,termination,019_L104_RN9.fbx,019_L104_RN9.glb,0.039,0.00088,0.072,True
20,L104_RN10,termination,020_L104_RN10.fbx,020_L104_RN10.glb,0.061,0.00088,0.072,True
21,L104_RN11,termination,021_L104_RN11.fbx,021_L104_RN11.glb,0.08,0.00088,0.072,True
22,L104_RN12,termination,022_L104_RN12.fbx,022_L104_RN12.glb,0.099,0.00088,0.072,True
23,L104_R1,resistor,023_L104_R1.fbx,023_L104_R1.glb,0.112,0.00088,0.072,True
24,L104_R2,resistor,024_L104_R2.fbx,024_L104_R2.glb,-0.112,0.00088,0.058,True
25,L104_R3,resistor,025_L104_R3.fbx,025_L104_R3.glb,-0.099,0.00088,0.058,True
26,L104_R4,resistor,026_L104_R4.fbx,026_L104_R4.glb,-0.071,0.00088,0.058,True
27,L104_R5,resistor,027_L104_R5.fbx,027_L104_R5.glb,0.002,0.00088,0.058,True
28,L104_R6,resistor,028_L104_R6.fbx,028_L104_R6.glb,0.021,0.00088,0.058,True
29,L104_R7,resistor,029_L104_R7.fbx,029_L104_R7.glb,0.039,0.00088,0.058,True
30,L104_R8,resistor,030_L104_R8.fbx,030_L104_R8.glb,0.099,0.00088,0.058,True
31,L104_R9,resistor,031_L104_R9.fbx,031_L104_R9.glb,0.112,0.00088,0.058,True
32,L104_R10,resistor,032_L104_R10.fbx,032_L104_R10.glb,-0.112,0.00088,0.044,True
33,L104_R11,resistor,033_L104_R11.fbx,033_L104_R11.glb,-0.099,0.00088,0.044,True
34,L104_R12,resistor,034_L104_R12.fbx,034_L104_R12.glb,-0.085,0.00088,0.044,True
35,L104_R13,resistor,035_L104_R13.fbx,035_L104_R13.glb,-0.071,0.00088,0.044,True
36,L104_R14,resistor,036_L104_R14.fbx,036_L104_R14.glb,-0.054,0.00088,0.044,True
37,L104_R15,resistor,037_L104_R15.fbx,037_L104_R15.glb,-0.035,0.00088,0.044,True
38,L104_R16,resistor,038_L104_R16.fbx,038_L104_R16.glb,-0.018,0.00088,0.044,True
39,L104_R17,resistor,039_L104_R17.fbx,039_L104_R17.glb,0.002,0.00088,0.044,True
40,L104_R18,resistor,040_L104_R18.fbx,040_L104_R18.glb,0.021,0.00088,0.044,True
41,L104_R19,resistor,041_L104_R19.fbx,041_L104_R19.glb,0.039,0.00088,0.044,True
42,L104_R20,resistor,042_L104_R20.fbx,042_L104_R20.glb,0.099,0.00088,0.044,True
43,L104_R21,resistor,043_L104_R21.fbx,043_L104_R21.glb,0.112,0.00088,0.044,True
44,L104_R22,resistor,044_L104_R22.fbx,044_L104_R22.glb,-0.112,0.00088,0.029,True
45,L104_C1,capacitor,045_L104_C1.fbx,045_L104_C1.glb,-0.099,0.00088,0.029,True
46,L104_C2,capacitor,046_L104_C2.fbx,046_L104_C2.glb,-0.085,0.00088,0.029,True
47,L104_C3,capacitor,047_L104_C3.fbx,047_L104_C3.glb,-0.071,0.00088,0.029,True
48,L104_C4,capacitor,048_L104_C4.fbx,048_L104_C4.glb,-0.054,0.00088,0.029,True
49,L104_C5,capacitor,049_L104_C5.fbx,049_L104_C5.glb,-0.035,0.00088,0.029,True
50,L104_C6,capacitor,050_L104_C6.fbx,050_L104_C6.glb,-0.018,0.00088,0.029,True
51,L104_C7,capacitor,051_L104_C7.fbx,051_L104_C7.glb,0.002,0.00088,0.029,True
52,L104_C8,capacitor,052_L104_C8.fbx,052_L104_C8.glb,0.021,0.00088,0.029,True
53,L104_C9,capacitor,053_L104_C9.fbx,053_L104_C9.glb,0.039,0.00088,0.029,True
54,L104_C10,capacitor,054_L104_C10.fbx,054_L104_C10.glb,0.099,0.00088,0.029,True
55,L104_C11,capacitor,055_L104_C11.fbx,055_L104_C11.glb,0.112,0.00088,0.029,True
56,L104_C12,capacitor,056_L104_C12.fbx,056_L104_C12.glb,-0.112,0.00088,0.013,True
57,L104_C13,capacitor,057_L104_C13.fbx,057_L104_C13.glb,-0.099,0.00088,0.013,True
58,L104_C14,capacitor,058_L104_C14.fbx,058_L104_C14.glb,-0.085,0.00088,0.013,True
59,L104_C15,capacitor,059_L104_C15.fbx,059_L104_C15.glb,-0.071,0.00088,0.013,True
60,L104_C16,capacitor,060_L104_C16.fbx,060_L104_C16.glb,-0.018,0.00088,0.013,True
61,L104_C17,capacitor,061_L104_C17.fbx,061_L104_C17.glb,0.002,0.00088,0.013,True
62,L104_C18,capacitor,062_L104_C18.fbx,062_L104_C18.glb,0.021,0.00088,0.013,True
63,L104_C19,capacitor,063_L104_C19.fbx,063_L104_C19.glb,0.039,0.00088,0.013,True
64,L104_C20,capacitor,064_L104_C20.fbx,064_L104_C20.glb,0.061,0.00088,0.013,True
65,L104_C21,capacitor,065_L104_C21.fbx,065_L104_C21.glb,0.08,0.00088,0.013,True
66,L104_C22,capacitor,066_L104_C22.fbx,066_L104_C22.glb,0.099,0.00088,0.013,True
67,L104_C23,capacitor,067_L104_C23.fbx,067_L104_C23.glb,0.112,0.00088,0.013,True
68,L104_C24,capacitor,068_L104_C24.fbx,068_L104_C24.glb,-0.112,0.00088,-0.004,True
69,L104_C25,capacitor,069_L104_C25.fbx,069_L104_C25.glb,-0.099,0.00088,-0.004,True
70,L104_C26,capacitor,070_L104_C26.fbx,070_L104_C26.glb,-0.085,0.00088,-0.004,True
71,L104_C27,capacitor,071_L104_C27.fbx,071_L104_C27.glb,-0.071,0.00088,-0.004,True
72,L104_C28,capacitor,072_L104_C28.fbx,072_L104_C28.glb,-0.018,0.00088,-0.004,True
73,L104_C29,capacitor,073_L104_C29.fbx,073_L104_C29.glb,0.002,0.00088,-0.004,True
74,L104_C30,capacitor,074_L104_C30.fbx,074_L104_C30.glb,0.021,0.00088,-0.004,True
75,L104_FB1,ferrite,075_L104_FB1.fbx,075_L104_FB1.glb,0.039,0.00088,-0.004,True
76,L104_FB2,ferrite,076_L104_FB2.fbx,076_L104_FB2.glb,0.061,0.00088,-0.004,True
77,L104_FB3,ferrite,077_L104_FB3.fbx,077_L104_FB3.glb,0.08,0.00088,-0.004,True
78,L104_FB4,ferrite,078_L104_FB4.fbx,078_L104_FB4.glb,0.099,0.00088,-0.004,True
79,L104_J1,connector,079_L104_J1.fbx,079_L104_J1.glb,0.112,0.00088,-0.004,True
80,L104_J2,connector,080_L104_J2.fbx,080_L104_J2.glb,-0.112,0.00088,-0.02,True
81,L104_J3,connector,081_L104_J3.fbx,081_L104_J3.glb,-0.099,0.00088,-0.02,True
82,L104_J4,connector,082_L104_J4.fbx,082_L104_J4.glb,-0.085,0.00088,-0.02,True
83,L104_LED1,led,083_L104_LED1.fbx,083_L104_LED1.glb,-0.071,0.00088,-0.02,True
84,L104_LED2,led,084_L104_LED2.fbx,084_L104_LED2.glb,-0.054,0.00088,-0.02,True
85,L104_LED3,led,085_L104_LED3.fbx,085_L104_LED3.glb,-0.035,0.00088,-0.02,True
86,L104_LED4,led,086_L104_LED4.fbx,086_L104_LED4.glb,-0.018,0.00088,-0.02,True
87,L104_TS1,sensor,087_L104_TS1.fbx,087_L104_TS1.glb,0.002,0.00088,-0.02,True
88,L104_TS2,sensor,088_L104_TS2.fbx,088_L104_TS2.glb,0.021,0.00088,-0.02,True
89,L104_SW1,switch,089_L104_SW1.fbx,089_L104_SW1.glb,0.099,0.00088,-0.02,True
90,L104_SW2,switch,090_L104_SW2.fbx,090_L104_SW2.glb,0.112,0.00088,-0.02,True
91,L104_D1,protection,091_L104_D1.fbx,091_L104_D1.glb,-0.112,0.00088,-0.035,True
92,L104_D2,protection,092_L104_D2.fbx,092_L104_D2.glb,-0.099,0.00088,-0.035,True
93,L104_D3,protection,093_L104_D3.fbx,093_L104_D3.glb,-0.085,0.00088,-0.035,True
94,L104_W125,jumper,094_L104_W125.fbx,094_L104_W125.glb,0.103,0.00088,-0.06,True
95,PCB_Substrate_260x185mm,board,095_L104_PCB_Substrate_260x185mm.fbx,095_L104_PCB_Substrate_260x185mm.glb,0,0,0,False
96,L1_CopperReference,layer,096_L104_L1_CopperReference.fbx,096_L104_L1_CopperReference.glb,0,0,0,False
97,L2_CopperReference,layer,097_L104_L2_CopperReference.fbx,097_L104_L2_CopperReference.glb,0,0,0,False
98,L3_CopperReference,layer,098_L104_L3_CopperReference.fbx,098_L104_L3_CopperReference.glb,0,0,0,False
99,L4_CopperReference,layer,099_L104_L4_CopperReference.fbx,099_L104_L4_CopperReference.glb,0,0,0,False
100,L5_CopperReference,layer,100_L104_L5_CopperReference.fbx,100_L104_L5_CopperReference.glb,0,0,0,False
101,L6_CopperReference,layer,101_L104_L6_CopperReference.fbx,101_L104_L6_CopperReference.glb,0,0,0,False
102,L7_CopperReference,layer,102_L104_L7_CopperReference.fbx,102_L104_L7_CopperReference.glb,0,0,0,False
103,L8_CopperReference,layer,103_L104_L8_CopperReference.fbx,103_L104_L8_CopperReference.glb,0,0,0,False
104,U1_BGA_ContactFootprint,padbank,104_L104_U1_BGA_ContactFootprint.fbx,104_L104_U1_BGA_ContactFootprint.glb,0,0,0,False
105,U2_BGA_ContactFootprint,padbank,105_L104_U2_BGA_ContactFootprint.fbx,105_L104_U2_BGA_ContactFootprint.glb,0,0,0,False
106,U3_BGA_ContactFootprint,padbank,106_L104_U3_BGA_ContactFootprint.fbx,106_L104_U3_BGA_ContactFootprint.glb,0,0,0,False
107,BGA_SolderBall_InspectionArray,padbank,107_L104_BGA_SolderBall_InspectionArray.fbx,107_L104_BGA_SolderBall_InspectionArray.glb,0,0,0,False
108,BGA_Repair_ContactSet,padbank,108_L104_BGA_Repair_ContactSet.fbx,108_L104_BGA_Repair_ContactSet.glb,0,0,0,False
109,ThroughVia_Detail,via,109_L104_ThroughVia_Detail.fbx,109_L104_ThroughVia_Detail.glb,0,0,0,False
110,SignalVia_Detail,via,110_L104_SignalVia_Detail.fbx,110_L104_SignalVia_Detail.glb,0,0,0,False
111,GroundStitchVia_Detail,via,111_L104_GroundStitchVia_Detail.fbx,111_L104_GroundStitchVia_Detail.glb,0,0,0,False
112,DQS_PairedCopper,trace,112_L104_DQS_PairedCopper.fbx,112_L104_DQS_PairedCopper.glb,0,0,0,False
113,CA_ClockRoute_Detail,trace,113_L104_CA_ClockRoute_Detail.fbx,113_L104_CA_ClockRoute_Detail.glb,0,0,0,False
114,TerminationRoute_Detail,trace,114_L104_TerminationRoute_Detail.fbx,114_L104_TerminationRoute_Detail.glb,0,0,0,False
115,Fault_DQS_OpenVia,fault,115_L104_Fault_DQS_OpenVia.fbx,115_L104_Fault_DQS_OpenVia.glb,0,0,0,False
116,Fault_CA_Termination,fault,116_L104_Fault_CA_Termination.fbx,116_L104_Fault_CA_Termination.glb,0,0,0,False
117,Fault_U3_BGA_Intermittent,fault,117_L104_Fault_U3_BGA_Intermittent.fbx,117_L104_Fault_U3_BGA_Intermittent.glb,0,0,0,False
118,BGA_Rework_VisualState,fault,118_L104_BGA_Rework_VisualState.fbx,118_L104_BGA_Rework_VisualState.glb,0,0,0,False
119,MemoryBoard_SilkscreenMark,mark,119_L104_MemoryBoard_SilkscreenMark.fbx,119_L104_MemoryBoard_SilkscreenMark.glb,0,0,0,False
120,MemoryBoard_TestPointGroup,padbank,120_L104_MemoryBoard_TestPointGroup.fbx,120_L104_MemoryBoard_TestPointGroup.glb,0,0,0,False
```

#### Annex C. Component BOM — `Data/ComponentBOM.csv`

```csv
ref,kind,description
U1,controller,DDR4 memory controller BGA package
U2,ddr4,"DDR4 x16 SDRAM, byte lanes 0/1"
U3,ddr4,"DDR4 x16 SDRAM, byte lanes 2/3"
U4,ic,1.2V DDR VDD regulator
U5,ic,2.5V DDR VPP regulator
U6,ic,0.6V DDR VTT termination supply
U7,ic,Controller auxiliary regulator
U8,ic,Memory clock generator
U9,ic,Power/reset supervisor
U10,ic,Clock conditioning
RN1,termination,DDR4 command/address termination network 1
RN2,termination,DDR4 command/address termination network 2
RN3,termination,DDR4 command/address termination network 3
RN4,termination,DDR4 command/address termination network 4
RN5,termination,DDR4 command/address termination network 5
RN6,termination,DDR4 command/address termination network 6
RN7,termination,DDR4 command/address termination network 7
RN8,termination,DDR4 command/address termination network 8
RN9,termination,DDR4 command/address termination network 9
RN10,termination,DDR4 command/address termination network 10
RN11,termination,DDR4 command/address termination network 11
RN12,termination,DDR4 command/address termination network 12
R1,resistor,Precision memory resistor 1
R2,resistor,Precision memory resistor 2
R3,resistor,Precision memory resistor 3
R4,resistor,Precision memory resistor 4
R5,resistor,Precision memory resistor 5
R6,resistor,Precision memory resistor 6
R7,resistor,Precision memory resistor 7
R8,resistor,Precision memory resistor 8
R9,resistor,Precision memory resistor 9
R10,resistor,Precision memory resistor 10
R11,resistor,Precision memory resistor 11
R12,resistor,Precision memory resistor 12
R13,resistor,Precision memory resistor 13
R14,resistor,Precision memory resistor 14
R15,resistor,Precision memory resistor 15
R16,resistor,Precision memory resistor 16
R17,resistor,Precision memory resistor 17
R18,resistor,Precision memory resistor 18
R19,resistor,Precision memory resistor 19
R20,resistor,Precision memory resistor 20
R21,resistor,Precision memory resistor 21
R22,resistor,Precision memory resistor 22
C1,capacitor,Memory decoupling/filter capacitor 1
C2,capacitor,Memory decoupling/filter capacitor 2
C3,capacitor,Memory decoupling/filter capacitor 3
C4,capacitor,Memory decoupling/filter capacitor 4
C5,capacitor,Memory decoupling/filter capacitor 5
C6,capacitor,Memory decoupling/filter capacitor 6
C7,capacitor,Memory decoupling/filter capacitor 7
C8,capacitor,Memory decoupling/filter capacitor 8
C9,capacitor,Memory decoupling/filter capacitor 9
C10,capacitor,Memory decoupling/filter capacitor 10
C11,capacitor,Memory decoupling/filter capacitor 11
C12,capacitor,Memory decoupling/filter capacitor 12
C13,capacitor,Memory decoupling/filter capacitor 13
C14,capacitor,Memory decoupling/filter capacitor 14
C15,capacitor,Memory decoupling/filter capacitor 15
C16,capacitor,Memory decoupling/filter capacitor 16
C17,capacitor,Memory decoupling/filter capacitor 17
C18,capacitor,Memory decoupling/filter capacitor 18
C19,capacitor,Memory decoupling/filter capacitor 19
C20,capacitor,Memory decoupling/filter capacitor 20
C21,capacitor,Memory decoupling/filter capacitor 21
C22,capacitor,Memory decoupling/filter capacitor 22
C23,capacitor,Memory decoupling/filter capacitor 23
C24,capacitor,Memory decoupling/filter capacitor 24
C25,capacitor,Memory decoupling/filter capacitor 25
C26,capacitor,Memory decoupling/filter capacitor 26
C27,capacitor,Memory decoupling/filter capacitor 27
C28,capacitor,Memory decoupling/filter capacitor 28
C29,capacitor,Memory decoupling/filter capacitor 29
C30,capacitor,Memory decoupling/filter capacitor 30
FB1,ferrite,Rail filtering ferrite 1
FB2,ferrite,Rail filtering ferrite 2
FB3,ferrite,Rail filtering ferrite 3
FB4,ferrite,Rail filtering ferrite 4
J1,connector,Debug/power interface connector 1
J2,connector,Debug/power interface connector 2
J3,connector,Debug/power interface connector 3
J4,connector,Debug/power interface connector 4
LED1,led,Memory qualification indicator 1
LED2,led,Memory qualification indicator 2
LED3,led,Memory qualification indicator 3
LED4,led,Memory qualification indicator 4
TS1,sensor,Memory thermal sensor 1
TS2,sensor,Memory thermal sensor 2
SW1,switch,Reset/configuration switch 1
SW2,switch,Reset/configuration switch 2
D1,protection,Power protection device 1
D2,protection,Power protection device 2
D3,protection,Power protection device 3
W125,jumper,Detachable trusted memory-ready jumper
```

#### Annex D. Component Placement Coordinates — `Data/ComponentPlacements.csv`

```csv
ref,x_m,y_m,z_m,placed_at_start,footprint_verified
U1,-0.048,0.00088,0.005,False,False
U2,0.058,0.00088,0.036,False,False
U3,0.058,0.00088,-0.037,False,False
U4,-0.104,0.00088,0.065,False,False
U5,-0.082,0.00088,0.065,False,False
U6,-0.103,0.00088,-0.059,False,False
U7,-0.082,0.00088,-0.059,False,False
U8,-0.058,0.00088,0.063,False,False
U9,-0.04,0.00088,0.063,False,False
U10,-0.02,0.00088,0.063,False,False
RN1,-0.112,0.00088,0.072,False,False
RN2,-0.099,0.00088,0.072,False,False
RN3,-0.071,0.00088,0.072,False,False
RN4,-0.054,0.00088,0.072,False,False
RN5,-0.035,0.00088,0.072,False,False
RN6,-0.018,0.00088,0.072,False,False
RN7,0.002,0.00088,0.072,False,False
RN8,0.021,0.00088,0.072,False,False
RN9,0.039,0.00088,0.072,False,False
RN10,0.061,0.00088,0.072,False,False
RN11,0.08,0.00088,0.072,False,False
RN12,0.099,0.00088,0.072,False,False
R1,0.112,0.00088,0.072,False,False
R2,-0.112,0.00088,0.058,False,False
R3,-0.099,0.00088,0.058,False,False
R4,-0.071,0.00088,0.058,False,False
R5,0.002,0.00088,0.058,False,False
R6,0.021,0.00088,0.058,False,False
R7,0.039,0.00088,0.058,False,False
R8,0.099,0.00088,0.058,False,False
R9,0.112,0.00088,0.058,False,False
R10,-0.112,0.00088,0.044,False,False
R11,-0.099,0.00088,0.044,False,False
R12,-0.085,0.00088,0.044,False,False
R13,-0.071,0.00088,0.044,False,False
R14,-0.054,0.00088,0.044,False,False
R15,-0.035,0.00088,0.044,False,False
R16,-0.018,0.00088,0.044,False,False
R17,0.002,0.00088,0.044,False,False
R18,0.021,0.00088,0.044,False,False
R19,0.039,0.00088,0.044,False,False
R20,0.099,0.00088,0.044,False,False
R21,0.112,0.00088,0.044,False,False
R22,-0.112,0.00088,0.029,False,False
C1,-0.099,0.00088,0.029,False,False
C2,-0.085,0.00088,0.029,False,False
C3,-0.071,0.00088,0.029,False,False
C4,-0.054,0.00088,0.029,False,False
C5,-0.035,0.00088,0.029,False,False
C6,-0.018,0.00088,0.029,False,False
C7,0.002,0.00088,0.029,False,False
C8,0.021,0.00088,0.029,False,False
C9,0.039,0.00088,0.029,False,False
C10,0.099,0.00088,0.029,False,False
C11,0.112,0.00088,0.029,False,False
C12,-0.112,0.00088,0.013,False,False
C13,-0.099,0.00088,0.013,False,False
C14,-0.085,0.00088,0.013,False,False
C15,-0.071,0.00088,0.013,False,False
C16,-0.018,0.00088,0.013,False,False
C17,0.002,0.00088,0.013,False,False
C18,0.021,0.00088,0.013,False,False
C19,0.039,0.00088,0.013,False,False
C20,0.061,0.00088,0.013,False,False
C21,0.08,0.00088,0.013,False,False
C22,0.099,0.00088,0.013,False,False
C23,0.112,0.00088,0.013,False,False
C24,-0.112,0.00088,-0.004,False,False
C25,-0.099,0.00088,-0.004,False,False
C26,-0.085,0.00088,-0.004,False,False
C27,-0.071,0.00088,-0.004,False,False
C28,-0.018,0.00088,-0.004,False,False
C29,0.002,0.00088,-0.004,False,False
C30,0.021,0.00088,-0.004,False,False
FB1,0.039,0.00088,-0.004,False,False
FB2,0.061,0.00088,-0.004,False,False
FB3,0.08,0.00088,-0.004,False,False
FB4,0.099,0.00088,-0.004,False,False
J1,0.112,0.00088,-0.004,False,False
J2,-0.112,0.00088,-0.02,False,False
J3,-0.099,0.00088,-0.02,False,False
J4,-0.085,0.00088,-0.02,False,False
LED1,-0.071,0.00088,-0.02,False,False
LED2,-0.054,0.00088,-0.02,False,False
LED3,-0.035,0.00088,-0.02,False,False
LED4,-0.018,0.00088,-0.02,False,False
TS1,0.002,0.00088,-0.02,False,False
TS2,0.021,0.00088,-0.02,False,False
SW1,0.099,0.00088,-0.02,False,False
SW2,0.112,0.00088,-0.02,False,False
D1,-0.112,0.00088,-0.035,False,False
D2,-0.099,0.00088,-0.035,False,False
D3,-0.085,0.00088,-0.035,False,False
W125,0.103,0.00088,-0.06,False,False
```

#### Annex E. 124 Copper Route Tasks — UNVERIFIED — `Data/CopperRoutes_124_REQUIRES_SCHEMATIC.csv`

```csv
id,domain,source_pad,dest_pad,net_id,certifiable
W001,Power,UNASSIGNED,UNASSIGNED,UNASSIGNED,False
W002,Power,UNASSIGNED,UNASSIGNED,UNASSIGNED,False
W003,Power,UNASSIGNED,UNASSIGNED,UNASSIGNED,False
W004,Power,UNASSIGNED,UNASSIGNED,UNASSIGNED,False
W005,Power,UNASSIGNED,UNASSIGNED,UNASSIGNED,False
W006,Power,UNASSIGNED,UNASSIGNED,UNASSIGNED,False
W007,Power,UNASSIGNED,UNASSIGNED,UNASSIGNED,False
W008,Power,UNASSIGNED,UNASSIGNED,UNASSIGNED,False
W009,Power,UNASSIGNED,UNASSIGNED,UNASSIGNED,False
W010,Power,UNASSIGNED,UNASSIGNED,UNASSIGNED,False
W011,Power,UNASSIGNED,UNASSIGNED,UNASSIGNED,False
W012,Power,UNASSIGNED,UNASSIGNED,UNASSIGNED,False
W013,Power,UNASSIGNED,UNASSIGNED,UNASSIGNED,False
W014,Power,UNASSIGNED,UNASSIGNED,UNASSIGNED,False
W015,Power,UNASSIGNED,UNASSIGNED,UNASSIGNED,False
W016,Power,UNASSIGNED,UNASSIGNED,UNASSIGNED,False
W017,Power,UNASSIGNED,UNASSIGNED,UNASSIGNED,False
W018,Power,UNASSIGNED,UNASSIGNED,UNASSIGNED,False
W019,Clock_Command_Address,UNASSIGNED,UNASSIGNED,UNASSIGNED,False
W020,Clock_Command_Address,UNASSIGNED,UNASSIGNED,UNASSIGNED,False
W021,Clock_Command_Address,UNASSIGNED,UNASSIGNED,UNASSIGNED,False
W022,Clock_Command_Address,UNASSIGNED,UNASSIGNED,UNASSIGNED,False
W023,Clock_Command_Address,UNASSIGNED,UNASSIGNED,UNASSIGNED,False
W024,Clock_Command_Address,UNASSIGNED,UNASSIGNED,UNASSIGNED,False
W025,Clock_Command_Address,UNASSIGNED,UNASSIGNED,UNASSIGNED,False
W026,Clock_Command_Address,UNASSIGNED,UNASSIGNED,UNASSIGNED,False
W027,Clock_Command_Address,UNASSIGNED,UNASSIGNED,UNASSIGNED,False
W028,Clock_Command_Address,UNASSIGNED,UNASSIGNED,UNASSIGNED,False
W029,Clock_Command_Address,UNASSIGNED,UNASSIGNED,UNASSIGNED,False
W030,Clock_Command_Address,UNASSIGNED,UNASSIGNED,UNASSIGNED,False
W031,Clock_Command_Address,UNASSIGNED,UNASSIGNED,UNASSIGNED,False
W032,Clock_Command_Address,UNASSIGNED,UNASSIGNED,UNASSIGNED,False
W033,Clock_Command_Address,UNASSIGNED,UNASSIGNED,UNASSIGNED,False
W034,Clock_Command_Address,UNASSIGNED,UNASSIGNED,UNASSIGNED,False
W035,Clock_Command_Address,UNASSIGNED,UNASSIGNED,UNASSIGNED,False
W036,Clock_Command_Address,UNASSIGNED,UNASSIGNED,UNASSIGNED,False
W037,Clock_Command_Address,UNASSIGNED,UNASSIGNED,UNASSIGNED,False
W038,Clock_Command_Address,UNASSIGNED,UNASSIGNED,UNASSIGNED,False
W039,ByteLanes_01,UNASSIGNED,UNASSIGNED,UNASSIGNED,False
W040,ByteLanes_01,UNASSIGNED,UNASSIGNED,UNASSIGNED,False
W041,ByteLanes_01,UNASSIGNED,UNASSIGNED,UNASSIGNED,False
W042,ByteLanes_01,UNASSIGNED,UNASSIGNED,UNASSIGNED,False
W043,ByteLanes_01,UNASSIGNED,UNASSIGNED,UNASSIGNED,False
W044,ByteLanes_01,UNASSIGNED,UNASSIGNED,UNASSIGNED,False
W045,ByteLanes_01,UNASSIGNED,UNASSIGNED,UNASSIGNED,False
W046,ByteLanes_01,UNASSIGNED,UNASSIGNED,UNASSIGNED,False
W047,ByteLanes_01,UNASSIGNED,UNASSIGNED,UNASSIGNED,False
W048,ByteLanes_01,UNASSIGNED,UNASSIGNED,UNASSIGNED,False
W049,ByteLanes_01,UNASSIGNED,UNASSIGNED,UNASSIGNED,False
W050,ByteLanes_01,UNASSIGNED,UNASSIGNED,UNASSIGNED,False
W051,ByteLanes_01,UNASSIGNED,UNASSIGNED,UNASSIGNED,False
W052,ByteLanes_01,UNASSIGNED,UNASSIGNED,UNASSIGNED,False
W053,ByteLanes_01,UNASSIGNED,UNASSIGNED,UNASSIGNED,False
W054,ByteLanes_01,UNASSIGNED,UNASSIGNED,UNASSIGNED,False
W055,ByteLanes_01,UNASSIGNED,UNASSIGNED,UNASSIGNED,False
W056,ByteLanes_01,UNASSIGNED,UNASSIGNED,UNASSIGNED,False
W057,ByteLanes_01,UNASSIGNED,UNASSIGNED,UNASSIGNED,False
W058,ByteLanes_01,UNASSIGNED,UNASSIGNED,UNASSIGNED,False
W059,ByteLanes_01,UNASSIGNED,UNASSIGNED,UNASSIGNED,False
W060,ByteLanes_01,UNASSIGNED,UNASSIGNED,UNASSIGNED,False
W061,ByteLanes_01,UNASSIGNED,UNASSIGNED,UNASSIGNED,False
W062,ByteLanes_01,UNASSIGNED,UNASSIGNED,UNASSIGNED,False
W063,ByteLanes_23,UNASSIGNED,UNASSIGNED,UNASSIGNED,False
W064,ByteLanes_23,UNASSIGNED,UNASSIGNED,UNASSIGNED,False
W065,ByteLanes_23,UNASSIGNED,UNASSIGNED,UNASSIGNED,False
W066,ByteLanes_23,UNASSIGNED,UNASSIGNED,UNASSIGNED,False
W067,ByteLanes_23,UNASSIGNED,UNASSIGNED,UNASSIGNED,False
W068,ByteLanes_23,UNASSIGNED,UNASSIGNED,UNASSIGNED,False
W069,ByteLanes_23,UNASSIGNED,UNASSIGNED,UNASSIGNED,False
W070,ByteLanes_23,UNASSIGNED,UNASSIGNED,UNASSIGNED,False
W071,ByteLanes_23,UNASSIGNED,UNASSIGNED,UNASSIGNED,False
W072,ByteLanes_23,UNASSIGNED,UNASSIGNED,UNASSIGNED,False
W073,ByteLanes_23,UNASSIGNED,UNASSIGNED,UNASSIGNED,False
W074,ByteLanes_23,UNASSIGNED,UNASSIGNED,UNASSIGNED,False
W075,ByteLanes_23,UNASSIGNED,UNASSIGNED,UNASSIGNED,False
W076,ByteLanes_23,UNASSIGNED,UNASSIGNED,UNASSIGNED,False
W077,ByteLanes_23,UNASSIGNED,UNASSIGNED,UNASSIGNED,False
W078,ByteLanes_23,UNASSIGNED,UNASSIGNED,UNASSIGNED,False
W079,ByteLanes_23,UNASSIGNED,UNASSIGNED,UNASSIGNED,False
W080,ByteLanes_23,UNASSIGNED,UNASSIGNED,UNASSIGNED,False
W081,ByteLanes_23,UNASSIGNED,UNASSIGNED,UNASSIGNED,False
W082,ByteLanes_23,UNASSIGNED,UNASSIGNED,UNASSIGNED,False
W083,ByteLanes_23,UNASSIGNED,UNASSIGNED,UNASSIGNED,False
W084,ByteLanes_23,UNASSIGNED,UNASSIGNED,UNASSIGNED,False
W085,ByteLanes_23,UNASSIGNED,UNASSIGNED,UNASSIGNED,False
W086,ByteLanes_23,UNASSIGNED,UNASSIGNED,UNASSIGNED,False
W087,DQS_Termination_Reference,UNASSIGNED,UNASSIGNED,UNASSIGNED,False
W088,DQS_Termination_Reference,UNASSIGNED,UNASSIGNED,UNASSIGNED,False
W089,DQS_Termination_Reference,UNASSIGNED,UNASSIGNED,UNASSIGNED,False
W090,DQS_Termination_Reference,UNASSIGNED,UNASSIGNED,UNASSIGNED,False
W091,DQS_Termination_Reference,UNASSIGNED,UNASSIGNED,UNASSIGNED,False
W092,DQS_Termination_Reference,UNASSIGNED,UNASSIGNED,UNASSIGNED,False
W093,DQS_Termination_Reference,UNASSIGNED,UNASSIGNED,UNASSIGNED,False
W094,DQS_Termination_Reference,UNASSIGNED,UNASSIGNED,UNASSIGNED,False
W095,DQS_Termination_Reference,UNASSIGNED,UNASSIGNED,UNASSIGNED,False
W096,DQS_Termination_Reference,UNASSIGNED,UNASSIGNED,UNASSIGNED,False
W097,DQS_Termination_Reference,UNASSIGNED,UNASSIGNED,UNASSIGNED,False
W098,DQS_Termination_Reference,UNASSIGNED,UNASSIGNED,UNASSIGNED,False
W099,DQS_Termination_Reference,UNASSIGNED,UNASSIGNED,UNASSIGNED,False
W100,DQS_Termination_Reference,UNASSIGNED,UNASSIGNED,UNASSIGNED,False
W101,DQS_Termination_Reference,UNASSIGNED,UNASSIGNED,UNASSIGNED,False
W102,DQS_Termination_Reference,UNASSIGNED,UNASSIGNED,UNASSIGNED,False
W103,DQS_Termination_Reference,UNASSIGNED,UNASSIGNED,UNASSIGNED,False
W104,DQS_Termination_Reference,UNASSIGNED,UNASSIGNED,UNASSIGNED,False
W105,Training_Telemetry_Safety,UNASSIGNED,UNASSIGNED,UNASSIGNED,False
W106,Training_Telemetry_Safety,UNASSIGNED,UNASSIGNED,UNASSIGNED,False
W107,Training_Telemetry_Safety,UNASSIGNED,UNASSIGNED,UNASSIGNED,False
W108,Training_Telemetry_Safety,UNASSIGNED,UNASSIGNED,UNASSIGNED,False
W109,Training_Telemetry_Safety,UNASSIGNED,UNASSIGNED,UNASSIGNED,False
W110,Training_Telemetry_Safety,UNASSIGNED,UNASSIGNED,UNASSIGNED,False
W111,Training_Telemetry_Safety,UNASSIGNED,UNASSIGNED,UNASSIGNED,False
W112,Training_Telemetry_Safety,UNASSIGNED,UNASSIGNED,UNASSIGNED,False
W113,Training_Telemetry_Safety,UNASSIGNED,UNASSIGNED,UNASSIGNED,False
W114,Training_Telemetry_Safety,UNASSIGNED,UNASSIGNED,UNASSIGNED,False
W115,Training_Telemetry_Safety,UNASSIGNED,UNASSIGNED,UNASSIGNED,False
W116,Training_Telemetry_Safety,UNASSIGNED,UNASSIGNED,UNASSIGNED,False
W117,Training_Telemetry_Safety,UNASSIGNED,UNASSIGNED,UNASSIGNED,False
W118,Training_Telemetry_Safety,UNASSIGNED,UNASSIGNED,UNASSIGNED,False
W119,Training_Telemetry_Safety,UNASSIGNED,UNASSIGNED,UNASSIGNED,False
W120,Training_Telemetry_Safety,UNASSIGNED,UNASSIGNED,UNASSIGNED,False
W121,Training_Telemetry_Safety,UNASSIGNED,UNASSIGNED,UNASSIGNED,False
W122,Training_Telemetry_Safety,UNASSIGNED,UNASSIGNED,UNASSIGNED,False
W123,Training_Telemetry_Safety,UNASSIGNED,UNASSIGNED,UNASSIGNED,False
W124,Training_Telemetry_Safety,UNASSIGNED,UNASSIGNED,UNASSIGNED,False
```

#### Annex F. 34 Via Tasks — UNVERIFIED — `Data/ViaTasks_34_REQUIRES_SCHEMATIC.csv`

```csv
id,start_layer,end_layer,net_id,verified
V01,TBD,TBD,UNASSIGNED,False
V02,TBD,TBD,UNASSIGNED,False
V03,TBD,TBD,UNASSIGNED,False
V04,TBD,TBD,UNASSIGNED,False
V05,TBD,TBD,UNASSIGNED,False
V06,TBD,TBD,UNASSIGNED,False
V07,TBD,TBD,UNASSIGNED,False
V08,TBD,TBD,UNASSIGNED,False
V09,TBD,TBD,UNASSIGNED,False
V10,TBD,TBD,UNASSIGNED,False
V11,TBD,TBD,UNASSIGNED,False
V12,TBD,TBD,UNASSIGNED,False
V13,TBD,TBD,UNASSIGNED,False
V14,TBD,TBD,UNASSIGNED,False
V15,TBD,TBD,UNASSIGNED,False
V16,TBD,TBD,UNASSIGNED,False
V17,TBD,TBD,UNASSIGNED,False
V18,TBD,TBD,UNASSIGNED,False
V19,TBD,TBD,UNASSIGNED,False
V20,TBD,TBD,UNASSIGNED,False
V21,TBD,TBD,UNASSIGNED,False
V22,TBD,TBD,UNASSIGNED,False
V23,TBD,TBD,UNASSIGNED,False
V24,TBD,TBD,UNASSIGNED,False
V25,TBD,TBD,UNASSIGNED,False
V26,TBD,TBD,UNASSIGNED,False
V27,TBD,TBD,UNASSIGNED,False
V28,TBD,TBD,UNASSIGNED,False
V29,TBD,TBD,UNASSIGNED,False
V30,TBD,TBD,UNASSIGNED,False
V31,TBD,TBD,UNASSIGNED,False
V32,TBD,TBD,UNASSIGNED,False
V33,TBD,TBD,UNASSIGNED,False
V34,TBD,TBD,UNASSIGNED,False
```

#### Annex G. Level Objectives and Initial State — `Data/Objectives.json`

```json
{
  "level": 104,
  "name": "MEMORY MATRIX",
  "model": "MMX-12",
  "objectives": [
    {
      "id": 1,
      "name": "Install 94 components"
    },
    {
      "id": 2,
      "name": "Complete 124 copper routes and 34 vias"
    },
    {
      "id": 3,
      "name": "Commission memory supplies and initialization"
    },
    {
      "id": 4,
      "name": "Train 4 byte lanes and verify DDR4 data"
    },
    {
      "id": 5,
      "name": "Repair 3 faults and requalify"
    },
    {
      "id": 6,
      "name": "Disconnect and recover W125 with fresh tests"
    }
  ],
  "initialState": {
    "installed": 0,
    "routesCompleted": 0,
    "viasInstalled": 0,
    "W125": "OPEN"
  },
  "authoritativeCompletion": false
}
```

#### Annex H. Recoverable Fault Plan — `Data/FaultPlan.json`

```json
{
  "faults": [
    {
      "id": "F1",
      "title": "DQS strobe via open",
      "target": "Byte lane 2 DQS connection",
      "repair": "verified layer-aware via rework"
    },
    {
      "id": "F2",
      "title": "Command/address termination failure",
      "target": "Termination resistor array",
      "repair": "replace value-correct component"
    },
    {
      "id": "F3",
      "title": "U3 intermittent BGA contact",
      "target": "U3 BGA ball grid",
      "repair": "qualified BGA rework or device replacement"
    }
  ]
}
```

#### Annex I. W125 Recovery and Endpoint Mismatch — `Data/W125_Recovery.json`

```json
{
  "id": "W125",
  "status": "SCHEMATIC_VALIDATION_REQUIRED",
  "source": "L104_MemoryTrainingMonitor_U11.TRUSTED_MEMORY_READY_OUT",
  "destination": "L104_MemorySafetySupervisor_U12.TRUSTED_MEMORY_READY_IN",
  "warning": "U11/U12 are logical endpoints from gameplay design; physical component BOM and actual package pins must be reconciled.",
  "initial": "OPEN",
  "monitor_requires_fresh_sequence": true,
  "recovery": [
    "reconnect physical path",
    "retrain all byte lanes",
    "run fresh data test",
    "explicit safety reset",
    "new qualification"
  ]
}
```

#### Annex J. Existing Visual-Only Runtime Script — `Unity/Runtime/SparkL104VisualOnly.cs`

```csharp
using UnityEngine; namespace ProjectSpark.Level104 { public sealed class SparkL104VisualOnly : MonoBehaviour { [TextArea] public string notice="ART ONLY: does not certify PCB continuity, soldering, memory training, or LevelGamePlayManager objectives."; }}
```

#### Annex K. Existing Empty Board Art Builder — `Unity/Editor/SparkL104BoardArtBuilder.cs`

```csharp
#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;
namespace ProjectSpark.Level104.Editor {
 public static class SparkL104BoardArtBuilder {
  const string Menu="Tools/Project Spark/Levels/104/";
  static string Find(string filename) {
   foreach (string g in AssetDatabase.FindAssets(Path.GetFileNameWithoutExtension(filename))) {
    string p=AssetDatabase.GUIDToAssetPath(g);
    if (p.EndsWith(filename, StringComparison.OrdinalIgnoreCase)) return p;
   }
   return null;
  }
  [MenuItem(Menu+"1. Build EMPTY MMX-12 Board")]
  public static void Build() {
   GameObject root=new GameObject("L104_MMX12_EmptyGameplayBoard");
   string name="095_L104_PCB_Substrate_260x185mm.fbx";
   string path=Find(name);
   if (path!=null) { var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(path); if(prefab!=null){var go=(GameObject)PrefabUtility.InstantiatePrefab(prefab);go.transform.SetParent(root.transform,false);go.name="PCB_VISUAL_ONLY";} }
   else Debug.LogError("[L104] Cannot find "+name+". Import Models_FBX folder first.");
   root.AddComponent<SparkL104VisualOnly>();
   Selection.activeGameObject=root;
   Debug.Log("[L104] Visual board created. Components, routes, vias and W125 are NOT automatically installed or certified.");
  }
  [MenuItem(Menu+"2. Show Assembled ART Reference")]
  public static void Reference() {
   string p=Find("121_L104_AssembledReference.fbx");
   if (p==null) {Debug.LogError("L104 reference not found.");return;}
   var go=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(p));
   go.name="L104_REFERENCE_DO_NOT_USE_FOR_GAMEPLAY";Selection.activeGameObject=go;
  }
 }
}
#endif
```

#### Annex L. Existing Starter Material Installer — `Unity/Editor/SparkL104MaterialStarter.cs`

```csharp
#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;
namespace ProjectSpark.Level104.Editor {
 public static class SparkL104MaterialStarter {
  [MenuItem("Tools/Project Spark/Levels/104/3. Create PBR Starter Materials")]
  static void Build() {
   string[] ids={"pcb","mask","copper","gold","silver","tin","black","chip","ceramic","resistor","green","red","white","core","dielectric","solder"};
   string texRoot=null;
   foreach(var guid in AssetDatabase.FindAssets("pcb_Albedo t:Texture2D")) {
    var p=AssetDatabase.GUIDToAssetPath(guid);
    if(p.Contains("Level104") || p.Contains("MemoryMatrix")){texRoot=Path.GetDirectoryName(p).Replace('\\','/');break;}
   }
   if (texRoot==null){Debug.LogError("[L104] Missing textures. Import complete kit, then retry.");return;}
   var outDir=Path.GetDirectoryName(texRoot).Replace('\\','/')+"/UnityGeneratedMaterials";
   if(!AssetDatabase.IsValidFolder(outDir))AssetDatabase.CreateFolder(Path.GetDirectoryName(outDir).Replace('\\','/'),"UnityGeneratedMaterials");
   foreach(var id in ids){
    var shader=Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
    if(shader==null)continue;
    var mat=new Material(shader); mat.name="L104_"+id;
    var albedo=AssetDatabase.LoadAssetAtPath<Texture2D>(texRoot+"/"+id+"_Albedo.png");
    if(mat.HasProperty("_BaseMap"))mat.SetTexture("_BaseMap",albedo);
    if(mat.HasProperty("_MainTex"))mat.SetTexture("_MainTex",albedo);
    if(mat.HasProperty("_Metallic"))mat.SetFloat("_Metallic",(id=="gold"||id=="silver"||id=="tin"||id=="copper"||id=="solder") ? 0.85f : 0f);
    if(mat.HasProperty("_Smoothness"))mat.SetFloat("_Smoothness",0.48f);
    AssetDatabase.CreateAsset(mat,outDir+"/L104_"+id+".mat");
   }
   AssetDatabase.SaveAssets();Debug.Log("[L104] Starter materials made. Verify and remap FBX material slots manually; metallic/roughness maps require pipeline-specific packing.");
  }
 }
}
#endif
```

### 23. Developer response contract

When implementing, return: (1) a list of actual files created/changed; (2) exact Unity menu command and installation procedure; (3) one-to-one mapping of every source FBX to a prefab or deliberate excluded decorative role; (4) schematic resolution report including U11/U12; (5) list of schematic/route/via blockers; (6) Unity compile/test logs and failed tests; (7) actual measured demo/playable results; and (8) screenshot or actual evidence of true 6/6 only if observed in Unity. Never claim source-art verification is equivalent to electrical gameplay verification. **Do not modify prior Levels 1–103.**
