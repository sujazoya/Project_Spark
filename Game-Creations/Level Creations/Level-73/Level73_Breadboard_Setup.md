# Level 73 — Polish to Perfection

Open **MainMenu** and select Level 73 after completing Level 72. For another Project Spark checkout, merge the package's **Assets** folder into the project and run **Tools → Project Spark → Levels → Create Level 73** in Edit Mode. Save MainMenu after reviewing the new entries if Unity marks the scene as modified.

## Play sequence

1. Wire the 5 V supply, drives, downforce, slurry, pad, metrology, handling and safety circuits from the exact drawing. Connect **W98 last** for the final fault test. Enable **5 V Control**.
2. Calibrate speed, force, slurry, torque and endpoint. Select **Inspect Pad** and **Condition Pad**. Enable the slurry pump and cooling. Load and clamp a wafer, then start both drives. Wait for healthy flow, pad and speed readings.
3. Enable **Zone PID** and select **Engage**. Watch the physical force rise as the carrier lowers. The optical and eddy-current channels must both identify endpoint before the carrier retracts.
4. When available, select **Unload → Rinse → Brush Clean → Spin Dry → Metrology**. The quality check covers film thickness, wafer height variation, removal uniformity, dishing, erosion, scratches, slurry residue and moisture.
5. Load a test wafer and start both drives. Engage and choose **Pad Glaze**. Observe the lower removal rate, then **Condition Pad** and **Reset Safety**. Restart the drives and engage again. Apply **Slurry Restrict**, restore it and reset once measured flow recovers. Recommission and apply **Edge Drift**. Restore and reset, discard the test wafer and complete another polished and cleaned wafer.
6. At 5/6, start polishing a sacrificial wafer. At stable force, select **W98 Test** and **Undo Last Wire**. Downforce feedback goes invalid, the electrical contact permit drops and the carrier physically retracts. Reconnect **CARRIER_DOWNFORCE_SENSOR_OUT → MULTIZONE_PRESSURE_PID_FORCE_FEEDBACK_IN**. After retraction, select **Recal Force → Verify Zero → Reset Safety**. Discard the test wafer, recommission both drives, then polish, clean and inspect a fresh wafer. Select **Certify**.

## Workbench notes

- The status line reports the first required wire that is missing. The connection drawing uses full terminal IDs, and the socket labels abbreviate them to fit the board.
- Conditioning requires retraction and a pad inspection. Correct the active fault before resetting, then restart both drives.
- The readings show measured force separately from the physical actuator state. Actual downforce decays during retraction after the sensor link opens.
- The default speed request follows the tested walkthrough. **Platen RPM** cycles through alternate requests for comparison while the platen drive is stopped.

The assembly keeps the original proportions of the 54 supplied modules under one common fit. Its 35 motion groups are bound to the imported hierarchy. The package includes the original kit and reference artwork. The electrical solver handles the 5 V rails, E-stop and carrier contact permit. The other process signals use the validated wire graph and data from the fictional CMP model.
