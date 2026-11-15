# Level 72 — Layer by Layer

Open **MainMenu** and select Level 72 after completing Level 71. To install into another Project Spark checkout, merge the package's **Assets** folder into the Unity project and run **Tools → Project Spark → Levels → Create Level 72** in Edit Mode. Save MainMenu after reviewing the new campaign and teaching entries if the scene was already dirty.

## Play sequence

1. **Build the network.** Follow the exact wiring drawing for W01–W94. Connect W94 last so **Undo Last Wire** can remove it for the final fault test. Enable **5 V Control**.
2. **Commission the reactor.** Calibrate pressure, flow, purge and heat. Open **Vac Isolation**, start the backing pump, then start the turbo when allowed. Enable **Pressure PID**. Load and clamp a wafer, turn on **Wafer Heat** and enable **Carrier Flow**. Wait for stable pressure and temperature.
3. **Demonstrate three manual cycles.** Select **A Dose → Purge A → B Dose → Purge B**. The sequencer waits for measured purge flow and low residual vapor before allowing the next dose. Repeat until the third objective is certified.
4. **Grow and inspect the film.** Select **Auto Batch**. It simulates all remaining cycles and stops at 120. Select **Metrology** to check the 12.0 ± 0.3 nm thickness, ≤3% nonuniformity and ≥95% trench conformality.
5. **Recover from disturbances.** Stop and commission a test wafer. Select **Purge Restrict**, then run **A Dose → Purge A** and observe the hold. Choose **Repair Fault**, then **Reset Safety** when available. Run **Heater Drift**, repair and reset it, then wait for the zone temperatures to recover. Select **Stuck A** and run **A Dose → Purge A** to observe the valve mismatch and hardware permit dropping. Repair and reset, then commission a fresh wafer and pass a second Auto Batch and Metrology.
6. **Restore purge feedback.** After 5/6, commission a sacrificial wafer. Start **A Dose → Purge A**, select **W94 Test**, then use **Undo Last Wire**. Reconnect the purge transmitter output to the sequencer's purge feedback input. Select **Recal Purge**, **Verify Purge**, then **Reset Safety**. Stop, load a fresh wafer, recommission, run Auto Batch and Metrology, then select **Certify**.

## On the workbench

- The status line identifies the first missing wire or the current interlock. **Dose Window** changes the shared exposure window. The default 1.16 s window is used in the walkthrough. Short doses can leave the deepest trench region underexposed.
- **Load Wafer** becomes available after pumpdown and pressure equalization. If the button stays disabled after a test, stop carrier flow with **Stop / Pump** and wait.
- Reconnection alone leaves the feedback fault latched. The level requires calibration, fresh purge and residual samples, an explicit reset and a fresh certified film.
- Runtime data includes pressure, purge-flow validity, three wafer zones, A/B residuals, QCM, optical thickness, film uniformity and trench coverage.

## Assembly and simulation

The 52 approved equipment models use a common scale and preserve their proportions. All 24 motion references are bound. Valves, pumps, robot, wafer lift, film and metrology visuals follow the process. The chamber uses translucent surfaces and a contained flow indicator. The source kit and original art references are archived in **Reference**.

The Project Spark electrical solver handles the 5 V rails, emergency-stop loop and dosing permit. Remaining process signals are routed through the validated wire graph and get their values from the level's simulation. The reduced-order model evolves finite gas inventory, actuator travel, separate residual vapors, regional reaction coverage, temperature and derived metrology. All recipe values are fictional game parameters.
