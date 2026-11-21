# PROJECT SPARK — LEVEL 120: THE AXIS FORGE (RAX-28)
## MASTER AAA-STYLE UNITY 6.3 URP IMPLEMENTATION PROMPT — COMPLETE MAINBOARD

**Source package:** `ProjectSpark_Level120_RAX28_FULL_AAA_Original_VisualKit.zip`  
**Title:** THE AXIS FORGE  
**Mission:** A six-axis robotics controller for a virtual manipulator with synchronized motion, independently verified encoders, shared power, dual STO channels and critical W353 safety feedback.  
**Physical PCB:** 520 × 380 × 2.2 mm, 32 inspection layers, metres in OBJ/GLB, Unity Y-up.  
**Visual inventory:** 320 independent OBJ/GLB pairs (264 components + 56 structural/layer parts), one **fully assembled mainboard** as pair `321_L120_AssembledReference.obj/.glb`, 38 PBR families with source and packed URP maps.  
**Circuit task design targets:** 352 route tasks, 132 via tasks, W353 removable link, three recoverable faults and six objective contracts.  
**Current authority:** NO signed electronic schematic, verified footprints, robot mechanical parameters or safety control ratings are supplied. The finished 3D motherboard is a **visual and authoring deliverable**, not an electrically functional or safely certified robot controller. All objectives remain blocked.

> **To the implementing agent (Codex):** Act as a senior Unity 6.3 URP gameplay engineer, six-axis robotics simulation engineer, motor-control systems engineer, technical artist, and test engineer. Inspect the **actual** Project Spark source APIs before adding connections to SparkTerminal, SparkCircuitSystem, SparkElectricalSolveContext, SparkSelectionController, LevelGamePlayManager, inventory/shop, repair tools or UI. Never invent APIs, conductive traces, motor ratings, approved pins, safe-stopping times or guaranteed success. Preserve Levels 1–119. Work additively inside Level 120 namespaces and paths. Clearly separate verified gameplay from illustrative 3D/showcase artwork.

## 01 — What is physically delivered

- A new detailed **main RAX-28 assembled visual motherboard**: `Models_GLB/321_L120_AssembledReference.glb`, `Models_OBJ/321_L120_AssembledReference.obj` with its `.mtl`, all separate 264 parts, and **six cooling heatsinks** and structural artwork.
- A bare `265_L120_PCB_Main.obj/.glb` physical-scale PCB (not a fake Unity primitive), six named axis power-stage zones, and 32 independent layer-inspection reference models which remain **disabled** by default.
- One model and exact asset ID for every one of the 264 installable components, with stable BOM/placement joins, matching independent OBJ and GLB files, source PBR materials and UVs.
- The complete board model is **never** the gameplay assembly. Unity creates `L120_CompleteMotherboard_VISUAL_ONLY.prefab` and keeps that separate from the empty gameplay `Level_120_Layout.prefab`/`.unity` scene.
- All complete motherboard geometry is generated from the original modular geometry; no Unity Shop or Placement flag changes because the showcase is instantiated.

### Art and material requirements

Use URP/Lit, 38 named families, albedo sRGB, tangent-space normal-map import, AO linear, metallic R and smoothness A in packed RGBA masks, with reasonable physically based reflectance. No major graphics-pipeline migration, and do not add a whole room, separate instruments or external robot model. The 3D kit is procedural, not a hand-finished high-poly commercial art set; profile mesh and draw calls and selectively improve visible connectors, silk, heat-sink fins, orientation markers and LODs. Mark copper-layer visuals as *illustrative*, not connectivity.

## 02 — Exact immutable source contracts

Read these files from `Assets/ProjectSpark/Level120/RAX28/Data` (or uniquely discovered matching source root):

| File | Expected records | Authority |
|---|---:|---|
| `ModelManifest.csv` | 320 | 264 installable + 56 other source meshes |
| `ComponentBOM.csv` | 264 | Design-stage part identities; values/footprints not verified |
| `ComponentPlacements.csv` | 264 | Visual anchors only; all initially `FALSE` for installed/verified |
| `CopperRoutes_352_REQUIRES_SCHEMATIC.csv` | 352 | Both pads/net IDs `UNASSIGNED`, certifiable false |
| `ViaTasks_132_REQUIRES_SCHEMATIC.csv` | 132 | Net `UNASSIGNED`, layer spans `TBD`, verified false |
| `W353_Recovery.json` | 1 | Initially OPEN, two logical endpoints, actual physical pins unassigned |

BOM contains one W353 jumper as part of 264; never add it as a 265th. Model manifest `obj` column is a valid exact filename and **not** `NOT_INCLUDED`. There are **321** OBJ and **321** GLB files including assembled showcase, not 320. Source root duplicates are an error, not a reason to select arbitrary art. Avoid the prior Level 112/114 model-path/count bugs.

Use `SOURCE_MANIFEST_SHA256.txt` to detect source revisions. Keep old `.meta` asset IDs stable where possible. Refuse overwriting user-modified original source art or previous levels. Build scripts must not leave open Unity scenes unsaved or accidentally regenerate older level assets.

## 03 — 3D motherboard organization and visual presentation

Real-size board X = **0.520 m**, Z = **0.380 m**, substrate Y = **0.0022 m**. Six motor-control zones are arranged in rows along the right edge with high-side/low-side MOSFET groups, visual thermal sinks, and corresponding right-side connectors J1 through J6. The control cluster, robot trajectory processors, clocking and dual-STO supervision occupy protected left/central areas. A separate world-space holographic panel exposes joint status, position errors, encoder quality, safety state, voltage/current telemetry, and fault evidence.

**Recommended generated Unity hierarchy:**

```text
L120_RAX28_THE_AXIS_FORGE
  VisualArtRoot_Metres
    EmptyPCB_Physical_520x380mm
    32_InspectionLayerReferences_DISABLED
    OtherUnverifiedFactoryArtwork_DISABLED
  GameplayAuthoringRoot_NONCERTIFIABLE
    264_UninstalledComponentTargets
    352_Routes_UNASSIGNED_BLOCKED
    132_Vias_UNASSIGNED_BLOCKED
    SixAxisDriveAuthoringZones_UNBOUND
      Axis_A_PowerStage_UNBOUND
      Axis_B_PowerStage_UNBOUND
      Axis_C_PowerStage_UNBOUND
      Axis_D_PowerStage_UNBOUND
      Axis_E_PowerStage_UNBOUND
      Axis_F_PowerStage_UNBOUND
    InventoryPrefabs_AsAssets_NotYetInstalled
  ProjectSparkSystemBindings_REQUIRED_UNBOUND
  OPTIONAL_SixAxisRobotMotion_Teaching_DEMO_NONCERTIFIABLE
  Assembled_ART_REFERENCE_DISABLED_NEVER_GAMEPLAY
```

**Separate prefab:** `Assets/ProjectSpark/AutoGeneratedLevels/Level120/L120_CompleteMotherboard_VISUAL_ONLY.prefab` contains full geometry (all parts + structural thermal art) and must never generate real circuit topology, objective counters or installed-state flags.

## 04 — Player experience and level phases

**Phase A / Audit and assembly:** Start from bare board, six separate power-stage areas and part tray. The player identifies the six axes, builds 264 components, aligns package pin-one and pad/footprint orientations and validates solder contacts. Wrong ref/footprint/value must fail.

**Phase B / Multilayer PCB construction:** Expose 32-layer board views and exactly 352 intended user copper actions plus 132 vias. These are selected educational tasks, not the entire physical copper required by a real robotics board. Route tasks only pass through actual correct pads/net IDs and a credible solver graph update after schematic approval. Via spans must be validated; a via must never be treated as a shortcut connecting all 32 layers.

**Phase C / Power and safety commissioning:** Qualify protected power input, isolated gate supplies, common DC link, return continuity, separate STO A and STO B, encoder communication and motor power stage preconditions. It is invalid to infer motor-enable from a lit LED or a closed showroom-art jumper.

**Phase D / Six-axis robot control:** Home six joints, independently validate encoder zero, test bounded joint-space and Cartesian trajectories, run forward/inverse kinematics and coordinated interpolation with speed/torque/temperature/acceleration limits. Virtual object collision or forbidden workspace must stop motion according to approved educational policy.

**Phase E / Diagnosis and repair:** Stage F1/F2/F3 only after healthy baseline qualification. Use existing world-space virtual multimeter, oscilloscope, net tracer, thermal monitor and safe robot-motion telemetry; click/touch targets require exact approved IDs and actual solver mutation. Repeat fresh tests following each repair.

**Phase F / W353 safety test:** After the earlier goals qualify, open trusted ready feedback on the independent U47→U48 logical path, demonstrate protected motion inhibit, repair actual terminal continuity, rerun six-axis current/encoder/STO/trajectory qualification, then explicitly reset and obtain independent new safety authorization. W353 **must not** pretend to remove physical supply rails or silently erase prior diagnostic history.

## 05 — Six objective contracts

| Objective | Positive evidence required | Blockers |
|---|---|---|
| O1 — Precision Assembly | 264 correct parts individually installed, orientation/footprints/solder measured | All supplied footprints unverified |
| O2 — 32-Layer Routing | 352 correct pad-level copper actions and 132 verified legal vias with electrical graph changes | All source nets UNASSIGNED and spans TBD |
| O3 — Six-Axis Commissioning | Qualified protected DC link, six motor stages, six independent encoder channels, both STO states and proper reset | Schematic, motor ratings and actual terminals absent |
| O4 — Coordinated Motion | Fresh homing, valid forward/inverse kinematics, bounded six-axis synchronized path, measured tracking error and protection behavior | Robot mechanics, feedback and collision constraints unapproved |
| O5 — Hardware Fault Recovery | Real F1/F2/F3 state changes and fresh independent retesting of all affected systems | No approved fault pins/contacts |
| O6 — W353 Safety Recovery | Recorded disconnection, independent motion inhibition, physical reconnect, fresh full qualification and explicit reset | U47/U48 physical pin IDs unresolved |

Start **0/264 components, 0/352 routes, 0/132 vias, W353 OPEN, six axes disabled, 0/6 objectives**. Keep progress history separate from current electrical health and evidence freshness. Only the actual existing LevelGamePlayManager authority may award objectives, through a reviewed explicit adapter, never by setting UI text or by attaching a model prefab.

## 06 — Six independent motor and encoder channels

Define explicit axis labels A–F. Treat each axis as a distinct control/electrical/mechanical subsystem: phase currents U/V/W, gate-driver health, motor current limits, rotor electrical angle, encoder count/offset, torque constant, back EMF, inertia, viscous and static friction, joint position, gear ratio, backlash if modeled, hard-limit range, thermal state, and current protection fault. Each parameter must come from approved educational configuration and be unit tagged. Do not infer a motor or its ratings from a Qx mesh. Use bounded deterministic control updates, cache static references, separate time steps for solver vs renderer and user UI, and avoid heavy `Update()` or `Find()` loops.

Produce joint-space coordinates and explicit robot kinematics with tested forward/inverse mapping, consistent rotation convention, known home pose and axis pivot transforms. Reject singular/unreachable poses or return a diagnostic failure; never fake a valid solution. Braking and safe-stop policies must not be advertised as certified machine safety / industrial standard compliance.

## 07 — Power, thermal, EMI and protection model

Implement actual protected DC-link qualification; independent gate supply checks; valid ground/reference paths; up to six parallel motor-stage loads; staged precharge; current/temperature derates; energy-balance under multiaxis regeneration; and modeled safe state on undervoltage, overcurrent, overtemperature, collision limit or encoder plausibility loss. In a real design, dual STO hardware behavior depends on rated safety architecture. This is a **bounded educational simulation**, not real-life safety certification. Observe the actual `SparkCircuitSystem` and measurement API before connecting any new service.

## 08 — Three recoverable faults (physical mapping blocked until approved)

- **F1 — Axis C phase-B high-side gate path OPEN:** model a disconnected approved gate-driver or MOSFET control contact; axis C torque and phase-current evidence diverge. The suggested candidates U18 / Q13–Q18 are **not approved pins**. Real repair must change topology and require a new six-axis path test.
- **F2 — Axis D encoder differential via intermittency:** introduce deterministic intermittent opens at a specified approved plated via. Corrupt that axis' independent position feedback, propagate plausibility and safe-motion reactions, and forbid UI-only repair. Via/net/layer identity is `UNASSIGNED`/`TBD` until reconciled.
- **F3 — Axis F thermal-sensor solder intermittency:** corrupt or remove a real qualified temperature/telemetry path, forcing thermal plausibility gating or derating. Sensors U34/SNx are candidates only, with physical pins `UNASSIGNED`. Validate restored contact and fresh thermal/current evidence before clearing.

All fault triggers must be stateful and replayable. Log fault ID, target, tool, topology revision, evidence generation, elapsed time and outcome. Failure must visibly affect source-derived state, not just tinting a model or playing sparks.

## 09 — W353 safety feedback truth

Source JSON: `W353_Recovery.json` uses logical `L120_U47.TRUSTED_SIX_AXIS_READY_OUT` → `L120_U48.TRUSTED_SIX_AXIS_READY_IN`. Physical pins are unassigned. Do **not** bind those labels to fictional `SparkTerminal` instances. Need actual authorized signal routing before W353 can affect real controller safety. When bound, initial open state blocks protected enable and valid prior closed→open transition latches a disconnection event. A static high signal, stale test report, reconnected visual jumper, or unrelated energized rail must not grant authorization.

## 10 — Virtual repair HUD, sound and UX

Reuse existing world-space virtual instruments and Project Spark tool lifecycle. The interface should offer: axis tabs A–F, dark laboratory HUD, highly legible numeric measurement typography, six mini phase-current scopes, encoder-quality timeline, real-time pose trace, safety dual-channel health, six-lane heat map, four-dimensional inspection modes (world, layer, net, fault), contextual tooltips and focus highlights. Illustrative waveforms must show `DEMO / NON-CERTIFIABLE`. Feedback priority: safety critical overrides decorative VFX; no extra physical oscilloscope, robot arm, lab room or meter props in this board-only kit. Mouse and touch must correctly prioritize existing input and UI interactions.

## 11 — Unity editor automation and preservation

Menus (actual package):

```text
Tools/Project Spark/Automatic Level Generator/120/3. Validate Sources and Report Blockers
Tools/Project Spark/Automatic Level Generator/120/4. Rebuild URP PBR Materials
Tools/Project Spark/Automatic Level Generator/120/5. Create COMPLETE assembled motherboard (VISUAL ONLY)
Tools/Project Spark/Automatic Level Generator/120/1. Build FULL Level_120_Layout
Tools/Project Spark/Automatic Level Generator/120/2. Place assembled VISUAL preview on SELECTED tabletop
Tools/Project Spark/Levels/Create Level 120
```

Builder requirements: discover the one real source root by `Data/ModelManifest.csv`, validate exact OBJ/GLB names, exact 320/264/264/352/132 row counts, verify main physical dimensions and importable OBJ mesh, create **264 empty slots** but not installed parts, persist blocked routes/vias in SourceBlueprint, create 264 distinct uninstalled visual prefabs, build isolated scene/prefab assets, create separately complete assembled motherboard prefab, never duplicate roots or edit older levels, preserve open modified Unity scenes with explicit save confirmation. Report remaining blockers and all test statuses. If user project has bespoke asmdefs, update assembly references after inspecting them, rather than guessing methods.

## 12 — QA, regression and final handoff

Use 100 case matrix `Tests/L120_AcceptanceTests_100.csv`. Do not mark test PASS solely from CSV existence or string-checking C#. Run Unity 6.3 compilation, EditMode tests, PlayMode tests, render/material checks, click/touch verification, save/load round trip, fault injection/recovery repeatability, and regression smoke tests of Levels 1–119. Mark individual tests PASS/FAIL/BLOCKED with actual logs/evidence timestamps. Provide a per-objective source explanation and a fresh schematic and mechanical-data revision hash. If source remains unapproved, final report must declare: **BLOCKED: UNSIGNED ELECTRICAL SCHEMATIC AND SIX-AXIS ROBOT MODEL**. Never state 6/6 or functional safety certified without independent evidence.

**DELIVERABLE SUMMARY:** New original **complete assembled visual mainboard** (real 3D assets + Unity showcase prefab), separate empty gameplay level authoring scene, modular 264 component models and 56 structural/layer assets, 38 PBR material families, signed-source gates, 100-test QA plan and installation script. Full electrical and robotic gameplay remains a separate integration and verification milestone.
