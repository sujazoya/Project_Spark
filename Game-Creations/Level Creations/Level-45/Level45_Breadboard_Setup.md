# Level 45 — Hold the Speed

The player builds a 5 V motor governor with nine wires across 16 unique terminals. Encoder power and the driver's internal return bus are preinstalled. Every visible jack is a fixed breadboard terminal. This level uses fixed modules throughout.

## Install and inspect

1. Merge the ZIP's Assets folder into Project Spark with its metadata intact. The fitted FBX comes from the supplied showcase. Unity uses this fitted copy for the workbench.
2. Open MainMenu in Edit Mode and run **Tools → Project Spark → Level 45 → Install and Register**. The menu creates or reuses the definition, fits the prefab, adds the four-page drawing and registers the campaign and layout catalog. Save MainMenu if it already had unsaved edits.
3. Run **Tools → Project Spark → Level 45 → Validate Level Assets**. Expect a console message confirming 16 unique terminals, nine wire declarations, six objectives and 13 indicators. The existing SparkMotor solver definition from Level 30 must be present in the active registry.
4. Complete Level 44 to unlock Level 45. The controls and workbench spawn with the Level 45 prefab.

| Prefab location | Inspector assignment |
| --- | --- |
| Root | Spark Level Layout references Level_45.asset and Spark Level Scene Bindings. Spark Level 45 Governor references the layout, three signal sources, 5 V supply, motor, driver enable contact, equivalent impedance, controls, and the visual rig. |
| Governor Workbench | Fixed electrical components own the 16 terminals declared in the binding manifest. Use the exact wire IDs when troubleshooting. |
| Governor Workbench → Motion pivots | Motor shaft, flywheel, axle and brake rotor rotate on local X. The brake arm and pad move separately. Spark Level 45 Visual Rig references all six pivots. |
| Governor Workbench → Breadboard Labels | Governor and RPM TextMeshPro readouts are bound to the runtime governor. |
| Level 45 Controls | Overlay shows mode, command, measured RPM, error, duty, count, A/B state, load, fault and objective progress. |

## Play and verify

1. Connect all nine wires from the diagram. **Place W06 last**, after W07–W09, so Undo Last removes encoder B. Each supply jack takes two wires. The level should show **1/6**.
2. Select **Open Loop**, set **30 RPM** and press **Start**. Valid encoder transitions advance to **2/6**. Apply **Test Load** and watch speed fall at the same duty. Press **Test Load** again to release the brake.
3. Select **Closed Loop**. Hold 30 RPM within 27–33 RPM for two seconds to reach **3/6**. Select **60 RPM** and hold 57–63 RPM for two seconds to reach **4/6**.
4. Press **Test Load** at steady 60 RPM. The speed drops, duty rises, and the motor returns to 57–63 RPM for two seconds. This reaches **5/6**.
5. While the motor runs, use **Undo Last** to remove W06. Expect **ENCODER B LOST · DRIVE DISABLED** and zero solved motor current. Restore encoder B, wait for the shaft to coast down, press **Reset Fault**, then **Start** in closed loop at 60 RPM. The level reaches **6/6** after valid edges resume and speed settles.
6. For an overload check on a fresh run, cycle the brake to **OVERLOAD** while running. Expect the drive to shut down on stall. Select a lower brake setting, wait for the shaft to stop and reset.

## Diagnostics

- **0/6** means the controller has not verified topology and solved supply. Compare all endpoints against the CSV and confirm both 5 V branches and both returns are connected.
- **Stuck on a speed check** means the two-second window has not held. Check the selected mode, target, load setting and measured RPM. The governor checks both the encoder estimate and the mechanical speed.
- **5/6** means the encoder fault or its recovery has not been verified. Remove W06 while running, restore it, reset explicitly, and restart at 60 RPM in closed loop.
- The driver uses a **DC-equivalent PWM model**. The real solver calculates supply, motor current and voltage through a duty-controlled impedance. The level integrates inertia, torque, back-EMF, friction and brake torque separately. It samples decoded A/B transitions every 0.5 seconds for feedback. The model represents average behavior and does not simulate switching ripple.
