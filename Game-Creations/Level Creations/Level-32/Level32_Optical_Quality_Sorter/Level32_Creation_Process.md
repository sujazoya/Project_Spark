# Level 32 — Optical Quality Sorter

Wire a 5 V optical inspection conveyor. Calibrate two reference tiles, then prove the gate can sort dark, bright, and borderline parts while ignoring brief glare.

## Generate and register

1. Copy the runtime script into a runtime folder under `Assets` and both editor scripts into an `Editor` folder. Skip the copy when the scripts are already installed. Let Unity finish compiling.
2. With Play Mode stopped, choose **Tools → Project Spark → Level 32 → Create Level Assets**. The command creates `Level_32.asset`, `Level_32_Optical_Sorter_Layout.prefab`, `Level_32_Drawing.asset`, and materials under `Assets/My_Assets/_Project_Spark/Prefabs/Levels/Level_32_Layout/`. The bench inherits the Level 31 workbench transform, with Level 30 as a fallback.
3. Add a row to the active **Level Layout Catalog**. Assign `Level_32.asset` to **Level** and `Level_32_Optical_Sorter_Layout.prefab` to **Prefab**. Add `Level_32.asset` after Level 31 in **LevelGamePlayManager → Levels**. Use the game's test unlock or complete Level 31 to unlock it.
4. Add `Level_32_Drawing.asset` to **Spark Draw Current Level → Drawings** on the visual teaching object. The four pages cover the circuit, exact wires, reference voltages, and glare rejection.
5. Enter Play Mode. Use the scene's existing EventSystem and Wire tool. The generated layout has fixed bindings for the ten terminals and six runtime objective IDs. Its five electrical components are preinstalled.

## Exact wiring

Connect the six wires with MAIN off. The diagram shows the generated bench from above. The layout starts with all wires open for the player to place.

| Wire | From | To |
|---|---|---|
| W01 | `Battery_positive` | `MAIN_IN` |
| W02 | `MAIN_OUT` | `LDR_A` |
| W03 | `LDR_B` | `R_DIV_IN` |
| W04 | `R_DIV_OUT` | `Battery_negative` |
| W05 | `MAIN_OUT` | `Motor_A` |
| W06 | `Motor_B` | `Battery_negative` |

Both W02 and W05 start at MAIN OUT. Both W04 and W06 land on supply negative. SENSE is the junction of LDR B and resistor IN. For an independent meter reading, use its red lead on SENSE and its black lead on supply negative. The built-in SENSE display is already connected to the solved nodes.

## Complete the six objectives

1. **Power.** After the status says the wiring is verified, turn MAIN on. The belt and shaft move, and the inspection light comes on.
2. **Calibrate.** Press **DARK REFERENCE** and wait for DARK OK. Press **BRIGHT REFERENCE** and wait for BRIGHT OK. The meter should read around 0.83 V and 4.32 V, respectively.
3. **Dark.** Press **RUN TEST**. Check that one dark part reaches DARK.
4. **Bright.** Press **RUN TEST**. Check that one bright part reaches BRIGHT.
5. **Review and glare.** Press **RUN TEST**. The first part reaches REVIEW. The second is dark with a brief bright reflection and still reaches DARK.
6. **Mixed batch.** Press **RUN TEST** and observe all six parts. Expected final counts are **DARK 2 / BRIGHT 2 / REVIEW 2** and the level manager should show **6/6**.

## Circuit behavior and troubleshooting

- The 10 kΩ resistor and LDR form a divider. More light lowers LDR resistance and raises SENSE. Nominal readings are 0.83 V for dark, 2.50 V for borderline, and 4.32 V for bright. Classification uses at most 1.5 V for DARK, at least 3.5 V for BRIGHT, and REVIEW between.
- The inspection window lasts 0.60 simulated seconds. The glare pulse occupies its first 0.12 seconds. A route needs 0.24 seconds in the same voltage band. A part without a stable reading goes to REVIEW. The gate uses the locked reading to route the part, and the trial awards credit after it reaches the correct bin.
- The existing solver supplies SENSE and motor current. The Level 32 controller supplies the simulated reflection values and animates the gate and part. The gate is a scene actuator, so it has no wire terminals.
- With MAIN off, the motor and sensor lose power. The objective counter resets when MAIN is turned off during the test. An invalid wire, bypass, or electrical fault also resets the sequence. Fix the connection reported in the status panel.
- If the belt moves but calibration fails, check the sensor branch and the 10 kΩ value. If the belt stays stopped, check the motor branch and orientation. The expected motor current is about 150 mA.
- If the controller reports a missing solver definition, check the active registry contains `SparkLDRSolverDefinition.asset` and `Level30_MotorSolver.asset`. Both were present in the project's shared registry when the level was built.
- If wiring clicks miss the sockets, verify that the Wire tool includes the `Pin` layer. If the whiteboard reports a missing Level 32 drawing, check that the drawing was added to the active `SparkDrawCurrentLevel`.
