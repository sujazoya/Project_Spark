# Project Spark — Electrical Architecture Audit

Generated: 2026-11-01 06:19:36 UTC
Project: ebm
C# files inspected: 766
Relevant source files: 389

**Interpretation:** DECLARED IN SOURCE means a class/type exists in scanned Assets C#; it does NOT prove that a component is in a loaded scene, connected to a solver, or functioning. 'Not found' does not mean the class is missing at runtime; DLLs, Packages, and generated sources may not be included.

## Electrical capability evidence

| Capability | Static result | Evidence | 
|---|---|---|
| Electrical component base | DECLARED IN SOURCE | Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Engineering/SparkElectricalComponent.cs |
| Solved component state | NOT FOUND IN SCANNED SOURCE | SparkElectricalState may be in a compiled assembly, package, or unscanned source. |
| Resistor | DECLARED IN SOURCE | Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Engineering/Components/SparkResistor.cs |
| Resistor thermal / burn | DECLARED IN SOURCE | Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Gameplay/BurnSystem/SparkResistorBurn.cs |
| LED | DECLARED IN SOURCE | Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Engineering/Components/SparkLED.cs |
| Switch | DECLARED IN SOURCE | Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Engineering/Components/SparkSwitch.cs |
| Power supply | DECLARED IN SOURCE | Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Engineering/Components/SparkPowerSupply.cs |
| Level gameplay evaluator | DECLARED IN SOURCE | Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Levels/LevelGamePlayManager.cs |
| Verified frame data | DECLARED IN SOURCE | Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/Visual-Teaching-System/Runtime/SparkVerifiedElectricalFrame.cs |
| Verified frame publisher | DECLARED IN SOURCE | Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/Visual-Teaching-System/Runtime/SparkVerifiedElectricalPublisher.cs |
| Telemetry synchronizer | DECLARED IN SOURCE | Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/Visual-Teaching-System/Runtime/SparkSolverTelemetrySync.cs |
| Virtual multimeter | DECLARED IN SOURCE | Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/SparkVirtualMultimeter/SparkVirtualMultimeter.cs |
| Experiment lab | DECLARED IN SOURCE | Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/Visual-Teaching-System/Runtime/CircuitExperimentLab/SparkCircuitExperimentLab.cs |
| Node potential feed | DECLARED IN SOURCE | Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/SparkVirtualMultimeter/SparkMeterVerifiedNodeFeed.cs |
| External completed-frame publish call | NO EXTERNAL CALL SITES FOUND | A solver integration may use an event, reflection, DLL or different handoff. Check Play Mode. |
| External node-potential publish call | NO EXTERNAL CALL SITES FOUND | A solver integration may use an event, reflection, DLL or different handoff. Check Play Mode. |
| Electrical solver source candidates | CANDIDATES FOUND | Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Electrical/SparkElectricalSolver.cs; Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Electrical/SolverFramework/SparkElectricalSolverDefinition.cs; Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Electrical/SolverFramework/SparkElectricalSolveResult.cs; Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Electrical/SolverFramework/SparkElectricalSolverRegistry.cs; Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Electrical/Solvers/SparkMultimeterElectricalSolverDefinition.cs; Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Electrical/Solvers/SparkWireBranchCurrentResolver.cs |

## Components in currently loaded scenes

Edit Mode inventory only; disabled children are counted. Other scenes and prefab assets are not loaded or inspected.

| Component | Instance count |
|---|---:|
| LevelGamePlayManager | 1 |
| SparkAIAdaptiveTeachingController | 1 |
| SparkAIAdaptiveTeachingPlanner | 1 |
| SparkAIAdaptiveTroubleshooter | 1 |
| SparkAIAnimatedAnswerPlayer | 1 |
| SparkAIBrain | 1 |
| SparkAIChallengeController | 1 |
| SparkAIChallengeEvaluator | 1 |
| SparkAIChallengeFeedback | 1 |
| SparkAIChallengeGenerator | 1 |
| SparkAIChallengeSession | 1 |
| SparkAICircuitAnalyzer | 1 |
| SparkAICircuitDiagnosis | 1 |
| SparkAICircuitPathAnalyzer | 1 |
| SparkAICircuitReasoner | 1 |
| SparkAIComponentInspector | 1 |
| SparkAIComponentReasoner | 1 |
| SparkAIComponentVisualLessonPlayer | 1 |
| SparkAIConceptChallengePlanner | 1 |
| SparkAIConceptMistakeAnalyzer | 1 |
| SparkAIConceptProgression | 1 |
| SparkAIConceptProgressionPlanner | 1 |
| SparkAIContextReasoner | 1 |
| SparkAIConversation | 1 |
| SparkAICurrentObjectiveProbe | 1 |
| SparkAIDirector | 2 |
| SparkAIElectricalReasoner | 1 |
| SparkAIExplanationAssistantConnector | 1 |
| SparkAIExplanationRequestBridge | 1 |
| SparkAIGameplayBridge | 1 |
| SparkAIGuidedLearningPaths | 1 |
| SparkAIGuidedRepairMissions | 1 |
| SparkAIIntent | 1 |
| SparkAIInteractiveTrainingChallenges | 1 |
| SparkAILearnerModel | 1 |
| SparkAILearning | 1 |
| SparkAILearningCoach | 1 |
| SparkAILearningHoverHint | 34 |
| SparkAILearningPanel | 1 |
| SparkAILearningPanel2UX | 1 |
| SparkAILearningPathAssessment | 1 |
| SparkAILesson | 1 |
| SparkAILessonChallengeBridge | 1 |
| SparkAILessonChallengeResolver | 1 |
| SparkAILessonLibrary | 1 |
| SparkAILessonTeachingBridge | 1 |
| SparkAILevelChallengeBridge | 1 |
| SparkAILevelLessonBridge | 2 |
| SparkAIMemory | 1 |
| SparkAIMistakeDetector | 1 |
| SparkAIObservationCoordinator | 1 |
| SparkAIPiperTTSProvider | 1 |
| SparkAIPlayerActivity | 1 |
| SparkAIPlayerResponseObserver | 1 |
| SparkAIQuestionController | 1 |
| SparkAIQuestionUI | 1 |
| SparkAIReasoner | 1 |
| SparkAIStudentProgressCenter | 1 |
| SparkAITTSBridge | 1 |
| SparkAITeacherController | 1 |
| SparkAITeachingReasoner | 1 |
| SparkAITeachingSession | 1 |
| SparkAITeachingSessionSaveResume | 1 |
| SparkAITrainingAnalytics | 1 |
| SparkAITrainingDashboard | 1 |
| SparkAIUnifiedAssistant | 1 |
| SparkAIUnifiedAssistantTransport | 1 |
| SparkAIUnifiedTutorialHub | 1 |
| SparkAIUnityEventExplanationTransport | 1 |
| SparkAIVoiceController | 1 |
| SparkAIWorld | 1 |
| SparkAIWorldContext | 1 |
| SparkAdaptiveExplanationEngine | 1 |
| SparkAdaptiveExplanationPanel | 1 |
| SparkAdvancedDisplay | 1 |
| SparkAudioVisualizer | 1 |
| SparkBattery | 1 |
| SparkBatteryCharging | 1 |
| SparkBatteryDisplay | 2 |
| SparkChapterAccessController | 1 |
| SparkCharacterAnimationController | 2 |
| SparkCircuitExperimentLab | 1 |
| SparkCircuitSystem | 1 |
| SparkComponentCurrentFeedback | 11 |
| SparkCurrentFeedbackBridge | 1 |
| SparkDisplayAlarm | 5 |
| SparkDisplayBar | 1 |
| SparkDisplayGraph | 1 |
| SparkDisplayRange | 1 |
| SparkDisplaySignalQuality | 1 |
| SparkDisplayStatusRenderer | 1 |
| SparkDisplayTestHarness | 1 |
| SparkDisplayTextAnimator | 1 |
| SparkDisplayValueRenderer | 7 |
| SparkEconomyService | 1 |
| SparkElectricalFeedbackSystem | 1 |
| SparkElectricalSafetyLimits | 1 |
| SparkElectricalSolver | 1 |
| SparkElectricalTelemetryBridge | 1 |
| SparkGameplayController | 1 |
| SparkHairDryerControlInteraction | 3 |
| SparkHairDryerElectricalController | 1 |
| SparkHairDryerHeaterElectrical | 1 |
| SparkHairDryerHeaterVisual | 2 |
| SparkHairDryerMotorElectrical | 1 |
| SparkHairDryerThermalProtectionVisual | 2 |
| SparkHairDryerVisualController | 1 |
| SparkInspectTool | 1 |
| SparkInteractable | 15 |
| SparkInteractionController | 1 |
| SparkJackSocket | 2 |
| SparkLED | 3 |
| SparkLevelLayout | 1 |
| SparkLevelLayoutLoader | 1 |
| SparkLevelMonitor | 1 |
| SparkLevelMonitorTheme | 1 |
| SparkLevelProgression | 1 |
| SparkLevelSceneBindings | 2 |
| SparkLevelShelfController | 1 |
| SparkManualShelfPlacement | 1 |
| SparkMeasureTool | 1 |
| SparkMeasurementSystem | 1 |
| SparkMonitorScanSequence | 1 |
| SparkMoveTool | 1 |
| SparkMultimeter | 1 |
| SparkMultimeterAnimation | 1 |
| SparkMultimeterButtons | 1 |
| SparkMultimeterDisplay | 1 |
| SparkMultimeterElectricalComponent | 1 |
| SparkMultimeterProbe | 2 |
| SparkObjectShelf | 1 |
| SparkObjectVisibilityController | 2 |
| SparkPlug | 2 |
| SparkPowerCable | 1 |
| SparkPowerSupply | 3 |
| SparkPurchasedComponentSpawner | 1 |
| SparkResistor | 9 |
| SparkResistorBurn | 9 |
| SparkRotateTool | 1 |
| SparkRotationGizmo | 1 |
| SparkScanOverlay | 1 |
| SparkScanSystem | 1 |
| SparkScanTool | 1 |
| SparkSceneReset | 1 |
| SparkSchematicRenderer | 1 |
| SparkSelectTool | 1 |
| SparkSelectable | 23 |
| SparkSelectionController | 1 |
| SparkShelfDragHandler | 1 |
| SparkShelfItem | 9 |
| SparkShelfSlot | 12 |
| SparkSingleWire | 6 |
| SparkSmartMove | 14 |
| SparkSolverTelemetrySync | 1 |
| SparkSwitch | 2 |
| SparkSwitchIndex | 2 |
| SparkSwitchVisual | 3 |
| SparkTMPTypewriter | 2 |
| SparkTeachingAnchor | 1 |
| SparkTeachingIntegrationAdapter | 1 |
| SparkTeachingVectorGraphic | 2 |
| SparkTemporaryObject | 4 |
| SparkTerminal | 72 |
| SparkTerminalIndicator | 22 |
| SparkToolController | 1 |
| SparkTutorialCinematicGuide | 1 |
| SparkTutorialCircuitAwareness | 1 |
| SparkTutorialDirector | 1 |
| SparkTutorialGameplaySignals | 1 |
| SparkTutorialGhostDemonstrator | 1 |
| SparkTutorialLevelBridge | 1 |
| SparkTutorialMistakeCoach | 1 |
| SparkTutorialResistorStateBridge | 1 |
| SparkTutorialStarter | 2 |
| SparkTutorialTarget | 5 |
| SparkUIAnimToggleButton | 1 |
| SparkUIAnimationController | 1 |
| SparkUIFade | 5 |
| SparkVFXController | 3 |
| SparkVerifiedElectricalPublisher | 1 |
| SparkVirtualMultimeter | 1 |
| SparkVisualTeachingLiveBridge | 1 |
| SparkVisualTeachingManager | 1 |
| SparkWireContact | 12 |
| SparkWireCurrentSolveBridge | 1 |
| SparkWireCurrentVisualPublisher | 1 |
| SparkWireSystem | 1 |
| SparkWireTool | 1 |
| SparkWorldAnnotationRenderer | 1 |

## Source file inventory

| Category | Source path | Declared types |
|---|---|---|
| Electrical components / state | `Assets/Demo/ProjectSpark_FullBurnDemo/ProjectSpark_CinematicBurn/SparkBurnCinematicVFX.cs` | SparkBurnCinematicVFX |
| Electrical components / state | `Assets/Demo/ProjectSpark_FullBurnDemo/SparkFullBurnVFXDemo.cs` | SparkFullBurnVFXDemo |
| Workbench instruments | `Assets/My_Assets/Models/Multimeter_/MultimeterCableMesh.cs` | MultimeterCableMesh, CableQuality, UpdateMode, EndpointDirectionMode |
| Teaching / UI consumer | `Assets/My_Assets/_Project_Spark/Levels/Level_04/Scripts/SparkHairDryerHeaterElectrical.cs` | SparkHairDryerHeaterElectrical |
| Electrical components / state | `Assets/My_Assets/_Project_Spark/Levels/Level_04/Scripts/SparkHairDryerMotorElectrical.cs` | SparkHairDryerMotorElectrical |
| Electrical components / state | `Assets/My_Assets/_Project_Spark/Prefabs/Electronic Components/Registers/Editor/SparkResistorBurnEditor.cs` | SparkResistorBurnEditor |
| Other Project Spark source | `Assets/My_Assets/_Project_Spark/Prefabs/Levels/Level_06_Layout/SparkLevel6CurrentDisplay.cs` | SparkLevel6CurrentDisplay |
| Teaching / UI consumer | `Assets/My_Assets/_Project_Spark/Prefabs/Smoke-Fire-blast/Editor/SparkFailureParticleSetupEditor.cs` | SparkFailureParticleSetupEditor |
| Teaching / UI consumer | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/Challenge/SparkAIChallengeController.cs` | SparkAIChallengeController |
| Teaching / UI consumer | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/Challenge/SparkAIChallengeDefinition.cs` | SparkAIChallengeDefinition |
| Teaching / UI consumer | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/Challenge/SparkAIChallengeEvaluator.cs` | SparkAIChallengeEvaluator |
| Teaching / UI consumer | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/Challenge/SparkAIChallengeFactory.cs` | SparkAIChallengeFactory |
| Teaching / UI consumer | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/Challenge/SparkAIChallengeFeedback.cs` | SparkAIChallengeFeedback |
| Teaching / UI consumer | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/Challenge/SparkAIChallengeGenerator.cs` | SparkAIChallengeGenerator |
| Teaching / UI consumer | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/Challenge/SparkAIChallengeSession.cs` | SparkAIChallengeSession, SparkAIChallengeSessionState |
| Teaching / UI consumer | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/Challenge/SparkAILessonChallenge.cs` | SparkAIChallengeDifficulty |
| Teaching / UI consumer | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/Challenge/SparkAILessonChallengeBridge.cs` | SparkAILessonChallengeBridge |
| Solver / topology candidate | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/Challenge/SparkAILessonChallengeResolver.cs` | SparkAILessonChallengeResolver |
| Teaching / UI consumer | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/Challenge/SparkAILevelChallengeBridge.cs` | SparkAILevelChallengeBridge |
| Teaching / UI consumer | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/Challenge/SparkAILevelLessonBridge.cs` | SparkAILevelLessonBridge |
| Other Project Spark source | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/Character/Character/SparkLipSyncAddon.cs` | SparkLipSyncAddon, CurveChannel, PoseData |
| Other Project Spark source | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/Character/Character/SparkVisemeScheduler.cs` | SparkVisemeScheduler, Pose, Cue |
| Teaching / UI consumer | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/Curriculum/SparkAIConceptChallengePlanner.cs` | SparkAIConceptChallengePlanner |
| Teaching / UI consumer | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/Curriculum/SparkAIConceptDefinition.cs` | SparkAIConceptDefinition, SparkAIVoiceEmotion |
| Teaching / UI consumer | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/Curriculum/SparkAIConceptProgression.cs` | SparkAIConceptProgression |
| Teaching / UI consumer | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/Curriculum/SparkAIConceptProgressionPlanner.cs` | SparkAIConceptProgressionPlanner |
| Teaching / UI consumer | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/Curriculum/SparkAICurriculum.cs` | SparkAICurriculum |
| Teaching / UI consumer | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/Database/SparkAIChallengeDatabase.cs` | SparkAIChallengeDatabase |
| Teaching / UI consumer | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/Database/SparkAIConceptDatabase.cs` | SparkAIConceptDatabase |
| Teaching / UI consumer | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/Database/SparkAICurriculumDefinition.cs` | SparkAICurriculumDefinition, SparkAICurriculumLevel |
| Electrical components / state | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/Database/SparkAIKnowledge.cs` | SparkAIKnowledge, Entry |
| Electrical components / state | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/Editor/parkAIKnowledgeLevel1Editor.cs` | SparkAIKnowledgeLevel1Editor |
| Teaching / UI consumer | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/Learning/SparkAILessonTeachingBridge.cs` | SparkAILessonTeachingBridge |
| Teaching / UI consumer | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/Learning/SparkAITeacherController.cs` | SparkAITeacherController |
| Teaching / UI consumer | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/LipSynk With Model/SparkAIFacialAnimationController.cs` | SparkAIViseme, SparkAIFacialShape, SparkAIFacialExpression, SparkAIVisemeMapping, SparkAIFacialAnimationController |
| Electrical components / state | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/Qustion/SparkAIKnowledgeQuestionResolver.cs` | SparkAIKnowledgeQuestionType, SparkAIKnowledgeQuestionResolver |
| Solver / topology candidate | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/Qustion/SparkAIObjectQuestionResolver.cs` | SparkAIObjectQuestionResolver |
| Teaching / UI consumer | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/Qustion/SparkAIPlayerQuestion.cs` |  |
| Teaching / UI consumer | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/Qustion/SparkAIQuestionController.cs` | SparkAIQuestionController, TerminalPathResult |
| Teaching / UI consumer | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/Qustion/SparkAIQuestionUI.cs` | SparkAIQuestionUI |
| Electrical components / state | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/Qustion/SparkAITerminalQuestionResolver.cs` | SparkAITerminalQuestionResolver |
| Teaching / UI consumer | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/SparkAIAdaptiveTeachingController.cs` | SparkAIAdaptiveTeachingController |
| Teaching / UI consumer | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/SparkAIAdaptiveTeachingPlanner.cs` | SparkAIAdaptiveTeachingPlanner |
| Teaching / UI consumer | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/SparkAIBrain.cs` | SparkAIBrain, SparkAIReasoningChangedEventArgs |
| Teaching / UI consumer | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/SparkAICircuitAnalyzer.cs` | SparkAICircuitAnalyzer, SparkAICircuitTopologyState |
| Teaching / UI consumer | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/SparkAICircuitPathAnalyzer.cs` | SparkAICircuitPathAnalyzer |
| Teaching / UI consumer | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/SparkAICircuitReasoner.cs` | SparkAICircuitReasoner |
| Teaching / UI consumer | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/SparkAIComponentReasoner.cs` | SparkAIComponentReasoner, SparkAIComponentState |
| Teaching / UI consumer | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/SparkAIConceptMistakeAnalyzer.cs` | SparkAIConceptMistakeAnalyzer |
| Teaching / UI consumer | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/SparkAIContextReasoner.cs` | SparkAIContextReasoner, SparkAIContextState, SparkAIContextFocus |
| Teaching / UI consumer | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/SparkAIDirector.cs` | SparkAIDirector, SparkAIDirectiveType |
| Teaching / UI consumer | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/SparkAIElectricalReasoner.cs` | SparkAIElectricalReasoner, SparkAIElectricalState |
| Teaching / UI consumer | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/SparkAIEvents.cs` | SparkAIEventType, SparkAIWorldChangedEventArgs, SparkAIMemoryChangedEventArgs |
| Teaching / UI consumer | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/SparkAIGameplayBridge.cs` | SparkAIGameplayBridge |
| Teaching / UI consumer | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/SparkAIIntent.cs` | SparkAIIntent, SparkAIIntentType, SparkAIIntentChangedEventArgs |
| Electrical components / state | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/SparkAIKnowledgeReasoner.cs` | SparkAIKnowledgeReasoner |
| Teaching / UI consumer | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/SparkAILearnerModel.cs` | SparkAILearnerModel, SparkAILearnerConcept, SparkAILearningState |
| Teaching / UI consumer | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/SparkAILearning.cs` | SparkAILearning, SparkAILearningChangedEventArgs |
| Teaching / UI consumer | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/SparkAILesson.cs` | SparkAILesson, SparkAILessonDefinition, SparkAILessonDifficulty |
| Teaching / UI consumer | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/SparkAIMemory.cs` | SparkAIMemory |
| Teaching / UI consumer | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/SparkAIMistakeDetector.cs` | SparkAIMistakeDetector |
| Teaching / UI consumer | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/SparkAIObservationCoordinator.cs` | SparkAIObservationCoordinator |
| Teaching / UI consumer | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/SparkAIPlayerActivity.cs` | SparkAIPlayerActivity, SparkAIPlayerActivityType, SparkAIPlayerActivityChangedEventArgs |
| Teaching / UI consumer | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/SparkAIPlayerResponse.cs` | SparkAIPlayerResponseType |
| Teaching / UI consumer | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/SparkAIPlayerResponseObserver.cs` | SparkAIPlayerResponseObserver |
| Teaching / UI consumer | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/SparkAIQuestionIntent.cs` | SparkAIQuestionIntent, SparkAIQuestionIntentResolver |
| Teaching / UI consumer | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/SparkAIReasoner.cs` | SparkAIReasoner, SparkAIReasoningState |
| Teaching / UI consumer | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/SparkAITeachingReasoner.cs` | SparkAITeachingReasoner, SparkAITeachingAction, SparkAITeachingTopic, SparkAITeachingIntensity |
| Teaching / UI consumer | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/SparkAITeachingSession.cs` | SparkAITeachingSession, SparkAITeachingSessionState |
| Teaching / UI consumer | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/SparkAIWorld.cs` | SparkAIWorld |
| Teaching / UI consumer | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/SparkAIWorldSnapshot.cs` | SparkAIWorldSnapshot |
| Teaching / UI consumer | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/Tutorial System/Editor/SparkTutorialSampleCreator.cs` | SparkTutorialSampleCreator |
| Teaching / UI consumer | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/Tutorial System/Runtime/SparkAIAdaptiveTroubleshooter.cs` | SparkAIAdaptiveTroubleshooter |
| Teaching / UI consumer | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/Tutorial System/Runtime/SparkAICircuitDiagnosis.cs` | SparkAICircuitDiagnosis |
| Teaching / UI consumer | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/Tutorial System/Runtime/SparkAIComponentInspector.cs` | SparkAIComponentInspector |
| Teaching / UI consumer | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/Tutorial System/Runtime/SparkAIConversation.cs` | SparkAIConversation |
| Workbench instruments | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/Tutorial System/Runtime/SparkAICurrentObjectiveProbe.cs` | SparkAICurrentObjectiveProbe |
| Teaching / UI consumer | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/Tutorial System/Runtime/SparkAIHybridBridge.cs` | SparkAIHybridBridge, Turn, AskRequest, AskResponse |
| Teaching / UI consumer | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/Tutorial System/Runtime/SparkAILiveTroubleshooter.cs` | SparkAILiveTroubleshooter |
| Teaching / UI consumer | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/Tutorial System/Runtime/SparkAILocalBrain.cs` | SparkAILocalBrain, Topic |
| Teaching / UI consumer | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/Tutorial System/Runtime/SparkAIObjectiveReasoner.cs` | SparkAIObjectiveReasoner |
| Teaching / UI consumer | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/Tutorial System/Runtime/SparkAIProgressionReasoner.cs` | SparkAIProgressionReasoner |
| Teaching / UI consumer | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/Tutorial System/Runtime/SparkAIRuntimeBindingAudit.cs` | SparkAIRuntimeBindingAudit |
| Teaching / UI consumer | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/Tutorial System/Runtime/SparkAIUnifiedAssistant.cs` | SparkAIUnifiedAssistant |
| Teaching / UI consumer | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/Tutorial System/Runtime/SparkAIWorldContext.cs` | SparkAIWorldContext |
| Teaching / UI consumer | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/Tutorial System/Runtime/SparkTutorialCinematicGuide.cs` | SparkTutorialCinematicGuide |
| Teaching / UI consumer | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/Tutorial System/Runtime/SparkTutorialCircuitAwareness.cs` | SparkTutorialCircuitAwareness |
| Teaching / UI consumer | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/Tutorial System/Runtime/SparkTutorialDefinition.cs` | SparkTutorialAdvance, SparkTutorialFocus, SparkTutorialStep, SparkTutorialDefinition |
| Teaching / UI consumer | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/Tutorial System/Runtime/SparkTutorialDirector.cs` | SparkTutorialDirector |
| Teaching / UI consumer | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/Tutorial System/Runtime/SparkTutorialGameplaySignals.cs` | SparkTutorialGameplaySignals |
| Teaching / UI consumer | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/Tutorial System/Runtime/SparkTutorialGhostDemonstrator.cs` | SparkTutorialGhostDemonstrator, DemonstrationKind, Demonstration |
| Teaching / UI consumer | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/Tutorial System/Runtime/SparkTutorialLevelBridge.cs` | SparkTutorialLevelBridge |
| Teaching / UI consumer | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/Tutorial System/Runtime/SparkTutorialMistakeCoach.cs` | SparkTutorialMistakeCoach, Mistake |
| Electrical components / state | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/Tutorial System/Runtime/SparkTutorialResistorStateBridge.cs` | SparkTutorialResistorStateBridge, ElectricalStage |
| Teaching / UI consumer | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/Tutorial System/Runtime/SparkTutorialTarget.cs` | SparkTutorialTarget |
| Teaching / UI consumer | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/Tutorial System/SparkTutorialStarter.cs` | SparkTutorialStarter |
| Teaching / UI consumer | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/Visual-Teaching-System/Editor/SparkAIAnimatedAnswerBuilder.cs` | SparkAIAnimatedAnswerBuilder |
| Teaching / UI consumer | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/Visual-Teaching-System/Editor/SparkAIComponentVisualLessonsBuilder.cs` | SparkAIComponentVisualLessonsBuilder |
| Teaching / UI consumer | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/Visual-Teaching-System/Editor/SparkAICourseCertificationBuilder.cs` | SparkAICourseCertificationBuilder |
| Teaching / UI consumer | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/Visual-Teaching-System/Editor/SparkAIExplanationBridgeBuilder.cs` | SparkAIExplanationBridgeBuilder |
| Teaching / UI consumer | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/Visual-Teaching-System/Editor/SparkAIGuidedLearningPathsBuilder.cs` | SparkAIGuidedLearningPathsBuilder |
| Teaching / UI consumer | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/Visual-Teaching-System/Editor/SparkAIGuidedRepairMissionsBuilder.cs` | SparkAIGuidedRepairMissionsBuilder |
| Teaching / UI consumer | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/Visual-Teaching-System/Editor/SparkAIInteractiveTrainingBuilder.cs` | SparkAIInteractiveTrainingBuilder |
| Teaching / UI consumer | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/Visual-Teaching-System/Editor/SparkAILearningCoachBuilder.cs` | SparkAILearningCoachBuilder |
| Teaching / UI consumer | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/Visual-Teaching-System/Editor/SparkAILearningPanel2Builder.cs` | SparkAILearningPanel2Builder |
| Teaching / UI consumer | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/Visual-Teaching-System/Editor/SparkAILearningPanelBuilder.cs` | SparkAILearningPanelBuilder |
| Teaching / UI consumer | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/Visual-Teaching-System/Editor/SparkAILearningPathAssessmentBuilder.cs` | SparkAILearningPathAssessmentBuilder |
| Teaching / UI consumer | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/Visual-Teaching-System/Editor/SparkAILessonLibraryBuilder.cs` | SparkAILessonLibraryBuilder, Definition |
| Teaching / UI consumer | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/Visual-Teaching-System/Editor/SparkAIStudentProgressCenterBuilder.cs` | SparkAIStudentProgressCenterBuilder |
| Teaching / UI consumer | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/Visual-Teaching-System/Editor/SparkAITeachingSessionSaveResumeBuilder.cs` | SparkAITeachingSessionSaveResumeBuilder |
| Teaching / UI consumer | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/Visual-Teaching-System/Editor/SparkAITrainingAnalyticsBuilder.cs` | SparkAITrainingAnalyticsBuilder |
| Teaching / UI consumer | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/Visual-Teaching-System/Editor/SparkAITrainingDashboardBuilder.cs` | SparkAITrainingDashboardBuilder |
| Teaching / UI consumer | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/Visual-Teaching-System/Editor/SparkAIUnifiedAssistantDirectBuilder.cs` | SparkAIUnifiedAssistantDirectBuilder |
| Teaching / UI consumer | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/Visual-Teaching-System/Editor/SparkAIUnifiedConnectionBuilder.cs` | SparkAIUnifiedConnectionBuilder |
| Teaching / UI consumer | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/Visual-Teaching-System/Editor/SparkAIUnifiedTutorialHubBuilder.cs` | SparkAIUnifiedTutorialHubBuilder |
| Other Project Spark source | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/Visual-Teaching-System/Editor/SparkAdaptiveExplanationBuilder.cs` | SparkAdaptiveExplanationBuilder |
| Workbench instruments | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/Visual-Teaching-System/Editor/SparkCircuitExperimentLabBuilder.cs` | SparkCircuitExperimentLabBuilder |
| Verified data transport | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/Visual-Teaching-System/Editor/SparkElectricalTelemetryBuilder.cs` | SparkElectricalTelemetryBuilder |
| Solver / topology candidate | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/Visual-Teaching-System/Editor/SparkSolverTelemetryLinkBuilder.cs` | SparkSolverTelemetryLinkBuilder |
| Teaching / UI consumer | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/Visual-Teaching-System/Editor/SparkTeachingAnchorEditor.cs` | SparkTeachingAnchorEditor |
| Teaching / UI consumer | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/Visual-Teaching-System/Editor/SparkVisualTeachingBuilder.cs` | SparkVisualTeachingBuilder |
| Teaching / UI consumer | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/Visual-Teaching-System/Editor/SparkVisualTeachingLiveBridgeBuilder.cs` | SparkVisualTeachingLiveBridgeBuilder |
| Teaching / UI consumer | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/Visual-Teaching-System/Runtime/AILearningPathAssessment/SparkAIAssessmentBank.cs` | SparkAssessmentItemKind, SparkAssessmentItem, SparkAIAssessmentBank |
| Teaching / UI consumer | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/Visual-Teaching-System/Runtime/AILearningPathAssessment/SparkAILearningPathAssessment.cs` | SparkAILearningPathAssessment, Attempt |
| Workbench instruments | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/Visual-Teaching-System/Runtime/CircuitExperimentLab/SparkCircuitExperimentLab.cs` | SparkCircuitExperimentLab, TargetObservation, Capture, ComparisonRow, ExperimentRecord, ExperimentExport |
| Teaching / UI consumer | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/Visual-Teaching-System/Runtime/Explainer/SparkAIExplanationAssistantConnector.cs` | SparkAIExplanationAssistantConnector |
| Teaching / UI consumer | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/Visual-Teaching-System/Runtime/Explainer/SparkAIUnityEventExplanationTransport.cs` | SparkAIUnityEventExplanationTransport |
| Teaching / UI consumer | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/Visual-Teaching-System/Runtime/Guided Learning/SparkAIGuidedLearningPaths.cs` | SparkAIGuidedLearningPaths |
| Other Project Spark source | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/Visual-Teaching-System/Runtime/Guided Learning/SparkGuidedLearningPathAsset.cs` | SparkLearningPathStageKind, SparkLearningPathStage, SparkGuidedLearningPathAsset |
| Teaching / UI consumer | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/Visual-Teaching-System/Runtime/SparkAIAnimatedAnswerPlayer.cs` | SparkAIAnimatedAnswerPlayer, AssistantReply, Step |
| Teaching / UI consumer | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/Visual-Teaching-System/Runtime/SparkAIComponentVisualLessonPlayer.cs` | SparkAIComponentVisualLessonPlayer, Topic, Case, Step |
| Teaching / UI consumer | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/Visual-Teaching-System/Runtime/SparkAICourseCertification.cs` | SparkAICourseCertification, CertificateRecord, CertificateDatabase, Audit |
| Teaching / UI consumer | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/Visual-Teaching-System/Runtime/SparkAIExplanationRequestBridge.cs` | SparkAIExplanationPromptEvent, SparkAIExplanationRequestBridge, AssistantReply |
| Teaching / UI consumer | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/Visual-Teaching-System/Runtime/SparkAIGuidedRepairMissions.cs` | SparkAIGuidedRepairMissions, Kind, Phase, Fault |
| Teaching / UI consumer | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/Visual-Teaching-System/Runtime/SparkAIInteractiveTrainingChallenges.cs` | SparkAIInteractiveTrainingChallenges, Phase, Tier, Challenge |
| Teaching / UI consumer | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/Visual-Teaching-System/Runtime/SparkAILearningCoach.cs` | SparkAILearningCoach, SessionPhase, Practice |
| Teaching / UI consumer | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/Visual-Teaching-System/Runtime/SparkAILearningPanel.cs` | SparkAILearningPanel, DrawerMode, ActionSpec, ActionBinding, SectionBinding, DrawerTween, SparkAILearningHoverHint |
| Teaching / UI consumer | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/Visual-Teaching-System/Runtime/SparkAILearningPanel2UX.cs` | SparkAILearningPanel2UX, ActionView |
| Teaching / UI consumer | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/Visual-Teaching-System/Runtime/SparkAILearningPanelMotionPreset.cs` | SparkAILearningPanelMotionPreset |
| Teaching / UI consumer | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/Visual-Teaching-System/Runtime/SparkAILessonLibrary.cs` | SparkLibraryTier, SparkLibraryLessonEntry, SparkAILessonLibrary |
| Teaching / UI consumer | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/Visual-Teaching-System/Runtime/SparkAIStudentProgressCenter.cs` | SparkAIStudentProgressCenter |
| Teaching / UI consumer | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/Visual-Teaching-System/Runtime/SparkAITeachingSessionSaveResume.cs` | SparkAITeachingSessionSaveResume, StudyEvent, StudySession, StudyStore, DisplayMode |
| Teaching / UI consumer | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/Visual-Teaching-System/Runtime/SparkAITrainingAnalytics.cs` | SparkAITrainingAnalytics |
| Teaching / UI consumer | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/Visual-Teaching-System/Runtime/SparkAITrainingDashboard.cs` | SparkAITrainingDashboard, ObjectiveRecord, LearningEvent, Store |
| Teaching / UI consumer | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/Visual-Teaching-System/Runtime/SparkAIUnifiedAssistantTransport.cs` | SparkAIUnifiedAssistantTransport, ReplyEnvelope |
| Teaching / UI consumer | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/Visual-Teaching-System/Runtime/SparkAIUnifiedTutorialHub.cs` | SparkAIUnifiedTutorialHub |
| Other Project Spark source | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/Visual-Teaching-System/Runtime/SparkAdaptiveExplanationEngine/SparkAdaptiveExplanationEngine.cs` | SparkAdaptiveSeverity, SparkAdaptiveCue, SparkAdaptiveExplanationEngine |
| Other Project Spark source | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/Visual-Teaching-System/Runtime/SparkAdaptiveExplanationEngine/SparkAdaptiveExplanationPanel.cs` | SparkAdaptiveExplanationPanel |
| Verified data transport | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/Visual-Teaching-System/Runtime/SparkElectricalTelemetryBridge.cs` | SparkElectricalCondition, SparkElectricalTelemetryBridge, Reading |
| Teaching / UI consumer | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/Visual-Teaching-System/Runtime/SparkSchematicRenderer.cs` | SparkSchematicRenderer |
| Solver / topology candidate | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/Visual-Teaching-System/Runtime/SparkSolverTelemetrySync.cs` | SparkSolverTelemetrySync |
| Teaching / UI consumer | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/Visual-Teaching-System/Runtime/SparkTeachingAnchor.cs` | SparkTeachingAnchor |
| Teaching / UI consumer | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/Visual-Teaching-System/Runtime/SparkTeachingIntegrationAdapter.cs` | SparkTeachingIntegrationAdapter |
| Teaching / UI consumer | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/Visual-Teaching-System/Runtime/SparkTeachingModels.cs` | SparkVisualShape, SparkPointSpace, SparkSchematicKind, SparkTeachingView, SparkVisualPoint, SparkVisualCommand, SparkSchematicPart, SparkSchematicWire, SparkSchematicData, SparkTeachingStep, SparkVisualLesson, SparkVisualTeachingPlan |
| Teaching / UI consumer | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/Visual-Teaching-System/Runtime/SparkTeachingValidator.cs` | SparkTeachingValidator |
| Teaching / UI consumer | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/Visual-Teaching-System/Runtime/SparkTeachingVectorGraphic.cs` | SparkVisualStroke, SparkTeachingVectorGraphic |
| Verified data transport | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/Visual-Teaching-System/Runtime/SparkVerifiedElectricalFrame.cs` | SparkVerifiedElectricalReading, SparkVerifiedElectricalFrame |
| Verified data transport | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/Visual-Teaching-System/Runtime/SparkVerifiedElectricalPublisher.cs` | SparkVerifiedElectricalPublisher |
| Teaching / UI consumer | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/Visual-Teaching-System/Runtime/SparkVisualTeachingLiveBridge.cs` | SparkVisualTeachingLiveBridge, LevelLessonBinding, VerifiedFaultPreset |
| Teaching / UI consumer | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/Visual-Teaching-System/Runtime/SparkVisualTeachingManager.cs` | SparkVisualTeachingManager |
| Teaching / UI consumer | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/Visual-Teaching-System/Runtime/SparkWorldAnnotationRenderer.cs` | SparkWorldAnnotationRenderer, Annotation |
| Teaching / UI consumer | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/Voice/SparkAIPiperTTSProvider.cs` | SparkAIPiperTTSProvider |
| Teaching / UI consumer | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/Voice/SparkAITTSBridge.cs` | SparkAITTSBridge |
| Teaching / UI consumer | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/Voice/SparkAIVoiceController.cs` | SparkAIVoiceController, SparkAIVoiceTextType |
| Other Project Spark source | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/Voice/SparkCharacterAnimationController.cs` | SparkCharacterAnimationController, SparkEmotion, SparkLookDirection, AnimationMode |
| Other Project Spark source | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Audio/SparkAudioVisualizer.cs` | SparkAudioVisualizer, SparkVoiceWaveGraphic |
| Electrical components / state | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Breadboard/SignalWireBuilder.cs` | WirePolarity, SignalWireBuilder |
| Electrical components / state | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Circuit/CircuitWireSystem.cs` | CircuitWireSystem |
| Other Project Spark source | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Circuit/SparkCircuitConnection.cs` |  |
| Other Project Spark source | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Circuit/SparkCircuitNode.cs` | SparkCircuitNode |
| Other Project Spark source | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Circuit/SparkCircuitSystem.cs` | SparkCircuitSystem, ElectricFlowVisualState |
| Electrical components / state | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Circuit/SparkCircuitTerminal.cs` | CircuitTerminal |
| Other Project Spark source | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Display/Alarm/SparkDisplayAlarm.cs` | SparkDisplayAlarm |
| Other Project Spark source | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Display/Components/SparkDisplayBar.cs` | SparkDisplayBar |
| Other Project Spark source | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Display/Components/SparkDisplayIndicator.cs` | SparkDisplayIndicator |
| Other Project Spark source | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Display/Core/SparkAdvancedDisplay.cs` | SparkAdvancedDisplay |
| Other Project Spark source | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Display/Core/SparkDisplayData.cs` | SparkDisplayData |
| Other Project Spark source | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Display/Core/SparkDisplayFormatter.cs` | SparkDisplayFormatter |
| Other Project Spark source | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Display/Core/SparkDisplayHistory.cs` | SparkDisplayHistory |
| Other Project Spark source | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Display/Core/SparkDisplayPeakHold.cs` | SparkDisplayPeakHold |
| Other Project Spark source | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Display/Core/SparkDisplayStatistics.cs` | SparkDisplayStatistics |
| Other Project Spark source | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Display/Core/SparkDisplayStatisticsTracker.cs` | SparkDisplayStatisticsTracker |
| Other Project Spark source | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Display/Core/SparkDisplayTypes.cs` | SparkDisplayUnit, SparkDisplayMode, SparkDisplayState, SparkDisplayQuality, SparkDisplayValueTransition, SparkDisplayTextAnimation, SparkDisplayStatusAnimation, SparkDisplayValueData |
| Other Project Spark source | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Display/Editor/SparkDisplayAssetCreator.cs` | SparkDisplayAssetCreator |
| Other Project Spark source | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Display/Graph/SparkDisplayGraph.cs` | SparkDisplayGraph |
| Other Project Spark source | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Display/Integration/SparkDisplayDemoSource.cs` | SparkDisplayDemoSource |
| Other Project Spark source | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Display/Integration/SparkDisplaySource.cs` | SparkDisplaySource |
| Other Project Spark source | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Display/Profiles/SparkDisplayAnimationProfile.cs` | SparkDisplayAnimationProfile |
| Other Project Spark source | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Display/Profiles/SparkDisplayProfile.cs` | SparkDisplayProfile |
| Other Project Spark source | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Display/Range/SparkDisplayRange.cs` | SparkDisplayRange |
| Other Project Spark source | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Display/Signal/SparkDisplaySignalQuality.cs` | SparkDisplaySignalQuality |
| Other Project Spark source | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Display/Status/SparkDisplayStatusRenderer.cs` | SparkDisplayStatusRenderer |
| Other Project Spark source | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Display/Test/Editor/SparkDisplayTestHarnessEditor.cs` | SparkDisplayTestHarnessEditor |
| Other Project Spark source | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Display/Test/SparkDisplayTestHarness.cs` | SparkDisplayTestHarness |
| Other Project Spark source | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Display/Text/SparkDisplayTextAnimator.cs` | SparkDisplayTextAnimator |
| Other Project Spark source | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Display/Value/SparkDisplayValueRenderer.cs` | SparkDisplayValueRenderer |
| Other Project Spark source | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Electrical Feedback/SparkComponentCurrentFeedback.cs` | SparkComponentCurrentFeedback |
| Other Project Spark source | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Electrical Feedback/SparkCurrentFeedbackBridge.cs` | SparkCurrentFeedbackBridge |
| Other Project Spark source | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Electrical Feedback/SparkElectricalFeedbackSystem.cs` | SparkElectricalFeedbackSystem |
| Other Project Spark source | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Electrical Feedback/SparkElectricalSafetyLimits.cs` | SparkElectricalSafetyLimits |
| Solver / topology candidate | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Electrical/SolverFramework/SparkElectricalNetworkGraph.cs` | SparkElectricalNetworkGraph |
| Other Project Spark source | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Electrical/SolverFramework/SparkElectricalSolveContext.cs` | SparkElectricalSolveContext |
| Solver / topology candidate | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Electrical/SolverFramework/SparkElectricalSolveResult.cs` | SparkElectricalSolveResult |
| Solver / topology candidate | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Electrical/SolverFramework/SparkElectricalSolverDefinition.cs` | SparkElectricalSolverDefinition |
| Solver / topology candidate | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Electrical/SolverFramework/SparkElectricalSolverRegistry.cs` | SparkElectricalSolverRegistry |
| Electrical components / state | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Electrical/Solvers/SparkBatterySolverDefinition.cs` | SparkBatterySolverDefinition |
| Electrical components / state | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Electrical/Solvers/SparkCapacitorSolverDefinition.cs` | SparkCapacitorSolverDefinition |
| Electrical components / state | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Electrical/Solvers/SparkDiodeSolverDefinition.cs` | SparkDiodeSolverDefinition |
| Solver / topology candidate | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Electrical/Solvers/SparkHairDryerHeaterSolverDefinition.cs` | SparkHairDryerHeaterSolverDefinition |
| Electrical components / state | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Electrical/Solvers/SparkHairDryerMotorSolverDefinition.cs` | SparkHairDryerMotorSolverDefinition |
| Electrical components / state | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Electrical/Solvers/SparkLEDSolverDefinition.cs` | SparkLEDSolverDefinition |
| Solver / topology candidate | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Electrical/Solvers/SparkMultimeterElectricalSolverDefinition.cs` | SparkMultimeterElectricalSolverDefinition |
| Solver / topology candidate | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Electrical/Solvers/SparkMultimeterSolverDefinition.cs` | SparkMultimeterSolverDefinition |
| Solver / topology candidate | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Electrical/Solvers/SparkPowerCableSolverDefinition.cs` | SparkPowerCableSolverDefinition |
| Electrical components / state | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Electrical/Solvers/SparkPowerSupplySolverDefinition.cs` | SparkPowerSupplySolverDefinition |
| Electrical components / state | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Electrical/Solvers/SparkResistorSolverDefinition.cs` | SparkResistorSolverDefinition |
| Electrical components / state | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Electrical/Solvers/SparkSingleWireSolverDefinition.cs` | SparkSingleWireSolverDefinition |
| Electrical components / state | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Electrical/Solvers/SparkSwitchIndexSolverDefinition.cs` | SparkSwitchIndexSolverDefinition |
| Electrical components / state | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Electrical/Solvers/SparkSwitchSolverDefinition.cs` | SparkSwitchSolverDefinition |
| Solver / topology candidate | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Electrical/Solvers/SparkTemporaryObjectSolverDefinition.cs` | SparkTemporaryObjectSolverDefinition |
| Electrical components / state | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Electrical/Solvers/SparkWireBranchCurrentResolver.cs` | SparkWireBranchCurrentResolver, WireCurrent, Edge |
| Electrical components / state | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Electrical/Solvers/SparkWireCurrentSolveBridge.cs` | SparkWireCurrentSolveBridge |
| Solver / topology candidate | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Electrical/SparkElectricalSolver.cs` | SparkElectricalSolver, UnionFind |
| Electrical components / state | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Electrical/SparkSingleWire.cs` | SparkSingleWire, ElectricFlowVisualState |
| Electrical components / state | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Electrical/SparkTerminalIndicator.cs` | SparkTerminalIndicator, IndicatorMode, DetectionMode, PopupDisplayMode |
| Other Project Spark source | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Electrical/SparkVoltagePopup.cs` | SparkVoltagePopup |
| Electrical components / state | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Electrical/SparkWireContact.cs` | SparkWireContact |
| Other Project Spark source | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/ElectricalArchitectureAuditor/Editor/SparkElectricalArchitectureAuditor.cs` | SparkElectricalArchitectureAuditor, FileInfoRecord, ApiInfoRecord, CheckRecord, SceneRecord, AuditData, Source |
| Electrical components / state | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Engineering/Components/Editor/SparkSwitchEditor.cs` | SparkSwitchEditor |
| Electrical components / state | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Engineering/Components/SparkBattery.cs` | SparkBattery |
| Electrical components / state | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Engineering/Components/SparkCapacitor.cs` | SparkCapacitor |
| Other Project Spark source | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Engineering/Components/SparkConnectorJack.cs` | SparkConnectorJack |
| Other Project Spark source | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Engineering/Components/SparkConnectorPlacement.cs` | SparkConnectorPlacement, SparkConnectorType |
| Other Project Spark source | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Engineering/Components/SparkDevice.cs` | SparkDevice |
| Electrical components / state | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Engineering/Components/SparkDiode.cs` | SparkDiode |
| Other Project Spark source | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Engineering/Components/SparkJackSocket.CS` | SparkJackSocket, SocketState, SocketRole, PlugEvent |
| Electrical components / state | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Engineering/Components/SparkLED.cs` | SparkLED |
| Electrical components / state | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Engineering/Components/SparkLEDHelixGlow.cs` | SparkLEDHelixGlow |
| Electrical components / state | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Engineering/Components/SparkMotor.cs` | SparkMotor |
| Other Project Spark source | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Engineering/Components/SparkPlug.CS` | SparkPlug |
| Other Project Spark source | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Engineering/Components/SparkPowerCable.cs` | SparkPowerCable |
| Electrical components / state | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Engineering/Components/SparkPowerSupply.cs` | SparkPowerSupplyState, SparkPowerSupply |
| Other Project Spark source | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Engineering/Components/SparkRelay.cs` | SparkRelay |
| Electrical components / state | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Engineering/Components/SparkResistor.cs` | SparkResistor |
| Electrical components / state | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Engineering/Components/SparkSwitch.cs` | SparkSwitchState, SparkSwitch |
| Electrical components / state | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Engineering/Components/SparkSwitchIndex.cs` | SparkSwitchIndex |
| Other Project Spark source | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Engineering/Components/SparkTemporaryElectricalObject.cs` | SparkTemporaryElectricalObject |
| Other Project Spark source | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Engineering/Components/SparkTemporaryObject.cs` | SparkTemporaryObject |
| Other Project Spark source | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Engineering/Components/SparkTransformManipulable.cs` | SparkTransformManipulable |
| Other Project Spark source | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Engineering/Components/SparkTransistor.cs` | SparkTransistorType, SparkTransistor |
| Other Project Spark source | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Engineering/Components/SparkVoltageSource.cs` | SparkVoltageSource |
| Other Project Spark source | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Engineering/SparkComponent.cs` | SparkComponent |
| Electrical components / state | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Engineering/SparkElectricalComponent.cs` | SparkConductionState, ISparkElectricalDevice, ISparkElectricalStateReceiver, SparkElectricalComponent |
| Other Project Spark source | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Engineering/SparkInstrument.cs` | SparkInstrumentState, SparkInstrument |
| Electrical components / state | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Engineering/SparkTerminal.cs` | SparkTerminalKind, SparkTerminalPolarity, SparkConnectionKind, SparkConnectionDirection, SparkTerminal |
| Electrical components / state | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Gameplay/BurnSystem/SparkResistorBurn.cs` | SparkResistorBurn, FailureMode, BurnState |
| Electrical components / state | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Gameplay/BurnSystem/old/SparkBurnParticleVFX.cs` | SparkBurnParticleVFX |
| Electrical components / state | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Gameplay/BurnSystem/old/SparkBurnVFXBridge.cs` | SparkBurnVFXBridge |
| Electrical components / state | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Gameplay/BurnSystem/old/SparkResistorFailureBurstVFX.cs` | SparkResistorFailureBurstVFX |
| Electrical components / state | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Gameplay/BurnSystem/old/SparkResistorSmokeGraphDriver.cs` | SparkResistorSmokeGraphDriver |
| Other Project Spark source | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Gameplay/Editor/SparkLevelSceneBindingsEditor.cs` | SparkLevelSceneBindingsEditor |
| Other Project Spark source | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Gameplay/Interactcion/SparkGameObjectVisibilityController.cs` | SparkObjectVisibilityController |
| Other Project Spark source | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Gameplay/Interactcion/SparkInteractable.cs` | SparkInteractable, InteractionType |
| Other Project Spark source | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Gameplay/Interactcion/SparkInteractionController.cs` | SparkInteractionController |
| Other Project Spark source | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Gameplay/Shelf/SparkLevelShelfController.cs` | SparkLevelShelfController, LevelShelfConfiguration |
| Other Project Spark source | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Gameplay/Shelf/SparkManualShelfPlacement.cs` | SparkManualShelfPlacement |
| Other Project Spark source | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Gameplay/Shelf/SparkObjectShelf.cs` | SparkObjectShelf, RigidbodyState |
| Other Project Spark source | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Gameplay/Shelf/SparkShelfDragHandler.cs` | SparkShelfDragHandler |
| Other Project Spark source | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Gameplay/Shelf/SparkShelfItem.cs` | SparkShelfItem |
| Other Project Spark source | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Gameplay/Shelf/SparkShelfPointerInteractor.cs` | SparkShelfPointerInteractor |
| Other Project Spark source | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Gameplay/Shelf/SparkShelfSlot.cs` | SparkShelfCategory, SparkShelfSlotKind, SparkShelfSlot |
| Other Project Spark source | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Gameplay/Shelf/SparkShelfSlotClickTarget.cs` | SparkShelfSlotClickTarget |
| Other Project Spark source | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Gameplay/SparkCommonTypes.cs` | SparkToolType, SparkInteractionType, SparkResultCode |
| Other Project Spark source | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Gameplay/SparkCurrentRangeObjective.cs` | SparkCurrentRangeObjective |
| Other Project Spark source | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Gameplay/SparkElectricalSequenceObjective.cs` | SparkElectricalSequenceObjective, SequenceStep |
| Other Project Spark source | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Gameplay/SparkElectronicObject.cs` | SparkOperationalState, SparkElectronicObject |
| Other Project Spark source | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Gameplay/SparkGameplayController.cs` | SparkGameplayController |
| Other Project Spark source | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Gameplay/SparkInteractionContracts.cs` | ISparkInteractable, ISparkMovable, ISparkRotatable |
| Other Project Spark source | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Gameplay/SparkInteractionSession.cs` | SparkInteractionSession |
| Gameplay evaluator | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Gameplay/SparkLevelEvaluator.cs` | SparkLevelEvaluator |
| Other Project Spark source | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Gameplay/SparkLevelSceneBindings.cs` | SparkLevelSceneBindings, TerminalBinding, ComponentBinding, ElectronicObjectBinding, GameObjectBinding |
| Other Project Spark source | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Gameplay/SparkTool.cs` | SparkTool, SparkToolContext |
| Other Project Spark source | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Gameplay/SparkToolController.cs` | SparkToolController |
| Teaching / UI consumer | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Levels/Campaign/SparkCampaignDefinition.cs` | SparkCampaignDefinition |
| Other Project Spark source | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Levels/Chapter/SparkChapterAccessController.cs` | SparkChapterAccessController, PlayMode |
| Other Project Spark source | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Levels/Chapter/SparkChapterDefinition.cs` | SparkChapterDefinition, ChapterAccessMode |
| Teaching / UI consumer | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Levels/Level4/HairDryer/SparkHairDryer.cs` | SparkHairDryer, PowerState, HeatMode, FanSpeed |
| Teaching / UI consumer | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Levels/Level4/HairDryer/SparkHairDryerAirflowVisual.cs` | SparkHairDryerAirflowVisual |
| Teaching / UI consumer | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Levels/Level4/HairDryer/SparkHairDryerCableVisual.cs` | SparkHairDryerCableVisual |
| Teaching / UI consumer | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Levels/Level4/HairDryer/SparkHairDryerControlInteraction.cs` | SparkHairDryerControlInteraction, ControlType |
| Teaching / UI consumer | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Levels/Level4/HairDryer/SparkHairDryerController.cs` | SparkHairDryerController |
| Teaching / UI consumer | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Levels/Level4/HairDryer/SparkHairDryerFanVisual.cs` | SparkHairDryerFanVisual |
| Teaching / UI consumer | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Levels/Level4/HairDryer/SparkHairDryerHeaterVisual.cs` | SparkHairDryerHeaterVisual, HeaterVisualState |
| Electrical components / state | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Levels/Level4/HairDryer/SparkHairDryerMotorVisual.cs` | SparkHairDryerMotorVisual |
| Electrical components / state | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Levels/Level4/HairDryer/SparkHairDryerSwitchIndicatorVisual.cs` | SparkHairDryerSwitchIndicatorVisual |
| Teaching / UI consumer | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Levels/Level4/HairDryer/SparkHairDryerThermalProtectionVisual.cs` | SparkHairDryerThermalProtectionVisual |
| Electrical components / state | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Levels/Level4/HairDryer/SparkSwitchVisual.cs` | SparkSwitchVisual, SwitchPosition, IndicatorIntensityMode |
| Teaching / UI consumer | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Levels/Level4/SparkHairDryerElectricalController.cs` | SparkHairDryerElectricalController |
| Teaching / UI consumer | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Levels/Level4/SparkHairDryerVisualController.cs` | SparkHairDryerVisualController |
| Gameplay evaluator | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Levels/LevelGamePlayManager.cs` | LevelGamePlayManager, LevelRuntimeState, TargetRuntimeResult |
| Other Project Spark source | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Levels/SparkLevelDefinition.cs` | SparkLevelDefinition, CompletionMode, LevelFailureMode, PowerSourceDefinition |
| Other Project Spark source | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Levels/SparkLevelEvaluation.cs` | SparkLevelEvaluationStatus, SparkLevelFailureReason, SparkLevelEvaluation |
| Other Project Spark source | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Levels/SparkLevelHandler.cs` | SparkLevelHandler |
| Other Project Spark source | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Levels/SparkLevelLayout.cs` | SparkLevelLayout |
| Other Project Spark source | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Levels/SparkLevelLayoutCatalog.cs` | SparkLevelLayoutCatalog, Entry |
| Other Project Spark source | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Levels/SparkLevelLayoutLoader.cs` | SparkLevelLayoutLoader |
| Other Project Spark source | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Levels/SparkLevelProgression.cs` | SparkLevelProgression, LevelProgressData, ProgressSaveData |
| Other Project Spark source | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Levels/SparkLevelSelector.cs` | SparkLevelSelector |
| Gameplay evaluator | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Levels/SparkLevelTarget.cs` | SparkLevelTarget, TargetType |
| Other Project Spark source | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Levels/SparkLevelValidationResult.cs` | SparkLevelValidationState, SparkLevelValidationResult |
| Other Project Spark source | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Levels/UI/SparkLevelMonitor.cs` | SparkLevelMonitor |
| Other Project Spark source | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Levels/UI/SparkLevelMonitorSnapshot.cs` | SparkLevelMonitorSnapshot |
| Other Project Spark source | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Levels/UI/SparkLevelMonitorTheme.cs` | SparkLevelMonitorTheme |
| Other Project Spark source | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Levels/UI/SparkLevelSelectionUI.cs` | SparkLevelSelectionUI |
| Electrical components / state | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Measurement/Holographic Object Viewer/HolographicWireframe.cs` | HolographicWireframe |
| Other Project Spark source | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Measurement/Holographic Object Viewer/SparkRotationGizmo.cs` | SparkRotationGizmo, GizmoAxis |
| Workbench instruments | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Measurement/Multi Meter/SparkMultimeter.cs` | SparkMultimeterMode, SparkMultimeterRange, SparkMultimeterDisplayState, SparkMultimeter |
| Workbench instruments | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Measurement/Multi Meter/SparkMultimeterAnimation.cs` | SparkMultimeterAnimation |
| Workbench instruments | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Measurement/Multi Meter/SparkMultimeterButtons.cs` | SparkMultimeterButtons |
| Workbench instruments | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Measurement/Multi Meter/SparkMultimeterDisplay.cs` | SparkMultimeterDisplay |
| Electrical components / state | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Measurement/Multi Meter/SparkMultimeterElectricalComponent.cs` | SparkMultimeterElectricalComponent |
| Workbench instruments | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Measurement/Multi Meter/SparkMultimeterProbe.cs` | SparkMultimeterProbeColor, SparkMultimeterProbe |
| Other Project Spark source | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Measurement/SparkMeasurementSystem.cs` | SparkMeasurementType, SparkMeasurementSystem |
| Other Project Spark source | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Measurement/SparkSmartObjectMover.cs` | SparkSelectedObjectMover |
| Other Project Spark source | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Scan/SparkScanSystem.cs` | SparkScanSeverity, SparkScanReport, SparkScanSystem |
| Other Project Spark source | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Sell Buy System/SparkEconomyCatalog.cs` | SparkShopCategory, SparkEconomyCatalog, Item |
| Other Project Spark source | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Sell Buy System/SparkEconomyInventoryUI.cs` | SparkEconomyInventoryUI |
| Other Project Spark source | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Sell Buy System/SparkEconomyService.cs` | SparkEconomyService |
| Other Project Spark source | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Sell Buy System/SparkEconomyShopUI.cs` | SparkEconomyShopUI |
| Other Project Spark source | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Sell Buy System/SparkEconomyWalletHUD.cs` | SparkEconomyWalletHUD |
| Other Project Spark source | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Sell Buy System/SparkLevelRewardBridge.cs` | SparkLevelRewardBridge, Reward |
| Other Project Spark source | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Sell Buy System/SparkPurchasedComponentSpawner.cs` | SparkPurchasedComponentSpawner |
| Other Project Spark source | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Sell Buy System/SparkRecycleBin.cs` | SparkRecycleBin |
| Other Project Spark source | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/SparkSceneReset.cs` | SparkSceneReset |
| Other Project Spark source | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/SparkSmartMove.cs` | SparkSmartMove |
| Other Project Spark source | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/SparkSmartMoveController.cs` |  |
| Workbench instruments | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/SparkVirtualMultimeter/Editor/SparkVirtualMultimeterBuilder.cs` | SparkVirtualMultimeterBuilder |
| Electrical components / state | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/SparkVirtualMultimeter/SparkMeterProbeTerminal.cs` | SparkMeterTerminalEnd, SparkMeterProbeTerminal |
| Verified data transport | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/SparkVirtualMultimeter/SparkMeterVerifiedNodeFeed.cs` | SparkMeterVerifiedNode, SparkMeterVerifiedNodeFrame, SparkMeterVerifiedNodeFeed |
| Workbench instruments | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/SparkVirtualMultimeter/SparkVirtualMultimeter.cs` | SparkVirtualMultimeter, MeterMode, DrawerMode, ProbeSelection, Measurement |
| Electrical components / state | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Text Exporter/Editor/SparkAIKnowledgeTextExporter.cs` | SparkAIKnowledgeTextExporter |
| Other Project Spark source | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Tools/SparkEngineeringTools.cs` | SparkToolController |
| Other Project Spark source | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Tools/SparkInspectTool.cs` | SparkInspectTool |
| Other Project Spark source | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Tools/SparkMeasureTool.cs` | SparkMeasureTool |
| Other Project Spark source | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Tools/SparkMoveTool.cs` | SparkMoveTool |
| Other Project Spark source | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Tools/SparkObjectRepositioner.cs` | SparkObjectRepositioner |
| Other Project Spark source | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Tools/SparkRotateTool.cs` | SparkRotateTool |
| Other Project Spark source | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Tools/SparkScanTool.cs` | SparkScanTool |
| Other Project Spark source | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Tools/SparkSelectTool.cs` | SparkSelectTool |
| Other Project Spark source | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Tools/SparkSelectable.cs` | SparkSelectable |
| Other Project Spark source | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Tools/SparkSelectionController.cs` | SparkSelectionController |
| Other Project Spark source | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Tools/SparkTransformManipulable.cs` | SparkTransformManipulable |
| Electrical components / state | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Tools/SparkWireTool.cs` | SparkWireTool |
| Other Project Spark source | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/TypeWriter/SparkTMPTypewriter.cs` | SparkTMPTypewriter, TextEvent |
| Other Project Spark source | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/UI/Editor/SparkUIFadeEditor.cs` | SparkUIFadeEditor |
| Electrical components / state | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/UI/SparkBatteryCharging.cs` | SparkBatteryCharging, BatteryState |
| Electrical components / state | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/UI/SparkBatteryDisplay.cs` | SparkBatteryDisplay |
| Other Project Spark source | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/UI/SparkUIFade.cs` | SparkUIFade, FadeTarget, FadeMode, FadeStyle |
| Electrical components / state | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/VFX/Signal/SignalWireCurrentFeedback.cs` | SignalWireCurrentFeedback |
| Electrical components / state | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/VFX/Signal/SignalWireElectricalGlow.cs` | SignalWireElectricalGlow |
| Other Project Spark source | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Visual/SparkCurrentPopup.cs` | SparkCurrentPopup |
| Other Project Spark source | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Visual/SparkCurrentVisual.cs` | SparkCurrentVisual |
| Electrical components / state | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Visual/SparkWireCurrentVisualPublisher.cs` | SparkWireCurrentVisualPublisher |
| Other Project Spark source | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Wire/BreadboardGrid.cs` | BreadboardGrid |
| Electrical components / state | `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Wire/SparkWireSystem.cs` | SparkWireSystem, ISparkWireSystem, SparkWire |
| Other Project Spark source | `Assets/My_Assets/_Project_Spark/Scripts/SparkUIAnimToggleButton.cs` | SparkUIAnimToggleButton |
| Other Project Spark source | `Assets/My_Assets/_Project_Spark/Scripts/SparkUIAnimationController.cs` | SparkUIAnimationController |
| Electrical components / state | `Assets/ProjectSpark-UI System/UI/Core/WireConnector.cs` | WireConnector |
| Electrical components / state | `Assets/ProjectSpark-UI System/UI/Core/WireController.cs` | WireController |
| Electrical components / state | `Assets/ProjectSpark-UI System/UI/Core/WireState.cs` | WireState |
| Other Project Spark source | `Assets/ProjectSpark-UI System/UI/Theme/AAAUI/Runtime/Core/Simulation/CircuitConnectionVFX.cs` | CircuitConnectionVFX |
| Other Project Spark source | `Assets/ProjectSpark-UI System/UI/Theme/AAAUI/Runtime/Core/Simulation/CircuitSignalSystem.cs` | CircuitSignalState, CircuitSignalSystem |
| Solver / topology candidate | `Assets/ProjectSpark-UI System/UI/Theme/AAAUI/Runtime/PropertySystem/UIMaterialResolver.cs` | UIMaterialResolver |
| Other Project Spark source | `Assets/ProjectSpark-UI System/UI/Theme/SparkAdvancedGlowController.cs` | SparkAdvancedGlowController, GlowState |
| Other Project Spark source | `Assets/ProjectSpark-UI System/UI/Theme/SparkSimpleGlow.cs` | SparkSimpleGlow |
| Other Project Spark source | `Assets/ProjectSpark-UI System/UI/Theme/VFX/Editor/SparkVFXControllerEditor.cs` | SparkVFXControllerEditor |
| Other Project Spark source | `Assets/ProjectSpark-UI System/UI/Theme/VFX/HolographicScanner/Scripts/ScannerCircuitFlowController.cs` | ScannerCircuitFlowController |
| Other Project Spark source | `Assets/ProjectSpark-UI System/UI/Theme/VFX/HolographicScanner/Scripts/ScannerCircuitFlowTarget.cs` | ScannerCircuitFlowTarget |
| Other Project Spark source | `Assets/ProjectSpark-UI System/UI/Theme/VFX/HolographicScanner/Scripts/ScannerCircuitFlowTest.cs` | ScannerCircuitFlowTest |
| Other Project Spark source | `Assets/ProjectSpark-UI System/UI/Theme/VFX/HolographicScanner/Scripts/ScannerCircuitTraceController.cs` | ScannerCircuitTraceController |
| Other Project Spark source | `Assets/ProjectSpark-UI System/UI/Theme/VFX/HolographicScanner/Scripts/ScannerCircuitTraceTest.cs` | ScannerCircuitTraceTest |
| Other Project Spark source | `Assets/ProjectSpark-UI System/UI/Theme/VFX/HolographicScanner/Scripts/ScannerCircuitVoltageController.cs` | ScannerCircuitVoltageController |
| Other Project Spark source | `Assets/ProjectSpark-UI System/UI/Theme/VFX/HolographicScanner/Scripts/ScannerCircuitVoltageTarget.cs` | ScannerCircuitVoltageTarget |
| Other Project Spark source | `Assets/ProjectSpark-UI System/UI/Theme/VFX/HolographicScanner/Scripts/ScannerCircuitVoltageTest.cs` | ScannerCircuitVoltageTest |
| Other Project Spark source | `Assets/ProjectSpark-UI System/UI/Theme/VFX/HolographicScanner/Scripts/ScannerReconstructionPointGenerator.cs` | ScannerReconstructionPointGenerator, MeshSource, TriangleSample |
| Other Project Spark source | `Assets/ProjectSpark-UI System/UI/Theme/VFX/Runtime/SparkScanOverlay.cs` | SparkScanOverlay |
| Other Project Spark source | `Assets/ProjectSpark-UI System/UI/Theme/VFX/Runtime/SparkVFXController.cs` | SparkVFXController |
| Other Project Spark source | `Assets/ProjectSpark-UI System/UI/Theme/VFX/Runtime/SparkVFXProfile.cs` | SparkVFXProfile |
| Other Project Spark source | `Assets/ProjectSpark-UI System/UI/Theme/VFX/SparkMonitorScanSequence.cs` | SparkMonitorScanSequence, ScanState, ScanResult |

## Next implementation gates

1. Confirm that the actual electrical solver has a completed-solve callback. Publish frame revisions only after a valid solve, on Unity's main thread.
2. Match each reading `anchorId` to a placed component and a `SparkTeachingAnchor`; do not infer identity from GameObject display names.
3. Ensure `circuitId` changes when the circuit layout changes, and use increasing positive revisions for synchronized node measurements.
4. For arbitrary terminal voltage, publish verified node potentials with the same `circuitId` and `revision` as the component frame.
5. Verify short/open/overload flags in the authoritative evaluator before allowing AI or instruments to assert faults.
6. Do not attempt time-domain oscilloscope or signal generator simulation until the solver supports time-varying signals and matching sample timestamps.

## Other outputs

- `SparkElectricalArchitectureAudit.json`: machine-readable scan results.
- `SparkElectricalPublicAPI.csv`: C# public declaration lines for inspection. This is a lexical scan, not a full C# compiler AST.

Report generation is read-only to gameplay assets. No scene, component, script, prefab, reward or save record is modified.
