# Spark Android build fixes

## Kept from the prior repair

The duplicate `Notification.c` and its metadata were moved outside Assets. The actual managed `Notification.cs` remains. Nine repaired Visual Teaching bindings are checked before the build.

## Android change set

- Added `SparkBenchPointer.cs` to sample and own New Input System touch contacts. Single contact goes through the existing gameplay, shelf, and wire owners. Graphic UI hit testing prevents interactions behind controls. Secondary touches and viewport changes cancel the active world gesture.
- Updated `SparkGameplayController.cs`, `SparkInteractionController.cs`, `SparkShelfDragHandler.cs`, `SparkShelfPointerInteractor.cs`, `SignalWireBuilder.cs`, and `SparkWireRouteEditor.cs` to consume that Android input. Existing desktop branches continue to use their previous inputs.
- Added two-finger pan, pinch zoom, twist orbit, and Android safe-area positioning for the camera view toolbar in `SparkGameCameraRig.cs`. Full UI safe-area coverage still needs device verification.
- Added Android background cleanup for the engineering hub, the Executive's speech playback, and active world input. Existing app and microphone permission handling remain responsible for capture. Android recognition treats an unavailable activity as a recoverable failure, and the recognition and TTS callback methods explicitly survive stripping. Relevant files are `SparkEngineeringControlHub.cs`, `SparkProExecutive.cs`, and `SparkAndroidVoice.cs`.
- Bound the existing Mobile URP asset to a new Android default quality entry in `ProjectSettings/QualitySettings.asset`. High remains the project-wide default for desktop. The existing Android cable budget remains 16 segments and four iterations.
- Extended the opt-in development verification in `SparkPhase4PlayerChecks.cs` with an Android launch-intent flag. Updated `SparkPhase4Verification.cs` to accept an Android-only validation build request and record Unity errors.
- Added `Tools/SparkAndroid/Test-SparkAndroidApk.ps1` to check APK contents, native ABI, package manifest and signing.

The native wire director, registry, electrical solver, level evaluation and Engineering Control approval sequence still own their respective results.
