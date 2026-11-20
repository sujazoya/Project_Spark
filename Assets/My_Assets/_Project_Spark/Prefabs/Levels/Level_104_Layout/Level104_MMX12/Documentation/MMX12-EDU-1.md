# Level 104 · Memory Matrix

MMX12-EDU-1 is the fictional training circuit for the packaged 260 × 185 mm, eight-layer board. It keeps all 94 original component positions and scales the visual board to about 10.13 × 7.21 Unity units on the 15.79 × 8.54-unit workbench.

The [training route map](../Data/MMX12_EDU_1_Training_Routes.csv) records W001–W124, 34 legal training spans, and the separate W125 connection. The source JSON names U11 and U12, which the BOM does not contain. The training model uses an isolated memory-ready output on U1, the controller, and an independent permit input on U9, the BOM's power/reset supervisor. The original JSON and unassigned data remain available.

| Objective | Evidence |
| --- | --- |
| Assembly | Fit and solder 94 parts. Inspect U1, U2 and U3 BGA alignment before closing their solder contacts. |
| Routing | Complete 124 exact pairs, 34 barrels, six continuity groups and W125. |
| Initialize | Qualify the native controller rail, 2.50 V wordline supply, 1.20 V data supply, 0.60 V termination and reference, then verify complementary CK and CA. |
| Train and test | Scan independent read/write taps at a modeled 625 ps unit interval. Each lane needs at least 85 ps of read and write margin. Test 64 bursts across four lanes and four patterns for 32,768 compared bits. |
| Recover faults | Repair the W063/V19 byte-2 strobe, RN1 CA termination, and warmed U3 data contact. The U3 virtual rework cycle blocks power until it finishes. |
| W125 safety | Open real feedback and verify the independent permit is withdrawn. Restore the link, retrain, pass fresh data, reset U9, then pass another full training and data run before authorization returns. |

Memory Matrix has the campaign/save ID `level_104_memory_matrix` so it can coexist with the historical W067 exercise. Use `Tools > Project Spark > Levels > 104 > Run Gameplay Acceptance` with MainMenu active. Reports and screenshots are written under `Logs`.
