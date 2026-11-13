# Level 43 — Hold the Line

Build a closed-loop position controller for a motorized inspection carriage. The level compares the solved command and position voltages and drives a reversible H-bridge. The signed voltage across the existing SparkMotor drives the supplied visual motion rig.

## Install

1. Merge the package's Assets folder into the Project Spark Unity project and preserve the asset metadata files.
2. In MainMenu Edit Mode, run **Tools → Project Spark → Level 43 → Install and Register**.
3. Save MainMenu if it already had unsaved changes. The installer registers the definition and teaching drawing in MainMenu and the prefab in the layout catalog.
4. Start Level 43 after completing Level 42. The active electrical solver registry needs the existing SparkMotor solver definition used by Level 30.

## Play

1. Use the Wire tool to connect W01–W06 from the exact wiring drawing. Add the two motor leads last.
2. Press **Jog Right** and watch the carriage move right and the position voltage rise.
3. Select **Auto A**. The command is 1.25 V, or 25% of travel. Wait until the motor stops.
4. Select **Auto B**. The command is 3.75 V, or 75% of travel. Hold in the ±3% band for one second.
5. Use **Undo Last** twice. Connect **OUT A → MOTOR B** and **OUT B → MOTOR A**. Press **Test Fault**. The test starts at mid-travel and stops the bridge when it detects movement away from Station B.
6. Undo both crossed leads and restore **OUT A → MOTOR A** and **OUT B → MOTOR B**. Select **Auto B** to verify the repair.
7. Select **Nudge Left**, wait for recovery, then select **Nudge Right**. Watch the driver correct in opposite directions and stop inside the same position band each time.

The model's carriage, pointer and sensor wiper move together. The motor shaft follows the signed drive direction. The four teaching pages cover wiring, position error, reversed leads, and recovery.
