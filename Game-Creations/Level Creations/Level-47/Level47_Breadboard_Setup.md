# Level 47 — Hold the Tension

The feed roll shrinks as the take-up roll grows. The controller matches surface speed and uses the powered analog dancer sensor to correct tension. In AUTO, the two motor RPMs change relative to one another even when the strip speed stays matched.

## Install

1. Merge the package Assets folder into Project Spark and preserve its metadata. The fitted assembly uses all 14 individual FBXs.
2. Open MainMenu in Edit Mode. Run **Tools → Project Spark → Level 47 → Install and Register**. This creates or reuses the definition and layout, fits the workbench, and registers the campaign, drawing and layout catalog.
3. Run **Validate Level Assets** from the same menu. The installer checks 28 terminal bindings, six evaluator IDs, live readouts and motion references. Save MainMenu if it already held unsaved edits.
4. Complete Level 46 to unlock Level 47. Keep the existing SparkMotor definition from Level 30 in the active solver registry.

| Prefab location | What to inspect |
| --- | --- |
| Root | Spark Level Layout, scene bindings, Spark Level 47 Tension and Spark Level 47 Visual Rig |
| Winding Workbench | Fixed motor channels, independent PWM branches, encoder terminals, and powered analog sensor output |
| Winding Workbench → Motion pivots | Shafts, spools, wound-material layers, encoder discs, dancer arm and roller, sensor wiper and brake pivots |
| Winding Workbench → Breadboard Labels | Socket labels and three live TextMeshPro displays |
| Level 47 Controls | Power, calibration, mode, start/stop, brake, fault reset and restart |

## Play and check progress

1. Wire W01–W15 using the diagram. **Place W15 last.** The supply positive and return each accept two wires. Both motors and calibration stay locked until sensor power, signal and other wiring are valid. This earns **1/6**.
2. With the centered dancer and stationary motors, press **Calibrate**. The solved sensor should read about 2.50 V. This earns **2/6**.
3. Select **Auto** and press **Start**. Both motors must turn and transfer material while the sensor holds 2.30–2.70 V for two seconds. This earns **3/6**.
4. Keep winding. The feed radius must fall, take-up radius must rise, and the speed ratio must adapt with stable tension. This earns **4/6**.
5. Apply **Test Load** while tension is stable. The feed brake briefly increases tension. The feed drive increases duty and take-up eases back. Allow tension to recover for two seconds to earn **5/6**.
6. While winding, undo W15. Both drives should stop. Restore W15, wait for both shafts to coast down, press **Reset Fault**, and **Start** in AUTO. Verified safe winding earns **6/6**.

## Physics and troubleshooting

- Core radii, material thickness and available length are serialized on the controller. The material on each spool follows the area relationship in the brief. A single transfer ledger conserves total length. The simplified web stores mismatch as a compliant state.
- The motor solver computes average current through each winding and duty-controlled impedance. The controller integrates motor acceleration and dancer motion using 10 ms steps. The brake applies torque only to FEED. Local quadrature follows validated wired nets at 80 counts per turn. The powered analog signal is read at the controller input from the actual DC solve.
- At **0/6**, follow the wiring diagnostic. At **3/6**, keep AUTO winding long enough for both radii to change. At **5/6**, W15 must be removed while winding, then reconnected before reset and restart.
- For overload verification, cycle the brake to **OVERLOAD** during a separate trial. Release the brake and wait for neutral feedback before resetting. **Restart** begins with a fresh feed roll and clears trial objectives.
