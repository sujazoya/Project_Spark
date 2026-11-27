# SUPER AI v19.1 Auto Draw implementation

Spark Live routes `hey Spark draw current level` through the existing Engineering Control hub. The hub verifies the current definition, layout, approval and revision. Manual mode retains player control. Assisted mode proposes one reviewed wire, and Auto mode executes the reviewed sequence. The exam and power-off guards remain in the native control path. The level evaluator reports completion.

## Changes

- `Assets/ProjectSpark/SparkV5/Pro/Integration/SparkNativeWireCinematic.cs` checks reach using the character’s actual pose and returns an out-of-reach approach to ground or permitted flight planning. It checks the final insertion pose before reaching. Diagnostics include the terminal, root, NavMesh position, original hand joints, maximum arm reach, motion owner and cable length.
- `Assets/ProjectSpark/SparkV5/Pro/Integration/SparkV5FlightController.cs` follows the same position and rotation interpolation that the flight corridor checker validates. It continues to sweep the physical body during movement.
- `SparkNativeWiringPlan.cs` and `SparkEngineeringControlHub.cs` in the same integration folder check the referenced mission-controller script as part of the authored revision when the evidence points to that script. Existing Level 1 and Level 20 asset references keep their prior revision scheme.
- `Editor/SparkUniversalConstructionEditor.cs` resolves evaluator nets through real serialized terminal bindings. It rejects missing or ambiguous endpoints, self-connections, duplicate pairs, electrical-owner bypasses, disabled interaction owners and authored terminal capacity conflicts.
- The Level 30–36 and 38–41 controllers in `Assets/My_Assets/_Project_Spark/Prefabs/Levels/Scripts/Runtime/` now expose the same net definitions used by their own evaluators. The Editor derives wire candidates from those nets. Drafts still require explicit review and a successful native gameplay test.
- `Editor/SparkFlightLevel20Verification.cs` checks the specific Level 48 fourth wire against the native circuit and logs motion samples across the operation. It continues through the remaining wiring and Level 48’s native mission controls when the wiring succeeds.

Level 20’s runtime resistor binding resolves after the player installs each part with the native shelf. Auto Draw waits for a valid prerequisite by refusing to start with unresolved endpoints, then revalidates the reviewed connection before dispatch. Autonomous part installation and cross-tool sequencing remain open.

No installers or player packages were produced for v19.1.

## Campaign draw browser

`Level_Draws/index.html` is a self-contained browser for all 130 campaign levels. The companion JSON files capture the level identity, saved layout revision, approval, source, ordered terminal pairs, and any missing requirement. A draw record with no authored pairs has an empty connection list.

