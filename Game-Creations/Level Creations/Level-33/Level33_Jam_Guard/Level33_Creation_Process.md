# Level 33 — Jam Guard

Build a capacitor timer that ignores a brief obstruction, cuts motor power during a sustained jam, and waits for a manual reset.

## Generate and register

1. If the scripts are not yet installed, copy `SparkLevel33JamGuard.cs` into a runtime folder under `Assets` and both editor scripts into an `Editor` folder. Let Unity compile.
2. Exit Play Mode and run **Tools → Project Spark → Level 33 → Create Level Assets**. Unity creates `Level_33.asset`, `Level_33_Jam_Guard_Layout.prefab`, `Level_33_Drawing.asset`, and the materials under `Assets/My_Assets/_Project_Spark/Prefabs/Levels/Level_33_Layout/`. The prefab uses Level 32's workbench placement, with Level 31 as a fallback.
3. In the active **Level Layout Catalog**, pair the Level 33 definition with the prefab. Append the same definition after Level 32 in **LevelGamePlayManager → Levels**. Use the game's test unlock or complete Level 32 to unlock Level 33.
4. Add `Level_33_Drawing.asset` to **Spark Draw Current Level → Drawings**. Its four pages cover charging, discharging, the motor cutoff, and the six checks.
5. Enter Play Mode, select Level 33, and wire the preinstalled components. The layout has fixed bindings and uses the scene's existing Wire tool and EventSystem.

## Exact wiring

The diagram shows the bench from above. On each contact and resistor, IN is left and OUT is right.

| Wire | From | To |
|---|---|---|
| W01 | `Battery_positive` | `JAM_IN` |
| W02 | `JAM_OUT` | `R_CHARGE_IN` |
| W03 | `R_CHARGE_OUT` | `Timer_Positive` |
| W04 | `Timer_Negative` | `Battery_negative` |
| W05 | `Timer_Positive` | `R_CLEAR_IN` |
| W06 | `R_CLEAR_OUT` | `CLEAR_IN` |
| W07 | `CLEAR_OUT` | `Battery_negative` |
| W08 | `Battery_positive` | `RUN_IN` |
| W09 | `RUN_OUT` | `Motor_A` |
| W10 | `Motor_B` | `Battery_negative` |

Two wires meet on Battery + and TIMER +. Three wires meet on Battery −. Keep the two timer contacts and the motor cutoff in their branches. A wire directly across a contact bypasses it and fails wiring validation.

## Six checks

| Objective | Player action | Expected behavior |
|---|---|---|
| 1/6 | Press **START TEST** after wiring is verified and TIMER is below 0.25 V. | Conveyor runs. RUN lights. |
| 2/6 | Press **BRIEF BLOCK** and wait for the obstruction to clear automatically. | TIMER peaks around 1.0 V, then falls below 0.25 V. The motor keeps running. |
| 3/6 | Press **JAM** and leave the parcel in place. | TIMER reaches 3.0 V in about 2 s. The motor stops and JAM lights. |
| 4/6 | Press **RESET** while blocked. | Reset is refused. Motor remains stopped. |
| 5/6 | Press **CLEAR JAM**, then wait for TIMER to fall below 1.0 V. | The timer discharges. The JAM trip stays latched. |
| 6/6 | Press **RESET** again. | JAM clears, RUN lights, and the motor restarts. |

The display shows solved TIMER voltage, motor current, and the current state. **RESTART TEST** clears trial progress and returns the controls to the initial state. Wait for any remaining capacitor charge to drain before starting the new test. Wiring edits also reset the trial.

## Settings and validation

- The capacitor is 470 µF, rated 16 V. The charge resistor is 4.7 kΩ, and the discharge resistor is 1 kΩ. Their nominal time constants are 2.21 s and 0.47 s.
- The JAM and CLEAR switches represent two complementary sensor contacts. The level controller opens the active path before closing the other. It reads the solved capacitor voltage, drives the separate motor cutoff, and latches a trip until manual reset.
- The shared solver registry already includes `SparkCapacitorSolverDefinition.asset` and `Level30_MotorSolver.asset`. Keep **Simulate Transient Devices** enabled on the active solver. The scene's configured 0.05 s solve step is suitable. The controller advances the shared solver at that cadence while the timer is changing. The 0.50 s pulse is measured in accepted simulation steps.
- The motor winding is 33 Ω. A running motor should read about 150 mA and a tripped motor should read under 1 mA.
- If TIMER stays at zero during a jam, check the charge branch and that the capacitor is electrically enabled. If it stays high after clearing, check the CLEAR path, including the 1 kΩ resistor and both returns. If the motor runs while the contact is open, check for a bypass at RUN IN and RUN OUT.
- Check the wire tool's `Pin` layer mask if sockets cannot be clicked. The teaching drawing is found by the `level_33` ID in the Drawings list.

The scripts compiled against the project’s Unity references. A backward-Euler timing model passed the trip and reset checks at 0.0167 s, 0.0333 s, and the scene’s 0.05 s step. Run the generated layout in Unity to verify the actual **6/6** sequence.
