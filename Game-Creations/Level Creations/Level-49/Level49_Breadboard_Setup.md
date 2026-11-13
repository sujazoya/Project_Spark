# Level 49 — Cut on the Mark

Open MainMenu in Unity Edit Mode and run **Tools → Project Spark → Levels → Create Level 49**. The creator saves the layout, definition and four teaching pages, then registers the campaign and layout catalog. If MainMenu has pending edits, review and save the scene to persist its registration.

## Play the six objectives

1. Use the Wire tool and follow the exact wiring drawing for W01–W15. Both supply sockets accept three wires. Confirm that the guard shows **CLOSED**.
2. Press **HOME** and wait for the solved home input. Press **CALIBRATE**. The web measures two registration marks 160 counts apart.
3. Wait for the web to stop, choose **AUTO**, and press **START**. Verify three accepted cuts and returns to home at 30 RPM.
4. While still running at 30 RPM, press **60 RPM**. Verify three more accurate synchronized cuts.
5. Press **SPEED TEST** between cycles. Watch the web slow, then verify two accurate cuts after recovery.
6. Open the guard during motion. Verify safe stop and retained progress. Close the guard, wait for motion to stop, press **RESET FAULT**, press **HOME**, then **START**. Verify the final cut.

Manual mode runs the web for observation. It leaves automatic cutting inhibited. If the guard is open, the machine requires closed feedback before homing, calibration or restart. If wiring fails, follow the displayed W-number. Check W14 for guard feedback and W15 for home feedback.

## Implementation

The fitted assembly uses the 13 individual FBX models. The supplied visual rig animates the rail, carriage, cutter and guard. The cutter moves with the carriage. Interactive sockets are actual Spark terminals seated on the existing breadboard. Labels and TextMeshPro readouts use 0.20 X/Y scale.

Project Spark solves the 5 V rails, motor windings, guard contact and home contact. Encoder, optical and command signals travel through the validated circuit connections as level-local logic. The rail drive is an average switched motor model. Position, speed and cut accuracy come from the Unity simulation. The two existing level evaluation systems own the 6/6 result.

The roller radius is 20 mm with 80 decoded counts per revolution. The mark interval is 160 counts. Each expected cut is 80 counts downstream of the optical eye. At contact the controller interpolates carriage position against the passing mark. The tolerance is ±2 mm with a maximum 6 mm/s speed mismatch. The simulated carriage travels approximately 25 mm during the synchronized cycle.

## Inspector assignments and tuning

The creator assigns the controller's source, two motors, drive contacts, guard contact, home contact, and 26 socket references. The binding manifests list each fixed terminal and the six objective IDs. The normally closed guard contact supplies the guard input through W14. The home contact closes at the simulated home end through W15. The supplied visual rig receives only simulation state. Its head-stroke pivot is nested under the carriage pivot.

Each motor winding is configured for 33 Ω and a 180 mA overcurrent threshold. The rail acceleration is 2.8 m/s² with a 55 mm travel limit. Homing speed is 42 mm/s. The disturbance requests a temporary 34% web speed reduction and starts before the next tracking window so the player can see the carriage adapt.

| Symptom | What to check |
|---|---|
| 0/6 with the wiring present | Check the first displayed W-number, the three 5 V/return pairs and whether the guard input is solved high. |
| HOME never lights | Check W15 and drive connections W08–W10. Use HOME with the guard closed. |
| 3/6 stays visible | Press 60 RPM during a running 30 RPM cycle to record the actual transition. |
| 4/6 stays visible | Arm SPEED TEST while the carriage is ready. Wait through the dip and for two accurate cuts at recovered speed. |
| Guard closed but motion is blocked | Wait for SAFE STOP to finish, then explicitly reset and home before pressing START. |

The Play Mode report lists the checks run on the installed prefab. The test driver in the ZIP is optional and belongs in the project's Editor folder if replaying verification.
