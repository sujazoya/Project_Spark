# PROJECT SPARK — LEVEL 113: THE REDUNDANCY CITADEL (RDX-21)
## MASTER UNITY IMPLEMENTATION SPECIFICATION — ADVANCED AAA GAMEPLAY TARGET

**Design version:** 1.0 | **Platform:** Unity 6.3.19 LTS, URP, desktop + mobile | **Target:** AAA-style educational motherboard mastery.

**Status:** This package supplies original board-only procedural 3D art, normalized planning records, URP PBR reference maps, Unity editor metadata scaffolding and an illustrative failover sandbox. It does **not** include an approved pin-level schematic, verified 18-layer copper routing, authenticated Project Spark electrical APIs, a working graph-authoritative failover solver, Unity Play Mode results or a finished 6/6 campaign. Do not describe the package as fully playable until those engineering gates are passed.

## 1. Mission identity

Level 113 continues the advanced motherboard arc after Level 111 signal integrity and Level 112 EMC. Whereas Level 112 focuses on suppressing unwanted energy, **The Redundancy Citadel** teaches that a board can survive partial hardware failure only if power paths, data integrity, independent control and fault reporting are truly independent. The fiction centers on an RDX-21 redundant critical computer interface with two separately protected 12V sources, 5V and 3.3V internal supplies, current balancing and power-path OR-ing MOSFETs, two watchdog/control domains, ECC-protected data and fresh safety authorization.

This is a fictional educational board, **not** certified safety hardware, medical apparatus, automotive ECU or real high-availability controller. Never control a real power supply, bypass real interlocks or imply a formal safety-integrity certification.

## 2. Immutable physical and asset contract

- Main board X=0.380m, Z=0.270m, Y-up, 18 inspectable reference layers; origin centered.
- 206 modular OBJ and matching GLB files, plus one assembled showcase OBJ and GLB; **166 installable pieces** and **40 art/structural pieces**.
- 24 source PBR families, 120 source PNG maps plus 24 URP metallic/smoothness packed maps. Material art is procedurally authored and editable, not artist-certified production AAA.
- 232 player-route planning records W001–W232, with **UNASSIGNED** pin endpoints/net IDs/layers and certifiable=FALSE.
- 76 via planning records V01–V76, all net IDs and layer spans TBD/UNASSIGNED.
- Separate removable safety-signal jumper **W233**, included in the 166-piece BOM and **never counted** as route W233 or 167th component.
- The assembled reference remains disabled, non-collidable, electrically inert, and never a source of installed/repair/progress flags.
- Large tabletop **15.79×8.54** units; preview target 10.13×7.20 units, uniform visual scale only. All physics/electrical length calculations remain physical metres.
- Board only: use existing large Project Spark room/table, not new room or furniture, and reuse the existing world-space Virtual PCB Repair UI instead of importing new physical instruments.

## 3. Engineering non-negotiables

Inspect actual source definitions first: `SparkTerminal`, `SparkCircuitSystem`, `SparkElectricalNetworkGraph`, `SparkElectricalSolveContext`, `LevelGamePlayManager`, shop/shelf/placement, component solder repair, virtual instruments and persistence APIs. Document public methods, namespaces, events, ownership and initialization. Do not invent existing method signatures. Prefer additive Level113-specific adapters and preserve Levels 1–112 and Unity GUIDs. Never assume renderer bounds equal approved manufacturer footprints.

Create an approved schematic schema for each package with `componentRef`, `package`, `pinNumber`, `padId`, `netId`, `role`, `ratedVoltage`, `sourceDocument`, plus reviewed 18-layer stack-up, copper widths, clearances and via types/spans. Assign every W001–W232 to specific real pads on the same approved net. Assign each V01–V76 a reviewed span. Explicitly protect primary and backup 12V domains from accidental backfeed or direct shorts. W233 is logic authorization feedback, not a current-carrying power bus. Until the netlist is signed and validated, keep all routing and electrical objectives **NOT_CERTIFIABLE**, with meaningful diagnostics.

## 4. Gameplay initialization and hierarchy

On level start: 0/166 components installed, 0/232 copper routes completed, 0/76 vias installed, 0/6 certified objectives, W233 OPEN and protected load authorization OFF. Only physical PCB/structural art exists. No assembled components in gameplay by default. Proposed scene hierarchy:

```text
Level113_RDX21
  ArtRoot
    EmptyPCBPhysicalMetres
    18LayerInspectionArtwork_DISABLED
    FactoryOnlyArtwork_DISABLED
    ComponentVisualPrefabs_REFERENCE_ONLY
    AssembledShowcaseReference_DISABLED_NEVER_GAMEPLAY
  ElectricalRoot  [blocked until approved schematic]
    VerifiedPackagePadsAndTerminals
    Primary12VInputAndReverseProtection
    Reserve12VInputAndReverseProtection
    ORingMOSFETAndHoldUpGraph
    PlayerRoutesAndVias
    W233TrustedSafetyContact
  SimulationRoot [actual integration required]
    Redundant5VAnd3VControl
    PowerFailoverAndEnergyPlant
    RedundantClocksAndVoting
    ECCMemoryAndFaultRecorder
    IndependentSafetyAuthority
  GameplayRoot
    ComponentPlacementAndSolder
    RouteViaRepairAuthority
    ThreeFaultScenarios
    SixObjectiveEvidence
  UI_AdaptersToExistingWorldSpaceVirtualStudio
    RailMonitor / Scope / CurrentProbe / ECCAnalyzer
    LogicAnalyzer / Watchdog / NetTracer / Repair
  DiagnosticsAndTestExport
```

## 5. Six authoritative objectives

1. **166-component assembly:** validate each exact ref, package, value, orientation, contact and solder. Inspect thermal interface for HS1–HS3 and never count an art renderer as installed. W233 is one of 166.
2. **18-layer construction:** complete 232 approved copper routes and 76 approved vias; pass electrical design rules, polarity isolation, no shorts/backfeed, and signed continuity proof. W233 is separate from the 232 routes.
3. **Redundant-power commissioning:** qualify separately protected primary/reserve 12V inputs; check 5V/3.3V rails, current flow, real OR-ing/switchover and independent supervisor heartbeat. No fake global Green bool.
4. **Failover and data integrity:** repeat reproducible idle/nominal/peak load and primary disconnect tests with measured voltage droop/recovery, capacitance and transfer timing. Run ECC single-bit correction and double-bit detection with fresh syndrome/transaction evidence, cross-check watchdog. No stale measurements can pass.
5. **Three fault recovery:** F1 primary OR-ing MOSFET stuck-open, F2 intermittent ECC memory connection via, F3 independent clock-voter/watchdog drift. Each produces actual graph/plant symptoms; tools diagnose specific targets and repairs modify authority, followed by fresh full tests.
6. **W233 supervised disconnect/recovery:** only after 1–5, disconnect real U15→U6 trusted readiness path and verify independent safety inhibition/latching; reconnect approved terminals, obtain new power/failover/ECC/clock measurements, explicitly reset independent safety, repeat full qualification; award only through actual LevelGamePlayManager.

## 6. Bounded educational physics, not real hardware compliance

Build one deterministic solver-owned state: primary12V, reserve12V, protected5V and control3V3, hold-up capacitor state, primary/reserve path MOSFET conductance, load current (educational profiles 2A, 10A, 20A), power-good thresholds, clock drift, ECC integrity and safety-latch state. Use Kirchhoff-consistent currents, piecewise MOSFET/ideal-diode protection and capacitor dynamics `C dV/dt = I_in - I_load`; finite reserve switchover and recovery (no voltage teleport). Use a stable fixed simulation step, clamp numerical values and explicitly document configurable assumed ESR, C, I limits and MOSFET losses. Simulate ECC syndrome/error injection deterministically with a seeded pattern and actual memory data/transaction authority, not decorative changing integers. A single-bit correctable event must differ from a double-bit uncorrectable fault. Compare both independent clock references with a bounded ppm limit; the independent watchdog must detect frozen, late or missing health messages, not simply accept HIGH.

Keep a separate educational-only **SparkL113ResilienceSandbox** that produces sample synthetic data and `electricallyCertified=false`. It is never connected to LevelGamePlayManager, never changes graph, and must display **SYNTHETIC TRAINING ONLY** if used in any UI. Only integrate real electrical plant after actual project API inspection and approved schematic.

## 7. Three faults and virtual repair

**F1 OR-ing MOSFET stuck open:** inject into reviewed Qn device, show reserve takeover, load droop and current redistribution; approved replacement under isolated power restores correct domain and requires fresh switchover test.

**F2 ECC data via intermittent:** act on an approved net/via, not nearest visual via. Seeded temperature/vibration changes conductance, causes ECC syndromes and errors, repair only via authorized legal via operation, retest continuity + ECC memory.

**F3 clock-voter watchdog drift:** shift actual comparative clock/supervisor model and safety heartbeat validity; diagnose two clocks independently; correct approved component/calibration and rerun freshness/evidence tests.

Reuse existing world-space virtual multimeter, oscilloscope, logic analyzer, power analyzer, thermal viewer, layer viewer, net tracer, solder/desolder, via repair and component replacement. Tool sequence: select UI mode -> click/touch valid PCB collider -> resolve exact target ID and network -> validate safe state and permissions -> request authoritative repair/measurement -> emit localized VFX only after valid action -> invalidate stale evidence -> recompute objectives. No unrelated physical instrument FBXs. Touching tool UI must never trigger underlying PCB edits.

## 8. W233 engineering contract

Source design labels: `L113_HeartbeatAuthentication_U15.TRUSTED_FAILOVER_READY_OUT` -> `L113_IndependentFailsafe_U6.TRUSTED_FAILOVER_READY_IN`. The BOM actually contains U15/U6, but **the labels do not establish physical pins**. Approved pin identities, pulled fail-safe logic and watchdog behavior are required before actual connectivity. It starts OPEN; during normal commissioning it must be closed using real physical interaction. During supervised challenge, opening the actual graph edge triggers independent inhibit/latch but preserves diagnostic power and finite rail physics. Reconnect alone cannot reset authorization; requires fresh multi-domain load, ECC and clock tests and explicit reset.

## 9. Editor menus and integration

`Tools/Project Spark/Automatic Level Generator/113/3. Validate Sources and Report Blockers` runs preflight. `113/4. Rebuild URP Materials` imports 24 native materials. `113/1. Build FULL Level_113_Layout` produces physical empty prefab and scene. `113/2. Place assembled VISUAL preview on SELECTED tabletop` makes a separate non-conductive showroom preview. `Tools/Project Spark/Levels/Create Level 113` aliases the builder. Original file kit installs exclusively beneath `Assets/ProjectSpark/Level113/RDX21/`, generated outputs under `Assets/ProjectSpark/AutoGeneratedLevels/Level113/`; avoid changing any previous level.

The source installer supports `-DryRun` and backs up overwritten files outside Assets. It must never overwrite the entire generator, Unity scene in progress, or change Unity project-wide tags/graphics/input settings. Builders must be idempotent and reject duplicate source kits; no fake terminal or socket objects for unresolved pins.

## 10. Acceptance and release gates

`Documentation/L113_AcceptanceTests_100_NOT_RUN.csv` lists 100 intended tests in 10 domains. All are **NOT_RUN** until executed and evidenced in Unity 6.3 Play Mode, including previous-level regression. Release stage labels must be separate: Source assets delivered, FBX/OBJ/GLB import proven, URP material appearance reviewed, board scale measured, actual netlist approved, real simulation connected, UI/touch tested, faults and safety proven, 6/6 achieved. Offline package CRC and mesh checks do not equate to an AAA playable, safety-certified level. Any missing approved connection must remain BLOCKED, not guessed.

## 11. Recommended production polish (after import and physics integration)

Macro inspection should reveal QFN/QFP metal leads, separate ferrite terminals, heavy current paths, double-zone copper tracework and cooler fins. Build cinematic but restrained cobalt/gold palette with violet reserve-power overlays and amber safety warnings. TAA/SMAA and URP lighting may be tuned in the real Unity project, but never overwrite the project's global render settings from a Level113 installer. Add multi-stage rail switchover holographs, physically localized short sparks, oscilloscope and event-buffer visualization. Use pooling/batching where safe, keep components independently selectable, maintain PC/mobile performance. For true AAA, artist rework, baking/LOD, collision tuning, shaders/VFX and user interaction testing are still required.
