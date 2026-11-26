# Spark Super AI Phase 4 readiness

Phase 4 extends the existing Cortex and Spark Live in MainMenu. It uses the native level loader, evaluator, circuit solver, Academy profile, and speech controllers.

## Release gates

| Gate | Status and evidence |
| --- | --- |
| Catalog | 130 authored levels and 732 stable objectives. Level 94's existing prefab was registered in the native loader. The project data audit resolved all 130 layouts after the repair. |
| Teaching | Generated 732 source-linked objective cards, including authentic explanations, native objective whiteboards and mission practice references. Coverage audit reports 732 gaps across richer per-objective curriculum fields. Dedicated concept, misconception and assessment mappings need authoring and review. |
| Local model | Actual CPU inference ran with Qwen2.5-0.5B-Instruct Q4_K_M on llama.cpp b12676. The final six-case run matched 3/6, with 892 ms median end-to-end latency while Unity and the build were active. Semantic selection remains opt-in in the developer monitor. |
| Cloud | The gateway requires developer enablement, an explicit session consent, and a server environment credential. Unity exports the question and fixed route choices only. Cloud contract mocks passed. No credential was configured and no live cloud request ran. |
| Microphone | Unity detected one microphone. Physical speech was left unverified at the user's direction. The developer panel uses the real Spark Live capture owner. |
| Duplex | Turn-based speech and interruption. Streaming recognition and acoustic echo cancellation remain outside the tested implementation. |
| Diagnostics | Cortex checks the current layout, topology and solve version. Native source, short, return and overload results are used only when the evaluator matches the current solve. Switch state and supported reverse-voltage flags are read from native components. Unsupported readings produce a next check. |
| Practical | Independent assessment pins the original attempt, layout and objective and requires a new native solve and evaluator result. Level 113 assembly earned practical credit in a Play Mode test. Wrong mission, manual verification, assisted and stale fixture paths were rejected. |
| Demonstration | The isolated switch experiment now opens the loop, energizes the load, then disconnects the return through native circuit APIs. The active mission topology stayed unchanged. Physical player-tool placement and overlapping demonstrations were outside the tested flow. |
| Level 120 | Blocked by the unchanged authored approval record. It requires the approved RAX-28 schematic and six-axis mechanical model, plus the mandatory qualification assessment evidence. The six in-game objectives are indexed. |
| Character | Preserved the approved character and four native meshes. Speaking and interruption were exercised through Piper in the existing Executive. Physical listening and Android voice performance need hardware validation. |
| Windows | The initial development build succeeded with 0 errors and 754 warnings. Its standalone smoke found a missing Bootstrap script and timed out before MainMenu. The source now restores the existing startup script GUID. The frozen Vosk bundle passed synthesized transcription and clean shutdown. |
| Android | Added Android 12+ on-device recognition, an installed offline TTS fallback and microphone/package visibility declarations. The initial Unity ARM64 script pipeline failed on two compilation issues. After the fixes, the Unity-generated Android Roslyn compilation completed successfully. A fresh Unity Android pipeline, APK and device run are pending. ADB detected no attached device. |
| Regression and performance | Native Play Mode loaded a matching layout and evaluator in all 13 chapters. One Editor sample reported 26.74 ms on the main thread and 97,241 bytes of GC allocation. Full mission completions and device profiling remain pending. |

## Integration

Open **Project Spark → Spark V5 Pro → Cortex → Inspect Current Intelligence Context** during Play Mode. The existing panel provides evidence revisions, safe semantic opt-in, and physical microphone test controls. The Phase 4 menu runs the contract and gameplay suite, exports authored teaching references, compiles Android player scripts and builds a Windows development player. Developer routing runs only for unresolved deterministic turns. Replies must match the turn, layout, selected object, topology, solver revision and current permissions. The native Executive formats the final reply.

The kit source was reviewed and its 16 file hashes matched its manifest. All 17 supplied Python tests passed. The baseline archive includes the pre-change AI source, metadata, production MainMenu and relevant project references. Separate snapshots cover the gameplay assessor, voice bridge, layout catalog and display graph. The Phase 4 preservation check matched the approved V5 prefab, importer, character controller and four native mesh hashes to its baseline. The later tabletop work extends the character navigation API.

The Windows build exposed an editor-only Unity UI override in the graph component. Its validation method now compiles only in the Editor, using the same condition as the underlying Unity API.

## Worktable navigation

MainMenu binds Spark to the existing 15.79 × 8.54 unit central table collider. The navigation surface is built when a wire is confirmed, with raised level colliders excluded from Spark's walking path. Spark approaches both terminal positions before the player Wire Tool commits the cable. That tool and the circuit system remain responsible for the wire and electrical link.

Unity Play Mode confirmed Level 1 terminal travel, cancellation, a native visual and electrical cable, undo, independent examination restrictions, tabletop obstruction, and cancellation on loading Level 2. Other tools and campaign-wide tabletop paths remain outside this verification.

- MainMenu scene binding and duplicate prefab component removal in `Assets/My_Assets/_Project_Spark/Scenes/MainMenu/MainMenu.unity` and `Assets/ProjectSpark/SparkV5/Pro/Generated/Spark AI.prefab`
- Navigation and native action changes in `SparkWorkbenchNavigation.cs`, `SparkNativeWireDirector.cs`, `SparkNativeWireAutoEngineer.cs` and `../Runtime/SparkProCharacter.cs`
- Play Mode verification in `Editor/SparkWorkbenchVerification.cs` with results in `Logs/SparkWorkbenchVerification.txt`
- Restored player entry script and original metadata in `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Bootstrap.cs` and its `.meta`

## Evidence and files

- Final log under Logs/SparkSuperAIFinalVerification.txt
- Kit tests, authored export and coverage under Logs/SparkPhase4
- Actual model inference and earlier prompt trials under Logs/SparkPhase4/LocalInference*.txt
- Baseline Cortex tests under Logs/SparkCortexVerification.txt and speech package verification under Logs/SparkSpeechPackageVerification.txt
- Exact source and asset list under Logs/SparkPhase4/ChangedFiles.txt

Provider contracts include [llama.cpp server](https://github.com/ggml-org/llama.cpp/blob/master/tools/server/README.md), [Qwen model](https://huggingface.co/Qwen/Qwen2.5-0.5B-Instruct-GGUF), [Ollama chat](https://docs.ollama.com/api/chat), and [OpenAI Responses](https://developers.openai.com/api/reference/resources/responses/methods/create).
Android API references include [SpeechRecognizer](https://developer.android.com/reference/android/speech/SpeechRecognizer) and [TextToSpeech](https://developer.android.com/reference/android/speech/tts/TextToSpeech).

## Phase 4 files

- modified `Assets/My_Assets/Folder Generator/CreateAAAUIFolders.cs`
- modified `Assets/My_Assets/_Project_Spark/Scenes/MainMenu/MainMenu.unity`
- modified `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/Visual-Teaching-System/Runtime/AILearningPathAssessment/SparkAIAssessmentBank.cs`
- modified `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/Visual-Teaching-System/Runtime/AILearningPathAssessment/SparkAILearningPathAssessment.cs`
- modified `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/Voice/SparkAITTSBridge.cs`
- modified `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Display/Graph/SparkDisplayGraph.cs`
- modified `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Levels/LevelGamePlayManager.cs`
- modified `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Levels/LevelLayoutCatalog/SparkLevelLayoutCatalog.asset`
- modified `Assets/ProjectSpark-UI System/UI/Animation/UIAnimator/UIAnimator.cs`
- created `Assets/ProjectSpark/SparkV5/Pro/Generated/AuthoredObjectiveCards.json`
- created `Assets/ProjectSpark/SparkV5/Pro/Generated/AuthoredObjectiveCards.json.meta`
- modified `Assets/ProjectSpark/SparkV5/Pro/Generated/SparkProGameCatalog.asset`
- created `Assets/ProjectSpark/SparkV5/Pro/Integration/Editor/SparkAndroidVoiceManifest.cs`
- created `Assets/ProjectSpark/SparkV5/Pro/Integration/Editor/SparkAndroidVoiceManifest.cs.meta`
- modified `Assets/ProjectSpark/SparkV5/Pro/Integration/Editor/SparkCortexVerification.cs`
- created `Assets/ProjectSpark/SparkV5/Pro/Integration/Editor/SparkPhase4TeachingExport.cs`
- created `Assets/ProjectSpark/SparkV5/Pro/Integration/Editor/SparkPhase4TeachingExport.cs.meta`
- created `Assets/ProjectSpark/SparkV5/Pro/Integration/Editor/SparkPhase4Verification.cs`
- created `Assets/ProjectSpark/SparkV5/Pro/Integration/Editor/SparkPhase4Verification.cs.meta`
- modified `Assets/ProjectSpark/SparkV5/Pro/Integration/Editor/SparkProLiveBuilder.cs`
- modified `Assets/ProjectSpark/SparkV5/Pro/Integration/SparkCortex.cs`
- created `Assets/ProjectSpark/SparkV5/Pro/Integration/SparkPhase4EvidenceGuard.cs`
- created `Assets/ProjectSpark/SparkV5/Pro/Integration/SparkPhase4EvidenceGuard.cs.meta`
- created `Assets/ProjectSpark/SparkV5/Pro/Integration/SparkPhase4PlayerChecks.cs`
- created `Assets/ProjectSpark/SparkV5/Pro/Integration/SparkPhase4PlayerChecks.cs.meta`
- created `Assets/ProjectSpark/SparkV5/Pro/Integration/SparkPhase4RuntimeChecks.cs`
- created `Assets/ProjectSpark/SparkV5/Pro/Integration/SparkPhase4RuntimeChecks.cs.meta`
- created `Assets/ProjectSpark/SparkV5/Pro/Integration/SparkPhase4SemanticRouter.cs`
- created `Assets/ProjectSpark/SparkV5/Pro/Integration/SparkPhase4SemanticRouter.cs.meta`
- created `Assets/ProjectSpark/SparkV5/Pro/Integration/SparkPhase4TeachingContract.cs`
- created `Assets/ProjectSpark/SparkV5/Pro/Integration/SparkPhase4TeachingContract.cs.meta`
- modified `Assets/ProjectSpark/SparkV5/Pro/Integration/SparkProAcademy.cs`
- modified `Assets/ProjectSpark/SparkV5/Pro/Integration/SparkProAssessmentGate.cs`
- modified `Assets/ProjectSpark/SparkV5/Pro/Integration/SparkProDiagnostics.cs`
- modified `Assets/ProjectSpark/SparkV5/Pro/Integration/SparkProExecutive.cs`
- modified `Assets/ProjectSpark/SparkV5/Pro/Integration/SparkProLabCircuit.cs`
- modified `Assets/ProjectSpark/SparkV5/Pro/Integration/SparkProLivePanel.cs`
- modified `Assets/ProjectSpark/SparkV5/Pro/Resources/SparkProTeachingLibrary.asset`
- created `Assets/ProjectSpark/SparkV5/Pro/Runtime/SparkAndroidVoice.cs`
- created `Assets/ProjectSpark/SparkV5/Pro/Runtime/SparkAndroidVoice.cs.meta`
- modified `Assets/ProjectSpark/SparkV5/Pro/Runtime/SparkProVoiceInput.cs`
- created `Python/spark_semantic_gateway.py`
- created `Python/verify_phase4_local_semantic.py`
- created `Assets/ProjectSpark/SparkV5/Pro/Integration/SuperAIFinalReadiness.md`
