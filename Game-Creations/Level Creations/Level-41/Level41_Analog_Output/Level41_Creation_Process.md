# Level 41 — Build the Analog Output

Wire a 3-bit R–2R ladder, measure its bit weights, diagnose an open termination, and verify all eight output codes.

The Unity assets have been generated in Project Spark. Start at step 3 when the definition, prefab, and teaching drawing are present. In another checkout, run the builder or copy the supplied `Unity_Generated/Level_41_Layout` folder into `Prefabs/Levels/`. Keep the matching Unity metadata files with the scripts and assets.

1. If needed, copy the runtime and editor scripts into the corresponding folders under `Assets/My_Assets/_Project_Spark/Prefabs/Levels/Scripts/`.
2. In Edit Mode, run **Tools → Project Spark → Level 41 → Create Level Assets** if the assets are missing. Unity creates `Level_41.asset`, `Level_41_Analog_Output_Layout.prefab`, and `Level_41_Drawing.asset` in `Assets/My_Assets/_Project_Spark/Prefabs/Levels/Level_41_Layout/`.
3. Pair the definition and prefab in the active **Level Layout Catalog**. Put the definition after Level 40 in **LevelGamePlayManager → Levels** with **Auto Evaluate** enabled. Add the teaching asset to **Spark Draw Current Level → Drawings**.
4. Open Level 41 in Play Mode and add W01–W09 with the Wire tool. B2 is the left driver, B1 is the middle driver, and B0 is the right driver. The 5 V reference feeds the drivers through internal contacts.

| Wire | From | To |
|---|---|---|
| W01 | `BIT2` | `B2_R_IN` |
| W02 | `BIT1` | `B1_R_IN` |
| W03 | `BIT0` | `B0_R_IN` |
| W04 | `B2_R_OUT` | `LINK21_IN` |
| W05 | `LINK21_OUT` | `B1_R_OUT` |
| W06 | `B1_R_OUT` | `LINK10_IN` |
| W07 | `LINK10_OUT` | `B0_R_OUT` |
| W08 | `B0_R_OUT` | `TERM_R_IN` |
| W09 | `TERM_OUT` | `DAC_RETURN` |

The 20 kΩ branches and both 10 kΩ links have IN on the left and OUT on the right. B1 OUT and B0 OUT each take two wires. The output is measured at B2 OUT relative to DAC RETURN. The termination contact is inside the 20 kΩ termination module.

| Progress | Action and expected reading |
|---|---|
| **1/6** | Finish the wiring. The applied code **000** reads **0.000 V**. |
| **2/6** | Set **001** and press **APPLY CODE**. Read **0.625 V**. |
| **3/6** | Apply **010** and **100**, in either order. Read **1.250 V** and **2.500 V**. |
| **4/6** | Make **3.125 V** by applying **101**. |
| **5/6** | Apply **111** and verify **4.375 V**. Press **OPEN LINK**. With 111 still applied, the output rises near **5.000 V**. Choose **OPEN TERMINATION**. |
| **6/6** | Press **RESTORE LINK**, then **RUN SWEEP**. The controller measures all eight codes and verifies the seven equal steps. |

At a 5 V reference, the nominal sweep is **0.000, 0.625, 1.250, 1.875, 2.500, 3.125, 3.750, 4.375 V**. The runtime uses the existing source, switch, resistor, and wire solvers. Each bit driver opens the previous rail contact before closing the other one. **RESTART TEST** resets the bits, restores the termination, and clears objectives. A wiring edit restarts the sequence.
