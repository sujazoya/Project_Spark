# Level 48 — Hit the Mark

## Install and inspect

The package uses the Project Spark Unity 6000.3 project. Merge its `Assets` folder into the project with the supplied metadata files. In Edit Mode open MainMenu and choose **Tools → Project Spark → Levels → Create Level 48**. The creator fits the saved assembly, validates terminal bindings, creates the teaching drawing and registers Level 48 in the campaign and layout catalog. If the scene is already dirty, save MainMenu after reviewing the pending scene changes.

The layout is `Level_48_Layout.prefab` and the definition is `Level_48.asset`. The fitted FBX combines the original 12 individual art assets into named modules with independent moving groups. It omits the decorative socket meshes where Unity places actual wire terminals. Each selectable terminal has a separate breadboard hole. World labels and the two live TextMeshPro readouts use 0.20 local X/Y scale.

## Play through the six objectives

1. Use the Wire tool and connect W01–W13 as shown in the wiring drawing. The +5 V and return sockets each take three wires. Progress should show 1/6 with all three powered modules connected.
2. Press **CALIBRATE**. The web crawls at 18 RPM and measures two genuine marks 160 encoder counts apart. Wait for 2/6 and for the web to stop.
3. Select **MODE: AUTO** and press **START** at 30 RPM. The quality display should show three accepted prints and 3/6.
4. Select **60 RPM** while the web is still running at 30 RPM. Watch the ramp and verify three more prints within ±2 mm. The manager should show 4/6.
5. Press **GLARE TEST** once. The controller rejects an extra pulse that arrives before the expected mark. A genuine print then advances progress to 5/6.
6. Press **MISSING MARK**. Wait for safe stop and verify that the head remains inhibited and that 5/6 stays recorded. Press **RESTORE EYE**, wait for **RESET FAULT**, then press **START**. The first accurate recovery print completes the level.

**MANUAL** runs the web without registering or stamping. **STOP** opens both drive contacts. **RESTART** clears Level 48's calibration, results and objective history while retaining the player wires. Use the standard wire controls to undo or clear wiring.

## Electrical and mechanical implementation

- The source, motor windings, switching contacts and powered input loads are solved by the existing Project Spark electrical solver. The web transport motor has an internal supply branch. The print motor uses the player-wired driver supply and return and its two player motor leads.
- The level routes digital encoder A/B, mark, index and print-trigger logic through the validated wire graph. Individual logic edges are processed in fixed simulation steps. The index output enables the print drive during the powered stroke. The level uses a switched on/off drive model.
- The calibrated distance scale is 80 decoded counts per revolution on a 20 mm radius roller. A mark crosses the eye every 160 counts and the head is 80 counts downstream. The controller predicts the 75 ms powered stroke using encoder position and web velocity, interpolates the contact point within the simulation step, and measures signed registration error against the target. The head returns home over 95 ms.
- The inspection camera indicates a pass from the measured stamp position. Image recognition is outside the implementation.
- The six runtime component bindings reflect achieved milestones. The existing manager evaluates all six targets and owns campaign completion.

## Diagnostics

| Display | Check |
|---|---|
| Connect W01–W13 | Use the exact wire number and terminal IDs shown. An incorrectly routed wire can make a socket appear connected while the required pair is missing. |
| Waiting for solved 5 V | Check supply power and all three power/return pairs. Remove any wire bridging separate signal or power nets. |
| 2/6 | Wait for calibration motion to stop, then start in AUTO at 30 RPM. |
| 3/6 | Change from 30 RPM to 60 RPM during the same run so the controller can observe the speed transition. |
| SAFE STOP | Check the reason shown. Restore the eye or repair the indicated connection, allow motion to stop, then explicitly reset. |
| 5/6 | Clearing the obstruction and resetting leave the machine stopped. Press START and wait for an accurate print. |

See the Play Mode report for the checks actually exercised in the project.
