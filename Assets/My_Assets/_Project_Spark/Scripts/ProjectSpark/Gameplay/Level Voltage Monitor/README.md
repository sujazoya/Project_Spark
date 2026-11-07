# Project Spark — Live Voltage Monitor

A **read-only** Unity uGUI / TextMeshPro dashboard created automatically by an Editor command. It detects changes in the **currently active level layout**, samples **every completed electrical solve**, and displays terminal and component voltages plus change history. No changes to the shared solver are required.

## Install

1. Copy `Runtime/SparkLevelVoltageMonitor.cs` to `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Gameplay/` (or another non-Editor scripts folder).
2. Copy `Editor/SparkLevelVoltageMonitorEditor.cs` into `Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Gameplay/Editor/`.
3. Confirm TextMeshPro Essentials are imported and Unity UI (uGUI) is present.
4. Open your main gameplay scene in Unity, **not a level prefab**.
5. Choose **Tools → Project Spark → Monitoring → Create Live Voltage Monitor UI**.
6. Save the scene. Start Play Mode and load Level 1, 21, 22, 23, or any other level.
7. Use PAUSE to freeze the display or CLEAR to clear the change history.

## What it shows

- All active `SparkTerminal.ElectricalState.Voltage` readings under `SparkLevelLayoutLoader.ActiveLayout`.
- Every active `SparkElectricalComponent.ElectricalState.Voltage` under the same layout.
- A chronological history of **observed changes** that exceed the chosen threshold (default 1 millivolt).
- It resets channel inventory when a new level layout becomes active.

**Important:** It monitors observations after solver completion. It cannot show continuous analog voltage changes between solver updates, and it does not create or solve new electrical states. Change history is bounded (default 80 entries). The view is throttled to 10 FPS but sampling happens after every SolveCompleted event.

## Inspector

- `Change Threshold Volts`: 0.001 (1 mV); reduce to 0.0001 for finer readings.
- `Display Refresh Hz`: 10. Increasing this increases TMP layout updates.
- `Max History Entries`: 80.
- `Include Component Voltages`: ON.
- `Include Inactive Objects`: OFF.

## Performance & compatibility

- Does **NOT** call `SolveNow()`, `MarkDirty()`, or trigger electrical evaluation.
- Does **NOT** modify `SparkElectricalSolver`, `SparkLevelEvaluator`, Level 21's driver, or diode definitions.
- Creates one Canvas in the gameplay scene; it survives changes of active layout within that scene.
- Existing `SparkElectricalSolver.SolveCompleted` notifications update the monitor automatically.
- No repetitive `Debug.Log` is emitted during normal use.

## Notes

- Component voltages represent the component's solved **voltage across its terminals**, whereas terminal voltages represent potential relative to the solver's reference node.
- Some components can be in the scene while inactive; these are omitted by default.
- If the active level is not found, assign the right `SparkLevelLayoutLoader` to the runtime component's Inspector. Likewise, explicitly assign `SparkElectricalSolver` if multiple solver instances exist.
- The UI uses screen-space overlay. Adjust the panel RectTransform to change position/size.
