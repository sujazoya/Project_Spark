# Level 66 — Save the Factory

Open MainMenu and select Level 66 after unlocking it through Level 65. In another checkout, merge the package's Assets directory into Project Spark, run **Tools → Project Spark → Levels → Create Level 66**, and save MainMenu.

1. Connect W01–W70 using the wiring diagram. Connect **W70 last** for the feedback-loss test.
2. Calibrate, start the motor, and record a healthy baseline at 1,200 RPM. Enable bearing wear and compare the outer-race FFT peak with the alignment and gear-mesh bands. Select **Diagnose BPFO** when the evidence is sufficient.
3. Stop the motor. Wait for the shaft to come to rest, isolate the drive, engage lockout, and open the guard. Extract the bearing, fit the replacement, align the shaft, and verify torque. Close the guard and release lockout.
4. Restore drive power and start the test run. Observe low-speed operation, select **Speed Test** for nominal speed, then apply **Load Test**.
5. Select **W70 Test** and undo **VIB3 X → FFT X**. Confirm that the machine isolates and clears the live spectrum. Reconnect, recalibrate, wait for fresh samples, reset the fault, and restore drive power.
6. Restart, apply load, and certify the fresh healthy run.

The level uses the shared Project Spark circuit graph and 5 V solver for control power and switch topology. A level-specific machine simulation integrates torque, shaft and gearbox speed, load, current, temperature and bearing wear. It computes RMS vibration and a 256-point FFT from 500 Hz model samples. Replacement, alignment and torque are staged workbench interactions. The art kit is uniformly fitted to the workbench and the supplied original assets are included as reference.
