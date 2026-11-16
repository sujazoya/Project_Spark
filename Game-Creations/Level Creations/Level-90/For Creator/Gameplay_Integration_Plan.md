# Project Spark Level 90 Gameplay Integration Plan — Proposed Work

1. Inspect actual existing SparkCircuitSystem, SparkElectricalSolver, SparkTerminal, SparkLevelEvaluator, LevelGamePlayManager, inventory and wire-disconnect API definitions.
2. Import 94 modular FBXs and the 80 exact ANIM_* parent groups; create gameplay colliders and electrical functional ports separate from disposable L90_HD_VISUAL_ART.
3. Reconcile 67 shared decorative socket locations. Keep 254 art anchors unchanged, create 352 distinct logical endpoint mappings, merge only provably compatible bus contacts. Record mapping and new contact counts.
4. Register 176 player-connectable wire declarations, but do not instantiate puzzle wires connected on load. All must update real circuit graph topology.
5. Model three separate domains: real educational 5 V control graph; fictional protected energy + xenon + plasma plant; independent safety authorizations.
6. Physically coherent finite propellant: tank pressure/mass state, regulators, actual valves, measured mass flows, measured PPU powers/losses and finite thermal dynamics.
7. Four propulsion channels: protected precharge, cathode neutralizer, magnet current, ion beam/discharge, actual thrust, gimbal feedback, thermal constraints and power availability.
8. Orbital guidance: commanded vs measured gimbal angles, finite thrust/propellant, full trajectory propagation, flight departure constraints and proper safe separation from the docking station.
9. Disturbances: xenon flow restriction, coil overheating, gimbal feedback drift; require diagnosis, physical repair and new qualified mission run.
10. Critical real W157 disconnect changes topology, invalidates trusted neutralizer feedback, inhibits ignition and initiates physical safe shutdown; reconnection + fresh measurement + calibration + independent checks + explicit reset + new full departure run.
11. Persist historical Objective 1–5 evidence even while current live health is invalid; Objective 6 requires evidence generated strictly after W157 repair. Connect only via actual LevelGamePlayManager APIs.
12. Wire read-only SCADA and visual rig to *actual authoritative measured state*, never to command widgets or fake timers. Perform Unity compilation, Play Mode, integration and previous-level regression tests in the actual user's Unity project.

**One-click gameplay builder target (NOT included here):** Tools/Project Spark/Levels/Create Level 90.

**Safety:** fictional bounded classroom game simulator, not physical propulsion system instructions or NASA qualification claims.
