# Level 111 · The Signal Crucible

SIX19-EDU-1 is the fictional campaign circuit for the supplied 330 × 240 mm SIX-19 board. The source drawings, manifest and unassigned fabrication records retain their original status. In this exercise, native training contacts implement the logical U18-to-U19 W185 feedback path.

The board keeps its 138 original placement positions and uniform scale. At 10.13 × 7.37 Unity units it fits the existing 15.79 × 8.54 game table.

| Objective | Evidence |
| --- | --- |
| Assembly | Seat and solder all 138 components, with explicit contact inspection for ICs and connectors. |
| Copper | Close all 184 exact terminal pairs and 60 plated vias across 14 layers, and connect W185. The [training route map](../Data/SIX19_EDU_1_Training_Routes.csv) defines the level’s pins and spans. |
| Commissioning | Verify 3.3 V and 1.8 V, sample both native REFCLK states, then qualify the four lanes at the 10 Gb/s teaching rate. |
| Signal quality | Measure TDR and eye, set equalizer taps and compare 65,536 PRBS31 pattern bits with the retimer above 53 °C. |
| Fault recovery | Locate and replate V37 (lane 2 return), repair U5 (excess phase noise), and rework U8 (hot equalizer bias drift). Acquire new instrument evidence after each repair. |
| W185 | Demonstrate independent workload inhibition while the diagnostic rails remain up. Restore W185 and qualify a fresh heartbeat, reset U19, and qualify again before the permit returns. |

The signal model uses sampled native terminal levels, authored training trace lengths, return continuity, reference jitter and measured retimer bias. TDR distance uses 0.16 mm/ps propagation. The eye and TDR plots show the bounded model. The billion-bit readout is a projection, and the separate observed-error counter reports the finite comparison.

Launch **The Signal Crucible** from the Main Menu. Its campaign entry is appended to preserve the positions used by existing progression saves. The **Create Level 111** menu item rebuilds the playable prefab and the **Run Gameplay Acceptance** item under **Levels → 111** records Unity Play Mode results in the project’s `Logs` directory. Level 112 is outside the current content.
