# Level 112 · The EMC Gauntlet

EMC20-EDU-1 is the fictional training circuit for the supplied 360 × 250 mm board. The source’s unassigned fabrication records keep their original status. The [training route map](../Data/EMC20_EDU_1_Training_Routes.csv) contains the playable terminals and layer spans. W209 connects the U21 qualification output to U9’s independent safety input.

All 152 original component positions are retained. The three source shield cans have separate playable mounts near the seam monitor, clock source and line driver. The copper view hides their lids while preserving the native bond. The board’s presentation scale is uniform and fits 10.13 × 7.03 Unity units on the 15.79 × 8.54 game table. The route map records physical pad coordinates in millimetres independently of the presentation scale.

| Objective | Required evidence |
| --- | --- |
| Assembly | Fit and solder 152 components. Mount all three shield cans. |
| Copper | Connect 208 exact terminal pairs, plate 68 spans across 16 layers, and install W209 separately. |
| Baseline | Verify the protected supply and complementary reference states. Calibrate the fixture and measure the idle spectrum. |
| Suppression | Verify the loaded spectra, 24 MHz insertion loss, shield bond, return continuity, and eye. Compare 512 packets totaling 8,192 bytes. |
| Repair | Rebond and remount shield can 1, replace L1 for load-dependent saturation, and plate V44 on L06 after measuring the open return. Pass a fresh test after each repair. |
| Safety | Open W209 and observe U9 inhibit the protected workload with diagnostics still powered. Reconnect, qualify the heartbeat, reset U9 and complete another full test. |

The fixture model uses a 50 Ω port and a 0.25 m probe at a fixed orientation. A 96-sample quadrature detector spans four cycles at each of eight center frequencies from 0.15 to 240 MHz. The source current, choke bias, shield bond and return state come from solved terminal or device measurements. The thresholds and ±0.8 dB observation window define the fictional training exercise. The spectrum, eye, and packet readback use bounded educational models.

Open **The EMC Gauntlet** from the Main Menu. **Tools → Project Spark → Levels → Create Level 112** rebuilds the playable prefab. **Levels → 112 → Run Gameplay Acceptance** records Play Mode results and captures under `Logs`.
