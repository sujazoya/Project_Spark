# Spark Android functionality tests

| Subsystem | Status | Evidence or next executable check |
| --- | --- | --- |
| Connected Android device | BLOCKED | Bundled ADB returned an empty device inventory during this session. |
| Install and cold launch | NOT TESTED | Requires an available supported Android device and the validated development APK. |
| Bootstrap, MainMenu, chapters and Level 20 | NOT TESTED | The Phase 2 Editor evidence is a regression baseline. The Android development player can write a separate runtime report. |
| Engineering Control, Spark AI and native hand wiring | NOT TESTED | Run the Level 20 development verification in the Android player. It loads the authored plan and uses reviewed Engineering Control commands and the native builder. |
| Native topology, target evaluation and switch | NOT TESTED | The development run checks ten authored connections and 5/5 powered targets. Multimeter, LED, removal and short-circuit scenarios still require separate device tests. |
| Touch selection, taps, long press and drag | NOT TESTED | Use the Select and Wire tools. Tap and hold a shelf part, drag to its slot, build and remove a native wire, test UI overlays and a second finger. |
| Camera, scrolling, buttons and safe area | NOT TESTED | Verify pan, pinch and twist, both landscape directions, allowed portrait orientations, and touches across the notch and toolbar on the device. |
| Voice, denied permission and playback | NOT TESTED | Test permission prompts, deny and retry, on-device recognition where available, push-to-talk, command dispatch, platform TTS and interruption. |
| Lifecycle, saving and recovery | NOT TESTED | Background and lock the device mid-wire and mid-voice. Resume, confirm only valid wires remain, retry, then verify level transitions and saved progress. |

## Android development-device commands

Use ADB with the package and launchable activity printed by the APK validator. For a regular launch, omit the verification extra. For the Level 20 automated check, add `--ez spark.phase2.verify true` to `adb shell am start -n PACKAGE/ACTIVITY`. The development player writes `SparkAndroidLevel20.txt` under its application-specific files directory. Pull the report and retain `adb logcat` for failures. Clear the old report before comparing repeat runs.
