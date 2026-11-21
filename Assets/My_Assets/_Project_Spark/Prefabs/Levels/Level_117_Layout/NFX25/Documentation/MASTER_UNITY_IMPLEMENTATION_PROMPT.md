# PROJECT SPARK — LEVEL 117: THE PACKET CITADEL (NFX-25)
## MASTER UNITY IMPLEMENTATION PROMPT — ADVANCED AAA DESIGN CONTRACT

**Mission category:** Network switching, SFP+-style optical links, high-speed signal integrity, precision time synchronization and safety recovery.

> **Implementation contract:** Implement a fully playable Level 117 in the EXISTING Project Spark Unity 6.3 URP project. Inspect actual API implementations before making integrations. Preserve every existing Level 1–116, every shop, selection, input, electrical, and virtual repair system. The supplied source is original procedural ART plus manifests and a visual-only level authoring foundation; it is not an approved electrical schematic or tested full gameplay.

### 1. Level goal and first state

Create a realistic, board-only advanced network switch and timing laboratory named **THE PACKET CITADEL**, model **NFX-25**. The board is 460 mm by 320 mm and exposes 26 separately viewable layer-art references. It hosts 220 separately installable component models, 52 noninstallable structural/diagnostic art assets, 304 selected player-route tasks, 108 via tasks, four SFP+-style optical network ports, three recoverable faults and independent trusted-feedback jumper W305.

The level must begin at **components 0/220**, **routes 0/304**, **vias 0/108**, **objectives 0/6**, **W305 OPEN**, port links unqualified, protected network workloads blocked. No preassembled reference model can supply electrical authority. Full objective 6/6 is only possible after authoritative solver graph checks, reproducible network simulation, fresh diagnostics, all three repairs, and W305 disconnected/reconnected under independent safety supervision.

### 2. Exact delivered source inventory and grounded constraints

- `Models_OBJ/`: **272 individual OBJ meshes + 273_L117_AssembledReference.obj**. `Models_GLB/`: matching 272 individual GLB meshes + assembled reference GLB. No FBX models are included or claimed.
- `Data/ModelManifest.csv`: exact `asset_id`, file names, roles, source placement and installable flags. Primary Unity build uses the **`obj`** column, never a placeholder `NOT_INCLUDED` field. The validator expects **272**, NOT a copied count from another level.
- `Data/ComponentBOM.csv` and `Data/ComponentPlacements.csv`: exactly **220** matching records. All `installed_at_start=FALSE` and `footprint_verified=FALSE`.
- `Data/CopperRoutes_304_REQUIRES_SCHEMATIC.csv`: **304** selected player tasks. Every pad/net field is `UNASSIGNED`, layer `TBD`, `certifiable=FALSE`.
- `Data/ViaTasks_108_REQUIRES_SCHEMATIC.csv`: **108** tasks with unapproved layer spans, types and nets; all `verified=FALSE`.
- `Data/W305_Recovery.json`: W305 U35→U36 is LOGICAL ONLY; **physical package pins unassigned**. Neither a trusted-ready signal nor power/optical link data may be invented from mesh names.
- `Data/BoardStackup_UNVERIFIED.json`: no approved 26-layer physical stackup, return current, controlled impedance or via technology.
- `Textures/`: 32 PBR families × BaseColor, Normal, AO, Metallic, Roughness = 160 maps; `URP_Packed/`: 32 metallic/smoothness textures. These are procedural authoring materials, not hand-authored production AAA.
- `Previews/NFX25_Procedural_Board_Overview.png` is an illustrative top-down preview, NOT a Unity screenshot or electrical test.
- `Unity/Editor/` contains a source validator, URP PBR binder and visual full-layout authoring builder. `Unity/Runtime/` has nonauthoritative data records and an isolated optical packet/PTP teaching demo.

**Preservation:** Never overwrite existing Level 1–116 scene assets, tags, layers, Unity input, ProjectSettings, electrical classes or shop prefabs. Author Level 117 under its isolated directory and use additive adapters to actual project services.

### 3. Physical board and component expectations

The NFX-25 visual layout has a centralized packet switch (U1), precision timing engine (U2), optical PHY channels U3–U6, high-speed buffering (U7), integrity processing (U8), distinct supply and clock conditioning regions (U9–U14), trusted monitor (U35) and independent supervisor (U36), plus four SFP+-style cage assemblies along the right edge.

The board measures **0.460 × 0.320 metres in Unity metres**. Keep this physical root at `localScale = Vector3.one`. A distinct presentation preview may be scaled uniformly to roughly **10.13 × 7.05 Unity units**, fitting the existing large tabletop without altering physical delay, impedance or length calculations. Never merge the assembled showcase into playable components. The 52 structural meshes are noninstallable and nonconductive unless separately and schematically represented.

Every installable item must have a stable reference ID and asset ID, expected package/value/orientation, independently verified solderable footprint, contacts, terminal pads and qualified placement. For BGA and fine-pitch ICs, validate each logical contact group; a mesh overlap or checked box alone does not make an electrical solder joint. Source coordinates are visual hints, not precision pin locations.

### 4. Required approved electrical schema and design-rule validation

Create a versioned, audited design source containing:

1. Pin-verified BOM, package type, footprint dimensions, pin-one orientation, electrical values and terminal definitions for **all 220 components**.
2. Exact 26-layer stackup, copper/plane regions, dielectric properties, return references, impedance class and loss data.
3. Pad table `{componentRef, packagePin, padId, netId, copperLayer, physicalPoint, terminalKind}`.
4. For all W001–W304: approved source/destination pad IDs, net ID, layer, length, width, clearances, signal domain and whether factory-fixed or player-built. Preserve user-required route tasks; do not treat 304 tasks as all physical copper segments.
5. For all V001–V108: net ID, layer span, drill/barrel geometry, via type, antipad/plane clearances and electrical continuity proof. Never connect 26 layers merely because a via graphic touches them.
6. Four individual SerDes TX/RX paths, module control/presence and optical interface mapping; approval of exact U3–U6 port architecture, SFP+ cage contacts and partner devices.
7. Concrete physical W305 pins for U35 trusted monitor and U36 supervisor, including the actual removable jumper contact identities and signal freshness validation.
8. Exact F1 optical transmitter-bias target, F2 Y2/PTP timing component path and F3 port-4 escape via.

Until this design is approved, **keep all topology-dependent objectives blocked, report `SCHEMATIC_VALIDATION_REQUIRED`, and do not create fake electrical terminals**.

### 5. Realistic educational optical network commissioning

Use a **bounded educational 10GbE-class optical-link model**, not an IEEE-certified SerDes or SFP+ optical hardware emulator. Four ports must be separate electrical and optical participants. Model distinct 12V/3.3V module and core rails where the approved supply design requires, and 1.2V/1.0V internal rails according to actual part specs. Qualify reset, optical module presence, TX_DISABLE, transmitter bias, receive optical power, loss-of-signal (LOS), and negotiated or configured link mode before reporting LINK_UP. Realistic data rate, packet timing, optical power units and carrier detection are separate from the visual LED and world-space screen.

A four-port link state contains `{portId, enabled, modulePresent, powerQualified, transmitterHealthy, receiveSignalDetected, electricalMargins, actualRate, lastTrainingGeneration, diagnosis}`. Any loss of clock, module power, F1 or F3 should change the corresponding underlying state, not merely colors. Allow the player to discover port-specific faults using virtual optical power meter, differential scope, eye analyzer, layer viewer and net tracer. All optical power and fiber losses are simulated, not connected to real hardware.

### 6. Precision clock, PTP and deterministic data plane

Design a bounded IEEE 1588/PTP-style educational synchronization model. Represent master clock, local oscillator, timestamp capture, message transit, path delay and offset calculations. Distinguish disciplined LOCKED state from HOLDOVER and INVALID states. `Y1` supplies nominal 125 MHz teaching-clock reference; `Y2` is the high-stability PTP timing reference pending schematic approval. F2 oscillator drift must affect measured time-offset according to elapsed time and ppm drift, with independent noise/jitter and asymmetrical path delays; do not force zero offset after UI calibration.

Implement packet frames with source/destination, sequence, payload length, traffic class, ingress timestamp, computed CRC and egress/arrival timestamp. Use bounded deterministic queues, FIFO/priority behavior, simulated lane bandwidth, traffic bursts, packet loss, error counters, traffic-shaping windows and delayed/reordered delivery as appropriate. Include reproducible traffic tests for all four ports. A visual '100% packets' animation is not evidence. Log exact packets sent/received, lost/duplicate/corrupted frames, expected versus observed CRC, latency/percentiles, tested duration and topology generation. Keep PTP sync evidence and traffic evidence versioned separately.

Use an optional, clearly labeled illustrative non-authoritative demo. It may simulate sample packet loss/ppm drift but **must never write to the real circuit, grade objectives, override verification, or report live measurements**.

### 7. Three recoverable faults

**F1 — Port 3 optical transmitter bias collapse.** On the APPROVED optical port-3 driver or transmitter-power connection, inject an actual low-bias/open state. Expected result: TX optical power falls or module reports fault, port 3 fails appropriate link/traffic checks, and instrumentation can localize the power/driver issue. Repair demands correct value/part, unpowered solder/replacement and post-repair optical/link/traffic qualification.

**F2 — PTP precision oscillator drift.** Inject a documented distortion or drift into Y2 or its approved timing path. Expected result: time offset accumulates deterministically, event timestamp lock becomes degraded, and scheduled traffic/PTP certification fails. Repair through correct clock part or contact state; rerun independent timing verification and packet traffic.

**F3 — Port 4 differential escape via intermittency.** One exact approved via in port 4's copper SerDes path opens under seeded deterministic thermal/mechanical conditions; the topology causes eye-margin degradation, carrier/recovery events or packet errors. Diagnose with 26-layer view and differential eye/TDR tracing, repair only the approved layer span, then remeasure link and packet integrity.

Faults must be staged and traceable with `faultId`, origin, revision, reproducible seed, repair authorization, before/after measurements and post-repair evidence generation. Do not award fault recovery from effects or arbitrary debug flags.

### 8. W305 independent safety restoration

Trusted monitor U35 produces a qualified network-ready signal that passes through physically removable **W305** to independent U36 supervisor. This is an **authorization/health signal, not power, SFP transceiver current, or data pair**. Without verified U35/U36 physical pads and a concrete jumper socket, W305 remains **not certifiable**.

During a supervised final challenge after objectives 1–5: record baseline four-port and timing qualification, deliberately disconnect W305's real conductive path, confirm independent detection of missing/stale signal and latching of protected network workload inhibition while retaining diagnostic supply. Reconnect alone does NOT clear the latch. Require a brand-new four-port electrical/optical qualification, PTP lock, packet integrity data test, and explicit independent supervisor reset with fresh heartbeat. Reject constant HIGH, copied debug output and stale measurements.

### 9. Six authoritative objectives

1. **O1 Assembly:** exactly 220 correctly placed, value/footprint/orientation-verified items with audited electrical contacts. Any missing/unqualified component blocks award.
2. **O2 Routing:** all 304 approved selected copper tasks and 108 approved via tasks with actual solver continuity, no shorts and W305 commissioned connection; do not bypass tasks with showcase artwork.
3. **O3 Four-port commissioning:** qualified power/clock/reset, four optical ports link up at the approved 10GbE-class teaching target and independent link-training evidence.
4. **O4 Packet/PTP proof:** fresh packet CRC/latency/priority/queue tests and precision timing proof with acceptance limits; zero observed finite errors is not proof of zero BER.
5. **O5 Fault recovery:** F1, F2 and F3 each diagnosed, correctly repaired and freshly requalified (clock, links and data plane).
6. **O6 W305 recovery:** actual open, safety inhibition, physical close, fresh four-port/PTP/packet evidence, explicit safety reset and restored protected authorization.

Use actual `LevelGamePlayManager` events after discovering its real method signatures. Start at **0/6**. Support stable history with separate current evidence validity; a UI click, preview, visual component count or non-authoritative demo must never award a real objective.

### 10. Existing workshop UI and virtual instruments

Reuse the existing large table/room, world-space PCB studio, the multimeter, oscilloscope, logic analyzer, net tracer, layer viewer, solder/desolder, component replacement and fault repair tools. Add **virtual SFP optical power meter**, **PTP clock monitor**, **packet analyzer**, **4-port health dashboard**, **traffic-queue/latency view** and **W305 safety panel** as world-space Canvas elements. Do not introduce physical standalone oscilloscopes, furniture or environment FBXs. For each click/touch: prioritize UI pointer hit, then physics raycast against approved component/pad/contact collider; validate tool-target compatibility, safety and solver state; dispatch typed request; apply actual authoritative action; emit restrained surface-local VFX only after acceptance. Support mouse, mobile touch and keyboard/gamepad accessibility.

### 11. Save/load, determinism, performance, audio and AAA art

Persist manifest version/hash, approved netlist version, each component installed/contact condition, task status, every fault state, all link states, last training/time/traffic evidence revision and U36 safety latch. On load, rebuild authoritative circuit topology BEFORE restoring evidence, then invalidate any evidence whose topology or firmware revisions differ. Bound per-frame packet work, queue size and logging. Use seeds for deterministic faults and repeatability. Pool highlights and VFX; keep idle CPU load low, avoid repeated `FindObjectsOfType`/LINQ in hot paths. Procedural PCB textures and UVs are source references; a final artist pass should implement realistic dielectric roughness, gold contacts, finely modelled optical cages, label legibility, physically motivated emission, camera lighting and subtle laboratory audio. **Do not fake live telemetry with static shader glows**.

### 12. Required project code boundaries (proposed, NOT existing API claims)

`SparkL117LevelCoordinator`, `SparkL117SourceManifestImporter`, `SparkL117BoardTopologyAdapter`, `SparkL117ComponentPlacementValidator`, `SparkL117OpticalPortModel`, `SparkL117SerDesTimingModel`, `SparkL117PtpClock`, `SparkL117PacketFlowEngine`, `SparkL117FrameIntegrityTest`, `SparkL117TrafficScheduler`, `SparkL117FaultController`, `SparkL117W305Supervisor`, `SparkL117ObjectiveTracker`, `SparkL117VirtualToolBridge`, `SparkL117SaveState`, `SparkL117ElectricalValidator`. Do not duplicate or override the real `SparkCircuitSystem`, `SparkTerminal`, `SparkElectricalNetworkGraph` or `LevelGamePlayManager`. Use interfaces and project-proven adapters only.

### 13. One-click Unity authoring commands and target output

The full visual generator must expose these editor menus exactly:

- `Tools/Project Spark/Automatic Level Generator/117/3. Validate Sources and Report Blockers`
- `Tools/Project Spark/Automatic Level Generator/117/4. Rebuild URP PBR Materials`
- `Tools/Project Spark/Automatic Level Generator/117/1. Build FULL Level_117_Layout`
- `Tools/Project Spark/Automatic Level Generator/117/2. Place assembled VISUAL preview on SELECTED tabletop`
- `Tools/Project Spark/Levels/Create Level 117`

Required generated output: `Assets/ProjectSpark/AutoGeneratedLevels/Level117/Level_117_Layout.prefab`, `.unity`, `L117_SourceBlueprint.asset`, `L117_Layout_Validation.txt`, `ComponentVisualPrefabs/`, and `URP_PBR_Materials/`. A second build should update the target assets without duplicating root objects or touching other levels. The builder may generate ONLY a **visual-only noncertifiable** source shell until actual electrical APIs and approved pin data are available. Avoid one-click false claims of full gameplay completion.

### 14. Explicit current blockers

- **B1:** Physical pin-level NFX-25 schematic and verified 26-layer stackup absent.
- **B2:** All 304 route endpoints/net IDs and layer assignments `UNASSIGNED` / `TBD`.
- **B3:** All 108 via net IDs, spans, geometries and technologies unverified.
- **B4:** All 220 component footprints and package pin maps unverified.
- **B5:** U35/U36 physical identities, W305 link pad IDs and safety semantics not approved.
- **B6:** No verified SFP+ module data sheet, optical link partner/power map, PLL distribution map or SerDes path simulation parameters.
- **B7:** Actual Project Spark game APIs and Unity Play Mode not available for validation in this supplied art kit.

Never silently manufacture these missing facts. Emit `BLOCKED: SCHEMATIC_VALIDATION_REQUIRED` as appropriate. Preserve 0/6 until certified tests truly pass.

### 15. Definition of done and final developer response

Produce file-by-file inventory, exact Unity menu instructions, every asset-to-prefab mapping or explicit decorative exclusion, approved schematic revision/hash, all electrical-blocker decisions, Unity compile/Play Mode evidence, test logs, actual measured packet/clock/link/fault values, and only a genuine final `LevelGamePlayManager` 6/6 result if achieved. Require all 100 acceptance checks below in EditMode/PlayMode, with `PASS`, `FAIL`, `BLOCKED` or `NOT_RUN` status and evidence. Existing Levels 1–116 smoke tests are mandatory. Until then, report **ART/EDITOR FOUNDATION COMPLETE, GAMEPLAY PARTIALLY IMPLEMENTED**, not finished AAA certification.

---

## 16. Acceptance tests (100 required checks; initial status NOT_RUN)

### Assets and build

- [ ] **T001** — 272 modular OBJ assets import with meshes *(NOT_RUN)*
- [ ] **T002** — 272 matching GLB assets remain available as alternatives *(NOT_RUN)*
- [ ] **T003** — assembled reference disabled and never counted as installed *(NOT_RUN)*
- [ ] **T004** — physical board bounds are 0.460 x 0.320 metres *(NOT_RUN)*
- [ ] **T005** — 26 inspection layers are visually isolated and unverified *(NOT_RUN)*
- [ ] **T006** — 220 unique component IDs join manifest/BOM/placements *(NOT_RUN)*
- [ ] **T007** — 52 noninstallable models cannot grant assembly progress *(NOT_RUN)*
- [ ] **T008** — 32 material families and masks remap without missing slots *(NOT_RUN)*
- [ ] **T009** — OBJ/GLB local positions and Unity Y-up are consistent *(NOT_RUN)*
- [ ] **T010** — builder re-run recreates one level root without duplicating assets *(NOT_RUN)*

### Assembly and topology

- [ ] **T011** — initial assembly progress exactly 0/220 *(NOT_RUN)*
- [ ] **T012** — wrong component part/value/pin-one orientation rejected *(NOT_RUN)*
- [ ] **T013** — no BGA contact verified by superficial mesh overlap *(NOT_RUN)*
- [ ] **T014** — copper W001-W304 begins incomplete and blocked *(NOT_RUN)*
- [ ] **T015** — unassigned source/destination pad prevents certification *(NOT_RUN)*
- [ ] **T016** — unassigned net prevents certification *(NOT_RUN)*
- [ ] **T017** — factory art cannot bypass unfinished player routes *(NOT_RUN)*
- [ ] **T018** — vias V001-V108 remain blocked for TBD spans *(NOT_RUN)*
- [ ] **T019** — wrong-net copper creates diagnostic short, never points *(NOT_RUN)*
- [ ] **T020** — safe solder/rework updates solver graph only after validation *(NOT_RUN)*

### Power and timing

- [ ] **T021** — power 12V/3.3V/1.2V/1.0V supply domains modeled separately *(NOT_RUN)*
- [ ] **T022** — missing ground return blocks power certification *(NOT_RUN)*
- [ ] **T023** — optical port supply loss blocks that port initialization *(NOT_RUN)*
- [ ] **T024** — power sequencing and reset gating enforced *(NOT_RUN)*
- [ ] **T025** — 125 MHz timing reference has independently measured jitter *(NOT_RUN)*
- [ ] **T026** — PTP oscillator drift modifies timestamp offset in seconds/ppm *(NOT_RUN)*
- [ ] **T027** — faulty clock does not silently produce qualified time lock *(NOT_RUN)*
- [ ] **T028** — holdover mode marks timestamps degraded, not valid PTP *(NOT_RUN)*
- [ ] **T029** — bad voltage/fault invalidates fresh link evidence *(NOT_RUN)*
- [ ] **T030** — port link-up cannot be asserted by a decorative LED *(NOT_RUN)*

### Four optical port links

- [ ] **T031** — four port IDs mapped to distinct PHY/module paths *(NOT_RUN)*
- [ ] **T032** — optical module missing means link-down *(NOT_RUN)*
- [ ] **T033** — loss-of-signal and TX_DISABLE modeled independently *(NOT_RUN)*
- [ ] **T034** — port 3 optical TX bias failure changes link state *(NOT_RUN)*
- [ ] **T035** — port 4 escape via open damages actual differential path *(NOT_RUN)*
- [ ] **T036** — per-port eye margin based on approved route geometry *(NOT_RUN)*
- [ ] **T037** — optical TX/RX power displayed with correct reference units *(NOT_RUN)*
- [ ] **T038** — port power and module presence do not imply carrier lock *(NOT_RUN)*
- [ ] **T039** — fiber partner mismatch prevents port qualification *(NOT_RUN)*
- [ ] **T040** — required four-port full-speed state is fresh and verified *(NOT_RUN)*

### Packet and congestion model

- [ ] **T041** — frame ingress/egress with bounded deterministic queues *(NOT_RUN)*
- [ ] **T042** — CRC errors alter acceptance and counters with provenance *(NOT_RUN)*
- [ ] **T043** — packet loss not inferred solely from queue occupancy *(NOT_RUN)*
- [ ] **T044** — VLAN or priority class labels preserve packet segregation *(NOT_RUN)*
- [ ] **T045** — duplicate sequence numbers detected *(NOT_RUN)*
- [ ] **T046** — bounded high-load test reports number of delivered frames *(NOT_RUN)*
- [ ] **T047** — latency and jitter computed from timestamps not hardcoded *(NOT_RUN)*
- [ ] **T048** — packet error injection is seeded and replayable *(NOT_RUN)*
- [ ] **T049** — link drop stops or retries in-flight packets with evidence *(NOT_RUN)*
- [ ] **T050** — objective four requires fresh complete data integrity test *(NOT_RUN)*

### PTP and traffic scheduling

- [ ] **T051** — PTP Sync/FollowUp/DelayReq/DelayResp educational exchange *(NOT_RUN)*
- [ ] **T052** — master and slave timing state distinguish lock and holdover *(NOT_RUN)*
- [ ] **T053** — oscillator skew is reflected in calculated offset *(NOT_RUN)*
- [ ] **T054** — path asymmetry impacts time-offset calculation *(NOT_RUN)*
- [ ] **T055** — measured offset cannot become exact zero by UI override *(NOT_RUN)*
- [ ] **T056** — gated schedule never sends out of eligible time window *(NOT_RUN)*
- [ ] **T057** — dropped PTP message invalidates fresh sync evidence *(NOT_RUN)*
- [ ] **T058** — stale PTP time lock invalid after a repair/route edit *(NOT_RUN)*
- [ ] **T059** — supported network rate documented as educational 10GbE-class *(NOT_RUN)*
- [ ] **T060** — no claim of IEEE 1588/TSN certification without test evidence *(NOT_RUN)*

### Faults F1-F3

- [ ] **T061** — F1 changes specific approved port 3 TX driver electrical state *(NOT_RUN)*
- [ ] **T062** — F1 repair requires approved tool/target and fresh optical test *(NOT_RUN)*
- [ ] **T063** — F1 optical glow animation alone cannot repair *(NOT_RUN)*
- [ ] **T064** — F2 changes Y2 oscillator/clock net characteristics *(NOT_RUN)*
- [ ] **T065** — F2 changes PTP skew and fresh measurement *(NOT_RUN)*
- [ ] **T066** — F2 repair requires approved clock package/value *(NOT_RUN)*
- [ ] **T067** — F3 changes approved port 4 differential via continuity *(NOT_RUN)*
- [ ] **T068** — F3 disturbance schedule is deterministic *(NOT_RUN)*
- [ ] **T069** — F3 repair requires valid layer-span via rework *(NOT_RUN)*
- [ ] **T070** — objective five requires all faults fixed and fresh traffic/PTP results *(NOT_RUN)*

### W305 independent safety

- [ ] **T071** — W305 starts OPEN and uncertifiable *(NOT_RUN)*
- [ ] **T072** — U35/U36 package pins unresolved blocks functional construction *(NOT_RUN)*
- [ ] **T073** — W305 is not network data or power supply *(NOT_RUN)*
- [ ] **T074** — physical disconnect forces trusted link path open in graph *(NOT_RUN)*
- [ ] **T075** — old/stuck HIGH cannot mimic fresh qualified heartbeat *(NOT_RUN)*
- [ ] **T076** — protected transfers inhibited while diagnostics remain active *(NOT_RUN)*
- [ ] **T077** — reconnection alone does not clear safety latch *(NOT_RUN)*
- [ ] **T078** — all four links and PTP need fresh requalification *(NOT_RUN)*
- [ ] **T079** — fresh packet tests must succeed after reconnection *(NOT_RUN)*
- [ ] **T080** — explicit safety reset required before objective six *(NOT_RUN)*

### Virtual tools, feedback and save

- [ ] **T081** — world-space UI pointer hits do not route underneath panels *(NOT_RUN)*
- [ ] **T082** — optical power meter results derive from verified simulated paths *(NOT_RUN)*
- [ ] **T083** — TDR/eye/clock display reports source and evidence generation ID *(NOT_RUN)*
- [ ] **T084** — unsupported tool-target action rejected without state changes *(NOT_RUN)*
- [ ] **T085** — repair effects use clicked physical contact location *(NOT_RUN)*
- [ ] **T086** — VFX events do not directly update objective counter *(NOT_RUN)*
- [ ] **T087** — save records topology revision, fault state and generation IDs *(NOT_RUN)*
- [ ] **T088** — load invalidates stale qualification evidence *(NOT_RUN)*
- [ ] **T089** — Preview-only packet demo cannot award objectives *(NOT_RUN)*
- [ ] **T090** — mobile touch, desktop pointer and keyboard navigation work *(NOT_RUN)*

### Objectives and regression

- [ ] **T091** — zero of six objectives at start *(NOT_RUN)*
- [ ] **T092** — objective one requires 220 verified installed parts *(NOT_RUN)*
- [ ] **T093** — objective two requires approved 304 routes and 108 vias *(NOT_RUN)*
- [ ] **T094** — objective three requires fresh four-port commissioning evidence *(NOT_RUN)*
- [ ] **T095** — objective four requires current packet and PTP checks *(NOT_RUN)*
- [ ] **T096** — objective five requires three independent repairs with fresh tests *(NOT_RUN)*
- [ ] **T097** — objective six requires actual W305 disconnect and recovery *(NOT_RUN)*
- [ ] **T098** — duplicate events never double-award objectives *(NOT_RUN)*
- [ ] **T099** — Levels 1-116 gameplay and assets remain unchanged *(NOT_RUN)*
- [ ] **T100** — actual Play Mode proves complete 6/6 before any release claim *(NOT_RUN)*

## 17. Source-grounded annexes

Use exact source files instead of transcribing made-up pins. The canonical truth tables are the kit’s `Data/ModelManifest.csv` (272), `Data/ComponentBOM.csv` (220), `Data/ComponentPlacements.csv` (220), `Data/CopperRoutes_304_REQUIRES_SCHEMATIC.csv` (304), `Data/ViaTasks_108_REQUIRES_SCHEMATIC.csv` (108), `Data/W305_Recovery.json`, `Data/FaultPlan.json`, `Data/Objectives.json`, `Data/BoardStackup_UNVERIFIED.json`, `Data/MaterialFamilies.json`, and `Data/AcceptanceTests_100.csv` (100).

**Truth boundary:** OBJ/GLB meshes, texture maps and Editor transforms provide appearance only; no electrical pinout, valid packet transfer, PTP lock, optical output power, safety logic or objective is certified by their existence.
