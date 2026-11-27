# Spark Android build audit

| Area | Status | Evidence |
| --- | --- | --- |
| Active workspace | PASS | The active Unity editor process uses the E: project. The D: project version file was absent. Unity reports 6000.3.19f1. |
| Architecture and backend | PASS | The Android project settings select ARM64 only and IL2CPP. Build packaging is configured through Unity and Gradle. |
| Android-target managed compilation | PASS | Unity's bundled Roslyn compiled the Android sources using Android player response-file references. Results are saved under `Logs/SparkAndroid/Managed`. |
| Toolchain inventory | PASS | Unity's Android module provides SDK platforms 34, 35 and 36, build-tools 36.0.0, bundled OpenJDK, and NDK 27.2.12479018. The prior retry log recorded a fallback from requested SDK 37 to installed SDK 36. Requested and packaged target APIs must be compared using the final APK. |
| Original native compilation | FAIL | The saved `Logs/SparkPhase2/BeforeLessonRepair-Android.buildreport` records Clang treating C# from `Notification.c` as native C. |
| Incorrect source import | PASS | A scan of active `.c` and `.cpp` files found no additional C# imports matching the original signature. The managed `Notification.cs` implementation remains in Assets. The original accidental import is archived in `Logs/SparkPhase2/Before/AndroidNativeImport`. |
| Teaching asset binding | PASS | The existing Android build preflight loaded nine typed Visual Teaching lessons and their authored steps. |
| APK validator negative case | PASS | The validator rejected an empty test archive with a missing-manifest error. Evidence is in `Logs/SparkAndroid/ValidatorNegativeCase.txt`. |
| Release signing | NOT TESTED | The requested artifact is a development APK. Release credentials and keystores were left untouched. |

MainMenu binds the New Input System UI module. The gameplay action asset binds pointer and primary actions to a mouse. The scene also binds the legacy workbench interaction controller and the native wire builder. Android input needed a route through those existing owners. The installed Android speech integration uses the platform recognizer and TTS, with microphone permission and recognition/TTS service queries added during Gradle project generation.
