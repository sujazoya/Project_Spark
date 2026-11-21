# PROJECT SPARK — LEVEL 115: THE SECURE ENCLAVE
## MASTER UNITY IMPLEMENTATION PROMPT — SHX-23 / COMPLETE AAA DESIGN CONTRACT

**Version:** 1.0 | **Designed:** 2026-11-20 | **Target:** Unity 6.3 URP, Windows + mobile | **Scope:** isolated board-only Level 115, no new room or physical instruments.

> Act as a senior Unity electrical-simulation, embedded firmware security, game-systems, and AAA technical-art engineer. Using the bundled `Level115_SHX23` as the source of truth for model IDs and art placement, implement Level 115 into the **existing actual** Project Spark Unity project. Inspect actual code for SparkCircuitSystem, SparkTerminal, SparkElectricalNetworkGraph, LevelGamePlayManager, inventory, virtual repair UI, save systems and existing levels. Never guess shared API signatures or overwrite Levels 1–114 to simplify Level 115. Keep electrical, cryptographic, thermal/tamper and objective authority separate from artwork. If an approved schematic or signed firmware policy is missing, create a playable-design scaffold only with clear NOT_CERTIFIABLE states. No visual animation, demo hash, debug button or static success string is gameplay evidence.

**Mission framing:** The earlier Level 114 taught thermal reliability. Level 115 demonstrates that a powered, cool, apparently functional board can still be unsafe or untrusted when its boot image, keys, rollback counter, physical tamper loop or trusted readiness feedback are invalid.

## 01. The real source inventory and provenance

The new source is independently procedural, not a duplicate of earlier boards. `Level115_SHX23/Models_OBJ` contains 240 modular OBJ and accompanying MTL files plus a separate `241_L115_AssembledReference.obj`; `Models_GLB` holds matching 241 GLBs. The 196 installation targets are exact BOM records; 44 other models cover the physical core, 22 non-conductive copper/layer inspection sheets and 21 pad, shield, tamper and diagnostic references. All meshes use **metres**; the Y-up main PCB is exactly **0.420 × 0.300 m**, and the assembled reference must remain out of gameplay. Procedural PBR assets consist of 28 named material families and 141 input textures (28 each of BaseColor, Normal, AO, Metallic and Roughness plus one 2048-pixel silkscreen reference) and 28 URP packed masks. This source kit provides realistic-style hard-surface art, not hand-authored final AAA production bakes or manufacturer-verified footprints.

Exact IDs and positions are in `Data/ModelManifest.csv`, `Data/ComponentBOM.csv` and `Data/ComponentPlacements.csv`. **Every source route and via task remains unassigned.** The source pack is not a verified circuit schematic. No Unity project compilation or Play Mode result is included.

## 02. Mission identity and goal

- Level: **115 — THE SECURE ENCLAVE**, fictional board model **SHX-23**.
- Category: Motherboard Mastery / Hardware Trust / Secure Boot and Fault Diagnosis.
- Board: 420 × 300 mm, 22 conceptual copper inspection layers (engineering stack-up not yet frozen).
- Components: **196** individually installable, shield cans included, plus **44** art-only models.
- Player-selected copper tasks: **272** route IDs W001–W272 (not the full physical trace count).
- Via tasks: **92** via IDs V01–V92.
- Safety link: **W273**, removable feedback. W273 is a component in the 196-part BOM, not a 273rd copper route.
- Three recoverable fault scenarios: secure supply glitch, intermittent boot-flash via, tamper-mesh discontinuity.
- Objectives: exactly six; all start incomplete and the trusted workload remains inhibited.
- Narrative: inspect → assemble → route → commission → authenticated boot → diagnose faults → requalify → disconnect W273 → supervised recovery.

## 03. Structural design and layer stack

Represent the entire PCB using real physical coordinates and a validated coordinate conversion at any visual-only display scale. Keep physical measurements in metres. The design proposal spans 22 individually addressable conceptual layer roles, recorded in `BoardStackup_UNVERIFIED.json`: Top Signal; Ground; Security Signals; Power A; Ground; Flash Signals; Ground; Tamper Mesh; Ground; Isolated Power; Ground; Crypto Signals; Ground; Clock and Reset; Ground; Power B; Ground; Tamper Mesh; Ground; Debug Control; Ground; Bottom Signal. Exact thickness, dielectric permittivity, copper thickness, via manufacturing methods, antipads, keepouts and return paths need approved engineering config. Do not imply the decorative planes conduct electricity. Require proper multilayer DRC, return-path validation, power isolation and anti-shorting behavior.

A suggested gameplay hierarchy (adapt after seeing project conventions):

```text
Level115_SHX23
  MissionCoordinator (project-authority adapters only)
  Board_420x300mm_RealScale
    BarePCB_Core
    22LayerArtReferences_DISABLED
    InstalledComponents (starts empty)
    ComponentFootprintAndPadTargets (196; not yet verified)
    RoutedPlayerCopper (starts empty)
    PlatedPlayerVias (starts empty)
    ShieldAndTamperTargetPoints
    W273_MonitorAndSupervisorSockets
  AuthorizedInstrumentation_WorldSpaceCanvas
    Multimeter / Oscilloscope / LogicAnalyzer / NetTracer
    BootIntegrityConsole / SignatureVerifier / FlashTester
    AttestationPanel / TamperMonitor / LayerViewer
  RuntimeSimulation / FaultController / EvidenceTracker / SaveState
  AssembledReference_EDITOR_ONLY_DISABLED
```

The shipped generator intentionally produces a visual shell and named unbound integration anchors rather than pretending to instantiate these uninspected runtime subsystems.

## 04. AAA visual production scope

Color design: dark obsidian soldermask, restrained turquoise instrument overlays, natural copper, ENIG gold pads, brushed titanium shields, matte black QFN/BGA devices, realistic ceramic passives, tiny printed reference IDs and readable macro inspection. Separate high-density chip U1, cryptographic element U2, main processor U3, signed-flash U4, four removable shield cans, golden sensor/connector contacts and shield seam points. Preserve detailed contacts when zoomed; use carefully bounded bloom, contact-localized solder-glow, micro-sparks after accepted operations, and a coherent probe/scan interaction design. Avoid cartoons, extreme emissive outlines and obscuring opaque holograms. Model budget: reusable materials, instancing of small passives, sensible LOD while macro pin readability is maintained. Produce actual performance profiling on PC/mobile rather than claiming it from triangle count.

PBR policy: texture BaseColor must be sRGB, packed metallic/smoothness must be linear, normal maps imported using NormalMap importer type; smoothness is packed in alpha as `1 - roughness`. Use URP Lit, map MTL family IDs to generated native `.mat`, preserve source texture aliases, never overwrite earlier-level materials. Automatically report any unresolved renderer slots or empty meshes. The included procedurally generated textures are reference grade and need final authored bake/artist pass before AAA approval.

## 05. Initial player state and component handling

Every installable part is in inventory and missing from the PCB: `0/196`. The board remains bare, all 272 player routes missing, all 92 vias missing, W273 OPEN, `0/6` objectives and secure authorization INHIBITED. Place using click/touch selected inventory item → hover ghost → rotation and footprint alignment → seat → solder/pad engagement → legitimate electrical simulation acceptance. Match `ref`, source `asset_id`, type, package, value, pin-one, solder contact, position and safe-state prerequisites. Never conflate a model's assembled reference pose with an approved footprint, and never award component installation from spawned decorative geometry alone.

U1/U2/U3/U4 and the shield cans have different target and inspection requirements. W273 is a removable safety-feedback component and must have a validated socket, monitor and supervisor pins. Shield-can mounting can block physical inspection of underlying components, so enforce correct assembly order without arbitrary shortcuts. Existing store, shelf, disposal and object-lifecycle behaviors must be reused.

## 06. Electrical topology and design reconciliation — hard block

Actual `SparkCircuitSystem` and `SparkTerminal` integration must use approved net IDs and physical package pins/balls. Create a reviewable typed schematic mapping `ref → package → package_pin → footprint_pad → solder_junction → net_id → copper_layer → route_segment → via → dest_pad → dest_pin` plus power-domain/return and safety roles. Source `CopperRoutes_272_REQUIRES_SCHEMATIC.csv` contains all `UNASSIGNED` source/destination pads, nets and `TBD` layers. Source `ViaTasks_92_REQUIRES_SCHEMATIC.csv` has `UNASSIGNED` nets and `TBD` layer spans. These are exactly planning records, never live conductive connections. A route must only certify after both true graph connectivity and geometric DRC pass. A via must connect only its approved plated span and must not short intersecting ground/power reference planes. Preserve W001–W272 and V01–V92 IDs.

W273 has **logical** monitor U7 and supervisor U8 in the BOM, but `W273_Recovery.json` leaves both physical pins `UNASSIGNED`. The source does **not** resolve legal pins, net, jumper orientation, or state transition timing. Do not guess pins or insert a hidden logical bypass. Mark W273 and objective 6 blocked until schematic approval and independent acceptance tests. Create a reconciliation log with original and approved records, signed revision/hash, and all downstream prefab/solver/objective mappings.

## 07. Secure power, reset, clock and trust chain

Build educational but bounded rail models for protected supply, MCU and secure-element rails, untrusted/debug domain and retained monitor power. Final voltages, startup thresholds and timing constants must be sourced from approved per-component design rather than copied from previous boards. Model separate POR/reset, clock validity, supply droop, isolation/fault latch, safe rework state and restart epoch. A transient on the secure-element rail must invalidate authentic boot; it must not silently zero unrelated diagnostic rails. Multimeter/scope readings are computed by the project’s authoritative electrical solver with timestamp, probing target and instrument revision, not decorative numbers.

Boot order: `OFF → SUPPLY_QUALIFICATION → RESET_HELD → BOOT_ROM → IMAGE_READ → DIGEST_CHECK → SIGNATURE_VERIFICATION → VERSION_POLICY → TRUST_READY` with explicit `UNTRUSTED`, `TAMPERED`, `RECOVERY`, and `SAFETY_LATCHED` exits. Every stage requires real preconditions. The functional trust manager must never be a second hidden electrical truth.

## 08. Firmware authenticity, rollback and measured boot

A correct cryptographic implementation must distinguish *hash integrity* from *digital signature authenticity*. SHA-256 equality alone is never a valid proof that firmware came from an approved signer. Build a bounded signed-image model with immutable provisioned public verification keys, approved signatures/test vectors, algorithm selection and deterministic verification results. Use safe Unity-compatible audited .NET crypto APIs where supported, or explicitly supply a tested abstraction. Never store real production private signing keys in game assets. The firmware image has a version, signer, expected digest, permitted version range/rollback counter and measured boot transcript.

Test invalid hash, corrupted image, wrong signer, expired/revoked key policy (if designed), downgrade rollback, replayed measurements and unauthorized debug mode. Measured boot records hashes and state transitions with timestamps but does not assert remote trust without verified attestation. This is an educational simulation, not a claim of FIPS, TPM, or real hardware certification.

## 09. Security attestation and freshness

Represent an independent trust monitor, boot event log, and challenge/response exchange. After valid boot, accept a fresh unpredictable or seeded test nonce and require a new response bound to that nonce, current board revision, firmware version, boot epoch, tamper status and key-policy revision. A recorded response to an old nonce, a constant HIGH signal, or mere green indicator may not qualify. Use a deterministic seeded test mode for QA while maintaining proper educational explanation of why genuine attestation uses unpredictable challenges in production. Save/reload cannot relabel old evidence as fresh.

## 10. Virtual inspection tools and 3D user interaction

Reuse the existing Project Spark World-Space Virtual PCB Repair UI. Add only needed UI panels: Boot Chain Analyzer, Signed Firmware Reader, Attestation Console, Tamper Mesh Inspector, Security Event Log, Clock/Rail Scope, Layer Explorer and Trust Authorization Panel. All instruments must use actual camera hit/target IDs, layer IDs, probes and authoritative snapshots. When the player clicks a component, pin, solder point, trace, via, shield seam or W273 jumper, build a typed request with tool ID, physical target ID, safe state and current epoch. Reject wrong tool, wrong target, unsafe hot work and invalid owner. Only accepted actions play localized VFX at the actual contact point. Instrument windows must intercept pointer/touch events so clicks do not route copper behind UI.

## 11. Fault scenario F1 — secure rail glitch

Inject F1 by applying an electrical transient to the *reviewed* secure supply path, e.g. a stuck-open protection FET/contact or decoupling failure at explicitly mapped pads. Record actual transient shape in the bounded supply model, crossing the security monitor’s threshold and invalidating current secure boot. The user observes via meter/scope, isolates the supply, repairs an authorized component/contact and performs a **new cold boot plus all trust checks**. A waveform animation or textual warning alone cannot inject or repair a fault. Do not provide real-world offensive bypass procedures; keep this an educational simulation and defense/recovery lesson.

## 12. Fault scenario F2 — boot flash via intermittency

Source objective: a real approved flash-bus signal via intermittently loses continuity under deterministic modeled stress, producing missing or corrupted reads and a boot hash/signature failure. Specify the exact device/pin/net and via layer span in the approved schematic. The player uses layer viewer, multimeter/net tracer and storage/logic tools to identify the real discontinuity, repairs using authorized via rework, then repeats continuity, flash-read integrity, hash and signature tests with a **new boot epoch**. Wrong-net jumps and bridging copper are invalid repairs.

## 13. Fault scenario F3 — tamper mesh open

Select an approved shield mesh net with independent monitor and latch. F3 opens the conductive loop (not merely hides a shield mesh). The player observes independent tamper-loop continuity and security-event log, safely repairs the correct physical shield joint and verifies continuity. The safety latch does not clear automatically after a visual fix; require explicit reset and a fresh authenticated boot/attestation. Shield can geometry and contact pins must be reconciled with source art before certification.

## 14. W273 independently supervised disconnect / reconnect

Initially OPEN. The player must connect W273 for normal commissioning after its physical pins are approved and then prove objectives 1–5. W273 is **trusted-boot feedback only**, not the main cryptographic data path or a power rail. For final objective 6, arm a supervised test with a fresh previously trusted epoch; physically remove W273 and verify an actual graph open. The independent supervisor must latch `UNTRUSTED` and inhibit protected workload even if processor logic continues operating. Reattaching W273 is insufficient. Require physical continuity, new boot state, fresh signer/digest/version/rollback checks, an independent new nonce-bound attestation and explicit safety reset. Only then may the original project objective authority certify objective 6. Historical O1–O5 awards can be retained while current operational qualification is invalid.

## 15. Six objective authority contracts

| Objective | Requirement | Reject if |
|---|---|---|
| O1 – Assemble | 196 correctly placed and electrically contacted physical components from approved footprints | art overlap, wrong part, unverified solder |
| O2 – Construct | 272 approved routes and 92 vias with correct graph, layers, DRC and return paths; W273 connected for initial commissioning | any `UNASSIGNED`, wrong layer, shortcut copper |
| O3 – Commission | supplies, clock, reset, secure element and protected boot initialization all independently measured | static UI LED, invalid rail, old evidence |
| O4 – Authenticate | fresh firmware digest, authentic signature, permitted version, measured boot and new attestation | hash-only demo, untrusted key, replayed nonce |
| O5 – Repair | F1/F2/F3 actually injected, separately diagnosed and physically repaired; new boot and trust tests | VFX-only repair, missing test, cached samples |
| O6 – W273 recover | supervised physical open/inhibit, reconnect, fresh trust tests + attestation and independent reset | constant-high jumper, missing physical pins, no new epoch |

`LevelGamePlayManager` stays the *single source of truth* for objectives. Never award from script-local bool, GUI button, model renderer, animation completion or `SparkL115TrustFlowTeachingDemo`. Persist evidence IDs, fault and boot epochs, generation/revision, calibration, device/net identity, signed policy revision and measured thresholds.

## 16. Project integration and module boundaries

First inspect source and assembly definitions of Unity 6.3 URP project, then add narrow adapters. Suggested names are new design proposals, not asserted existing classes: `SparkLevel115Coordinator`, `SparkLevel115SchemaValidator`, `SparkLevel115FootprintAdapter`, `SparkLevel115ProtectedRailModel`, `SparkLevel115BootStateMachine`, `SparkLevel115FirmwareVerifier`, `SparkLevel115MeasuredBootLog`, `SparkLevel115Attestation`, `SparkLevel115TamperMonitor`, `SparkLevel115FaultController`, `SparkLevel115W273Safety`, `SparkLevel115ObjectiveEvidenceAdapter`, `SparkLevel115VirtualRepairBridge` and `SparkLevel115SaveState`. Never create a parallel `SparkCircuitSystem`; subscribe to actual topology/events. If an uninspected project API is required, leave an explicit adapter binding unbound with diagnostic instead of fabricating a method signature.

Runtime complexity: event-driven rather than heavy `Update()`, cached references, avoid repeated search/Find and LINQ per frame, bounded waveform/training samples, deterministic seeds, no allocations per pointer hit, pooled repair FX, mobile framerate profiling. Audio/voice can reuse the existing Spark AI communication system via existing alert channels, if discovered, without inventing integrations.

## 17. Editor creation, package, and asset rules

`Tools/Project Spark/Automatic Level Generator/115/3. Validate Sources and Report Blockers` checks file identity and counts and emits diagnostics (it is not an electrical validator).

`115/4. Rebuild URP PBR Materials` creates/updates the exact 28 URP Lit material families without replacing earlier-level material GUIDs.

`115/1. Build FULL Level_115_Layout` and alias `Tools/Project Spark/Levels/Create Level 115` create an isolated Unity scene and prefab with physical-scale empty board, disabled decorative layers, 196 **uninstalled** targets, 272 blocked route records, 92 blocked vias, a non-authoritative educational SHA-256 demo, project service integration anchors, an inactive reference art model and a typed `L115_SourceBlueprint.asset`. The source is idempotent and prompts to preserve unsaved Editor scenes. It does **not** bind unknown project gameplay APIs or award objectives.

`115/2. Place assembled VISUAL preview on SELECTED tabletop` places the showcase independently on existing `Table_top`, removing collider interactions; it displays art only at approximately **10.13 × 7.24 units**. The real physical mesh remains 0.420 × 0.300 m. Never use the enlarged preview in physics, simulation or measurement.

## 18. Persistence and safety case

Persist authoritative installed parts, approved copper topology, via connections, firmware version/digest/signer policy, monotonic version, boot state, tamper state, W273 graph status, objective history, instrument revisions, fault epochs and nonce generations. Recompute topology and cryptographic policy on restore before loading objective health. Retain data needed for reproducible test logs; do not persist private production signing keys or real credentials. Replayed snapshots with stale evidence may show historical milestones but never current certification. All actions simulate fictional electronics and do not control actual hardware.

## 19. Critical blockers to resolve before certifying

1. Approved physical U1–U28 package-pin and BGA pad specifications.
2. Exact 272 route source/destination pad IDs, net identities, layer, geometry and signal-class constraints.
3. Exact 92 via nets, plating technology, layer spans, drill geometry and clearance rules.
4. W273 physical pins at U7/U8, socket contact locations, safe authorization semantics, monitor heartbeat policy.
5. Identity of source U7/U8 semiconductor packages and the physical tamper shield / F1 / F2 / F3 target pins.
6. 22-layer dielectric stack-up, measured board references, impedance and power-island isolation.
7. Approved firmware image formats, signing algorithm/public keys, version policy and test-vector fixtures.
8. Actual interfaces of Project Spark electrical solver, components, inventory, UI, objective authority, save and safety.
9. Calibration / acceptable limits for protected power, clock, tamper and boot/attestation evidence.
10. Actual Unity compiler/Play Mode and earlier Levels 1–114 regression results.

**Until this design evidence exists, the created artifact is a visual authoring / integration foundation, not a complete playable level.** Resolve rather than silently invent pins, cryptographic signatures or test measurements.

## 20. Engineering acceptance tests — exactly 100 proposed checks

**All checks are proposed, none executed in Unity.** Each test must collect setup, expected result, measured actual state, evidence ID, pass/fail status, revision and any blockers. `Documentation/L115_Acceptance_100_NOT_RUN.csv` contains machine-readable records.

### Asset and Unity authoring


- **L115-AT-001** — 240 modular OBJ models import with renderer meshes. **Status: NOT_RUN**.

- **L115-AT-002** — 240 matching GLB references are present and independently inspectable. **Status: NOT_RUN**.

- **L115-AT-003** — Assembled reference never installs game components. **Status: NOT_RUN**.

- **L115-AT-004** — Main PCB measures 0.420 × 0.300 m in local physical coordinates. **Status: NOT_RUN**.

- **L115-AT-005** — Source origin and Y-up axis align to board. **Status: NOT_RUN**.

- **L115-AT-006** — UV maps are present on every modular OBJ. **Status: NOT_RUN**.

- **L115-AT-007** — 28 URP Lit material assets rebind by source identity. **Status: NOT_RUN**.

- **L115-AT-008** — Normal maps import as NormalMap, not sRGB. **Status: NOT_RUN**.

- **L115-AT-009** — Repeated one-click build never duplicates assets or root. **Status: NOT_RUN**.

- **L115-AT-010** — Existing Levels 1–114 are not modified. **Status: NOT_RUN**.



### Component placement and inventory


- **L115-AT-011** — Initial state is 0/196 components installed. **Status: NOT_RUN**.

- **L115-AT-012** — All BOM refs match unique manifest asset IDs. **Status: NOT_RUN**.

- **L115-AT-013** — U1 trusted controller can be inspected as a distinct package. **Status: NOT_RUN**.

- **L115-AT-014** — U2 secure element remains separate from U1 and U3. **Status: NOT_RUN**.

- **L115-AT-015** — Wrong component value and footprint are rejected. **Status: NOT_RUN**.

- **L115-AT-016** — Incorrect pin-one orientation does not make electrical contact. **Status: NOT_RUN**.

- **L115-AT-017** — Component installation requires legitimate inventory and slot. **Status: NOT_RUN**.

- **L115-AT-018** — Shield can is removable and not preinstalled at game start. **Status: NOT_RUN**.

- **L115-AT-019** — W273 occupies exactly one of 196 component slots. **Status: NOT_RUN**.

- **L115-AT-020** — Art-only assembled reference cannot increment placement count. **Status: NOT_RUN**.



### Copper and DRC


- **L115-AT-021** — All 272 routes require reviewed pad endpoints. **Status: NOT_RUN**.

- **L115-AT-022** — UNASSIGNED source pads block route certification. **Status: NOT_RUN**.

- **L115-AT-023** — UNASSIGNED destination pads block route certification. **Status: NOT_RUN**.

- **L115-AT-024** — Wrong-net copper is rejected as a short. **Status: NOT_RUN**.

- **L115-AT-025** — A ground reference plane is not auto-connected to all vias. **Status: NOT_RUN**.

- **L115-AT-026** — Factory visual traces never bypass player tasks. **Status: NOT_RUN**.

- **L115-AT-027** — Player trace geometry must pass actual clearance DRC. **Status: NOT_RUN**.

- **L115-AT-028** — Tamper mesh signals remain isolated from power planes. **Status: NOT_RUN**.

- **L115-AT-029** — Solderless component graphic cannot close a conductive net. **Status: NOT_RUN**.

- **L115-AT-030** — Copper route history survives edit undo without false evidence. **Status: NOT_RUN**.



### Via and stack-up


- **L115-AT-031** — All 92 via IDs are present exactly once. **Status: NOT_RUN**.

- **L115-AT-032** — TBD layer span prevents via certification. **Status: NOT_RUN**.

- **L115-AT-033** — UNASSIGNED via net prevents via certification. **Status: NOT_RUN**.

- **L115-AT-034** — Legal through via connects only specified plated layers. **Status: NOT_RUN**.

- **L115-AT-035** — Blind/buried via requires approved process and span. **Status: NOT_RUN**.

- **L115-AT-036** — Via footprint pads preserve copper keepouts. **Status: NOT_RUN**.

- **L115-AT-037** — Invalid power-to-ground via link is rejected. **Status: NOT_RUN**.

- **L115-AT-038** — Damaged F2 flash via changes actual graph continuity. **Status: NOT_RUN**.

- **L115-AT-039** — Repair after F2 requires independent continuity test. **Status: NOT_RUN**.

- **L115-AT-040** — 22-layer filter UI cannot change electrical graph itself. **Status: NOT_RUN**.



### Power and reset


- **L115-AT-041** — Cold secure boot refuses unstable trust rail. **Status: NOT_RUN**.

- **L115-AT-042** — Protected supply brownout invalidates current boot epoch. **Status: NOT_RUN**.

- **L115-AT-043** — Secure element reset is distinct from processor reset. **Status: NOT_RUN**.

- **L115-AT-044** — Clock instability prevents authenticated boot. **Status: NOT_RUN**.

- **L115-AT-045** — Voltage glitch changes measured rail state, not just UI. **Status: NOT_RUN**.

- **L115-AT-046** — Independent supervisor can inhibit protected operations. **Status: NOT_RUN**.

- **L115-AT-047** — Boot power is never sourced by art-only decals. **Status: NOT_RUN**.

- **L115-AT-048** — Probe voltage is returned from the live solver. **Status: NOT_RUN**.

- **L115-AT-049** — Safe rework requires simulated isolation. **Status: NOT_RUN**.

- **L115-AT-050** — Power restoration requires fresh boot evidence. **Status: NOT_RUN**.



### Cryptography and trust


- **L115-AT-051** — SHA-256 educational digest matches known test vector. **Status: NOT_RUN**.

- **L115-AT-052** — Hash equality alone does not certify signature authenticity. **Status: NOT_RUN**.

- **L115-AT-053** — Untrusted firmware signature is rejected by approved key. **Status: NOT_RUN**.

- **L115-AT-054** — Valid signed image requires version and policy checks. **Status: NOT_RUN**.

- **L115-AT-055** — Rollback to disallowed version blocks authorization. **Status: NOT_RUN**.

- **L115-AT-056** — Revoked signer blocks a previously valid image. **Status: NOT_RUN**.

- **L115-AT-057** — Firmware modification invalidates digest and signature. **Status: NOT_RUN**.

- **L115-AT-058** — Attestation challenge nonce is never silently reused. **Status: NOT_RUN**.

- **L115-AT-059** — Stale successful boot evidence cannot satisfy new epoch. **Status: NOT_RUN**.

- **L115-AT-060** — Real digital signature verification requires approved test vectors. **Status: NOT_RUN**.



### Tamper and security diagnostics


- **L115-AT-061** — F1 secure-rail glitch reliably invalidates trust. **Status: NOT_RUN**.

- **L115-AT-062** — Tamper mesh continuity depends on verified physical net. **Status: NOT_RUN**.

- **L115-AT-063** — F3 shield-open fault latches independent security event. **Status: NOT_RUN**.

- **L115-AT-064** — Restoring visual shield alone does not clear tamper latch. **Status: NOT_RUN**.

- **L115-AT-065** — Temperature/voltage anomaly is observable on instruments. **Status: NOT_RUN**.

- **L115-AT-066** — Tamper sensor wrong target is rejected. **Status: NOT_RUN**.

- **L115-AT-067** — Side-channel teaching graph is marked illustrative unless measured. **Status: NOT_RUN**.

- **L115-AT-068** — Debug lock policy is distinct from graphical lock icon. **Status: NOT_RUN**.

- **L115-AT-069** — Trusted counter cannot be reset by replaying UI. **Status: NOT_RUN**.

- **L115-AT-070** — Tamper evidence stores timestamp and fault epoch. **Status: NOT_RUN**.



### Fault repair and requalification


- **L115-AT-071** — F1 requires actual qualified power-path repair. **Status: NOT_RUN**.

- **L115-AT-072** — F1 new boot from cold start is required. **Status: NOT_RUN**.

- **L115-AT-073** — F2 modifies flash bus in real topology. **Status: NOT_RUN**.

- **L115-AT-074** — F2 correct repair requires fresh read and signature tests. **Status: NOT_RUN**.

- **L115-AT-075** — F3 changes physically assigned tamper loop. **Status: NOT_RUN**.

- **L115-AT-076** — F3 qualified repair restores continuity but latch remains. **Status: NOT_RUN**.

- **L115-AT-077** — Repair VFX cannot mutate solver by itself. **Status: NOT_RUN**.

- **L115-AT-078** — Wrong-tool repair attempt preserves existing fault. **Status: NOT_RUN**.

- **L115-AT-079** — All three repaired faults require new independent evidence. **Status: NOT_RUN**.

- **L115-AT-080** — Fault controller uses deterministic repeatable events. **Status: NOT_RUN**.



### W273 safety cycle


- **L115-AT-081** — W273 starts physically OPEN. **Status: NOT_RUN**.

- **L115-AT-082** — U7/U8 logical role mapping without actual pins blocks certification. **Status: NOT_RUN**.

- **L115-AT-083** — Valid W273 connection for initial commissioning is graph-backed. **Status: NOT_RUN**.

- **L115-AT-084** — After O1–O5, supervised W273 removal opens a real edge. **Status: NOT_RUN**.

- **L115-AT-085** — Independent safety latch inhibits trust-dependent operations. **Status: NOT_RUN**.

- **L115-AT-086** — Diagnostic instrumentation remains available after inhibition. **Status: NOT_RUN**.

- **L115-AT-087** — Reconnection alone does not clear an existing latch. **Status: NOT_RUN**.

- **L115-AT-088** — Fresh firmware verification and new nonce are required. **Status: NOT_RUN**.

- **L115-AT-089** — Explicit reset is rejected until fresh trust state qualifies. **Status: NOT_RUN**.

- **L115-AT-090** — Objective 6 requires real post-disconnect trust requalification. **Status: NOT_RUN**.



### Objectives, input, persistence, regression


- **L115-AT-091** — Initial objective count is 0/6. **Status: NOT_RUN**.

- **L115-AT-092** — LevelGamePlayManager is sole authority for six objectives. **Status: NOT_RUN**.

- **L115-AT-093** — Canvas overlay does not intercept 3D hits unintentionally. **Status: NOT_RUN**.

- **L115-AT-094** — Mouse and touch produce identical typed target IDs. **Status: NOT_RUN**.

- **L115-AT-095** — Save/reload does not recycle old qualified boot evidence. **Status: NOT_RUN**.

- **L115-AT-096** — Visual materials cannot write mission completion state. **Status: NOT_RUN**.

- **L115-AT-097** — Generated scene contains no added room or physical tools. **Status: NOT_RUN**.

- **L115-AT-098** — Editor output contains all 196 component visual prefabs. **Status: NOT_RUN**.

- **L115-AT-099** — Unity Play Mode completes after approved schema and tests. **Status: NOT_RUN**.

- **L115-AT-100** — No regression in Levels 1–114 under project CI. **Status: NOT_RUN**.



## 21. Definition of done and evidence handoff

Do **not** claim fully playable, AAA, secure-boot-certified or 6/6 until the actual project demonstrates: Unity 6.3 Editor import, material slot validation, physical mesh scale, all 196 footprint placements and solder effects, 272/92 approved real electrical routes/vias, verified boot using trusted signed test vectors, fresh nonce-based attestation, three faults with independent repairs, W273 true disconnect/inhibit/recovery, no stale-save bypass, all 100 acceptance tests with actual logs, and Levels 1–114 smoke checks. Include exact changed file list, assembly/namespace references, schematic signature/revision, test pass/fail/block results and real captures. Treat any missing evidence as **PARTIAL IMPLEMENTATION**, never as passing. Avoid changing earlier scenes or existing shared gameplay behavior.

**Mission tagline: VERIFY. MEASURE. ATTEST. RECOVER.**
