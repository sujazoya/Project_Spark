# Native wiring verification

## Baseline MainMenu

The stock Level 20 fixture reproduced the physical reach failure on `SwitchOutput`. It measured a 1.295-unit grip gap and stopped before committing the tested wire. The baseline also confirmed the existing manual ownership guard, exam and energized-supply rejection, obstruction rejection, and preview removal after cancellation. See `Logs/SparkFlightV13Repair/NativeWiringBaseline.txt`.

## Binding and first connection

The updated fixture confirmed that `SwitchOutput` resolves immediately while `R_LTOP_Input` stays unpopulated before shelf installation. It then installed the four resistors through the existing native shelf and resolved all ten authored pairs. The first reviewed wire travelled through the native Wire Tool and appeared in the live electrical graph and visual registry. A post-fix run measured 25 route points and a 1.226-unit route over a 1.160-unit terminal span. The fixture also confirmed Undo and pause/resume before starting the sequence.

## Authored Level 20

The final replay registered **10/10 approved native wires** and reached **5/5 evaluated objectives** with the circuit powered through the approved device actions. It verified the native registry and renderer for every wire, original-hand source contact and retraction, Undo, and cancellation on a level change. The first wire used 25 points and measured 1.228 units over a 1.160-unit span. The replay recorded 49 walking-carry polls with a maximum grip-to-preview gap of 0.0000 units at report precision.

The separate walking-carry fixture limited reach for the temporary Play Mode setup so the sockets could not be handled from one stance. It passed with 18 measured hand samples, no flight approaches and a maximum grip-to-preview gap of 0.0000 units at report precision. The fixture also passed the engineering UI and chapter-load smoke checks.

The sampled Editor counters averaged 191.96 ms main-thread time and 128,338 bytes of frame GC allocation over 103 carry polls. These are whole-Editor counters, including the scene, fixture and editor overhead. They do not isolate the cable solver and do not establish player performance.

Evidence: `Logs/SparkFlightV13Repair/Level20FullPass.txt`.

## Standing switch in Level 20

The focused Play Mode replay verified OFF and ON through the approved device commands and fresh native solver readback. Both changes took place with Spark stationary on the workbench and measured a 0.0032 m index-to-lever gap. The original arm returned to rest after each press. The Windows and Android player script compilers also passed after the standing-press change. Evidence is in `Logs/SparkFlightV13Repair/StandingSwitch.txt` and `StandingSwitchRoslyn.log` in each player compiler output folder.

The cable visual replay after the alternate pickup stance change also passed both switch directions at a 0.0032 m gap. It registered all 10 wires, passed 5/5 powered objectives and passed the controlled walking-carry fixture. See `Logs/SparkMasterAudit/NativeWireVisualRepair/cable.txt`. The Windows and Android target script compilers exited 0 on this revision. See `StandingSwitchFinalRoslyn.log` in each player compiler output folder.

The regular replay independently passed 10/10 native wires and 5/5 powered objectives. The measured standing switch gaps were 0.0032 m for OFF and 0.0049 m for ON. See `Logs/SparkFlightV13Repair/Verification.txt`.

## Bootstrap

The old load guard reproduced the MainMenu startup error. The isolated Play Mode request started at `Bootstrap.unity`, found MainMenu at index 1 and loaded it successfully. See `Logs/SparkWorkbenchVerification.txt`.

## Player targets

The Android ARM64 script compiler produced a fresh `Assembly-CSharp.dll` and recorded PASS in `Logs/SparkPhase4/AndroidCompile.txt`. After the carry adjustments, Unity's Roslyn compiler also compiled the current gameplay sources with the Windows and Android player response files, both with exit code 0. These were script compilation checks. Output and logs are under `Logs/SparkNativeWiring_Windows_Compile` and `Logs/SparkNativeWiring_Android_Compile`. Android device behavior and Windows player runtime performance are outside this result.

The full Windows development build completed with 0 errors and 764 warnings. Its 1.88 GB output is at `Logs/SparkPhase4/WindowsPlayer/ProjectSpark.exe`. The build began before the final carry adjustment. The separate Windows-target Roslyn check covers the later gameplay sources. The build log is `Logs/SparkSuperAIFinalVerification.txt`.
