# Project Spark Phase 2 test results

## Repeatable Unity requests

From the active E: project root, set the contents of `Logs/SparkWorkbench/run.request` to `flight` for the regular Level 20 replay, `cable` for the existing cable visual replay, `phase2` for motion telemetry and recovery tests, `switch` for the focused standing press, or `boot` for the isolated Bootstrap test. Queue each request from Edit Mode after verifying script compilation. The development build runner accepts `windows` or `android` in `Logs/SparkPhase2/build.request`. The Phase 2 Windows player accepts `-spark-phase2-verify <absolute report path>`.

## Incremental editor evidence

| Replay | Result | Evidence |
| --- | --- | --- |
| Reconfirmed baseline | 10/10 wires, 5/5 objectives, 13 chapter loads. Switch gaps 6.9 mm OFF and 3.2 mm ON. | `Logs/SparkFlightV13Repair/Phase2Baseline.txt` |
| Cable geometry | Passed the synthetic 10.54-unit regression and the full Level 20 visual replay. | `Logs/SparkPhase2/CableGeometryReplay.txt` |
| Articulated original hand | 10/10 wires, 5/5 objectives, both switch gaps 3.2 mm, completed all chapter smoke checks. | `Logs/SparkPhase2/HandMotionReplay.txt` |
| First complete Phase 2 fixture attempt | Recovery checks passed. The replay stopped at 8/10 when a live collision blocked wire nine's initial pickup walk. | `Logs/SparkPhase2/Phase2WireNinePickupFailure.txt` |
| Final Phase 2 editor fixture | 10/10 wires and 5/5 objectives, standing switch OFF and ON at 3.2 mm, controlled walking carry, and all 13 chapter smoke checks. | `Logs/SparkPhase2/Level20FinalEditor.txt` |

The baseline whole-Editor carry sample averaged 143.48 ms main-thread time and 122,143 bytes GC per sampled frame. The hand-stage replay averaged 178.86 ms and 122,409 bytes. The final Phase 2 capture averaged 117.21 ms and 119,227 bytes over 57 polls. These runs cover different motion and scene timing and include the full Editor. Treat them as load observations pending an isolated player profile.

The final fixture interrupted live previews by moving the source and destination, deactivating the destination, displacing Spark, switching control to Manual, entering exam mode, and energizing the virtual supply. Each run released the native preview and restored the player tool. An undersized lead and an unbound connector frame were rejected before attachment. The full fixture additionally exercised deliberate cancellation, Undo, pause/resume, obstruction, and level change.

## Live pose and cable capture

`Logs/SparkMasterAudit/NativeWireVisualRepair/phase2-motion.tsv` has hand position and orientation, aperture, extension ratio, correction magnitude, rest error, rendered-segment budget, preview drift, contact sweeps and sampled frame time. The sample at verified grip had a 46.82 mm pivot aperture for a 46.80 mm native modeled end. During the sampled controlled walk, the largest hand-to-preview gap rounded to 0.0000 units. In 11 sampled insertion stages, the wrist aim error reached 0.05 degrees against the runtime approach target and the measured segment ratio stayed at or below 1.550. These wrist values refer to the virtual endpoint approach target. Per-part mating accuracy requires authored frames and actual mesh clearance.

Gameplay recordings are `phase2-native-wire.mp4` and `phase2-standing-switch.mp4` in the same directory. The `phase2-*.png` close-ups show the original pickup, carry, alignment, insertion, release, switch and retraction. Close-up inspection showed that neighboring geometry can occlude contact from some views. Inspect two complementary angles before signing off contact clearances.

## Target compilation

The Windows and Android target Roslyn compilations both exited 0 on the final runtime revision. The target response files and `FinalRoslyn.log` files are in `Logs/SparkPhase2/WindowsScripts` and `Logs/SparkPhase2/AndroidScripts`. Use the separate build and player reports to verify the finished packages.

## Platform recovery

The first Phase 2 Windows package built with 0 errors and 765 warnings. It launched at Bootstrap and crashed while loading MainMenu, with nine scripted-object serialization-layout errors followed by a packaged level read error. The earlier Windows package reproduced the same sequence. The nine Visual Teaching lesson assets had empty script references. The correction binds each lesson to the matching `SparkVisualLesson.cs` source while preserving its content. The updated Editor startup replay passed and the packaging preflight loaded all nine typed objects with their lesson steps.

The first Android package failed in IL2CPP. The saved build report identifies `Notification.c` as containing C# that the native C compiler rejected. The managed `Notification.cs` already provides the live implementation. The unused native import and its metadata were archived under `Logs/SparkPhase2/Before/AndroidNativeImport`. Backup reports start with `BeforeLessonRepair-`, and the earlier player crash is in `WindowsPriorPlayerUnity.log`. The rebuild runner records errors from the Unity BuildReport for subsequent attempts.

## Source paths changed in Phase 2

- `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/VFX/Signal-Wires/SignalPathMesh.cs`
- `Assets/ProjectSpark/SparkV5/Pro/Integration/` files `SparkAINativeSignalWirePort.cs`, `SparkCableSimulation.cs`, `SparkNativeWireCinematic.cs`, `SparkNativeWireDirector.cs`, `SparkTelescopicArms.cs`, `SparkTelescopicNativeWireBridge.cs`, and `SparkPhase4PlayerChecks.cs`
- Added `SparkNativeConnectorContact.cs` and `SparkNativeConnectorProfile.cs` in that integration folder with their Unity metadata
- `Assets/ProjectSpark/SparkV5/Pro/Integration/Editor/` files `SparkFlightLevel20Verification.cs`, `SparkNativeCableVisualCapture.cs`, `SparkWorkbenchVerification.cs`, and `SparkPhase4Verification.cs`
- `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/Visual-Teaching-System/` files `Runtime/SparkTeachingModels.cs`, the new `Runtime/SparkVisualLesson.cs` and its metadata, and the nine `LessonLibraryLessons/*.asset` script bindings
- Archived the unused `Assets/ProjectSpark-UI System/UI/Feedback/Notification.c` native import and its metadata

## Rollback

The tracked working-tree state before Phase 2 is recorded in `Logs/SparkPhase2/Before/WorkingTree.patch` and `GitStatus.txt`. From the project root, run `git apply --reverse --check Logs/SparkPhase2/Phase2Source.patch` before a rollback. When the check passes and you want to revert the Phase 2 source changes, run `git apply --reverse Logs/SparkPhase2/Phase2Source.patch`. The scoped patch preserves the earlier wiring fixes and excludes scenes, generated knowledge, solution changes and reports.
