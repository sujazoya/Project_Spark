# Project Spark — Visual Teaching Engine (AAA foundation)

A self-contained **Unity 6 / UGUI / TextMeshPro** visual-teaching implementation.
It does not modify your existing Spark economy, shop, tools, shelf, simulator, level manager,
`SparkAICircuitDiagnosis`, `SparkAIAdaptiveTroubleshooter` or `SparkAIUnifiedAssistant` scripts.

## Contents

- `Runtime/SparkTeachingModels.cs` — data-only drawing commands, lesson assets, schematic objects.
- `Runtime/SparkTeachingAnchor.cs` — stable scene anchor IDs that follow objects and cameras.
- `Runtime/SparkTeachingValidator.cs` — limits command count, text lengths, geometry and dimensions.
- `Runtime/SparkTeachingVectorGraphic.cs` — efficient Canvas vector line / arrow / wire mesh.
- `Runtime/SparkWorldAnnotationRenderer.cs` — follow actual world-space components with callouts.
- `Runtime/SparkSchematicRenderer.cs` — diagram symbols, verified explicit wire routes, zoom / pan.
- `Runtime/SparkVisualTeachingManager.cs` — guided lessons, playback, verified action gates, progress, demo.
- `Runtime/SparkTeachingIntegrationAdapter.cs` — safe APIs for your existing AI and gameplay systems.
- `Editor/SparkVisualTeachingBuilder.cs` — creates Canvas, world overlay, whiteboard and all controls.
- `Editor/SparkTeachingAnchorEditor.cs` — adds/assigns scene anchor components.

## INSTALL (do this exactly)

1. Copy `Runtime/*.cs` into `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/VisualTeaching/`.
2. Copy `Editor/*.cs` into `Assets/My_Assets/_Project_Spark/Editor/VisualTeaching/`.
3. Ensure TextMeshPro and Unity UI (UGUI) are installed (already used by your project).
4. Allow Unity to compile. Fix any pre-existing compile errors first.
5. Menu: **Tools → Project Spark → AI → Create Visual Teaching System (AAA)**.
6. The builder creates a separate root Canvas: `SparkAIVisualTeachingCanvas`.
7. Press Play. Click the small **AI** launcher, then **DEMO**.
8. Use **NEXT**, **PREVIOUS**, **REPLAY**, **PAUSE**, **CLEAR**, **ZOOM + / -**, or drag/scroll
   the schematic. Select **BOTH VIEWS**, **WHITEBOARD**, or **3D WORLD**.
9. To render over real 3D components, select the battery/resistor/etc game objects and use
   **Tools → Project Spark → AI → Add Anchors to Selected Scene Objects**. Assign stable, unique
   `anchorId`s in Inspector. Re-run this only when needed: it derives IDs from names.
10. Link the adapter's public methods from your existing *verified* AI / tutorial code.

Note: The UI is generated in a **separate Screen Space Overlay Canvas (sort order 690)**.
You may change the sorting order if your existing monitor/diagnostic overlays overlap it.
The full-screen world drawing layer is not raycastable and does not block gameplay clicks.
The whiteboard covers only its own area. Its launcher is shown while closed.

## API: trusted gameplay adapters

Reference `SparkTeachingIntegrationAdapter` (component on generated canvas):

```csharp
using ProjectSpark.UI.VisualTeaching;

[SerializeField] private SparkTeachingIntegrationAdapter teacher;

// After your circuit diagnostics has positively identified a fault:
teacher.HighlightVerifiedFault("resistor", "Resistor is disconnected from the return path.");

// When your game wants to teach a connection:
teacher.GuideConnection("battery_positive", "resistor_input", "Connect this terminal here.");

// After the circuit solver has verified the true path (never guess these IDs):
teacher.DrawVerifiedCurrentRoute(new[]{"battery", "resistor", "led"}, "Verified series path.");

// After your gameplay code confirms the required user action:
teacher.SignalVerifiedTutorialAction("placed_resistor_correctly");
```

For direct control of the lesson asset:

```csharp
[SerializeField] private SparkVisualTeachingManager teacher;
[SerializeField] private SparkVisualLesson lesson;

public void BeginTutorial() => teacher.StartLesson(lesson);
public void ReplayCurrentStep() => teacher.ReplayStep();
```

Create lesson assets via **Assets → Create → Project Spark → AI → Visual Teaching Lesson**.
Edit steps, instructions, hints, optional verified action IDs, schematic parts/wires and world
annotations in the Inspector.

## AI-command boundary

An AI integration can produce **data only**, following `SparkVisualTeachingPlan` schema.
Use `manager.PresentValidatedPlan(plan)` or `manager.PresentPlanJson(json)`.
The validator restricts types, counts, text, geometry and numeric ranges.
Never invoke game-object methods, execute scripts, or change circuit values using an AI response.

Example plan (JSON enum values are serialized numerically by Unity):

```json
{
  "title": "Component callout",
  "explanation": "Find the resistor. This illustration is not a current measurement.",
  "annotations": [{
    "shape": 2,
    "from": {"space": 0, "normalized": {"x": 0.5, "y": 0.5}},
    "to": {"space": 0, "normalized": {"x": 0.6, "y": 0.6}},
    "normalizedSize": {"x": 0.12, "y": 0.14},
    "color": {"r": 0.2, "g": 0.85, "b": 1, "a": 1},
    "thickness": 5,
    "drawSeconds": 0.5,
    "holdSeconds": 0.3,
    "text": "",
    "fontSize": 22,
    "persistent": true
  }],
  "schematic": {"caption": "No verified circuit diagram provided", "parts": [], "wires": []}
}
```

`SparkVisualShape`: `Line=0, Arrow=1, Ring=2, Rectangle=3, Label=4, Wait=5, Clear=6`.
`SparkPointSpace`: `NormalizedScreen=0, WorldAnchor=1`.
For world anchors, specify `space=1, anchorId="resistor"` inside `from` / `to`.
If the component is not registered or behind the camera, it is **not drawn at all** (no
misleading fallback). Plans are limited to 64 annotations, 48 parts, 96 wires, and 64 points
per wire. This guards the UI, but the upstream AI connector should also authenticate and
rate-limit requests.

## Important scope / truthfulness

This package contains the production-oriented **presentation and lesson-authoring layer**, a
working end-to-end *offline demo*, and APIs for existing systems. It is **not** a language model,
a circuit solver, or an automatic adapter to your private Project Spark diagnostics classes.
Those classes' exact event types and state APIs have not been supplied, so no safe code can
claim to interpret them automatically. The correct integration is for your existing diagnostic
controller to supply verified facts and true wire routes to the adapter.

The demo shows a **schematic illustration**, not a measurement or certification of a live circuit.
Do not interpret orange/yellow wire colors as actual energized state unless your simulator has
verified it. Level changes must ensure spawned anchors use stable IDs and that actions are
reported *only after verified game state changes*.

## Recommended integration order

1. Run standalone demo.
2. Add anchors to a couple of known test objects.
3. Call `ExplainComponent` in gameplay and move the camera to verify following behavior.
4. Wire your trusted diagnostic callback to `HighlightVerifiedFault`.
5. Author a `SparkVisualLesson` asset and connect verified step-completion actions.
6. If available, add a read-only snapshot adapter to the existing circuit simulation engine.

No changes are necessary to Shelf, Economy, Purchases, Inventory, or the existing tool panel.
