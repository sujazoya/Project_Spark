# Level 35 — Bridge Balance: Find the Hidden Fault

Build a Wheatstone bridge, establish a healthy reference, diagnose three hidden resistor faults from voltage readings, and restore both branches. A zero difference is possible even when both arms are wrong.

**Already generated in the Project Spark Unity project.** Start at step 3 if the definition, prefab, and drawing are already present. For another checkout of the same project, either follow steps 1–2 to rebuild or import the bundled `Unity_Generated/Level_35_Layout` folder into `Prefabs/Levels/` along with its folder metadata. When importing the ready-made assets, preserve the supplied script and asset `.meta` files so the prefab keeps its references.

1. With Play Mode stopped, let Unity compile the three Level 35 scripts. For a manual install, copy the files in `Runtime/` and `Editor/` into the matching folders under `Assets/My_Assets/_Project_Spark/Prefabs/Levels/Scripts/`.
2. Run **Tools → Project Spark → Level 35 → Create Level Assets**. This creates `Level_35.asset`, `Level_35_Bridge_Balance_Layout.prefab`, `Level_35_Drawing.asset`, and materials under `Assets/My_Assets/_Project_Spark/Prefabs/Levels/Level_35_Layout/`. The builder copies the workbench placement from Level 34, with Level 33 as a fallback.
3. In the active **Level Layout Catalog**, pair the new definition with the new prefab. In **LevelGamePlayManager → Levels**, append the definition after Level 34 and keep **Auto Evaluate** enabled. Complete Level 34 or use your existing test unlock.
4. Add `Level_35_Drawing.asset` to **Spark Draw Current Level → Drawings**. The four pages cover wiring and baseline, single faults, false balance, and repair. If you need to generate only the drawing, use **Tools → Project Spark → Level 35 → Create Teaching Drawing**.
5. Enter Play Mode, open Level 35, select the Wire tool, and add these six wires. The supply and four resistor modules are already installed. On each resistor, **IN** is left and **OUT** is right.

| Wire | From | To |
|---|---|---|
| W01 | `Battery_positive` | `LU_IN` |
| W02 | `Battery_positive` | `RU_IN` |
| W03 | `LU_OUT` | `LL_IN` |
| W04 | `RU_OUT` | `RL_IN` |
| W05 | `LL_OUT` | `Battery_negative` |
| W06 | `RL_OUT` | `Battery_negative` |

LU and LL are the left upper and lower arms; RU and RL are the right upper and lower arms. Each supply socket takes two wires. Leave the two midpoint nets separate. The panel measures them independently against Battery − and displays **DIFF = LEFT − RIGHT**.

| Progress | Action | LEFT | RIGHT | DIFF |
|---|---|---:|---:|---:|
| 1/6 | Finish the six wires; both branch indicators turn on. | 2.50 V | 2.50 V | 0.00 V |
| 2/6 | Press **CAPTURE BASELINE**. | 2.50 V | 2.50 V | 0.00 V |
| 3/6 | **LOAD CASE**, then choose **LEFT LOWER**. | 3.44 V | 2.50 V | +0.94 V |
| 4/6 | **LOAD CASE**, then choose **RIGHT LOWER**. | 2.50 V | 3.44 V | −0.94 V |
| 5/6 | **LOAD CASE**, then choose **BOTH LOWER**. | 3.44 V | 3.44 V | 0.00 V |
| 6/6 | Press both **FIT … 1 kΩ** buttons, wait for the readings, then press **VERIFY REPAIR**. | 2.50 V | 2.50 V | 0.00 V |

The upper arms stay at 1 kΩ. The cases change one or both lower arms from 1 kΩ to 2.2 kΩ. The double fault preserves the voltage ratio between the two midpoint readings, raising both equally. Fixing just one arm brings back the signed difference; fixing both restores the healthy baseline.

For a negative check, try a wrong diagnosis and confirm progress stays put. Press **RESTART TEST** to clear the trial and restore the nominal resistors. Editing a wire resets the trial too. If the meter stays blank, read the status line for the first missing or merged net and check the terminal labels. If the panel says “Bridge verified” but the manager stays below 6/6, confirm the active definition and layout are both from Level 35 and Auto Evaluate is enabled.

The level reuses the existing power supply, resistor solver, wire tool, and EventSystem. The six objective bindings are generated with the layout. No shared solver changes or extra solver registration are required.
