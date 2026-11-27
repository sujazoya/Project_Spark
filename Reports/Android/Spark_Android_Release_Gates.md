# Spark Android release gates

| Gate | Status | Requirement |
| --- | --- | --- |
| Fresh development APK | BLOCKED | The initial build failed in native compilation before packaging. Use the latest Android build and APK validation reports for the retry outcome. |
| Android installation and launch | BLOCKED | ADB had no connected device. |
| Touch, wiring, evaluation and visual readability | NOT TESTED | Pass on representative supported device hardware. |
| Microphone, offline recognition and narration | NOT TESTED | Validate permissions and the installed device speech services. The on-device recognizer requires Android API 31 or above and an available service. |
| Lifecycle and save recovery | NOT TESTED | Validate with background, screen lock, audio interruption, permission changes and restart. |
| Device performance | NOT TESTED | Measure against the chosen 30 FPS or 60 FPS device target with thermal observation. |
| Signed release APK | NOT TESTED | Produce after development device validation, using authorized release credentials. |
