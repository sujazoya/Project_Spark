# PROJECT SPARK — LEVEL 100: DIGITAL NEXUS
## MASTER UNITY IMPLEMENTATION PROMPT

**Scope:** Board-only, realistic 3D art integration, live PCB assembly/routing, I²C/SPI/UART, W093 recovery, six objective certification.  
**Source:** Level 100 DNX-8 board-only realistic package, inspected directly.  
**Status:** This is a build specification. No claim that electrical simulation or Play Mode is already complete.

**Prompt to implementation agent:** Follow every section below, adapt to the actual project codebase and use the appended source annexes as provenance. Do not fake netlist validity, instrument data, repairs or objectives. Preserve all Levels 1–99.

---
## 01. Purpose, scope and inviolable constraints

You are implementing **PROJECT SPARK — LEVEL 100: DIGITAL NEXUS (DNX-8)** inside the user's **existing** Unity project, not creating a standalone imitation. Use the supplied real Level 100 FBX/GLB kit, source manifests, and established Project Spark APIs. The goal is a genuinely playable, educational multi-chip digital-communication board with authoritative electrical state, firmware behavior and six objectives.

**Do not claim compilation, Play Mode, electrical correctness or 6/6 until tested in the project.** First inspect source classes and level setup for Levels 94–99. Preserve the current level catalog, save format, object shelf, shop, interaction system, `SparkTerminal`, existing circuit solver, and `LevelGamePlayManager` as found. If an expected API is absent or differs, adapt to the real API rather than inventing an incompatible parallel manager. Keep all modifications local and backward compatible. No new room, table, physical multimeter, oscilloscope, debugger, soldering iron or tool-rack FBXs; the gameplay is **board-only** with virtual instruments on the existing world-space Canvas.

The supplied models are source art, **not** functional electrical connections. Avoid using world-space visual scale as physical board dimensions. No fabricated live results, no test counters incremented by visuals, no fake connected factory copper, and no bypass of missing tasks.

## 02. Verified source package and limitations

Actual source package: `ProjectSpark_Level100_DigitalNexus_BoardOnly_Realistic_Complete.zip` with the `ProjectSpark_Level100_DigitalNexus_BoardOnly_Realistic/` source folder. Source README lists **88 modular FBXs + 88 GLBs**, a separate completed showcase pair, **70 individual parts + 18 board details**, physical board **210 × 145 mm**, **four layers**, **92 conceptual routes**, **20 conceptual vias**, and removable **W093**. The two provided Unity Editor scripts import *art* and generate basic native materials; neither creates circuit logic, working protocols, player-routing controls or objective certification.

The route table contains literal `UNASSIGNED` from-pad, to-pad, net and layer values; the via table similarly has unassigned nets/layers and TBD coordinates. These are deliberately unverified. They are included verbatim below only as provenance and task-ID reservations. **No automated system may auto-mark these verified.** Select a real MCU and peripheral set, and author a signed-off pin-level schematic/netlist and footprint mapping as a precondition of electrical tests.

The source PBR textures are procedural references and the custom binary FBXs have not been proven to import cleanly into Unity. Review UVs, units, material assignments, normals, collision proxies and render pipeline settings. Do not state production AAA readiness merely because files exist.

## 03. Level 100 success conditions

A level is complete only when all of these are true: (a) 70 individual components were intentionally installed and verified; (b) each of 92 required player copper tasks and 20 vias was built through legal target interactions and is electrically meaningful under a verified pin-level netlist; (c) the MCU powers, clocks, resets and runs approved deterministic firmware; (d) all three I²C peripherals and two SPI peripherals pass discovery and data exchange, and UART passes bidirectional diagnostics; (e) three injected faults have been actually diagnosed and repaired with fresh test evidence; (f) removing and reinstalling **W093** causes an independent safety response followed by fresh recovery certification; and (g) the existing `LevelGamePlayManager` reports exactly **6/6** from the real objective evaluator.

Counters are display-only reflections of authoritative state, never sources of truth. A finished reference board may be displayed in an art gallery but **must never be parented beneath the playable board in a way that creates functional terminals, colliders or components**.

## 04. Board and assets

Create `Level100_DNX8_Empty_Playable_Board` using model `071_L100_DNX8_Empty_FourLayer_PCB.fbx`. This is a 0.210 × 0.145 m real-scale board (the art's local thickness and orientation must be verified in Unity). Use the source `Asset_Manifest_88.csv` and `Component_Placement_70.csv` for reference designators, model names, mm positions and Z-axis orientation. Models `001`–`070` are **uninstalled inventory**, not mounted parts. Models `071`–`088` are PCB/layer/via/test-pad/repair/fault visual helpers. Render layer references as inspection overlays, not conducting short circuits. Detect mesh-only duplicate completed-board reference imports and isolate them from playable scenes.

Install components through a player interaction transaction: select inventory item → hover compatible footprint → preview orientation/pin one → validate supply/footprint/polarity → attach art/pin contact bindings → update a component-installed record. Store all positions in the board's millimeter coordinate system, with a single explicit mm-to-m converter. Avoid `FindObjectOfType` polling per frame and avoid spontaneous installation due to scene instantiation.

## 05. Exact source BOM and canonical identity policy

The 70-component BOM and component placement CSV are canonical for **art asset identities and locations**, but **not for schematic pins or device capabilities**. Each installable gets an immutable `ComponentId`, `ReferenceDesignator`, `PrefabAssetGUID`, `FootprintId`, `PinMappingVersion`, `PoseMm`, polarity/orientation rules and allowed repair procedures. W093 is already one of the 70 installable items; do not create a 71st counted jumper. A detected mismatch between BOM and placement records is a build-blocking error; never silently fill missing components. Model names and placement table are provided in full in Annexes A–C.

## 06. Pin-specific schematic reconciliation gate

Before gameplay certification, create an approved design document and machine-readable `DNX8_VerifiedNetlist.json` containing: exact MCU and device part numbers, package drawings, pin numbers, alternate-function mux, ground/power pins, decoupling, crystal/reset, I²C pullups, SPI pin directions, UART levels, supervisor input, independent diagnostic supply, ESD/protection, connectors and authorized load paths.

Define immutable `NetId`, `PadId`, `TerminalId`, `ComponentPinId`, copper `LayerId` and junction semantics. Each original W001–W092 row must be mapped to exactly one required route with verified source pad, destination pad, net ID and allowed layer(s). Each V01–V20 must receive a coordinate and valid layer span/net with drill and annular-ring clearances. Approval must reject duplicate shorts, unconnected required pins, routes assigned to different nets, illegal layer transitions, invalid pin directions, unsafe supply crossing and hidden factory-net bypass. Preserve original CSVs as unmodified historical source files; write separately versioned **verified** files. Until verified, show a clear editor blocker and keep route certification disabled.

## 07. Four-layer topology

Use four physical copper layers: **L1 top signal, L2 ground plane, L3 3.3 V power plane, L4 bottom signal** as described by the reference art. Confirm actual board stack-up and isolation cutouts from schematic design. Maintain dedicated plane-region identities and through-hole/via span semantics; don't electrically connect two crossing traces on separate layers without a proper via/contact. Model factory traces as explicit validated edges that cannot satisfy designated missing player-required edges by accidental alternate paths. If a route is redundant at circuit level, add task-specific authorship evidence and explain it; never fake necessity through an arbitrary counter.

## 08. Component installation and soldering

The existing virtual PCB tool palette is the control surface. Allow mouse/touch/raycast selection of footprint, pad, trace and via through collider targets or world-space UI binding. Component placement requires correct part, orientation and electrical ratings. Model bad solder joint, open contact, bent pin and wrong-value part as actual component/contact state; use VFX as a visual effect only. Click/touch solder or desolder applies an asynchronous-looking but synchronously validated action transaction: authorization → affected copper/component snapshot → operation completion → circuit graph rebuild/re-solve → refreshed instrument reading. Reject unsafe hot repair when the simulation requires power-off, and provide UI feedback explaining why. Never certify merely on an animation timeline.

## 09. Player copper routes and vias

Show 92 explicit task IDs **W001–W092** in order with source/destination designators, net identity and layer guidance only after approval. Click/touch first pad and target pad, choose copper layer and route segments; validate spacing, trace width, layer and bus rules, and forbid illegal shorts. Keep 20 vias separately installable. The 92 task IDs are **not physical loose wires**; they represent player-repaired/constructed conductive paths. Their completion must change the same authoritative circuit graph used by voltage and signal diagnostics. The required W093 safety jumper is separate from the 92 traces. The finished reference geometry can guide appearance but cannot serve as connected gameplay routes.

## 10. Power, clock, reset and firmware prerequisites

Implement a deterministic 5 V protected input and 3.3 V rail with bounded startup, current limiting, brownout and ground-return behavior. The selected MCU must have its configured 48 MHz system clock and valid reset release before it can run board firmware. Keep firmware runtime bounded, reproducible and educational: it emits actual simulated I²C/SPI/UART transactions based on live device state, not merely green indicator flags. Support programmer-controlled firmware deployment and integrity/version check using the existing world-space UI. If powered but unprogrammed, show diagnostic access as appropriate without certifying running application. Reuse Level 99 firmware and processor services where genuinely compatible; do not duplicate implementations unless necessary.

## 11. I²C electrical and protocol simulation

Bus shares SDA and SCL with three separate addressable peripherals. Use open-drain low drivers and resistive pull-ups toward qualified 3.3 V, with capacitance, rise time, low-voltage and input threshold considerations. Simulate START/STOP, address, read/write, ACK/NACK, clock stretching, bus held low, device discovery and repeated-start as needed for chosen devices. Base event logs on device transactions and timing. Address collision must be represented correctly, and arbitration tests should only be enabled if two genuine simulated controllers are actually designed; otherwise explicitly keep arbitration as a non-required tutorial. Include qualification at 100 kHz and 400 kHz when approved devices support both. Do not use arbitrary static 'device online' booleans to fake successful discovery.

## 12. SPI electrical and protocol simulation

Provide at least two SPI peripherals from the vetted design, with shared SCK/MOSI/MISO if compatible, separate CS signals, configurable CPOL/CPHA and device-specific frequency ceilings (up to proposed 8 MHz). Enforce chip-select, input/output and clock sampling semantics. Test memory JEDEC-style identification only if real selected part supports it, and run deterministic read/write/verify with bounded storage. F02 breaks a **specific approved SCK via**, whose loss must prevent real clock edges at the peripheral input while leaving MCU-side toggles visible. A restored via must lead to valid transactions; do not heal the fault by directly setting a success flag.

## 13. UART communication and diagnostics

Provide a serial endpoint with correct TX/RX net direction and 3.3 V-compatible logic or an explicitly designed level transceiver. Implement framing with 115200 baud, 8 data bits, no parity, 1 stop bit. Generate start/data/stop transitions and frame sampling at the configured remote baud and clock tolerances. F03 changes endpoint configuration or UART divider, producing detectable mismatch. Virtual terminal displays raw bytes, decoding errors, frame/CRC status and timestamps. Firmware reconfiguration must be separately logged, and correction must be proven by a new bidirectional packet exchange.

## 14. Signal-integrity and observability model

Model first-order rise times, output drive strength, pull-up/down resistance, net capacitance, selected trace delay and logic thresholds. Keep the simulation performant and deterministic: it need not solve a full electromagnetic field model. DRC still matters: if an I²C line is shorted or a SPI clock path is open, decoded traffic must fail even if a UI diagram looks complete. Multimeter probes sample circuit nodes; oscilloscope streams sampled node waveforms; logic analyzer decodes events with appropriate sampling and threshold validation. Readouts must indicate stale/invalid samples and never emit invented data on an unpowered net.

## 15. Fault F01: I²C pull-up resistor

Source fault: **F01**, affected `I2C pullup resistor`, effect `Rise-time violation / possible invalid logic levels`, repair `Replace incorrect resistor, verify electrical response`. Final selected resistor reference must be specified by the vetted schematic, not invented from generic source CSV. Fault injection changes actual resistor electrical value, which changes line rise time with capacitance. Diagnosis is through the scope or logic analyzer, fault cause validation through selected replacement component, and recovery through measured compliant timing and fresh successful sensor/EEPROM transactions. Do not silently replace the wrong part by magic repair button.

## 16. Fault F02: open SPI SCK via

Source fault: **F02**, affected `SPI SCK plated via`, effect `SCK not received by target peripheral`, repair `Restore via connectivity, retest transactions`. Final specific via ID from V01–V20 must be assigned through the approved schematic, with no parallel factory trace bypass. Fault opens the physical graph edge, not just the SPI UI. Show MCU-side SCK and peripheral-side absence when probed. Via repair requires correct tool, target and safe state; then retest memory/peripheral transfers with fresh evidence.

## 17. Fault F03: UART baud/configuration mismatch

Source fault: **F03**, affected `UART configuration`, effect `Baud and frame mismatch`, repair `Correct endpoint firmware setting, verify decoded bytes`. This is a firmware/configuration repair, not mandatory physical desoldering. Modify actual simulated endpoint UART divider/format, causing incorrect receiver samples. The player identifies true baud using the virtual logic analyzer/terminal, changes the incompatible configuration and proves it with bidirectional independently validated packets. UI must not report a repaired wire for a pure configuration fault.

## 18. W093 critical feedback and safety behavior

Exact source JSON:

```json
{
  "id": "W093",
  "source": "L100_MCU_U1.TRUSTED_COMM_HEALTH_OUT",
  "destination": "L100_CommSafetySupervisor_U9.TRUSTED_COMM_HEALTH_IN",
  "type": "removable digital watchdog jumper",
  "initial": "disconnected",
  "validation": "requires fresh changing heartbeat evidence and independent safety reset",
  "note": "Logical interface designation only; pin-level design not yet verified"
}
```

W093 is an **installable removable digital heartbeat/communication-health jumper**, not a 3.3 V power conductor and not SDA/SCL/SCK/UART TX/RX. It starts **disconnected** for assembly and is installed for initial commissioning. In the supervised Objective 6 challenge, deliberately unplug it again: authoritative connectivity opens; the independent supervisor no longer receives fresh changing health pulses; after the specified bounded timeout, safety-dependent commands are inhibited; the event is latched and logged. The unrelated MCU bus hardware and independent protection remain functional when separately powered. Reconnection alone must not reset it. Require fresh changing qualified heartbeats, power and bus recheck, explicit user safety reset and full I²C/SPI/UART qualification run after the repair. Historical objectives 1–5 remain certified.

## 19. World-space virtual instruments, click/touch tools and VFX

Integrate the already established AAA virtual motherboard repair toolkit. Use world-space Canvas and mouse/touch input for multimeter, oscilloscope, logic analyzer, I²C scanner, SPI analyzer, UART terminal, firmware programmer, continuity and net tracer, microscope, solder/desolder, via repair, replacement and calibration. All tools should report the selected actual target object, selected `NetId`, probe state and measurement timestamp. Only position procedural spark, localized solder glow, trace pulse, via-glint or thermal feedback at the target contact point. Unassigned pad targets must be reported as not configured. Tool selection is UI state; repair authority and objective progression are controlled by simulation services and the existing manager.

## 20. Six objectives and evidence retention

Objective 1: 70 components deliberately installed with verified footprints/pins/solder. Objective 2: all 92 required routes plus 20 vias built and validated against a certified netlist, W093 installed for normal operation. Objective 3: I²C and SPI commissioning with valid discovered devices and data transactions. Objective 4: UART, chosen I²C stretch/arbitration tests, SPI memory integrity and communication error logging. Objective 5: F01/F02/F03 are individually detected and repaired, followed by fresh complete multi-bus qualification. Objective 6: real W093 removal, safety inhibit/latch, physical reconnection, fresh changing heartbeat, explicit reset and a new full commissioning run.

Keep evidence objects keyed by `objective_id`, `test_case_id`, run ID, topology revision, component state revision, firmware version, timestamp, result and critical readings. The authoritative manager must own certification and never derive it from UI text. If a later test fails, historical completed objectives remain certified where design requires, but no 'new qualification' requirement may be satisfied by older test records.

## 21. Expected starting state and transition design

At first playable load: 70 components UNINSTALLED (including W093), 92 copper tasks MISSING, 20 vias MISSING; no valid power and no firmware/hardware qualification. Existing display model is not part of this root. UI counts must read `0/70`, `0/92`, `0/20`, `0/6` initially. Progress through assembly → copper/via → W093 insertion → power/firmware → bus commissioning → protocol diagnostics → F01–F03 recoveries → supervised W093 disconnect/reset/recertification. Allow safe revisit of earlier tasks but prevent shortcutting missing prerequisites.

## 22. Editor one-click build and idempotency

Provide `Tools/Project Spark/Levels/Create Level 100` which locates the supplied FBXs and manifests by exact art root or durable GUID/config reference, checks required files, detects the current Project Spark level architecture, creates board prefab and component-footprint bindings, registers Level 100 only once and configures six objectives. Re-running the builder updates owned bindings without duplicating GameObjects, terminals, managers or UI canvases. The supplied existing art utilities remain available under:

- `Tools/Project Spark/Levels/100/Board Only/1. Build Empty DNX-8 PCB`
- `Tools/Project Spark/Levels/100/Board Only/2. Validate FBX Board Import`
- `Tools/Project Spark/Levels/100/Board Only/3. Generate Native PBR Materials`

The importer `AssetDatabase.FindAssets` currently uses model-name substring matching. Require unambiguous assets and fail clearly on duplicates, and improve texture root resolution rather than hardcode an installation path.

## 23. Unity art/material integration

Use pipeline-appropriate native materials, normal maps with correct importer type, roughness-to-smoothness conversion as appropriate, metallic and AO textures, readable markings and restrained emissions. Respect original FBX material-slot names. Verify FBX imports visually in Unity; the provided PBR installer creates albedo and selected mask assignments only, so finish normal/AO/emissive remapping and inspect HDRP/URP/Built-in differences. Favor a small number of shared materials, SRP Batcher compatibility, reasonable collider counts and LOD only where it helps. Maintain real-scale PCB `0.210 x 0.145 m` with separate preview scale on the big table; don't scale the authoritative electrical model for art presentation.

## 24. Architecture sketch and suggested new classes

Suggested responsibilities (names not assumed to exist): `SparkLevel100Coordinator`, `SparkLevel100NetlistValidator`, `SparkPCBComponentPlacement`, `SparkPCBPad`, `SparkPCBTrace`, `SparkPCBVia`, `SparkPCBDigitalBusGraph`, `SparkPCBI2CBus`, `SparkPCBSPIBus`, `SparkPCBUARTBus`, `SparkPCBCommunicationFirmware`, `SparkPCBSignalIntegrity`, `SparkPCBCommSafetySupervisor`, `SparkLevel100FaultController`, `SparkLevel100W093Recovery`, `SparkLevel100EvidenceRecorder`, `SparkLevel100ObjectiveTracker`, `SparkLevel100VisualRigAdapter`.

Inspect existing implementations first and **extend** them when possible; avoid parallel solver state. Use deterministic fixed-step processing for electrical and protocol state, separate visual-frame interpolation for UI, and traceable events for component changes. Be careful with save/load, undo, disabled scenes, prefab stages and domain reload in Editor.

## 25. Safety and gameplay correctness

The simulation is educational and virtual, not instructions for physical mains/high-voltage repair. Prevent a repair tool from silently working on unrelated targets or ignoring a powered-state interlock. Project Spark's player shelf, shop and recycle behavior must refuse operations on connected/in-use items when actual gameplay rules forbid them. Do not destroy a CPU or change circuit topology simply to animate a fault. Keep independent safety monitoring separate from bus data and the watchdog pattern. Avoid accepting a constant-high heartbeat or old data as alive. Ensure touch/raycast routing does not trigger clicks through the UI onto the PCB.

## 26. Performance and save/load requirements

Use pooled contact VFX, cache selected components and footprint references, avoid graph recomputation on every frame, and recompute electrical subgraphs only on topology changes or required simulation ticks. Keep high-frequency protocol events simulated without requiring Unity rendering every edge. Include deterministic test seeds and meaningful telemetry timestamps. Save placed components, routes/vias, firmware config, faults, objective evidence and safety latch state; reload and verify no false auto-completion and no missing W093 recovery history. All changes must leave Levels 1–99 unchanged.

## 27. Acceptance-test matrix — 90 cases

Implement the following 90 proposed checks as automated EditMode or PlayMode tests where practical, plus targeted manual visual/touch checks. Test cases must assert authoritative states, not screenshot colors.

### ART (10 checks)
- **AT-001** All 88 modular FBXs import.
- **AT-002** All 88 matching GLBs correspond.
- **AT-003** Assembled showcase remains isolated.
- **AT-004** PCB physical dimensions correct.
- **AT-005** PBR slots remapped.
- **AT-006** Normal maps imported correctly.
- **AT-007** No duplicated scene level root.
- **AT-008** Builder re-run idempotent.
- **AT-009** Layers show without conducting shorts.
- **AT-010** Board asset positions match placement CSV.
### ASSEMBLY (10 checks)
- **AT-011** 70 inventory components start uninstalled.
- **AT-012** W093 included in 70 total.
- **AT-013** Correct footprint accepted.
- **AT-014** Wrong footprint rejected.
- **AT-015** Pin one/orientation verified.
- **AT-016** Wrong IC rejected.
- **AT-017** Solder contact required.
- **AT-018** Duplicate placement rejected.
- **AT-019** Component removal updates topology.
- **AT-020** 70/70 only after all valid placements.
### NETLIST (10 checks)
- **AT-021** Unverified routes cannot certify.
- **AT-022** Unverified vias cannot certify.
- **AT-023** 92 verified routes required.
- **AT-024** 20 verified vias required.
- **AT-025** Cross-net short rejected.
- **AT-026** Layer transition without via rejected.
- **AT-027** Plane segment isolation respected.
- **AT-028** Factory trace cannot bypass missing route.
- **AT-029** Valid continuity updates solver.
- **AT-030** Save/load retains route state.
### POWER (10 checks)
- **AT-031** Unpowered MCU cannot execute.
- **AT-032** 3.3 V rail required.
- **AT-033** 5 V protected input behavior valid.
- **AT-034** Missing return path detected.
- **AT-035** Brownout affects MCU realistically.
- **AT-036** Clock invalid blocks firmware.
- **AT-037** Reset held blocks boot.
- **AT-038** Valid firmware starts only after readiness.
- **AT-039** Wrong firmware version rejected.
- **AT-040** Power loss invalidates fresh telemetry.
### I2C (10 checks)
- **AT-041** SDA and SCL open-drain behavior.
- **AT-042** Correct device discovery.
- **AT-043** Invalid address gives NACK.
- **AT-044** Rise time depends on pullups.
- **AT-045** F01 shifts rise time.
- **AT-046** F01 repair restores timing.
- **AT-047** Clock stretching represented.
- **AT-048** Bus held low causes failure.
- **AT-049** Address conflict handled.
- **AT-050** Fresh I2C transactions required.
### SPI (10 checks)
- **AT-051** Chip selects independent.
- **AT-052** SCK only at selected peripheral.
- **AT-053** SPI CPOL/CPHA validated.
- **AT-054** Wrong mode fails as appropriate.
- **AT-055** Data lines direction correct.
- **AT-056** F02 opens actual via.
- **AT-057** MCU-side SCK remains visible.
- **AT-058** Peripheral SCK disappears with fault.
- **AT-059** Via repair restores transactions.
- **AT-060** Memory write/read verification checks data.
### UART (10 checks)
- **AT-061** UART 115200 8N1 valid.
- **AT-062** Transmit produces sampled waveform.
- **AT-063** Receive decodes correct bytes.
- **AT-064** Wrong baud causes errors.
- **AT-065** F03 changes configuration state.
- **AT-066** Config repair restores decoding.
- **AT-067** Wrong pin routing prevents UART.
- **AT-068** Stale frames rejected.
- **AT-069** Bidirectional packets verified.
- **AT-070** UART log records timestamps.
### SAFETY (10 checks)
- **AT-071** W093 initially disconnected.
- **AT-072** W093 install enables future qualification.
- **AT-073** Heartbeat must change in time.
- **AT-074** Constant-high heartbeat rejected.
- **AT-075** W093 removal opens graph.
- **AT-076** Safety-dependent command inhibited.
- **AT-077** Independent monitor still powered.
- **AT-078** Critical safety fault latched.
- **AT-079** Reconnection alone does not clear.
- **AT-080** Explicit reset plus fresh full test required.
### OBJECTIVE (10 checks)
- **AT-081** Objective 1 actual assembly.
- **AT-082** Objective 2 actual routing+vias.
- **AT-083** Objective 3 real I2C+SPI.
- **AT-084** Objective 4 real UART+diagnostics.
- **AT-085** Objective 5 all three genuine repairs.
- **AT-086** Objective 6 fresh W093 recovery.
- **AT-087** No UI-only objective progress.
- **AT-088** Exactly 6/6 from LevelGamePlayManager.
- **AT-089** Save/reload preserves certificates correctly.
- **AT-090** Levels 1–99 regression suite unchanged.

## 28. Required delivery artifacts

Deliver actual `.cs` code integrated with the live Project Spark APIs; prefabs, scene or level asset, netlist and footprint mapping with audit report, validated material assets, and complete test project instructions. Include: `DNX8_VerifiedNetlist.json`, `DNX8_PadMap.csv`, `DNX8_Routes_Verified.csv`, `DNX8_Vias_Verified.csv`, component definitions, six real objectives, protocol test datasets, W093 recovery test sequence, diagnostics test scripts, README and a list of project integration points. Provide inspector references and editor menu steps. State explicitly which tests have actually run in Unity and which remain pending. Do not claim 6/6 until Play Mode confirms it.

## 29. Execution order for implementing agent

1. Audit installed Project Spark APIs and imported source kit. 2. Validate art/model manifest and clear import conflicts. 3. Select concrete MCU/peripheral parts and approve pin-level schematic. 4. Reconcile original 92 routes and 20 vias into authentic nets; fail closed until complete. 5. Build component placement and shared graph integration. 6. Implement power/clock/reset/firmware dependencies. 7. Implement I²C/SPI/UART simulations and virtual measurement hookups. 8. Implement F01–F03 stateful faults and repairs. 9. Implement independent W093 safety sequence. 10. Implement all six evidence-based objectives. 11. Create one-click editor level builder and material mapping. 12. Run 90 tests and Level 1–99 regressions; verify 6/6 honestly. Avoid treating verification as a UI cosmetic exercise.

---
# TECHNICAL ANNEXES — ORIGINAL SUPPLIED DATA

These annexes are copied from the actual asset kit. **UNASSIGNED** and **TBD** values are original placeholders, not valid electric assignments. The source route/via CSVs must be preserved and augmented with separate verified versions, never silently overwritten.
## Annex A. Asset manifest — all 88 FBX names

Source file: `Data/Asset_Manifest_88.csv`

```csv
name,type,region,large,x_mm,y_mm,rotation_deg,placement_state,index,file,glb,triangle_count,layer
U1,IC,MCU / I2C / SPI / UART / Supervision,True,0,0,0,uninstalled,1,001_L100_U1.fbx,001_L100_U1.glb,1612,
U2,IC,MCU / I2C / SPI / UART / Supervision,True,-55,34,0,uninstalled,2,002_L100_U2.fbx,002_L100_U2.glb,408,
U3,IC,MCU / I2C / SPI / UART / Supervision,True,-74,12,0,uninstalled,3,003_L100_U3.fbx,003_L100_U3.glb,408,
U4,IC,MCU / I2C / SPI / UART / Supervision,True,-55,-22,0,uninstalled,4,004_L100_U4.fbx,004_L100_U4.glb,408,
U5,IC,MCU / I2C / SPI / UART / Supervision,True,-74,-34,0,uninstalled,5,005_L100_U5.fbx,005_L100_U5.glb,408,
U6,IC,MCU / I2C / SPI / UART / Supervision,True,57,35,0,uninstalled,6,006_L100_U6.fbx,006_L100_U6.glb,408,
U7,IC,MCU / I2C / SPI / UART / Supervision,True,79,15,0,uninstalled,7,007_L100_U7.fbx,007_L100_U7.glb,408,
U8,IC,MCU / I2C / SPI / UART / Supervision,True,67,-26,0,uninstalled,8,008_L100_U8.fbx,008_L100_U8.glb,408,
U9,IC,MCU / I2C / SPI / UART / Supervision,True,46,-43,0,uninstalled,9,009_L100_U9.fbx,009_L100_U9.glb,408,
U10,IC,MCU / I2C / SPI / UART / Supervision,True,28,37,0,uninstalled,10,010_L100_U10.fbx,010_L100_U10.glb,408,
U11,IC,MCU / I2C / SPI / UART / Supervision,True,-29,37,0,uninstalled,11,011_L100_U11.fbx,011_L100_U11.glb,408,
U12,IC,MCU / I2C / SPI / UART / Supervision,True,-29,-40,0,uninstalled,12,012_L100_U12.fbx,012_L100_U12.glb,408,
R1,Resistor,Pullups / Series / Feedback,False,-85,-59,0,uninstalled,13,013_L100_R1.fbx,013_L100_R1.glb,48,
R2,Resistor,Pullups / Series / Feedback,False,-61,-59,0,uninstalled,14,014_L100_R2.fbx,014_L100_R2.glb,48,
R3,Resistor,Pullups / Series / Feedback,False,-37,-59,0,uninstalled,15,015_L100_R3.fbx,015_L100_R3.glb,48,
R4,Resistor,Pullups / Series / Feedback,False,-13,-59,0,uninstalled,16,016_L100_R4.fbx,016_L100_R4.glb,48,
R5,Resistor,Pullups / Series / Feedback,False,11,-59,0,uninstalled,17,017_L100_R5.fbx,017_L100_R5.glb,48,
R6,Resistor,Pullups / Series / Feedback,False,35,-59,0,uninstalled,18,018_L100_R6.fbx,018_L100_R6.glb,48,
R7,Resistor,Pullups / Series / Feedback,False,59,-59,0,uninstalled,19,019_L100_R7.fbx,019_L100_R7.glb,48,
R8,Resistor,Pullups / Series / Feedback,False,83,-59,0,uninstalled,20,020_L100_R8.fbx,020_L100_R8.glb,48,
R9,Resistor,Pullups / Series / Feedback,False,-85,-50,0,uninstalled,21,021_L100_R9.fbx,021_L100_R9.glb,48,
R10,Resistor,Pullups / Series / Feedback,False,-61,-50,0,uninstalled,22,022_L100_R10.fbx,022_L100_R10.glb,48,
R11,Resistor,Pullups / Series / Feedback,False,-37,-50,0,uninstalled,23,023_L100_R11.fbx,023_L100_R11.glb,48,
R12,Resistor,Pullups / Series / Feedback,False,-13,-50,0,uninstalled,24,024_L100_R12.fbx,024_L100_R12.glb,48,
R13,Resistor,Pullups / Series / Feedback,False,11,-50,0,uninstalled,25,025_L100_R13.fbx,025_L100_R13.glb,48,
R14,Resistor,Pullups / Series / Feedback,False,35,-50,0,uninstalled,26,026_L100_R14.fbx,026_L100_R14.glb,48,
R15,Resistor,Pullups / Series / Feedback,False,59,-50,0,uninstalled,27,027_L100_R15.fbx,027_L100_R15.glb,48,
R16,Resistor,Pullups / Series / Feedback,False,83,-50,0,uninstalled,28,028_L100_R16.fbx,028_L100_R16.glb,48,
R17,Resistor,Pullups / Series / Feedback,False,-85,-41,0,uninstalled,29,029_L100_R17.fbx,029_L100_R17.glb,48,
R18,Resistor,Pullups / Series / Feedback,False,-61,-41,0,uninstalled,30,030_L100_R18.fbx,030_L100_R18.glb,48,
R19,Resistor,Pullups / Series / Feedback,False,-37,-41,0,uninstalled,31,031_L100_R19.fbx,031_L100_R19.glb,48,
C1,MLCC,Bypass / Filtering,False,-86,51,0,uninstalled,32,032_L100_C1.fbx,032_L100_C1.glb,36,
C2,MLCC,Bypass / Filtering,False,-62,51,0,uninstalled,33,033_L100_C2.fbx,033_L100_C2.glb,36,
C3,MLCC,Bypass / Filtering,False,-38,51,0,uninstalled,34,034_L100_C3.fbx,034_L100_C3.glb,36,
C4,MLCC,Bypass / Filtering,False,-14,51,0,uninstalled,35,035_L100_C4.fbx,035_L100_C4.glb,36,
C5,MLCC,Bypass / Filtering,False,10,51,0,uninstalled,36,036_L100_C5.fbx,036_L100_C5.glb,36,
C6,MLCC,Bypass / Filtering,False,34,51,0,uninstalled,37,037_L100_C6.fbx,037_L100_C6.glb,36,
C7,MLCC,Bypass / Filtering,False,58,51,0,uninstalled,38,038_L100_C7.fbx,038_L100_C7.glb,36,
C8,MLCC,Bypass / Filtering,False,82,51,0,uninstalled,39,039_L100_C8.fbx,039_L100_C8.glb,36,
C9,MLCC,Bypass / Filtering,False,-86,59,0,uninstalled,40,040_L100_C9.fbx,040_L100_C9.glb,36,
C10,MLCC,Bypass / Filtering,False,-62,59,0,uninstalled,41,041_L100_C10.fbx,041_L100_C10.glb,36,
C11,MLCC,Bypass / Filtering,False,-38,59,0,uninstalled,42,042_L100_C11.fbx,042_L100_C11.glb,36,
C12,MLCC,Bypass / Filtering,False,-14,59,0,uninstalled,43,043_L100_C12.fbx,043_L100_C12.glb,36,
C13,MLCC,Bypass / Filtering,False,10,59,0,uninstalled,44,044_L100_C13.fbx,044_L100_C13.glb,36,
C14,MLCC,Bypass / Filtering,False,34,59,0,uninstalled,45,045_L100_C14.fbx,045_L100_C14.glb,36,
C15,MLCC,Bypass / Filtering,False,58,59,0,uninstalled,46,046_L100_C15.fbx,046_L100_C15.glb,36,
C16,MLCC,Bypass / Filtering,False,82,59,0,uninstalled,47,047_L100_C16.fbx,047_L100_C16.glb,36,
C17,MLCC,Bypass / Filtering,False,-86,67,0,uninstalled,48,048_L100_C17.fbx,048_L100_C17.glb,36,
C18,MLCC,Bypass / Filtering,False,-62,67,0,uninstalled,49,049_L100_C18.fbx,049_L100_C18.glb,36,
C19,MLCC,Bypass / Filtering,False,-38,67,0,uninstalled,50,050_L100_C19.fbx,050_L100_C19.glb,36,
C20,MLCC,Bypass / Filtering,False,-14,67,0,uninstalled,51,051_L100_C20.fbx,051_L100_C20.glb,36,
Y1,Crystal,Clock,False,-34,9,0,uninstalled,52,052_L100_Y1.fbx,052_L100_Y1.glb,84,
J1,Connector,Programming / Bus / USB,False,-91,42,0,uninstalled,53,053_L100_J1.fbx,053_L100_J1.glb,220,
J2,Connector,Programming / Bus / USB,False,91,43,0,uninstalled,54,054_L100_J2.fbx,054_L100_J2.glb,220,
J3,Connector,Programming / Bus / USB,False,92,-29,0,uninstalled,55,055_L100_J3.fbx,055_L100_J3.glb,324,
J4,Connector,Programming / Bus / USB,False,-92,-43,0,uninstalled,56,056_L100_J4.fbx,056_L100_J4.glb,220,
J5,Connector,Programming / Bus / USB,False,89,-54,0,uninstalled,57,057_L100_J5.fbx,057_L100_J5.glb,220,
LED1,LED,Communications Status,False,14,-59,0,uninstalled,58,058_L100_LED1.fbx,058_L100_LED1.glb,48,
LED2,LED,Communications Status,False,30,-59,0,uninstalled,59,059_L100_LED2.fbx,059_L100_LED2.glb,48,
LED3,LED,Communications Status,False,46,-59,0,uninstalled,60,060_L100_LED3.fbx,060_L100_LED3.glb,48,
LED4,LED,Communications Status,False,62,-59,0,uninstalled,61,061_L100_LED4.fbx,061_L100_LED4.glb,48,
SW1,Switch,Reset / Address / Bus,False,-76,-8,0,uninstalled,62,062_L100_SW1.fbx,062_L100_SW1.glb,156,
SW2,Switch,Reset / Address / Bus,False,-58,-8,0,uninstalled,63,063_L100_SW2.fbx,063_L100_SW2.glb,156,
SW3,Switch,Reset / Address / Bus,False,-40,-8,0,uninstalled,64,064_L100_SW3.fbx,064_L100_SW3.glb,156,
D1,Protection,ESD / Protection,False,-83,62,0,uninstalled,65,065_L100_D1.fbx,065_L100_D1.glb,36,
D2,Protection,ESD / Protection,False,-66,62,0,uninstalled,66,066_L100_D2.fbx,066_L100_D2.glb,36,
D3,Protection,ESD / Protection,False,-49,62,0,uninstalled,67,067_L100_D3.fbx,067_L100_D3.glb,36,
FB1,FerriteBead,Filtering,False,30,61,0,uninstalled,68,068_L100_FB1.fbx,068_L100_FB1.glb,36,
FB2,FerriteBead,Filtering,False,50,61,0,uninstalled,69,069_L100_FB2.fbx,069_L100_FB2.glb,36,
W093,Jumper,Comm Health,False,79,-3,0,uninstalled,70,070_L100_W093.fbx,070_L100_W093.glb,120,
DNX8_Empty_FourLayer_PCB,Board,,,0,0,0,,71,071_L100_DNX8_Empty_FourLayer_PCB.fbx,071_L100_DNX8_Empty_FourLayer_PCB.glb,3376,
L1_Top_Signal_Reference,Plane,,,0,0,0,,72,072_L100_L1_Top_Signal_Reference.fbx,072_L100_L1_Top_Signal_Reference.glb,12,L1
L2_Ground_Plane_Reference,Plane,,,0,0,0,,73,073_L100_L2_Ground_Plane_Reference.fbx,073_L100_L2_Ground_Plane_Reference.glb,12,L2
L3_3V3_Power_Plane_Reference,Plane,,,0,0,0,,74,074_L100_L3_3V3_Power_Plane_Reference.fbx,074_L100_L3_3V3_Power_Plane_Reference.glb,12,L3
L4_Bottom_Signal_Reference,Plane,,,0,0,0,,75,075_L100_L4_Bottom_Signal_Reference.fbx,075_L100_L4_Bottom_Signal_Reference.glb,12,L4
Plated_Through_Via,Via,,,0,0,0,,76,076_L100_Plated_Through_Via.fbx,076_L100_Plated_Through_Via.glb,88,
Microvia_L1_L2,Via,,,0,0,0,,77,077_L100_Microvia_L1_L2.fbx,077_L100_Microvia_L1_L2.glb,88,
Factory_Copper_Reference,Trace,,,0,0,0,,78,078_L100_Factory_Copper_Reference.fbx,078_L100_Factory_Copper_Reference.glb,84,
I2C_SDA_TestPad,TestPad,,,0,0,0,,79,079_L100_I2C_SDA_TestPad.fbx,079_L100_I2C_SDA_TestPad.glb,144,
I2C_SCL_TestPad,TestPad,,,0,0,0,,80,080_L100_I2C_SCL_TestPad.fbx,080_L100_I2C_SCL_TestPad.glb,144,
SPI_SCK_TestPad,TestPad,,,0,0,0,,81,081_L100_SPI_SCK_TestPad.fbx,081_L100_SPI_SCK_TestPad.glb,144,
UART_TX_TestPad,TestPad,,,0,0,0,,82,082_L100_UART_TX_TestPad.fbx,082_L100_UART_TX_TestPad.glb,144,
Via_Repair_Segment,Repair,,,0,0,0,,83,083_L100_Via_Repair_Segment.fbx,083_L100_Via_Repair_Segment.glb,212,
Solder_Bridge_Repair,Repair,,,0,0,0,,84,084_L100_Solder_Bridge_Repair.fbx,084_L100_Solder_Bridge_Repair.glb,212,
Factory_Silkscreen_Reference,Silk,,,0,0,0,,85,085_L100_Factory_Silkscreen_Reference.fbx,085_L100_Factory_Silkscreen_Reference.glb,60,
I2C_Pullup_Fault_Overlay,Fault,,,0,0,0,,86,086_L100_I2C_Pullup_Fault_Overlay.fbx,086_L100_I2C_Pullup_Fault_Overlay.glb,24,
SPI_OpenVia_Fault_Overlay,Fault,,,0,0,0,,87,087_L100_SPI_OpenVia_Fault_Overlay.fbx,087_L100_SPI_OpenVia_Fault_Overlay.glb,24,
UART_Config_Fault_Overlay,Fault,,,0,0,0,,88,088_L100_UART_Config_Fault_Overlay.fbx,088_L100_UART_Config_Fault_Overlay.glb,24,
```

## Annex B. Installable bill of materials — 70 parts

Source file: `Data/BOM_70_Installable.csv`

```csv
name,type,region,large,x_mm,y_mm,rotation_deg,placement_state,index
U1,IC,MCU / I2C / SPI / UART / Supervision,True,0,0,0,uninstalled,1
U2,IC,MCU / I2C / SPI / UART / Supervision,True,-55,34,0,uninstalled,2
U3,IC,MCU / I2C / SPI / UART / Supervision,True,-74,12,0,uninstalled,3
U4,IC,MCU / I2C / SPI / UART / Supervision,True,-55,-22,0,uninstalled,4
U5,IC,MCU / I2C / SPI / UART / Supervision,True,-74,-34,0,uninstalled,5
U6,IC,MCU / I2C / SPI / UART / Supervision,True,57,35,0,uninstalled,6
U7,IC,MCU / I2C / SPI / UART / Supervision,True,79,15,0,uninstalled,7
U8,IC,MCU / I2C / SPI / UART / Supervision,True,67,-26,0,uninstalled,8
U9,IC,MCU / I2C / SPI / UART / Supervision,True,46,-43,0,uninstalled,9
U10,IC,MCU / I2C / SPI / UART / Supervision,True,28,37,0,uninstalled,10
U11,IC,MCU / I2C / SPI / UART / Supervision,True,-29,37,0,uninstalled,11
U12,IC,MCU / I2C / SPI / UART / Supervision,True,-29,-40,0,uninstalled,12
R1,Resistor,Pullups / Series / Feedback,False,-85,-59,0,uninstalled,13
R2,Resistor,Pullups / Series / Feedback,False,-61,-59,0,uninstalled,14
R3,Resistor,Pullups / Series / Feedback,False,-37,-59,0,uninstalled,15
R4,Resistor,Pullups / Series / Feedback,False,-13,-59,0,uninstalled,16
R5,Resistor,Pullups / Series / Feedback,False,11,-59,0,uninstalled,17
R6,Resistor,Pullups / Series / Feedback,False,35,-59,0,uninstalled,18
R7,Resistor,Pullups / Series / Feedback,False,59,-59,0,uninstalled,19
R8,Resistor,Pullups / Series / Feedback,False,83,-59,0,uninstalled,20
R9,Resistor,Pullups / Series / Feedback,False,-85,-50,0,uninstalled,21
R10,Resistor,Pullups / Series / Feedback,False,-61,-50,0,uninstalled,22
R11,Resistor,Pullups / Series / Feedback,False,-37,-50,0,uninstalled,23
R12,Resistor,Pullups / Series / Feedback,False,-13,-50,0,uninstalled,24
R13,Resistor,Pullups / Series / Feedback,False,11,-50,0,uninstalled,25
R14,Resistor,Pullups / Series / Feedback,False,35,-50,0,uninstalled,26
R15,Resistor,Pullups / Series / Feedback,False,59,-50,0,uninstalled,27
R16,Resistor,Pullups / Series / Feedback,False,83,-50,0,uninstalled,28
R17,Resistor,Pullups / Series / Feedback,False,-85,-41,0,uninstalled,29
R18,Resistor,Pullups / Series / Feedback,False,-61,-41,0,uninstalled,30
R19,Resistor,Pullups / Series / Feedback,False,-37,-41,0,uninstalled,31
C1,MLCC,Bypass / Filtering,False,-86,51,0,uninstalled,32
C2,MLCC,Bypass / Filtering,False,-62,51,0,uninstalled,33
C3,MLCC,Bypass / Filtering,False,-38,51,0,uninstalled,34
C4,MLCC,Bypass / Filtering,False,-14,51,0,uninstalled,35
C5,MLCC,Bypass / Filtering,False,10,51,0,uninstalled,36
C6,MLCC,Bypass / Filtering,False,34,51,0,uninstalled,37
C7,MLCC,Bypass / Filtering,False,58,51,0,uninstalled,38
C8,MLCC,Bypass / Filtering,False,82,51,0,uninstalled,39
C9,MLCC,Bypass / Filtering,False,-86,59,0,uninstalled,40
C10,MLCC,Bypass / Filtering,False,-62,59,0,uninstalled,41
C11,MLCC,Bypass / Filtering,False,-38,59,0,uninstalled,42
C12,MLCC,Bypass / Filtering,False,-14,59,0,uninstalled,43
C13,MLCC,Bypass / Filtering,False,10,59,0,uninstalled,44
C14,MLCC,Bypass / Filtering,False,34,59,0,uninstalled,45
C15,MLCC,Bypass / Filtering,False,58,59,0,uninstalled,46
C16,MLCC,Bypass / Filtering,False,82,59,0,uninstalled,47
C17,MLCC,Bypass / Filtering,False,-86,67,0,uninstalled,48
C18,MLCC,Bypass / Filtering,False,-62,67,0,uninstalled,49
C19,MLCC,Bypass / Filtering,False,-38,67,0,uninstalled,50
C20,MLCC,Bypass / Filtering,False,-14,67,0,uninstalled,51
Y1,Crystal,Clock,False,-34,9,0,uninstalled,52
J1,Connector,Programming / Bus / USB,False,-91,42,0,uninstalled,53
J2,Connector,Programming / Bus / USB,False,91,43,0,uninstalled,54
J3,Connector,Programming / Bus / USB,False,92,-29,0,uninstalled,55
J4,Connector,Programming / Bus / USB,False,-92,-43,0,uninstalled,56
J5,Connector,Programming / Bus / USB,False,89,-54,0,uninstalled,57
LED1,LED,Communications Status,False,14,-59,0,uninstalled,58
LED2,LED,Communications Status,False,30,-59,0,uninstalled,59
LED3,LED,Communications Status,False,46,-59,0,uninstalled,60
LED4,LED,Communications Status,False,62,-59,0,uninstalled,61
SW1,Switch,Reset / Address / Bus,False,-76,-8,0,uninstalled,62
SW2,Switch,Reset / Address / Bus,False,-58,-8,0,uninstalled,63
SW3,Switch,Reset / Address / Bus,False,-40,-8,0,uninstalled,64
D1,Protection,ESD / Protection,False,-83,62,0,uninstalled,65
D2,Protection,ESD / Protection,False,-66,62,0,uninstalled,66
D3,Protection,ESD / Protection,False,-49,62,0,uninstalled,67
FB1,FerriteBead,Filtering,False,30,61,0,uninstalled,68
FB2,FerriteBead,Filtering,False,50,61,0,uninstalled,69
W093,Jumper,Comm Health,False,79,-3,0,uninstalled,70
```

## Annex C. Exact 70 placement coordinates

Source file: `Data/Component_Placement_70.csv`

```csv
ref,x_mm,y_mm,rotation_deg,state,model
U1,0,0,0,UNINSTALLED,001_L100_U1.fbx
U2,-55,34,0,UNINSTALLED,002_L100_U2.fbx
U3,-74,12,0,UNINSTALLED,003_L100_U3.fbx
U4,-55,-22,0,UNINSTALLED,004_L100_U4.fbx
U5,-74,-34,0,UNINSTALLED,005_L100_U5.fbx
U6,57,35,0,UNINSTALLED,006_L100_U6.fbx
U7,79,15,0,UNINSTALLED,007_L100_U7.fbx
U8,67,-26,0,UNINSTALLED,008_L100_U8.fbx
U9,46,-43,0,UNINSTALLED,009_L100_U9.fbx
U10,28,37,0,UNINSTALLED,010_L100_U10.fbx
U11,-29,37,0,UNINSTALLED,011_L100_U11.fbx
U12,-29,-40,0,UNINSTALLED,012_L100_U12.fbx
R1,-85,-59,0,UNINSTALLED,013_L100_R1.fbx
R2,-61,-59,0,UNINSTALLED,014_L100_R2.fbx
R3,-37,-59,0,UNINSTALLED,015_L100_R3.fbx
R4,-13,-59,0,UNINSTALLED,016_L100_R4.fbx
R5,11,-59,0,UNINSTALLED,017_L100_R5.fbx
R6,35,-59,0,UNINSTALLED,018_L100_R6.fbx
R7,59,-59,0,UNINSTALLED,019_L100_R7.fbx
R8,83,-59,0,UNINSTALLED,020_L100_R8.fbx
R9,-85,-50,0,UNINSTALLED,021_L100_R9.fbx
R10,-61,-50,0,UNINSTALLED,022_L100_R10.fbx
R11,-37,-50,0,UNINSTALLED,023_L100_R11.fbx
R12,-13,-50,0,UNINSTALLED,024_L100_R12.fbx
R13,11,-50,0,UNINSTALLED,025_L100_R13.fbx
R14,35,-50,0,UNINSTALLED,026_L100_R14.fbx
R15,59,-50,0,UNINSTALLED,027_L100_R15.fbx
R16,83,-50,0,UNINSTALLED,028_L100_R16.fbx
R17,-85,-41,0,UNINSTALLED,029_L100_R17.fbx
R18,-61,-41,0,UNINSTALLED,030_L100_R18.fbx
R19,-37,-41,0,UNINSTALLED,031_L100_R19.fbx
C1,-86,51,0,UNINSTALLED,032_L100_C1.fbx
C2,-62,51,0,UNINSTALLED,033_L100_C2.fbx
C3,-38,51,0,UNINSTALLED,034_L100_C3.fbx
C4,-14,51,0,UNINSTALLED,035_L100_C4.fbx
C5,10,51,0,UNINSTALLED,036_L100_C5.fbx
C6,34,51,0,UNINSTALLED,037_L100_C6.fbx
C7,58,51,0,UNINSTALLED,038_L100_C7.fbx
C8,82,51,0,UNINSTALLED,039_L100_C8.fbx
C9,-86,59,0,UNINSTALLED,040_L100_C9.fbx
C10,-62,59,0,UNINSTALLED,041_L100_C10.fbx
C11,-38,59,0,UNINSTALLED,042_L100_C11.fbx
C12,-14,59,0,UNINSTALLED,043_L100_C12.fbx
C13,10,59,0,UNINSTALLED,044_L100_C13.fbx
C14,34,59,0,UNINSTALLED,045_L100_C14.fbx
C15,58,59,0,UNINSTALLED,046_L100_C15.fbx
C16,82,59,0,UNINSTALLED,047_L100_C16.fbx
C17,-86,67,0,UNINSTALLED,048_L100_C17.fbx
C18,-62,67,0,UNINSTALLED,049_L100_C18.fbx
C19,-38,67,0,UNINSTALLED,050_L100_C19.fbx
C20,-14,67,0,UNINSTALLED,051_L100_C20.fbx
Y1,-34,9,0,UNINSTALLED,052_L100_Y1.fbx
J1,-91,42,0,UNINSTALLED,053_L100_J1.fbx
J2,91,43,0,UNINSTALLED,054_L100_J2.fbx
J3,92,-29,0,UNINSTALLED,055_L100_J3.fbx
J4,-92,-43,0,UNINSTALLED,056_L100_J4.fbx
J5,89,-54,0,UNINSTALLED,057_L100_J5.fbx
LED1,14,-59,0,UNINSTALLED,058_L100_LED1.fbx
LED2,30,-59,0,UNINSTALLED,059_L100_LED2.fbx
LED3,46,-59,0,UNINSTALLED,060_L100_LED3.fbx
LED4,62,-59,0,UNINSTALLED,061_L100_LED4.fbx
SW1,-76,-8,0,UNINSTALLED,062_L100_SW1.fbx
SW2,-58,-8,0,UNINSTALLED,063_L100_SW2.fbx
SW3,-40,-8,0,UNINSTALLED,064_L100_SW3.fbx
D1,-83,62,0,UNINSTALLED,065_L100_D1.fbx
D2,-66,62,0,UNINSTALLED,066_L100_D2.fbx
D3,-49,62,0,UNINSTALLED,067_L100_D3.fbx
FB1,30,61,0,UNINSTALLED,068_L100_FB1.fbx
FB2,50,61,0,UNINSTALLED,069_L100_FB2.fbx
W093,79,-3,0,UNINSTALLED,070_L100_W093.fbx
```

## Annex D. Original 92 unverified copper-route tasks

Source file: `Data/Player_Copper_Routes_W001_W092_UNVERIFIED.csv`

```csv
route_id,subsystem,from_pad,to_pad,net_id,layer,verified,initial_state,note
W001,Power/Ground,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,False,MISSING,Conceptual task only: assign authentic package pins and verify topology before certification
W002,Power/Ground,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,False,MISSING,Conceptual task only: assign authentic package pins and verify topology before certification
W003,Power/Ground,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,False,MISSING,Conceptual task only: assign authentic package pins and verify topology before certification
W004,Power/Ground,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,False,MISSING,Conceptual task only: assign authentic package pins and verify topology before certification
W005,Power/Ground,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,False,MISSING,Conceptual task only: assign authentic package pins and verify topology before certification
W006,Power/Ground,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,False,MISSING,Conceptual task only: assign authentic package pins and verify topology before certification
W007,Power/Ground,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,False,MISSING,Conceptual task only: assign authentic package pins and verify topology before certification
W008,Power/Ground,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,False,MISSING,Conceptual task only: assign authentic package pins and verify topology before certification
W009,Power/Ground,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,False,MISSING,Conceptual task only: assign authentic package pins and verify topology before certification
W010,Power/Ground,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,False,MISSING,Conceptual task only: assign authentic package pins and verify topology before certification
W011,Power/Ground,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,False,MISSING,Conceptual task only: assign authentic package pins and verify topology before certification
W012,Power/Ground,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,False,MISSING,Conceptual task only: assign authentic package pins and verify topology before certification
W013,Power/Ground,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,False,MISSING,Conceptual task only: assign authentic package pins and verify topology before certification
W014,Power/Ground,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,False,MISSING,Conceptual task only: assign authentic package pins and verify topology before certification
W015,I2C_SDA_SCL_Pullups,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,False,MISSING,Conceptual task only: assign authentic package pins and verify topology before certification
W016,I2C_SDA_SCL_Pullups,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,False,MISSING,Conceptual task only: assign authentic package pins and verify topology before certification
W017,I2C_SDA_SCL_Pullups,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,False,MISSING,Conceptual task only: assign authentic package pins and verify topology before certification
W018,I2C_SDA_SCL_Pullups,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,False,MISSING,Conceptual task only: assign authentic package pins and verify topology before certification
W019,I2C_SDA_SCL_Pullups,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,False,MISSING,Conceptual task only: assign authentic package pins and verify topology before certification
W020,I2C_SDA_SCL_Pullups,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,False,MISSING,Conceptual task only: assign authentic package pins and verify topology before certification
W021,I2C_SDA_SCL_Pullups,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,False,MISSING,Conceptual task only: assign authentic package pins and verify topology before certification
W022,I2C_SDA_SCL_Pullups,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,False,MISSING,Conceptual task only: assign authentic package pins and verify topology before certification
W023,I2C_SDA_SCL_Pullups,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,False,MISSING,Conceptual task only: assign authentic package pins and verify topology before certification
W024,I2C_SDA_SCL_Pullups,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,False,MISSING,Conceptual task only: assign authentic package pins and verify topology before certification
W025,I2C_SDA_SCL_Pullups,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,False,MISSING,Conceptual task only: assign authentic package pins and verify topology before certification
W026,I2C_SDA_SCL_Pullups,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,False,MISSING,Conceptual task only: assign authentic package pins and verify topology before certification
W027,I2C_SDA_SCL_Pullups,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,False,MISSING,Conceptual task only: assign authentic package pins and verify topology before certification
W028,I2C_SDA_SCL_Pullups,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,False,MISSING,Conceptual task only: assign authentic package pins and verify topology before certification
W029,I2C_SDA_SCL_Pullups,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,False,MISSING,Conceptual task only: assign authentic package pins and verify topology before certification
W030,I2C_SDA_SCL_Pullups,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,False,MISSING,Conceptual task only: assign authentic package pins and verify topology before certification
W031,I2C_SDA_SCL_Pullups,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,False,MISSING,Conceptual task only: assign authentic package pins and verify topology before certification
W032,I2C_SDA_SCL_Pullups,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,False,MISSING,Conceptual task only: assign authentic package pins and verify topology before certification
W033,I2C_SDA_SCL_Pullups,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,False,MISSING,Conceptual task only: assign authentic package pins and verify topology before certification
W034,I2C_SDA_SCL_Pullups,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,False,MISSING,Conceptual task only: assign authentic package pins and verify topology before certification
W035,SPI_Clock_Data_ChipSelect,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,False,MISSING,Conceptual task only: assign authentic package pins and verify topology before certification
W036,SPI_Clock_Data_ChipSelect,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,False,MISSING,Conceptual task only: assign authentic package pins and verify topology before certification
W037,SPI_Clock_Data_ChipSelect,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,False,MISSING,Conceptual task only: assign authentic package pins and verify topology before certification
W038,SPI_Clock_Data_ChipSelect,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,False,MISSING,Conceptual task only: assign authentic package pins and verify topology before certification
W039,SPI_Clock_Data_ChipSelect,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,False,MISSING,Conceptual task only: assign authentic package pins and verify topology before certification
W040,SPI_Clock_Data_ChipSelect,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,False,MISSING,Conceptual task only: assign authentic package pins and verify topology before certification
W041,SPI_Clock_Data_ChipSelect,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,False,MISSING,Conceptual task only: assign authentic package pins and verify topology before certification
W042,SPI_Clock_Data_ChipSelect,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,False,MISSING,Conceptual task only: assign authentic package pins and verify topology before certification
W043,SPI_Clock_Data_ChipSelect,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,False,MISSING,Conceptual task only: assign authentic package pins and verify topology before certification
W044,SPI_Clock_Data_ChipSelect,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,False,MISSING,Conceptual task only: assign authentic package pins and verify topology before certification
W045,SPI_Clock_Data_ChipSelect,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,False,MISSING,Conceptual task only: assign authentic package pins and verify topology before certification
W046,SPI_Clock_Data_ChipSelect,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,False,MISSING,Conceptual task only: assign authentic package pins and verify topology before certification
W047,SPI_Clock_Data_ChipSelect,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,False,MISSING,Conceptual task only: assign authentic package pins and verify topology before certification
W048,SPI_Clock_Data_ChipSelect,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,False,MISSING,Conceptual task only: assign authentic package pins and verify topology before certification
W049,SPI_Clock_Data_ChipSelect,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,False,MISSING,Conceptual task only: assign authentic package pins and verify topology before certification
W050,SPI_Clock_Data_ChipSelect,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,False,MISSING,Conceptual task only: assign authentic package pins and verify topology before certification
W051,SPI_Clock_Data_ChipSelect,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,False,MISSING,Conceptual task only: assign authentic package pins and verify topology before certification
W052,SPI_Clock_Data_ChipSelect,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,False,MISSING,Conceptual task only: assign authentic package pins and verify topology before certification
W053,SPI_Clock_Data_ChipSelect,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,False,MISSING,Conceptual task only: assign authentic package pins and verify topology before certification
W054,SPI_Clock_Data_ChipSelect,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,False,MISSING,Conceptual task only: assign authentic package pins and verify topology before certification
W055,UART_Diagnostics,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,False,MISSING,Conceptual task only: assign authentic package pins and verify topology before certification
W056,UART_Diagnostics,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,False,MISSING,Conceptual task only: assign authentic package pins and verify topology before certification
W057,UART_Diagnostics,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,False,MISSING,Conceptual task only: assign authentic package pins and verify topology before certification
W058,UART_Diagnostics,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,False,MISSING,Conceptual task only: assign authentic package pins and verify topology before certification
W059,UART_Diagnostics,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,False,MISSING,Conceptual task only: assign authentic package pins and verify topology before certification
W060,UART_Diagnostics,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,False,MISSING,Conceptual task only: assign authentic package pins and verify topology before certification
W061,UART_Diagnostics,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,False,MISSING,Conceptual task only: assign authentic package pins and verify topology before certification
W062,UART_Diagnostics,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,False,MISSING,Conceptual task only: assign authentic package pins and verify topology before certification
W063,UART_Diagnostics,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,False,MISSING,Conceptual task only: assign authentic package pins and verify topology before certification
W064,UART_Diagnostics,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,False,MISSING,Conceptual task only: assign authentic package pins and verify topology before certification
W065,UART_Diagnostics,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,False,MISSING,Conceptual task only: assign authentic package pins and verify topology before certification
W066,UART_Diagnostics,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,False,MISSING,Conceptual task only: assign authentic package pins and verify topology before certification
W067,UART_Diagnostics,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,False,MISSING,Conceptual task only: assign authentic package pins and verify topology before certification
W068,UART_Diagnostics,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,False,MISSING,Conceptual task only: assign authentic package pins and verify topology before certification
W069,Clock_Reset_Firmware,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,False,MISSING,Conceptual task only: assign authentic package pins and verify topology before certification
W070,Clock_Reset_Firmware,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,False,MISSING,Conceptual task only: assign authentic package pins and verify topology before certification
W071,Clock_Reset_Firmware,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,False,MISSING,Conceptual task only: assign authentic package pins and verify topology before certification
W072,Clock_Reset_Firmware,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,False,MISSING,Conceptual task only: assign authentic package pins and verify topology before certification
W073,Clock_Reset_Firmware,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,False,MISSING,Conceptual task only: assign authentic package pins and verify topology before certification
W074,Clock_Reset_Firmware,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,False,MISSING,Conceptual task only: assign authentic package pins and verify topology before certification
W075,Clock_Reset_Firmware,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,False,MISSING,Conceptual task only: assign authentic package pins and verify topology before certification
W076,Clock_Reset_Firmware,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,False,MISSING,Conceptual task only: assign authentic package pins and verify topology before certification
W077,Clock_Reset_Firmware,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,False,MISSING,Conceptual task only: assign authentic package pins and verify topology before certification
W078,Clock_Reset_Firmware,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,False,MISSING,Conceptual task only: assign authentic package pins and verify topology before certification
W079,Clock_Reset_Firmware,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,False,MISSING,Conceptual task only: assign authentic package pins and verify topology before certification
W080,Clock_Reset_Firmware,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,False,MISSING,Conceptual task only: assign authentic package pins and verify topology before certification
W081,Safety_Monitoring,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,False,MISSING,Conceptual task only: assign authentic package pins and verify topology before certification
W082,Safety_Monitoring,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,False,MISSING,Conceptual task only: assign authentic package pins and verify topology before certification
W083,Safety_Monitoring,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,False,MISSING,Conceptual task only: assign authentic package pins and verify topology before certification
W084,Safety_Monitoring,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,False,MISSING,Conceptual task only: assign authentic package pins and verify topology before certification
W085,Safety_Monitoring,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,False,MISSING,Conceptual task only: assign authentic package pins and verify topology before certification
W086,Safety_Monitoring,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,False,MISSING,Conceptual task only: assign authentic package pins and verify topology before certification
W087,Safety_Monitoring,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,False,MISSING,Conceptual task only: assign authentic package pins and verify topology before certification
W088,Safety_Monitoring,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,False,MISSING,Conceptual task only: assign authentic package pins and verify topology before certification
W089,Safety_Monitoring,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,False,MISSING,Conceptual task only: assign authentic package pins and verify topology before certification
W090,Safety_Monitoring,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,False,MISSING,Conceptual task only: assign authentic package pins and verify topology before certification
W091,Safety_Monitoring,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,False,MISSING,Conceptual task only: assign authentic package pins and verify topology before certification
W092,Safety_Monitoring,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,False,MISSING,Conceptual task only: assign authentic package pins and verify topology before certification
```

## Annex E. Original 20 unverified via tasks

Source file: `Data/Via_Placements_20_UNVERIFIED.csv`

```csv
via_id,net_id,x_mm,y_mm,from_layer,to_layer,verified,initial_state
V01,UNASSIGNED,TBD,TBD,UNASSIGNED,UNASSIGNED,False,MISSING
V02,UNASSIGNED,TBD,TBD,UNASSIGNED,UNASSIGNED,False,MISSING
V03,UNASSIGNED,TBD,TBD,UNASSIGNED,UNASSIGNED,False,MISSING
V04,UNASSIGNED,TBD,TBD,UNASSIGNED,UNASSIGNED,False,MISSING
V05,UNASSIGNED,TBD,TBD,UNASSIGNED,UNASSIGNED,False,MISSING
V06,UNASSIGNED,TBD,TBD,UNASSIGNED,UNASSIGNED,False,MISSING
V07,UNASSIGNED,TBD,TBD,UNASSIGNED,UNASSIGNED,False,MISSING
V08,UNASSIGNED,TBD,TBD,UNASSIGNED,UNASSIGNED,False,MISSING
V09,UNASSIGNED,TBD,TBD,UNASSIGNED,UNASSIGNED,False,MISSING
V10,UNASSIGNED,TBD,TBD,UNASSIGNED,UNASSIGNED,False,MISSING
V11,UNASSIGNED,TBD,TBD,UNASSIGNED,UNASSIGNED,False,MISSING
V12,UNASSIGNED,TBD,TBD,UNASSIGNED,UNASSIGNED,False,MISSING
V13,UNASSIGNED,TBD,TBD,UNASSIGNED,UNASSIGNED,False,MISSING
V14,UNASSIGNED,TBD,TBD,UNASSIGNED,UNASSIGNED,False,MISSING
V15,UNASSIGNED,TBD,TBD,UNASSIGNED,UNASSIGNED,False,MISSING
V16,UNASSIGNED,TBD,TBD,UNASSIGNED,UNASSIGNED,False,MISSING
V17,UNASSIGNED,TBD,TBD,UNASSIGNED,UNASSIGNED,False,MISSING
V18,UNASSIGNED,TBD,TBD,UNASSIGNED,UNASSIGNED,False,MISSING
V19,UNASSIGNED,TBD,TBD,UNASSIGNED,UNASSIGNED,False,MISSING
V20,UNASSIGNED,TBD,TBD,UNASSIGNED,UNASSIGNED,False,MISSING
```

## Annex F. Three seeded fault definitions

Source file: `Data/Faults_3.csv`

```csv
fault_id,affected,effect,repair
F01,I2C pullup resistor,Rise-time violation / possible invalid logic levels,"Replace incorrect resistor, verify electrical response"
F02,SPI SCK plated via,SCK not received by target peripheral,"Restore via connectivity, retest transactions"
F03,UART configuration,Baud and frame mismatch,"Correct endpoint firmware setting, verify decoded bytes"
```

## Annex G. Six objective source definitions

Source file: `Data/Objectives_6.csv`

```csv
objective,name,authoritative_gate
1,Assemble 70 components,Requires live verified netlist/measurement/firmware/safety evidence
2,Route 92 copper tasks and 20 vias,Requires live verified netlist/measurement/firmware/safety evidence
3,Commission I2C and SPI,Requires live verified netlist/measurement/firmware/safety evidence
4,Qualify UART and Multi-Bus Diagnostics,Requires live verified netlist/measurement/firmware/safety evidence
5,Recover three communication faults,Requires live verified netlist/measurement/firmware/safety evidence
6,W093 disconnection and fresh recovery,Requires live verified netlist/measurement/firmware/safety evidence
```

## Annex H. Critical W093 original JSON

```json
{
  "id": "W093",
  "source": "L100_MCU_U1.TRUSTED_COMM_HEALTH_OUT",
  "destination": "L100_CommSafetySupervisor_U9.TRUSTED_COMM_HEALTH_IN",
  "type": "removable digital watchdog jumper",
  "initial": "disconnected",
  "validation": "requires fresh changing heartbeat evidence and independent safety reset",
  "note": "Logical interface designation only; pin-level design not yet verified"
}
```

## Annex I. Materials source manifest

```json
{
  "PCB_FR4": {
    "base_color_rgba": [
      0.09,
      0.17,
      0.14,
      1
    ],
    "metallic": 0.15,
    "roughness": 0.83
  },
  "PCB_Soldermask": {
    "base_color_rgba": [
      0.028,
      0.19,
      0.13,
      1
    ],
    "metallic": 0.1,
    "roughness": 0.62
  },
  "Copper": {
    "base_color_rgba": [
      0.68,
      0.32,
      0.095,
      1
    ],
    "metallic": 0.83,
    "roughness": 0.34
  },
  "ENIG_Gold": {
    "base_color_rgba": [
      0.84,
      0.65,
      0.28,
      1
    ],
    "metallic": 0.9,
    "roughness": 0.24
  },
  "Solder_Tin": {
    "base_color_rgba": [
      0.59,
      0.64,
      0.68,
      1
    ],
    "metallic": 0.78,
    "roughness": 0.35
  },
  "IC_Epoxy": {
    "base_color_rgba": [
      0.044,
      0.05,
      0.058,
      1
    ],
    "metallic": 0.07,
    "roughness": 0.68
  },
  "IC_Marking": {
    "base_color_rgba": [
      0.72,
      0.76,
      0.79,
      1
    ],
    "metallic": 0.05,
    "roughness": 0.9
  },
  "Ceramic_Ivory": {
    "base_color_rgba": [
      0.84,
      0.82,
      0.7,
      1
    ],
    "metallic": 0.05,
    "roughness": 0.72
  },
  "Resistor_Black": {
    "base_color_rgba": [
      0.12,
      0.12,
      0.14,
      1
    ],
    "metallic": 0.1,
    "roughness": 0.65
  },
  "Steel": {
    "base_color_rgba": [
      0.38,
      0.42,
      0.46,
      1
    ],
    "metallic": 0.85,
    "roughness": 0.27
  },
  "Polymer_Sleeve": {
    "base_color_rgba": [
      0.22,
      0.24,
      0.28,
      1
    ],
    "metallic": 0.45,
    "roughness": 0.5
  },
  "Polymer_Top": {
    "base_color_rgba": [
      0.64,
      0.68,
      0.7,
      1
    ],
    "metallic": 0.76,
    "roughness": 0.3
  },
  "Copper_Winding": {
    "base_color_rgba": [
      0.71,
      0.34,
      0.12,
      1
    ],
    "metallic": 0.78,
    "roughness": 0.28
  },
  "Ferrite": {
    "base_color_rgba": [
      0.14,
      0.14,
      0.15,
      1
    ],
    "metallic": 0.1,
    "roughness": 0.7
  },
  "Connector_Black": {
    "base_color_rgba": [
      0.11,
      0.12,
      0.15,
      1
    ],
    "metallic": 0.08,
    "roughness": 0.5
  },
  "Plastic_Red": {
    "base_color_rgba": [
      0.7,
      0.08,
      0.07,
      1
    ],
    "metallic": 0.07,
    "roughness": 0.48
  },
  "LED_Green": {
    "base_color_rgba": [
      0.08,
      0.86,
      0.3,
      1
    ],
    "metallic": 0.03,
    "roughness": 0.25
  },
  "PCB_Silkscreen": {
    "base_color_rgba": [
      0.82,
      0.87,
      0.83,
      1
    ],
    "metallic": 0.01,
    "roughness": 0.85
  },
  "Dielectric": {
    "base_color_rgba": [
      0.34,
      0.27,
      0.15,
      1
    ],
    "metallic": 0.02,
    "roughness": 0.75
  },
  "Insulator": {
    "base_color_rgba": [
      0.95,
      0.94,
      0.86,
      1
    ],
    "metallic": 0.0,
    "roughness": 0.7
  },
  "Thermal_Ceramic": {
    "base_color_rgba": [
      0.45,
      0.47,
      0.51,
      1
    ],
    "metallic": 0.12,
    "roughness": 0.5
  }
}
```

## Annex J. Existing board-only FBX importer

Source file: `Unity/Editor/SparkLevel100BoardOnlyImporter.cs`

```csharp
#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
public static class SparkLevel100BoardOnlyImporter
{
    [MenuItem("Tools/Project Spark/Levels/100/Board Only/1. Build Empty DNX-8 PCB")]
    public static void BuildEmpty()
    {
        string[] paths=AssetDatabase.FindAssets("t:Model").Select(AssetDatabase.GUIDToAssetPath).Where(p=>p.Contains("L100_DNX8_Empty_FourLayer_PCB") && p.EndsWith(".fbx",StringComparison.OrdinalIgnoreCase)).ToArray();
        if(paths.Length!=1){Debug.LogError("[L100] Expected exactly one empty DNX8 board FBX; found "+paths.Length);return;}
        var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(paths[0]);if(prefab==null){Debug.LogError("[L100] FBX import failed");return;}
        var root=new GameObject("Level100_DNX8_Empty_Playable_Board");
        GameObject.Instantiate(prefab,root.transform).name="EMPTY_PCB_ART_ONLY";
        Selection.activeGameObject=root;
        Debug.Log("[L100] Empty board created. 70 placements, 92 routes, 20 vias and W093 remain uncompleted. Art-only; no certification.");
    }
    [MenuItem("Tools/Project Spark/Levels/100/Board Only/2. Validate FBX Board Import")]
    public static void Validate()
    {
        var paths=AssetDatabase.FindAssets("t:Model").Select(AssetDatabase.GUIDToAssetPath).Where(p=>p.Contains("_L100_") && p.EndsWith(".fbx",StringComparison.OrdinalIgnoreCase)).ToArray();
        Debug.Log("[L100] Found "+paths.Length+" matching modular FBX assets (expected 88); no Play Mode/electrical verification performed.");
    }
}
#endif
```

## Annex K. Existing PBR material installer

Source file: `Unity/Editor/SparkLevel100PBRMaterialInstaller.cs`

```csharp
#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
public static class SparkLevel100PBRMaterialInstaller
{
    [MenuItem("Tools/Project Spark/Levels/100/Board Only/3. Generate Native PBR Materials")]
    public static void Build()
    {
        var texPaths=AssetDatabase.FindAssets("t:Texture2D").Select(AssetDatabase.GUIDToAssetPath).Where(p=>p.Contains("/Textures/") && p.EndsWith("_Albedo.png",StringComparison.OrdinalIgnoreCase)).ToArray();
        var own=texPaths.Where(p=>p.Contains("Level100_DNX8") || p.Contains("ProjectSpark_Level100_DigitalNexus_BoardOnly_Realistic")).ToArray();
        if(own.Length==0){Debug.LogError("[L100] Textures not found. Import the whole archive together, and retain source root folder.");return;}
        string root=own[0].Substring(0,own[0].LastIndexOf("/Textures/",StringComparison.Ordinal));
        string folder=root+"/UnityGeneratedMaterials";
        if(!AssetDatabase.IsValidFolder(folder))AssetDatabase.CreateFolder(root,"UnityGeneratedMaterials");
        Shader shader=Shader.Find("Universal Render Pipeline/Lit")??Shader.Find("HDRP/Lit")??Shader.Find("Standard");
        if(shader==null){Debug.LogError("[L100] No supported Lit shader was found");return;}
        foreach(var albedoPath in own)
        {
            var name=Path.GetFileName(albedoPath).Replace("_Albedo.png","");
            var mat=new Material(shader){name="L100_"+name};
            var t=AssetDatabase.LoadAssetAtPath<Texture2D>(albedoPath);
            if(mat.HasProperty("_BaseMap"))mat.SetTexture("_BaseMap",t);
            if(mat.HasProperty("_MainTex"))mat.SetTexture("_MainTex",t);
            var mask=AssetDatabase.LoadAssetAtPath<Texture2D>(root+"/Textures/"+name+"_UnityMetallicSmoothness.png");
            if(mask!=null){if(mat.HasProperty("_MetallicGlossMap"))mat.SetTexture("_MetallicGlossMap",mask);if(mat.HasProperty("_MaskMap"))mat.SetTexture("_MaskMap",mask);}
            string dest=folder+"/L100_"+name+".mat";if(AssetDatabase.LoadAssetAtPath<Material>(dest)==null)AssetDatabase.CreateAsset(mat,dest);else UnityEngine.Object.DestroyImmediate(mat);
        }
        AssetDatabase.SaveAssets();AssetDatabase.Refresh();
        Debug.Log("[L100] Native materials generated; confirm FBX material remapping and render pipeline normal-map compatibility manually.");
    }
}
#endif
```

## Annex L. Source README and caveats

```markdown
# PROJECT SPARK LEVEL 100 — DIGITAL NEXUS / DNX-8

REAL MODULAR 3D ASSETS, BOARD ONLY — NO ROOM, PHYSICAL TOOLS, OR GAME UI.

## Contents
- 88 independent binary FBX 7400 assets + 88 matching GLBs
- Separate completed-board visual-only showcase FBX/GLB
- 70 individually placeable parts, 18 structural/fault/reference parts
- 210 x 145 mm four-layer PCB (models in METERS; the board is 0.210 x 0.145 m)
- Shared PBR source textures, basic physically based material definitions, Unity metallic/smoothness maps
- 92 concept-stage copper route tasks, 20 concept-stage vias; W093 jumper
- Component placement CSV and Unity editor art importer and material builder

## Unity
1. Copy the entire source directory under Assets, keeping Models_FBX, Textures, Data and Unity together.
2. Put included Unity scripts in an Editor folder (already arranged).
3. Tools > Project Spark > Levels > 100 > Board Only > 1. Build Empty DNX-8 PCB.
4. Tools > Project Spark > Levels > 100 > Board Only > 3. Generate Native PBR Materials.
5. Assign generated materials to matching FBX material names; review units, lights, normal maps and import warnings.
6. Use component placement CSV to create interactive targets and footprint anchors. No electrical solver has been implemented.

## Gameplay safeguards
- Start with 70 components UNINSTALLED, 92 copper tasks MISSING, 20 vias MISSING, W093 DISCONNECTED.
- Never place completed-board showcase under the playable hierarchy.
- Route and via tables are **NOT a pin-level netlist**; they contain UNASSIGNED endpoints intentionally.
- Require a verified pin-specific DNX-8 schematic, footprints, net IDs and MCU pin mux before claiming electrical validity.
- W093 is a communications-health signal, NOT a power rail or direct I2C/SPI/UART data wire.
- This package contains 3D art and source planning data, not working firmware or six certified objectives.

## Quality scope
Procedural original geometry with individual LQFP leads, gold pads, package leads and separate components. Shared PBR reference maps are *not* artist baked. FBX generated by a custom binary exporter; Unity import has not been tested. Real motherboard engineering would require precise chip selection, datasheets, PCB layout DRC, and professional finishing.
```
