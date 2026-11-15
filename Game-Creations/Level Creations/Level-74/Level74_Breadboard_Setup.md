# Level 74 — Align the Light

Open **MainMenu** and select Level 74 after completing Level 73. For another Project Spark checkout, merge the package's **Assets** folder into the Unity project and run **Tools → Project Spark → Levels → Create Level 74** in Edit Mode. Save MainMenu if Unity marks it as modified.

## Play sequence

1. Connect all 102 wires from the exact wiring drawing. Connect **W102 last** so **Undo Last Wire** will remove the interferometer feedback during the final test. Enable **5 V Control**.
2. Select **Cal Prep**, **Load Wafer**, **Spin Coat**, **Soft Bake**, then **Transfer**. Wait for each control to become available. Coating is checked against 350 ±15 nm.
3. **Load Reticle**, **Clamp Wafer**, **Home Stages** and **Cal Position**. Select **Align Marks**, **Map Focus**, **Isolation** and **4:1 Sync**. Run **Dry Scan**. The shutter stays closed while the controller checks the measured stage ratio.
4. Turn on **Virtual Light**, wait for **Cal Dose**, then select **Expose 16**. Keep the default aperture for this walkthrough. The field map tracks settling, exposure, dose progress, rejection and certification. After 16 fields, select **Post Bake → Develop → Rinse → Spin Dry → Metrology**.
5. Load a test wafer, select **Nozzle Restrict**, then **Spin Coat** to observe the thin-film fault. Restore and reset, load and prepare a fresh wafer, align and map focus. Select **Isolation Test** and run **Dry Scan** to verify the motion hold. Restore and reset. Select **Lens Drift**, wait for the thermal hold, then restore and reset once the lens cools. Prepare a new wafer and repeat a full print and metrology.
6. At 5/6, prepare and align a test wafer, run the dry scan and start exposure. When **W102 Test** is available, select it and use **Undo Last Wire**. The shutter closes, the current field is rejected and the stages brake from their actual positions. Reconnect **WAFER_STAGE_INTERFEROMETER_SIGNAL_OUT → STAGE_SYNCHRONIZATION_POSITION_FEEDBACK_IN**. Select **Recal Laser**, **Reference** and **Reset Safety** when available. Prepare and align a fresh wafer, run all 16 fields and finish post-processing and metrology. Select **Certify**.

## Workbench

- The 4×4 map reads left to right, top to bottom. Amber is scanning, blue is exposed, violet is developed, green is verified and red marks a rejected field. The short bars show integrated dose.
- Reconnection supplies the signal again. Recalibration, fresh samples, re-referencing and reset are required before the shutter can be enabled again.
- Printed CD, overlay, focus, dose and pattern quality are measured per field. A partially exposed wafer must be replaced before a fresh full scan.
- The 56 modules retain the original proportions under one common fit. All 42 authored motion groups are bound. The miniature stage travel is exaggerated visually so it can be seen on the workbench.

Project Spark solves the 5 V rails, emergency-stop loop and shutter permit. The remaining signals use the validated Project Spark wire graph and the level's double-precision stage model, synthetic resist and optical response. The process values are fictional gameplay parameters.
