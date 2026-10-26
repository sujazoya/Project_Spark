# ProjectSpark AI Tutorial System — architecture and installation

## Architecture plan (production target)

1. **Authoritative gameplay layer** — `SparkObjectShelf`, `SparkManualShelfPlacement`, `SparkCircuitSystem` and your level/solver report *completed* actions. Tutorial never fakes placement, connections, electrical correctness or objectives.
2. **AI learning layer** — reuse `SparkAIBrain`, `SparkAITeacherController`, `SparkAITeachingSession`, `SparkAILearnerModel`, curriculum and existing `SparkAIVoiceController`. This package adds guided on-screen scenarios, not a replacement AI brain or online LLM.
3. **Tutorial data** — one `SparkTutorialDefinition` ScriptableObject per onboarding, mechanic, or level family; reusable `locatorKey` targets work for runtime-instantiated layouts.
4. **Director** — step state machine, continue/skip/hint, event-gated steps, checkpoints, accessible text captions, on-screen animated focus and AI voice.
5. **Presentation** — initial implementation creates its own Canvas at runtime. For production, replace with an art-directed UI prefab, character portraits, voice lip-sync, spotlight masks, lines/arrows, tweening, pad/controller navigation.
6. **Adaptive support** — later connect `SparkAILearnerModel`/mistake detectors to choose remedial sequences and variation. Never claim mastery from simply advancing a tutorial step.
7. **Level orchestration** — start tutorial when runtime layout is ready, use `SparkTutorialTarget` locators on slot prefabs, advance from real player actions, transition to challenge/evaluator when complete.
8. **Scale** — teach mechanic once, use optional micro-tutorials later. Save tutorial checkpoints per stable `tutorialId`; level-specific overrides use asset definitions, not unique controller scripts.
9. **QA** — test pause, scene reload, layout destruction/recreation, missing targets, UI scaling, input routing, wired-part safety, and tutorial cancel/restart.

## Install

1. Copy Runtime folder into `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/AI/Tutorial/`.
2. Copy Editor folder under an `Editor/` folder in your Unity project.
3. This initial overlay uses `UnityEngine.UI` (uGUI); install/enable Unity UI. It references existing types in your uploaded project: `ProjectSpark.AI.SparkAIVoiceController`, `ProjectSpark.Circuit.SparkCircuitSystem`, and `SparkCircuitConnection`.
4. In Unity, choose **Tools → ProjectSpark → AI → Create Getting Started Tutorial**. Select your intended asset folder first.
5. Create a scene GameObject `AI_TutorialSystem` and add `SparkTutorialDirector` and `SparkTutorialGameplaySignals`.
6. Assign the generated tutorial to the director **Default Tutorial**, your existing **SparkAIVoiceController** to Voice, and Main Camera to Scene Camera. Assign director and circuit references on signals component.
7. Start with `Auto Start` turned off. When the level layout is fully instantiated, call `director.BeginDefault()` or `director.Begin(tutorialForCurrentLevel)`. Setting Auto Start true is useful only in standalone test scenes.
8. Add `SparkTutorialTarget` to the storage resistor, runtime workstation slot, switch and starting terminal. Set respective keys: `shelf.resistor`, `workstation.resistor`, `device.switch`, `terminal.start`. Dynamic level prefabs should contain their own target components. Mark target anchors via the `worldAnchor` field.
9. Enter Play mode: the runtime overlay is created automatically. `F1` requests a hint, `F2` repeats speech. Buttons: Continue, Hint, Skip.

## Essential gameplay hooks

A tutorial must advance ONLY after a successful operation. In the `SparkManualShelfPlacement` controller:

After **successful** selection (where `selected = item` happens):
```csharp
FindFirstObjectByType<ProjectSpark.AI.Tutorial.SparkTutorialGameplaySignals>()
    ?.ReportShelfItemSelected();
```

After **successful** `shelf.MoveToSlot(item, slot)`:
```csharp
FindFirstObjectByType<ProjectSpark.AI.Tutorial.SparkTutorialGameplaySignals>()
    ?.ReportShelfItemPlaced();
```

After **successful** `objectShelf.TryReturnToShelf(item)`:
```csharp
FindFirstObjectByType<ProjectSpark.AI.Tutorial.SparkTutorialGameplaySignals>()
    ?.ReportShelfItemReturned();
```

On actual successful switch state change:
```csharp
FindFirstObjectByType<ProjectSpark.AI.Tutorial.SparkTutorialGameplaySignals>()
    ?.ReportSwitchToggled();
```

**Performance recommendation:** Cache the signals component once in each gameplay controller rather than searching with `FindFirstObjectByType` every interaction; calls shown are minimal integration examples.

The signal bridge automatically listens to `SparkCircuitSystem.ConnectionCreated` and `ConnectionRemoved`. Make sure only one bridge subscribes to avoid duplicate messages.

## Level events and existing AI

Start the tutorial only after the **Level_XX_Layout(Clone)** exists. The director will re-resolve targets when layouts are replaced. This package deliberately does not change your existing `SparkAILessonTeachingBridge`, `SparkAIDirector`, circuit solver, `SparkAIQuestionController`, or game-level evaluator.

Tutorial voice uses `SparkAIVoiceController.SpeakText(explanation)`. The existing voice provider decides whether playback is available. Text always remains readable even when voice is disabled.

The demo text is authored offline, not generated in real time. A future contextual AI explanation can route verified `SparkAIContextReasoner` output to this UI, with guardrails to avoid inventing actual circuit states.

## Limitations of delivered foundation

- Animated pulse spotlight and caption panel, not a fully art-directed AAA HUD or cutscene system.
- Tutorial step completion only checks matching event **names**. Stronger production rules should include item IDs, required slot, terminal endpoints, and actual circuit-evaluator predicates so selecting the wrong resistor or wiring wrong terminals cannot complete the lesson.
- PlayerPrefs checkpoints are local device saves; migrate to your project's profile/save system if needed.
- No automatic camera rail, 3D avatar lip sync choreography, or controller/touch implementation included yet.
- No runtime LLM or network AI added; it reuses your current deterministic AI and TTS.
- UI overlay does not block gameplay; UI buttons consume their own pointer events. Avoid clicking a focused control behind the panel.
- Event callback re-entrancy: callers must emit signals **after** their authoritative state transitions complete.

## Recommended next production phases

**Phase 2:** Predicate-based validators for shelf item/slot, switch state, wiring endpoints, resistor value, powered state; structured action telemetry & analytics. 

**Phase 3:** Polished overlay prefab with animated curved pointer, spotlight masking, on-screen demonstration ghost hands, reversible camera moves, voice/lip-sync character states and subtitle timings.

**Phase 4:** AI adaptation: struggling player detection, contextual explanation grounded in solver values, misconception challenge, mastery checks and skip for veterans.

**Phase 5:** authoring editor, localization, accessibility, gamepad, mobile, save migration, playmode tests, analytics & performance validation.
