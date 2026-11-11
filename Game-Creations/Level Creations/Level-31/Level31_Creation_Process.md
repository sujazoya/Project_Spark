# Level 31 — Loop Connection

Build a START/STOP conveyor that holds its run command until STOP or a power interruption. After restoring MAIN, the conveyor must wait for a new START command.

## Generate the Unity assets

1. With Play Mode stopped, place `SparkLevel31SealIn.cs` and `SparkLevel31MomentaryButton.cs` in a runtime folder under `Assets`. Put `SparkLevel31Builder.cs` in an `Editor` folder. If these files are already installed, wait for Unity to compile and proceed to step 2.
2. Run **Tools → Project Spark → Level 31 → Create Level Assets**. The menu creates `Level_31.asset`, `Level_31_SealIn_Layout.prefab`, and the workbench materials in `Assets/My_Assets/_Project_Spark/Prefabs/Levels/Level_31_Layout/`. It takes the Level 30 bench placement as its reference, with a Level 29 fallback. A matching existing Level 31 definition is reused. The generated prefab has 17 labeled socket objects, a relay armature, a motor shaft, a moving conveyor, a RUN light, and the MAIN, START and STOP controls.
3. In the active **Level Layout Catalog**, set **Level** to `Level_31.asset` and **Prefab** to `Level_31_SealIn_Layout.prefab`. Add the same definition after Level 30 in the `LevelGamePlayManager` Levels list. If Level 30 was already completed before registering Level 31, use the game's existing test unlock or complete Level 30 again to unlock the new level.
4. Check that the active `SparkElectricalSolverRegistry` contains `SparkRelaySolverDefinition.asset` and `Level30_MotorSolver.asset`. Both were already present in MainMenu's registry when the package was prepared. Use those definitions for this level.

## Parts and settings

| Part | Settings |
|---|---|
| 5 V supply | Active, 0.35 A limit. MAIN switches the downstream circuit. |
| MAIN | Open initially. Click to toggle. |
| START | Normally open. Hold to close, release to open. |
| STOP | Normally closed. Hold to open, release to close. |
| Holding relay | Existing 100 Ω coil model. Pickup threshold 3.8 V. Use COM and NO. Leave NC spare empty. |
| Motor contact | Normally open. The controller links it to the solved relay coil state. |
| Conveyor motor | 33 Ω winding, 3 V startup, 0.25 A maximum. A is positive during normal travel. |

The relay solver closes the holding contact when its coil picks up. The Level 31 controller then closes the second, motor-power contact from the accepted relay state. The controller observes motor and coil readings for all six objectives. This relay model uses a resistive coil and omits flyback and contact bounce.

## Wire the bench in Play Mode

Start with MAIN off. Use the normal Wire tool and the workbench diagram. Switch IN is left and OUT is right. The generated parts have fixed bindings, so they do not need shelf slots.

| Wire | From | To |
|---|---|---|
| W01 | `Battery_positive` | `MAIN_IN` |
| W02 | `MAIN_OUT` | `STOP_IN` |
| W03 | `MAIN_OUT` | `RUN_IN` |
| W04 | `STOP_OUT` | `START_IN` |
| W05 | `STOP_OUT` | `Hold_COM` |
| W06 | `START_OUT` | `Coil_plus` |
| W07 | `START_OUT` | `Hold_NO` |
| W08 | `Coil_minus` | `Battery_negative` |
| W09 | `Motor_B` | `Battery_negative` |
| W10 | `RUN_OUT` | `Motor_A` |

`Hold_NC_spare` stays empty. W05 and W07 put the NO holding contact across START. W02 and W03 both start at MAIN OUT. The coil and motor return separately to battery negative. The status panel identifies a missing connection or a wire joining two paths that should remain separate.

## Complete the six objectives

Wait for the status prompt to advance at each step. Each passed condition is checked for at least 0.20 seconds.

| Objective | Player action and expected observation |
|---|---|
| 1/6 — Power-on idle | Turn MAIN on with both buttons released. The conveyor stays stopped. |
| 2/6 — START holds | Press START until the relay picks up and the conveyor moves. Release START. The relay and motor stay on. |
| 3/6 — STOP drops out | Press STOP until the conveyor stops. Release STOP. It stays stopped. |
| 4/6 — Power-loss dropout | Start again and release START. Turn MAIN off after the status requests it. The relay and motor drop out. |
| 5/6 — Safe restoration | Restore MAIN with both buttons released. The conveyor stays stopped. |
| 6/6 — Deliberate restart | Press START again and release it. The conveyor keeps running and the level completes. |

The completed objectives bind to the live 5 V source, which sits upstream of MAIN. This allows the target counter to retain progress during the outage. The layout controller awards an objective only after the corresponding solved check passes. The level manager handles completion and unlocks.

## Check before release

- Correct START hold should give about **4.99 V across the coil**, **50 mA coil current**, and **150 mA motor current**. The total should be near **200 mA**, below the 350 mA supply limit.
- With START held down, the hold objective stays pending. Release it to earn the objective.
- With MAIN off after a verified latch, coil voltage should drop near zero and motor current should fall below **1 mA**. After restoring MAIN, both remain low until START is pressed again.
- Removing W05 or W07 should identify the missing hold-loop connection. A wire across STOP should identify joined paths. Fix the wiring before running the sequence.
- A wire edit resets the test with MAIN off. Releasing START or STOP after leaving the Game window restores its normal contact state.
- If the selector panel is visible but buttons do not work, check the existing scene's EventSystem. If sockets are hard to hit, check the wire tool's terminal layer mask includes `Pin`.
- Adjust `Relay Workbench` on the generated prefab if the board needs alignment in your MainMenu scene.

The package was compiled against the project assemblies and checked with a nominal circuit model. Unity prefab generation and the full Play Mode sequence require an in-editor run.
