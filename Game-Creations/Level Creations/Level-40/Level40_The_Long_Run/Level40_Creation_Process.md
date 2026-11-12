# Level 40 — The Long Run

Wire a tank transmitter and a 250 Ω receiver. Verify the 4–20 mA signal, diagnose the long cable's voltage limit, and prove that an open line differs from a valid empty-tank reading.

The Unity assets have been generated in Project Spark. Start at step 3 when the definition, prefab, and teaching drawing are present. In another checkout, run the builder or copy the supplied `Unity_Generated/Level_40_Layout` folder into `Prefabs/Levels/`. Keep the matching Unity metadata files with the scripts and assets.

1. If needed, copy the runtime and editor scripts into their matching folders under `Assets/My_Assets/_Project_Spark/Prefabs/Levels/Scripts/`.
2. In Edit Mode, run **Tools → Project Spark → Level 40 → Create Level Assets** if the assets are missing. Unity creates `Level_40.asset`, `Level_40_The_Long_Run_Layout.prefab`, and `Level_40_Drawing.asset` in `Assets/My_Assets/_Project_Spark/Prefabs/Levels/Level_40_Layout/`.
3. Pair the level definition and prefab in the active **Level Layout Catalog**. Put the definition after Level 39 in **LevelGamePlayManager → Levels** with **Auto Evaluate** enabled. Add the teaching asset to **Spark Draw Current Level → Drawings**.
4. Open Level 40 in Play Mode and connect W01–W05 with the Wire tool. On each module, the left socket is IN or + and the right socket is OUT or RETURN.

| Wire | From | To |
|---|---|---|
| W01 | `LOOP_POS` | `TX_IN` |
| W02 | `TX_OUT` | `CABLE_IN` |
| W03 | `CABLE_OUT` | `LINE_IN` |
| W04 | `LINE_OUT` | `RX_IN` |
| W05 | `RX_OUT` | `LOOP_RETURN` |

| Progress | Action | Loop | RX voltage | TX voltage |
|---|---|---:|---:|---:|
| **1/6** | Complete the loop at 0% and with the 50 Ω cable. | 4.00 mA | 1.00 V | 10.80 V |
| **2/6** | Press **50% TANK**. | 12.00 mA | 3.00 V | 8.40 V |
| **3/6** | Press **FIT LONG CABLE**. The 350 Ω cable uses more voltage, and the receiver still reads 50%. | 12.00 mA | 3.00 V | 4.80 V |
| **4/6** | Press **100% TANK**. The panel shows HEADROOM LIMIT. The receiver decodes about 79%. | ≈16.67 mA | 4.17 V | 2.00 V |
| **5/6** | Press **FIT SHORT CABLE**. The receiver returns to 100%. | 20.00 mA | 5.00 V | 6.00 V |
| **6/6** | Press **OPEN LINE**. Confirm 0 mA and LINE OPEN. Press **CLOSE LINE** and verify 20 mA returns. | 0 → 20.00 mA | 0 → 5.00 V | — |

The transmitter controller uses the solved voltage drop and current to estimate external loop resistance. It models current regulation with an adjustable series resistance and reserves 2 V for the transmitter. With the 350 Ω cable plus the 250 Ω receiver, the external load can draw about 16.67 mA from the 10 V available to it. The 250 Ω receiver decodes the 4–20 mA range from 0–100%. An open line shows a fault instead of a percentage.

The workbench reuses the existing supply, resistor, switch, and wire solvers. **RESTART TEST** closes the test contact, restores the short cable and 0% command, and clears the objectives. Rewiring restarts the sequence.
