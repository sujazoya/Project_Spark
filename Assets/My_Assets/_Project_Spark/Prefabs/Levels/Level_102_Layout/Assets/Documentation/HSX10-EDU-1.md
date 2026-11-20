# Level 102 · Signal Phantom

HSX10-EDU-1 is the fictional training circuit for the HSX-10 board. It uses the source kit's 250 × 175 mm board and 82 original component positions. The workbench view scales the board to about 10.13 × 7.09 Unity units to fit the campaign table.

The [training route map](../Data/HSX10_EDU_1_Training_Routes.csv) records W001–W108, their 28 plated layer spans, and W109. Coordinates are measured in millimetres in the board plane. The source kit's unassigned route and via tables remain the provenance for the task identifiers.

| Objective | Evidence |
| --- | --- |
| Assembly | Fit and solder 82 parts, including the W109 installable. |
| Routing | Complete 108 native route pairs, 28 via barrels, six continuity branches and the separate W109 link. |
| Commissioning | Verify the protected 5 V input, 3.3 V logic and 1.8 V lane rails, clock, and reset. Calibrate and verify both polarities at both receivers. |
| Signal integrity | Pass both lanes' modeled 1 Gb/s TDR, eye, skew, coupling and finite 4,096-bit checks. |
| Repair | Restore R1 from 150 Ω to 100 Ω, increase the W041 aggressor spacing, and rework the V19 open stub while retaining its conducting L1–L3 span. Verify each with a new calibration, training epoch, and capture. |
| Safety | Open W109 to inhibit U11's protected transmission. Reconnect, verify fresh U9 heartbeat edges, reset U11, retrain and qualify both lanes before enabling transmission. |

Unity's existing electrical solver owns continuity, power and receiver polarity. After those checks, the training channel computes attenuation from route length, sampled reflections from impedance discontinuities, coupling from spacing, and eye margins and observed errors from a deterministic PRBS sequence. The capture is a finite educational measurement. It supports comparison of game repairs and does not establish physical board compliance or a zero bit-error rate.

Signal Phantom uses the campaign/save identifier `level_102_signal_phantom`. The earlier timing-board definition carries the historical `level_102` identifier.

Run `Tools > Project Spark > Levels > 102 > Run Gameplay Acceptance` with MainMenu active. Unity writes the report and screenshots under `Logs`.
