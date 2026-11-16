# Level 79 — Bond the Worlds

After completing Level 78, open **MainMenu** and select Level 79. To install this package in another Project Spark checkout, merge its **Assets** directory into the Unity project, then run **Tools → Project Spark → Levels → Create Level 79** in Edit Mode.

## Wire, align and bond

1. Enable **5 V Control** and connect the 122 wires on the breadboard diagram. Connect **W037 last**, from **LASER POS OUT** to **HEX FINE IN**. **Undo Last Wire** can then remove it during the precision-motion test.
2. Select **Home Motion → Cal Cameras → Cal Laser**. Load the base wafer, select **Inspect Surfaces** to verify the wafer and known-good dies, then select **Prep Surfaces**. This reaches 2/6 after the optical feedback becomes valid and the surface checks pass.
3. For each die, select **Load Chiplet → Align Six Axes → Approach → Initiate Bond → Scan Interface**. The order is **A Logic**, **B Memory**, **C I/O**, **D Sensor**. The first two accepted scans award 3/6. The acoustic and infrared scans use the modeled interface coverage and void map.
4. After all four scans, select **Consolidate**. Wait for cooldown and select **Test Four Chiplets**. Kelvin resistance, daisy-chain continuity, inter-chip routing, timing and modeled BER must pass to reach 4/6.

## Disturbance and recovery tests

5. Use three separate assemblies. Apply **Camera Drift** during alignment, **Chuck Anomaly** during approach and **Particle Fault** during bond-front growth. Wait for each protection latch. Select **Restore → Reset Safety → Discard Assembly**. Recalibrate the cameras after the drift test. Build and electrically qualify a fresh four-chiplet assembly to reach 5/6.
6. On a fresh wafer, load the first chiplet and align. Start **Approach**, wait for **Arm W037 Test**, then select it and **Undo Last Wire**. Laser feedback becomes stale, the contact permit opens and the fine axis brakes to a controlled hold. The first five objectives stay complete.
7. Reconnect W037. Wait for motion to stop, then select **Home Motion → Cal Laser → Verify Laser → Reset Safety → Discard Assembly**. Build and electrically test a fresh four-chiplet assembly, then select **Certify**.

## Art and training model

The source kit provides 66 modules and 48 motion pivots. The assembly uses a uniform fit. The moving chiplet visuals scale uniformly from the inspection tray to the wafer so all four fit the pad field. Socket bases sit directly on the breadboard, and TextMesh labels use a 0.20 local scale.

Level 79 starts from a deterministic Level 78 bond-ready wafer preset. It tracks four interface regions per chiplet. The electrical solver handles the 5 V control branches, E-stop loop and contact permit. Other process I/O follows the validated player wire graph and the normalized motion and material simulation. Electrical quality indices are fictional training values.
