# Spark V5 Pro integration audit

## Baseline from the live project

| Area | Current owner and verified connection | Work required |
| --- | --- | --- |
| Character | MainMenu contains the existing SparkV5Pro prefab instance, with SparkProCharacter, SparkProVoiceInput, four native mesh LOD sources, and a TeachingProjectionAnchor. SparkProCharacter creates its MaterialPropertyBlock in Awake. | Preserve the prefab and models. Drive character state from communication and gameplay events. |
| Conversation | SparkProExecutive references SparkAIUnifiedAssistant, SparkAIBrain, SparkSelectionController, SparkAIKnowledge and the existing game catalog. | Expose the route through the shared UI manager and give voice and text the same bounded history. |
| Speech recognition | The running loopback SpeechServer returned ready true. It accepts mono PCM16 audio/wav at 8–48 kHz via POST /transcribe and returns JSON text or error. | Add provider health monitoring, accessible controls and cancellation checks. Keep raw audio in memory. |
| Voice and face | MainMenu wires SparkAIVoiceController and SparkAITTSBridge to SparkAIPiperTTSProvider. The Executive references the provider's AudioSource. SparkVisemeScheduler already maps mouth poses against playback time. | Verify local Windows playback and handle missing providers so hands-free listening can recover. |
| Live UI | SparkProLivePanel and SparkProPushToTalkButton exist in source. MainMenu has no serialized SparkProLivePanel. SparkUIManager already provides managed modal registration. | Provide a reusable panel and bind it automatically to the existing Executive. |
| Gameplay and electrical evidence | LevelGamePlayManager and SparkLevelLayoutLoader own the active lesson. SparkAIBrain observes gameplay through SparkAIWorld. SparkAIUnifiedAssistant already exposes a verified local route. | Use the campaign, selection and evaluator sources in assistant replies. Preserve circuit authority and read-only AI actions. |
| Knowledge | KnowledgeAudit lists 129 numbered definitions and Level 113 as missing. The chapter scanner looks for fields on each level while chapter membership is authored on SparkChapterDefinition. The current chapter and MainMenu registrations include Levels 89–93. Earlier campaign snapshots deferred them. | Resolve chapters using actual asset references. Record deferred and missing content separately. |
| Learning | SparkProAcademy tracks independent and assisted practice separately. SparkAILearningPathAssessment and SparkAICourseCertification already share the practical qualification gate. | Connect the active assessment to assistant restrictions and preserve qualification records. |
| Holograms | SparkProHologram copies render meshes and materials. Its MainMenu library is serialized empty. | Register known component models and keep projections isolated from gameplay. |
| Platform | Project uses Unity 6.3, URP, uGUI, TMP and the Input System. Piper is currently compiled for Windows. The loopback Vosk service runs on this PC. | Verify Windows Play Mode. Android requires a reachable configured speech provider and device testing. |

Prepared before implementation from current source, MainMenu serialized references, the existing knowledge audit and a direct local SpeechServer health check.

