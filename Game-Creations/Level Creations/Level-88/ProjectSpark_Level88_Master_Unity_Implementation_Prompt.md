# PROJECT SPARK — LEVEL 88 MASTER UNITY IMPLEMENTATION PROMPT

## LEVEL 88 — THE BREATHING RING

**Advanced AAA Habitat Atmosphere Control • Four-Compartment Gas Dynamics • Dual CO₂ Regeneration • Finite Water-Fed Oxygen Production • Humidity and Water Recovery • Pressure/Flow/Temperature Control • Independent Life-Support Safety • Three Real Process Disturbances • Critical W145 Requalification**

> **SOURCE-GROUNDED IMPLEMENTATION SPECIFICATION.** This document uses the actual `ProjectSpark_Level88_HD_AAA_Models.zip` manifest, CSVs, scripts, W145 recovery plan and validation report. The ZIP provides procedural mesh art, preview textures and *conceptual* system plans; it is **not** a compiled/playable Unity level. Exact asset IDs and planned routes below are extracted from those files. Dynamics, solver architecture, thresholds, tests and runtime objects are **requirements to implement**, not present features.

# 01. PRIMARY MANDATE

Act as my **senior Unity C# gameplay engineer, Project Spark electrical simulation integration architect, spacecraft Environmental Control and Life Support System (ECLSS) engineer, multiphase process and mass-balance simulation engineer, instrumentation/calibration specialist, closed-loop control and PLC safety engineer, fluid systems designer, technical gameplay designer, AAA environment technical artist, Unity Editor automation developer and QA engineer**.

**BUILD THE COMPLETE, FUNCTIONAL, INTERACTIVE LEVEL 88 — THE BREATHING RING — INSIDE MY EXISTING PROJECT SPARK UNITY GAME.** The art is already supplied in `ProjectSpark_Level88_HD_AAA_Models.zip`; **import those actual FBX models**, use the package's manifest and do not recreate/replace the visual models unless real defects must be corrected. Connect new gameplay to the real Project Spark APIs discovered in source. Preserve all existing Levels 1–87.

Deliver **164 real player-created electrical connections**, 242 original decorative art sockets mapped into **328 independent proposed logical electrical endpoint contacts**, four-compartment mole/thermal gas simulation, protected educational 5 V controls, authentic finite oxygen/CO₂/water/energy processing, ventilation and valves with measured motion, three independently recoverable failures, physically removable **W145**, six authoritative objective certificates, SCADA, one-click idempotent Editor builder, tests and truthful results. Only the real `LevelGamePlayManager` may complete **6/6**.

# 02. READ THESE EXACT FILES BEFORE TOUCHING PROJECT CODE

Open the supplied ZIP and inspect rather than relying only on this prose:

- `Model_Manifest.json`, `Art_Assembly.csv`, `Component_Roles.csv`, `README_Unity.txt`, `Verification_Report.json`.
- `Animation_Rigs.csv` (**72 actual named pivots** with owner, type, axis and XYZ metre coordinates).
- `Gameplay_Wiring_Concept_W001_W164.csv` and `Gameplay_Wiring_Budget.csv` (**164 planned wire IDs; none connected at source**).
- `Terminal_Anchor_Plan_242.csv`, `Visual_Terminal_Sockets_242.csv` (**242 art-only decorative socket IDs**).
- `Terminal_Logical_Port_Reconciliation_328.csv` (**328 independent proposed source/target contacts**) and `Terminal_Anchor_Signal_Conflict_Report.csv` (**108 reused art-anchor records**, including mixed signal classifications).
- `Objective_Certification_Plan_6.csv`, `W145_Critical_Cabin_Pressure_Recovery.md`, `Physical_Process_Flow_Concept.csv`, `Gameplay_Integration_Plan.md`.
- `Material_Palette.json`, `Texture_Manifest.json`, `AAA_Polish_And_Lighting_Plan.md`, `Collision_LOD_Production_Plan.csv`, `Visual_Rig_Integration_Notes.md`.
- `Unity_Optional/SparkLevel88ArtPlacementEditor.cs`, `SparkLevel88MaterialUpgradeEditor.cs`, `SparkLevel88AssetValidatorEditor.cs` and `SparkLevel88VisualRig.cs`.

**Annexes A–K** reproduce the exact manifest model filenames, full pivot table, 164 wire routes, 242 art anchors, 328 logical port records, 108 conflicts, six original objectives, nearby safety rows, 13 process-flow design links and asset/tool metadata. If the archive and this document differ, investigate the actual imported asset structure first. Do not infer existing runtime API methods from an earlier prompt.

# 03. VERIFIED ASSET BASELINE — ART IS NOT GAMEPLAY

The source manifest reports **90** modular binary FBX 7400 models and **90** GLBs, plus an assembled FBX/GLB preview pair; **184,224 equipment triangles** in **1,219 mesh parts**, **72 actual mesh-owning pivot parents**, **242 modeled decorative visual sockets**, **164 concept-only wire definitions**, **328 proposed independent logical endpoints**, **108 reused visual socket records**, and **six objective planning records**.

The source art bounds are **min (-15.4, -0.39, -14.006) m; max (15.4, 13.98959, 12.2) m** (about **30.8 m wide** and **14.38 m tall**). Materials: **12 sets / 36 procedural 1024×1024 PNG maps**. The structural report states **27/27 package/geometry checks passed**, but `unity_compilation_tested=false`, `unity_playmode_tested=false`, `actual_player_wires_included=false`, `functional_sparkterminals_included=false`, `real_atmosphere_gas_solver_included=false`, `runtime_safety_plc_included=false` and `six_runtime_objectives_implemented=false`.

Never present mesh/socket geometry as a playable electrical circuit or finished physical ECLSS system. The 36 maps are material *references* that still need UV and production shader authoring; automatic LODs/collisions and bespoke finished VFX are not present. All six objectives have `Implemented=NO` in the source CSV. Never claim Unity Play Mode, spaceflight certification, clinical atmosphere safety or existing 6/6.

# 04. INSPECT THE ACTUAL PROJECT SPARK CODE AND SIGNATURES

Find these types and related working code where they exist: `SparkElectricalSolver`, `SparkCircuitSystem`, `SparkElectricalComponent`, `SparkTerminal`, `SparkSwitch`, `SparkLevelDefinition`, `SparkLevelEvaluator`, `SparkLevelSceneBindings`, `SparkLevelWorkstationBindingBridge`, `SparkShelfSlot`, `SparkObjectShelf`, `SparkLevelLayoutLoader`, `SparkLevel`, `LevelGamePlayManager`, existing shop/item registries, real wire connection events and disconnection implementations.

Determine *real* namespaces, power/return semantics, analog and digital signal constraints, registered sockets, named terminal allocation, wire acceptance/rejection, physical wire removal, circuit-state sampling, objective schema, scene binding, progression and persistence. Reuse supported components from prior levels when present; otherwise add minimal testable adapters and new level-local scripts. Read real Level 87 implementation rather than assuming its written design describes working code. **No speculative methods or fake calls to existing managers.**

# 05. ABSOLUTE IMPLEMENTATION RULES

1. Preserve Levels 1–87. Avoid destructive global prefab or class replacement; level-local identifiers and safe migrations only.
2. All 164 puzzle wires must be **player-created real Spark graph edges** and actually influence controller/measurement behavior. Do not start them connected.
3. The 242 `SOCKET_P###` visible meshes are art, **not existing SparkTerminal components**. Reconcile 328 logical source/target endpoints without cross-domain shorting.
4. An endpoint marked `POWER_CONTROL` must not become an analog pressure node, `GROUND_RETURN` must not become 5 V, and `SAFETY_INTERLOCK` must not be casually bused with telemetry.
5. Keep educational **5 V control/puzzle wiring**, **fictional process gas/water/power and thermal state**, and **independent safety protection** in separate coherent domains.
6. Gas species, mass/moles, water inventory, oxygen inventory, adsorbed CO₂, energy and thermal balance must be conserved with explicit bounded losses; no gas/energy creation by button press.
7. Actual valve/damper/fan/pump states respond with finite speed/inertia/constraints; requested command is not measured physical position.
8. A displayed oxygen % cannot substitute for O₂ **partial pressure**; CO₂ sensing, total pressure, humidity and temperature each have distinct safety meaning.
9. No zero-load exploit: four compartments must meet required occupancy/airflow/thermal/mission duty during tests. Scrubbers, oxygen plant and fans consume energy/resources.
10. Both adsorption beds have finite capture capacity, realistic breakthrough proxy and finite regeneration cost/time; alternating bed states must not magically dispose of CO₂.
11. Electrolyzer consumes finite water and electrical energy, preserves separated byproduct inventory/isolated path, and never generates oxygen from nothing.
12. If pressure is truly falling due to a leak, the PLC cannot mark atmosphere safe merely by restoring its feedback cable. It must verify real physical recovery.
13. Missing W145 is *invalid measurement*, **not** zero cabin pressure; normal regulation permission must withdraw while an independent relief/backup protective path remains effective.
14. W145 may not be bypassed by SCADA cache, independent sensor telemetry, W145 health display, reset command or a synthetic last-good sample.
15. Historical Objectives 1–5 persist when the final planned W145 fault occurs; Objective 6 needs *fresh* post-repair full-cycle evidence.
16. The visual rig, indicator lights, display graphs and shader colors consume authoritative state; never drive real atmosphere/safety/objectives from transforms or UI.
17. Use fictional game-scale limits, not real spaceflight human-safety, life-support operating recipes or mission certification claims. Clearly label defaults as scenario settings.
18. Correctly report missing tools, project compilation failures, bad data, absent production UVs/LOD and unexecuted tests. **Never invent PASS.**

# 06. LEVEL CONFIGURATION AND INITIAL 0/6 STATE

Create `level_88`, display name **The Breathing Ring**, subtitle **Autonomous Life Support, Atmospheric Regeneration and Pressure Safety**, expert/master difficulty. Six sequential but historically preserved objectives; real `LevelGamePlayManager` authority. Fictional 4-compartment habitat: A/Crew, B/Research, C/Garden, D/Habitat, with a shared process duct and sector dampers.

Opening state: art assembled; player control wires disconnected; independent emergency isolation available; ventilation not commissioned; both CO₂ adsorption beds idle/unqualified; oxygen generator isolated; condensate recovery idle; pressure/gas sensors staged but not calibrated; four compartment inventories initialized to sensible, **fictional** starting conditions with a meaningful deteriorating-demand challenge. Do not initially assert lethal/unrecoverable air conditions; treat the scenario as a safeguarded training simulator with no real astronauts exposed. Goal 0/6.

Use configurable game-time presets such as **50 s low-demand → 45 s peak-demand → 55 s recovery** (example only). Default limits for four zones, O₂ partial pressure, CO₂ partial pressure, total pressure, dewpoint/humidity, circulation, water reserve and temperature are *fictional adjustable acceptance envelopes*, not medical or spacecraft safety standards. Ensure enough resource capacity to support solvable player actions and discernible faults.

# 07. ART IMPORT, GEOMETRY UNITS AND COMMON-ORIGIN ASSEMBLY

Import files `Models/00_L88_...fbx` through `Models/89_L88_...fbx`; use corresponding `GLB/` for reference/alternate interchange. Source FBX **numeric geometry is in centimetres**, `UnitScaleFactor=1`; GLB geometry is in **metres**. Unity should show **1 unit = 1 metre** after converting FBX cm→m **once**. Y up, X width, Z forward.

All 90 modular models are authored in a **shared world-origin assembly frame**. Place identity-local-transform instances under `L88_HD_VISUAL_ART` beneath the level layout; do not center each model individually or add model-specific offsets. Verify approximately 30.8 m X span and 14.38 m Y span after import (account for transparent/particle extensions). The standalone `Models/Level88_Complete_ArtKit_Showcase.fbx` and matching GLB are alternative assembled previews and **must not be instantiated with the 90 modules**.

Keep actual gameplay solver, `SparkTerminal` children, controller interfaces, colliders, hit targets, switch actions, saved state, objective tracker and custom authored gameplay **outside** the replaceable art-only hierarchy. Make art replacement undoable and idempotent. Avoid double scaling, mirrored FBX import, pivot detachment or exporting all gameplay wiring with the showcase.

# 08. SUPPLIED UNITY HELPER MENUS — ART TOOLS ONLY

Shipped namespace: `ProjectSpark.EditorTools`. Exact installed helpers:

- `SparkLevel88ArtPlacementEditor`: `Tools/Project Spark/Levels/88/Place HD Art Under Selected Layout`.
- `SparkLevel88MaterialUpgradeEditor`: `Tools/Project Spark/Levels/88/Create & Apply HD Materials To Selected Art`.
- `SparkLevel88AssetValidatorEditor`: `Tools/Project Spark/Levels/88/Validate Imported HD Art`.
- Runtime `ProjectSpark.Visuals.SparkLevel88VisualRig`: actual-state presentation only.

Place the three `*Editor.cs` scripts inside a Unity `Editor/` assembly and the visual rig **outside Editor**. The existing art placement command does not create the gameplay; **implement a separate `Tools/Project Spark/Levels/Create Level 88`** command that creates real circuit terminals/wire definitions, process, safety, objectives and tests. Do not use the art placer to wipe game logic on reruns.

# 09. AAA PBR, PERFORMANCE, COLLISIONS AND VISUAL POLISH

There are **12 procedural material sets with 36 maps**, one albedo, normal and packed `MetalRoughAO` per material. Packed channels: **R metallic, G roughness, B AO**. Repurpose/repack to the actual Unity URP/HDRP/Standard shader (roughness ≠ smoothness) and verify normal-map importer state. The source specifies `uv_authoring_required=true`; commission production UV unwraps and baked/hand-authored details before describing final AAA.

Finish stainless adsorber towers, white ceramic habitat liners, brushed steel pipes, copper electrolyzer busbars, blue flow-tube glass, clear condensate water, rubber gaskets, fan impellers, filter matrices, safety-yellow railings and holographic diagnostics. Author correct glass sorting, emission intensity, filmic lighting, light probes/reflections, dust/dew/frost visuals, water level displays and high-fidelity UI labels without changing the authoritative data.

Simplify colliders on walkways and equipment; mesh colliders only for static/selective shapes; no dense convex colliders per pipe segment. Instancing/LOD for repeated cartridge modules, flange bolts, tubes, greenhouse parts; static batching for stationary process structure; animated fans/dampers as separate dynamic meshes. Add actual LODs and collision prefabs as production work, using `Collision_LOD_Production_Plan.csv` as **plan**, not completed evidence. Keep visual effect performance budget measurable.

# 10. VERIFIED SHIPPED SPARKLEVEL88VISUALRIG PUBLIC API

`ProjectSpark.Visuals.SparkLevel88VisualRig` has these exact public methods in the supplied C# file:

```csharp
public void BindVisuals();
public void SetActualCO2Processing(float actualBedASaturation01,
    float actualBedBSaturation01, float actualValveAngleDegrees);
public void SetActualOxygenProcess(float actualDeliveryFlow01,
    float actualRegulatorAngleDegrees);
public void SetActualWaterAndHumidity(float actualWaterPumpSpeed01,
    float actualCondenserFlow01, float actualFilterDrawerOpen01);
public void SetActualVentilation(float actualMainFanSpeed01,
    float actualBackupFanSpeed01, float actualDamperAOpen01,
    float actualDamperBOpen01, float actualDamperCOpen01,
    float actualDamperDOpen01);
public void SetActualSafety(bool actualW145TrustedAndFresh,
    bool actualCriticalFaultLatched, bool actualAtmosphereQualified,
    bool actualLowOxygen, bool actualAirlockOpen,
    float actualPressureRegulatorDegrees);
```

The rig finds named pivots, caches home transforms, then animates the **primary and backup blowers**, **water feed/recovery pumps**, **adsorber switching manifold**, **oxygen regulator**, **cabin pressure regulator**, **four damper pivots**, **adsorber display column scans**, **water filter drawer** and **airlock door** in `LateUpdate`. It adjusts three optional lights: healthy, low O₂ and W145 fault. Fans are presentation-rate rotation from measured normalized speed; do not read that angle back as physics speed. The script currently does **not** implement gas composition, control signals, safety, PLC, ECLSS or objectives; other named indicators may need additional shaders/VFX.

For normalized inputs provide actual physical measurements mapped intentionally to [0,1]; for angle parameters send actual degrees, not 0–1. Bind at startup and rebind after replacing visual art; do not invoke nonexistent members.

# 11. 72 VERIFIED ANIMATION PIVOTS AND OWNERSHIP

Read all 72 records from `Animation_Rigs.csv` (Annex B), and preserve exact `Pivot`, `Model`, `Kind`, `Axis`, XYZ m. `Kind=indicator` means a visual/VFX anchor, not physical motion. Rotating fans and pumps use source axis (`z` for blower rotor); `ANIM_ADSORBER_SWITCHING_MANIFOLD`, `ANIM_O2_DELIVERY_REGULATOR` and `ANIM_PRESSURE_REGULATOR` use `y`. `ANIM_DAMPER_A/B/C/D` are independent measured-position controls. The source also includes the airlock, process visual indicators, adsorber scans, condenser/ice, water levels, tank status, PLC/fault/SCADA anchors and final beacon.

Don't rotate the whole sector or process console when one valve turns. Preserve base local transforms and hierarchy; validate unique pivot names, owners, correct axes and intended constraints. If the art helper only animates a subset, extend with a level-specific visual adapter and optional actual-state-driven VFX, not false physical authority.

# 12. REAL 5 V CIRCUIT, GAS/WATER PLANT AND INDEPENDENT SAFETY

**Domain A: Educational player wiring** — 164 genuine connected electrical/signal edges created via the existing Spark tool, grounded control supply, working 5 V returns, typed analog/digital/ground/safety nodes, and topology-driven device enable/calibration/measurement validity.

**Domain B: Fictional process plant** — distinct inventories and physical flows of O₂, CO₂, inert cabin gases and water vapor, temperature/internal energy, finite water, adsorbent load, tank reserves, fan and pump dynamics, electrical consumption, heaters/chillers, gas regulators, valves, leaks and cooling. Do **not** treat a thin 5 V puzzle wire as a physical high-volume gas tube or electrolyzer power bus.

**Domain C: Independent protection** — emergency isolation, relief/regulation hard stops, safeguarded reserve authorization and sensor plausibility, with one latched fault and separate normal control permission. Independent protection uses independently valid measured data and its own action path; it cannot authorize normal pressure regulation when W145 is absent.

**Domain D: Mission/scenario evidence** — four-zone habitability acceptance, changing simulator demand, calibration and operator actions, historical goals and scoped run IDs. Keep this derived, not an alternate copy of plant truth.

# 13. EXACT 164-WIRE BUDGET

The source `Gameplay_Wiring_Budget.csv` is authoritative for the *planned* WIDs:

| IDs | Source group | Wires |
|---|---|---:|
| W001–W012 | Educational 5V Control and Return | 12 |
| W013–W036 | Cabin Gas and Pressure Instrumentation | 24 |
| W037–W060 | Dual CO2 Scrubbing and Regeneration | 24 |
| W061–W082 | Oxygen Generation and Gas Conditioning | 22 |
| W083–W102 | Humidity and Water-Recovery Process | 20 |
| W103–W120 | Ventilation, Valves and Filtration | 18 |
| W121–W146 | Independent Safety and Pressure Disturbances | 26 |
| W147–W164 | SCADA Diagnostics and Certification | 18 |
| **TOTAL** | | **164** |

Annex C enumerates every exact W001–W164 source model, art socket, destination model/socket, source `SignalKind`, purpose and critical flag. **All source rows have `ImplementationStatus=CONCEPT_ONLY_NO_PLAYER_WIRE`.** Build typed, actual runtime connection definitions but leave player edges unplugged at start. A row PASSes only when graph topology (including supported returns and safe signal compatibility) reports the correct live connection; a manually drawn line, light, or prefab child is not sufficient.

# 14. 242 ART SOCKETS → 328 LOGICAL CONTACTS: MANDATORY RECONCILIATION

There are exactly **242 decorative socket anchors** `P001`–`P242` with owner-model and position, but 164 wires need **328 distinct endpoint occurrences**. The package includes a **328-row logical-port plan**, plus **108 reused art socket records** (of which **62** combine different signal kinds). Examples: `P199` mixes `ANALOG_FEEDBACK` with `POWER_CONTROL`; `P193` and `P188` mix `GROUND_RETURN` with `POWER_CONTROL`. `P180` is the W145 target and has two distinct logical analog uses. Do not merge these contacts solely because their art sockets coincide.

Implement a deterministic reconciliation pass mapping every `WireID` + `Source/Target` to `Model`, original `P###`, independent `FunctionalPortID`, `SignalKind`, direction, voltage domain, supported node capacity and actual SparkTerminal instance. Where 242 unique socket meshes cannot support all 328 independent contacts, add named child subports/terminal blocks or physically offset additional functional connector faces; do not destroy the original 242 socket references. Never short a ground and 5 V feed or mix independent analog sensors into one line. If the actual solver supports legitimate multiconductor connectors/junctions, validate topology and isolate each independent contact. Output `Level88_TerminalReconciliationReport.csv` and actionable conflicts. The source 328 logical endpoints are planning declarations **not already instantiated functional terminals**.

# 15. CRITICAL W145: EXACT VERIFIED ROUTE

**W145 is the ONLY `Critical=YES` wire in the source wiring CSV**. Exact row:

- Source model: `L88_CabinPressurePrimaryTransducer`.
- Source visual anchor: **`P134`**.
- Source proposed functional port: **`TRUSTED_CABIN_PRESSURE_OUT`**.
- Destination model: `L88_HabitatLifeSupportSafetyPLC`.
- Destination visual anchor: **`P180`**.
- Destination proposed port: **`TRUSTED_CABIN_PRESSURE_IN`**.
- `SignalKind=ANALOG_FEEDBACK`, purpose **`CRITICAL trusted cabin pressure feedback`**.

Make W145 a genuine physically removable player-installed wire feeding the safety controller's **trusted normal-pressure sensor input**. Its continuity and sensor freshness must be checked from the real game circuit graph. A disconnected W145 must not convert the true physical pressure to zero. The separate independent cabin pressure sensor (`L88_IndependentCabinPressureSensor`) can serve *independent protective* checks but must never silently replace W145 for normal-pressure-control qualification.

# 16. ADJACENT SAFETY ROWS AND W145 TRUST BOUNDARY

Read exact W121–W146 and W147–W164 (Annexes C and H). Adjacent rows of particular importance:

- **W133:** `L88_ECLSSMasterPLC` → `L88_HabitatLifeSupportSafetyPLC`, normal supervisor safety run request.
- **W134:** Safety PLC → ECLSS master PLC, actual independent safety authorization.
- **W135:** Safety PLC → emergency isolation console, emergency isolation inhibit.
- **W136:** Emergency console → safety PLC, real station emergency-stop feedback.
- **W137:** Safety PLC → ECLSS PLC, fault latch event reporting.
- **W139/W140:** Condenser-freeze and CO₂-breakthrough safety conditions.
- **W142:** Safety latched-fault SCADA telemetry; diagnostic only.
- **W143/W144:** Explicit reset request and reset-demand feedback; not proof of repaired sensing.
- **W145:** Trusted primary pressure signal that normal control requires.
- **W146:** Final safety permission to mission certification beacon.

A healthy `W136` E-stop, safe `W142` alarm display, qualified independent sensor and `W146` visual signal cannot replace trusted W145 for normal closed-loop regulation. Cached W145 values must expire once the real wire disconnects; no synthetic timestamp refresh. Protect against false safe-state from an apparently low or absent sample. Safety may prevent further unsafe actuation while real physics continues.

# 17. ONE AUTHORITATIVE ECLSS PROCESS STATE

Track four compartments A/B/C/D separately: dry gas species moles `nO2`, `nCO2`, `nInert`, water vapor moles `nH2O_vapor`; actual gas temperature and thermal energy; volume; derived pressure and O₂/CO₂ partial pressures; humidity and dewpoint; actual supply/return flow; internal gas production/consumption; gas exchange through valves/dampers; loss through environment; current occupancy and heat/moisture loads.

Track process-wide: dual bed adsorbed-CO₂ mass and available capacity, capture/regeneration cycles, bed temperature/pressure drop; oxygen generator feedwater, process power/temperature, oxygen production and finite buffer tank, isolated byproduct inventory; condenser frost/heat transfer, reclaimed water reservoir/quality; fan speed/duct restriction, independent gas buffer, regulator and relief positions, active zone isolations; PLC states, all sensors' sample values/time/validity/calibration IDs, event log and six objective certificates.

Do not maintain contradictory truth in a `SparkLevel88Coordinator`, each machine component, SCADA, visual rig and objective tracker. Distinguish four layers: **requested command**, **true plant state**, **sampled measurement**, **qualified authorization/certification**.

# 18. DETERMINISTIC FIXED-STEP UPDATE ORDER

Use a stable fixed physics step (configurable, e.g. 0.02–0.1 game seconds) with mass/energy conservation assertions and deterministic fault seeds. Recommended order:

1. Read current real Spark circuit graph and player commands/physical wire changes.
2. Evaluate educational 5 V power/returns and independent safety input validity.
3. Apply latched permits, E-stop and available safe actuation commands.
4. Advance actual fans, valves, dampers, pumps, heaters and relief actuators with rate/limit constraints.
5. Compute demand from scheduled four-zone occupancy, heat, respiratory gas and moisture profiles.
6. Compute fan/duct network flows and inter-zone gas transfer from actual pressure/geometry/damper positions.
7. Transfer each gas species conservatively, including sensible heat of transported gas.
8. Simulate dual CO₂ bed capture/pressure drop/regeneration and bound remaining capacity.
9. Simulate water-fed oxygen electrolysis, byproduct isolation, buffer storage and gas delivery.
10. Simulate humidity condensation, recovered-water flow, frozen-coil restriction and water quality.
11. Apply all pressure regulators, relief flows and actual micro-leak mass loss.
12. Integrate gas inventory and thermal energy, then derive pressure/partial pressures/humidity.
13. Emit sensor samples with timestamps, calibration state, noise, range and live connectivity.
14. Evaluate independent safety thresholds, conflict checks, safe fallback and latching.
15. Progress PLC and time-stamped process/mission evidence with a real `runId`.
16. Push read-only actual/measured state to SCADA, logs, holograms and visual rig.

Avoid numerical concentration negatives, flow without inventory, violation of conservation at large `dt`, and hidden UI-driven teleports. Test step-size sensitivity and seeded replay determinism.

# 19. FOUR-COMPARTMENT GAS INVENTORY / EOS

Represent four physically separated well-mixed educational zones with finite volumes. Compute total pressure from total gas inventory and temperature using coherent ideal-gas approximation, `P_i = n_total_i*R*T_i/V_i`, and individual partial pressure `p_species_i = n_species_i*R*T_i/V_i`. Use absolute kelvin for gas temperature and consistent mole/joule/second/volume units. If using arbitrary game-scale units, document the mapping, derive rather than invent pressures and label all panels.

Use separate moles for O₂, CO₂, inert gas and water vapor; a leak removes a proportional mixture, while an oxygen regulator increases O₂ *only* through actual inventory transfer. Avoid fixed/constant cabin pressure as a magic property. Model ideal-gas limitations honestly; it's a bounded educational approximation, not detailed real spacecraft thermodynamics.

# 20. SPECIES CONSERVATION, MIXING AND NUMERIC INVARIANTS

For each compartment and species `s`: `dn[s]/dt = Σ inflow_moles_s − Σ outflow_moles_s + local_generation_s − local_consumption_s − leak_loss_s`. Intercompartment transport transfers exactly the same amount out of the donor that enters the receiver, apart from explicitly documented losses. Adsorption moves CO₂ from cabin circulation into the bed reservoir, and regeneration moves captured CO₂ to a separated process waste/store; it must not vanish from the universe. Condensation moves vapor → liquid reservoir, not to an implicit null sink.

Clamp outgoing streams to physically available inventory and reject NaN/overflow with diagnostics. Unit tests must check species total change equals traced source/sink terms, and an isolated closed habitat conserves total species moles absent declared internal conversion. Keep inert gas inventory, too, so O₂ generation cannot falsely supply adequate *total* pressure in arbitrary circumstances.

# 21. OCCUPANCY DEMAND, TEMPERATURE AND REAL UTILITY

Use `L88_MissionOccupancyDemandSimulator` for controlled fictional profiles with per-sector demand: oxygen uptake, CO₂ generation, humidity/heat release, varying equipment thermal load and ventilation duty. A low-demand phase, peak-demand phase and recovery phase must have different true process requirements and nontrivial resource usage. Demand continues while sensors are stale unless the scenario explicitly pauses. No oxygen or CO₂ all-off shortcut: a sealed chamber with no demand cannot pass mandated active-occupancy utility. Preserve objective progress only when the scenario actually traverses all stages with fresh authority/measurement data.

# 22. ACTUAL VENTILATION AND INTERZONE TRANSPORT

Use two independently driven circulation blowers `L88_VentilationMainCirculationBlower` and `L88_VentilationSecondaryCirculationBlower`, two variable-frequency fan drives, supply/return duct manifolds and four actual dampers. Model requested vs actual normalized motor speed with response lag and fault limits; derive airflow from available pressure head, duct resistance, path openings and density proxy. Bypass airflow cannot teleport gas from one compartment to another. Inadequate circulation creates localized CO₂ and humidity pockets even if average/SCADA readings appear good. Require validated return path and four-sector measured flow.

# 23. FOUR ZONE DAMPERS, ISOLATION AND AIRLOCKS

`L88_FourZoneMotorizedDamperBank` supports individually commanded and measured A/B/C/D positions. Simulate motor delay, obstruction, position feedback, upper/lower physical stops and proportional flow conductance. A closed damper protects a leaking zone but may deprive occupied zones of ventilation; balance mission utility and safety. Model airlock/pressure-service hatch requested vs actual state, pressure differential interlock and rate-limited mechanical travel. An E-stop or W145 loss must not instantly move heavy doors or freeze molecular motion.

# 24. DUAL CO₂ BED ADSORPTION PHYSICS

Tower A/B (`L88_DualAdsorberTowerA/B`) each have finite remaining capacity, gas residence time, inlet CO₂ partial pressure, loading/saturation, flow/pressure drop and temperature-dependent performance proxy. A physically motivated simplified capture rate is bounded by inlet CO₂ mole flow, available capacity and contact efficiency; it decreases when bed saturates, creating measurable outlet breakthrough. Both towers must support capture, transition, regeneration, cool-down and readiness stages rather than one binary capture flag. Instrument `L88_AdsorberInletCO2Analyzer`, `L88_AdsorberOutletCO2Analyzer` and `L88_AdsorptionCapacityMonitor` with genuine, fresh readings.

# 25. ADSORBER REGENERATION, SWITCHING AND BYPRODUCT HANDLING

`L88_AdsorberRegenerationHeaterA/B`, cycle selector, exhaust condenser and CO₂ isolation canister must use finite thermal energy, actual valve positions and finite regeneration time; captured CO₂ transfers into an isolated waste/storage path or accounted exhaust. Include heater limits, cooldown, pressure and power constraints. Player must schedule which bed is online so continued CO₂ capture is possible during regeneration. A stuck switching valve produces real flow misrouting and elevated CO₂ downstream. Do not regenerate instantly when a player presses REPAIR, and do not let both beds claim full capture without available capacity.

# 26. OXYGEN GENERATION: FINITE WATER, POWER AND RATE

Models include `L88_O2WaterElectrolysisChamber`, `L88_O2CellStackA/B`, `L88_O2GenerationPowerConverter`, `L88_WaterFeedPumpToElectrolyzer`, gas separator, `L88_OxygenBufferStorageTank`, delivery regulators, monitoring and oxygen process PLC. Use a **bounded educational** electrolysis proxy with coherent stoichiometric accounting: oxygen production consumes water and electrical energy; byproduct stream enters `L88_HydrogenByproductIsolationModule` as a *protected fictional separate inventory*, never directly mixed into cabin. Account for output flow, finite cell current/power, efficiency, thermal response, water feed quality and storage constraints. Do not provide real electrolyzer construction or operating procedures; only implement abstract game-model behavior.

# 27. O₂ BUFFER, REGULATORS AND FINITE EMERGENCY RESERVE

Track process output into a finite oxygen buffer and a separately finite protected backup reserve. Delivery through `L88_OxygenOutputFlowController`, `L88_OxygenDeliveryRegulatorManifold`, `L88_CabinPressureRegulator` and actual zone supply branches must obey valve response, supply pressure and safety interlocks. Reserve depletion must appear in SCADA and fail when exhausted. High O₂ concentration from a low-total-pressure cabin is not necessarily a safe O₂ partial pressure; qualification checks both. Don't allow indefinite oxygen flow from a tank with zero inventory.

# 28. PHYSICALLY DISTINCT O₂, CO₂ AND INERT GAS SAFETY

Model O₂ production, CO₂ adsorption and pressure control as separate interacting processes. A total-pressure controller cannot assume gas composition is healthy. A mixture can have acceptable `P_total` with an out-of-range `pO2` or `pCO2`, and vice versa. Gas analyzers need fresh calibration and independent measurement health. CO₂ threshold/alarm uses the actual fictional molar partial pressure (or a clearly documented proxy), never a fixed green UI percent. Inert balance prevents a pressure controller from inventing replacement N₂/inert inventory.

# 29. HUMIDITY CONDENSER, COOLING RESPONSE AND FROST

`L88_HumidityCondensingHeatExchanger`, `L88_CoolingCoilSerpentineAssembly`, `L88_CoolingValveActuator` and `L88_CondensateWaterSeparator` should calculate finite thermal exchanger effectiveness and dew-point-based condensation of a bounded vapor stream. Actual cooling loop and valve actuation have lag and heat-transfer limits. Frost can build when a fictional freeze condition is injected, restricting airflow and reducing recovery; remelting/recovery must take finite energy/time, not an instant UI reset. Surface condensation is visualized as actual vapor-to-liquid transfer and impacts water inventory.

# 30. RECLAIMED WATER INVENTORY AND QUALITY GATE

`L88_CondensateCollectionReservoir`, `L88_ReclaimedWaterPump`, `L88_WaterQualityMonitoringStation`, `L88_RecoveredWaterFiltrationRack` and `L88_WaterResourceBudgetAnalyzer` need measured quantity, finite pump rates, sensor health and quality gate. Condensate is not immediately purified. A water-quality failure must inhibit unsuitable feed to the fictional oxygen plant until corrected. Collecting, filtering, pumping and electrolysis all have actual resource and energy costs. Expose inputs and losses on resource trend graphs and compare predicted vs actual inventory after a complete demand run.

# 31. THERMAL ENERGY BALANCE AND INSTRUMENTATION

Each compartment has a heat-capacity approximation, occupancy/equipment heat generation, supply-air enthalpy transfer and environmental/radiative heat loss. Process devices (blower drive, regeneration heaters, electrolyzer power converter, condenser/chiller) dissipate thermal energy; water phase transition accounts for latent heat. Temperature should drift under disabled/failed equipment rather than stay hard-coded at 22 °C. Compute derived relative humidity from temperature and vapor partial pressure, with appropriate clamps and diagnostic warnings. Use game-scale thermal constants, not actual astronaut qualification parameters.

# 32. REAL MICRO-LEAK AND PRESSURE REGULATION

`L88_HabitatMicroLeakFaultInjector` changes an actual zone-to-environment conductance/area proxy. Gas loss depends on true zone pressure and available gas; the affected compartment pressure falls and finite reserve oxygen/inert supply may be consumed to compensate. Use a stable, bounded compressible-orifice-inspired approximation (or documented equivalent) not arbitrary pressure subtraction from a text field. Isolation dampers can reduce transfer, but a leak must still remove gas from the affected inventory. Repair physically changes seal conductance and must be verified by rate-of-pressure-change testing, not an alarm acknowledgement.

# 33. PRESSURE RELIEF, ISOLATION AND INDEPENDENT PROTECTION

`L88_HabitatPressureVesselSafetyRelief`, `L88_IndependentCabinPressureSensor`, `L88_CabinLeakIsolationManifold`, `L88_EmergencyAtmosphereIsolationConsole`, `L88_HabitatLifeSupportSafetyPLC` and `L88_WaterAndEnergySafetyInterlock` form protective and normal layers. Independent overpressure relief continues when the ECLSS master PLC or W145 is faulty. Safe mode must inhibit unqualified normal pressure commands, prevent unsafe large gas releases, conserve scarce inventory where possible, and permit only independently qualified limited actions. Protective relief should respond to true independent sensing and actual valve dynamics, not show a fictional always-safe status. Record unmet protections honestly.

# 34. CALIBRATED SENSOR SAMPLING AND TRUST

Every live pressure/gas/humidity/flow/temp sensor has its own calibration version, bias/offset, range, noise, sample interval, timestamp, physical power/wire validity, sanity bounds and fault flags. Separate direct true plant data from the measurement fed to PLC. Calibration is a recorded procedure using a fictional traceable reference, not pressing an always-pass button. Filtered samples have latency; don't silently reuse stale numbers as current. Critical W145 must be fresh, graph-connected, in range, calibrated and consistent with normal control expectations; backup/independent sensors can cross-check but not satisfy its trust contract.

# 35. EXPLICIT ECLSS MASTER PLC STATE MACHINE

Suggested actual states: `OFF`, `CONTROL_WIRING`, `CONTROL_POWER_CHECK`, `SAFETY_SELFTEST`, `SENSOR_CALIBRATION`, `VENTILATION_COMMISSIONING`, `ADSORBER_CAPTURE_START`, `ADSORBER_SWITCHING`, `ADSORBER_REGENERATION`, `OXYGEN_FEED_CHECK`, `OXYGEN_GENERATION`, `HUMIDITY_RECOVERY`, `PRESSURE_REGULATION`, `FOUR_ZONE_BALANCE`, `LOW_DEMAND_RUN`, `PEAK_DEMAND_RUN`, `RECOVERY_RUN`, `ATMOSPHERE_QUALIFIED`, `DISTURBANCE_DIAGNOSIS`, `SAFE_HOLD`, `PRESSURE_FEEDBACK_LOST`, `CRITICAL_FAULT_LATCHED`, `RESET_REQUIRED`, `FINAL_CERTIFIED`.

Transitions depend on real topology, actual flows, measured fresh sensor data, protected limits and qualified stage evidence—not elapsed timers or SCADA checkbox. Distinguish safety controller state from master sequence state and keep emergency command paths independent.

# 36. INITIAL FULL ENDURANCE QUALIFICATION RUN

Begin a new `runId` with a snapshot of four-zone inventories, water/oxygen reserves, bed loadings, electrical graph fingerprint, calibrated sensor versions and independent safety state. Advance **low demand → peak demand → recovery** with fresh time-stamped records at every required sector. Require actual O₂/CO₂ partial-pressure envelopes, cabin total-pressure bands, humidity/temp limits, minimum required interzone circulation, controlled water/oxygen usage, real dual-bed cycling, protected byproduct isolation, adequate water/energy reserve and no unresolved latched fault.

An all-fans-off room, a zero-demand fake scenario, skipped peak load, frozen simulation, debug state set, preloaded data or finished timer alone must fail. Compare resource budgets and true mass/energy accounting at the end; use configurable game-scale durations and tolerance. A normal operating interval must survive changing demand and finite adsorber capacity.

# 37. DISTURBANCE A — CO₂ ADSORBER BREAKTHROUGH

Inject a deterministic reduction of **actual** available capacity in Bed A or fail the selector valve changeover while Bed B is regenerating. Increase measured outlet CO₂ and four-zone CO₂ inventory according to airflow; physical cabin gas dynamics keep evolving. Diagnosis requires inlet vs outlet analyzer, flow, bed load/capture state, selector actual position and calibration. Recovery requires proper isolation or bed replacement/regeneration, restoration of valve switching, measured downstream capture, and fresh meaningful operation. No direct `disturbanceRecovered=true` without objective proof.

# 38. DISTURBANCE B — CONDENSER FREEZE AND WATER-CHAIN FAILURE

Inject an actual cooling-control fault with lower coil temperature, frost growth, reduced gas-side airflow/heat transfer and declining condensate production; water inventory and oxygen generation feed become affected as resources run down. Diagnosis through cooling valve actual state, coil temperature, airflow differential, moisture sensor, reservoir level and water quality. Recovery: safe warm/recover, clear modeled ice over time, reestablish controlled condenser operation, water quality and finite electrolyzer feed. A thaw animation alone does not pass.

# 39. DISTURBANCE C — FOUR-COMPARTMENT MICRO-LEAK

Inject a localized leak (e.g., Sector B) causing actual gas inventory loss and pressure decline. Player must locate via compartment pressure decay, independent pressure measurements, flow/differential-pressure comparisons, and real finite reserve consumption. A qualified seal/valve repair removes leak conductance only after correct isolation/repair interaction; perform a fresh pressure-hold/leak-rate test. Do not fabricate mass to refill the room instantly, and do not pass if permanent leak remains active.

# 40. THREE-FAULT RECOVERY CERTIFICATION

All three disturbances must be independently activated after the initial qualification, change authoritative plant states, yield diagnostic evidence and actually be repaired through supported game interactions. Require a new full low→peak→recovery endurance run **after the final repair** with healthy gas/thermal/resource conservation. Store separate fault IDs, plant-state deltas, repair IDs, calibration snapshots and new run ID. Objective 5 cannot pass because the player clicked three buttons, because alarms were cleared or because a prior baseline qualification was reused.

# 41. W145 DISCONNECTION: FINAL CONTROL TRUST FAILURE

After Objectives 1–5 have historically certified and the simulation is in a supervised safe training state, the player **physically unplugs real W145** through the Project Spark wire tool. The Spark graph loses that exact `P134→P180` logical path; W145 sensing becomes invalid/stale without zeroing actual cabin pressure or gas inventory. Normal automatic cabin-pressure control permission withdraws. The independent safety layer enters a restrictive fallback, maintains qualified protective pressure monitoring/relief and stops unsafe pressure regulator actions. It does not necessarily shut down all ventilation/CO₂ capture if they remain independently safe and essential to preserve atmosphere. Latch a critical fault, log timestamps and reason, display `TRUSTED CABIN PRESSURE FEEDBACK LOST — NORMAL REGULATION INHIBITED`.

Safety is not omnipotent: if a real leak or low resources continue, physical pressure may still deteriorate; declare actual risk/failure instead of an immortal-safe green dashboard. Never substitute cached pressure, independent backup readings, SCADA telemetry, health bits or a disconnected terminal that happens to look adjacent. Preserve historical Objective 1–5 certificates but mark current process authorization as invalid.

# 42. W145 RECONNECTION, RECALIBRATION AND EXPLICIT RESET

Reconnect the correct **physical player wire** and verify genuine circuit edge, `TRUSTED_CABIN_PRESSURE_OUT` to `TRUSTED_CABIN_PRESSURE_IN`, correct analog domain, 5 V transducer conditioning and return paths. Wait for actually new, range-valid samples with fresh timestamps. Perform a new pressure calibration/independent protection cross-check and confirm no unresolved leak, relief fault, oxygen/CO₂ emergency or harmful command. The PLC must refuse reset until each real prerequisite is met; require an explicit operator reset action carried through the correct functional sequence.

Only *then* restore normal control gradually, start a **new post-fault run ID**, and complete full four-zone low→peak→recovery life-support qualification with valid dynamic pressure/gas/water/heat tracking. Objective 6 must not certify on reconnection, calibration or reset alone.

# 43. SOURCE OBJECTIVE 1 — BUILD THE ATMOSPHERIC CONTROL NETWORK

Use exact source title **Build the Atmospheric Control Network**. Validate all 164 real player wires with unique WIDs, correct source/target and typed functional ports mapped to all 242 preserved art socket IDs; correct 5 V and returns; live interlocks, SCADA, process controller power, critical W145, and no unsafe bypass/shunt. Graph fingerprint + actual device/power/safety evidence required. Certify **1/6** only from the real evaluator.

# 44. SOURCE OBJECTIVE 2 — CALIBRATE THE HABITAT ATMOSPHERE

Use exact title **Calibrate the Habitat Atmosphere**. Four-zone pressure, O₂ partial pressure, CO₂, humidity and airflow instrumentation must be calibrated, fresh and valid; primary and backup fans and four independent dampers must prove finite actual motion and measurable circulation. Confirm independent safety checks and pressure protections. Timestamp calibration coefficients, device serial/port identity, sample quality and actual actuator evidence before **2/6**.

# 45. SOURCE OBJECTIVE 3 — COMMISSION OXYGEN AND CO₂ REGENERATION

Use exact title **Commission Oxygen and CO2 Regeneration**. Require actual finite-water oxygen generation with verifiable input/output O₂ and byproduct isolation, limited power and storage, two physically cycling CO₂ bed modules with breakthrough/capture measurements, safe switching/regeneration, and closed mass/energy balance across each process. No infinite refill or instant regenerated beds. Certify **3/6** with independent sensor samples and real resource histories.

# 46. SOURCE OBJECTIVE 4 — SUSTAIN THE LIVING ATMOSPHERE

Use exact title **Sustain the Living Atmosphere**. Initiate a fresh run with actual low-demand→peak-demand→recovery sequence and test all four zones, oxygen/CO₂ partial pressures, total pressure, temperatures, humidity, fan and damper flow, realistic occupancy duty, energy/resource margin, clean water supply and no unresolved protection alarms. Record run-scoped timestamps and system fingerprints. Only valid full-cycle evidence awards **4/6**.

# 47. SOURCE OBJECTIVE 5 — RECOVER THREE LIFE-SUPPORT FAILURES

Use exact title **Recover Three Life-Support Failures**. Require all three true plant disturbances (adsorption breakthrough, condenser freeze, cabin micro-leak) with causal sensor/flow/resource effects, diagnosis, physical repair and new full four-zone endurance operation with no active faults. A green SCADA overlay or replayed old test cannot qualify. Certify **5/6** only with fault-specific logs and new run evidence.

# 48. SOURCE OBJECTIVE 6 — RESTORE CRITICAL CABIN PRESSURE FEEDBACK

Use exact title **Restore Critical Cabin Pressure Feedback**. Require historical 1–5, intentional physical W145 disconnect, graph-edge mutation, trusted pressure sample invalidation, independent safe fallback and latched fault; then real correct reconnection, fresh pressure samples, calibration, protection cross-check, explicit reset and a **brand-new full low→peak→recovery run** with qualified four-zone inventories. The only success is actual `LevelGamePlayManager` **6/6**. Reset or wire repair alone cannot pass.

# 49. HISTORICAL OBJECTIVES, EVENT IDS, SAVE AND LOAD

Persist `ObjectiveCertifiedAt`, evidence IDs, calibration versions, topology fingerprint, `runId`, environment/process state snapshot, fault injection/repair IDs and W145 disconnect/reconnect history. Historical Objectives 1–5 remain certified even if current machinery enters a hazardous safety state during intentional final test. Qualification 6 requires a `runId` started after the W145 repair and reset; stale pre-fault measurements and aborted runs may not be reused. Builder reruns must not erase certificate history or multiply evaluator instances. Restore deterministic atmosphere and resource state safely on save/load.

# 50. LIVE AAA SCADA — ENGINEERING DATA NOT GAMEPLAY AUTHORITY

Present four-sector dashboard: true vs measured pressure, O₂/CO₂ partial pressures, gas composition, compartment temperature and relative humidity, individual damper positions and flow; main/backup blower actual speed and pressure head; dual bed inlet/outlet CO₂ and capacity, capture/regeneration phase, heater power and thermal state; electrolyzer power/water feed/O₂ flow and isolated byproduct; oxygen tank and emergency reserve inventories; condenser temperature, ice restriction, recovered water, reservoir level/quality; actual leak rate and regulator/relief state; full mass/energy audit, resource forecast and calibration validity.

Show W145 **graph continuity**, primary sample age, actual pressure separately, safety authorization, fault latch, explicit reset readiness and historical objectives 0/6→6/6. Distinguish `REQUESTED`, `ACTUAL`, `MEASURED`, `VALIDATED`; a green LED is never a safety fact. Trend histories must come from captured samples, not hard-coded polylines.

# 51. PLAYER INTERACTION BUTTONS AND DIAGNOSTICS

Create working buttons/interactables: `MAIN 5V`, `CHECK CONTROL RETURN`, `ECLSS SAFETY SELF-TEST`, `CALIBRATE PRESSURE`, `CALIBRATE O2`, `CALIBRATE CO2`, `CALIBRATE HUMIDITY`, `CALIBRATE FLOW`, `RUN BLOWER A/B`, `MOVE DAMPER A/B/C/D`, `SELECT BED A/B`, `REGENERATE BED A/B`, `READ BED CAPACITY`, `ENABLE WATER FEED`, `VERIFY WATER QUALITY`, `START O2 GENERATION`, `BUFFER/DELIVER O2`, `START CONDENSATE RECOVERY`, `HEAT EXCHANGER CONTROL`, `ZONE BALANCE`, `PRESSURE HOLD TEST`, `RUN OCCUPANCY CYCLE`, `DIAGNOSE ADSORBER`, `CLEAR CONDENSER ICE`, `REPAIR LEAK`, `RUN POSTREPAIR ENDURANCE`, `W145 DISCONNECT/RECONNECT THROUGH REAL WIRE TOOL`, `RECALIBRATE W145`, `EXPLICIT SAFETY RESET`, `START FINAL QUALIFICATION`.

Controls must have real effects on commands or actual supported repair interactions, and fail with precise reasons when prerequisites are unsatisfied. Provide context on equipment so players understand which terminal, sensor and physical repair is needed. Do not create functional-looking dead UI.

# 52. AAA VISUALS, ANIMATION AND PROCESS VFX

Level 88 should present a huge panoramic orbital biodome and four clearly labeled habitat sectors; dramatic twin stainless CO₂ adsorbers (one cyan active, one amber regenerating), luminous oxygen-cell electrodes, gas cylinders, air circulation impellers, cooling coils that visibly frost during failure, water-level vessels, ducting and dozens of real accessible electrical cabinet sockets. Use subtle realistic copper/gold, blue/cyan, green/amber/red status lighting, restrained flow holograms and high-contrast screen typography. Actual plant gas is invisible; colored flow arrows are a **telemetry overlay**, not literal glowing oxygen.

On breakthrough show measurable rising outlet CO₂ trends, not colored smoke as the only effect. Freeze visibly affects condenser/airflow/water inventory. Leak marks an isolation segment with pressure-loss trend, not a fake explosive decompression unless specifically required and physically modeled. W145 final fault changes pressure-trust indicator and regulated command state, without teleporting actual pressure to zero. Cinematic completion only plays when real 6/6 evidence is certified.

# 53. RECOMMENDED NEW C# RUNTIME COMPONENT RESPONSIBILITIES

Create only if no compatible existing code; these names are **proposals**: `SparkLevel88Coordinator`, `SparkLevel88State`, `SparkLevel88ElectricalBindings`, `SparkLevel88TerminalReconciler`, `SparkLevel88AtmosphereIntegrator`, `SparkLevel88FourZoneGasModel`, `SparkLevel88SpeciesTransfer`, `SparkLevel88ThermalModel`, `SparkLevel88OccupancyDemand`, `SparkLevel88VentilationNetwork`, `SparkLevel88DamperDynamics`, `SparkLevel88AdsorptionBed`, `SparkLevel88CO2RegenerationManager`, `SparkLevel88ElectrolyzerModel`, `SparkLevel88GasTankAndRegulator`, `SparkLevel88CondensateAndWaterModel`, `SparkLevel88WaterQualityGate`, `SparkLevel88LeakModel`, `SparkLevel88PressureRelief`, `SparkLevel88InstrumentSampler`, `SparkLevel88CalibrationService`, `SparkLevel88ECLSSMasterPLC`, `SparkLevel88IndependentSafetyPLC`, `SparkLevel88W145Recovery`, `SparkLevel88DisturbanceController`, `SparkLevel88ObjectiveTracker`, `SparkLevel88EnvironmentalSCADA`, `SparkLevel88VisualRigAdapter`.

Use one authority for gas/process state, injected interfaces for solver/safety, fixed physics step, testable calibration adapters and read-only UI. Group processes into coherent modules rather than 100 unrelated `Update()` functions. Avoid runtime reflection guesses about unavailable Spark methods.

# 54. ONE-CLICK UNITY LEVEL 88 GAMEPLAY BUILDER

Implement **`Tools/Project Spark/Levels/Create Level 88`** separately from shipped art helpers. Must:

1. Inspect installed Project Spark API and existing serialized schemas; refuse incompatible calls with clear diagnosis.
2. Confirm 90 numbered modular FBXs and matching import scale; never double-place assembled showcase.
3. Create/idempotently update `Level_88.asset`, `Level_88_Layout.prefab`, and existing level database/progression wiring.
4. Place/update 90 common-origin art modules only inside replaceable `L88_HD_VISUAL_ART`; preserve gameplay hierarchy.
5. Bind 72 original `ANIM_*` pivots, set up interaction colliders and appropriate art-only material previews.
6. Preserve all 242 original visual socket anchors; reconcile all 328 proposed independent functional logical endpoints against actual solver type/direction/domain.
7. Generate 164 real player-wire declarations (not already connected) using exact CSV WIDs and owner models.
8. Set up 5 V control supply and actual returns, independent safety, gas/thermal/water process power and inventory.
9. Create four separate compartments, ventilation/fan/damper topology, CO₂ dual-bed processing, oxygen and water chains and pressure/leak/regulator systems.
10. Bind actual state to timestamped instruments, independent calibration and master PLC.
11. Install six real objective definitions and run-scoped event evidence via existing project evaluator.
12. Build three real disturbance systems, repairs, critical W145 disconnection, reset and post-fault full qualification.
13. Set up SCADA panels, holograms, lights and visual rig actual-state adaptation.
14. Validate duplicates, incorrect domains, power and return, process flows, missing pivots and incompatible anchor reuse.
15. Generate `Level88_ImportReport`, `Level88_TerminalReconciliationReport.csv`, `Level88_ECLSSProcessAudit`, test artifacts and honest Unity result summary.
16. Use Undo and idempotent asset updates. Never overwrite prior levels or wipe hand-authored gameplay on rerun.

# 55. ACCEPTANCE TESTS — IMPORT AND STRUCTURAL

Test in actual Unity where available; do not mark PASS for tests not run:

1. All 90 exact numbered FBXs import and 90 matching GLBs exist; showcase not double-instantiated.
2. Imported assembly bounds approximately **30.8 m X span / 14.38 m Y span**, not 100× scaling.
3. All 72 original pivots appear under correct owner meshes, unique and correctly oriented.
4. All 242 original decorative socket IDs exist with correct owner coordinates.
5. All 328 proposed logical endpoint records load and map to safe functional Spark nodes.
6. All 108 reused-anchor conflicts are reconciled, including **62 mixed-signal-kind cases**.
7. Original 164 WIDs unique sequentially and map to correct source/destination and anchor.
8. W145 only critical, with exact `P134→P180` route and independent functional port identities.
9. 12 procedural material sets/36 maps import; UV production gaps reported, not hidden.
10. Unity art helper menus work, `SparkLevel88VisualRig` compiles and 72 pivots bind without duplicate transforms.
11. Art re-placement is undoable/idempotent and does not erase actual circuits or user content.
12. Levels 1–87 still load/behave; all compilation and editor validations recorded truthfully.

# 56. ACCEPTANCE TESTS — CIRCUIT, CALIBRATION AND SAFETY

13. All 164 player connections are initially absent and may be created through actual wire tool.
14. Correct live connections, not visual cables, power logic and qualify source/return.
15. Disconnect/incorrectly connect any required sensor wire changes device state and blocks certification.
16. No short between 5 V, ground, analog and safety even at P199/P193/P188 or P180.
17. W145 physical connection, sample freshness and calibration are continuously checked.
18. Independent sensor/SCADA cache cannot satisfy W145 trusted normal-control qualification.
19. Missing/expired sensor samples are invalid, never silently reported as zero true pressure.
20. Independent overpressure relief remains active with master PLC or W145 unavailable.
21. Emergency isolation and explicit reset require real supported circuit and safety evidence.
22. Requested damper/valve/pump/fan commands show finite measured actuator response and limit stops.
23. Reset refused while W145 disconnected/stale, independent protection unhealthy or real gas limits unsafe.
24. Objective 1 cannot pass through arbitrary prewired objects/forced UI statuses.

# 57. ACCEPTANCE TESTS — GAS, WATER, ENERGY AND PROCESS

25. Four zones maintain separate O₂/CO₂/inert/H₂O vapor mole balances and correct total pressure.
26. Species transfer from zone to process/other zone is conservative; no negative moles.
27. Leak removes finite gas inventory and actual pressure falls without pressure-value editing.
28. Gas addition occurs only from physically modeled supply with finite inventory.
29. O₂ partial pressure and oxygen % can change differently during depressurization.
30. Two CO₂ beds independently saturate, breakthrough and regenerate with finite time/energy.
31. Adsorbed carbon moves to bed inventory then to separated regeneration stream, no silent disappearance.
32. Failed bed-switching actuator produces true CO₂ outlet consequences and feedback mismatch.
33. Electrolyzer consumes real modeled water and power for bounded oxygen output; byproduct stays isolated.
34. Empty oxygen tank cannot deliver unlimited oxygen; finite reserve is not infinite safety.
35. Water vapor condensation increases liquid water inventory and reduces vapor accordingly.
36. Frozen condenser decreases heat transfer/airflow and limits water recovery until finite repair.
37. Water quality and reservoir level gate oxygen-generation feed honestly.
38. Both blowers and all four dampers affect flows and compartment mixing.
39. All-fans-off/zero-demand shortcut fails occupancy mission duty.
40. Occupancy heat, moisture, O₂/CO₂ demand affects actual measured process state.
41. Pressure regulator, relief and fan/pump electrical loads have finite response and resource cost.
42. Low→peak→recovery requires all actual stages, valid compartment envelopes and resource margin.
43. Mass/energy numerical audit remains within tolerance over full endurance interval.

# 58. ACCEPTANCE TESTS — THREE FAILURES, W145, OBJECTIVES

44. Adsorption breakthrough physically raises downstream/cabin CO₂ and repair restores capture through actual bed/valve state.
45. Condenser freeze affects actual thermal, frost, airflow and reclaimed-water quantities and requires real repair.
46. Micro-leak changes gas moles/pressure; isolation/repair reduces true leak conductance, verified by hold test.
47. All three disturbances are independently triggered/diagnosed/repaired with event IDs.
48. Fresh complete endurance run required after all three, not replayed original evidence.
49. Historical Objectives 1–5 remain certified after intentional W145 fault.
50. Actual player unplug changes real W145 graph edge; trusted sample becomes invalid/stale.
51. Actual true cabin pressure and gas inventory are **not** zeroed or rewritten on unplug.
52. Normal regulation authorization withdraws, independent protection remains active.
53. W145 reconnect from correct source/target restores only topology, not instantaneous safety clearance.
54. New pressure calibration, fresh sample and independent safety checks required.
55. Explicit operator reset required, with rejected reset diagnostic if prerequisites missing.
56. Reconnect/reset alone cannot qualify Objective 6.
57. Post-fault runId is new and complete low→peak→recovery test physically runs.
58. Objective 6 requires four-zone gas/thermal/resource performance and no unresolved latch.
59. Real `LevelGamePlayManager` displays **6/6** only after the above actual mission evidence.
60. Save/load, builder rerun and old-level regression preserve trusted history and do not artificially award 6/6.

# 59. COMPLETE PLAYER GAMEPLAY WALKTHROUGH

**CONNECT 164 REAL WIRES → RECONCILE 242 ORIGINAL ART SOCKETS TO 328 FUNCTIONAL CONTACTS → VERIFY 5 V LOGIC POWER AND RETURNS → INDEPENDENT ECLSS SAFETY TEST → CALIBRATE FOUR-ZONE PRESSURE, O₂, CO₂, HUMIDITY, FLOW AND TEMPERATURE SENSORS → COMMISSION BOTH FAN DRIVES → PROVE EACH OF FOUR DAMPERS AND RETURN PATHS → MEASURE REAL FOUR-COMPARTMENT AIR CIRCULATION → COMMISSION ADSORBER BED A CAPTURE / BED B REGENERATION → MEASURE INLET/OUTLET CO₂ AND CAPACITY → SWITCH BEDS WITH FINITE VALVE MOTION → QUALIFY WATER RESERVOIR/QUALITY → ENABLE ELECTROLYZER PROCESS POWER AND WATER FEED → VERIFY O₂ OUTPUT AND SAFE BYPRODUCT ISOLATION → STORE/REGULATE FINITE OXYGEN → COMMISSION CONDENSER / HUMIDITY RECOVERY → VERIFY COMPARTMENT PRESSURE/O₂ PARTIAL PRESSURE/CO₂/HUMIDITY BALANCE → RUN FULL LOW-DEMAND→PEAK-DEMAND→RECOVERY CYCLE → CERTIFY BASELINE → INJECT ADSORBER BREAKTHROUGH → MEASURE/DIAGNOSE/RESTORE BED → INJECT CONDENSER FREEZE → DETECT FLOW/WATER LOSS AND REPAIR → INJECT MICRO-LEAK → TRACK SECTOR GAS LOSS AND REPAIR/VERIFY PRESSURE-HOLD → COMPLETE A FRESH FULL ENDURANCE RUN → AT SUPERVISED SAFE STATE PHYSICALLY UNPLUG W145 → TRUSTED PRESSURE DATA INVALID → WITHDRAW NORMAL PRESSURE CONTROL → INDEPENDENT SAFE FALLBACK AND LATCH → RECONNECT EXACT W145 → VERIFY LIVE ANALOG SAMPLES AND 5 V RETURN → RECALIBRATE PRIMARY PRESSURE MEASUREMENT → CROSS-CHECK INDEPENDENT PROTECTION → EXPLICIT SAFETY RESET → BEGIN BRAND-NEW FULL FOUR-ZONE LOW→PEAK→RECOVERY LIFE SUPPORT QUALIFICATION → REAL `LevelGamePlayManager` CERTIFIES 6/6.**

# 60. DIAGNOSTICS, DELIVERABLES AND FINAL ACCEPTANCE CONTRACT

Generate readable errors: missing FBX/GLB/texture, wrong cm→m import, duplicate pivot or art root, missing P sockets, incompatible electrical domains on shared art sockets, missing functional `SparkTerminal`/return, non-player wire, W145 graph broken/sensor stale, analog calibration failed, pressure too low/high, separate O₂/CO₂ partial-pressure violations, incorrect gas composition, failed fan/duct/zone damper, adsorption saturation/breakthrough, regeneration valve jam, insufficient bed capacity, depleted O₂ tank/water reserve, electrolyzer power/feed missing, isolated byproduct fault, condenser freeze, water quality bad, reservoir dry, unaccounted species imbalance, active leak, independent pressure relief failure, reset refused, stale mission evidence, objectives stuck at 0/6 or 5/6, old levels corrupted, no Unity compilation and no Play Mode test.

Deliver importable Unity C# runtime source/editor scripts; `Level_88.asset`, `Level_88_Layout.prefab`, exact 90 model bindings, all 72 original pivot bindings, correct art/material setup, all 242 decorative sockets and reconciled functional ports for 328 endpoints, 164 truly player-connectable circuits, the four-zone ECLSS gas/heat/water process model, two regenerative CO₂ beds, finite oxygen electrolysis, resource inventories, ventilation and pressure control, actual sensor calibration, independent safety, three genuine fault-and-repair systems, critical W145 physical disconnect/reconnect/reset, SCADA, evidence-based six objectives, automated tests and honest results.

**Do not report “Level 88 complete” without actual Unity compilation, Play Mode demonstration of the full fresh post-W145 four-compartment atmosphere cycle, and authoritative `LevelGamePlayManager` 6/6. Preserve Levels 1–87.**

**PROJECT SPARK — LEVEL 88: THE BREATHING RING. CAPTURE. REGENERATE. REGULATE. BREATHE.**

---

# SOURCE-DERIVED ANNEXES — EXACT PACKAGE RECORDS

> The following records are generated from the original Level 88 CSV and JSON files, not paraphrased into imagined model IDs. They are art/planning source records, **not** Unity gameplay verification. The builder should consume the actual source files and compare against these tables.

## ANNEX A — ALL 90 EXACT MODULAR FBX AND GLB FILENAMES

| # | FBX filename | GLB filename | Triangles | Mesh parts |
|---:|---|---|---:|---:|
| 00 | `00_L88_HabitatAtmosphereQualificationArenaBase.fbx` | `00_L88_HabitatAtmosphereQualificationArenaBase.glb` | 900 | 9 |
| 01 | `01_L88_PanoramicOrbitalGardenViewport.fbx` | `01_L88_PanoramicOrbitalGardenViewport.glb` | 2,154 | 11 |
| 02 | `02_L88_HabitatRingGlassBioDome.fbx` | `02_L88_HabitatRingGlassBioDome.glb` | 18,078 | 38 |
| 03 | `03_L88_HabitatSectorA_Crew.fbx` | `03_L88_HabitatSectorA_Crew.glb` | 904 | 8 |
| 04 | `04_L88_HabitatSectorB_Research.fbx` | `04_L88_HabitatSectorB_Research.glb` | 904 | 8 |
| 05 | `05_L88_HabitatSectorC_Garden.fbx` | `05_L88_HabitatSectorC_Garden.glb` | 904 | 8 |
| 06 | `06_L88_HabitatSectorD_Habitat.fbx` | `06_L88_HabitatSectorD_Habitat.glb` | 904 | 8 |
| 07 | `07_L88_FourCompartmentAtmosphereDigitalTwin.fbx` | `07_L88_FourCompartmentAtmosphereDigitalTwin.glb` | 6,560 | 7 |
| 08 | `08_L88_PressurizedHabitatServiceBulkhead.fbx` | `08_L88_PressurizedHabitatServiceBulkhead.glb` | 2,148 | 5 |
| 09 | `09_L88_AtmosphereAirlockIsolationHatch.fbx` | `09_L88_AtmosphereAirlockIsolationHatch.glb` | 1,856 | 5 |
| 10 | `10_L88_LifeSupportEquipmentServiceWalkway.fbx` | `10_L88_LifeSupportEquipmentServiceWalkway.glb` | 684 | 4 |
| 11 | `11_L88_ProcessGasOverheadPipeNetwork.fbx` | `11_L88_ProcessGasOverheadPipeNetwork.glb` | 7,464 | 7 |
| 12 | `12_L88_DualAdsorberTowerA.fbx` | `12_L88_DualAdsorberTowerA.glb` | 6,090 | 14 |
| 13 | `13_L88_DualAdsorberTowerB.fbx` | `13_L88_DualAdsorberTowerB.glb` | 6,090 | 14 |
| 14 | `14_L88_AdsorberAFlowDistributor.fbx` | `14_L88_AdsorberAFlowDistributor.glb` | 1,286 | 8 |
| 15 | `15_L88_AdsorberBFlowDistributor.fbx` | `15_L88_AdsorberBFlowDistributor.glb` | 1,286 | 8 |
| 16 | `16_L88_AdsorbentCartridgeArrayA.fbx` | `16_L88_AdsorbentCartridgeArrayA.glb` | 1,596 | 7 |
| 17 | `17_L88_AdsorbentCartridgeArrayB.fbx` | `17_L88_AdsorbentCartridgeArrayB.glb` | 1,596 | 7 |
| 18 | `18_L88_AdsorberRegenerationHeaterA.fbx` | `18_L88_AdsorberRegenerationHeaterA.glb` | 1,292 | 8 |
| 19 | `19_L88_AdsorberRegenerationHeaterB.fbx` | `19_L88_AdsorberRegenerationHeaterB.glb` | 1,292 | 8 |
| 20 | `20_L88_AdsorberCycleSelectorValveManifold.fbx` | `20_L88_AdsorberCycleSelectorValveManifold.glb` | 1,286 | 8 |
| 21 | `21_L88_AdsorberInletCO2Analyzer.fbx` | `21_L88_AdsorberInletCO2Analyzer.glb` | 1,116 | 15 |
| 22 | `22_L88_AdsorberOutletCO2Analyzer.fbx` | `22_L88_AdsorberOutletCO2Analyzer.glb` | 1,116 | 15 |
| 23 | `23_L88_AdsorptionCapacityMonitor.fbx` | `23_L88_AdsorptionCapacityMonitor.glb` | 1,212 | 17 |
| 24 | `24_L88_RegenerationExhaustCondenser.fbx` | `24_L88_RegenerationExhaustCondenser.glb` | 1,292 | 8 |
| 25 | `25_L88_CO2StorageIsolationCanister.fbx` | `25_L88_CO2StorageIsolationCanister.glb` | 3,558 | 9 |
| 26 | `26_L88_CO2BreakthroughFaultInjector.fbx` | `26_L88_CO2BreakthroughFaultInjector.glb` | 948 | 13 |
| 27 | `27_L88_CO2RegenerationControllerPLC.fbx` | `27_L88_CO2RegenerationControllerPLC.glb` | 2,664 | 47 |
| 28 | `28_L88_O2WaterElectrolysisChamber.fbx` | `28_L88_O2WaterElectrolysisChamber.glb` | 7,656 | 13 |
| 29 | `29_L88_O2CellStackA.fbx` | `29_L88_O2CellStackA.glb` | 1,596 | 7 |
| 30 | `30_L88_O2CellStackB.fbx` | `30_L88_O2CellStackB.glb` | 1,596 | 7 |
| 31 | `31_L88_OxygenGasSeparator.fbx` | `31_L88_OxygenGasSeparator.glb` | 3,304 | 11 |
| 32 | `32_L88_HydrogenByproductIsolationModule.fbx` | `32_L88_HydrogenByproductIsolationModule.glb` | 1,032 | 14 |
| 33 | `33_L88_OxygenBufferStorageTank.fbx` | `33_L88_OxygenBufferStorageTank.glb` | 3,654 | 11 |
| 34 | `34_L88_OxygenOutputFlowController.fbx` | `34_L88_OxygenOutputFlowController.glb` | 936 | 12 |
| 35 | `35_L88_OxygenPartialPressureAnalyzer.fbx` | `35_L88_OxygenPartialPressureAnalyzer.glb` | 1,224 | 17 |
| 36 | `36_L88_OxygenOutputQualityMonitor.fbx` | `36_L88_OxygenOutputQualityMonitor.glb` | 936 | 12 |
| 37 | `37_L88_O2GenerationPowerConverter.fbx` | `37_L88_O2GenerationPowerConverter.glb` | 1,032 | 13 |
| 38 | `38_L88_OxygenGeneratorThermalCoolingLoop.fbx` | `38_L88_OxygenGeneratorThermalCoolingLoop.glb` | 1,292 | 8 |
| 39 | `39_L88_WaterFeedPumpToElectrolyzer.fbx` | `39_L88_WaterFeedPumpToElectrolyzer.glb` | 1,292 | 8 |
| 40 | `40_L88_OxygenDeliveryRegulatorManifold.fbx` | `40_L88_OxygenDeliveryRegulatorManifold.glb` | 1,286 | 8 |
| 41 | `41_L88_OxygenEmergencyBackupReserve.fbx` | `41_L88_OxygenEmergencyBackupReserve.glb` | 3,558 | 9 |
| 42 | `42_L88_ElectrolyzerCellMonitoringConsole.fbx` | `42_L88_ElectrolyzerCellMonitoringConsole.glb` | 1,032 | 14 |
| 43 | `43_L88_OxygenGenerationProcessPLC.fbx` | `43_L88_OxygenGenerationProcessPLC.glb` | 2,568 | 45 |
| 44 | `44_L88_HumidityCondensingHeatExchanger.fbx` | `44_L88_HumidityCondensingHeatExchanger.glb` | 1,758 | 9 |
| 45 | `45_L88_CoolingCoilSerpentineAssembly.fbx` | `45_L88_CoolingCoilSerpentineAssembly.glb` | 1,758 | 9 |
| 46 | `46_L88_CondensateWaterSeparator.fbx` | `46_L88_CondensateWaterSeparator.glb` | 3,242 | 10 |
| 47 | `47_L88_CondensateCollectionReservoir.fbx` | `47_L88_CondensateCollectionReservoir.glb` | 3,558 | 10 |
| 48 | `48_L88_ReclaimedWaterPump.fbx` | `48_L88_ReclaimedWaterPump.glb` | 1,292 | 8 |
| 49 | `49_L88_WaterQualityMonitoringStation.fbx` | `49_L88_WaterQualityMonitoringStation.glb` | 936 | 12 |
| 50 | `50_L88_FrozenCondenserFaultInjector.fbx` | `50_L88_FrozenCondenserFaultInjector.glb` | 948 | 12 |
| 51 | `51_L88_HumidityRecoveryPLC.fbx` | `51_L88_HumidityRecoveryPLC.glb` | 2,472 | 43 |
| 52 | `52_L88_VaporHumidityProbeArray.fbx` | `52_L88_VaporHumidityProbeArray.glb` | 1,032 | 13 |
| 53 | `53_L88_CoolingValveActuator.fbx` | `53_L88_CoolingValveActuator.glb` | 1,286 | 8 |
| 54 | `54_L88_RecoveredWaterFiltrationRack.fbx` | `54_L88_RecoveredWaterFiltrationRack.glb` | 548 | 6 |
| 55 | `55_L88_WaterResourceBudgetAnalyzer.fbx` | `55_L88_WaterResourceBudgetAnalyzer.glb` | 1,128 | 16 |
| 56 | `56_L88_VentilationMainCirculationBlower.fbx` | `56_L88_VentilationMainCirculationBlower.glb` | 2,698 | 12 |
| 57 | `57_L88_VentilationSecondaryCirculationBlower.fbx` | `57_L88_VentilationSecondaryCirculationBlower.glb` | 2,602 | 10 |
| 58 | `58_L88_VariableFrequencyFanDriveA.fbx` | `58_L88_VariableFrequencyFanDriveA.glb` | 1,032 | 13 |
| 59 | `59_L88_VariableFrequencyFanDriveB.fbx` | `59_L88_VariableFrequencyFanDriveB.glb` | 1,032 | 13 |
| 60 | `60_L88_PrimaryAirDuctDistributionManifold.fbx` | `60_L88_PrimaryAirDuctDistributionManifold.glb` | 4,992 | 5 |
| 61 | `61_L88_FourZoneMotorizedDamperBank.fbx` | `61_L88_FourZoneMotorizedDamperBank.glb` | 656 | 11 |
| 62 | `62_L88_HabitatA_ZonePressureSensor.fbx` | `62_L88_HabitatA_ZonePressureSensor.glb` | 936 | 12 |
| 63 | `63_L88_HabitatB_ZonePressureSensor.fbx` | `63_L88_HabitatB_ZonePressureSensor.glb` | 936 | 12 |
| 64 | `64_L88_HabitatC_ZonePressureSensor.fbx` | `64_L88_HabitatC_ZonePressureSensor.glb` | 936 | 12 |
| 65 | `65_L88_HabitatD_ZonePressureSensor.fbx` | `65_L88_HabitatD_ZonePressureSensor.glb` | 936 | 12 |
| 66 | `66_L88_CabinPressurePrimaryTransducer.fbx` | `66_L88_CabinPressurePrimaryTransducer.glb` | 1,236 | 16 |
| 67 | `67_L88_IndependentCabinPressureSensor.fbx` | `67_L88_IndependentCabinPressureSensor.glb` | 1,224 | 17 |
| 68 | `68_L88_DifferentialPressureMonitorArray.fbx` | `68_L88_DifferentialPressureMonitorArray.glb` | 1,212 | 16 |
| 69 | `69_L88_CabinLeakIsolationManifold.fbx` | `69_L88_CabinLeakIsolationManifold.glb` | 1,286 | 8 |
| 70 | `70_L88_CabinPressureRegulator.fbx` | `70_L88_CabinPressureRegulator.glb` | 1,286 | 8 |
| 71 | `71_L88_HabitatMicroLeakFaultInjector.fbx` | `71_L88_HabitatMicroLeakFaultInjector.glb` | 948 | 12 |
| 72 | `72_L88_TraceContaminantFilterArray.fbx` | `72_L88_TraceContaminantFilterArray.glb` | 276 | 6 |
| 73 | `73_L88_AirFlowVelocityMeasurementRack.fbx` | `73_L88_AirFlowVelocityMeasurementRack.glb` | 1,128 | 15 |
| 74 | `74_L88_GasMixingAndSamplingStation.fbx` | `74_L88_GasMixingAndSamplingStation.glb` | 1,020 | 13 |
| 75 | `75_L88_HabitatPressureVesselSafetyRelief.fbx` | `75_L88_HabitatPressureVesselSafetyRelief.glb` | 648 | 5 |
| 76 | `76_L88_AirQualityEnvironmentalPLC.fbx` | `76_L88_AirQualityEnvironmentalPLC.glb` | 2,268 | 38 |
| 77 | `77_L88_HabitatLifeSupportSafetyPLC.fbx` | `77_L88_HabitatLifeSupportSafetyPLC.glb` | 3,252 | 60 |
| 78 | `78_L88_Protected5VControlSupply.fbx` | `78_L88_Protected5VControlSupply.glb` | 1,788 | 28 |
| 79 | `79_L88_OxygenCO2TelemetryGateway.fbx` | `79_L88_OxygenCO2TelemetryGateway.glb` | 1,020 | 12 |
| 80 | `80_L88_ECLSSMasterPLC.fbx` | `80_L88_ECLSSMasterPLC.glb` | 2,652 | 46 |
| 81 | `81_L88_WaterAndEnergySafetyInterlock.fbx` | `81_L88_WaterAndEnergySafetyInterlock.glb` | 1,020 | 12 |
| 82 | `82_L88_HabitatEnvironmentalSCADA.fbx` | `82_L88_HabitatEnvironmentalSCADA.glb` | 4,336 | 40 |
| 83 | `83_L88_AtmosphericDigitalFlowHologram.fbx` | `83_L88_AtmosphericDigitalFlowHologram.glb` | 3,472 | 5 |
| 84 | `84_L88_AirSystemDiagnosticsRecorder.fbx` | `84_L88_AirSystemDiagnosticsRecorder.glb` | 924 | 11 |
| 85 | `85_L88_MissionOccupancyDemandSimulator.fbx` | `85_L88_MissionOccupancyDemandSimulator.glb` | 924 | 11 |
| 86 | `86_L88_W145CriticalPressureFeedbackPatchStation.fbx` | `86_L88_W145CriticalPressureFeedbackPatchStation.glb` | 1,172 | 13 |
| 87 | `87_L88_EmergencyAtmosphereIsolationConsole.fbx` | `87_L88_EmergencyAtmosphereIsolationConsole.glb` | 1,128 | 16 |
| 88 | `88_L88_AtmosphereMissionCertificationBeacon.fbx` | `88_L88_AtmosphereMissionCertificationBeacon.glb` | 1,772 | 5 |
| 89 | `89_L88_EnvironmentalInspectionCamera.fbx` | `89_L88_EnvironmentalInspectionCamera.glb` | 344 | 5 |

**Modular totals: 184,224 triangles and 1,219 mesh parts.** Alternative assembled preview: `Models/Level88_Complete_ArtKit_Showcase.fbx`, `GLB/Level88_Complete_ArtKit_Showcase.glb` (do not place with the modular set).

## ANNEX B — ALL 72 ACTUAL ANIMATION PIVOT PARENTS

These are exact pivot-owner, kind, axis and world XYZ records (metres). `indicator` is presentation only.

| # | Pivot name | Owner model | Kind | Axis | X m | Y m | Z m |
|---:|---|---|---|:---:|---:|---:|---:|
| 1 | `ANIM_HABITAT_HOLOGRAM_RING` | `L88_HabitatRingGlassBioDome` | indicator | `none` | 0 | 8.2 | -9.8 |
| 2 | `ANIM_COMPARTMENT_AIR_QUALITY` | `L88_FourCompartmentAtmosphereDigitalTwin` | indicator | `none` | 0 | 11.6 | -5.7 |
| 3 | `ANIM_AIRLOCK_DOOR` | `L88_AtmosphereAirlockIsolationHatch` | translation | `x` | -11.4 | 4.5 | -7.6 |
| 4 | `ANIM_GAS_FLOW_HEADER` | `L88_ProcessGasOverheadPipeNetwork` | indicator | `none` | 0 | 6.0 | -3.0 |
| 5 | `ANIM_ADSORBER_A_CYCLE` | `L88_DualAdsorberTowerA` | rotation | `z` | -11.0 | 3.2 | -0.7 |
| 6 | `ANIM_ADSORBER_B_CYCLE` | `L88_DualAdsorberTowerB` | rotation | `z` | -8.35 | 3.2 | -0.7 |
| 7 | `ANIM_ADSORBER_A_INLET_VALVE` | `L88_AdsorberAFlowDistributor` | rotation | `y` | -11 | 1.85 | 1.3 |
| 8 | `ANIM_ADSORBER_B_INLET_VALVE` | `L88_AdsorberBFlowDistributor` | rotation | `y` | -8.35 | 1.85 | 1.3 |
| 9 | `ANIM_ADSORBENT_A_COLUMN_SCAN` | `L88_AdsorbentCartridgeArrayA` | translation | `y` | -11 | 2.7 | -0.5 |
| 10 | `ANIM_ADSORBENT_B_COLUMN_SCAN` | `L88_AdsorbentCartridgeArrayB` | translation | `y` | -8.35 | 2.7 | -0.5 |
| 11 | `ANIM_ADSORBER_A_HEATER` | `L88_AdsorberRegenerationHeaterA` | indicator | `none` | -11 | 1.7 | -2 |
| 12 | `ANIM_ADSORBER_B_HEATER` | `L88_AdsorberRegenerationHeaterB` | indicator | `none` | -8.35 | 1.7 | -2 |
| 13 | `ANIM_ADSORBER_SWITCHING_MANIFOLD` | `L88_AdsorberCycleSelectorValveManifold` | rotation | `y` | -9.7 | 2.0 | 2.0 |
| 14 | `ANIM_REGEN_EXHAUST_CONDENSER` | `L88_RegenerationExhaustCondenser` | rotation | `x` | -10.7 | 1.45 | 3.8 |
| 15 | `ANIM_CO2_CANISTER_VALVE` | `L88_CO2StorageIsolationCanister` | rotation | `y` | -8.9 | 1.7 | 3.8 |
| 16 | `ANIM_CO2_BREAKTHROUGH_FAULT` | `L88_CO2BreakthroughFaultInjector` | translation | `x` | -8.64 | 1.65 | 5.5 |
| 17 | `ANIM_ADSORBER_CONTROL_STATUS` | `L88_CO2RegenerationControllerPLC` | indicator | `none` | -7.319999999999999 | 1.65 | 5.5 |
| 18 | `ANIM_ELECTROLYSIS_BUBBLE_COLUMN` | `L88_O2WaterElectrolysisChamber` | indicator | `none` | -4.0 | 2.95 | -1.15 |
| 19 | `ANIM_O2_CELL_A_SCAN` | `L88_O2CellStackA` | translation | `y` | -5.25 | 2.7 | -0.6 |
| 20 | `ANIM_O2_CELL_B_SCAN` | `L88_O2CellStackB` | translation | `y` | -2.75 | 2.7 | -0.6 |
| 21 | `ANIM_O2_SEPARATOR` | `L88_OxygenGasSeparator` | rotation | `y` | -1.65 | 2.0 | 1 |
| 22 | `ANIM_BYPRODUCT_ISOLATION` | `L88_HydrogenByproductIsolationModule` | rotation | `y` | -5.5 | 1.75 | 1.7 |
| 23 | `ANIM_O2_BUFFER_LEVEL` | `L88_OxygenBufferStorageTank` | indicator | `none` | -0.2 | 2.65 | -1 |
| 24 | `ANIM_O2_FLOW_CONTROL` | `L88_OxygenOutputFlowController` | rotation | `z` | -5.999999999999999 | 1.65 | 5.5 |
| 25 | `ANIM_O2_PARTIAL_PRESSURE` | `L88_OxygenPartialPressureAnalyzer` | indicator | `none` | -4.68 | 1.65 | 5.5 |
| 26 | `ANIM_O2_PURITY_MONITOR` | `L88_OxygenOutputQualityMonitor` | indicator | `none` | -3.3599999999999994 | 1.65 | 5.5 |
| 27 | `ANIM_O2_POWER_CONVERTER` | `L88_O2GenerationPowerConverter` | indicator | `none` | -2.039999999999999 | 1.65 | 5.5 |
| 28 | `ANIM_O2_COOLING_PUMP` | `L88_OxygenGeneratorThermalCoolingLoop` | rotation | `z` | -2.2 | 1.35 | 3.1 |
| 29 | `ANIM_WATER_FEED_PUMP` | `L88_WaterFeedPumpToElectrolyzer` | rotation | `z` | -4.3 | 1.1 | 3.2 |
| 30 | `ANIM_O2_DELIVERY_REGULATOR` | `L88_OxygenDeliveryRegulatorManifold` | rotation | `y` | -0.2 | 1.7 | 1.6 |
| 31 | `ANIM_O2_BACKUP_ISOLATION` | `L88_OxygenEmergencyBackupReserve` | rotation | `y` | 1.4 | 2.3 | 1.1 |
| 32 | `ANIM_CELL_MONITOR` | `L88_ElectrolyzerCellMonitoringConsole` | indicator | `none` | -0.7199999999999989 | 1.65 | 5.5 |
| 33 | `ANIM_O2_PROCESS_PLC` | `L88_OxygenGenerationProcessPLC` | indicator | `none` | 0.6000000000000014 | 1.65 | 5.5 |
| 34 | `ANIM_CONDENSER_PROCESS` | `L88_HumidityCondensingHeatExchanger` | indicator | `none` | 4.0 | 2.2 | -1.3 |
| 35 | `ANIM_CONDENSER_COOLANT` | `L88_CoolingCoilSerpentineAssembly` | indicator | `none` | 4 | 2.8 | -1.3 |
| 36 | `ANIM_CONDENSATE_SEPARATOR` | `L88_CondensateWaterSeparator` | rotation | `y` | 5.9 | 1.5 | 1.4 |
| 37 | `ANIM_CONDENSATE_TANK_LEVEL` | `L88_CondensateCollectionReservoir` | indicator | `none` | 5.3 | 1.6 | 3.1 |
| 38 | `ANIM_RECOVERED_WATER_PUMP` | `L88_ReclaimedWaterPump` | rotation | `z` | 6.9 | 1.4 | 3.15 |
| 39 | `ANIM_WATER_QUALITY` | `L88_WaterQualityMonitoringStation` | indicator | `none` | -12.5 | 1.65 | 7.65 |
| 40 | `ANIM_FREEZE_FAULT` | `L88_FrozenCondenserFaultInjector` | translation | `x` | -10.879999999999999 | 1.65 | 7.65 |
| 41 | `ANIM_HUMIDITY_PLC` | `L88_HumidityRecoveryPLC` | indicator | `none` | -9.26 | 1.65 | 7.65 |
| 42 | `ANIM_HUMIDITY_SENSOR` | `L88_VaporHumidityProbeArray` | indicator | `none` | -7.64 | 1.65 | 7.65 |
| 43 | `ANIM_COOLING_VALVE` | `L88_CoolingValveActuator` | rotation | `y` | -6.02 | 1.65 | 7.65 |
| 44 | `ANIM_WATER_FILTER_DRAWER` | `L88_RecoveredWaterFiltrationRack` | translation | `x` | 4 | 1.65 | 3.3 |
| 45 | `ANIM_WATER_BUDGET` | `L88_WaterResourceBudgetAnalyzer` | indicator | `none` | -4.399999999999999 | 1.65 | 7.65 |
| 46 | `ANIM_PRIMARY_BLOWER_ROTOR` | `L88_VentilationMainCirculationBlower` | rotation | `z` | 9.0 | 2.1 | -1 |
| 47 | `ANIM_BACKUP_BLOWER_ROTOR` | `L88_VentilationSecondaryCirculationBlower` | rotation | `z` | 11.1 | 2.1 | -1 |
| 48 | `ANIM_FAN_A_FREQUENCY` | `L88_VariableFrequencyFanDriveA` | indicator | `none` | -2.7799999999999994 | 1.65 | 7.65 |
| 49 | `ANIM_FAN_B_FREQUENCY` | `L88_VariableFrequencyFanDriveB` | indicator | `none` | -1.1600000000000001 | 1.65 | 7.65 |
| 50 | `ANIM_MAIN_DUCT_AIR_FLOW` | `L88_PrimaryAirDuctDistributionManifold` | indicator | `none` | 10 | 4.25 | -3.0 |
| 51 | `ANIM_DAMPER_A` | `L88_FourZoneMotorizedDamperBank` | rotation | `z` | 9.13 | 3.6 | 1.75 |
| 52 | `ANIM_DAMPER_B` | `L88_FourZoneMotorizedDamperBank` | rotation | `z` | 9.71 | 3.6 | 1.75 |
| 53 | `ANIM_DAMPER_C` | `L88_FourZoneMotorizedDamperBank` | rotation | `z` | 10.29 | 3.6 | 1.75 |
| 54 | `ANIM_DAMPER_D` | `L88_FourZoneMotorizedDamperBank` | rotation | `z` | 10.87 | 3.6 | 1.75 |
| 55 | `ANIM_ZONE_A_PRESSURE` | `L88_HabitatA_ZonePressureSensor` | indicator | `none` | 0.46000000000000085 | 1.65 | 7.65 |
| 56 | `ANIM_ZONE_B_PRESSURE` | `L88_HabitatB_ZonePressureSensor` | indicator | `none` | 2.080000000000002 | 1.65 | 7.65 |
| 57 | `ANIM_ZONE_C_PRESSURE` | `L88_HabitatC_ZonePressureSensor` | indicator | `none` | 3.700000000000003 | 1.65 | 7.65 |
| 58 | `ANIM_ZONE_D_PRESSURE` | `L88_HabitatD_ZonePressureSensor` | indicator | `none` | 5.32 | 1.65 | 7.65 |
| 59 | `ANIM_TRUSTED_PRESSURE_MONITOR` | `L88_CabinPressurePrimaryTransducer` | indicator | `none` | 6.940000000000001 | 1.65 | 7.65 |
| 60 | `ANIM_INDEPENDENT_PRESSURE` | `L88_IndependentCabinPressureSensor` | indicator | `none` | 8.560000000000002 | 1.65 | 7.65 |
| 61 | `ANIM_LEAK_ISOLATION` | `L88_CabinLeakIsolationManifold` | rotation | `y` | 9.7 | 1.5 | 3.6 |
| 62 | `ANIM_PRESSURE_REGULATOR` | `L88_CabinPressureRegulator` | rotation | `y` | 11.3 | 1.5 | 3.6 |
| 63 | `ANIM_MICROLEAK_FAULT` | `L88_HabitatMicroLeakFaultInjector` | translation | `x` | -11.4 | 1.8 | 9.7 |
| 64 | `ANIM_FILTER_EXTRACT` | `L88_TraceContaminantFilterArray` | translation | `x` | 11.8 | 2.5 | 2.1 |
| 65 | `ANIM_FLOW_MEASUREMENT` | `L88_AirFlowVelocityMeasurementRack` | indicator | `none` | -9.57 | 1.8 | 9.7 |
| 66 | `ANIM_PRESSURE_RELIEF` | `L88_HabitatPressureVesselSafetyRelief` | translation | `y` | 13.2 | 2.0 | 1 |
| 67 | `ANIM_LIFE_SUPPORT_SAFETY` | `L88_HabitatLifeSupportSafetyPLC` | indicator | `none` | -4.08 | 1.8 | 9.7 |
| 68 | `ANIM_MASTER_ECLSS_SCADA` | `L88_HabitatEnvironmentalSCADA` | indicator | `none` | 0 | 7.4 | -5.75 |
| 69 | `ANIM_ATMOS_FLOW_HOLOGRAM` | `L88_AtmosphericDigitalFlowHologram` | indicator | `none` | 4.3 | 7.8 | -5.8 |
| 70 | `ANIM_W145_TRUSTED_PATCH` | `L88_W145CriticalPressureFeedbackPatchStation` | indicator | `none` | 8.730000000000002 | 1.8 | 9.7 |
| 71 | `ANIM_EMERGENCY_ISOLATION` | `L88_EmergencyAtmosphereIsolationConsole` | translation | `z` | 10.56 | 1.8 | 9.7 |
| 72 | `ANIM_FINAL_CERTIFICATION` | `L88_AtmosphereMissionCertificationBeacon` | indicator | `none` | 0 | 11.75 | -5.8 |

## ANNEX C — ALL 164 EXACT CONCEPTUAL PLAYER WIRE ROUTES

The original rows say `CONCEPT_ONLY_NO_PLAYER_WIRE`. Every WID below must become real player-connectable topology with audited type/direction, but is **not an installed live wire in the source kit**.

| WID | Source model (art anchor) | Target model (art anchor) | Source signal type | Original purpose | Critical |
|---|---|---|---|---|:---:|
| **W001** | `L88_Protected5VControlSupply` (`P189`) | `L88_ECLSSMasterPLC` (`P199`) | `POWER_CONTROL` | Primary ECLSS PLC logic power | NO |
| **W002** | `L88_Protected5VControlSupply` (`P193`) | `L88_HabitatLifeSupportSafetyPLC` (`P166`) | `POWER_CONTROL` | Independent life-support safety controller power | NO |
| **W003** | `L88_Protected5VControlSupply` (`P188`) | `L88_CO2RegenerationControllerPLC` (`P038`) | `POWER_CONTROL` | Adsorption-cycle PLC control power | NO |
| **W004** | `L88_Protected5VControlSupply` (`P192`) | `L88_OxygenGenerationProcessPLC` (`P077`) | `POWER_CONTROL` | Oxygen process controller power | NO |
| **W005** | `L88_Protected5VControlSupply` (`P196`) | `L88_HumidityRecoveryPLC` (`P102`) | `POWER_CONTROL` | Humidity recovery controller power | NO |
| **W006** | `L88_Protected5VControlSupply` (`P191`) | `L88_AirQualityEnvironmentalPLC` (`P156`) | `POWER_CONTROL` | Ventilation and air-quality controller power | NO |
| **W007** | `L88_Protected5VControlSupply` (`P195`) | `L88_HabitatEnvironmentalSCADA` (`P224`) | `POWER_CONTROL` | Atmosphere SCADA control power | NO |
| **W008** | `L88_Protected5VControlSupply` (`P190`) | `L88_CabinPressurePrimaryTransducer` (`P133`) | `POWER_CONTROL` | Primary cabin pressure transducer conditioning power | NO |
| **W009** | `L88_Protected5VControlSupply` (`P194`) | `L88_IndependentCabinPressureSensor` (`P135`) | `POWER_CONTROL` | Backup pressure monitor logic power | NO |
| **W010** | `L88_Protected5VControlSupply` (`P189`) | `L88_OxygenCO2TelemetryGateway` (`P197`) | `POWER_CONTROL` | Telemetry gateway logic power | NO |
| **W011** | `L88_HabitatLifeSupportSafetyPLC` (`P178`) | `L88_Protected5VControlSupply` (`P193`) | `GROUND_RETURN` | Life-support safety control return | NO |
| **W012** | `L88_ECLSSMasterPLC` (`P213`) | `L88_Protected5VControlSupply` (`P188`) | `GROUND_RETURN` | Main ECLSS supervisory control return | NO |
| **W013** | `L88_HabitatA_ZonePressureSensor` (`P129`) | `L88_ECLSSMasterPLC` (`P199`) | `ANALOG_FEEDBACK` | Crew compartment measured cabin pressure | NO |
| **W014** | `L88_HabitatB_ZonePressureSensor` (`P130`) | `L88_ECLSSMasterPLC` (`P203`) | `ANALOG_FEEDBACK` | Research compartment measured cabin pressure | NO |
| **W015** | `L88_HabitatC_ZonePressureSensor` (`P131`) | `L88_ECLSSMasterPLC` (`P207`) | `ANALOG_FEEDBACK` | Garden compartment measured cabin pressure | NO |
| **W016** | `L88_HabitatD_ZonePressureSensor` (`P132`) | `L88_ECLSSMasterPLC` (`P211`) | `ANALOG_FEEDBACK` | Habitat compartment measured cabin pressure | NO |
| **W017** | `L88_CabinPressurePrimaryTransducer` (`P133`) | `L88_ECLSSMasterPLC` (`P215`) | `ANALOG_FEEDBACK` | Primary cabin total-pressure telemetry diagnostic | NO |
| **W018** | `L88_IndependentCabinPressureSensor` (`P135`) | `L88_HabitatLifeSupportSafetyPLC` (`P164`) | `ANALOG_FEEDBACK` | Independent cabin-pressure safety monitor | NO |
| **W019** | `L88_DifferentialPressureMonitorArray` (`P139`) | `L88_ECLSSMasterPLC` (`P202`) | `ANALOG_FEEDBACK` | Differential compartment pressure trend | NO |
| **W020** | `L88_AdsorberInletCO2Analyzer` (`P024`) | `L88_CO2RegenerationControllerPLC` (`P040`) | `ANALOG_FEEDBACK` | CO2 absorption intake actual measurement | NO |
| **W021** | `L88_AdsorberOutletCO2Analyzer` (`P025`) | `L88_CO2RegenerationControllerPLC` (`P044`) | `ANALOG_FEEDBACK` | CO2 absorption exhaust actual measurement | NO |
| **W022** | `L88_OxygenPartialPressureAnalyzer` (`P063`) | `L88_OxygenGenerationProcessPLC` (`P081`) | `ANALOG_FEEDBACK` | Oxygen partial-pressure sensor feedback | NO |
| **W023** | `L88_VaporHumidityProbeArray` (`P113`) | `L88_HumidityRecoveryPLC` (`P107`) | `ANALOG_FEEDBACK` | Water-vapor humidity measurement | NO |
| **W024** | `L88_AirFlowVelocityMeasurementRack` (`P145`) | `L88_AirQualityEnvironmentalPLC` (`P163`) | `ANALOG_FEEDBACK` | Actual air-duct flow velocity | NO |
| **W025** | `L88_GasMixingAndSamplingStation` (`P148`) | `L88_ECLSSMasterPLC` (`P211`) | `ANALOG_FEEDBACK` | Mixed sample gas analyzer output | NO |
| **W026** | `L88_OxygenCO2TelemetryGateway` (`P197`) | `L88_HabitatEnvironmentalSCADA` (`P229`) | `ANALOG_FEEDBACK` | Atmosphere sampling gateway display | NO |
| **W027** | `L88_ECLSSMasterPLC` (`P198`) | `L88_HabitatA_ZonePressureSensor` (`P129`) | `ANALOG_FEEDBACK` | Zone A pressure sampling request | NO |
| **W028** | `L88_ECLSSMasterPLC` (`P202`) | `L88_HabitatB_ZonePressureSensor` (`P130`) | `ANALOG_FEEDBACK` | Zone B pressure sampling request | NO |
| **W029** | `L88_ECLSSMasterPLC` (`P206`) | `L88_HabitatC_ZonePressureSensor` (`P131`) | `ANALOG_FEEDBACK` | Zone C pressure sampling request | NO |
| **W030** | `L88_ECLSSMasterPLC` (`P210`) | `L88_HabitatD_ZonePressureSensor` (`P132`) | `ANALOG_FEEDBACK` | Zone D pressure sampling request | NO |
| **W031** | `L88_HabitatLifeSupportSafetyPLC` (`P180`) | `L88_DifferentialPressureMonitorArray` (`P139`) | `ANALOG_FEEDBACK` | Safety differential pressure diagnostic | NO |
| **W032** | `L88_AirQualityEnvironmentalPLC` (`P160`) | `L88_AirFlowVelocityMeasurementRack` (`P146`) | `ANALOG_FEEDBACK` | Airflow transducer calibration command | NO |
| **W033** | `L88_ECLSSMasterPLC` (`P198`) | `L88_OxygenPartialPressureAnalyzer` (`P062`) | `ANALOG_FEEDBACK` | Oxygen analyzer measurement request | NO |
| **W034** | `L88_ECLSSMasterPLC` (`P202`) | `L88_AdsorberInletCO2Analyzer` (`P023`) | `ANALOG_FEEDBACK` | CO2 intake calibration request | NO |
| **W035** | `L88_ECLSSMasterPLC` (`P206`) | `L88_AdsorberOutletCO2Analyzer` (`P027`) | `ANALOG_FEEDBACK` | CO2 outlet calibration request | NO |
| **W036** | `L88_IndependentCabinPressureSensor` (`P135`) | `L88_HabitatEnvironmentalSCADA` (`P225`) | `ANALOG_FEEDBACK` | Non-authoritative backup pressure diagnostic display | NO |
| **W037** | `L88_CO2RegenerationControllerPLC` (`P045`) | `L88_DualAdsorberTowerA` (`P013`) | `DIGITAL_CONTROL` | Adsorber A process start permit | NO |
| **W038** | `L88_CO2RegenerationControllerPLC` (`P049`) | `L88_DualAdsorberTowerB` (`P014`) | `DIGITAL_CONTROL` | Adsorber B process start permit | NO |
| **W039** | `L88_CO2RegenerationControllerPLC` (`P035`) | `L88_AdsorberAFlowDistributor` (`P015`) | `DIGITAL_CONTROL` | Adsorber A inlet command | NO |
| **W040** | `L88_CO2RegenerationControllerPLC` (`P039`) | `L88_AdsorberBFlowDistributor` (`P016`) | `DIGITAL_CONTROL` | Adsorber B inlet command | NO |
| **W041** | `L88_AdsorberAFlowDistributor` (`P015`) | `L88_CO2RegenerationControllerPLC` (`P043`) | `ANALOG_FEEDBACK` | Adsorber A actual valve feedback | NO |
| **W042** | `L88_AdsorberBFlowDistributor` (`P016`) | `L88_CO2RegenerationControllerPLC` (`P047`) | `ANALOG_FEEDBACK` | Adsorber B actual valve feedback | NO |
| **W043** | `L88_AdsorbentCartridgeArrayA` (`P017`) | `L88_AdsorptionCapacityMonitor` (`P031`) | `DIGITAL_CONTROL` | Adsorber A sorbent loading measurement | NO |
| **W044** | `L88_AdsorbentCartridgeArrayB` (`P018`) | `L88_AdsorptionCapacityMonitor` (`P031`) | `DIGITAL_CONTROL` | Adsorber B sorbent loading measurement | NO |
| **W045** | `L88_AdsorptionCapacityMonitor` (`P031`) | `L88_CO2RegenerationControllerPLC` (`P035`) | `ANALOG_FEEDBACK` | Measured capture capacity qualification | NO |
| **W046** | `L88_CO2RegenerationControllerPLC` (`P039`) | `L88_AdsorberRegenerationHeaterA` (`P019`) | `DIGITAL_CONTROL` | Adsorber A regeneration heater enable | NO |
| **W047** | `L88_CO2RegenerationControllerPLC` (`P043`) | `L88_AdsorberRegenerationHeaterB` (`P020`) | `DIGITAL_CONTROL` | Adsorber B regeneration heater enable | NO |
| **W048** | `L88_AdsorberRegenerationHeaterA` (`P019`) | `L88_CO2RegenerationControllerPLC` (`P047`) | `ANALOG_FEEDBACK` | Adsorber A actual heat feedback | NO |
| **W049** | `L88_AdsorberRegenerationHeaterB` (`P020`) | `L88_CO2RegenerationControllerPLC` (`P051`) | `ANALOG_FEEDBACK` | Adsorber B actual heat feedback | NO |
| **W050** | `L88_CO2RegenerationControllerPLC` (`P037`) | `L88_AdsorberCycleSelectorValveManifold` (`P021`) | `DIGITAL_CONTROL` | Selector valve switching command | NO |
| **W051** | `L88_AdsorberCycleSelectorValveManifold` (`P021`) | `L88_CO2RegenerationControllerPLC` (`P041`) | `ANALOG_FEEDBACK` | Selector valve actual transition feedback | NO |
| **W052** | `L88_CO2RegenerationControllerPLC` (`P045`) | `L88_RegenerationExhaustCondenser` (`P032`) | `DIGITAL_CONTROL` | Exhaust regeneration condenser enable | NO |
| **W053** | `L88_RegenerationExhaustCondenser` (`P032`) | `L88_CO2RegenerationControllerPLC` (`P049`) | `ANALOG_FEEDBACK` | Exhaust condenser active status | NO |
| **W054** | `L88_CO2RegenerationControllerPLC` (`P035`) | `L88_CO2StorageIsolationCanister` (`P033`) | `DIGITAL_CONTROL` | Captured CO2 storage isolation command | NO |
| **W055** | `L88_CO2StorageIsolationCanister` (`P033`) | `L88_CO2RegenerationControllerPLC` (`P039`) | `ANALOG_FEEDBACK` | CO2 canister isolation actual position | NO |
| **W056** | `L88_CO2BreakthroughFaultInjector` (`P034`) | `L88_CO2RegenerationControllerPLC` (`P043`) | `ANALOG_FEEDBACK` | Breakthrough fault injection status | NO |
| **W057** | `L88_AdsorberInletCO2Analyzer` (`P022`) | `L88_AdsorptionCapacityMonitor` (`P030`) | `DIGITAL_CONTROL` | Adsorbent upstream CO2 sample | NO |
| **W058** | `L88_AdsorberOutletCO2Analyzer` (`P026`) | `L88_AdsorptionCapacityMonitor` (`P030`) | `DIGITAL_CONTROL` | Adsorbent downstream CO2 sample | NO |
| **W059** | `L88_CO2RegenerationControllerPLC` (`P049`) | `L88_HabitatEnvironmentalSCADA` (`P217`) | `ANALOG_FEEDBACK` | Measured scrubber cycle health telemetry | NO |
| **W060** | `L88_ECLSSMasterPLC` (`P198`) | `L88_CO2RegenerationControllerPLC` (`P035`) | `DIGITAL_CONTROL` | Master scrubber operating setpoint | NO |
| **W061** | `L88_OxygenGenerationProcessPLC` (`P089`) | `L88_O2WaterElectrolysisChamber` (`P053`) | `DIGITAL_CONTROL` | Water electrolysis generation enable | NO |
| **W062** | `L88_OxygenGenerationProcessPLC` (`P076`) | `L88_O2CellStackA` (`P054`) | `DIGITAL_CONTROL` | Electrolysis cell stack A drive request | NO |
| **W063** | `L88_OxygenGenerationProcessPLC` (`P080`) | `L88_O2CellStackB` (`P055`) | `DIGITAL_CONTROL` | Electrolysis cell stack B drive request | NO |
| **W064** | `L88_O2CellStackA` (`P054`) | `L88_ElectrolyzerCellMonitoringConsole` (`P071`) | `ANALOG_FEEDBACK` | Cell stack A voltage and health | NO |
| **W065** | `L88_O2CellStackB` (`P055`) | `L88_ElectrolyzerCellMonitoringConsole` (`P071`) | `ANALOG_FEEDBACK` | Cell stack B voltage and health | NO |
| **W066** | `L88_ElectrolyzerCellMonitoringConsole` (`P071`) | `L88_OxygenGenerationProcessPLC` (`P086`) | `ANALOG_FEEDBACK` | Individual electrolysis cell diagnostics | NO |
| **W067** | `L88_OxygenGenerationProcessPLC` (`P073`) | `L88_OxygenGasSeparator` (`P056`) | `DIGITAL_CONTROL` | Oxygen product gas separator command | NO |
| **W068** | `L88_OxygenGasSeparator` (`P056`) | `L88_OxygenGenerationProcessPLC` (`P077`) | `ANALOG_FEEDBACK` | Gas separator measured qualification | NO |
| **W069** | `L88_OxygenGenerationProcessPLC` (`P081`) | `L88_HydrogenByproductIsolationModule` (`P058`) | `DIGITAL_CONTROL` | Isolate byproduct stream safety command | NO |
| **W070** | `L88_HydrogenByproductIsolationModule` (`P058`) | `L88_HabitatLifeSupportSafetyPLC` (`P174`) | `ANALOG_FEEDBACK` | Byproduct containment safety status | NO |
| **W071** | `L88_OxygenGenerationProcessPLC` (`P086`) | `L88_OxygenBufferStorageTank` (`P060`) | `DIGITAL_CONTROL` | Oxygen buffer fill request | NO |
| **W072** | `L88_OxygenBufferStorageTank` (`P060`) | `L88_OxygenGenerationProcessPLC` (`P073`) | `ANALOG_FEEDBACK` | Measured oxygen buffer inventory | NO |
| **W073** | `L88_OxygenGenerationProcessPLC` (`P077`) | `L88_OxygenOutputFlowController` (`P061`) | `ANALOG_FEEDBACK` | Oxygen outlet flow demand | NO |
| **W074** | `L88_OxygenOutputFlowController` (`P061`) | `L88_OxygenGenerationProcessPLC` (`P081`) | `ANALOG_FEEDBACK` | Measured delivered oxygen flow | NO |
| **W075** | `L88_OxygenPartialPressureAnalyzer` (`P062`) | `L88_OxygenGenerationProcessPLC` (`P085`) | `ANALOG_FEEDBACK` | Oxygen partial pressure control feedback | NO |
| **W076** | `L88_OxygenOutputQualityMonitor` (`P065`) | `L88_OxygenGenerationProcessPLC` (`P089`) | `ANALOG_FEEDBACK` | Oxygen product quality feedback | NO |
| **W077** | `L88_OxygenGenerationProcessPLC` (`P076`) | `L88_O2GenerationPowerConverter` (`P066`) | `DIGITAL_CONTROL` | Electrolysis power converter current limit | NO |
| **W078** | `L88_O2GenerationPowerConverter` (`P066`) | `L88_OxygenGenerationProcessPLC` (`P080`) | `ANALOG_FEEDBACK` | Measured electrolyzer power and losses | NO |
| **W079** | `L88_OxygenGenerationProcessPLC` (`P084`) | `L88_OxygenGeneratorThermalCoolingLoop` (`P067`) | `DIGITAL_CONTROL` | Oxygen plant thermal cooling command | NO |
| **W080** | `L88_OxygenGeneratorThermalCoolingLoop` (`P067`) | `L88_OxygenGenerationProcessPLC` (`P088`) | `ANALOG_FEEDBACK` | Coolant flow and temperature feedback | NO |
| **W081** | `L88_OxygenGenerationProcessPLC` (`P075`) | `L88_OxygenDeliveryRegulatorManifold` (`P069`) | `DIGITAL_CONTROL` | Oxygen delivery regulation permit | NO |
| **W082** | `L88_OxygenEmergencyBackupReserve` (`P070`) | `L88_HabitatLifeSupportSafetyPLC` (`P165`) | `DIGITAL_CONTROL` | Emergency reserve isolation safety state | NO |
| **W083** | `L88_HumidityRecoveryPLC` (`P106`) | `L88_HumidityCondensingHeatExchanger` (`P090`) | `ANALOG_FEEDBACK` | Humidity condenser process enable | NO |
| **W084** | `L88_HumidityRecoveryPLC` (`P110`) | `L88_CoolingCoilSerpentineAssembly` (`P091`) | `ANALOG_FEEDBACK` | Cooling coil heat exchange command | NO |
| **W085** | `L88_CoolingCoilSerpentineAssembly` (`P091`) | `L88_HumidityRecoveryPLC` (`P098`) | `ANALOG_FEEDBACK` | Cooling circuit actual temperature feedback | NO |
| **W086** | `L88_HumidityCondensingHeatExchanger` (`P090`) | `L88_HumidityRecoveryPLC` (`P102`) | `ANALOG_FEEDBACK` | Measured condensate collection rate | NO |
| **W087** | `L88_HumidityRecoveryPLC` (`P106`) | `L88_CondensateWaterSeparator` (`P092`) | `ANALOG_FEEDBACK` | Condensate separation command | NO |
| **W088** | `L88_CondensateWaterSeparator` (`P092`) | `L88_HumidityRecoveryPLC` (`P110`) | `ANALOG_FEEDBACK` | Separator actual throughput feedback | NO |
| **W089** | `L88_CondensateCollectionReservoir` (`P093`) | `L88_HumidityRecoveryPLC` (`P098`) | `ANALOG_FEEDBACK` | Recovered water tank level | NO |
| **W090** | `L88_HumidityRecoveryPLC` (`P102`) | `L88_ReclaimedWaterPump` (`P094`) | `ANALOG_FEEDBACK` | Reclaimed water pump run command | NO |
| **W091** | `L88_ReclaimedWaterPump` (`P094`) | `L88_HumidityRecoveryPLC` (`P106`) | `ANALOG_FEEDBACK` | Actual reclaimed water pump flow | NO |
| **W092** | `L88_WaterQualityMonitoringStation` (`P095`) | `L88_HumidityRecoveryPLC` (`P110`) | `ANALOG_FEEDBACK` | Reclaimed water quality validity | NO |
| **W093** | `L88_HumidityRecoveryPLC` (`P098`) | `L88_RecoveredWaterFiltrationRack` (`P115`) | `ANALOG_FEEDBACK` | Water cartridge filtration enable | NO |
| **W094** | `L88_RecoveredWaterFiltrationRack` (`P115`) | `L88_HumidityRecoveryPLC` (`P102`) | `ANALOG_FEEDBACK` | Water filtration actual health | NO |
| **W095** | `L88_VaporHumidityProbeArray` (`P113`) | `L88_HumidityRecoveryPLC` (`P106`) | `ANALOG_FEEDBACK` | Compartment humidity actual measurement | NO |
| **W096** | `L88_HumidityRecoveryPLC` (`P110`) | `L88_CoolingValveActuator` (`P114`) | `ANALOG_FEEDBACK` | Condensation cooling valve actuator command | NO |
| **W097** | `L88_CoolingValveActuator` (`P114`) | `L88_HumidityRecoveryPLC` (`P098`) | `ANALOG_FEEDBACK` | Actual cooling valve position feedback | NO |
| **W098** | `L88_FrozenCondenserFaultInjector` (`P096`) | `L88_HumidityRecoveryPLC` (`P102`) | `ANALOG_FEEDBACK` | Frozen condenser diagnostic fault state | NO |
| **W099** | `L88_WaterResourceBudgetAnalyzer` (`P116`) | `L88_HumidityRecoveryPLC` (`P106`) | `ANALOG_FEEDBACK` | Water reserve budget constraint | NO |
| **W100** | `L88_HumidityRecoveryPLC` (`P110`) | `L88_WaterResourceBudgetAnalyzer` (`P117`) | `ANALOG_FEEDBACK` | Measured recovered water inventory | NO |
| **W101** | `L88_WaterResourceBudgetAnalyzer` (`P118`) | `L88_WaterFeedPumpToElectrolyzer` (`P068`) | `ANALOG_FEEDBACK` | Water supply allowance to oxygen plant | NO |
| **W102** | `L88_WaterFeedPumpToElectrolyzer` (`P068`) | `L88_OxygenGenerationProcessPLC` (`P082`) | `ANALOG_FEEDBACK` | Water pump actual feed flow to electrolysis | NO |
| **W103** | `L88_AirQualityEnvironmentalPLC` (`P150`) | `L88_VentilationMainCirculationBlower` (`P120`) | `DIGITAL_CONTROL` | Main circulation blower enable | NO |
| **W104** | `L88_AirQualityEnvironmentalPLC` (`P154`) | `L88_VentilationSecondaryCirculationBlower` (`P122`) | `DIGITAL_CONTROL` | Backup circulation blower enable | NO |
| **W105** | `L88_AirQualityEnvironmentalPLC` (`P158`) | `L88_VariableFrequencyFanDriveA` (`P124`) | `DIGITAL_CONTROL` | Main fan speed request | NO |
| **W106** | `L88_AirQualityEnvironmentalPLC` (`P162`) | `L88_VariableFrequencyFanDriveB` (`P125`) | `DIGITAL_CONTROL` | Backup fan speed request | NO |
| **W107** | `L88_VariableFrequencyFanDriveA` (`P124`) | `L88_VentilationMainCirculationBlower` (`P121`) | `ANALOG_FEEDBACK` | Main fan VFD actual output | NO |
| **W108** | `L88_VariableFrequencyFanDriveB` (`P125`) | `L88_VentilationSecondaryCirculationBlower` (`P123`) | `ANALOG_FEEDBACK` | Backup fan VFD actual output | NO |
| **W109** | `L88_VentilationMainCirculationBlower` (`P120`) | `L88_AirQualityEnvironmentalPLC` (`P154`) | `ANALOG_FEEDBACK` | Measured primary blower speed | NO |
| **W110** | `L88_VentilationSecondaryCirculationBlower` (`P122`) | `L88_AirQualityEnvironmentalPLC` (`P158`) | `ANALOG_FEEDBACK` | Measured standby blower speed | NO |
| **W111** | `L88_AirQualityEnvironmentalPLC` (`P162`) | `L88_PrimaryAirDuctDistributionManifold` (`P126`) | `ANALOG_FEEDBACK` | Main duct distribution flow setpoint | NO |
| **W112** | `L88_PrimaryAirDuctDistributionManifold` (`P126`) | `L88_AirQualityEnvironmentalPLC` (`P152`) | `ANALOG_FEEDBACK` | Actual main duct pressure/flow | NO |
| **W113** | `L88_AirQualityEnvironmentalPLC` (`P156`) | `L88_FourZoneMotorizedDamperBank` (`P128`) | `DIGITAL_CONTROL` | Four-zone motorized damper requests | NO |
| **W114** | `L88_FourZoneMotorizedDamperBank` (`P128`) | `L88_AirQualityEnvironmentalPLC` (`P160`) | `ANALOG_FEEDBACK` | Measured zone damper positions | NO |
| **W115** | `L88_AirQualityEnvironmentalPLC` (`P150`) | `L88_TraceContaminantFilterArray` (`P144`) | `ANALOG_FEEDBACK` | Cabin trace filtration airflow command | NO |
| **W116** | `L88_TraceContaminantFilterArray` (`P144`) | `L88_AirQualityEnvironmentalPLC` (`P154`) | `ANALOG_FEEDBACK` | Measured replaceable filter pressure drop | NO |
| **W117** | `L88_AirFlowVelocityMeasurementRack` (`P146`) | `L88_AirQualityEnvironmentalPLC` (`P158`) | `ANALOG_FEEDBACK` | Measured air velocity after filtration | NO |
| **W118** | `L88_AirQualityEnvironmentalPLC` (`P162`) | `L88_GasMixingAndSamplingStation` (`P148`) | `ANALOG_FEEDBACK` | Compartment gas mixing sampling command | NO |
| **W119** | `L88_GasMixingAndSamplingStation` (`P148`) | `L88_AirQualityEnvironmentalPLC` (`P152`) | `ANALOG_FEEDBACK` | Sample station calibrated gas quality | NO |
| **W120** | `L88_ECLSSMasterPLC` (`P207`) | `L88_AirQualityEnvironmentalPLC` (`P156`) | `DIGITAL_CONTROL` | Habitat occupancy demand ventilation plan | NO |
| **W121** | `L88_HabitatLifeSupportSafetyPLC` (`P183`) | `L88_CabinLeakIsolationManifold` (`P141`) | `SAFETY_INTERLOCK` | Compartment leak isolation authorization | NO |
| **W122** | `L88_CabinLeakIsolationManifold` (`P141`) | `L88_HabitatLifeSupportSafetyPLC` (`P187`) | `ANALOG_FEEDBACK` | Actual leak isolation valve feedback | NO |
| **W123** | `L88_HabitatLifeSupportSafetyPLC` (`P167`) | `L88_CabinPressureRegulator` (`P142`) | `SAFETY_INTERLOCK` | Normal pressure regulator safety permit | NO |
| **W124** | `L88_CabinPressureRegulator` (`P142`) | `L88_HabitatLifeSupportSafetyPLC` (`P171`) | `ANALOG_FEEDBACK` | Actual pressure regulator operation state | NO |
| **W125** | `L88_HabitatMicroLeakFaultInjector` (`P143`) | `L88_HabitatLifeSupportSafetyPLC` (`P175`) | `ANALOG_FEEDBACK` | Micro-leak fault source declaration | NO |
| **W126** | `L88_HabitatPressureVesselSafetyRelief` (`P149`) | `L88_HabitatLifeSupportSafetyPLC` (`P179`) | `SAFETY_INTERLOCK` | Independent relief valve mechanically sensed state | NO |
| **W127** | `L88_IndependentCabinPressureSensor` (`P136`) | `L88_HabitatLifeSupportSafetyPLC` (`P183`) | `ANALOG_FEEDBACK` | Independent backup pressure protector health | NO |
| **W128** | `L88_DifferentialPressureMonitorArray` (`P140`) | `L88_HabitatLifeSupportSafetyPLC` (`P187`) | `SAFETY_INTERLOCK` | Compartments differential pressure safety | NO |
| **W129** | `L88_HydrogenByproductIsolationModule` (`P058`) | `L88_HabitatLifeSupportSafetyPLC` (`P167`) | `SAFETY_INTERLOCK` | Byproduct stream segregation interlock | NO |
| **W130** | `L88_OxygenEmergencyBackupReserve` (`P070`) | `L88_HabitatLifeSupportSafetyPLC` (`P171`) | `SAFETY_INTERLOCK` | Backup oxygen supply isolation state | NO |
| **W131** | `L88_WaterAndEnergySafetyInterlock` (`P216`) | `L88_HabitatLifeSupportSafetyPLC` (`P175`) | `SAFETY_INTERLOCK` | Water resource independent safety inhibit | NO |
| **W132** | `L88_HabitatLifeSupportSafetyPLC` (`P179`) | `L88_WaterAndEnergySafetyInterlock` (`P216`) | `ANALOG_FEEDBACK` | Safety PLC water-energy permit status | NO |
| **W133** | `L88_ECLSSMasterPLC` (`P205`) | `L88_HabitatLifeSupportSafetyPLC` (`P183`) | `SAFETY_INTERLOCK` | Atmosphere supervisor safety run request | NO |
| **W134** | `L88_HabitatLifeSupportSafetyPLC` (`P187`) | `L88_ECLSSMasterPLC` (`P209`) | `ANALOG_FEEDBACK` | Actual independent safety authorization | NO |
| **W135** | `L88_HabitatLifeSupportSafetyPLC` (`P167`) | `L88_EmergencyAtmosphereIsolationConsole` (`P238`) | `SAFETY_INTERLOCK` | Emergency isolation inhibit output | NO |
| **W136** | `L88_EmergencyAtmosphereIsolationConsole` (`P239`) | `L88_HabitatLifeSupportSafetyPLC` (`P171`) | `ANALOG_FEEDBACK` | Emergency station actual stop state | NO |
| **W137** | `L88_HabitatLifeSupportSafetyPLC` (`P175`) | `L88_ECLSSMasterPLC` (`P215`) | `ANALOG_FEEDBACK` | Safety fault latch event report | NO |
| **W138** | `L88_MissionOccupancyDemandSimulator` (`P236`) | `L88_HabitatLifeSupportSafetyPLC` (`P179`) | `ANALOG_FEEDBACK` | Simulated occupancy test safety status | NO |
| **W139** | `L88_FrozenCondenserFaultInjector` (`P096`) | `L88_HabitatLifeSupportSafetyPLC` (`P183`) | `SAFETY_INTERLOCK` | Condenser freeze critical inhibit | NO |
| **W140** | `L88_CO2BreakthroughFaultInjector` (`P034`) | `L88_HabitatLifeSupportSafetyPLC` (`P187`) | `ANALOG_FEEDBACK` | CO2 breakthrough safety condition | NO |
| **W141** | `L88_HabitatLifeSupportSafetyPLC` (`P167`) | `L88_AtmosphericDigitalFlowHologram` (`P234`) | `ANALOG_FEEDBACK` | Safety status for hologram readout | NO |
| **W142** | `L88_HabitatLifeSupportSafetyPLC` (`P171`) | `L88_HabitatEnvironmentalSCADA` (`P218`) | `ANALOG_FEEDBACK` | Safety latched fault SCADA output | NO |
| **W143** | `L88_ECLSSMasterPLC` (`P206`) | `L88_EmergencyAtmosphereIsolationConsole` (`P240`) | `SAFETY_INTERLOCK` | Operator explicit safety reset request | NO |
| **W144** | `L88_EmergencyAtmosphereIsolationConsole` (`P238`) | `L88_HabitatLifeSupportSafetyPLC` (`P176`) | `ANALOG_FEEDBACK` | Operator explicit reset demand feedback | NO |
| **W145** | `L88_CabinPressurePrimaryTransducer` (`P134`) | `L88_HabitatLifeSupportSafetyPLC` (`P180`) | `ANALOG_FEEDBACK` | CRITICAL trusted cabin pressure feedback | YES |
| **W146** | `L88_HabitatLifeSupportSafetyPLC` (`P184`) | `L88_AtmosphereMissionCertificationBeacon` (`P241`) | `SAFETY_INTERLOCK` | Six-objective final safety permission | NO |
| **W147** | `L88_AdsorberInletCO2Analyzer` (`P022`) | `L88_HabitatEnvironmentalSCADA` (`P226`) | `ANALOG_FEEDBACK` | Inlet CO2 analyzers SCADA sampled data | NO |
| **W148** | `L88_AdsorberOutletCO2Analyzer` (`P026`) | `L88_HabitatEnvironmentalSCADA` (`P230`) | `ANALOG_FEEDBACK` | Outlet CO2 analyzers SCADA sampled data | NO |
| **W149** | `L88_AdsorptionCapacityMonitor` (`P028`) | `L88_HabitatEnvironmentalSCADA` (`P217`) | `ANALOG_FEEDBACK` | CO2 capture-bed lifetime telemetry | NO |
| **W150** | `L88_CO2RegenerationControllerPLC` (`P038`) | `L88_HabitatEnvironmentalSCADA` (`P221`) | `ANALOG_FEEDBACK` | Adsorption cycle and regeneration status | NO |
| **W151** | `L88_OxygenBufferStorageTank` (`P060`) | `L88_HabitatEnvironmentalSCADA` (`P225`) | `ANALOG_FEEDBACK` | Oxygen buffer inventory telemetry | NO |
| **W152** | `L88_OxygenPartialPressureAnalyzer` (`P064`) | `L88_HabitatEnvironmentalSCADA` (`P229`) | `ANALOG_FEEDBACK` | Oxygen partial pressure trend | NO |
| **W153** | `L88_OxygenOutputQualityMonitor` (`P065`) | `L88_HabitatEnvironmentalSCADA` (`P233`) | `ANALOG_FEEDBACK` | Oxygen product quality display | NO |
| **W154** | `L88_OxygenGenerationProcessPLC` (`P086`) | `L88_HabitatEnvironmentalSCADA` (`P220`) | `ANALOG_FEEDBACK` | Oxygen generation process status | NO |
| **W155** | `L88_CondensateCollectionReservoir` (`P093`) | `L88_HabitatEnvironmentalSCADA` (`P224`) | `ANALOG_FEEDBACK` | Condensate inventory telemetry | NO |
| **W156** | `L88_HumidityRecoveryPLC` (`P105`) | `L88_HabitatEnvironmentalSCADA` (`P228`) | `ANALOG_FEEDBACK` | Humidity recovery process telemetry | NO |
| **W157** | `L88_WaterResourceBudgetAnalyzer` (`P117`) | `L88_HabitatEnvironmentalSCADA` (`P232`) | `ANALOG_FEEDBACK` | Water reserve forecast telemetry | NO |
| **W158** | `L88_VentilationMainCirculationBlower` (`P121`) | `L88_HabitatEnvironmentalSCADA` (`P219`) | `ANALOG_FEEDBACK` | Main blower actual response trend | NO |
| **W159** | `L88_FourZoneMotorizedDamperBank` (`P128`) | `L88_HabitatEnvironmentalSCADA` (`P223`) | `ANALOG_FEEDBACK` | Compartment damper actual state trend | NO |
| **W160** | `L88_DifferentialPressureMonitorArray` (`P139`) | `L88_HabitatEnvironmentalSCADA` (`P227`) | `ANALOG_FEEDBACK` | Differential pressure survey | NO |
| **W161** | `L88_ECLSSMasterPLC` (`P209`) | `L88_HabitatEnvironmentalSCADA` (`P231`) | `ANALOG_FEEDBACK` | Authoritative life support mission state | NO |
| **W162** | `L88_HabitatEnvironmentalSCADA` (`P218`) | `L88_AtmosphericDigitalFlowHologram` (`P234`) | `ANALOG_FEEDBACK` | Atmosphere digital twin display updates | NO |
| **W163** | `L88_AirSystemDiagnosticsRecorder` (`P235`) | `L88_HabitatEnvironmentalSCADA` (`P222`) | `ANALOG_FEEDBACK` | Process history trend recording | NO |
| **W164** | `L88_HabitatEnvironmentalSCADA` (`P226`) | `L88_AtmosphereMissionCertificationBeacon` (`P241`) | `ANALOG_FEEDBACK` | Objective certification visualization only | NO |

## ANNEX D — ALL 242 ORIGINAL DECORATIVE VISUAL SOCKET ANCHORS

These rows describe source art positions, not existing working `SparkTerminal` nodes. Preserve `P001`–`P242` and their actual owner models.

| AnchorID | Model | WorldX_m | WorldY_m | WorldZ_m | Status |
|---|---|---|---|---|---|
| `P001` | `L88_HabitatAtmosphereQualificationArenaBase` | -0.64 | -0.247 | 0.27 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P002` | `L88_PanoramicOrbitalGardenViewport` | -0.64 | 6.353 | -11.23 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P003` | `L88_HabitatRingGlassBioDome` | -0.64 | 8.053 | -9.53 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P004` | `L88_HabitatSectorA_Crew` | -7.14 | 7.753 | -9.13 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P005` | `L88_HabitatSectorB_Research` | -2.64 | 7.753 | -9.13 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P006` | `L88_HabitatSectorC_Garden` | 1.86 | 7.753 | -9.13 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P007` | `L88_HabitatSectorD_Habitat` | 6.36 | 7.753 | -9.13 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P008` | `L88_FourCompartmentAtmosphereDigitalTwin` | -0.64 | 11.453 | -5.43 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P009` | `L88_PressurizedHabitatServiceBulkhead` | -0.64 | 4.853 | -7.03 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P010` | `L88_AtmosphereAirlockIsolationHatch` | -12.04 | 4.353 | -7.33 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P011` | `L88_LifeSupportEquipmentServiceWalkway` | -0.64 | -0.047 | 0.27 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P012` | `L88_ProcessGasOverheadPipeNetwork` | -0.64 | 5.853 | -2.73 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P013` | `L88_DualAdsorberTowerA` | -11.368 | 3.053 | -0.22 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P014` | `L88_DualAdsorberTowerB` | -8.718 | 3.053 | -0.22 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P015` | `L88_AdsorberAFlowDistributor` | -11.368 | 1.703 | 1.78 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P016` | `L88_AdsorberBFlowDistributor` | -8.718 | 1.703 | 1.78 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P017` | `L88_AdsorbentCartridgeArrayA` | -11.368 | 2.553 | -0.02 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P018` | `L88_AdsorbentCartridgeArrayB` | -8.718 | 2.553 | -0.02 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P019` | `L88_AdsorberRegenerationHeaterA` | -11.368 | 1.553 | -1.52 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P020` | `L88_AdsorberRegenerationHeaterB` | -8.718 | 1.553 | -1.52 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P021` | `L88_AdsorberCycleSelectorValveManifold` | -10.068 | 1.853 | 2.48 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P022` | `L88_AdsorberInletCO2Analyzer` | -12.968 | 1.503 | 5.98 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P023` | `L88_AdsorberInletCO2Analyzer` | -12.876 | 1.503 | 5.98 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P024` | `L88_AdsorberInletCO2Analyzer` | -12.784 | 1.503 | 5.98 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P025` | `L88_AdsorberOutletCO2Analyzer` | -11.648 | 1.503 | 5.98 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P026` | `L88_AdsorberOutletCO2Analyzer` | -11.556 | 1.503 | 5.98 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P027` | `L88_AdsorberOutletCO2Analyzer` | -11.464 | 1.503 | 5.98 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P028` | `L88_AdsorptionCapacityMonitor` | -10.328 | 1.503 | 5.98 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P029` | `L88_AdsorptionCapacityMonitor` | -10.236 | 1.503 | 5.98 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P030` | `L88_AdsorptionCapacityMonitor` | -10.144 | 1.503 | 5.98 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P031` | `L88_AdsorptionCapacityMonitor` | -10.052 | 1.503 | 5.98 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P032` | `L88_RegenerationExhaustCondenser` | -11.068 | 1.303 | 4.28 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P033` | `L88_CO2StorageIsolationCanister` | -9.268 | 1.553 | 4.28 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P034` | `L88_CO2BreakthroughFaultInjector` | -9.008 | 1.503 | 5.98 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P035` | `L88_CO2RegenerationControllerPLC` | -7.688 | 1.503 | 5.98 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P036` | `L88_CO2RegenerationControllerPLC` | -7.596 | 1.503 | 5.98 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P037` | `L88_CO2RegenerationControllerPLC` | -7.504 | 1.503 | 5.98 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P038` | `L88_CO2RegenerationControllerPLC` | -7.412 | 1.503 | 5.98 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P039` | `L88_CO2RegenerationControllerPLC` | -7.32 | 1.503 | 5.98 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P040` | `L88_CO2RegenerationControllerPLC` | -7.228 | 1.503 | 5.98 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P041` | `L88_CO2RegenerationControllerPLC` | -7.136 | 1.503 | 5.98 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P042` | `L88_CO2RegenerationControllerPLC` | -7.044 | 1.503 | 5.98 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P043` | `L88_CO2RegenerationControllerPLC` | -6.952 | 1.503 | 5.98 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P044` | `L88_CO2RegenerationControllerPLC` | -7.688 | 1.608 | 5.98 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P045` | `L88_CO2RegenerationControllerPLC` | -7.596 | 1.608 | 5.98 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P046` | `L88_CO2RegenerationControllerPLC` | -7.504 | 1.608 | 5.98 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P047` | `L88_CO2RegenerationControllerPLC` | -7.412 | 1.608 | 5.98 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P048` | `L88_CO2RegenerationControllerPLC` | -7.32 | 1.608 | 5.98 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P049` | `L88_CO2RegenerationControllerPLC` | -7.228 | 1.608 | 5.98 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P050` | `L88_CO2RegenerationControllerPLC` | -7.136 | 1.608 | 5.98 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P051` | `L88_CO2RegenerationControllerPLC` | -7.044 | 1.608 | 5.98 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P052` | `L88_CO2RegenerationControllerPLC` | -6.952 | 1.608 | 5.98 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P053` | `L88_O2WaterElectrolysisChamber` | -4.368 | 2.803 | -0.67 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P054` | `L88_O2CellStackA` | -5.618 | 2.553 | -0.12 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P055` | `L88_O2CellStackB` | -3.118 | 2.553 | -0.12 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P056` | `L88_OxygenGasSeparator` | -2.018 | 1.853 | 1.48 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P057` | `L88_HydrogenByproductIsolationModule` | -5.868 | 1.603 | 2.18 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P058` | `L88_HydrogenByproductIsolationModule` | -5.776 | 1.603 | 2.18 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P059` | `L88_OxygenBufferStorageTank` | -0.568 | 2.503 | -0.52 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P060` | `L88_OxygenBufferStorageTank` | -0.476 | 2.503 | -0.52 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P061` | `L88_OxygenOutputFlowController` | -6.368 | 1.503 | 5.98 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P062` | `L88_OxygenPartialPressureAnalyzer` | -5.048 | 1.503 | 5.98 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P063` | `L88_OxygenPartialPressureAnalyzer` | -4.956 | 1.503 | 5.98 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P064` | `L88_OxygenPartialPressureAnalyzer` | -4.864 | 1.503 | 5.98 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P065` | `L88_OxygenOutputQualityMonitor` | -3.728 | 1.503 | 5.98 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P066` | `L88_O2GenerationPowerConverter` | -2.408 | 1.503 | 5.98 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P067` | `L88_OxygenGeneratorThermalCoolingLoop` | -2.568 | 1.203 | 3.58 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P068` | `L88_WaterFeedPumpToElectrolyzer` | -4.668 | 0.953 | 3.68 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P069` | `L88_OxygenDeliveryRegulatorManifold` | -0.568 | 1.553 | 2.08 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P070` | `L88_OxygenEmergencyBackupReserve` | 1.032 | 2.153 | 1.58 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P071` | `L88_ElectrolyzerCellMonitoringConsole` | -1.088 | 1.503 | 5.98 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P072` | `L88_ElectrolyzerCellMonitoringConsole` | -0.996 | 1.503 | 5.98 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P073` | `L88_OxygenGenerationProcessPLC` | 0.232 | 1.503 | 5.98 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P074` | `L88_OxygenGenerationProcessPLC` | 0.324 | 1.503 | 5.98 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P075` | `L88_OxygenGenerationProcessPLC` | 0.416 | 1.503 | 5.98 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P076` | `L88_OxygenGenerationProcessPLC` | 0.508 | 1.503 | 5.98 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P077` | `L88_OxygenGenerationProcessPLC` | 0.6 | 1.503 | 5.98 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P078` | `L88_OxygenGenerationProcessPLC` | 0.692 | 1.503 | 5.98 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P079` | `L88_OxygenGenerationProcessPLC` | 0.784 | 1.503 | 5.98 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P080` | `L88_OxygenGenerationProcessPLC` | 0.876 | 1.503 | 5.98 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P081` | `L88_OxygenGenerationProcessPLC` | 0.968 | 1.503 | 5.98 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P082` | `L88_OxygenGenerationProcessPLC` | 0.232 | 1.608 | 5.98 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P083` | `L88_OxygenGenerationProcessPLC` | 0.324 | 1.608 | 5.98 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P084` | `L88_OxygenGenerationProcessPLC` | 0.416 | 1.608 | 5.98 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P085` | `L88_OxygenGenerationProcessPLC` | 0.508 | 1.608 | 5.98 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P086` | `L88_OxygenGenerationProcessPLC` | 0.6 | 1.608 | 5.98 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P087` | `L88_OxygenGenerationProcessPLC` | 0.692 | 1.608 | 5.98 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P088` | `L88_OxygenGenerationProcessPLC` | 0.784 | 1.608 | 5.98 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P089` | `L88_OxygenGenerationProcessPLC` | 0.876 | 1.608 | 5.98 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P090` | `L88_HumidityCondensingHeatExchanger` | 3.632 | 2.053 | -0.82 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P091` | `L88_CoolingCoilSerpentineAssembly` | 3.632 | 2.653 | -0.82 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P092` | `L88_CondensateWaterSeparator` | 5.532 | 1.353 | 1.88 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P093` | `L88_CondensateCollectionReservoir` | 4.932 | 1.453 | 3.58 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P094` | `L88_ReclaimedWaterPump` | 6.532 | 1.253 | 3.63 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P095` | `L88_WaterQualityMonitoringStation` | -12.868 | 1.503 | 8.13 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P096` | `L88_FrozenCondenserFaultInjector` | -11.248 | 1.503 | 8.13 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P097` | `L88_HumidityRecoveryPLC` | -9.628 | 1.503 | 8.13 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P098` | `L88_HumidityRecoveryPLC` | -9.536 | 1.503 | 8.13 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P099` | `L88_HumidityRecoveryPLC` | -9.444 | 1.503 | 8.13 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P100` | `L88_HumidityRecoveryPLC` | -9.352 | 1.503 | 8.13 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P101` | `L88_HumidityRecoveryPLC` | -9.26 | 1.503 | 8.13 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P102` | `L88_HumidityRecoveryPLC` | -9.168 | 1.503 | 8.13 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P103` | `L88_HumidityRecoveryPLC` | -9.076 | 1.503 | 8.13 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P104` | `L88_HumidityRecoveryPLC` | -8.984 | 1.503 | 8.13 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P105` | `L88_HumidityRecoveryPLC` | -8.892 | 1.503 | 8.13 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P106` | `L88_HumidityRecoveryPLC` | -9.628 | 1.608 | 8.13 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P107` | `L88_HumidityRecoveryPLC` | -9.536 | 1.608 | 8.13 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P108` | `L88_HumidityRecoveryPLC` | -9.444 | 1.608 | 8.13 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P109` | `L88_HumidityRecoveryPLC` | -9.352 | 1.608 | 8.13 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P110` | `L88_HumidityRecoveryPLC` | -9.26 | 1.608 | 8.13 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P111` | `L88_HumidityRecoveryPLC` | -9.168 | 1.608 | 8.13 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P112` | `L88_HumidityRecoveryPLC` | -9.076 | 1.608 | 8.13 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P113` | `L88_VaporHumidityProbeArray` | -8.008 | 1.503 | 8.13 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P114` | `L88_CoolingValveActuator` | -6.388 | 1.503 | 8.13 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P115` | `L88_RecoveredWaterFiltrationRack` | 3.632 | 1.503 | 3.78 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P116` | `L88_WaterResourceBudgetAnalyzer` | -4.768 | 1.503 | 8.13 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P117` | `L88_WaterResourceBudgetAnalyzer` | -4.676 | 1.503 | 8.13 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P118` | `L88_WaterResourceBudgetAnalyzer` | -4.584 | 1.503 | 8.13 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P119` | `L88_VentilationMainCirculationBlower` | 8.632 | 1.953 | -0.52 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P120` | `L88_VentilationMainCirculationBlower` | 8.724 | 1.953 | -0.52 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P121` | `L88_VentilationMainCirculationBlower` | 8.816 | 1.953 | -0.52 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P122` | `L88_VentilationSecondaryCirculationBlower` | 10.732 | 1.953 | -0.52 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P123` | `L88_VentilationSecondaryCirculationBlower` | 10.824 | 1.953 | -0.52 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P124` | `L88_VariableFrequencyFanDriveA` | -3.148 | 1.503 | 8.13 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P125` | `L88_VariableFrequencyFanDriveB` | -1.528 | 1.503 | 8.13 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P126` | `L88_PrimaryAirDuctDistributionManifold` | 9.632 | 4.103 | -2.52 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P127` | `L88_FourZoneMotorizedDamperBank` | 9.632 | 3.453 | 1.98 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P128` | `L88_FourZoneMotorizedDamperBank` | 9.724 | 3.453 | 1.98 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P129` | `L88_HabitatA_ZonePressureSensor` | 0.092 | 1.503 | 8.13 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P130` | `L88_HabitatB_ZonePressureSensor` | 1.712 | 1.503 | 8.13 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P131` | `L88_HabitatC_ZonePressureSensor` | 3.332 | 1.503 | 8.13 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P132` | `L88_HabitatD_ZonePressureSensor` | 4.952 | 1.503 | 8.13 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P133` | `L88_CabinPressurePrimaryTransducer` | 6.572 | 1.503 | 8.13 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P134` | `L88_CabinPressurePrimaryTransducer` | 6.664 | 1.503 | 8.13 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P135` | `L88_IndependentCabinPressureSensor` | 8.192 | 1.503 | 8.13 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P136` | `L88_IndependentCabinPressureSensor` | 8.284 | 1.503 | 8.13 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P137` | `L88_IndependentCabinPressureSensor` | 8.376 | 1.503 | 8.13 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P138` | `L88_DifferentialPressureMonitorArray` | 9.812 | 1.503 | 8.13 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P139` | `L88_DifferentialPressureMonitorArray` | 9.904 | 1.503 | 8.13 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P140` | `L88_DifferentialPressureMonitorArray` | 9.996 | 1.503 | 8.13 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P141` | `L88_CabinLeakIsolationManifold` | 9.332 | 1.353 | 4.08 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P142` | `L88_CabinPressureRegulator` | 10.932 | 1.353 | 4.08 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P143` | `L88_HabitatMicroLeakFaultInjector` | -11.768 | 1.653 | 10.18 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P144` | `L88_TraceContaminantFilterArray` | 11.432 | 2.353 | 2.58 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P145` | `L88_AirFlowVelocityMeasurementRack` | -9.938 | 1.653 | 10.18 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P146` | `L88_AirFlowVelocityMeasurementRack` | -9.846 | 1.653 | 10.18 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P147` | `L88_GasMixingAndSamplingStation` | -8.108 | 1.653 | 10.18 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P148` | `L88_GasMixingAndSamplingStation` | -8.016 | 1.653 | 10.18 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P149` | `L88_HabitatPressureVesselSafetyRelief` | 12.832 | 1.853 | 1.48 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P150` | `L88_AirQualityEnvironmentalPLC` | -6.278 | 1.653 | 10.18 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P151` | `L88_AirQualityEnvironmentalPLC` | -6.186 | 1.653 | 10.18 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P152` | `L88_AirQualityEnvironmentalPLC` | -6.094 | 1.653 | 10.18 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P153` | `L88_AirQualityEnvironmentalPLC` | -6.002 | 1.653 | 10.18 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P154` | `L88_AirQualityEnvironmentalPLC` | -5.91 | 1.653 | 10.18 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P155` | `L88_AirQualityEnvironmentalPLC` | -5.818 | 1.653 | 10.18 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P156` | `L88_AirQualityEnvironmentalPLC` | -5.726 | 1.653 | 10.18 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P157` | `L88_AirQualityEnvironmentalPLC` | -5.634 | 1.653 | 10.18 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P158` | `L88_AirQualityEnvironmentalPLC` | -5.542 | 1.653 | 10.18 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P159` | `L88_AirQualityEnvironmentalPLC` | -6.278 | 1.758 | 10.18 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P160` | `L88_AirQualityEnvironmentalPLC` | -6.186 | 1.758 | 10.18 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P161` | `L88_AirQualityEnvironmentalPLC` | -6.094 | 1.758 | 10.18 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P162` | `L88_AirQualityEnvironmentalPLC` | -6.002 | 1.758 | 10.18 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P163` | `L88_AirQualityEnvironmentalPLC` | -5.91 | 1.758 | 10.18 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P164` | `L88_HabitatLifeSupportSafetyPLC` | -4.448 | 1.653 | 10.18 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P165` | `L88_HabitatLifeSupportSafetyPLC` | -4.356 | 1.653 | 10.18 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P166` | `L88_HabitatLifeSupportSafetyPLC` | -4.264 | 1.653 | 10.18 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P167` | `L88_HabitatLifeSupportSafetyPLC` | -4.172 | 1.653 | 10.18 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P168` | `L88_HabitatLifeSupportSafetyPLC` | -4.08 | 1.653 | 10.18 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P169` | `L88_HabitatLifeSupportSafetyPLC` | -3.988 | 1.653 | 10.18 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P170` | `L88_HabitatLifeSupportSafetyPLC` | -3.896 | 1.653 | 10.18 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P171` | `L88_HabitatLifeSupportSafetyPLC` | -3.804 | 1.653 | 10.18 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P172` | `L88_HabitatLifeSupportSafetyPLC` | -3.712 | 1.653 | 10.18 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P173` | `L88_HabitatLifeSupportSafetyPLC` | -4.448 | 1.758 | 10.18 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P174` | `L88_HabitatLifeSupportSafetyPLC` | -4.356 | 1.758 | 10.18 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P175` | `L88_HabitatLifeSupportSafetyPLC` | -4.264 | 1.758 | 10.18 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P176` | `L88_HabitatLifeSupportSafetyPLC` | -4.172 | 1.758 | 10.18 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P177` | `L88_HabitatLifeSupportSafetyPLC` | -4.08 | 1.758 | 10.18 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P178` | `L88_HabitatLifeSupportSafetyPLC` | -3.988 | 1.758 | 10.18 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P179` | `L88_HabitatLifeSupportSafetyPLC` | -3.896 | 1.758 | 10.18 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P180` | `L88_HabitatLifeSupportSafetyPLC` | -3.804 | 1.758 | 10.18 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P181` | `L88_HabitatLifeSupportSafetyPLC` | -3.712 | 1.758 | 10.18 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P182` | `L88_HabitatLifeSupportSafetyPLC` | -4.448 | 1.863 | 10.18 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P183` | `L88_HabitatLifeSupportSafetyPLC` | -4.356 | 1.863 | 10.18 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P184` | `L88_HabitatLifeSupportSafetyPLC` | -4.264 | 1.863 | 10.18 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P185` | `L88_HabitatLifeSupportSafetyPLC` | -4.172 | 1.863 | 10.18 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P186` | `L88_HabitatLifeSupportSafetyPLC` | -4.08 | 1.863 | 10.18 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P187` | `L88_HabitatLifeSupportSafetyPLC` | -3.988 | 1.863 | 10.18 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P188` | `L88_Protected5VControlSupply` | -2.618 | 1.653 | 10.18 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P189` | `L88_Protected5VControlSupply` | -2.526 | 1.653 | 10.18 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P190` | `L88_Protected5VControlSupply` | -2.434 | 1.653 | 10.18 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P191` | `L88_Protected5VControlSupply` | -2.342 | 1.653 | 10.18 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P192` | `L88_Protected5VControlSupply` | -2.25 | 1.653 | 10.18 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P193` | `L88_Protected5VControlSupply` | -2.158 | 1.653 | 10.18 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P194` | `L88_Protected5VControlSupply` | -2.066 | 1.653 | 10.18 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P195` | `L88_Protected5VControlSupply` | -1.974 | 1.653 | 10.18 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P196` | `L88_Protected5VControlSupply` | -1.882 | 1.653 | 10.18 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P197` | `L88_OxygenCO2TelemetryGateway` | -0.788 | 1.653 | 10.18 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P198` | `L88_ECLSSMasterPLC` | 1.042 | 1.653 | 10.18 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P199` | `L88_ECLSSMasterPLC` | 1.134 | 1.653 | 10.18 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P200` | `L88_ECLSSMasterPLC` | 1.226 | 1.653 | 10.18 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P201` | `L88_ECLSSMasterPLC` | 1.318 | 1.653 | 10.18 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P202` | `L88_ECLSSMasterPLC` | 1.41 | 1.653 | 10.18 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P203` | `L88_ECLSSMasterPLC` | 1.502 | 1.653 | 10.18 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P204` | `L88_ECLSSMasterPLC` | 1.594 | 1.653 | 10.18 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P205` | `L88_ECLSSMasterPLC` | 1.686 | 1.653 | 10.18 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P206` | `L88_ECLSSMasterPLC` | 1.778 | 1.653 | 10.18 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P207` | `L88_ECLSSMasterPLC` | 1.042 | 1.758 | 10.18 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P208` | `L88_ECLSSMasterPLC` | 1.134 | 1.758 | 10.18 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P209` | `L88_ECLSSMasterPLC` | 1.226 | 1.758 | 10.18 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P210` | `L88_ECLSSMasterPLC` | 1.318 | 1.758 | 10.18 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P211` | `L88_ECLSSMasterPLC` | 1.41 | 1.758 | 10.18 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P212` | `L88_ECLSSMasterPLC` | 1.502 | 1.758 | 10.18 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P213` | `L88_ECLSSMasterPLC` | 1.594 | 1.758 | 10.18 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P214` | `L88_ECLSSMasterPLC` | 1.686 | 1.758 | 10.18 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P215` | `L88_ECLSSMasterPLC` | 1.778 | 1.758 | 10.18 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P216` | `L88_WaterAndEnergySafetyInterlock` | 2.872 | 1.653 | 10.18 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P217` | `L88_HabitatEnvironmentalSCADA` | -0.368 | 7.253 | -5.27 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P218` | `L88_HabitatEnvironmentalSCADA` | -0.276 | 7.253 | -5.27 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P219` | `L88_HabitatEnvironmentalSCADA` | -0.184 | 7.253 | -5.27 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P220` | `L88_HabitatEnvironmentalSCADA` | -0.092 | 7.253 | -5.27 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P221` | `L88_HabitatEnvironmentalSCADA` | 0.0 | 7.253 | -5.27 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P222` | `L88_HabitatEnvironmentalSCADA` | 0.092 | 7.253 | -5.27 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P223` | `L88_HabitatEnvironmentalSCADA` | 0.184 | 7.253 | -5.27 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P224` | `L88_HabitatEnvironmentalSCADA` | 0.276 | 7.253 | -5.27 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P225` | `L88_HabitatEnvironmentalSCADA` | 0.368 | 7.253 | -5.27 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P226` | `L88_HabitatEnvironmentalSCADA` | -0.368 | 7.358 | -5.27 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P227` | `L88_HabitatEnvironmentalSCADA` | -0.276 | 7.358 | -5.27 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P228` | `L88_HabitatEnvironmentalSCADA` | -0.184 | 7.358 | -5.27 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P229` | `L88_HabitatEnvironmentalSCADA` | -0.092 | 7.358 | -5.27 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P230` | `L88_HabitatEnvironmentalSCADA` | 0.0 | 7.358 | -5.27 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P231` | `L88_HabitatEnvironmentalSCADA` | 0.092 | 7.358 | -5.27 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P232` | `L88_HabitatEnvironmentalSCADA` | 0.184 | 7.358 | -5.27 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P233` | `L88_HabitatEnvironmentalSCADA` | 0.276 | 7.358 | -5.27 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P234` | `L88_AtmosphericDigitalFlowHologram` | 3.932 | 7.653 | -5.32 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P235` | `L88_AirSystemDiagnosticsRecorder` | 4.702 | 1.653 | 10.18 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P236` | `L88_MissionOccupancyDemandSimulator` | 6.532 | 1.653 | 10.18 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P237` | `L88_W145CriticalPressureFeedbackPatchStation` | 8.362 | 1.653 | 10.18 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P238` | `L88_EmergencyAtmosphereIsolationConsole` | 10.192 | 1.653 | 10.18 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P239` | `L88_EmergencyAtmosphereIsolationConsole` | 10.284 | 1.653 | 10.18 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P240` | `L88_EmergencyAtmosphereIsolationConsole` | 10.376 | 1.653 | 10.18 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P241` | `L88_AtmosphereMissionCertificationBeacon` | -0.368 | 11.603 | -5.32 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |
| `P242` | `L88_EnvironmentalInspectionCamera` | 11.832 | 5.553 | -1.82 | MODELED_VISUAL_SOCKET_NOT_FUNCTIONAL_SPARKTERMINAL |

## ANNEX E — ALL 328 PROPOSED INDEPENDENT LOGICAL WIRE CONTACTS

These contacts are `PROPOSED_NOT_ACTUAL_SPARKTERMINAL`, so implement them using the real Project Spark API. Functional port IDs and direction are exact source values.

| WID | End | Model | Visual socket | Proposed functional port | Signal kind | Direction |
|---|---|---|---|---|---|---|
| W001 | Source | `L88_Protected5VControlSupply` | `P189` | `W001_OUT_POWER_CONTROL` | `POWER_CONTROL` | OUTPUT |
| W001 | Target | `L88_ECLSSMasterPLC` | `P199` | `W001_IN_POWER_CONTROL` | `POWER_CONTROL` | INPUT |
| W002 | Source | `L88_Protected5VControlSupply` | `P193` | `W002_OUT_POWER_CONTROL` | `POWER_CONTROL` | OUTPUT |
| W002 | Target | `L88_HabitatLifeSupportSafetyPLC` | `P166` | `W002_IN_POWER_CONTROL` | `POWER_CONTROL` | INPUT |
| W003 | Source | `L88_Protected5VControlSupply` | `P188` | `W003_OUT_POWER_CONTROL` | `POWER_CONTROL` | OUTPUT |
| W003 | Target | `L88_CO2RegenerationControllerPLC` | `P038` | `W003_IN_POWER_CONTROL` | `POWER_CONTROL` | INPUT |
| W004 | Source | `L88_Protected5VControlSupply` | `P192` | `W004_OUT_POWER_CONTROL` | `POWER_CONTROL` | OUTPUT |
| W004 | Target | `L88_OxygenGenerationProcessPLC` | `P077` | `W004_IN_POWER_CONTROL` | `POWER_CONTROL` | INPUT |
| W005 | Source | `L88_Protected5VControlSupply` | `P196` | `W005_OUT_POWER_CONTROL` | `POWER_CONTROL` | OUTPUT |
| W005 | Target | `L88_HumidityRecoveryPLC` | `P102` | `W005_IN_POWER_CONTROL` | `POWER_CONTROL` | INPUT |
| W006 | Source | `L88_Protected5VControlSupply` | `P191` | `W006_OUT_POWER_CONTROL` | `POWER_CONTROL` | OUTPUT |
| W006 | Target | `L88_AirQualityEnvironmentalPLC` | `P156` | `W006_IN_POWER_CONTROL` | `POWER_CONTROL` | INPUT |
| W007 | Source | `L88_Protected5VControlSupply` | `P195` | `W007_OUT_POWER_CONTROL` | `POWER_CONTROL` | OUTPUT |
| W007 | Target | `L88_HabitatEnvironmentalSCADA` | `P224` | `W007_IN_POWER_CONTROL` | `POWER_CONTROL` | INPUT |
| W008 | Source | `L88_Protected5VControlSupply` | `P190` | `W008_OUT_POWER_CONTROL` | `POWER_CONTROL` | OUTPUT |
| W008 | Target | `L88_CabinPressurePrimaryTransducer` | `P133` | `W008_IN_POWER_CONTROL` | `POWER_CONTROL` | INPUT |
| W009 | Source | `L88_Protected5VControlSupply` | `P194` | `W009_OUT_POWER_CONTROL` | `POWER_CONTROL` | OUTPUT |
| W009 | Target | `L88_IndependentCabinPressureSensor` | `P135` | `W009_IN_POWER_CONTROL` | `POWER_CONTROL` | INPUT |
| W010 | Source | `L88_Protected5VControlSupply` | `P189` | `W010_OUT_POWER_CONTROL` | `POWER_CONTROL` | OUTPUT |
| W010 | Target | `L88_OxygenCO2TelemetryGateway` | `P197` | `W010_IN_POWER_CONTROL` | `POWER_CONTROL` | INPUT |
| W011 | Source | `L88_HabitatLifeSupportSafetyPLC` | `P178` | `W011_OUT_GROUND_RETURN` | `GROUND_RETURN` | OUTPUT |
| W011 | Target | `L88_Protected5VControlSupply` | `P193` | `W011_IN_GROUND_RETURN` | `GROUND_RETURN` | INPUT |
| W012 | Source | `L88_ECLSSMasterPLC` | `P213` | `W012_OUT_GROUND_RETURN` | `GROUND_RETURN` | OUTPUT |
| W012 | Target | `L88_Protected5VControlSupply` | `P188` | `W012_IN_GROUND_RETURN` | `GROUND_RETURN` | INPUT |
| W013 | Source | `L88_HabitatA_ZonePressureSensor` | `P129` | `W013_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W013 | Target | `L88_ECLSSMasterPLC` | `P199` | `W013_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W014 | Source | `L88_HabitatB_ZonePressureSensor` | `P130` | `W014_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W014 | Target | `L88_ECLSSMasterPLC` | `P203` | `W014_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W015 | Source | `L88_HabitatC_ZonePressureSensor` | `P131` | `W015_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W015 | Target | `L88_ECLSSMasterPLC` | `P207` | `W015_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W016 | Source | `L88_HabitatD_ZonePressureSensor` | `P132` | `W016_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W016 | Target | `L88_ECLSSMasterPLC` | `P211` | `W016_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W017 | Source | `L88_CabinPressurePrimaryTransducer` | `P133` | `W017_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W017 | Target | `L88_ECLSSMasterPLC` | `P215` | `W017_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W018 | Source | `L88_IndependentCabinPressureSensor` | `P135` | `W018_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W018 | Target | `L88_HabitatLifeSupportSafetyPLC` | `P164` | `W018_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W019 | Source | `L88_DifferentialPressureMonitorArray` | `P139` | `W019_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W019 | Target | `L88_ECLSSMasterPLC` | `P202` | `W019_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W020 | Source | `L88_AdsorberInletCO2Analyzer` | `P024` | `W020_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W020 | Target | `L88_CO2RegenerationControllerPLC` | `P040` | `W020_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W021 | Source | `L88_AdsorberOutletCO2Analyzer` | `P025` | `W021_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W021 | Target | `L88_CO2RegenerationControllerPLC` | `P044` | `W021_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W022 | Source | `L88_OxygenPartialPressureAnalyzer` | `P063` | `W022_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W022 | Target | `L88_OxygenGenerationProcessPLC` | `P081` | `W022_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W023 | Source | `L88_VaporHumidityProbeArray` | `P113` | `W023_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W023 | Target | `L88_HumidityRecoveryPLC` | `P107` | `W023_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W024 | Source | `L88_AirFlowVelocityMeasurementRack` | `P145` | `W024_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W024 | Target | `L88_AirQualityEnvironmentalPLC` | `P163` | `W024_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W025 | Source | `L88_GasMixingAndSamplingStation` | `P148` | `W025_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W025 | Target | `L88_ECLSSMasterPLC` | `P211` | `W025_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W026 | Source | `L88_OxygenCO2TelemetryGateway` | `P197` | `W026_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W026 | Target | `L88_HabitatEnvironmentalSCADA` | `P229` | `W026_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W027 | Source | `L88_ECLSSMasterPLC` | `P198` | `W027_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W027 | Target | `L88_HabitatA_ZonePressureSensor` | `P129` | `W027_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W028 | Source | `L88_ECLSSMasterPLC` | `P202` | `W028_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W028 | Target | `L88_HabitatB_ZonePressureSensor` | `P130` | `W028_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W029 | Source | `L88_ECLSSMasterPLC` | `P206` | `W029_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W029 | Target | `L88_HabitatC_ZonePressureSensor` | `P131` | `W029_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W030 | Source | `L88_ECLSSMasterPLC` | `P210` | `W030_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W030 | Target | `L88_HabitatD_ZonePressureSensor` | `P132` | `W030_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W031 | Source | `L88_HabitatLifeSupportSafetyPLC` | `P180` | `W031_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W031 | Target | `L88_DifferentialPressureMonitorArray` | `P139` | `W031_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W032 | Source | `L88_AirQualityEnvironmentalPLC` | `P160` | `W032_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W032 | Target | `L88_AirFlowVelocityMeasurementRack` | `P146` | `W032_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W033 | Source | `L88_ECLSSMasterPLC` | `P198` | `W033_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W033 | Target | `L88_OxygenPartialPressureAnalyzer` | `P062` | `W033_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W034 | Source | `L88_ECLSSMasterPLC` | `P202` | `W034_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W034 | Target | `L88_AdsorberInletCO2Analyzer` | `P023` | `W034_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W035 | Source | `L88_ECLSSMasterPLC` | `P206` | `W035_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W035 | Target | `L88_AdsorberOutletCO2Analyzer` | `P027` | `W035_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W036 | Source | `L88_IndependentCabinPressureSensor` | `P135` | `W036_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W036 | Target | `L88_HabitatEnvironmentalSCADA` | `P225` | `W036_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W037 | Source | `L88_CO2RegenerationControllerPLC` | `P045` | `W037_OUT_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | OUTPUT |
| W037 | Target | `L88_DualAdsorberTowerA` | `P013` | `W037_IN_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | INPUT |
| W038 | Source | `L88_CO2RegenerationControllerPLC` | `P049` | `W038_OUT_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | OUTPUT |
| W038 | Target | `L88_DualAdsorberTowerB` | `P014` | `W038_IN_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | INPUT |
| W039 | Source | `L88_CO2RegenerationControllerPLC` | `P035` | `W039_OUT_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | OUTPUT |
| W039 | Target | `L88_AdsorberAFlowDistributor` | `P015` | `W039_IN_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | INPUT |
| W040 | Source | `L88_CO2RegenerationControllerPLC` | `P039` | `W040_OUT_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | OUTPUT |
| W040 | Target | `L88_AdsorberBFlowDistributor` | `P016` | `W040_IN_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | INPUT |
| W041 | Source | `L88_AdsorberAFlowDistributor` | `P015` | `W041_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W041 | Target | `L88_CO2RegenerationControllerPLC` | `P043` | `W041_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W042 | Source | `L88_AdsorberBFlowDistributor` | `P016` | `W042_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W042 | Target | `L88_CO2RegenerationControllerPLC` | `P047` | `W042_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W043 | Source | `L88_AdsorbentCartridgeArrayA` | `P017` | `W043_OUT_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | OUTPUT |
| W043 | Target | `L88_AdsorptionCapacityMonitor` | `P031` | `W043_IN_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | INPUT |
| W044 | Source | `L88_AdsorbentCartridgeArrayB` | `P018` | `W044_OUT_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | OUTPUT |
| W044 | Target | `L88_AdsorptionCapacityMonitor` | `P031` | `W044_IN_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | INPUT |
| W045 | Source | `L88_AdsorptionCapacityMonitor` | `P031` | `W045_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W045 | Target | `L88_CO2RegenerationControllerPLC` | `P035` | `W045_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W046 | Source | `L88_CO2RegenerationControllerPLC` | `P039` | `W046_OUT_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | OUTPUT |
| W046 | Target | `L88_AdsorberRegenerationHeaterA` | `P019` | `W046_IN_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | INPUT |
| W047 | Source | `L88_CO2RegenerationControllerPLC` | `P043` | `W047_OUT_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | OUTPUT |
| W047 | Target | `L88_AdsorberRegenerationHeaterB` | `P020` | `W047_IN_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | INPUT |
| W048 | Source | `L88_AdsorberRegenerationHeaterA` | `P019` | `W048_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W048 | Target | `L88_CO2RegenerationControllerPLC` | `P047` | `W048_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W049 | Source | `L88_AdsorberRegenerationHeaterB` | `P020` | `W049_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W049 | Target | `L88_CO2RegenerationControllerPLC` | `P051` | `W049_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W050 | Source | `L88_CO2RegenerationControllerPLC` | `P037` | `W050_OUT_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | OUTPUT |
| W050 | Target | `L88_AdsorberCycleSelectorValveManifold` | `P021` | `W050_IN_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | INPUT |
| W051 | Source | `L88_AdsorberCycleSelectorValveManifold` | `P021` | `W051_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W051 | Target | `L88_CO2RegenerationControllerPLC` | `P041` | `W051_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W052 | Source | `L88_CO2RegenerationControllerPLC` | `P045` | `W052_OUT_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | OUTPUT |
| W052 | Target | `L88_RegenerationExhaustCondenser` | `P032` | `W052_IN_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | INPUT |
| W053 | Source | `L88_RegenerationExhaustCondenser` | `P032` | `W053_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W053 | Target | `L88_CO2RegenerationControllerPLC` | `P049` | `W053_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W054 | Source | `L88_CO2RegenerationControllerPLC` | `P035` | `W054_OUT_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | OUTPUT |
| W054 | Target | `L88_CO2StorageIsolationCanister` | `P033` | `W054_IN_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | INPUT |
| W055 | Source | `L88_CO2StorageIsolationCanister` | `P033` | `W055_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W055 | Target | `L88_CO2RegenerationControllerPLC` | `P039` | `W055_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W056 | Source | `L88_CO2BreakthroughFaultInjector` | `P034` | `W056_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W056 | Target | `L88_CO2RegenerationControllerPLC` | `P043` | `W056_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W057 | Source | `L88_AdsorberInletCO2Analyzer` | `P022` | `W057_OUT_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | OUTPUT |
| W057 | Target | `L88_AdsorptionCapacityMonitor` | `P030` | `W057_IN_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | INPUT |
| W058 | Source | `L88_AdsorberOutletCO2Analyzer` | `P026` | `W058_OUT_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | OUTPUT |
| W058 | Target | `L88_AdsorptionCapacityMonitor` | `P030` | `W058_IN_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | INPUT |
| W059 | Source | `L88_CO2RegenerationControllerPLC` | `P049` | `W059_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W059 | Target | `L88_HabitatEnvironmentalSCADA` | `P217` | `W059_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W060 | Source | `L88_ECLSSMasterPLC` | `P198` | `W060_OUT_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | OUTPUT |
| W060 | Target | `L88_CO2RegenerationControllerPLC` | `P035` | `W060_IN_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | INPUT |
| W061 | Source | `L88_OxygenGenerationProcessPLC` | `P089` | `W061_OUT_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | OUTPUT |
| W061 | Target | `L88_O2WaterElectrolysisChamber` | `P053` | `W061_IN_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | INPUT |
| W062 | Source | `L88_OxygenGenerationProcessPLC` | `P076` | `W062_OUT_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | OUTPUT |
| W062 | Target | `L88_O2CellStackA` | `P054` | `W062_IN_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | INPUT |
| W063 | Source | `L88_OxygenGenerationProcessPLC` | `P080` | `W063_OUT_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | OUTPUT |
| W063 | Target | `L88_O2CellStackB` | `P055` | `W063_IN_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | INPUT |
| W064 | Source | `L88_O2CellStackA` | `P054` | `W064_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W064 | Target | `L88_ElectrolyzerCellMonitoringConsole` | `P071` | `W064_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W065 | Source | `L88_O2CellStackB` | `P055` | `W065_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W065 | Target | `L88_ElectrolyzerCellMonitoringConsole` | `P071` | `W065_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W066 | Source | `L88_ElectrolyzerCellMonitoringConsole` | `P071` | `W066_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W066 | Target | `L88_OxygenGenerationProcessPLC` | `P086` | `W066_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W067 | Source | `L88_OxygenGenerationProcessPLC` | `P073` | `W067_OUT_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | OUTPUT |
| W067 | Target | `L88_OxygenGasSeparator` | `P056` | `W067_IN_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | INPUT |
| W068 | Source | `L88_OxygenGasSeparator` | `P056` | `W068_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W068 | Target | `L88_OxygenGenerationProcessPLC` | `P077` | `W068_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W069 | Source | `L88_OxygenGenerationProcessPLC` | `P081` | `W069_OUT_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | OUTPUT |
| W069 | Target | `L88_HydrogenByproductIsolationModule` | `P058` | `W069_IN_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | INPUT |
| W070 | Source | `L88_HydrogenByproductIsolationModule` | `P058` | `W070_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W070 | Target | `L88_HabitatLifeSupportSafetyPLC` | `P174` | `W070_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W071 | Source | `L88_OxygenGenerationProcessPLC` | `P086` | `W071_OUT_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | OUTPUT |
| W071 | Target | `L88_OxygenBufferStorageTank` | `P060` | `W071_IN_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | INPUT |
| W072 | Source | `L88_OxygenBufferStorageTank` | `P060` | `W072_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W072 | Target | `L88_OxygenGenerationProcessPLC` | `P073` | `W072_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W073 | Source | `L88_OxygenGenerationProcessPLC` | `P077` | `W073_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W073 | Target | `L88_OxygenOutputFlowController` | `P061` | `W073_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W074 | Source | `L88_OxygenOutputFlowController` | `P061` | `W074_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W074 | Target | `L88_OxygenGenerationProcessPLC` | `P081` | `W074_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W075 | Source | `L88_OxygenPartialPressureAnalyzer` | `P062` | `W075_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W075 | Target | `L88_OxygenGenerationProcessPLC` | `P085` | `W075_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W076 | Source | `L88_OxygenOutputQualityMonitor` | `P065` | `W076_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W076 | Target | `L88_OxygenGenerationProcessPLC` | `P089` | `W076_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W077 | Source | `L88_OxygenGenerationProcessPLC` | `P076` | `W077_OUT_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | OUTPUT |
| W077 | Target | `L88_O2GenerationPowerConverter` | `P066` | `W077_IN_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | INPUT |
| W078 | Source | `L88_O2GenerationPowerConverter` | `P066` | `W078_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W078 | Target | `L88_OxygenGenerationProcessPLC` | `P080` | `W078_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W079 | Source | `L88_OxygenGenerationProcessPLC` | `P084` | `W079_OUT_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | OUTPUT |
| W079 | Target | `L88_OxygenGeneratorThermalCoolingLoop` | `P067` | `W079_IN_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | INPUT |
| W080 | Source | `L88_OxygenGeneratorThermalCoolingLoop` | `P067` | `W080_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W080 | Target | `L88_OxygenGenerationProcessPLC` | `P088` | `W080_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W081 | Source | `L88_OxygenGenerationProcessPLC` | `P075` | `W081_OUT_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | OUTPUT |
| W081 | Target | `L88_OxygenDeliveryRegulatorManifold` | `P069` | `W081_IN_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | INPUT |
| W082 | Source | `L88_OxygenEmergencyBackupReserve` | `P070` | `W082_OUT_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | OUTPUT |
| W082 | Target | `L88_HabitatLifeSupportSafetyPLC` | `P165` | `W082_IN_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | INPUT |
| W083 | Source | `L88_HumidityRecoveryPLC` | `P106` | `W083_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W083 | Target | `L88_HumidityCondensingHeatExchanger` | `P090` | `W083_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W084 | Source | `L88_HumidityRecoveryPLC` | `P110` | `W084_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W084 | Target | `L88_CoolingCoilSerpentineAssembly` | `P091` | `W084_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W085 | Source | `L88_CoolingCoilSerpentineAssembly` | `P091` | `W085_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W085 | Target | `L88_HumidityRecoveryPLC` | `P098` | `W085_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W086 | Source | `L88_HumidityCondensingHeatExchanger` | `P090` | `W086_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W086 | Target | `L88_HumidityRecoveryPLC` | `P102` | `W086_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W087 | Source | `L88_HumidityRecoveryPLC` | `P106` | `W087_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W087 | Target | `L88_CondensateWaterSeparator` | `P092` | `W087_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W088 | Source | `L88_CondensateWaterSeparator` | `P092` | `W088_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W088 | Target | `L88_HumidityRecoveryPLC` | `P110` | `W088_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W089 | Source | `L88_CondensateCollectionReservoir` | `P093` | `W089_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W089 | Target | `L88_HumidityRecoveryPLC` | `P098` | `W089_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W090 | Source | `L88_HumidityRecoveryPLC` | `P102` | `W090_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W090 | Target | `L88_ReclaimedWaterPump` | `P094` | `W090_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W091 | Source | `L88_ReclaimedWaterPump` | `P094` | `W091_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W091 | Target | `L88_HumidityRecoveryPLC` | `P106` | `W091_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W092 | Source | `L88_WaterQualityMonitoringStation` | `P095` | `W092_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W092 | Target | `L88_HumidityRecoveryPLC` | `P110` | `W092_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W093 | Source | `L88_HumidityRecoveryPLC` | `P098` | `W093_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W093 | Target | `L88_RecoveredWaterFiltrationRack` | `P115` | `W093_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W094 | Source | `L88_RecoveredWaterFiltrationRack` | `P115` | `W094_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W094 | Target | `L88_HumidityRecoveryPLC` | `P102` | `W094_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W095 | Source | `L88_VaporHumidityProbeArray` | `P113` | `W095_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W095 | Target | `L88_HumidityRecoveryPLC` | `P106` | `W095_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W096 | Source | `L88_HumidityRecoveryPLC` | `P110` | `W096_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W096 | Target | `L88_CoolingValveActuator` | `P114` | `W096_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W097 | Source | `L88_CoolingValveActuator` | `P114` | `W097_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W097 | Target | `L88_HumidityRecoveryPLC` | `P098` | `W097_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W098 | Source | `L88_FrozenCondenserFaultInjector` | `P096` | `W098_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W098 | Target | `L88_HumidityRecoveryPLC` | `P102` | `W098_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W099 | Source | `L88_WaterResourceBudgetAnalyzer` | `P116` | `W099_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W099 | Target | `L88_HumidityRecoveryPLC` | `P106` | `W099_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W100 | Source | `L88_HumidityRecoveryPLC` | `P110` | `W100_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W100 | Target | `L88_WaterResourceBudgetAnalyzer` | `P117` | `W100_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W101 | Source | `L88_WaterResourceBudgetAnalyzer` | `P118` | `W101_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W101 | Target | `L88_WaterFeedPumpToElectrolyzer` | `P068` | `W101_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W102 | Source | `L88_WaterFeedPumpToElectrolyzer` | `P068` | `W102_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W102 | Target | `L88_OxygenGenerationProcessPLC` | `P082` | `W102_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W103 | Source | `L88_AirQualityEnvironmentalPLC` | `P150` | `W103_OUT_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | OUTPUT |
| W103 | Target | `L88_VentilationMainCirculationBlower` | `P120` | `W103_IN_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | INPUT |
| W104 | Source | `L88_AirQualityEnvironmentalPLC` | `P154` | `W104_OUT_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | OUTPUT |
| W104 | Target | `L88_VentilationSecondaryCirculationBlower` | `P122` | `W104_IN_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | INPUT |
| W105 | Source | `L88_AirQualityEnvironmentalPLC` | `P158` | `W105_OUT_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | OUTPUT |
| W105 | Target | `L88_VariableFrequencyFanDriveA` | `P124` | `W105_IN_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | INPUT |
| W106 | Source | `L88_AirQualityEnvironmentalPLC` | `P162` | `W106_OUT_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | OUTPUT |
| W106 | Target | `L88_VariableFrequencyFanDriveB` | `P125` | `W106_IN_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | INPUT |
| W107 | Source | `L88_VariableFrequencyFanDriveA` | `P124` | `W107_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W107 | Target | `L88_VentilationMainCirculationBlower` | `P121` | `W107_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W108 | Source | `L88_VariableFrequencyFanDriveB` | `P125` | `W108_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W108 | Target | `L88_VentilationSecondaryCirculationBlower` | `P123` | `W108_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W109 | Source | `L88_VentilationMainCirculationBlower` | `P120` | `W109_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W109 | Target | `L88_AirQualityEnvironmentalPLC` | `P154` | `W109_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W110 | Source | `L88_VentilationSecondaryCirculationBlower` | `P122` | `W110_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W110 | Target | `L88_AirQualityEnvironmentalPLC` | `P158` | `W110_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W111 | Source | `L88_AirQualityEnvironmentalPLC` | `P162` | `W111_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W111 | Target | `L88_PrimaryAirDuctDistributionManifold` | `P126` | `W111_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W112 | Source | `L88_PrimaryAirDuctDistributionManifold` | `P126` | `W112_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W112 | Target | `L88_AirQualityEnvironmentalPLC` | `P152` | `W112_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W113 | Source | `L88_AirQualityEnvironmentalPLC` | `P156` | `W113_OUT_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | OUTPUT |
| W113 | Target | `L88_FourZoneMotorizedDamperBank` | `P128` | `W113_IN_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | INPUT |
| W114 | Source | `L88_FourZoneMotorizedDamperBank` | `P128` | `W114_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W114 | Target | `L88_AirQualityEnvironmentalPLC` | `P160` | `W114_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W115 | Source | `L88_AirQualityEnvironmentalPLC` | `P150` | `W115_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W115 | Target | `L88_TraceContaminantFilterArray` | `P144` | `W115_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W116 | Source | `L88_TraceContaminantFilterArray` | `P144` | `W116_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W116 | Target | `L88_AirQualityEnvironmentalPLC` | `P154` | `W116_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W117 | Source | `L88_AirFlowVelocityMeasurementRack` | `P146` | `W117_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W117 | Target | `L88_AirQualityEnvironmentalPLC` | `P158` | `W117_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W118 | Source | `L88_AirQualityEnvironmentalPLC` | `P162` | `W118_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W118 | Target | `L88_GasMixingAndSamplingStation` | `P148` | `W118_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W119 | Source | `L88_GasMixingAndSamplingStation` | `P148` | `W119_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W119 | Target | `L88_AirQualityEnvironmentalPLC` | `P152` | `W119_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W120 | Source | `L88_ECLSSMasterPLC` | `P207` | `W120_OUT_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | OUTPUT |
| W120 | Target | `L88_AirQualityEnvironmentalPLC` | `P156` | `W120_IN_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | INPUT |
| W121 | Source | `L88_HabitatLifeSupportSafetyPLC` | `P183` | `W121_OUT_SAFETY_INTERLOCK` | `SAFETY_INTERLOCK` | OUTPUT |
| W121 | Target | `L88_CabinLeakIsolationManifold` | `P141` | `W121_IN_SAFETY_INTERLOCK` | `SAFETY_INTERLOCK` | INPUT |
| W122 | Source | `L88_CabinLeakIsolationManifold` | `P141` | `W122_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W122 | Target | `L88_HabitatLifeSupportSafetyPLC` | `P187` | `W122_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W123 | Source | `L88_HabitatLifeSupportSafetyPLC` | `P167` | `W123_OUT_SAFETY_INTERLOCK` | `SAFETY_INTERLOCK` | OUTPUT |
| W123 | Target | `L88_CabinPressureRegulator` | `P142` | `W123_IN_SAFETY_INTERLOCK` | `SAFETY_INTERLOCK` | INPUT |
| W124 | Source | `L88_CabinPressureRegulator` | `P142` | `W124_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W124 | Target | `L88_HabitatLifeSupportSafetyPLC` | `P171` | `W124_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W125 | Source | `L88_HabitatMicroLeakFaultInjector` | `P143` | `W125_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W125 | Target | `L88_HabitatLifeSupportSafetyPLC` | `P175` | `W125_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W126 | Source | `L88_HabitatPressureVesselSafetyRelief` | `P149` | `W126_OUT_SAFETY_INTERLOCK` | `SAFETY_INTERLOCK` | OUTPUT |
| W126 | Target | `L88_HabitatLifeSupportSafetyPLC` | `P179` | `W126_IN_SAFETY_INTERLOCK` | `SAFETY_INTERLOCK` | INPUT |
| W127 | Source | `L88_IndependentCabinPressureSensor` | `P136` | `W127_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W127 | Target | `L88_HabitatLifeSupportSafetyPLC` | `P183` | `W127_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W128 | Source | `L88_DifferentialPressureMonitorArray` | `P140` | `W128_OUT_SAFETY_INTERLOCK` | `SAFETY_INTERLOCK` | OUTPUT |
| W128 | Target | `L88_HabitatLifeSupportSafetyPLC` | `P187` | `W128_IN_SAFETY_INTERLOCK` | `SAFETY_INTERLOCK` | INPUT |
| W129 | Source | `L88_HydrogenByproductIsolationModule` | `P058` | `W129_OUT_SAFETY_INTERLOCK` | `SAFETY_INTERLOCK` | OUTPUT |
| W129 | Target | `L88_HabitatLifeSupportSafetyPLC` | `P167` | `W129_IN_SAFETY_INTERLOCK` | `SAFETY_INTERLOCK` | INPUT |
| W130 | Source | `L88_OxygenEmergencyBackupReserve` | `P070` | `W130_OUT_SAFETY_INTERLOCK` | `SAFETY_INTERLOCK` | OUTPUT |
| W130 | Target | `L88_HabitatLifeSupportSafetyPLC` | `P171` | `W130_IN_SAFETY_INTERLOCK` | `SAFETY_INTERLOCK` | INPUT |
| W131 | Source | `L88_WaterAndEnergySafetyInterlock` | `P216` | `W131_OUT_SAFETY_INTERLOCK` | `SAFETY_INTERLOCK` | OUTPUT |
| W131 | Target | `L88_HabitatLifeSupportSafetyPLC` | `P175` | `W131_IN_SAFETY_INTERLOCK` | `SAFETY_INTERLOCK` | INPUT |
| W132 | Source | `L88_HabitatLifeSupportSafetyPLC` | `P179` | `W132_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W132 | Target | `L88_WaterAndEnergySafetyInterlock` | `P216` | `W132_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W133 | Source | `L88_ECLSSMasterPLC` | `P205` | `W133_OUT_SAFETY_INTERLOCK` | `SAFETY_INTERLOCK` | OUTPUT |
| W133 | Target | `L88_HabitatLifeSupportSafetyPLC` | `P183` | `W133_IN_SAFETY_INTERLOCK` | `SAFETY_INTERLOCK` | INPUT |
| W134 | Source | `L88_HabitatLifeSupportSafetyPLC` | `P187` | `W134_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W134 | Target | `L88_ECLSSMasterPLC` | `P209` | `W134_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W135 | Source | `L88_HabitatLifeSupportSafetyPLC` | `P167` | `W135_OUT_SAFETY_INTERLOCK` | `SAFETY_INTERLOCK` | OUTPUT |
| W135 | Target | `L88_EmergencyAtmosphereIsolationConsole` | `P238` | `W135_IN_SAFETY_INTERLOCK` | `SAFETY_INTERLOCK` | INPUT |
| W136 | Source | `L88_EmergencyAtmosphereIsolationConsole` | `P239` | `W136_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W136 | Target | `L88_HabitatLifeSupportSafetyPLC` | `P171` | `W136_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W137 | Source | `L88_HabitatLifeSupportSafetyPLC` | `P175` | `W137_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W137 | Target | `L88_ECLSSMasterPLC` | `P215` | `W137_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W138 | Source | `L88_MissionOccupancyDemandSimulator` | `P236` | `W138_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W138 | Target | `L88_HabitatLifeSupportSafetyPLC` | `P179` | `W138_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W139 | Source | `L88_FrozenCondenserFaultInjector` | `P096` | `W139_OUT_SAFETY_INTERLOCK` | `SAFETY_INTERLOCK` | OUTPUT |
| W139 | Target | `L88_HabitatLifeSupportSafetyPLC` | `P183` | `W139_IN_SAFETY_INTERLOCK` | `SAFETY_INTERLOCK` | INPUT |
| W140 | Source | `L88_CO2BreakthroughFaultInjector` | `P034` | `W140_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W140 | Target | `L88_HabitatLifeSupportSafetyPLC` | `P187` | `W140_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W141 | Source | `L88_HabitatLifeSupportSafetyPLC` | `P167` | `W141_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W141 | Target | `L88_AtmosphericDigitalFlowHologram` | `P234` | `W141_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W142 | Source | `L88_HabitatLifeSupportSafetyPLC` | `P171` | `W142_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W142 | Target | `L88_HabitatEnvironmentalSCADA` | `P218` | `W142_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W143 | Source | `L88_ECLSSMasterPLC` | `P206` | `W143_OUT_SAFETY_INTERLOCK` | `SAFETY_INTERLOCK` | OUTPUT |
| W143 | Target | `L88_EmergencyAtmosphereIsolationConsole` | `P240` | `W143_IN_SAFETY_INTERLOCK` | `SAFETY_INTERLOCK` | INPUT |
| W144 | Source | `L88_EmergencyAtmosphereIsolationConsole` | `P238` | `W144_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W144 | Target | `L88_HabitatLifeSupportSafetyPLC` | `P176` | `W144_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W145 | Source | `L88_CabinPressurePrimaryTransducer` | `P134` | `TRUSTED_CABIN_PRESSURE_OUT` | `ANALOG_FEEDBACK` | OUTPUT |
| W145 | Target | `L88_HabitatLifeSupportSafetyPLC` | `P180` | `TRUSTED_CABIN_PRESSURE_IN` | `ANALOG_FEEDBACK` | INPUT |
| W146 | Source | `L88_HabitatLifeSupportSafetyPLC` | `P184` | `W146_OUT_SAFETY_INTERLOCK` | `SAFETY_INTERLOCK` | OUTPUT |
| W146 | Target | `L88_AtmosphereMissionCertificationBeacon` | `P241` | `W146_IN_SAFETY_INTERLOCK` | `SAFETY_INTERLOCK` | INPUT |
| W147 | Source | `L88_AdsorberInletCO2Analyzer` | `P022` | `W147_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W147 | Target | `L88_HabitatEnvironmentalSCADA` | `P226` | `W147_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W148 | Source | `L88_AdsorberOutletCO2Analyzer` | `P026` | `W148_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W148 | Target | `L88_HabitatEnvironmentalSCADA` | `P230` | `W148_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W149 | Source | `L88_AdsorptionCapacityMonitor` | `P028` | `W149_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W149 | Target | `L88_HabitatEnvironmentalSCADA` | `P217` | `W149_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W150 | Source | `L88_CO2RegenerationControllerPLC` | `P038` | `W150_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W150 | Target | `L88_HabitatEnvironmentalSCADA` | `P221` | `W150_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W151 | Source | `L88_OxygenBufferStorageTank` | `P060` | `W151_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W151 | Target | `L88_HabitatEnvironmentalSCADA` | `P225` | `W151_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W152 | Source | `L88_OxygenPartialPressureAnalyzer` | `P064` | `W152_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W152 | Target | `L88_HabitatEnvironmentalSCADA` | `P229` | `W152_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W153 | Source | `L88_OxygenOutputQualityMonitor` | `P065` | `W153_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W153 | Target | `L88_HabitatEnvironmentalSCADA` | `P233` | `W153_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W154 | Source | `L88_OxygenGenerationProcessPLC` | `P086` | `W154_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W154 | Target | `L88_HabitatEnvironmentalSCADA` | `P220` | `W154_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W155 | Source | `L88_CondensateCollectionReservoir` | `P093` | `W155_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W155 | Target | `L88_HabitatEnvironmentalSCADA` | `P224` | `W155_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W156 | Source | `L88_HumidityRecoveryPLC` | `P105` | `W156_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W156 | Target | `L88_HabitatEnvironmentalSCADA` | `P228` | `W156_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W157 | Source | `L88_WaterResourceBudgetAnalyzer` | `P117` | `W157_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W157 | Target | `L88_HabitatEnvironmentalSCADA` | `P232` | `W157_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W158 | Source | `L88_VentilationMainCirculationBlower` | `P121` | `W158_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W158 | Target | `L88_HabitatEnvironmentalSCADA` | `P219` | `W158_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W159 | Source | `L88_FourZoneMotorizedDamperBank` | `P128` | `W159_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W159 | Target | `L88_HabitatEnvironmentalSCADA` | `P223` | `W159_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W160 | Source | `L88_DifferentialPressureMonitorArray` | `P139` | `W160_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W160 | Target | `L88_HabitatEnvironmentalSCADA` | `P227` | `W160_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W161 | Source | `L88_ECLSSMasterPLC` | `P209` | `W161_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W161 | Target | `L88_HabitatEnvironmentalSCADA` | `P231` | `W161_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W162 | Source | `L88_HabitatEnvironmentalSCADA` | `P218` | `W162_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W162 | Target | `L88_AtmosphericDigitalFlowHologram` | `P234` | `W162_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W163 | Source | `L88_AirSystemDiagnosticsRecorder` | `P235` | `W163_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W163 | Target | `L88_HabitatEnvironmentalSCADA` | `P222` | `W163_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |
| W164 | Source | `L88_HabitatEnvironmentalSCADA` | `P226` | `W164_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT |
| W164 | Target | `L88_AtmosphereMissionCertificationBeacon` | `P241` | `W164_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT |

## ANNEX F — ALL 108 REUSED SOCKET / SIGNAL RECONCILIATION RECORDS

**62** of these 108 reused anchor records have multiple different signal kinds. Preserve original socket IDs and create separate functional contacts where necessary; `Requirement` is from the source.

| Original anchor | Owner model | Logical contact count | Signal kinds | Mixed kinds? | Source reconciliation requirement |
|---|---|---:|---|:---:|---|
| `P189` | `L88_Protected5VControlSupply` | 2 | `POWER_CONTROL` | False | `RECONCILE_SPLIT_LOGICAL_PORTS_NO_CROSS_DOMAIN_SHORT` |
| `P199` | `L88_ECLSSMasterPLC` | 2 | `ANALOG_FEEDBACK;POWER_CONTROL` | True | `RECONCILE_SPLIT_LOGICAL_PORTS_NO_CROSS_DOMAIN_SHORT` |
| `P193` | `L88_Protected5VControlSupply` | 2 | `GROUND_RETURN;POWER_CONTROL` | True | `RECONCILE_SPLIT_LOGICAL_PORTS_NO_CROSS_DOMAIN_SHORT` |
| `P188` | `L88_Protected5VControlSupply` | 2 | `GROUND_RETURN;POWER_CONTROL` | True | `RECONCILE_SPLIT_LOGICAL_PORTS_NO_CROSS_DOMAIN_SHORT` |
| `P038` | `L88_CO2RegenerationControllerPLC` | 2 | `ANALOG_FEEDBACK;POWER_CONTROL` | True | `RECONCILE_SPLIT_LOGICAL_PORTS_NO_CROSS_DOMAIN_SHORT` |
| `P077` | `L88_OxygenGenerationProcessPLC` | 3 | `ANALOG_FEEDBACK;POWER_CONTROL` | True | `RECONCILE_SPLIT_LOGICAL_PORTS_NO_CROSS_DOMAIN_SHORT` |
| `P102` | `L88_HumidityRecoveryPLC` | 5 | `ANALOG_FEEDBACK;POWER_CONTROL` | True | `RECONCILE_SPLIT_LOGICAL_PORTS_NO_CROSS_DOMAIN_SHORT` |
| `P156` | `L88_AirQualityEnvironmentalPLC` | 3 | `DIGITAL_CONTROL;POWER_CONTROL` | True | `RECONCILE_SPLIT_LOGICAL_PORTS_NO_CROSS_DOMAIN_SHORT` |
| `P224` | `L88_HabitatEnvironmentalSCADA` | 2 | `ANALOG_FEEDBACK;POWER_CONTROL` | True | `RECONCILE_SPLIT_LOGICAL_PORTS_NO_CROSS_DOMAIN_SHORT` |
| `P133` | `L88_CabinPressurePrimaryTransducer` | 2 | `ANALOG_FEEDBACK;POWER_CONTROL` | True | `RECONCILE_SPLIT_LOGICAL_PORTS_NO_CROSS_DOMAIN_SHORT` |
| `P135` | `L88_IndependentCabinPressureSensor` | 3 | `ANALOG_FEEDBACK;POWER_CONTROL` | True | `RECONCILE_SPLIT_LOGICAL_PORTS_NO_CROSS_DOMAIN_SHORT` |
| `P197` | `L88_OxygenCO2TelemetryGateway` | 2 | `ANALOG_FEEDBACK;POWER_CONTROL` | True | `RECONCILE_SPLIT_LOGICAL_PORTS_NO_CROSS_DOMAIN_SHORT` |
| `P129` | `L88_HabitatA_ZonePressureSensor` | 2 | `ANALOG_FEEDBACK` | False | `RECONCILE_SPLIT_LOGICAL_PORTS_NO_CROSS_DOMAIN_SHORT` |
| `P130` | `L88_HabitatB_ZonePressureSensor` | 2 | `ANALOG_FEEDBACK` | False | `RECONCILE_SPLIT_LOGICAL_PORTS_NO_CROSS_DOMAIN_SHORT` |
| `P131` | `L88_HabitatC_ZonePressureSensor` | 2 | `ANALOG_FEEDBACK` | False | `RECONCILE_SPLIT_LOGICAL_PORTS_NO_CROSS_DOMAIN_SHORT` |
| `P207` | `L88_ECLSSMasterPLC` | 2 | `ANALOG_FEEDBACK;DIGITAL_CONTROL` | True | `RECONCILE_SPLIT_LOGICAL_PORTS_NO_CROSS_DOMAIN_SHORT` |
| `P132` | `L88_HabitatD_ZonePressureSensor` | 2 | `ANALOG_FEEDBACK` | False | `RECONCILE_SPLIT_LOGICAL_PORTS_NO_CROSS_DOMAIN_SHORT` |
| `P211` | `L88_ECLSSMasterPLC` | 2 | `ANALOG_FEEDBACK` | False | `RECONCILE_SPLIT_LOGICAL_PORTS_NO_CROSS_DOMAIN_SHORT` |
| `P215` | `L88_ECLSSMasterPLC` | 2 | `ANALOG_FEEDBACK` | False | `RECONCILE_SPLIT_LOGICAL_PORTS_NO_CROSS_DOMAIN_SHORT` |
| `P139` | `L88_DifferentialPressureMonitorArray` | 3 | `ANALOG_FEEDBACK` | False | `RECONCILE_SPLIT_LOGICAL_PORTS_NO_CROSS_DOMAIN_SHORT` |
| `P202` | `L88_ECLSSMasterPLC` | 3 | `ANALOG_FEEDBACK` | False | `RECONCILE_SPLIT_LOGICAL_PORTS_NO_CROSS_DOMAIN_SHORT` |
| `P081` | `L88_OxygenGenerationProcessPLC` | 3 | `ANALOG_FEEDBACK;DIGITAL_CONTROL` | True | `RECONCILE_SPLIT_LOGICAL_PORTS_NO_CROSS_DOMAIN_SHORT` |
| `P113` | `L88_VaporHumidityProbeArray` | 2 | `ANALOG_FEEDBACK` | False | `RECONCILE_SPLIT_LOGICAL_PORTS_NO_CROSS_DOMAIN_SHORT` |
| `P148` | `L88_GasMixingAndSamplingStation` | 3 | `ANALOG_FEEDBACK` | False | `RECONCILE_SPLIT_LOGICAL_PORTS_NO_CROSS_DOMAIN_SHORT` |
| `P229` | `L88_HabitatEnvironmentalSCADA` | 2 | `ANALOG_FEEDBACK` | False | `RECONCILE_SPLIT_LOGICAL_PORTS_NO_CROSS_DOMAIN_SHORT` |
| `P198` | `L88_ECLSSMasterPLC` | 3 | `ANALOG_FEEDBACK;DIGITAL_CONTROL` | True | `RECONCILE_SPLIT_LOGICAL_PORTS_NO_CROSS_DOMAIN_SHORT` |
| `P206` | `L88_ECLSSMasterPLC` | 3 | `ANALOG_FEEDBACK;SAFETY_INTERLOCK` | True | `RECONCILE_SPLIT_LOGICAL_PORTS_NO_CROSS_DOMAIN_SHORT` |
| `P180` | `L88_HabitatLifeSupportSafetyPLC` | 2 | `ANALOG_FEEDBACK` | False | `RECONCILE_SPLIT_LOGICAL_PORTS_NO_CROSS_DOMAIN_SHORT` |
| `P160` | `L88_AirQualityEnvironmentalPLC` | 2 | `ANALOG_FEEDBACK` | False | `RECONCILE_SPLIT_LOGICAL_PORTS_NO_CROSS_DOMAIN_SHORT` |
| `P146` | `L88_AirFlowVelocityMeasurementRack` | 2 | `ANALOG_FEEDBACK` | False | `RECONCILE_SPLIT_LOGICAL_PORTS_NO_CROSS_DOMAIN_SHORT` |
| `P062` | `L88_OxygenPartialPressureAnalyzer` | 2 | `ANALOG_FEEDBACK` | False | `RECONCILE_SPLIT_LOGICAL_PORTS_NO_CROSS_DOMAIN_SHORT` |
| `P225` | `L88_HabitatEnvironmentalSCADA` | 2 | `ANALOG_FEEDBACK` | False | `RECONCILE_SPLIT_LOGICAL_PORTS_NO_CROSS_DOMAIN_SHORT` |
| `P045` | `L88_CO2RegenerationControllerPLC` | 2 | `DIGITAL_CONTROL` | False | `RECONCILE_SPLIT_LOGICAL_PORTS_NO_CROSS_DOMAIN_SHORT` |
| `P049` | `L88_CO2RegenerationControllerPLC` | 3 | `ANALOG_FEEDBACK;DIGITAL_CONTROL` | True | `RECONCILE_SPLIT_LOGICAL_PORTS_NO_CROSS_DOMAIN_SHORT` |
| `P035` | `L88_CO2RegenerationControllerPLC` | 4 | `ANALOG_FEEDBACK;DIGITAL_CONTROL` | True | `RECONCILE_SPLIT_LOGICAL_PORTS_NO_CROSS_DOMAIN_SHORT` |
| `P015` | `L88_AdsorberAFlowDistributor` | 2 | `ANALOG_FEEDBACK;DIGITAL_CONTROL` | True | `RECONCILE_SPLIT_LOGICAL_PORTS_NO_CROSS_DOMAIN_SHORT` |
| `P039` | `L88_CO2RegenerationControllerPLC` | 3 | `ANALOG_FEEDBACK;DIGITAL_CONTROL` | True | `RECONCILE_SPLIT_LOGICAL_PORTS_NO_CROSS_DOMAIN_SHORT` |
| `P016` | `L88_AdsorberBFlowDistributor` | 2 | `ANALOG_FEEDBACK;DIGITAL_CONTROL` | True | `RECONCILE_SPLIT_LOGICAL_PORTS_NO_CROSS_DOMAIN_SHORT` |
| `P043` | `L88_CO2RegenerationControllerPLC` | 3 | `ANALOG_FEEDBACK;DIGITAL_CONTROL` | True | `RECONCILE_SPLIT_LOGICAL_PORTS_NO_CROSS_DOMAIN_SHORT` |
| `P047` | `L88_CO2RegenerationControllerPLC` | 2 | `ANALOG_FEEDBACK` | False | `RECONCILE_SPLIT_LOGICAL_PORTS_NO_CROSS_DOMAIN_SHORT` |
| `P031` | `L88_AdsorptionCapacityMonitor` | 3 | `ANALOG_FEEDBACK;DIGITAL_CONTROL` | True | `RECONCILE_SPLIT_LOGICAL_PORTS_NO_CROSS_DOMAIN_SHORT` |
| `P019` | `L88_AdsorberRegenerationHeaterA` | 2 | `ANALOG_FEEDBACK;DIGITAL_CONTROL` | True | `RECONCILE_SPLIT_LOGICAL_PORTS_NO_CROSS_DOMAIN_SHORT` |
| `P020` | `L88_AdsorberRegenerationHeaterB` | 2 | `ANALOG_FEEDBACK;DIGITAL_CONTROL` | True | `RECONCILE_SPLIT_LOGICAL_PORTS_NO_CROSS_DOMAIN_SHORT` |
| `P021` | `L88_AdsorberCycleSelectorValveManifold` | 2 | `ANALOG_FEEDBACK;DIGITAL_CONTROL` | True | `RECONCILE_SPLIT_LOGICAL_PORTS_NO_CROSS_DOMAIN_SHORT` |
| `P032` | `L88_RegenerationExhaustCondenser` | 2 | `ANALOG_FEEDBACK;DIGITAL_CONTROL` | True | `RECONCILE_SPLIT_LOGICAL_PORTS_NO_CROSS_DOMAIN_SHORT` |
| `P033` | `L88_CO2StorageIsolationCanister` | 2 | `ANALOG_FEEDBACK;DIGITAL_CONTROL` | True | `RECONCILE_SPLIT_LOGICAL_PORTS_NO_CROSS_DOMAIN_SHORT` |
| `P034` | `L88_CO2BreakthroughFaultInjector` | 2 | `ANALOG_FEEDBACK` | False | `RECONCILE_SPLIT_LOGICAL_PORTS_NO_CROSS_DOMAIN_SHORT` |
| `P022` | `L88_AdsorberInletCO2Analyzer` | 2 | `ANALOG_FEEDBACK;DIGITAL_CONTROL` | True | `RECONCILE_SPLIT_LOGICAL_PORTS_NO_CROSS_DOMAIN_SHORT` |
| `P030` | `L88_AdsorptionCapacityMonitor` | 2 | `DIGITAL_CONTROL` | False | `RECONCILE_SPLIT_LOGICAL_PORTS_NO_CROSS_DOMAIN_SHORT` |
| `P026` | `L88_AdsorberOutletCO2Analyzer` | 2 | `ANALOG_FEEDBACK;DIGITAL_CONTROL` | True | `RECONCILE_SPLIT_LOGICAL_PORTS_NO_CROSS_DOMAIN_SHORT` |
| `P217` | `L88_HabitatEnvironmentalSCADA` | 2 | `ANALOG_FEEDBACK` | False | `RECONCILE_SPLIT_LOGICAL_PORTS_NO_CROSS_DOMAIN_SHORT` |
| `P089` | `L88_OxygenGenerationProcessPLC` | 2 | `ANALOG_FEEDBACK;DIGITAL_CONTROL` | True | `RECONCILE_SPLIT_LOGICAL_PORTS_NO_CROSS_DOMAIN_SHORT` |
| `P076` | `L88_OxygenGenerationProcessPLC` | 2 | `DIGITAL_CONTROL` | False | `RECONCILE_SPLIT_LOGICAL_PORTS_NO_CROSS_DOMAIN_SHORT` |
| `P054` | `L88_O2CellStackA` | 2 | `ANALOG_FEEDBACK;DIGITAL_CONTROL` | True | `RECONCILE_SPLIT_LOGICAL_PORTS_NO_CROSS_DOMAIN_SHORT` |
| `P080` | `L88_OxygenGenerationProcessPLC` | 2 | `ANALOG_FEEDBACK;DIGITAL_CONTROL` | True | `RECONCILE_SPLIT_LOGICAL_PORTS_NO_CROSS_DOMAIN_SHORT` |
| `P055` | `L88_O2CellStackB` | 2 | `ANALOG_FEEDBACK;DIGITAL_CONTROL` | True | `RECONCILE_SPLIT_LOGICAL_PORTS_NO_CROSS_DOMAIN_SHORT` |
| `P071` | `L88_ElectrolyzerCellMonitoringConsole` | 3 | `ANALOG_FEEDBACK` | False | `RECONCILE_SPLIT_LOGICAL_PORTS_NO_CROSS_DOMAIN_SHORT` |
| `P086` | `L88_OxygenGenerationProcessPLC` | 3 | `ANALOG_FEEDBACK;DIGITAL_CONTROL` | True | `RECONCILE_SPLIT_LOGICAL_PORTS_NO_CROSS_DOMAIN_SHORT` |
| `P073` | `L88_OxygenGenerationProcessPLC` | 2 | `ANALOG_FEEDBACK;DIGITAL_CONTROL` | True | `RECONCILE_SPLIT_LOGICAL_PORTS_NO_CROSS_DOMAIN_SHORT` |
| `P056` | `L88_OxygenGasSeparator` | 2 | `ANALOG_FEEDBACK;DIGITAL_CONTROL` | True | `RECONCILE_SPLIT_LOGICAL_PORTS_NO_CROSS_DOMAIN_SHORT` |
| `P058` | `L88_HydrogenByproductIsolationModule` | 3 | `ANALOG_FEEDBACK;DIGITAL_CONTROL;SAFETY_INTERLOCK` | True | `RECONCILE_SPLIT_LOGICAL_PORTS_NO_CROSS_DOMAIN_SHORT` |
| `P060` | `L88_OxygenBufferStorageTank` | 3 | `ANALOG_FEEDBACK;DIGITAL_CONTROL` | True | `RECONCILE_SPLIT_LOGICAL_PORTS_NO_CROSS_DOMAIN_SHORT` |
| `P061` | `L88_OxygenOutputFlowController` | 2 | `ANALOG_FEEDBACK` | False | `RECONCILE_SPLIT_LOGICAL_PORTS_NO_CROSS_DOMAIN_SHORT` |
| `P065` | `L88_OxygenOutputQualityMonitor` | 2 | `ANALOG_FEEDBACK` | False | `RECONCILE_SPLIT_LOGICAL_PORTS_NO_CROSS_DOMAIN_SHORT` |
| `P066` | `L88_O2GenerationPowerConverter` | 2 | `ANALOG_FEEDBACK;DIGITAL_CONTROL` | True | `RECONCILE_SPLIT_LOGICAL_PORTS_NO_CROSS_DOMAIN_SHORT` |
| `P067` | `L88_OxygenGeneratorThermalCoolingLoop` | 2 | `ANALOG_FEEDBACK;DIGITAL_CONTROL` | True | `RECONCILE_SPLIT_LOGICAL_PORTS_NO_CROSS_DOMAIN_SHORT` |
| `P070` | `L88_OxygenEmergencyBackupReserve` | 2 | `DIGITAL_CONTROL;SAFETY_INTERLOCK` | True | `RECONCILE_SPLIT_LOGICAL_PORTS_NO_CROSS_DOMAIN_SHORT` |
| `P106` | `L88_HumidityRecoveryPLC` | 5 | `ANALOG_FEEDBACK` | False | `RECONCILE_SPLIT_LOGICAL_PORTS_NO_CROSS_DOMAIN_SHORT` |
| `P090` | `L88_HumidityCondensingHeatExchanger` | 2 | `ANALOG_FEEDBACK` | False | `RECONCILE_SPLIT_LOGICAL_PORTS_NO_CROSS_DOMAIN_SHORT` |
| `P110` | `L88_HumidityRecoveryPLC` | 5 | `ANALOG_FEEDBACK` | False | `RECONCILE_SPLIT_LOGICAL_PORTS_NO_CROSS_DOMAIN_SHORT` |
| `P091` | `L88_CoolingCoilSerpentineAssembly` | 2 | `ANALOG_FEEDBACK` | False | `RECONCILE_SPLIT_LOGICAL_PORTS_NO_CROSS_DOMAIN_SHORT` |
| `P098` | `L88_HumidityRecoveryPLC` | 4 | `ANALOG_FEEDBACK` | False | `RECONCILE_SPLIT_LOGICAL_PORTS_NO_CROSS_DOMAIN_SHORT` |
| `P092` | `L88_CondensateWaterSeparator` | 2 | `ANALOG_FEEDBACK` | False | `RECONCILE_SPLIT_LOGICAL_PORTS_NO_CROSS_DOMAIN_SHORT` |
| `P093` | `L88_CondensateCollectionReservoir` | 2 | `ANALOG_FEEDBACK` | False | `RECONCILE_SPLIT_LOGICAL_PORTS_NO_CROSS_DOMAIN_SHORT` |
| `P094` | `L88_ReclaimedWaterPump` | 2 | `ANALOG_FEEDBACK` | False | `RECONCILE_SPLIT_LOGICAL_PORTS_NO_CROSS_DOMAIN_SHORT` |
| `P115` | `L88_RecoveredWaterFiltrationRack` | 2 | `ANALOG_FEEDBACK` | False | `RECONCILE_SPLIT_LOGICAL_PORTS_NO_CROSS_DOMAIN_SHORT` |
| `P114` | `L88_CoolingValveActuator` | 2 | `ANALOG_FEEDBACK` | False | `RECONCILE_SPLIT_LOGICAL_PORTS_NO_CROSS_DOMAIN_SHORT` |
| `P096` | `L88_FrozenCondenserFaultInjector` | 2 | `ANALOG_FEEDBACK;SAFETY_INTERLOCK` | True | `RECONCILE_SPLIT_LOGICAL_PORTS_NO_CROSS_DOMAIN_SHORT` |
| `P117` | `L88_WaterResourceBudgetAnalyzer` | 2 | `ANALOG_FEEDBACK` | False | `RECONCILE_SPLIT_LOGICAL_PORTS_NO_CROSS_DOMAIN_SHORT` |
| `P068` | `L88_WaterFeedPumpToElectrolyzer` | 2 | `ANALOG_FEEDBACK` | False | `RECONCILE_SPLIT_LOGICAL_PORTS_NO_CROSS_DOMAIN_SHORT` |
| `P150` | `L88_AirQualityEnvironmentalPLC` | 2 | `ANALOG_FEEDBACK;DIGITAL_CONTROL` | True | `RECONCILE_SPLIT_LOGICAL_PORTS_NO_CROSS_DOMAIN_SHORT` |
| `P120` | `L88_VentilationMainCirculationBlower` | 2 | `ANALOG_FEEDBACK;DIGITAL_CONTROL` | True | `RECONCILE_SPLIT_LOGICAL_PORTS_NO_CROSS_DOMAIN_SHORT` |
| `P154` | `L88_AirQualityEnvironmentalPLC` | 3 | `ANALOG_FEEDBACK;DIGITAL_CONTROL` | True | `RECONCILE_SPLIT_LOGICAL_PORTS_NO_CROSS_DOMAIN_SHORT` |
| `P122` | `L88_VentilationSecondaryCirculationBlower` | 2 | `ANALOG_FEEDBACK;DIGITAL_CONTROL` | True | `RECONCILE_SPLIT_LOGICAL_PORTS_NO_CROSS_DOMAIN_SHORT` |
| `P158` | `L88_AirQualityEnvironmentalPLC` | 3 | `ANALOG_FEEDBACK;DIGITAL_CONTROL` | True | `RECONCILE_SPLIT_LOGICAL_PORTS_NO_CROSS_DOMAIN_SHORT` |
| `P124` | `L88_VariableFrequencyFanDriveA` | 2 | `ANALOG_FEEDBACK;DIGITAL_CONTROL` | True | `RECONCILE_SPLIT_LOGICAL_PORTS_NO_CROSS_DOMAIN_SHORT` |
| `P162` | `L88_AirQualityEnvironmentalPLC` | 3 | `ANALOG_FEEDBACK;DIGITAL_CONTROL` | True | `RECONCILE_SPLIT_LOGICAL_PORTS_NO_CROSS_DOMAIN_SHORT` |
| `P125` | `L88_VariableFrequencyFanDriveB` | 2 | `ANALOG_FEEDBACK;DIGITAL_CONTROL` | True | `RECONCILE_SPLIT_LOGICAL_PORTS_NO_CROSS_DOMAIN_SHORT` |
| `P121` | `L88_VentilationMainCirculationBlower` | 2 | `ANALOG_FEEDBACK` | False | `RECONCILE_SPLIT_LOGICAL_PORTS_NO_CROSS_DOMAIN_SHORT` |
| `P126` | `L88_PrimaryAirDuctDistributionManifold` | 2 | `ANALOG_FEEDBACK` | False | `RECONCILE_SPLIT_LOGICAL_PORTS_NO_CROSS_DOMAIN_SHORT` |
| `P152` | `L88_AirQualityEnvironmentalPLC` | 2 | `ANALOG_FEEDBACK` | False | `RECONCILE_SPLIT_LOGICAL_PORTS_NO_CROSS_DOMAIN_SHORT` |
| `P128` | `L88_FourZoneMotorizedDamperBank` | 3 | `ANALOG_FEEDBACK;DIGITAL_CONTROL` | True | `RECONCILE_SPLIT_LOGICAL_PORTS_NO_CROSS_DOMAIN_SHORT` |
| `P144` | `L88_TraceContaminantFilterArray` | 2 | `ANALOG_FEEDBACK` | False | `RECONCILE_SPLIT_LOGICAL_PORTS_NO_CROSS_DOMAIN_SHORT` |
| `P183` | `L88_HabitatLifeSupportSafetyPLC` | 4 | `ANALOG_FEEDBACK;SAFETY_INTERLOCK` | True | `RECONCILE_SPLIT_LOGICAL_PORTS_NO_CROSS_DOMAIN_SHORT` |
| `P141` | `L88_CabinLeakIsolationManifold` | 2 | `ANALOG_FEEDBACK;SAFETY_INTERLOCK` | True | `RECONCILE_SPLIT_LOGICAL_PORTS_NO_CROSS_DOMAIN_SHORT` |
| `P187` | `L88_HabitatLifeSupportSafetyPLC` | 4 | `ANALOG_FEEDBACK;SAFETY_INTERLOCK` | True | `RECONCILE_SPLIT_LOGICAL_PORTS_NO_CROSS_DOMAIN_SHORT` |
| `P167` | `L88_HabitatLifeSupportSafetyPLC` | 4 | `ANALOG_FEEDBACK;SAFETY_INTERLOCK` | True | `RECONCILE_SPLIT_LOGICAL_PORTS_NO_CROSS_DOMAIN_SHORT` |
| `P142` | `L88_CabinPressureRegulator` | 2 | `ANALOG_FEEDBACK;SAFETY_INTERLOCK` | True | `RECONCILE_SPLIT_LOGICAL_PORTS_NO_CROSS_DOMAIN_SHORT` |
| `P171` | `L88_HabitatLifeSupportSafetyPLC` | 4 | `ANALOG_FEEDBACK;SAFETY_INTERLOCK` | True | `RECONCILE_SPLIT_LOGICAL_PORTS_NO_CROSS_DOMAIN_SHORT` |
| `P175` | `L88_HabitatLifeSupportSafetyPLC` | 3 | `ANALOG_FEEDBACK;SAFETY_INTERLOCK` | True | `RECONCILE_SPLIT_LOGICAL_PORTS_NO_CROSS_DOMAIN_SHORT` |
| `P179` | `L88_HabitatLifeSupportSafetyPLC` | 3 | `ANALOG_FEEDBACK;SAFETY_INTERLOCK` | True | `RECONCILE_SPLIT_LOGICAL_PORTS_NO_CROSS_DOMAIN_SHORT` |
| `P216` | `L88_WaterAndEnergySafetyInterlock` | 2 | `ANALOG_FEEDBACK;SAFETY_INTERLOCK` | True | `RECONCILE_SPLIT_LOGICAL_PORTS_NO_CROSS_DOMAIN_SHORT` |
| `P209` | `L88_ECLSSMasterPLC` | 2 | `ANALOG_FEEDBACK` | False | `RECONCILE_SPLIT_LOGICAL_PORTS_NO_CROSS_DOMAIN_SHORT` |
| `P238` | `L88_EmergencyAtmosphereIsolationConsole` | 2 | `ANALOG_FEEDBACK;SAFETY_INTERLOCK` | True | `RECONCILE_SPLIT_LOGICAL_PORTS_NO_CROSS_DOMAIN_SHORT` |
| `P234` | `L88_AtmosphericDigitalFlowHologram` | 2 | `ANALOG_FEEDBACK` | False | `RECONCILE_SPLIT_LOGICAL_PORTS_NO_CROSS_DOMAIN_SHORT` |
| `P218` | `L88_HabitatEnvironmentalSCADA` | 2 | `ANALOG_FEEDBACK` | False | `RECONCILE_SPLIT_LOGICAL_PORTS_NO_CROSS_DOMAIN_SHORT` |
| `P241` | `L88_AtmosphereMissionCertificationBeacon` | 2 | `ANALOG_FEEDBACK;SAFETY_INTERLOCK` | True | `RECONCILE_SPLIT_LOGICAL_PORTS_NO_CROSS_DOMAIN_SHORT` |
| `P226` | `L88_HabitatEnvironmentalSCADA` | 2 | `ANALOG_FEEDBACK` | False | `RECONCILE_SPLIT_LOGICAL_PORTS_NO_CROSS_DOMAIN_SHORT` |

## ANNEX G — ALL SIX SOURCE OBJECTIVE RECORDS

The original objective CSV reports `Implemented=NO` on all six records. The titles, criteria and planned evidence are exact source text.

| # | Exact source title | Original acceptance criteria | Original proposed evidence | Already implemented? |
|---:|---|---|---|:---:|
| 1 | **Build the Atmospheric Control Network** | All 164 actual player-created live electrical wires, compatible terminals/returns, safety, critical W145, no bypass | Electrical topology fingerprint, energy/safety qualification | NO |
| 2 | **Calibrate the Habitat Atmosphere** | All four zone pressure, O2 partial pressure, CO2, humidity and airflow instruments calibrated; actual fans and dampers proven | Timestamped calibration coefficients, fresh sample and actual actuator evidence | NO |
| 3 | **Commission Oxygen and CO2 Regeneration** | Finite water-fed oxygen generator and two physically cycling CO2 beds achieve qualified mass/energy accounting | Captured gas inventory, water/energy balance, valid actual flows and safe stream separation | NO |
| 4 | **Sustain the Living Atmosphere** | Fresh full low-demand→peak-demand→recovery scenario meets protected fictional gas partial pressures and humidity limits | Fresh run ID with compartment mass/thermal balance, actual ventilation and resource margin | NO |
| 5 | **Recover Three Life-Support Failures** | CO2 bed breakthrough, condenser freeze and micro-leak each change plant, repaired, then fresh full endurance run | Causal fault/repair history, no unresolved safety latch, new run ID | NO |
| 6 | **Restore Critical Cabin Pressure Feedback** | Physically disconnect W145 after historical 1–5, safety-inhibit, reconnect, calibrate, explicit reset, new full qualification | Real W145 circuit-edge mutation, sample validity, independent protection, post-fault runID, LevelGamePlayManager 6/6 | NO |

## ANNEX H — W121–W146 INDEPENDENT SAFETY AND CRITICAL W145 ROUTES

These are the exact relevant source rows; where a visual socket is reused, the proposed functional ports must still be separated as specified in Annex E.

| WID | Source (socket) | Target (socket) | Kind | Intended purpose | Critical |
|---|---|---|---|---|:---:|
| **W121** | `L88_HabitatLifeSupportSafetyPLC` (`P183`) | `L88_CabinLeakIsolationManifold` (`P141`) | `SAFETY_INTERLOCK` | Compartment leak isolation authorization | NO |
| **W122** | `L88_CabinLeakIsolationManifold` (`P141`) | `L88_HabitatLifeSupportSafetyPLC` (`P187`) | `ANALOG_FEEDBACK` | Actual leak isolation valve feedback | NO |
| **W123** | `L88_HabitatLifeSupportSafetyPLC` (`P167`) | `L88_CabinPressureRegulator` (`P142`) | `SAFETY_INTERLOCK` | Normal pressure regulator safety permit | NO |
| **W124** | `L88_CabinPressureRegulator` (`P142`) | `L88_HabitatLifeSupportSafetyPLC` (`P171`) | `ANALOG_FEEDBACK` | Actual pressure regulator operation state | NO |
| **W125** | `L88_HabitatMicroLeakFaultInjector` (`P143`) | `L88_HabitatLifeSupportSafetyPLC` (`P175`) | `ANALOG_FEEDBACK` | Micro-leak fault source declaration | NO |
| **W126** | `L88_HabitatPressureVesselSafetyRelief` (`P149`) | `L88_HabitatLifeSupportSafetyPLC` (`P179`) | `SAFETY_INTERLOCK` | Independent relief valve mechanically sensed state | NO |
| **W127** | `L88_IndependentCabinPressureSensor` (`P136`) | `L88_HabitatLifeSupportSafetyPLC` (`P183`) | `ANALOG_FEEDBACK` | Independent backup pressure protector health | NO |
| **W128** | `L88_DifferentialPressureMonitorArray` (`P140`) | `L88_HabitatLifeSupportSafetyPLC` (`P187`) | `SAFETY_INTERLOCK` | Compartments differential pressure safety | NO |
| **W129** | `L88_HydrogenByproductIsolationModule` (`P058`) | `L88_HabitatLifeSupportSafetyPLC` (`P167`) | `SAFETY_INTERLOCK` | Byproduct stream segregation interlock | NO |
| **W130** | `L88_OxygenEmergencyBackupReserve` (`P070`) | `L88_HabitatLifeSupportSafetyPLC` (`P171`) | `SAFETY_INTERLOCK` | Backup oxygen supply isolation state | NO |
| **W131** | `L88_WaterAndEnergySafetyInterlock` (`P216`) | `L88_HabitatLifeSupportSafetyPLC` (`P175`) | `SAFETY_INTERLOCK` | Water resource independent safety inhibit | NO |
| **W132** | `L88_HabitatLifeSupportSafetyPLC` (`P179`) | `L88_WaterAndEnergySafetyInterlock` (`P216`) | `ANALOG_FEEDBACK` | Safety PLC water-energy permit status | NO |
| **W133** | `L88_ECLSSMasterPLC` (`P205`) | `L88_HabitatLifeSupportSafetyPLC` (`P183`) | `SAFETY_INTERLOCK` | Atmosphere supervisor safety run request | NO |
| **W134** | `L88_HabitatLifeSupportSafetyPLC` (`P187`) | `L88_ECLSSMasterPLC` (`P209`) | `ANALOG_FEEDBACK` | Actual independent safety authorization | NO |
| **W135** | `L88_HabitatLifeSupportSafetyPLC` (`P167`) | `L88_EmergencyAtmosphereIsolationConsole` (`P238`) | `SAFETY_INTERLOCK` | Emergency isolation inhibit output | NO |
| **W136** | `L88_EmergencyAtmosphereIsolationConsole` (`P239`) | `L88_HabitatLifeSupportSafetyPLC` (`P171`) | `ANALOG_FEEDBACK` | Emergency station actual stop state | NO |
| **W137** | `L88_HabitatLifeSupportSafetyPLC` (`P175`) | `L88_ECLSSMasterPLC` (`P215`) | `ANALOG_FEEDBACK` | Safety fault latch event report | NO |
| **W138** | `L88_MissionOccupancyDemandSimulator` (`P236`) | `L88_HabitatLifeSupportSafetyPLC` (`P179`) | `ANALOG_FEEDBACK` | Simulated occupancy test safety status | NO |
| **W139** | `L88_FrozenCondenserFaultInjector` (`P096`) | `L88_HabitatLifeSupportSafetyPLC` (`P183`) | `SAFETY_INTERLOCK` | Condenser freeze critical inhibit | NO |
| **W140** | `L88_CO2BreakthroughFaultInjector` (`P034`) | `L88_HabitatLifeSupportSafetyPLC` (`P187`) | `ANALOG_FEEDBACK` | CO2 breakthrough safety condition | NO |
| **W141** | `L88_HabitatLifeSupportSafetyPLC` (`P167`) | `L88_AtmosphericDigitalFlowHologram` (`P234`) | `ANALOG_FEEDBACK` | Safety status for hologram readout | NO |
| **W142** | `L88_HabitatLifeSupportSafetyPLC` (`P171`) | `L88_HabitatEnvironmentalSCADA` (`P218`) | `ANALOG_FEEDBACK` | Safety latched fault SCADA output | NO |
| **W143** | `L88_ECLSSMasterPLC` (`P206`) | `L88_EmergencyAtmosphereIsolationConsole` (`P240`) | `SAFETY_INTERLOCK` | Operator explicit safety reset request | NO |
| **W144** | `L88_EmergencyAtmosphereIsolationConsole` (`P238`) | `L88_HabitatLifeSupportSafetyPLC` (`P176`) | `ANALOG_FEEDBACK` | Operator explicit reset demand feedback | NO |
| **W145** | `L88_CabinPressurePrimaryTransducer` (`P134`) | `L88_HabitatLifeSupportSafetyPLC` (`P180`) | `ANALOG_FEEDBACK` | CRITICAL trusted cabin pressure feedback | YES |
| **W146** | `L88_HabitatLifeSupportSafetyPLC` (`P184`) | `L88_AtmosphereMissionCertificationBeacon` (`P241`) | `SAFETY_INTERLOCK` | Six-objective final safety permission | NO |

## ANNEX I — ALL 13 ORIGINAL PHYSICAL PROCESS-FLOW CONCEPTS

These are **physical gas/water/process flow plans**, not electrical Spark wires. `RequiredAuthority` describes the intended physical conservation control, not a tested process solver.

| # | From | To | Physical flow | Required process authority |
|---:|---|---|---|---|
| 1 | Habitat Compartment A | Main Airflow Duct | Gas flow / loop return | Mass balance and fan pressure |
| 2 | Habitat Compartment B | Main Airflow Duct | Gas flow / loop return | Mass balance and damper position |
| 3 | Habitat Compartment C | Main Airflow Duct | Gas flow / loop return | Mass balance and damper position |
| 4 | Habitat Compartment D | Main Airflow Duct | Gas flow / loop return | Mass balance and damper position |
| 5 | Air Duct | Adsorber Tower A | Mixed cabin gas | Bed capacity and actual inlet valve |
| 6 | Air Duct | Adsorber Tower B | Mixed cabin gas | Bed capacity and actual inlet valve |
| 7 | Adsorber Towers | Scrubbed Airflow Return | CO2-depleted gas | Regeneration cycle and capture efficiency |
| 8 | Water Recovery | Finite Clean Water Reservoir | Condensed water | Condensation and water quality |
| 9 | Water Reservoir | Oxygen Electrolyzer | Clean water flow | Resource consumption and pump response |
| 10 | Oxygen Electrolyzer | Oxygen Buffer Tank | Separated oxygen | Finite power, water, and process qualification |
| 11 | Oxygen Buffer Tank | Cabin Gas Regulation | Regulated oxygen | Actual valve, fresh pressure and safety authorization |
| 12 | Habitat Compartments | Environment / Micro-Leak | Gas inventory loss | Actual leak state and compartment pressure |
| 13 | Safety Relief Valve | Environment | Protected pressure release | Independent trip and actual valve dynamics |

## ANNEX J — SOURCE MANIFEST, SPATIAL AND TEST STATUS

| Manifest field | Exact source value |
|---|---|
| `level_id` | `"level_88"` |
| `display_name` | `"The Breathing Ring"` |
| `subtitle` | `"Autonomous Life Support, Atmospheric Regeneration and Pressure Safety"` |
| `individual_models` | `90` |
| `individual_glbs` | `90` |
| `assembled_fbx` | `"Models/Level88_Complete_ArtKit_Showcase.fbx"` |
| `assembled_glb` | `"GLB/Level88_Complete_ArtKit_Showcase.glb"` |
| `individual_triangles` | `184224` |
| `individual_mesh_parts` | `1219` |
| `animation_parent_pivot_groups` | `72` |
| `planned_player_wires` | `164` |
| `planned_visual_terminal_sockets` | `242` |
| `proposed_independent_logical_wire_endpoints` | `328` |
| `anchor_reconciliation_conflicts` | `108` |
| `workstation_world_bounds_m` | `[[-15.4, -0.39, -14.006], [15.4, 13.98959, 12.2]]` |
| `fbx_numeric_units` | `"centimetres"` |
| `fbx_unit_scale_factor` | `1` |
| `glb_numeric_units` | `"metres"` |
| `axes` | `"Y-up X-width Z-forward"` |
| `assembly_origin` | `[0, 0, 0]` |
| `actual_player_wires_included` | `false` |
| `functional_sparkterminals_included` | `false` |
| `real_atmosphere_gas_solver_included` | `false` |
| `runtime_safety_plc_included` | `false` |
| `six_runtime_objectives_implemented` | `false` |
| `unity_compilation_tested` | `false` |
| `unity_playmode_tested` | `false` |

The verification file reports **27/27 structural checks passed**, `0` failed; this is not a substitute for Unity compile, runtime simulation, Play Mode or hand-authored production art qualification.

## ANNEX K — UNITY ART HELPERS, VISUAL COMPONENT, AND 12 MATERIAL SETS

**Exact supplied Unity files/menu contracts:**

- `Unity_Optional/SparkLevel88ArtPlacementEditor.cs`: `Tools/Project Spark/Levels/88/Place HD Art Under Selected Layout`.
- `Unity_Optional/SparkLevel88MaterialUpgradeEditor.cs`: `Tools/Project Spark/Levels/88/Create & Apply HD Materials To Selected Art`.
- `Unity_Optional/SparkLevel88AssetValidatorEditor.cs`: `Tools/Project Spark/Levels/88/Validate Imported HD Art`.
- `Unity_Optional/SparkLevel88VisualRig.cs`: `ProjectSpark.Visuals.SparkLevel88VisualRig`, whose exact `SetActual...` signatures appear in section 10.

**Source material sets (all 1024×1024 reference maps):**

| Material set | Resolution | Available maps | Source usage status |
|---|---|---|---|
| `PressurizedHabitatCeramicWhite` | 1024×1024 | Albedo / Normal / MetalRoughAO | `OPTIONAL_REFERENCE_MAPS_REQUIRING_UV_AND_SHADER_CHANNEL_MAPPING` |
| `CryogenicPipeBrushedStainless` | 1024×1024 | Albedo / Normal / MetalRoughAO | `OPTIONAL_REFERENCE_MAPS_REQUIRING_UV_AND_SHADER_CHANNEL_MAPPING` |
| `ElectrolyzerCopperBusbars` | 1024×1024 | Albedo / Normal / MetalRoughAO | `OPTIONAL_REFERENCE_MAPS_REQUIRING_UV_AND_SHADER_CHANNEL_MAPPING` |
| `ECLSSGoldInstrumentContacts` | 1024×1024 | Albedo / Normal / MetalRoughAO | `OPTIONAL_REFERENCE_MAPS_REQUIRING_UV_AND_SHADER_CHANNEL_MAPPING` |
| `ActivatedCO2SorbentBlue` | 1024×1024 | Albedo / Normal / MetalRoughAO | `OPTIONAL_REFERENCE_MAPS_REQUIRING_UV_AND_SHADER_CHANNEL_MAPPING` |
| `HabitatBlueGlass` | 1024×1024 | Albedo / Normal / MetalRoughAO | `OPTIONAL_REFERENCE_MAPS_REQUIRING_UV_AND_SHADER_CHANNEL_MAPPING` |
| `HabitatEnvironmentalDarkPanels` | 1024×1024 | Albedo / Normal / MetalRoughAO | `OPTIONAL_REFERENCE_MAPS_REQUIRING_UV_AND_SHADER_CHANNEL_MAPPING` |
| `AtmosphereCyanHolographic` | 1024×1024 | Albedo / Normal / MetalRoughAO | `OPTIONAL_REFERENCE_MAPS_REQUIRING_UV_AND_SHADER_CHANNEL_MAPPING` |
| `CondenserColdSteelFins` | 1024×1024 | Albedo / Normal / MetalRoughAO | `OPTIONAL_REFERENCE_MAPS_REQUIRING_UV_AND_SHADER_CHANNEL_MAPPING` |
| `HabitatServiceFloor` | 1024×1024 | Albedo / Normal / MetalRoughAO | `OPTIONAL_REFERENCE_MAPS_REQUIRING_UV_AND_SHADER_CHANNEL_MAPPING` |
| `CO2SafetyAmberStatus` | 1024×1024 | Albedo / Normal / MetalRoughAO | `OPTIONAL_REFERENCE_MAPS_REQUIRING_UV_AND_SHADER_CHANNEL_MAPPING` |
| `LifeSupportEmergencyRed` | 1024×1024 | Albedo / Normal / MetalRoughAO | `OPTIONAL_REFERENCE_MAPS_REQUIRING_UV_AND_SHADER_CHANNEL_MAPPING` |

`MetalRoughAO`: R=metallic, G=roughness, B=AO. `uv_authoring_required=true`; production UV, maps/shaders, LODs, VFX and collision are still separate engineering/art tasks.

## FINAL IMPLEMENTATION AUTHORITY REMINDER

This is a complete, source-grounded **implementation request**, not proof of implementation. The supplied ZIP is real original modular art + planning, not functioning life support or live player wiring. Complete work inside the real Project Spark Unity game, verify full ECLSS process physics and actual wire graph, perform real Play Mode tests, preserve Levels 1–87 and report objective state from **only** the authoritative `LevelGamePlayManager` after fresh post-W145 qualification.
