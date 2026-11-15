# PROJECT SPARK — LEVEL 83 MASTER UNITY IMPLEMENTATION PROMPT

## LEVEL 83: BEYOND THE SHADOW

**Advanced AAA Orbital Electrical Power Systems • Solar Array Dynamics • Dual MPPT • Battery Management • DC Bus Regulation • Eclipse Survival • Load Priority Scheduling • Critical W104 Feedback Recovery**

> **SOURCE-GROUNDED DELIVERY:** This document is based on the local `ProjectSpark_Level83_HD_AAA_Models.zip` and its exact extracted files. Asset counts, component names, coordinates, wiring CSV, animation API and W104 are source-derived; simulation architecture, thresholds and testing workflow are **proposed requirements to implement**, not existing runtime functionality.


# 1. PRIMARY MANDATE

Act as my **senior Unity C# gameplay engineer, Project Spark electrical-system integration specialist, electrical power systems architect, spacecraft EPS engineer, photovoltaic/MPPT controls engineer, battery-management engineer, mechatronics engineer, industrial PLC and functional-safety engineer, simulation/gameplay systems designer, AAA technical artist, and Unity Editor automation developer**.

**IMPLEMENT THE COMPLETE, FUNCTIONAL, PLAYABLE LEVEL 83 — BEYOND THE SHADOW — INSIDE MY EXISTING PROJECT SPARK UNITY PROJECT.** This is not a redesign request. Import and use the existing generated models in `ProjectSpark_Level83_HD_AAA_Models.zip`; use the exact package manifests; do not regenerate its geometry unless a genuine integration defect makes correction necessary.

Deliver real player-created circuits and sensor feedback, a physically conservative educational spacecraft energy simulator, actual orbital illumination, solar MPPT and converters, two BMS-controlled battery packs, DC bus regulation, priority load shedding, three real disturbances, final W104 fault recovery, and six actual `LevelGamePlayManager` objectives. **The authoritative game must reach 6/6.** Preserve Levels 1–82.

# 2. READ THESE EXACT SOURCE FILES FIRST

Open the supplied ZIP and inspect **actual** package files rather than relying on a narrative prompt:

- `Model_Manifest.json`, `Art_Assembly.csv`, `Component_Roles.csv`.
- `Animation_Rigs.csv` for 56 physical pivot group parents, exact pivots/axes/coords.
- `Gameplay_Wiring_Concept_W001_W138.csv` and `Gameplay_Wiring_Budget.csv`.
- `Terminal_Anchor_Plan_212.csv` and `Visual_Terminal_Sockets_212.csv`.
- `Objective_Certification_Plan_6.csv`, `W104_Critical_Feedback_Recovery.md`, `Gameplay_Integration_Plan.md`.
- `Unity_Optional/SparkLevel83ArtPlacementEditor.cs`, `SparkLevel83MaterialUpgradeEditor.cs`, `SparkLevel83AssetValidatorEditor.cs`, `SparkLevel83VisualRig.cs`.
- `Texture_Manifest.json`, `Material_Palette.json`, `README_Unity.txt`, `Verification_Report.json`.

Annexes A–D below reproduce the important **source-derived** names, positions, wiring pairs and anchor positions. If the archive differs from this text, investigate and use the actual imported asset structure. Never invent methods or claim runtime behavior exists in the art package.

# 3. VERIFIED ASSET BASELINE AND WHAT IS NOT INCLUDED

The original `Model_Manifest.json` reports: **74 modular FBXs, 74 matching GLBs, 1 assembled showcase FBX and GLB, 299,296 modular equipment triangles, 8,719 mesh parts, 56 pivot parent groups, 212 decorative socket visuals, 138 conceptual wire entries, 212 proposed terminal interfaces, and 6 objective design records**. Nominal workstation bounds are `(-13.1,0,-6.625)` to `(13.1,9.29,6.6975)` metres; overall width ~**26.20 m** and height **9.29 m**. The structural `Verification_Report.json` indicates the FBX/GLB files and pivots were checked, but `unity_compilation_tested=false`, `unity_play_mode_tested=false`, `actual_player_wires_included=false`, `functional_sparkterminals_included=false`, and `gameplay_functionality_included=false`.

**Do not claim source package already contains playable circuits, power physics, solar tracking algorithms, Unity gameplay scenes or completed objectives.** The 36 procedural PBR texture maps are preview-grade, not hand-authored final AAA texture/baking/LOD production.

# 4. INSPECT THE EXISTING PROJECT SPARK APIs

Search the user's actual Unity codebase, and inspect when present: `SparkElectricalSolver`, `SparkCircuitSystem`, `SparkElectricalComponent`, `SparkTerminal`, `SparkSwitch`, `SparkLevelDefinition`, `SparkLevelEvaluator`, `SparkLevelSceneBindings`, `SparkLevelWorkstationBindingBridge`, `SparkShelfSlot`, `SparkObjectShelf`, `SparkLevelLayoutLoader`, `SparkLevel`, `LevelGamePlayManager`.

Identify real namespaces, serialized fields, wire creation/removal events, terminal compatibility, voltage semantics, power supplies/returns, PLC conventions, safety-latch/reset mechanisms, shelf/shop inventory, scene/prefab placement, objective truth and Editor menu patterns. Reuse source from earlier completed Levels 58–82 where actually implemented (e.g. topology disconnect, motion, thermal dynamics, industrial PLC, sensor calibration, historical objectives). Avoid speculative method calls. If APIs differ, create small, testable adapters.

# 5. ABSOLUTE IMPLEMENTATION RULES

1. Preserve Levels 1–82; no global prefabs/scripts overwritten without deliberate migration.
2. Actual player-created wires must affect actual circuit topology; visual cables alone never qualify.
3. Actual `SparkTerminal` components must be registered and compatible; avoid mixing power, analog and digital at one node.
4. Keep the educational **5 V control domain**, **fictional spacecraft energy/power plant**, and **independent safety protection** separate.
5. Preserve energy and charge/efficiency accounting; no free energy or negative stored energy.
6. Requested solar-gimbal positions, converter setpoints, contactor commands and load shed commands must not instantly equal actual physical states.
7. Solar input tracks true incidence and eclipse; a pointing command cannot create sunlight.
8. MPPT must optimize measured/estimated PV characteristics, not a cosmetic knob.
9. Battery SOC, cell-group voltages/current/temperature and contactor feedback are authoritative plant values, not UI flags.
10. Supercapacitor energy is finite and cannot carry the full eclipse.
11. Essential mission loads and deadlines prevent an all-off shortcut.
12. Invalid/stale W104 must block normal closed-loop certification, but must **not** teleport the bus voltage to zero.
13. Independent undervoltage protection must still work during W104 loss and cannot serve as an unapproved normal-control substitute.
14. W104 cannot be bypassed by health telemetry, SCADA cached voltage or old samples.
15. Fault latch clears only after real wire repair, fresh sampling, calibration, safety check and explicit reset.
16. Objectives 1–5 stay historically certified during deliberate W104 fault; only Objective 6 requires new fresh full-cycle evidence.
17. Visual rig consumes data only; do not read lights or animation pivots to determine safety/objectives.
18. Do not claim Unity compilation, Play Mode, battery safety or spacecraft flight qualification absent actual tests.

# 6. LEVEL CONFIGURATION

Use `level_83`, title `Beyond the Shadow`, subtitle `Advanced Orbital Power and Eclipse Survival`, expert/master difficulty. **6 objectives**, **138 player connections**, **212 primary gold-rimmed decorative socket anchors**, **74 modular models**, **56 animation pivots**. The power plant includes two deployable solar wings, two independent MPPT converters, two protected battery packs, regulated DC bus, independent undervoltage protection, transient supercapacitor, mission workload scheduler and orbital sunlight/eclipse environment. `W104` is the final trusted bus voltage measurement. Provide one-click `Tools/Project Spark/Levels/Create Level 83`.

Use configurable **fictional, normalized** gameplay thresholds. Do not substitute real spacecraft qualification specifications or dangerous physical battery recipes. Present test times such as 45 seconds sunlight, 35 seconds eclipse and 40 seconds recovery only as adjustable *game-scale defaults*, not real orbital mechanics.

# 7. ART IMPORT, COORDINATES AND PREFAB ASSEMBLY

Import the 74 numbered binary FBXs as individually addressable modular models. FBX numeric geometry is in **centimetres** (`UnitScaleFactor=1`); GLB geometry is in **metres**. Y is up; X is width; Z is forward. Convert FBX geometry centimetres to metres **exactly once**. All modular models are authored at common `(0,0,0)` origin.

Place identity-local-transform copies under `L83_HD_VISUAL_ART` beneath Level 83 layout. Verify renderer bounds ~26.20 m across and 9.29 m tall. The separate assembled showcase `Models/Level83_Complete_ArtKit_Showcase.fbx` / `GLB/Level83_Complete_ArtKit_Showcase.glb` is for reference/alternative display and **must not be instantiated beside the modular set**. Keep physics, SparkTerminals, colliders, wiring and gameplay objects outside the replaceable art root. Use undo and idempotent scene/prefab updates.

# 8. REUSE PROVIDED UNITY ART MENUS

The archive provides Editor classes in namespace `ProjectSpark.EditorTools`:

- `SparkLevel83ArtPlacementEditor` menu: `Tools/Project Spark/Levels/83/Place HD Art Under Selected Layout`. It instantiates 74 FBXs and may replace `L83_HD_VISUAL_ART` after confirmation.
- `SparkLevel83MaterialUpgradeEditor` menu: `Tools/Project Spark/Levels/83/Create & Apply HD Materials To Selected Art`; output materials in `Assets/ProjectSpark/Level83/Materials`.
- `SparkLevel83AssetValidatorEditor` menu: `Tools/Project Spark/Levels/83/Validate Imported HD Art`.
- Runtime `ProjectSpark.Visuals.SparkLevel83VisualRig` for animation only.

Place `*Editor.cs` files in a Unity `Editor` directory, runtime visual rig elsewhere. Do not mistake the art placement menu for the required new gameplay builder.

# 9. PBR, HDRP/URP, AAA FINISH AND OPTIMIZATION

The asset package has **12 sets/36 procedural maps**. Each set provides albedo, normal and metallic/roughness/AO; packed channels: **R metallic, G roughness, B AO**. Interpret/repack channels correctly for the project's URP or HDRP shaders (roughness may need smoothness inversion). Ensure transparent materials (test chamber, battery enclosures, orbital holograms) use appropriate depth/sorting settings.

Upgrade solar cells, gold ribbons, battery cell groups, aluminum frames, electronics racks, cooling fins, metallic contactors, coated glass and emissive orbital graphics. Add separate LODs, GPU instancing for thousands of PV-cell meshes, static batching for stationary equipment, dynamic batching only where appropriate, mesh colliders only where necessary, draw-call budget and occlusion considerations. No visual-only animation should drive physics. Report uncreated UV bakes or authored VFX honestly.

# 10. VERIFIED VISUAL RIG API — DO NOT INVENT SIGNATURES

Existing namespace: `ProjectSpark.Visuals`, sealed component: `SparkLevel83VisualRig`. Verified public methods:

```csharp
SetActualOrbitalState(float actualOrbitAngleDegrees, float actualSunlightFraction01, float actualEclipseFraction01);
SetActualSolarArrays(float actualPortDeploy01, float actualStarboardDeploy01,
    float actualPortAzimuthDegrees, float actualPortElevationDegrees,
    float actualStarboardAzimuthDegrees, float actualStarboardElevationDegrees,
    bool actualSolarInputAvailable);
SetActualPowerMeasurements(float actualPortMPPTPerformance01, float actualStarboardMPPTPerformance01,
    float measuredBusVoltageQuality01, float actualPackASOC01, float actualPackBSOC01,
    float actualBusTransientActivity01, bool measuredBusQualified, bool actualChargeEnabled);
SetActualBatteryMechanisms(float measuredTrayA01, float measuredTrayB01, float actualBalancing01,
    float actualContactorClosed01, float actualSupercapacitorActivity01);
SetActualLoadDistribution(float essential01, float attitude01, float optical01,
    float science01, float thermal01);
SetActualSafetyAndFaults(bool actualW104FeedbackValid, bool actualCriticalFaultLatched,
    bool actualPowerSafeMode, float actualGimbalJam01, float actualCellImbalance01,
    float actualOpticalBurst01, float actualCertificationProgress01);
```

The class consumes **actual-state** values, clamps normalized visuals, binds 56 pivots in `Awake`, and updates them during `LateUpdate`. It does not expose/set actual voltage physics or objective completion. The editor implementation may need an adapter that maps physical values to normalized rig inputs. Optional serialized `Light` references: solar illumination, battery charge, DC bus healthy, eclipse, feedback fault, trusted-feedback healthy; optional energy-flow `ParticleSystem`. Bind them if authored. Do not invoke nonexistent setter methods.

# 11. PIVOT TYPES AND SPACE-TIME RELATIONSHIP

Read all 56 verified `Animation_Rigs.csv` records; Annex B prints exact name, owner, kind, axis, and XYZ metres. The existing rig rotates Earth/time visualization and moves eclipse/shadow graphics from the **orbital simulator state**, not from a lighting toggle. Port/starboard wings and gimbals rotate from **measured deployed angles**. MPPT dials use measured performance; contactors and battery trays follow actual mechanism states. Fault lights reflect actual critical fault or invalid W104 feedback.

Treat `Kind=indicator` as a display anchor, not a mechanical actuator. Preserve imported hierarchy and pivot home transforms. Prevent duplicate `ANIM_*` names across asset instances; validate all 56. Never set physical system values based on pivot positions.

# 12. EXACT 138 PLAYER-WIRE BUDGET

Use the **actual** `Gameplay_Wiring_Concept_W001_W138.csv` as authoritative for *planned* source model, target model, visual anchors, purpose and `SignalKind`. Do not substitute generic connection names. Group budgets:

| IDs | Group | Count |
|---|---|---:|
| W001–W010 | Educational 5 V control power/return | 10 |
| W011–W030 | Solar wing deployment/tracking | 20 |
| W031–W052 | PV generation/MPPT conversion | 22 |
| W053–W074 | Battery packs, cell sensing/BMS | 22 |
| W075–W092 | Main DC bus/distribution | 18 |
| W093–W110 | Energy measurement/diagnostics | 18 |
| W111–W126 | Mission loads/eclipse scheduler | 16 |
| W127–W138 | Safety, control/SCADA | 12 |
| **Total** | | **138** |

Do not precreate 138 connected cables at level start. Build real wire declarations/endpoints and require player wiring; register each successful actual wire through the existing electrical graph. Validate source-output/destination-input, distinct power vs analog vs digital domains, voltage range, common-reference/returns, and valid shared buses. A wire is PASS only if the circuit solver reports the correct live topology.

# 13. 212 VISUAL SOCKETS — REQUIRED ELECTRICAL RECONCILIATION

The package has exactly 212 **art-only** socket positions `P001`–`P212` with `WorldX_m/WorldY_m/WorldZ_m`, but 138 wires require **276 endpoint references**. Repeated endpoints are allowed **only** when electrically compatible and supported by a legitimate shared bus/junction.

**Important verified source conflict:** 145 of the 212 anchors are referenced by at least one wire; **46** are referenced more than once, and **26** have mixed `SignalKind` classifications. For example `P075` is used for W010 5 V power and W104 analog bus-voltage feedback, while `P195` is used for W002 power, W104 analog feedback, multiple digital control commands and other measurement signals. Never instantiate one electrical node that shorts these together merely because the art marks one socket position.

Create a deterministic **terminal-reconciliation report** mapping `WireID`, model, art anchor, **logical functional port** ID, type, direction and domain. Separate incompatible logical ports into distinct physical child connectors or offset sub-sockets on the equipment; preserve the 212 original decorative anchors and 138 wiring IDs. If the true Project Spark terminal API requires more than 212 actual physical terminals to support incompatible signals, produce a clear report and create the necessary additional ports rather than silently weakening circuit correctness. Keep the 212 original anchor records intact and label any extra ports. For a valid 212-terminal exact game target, redesign endpoint allocations explicitly and validate that no incompatible sharing remains.

# 14. W104 TRUSTED DC BUS FEEDBACK — EXACT PACKAGE DATA

**W104** is the only `Critical=YES` connection in the actual wiring CSV. Its exact planned row is:

- Source model: `L83_BusVoltageTransducer`.
- Source visual socket: **`P075`**.
- Proposed logical source terminal: **`MAIN_DC_BUS_VOLTAGE_OUT`**.
- Target model: `L83_MainBusEnergyManagementController`.
- Target visual socket: **`P195`**.
- Proposed logical destination terminal: **`EPS_TRUSTED_BUS_VOLTAGE_IN`**.
- Purpose: `CRITICAL trusted DC bus voltage control feedback`.
- Classification: `ANALOG_FEEDBACK`.

Build it as a **real, physically detachable player wire** and bind topology validity to the circuit graph. W105 is the separate calibration health signal. W106 is the independent undervoltage protector status; neither may substitute for W104 control feedback. SCADA historical readings and display telemetry cannot authorize normal bus regulation while W104 is missing.

# 15. THREE DISTINCT ENGINEERING DOMAINS

A. **5 V educational player control network:** `SparkTerminal`, switches, PLC control inputs, signal flags, analog measurement references, 138 player wires.

B. **Fictional spacecraft electrical power simulation:** photovoltaic source power, DC/DC conversion, bus capacitor and voltage, BMS/battery energy/cell groups, contactor switching, power distribution and load energy consumption; track real energy/time units or a fully documented normalized coherent system.

C. **Independent safety domain:** undervoltage protector, BMS limits, thermal/overcurrent trip conditions, emergency isolation, latched faults, safe-state outputs; never bypass critical protections with ordinary SCADA commands.

Keep the control-wire graph electrically truthful while representing high-energy plant behavior in the dedicated educational simulation, not as impossible 5 V control-wire power. Couple them via modeled devices and real feedback.

# 16. ONE AUTHORITATIVE ORBITAL ENERGY PROCESS STATE

Maintain authoritative records for orbital time, solar vector/eclipse fraction, actual wing deployment/azimuth/elevation/servo feedback, array incidence and temperature, per-string PV I–V operating point and bypass state, per-converter MPPT output and efficiency, bus voltage/current/capacitance, battery A/B SOC/SOH/temperature/cell-group state/contactor status, supercapacitor stored energy, five load buses and mission deadlines, predicted reserve, electrical sensor validity/age, disturbance state, independent safety state and historical objective evidence.

Do not make UI, visual rig, SCADA, PLC and physics each own contradictory copies of the same value. Commands, true plant state, sampled measurements, and derived qualification must be separate concepts.

# 17. DETERMINISTIC UPDATE ORDER AND TIME UNITS

Recommended fixed-step authoritative process:

1. Read actual Spark electrical graph, player commands and safety inputs.
2. Evaluate control power, interlocks and latched faults.
3. Advance orbit and compute sunlight/eclipse geometry.
4. Integrate deployment/gimbal motor motion with limits and jam state.
5. Calculate irradiance/incidence/temperature effects on each PV string.
6. Update PV source I–V and bypass behavior.
7. Update actual MPPT controllers and converter response.
8. Apply safety limits and precharge/contactor states.
9. Update PDU branch switches and scheduled mission load demand.
10. Solve bus capacitor/current balance and source/load powers.
11. Integrate battery and supercap energy and thermal state, obey constraints.
12. Generate sensor samples (calibration, range, timestamp, connectivity, freshness).
13. Enforce independent undervoltage and BMS protections.
14. Evaluate EPS PLC state, fault sequencer and mission task success.
15. Capture event evidence; update historical objective certification.
16. Push read-only state to SCADA and actual-state visual rig.

Use consistent seconds, watts, joules/Wh and Ampere-hour conversions. Clamp negative power/energy where physically impossible; preserve error indications instead of silently hiding numerical instability. Deterministic seeded fault injection for replay/tests.

# 18. SUN VECTOR, ECLIPSE FRACTION AND GAME ORBIT

Implement a fictional orbit simulator providing continuous sun direction, orbital angle, eclipse entry/exit and illumination fraction for each photovoltaic wing. Make a real geometric/analytic shadow model consistent with the displayed Earth occluder. At eclipse direct illumination → approximately zero (allow configured penumbra transitions). The two wings may differ in body shadow/orientation. Display `sunlight`, `eclipse`, `orbit angle`, future entry/exit forecasts separately.

The level's 45 s sunlight → 35 s eclipse → 40 s sunlight example is a **game duration preset**, not a realistic orbital period. Certification must start from a fresh `runId`, cross all transitions, and require a full *qualified* orbit with no hidden skipping by advancing the UI timeline.

# 19. SOLAR WING DEPLOYMENT AND GIMBAL DYNAMICS

The two wings `L83_DeployableSolarWingPort`/`L83_DeployableSolarWingStarboard` have separate hinges, deploy commands, azimuth and elevation gimbals, encoder feedback, actual positions, velocity, acceleration, soft/hard limits, latch conditions and fault states. Apply finite motor response, stiction/settling and mechanical limits. Define sunlight incidence using actual panel normal versus solar vector, not command setpoint. A jammed gimbal freezes/limits **actual** angle, lowers generation as alignment degrades, and creates encoder mismatch evidence.

Do not collapse the whole spacecraft into one solar-production slider. The visual rig uses measured deployment and gimbal angles only.

# 20. PHOTOVOLTAIC STRINGS AND I–V/P–V CURVES

Model two independent arrays comprising configurable series/parallel representative strings. At each timestep compute array I–V operating behavior (irradiance, temperature, optional degradation/shadow, bypass diode states) and power `P=V×I`. Under idealized sun incidence use effective incident light `G_eff ∝ illumination×max(0, dot(panelNormal,sunDirection))`. Include converter/input limits. Use documented approximations like a parametric I–V curve, not arbitrary output from a UI slider.

Provide plotted I–V and P–V data for both wings. Sweep only when safe and necessary; an I–V sweep itself temporarily changes the operating point, rather than being free diagnostic access to the maximum at all times.

# 21. BYPASS DIODES, PARTIAL SHADOW AND PV FAULTS

Track solar string bypass modules and optional partial shading conditions. Bypass diode conduction may reduce output voltage and create nontrivial P–V curves; under partial shade there can be local extrema. A simpler base preset may have one maximum; if advanced shading mode is enabled, model multiple peaks and ensure MPPT behavior follows the selected algorithm. Keep these as fictional educational models, not real solar-cell engineering certification. Do not allow array production through a fully eclipsed source.

# 22. DUAL MPPT CONTROL — DYNAMIC AND MEASURABLE

Port/starboard MPPT controllers must each read their own actual PV voltage/current (with calibration, noise and freshness), request converter duty/voltage changes and dynamically move an actual operating point. Implement perturb-and-observe or equivalent educational algorithm with settling, sampling interval, efficiency, control limits and oscillation near the maximum. A player can choose scanning or normal tracking modes, but must earn a valid lock through measured array performance.

Prevent maximum-power hunting from destabilizing the DC bus; allow power curtailment if batteries are full or loads are low. Do not assume maximizing PV source power is always the safe EPS command.

# 23. POWER CONVERTERS AND EFFICIENCY

Implement `L83_PowerConditioningConverterA/B` with command limits, actual conversion efficiency, input power cap, output current/voltage limit, finite response, and fault status. Model inductor/capacitor effects at a useful aggregate educational level. `P_out <= η × P_in` with documented, bounded `η`. Quarantine faulty converter values, enforce telemetry timestamps, and avoid source/sink algebra that magically creates power.

# 24. REGULATED DC BUS AND POWER CONSERVATION

Model `L83_RegulatedMainDCBus`, `L83_BusBulkCapacitorModule`, `L83_BusCurrentShuntMonitor`, `L83_BusVoltageTransducer`, PDU and load branches. Use a coherent bus-capacitor dynamic such as `dE_bus/dt=P_sources-P_loads-P_losses` and `E_bus=0.5*C*V_bus²`, with safeguarded low-voltage handling and finite converter response. Include battery charging as a bus *load* and battery discharging as a bus *source* at the correct efficiency. Compute instantaneous source-vs-load mismatch and bus transients.

SCADA '28.0 V' (if used as fictional design target) cannot be set as a constant regardless of input energy. Bus remains physically energized or de-energized according to actual sources, batteries and loads.

# 25. BUS PRECHARGE, CONTACTORS AND STARTUP ORDER

Implement real states: source isolation, BMS ready, pack contactor requested/actual, main bus capacitor charge state, precharge resistor path, precharge completion, safe main closure and fault handling. Prevent instantaneous high-current closure as an ordinary success path in the educational model. The package's `ANIM_MAIN_BUS_PRECHARGE`, `ANIM_BATTERY_CONTACTOR`, `L83_BatteryPrechargeResistorModule` and `L83_BatteryIsolationContactor` should animate actual states. A faulted contactor should not be declared closed merely because the player pressed ENABLE.

# 26. TWO BATTERY PACKS AND ENERGY INTEGRATION

Model Pack A and Pack B independently with actual stored energy, nominal capacity, SOC, temperature, internal resistance, max charge/discharge current, charge/discharge efficiency, cell-group conditions, pack available energy and protection. Example coherent update: `E_next=clamp(E + η_charge*P_charge*dt - P_discharge*dt/η_discharge, 0, E_max)`. If using Wh, divide watt-seconds by 3600. Never set SOC directly via HUD. Current draw, voltage sag and heat should respond to actual mission loads; discharged packs cannot provide unlimited energy.

# 27. CELL-GROUP SENSING, BALANCING AND BMS

Each battery has multiple representative series cell groups with individual sensed voltage, imbalance, temperature, health and balance command. The BMS checks pack/group charge and discharge protection limits. Balancing is finite, consumes/dissipates energy, and cannot fix permanent degraded capacity in an instant. A pack with high average SOC but one weak limiting group can hit a protection threshold early. Use seeded deterministic state for the imbalance disturbance and record any rejected pack/replacement action. Do not call balancing an instant reset.

# 28. BATTERY THERMAL RESPONSE AND LIMITS

Model thermal mass, electrical loss heat `I²R` proxy, cooling-plate/radiator response, heater load, environmental temperature and sensor validity. During shadow the spacecraft may need a minimum thermal-control demand even when seeking low energy use. Excess heat or cold can limit permissible battery current. Balance heat generation and dissipation over time. Avoid real industrial battery testing instructions; this is a game-safe abstract simulation.

# 29. SUPERCAPACITOR RIDE-THROUGH

The `L83_SupercapacitorRideThroughModule` provides a bounded transient energy store with actual voltage/energy, current limit, charge/discharge efficiency and response time. On sudden optical burst demand, it can supply a brief pulse while converters/batteries respond. It cannot run the spacecraft through 35 seconds of eclipse after its stored energy is depleted. Include precharge and recharge as real energy costs.

# 30. PRIORITY LOAD BUS ARCHITECTURE

Create independent P0 essential, P1 attitude/required thermal, P2 scheduled optical data and other mission jobs, and P3 optional science/diagnostics. Use existing individual models `L83_SafetyEssentialLoadBus`, `L83_AttitudeControlLoadEmulator`, `L83_MissionComputerLoadEmulator`, `L83_OpticalTelemetryLoadEmulator`, `L83_SciencePayloadLoadEmulator`, `L83_HeaterThermalLoadEmulator`. Each load has requested/actual enable, power demand, startup transient, minimum duty, deadlines, telemetry and effect on the mission.

Loads must obey real switch state and bus limits. Do not make optional load shedding fabricate power. Essential load may still fail if the battery is truly depleted: report realistic survival failure, not infinite priority protection.

# 31. MISSION TASK SCHEDULER AND DEADLINE RULES

Model useful tasks: attitude control intervals, sensor acquisition, essential compute, battery thermal maintenance, an optical downlink, optional science capture. Maintain task deadlines and energy cost. Player can shift optical communications into daylight, reduce science duty, precharge before eclipse, and restore deferred tasks after sunrise. **All loads OFF** cannot pass mission utility. Check valid completed tasks in addition to no brownout. Distinguish task requested/active/success/expired.

# 32. ECLIPSE FORECAST AND BUDGET

The `L83_LoadForecastEnergyBudgetAnalyzer`, `L83_EclipseEntryPredictionConsole` and `L83_SunlightReturnChargePlanner` should use current observed state, future orbit illumination, mission plan, pack limits and converter efficiency to estimate expected eclipse-exit SOC, bus margin, and task completion likelihood. The forecast is an estimate, may incorporate uncertainty, and must not have perfect foreknowledge of undisclosed disturbances. Compare predicted vs actual outcomes after a completed orbit. Use a trend/history graph for P_solar, P_load, V_bus and E_battery.

# 33. FIRST FULL ORBIT QUALIFICATION

Begin with a new `runId` and snapshot needed safety/equipment status. Require the real simulated transitions **sunlight → eclipse → sunlight**, all required periods/loads, power and thermal limits, mission deadlines, post-eclipse recharge and adequate reserve. Store time-stamped evidence and acceptance margins, not a simple 120-second timer. A skipped eclipse, manual teleport, zero power demand or unauthorized protective bypass must fail. Report power balance with a deterministic integrator.

# 34. DISTURBANCE A — SOLAR GIMBAL JAM

Introduce a jam on **port** gimbal actual motion only; commanded pose continues changing, encoder feedback stops/responds inconsistently, incidence worsens and port PV output decreases despite healthy MPPT. Diagnosis uses sun vector, requested-vs-actual angle, PV string current and controller readings. Recovery: safe motion inhibit, diagnose jam, restore fictional mechanical condition, home/recalibrate encoder, reacquire sun and requalify port/starboard arrays. Do not let merely increasing MPPT command compensate for the missing photons. Must complete subsequent fresh orbit.

# 35. DISTURBANCE B — BATTERY CELL-GROUP IMBALANCE

Introduce a real difference in a selected cell-group state/health on Pack B or A. Pack SOC can appear healthy while one group hits a current or voltage limit. Diagnosis via actual cell voltages, temperature, current, contactor and BMS flags. Recovery: reduce load/charge demand, rebalance within finite safe limits, isolate or replace a permanently degraded pack with qualified inventory when necessary, requalify usable energy and redo the full orbit. Do not wipe the affected group by resetting an alarm.

# 36. DISTURBANCE C — OPTICAL BURST DEMAND

Introduce an unexpected high-power optical telemetry event during eclipse. The demand increases P2 load, potentially dips bus voltage, draws supercap, increases battery current and competes with essentials. Diagnosis via transient recorder, actual load branch telemetry, V_bus, battery currents and scheduler. Recovery: preserve critical loads, cap/reschedule noncritical burst, wait for stable bus, transmit required data in a permissible later slot and verify mission deadlines. The surge must be actual consumed energy, not only a SCADA alarm.

# 37. EPS PLC STATES

Implement states such as: `OFF`, `INITIALIZING`, `WIRING_CHECK`, `CONTROL_POWER_OK`, `INDEPENDENT_SAFETY_TEST`, `SOLAR_DEPLOYMENT`, `GIMBAL_HOMING`, `PV_MEASUREMENT`, `MPPT_COMMISSIONING`, `BATTERY_BMS_CHECK`, `PRECHARGE`, `BUS_REGULATION`, `ENERGY_BUDGET_PLANNING`, `SUNLIGHT_OPERATION`, `ECLIPSE_PREP`, `ECLIPSE_OPERATION`, `SUNLIGHT_RETURN`, `RECHARGE`, `DISTURBANCE_RECOVERY`, `POWER_SAFE`, `CRITICAL_FAULT_LATCHED`, `RESET_REQUIRED`, `FINAL_CERTIFIED`. Transitions require actual source, power, sensor, safety and mission evidence rather than decorative animation or elapsed-time triggers alone.

# 38. INDEPENDENT SAFETY, W104 AND FALLBACK BEHAVIOR

Use `L83_EPSPowerSafetyPLC`, `L83_IndependentUndervoltageProtector`, `L83_EmergencyPowerIsolationStation` and BMS protection. An invalid W104 does **not** shut down the physical bus itself; it withdraws *normal closed-loop regulation authorization* from the energy-management supervisor. The safety system commands safe fallback: inhibit new high-power loads and charging, reduce optional loads, constrain converter/battery modes, preserve P0/P1 if physically supportable, and hold mission scheduling. Independent undervoltage/overcurrent/thermal protections remain active. Capture actual bus state and event logs. Never silently bypass missing critical voltage using W105 health or W106 independent protector status.

# 39. W104 PHYSICAL DISCONNECTION AND VALIDITY

Make the player disconnect real wire **W104 P075→P195** after Objectives 1–5 historically certify. This must mutate the circuit topology. Mark samples invalid when missing/stale; invalidate voltage feedback and the normal bus regulation qualification, **without overwriting actual V_bus**. Trigger independent EPS safety fallback and load shedding, latch a critical fault, illuminate actual W104 alarm, and block new energy-heavy mission operations. The model must still conserve power; if battery energy cannot support essential loads, fail gracefully and log it. W105 calibration-health and independent undervoltage W106 are not alternate W104 measurements.

# 40. W104 RECONNECT, RECALIBRATION AND EXPLICIT RESET

After actual W104 reconnection, verify compatible electrical topology, 5 V sensor power and returns, fresh timestamps, realistic sensor range/offset/calibration checks, agreement with independent protection *only as a safety cross-check*, converter current constraints, contactor protection, and separate independent undervoltage health. Keep latched safety until **explicit operator reset** is accepted. Re-enable normal regulation gradually, respecting precharge and state limits; do not instantly move bus voltage. Generate new qualification `runId`; prior test-run evidence cannot be reused for Objective 6.

# 41. OBJECTIVE 1 — BUILD THE ORBITAL ENERGY NETWORK

Certificate `1/6` only when the real circuit graph has exactly 138 correct player-created connections, valid functional terminal reconciliation preserving all 212 original socket locations, powered controllers with valid returns, functional MPPT, BMS, EPS PLC, independent protection, actual W104 connected and valid. Do not count 138 decorative wires as success. Capture topology hash and real safety health in objective evidence. Title from source: **Build the Orbital Energy Network**.

# 42. OBJECTIVE 2 — CAPTURE AND REGULATE SOLAR ENERGY

Certificate `2/6` after physically deployed and homed wings, actual incidence and PV I–V measurements from both wings, independent MPPT near available operating points, qualified converter output, safe bus precharge and stable regulated DC bus under changing sunlight. Both sensor and mechanical feedback must be fresh and calibrated. Source title: **Capture and Regulate Solar Energy**.

# 43. OBJECTIVE 3 — QUALIFY BATTERY STORAGE AND DISTRIBUTION

Certificate `3/6` only after battery A/B current/cell-group voltage/temperature monitoring, safe charging and discharging, finite balancing proof, correct precharge/contactor state, functioning thermal management, correct load priority switching, and valid independent protection. Battery SOC must be physically consistent with measured power integration. Source title: **Qualify Battery Storage and Distribution**.

# 44. OBJECTIVE 4 — SURVIVE FIRST COMPLETE ECLIPSE

Certificate `4/6` after a **fresh full** sunlight → eclipse → sunlight cycle with no unqualified brownouts, enough actual battery reserve, P0/P1 essential load continuity, valid timing/deadlines for required mission work, safe thermal state, and post-eclipse energy recovery. Use run-scoped evidence; a countdown animation is not evidence. Source title: **Survive the First Complete Eclipse**.

# 45. OBJECTIVE 5 — RECOVER THREE POWER-SYSTEM DISTURBANCES

Certificate `5/6` only if all three disturbances were actually activated, changed authoritative plant states, were diagnosed and repaired, and the player subsequently completed a **new** qualified orbit with true bus stability, energy margin, thermal safety and mission jobs. Source title: **Recover Three Power-System Disturbances**.

# 46. OBJECTIVE 6 — RESTORE CRITICAL DC BUS FEEDBACK

Require historical Objectives 1–5, an intentional actual W104 disconnect, invalidated trusted bus voltage, safe load shed/fallback and latched fault, real reconnection, fresh samples, calibration/independent protection checks, explicit reset, and **another brand-new complete** sunlight → eclipse → sunlight orbit with valid MPPT, battery/SOC/cell and temperature limits, bus regulation, mission deadlines and no unresolved critical fault. Only then award `6/6` from the actual `LevelGamePlayManager`. Do not clear historical badges or award on reconnection alone. Source title: **Restore Critical DC Bus Feedback**.

# 47. AUTHORITATIVE OBJECTIVE HISTORY AND RUN IDS

Store `ObjectiveCertifiedAt`, simulation-event IDs, `runId`, equipment state fingerprints, electrical topology evidence, sensor validity and quality margins. Historical objectives 1–5 stay certified when deliberate W104 disconnection occurs; live machine health can still turn red. Objective 6 needs `postW104RepairRunId > faultRunId` and a fully completed verified fresh cycle. Prevent a duplicate builder or scene reload from artificially awarding progress. Support deterministic save/load according to existing Project Spark framework.

# 48. SCADA DASHBOARD — LIVE ENGINEERING DATA

Display: orbit time, predicted entry/exit, Sun vector, real wing positions/angle error; port/starboard string V/I/P and I–V/P–V curves; MPPT commanded and measured operating point/efficiency; actual 5 V control power; converter loss/limits; bus V/I/P, P_in/P_out, transient waveform, precharge state; Pack A/B SOC, Wh, current, cell-group min/max, imbalance, temp and BMS faults; supercap voltage/energy; active P0–P3 loads; predicted eclipse-exit reserve; deferred task deadlines; W104 physical connection, sample freshness, calibration, independent undervoltage protector and fault latch; historical objectives 0/6–6/6. Every number must come from authority, not a decorative formula hardcoded to green.

# 49. PLAYER CONTROLS AND INTERACTION DESIGN

Provide usable panel actions: MAIN 5V, SAFETY SELF-TEST, DEPLOY PORT, DEPLOY STARBOARD, HOME GIMBALS, SUN TRACKING, PORT PV SWEEP, STARBOARD PV SWEEP, MPPT MODE, POWER CONVERTER ENABLE, PRECHARGE, CLOSE MAIN CONTACTOR, CALIBRATE BUS SENSOR, COMMISSION PACK A/B, CELL SCAN, BALANCE, CHARGE LIMIT, LOAD P0/P1/P2/P3, ECLIPSE FORECAST, SCHEDULE DOWNLINK, START ORBIT RUN, SHED OPTIONAL, SUNLIGHT RETURN, DIAGNOSE JAM, REPAIR GIMBAL, BALANCE PACK, LIMIT OPTICAL BURST, W104 DISCONNECT/RECONNECT through real player wire tool, CALIBRATE, EXPLICIT SAFETY RESET, FINAL QUALIFICATION. Visual feedback must distinguish requested, actual and qualified state.

# 50. VISUAL FX AND AAA ORBITAL THEATER

Art direction: enormous suspended spacecraft with dual wing deployment; Earth hologram and moving eclipse cone; gold incoming solar incidence rays, cyan actual bus flow, green charging, blue battery discharge, amber reserve warning, red critical faults, violet shadow. Use real source/energy state to vary light/particles. Cell-grid shaders should update efficiently (GPU instancing/gradient maps) rather than thousands of per-cell scripts. Disturbance A shows one wing lagging actual sun angle; B reveals unequal cell-group bar states; C shows transient bus dip and supercap effect; final W104 fault changes status/hologram without magically stopping all electrical power.

# 51. RECOMMENDED C# COMPONENTS — ADAPT TO REAL CODEBASE

Proposed new classes only where missing: `SparkLevel83Coordinator`, `SparkLevel83ProcessState`, `SparkLevel83ElectricalBindings`, `SparkLevel83OrbitalSunlightModel`, `SparkLevel83SolarArrayDynamics`, `SparkLevel83PVStringModel`, `SparkLevel83MPPTController`, `SparkLevel83ConverterModel`, `SparkLevel83MainDCBus`, `SparkLevel83BatteryPackModel`, `SparkLevel83BMS`, `SparkLevel83BatteryThermal`, `SparkLevel83Supercapacitor`, `SparkLevel83PowerDistribution`, `SparkLevel83MissionLoadScheduler`, `SparkLevel83EnergyForecast`, `SparkLevel83IndependentUndervoltageProtection`, `SparkLevel83EnergyManagementController`, `SparkLevel83PowerSafetyPLC`, `SparkLevel83DisturbanceController`, `SparkLevel83W104FaultRecovery`, `SparkLevel83ObjectiveTracker`, `SparkLevel83OrbitalPowerSCADA`, `SparkLevel83VisualRigAdapter`. Compose clean interfaces and tests. Avoid duplicated authoritative process state and avoid speculation about existing APIs.

# 52. ONE-CLICK LEVEL 83 EDITOR BUILDER

Create **`Tools/Project Spark/Levels/Create Level 83`**. It must inspect real code, create or idempotently update `Level_83.asset` and `Level_83_Layout.prefab`, 74 common-origin model instances under the disposable art root, correct import scaling/materials, 212 source decorative anchor records plus reconciled functional terminal ports, 138 player-wire declarations, separate control/power/safety domains, orbit/PV/MPPT/converter/battery/bus/load physics, three disturbances, W104 fault sequence, six objective definitions, 56 pivot bindings, SCADA, VFX, menus, diagnostics and validation report.

Reruns must not duplicate functional terminals, spawn duplicate circuit solvers, overwrite prior levels, or wipe customized gameplay logic. Report stale/missing model/port refs; preserve authored gameplay children outside `L83_HD_VISUAL_ART`. Generate a reconciliation CSV for any split art sockets.

# 53. ACCEPTANCE TESTS — STRUCTURAL AND ELECTRICAL

Execute in the actual Unity project where available:

1. 74 distinct numbered FBXs imported (and 74 GLBs available).
2. Workstation bounds approximately 26.20 m wide/9.29 m high.
3. Exactly 56 `ANIM_*` pivot parents discovered.
4. No duplicate names or wrong pivot axes/hierarchy.
5. All 212 original `P###` art anchors found; logical terminal mapping reported.
6. Every one of 138 WIDs unique, correct source/target, player-connectable.
7. Conflicting signal uses at P075, P195 and elsewhere split safely; no cross-domain shorts.
8. 5 V control + returns qualified, separate plant power domain.
9. W104 is a live, physically disconnectable real circuit edge.
10. Component missing/source unavailable diagnostics work.
11. Art placement rerun is idempotent and doesn't delete gameplay.
12. Levels 1–82 still load correctly.

Do not label a test PASS without actually executing it.

# 54. ACCEPTANCE TESTS — ORBIT, PV, MPPT, BUS

13. Sunlight fraction evolves by real orbit simulation.
14. Eclipse reduces direct PV input near zero.
15. Port/starboard wing deployment requires real actuator time.
16. Gimbal actual angle lags commanded angle appropriately.
17. Jam freezes actual angle and degrades incidence/power.
18. Port and starboard I–V curves are distinct and coherent.
19. MPPT moves real operating voltage/current and has bounded dynamics.
20. Converter P_out never exceeds permitted P_in×efficiency.
21. Curtailment works when battery full/demand low.
22. Bus precharge operates before full contactor closure.
23. Transient bus voltage responds to mismatch and capacitive energy.
24. Charge/discharge energy is conserved.
25. No negative SOC/energy or infinite current created.
26. Stale PV or bus samples cannot be treated as fresh for certification.
27. Independent undervoltage protector trips based on actual protected measurement.

# 55. ACCEPTANCE TESTS — BATTERY, LOADS AND DISTURBANCES

28. Pack A/B independently store energy and report SOC.
29. Cell-group imbalance affects usable reserve/limits.
30. BMS prevents unsafe charging/discharging.
31. Balancing has finite rate and measurable energy/heat.
32. Thermal plate/radiator affects temperature over time.
33. Supercap energy is finite and handles short transient only.
34. P0 essential and P1 critical remain powered when physically feasible.
35. All-loads-off shortcut fails mission tasks.
36. Deferred optical downlink can succeed before deadline.
37. Eclipse forecast compares to actual outcomes.
38. Full orbit progress requires actual sunlight→eclipse→sunlight states.
39. Gimbal jam disturbance changes physical plant and can be repaired.
40. Cell imbalance disturbance changes physical pack state and can be recovered or replaced.
41. Optical burst disturbance affects P_load, bus and supercap.
42. Three recoveries are recorded and a fresh orbit is actually run.

# 56. ACCEPTANCE TESTS — W104 CRITICAL SAFETY AND OBJECTIVES

43. Disconnect W104 through actual wire UI, mutate graph.
44. W104 measurement becomes invalid/stale even if cached voltage exists.
45. W105 calibration health cannot bypass W104.
46. W106 undervoltage status cannot substitute for W104 normal control.
47. Actual bus voltage is **not** forced to zero on disconnect.
48. Essential loads are preserved where battery/physical conditions permit.
49. Optional high-power loads are shed/inhibited in power-safe fallback.
50. Independent protection and BMS remain active.
51. Critical fault latches and reset is refused while W104 disconnected.
52. Reconnect W104 and acquire fresh calibrated samples.
53. Explicit reset required and safety conditions checked.
54. Fresh post-repair full cycle required; old evidence refused.
55. Objectives 1–5 stay historically certified during fault.
56. Objective 6 does **not** pass on reconnection alone.
57. Actual `LevelGamePlayManager` reaches 6/6 after valid fresh run.
58. All relevant Unity compilation/Play Mode testing results reported truthfully.

Keep red/green SCADA consistent with actual underlying status.

# 57. GAMEPLAY WALKTHROUGH — REQUIRED REAL SEQUENCE

**CONNECT 138 REAL WIRES → RECONCILE/CREATE FUNCTIONAL TERMINALS AT 212 ART ANCHORS → 5 V CONTROL POWER → INDEPENDENT SAFETY TEST → DEPLOY PORT AND STARBOARD WINGS → HOME GIMBALS → TRACK SIMULATED SUN → MEASURE PV STRINGS → COMMISSION DUAL MPPT → APPLY CONVERTER LIMITS → PRECHARGE MAIN BUS → VERIFY ACTUAL BUS VOLTAGE → COMMISSION BOTH BATTERY PACKS AND BMS → CALIBRATE CELL VOLTAGE/CURRENT/TEMPERATURE → VERIFY BALANCING AND CONTACTORS → CONFIGURE P0/P1/P2/P3 LOADS → PLAN OPTICAL DOWNLINK AND ECLIPSE RESERVE → RUN FRESH SUNLIGHT → ENTER TRUE ECLIPSE → SHED OPTIONAL LOADS → PRESERVE ESSENTIALS → EXIT ECLIPSE → RECHARGE → CERTIFY FIRST ORBIT → TRIGGER SOLAR GIMBAL JAM → DIAGNOSE/REPAIR → TRIGGER BATTERY IMBALANCE → DIAGNOSE/RECOVER → TRIGGER OPTICAL LOAD SURGE → PROTECT BUS AND RESCHEDULE → PASS FRESH ORBIT → PHYSICALLY DISCONNECT W104 → STALE BUS FEEDBACK → POWER-SAFE FALLBACK + LOAD SHED → LATCH CRITICAL FAULT → PHYSICALLY RECONNECT W104 → FRESH SAMPLES + CALIBRATION → VERIFY INDEPENDENT PROTECTION → EXPLICIT RESET → RUN BRAND-NEW SUNLIGHT→ECLIPSE→SUNLIGHT CYCLE → PASS ENERGY/SAFETY/MISSION TESTS → AUTHORITATIVE 6/6.**

# 58. ERRORS, DIAGNOSTICS AND VALIDATION REPORT

Emit structured errors and actionable messages for missing FBX/GLB, wrong FBX scale, duplicate pivots, missing P IDs, unresolved multi-class anchor conflicts, 5 V supply/returns missing, solar wing deploy/encoder faults, MPPT operating point stale/unqualified, converter overcurrent, failed precharge, bus over/undervoltage, missing W104/stale sample, BMS cell group mismatch, SOC exhaustion, contactor open, thermal limits, excessive load, missed mission deadline, forecast reserve shortfall, supercap depletion, jam, cell imbalance, optical demand surge, safety inhibit, invalid explicit reset, objectives stuck at 0/6 or 5/6, unintended history reset, older-level regression. Generate an import, wiring/terminal reconciliation and mission-simulation test report. **Never manufacture passing results.**

# 59. DELIVERABLES AND FINAL SUCCESS CONTRACT

Provide real Unity source, compiled/importable `Level_83.asset`, `Level_83_Layout.prefab`, 74 modular art bindings, verified 56-pivot visual rig, correctly reconciled functional terminal interfaces at the 212 planned socket locations, 138 genuine player-wire connections, solar/PV/MPPT plant, converter dynamics, BMS/battery/thermal, regulated DC bus, supercap, mission load scheduler, orbit and forecast, independent safety PLC, three disturbances and repairs, W104 physical disconnect/recalibration/reset, six authoritative objectives, SCADA, VFX, one-click Editor builder, tests and honest results.

Only report **Level 83 complete** if actual Unity compilation and gameplay have demonstrated full sunlight→eclipse→sunlight survival after W104 recovery and the real `LevelGamePlayManager` reads **6/6**. Do not alter Levels 1–82.

**THE SUN CAN DISAPPEAR. THE MISSION MUST NOT. CAPTURE. STORE. DISTRIBUTE. SURVIVE BEYOND THE SHADOW.**

---

# SOURCE-DERIVED APPENDICES (DO NOT REPLACE WITH GUESSES)

## ANNEX A — EXACT 74 MODEL FILENAMES FROM MODEL MANIFEST

| # | FBX filename | GLB filename | Triangles | Mesh parts |
|---:|---|---|---:|---:|
| 00 | `00_L83_OrbitalEnergyQualificationArenaBase.fbx` | `00_L83_OrbitalEnergyQualificationArenaBase.glb` | 912 | 76 |
| 01 | `01_L83_PanoramicOrbitShadowDome.fbx` | `01_L83_PanoramicOrbitShadowDome.glb` | 23,356 | 491 |
| 02 | `02_L83_TransparentSpacecraftPowerTestChamber.fbx` | `02_L83_TransparentSpacecraftPowerTestChamber.glb` | 240 | 20 |
| 03 | `03_L83_FourChipletMissionComputerPayload.fbx` | `03_L83_FourChipletMissionComputerPayload.glb` | 1,356 | 77 |
| 04 | `04_L83_SpacecraftServiceBusStructure.fbx` | `04_L83_SpacecraftServiceBusStructure.glb` | 960 | 38 |
| 05 | `05_L83_DeployableSolarWingPort.fbx` | `05_L83_DeployableSolarWingPort.glb` | 16,036 | 1,146 |
| 06 | `06_L83_DeployableSolarWingStarboard.fbx` | `06_L83_DeployableSolarWingStarboard.glb` | 16,036 | 1,146 |
| 07 | `07_L83_SolarWingPortHingeAssembly.fbx` | `07_L83_SolarWingPortHingeAssembly.glb` | 4,088 | 33 |
| 08 | `08_L83_SolarWingStarboardHingeAssembly.fbx` | `08_L83_SolarWingStarboardHingeAssembly.glb` | 4,088 | 33 |
| 09 | `09_L83_SolarArrayPortTrackingGimbal.fbx` | `09_L83_SolarArrayPortTrackingGimbal.glb` | 2,196 | 32 |
| 10 | `10_L83_SolarArrayStarboardTrackingGimbal.fbx` | `10_L83_SolarArrayStarboardTrackingGimbal.glb` | 2,196 | 32 |
| 11 | `11_L83_SolarArrayAzimuthEncoder.fbx` | `11_L83_SolarArrayAzimuthEncoder.glb` | 960 | 11 |
| 12 | `12_L83_SolarIncidenceVectorProjector.fbx` | `12_L83_SolarIncidenceVectorProjector.glb` | 8,880 | 159 |
| 13 | `13_L83_ProgrammableSolarIlluminationArc.fbx` | `13_L83_ProgrammableSolarIlluminationArc.glb` | 7,700 | 134 |
| 14 | `14_L83_EarthShadowOccluderStage.fbx` | `14_L83_EarthShadowOccluderStage.glb` | 13,164 | 240 |
| 15 | `15_L83_OrbitalDayNightHologram.fbx` | `15_L83_OrbitalDayNightHologram.glb` | 24,156 | 412 |
| 16 | `16_L83_SolarArrayCellStringMap.fbx` | `16_L83_SolarArrayCellStringMap.glb` | 2,004 | 96 |
| 17 | `17_L83_PVStringPortIVTracer.fbx` | `17_L83_PVStringPortIVTracer.glb` | 2,928 | 89 |
| 18 | `18_L83_PVStringStarboardIVTracer.fbx` | `18_L83_PVStringStarboardIVTracer.glb` | 2,928 | 89 |
| 19 | `19_L83_PVStringBypassDiodeBank.fbx` | `19_L83_PVStringBypassDiodeBank.glb` | 3,044 | 96 |
| 20 | `20_L83_SolarArrayCombinerBox.fbx` | `20_L83_SolarArrayCombinerBox.glb` | 1,536 | 45 |
| 21 | `21_L83_SolarArrayCurrentSensors.fbx` | `21_L83_SolarArrayCurrentSensors.glb` | 904 | 24 |
| 22 | `22_L83_SolarArrayVoltageSensors.fbx` | `22_L83_SolarArrayVoltageSensors.glb` | 904 | 24 |
| 23 | `23_L83_MPPTControllerPort.fbx` | `23_L83_MPPTControllerPort.glb` | 2,828 | 82 |
| 24 | `24_L83_MPPTControllerStarboard.fbx` | `24_L83_MPPTControllerStarboard.glb` | 2,828 | 82 |
| 25 | `25_L83_MPPTPerturbObserveDisplay.fbx` | `25_L83_MPPTPerturbObserveDisplay.glb` | 1,404 | 70 |
| 26 | `26_L83_PVMaximumPowerOperatingPointHologram.fbx` | `26_L83_PVMaximumPowerOperatingPointHologram.glb` | 8,516 | 202 |
| 27 | `27_L83_PowerConditioningConverterA.fbx` | `27_L83_PowerConditioningConverterA.glb` | 3,564 | 125 |
| 28 | `28_L83_PowerConditioningConverterB.fbx` | `28_L83_PowerConditioningConverterB.glb` | 3,564 | 125 |
| 29 | `29_L83_RegulatedMainDCBus.fbx` | `29_L83_RegulatedMainDCBus.glb` | 1,136 | 32 |
| 30 | `30_L83_BusVoltageTransducer.fbx` | `30_L83_BusVoltageTransducer.glb` | 1,128 | 27 |
| 31 | `31_L83_BusCurrentShuntMonitor.fbx` | `31_L83_BusCurrentShuntMonitor.glb` | 1,128 | 27 |
| 32 | `32_L83_BusBulkCapacitorModule.fbx` | `32_L83_BusBulkCapacitorModule.glb` | 1,272 | 29 |
| 33 | `33_L83_HighSpeedPowerTransientRecorder.fbx` | `33_L83_HighSpeedPowerTransientRecorder.glb` | 2,468 | 84 |
| 34 | `34_L83_BatteryPackA.fbx` | `34_L83_BatteryPackA.glb` | 22,336 | 412 |
| 35 | `35_L83_BatteryPackB.fbx` | `35_L83_BatteryPackB.glb` | 22,336 | 412 |
| 36 | `36_L83_ProtectedBatteryCellTray.fbx` | `36_L83_ProtectedBatteryCellTray.glb` | 888 | 27 |
| 37 | `37_L83_BatteryCellVoltageTapArray.fbx` | `37_L83_BatteryCellVoltageTapArray.glb` | 1,508 | 51 |
| 38 | `38_L83_BatteryCurrentHallSensor.fbx` | `38_L83_BatteryCurrentHallSensor.glb` | 1,268 | 19 |
| 39 | `39_L83_BatteryThermalSensorGrid.fbx` | `39_L83_BatteryThermalSensorGrid.glb` | 4,288 | 116 |
| 40 | `40_L83_BatteryManagementController.fbx` | `40_L83_BatteryManagementController.glb` | 2,692 | 87 |
| 41 | `41_L83_BatteryBalancingCircuit.fbx` | `41_L83_BatteryBalancingCircuit.glb` | 2,836 | 87 |
| 42 | `42_L83_BatteryChargeController.fbx` | `42_L83_BatteryChargeController.glb` | 2,384 | 77 |
| 43 | `43_L83_BatteryDischargeController.fbx` | `43_L83_BatteryDischargeController.glb` | 2,384 | 77 |
| 44 | `44_L83_BatteryIsolationContactor.fbx` | `44_L83_BatteryIsolationContactor.glb` | 916 | 23 |
| 45 | `45_L83_BatteryPrechargeResistorModule.fbx` | `45_L83_BatteryPrechargeResistorModule.glb` | 2,396 | 78 |
| 46 | `46_L83_SupercapacitorRideThroughModule.fbx` | `46_L83_SupercapacitorRideThroughModule.glb` | 1,288 | 31 |
| 47 | `47_L83_BatteryThermalControlPlate.fbx` | `47_L83_BatteryThermalControlPlate.glb` | 1,096 | 18 |
| 48 | `48_L83_BatteryRadiatorFinAssembly.fbx` | `48_L83_BatteryRadiatorFinAssembly.glb` | 1,300 | 35 |
| 49 | `49_L83_PowerDistributionUnit.fbx` | `49_L83_PowerDistributionUnit.glb` | 2,432 | 81 |
| 50 | `50_L83_PriorityLoadSwitchingRack.fbx` | `50_L83_PriorityLoadSwitchingRack.glb` | 2,492 | 86 |
| 51 | `51_L83_SafetyEssentialLoadBus.fbx` | `51_L83_SafetyEssentialLoadBus.glb` | 780 | 18 |
| 52 | `52_L83_AttitudeControlLoadEmulator.fbx` | `52_L83_AttitudeControlLoadEmulator.glb` | 2,468 | 84 |
| 53 | `53_L83_MissionComputerLoadEmulator.fbx` | `53_L83_MissionComputerLoadEmulator.glb` | 2,468 | 84 |
| 54 | `54_L83_OpticalTelemetryLoadEmulator.fbx` | `54_L83_OpticalTelemetryLoadEmulator.glb` | 2,468 | 84 |
| 55 | `55_L83_SciencePayloadLoadEmulator.fbx` | `55_L83_SciencePayloadLoadEmulator.glb` | 2,468 | 84 |
| 56 | `56_L83_HeaterThermalLoadEmulator.fbx` | `56_L83_HeaterThermalLoadEmulator.glb` | 2,468 | 84 |
| 57 | `57_L83_MissionTaskPowerScheduler.fbx` | `57_L83_MissionTaskPowerScheduler.glb` | 2,468 | 84 |
| 58 | `58_L83_LoadForecastEnergyBudgetAnalyzer.fbx` | `58_L83_LoadForecastEnergyBudgetAnalyzer.glb` | 2,468 | 84 |
| 59 | `59_L83_EclipseEntryPredictionConsole.fbx` | `59_L83_EclipseEntryPredictionConsole.glb` | 2,384 | 77 |
| 60 | `60_L83_SunlightReturnChargePlanner.fbx` | `60_L83_SunlightReturnChargePlanner.glb` | 2,384 | 77 |
| 61 | `61_L83_ContingencyPowerFaultInjector.fbx` | `61_L83_ContingencyPowerFaultInjector.glb` | 2,384 | 77 |
| 62 | `62_L83_SolarGimbalJamSimulator.fbx` | `62_L83_SolarGimbalJamSimulator.glb` | 1,344 | 32 |
| 63 | `63_L83_BatteryCellImbalanceSimulator.fbx` | `63_L83_BatteryCellImbalanceSimulator.glb` | 876 | 26 |
| 64 | `64_L83_OpticalBurstDemandInjector.fbx` | `64_L83_OpticalBurstDemandInjector.glb` | 876 | 26 |
| 65 | `65_L83_IndependentUndervoltageProtector.fbx` | `65_L83_IndependentUndervoltageProtector.glb` | 2,396 | 78 |
| 66 | `66_L83_EPSPowerSafetyPLC.fbx` | `66_L83_EPSPowerSafetyPLC.glb` | 2,384 | 77 |
| 67 | `67_L83_MainBusEnergyManagementController.fbx` | `67_L83_MainBusEnergyManagementController.glb` | 2,384 | 77 |
| 68 | `68_L83_Protected5VControlSupply.fbx` | `68_L83_Protected5VControlSupply.glb` | 2,384 | 77 |
| 69 | `69_L83_OrbitalPowerSCADA.fbx` | `69_L83_OrbitalPowerSCADA.glb` | 1,428 | 72 |
| 70 | `70_L83_ThreeDimensionalEnergyFlowHologram.fbx` | `70_L83_ThreeDimensionalEnergyFlowHologram.glb` | 11,348 | 218 |
| 71 | `71_L83_MissionSurvivalCertificationBeacon.fbx` | `71_L83_MissionSurvivalCertificationBeacon.glb` | 1,268 | 18 |
| 72 | `72_L83_EmergencyPowerIsolationStation.fbx` | `72_L83_EmergencyPowerIsolationStation.glb` | 1,044 | 18 |
| 73 | `73_L83_SpacecraftPowerThermalInspectionCamera.fbx` | `73_L83_SpacecraftPowerThermalInspectionCamera.glb` | 1,108 | 18 |

## ANNEX B — ALL 56 ACTUAL ANIMATION PIVOT PARENTS

Coordinates below are from the art CSV in metres. `Kind=indicator` means display-only.

| # | Pivot parent | Owning model | Kind | Axis | X | Y | Z |
|---:|---|---|---|---|---:|---:|---:|
| 1 | `ANIM_ORBIT_EARTH_TURN` | `L83_OrbitalDayNightHologram` | rotation | `y` | 0 | 7.62 | -4.45 |
| 2 | `ANIM_SUN_VECTOR_ARC` | `L83_ProgrammableSolarIlluminationArc` | rotation | `y` | -10.5 | 7.88 | -3.4 |
| 3 | `ANIM_ECLIPSE_OCCLUDER` | `L83_EarthShadowOccluderStage` | translation | `x` | 8.8 | 7.78 | -3.75 |
| 4 | `ANIM_ORBIT_TIME_MARKER` | `L83_OrbitalDayNightHologram` | rotation | `y` | 0 | 7.62 | -4.45 |
| 5 | `ANIM_SOLAR_SIMULATOR_YAW` | `L83_ProgrammableSolarIlluminationArc` | rotation | `y` | -10.5 | 7.88 | -3.4 |
| 6 | `ANIM_SOLAR_SIMULATOR_TILT` | `L83_ProgrammableSolarIlluminationArc` | rotation | `x` | -10.5 | 7.88 | -3.4 |
| 7 | `ANIM_MISSION_BUS_ROTATION` | `L83_SpacecraftServiceBusStructure` | rotation | `y` | 0 | 4.35 | -0.9 |
| 8 | `ANIM_SHADOW_BOUNDARY` | `L83_EarthShadowOccluderStage` | translation | `x` | 8.8 | 7.78 | -3.75 |
| 9 | `ANIM_PORT_WING_DEPLOY` | `L83_DeployableSolarWingPort` | rotation | `z` | -6.43 | 5.17 | -0.85 |
| 10 | `ANIM_STARBOARD_WING_DEPLOY` | `L83_DeployableSolarWingStarboard` | rotation | `z` | 6.43 | 5.17 | -0.85 |
| 11 | `ANIM_PORT_OUTER_PANEL` | `L83_DeployableSolarWingPort` | rotation | `y` | -6.43 | 5.17 | -0.85 |
| 12 | `ANIM_STARBOARD_OUTER_PANEL` | `L83_DeployableSolarWingStarboard` | rotation | `y` | 6.43 | 5.17 | -0.85 |
| 13 | `ANIM_PORT_GIMBAL_AZIMUTH` | `L83_SolarArrayPortTrackingGimbal` | rotation | `y` | -3.1 | 4.95 | -0.85 |
| 14 | `ANIM_PORT_GIMBAL_ELEVATION` | `L83_SolarArrayPortTrackingGimbal` | rotation | `x` | -3.1 | 4.95 | -0.85 |
| 15 | `ANIM_STARBOARD_GIMBAL_AZIMUTH` | `L83_SolarArrayStarboardTrackingGimbal` | rotation | `y` | 3.1 | 4.95 | -0.85 |
| 16 | `ANIM_STARBOARD_GIMBAL_ELEVATION` | `L83_SolarArrayStarboardTrackingGimbal` | rotation | `x` | 3.1 | 4.95 | -0.85 |
| 17 | `ANIM_PORT_STRING_SCAN` | `L83_SolarArrayCellStringMap` | indicator | `none` | -6.15 | 5.6 | -0.62 |
| 18 | `ANIM_STARBOARD_STRING_SCAN` | `L83_SolarArrayCellStringMap` | indicator | `none` | -6.15 | 5.6 | -0.62 |
| 19 | `ANIM_BYPASS_DIODE_DISPLAY` | `L83_PVStringBypassDiodeBank` | indicator | `none` | -8.6 | 1.72 | 3.9 |
| 20 | `ANIM_SOLAR_COMBINER_SHUTTER` | `L83_SolarArrayCombinerBox` | translation | `y` | -7.1 | 2.08 | 3.65 |
| 21 | `ANIM_MPPT_PORT_CONTROL` | `L83_MPPTControllerPort` | rotation | `z` | -10.8 | 2.15 | 5.08 |
| 22 | `ANIM_MPPT_STARBOARD_CONTROL` | `L83_MPPTControllerStarboard` | rotation | `z` | -9.45 | 2.15 | 5.08 |
| 23 | `ANIM_MPPT_SWEEP_PROBE` | `L83_MPPTPerturbObserveDisplay` | translation | `x` | -7.95 | 3.48 | 3.72 |
| 24 | `ANIM_PV_CURVE_HOLOGRAM` | `L83_PVMaximumPowerOperatingPointHologram` | indicator | `none` | -7.0 | 7.76 | -4.35 |
| 25 | `ANIM_CONVERTER_A_COOLING` | `L83_PowerConditioningConverterA` | rotation | `z` | -8.06 | 2.04 | 5.05 |
| 26 | `ANIM_CONVERTER_B_COOLING` | `L83_PowerConditioningConverterB` | rotation | `z` | -6.8 | 2.04 | 5.05 |
| 27 | `ANIM_MAIN_BUS_PRECHARGE` | `L83_RegulatedMainDCBus` | translation | `x` | 0 | 1.58 | 1.25 |
| 28 | `ANIM_BUS_CAPACITOR_DISPLAY` | `L83_BusBulkCapacitorModule` | indicator | `none` | -3.15 | 1.65 | 5.06 |
| 29 | `ANIM_BUS_TRANSIENT_RECORDER` | `L83_HighSpeedPowerTransientRecorder` | indicator | `none` | -1.95 | 1.65 | 5.15 |
| 30 | `ANIM_ENERGY_FLOW_HOLOGRAM` | `L83_ThreeDimensionalEnergyFlowHologram` | indicator | `none` | 0 | 6.8 | -4.85 |
| 31 | `ANIM_BATTERY_VAULT_DOOR` | `L83_ProtectedBatteryCellTray` | rotation | `y` | 8.92 | 1.35 | 2.42 |
| 32 | `ANIM_BATTERY_PACK_A_TRAY` | `L83_BatteryPackA` | translation | `z` | 7.78 | 2.4 | 1.3 |
| 33 | `ANIM_BATTERY_PACK_B_TRAY` | `L83_BatteryPackB` | translation | `z` | 10.1 | 2.4 | 1.3 |
| 34 | `ANIM_CELL_BALANCE_SCAN` | `L83_BatteryBalancingCircuit` | translation | `x` | 9.0 | 1.58 | 3.86 |
| 35 | `ANIM_CELL_VOLTAGE_SCAN` | `L83_BatteryCellVoltageTapArray` | translation | `x` | 7.54 | 3.55 | 2.1 |
| 36 | `ANIM_BATTERY_CURRENT_PROBE` | `L83_BatteryCurrentHallSensor` | translation | `y` | 9.0 | 3.55 | 2.1 |
| 37 | `ANIM_BATTERY_CONTACTOR` | `L83_BatteryIsolationContactor` | translation | `y` | 9.02 | 1.55 | 5.17 |
| 38 | `ANIM_BATTERY_PRECHARGE_SLIDE` | `L83_BatteryPrechargeResistorModule` | translation | `x` | 10.45 | 1.55 | 5.17 |
| 39 | `ANIM_THERMAL_PLATE_VALVE` | `L83_BatteryThermalControlPlate` | rotation | `y` | 8.55 | 1.08 | 4.1 |
| 40 | `ANIM_RADIATOR_FIN_POSITION` | `L83_BatteryRadiatorFinAssembly` | rotation | `y` | 10.7 | 1.07 | 4.08 |
| 41 | `ANIM_SUPERCAP_RIDE_THROUGH` | `L83_SupercapacitorRideThroughModule` | indicator | `none` | 7.2 | 2.45 | 4.22 |
| 42 | `ANIM_BATTERY_HEALTH_HOLOGRAM` | `L83_BatteryManagementController` | indicator | `none` | 7.54 | 1.58 | 3.86 |
| 43 | `ANIM_LOAD_PRIORITY_SWITCHBANK` | `L83_PriorityLoadSwitchingRack` | translation | `x` | 2.3 | 2.0 | 3.72 |
| 44 | `ANIM_ATTITUDE_LOAD` | `L83_AttitudeControlLoadEmulator` | indicator | `none` | -1.9 | 2.3 | 3.93 |
| 45 | `ANIM_COMPUTER_LOAD` | `L83_MissionComputerLoadEmulator` | indicator | `none` | -0.7 | 2.3 | 3.93 |
| 46 | `ANIM_OPTICAL_TELEMETRY_LOAD` | `L83_OpticalTelemetryLoadEmulator` | indicator | `none` | 0.55 | 2.3 | 3.93 |
| 47 | `ANIM_SCIENCE_LOAD` | `L83_SciencePayloadLoadEmulator` | indicator | `none` | 1.8 | 2.3 | 3.93 |
| 48 | `ANIM_HEATER_LOAD` | `L83_HeaterThermalLoadEmulator` | indicator | `none` | 3.12 | 2.3 | 3.93 |
| 49 | `ANIM_MISSION_SCHEDULER` | `L83_MissionTaskPowerScheduler` | indicator | `none` | -3.9 | 1.65 | 5.65 |
| 50 | `ANIM_BUDGET_FORECAST` | `L83_LoadForecastEnergyBudgetAnalyzer` | indicator | `none` | -2.4 | 1.65 | 5.65 |
| 51 | `ANIM_GIMBAL_JAM_TEST` | `L83_SolarGimbalJamSimulator` | translation | `x` | -3.5 | 5.6 | -0.85 |
| 52 | `ANIM_CELL_IMBALANCE_TEST` | `L83_BatteryCellImbalanceSimulator` | translation | `x` | 8.85 | 4.1 | 1.45 |
| 53 | `ANIM_BURST_LOAD_TEST` | `L83_OpticalBurstDemandInjector` | translation | `x` | 3.25 | 1.38 | 5.72 |
| 54 | `ANIM_POWER_SAFE_MODE_SWITCH` | `L83_IndependentUndervoltageProtector` | rotation | `z` | 5.1 | 1.66 | 5.35 |
| 55 | `ANIM_EPS_SCADA` | `L83_OrbitalPowerSCADA` | indicator | `none` | 0 | 7.45 | -5.06 |
| 56 | `ANIM_FINAL_CERTIFICATION` | `L83_MissionSurvivalCertificationBeacon` | indicator | `none` | 0 | 8.8 | -5.1 |

## ANNEX C — ALL 138 PLANNED PLAYER-WIRE SOURCE/DESTINATION PAIRS

This is the **conceptual CSV**, not already built Unity SparkTerminal wiring. The visual anchors must be reconciled into electrically safe logical ports.

| Wire | Source (art anchor) | Target (art anchor) | Signal kind | Signal purpose |
|---|---|---|---|---|
| **W001** | `L83_Protected5VControlSupply` (`P198`) | `L83_EPSPowerSafetyPLC` (`P192`) | `POWER_CONTROL` | 5V EPS safety PLC power |
| **W002** | `L83_Protected5VControlSupply` (`P199`) | `L83_MainBusEnergyManagementController` (`P195`) | `POWER_CONTROL` | 5V energy management supervisor supply |
| **W003** | `L83_Protected5VControlSupply` (`P200`) | `L83_MPPTControllerPort` (`P052`) | `POWER_CONTROL` | 5V port MPPT controller logic |
| **W004** | `L83_Protected5VControlSupply` (`P198`) | `L83_MPPTControllerStarboard` (`P056`) | `POWER_CONTROL` | 5V starboard MPPT controller logic |
| **W005** | `L83_Protected5VControlSupply` (`P199`) | `L83_BatteryManagementController` (`P112`) | `POWER_CONTROL` | 5V battery management logic |
| **W006** | `L83_Protected5VControlSupply` (`P200`) | `L83_PowerDistributionUnit` (`P141`) | `POWER_CONTROL` | 5V distribution controller logic |
| **W007** | `L83_Protected5VControlSupply` (`P198`) | `L83_PriorityLoadSwitchingRack` (`P144`) | `POWER_CONTROL` | 5V priority load controller logic |
| **W008** | `L83_Protected5VControlSupply` (`P199`) | `L83_OrbitalPowerSCADA` (`P201`) | `POWER_CONTROL` | 5V SCADA logic supply |
| **W009** | `L83_Protected5VControlSupply` (`P200`) | `L83_IndependentUndervoltageProtector` (`P189`) | `POWER_CONTROL` | 5V independent undervoltage protector logic |
| **W010** | `L83_Protected5VControlSupply` (`P198`) | `L83_BusVoltageTransducer` (`P075`) | `POWER_CONTROL` | 5V bus transducer conditioning feed |
| **W011** | `L83_MainBusEnergyManagementController` (`P196`) | `L83_SolarWingPortHingeAssembly` (`P007`) | `DIGITAL_CONTROL` | Port wing deploy command |
| **W012** | `L83_SolarWingPortHingeAssembly` (`P008`) | `L83_MainBusEnergyManagementController` (`P197`) | `ANALOG_FEEDBACK` | Port wing deployed limit feedback |
| **W013** | `L83_MainBusEnergyManagementController` (`P195`) | `L83_SolarWingStarboardHingeAssembly` (`P010`) | `DIGITAL_CONTROL` | Starboard wing deploy command |
| **W014** | `L83_SolarWingStarboardHingeAssembly` (`P011`) | `L83_MainBusEnergyManagementController` (`P196`) | `ANALOG_FEEDBACK` | Starboard wing deployed limit feedback |
| **W015** | `L83_MainBusEnergyManagementController` (`P197`) | `L83_SolarArrayPortTrackingGimbal` (`P013`) | `DIGITAL_CONTROL` | Port azimuth tracking request |
| **W016** | `L83_SolarArrayPortTrackingGimbal` (`P014`) | `L83_MainBusEnergyManagementController` (`P195`) | `ANALOG_FEEDBACK` | Port array actual azimuth |
| **W017** | `L83_MainBusEnergyManagementController` (`P196`) | `L83_SolarArrayStarboardTrackingGimbal` (`P016`) | `DIGITAL_CONTROL` | Starboard azimuth tracking request |
| **W018** | `L83_SolarArrayStarboardTrackingGimbal` (`P017`) | `L83_MainBusEnergyManagementController` (`P197`) | `ANALOG_FEEDBACK` | Starboard array actual azimuth |
| **W019** | `L83_SolarArrayAzimuthEncoder` (`P019`) | `L83_MainBusEnergyManagementController` (`P195`) | `ANALOG_FEEDBACK` | Solar tracking encoder measured position |
| **W020** | `L83_MainBusEnergyManagementController` (`P196`) | `L83_SolarArrayAzimuthEncoder` (`P020`) | `DIGITAL_CONTROL` | Tracking encoder calibration enable |
| **W021** | `L83_ProgrammableSolarIlluminationArc` (`P022`) | `L83_MainBusEnergyManagementController` (`P197`) | `ANALOG_FEEDBACK` | Modeled Sun vector and irradiance diagnostic |
| **W022** | `L83_EarthShadowOccluderStage` (`P025`) | `L83_MainBusEnergyManagementController` (`P195`) | `ANALOG_FEEDBACK` | Orbital eclipse shadow boundary status |
| **W023** | `L83_OrbitalDayNightHologram` (`P028`) | `L83_MainBusEnergyManagementController` (`P196`) | `ANALOG_FEEDBACK` | Simulated orbital phase status |
| **W024** | `L83_MainBusEnergyManagementController` (`P197`) | `L83_SolarGimbalJamSimulator` (`P180`) | `DIGITAL_CONTROL` | Solar array jam fault arm |
| **W025** | `L83_SolarGimbalJamSimulator` (`P181`) | `L83_MainBusEnergyManagementController` (`P195`) | `ANALOG_FEEDBACK` | Port gimbal jam diagnostic |
| **W026** | `L83_MainBusEnergyManagementController` (`P196`) | `L83_SolarArrayCellStringMap` (`P031`) | `DIGITAL_CONTROL` | Solar string inspection request |
| **W027** | `L83_SolarArrayCellStringMap` (`P032`) | `L83_MainBusEnergyManagementController` (`P197`) | `ANALOG_FEEDBACK` | Solar cell string distribution feedback |
| **W028** | `L83_MainBusEnergyManagementController` (`P195`) | `L83_SolarArrayCombinerBox` (`P043`) | `DIGITAL_CONTROL` | Solar combiner isolation command |
| **W029** | `L83_SolarArrayCombinerBox` (`P044`) | `L83_MainBusEnergyManagementController` (`P196`) | `ANALOG_FEEDBACK` | Solar combiner ready feedback |
| **W030** | `L83_SolarArrayPortTrackingGimbal` (`P015`) | `L83_EPSPowerSafetyPLC` (`P193`) | `ANALOG_FEEDBACK` | Port gimbal motion safety status |
| **W031** | `L83_SolarArrayCurrentSensors` (`P046`) | `L83_MPPTControllerPort` (`P053`) | `ANALOG_FEEDBACK` | Port solar measured array current |
| **W032** | `L83_SolarArrayVoltageSensors` (`P049`) | `L83_MPPTControllerPort` (`P054`) | `ANALOG_FEEDBACK` | Port solar measured array voltage |
| **W033** | `L83_SolarArrayCurrentSensors` (`P047`) | `L83_MPPTControllerStarboard` (`P057`) | `ANALOG_FEEDBACK` | Starboard solar measured current |
| **W034** | `L83_SolarArrayVoltageSensors` (`P050`) | `L83_MPPTControllerStarboard` (`P058`) | `ANALOG_FEEDBACK` | Starboard solar measured voltage |
| **W035** | `L83_MainBusEnergyManagementController` (`P197`) | `L83_MPPTControllerPort` (`P055`) | `DIGITAL_CONTROL` | Port MPPT enable request |
| **W036** | `L83_MainBusEnergyManagementController` (`P195`) | `L83_MPPTControllerStarboard` (`P059`) | `DIGITAL_CONTROL` | Starboard MPPT enable request |
| **W037** | `L83_MPPTControllerPort` (`P052`) | `L83_PowerConditioningConverterA` (`P063`) | `DIGITAL_CONTROL` | Port MPPT converter duty request |
| **W038** | `L83_MPPTControllerStarboard` (`P056`) | `L83_PowerConditioningConverterB` (`P067`) | `DIGITAL_CONTROL` | Starboard MPPT converter duty request |
| **W039** | `L83_PowerConditioningConverterA` (`P064`) | `L83_MainBusEnergyManagementController` (`P196`) | `ANALOG_FEEDBACK` | Port converter measured DC output |
| **W040** | `L83_PowerConditioningConverterB` (`P068`) | `L83_MainBusEnergyManagementController` (`P197`) | `ANALOG_FEEDBACK` | Starboard converter measured DC output |
| **W041** | `L83_MPPTControllerPort` (`P053`) | `L83_MainBusEnergyManagementController` (`P195`) | `ANALOG_FEEDBACK` | Port measured MPPT tracking quality |
| **W042** | `L83_MPPTControllerStarboard` (`P057`) | `L83_MainBusEnergyManagementController` (`P196`) | `ANALOG_FEEDBACK` | Starboard measured MPPT tracking quality |
| **W043** | `L83_PVStringPortIVTracer` (`P034`) | `L83_MPPTControllerPort` (`P054`) | `ANALOG_FEEDBACK` | Port IV curve sample data |
| **W044** | `L83_PVStringStarboardIVTracer` (`P037`) | `L83_MPPTControllerStarboard` (`P058`) | `ANALOG_FEEDBACK` | Starboard IV curve sample data |
| **W045** | `L83_PVStringBypassDiodeBank` (`P040`) | `L83_MPPTControllerPort` (`P055`) | `ANALOG_FEEDBACK` | Bypass diode array health |
| **W046** | `L83_SolarArrayCombinerBox` (`P045`) | `L83_PowerConditioningConverterA` (`P065`) | `ANALOG_FEEDBACK` | Solar combined port output monitoring |
| **W047** | `L83_SolarArrayCombinerBox` (`P043`) | `L83_PowerConditioningConverterB` (`P069`) | `ANALOG_FEEDBACK` | Solar combined starboard output monitoring |
| **W048** | `L83_MainBusEnergyManagementController` (`P197`) | `L83_PowerConditioningConverterA` (`P066`) | `DIGITAL_CONTROL` | Port converter protected enable |
| **W049** | `L83_MainBusEnergyManagementController` (`P195`) | `L83_PowerConditioningConverterB` (`P070`) | `DIGITAL_CONTROL` | Starboard converter protected enable |
| **W050** | `L83_PowerConditioningConverterA` (`P063`) | `L83_RegulatedMainDCBus` (`P071`) | `DIGITAL_CONTROL` | Port converter DC bus power interface |
| **W051** | `L83_PowerConditioningConverterB` (`P067`) | `L83_RegulatedMainDCBus` (`P072`) | `DIGITAL_CONTROL` | Starboard converter DC bus power interface |
| **W052** | `L83_MPPTPerturbObserveDisplay` (`P060`) | `L83_OrbitalPowerSCADA` (`P202`) | `ANALOG_FEEDBACK` | MPPT perturb observe diagnostic telemetry |
| **W053** | `L83_BatteryPackA` (`P089`) | `L83_BatteryManagementController` (`P113`) | `DIGITAL_CONTROL` | Battery A module presence and pack state |
| **W054** | `L83_BatteryPackB` (`P093`) | `L83_BatteryManagementController` (`P114`) | `DIGITAL_CONTROL` | Battery B module presence and pack state |
| **W055** | `L83_BatteryCellVoltageTapArray` (`P100`) | `L83_BatteryManagementController` (`P115`) | `ANALOG_FEEDBACK` | Battery individual cell-group voltages |
| **W056** | `L83_BatteryCurrentHallSensor` (`P104`) | `L83_BatteryManagementController` (`P112`) | `ANALOG_FEEDBACK` | Battery measured pack current |
| **W057** | `L83_BatteryThermalSensorGrid` (`P108`) | `L83_BatteryManagementController` (`P113`) | `ANALOG_FEEDBACK` | Battery pack thermistor array feedback |
| **W058** | `L83_BatteryManagementController` (`P114`) | `L83_BatteryBalancingCircuit` (`P116`) | `DIGITAL_CONTROL` | BMS cell balancing command |
| **W059** | `L83_BatteryBalancingCircuit` (`P117`) | `L83_BatteryManagementController` (`P115`) | `ANALOG_FEEDBACK` | Cell balancing progress and diagnostic |
| **W060** | `L83_BatteryManagementController` (`P112`) | `L83_BatteryChargeController` (`P120`) | `DIGITAL_CONTROL` | BMS safe charge authorization |
| **W061** | `L83_BatteryManagementController` (`P113`) | `L83_BatteryDischargeController` (`P123`) | `DIGITAL_CONTROL` | BMS safe discharge authorization |
| **W062** | `L83_BatteryManagementController` (`P114`) | `L83_BatteryIsolationContactor` (`P126`) | `DIGITAL_CONTROL` | BMS isolation contactor drive |
| **W063** | `L83_BatteryIsolationContactor` (`P127`) | `L83_BatteryManagementController` (`P115`) | `ANALOG_FEEDBACK` | Actual isolation contactor feedback |
| **W064** | `L83_BatteryManagementController` (`P112`) | `L83_BatteryPrechargeResistorModule` (`P129`) | `DIGITAL_CONTROL` | Safe precharge resistor switching request |
| **W065** | `L83_BatteryPrechargeResistorModule` (`P130`) | `L83_BatteryManagementController` (`P113`) | `ANALOG_FEEDBACK` | Precharge measured completion |
| **W066** | `L83_BatteryChargeController` (`P121`) | `L83_BatteryPackA` (`P090`) | `DIGITAL_CONTROL` | Pack A charge controller output |
| **W067** | `L83_BatteryChargeController` (`P122`) | `L83_BatteryPackB` (`P094`) | `DIGITAL_CONTROL` | Pack B charge controller output |
| **W068** | `L83_BatteryPackA` (`P091`) | `L83_BatteryDischargeController` (`P124`) | `DIGITAL_CONTROL` | Battery A available discharge capacity |
| **W069** | `L83_BatteryPackB` (`P095`) | `L83_BatteryDischargeController` (`P125`) | `DIGITAL_CONTROL` | Battery B available discharge capacity |
| **W070** | `L83_BatteryThermalSensorGrid` (`P109`) | `L83_BatteryThermalControlPlate` (`P135`) | `ANALOG_FEEDBACK` | Thermal battery cooling plate control feedback |
| **W071** | `L83_BatteryThermalControlPlate` (`P136`) | `L83_BatteryManagementController` (`P114`) | `DIGITAL_CONTROL` | Battery thermal plate state |
| **W072** | `L83_BatteryManagementController` (`P115`) | `L83_BatteryRadiatorFinAssembly` (`P138`) | `ANALOG_FEEDBACK` | Battery radiator position request |
| **W073** | `L83_BatteryRadiatorFinAssembly` (`P139`) | `L83_BatteryManagementController` (`P112`) | `ANALOG_FEEDBACK` | Battery radiator actual state |
| **W074** | `L83_BatteryCellImbalanceSimulator` (`P183`) | `L83_BatteryManagementController` (`P113`) | `DIGITAL_CONTROL` | Controlled cell imbalance fault state |
| **W075** | `L83_RegulatedMainDCBus` (`P073`) | `L83_BusVoltageTransducer` (`P076`) | `ANALOG_FEEDBACK` | Main bus physical potential measurement sense |
| **W076** | `L83_RegulatedMainDCBus` (`P074`) | `L83_BusCurrentShuntMonitor` (`P079`) | `ANALOG_FEEDBACK` | Main bus actual shunt current measurement |
| **W077** | `L83_RegulatedMainDCBus` (`P071`) | `L83_BusBulkCapacitorModule` (`P083`) | `DIGITAL_CONTROL` | Bus bulk capacitor energy interface |
| **W078** | `L83_BusBulkCapacitorModule` (`P084`) | `L83_MainBusEnergyManagementController` (`P196`) | `DIGITAL_CONTROL` | Bus transient ride through measurement |
| **W079** | `L83_RegulatedMainDCBus` (`P072`) | `L83_PowerDistributionUnit` (`P142`) | `DIGITAL_CONTROL` | Regulated DC bus distribution input |
| **W080** | `L83_PowerDistributionUnit` (`P143`) | `L83_PriorityLoadSwitchingRack` (`P145`) | `DIGITAL_CONTROL` | Power distribution priority switching command feed |
| **W081** | `L83_PriorityLoadSwitchingRack` (`P146`) | `L83_SafetyEssentialLoadBus` (`P147`) | `DIGITAL_CONTROL` | P0 essential bus supply permit |
| **W082** | `L83_PriorityLoadSwitchingRack` (`P144`) | `L83_AttitudeControlLoadEmulator` (`P150`) | `DIGITAL_CONTROL` | P1 attitude control load switch |
| **W083** | `L83_PriorityLoadSwitchingRack` (`P145`) | `L83_MissionComputerLoadEmulator` (`P153`) | `DIGITAL_CONTROL` | P0 mission computer load switch |
| **W084** | `L83_PriorityLoadSwitchingRack` (`P146`) | `L83_OpticalTelemetryLoadEmulator` (`P156`) | `ANALOG_FEEDBACK` | P2 optical telemetry load switch |
| **W085** | `L83_PriorityLoadSwitchingRack` (`P144`) | `L83_SciencePayloadLoadEmulator` (`P159`) | `DIGITAL_CONTROL` | P3 science payload load switch |
| **W086** | `L83_PriorityLoadSwitchingRack` (`P145`) | `L83_HeaterThermalLoadEmulator` (`P162`) | `DIGITAL_CONTROL` | P1 thermal heater load switch |
| **W087** | `L83_MainBusEnergyManagementController` (`P197`) | `L83_PowerDistributionUnit` (`P141`) | `DIGITAL_CONTROL` | Distribution power limit request |
| **W088** | `L83_PowerDistributionUnit` (`P142`) | `L83_MainBusEnergyManagementController` (`P195`) | `ANALOG_FEEDBACK` | Actual distribution branch current status |
| **W089** | `L83_SupercapacitorRideThroughModule` (`P132`) | `L83_RegulatedMainDCBus` (`P073`) | `ANALOG_FEEDBACK` | Supercapacitor discharge support status |
| **W090** | `L83_MainBusEnergyManagementController` (`P196`) | `L83_SupercapacitorRideThroughModule` (`P133`) | `DIGITAL_CONTROL` | Bounded supercapacitor ride-through command |
| **W091** | `L83_IndependentUndervoltageProtector` (`P190`) | `L83_PowerDistributionUnit` (`P143`) | `ANALOG_FEEDBACK` | Independent bus undervoltage distribution inhibit |
| **W092** | `L83_RegulatedMainDCBus` (`P074`) | `L83_IndependentUndervoltageProtector` (`P191`) | `ANALOG_FEEDBACK` | Independent main bus voltage protector sense |
| **W093** | `L83_HighSpeedPowerTransientRecorder` (`P086`) | `L83_MainBusEnergyManagementController` (`P197`) | `ANALOG_FEEDBACK` | Bus transient recorder measured ripple diagnostic |
| **W094** | `L83_BusVoltageTransducer` (`P077`) | `L83_HighSpeedPowerTransientRecorder` (`P087`) | `ANALOG_FEEDBACK` | Bus voltage acquisition to transient recorder |
| **W095** | `L83_BusCurrentShuntMonitor` (`P080`) | `L83_HighSpeedPowerTransientRecorder` (`P088`) | `ANALOG_FEEDBACK` | Shunt current acquisition to transient recorder |
| **W096** | `L83_BusVoltageTransducer` (`P078`) | `L83_OrbitalPowerSCADA` (`P203`) | `ANALOG_FEEDBACK` | Display only main bus voltage telemetry |
| **W097** | `L83_BusCurrentShuntMonitor` (`P081`) | `L83_OrbitalPowerSCADA` (`P201`) | `ANALOG_FEEDBACK` | Main bus current trend telemetry |
| **W098** | `L83_BatteryManagementController` (`P114`) | `L83_OrbitalPowerSCADA` (`P202`) | `ANALOG_FEEDBACK` | Battery cell health and SOC diagnostic telemetry |
| **W099** | `L83_SolarArrayCurrentSensors` (`P048`) | `L83_OrbitalPowerSCADA` (`P203`) | `ANALOG_FEEDBACK` | Solar port current telemetry |
| **W100** | `L83_SolarArrayVoltageSensors` (`P051`) | `L83_OrbitalPowerSCADA` (`P201`) | `ANALOG_FEEDBACK` | Solar port voltage telemetry |
| **W101** | `L83_LoadForecastEnergyBudgetAnalyzer` (`P168`) | `L83_MainBusEnergyManagementController` (`P195`) | `DIGITAL_CONTROL` | Mission energy reserve forecast result |
| **W102** | `L83_EclipseEntryPredictionConsole` (`P171`) | `L83_MainBusEnergyManagementController` (`P196`) | `DIGITAL_CONTROL` | Predicted eclipse entry and duration |
| **W103** | `L83_SunlightReturnChargePlanner` (`P174`) | `L83_MainBusEnergyManagementController` (`P197`) | `DIGITAL_CONTROL` | Sunlight return battery recharge plan |
| **W104** 🔴 | `L83_BusVoltageTransducer` (`P075`) | `L83_MainBusEnergyManagementController` (`P195`) | `ANALOG_FEEDBACK` | CRITICAL trusted DC bus voltage control feedback |
| **W105** | `L83_BusVoltageTransducer` (`P076`) | `L83_MainBusEnergyManagementController` (`P196`) | `ANALOG_FEEDBACK` | Bus voltage transducer calibration health (diagnostic only) |
| **W106** | `L83_IndependentUndervoltageProtector` (`P189`) | `L83_MainBusEnergyManagementController` (`P197`) | `ANALOG_FEEDBACK` | Independent under voltage protection status |
| **W107** | `L83_MainBusEnergyManagementController` (`P195`) | `L83_HighSpeedPowerTransientRecorder` (`P086`) | `DIGITAL_CONTROL` | Bus transient capture trigger |
| **W108** | `L83_MainBusEnergyManagementController` (`P196`) | `L83_LoadForecastEnergyBudgetAnalyzer` (`P169`) | `DIGITAL_CONTROL` | Energy budget computation request |
| **W109** | `L83_ContingencyPowerFaultInjector` (`P177`) | `L83_MainBusEnergyManagementController` (`P197`) | `ANALOG_FEEDBACK` | Contingency power fault diagnostic status |
| **W110** | `L83_HighSpeedPowerTransientRecorder` (`P087`) | `L83_OrbitalPowerSCADA` (`P202`) | `DIGITAL_CONTROL` | Transient waveform SCADA display only |
| **W111** | `L83_MissionTaskPowerScheduler` (`P165`) | `L83_MainBusEnergyManagementController` (`P195`) | `DIGITAL_CONTROL` | Scheduled mission task demand |
| **W112** | `L83_MainBusEnergyManagementController` (`P196`) | `L83_MissionTaskPowerScheduler` (`P166`) | `DIGITAL_CONTROL` | Approved mission task energy budget |
| **W113** | `L83_LoadForecastEnergyBudgetAnalyzer` (`P170`) | `L83_MissionTaskPowerScheduler` (`P167`) | `DIGITAL_CONTROL` | Eclipse energy reserve prediction |
| **W114** | `L83_EclipseEntryPredictionConsole` (`P172`) | `L83_MissionTaskPowerScheduler` (`P165`) | `DIGITAL_CONTROL` | Eclipse phase transition forecast |
| **W115** | `L83_SunlightReturnChargePlanner` (`P175`) | `L83_MissionTaskPowerScheduler` (`P166`) | `DIGITAL_CONTROL` | Sunlight return charge opportunity |
| **W116** | `L83_MainBusEnergyManagementController` (`P197`) | `L83_PriorityLoadSwitchingRack` (`P146`) | `DIGITAL_CONTROL` | Mission priority load schedule |
| **W117** | `L83_AttitudeControlLoadEmulator` (`P151`) | `L83_MissionTaskPowerScheduler` (`P167`) | `DIGITAL_CONTROL` | Attitude control required energy demand |
| **W118** | `L83_MissionComputerLoadEmulator` (`P154`) | `L83_MissionTaskPowerScheduler` (`P165`) | `DIGITAL_CONTROL` | Mission computer required electrical demand |
| **W119** | `L83_OpticalTelemetryLoadEmulator` (`P157`) | `L83_MissionTaskPowerScheduler` (`P166`) | `ANALOG_FEEDBACK` | Optical telemetry downlink power demand |
| **W120** | `L83_SciencePayloadLoadEmulator` (`P160`) | `L83_MissionTaskPowerScheduler` (`P167`) | `DIGITAL_CONTROL` | Science payload optional power demand |
| **W121** | `L83_HeaterThermalLoadEmulator` (`P163`) | `L83_MissionTaskPowerScheduler` (`P165`) | `DIGITAL_CONTROL` | Thermal heater required demand |
| **W122** | `L83_MissionTaskPowerScheduler` (`P166`) | `L83_PriorityLoadSwitchingRack` (`P144`) | `DIGITAL_CONTROL` | Priority shedding request |
| **W123** | `L83_OpticalBurstDemandInjector` (`P186`) | `L83_OpticalTelemetryLoadEmulator` (`P158`) | `DIGITAL_CONTROL` | Optical burst surge fault injection state |
| **W124** | `L83_OpticalTelemetryLoadEmulator` (`P156`) | `L83_MainBusEnergyManagementController` (`P195`) | `ANALOG_FEEDBACK` | Actual optical transient demand feedback |
| **W125** | `L83_MainBusEnergyManagementController` (`P196`) | `L83_OpticalBurstDemandInjector` (`P187`) | `DIGITAL_CONTROL` | Optical burst disturbance activation |
| **W126** | `L83_MissionTaskPowerScheduler` (`P167`) | `L83_OrbitalPowerSCADA` (`P203`) | `ANALOG_FEEDBACK` | Mission load schedule visualization telemetry |
| **W127** | `L83_EPSPowerSafetyPLC` (`P194`) | `L83_MainBusEnergyManagementController` (`P197`) | `DIGITAL_CONTROL` | EPS safety authorization and interlock permit |
| **W128** | `L83_EPSPowerSafetyPLC` (`P192`) | `L83_PowerDistributionUnit` (`P141`) | `DIGITAL_CONTROL` | Independent priority load safe-state command |
| **W129** | `L83_IndependentUndervoltageProtector` (`P190`) | `L83_EPSPowerSafetyPLC` (`P193`) | `ANALOG_FEEDBACK` | Hardwired independent undervoltage trip status |
| **W130** | `L83_EmergencyPowerIsolationStation` (`P207`) | `L83_EPSPowerSafetyPLC` (`P194`) | `ANALOG_FEEDBACK` | Emergency power isolation actuator status |
| **W131** | `L83_EPSPowerSafetyPLC` (`P192`) | `L83_BatteryIsolationContactor` (`P128`) | `DIGITAL_CONTROL` | Battery isolation interlock override |
| **W132** | `L83_MainBusEnergyManagementController` (`P195`) | `L83_EPSPowerSafetyPLC` (`P193`) | `ANALOG_FEEDBACK` | Supervisor power system fault/status |
| **W133** | `L83_MainBusEnergyManagementController` (`P196`) | `L83_OrbitalPowerSCADA` (`P201`) | `ANALOG_FEEDBACK` | Authoritative mission energy objective telemetry |
| **W134** | `L83_OrbitalPowerSCADA` (`P202`) | `L83_MainBusEnergyManagementController` (`P197`) | `DIGITAL_CONTROL` | Operator requested mission load schedule |
| **W135** | `L83_EPSPowerSafetyPLC` (`P194`) | `L83_OrbitalPowerSCADA` (`P203`) | `DIGITAL_CONTROL` | EPS safety fault latch and alarm |
| **W136** | `L83_MainBusEnergyManagementController` (`P195`) | `L83_EPSPowerSafetyPLC` (`P192`) | `DIGITAL_CONTROL` | Operator explicit safety reset request |
| **W137** | `L83_BatteryManagementController` (`P115`) | `L83_EPSPowerSafetyPLC` (`P193`) | `ANALOG_FEEDBACK` | Battery thermal and current protection health |
| **W138** | `L83_MainBusEnergyManagementController` (`P196`) | `L83_MissionSurvivalCertificationBeacon` (`P204`) | `DIGITAL_CONTROL` | Final orbital power qualification beacon |

## ANNEX D — ALL 212 DECORATIVE SOCKET ANCHORS AND WORLD POSITIONS

These anchors currently contain visual meshes, **not** Unity `SparkTerminal`s.

| Anchor | Owning model | X (m) | Y (m) | Z (m) |
|---|---|---:|---:|---:|
| `P001` | `L83_DeployableSolarWingPort` | -6.31333 | 5.17 | -0.472 |
| `P002` | `L83_DeployableSolarWingPort` | -6.14333 | 5.17 | -0.472 |
| `P003` | `L83_DeployableSolarWingPort` | -6.31333 | 5.33 | -0.472 |
| `P004` | `L83_DeployableSolarWingStarboard` | 6.14333 | 5.17 | -0.472 |
| `P005` | `L83_DeployableSolarWingStarboard` | 6.31333 | 5.17 | -0.472 |
| `P006` | `L83_DeployableSolarWingStarboard` | 6.14333 | 5.33 | -0.472 |
| `P007` | `L83_SolarWingPortHingeAssembly` | -2.43655 | 4.96 | -0.482 |
| `P008` | `L83_SolarWingPortHingeAssembly` | -2.26655 | 4.96 | -0.482 |
| `P009` | `L83_SolarWingPortHingeAssembly` | -2.43655 | 5.12 | -0.482 |
| `P010` | `L83_SolarWingStarboardHingeAssembly` | 2.29595 | 4.96 | -0.482 |
| `P011` | `L83_SolarWingStarboardHingeAssembly` | 2.46595 | 4.96 | -0.482 |
| `P012` | `L83_SolarWingStarboardHingeAssembly` | 2.29595 | 5.12 | -0.482 |
| `P013` | `L83_SolarArrayPortTrackingGimbal` | -3.185 | 5.01901 | -0.44419 |
| `P014` | `L83_SolarArrayPortTrackingGimbal` | -3.015 | 5.01901 | -0.44419 |
| `P015` | `L83_SolarArrayPortTrackingGimbal` | -3.185 | 5.17901 | -0.44419 |
| `P016` | `L83_SolarArrayStarboardTrackingGimbal` | 3.015 | 5.01901 | -0.44419 |
| `P017` | `L83_SolarArrayStarboardTrackingGimbal` | 3.185 | 5.01901 | -0.44419 |
| `P018` | `L83_SolarArrayStarboardTrackingGimbal` | 3.015 | 5.17901 | -0.44419 |
| `P019` | `L83_SolarArrayAzimuthEncoder` | -2.835 | 5.12 | -0.382 |
| `P020` | `L83_SolarArrayAzimuthEncoder` | -2.665 | 5.12 | -0.382 |
| `P021` | `L83_SolarArrayAzimuthEncoder` | -2.835 | 5.28 | -0.382 |
| `P022` | `L83_ProgrammableSolarIlluminationArc` | -11.11628 | 7.82 | -3.202 |
| `P023` | `L83_ProgrammableSolarIlluminationArc` | -10.94628 | 7.82 | -3.202 |
| `P024` | `L83_ProgrammableSolarIlluminationArc` | -11.11628 | 7.98 | -3.202 |
| `P025` | `L83_EarthShadowOccluderStage` | 8.715 | 7.7 | -3.5245 |
| `P026` | `L83_EarthShadowOccluderStage` | 8.885 | 7.7 | -3.5245 |
| `P027` | `L83_EarthShadowOccluderStage` | 8.715 | 7.86 | -3.5245 |
| `P028` | `L83_OrbitalDayNightHologram` | -0.08502 | 7.54 | -4.252 |
| `P029` | `L83_OrbitalDayNightHologram` | 0.08498 | 7.54 | -4.252 |
| `P030` | `L83_OrbitalDayNightHologram` | -0.08502 | 7.7 | -4.252 |
| `P031` | `L83_SolarArrayCellStringMap` | -6.235 | 5.52 | -0.287 |
| `P032` | `L83_SolarArrayCellStringMap` | -6.065 | 5.52 | -0.287 |
| `P033` | `L83_SolarArrayCellStringMap` | -6.235 | 5.68 | -0.287 |
| `P034` | `L83_PVStringPortIVTracer` | -11.335 | 2.264 | 3.485 |
| `P035` | `L83_PVStringPortIVTracer` | -11.165 | 2.264 | 3.485 |
| `P036` | `L83_PVStringPortIVTracer` | -11.335 | 2.424 | 3.485 |
| `P037` | `L83_PVStringStarboardIVTracer` | -10.005 | 2.264 | 3.485 |
| `P038` | `L83_PVStringStarboardIVTracer` | -9.835 | 2.264 | 3.485 |
| `P039` | `L83_PVStringStarboardIVTracer` | -10.005 | 2.424 | 3.485 |
| `P040` | `L83_PVStringBypassDiodeBank` | -8.685 | 1.6825 | 4.4305 |
| `P041` | `L83_PVStringBypassDiodeBank` | -8.515 | 1.6825 | 4.4305 |
| `P042` | `L83_PVStringBypassDiodeBank` | -8.685 | 1.8425 | 4.4305 |
| `P043` | `L83_SolarArrayCombinerBox` | -7.185 | 2.0625 | 4.238 |
| `P044` | `L83_SolarArrayCombinerBox` | -7.015 | 2.0625 | 4.238 |
| `P045` | `L83_SolarArrayCombinerBox` | -7.185 | 2.2225 | 4.238 |
| `P046` | `L83_SolarArrayCurrentSensors` | -10.585 | 1.4022 | 4.189 |
| `P047` | `L83_SolarArrayCurrentSensors` | -10.415 | 1.4022 | 4.189 |
| `P048` | `L83_SolarArrayCurrentSensors` | -10.585 | 1.5622 | 4.189 |
| `P049` | `L83_SolarArrayVoltageSensors` | -9.255 | 1.4022 | 4.189 |
| `P050` | `L83_SolarArrayVoltageSensors` | -9.085 | 1.4022 | 4.189 |
| `P051` | `L83_SolarArrayVoltageSensors` | -9.255 | 1.5622 | 4.189 |
| `P052` | `L83_MPPTControllerPort` | -10.885 | 2.07 | 5.748 |
| `P053` | `L83_MPPTControllerPort` | -10.715 | 2.07 | 5.748 |
| `P054` | `L83_MPPTControllerPort` | -10.885 | 2.23 | 5.748 |
| `P055` | `L83_MPPTControllerPort` | -10.715 | 2.23 | 5.748 |
| `P056` | `L83_MPPTControllerStarboard` | -9.535 | 2.07 | 5.748 |
| `P057` | `L83_MPPTControllerStarboard` | -9.365 | 2.07 | 5.748 |
| `P058` | `L83_MPPTControllerStarboard` | -9.535 | 2.23 | 5.748 |
| `P059` | `L83_MPPTControllerStarboard` | -9.365 | 2.23 | 5.748 |
| `P060` | `L83_MPPTPerturbObserveDisplay` | -8.035 | 3.2445 | 4.003 |
| `P061` | `L83_MPPTPerturbObserveDisplay` | -7.865 | 3.2445 | 4.003 |
| `P062` | `L83_MPPTPerturbObserveDisplay` | -8.035 | 3.4045 | 4.003 |
| `P063` | `L83_PowerConditioningConverterA` | -8.145 | 1.84815 | 5.7015 |
| `P064` | `L83_PowerConditioningConverterA` | -7.975 | 1.84815 | 5.7015 |
| `P065` | `L83_PowerConditioningConverterA` | -8.145 | 2.00815 | 5.7015 |
| `P066` | `L83_PowerConditioningConverterA` | -7.975 | 2.00815 | 5.7015 |
| `P067` | `L83_PowerConditioningConverterB` | -6.885 | 1.84815 | 5.7015 |
| `P068` | `L83_PowerConditioningConverterB` | -6.715 | 1.84815 | 5.7015 |
| `P069` | `L83_PowerConditioningConverterB` | -6.885 | 2.00815 | 5.7015 |
| `P070` | `L83_PowerConditioningConverterB` | -6.715 | 2.00815 | 5.7015 |
| `P071` | `L83_RegulatedMainDCBus` | -0.085 | 1.3675 | 2.073 |
| `P072` | `L83_RegulatedMainDCBus` | 0.085 | 1.3675 | 2.073 |
| `P073` | `L83_RegulatedMainDCBus` | -0.085 | 1.5275 | 2.073 |
| `P074` | `L83_RegulatedMainDCBus` | 0.085 | 1.5275 | 2.073 |
| `P075` | `L83_BusVoltageTransducer` | -5.365 | 1.4522 | 5.419 |
| `P076` | `L83_BusVoltageTransducer` | -5.195 | 1.4522 | 5.419 |
| `P077` | `L83_BusVoltageTransducer` | -5.365 | 1.6122 | 5.419 |
| `P078` | `L83_BusVoltageTransducer` | -5.195 | 1.6122 | 5.419 |
| `P079` | `L83_BusCurrentShuntMonitor` | -4.285 | 1.4522 | 5.419 |
| `P080` | `L83_BusCurrentShuntMonitor` | -4.115 | 1.4522 | 5.419 |
| `P081` | `L83_BusCurrentShuntMonitor` | -4.285 | 1.6122 | 5.419 |
| `P082` | `L83_BusCurrentShuntMonitor` | -4.115 | 1.6122 | 5.419 |
| `P083` | `L83_BusBulkCapacitorModule` | -3.235 | 1.5701 | 5.56413 |
| `P084` | `L83_BusBulkCapacitorModule` | -3.065 | 1.5701 | 5.56413 |
| `P085` | `L83_BusBulkCapacitorModule` | -3.235 | 1.7301 | 5.56413 |
| `P086` | `L83_HighSpeedPowerTransientRecorder` | -2.035 | 1.558 | 5.763 |
| `P087` | `L83_HighSpeedPowerTransientRecorder` | -1.865 | 1.558 | 5.763 |
| `P088` | `L83_HighSpeedPowerTransientRecorder` | -2.035 | 1.718 | 5.763 |
| `P089` | `L83_BatteryPackA` | 7.695 | 2.345 | 2.5555 |
| `P090` | `L83_BatteryPackA` | 7.865 | 2.345 | 2.5555 |
| `P091` | `L83_BatteryPackA` | 7.695 | 2.505 | 2.5555 |
| `P092` | `L83_BatteryPackA` | 7.865 | 2.505 | 2.5555 |
| `P093` | `L83_BatteryPackB` | 10.015 | 2.345 | 2.5555 |
| `P094` | `L83_BatteryPackB` | 10.185 | 2.345 | 2.5555 |
| `P095` | `L83_BatteryPackB` | 10.015 | 2.505 | 2.5555 |
| `P096` | `L83_BatteryPackB` | 10.185 | 2.505 | 2.5555 |
| `P097` | `L83_ProtectedBatteryCellTray` | 8.835 | 1.27 | 3.1555 |
| `P098` | `L83_ProtectedBatteryCellTray` | 9.005 | 1.27 | 3.1555 |
| `P099` | `L83_ProtectedBatteryCellTray` | 8.835 | 1.43 | 3.1555 |
| `P100` | `L83_BatteryCellVoltageTapArray` | 7.455 | 3.495 | 2.433 |
| `P101` | `L83_BatteryCellVoltageTapArray` | 7.625 | 3.495 | 2.433 |
| `P102` | `L83_BatteryCellVoltageTapArray` | 7.455 | 3.655 | 2.433 |
| `P103` | `L83_BatteryCellVoltageTapArray` | 7.625 | 3.655 | 2.433 |
| `P104` | `L83_BatteryCurrentHallSensor` | 8.915 | 3.47 | 2.50545 |
| `P105` | `L83_BatteryCurrentHallSensor` | 9.085 | 3.47 | 2.50545 |
| `P106` | `L83_BatteryCurrentHallSensor` | 8.915 | 3.63 | 2.50545 |
| `P107` | `L83_BatteryCurrentHallSensor` | 9.085 | 3.63 | 2.50545 |
| `P108` | `L83_BatteryThermalSensorGrid` | 10.37244 | 3.40536 | 2.3018 |
| `P109` | `L83_BatteryThermalSensorGrid` | 10.54244 | 3.40536 | 2.3018 |
| `P110` | `L83_BatteryThermalSensorGrid` | 10.37244 | 3.56536 | 2.3018 |
| `P111` | `L83_BatteryThermalSensorGrid` | 10.54244 | 3.56536 | 2.3018 |
| `P112` | `L83_BatteryManagementController` | 7.455 | 1.4895 | 4.517 |
| `P113` | `L83_BatteryManagementController` | 7.625 | 1.4895 | 4.517 |
| `P114` | `L83_BatteryManagementController` | 7.455 | 1.6495 | 4.517 |
| `P115` | `L83_BatteryManagementController` | 7.625 | 1.6495 | 4.517 |
| `P116` | `L83_BatteryBalancingCircuit` | 8.915 | 1.4895 | 4.517 |
| `P117` | `L83_BatteryBalancingCircuit` | 9.085 | 1.4895 | 4.517 |
| `P118` | `L83_BatteryBalancingCircuit` | 8.915 | 1.6495 | 4.517 |
| `P119` | `L83_BatteryBalancingCircuit` | 9.085 | 1.6495 | 4.517 |
| `P120` | `L83_BatteryChargeController` | 10.365 | 1.4895 | 4.517 |
| `P121` | `L83_BatteryChargeController` | 10.535 | 1.4895 | 4.517 |
| `P122` | `L83_BatteryChargeController` | 10.365 | 1.6495 | 4.517 |
| `P123` | `L83_BatteryDischargeController` | 7.495 | 1.4595 | 5.827 |
| `P124` | `L83_BatteryDischargeController` | 7.665 | 1.4595 | 5.827 |
| `P125` | `L83_BatteryDischargeController` | 7.495 | 1.6195 | 5.827 |
| `P126` | `L83_BatteryIsolationContactor` | 8.935 | 1.57 | 5.703 |
| `P127` | `L83_BatteryIsolationContactor` | 9.105 | 1.57 | 5.703 |
| `P128` | `L83_BatteryIsolationContactor` | 8.935 | 1.73 | 5.703 |
| `P129` | `L83_BatteryPrechargeResistorModule` | 10.365 | 1.4595 | 5.827 |
| `P130` | `L83_BatteryPrechargeResistorModule` | 10.535 | 1.4595 | 5.827 |
| `P131` | `L83_BatteryPrechargeResistorModule` | 10.365 | 1.6195 | 5.827 |
| `P132` | `L83_SupercapacitorRideThroughModule` | 7.115 | 2.55 | 4.63408 |
| `P133` | `L83_SupercapacitorRideThroughModule` | 7.285 | 2.55 | 4.63408 |
| `P134` | `L83_SupercapacitorRideThroughModule` | 7.115 | 2.71 | 4.63408 |
| `P135` | `L83_BatteryThermalControlPlate` | 8.465 | 1.0 | 4.558 |
| `P136` | `L83_BatteryThermalControlPlate` | 8.635 | 1.0 | 4.558 |
| `P137` | `L83_BatteryThermalControlPlate` | 8.465 | 1.16 | 4.558 |
| `P138` | `L83_BatteryRadiatorFinAssembly` | 10.615 | 0.9175 | 4.518 |
| `P139` | `L83_BatteryRadiatorFinAssembly` | 10.785 | 0.9175 | 4.518 |
| `P140` | `L83_BatteryRadiatorFinAssembly` | 10.615 | 1.0775 | 4.518 |
| `P141` | `L83_PowerDistributionUnit` | 0.515 | 1.92 | 4.296 |
| `P142` | `L83_PowerDistributionUnit` | 0.685 | 1.92 | 4.296 |
| `P143` | `L83_PowerDistributionUnit` | 0.515 | 2.08 | 4.296 |
| `P144` | `L83_PriorityLoadSwitchingRack` | 2.215 | 1.92 | 4.366 |
| `P145` | `L83_PriorityLoadSwitchingRack` | 2.385 | 1.92 | 4.366 |
| `P146` | `L83_PriorityLoadSwitchingRack` | 2.215 | 2.08 | 4.366 |
| `P147` | `L83_SafetyEssentialLoadBus` | 4.415 | 1.5925 | 4.338 |
| `P148` | `L83_SafetyEssentialLoadBus` | 4.585 | 1.5925 | 4.338 |
| `P149` | `L83_SafetyEssentialLoadBus` | 4.415 | 1.7525 | 4.338 |
| `P150` | `L83_AttitudeControlLoadEmulator` | -1.985 | 2.2077 | 4.532 |
| `P151` | `L83_AttitudeControlLoadEmulator` | -1.815 | 2.2077 | 4.532 |
| `P152` | `L83_AttitudeControlLoadEmulator` | -1.985 | 2.3677 | 4.532 |
| `P153` | `L83_MissionComputerLoadEmulator` | -0.785 | 2.2077 | 4.532 |
| `P154` | `L83_MissionComputerLoadEmulator` | -0.615 | 2.2077 | 4.532 |
| `P155` | `L83_MissionComputerLoadEmulator` | -0.785 | 2.3677 | 4.532 |
| `P156` | `L83_OpticalTelemetryLoadEmulator` | 0.465 | 2.2077 | 4.532 |
| `P157` | `L83_OpticalTelemetryLoadEmulator` | 0.635 | 2.2077 | 4.532 |
| `P158` | `L83_OpticalTelemetryLoadEmulator` | 0.465 | 2.3677 | 4.532 |
| `P159` | `L83_SciencePayloadLoadEmulator` | 1.715 | 2.2077 | 4.532 |
| `P160` | `L83_SciencePayloadLoadEmulator` | 1.885 | 2.2077 | 4.532 |
| `P161` | `L83_SciencePayloadLoadEmulator` | 1.715 | 2.3677 | 4.532 |
| `P162` | `L83_HeaterThermalLoadEmulator` | 3.035 | 2.2077 | 4.532 |
| `P163` | `L83_HeaterThermalLoadEmulator` | 3.205 | 2.2077 | 4.532 |
| `P164` | `L83_HeaterThermalLoadEmulator` | 3.035 | 2.3677 | 4.532 |
| `P165` | `L83_MissionTaskPowerScheduler` | -3.985 | 1.5619 | 6.285 |
| `P166` | `L83_MissionTaskPowerScheduler` | -3.815 | 1.5619 | 6.285 |
| `P167` | `L83_MissionTaskPowerScheduler` | -3.985 | 1.7219 | 6.285 |
| `P168` | `L83_LoadForecastEnergyBudgetAnalyzer` | -2.485 | 1.5619 | 6.285 |
| `P169` | `L83_LoadForecastEnergyBudgetAnalyzer` | -2.315 | 1.5619 | 6.285 |
| `P170` | `L83_LoadForecastEnergyBudgetAnalyzer` | -2.485 | 1.7219 | 6.285 |
| `P171` | `L83_EclipseEntryPredictionConsole` | -1.085 | 1.5619 | 6.285 |
| `P172` | `L83_EclipseEntryPredictionConsole` | -0.915 | 1.5619 | 6.285 |
| `P173` | `L83_EclipseEntryPredictionConsole` | -1.085 | 1.7219 | 6.285 |
| `P174` | `L83_SunlightReturnChargePlanner` | 0.315 | 1.5619 | 6.285 |
| `P175` | `L83_SunlightReturnChargePlanner` | 0.485 | 1.5619 | 6.285 |
| `P176` | `L83_SunlightReturnChargePlanner` | 0.315 | 1.7219 | 6.285 |
| `P177` | `L83_ContingencyPowerFaultInjector` | 1.965 | 1.5119 | 6.335 |
| `P178` | `L83_ContingencyPowerFaultInjector` | 2.135 | 1.5119 | 6.335 |
| `P179` | `L83_ContingencyPowerFaultInjector` | 1.965 | 1.6719 | 6.335 |
| `P180` | `L83_SolarGimbalJamSimulator` | -3.585 | 5.565 | -0.46628 |
| `P181` | `L83_SolarGimbalJamSimulator` | -3.415 | 5.565 | -0.46628 |
| `P182` | `L83_SolarGimbalJamSimulator` | -3.585 | 5.725 | -0.46628 |
| `P183` | `L83_BatteryCellImbalanceSimulator` | 8.765 | 4.065 | 1.838 |
| `P184` | `L83_BatteryCellImbalanceSimulator` | 8.935 | 4.065 | 1.838 |
| `P185` | `L83_BatteryCellImbalanceSimulator` | 8.765 | 4.225 | 1.838 |
| `P186` | `L83_OpticalBurstDemandInjector` | 3.165 | 1.345 | 6.108 |
| `P187` | `L83_OpticalBurstDemandInjector` | 3.335 | 1.345 | 6.108 |
| `P188` | `L83_OpticalBurstDemandInjector` | 3.165 | 1.505 | 6.108 |
| `P189` | `L83_IndependentUndervoltageProtector` | 5.015 | 1.5725 | 6.0015 |
| `P190` | `L83_IndependentUndervoltageProtector` | 5.185 | 1.5725 | 6.0015 |
| `P191` | `L83_IndependentUndervoltageProtector` | 5.015 | 1.7325 | 6.0015 |
| `P192` | `L83_EPSPowerSafetyPLC` | 10.715 | 1.5673 | 6.646 |
| `P193` | `L83_EPSPowerSafetyPLC` | 10.885 | 1.5673 | 6.646 |
| `P194` | `L83_EPSPowerSafetyPLC` | 10.715 | 1.7273 | 6.646 |
| `P195` | `L83_MainBusEnergyManagementController` | 5.715 | 2.2673 | 5.596 |
| `P196` | `L83_MainBusEnergyManagementController` | 5.885 | 2.2673 | 5.596 |
| `P197` | `L83_MainBusEnergyManagementController` | 5.715 | 2.4273 | 5.596 |
| `P198` | `L83_Protected5VControlSupply` | -5.585 | 1.0973 | 6.546 |
| `P199` | `L83_Protected5VControlSupply` | -5.415 | 1.0973 | 6.546 |
| `P200` | `L83_Protected5VControlSupply` | -5.585 | 1.2573 | 6.546 |
| `P201` | `L83_OrbitalPowerSCADA` | -0.085 | 7.1893 | -4.881 |
| `P202` | `L83_OrbitalPowerSCADA` | 0.085 | 7.1893 | -4.881 |
| `P203` | `L83_OrbitalPowerSCADA` | -0.085 | 7.3493 | -4.881 |
| `P204` | `L83_MissionSurvivalCertificationBeacon` | -0.085 | 8.72 | -4.921 |
| `P205` | `L83_MissionSurvivalCertificationBeacon` | 0.085 | 8.72 | -4.921 |
| `P206` | `L83_MissionSurvivalCertificationBeacon` | -0.085 | 8.88 | -4.921 |
| `P207` | `L83_EmergencyPowerIsolationStation` | 11.745 | 1.57035 | 6.438 |
| `P208` | `L83_EmergencyPowerIsolationStation` | 11.915 | 1.57035 | 6.438 |
| `P209` | `L83_EmergencyPowerIsolationStation` | 11.745 | 1.73035 | 6.438 |
| `P210` | `L83_SpacecraftPowerThermalInspectionCamera` | 5.445 | 4.42 | 2.858 |
| `P211` | `L83_SpacecraftPowerThermalInspectionCamera` | 5.615 | 4.42 | 2.858 |
| `P212` | `L83_SpacecraftPowerThermalInspectionCamera` | 5.445 | 4.58 | 2.858 |

## ANNEX E — REUSED ART SOCKETS / ELECTRICAL TYPE CONFLICT REPORT

Source analysis: 145 anchors referenced by wire endpoints; 46 anchors referenced more than once; **26** anchors mix different signal kinds. A shared decorative location is **not proof** of electrically compatible shared use. These conflicts must be explicitly resolved in Unity.

| Anchor | Signal kinds | Referencing wire IDs |
|---|---|---|
| `P043` | ANALOG_FEEDBACK, DIGITAL_CONTROL | W028, W047 |
| `P052` | DIGITAL_CONTROL, POWER_CONTROL | W003, W037 |
| `P055` | ANALOG_FEEDBACK, DIGITAL_CONTROL | W035, W045 |
| `P056` | DIGITAL_CONTROL, POWER_CONTROL | W004, W038 |
| `P075` | ANALOG_FEEDBACK, POWER_CONTROL | W010, W104 |
| `P086` | ANALOG_FEEDBACK, DIGITAL_CONTROL | W093, W107 |
| `P087` | ANALOG_FEEDBACK, DIGITAL_CONTROL | W094, W110 |
| `P112` | ANALOG_FEEDBACK, DIGITAL_CONTROL, POWER_CONTROL | W005, W056, W060, W064, W073 |
| `P113` | ANALOG_FEEDBACK, DIGITAL_CONTROL | W053, W057, W061, W065, W074 |
| `P114` | ANALOG_FEEDBACK, DIGITAL_CONTROL | W054, W058, W062, W071, W098 |
| `P141` | DIGITAL_CONTROL, POWER_CONTROL | W006, W087, W128 |
| `P142` | ANALOG_FEEDBACK, DIGITAL_CONTROL | W079, W088 |
| `P143` | ANALOG_FEEDBACK, DIGITAL_CONTROL | W080, W091 |
| `P144` | DIGITAL_CONTROL, POWER_CONTROL | W007, W082, W085, W122 |
| `P146` | ANALOG_FEEDBACK, DIGITAL_CONTROL | W081, W084, W116 |
| `P166` | ANALOG_FEEDBACK, DIGITAL_CONTROL | W112, W115, W119, W122 |
| `P167` | ANALOG_FEEDBACK, DIGITAL_CONTROL | W113, W117, W120, W126 |
| `P189` | ANALOG_FEEDBACK, POWER_CONTROL | W009, W106 |
| `P192` | DIGITAL_CONTROL, POWER_CONTROL | W001, W128, W131, W136 |
| `P194` | ANALOG_FEEDBACK, DIGITAL_CONTROL | W127, W130, W135 |
| `P195` | ANALOG_FEEDBACK, DIGITAL_CONTROL, POWER_CONTROL | W002, W013, W016, W019, W022, W025, W028, W036, W041, W049, W088, W101, W104, W107, W111, W124, W132, W136 |
| `P196` | ANALOG_FEEDBACK, DIGITAL_CONTROL | W011, W014, W017, W020, W023, W026, W029, W039, W042, W078, W090, W102, W105, W108, W112, W125, W133, W138 |
| `P197` | ANALOG_FEEDBACK, DIGITAL_CONTROL | W012, W015, W018, W021, W024, W027, W035, W040, W048, W087, W093, W103, W106, W109, W116, W127, W134 |
| `P201` | ANALOG_FEEDBACK, POWER_CONTROL | W008, W097, W100, W133 |
| `P202` | ANALOG_FEEDBACK, DIGITAL_CONTROL | W052, W098, W110, W134 |
| `P203` | ANALOG_FEEDBACK, DIGITAL_CONTROL | W096, W099, W126, W135 |

## ANNEX F — SIX OBJECTIVE RECORDS FROM SOURCE CSV

| Objective | Exact title | Authoritative evidence planned in the source |
|---:|---|---|
| 1 | Build the Orbital Energy Network | 138 actual player-created wires; 212 valid functional terminals; valid control power, converters, BMS, mission loads, independent protection and W104 |
| 2 | Capture and Regulate Solar Energy | Deployed and calibrated solar wings; actual solar incidence and PV IV measurements; independent MPPT controllers; bus precharge and stable DC regulation |
| 3 | Qualify Battery Storage and Distribution | Cell group sense, protected pack current, BMS, balancing, charge/discharge, contactors, thermal control and priority load switching passed |
| 4 | Survive the First Complete Eclipse | Fresh sunlight/eclipsed/sunlight cycle with physically conserved energy, essential bus stability, validated mission task deadlines and battery reserve |
| 5 | Recover Three Power-System Disturbances | Gimbal jam, cell imbalance and optical burst demand each physically activate and are repaired; fresh orbital cycle passes all mission and safety metrics |
| 6 | Restore Critical DC Bus Feedback | Real W104 physically disconnected, measurement stale, supervised power-safe state and latched fault; reconnect, calibrate, explicit reset, then new full orbital EPS certification |

## ANNEX G — VERIFIED UNITY VISUAL HELPER AND ART MENUS

- Runtime: `ProjectSpark.Visuals.SparkLevel83VisualRig` with methods exactly as shown in Section 10.
- Art placement: `Tools/Project Spark/Levels/83/Place HD Art Under Selected Layout`.
- Material upgrade: `Tools/Project Spark/Levels/83/Create & Apply HD Materials To Selected Art`.
- Art validator: `Tools/Project Spark/Levels/83/Validate Imported HD Art`.
- New full gameplay builder requested: `Tools/Project Spark/Levels/Create Level 83` (**not yet in art pack**).

**FINAL REAL COMPLETION: `LevelGamePlayManager` 6/6.**
