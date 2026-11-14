# Level 59 — Bring Back the Power

## Install and play

Open Project Spark’s MainMenu scene and select Level 59. In another checkout, merge the package’s Assets folder into the Unity project. Open MainMenu and use **Tools → Project Spark → Levels → Create Level 59**, then save the scene. The installer registers the layout, definition and teaching drawing. The original art kit and visual references are included under Reference.

1. Connect W01–W42 using the exact wiring guide. The 5 V laboratory rails power the control circuit. The battery and AC network use a separate educational simulation.
2. Enable BMS, verify QG open, and precharge the DC link. Once precharge completes, select **Form Bus** and hold the 50 Hz reference.
3. Restore lighting, control and cooling in order. Start the generator and select **Auto Sync** after it accelerates. Close QGEN once the relay reports a held synchronization window.
4. Restore the motor and wait for acceleration. Restore the auxiliary load. Verify stable full load and both source contributions.
5. Select **Failure Test**. QGEN opens, motor and auxiliary shed, and the battery inverter keeps essentials powered. Wait for the rotor to stop. Repair, then reset the latched fault.
6. Restart, resynchronize and close QGEN. Restore motor and auxiliary again, then select **Final Certify** after full-load operation stabilizes.

## Model and layout

All 26 individual kit models are used, with one uniform fit that preserves proportions. The TextMesh labels use 0.20 local scale and the player terminals sit on distinct breadboard holes. The assembled FBX is included as a placement reference.

The control rails and player-wire validation use Project Spark’s shared solver and circuit graph. The level-specific educational model simulates a 2.4 V RMS phase reference at 50 Hz, a battery starting at 82% charge with a 24% reserve and 0.12 Wh scaled capacity, first-order DC-link precharge, generator acceleration, synchronization, source contribution, and cold-load pickup. The relay holds a tolerance window of 0.2 Hz, 5° and 5% voltage difference for 0.8 seconds. Current and energy are derived from this scaled model.
