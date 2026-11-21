# PROJECT SPARK — LEVEL 118: THE DETERMINISTIC GRID
## MASTER UNITY IMPLEMENTATION PROMPT — SOURCE-ALIGNED, AAA PRODUCTION TARGET

**Board:** TSX-26 · **Category:** Industrial real-time networking / time-sensitive Ethernet (TSN) · **Project:** Unity 6.3 URP · **Level:** 118

**Source package:** `ProjectSpark_Level118_TSX26_FULL_AAA_Original_VisualKit.zip`.

> **Implementation contract:** Act as a senior Unity gameplay/electrical-simulation engineer, embedded networking engineer and technical artist. Inspect the actual Project Spark source APIs **before** writing adapters. Integrate the Level 118 art and proposed engineering model with the authoritative circuit, terminal, selection, shop/shelf, gameplay manager, virtual inspection/repair, save and level systems. Preserve every prior Level 1–117 and the existing input/UI behavior. Do not invent methods or assert testing you have not run. This prompt specifies a **production target**, not a claim that the delivered kit already passes Play Mode or the simulation acceptance suite.

## 1. Source inventory and strict evidence gates

The package contains **288 separately authored modular OBJ and GLB pairs**, comprising **236 installable electronic parts**, **52 structural/inspection art models**, and one separate `289_L118_AssembledReference.obj/.glb` as non-gameplay reference. Physical TSX-26 PCB **480 x 340 mm**, Y-up, metres; 28 inspection-layer art models, **34 procedural PBR material families** each containing BaseColor, Normal, AO and MetalRoughAO, plus 34 URP metallic-smoothness RGBA maps. Original procedural geometry includes realistic package/lead outlines and is not final, manually art-directed AAA quality or manufacturer-qualified pin-accurate CAD.

Authoritative data candidates live under `Data/`:

- `ModelManifest.csv` — 288 rows with `obj`/`glb` filenames that actually exist. **Never** use a generic `NOT_INCLUDED` string or an inherited model count.
- `ComponentBOM.csv` and `ComponentPlacements.csv` — 236 unique parts and targets; `installed_at_start=FALSE`, `footprint_verified=FALSE`.
- `CopperRoutes_320_REQUIRES_SCHEMATIC.csv` — 320 tasks W001–W320 with `source_pad=UNASSIGNED`, `dest_pad=UNASSIGNED`, `net_id=UNASSIGNED`, `layer=TBD`, `certifiable=FALSE`.
- `ViaTasks_116_REQUIRES_SCHEMATIC.csv` — 116 V001–V116 with `net_id=UNASSIGNED`, `start_layer=TBD`, `end_layer=TBD`, `verified=FALSE`.
- `W321_Recovery.json` — open safety feedback at mission start. `U31.TRUSTED_DETERMINISTIC_READY_OUT -> U32.TRUSTED_DETERMINISTIC_READY_IN` describes **logical roles**, not confirmed physical pins.
- `FaultPlan.json`, `Objectives.json`, `ExportSummary.json`, and `AcceptanceTests_100.csv`.

**Non-negotiable:** Gameplay certification is **BLOCKED: APPROVED SCHEMATIC AND ACTUAL PROJECT INTEGRATION MISSING**. Art-only repairs, visual copper, illustrative teaching simulation, six objective counters and port LEDs must not grant success. The source has no manufacturer-verified device pinout, complete 28-layer stackup, timing budget, safety validation, or hardware-accurate TSN switch RTL.

## 2. Project preflight and preservation

1. Back up or branch the Unity project, preserve Unity `.meta` GUIDs, scenes/prefabs, and all Levels 1–117.
2. Inspect real definitions of `LevelGamePlayManager`, `SparkCircuitSystem`, `SparkElectricalNetworkGraph`, `SparkTerminal`, solver components, terminal binding, virtual PCB repair UI, selection, shop/shelf, scene bindings and save systems. Write the existing signature inventory before new integration.
3. Install source only under `Assets/ProjectSpark/Level118/TSX26/` (unless existing architecture requires an equivalent isolated root); never duplicate `ModelManifest.csv` roots.
4. Run Unity import checks: 288 OBJ meshes, 288 GLB files retained as source, PBR material slots, UVs, normals, Y-up geometry, board bounds, editor compile and asset dependency checks.
5. Expose source-status diagnostics: `Source Validated`, `Unity Compiled`, `Schematic Approved`, `Terminal Bindings Verified`, `Electrical Graph Verified`, `Gameplay Certified`; none is implied by an earlier status.

## 3. Scene and hierarchy

```text
Level118_TSX26_TheDeterministicGrid
  MissionCoordinator                   [UNBOUND until actual APIs inspected]
  AuthoritativeElectricalAdapters      [NO invented SparkTerminal APIs]
  TimebaseAndClockAuthority             [gPTP primary / backup holdover]
  SixPortEthernetAndRedundantPaths
  TimeAwareGateScheduler                [IEEE 802.1Qbv-inspired educational simulation]
  PacketQueuesAndCommandEvidence
  IndependentSafetySupervisor
  ThreeFaultController
  SixObjectivesVerifier
  Board_PhysicalMetres                  [480x340mm]
    EmptyPCB
    ComponentFootprintTargets           [236, all uninstalled]
    PCBRouteTargets                     [320 BLOCKED]
    PCBViaTargets                       [116 BLOCKED]
    ClockAndSafetyContacts              [approved pin IDs only]
  WorldSpaceVirtualRepairCanvas         [reuse existing, do not add physical instrument meshes]
    ClockScope / TimestampDiff / GateTimeline / QueueMonitor
    RedundancyAnalyzer / PacketTracer / LayerViewer / SafetyLogs
  DiagnosticsAndEvidenceLog
  ART_REFERENCE_NOT_GAMEPLAY            [inactive]
```

Keep meter-space authoritative dimensions separate from the tabletop visual preview. For a 15.79 x 8.54-unit tabletop, uniformly scale the **visual-only reference** to about 10.13 x 7.18 units; no nonuniform distortion or solver metre conversion.

## 4. Component BOM, identity and interaction

Instantiate **236 independent, initially uninstalled placement targets** keyed by exact `ref` + board ID. W321 is **already included** in 236. Render component visuals as inventory/showcase prefab assets; do **not** place them into soldered footprints at startup. Reuse the project's existing select/drag/rotate/shop/shelf/throw controllers, with adapter interfaces rather than duplicate input systems. All snap placements need later verified pin-one, package, electrical values, pad geometry, interaction layer and solder checks. Until approved, allow clearly labeled visual rehearsal only, score 0.

Six network connectors are J1–J6 by *design intent*: two redundant uplinks and four deterministic field ports. J7 is power input, J8 debug, J9 PPS/test clock, J10 protected-output terminal. Exact physical connector pin maps and speeds require approval. Critical planned IC roles: U1 TSN switch ASIC, U2 schedule FPGA, U4 hardware timestamp engine, U5/U6 timebase control, U7 redundancy supervisor, U15 gate scheduler SRAM, U31 ready monitor, U32 independent supervisor. Y1 primary time reference, Y2 backup; functions are planning labels, not verified manufacturer devices.

## 5. Electrical graph and 28-layer authority

The actual Project Spark circuit system is the **only electrical truth**. Before enabling W001–W320, obtain a signed pin-level source mapping (`board_id, component_ref, pad_id, package_pin, net_id, source_layer, destination_layer, net_class, copper_geometry, footprint_id, factory/player flag`). For vias provide `via_id, approved_net, from_layer, to_layer, via_type, drill/clearance, position`. Never connect all 28 layers by default; reject wrong net, short, open pad, illegal via span and missing ground return. Distinguish decorative trace art from conductive scene data. Fresh graph evidence must follow each mutation.

## 6. Power, clock quality and gPTP model

Model independent protected supply/return connections, oscillator power, actual reset readiness, qualified primary timing and backup holdover. Time synchronization is an educational bounded **gPTP-like** model, not certified IEEE 802.1AS implementation. Track each timestamp's source clock, offset (ns), frequency error (ppm), uncertainty, measured revision, generation and expiry. A claimed healthy reference must not be inferred from a constant label. On source switching, carry uncertainty and bounded frequency drift; invalidate dependent scheduled-traffic evidence. Confirm no unsafe protected commands while uncertain.

## 7. Six-port network and redundant paths

Define six separately modeled PHY/link states, port ID, source clock, port speed, MAC identity, power/reset, connector contact sets, collisions/BER abstraction, packet error counters and return-path quality. Build two separately verified data paths A and B. Avoid artificially declaring a ring when a path has a single point of failure. Route packets through a bounded deterministic simulation using ingress time, size, VLAN/priority class, queue, serialization delay, gate state, propagation latency and link status; provide replayable packet IDs and end-to-end CRC/payload comparison. Detect duplicate deliveries where redundancy creates copies. No traffic is considered delivered from a UI animation alone.

## 8. Time-aware shaping and deadline proof

Implement an **IEEE 802.1Qbv-inspired teaching model**, explicitly labeled non-certified. Use a configurable cycle, per-priority gate windows, guard bands, queue discipline, offsets, link rate and traffic arrival schedule. A frame can only start if the selected gate is open and can finish within its allowed window under the modeled policy. Validate scheduling constraints and enough serialization time. Collect fresh actual simulated time-of-arrival and delivery events, observed worst-case delay, maximum jitter, deadline misses, queue depth, drops and congestion evidence. Suggested gameplay target for scenario authoring: **<= 500 microseconds** end-to-end for the selected safety frames, **<= 50 microseconds** arrival jitter; these are *fictional educational design targets*, NOT manufacturer or IEEE guarantees and require user-configured authority and proof.

Objective 4 must require a sustained, seeded scenario with background overload, healthy redundant paths, fresh timestamps, and zero unacknowledged safety-command loss. Preserve exact units in graphs and UI. Neither best-effort throughput nor a pretty eye diagram can replace deadline proof.

## 9. Three recoverable authoritative faults

| Fault | Physical target still requiring schematic approval | Required system consequence and repair proof |
|---|---|---|
| F1 — Primary gPTP oscillator drift | Y1 and relevant timing contact/power path | Independent clock measurement exceeds offset/drift threshold, gate launch uncertainty increases. Repair actual oscillator/contact, then fresh clock + deadline tests. |
| F2 — Intermittent redundant Path B via | **specific via ID UNASSIGNED** pending netlist | Conductive graph mutates with deterministic periodic open; logs path failover and outage. Qualified via repair restores topology, then retest both paths and deadlines. |
| F3 — Scheduler SRAM data-line open | U15 SRAM net/pad physical identity **UNASSIGNED** | Corrupt or invalidate time-aware gate table, triggering safety inhibit. Restore physical line/component and require new schedule loading, integrity check and sustained traffic proof. |

Faults must mutate authoritative component/electrical state, never just `Text` labels. Require correct tool, collider, target ID, safety prerequisites, repair transaction, history, source revision and follow-up tests. No simulated repair VFX can change objective authority.

## 10. W321 feedback and independent safety

`W321` begins `OPEN`. Trusted deterministic ready is a monitored feedback path between U31 and U32 **logical** endpoints. Unlike main network ports and supply rails, a W321 removal should open only its designated qualified safety feedback, latch protected-output inhibition and leave unrelated clock/power evidence intact. Reject stuck HIGH, replayed/old timestamps, unsolicited resets and reconnect-only recovery. After reconnect: requalify clock, BOTH redundant paths, gate table, fresh end-to-end command test, and require explicit independent supervisor reset. Stage Objective 6 only after prior objectives are properly certified. Until physical U31/U32 contacts are approved, do not expose a certifiable safety disconnect test.

## 11. World-space AAA repair UI and visual experience

Reuse the existing world-space virtual PCB repair/measurement system. Add network-specific views without creating real-world physical instruments/room assets: gPTP offset chart, timing uncertainty, gate chart, per-priority queue graph, dual-path status, packet tracer, return-path and via-layer inspections, hardware contact probe and safety audit. Mouse/touch selection must resolve unique grounded colliders. Use URP-lit materials, subtle glow, readable TMP text, pooled localized solder/contact VFX, high contrast, consistent error highlighting, streaming from real snapshots not invented happy indicators. Use caching, no per-frame `Find()` or LINQ, and event-driven updates rather than unnecessary `Update()` loops.

## 12. Six objectives and phase order

1. **O1:** 236 installed and verified physical/soldered electronic parts. Disallow unverified footprints and reference auto-installs.
2. **O2:** 320 completed, solver-verified copper routes and 116 legal vias on approved stackup. Disallow all placeholder tasks.
3. **O3:** Protected power; primary/backup clocks; six physical ports; both independently qualified redundant network paths.
4. **O4:** A fresh approved time-aware gate schedule under congestion, meets configured worst-case deadline and jitter with intact packet IDs/payload CRC.
5. **O5:** All three distinct underlying faults diagnosed and repaired; completely new clock, topology, schedule, and safety-command evidence.
6. **O6:** W321 open/inhibit/reconnect; fresh clock/paths/schedule/commands and explicit independent supervisor reset. Final 6/6 only through the **real** `LevelGamePlayManager` API after inspection.

Mission phases: empty art and BOM assembly → verified routing → power/clock/links → schedule under congestion → diagnose F1/F2/F3 → W321 supervised recovery. Maintain distinct **historical milestone completed** and **currently safe/qualified** state; a later fault should invalidate current operational permission even if old milestones remain in history.

## 13. Runtime architecture (proposed only; do not claim these APIs exist)

- `SparkL118SourceAuthoring` / `SparkL118ApprovedSchematicLoader`: strict CSV schema and manifest hashes.
- `SparkL118CircuitAuthorityAdapter`: bridge actual `SparkCircuitSystem` and `SparkTerminal` signatures.
- `SparkL118ClockAuthority`: gPTP/timebase snapshots and qualification.
- `SparkL118SixPortLinkModel`: independent PHY/connector/return-path states.
- `SparkL118RedundantPathController`: two-path eligibility, switching and duplicate detection.
- `SparkL118TimeAwareGateScheduler`: per-class Qbv-style windows and queue backing.
- `SparkL118PacketEvidenceTester`: deterministic delivery, CRC and deadline proof.
- `SparkL118FaultController`: F1/F2/F3 authoritative repair transactions.
- `SparkL118W321SafetySupervisor`: independent feedback edge, inhibition, fresh recovery.
- `SparkL118ObjectiveAuthorityAdapter`: only verified LevelGamePlayManager integration.
- `SparkL118VirtualRepairAdapter`: existing UI and click/touch target mapping.
- `SparkL118PersistentState`: save formats with schema version and source hash.

The included C# `SparkL118TSNTeachingDemo` is only a **non-certifiable learning demonstration** and is never a source for these services.

## 14. Source security, performance and reproducibility

Use deterministic seed/event logs and fixed microsecond or nanosecond time bases; do not step each GHz symbol. Separate physical scale from UI scale. Limit collisions and dynamic mesh overhead; use batching/instancing for repeated passives, serialized unique contact IDs, and pooling for VFX. Collect test reproducibility seeds, simulation revision, source manifest SHA-256, evidence-generation IDs and wall-clock-independent simulation times. Validate save/load against schema revisions. Disable any existing inspector checkbox that can bypass certification. Avoid modifications outside Level118 namespaces/paths absent explicit adapter necessity.

## 15. Unity editor automation and output

Expected installed source: `Assets/ProjectSpark/Level118/TSX26/`.

Target menu paths:

```
Tools/Project Spark/Automatic Level Generator/118/3. Validate Sources and Report Blockers
Tools/Project Spark/Automatic Level Generator/118/4. Rebuild URP PBR Materials
Tools/Project Spark/Automatic Level Generator/118/1. Build FULL Level_118_Layout
Tools/Project Spark/Automatic Level Generator/118/2. Place assembled VISUAL preview on SELECTED tabletop
Tools/Project Spark/Levels/Create Level 118
```

Expected Unity-generated paths:

```
Assets/ProjectSpark/AutoGeneratedLevels/Level118/Level_118_Layout.prefab
Assets/ProjectSpark/AutoGeneratedLevels/Level118/Level_118_Layout.unity
Assets/ProjectSpark/AutoGeneratedLevels/Level118/L118_SourceBlueprint.asset
Assets/ProjectSpark/AutoGeneratedLevels/Level118/ComponentVisualPrefabs/
Assets/ProjectSpark/AutoGeneratedLevels/Level118/URP_PBR_Materials/
Assets/ProjectSpark/AutoGeneratedLevels/Level118/L118_Layout_Validation.txt
```

Builder must be idempotent, avoid overwriting unrelated levels, validate material and model **filename** and expected **288** model rows, fail closed on incomplete OBJ import or incorrect board size. Never pretend Unity imported GLBs natively without an appropriate importer (Unity default uses OBJ fallback).

## 16. Acceptance tests and reporting

`Data/AcceptanceTests_100.csv` contains **100 explicit checks T001–T100** in 10 categories. Provide actual EditMode and PlayMode NUnit tests after inspecting installed Unity packages. Tests depending on unsigned schematic must remain `BLOCKED`; tests not executed must remain `NOT_RUN`, not PASS. Run smoke/regression across Levels 1–117 and shop, virtual repair UI and electrical graph. For final handoff report each test's exact evidence and date, Unity version, scene/prefab paths, component GUIDs, manager integration, warnings, performance on PC/mobile and whether all six objectives were truly earned.

## 17. Required external engineering source to finish AAA gameplay

Before switching from art mode to playable certification, provide/approve: TSX-26 28-layer stackup, complete symbol/footprint library, fully pin-mapped 236-part BOM, W001–W320 nets and source/dest pads, V001–V116 net IDs and legal spans, Y1/Y2 real clock pins/specifications, J1–J6 MAC/PHY/connector contact maps, dual-path topology, U15 schedule SRAM pin map, power/return/reset rail design, safety U31/U32 physical pins and W321 feedback protocol, deterministic educational link-rate/configuration limits, approved deadline budget and explicit fault-to-electrical-device mapping. Until then the implementation result should state **VISUAL AUTHORING COMPLETE — ELECTRICAL / AAA GAMEPLAY BLOCKED** rather than 6/6.

**MASTER PROMPT END — LEVEL 118**
