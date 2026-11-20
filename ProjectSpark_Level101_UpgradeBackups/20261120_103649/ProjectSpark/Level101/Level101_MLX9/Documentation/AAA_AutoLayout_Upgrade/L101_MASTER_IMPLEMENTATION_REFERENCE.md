# PROJECT SPARK — LEVEL 101: THE HIDDEN LAYERS
## MASTER UNITY IMPLEMENTATION PROMPT — SIX-LAYER MLX-9

**Artifact type:** Implementation contract and verified source annexes; not runnable Unity code.  **Version:** 1.0 · 2026-11-18.



## 01. Primary instruction to implementation agent

Implement a complete playable Level 101, **The Hidden Layers**, in the existing Project Spark Unity codebase using the supplied `ProjectSpark_Level101_HiddenLayers_Realistic_BoardOnly_Complete.zip` as the authoritative art and planning input. Preserve Levels 1–100, all existing gameplay object lifecycles, electrical and objective APIs. Inspect the project source and assembly definitions **before** creating code or assuming class contracts. The expected endpoint is six authoritative objectives certified by `LevelGamePlayManager` after real assembly, copper/via construction, measured commissioning, differential-link qualification, three recoverable internal faults, and W101 disconnect/recover/requalification. Implement board-only art and reuse the existing large table and World-Space Virtual PCB Repair UI. No new room, furniture, physical instrument meshes or unrelated objects. Never claim functional success from an art import alone.


## 02. Verified package inventory

**Directly verified from the ZIP:** 96 modular FBX models and 96 matching GLBs; 76 installable component BOM entries; 20 non-installable board/layer/repair modules; 100 copper-route placeholder tasks; 24 via placeholders; 6 objectives; 3 fault scenarios. README declares 230 × 160 mm physical board, 6 copper layers, 12 PBR material families, 60 reference texture maps and one 2048-pixel silkscreen. An assembled showcase is supplied **only as visual reference**. FBX binary version 7400 uses centimetre coordinates; confirm Unity-scale conversion to 0.230 × 0.160 m. ZIP content is not proof of Unity import, electrical validity, manufacturer-accurate footprints, PBR quality or Play Mode. Source content transcribed in annexes.


## 03. Explicit source conflicts and data quality gates

**STOP-GATE A — no actual schematic:** All 100 route rows have `UNASSIGNED` endpoints/nets and `24` vias have `UNASSIGNED` nets and layer spans. These are **planning placeholders**, not permissible live contacts. Create a reviewed, typed, pin-verified schematic and mapping to `SparkTerminal`/PCB graph; only then enable affected gameplay. Preserve original IDs and group counts. Never fabricate contact endpoints while claiming source verification.

**STOP-GATE B — W101 naming mismatch:** `Critical_W101.json` names `L101_LinkIntegrityMonitor_U10` -> `L101_MultilayerSafetySupervisor_U11`, while the actual BOM lists **U7 = LINK_MON, U8 = SAFETY, U10 = LDO, U11 = INTERFACE**. This is a source inconsistency; do not silently map the safety path onto an LDO or interface. Establish canonical functional ownership after inspecting intended schematic and Level 101 original art, map it to consistent refdes and package pins, document the change in a signed reconciliation record, and update every asset binding, runtime port reference and acceptance test. Preserve `W101` as its public gameplay identity.

**STOP-GATE C — source art is not gameplay:** The supplied editor scripts only create an empty art root and reference `.mat` assets. They do not install components, wire circuit nets, implement six-layer DRC, run high-speed simulation, or register objectives. The visual reference is never a completion witness.

**STOP-GATE D — scene directory:** The source material creator searches paths containing `Level101` and `*_BaseColor.png`, but creates assets in `Assets/ProjectSpark_L101_Generated_Materials`; inspect and adapt if your Project Spark Assets root differs. Ensure the helper searches its package root instead of assuming a fixed directory.

**STOP-GATE E — safety and independent execution:** An illustrative differential eye graph, glowy route, static '100 Ω' label, or checkbox cannot certify impedance, continuity, data integrity or the W101 exercise.


## 04. Game and art scope

Build the level as a **single realistic MLX-9 PCB** on the existing Project Spark table. Use ONLY packaged 3D board and electronic component assets. Existing 21-tool AAA Virtual PCB Repair UI remains the inspection/repair interface: multimeter, oscilloscope, logic analyzer, TDR, layer viewer, net tracer, via repair, trace repair, solder/desolder, component replacement, calibration. All screen instruments must be World-Space Canvas and touch/mouse accessible. 3D board interaction must select actual `PCBComponent`, `PCBPad`, `PCBVia`, `PCBTrace` or probe target via hit testing. Differentiate editor showroom assembly from game start. Camera target presets: full-board, macro component, layer section, relevant via, differential pair, W101.


## 05. Initial world and player state

**At level start:** 76 installable components absent from their footprints, 100 copper routes uncompleted, 24 via tasks missing, W101 physically disconnected, safety-dependent link authorization inhibited, all six objectives incomplete. Board core/approved factory stack material may exist, but generated art may not secretly bridge missing copper. The completed showcase must not be active, collidable, or counted in the gameplay hierarchy. Store puzzle progress in save-state including layout, faults, objective proof, calibration timestamps, safety latch state and manufacturing history; reload must not turn stale calibration into fresh evidence.


## 06. Board coordinate and visual hierarchy

Treat physical PCB dimensions as X=0.230 m and Z=0.160 m, thickness/Y defined by the imported geometry. Validate origin and orientation against `Asset_Manifest_96.csv` measured placement positions, not guesses. Make root `Level101_MLX9` with separate `PCB_Core`, `TopMask`, `LayerVisualization_ArtOnly`, `InstalledComponents`, `FootprintPads`, `PlayerCopper`, `PlayerVias`, `FaultVisuals`, `RepairVFX`, `HologramOverlay_UI`, `ProbeTargets`. Any board-preview scaling belongs to a **visual-only parent transform**; electrical distances and physical positions remain metres. Keep layer reference meshes tagged non-conductive unless represented by validated factory netlist objects.


## 07. Asset import and material policy

Import exact FBX filenames in Annex A, preserve `L101_*` names and material-slot IDs, and map to a robust asset manifest rather than file discovery by substring alone. Validate 96 modular models and one assembled showcase independently. Each placeable component should be instantiated only after player placement, with ghost preview, orientation validation and undo. Check LQFP144 lead count, distinct metal leads, contact geometry, pads, solder fillet directions and L1 vs L6 face. Generate pipeline-appropriate URP/HDRP/Built-in materials, correct normal map import types, metallic/smoothness channel packing and color-space semantics. Source PBR textures are reference grade; final authored bakes, accurate footprint dimensions and mesh LODs need production verification. Fail loudly on unresolved source paths or broken material slots.


## 08. Shared PCB infrastructure and electrical authority

Inspect existing `SparkElectricalSolver`, `SparkCircuitSystem`, `SparkTerminal`, PCB footprint/trace/via abstractions, `LevelGamePlayManager`, inventory/shop, throw/recycle subsystem and virtual repair toolkit. Create adapters only where necessary; do not fork the electrical solver or duplicate gameplay events. Build a normalized electrical graph: package pin -> solder junction -> pad -> copper segment -> via/plane -> destination pad -> component pin, with typed nets, rail domains and return paths. Electrical contact semantics must be provided by the reconciled schematic, not component artwork. A source name ending `_Reference` or `_Plane` is decorative until a backed logical net is assigned.


## 09. Six-layer stack-up and manufacturing semantics

Implement independently addressable layers: L1 top signal/footprints, L2 ground reference, L3 internal signal, L4 segmented 3.3 V / 1.8 V power, L5 ground reference, L6 bottom signal/footprints. These are conceptual roles from design; freeze dielectric thickness, core/prepreg stack-up and copper thickness through a reviewable engineering config. Add through, blind and (if properly specified) buried via types with layer ranges, plating connectivity, drill/annular ring/clearance checks; a via is **not automatically connected to every intersected plane**. A copper path touching incompatible power and ground creates a fault rather than completion. Validate ground return reference continuity for each routed link and controlled-impedance pair.


## 10. Routing of 100 copper tasks and 24 via tasks

Respect public route IDs W001–W100 and via IDs V01–V24, and task-group allocation stored in `Copper_Routes_100_UNVERIFIED.csv`. Each task must receive reviewed `{sourceFootprint, sourcePin, sourcePad, destFootprint, destPin, destPad, netId, copperLayer, width, clearance, routeGeometry, allowedViaSpans, requireProof}` metadata. Route editing: select compatible source pad, choose layer, draw/edit route, snap to destination, check clearance/shorts, commit geometry, update solver, certify **only** if graph AND geometric DRC both pass. Via placement must bind a concrete net and legal layer span and return a unique verified physical junction. Disallow auto-completion from factory branches, showcase art, overlapping terminal visuals or already installed neighbors. Surface route tasks and internal plane tasks have distinct UX.


## 11. MCU, memory, clock, reset and power commissioning

Implement measurable protected 5 V input with 3.3 V logic and 1.8 V auxiliary rails, clock/reset sequencing, MCU state machine, SPI NOR flash and RAM behavior on the approved reconstructed schematic. A source mismatch remains between description and actual BOM for some roles: never assume U10 is the Link Monitor. Validate component values, regulator paths, decoupling and stable reference/return connections before boot. Simulate RAM access test, SPI read/write/CRC test, reset failures and accepted firmware image separately. A 3D LED turning green does not prove initialization. Clock and power observations must come from the simulation with timestamps.


## 12. Differential serial link and signal integrity

The training interface is fictional source-synchronous differential serial signaling with nominal **100 Ω differential** target, not full USB or PCIe compliance. Model actual pair traces, skew using propagation delays, reference discontinuities, via effects and approximate characteristic impedance from physically configured width, gap, copper and dielectric, with an explicit model confidence bound. Support known diagnostic patterns, eye-diagram samples, inter-symbol interference approximation, output amplitude, receiver threshold, bit error accounting and eye opening. Qualification depends on fresh instrument observations, repeatable data traffic and corrected pair geometry, not selected UI mode or zero-length data. Define fictional tolerances in a Level101_Specification asset and document why they are reasonable. Do not imply 100 Ω just because copper is drawn gold.


## 13. Ground return integrity and plane referencing

Return-path simulation must distinguish a healthy L2/L5 reference plane from a disrupted path around an anti-pad or invalid stitching via. Include net continuity, loop discontinuity/return length proxy, ground bounce and ringing approximation, shared return impedance and layout-specific return stitching. Fault F2 increases modeled return-path impedance and degrades measured link quality, without automatically shorting other nets. Repairs require a verified legal copper/through connection or approved board replacement; a surface-click effect cannot physically restore inaccessible internal copper without an authorized repair method.


## 14. Virtual instruments and touch workflow

Reuse the existing World-Space Canvas virtual repair kit. On tool selection, pointer/touch must resolve 3D hit position + target ID + PCB layer. Probe with the multimeter, current measurement, oscilloscope, logic analyzer and time-domain reflectometer using authoritative time-series data. Layer viewer may show filtered visual meshes but may not change real graph connectivity. TDR target distance is estimated with the configured propagation velocity and reflection response; calibrate and avoid pretending it perfectly locates arbitrary buried defects. Show tool eligibility, safe-isolation requirements, wrong-target feedback and cancellation; support mouse/touch, accessible hit size and camera focus. Dispatch VFX only after authority confirms an action is valid, and only record a repair after post-action electrical measurement.


## 15. Fault F1 — internal via open

Source `Faults_3.csv` says F1: `Selected signal-layer connection open`; repair `Qualified repair/jumper or replacement`. Inject fault by removing graph continuity of the resolved via net and layer span, not by hiding one mesh. Player observes an open with multi-layer net trace/TDR and follows a repair method permitted by the reviewed board design (or replaces the affected part). Measure new continuity and run fresh memory/link test. Reject any repair that adds an illegal bypass net or shorts a reference plane.


## 16. Fault F2 — ground-return discontinuity

Source states `Ground return impedance and ringing change`; repair `Restore legal return path`. Change actual modeled ground stitching/plane connection; detect increased return impedance and waveform/ringing changes. Player locates the failed path, fixes the approved return route and re-runs signal integrity tests. After repair, independent measurement must show reduced return impedance and recovered timing margins.


## 17. Fault F3 — differential pair skew

Source states `Pair mismatch causes timing errors`; repair `Modify verified trace segment geometry`. Change one P/N route's electrical delay while preserving DC continuity. Player uses differential pair tool and oscilloscope to identify mismatch, adjust authorized segment geometry, validate spacing/impedance and re-run fresh traffic patterns. Never certify by raw visual trace length alone; use complete path including vias and layer delay.


## 18. W101 safety disconnect challenge

Keep public ID `W101`. Source says `L101_LinkIntegrityMonitor_U10.TRUSTED_LINK_HEALTH_OUT` -> `L101_MultilayerSafetySupervisor_U11.TRUSTED_LINK_HEALTH_IN` but BOM names do not match; stop until canonical pins are reconciled (Section 03). W101 is a **removable logic health feedback**, not the differential data pair itself. Initial assembly installs W101 for normal commissioning. Objective 6 only opens after objectives 1–5 have been certified, then supervises physical jumper removal and confirms the actual electrical graph opens, health quality becomes untrusted, safety-dependent commands are inhibited and fault latches. Do not wipe unrelated clocks/memory or instantly force false zero measurement. Reconnect the correct physical contact, obtain fresh link measurements and fresh changing health frames, explicitly reset safety, then perform a brand-new fully qualified signal-integrity test. Historical objectives 1–5 remain certified but their evidence timestamps cannot substitute for Objective 6 post-repair requirements.


## 19. Objective authority and evidence

Register six authoritative objectives with the project's actual `LevelGamePlayManager`; do not make local bools, visible counter text or VFX independently award completion. O1 component presence/quality, O2 valid copper and via netlist, O3 real MCU boot/memory/power test, O4 signal integrity and return path, O5 three fault repairs + fresh tests, O6 W101 sequence + completely new qualification. Each objective requires evidence ID, device/net/layer identity, simulation timestamp, instrument calibration revision, fault epoch and measured limits. Reset/restart must not allow replay of a recorded success from a different fault epoch.


## 20. Unity builder and package layout

Create a one-click Unity Editor command: **`Tools/Project Spark/Levels/Create Level 101`**. On invocation: (1) inspect and validate existing APIs; (2) resolve source package model paths by manifest; (3) build empty art prefab; (4) create real component/footprint/pad records from reconciled schematic, preserving 76/100/24 tasks; (5) add routed-signal/plane visualization and touch targets; (6) add virtual tool integration; (7) add fault controllers and W101; (8) create six objectives; (9) emit a scene/config validation report. Suggested resources: `Level101_SchematicVerified.asset`, `Level101_ComponentMap.asset`, `Level101_PCBStack.asset`, `Level101_FaultProfile.asset`, `Level101_Objectives.asset`, `Level101_AcceptanceTests.asset`. Components proposed (only after API inspection): `SparkLevel101Coordinator`, `SparkPCBLayerStack`, `SparkPCBPlaneConnectivity`, `SparkPCBAdvancedViaSystem`, `SparkPCBDifferentialPairModel`, `SparkPCBReturnPathAnalyzer`, `SparkLevel101FaultController`, `SparkLevel101W101Recovery`, `SparkLevel101ObjectiveTracker`. These are design names, NOT confirmed existing classes.


## 21. Visual quality and AAA UX

Material look: navy FR-4, realistic matte soldermask, metallic solder/lead specular, gold finished pads, fine component silkscreen, restrained cyan data overlays, amber safety warnings. 3D visuals should be physically legible at macro zoom, not excessively emissive. Add pad hover highlight, precise alignment ghost, legitimate solder flow, short spark bursts, bounded heat shimmer, repair cool-down and diagnostic trace pulses. VFX use actual contact point and are canceled on denied action. Instrument windows are world-space UI, and repair tools appear as UI selections, not new physical FBX assets. Performance: avoid per-contact allocations, group via/trace visuals into batches where appropriate, disable art-only showcase colliders, profile scene on project target hardware.


## 22. Failure handling and regression guarantees

Recoverable invalid cases: wrong value part, reversed polarized part, floating return, duplicate pad assignment, unsupported layer transition, invalid W101 pin owner, lost project API binding, missing shader, stale measurement, malformed firmware, malformed netlist, attempts to certify from simulated UI values. Fail closed: useful explicit diagnostics, not fabricated success. Test interactions with shop purchase, shelf recycle/throw and equipment placement rules. Preserve prefab/file GUIDs where possible; do not alter saved Levels 1–100, shared materials, root camera or `LevelGamePlayManager` API to force progress.


## 23. Acceptance gates and deliverables

Deliver Unity source and serialized assets, verified source schema, reconciliation log, layer-aware schematic/netlist, importer/builder, sample test vectors, feature-complete virtual instrument adapters, fault scripts, six objectives, automated EditMode+PlayMode tests, a build log and before/after fault evidence. **No claim of 6/6** without real Unity Play Mode proof. Gate 1 source import; Gate 2 schematic pin-net review; Gate 3 full assembly and routing; Gate 4 instrument-sim calibration; Gate 5 every recoverable fault; Gate 6 W101 reset and new qualification; Gate 7 regression Levels 1–100.


## 24. Engineering acceptance test matrix — 96 proposed tests

**Proposed tests only — none have been run in Unity.** Each test requires recorded expected outcome, evidence, reproducible setup, and independent pass/fail result.

- **L101-AT-001 [Asset & editor import]** — All 96 model names resolve.
- **L101-AT-002 [Asset & editor import]** — FBX dimensions agree with 0.230 x 0.160 m.
- **L101-AT-003 [Asset & editor import]** — FBX material slots remap without silent fallback.
- **L101-AT-004 [Asset & editor import]** — Reference showcase remains disabled.
- **L101-AT-005 [Asset & editor import]** — 76 installable prefabs are separated.
- **L101-AT-006 [Asset & editor import]** — All noninstallable layer assets are art only.
- **L101-AT-007 [Asset & editor import]** — Empty root excludes components.
- **L101-AT-008 [Asset & editor import]** — Model import reports missing asset.
- **L101-AT-009 [Asset & editor import]** — Material installer locates nested package.
- **L101-AT-010 [Asset & editor import]** — No physical tool or room model created.
- **L101-AT-011 [Asset & editor import]** — LQFP lead count and orientation validated.
- **L101-AT-012 [Asset & editor import]** — Editor tool is idempotent.
- **L101-AT-013 [Schematic reconciliation]** — All 100 route nets resolved.
- **L101-AT-014 [Schematic reconciliation]** — All 24 via nets and spans resolved.
- **L101-AT-015 [Schematic reconciliation]** — All 76 footprint pins grounded.
- **L101-AT-016 [Schematic reconciliation]** — W101 refdes mismatch explicitly resolved.
- **L101-AT-017 [Schematic reconciliation]** — Power and signal domain typing succeeds.
- **L101-AT-018 [Schematic reconciliation]** — Every factory branch documented.
- **L101-AT-019 [Schematic reconciliation]** — No duplicate logical pad collision.
- **L101-AT-020 [Schematic reconciliation]** — No invalid ground/power short.
- **L101-AT-021 [Schematic reconciliation]** — Return path metadata complete.
- **L101-AT-022 [Schematic reconciliation]** — Netlist revision is hashed.
- **L101-AT-023 [Schematic reconciliation]** — Manifest variant selection validated.
- **L101-AT-024 [Schematic reconciliation]** — Missing schematic blocks certification.
- **L101-AT-025 [PCB placement & copper]** — Wrong footprint part refused.
- **L101-AT-026 [PCB placement & copper]** — Wrong orientation refused.
- **L101-AT-027 [PCB placement & copper]** — Correct part can be placed.
- **L101-AT-028 [PCB placement & copper]** — Uninstalled component not electrically live.
- **L101-AT-029 [PCB placement & copper]** — Wrong net route refused.
- **L101-AT-030 [PCB placement & copper]** — Incomplete route cannot certify.
- **L101-AT-031 [PCB placement & copper]** — Copper clearance violation rejected.
- **L101-AT-032 [PCB placement & copper]** — Trace editing undo works.
- **L101-AT-033 [PCB placement & copper]** — Via across wrong layer rejected.
- **L101-AT-034 [PCB placement & copper]** — Blind via span respected.
- **L101-AT-035 [PCB placement & copper]** — Completed route changes electrical graph.
- **L101-AT-036 [PCB placement & copper]** — All 100 and 24 necessary for O2.
- **L101-AT-037 [MCU & signal simulation]** — Reset held on rail undervoltage.
- **L101-AT-038 [MCU & signal simulation]** — 3.3V and 1.8V rails independently measured.
- **L101-AT-039 [MCU & signal simulation]** — Memory read/write integrity responds to via open.
- **L101-AT-040 [MCU & signal simulation]** — Clock failure prevents boot.
- **L101-AT-041 [MCU & signal simulation]** — Boot reports verified firmware state.
- **L101-AT-042 [MCU & signal simulation]** — Source-synchronous pair data transmits.
- **L101-AT-043 [MCU & signal simulation]** — Pair skew affects timing metric.
- **L101-AT-044 [MCU & signal simulation]** — Ground return impedance affects ringing.
- **L101-AT-045 [MCU & signal simulation]** — Eye samples vary with stimulus.
- **L101-AT-046 [MCU & signal simulation]** — Incorrect impedance changes quality.
- **L101-AT-047 [MCU & signal simulation]** — TDR distance uses configured velocity.
- **L101-AT-048 [MCU & signal simulation]** — Art-only overlay cannot pass O4.
- **L101-AT-049 [Fault handling]** — F1 opens correct via connection.
- **L101-AT-050 [Fault handling]** — F1 repairing wrong via rejected.
- **L101-AT-051 [Fault handling]** — F1 legal repair restores continuity.
- **L101-AT-052 [Fault handling]** — F1 fresh test required.
- **L101-AT-053 [Fault handling]** — F2 increases return impedance.
- **L101-AT-054 [Fault handling]** — F2 illegal power short rejected.
- **L101-AT-055 [Fault handling]** — F2 legal repair restores return.
- **L101-AT-056 [Fault handling]** — F2 fresh test required.
- **L101-AT-057 [Fault handling]** — F3 increases electrical skew.
- **L101-AT-058 [Fault handling]** — F3 continuity remains healthy.
- **L101-AT-059 [Fault handling]** — F3 geometry correction validates impedance.
- **L101-AT-060 [Fault handling]** — All three faults require new qualification.
- **L101-AT-061 [W101 critical safety]** — W101 initially disconnected.
- **L101-AT-062 [W101 critical safety]** — W101 installed for commissioning.
- **L101-AT-063 [W101 critical safety]** — W101 unrelated routes remain intact.
- **L101-AT-064 [W101 critical safety]** — Disconnect removes real feedback edge.
- **L101-AT-065 [W101 critical safety]** — Missing pulse is detected.
- **L101-AT-066 [W101 critical safety]** — Stuck-high heartbeat rejected.
- **L101-AT-067 [W101 critical safety]** — Safety inhibited with latch.
- **L101-AT-068 [W101 critical safety]** — Reconnect without reset remains latched.
- **L101-AT-069 [W101 critical safety]** — Fresh signal measurements required.
- **L101-AT-070 [W101 critical safety]** — Explicit reset only with healthy status.
- **L101-AT-071 [W101 critical safety]** — New epoch qualification required.
- **L101-AT-072 [W101 critical safety]** — W101 endpoint mismatch stops builder.
- **L101-AT-073 [UI repair + accessibility]** — World-space Canvas receives touch.
- **L101-AT-074 [UI repair + accessibility]** — World-space Canvas receives mouse.
- **L101-AT-075 [UI repair + accessibility]** — Probe click resolves layer and target.
- **L101-AT-076 [UI repair + accessibility]** — VFX emits at target contact.
- **L101-AT-077 [UI repair + accessibility]** — Invalid target does not play success effect.
- **L101-AT-078 [UI repair + accessibility]** — Desolder requires safe isolation.
- **L101-AT-079 [UI repair + accessibility]** — Layer viewer never modifies graph.
- **L101-AT-080 [UI repair + accessibility]** — Net tracer reflects live topology.
- **L101-AT-081 [UI repair + accessibility]** — Tool switching cancels old operation.
- **L101-AT-082 [UI repair + accessibility]** — Accessibility zoom keeps targets selectable.
- **L101-AT-083 [UI repair + accessibility]** — Instrument readings use authoritative samples.
- **L101-AT-084 [UI repair + accessibility]** — No VFX-driven objective success.
- **L101-AT-085 [Objectives, save & regression]** — O1 only after 76 verified placements.
- **L101-AT-086 [Objectives, save & regression]** — O2 only after 100 routes and 24 vias.
- **L101-AT-087 [Objectives, save & regression]** — O3 requires actual boot and memory tests.
- **L101-AT-088 [Objectives, save & regression]** — O4 requires qualified differential link.
- **L101-AT-089 [Objectives, save & regression]** — O5 requires fault history and fresh tests.
- **L101-AT-090 [Objectives, save & regression]** — O6 requires disconnect recovery epoch.
- **L101-AT-091 [Objectives, save & regression]** — Objective manager not duplicated.
- **L101-AT-092 [Objectives, save & regression]** — Saving preserves legitimate incomplete state.
- **L101-AT-093 [Objectives, save & regression]** — Reload does not certify stale results.
- **L101-AT-094 [Objectives, save & regression]** — Levels 1–100 unchanged.
- **L101-AT-095 [Objectives, save & regression]** — No unrelated electrical topology modified.
- **L101-AT-096 [Objectives, save & regression]** — Playable 6/6 demands real evidence.


## 25. Recommended gameplay walkthrough

1. Start with empty MLX-9 and scan component footprints. 2. Place and solder 76 correct parts. 3. Route 100 copper tasks and place 24 approved via spans. 4. Install W101 and verify power/clock/reset/memory boot. 5. Verify paired signal timing, return path, eye quality and link data. 6. Inject F1 internal via open, diagnose and repair. 7. Inject F2 ground reference fault, measure and repair. 8. Inject F3 pair skew, correct physical timing path. 9. Repeat full fresh O5 requalification. 10. Physically unplug W101; observe independent safety inhibition. 11. Reconnect proper terminal, obtain new health and calibration data, reset latch. 12. Run final fresh multilayer signal qualification. 13. Award 6/6 only through project authority.


## 26. Unresolved design questions requiring a reviewed artifact

The implementation agent must resolve these **with project engineers/source files** rather than invent them from a count table: exact U7/U8/U10/U11 functional owners for W101; CPU, RAM, SPI flash manufacturer's pinouts; true power-rail values by component; copper stack dielectric thickness; 100 Ω pair topology/rate; source-synchronous clock assignment; via manufacturing types and spans; true repair feasibility for internal defects; official Project Spark solver/PCB Editor APIs; behavior under partial connection. Preserve original source fields in a reconciliation report and clearly separate approved additions from original package claims.


## ANNEX 01 — Exact 96-model asset manifest

**Source path:** `Level101_MLX9/Data/Asset_Manifest_96.csv`. Copied verbatim from ZIP; unverified fields remain unverified.

```csv
index,name,kind,ref,fbx,glb,x_m,y_m,z_m,installable,material_slots,mesh_pieces
1,L101_U1,MCU,U1,001_L101_U1.fbx,001_L101_U1.glb,0,0.0008,0,true,IC_Epoxy|Tin_Leads,146
2,L101_U2,SPI_FLASH,U2,002_L101_U2.fbx,002_L101_U2.glb,0.075,0.0008,0.038,true,IC_Epoxy|Tin_Leads,17
3,L101_U3,RAM,U3,003_L101_U3.fbx,003_L101_U3.glb,0.075,0.0008,-0.015,true,IC_Epoxy|Tin_Leads,21
4,L101_U4,PMIC,U4,004_L101_U4.fbx,004_L101_U4.glb,-0.072,0.0008,0.038,true,IC_Epoxy|Tin_Leads,17
5,L101_U5,CLOCK,U5,005_L101_U5.fbx,005_L101_U5.glb,-0.072,0.0008,-0.02,true,IC_Epoxy|Tin_Leads,17
6,L101_U6,DIFF_TX,U6,006_L101_U6.fbx,006_L101_U6.glb,-0.072,0.0008,-0.06,true,IC_Epoxy|Tin_Leads,17
7,L101_U7,LINK_MON,U7,007_L101_U7.fbx,007_L101_U7.glb,0.075,0.0008,-0.06,true,IC_Epoxy|Tin_Leads,17
8,L101_U8,SAFETY,U8,008_L101_U8.fbx,008_L101_U8.glb,0.024,0.0008,-0.06,true,IC_Epoxy|Tin_Leads,17
9,L101_U9,TRANSCEIVER,U9,009_L101_U9.fbx,009_L101_U9.glb,0.027,0.0008,0.054,true,IC_Epoxy|Tin_Leads,17
10,L101_U10,LDO,U10,010_L101_U10.fbx,010_L101_U10.glb,-0.017,0.0008,0.054,true,IC_Epoxy|Tin_Leads,17
11,L101_U11,INTERFACE,U11,011_L101_U11.fbx,011_L101_U11.glb,0.003,0.0008,-0.063,true,IC_Epoxy|Tin_Leads,17
12,L101_U12,SENSOR,U12,012_L101_U12.fbx,012_L101_U12.glb,0.072,0.0008,0.066,true,IC_Epoxy|Tin_Leads,17
13,L101_U13,BOOT,U13,013_L101_U13.fbx,013_L101_U13.glb,-0.084,0.0008,0.066,true,IC_Epoxy|Tin_Leads,17
14,L101_R1,RES,R1,014_L101_R1.fbx,014_L101_R1.glb,-0.076,0.0008,0.068,true,IC_Epoxy|Tin_Leads,3
15,L101_R2,RES,R2,015_L101_R2.fbx,015_L101_R2.glb,-0.059,0.0008,0.068,true,IC_Epoxy|Tin_Leads,3
16,L101_R3,RES,R3,016_L101_R3.fbx,016_L101_R3.glb,-0.033,0.0008,0.068,true,IC_Epoxy|Tin_Leads,3
17,L101_R4,RES,R4,017_L101_R4.fbx,017_L101_R4.glb,-0.016,0.0008,0.068,true,IC_Epoxy|Tin_Leads,3
18,L101_R5,RES,R5,018_L101_R5.fbx,018_L101_R5.glb,0.001,0.0008,0.068,true,IC_Epoxy|Tin_Leads,3
19,L101_R6,RES,R6,019_L101_R6.fbx,019_L101_R6.glb,0.018,0.0008,0.068,true,IC_Epoxy|Tin_Leads,3
20,L101_R7,RES,R7,020_L101_R7.fbx,020_L101_R7.glb,0.035,0.0008,0.068,true,IC_Epoxy|Tin_Leads,3
21,L101_R8,RES,R8,021_L101_R8.fbx,021_L101_R8.glb,0.052,0.0008,0.068,true,IC_Epoxy|Tin_Leads,3
22,L101_R9,RES,R9,022_L101_R9.fbx,022_L101_R9.glb,0.06,0.0008,0.068,true,IC_Epoxy|Tin_Leads,3
23,L101_R10,RES,R10,023_L101_R10.fbx,023_L101_R10.glb,0.077,0.0008,0.068,true,IC_Epoxy|Tin_Leads,3
24,L101_R11,RES,R11,024_L101_R11.fbx,024_L101_R11.glb,-0.093,0.0008,0.056,true,IC_Epoxy|Tin_Leads,3
25,L101_R12,RES,R12,025_L101_R12.fbx,025_L101_R12.glb,-0.076,0.0008,0.056,true,IC_Epoxy|Tin_Leads,3
26,L101_R13,RES,R13,026_L101_R13.fbx,026_L101_R13.glb,-0.059,0.0008,0.056,true,IC_Epoxy|Tin_Leads,3
27,L101_R14,RES,R14,027_L101_R14.fbx,027_L101_R14.glb,-0.033,0.0008,0.056,true,IC_Epoxy|Tin_Leads,3
28,L101_R15,RES,R15,028_L101_R15.fbx,028_L101_R15.glb,-0.016,0.0008,0.056,true,IC_Epoxy|Tin_Leads,3
29,L101_R16,RES,R16,029_L101_R16.fbx,029_L101_R16.glb,0.001,0.0008,0.056,true,IC_Epoxy|Tin_Leads,3
30,L101_R17,RES,R17,030_L101_R17.fbx,030_L101_R17.glb,0.018,0.0008,0.056,true,IC_Epoxy|Tin_Leads,3
31,L101_R18,RES,R18,031_L101_R18.fbx,031_L101_R18.glb,0.035,0.0008,0.056,true,IC_Epoxy|Tin_Leads,3
32,L101_R19,RES,R19,032_L101_R19.fbx,032_L101_R19.glb,0.052,0.0008,0.056,true,IC_Epoxy|Tin_Leads,3
33,L101_R20,RES,R20,033_L101_R20.fbx,033_L101_R20.glb,0.06,0.0008,0.056,true,IC_Epoxy|Tin_Leads,3
34,L101_R21,RES,R21,034_L101_R21.fbx,034_L101_R21.glb,0.077,0.0008,0.056,true,IC_Epoxy|Tin_Leads,3
35,L101_R22,RES,R22,035_L101_R22.fbx,035_L101_R22.glb,-0.093,0.0008,0.044,true,IC_Epoxy|Tin_Leads,3
36,L101_C1,CAP,C1,036_L101_C1.fbx,036_L101_C1.glb,-0.079,0.0008,-0.073,true,Ceramic_Tan|Tin_Leads,3
37,L101_C2,CAP,C2,037_L101_C2.fbx,037_L101_C2.glb,-0.062,0.0008,-0.073,true,Ceramic_Tan|Tin_Leads,3
38,L101_C3,CAP,C3,038_L101_C3.fbx,038_L101_C3.glb,-0.045,0.0008,-0.073,true,Ceramic_Tan|Tin_Leads,3
39,L101_C4,CAP,C4,039_L101_C4.fbx,039_L101_C4.glb,-0.028,0.0008,-0.073,true,Ceramic_Tan|Tin_Leads,3
40,L101_C5,CAP,C5,040_L101_C5.fbx,040_L101_C5.glb,-0.011,0.0008,-0.073,true,Ceramic_Tan|Tin_Leads,3
41,L101_C6,CAP,C6,041_L101_C6.fbx,041_L101_C6.glb,0.006,0.0008,-0.073,true,Ceramic_Tan|Tin_Leads,3
42,L101_C7,CAP,C7,042_L101_C7.fbx,042_L101_C7.glb,0.023,0.0008,-0.073,true,Ceramic_Tan|Tin_Leads,3
43,L101_C8,CAP,C8,043_L101_C8.fbx,043_L101_C8.glb,0.04,0.0008,-0.073,true,Ceramic_Tan|Tin_Leads,3
44,L101_C9,CAP,C9,044_L101_C9.fbx,044_L101_C9.glb,0.057,0.0008,-0.073,true,Ceramic_Tan|Tin_Leads,3
45,L101_C10,CAP,C10,045_L101_C10.fbx,045_L101_C10.glb,0.074,0.0008,-0.073,true,Ceramic_Tan|Tin_Leads,3
46,L101_C11,CAP,C11,046_L101_C11.fbx,046_L101_C11.glb,-0.096,0.0008,-0.061,true,Ceramic_Tan|Tin_Leads,3
47,L101_C12,CAP,C12,047_L101_C12.fbx,047_L101_C12.glb,-0.079,0.0008,-0.061,true,Ceramic_Tan|Tin_Leads,3
48,L101_C13,CAP,C13,048_L101_C13.fbx,048_L101_C13.glb,-0.062,0.0008,-0.061,true,Ceramic_Tan|Tin_Leads,3
49,L101_C14,CAP,C14,049_L101_C14.fbx,049_L101_C14.glb,-0.045,0.0008,-0.061,true,Ceramic_Tan|Tin_Leads,3
50,L101_C15,CAP,C15,050_L101_C15.fbx,050_L101_C15.glb,-0.028,0.0008,-0.061,true,Ceramic_Tan|Tin_Leads,3
51,L101_C16,CAP,C16,051_L101_C16.fbx,051_L101_C16.glb,-0.011,0.0008,-0.061,true,Ceramic_Tan|Tin_Leads,3
52,L101_C17,CAP,C17,052_L101_C17.fbx,052_L101_C17.glb,0.006,0.0008,-0.061,true,Ceramic_Tan|Tin_Leads,3
53,L101_C18,CAP,C18,053_L101_C18.fbx,053_L101_C18.glb,0.023,0.0008,-0.061,true,Ceramic_Tan|Tin_Leads,3
54,L101_C19,CAP,C19,054_L101_C19.fbx,054_L101_C19.glb,0.04,0.0008,-0.061,true,Ceramic_Tan|Tin_Leads,3
55,L101_C20,CAP,C20,055_L101_C20.fbx,055_L101_C20.glb,0.057,0.0008,-0.061,true,Ceramic_Tan|Tin_Leads,3
56,L101_C21,CAP,C21,056_L101_C21.fbx,056_L101_C21.glb,0.074,0.0008,-0.061,true,Ceramic_Tan|Tin_Leads,3
57,L101_C22,CAP,C22,057_L101_C22.fbx,057_L101_C22.glb,-0.096,0.0008,-0.049,true,Ceramic_Tan|Tin_Leads,3
58,L101_Y1,CRYSTAL,Y1,058_L101_Y1.fbx,058_L101_Y1.glb,-0.044,0.0008,0.016,true,Copper_Gold|Tin_Leads,5
59,L101_J1,CONNECTOR,J1,059_L101_J1.fbx,059_L101_J1.glb,-0.101,0.0008,0.072,true,Connector_Black|Copper_Gold|Tin_Leads,10
60,L101_J2,CONNECTOR,J2,060_L101_J2.fbx,060_L101_J2.glb,-0.051,0.0008,-0.073,true,Connector_Black|Copper_Gold|Tin_Leads,10
61,L101_J3,CONNECTOR,J3,061_L101_J3.fbx,061_L101_J3.glb,-0.001,0.0008,0.072,true,Connector_Black|Copper_Gold|Tin_Leads,10
62,L101_J4,CONNECTOR,J4,062_L101_J4.fbx,062_L101_J4.glb,0.049,0.0008,-0.073,true,Connector_Black|Copper_Gold|Tin_Leads,10
63,L101_J5,CONNECTOR,J5,063_L101_J5.fbx,063_L101_J5.glb,0.099,0.0008,0.072,true,Connector_Black|Copper_Gold|Tin_Leads,10
64,L101_LED1,LED,LED1,064_L101_LED1.fbx,064_L101_LED1.glb,0.044,0.0008,-0.075,true,Ceramic_Tan|LED_Green,2
65,L101_LED2,LED,LED2,065_L101_LED2.fbx,065_L101_LED2.glb,0.061,0.0008,-0.075,true,Ceramic_Tan|LED_Green,2
66,L101_LED3,LED,LED3,066_L101_LED3.fbx,066_L101_LED3.glb,0.078,0.0008,-0.075,true,Ceramic_Tan|LED_Green,2
67,L101_LED4,LED,LED4,067_L101_LED4.fbx,067_L101_LED4.glb,0.095,0.0008,-0.075,true,Ceramic_Tan|LED_Green,2
68,L101_SW1,SWITCH,SW1,068_L101_SW1.fbx,068_L101_SW1.glb,-0.091,0.0008,-0.044,true,Connector_Black|Tin_Leads,6
69,L101_SW2,SWITCH,SW2,069_L101_SW2.fbx,069_L101_SW2.glb,-0.065,0.0008,-0.044,true,Connector_Black|Tin_Leads,6
70,L101_FB1,FERRITE,FB1,070_L101_FB1.fbx,070_L101_FB1.glb,-0.025,0.0008,0.075,true,Ferrite_Grey|Tin_Leads,3
71,L101_FB2,FERRITE,FB2,071_L101_FB2.fbx,071_L101_FB2.glb,-0.01,0.0008,0.075,true,Ferrite_Grey|Tin_Leads,3
72,L101_FB3,FERRITE,FB3,072_L101_FB3.fbx,072_L101_FB3.glb,0.005,0.0008,0.075,true,Ferrite_Grey|Tin_Leads,3
73,L101_D1,DIODE,D1,073_L101_D1.fbx,073_L101_D1.glb,0.031,0.0008,0.075,true,IC_Epoxy|Tin_Leads,3
74,L101_D2,DIODE,D2,074_L101_D2.fbx,074_L101_D2.glb,0.047,0.0008,0.075,true,IC_Epoxy|Tin_Leads,3
75,L101_D3,DIODE,D3,075_L101_D3.fbx,075_L101_D3.glb,0.063,0.0008,0.075,true,IC_Epoxy|Tin_Leads,3
76,L101_W101,JUMPER,W101,076_L101_W101.fbx,076_L101_W101.glb,0.097,0.0008,-0.048,true,Fault_Amber|Tin_Leads,3
77,L101_PCB_FR4_Core,BoardArt,,077_L101_PCB_FR4_Core.fbx,077_L101_PCB_FR4_Core.glb,0,0,0,false,Copper_Gold|FR4_Core,5
78,L101_L1_Top_Signal_Copper,BoardArt,,078_L101_L1_Top_Signal_Copper.fbx,078_L101_L1_Top_Signal_Copper.glb,0,0,0,false,Copper_Gold,18
79,L101_L2_Ground_Plane,BoardArt,,079_L101_L2_Ground_Plane.fbx,079_L101_L2_Ground_Plane.glb,0,0,0,false,Copper_Gold,1
80,L101_L3_Internal_Signal,BoardArt,,080_L101_L3_Internal_Signal.fbx,080_L101_L3_Internal_Signal.glb,0,0,0,false,Copper_Gold,18
81,L101_L4_Power_Plane,BoardArt,,081_L101_L4_Power_Plane.fbx,081_L101_L4_Power_Plane.glb,0,0,0,false,Copper_Gold,1
82,L101_L5_Ground_Plane,BoardArt,,082_L101_L5_Ground_Plane.fbx,082_L101_L5_Ground_Plane.glb,0,0,0,false,Copper_Gold,1
83,L101_L6_Bottom_Signal,BoardArt,,083_L101_L6_Bottom_Signal.fbx,083_L101_L6_Bottom_Signal.glb,0,0,0,false,Copper_Gold,18
84,L101_SMD_Footprint_Pads,BoardArt,,084_L101_SMD_Footprint_Pads.fbx,084_L101_SMD_Footprint_Pads.glb,0,0,0,false,Copper_Gold,34
85,L101_Plated_Through_Vias,BoardArt,,085_L101_Plated_Through_Vias.fbx,085_L101_Plated_Through_Vias.glb,0,0,0,false,Copper_Gold,24
86,L101_Ground_Stitching,BoardArt,,086_L101_Ground_Stitching.fbx,086_L101_Ground_Stitching.glb,0,0,0,false,Copper_Gold,20
87,L101_Test_Pads,BoardArt,,087_L101_Test_Pads.fbx,087_L101_Test_Pads.glb,0,0,0,false,Copper_Gold,8
88,L101_Solder_Contacts,BoardArt,,088_L101_Solder_Contacts.fbx,088_L101_Solder_Contacts.glb,0,0,0,false,Tin_Leads,10
89,L101_Internal_Via_Fault_Overlay,BoardArt,,089_L101_Internal_Via_Fault_Overlay.fbx,089_L101_Internal_Via_Fault_Overlay.glb,0,0,0,false,Fault_Amber,3
90,L101_Ground_Plane_Fault_Overlay,BoardArt,,090_L101_Ground_Plane_Fault_Overlay.fbx,090_L101_Ground_Plane_Fault_Overlay.glb,0,0,0,false,Fault_Amber,3
91,L101_Differential_Skew_Fault_Overlay,BoardArt,,091_L101_Differential_Skew_Fault_Overlay.fbx,091_L101_Differential_Skew_Fault_Overlay.glb,0,0,0,false,Fault_Amber,3
92,L101_Link_Health_Fault_Overlay,BoardArt,,092_L101_Link_Health_Fault_Overlay.fbx,092_L101_Link_Health_Fault_Overlay.glb,0,0,0,false,Fault_Amber,3
93,L101_Silkscreen_Reference,BoardArt,,093_L101_Silkscreen_Reference.fbx,093_L101_Silkscreen_Reference.glb,0,0,0,false,Signal_Cyan,5
94,L101_Layer_Stack_Reference,BoardArt,,094_L101_Layer_Stack_Reference.fbx,094_L101_Layer_Stack_Reference.glb,0,0,0,false,Signal_Cyan,5
95,L101_Differential_Pair_Reference,BoardArt,,095_L101_Differential_Pair_Reference.fbx,095_L101_Differential_Pair_Reference.glb,0,0,0,false,Signal_Cyan,5
96,L101_Repair_Reference,BoardArt,,096_L101_Repair_Reference.fbx,096_L101_Repair_Reference.glb,0,0,0,false,Signal_Cyan,5
```


## ANNEX 02 — Exact 76-part bill of materials

**Source path:** `Level101_MLX9/Data/Component_BOM_76.csv`. Copied verbatim from ZIP; unverified fields remain unverified.

```csv
ref,kind,variant,x_m,z_m,install_state
U1,MCU,LQFP144,0,0,MISSING
U2,SPI_FLASH,,0.075,0.038,MISSING
U3,RAM,,0.075,-0.015,MISSING
U4,PMIC,,-0.072,0.038,MISSING
U5,CLOCK,,-0.072,-0.02,MISSING
U6,DIFF_TX,,-0.072,-0.06,MISSING
U7,LINK_MON,,0.075,-0.06,MISSING
U8,SAFETY,,0.024,-0.06,MISSING
U9,TRANSCEIVER,,0.027,0.054,MISSING
U10,LDO,,-0.017,0.054,MISSING
U11,INTERFACE,,0.003,-0.063,MISSING
U12,SENSOR,,0.072,0.066,MISSING
U13,BOOT,,-0.084,0.066,MISSING
R1,RES,,-0.076,0.068,MISSING
R2,RES,,-0.059,0.068,MISSING
R3,RES,,-0.033,0.068,MISSING
R4,RES,,-0.016,0.068,MISSING
R5,RES,,0.001,0.068,MISSING
R6,RES,,0.018,0.068,MISSING
R7,RES,,0.035,0.068,MISSING
R8,RES,,0.052,0.068,MISSING
R9,RES,,0.06,0.068,MISSING
R10,RES,,0.077,0.068,MISSING
R11,RES,,-0.093,0.056,MISSING
R12,RES,,-0.076,0.056,MISSING
R13,RES,,-0.059,0.056,MISSING
R14,RES,,-0.033,0.056,MISSING
R15,RES,,-0.016,0.056,MISSING
R16,RES,,0.001,0.056,MISSING
R17,RES,,0.018,0.056,MISSING
R18,RES,,0.035,0.056,MISSING
R19,RES,,0.052,0.056,MISSING
R20,RES,,0.06,0.056,MISSING
R21,RES,,0.077,0.056,MISSING
R22,RES,,-0.093,0.044,MISSING
C1,CAP,,-0.079,-0.073,MISSING
C2,CAP,,-0.062,-0.073,MISSING
C3,CAP,,-0.045,-0.073,MISSING
C4,CAP,,-0.028,-0.073,MISSING
C5,CAP,,-0.011,-0.073,MISSING
C6,CAP,,0.006,-0.073,MISSING
C7,CAP,,0.023,-0.073,MISSING
C8,CAP,,0.04,-0.073,MISSING
C9,CAP,,0.057,-0.073,MISSING
C10,CAP,,0.074,-0.073,MISSING
C11,CAP,,-0.096,-0.061,MISSING
C12,CAP,,-0.079,-0.061,MISSING
C13,CAP,,-0.062,-0.061,MISSING
C14,CAP,,-0.045,-0.061,MISSING
C15,CAP,,-0.028,-0.061,MISSING
C16,CAP,,-0.011,-0.061,MISSING
C17,CAP,,0.006,-0.061,MISSING
C18,CAP,,0.023,-0.061,MISSING
C19,CAP,,0.04,-0.061,MISSING
C20,CAP,,0.057,-0.061,MISSING
C21,CAP,,0.074,-0.061,MISSING
C22,CAP,,-0.096,-0.049,MISSING
Y1,CRYSTAL,,-0.044,0.016,MISSING
J1,CONNECTOR,,-0.101,0.072,MISSING
J2,CONNECTOR,,-0.051,-0.073,MISSING
J3,CONNECTOR,,-0.001,0.072,MISSING
J4,CONNECTOR,,0.049,-0.073,MISSING
J5,CONNECTOR,,0.099,0.072,MISSING
LED1,LED,,0.044,-0.075,MISSING
LED2,LED,,0.061,-0.075,MISSING
LED3,LED,,0.078,-0.075,MISSING
LED4,LED,,0.095,-0.075,MISSING
SW1,SWITCH,,-0.091,-0.044,MISSING
SW2,SWITCH,,-0.065,-0.044,MISSING
FB1,FERRITE,,-0.025,0.075,MISSING
FB2,FERRITE,,-0.01,0.075,MISSING
FB3,FERRITE,,0.005,0.075,MISSING
D1,DIODE,,0.031,0.075,MISSING
D2,DIODE,,0.047,0.075,MISSING
D3,DIODE,,0.063,0.075,MISSING
W101,JUMPER,,0.097,-0.048,MISSING
```


## ANNEX 03 — 100 source copper-route placeholders

**Source path:** `Level101_MLX9/Data/Copper_Routes_100_UNVERIFIED.csv`. Copied verbatim from ZIP; unverified fields remain unverified.

```csv
route_id,group,source_pad,destination_pad,net_id,status,requires_schematic
W001,Power/Returns,UNASSIGNED,UNASSIGNED,UNASSIGNED,MISSING,YES
W002,Power/Returns,UNASSIGNED,UNASSIGNED,UNASSIGNED,MISSING,YES
W003,Power/Returns,UNASSIGNED,UNASSIGNED,UNASSIGNED,MISSING,YES
W004,Power/Returns,UNASSIGNED,UNASSIGNED,UNASSIGNED,MISSING,YES
W005,Power/Returns,UNASSIGNED,UNASSIGNED,UNASSIGNED,MISSING,YES
W006,Power/Returns,UNASSIGNED,UNASSIGNED,UNASSIGNED,MISSING,YES
W007,Power/Returns,UNASSIGNED,UNASSIGNED,UNASSIGNED,MISSING,YES
W008,Power/Returns,UNASSIGNED,UNASSIGNED,UNASSIGNED,MISSING,YES
W009,Power/Returns,UNASSIGNED,UNASSIGNED,UNASSIGNED,MISSING,YES
W010,Power/Returns,UNASSIGNED,UNASSIGNED,UNASSIGNED,MISSING,YES
W011,Power/Returns,UNASSIGNED,UNASSIGNED,UNASSIGNED,MISSING,YES
W012,Power/Returns,UNASSIGNED,UNASSIGNED,UNASSIGNED,MISSING,YES
W013,Power/Returns,UNASSIGNED,UNASSIGNED,UNASSIGNED,MISSING,YES
W014,Power/Returns,UNASSIGNED,UNASSIGNED,UNASSIGNED,MISSING,YES
W015,Power/Returns,UNASSIGNED,UNASSIGNED,UNASSIGNED,MISSING,YES
W016,Power/Returns,UNASSIGNED,UNASSIGNED,UNASSIGNED,MISSING,YES
W017,MCU/Clock/Reset,UNASSIGNED,UNASSIGNED,UNASSIGNED,MISSING,YES
W018,MCU/Clock/Reset,UNASSIGNED,UNASSIGNED,UNASSIGNED,MISSING,YES
W019,MCU/Clock/Reset,UNASSIGNED,UNASSIGNED,UNASSIGNED,MISSING,YES
W020,MCU/Clock/Reset,UNASSIGNED,UNASSIGNED,UNASSIGNED,MISSING,YES
W021,MCU/Clock/Reset,UNASSIGNED,UNASSIGNED,UNASSIGNED,MISSING,YES
W022,MCU/Clock/Reset,UNASSIGNED,UNASSIGNED,UNASSIGNED,MISSING,YES
W023,MCU/Clock/Reset,UNASSIGNED,UNASSIGNED,UNASSIGNED,MISSING,YES
W024,MCU/Clock/Reset,UNASSIGNED,UNASSIGNED,UNASSIGNED,MISSING,YES
W025,MCU/Clock/Reset,UNASSIGNED,UNASSIGNED,UNASSIGNED,MISSING,YES
W026,MCU/Clock/Reset,UNASSIGNED,UNASSIGNED,UNASSIGNED,MISSING,YES
W027,MCU/Clock/Reset,UNASSIGNED,UNASSIGNED,UNASSIGNED,MISSING,YES
W028,MCU/Clock/Reset,UNASSIGNED,UNASSIGNED,UNASSIGNED,MISSING,YES
W029,MCU/Clock/Reset,UNASSIGNED,UNASSIGNED,UNASSIGNED,MISSING,YES
W030,MCU/Clock/Reset,UNASSIGNED,UNASSIGNED,UNASSIGNED,MISSING,YES
W031,MCU/Clock/Reset,UNASSIGNED,UNASSIGNED,UNASSIGNED,MISSING,YES
W032,MCU/Clock/Reset,UNASSIGNED,UNASSIGNED,UNASSIGNED,MISSING,YES
W033,MCU/Clock/Reset,UNASSIGNED,UNASSIGNED,UNASSIGNED,MISSING,YES
W034,MCU/Clock/Reset,UNASSIGNED,UNASSIGNED,UNASSIGNED,MISSING,YES
W035,Memory/Digital,UNASSIGNED,UNASSIGNED,UNASSIGNED,MISSING,YES
W036,Memory/Digital,UNASSIGNED,UNASSIGNED,UNASSIGNED,MISSING,YES
W037,Memory/Digital,UNASSIGNED,UNASSIGNED,UNASSIGNED,MISSING,YES
W038,Memory/Digital,UNASSIGNED,UNASSIGNED,UNASSIGNED,MISSING,YES
W039,Memory/Digital,UNASSIGNED,UNASSIGNED,UNASSIGNED,MISSING,YES
W040,Memory/Digital,UNASSIGNED,UNASSIGNED,UNASSIGNED,MISSING,YES
W041,Memory/Digital,UNASSIGNED,UNASSIGNED,UNASSIGNED,MISSING,YES
W042,Memory/Digital,UNASSIGNED,UNASSIGNED,UNASSIGNED,MISSING,YES
W043,Memory/Digital,UNASSIGNED,UNASSIGNED,UNASSIGNED,MISSING,YES
W044,Memory/Digital,UNASSIGNED,UNASSIGNED,UNASSIGNED,MISSING,YES
W045,Memory/Digital,UNASSIGNED,UNASSIGNED,UNASSIGNED,MISSING,YES
W046,Memory/Digital,UNASSIGNED,UNASSIGNED,UNASSIGNED,MISSING,YES
W047,Memory/Digital,UNASSIGNED,UNASSIGNED,UNASSIGNED,MISSING,YES
W048,Memory/Digital,UNASSIGNED,UNASSIGNED,UNASSIGNED,MISSING,YES
W049,Memory/Digital,UNASSIGNED,UNASSIGNED,UNASSIGNED,MISSING,YES
W050,Memory/Digital,UNASSIGNED,UNASSIGNED,UNASSIGNED,MISSING,YES
W051,Memory/Digital,UNASSIGNED,UNASSIGNED,UNASSIGNED,MISSING,YES
W052,Memory/Digital,UNASSIGNED,UNASSIGNED,UNASSIGNED,MISSING,YES
W053,Differential Link,UNASSIGNED,UNASSIGNED,UNASSIGNED,MISSING,YES
W054,Differential Link,UNASSIGNED,UNASSIGNED,UNASSIGNED,MISSING,YES
W055,Differential Link,UNASSIGNED,UNASSIGNED,UNASSIGNED,MISSING,YES
W056,Differential Link,UNASSIGNED,UNASSIGNED,UNASSIGNED,MISSING,YES
W057,Differential Link,UNASSIGNED,UNASSIGNED,UNASSIGNED,MISSING,YES
W058,Differential Link,UNASSIGNED,UNASSIGNED,UNASSIGNED,MISSING,YES
W059,Differential Link,UNASSIGNED,UNASSIGNED,UNASSIGNED,MISSING,YES
W060,Differential Link,UNASSIGNED,UNASSIGNED,UNASSIGNED,MISSING,YES
W061,Differential Link,UNASSIGNED,UNASSIGNED,UNASSIGNED,MISSING,YES
W062,Differential Link,UNASSIGNED,UNASSIGNED,UNASSIGNED,MISSING,YES
W063,Differential Link,UNASSIGNED,UNASSIGNED,UNASSIGNED,MISSING,YES
W064,Differential Link,UNASSIGNED,UNASSIGNED,UNASSIGNED,MISSING,YES
W065,Differential Link,UNASSIGNED,UNASSIGNED,UNASSIGNED,MISSING,YES
W066,Differential Link,UNASSIGNED,UNASSIGNED,UNASSIGNED,MISSING,YES
W067,Differential Link,UNASSIGNED,UNASSIGNED,UNASSIGNED,MISSING,YES
W068,Differential Link,UNASSIGNED,UNASSIGNED,UNASSIGNED,MISSING,YES
W069,Differential Link,UNASSIGNED,UNASSIGNED,UNASSIGNED,MISSING,YES
W070,Differential Link,UNASSIGNED,UNASSIGNED,UNASSIGNED,MISSING,YES
W071,Differential Link,UNASSIGNED,UNASSIGNED,UNASSIGNED,MISSING,YES
W072,Differential Link,UNASSIGNED,UNASSIGNED,UNASSIGNED,MISSING,YES
W073,Ground/Return,UNASSIGNED,UNASSIGNED,UNASSIGNED,MISSING,YES
W074,Ground/Return,UNASSIGNED,UNASSIGNED,UNASSIGNED,MISSING,YES
W075,Ground/Return,UNASSIGNED,UNASSIGNED,UNASSIGNED,MISSING,YES
W076,Ground/Return,UNASSIGNED,UNASSIGNED,UNASSIGNED,MISSING,YES
W077,Ground/Return,UNASSIGNED,UNASSIGNED,UNASSIGNED,MISSING,YES
W078,Ground/Return,UNASSIGNED,UNASSIGNED,UNASSIGNED,MISSING,YES
W079,Ground/Return,UNASSIGNED,UNASSIGNED,UNASSIGNED,MISSING,YES
W080,Ground/Return,UNASSIGNED,UNASSIGNED,UNASSIGNED,MISSING,YES
W081,Ground/Return,UNASSIGNED,UNASSIGNED,UNASSIGNED,MISSING,YES
W082,Ground/Return,UNASSIGNED,UNASSIGNED,UNASSIGNED,MISSING,YES
W083,Ground/Return,UNASSIGNED,UNASSIGNED,UNASSIGNED,MISSING,YES
W084,Ground/Return,UNASSIGNED,UNASSIGNED,UNASSIGNED,MISSING,YES
W085,Ground/Return,UNASSIGNED,UNASSIGNED,UNASSIGNED,MISSING,YES
W086,Ground/Return,UNASSIGNED,UNASSIGNED,UNASSIGNED,MISSING,YES
W087,Ground/Return,UNASSIGNED,UNASSIGNED,UNASSIGNED,MISSING,YES
W088,Ground/Return,UNASSIGNED,UNASSIGNED,UNASSIGNED,MISSING,YES
W089,Diagnostics/Safety,UNASSIGNED,UNASSIGNED,UNASSIGNED,MISSING,YES
W090,Diagnostics/Safety,UNASSIGNED,UNASSIGNED,UNASSIGNED,MISSING,YES
W091,Diagnostics/Safety,UNASSIGNED,UNASSIGNED,UNASSIGNED,MISSING,YES
W092,Diagnostics/Safety,UNASSIGNED,UNASSIGNED,UNASSIGNED,MISSING,YES
W093,Diagnostics/Safety,UNASSIGNED,UNASSIGNED,UNASSIGNED,MISSING,YES
W094,Diagnostics/Safety,UNASSIGNED,UNASSIGNED,UNASSIGNED,MISSING,YES
W095,Diagnostics/Safety,UNASSIGNED,UNASSIGNED,UNASSIGNED,MISSING,YES
W096,Diagnostics/Safety,UNASSIGNED,UNASSIGNED,UNASSIGNED,MISSING,YES
W097,Diagnostics/Safety,UNASSIGNED,UNASSIGNED,UNASSIGNED,MISSING,YES
W098,Diagnostics/Safety,UNASSIGNED,UNASSIGNED,UNASSIGNED,MISSING,YES
W099,Diagnostics/Safety,UNASSIGNED,UNASSIGNED,UNASSIGNED,MISSING,YES
W100,Diagnostics/Safety,UNASSIGNED,UNASSIGNED,UNASSIGNED,MISSING,YES
```


## ANNEX 04 — 24 source via placeholders

**Source path:** `Level101_MLX9/Data/Vias_24_UNVERIFIED.csv`. Copied verbatim from ZIP; unverified fields remain unverified.

```csv
via_id,net_id,x_m,z_m,from_layer,to_layer,status,requires_schematic
V01,UNASSIGNED,-0.092,-0.05,UNASSIGNED,UNASSIGNED,MISSING,YES
V02,UNASSIGNED,-0.076,-0.05,UNASSIGNED,UNASSIGNED,MISSING,YES
V03,UNASSIGNED,-0.06,-0.05,UNASSIGNED,UNASSIGNED,MISSING,YES
V04,UNASSIGNED,-0.044,-0.05,UNASSIGNED,UNASSIGNED,MISSING,YES
V05,UNASSIGNED,-0.028,-0.05,UNASSIGNED,UNASSIGNED,MISSING,YES
V06,UNASSIGNED,-0.012,-0.05,UNASSIGNED,UNASSIGNED,MISSING,YES
V07,UNASSIGNED,0.004,-0.05,UNASSIGNED,UNASSIGNED,MISSING,YES
V08,UNASSIGNED,0.02,-0.05,UNASSIGNED,UNASSIGNED,MISSING,YES
V09,UNASSIGNED,0.036,-0.05,UNASSIGNED,UNASSIGNED,MISSING,YES
V10,UNASSIGNED,0.052,-0.05,UNASSIGNED,UNASSIGNED,MISSING,YES
V11,UNASSIGNED,0.068,-0.05,UNASSIGNED,UNASSIGNED,MISSING,YES
V12,UNASSIGNED,0.084,-0.05,UNASSIGNED,UNASSIGNED,MISSING,YES
V13,UNASSIGNED,-0.092,0.055,UNASSIGNED,UNASSIGNED,MISSING,YES
V14,UNASSIGNED,-0.076,0.055,UNASSIGNED,UNASSIGNED,MISSING,YES
V15,UNASSIGNED,-0.06,0.055,UNASSIGNED,UNASSIGNED,MISSING,YES
V16,UNASSIGNED,-0.044,0.055,UNASSIGNED,UNASSIGNED,MISSING,YES
V17,UNASSIGNED,-0.028,0.055,UNASSIGNED,UNASSIGNED,MISSING,YES
V18,UNASSIGNED,-0.012,0.055,UNASSIGNED,UNASSIGNED,MISSING,YES
V19,UNASSIGNED,0.004,0.055,UNASSIGNED,UNASSIGNED,MISSING,YES
V20,UNASSIGNED,0.02,0.055,UNASSIGNED,UNASSIGNED,MISSING,YES
V21,UNASSIGNED,0.036,0.055,UNASSIGNED,UNASSIGNED,MISSING,YES
V22,UNASSIGNED,0.052,0.055,UNASSIGNED,UNASSIGNED,MISSING,YES
V23,UNASSIGNED,0.068,0.055,UNASSIGNED,UNASSIGNED,MISSING,YES
V24,UNASSIGNED,0.084,0.055,UNASSIGNED,UNASSIGNED,MISSING,YES
```


## ANNEX 05 — Six objective source records

**Source path:** `Level101_MLX9/Data/Objectives_6.csv`. Copied verbatim from ZIP; unverified fields remain unverified.

```csv
objective,name,evidence
1,Install components,76 verified installed
2,Route multilayer board,100 valid routed nets and 24 vias
3,Commission board,"Power, clock, boot and memory"
4,Qualify high-speed link,"Differential timing, return path and integrity"
5,Repair three faults,"Open via, broken return, pair skew; fresh tests"
6,Restore W101 feedback,"Disconnect, inhibit, reconnect, calibrate, reset and retest"
```


## ANNEX 06 — Three fault source records

**Source path:** `Level101_MLX9/Data/Faults_3.csv`. Copied verbatim from ZIP; unverified fields remain unverified.

```csv
fault,effect,repair
F1 Internal Via Open,Selected signal-layer connection open,Qualified repair/jumper or replacement
F2 Ground Return Discontinuity,Ground return impedance and ringing change,Restore legal return path
F3 Differential Pair Skew,Pair mismatch causes timing errors,Modify verified trace segment geometry
```


## ANNEX 07 — W101 source contract

**Source path:** `Level101_MLX9/Data/Critical_W101.json`. Copied verbatim from ZIP; unverified fields remain unverified.

```json
{
  "id": "W101",
  "from": "L101_LinkIntegrityMonitor_U10.TRUSTED_LINK_HEALTH_OUT",
  "to": "L101_MultilayerSafetySupervisor_U11.TRUSTED_LINK_HEALTH_IN",
  "type": "removable_logic_feedback_jumper",
  "start_connected": false,
  "functional_pins_verified": false,
  "disconnect_response": "Invalidate fresh link health, inhibit safety-dependent commands, latch fault",
  "recovery": [
    "physical reconnection",
    "fresh link measurements",
    "explicit safety reset",
    "full requalification"
  ]
}
```


## ANNEX 08 — Reference material parameters

**Source path:** `Level101_MLX9/Data/Materials.json`. Copied verbatim from ZIP; unverified fields remain unverified.

```json
{
  "PCB_Navy": {
    "color": [
      0.025,
      0.09,
      0.2
    ],
    "metallic": 0.1,
    "roughness": 0.36
  },
  "Copper_Gold": {
    "color": [
      0.7,
      0.43,
      0.13
    ],
    "metallic": 0.91,
    "roughness": 0.23
  },
  "IC_Epoxy": {
    "color": [
      0.065,
      0.071,
      0.083
    ],
    "metallic": 0.05,
    "roughness": 0.47
  },
  "Tin_Leads": {
    "color": [
      0.59,
      0.66,
      0.7
    ],
    "metallic": 0.89,
    "roughness": 0.24
  },
  "Ceramic_Tan": {
    "color": [
      0.77,
      0.67,
      0.49
    ],
    "metallic": 0.06,
    "roughness": 0.6
  },
  "Cap_Dark": {
    "color": [
      0.14,
      0.18,
      0.22
    ],
    "metallic": 0.28,
    "roughness": 0.34
  },
  "Connector_Black": {
    "color": [
      0.045,
      0.057,
      0.07
    ],
    "metallic": 0.09,
    "roughness": 0.4
  },
  "Signal_Cyan": {
    "color": [
      0.06,
      0.72,
      0.83
    ],
    "metallic": 0.3,
    "roughness": 0.28
  },
  "Fault_Amber": {
    "color": [
      1,
      0.35,
      0.02
    ],
    "metallic": 0.34,
    "roughness": 0.34
  },
  "LED_Green": {
    "color": [
      0.07,
      0.9,
      0.23
    ],
    "metallic": 0.03,
    "roughness": 0.2
  },
  "Ferrite_Grey": {
    "color": [
      0.17,
      0.19,
      0.2
    ],
    "metallic": 0.12,
    "roughness": 0.64
  },
  "FR4_Core": {
    "color": [
      0.21,
      0.28,
      0.19
    ],
    "metallic": 0.0,
    "roughness": 0.74
  }
}
```


## ANNEX 09 — Original README

**Source path:** `Level101_MLX9/README_FIRST.md`. Copied verbatim from ZIP; unverified fields remain unverified.

```markdown
# Project Spark Level 101 — MLX-9 The Hidden Layers

BOARD-ONLY PROCEDURAL 3D ASSET KIT. No room, table, physical instruments or UI model assets.

- 96 individually exported FBX + matching GLB models: 76 installable components, 20 PCB/layer/repair modules.
- 230 x 160 mm fictional PCB with six copper layers. All mesh positions expressed in metres, Y = thickness axis.
- One assembled reference FBX + GLB in Showcase, NOT for playable puzzle state.
- Shared 12 material families, 60 procedural PBR maps and a 2048-pixel silkscreen reference. These are reference-grade textures, not artist-authored AAA bakes.
- Gameplay starts with all 76 components missing, 100 routes missing, 24 vias missing, and W101 disconnected.
- CSV copper/via data are UNVERIFIED DESIGN PLACEHOLDERS, NOT A FUNCTIONAL PIN-LEVEL SCHEMATIC. Do not mark any route complete based on decorative traces.
- FBX uses binary 7400 and centimetre coordinates (physical units originally metres). Unity FBX import scaling should be checked against known 0.230 x 0.160 m board dimensions.
- Unity helper is art-only. It does not add electrical gameplay, solder functionality, or objective certification.

**Suggested install:** Place all folders under Assets/ProjectSpark/Level101 and run Tools > Project Spark > Levels > 101 > Build Empty MLX-9 Board.

Realism: modeled IC metal leads, component packages, PCB metal details, six-layer visualization. Manufacturer-accurate mechanical footprints and fully qualified Unity materials still require a production pass.
```


## ANNEX 10 — Original Unity empty-board builder

**Source path:** `Level101_MLX9/Unity/Editor/SparkL101ArtBuilder.cs`. Copied verbatim from ZIP; unverified fields remain unverified.

```csharp
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Linq;
public static class SparkL101ArtBuilder {
    [MenuItem("Tools/Project Spark/Levels/101/Build Empty MLX-9 Board")]
    static void Build() {
        string[] assets = AssetDatabase.FindAssets("L101_PCB_FR4_Core t:Model");
        if (assets.Length == 0) { Debug.LogError("[L101] PCB FBX not found. Import Models_FBX folder first."); return; }
        string path = AssetDatabase.GUIDToAssetPath(assets[0]);
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (!prefab) { Debug.LogError("[L101] Core FBX is not importable as GameObject."); return; }
        var root = new GameObject("L101_MLX9_Empty_Board_Art_ONLY");
        var child = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        child.transform.SetParent(root.transform, false);
        Selection.activeGameObject = root;
        Undo.RegisterCreatedObjectUndo(root, "Create L101 Empty Board");
        Debug.Log("[L101] Empty board art created. No components/routes/vias/W101 automatically installed. Check physical size and materials.");
    }
    [MenuItem("Tools/Project Spark/Levels/101/Validate FBX Imports")]
    static void Validate() {
        var all=AssetDatabase.FindAssets("t:Model").Select(AssetDatabase.GUIDToAssetPath).Where(x=>x.Contains("L101_")&&x.EndsWith(".fbx")).ToArray();
        Debug.Log("[L101] Found "+all.Length+" Level 101 FBX assets. Target = 96 modular + one reference; check scale 0.230 m x 0.160 m manually.");
    }
}
#endif
```


## ANNEX 11 — Original Unity material helper

**Source path:** `Level101_MLX9/Unity/Editor/SparkL101PBRMaterials.cs`. Copied verbatim from ZIP; unverified fields remain unverified.

```csharp
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
public static class SparkL101PBRMaterials {
 [MenuItem("Tools/Project Spark/Levels/101/Create Reference PBR Materials")]
 static void Build() {
  var paths=AssetDatabase.FindAssets("t:Texture2D").Select(AssetDatabase.GUIDToAssetPath).Where(s=>s.Contains("Level101")&&s.EndsWith("_BaseColor.png")).ToArray();
  const string folder="Assets/ProjectSpark_L101_Generated_Materials";
  if(!AssetDatabase.IsValidFolder(folder))AssetDatabase.CreateFolder("Assets","ProjectSpark_L101_Generated_Materials");
  foreach(var path in paths){
   var name=Path.GetFileNameWithoutExtension(path).Replace("_BaseColor","");
   var shader=Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
   if(!shader){Debug.LogError("Missing Lit shader");return;}
   var m=new Material(shader){name=name};
   var baseColor=AssetDatabase.LoadAssetAtPath<Texture2D>(path);
   if(m.HasProperty("_BaseMap"))m.SetTexture("_BaseMap",baseColor);
   if(m.HasProperty("_MainTex"))m.SetTexture("_MainTex",baseColor);
   var op=folder+"/"+name+".mat";
   if(!AssetDatabase.LoadAssetAtPath<Material>(op))AssetDatabase.CreateAsset(m,op);else Object.DestroyImmediate(m);
  }
  AssetDatabase.SaveAssets();
  Debug.Log("[L101] Generated "+paths.Length+" source reference materials. Assign/remap FBX slots; normal/metal/smoothness require pipeline-specific setup.");
 }
}
#endif
```


## 27. Final implementation definition of done

**Done =** Unity imports correctly, the six-layer board supports accurate placement/routing/repair interactions; all source placeholders have approved electrical pin and layer assignments; six objectives are certified by actual runtime evidence; an active link health test survives three faults and W101 supervised disconnection recovery; all acceptance tests are executed and reported; Levels 1–100 remain unaffected. Art-only success or editor compile alone is not playable completion.

**Mission tagline:** INSPECT. ROUTE. REFERENCE. REPAIR.
