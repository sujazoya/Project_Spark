# Level 71 — Etch the Future

Open **MainMenu** and select Level 71 after completing Level 70. In another Project Spark checkout, merge the package's **Assets** folder into the Unity project, then run **Tools → Project Spark → Levels → Create Level 71** in Edit Mode. The creator registers the layout, level definition and four teaching pages. If MainMenu already has unsaved changes, save it after reviewing the registration.

## Play sequence

1. **Build the control network.** Follow the exact wiring drawing for W01–W90. Wire W90 last so **Undo Last Wire** can remove it during the final test. Turn on **5 V Control**.
2. **Commission vacuum.** Select **Cal Pressure**, **Cal Gas**, **Cal RF** and **Cal Optical**. Open **Vac Isolation**, start the backing pump, then start the turbo when its button becomes available. Wait for **Load Wafer**, then **Clamp Chuck** after the robot seats the wafer.
3. **Ignite stable plasma.** Enable the **Chiller**, **Pressure PID** and **Auto Match**. Start **Gas Flow**. When the interlocks permit it, select **Enable RF** and then **Ignite**. Watch reflected power fall and pressure stabilize.
4. **Etch and inspect.** Enable **Endpoint Auto**, then select **Etch**. The optical film feature falls as the film is removed. The endpoint controller stops RF when the measured feature stays below its threshold. Select **Metrology** once the plasma decays.
5. **Recover from three disturbances.** On the same clamped wafer, restart gas, RF and ignition without starting another etch. Select **Vac Leak**, wait for recovery, then select **RF Drift**. When reflected power settles, run **Cooling Test**. Thermal protection trips RF. Wait for **Reset Safety**, then load and clamp a fresh wafer and repeat the etch and metrology.
6. **Prove recovery from lost optical feedback.** After 5/6, load a test wafer and start a new etch. Select **W90 Test** while the etch is active. Use **Undo Last Wire** to remove **OES_SIGNAL_OUT → ENDPOINT_OPTICAL_IN**. The optical reading goes invalid and RF enable drops. Reconnect W90, select **Recal Optical**, wait for **Reset Safety**, and reset. Load a fresh wafer, repeat gas, RF, ignition, etch and metrology, and select **Certify**.

## Troubleshooting

- **Wire missing or wrong circuit.** The status line shows the first missing connection. Match both terminal IDs. Multiple wires may share designated supply, return or fan-out sockets.
- **Load Wafer unavailable.** Check isolation, pump readiness and chamber-to-load-lock pressure difference. Stop the process and wait for pumpdown before loading.
- **Ignite unavailable.** Check that the chuck is clamped, the chiller and gas are enabled, and RF delivered power is adequate. Use **Auto Match** for the normal route, or use the two manual tuning controls with Auto Match off.
- **Endpoint unavailable or invalid.** Check W90 and optical calibration. After a fault, reconnect, recalibrate, then wait for a fresh sample before resetting.
- **A wafer fails metrology.** Pump down, load a new wafer, then repeat the verified process settings and endpoint sequence.

## Implementation

The 50 equipment models retain their common source proportions. They were uniformly fitted to the existing Unity board. The art assembly has 21 pivot bindings. The visual rig animates available moving submeshes and uses plasma activity from the process model for the reactor glow. The original art kit, backups and helpers are in the package's **Reference** folder and the original art ZIP.

Project Spark's electrical solver handles the 5 V rails, emergency-stop loop and RF permit. Other process I/O channels use Project Spark wire connections and carry values from the Level 71 simulation. The simulator integrates chamber and load-lock pressure, limited gases, matching impedance, plasma activity, regional film, mask and substrate damage, and wafer temperature. The simulation uses fictional gas and etch parameters.
