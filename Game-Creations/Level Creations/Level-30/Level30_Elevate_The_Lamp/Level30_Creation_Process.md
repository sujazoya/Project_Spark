# Level 30 — Elevate the Lamp

Wire a lift that can travel both ways, stop automatically at each end, and light its lamp only at the top. Complete a full UP and DOWN test.

## Install and generate the level

1. Exit Play Mode. Copy the two files from `Runtime` into `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Levels/Level30/`. Copy `SparkLevel30Builder.cs` from `Editor` into an `Editor` subfolder there. Wait for Unity to compile.
2. Run **Tools → Project Spark → Level 30 → Create Level Assets**. This generates `Level_30.asset`, `Level_30_Layout.prefab`, `Level30_MotorSolver.asset`, and the workbench materials under `Assets/My_Assets/_Project_Spark/Prefabs/Levels/Level_30_Layout/`. It uses the Level 29 bench as a placement reference when available. Open the generated prefab and check the `Lift Workbench` child position and scale in your scene.
3. Select the solver registry used by `SparkElectricalSolver` on `SparkElectronicSystems`. Run **Tools → Project Spark → Level 30 → Register Motor Solver in Selected Registry**. Verify that the registry includes `Level30_MotorSolver`. This adds support for the existing `SparkMotor` component with a signed resistive winding model.
4. On the `SparkLevelLayoutCatalog` assigned to `SparkLevelLayoutLoader`, add a new entry with **Level = Level_30.asset** and **Prefab = Level_30_Layout.prefab**. Add the same definition to the `LevelGamePlayManager` Levels list after Level 29. Keep auto-advance off for the first run.
5. Enter Play Mode and select Level 30. If you finished Level 29 before registering Level 30, use your project's existing test unlock or complete Level 29 again so progression can unlock the new entry.

The generated prefab includes the parts and fixed bindings. Use the Wire tool to build the circuit in Play Mode. The direction selector is the **UP / OFF / DOWN** button strip in the generated layout. The four direction contacts are controlled together by these buttons. The upper, lower, and top lamp contacts are driven by platform position.

## Parts and modeled values

| Part | Configuration |
|---|---|
| Supply | 5 V, 0.35 A limit |
| Reversible lift motor | 33 Ω winding, 3 V startup threshold, 0.25 A maximum |
| UP and DOWN contacts | Two contacts per direction, feeds and returns |
| Upper limit | Closed below the top, opens at the top |
| Lower limit | Closed above the bottom, opens at the bottom |
| Top lamp contact | Closes at the top, opens as the platform leaves |
| Lamp | 2 V LED with 330 Ω series resistor |

Motor terminals **A and B** accept either polarity. A above B raises the lift. B above A lowers it. Movement uses solved motor voltage and current. The motor model treats the winding as a resistor and uses a fixed travel speed once the motor reaches its startup voltage. It does not model acceleration, load torque, or back EMF.

## Connect the 14 wires

Use the left socket for **IN** and right socket for **OUT** on every contact. **A** and **K** on the lamp mean anode and cathode. The generated terminal GameObject names match the IDs in this table.

| Wire | From | To |
|---|---|---|
| W01 | `Battery_positive` | `UP_Feed_IN` |
| W02 | `Battery_positive` | `DOWN_Feed_IN` |
| W03 | `Battery_positive` | `Top_Lamp_IN` |
| W04 | `Battery_negative` | `UP_Return_OUT` |
| W05 | `Battery_negative` | `DOWN_Return_OUT` |
| W06 | `Battery_negative` | `Lamp_K` |
| W07 | `UP_Feed_OUT` | `Upper_Limit_IN` |
| W08 | `DOWN_Feed_OUT` | `Lower_Limit_IN` |
| W09 | `Upper_Limit_OUT` | `Motor_A` |
| W10 | `Motor_A` | `DOWN_Return_IN` |
| W11 | `Lower_Limit_OUT` | `Motor_B` |
| W12 | `Motor_B` | `UP_Return_IN` |
| W13 | `Top_Lamp_OUT` | `R_LED_IN` |
| W14 | `R_LED_OUT` | `Lamp_A` |

Each battery terminal has three wires. Both motor terminals have two. The validator reports the first missing connection or joined path in the Level 30 status panel. Wiring edits reset the trial and place the lift at the bottom with the selector off.

## Play Mode acceptance test

| Step | Player action | Expected observation |
|---|---|---|
| 1 | At the bottom, select **DOWN** | Lower limit is open. Motor remains still, current ≤1 mA, lamp off. **1/5**. |
| 2 | Select **UP** | Motor A is positive relative to B. Expect about +4.96 V and +150 mA. The lift rises with the lamp off. **2/5**. |
| 3 | Leave **UP** selected at the top | Upper limit opens. Motor current falls below 1 mA. Lamp lights at about 9 mA. **3/5**. |
| 4 | Select **DOWN** | Motor voltage and current reverse. The lift leaves the top and the lamp goes out. **4/5**. |
| 5 | Leave **DOWN** selected at the bottom | Lower limit opens. Motor stops, lamp stays off, and the level reaches **5/5**. |

The selector opens the previous direction contacts before closing the next pair. The lower limit affects only the DOWN feed and the upper limit affects only the UP feed, so a limit stop leaves the opposite direction available. The `OFF` position opens both pairs.

After the positive test, add a wire across either limit and confirm the validator rejects it. Remove it, then test a wire directly from 5 V to the LED resistor input. The validator should flag the top-lamp contact bypass. Edit or clear wires with the selector OFF.

## Completion and troubleshooting

- The generated level expects all **five** sequence objectives. Each verified step binds one runtime objective to the real 5 V source. This lets earned steps remain visible while the lamp is off. The standard level manager awards completion after the round trip. Closed source return is optional at final evaluation because the lift and lamp are both off at the bottom.
- If the wire tool cannot hit a socket, check the active tool, the terminal collider and `SignalWireBuilder` terminal mask. The builder puts sockets on the existing `Pin` layer.
- If the status asks for `Level30_MotorSolver`, verify that the asset is on the active registry, then restart Play Mode.
- If the workbench needs alignment, adjust only the `Lift Workbench` child of the Level 30 prefab. Keep its electrical references and terminals together.
- Verify that the standard EventSystem receives the selector button clicks. The generated panel uses your scene's existing EventSystem.

The scripts compile against the project assemblies. The editor menu and live wiring sequence must still be verified in Unity after installation.
