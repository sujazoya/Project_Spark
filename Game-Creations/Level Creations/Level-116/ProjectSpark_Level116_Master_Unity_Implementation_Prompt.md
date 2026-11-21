# PROJECT SPARK — LEVEL 116: THE DATA PHOENIX
## MASTER UNITY IMPLEMENTATION PROMPT — SOURCE-GROUNDED, AAA-STYLE PRODUCTION CONTRACT

**Board:** NVX-24 | **Category:** Storage Reliability / NVMe Systems | **Engine:** Unity 6.3 LTS URP | **Platforms:** Desktop and Mobile | **Scene:** Existing Project Spark workshop/table.

> Implement a fully playable, technically defensible Level 116 using the accompanying `ProjectSpark_Level116_NVX24` procedural art/data source and the actual installed Project Spark codebase. This is an implementation specification, NOT a claim the included art kit already contains playable storage circuits. Inspect the real existing electrical solver, component placement, shop, LevelGamePlayManager, and world-space virtual PCB Repair Studio APIs before integrating. Never invent working APIs, pins, connections, timing specs, or simulation test results.

### 0. Mission / cinematic identity

**Title: The Data Phoenix.** A storage module boots, appears to work, and then loses information at the worst possible instant. The player must commission a flash-based NVMe educational controller, build/repair approved PCB connections, test real simulated error-correction and durable write transactions, diagnose failures, and demonstrate that accepted writes survive an interrupted power rail. The final W289 incident deliberately removes independent trust in the storage-ready feedback signal: data that *appears* intact cannot be used by the protected processor until fresh verification and an explicit safety reset.

AAA presentation: professional laboratory inspection of a genuine-looking PCB, dark blue solder mask, gold ENIG contacts, black packages, individual solder balls, metallic heatsink, large power-loss backup capacitor bank, realistic close inspection. **Do not create a new room or physical handheld instruments.** Reuse existing large engineering tabletop, lighting and world-space virtual test panels. VFX communicate actual simulated events, never prove a test passed.

### 1. Source inventory and required initial state

- Physical 0.440 m × 0.310 m board; **24 copper-layer artwork references**, not an approved 24-layer PCB stack-up.
- Exactly 256 modular OBJ models and 256 matching GLBs, plus one separate assembled OBJ/GLB artwork reference (`257_L116_AssembledReference`). Procedural geometry units = metres, Y-up.
- 208 installable source parts (including removable W289), 48 additional board/layer/diagnostic art assets.
- 30 independent material families; 150 PBR source maps and 30 URP packed metallic/smoothness maps.
- 288 selected route tasks W001–W288 with all source/destination nets `UNASSIGNED` and `certifiable=FALSE`; 100 via tasks V001–V100 with `TBD` layer span and `UNASSIGNED` net.
- Initial: `installed=0/208`, `routes=0/288`, `vias=0/100`, `W289=OPEN`, `objectives=0/6`, power OFF, NAND UNINITIALIZED, protected writes DISABLED.
- W289 is NOT route W289 among required copper segments. It is a distinct removable trusted-ready jumper, included once in the component BOM.
- Source intended W289 endpoints: logical **U31 trusted-storage-ready monitor** -> logical **U30 independent safety supervisor**, no approved package pin/ball IDs. Keep `SCHEMATIC_VALIDATION_REQUIRED` until resolved.
- Full electrical gameplay requires approved pin-level PCB, actual device footprints, power constraints, NAND capacity/channel organization and deterministic storage model. Without these it is **PARTIALLY IMPLEMENTED / NOT_CERTIFIABLE**.

### 2. Project integration and preservation

1. Back up the project; preserve Levels 1–115 and their `.meta` identities, prefab connections, gameplay managers and Universal Automatic Level Generator V2.
2. Inspect existing Unity namespaces, assemblies, installed packages, SparkCircuitSystem, SparkElectricalNetworkGraph, SparkTerminal equivalents, full component lifecycle and the LevelGamePlayManager's actual achievement method signatures.
3. Keep authoring Editor scripts in `Unity/Editor` under namespace `ProjectSpark.Level116.Editor`; runtime records and adapters under namespace `ProjectSpark.Level116`. Avoid global static mutations or using Find() each frame; cache references.
4. Create only a Level116-specific scene and generated Level116 assets. Idempotently reconcile/replace ONLY these generated paths. No new room, shelves, physical instruments or environment FX.
5. Use real-world metre-based topology/measurements and separate preview scale, with correct transform conversion for raycast hits, cable lengths, thermal data and collider interaction.
6. Bind every component by unique source asset_id and validated reference/board owner. No electrically effective assembled-reference artwork.
7. Preserve original OBJ/GLB filenames. Validate actual imported mesh bounds, normals, orientation and UVs inside Unity 6.3. The procedural textures are STARTER PBR, not finished AAA artist-grade maps.

### 3. Scene architecture

```
Level116_NVX24_TheDataPhoenix
  VisualArtRoot_Metres
    EmptyPCB_Physical_440x310mm
    LayerArtwork_24_DISABLED_NONCONDUCTIVE
    OtherArtwork_DISABLED_NONCONDUCTIVE
  GameplayAuthoringRoot_NONCERTIFIABLE
    208_UninstalledComponentTargets
    288_Routes_UNASSIGNED_BLOCKED
    100_Vias_UNASSIGNED_BLOCKED
    InventoryPrefabs_AsAssets_NotYetInstalled
  ActualRuntimeIntegration_REQUIRED
    ComponentInstallationAdapter
    ElectricalGraphAdapter
    NANDControllerAndChannelAdapter
    PowerLossProtectionAdapter
    ECCAndCrashConsistencyAdapter
    WorldSpaceRepairToolsAdapter
    IndependentW289SafetyAdapter
    LevelGamePlayManagerEvidenceAdapter
  DiagnosticsAndEvidence
  OPTIONAL_Journal_PLP_Teaching_DEMO_NONCERTIFIABLE
  Assembled_ART_REFERENCE_DISABLED_NEVER_GAMEPLAY
```

The supplied isolated art builder creates a subset of this authoring hierarchy and leaves actual runtime adapter nodes unbound. Do not mark runtime services implemented just because an empty GameObject was created.

### 4. Component assembly / physical target validation

All 208 parts must begin uninstalled and appear only in inventory or the optional disabled reference. Component mounting requires correct BOM identity, package/footprint, pin-one, orientation, contact pattern and value. NAND1..NAND8 are separate packages associated with eight illustrative channels; a physical NAND channel pin map must be approved, and a BGA solder ball requires independent validation. U1 is controller, U2 PLP supervisor, U30 independent supervisor and U31 ready monitor. All other device identities are proposed educational roles, not official IC part numbers.

Player interactions: select item -> ghost hover -> rotate/align -> seat -> solder/contacts -> independently verify. A misplaced/incorrect part cannot count, render glow alone never qualifies. Removing/replacing a part invalidates only dependent evidence. Touch, mouse, keyboard/gamepad and mobile performance must be supported through existing input adapters.

### 5. Actual physical schematic/stack-up gate (MUST NOT BYPASS)

Obtain reviewed schematic+PCB geometry with source revisions and hashes:
- Every active component's physical pins/balls: `board,ref,pin,pad,net,layer,x,y,z,type`.
- NAND channel 0..7 chip-enable/ready-busy, DQ, clocks/strobes and reset/control, boot ROM, 1.2/1.8/3.3 V rails, returns, power-path devices, watchdog and PLP hold-up control.
- NVMe differential TX/RX pairs, coupling capacitors, receiver/clock/reset, actual package/edge-contact connections; don't claim any particular connector electrical configuration without approved design.
- Real 24-layer stack-up (signal/plane assignments, thickness, dielectric, via rules, return paths) and all factory-fixed copper vs player-built segments.
- All W001–W288 exact source/dest pads, nets, layers and design rules, plus V001–V100 exact net/type/plating/span and cross-layer continuity.
- W289's actual source and destination physical pins on U31 and U30, matched to approved component identities, safety signal levels and independent monitor authority.
- Actual PLP capacitor capacity/ESR/voltage range, thermal limits and allowable write power under defined load.

A visually drawn line, imported OBJ layer mesh, or color overlay is not an electrical graph edge. Fail closed on `UNASSIGNED`/`TBD`. Report unsatisfied gates rather than inventing pins. **All 288 and 100 source task records are design placeholders.**

### 6. Power, NAND and NVMe initialization

Implement a deterministic educational NAND/SSD model with independent protected input, controller rails, NAND rails, power-return validity, reset state, clock readiness, controller firmware/FTL revision, and eight independent channel states. The model must distinguish idle power, NAND ready, selected channel ready, protocol link ready, and protected write authorization. A device enumerating is not equivalent to durable data integrity. A power loss changes state according to available modeled hold-up energy, actual in-flight commits and approved protection sequencing.

NAND model scope: define educational page, block, plane, die, erase count, bad block allocation, program/erase limitations, bit-error injection, retention temperature effects and read retry rules. The exact density, timing, ECC parity, LDPC parameters, cache behaviour and device organization must be chosen, documented, and validated against a specific approved model: never claim hardware/standards certification.

NVMe model scope: queue initialization and bounded commands, submission/completion identifiers, reads, writes, flushes, abort/reset, command timeouts, error/status codes and optional intelligent controller telemetry. Simulate command effects at a computationally manageable rate rather than stepping a full gigabit-scale bus edge by edge.

### 7. ECC and end-to-end data integrity

Build an inspectable deterministic ECC pipeline: injected raw errors, syndrome or bounded LDPC-style algorithm, detection, correction up to a documented capability and explicit uncorrectable state. Track the raw error count separately from corrected and uncorrectable errors. Detect wrong page/address, stale metadata, bad blocks, checksum mismatches and overwritten journal pages. Any source says zero errors should mean zero **observed in finite test**, not mathematically guaranteed lifetime BER=0.

Use seeded test runs with address walking, sequential and pseudorandom payloads, CRCs/hashes, variable traffic, reads after writes and post-power-cycle verification. Store data-size, seed, duration, revision, failure location, expected/actual, verified timestamp and full configuration revision.

### 8. Power-loss protection / journal crash consistency

The external optional demo uses only idealized `0.5*C*(Vhigh²−Vlow²)` for classroom explanation; it is NOT PLP authorization. Actual simulated PLP budget includes ESR, load profile, conversion efficiency, temperature and minimum controller/NAND operating voltage; lower rail voltage or failed capacitor must reduce real available duration.

Use a sequence such as: validate write authorization -> stage write-ahead intent -> program candidate data -> verify page ECC and CRC -> persist metadata commit marker -> atomically select new checkpoint -> acknowledge durability. Inject deterministic power failure after any transition. After restart, a replay engine chooses only a valid durable checkpoint, discards incomplete writes and proves no acknowledged write loss. Define the exact expected persistence contract: a device may legitimately lose *unacknowledged* writes, never advertise them as acknowledged. Model cache flush, FUA-like requests if supported and atomic metadata updates. Timed tests must record pre-failure snapshot, commit markers, post-boot replay and every recovered page ID.

### 9. Faults / repair evidence

**F1 — NAND channel 3 chip-enable microvia open.** Choose one actual approved NAND CE# net/physical via. Fault injection must break that conductive graph edge, cause channel-specific command timeouts and appear in the net tracer. Qualified via rework in safe state must restore continuity, then fresh channel initialization, write/read/ECC tests.

**F2 — PLP high-ESR capacitor.** Choose one approved source BOM PLP capacitor; high ESR or reduced capacitance must numerically lower available energy/voltage under load and cause appropriately timed incomplete durable flushing. Repair requires value/rating-correct replacement, fresh PLP measurement, supervised power-drop, and journal verification.

**F3 — Metadata ECC/journal recovery fault.** Select an approved metadata page/block/signal path and define deterministic correctable vs uncorrectable errors. The failure must compromise replay or integrity check under a documented condition. Authorized repair (bad-block remap, qualified storage-path repair, verified restore/recovery) must be appropriate to cause, and never simply clear all error flags.

Faults are sequential scenarios, not three unavoidable simultaneous initial faults. Each has evidence of injected physical/data state, failed measurement, correct tool choice, repair transaction, changed authoritative state and fresh requalification. VFX cannot certify a repair.

### 10. W289 independent trusted-storage-ready disconnect/recovery

The art source's W289 is a removable jumper, included in 208 parts but separate from 288 copper tasks. It carries a **health/authorization signal**, not NAND main power or NVMe data. Approved physical pins must be assigned for U31 trusted monitor -> U30 independent supervisor before runtime W289 testing can begin. The monitor must send fresh sequence-numbered or otherwise monotonic validity; static HIGH/frozen status cannot pass. Only stage the supervised disconnect after earlier objectives are truly qualified.

On the physical jumper opening, the authoritative graph must open and independent supervisor must latch a safety fault, inhibiting protected writes while leaving qualified diagnostic power. Reconnection alone is insufficient; require approved graph reconnect -> fresh PLP test -> fresh eight-channel ECC and page tests -> fresh journal and power-cut replay -> independent safety reset -> new trustworthy storage authorization. Prove a cached healthy signature is rejected. Historical objective awards may remain visible but operational qualification becomes invalid.

### 11. Six authoritative objectives

O1: all 208 parts installed correctly with verified solder contacts and approved footprints.
O2: all 288 selected user routes and 100 vias physically valid in the solver, with a valid 24-layer net graph, W289 connected for commissioning.
O3: power, NAND channel init, NVMe educational transport, PLP state and all interconnects independently commissioned.
O4: fresh end-to-end ECC, deterministic data tests, crash-consistency proof and protected write durability.
O5: F1/F2/F3 distinctly diagnosed and physically/algorithmically repaired, fresh full tests.
O6: W289 disconnected during supervised active operation, independently inhibited, reconnected, fully retested and explicitly safety-reset.

Use the **actual** `LevelGamePlayManager` event interface discovered in the project; never invent signatures. Start at 0/6. Award only after authoritative, current-revision evidence events; UI indicators, mesh count, demo output, animation, or direct inspector flags have no authority. Never count a repair twice and never reuse stale test results across a topology/power/firmware revision.

### 12. AAA world-space virtual inspection and audio visual design

Reuse the existing large tabletop/room, viewer input, selection system, holographic outline, actual repair/tool UI and Spark character/voice architecture if available. Use event-driven updates with cached target IDs. Add virtual panels as appropriate: multimeter, logic analyzer, NAND channel timing, ECC syndrome viewer, flash page/erase map, journal timeline, storage profiler, PLP energy/ESR analyser, thermal scanner, net tracer, X-ray BGA, via solder repair, safety latch ledger.

Mood: genuine enterprise storage R&D lab, physically realistic dark navy PCB, precise gold/silver contacts, panel highlights using translucent restrained cyan/orange with clear warning contrast; no cartoon effects. Distinct action sounds for successful contact, failed channel and safety inhibit; screen-reader/keyboard-friendly cues where possible. Only show measured values as LIVE after a valid solver query. Illustrative plots must say DEMO. Do not spawn new separate physical oscilloscopes or another room.

A tool click produces a typed request `(toolId, targetRef, padId, worldHitPoint, playerId, safeState)`, validates target/tool compatibility, updates authorized physical/electrical state, then emits localized VFX and refreshes actual measurements. No uncontrolled thermal/ESD repair. Support mouse/touch and let world-space UI intercept pointer hits.

### 13. Save/load, evidence and performance

Persist immutable BOM identities, each installed part and footprint, graph revisions, route/via states, wear and bad-block map, PLP state, NAND data pages (bounded sample), journal/metadata state, deterministic seeds, repaired fault history, supervisor latch and objective award history. Version and hash the saved schematic/source manifest. On load rebuild authoritative graph first and invalidate impossible/stale evidence; never load pre-checked 6/6 flags without proof.

Prefer fixed-size arrays/object pooling for pages/channels; cap flash test work per frame. Avoid per-frame global FindObjects or arbitrary Unity Update. Numerical test/thermal calculations and electrical states must be deterministic for repeatable acceptance. Target mobile and PC; instrument panel and board rendering should profile correctly.

### 14. Unity editor authoring / import

Required menu commands supplied with this package:
```
Tools/Project Spark/Automatic Level Generator/116/1. Build FULL Level_116_Layout
Tools/Project Spark/Automatic Level Generator/116/2. Place assembled VISUAL preview on SELECTED tabletop
Tools/Project Spark/Automatic Level Generator/116/3. Validate Sources and Report Blockers
Tools/Project Spark/Automatic Level Generator/116/4. Rebuild URP PBR Materials
Tools/Project Spark/Levels/Create Level 116
```

Output under `Assets/ProjectSpark/AutoGeneratedLevels/Level116/`. It must be idempotent, preserve earlier levels, be compile-safe in Unity 6.3 with URP, and refuse to generate when an imported physical PCB has missing renderers, ambiguous source roots, incorrect 0.440 × 0.310 m bounds or mismatched model manifest. Its source manifest uses actual `obj` column 5, with 256 records, not placeholder `Models_OBJ/NOT_INCLUDED`. Be careful with CSV descriptions containing commas; parse quoted fields correctly. Do not assume import completion/Unity compilation happened just because an offline ZIP test succeeded. A tabletop visual preview is enlarged independently to fit 15.79×8.54 units, never electrically operative.

### 15. Schematic and production blocker handoff

Publish an actual unresolved-pin report including U31/U30/W289, NAND channel 3 CE via, F2 capacitor ratings, F3 NAND metadata page, NVMe connector, verified 24-layer stack-up and complete source/destination pad/network assignments. Never invent missing values. Add authoritative adapters only after inspecting real project services. Return exact file paths, current compilation logs, missing imports, actual Play Mode logs, all test statuses and a successful 6/6 screenshot only if actually observed.

### 16. Definition of done

A genuinely complete playable Level 116 requires a clean Unity import, real gameplay placement/soldering, approved 288/100 net topology, initialized storage and PLP, fresh ECC/crash consistency, three repaired faults, physically interrupted W289 followed by full requalification, 6/6 authoritative LevelGamePlayManager, save/load correctness, and no regressions to Levels 1–115.

**Until those tests are independently observed, deliver the source/model kit as a visual-authoring foundation and label actual electrical gameplay NOT_CERTIFIABLE.**

---

## ANNEX A — Actual source manifests and creative art inventory

The full immutable 256-line ModelManifest.csv, 208-line ComponentBOM.csv, ComponentPlacements.csv, 288 unresolved copper records, 100 unresolved via records and 30 material families live inside `Data/` in the ZIP. Only `ModelManifest.csv` source records are authoritative for art filenames. Source status flags remain FALSE pending approval.

## ANNEX B — Required 100 acceptance tests (NOT RUN)

### Assets and import

- [ ] T001. All 256 modular OBJ mesh imports exist and have nonempty geometry. **Status: NOT_RUN**.
- [ ] T002. All 256 matching GLB meshes exist as alternate references. **Status: NOT_RUN**.
- [ ] T003. The separate 257_L116 assembled reference is excluded from the gameplay root. **Status: NOT_RUN**.
- [ ] T004. Physical board OBJ bounds are 0.440 x 0.310 metres. **Status: NOT_RUN**.
- [ ] T005. The imported model renders correctly in the active URP project. **Status: NOT_RUN**.
- [ ] T006. 30 PBR source families and packed masks have valid import settings. **Status: NOT_RUN**.
- [ ] T007. All 208 BOM identifiers are unique and join one-to-one with manifest and placements. **Status: NOT_RUN**.
- [ ] T008. 48 noninstallable art assets never count toward installation. **Status: NOT_RUN**.
- [ ] T009. 24 inspection layer meshes are initially disabled and nonconductive. **Status: NOT_RUN**.
- [ ] T010. First/second builder runs preserve exact scene and prefab identities. **Status: NOT_RUN**.

### Assembly and safety

- [ ] T011. Initial installed count is exactly 0 of 208. **Status: NOT_RUN**.
- [ ] T012. Incorrect controller footprint or rotation cannot certify installation. **Status: NOT_RUN**.
- [ ] T013. Wrong NAND die/package cannot certify its socket. **Status: NOT_RUN**.
- [ ] T014. Wrong capacitor capacity or voltage rating cannot qualify PLP. **Status: NOT_RUN**.
- [ ] T015. Installing the same component twice does not increment the count. **Status: NOT_RUN**.
- [ ] T016. BGA incomplete solder contact blocks independent validation. **Status: NOT_RUN**.
- [ ] T017. Wrong component reference or inventory identity is rejected. **Status: NOT_RUN**.
- [ ] T018. The assembled reference artwork cannot produce installed flags. **Status: NOT_RUN**.
- [ ] T019. Safe-state gating blocks hot replacement under energized storage paths. **Status: NOT_RUN**.
- [ ] T020. Removal or replacement invalidates dependent electrical and test evidence. **Status: NOT_RUN**.

### Topology and PCB

- [ ] T021. W001-W288 require approved source/destination pad and net identifiers. **Status: NOT_RUN**.
- [ ] T022. Every required copper task begins incomplete. **Status: NOT_RUN**.
- [ ] T023. UNASSIGNED source/destination or net data cannot be certified. **Status: NOT_RUN**.
- [ ] T024. Wrong-net route creates an observable design-rule failure. **Status: NOT_RUN**.
- [ ] T025. Cross-layer route without an approved via span is blocked. **Status: NOT_RUN**.
- [ ] T026. All 100 vias require approved net/layer/span and physical geometry. **Status: NOT_RUN**.
- [ ] T027. Factory copper cannot bypass a missing player-required route. **Status: NOT_RUN**.
- [ ] T028. Signal and high-current power nets do not silently merge. **Status: NOT_RUN**.
- [ ] T029. Real solver graph mutations—not visual strokes—update completion. **Status: NOT_RUN**.
- [ ] T030. W289 is not counted as one of W001-W288 selected route tasks. **Status: NOT_RUN**.

### NAND/PCIe and startup

- [ ] T031. Missing 3.3 V supply blocks the protected controller startup. **Status: NOT_RUN**.
- [ ] T032. NAND supply and controller rail qualification are independent. **Status: NOT_RUN**.
- [ ] T033. Missing return continuity prevents NAND channel qualification. **Status: NOT_RUN**.
- [ ] T034. Incorrect clock/reset state blocks command operations. **Status: NOT_RUN**.
- [ ] T035. Each of eight NAND channels has separate ready/error state. **Status: NOT_RUN**.
- [ ] T036. Unavailable NAND package produces attributable channel failure. **Status: NOT_RUN**.
- [ ] T037. Bad-block handling never silently reports a passing page. **Status: NOT_RUN**.
- [ ] T038. PCIe device discovery cannot imply memory-page integrity. **Status: NOT_RUN**.
- [ ] T039. Power loss cancels or concludes in-flight writes according to durable state. **Status: NOT_RUN**.
- [ ] T040. Firmware/flash-controller revision invalidates stale test evidence. **Status: NOT_RUN**.

### Power-loss protection

- [ ] T041. PLP capacitor voltage and capacitance affect stored energy. **Status: NOT_RUN**.
- [ ] T042. Effective hold-up budget includes load, ESR, efficiency and safety margin. **Status: NOT_RUN**.
- [ ] T043. High ESR reduces usable flush time under a specified load. **Status: NOT_RUN**.
- [ ] T044. A missing charged capacitor prevents durable journal shutdown. **Status: NOT_RUN**.
- [ ] T045. No completed flush is reported when power vanishes too early. **Status: NOT_RUN**.
- [ ] T046. PLP measurements come from the current simulated state. **Status: NOT_RUN**.
- [ ] T047. Inappropriate capacitor polarity/rating blocks safe commissioning. **Status: NOT_RUN**.
- [ ] T048. A brownout changes write authorization and produces a log. **Status: NOT_RUN**.
- [ ] T049. Post-PLP repair requires a fresh supervised power-cut test. **Status: NOT_RUN**.
- [ ] T050. Teaching demo PLP calculations cannot certify Objective 3 or 4. **Status: NOT_RUN**.

### ECC/journal and data

- [ ] T051. Deterministic stuck-bit injection creates attributable read errors. **Status: NOT_RUN**.
- [ ] T052. Errors within approved correction capability return corrected status. **Status: NOT_RUN**.
- [ ] T053. Errors above correction capability return uncorrectable status. **Status: NOT_RUN**.
- [ ] T054. ECC correction is not falsely counted as zero raw errors. **Status: NOT_RUN**.
- [ ] T055. Metadata journal commit is atomic across modeled crash points. **Status: NOT_RUN**.
- [ ] T056. Crash before durable commit replays last valid checkpoint. **Status: NOT_RUN**.
- [ ] T057. Crash after durable commit replays newly committed checkpoint. **Status: NOT_RUN**.
- [ ] T058. CRC/hash mismatch is logged with page/block/channel context. **Status: NOT_RUN**.
- [ ] T059. Sequenced data-pattern tests record bytes tested and observed failures. **Status: NOT_RUN**.
- [ ] T060. Crash-consistency evidence expires after storage graph, PLP or firmware change. **Status: NOT_RUN**.

### Fault F1 and repair

- [ ] T061. F1 targets only a schematic-approved NAND channel 3 chip-enable via. **Status: NOT_RUN**.
- [ ] T062. Opening the selected via changes real conductive graph connectivity. **Status: NOT_RUN**.
- [ ] T063. Timed-out NAND commands identify channel 3, not generic failure. **Status: NOT_RUN**.
- [ ] T064. Net tracer and layer view show only verified source locations. **Status: NOT_RUN**.
- [ ] T065. Repair with an incompatible tool is denied. **Status: NOT_RUN**.
- [ ] T066. An energized solder or via repair is denied by safe-state policy. **Status: NOT_RUN**.
- [ ] T067. Visual spark/glow alone cannot repair the circuit. **Status: NOT_RUN**.
- [ ] T068. Qualified physical via rework restores measured continuity. **Status: NOT_RUN**.
- [ ] T069. Channel 3 requires fresh channel initialization and page tests. **Status: NOT_RUN**.
- [ ] T070. F1 history is maintained distinctly from F2 and F3. **Status: NOT_RUN**.

### Fault F2 and F3

- [ ] T071. F2 identifies an approved power-loss capacitor or contact. **Status: NOT_RUN**.
- [ ] T072. F2 high ESR degrades power-cut hold-up energy with numerical evidence. **Status: NOT_RUN**.
- [ ] T073. A lower-capacity substitute cannot certify the PLP repair. **Status: NOT_RUN**.
- [ ] T074. F2 replacement requires independent capacitor/rail retesting. **Status: NOT_RUN**.
- [ ] T075. F3 corrupted metadata produces ECC or journal replay failure. **Status: NOT_RUN**.
- [ ] T076. F3 identifies a schematic-approved NAND path/page location. **Status: NOT_RUN**.
- [ ] T077. A generic UI action cannot silently clear journal corruption. **Status: NOT_RUN**.
- [ ] T078. F3 restoration requires a verified good checkpoint and re-scrub. **Status: NOT_RUN**.
- [ ] T079. All three fault repairs must have fresh evidence after mutation. **Status: NOT_RUN**.
- [ ] T080. Objective 5 is blocked if any target or repair is unverified. **Status: NOT_RUN**.

### W289 independent safety

- [ ] T081. W289 starts physically OPEN and is not a storage supply rail. **Status: NOT_RUN**.
- [ ] T082. U31/U30 package pins remain blocked until approved mapping. **Status: NOT_RUN**.
- [ ] T083. Disconnecting an operating approved W289 opens the real graph. **Status: NOT_RUN**.
- [ ] T084. Independent supervisor inhibits protected writes on missing feedback. **Status: NOT_RUN**.
- [ ] T085. Unrelated diagnostic power remains available after W289 opens. **Status: NOT_RUN**.
- [ ] T086. A constant HIGH cannot mimic a fresh monotonic heartbeat. **Status: NOT_RUN**.
- [ ] T087. W289 reconnection alone cannot clear the latched safety condition. **Status: NOT_RUN**.
- [ ] T088. Fresh NAND/ECC, PLP and journal replay tests are required. **Status: NOT_RUN**.
- [ ] T089. An explicit supervised reset is required for safety requalification. **Status: NOT_RUN**.
- [ ] T090. Objective 6 requires new independent proof and no stale pass flags. **Status: NOT_RUN**.

### UI, editor and regression

- [ ] T091. Touch/mouse world-space UI blocks raycasts to hidden PCB objects. **Status: NOT_RUN**.
- [ ] T092. Contact VFX appears at actual accepted hit location. **Status: NOT_RUN**.
- [ ] T093. Instrument telemetry is bound to measured solver state when labeled live. **Status: NOT_RUN**.
- [ ] T094. False telemetry is clearly marked DEMO and never grants objectives. **Status: NOT_RUN**.
- [ ] T095. Unity scene generator runs twice without duplicated board roots. **Status: NOT_RUN**.
- [ ] T096. Installation does not overwrite assets from Levels 1-115. **Status: NOT_RUN**.
- [ ] T097. Save/load reconstructs graph before restoring objective certification. **Status: NOT_RUN**.
- [ ] T098. Player objective tracker starts at 0/6 and cannot be spoofed from UI. **Status: NOT_RUN**.
- [ ] T099. Unity EditMode and PlayMode tests report reproducible evidence. **Status: NOT_RUN**.
- [ ] T100. Final verified gameplay 6/6 occurs only after every authoritative test passes. **Status: NOT_RUN**.

## ANNEX C — Required developer final handoff format

Provide a list of actual files touched, accurate source manifest reconciliation, imported Unity OBJ material/bounds audit, package/scene build commands, compiler results, renderer screenshot if available, actual EditMode & PlayMode test log, 288-route/100-via blockers, approved U31/U30 pin-source revision, data-integrity observations, and objective 0/6 -> 6/6 proof ONLY if measured. Never substitute a procedural rendering for electrical authority.
