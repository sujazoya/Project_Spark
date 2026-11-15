# PROJECT SPARK — LEVEL 82 MASTER UNITY IMPLEMENTATION PROMPT

## LEVEL 82 — SILENCE THE SPECTRUM
### Advanced AAA Electromagnetic Compatibility, Conducted/Radiated Emissions, Three-Dimensional RF Mapping, Near-Field Diagnosis, Shielding and Harness Repair, Controlled RF Immunity, Mission Computer Integrity and Critical W119 Safety Feedback Recovery

**ROLE:** Act as my senior Unity C# engineer, Project Spark integration architect, electrical simulation engineer, EMC/RF engineer, high-speed avionics engineer, instrumentation and metrology engineer, industrial control and safety engineer, AAA technical artist, gameplay designer, and Unity Editor tooling developer.

**PRIMARY REQUEST: IMPLEMENT THE COMPLETE, FUNCTIONAL, ACTUALLY PLAYABLE LEVEL 82 IN MY EXISTING PROJECT SPARK UNITY GAME.** Level 82's design and procedural 3D art have already been produced. **Do not redesign or replace the 3D models.** Inspect actual existing Project Spark source and adapt to real class/API signatures instead of inventing them. Preserve Levels 1–81. The actual `LevelGamePlayManager` must certify **6/6** through real gameplay evidence.

Use `ProjectSpark_Level82_HD_AAA_Models.zip`, and the exact annexed manifests, as the source of truth for artwork and planned connections. This prompt is a Unity implementation specification, not a claim that the source project has already been compiled or tested.

---

## 01. THE EXPERIENCE — ONE UNIQUE LEVEL

**MISSION:** THE COMPUTER IS READY. THE SIGNALS ARE NOT SAFE. FIND THE INVISIBLE LEAKS. CONTROL THE INTERFERENCE. PROTECT EVERY CIRCUIT. SILENCE THE SPECTRUM.

The four-chiplet spacecraft computer assembled in Level 79, optically connected in Level 80, and fault-tolerant in Level 81 becomes the **Equipment Under Test (EUT)** inside a huge RF semi-anechoic chamber. The player must identify conducted and radiated electromagnetic interference, map worst-case antenna orientations, physically repair cable/common-mode, shield-seam and filtering problems, run protected RF immunity exposure, monitor actual spacecraft computer functionality, recover three independent disturbances, then unplug and restore critical W119 feedback. This is **not** another wafer manufacturing, optical resonance, digital ECC or robotics process level.

Gameplay loop: **wire → commission → calibrate → measure → diagnose source/path/victim → perform physical repair → remeasure → immunity qualify → survive disturbances → recover W119 → fresh comprehensive qualification → 6/6**.

## 02. ACTUAL DELIVERED ART, VERIFIED IN SOURCE MANIFESTS

- `level_id`: `level_82`; title: **Silence the Spectrum**; subtitle: *Advanced Electromagnetic Compatibility Qualification*.
- **72** modular binary FBX 7400 models plus **72** corresponding GLBs.
- **One assembled FBX and one assembled GLB**, separate alternative previews.
- **252,048 equipment triangles**, **7,369 mesh parts**, **54 actual mesh-owning animation pivot parent groups**.
- **206 modeled gold-rimmed visual socket meshes**, **134 conceptual wire rows**, **six planned objective definitions**.
- **12 procedural PBR material sets / 36 PNG maps** in the ZIP. They are preview maps, not baked artist-authored production textures.
- Nominal width 25.4 m; verified assembled width about **25.425 m**. Manifest bounds **min (-12.725, 0.0, -6.85), max (12.7, 9.095, 6.85)** in metres.
- **Art-only current state:** `actual_player_wires_included=false`, `functional_spark_terminals_included=false`, `emc_solver_included=false`, `six_authoritative_objectives_implemented=false`.
- Structural FBX/GLB verification passed. **Unity import, compilation, and Play Mode were not tested.** Do not claim they were.

Detailed exact filenames, pivot records, wire endpoint rows, socket anchors and all six source objective records are appended at the end of this Markdown document. Use the original CSV/JSON files when importing, rather than hand-transcribing from this prompt.

## 03. INSPECT REAL PROJECT SPARK APIS FIRST

Find these real classes where present: `SparkElectricalSolver`, `SparkCircuitSystem`, `SparkElectricalComponent`, `SparkTerminal`, `SparkSwitch`, `SparkLevelDefinition`, `SparkLevelEvaluator`, `SparkLevelSceneBindings`, `SparkLevelWorkstationBindingBridge`, `SparkShelfSlot`, `SparkObjectShelf`, `SparkLevelLayoutLoader`, `SparkLevel`, `LevelGamePlayManager`. Inspect namespaces, public method signatures, actual power/return semantics, terminal pin capacities, multiple terminal junction support, player wire placement/disconnect, PLC timing, acquisition timestamps, safety interlocks, animation, levels/prefabs, persistence, undo and existing Editor builder patterns. Reuse suitable verified components from earlier levels. **Never assume an earlier level design prompt proves an API actually exists.**

## 04. ABSOLUTE IMPLEMENTATION CONTRACT

- Maintain actual player wire topology, no prewired 134 puzzle wires, no decorative wire accepted as a real connection.
- Keep educational 5 V control, simulated EUT power, protected simulated RF power and independent safety permission as clearly separated domains.
- No physical RF radiation; all RF tests are deterministic, fictional game simulation.
- Model coherent device behavior, frequency response, geometry, instrument calibration, sample validity and exposure dynamics; do not fake pass/fail based on button presses.
- Keep requested actuator positions separate from actual positions; no teleporting mast, turntable, near-field probe or shutters.
- Neither full-wave Maxwell accuracy nor real-world EMC compliance is claimed. Use configurable educational limits/units; name results **Project Spark EMC Qualified**, not real FCC/CISPR/IEC/spacecraft certification.
- Protect personnel-facing game state conceptually: never allow RF exposure when door open, E-stop active, field monitor invalid or safety power absent. No unsafe physical instructions in this simulated game.
- Never reward disconnecting protective-earth/chassis safety bonding as an EMC solution.
- No SCADA/diagnostic telemetry can bypass W119 trusted RF field measurement. Safety shutdown remains independently effective.
- Keep Objectives 1–5 historically certified during the deliberate final fault. Only Objective 6 requires fresh full requalification after the fault.
- Build an idempotent Unity Editor generator; preserve Levels 1–81 and player-customized gameplay objects outside disposable visual art root.
- Report precisely which pieces were implemented, compiled, exercised in Play Mode and validated. **No fabricated 6/6**.

## 05. UNITY IMPORT, UNIT CONVENTION AND WORLD LAYOUT

Import numbered files `00_...fbx` to `71_...fbx` from `Models/`, optionally retain GLBs as backups. FBX numeric vertices are **centimetres, UnitScaleFactor=1**; GLB numeric geometry is **metres**. X width, Y up, Z front. Convert **once** so one Unity unit = one metre. All 72 modular pieces have a **common origin**, use local `(0,0,0)`, identity rotation and unit object scale after importer conversion. Create `L82_HD_VISUAL_ART`. **Do not instantiate** `Models/Level82_Complete_ArtKit_Showcase.fbx` or its GLB simultaneously with the modular set.

Expected visible width ~**25.425 m**, maximum height ~**9.095 m**. Do not force all individual models to 25.4 m wide: they are coordinated pieces. Use equipment colliders consistent with interior walkable space; absorber wall collider complexity should be simplified. Inspect performance impact: 7,369 parts require batching/instancing/LOD production optimization.

## 06. EXISTING UNITY TOOLS — USE ACTUAL MENU NAMES

Packaged Editor helpers, namespace `ProjectSpark.EditorTools`:

- `SparkLevel82ArtPlacementEditor.cs`: `Tools/Project Spark/Levels/82/Place HD Art Under Selected Layout` (creates/replaces `L82_HD_VISUAL_ART`).
- `SparkLevel82MaterialUpgradeEditor.cs`: `Tools/Project Spark/Levels/82/Create & Apply HD Materials To Selected Art` (material folder `Assets/ProjectSpark/Level82/Materials`; textures at `Assets/ProjectSpark/Level82/Textures`).
- `SparkLevel82AssetValidatorEditor.cs`: `Tools/Project Spark/Levels/82/Validate Imported HD Art`.
- `SparkLevel82VisualRig.cs` goes outside Editor folders, under the runtime assembly; namespace `ProjectSpark.Visuals`.

These are **art tools**, not the complete level builder. Their art-root replacement behavior must not erase actual terminals, wire objects, PLC, scenario state, authored gameplay or prefab links. Create the full gameplay Editor menu separately: `Tools/Project Spark/Levels/Create Level 82`.

## 07. PBR TEXTURES, SHADERS AND EFFECTS

Load the 12 material sets/36 maps from `Textures/`: each albedo, normal and packed `MetalRoughAO` RGB (R metallic, G roughness, B AO). Adapt packed channels correctly to the project's URP/HDRP/Standard shader workflow; do not misinterpret roughness as smoothness. Finish with dark RF absorber foam, satin shield metal, copper/black electronics, glass instrument panels, realistic floor reflections, subtle wear, emittance on meters and false-color spectral holograms. Author actual Lights/ParticleSystems as needed: the FBX geometry does not already provide complete gameplay VFX. Use instancing/baking/LOD judiciously.

## 08. EXISTING VISUAL RIG — VERIFIED PUBLIC METHOD SIGNATURES

Namespace: `ProjectSpark.Visuals`; class `SparkLevel82VisualRig`.

```csharp
SetActualChamber(float actualTurntableAzimuthDeg, float actualDoorOpen01,
    bool actualChamberDoorClosed, float actualShieldLidOpen01, float actualSeamCompression01);

SetActualScanMotion(float actualAntennaHeight01, float actualPolarizationDeg,
    float actualAntennaDistance01, float actualNearfieldX01,
    float actualNearfieldY01, float actualNearfieldZ01, float actualScanActivity01);

SetActualRepairs(float actualHarnessRoute01, float actualFerriteLatch01,
    float actualFilterDrawer01, float actualGasketCompression01,
    float actualThermalCameraPanDegrees);

SetActualEmissionMeasurements(float measuredHotspotSeverity01,
    float measuredConductedMargin01, float measuredRadiatedMargin01,
    bool actualConductedPass, bool actualRadiatedPass);

SetActualRFImmunity(float actualGeneratorDial01, float actualExposureAmplitude01,
    float actualFieldProbeHeight01, float actualAmplifierShutterClosed01,
    bool actualRFPowerAuthorized, bool trustedW119FieldFeedbackValid);

SetActualMissionMonitoring(float measuredSensorDisturbance01,
    float measuredClockJitter01, float measuredOpticalTelemetryActivity01,
    bool actualMissionComputerQualified, float actualCertificationProgress01);

SetActualRFSafety(bool actualCriticalFaultLatched,
    bool actualW119TrustedFeedbackValid, bool actualRFSourcePermission,
    float actualAmplifierShutterClosed01);
```

Call these from a gameplay-to-art adapter using **actual authoritative plant and sensor states**, never as a way to set outcomes. The rig's `LateUpdate` handles turntable azimuth, chamber door, shield canopy, seam compression, antenna height/polarization, near-field XYZ, harness route, ferrite, drawer, thermal camera, RF dials and shutter; conditional Lights/VFX reflect actual status. Feed physical degrees where the API requests degrees, normalized 0–1 where requested. The rig expects **54** unique `ANIM_*` transforms. It is art-only and contains no EMC solver, SparkTerminal, PLC or objective logic. It uses the `ANIM_OPTICAL_LINK_MONITOR` indicator with zero rotation intentionally: do not make the indicator a physical actuator.

## 09. REAL 54-PIVOT HIERARCHY

Read `Animation_Rigs.csv` for exact `Pivot`, `Model`, `Kind`, `Axis`, `X`, `Y`, `Z`, `Group`. The file contains **54** actual mesh-owning parents. Never regenerate fake names. Keep original model parenting/local pivot transformations; don't blindly move entire EUT along with its enclosing chamber when only the turntable should rotate. Animate `indicator` types with UI/material/VFX, not physical motion. The annex contains all 54 real positions.

## 10. TWO GRAPHS PLUS INDEPENDENT SAFETY

**Electrical graph:** 134 actual player-created wire connections among 206 planned visible socket positions; use real `SparkTerminal` components, signal types and solver graph. **EMC physics graph:** source noise spectrum → cable/path/shield/filter transfer functions → E/H field or conducted voltage/current → measurement instruments → susceptible EUT electronics. **Machine actuation graph:** commands → actuator dynamics → actual motor/antenna/probe positions → sensor feedback. **Safety domain:** protected RF amplifier supply, independent E-stop, door interlock, field feedback validity, calibrated power/safe conditions and latched shutdown. **Mission domain:** Level 81 EUT sensor, timing, optical telemetry, data integrity, watchdog and safe command gate.

Do not represent RF transmission as a normal Spark electrical wire; the player wiring controls instruments/actuators and carries sensor signals, while RF exposure is a separate bounded simulated process.

## 11. EXACT 134-PLAYER-WIRE BUDGET

Use **only source-authored** `Gameplay_Wiring_Concept_W001_W134.csv` as the mapping source. Groups:

| Wire IDs | Group | Count |
|---|---|---:|
| W001–W010 | 5 V educational power and returns | 10 |
| W011–W028 | Turntable, antenna and scan motion | 18 |
| W029–W048 | Conducted emissions and bonding | 20 |
| W049–W066 | Radiated emissions and metrology | 18 |
| W067–W084 | Shielding, filters and harness repairs | 18 |
| W085–W100 | Controlled RF immunity testing | 16 |
| W101–W116 | Mission function monitoring and faults | 16 |
| W117–W134 | PLC safety feedback and SCADA | 18 |
| **W001–W134** | **TOTAL** | **134** |

Each CSV row contains actual planned `WireID`, group, source/target model, source/target visual anchor, signal purpose, kind and critical flag. The generated source explicitly marks all rows `CONCEPT_ONLY_NO_PLAYER_WIRE`. Convert each to genuine player-created connections using real project APIs, preserve IDs, and make disconnection affect the measured subsystem.

## 12. EXACTLY 206 VISUAL SOCKETS — REQUIRED FUNCTIONAL RECONCILIATION

Source `Terminal_Anchor_Plan_206.csv` includes `P001`–`P206`, with owner model and coordinates; status is `ART_ANCHOR_NOT_SPARKTERMINAL`. The ZIP also contains `Visual_Terminal_Sockets_206.csv`. The 134 rows reference 268 source+target endpoint occurrences but only a subset of distinct physical anchors, with shared/repeated anchors.

**IMPORTANT REAL-SOURCE PRE-FLIGHT ISSUE:** The conceptual CSV reuses some visual anchors for **incompatible signal kinds**. For example, **P119** is referenced as W006 5 V signal-conditioning power *and* W119 analog RF field output, while **P126** is referenced by W002 power, W119 analog input, W124 amplifier status, W128 mast-ready and W134 final certification. P191–P194 are similarly reused for several inputs/outputs. Some commands such as W022 and W024 were marked `ANALOG_FEEDBACK` despite being described as setpoint commands. **Do not wire multiple incompatible signal types through one bare terminal or silently change the source plan.**

Builder must run a **terminal allocation/reconciliation pass**: distinguish the 206 modeled visual sockets from *logical port identities*, validate each endpoint type/direction, use proper compatible multi-contact devices/terminal blocks or explicitly allocate additional functional subports/visual grouping where real project API supports it, keep a documented mapping to the original P-IDs, and preserve 134 player-connectable connections. If 206 unique functional `SparkTerminal` objects cannot safely express all independent signal paths, implement 206 primary functional sockets plus separately named internal junction/bus contacts as warranted, or report the necessary change rather than faking electrical compatibility. Never place active 5 V on W119's feedback port. Record each repair in `Level82_TerminalReconciliationReport.csv` and use an automated validation test.

## 13. CONTROL-POWER AND RF-POWER DOMAINS

W001–W010 are educational 5 V control-interface feeds. Implement actual returns through the project's supported circuit architecture; do not infer that the CSV contains every return conductor just because the group is titled 'power and returns'. The RF amplifier must use a **separate simulated protected power domain**. PLC logical power and real RF safety permission must be independently validated. No test amplifier output without door, E-stop, field feedback and equipment readiness. Never simulate dangerous RF or provide physical RF setup instructions; stay entirely within fictional bounded game parameters.

## 14. FULL LEVEL INITIAL STATE — 0/6

- EUT mission computer staged but not fully commissioned for test.
- Chamber door open or unqualified; absorber and shield assembly present.
- Turntable not calibrated and stationary; antenna mast not referenced.
- Emissions measurement receiver and analyzer uncalibrated.
- Near-field probes not calibrated; conducted emissions equipment idle.
- Cable/shield/filter repairs unqualified; baseline leakage path and noisy harness exist in the simulated EUT.
- RF generator disabled, amplifier disabled, RF exposure shutter closed, RF field-strength monitor uncalibrated.
- W119 will be wired by player in Objective 1 and is required valid for immunity authorization.
- Mission sensor/clock/optical telemetry monitoring not yet certified.
- All objectives 0/6. No active severe injected disturbance at start.

## 15. ONE AUTHORITATIVE EMC SIMULATION STATE

Track source spectral amplitudes and phases, harmonic locations, DC power rail ripple, differential/common-mode harness currents, cable geometry, shield-seam gap and compression, shield terminations, chassis reference state, filters and their nonideal attenuation, EUT orientation, antenna polarization/height/distance, near-field probe XYZ, measurement receiver bandwidth/detector, calibration state, sample timestamps, measured emissions margins, RF immunity commanded/actual field, field probe timestamp/validity, RF amplifier dynamics, door/E-stop interlocks, actuator positions, EUT sensor/clock/optical packet health, temperature, fault history and six objective evidence objects. No one scalar `emcProgress` may create pass/fail.

## 16. SOURCE/PATH/VICTIM EMC MODEL

Represent distinct interference **sources**: switching power converter ripple/harmonics, high-speed clock edge, cable common-mode current and test RF exposure. Represent **paths**: power lead conduction, differential signal conduction, common-mode cable radiation, seam aperture radiation, shield penetration/feedthrough and field pickup. Represent **victims**: precision analog sensor input/ADC, clock/logic timing, power-feedback sensing, Level 80 optical control and Level 81 mission decision path. Allow multiple simultaneous sources and paths.

Use a configurable, reproducible **educational** frequency-response model, not a claim of real full-wave electromagnetic accuracy. Example coherent formulation: `E(f,pose,pol) = magnitude(sum_k source_complex_k(f) * coupling_complex_k(f,pose,pol))`. Keep phase if using coherent sum; otherwise document a conservative power-sum approximation. Use physically motivated falloff, resonance peaks, antenna orientation, shield effectiveness and attenuation. Do not just lower all measured values whenever a repair UI button is clicked.

## 17. CONDUCTED EMISSIONS MODEL

Model **DC-power artificial network**, common-mode clamp, differential current probe, receiver input and spectral calibration. Source devices: `L82_DCPowerArtificialNetwork`, `L82_CommonModeCurrentClamp`, `L82_DifferentialCurrentProbe`, `L82_ConductedEmissionMeasurementRack`, `L82_EMIComplianceMeasurementReceiver`. Measure frequency-dependent conducted noise; distinguish differential and common-mode current. Cable return-route and filter topology alter modeled traces. Source test must use actual EUT workload, not a dormant computer with no switching activity.

## 18. RADIATED EMISSIONS MODEL

Model enclosure and harness radiation, near-field source hotspots, antenna pattern/polarization, receiver sensitivity, orientation, height and frequency. Use `L82_BroadbandEmissionMeasurementAntenna`, `L82_MotorizedAntennaHeightMast`, `L82_AntennaPolarizationRotationHead`, `L82_RotatingEUTTurntable`, `L82_EMIComplianceMeasurementReceiver`, `L82_HighDynamicRangeSpectrumAnalyzer`, `L82_RFPreselectorAndAttenuator`. Emissions and immunity must be separate test modes: immunity generator off/isolated while measuring EUT emissions.

## 19. ACTUAL TURNTABLE DYNAMICS

Track target angle, measured encoder angle, rotational speed/acceleration, safe motion enable, travel completion and settle. Range one full 360° sweep. Use `ANIM_EUT_TURNTABLE` and `ANIM_AZIMUTH_ENCODER` from the actual rig. Calibration must account for a home reference. Capture samples at *actual* angle after settling; don't pretend an unturned device was measured from every azimuth.

## 20. ACTUAL ANTENNA MAST, POLARIZATION AND DISTANCE

Track requested/actual antenna mast height, actual polarization, position limits, encoder validity, stabilization and measurement distance proxy. The visual rig uses normalized height 0–1 and polarization in degrees. Height/polarization influence measured radiation response. The player must search for worst measured case, not choose the quietest pose and skip other mandatory scan positions.

## 21. REQUIRED SPATIAL SCAN COVERAGE

Proposed game qualification: **12 angular samples × 3 mast heights × 2 polarizations = 72 required spatial combinations per chosen frequency sweep/band**; use configurable sampling density to balance playtime and performance. The scan must record valid actual turntable/antenna positions, receiver calibration, dwell/settle and test mode. Missing samples invalidate 'full scan' certification. Present a 3D radiation pattern and heatmap derived from real stored results. Keep worst-case amplitude and margin across the entire required grid.

## 22. NEAR-FIELD HOTSPOT INSPECTION

Use `L82_NearFieldScannerXYGantry`, `L82_NearFieldScannerZLift`, `L82_NearFieldMagneticHProbe`, `L82_NearFieldElectricEProbe`, `L82_EnclosureSeamLeakageProbe`. Track actual XYZ scan position and probe selection. Simulated E and H responses must be different. Produce maps that localize switching converter, cable current and seam leakage. Diagnostic maps may guide player but cannot prove radiated-compliance scan alone. Real moving probe axes must be bounded and collision-safe.

## 23. SOURCE CLASSIFICATION AND FREQUENCY SIGNATURES

Assign distinguishable modeled signatures: switch-mode converter harmonics/ripple, high-speed clock frequencies, cable common-mode resonances, enclosure seam leakage maxima and external RF immunity excitation. The player should infer source → path → affected instrument rather than treat all spikes as one noise value. Maintain deterministic fault seeds for repeatable testing; a successful repair should address the identified cause, not merely hide a display trace.

## 24. SHIELD ENCLOSURE AND GASKET PHYSICS

Use `L82_RemovableEMCShieldCanopy`, `L82_ConductiveSeamGasketSet`, `L82_AvionicsAccessHatch`, `L82_ShieldGasketCompressionTester`, `L82_EnclosureSeamLeakageProbe`. Track lid/door state, gasket compression, seam length/gap proxy, electrical contact quality, frequency-dependent shielding effectiveness and cooling effects. A small seam defect should increase leakage only on appropriate paths/bands; gasket repair should be an actual object state change and verified by both local probe and radiated rescan.

## 25. CHASSIS BONDING AND SAFE RETURNS

Use `L82_ChassisEquipotentialBondingGrid`, `L82_ReferenceGroundPlane`, `L82_360DegreeShieldTerminationClamp`, `L82_GroundReturnFaultSwitch`. Model intended spacecraft-chassis bonding, signal returns and cable shields separately from facility protective-earth safety. Incorrect return paths can raise common-mode current. Player must restore the *qualified design topology*, never disconnect facility protective earth for a false improvement. Actual terminal type and legal topology must govern repair.

## 26. CABLE ROUTING AND FERRITE/CHOKE REPAIRS

Use `L82_ShieldedHarnessBundle`, `L82_CableRoutingTray`, `L82_HighSpeedDifferentialCable`, `L82_ClipOnFerriteCoreRack`, `L82_CommonModeChokeModule`. Player physically moves/installs components, which modify frequency-dependent common-mode attenuation, routing proximity, cable radiation and loss. Model nonideal effects such as limited frequency range, saturation proxy and added loss, not 'all EMI removed' magic.

## 27. FEEDTHROUGH, LC FILTER AND SENSOR FILTER TRADEOFFS

Use `L82_FilteredBulkheadFeedthrough`, `L82_DCFeedthroughFilterArray`, `L82_DifferentialLCFilterModule`, `L82_ProtectedSensorInputFilter`. Track insertion loss, frequency response, ripple attenuation, sensor input latency, bandwidth, voltage drop and heat proxy. Some fixes improve EMC but may violate EUT sensor timing or thermal limits. Require **both** EMC margin **and** mission function to pass.

## 28. THERMAL AND SYSTEM-HEALTH CONSEQUENCES

Use `L82_InfraredThermalInspectionCamera`, `L82_ShieldedCoolingAirDuct`, `L82_SwitchModePowerConverterModel`. Shield/lid changes can reduce cooling; filters can dissipate power; power-converter switching changes noise. Include simplified thermal dynamics. Do not pass EMC tests when the mission computer overheats or fails timing due to an excessive repair.

## 29. EMISSIONS INSTRUMENTATION TRUTH

Measurements need calibration status, actual instrument bandwidth/detector mode, chosen span, settling/dwell time, timestamp, uncertainty and overload state. The analyzer never directly sets simulation truth. Instrument scans produce samples from the EMC plant; only valid/complete scan evidence qualifies. Display fictional limits with guard band; do **not** cite actual standards conformance unless verified against proper standard procedures.

## 30. DISTINGUISH TEST MODES

Use at least `OFF`, `COMMISSIONING`, `CONDUCTED_EMISSIONS`, `RADIATED_EMISSIONS`, `NEARFIELD_DIAGNOSTIC`, `REPAIR_SAFE`, `RF_IMMUNITY_CALIBRATION`, `RF_IMMUNITY_EXPOSURE`, `POSTREPAIR_RETEST`, `SAFE_HOLD`, `CRITICAL_FAULT`, `RESET_REQUIRED`, `FINAL_CERTIFIED`. Enforce mutual exclusions: cannot open EUT shield during RF exposure; cannot run high-power immunity output during emission measurement; cannot start scans with uncalibrated receivers; cannot move interlocked equipment into unsafe positions.

## 31. PROTECTED RF IMMUNITY FIELD PLANT

Use `L82_RFImmunityCouplingFixture`, `L82_ProgrammableRFSweepGenerator`, `L82_ProtectedRFAmplifier`, `L82_TriaxialRFFieldStrengthProbe`, `L82_RFUniformityCalibrationFrame`. Track requested test frequency/field, actual source level, amplifier gain/enabled state, output shutter state, fictional field distribution, test fixture geometry, field probe position, actual triaxial field magnitude, calibration range, sample timestamp and measurement health. Source shuts down with finite dynamics; it must not retain power because SCADA still displays an old number. Exposure requires safety permission and active valid field feedback.

## 32. IMMUNITY EFFECTS ON EUT FUNCTION

External simulated RF couples into Level 81-style EUT sensor ADC, clock timing, power feedback and Level 80 optical control/telemetry. Choose distinct frequency-dependent susceptible pathways. Track actual synthetic sensor truth versus digitized sensor reading, jitter proxy, data packet validity, ECC/TMR monitoring and protected mission command gate. Reject unsafe commands. A qualified computer is not necessarily immune to analog corruption; mission-function testing must run throughout prescribed exposures.

## 33. MISSION COMPUTER ADAPTER

If Level 81 gameplay has real reusable public interfaces, connect directly. Otherwise create a deterministic `L82_MissionComputerAdapter` with clearly documented representative sensor/clock/optical/data integrity simulation, **not** a fake success signal. Model actual workload, sensor reference inputs, clock health, optical telemetry, power state, thermal state, protected command validity and number of blocked unsafe outputs. Use one authoritative source of pass/fail evidence.

## 34. THREE DISTINCT DISTURBANCES — NOT SCRIPTED PASS FLAGS

**A: common-mode harness fault.** Modify harness return/routing state; raise conducted current in selected bands and directional cable radiation. Diagnose with current clamp + conducted receiver + rotating antenna. Repair qualified bonding/path/filter and require new scans.

**B: shield-seam defect.** Actuate a small modeled gasket/seam gap; degrade shielding in selected bands, produce near-field seam hotspot and orientation-dependent radiation. Safely remove test energy, repair gasket/compression, verify seam, redo radiated scan.

**C: RF-induced sensor desense.** During protected immunity exposure, coupled field perturbs analog sensor input while true synthetic sensor reference remains unchanged. Navigation estimator/command gate must identify unsafe data. Repair sensor input/filter/cable route; verify latency not excessive; redo immunity plus mission function tests.

Each disturbance has: trigger, actual plant mutation, diagnostic evidence, safety consequences, repair interaction, requalification evidence, and a logged event trail. **No direct objective flag toggles.**

## 35. FINAL CRITICAL WIRE W119 — REAL SOURCE-VERIFIED ROUTE

`W119` is in the source CSV with `SignalKind=ANALOG_FEEDBACK`, `Critical=YES`:

- Source model: **`L82_TriaxialRFFieldStrengthProbe`**.
- Source proposed terminal: **`RF_FIELD_MEASURED_OUT`**.
- **Source visual socket: `P119`**.
- Destination model: **`L82_RFImmunitySafetyPLC`**.
- Destination proposed terminal: **`RF_SAFE_FIELD_FEEDBACK_IN`**.
- **Destination visual socket: `P126`**.
- Purpose: **CRITICAL trusted measured RF field strength feedback**.

The field probe must report **measured actual fictional RF field, validity, calibration, range and timestamp** at a meaningful sample rate. W119 is a monitored continuously sampled feedback path; a disconnected wire must not resemble 'zero field' or 'safe low field'. The signal is not itself the RF amplifier power line.

## 36. W119 TRUST-BOUNDARY / NO BYPASS

- **W120**: separate non-authoritative triaxial probe telemetry to SCADA. Does not replace W119.
- **W125**: probe calibration diagnostic health. Does not replace W119.
- **W124**: amplifier-disabled confirmation, useful for validating shutdown, not a field-strength substitute.
- **W118**: chamber door interlock, independent prerequisite.
- **W123**: emergency RF stop, independently effective.
- **W121**: protected amplifier enable request.
- **W122**: independent RF shutdown override.
- **W131**: fault-latch reporting; **W132**: explicit reset request.

Do not accept field qualification from SCADA values, cached last-good measurements, display states, W125 health or guessed source output. Keep independent safety shutdown possible even if the ordinary RF test PLC malfunctions.

## 37. W119 FINAL FAULT — MANDATORY DISCONNECTION SEQUENCE

After Objectives 1–5 have **historically completed**:

1. Player physically unplugs the **actual W119 wire**.
2. The real Spark electrical graph changes and W119's electrical continuity is false.
3. The supervisor marks receiver field data invalid/stale; timestamps cannot silently refresh from cached values.
4. RF immunity exposure authorization immediately withdraws.
5. Protected amplifier enable is removed through real safety output.
6. Independent RF shutdown command engages; protective output/shutter transitions to safe state.
7. Simulated RF source energy decays through defined dynamics; confirm amplifier-disabled state using **W124** when available.
8. Enforce no new exposure, scan certification or mission-immunity PASS while monitoring is lost.
9. Preserve chamber door position, turntable pose, EUT state, existing emissions history and Objectives 1–5 historical certifications.
10. Latch critical fault, log causal reason, display `CRITICAL RF FIELD FEEDBACK LOST — RF AMPLIFIER INHIBITED`.

If actual shutdown confirmation is missing, indicate an unresolved safety fault, not a successful shutdown based on the command alone.

## 38. W119 REPAIR, RECOMMISSIONING AND FRESH QUALIFICATION

11. Player reconnects real W119 and electrical topology becomes valid.
12. Receive genuinely **fresh** probe samples; verify sensor power and signal range.
13. Field probe must be **recalibrated** against configured fictional calibration reference; retain historical fault until complete.
14. Verify chamber door, emergency stop, RF amplifier disabled state, guard checks, mission EUT readiness and separate safety power.
15. User performs **explicit** fault reset; safety PLC accepts only after all prerequisites pass.
16. Recommission conducted-emissions setup and perform a **new complete conducted test**.
17. Recommission antenna/turntable and perform a **new full radiated scan** with spatial coverage and worst-case margin.
18. Recommission protected field source and execute a **new measured RF immunity profile** with W119 continuously valid.
19. Verify EUT sensor, clock, optical telemetry, mission workload and command-gate safety remain within qualification window.
20. No unresolved critical fault; **Objective 6** may now certify actual `LevelGamePlayManager` **6/6**. Reconnecting W119 or pressing RESET alone must never certify 6/6.

## 39. SIX AUTHORITATIVE OBJECTIVES — EXACT SOURCE TITLES

1. **Build the EMC Qualification Network** — 134 real player wires; 206 registered functional socket/terminal mappings; true power, instruments, safety.
2. **Calibrate the Blackstar Chamber** — chamber interlock, turntable, antenna mast/polarization, EMI receiver, near-field probes, conducted instrumentation and trusted field probe calibrated.
3. **Silence Conducted and Radiated Emissions** — source/path diagnosis, real repairs, full spatial radiated coverage and conducted/radiated margins; EUT mission function intact.
4. **Prove RF Immunity** — W119 valid, interlocks independent, protected exposure profiles complete, actual sensor/clock/optical telemetry/mission outputs qualified and no unsafe commands.
5. **Survive Three EMC Failures** — repair harness common-mode, gasket seam and RF sensor susceptibility; complete **fresh EMC + mission-function qualification** after disturbances.
6. **Restore Critical RF Field Feedback** — deliberate W119 disconnection, stale-sample detection, protected shutdown, latched fault; actual reconnect + probe recalibration + explicit reset + **fresh conducted/radiated/immunity full qualification**, then 6/6.

Register these with the real `LevelGamePlayManager` / actual Level 82 evaluator APIs. Require evidence objects with event IDs, measurement batches, timestamps, test coverage, calibration versions, simulation state IDs, and fault sequence IDs. Avoid UI-only certification. Historical completion of Objectives 1–5 is immutable during the final deliberate fault; the *current safe operating state* may still go invalid.

## 40. OBJECTIVE 1 EVIDENCE GATES

Every W001–W134 required player wire connected correctly; 206 modeled socket anchor mappings validated; functional terminal types/returns reconciled; control supply meets actual solver requirement; mission EUT starts; turntable/antenna/nearfield motion, EMI receiver, conducted rack, analyzer, filter controls, field probe, W119 and independent PLC safety have valid electrical state. Reject bypass wires, invalid junctions, incompatible signal types or unmatched source/destination ports. **1/6** only on real circuit evidence.

## 41. OBJECTIVE 2 EVIDENCE GATES

With chamber secured and protected source disabled: calibrate turntable azimuth encoder, motor settling, antenna height and polarization, near-field XYZ/probe calibration, EMI receiver and spectrum analyzer, spectral reference source, conducted current clamps/network, shield conditions, door interlock, W119 field probe against fictional reference and emergency RF disable. Generate a calibration certificate with sensor validity/time. **2/6**.

## 42. OBJECTIVE 3 EVIDENCE GATES

Measure actual conducted emissions under EUT workload; execute full required 12×3×2 radiated spatial sampling with frequency sweeps; find worst case and diagnose source/path/victim using conducted + nearfield + antenna evidence; make physical repairs; re-scan conducted and radiated emissions until all fictional frequency-dependent limits and guard bands pass. Verify repairs did not break sensor timing, optical telemetry or thermal functionality. **3/6**.

## 43. OBJECTIVE 4 EVIDENCE GATES

Verify independent door/E-stop checks, calibrated W119 with fresh measured field, protected RF amplifier disabled before arming, calibration frame/uniformity proxy and bounded sweep profile. Carry out actual simulated exposure at prescribed positions/frequencies, monitor real sensor ADC deviation, clock jitter, optical telemetry, power rail, mission commands and thermal conditions; verify zero unsafe command releases. Monitor safe power after exposure ends. **4/6**.

## 44. OBJECTIVE 5 EVIDENCE GATES

Activate and prove all three disturbances by actual plant state changes; diagnose source/path/victim; enact physical repair interactions; validate repair effectiveness and absence of adverse side-effects; then run **fresh conducted + full radiated + protected immunity + mission-function** qualification after the last disturbance. No critical fault. **5/6**.

## 45. OBJECTIVE 6 EVIDENCE GATES

After previous five historically certified, deliberately unplug W119; witness real topology break → stale field samples → no RF permission → independent shutdown + actual amplifier disabled confirmation → latched fault. Physically reconnect W119; acquire fresh data, recalibrate, verify interlocks, explicit reset; then perform new comprehensive conducted/radiated/immunity tests with full spatial scan and EUT mission-function monitoring. No stale pre-fault results can count for Objective 6. Finish authoritative **6/6**.

## 46. LIVE AAA SCADA — MEASUREMENTS, NOT AUTHORITIES

Create dashboards for: emissions frequency spectrum, before/after repair trace, conducted vs radiated limit/margin, 3D emission pattern, antenna angle/height/polarization, XY nearfield E/H heatmaps, cable common-mode current, shield seam leakage, filter attenuation and thermal consequences, chamber and door status, field probe measured values/range/sample age, W119 connectivity, RF source actual output, amplifier permit/shutdown, mission ADC error, clock jitter, optical telemetry, protected commands, disturbance history, objective progress and explicit reset readiness. Display actual source/measurement confidence. Holograms visualize state but never determine fault truth.

## 47. PLAYER INTERACTIONS AND UI COMMANDS

Implement real interactions: `MAIN POWER`, `VERIFY CHAMBER`, `CALIBRATE TURNTABLE`, `ROTATE EUT`, `MOVE ANTENNA`, `POLARIZATION`, `CALIBRATE EMI RECEIVER`, `SCAN CONDUCTED`, `SCAN RADIATED`, `NEARFIELD H/E`, `INSPECT SEAM`, `ROUTE HARNESS`, `FIX BONDING`, `INSTALL CHOKE`, `APPLY FERRITE`, `REPLACE GASKET`, `SELECT FILTER`, `CHECK SENSOR LATENCY`, `CALIBRATE W119`, `ARM RF`, `START IMMUNITY`, `STOP RF`, `INJECT COMMON-MODE FAULT`, `INJECT SEAM GAP`, `INJECT SENSOR DESENSE`, `DISCONNECT W119`, `RECONNECT W119`, `RESET PROTECTION`, `FRESH CERTIFY`. Every control must invoke an actual supported gameplay operation; safe controls may be disabled when interlocks fail.

## 48. EXAMPLE GAMEPLAY QUALITY TARGETS (FICTIONAL)

Use configurable fictional frequency-band limits for conducted/radiated emission amplitude, **6 dB illustrative radiated guard band**, complete 72 spatial samples per frequency sweep/band, calibrated field probe tolerance, bounded exposure profiles, sensor accuracy and latency margins, acceptable clock/data performance, valid thermal operating range, zero unsafe command releases and no unresolved safety faults. Units and limits must be labeled **game scenario**, not copied from a real EMC standard. Final status: `PROJECT SPARK EMC QUALIFIED`.

## 49. SUGGESTED RUNTIME CLASS RESPONSIBILITIES

Create only when no compatible implementation exists; names are proposals:

- `SparkLevel82Coordinator`, `SparkLevel82ProcessState`, `SparkLevel82ElectricalBindings`, `SparkLevel82TerminalMapper`.
- `SparkLevel82FrequencySpectrum`, `SparkLevel82EMCSourceModel`, `SparkLevel82ConductedEmissionModel`, `SparkLevel82RadiatedEmissionModel`, `SparkLevel82CableCouplingModel`.
- `SparkLevel82ShieldingModel`, `SparkLevel82ChassisBonding`, `SparkLevel82FilterModel`, `SparkLevel82ThermalConstraintModel`.
- `SparkLevel82TurntableController`, `SparkLevel82AntennaMastController`, `SparkLevel82NearFieldScanner`, `SparkLevel82EMIReceiver`, `SparkLevel82CalibrationService`.
- `SparkLevel82ImmunityFieldPlant`, `SparkLevel82FieldProbe`, `SparkLevel82RFSourceController`, `SparkLevel82RFSafetyController`, `SparkLevel82TestSequencingPLC`.
- `SparkLevel82MissionComputerAdapter`, `SparkLevel82SensorSusceptibility`, `SparkLevel82ClockSusceptibility`, `SparkLevel82OpticalTelemetryMonitor`.
- `SparkLevel82DisturbanceController`, `SparkLevel82W119Recovery`, `SparkLevel82ObjectiveTracker`, `SparkLevel82SCADA`, `SparkLevel82VisualRigAdapter`.

Use one authority per state dimension. Do not duplicate solver state in visual classes. Use stable fixed simulation steps for plant and PLC, distinct acquisition update cycles, and smooth render interpolation.

## 50. ONE-CLICK LEVEL BUILDER

Create actual Editor menu **`Tools/Project Spark/Levels/Create Level 82`**. Builder must:

1. Verify real Project Spark APIs and import manifest.
2. Find all **72 exact numbered FBX models**; check bounds/import scale.
3. Create/update `Level_82.asset`, `Level_82_Layout.prefab`, and six objective records via actual project schemas.
4. Place 72 modular art pieces at common origin; keep gameplay outside replaceable `L82_HD_VISUAL_ART`.
5. Bind 54 verified pivots and configure collider/interaction volumes.
6. Reconcile all 206 visual socket anchors with compatible real terminals; generate conflict report for reused anchor/signal directions.
7. Create **134 player wire definitions** without pre-connecting puzzle wires, and actual power/return validation.
8. Configure distinct educational 5 V, simulated EUT and protected RF power domains.
9. Implement machine dynamics, calibration, conducted/radiated field models, receiver measurements and spectral limits.
10. Implement installable harness/gasket/choke/ferrite/filter repair objects and proper engineering tradeoffs.
11. Implement protected RF immunity field plant and W119 monitored feedback, independent E-stop, door interlock and amplifier-disable confirmation.
12. Implement EUT mission function adapter, three disturbances and actual fault recovery.
13. Implement six authoritative evidence-based objectives preserving history.
14. Bind simulation-driven SCADA, holograms, indicators and VFX through `SparkLevel82VisualRig`.
15. Validate duplicates, compile-time bindings, wire/terminal topology, all pivots, scenario preconditions, safe fault response; generate report.
16. Save assets/prefabs safely; do not overwrite prior levels or custom gameplay without migration/backup.

## 51. MAIN PLC/SCENARIO STATE MACHINE

Suggested explicit states: `OFF`, `CONTROL_WIRING`, `EUT_COMMISSIONING`, `CHAMBER_SAFETY_CHECK`, `TURNTABLE_CALIBRATION`, `ANTENNA_CALIBRATION`, `RECEIVER_CALIBRATION`, `NEARFIELD_CALIBRATION`, `CONDUCTED_SCAN`, `RADIATED_SCAN`, `SOURCE_DIAGNOSIS`, `REPAIR_SAFE`, `REPAIR_VERIFICATION`, `EMISSIONS_QUALIFIED`, `FIELD_PROBE_CALIBRATION`, `RF_SOURCE_ARMED`, `RF_EXPOSURE_ACTIVE`, `RF_EXPOSURE_SETTLING`, `IMMUNITY_VERIFY`, `EMC_QUALIFIED`, `DISTURBANCE_RECOVERY`, `SAFE_HOLD`, `CRITICAL_FAULT`, `RESET_REQUIRED`, `FINAL_CERTIFIED`.

State transitions must evaluate actual topology, safety, sensor freshness, setup, measurement coverage and mission results; timers alone cannot confer PASS. Door open/unstable positioning during RF exposure immediately inhibit; accidental source setup must fail closed.

## 52. DIAGNOSTIC MESSAGES

Report: missing or incompatible wire, duplicate terminal, no valid 5 V returns, reused visual socket incompatible type, chamber door open, missing turntable home, antenna actual/command mismatch, mast axis uncalibrated, EMI receiver overload, calibration expired, absent nearfield probe, incomplete radiated scan, different polarizations missing, common-mode current excessive, E/H hotspot, shield gasket open, chassis bonding invalid, filter thermal limit exceeded, sensor latency excessive, insufficient attenuation, EUT mission invalid, invalid field probe calibration, W119 unplugged, W119 sample stale, W120 telemetry attempted bypass, W125 diagnostic attempted bypass, RF amplifier not confirmed disabled, emergency stop, safety fault latched, reset prerequisites missing, Objective 6 triggered by reconnection alone, stale pre-fault test evidence, `LevelGamePlayManager` stuck at 0/6 or 5/6, missing FBX/pivot, double import scale, and previous-level regression.

## 53. MANDATORY ENGINEERING TESTS

Test in real Unity where available, and honestly distinguish unexecuted tests:

**Asset integration:** 72 FBXs/GLBs; 25.425 m assembled bounds; 54 pivots; 206 modeled sockets; 36 PBR maps; 134 unique wire rows.

**Electrical:** functional SparkTerminals, correct signal direction/pin capacities, true player wires, source/return, 5 V and RF domains, connection and disconnection influence device state, documented mixed-anchor reconciliation.

**Motion:** turntable actual acceleration/rotation/encoder, antenna mast height and polarization, nearfield X/Y/Z, door and RF shutter, limits/interlocks, visual adapter reflects measurements.

**Conducted:** modeled source harmonics change with EUT workload; common-mode vs differential measures differ; cable/return/filter repairs alter correct frequency bands; instrument calibration/settle validity.

**Radiated:** enclosure seam defect changes correct leakage path; directional antenna and polarization matter; 12×3×2 scan stores 72 valid spatial combinations; missing samples fail qualification; worst-case margin reported; no immunity test power during emissions.

**Immunity:** RF source permission requires door/E-stop/W119; actual source field integrates; probe reads actual simulated field and timestamps; RF susceptibility perturbs ADC/clock/telemetry; protected command gate prevents unsafe output; source and shutter shut down on W119 failure.

**Fault recovery:** three disturbances actually change plant variables, diagnoses/repairs affect source-path-victim chain, fresh requalification required, W119 physical loss → data invalid → RF disable → amplifier disabled confirmation → latched fault; W120/W125 not bypass; reconnection → fresh calibration → explicit reset → full new conducted/radiated/immunity and EUT tests.

**Objectives:** historical Objectives 1–5 persist; Objective 6 cannot complete from reconnect alone; actual `LevelGamePlayManager` reaches 6/6; next-level progression and Levels 1–81 regression.

## 54. COMPLETE REQUIRED PLAYER GAMEPLAY FLOW

**CONNECT 134 REAL WIRES → REGISTER/RECONCILE 206 FUNCTIONAL SOCKETS → VALIDATE 5 V CONTROL AND SAFETY → VERIFY LEVEL 81 MISSION COMPUTER → CLOSE RF CHAMBER → CALIBRATE TURNTABLE → HOME ANTENNA MAST → CALIBRATE POLARIZATION → CALIBRATE RECEIVER AND SPECTRUM ANALYZER → CALIBRATE COMMON-MODE/DIFFERENTIAL PROBES → COMMISSION NEARFIELD XYZ SCANNER → SCAN CONDUCTED NOISE → ROTATE EUT THROUGH REQUIRED AZIMUTHS → MOVE ANTENNA THROUGH REQUIRED HEIGHTS → MEASURE BOTH POLARIZATIONS → FIND WORST RADIATED HOTSPOT → PERFORM NEARFIELD E/H INSPECTION → IDENTIFY COMMON-MODE/CLOCK/SEAM SOURCE AND PATH → SAFELY OPEN EQUIPMENT → FIX CHASSIS BONDING → REROUTE HARNESS → INSTALL FILTER/FERRITE/CHOKE → REPAIR SEAM GASKET → VERIFY EUT THERMAL AND SENSOR FUNCTION → RESCAN CONDUCTED AND RADIATED → CALIBRATE FIELD PROBE → VERIFY REAL W119 → VERIFY E-STOP/DOOR → ARM PROTECTED RF IMMUNITY → RUN CONTROLLED FICTIONAL FIELD SWEEP → MEASURE FIELD STRENGTH → VERIFY ADC/CLOCK/OPTICAL/COMMAND GATE → PASS IMMUNITY → INJECT COMMON-MODE CABLE FAULT → DIAGNOSE AND REPAIR → INJECT SHIELD-SEAM GAP → DIAGNOSE AND REPAIR → INJECT RF SENSOR DESENSE → DIAGNOSE AND REPAIR → FRESH EMC AND MISSION QUALIFICATION → PHYSICALLY DISCONNECT W119 → DETECT STALE RF FIELD FEEDBACK → STOP EXPOSURE → DISABLE AMPLIFIER AND CONFIRM SHUTDOWN → LATCH FAULT → RECONNECT W119 → ACQUIRE FRESH MEASUREMENTS → RECALIBRATE PROBE → EXPLICITLY RESET SAFETY → EXECUTE NEW COMPLETE CONDUCTED SCAN → EXECUTE NEW COMPLETE RADIATED SCAN → EXECUTE NEW PROTECTED RF IMMUNITY TEST → VERIFY MISSION COMPUTER FUNCTION → REAL LEVEL MANAGER 6/6.**

## 55. DELIVERABLES — REAL SOURCE AND UNITY ARTIFACTS

- Runtime C# simulation components and tests; no fake electrical or RF physics behavior.
- Actual 134-wire mapping and reconciled 206 visual socket/terminal scheme with CSV conflict report.
- Complete Level 82 Unity level definition, objective evaluator bindings and layout prefab; one-click Editor builder.
- Conducted and radiated spectrum solver, calibration instruments, scan-coverage certificates, wave/source/path diagnostic system.
- Turntable/antenna/near-field actuator models; physical shield/harness/filter repair states and thermal/latency consequences.
- Independent RF safety controller, W119 trusted field feedback, amplifier disable, E-stop, door and latching/reset.
- Level 81 EUT mission monitor, three active disturbances, fresh recovery qualifications, six real objective certifications.
- Integration with existing `SparkLevel82VisualRig`, 54 actual pivots, emissions/field holograms, meter UI, VFX and PBR materials.
- QA/test report distinguishing passed, failed and untested; reproducible project-ready files.

## 56. FINAL SUCCESS CONDITION

Player can wire/commission the physical-looking EMC facility, locate distinct conducted/radiated causes through spatial/frequency measurements, make tradeoff-aware shielding and filtering repairs, maintain a functioning four-chiplet mission computer under safe fictional RF immunity exposure, recover three disturbances, physically disconnect and restore trusted W119 with independently enforced RF shutdown and explicit reset, and pass fresh complete certification. **The real `LevelGamePlayManager` records six authoritative objectives: 6/6.**

**PROJECT SPARK — LEVEL 82: SILENCE THE SPECTRUM. YOU CANNOT SEE THE INTERFERENCE. YOU CAN MEASURE IT. YOU CAN CONTROL IT. YOU CAN SILENCE THE SPECTRUM.**

---

# SOURCE-DERIVED ANNEXES

The following annexes are generated directly from the supplied Level 82 files in this conversation, not guessed from the level-design narrative. Preserve exact spelling and IDs. The `Wiring Concept` rows are **planned, not live wires**; `Terminal Anchor` records are **modeled visual art, not SparkTerminal components**.

## ANNEX A — ALL 72 ACTUAL MODULAR FBX/GLB FILENAMES

| # | Exact FBX filename | GLB |
|---:|---|---|
| 00 | `00_L82_BlackstarEMCChamberBase.fbx` | `00_L82_BlackstarEMCChamberBase.glb` |
| 01 | `01_L82_SemiAnechoicShieldedHall.fbx` | `01_L82_SemiAnechoicShieldedHall.glb` |
| 02 | `02_L82_RFAbsorberPyramidWallSet.fbx` | `02_L82_RFAbsorberPyramidWallSet.glb` |
| 03 | `03_L82_RFSealedChamberDoor.fbx` | `03_L82_RFSealedChamberDoor.glb` |
| 04 | `04_L82_RotatingEUTTurntable.fbx` | `04_L82_RotatingEUTTurntable.glb` |
| 05 | `05_L82_TurntableServoGearbox.fbx` | `05_L82_TurntableServoGearbox.glb` |
| 06 | `06_L82_FourChipletMissionComputerEUT.fbx` | `06_L82_FourChipletMissionComputerEUT.glb` |
| 07 | `07_L82_SpacecraftAvionicsChassis.fbx` | `07_L82_SpacecraftAvionicsChassis.glb` |
| 08 | `08_L82_CoPackagedOpticalTransceiver.fbx` | `08_L82_CoPackagedOpticalTransceiver.glb` |
| 09 | `09_L82_RemovableEMCShieldCanopy.fbx` | `09_L82_RemovableEMCShieldCanopy.glb` |
| 10 | `10_L82_ConductiveSeamGasketSet.fbx` | `10_L82_ConductiveSeamGasketSet.glb` |
| 11 | `11_L82_AvionicsAccessHatch.fbx` | `11_L82_AvionicsAccessHatch.glb` |
| 12 | `12_L82_ReferenceGroundPlane.fbx` | `12_L82_ReferenceGroundPlane.glb` |
| 13 | `13_L82_ChassisEquipotentialBondingGrid.fbx` | `13_L82_ChassisEquipotentialBondingGrid.glb` |
| 14 | `14_L82_ShieldedHarnessBundle.fbx` | `14_L82_ShieldedHarnessBundle.glb` |
| 15 | `15_L82_HighSpeedDifferentialCable.fbx` | `15_L82_HighSpeedDifferentialCable.glb` |
| 16 | `16_L82_FilteredBulkheadFeedthrough.fbx` | `16_L82_FilteredBulkheadFeedthrough.glb` |
| 17 | `17_L82_DCFeedthroughFilterArray.fbx` | `17_L82_DCFeedthroughFilterArray.glb` |
| 18 | `18_L82_CommonModeChokeModule.fbx` | `18_L82_CommonModeChokeModule.glb` |
| 19 | `19_L82_DifferentialLCFilterModule.fbx` | `19_L82_DifferentialLCFilterModule.glb` |
| 20 | `20_L82_ClipOnFerriteCoreRack.fbx` | `20_L82_ClipOnFerriteCoreRack.glb` |
| 21 | `21_L82_CableRoutingTray.fbx` | `21_L82_CableRoutingTray.glb` |
| 22 | `22_L82_360DegreeShieldTerminationClamp.fbx` | `22_L82_360DegreeShieldTerminationClamp.glb` |
| 23 | `23_L82_ShieldGasketCompressionTester.fbx` | `23_L82_ShieldGasketCompressionTester.glb` |
| 24 | `24_L82_EnclosureSeamLeakageProbe.fbx` | `24_L82_EnclosureSeamLeakageProbe.glb` |
| 25 | `25_L82_NearFieldMagneticHProbe.fbx` | `25_L82_NearFieldMagneticHProbe.glb` |
| 26 | `26_L82_NearFieldElectricEProbe.fbx` | `26_L82_NearFieldElectricEProbe.glb` |
| 27 | `27_L82_NearFieldScannerXYGantry.fbx` | `27_L82_NearFieldScannerXYGantry.glb` |
| 28 | `28_L82_NearFieldScannerZLift.fbx` | `28_L82_NearFieldScannerZLift.glb` |
| 29 | `29_L82_EMIComplianceMeasurementReceiver.fbx` | `29_L82_EMIComplianceMeasurementReceiver.glb` |
| 30 | `30_L82_HighDynamicRangeSpectrumAnalyzer.fbx` | `30_L82_HighDynamicRangeSpectrumAnalyzer.glb` |
| 31 | `31_L82_RFPreselectorAndAttenuator.fbx` | `31_L82_RFPreselectorAndAttenuator.glb` |
| 32 | `32_L82_SpectralCalibrationReferenceSource.fbx` | `32_L82_SpectralCalibrationReferenceSource.glb` |
| 33 | `33_L82_BroadbandEmissionMeasurementAntenna.fbx` | `33_L82_BroadbandEmissionMeasurementAntenna.glb` |
| 34 | `34_L82_MotorizedAntennaHeightMast.fbx` | `34_L82_MotorizedAntennaHeightMast.glb` |
| 35 | `35_L82_AntennaPolarizationRotationHead.fbx` | `35_L82_AntennaPolarizationRotationHead.glb` |
| 36 | `36_L82_TurntableAzimuthEncoder.fbx` | `36_L82_TurntableAzimuthEncoder.glb` |
| 37 | `37_L82_ThreeDimensionalRadiationPatternHologram.fbx` | `37_L82_ThreeDimensionalRadiationPatternHologram.glb` |
| 38 | `38_L82_EMIFieldVectorHeatmapDisplay.fbx` | `38_L82_EMIFieldVectorHeatmapDisplay.glb` |
| 39 | `39_L82_CommonModeCurrentClamp.fbx` | `39_L82_CommonModeCurrentClamp.glb` |
| 40 | `40_L82_DifferentialCurrentProbe.fbx` | `40_L82_DifferentialCurrentProbe.glb` |
| 41 | `41_L82_DCPowerArtificialNetwork.fbx` | `41_L82_DCPowerArtificialNetwork.glb` |
| 42 | `42_L82_ConductedEmissionMeasurementRack.fbx` | `42_L82_ConductedEmissionMeasurementRack.glb` |
| 43 | `43_L82_RFImmunityCouplingFixture.fbx` | `43_L82_RFImmunityCouplingFixture.glb` |
| 44 | `44_L82_ProgrammableRFSweepGenerator.fbx` | `44_L82_ProgrammableRFSweepGenerator.glb` |
| 45 | `45_L82_ProtectedRFAmplifier.fbx` | `45_L82_ProtectedRFAmplifier.glb` |
| 46 | `46_L82_TriaxialRFFieldStrengthProbe.fbx` | `46_L82_TriaxialRFFieldStrengthProbe.glb` |
| 47 | `47_L82_RFUniformityCalibrationFrame.fbx` | `47_L82_RFUniformityCalibrationFrame.glb` |
| 48 | `48_L82_RFImmunitySafetyPLC.fbx` | `48_L82_RFImmunitySafetyPLC.glb` |
| 49 | `49_L82_ChamberDoorInterlockSwitch.fbx` | `49_L82_ChamberDoorInterlockSwitch.glb` |
| 50 | `50_L82_AnechoicBoundaryConditionMonitor.fbx` | `50_L82_AnechoicBoundaryConditionMonitor.glb` |
| 51 | `51_L82_AvionicsSensorSignalEmulator.fbx` | `51_L82_AvionicsSensorSignalEmulator.glb` |
| 52 | `52_L82_AnalogInputSusceptibilityAnalyzer.fbx` | `52_L82_AnalogInputSusceptibilityAnalyzer.glb` |
| 53 | `53_L82_OpticalLinkJitterAndControlMonitor.fbx` | `53_L82_OpticalLinkJitterAndControlMonitor.glb` |
| 54 | `54_L82_PrecisionClockEMIProbe.fbx` | `54_L82_PrecisionClockEMIProbe.glb` |
| 55 | `55_L82_SwitchModePowerConverterModel.fbx` | `55_L82_SwitchModePowerConverterModel.glb` |
| 56 | `56_L82_SpreadSpectrumClockController.fbx` | `56_L82_SpreadSpectrumClockController.glb` |
| 57 | `57_L82_ProtectedSensorInputFilter.fbx` | `57_L82_ProtectedSensorInputFilter.glb` |
| 58 | `58_L82_InfraredThermalInspectionCamera.fbx` | `58_L82_InfraredThermalInspectionCamera.glb` |
| 59 | `59_L82_ShieldedCoolingAirDuct.fbx` | `59_L82_ShieldedCoolingAirDuct.glb` |
| 60 | `60_L82_RealTimeMissionWorkloadGenerator.fbx` | `60_L82_RealTimeMissionWorkloadGenerator.glb` |
| 61 | `61_L82_FaultDiagnosisAndRepairRouter.fbx` | `61_L82_FaultDiagnosisAndRepairRouter.glb` |
| 62 | `62_L82_CableInjectionFaultFixture.fbx` | `62_L82_CableInjectionFaultFixture.glb` |
| 63 | `63_L82_GroundReturnFaultSwitch.fbx` | `63_L82_GroundReturnFaultSwitch.glb` |
| 64 | `64_L82_SeamGapFaultActuator.fbx` | `64_L82_SeamGapFaultActuator.glb` |
| 65 | `65_L82_RFDesenseFaultEmulator.fbx` | `65_L82_RFDesenseFaultEmulator.glb` |
| 66 | `66_L82_HolographicEMCAnalysisSCADA.fbx` | `66_L82_HolographicEMCAnalysisSCADA.glb` |
| 67 | `67_L82_EMCTestSequencingPLCRack.fbx` | `67_L82_EMCTestSequencingPLCRack.glb` |
| 68 | `68_L82_Regulated5VControlSupply.fbx` | `68_L82_Regulated5VControlSupply.glb` |
| 69 | `69_L82_RFSafetyPowerCabinet.fbx` | `69_L82_RFSafetyPowerCabinet.glb` |
| 70 | `70_L82_EMCCertificationHologram.fbx` | `70_L82_EMCCertificationHologram.glb` |
| 71 | `71_L82_EmergencyRFStopStation.fbx` | `71_L82_EmergencyRFStopStation.glb` |

Assembled preview: `Models/Level82_Complete_ArtKit_Showcase.fbx` and `GLB/Level82_Complete_ArtKit_Showcase.glb`. Import as an alternate reference only.


## ANNEX B — ALL 54 VERIFIED PIVOT PARENTS AND WORLD POSITIONS

| # | Pivot | Owning model | Type | Axis | XYZ (m) |
|---:|---|---|---|:---:|---|
| 01 | `ANIM_CHAMBER_RF_DOOR` | `L82_RFSealedChamberDoor` | rotation | y | (-11.85, 3.12, 2.95) |
| 02 | `ANIM_EUT_TURNTABLE` | `L82_RotatingEUTTurntable` | rotation | y | (0, 1.18, -0.65) |
| 03 | `ANIM_TURNTABLE_LOCK` | `L82_TurntableServoGearbox` | translation | x | (2.85, 1.02, -1.7) |
| 04 | `ANIM_EUT_CHASSIS_LID` | `L82_SpacecraftAvionicsChassis` | rotation | x | (0, 2.57, -0.65) |
| 05 | `ANIM_SHIELD_CANOPY_LIFT` | `L82_RemovableEMCShieldCanopy` | translation | y | (0, 3.35, -0.65) |
| 06 | `ANIM_ACCESS_HATCH` | `L82_AvionicsAccessHatch` | rotation | x | (-1.15, 2.7, 0.7) |
| 07 | `ANIM_ABSORBER_SERVICE_PANEL` | `L82_RFAbsorberPyramidWallSet` | translation | z | (0, 4.3, -0.7) |
| 08 | `ANIM_GROUND_PLANE_ACCESS` | `L82_ReferenceGroundPlane` | translation | z | (0, 0.9, -0.65) |
| 09 | `ANIM_SEAM_GASKET_COMPRESSION` | `L82_ConductiveSeamGasketSet` | translation | y | (0, 3.1, -0.65) |
| 10 | `ANIM_SHIELDED_AIR_DUCT` | `L82_ShieldedCoolingAirDuct` | rotation | z | (4.0, 3.96, -3.2) |
| 11 | `ANIM_EUT_LOAD_TRAY` | `L82_FourChipletMissionComputerEUT` | translation | z | (0, 2.7, -0.65) |
| 12 | `ANIM_AZIMUTH_ENCODER` | `L82_TurntableAzimuthEncoder` | rotation | y | (-2.25, 1.22, -2.85) |
| 13 | `ANIM_ANTENNA_MAST_VERTICAL` | `L82_MotorizedAntennaHeightMast` | translation | y | (-8.75, 4.0, -2.18) |
| 14 | `ANIM_ANTENNA_POLARIZATION` | `L82_AntennaPolarizationRotationHead` | rotation | z | (-8.75, 4.33, -2.18) |
| 15 | `ANIM_ANTENNA_DISTANCE_SLIDE` | `L82_BroadbandEmissionMeasurementAntenna` | translation | x | (-8.75, 4.2, -2.15) |
| 16 | `ANIM_NEARFIELD_SCAN_X` | `L82_NearFieldScannerXYGantry` | translation | x | (5.68, 4.95, -1.35) |
| 17 | `ANIM_NEARFIELD_SCAN_Y` | `L82_NearFieldScannerXYGantry` | translation | z | (5.68, 4.95, -1.35) |
| 18 | `ANIM_NEARFIELD_SCAN_Z` | `L82_NearFieldScannerZLift` | translation | y | (5.7, 3.34, -1.5) |
| 19 | `ANIM_EFIELD_PROBE_ARM` | `L82_NearFieldElectricEProbe` | translation | y | (6.02, 3.95, -1.2) |
| 20 | `ANIM_HFIELD_PROBE_ARM` | `L82_NearFieldMagneticHProbe` | translation | y | (5.25, 3.95, -1.25) |
| 21 | `ANIM_SEAM_LEAK_PROBE` | `L82_EnclosureSeamLeakageProbe` | translation | x | (-4.2, 3.73, 1.2) |
| 22 | `ANIM_COMMONMODE_CLAMP` | `L82_CommonModeCurrentClamp` | rotation | z | (-6.9, 2.55, 3.8) |
| 23 | `ANIM_DIFFERENTIAL_PROBE` | `L82_DifferentialCurrentProbe` | translation | y | (-5.66, 2.52, 3.7) |
| 24 | `ANIM_RF_PRESELECTOR_DIAL` | `L82_RFPreselectorAndAttenuator` | rotation | z | (-10.45, 1.53, 4.45) |
| 25 | `ANIM_SPECTRUM_SWEEP_INDICATOR` | `L82_HighDynamicRangeSpectrumAnalyzer` | indicator | none | (-8.15, 2.22, 4.05) |
| 26 | `ANIM_RADIATION_HOLOGRAM` | `L82_ThreeDimensionalRadiationPatternHologram` | indicator | none | (-3.1, 6.75, -4.3) |
| 27 | `ANIM_HARNESS_CABLE_TRAY` | `L82_CableRoutingTray` | translation | z | (-2.2, 1.27, 4.55) |
| 28 | `ANIM_DIFFERENTIAL_CABLE_ROUTE` | `L82_HighSpeedDifferentialCable` | translation | x | (-3.4, 2.04, 2.22) |
| 29 | `ANIM_FEEDTHROUGH_PANEL` | `L82_FilteredBulkheadFeedthrough` | translation | z | (-4.6, 1.65, 3.2) |
| 30 | `ANIM_DC_FILTER_TRAY` | `L82_DCFeedthroughFilterArray` | translation | x | (-7.35, 1.68, 4.58) |
| 31 | `ANIM_COMMONMODE_CHOKE_INSERT` | `L82_CommonModeChokeModule` | translation | y | (-5.93, 1.52, 4.6) |
| 32 | `ANIM_LC_FILTER_INSERT` | `L82_DifferentialLCFilterModule` | translation | x | (-4.75, 1.53, 4.6) |
| 33 | `ANIM_FERRITE_CLIP_LATCH` | `L82_ClipOnFerriteCoreRack` | rotation | x | (-3.62, 1.56, 4.62) |
| 34 | `ANIM_SHIELD_TERMINATION_CLAMP` | `L82_360DegreeShieldTerminationClamp` | rotation | x | (-4.4, 2.22, 3.7) |
| 35 | `ANIM_BONDING_STRAP_LATCH` | `L82_ChassisEquipotentialBondingGrid` | rotation | z | (-3.6, 0.9, -0.65) |
| 36 | `ANIM_GASKET_TEST_HEAD` | `L82_ShieldGasketCompressionTester` | translation | y | (-6.55, 3.0, 3.45) |
| 37 | `ANIM_SWITCHER_POWER_MODULE` | `L82_SwitchModePowerConverterModel` | translation | x | (3.9, 2.03, 2.0) |
| 38 | `ANIM_SPREAD_SPECTRUM_DIAL` | `L82_SpreadSpectrumClockController` | rotation | z | (5.0, 1.55, 4.53) |
| 39 | `ANIM_SENSOR_FILTER_DRAWER` | `L82_ProtectedSensorInputFilter` | translation | z | (6.02, 1.5, 4.6) |
| 40 | `ANIM_THERMAL_CAMERA_PAN` | `L82_InfraredThermalInspectionCamera` | rotation | y | (4.9, 4.85, 1.75) |
| 41 | `ANIM_RF_COUPLING_FIXTURE` | `L82_RFImmunityCouplingFixture` | translation | x | (9.14, 4.2, -2.2) |
| 42 | `ANIM_RF_GENERATOR_DIAL` | `L82_ProgrammableRFSweepGenerator` | rotation | z | (8.95, 2.75, 3.4) |
| 43 | `ANIM_RF_AMPLIFIER_SHUTTER` | `L82_ProtectedRFAmplifier` | translation | y | (10.55, 2.76, 3.25) |
| 44 | `ANIM_FIELD_PROBE_CARRIAGE` | `L82_TriaxialRFFieldStrengthProbe` | translation | y | (8.1, 4.17, -0.8) |
| 45 | `ANIM_RF_CALIBRATION_FRAME` | `L82_RFUniformityCalibrationFrame` | translation | x | (10.65, 4.0, -2.5) |
| 46 | `ANIM_DOOR_INTERLOCK_LEVER` | `L82_ChamberDoorInterlockSwitch` | rotation | x | (-11.75, 2.62, 2.3) |
| 47 | `ANIM_RF_SAFETY_ESTOP` | `L82_EmergencyRFStopStation` | translation | z | (11.48, 1.62, 5.8) |
| 48 | `ANIM_SENSOR_INPUT_STREAM` | `L82_AvionicsSensorSignalEmulator` | indicator | none | (-1.1, 1.58, 4.57) |
| 49 | `ANIM_OPTICAL_LINK_MONITOR` | `L82_OpticalLinkJitterAndControlMonitor` | indicator | none | (1.72, 1.55, 4.62) |
| 50 | `ANIM_CLOCK_JITTER_DISPLAY` | `L82_PrecisionClockEMIProbe` | indicator | none | (3.1, 1.57, 4.45) |
| 51 | `ANIM_FAULT_CABLE_INJECTOR` | `L82_CableInjectionFaultFixture` | translation | x | (6.25, 1.58, 4.55) |
| 52 | `ANIM_SEAM_FAULT_ACTUATOR` | `L82_SeamGapFaultActuator` | translation | y | (-1.35, 3.1, 1.35) |
| 53 | `ANIM_SCADA_EMC_DISPLAY` | `L82_HolographicEMCAnalysisSCADA` | indicator | none | (0, 7.3, -4.8) |
| 54 | `ANIM_CERTIFICATION_BEACON` | `L82_EMCCertificationHologram` | indicator | none | (0, 8.3, -4.9) |

`indicator` type pivots are animation anchor parents, not mechanical axes. Use each model’s actual FBX transform as authoritative local pivot.


## ANNEX C — EVERY SOURCE W001–W134 PLANNED ELECTRICAL CONNECTION

| Wire | Source model / socket | Target model / socket | Signal kind | Actual concept purpose |
|---|---|---|---|---|
| **W001** | `L82_Regulated5VControlSupply` / `P195` | `L82_EMCTestSequencingPLCRack` / `P191` | POWER | 5V sequencing PLC logic feed |
| **W002** | `L82_Regulated5VControlSupply` / `P196` | `L82_RFImmunitySafetyPLC` / `P126` | POWER | 5V immunity safety PLC logic feed |
| **W003** | `L82_Regulated5VControlSupply` / `P197` | `L82_EMIComplianceMeasurementReceiver` / `P069` | POWER | 5V EMI receiver control feed |
| **W004** | `L82_Regulated5VControlSupply` / `P198` | `L82_HighDynamicRangeSpectrumAnalyzer` / `P073` | POWER | 5V spectrum analyzer logic feed |
| **W005** | `L82_Regulated5VControlSupply` / `P195` | `L82_ProgrammableRFSweepGenerator` / `P111` | POWER | 5V RF sweep controller logic feed |
| **W006** | `L82_Regulated5VControlSupply` / `P196` | `L82_TriaxialRFFieldStrengthProbe` / `P119` | POWER | 5V RF field probe signal conditioning |
| **W007** | `L82_Regulated5VControlSupply` / `P197` | `L82_NearFieldScannerXYGantry` / `P064` | POWER | 5V near-field scanning controller feed |
| **W008** | `L82_Regulated5VControlSupply` / `P198` | `L82_RotatingEUTTurntable` / `P004` | POWER | 5V turntable servo control interface |
| **W009** | `L82_Regulated5VControlSupply` / `P195` | `L82_FaultDiagnosisAndRepairRouter` / `P173` | POWER | 5V fault diagnostic router logic feed |
| **W010** | `L82_Regulated5VControlSupply` / `P196` | `L82_HolographicEMCAnalysisSCADA` / `P188` | POWER | 5V EMC SCADA logic feed |
| **W011** | `L82_EMCTestSequencingPLCRack` / `P192` | `L82_RotatingEUTTurntable` / `P005` | DIGITAL_COMMAND_OR_STATUS | Turntable azimuth command |
| **W012** | `L82_RotatingEUTTurntable` / `P006` | `L82_TurntableAzimuthEncoder` / `P091` | ANALOG_FEEDBACK | Turntable actual angle sensing |
| **W013** | `L82_TurntableAzimuthEncoder` / `P092` | `L82_EMCTestSequencingPLCRack` / `P193` | ANALOG_FEEDBACK | Measured turntable azimuth feedback |
| **W014** | `L82_EMCTestSequencingPLCRack` / `P194` | `L82_TurntableServoGearbox` / `P007` | DIGITAL_COMMAND_OR_STATUS | Turntable gearbox run command |
| **W015** | `L82_TurntableServoGearbox` / `P008` | `L82_EMCTestSequencingPLCRack` / `P191` | ANALOG_FEEDBACK | Turntable gearbox ready status |
| **W016** | `L82_EMCTestSequencingPLCRack` / `P192` | `L82_MotorizedAntennaHeightMast` / `P085` | DIGITAL_COMMAND_OR_STATUS | Antenna mast vertical position request |
| **W017** | `L82_MotorizedAntennaHeightMast` / `P086` | `L82_EMCTestSequencingPLCRack` / `P193` | ANALOG_FEEDBACK | Antenna mast actual-height feedback |
| **W018** | `L82_EMCTestSequencingPLCRack` / `P194` | `L82_AntennaPolarizationRotationHead` / `P088` | DIGITAL_COMMAND_OR_STATUS | Antenna polarization rotation command |
| **W019** | `L82_AntennaPolarizationRotationHead` / `P089` | `L82_EMCTestSequencingPLCRack` / `P191` | ANALOG_FEEDBACK | Antenna actual polarization feedback |
| **W020** | `L82_EMCTestSequencingPLCRack` / `P192` | `L82_BroadbandEmissionMeasurementAntenna` / `P082` | DIGITAL_COMMAND_OR_STATUS | Antenna position/coupling readiness request |
| **W021** | `L82_BroadbandEmissionMeasurementAntenna` / `P083` | `L82_EMCTestSequencingPLCRack` / `P193` | ANALOG_FEEDBACK | Antenna system measured status |
| **W022** | `L82_EMCTestSequencingPLCRack` / `P194` | `L82_NearFieldScannerXYGantry` / `P065` | ANALOG_FEEDBACK | Nearfield gantry XY scan command |
| **W023** | `L82_NearFieldScannerXYGantry` / `P066` | `L82_EMCTestSequencingPLCRack` / `P191` | ANALOG_FEEDBACK | Nearfield XY actual position feedback |
| **W024** | `L82_EMCTestSequencingPLCRack` / `P192` | `L82_NearFieldScannerZLift` / `P067` | ANALOG_FEEDBACK | Nearfield scan Z lift setpoint |
| **W025** | `L82_NearFieldScannerZLift` / `P068` | `L82_EMCTestSequencingPLCRack` / `P193` | ANALOG_FEEDBACK | Nearfield Z actual position feedback |
| **W026** | `L82_NearFieldScannerXYGantry` / `P064` | `L82_NearFieldMagneticHProbe` / `P058` | ANALOG_FEEDBACK | Nearfield magnetic probe carriage control |
| **W027** | `L82_NearFieldScannerXYGantry` / `P065` | `L82_NearFieldElectricEProbe` / `P061` | ANALOG_FEEDBACK | Nearfield electric probe carriage control |
| **W028** | `L82_EMCTestSequencingPLCRack` / `P194` | `L82_TurntableAzimuthEncoder` / `P093` | DIGITAL_COMMAND_OR_STATUS | Turntable sweep home calibration request |
| **W029** | `L82_EMCTestSequencingPLCRack` / `P191` | `L82_DCPowerArtificialNetwork` / `P100` | DIGITAL_COMMAND_OR_STATUS | DC power artificial-network safe enable |
| **W030** | `L82_DCPowerArtificialNetwork` / `P101` | `L82_ConductedEmissionMeasurementRack` / `P104` | ANALOG_FEEDBACK | Conducted emissions source measurement |
| **W031** | `L82_ConductedEmissionMeasurementRack` / `P105` | `L82_EMIComplianceMeasurementReceiver` / `P070` | ANALOG_FEEDBACK | Conducted noise spectrum receiver path |
| **W032** | `L82_CommonModeCurrentClamp` / `P094` | `L82_ConductedEmissionMeasurementRack` / `P106` | ANALOG_FEEDBACK | Common-mode cable clamp measured current |
| **W033** | `L82_DifferentialCurrentProbe` / `P097` | `L82_ConductedEmissionMeasurementRack` / `P107` | ANALOG_FEEDBACK | Differential cable current measured value |
| **W034** | `L82_DCPowerArtificialNetwork` / `P102` | `L82_CommonModeCurrentClamp` / `P095` | DIGITAL_COMMAND_OR_STATUS | Common-mode clamp calibration request |
| **W035** | `L82_DCPowerArtificialNetwork` / `P103` | `L82_DifferentialCurrentProbe` / `P098` | DIGITAL_COMMAND_OR_STATUS | Differential clamp calibration request |
| **W036** | `L82_ChassisEquipotentialBondingGrid` / `P025` | `L82_ConductedEmissionMeasurementRack` / `P104` | ANALOG_FEEDBACK | Chassis bonding monitor status |
| **W037** | `L82_360DegreeShieldTerminationClamp` / `P050` | `L82_ChassisEquipotentialBondingGrid` / `P026` | ANALOG_FEEDBACK | Shield termination to bonding grid status |
| **W038** | `L82_ShieldedHarnessBundle` / `P028` | `L82_CommonModeCurrentClamp` / `P096` | ANALOG_FEEDBACK | Harness common-mode current monitor |
| **W039** | `L82_HighSpeedDifferentialCable` / `P031` | `L82_DifferentialCurrentProbe` / `P099` | ANALOG_FEEDBACK | Differential harness response measurement |
| **W040** | `L82_CommonModeChokeModule` / `P040` | `L82_ConductedEmissionMeasurementRack` / `P105` | DIGITAL_COMMAND_OR_STATUS | Common-mode choke insertion attenuation telemetry |
| **W041** | `L82_DifferentialLCFilterModule` / `P043` | `L82_ConductedEmissionMeasurementRack` / `P106` | DIGITAL_COMMAND_OR_STATUS | Differential LC filter loss telemetry |
| **W042** | `L82_DCFeedthroughFilterArray` / `P037` | `L82_DCPowerArtificialNetwork` / `P100` | ANALOG_FEEDBACK | Feedthrough filter DC path measurement |
| **W043** | `L82_SwitchModePowerConverterModel` / `P152` | `L82_DCPowerArtificialNetwork` / `P101` | DIGITAL_COMMAND_OR_STATUS | Switch-mode converter conducted noise proxy |
| **W044** | `L82_ConductedEmissionMeasurementRack` / `P107` | `L82_EMCTestSequencingPLCRack` / `P192` | DIGITAL_COMMAND_OR_STATUS | Conducted scan passfail and margin |
| **W045** | `L82_EMIComplianceMeasurementReceiver` / `P071` | `L82_ConductedEmissionMeasurementRack` / `P104` | DIGITAL_COMMAND_OR_STATUS | Receiver detector bandwidth setup |
| **W046** | `L82_SpectralCalibrationReferenceSource` / `P079` | `L82_EMIComplianceMeasurementReceiver` / `P072` | DIGITAL_COMMAND_OR_STATUS | Spectral reference receiver calibration |
| **W047** | `L82_ConductedEmissionMeasurementRack` / `P105` | `L82_HolographicEMCAnalysisSCADA` / `P189` | ANALOG_FEEDBACK | Conducted spectrum SCADA telemetry |
| **W048** | `L82_EMCTestSequencingPLCRack` / `P193` | `L82_ConductedEmissionMeasurementRack` / `P106` | DIGITAL_COMMAND_OR_STATUS | Conducted emission scan initiate |
| **W049** | `L82_EMCTestSequencingPLCRack` / `P194` | `L82_EMIComplianceMeasurementReceiver` / `P069` | DIGITAL_COMMAND_OR_STATUS | Radiated emissions receiver acquisition request |
| **W050** | `L82_BroadbandEmissionMeasurementAntenna` / `P084` | `L82_EMIComplianceMeasurementReceiver` / `P070` | DIGITAL_COMMAND_OR_STATUS | Emission antenna sensed RF input |
| **W051** | `L82_EMIComplianceMeasurementReceiver` / `P071` | `L82_HighDynamicRangeSpectrumAnalyzer` / `P074` | ANALOG_FEEDBACK | Measured EMI spectrum into frequency analyzer |
| **W052** | `L82_HighDynamicRangeSpectrumAnalyzer` / `P075` | `L82_HolographicEMCAnalysisSCADA` / `P190` | ANALOG_FEEDBACK | Radiated spectrum telemetry to SCADA |
| **W053** | `L82_SpectralCalibrationReferenceSource` / `P080` | `L82_HighDynamicRangeSpectrumAnalyzer` / `P076` | DIGITAL_COMMAND_OR_STATUS | Instrument spectral reference calibration |
| **W054** | `L82_RFPreselectorAndAttenuator` / `P077` | `L82_EMIComplianceMeasurementReceiver` / `P072` | ANALOG_FEEDBACK | Preselector actual attenuation and tuning |
| **W055** | `L82_EMIComplianceMeasurementReceiver` / `P069` | `L82_RFPreselectorAndAttenuator` / `P078` | DIGITAL_COMMAND_OR_STATUS | Receiver front-end filter request |
| **W056** | `L82_MotorizedAntennaHeightMast` / `P087` | `L82_EMIComplianceMeasurementReceiver` / `P070` | DIGITAL_COMMAND_OR_STATUS | Antenna mast height association |
| **W057** | `L82_AntennaPolarizationRotationHead` / `P090` | `L82_EMIComplianceMeasurementReceiver` / `P071` | DIGITAL_COMMAND_OR_STATUS | Antenna polarization association |
| **W058** | `L82_TurntableAzimuthEncoder` / `P091` | `L82_EMIComplianceMeasurementReceiver` / `P072` | ANALOG_FEEDBACK | Turntable measured azimuth association |
| **W059** | `L82_NearFieldMagneticHProbe` / `P059` | `L82_EMIComplianceMeasurementReceiver` / `P069` | ANALOG_FEEDBACK | Magnetic nearfield probe emission measurement |
| **W060** | `L82_NearFieldElectricEProbe` / `P062` | `L82_EMIComplianceMeasurementReceiver` / `P070` | ANALOG_FEEDBACK | Electric nearfield probe emission measurement |
| **W061** | `L82_EnclosureSeamLeakageProbe` / `P055` | `L82_EMIComplianceMeasurementReceiver` / `P071` | ANALOG_FEEDBACK | Shield seam leakage measurement |
| **W062** | `L82_EMIComplianceMeasurementReceiver` / `P072` | `L82_EMCTestSequencingPLCRack` / `P191` | DIGITAL_COMMAND_OR_STATUS | Emission worst-case result validity |
| **W063** | `L82_HighDynamicRangeSpectrumAnalyzer` / `P073` | `L82_EMCTestSequencingPLCRack` / `P192` | DIGITAL_COMMAND_OR_STATUS | Spectral peak and detector certificate |
| **W064** | `L82_EMCTestSequencingPLCRack` / `P193` | `L82_SpectralCalibrationReferenceSource` / `P081` | DIGITAL_COMMAND_OR_STATUS | Calibration source enable request |
| **W065** | `L82_EMIComplianceMeasurementReceiver` / `P069` | `L82_HolographicEMCAnalysisSCADA` / `P188` | ANALOG_FEEDBACK | Field heatmap instrumentation telemetry |
| **W066** | `L82_EMCTestSequencingPLCRack` / `P194` | `L82_NearFieldMagneticHProbe` / `P060` | ANALOG_FEEDBACK | Nearfield probe head measurement enable |
| **W067** | `L82_FaultDiagnosisAndRepairRouter` / `P174` | `L82_ShieldGasketCompressionTester` / `P053` | DIGITAL_COMMAND_OR_STATUS | Shield gasket compression test request |
| **W068** | `L82_ShieldGasketCompressionTester` / `P054` | `L82_FaultDiagnosisAndRepairRouter` / `P175` | ANALOG_FEEDBACK | Measured gasket compression and closure status |
| **W069** | `L82_FaultDiagnosisAndRepairRouter` / `P173` | `L82_360DegreeShieldTerminationClamp` / `P051` | DIGITAL_COMMAND_OR_STATUS | 360 degree cable shield clamp command |
| **W070** | `L82_360DegreeShieldTerminationClamp` / `P052` | `L82_FaultDiagnosisAndRepairRouter` / `P174` | ANALOG_FEEDBACK | Measured shield termination contact state |
| **W071** | `L82_FaultDiagnosisAndRepairRouter` / `P175` | `L82_CommonModeChokeModule` / `P041` | DIGITAL_COMMAND_OR_STATUS | Common-mode choke installation request |
| **W072** | `L82_CommonModeChokeModule` / `P042` | `L82_FaultDiagnosisAndRepairRouter` / `P173` | DIGITAL_COMMAND_OR_STATUS | Common-mode choke installation confirmation |
| **W073** | `L82_FaultDiagnosisAndRepairRouter` / `P174` | `L82_DifferentialLCFilterModule` / `P044` | DIGITAL_COMMAND_OR_STATUS | Differential LC filter installation request |
| **W074** | `L82_DifferentialLCFilterModule` / `P045` | `L82_FaultDiagnosisAndRepairRouter` / `P175` | DIGITAL_COMMAND_OR_STATUS | LC filter installed condition |
| **W075** | `L82_FaultDiagnosisAndRepairRouter` / `P173` | `L82_ClipOnFerriteCoreRack` / `P046` | DIGITAL_COMMAND_OR_STATUS | Ferrite latch installation request |
| **W076** | `L82_ClipOnFerriteCoreRack` / `P047` | `L82_FaultDiagnosisAndRepairRouter` / `P174` | ANALOG_FEEDBACK | Ferrite component fitted feedback |
| **W077** | `L82_FaultDiagnosisAndRepairRouter` / `P175` | `L82_DCFeedthroughFilterArray` / `P038` | DIGITAL_COMMAND_OR_STATUS | DC feedthrough filter selection request |
| **W078** | `L82_DCFeedthroughFilterArray` / `P039` | `L82_FaultDiagnosisAndRepairRouter` / `P173` | ANALOG_FEEDBACK | Feedthrough filter qualified status |
| **W079** | `L82_FaultDiagnosisAndRepairRouter` / `P174` | `L82_CableRoutingTray` / `P048` | DIGITAL_COMMAND_OR_STATUS | Harness routing tray position request |
| **W080** | `L82_CableRoutingTray` / `P049` | `L82_FaultDiagnosisAndRepairRouter` / `P175` | ANALOG_FEEDBACK | Harness routing actual state |
| **W081** | `L82_FaultDiagnosisAndRepairRouter` / `P173` | `L82_ChassisEquipotentialBondingGrid` / `P027` | ANALOG_FEEDBACK | Chassis bonding continuity measurement request |
| **W082** | `L82_ChassisEquipotentialBondingGrid` / `P025` | `L82_FaultDiagnosisAndRepairRouter` / `P174` | ANALOG_FEEDBACK | Chassis bonding integrity feedback |
| **W083** | `L82_FaultDiagnosisAndRepairRouter` / `P175` | `L82_ProtectedSensorInputFilter` / `P160` | ANALOG_FEEDBACK | Sensor input filter commissioning request |
| **W084** | `L82_ProtectedSensorInputFilter` / `P161` | `L82_FaultDiagnosisAndRepairRouter` / `P173` | ANALOG_FEEDBACK | Sensor input filter response telemetry |
| **W085** | `L82_EMCTestSequencingPLCRack` / `P191` | `L82_ProgrammableRFSweepGenerator` / `P112` | DIGITAL_COMMAND_OR_STATUS | Protected immunity sweep generator request |
| **W086** | `L82_ProgrammableRFSweepGenerator` / `P113` | `L82_ProtectedRFAmplifier` / `P115` | DIGITAL_COMMAND_OR_STATUS | Programmed RF test waveform to amplifier |
| **W087** | `L82_RFImmunitySafetyPLC` / `P127` | `L82_ProtectedRFAmplifier` / `P116` | DIGITAL_COMMAND_OR_STATUS | Immunity safety output authorization |
| **W088** | `L82_ProtectedRFAmplifier` / `P117` | `L82_RFImmunitySafetyPLC` / `P128` | ANALOG_FEEDBACK | Actual RF amplifier state feedback |
| **W089** | `L82_TriaxialRFFieldStrengthProbe` / `P120` | `L82_RFImmunitySafetyPLC` / `P129` | ANALOG_FEEDBACK | Field monitoring auxiliary validity |
| **W090** | `L82_EMCTestSequencingPLCRack` / `P192` | `L82_RFUniformityCalibrationFrame` / `P123` | ANALOG_FEEDBACK | Field uniformity frame calibration request |
| **W091** | `L82_RFUniformityCalibrationFrame` / `P124` | `L82_TriaxialRFFieldStrengthProbe` / `P121` | ANALOG_FEEDBACK | Field probe calibration reference state |
| **W092** | `L82_TriaxialRFFieldStrengthProbe` / `P122` | `L82_EMCTestSequencingPLCRack` / `P193` | ANALOG_FEEDBACK | Calibrated RF field measurement telemetry |
| **W093** | `L82_ProgrammableRFSweepGenerator` / `P114` | `L82_EMCTestSequencingPLCRack` / `P194` | ANALOG_FEEDBACK | RF sweep frequency and modulation status |
| **W094** | `L82_ProtectedRFAmplifier` / `P118` | `L82_RFImmunityCouplingFixture` / `P108` | DIGITAL_COMMAND_OR_STATUS | Amplified RF stimulus to coupling fixture supervisory state |
| **W095** | `L82_RFImmunityCouplingFixture` / `P109` | `L82_EMCTestSequencingPLCRack` / `P191` | DIGITAL_COMMAND_OR_STATUS | RF coupling fixture readiness |
| **W096** | `L82_RFImmunitySafetyPLC` / `P126` | `L82_RFImmunityCouplingFixture` / `P110` | DIGITAL_COMMAND_OR_STATUS | RF coupling fixture inhibit command |
| **W097** | `L82_EMCTestSequencingPLCRack` / `P192` | `L82_AvionicsSensorSignalEmulator` / `P136` | ANALOG_FEEDBACK | Sensor emulator qualification during RF test |
| **W098** | `L82_EMCTestSequencingPLCRack` / `P193` | `L82_OpticalLinkJitterAndControlMonitor` / `P144` | DIGITAL_COMMAND_OR_STATUS | Optical link functional check request |
| **W099** | `L82_EMCTestSequencingPLCRack` / `P194` | `L82_PrecisionClockEMIProbe` / `P148` | DIGITAL_COMMAND_OR_STATUS | Clock stability functional check request |
| **W100** | `L82_EMCTestSequencingPLCRack` / `P191` | `L82_RealTimeMissionWorkloadGenerator` / `P170` | DIGITAL_COMMAND_OR_STATUS | Functional mission workload test request |
| **W101** | `L82_AvionicsSensorSignalEmulator` / `P137` | `L82_AnalogInputSusceptibilityAnalyzer` / `P140` | ANALOG_FEEDBACK | Sensor emulator sampled analog input |
| **W102** | `L82_AnalogInputSusceptibilityAnalyzer` / `P141` | `L82_RealTimeMissionWorkloadGenerator` / `P171` | ANALOG_FEEDBACK | Measured ADC deviation mission test result |
| **W103** | `L82_OpticalLinkJitterAndControlMonitor` / `P145` | `L82_RealTimeMissionWorkloadGenerator` / `P172` | ANALOG_FEEDBACK | Optical link and telemetry jitter measurement |
| **W104** | `L82_PrecisionClockEMIProbe` / `P149` | `L82_RealTimeMissionWorkloadGenerator` / `P170` | ANALOG_FEEDBACK | Precision mission-clock interference measurement |
| **W105** | `L82_SwitchModePowerConverterModel` / `P153` | `L82_AnalogInputSusceptibilityAnalyzer` / `P142` | DIGITAL_COMMAND_OR_STATUS | Power converter induced ADC disturbance coupling |
| **W106** | `L82_SpreadSpectrumClockController` / `P156` | `L82_PrecisionClockEMIProbe` / `P150` | ANALOG_FEEDBACK | Spread spectrum timing profile monitor |
| **W107** | `L82_ProtectedSensorInputFilter` / `P162` | `L82_AnalogInputSusceptibilityAnalyzer` / `P143` | ANALOG_FEEDBACK | Filtered sensor signal actual response |
| **W108** | `L82_InfraredThermalInspectionCamera` / `P164` | `L82_RealTimeMissionWorkloadGenerator` / `P171` | ANALOG_FEEDBACK | Infrared thermal enclosure health measurement |
| **W109** | `L82_RealTimeMissionWorkloadGenerator` / `P172` | `L82_EMCTestSequencingPLCRack` / `P192` | ANALOG_FEEDBACK | Mission workload functional passfail status |
| **W110** | `L82_RealTimeMissionWorkloadGenerator` / `P170` | `L82_RFImmunitySafetyPLC` / `P127` | DIGITAL_COMMAND_OR_STATUS | Mission command safety validity |
| **W111** | `L82_CableInjectionFaultFixture` / `P176` | `L82_FaultDiagnosisAndRepairRouter` / `P174` | DIGITAL_COMMAND_OR_STATUS | Common-mode cable injection fault report |
| **W112** | `L82_GroundReturnFaultSwitch` / `P179` | `L82_FaultDiagnosisAndRepairRouter` / `P175` | DIGITAL_COMMAND_OR_STATUS | Ground and return fault actuator report |
| **W113** | `L82_SeamGapFaultActuator` / `P182` | `L82_FaultDiagnosisAndRepairRouter` / `P173` | DIGITAL_COMMAND_OR_STATUS | Seam-gap actuator fault report |
| **W114** | `L82_RFDesenseFaultEmulator` / `P185` | `L82_FaultDiagnosisAndRepairRouter` / `P174` | DIGITAL_COMMAND_OR_STATUS | RF desense virtual fault state |
| **W115** | `L82_FaultDiagnosisAndRepairRouter` / `P175` | `L82_EMCTestSequencingPLCRack` / `P193` | DIGITAL_COMMAND_OR_STATUS | Diagnostic sourcepathreceiver determination |
| **W116** | `L82_FaultDiagnosisAndRepairRouter` / `P173` | `L82_HolographicEMCAnalysisSCADA` / `P189` | DIGITAL_COMMAND_OR_STATUS | EMC fault diagnosis and repair telemetry |
| **W117** | `L82_EMCTestSequencingPLCRack` / `P194` | `L82_RFImmunitySafetyPLC` / `P128` | DIGITAL_COMMAND_OR_STATUS | RF exposure safety permit request |
| **W118** | `L82_ChamberDoorInterlockSwitch` / `P130` | `L82_RFImmunitySafetyPLC` / `P129` | ANALOG_FEEDBACK | Chamber door actual interlock status |
| **W119** **CRITICAL** | `L82_TriaxialRFFieldStrengthProbe` / `P119` | `L82_RFImmunitySafetyPLC` / `P126` | ANALOG_FEEDBACK | CRITICAL trusted measured RF field strength feedback |
| **W120** | `L82_TriaxialRFFieldStrengthProbe` / `P120` | `L82_HolographicEMCAnalysisSCADA` / `P190` | ANALOG_FEEDBACK | Non-authoritative field telemetry for SCADA |
| **W121** | `L82_RFImmunitySafetyPLC` / `P127` | `L82_ProtectedRFAmplifier` / `P115` | DIGITAL_COMMAND_OR_STATUS | Protected RF amplifier enable command |
| **W122** | `L82_RFImmunitySafetyPLC` / `P128` | `L82_ProtectedRFAmplifier` / `P116` | DIGITAL_COMMAND_OR_STATUS | Independent RF shutdown override |
| **W123** | `L82_EmergencyRFStopStation` / `P203` | `L82_RFImmunitySafetyPLC` / `P129` | ANALOG_FEEDBACK | Emergency RF stop hardwired status |
| **W124** | `L82_ProtectedRFAmplifier` / `P117` | `L82_RFImmunitySafetyPLC` / `P126` | ANALOG_FEEDBACK | Actual amplifier disabled confirmation |
| **W125** | `L82_TriaxialRFFieldStrengthProbe` / `P121` | `L82_RFImmunitySafetyPLC` / `P127` | ANALOG_FEEDBACK | Field probe calibration diagnostic health |
| **W126** | `L82_RFImmunitySafetyPLC` / `P128` | `L82_EMCTestSequencingPLCRack` / `P191` | ANALOG_FEEDBACK | Measured RF overlimit alarm status |
| **W127** | `L82_RFImmunitySafetyPLC` / `P129` | `L82_RotatingEUTTurntable` / `P004` | DIGITAL_COMMAND_OR_STATUS | Turntable exposure position authorization |
| **W128** | `L82_MotorizedAntennaHeightMast` / `P085` | `L82_RFImmunitySafetyPLC` / `P126` | DIGITAL_COMMAND_OR_STATUS | Antenna mast ready for exposure |
| **W129** | `L82_RealTimeMissionWorkloadGenerator` / `P171` | `L82_RFImmunitySafetyPLC` / `P127` | DIGITAL_COMMAND_OR_STATUS | Mission function validity for RF exposure |
| **W130** | `L82_ChamberDoorInterlockSwitch` / `P131` | `L82_EMCTestSequencingPLCRack` / `P192` | ANALOG_FEEDBACK | Chamber door and seal qualified status |
| **W131** | `L82_RFImmunitySafetyPLC` / `P128` | `L82_EMCTestSequencingPLCRack` / `P193` | DIGITAL_COMMAND_OR_STATUS | Latched critical RF safety fault |
| **W132** | `L82_EMCTestSequencingPLCRack` / `P194` | `L82_RFImmunitySafetyPLC` / `P129` | DIGITAL_COMMAND_OR_STATUS | Operator explicit RF safety reset request |
| **W133** | `L82_EMCTestSequencingPLCRack` / `P191` | `L82_HolographicEMCAnalysisSCADA` / `P188` | ANALOG_FEEDBACK | Authoritative six-objective status telemetry |
| **W134** | `L82_RFImmunitySafetyPLC` / `P126` | `L82_EMCTestSequencingPLCRack` / `P192` | ANALOG_FEEDBACK | Final conductedradiatedimmunity certification status |

**Reconcile conflicting socket reuse/command direction in this CSV before creating SparkTerminal objects. Do not silently claim the visual endpoints are ready-to-use functional ports.**


## ANNEX D — ALL 206 ART SOCKET ANCHORS (METER COORDINATES)

| Anchor | Owning model | World XYZ (m) | Source status |
|---|---|---|---|
| `P001` | `L82_RFSealedChamberDoor` | (-11.935, 3.05, 3.3075) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P002` | `L82_RFSealedChamberDoor` | (-11.765, 3.05, 3.3075) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P003` | `L82_RFSealedChamberDoor` | (-11.935, 3.19, 3.3075) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P004` | `L82_RotatingEUTTurntable` | (-0.085, 0.91875, 1.895) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P005` | `L82_RotatingEUTTurntable` | (0.085, 0.91875, 1.895) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P006` | `L82_RotatingEUTTurntable` | (-0.085, 1.05875, 1.895) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P007` | `L82_TurntableServoGearbox` | (2.8154, 0.9662, -1.2894) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P008` | `L82_TurntableServoGearbox` | (2.9854, 0.9662, -1.2894) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P009` | `L82_TurntableServoGearbox` | (2.8154, 1.1062, -1.2894) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P010` | `L82_FourChipletMissionComputerEUT` | (-0.085, 2.42534, 0.805) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P011` | `L82_FourChipletMissionComputerEUT` | (0.085, 2.42534, 0.805) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P012` | `L82_FourChipletMissionComputerEUT` | (-0.085, 2.56534, 0.805) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P013` | `L82_SpacecraftAvionicsChassis` | (-0.085, 2.5, 0.99) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P014` | `L82_SpacecraftAvionicsChassis` | (0.085, 2.5, 0.99) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P015` | `L82_SpacecraftAvionicsChassis` | (-0.085, 2.64, 0.99) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P016` | `L82_CoPackagedOpticalTransceiver` | (1.55272, 2.7191, -0.15325) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P017` | `L82_CoPackagedOpticalTransceiver` | (1.72272, 2.7191, -0.15325) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P018` | `L82_CoPackagedOpticalTransceiver` | (1.55272, 2.8591, -0.15325) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P019` | `L82_RemovableEMCShieldCanopy` | (-0.085, 3.28, 1.28) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P020` | `L82_RemovableEMCShieldCanopy` | (0.085, 3.28, 1.28) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P021` | `L82_RemovableEMCShieldCanopy` | (-0.085, 3.42, 1.28) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P022` | `L82_AvionicsAccessHatch` | (-1.235, 2.63, 0.945) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P023` | `L82_AvionicsAccessHatch` | (-1.065, 2.63, 0.945) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P024` | `L82_AvionicsAccessHatch` | (-1.235, 2.77, 0.945) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P025` | `L82_ChassisEquipotentialBondingGrid` | (-3.685, 0.8525, -0.035) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P026` | `L82_ChassisEquipotentialBondingGrid` | (-3.515, 0.8525, -0.035) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P027` | `L82_ChassisEquipotentialBondingGrid` | (-3.685, 0.9925, -0.035) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P028` | `L82_ShieldedHarnessBundle` | (-3.785, 1.9725, 2.40317) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P029` | `L82_ShieldedHarnessBundle` | (-3.615, 1.9725, 2.40317) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P030` | `L82_ShieldedHarnessBundle` | (-3.785, 2.1125, 2.40317) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P031` | `L82_HighSpeedDifferentialCable` | (-3.485, 1.9625, 3.02317) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P032` | `L82_HighSpeedDifferentialCable` | (-3.315, 1.9625, 3.02317) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P033` | `L82_HighSpeedDifferentialCable` | (-3.485, 2.1025, 3.02317) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P034` | `L82_FilteredBulkheadFeedthrough` | (-4.685, 1.58175, 3.715) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P035` | `L82_FilteredBulkheadFeedthrough` | (-4.515, 1.58175, 3.715) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P036` | `L82_FilteredBulkheadFeedthrough` | (-4.685, 1.72175, 3.715) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P037` | `L82_DCFeedthroughFilterArray` | (-7.435, 1.66, 5.125) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P038` | `L82_DCFeedthroughFilterArray` | (-7.265, 1.66, 5.125) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P039` | `L82_DCFeedthroughFilterArray` | (-7.435, 1.8, 5.125) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P040` | `L82_CommonModeChokeModule` | (-6.015, 1.4362, 5.045) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P041` | `L82_CommonModeChokeModule` | (-5.845, 1.4362, 5.045) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P042` | `L82_CommonModeChokeModule` | (-6.015, 1.5762, 5.045) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P043` | `L82_DifferentialLCFilterModule` | (-4.835, 1.465, 5.078) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P044` | `L82_DifferentialLCFilterModule` | (-4.665, 1.465, 5.078) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P045` | `L82_DifferentialLCFilterModule` | (-4.835, 1.605, 5.078) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P046` | `L82_ClipOnFerriteCoreRack` | (-3.705, 1.5213, 5.065) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P047` | `L82_ClipOnFerriteCoreRack` | (-3.535, 1.5213, 5.065) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P048` | `L82_CableRoutingTray` | (-2.285, 1.2625, 5.35317) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P049` | `L82_CableRoutingTray` | (-2.115, 1.2625, 5.35317) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P050` | `L82_360DegreeShieldTerminationClamp` | (-4.38318, 2.15175, 4.215) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P051` | `L82_360DegreeShieldTerminationClamp` | (-4.21318, 2.15175, 4.215) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P052` | `L82_360DegreeShieldTerminationClamp` | (-4.38318, 2.29175, 4.215) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P053` | `L82_ShieldGasketCompressionTester` | (-6.635, 3.30375, 3.895) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P054` | `L82_ShieldGasketCompressionTester` | (-6.465, 3.30375, 3.895) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P055` | `L82_EnclosureSeamLeakageProbe` | (-4.285, 3.7158, 1.715) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P056` | `L82_EnclosureSeamLeakageProbe` | (-4.115, 3.7158, 1.715) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P057` | `L82_EnclosureSeamLeakageProbe` | (-4.285, 3.8558, 1.715) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P058` | `L82_NearFieldMagneticHProbe` | (5.165, 3.9358, -0.735) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P059` | `L82_NearFieldMagneticHProbe` | (5.335, 3.9358, -0.735) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P060` | `L82_NearFieldMagneticHProbe` | (5.165, 4.0758, -0.735) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P061` | `L82_NearFieldElectricEProbe` | (5.935, 3.9358, -0.685) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P062` | `L82_NearFieldElectricEProbe` | (6.105, 3.9358, -0.685) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P063` | `L82_NearFieldElectricEProbe` | (5.935, 4.0758, -0.685) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P064` | `L82_NearFieldScannerXYGantry` | (5.595, 3.93, -0.835) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P065` | `L82_NearFieldScannerXYGantry` | (5.765, 3.93, -0.835) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P066` | `L82_NearFieldScannerXYGantry` | (5.595, 4.07, -0.835) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P067` | `L82_NearFieldScannerZLift` | (5.615, 3.3958, -0.985) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P068` | `L82_NearFieldScannerZLift` | (5.785, 3.3958, -0.985) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P069` | `L82_EMIComplianceMeasurementReceiver` | (-9.585, 2.13, 4.576) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P070` | `L82_EMIComplianceMeasurementReceiver` | (-9.415, 2.13, 4.576) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P071` | `L82_EMIComplianceMeasurementReceiver` | (-9.585, 2.27, 4.576) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P072` | `L82_EMIComplianceMeasurementReceiver` | (-9.415, 2.27, 4.576) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P073` | `L82_HighDynamicRangeSpectrumAnalyzer` | (-8.235, 2.15, 4.626) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P074` | `L82_HighDynamicRangeSpectrumAnalyzer` | (-8.065, 2.15, 4.626) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P075` | `L82_HighDynamicRangeSpectrumAnalyzer` | (-8.235, 2.29, 4.626) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P076` | `L82_HighDynamicRangeSpectrumAnalyzer` | (-8.065, 2.29, 4.626) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P077` | `L82_RFPreselectorAndAttenuator` | (-10.535, 1.53, 5.045) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P078` | `L82_RFPreselectorAndAttenuator` | (-10.365, 1.53, 5.045) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P079` | `L82_SpectralCalibrationReferenceSource` | (-9.335, 1.43, 3.808) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P080` | `L82_SpectralCalibrationReferenceSource` | (-9.165, 1.43, 3.808) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P081` | `L82_SpectralCalibrationReferenceSource` | (-9.335, 1.57, 3.808) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P082` | `L82_BroadbandEmissionMeasurementAntenna` | (-8.835, 4.13, -1.19375) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P083` | `L82_BroadbandEmissionMeasurementAntenna` | (-8.665, 4.13, -1.19375) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P084` | `L82_BroadbandEmissionMeasurementAntenna` | (-8.835, 4.27, -1.19375) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P085` | `L82_MotorizedAntennaHeightMast` | (-8.835, 3.9175, -1.515) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P086` | `L82_MotorizedAntennaHeightMast` | (-8.665, 3.9175, -1.515) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P087` | `L82_MotorizedAntennaHeightMast` | (-8.835, 4.0575, -1.515) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P088` | `L82_AntennaPolarizationRotationHead` | (-8.835, 4.26, -1.45568) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P089` | `L82_AntennaPolarizationRotationHead` | (-8.665, 4.26, -1.45568) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P090` | `L82_AntennaPolarizationRotationHead` | (-8.835, 4.4, -1.45568) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P091` | `L82_TurntableAzimuthEncoder` | (-2.335, 1.15, -2.4375) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P092` | `L82_TurntableAzimuthEncoder` | (-2.165, 1.15, -2.4375) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P093` | `L82_TurntableAzimuthEncoder` | (-2.335, 1.29, -2.4375) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P094` | `L82_CommonModeCurrentClamp` | (-6.985, 2.41, 4.025) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P095` | `L82_CommonModeCurrentClamp` | (-6.815, 2.41, 4.025) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P096` | `L82_CommonModeCurrentClamp` | (-6.985, 2.55, 4.025) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P097` | `L82_DifferentialCurrentProbe` | (-5.745, 2.6043, 4.08471) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P098` | `L82_DifferentialCurrentProbe` | (-5.575, 2.6043, 4.08471) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P099` | `L82_DifferentialCurrentProbe` | (-5.745, 2.7443, 4.08471) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P100` | `L82_DCPowerArtificialNetwork` | (-8.135, 1.1805, 2.99) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P101` | `L82_DCPowerArtificialNetwork` | (-7.965, 1.1805, 2.99) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P102` | `L82_DCPowerArtificialNetwork` | (-8.135, 1.3205, 2.99) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P103` | `L82_DCPowerArtificialNetwork` | (-7.965, 1.3205, 2.99) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P104` | `L82_ConductedEmissionMeasurementRack` | (-9.685, 1.55, 3.076) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P105` | `L82_ConductedEmissionMeasurementRack` | (-9.515, 1.55, 3.076) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P106` | `L82_ConductedEmissionMeasurementRack` | (-9.685, 1.69, 3.076) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P107` | `L82_ConductedEmissionMeasurementRack` | (-9.515, 1.69, 3.076) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P108` | `L82_RFImmunityCouplingFixture` | (9.055, 4.13, -1.915) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P109` | `L82_RFImmunityCouplingFixture` | (9.225, 4.13, -1.915) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P110` | `L82_RFImmunityCouplingFixture` | (9.055, 4.27, -1.915) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P111` | `L82_ProgrammableRFSweepGenerator` | (8.865, 2.68, 3.995) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P112` | `L82_ProgrammableRFSweepGenerator` | (9.035, 2.68, 3.995) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P113` | `L82_ProgrammableRFSweepGenerator` | (8.865, 2.82, 3.995) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P114` | `L82_ProgrammableRFSweepGenerator` | (9.035, 2.82, 3.995) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P115` | `L82_ProtectedRFAmplifier` | (10.465, 2.69, 3.826) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P116` | `L82_ProtectedRFAmplifier` | (10.635, 2.69, 3.826) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P117` | `L82_ProtectedRFAmplifier` | (10.465, 2.83, 3.826) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P118` | `L82_ProtectedRFAmplifier` | (10.635, 2.83, 3.826) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P119` | `L82_TriaxialRFFieldStrengthProbe` | (8.015, 4.2543, -0.41529) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P120` | `L82_TriaxialRFFieldStrengthProbe` | (8.185, 4.2543, -0.41529) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P121` | `L82_TriaxialRFFieldStrengthProbe` | (8.015, 4.3943, -0.41529) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P122` | `L82_TriaxialRFFieldStrengthProbe` | (8.185, 4.3943, -0.41529) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P123` | `L82_RFUniformityCalibrationFrame` | (10.565, 3.93, -1.725) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P124` | `L82_RFUniformityCalibrationFrame` | (10.735, 3.93, -1.725) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P125` | `L82_RFUniformityCalibrationFrame` | (10.565, 4.07, -1.725) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P126` | `L82_RFImmunitySafetyPLC` | (11.065, 1.59, 4.858) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P127` | `L82_RFImmunitySafetyPLC` | (11.235, 1.59, 4.858) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P128` | `L82_RFImmunitySafetyPLC` | (11.065, 1.73, 4.858) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P129` | `L82_RFImmunitySafetyPLC` | (11.235, 1.73, 4.858) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P130` | `L82_ChamberDoorInterlockSwitch` | (-11.835, 2.55, 2.695) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P131` | `L82_ChamberDoorInterlockSwitch` | (-11.665, 2.55, 2.695) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P132` | `L82_ChamberDoorInterlockSwitch` | (-11.835, 2.69, 2.695) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P133` | `L82_AnechoicBoundaryConditionMonitor` | (-10.735, 6.25875, -2.635) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P134` | `L82_AnechoicBoundaryConditionMonitor` | (-10.565, 6.25875, -2.635) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P135` | `L82_AnechoicBoundaryConditionMonitor` | (-10.735, 6.39875, -2.635) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P136` | `L82_AvionicsSensorSignalEmulator` | (-1.185, 1.6075, 5.075) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P137` | `L82_AvionicsSensorSignalEmulator` | (-1.015, 1.6075, 5.075) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P138` | `L82_AvionicsSensorSignalEmulator` | (-1.185, 1.7475, 5.075) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P139` | `L82_AvionicsSensorSignalEmulator` | (-1.015, 1.7475, 5.075) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P140` | `L82_AnalogInputSusceptibilityAnalyzer` | (0.265, 1.51, 5.208) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P141` | `L82_AnalogInputSusceptibilityAnalyzer` | (0.435, 1.51, 5.208) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P142` | `L82_AnalogInputSusceptibilityAnalyzer` | (0.265, 1.65, 5.208) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P143` | `L82_AnalogInputSusceptibilityAnalyzer` | (0.435, 1.65, 5.208) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P144` | `L82_OpticalLinkJitterAndControlMonitor` | (1.635, 1.48, 5.178) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P145` | `L82_OpticalLinkJitterAndControlMonitor` | (1.805, 1.48, 5.178) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P146` | `L82_OpticalLinkJitterAndControlMonitor` | (1.635, 1.62, 5.178) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P147` | `L82_OpticalLinkJitterAndControlMonitor` | (1.805, 1.62, 5.178) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P148` | `L82_PrecisionClockEMIProbe` | (3.015, 1.6543, 4.83471) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P149` | `L82_PrecisionClockEMIProbe` | (3.185, 1.6543, 4.83471) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P150` | `L82_PrecisionClockEMIProbe` | (3.015, 1.7943, 4.83471) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P151` | `L82_PrecisionClockEMIProbe` | (3.185, 1.7943, 4.83471) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P152` | `L82_SwitchModePowerConverterModel` | (3.815, 1.96125, 2.495) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P153` | `L82_SwitchModePowerConverterModel` | (3.985, 1.96125, 2.495) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P154` | `L82_SwitchModePowerConverterModel` | (3.815, 2.10125, 2.495) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P155` | `L82_SwitchModePowerConverterModel` | (3.985, 2.10125, 2.495) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P156` | `L82_SpreadSpectrumClockController` | (4.915, 1.48, 5.125) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P157` | `L82_SpreadSpectrumClockController` | (5.085, 1.48, 5.125) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P158` | `L82_SpreadSpectrumClockController` | (4.915, 1.62, 5.125) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P159` | `L82_SpreadSpectrumClockController` | (5.085, 1.62, 5.125) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P160` | `L82_ProtectedSensorInputFilter` | (5.935, 1.43, 5.158) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P161` | `L82_ProtectedSensorInputFilter` | (6.105, 1.43, 5.158) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P162` | `L82_ProtectedSensorInputFilter` | (5.935, 1.57, 5.158) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P163` | `L82_ProtectedSensorInputFilter` | (6.105, 1.57, 5.158) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P164` | `L82_InfraredThermalInspectionCamera` | (4.815, 4.8358, 2.265) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P165` | `L82_InfraredThermalInspectionCamera` | (4.985, 4.8358, 2.265) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P166` | `L82_InfraredThermalInspectionCamera` | (4.815, 4.9758, 2.265) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P167` | `L82_ShieldedCoolingAirDuct` | (3.915, 3.98001, -2.395) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P168` | `L82_ShieldedCoolingAirDuct` | (4.085, 3.98001, -2.395) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P169` | `L82_ShieldedCoolingAirDuct` | (3.915, 4.12001, -2.395) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P170` | `L82_RealTimeMissionWorkloadGenerator` | (-0.085, 6.38, -4.192) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P171` | `L82_RealTimeMissionWorkloadGenerator` | (0.085, 6.38, -4.192) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P172` | `L82_RealTimeMissionWorkloadGenerator` | (-0.085, 6.52, -4.192) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P173` | `L82_FaultDiagnosisAndRepairRouter` | (-0.085, 1.2, 6.108) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P174` | `L82_FaultDiagnosisAndRepairRouter` | (0.085, 1.2, 6.108) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P175` | `L82_FaultDiagnosisAndRepairRouter` | (-0.085, 1.34, 6.108) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P176` | `L82_CableInjectionFaultFixture` | (6.27932, 1.56222, 5.0575) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P177` | `L82_CableInjectionFaultFixture` | (6.44932, 1.56222, 5.0575) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P178` | `L82_CableInjectionFaultFixture` | (6.27932, 1.70222, 5.0575) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P179` | `L82_GroundReturnFaultSwitch` | (-3.37522, 1.51222, 6.0575) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P180` | `L82_GroundReturnFaultSwitch` | (-3.20522, 1.51222, 6.0575) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P181` | `L82_GroundReturnFaultSwitch` | (-3.37522, 1.65222, 6.0575) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P182` | `L82_SeamGapFaultActuator` | (-1.42522, 3.08222, 1.8575) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P183` | `L82_SeamGapFaultActuator` | (-1.25522, 3.08222, 1.8575) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P184` | `L82_SeamGapFaultActuator` | (-1.42522, 3.22222, 1.8575) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P185` | `L82_RFDesenseFaultEmulator` | (2.665, 2.03, 5.408) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P186` | `L82_RFDesenseFaultEmulator` | (2.835, 2.03, 5.408) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P187` | `L82_RFDesenseFaultEmulator` | (2.665, 2.17, 5.408) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P188` | `L82_HolographicEMCAnalysisSCADA` | (-0.085, 7.23, -4.584) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P189` | `L82_HolographicEMCAnalysisSCADA` | (0.085, 7.23, -4.584) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P190` | `L82_HolographicEMCAnalysisSCADA` | (-0.085, 7.37, -4.584) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P191` | `L82_EMCTestSequencingPLCRack` | (8.915, 1.46, 5.938) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P192` | `L82_EMCTestSequencingPLCRack` | (9.085, 1.46, 5.938) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P193` | `L82_EMCTestSequencingPLCRack` | (8.915, 1.6, 5.938) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P194` | `L82_EMCTestSequencingPLCRack` | (9.085, 1.6, 5.938) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P195` | `L82_Regulated5VControlSupply` | (-8.535, 1.2, 5.858) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P196` | `L82_Regulated5VControlSupply` | (-8.365, 1.2, 5.858) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P197` | `L82_Regulated5VControlSupply` | (-8.535, 1.34, 5.858) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P198` | `L82_Regulated5VControlSupply` | (-8.365, 1.34, 5.858) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P199` | `L82_RFSafetyPowerCabinet` | (10.515, 1.48, 5.758) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P200` | `L82_RFSafetyPowerCabinet` | (10.685, 1.48, 5.758) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P201` | `L82_RFSafetyPowerCabinet` | (10.515, 1.62, 5.758) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P202` | `L82_RFSafetyPowerCabinet` | (10.685, 1.62, 5.758) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P203` | `L82_EmergencyRFStopStation` | (11.395, 1.55165, 6.35) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P204` | `L82_EmergencyRFStopStation` | (11.565, 1.55165, 6.35) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P205` | `L82_EmergencyRFStopStation` | (11.395, 1.69165, 6.35) | `ART_ANCHOR_NOT_SPARKTERMINAL` |
| `P206` | `L82_EmergencyRFStopStation` | (11.565, 1.69165, 6.35) | `ART_ANCHOR_NOT_SPARKTERMINAL` |

## ANNEX E — EXACT SIX SOURCE OBJECTIVE DEFINITIONS

| # | Title | Source evidence requirement | Status in delivered pack |
|---:|---|---|---|
| 1 | **Build the EMC Qualification Network** | 134 player-created electrical wires connected in real Spark graph; 206 SparkTerminals registered; 5V power, EMC instruments and independent RF safety healthy | `DESIGN_ONLY_NOT_IMPLEMENTED` |
| 2 | **Calibrate the Blackstar Chamber** | Chamber door interlock; turntable; antenna mast and polarization; EMI receiver; near-field probes; conducted instruments and trusted field probe calibrated | `DESIGN_ONLY_NOT_IMPLEMENTED` |
| 3 | **Silence Conducted and Radiated Emissions** | Source/path diagnosis and physical repairs; complete required rotational antenna measurements; conducted and radiated margins pass without breaking mission functionality | `DESIGN_ONLY_NOT_IMPLEMENTED` |
| 4 | **Prove RF Immunity** | W119 trusted field feedback and independent interlocks verified; safe fictional exposure sweeps pass with valid sensors clocks telemetry and zero unsafe commands | `DESIGN_ONLY_NOT_IMPLEMENTED` |
| 5 | **Survive Three EMC Failures** | Common-mode cable fault; shielding seam leakage; RF-induced sensor susceptibility all diagnosed and repaired; fresh complete EMC and mission-function qualification passes | `DESIGN_ONLY_NOT_IMPLEMENTED` |
| 6 | **Restore Critical RF Field Feedback** | Player unplugs real W119, PLC sees invalid stale field measurements and inhibits RF, fault latches; reconnect and recalibrate then explicitly reset and run fresh comprehensive emissionsimmunity certification | `DESIGN_ONLY_NOT_IMPLEMENTED` |

## ANNEX F — VERIFIED ART/PLANNING STATUS AND UNITY LIMITATIONS

- `individual_fbx_verified`: [{'file': '00_L82_BlackstarEMCChamberBase.fbx', 'triangles': 588, 'mesh_parts': 49, 'bytes': 72642, 'named_parent_pivots': 0}, {'file': '01_L82_SemiAnechoicShieldedHall.fbx', 'triangles': 600, 'mesh_parts': 50, 'bytes': 72991, 'named_parent_pivots': 0}, {'file': '02_L82_RFAbsorberPyramidWallSet.fbx', 'triangles': 6276, 'mesh_parts': 1039, 'bytes': 1413437, 'named_parent_pivots': 1}, {'file': '03_L82_RFSealedChamberDoor.fbx', 'triangles': 1392, 'mesh_parts': 46, 'bytes': 75112, 'named_parent_pivots': 1}, {'file': '04_L82_RotatingEUTTurntable.fbx', 'triangles': 4096, 'mesh_parts': 86, 'bytes': 150589, 'named_parent_pivots': 1}, {'file': '05_L82_TurntableServoGearbox.fbx', 'triangles': 2980, 'mesh_parts': 28, 'bytes': 62985, 'named_parent_pivots': 1}, {'file': '06_L82_FourChipletMissionComputerEUT.fbx', 'triangles': 2104, 'mesh_parts': 90, 'bytes': 142794, 'named_parent_pivots': 1}, {'file': '07_L82_SpacecraftAvionicsChassis.fbx', 'triangles': 1416, 'mesh_parts': 52, 'bytes': 83794, 'named_parent_pivots': 1}, {'file': '08_L82_CoPackagedOpticalTransceiver.fbx', 'triangles': 1924, 'mesh_parts': 83, 'bytes': 130935, 'named_parent_pivots': 0}, {'file': '09_L82_RemovableEMCShieldCanopy.fbx', 'triangles': 1032, 'mesh_parts': 39, 'bytes': 64194, 'named_parent_pivots': 1}, {'file': '10_L82_ConductiveSeamGasketSet.fbx', 'triangles': 896, 'mesh_parts': 6, 'bytes': 16833, 'named_parent_pivots': 1}, {'file': '11_L82_AvionicsAccessHatch.fbx', 'triangles': 1256, 'mesh_parts': 17, 'bytes': 35327, 'named_parent_pivots': 1}, {'file': '12_L82_ReferenceGroundPlane.fbx', 'triangles': 288, 'mesh_parts': 24, 'bytes': 37485, 'named_parent_pivots': 1}, {'file': '13_L82_ChassisEquipotentialBondingGrid.fbx', 'triangles': 2212, 'mesh_parts': 44, 'bytes': 87227, 'named_parent_pivots': 1}, {'file': '14_L82_ShieldedHarnessBundle.fbx', 'triangles': 3756, 'mesh_parts': 70, 'bytes': 130820, 'named_parent_pivots': 0}, {'file': '15_L82_HighSpeedDifferentialCable.fbx', 'triangles': 3228, 'mesh_parts': 61, 'bytes': 117104, 'named_parent_pivots': 1}, {'file': '16_L82_FilteredBulkheadFeedthrough.fbx', 'triangles': 1660, 'mesh_parts': 38, 'bytes': 66750, 'named_parent_pivots': 1}, {'file': '17_L82_DCFeedthroughFilterArray.fbx', 'triangles': 13688, 'mesh_parts': 747, 'bytes': 1071242, 'named_parent_pivots': 1}, {'file': '18_L82_CommonModeChokeModule.fbx', 'triangles': 2540, 'mesh_parts': 94, 'bytes': 145235, 'named_parent_pivots': 1}, {'file': '19_L82_DifferentialLCFilterModule.fbx', 'triangles': 7416, 'mesh_parts': 403, 'bytes': 585058, 'named_parent_pivots': 1}, {'file': '20_L82_ClipOnFerriteCoreRack.fbx', 'triangles': 2100, 'mesh_parts': 42, 'bytes': 73438, 'named_parent_pivots': 1}, {'file': '21_L82_CableRoutingTray.fbx', 'triangles': 3772, 'mesh_parts': 73, 'bytes': 137903, 'named_parent_pivots': 1}, {'file': '22_L82_360DegreeShieldTerminationClamp.fbx', 'triangles': 2596, 'mesh_parts': 53, 'bytes': 97979, 'named_parent_pivots': 1}, {'file': '23_L82_ShieldGasketCompressionTester.fbx', 'triangles': 984, 'mesh_parts': 18, 'bytes': 36955, 'named_parent_pivots': 1}, {'file': '24_L82_EnclosureSeamLeakageProbe.fbx', 'triangles': 3856, 'mesh_parts': 38, 'bytes': 83878, 'named_parent_pivots': 1}, {'file': '25_L82_NearFieldMagneticHProbe.fbx', 'triangles': 3856, 'mesh_parts': 38, 'bytes': 83492, 'named_parent_pivots': 1}, {'file': '26_L82_NearFieldElectricEProbe.fbx', 'triangles': 3856, 'mesh_parts': 38, 'bytes': 83586, 'named_parent_pivots': 1}, {'file': '27_L82_NearFieldScannerXYGantry.fbx', 'triangles': 1072, 'mesh_parts': 29, 'bytes': 51593, 'named_parent_pivots': 2}, {'file': '28_L82_NearFieldScannerZLift.fbx', 'triangles': 3632, 'mesh_parts': 35, 'bytes': 78019, 'named_parent_pivots': 1}, {'file': '29_L82_EMIComplianceMeasurementReceiver.fbx', 'triangles': 2908, 'mesh_parts': 93, 'bytes': 147356, 'named_parent_pivots': 0}, {'file': '30_L82_HighDynamicRangeSpectrumAnalyzer.fbx', 'triangles': 7008, 'mesh_parts': 173, 'bytes': 293519, 'named_parent_pivots': 1}, {'file': '31_L82_RFPreselectorAndAttenuator.fbx', 'triangles': 2448, 'mesh_parts': 77, 'bytes': 122636, 'named_parent_pivots': 1}, {'file': '32_L82_SpectralCalibrationReferenceSource.fbx', 'triangles': 2540, 'mesh_parts': 78, 'bytes': 125394, 'named_parent_pivots': 0}, {'file': '33_L82_BroadbandEmissionMeasurementAntenna.fbx', 'triangles': 8880, 'mesh_parts': 88, 'bytes': 177995, 'named_parent_pivots': 1}, {'file': '34_L82_MotorizedAntennaHeightMast.fbx', 'triangles': 1092, 'mesh_parts': 44, 'bytes': 70606, 'named_parent_pivots': 1}, {'file': '35_L82_AntennaPolarizationRotationHead.fbx', 'triangles': 8496, 'mesh_parts': 86, 'bytes': 172466, 'named_parent_pivots': 1}, {'file': '36_L82_TurntableAzimuthEncoder.fbx', 'triangles': 1452, 'mesh_parts': 24, 'bytes': 45626, 'named_parent_pivots': 1}, {'file': '37_L82_ThreeDimensionalRadiationPatternHologram.fbx', 'triangles': 40536, 'mesh_parts': 758, 'bytes': 1435446, 'named_parent_pivots': 1}, {'file': '38_L82_EMIFieldVectorHeatmapDisplay.fbx', 'triangles': 1860, 'mesh_parts': 155, 'bytes': 220358, 'named_parent_pivots': 0}, {'file': '39_L82_CommonModeCurrentClamp.fbx', 'triangles': 1192, 'mesh_parts': 13, 'bytes': 30541, 'named_parent_pivots': 1}, {'file': '40_L82_DifferentialCurrentProbe.fbx', 'triangles': 3812, 'mesh_parts': 41, 'bytes': 86339, 'named_parent_pivots': 1}, {'file': '41_L82_DCPowerArtificialNetwork.fbx', 'triangles': 1148, 'mesh_parts': 33, 'bytes': 55381, 'named_parent_pivots': 0}, {'file': '42_L82_ConductedEmissionMeasurementRack.fbx', 'triangles': 3632, 'mesh_parts': 110, 'bytes': 174160, 'named_parent_pivots': 0}, {'file': '43_L82_RFImmunityCouplingFixture.fbx', 'triangles': 1256, 'mesh_parts': 39, 'bytes': 65491, 'named_parent_pivots': 1}, {'file': '44_L82_ProgrammableRFSweepGenerator.fbx', 'triangles': 3040, 'mesh_parts': 95, 'bytes': 150754, 'named_parent_pivots': 1}, {'file': '45_L82_ProtectedRFAmplifier.fbx', 'triangles': 3728, 'mesh_parts': 118, 'bytes': 183932, 'named_parent_pivots': 1}, {'file': '46_L82_TriaxialRFFieldStrengthProbe.fbx', 'triangles': 4156, 'mesh_parts': 47, 'bytes': 98100, 'named_parent_pivots': 1}, {'file': '47_L82_RFUniformityCalibrationFrame.fbx', 'triangles': 1784, 'mesh_parts': 41, 'bytes': 70734, 'named_parent_pivots': 1}, {'file': '48_L82_RFImmunitySafetyPLC.fbx', 'triangles': 3488, 'mesh_parts': 98, 'bytes': 155266, 'named_parent_pivots': 0}, {'file': '49_L82_ChamberDoorInterlockSwitch.fbx', 'triangles': 824, 'mesh_parts': 13, 'bytes': 27664, 'named_parent_pivots': 1}, {'file': '50_L82_AnechoicBoundaryConditionMonitor.fbx', 'triangles': 900, 'mesh_parts': 28, 'bytes': 47687, 'named_parent_pivots': 0}, {'file': '51_L82_AvionicsSensorSignalEmulator.fbx', 'triangles': 1312, 'mesh_parts': 25, 'bytes': 46185, 'named_parent_pivots': 1}, {'file': '52_L82_AnalogInputSusceptibilityAnalyzer.fbx', 'triangles': 2764, 'mesh_parts': 81, 'bytes': 130725, 'named_parent_pivots': 0}, {'file': '53_L82_OpticalLinkJitterAndControlMonitor.fbx', 'triangles': 2848, 'mesh_parts': 88, 'bytes': 141274, 'named_parent_pivots': 1}, {'file': '54_L82_PrecisionClockEMIProbe.fbx', 'triangles': 4048, 'mesh_parts': 45, 'bytes': 92784, 'named_parent_pivots': 1}, {'file': '55_L82_SwitchModePowerConverterModel.fbx', 'triangles': 3004, 'mesh_parts': 117, 'bytes': 181104, 'named_parent_pivots': 1}, {'file': '56_L82_SpreadSpectrumClockController.fbx', 'triangles': 2896, 'mesh_parts': 83, 'bytes': 133796, 'named_parent_pivots': 1}, {'file': '57_L82_ProtectedSensorInputFilter.fbx', 'triangles': 8300, 'mesh_parts': 379, 'bytes': 559512, 'named_parent_pivots': 1}, {'file': '58_L82_InfraredThermalInspectionCamera.fbx', 'triangles': 3856, 'mesh_parts': 38, 'bytes': 84207, 'named_parent_pivots': 1}, {'file': '59_L82_ShieldedCoolingAirDuct.fbx', 'triangles': 2872, 'mesh_parts': 68, 'bytes': 114705, 'named_parent_pivots': 1}, {'file': '60_L82_RealTimeMissionWorkloadGenerator.fbx', 'triangles': 3264, 'mesh_parts': 95, 'bytes': 152249, 'named_parent_pivots': 0}, {'file': '61_L82_FaultDiagnosisAndRepairRouter.fbx', 'triangles': 2540, 'mesh_parts': 78, 'bytes': 124834, 'named_parent_pivots': 0}, {'file': '62_L82_CableInjectionFaultFixture.fbx', 'triangles': 2356, 'mesh_parts': 44, 'bytes': 85847, 'named_parent_pivots': 1}, {'file': '63_L82_GroundReturnFaultSwitch.fbx', 'triangles': 1348, 'mesh_parts': 23, 'bytes': 46387, 'named_parent_pivots': 0}, {'file': '64_L82_SeamGapFaultActuator.fbx', 'triangles': 1420, 'mesh_parts': 29, 'bytes': 55952, 'named_parent_pivots': 1}, {'file': '65_L82_RFDesenseFaultEmulator.fbx', 'triangles': 2540, 'mesh_parts': 78, 'bytes': 123970, 'named_parent_pivots': 0}, {'file': '66_L82_HolographicEMCAnalysisSCADA.fbx', 'triangles': 1320, 'mesh_parts': 63, 'bytes': 97396, 'named_parent_pivots': 1}, {'file': '67_L82_EMCTestSequencingPLCRack.fbx', 'triangles': 2764, 'mesh_parts': 81, 'bytes': 128991, 'named_parent_pivots': 0}, {'file': '68_L82_Regulated5VControlSupply.fbx', 'triangles': 2764, 'mesh_parts': 81, 'bytes': 129139, 'named_parent_pivots': 0}, {'file': '69_L82_RFSafetyPowerCabinet.fbx', 'triangles': 3488, 'mesh_parts': 98, 'bytes': 155276, 'named_parent_pivots': 0}, {'file': '70_L82_EMCCertificationHologram.fbx', 'triangles': 3688, 'mesh_parts': 66, 'bytes': 128262, 'named_parent_pivots': 1}, {'file': '71_L82_EmergencyRFStopStation.fbx', 'triangles': 1404, 'mesh_parts': 27, 'bytes': 50843, 'named_parent_pivots': 1}].
- `individual_glb_verified`: [{'file': '00_L82_BlackstarEMCChamberBase.glb', 'triangles': 588, 'mesh_parts': 49, 'bytes': 30500, 'named_parent_pivots': 0}, {'file': '01_L82_SemiAnechoicShieldedHall.glb', 'triangles': 600, 'mesh_parts': 50, 'bytes': 30532, 'named_parent_pivots': 0}, {'file': '02_L82_RFAbsorberPyramidWallSet.glb', 'triangles': 6276, 'mesh_parts': 1039, 'bytes': 529196, 'named_parent_pivots': 1}, {'file': '03_L82_RFSealedChamberDoor.glb', 'triangles': 1392, 'mesh_parts': 46, 'bytes': 41032, 'named_parent_pivots': 1}, {'file': '04_L82_RotatingEUTTurntable.glb', 'triangles': 4096, 'mesh_parts': 86, 'bytes': 89420, 'named_parent_pivots': 1}, {'file': '05_L82_TurntableServoGearbox.glb', 'triangles': 2980, 'mesh_parts': 28, 'bytes': 45916, 'named_parent_pivots': 1}, {'file': '06_L82_FourChipletMissionComputerEUT.glb', 'triangles': 2104, 'mesh_parts': 90, 'bytes': 69788, 'named_parent_pivots': 1}, {'file': '07_L82_SpacecraftAvionicsChassis.glb', 'triangles': 1416, 'mesh_parts': 52, 'bytes': 44384, 'named_parent_pivots': 1}, {'file': '08_L82_CoPackagedOpticalTransceiver.glb', 'triangles': 1924, 'mesh_parts': 83, 'bytes': 65100, 'named_parent_pivots': 0}, {'file': '09_L82_RemovableEMCShieldCanopy.glb', 'triangles': 1032, 'mesh_parts': 39, 'bytes': 31492, 'named_parent_pivots': 1}, {'file': '10_L82_ConductiveSeamGasketSet.glb', 'triangles': 896, 'mesh_parts': 6, 'bytes': 20628, 'named_parent_pivots': 1}, {'file': '11_L82_AvionicsAccessHatch.glb', 'triangles': 1256, 'mesh_parts': 17, 'bytes': 25904, 'named_parent_pivots': 1}, {'file': '12_L82_ReferenceGroundPlane.glb', 'triangles': 288, 'mesh_parts': 24, 'bytes': 15332, 'named_parent_pivots': 1}, {'file': '13_L82_ChassisEquipotentialBondingGrid.glb', 'triangles': 2212, 'mesh_parts': 44, 'bytes': 46776, 'named_parent_pivots': 1}, {'file': '14_L82_ShieldedHarnessBundle.glb', 'triangles': 3756, 'mesh_parts': 70, 'bytes': 53068, 'named_parent_pivots': 0}, {'file': '15_L82_HighSpeedDifferentialCable.glb', 'triangles': 3228, 'mesh_parts': 61, 'bytes': 60980, 'named_parent_pivots': 1}, {'file': '16_L82_FilteredBulkheadFeedthrough.glb', 'triangles': 1660, 'mesh_parts': 38, 'bytes': 37548, 'named_parent_pivots': 1}, {'file': '17_L82_DCFeedthroughFilterArray.glb', 'triangles': 13688, 'mesh_parts': 747, 'bytes': 489244, 'named_parent_pivots': 1}, {'file': '18_L82_CommonModeChokeModule.glb', 'triangles': 2540, 'mesh_parts': 94, 'bytes': 71576, 'named_parent_pivots': 1}, {'file': '19_L82_DifferentialLCFilterModule.glb', 'triangles': 7416, 'mesh_parts': 403, 'bytes': 265808, 'named_parent_pivots': 1}, {'file': '20_L82_ClipOnFerriteCoreRack.glb', 'triangles': 2100, 'mesh_parts': 42, 'bytes': 43448, 'named_parent_pivots': 1}, {'file': '21_L82_CableRoutingTray.glb', 'triangles': 3772, 'mesh_parts': 73, 'bytes': 61864, 'named_parent_pivots': 1}, {'file': '22_L82_360DegreeShieldTerminationClamp.glb', 'triangles': 2596, 'mesh_parts': 53, 'bytes': 55404, 'named_parent_pivots': 1}, {'file': '23_L82_ShieldGasketCompressionTester.glb', 'triangles': 984, 'mesh_parts': 18, 'bytes': 27308, 'named_parent_pivots': 1}, {'file': '24_L82_EnclosureSeamLeakageProbe.glb', 'triangles': 3856, 'mesh_parts': 38, 'bytes': 65588, 'named_parent_pivots': 1}, {'file': '25_L82_NearFieldMagneticHProbe.glb', 'triangles': 3856, 'mesh_parts': 38, 'bytes': 65436, 'named_parent_pivots': 1}, {'file': '26_L82_NearFieldElectricEProbe.glb', 'triangles': 3856, 'mesh_parts': 38, 'bytes': 65408, 'named_parent_pivots': 1}, {'file': '27_L82_NearFieldScannerXYGantry.glb', 'triangles': 1072, 'mesh_parts': 29, 'bytes': 27048, 'named_parent_pivots': 2}, {'file': '28_L82_NearFieldScannerZLift.glb', 'triangles': 3632, 'mesh_parts': 35, 'bytes': 62428, 'named_parent_pivots': 1}, {'file': '29_L82_EMIComplianceMeasurementReceiver.glb', 'triangles': 2908, 'mesh_parts': 93, 'bytes': 74840, 'named_parent_pivots': 0}, {'file': '30_L82_HighDynamicRangeSpectrumAnalyzer.glb', 'triangles': 7008, 'mesh_parts': 173, 'bytes': 146652, 'named_parent_pivots': 1}, {'file': '31_L82_RFPreselectorAndAttenuator.glb', 'triangles': 2448, 'mesh_parts': 77, 'bytes': 64464, 'named_parent_pivots': 1}, {'file': '32_L82_SpectralCalibrationReferenceSource.glb', 'triangles': 2540, 'mesh_parts': 78, 'bytes': 64544, 'named_parent_pivots': 0}, {'file': '33_L82_BroadbandEmissionMeasurementAntenna.glb', 'triangles': 8880, 'mesh_parts': 88, 'bytes': 115092, 'named_parent_pivots': 1}, {'file': '34_L82_MotorizedAntennaHeightMast.glb', 'triangles': 1092, 'mesh_parts': 44, 'bytes': 34584, 'named_parent_pivots': 1}, {'file': '35_L82_AntennaPolarizationRotationHead.glb', 'triangles': 8496, 'mesh_parts': 86, 'bytes': 113132, 'named_parent_pivots': 1}, {'file': '36_L82_TurntableAzimuthEncoder.glb', 'triangles': 1452, 'mesh_parts': 24, 'bytes': 35140, 'named_parent_pivots': 1}, {'file': '37_L82_ThreeDimensionalRadiationPatternHologram.glb', 'triangles': 40536, 'mesh_parts': 758, 'bytes': 679596, 'named_parent_pivots': 1}, {'file': '38_L82_EMIFieldVectorHeatmapDisplay.glb', 'triangles': 1860, 'mesh_parts': 155, 'bytes': 93868, 'named_parent_pivots': 0}, {'file': '39_L82_CommonModeCurrentClamp.glb', 'triangles': 1192, 'mesh_parts': 13, 'bytes': 26052, 'named_parent_pivots': 1}, {'file': '40_L82_DifferentialCurrentProbe.glb', 'triangles': 3812, 'mesh_parts': 41, 'bytes': 65136, 'named_parent_pivots': 1}, {'file': '41_L82_DCPowerArtificialNetwork.glb', 'triangles': 1148, 'mesh_parts': 33, 'bytes': 28696, 'named_parent_pivots': 0}, {'file': '42_L82_ConductedEmissionMeasurementRack.glb', 'triangles': 3632, 'mesh_parts': 110, 'bytes': 88704, 'named_parent_pivots': 0}, {'file': '43_L82_RFImmunityCouplingFixture.glb', 'triangles': 1256, 'mesh_parts': 39, 'bytes': 33796, 'named_parent_pivots': 1}, {'file': '44_L82_ProgrammableRFSweepGenerator.glb', 'triangles': 3040, 'mesh_parts': 95, 'bytes': 78164, 'named_parent_pivots': 1}, {'file': '45_L82_ProtectedRFAmplifier.glb', 'triangles': 3728, 'mesh_parts': 118, 'bytes': 91244, 'named_parent_pivots': 1}, {'file': '46_L82_TriaxialRFFieldStrengthProbe.glb', 'triangles': 4156, 'mesh_parts': 47, 'bytes': 71604, 'named_parent_pivots': 1}, {'file': '47_L82_RFUniformityCalibrationFrame.glb', 'triangles': 1784, 'mesh_parts': 41, 'bytes': 38632, 'named_parent_pivots': 1}, {'file': '48_L82_RFImmunitySafetyPLC.glb', 'triangles': 3488, 'mesh_parts': 98, 'bytes': 78452, 'named_parent_pivots': 0}, {'file': '49_L82_ChamberDoorInterlockSwitch.glb', 'triangles': 824, 'mesh_parts': 13, 'bytes': 17768, 'named_parent_pivots': 1}, {'file': '50_L82_AnechoicBoundaryConditionMonitor.glb', 'triangles': 900, 'mesh_parts': 28, 'bytes': 24928, 'named_parent_pivots': 0}, {'file': '51_L82_AvionicsSensorSignalEmulator.glb', 'triangles': 1312, 'mesh_parts': 25, 'bytes': 27140, 'named_parent_pivots': 1}, {'file': '52_L82_AnalogInputSusceptibilityAnalyzer.glb', 'triangles': 2764, 'mesh_parts': 81, 'bytes': 67276, 'named_parent_pivots': 0}, {'file': '53_L82_OpticalLinkJitterAndControlMonitor.glb', 'triangles': 2848, 'mesh_parts': 88, 'bytes': 71824, 'named_parent_pivots': 1}, {'file': '54_L82_PrecisionClockEMIProbe.glb', 'triangles': 4048, 'mesh_parts': 45, 'bytes': 68620, 'named_parent_pivots': 1}, {'file': '55_L82_SwitchModePowerConverterModel.glb', 'triangles': 3004, 'mesh_parts': 117, 'bytes': 88360, 'named_parent_pivots': 1}, {'file': '56_L82_SpreadSpectrumClockController.glb', 'triangles': 2896, 'mesh_parts': 83, 'bytes': 70408, 'named_parent_pivots': 1}, {'file': '57_L82_ProtectedSensorInputFilter.glb', 'triangles': 8300, 'mesh_parts': 379, 'bytes': 261292, 'named_parent_pivots': 1}, {'file': '58_L82_InfraredThermalInspectionCamera.glb', 'triangles': 3856, 'mesh_parts': 38, 'bytes': 65936, 'named_parent_pivots': 1}, {'file': '59_L82_ShieldedCoolingAirDuct.glb', 'triangles': 2872, 'mesh_parts': 68, 'bytes': 67292, 'named_parent_pivots': 1}, {'file': '60_L82_RealTimeMissionWorkloadGenerator.glb', 'triangles': 3264, 'mesh_parts': 95, 'bytes': 78180, 'named_parent_pivots': 0}, {'file': '61_L82_FaultDiagnosisAndRepairRouter.glb', 'triangles': 2540, 'mesh_parts': 78, 'bytes': 63916, 'named_parent_pivots': 0}, {'file': '62_L82_CableInjectionFaultFixture.glb', 'triangles': 2356, 'mesh_parts': 44, 'bytes': 48520, 'named_parent_pivots': 1}, {'file': '63_L82_GroundReturnFaultSwitch.glb', 'triangles': 1348, 'mesh_parts': 23, 'bytes': 27148, 'named_parent_pivots': 0}, {'file': '64_L82_SeamGapFaultActuator.glb', 'triangles': 1420, 'mesh_parts': 29, 'bytes': 31044, 'named_parent_pivots': 1}, {'file': '65_L82_RFDesenseFaultEmulator.glb', 'triangles': 2540, 'mesh_parts': 78, 'bytes': 62756, 'named_parent_pivots': 0}, {'file': '66_L82_HolographicEMCAnalysisSCADA.glb', 'triangles': 1320, 'mesh_parts': 63, 'bytes': 46316, 'named_parent_pivots': 1}, {'file': '67_L82_EMCTestSequencingPLCRack.glb', 'triangles': 2764, 'mesh_parts': 81, 'bytes': 65652, 'named_parent_pivots': 0}, {'file': '68_L82_Regulated5VControlSupply.glb', 'triangles': 2764, 'mesh_parts': 81, 'bytes': 65748, 'named_parent_pivots': 0}, {'file': '69_L82_RFSafetyPowerCabinet.glb', 'triangles': 3488, 'mesh_parts': 98, 'bytes': 78620, 'named_parent_pivots': 0}, {'file': '70_L82_EMCCertificationHologram.glb', 'triangles': 3688, 'mesh_parts': 66, 'bytes': 65404, 'named_parent_pivots': 1}, {'file': '71_L82_EmergencyRFStopStation.glb', 'triangles': 1404, 'mesh_parts': 27, 'bytes': 32900, 'named_parent_pivots': 1}].
- `assembled_fbx_verified`: True.
- `assembled_glb_verified`: True.
- `total_equipment_triangles`: 252048.
- `total_equipment_mesh_parts`: 7369.
- `assembled_workstation_width_m`: 25.424999999999997.
- `unity_compilation_tested`: False.
- `unity_play_mode_tested`: False.
- `gameplay_functionality_included`: False.

**Original files reviewed:** `README_Unity.txt`, `Gameplay_Integration_Plan.md`, `W119_Critical_Feedback_Recovery.md`, `Model_Manifest.json`, `Verification_Report.json`, `Animation_Rigs.csv`, `Gameplay_Wiring_Budget.csv`, `Gameplay_Wiring_Concept_W001_W134.csv`, `Terminal_Anchor_Plan_206.csv`, `Objective_Certification_Plan_6.csv`, and the four packaged Unity C# art helpers.

