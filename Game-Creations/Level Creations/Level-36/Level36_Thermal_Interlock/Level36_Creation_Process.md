# Level 36 — Thermal Interlock

Wire a test chamber and prove that the cooling state survives small temperature changes. Then test the sensor fault latch and recover the heater.

The assets are already generated in the Project Spark Unity project. Start at step 3 if the definition, prefab, and drawing are present. For another checkout of Project Spark, run the builder or copy the included `Unity_Generated/Level_36_Layout` folder to `Prefabs/Levels/`. Keep the script, folder, and asset `.meta` files with their matching files to preserve references.

1. Install the runtime and two editor scripts in the matching folders under `Assets/My_Assets/_Project_Spark/Prefabs/Levels/Scripts/` if needed.
2. With Play Mode stopped, run **Tools → Project Spark → Level 36 → Create Level Assets** if the assets are missing. Unity saves `Level_36.asset`, `Level_36_Thermal_Interlock_Layout.prefab`, `Level_36_Drawing.asset`, and materials in `Assets/My_Assets/_Project_Spark/Prefabs/Levels/Level_36_Layout/`.
3. In the active **Level Layout Catalog**, pair the definition with the layout prefab. In **LevelGamePlayManager → Levels**, put the definition after Level 35 and keep **Auto Evaluate** enabled. Add the drawing to **Spark Draw Current Level → Drawings**. Complete Level 35 or use your existing test unlock.
4. Confirm the existing motor solver is in the active registry. Project Spark used `Level30_MotorSolver.asset` for this solver in Levels 30 and 34. The level also uses the existing power supply, resistor, switch, LED, and wire solvers.
5. Enter Play Mode and use the Wire tool to connect the 13 wires. Each supply socket takes four wires. Use the **left** socket as IN or A and the **right** socket as OUT, B, K, or RETURN on the workbench modules.

| Wire | From | To |
|---|---|---|
| W01 | `Battery_positive` | `PULLUP_IN` |
| W02 | `PULLUP_OUT` | `SENSOR_IN` |
| W03 | `SENSOR_RETURN` | `Battery_negative` |
| W04 | `Battery_positive` | `FAN_SW_IN` |
| W05 | `FAN_SW_OUT` | `FAN_A` |
| W06 | `FAN_B` | `Battery_negative` |
| W07 | `Battery_positive` | `HEAT_SW_IN` |
| W08 | `HEAT_SW_OUT` | `HEATER_IN` |
| W09 | `HEATER_OUT` | `Battery_negative` |
| W10 | `Battery_positive` | `FAULT_SW_IN` |
| W11 | `FAULT_SW_OUT` | `LED_R_IN` |
| W12 | `LED_R_OUT` | `LED_A` |
| W13 | `LED_K` | `Battery_negative` |

| Progress | Controls and expected result |
|---|---|
| **1/6** | Complete the wiring. **COOL** reads about 3.00 V. Fan off, heater on, fault LED off. |
| **2/6** | Press **HOT**. At about 1.60 V, the fan starts and the heater cuts out. |
| **3/6** | Press **WARM**. At about 2.25 V, the fan stays on. |
| **4/6** | Press **COOL**, then **WARM**. The fan stops at 3.00 V and stays off at 2.25 V. The heater stays on. |
| **5/6** | Press **COOL**, then **OPEN SENSOR**. SENSE rises near 5 V, the warning latches, the heater cuts out, and the fan runs. |
| **6/6** | Press **RESTORE SENSOR**. The warning should stay on at 3.00 V. Press **RESET FAULT**, then **SHORT SENSOR**. At about 0 V, verify the same fault response. Press **RESTORE SENSOR**, then **RESET FAULT**. Wait for the fan and warning to go off and the heater to return. |

The 10 kΩ pull-up limits the short-test current to approximately 0.50 mA. The simulated NTC is 15 kΩ at COOL, 8.2 kΩ at WARM, and 4.7 kΩ at HOT. The fan turns on below 2.00 V and turns off above 2.70 V. The open fault is detected within 0.15 V of the supply rail and the short fault at 0.15 V or below.

Use **RESTART TEST** to return to the cold-start check. A wiring edit also resets the test. If the meter stays blank, follow the status prompt for the first missing connection. If the fault LED stays dark during a fault, check W10–W13 and the LED polarity.
