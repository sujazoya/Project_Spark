# Level 53 — The Hidden Couple

Merge the package's Assets folder into Project Spark with its metadata. Open MainMenu in Edit Mode and run **Tools → Project Spark → Levels → Create Level 53** to fit and register the rig. Review and save MainMenu if it has pending changes.

## Play all six objectives

1. Connect all 24 wires. Connect **W15 last** so Undo Last Wire can isolate the right vibration pickup. Each supply socket takes five wires.
2. Start at **45 RPM**, wait for five complete measured revolutions and record **BASELINE**. Compare both bearing phases. The baseline check tests whether either single plane could meet both vibration limits.
3. Run **TRIAL A**. Both pickups measure the trial, then Plane A returns to neutral automatically.
4. Run **TRIAL B**, wait for its measurement window, then select **SOLVE**. The display shows the complex influence matrix and predicted bearing residuals.
5. Select **AUTO**, press **APPLY** and wait for both planes to settle. Press **VERIFY**. Each bearing must reach 30% or less of its own baseline.
6. Select **60 RPM** while running and verify. After initial certification, select **SENSOR FAULT TEST** and undo W15. Wait for the rotor to stop, reconnect W15, reset, restart and verify both bearings again.

## Inspector and simulation

The layout root holds the fixed terminal bindings, six objective bindings, controller and supplied visual adapter. The installer assigns the two independent correction pivots and guard hinge. The fitted assembly uses the 17 supplied modules. See the binding and pivot manifests for reference names.

The shared solver provides rail voltages, motor current and separate actuator currents. An averaged electrical drive feeds a level-local shaft model. The encoder routes 80 A/B counts per turn and uses Z for absolute phase. The simulated left and right pickups independently measure each trial's response. The complex solver rejects small or poorly conditioned determinants and corrections outside the 1.10 normalized travel limit. The visible weights change angle and radial offset as the simulated actuators move. Measurements are collected after motion settles. At high speed both residuals must stay below 30% of baseline and 0.35 mm/s.

| Symptom | Check |
|---|---|
| Start blocked | Follow the first missing W-number and check the guard on W24. |
| Baseline unavailable | Wait for stable 45 RPM and five valid Z revolutions. |
| Trial or apply stalls | Check the individual actuator supply and command wires W19–W23. |
| SOLVE unavailable | Finish both trial measurements. |
| Reset unavailable | Restore W15 and wait for the shaft to stop. |
| Still 5/6 after reset | Restart and press VERIFY again at stable 60 RPM. |
