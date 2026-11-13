# Level 54 — Cross the Critical

## Install in Project Spark

Merge the ZIP’s Assets folder into the project with its Unity metadata. Open MainMenu in Edit Mode and choose **Tools → Project Spark → Levels → Create Level 54**. The creator registers the level, layout and four-page teaching drawing. Save MainMenu if Unity reports existing unsaved edits. Use **Tools → Project Spark → Level 54 → Validate Level Assets** to check the serialized references.

The playable prefab uses 17 separately fitted FBXs. The supplied FBXs provide the original animation and material bindings, and their matching GLBs provide mesh geometry. The supplied showcase, placement tool and source manifests are in the Reference folder. The shipped installer does the breadboard placement.

## Wiring and play

1. Use the exact wiring image or CSV. Connect **W15 last**. The supply positive and return each accept five wires. All 36 sockets use distinct breadboard holes.
2. With the guard closed and the rotor at rest, select **CALIBRATE X/Y**, then **START**. At stable **45 RPM**, wait for three encoder-indexed revolutions and select **BASELINE**.
3. Select **SPEED SWEEP**. The controller samples eight speeds with low damping. Look for the amplitude peak in the amber trace and the change in phase from the first to the last point.
4. Select **HIGH DAMPING** or **AUTO DAMPING**, repeat **SPEED SWEEP**, and select **VERIFY DAMPING** at 45 RPM. The measured peak must fall by at least 50%. The cyan trace shows the comparison sweep.
5. Select **CROSS CRITICAL**. The rotor must cross resonance and hold 75 RPM. Select **COAST DOWN** after the prompt and wait for the rotor to stop. This records objective 5.
6. Select **SENSOR FAULT TEST**, then **START**. At the prompt, undo W15. The controller removes power from both drives and retains the earlier objectives. Reconnect W15, wait for the rotor to settle, select **RESET FAULT**, recalibrate, start at 45 RPM and run **CROSS CRITICAL** again.

## Simulation and bindings

- The shared solver measures the rails, motor winding and electromagnetic actuator. PWM uses a level-local averaged drive circuit. Signal routing validates the wired nets and carries the level’s encoder, probe and command values.
- The lateral model uses 0.65 kg modal mass, 45 N/m shaft stiffness and 60 N/m support stiffness in series, with 0.220 N·s/m passive damping. It integrates rotating imbalance and damping on X/Y at 0.01-second simulation intervals. The resulting natural speed is about 60 RPM. The sweep measures the actual peak independently.
- Each probe reports physical displacement through its own calibrated and filtered signal. RMS and phase are computed over complete Z-indexed revolutions. The damper acts against lateral velocity, with a 32 mN force limit. The normal displacement trip is 2.55 mm. Certified crossing uses a stricter 1.45 mm limit.
- The rotor, flexure bearings, seven shaft sections and encoder follow the simulated motion. The visual orbit exaggeration is independent of the measured displacements. The stationary socket objects have fixed terminal bindings. The supplied animation manifest lists the model pivots.
- Verified milestones bind existing evaluator targets to the solved 5 V supply. The campaign manager evaluates all six targets. No alternate completion path is used.

| Symptom | Check |
|---|---|
| No start | Follow the first W-number on the dashboard, close the guard, and zero both probes. |
| Baseline unavailable | Wait for stable 45 RPM and three valid Z turns. Check W16–W18. |
| Peak reduction below 50% | Select a stronger gain and repeat the comparable sweep. Check W19–W21. |
| Reset unavailable | Restore W15 and let the rotor stop and settle. |
| Still 5/6 | Recalibrate, start at 45 RPM, then cross and hold 75 RPM again. |

The included Play Mode report records the tested wiring, seating, resonance, damping, coast-down and recovery sequence.
