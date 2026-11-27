# Implementation log

Active Unity project: `E:/Project_Spark_Master_Project/Project_Spark` with Unity 6000.3.19f1.

| Project-relative path | Change |
| --- | --- |
| `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Levels/SparkLevelLayoutLoader.cs` | Exposes the actually loaded prefab source and shares the catalog lookup with the audit. |
| `Assets/ProjectSpark/SparkV5/Pro/Integration/SparkNativeWiringPlan.cs` | Stores the reviewed definition, prefab, revision and digest and validates their binding. |
| `Assets/ProjectSpark/SparkV5/Pro/Integration/SparkEngineeringControlHub.cs` | Adds the command aliases, mode-aware universal execution, runtime diagnostics, and official evaluator readback. |
| `Assets/ProjectSpark/SparkV5/Pro/Integration/SparkEngineeringControlPanel.cs` | Extends the existing canvas with draw, explanation and status, including safe-area updates. |
| `Assets/ProjectSpark/SparkV5/Pro/Integration/SparkNativeWireCinematic.cs` | Publishes the existing physical stages for live status. |
| `Assets/ProjectSpark/SparkV5/Pro/Integration/Editor/SparkUniversalConstructionEditor.cs` and its Unity metadata | Adds evidence export, Editor review, campaign scan, and a stale-review build check. |
| `Assets/ProjectSpark/SparkV5/Pro/Integration/Editor/SparkFlightLevel20Verification.cs` | Exercises the v19 route in the existing physical Level 20 regression. |
| `Assets/ProjectSpark/SparkV5/Pro/Integration/Editor/SparkWorkbenchVerification.cs` | Registers the v19 run and separate Play Mode evidence file. |
| `Assets/Resources/SparkNativeWirePlans/Level_020_NativeWiring_DRAFT.asset` | Retains the existing reviewer and signed pairs while binding them to the audited authored revision. |
| `Reports/SuperAI_v19/` | Contains the requested reports, campaign CSV and evidence-backed draft export. |

## Rollback

The pre-change working-tree patch is saved at `Logs/SuperAI_v19/Before/WorkingTree.patch`. Preserve all other workspace changes. Use the v19-only patch at Logs/SuperAI_v19/ScopedSource.patch, review it with `git apply --reverse --check`, then apply it in reverse. The reports and logs can be retained as evidence. Do not reset the working tree.



## Campaign follow-up

The Android build was cancelled when campaign Auto Draw became the active priority. The follow-up kept validation in the Editor.

| Project-relative path | Change |
| --- | --- |
| `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Engineering/SparkTerminal.cs` | Distinguishes shared terminal carriers from actual two-terminal electrical owners. |
| `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Gameplay/SparkLevelSceneBindings.cs` | Uses the same distinction for authoritative live endpoint resolution. |
| `Assets/ProjectSpark/SparkV5/Pro/Integration/SparkAINativeSignalWirePort.cs` | Preserves the device-bypass rejection while allowing separate sockets on a passive carrier to reach the normal native checks. |

The Editor derives native-wire drafts from actual controller pair tables and serialized pin references. The campaign CSV records board controllers with separate placement, routing, via, or diagnostic requirements. The generated-draft import rechecks the source before adding an unapproved project asset.
