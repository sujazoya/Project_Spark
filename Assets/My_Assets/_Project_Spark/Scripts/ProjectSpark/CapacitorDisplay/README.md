# Project Spark — Digital Capacitor Display (native Unity system)

This package was written against the **ProjectSpark.zip** snapshot supplied in this conversation. It **does not replace SparkCapacitor or SparkCapacitorSolverDefinition**, and never changes the circuit or forces a solve.

## Contents
- `Runtime/SparkCapacitorDigitalDisplay.cs`: solver-completion telemetry; read-only measurement state; capacitance, voltage, current, stored charge, stored energy, charge-target percentage and status.
- `Runtime/SparkCapacitorDigitalScreen.cs`: neon TextMeshPro world-space UI, mode buttons, visual-only graph history, progress bar.
- `Editor/SparkCapacitorDigitalScreenBuilder.cs`: build the complete editable Canvas on a selected capacitor model/instance.

## Install
1. Copy both Runtime C# files to a scripts/runtime directory in Unity and the Editor C# file under any `Editor` directory.
2. Let Unity compile. Install TextMeshPro Essentials if prompted.
3. Select a **scene instance** containing `SparkCapacitor` in the Unity Hierarchy.
4. Choose **Tools > Project Spark > Electrical > Capacitor Display > Build on Selected Capacitor**.
5. In the selected object's `SparkCapacitorDigitalDisplay`, verify **Capacitor** and **Solver** references. Set **Target Charge Voltage** to 5 V for Level 21.
6. Position the generated `Spark_Capacitor_Digital_UI` above/in front of the capacitor; face toward the camera. The root has an editable world-space Canvas.
7. Ensure the scene has an **EventSystem** and a compatible UI input module to click mode buttons (camera/raycast configuration depends on your project).
8. Wire the **existing SparkCapacitor terminals** into the 5 V -> 1 kΩ -> capacitor -> return circuit. The UI creates no electrical terminals.
9. Enter Play Mode. UI must show `AWAITING` until an accepted solve. Check the Console diagnostic context menu **Capacitor Display / Log Diagnostic** if blank.

## Accuracy
- **Voltage, current, and power** come from `SparkCapacitor.ElectricalState` after `SparkElectricalSolver.SolveCompleted`.
- **Capacitance** and **rated voltage** are capacitor settings, not sensor readings.
- **Stored energy** is derived via `E = 0.5*C*V^2` and **stored charge** via `Q = C*V`.
- **Charge %** means `abs(Vc) / TargetChargeVoltage`, clamped to 0..100%; it does **not** indicate measured state of health.
- **No ESR, leakage, temperature, or capacitance measurement** is claimed: the current solver/component has no verified model of these properties.
- **Graph** samples read-only charge fraction; it is not a waveform emitted by the solver.
- **No electrical reset** button is provided: resetting a graph is cosmetic and does not clear capacitor transient history.
- **Status charging/discharging** is based on signed solver current. Terminal orientation matters; if the polarity is reversed, the status may invert. Set device terminals consistently.
- **Stale readings** are rejected after 1.5 seconds by default; if your simulation intentionally solves less frequently, raise `maxAcceptedDataAgeSeconds` in the Inspector.

## Existing gameplay compatibility
- No changes to SparkElectricalSolver, SparkLCDMeter, SparkLCDMeterDisplay, level targets, shelf, or bindings.
- This display is **not** itself an electrical meter; it is a dashboard attached to one capacitor. A physical LCD placed across its terminals still works independently.
- Use one display per capacitor; two or more displays may subscribe to the same solver without changing topology.
- The builder is idempotent for its generated UI and leaves the component/model intact. Use **Remove Generated UI from Selection** to remove only the UI.

## Test checklist
- Empty circuit: `AWAITING SOLVE`, no fictional readings.
- Healthy 5 V / 1k / 1000uF RC circuit: rising capacitor voltage and falling charging current during transient simulation.
- Voltage and energy respond to a successful solve.
- Disconnect circuit: stale/invalid display or new zero values, depending on solver response.
- Turn off transient simulation: capacitor behavior will differ; no UI implementation can fix this.
- All four mode buttons change only the display page, never the meter mode or solver topology.

**Limitations:** This code has not been compiled in your Unity editor. It is matched to the source snapshot but requires Unity validation and canvas alignment on your specific 3D model.
