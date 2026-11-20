# Level 103 · The Power Core

CPX11-EDU-1 is the fictional training schematic for the source CPX-11. It uses all 88 original footprints on the 240 × 180 mm, eight-layer PCB. The workbench view scales to approximately 10.13 × 7.60 Unity units.

The [training route map](../Data/CPX11_EDU_1_Training_Routes.csv) records W001–W116, 30 plated spans, and W117, with coordinates in millimetres. The original unassigned source records remain the provenance for task IDs.

| Stage | Evidence |
| --- | --- |
| Assembly | Correct placement and native solder contacts for all 88 parts. |
| Routing | Exact terminal pairs, legal layer spans, six live continuity branches and the separate W117 link. |
| Commissioning | Solved protected 12 V, 3.3 V and 1.8 V rails, a live path for each phase and stable low-load VCORE. |
| CPU loads | Four measured profiles with voltage, phase balance, ripple, temperature and efficiency limits. |
| Repairs | Reproduce and correct R2 Phase 2 sense drift, W041/V017 Phase 3 output loss, and Q7 Phase 4 heating. Each repair needs new calibration and all four loads. |
| Safety | U10 sends fresh VCORE-good pulses to U11 through W117. Opening the connection withdraws the CPU permit while the auxiliary rail and diagnostic preload remain available. Reconnection, fresh evidence, explicit U11 reset and a new protected capture are required. |

The existing four-phase averaged model uses 1.00 V nominal VCORE, 400 kHz per phase with quarter-cycle interleaving, and a 2.5 mΩ load line. The test bank applies 2 A, 10 A, 20 A, then a 2–20–2 A transient with a 450 A/s command slew to model a bounded load edge. Each run samples the output, independent phase currents, sensor deviation and heating. Native circuit measurements determine the input, auxiliary rails, phase availability and W117 authorization. The separate acquisition board retains its historical campaign ID. The Power Core uses `level_103_power_core`.

Use `Tools > Project Spark > Levels > 103 > Run Gameplay Acceptance` with MainMenu active. The report and captures appear under `Logs`.
