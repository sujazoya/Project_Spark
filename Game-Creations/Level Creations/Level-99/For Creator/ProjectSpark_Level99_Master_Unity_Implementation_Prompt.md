# PROJECT SPARK — LEVEL 99: SILICON BRAIN
## MASTER UNITY IMPLEMENTATION PROMPT — BOARD-ONLY REALISTIC 3D + AUTHORITATIVE ELECTRONICS / FIRMWARE SIMULATION

**Mission:** Deliver a playable, maintainable, verified Level 99 in the existing Project Spark Unity project using the supplied Level 99 board-only asset kit. Preserve Levels 1–98 and all existing systems. Do not claim success or 6/6 until it passes actual Unity compilation, Play Mode, electrical-solver integration, and testing.

**Role:** Act as a senior Unity C# gameplay engineer, electrical simulation engineer, PCB gameplay architect, embedded-systems engineer, editor tooling developer, technical artist, and QA lead. Review the current source code and API contracts before writing integration code. Never invent an existing API. Implement new cohesive adapters where needed.

**Input artifact:** `ProjectSpark_Level99_SiliconBrain_BoardOnly_Realistic_Complete.zip` (supplied separately). Its Data, Models_FBX, Textures, and Unity helpers are the source of the asset manifest. If files are not accessible in the target workspace, request the archive or locate the actual imported copies; do not hallucinate paths.

### Fixed data contract

| Item | Grounded specification |
|---|---|
| Level / name | 99 — Silicon Brain |
| Board | SBC-7 embedded microcontroller board, 190 × 130 mm, nominal 1.6 mm thick |
| PCB stack-up | Four conductive layers; actual planes not electrically defined by decorative meshes |
| Models | 80 modular FBX + 80 matching GLB; assembled FBX/GLB reference only |
| Gameplay components | 62 individually placeable electronic parts |
| Missing copper tasks | W001–W082: 82 design-stage route requirements |
| Missing vias | V01–V18: 18 design-stage via requirements |
| Detachable safety link | W083, U1.APPLICATION_HEARTBEAT_OUT → U8.TRUSTED_HEARTBEAT_IN (conceptual ports) |
| Reference materials | 21 families, 126 shared reference maps + 1 silkscreen atlas |
| Recoverable faults | F01 crystal capacitor, F02 SPI via, F03 reset resistor |
| Objectives | 6 authoritative evidence-backed certifications |
| Scope | Board-only assets; reuse existing World-Space Canvas virtual inspection/repair UI |

**Asset-source blocker:** All 82 copper-route records have `SCHEMATIC_REQUIRED` contacts, `UNASSIGNED` nets, and `pin_verified=NO`. All 18 via records likewise have `UNASSIGNED` nets, unknown layer spans and no verified positions. This source kit is not an electrical schematic. Build and review a *verified pin-level schematic and netlist* before running continuity, short-circuit, power, boot, or objective certification. Never silently invent manufacturer pinouts and call them verified.


## 01. Strict scope, safe integration and preservation

1. Work inside the existing Project Spark Unity project. Inspect Unity version, scripting backend, active rendering pipeline, prefab/scene conventions, the existing PCB framework, `SparkTerminal`, electrical graph/solver, gameplay wire API, `LevelGamePlayManager`, mission evaluator, shop inventory, object-shelf and virtual repair systems.
2. Do not rewrite or duplicate global services. Preserve Levels 1–98, prior PCB levels, shared menus, save-state schemas and gameplay input bindings. Namespace new components consistently; keep Editor code under an `Editor` folder.
3. Do not create room, furniture, tools-as-FBX, multimeter/oscilloscope physical models, cameras or unrelated art. Mount the board inside the existing game-room level presentation system, maintaining electrical simulation scale independently from visual scale.
4. Never silently auto-place components, satisfy missing routes, fill vias, install W083 or mark calibration complete. The assembled reference FBX/GLB is barred from live gameplay roots.
5. Target **desktop mouse and mobile touch**, with accessible selection indicators, recoverable inputs, Undo/Redo where safe, pause and save/load.
6. Simulation is an educational fictional board, not hardware-design or safety certification. Keep voltage supplies virtual. Allow only authorized gameplay actions and validate them against authoritative game state.

## 02. Deliverables and output layout

Produce a clean Level 99 package and changeset: playable level asset / layout prefab, empty PCB visual hierarchy, 62 installable component prefabs/bindings, verified logical footprint/netlist asset, 82 route definitions, 18 via definitions, W083 jumper model and two functional terminal endpoints, processor/firmware runtime, embedded bus simulation, virtual repair UI hooks, objective evaluator, data-driven faults, editor builder, save data, automated tests, README and screenshot/Play Mode evidence. Use existing project naming patterns after inspection. Suggested Editor action: `Tools/Project Spark/Levels/Create Level 99`.

Save any resolved schematic as a separate approved source artifact with provenance (`Verified_Netlist_L99.csv`/ScriptableObject). Retain the *original* unverified `Copper_Routes_82_DESIGN_REQUIRED.csv` and `Via_Plan_18_DESIGN_REQUIRED.csv` unchanged for traceability; emit a mapping from each W/V ID to reviewed source/destination pins and PCB layers.

## 03. Import the exact source assets

Import the 80 modular FBXs and their material sources without renaming IDs. Preserve visual hierarchy and individual component selection, and instantiate only the empty board model on level start. The supplied art builder has menu `Tools/Project Spark/Levels/99/Board Only/1. Create EMPTY SBC-7 Board`; it searches for `63_L99_SBC7_Empty_FourLayer_PCB.fbx`. The supplied validation menu checks only FBX presence: it is not a wiring or gameplay validator.

The supplied PBR installer menu is `Tools/Project Spark/Levels/99/Board Only/3. Create Native PBR Materials`. Its script creates 21 Unity `.mat` files using the first found supported Lit shader and assigns color/metallic-mask/occlusion references, but **does not complete automatic FBX slot remapping or normal-map import configuration**. Verify and fix those steps in the final project, with explicit URP/HDRP/Built-in handling. Do not claim texture-map compatibility until checked in Unity.

Normalize units/axes: 0.190 × 0.130 m board outline, 0.0016 m thickness; CSV coordinates are in millimetres and must be converted once to metres (do not scale twice). Confirm model transforms and orientation by test placements. Functional circuits must not depend on any board art presentation scale or moving table preview.

## 04. Empty-board build and visual fidelity

At level start spawn bare FR-4/soldermask board and factory decorative geometry. Show unpopulated footprints, pad identifiers and silk labels. Do not include completed assembly in playable hierarchy. Pads/traces/vias must have independent logical identities and touch hit zones, not be inferred from coarse visual mesh triangles. Use contextual zoom, pin highlighting, depth-aware gizmos, and four-layer visualization with no physical duplication of player route completion.

Each installable part must have a unique `refdes`, expected footprint, intended pose, orientation restrictions, polarity where relevant, catalog entry, part status, and independent removal/replacement state. Realistic modeling is a visual foundation; U1's 64-lead package is not manufacturer-accurate CAD and its *logical pin assignment must be approved separately*.

## 05. Pin-level schematic reconciliation — HARD GATE

Before gameplay electronics, enumerate all devices and logical contacts; choose an internally consistent educational 32-bit MCU model and functional pin assignment, review MCU ground/VDD groups, clock pins, reset, SWD, GPIO, SPI flash pins and communications. For every W001–W082 route, specify start component-pin and pad ID, destination component-pin and pad ID, net, layer eligibility, track width/clearance, placement authority, electrical class, and task-specific completion evidence. For V01–V18, specify net, x/y in board coordinates, actual connected layers, drill/annular geometry, and allowed action.

Do not substitute `Wxx -> PADxx` with fabricated pad numbers and report it verified. Stop certification until every required route/via resolves to distinct, type-compatible logical contacts and the team has approved the map. Factory traces and internal planes must be derived from the same verified netlist and must not provide an alternate bypass that marks missing player routes as completed. Any shared nets must be explicitly justified. Build a DRC graph checker and prove no shorts between power domains, open grounds, dangling power pins, broken reset paths, or conflicts with W083.

For demonstration while the schematic is pending, allow **art-only preview/demo mode**, visibly labeled and disconnected from achievement certification.

## 06. PCB assembly game loop

Start with all 62 BOM items in appropriate inventory rather than already soldered onto board. Players select a part and intended footprint, align pin 1, rotate when supported, place, inspect, and use virtual solder action. Enforce values, wrong-footprint and wrong-polarity handling, visible placement feedback and real installed-contact transitions. Graph connectivity must include solder-joint health—not merely overlapping visual meshes. Mark objective 1 only after placement and solder evidence for all 62 parts, with zero unresolved misorientation/incorrect values.

## 07. Four-layer copper and via routing

Use unified PCB graph and drawing interactions: select logical pad, choose top/bottom exposed copper layer, route a path, and inspect DRC feedback. Respect internal GND/3V3 reference layers; these are visualization-only until explicitly defined and assigned to net nodes. Add vias only when the verified source plan permits and connect actual specified layers. Persist each W/V task separately. For 82 routes plus 18 vias, completion checks require direct path existence and route-specific evidence, not mere global connectedness through a factory path. Group as 14 power/ground, 14 clock/reset, 16 SWD/programming, 14 external SPI, 12 GPIO/I2C/UART, 12 monitoring/heartbeat. Keep W083 a 63rd? **No**—it is already one of the 62 BOM items and is **separate from W001–W082**.

## 08. Microcontroller power / clock / reset

Implement bounded educational simulation with protected 5 V input, finite 3.3 V regulator startup, decoupling, ground continuity, reset supervisor conditions, optional brownout and clock generation. Use `Y1` as nominal 8 MHz reference and a virtual PLL model generating 48 MHz only when the actual clock and configured parameters are valid. Report failure if clock does not start or settle. Implement reset hold/release under actual supply stability and supervisor behavior; do not 'boot' from a UI toggle.

## 09. Firmware programming and deterministic virtual runtime

Virtual SWD debugger/programmer is part of World-Space Canvas UI—no model required. Implement legitimate debug-session prerequisites: MCU supply, valid SWD net, clock/reset state, controller permission, upload image, checksum/signature validation as configured, target/board compatibility, stored firmware and safe reset. Provide at least: approved diagnostic v1.0, incompatible legacy version, corrupt image. Distinguish image integrity from application correctness. A bounded deterministic firmware state machine is sufficient; do not pretend to execute an entire Cortex-M instruction set unless implemented and tested.

Required CPU states: `POWER_OFF → POWER_RAMP → RESET_HELD → CLOCK_STARTING → BOOT_ROM → FIRMWARE_VERIFY → PERIPHERAL_INIT → APPLICATION_RUNNING`, with safe FAULT/RESET transitions. Track reason codes, monotonic run epoch, firmware image ID/hash, last success and peripheral state. Make reset and watchdog authority independent of animated readouts.

## 10. Peripheral diagnostics

Provide data-driven GPIO test, timer interrupt progression, SPI flash read/write verification on external U2, I²C transaction validation, and watchdog servicing. Simulate bus clocks, line status, data and error handling as coherent events rather than arbitrary success values. Board design specifies 8 MB SPI NOR flash, used for diagnostic storage, not an assumption that U2 stores mandatory boot firmware. Model internal program flash/application separately unless the eventual verified schematic says otherwise. Include logic-analyzer views sourced from actual event traces, not free-running decorative oscillations.

## 11. Independent safety and W083 recovery

`U1.APPLICATION_HEARTBEAT_OUT → W083 → U8.TRUSTED_HEARTBEAT_IN` is the **conceptual** function and must be assigned verified physical package contacts first. W083 starts uninstalled; player installs it during initial commissioning. Supervisor requires multiple fresh transitions at a configured expected cadence, rejects steady HIGH, steady LOW, stale edge samples, bad wiring and implausible activity. Its watchdog may persist if MCU is still executing, but protected outputs must be inhibited and fault latched after the configured detection window. Do not switch off real 3.3 V supply or invent a crystal problem just because this feedback is removed.

Objective 6 challenge: after 1–5 are historically certified, authorize supervised disconnect → actually remove W083 → observe lost trusted feedback and independent inhibition → reconnect physical jumper → reacquire fresh edge measurements and qualify timing → explicit reset → new full boot and peripheral diagnostic epoch → certify Objective 6. Reconnecting without fresh proof or reset must not pass. Prevent replaying old data/old test certificates. Treat watchdog correctness separately from MCU boot success.

## 12. Three seeded recoverable faults

**F01 — Y1/C1 oscillator capacitor:** Implement a configurable incorrect capacitor/damaged connection that degrades or blocks clock startup. Repair by identifying and replacing the faulty component; recalibrate and observe genuine startup.

**F02 — V11/SPI_DATA open via:** External SPI flash data net must be interrupted, causing bus transactions to fail while internal application may continue executing. Do not kill the CPU unless its own prerequisites fail. Repair via click/touch, recheck topology, rerun external-memory transactions.

**F03 — R_RESET defective pull-up:** Reset line remains low/noisy, blocking or intermittently restarting boot. Diagnose with live scope, replace resistor, verify supervisor release and fresh complete boot.

Maintain source fault identity/locations; `R_RESET` is a functional label until final schematic maps it to an exact R1–R19 refdes. Do not arbitrarily assign a refdes and claim it came from the source. F01/F03 positions and part values must be resolved during netlist/footprint review.

## 13. World-space virtual repair UI and VFX integration

Reuse existing PCB Virtual Studio tools and actual input interactions. Inspection tools: multimeter, oscilloscope, logic analyzer, net tracer, continuity, thermal, microscope, SWD debugger, firmware programmer. Repair tools: solder/desolder, component replacement, via/trace repair, cleaning, calibration. Prefer click/touch on the real installed component's colliders or logical pad markers; avoid requiring physical tool FBXs.

Repair pipeline: target selection → authority/precondition checks (board safe, power requirements) → repair action → bounded contact VFX (solder glow/sparks/heat/diagnostic pulses) → authoritative mutation → fresh electrical/peripheral remeasurement → objective evidence. VFX failure or success never equals electrical repair success. Keep all demo-only waveforms clearly labeled and prevented from influencing objective certification. UI must be readable at board zoom, support mouse/touch and coexist with existing level scene.

## 14. Real-time state and objective evidence

Define a single authoritative state snapshot keyed by board version, physics step and repair/boot epoch. Each objective stores required evidence objects and revision/age thresholds. Objective 1 = 62 correct placements; 2 = 82 validated routes and 18 validated vias; 3 = 3.3 V qualified + 8 MHz reference + 48 MHz configured CPU + reset + programmed verified firmware + qualified boot; 4 = GPIO, timers, SPI, I2C and watchdog tests; 5 = F01–F03 demonstrably injected/repaired + post-repair boot/diagnostics; 6 = supervised W083 disconnect/inhibit/reconnect/fresh heartbeat/reset/new boot/test. Integrate with actual `LevelGamePlayManager` and the real evaluator after inspecting signatures.

Do not use unchecked booleans, visual renderer state, counters manually set to target, or UI success buttons as evidence. Preserve historical objective certification unless project rules explicitly specify recertification; requiring fresh run for 5/6 must not retroactively erase legitimately completed earlier objectives. Display reasons for incomplete objectives and new fault evidence.

## 15. Determinism, save/load, and replay resistance

Save component inventory/placement, solder-joint health, wires and vias, schematic version, fault history, power/clock state, firmware image and flash content, current boot epoch, watchdog latch, calibration evidence and objective records. Restore in a deterministic safe order; cached UI values are not authoritative. Prevent older certificate timestamps from validating post-repair actions. Keep random fault/noise simulation seedable and testable. If the save uses an older verified netlist version, migrate or require explicit requalification rather than silently changing physical connectivity.

## 16. Build process and editor tooling

One-click command `Tools/Project Spark/Levels/Create Level 99`: validate exact assets; locate world-space game room root; place empty PCB; create non-live component preview/caddy; add 62 authoritative placement definitions; import the resolved netlist and route/via tasks; add simulation and safety components; bind UI endpoints; register objectives with real manager; optionally generate demo scenes separately. Preflight must refuse to enable live certification when any W/V item is unresolved. Provide detailed console warnings and a report rather than crashing.

Use actual prefab/ScriptableObject/scene workflow from the current project. Provide idempotent build/refresh and Undo support for Editor actions. Never destroy user-authored existing level content without an explicit backup.

## 17. Rendering, materials and art acceptance

Remap 21 PBR families to appropriate native materials, verify FBX axes/normals, small lead and pin shading, PCB 190×130×1.6 mm, and depth rendering of labels/pads. Keep a high-contrast inspect mode for microscopic targets. Do not rely on emissive copper to convey genuine connections. Material installer source uses a fallback shader choice and may not set normal textures; verify shader-specific properties and normal import. Keep performance reasonable: batching for static factory board, instancing for repeating passives where interaction bindings permit, colliders only for selected targets, and occlusion/culling of hidden layer visuals.

## 18. Security and robust input

Do not let tool UI call certification setters. All placement/repair actions go through validated gameplay command handlers; maintain tool mode, raycast filtering and board-target ownership. Prevent repair of unrelated level objects. For programmable firmware, use only safe curated virtual images/no arbitrary code execution. Clamp simulation time steps and malformed saved data; prevent negative currents, NaNs, duplicated installed part IDs or ghost routes.

## 19. Final release criteria

A completed mission must start from an empty board, allow all correct assembly/routing actions, power the MCU, program approved firmware, pass peripheral diagnostics, recover the three faults, execute the W083 disconnection/requalification, and reach **6/6 only through authoritative gameplay evidence**. Do not use the assembled reference art in the live scene. Do not claim Unity compile/test success without logs and captured evidence.


## 20. Engineering acceptance test matrix

Run every test against the real Unity scene or a deterministic play-test harness. An offline FBX/ZIP check is insufficient.


| Test ID | Category | Expected acceptance condition |
|---|---|---|
| AT99-001 | Assets | 80 modular FBXs resolve and no reference FBX in live scene |
| AT99-002 | Assets | Empty PCB measured 190 x 130 mm and 1.6 mm nominal |
| AT99-003 | Assets | 62 BOM refs have correct model mapping and unique placement |
| AT99-004 | Assets | Material 21 families remapped and normal maps imported |
| AT99-005 | Assets | Source file data stays unchanged and provenance recorded |
| AT99-006 | Assembly | Start state 0 of 62 installed |
| AT99-007 | Assembly | All 62 parts placed at correct footprints |
| AT99-008 | Assembly | Incorrect footprint rejected |
| AT99-009 | Assembly | Wrong rotation/pin one rejected |
| AT99-010 | Assembly | Solder health required for electrical contact |
| AT99-011 | Assembly | Remove/replace preserves correct inventory |
| AT99-012 | Assembly | Duplicate refdes cannot install twice |
| AT99-013 | Assembly | W083 counted exactly once in BOM |
| AT99-014 | Netlist | 82 routes all reviewed against pin-level netlist |
| AT99-015 | Netlist | 18 vias all reviewed against nets/layers |
| AT99-016 | Netlist | No unresolved SCHEMATIC_REQUIRED contact in live mode |
| AT99-017 | Netlist | No unassigned vias in live mode |
| AT99-018 | Netlist | Cross-domain short rejected |
| AT99-019 | Netlist | Ground return continuity verified |
| AT99-020 | Netlist | Factory copper cannot bypass incomplete W routes |
| AT99-021 | Netlist | Visual plane mesh cannot create logic connection |
| AT99-022 | Netlist | Real component solder state affects circuit graph |
| AT99-023 | Netlist | Player can route in permitted copper layers |
| AT99-024 | Netlist | Via spans valid actual layers |
| AT99-025 | Netlist | Missing route prevents related objective |
| AT99-026 | Netlist | Missing via prevents related objective |
| AT99-027 | Netlist | Invalid trace clearance rejected |
| AT99-028 | Power | Power-off prevents CPU execution |
| AT99-029 | Power | 5V input protected |
| AT99-030 | Power | 3.3V regulator startup finite |
| AT99-031 | Power | Brownout triggers reset |
| AT99-032 | Power | Power returns do not float |
| AT99-033 | Clock | 8 MHz oscillator only valid with correct hardware |
| AT99-034 | Clock | 48 MHz PLL only after proper configuration |
| AT99-035 | Clock | Clock startup failure blocks boot |
| AT99-036 | Reset | Reset held blocks boot |
| AT99-037 | Reset | Correct reset release advances boot |
| AT99-038 | Reset | Noisy reset emits explicit reason |
| AT99-039 | Firmware | SWD access requires connectivity |
| AT99-040 | Firmware | Approved image can be uploaded and verified |
| AT99-041 | Firmware | Corrupt image rejected |
| AT99-042 | Firmware | Legacy incompatible image not certified |
| AT99-043 | Firmware | Hash validity alone does not certify runtime |
| AT99-044 | Firmware | Processor state machine transitions deterministic |
| AT99-045 | Firmware | Boot epoch increments on fresh boot |
| AT99-046 | Firmware | Internal boot flash distinguished from SPI data flash |
| AT99-047 | Peripherals | GPIO read/write test reacts to graph |
| AT99-048 | Peripherals | Timer test uses actual virtual runtime |
| AT99-049 | Peripherals | SPI flash read/write passes on healthy via |
| AT99-050 | Peripherals | SPI fails on open V11 |
| AT99-051 | Peripherals | I2C status reflects bus transactions |
| AT99-052 | Peripherals | Logic analyzer displays authoritative events |
| AT99-053 | Peripherals | No decorative waveform can pass objectives |
| AT99-054 | Faults | F01 oscillator fault changes clock startup |
| AT99-055 | Faults | F01 repair restores measured clock |
| AT99-056 | Faults | F02 open via breaks external SPI only |
| AT99-057 | Faults | F02 repair requalifies via and SPI |
| AT99-058 | Faults | F03 reset resistor blocks/restarts boot |
| AT99-059 | Faults | F03 repair yields fresh boot |
| AT99-060 | Faults | Objective 5 requires recovery after all three |
| AT99-061 | W083 | W083 starts physically absent |
| AT99-062 | W083 | W083 must be installed before normal commissioning |
| AT99-063 | W083 | Steady HIGH not accepted as heartbeat |
| AT99-064 | W083 | Steady LOW not accepted as heartbeat |
| AT99-065 | W083 | Lost/stale transitions latch independent safety fault |
| AT99-066 | W083 | Disconnect removes protected output authorization |
| AT99-067 | W083 | Disconnect does not fake loss of 3.3V or oscillator |
| AT99-068 | W083 | Reconnect alone does not release latch |
| AT99-069 | W083 | Multiple fresh transitions required |
| AT99-070 | W083 | Explicit reset required |
| AT99-071 | W083 | New boot and peripherals epoch required for objective 6 |
| AT99-072 | UI | Mouse selection works for small pads |
| AT99-073 | UI | Touch selection works for small pads |
| AT99-074 | UI | Repair VFX at actual contact point |
| AT99-075 | UI | Art-only demo clearly separated from live certification |
| AT99-076 | UI | No untrusted UI action mutates certification |
| AT99-077 | UI | Existing room and other levels unchanged |
| AT99-078 | Lifecycle | Save/load preserves topology and firmware |
| AT99-079 | Lifecycle | Save/load preserves fault/recovery evidence |
| AT99-080 | Lifecycle | Old evidence cannot certify new post-repair run |
| AT99-081 | Lifecycle | Create Level 99 editor action idempotent |
| AT99-082 | Lifecycle | Unresolved netlist blocks live certification |
| AT99-083 | Lifecycle | Existing Levels 1–98 regression pass |
| AT99-084 | Lifecycle | Genuine six objectives reach 6/6 |
| AT99-085 | Lifecycle | Build compiles without errors |
| AT99-086 | Lifecycle | Play Mode smoke test completes with evidence |


**Total proposed tests: 86.** Mark PASS/FAIL/SKIP with evidence; no invented success reports.


## 21. Implementation order and risk gates

1. Inspect and map real Project Spark APIs, source layout, Unity version, UI and electrical graph.
2. Import art, build only empty PCB and verify scaling/materials.
3. Resolve and approve component-level schematic and 82+18 logical routing definitions (blocking gate).
4. Wire assembly/inventory/solder simulation and route/via graph.
5. Add virtual 5 V/3.3 V power, 8 MHz oscillator, 48 MHz clock model, reset and MCU boot state machine.
6. Add programming, firmware image validation and the bounded virtual execution engine.
7. Add GPIO/timer/SPI/I2C diagnostic behaviors and World-Space Canvas panels.
8. Implement F01/F02/F03 fault injection, PCB repair targeting, localized effects.
9. Implement independent W083 heartbeat supervisor and full supervised recovery proof.
10. Bind six evidence-based objectives, persistence, QA tests and 6/6.

Report blockers clearly; if some exact current API signatures are inaccessible, describe pending integration points rather than manufacturing stand-in API claims.

## 22. Code quality requirements

Use strict typing, documented state transitions, clear ownership of authoritative state, data-driven board definitions, event subscriptions with proper lifetime cleanup and stable IDs. Avoid per-frame allocations in waveform rendering; cap diagnostic buffers. Each new class must have a single responsibility and use namespaces aligned with the actual codebase. Add test utilities and Editor-only assembly code separately. Include migration notes when introducing new save-state fields.

## 23. Completion report format

At delivery, provide: changed/created files, exact Unity menu actions, netlist verification status, class/APIs integrated, model/material import findings, build/compiler results, Play Mode testing results, AT99 test matrix, screenshots or logs, missing work, and the actual authoritative 6/6 state. If the verified netlist, Unity project or test environment is absent, state NOT IMPLEMENTED/NOT VERIFIED for affected items.

---


## ANNEX A. Exact 80-model source manifest

Source: `Data/Model_Manifest_80.csv`. Preserve the original source unchanged.

```csv
asset_id,component_name,category,fbx,glb,x_mm,y_mm,engineer_note
1,U1,IC,01_L99_U1.fbx,01_L99_U1.glb,0,2,Initially uninstalled
2,U2,IC,02_L99_U2.fbx,02_L99_U2.glb,36,25,Initially uninstalled
3,U3,IC,03_L99_U3.fbx,03_L99_U3.glb,62,24,Initially uninstalled
4,U4,IC,04_L99_U4.fbx,04_L99_U4.glb,-56,21,Initially uninstalled
5,U5,IC,05_L99_U5.fbx,05_L99_U5.glb,-36,-26,Initially uninstalled
6,U6,IC,06_L99_U6.fbx,06_L99_U6.glb,32,-29,Initially uninstalled
7,U7,IC,07_L99_U7.fbx,07_L99_U7.glb,62,-29,Initially uninstalled
8,U8,IC,08_L99_U8.fbx,08_L99_U8.glb,-60,-19,Initially uninstalled
9,R1,Resistor,09_L99_R1.fbx,09_L99_R1.glb,-73,-47,Initially uninstalled
10,R2,Resistor,10_L99_R2.fbx,10_L99_R2.glb,-49,-47,Initially uninstalled
11,R3,Resistor,11_L99_R3.fbx,11_L99_R3.glb,-25,-47,Initially uninstalled
12,R4,Resistor,12_L99_R4.fbx,12_L99_R4.glb,-1,-47,Initially uninstalled
13,R5,Resistor,13_L99_R5.fbx,13_L99_R5.glb,23,-47,Initially uninstalled
14,R6,Resistor,14_L99_R6.fbx,14_L99_R6.glb,47,-47,Initially uninstalled
15,R7,Resistor,15_L99_R7.fbx,15_L99_R7.glb,71,-47,Initially uninstalled
16,R8,Resistor,16_L99_R8.fbx,16_L99_R8.glb,-73,-36,Initially uninstalled
17,R9,Resistor,17_L99_R9.fbx,17_L99_R9.glb,-49,-36,Initially uninstalled
18,R10,Resistor,18_L99_R10.fbx,18_L99_R10.glb,-25,-36,Initially uninstalled
19,R11,Resistor,19_L99_R11.fbx,19_L99_R11.glb,-1,-36,Initially uninstalled
20,R12,Resistor,20_L99_R12.fbx,20_L99_R12.glb,23,-36,Initially uninstalled
21,R13,Resistor,21_L99_R13.fbx,21_L99_R13.glb,47,-36,Initially uninstalled
22,R14,Resistor,22_L99_R14.fbx,22_L99_R14.glb,71,-36,Initially uninstalled
23,R15,Resistor,23_L99_R15.fbx,23_L99_R15.glb,-73,-25,Initially uninstalled
24,R16,Resistor,24_L99_R16.fbx,24_L99_R16.glb,-49,-25,Initially uninstalled
25,R17,Resistor,25_L99_R17.fbx,25_L99_R17.glb,-25,-25,Initially uninstalled
26,R18,Resistor,26_L99_R18.fbx,26_L99_R18.glb,-1,-25,Initially uninstalled
27,R19,Resistor,27_L99_R19.fbx,27_L99_R19.glb,23,-25,Initially uninstalled
28,C1,MLCC,28_L99_C1.fbx,28_L99_C1.glb,-75,43,Initially uninstalled
29,C2,MLCC,29_L99_C2.fbx,29_L99_C2.glb,-49,43,Initially uninstalled
30,C3,MLCC,30_L99_C3.fbx,30_L99_C3.glb,-23,43,Initially uninstalled
31,C4,MLCC,31_L99_C4.fbx,31_L99_C4.glb,3,43,Initially uninstalled
32,C5,MLCC,32_L99_C5.fbx,32_L99_C5.glb,29,43,Initially uninstalled
33,C6,MLCC,33_L99_C6.fbx,33_L99_C6.glb,55,43,Initially uninstalled
34,C7,MLCC,34_L99_C7.fbx,34_L99_C7.glb,-75,51,Initially uninstalled
35,C8,MLCC,35_L99_C8.fbx,35_L99_C8.glb,-49,51,Initially uninstalled
36,C9,MLCC,36_L99_C9.fbx,36_L99_C9.glb,-23,51,Initially uninstalled
37,C10,MLCC,37_L99_C10.fbx,37_L99_C10.glb,3,51,Initially uninstalled
38,C11,MLCC,38_L99_C11.fbx,38_L99_C11.glb,29,51,Initially uninstalled
39,C12,MLCC,39_L99_C12.fbx,39_L99_C12.glb,55,51,Initially uninstalled
40,C13,MLCC,40_L99_C13.fbx,40_L99_C13.glb,-75,59,Initially uninstalled
41,C14,MLCC,41_L99_C14.fbx,41_L99_C14.glb,-49,59,Initially uninstalled
42,C15,MLCC,42_L99_C15.fbx,42_L99_C15.glb,-23,59,Initially uninstalled
43,C16,MLCC,43_L99_C16.fbx,43_L99_C16.glb,3,59,Initially uninstalled
44,C17,MLCC,44_L99_C17.fbx,44_L99_C17.glb,29,59,Initially uninstalled
45,C18,MLCC,45_L99_C18.fbx,45_L99_C18.glb,55,59,Initially uninstalled
46,Y1,Crystal,46_L99_Y1.fbx,46_L99_Y1.glb,-33,13,Initially uninstalled
47,J1,Connector,47_L99_J1.fbx,47_L99_J1.glb,-81,30,Initially uninstalled
48,J2,Connector,48_L99_J2.fbx,48_L99_J2.glb,81,31,Initially uninstalled
49,J3,Connector,49_L99_J3.fbx,49_L99_J3.glb,81,-24,Initially uninstalled
50,J4,Connector,50_L99_J4.fbx,50_L99_J4.glb,-79,-29,Initially uninstalled
51,LED1,LED,51_L99_LED1.fbx,51_L99_LED1.glb,15,-53,Initially uninstalled
52,LED2,LED,52_L99_LED2.fbx,52_L99_LED2.glb,30,-53,Initially uninstalled
53,LED3,LED,53_L99_LED3.fbx,53_L99_LED3.glb,45,-53,Initially uninstalled
54,LED4,LED,54_L99_LED4.fbx,54_L99_LED4.glb,60,-53,Initially uninstalled
55,SW1,Switch,55_L99_SW1.fbx,55_L99_SW1.glb,-66,-6,Initially uninstalled
56,SW2,Switch,56_L99_SW2.fbx,56_L99_SW2.glb,-66,5,Initially uninstalled
57,D1,Protection,57_L99_D1.fbx,57_L99_D1.glb,-72,51,Initially uninstalled
58,D2,Protection,58_L99_D2.fbx,58_L99_D2.glb,-57,51,Initially uninstalled
59,W083,Jumper,59_L99_W083.fbx,59_L99_W083.glb,59,-6,Initially uninstalled
60,FB1,FerriteBead,60_L99_FB1.fbx,60_L99_FB1.glb,-20,51,Initially uninstalled
61,FB2,FerriteBead,61_L99_FB2.fbx,61_L99_FB2.glb,-2,51,Initially uninstalled
62,FB3,FerriteBead,62_L99_FB3.fbx,62_L99_FB3.glb,16,51,Initially uninstalled
63,SBC7_Empty_FourLayer_PCB,BoardDetail,63_L99_SBC7_Empty_FourLayer_PCB.fbx,63_L99_SBC7_Empty_FourLayer_PCB.glb,0,0,ASSEMBLED REFERENCE ONLY
64,L1_Top_Signal_Reference,BoardDetail,64_L99_L1_Top_Signal_Reference.fbx,64_L99_L1_Top_Signal_Reference.glb,0,0,ASSEMBLED REFERENCE ONLY
65,L2_Ground_Plane_Reference,BoardDetail,65_L99_L2_Ground_Plane_Reference.fbx,65_L99_L2_Ground_Plane_Reference.glb,0,0,ASSEMBLED REFERENCE ONLY
66,L3_3V3_Power_Plane_Reference,BoardDetail,66_L99_L3_3V3_Power_Plane_Reference.fbx,66_L99_L3_3V3_Power_Plane_Reference.glb,0,0,ASSEMBLED REFERENCE ONLY
67,L4_Bottom_Signal_Reference,BoardDetail,67_L99_L4_Bottom_Signal_Reference.fbx,67_L99_L4_Bottom_Signal_Reference.glb,0,0,ASSEMBLED REFERENCE ONLY
68,Plated_Through_Via,BoardDetail,68_L99_Plated_Through_Via.fbx,68_L99_Plated_Through_Via.glb,0,0,ASSEMBLED REFERENCE ONLY
69,Microvia_L1_L2,BoardDetail,69_L99_Microvia_L1_L2.fbx,69_L99_Microvia_L1_L2.glb,0,0,ASSEMBLED REFERENCE ONLY
70,Factory_Copper_Reference,BoardDetail,70_L99_Factory_Copper_Reference.fbx,70_L99_Factory_Copper_Reference.glb,0,0,ASSEMBLED REFERENCE ONLY
71,SWD_Debug_TestPad,BoardDetail,71_L99_SWD_Debug_TestPad.fbx,71_L99_SWD_Debug_TestPad.glb,0,0,ASSEMBLED REFERENCE ONLY
72,Crystal_ProbePad,BoardDetail,72_L99_Crystal_ProbePad.fbx,72_L99_Crystal_ProbePad.glb,0,0,ASSEMBLED REFERENCE ONLY
73,SPI_ProbePad,BoardDetail,73_L99_SPI_ProbePad.fbx,73_L99_SPI_ProbePad.glb,0,0,ASSEMBLED REFERENCE ONLY
74,Via_Repair_Segment,BoardDetail,74_L99_Via_Repair_Segment.fbx,74_L99_Via_Repair_Segment.glb,0,0,ASSEMBLED REFERENCE ONLY
75,Solder_Bridge_Repair,BoardDetail,75_L99_Solder_Bridge_Repair.fbx,75_L99_Solder_Bridge_Repair.glb,0,0,ASSEMBLED REFERENCE ONLY
76,Factory_Silkscreen_Reference,BoardDetail,76_L99_Factory_Silkscreen_Reference.fbx,76_L99_Factory_Silkscreen_Reference.glb,0,0,ASSEMBLED REFERENCE ONLY
77,Crystal_Fault_Overlay,BoardDetail,77_L99_Crystal_Fault_Overlay.fbx,77_L99_Crystal_Fault_Overlay.glb,0,0,ASSEMBLED REFERENCE ONLY
78,SPI_Open_Via_Overlay,BoardDetail,78_L99_SPI_Open_Via_Overlay.fbx,78_L99_SPI_Open_Via_Overlay.glb,0,0,ASSEMBLED REFERENCE ONLY
79,Reset_Network_Fault_Overlay,BoardDetail,79_L99_Reset_Network_Fault_Overlay.fbx,79_L99_Reset_Network_Fault_Overlay.glb,0,0,ASSEMBLED REFERENCE ONLY
80,Pin1_Alignment_Reference,BoardDetail,80_L99_Pin1_Alignment_Reference.fbx,80_L99_Pin1_Alignment_Reference.glb,0,0,ASSEMBLED REFERENCE ONLY
```


## ANNEX B. Exact 62-component BOM and footprint coordinates

Source: `Data/BOM_62_Component_Placements.csv`. Preserve the original source unchanged.

```csv
refdes,asset_fbx,package_family,x_mm,y_mm,rotation_deg,initially_installed
U1,01_L99_U1.fbx,IC,0,2,0,NO
U2,02_L99_U2.fbx,IC,36,25,0,NO
U3,03_L99_U3.fbx,IC,62,24,0,NO
U4,04_L99_U4.fbx,IC,-56,21,0,NO
U5,05_L99_U5.fbx,IC,-36,-26,0,NO
U6,06_L99_U6.fbx,IC,32,-29,0,NO
U7,07_L99_U7.fbx,IC,62,-29,0,NO
U8,08_L99_U8.fbx,IC,-60,-19,0,NO
R1,09_L99_R1.fbx,Resistor,-73,-47,0,NO
R2,10_L99_R2.fbx,Resistor,-49,-47,0,NO
R3,11_L99_R3.fbx,Resistor,-25,-47,0,NO
R4,12_L99_R4.fbx,Resistor,-1,-47,0,NO
R5,13_L99_R5.fbx,Resistor,23,-47,0,NO
R6,14_L99_R6.fbx,Resistor,47,-47,0,NO
R7,15_L99_R7.fbx,Resistor,71,-47,0,NO
R8,16_L99_R8.fbx,Resistor,-73,-36,0,NO
R9,17_L99_R9.fbx,Resistor,-49,-36,0,NO
R10,18_L99_R10.fbx,Resistor,-25,-36,0,NO
R11,19_L99_R11.fbx,Resistor,-1,-36,0,NO
R12,20_L99_R12.fbx,Resistor,23,-36,0,NO
R13,21_L99_R13.fbx,Resistor,47,-36,0,NO
R14,22_L99_R14.fbx,Resistor,71,-36,0,NO
R15,23_L99_R15.fbx,Resistor,-73,-25,0,NO
R16,24_L99_R16.fbx,Resistor,-49,-25,0,NO
R17,25_L99_R17.fbx,Resistor,-25,-25,0,NO
R18,26_L99_R18.fbx,Resistor,-1,-25,0,NO
R19,27_L99_R19.fbx,Resistor,23,-25,0,NO
C1,28_L99_C1.fbx,MLCC,-75,43,0,NO
C2,29_L99_C2.fbx,MLCC,-49,43,0,NO
C3,30_L99_C3.fbx,MLCC,-23,43,0,NO
C4,31_L99_C4.fbx,MLCC,3,43,0,NO
C5,32_L99_C5.fbx,MLCC,29,43,0,NO
C6,33_L99_C6.fbx,MLCC,55,43,0,NO
C7,34_L99_C7.fbx,MLCC,-75,51,0,NO
C8,35_L99_C8.fbx,MLCC,-49,51,0,NO
C9,36_L99_C9.fbx,MLCC,-23,51,0,NO
C10,37_L99_C10.fbx,MLCC,3,51,0,NO
C11,38_L99_C11.fbx,MLCC,29,51,0,NO
C12,39_L99_C12.fbx,MLCC,55,51,0,NO
C13,40_L99_C13.fbx,MLCC,-75,59,0,NO
C14,41_L99_C14.fbx,MLCC,-49,59,0,NO
C15,42_L99_C15.fbx,MLCC,-23,59,0,NO
C16,43_L99_C16.fbx,MLCC,3,59,0,NO
C17,44_L99_C17.fbx,MLCC,29,59,0,NO
C18,45_L99_C18.fbx,MLCC,55,59,0,NO
Y1,46_L99_Y1.fbx,Crystal,-33,13,0,NO
J1,47_L99_J1.fbx,Connector,-81,30,0,NO
J2,48_L99_J2.fbx,Connector,81,31,0,NO
J3,49_L99_J3.fbx,Connector,81,-24,0,NO
J4,50_L99_J4.fbx,Connector,-79,-29,0,NO
LED1,51_L99_LED1.fbx,LED,15,-53,0,NO
LED2,52_L99_LED2.fbx,LED,30,-53,0,NO
LED3,53_L99_LED3.fbx,LED,45,-53,0,NO
LED4,54_L99_LED4.fbx,LED,60,-53,0,NO
SW1,55_L99_SW1.fbx,Switch,-66,-6,0,NO
SW2,56_L99_SW2.fbx,Switch,-66,5,0,NO
D1,57_L99_D1.fbx,Protection,-72,51,0,NO
D2,58_L99_D2.fbx,Protection,-57,51,0,NO
W083,59_L99_W083.fbx,Jumper,59,-6,0,NO
FB1,60_L99_FB1.fbx,FerriteBead,-20,51,0,NO
FB2,61_L99_FB2.fbx,FerriteBead,-2,51,0,NO
FB3,62_L99_FB3.fbx,FerriteBead,16,51,0,NO
```


## ANNEX C. Original 82 route task records — UNVERIFIED

Source: `Data/Copper_Routes_82_DESIGN_REQUIRED.csv`. Preserve the original source unchanged.

```csv
route_id,group,source_contact,destination_contact,net,initial_state,pin_verified
W001,PowerGroundDecoupling,SCHEMATIC_REQUIRED,SCHEMATIC_REQUIRED,UNASSIGNED,INCOMPLETE,NO
W002,PowerGroundDecoupling,SCHEMATIC_REQUIRED,SCHEMATIC_REQUIRED,UNASSIGNED,INCOMPLETE,NO
W003,PowerGroundDecoupling,SCHEMATIC_REQUIRED,SCHEMATIC_REQUIRED,UNASSIGNED,INCOMPLETE,NO
W004,PowerGroundDecoupling,SCHEMATIC_REQUIRED,SCHEMATIC_REQUIRED,UNASSIGNED,INCOMPLETE,NO
W005,PowerGroundDecoupling,SCHEMATIC_REQUIRED,SCHEMATIC_REQUIRED,UNASSIGNED,INCOMPLETE,NO
W006,PowerGroundDecoupling,SCHEMATIC_REQUIRED,SCHEMATIC_REQUIRED,UNASSIGNED,INCOMPLETE,NO
W007,PowerGroundDecoupling,SCHEMATIC_REQUIRED,SCHEMATIC_REQUIRED,UNASSIGNED,INCOMPLETE,NO
W008,PowerGroundDecoupling,SCHEMATIC_REQUIRED,SCHEMATIC_REQUIRED,UNASSIGNED,INCOMPLETE,NO
W009,PowerGroundDecoupling,SCHEMATIC_REQUIRED,SCHEMATIC_REQUIRED,UNASSIGNED,INCOMPLETE,NO
W010,PowerGroundDecoupling,SCHEMATIC_REQUIRED,SCHEMATIC_REQUIRED,UNASSIGNED,INCOMPLETE,NO
W011,PowerGroundDecoupling,SCHEMATIC_REQUIRED,SCHEMATIC_REQUIRED,UNASSIGNED,INCOMPLETE,NO
W012,PowerGroundDecoupling,SCHEMATIC_REQUIRED,SCHEMATIC_REQUIRED,UNASSIGNED,INCOMPLETE,NO
W013,PowerGroundDecoupling,SCHEMATIC_REQUIRED,SCHEMATIC_REQUIRED,UNASSIGNED,INCOMPLETE,NO
W014,PowerGroundDecoupling,SCHEMATIC_REQUIRED,SCHEMATIC_REQUIRED,UNASSIGNED,INCOMPLETE,NO
W015,OscillatorPLLReset,SCHEMATIC_REQUIRED,SCHEMATIC_REQUIRED,UNASSIGNED,INCOMPLETE,NO
W016,OscillatorPLLReset,SCHEMATIC_REQUIRED,SCHEMATIC_REQUIRED,UNASSIGNED,INCOMPLETE,NO
W017,OscillatorPLLReset,SCHEMATIC_REQUIRED,SCHEMATIC_REQUIRED,UNASSIGNED,INCOMPLETE,NO
W018,OscillatorPLLReset,SCHEMATIC_REQUIRED,SCHEMATIC_REQUIRED,UNASSIGNED,INCOMPLETE,NO
W019,OscillatorPLLReset,SCHEMATIC_REQUIRED,SCHEMATIC_REQUIRED,UNASSIGNED,INCOMPLETE,NO
W020,OscillatorPLLReset,SCHEMATIC_REQUIRED,SCHEMATIC_REQUIRED,UNASSIGNED,INCOMPLETE,NO
W021,OscillatorPLLReset,SCHEMATIC_REQUIRED,SCHEMATIC_REQUIRED,UNASSIGNED,INCOMPLETE,NO
W022,OscillatorPLLReset,SCHEMATIC_REQUIRED,SCHEMATIC_REQUIRED,UNASSIGNED,INCOMPLETE,NO
W023,OscillatorPLLReset,SCHEMATIC_REQUIRED,SCHEMATIC_REQUIRED,UNASSIGNED,INCOMPLETE,NO
W024,OscillatorPLLReset,SCHEMATIC_REQUIRED,SCHEMATIC_REQUIRED,UNASSIGNED,INCOMPLETE,NO
W025,OscillatorPLLReset,SCHEMATIC_REQUIRED,SCHEMATIC_REQUIRED,UNASSIGNED,INCOMPLETE,NO
W026,OscillatorPLLReset,SCHEMATIC_REQUIRED,SCHEMATIC_REQUIRED,UNASSIGNED,INCOMPLETE,NO
W027,OscillatorPLLReset,SCHEMATIC_REQUIRED,SCHEMATIC_REQUIRED,UNASSIGNED,INCOMPLETE,NO
W028,OscillatorPLLReset,SCHEMATIC_REQUIRED,SCHEMATIC_REQUIRED,UNASSIGNED,INCOMPLETE,NO
W029,ProgrammingSWD,SCHEMATIC_REQUIRED,SCHEMATIC_REQUIRED,UNASSIGNED,INCOMPLETE,NO
W030,ProgrammingSWD,SCHEMATIC_REQUIRED,SCHEMATIC_REQUIRED,UNASSIGNED,INCOMPLETE,NO
W031,ProgrammingSWD,SCHEMATIC_REQUIRED,SCHEMATIC_REQUIRED,UNASSIGNED,INCOMPLETE,NO
W032,ProgrammingSWD,SCHEMATIC_REQUIRED,SCHEMATIC_REQUIRED,UNASSIGNED,INCOMPLETE,NO
W033,ProgrammingSWD,SCHEMATIC_REQUIRED,SCHEMATIC_REQUIRED,UNASSIGNED,INCOMPLETE,NO
W034,ProgrammingSWD,SCHEMATIC_REQUIRED,SCHEMATIC_REQUIRED,UNASSIGNED,INCOMPLETE,NO
W035,ProgrammingSWD,SCHEMATIC_REQUIRED,SCHEMATIC_REQUIRED,UNASSIGNED,INCOMPLETE,NO
W036,ProgrammingSWD,SCHEMATIC_REQUIRED,SCHEMATIC_REQUIRED,UNASSIGNED,INCOMPLETE,NO
W037,ProgrammingSWD,SCHEMATIC_REQUIRED,SCHEMATIC_REQUIRED,UNASSIGNED,INCOMPLETE,NO
W038,ProgrammingSWD,SCHEMATIC_REQUIRED,SCHEMATIC_REQUIRED,UNASSIGNED,INCOMPLETE,NO
W039,ProgrammingSWD,SCHEMATIC_REQUIRED,SCHEMATIC_REQUIRED,UNASSIGNED,INCOMPLETE,NO
W040,ProgrammingSWD,SCHEMATIC_REQUIRED,SCHEMATIC_REQUIRED,UNASSIGNED,INCOMPLETE,NO
W041,ProgrammingSWD,SCHEMATIC_REQUIRED,SCHEMATIC_REQUIRED,UNASSIGNED,INCOMPLETE,NO
W042,ProgrammingSWD,SCHEMATIC_REQUIRED,SCHEMATIC_REQUIRED,UNASSIGNED,INCOMPLETE,NO
W043,ProgrammingSWD,SCHEMATIC_REQUIRED,SCHEMATIC_REQUIRED,UNASSIGNED,INCOMPLETE,NO
W044,ProgrammingSWD,SCHEMATIC_REQUIRED,SCHEMATIC_REQUIRED,UNASSIGNED,INCOMPLETE,NO
W045,ExternalSPIFlash,SCHEMATIC_REQUIRED,SCHEMATIC_REQUIRED,UNASSIGNED,INCOMPLETE,NO
W046,ExternalSPIFlash,SCHEMATIC_REQUIRED,SCHEMATIC_REQUIRED,UNASSIGNED,INCOMPLETE,NO
W047,ExternalSPIFlash,SCHEMATIC_REQUIRED,SCHEMATIC_REQUIRED,UNASSIGNED,INCOMPLETE,NO
W048,ExternalSPIFlash,SCHEMATIC_REQUIRED,SCHEMATIC_REQUIRED,UNASSIGNED,INCOMPLETE,NO
W049,ExternalSPIFlash,SCHEMATIC_REQUIRED,SCHEMATIC_REQUIRED,UNASSIGNED,INCOMPLETE,NO
W050,ExternalSPIFlash,SCHEMATIC_REQUIRED,SCHEMATIC_REQUIRED,UNASSIGNED,INCOMPLETE,NO
W051,ExternalSPIFlash,SCHEMATIC_REQUIRED,SCHEMATIC_REQUIRED,UNASSIGNED,INCOMPLETE,NO
W052,ExternalSPIFlash,SCHEMATIC_REQUIRED,SCHEMATIC_REQUIRED,UNASSIGNED,INCOMPLETE,NO
W053,ExternalSPIFlash,SCHEMATIC_REQUIRED,SCHEMATIC_REQUIRED,UNASSIGNED,INCOMPLETE,NO
W054,ExternalSPIFlash,SCHEMATIC_REQUIRED,SCHEMATIC_REQUIRED,UNASSIGNED,INCOMPLETE,NO
W055,ExternalSPIFlash,SCHEMATIC_REQUIRED,SCHEMATIC_REQUIRED,UNASSIGNED,INCOMPLETE,NO
W056,ExternalSPIFlash,SCHEMATIC_REQUIRED,SCHEMATIC_REQUIRED,UNASSIGNED,INCOMPLETE,NO
W057,ExternalSPIFlash,SCHEMATIC_REQUIRED,SCHEMATIC_REQUIRED,UNASSIGNED,INCOMPLETE,NO
W058,ExternalSPIFlash,SCHEMATIC_REQUIRED,SCHEMATIC_REQUIRED,UNASSIGNED,INCOMPLETE,NO
W059,GPIO_I2C_UART,SCHEMATIC_REQUIRED,SCHEMATIC_REQUIRED,UNASSIGNED,INCOMPLETE,NO
W060,GPIO_I2C_UART,SCHEMATIC_REQUIRED,SCHEMATIC_REQUIRED,UNASSIGNED,INCOMPLETE,NO
W061,GPIO_I2C_UART,SCHEMATIC_REQUIRED,SCHEMATIC_REQUIRED,UNASSIGNED,INCOMPLETE,NO
W062,GPIO_I2C_UART,SCHEMATIC_REQUIRED,SCHEMATIC_REQUIRED,UNASSIGNED,INCOMPLETE,NO
W063,GPIO_I2C_UART,SCHEMATIC_REQUIRED,SCHEMATIC_REQUIRED,UNASSIGNED,INCOMPLETE,NO
W064,GPIO_I2C_UART,SCHEMATIC_REQUIRED,SCHEMATIC_REQUIRED,UNASSIGNED,INCOMPLETE,NO
W065,GPIO_I2C_UART,SCHEMATIC_REQUIRED,SCHEMATIC_REQUIRED,UNASSIGNED,INCOMPLETE,NO
W066,GPIO_I2C_UART,SCHEMATIC_REQUIRED,SCHEMATIC_REQUIRED,UNASSIGNED,INCOMPLETE,NO
W067,GPIO_I2C_UART,SCHEMATIC_REQUIRED,SCHEMATIC_REQUIRED,UNASSIGNED,INCOMPLETE,NO
W068,GPIO_I2C_UART,SCHEMATIC_REQUIRED,SCHEMATIC_REQUIRED,UNASSIGNED,INCOMPLETE,NO
W069,GPIO_I2C_UART,SCHEMATIC_REQUIRED,SCHEMATIC_REQUIRED,UNASSIGNED,INCOMPLETE,NO
W070,GPIO_I2C_UART,SCHEMATIC_REQUIRED,SCHEMATIC_REQUIRED,UNASSIGNED,INCOMPLETE,NO
W071,WatchdogMonitoring,SCHEMATIC_REQUIRED,SCHEMATIC_REQUIRED,UNASSIGNED,INCOMPLETE,NO
W072,WatchdogMonitoring,SCHEMATIC_REQUIRED,SCHEMATIC_REQUIRED,UNASSIGNED,INCOMPLETE,NO
W073,WatchdogMonitoring,SCHEMATIC_REQUIRED,SCHEMATIC_REQUIRED,UNASSIGNED,INCOMPLETE,NO
W074,WatchdogMonitoring,SCHEMATIC_REQUIRED,SCHEMATIC_REQUIRED,UNASSIGNED,INCOMPLETE,NO
W075,WatchdogMonitoring,SCHEMATIC_REQUIRED,SCHEMATIC_REQUIRED,UNASSIGNED,INCOMPLETE,NO
W076,WatchdogMonitoring,SCHEMATIC_REQUIRED,SCHEMATIC_REQUIRED,UNASSIGNED,INCOMPLETE,NO
W077,WatchdogMonitoring,SCHEMATIC_REQUIRED,SCHEMATIC_REQUIRED,UNASSIGNED,INCOMPLETE,NO
W078,WatchdogMonitoring,SCHEMATIC_REQUIRED,SCHEMATIC_REQUIRED,UNASSIGNED,INCOMPLETE,NO
W079,WatchdogMonitoring,SCHEMATIC_REQUIRED,SCHEMATIC_REQUIRED,UNASSIGNED,INCOMPLETE,NO
W080,WatchdogMonitoring,SCHEMATIC_REQUIRED,SCHEMATIC_REQUIRED,UNASSIGNED,INCOMPLETE,NO
W081,WatchdogMonitoring,SCHEMATIC_REQUIRED,SCHEMATIC_REQUIRED,UNASSIGNED,INCOMPLETE,NO
W082,WatchdogMonitoring,SCHEMATIC_REQUIRED,SCHEMATIC_REQUIRED,UNASSIGNED,INCOMPLETE,NO
```


## ANNEX D. Original 18 via records — UNVERIFIED

Source: `Data/Via_Plan_18_DESIGN_REQUIRED.csv`. Preserve the original source unchanged.

```csv
via_id,net,source_layer,target_layer,x_mm,y_mm,initial_state,pin_verified
V01,UNASSIGNED,TBD,TBD,,,MISSING,NO
V02,UNASSIGNED,TBD,TBD,,,MISSING,NO
V03,UNASSIGNED,TBD,TBD,,,MISSING,NO
V04,UNASSIGNED,TBD,TBD,,,MISSING,NO
V05,UNASSIGNED,TBD,TBD,,,MISSING,NO
V06,UNASSIGNED,TBD,TBD,,,MISSING,NO
V07,UNASSIGNED,TBD,TBD,,,MISSING,NO
V08,UNASSIGNED,TBD,TBD,,,MISSING,NO
V09,UNASSIGNED,TBD,TBD,,,MISSING,NO
V10,UNASSIGNED,TBD,TBD,,,MISSING,NO
V11,UNASSIGNED,TBD,TBD,,,MISSING,NO
V12,UNASSIGNED,TBD,TBD,,,MISSING,NO
V13,UNASSIGNED,TBD,TBD,,,MISSING,NO
V14,UNASSIGNED,TBD,TBD,,,MISSING,NO
V15,UNASSIGNED,TBD,TBD,,,MISSING,NO
V16,UNASSIGNED,TBD,TBD,,,MISSING,NO
V17,UNASSIGNED,TBD,TBD,,,MISSING,NO
V18,UNASSIGNED,TBD,TBD,,,MISSING,NO
```


## ANNEX E. Three authored fault profiles

Source: `Data/Fault_Profiles_3.csv`. Preserve the original source unchanged.

```csv
fault_id,location,fault,expected_effect,virtual_repair
F01,Y1/C1,Oscillator capacitor fault,8 MHz crystal unstable or missing,Replace faulty oscillator capacitor
F02,V11/SPI_DATA,SPI flash trace via open,"Internal firmware runs, external 8MB SPI flash fails",Restore open via and test bus
F03,R_RESET,Defective reset pull-up resistor,"Reset stuck low or noisy, boot blocked",Replace resistor and verify reset release
```


## ANNEX F. Six objective certification records

Source: `Data/Objective_Certification_6.csv`. Preserve the original source unchanged.

```csv
id,name,evidence_required
1,Assembly,62 parts placed correctly and soldered
2,CopperConnectivity,82 routes and 18 vias validated against reconciled pin-level netlist
3,FirstBoot,"3.3V,8MHz crystal,48MHz PLL,reset,firmware programming and qualified boot"
4,Diagnostics,"GPIO,timers,SPI,I2C,watchdog self-test successful"
5,FaultRecovery,"Crystal startup, SPI open via, reset resistor faults repaired; fresh new boot"
6,W083,"Removable heartbeat path disconnected,inhibit,replace,fresh pulses,explicit reset,new boot and test"
```


## ANNEX G. W083 critical heartbeat recovery

Source: `Data/W083_Critical_Heartbeat_Recovery.md`.

```markdown
# Critical W083 — application heartbeat

**U1.APPLICATION_HEARTBEAT_OUT → detachable W083 jumper → U8.TRUSTED_HEARTBEAT_IN**. This is a *conceptual functional* link; assign verified package contacts after completing the schematic.

Start with W083 uninstalled, install for first commissioning. During final supervised test disconnect it, observe trusted heartbeat invalidation and independent safety output inhibition. Reconnect, obtain multiple fresh valid transitions (not merely steady HIGH), explicitly reset latched protection and finish a new complete boot and diagnostics qualification. Do not alter the actual processor power rail or invent false clock errors.
```


## ANNEX H. Manufacturing and art accuracy notes

Source: `Data/Board_Manufacturing_Requirements.md`.

```markdown
# Manufacturing and rendering constraints
- Physical design outline: **190 x 130 mm**; nominal 1.6 mm thick FR4, four conductive layers. Coordinates in metres in FBX, millimetres in placement CSV.
- PCB geometry contains **decorative pad and factory trace references**, not a validated circuit; final routing requires actual pad and net IDs, clearance rules and layer connectivity.
- U1 is a procedural 64-lead LQFP, not a manufacturer-exact land pattern; verify pin pitch, body and package footprint before true electrical hookup.
- Four-layer references are visualization only. Do not treat the plane meshes as electrically continuous until the circuit solver confirms it.
- Avoid electrical false positives from the assembled reference FBX.
- Models are procedural realism-style, not certified CAD or manufactured hardware.
```


## ANNEX I. Virtual repair UI source integration

Source: `Data/Virtual_Repair_UI_Integration.md`.

```markdown
# Existing Project Spark AAA World-Space Repair UI integration
No instrument FBXs included. Use the existing 2D/World-Space Canvas multimeter, oscilloscope, logic analyzer, debugger, firmware programmer, via repair and component replacement tools.

Attach selectable game colliders/target bindings to the installed board pieces and pads. Never certify an electrical repair based solely on clicked target or a contact VFX. Read actual circuit solver connectivity and firmware state after action.

Suggested selectable targets: U1 pin rows, Y1 oscillator contacts, U2 flash connections, SW1 reset button, U8 watchdog, J2 SWD header and removable W083 jumper.
```


## ANNEX J. Exact source Unity empty-board art builder

Original source file: `Unity/Editor/SparkLevel99BoardOnlyArtBuilder.cs`. Source is for reference and requires project-side verification.

```csharp
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;

public static class SparkLevel99BoardOnlyArtBuilder
{
    private static GameObject FindModel(string stem)
    {
        var ids=AssetDatabase.FindAssets(stem+" t:Model");
        foreach(string id in ids)
        {
            string p=AssetDatabase.GUIDToAssetPath(id);
            if(p.EndsWith(stem+".fbx")) return AssetDatabase.LoadAssetAtPath<GameObject>(p);
        }
        return null;
    }
    [MenuItem("Tools/Project Spark/Levels/99/Board Only/1. Create EMPTY SBC-7 Board")]
    public static void BuildEmpty()
    {
        GameObject board=FindModel("63_L99_SBC7_Empty_FourLayer_PCB");
        if (!board) {Debug.LogError("[L99] Missing empty PCB FBX. Import Models_FBX first.");return;}
        GameObject root=new GameObject("L99_SBC7_BoardOnly_ArtRoot");
        if(Selection.activeTransform)root.transform.SetParent(Selection.activeTransform,false);
        GameObject visual=(GameObject)PrefabUtility.InstantiatePrefab(board);
        visual.name="Empty_PCB_ArtOnly";
        visual.transform.SetParent(root.transform,false);
        Selection.activeGameObject=root;
        Debug.Log("[L99] Empty board. 62 components, 82 traces, 18 vias and W083 are NOT installed. The visual FBX cannot certify gameplay.");
    }
    [MenuItem("Tools/Project Spark/Levels/99/Board Only/2. Validate 80 FBX Presence")]
    public static void Validate()
    {
        int count=0;
        foreach(string guid in AssetDatabase.FindAssets("t:Model"))
        {
            var path=AssetDatabase.GUIDToAssetPath(guid);
            if(path.EndsWith(".fbx") && Path.GetFileName(path).Contains("_L99_"))count++;
        }
        Debug.Log("[L99] Found "+count+" modular FBX art files. Expected 80. This is an ART ONLY check.");
    }
}
#endif
```


## ANNEX K. Exact source Unity PBR material installer

Original source file: `Unity/Editor/SparkLevel99PBRMaterialInstaller.cs`. Source is for reference and requires project-side verification.

```csharp
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;

public static class SparkLevel99PBRMaterialInstaller
{
 [MenuItem("Tools/Project Spark/Levels/99/Board Only/3. Create Native PBR Materials")]
 static void Install()
 {
   var ids=AssetDatabase.FindAssets("PCB_FR4_Albedo t:Texture2D");
   if(ids.Length==0){Debug.LogError("[L99] Shared PBR textures not found.");return;}
   string folder=Path.GetDirectoryName(AssetDatabase.GUIDToAssetPath(ids[0])).Replace('\\','/');
   string destination=folder+"/Generated_Unity_Materials";
   if(!AssetDatabase.IsValidFolder(destination))AssetDatabase.CreateFolder(folder,"Generated_Unity_Materials");
   Shader sh=Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("HDRP/Lit") ?? Shader.Find("Standard");
   if(!sh){Debug.LogError("[L99] No supported Lit shader found.");return;}
   string[] names={"PCB_FR4","PCB_Soldermask","Copper","ENIG_Gold","Solder_Tin","IC_Epoxy","IC_Marking","Ceramic_Ivory","Resistor_Black","Steel","Polymer_Sleeve","Polymer_Top","Copper_Winding","Ferrite","Connector_Black","Plastic_Red","LED_Green","PCB_Silkscreen","Dielectric","Insulator","Thermal_Ceramic"};
   foreach(string name in names)
   {
     string path=destination+"/L99_"+name+".mat";
     Material mat=AssetDatabase.LoadAssetAtPath<Material>(path);
     if(mat==null){mat=new Material(sh);AssetDatabase.CreateAsset(mat,path);}
     Texture2D alb=AssetDatabase.LoadAssetAtPath<Texture2D>(folder+"/"+name+"_Albedo.png");
     Texture2D mask=AssetDatabase.LoadAssetAtPath<Texture2D>(folder+"/"+name+"_UnityMetallicSmoothness.png");
     Texture2D ao=AssetDatabase.LoadAssetAtPath<Texture2D>(folder+"/"+name+"_AO.png");
     if(mat.HasProperty("_BaseMap"))mat.SetTexture("_BaseMap",alb);
     if(mat.HasProperty("_MainTex"))mat.SetTexture("_MainTex",alb);
     if(mat.HasProperty("_MetallicGlossMap"))mat.SetTexture("_MetallicGlossMap",mask);
     if(mat.HasProperty("_MaskMap"))mat.SetTexture("_MaskMap",mask);
     if(mat.HasProperty("_OcclusionMap"))mat.SetTexture("_OcclusionMap",ao);
     EditorUtility.SetDirty(mat);
   }
   AssetDatabase.SaveAssets();AssetDatabase.Refresh();
   Debug.Log("[L99] Created native PBR material assets. Assign/remap FBX material slots; verify URP/HDRP shader maps and imported normal settings.");
 }
}
#endif
```


## ANNEX L. Source kit file and verification policy

- Canonical complete archive: `ProjectSpark_Level99_SiliconBrain_BoardOnly_Realistic_Complete.zip`.
- Canonical subset: `ProjectSpark_Level99_SiliconBrain_BoardOnly_FBX_Only.zip`.
- Preserve numbered FBX filenames, component refdes, W001–W083 IDs and V01–V18 IDs. The named assembled reference FBX is never used as live gameplay geometry.
- Original reference textures and procedural FBXs are not proof of manufacturer CAD accuracy, verified electrical connectivity or compiled Unity gameplay.
- Real pin-level electrical definitions, chosen MCU pin mapping, actual programming firmware behavior, Unity hooks, testing and objective certification must be implemented and independently verified.

