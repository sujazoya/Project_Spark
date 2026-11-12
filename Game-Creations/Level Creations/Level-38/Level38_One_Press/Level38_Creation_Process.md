# Level 38 — One Press, One Count

Wire a button and an RC filter. Compare the raw bounce with the filtered signal and verify that two deliberate presses count exactly two parts.

The Unity assets were generated in Project Spark. Start at step 3 when the definition, prefab, and teaching drawing are present. For another checkout, use the builder or copy the supplied `Unity_Generated/Level_38_Layout` folder into `Prefabs/Levels/`. Keep the matching Unity metadata files alongside the scripts and assets.

1. If needed, copy the runtime and editor scripts into their matching directories under `Assets/My_Assets/_Project_Spark/Prefabs/Levels/Scripts/`.
2. With Play Mode stopped, run **Tools → Project Spark → Level 38 → Create Level Assets** if the assets are missing. Unity saves `Level_38.asset`, `Level_38_One_Press_Layout.prefab`, `Level_38_Drawing.asset`, and materials under `Assets/My_Assets/_Project_Spark/Prefabs/Levels/Level_38_Layout/`.
3. Pair the definition and prefab in the active **Level Layout Catalog**. Put the definition after Level 37 in **LevelGamePlayManager → Levels** with **Auto Evaluate** enabled. Add the drawing to **Spark Draw Current Level → Drawings**.
4. Open Level 38 in Play Mode and make all seven connections with the Wire tool. The left socket on each module is IN or +. The right socket is OUT or −. Use the panel buttons to run the repeatable contact waveforms.

| Wire | From | To |
|---|---|---|
| W01 | `Battery_positive` | `SW_IN` |
| W02 | `SW_OUT` | `FEED_IN` |
| W03 | `FEED_OUT` | `PULLDOWN_IN` |
| W04 | `FEED_OUT` | `FILTER_IN` |
| W05 | `PULLDOWN_OUT` | `Battery_negative` |
| W06 | `FILTER_OUT` | `CAP_PLUS` |
| W07 | `CAP_MINUS` | `Battery_negative` |

W03 and W04 share FEED OUT, the RAW node. FILTERED is at CAP +. W05 and W07 share Battery −.

| Progress | Action and expected result |
|---|---|
| **1/6** | Finish the wiring. RAW and FILTERED settle near 0 V. |
| **2/6** | Press **RAW TEST**. One bounced press produces three raw rising edges and three counts. Wait for the end of the discharge period. |
| **3/6** | Press **FILTERED MODE**, then **100 ms GLITCH**. The mode switch clears the raw count. RAW sees one edge, FILTERED stays below 3.0 V, and the count stays at zero. |
| **4/6** | Press **BOUNCED PRESS**. RAW sees three edges. FILTERED crosses 3.0 V once and the part count becomes one. The input is held. |
| **5/6** | Press **BOUNCED RELEASE**. RAW sees two rising edges during the release bounce. The part count stays at one. When FILTERED falls below 2.0 V, the panel reads ARMED. |
| **6/6** | Press **SECOND CYCLE**. The second bounced press and release adds one part and ends armed. The manager reports completion. |

The controller samples the solver's transient voltages. The circuit uses a 5 V source, a 1 kΩ feed, a 10 kΩ pull-down, a 10 kΩ series filter, and a 47 µF capacitor rated at 16 V. The active solver needs transient simulation enabled and a time step of 50 ms or less. The waveform changes at solver step boundaries. The workbench uses the existing power supply, switch, resistor, wire, and capacitor solvers.

The panel displays RAW edges for the current test and a cumulative PART COUNT within the selected mode. **RESTART TEST** clears the trial and lets the capacitor discharge. A wiring edit resets the trial. The four teaching pages cover wiring, the raw trace, glitch rejection and press acceptance, and release with rearm.
