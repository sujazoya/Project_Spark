# Spark V5 Pro in ProjectSpark

## Use Spark Live

1. Open MainMenu in Unity and enter Play Mode. Select **Spark Live** at the top right, or open **Spark Live** from the Master Panel registry.
2. Type a question and select **Send**. Select a campaign level first for questions about the workbench, live circuits, or current objectives.
3. **Hold Talk** records the current turn. Release to transcribe it. **Live** listens between replies. **Mute mic** stops capture, and **Interrupt** cancels the active turn and stops playback.
4. **Settings** lets you select the microphone, adjust the speech threshold and end silence, require “Hey Spark” at the start of voice turns, or disable spoken replies. The microphone starts off each time.
5. Try “Explain Level 120”, “Explain the current mission”, “Practice Ohm’s law”, “Continue our lesson”, or “Show me a transistor in 3D”. The projection toolbar can rotate, zoom, highlight and isolate parts, reset, and close the model. Terminal highlights are available when the model source contains authored terminals.

See [Spark Cortex](CortexImplementation.md) for natural questions, adaptive lessons, live diagnostics and Windows voice packaging.

## Setup and verification

- The project uses the existing SparkV5Pro prefab, native mesh imports, materials, face and four LODs. The MainMenu UI adapter binds the new panel to the existing scene executive. A new copy of the character is unnecessary.
- Use **Project Spark → Spark V5 Pro → Auto Configure Complete AI** when setting up another workshop scene. The setup preserves already assigned references and reports ambiguous or missing scene objects.
- **Build Live Communication Panel** generates the reusable prefab. **Register Holographic Teaching Models** publishes the curated model library. **Compile Complete Game Knowledge** rebuilds the catalog from authored definitions and the active campaign asset. Relevant asset imports, deletions and moves also trigger a rebuild.
- **Create AI Integration Test Scene** creates a separate copy of the saved MainMenu scene in the generated IntegrationTest folder. **Run AI Integration Tests** exercises the saved production scene and writes results to `Logs/SparkV5PlayMode.txt`.
- Windows speech recognition uses the existing `127.0.0.1:8766` Vosk service. Keep that service on loopback. Windows speech playback uses the existing bundled Piper model and audio source. Optional online concept explanations remain disabled until configured.
- Android needs a configured HTTPS recognition endpoint reachable from the device and an Android-compatible TTS provider. The microphone permission flow is included. Text operation can be used independently of the speech provider.

## Data and boundaries

The knowledge catalog indexes authored definitions. The campaign has 130 unique authored definitions. Level 113, The Redundancy Citadel, sits between Levels 112 and 114 and passed 6/6 in its native Play Mode acceptance run. The current chapter assets and MainMenu enroll Levels 89–93. Knowledge indexing leaves those authored registrations in place. Level 120 is indexed as The Axis Forge with six objectives. Any official qualification comes from the existing assessment and certification systems.

Spark reads live objective evidence from the loaded layout and evaluator. It uses the existing world snapshot for circuit connections and the selected object for pointing. Teaching-model geometry is copied into render-only objects. The AI's commands observe, teach or present visuals. Wiring, configuration, and assessment results remain controlled by their gameplay systems.

Dialogue history is bounded in memory. Academy keeps formative practice counts and the interrupted exercise in the existing learner profile format. Voice payloads are kept in memory during recognition. Hands-free mode alternates between recording and playback on the current backend.

## Files

| Area | Files |
| --- | --- |
| Panel and conversation | `Integration/SparkProLivePanel.cs`, `SparkProExecutive.cs`, `SparkProPushToTalkButton.cs` |
| Recognition and character | `Runtime/SparkProVoiceInput.cs`, `SparkProCharacter.cs` |
| Learning and knowledge | `Integration/SparkProAcademy.cs`, `SparkProGameCatalog.cs`, `Integration/Editor/SparkProKnowledgeCompiler.cs` |
| Projection | `Runtime/SparkProHologram.cs`, `SparkProTeachingLibrary.cs` |
| Setup and tests | `Integration/Editor/SparkProLiveBuilder.cs`, `SparkProIntegrationSetup.cs`, `SparkProIntegrationVerification.cs`, `Editor/SparkProBuilder.cs` |
| Generated assets | `Resources/SparkProLivePanel.prefab`, `Resources/SparkProTeachingLibrary.asset`, `Generated/SparkProGameCatalog.asset`, `Generated/KnowledgeAudit.txt` |
| Existing game bindings | `SparkMainMenuPanelBindings.cs`, `SparkUIMonitorDashboard.cs`, `SparkAIUnifiedAssistant.cs`, `SparkAILearningPathAssessment.cs` |
| Level evidence correction | `Level_03.asset` now has a separate ID for its safe-current objective. Electrical thresholds and target rules are the same. |

## Device checks to complete

The automated run uses Unity on Windows. Separately, the running Vosk service was sent synthesized speech from memory and returned the expected words. Complete physical microphone, wake-phrase, automatic resume, and permission checks on Windows and Android. Verify an Android speech provider and a standalone player build on the target devices. Part highlighting is supported by the shipped models. Exploded views require authored animations.


The Android SDK tools are installed in this Unity Editor. No Android device was attached during verification. The test checks authored Level 120 mission data and independent assessment restrictions. The Level 120 source disables Motherboard Master certification until its schematic and six-axis mechanical model are approved. The game mission and certification approvals have separate validation.
