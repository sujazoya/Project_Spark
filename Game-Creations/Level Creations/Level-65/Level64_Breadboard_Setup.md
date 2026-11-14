# Level 64 — Hold the Synchronism

Open MainMenu and select Level 64. In another checkout, merge the package's Assets directory into the project, open MainMenu, run **Tools → Project Spark → Levels → Create Level 64**, then save. The supplied artwork and its companion files are included under Reference.

1. Wire W01–W62 using the illustrated guide. Connect **W62 last** so Undo Last Wire can remove it during the PMU fault test.
2. Calibrate, start both machines and select **Auto Sync**. Wait until the synchronization window is held, then close the sources.
3. Compare the relay delay plus opening time with the estimated critical clearing time. Use the initial 140 ms relay delay and apply **Feeder A Fault**. Both Feeder A ends should open while Feeder B stays available.
4. Enable **PSS** and apply **Load Step**. Watch the cyan relative angle and amber frequency difference settle. Restore Feeder A, then run **Breaker Failure**. Its receiving breaker stays mechanically closed while the series backup isolator clears the fault.
5. Service QFA OUT, restore Feeder A and start **W62 Test**. Remove the PMU B angle signal. Reconnect it, reset, calibrate and resynchronize before closing the sources.
6. Repeat **Feeder A Fault** and **Certify** once post-fault synchronism is verified.

The shared Project Spark solver validates the 5 V control rails and the player circuit graph. Generator response uses a reduced two-machine swing model with an unwrapped relative angle, bounded stabilizer torque, and changing transfer limits for fault-on and post-fault topologies. The CCT display estimates a stability boundary using isolated 5 ms simulation trials. The displayed clearing time comes from the elapsed simulated fault time when the path is isolated. The supplied art retains its proportions under a uniform workbench fit. TextMesh labels use a 0.20 local scale.
