# PROJECT SPARK — LEVEL 107: PHOENIX PROTOCOL
## MASTER UNITY IMPLEMENTATION PROMPT — PFX-15 Firmware Recovery Motherboard

> **Paste this specification into the Unity implementation agent along with the actual Project Spark codebase and the Level 107 asset ZIP.** Implement, integrate, compile, and verify the complete level. Do not substitute a static demo or certify fabricated electrical data. The package contains 3D art and planning manifests—not an electrical schematic or finished gameplay.

### 0. Non-negotiable implementation contract

You are the senior Unity gameplay/electrical engineer for **Project Spark**. Build **Level 107 — Phoenix Protocol**, a complete motherboard firmware-recovery experience, preserving existing Levels 1–106, player progression, existing shop, item shelf, throw/recycle restrictions, level managers, wire/copper authoring, and the user's established AAA World-Space Virtual PCB Repair UI.

**Expected outcomes:** one-click builder `Tools/Project Spark/Levels/Create Level 107`, a playable empty board, 112 placeable component tasks, 148 player copper tasks, 46 via tasks, three replayable authenticated repair scenarios, independent W149 safety recovery, believable dual-bank SPI firmware simulation, six authoritative objectives, save/reload safety, edit-mode/play-mode tests, and a clear validation report. Do not assert the level is complete until these outcomes have been tested.

**Environment:** Unity, C#, Project Spark's existing runtime components and editor patterns. Discover actual code APIs before writing adapters: in particular the authoritative `LevelGamePlayManager` interface, electrical connectivity solver, component shelf/placement registration, existing virtual inspection UI, circuit condition effects, HUD objective events, save system and shader pipeline. **Do not invent engine API method names as if they already exist.** Where an integration point is unavailable, create a narrow adapter and clearly report the unresolved dependency without pretending success.

### 1. Exact source kit and authority hierarchy

Asset ZIP: `ProjectSpark_Level107_PhoenixProtocol_BoardOnly_Realistic_Complete.zip`

Expected extraction location: `Assets/ProjectSpark/Level107/` containing `Models_FBX/`, `Models_GLB/`, `Textures/`, `Data/`, `Unity/Editor/`, and `Documentation/`.

Source-verified counts:

| Item | Source quantity / state |
|---|---|
| Modular visual assets | 144 binary FBX + 144 matching GLB |
| Assembled showcase | 1 FBX + 1 GLB, **art only** |
| Installable electronic components | 112, initially uninstalled |
| Structural / copper / repair references | 32, non-installable |
| Copper routing task records | 148, all `UNASSIGNED` and `NOT_CERTIFIABLE` |
| Via task records | 46, all electrical nets / spans unassigned |
| Recoverable fault definitions | 3 |
| Objectives | 6 |
| PCB | 10 copper layers, PFX-15 target 290 × 210 mm |
| Existing inherited source geometry | 285 × 205 mm board art from Level 106 |
| W149 | Logical freshness-monitored trust path, physical terminals unassigned |

**Authoritative hierarchy:** approved pin-level schematic and topology > real components/placements > measurements/firmware state > objectives > UI animations. Art meshes, green labels, route counts, holograms, prepopulated showcase, visual contacts, arbitrary collisions or fake wire segments are never evidence of electrical correctness.

**Art-source caveat:** meshes are realistic-style procedural starter assets derived in part from Level 106; they are not final hand-authored AAA assets or certified footprints. Verify import, axes, dimensions, pivots, PBR assignments, mesh normals, materials, collider placement and light/render consistency. The supplied assembled reference GLB and FBX may not precisely match. The art builder scales the inherited bare board toward 290 × 210 mm; do **not** scale electrical pad coordinates blindly. Reconcile dimensions in a single board-local coordinate frame and measure mounting/connector footprints in Unity.

### 2. One-click builder and project hygiene

Implement a non-destructive Editor pipeline with menu:

`Tools/Project Spark/Levels/Create Level 107`

1. Validate source folders/files, confirm all 144 modular FBX names and manifest consistency; import GLBs only as optional preview support.
2. Validate no duplicate component refs, no missing assets, no floating nonfinite coordinates, no duplicate objective IDs, no wrong layer identifiers.
3. Run electrical schematic reconciliation; if unresolved, build only an **art-only playable-preview root** with uncompleted objectives and prominent `SCHEMATIC_REQUIRED` diagnostics; **never** silently auto-certify.
4. Build distinct `L107_PFX15_EMPTY_GAMEPLAY_ROOT`, equipment-free `L107_VirtualInstrumentCanvas`, and an optional *disabled by default* `L107_PFX15_COMPLETED_REFERENCE_ART_ONLY` under a showcase/editor-only parent.
5. Instantiate board-related structural assets and separate installable component ghost/footprint targets; leave all 112 actual installable parts uninstalled.
6. Reconcile and bake 148 copper tasks and 46 via tasks only against an approved board netlist. All start physically/electrically incomplete.
7. Wire distinct firmware, boot, safety, repair, measurements, objective and UI services through serialized references/DI matching existing project conventions.
8. Register level id=107, display title `PHOENIX PROTOCOL`, mission, counts and six objectives with the real `LevelGamePlayManager` contract.
9. Generate local prefabs, ScriptableObject definitions, materials and a validation report without modifying earlier levels.
10. Support idempotent build/rebuild with clean generated folders and no destructive changes to pre-existing scene objects.

**Existing art menus** in the shipped Editor helpers:
- `Tools/Project Spark/Levels/107/1. Build EMPTY PFX-15 Board`
- `Tools/Project Spark/Levels/107/2. Select Level 107 Art FBXs`
- `Tools/Project Spark/Levels/107/3. Create PBR Starter Materials`

Do not confuse these art utilities with a complete gameplay builder.

### 3. Model and scene composition

- Board-only: do not generate furniture, room, table, soldering iron, physical multimeter, scope or rack props.
- The central CPU/SoC, VRM, two DDR4 devices, connectors, U5 recovery controller, U6 primary SPI NOR, U7 protected recovery SPI NOR, standby/controller parts and removable W149 must be separately addressable.
- All 112 component slots must have placement authority, footprint, orientation/polarity/pin-one tests and per-contact solder state. `component installed` requires a real authoritative placement registration and valid contacts.
- Use measured board bounds, PBR materials and scale-neutral UI hit targets. Prevent click-through to underlying PCB when overlapping tool/pad targets.
- Treat layer and trace art as visualization only; only the approved graph authoritatively conducts current/data.
- Separate complete reference/showcase from player-state hierarchy and save state.

### 4. Proposed simulated electrical architecture

Create a **bounded educational**, deterministic simulation—not a claim of authentic consumer BIOS silicon or a transistor-accurate entire computer.

Functional domains:
- Protected 12 V input and standby 3.3 V to recovery/controller supervision.
- CPU/SoC/VRM/memory rail readiness from prior Project Spark systems.
- Independent U5-style recovery controller with its own power/reset domain and control of flash ownership.
- U6 primary 32 MB SPI NOR virtual memory with explicit erase/page program/busy/protection/read behavior.
- U7 protected 32 MB SPI NOR recovery memory with default read-only/update-denied normal gameplay policy.
- SPI bus and arbiter: appropriate chip selects, SCLK, IO pin group, voltage, bus master ownership; do not model two independent drivers actively driving the same IO lines.
- Independent platform safety supervisor and W149 heartbeat/trust path.

**SPI flash state:** implement status/config registers, sector erase, page program, access conflict, write enable, write-protect, busy/ready, repeated read and power-interruption behavior at an educational abstraction level. Corrupted data must be distinct from an open chip-select, and a recovery request must not be accepted when the input is electrically invalid.

**Boot stages:** `POWER_OFF → STANDBY → RESET_QUALIFICATION → FLASH_DISCOVERY → PRIMARY_IMAGE_VERIFY → SEC → PEI → DXE → BDS → BOOT_HANDOFF → APPLICATION_READY`. Explicit failure states: `NO_PRIMARY_FLASH_RESPONSE`, `INVALID_IMAGE`, `RECOVERY_REQUEST_INVALID`, `RECOVERY_IMAGE_UNTRUSTED`, `RECOVERY_IN_PROGRESS`, `INTERRUPTED_WRITE`, `PROTECTED_OPERATION_INHIBITED`. POST state must be derived from actual preconditions, not selected just to match objective text.

### 5. Dual-bank firmware recovery and trust

- Primary and backup contents are separately modeled by version, image metadata, integrity digest, simulated signature, trust root/board compatibility and monotonic anti-rollback policy (if this mechanic is enabled).
- Use original fictional Project Spark firmware images, never copyrighted commercial BIOS dumps or real production signing keys.
- A digest match establishes integrity, not authorship. Signature checking and trust-policy evaluation are separate operations.
- U7 protection is enforced in service logic and ownership graph; normal update UI never overwrites it.
- A recovery transaction has explicit stages: `request → authorize → verify protected source → arbitrate flash bus → erase selected primary sectors → program pages → read back → verify digest/signature → commit boot bank → release flash ownership → restart and revalidate`.
- Interrupted-update testing intentionally damages the primary region but leaves a genuine recoverable U7 state; use deterministic fault injection with a seeded simulation and repeatable expected outcomes.
- Recovery progress is a visualization of real backing-state changes, never a success trigger. A 100% progress bar alone cannot award an objective.
- Distinguish firmware image corruption, incorrect NVRAM/boot entries, absent SPI hardware access and recovery button faults; the player should use appropriate repairs for each.

### 6. Firmware programmer and World-Space UI

Reuse the current Project Spark world-space virtual instruments as UI-based interactions:
`Inspect`, `Assemble`, `Route`, `Layer Viewer`, `POST`, `Recovery`, `Calibrate` plus virtual multimeter, oscilloscope, SPI analyzer, firmware programmer, image-verification console, recovery console, NVRAM editor, microscope, solder/desolder, net tracer and component replacement.

The AAA UI should have:
- Left: objectives 0/6 and F1/F2/F3 status with compact actionable descriptions.
- Top: component/route/via counts (0/112, 0/148, 0/46 initially), W149 state and firmware bank selection.
- Center: interactive PFX-15 board and selectively enlarged U5/U6/U7/W149 area, clear selection highlight, scene/pointer input for mouse/touch, virtual tool active state.
- Right: U6/U7 response, firmware version/digest/trust, POST trace, SPI CS/SCLK/IO waveforms, recovery phase and platform trust.
- Controls: inspect/read, verify, authorize recovery, erase/program/verify, reset, simulate interrupted update, inspect/reset NVRAM; disable controls when prerequisites fail.
- Localization-ready text and gamepad/touch-accessible targets when practical.

UI must display measured/simulated results from the same underlying state used for objective and safety checks. No fake constant waveforms, no fake POST progression, no fake repaired flags.

### 7. Three recoverable faults — actual mechanics

**F1 — U6 Primary SPI Chip-Select Contact / `OPEN_SPI_CS`.** Force an electrical open in the verified CS net at U6 contact. Reading flash returns missing/invalid transaction response *before* image verification. Virtual SPI analyzer and microscope localize failure. A valid solder-contact rework restores that precise net and then fresh reads prove it. Do not allow generic `Verify Firmware` to pass until CS contact is repaired.

**F2 — Corrupted Primary Firmware / `FIRMWARE_CORRUPTION`.** U6 electrical SPI reads succeed, but primary image fails integrity/authentication. Recovery requires protected U7 authenticity, authorized flash arbiter ownership, actual simulated flash rewrite, readback validation and reboot. The initial narrative may introduce the corruption before Objective 3; Objective 5 must separately track its repair proof without resetting earlier certified historical objectives. Do not corrupt U7 as side effect.

**F3 — Recovery Request Pull-up / `RECOVERY_INPUT`.** Electrical value/transition at the recovery-request input is unreliable. Virtual multimeter and logic analyzer identify the defective resistor/input network. Qualified replacement restores logic transitions; fresh button/test results prove recovery request activation/deactivation. Avoid fixing hardware merely by changing an enum or UI label.

**Fault scheduling:** inject separately so earlier faults do not mask later diagnostic evidence. New failure invalidates dependent live qualification and forces new measurements, but do not erase already credited historical objectives. Support controlled replay for testing.

### 8. W149 critical safety recovery — strict

Logical path from source JSON:
`FirmwareTrustMonitor.TRUSTED_FIRMWARE_READY_OUT → W149 → IndependentPlatformSafetySupervisor.TRUSTED_FIRMWARE_READY_IN`

The source explicitly says component IDs are `SCHEMATIC_REQUIRED` and pins `UNASSIGNED`. Do not invent U14/U15 or any other physical pins; **resolve physical component refs and both endpoints through an approved schematic first.**

W149 must be a physically detachable **signal** jumper. It does **not** carry the main CPU power, SPI data or 12 V supply. The health signal must be changing/freshness validated; permanent HIGH is not valid.

Required sequence:
1. Begin from a fully qualified healthy boot with W149 connected and protected workloads authorized.
2. On actual W149 removal, open graph edge, invalidate trust, latch safety fault and inhibit protected workloads without instantly zeroing unrelated power rails.
3. Demonstrate the actual independent inhibition in simulator/UI.
4. Physically reconnect contacts and prove graph continuity.
5. Reauthenticate primary firmware using fresh data and complete a new POST/boot qualification.
6. Obtain fresh, valid changing health evidence and explicitly reset safety latch.
7. Independently verify no stale readings, no active faults, correct W149, and a valid system; only then certify Objective 6.

Reconnection alone must not grant completion. Unverified schematic/pins means W149 cannot be certified.

### 9. Six authoritative objectives

1. **Assembly — 112/112:** complete installable components, correct footprint/orientation and contact validation.
2. **Electrical Network — 148/148 + 46/46 + W149:** only graph-verified tasks; no placeholder/presentation auto-credit.
3. **Firmware Recovery:** authenticate protected U7 and restore primary U6 through real authorized recovery and successful early initialization.
4. **POST & Resilience:** valid boot/NVRAM, complete POST, deliberate interrupted update and successful authorized recovery followed by full cold boot.
5. **Three Repairs:** independent F1/F2/F3 repair proofs and a fresh complete qualification after all repairs.
6. **W149 Safety Recovery:** disconnect, inhibit, reconnect, reverify firmware/boot, explicit safety reset and independent final certification.

Integrate only through the **actual** LevelGamePlayManager objective/public API discovered in the Project Spark codebase. Objective events must be unique, authoritative and idempotent. A debug bypass may exist only in editor testing, with visible labeling and never in production.

### 10. Save/load, scene lifecycle and evidence provenance

Persist component placements, solder contacts, completed verified graph edges, via installs, W149 physical state, flash bank bytes/metadata and protection, NVRAM, recovery transactions, fault state, pending reset, objective history and measurement epochs. Persist deterministic simulation seed. On load, validate version and netlist compatibility; discard or quarantine obsolete proofs rather than trusting old UI strings. Avoid race conditions, duplicate subscriptions, static cross-level state, unsupported background operations and stale object references.

For each measurement/qualification store `device`, `probe points`, `sim tick`, `topology revision`, `firmware revision`, `fault revision`, `calibration revision`, `value/status`, `authority`. Faults or rewires invalidate affected live proofs. On restart, all stale conclusions must be checked against the current revision.

### 11. Materials and art finishing

Use the 74 supplied PBR reference texture files and Unity starter materials as a base only. Validate URP/HDRP/built-in pipeline compatibility; do not import or redistribute font files. Author/adjust realistic soldermask, FR-4, epoxy, solder, copper, flash chip metal leads, connectors and gold. Improve normals, contact highlights, roughness, silkscreen and service-area labeling without requiring new scenery. Avoid false AAA completion claims until real visual review and Unity Play Mode validation.

**Important discrepancy:** the bare-board source geometry is 285 × 205 mm; Level 107 target is 290 × 210 mm. Transform and source manifest coordinates must be reconciled so component footprint constraints, object colliders, vias, visual traces and copper layout all agree. The delivered reference FBX is inherited, whereas the GLB reference adds dual-bank markers and a board-scale adjustment; do not treat either as a fabrication truth.

### 12. Suggested clean component decomposition (proposed names)

Proposed class names only—reconcile with the existing project namespace and naming conventions:

- `SparkLevel107Coordinator`
- `SparkL107BoardAssemblyAdapter`
- `SparkL107ApprovedNetlistAsset`
- `SparkPCBSPINORDevice`
- `SparkPCBDualFirmwareBanks`
- `SparkPCBRecoveryBusArbiter`
- `SparkPCBTrustedImageVerifier`
- `SparkPCBFirmwareRecoveryController`
- `SparkPCBVirtualBootPOST`
- `SparkPCBVirtualNVRAM`
- `SparkLevel107FaultController`
- `SparkLevel107W149Supervisor`
- `SparkLevel107ObjectiveAuthority`
- `SparkLevel107VirtualToolUIBinder`
- `SparkLevel107BuildValidator`

Keep electrical physics, firmware, objective authority, presentation and editor construction separated. Expose immutable measurement snapshots/events to UI and objective tracking.

### 13. Validation gates and acceptance test matrix

Write edit-mode and Play Mode tests. Cover at minimum:

**Asset/scene (12):** all 144 models exist, 112 component refs unique, 32 supporting refs, all 10 layers distinguishable, empty starts empty, reference disabled, transforms finite, scale reconciled, shaders resolve, no missing colliders, no scene edits outside Level107, rebuilt output idempotent.

**Topology (16):** 148 route IDs unique, 46 via IDs unique, no unassigned route certification, no unassigned via certification, layer span enforcement, wrong-net shorts rejected, invalid floating pad rejected, contact orientation checked, jumper edge actual, ground/power isolation, SPI CS proper, recovery bank correct, current paths consistent, fault-cut graph correctly, repair reconnect correctly, old proofs invalidated on topology changes.

**SPI/firmware (22):** U6/U7 independent bytes and status, wrong chip select, simultaneous bus owner denial, flash busy/read handling, read/erase/program behavior, write protection, bad hash, bad signature, incompatible board target, disallowed firmware version, protected U7 immunity, auth required before flash, mid-erase interruption, mid-program interruption, readback mismatch, post-interruption recovery from U7, restart, NVRAM restoration, cold boot, warm reboot, no fake progress completion.

**Faults/repair (14):** F1 physical CS open and repair, F2 image corruption with successful SPI, F3 recovery request pull-up state, tools only valid at target, wrong tool rejected, unauthorized rework rejected, VFX decoupled from electrical status, fresh probes required, incident revisions, repeatable injection, repair order, faults coexist safely, no false repair, save/load fault state, qualified final pass.

**Safety/objectives (18):** W149 open is graph break, changing heartbeat validation, permanent HIGH rejected, stale heartbeat rejected, inhibited protected workloads, diagnostic access retained, reconnect insufficient, fresh auth required, fresh boot required, explicit reset required, six objective transitions, no double awards, five historical results retained, stale evidence invalidated, scene reload, no cross-level leakage, real LevelGamePlayManager 6/6, unverified schematic blocks 6/6.

**UI/accessibility/performance (18):** virtual tool selection, touch and mouse, no click-through, labels synchronized, PBR materials import, realistic component identity, POST first blocker, SPI waveform truth, recover buttons gated, progress persistence, reset options safe, performance under all widgets, no frame alloc spikes from UI every frame, pool local effects, no fatal console errors, meaningful warnings, no overlapping labels, functional hover/tap targets, completion animation only after objective authority.

**Total proposed tests: 100.** They are acceptance *requirements*, not a claim tests already exist or pass.

### 14. Implementation order and delivery

1. Inspect real Project Spark codebase and integration contracts, report confirmed API names.
2. Fix/validate Unity source imports, build editor-only art preview, test scale.
3. Create approved electrical schematic/netlist for Level 107, pin references, 148 route tasks and 46 via spans; reconcile W149 exact terminals and material BOM. Report gaps instead of guessing.
4. Build actual empty gameplay scene, component placement and routing authority.
5. Implement flash model, recovery controller, security and virtual POST states.
6. Implement fault injection, valid repair and safety recovery.
7. Bind all world-space UI instruments, shader/VFX and input.
8. Implement six objective evaluators and persistence.
9. Run tests and validate first clean boot, each individual fault, interrupted-update resilience, W149 disconnect/reconnect and 6/6.
10. Produce source files/prefabs/data assets, editor menu, test report, Unity console log, build instructions and a list of any unresolved schema/art issues. Do not claim finished until reviewed.

### 15. Source-backed caveats / explicitly NOT in provided ZIP

The source package **does not** provide a verified schematic, pad-to-pin netlist, functional SPI/NVRAM firmware state machine, real firmware signatures, true electrical repair system, automatic LevelGamePlayManager integration or Play Mode verification. The 148/46 planning records explicitly say NOT_CERTIFIABLE and should remain that way until reconciled. The source U6/U7 3D files are packaged as generic inherited IC geometry, not verified manufacturer-specific SOIC-8 footprints. Do not silently claim otherwise.

---

## SOURCE ANNEXES — EXACT SUPPLIED RECORDS

These annexes are verbatim text extracted from the Level 107 source ZIP, preserving planned identifiers and omissions. They are source inputs, **not approvals** for electrical connectivity.

### Annex A. Models: 144 visual assets

Source path: `Data/L107_Models_144.csv`

```csv
asset_name,asset_type,source_package_asset,board,x_mm,y_mm,z_mm,individually_installable,art_only,electrical_authority,notes
001_L107_U1,CPU,001_L106_U1,PFX-15,0.0,0.0,0,True,True,UNVERIFIED_PIN_LEVEL,
002_L107_U2,DDR,002_L106_U2,PFX-15,87.0,50.0,0,True,True,UNVERIFIED_PIN_LEVEL,
003_L107_U3,DDR,003_L106_U3,PFX-15,87.0,23.0,0,True,True,UNVERIFIED_PIN_LEVEL,
004_L107_IC1,IC,004_L106_IC1,PFX-15,-51.0,52.0,0,True,True,UNVERIFIED_PIN_LEVEL,
005_L107_U5_RecoveryController,RECOVERY_CONTROLLER,005_L106_IC2,PFX-15,-79.0,-43.0,0,True,True,UNVERIFIED_PIN_LEVEL,
006_L107_U6_PrimarySPIFlash,PRIMARY_SPI_NOR,006_L106_IC3,PFX-15,-22.0,-54.0,0,True,True,UNVERIFIED_PIN_LEVEL,
007_L107_U7_RecoverySPIFlash,PROTECTED_RECOVERY_SPI_NOR,007_L106_IC4,PFX-15,7.0,-54.0,0,True,True,UNVERIFIED_PIN_LEVEL,
008_L107_IC5,IC,008_L106_IC5,PFX-15,106.0,65.0,0,True,True,UNVERIFIED_PIN_LEVEL,
009_L107_U7,STORAGE,009_L106_U7,PFX-15,33.0,-43.0,0,True,True,UNVERIFIED_PIN_LEVEL,
010_L107_Y1,OSC,010_L106_Y1,PFX-15,-33.0,75.0,0,True,True,UNVERIFIED_PIN_LEVEL,
011_L107_U6,FLASH,011_L106_U6,PFX-15,-50.0,-55.0,0,True,True,UNVERIFIED_PIN_LEVEL,
012_L107_U5,EC,012_L106_U5,PFX-15,-99.0,-38.0,0,True,True,UNVERIFIED_PIN_LEVEL,
013_L107_U8,IO,013_L106_U8,PFX-15,68.0,-26.0,0,True,True,UNVERIFIED_PIN_LEVEL,
014_L107_U9,IO,014_L106_U9,PFX-15,115.0,-15.0,0,True,True,UNVERIFIED_PIN_LEVEL,
015_L107_REG1,REG,015_L106_REG1,PFX-15,-76.0,-17.0,0,True,True,UNVERIFIED_PIN_LEVEL,
016_L107_REG2,REG,016_L106_REG2,PFX-15,68.0,-71.0,0,True,True,UNVERIFIED_PIN_LEVEL,
017_L107_U15,SAFETY,017_L106_U15,PFX-15,75.0,-35.0,0,True,True,UNVERIFIED_PIN_LEVEL,
018_L107_TEMP1,TEMP,018_L106_TEMP1,PFX-15,-25.0,71.0,0,True,True,UNVERIFIED_PIN_LEVEL,
019_L107_RES1,RES,019_L106_RES1,PFX-15,116.84,-32.698,0,True,True,UNVERIFIED_PIN_LEVEL,
020_L107_RES2,RES,020_L106_RES2,PFX-15,48.49,-55.554,0,True,True,UNVERIFIED_PIN_LEVEL,
021_L107_RES3,RES,021_L106_RES3,PFX-15,111.199,70.863,0,True,True,UNVERIFIED_PIN_LEVEL,
022_L107_RES4,RES,022_L106_RES4,PFX-15,-67.594,-81.658,0,True,True,UNVERIFIED_PIN_LEVEL,
023_L107_RES5,RES,023_L106_RES5,PFX-15,4.154,54.934,0,True,True,UNVERIFIED_PIN_LEVEL,
024_L107_RES6,RES,024_L106_RES6,PFX-15,62.017,-34.962,0,True,True,UNVERIFIED_PIN_LEVEL,
025_L107_RES7,RES,025_L106_RES7,PFX-15,-14.08,56.828,0,True,True,UNVERIFIED_PIN_LEVEL,
026_L107_RES8,RES,026_L106_RES8,PFX-15,-53.466,71.105,0,True,True,UNVERIFIED_PIN_LEVEL,
027_L107_RES9,RES,027_L106_RES9,PFX-15,2.736,82.905,0,True,True,UNVERIFIED_PIN_LEVEL,
028_L107_RES10,RES,028_L106_RES10,PFX-15,-7.85,43.487,0,True,True,UNVERIFIED_PIN_LEVEL,
029_L107_RES11,RES,029_L106_RES11,PFX-15,-119.747,1.571,0,True,True,UNVERIFIED_PIN_LEVEL,
030_L107_RES12,RES,030_L106_RES12,PFX-15,91.23,-34.954,0,True,True,UNVERIFIED_PIN_LEVEL,
031_L107_RES13,RES,031_L106_RES13,PFX-15,101.928,6.888,0,True,True,UNVERIFIED_PIN_LEVEL,
032_L107_RES14,RES,032_L106_RES14,PFX-15,-66.132,62.698,0,True,True,UNVERIFIED_PIN_LEVEL,
033_L107_RES15,RES,033_L106_RES15,PFX-15,-115.959,-7.956,0,True,True,UNVERIFIED_PIN_LEVEL,
034_L107_RES16,RES,034_L106_RES16,PFX-15,-33.139,-41.004,0,True,True,UNVERIFIED_PIN_LEVEL,
035_L107_RES17,RES,035_L106_RES17,PFX-15,-6.409,82.141,0,True,True,UNVERIFIED_PIN_LEVEL,
036_L107_RES18,RES,036_L106_RES18,PFX-15,92.559,-22.654,0,True,True,UNVERIFIED_PIN_LEVEL,
037_L107_RES19,RES,037_L106_RES19,PFX-15,-110.948,41.087,0,True,True,UNVERIFIED_PIN_LEVEL,
038_L107_RES20,RES,038_L106_RES20,PFX-15,-68.392,-63.593,0,True,True,UNVERIFIED_PIN_LEVEL,
039_L107_RES21,RES,039_L106_RES21,PFX-15,13.774,-53.781,0,True,True,UNVERIFIED_PIN_LEVEL,
040_L107_RES22,RES,040_L106_RES22,PFX-15,-58.873,-11.738,0,True,True,UNVERIFIED_PIN_LEVEL,
041_L107_RES23,RES,041_L106_RES23,PFX-15,-109.21,-70.07,0,True,True,UNVERIFIED_PIN_LEVEL,
042_L107_RES24,RES,042_L106_RES24,PFX-15,30.614,19.717,0,True,True,UNVERIFIED_PIN_LEVEL,
043_L107_RES25,RES,043_L106_RES25,PFX-15,55.544,60.566,0,True,True,UNVERIFIED_PIN_LEVEL,
044_L107_RES26,RES,044_L106_RES26,PFX-15,-79.121,-47.915,0,True,True,UNVERIFIED_PIN_LEVEL,
045_L107_RES27,RES,045_L106_RES27,PFX-15,-46.512,46.277,0,True,True,UNVERIFIED_PIN_LEVEL,
046_L107_RES28,RES,046_L106_RES28,PFX-15,118.315,17.856,0,True,True,UNVERIFIED_PIN_LEVEL,
047_L107_CAP1,CAP,047_L106_CAP1,PFX-15,-115.872,-61.12,0,True,True,UNVERIFIED_PIN_LEVEL,
048_L107_CAP2,CAP,048_L106_CAP2,PFX-15,43.533,5.789,0,True,True,UNVERIFIED_PIN_LEVEL,
049_L107_CAP3,CAP,049_L106_CAP3,PFX-15,-7.903,50.467,0,True,True,UNVERIFIED_PIN_LEVEL,
050_L107_CAP4,CAP,050_L106_CAP4,PFX-15,-16.058,43.553,0,True,True,UNVERIFIED_PIN_LEVEL,
051_L107_CAP5,CAP,051_L106_CAP5,PFX-15,110.695,-80.095,0,True,True,UNVERIFIED_PIN_LEVEL,
052_L107_CAP6,CAP,052_L106_CAP6,PFX-15,34.366,8.669,0,True,True,UNVERIFIED_PIN_LEVEL,
053_L107_CAP7,CAP,053_L106_CAP7,PFX-15,-101.293,-27.657,0,True,True,UNVERIFIED_PIN_LEVEL,
054_L107_CAP8,CAP,054_L106_CAP8,PFX-15,34.781,76.07,0,True,True,UNVERIFIED_PIN_LEVEL,
055_L107_CAP9,CAP,055_L106_CAP9,PFX-15,33.523,-57.739,0,True,True,UNVERIFIED_PIN_LEVEL,
056_L107_CAP10,CAP,056_L106_CAP10,PFX-15,35.601,53.221,0,True,True,UNVERIFIED_PIN_LEVEL,
057_L107_CAP11,CAP,057_L106_CAP11,PFX-15,37.758,65.638,0,True,True,UNVERIFIED_PIN_LEVEL,
058_L107_CAP12,CAP,058_L106_CAP12,PFX-15,-55.554,-34.148,0,True,True,UNVERIFIED_PIN_LEVEL,
059_L107_CAP13,CAP,059_L106_CAP13,PFX-15,-45.387,79.482,0,True,True,UNVERIFIED_PIN_LEVEL,
060_L107_CAP14,CAP,060_L106_CAP14,PFX-15,6.747,-42.045,0,True,True,UNVERIFIED_PIN_LEVEL,
061_L107_CAP15,CAP,061_L106_CAP15,PFX-15,43.548,-30.754,0,True,True,UNVERIFIED_PIN_LEVEL,
062_L107_CAP16,CAP,062_L106_CAP16,PFX-15,-40.076,-44.856,0,True,True,UNVERIFIED_PIN_LEVEL,
063_L107_CAP17,CAP,063_L106_CAP17,PFX-15,-116.986,-25.441,0,True,True,UNVERIFIED_PIN_LEVEL,
064_L107_CAP18,CAP,064_L106_CAP18,PFX-15,-96.274,-52.99,0,True,True,UNVERIFIED_PIN_LEVEL,
065_L107_CAP19,CAP,065_L106_CAP19,PFX-15,61.452,11.852,0,True,True,UNVERIFIED_PIN_LEVEL,
066_L107_CAP20,CAP,066_L106_CAP20,PFX-15,-20.224,-58.869,0,True,True,UNVERIFIED_PIN_LEVEL,
067_L107_CAP21,CAP,067_L106_CAP21,PFX-15,33.773,28.953,0,True,True,UNVERIFIED_PIN_LEVEL,
068_L107_CAP22,CAP,068_L106_CAP22,PFX-15,110.104,-7.008,0,True,True,UNVERIFIED_PIN_LEVEL,
069_L107_CAP23,CAP,069_L106_CAP23,PFX-15,-85.536,-61.108,0,True,True,UNVERIFIED_PIN_LEVEL,
070_L107_CAP24,CAP,070_L106_CAP24,PFX-15,113.783,32.553,0,True,True,UNVERIFIED_PIN_LEVEL,
071_L107_CAP25,CAP,071_L106_CAP25,PFX-15,-69.696,-3.355,0,True,True,UNVERIFIED_PIN_LEVEL,
072_L107_CAP26,CAP,072_L106_CAP26,PFX-15,64.014,31.889,0,True,True,UNVERIFIED_PIN_LEVEL,
073_L107_CAP27,CAP,073_L106_CAP27,PFX-15,-96.5,-21.431,0,True,True,UNVERIFIED_PIN_LEVEL,
074_L107_CAP28,CAP,074_L106_CAP28,PFX-15,68.81,-57.279,0,True,True,UNVERIFIED_PIN_LEVEL,
075_L107_CAP29,CAP,075_L106_CAP29,PFX-15,3.791,76.72,0,True,True,UNVERIFIED_PIN_LEVEL,
076_L107_CAP30,CAP,076_L106_CAP30,PFX-15,-7.891,-44.846,0,True,True,UNVERIFIED_PIN_LEVEL,
077_L107_INDUCTOR1,INDUCTOR,077_L106_INDUCTOR1,PFX-15,-95.0,67.0,0,True,True,UNVERIFIED_PIN_LEVEL,
078_L107_INDUCTOR2,INDUCTOR,078_L106_INDUCTOR2,PFX-15,-95.0,47.0,0,True,True,UNVERIFIED_PIN_LEVEL,
079_L107_INDUCTOR3,INDUCTOR,079_L106_INDUCTOR3,PFX-15,-95.0,27.0,0,True,True,UNVERIFIED_PIN_LEVEL,
080_L107_INDUCTOR4,INDUCTOR,080_L106_INDUCTOR4,PFX-15,-95.0,7.0,0,True,True,UNVERIFIED_PIN_LEVEL,
081_L107_INDUCTOR5,INDUCTOR,081_L106_INDUCTOR5,PFX-15,-61.0,72.0,0,True,True,UNVERIFIED_PIN_LEVEL,
082_L107_INDUCTOR6,INDUCTOR,082_L106_INDUCTOR6,PFX-15,-61.0,52.0,0,True,True,UNVERIFIED_PIN_LEVEL,
083_L107_MOSFET1,MOSFET,083_L106_MOSFET1,PFX-15,-115.0,71.0,0,True,True,UNVERIFIED_PIN_LEVEL,
084_L107_MOSFET2,MOSFET,084_L106_MOSFET2,PFX-15,-114.0,55.0,0,True,True,UNVERIFIED_PIN_LEVEL,
085_L107_MOSFET3,MOSFET,085_L106_MOSFET3,PFX-15,-115.0,36.0,0,True,True,UNVERIFIED_PIN_LEVEL,
086_L107_MOSFET4,MOSFET,086_L106_MOSFET4,PFX-15,-114.0,18.0,0,True,True,UNVERIFIED_PIN_LEVEL,
087_L107_MOSFET5,MOSFET,087_L106_MOSFET5,PFX-15,-76.0,81.0,0,True,True,UNVERIFIED_PIN_LEVEL,
088_L107_MOSFET6,MOSFET,088_L106_MOSFET6,PFX-15,-48.0,81.0,0,True,True,UNVERIFIED_PIN_LEVEL,
089_L107_CONNECTOR1,CONNECTOR,089_L106_CONNECTOR1,PFX-15,-121.0,45.0,0,True,True,UNVERIFIED_PIN_LEVEL,
090_L107_CONNECTOR2,CONNECTOR,090_L106_CONNECTOR2,PFX-15,-122.0,-67.0,0,True,True,UNVERIFIED_PIN_LEVEL,
091_L107_CONNECTOR3,CONNECTOR,091_L106_CONNECTOR3,PFX-15,122.0,81.0,0,True,True,UNVERIFIED_PIN_LEVEL,
092_L107_CONNECTOR4,CONNECTOR,092_L106_CONNECTOR4,PFX-15,124.0,11.0,0,True,True,UNVERIFIED_PIN_LEVEL,
093_L107_J1,PCIE,093_L106_J1,PFX-15,18.0,-79.0,0,True,True,UNVERIFIED_PIN_LEVEL,
094_L107_LED1,LED,094_L106_LED1,PFX-15,55.0,-50.0,0,True,True,UNVERIFIED_PIN_LEVEL,
095_L107_LED2,LED,095_L106_LED2,PFX-15,68.0,-50.0,0,True,True,UNVERIFIED_PIN_LEVEL,
096_L107_LED3,LED,096_L106_LED3,PFX-15,81.0,-50.0,0,True,True,UNVERIFIED_PIN_LEVEL,
097_L107_LED4,LED,097_L106_LED4,PFX-15,94.0,-50.0,0,True,True,UNVERIFIED_PIN_LEVEL,
098_L107_SWITCH1,SWITCH,098_L106_SWITCH1,PFX-15,-117.0,-78.0,0,True,True,UNVERIFIED_PIN_LEVEL,
099_L107_SWITCH2,SWITCH,099_L106_SWITCH2,PFX-15,-78.0,-77.0,0,True,True,UNVERIFIED_PIN_LEVEL,
100_L107_SWITCH3,SWITCH,100_L106_SWITCH3,PFX-15,126.0,-81.0,0,True,True,UNVERIFIED_PIN_LEVEL,
101_L107_DIODE1,DIODE,101_L106_DIODE1,PFX-15,68.1,56.239,0,True,True,UNVERIFIED_PIN_LEVEL,
102_L107_DIODE2,DIODE,102_L106_DIODE2,PFX-15,83.001,-12.68,0,True,True,UNVERIFIED_PIN_LEVEL,
103_L107_DIODE3,DIODE,103_L106_DIODE3,PFX-15,-79.132,-32.474,0,True,True,UNVERIFIED_PIN_LEVEL,
104_L107_FERRITE1,FERRITE,104_L106_FERRITE1,PFX-15,95.747,-7.488,0,True,True,UNVERIFIED_PIN_LEVEL,
105_L107_FERRITE2,FERRITE,105_L106_FERRITE2,PFX-15,108.766,-43.932,0,True,True,UNVERIFIED_PIN_LEVEL,
106_L107_W149,REMOVABLE_TRUST_JUMPER,106_L106_W141,PFX-15,116.0,-66.0,0,True,True,UNVERIFIED_PIN_LEVEL,
107_L107_S107_FlashBusArbiter,FLASH_ARBITER,008_L106_IC5,PFX-15,-57.0,-34.0,0,True,True,UNVERIFIED_PIN_LEVEL,
108_L107_S108_RecoveryInputPullup,RECOVERY_INPUT_RESISTOR,021_L106_RES3,PFX-15,47.0,-54.0,0,True,True,UNVERIFIED_PIN_LEVEL,
109_L107_S109_RecoveryButton,RECOVERY_SWITCH,096_L106_LED3,PFX-15,59.0,-49.0,0,True,True,UNVERIFIED_PIN_LEVEL,
110_L107_S110_WriteProtectSwitch,FLASH_WRITE_PROTECT,095_L106_LED2,PFX-15,80.0,-49.0,0,True,True,UNVERIFIED_PIN_LEVEL,
111_L107_S111_FirmwareServiceHeader,SERVICE_HEADER,092_L106_CONNECTOR4,PFX-15,-67.0,-72.0,0,True,True,UNVERIFIED_PIN_LEVEL,
112_L107_S112_BootPolicySelector,BOOT_SELECTOR,093_L106_J1,PFX-15,82.0,-72.0,0,True,True,UNVERIFIED_PIN_LEVEL,
113_L107_PFX15_BarePCB,BOARD,107_L106_SFX14_BarePCB,PFX-15,0.0,0.0,0,False,True,UNVERIFIED_PIN_LEVEL,
114_L107_Copper_L01_Reference,REFERENCE_LAYER,108_L106_Copper_L01_Reference,PFX-15,0.0,0.0,0,False,True,UNVERIFIED_PIN_LEVEL,
115_L107_Copper_L02_Reference,REFERENCE_LAYER,109_L106_Copper_L02_Reference,PFX-15,0.0,0.0,0,False,True,UNVERIFIED_PIN_LEVEL,
116_L107_Copper_L03_Reference,REFERENCE_LAYER,110_L106_Copper_L03_Reference,PFX-15,0.0,0.0,0,False,True,UNVERIFIED_PIN_LEVEL,
117_L107_Copper_L04_Reference,REFERENCE_LAYER,111_L106_Copper_L04_Reference,PFX-15,0.0,0.0,0,False,True,UNVERIFIED_PIN_LEVEL,
118_L107_Copper_L05_Reference,REFERENCE_LAYER,112_L106_Copper_L05_Reference,PFX-15,0.0,0.0,0,False,True,UNVERIFIED_PIN_LEVEL,
119_L107_Copper_L06_Reference,REFERENCE_LAYER,113_L106_Copper_L06_Reference,PFX-15,0.0,0.0,0,False,True,UNVERIFIED_PIN_LEVEL,
120_L107_Copper_L07_Reference,REFERENCE_LAYER,114_L106_Copper_L07_Reference,PFX-15,0.0,0.0,0,False,True,UNVERIFIED_PIN_LEVEL,
121_L107_Copper_L08_Reference,REFERENCE_LAYER,115_L106_Copper_L08_Reference,PFX-15,0.0,0.0,0,False,True,UNVERIFIED_PIN_LEVEL,
122_L107_Copper_L09_Reference,REFERENCE_LAYER,116_L106_Copper_L09_Reference,PFX-15,0.0,0.0,0,False,True,UNVERIFIED_PIN_LEVEL,
123_L107_Copper_L10_Reference,REFERENCE_LAYER,117_L106_Copper_L10_Reference,PFX-15,0.0,0.0,0,False,True,UNVERIFIED_PIN_LEVEL,
124_L107_CPU_Socket_Frame,SOCKET,118_L106_CPU_Socket_Frame,PFX-15,0.0,0.0,0,False,True,UNVERIFIED_PIN_LEVEL,
125_L107_CPU_HeatSpreader,CPU_HARDWARE,119_L106_CPU_HeatSpreader,PFX-15,0.0,0.0,0,False,True,UNVERIFIED_PIN_LEVEL,
126_L107_PCIe_Socket_Detail,CONNECTOR_DETAIL,120_L106_PCIe_Socket_Detail,PFX-15,18.0,-79.0,0,False,True,UNVERIFIED_PIN_LEVEL,
127_L107_DDR4_Footprint_U2,BGA_FOOTPRINT,121_L106_DDR4_Footprint_U2,PFX-15,87.0,50.0,0,False,True,UNVERIFIED_PIN_LEVEL,
128_L107_DDR4_Footprint_U3,BGA_FOOTPRINT,122_L106_DDR4_Footprint_U3,PFX-15,87.0,23.0,0,False,True,UNVERIFIED_PIN_LEVEL,
129_L107_SPI_Flash_Footprint,FLASH_FOOTPRINT,123_L106_SPI_Flash_Footprint,PFX-15,-50.0,-55.0,0,False,True,UNVERIFIED_PIN_LEVEL,
130_L107_Memory_TraceDetail_1,DECORATIVE,124_L106_Memory_TraceDetail_1,PFX-15,0.0,0.0,0,False,True,UNVERIFIED_PIN_LEVEL,
131_L107_Memory_TraceDetail_2,DECORATIVE,125_L106_Memory_TraceDetail_2,PFX-15,0.0,0.0,0,False,True,UNVERIFIED_PIN_LEVEL,
132_L107_Memory_TraceDetail_3,DECORATIVE,126_L106_Memory_TraceDetail_3,PFX-15,0.0,0.0,0,False,True,UNVERIFIED_PIN_LEVEL,
133_L107_GroundVia,VIA_REFERENCE,127_L106_GroundVia,PFX-15,0.0,0.0,0,False,True,UNVERIFIED_PIN_LEVEL,
134_L107_PowerVia,VIA_REFERENCE,128_L106_PowerVia,PFX-15,0.0,0.0,0,False,True,UNVERIFIED_PIN_LEVEL,
135_L107_SignalVia,VIA_REFERENCE,129_L106_SignalVia,PFX-15,0.0,0.0,0,False,True,UNVERIFIED_PIN_LEVEL,
136_L107_CPU_PowerGood_Fault,FAULT_OVERLAY,130_L106_CPU_PowerGood_Fault,PFX-15,0.0,0.0,0,False,True,UNVERIFIED_PIN_LEVEL,
137_L107_SPI_SolderBridge_Fault,FAULT_OVERLAY,131_L106_SPI_SolderBridge_Fault,PFX-15,0.0,0.0,0,False,True,UNVERIFIED_PIN_LEVEL,
138_L107_Storage_Contact_Fault,FAULT_OVERLAY,132_L106_Storage_Contact_Fault,PFX-15,0.0,0.0,0,False,True,UNVERIFIED_PIN_LEVEL,
139_L107_12V_Input_TestPad,TESTPAD,133_L106_12V_Input_TestPad,PFX-15,0.0,0.0,0,False,True,UNVERIFIED_PIN_LEVEL,
140_L107_VCORE_TestPad,TESTPAD,134_L106_VCORE_TestPad,PFX-15,0.0,0.0,0,False,True,UNVERIFIED_PIN_LEVEL,
141_L107_DDR_VDD_TestPad,TESTPAD,135_L106_DDR_VDD_TestPad,PFX-15,0.0,0.0,0,False,True,UNVERIFIED_PIN_LEVEL,
142_L107_W149_Safety_TestPad,TESTPAD,136_L106_W141_Safety_TestPad,PFX-15,0.0,0.0,0,False,True,UNVERIFIED_PIN_LEVEL,
143_L107_DualFlash_ServicePad_Reference,TEST_PAD_REFERENCE,134_L106_VCORE_TestPad,PFX-15,-9.0,-70.0,0,False,True,UNVERIFIED_PIN_LEVEL,
144_L107_RecoveryBoot_TraceReference,COPPER_REFERENCE,127_L106_GroundVia,PFX-15,35.0,-63.0,0,False,True,UNVERIFIED_PIN_LEVEL,
```

### Annex B. Installable components: 112

Source path: `Data/L107_Components_112.csv`

```csv
model,ref,kind,board,x_mm,y_mm,orientation_deg,installed_at_start,footprint_status,netlist_status
001_L107_U1,U1,CPU,PFX-15,0.0,0.0,0,False,ART_PLACEHOLDER,NOT_CERTIFIABLE
002_L107_U2,U2,DDR,PFX-15,87.0,50.0,0,False,ART_PLACEHOLDER,NOT_CERTIFIABLE
003_L107_U3,U3,DDR,PFX-15,87.0,23.0,0,False,ART_PLACEHOLDER,NOT_CERTIFIABLE
004_L107_IC1,IC1,IC,PFX-15,-51.0,52.0,0,False,ART_PLACEHOLDER,NOT_CERTIFIABLE
005_L107_U5_RecoveryController,U5,RECOVERY_CONTROLLER,PFX-15,-79.0,-43.0,0,False,ART_PLACEHOLDER,NOT_CERTIFIABLE
006_L107_U6_PrimarySPIFlash,U6,PRIMARY_SPI_NOR,PFX-15,-22.0,-54.0,0,False,ART_PLACEHOLDER,NOT_CERTIFIABLE
007_L107_U7_RecoverySPIFlash,U7,PROTECTED_RECOVERY_SPI_NOR,PFX-15,7.0,-54.0,0,False,ART_PLACEHOLDER,NOT_CERTIFIABLE
008_L107_IC5,IC5,IC,PFX-15,106.0,65.0,0,False,ART_PLACEHOLDER,NOT_CERTIFIABLE
009_L107_U7,U7,STORAGE,PFX-15,33.0,-43.0,0,False,ART_PLACEHOLDER,NOT_CERTIFIABLE
010_L107_Y1,Y1,OSC,PFX-15,-33.0,75.0,0,False,ART_PLACEHOLDER,NOT_CERTIFIABLE
011_L107_U6,U6,FLASH,PFX-15,-50.0,-55.0,0,False,ART_PLACEHOLDER,NOT_CERTIFIABLE
012_L107_U5,U5,EC,PFX-15,-99.0,-38.0,0,False,ART_PLACEHOLDER,NOT_CERTIFIABLE
013_L107_U8,U8,IO,PFX-15,68.0,-26.0,0,False,ART_PLACEHOLDER,NOT_CERTIFIABLE
014_L107_U9,U9,IO,PFX-15,115.0,-15.0,0,False,ART_PLACEHOLDER,NOT_CERTIFIABLE
015_L107_REG1,REG1,REG,PFX-15,-76.0,-17.0,0,False,ART_PLACEHOLDER,NOT_CERTIFIABLE
016_L107_REG2,REG2,REG,PFX-15,68.0,-71.0,0,False,ART_PLACEHOLDER,NOT_CERTIFIABLE
017_L107_U15,U15,SAFETY,PFX-15,75.0,-35.0,0,False,ART_PLACEHOLDER,NOT_CERTIFIABLE
018_L107_TEMP1,TEMP1,TEMP,PFX-15,-25.0,71.0,0,False,ART_PLACEHOLDER,NOT_CERTIFIABLE
019_L107_RES1,RES1,RES,PFX-15,116.84,-32.698,0,False,ART_PLACEHOLDER,NOT_CERTIFIABLE
020_L107_RES2,RES2,RES,PFX-15,48.49,-55.554,0,False,ART_PLACEHOLDER,NOT_CERTIFIABLE
021_L107_RES3,RES3,RES,PFX-15,111.199,70.863,0,False,ART_PLACEHOLDER,NOT_CERTIFIABLE
022_L107_RES4,RES4,RES,PFX-15,-67.594,-81.658,0,False,ART_PLACEHOLDER,NOT_CERTIFIABLE
023_L107_RES5,RES5,RES,PFX-15,4.154,54.934,0,False,ART_PLACEHOLDER,NOT_CERTIFIABLE
024_L107_RES6,RES6,RES,PFX-15,62.017,-34.962,0,False,ART_PLACEHOLDER,NOT_CERTIFIABLE
025_L107_RES7,RES7,RES,PFX-15,-14.08,56.828,0,False,ART_PLACEHOLDER,NOT_CERTIFIABLE
026_L107_RES8,RES8,RES,PFX-15,-53.466,71.105,0,False,ART_PLACEHOLDER,NOT_CERTIFIABLE
027_L107_RES9,RES9,RES,PFX-15,2.736,82.905,0,False,ART_PLACEHOLDER,NOT_CERTIFIABLE
028_L107_RES10,RES10,RES,PFX-15,-7.85,43.487,0,False,ART_PLACEHOLDER,NOT_CERTIFIABLE
029_L107_RES11,RES11,RES,PFX-15,-119.747,1.571,0,False,ART_PLACEHOLDER,NOT_CERTIFIABLE
030_L107_RES12,RES12,RES,PFX-15,91.23,-34.954,0,False,ART_PLACEHOLDER,NOT_CERTIFIABLE
031_L107_RES13,RES13,RES,PFX-15,101.928,6.888,0,False,ART_PLACEHOLDER,NOT_CERTIFIABLE
032_L107_RES14,RES14,RES,PFX-15,-66.132,62.698,0,False,ART_PLACEHOLDER,NOT_CERTIFIABLE
033_L107_RES15,RES15,RES,PFX-15,-115.959,-7.956,0,False,ART_PLACEHOLDER,NOT_CERTIFIABLE
034_L107_RES16,RES16,RES,PFX-15,-33.139,-41.004,0,False,ART_PLACEHOLDER,NOT_CERTIFIABLE
035_L107_RES17,RES17,RES,PFX-15,-6.409,82.141,0,False,ART_PLACEHOLDER,NOT_CERTIFIABLE
036_L107_RES18,RES18,RES,PFX-15,92.559,-22.654,0,False,ART_PLACEHOLDER,NOT_CERTIFIABLE
037_L107_RES19,RES19,RES,PFX-15,-110.948,41.087,0,False,ART_PLACEHOLDER,NOT_CERTIFIABLE
038_L107_RES20,RES20,RES,PFX-15,-68.392,-63.593,0,False,ART_PLACEHOLDER,NOT_CERTIFIABLE
039_L107_RES21,RES21,RES,PFX-15,13.774,-53.781,0,False,ART_PLACEHOLDER,NOT_CERTIFIABLE
040_L107_RES22,RES22,RES,PFX-15,-58.873,-11.738,0,False,ART_PLACEHOLDER,NOT_CERTIFIABLE
041_L107_RES23,RES23,RES,PFX-15,-109.21,-70.07,0,False,ART_PLACEHOLDER,NOT_CERTIFIABLE
042_L107_RES24,RES24,RES,PFX-15,30.614,19.717,0,False,ART_PLACEHOLDER,NOT_CERTIFIABLE
043_L107_RES25,RES25,RES,PFX-15,55.544,60.566,0,False,ART_PLACEHOLDER,NOT_CERTIFIABLE
044_L107_RES26,RES26,RES,PFX-15,-79.121,-47.915,0,False,ART_PLACEHOLDER,NOT_CERTIFIABLE
045_L107_RES27,RES27,RES,PFX-15,-46.512,46.277,0,False,ART_PLACEHOLDER,NOT_CERTIFIABLE
046_L107_RES28,RES28,RES,PFX-15,118.315,17.856,0,False,ART_PLACEHOLDER,NOT_CERTIFIABLE
047_L107_CAP1,CAP1,CAP,PFX-15,-115.872,-61.12,0,False,ART_PLACEHOLDER,NOT_CERTIFIABLE
048_L107_CAP2,CAP2,CAP,PFX-15,43.533,5.789,0,False,ART_PLACEHOLDER,NOT_CERTIFIABLE
049_L107_CAP3,CAP3,CAP,PFX-15,-7.903,50.467,0,False,ART_PLACEHOLDER,NOT_CERTIFIABLE
050_L107_CAP4,CAP4,CAP,PFX-15,-16.058,43.553,0,False,ART_PLACEHOLDER,NOT_CERTIFIABLE
051_L107_CAP5,CAP5,CAP,PFX-15,110.695,-80.095,0,False,ART_PLACEHOLDER,NOT_CERTIFIABLE
052_L107_CAP6,CAP6,CAP,PFX-15,34.366,8.669,0,False,ART_PLACEHOLDER,NOT_CERTIFIABLE
053_L107_CAP7,CAP7,CAP,PFX-15,-101.293,-27.657,0,False,ART_PLACEHOLDER,NOT_CERTIFIABLE
054_L107_CAP8,CAP8,CAP,PFX-15,34.781,76.07,0,False,ART_PLACEHOLDER,NOT_CERTIFIABLE
055_L107_CAP9,CAP9,CAP,PFX-15,33.523,-57.739,0,False,ART_PLACEHOLDER,NOT_CERTIFIABLE
056_L107_CAP10,CAP10,CAP,PFX-15,35.601,53.221,0,False,ART_PLACEHOLDER,NOT_CERTIFIABLE
057_L107_CAP11,CAP11,CAP,PFX-15,37.758,65.638,0,False,ART_PLACEHOLDER,NOT_CERTIFIABLE
058_L107_CAP12,CAP12,CAP,PFX-15,-55.554,-34.148,0,False,ART_PLACEHOLDER,NOT_CERTIFIABLE
059_L107_CAP13,CAP13,CAP,PFX-15,-45.387,79.482,0,False,ART_PLACEHOLDER,NOT_CERTIFIABLE
060_L107_CAP14,CAP14,CAP,PFX-15,6.747,-42.045,0,False,ART_PLACEHOLDER,NOT_CERTIFIABLE
061_L107_CAP15,CAP15,CAP,PFX-15,43.548,-30.754,0,False,ART_PLACEHOLDER,NOT_CERTIFIABLE
062_L107_CAP16,CAP16,CAP,PFX-15,-40.076,-44.856,0,False,ART_PLACEHOLDER,NOT_CERTIFIABLE
063_L107_CAP17,CAP17,CAP,PFX-15,-116.986,-25.441,0,False,ART_PLACEHOLDER,NOT_CERTIFIABLE
064_L107_CAP18,CAP18,CAP,PFX-15,-96.274,-52.99,0,False,ART_PLACEHOLDER,NOT_CERTIFIABLE
065_L107_CAP19,CAP19,CAP,PFX-15,61.452,11.852,0,False,ART_PLACEHOLDER,NOT_CERTIFIABLE
066_L107_CAP20,CAP20,CAP,PFX-15,-20.224,-58.869,0,False,ART_PLACEHOLDER,NOT_CERTIFIABLE
067_L107_CAP21,CAP21,CAP,PFX-15,33.773,28.953,0,False,ART_PLACEHOLDER,NOT_CERTIFIABLE
068_L107_CAP22,CAP22,CAP,PFX-15,110.104,-7.008,0,False,ART_PLACEHOLDER,NOT_CERTIFIABLE
069_L107_CAP23,CAP23,CAP,PFX-15,-85.536,-61.108,0,False,ART_PLACEHOLDER,NOT_CERTIFIABLE
070_L107_CAP24,CAP24,CAP,PFX-15,113.783,32.553,0,False,ART_PLACEHOLDER,NOT_CERTIFIABLE
071_L107_CAP25,CAP25,CAP,PFX-15,-69.696,-3.355,0,False,ART_PLACEHOLDER,NOT_CERTIFIABLE
072_L107_CAP26,CAP26,CAP,PFX-15,64.014,31.889,0,False,ART_PLACEHOLDER,NOT_CERTIFIABLE
073_L107_CAP27,CAP27,CAP,PFX-15,-96.5,-21.431,0,False,ART_PLACEHOLDER,NOT_CERTIFIABLE
074_L107_CAP28,CAP28,CAP,PFX-15,68.81,-57.279,0,False,ART_PLACEHOLDER,NOT_CERTIFIABLE
075_L107_CAP29,CAP29,CAP,PFX-15,3.791,76.72,0,False,ART_PLACEHOLDER,NOT_CERTIFIABLE
076_L107_CAP30,CAP30,CAP,PFX-15,-7.891,-44.846,0,False,ART_PLACEHOLDER,NOT_CERTIFIABLE
077_L107_INDUCTOR1,INDUCTOR1,INDUCTOR,PFX-15,-95.0,67.0,0,False,ART_PLACEHOLDER,NOT_CERTIFIABLE
078_L107_INDUCTOR2,INDUCTOR2,INDUCTOR,PFX-15,-95.0,47.0,0,False,ART_PLACEHOLDER,NOT_CERTIFIABLE
079_L107_INDUCTOR3,INDUCTOR3,INDUCTOR,PFX-15,-95.0,27.0,0,False,ART_PLACEHOLDER,NOT_CERTIFIABLE
080_L107_INDUCTOR4,INDUCTOR4,INDUCTOR,PFX-15,-95.0,7.0,0,False,ART_PLACEHOLDER,NOT_CERTIFIABLE
081_L107_INDUCTOR5,INDUCTOR5,INDUCTOR,PFX-15,-61.0,72.0,0,False,ART_PLACEHOLDER,NOT_CERTIFIABLE
082_L107_INDUCTOR6,INDUCTOR6,INDUCTOR,PFX-15,-61.0,52.0,0,False,ART_PLACEHOLDER,NOT_CERTIFIABLE
083_L107_MOSFET1,MOSFET1,MOSFET,PFX-15,-115.0,71.0,0,False,ART_PLACEHOLDER,NOT_CERTIFIABLE
084_L107_MOSFET2,MOSFET2,MOSFET,PFX-15,-114.0,55.0,0,False,ART_PLACEHOLDER,NOT_CERTIFIABLE
085_L107_MOSFET3,MOSFET3,MOSFET,PFX-15,-115.0,36.0,0,False,ART_PLACEHOLDER,NOT_CERTIFIABLE
086_L107_MOSFET4,MOSFET4,MOSFET,PFX-15,-114.0,18.0,0,False,ART_PLACEHOLDER,NOT_CERTIFIABLE
087_L107_MOSFET5,MOSFET5,MOSFET,PFX-15,-76.0,81.0,0,False,ART_PLACEHOLDER,NOT_CERTIFIABLE
088_L107_MOSFET6,MOSFET6,MOSFET,PFX-15,-48.0,81.0,0,False,ART_PLACEHOLDER,NOT_CERTIFIABLE
089_L107_CONNECTOR1,CONNECTOR1,CONNECTOR,PFX-15,-121.0,45.0,0,False,ART_PLACEHOLDER,NOT_CERTIFIABLE
090_L107_CONNECTOR2,CONNECTOR2,CONNECTOR,PFX-15,-122.0,-67.0,0,False,ART_PLACEHOLDER,NOT_CERTIFIABLE
091_L107_CONNECTOR3,CONNECTOR3,CONNECTOR,PFX-15,122.0,81.0,0,False,ART_PLACEHOLDER,NOT_CERTIFIABLE
092_L107_CONNECTOR4,CONNECTOR4,CONNECTOR,PFX-15,124.0,11.0,0,False,ART_PLACEHOLDER,NOT_CERTIFIABLE
093_L107_J1,J1,PCIE,PFX-15,18.0,-79.0,0,False,ART_PLACEHOLDER,NOT_CERTIFIABLE
094_L107_LED1,LED1,LED,PFX-15,55.0,-50.0,0,False,ART_PLACEHOLDER,NOT_CERTIFIABLE
095_L107_LED2,LED2,LED,PFX-15,68.0,-50.0,0,False,ART_PLACEHOLDER,NOT_CERTIFIABLE
096_L107_LED3,LED3,LED,PFX-15,81.0,-50.0,0,False,ART_PLACEHOLDER,NOT_CERTIFIABLE
097_L107_LED4,LED4,LED,PFX-15,94.0,-50.0,0,False,ART_PLACEHOLDER,NOT_CERTIFIABLE
098_L107_SWITCH1,SWITCH1,SWITCH,PFX-15,-117.0,-78.0,0,False,ART_PLACEHOLDER,NOT_CERTIFIABLE
099_L107_SWITCH2,SWITCH2,SWITCH,PFX-15,-78.0,-77.0,0,False,ART_PLACEHOLDER,NOT_CERTIFIABLE
100_L107_SWITCH3,SWITCH3,SWITCH,PFX-15,126.0,-81.0,0,False,ART_PLACEHOLDER,NOT_CERTIFIABLE
101_L107_DIODE1,DIODE1,DIODE,PFX-15,68.1,56.239,0,False,ART_PLACEHOLDER,NOT_CERTIFIABLE
102_L107_DIODE2,DIODE2,DIODE,PFX-15,83.001,-12.68,0,False,ART_PLACEHOLDER,NOT_CERTIFIABLE
103_L107_DIODE3,DIODE3,DIODE,PFX-15,-79.132,-32.474,0,False,ART_PLACEHOLDER,NOT_CERTIFIABLE
104_L107_FERRITE1,FERRITE1,FERRITE,PFX-15,95.747,-7.488,0,False,ART_PLACEHOLDER,NOT_CERTIFIABLE
105_L107_FERRITE2,FERRITE2,FERRITE,PFX-15,108.766,-43.932,0,False,ART_PLACEHOLDER,NOT_CERTIFIABLE
106_L107_W149,W149,REMOVABLE_TRUST_JUMPER,PFX-15,116.0,-66.0,0,False,ART_PLACEHOLDER,NOT_CERTIFIABLE
107_L107_S107_FlashBusArbiter,S107,FLASH_ARBITER,PFX-15,-57.0,-34.0,0,False,ART_PLACEHOLDER,NOT_CERTIFIABLE
108_L107_S108_RecoveryInputPullup,S108,RECOVERY_INPUT_RESISTOR,PFX-15,47.0,-54.0,0,False,ART_PLACEHOLDER,NOT_CERTIFIABLE
109_L107_S109_RecoveryButton,S109,RECOVERY_SWITCH,PFX-15,59.0,-49.0,0,False,ART_PLACEHOLDER,NOT_CERTIFIABLE
110_L107_S110_WriteProtectSwitch,S110,FLASH_WRITE_PROTECT,PFX-15,80.0,-49.0,0,False,ART_PLACEHOLDER,NOT_CERTIFIABLE
111_L107_S111_FirmwareServiceHeader,S111,SERVICE_HEADER,PFX-15,-67.0,-72.0,0,False,ART_PLACEHOLDER,NOT_CERTIFIABLE
112_L107_S112_BootPolicySelector,S112,BOOT_SELECTOR,PFX-15,82.0,-72.0,0,False,ART_PLACEHOLDER,NOT_CERTIFIABLE
```

### Annex C. Routes: 148 NOT CERTIFIABLE

Source path: `Data/L107_Routes_148_PLANNING.csv`

```csv
route_id,subsystem,net_id,source_component,source_pin,destination_component,destination_pin,copper_layer,status,complete_at_start
W001,POWER_STANDBY,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W002,POWER_STANDBY,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W003,POWER_STANDBY,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W004,POWER_STANDBY,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W005,POWER_STANDBY,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W006,POWER_STANDBY,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W007,POWER_STANDBY,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W008,POWER_STANDBY,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W009,POWER_STANDBY,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W010,POWER_STANDBY,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W011,POWER_STANDBY,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W012,POWER_STANDBY,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W013,POWER_STANDBY,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W014,POWER_STANDBY,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W015,POWER_STANDBY,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W016,POWER_STANDBY,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W017,POWER_STANDBY,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W018,POWER_STANDBY,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W019,POWER_STANDBY,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W020,POWER_STANDBY,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W021,CPU_VRM_RESET,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W022,CPU_VRM_RESET,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W023,CPU_VRM_RESET,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W024,CPU_VRM_RESET,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W025,CPU_VRM_RESET,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W026,CPU_VRM_RESET,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W027,CPU_VRM_RESET,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W028,CPU_VRM_RESET,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W029,CPU_VRM_RESET,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W030,CPU_VRM_RESET,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W031,CPU_VRM_RESET,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W032,CPU_VRM_RESET,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W033,CPU_VRM_RESET,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W034,CPU_VRM_RESET,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W035,CPU_VRM_RESET,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W036,CPU_VRM_RESET,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W037,CPU_VRM_RESET,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W038,CPU_VRM_RESET,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W039,CPU_VRM_RESET,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W040,CPU_VRM_RESET,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W041,CPU_VRM_RESET,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W042,CPU_VRM_RESET,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W043,CPU_VRM_RESET,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W044,CPU_VRM_RESET,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W045,PRIMARY_SPI,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W046,PRIMARY_SPI,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W047,PRIMARY_SPI,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W048,PRIMARY_SPI,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W049,PRIMARY_SPI,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W050,PRIMARY_SPI,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W051,PRIMARY_SPI,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W052,PRIMARY_SPI,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W053,PRIMARY_SPI,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W054,PRIMARY_SPI,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W055,PRIMARY_SPI,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W056,PRIMARY_SPI,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W057,PRIMARY_SPI,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W058,PRIMARY_SPI,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W059,PRIMARY_SPI,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W060,PRIMARY_SPI,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W061,PRIMARY_SPI,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W062,PRIMARY_SPI,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W063,PRIMARY_SPI,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W064,PRIMARY_SPI,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W065,PRIMARY_SPI,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W066,PRIMARY_SPI,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W067,RECOVERY_SPI,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W068,RECOVERY_SPI,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W069,RECOVERY_SPI,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W070,RECOVERY_SPI,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W071,RECOVERY_SPI,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W072,RECOVERY_SPI,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W073,RECOVERY_SPI,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W074,RECOVERY_SPI,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W075,RECOVERY_SPI,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W076,RECOVERY_SPI,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W077,RECOVERY_SPI,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W078,RECOVERY_SPI,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W079,RECOVERY_SPI,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W080,RECOVERY_SPI,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W081,RECOVERY_SPI,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W082,RECOVERY_SPI,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W083,RECOVERY_SPI,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W084,RECOVERY_SPI,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W085,RECOVERY_SPI,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W086,RECOVERY_SPI,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W087,RECOVERY_ARBITRATION,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W088,RECOVERY_ARBITRATION,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W089,RECOVERY_ARBITRATION,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W090,RECOVERY_ARBITRATION,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W091,RECOVERY_ARBITRATION,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W092,RECOVERY_ARBITRATION,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W093,RECOVERY_ARBITRATION,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W094,RECOVERY_ARBITRATION,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W095,RECOVERY_ARBITRATION,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W096,RECOVERY_ARBITRATION,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W097,RECOVERY_ARBITRATION,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W098,RECOVERY_ARBITRATION,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W099,RECOVERY_ARBITRATION,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W100,RECOVERY_ARBITRATION,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W101,RECOVERY_ARBITRATION,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W102,RECOVERY_ARBITRATION,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W103,RECOVERY_ARBITRATION,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W104,RECOVERY_ARBITRATION,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W105,RECOVERY_ARBITRATION,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W106,RECOVERY_ARBITRATION,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W107,RECOVERY_ARBITRATION,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W108,RECOVERY_ARBITRATION,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W109,DDR_BOOT_POST,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W110,DDR_BOOT_POST,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W111,DDR_BOOT_POST,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W112,DDR_BOOT_POST,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W113,DDR_BOOT_POST,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W114,DDR_BOOT_POST,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W115,DDR_BOOT_POST,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W116,DDR_BOOT_POST,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W117,DDR_BOOT_POST,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W118,DDR_BOOT_POST,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W119,DDR_BOOT_POST,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W120,DDR_BOOT_POST,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W121,DDR_BOOT_POST,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W122,DDR_BOOT_POST,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W123,DDR_BOOT_POST,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W124,DDR_BOOT_POST,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W125,DDR_BOOT_POST,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W126,DDR_BOOT_POST,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W127,DDR_BOOT_POST,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W128,DDR_BOOT_POST,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W129,SAFETY_MONITORING,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W130,SAFETY_MONITORING,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W131,SAFETY_MONITORING,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W132,SAFETY_MONITORING,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W133,SAFETY_MONITORING,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W134,SAFETY_MONITORING,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W135,SAFETY_MONITORING,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W136,SAFETY_MONITORING,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W137,SAFETY_MONITORING,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W138,SAFETY_MONITORING,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W139,SAFETY_MONITORING,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W140,SAFETY_MONITORING,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W141,SAFETY_MONITORING,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W142,SAFETY_MONITORING,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W143,SAFETY_MONITORING,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W144,SAFETY_MONITORING,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W145,SAFETY_MONITORING,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W146,SAFETY_MONITORING,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W147,SAFETY_MONITORING,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
W148,SAFETY_MONITORING,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
```

### Annex D. Vias: 46 NOT CERTIFIABLE

Source path: `Data/L107_Vias_46_PLANNING.csv`

```csv
via_id,net_id,from_layer,to_layer,xy_mm,via_type,status,installed_at_start
V01,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
V02,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
V03,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
V04,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
V05,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
V06,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
V07,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
V08,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
V09,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
V10,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
V11,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
V12,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
V13,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
V14,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
V15,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
V16,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
V17,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
V18,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
V19,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
V20,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
V21,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
V22,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
V23,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
V24,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
V25,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
V26,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
V27,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
V28,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
V29,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
V30,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
V31,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
V32,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
V33,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
V34,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
V35,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
V36,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
V37,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
V38,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
V39,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
V40,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
V41,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
V42,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
V43,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
V44,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
V45,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
V46,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,UNASSIGNED,NOT_CERTIFIABLE,False
```

### Annex E. Faults: 3

Source path: `Data/L107_RepairFaults_3.csv`

```csv
id,name,fault_type,observable_effect,tool,repair,proof
F1,U6 Primary SPI Chip-Select Contact,OPEN_SPI_CS,Flash reads fail before image verification,SPI Analyzer + Microscope,Qualified solder contact restoration,New valid SPI transactions
F2,Corrupted Primary Firmware Image,FIRMWARE_CORRUPTION,Flash reads complete but image authentication fails,Image Verifier + Recovery Console,Authenticated rewrite from protected U7 recovery bank,Fresh verified primary image and boot
F3,Recovery Request Pull-Up Failure,RECOVERY_INPUT,Recovery button not reliably sampled,Multimeter + Logic Analyzer,Replace faulty recovery request resistor,Fresh input transition and recovery test
```

### Annex F. Objectives: 6

Source path: `Data/L107_Objectives_6.csv`

```csv
id,name,requirements
01,Assembly,Install all 112 components and verify actual contacts
02,Electrical Network,"Complete 148 approved player routes, 46 vias, install W149"
03,Firmware Recovery,Authenticate recovery bank and restore corrupted primary
04,POST & Resilience,"Configure boot, pass POST and interrupted-update recovery"
05,Three Repairs,"Verify F1, F2 and F3 and complete fresh qualification"
06,W149 Safety Recovery,"Disconnect, inhibit, reconnect, revalidate, reset, certify"
```

### Annex G. W149 safety definition

Source path: `Data/L107_W149_Safety.json`

```json
{
  "level": 107,
  "jumper": "W149",
  "logical_source": "FirmwareTrustMonitor.TRUSTED_FIRMWARE_READY_OUT",
  "logical_destination": "IndependentPlatformSafetySupervisor.TRUSTED_FIRMWARE_READY_IN",
  "exact_component_ids": "SCHEMATIC_REQUIRED",
  "physical_pins": "UNASSIGNED",
  "connection_kind": "freshness_validated_digital_feedback",
  "required_recovery": [
    "disconnect actual jumper",
    "latch safety inhibition",
    "reconnect physical contacts",
    "authenticate fresh firmware image",
    "perform full fresh boot qualification",
    "explicitly reset safety",
    "record final independent certification"
  ],
  "status": "NOT_CERTIFIABLE_UNTIL_SCHEMATIC_APPROVED"
}
```

### Annex H. Source README

Source path: `Documentation/README_FIRST.md`

```markdown
# PROJECT SPARK — LEVEL 107: PHOENIX PROTOCOL
## PFX-15 realistic-style board-only model package

**WHAT THIS IS:** A Level 107 art and engineering-planning package. It is NOT a production-proven electrical circuit, gameplay implementation, commercial motherboard, or hand-finished AAA model set.

### Content
- 144 modular FBX models and 144 equivalent GLB previews.
- 112 independent, installable electronic components and 32 board-only structural/repair assets.
- Separate assembled visual reference GLB and FBX.
- Starter PBR maps, including a new PFX-15 2K firmware silkscreen.
- Component placement, proposed BOM, 148 unassigned route tasks, 46 unassigned via tasks, 3 fault definitions, 6 objectives and W149 recovery requirements.
- Unity Editor scripts for an art-only empty board and starter materials.

### Source/art lineage and accuracy
This package extends the supplied Project Spark Level 106 SFX-14 procedural mesh library. Compatible common CPU, VRM, DDR, connectors and passives were reused; six additional firmware/control component meshes and two board-only reference meshes were derived from existing electronic package geometry. The PFX-15-specific component manifest, dual-flash assignment, firmware-zone layout, new silkscreen, recovery data, W149 data and composed GLB reference are authored for Level 107.

FBX models are source-library binary FBX geometry, not newly sculpted artist-unique assets; the preview reference FBX retains reused geometry. The assembled GLB includes additional colored firmware-region indication and a modest XY scale adjustment, and may not exactly match the FBX reference. Import settings, component pivots, footprint accuracy and production materials must be verified in Unity.

### IMPORTANT: use source model dimensions correctly
The underlying inherited bare-board art measures 285 x 205 mm. The Level 107 target is 290 x 210 mm. The Unity Art Builder applies an XY footprint adjustment to the bare-board presentation. Reconcile source axes/scale and exact pad coordinates before a functional board prefab is built.

### Safety and electrical authority
- `Routes_148_PLANNING.csv` endpoints are UNASSIGNED and NOT_CERTIFIABLE.
- `Vias_46_PLANNING.csv` net identities and layer spans are UNASSIGNED and NOT_CERTIFIABLE.
- W149 physical pins and supervisor component assignments are NOT CERTIFIED.
- No real firmware authentication or update execution is included.
- The completed reference is artwork ONLY. Never convert it directly to gameplay starting state.
- A genuine real-time electrical topology and gameplay authority must be implemented separately and tested.

### Unity setup
1. Extract directory into `Assets/ProjectSpark/Level107/`.
2. Ensure Editor scripts live under `/Unity/Editor/`.
3. Open `Tools/Project Spark/Levels/107/1. Build EMPTY PFX-15 Board`.
4. Run `Tools/Project Spark/Levels/107/3. Create PBR Starter Materials`.
5. Assign and validate mesh materials, UVs and transforms.
6. Build true electrical netlist, assembly and routes using actual project gameplay APIs.

Initial playable conditions: 0/112 components, 0/148 routes, 0/46 vias, W149 DISCONNECTED, objectives 0/6.

### Level 107 recovery faults
- F1: U6 primary SPI chip-select contact
- F2: corrupted primary image
- F3: recovery request input/pull-up fault

### W149 recovery
Disconnect, independently inhibit, physically reconnect, reauthenticate firmware, fresh POST and boot, explicit safety reset, independent qualification.

**Do not claim AAA completion or Unity compatibility without actual engine import/Play Mode review.**
```

### Annex I. Empty art builder C#

Source path: `Unity/Editor/SparkL107BoardArtBuilder.cs`

```csharp
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.Level107.Editor
{
    public static class SparkL107BoardArtBuilder
    {
        private const string BoardSearch = "113_L107_PFX15_BarePCB t:Model";

        [MenuItem("Tools/Project Spark/Levels/107/1. Build EMPTY PFX-15 Board")]
        public static void BuildEmpty()
        {
            var guids = AssetDatabase.FindAssets(BoardSearch);
            if (guids.Length == 0)
            {
                Debug.LogError("[L107] Import the Level 107 Models_FBX before building.");
                return;
            }
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guids[0]));
            if (prefab == null) { Debug.LogError("[L107] Bare board FBX is not an importable GameObject."); return; }

            var root = new GameObject("L107_PFX15_EMPTY_ART_ROOT");
            Undo.RegisterCreatedObjectUndo(root, "Create Level107 board art");
            var board = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            if (board == null) { Object.DestroyImmediate(root); return; }
            Undo.SetTransformParent(board.transform,root.transform,"Parent Level107 art");
            board.name = "PFX15_BarePCB_ArtOnly";
            // Underlying source board: 285 x 205 mm. Target: 290 x 210 mm.
            board.transform.localScale = new Vector3(290f/285f,1f,210f/205f);
            Selection.activeGameObject = root;
            Debug.Log("[L107] EMPTY visual root created. NO components, routes, vias, firmware or objectives were completed.");
        }

        [MenuItem("Tools/Project Spark/Levels/107/2. Select Level 107 Art FBXs")]
        public static void SelectModels()
        {
            var guids=AssetDatabase.FindAssets("L107_ t:Model");
            var assets = new System.Collections.Generic.List<Object>();
            foreach(var guid in guids)
            {
                var p=AssetDatabase.GUIDToAssetPath(guid);
                if(p.Contains("Models_FBX/"))
                {
                    var a=AssetDatabase.LoadMainAssetAtPath(p);
                    if(a!=null) assets.Add(a);
                }
            }
            Selection.objects=assets.ToArray();
            Debug.Log("[L107] Found "+assets.Count+" FBX art assets.");
        }
    }
}
#endif
```

### Annex J. Starter material builder C#

Source path: `Unity/Editor/SparkL107StarterMaterials.cs`

```csharp
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;

namespace ProjectSpark.Level107.Editor
{
    public static class SparkL107StarterMaterials
    {
        [MenuItem("Tools/Project Spark/Levels/107/3. Create PBR Starter Materials")]
        public static void Generate()
        {
            const string folder="Assets/ProjectSpark/Level107/Materials_Generated";
            Directory.CreateDirectory(folder);
            string[][] specs = {
                new []{"PCB_Midnight_Blue","#0B294D","0.1"},
                new []{"Soldermask_Gloss","#103765","0.3"},
                new []{"FR4_Dielectric","#C7A56C","0.1"},
                new []{"Gold_Plating","#B99742","0.85"},
                new []{"Copper","#B77935","0.8"},
                new []{"Solder_Silver","#ADB7C1","0.75"},
                new []{"IC_Epoxy","#161B22","0.0"},
                new []{"Connector_Polymer","#20242A","0.0"},
                new []{"Brushed_Aluminum","#8F969E","0.75"},
                new []{"Flash_U6_Primary","#28252A","0.0"},
                new []{"Flash_U7_Recovery","#222D27","0.0"},
                new []{"Protection_Red","#B03D4B","0.1"}
            };
            var shader=Shader.Find("Universal Render Pipeline/Lit");
            if(shader==null) shader=Shader.Find("Standard");
            if(shader==null) { Debug.LogError("[L107] No supported shader found."); return; }
            foreach(var sp in specs)
            {
                var path=folder+"/"+sp[0]+".mat";
                if(AssetDatabase.LoadAssetAtPath<Material>(path)!=null) continue;
                var mat=new Material(shader);
                Color col=Color.white; ColorUtility.TryParseHtmlString(sp[1],out col);
                if(mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor",col);
                if(mat.HasProperty("_Color")) mat.SetColor("_Color",col);
                var metallic=float.Parse(sp[2],System.Globalization.CultureInfo.InvariantCulture);
                if(mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic",metallic);
                if(mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness",0.48f);
                AssetDatabase.CreateAsset(mat,path);
            }
            AssetDatabase.SaveAssets();
            Debug.Log("[L107] Starter materials created. Assign FBX slots and validate your render pipeline.");
        }
    }
}
#endif
```
