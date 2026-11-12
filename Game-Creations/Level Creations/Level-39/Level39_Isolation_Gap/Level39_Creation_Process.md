# Level 39 — Across the Isolation Gap

Build two circuits that share an optical command. Verify active-low signaling, independent receiver power, and electrical isolation.

The Unity assets have been generated in Project Spark. Start at step 3 when the definition, prefab, and teaching drawing are present. In another checkout, run the builder or copy the supplied `Unity_Generated/Level_39_Layout` folder into `Prefabs/Levels/`. Keep the matching Unity metadata files alongside the scripts and assets.

1. If needed, install the runtime and editor scripts into the matching folders under `Assets/My_Assets/_Project_Spark/Prefabs/Levels/Scripts/`.
2. In Edit Mode, run **Tools → Project Spark → Level 39 → Create Level Assets** if the assets are missing. Unity generates `Level_39.asset`, `Level_39_Isolation_Gap_Layout.prefab`, and `Level_39_Drawing.asset` in `Assets/My_Assets/_Project_Spark/Prefabs/Levels/Level_39_Layout/`.
3. Pair the level definition and prefab in the active **Level Layout Catalog**. Put the definition after Level 38 in **LevelGamePlayManager → Levels** with **Auto Evaluate** enabled. Add the teaching asset to **Spark Draw Current Level → Drawings**.
4. Open Level 39 in Play Mode. Connect W01–W07 with the Wire tool. Use the panel to control SEND and RX POWER. Add W08 only when the level asks for the bridge test.

| Wire | From | To | Use |
|---|---|---|---|
| W01 | `TX_POS` | `SEND_IN` | Base |
| W02 | `SEND_OUT` | `TX_R_IN` | Base |
| W03 | `TX_R_OUT` | `OPTO_A` | Base |
| W04 | `OPTO_K` | `TX_RETURN` | Base |
| W05 | `RX_POS` | `PULLUP_IN` | Base |
| W06 | `PULLUP_OUT` | `OPTO_C` | Base |
| W07 | `OPTO_E` | `RX_RETURN` | Base |
| W08 | `TX_RETURN` | `RX_RETURN` | Temporary fault test |

The TX and RX supplies are on opposite sides of the bench. The left socket on each supply is + and the right socket is RETURN. A/K are the left/right TX LED sockets. C/E are the left/right RX output sockets. The receiver meter references **RX RETURN**.

| Progress | What to do and what to observe |
|---|---|
| **1/6** | Connect W01–W07. SEND is off, TX LED reads near 0 mA, RX OUT is near 5 V, and the panel shows ISOLATED. |
| **2/6** | Press **SEND ON**. TX LED is about 6.4 mA and RX OUT falls near 0 V. |
| **3/6** | Press **SEND OFF**. TX LED goes out and the receiver pull-up returns RX OUT to about 5 V. |
| **4/6** | Press **SEND ON**, then **RX POWER OFF**. The TX LED stays on while RX OUT shows UNPOWERED. Press **RX POWER ON** and wait for RX OUT to return LOW. |
| **5/6** | Add W08 between the two return sockets. The panel must show BRIDGED even while RX OUT remains LOW. |
| **6/6** | Remove W08. Press **SEND OFF** to verify isolated HIGH, then **SEND ON** to verify isolated LOW and finish. |

Project Spark models the optocoupler with a solved TX LED and a receiver contact driven by the LED current. At 4 mA or higher the receiver contact closes, sinking about 5 mA through the 1 kΩ pull-up. The circuit checker evaluates cross-gap wiring independently of the output voltage. **RESTART TEST** clears progress and resets the switches. Remove any test jumper separately if it is still installed.
