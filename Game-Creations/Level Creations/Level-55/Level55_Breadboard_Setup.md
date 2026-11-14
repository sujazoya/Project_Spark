# Level 55 — Float the Shaft

## Install

Merge the ZIP’s Assets folder into Project Spark with its Unity metadata. Open MainMenu in Edit Mode and run **Tools → Project Spark → Levels → Create Level 55**. Save MainMenu if it had pending edits. The creator registers the level definition, breadboard prefab and four-page teaching drawing. Use **Tools → Project Spark → Level 55 → Validate Level Assets** for serialized references and terminal IDs.

The playable prefab uses 20 fitted models derived from the supplied kit geometry and FBX material and pivot metadata. The Reference folder retains the original art, manifests, previews and authoring tools. Only the Assets folder belongs inside Unity.

## Play

1. Follow the exact wiring plan. **Connect W13 last** so Undo Last Wire isolates the right X gap sensor. Both supply jacks accept five wires.
2. Close the guard and select **CALIBRATE ALL** while the shaft is at rest on its touchdown bearings. Select **ENABLE MAGNETS**, then **LIFT SHAFT**. Wait for both ends to center and clear the supports.
3. Select **START MOTOR** to capture a stable, contact-free **45 RPM** reference.
4. Run **LEFT DISTURBANCE** and **RIGHT DISTURBANCE**. Compare the measured peak and settling time, then select **VERIFY RECOVERY**. Manual gain changes require new runs of both tests.
5. Select **HIGH-SPEED TEST**. The controller ramps to **75 RPM**, holds the reference and decelerates to **45 RPM** before awarding the objective.
6. Select **SENSOR FAULT TEST**, then undo W13. Watch the three drive circuits open, the shaft land on its mechanical supports and its speed reach zero. Reconnect W13, select **RESET FAULT**, recalibrate, enable magnets and select **RELIFT**. Start the motor to capture the new 45 RPM reference, then select **FINAL CERTIFICATION**.

## Model and tuning

- The project solver supplies motor voltage and current and separate solved electrical loads for the left and right drivers. W11–W18, W21–W22 and W28 route the level-local feedback, commands, encoder edges and contact state through the real wiring graph. Opposing pole currents are simulated within each powered station.
- The rotor is a rigid shaft with lateral translation and tilt. Journal separation is 0.42 m, rotor mass is 0.22 kg and touchdown clearance is 1.20 mm. Gravity causes touchdown when support is removed. The two contact constraints correct penetration and outward velocity at each journal.
- Each station uses four attractive poles with position-dependent force proportional to current squared over gap squared, clamped for force and current. The controller uses gravity feed-forward and per-axis PD feedback at 1 ms. Default Kp is 2350 N/m and Kd is 7 N·s/m. The mechanical radial model treats the axial guide as a fixed axial constraint.
- The UI reports simulated gap readings and shaft tilt, solved winding and driver currents, encoder reference, measured disturbance settling times, contact state and objective progress. The model uses a separate visual scale for shaft displacement and tilt.

| Symptom | Check |
|---|---|
| Cannot calibrate | Check the 5 V source, all four gap routes, W27 guard and W28 touchdown. Let the shaft stop. |
| Cannot lift | Calibrate the sensors, then enable both magnetic drivers. Verify W21–W26 and the driver rails. |
| No 45 RPM reference | Check W15–W20 and allow the speed and bearing gaps to settle. |
| Recovery button unavailable | Run both disturbances using the current gains. If either test fails, tune and repeat. |
| Reset disabled | Reconnect W13 and wait for a stopped touchdown. |
| Still 5/6 | Complete the fresh contact-free 45 RPM reference and select Final Certification. |
