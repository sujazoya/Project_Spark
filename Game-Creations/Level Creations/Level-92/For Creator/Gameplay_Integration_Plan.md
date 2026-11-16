# Level 92 — gameplay engineering integration plan (not implemented)

**Goal:** use the actual Project Spark Unity APIs, not invented signatures. Preserve Levels 1–91.

## Three separate authorities
1. **Electrical graph**: player-created wires, educational 5 V control power, returns, compatible functional `SparkTerminal` directions and real disconnect consequences.
2. **Mechanical/process plant**: geophysical survey, six-leg contact physics, microspine actual grip, anchor load redistribution, bit feed/torque/temperature/rock resistance, sample capture/containment and finite tether length.
3. **Independent safety PLC**: critical W171 trusted force path, qualified force margins, interlock inputs, latched faults, confirmed actuator disable, manual reset. Safety must not rely on SCADA visuals.

## Electrical plan
- Source `Gameplay_Wiring_Concept_W001_W188.csv` has **188 conceptual, NOT player-wired edges**. Reconcile **266 decorative P-socket nodes** and **376 independent logical endpoint occurrences**.
- `Terminal_Anchor_Signal_Conflict_Report.csv` lists reused socket locations; never electrically short differing `SignalKind` fields through one decorative anchor. Map to functional multichannel terminal blocks or allocate actual separate subcontacts with recorded validation.
- Keep simulated protected drill power separate from 5 V interface and independent safety channels. Do not attach high-power process values to Spark 5 V control nodes.
- Required critical route: `L92_AnchorLoadCellArray` / `P072` `TRUSTED_ANCHOR_LOAD_OUT` → `L92_SamplingSafetyPLC` / `P219` `TRUSTED_ANCHOR_LOAD_IN`.

## Single authoritative geophysics, landing and coring plant
- Subsurface layered properties: regolith, fractured interface, bedrock inclusion, inferred radar response and uncertainty; deterministic scenario seeds.
- Geophysical instrument measured data must respect calibration, pose, scan coverage, noise, validity and timestamp.
- Lander rigid-body position, orientation, momentum, contact locations and finite tether deployment; no scripted docking/teleporting.
- Per-leg encoder, contact normal, shear, microspine grip and spring behavior; actual operational load boundaries from geometry and measured forces.
- Drilling bit rpm, axial feed, feed acceleration, motor torque/current, bit wear, thermal response, actual depth, debris mass and core integrity.
- Sample capture, tube seal state, material continuity, mass conservation, contamination proxy, cartridge leak measurement and retrieval.
- Mutations from all three faults must affect plant variables, be observable through calibrated instruments, and require physical simulated repair.

## Objective evidence
1. Real 188-wire circuit, valid supply, return and independent safety.
2. Calibrated geophysical scans, adequate coverage and qualified site selection with uncertainty.
3. Deployed lander, six-leg contact, secure anchor and successfully bounded test load.
4. Actual drilled valid core, documented sample depth, cartridge seal and mass confirmation.
5. Real anchor-slip, drill-stall and cartridge-seal disturbances diagnosed, repaired and followed by a fresh complete sampling run.
6. Actual W171 removed from electrical graph -> trusted samples stale -> drilling safely inhibited -> latched fault -> physical reconnect -> fresh calibration -> explicit reset -> new complete core recovery and containment qualification. Historical 1–5 remain certified.

## Exact integration guidance
Inspect real `SparkElectricalSolver`, `SparkCircuitSystem`, `SparkElectricalComponent`, `SparkTerminal`, `SparkLevelDefinition`, `SparkLevelEvaluator`, `SparkLevelSceneBindings`, `SparkObjectShelf`, `SparkLevelLayoutLoader` and `LevelGamePlayManager` classes where present. Use their actual source signatures and existing level-builder patterns. `SparkLevel92VisualRig` is **visual only**; pass authoritative actual positions/speeds/load and safety status to it. It cannot certify any objective.

## Deliverables still required
- Full one-click gameplay Editor menu `Tools/Project Spark/Levels/Create Level 92`, actual Level_92.asset and Level_92_Layout.prefab with real evaluator bindings.
- Real 188 player-connectable edges and reconciled independent terminals.
- Unity simulation, safety PLC, scenario state machine, three repair interactions, measured telemetry, six objective evaluator classes and Play Mode QA.
- Collisions/LODs and production material/VFX polish. Do not report completed implementation until exercised in Unity.
