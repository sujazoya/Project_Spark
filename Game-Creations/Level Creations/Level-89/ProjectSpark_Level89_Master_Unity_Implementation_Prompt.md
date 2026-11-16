# PROJECT SPARK — LEVEL 89 MASTER UNITY IMPLEMENTATION PROMPT

## LEVEL 89 — THE HELIOS STORM

**Advanced AAA Solar Particle Event Defense • Space Weather Instruments • Energy-Binned Radiation Simulation • Distributed Dosimetry • Finite Water-Wall Shielding • Mechanical Shield Cassettes • Protected Life Support • Independent Safety PLC • Three Dynamic Disturbances • Critical W149 Feedback Recovery**

> **SOURCE-GROUNDED IMPLEMENTATION SPECIFICATION.** This implementation prompt references the exact `ProjectSpark_Level89_HD_AAA_Models.zip`, its 92 individual models, 76 named pivots, 170 conceptual wire definitions, 248 modeled socket positions, 340 proposed logical contacts, 68 multi-use socket records, six objective records, 17 conceptual process-flow entries and actual Unity art helper code. Art assets and CSV plans are **not** working gameplay, electrical connectivity, particle transport or validated radiation protection. The physics/control model and tests described here are requirements for the next Unity implementation stage, not existing accomplishments.

# 01. PRIMARY MANDATE

Act as my **senior Unity C# gameplay and Editor engineer, Project Spark circuit integration specialist, spacecraft radiation-instrumentation and electrical-controls engineer, deterministic scientific gameplay simulation architect, propulsion-independent mechanical automation engineer, independent safety PLC designer, mission resource planner, AAA technical artist, and QA automation engineer**.

**IMPLEMENT THE COMPLETE, REAL, PLAYABLE LEVEL 89 — THE HELIOS STORM — INSIDE MY CURRENT PROJECT SPARK UNITY GAME.** Do not redesign the already generated 3D kit. Import the exact supplied modular FBX set, inspect my actual project APIs, preserve all Levels **1–88**, and implement 170 player-created electrical/signal connections, calibrated detector and dosimetry models, a fictional energy-binned particle-event environment, geometric shielding and finite water conservation, motorized protective structures, genuine three-disturbance repair loops, W149 disconnect/reconnect/fresh-calibration/explicit-reset behavior, six evidence-based objectives, SCADA, simulation-driven animations, one-click Editor construction, diagnostics, automated tests and honest validation. Completion means the real `LevelGamePlayManager` certifies **6/6** in Play Mode—not a mock flag or console log.

# 02. READ THE ACTUAL ASSET SOURCE FILES FIRST

Inspect the user-supplied ZIP directly, prioritizing source tables over narrative:

- `Model_Manifest.json`, `Art_Assembly.csv`, `Component_Roles.csv`, `README_Unity.txt`, `Verification_Report.json`.
- `Animation_Rigs.csv` — exactly **76** original pivot definitions with owner, kind, axis and XYZ metre positions.
- `Gameplay_Wiring_Concept_W001_W170.csv`, `Gameplay_Wiring_Budget.csv` — **170 conceptual** WIDs; no player-created wires exist in the art source.
- `Terminal_Anchor_Plan_248.csv`, `Visual_Terminal_Sockets_248.csv` — exactly **248 art-only** visual sockets.
- `Terminal_Logical_Port_Reconciliation_340.csv` — **340 independent proposed endpoint contacts**.
- `Terminal_Anchor_Signal_Conflict_Report.csv` — **68** shared art-anchor records; **35** mix different signal kinds.
- `Objective_Certification_Plan_6.csv`, `W149_Critical_Shelter_Dose_Recovery.md`, `Physical_Shielding_Process_Concept.csv`, `Gameplay_Integration_Plan.md`.
- `Texture_Manifest.json`, `Material_Palette.json`, `AAA_Polish_And_Lighting_Plan.md`, `Collision_LOD_Production_Plan.csv`.
- `Unity_Optional/SparkLevel89ArtPlacementEditor.cs`, `SparkLevel89MaterialUpgradeEditor.cs`, `SparkLevel89AssetValidatorEditor.cs`, `SparkLevel89VisualRig.cs`.

The annexes reproduce source-authored rows and exact IDs. If the archive differs from this document, investigate and privilege what is actually imported. Do not invent methods on existing Project Spark types.

# 03. VERIFIED ART BASELINE, BOUNDS, AND NOT-IMPLEMENTED STATUS

Actual manifest: **92 numbered binary FBX 7400 models, 92 corresponding GLB files**, one assembled showcase FBX + GLB, **156,444** triangles and **1,154** mesh parts across modular equipment, **76** mesh-owning pivot groups, **248** decorative socket nodes, **170** conceptual player wires, **340** proposed independent logical endpoint contacts, **68** reused socket records and six unimplemented objective declarations. Scene bounds from source: min **(-16.25, -3.42, -17.75) m**, max **(16.25, 15.97648, 13.1) m**, overall approx **32.5 m X × 19.39648 m Y × 30.85 m Z**. Materials are **12 procedural reference sets / 36 reference PNGs**.

The source structural report has **34/34** checks passed, which establishes archive and mesh-format structure only. It explicitly does **not** verify FBX DCC roundtrip, Unity import/compilation/Play Mode, playable `SparkTerminal` components, solver, particle physics, runtime PLC, objectives or AAA finished art. Every source objective has `Implemented=NO`. Do not describe project functionality as present until implemented and actually exercised.

# 04. INSPECT THE LIVE PROJECT SPARK CODEBASE AND LEVEL SCHEMAS

Read `SparkElectricalSolver`, `SparkCircuitSystem`, `SparkElectricalComponent`, `SparkTerminal`, `SparkSwitch`, `SparkLevelDefinition`, `SparkLevelEvaluator`, `SparkLevelSceneBindings`, `SparkLevelWorkstationBindingBridge`, `SparkShelfSlot`, `SparkObjectShelf`, `SparkLevelLayoutLoader`, `SparkLevel`, `LevelGamePlayManager`, real wire creation/removal events, inspection/interaction, shop and persistence classes where available. Determine namespaces, serialized fields, ownership, circuit evaluation timing, 5 V return reference, actual terminal direction/classification, valid shared buses, direct scene/prefab builder patterns and objective historical certification. Treat prior prompt documents as design history, not proof of compiled APIs. Use adapters if signatures differ.

# 05. NONNEGOTIABLE IMPLEMENTATION RULES

1. Preserve Levels 1–88 and user-customized gameplay components; do not overwrite shared solvers or global prefabs blindly.
2. All 170 wires are truly player-created and initially **unconnected**. The electrical graph controls actual commissioning and sensor/actuator authorization.
3. The 248 gold-rimmed **visual sockets are not functional terminals**; allocate the required independent contacts safely.
4. Keep educational 5 V control, fictional storm/exposure physics, finite water/resources, actuators, and independent protection in separate authority domains.
5. Storm energy-bin intensity and trajectory evolve regardless of the player's buttons, hologram colors or sensor availability.
6. No claims of accurate space-weather prediction, particle transport, real-world human dosimetry, radiation safety certification or flight qualification.
7. Shield mass, covered solid angle, actual positions and gaps affect modeled exposure, not an instant arbitrary `radiationSafe=true` switch.
8. Water inventory is finite; moving water cannot duplicate mass, ignore reserve requirements or bypass pump energy costs.
9. Requested shield position, servo command, lock state and actual physical state must remain distinct.
10. Detectors have calibration, range, sample age, statistics/noise proxy and possible saturation; saturated samples are invalid, not zero.
11. Essential life support, reserve electrical power, thermal maintenance and crew occupancy obligations must continue through storms.
12. Faults must change real simulation variables, not only warning UI states or a mission boolean.
13. W149 loss invalidates **trusted measured dose rate**, not actual ambient particle flux or sheltered physical exposure.
14. Independent monitoring can protect in an emergency but cannot silently bypass missing trusted W149 for normal certification.
15. Fault latch clears only after actual topology repair, genuinely fresh calibrated measurements, verified interlocks and explicit reset.
16. Objectives 1–5 retain historical certification during deliberate W149 loss; Objective 6 needs fresh complete event evidence.
17. SCADA and visual rig are consumers of qualified states; no reading VFX, lights or mesh transforms to award objectives.
18. Never manufacture test PASS, objective 6/6 or compiled Unity artifacts without running the corresponding checks.

# 06. LEVEL IDENTITY, PLAYER INITIAL STATE, AND SIX OBJECTIVES

Use `level_89`, title **The Helios Storm**, subtitle **Solar Radiation Protection, Dosimetry, and Adaptive Shielding**, expert/master difficulty. The mission begins at actual **0/6**, no conceptual puzzle wires installed, detector channels unqualified, four shield cassettes not commissioned, water allocation unset, major storm not yet started, and shelter protection/crew utility not certified. A quiet training-mode particle background is permitted for initial calibration. W149 must be a real player-installable wire during Objective 1 and a real detachable critical test wire after historical Objectives 1–5.

# 07. GEOMETRY IMPORT, UNITS, AND COMMON-ORIGIN ASSEMBLY

Import the **92 numbered binary FBXs** from `Models/`, preserving their individual names and material/pivot hierarchy; optional GLBs remain interchange/reference assets. FBX numeric geometry uses **centimetres** with `UnitScaleFactor=1`; GLB numeric geometry is in **metres**. Convert FBX centimetres to Unity metres **once only** via the actual importer configuration, then use identity-local copies beneath `L89_HD_VISUAL_ART` at common origin `(0,0,0)`. Y-up, X-width, Z-forward. Validate world bounds against source. Do not instantiate the alternative assembled `Models/Level89_Complete_ArtKit_Showcase.fbx` or matching GLB alongside the modular 92, which would visibly double the scene.

Keep functional terminals, colliders, circuit nodes, process states, objectives and hand-authored gameplay under stable children **outside** the disposable visual art root. Provide idempotent build/cleanup, Undo and source import checks.

# 08. EXACT SHIPPED UNITY ART HELPER MENUS

The supplied `ProjectSpark.EditorTools` art scripts expose:

- `Tools/Project Spark/Levels/89/Place HD Art Under Selected Layout` (`SparkLevel89ArtPlacementEditor`).
- `Tools/Project Spark/Levels/89/Create & Apply HD Materials To Selected Art` (`SparkLevel89MaterialUpgradeEditor`).
- `Tools/Project Spark/Levels/89/Validate Imported HD Art` (`SparkLevel89AssetValidatorEditor`).

Put `*Editor.cs` in an `Editor` folder. Runtime `ProjectSpark.Visuals.SparkLevel89VisualRig` belongs in a runtime assembly. These shipped tools operate on artwork only. **Build a separate full gameplay command** `Tools/Project Spark/Levels/Create Level 89` and do not mistake art placement/validation for completed game logic.

# 09. PBR TEXTURES, COLLIDERS, LOD, AND AAA FINISH

Source maps: **12 sets × albedo / normal / MetalRoughAO** (36 PNGs), each **1024×1024**. Packed R=metallic, G=roughness, B=ambient occlusion; convert roughness to shader-specific smoothness correctly in URP/HDRP or the project's pipeline. All maps are reference procedural textures with production UV authoring required. Finish the armored storm shelter, electro-mechanical rail motors, metallic spectrometers, water tank glass and sight tubes, shield locks, microfasteners, insulated control cabling, instrument glass and hologram transparencies. Author real VFX/lighting rather than claiming meshes already contain all effects. Create appropriate simplified colliders; static meshes and moving shield/pump parts need different collision policies. LOD/hysteresis, static batching, instancing and occlusion must preserve sensor visibility and player-readable wiring. Report actual measured render performance and any remaining art gaps.

# 10. VERIFIED SPARKLEVEL89VISUALRIG PUBLIC API — EXACT CALLS

The package contains `ProjectSpark.Visuals.SparkLevel89VisualRig`, an **art-only readout** component. Its verified public API is:

```csharp
public void BindVisuals();
public void SetActualShieldMechanisms(float actualTopClosed01, float actualBottomClosed01,
    float actualPortClosed01, float actualStarboardClosed01,
    float actualLockEngagement01, bool actualDoorClosed);
public void SetActualWaterAndPumps(float actualPortA01, float actualPortB01,
    float actualStarboardA01, float actualStarboardB01,
    float actualPumpASpeed01, float actualPumpBSpeed01);
public void SetActualSubsystemMotion(float actualServoA01, float actualServoB01,
    float actualServoC01, float actualServoD01,
    float actualVentilationFan01, float actualSurveyAzimuthDeg);
public void SetActualRadiationAndSafety(float measuredActualStormSeverity01,
    float measuredShieldCoverage01, float measuredShelterDoseVisual01,
    bool trustedW149FeedbackFreshAndValid,
    bool actualCriticalFaultLatched, bool actualShelterMissionQualified);
```

Use a data adapter to convert authoritative process/measurement values into normalized `[0,1]` visuals or degrees, respecting actual arguments. Methods **do not** set true particle flux, safe dose, actuator physics, wire continuity or objective completion. The shipped rig binds known `ANIM_*` transforms in `Awake`, stores homes, updates mechanical transform offsets in `LateUpdate` and controls optional lights. It currently updates only a subset of 76 pivots; extend for other art indicators via separate read-only visual behavior if required. Do not pretend it has nonexistent setter methods.

# 11. ALL 76 PIVOTS — AXIS AND HIERARCHY REQUIREMENTS

Use `Animation_Rigs.csv` and Annex B for **all 76 exact** `Pivot`, `Model`, `Kind`, `Axis` and source metre coordinates. Distinguish `rotation`, `translation`, and `indicator` (visual anchor only). Particularly important: `ANIM_SHELTER_MAIN_SHIELD_DOOR` translation; `ANIM_SHIELD_CASSETTE_TOP/BOTTOM/PORT/STARBOARD` translation; `ANIM_SHIELD_ACTUATOR_A/B/C/D` translations; water pump rotors; `ANIM_SHELTER_AIR_BLOWER` rotation; locking pins; `ANIM_SURVEY_ANTENNA_AZIMUTH`. Verify each transform belongs to its correct owner and is unique. Preserve original local pivots and home transforms; never rotate entire building equipment when only a valve or panel must move. A visual indicator must never drive mechanical collision or qualification.

# 12. FOUR AUTHORITY DOMAINS AND COUPLINGS

**A. Player electrical network:** protected 5 V control power, valid signal returns, digital controls, analog detector measurements, independent safety wires, 170 actual topology edges, registered SparkTerminal data and live graph checks.

**B. Fictional particle + shield physical plant:** external solar-particle event, energy bins and angular distributions, shield mass/areal density, gaps, actual barriers, mass transfer, power use, effective sheltered flux and accumulated educational dose proxy. This is a separate numerical model; ordinary 5 V puzzle wires are not physical high-energy radiation beams or water pipes.

**C. Independent functional safety:** safety PLC, emergency isolate, shelter door interlock, independent dosimetry, safe actuation, life-support continuity, hard inhibits and fault latching. It may protect during W149 loss but must not replace trusted W149 normal certification.

**D. Scenario and historical evidence:** run ID, event timing, instrument validity, progress, dynamic faults and six completed objective certificates. SCADA and holograms display state but are not separate authorities.

# 13. EXACT 170-WIRE BUDGET FROM SOURCE

| WIDs | Source group | Count |
|---|---|---:|
| W001–W012 | Educational 5 V control and independent returns | 12 |
| W013–W038 | Particle detection and space-weather acquisition | 26 |
| W039–W062 | Distributed radiation sensing and calibration | 24 |
| W063–W084 | Hydrogen-rich water wall and finite inventory | 22 |
| W085–W106 | Motorized shield barriers and position feedback | 22 |
| W107–W128 | Storm forecasting and essential-system continuity | 22 |
| W129–W154 | Independent safety critical feedback and fault handling | 26 |
| W155–W170 | SCADA data, diagnostics and qualification | 16 |
| **TOTAL** | | **170** |

Use the actual `Gameplay_Wiring_Concept_W001_W170.csv` as authority for exact model pair, visual anchors, signal kind and purpose. Every row explicitly has `State=CONCEPT_ONLY_PLAYER_NOT_CONNECTED`. The Editor builder must declare the connection but never begin with an already-connected physical wire. A row qualifies only after actual player-created topology passes live solver validation, including legal connection direction, shared returns, safe power-domain isolation, compatibility and availability.

# 14. 248 ART SOCKETS VS. 340 INDEPENDENT LOGICAL CONTACTS

The art has exactly **248** original decorative sockets `P001`–`P248`, but **170 wires × 2 endpoints = 340** logically independent endpoint occurrences. There are **68 reused art-anchor entries**, **35** with mixed signal kinds. Most severe: `P222` is the W149 destination and is reused by **12** contacts spanning `ANALOG_FEEDBACK`, `DIGITAL_CONTROL`, `POWER_CONTROL`, `SAFETY_FEEDBACK`. `P072`, the W149 source socket, represents **three** independent analog contacts. Never make all contacts at a shared visual socket the same electrical node. It would short independent measurements, power feeds and safety signals.

Implement a deterministic, reported mapping `WireID / Endpoint / owner model / source P### / distinct FunctionalPortID / SignalKind / direction / expected electrical domain / SparkTerminal runtime instance`. Use exact 340 source rows as proposed identities (their status is `PLANNED_NOT_ACTUAL_SPARKTERMINAL`). Split physically separate child connector contacts, preserve 248 decorative socket transforms, and add labeled offset pads where physically necessary. Legitimate shared ground/power buses must be explicitly modeled and solver-approved; do not infer sharing from visual position. Emit `Level89_TerminalReconciliationReport.csv` covering conflicts and any additional contacts.

# 15. CRITICAL W149 — SOURCE-VERIFIED ELECTRICAL ROUTE

The **only** source `Critical=YES` row is:

- **Wire:** `W149`.
- **Source:** `L89_ShelterReferenceDosimeter`, modeled socket **`P072`**.
- **Source functional port:** `TRUSTED_SHELTER_DOSE_RATE_OUT` (from the shipped W149 recovery plan).
- **Destination:** `L89_HeliosSafetyPLC`, modeled socket **`P222`**.
- **Destination functional port:** `TRUSTED_SHELTER_DOSE_RATE_IN`.
- **Signal kind:** `ANALOG_FEEDBACK`.
- **Purpose:** `CRITICAL trusted shelter dose-rate measurement to independent radiation safety PLC`.

The generic 340-row reconciliation table uses a per-wire contact label (`W149_OUT_ANALOG_FEEDBACK` / `W149_IN_ANALOG_FEEDBACK`) whereas the separate recovery plan supplies human-readable semantic aliases above; map both onto the same actual two distinct functional contacts, never instantiate two parallel trusted sensing channels by mistake. W149 is a real physically disconnectable electrical graph edge—not a UI switch, material or decoration.

# 16. ADJACENT SAFETY SIGNALS — NO W149 BYPASS

Inspect exact W129–W154 routes (Annex H). `W129` actual door latch, `W130` shield locks, `W131` actual shield hardware quality, `W132` independent dosimeter, `W133` life support status, `W134` reserve power, `W135` scenario shielding permit, `W136` emergency isolation, `W137` motor drive safety, `W138` sequencing PLC authorization, `W140` emergency ventilation, `W145` fault logger command, `W146` W149 patch diagnostic, `W147` explicit reset request, `W148` fault-latch state, `W153` certification display and `W154` independent storm-state verification all serve separate functions.

**W146 is only a patch diagnostic, not an alternate measurement. W132 is an independent protective detector, not normal trusted W149.** SCADA trend W167, cached history, event recorder, image colors and other instruments cannot silently supply W149's normal trusted dose feedback. The independent protection path must remain effective even when normal certification is disabled.

# 17. ONE AUTHORITATIVE HELIOS PROCESS STATE

Track seeded storm event `runId`, physical time, phase, intensity by energy bin, directional exposure profile, any fictional secondary particle component, source uncertainties; external/habitat/shelter modeled flux and exposure proxy; detectors' actual signal, measured sample values, ranges, times, noise, calibration and overrange; actual four water-wall tank mass, reservoir and life-support reserve, valve states, pump speeds/power, leaks, shielding areal-density maps; actual four cassette positions, motor commands, accelerations, encoder readings and locks; door/occupancy/ventilation/power; thermal and power status; independent safety conditions and W149 validity; integrated dose history; fault timelines, evidence and historical objective state.

Strictly separate **commanded configuration**, **physical plant**, **sampled instrument reading**, and **qualified protective/certification decision**. Do not allow process, PLC, SCADA, visual rig and objective tracker to each own contradictory versions of exposure or safety.

# 18. FIXED-STEP DETERMINISTIC UPDATE ORDER

Use a documented fixed-step educational process model, deterministic seeds and bounded numerical integration, for example:

1. Capture player wire graph, live 5 V returns, commands and critical wire state.
2. Evaluate independent hard interlocks and previously latched protection.
3. Advance fictional storm environment in time independent of sensors and UI.
4. Integrate physical servo/panel/door/lock positions with limits and jam constraints.
5. Integrate pump, valve and water inventory transfer with mass and energy accounting.
6. Evaluate actual shield geometry and water areal density per direction/energy group.
7. Compute external, unshielded and sheltered educational exposure rates.
8. Update required life-support and reserve-power consumption and continuity.
9. Integrate accumulated fictional scenario dose/exposure for the active run.
10. Sample detector channels with range/calibration/noise/latency/overrange and live topology.
11. Enforce independent safety thresholds and W149 trust/freshness checks.
12. Transition PLC/contingency states from actual evidence and safe commands.
13. Collect timestamped objective test evidence and historical certifications.
14. Publish read-only actual/measured state to SCADA, diagnostics, VFX and art rig.

No render-frame-rate-dependent radiation totals, zero-time mass transfers, negative inventories, NaN/Inf or infinite actuator velocities. Adjustable time scale must not skip verification phase transitions.

# 19. SOLAR EVENT PHASES AND INDEPENDENT STORM PHYSICS

Build a fictional, bounded event progressing **quiet → rise → peak → decay → post-storm clearance**. Arrival time is a scenario scheduler estimate, not a privileged truth. External fluence/intensity must evolve from scenario time independently of what the player or instruments display. Permit phase-specific energy spectra, duration, intensity ramps, directional profile and injected higher-energy surge. Allow deterministic replay and configurable lesson-grade thresholds/time windows without suggesting real solar weather is controllable or always predictable. Avoid implying that flares visibly traveling through glass represent literal radiation mechanics; dramatic VFX are cinematic only.

# 20. ENERGY BINS AND QUALIFIED MEASUREMENT FORMULATION

Represent several fictional particle energy bins `i` with time-dependent intensity `Phi_i(t)` and configured scenario response weights `q_i`. A deliberately simplified pedagogical exposure proxy can use `rate_proxy(t)=sum_i Phi_i(t)*T_i(t)*q_i`, where `T_i` is a bounded *illustrative* transmission determined by geometry/material coverage. Document all assumptions. This is **not** validated transport, absorbed dose in tissue, or medical/shelter certification. Use game units or explicitly label any proxy units as nonphysical. Require nonnegative intensities, finite coefficients, and instrument dynamic-range limits.

# 21. EXTERNAL TELESCOPES, SPECTROMETERS AND COUNTER DYNAMICS

Use both `L89_SolarEnergeticParticleTelescopeA/B`, `L89_ChargedParticleSpectrometer`, `L89_ProtonEnergyBinAnalyzer`, `L89_HeavyIonChannelDetector`, `L89_PrimaryParticleCounter`, `L89_SecondaryParticleCounter`, `L89_DetectorPulseShaper`, `L89_ParticleCollimatorHead`, `L89_SolarVectorTrackingGimbal`. Sensor readings depend on actual configured orientation/acceptance, bin response, background, sampling interval, saturation and calibration. Do not offer an always-perfect global true intensity value through an uncalibrated counter.

# 22. DOSIMETRY AT EXTERNAL, HABITAT AND SHELTER LOCATIONS

Model separate readings and accumulation at `L89_ExternalDosimeterArray`, `L89_HabitatDosimeterArray`, `L89_ShelterReferenceDosimeter`, `L89_IndependentShelterSafetyDosimeter`, and `L89_TriaxialRadiationDoseProbe`. A shelter detector samples the *sheltered plant* (which may be much lower), not simply the external sensor multiplied by a UI setting. Each record needs measured value, channel range, acquisition time, sample age, calibration revision and uncertainty. Independent safety sensor may disagree; diagnose the discrepancy rather than averaging away a fault.

# 23. SENSOR CALIBRATION, OVER-RANGE AND STALE DATA

Allow protected calibration using `L89_SpectralCalibrationSource` and `L89_DosimeterCalibrationFrame` as fictional low-energy reference instruments. Validate zero/reference, full-scale/range, instrument power, temperature proxy, timestamps and sample stability. Over-range must be flagged as a **lower-bound unknown/high** condition, never a bogus fixed low reading; stale samples cannot be used as current evidence. Calibrating one sensor does not automatically qualify other sensors. Store individual calibration version IDs and expire evidence when a relevant sensor is serviced or disconnected.

# 24. REDUNDANCY COMPARISON AND INDEPENDENT PROTECTION

`L89_DetectorRedundancyComparator` compares independently measured channels after correcting for different geometry/range. Agreement supports diagnosis; disagreement is not automatically solved by choosing the smallest reading. Independent safety PLC may make conservative inhibit decisions from `L89_IndependentShelterSafetyDosimeter` and separate hard interlocks even while W149 is invalid, but must **never** claim a trusted W149 normal-certification sample was acquired.

# 25. ENERGY-DEPENDENT SHIELDING AND AREAL DENSITY

Four water-wall tanks are physical mass stores whose filled volumes map to local mass distribution and directional areal-density proxies. Protective cassettes contribute fixed fictional material layers depending on measured pose and lock state. For each energy bin and source direction, estimate effective coverage, slab material proxies and transmission. Distinguish open gaps versus partially covered directions; no single global opacity/percentage replaces geometry. An illustrative bounded attenuation surrogate may be used if clearly labeled nonphysical. Avoid advising real shield thicknesses or treating the calculation as a real human dose assessment.

# 26. FOUR WATER-WALL TANKS AND FINITE RESOURCE INVARIANTS

Treat `L89_WaterWallTankPortA`, `PortB`, `StarboardA`, `StarboardB` as independent capacity-limited inventories. Source water comes from `L89_WaterReservoirMain` via `L89_WaterTransferPumpA/B` and `L89_WaterDistributionValveManifold`. Track total mass across reservoir, tanks, other reserves, and any simulated leak/loss. `sum(masses_next)=sum(masses_prev)+external_source-external_loss`. No water spawns from a full tank, and tank-level HUD cannot change physical contents. All transfers have bounded flow and valve/pump response time.

# 27. ESSENTIAL LIFE-SUPPORT WATER AND POWER RESERVES

Water is shared with the Level 88 life-support abstraction. `L89_ThermalWaterResourceGuard`, `L89_WaterMassAllocationController`, `L89_WaterPowerPriorityScheduler`, `L89_ShelterProtectedLifeSupportPLC`, `L89_ShelterEnvironmentalAirSupply`, `L89_ShelterOxygenBufferInterface`, and `L89_ShelterReservePowerDistribution` must maintain actual demand and finite reserve. The player cannot put all water into shielding if ventilation/thermal/oxygen requirements then fail, and cannot switch off every load to pass an exposure-only check. Water movement and servo operation consume modeled energy; essential systems remain active when physically supportable.

# 28. PUMP, VALVE AND LEAK MECHANICS

Requested pump/valve action must pass power, safety and reservoir-limit checks and then change actual flow over time. Pump flow depends on health, head/pressure proxy and valve availability. Measured rotor speeds drive `ANIM_WATER_PUMP_A_ROTOR` / B from the art rig; the rig never drives water mass. A water-wall leak (if implemented as supporting diagnostic) must cause mass loss and an observable reduction in areal coverage; diagnostic panel alone does not prove physical leak or recovery.

# 29. FOUR MOTOR-DRIVEN SHIELD CASSETTES

Use `L89_ShieldPanelTop`, `Bottom`, `Port`, `Starboard`, `L89_ServoShieldActuatorA/B/C/D`, `L89_ShieldCassetteRailArray`, `L89_ShieldMotorDriveCabinet` and `L89_ShieldPositionEncoderRack`. Track actual panel distance/velocity/settling, travel limits, command-versus-feedback error, measured closure and actuator load. A jam arrests **actual movement** even as the command requests closure. Configure speed and position gates; certify cover only after actual arrival and settling. The visual rig's normalized closure range must reflect measured motion.

# 30. PHYSICAL SHIELD LOCKS, DOOR, EGRESS AND OCCUPANCY

`L89_ShieldLockingPinArray`, `L89_ShelterEntranceShieldedBulkhead`, `L89_ShelterDoorActualInterlock`, and `L89_ShelterEmergencyEgressHatch` need distinct requested/actual positions, lock engagements and sensor confirmations. Door status and shield coverage cannot be inferred from animation alone. Enforce game-safe rules for shelter occupancy, emergency egress and lock actuations, including no unsafe door opening during unresolved high storm risk. Emergency operations must remain possible in protected contexts; avoid designs that permanently trap a crew state without recovery path.

# 31. DIRECTIONAL COVERAGE MAP, GAPS AND DIGITAL TWIN

Build a low-cost angular coverage map from four panels, water tanks, door, frame and gap geometry. Record actual material stacks for selected directions and compute a coherent effective transmission for each energy group. `L89_ShieldCoverageDigitalTwin`, `L89_ShelterRadiationShieldQualityMonitor` and `L89_RadiationHeatmapHologram` display derived maps and hotspots. A gap at one side affects only relevant directions/angles rather than globally zeroing protection. Store the current coverage fingerprint with qualification evidence.

# 32. SHELTER EXPOSURE INTEGRATION AND EVIDENCE

Integrate actual fictional sheltered exposure over a full storm `D_run=integral(rate_proxy dt)` from the authoritative process state, using fixed simulation time. Also maintain separately the *reported measured* accumulated dose with channel confidence; both are useful for QA. Qualification requires trusted in-range W149 samples across required intervals, enough monitoring coverage, resource continuity, adequate shielding, valid PLC and a bounded educational scenario budget. Do not use a single final sampled zero reading to erase prior exposure.

# 33. FORECAST VS. ACTUAL STORM, RESOURCE PLANNING

`L89_SpaceWeatherForecastConsole`, `L89_StormArrivalPredictionHologram`, `L89_ShieldingScenarioSafetyController`, `L89_ShelterRadiationContingencyConsole` and associated forecast state should estimate event phases and necessary resource allocation from calibrated data. Forecasts are predictions with uncertainty, not exact preknowledge of disturbance C. Show predicted vs observed bin intensities, cumulative exposure budget, water reserves, shield coverage and remaining essential power, with different confidence states. A false optimistic forecast cannot force actual exposure to be low.

# 34. EXPLICIT PLC AND EMERGENCY STATE MACHINE

Define states such as `OFF`, `WIRE_INSTALL`, `CONTROL_POWER_CHECK`, `INDEPENDENT_SAFETY_CHECK`, `INSTRUMENT_CALIBRATION`, `SHELTER_RESOURCE_PLANNING`, `SHIELD_DEPLOY`, `LOCK_VERIFY`, `OCCUPANCY_READY`, `STORM_QUIET`, `STORM_RISE`, `STORM_PEAK`, `STORM_DECAY`, `POST_EVENT_MONITOR`, `QUALIFIED`, `DISTURBANCE_DIAGNOSIS`, `PROTECTED_HOLD`, `TRUSTED_FEEDBACK_LOST`, `CRITICAL_FAULT_LATCHED`, `RESET_REQUIRED`, `FINAL_CERTIFIED`. Transitions require qualified actual evidence, not a UI countdown alone. Door/egress, E-stop, independent sensor, shield lock and essential power remain enforceable at every state.

# 35. FIRST FULL STORM QUALIFICATION

Begin a new run ID from a verified quiet baseline. Record actual full **quiet → rise → peak → decay** transitions and a post-event clearance check, with continuous trusted dose sampling, independent safety, shield cover, water mass, sensor validity, ventilation/power/oxygen continuity and bounded fictional integrated dose. Require a minimum useful occupancy/mission workload, so all systems off cannot pass. A skipped phase, forced SCADA event flag, missing fresh samples, mass-duplication or failed essential loads invalidates the run. Full event duration is adjustable game time, **not** real storm forecasting.

# 36. DISTURBANCE A — DETECTOR SATURATION

Inject saturation into a chosen detector channel via `L89_DetectorSaturationFaultInjector`. The actual external storm remains unchanged; measured channel rails/flags overrange and becomes ineligible for calibration/forecast certification, possibly corrupting an unprotected forecast. Correct diagnosis requires comparison with redundant instruments, reference/range state, counter health and timestamps. Recover by entering safe acquisition mode, selecting an appropriate fictional instrument range/calibration path, recommissioning and verifying fresh samples. Do not pretend saturation is cured by lowering the solar event input slider.

# 37. DISTURBANCE B — SHIELD CARRIAGE JAM

Use `L89_ShieldJammingFaultInjector` to stop an actual specific cassette's progress; other panels continue to obey independent actuators. Commanded-vs-measured positions diverge; coverage map reveals the missing sector; drive load and fault indicators reflect the jam. Recovery: inhibit unsafe motion, diagnose affected panel/rail, perform fictional service release, verify limit switches and encoder, rehome, redeploy and relock. Requalification must prove real measured protection, not only a cleared alarm.

# 38. DISTURBANCE C — HIGHER-ENERGY PARTICLE SURGE

Use `L89_HighEnergySurgeFaultInjector` to change the **actual simulated storm energy spectrum**, increasing a selected high-energy component compared with the forecast. Existing shielding may be less effective for that bin. Players compare measured vs predicted spectra, re-evaluate transmission/essential reserve and adjust finite water allocation/panel configuration where safe, then rerun a fresh qualified full storm. This is a fictional learning scenario; never claim its scalar model predicts real radiation dose or crew safety.

# 39. THREE-FAILURE RECOVERY QUALIFICATION

Objective 5 needs evidence that each disturbance was activated after normal commissioning, changed an actual correct process/measurement/mechanical variable, was diagnosed by real instrument observations, was repaired by correct player actions, and returned to a qualified configuration. Require a **brand-new full storm** after the last repair with valid trusted W149 and essential resources. Track distinct scenario/event IDs and fault-cause evidence; do not award three failures when the player presses three diagnostic buttons.

# 40. INDEPENDENT SAFETY AND PROTECTED CONTINGENCY

`L89_HeliosSafetyPLC` is the main protected radiation safety controller; `L89_ShieldingScenarioSafetyController` performs shield scenario validity; `L89_ShelterProtectedLifeSupportPLC` protects essential utility. Independent W132 dosimetry and W136 emergency isolation must not depend on ordinary SCADA. If normal trusted W149 becomes invalid, withdraw normal release/open authorization, preserve physically secured barriers, hold protected environmental service, log causal state and latch fault. Never falsely zero true particle flux/exposure, instantly teleport panels closed or guarantee survival if actual reserves are exhausted. If independent instrumentation also fails, show unresolved uncertainty and maintain the safest physically available actions.

# 41. W149 CRITICAL PHYSICAL DISCONNECTION SEQUENCE

Only after Objectives 1–5 are historically certified, the player uses the real Spark wire interaction tool to remove **W149 `P072` → `P222`** in a controlled training fault state. Actual solver graph must now lack that edge. Record event time, topology fingerprint and invalidate new trusted sensor samples immediately/at specified sample timeout. Cached last-good display remains visibly marked historical; normal shelter exposure certification and door-release permission withdraw; safe hold/locked protection and essential utilities continue where possible. Latched alarm text: `CRITICAL SHELTER DOSE FEEDBACK LOST — CERTIFICATION INHIBITED`. Independent dosimetry may trigger extra protection but cannot be relabeled as W149.

# 42. RECONNECT W149, FRESH CALIBRATION AND EXPLICIT RESET

Recreate W149 through the real player wiring system, validate source/target functional ports and 5 V conditioning/returns, acquire **new** in-range timestamped shelter dosimeter readings, compare with independently valid protection inputs as a safety check, verify mechanical barriers/locks, emergency controls and essential reserve, then require operator **explicit reset** (source W147). Refuse reset before prerequisite fulfillment, preserve audit history and issue a **new post-fault run ID** only after legitimate reset. Run full quiet→rise→peak→decay again; old Objective 4/5 events cannot be reused for Objective 6.

# 43. OBJECTIVE 1 — BUILD THE HELIOS RADIATION DEFENSE NETWORK

Exact source title: **Build the Helios Radiation Defense Network**. Certify only after 170 correct player-made graph connections, source 248 modeled art sockets reconciled to type-compatible real 340 logical contacts, valid 5 V power/return, registered detector and actuator controllers, independent life support/power protection, and connected valid W149. Capture a circuit topology hash, source/target mapping proof, functional-supply measurements and timestamps. No early pass from art or pre-connected lines.

# 44. OBJECTIVE 2 — CALIBRATE THE SOLAR RADIATION OBSERVATORY

Exact source title: **Calibrate the Solar Radiation Observatory**. Require two actual particle telescopes, external/habitat/shelter and independent dosimeters, valid reference/calibration, range and timestamps, readable spectrum categories, configured gimbals and healthy acquisition hardware. Document measured channel quality and any rejected saturated/stale readings. Certify from acquisition evidence, not from calibration animations.

# 45. OBJECTIVE 3 — COMMISSION THE RADIATION STORM SHELTER

Exact source title: **Commission the Radiation Storm Shelter**. Require finite water allocation for four wall tanks without violating life-support reserve, powered pump/valve feedback, actually deployed and settled four shield cassettes, engaged mechanical locks/door sensors, measured shield map coverage and functioning essential environmental power/ventilation/oxygen interfaces. Source evidence is mechanical, resource and interlock proof; no declared protected state solely from a command.

# 46. OBJECTIVE 4 — SURVIVE THE FIRST COMPLETE SOLAR STORM

Exact source title: **Survive the First Complete Solar Storm**. Start a fresh run with quiet→rise→peak→decay actual phases, continuously qualified W149, valid independent safety, monitored bounded fictional dose proxy, physical shielding protection and essential-systems continuity. Record time-series integrated exposure, event boundaries, water/power budgets and objective evidence. Full certification cannot come from a countdown or green screen.

# 47. OBJECTIVE 5 — RECOVER THREE RADIATION DEFENSE FAILURES

Exact source title: **Recover Three Radiation Defense Failures**. Prove detector saturation, cassette jam and higher-energy surge each caused actual independent state changes, were diagnosed and appropriately repaired, and then a subsequent new storm event passed under fresh instrument and shelter conditions. Track cause/repair IDs, calibration revisions, physical shielding evidence and new event run ID.

# 48. OBJECTIVE 6 — RESTORE CRITICAL SHELTER RADIATION FEEDBACK

Exact source title: **Restore Critical Shelter Radiation Feedback**. Require historical 1–5, intentional actual wire-edge removal and absence of W149, invalid sample/authorization, protective fault latch, physical wire restoration, genuinely fresh calibrated W149 measurement, verified independent safety, explicit reset, and a brand-new full qualified storm. Preserve past certificates but block 6/6 on reconnection/reset alone. Only the actual `LevelGamePlayManager` may grant **6/6**.

# 49. OBJECTIVE HISTORY, SAVE/LOAD, AND REPLAY

Store objective `certifiedAt`, topology hash, safe-state event IDs, simulation seed, instrument calibration versions, storm run ID and measured/source evidence references. Historical Objectives 1–5 remain certified through deliberate W149 test, but *live machine health* becomes invalid. After scene reload or save/load, do not transform previously connected graph or historical exposure into fraudulent newly acquired post-W149 evidence. Ensure deterministic seeded replay, migration-safe serialization and no double-bonus award on builder rerun.

# 50. LIVE AAA SCADA AND SCIENTIFIC READABILITY

Show storm phase/intensity/energy-bin spectrum; forecast vs actual severity; telescope orientation; per-detector calibration/range/age/saturation; independent vs primary shelter dosimetry; external/habitat/shelter differential exposure proxies; actual shield cassette pose and locks; angular gap map; water mass per tank/reservoir; pumps, valve flow and power draw; life support and ventilation health; finite reserve and remaining storm budget; independent PLC inhibits; W149 graph connected? sample fresh? calibration version? explicit reset ready?; actual current and historically certified objectives 0/6–6/6. Distinguish **true simulated** model values, **measured** instruments and **predicted** future conditions, including unreliable/conflicting samples.

# 51. PLAYER INTERACTIONS, INSTRUMENTS AND PANEL COMMANDS

Implement real actions: `MAIN 5V`, `SAFETY SELF TEST`, `CALIBRATE TELESCOPE A/B`, `SET COLLIMATOR`, `CALIBRATE EXTERNAL/HABITAT/SHELTER DOSIMETERS`, `SELECT RANGE`, `ACQUIRE PARTICLE SPECTRUM`, `CHECK REDUNDANCY`, `FORECAST EVENT`, `ALLOCATE WATER`, `PUMP A/B`, `SET WATER VALVES`, `DEPLOY TOP/BOTTOM/PORT/STARBOARD`, `VERIFY SHIELD ENCODERS`, `ENGAGE LOCKS`, `CLOSE SHELTER DOOR`, `VERIFY LIFE SUPPORT`, `VERIFY ESSENTIAL POWER`, `START STORM`, `REVIEW DOSE INTEGRAL`, `INJECT DETECTOR SATURATION`, `REPAIR DETECTOR`, `INJECT SHIELD JAM`, `SERVICE SHIELD RAIL`, `INJECT ENERGY SURGE`, `REPLAN COVERAGE`, `W149 DISCONNECT/RECONNECT BY REAL WIRE TOOL`, `CALIBRATE W149`, `EXPLICIT SAFETY RESET`, `START FRESH FINAL STORM`. Disabled actions must explain which actual interlock blocks them. No button directly sets objective PASS.

# 52. AAA STORM, EQUIPMENT ANIMATION, LIGHTING AND VFX

Visually stage the huge habitat exterior, armored storm shelter, water-wall panels and orange solar-storm observatory. Use brilliant but controlled sunlight/flaring holograms, readable cyan/amber/red data, local metallic highlights, instrument glow, physically separate moving shield cassettes, tank level surface changes, spinning pump impellers and lock-pin movement. Show signal saturation by instrument readings and alert indicators, jam by actual pose mismatch, surge by new spectrum, and W149 loss by untrusted channel red status while physical shelter exposure continues normally. Never make a spectacular VFX effect the authoritative dose or protection rule.

# 53. RECOMMENDED NEW RUNTIME C# COMPONENTS — ADAPT TO REAL CODE

Only when absent in the project, consider: `SparkLevel89Coordinator`, `SparkLevel89ProcessState`, `SparkLevel89ElectricalBindings`, `SparkLevel89TerminalReconciler`, `SparkLevel89StormEventModel`, `SparkLevel89EnergySpectrumModel`, `SparkLevel89FictionalExposureIntegrator`, `SparkLevel89ParticleInstrumentModel`, `SparkLevel89DosimeterAcquisition`, `SparkLevel89CalibrationService`, `SparkLevel89WaterMassInventory`, `SparkLevel89TransferPumpModel`, `SparkLevel89ShieldActuatorController`, `SparkLevel89ShieldGeometryModel`, `SparkLevel89ShieldLockInterlocks`, `SparkLevel89LifeSupportReserve`, `SparkLevel89RadiationSafetyPLC`, `SparkLevel89ForecastEstimator`, `SparkLevel89DisturbanceController`, `SparkLevel89W149Recovery`, `SparkLevel89ObjectiveTracker`, `SparkLevel89SCADA`, and `SparkLevel89VisualRigAdapter`. Decouple by interfaces and make only one component authoritative over each process state field. No speculative calling of source APIs without compile validation.

# 54. ONE-CLICK FULL LEVEL 89 UNITY GAMEPLAY BUILDER

Implement `Tools/Project Spark/Levels/Create Level 89`, separate from shipped art menus. On repeat invocations it must idempotently create/update `Level_89.asset`, `Level_89_Layout.prefab`, six objective definitions, exact 92 modular FBX instances, one replaceable `L89_HD_VISUAL_ART`, named 76 pivot bindings, collision/interactions, 248 original art sockets plus functional contacts reconciled to 340 endpoint declarations, 170 initially unconnected player-wire targets, control and independent safety bindings, educational particle and shielding process, water/power/ventilation constraints, three disturbances, W149 sequence, PLC, SCADA and certificate logger.

Validate project compile-time API compatibility, avoid duplicate circuit solvers/sockets, preserve authored gameplay children outside the art root, avoid wiping previously completed levels, and generate meaningful import/terminal/mission-simulation reports. Do not set objective progress or create prewired cable meshes to fake Level 89 initial state.

# 55. ACCEPTANCE TESTS — SOURCE IMPORT AND VISUALS (01–14)

1. All 92 original numbered FBXs found; optional 92 GLBs available.
2. Assembled showcase not duplicated alongside modular set.
3. FBX centimetre-to-metre scaling performed exactly once.
4. Overall bounds agree with 32.5 m X / ~19.396 m Y / 30.85 m Z.
5. 76 unique correct mesh-owning animation pivots exist.
6. Pivot parent/axis/kind/source coordinates match CSV.
7. All 248 original modeled decorative socket IDs exist.
8. 170 unique WIDs and eight budget groups match source.
9. All 340 proposed logical endpoint IDs mapped or explicitly reported.
10. 68 reused visual sockets reconciled; 35 mixed-kind cases separated.
11. W149 exact source `P072`, destination `P222` mapping matches source.
12. Reference 36 PNG maps imported without incorrect packed-channel handling.
13. Static/dynamic collider policy does not block gameplay controls.
14. Editor rerun idempotent; Levels 1–88 still load.

# 56. ACCEPTANCE TESTS — ELECTRICAL, INSTRUMENTS AND SAFETY (15–28)

15. Initial Level 89 has zero puzzle wires connected and 0/6 objectives.
16. 170 real player-created edges can be validated in the actual Spark solver.
17. Wrong-direction/wrong-type/shared-art-socket short attempts are rejected.
18. 5 V and returns measured correctly; no cross-domain electrical shorts.
19. Disconnecting/reconnecting any required wire modifies its actual component readiness.
20. Each detector reports calibrated range, timestamp and sample freshness.
21. Particle counters respond to actual modeled event and configured orientation.
22. External, habitat, trusted and independent dosimeter readings differ appropriately.
23. Saturated detector does not report falsely qualified low dose.
24. Stale/unpowered detector cannot keep silently refreshing normal measurements.
25. Independent protective dosimeter remains functional without W149 where physically powered.
26. W146 patch status does not substitute for W149 analog measured feedback.
27. Door/lock/E-stop/life-support interlocks enforce safe permissions.
28. SCADA indicators are sourced from authoritative state, never determine safety.

# 57. ACCEPTANCE TESTS — STORM, SHIELDS AND RESOURCES (29–44)

29. Quiet→rise→peak→decay physically updates storm energy bins independent of commands.
30. Forecast can differ from actual surge and report uncertainty.
31. Distinct energy bins have distinct configured educational transmission response.
32. Shelter physical exposure proxy changes with actual shield areal density and gaps.
33. Uncovered directions have poorer protection than actually shielded directions.
34. Four water tanks maintain independent bounded mass balances.
35. Total water conserved across reservoir, tanks, transfers and modeled losses.
36. Water for life support protected from shield-allocation overdraw.
37. Pump/valve response is finite, consumes modeled power and obeys state.
38. Four shield actuators move with finite speed and limits.
39. Jam freezes actual cassette, not merely its UI setpoint.
40. Actual encoder and lock feedback required for shield qualification.
41. Emergency door/egress protection behaves according to real interlocks.
42. Essential ventilation, power, oxygen and thermal continuity measured.
43. Integrated fictional sheltered dose accumulates over actual elapsed simulation time.
44. Full event evidence rejects skipped phase and all-loads-off shortcut.

# 58. ACCEPTANCE TESTS — THREE FAULTS, W149, OBJECTIVES (45–66)

45. Saturation fault changes detector readings and invalidates acquisition.
46. Detector repair/recalibration restores genuinely fresh channel evidence.
47. Jam fault alters physical pose, coverage, current/encoder diagnostics.
48. Jam repair actually restores motion and requalification.
49. High-energy surge alters actual storm-bin environment, not only forecast.
50. Surge recovery requires new resource/coverage evidence.
51. All three repairs logged with distinct cause and fix events.
52. Objective 5 requires a fresh complete storm after final repair.
53. W149 physical disconnect removes a real graph edge.
54. Trusted shelter measurement invalidates; underlying exposure remains physical.
55. Normal shelter opening and qualification permissions withdraw on W149 loss.
56. Independent safety and essential services stay active when physically feasible.
57. Fault latches; reset refused while W149 absent or stale.
58. Independent W132, W146 diagnostics and SCADA cannot bypass W149.
59. W149 reconnection requires correct source/destination functional ports.
60. New W149 sensor calibration and timestamp are acquired after repair.
61. Explicit reset accepted only after all safety prerequisites.
62. Historical Objectives 1–5 remain certified during deliberate W149 fault.
63. Objective 6 does not certify on reconnection or reset alone.
64. Brand-new post-W149 full storm and continuity/fictional-dose limits pass.
65. Real `LevelGamePlayManager` reaches 6/6, progression works.
66. Unity compilation/Play Mode and prior-level regression results honestly logged, without manufactured PASS statuses.

# 59. COMPLETE REQUIRED PLAYER GAMEPLAY WALKTHROUGH

**CONNECT ALL 170 REAL WIRES → RECONCILE 248 ART SOCKETS/340 LOGICAL CONTACTS → VALIDATE 5 V CONTROL/RETURNS → INDEPENDENT SAFETY SELF-TEST → CALIBRATE TWO TELESCOPES AND ENERGY-BIN SENSORS → CALIBRATE EXTERNAL/HABITAT/SHELTER/INDEPENDENT DOSIMETERS → VERIFY FRESH RANGE-QUALIFIED SAMPLES → FORECAST FICTIONAL STORM → CHECK WATER/POWER/LIFE-SUPPORT RESERVES → ALLOCATE FINITE WATER TO FOUR TANKS THROUGH ACTUAL PUMPS/VALVES → POSITION FOUR SHIELD CASSETTES → CONFIRM ENCODERS/LOCKS/DOOR → QUALIFY VENTILATION/OXYGEN/ESSENTIAL POWER → START FRESH QUIET→RISE→PEAK→DECAY EVENT → INTEGRATE ACTUAL SHELTER EXPOSURE → CERTIFY FIRST STORM → INJECT DETECTOR SATURATION → DIAGNOSE/CALIBRATE → INJECT SHIELD JAM → DIAGNOSE/REPAIR/REHOME → INJECT HIGH-ENERGY SURGE → REPLAN FINITE SHIELD RESOURCES → RUN FRESH COMPLETE STORM → HISTORICAL 5/6 → PHYSICALLY DISCONNECT W149 → TRUSTED SAMPLE INVALID/STORM CONTINUES → WITHDRAW NORMAL SAFETY PERMIT/LOCK PROTECTION/LATCH FAULT → PHYSICALLY RECONNECT W149 → ACQUIRE NEW CALIBRATED SAMPLES → CHECK INDEPENDENT SAFETY → EXPLICITLY RESET → RUN NEW QUIET→RISE→PEAK→DECAY STORM WITH FULL LIFE SUPPORT AND MEASURED EXPOSURE → ACTUAL LEVELGAMEPLAYMANAGER 6/6.**

# 60. DIAGNOSTICS, DELIVERABLES AND FINAL SUCCESS CONTRACT

Emit actionable diagnostics: missing/bad FBX, wrong cm scaling, duplicate pivot/socket, incompatible contact sharing at P222, missing 5 V return, wrong wire direction, disconnected W149, sample stale/overrange, invalid dosimeter calibration, sensor disagreement, invalid spectrum, predicted-vs-measured surge, water reserve insufficient, valve closed, pump stalled, panel jammed, lock unverified, shelter door open, air supply unavailable, power exhausted, exposure proxy over scenario budget, safety PLC inhibited, failed reset prerequisite, stale pre-repair storm evidence, objective stuck at 0/6 or 5/6 and earlier-level regression.

Deliver real Unity runtime C#, Editor builder, actual level asset and prefab, 92 art model bindings, 76 pivot mappings, 248 art sockets plus reconciled 340 contacts, 170 genuinely player-connectable wires, independent safety, fictional storm/energy bins, instrument calibration/acquisition, finite shielding water/motor states, exposure accounting, three recoverable disturbances, W149 full physical recovery, SCADA and evidence-based six-objective certification. Provide CSV/JSON QA reports and **exactly what was actually run**.

**Do not report “Level 89 complete” until actual Unity compilation, Play Mode validation and fresh full post-W149 storm show real `LevelGamePlayManager` progress = 6/6. Do not modify Levels 1–88.**

**PROJECT SPARK — LEVEL 89: THE HELIOS STORM. DETECT. SHIELD. PROTECT. SURVIVE.**

---

# SOURCE-DERIVED ANNEXES — EXACT PACKAGE RECORDS

> The annexes below are generated from the actual Level 89 source CSV and manifest files, not imagined assets. Original socket anchors, wire routing, logical ports, process-flow links and objective plans remain **unimplemented design records** until functional Unity integration and testing.

## ANNEX A — ALL 92 EXACT MODULAR FBX AND GLB FILENAMES

| # | FBX filename | GLB filename | Triangles | Mesh parts |
|---:|---|---|---:|---:|
| 00 | `00_L89_HeliosRadiationDefenseArenaBase.fbx` | `00_L89_HeliosRadiationDefenseArenaBase.glb` | 1,128 | 12 |
| 01 | `01_L89_SolarStormPanoramicObservatory.fbx` | `01_L89_SolarStormPanoramicObservatory.glb` | 1,668 | 13 |
| 02 | `02_L89_OrbitalStationHabitatExterior.fbx` | `02_L89_OrbitalStationHabitatExterior.glb` | 5,820 | 10 |
| 03 | `03_L89_StormShelterPressureHull.fbx` | `03_L89_StormShelterPressureHull.glb` | 4,260 | 9 |
| 04 | `04_L89_ShelterEntranceShieldedBulkhead.fbx` | `04_L89_ShelterEntranceShieldedBulkhead.glb` | 3,052 | 9 |
| 05 | `05_L89_SolarWeatherHologramDome.fbx` | `05_L89_SolarWeatherHologramDome.glb` | 7,072 | 8 |
| 06 | `06_L89_EarthAndHabitatObservationWindow.fbx` | `06_L89_EarthAndHabitatObservationWindow.glb` | 512 | 7 |
| 07 | `07_L89_StormShelterInteriorSupportDeck.fbx` | `07_L89_StormShelterInteriorSupportDeck.glb` | 444 | 7 |
| 08 | `08_L89_WaterWallStructuralContainmentFrame.fbx` | `08_L89_WaterWallStructuralContainmentFrame.glb` | 640 | 8 |
| 09 | `09_L89_ShelterEmergencyEgressHatch.fbx` | `09_L89_ShelterEmergencyEgressHatch.glb` | 1,768 | 7 |
| 10 | `10_L89_OuterExposureInstrumentPlatform.fbx` | `10_L89_OuterExposureInstrumentPlatform.glb` | 412 | 7 |
| 11 | `11_L89_StationShieldWaterUtilitySkid.fbx` | `11_L89_StationShieldWaterUtilitySkid.glb` | 1,676 | 7 |
| 12 | `12_L89_SolarEnergeticParticleTelescopeA.fbx` | `12_L89_SolarEnergeticParticleTelescopeA.glb` | 3,532 | 14 |
| 13 | `13_L89_SolarEnergeticParticleTelescopeB.fbx` | `13_L89_SolarEnergeticParticleTelescopeB.glb` | 3,532 | 14 |
| 14 | `14_L89_ChargedParticleSpectrometer.fbx` | `14_L89_ChargedParticleSpectrometer.glb` | 2,988 | 11 |
| 15 | `15_L89_ProtonEnergyBinAnalyzer.fbx` | `15_L89_ProtonEnergyBinAnalyzer.glb` | 2,988 | 12 |
| 16 | `16_L89_HeavyIonChannelDetector.fbx` | `16_L89_HeavyIonChannelDetector.glb` | 2,892 | 10 |
| 17 | `17_L89_SolarVectorTrackingGimbal.fbx` | `17_L89_SolarVectorTrackingGimbal.glb` | 4,236 | 15 |
| 18 | `18_L89_ParticleCollimatorHead.fbx` | `18_L89_ParticleCollimatorHead.glb` | 1,232 | 9 |
| 19 | `19_L89_PrimaryParticleCounter.fbx` | `19_L89_PrimaryParticleCounter.glb` | 1,476 | 17 |
| 20 | `20_L89_SecondaryParticleCounter.fbx` | `20_L89_SecondaryParticleCounter.glb` | 1,476 | 17 |
| 21 | `21_L89_DetectorPulseShaper.fbx` | `21_L89_DetectorPulseShaper.glb` | 1,004 | 14 |
| 22 | `22_L89_SpectralCalibrationSource.fbx` | `22_L89_SpectralCalibrationSource.glb` | 1,644 | 18 |
| 23 | `23_L89_ParticleDetectorThermalShroud.fbx` | `23_L89_ParticleDetectorThermalShroud.glb` | 1,004 | 14 |
| 24 | `24_L89_SpaceWeatherForecastConsole.fbx` | `24_L89_SpaceWeatherForecastConsole.glb` | 1,100 | 16 |
| 25 | `25_L89_StormArrivalPredictionHologram.fbx` | `25_L89_StormArrivalPredictionHologram.glb` | 3,680 | 8 |
| 26 | `26_L89_ExternalDosimeterArray.fbx` | `26_L89_ExternalDosimeterArray.glb` | 1,588 | 11 |
| 27 | `27_L89_HabitatDosimeterArray.fbx` | `27_L89_HabitatDosimeterArray.glb` | 1,588 | 11 |
| 28 | `28_L89_ShelterReferenceDosimeter.fbx` | `28_L89_ShelterReferenceDosimeter.glb` | 1,600 | 11 |
| 29 | `29_L89_IndependentShelterSafetyDosimeter.fbx` | `29_L89_IndependentShelterSafetyDosimeter.glb` | 1,588 | 11 |
| 30 | `30_L89_TriaxialRadiationDoseProbe.fbx` | `30_L89_TriaxialRadiationDoseProbe.glb` | 1,588 | 11 |
| 31 | `31_L89_DoseRateTransientRecorder.fbx` | `31_L89_DoseRateTransientRecorder.glb` | 1,100 | 16 |
| 32 | `32_L89_DosimeterCalibrationFrame.fbx` | `32_L89_DosimeterCalibrationFrame.glb` | 1,392 | 10 |
| 33 | `33_L89_DetectorSaturationFaultInjector.fbx` | `33_L89_DetectorSaturationFaultInjector.glb` | 1,112 | 16 |
| 34 | `34_L89_DoseAccumulationAnalyzer.fbx` | `34_L89_DoseAccumulationAnalyzer.glb` | 1,100 | 16 |
| 35 | `35_L89_DistributedDetectorNetworkGateway.fbx` | `35_L89_DistributedDetectorNetworkGateway.glb` | 1,196 | 17 |
| 36 | `36_L89_RadiationHeatmapHologram.fbx` | `36_L89_RadiationHeatmapHologram.glb` | 3,680 | 8 |
| 37 | `37_L89_DetectorRedundancyComparator.fbx` | `37_L89_DetectorRedundancyComparator.glb` | 1,100 | 16 |
| 38 | `38_L89_ShelterDoseLimitEvaluator.fbx` | `38_L89_ShelterDoseLimitEvaluator.glb` | 1,100 | 16 |
| 39 | `39_L89_WaterWallTankPortA.fbx` | `39_L89_WaterWallTankPortA.glb` | 612 | 12 |
| 40 | `40_L89_WaterWallTankPortB.fbx` | `40_L89_WaterWallTankPortB.glb` | 612 | 12 |
| 41 | `41_L89_WaterWallTankStarboardA.fbx` | `41_L89_WaterWallTankStarboardA.glb` | 612 | 12 |
| 42 | `42_L89_WaterWallTankStarboardB.fbx` | `42_L89_WaterWallTankStarboardB.glb` | 612 | 12 |
| 43 | `43_L89_WaterReservoirMain.fbx` | `43_L89_WaterReservoirMain.glb` | 2,640 | 11 |
| 44 | `44_L89_WaterTankLevelSensorArray.fbx` | `44_L89_WaterTankLevelSensorArray.glb` | 1,256 | 18 |
| 45 | `45_L89_WaterTransferPumpA.fbx` | `45_L89_WaterTransferPumpA.glb` | 1,680 | 12 |
| 46 | `46_L89_WaterTransferPumpB.fbx` | `46_L89_WaterTransferPumpB.glb` | 1,680 | 12 |
| 47 | `47_L89_WaterDistributionValveManifold.fbx` | `47_L89_WaterDistributionValveManifold.glb` | 4,968 | 13 |
| 48 | `48_L89_WaterMassAllocationController.fbx` | `48_L89_WaterMassAllocationController.glb` | 1,100 | 16 |
| 49 | `49_L89_ShieldArealDensityAnalyzer.fbx` | `49_L89_ShieldArealDensityAnalyzer.glb` | 1,100 | 16 |
| 50 | `50_L89_ThermalWaterResourceGuard.fbx` | `50_L89_ThermalWaterResourceGuard.glb` | 1,196 | 17 |
| 51 | `51_L89_WaterWallLeakMonitor.fbx` | `51_L89_WaterWallLeakMonitor.glb` | 1,004 | 14 |
| 52 | `52_L89_ShieldPanelTop.fbx` | `52_L89_ShieldPanelTop.glb` | 972 | 13 |
| 53 | `53_L89_ShieldPanelBottom.fbx` | `53_L89_ShieldPanelBottom.glb` | 972 | 13 |
| 54 | `54_L89_ShieldPanelPort.fbx` | `54_L89_ShieldPanelPort.glb` | 504 | 11 |
| 55 | `55_L89_ShieldPanelStarboard.fbx` | `55_L89_ShieldPanelStarboard.glb` | 504 | 11 |
| 56 | `56_L89_ShieldCassetteRailArray.fbx` | `56_L89_ShieldCassetteRailArray.glb` | 1,548 | 8 |
| 57 | `57_L89_ServoShieldActuatorA.fbx` | `57_L89_ServoShieldActuatorA.glb` | 988 | 12 |
| 58 | `58_L89_ServoShieldActuatorB.fbx` | `58_L89_ServoShieldActuatorB.glb` | 988 | 12 |
| 59 | `59_L89_ServoShieldActuatorC.fbx` | `59_L89_ServoShieldActuatorC.glb` | 988 | 12 |
| 60 | `60_L89_ServoShieldActuatorD.fbx` | `60_L89_ServoShieldActuatorD.glb` | 988 | 12 |
| 61 | `61_L89_ShieldLockingPinArray.fbx` | `61_L89_ShieldLockingPinArray.glb` | 3,436 | 11 |
| 62 | `62_L89_ShieldPositionEncoderRack.fbx` | `62_L89_ShieldPositionEncoderRack.glb` | 1,100 | 16 |
| 63 | `63_L89_ShieldMotorDriveCabinet.fbx` | `63_L89_ShieldMotorDriveCabinet.glb` | 1,196 | 17 |
| 64 | `64_L89_ShieldJammingFaultInjector.fbx` | `64_L89_ShieldJammingFaultInjector.glb` | 1,112 | 16 |
| 65 | `65_L89_ShelterDoorActualInterlock.fbx` | `65_L89_ShelterDoorActualInterlock.glb` | 1,112 | 16 |
| 66 | `66_L89_ShelterRadiationShieldQualityMonitor.fbx` | `66_L89_ShelterRadiationShieldQualityMonitor.glb` | 1,100 | 16 |
| 67 | `67_L89_ShieldCoverageDigitalTwin.fbx` | `67_L89_ShieldCoverageDigitalTwin.glb` | 3,680 | 8 |
| 68 | `68_L89_ShelterEnvironmentalAirSupply.fbx` | `68_L89_ShelterEnvironmentalAirSupply.glb` | 1,424 | 9 |
| 69 | `69_L89_ShelterOxygenBufferInterface.fbx` | `69_L89_ShelterOxygenBufferInterface.glb` | 2,616 | 11 |
| 70 | `70_L89_ShelterThermalAirReclaimer.fbx` | `70_L89_ShelterThermalAirReclaimer.glb` | 1,356 | 11 |
| 71 | `71_L89_ShelterProtectedLifeSupportPLC.fbx` | `71_L89_ShelterProtectedLifeSupportPLC.glb` | 1,196 | 17 |
| 72 | `72_L89_ShelterReservePowerDistribution.fbx` | `72_L89_ShelterReservePowerDistribution.glb` | 1,184 | 16 |
| 73 | `73_L89_WaterPowerPriorityScheduler.fbx` | `73_L89_WaterPowerPriorityScheduler.glb` | 1,184 | 16 |
| 74 | `74_L89_HabitatCrewShelterOccupancyEmulator.fbx` | `74_L89_HabitatCrewShelterOccupancyEmulator.glb` | 1,088 | 15 |
| 75 | `75_L89_StormIntensityProfileGenerator.fbx` | `75_L89_StormIntensityProfileGenerator.glb` | 1,100 | 16 |
| 76 | `76_L89_HighEnergySurgeFaultInjector.fbx` | `76_L89_HighEnergySurgeFaultInjector.glb` | 1,112 | 16 |
| 77 | `77_L89_ShieldingScenarioSafetyController.fbx` | `77_L89_ShieldingScenarioSafetyController.glb` | 1,184 | 16 |
| 78 | `78_L89_RadiationEmergencyIsolationStation.fbx` | `78_L89_RadiationEmergencyIsolationStation.glb` | 1,100 | 14 |
| 79 | `79_L89_Protected5VControlSupply.fbx` | `79_L89_Protected5VControlSupply.glb` | 1,184 | 16 |
| 80 | `80_L89_HeliosSafetyPLC.fbx` | `80_L89_HeliosSafetyPLC.glb` | 1,196 | 17 |
| 81 | `81_L89_HeliosMissionSequencingPLC.fbx` | `81_L89_HeliosMissionSequencingPLC.glb` | 1,184 | 16 |
| 82 | `82_L89_RadiationDefenseSCADA.fbx` | `82_L89_RadiationDefenseSCADA.glb` | 3,544 | 12 |
| 83 | `83_L89_SolarStorm3DParticleHologram.fbx` | `83_L89_SolarStorm3DParticleHologram.glb` | 3,584 | 6 |
| 84 | `84_L89_ExteriorParticleFieldEmitter.fbx` | `84_L89_ExteriorParticleFieldEmitter.glb` | 1,536 | 6 |
| 85 | `85_L89_ShelterRadiationContingencyConsole.fbx` | `85_L89_ShelterRadiationContingencyConsole.glb` | 1,088 | 15 |
| 86 | `86_L89_W149TrustedDoseFeedbackPatchStation.fbx` | `86_L89_W149TrustedDoseFeedbackPatchStation.glb` | 1,304 | 15 |
| 87 | `87_L89_RadiationScenarioDiagnosticRecorder.fbx` | `87_L89_RadiationScenarioDiagnosticRecorder.glb` | 1,088 | 15 |
| 88 | `88_L89_StormShelterCertificationBeacon.fbx` | `88_L89_StormShelterCertificationBeacon.glb` | 1,964 | 7 |
| 89 | `89_L89_EmergencyShelterCommunications.fbx` | `89_L89_EmergencyShelterCommunications.glb` | 992 | 13 |
| 90 | `90_L89_EquipmentMaintenanceInspectionCamera.fbx` | `90_L89_EquipmentMaintenanceInspectionCamera.glb` | 476 | 7 |
| 91 | `91_L89_SolarStormSurveyAntennaArray.fbx` | `91_L89_SolarStormSurveyAntennaArray.glb` | 1,532 | 8 |

**Modular totals: 156,444 triangles and 1,154 mesh parts.** Alternative assembled preview: `Models/Level89_Complete_ArtKit_Showcase.fbx`, `GLB/Level89_Complete_ArtKit_Showcase.glb` (not to be instantiated with modular set).

## ANNEX B — ALL 76 SOURCE ANIMATION PIVOT PARENTS

Exact pivot, owner, kind, axis and world XYZ metres. `indicator` means art display anchor and is not a physical actuator.

| # | Pivot | Owner model | Kind | Axis | X m | Y m | Z m |
|---:|---|---|---|:---:|---:|---:|---:|
| 1 | `ANIM_SHELTER_MAIN_SHIELD_DOOR` | `L89_ShelterEntranceShieldedBulkhead` | translation | `y` | 0 | 3.8 | -1.15 |
| 2 | `ANIM_STORM_FORECAST_HOLOGRAM` | `L89_SolarWeatherHologramDome` | indicator | `none` | -6.3 | 9.7 | -12.4 |
| 3 | `ANIM_EMERGENCY_EGRESS_HATCH` | `L89_ShelterEmergencyEgressHatch` | rotation | `y` | 0 | 3.8 | -6.1 |
| 4 | `ANIM_TELESCOPE_A_YAW` | `L89_SolarEnergeticParticleTelescopeA` | rotation | `y` | -11.1 | 5.1 | -7.3 |
| 5 | `ANIM_TELESCOPE_A_ELEVATION` | `L89_SolarEnergeticParticleTelescopeA` | rotation | `x` | -11.1 | 5.949999999999999 | -7.3 |
| 6 | `ANIM_TELESCOPE_B_YAW` | `L89_SolarEnergeticParticleTelescopeB` | rotation | `y` | -8.9 | 5.1 | -7.3 |
| 7 | `ANIM_TELESCOPE_B_ELEVATION` | `L89_SolarEnergeticParticleTelescopeB` | rotation | `x` | -8.9 | 5.949999999999999 | -7.3 |
| 8 | `ANIM_SPECTROMETER_DETECTOR_SCAN` | `L89_ChargedParticleSpectrometer` | translation | `x` | -12.2 | 2.3 | -4.5 |
| 9 | `ANIM_PROTON_ENERGY_SELECTOR` | `L89_ProtonEnergyBinAnalyzer` | rotation | `z` | -10.4 | 2.3 | -4.5 |
| 10 | `ANIM_HEAVY_ION_CHANNEL_SCAN` | `L89_HeavyIonChannelDetector` | translation | `y` | -8.55 | 2.3 | -4.5 |
| 11 | `ANIM_SUN_TRACKER_AZIMUTH` | `L89_SolarVectorTrackingGimbal` | rotation | `y` | -9.8 | 5.3 | -8.3 |
| 12 | `ANIM_SUN_TRACKER_ELEVATION` | `L89_SolarVectorTrackingGimbal` | rotation | `x` | -9.8 | 5.83 | -8.3 |
| 13 | `ANIM_PARTICLE_COLLIMATOR_APERTURE` | `L89_ParticleCollimatorHead` | rotation | `z` | -11 | 4.5 | -6.6 |
| 14 | `ANIM_PRIMARY_PARTICLE_PULSE` | `L89_PrimaryParticleCounter` | indicator | `none` | -10.5 | 1.85 | -3.3 |
| 15 | `ANIM_SECONDARY_PARTICLE_PULSE` | `L89_SecondaryParticleCounter` | indicator | `none` | -8.9 | 1.85 | -3.3 |
| 16 | `ANIM_DETECTOR_PULSE_CONDITIONING` | `L89_DetectorPulseShaper` | indicator | `none` | -13.0 | 1.85 | 2.0 |
| 17 | `ANIM_SPECTRAL_CALIBRATION_LAMP` | `L89_SpectralCalibrationSource` | indicator | `none` | -11.56 | 1.85 | 2.0 |
| 18 | `ANIM_DETECTOR_THERMAL_SHROUD` | `L89_ParticleDetectorThermalShroud` | rotation | `y` | -10.120000000000001 | 1.85 | 2.0 |
| 19 | `ANIM_STORM_FORECAST_CONSOLE` | `L89_SpaceWeatherForecastConsole` | indicator | `none` | -8.68 | 1.85 | 2.0 |
| 20 | `ANIM_STORM_ARRIVAL_TIMELINE` | `L89_StormArrivalPredictionHologram` | indicator | `none` | -7.24 | 1.85 | 2.0 |
| 21 | `ANIM_EXTERNAL_DOSIMETER_SCAN` | `L89_ExternalDosimeterArray` | indicator | `none` | -6.0 | 4.1 | -6.6 |
| 22 | `ANIM_HABITAT_DOSIMETER_SCAN` | `L89_HabitatDosimeterArray` | indicator | `none` | -4.5 | 4.1 | -6.6 |
| 23 | `ANIM_SHELTER_REFERENCE_DOSE` | `L89_ShelterReferenceDosimeter` | indicator | `none` | -3.2 | 3.4 | -2.0 |
| 24 | `ANIM_INDEPENDENT_DOSE_MONITOR` | `L89_IndependentShelterSafetyDosimeter` | indicator | `none` | 3.2 | 3.4 | -2.0 |
| 25 | `ANIM_TRIAXIAL_DOSE_PROBE` | `L89_TriaxialRadiationDoseProbe` | rotation | `y` | -5.4 | 1.8 | -3.3 |
| 26 | `ANIM_DOSE_TRANSIENT_RECORDER` | `L89_DoseRateTransientRecorder` | indicator | `none` | -5.800000000000001 | 1.85 | 2.0 |
| 27 | `ANIM_DOSIMETER_CALIBRATION_FRAME` | `L89_DosimeterCalibrationFrame` | translation | `x` | -7.2 | 1.9 | -3.2 |
| 28 | `ANIM_DETECTOR_SATURATION_TEST` | `L89_DetectorSaturationFaultInjector` | translation | `x` | -4.359999999999999 | 1.85 | 2.0 |
| 29 | `ANIM_ACCUMULATED_DOSE_DISPLAY` | `L89_DoseAccumulationAnalyzer` | indicator | `none` | -2.92 | 1.85 | 2.0 |
| 30 | `ANIM_DETECTOR_NETWORK_GATEWAY` | `L89_DistributedDetectorNetworkGateway` | indicator | `none` | -1.4800000000000004 | 1.85 | 2.0 |
| 31 | `ANIM_RADIATION_HEATMAP_HOLOGRAM` | `L89_RadiationHeatmapHologram` | indicator | `none` | -5.9 | 7.4 | -10.0 |
| 32 | `ANIM_DETECTOR_COMPARATOR` | `L89_DetectorRedundancyComparator` | indicator | `none` | -0.040000000000000924 | 1.85 | 2.0 |
| 33 | `ANIM_DOSE_LIMIT_EVALUATOR` | `L89_ShelterDoseLimitEvaluator` | indicator | `none` | 1.3999999999999986 | 1.85 | 2.0 |
| 34 | `ANIM_WATERWALL_PORT_A_LEVEL` | `L89_WaterWallTankPortA` | indicator | `none` | -2.15 | 3.8 | -3.3 |
| 35 | `ANIM_WATERWALL_PORT_B_LEVEL` | `L89_WaterWallTankPortB` | indicator | `none` | -1.1 | 3.8 | -3.3 |
| 36 | `ANIM_WATERWALL_STARBOARD_A_LEVEL` | `L89_WaterWallTankStarboardA` | indicator | `none` | 1.1 | 3.8 | -3.3 |
| 37 | `ANIM_WATERWALL_STARBOARD_B_LEVEL` | `L89_WaterWallTankStarboardB` | indicator | `none` | 2.15 | 3.8 | -3.3 |
| 38 | `ANIM_MAIN_WATER_TANK_LEVEL` | `L89_WaterReservoirMain` | indicator | `none` | 9.0 | 2.4 | -5.5 |
| 39 | `ANIM_SHIELD_WATER_SENSOR_SCAN` | `L89_WaterTankLevelSensorArray` | translation | `y` | 10.5 | 3.35 | -4.4 |
| 40 | `ANIM_WATER_PUMP_A_ROTOR` | `L89_WaterTransferPumpA` | rotation | `z` | 10.6 | 1.75 | -2.65 |
| 41 | `ANIM_WATER_PUMP_B_ROTOR` | `L89_WaterTransferPumpB` | rotation | `z` | 12.2 | 1.75 | -2.65 |
| 42 | `ANIM_SHIELD_WATER_DISTRIBUTOR` | `L89_WaterDistributionValveManifold` | rotation | `y` | 11.1 | 1.6 | -1.1 |
| 43 | `ANIM_SHIELD_MASS_ALLOCATION` | `L89_WaterMassAllocationController` | indicator | `none` | -12.2 | 1.75 | 3.92 |
| 44 | `ANIM_AREAL_DENSITY_HEATMAP` | `L89_ShieldArealDensityAnalyzer` | indicator | `none` | -10.07 | 1.75 | 3.92 |
| 45 | `ANIM_WATER_RESOURCE_GUARD` | `L89_ThermalWaterResourceGuard` | indicator | `none` | -7.9399999999999995 | 1.75 | 3.92 |
| 46 | `ANIM_WATER_WALL_LEAK_MONITOR` | `L89_WaterWallLeakMonitor` | indicator | `none` | -5.81 | 1.75 | 3.92 |
| 47 | `ANIM_SHIELD_CASSETTE_TOP` | `L89_ShieldPanelTop` | translation | `y` | 0 | 6.8 | -3.3 |
| 48 | `ANIM_SHIELD_CASSETTE_BOTTOM` | `L89_ShieldPanelBottom` | translation | `y` | 0 | 1.0 | -3.3 |
| 49 | `ANIM_SHIELD_CASSETTE_PORT` | `L89_ShieldPanelPort` | translation | `x` | -3.35 | 3.8 | -3.3 |
| 50 | `ANIM_SHIELD_CASSETTE_STARBOARD` | `L89_ShieldPanelStarboard` | translation | `x` | 3.35 | 3.8 | -3.3 |
| 51 | `ANIM_SHIELD_CARRIAGE_LOCK` | `L89_ShieldCassetteRailArray` | translation | `z` | 0 | 3.85 | -3.2 |
| 52 | `ANIM_SHIELD_ACTUATOR_A` | `L89_ServoShieldActuatorA` | translation | `x` | -3.0 | 2.0 | -1.0 |
| 53 | `ANIM_SHIELD_ACTUATOR_B` | `L89_ServoShieldActuatorB` | translation | `x` | -1.0 | 2.0 | -1.0 |
| 54 | `ANIM_SHIELD_ACTUATOR_C` | `L89_ServoShieldActuatorC` | translation | `x` | 1.0 | 2.0 | -1.0 |
| 55 | `ANIM_SHIELD_ACTUATOR_D` | `L89_ServoShieldActuatorD` | translation | `x` | 3.0 | 2.0 | -1.0 |
| 56 | `ANIM_LOCKING_PIN_A` | `L89_ShieldLockingPinArray` | translation | `z` | -0.6 | 6.45 | -0.98 |
| 57 | `ANIM_LOCKING_PIN_B` | `L89_ShieldLockingPinArray` | translation | `z` | 0.6 | 6.45 | -0.98 |
| 58 | `ANIM_SHIELD_POSITION_FEEDBACK` | `L89_ShieldPositionEncoderRack` | indicator | `none` | -3.6799999999999997 | 1.75 | 3.92 |
| 59 | `ANIM_SHIELD_MOTOR_DRIVE` | `L89_ShieldMotorDriveCabinet` | indicator | `none` | -1.5500000000000007 | 1.75 | 3.92 |
| 60 | `ANIM_SHIELD_JAM_FAULT` | `L89_ShieldJammingFaultInjector` | translation | `x` | 0.5800000000000001 | 1.75 | 3.92 |
| 61 | `ANIM_SHELTER_DOOR_INTERLOCK` | `L89_ShelterDoorActualInterlock` | rotation | `x` | 1.6 | 2.0 | -0.85 |
| 62 | `ANIM_SHELTER_SHIELD_QUALITY` | `L89_ShelterRadiationShieldQualityMonitor` | indicator | `none` | 0 | 7.3 | -3.3 |
| 63 | `ANIM_SHIELD_COVERAGE_TWIN` | `L89_ShieldCoverageDigitalTwin` | indicator | `none` | 0 | 8.45 | -10.1 |
| 64 | `ANIM_SHELTER_AIR_BLOWER` | `L89_ShelterEnvironmentalAirSupply` | rotation | `z` | 5.1 | 2.2 | -5.3 |
| 65 | `ANIM_OXYGEN_BUFFER_STATUS` | `L89_ShelterOxygenBufferInterface` | indicator | `none` | 6.9 | 2.2 | -5.3 |
| 66 | `ANIM_THERMAL_RECLAIMER_VALVE` | `L89_ShelterThermalAirReclaimer` | rotation | `y` | 8.5 | 2.2 | -5.3 |
| 67 | `ANIM_SHELTER_LIFE_SUPPORT_PLC` | `L89_ShelterProtectedLifeSupportPLC` | indicator | `none` | 2.710000000000001 | 1.75 | 3.92 |
| 68 | `ANIM_SIMULATED_SOLAR_STORM_INTENSITY` | `L89_StormIntensityProfileGenerator` | indicator | `none` | -6.4 | 8.0 | -10.5 |
| 69 | `ANIM_ENERGY_SURGE_FAULT` | `L89_HighEnergySurgeFaultInjector` | translation | `x` | 9.099999999999998 | 1.75 | 3.92 |
| 70 | `ANIM_EMERGENCY_ISOLATION_LEVER` | `L89_RadiationEmergencyIsolationStation` | rotation | `x` | 12.8 | 1.65 | 5.2 |
| 71 | `ANIM_RADIATION_SAFETY_PLC` | `L89_HeliosSafetyPLC` | indicator | `none` | 7.9 | 1.8 | 5.4 |
| 72 | `ANIM_RADIATION_DEFENSE_SCADA` | `L89_RadiationDefenseSCADA` | indicator | `none` | 0 | 8.5 | -8.9 |
| 73 | `ANIM_SOLAR_STORM_3D_HOLOGRAM` | `L89_SolarStorm3DParticleHologram` | indicator | `none` | 0 | 11.2 | -11.1 |
| 74 | `ANIM_W149_CRITICAL_FEEDBACK_PATCH` | `L89_W149TrustedDoseFeedbackPatchStation` | indicator | `none` | -6.4 | 1.7 | 5.4 |
| 75 | `ANIM_FINAL_CERTIFICATION` | `L89_StormShelterCertificationBeacon` | indicator | `none` | 0 | 12.4 | -9.9 |
| 76 | `ANIM_SURVEY_ANTENNA_AZIMUTH` | `L89_SolarStormSurveyAntennaArray` | rotation | `y` | -13.3 | 5.3 | -7.7 |

## ANNEX C — ALL 170 EXACT CONCEPTUAL PLAYER-WIRE ROUTES

Source `State=CONCEPT_ONLY_PLAYER_NOT_CONNECTED`: none are working connected Unity wires yet. The runtime builder must preserve exact WIDs, model owners, art anchors and source signal classifications, then validate real electrical semantics and actual player connection.

| Wire | Source model (socket) | Target model (socket) | Signal kind | Original purpose | Critical |
|---|---|---|---|---|:---:|
| **W001** | `L89_Protected5VControlSupply` (`P218`) | `L89_HeliosSafetyPLC` (`P221`) | `POWER_CONTROL` | Protected 5 V radiation safety PLC supply | NO |
| **W002** | `L89_Protected5VControlSupply` (`P219`) | `L89_HeliosMissionSequencingPLC` (`P225`) | `POWER_CONTROL` | Protected 5 V sequencing PLC supply | NO |
| **W003** | `L89_Protected5VControlSupply` (`P217`) | `L89_DistributedDetectorNetworkGateway` (`P091`) | `POWER_CONTROL` | Radiation detector network logic supply | NO |
| **W004** | `L89_Protected5VControlSupply` (`P218`) | `L89_ShieldMotorDriveCabinet` (`P173`) | `POWER_CONTROL` | Motor-shield cabinet control supply | NO |
| **W005** | `L89_Protected5VControlSupply` (`P219`) | `L89_ShelterProtectedLifeSupportPLC` (`P196`) | `POWER_CONTROL` | Shelter ventilation safety control supply | NO |
| **W006** | `L89_Protected5VControlSupply` (`P217`) | `L89_RadiationDefenseSCADA` (`P226`) | `POWER_CONTROL` | SCADA logic supply | NO |
| **W007** | `L89_Protected5VControlSupply` (`P218`) | `L89_ShelterReferenceDosimeter` (`P071`) | `POWER_CONTROL` | Primary shelter dosimeter signal conditioning | NO |
| **W008** | `L89_Protected5VControlSupply` (`P219`) | `L89_IndependentShelterSafetyDosimeter` (`P075`) | `POWER_CONTROL` | Independent shelter dosimeter conditioning | NO |
| **W009** | `L89_Protected5VControlSupply` (`P217`) | `L89_WaterMassAllocationController` (`P129`) | `POWER_CONTROL` | Water-shield distribution controller power | NO |
| **W010** | `L89_Protected5VControlSupply` (`P218`) | `L89_SpaceWeatherForecastConsole` (`P059`) | `POWER_CONTROL` | Forecast instrument control power | NO |
| **W011** | `L89_HeliosSafetyPLC` (`P222`) | `L89_Protected5VControlSupply` (`P219`) | `POWER_CONTROL` | Safety PLC protected control return | NO |
| **W012** | `L89_HeliosMissionSequencingPLC` (`P223`) | `L89_Protected5VControlSupply` (`P217`) | `POWER_CONTROL` | Mission PLC protected control return | NO |
| **W013** | `L89_SolarEnergeticParticleTelescopeA` (`P026`) | `L89_DistributedDetectorNetworkGateway` (`P092`) | `ANALOG_FEEDBACK` | Primary telescope measured particle count | NO |
| **W014** | `L89_SolarEnergeticParticleTelescopeB` (`P030`) | `L89_DistributedDetectorNetworkGateway` (`P093`) | `ANALOG_FEEDBACK` | Redundant telescope measured particle count | NO |
| **W015** | `L89_ChargedParticleSpectrometer` (`P031`) | `L89_ProtonEnergyBinAnalyzer` (`P034`) | `ANALOG_FEEDBACK` | Charged particle spectrometer energy channel | NO |
| **W016** | `L89_ProtonEnergyBinAnalyzer` (`P035`) | `L89_DistributedDetectorNetworkGateway` (`P092`) | `ANALOG_FEEDBACK` | Proton energy-bin spectral feedback | NO |
| **W017** | `L89_HeavyIonChannelDetector` (`P038`) | `L89_DistributedDetectorNetworkGateway` (`P093`) | `ANALOG_FEEDBACK` | Heavy-ion distribution health | NO |
| **W018** | `L89_SolarVectorTrackingGimbal` (`P039`) | `L89_HeliosMissionSequencingPLC` (`P223`) | `ANALOG_FEEDBACK` | Sun tracker measured actual azimuth | NO |
| **W019** | `L89_ParticleCollimatorHead` (`P043`) | `L89_ChargedParticleSpectrometer` (`P032`) | `ANALOG_FEEDBACK` | Collimator aperture measured state | NO |
| **W020** | `L89_PrimaryParticleCounter` (`P047`) | `L89_DistributedDetectorNetworkGateway` (`P093`) | `ANALOG_FEEDBACK` | Primary particle-counter histogram | NO |
| **W021** | `L89_SecondaryParticleCounter` (`P048`) | `L89_DistributedDetectorNetworkGateway` (`P091`) | `ANALOG_FEEDBACK` | Secondary particle-counter histogram | NO |
| **W022** | `L89_DetectorPulseShaper` (`P051`) | `L89_DistributedDetectorNetworkGateway` (`P092`) | `ANALOG_FEEDBACK` | Pulse shaping instrument actual signal quality | NO |
| **W023** | `L89_SpectralCalibrationSource` (`P055`) | `L89_DistributedDetectorNetworkGateway` (`P093`) | `ANALOG_FEEDBACK` | Calibrator spectral reference | NO |
| **W024** | `L89_ParticleDetectorThermalShroud` (`P056`) | `L89_DistributedDetectorNetworkGateway` (`P091`) | `ANALOG_FEEDBACK` | Detector thermal shroud sensed state | NO |
| **W025** | `L89_DistributedDetectorNetworkGateway` (`P092`) | `L89_SpaceWeatherForecastConsole` (`P059`) | `ANALOG_FEEDBACK` | Aggregated measured solar particle flux | NO |
| **W026** | `L89_SpaceWeatherForecastConsole` (`P060`) | `L89_HeliosMissionSequencingPLC` (`P225`) | `ANALOG_FEEDBACK` | Qualified space-weather forecasting diagnostic | NO |
| **W027** | `L89_StormArrivalPredictionHologram` (`P061`) | `L89_RadiationDefenseSCADA` (`P226`) | `ANALOG_FEEDBACK` | Forecast-time visualization telemetry | NO |
| **W028** | `L89_DetectorSaturationFaultInjector` (`P086`) | `L89_DistributedDetectorNetworkGateway` (`P092`) | `ANALOG_FEEDBACK` | Detector overrange diagnostic | NO |
| **W029** | `L89_HeliosMissionSequencingPLC` (`P225`) | `L89_SolarVectorTrackingGimbal` (`P041`) | `DIGITAL_CONTROL` | Solar gimbal tracking setpoint | NO |
| **W030** | `L89_SolarVectorTrackingGimbal` (`P039`) | `L89_ParticleCollimatorHead` (`P042`) | `ANALOG_FEEDBACK` | Measured tracking collimator alignment | NO |
| **W031** | `L89_HeliosMissionSequencingPLC` (`P224`) | `L89_SolarEnergeticParticleTelescopeA` (`P026`) | `DIGITAL_CONTROL` | Enable primary solar particle telescope | NO |
| **W032** | `L89_HeliosMissionSequencingPLC` (`P225`) | `L89_SolarEnergeticParticleTelescopeB` (`P030`) | `DIGITAL_CONTROL` | Enable redundant solar particle telescope | NO |
| **W033** | `L89_HeliosMissionSequencingPLC` (`P223`) | `L89_SpectralCalibrationSource` (`P053`) | `DIGITAL_CONTROL` | Request calibration reference | NO |
| **W034** | `L89_HeliosMissionSequencingPLC` (`P224`) | `L89_PrimaryParticleCounter` (`P046`) | `DIGITAL_CONTROL` | Primary counter acquisition gate | NO |
| **W035** | `L89_HeliosMissionSequencingPLC` (`P225`) | `L89_SecondaryParticleCounter` (`P050`) | `DIGITAL_CONTROL` | Secondary counter acquisition gate | NO |
| **W036** | `L89_DistributedDetectorNetworkGateway` (`P091`) | `L89_DetectorRedundancyComparator` (`P097`) | `ANALOG_FEEDBACK` | Redundant particle-counter consistency | NO |
| **W037** | `L89_DetectorRedundancyComparator` (`P098`) | `L89_HeliosMissionSequencingPLC` (`P224`) | `ANALOG_FEEDBACK` | Measurement health and confidence qualification | NO |
| **W038** | `L89_DistributedDetectorNetworkGateway` (`P093`) | `L89_SolarStorm3DParticleHologram` (`P229`) | `ANALOG_FEEDBACK` | Measured space-weather visualization only | NO |
| **W039** | `L89_ExternalDosimeterArray` (`P064`) | `L89_DistributedDetectorNetworkGateway` (`P091`) | `ANALOG_FEEDBACK` | Exterior dosimetry measured rate | NO |
| **W040** | `L89_HabitatDosimeterArray` (`P068`) | `L89_DistributedDetectorNetworkGateway` (`P092`) | `ANALOG_FEEDBACK` | Habitat compartment measured dose-rate | NO |
| **W041** | `L89_ShelterReferenceDosimeter` (`P072`) | `L89_HeliosSafetyPLC` (`P222`) | `ANALOG_FEEDBACK` | Shelter local non-authoritative commissioning measurement | NO |
| **W042** | `L89_IndependentShelterSafetyDosimeter` (`P073`) | `L89_HeliosSafetyPLC` (`P220`) | `ANALOG_FEEDBACK` | Independent shelter radiation safety crosscheck | NO |
| **W043** | `L89_TriaxialRadiationDoseProbe` (`P077`) | `L89_DistributedDetectorNetworkGateway` (`P092`) | `ANALOG_FEEDBACK` | Triaxial reference instrument measured field proxy | NO |
| **W044** | `L89_DoseRateTransientRecorder` (`P081`) | `L89_DistributedDetectorNetworkGateway` (`P093`) | `ANALOG_FEEDBACK` | Dose-rate transient recorder | NO |
| **W045** | `L89_DosimeterCalibrationFrame` (`P082`) | `L89_ShelterReferenceDosimeter` (`P070`) | `ANALOG_FEEDBACK` | Dosimeter fictional calibration reference | NO |
| **W046** | `L89_DosimeterCalibrationFrame` (`P083`) | `L89_IndependentShelterSafetyDosimeter` (`P074`) | `ANALOG_FEEDBACK` | Independent dosimeter calibration reference | NO |
| **W047** | `L89_DoseAccumulationAnalyzer` (`P090`) | `L89_ShelterDoseLimitEvaluator` (`P102`) | `ANALOG_FEEDBACK` | Run-scoped accumulated shelter dose | NO |
| **W048** | `L89_DistributedDetectorNetworkGateway` (`P091`) | `L89_DetectorRedundancyComparator` (`P097`) | `ANALOG_FEEDBACK` | Spectral acquisition repeatability | NO |
| **W049** | `L89_DetectorRedundancyComparator` (`P098`) | `L89_ShelterDoseLimitEvaluator` (`P101`) | `ANALOG_FEEDBACK` | Validated instrument-fusion confidence | NO |
| **W050** | `L89_ShelterDoseLimitEvaluator` (`P102`) | `L89_HeliosSafetyPLC` (`P222`) | `ANALOG_FEEDBACK` | Dose-budget exceedance risk to protection | NO |
| **W051** | `L89_ExternalDosimeterArray` (`P064`) | `L89_RadiationHeatmapHologram` (`P094`) | `ANALOG_FEEDBACK` | External dose-area heatmap visualization | NO |
| **W052** | `L89_HabitatDosimeterArray` (`P068`) | `L89_RadiationHeatmapHologram` (`P095`) | `ANALOG_FEEDBACK` | Habitat dose-area heatmap visualization | NO |
| **W053** | `L89_ShelterReferenceDosimeter` (`P072`) | `L89_RadiationDefenseSCADA` (`P228`) | `ANALOG_FEEDBACK` | Display-only shelter dose telemetry | NO |
| **W054** | `L89_IndependentShelterSafetyDosimeter` (`P073`) | `L89_RadiationDefenseSCADA` (`P226`) | `ANALOG_FEEDBACK` | Display-only independent shelter reading | NO |
| **W055** | `L89_HeliosMissionSequencingPLC` (`P224`) | `L89_DosimeterCalibrationFrame` (`P083`) | `DIGITAL_CONTROL` | Initiate dosimeter calibration | NO |
| **W056** | `L89_HeliosMissionSequencingPLC` (`P225`) | `L89_DoseRateTransientRecorder` (`P081`) | `DIGITAL_CONTROL` | Capture transient radiation trace | NO |
| **W057** | `L89_HeliosMissionSequencingPLC` (`P223`) | `L89_TriaxialRadiationDoseProbe` (`P076`) | `DIGITAL_CONTROL` | Select triaxial dosimeter scan mode | NO |
| **W058** | `L89_DistributedDetectorNetworkGateway` (`P092`) | `L89_DoseAccumulationAnalyzer` (`P089`) | `ANALOG_FEEDBACK` | Validated dose-rate integration samples | NO |
| **W059** | `L89_DoseAccumulationAnalyzer` (`P090`) | `L89_RadiationDefenseSCADA` (`P228`) | `ANALOG_FEEDBACK` | Accumulated mission exposure visualization | NO |
| **W060** | `L89_ShelterDoseLimitEvaluator` (`P100`) | `L89_HeliosMissionSequencingPLC` (`P223`) | `ANALOG_FEEDBACK` | Qualified shelter dose budget assessment | NO |
| **W061** | `L89_DetectorSaturationFaultInjector` (`P086`) | `L89_HeliosMissionSequencingPLC` (`P224`) | `ANALOG_FEEDBACK` | Detector overload fault report | NO |
| **W062** | `L89_HeliosMissionSequencingPLC` (`P225`) | `L89_DetectorSaturationFaultInjector` (`P087`) | `DIGITAL_CONTROL` | Activate deterministic detector saturation test | NO |
| **W063** | `L89_WaterMassAllocationController` (`P129`) | `L89_WaterWallTankPortA` (`P103`) | `DIGITAL_CONTROL` | Port A tank shield allocation command | NO |
| **W064** | `L89_WaterMassAllocationController` (`P130`) | `L89_WaterWallTankPortB` (`P107`) | `DIGITAL_CONTROL` | Port B tank shield allocation command | NO |
| **W065** | `L89_WaterMassAllocationController` (`P131`) | `L89_WaterWallTankStarboardA` (`P111`) | `DIGITAL_CONTROL` | Starboard A tank shield allocation command | NO |
| **W066** | `L89_WaterMassAllocationController` (`P129`) | `L89_WaterWallTankStarboardB` (`P112`) | `DIGITAL_CONTROL` | Starboard B tank shield allocation command | NO |
| **W067** | `L89_WaterTankLevelSensorArray` (`P118`) | `L89_WaterMassAllocationController` (`P130`) | `ANALOG_FEEDBACK` | Actual compartment water-level distribution | NO |
| **W068** | `L89_WaterReservoirMain` (`P115`) | `L89_WaterTankLevelSensorArray` (`P119`) | `ANALOG_FEEDBACK` | Actual main water inventory | NO |
| **W069** | `L89_WaterMassAllocationController` (`P129`) | `L89_WaterTransferPumpA` (`P120`) | `DIGITAL_CONTROL` | Port water transfer pump start | NO |
| **W070** | `L89_WaterMassAllocationController` (`P130`) | `L89_WaterTransferPumpB` (`P124`) | `DIGITAL_CONTROL` | Starboard water transfer pump start | NO |
| **W071** | `L89_WaterTransferPumpA` (`P122`) | `L89_WaterMassAllocationController` (`P131`) | `ANALOG_FEEDBACK` | Measured transfer pump A flow | NO |
| **W072** | `L89_WaterTransferPumpB` (`P123`) | `L89_WaterMassAllocationController` (`P129`) | `ANALOG_FEEDBACK` | Measured transfer pump B flow | NO |
| **W073** | `L89_WaterMassAllocationController` (`P130`) | `L89_WaterDistributionValveManifold` (`P127`) | `DIGITAL_CONTROL` | Water distributor valve requested position | NO |
| **W074** | `L89_WaterDistributionValveManifold` (`P128`) | `L89_WaterMassAllocationController` (`P131`) | `ANALOG_FEEDBACK` | Distributor actual valve position | NO |
| **W075** | `L89_WaterWallTankPortA` (`P103`) | `L89_ShieldArealDensityAnalyzer` (`P132`) | `ANALOG_FEEDBACK` | Port A measured shielding areal density | NO |
| **W076** | `L89_WaterWallTankPortB` (`P107`) | `L89_ShieldArealDensityAnalyzer` (`P133`) | `ANALOG_FEEDBACK` | Port B measured shielding areal density | NO |
| **W077** | `L89_WaterWallTankStarboardA` (`P111`) | `L89_ShieldArealDensityAnalyzer` (`P134`) | `ANALOG_FEEDBACK` | Starboard A measured shielding areal density | NO |
| **W078** | `L89_WaterWallTankStarboardB` (`P112`) | `L89_ShieldArealDensityAnalyzer` (`P132`) | `ANALOG_FEEDBACK` | Starboard B measured shielding areal density | NO |
| **W079** | `L89_ShieldArealDensityAnalyzer` (`P133`) | `L89_ShieldingScenarioSafetyController` (`P213`) | `ANALOG_FEEDBACK` | Actual mass-cover protection evaluation | NO |
| **W080** | `L89_ThermalWaterResourceGuard` (`P137`) | `L89_WaterMassAllocationController` (`P131`) | `SAFETY_FEEDBACK` | Water / life support budget permit | NO |
| **W081** | `L89_WaterWallLeakMonitor` (`P139`) | `L89_WaterMassAllocationController` (`P129`) | `ANALOG_FEEDBACK` | Water-wall leak inspection | NO |
| **W082** | `L89_ShelterProtectedLifeSupportPLC` (`P195`) | `L89_ThermalWaterResourceGuard` (`P136`) | `SAFETY_FEEDBACK` | Life-support resource reserve constraint | NO |
| **W083** | `L89_WaterMassAllocationController` (`P131`) | `L89_WaterPowerPriorityScheduler` (`P202`) | `ANALOG_FEEDBACK` | Real resource allocation and transfer status | NO |
| **W084** | `L89_HeliosMissionSequencingPLC` (`P223`) | `L89_WaterMassAllocationController` (`P129`) | `DIGITAL_CONTROL` | Approved shielding resource optimization request | NO |
| **W085** | `L89_ShieldMotorDriveCabinet` (`P173`) | `L89_ServoShieldActuatorA` (`P155`) | `DIGITAL_CONTROL` | Port cassette servo A drive command | NO |
| **W086** | `L89_ShieldMotorDriveCabinet` (`P174`) | `L89_ServoShieldActuatorB` (`P159`) | `DIGITAL_CONTROL` | Upper cassette servo B drive command | NO |
| **W087** | `L89_ShieldMotorDriveCabinet` (`P172`) | `L89_ServoShieldActuatorC` (`P160`) | `DIGITAL_CONTROL` | Lower cassette servo C drive command | NO |
| **W088** | `L89_ShieldMotorDriveCabinet` (`P173`) | `L89_ServoShieldActuatorD` (`P164`) | `DIGITAL_CONTROL` | Starboard cassette servo D drive command | NO |
| **W089** | `L89_ServoShieldActuatorA` (`P156`) | `L89_ShieldPositionEncoderRack` (`P171`) | `ANALOG_FEEDBACK` | Servo A actual carriage position | NO |
| **W090** | `L89_ServoShieldActuatorB` (`P157`) | `L89_ShieldPositionEncoderRack` (`P169`) | `ANALOG_FEEDBACK` | Servo B actual carriage position | NO |
| **W091** | `L89_ServoShieldActuatorC` (`P161`) | `L89_ShieldPositionEncoderRack` (`P170`) | `ANALOG_FEEDBACK` | Servo C actual carriage position | NO |
| **W092** | `L89_ServoShieldActuatorD` (`P165`) | `L89_ShieldPositionEncoderRack` (`P171`) | `ANALOG_FEEDBACK` | Servo D actual carriage position | NO |
| **W093** | `L89_ShieldPositionEncoderRack` (`P169`) | `L89_ShieldingScenarioSafetyController` (`P212`) | `ANALOG_FEEDBACK` | Actual shield lockable pose qualification | NO |
| **W094** | `L89_ShieldMotorDriveCabinet` (`P173`) | `L89_ShieldPanelTop` (`P141`) | `DIGITAL_CONTROL` | Deploy top radiation shield cassette | NO |
| **W095** | `L89_ShieldMotorDriveCabinet` (`P174`) | `L89_ShieldPanelBottom` (`P145`) | `DIGITAL_CONTROL` | Deploy bottom radiation shield cassette | NO |
| **W096** | `L89_ShieldMotorDriveCabinet` (`P172`) | `L89_ShieldPanelPort` (`P146`) | `DIGITAL_CONTROL` | Deploy port side radiation shield cassette | NO |
| **W097** | `L89_ShieldMotorDriveCabinet` (`P173`) | `L89_ShieldPanelStarboard` (`P150`) | `DIGITAL_CONTROL` | Deploy starboard radiation shield cassette | NO |
| **W098** | `L89_ShieldPanelTop` (`P142`) | `L89_ShelterRadiationShieldQualityMonitor` (`P182`) | `ANALOG_FEEDBACK` | Actual top cassette coverage | NO |
| **W099** | `L89_ShieldPanelBottom` (`P143`) | `L89_ShelterRadiationShieldQualityMonitor` (`P180`) | `ANALOG_FEEDBACK` | Actual bottom cassette coverage | NO |
| **W100** | `L89_ShieldPanelPort` (`P147`) | `L89_ShelterRadiationShieldQualityMonitor` (`P181`) | `ANALOG_FEEDBACK` | Actual port cassette coverage | NO |
| **W101** | `L89_ShieldPanelStarboard` (`P151`) | `L89_ShelterRadiationShieldQualityMonitor` (`P182`) | `ANALOG_FEEDBACK` | Actual starboard cassette coverage | NO |
| **W102** | `L89_ShieldingScenarioSafetyController` (`P212`) | `L89_ShieldLockingPinArray` (`P166`) | `DIGITAL_CONTROL` | Authorize mechanical shield locking | NO |
| **W103** | `L89_ShieldLockingPinArray` (`P167`) | `L89_ShieldingScenarioSafetyController` (`P213`) | `SAFETY_FEEDBACK` | Mechanical lock engagement sensed | NO |
| **W104** | `L89_ShieldJammingFaultInjector` (`P177`) | `L89_ShieldMotorDriveCabinet` (`P174`) | `SAFETY_FEEDBACK` | Shield jamming actual fault state | NO |
| **W105** | `L89_HeliosMissionSequencingPLC` (`P223`) | `L89_ShieldJammingFaultInjector` (`P175`) | `DIGITAL_CONTROL` | Inject deterministic shield carriage jam | NO |
| **W106** | `L89_ShelterRadiationShieldQualityMonitor` (`P181`) | `L89_ShieldCoverageDigitalTwin` (`P184`) | `ANALOG_FEEDBACK` | Coverage summary into 3D shield twin | NO |
| **W107** | `L89_SpaceWeatherForecastConsole` (`P060`) | `L89_StormIntensityProfileGenerator` (`P208`) | `DIGITAL_CONTROL` | Space weather forecast configuration | NO |
| **W108** | `L89_StormIntensityProfileGenerator` (`P206`) | `L89_HeliosMissionSequencingPLC` (`P223`) | `ANALOG_FEEDBACK` | Actual simulated particle event telemetry | NO |
| **W109** | `L89_StormArrivalPredictionHologram` (`P062`) | `L89_HeliosMissionSequencingPLC` (`P224`) | `ANALOG_FEEDBACK` | Predicted storm onset window | NO |
| **W110** | `L89_ShieldArealDensityAnalyzer` (`P134`) | `L89_WaterPowerPriorityScheduler` (`P202`) | `ANALOG_FEEDBACK` | Current shielding mass and reserve margin | NO |
| **W111** | `L89_ThermalWaterResourceGuard` (`P135`) | `L89_WaterPowerPriorityScheduler` (`P200`) | `ANALOG_FEEDBACK` | Water inventory essential-utility constraints | NO |
| **W112** | `L89_ShelterEnvironmentalAirSupply` (`P187`) | `L89_ShelterProtectedLifeSupportPLC` (`P195`) | `ANALOG_FEEDBACK` | Actual shelter ventilation airflow | NO |
| **W113** | `L89_ShelterOxygenBufferInterface` (`P190`) | `L89_ShelterProtectedLifeSupportPLC` (`P196`) | `ANALOG_FEEDBACK` | Available oxygen buffer and partial pressure | NO |
| **W114** | `L89_ShelterThermalAirReclaimer` (`P191`) | `L89_ShelterProtectedLifeSupportPLC` (`P194`) | `ANALOG_FEEDBACK` | Shelter thermal recovery status | NO |
| **W115** | `L89_ShelterProtectedLifeSupportPLC` (`P195`) | `L89_HeliosSafetyPLC` (`P221`) | `SAFETY_FEEDBACK` | Independent life-support permissive state | NO |
| **W116** | `L89_ShelterReservePowerDistribution` (`P199`) | `L89_ShelterProtectedLifeSupportPLC` (`P196`) | `SAFETY_FEEDBACK` | Essential shelter power distribution health | NO |
| **W117** | `L89_WaterPowerPriorityScheduler` (`P200`) | `L89_ShelterReservePowerDistribution` (`P197`) | `DIGITAL_CONTROL` | Protected essential load schedule | NO |
| **W118** | `L89_HabitatCrewShelterOccupancyEmulator` (`P204`) | `L89_WaterPowerPriorityScheduler` (`P201`) | `ANALOG_FEEDBACK` | Occupancy-driven essential demand | NO |
| **W119** | `L89_StormIntensityProfileGenerator` (`P208`) | `L89_ShelterDoseLimitEvaluator` (`P102`) | `ANALOG_FEEDBACK` | Measured simulated storm intensity history | NO |
| **W120** | `L89_HighEnergySurgeFaultInjector` (`P209`) | `L89_HeliosMissionSequencingPLC` (`P223`) | `ANALOG_FEEDBACK` | High-energy surge event confirmation | NO |
| **W121** | `L89_HeliosMissionSequencingPLC` (`P224`) | `L89_HighEnergySurgeFaultInjector` (`P210`) | `DIGITAL_CONTROL` | Arm and inject higher-energy event | NO |
| **W122** | `L89_HeliosMissionSequencingPLC` (`P225`) | `L89_HabitatCrewShelterOccupancyEmulator` (`P205`) | `DIGITAL_CONTROL` | Start qualified simulated shelter occupation | NO |
| **W123** | `L89_HeliosMissionSequencingPLC` (`P223`) | `L89_ShelterEnvironmentalAirSupply` (`P186`) | `DIGITAL_CONTROL` | Enable shelter ventilation | NO |
| **W124** | `L89_HeliosMissionSequencingPLC` (`P224`) | `L89_ShelterThermalAirReclaimer` (`P192`) | `DIGITAL_CONTROL` | Enable controlled thermal recovery | NO |
| **W125** | `L89_ShieldingScenarioSafetyController` (`P214`) | `L89_HeliosMissionSequencingPLC` (`P225`) | `SAFETY_FEEDBACK` | Calculated shielding and shelter permit | NO |
| **W126** | `L89_HeliosMissionSequencingPLC` (`P223`) | `L89_ShelterRadiationContingencyConsole` (`P233`) | `DIGITAL_CONTROL` | Generate contingency response plan | NO |
| **W127** | `L89_ShelterRadiationContingencyConsole` (`P234`) | `L89_HeliosMissionSequencingPLC` (`P224`) | `ANALOG_FEEDBACK` | Contingency handling diagnostic | NO |
| **W128** | `L89_StormIntensityProfileGenerator` (`P208`) | `L89_SolarStorm3DParticleHologram` (`P230`) | `ANALOG_FEEDBACK` | Solar storm holographic event status | NO |
| **W129** | `L89_ShelterDoorActualInterlock` (`P179`) | `L89_HeliosSafetyPLC` (`P220`) | `SAFETY_FEEDBACK` | Actual shelter door closed and latched | NO |
| **W130** | `L89_ShieldLockingPinArray` (`P167`) | `L89_HeliosSafetyPLC` (`P221`) | `SAFETY_FEEDBACK` | Actual shield locking contact | NO |
| **W131** | `L89_ShelterRadiationShieldQualityMonitor` (`P182`) | `L89_HeliosSafetyPLC` (`P222`) | `SAFETY_FEEDBACK` | Shield coverage hardware health | NO |
| **W132** | `L89_IndependentShelterSafetyDosimeter` (`P073`) | `L89_HeliosSafetyPLC` (`P220`) | `SAFETY_FEEDBACK` | Independent radiation dose monitoring | NO |
| **W133** | `L89_ShelterProtectedLifeSupportPLC` (`P195`) | `L89_HeliosSafetyPLC` (`P221`) | `SAFETY_FEEDBACK` | Independent life support continuity | NO |
| **W134** | `L89_ShelterReservePowerDistribution` (`P199`) | `L89_HeliosSafetyPLC` (`P222`) | `SAFETY_FEEDBACK` | Essential shelter electrical power status | NO |
| **W135** | `L89_ShieldingScenarioSafetyController` (`P212`) | `L89_HeliosSafetyPLC` (`P220`) | `SAFETY_FEEDBACK` | Shielding adequacy permit | NO |
| **W136** | `L89_RadiationEmergencyIsolationStation` (`P215`) | `L89_HeliosSafetyPLC` (`P221`) | `SAFETY_FEEDBACK` | Emergency shelter stop input | NO |
| **W137** | `L89_HeliosSafetyPLC` (`P222`) | `L89_ShieldMotorDriveCabinet` (`P174`) | `DIGITAL_CONTROL` | Protected shield servo authorization | NO |
| **W138** | `L89_HeliosSafetyPLC` (`P220`) | `L89_HeliosMissionSequencingPLC` (`P223`) | `SAFETY_FEEDBACK` | Safety PLC overall release | NO |
| **W139** | `L89_HeliosSafetyPLC` (`P221`) | `L89_ShieldingScenarioSafetyController` (`P213`) | `DIGITAL_CONTROL` | Independent shielding enable request | NO |
| **W140** | `L89_HeliosSafetyPLC` (`P222`) | `L89_ShelterProtectedLifeSupportPLC` (`P196`) | `DIGITAL_CONTROL` | Emergency ventilation continuity permit | NO |
| **W141** | `L89_HeliosSafetyPLC` (`P220`) | `L89_RadiationEmergencyIsolationStation` (`P215`) | `DIGITAL_CONTROL` | Emergency isolation status request | NO |
| **W142** | `L89_HeliosSafetyPLC` (`P221`) | `L89_ShelterDoseLimitEvaluator` (`P101`) | `SAFETY_FEEDBACK` | Qualified sheltered dose ceiling permission | NO |
| **W143** | `L89_HeliosSafetyPLC` (`P222`) | `L89_ShelterRadiationContingencyConsole` (`P235`) | `DIGITAL_CONTROL` | Protected contingency power-mode command | NO |
| **W144** | `L89_RadiationScenarioDiagnosticRecorder` (`P238`) | `L89_HeliosSafetyPLC` (`P220`) | `SAFETY_FEEDBACK` | Fault recorder event timestamp | NO |
| **W145** | `L89_HeliosSafetyPLC` (`P221`) | `L89_RadiationScenarioDiagnosticRecorder` (`P239`) | `DIGITAL_CONTROL` | Latch safety trip event | NO |
| **W146** | `L89_HeliosSafetyPLC` (`P222`) | `L89_W149TrustedDoseFeedbackPatchStation` (`P236`) | `SAFETY_FEEDBACK` | Critical W149 patch circuit diagnostic | NO |
| **W147** | `L89_HeliosMissionSequencingPLC` (`P223`) | `L89_HeliosSafetyPLC` (`P220`) | `DIGITAL_CONTROL` | Operator explicit protection reset request | NO |
| **W148** | `L89_HeliosSafetyPLC` (`P221`) | `L89_HeliosMissionSequencingPLC` (`P224`) | `SAFETY_FEEDBACK` | Fault-latch event status | NO |
| **W149** | `L89_ShelterReferenceDosimeter` (`P072`) | `L89_HeliosSafetyPLC` (`P222`) | `ANALOG_FEEDBACK` | CRITICAL trusted shelter dose-rate measurement to independent radiation safety PLC | YES |
| **W150** | `L89_ShieldingScenarioSafetyController` (`P212`) | `L89_ShieldCoverageDigitalTwin` (`P183`) | `SAFETY_FEEDBACK` | Current shield coverage qualification | NO |
| **W151** | `L89_HeliosSafetyPLC` (`P221`) | `L89_EmergencyShelterCommunications` (`P244`) | `SAFETY_FEEDBACK` | Emergency crew warning authorisation | NO |
| **W152** | `L89_EmergencyShelterCommunications` (`P244`) | `L89_HeliosSafetyPLC` (`P222`) | `SAFETY_FEEDBACK` | Emergency communication equipment status | NO |
| **W153** | `L89_HeliosSafetyPLC` (`P220`) | `L89_StormShelterCertificationBeacon` (`P242`) | `DIGITAL_CONTROL` | Authorization to show final certification | NO |
| **W154** | `L89_StormIntensityProfileGenerator` (`P207`) | `L89_HeliosSafetyPLC` (`P221`) | `SAFETY_FEEDBACK` | Monitored actual storm state independent verification | NO |
| **W155** | `L89_PrimaryParticleCounter` (`P047`) | `L89_RadiationDefenseSCADA` (`P228`) | `ANALOG_FEEDBACK` | Live primary particle detection trend | NO |
| **W156** | `L89_SecondaryParticleCounter` (`P048`) | `L89_RadiationDefenseSCADA` (`P226`) | `ANALOG_FEEDBACK` | Live secondary particle trend | NO |
| **W157** | `L89_DoseAccumulationAnalyzer` (`P089`) | `L89_RadiationDefenseSCADA` (`P227`) | `ANALOG_FEEDBACK` | Dose integral run history | NO |
| **W158** | `L89_ShelterDoseLimitEvaluator` (`P102`) | `L89_RadiationDefenseSCADA` (`P228`) | `ANALOG_FEEDBACK` | Exposition budget and guard-band data | NO |
| **W159** | `L89_WaterTankLevelSensorArray` (`P117`) | `L89_RadiationDefenseSCADA` (`P226`) | `ANALOG_FEEDBACK` | Finite water shield mass visualization | NO |
| **W160** | `L89_WaterMassAllocationController` (`P130`) | `L89_RadiationDefenseSCADA` (`P227`) | `ANALOG_FEEDBACK` | Water transfer operation status | NO |
| **W161** | `L89_ShieldPositionEncoderRack` (`P171`) | `L89_RadiationDefenseSCADA` (`P228`) | `ANALOG_FEEDBACK` | Four shield encoders and lock positions | NO |
| **W162** | `L89_ShelterRadiationShieldQualityMonitor` (`P180`) | `L89_RadiationDefenseSCADA` (`P226`) | `ANALOG_FEEDBACK` | Actual shielding coverage map | NO |
| **W163** | `L89_ShelterEnvironmentalAirSupply` (`P187`) | `L89_RadiationDefenseSCADA` (`P227`) | `ANALOG_FEEDBACK` | Shelter ventilation status | NO |
| **W164** | `L89_ShelterProtectedLifeSupportPLC` (`P196`) | `L89_RadiationDefenseSCADA` (`P228`) | `ANALOG_FEEDBACK` | Life support capability margin | NO |
| **W165** | `L89_StormIntensityProfileGenerator` (`P206`) | `L89_RadiationDefenseSCADA` (`P226`) | `ANALOG_FEEDBACK` | Solar storm timeline and phase | NO |
| **W166** | `L89_ShieldingScenarioSafetyController` (`P213`) | `L89_RadiationDefenseSCADA` (`P227`) | `ANALOG_FEEDBACK` | Independent shield safety health | NO |
| **W167** | `L89_HeliosSafetyPLC` (`P222`) | `L89_RadiationDefenseSCADA` (`P228`) | `ANALOG_FEEDBACK` | Authoritative safety latches and validity | NO |
| **W168** | `L89_HeliosMissionSequencingPLC` (`P223`) | `L89_RadiationDefenseSCADA` (`P226`) | `ANALOG_FEEDBACK` | Six authoritative objective progress | NO |
| **W169** | `L89_RadiationScenarioDiagnosticRecorder` (`P239`) | `L89_RadiationDefenseSCADA` (`P227`) | `ANALOG_FEEDBACK` | Fault trace and diagnostic record | NO |
| **W170** | `L89_RadiationDefenseSCADA` (`P228`) | `L89_StormShelterCertificationBeacon` (`P242`) | `DIGITAL_CONTROL` | Read only final certification display request | NO |

## ANNEX D — ALL 248 MODELED DECORATIVE VISUAL SOCKET ANCHORS

These exact source records are **not actual functional `SparkTerminal` objects**. Keep their owners and metre positions intact.

| AnchorID | Model | WorldX_m | WorldY_m | WorldZ_m | Status |
|---|---|---|---|---|---|
| `P001` | `L89_HeliosRadiationDefenseArenaBase` | -0.09 | -0.56 | 0.72 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P002` | `L89_HeliosRadiationDefenseArenaBase` | 0.09 | -0.56 | 0.72 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P003` | `L89_SolarStormPanoramicObservatory` | -0.09 | 7.0 | -12.53 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P004` | `L89_SolarStormPanoramicObservatory` | 0.09 | 7.0 | -12.53 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P005` | `L89_OrbitalStationHabitatExterior` | 4.81 | 9.8 | -15.98 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P006` | `L89_OrbitalStationHabitatExterior` | 4.99 | 9.8 | -15.98 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P007` | `L89_StormShelterPressureHull` | -0.09 | 3.4 | -2.58 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P008` | `L89_StormShelterPressureHull` | 0.09 | 3.4 | -2.58 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P009` | `L89_ShelterEntranceShieldedBulkhead` | -0.09 | 3.4 | -0.43 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P010` | `L89_ShelterEntranceShieldedBulkhead` | 0.09 | 3.4 | -0.43 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P011` | `L89_SolarWeatherHologramDome` | -6.39 | 9.3 | -11.68 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P012` | `L89_SolarWeatherHologramDome` | -6.21 | 9.3 | -11.68 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P013` | `L89_EarthAndHabitatObservationWindow` | -0.09 | 7.1 | -12.28 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P014` | `L89_EarthAndHabitatObservationWindow` | 0.09 | 7.1 | -12.28 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P015` | `L89_StormShelterInteriorSupportDeck` | -0.09 | 0.1 | -2.48 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P016` | `L89_StormShelterInteriorSupportDeck` | 0.09 | 0.1 | -2.48 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P017` | `L89_WaterWallStructuralContainmentFrame` | -0.09 | 3.4 | -2.28 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P018` | `L89_WaterWallStructuralContainmentFrame` | 0.09 | 3.4 | -2.28 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P019` | `L89_ShelterEmergencyEgressHatch` | -0.09 | 3.4 | -5.38 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P020` | `L89_ShelterEmergencyEgressHatch` | 0.09 | 3.4 | -5.38 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P021` | `L89_OuterExposureInstrumentPlatform` | -12.49 | 4.0 | -5.88 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P022` | `L89_OuterExposureInstrumentPlatform` | -12.31 | 4.0 | -5.88 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P023` | `L89_StationShieldWaterUtilitySkid` | 11.81 | 1.6 | -5.28 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P024` | `L89_StationShieldWaterUtilitySkid` | 11.99 | 1.6 | -5.28 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P025` | `L89_SolarEnergeticParticleTelescopeA` | -11.28 | 4.7 | -6.72 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P026` | `L89_SolarEnergeticParticleTelescopeA` | -11.1 | 4.7 | -6.72 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P027` | `L89_SolarEnergeticParticleTelescopeA` | -10.92 | 4.7 | -6.72 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P028` | `L89_SolarEnergeticParticleTelescopeB` | -9.08 | 4.7 | -6.72 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P029` | `L89_SolarEnergeticParticleTelescopeB` | -8.9 | 4.7 | -6.72 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P030` | `L89_SolarEnergeticParticleTelescopeB` | -8.72 | 4.7 | -6.72 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P031` | `L89_ChargedParticleSpectrometer` | -12.38 | 1.9 | -3.92 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P032` | `L89_ChargedParticleSpectrometer` | -12.2 | 1.9 | -3.92 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P033` | `L89_ChargedParticleSpectrometer` | -12.02 | 1.9 | -3.92 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P034` | `L89_ProtonEnergyBinAnalyzer` | -10.58 | 1.9 | -3.92 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P035` | `L89_ProtonEnergyBinAnalyzer` | -10.4 | 1.9 | -3.92 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P036` | `L89_ProtonEnergyBinAnalyzer` | -10.22 | 1.9 | -3.92 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P037` | `L89_HeavyIonChannelDetector` | -8.64 | 1.9 | -3.92 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P038` | `L89_HeavyIonChannelDetector` | -8.46 | 1.9 | -3.92 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P039` | `L89_SolarVectorTrackingGimbal` | -9.98 | 4.9 | -7.72 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P040` | `L89_SolarVectorTrackingGimbal` | -9.8 | 4.9 | -7.72 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P041` | `L89_SolarVectorTrackingGimbal` | -9.62 | 4.9 | -7.72 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P042` | `L89_ParticleCollimatorHead` | -11.18 | 4.1 | -6.02 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P043` | `L89_ParticleCollimatorHead` | -11.0 | 4.1 | -6.02 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P044` | `L89_ParticleCollimatorHead` | -10.82 | 4.1 | -6.02 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P045` | `L89_PrimaryParticleCounter` | -10.68 | 1.45 | -2.72 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P046` | `L89_PrimaryParticleCounter` | -10.5 | 1.45 | -2.72 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P047` | `L89_PrimaryParticleCounter` | -10.32 | 1.45 | -2.72 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P048` | `L89_SecondaryParticleCounter` | -9.08 | 1.45 | -2.72 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P049` | `L89_SecondaryParticleCounter` | -8.9 | 1.45 | -2.72 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P050` | `L89_SecondaryParticleCounter` | -8.72 | 1.45 | -2.72 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P051` | `L89_DetectorPulseShaper` | -13.09 | 1.45 | 2.58 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P052` | `L89_DetectorPulseShaper` | -12.91 | 1.45 | 2.58 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P053` | `L89_SpectralCalibrationSource` | -11.74 | 1.45 | 2.58 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P054` | `L89_SpectralCalibrationSource` | -11.56 | 1.45 | 2.58 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P055` | `L89_SpectralCalibrationSource` | -11.38 | 1.45 | 2.58 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P056` | `L89_ParticleDetectorThermalShroud` | -10.21 | 1.45 | 2.58 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P057` | `L89_ParticleDetectorThermalShroud` | -10.03 | 1.45 | 2.58 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P058` | `L89_SpaceWeatherForecastConsole` | -8.86 | 1.45 | 2.58 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P059` | `L89_SpaceWeatherForecastConsole` | -8.68 | 1.45 | 2.58 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P060` | `L89_SpaceWeatherForecastConsole` | -8.5 | 1.45 | 2.58 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P061` | `L89_StormArrivalPredictionHologram` | -7.42 | 1.45 | 2.58 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P062` | `L89_StormArrivalPredictionHologram` | -7.24 | 1.45 | 2.58 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P063` | `L89_StormArrivalPredictionHologram` | -7.06 | 1.45 | 2.58 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P064` | `L89_ExternalDosimeterArray` | -6.18 | 3.7 | -6.02 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P065` | `L89_ExternalDosimeterArray` | -6.0 | 3.7 | -6.02 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P066` | `L89_ExternalDosimeterArray` | -5.82 | 3.7 | -6.02 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P067` | `L89_HabitatDosimeterArray` | -4.68 | 3.7 | -6.02 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P068` | `L89_HabitatDosimeterArray` | -4.5 | 3.7 | -6.02 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P069` | `L89_HabitatDosimeterArray` | -4.32 | 3.7 | -6.02 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P070` | `L89_ShelterReferenceDosimeter` | -3.38 | 3.0 | -1.42 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P071` | `L89_ShelterReferenceDosimeter` | -3.2 | 3.0 | -1.42 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P072` | `L89_ShelterReferenceDosimeter` | -3.02 | 3.0 | -1.42 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P073` | `L89_IndependentShelterSafetyDosimeter` | 3.02 | 3.0 | -1.42 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P074` | `L89_IndependentShelterSafetyDosimeter` | 3.2 | 3.0 | -1.42 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P075` | `L89_IndependentShelterSafetyDosimeter` | 3.38 | 3.0 | -1.42 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P076` | `L89_TriaxialRadiationDoseProbe` | -5.58 | 1.4 | -2.72 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P077` | `L89_TriaxialRadiationDoseProbe` | -5.4 | 1.4 | -2.72 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P078` | `L89_TriaxialRadiationDoseProbe` | -5.22 | 1.4 | -2.72 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P079` | `L89_DoseRateTransientRecorder` | -5.98 | 1.45 | 2.58 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P080` | `L89_DoseRateTransientRecorder` | -5.8 | 1.45 | 2.58 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P081` | `L89_DoseRateTransientRecorder` | -5.62 | 1.45 | 2.58 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P082` | `L89_DosimeterCalibrationFrame` | -7.38 | 1.5 | -2.62 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P083` | `L89_DosimeterCalibrationFrame` | -7.2 | 1.5 | -2.62 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P084` | `L89_DosimeterCalibrationFrame` | -7.02 | 1.5 | -2.62 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P085` | `L89_DetectorSaturationFaultInjector` | -4.54 | 1.45 | 2.58 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P086` | `L89_DetectorSaturationFaultInjector` | -4.36 | 1.45 | 2.58 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P087` | `L89_DetectorSaturationFaultInjector` | -4.18 | 1.45 | 2.58 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P088` | `L89_DoseAccumulationAnalyzer` | -3.1 | 1.45 | 2.58 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P089` | `L89_DoseAccumulationAnalyzer` | -2.92 | 1.45 | 2.58 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P090` | `L89_DoseAccumulationAnalyzer` | -2.74 | 1.45 | 2.58 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P091` | `L89_DistributedDetectorNetworkGateway` | -1.66 | 1.45 | 2.58 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P092` | `L89_DistributedDetectorNetworkGateway` | -1.48 | 1.45 | 2.58 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P093` | `L89_DistributedDetectorNetworkGateway` | -1.3 | 1.45 | 2.58 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P094` | `L89_RadiationHeatmapHologram` | -6.08 | 7.0 | -9.42 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P095` | `L89_RadiationHeatmapHologram` | -5.9 | 7.0 | -9.42 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P096` | `L89_RadiationHeatmapHologram` | -5.72 | 7.0 | -9.42 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P097` | `L89_DetectorRedundancyComparator` | -0.22 | 1.45 | 2.58 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P098` | `L89_DetectorRedundancyComparator` | -0.04 | 1.45 | 2.58 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P099` | `L89_DetectorRedundancyComparator` | 0.14 | 1.45 | 2.58 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P100` | `L89_ShelterDoseLimitEvaluator` | 1.22 | 1.45 | 2.58 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P101` | `L89_ShelterDoseLimitEvaluator` | 1.4 | 1.45 | 2.58 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P102` | `L89_ShelterDoseLimitEvaluator` | 1.58 | 1.45 | 2.58 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P103` | `L89_WaterWallTankPortA` | -2.33 | 3.4 | -2.58 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P104` | `L89_WaterWallTankPortA` | -2.15 | 3.4 | -2.58 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P105` | `L89_WaterWallTankPortA` | -1.97 | 3.4 | -2.58 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P106` | `L89_WaterWallTankPortB` | -1.28 | 3.4 | -2.58 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P107` | `L89_WaterWallTankPortB` | -1.1 | 3.4 | -2.58 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P108` | `L89_WaterWallTankPortB` | -0.92 | 3.4 | -2.58 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P109` | `L89_WaterWallTankStarboardA` | 0.92 | 3.4 | -2.58 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P110` | `L89_WaterWallTankStarboardA` | 1.1 | 3.4 | -2.58 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P111` | `L89_WaterWallTankStarboardA` | 1.28 | 3.4 | -2.58 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P112` | `L89_WaterWallTankStarboardB` | 1.97 | 3.4 | -2.58 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P113` | `L89_WaterWallTankStarboardB` | 2.15 | 3.4 | -2.58 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P114` | `L89_WaterWallTankStarboardB` | 2.33 | 3.4 | -2.58 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P115` | `L89_WaterReservoirMain` | 8.91 | 2.0 | -4.78 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P116` | `L89_WaterReservoirMain` | 9.09 | 2.0 | -4.78 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P117` | `L89_WaterTankLevelSensorArray` | 10.32 | 2.95 | -3.82 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P118` | `L89_WaterTankLevelSensorArray` | 10.5 | 2.95 | -3.82 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P119` | `L89_WaterTankLevelSensorArray` | 10.68 | 2.95 | -3.82 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P120` | `L89_WaterTransferPumpA` | 10.42 | 1.35 | -2.07 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P121` | `L89_WaterTransferPumpA` | 10.6 | 1.35 | -2.07 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P122` | `L89_WaterTransferPumpA` | 10.78 | 1.35 | -2.07 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P123` | `L89_WaterTransferPumpB` | 12.02 | 1.35 | -2.07 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P124` | `L89_WaterTransferPumpB` | 12.2 | 1.35 | -2.07 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P125` | `L89_WaterTransferPumpB` | 12.38 | 1.35 | -2.07 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P126` | `L89_WaterDistributionValveManifold` | 10.92 | 1.2 | -0.52 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P127` | `L89_WaterDistributionValveManifold` | 11.1 | 1.2 | -0.52 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P128` | `L89_WaterDistributionValveManifold` | 11.28 | 1.2 | -0.52 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P129` | `L89_WaterMassAllocationController` | -12.38 | 1.35 | 4.5 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P130` | `L89_WaterMassAllocationController` | -12.2 | 1.35 | 4.5 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P131` | `L89_WaterMassAllocationController` | -12.02 | 1.35 | 4.5 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P132` | `L89_ShieldArealDensityAnalyzer` | -10.25 | 1.35 | 4.5 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P133` | `L89_ShieldArealDensityAnalyzer` | -10.07 | 1.35 | 4.5 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P134` | `L89_ShieldArealDensityAnalyzer` | -9.89 | 1.35 | 4.5 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P135` | `L89_ThermalWaterResourceGuard` | -8.12 | 1.35 | 4.5 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P136` | `L89_ThermalWaterResourceGuard` | -7.94 | 1.35 | 4.5 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P137` | `L89_ThermalWaterResourceGuard` | -7.76 | 1.35 | 4.5 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P138` | `L89_WaterWallLeakMonitor` | -5.9 | 1.35 | 4.5 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P139` | `L89_WaterWallLeakMonitor` | -5.72 | 1.35 | 4.5 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P140` | `L89_ShieldPanelTop` | -0.18 | 6.4 | -2.58 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P141` | `L89_ShieldPanelTop` | 0.0 | 6.4 | -2.58 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P142` | `L89_ShieldPanelTop` | 0.18 | 6.4 | -2.58 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P143` | `L89_ShieldPanelBottom` | -0.18 | 0.6 | -2.58 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P144` | `L89_ShieldPanelBottom` | 0.0 | 0.6 | -2.58 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P145` | `L89_ShieldPanelBottom` | 0.18 | 0.6 | -2.58 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P146` | `L89_ShieldPanelPort` | -3.53 | 3.4 | -2.58 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P147` | `L89_ShieldPanelPort` | -3.35 | 3.4 | -2.58 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P148` | `L89_ShieldPanelPort` | -3.17 | 3.4 | -2.58 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P149` | `L89_ShieldPanelStarboard` | 3.17 | 3.4 | -2.58 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P150` | `L89_ShieldPanelStarboard` | 3.35 | 3.4 | -2.58 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P151` | `L89_ShieldPanelStarboard` | 3.53 | 3.4 | -2.58 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P152` | `L89_ShieldCassetteRailArray` | -0.09 | 3.45 | -2.48 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P153` | `L89_ShieldCassetteRailArray` | 0.09 | 3.45 | -2.48 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P154` | `L89_ServoShieldActuatorA` | -3.18 | 1.6 | -0.28 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P155` | `L89_ServoShieldActuatorA` | -3.0 | 1.6 | -0.28 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P156` | `L89_ServoShieldActuatorA` | -2.82 | 1.6 | -0.28 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P157` | `L89_ServoShieldActuatorB` | -1.18 | 1.6 | -0.28 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P158` | `L89_ServoShieldActuatorB` | -1.0 | 1.6 | -0.28 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P159` | `L89_ServoShieldActuatorB` | -0.82 | 1.6 | -0.28 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P160` | `L89_ServoShieldActuatorC` | 0.82 | 1.6 | -0.28 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P161` | `L89_ServoShieldActuatorC` | 1.0 | 1.6 | -0.28 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P162` | `L89_ServoShieldActuatorC` | 1.18 | 1.6 | -0.28 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P163` | `L89_ServoShieldActuatorD` | 2.82 | 1.6 | -0.28 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P164` | `L89_ServoShieldActuatorD` | 3.0 | 1.6 | -0.28 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P165` | `L89_ServoShieldActuatorD` | 3.18 | 1.6 | -0.28 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P166` | `L89_ShieldLockingPinArray` | -0.18 | 6.05 | -0.28 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P167` | `L89_ShieldLockingPinArray` | 0.0 | 6.05 | -0.28 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P168` | `L89_ShieldLockingPinArray` | 0.18 | 6.05 | -0.28 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P169` | `L89_ShieldPositionEncoderRack` | -3.86 | 1.35 | 4.5 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P170` | `L89_ShieldPositionEncoderRack` | -3.68 | 1.35 | 4.5 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P171` | `L89_ShieldPositionEncoderRack` | -3.5 | 1.35 | 4.5 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P172` | `L89_ShieldMotorDriveCabinet` | -1.73 | 1.35 | 4.5 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P173` | `L89_ShieldMotorDriveCabinet` | -1.55 | 1.35 | 4.5 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P174` | `L89_ShieldMotorDriveCabinet` | -1.37 | 1.35 | 4.5 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P175` | `L89_ShieldJammingFaultInjector` | 0.4 | 1.35 | 4.5 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P176` | `L89_ShieldJammingFaultInjector` | 0.58 | 1.35 | 4.5 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P177` | `L89_ShieldJammingFaultInjector` | 0.76 | 1.35 | 4.5 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P178` | `L89_ShelterDoorActualInterlock` | 1.51 | 1.6 | -0.27 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P179` | `L89_ShelterDoorActualInterlock` | 1.69 | 1.6 | -0.27 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P180` | `L89_ShelterRadiationShieldQualityMonitor` | -0.18 | 6.9 | -2.72 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P181` | `L89_ShelterRadiationShieldQualityMonitor` | 0.0 | 6.9 | -2.72 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P182` | `L89_ShelterRadiationShieldQualityMonitor` | 0.18 | 6.9 | -2.72 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P183` | `L89_ShieldCoverageDigitalTwin` | -0.18 | 8.05 | -9.52 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P184` | `L89_ShieldCoverageDigitalTwin` | 0.0 | 8.05 | -9.52 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P185` | `L89_ShieldCoverageDigitalTwin` | 0.18 | 8.05 | -9.52 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P186` | `L89_ShelterEnvironmentalAirSupply` | 4.92 | 1.8 | -4.72 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P187` | `L89_ShelterEnvironmentalAirSupply` | 5.1 | 1.8 | -4.72 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P188` | `L89_ShelterEnvironmentalAirSupply` | 5.28 | 1.8 | -4.72 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P189` | `L89_ShelterOxygenBufferInterface` | 6.81 | 1.8 | -4.72 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P190` | `L89_ShelterOxygenBufferInterface` | 6.99 | 1.8 | -4.72 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P191` | `L89_ShelterThermalAirReclaimer` | 8.32 | 1.8 | -4.72 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P192` | `L89_ShelterThermalAirReclaimer` | 8.5 | 1.8 | -4.72 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P193` | `L89_ShelterThermalAirReclaimer` | 8.68 | 1.8 | -4.72 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P194` | `L89_ShelterProtectedLifeSupportPLC` | 2.53 | 1.35 | 4.5 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P195` | `L89_ShelterProtectedLifeSupportPLC` | 2.71 | 1.35 | 4.5 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P196` | `L89_ShelterProtectedLifeSupportPLC` | 2.89 | 1.35 | 4.5 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P197` | `L89_ShelterReservePowerDistribution` | 4.66 | 1.35 | 4.5 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P198` | `L89_ShelterReservePowerDistribution` | 4.84 | 1.35 | 4.5 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P199` | `L89_ShelterReservePowerDistribution` | 5.02 | 1.35 | 4.5 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P200` | `L89_WaterPowerPriorityScheduler` | 6.79 | 1.35 | 4.5 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P201` | `L89_WaterPowerPriorityScheduler` | 6.97 | 1.35 | 4.5 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P202` | `L89_WaterPowerPriorityScheduler` | 7.15 | 1.35 | 4.5 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P203` | `L89_HabitatCrewShelterOccupancyEmulator` | 6.22 | 1.5 | -2.42 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P204` | `L89_HabitatCrewShelterOccupancyEmulator` | 6.4 | 1.5 | -2.42 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P205` | `L89_HabitatCrewShelterOccupancyEmulator` | 6.58 | 1.5 | -2.42 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P206` | `L89_StormIntensityProfileGenerator` | -6.58 | 7.6 | -9.92 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P207` | `L89_StormIntensityProfileGenerator` | -6.4 | 7.6 | -9.92 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P208` | `L89_StormIntensityProfileGenerator` | -6.22 | 7.6 | -9.92 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P209` | `L89_HighEnergySurgeFaultInjector` | 8.92 | 1.35 | 4.5 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P210` | `L89_HighEnergySurgeFaultInjector` | 9.1 | 1.35 | 4.5 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P211` | `L89_HighEnergySurgeFaultInjector` | 9.28 | 1.35 | 4.5 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P212` | `L89_ShieldingScenarioSafetyController` | 11.05 | 1.35 | 4.5 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P213` | `L89_ShieldingScenarioSafetyController` | 11.23 | 1.35 | 4.5 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P214` | `L89_ShieldingScenarioSafetyController` | 11.41 | 1.35 | 4.5 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P215` | `L89_RadiationEmergencyIsolationStation` | 12.71 | 1.25 | 5.78 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P216` | `L89_RadiationEmergencyIsolationStation` | 12.89 | 1.25 | 5.78 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P217` | `L89_Protected5VControlSupply` | 10.82 | 1.3 | 5.98 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P218` | `L89_Protected5VControlSupply` | 11.0 | 1.3 | 5.98 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P219` | `L89_Protected5VControlSupply` | 11.18 | 1.3 | 5.98 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P220` | `L89_HeliosSafetyPLC` | 7.72 | 1.4 | 5.98 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P221` | `L89_HeliosSafetyPLC` | 7.9 | 1.4 | 5.98 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P222` | `L89_HeliosSafetyPLC` | 8.08 | 1.4 | 5.98 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P223` | `L89_HeliosMissionSequencingPLC` | 6.12 | 1.4 | 5.98 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P224` | `L89_HeliosMissionSequencingPLC` | 6.3 | 1.4 | 5.98 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P225` | `L89_HeliosMissionSequencingPLC` | 6.48 | 1.4 | 5.98 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P226` | `L89_RadiationDefenseSCADA` | -0.18 | 8.1 | -8.32 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P227` | `L89_RadiationDefenseSCADA` | 0.0 | 8.1 | -8.32 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P228` | `L89_RadiationDefenseSCADA` | 0.18 | 8.1 | -8.32 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P229` | `L89_SolarStorm3DParticleHologram` | -0.09 | 10.8 | -10.52 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P230` | `L89_SolarStorm3DParticleHologram` | 0.09 | 10.8 | -10.52 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P231` | `L89_ExteriorParticleFieldEmitter` | -3.29 | 7.6 | -10.62 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P232` | `L89_ExteriorParticleFieldEmitter` | -3.11 | 7.6 | -10.62 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P233` | `L89_ShelterRadiationContingencyConsole` | -7.98 | 1.3 | 5.98 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P234` | `L89_ShelterRadiationContingencyConsole` | -7.8 | 1.3 | 5.98 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P235` | `L89_ShelterRadiationContingencyConsole` | -7.62 | 1.3 | 5.98 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P236` | `L89_W149TrustedDoseFeedbackPatchStation` | -6.49 | 1.3 | 5.98 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P237` | `L89_W149TrustedDoseFeedbackPatchStation` | -6.31 | 1.3 | 5.98 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P238` | `L89_RadiationScenarioDiagnosticRecorder` | -4.98 | 1.3 | 5.98 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P239` | `L89_RadiationScenarioDiagnosticRecorder` | -4.8 | 1.3 | 5.98 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P240` | `L89_RadiationScenarioDiagnosticRecorder` | -4.62 | 1.3 | 5.98 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P241` | `L89_StormShelterCertificationBeacon` | -0.09 | 12.0 | -9.32 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P242` | `L89_StormShelterCertificationBeacon` | 0.09 | 12.0 | -9.32 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P243` | `L89_EmergencyShelterCommunications` | -3.29 | 1.3 | 8.28 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P244` | `L89_EmergencyShelterCommunications` | -3.11 | 1.3 | 8.28 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P245` | `L89_EquipmentMaintenanceInspectionCamera` | -1.64 | 1.3 | 8.28 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P246` | `L89_EquipmentMaintenanceInspectionCamera` | -1.46 | 1.3 | 8.28 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P247` | `L89_SolarStormSurveyAntennaArray` | -13.39 | 4.9 | -7.12 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |
| `P248` | `L89_SolarStormSurveyAntennaArray` | -13.21 | 4.9 | -7.12 | MODELED_DECORATIVE_SOCKET_NOT_SPARKTERMINAL |

## ANNEX E — ALL 340 PROPOSED INDEPENDENT LOGICAL WIRE ENDPOINTS

Source `FunctionalStatus=PLANNED_NOT_ACTUAL_SPARKTERMINAL`: each endpoint is a distinct proposed contact. The functional IDs are from the CSV; human-readable W149 aliases from the separate recovery plan must map to the same actual contacts.

| Wire | End | Model | Original art socket | Functional port ID | Signal kind | Direction | Critical |
|---|---|---|---|---|---|---|:---:|
| W001 | Source | `L89_Protected5VControlSupply` | `P218` | `W001_OUT_POWER_CONTROL` | `POWER_CONTROL` | OUTPUT | NO |
| W001 | Target | `L89_HeliosSafetyPLC` | `P221` | `W001_IN_POWER_CONTROL` | `POWER_CONTROL` | INPUT | NO |
| W002 | Source | `L89_Protected5VControlSupply` | `P219` | `W002_OUT_POWER_CONTROL` | `POWER_CONTROL` | OUTPUT | NO |
| W002 | Target | `L89_HeliosMissionSequencingPLC` | `P225` | `W002_IN_POWER_CONTROL` | `POWER_CONTROL` | INPUT | NO |
| W003 | Source | `L89_Protected5VControlSupply` | `P217` | `W003_OUT_POWER_CONTROL` | `POWER_CONTROL` | OUTPUT | NO |
| W003 | Target | `L89_DistributedDetectorNetworkGateway` | `P091` | `W003_IN_POWER_CONTROL` | `POWER_CONTROL` | INPUT | NO |
| W004 | Source | `L89_Protected5VControlSupply` | `P218` | `W004_OUT_POWER_CONTROL` | `POWER_CONTROL` | OUTPUT | NO |
| W004 | Target | `L89_ShieldMotorDriveCabinet` | `P173` | `W004_IN_POWER_CONTROL` | `POWER_CONTROL` | INPUT | NO |
| W005 | Source | `L89_Protected5VControlSupply` | `P219` | `W005_OUT_POWER_CONTROL` | `POWER_CONTROL` | OUTPUT | NO |
| W005 | Target | `L89_ShelterProtectedLifeSupportPLC` | `P196` | `W005_IN_POWER_CONTROL` | `POWER_CONTROL` | INPUT | NO |
| W006 | Source | `L89_Protected5VControlSupply` | `P217` | `W006_OUT_POWER_CONTROL` | `POWER_CONTROL` | OUTPUT | NO |
| W006 | Target | `L89_RadiationDefenseSCADA` | `P226` | `W006_IN_POWER_CONTROL` | `POWER_CONTROL` | INPUT | NO |
| W007 | Source | `L89_Protected5VControlSupply` | `P218` | `W007_OUT_POWER_CONTROL` | `POWER_CONTROL` | OUTPUT | NO |
| W007 | Target | `L89_ShelterReferenceDosimeter` | `P071` | `W007_IN_POWER_CONTROL` | `POWER_CONTROL` | INPUT | NO |
| W008 | Source | `L89_Protected5VControlSupply` | `P219` | `W008_OUT_POWER_CONTROL` | `POWER_CONTROL` | OUTPUT | NO |
| W008 | Target | `L89_IndependentShelterSafetyDosimeter` | `P075` | `W008_IN_POWER_CONTROL` | `POWER_CONTROL` | INPUT | NO |
| W009 | Source | `L89_Protected5VControlSupply` | `P217` | `W009_OUT_POWER_CONTROL` | `POWER_CONTROL` | OUTPUT | NO |
| W009 | Target | `L89_WaterMassAllocationController` | `P129` | `W009_IN_POWER_CONTROL` | `POWER_CONTROL` | INPUT | NO |
| W010 | Source | `L89_Protected5VControlSupply` | `P218` | `W010_OUT_POWER_CONTROL` | `POWER_CONTROL` | OUTPUT | NO |
| W010 | Target | `L89_SpaceWeatherForecastConsole` | `P059` | `W010_IN_POWER_CONTROL` | `POWER_CONTROL` | INPUT | NO |
| W011 | Source | `L89_HeliosSafetyPLC` | `P222` | `W011_OUT_POWER_CONTROL` | `POWER_CONTROL` | OUTPUT | NO |
| W011 | Target | `L89_Protected5VControlSupply` | `P219` | `W011_IN_POWER_CONTROL` | `POWER_CONTROL` | INPUT | NO |
| W012 | Source | `L89_HeliosMissionSequencingPLC` | `P223` | `W012_OUT_POWER_CONTROL` | `POWER_CONTROL` | OUTPUT | NO |
| W012 | Target | `L89_Protected5VControlSupply` | `P217` | `W012_IN_POWER_CONTROL` | `POWER_CONTROL` | INPUT | NO |
| W013 | Source | `L89_SolarEnergeticParticleTelescopeA` | `P026` | `W013_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT | NO |
| W013 | Target | `L89_DistributedDetectorNetworkGateway` | `P092` | `W013_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT | NO |
| W014 | Source | `L89_SolarEnergeticParticleTelescopeB` | `P030` | `W014_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT | NO |
| W014 | Target | `L89_DistributedDetectorNetworkGateway` | `P093` | `W014_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT | NO |
| W015 | Source | `L89_ChargedParticleSpectrometer` | `P031` | `W015_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT | NO |
| W015 | Target | `L89_ProtonEnergyBinAnalyzer` | `P034` | `W015_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT | NO |
| W016 | Source | `L89_ProtonEnergyBinAnalyzer` | `P035` | `W016_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT | NO |
| W016 | Target | `L89_DistributedDetectorNetworkGateway` | `P092` | `W016_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT | NO |
| W017 | Source | `L89_HeavyIonChannelDetector` | `P038` | `W017_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT | NO |
| W017 | Target | `L89_DistributedDetectorNetworkGateway` | `P093` | `W017_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT | NO |
| W018 | Source | `L89_SolarVectorTrackingGimbal` | `P039` | `W018_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT | NO |
| W018 | Target | `L89_HeliosMissionSequencingPLC` | `P223` | `W018_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT | NO |
| W019 | Source | `L89_ParticleCollimatorHead` | `P043` | `W019_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT | NO |
| W019 | Target | `L89_ChargedParticleSpectrometer` | `P032` | `W019_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT | NO |
| W020 | Source | `L89_PrimaryParticleCounter` | `P047` | `W020_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT | NO |
| W020 | Target | `L89_DistributedDetectorNetworkGateway` | `P093` | `W020_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT | NO |
| W021 | Source | `L89_SecondaryParticleCounter` | `P048` | `W021_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT | NO |
| W021 | Target | `L89_DistributedDetectorNetworkGateway` | `P091` | `W021_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT | NO |
| W022 | Source | `L89_DetectorPulseShaper` | `P051` | `W022_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT | NO |
| W022 | Target | `L89_DistributedDetectorNetworkGateway` | `P092` | `W022_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT | NO |
| W023 | Source | `L89_SpectralCalibrationSource` | `P055` | `W023_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT | NO |
| W023 | Target | `L89_DistributedDetectorNetworkGateway` | `P093` | `W023_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT | NO |
| W024 | Source | `L89_ParticleDetectorThermalShroud` | `P056` | `W024_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT | NO |
| W024 | Target | `L89_DistributedDetectorNetworkGateway` | `P091` | `W024_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT | NO |
| W025 | Source | `L89_DistributedDetectorNetworkGateway` | `P092` | `W025_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT | NO |
| W025 | Target | `L89_SpaceWeatherForecastConsole` | `P059` | `W025_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT | NO |
| W026 | Source | `L89_SpaceWeatherForecastConsole` | `P060` | `W026_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT | NO |
| W026 | Target | `L89_HeliosMissionSequencingPLC` | `P225` | `W026_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT | NO |
| W027 | Source | `L89_StormArrivalPredictionHologram` | `P061` | `W027_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT | NO |
| W027 | Target | `L89_RadiationDefenseSCADA` | `P226` | `W027_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT | NO |
| W028 | Source | `L89_DetectorSaturationFaultInjector` | `P086` | `W028_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT | NO |
| W028 | Target | `L89_DistributedDetectorNetworkGateway` | `P092` | `W028_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT | NO |
| W029 | Source | `L89_HeliosMissionSequencingPLC` | `P225` | `W029_OUT_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | OUTPUT | NO |
| W029 | Target | `L89_SolarVectorTrackingGimbal` | `P041` | `W029_IN_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | INPUT | NO |
| W030 | Source | `L89_SolarVectorTrackingGimbal` | `P039` | `W030_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT | NO |
| W030 | Target | `L89_ParticleCollimatorHead` | `P042` | `W030_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT | NO |
| W031 | Source | `L89_HeliosMissionSequencingPLC` | `P224` | `W031_OUT_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | OUTPUT | NO |
| W031 | Target | `L89_SolarEnergeticParticleTelescopeA` | `P026` | `W031_IN_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | INPUT | NO |
| W032 | Source | `L89_HeliosMissionSequencingPLC` | `P225` | `W032_OUT_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | OUTPUT | NO |
| W032 | Target | `L89_SolarEnergeticParticleTelescopeB` | `P030` | `W032_IN_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | INPUT | NO |
| W033 | Source | `L89_HeliosMissionSequencingPLC` | `P223` | `W033_OUT_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | OUTPUT | NO |
| W033 | Target | `L89_SpectralCalibrationSource` | `P053` | `W033_IN_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | INPUT | NO |
| W034 | Source | `L89_HeliosMissionSequencingPLC` | `P224` | `W034_OUT_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | OUTPUT | NO |
| W034 | Target | `L89_PrimaryParticleCounter` | `P046` | `W034_IN_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | INPUT | NO |
| W035 | Source | `L89_HeliosMissionSequencingPLC` | `P225` | `W035_OUT_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | OUTPUT | NO |
| W035 | Target | `L89_SecondaryParticleCounter` | `P050` | `W035_IN_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | INPUT | NO |
| W036 | Source | `L89_DistributedDetectorNetworkGateway` | `P091` | `W036_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT | NO |
| W036 | Target | `L89_DetectorRedundancyComparator` | `P097` | `W036_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT | NO |
| W037 | Source | `L89_DetectorRedundancyComparator` | `P098` | `W037_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT | NO |
| W037 | Target | `L89_HeliosMissionSequencingPLC` | `P224` | `W037_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT | NO |
| W038 | Source | `L89_DistributedDetectorNetworkGateway` | `P093` | `W038_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT | NO |
| W038 | Target | `L89_SolarStorm3DParticleHologram` | `P229` | `W038_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT | NO |
| W039 | Source | `L89_ExternalDosimeterArray` | `P064` | `W039_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT | NO |
| W039 | Target | `L89_DistributedDetectorNetworkGateway` | `P091` | `W039_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT | NO |
| W040 | Source | `L89_HabitatDosimeterArray` | `P068` | `W040_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT | NO |
| W040 | Target | `L89_DistributedDetectorNetworkGateway` | `P092` | `W040_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT | NO |
| W041 | Source | `L89_ShelterReferenceDosimeter` | `P072` | `W041_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT | NO |
| W041 | Target | `L89_HeliosSafetyPLC` | `P222` | `W041_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT | NO |
| W042 | Source | `L89_IndependentShelterSafetyDosimeter` | `P073` | `W042_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT | NO |
| W042 | Target | `L89_HeliosSafetyPLC` | `P220` | `W042_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT | NO |
| W043 | Source | `L89_TriaxialRadiationDoseProbe` | `P077` | `W043_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT | NO |
| W043 | Target | `L89_DistributedDetectorNetworkGateway` | `P092` | `W043_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT | NO |
| W044 | Source | `L89_DoseRateTransientRecorder` | `P081` | `W044_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT | NO |
| W044 | Target | `L89_DistributedDetectorNetworkGateway` | `P093` | `W044_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT | NO |
| W045 | Source | `L89_DosimeterCalibrationFrame` | `P082` | `W045_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT | NO |
| W045 | Target | `L89_ShelterReferenceDosimeter` | `P070` | `W045_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT | NO |
| W046 | Source | `L89_DosimeterCalibrationFrame` | `P083` | `W046_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT | NO |
| W046 | Target | `L89_IndependentShelterSafetyDosimeter` | `P074` | `W046_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT | NO |
| W047 | Source | `L89_DoseAccumulationAnalyzer` | `P090` | `W047_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT | NO |
| W047 | Target | `L89_ShelterDoseLimitEvaluator` | `P102` | `W047_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT | NO |
| W048 | Source | `L89_DistributedDetectorNetworkGateway` | `P091` | `W048_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT | NO |
| W048 | Target | `L89_DetectorRedundancyComparator` | `P097` | `W048_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT | NO |
| W049 | Source | `L89_DetectorRedundancyComparator` | `P098` | `W049_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT | NO |
| W049 | Target | `L89_ShelterDoseLimitEvaluator` | `P101` | `W049_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT | NO |
| W050 | Source | `L89_ShelterDoseLimitEvaluator` | `P102` | `W050_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT | NO |
| W050 | Target | `L89_HeliosSafetyPLC` | `P222` | `W050_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT | NO |
| W051 | Source | `L89_ExternalDosimeterArray` | `P064` | `W051_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT | NO |
| W051 | Target | `L89_RadiationHeatmapHologram` | `P094` | `W051_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT | NO |
| W052 | Source | `L89_HabitatDosimeterArray` | `P068` | `W052_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT | NO |
| W052 | Target | `L89_RadiationHeatmapHologram` | `P095` | `W052_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT | NO |
| W053 | Source | `L89_ShelterReferenceDosimeter` | `P072` | `W053_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT | NO |
| W053 | Target | `L89_RadiationDefenseSCADA` | `P228` | `W053_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT | NO |
| W054 | Source | `L89_IndependentShelterSafetyDosimeter` | `P073` | `W054_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT | NO |
| W054 | Target | `L89_RadiationDefenseSCADA` | `P226` | `W054_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT | NO |
| W055 | Source | `L89_HeliosMissionSequencingPLC` | `P224` | `W055_OUT_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | OUTPUT | NO |
| W055 | Target | `L89_DosimeterCalibrationFrame` | `P083` | `W055_IN_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | INPUT | NO |
| W056 | Source | `L89_HeliosMissionSequencingPLC` | `P225` | `W056_OUT_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | OUTPUT | NO |
| W056 | Target | `L89_DoseRateTransientRecorder` | `P081` | `W056_IN_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | INPUT | NO |
| W057 | Source | `L89_HeliosMissionSequencingPLC` | `P223` | `W057_OUT_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | OUTPUT | NO |
| W057 | Target | `L89_TriaxialRadiationDoseProbe` | `P076` | `W057_IN_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | INPUT | NO |
| W058 | Source | `L89_DistributedDetectorNetworkGateway` | `P092` | `W058_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT | NO |
| W058 | Target | `L89_DoseAccumulationAnalyzer` | `P089` | `W058_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT | NO |
| W059 | Source | `L89_DoseAccumulationAnalyzer` | `P090` | `W059_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT | NO |
| W059 | Target | `L89_RadiationDefenseSCADA` | `P228` | `W059_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT | NO |
| W060 | Source | `L89_ShelterDoseLimitEvaluator` | `P100` | `W060_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT | NO |
| W060 | Target | `L89_HeliosMissionSequencingPLC` | `P223` | `W060_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT | NO |
| W061 | Source | `L89_DetectorSaturationFaultInjector` | `P086` | `W061_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT | NO |
| W061 | Target | `L89_HeliosMissionSequencingPLC` | `P224` | `W061_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT | NO |
| W062 | Source | `L89_HeliosMissionSequencingPLC` | `P225` | `W062_OUT_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | OUTPUT | NO |
| W062 | Target | `L89_DetectorSaturationFaultInjector` | `P087` | `W062_IN_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | INPUT | NO |
| W063 | Source | `L89_WaterMassAllocationController` | `P129` | `W063_OUT_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | OUTPUT | NO |
| W063 | Target | `L89_WaterWallTankPortA` | `P103` | `W063_IN_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | INPUT | NO |
| W064 | Source | `L89_WaterMassAllocationController` | `P130` | `W064_OUT_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | OUTPUT | NO |
| W064 | Target | `L89_WaterWallTankPortB` | `P107` | `W064_IN_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | INPUT | NO |
| W065 | Source | `L89_WaterMassAllocationController` | `P131` | `W065_OUT_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | OUTPUT | NO |
| W065 | Target | `L89_WaterWallTankStarboardA` | `P111` | `W065_IN_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | INPUT | NO |
| W066 | Source | `L89_WaterMassAllocationController` | `P129` | `W066_OUT_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | OUTPUT | NO |
| W066 | Target | `L89_WaterWallTankStarboardB` | `P112` | `W066_IN_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | INPUT | NO |
| W067 | Source | `L89_WaterTankLevelSensorArray` | `P118` | `W067_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT | NO |
| W067 | Target | `L89_WaterMassAllocationController` | `P130` | `W067_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT | NO |
| W068 | Source | `L89_WaterReservoirMain` | `P115` | `W068_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT | NO |
| W068 | Target | `L89_WaterTankLevelSensorArray` | `P119` | `W068_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT | NO |
| W069 | Source | `L89_WaterMassAllocationController` | `P129` | `W069_OUT_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | OUTPUT | NO |
| W069 | Target | `L89_WaterTransferPumpA` | `P120` | `W069_IN_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | INPUT | NO |
| W070 | Source | `L89_WaterMassAllocationController` | `P130` | `W070_OUT_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | OUTPUT | NO |
| W070 | Target | `L89_WaterTransferPumpB` | `P124` | `W070_IN_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | INPUT | NO |
| W071 | Source | `L89_WaterTransferPumpA` | `P122` | `W071_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT | NO |
| W071 | Target | `L89_WaterMassAllocationController` | `P131` | `W071_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT | NO |
| W072 | Source | `L89_WaterTransferPumpB` | `P123` | `W072_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT | NO |
| W072 | Target | `L89_WaterMassAllocationController` | `P129` | `W072_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT | NO |
| W073 | Source | `L89_WaterMassAllocationController` | `P130` | `W073_OUT_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | OUTPUT | NO |
| W073 | Target | `L89_WaterDistributionValveManifold` | `P127` | `W073_IN_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | INPUT | NO |
| W074 | Source | `L89_WaterDistributionValveManifold` | `P128` | `W074_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT | NO |
| W074 | Target | `L89_WaterMassAllocationController` | `P131` | `W074_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT | NO |
| W075 | Source | `L89_WaterWallTankPortA` | `P103` | `W075_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT | NO |
| W075 | Target | `L89_ShieldArealDensityAnalyzer` | `P132` | `W075_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT | NO |
| W076 | Source | `L89_WaterWallTankPortB` | `P107` | `W076_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT | NO |
| W076 | Target | `L89_ShieldArealDensityAnalyzer` | `P133` | `W076_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT | NO |
| W077 | Source | `L89_WaterWallTankStarboardA` | `P111` | `W077_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT | NO |
| W077 | Target | `L89_ShieldArealDensityAnalyzer` | `P134` | `W077_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT | NO |
| W078 | Source | `L89_WaterWallTankStarboardB` | `P112` | `W078_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT | NO |
| W078 | Target | `L89_ShieldArealDensityAnalyzer` | `P132` | `W078_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT | NO |
| W079 | Source | `L89_ShieldArealDensityAnalyzer` | `P133` | `W079_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT | NO |
| W079 | Target | `L89_ShieldingScenarioSafetyController` | `P213` | `W079_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT | NO |
| W080 | Source | `L89_ThermalWaterResourceGuard` | `P137` | `W080_OUT_SAFETY_FEEDBACK` | `SAFETY_FEEDBACK` | OUTPUT | NO |
| W080 | Target | `L89_WaterMassAllocationController` | `P131` | `W080_IN_SAFETY_FEEDBACK` | `SAFETY_FEEDBACK` | INPUT | NO |
| W081 | Source | `L89_WaterWallLeakMonitor` | `P139` | `W081_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT | NO |
| W081 | Target | `L89_WaterMassAllocationController` | `P129` | `W081_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT | NO |
| W082 | Source | `L89_ShelterProtectedLifeSupportPLC` | `P195` | `W082_OUT_SAFETY_FEEDBACK` | `SAFETY_FEEDBACK` | OUTPUT | NO |
| W082 | Target | `L89_ThermalWaterResourceGuard` | `P136` | `W082_IN_SAFETY_FEEDBACK` | `SAFETY_FEEDBACK` | INPUT | NO |
| W083 | Source | `L89_WaterMassAllocationController` | `P131` | `W083_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT | NO |
| W083 | Target | `L89_WaterPowerPriorityScheduler` | `P202` | `W083_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT | NO |
| W084 | Source | `L89_HeliosMissionSequencingPLC` | `P223` | `W084_OUT_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | OUTPUT | NO |
| W084 | Target | `L89_WaterMassAllocationController` | `P129` | `W084_IN_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | INPUT | NO |
| W085 | Source | `L89_ShieldMotorDriveCabinet` | `P173` | `W085_OUT_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | OUTPUT | NO |
| W085 | Target | `L89_ServoShieldActuatorA` | `P155` | `W085_IN_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | INPUT | NO |
| W086 | Source | `L89_ShieldMotorDriveCabinet` | `P174` | `W086_OUT_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | OUTPUT | NO |
| W086 | Target | `L89_ServoShieldActuatorB` | `P159` | `W086_IN_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | INPUT | NO |
| W087 | Source | `L89_ShieldMotorDriveCabinet` | `P172` | `W087_OUT_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | OUTPUT | NO |
| W087 | Target | `L89_ServoShieldActuatorC` | `P160` | `W087_IN_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | INPUT | NO |
| W088 | Source | `L89_ShieldMotorDriveCabinet` | `P173` | `W088_OUT_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | OUTPUT | NO |
| W088 | Target | `L89_ServoShieldActuatorD` | `P164` | `W088_IN_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | INPUT | NO |
| W089 | Source | `L89_ServoShieldActuatorA` | `P156` | `W089_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT | NO |
| W089 | Target | `L89_ShieldPositionEncoderRack` | `P171` | `W089_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT | NO |
| W090 | Source | `L89_ServoShieldActuatorB` | `P157` | `W090_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT | NO |
| W090 | Target | `L89_ShieldPositionEncoderRack` | `P169` | `W090_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT | NO |
| W091 | Source | `L89_ServoShieldActuatorC` | `P161` | `W091_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT | NO |
| W091 | Target | `L89_ShieldPositionEncoderRack` | `P170` | `W091_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT | NO |
| W092 | Source | `L89_ServoShieldActuatorD` | `P165` | `W092_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT | NO |
| W092 | Target | `L89_ShieldPositionEncoderRack` | `P171` | `W092_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT | NO |
| W093 | Source | `L89_ShieldPositionEncoderRack` | `P169` | `W093_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT | NO |
| W093 | Target | `L89_ShieldingScenarioSafetyController` | `P212` | `W093_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT | NO |
| W094 | Source | `L89_ShieldMotorDriveCabinet` | `P173` | `W094_OUT_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | OUTPUT | NO |
| W094 | Target | `L89_ShieldPanelTop` | `P141` | `W094_IN_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | INPUT | NO |
| W095 | Source | `L89_ShieldMotorDriveCabinet` | `P174` | `W095_OUT_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | OUTPUT | NO |
| W095 | Target | `L89_ShieldPanelBottom` | `P145` | `W095_IN_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | INPUT | NO |
| W096 | Source | `L89_ShieldMotorDriveCabinet` | `P172` | `W096_OUT_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | OUTPUT | NO |
| W096 | Target | `L89_ShieldPanelPort` | `P146` | `W096_IN_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | INPUT | NO |
| W097 | Source | `L89_ShieldMotorDriveCabinet` | `P173` | `W097_OUT_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | OUTPUT | NO |
| W097 | Target | `L89_ShieldPanelStarboard` | `P150` | `W097_IN_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | INPUT | NO |
| W098 | Source | `L89_ShieldPanelTop` | `P142` | `W098_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT | NO |
| W098 | Target | `L89_ShelterRadiationShieldQualityMonitor` | `P182` | `W098_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT | NO |
| W099 | Source | `L89_ShieldPanelBottom` | `P143` | `W099_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT | NO |
| W099 | Target | `L89_ShelterRadiationShieldQualityMonitor` | `P180` | `W099_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT | NO |
| W100 | Source | `L89_ShieldPanelPort` | `P147` | `W100_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT | NO |
| W100 | Target | `L89_ShelterRadiationShieldQualityMonitor` | `P181` | `W100_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT | NO |
| W101 | Source | `L89_ShieldPanelStarboard` | `P151` | `W101_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT | NO |
| W101 | Target | `L89_ShelterRadiationShieldQualityMonitor` | `P182` | `W101_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT | NO |
| W102 | Source | `L89_ShieldingScenarioSafetyController` | `P212` | `W102_OUT_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | OUTPUT | NO |
| W102 | Target | `L89_ShieldLockingPinArray` | `P166` | `W102_IN_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | INPUT | NO |
| W103 | Source | `L89_ShieldLockingPinArray` | `P167` | `W103_OUT_SAFETY_FEEDBACK` | `SAFETY_FEEDBACK` | OUTPUT | NO |
| W103 | Target | `L89_ShieldingScenarioSafetyController` | `P213` | `W103_IN_SAFETY_FEEDBACK` | `SAFETY_FEEDBACK` | INPUT | NO |
| W104 | Source | `L89_ShieldJammingFaultInjector` | `P177` | `W104_OUT_SAFETY_FEEDBACK` | `SAFETY_FEEDBACK` | OUTPUT | NO |
| W104 | Target | `L89_ShieldMotorDriveCabinet` | `P174` | `W104_IN_SAFETY_FEEDBACK` | `SAFETY_FEEDBACK` | INPUT | NO |
| W105 | Source | `L89_HeliosMissionSequencingPLC` | `P223` | `W105_OUT_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | OUTPUT | NO |
| W105 | Target | `L89_ShieldJammingFaultInjector` | `P175` | `W105_IN_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | INPUT | NO |
| W106 | Source | `L89_ShelterRadiationShieldQualityMonitor` | `P181` | `W106_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT | NO |
| W106 | Target | `L89_ShieldCoverageDigitalTwin` | `P184` | `W106_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT | NO |
| W107 | Source | `L89_SpaceWeatherForecastConsole` | `P060` | `W107_OUT_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | OUTPUT | NO |
| W107 | Target | `L89_StormIntensityProfileGenerator` | `P208` | `W107_IN_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | INPUT | NO |
| W108 | Source | `L89_StormIntensityProfileGenerator` | `P206` | `W108_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT | NO |
| W108 | Target | `L89_HeliosMissionSequencingPLC` | `P223` | `W108_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT | NO |
| W109 | Source | `L89_StormArrivalPredictionHologram` | `P062` | `W109_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT | NO |
| W109 | Target | `L89_HeliosMissionSequencingPLC` | `P224` | `W109_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT | NO |
| W110 | Source | `L89_ShieldArealDensityAnalyzer` | `P134` | `W110_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT | NO |
| W110 | Target | `L89_WaterPowerPriorityScheduler` | `P202` | `W110_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT | NO |
| W111 | Source | `L89_ThermalWaterResourceGuard` | `P135` | `W111_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT | NO |
| W111 | Target | `L89_WaterPowerPriorityScheduler` | `P200` | `W111_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT | NO |
| W112 | Source | `L89_ShelterEnvironmentalAirSupply` | `P187` | `W112_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT | NO |
| W112 | Target | `L89_ShelterProtectedLifeSupportPLC` | `P195` | `W112_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT | NO |
| W113 | Source | `L89_ShelterOxygenBufferInterface` | `P190` | `W113_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT | NO |
| W113 | Target | `L89_ShelterProtectedLifeSupportPLC` | `P196` | `W113_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT | NO |
| W114 | Source | `L89_ShelterThermalAirReclaimer` | `P191` | `W114_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT | NO |
| W114 | Target | `L89_ShelterProtectedLifeSupportPLC` | `P194` | `W114_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT | NO |
| W115 | Source | `L89_ShelterProtectedLifeSupportPLC` | `P195` | `W115_OUT_SAFETY_FEEDBACK` | `SAFETY_FEEDBACK` | OUTPUT | NO |
| W115 | Target | `L89_HeliosSafetyPLC` | `P221` | `W115_IN_SAFETY_FEEDBACK` | `SAFETY_FEEDBACK` | INPUT | NO |
| W116 | Source | `L89_ShelterReservePowerDistribution` | `P199` | `W116_OUT_SAFETY_FEEDBACK` | `SAFETY_FEEDBACK` | OUTPUT | NO |
| W116 | Target | `L89_ShelterProtectedLifeSupportPLC` | `P196` | `W116_IN_SAFETY_FEEDBACK` | `SAFETY_FEEDBACK` | INPUT | NO |
| W117 | Source | `L89_WaterPowerPriorityScheduler` | `P200` | `W117_OUT_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | OUTPUT | NO |
| W117 | Target | `L89_ShelterReservePowerDistribution` | `P197` | `W117_IN_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | INPUT | NO |
| W118 | Source | `L89_HabitatCrewShelterOccupancyEmulator` | `P204` | `W118_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT | NO |
| W118 | Target | `L89_WaterPowerPriorityScheduler` | `P201` | `W118_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT | NO |
| W119 | Source | `L89_StormIntensityProfileGenerator` | `P208` | `W119_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT | NO |
| W119 | Target | `L89_ShelterDoseLimitEvaluator` | `P102` | `W119_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT | NO |
| W120 | Source | `L89_HighEnergySurgeFaultInjector` | `P209` | `W120_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT | NO |
| W120 | Target | `L89_HeliosMissionSequencingPLC` | `P223` | `W120_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT | NO |
| W121 | Source | `L89_HeliosMissionSequencingPLC` | `P224` | `W121_OUT_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | OUTPUT | NO |
| W121 | Target | `L89_HighEnergySurgeFaultInjector` | `P210` | `W121_IN_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | INPUT | NO |
| W122 | Source | `L89_HeliosMissionSequencingPLC` | `P225` | `W122_OUT_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | OUTPUT | NO |
| W122 | Target | `L89_HabitatCrewShelterOccupancyEmulator` | `P205` | `W122_IN_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | INPUT | NO |
| W123 | Source | `L89_HeliosMissionSequencingPLC` | `P223` | `W123_OUT_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | OUTPUT | NO |
| W123 | Target | `L89_ShelterEnvironmentalAirSupply` | `P186` | `W123_IN_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | INPUT | NO |
| W124 | Source | `L89_HeliosMissionSequencingPLC` | `P224` | `W124_OUT_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | OUTPUT | NO |
| W124 | Target | `L89_ShelterThermalAirReclaimer` | `P192` | `W124_IN_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | INPUT | NO |
| W125 | Source | `L89_ShieldingScenarioSafetyController` | `P214` | `W125_OUT_SAFETY_FEEDBACK` | `SAFETY_FEEDBACK` | OUTPUT | NO |
| W125 | Target | `L89_HeliosMissionSequencingPLC` | `P225` | `W125_IN_SAFETY_FEEDBACK` | `SAFETY_FEEDBACK` | INPUT | NO |
| W126 | Source | `L89_HeliosMissionSequencingPLC` | `P223` | `W126_OUT_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | OUTPUT | NO |
| W126 | Target | `L89_ShelterRadiationContingencyConsole` | `P233` | `W126_IN_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | INPUT | NO |
| W127 | Source | `L89_ShelterRadiationContingencyConsole` | `P234` | `W127_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT | NO |
| W127 | Target | `L89_HeliosMissionSequencingPLC` | `P224` | `W127_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT | NO |
| W128 | Source | `L89_StormIntensityProfileGenerator` | `P208` | `W128_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT | NO |
| W128 | Target | `L89_SolarStorm3DParticleHologram` | `P230` | `W128_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT | NO |
| W129 | Source | `L89_ShelterDoorActualInterlock` | `P179` | `W129_OUT_SAFETY_FEEDBACK` | `SAFETY_FEEDBACK` | OUTPUT | NO |
| W129 | Target | `L89_HeliosSafetyPLC` | `P220` | `W129_IN_SAFETY_FEEDBACK` | `SAFETY_FEEDBACK` | INPUT | NO |
| W130 | Source | `L89_ShieldLockingPinArray` | `P167` | `W130_OUT_SAFETY_FEEDBACK` | `SAFETY_FEEDBACK` | OUTPUT | NO |
| W130 | Target | `L89_HeliosSafetyPLC` | `P221` | `W130_IN_SAFETY_FEEDBACK` | `SAFETY_FEEDBACK` | INPUT | NO |
| W131 | Source | `L89_ShelterRadiationShieldQualityMonitor` | `P182` | `W131_OUT_SAFETY_FEEDBACK` | `SAFETY_FEEDBACK` | OUTPUT | NO |
| W131 | Target | `L89_HeliosSafetyPLC` | `P222` | `W131_IN_SAFETY_FEEDBACK` | `SAFETY_FEEDBACK` | INPUT | NO |
| W132 | Source | `L89_IndependentShelterSafetyDosimeter` | `P073` | `W132_OUT_SAFETY_FEEDBACK` | `SAFETY_FEEDBACK` | OUTPUT | NO |
| W132 | Target | `L89_HeliosSafetyPLC` | `P220` | `W132_IN_SAFETY_FEEDBACK` | `SAFETY_FEEDBACK` | INPUT | NO |
| W133 | Source | `L89_ShelterProtectedLifeSupportPLC` | `P195` | `W133_OUT_SAFETY_FEEDBACK` | `SAFETY_FEEDBACK` | OUTPUT | NO |
| W133 | Target | `L89_HeliosSafetyPLC` | `P221` | `W133_IN_SAFETY_FEEDBACK` | `SAFETY_FEEDBACK` | INPUT | NO |
| W134 | Source | `L89_ShelterReservePowerDistribution` | `P199` | `W134_OUT_SAFETY_FEEDBACK` | `SAFETY_FEEDBACK` | OUTPUT | NO |
| W134 | Target | `L89_HeliosSafetyPLC` | `P222` | `W134_IN_SAFETY_FEEDBACK` | `SAFETY_FEEDBACK` | INPUT | NO |
| W135 | Source | `L89_ShieldingScenarioSafetyController` | `P212` | `W135_OUT_SAFETY_FEEDBACK` | `SAFETY_FEEDBACK` | OUTPUT | NO |
| W135 | Target | `L89_HeliosSafetyPLC` | `P220` | `W135_IN_SAFETY_FEEDBACK` | `SAFETY_FEEDBACK` | INPUT | NO |
| W136 | Source | `L89_RadiationEmergencyIsolationStation` | `P215` | `W136_OUT_SAFETY_FEEDBACK` | `SAFETY_FEEDBACK` | OUTPUT | NO |
| W136 | Target | `L89_HeliosSafetyPLC` | `P221` | `W136_IN_SAFETY_FEEDBACK` | `SAFETY_FEEDBACK` | INPUT | NO |
| W137 | Source | `L89_HeliosSafetyPLC` | `P222` | `W137_OUT_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | OUTPUT | NO |
| W137 | Target | `L89_ShieldMotorDriveCabinet` | `P174` | `W137_IN_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | INPUT | NO |
| W138 | Source | `L89_HeliosSafetyPLC` | `P220` | `W138_OUT_SAFETY_FEEDBACK` | `SAFETY_FEEDBACK` | OUTPUT | NO |
| W138 | Target | `L89_HeliosMissionSequencingPLC` | `P223` | `W138_IN_SAFETY_FEEDBACK` | `SAFETY_FEEDBACK` | INPUT | NO |
| W139 | Source | `L89_HeliosSafetyPLC` | `P221` | `W139_OUT_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | OUTPUT | NO |
| W139 | Target | `L89_ShieldingScenarioSafetyController` | `P213` | `W139_IN_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | INPUT | NO |
| W140 | Source | `L89_HeliosSafetyPLC` | `P222` | `W140_OUT_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | OUTPUT | NO |
| W140 | Target | `L89_ShelterProtectedLifeSupportPLC` | `P196` | `W140_IN_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | INPUT | NO |
| W141 | Source | `L89_HeliosSafetyPLC` | `P220` | `W141_OUT_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | OUTPUT | NO |
| W141 | Target | `L89_RadiationEmergencyIsolationStation` | `P215` | `W141_IN_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | INPUT | NO |
| W142 | Source | `L89_HeliosSafetyPLC` | `P221` | `W142_OUT_SAFETY_FEEDBACK` | `SAFETY_FEEDBACK` | OUTPUT | NO |
| W142 | Target | `L89_ShelterDoseLimitEvaluator` | `P101` | `W142_IN_SAFETY_FEEDBACK` | `SAFETY_FEEDBACK` | INPUT | NO |
| W143 | Source | `L89_HeliosSafetyPLC` | `P222` | `W143_OUT_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | OUTPUT | NO |
| W143 | Target | `L89_ShelterRadiationContingencyConsole` | `P235` | `W143_IN_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | INPUT | NO |
| W144 | Source | `L89_RadiationScenarioDiagnosticRecorder` | `P238` | `W144_OUT_SAFETY_FEEDBACK` | `SAFETY_FEEDBACK` | OUTPUT | NO |
| W144 | Target | `L89_HeliosSafetyPLC` | `P220` | `W144_IN_SAFETY_FEEDBACK` | `SAFETY_FEEDBACK` | INPUT | NO |
| W145 | Source | `L89_HeliosSafetyPLC` | `P221` | `W145_OUT_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | OUTPUT | NO |
| W145 | Target | `L89_RadiationScenarioDiagnosticRecorder` | `P239` | `W145_IN_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | INPUT | NO |
| W146 | Source | `L89_HeliosSafetyPLC` | `P222` | `W146_OUT_SAFETY_FEEDBACK` | `SAFETY_FEEDBACK` | OUTPUT | NO |
| W146 | Target | `L89_W149TrustedDoseFeedbackPatchStation` | `P236` | `W146_IN_SAFETY_FEEDBACK` | `SAFETY_FEEDBACK` | INPUT | NO |
| W147 | Source | `L89_HeliosMissionSequencingPLC` | `P223` | `W147_OUT_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | OUTPUT | NO |
| W147 | Target | `L89_HeliosSafetyPLC` | `P220` | `W147_IN_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | INPUT | NO |
| W148 | Source | `L89_HeliosSafetyPLC` | `P221` | `W148_OUT_SAFETY_FEEDBACK` | `SAFETY_FEEDBACK` | OUTPUT | NO |
| W148 | Target | `L89_HeliosMissionSequencingPLC` | `P224` | `W148_IN_SAFETY_FEEDBACK` | `SAFETY_FEEDBACK` | INPUT | NO |
| W149 | Source | `L89_ShelterReferenceDosimeter` | `P072` | `TRUSTED_SHELTER_DOSE_RATE_OUT` | `ANALOG_FEEDBACK` | OUTPUT | YES |
| W149 | Target | `L89_HeliosSafetyPLC` | `P222` | `TRUSTED_SHELTER_DOSE_RATE_IN` | `ANALOG_FEEDBACK` | INPUT | YES |
| W150 | Source | `L89_ShieldingScenarioSafetyController` | `P212` | `W150_OUT_SAFETY_FEEDBACK` | `SAFETY_FEEDBACK` | OUTPUT | NO |
| W150 | Target | `L89_ShieldCoverageDigitalTwin` | `P183` | `W150_IN_SAFETY_FEEDBACK` | `SAFETY_FEEDBACK` | INPUT | NO |
| W151 | Source | `L89_HeliosSafetyPLC` | `P221` | `W151_OUT_SAFETY_FEEDBACK` | `SAFETY_FEEDBACK` | OUTPUT | NO |
| W151 | Target | `L89_EmergencyShelterCommunications` | `P244` | `W151_IN_SAFETY_FEEDBACK` | `SAFETY_FEEDBACK` | INPUT | NO |
| W152 | Source | `L89_EmergencyShelterCommunications` | `P244` | `W152_OUT_SAFETY_FEEDBACK` | `SAFETY_FEEDBACK` | OUTPUT | NO |
| W152 | Target | `L89_HeliosSafetyPLC` | `P222` | `W152_IN_SAFETY_FEEDBACK` | `SAFETY_FEEDBACK` | INPUT | NO |
| W153 | Source | `L89_HeliosSafetyPLC` | `P220` | `W153_OUT_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | OUTPUT | NO |
| W153 | Target | `L89_StormShelterCertificationBeacon` | `P242` | `W153_IN_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | INPUT | NO |
| W154 | Source | `L89_StormIntensityProfileGenerator` | `P207` | `W154_OUT_SAFETY_FEEDBACK` | `SAFETY_FEEDBACK` | OUTPUT | NO |
| W154 | Target | `L89_HeliosSafetyPLC` | `P221` | `W154_IN_SAFETY_FEEDBACK` | `SAFETY_FEEDBACK` | INPUT | NO |
| W155 | Source | `L89_PrimaryParticleCounter` | `P047` | `W155_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT | NO |
| W155 | Target | `L89_RadiationDefenseSCADA` | `P228` | `W155_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT | NO |
| W156 | Source | `L89_SecondaryParticleCounter` | `P048` | `W156_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT | NO |
| W156 | Target | `L89_RadiationDefenseSCADA` | `P226` | `W156_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT | NO |
| W157 | Source | `L89_DoseAccumulationAnalyzer` | `P089` | `W157_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT | NO |
| W157 | Target | `L89_RadiationDefenseSCADA` | `P227` | `W157_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT | NO |
| W158 | Source | `L89_ShelterDoseLimitEvaluator` | `P102` | `W158_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT | NO |
| W158 | Target | `L89_RadiationDefenseSCADA` | `P228` | `W158_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT | NO |
| W159 | Source | `L89_WaterTankLevelSensorArray` | `P117` | `W159_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT | NO |
| W159 | Target | `L89_RadiationDefenseSCADA` | `P226` | `W159_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT | NO |
| W160 | Source | `L89_WaterMassAllocationController` | `P130` | `W160_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT | NO |
| W160 | Target | `L89_RadiationDefenseSCADA` | `P227` | `W160_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT | NO |
| W161 | Source | `L89_ShieldPositionEncoderRack` | `P171` | `W161_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT | NO |
| W161 | Target | `L89_RadiationDefenseSCADA` | `P228` | `W161_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT | NO |
| W162 | Source | `L89_ShelterRadiationShieldQualityMonitor` | `P180` | `W162_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT | NO |
| W162 | Target | `L89_RadiationDefenseSCADA` | `P226` | `W162_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT | NO |
| W163 | Source | `L89_ShelterEnvironmentalAirSupply` | `P187` | `W163_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT | NO |
| W163 | Target | `L89_RadiationDefenseSCADA` | `P227` | `W163_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT | NO |
| W164 | Source | `L89_ShelterProtectedLifeSupportPLC` | `P196` | `W164_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT | NO |
| W164 | Target | `L89_RadiationDefenseSCADA` | `P228` | `W164_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT | NO |
| W165 | Source | `L89_StormIntensityProfileGenerator` | `P206` | `W165_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT | NO |
| W165 | Target | `L89_RadiationDefenseSCADA` | `P226` | `W165_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT | NO |
| W166 | Source | `L89_ShieldingScenarioSafetyController` | `P213` | `W166_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT | NO |
| W166 | Target | `L89_RadiationDefenseSCADA` | `P227` | `W166_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT | NO |
| W167 | Source | `L89_HeliosSafetyPLC` | `P222` | `W167_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT | NO |
| W167 | Target | `L89_RadiationDefenseSCADA` | `P228` | `W167_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT | NO |
| W168 | Source | `L89_HeliosMissionSequencingPLC` | `P223` | `W168_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT | NO |
| W168 | Target | `L89_RadiationDefenseSCADA` | `P226` | `W168_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT | NO |
| W169 | Source | `L89_RadiationScenarioDiagnosticRecorder` | `P239` | `W169_OUT_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | OUTPUT | NO |
| W169 | Target | `L89_RadiationDefenseSCADA` | `P227` | `W169_IN_ANALOG_FEEDBACK` | `ANALOG_FEEDBACK` | INPUT | NO |
| W170 | Source | `L89_RadiationDefenseSCADA` | `P228` | `W170_OUT_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | OUTPUT | NO |
| W170 | Target | `L89_StormShelterCertificationBeacon` | `P242` | `W170_IN_DIGITAL_CONTROL` | `DIGITAL_CONTROL` | INPUT | NO |

## ANNEX F — ALL 68 SHARED SOCKET RECONCILIATION RECORDS

**35 of 68** shared source art sockets mix more than one signal class. Every conflicting independent contact must be split or wired through a proven suitable multi-contact design; identical source art coordinates are not evidence of a shared circuit node.

| Socket | Owner model | Logical contacts | Source signal kinds | Mixed? | Source requirement |
|---|---|---:|---|:---:|---|
| `P218` | `L89_Protected5VControlSupply` | 4 | `POWER_CONTROL` | False | `SPLIT_INDEPENDENT_CONTACTS_TO_AVOID_ELECTRICAL_SHORT` |
| `P221` | `L89_HeliosSafetyPLC` | 11 | `DIGITAL_CONTROL;POWER_CONTROL;SAFETY_FEEDBACK` | True | `SPLIT_INDEPENDENT_CONTACTS_TO_AVOID_ELECTRICAL_SHORT` |
| `P219` | `L89_Protected5VControlSupply` | 4 | `POWER_CONTROL` | False | `SPLIT_INDEPENDENT_CONTACTS_TO_AVOID_ELECTRICAL_SHORT` |
| `P225` | `L89_HeliosMissionSequencingPLC` | 9 | `ANALOG_FEEDBACK;DIGITAL_CONTROL;POWER_CONTROL;SAFETY_FEEDBACK` | True | `SPLIT_INDEPENDENT_CONTACTS_TO_AVOID_ELECTRICAL_SHORT` |
| `P217` | `L89_Protected5VControlSupply` | 4 | `POWER_CONTROL` | False | `SPLIT_INDEPENDENT_CONTACTS_TO_AVOID_ELECTRICAL_SHORT` |
| `P091` | `L89_DistributedDetectorNetworkGateway` | 6 | `ANALOG_FEEDBACK;POWER_CONTROL` | True | `SPLIT_INDEPENDENT_CONTACTS_TO_AVOID_ELECTRICAL_SHORT` |
| `P173` | `L89_ShieldMotorDriveCabinet` | 5 | `DIGITAL_CONTROL;POWER_CONTROL` | True | `SPLIT_INDEPENDENT_CONTACTS_TO_AVOID_ELECTRICAL_SHORT` |
| `P196` | `L89_ShelterProtectedLifeSupportPLC` | 5 | `ANALOG_FEEDBACK;DIGITAL_CONTROL;POWER_CONTROL;SAFETY_FEEDBACK` | True | `SPLIT_INDEPENDENT_CONTACTS_TO_AVOID_ELECTRICAL_SHORT` |
| `P226` | `L89_RadiationDefenseSCADA` | 8 | `ANALOG_FEEDBACK;POWER_CONTROL` | True | `SPLIT_INDEPENDENT_CONTACTS_TO_AVOID_ELECTRICAL_SHORT` |
| `P129` | `L89_WaterMassAllocationController` | 7 | `ANALOG_FEEDBACK;DIGITAL_CONTROL;POWER_CONTROL` | True | `SPLIT_INDEPENDENT_CONTACTS_TO_AVOID_ELECTRICAL_SHORT` |
| `P059` | `L89_SpaceWeatherForecastConsole` | 2 | `ANALOG_FEEDBACK;POWER_CONTROL` | True | `SPLIT_INDEPENDENT_CONTACTS_TO_AVOID_ELECTRICAL_SHORT` |
| `P222` | `L89_HeliosSafetyPLC` | 12 | `ANALOG_FEEDBACK;DIGITAL_CONTROL;POWER_CONTROL;SAFETY_FEEDBACK` | True | `SPLIT_INDEPENDENT_CONTACTS_TO_AVOID_ELECTRICAL_SHORT` |
| `P223` | `L89_HeliosMissionSequencingPLC` | 14 | `ANALOG_FEEDBACK;DIGITAL_CONTROL;POWER_CONTROL;SAFETY_FEEDBACK` | True | `SPLIT_INDEPENDENT_CONTACTS_TO_AVOID_ELECTRICAL_SHORT` |
| `P026` | `L89_SolarEnergeticParticleTelescopeA` | 2 | `ANALOG_FEEDBACK;DIGITAL_CONTROL` | True | `SPLIT_INDEPENDENT_CONTACTS_TO_AVOID_ELECTRICAL_SHORT` |
| `P092` | `L89_DistributedDetectorNetworkGateway` | 8 | `ANALOG_FEEDBACK` | False | `SPLIT_INDEPENDENT_CONTACTS_TO_AVOID_ELECTRICAL_SHORT` |
| `P030` | `L89_SolarEnergeticParticleTelescopeB` | 2 | `ANALOG_FEEDBACK;DIGITAL_CONTROL` | True | `SPLIT_INDEPENDENT_CONTACTS_TO_AVOID_ELECTRICAL_SHORT` |
| `P093` | `L89_DistributedDetectorNetworkGateway` | 6 | `ANALOG_FEEDBACK` | False | `SPLIT_INDEPENDENT_CONTACTS_TO_AVOID_ELECTRICAL_SHORT` |
| `P039` | `L89_SolarVectorTrackingGimbal` | 2 | `ANALOG_FEEDBACK` | False | `SPLIT_INDEPENDENT_CONTACTS_TO_AVOID_ELECTRICAL_SHORT` |
| `P047` | `L89_PrimaryParticleCounter` | 2 | `ANALOG_FEEDBACK` | False | `SPLIT_INDEPENDENT_CONTACTS_TO_AVOID_ELECTRICAL_SHORT` |
| `P048` | `L89_SecondaryParticleCounter` | 2 | `ANALOG_FEEDBACK` | False | `SPLIT_INDEPENDENT_CONTACTS_TO_AVOID_ELECTRICAL_SHORT` |
| `P060` | `L89_SpaceWeatherForecastConsole` | 2 | `ANALOG_FEEDBACK;DIGITAL_CONTROL` | True | `SPLIT_INDEPENDENT_CONTACTS_TO_AVOID_ELECTRICAL_SHORT` |
| `P086` | `L89_DetectorSaturationFaultInjector` | 2 | `ANALOG_FEEDBACK` | False | `SPLIT_INDEPENDENT_CONTACTS_TO_AVOID_ELECTRICAL_SHORT` |
| `P224` | `L89_HeliosMissionSequencingPLC` | 10 | `ANALOG_FEEDBACK;DIGITAL_CONTROL;SAFETY_FEEDBACK` | True | `SPLIT_INDEPENDENT_CONTACTS_TO_AVOID_ELECTRICAL_SHORT` |
| `P097` | `L89_DetectorRedundancyComparator` | 2 | `ANALOG_FEEDBACK` | False | `SPLIT_INDEPENDENT_CONTACTS_TO_AVOID_ELECTRICAL_SHORT` |
| `P098` | `L89_DetectorRedundancyComparator` | 2 | `ANALOG_FEEDBACK` | False | `SPLIT_INDEPENDENT_CONTACTS_TO_AVOID_ELECTRICAL_SHORT` |
| `P064` | `L89_ExternalDosimeterArray` | 2 | `ANALOG_FEEDBACK` | False | `SPLIT_INDEPENDENT_CONTACTS_TO_AVOID_ELECTRICAL_SHORT` |
| `P068` | `L89_HabitatDosimeterArray` | 2 | `ANALOG_FEEDBACK` | False | `SPLIT_INDEPENDENT_CONTACTS_TO_AVOID_ELECTRICAL_SHORT` |
| `P072` | `L89_ShelterReferenceDosimeter` | 3 | `ANALOG_FEEDBACK` | False | `SPLIT_INDEPENDENT_CONTACTS_TO_AVOID_ELECTRICAL_SHORT` |
| `P073` | `L89_IndependentShelterSafetyDosimeter` | 3 | `ANALOG_FEEDBACK;SAFETY_FEEDBACK` | True | `SPLIT_INDEPENDENT_CONTACTS_TO_AVOID_ELECTRICAL_SHORT` |
| `P220` | `L89_HeliosSafetyPLC` | 9 | `ANALOG_FEEDBACK;DIGITAL_CONTROL;SAFETY_FEEDBACK` | True | `SPLIT_INDEPENDENT_CONTACTS_TO_AVOID_ELECTRICAL_SHORT` |
| `P081` | `L89_DoseRateTransientRecorder` | 2 | `ANALOG_FEEDBACK;DIGITAL_CONTROL` | True | `SPLIT_INDEPENDENT_CONTACTS_TO_AVOID_ELECTRICAL_SHORT` |
| `P083` | `L89_DosimeterCalibrationFrame` | 2 | `ANALOG_FEEDBACK;DIGITAL_CONTROL` | True | `SPLIT_INDEPENDENT_CONTACTS_TO_AVOID_ELECTRICAL_SHORT` |
| `P090` | `L89_DoseAccumulationAnalyzer` | 2 | `ANALOG_FEEDBACK` | False | `SPLIT_INDEPENDENT_CONTACTS_TO_AVOID_ELECTRICAL_SHORT` |
| `P102` | `L89_ShelterDoseLimitEvaluator` | 4 | `ANALOG_FEEDBACK` | False | `SPLIT_INDEPENDENT_CONTACTS_TO_AVOID_ELECTRICAL_SHORT` |
| `P101` | `L89_ShelterDoseLimitEvaluator` | 2 | `ANALOG_FEEDBACK;SAFETY_FEEDBACK` | True | `SPLIT_INDEPENDENT_CONTACTS_TO_AVOID_ELECTRICAL_SHORT` |
| `P228` | `L89_RadiationDefenseSCADA` | 8 | `ANALOG_FEEDBACK;DIGITAL_CONTROL` | True | `SPLIT_INDEPENDENT_CONTACTS_TO_AVOID_ELECTRICAL_SHORT` |
| `P089` | `L89_DoseAccumulationAnalyzer` | 2 | `ANALOG_FEEDBACK` | False | `SPLIT_INDEPENDENT_CONTACTS_TO_AVOID_ELECTRICAL_SHORT` |
| `P103` | `L89_WaterWallTankPortA` | 2 | `ANALOG_FEEDBACK;DIGITAL_CONTROL` | True | `SPLIT_INDEPENDENT_CONTACTS_TO_AVOID_ELECTRICAL_SHORT` |
| `P130` | `L89_WaterMassAllocationController` | 5 | `ANALOG_FEEDBACK;DIGITAL_CONTROL` | True | `SPLIT_INDEPENDENT_CONTACTS_TO_AVOID_ELECTRICAL_SHORT` |
| `P107` | `L89_WaterWallTankPortB` | 2 | `ANALOG_FEEDBACK;DIGITAL_CONTROL` | True | `SPLIT_INDEPENDENT_CONTACTS_TO_AVOID_ELECTRICAL_SHORT` |
| `P131` | `L89_WaterMassAllocationController` | 5 | `ANALOG_FEEDBACK;DIGITAL_CONTROL;SAFETY_FEEDBACK` | True | `SPLIT_INDEPENDENT_CONTACTS_TO_AVOID_ELECTRICAL_SHORT` |
| `P111` | `L89_WaterWallTankStarboardA` | 2 | `ANALOG_FEEDBACK;DIGITAL_CONTROL` | True | `SPLIT_INDEPENDENT_CONTACTS_TO_AVOID_ELECTRICAL_SHORT` |
| `P112` | `L89_WaterWallTankStarboardB` | 2 | `ANALOG_FEEDBACK;DIGITAL_CONTROL` | True | `SPLIT_INDEPENDENT_CONTACTS_TO_AVOID_ELECTRICAL_SHORT` |
| `P132` | `L89_ShieldArealDensityAnalyzer` | 2 | `ANALOG_FEEDBACK` | False | `SPLIT_INDEPENDENT_CONTACTS_TO_AVOID_ELECTRICAL_SHORT` |
| `P133` | `L89_ShieldArealDensityAnalyzer` | 2 | `ANALOG_FEEDBACK` | False | `SPLIT_INDEPENDENT_CONTACTS_TO_AVOID_ELECTRICAL_SHORT` |
| `P134` | `L89_ShieldArealDensityAnalyzer` | 2 | `ANALOG_FEEDBACK` | False | `SPLIT_INDEPENDENT_CONTACTS_TO_AVOID_ELECTRICAL_SHORT` |
| `P213` | `L89_ShieldingScenarioSafetyController` | 4 | `ANALOG_FEEDBACK;DIGITAL_CONTROL;SAFETY_FEEDBACK` | True | `SPLIT_INDEPENDENT_CONTACTS_TO_AVOID_ELECTRICAL_SHORT` |
| `P195` | `L89_ShelterProtectedLifeSupportPLC` | 4 | `ANALOG_FEEDBACK;SAFETY_FEEDBACK` | True | `SPLIT_INDEPENDENT_CONTACTS_TO_AVOID_ELECTRICAL_SHORT` |
| `P202` | `L89_WaterPowerPriorityScheduler` | 2 | `ANALOG_FEEDBACK` | False | `SPLIT_INDEPENDENT_CONTACTS_TO_AVOID_ELECTRICAL_SHORT` |
| `P174` | `L89_ShieldMotorDriveCabinet` | 4 | `DIGITAL_CONTROL;SAFETY_FEEDBACK` | True | `SPLIT_INDEPENDENT_CONTACTS_TO_AVOID_ELECTRICAL_SHORT` |
| `P172` | `L89_ShieldMotorDriveCabinet` | 2 | `DIGITAL_CONTROL` | False | `SPLIT_INDEPENDENT_CONTACTS_TO_AVOID_ELECTRICAL_SHORT` |
| `P171` | `L89_ShieldPositionEncoderRack` | 3 | `ANALOG_FEEDBACK` | False | `SPLIT_INDEPENDENT_CONTACTS_TO_AVOID_ELECTRICAL_SHORT` |
| `P169` | `L89_ShieldPositionEncoderRack` | 2 | `ANALOG_FEEDBACK` | False | `SPLIT_INDEPENDENT_CONTACTS_TO_AVOID_ELECTRICAL_SHORT` |
| `P212` | `L89_ShieldingScenarioSafetyController` | 4 | `ANALOG_FEEDBACK;DIGITAL_CONTROL;SAFETY_FEEDBACK` | True | `SPLIT_INDEPENDENT_CONTACTS_TO_AVOID_ELECTRICAL_SHORT` |
| `P182` | `L89_ShelterRadiationShieldQualityMonitor` | 3 | `ANALOG_FEEDBACK;SAFETY_FEEDBACK` | True | `SPLIT_INDEPENDENT_CONTACTS_TO_AVOID_ELECTRICAL_SHORT` |
| `P180` | `L89_ShelterRadiationShieldQualityMonitor` | 2 | `ANALOG_FEEDBACK` | False | `SPLIT_INDEPENDENT_CONTACTS_TO_AVOID_ELECTRICAL_SHORT` |
| `P181` | `L89_ShelterRadiationShieldQualityMonitor` | 2 | `ANALOG_FEEDBACK` | False | `SPLIT_INDEPENDENT_CONTACTS_TO_AVOID_ELECTRICAL_SHORT` |
| `P167` | `L89_ShieldLockingPinArray` | 2 | `SAFETY_FEEDBACK` | False | `SPLIT_INDEPENDENT_CONTACTS_TO_AVOID_ELECTRICAL_SHORT` |
| `P208` | `L89_StormIntensityProfileGenerator` | 3 | `ANALOG_FEEDBACK;DIGITAL_CONTROL` | True | `SPLIT_INDEPENDENT_CONTACTS_TO_AVOID_ELECTRICAL_SHORT` |
| `P206` | `L89_StormIntensityProfileGenerator` | 2 | `ANALOG_FEEDBACK` | False | `SPLIT_INDEPENDENT_CONTACTS_TO_AVOID_ELECTRICAL_SHORT` |
| `P200` | `L89_WaterPowerPriorityScheduler` | 2 | `ANALOG_FEEDBACK;DIGITAL_CONTROL` | True | `SPLIT_INDEPENDENT_CONTACTS_TO_AVOID_ELECTRICAL_SHORT` |
| `P187` | `L89_ShelterEnvironmentalAirSupply` | 2 | `ANALOG_FEEDBACK` | False | `SPLIT_INDEPENDENT_CONTACTS_TO_AVOID_ELECTRICAL_SHORT` |
| `P199` | `L89_ShelterReservePowerDistribution` | 2 | `SAFETY_FEEDBACK` | False | `SPLIT_INDEPENDENT_CONTACTS_TO_AVOID_ELECTRICAL_SHORT` |
| `P215` | `L89_RadiationEmergencyIsolationStation` | 2 | `DIGITAL_CONTROL;SAFETY_FEEDBACK` | True | `SPLIT_INDEPENDENT_CONTACTS_TO_AVOID_ELECTRICAL_SHORT` |
| `P239` | `L89_RadiationScenarioDiagnosticRecorder` | 2 | `ANALOG_FEEDBACK;DIGITAL_CONTROL` | True | `SPLIT_INDEPENDENT_CONTACTS_TO_AVOID_ELECTRICAL_SHORT` |
| `P244` | `L89_EmergencyShelterCommunications` | 2 | `SAFETY_FEEDBACK` | False | `SPLIT_INDEPENDENT_CONTACTS_TO_AVOID_ELECTRICAL_SHORT` |
| `P242` | `L89_StormShelterCertificationBeacon` | 2 | `DIGITAL_CONTROL` | False | `SPLIT_INDEPENDENT_CONTACTS_TO_AVOID_ELECTRICAL_SHORT` |
| `P227` | `L89_RadiationDefenseSCADA` | 5 | `ANALOG_FEEDBACK` | False | `SPLIT_INDEPENDENT_CONTACTS_TO_AVOID_ELECTRICAL_SHORT` |

## ANNEX G — ALL SIX ORIGINAL OBJECTIVE CERTIFICATION RECORDS

Every source objective has `Implemented=NO`. Titles and criteria below are package-derived design inputs; implementing and testing them is required.

| # | Exact title | Source acceptance criteria | Planned evidence | Implemented in source? |
|---:|---|---|---|:---:|
| 1 | **Build the Helios Radiation Defense Network** | 170 real player-created wires; correct functional returns/terminals and protected 5 V, independent safety including W149 | Topology fingerprint and live circuit graph proof | NO |
| 2 | **Calibrate the Solar Radiation Observatory** | External, habitat, shelter and independent dosimeters, two particle telescopes, valid range/reference and fresh samples | Acquisition and calibration record with timestamps, channel health | NO |
| 3 | **Commission the Radiation Storm Shelter** | Finite water allocation and shielding coverage, real actuator positions and locks, life-support and essential power continuity | Mechanical feedback, water inventory, environmental margins and protected readiness | NO |
| 4 | **Survive the First Complete Solar Storm** | Fresh quiet→rise→peak→decay event; measured sheltered dose within fictional budget and essential continuity | New run ID, measured integrated dose and actual protected shelter-state history | NO |
| 5 | **Recover Three Radiation Defense Failures** | Detector saturation, jammed shield cassette and higher-energy storm surge all physically impact plant; repair, then run new qualified storm | Causal disturbance/repair event records and post-disturbance run ID | NO |
| 6 | **Restore Critical Shelter Radiation Feedback** | After historical 1–5 disconnect W149, safety inhibition/latched fault; reconnect, recalibrate, explicit reset, new entire storm | Real W149 live circuit-edge mutation, fresh trusted sample, safety permit and final LevelGamePlayManager 6/6 | NO |

## ANNEX H — EXACT SAFETY / W149 NEIGHBOR CONNECTIONS W129–W154

Source-only conceptual connections. Distinguish independent safety feedback and W149 from display, fault and operator actions.

| Wire | Source (socket) | Target (socket) | Kind | Intended purpose | Critical |
|---|---|---|---|---|:---:|
| **W129** | `L89_ShelterDoorActualInterlock` (`P179`) | `L89_HeliosSafetyPLC` (`P220`) | `SAFETY_FEEDBACK` | Actual shelter door closed and latched | NO |
| **W130** | `L89_ShieldLockingPinArray` (`P167`) | `L89_HeliosSafetyPLC` (`P221`) | `SAFETY_FEEDBACK` | Actual shield locking contact | NO |
| **W131** | `L89_ShelterRadiationShieldQualityMonitor` (`P182`) | `L89_HeliosSafetyPLC` (`P222`) | `SAFETY_FEEDBACK` | Shield coverage hardware health | NO |
| **W132** | `L89_IndependentShelterSafetyDosimeter` (`P073`) | `L89_HeliosSafetyPLC` (`P220`) | `SAFETY_FEEDBACK` | Independent radiation dose monitoring | NO |
| **W133** | `L89_ShelterProtectedLifeSupportPLC` (`P195`) | `L89_HeliosSafetyPLC` (`P221`) | `SAFETY_FEEDBACK` | Independent life support continuity | NO |
| **W134** | `L89_ShelterReservePowerDistribution` (`P199`) | `L89_HeliosSafetyPLC` (`P222`) | `SAFETY_FEEDBACK` | Essential shelter electrical power status | NO |
| **W135** | `L89_ShieldingScenarioSafetyController` (`P212`) | `L89_HeliosSafetyPLC` (`P220`) | `SAFETY_FEEDBACK` | Shielding adequacy permit | NO |
| **W136** | `L89_RadiationEmergencyIsolationStation` (`P215`) | `L89_HeliosSafetyPLC` (`P221`) | `SAFETY_FEEDBACK` | Emergency shelter stop input | NO |
| **W137** | `L89_HeliosSafetyPLC` (`P222`) | `L89_ShieldMotorDriveCabinet` (`P174`) | `DIGITAL_CONTROL` | Protected shield servo authorization | NO |
| **W138** | `L89_HeliosSafetyPLC` (`P220`) | `L89_HeliosMissionSequencingPLC` (`P223`) | `SAFETY_FEEDBACK` | Safety PLC overall release | NO |
| **W139** | `L89_HeliosSafetyPLC` (`P221`) | `L89_ShieldingScenarioSafetyController` (`P213`) | `DIGITAL_CONTROL` | Independent shielding enable request | NO |
| **W140** | `L89_HeliosSafetyPLC` (`P222`) | `L89_ShelterProtectedLifeSupportPLC` (`P196`) | `DIGITAL_CONTROL` | Emergency ventilation continuity permit | NO |
| **W141** | `L89_HeliosSafetyPLC` (`P220`) | `L89_RadiationEmergencyIsolationStation` (`P215`) | `DIGITAL_CONTROL` | Emergency isolation status request | NO |
| **W142** | `L89_HeliosSafetyPLC` (`P221`) | `L89_ShelterDoseLimitEvaluator` (`P101`) | `SAFETY_FEEDBACK` | Qualified sheltered dose ceiling permission | NO |
| **W143** | `L89_HeliosSafetyPLC` (`P222`) | `L89_ShelterRadiationContingencyConsole` (`P235`) | `DIGITAL_CONTROL` | Protected contingency power-mode command | NO |
| **W144** | `L89_RadiationScenarioDiagnosticRecorder` (`P238`) | `L89_HeliosSafetyPLC` (`P220`) | `SAFETY_FEEDBACK` | Fault recorder event timestamp | NO |
| **W145** | `L89_HeliosSafetyPLC` (`P221`) | `L89_RadiationScenarioDiagnosticRecorder` (`P239`) | `DIGITAL_CONTROL` | Latch safety trip event | NO |
| **W146** | `L89_HeliosSafetyPLC` (`P222`) | `L89_W149TrustedDoseFeedbackPatchStation` (`P236`) | `SAFETY_FEEDBACK` | Critical W149 patch circuit diagnostic | NO |
| **W147** | `L89_HeliosMissionSequencingPLC` (`P223`) | `L89_HeliosSafetyPLC` (`P220`) | `DIGITAL_CONTROL` | Operator explicit protection reset request | NO |
| **W148** | `L89_HeliosSafetyPLC` (`P221`) | `L89_HeliosMissionSequencingPLC` (`P224`) | `SAFETY_FEEDBACK` | Fault-latch event status | NO |
| **W149** | `L89_ShelterReferenceDosimeter` (`P072`) | `L89_HeliosSafetyPLC` (`P222`) | `ANALOG_FEEDBACK` | CRITICAL trusted shelter dose-rate measurement to independent radiation safety PLC | YES |
| **W150** | `L89_ShieldingScenarioSafetyController` (`P212`) | `L89_ShieldCoverageDigitalTwin` (`P183`) | `SAFETY_FEEDBACK` | Current shield coverage qualification | NO |
| **W151** | `L89_HeliosSafetyPLC` (`P221`) | `L89_EmergencyShelterCommunications` (`P244`) | `SAFETY_FEEDBACK` | Emergency crew warning authorisation | NO |
| **W152** | `L89_EmergencyShelterCommunications` (`P244`) | `L89_HeliosSafetyPLC` (`P222`) | `SAFETY_FEEDBACK` | Emergency communication equipment status | NO |
| **W153** | `L89_HeliosSafetyPLC` (`P220`) | `L89_StormShelterCertificationBeacon` (`P242`) | `DIGITAL_CONTROL` | Authorization to show final certification | NO |
| **W154** | `L89_StormIntensityProfileGenerator` (`P207`) | `L89_HeliosSafetyPLC` (`P221`) | `SAFETY_FEEDBACK` | Monitored actual storm state independent verification | NO |

## ANNEX I — ALL 17 EXACT PHYSICAL SHIELDING / EXPOSURE PROCESS CONCEPT LINKS

These are **planned physical or measurement-system dependencies**, not Spark player wires or implemented numerical solvers.

| # | Source | Target | Exact source meaning | Original status |
|---:|---|---|---|---|
| 1 | `ExternalParticleFlux` | `DetectorA/B` | Spectral samples, bounded dynamic range | `PLANNED_NOT_IMPLEMENTED` |
| 2 | `DetectorA/B` | `NavigationForecast` | Measured particle spectrum with sampling validity | `PLANNED_NOT_IMPLEMENTED` |
| 3 | `Forecast` | `MissionSequencingPLC` | Predicted storm horizon, non-authoritative | `PLANNED_NOT_IMPLEMENTED` |
| 4 | `ExternalParticleFlux` | `UnshieldedHabitat` | Actual fictional ambient exposure process | `PLANNED_NOT_IMPLEMENTED` |
| 5 | `WaterReservoir` | `WaterPumpA/B` | Finite water mass transfer | `PLANNED_NOT_IMPLEMENTED` |
| 6 | `WaterPumpA/B` | `FourShieldTanks` | Only if actual valve flows and pump power | `PLANNED_NOT_IMPLEMENTED` |
| 7 | `FourShieldTanks` | `ShieldArealDensity` | Physical shielding geometry and per-energy transmission | `PLANNED_NOT_IMPLEMENTED` |
| 8 | `ShieldCassettes` | `ShieldCoverage` | Actual encoder/lock state, not command state | `PLANNED_NOT_IMPLEMENTED` |
| 9 | `ShieldArealDensity` | `ShelterDoseProcess` | Areal mass and physical gaps affect protection | `PLANNED_NOT_IMPLEMENTED` |
| 10 | `UnshieldedHabitat` | `ShelterDoseProcess` | Actual external environment, independent of UI | `PLANNED_NOT_IMPLEMENTED` |
| 11 | `ShelterDoseProcess` | `ShelterReferenceDosimeter` | Fresh measured sample, possible overrange | `PLANNED_NOT_IMPLEMENTED` |
| 12 | `ShelterDoseProcess` | `IndependentDosimeter` | Separate protection measurement | `PLANNED_NOT_IMPLEMENTED` |
| 13 | `ShelterReferenceDosimeter` | `W149` | Critical sampled feedback; must be live circuit edge | `PLANNED_NOT_IMPLEMENTED` |
| 14 | `W149` | `HeliosSafetyPLC` | Invalid sample inhibits normal safety authorization | `PLANNED_NOT_IMPLEMENTED` |
| 15 | `HeliosSafetyPLC` | `ShieldActuators` | Bounded protected movement permission | `PLANNED_NOT_IMPLEMENTED` |
| 16 | `EssentialPowerAndLifeSupport` | `ShelterMission` | Must remain active through storm | `PLANNED_NOT_IMPLEMENTED` |
| 17 | `ShelterDoseProcess` | `ObjectiveHistory` | Full run-scoped integrated dose evidence | `PLANNED_NOT_IMPLEMENTED` |

## ANNEX J — EXACT MANIFEST, SPATIAL AND VERIFICATION DATA

| Source manifest field | Exact recorded value |
|---|---|
| `level_id` | `"level_89"` |
| `display_name` | `"The Helios Storm"` |
| `subtitle` | `"Solar Radiation Protection, Dosimetry, and Adaptive Shielding"` |
| `individual_models` | `92` |
| `individual_glbs` | `92` |
| `assembled_fbx` | `"Models/Level89_Complete_ArtKit_Showcase.fbx"` |
| `assembled_glb` | `"GLB/Level89_Complete_ArtKit_Showcase.glb"` |
| `individual_triangles` | `156444` |
| `individual_mesh_parts` | `1154` |
| `animation_parent_pivot_groups` | `76` |
| `planned_player_wires` | `170` |
| `planned_visual_terminal_sockets` | `248` |
| `proposed_independent_logical_wire_endpoints` | `340` |
| `anchor_reconciliation_conflicts` | `68` |
| `workstation_world_bounds_m` | `[[-16.25, -3.42, -17.75], [16.25, 15.97648, 13.1]]` |
| `fbx_numeric_units` | `"centimetres"` |
| `fbx_unit_scale_factor` | `1` |
| `glb_numeric_units` | `"metres"` |
| `axes` | `"Y-up X-width Z-forward"` |
| `assembly_origin` | `[0, 0, 0]` |
| `actual_player_wires_included` | `false` |
| `functional_sparkterminals_included` | `false` |
| `real_particle_transport_solver_included` | `false` |
| `runtime_safety_plc_included` | `false` |
| `six_runtime_objectives_implemented` | `false` |
| `unity_compilation_tested` | `false` |
| `unity_playmode_tested` | `false` |

`Verification_Report.json` reports **34/34 structural checks passed**, **0 failed**, `unity_import_tested=False`, `unity_compilation_tested=False`, `unity_play_mode_tested=False`, `objective_6_of_6_tested=False`. These are not gameplay results.

## ANNEX K — EXACT UNITY ART HELPERS AND TWELVE PROCEDURAL MATERIAL SETS

**Optional supplied Unity helper contract (art only):**

- `Unity_Optional/SparkLevel89ArtPlacementEditor.cs` → `Tools/Project Spark/Levels/89/Place HD Art Under Selected Layout`.
- `Unity_Optional/SparkLevel89MaterialUpgradeEditor.cs` → `Tools/Project Spark/Levels/89/Create & Apply HD Materials To Selected Art`.
- `Unity_Optional/SparkLevel89AssetValidatorEditor.cs` → `Tools/Project Spark/Levels/89/Validate Imported HD Art`.
- `Unity_Optional/SparkLevel89VisualRig.cs` → namespace `ProjectSpark.Visuals`, class `SparkLevel89VisualRig`, with method signatures reproduced exactly in section 10.

**Twelve named procedural reference material sets (1024×1024 maps):**

| Set | Texture resolution | Maps | Source status |
|---|---|---|---|
| `HeliosStainlessShieldingPlate` | 1024×1024 | Albedo / Normal / MetalRoughAO | `OPTIONAL_REFERENCE_MAPS_REQUIRING_UV_AND_SHADER_CHANNEL_MAPPING` |
| `HydrogenRichWaterTankBlue` | 1024×1024 | Albedo / Normal / MetalRoughAO | `OPTIONAL_REFERENCE_MAPS_REQUIRING_UV_AND_SHADER_CHANNEL_MAPPING` |
| `SolarObservatoryTitaniumCeramic` | 1024×1024 | Albedo / Normal / MetalRoughAO | `OPTIONAL_REFERENCE_MAPS_REQUIRING_UV_AND_SHADER_CHANNEL_MAPPING` |
| `ParticleDetectorCopperCoils` | 1024×1024 | Albedo / Normal / MetalRoughAO | `OPTIONAL_REFERENCE_MAPS_REQUIRING_UV_AND_SHADER_CHANNEL_MAPPING` |
| `GoldRadiationInstrumentSockets` | 1024×1024 | Albedo / Normal / MetalRoughAO | `OPTIONAL_REFERENCE_MAPS_REQUIRING_UV_AND_SHADER_CHANNEL_MAPPING` |
| `SolarStormOrangeHologram` | 1024×1024 | Albedo / Normal / MetalRoughAO | `OPTIONAL_REFERENCE_MAPS_REQUIRING_UV_AND_SHADER_CHANNEL_MAPPING` |
| `ShelterStatusCyanHologram` | 1024×1024 | Albedo / Normal / MetalRoughAO | `OPTIONAL_REFERENCE_MAPS_REQUIRING_UV_AND_SHADER_CHANNEL_MAPPING` |
| `ArmoredShelterBlackInsulator` | 1024×1024 | Albedo / Normal / MetalRoughAO | `OPTIONAL_REFERENCE_MAPS_REQUIRING_UV_AND_SHADER_CHANNEL_MAPPING` |
| `SpacecraftOpticalGlass` | 1024×1024 | Albedo / Normal / MetalRoughAO | `OPTIONAL_REFERENCE_MAPS_REQUIRING_UV_AND_SHADER_CHANNEL_MAPPING` |
| `MachinedSafetyControlPanels` | 1024×1024 | Albedo / Normal / MetalRoughAO | `OPTIONAL_REFERENCE_MAPS_REQUIRING_UV_AND_SHADER_CHANNEL_MAPPING` |
| `RadiationWarningAmber` | 1024×1024 | Albedo / Normal / MetalRoughAO | `OPTIONAL_REFERENCE_MAPS_REQUIRING_UV_AND_SHADER_CHANNEL_MAPPING` |
| `HeliosServiceDeckAntiSlip` | 1024×1024 | Albedo / Normal / MetalRoughAO | `OPTIONAL_REFERENCE_MAPS_REQUIRING_UV_AND_SHADER_CHANNEL_MAPPING` |

`MetalRoughAO` channels are R metallic, G roughness, B ambient occlusion. Source `uv_authoring_required=true`, so UV layout, authored maps, shader mapping, collisions, LODs and VFX are separate required production steps.

## FINAL SOURCE AND TEST AUTHORITY REMINDER

**Actual delivery in the ZIP: source-generated modular 3D art + conceptual engineering plans.** The planned electrical circuits, energy-group radiation model, independent dosimetry, finite water shielding, active safety PLC and six objective completions are **not delivered as working runtime gameplay**. Build them inside the real Unity project, verify real APIs and Play Mode, record actual error/test states, preserve Levels 1–88 and certify 6/6 only after fresh post-W149 full storm qualification.
