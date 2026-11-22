# Draw Current Level

MainMenu connects the Draw Current Level button and Ask Spark to the active workbench. The loaded layout supplies the level identity. Objective results come from the gameplay manager when its active definition and scene bindings match that layout.

| Player action | Teaching view |
| --- | --- |
| Draw Current Level or “draw this level in 2D” | Authored pages if supplied. Otherwise, the objective map. |
| “Draw the objectives in 2D” | Objective cards, completion rules, current progress, and the next objective. |
| “Draw live circuit in 2D” | Recorded player wires connected to the loaded layout. The caption reports how many are shown. |
| “Show this objective in 3D” | A scene marker and callout on the current task's bound component, terminal, or workshop target. |

The whiteboard's **Previous** and **Next** buttons navigate multi-page drawings. **Replay** replays the current view. **3D World** requests a workbench target, and **Both Views** pairs a compatible whiteboard page with its marker.

## Authoring

- Create a **Project Spark → Visual Teaching → Level Drawing** asset and register it in the drawer's **Drawings** list if the level needs a custom illustrated schematic. Set its Level ID to the gameplay definition's ID.
- Give objectives player-facing titles and descriptions in the level definition. The generated map reads those fields directly.
- Use SparkLevelSceneBindings for terminal and component targets. A bespoke workshop whose evaluator uses a separate proof component can expose its current physical teaching target through ISparkAILevelTeachingFocus.
- Wire connections come from SparkCircuitSystem. Authored schematic pages illustrate their lesson. Evaluation and electrical state remain under the gameplay manager and solver.

Run **Tools → Project Spark → Visual Teaching → Verify MainMenu Draw System** with MainMenu open in Edit Mode to check the six Level 130 objective cards, 3D marker, reload behavior, and Level 1 wiring and paging. The report and reference screenshots are written to Logs.
