# Project Spark — Draw Current Level (Asset-Based)

## Installation

1. Import the `Runtime` and `Editor` folders into `Assets` (keep the Editor script in an Editor folder).
2. Existing `SparkVisualTeachingManager`, `LevelGamePlayManager`, and visual-teaching schematic classes must already compile.
3. Use `Tools > Project Spark > Visual Teaching > Install Current Level Drawer` in your **shared gameplay scene**.
4. Select the teaching manager GameObject, find `SparkDrawCurrentLevel`, and assign `LevelGamePlayManager` and `SparkVisualTeachingManager` if auto-discovery doesn't find them.
5. Create assets with `Assets > Create > Project Spark > Visual Teaching > Level Drawing`. Set `levelId` to the exact active `SparkLevelDefinition.LevelId`, e.g. `level_07`, `level_08`, `level_09`.
6. Assign authored pages with `SparkSchematicPart[]` and `SparkSchematicWire[]` in each asset. Normalized coordinates run from 0 to 1. Avoid labeling many parts in the same region.
7. Add created assets to the `Drawings` list of `SparkDrawCurrentLevel`. The script selects by current `LevelGamePlayManager.ActiveLevel.LevelId`, never by scene name.
8. For a ready example run `Assets > Create > Project Spark > Visual Teaching > Level 8 Sample (4 Pages)` and assign the resulting asset to `Drawings`.

## UI hookup

On an existing Unity Button's `On Click()` drag the GameObject with `SparkDrawCurrentLevel` and select **SparkDrawCurrentLevel > DrawCurrentLevel()**. Optional page navigation: `NextPage()`, `PreviousPage()`, `ShowOverview()`, `ShowSolution()`.

## AI hookup

In your **existing** Ask Spark intent router, route the command `drawCurrentLevel` or `draw current level` to `SparkDrawCurrentLevel.DrawCurrentLevelFromAI()` (or `DrawCurrentLevel()`). This package does **not** claim to intercept AI text on its own; you must connect that command in the router, or call the public method from another script.

## Guarantees / limitations

- Reuses `PresentValidatedPlan()`; invalid plans are rejected by your existing teaching validator.
- Author-created educational circuit diagrams are **NOT live electrical topology scans**. Actual circuit validation remains with the solver/evaluator.
- Does not alter wiring, power, components, or the solver.
- If no current active level or matching asset exists, logs an explicit warning; it never silently draws another level.
- When a new level is created, only a drawing asset and its registration are needed — no new level-specific MonoBehaviour.
- `DrawCurrentLevel()` resets to that asset's initial page. Page navigation stops at the first/last page.
- The Level 8 sample draws the **target solution** on all four pages with different teaching text; this is intentional, not live state or step-by-step wire animation.

Unity compilation and Play Mode rendering must be tested inside your project.
