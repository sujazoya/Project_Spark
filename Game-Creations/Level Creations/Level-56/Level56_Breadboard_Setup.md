# Level 56 — Harvest the Spin

Merge only the ZIP’s Assets folder into Project Spark. In MainMenu Edit Mode, run **Tools → Project Spark → Levels → Create Level 56**. Save MainMenu if it has pending changes. The archive also includes the supplied reference kit, the exact player wiring, component and terminal bindings, and the original animation map.

## Play

1. Wire the rig using the exact workbench diagram. **Connect W16 last**. It carries capacitor voltage feedback from CAP V OUT to CAP V IN.
2. Close the guard. Select **CALIBRATE**, then **START**. Wait for a stable 90 RPM reference.
3. Select **BEGIN RECOVERY**. Record the first 90-to-20 RPM result at the default high current.
4. Select **OPTIMIZE** to discharge the bank through the resistor. Select **LOW CURRENT**, then **START** from 20 RPM to establish the comparable supply-only run. Perform **BEGIN RECOVERY** again and select **VERIFY EFFICIENCY**. It requires at least 60% measured energy recovery.
5. Select **ENERGY REUSE TEST** at the 20 RPM reference. The stored energy contributes to acceleration and the dashboard reports the external supply work saved.
6. At 90 RPM, select **FAULT TEST** and undo W16. Wait for protected braking and zero speed, reconnect W16, and select **RESET FAULT**. Recalibrate, start, and run **FINAL CERTIFICATION** from the stable 90 RPM reference.

## Simulation and wiring

- The shared solver verifies power rails, guard feedback and the electrical topology. The level-local averaged converter integrates motor torque and back EMF, flywheel motion, capacitor energy and voltage, copper and ESR heat, converter losses and resistor heat. It reports energy supplied to the modeled drive. It does not stamp the internal DC bank or switched semiconductor waveforms into the shared solver.
- Flywheel inertia is 0.105 kg·m². The bank starts at 0 V with 0.80 F equivalent capacitance and 0.075 Ω ESR. The storage ceiling is 4.8 V. The resistor model uses 12 Ω and thermal capacity of 2.6 J/°C. Low and high braking-current commands are 0.20 and 0.48 A.
- Power endpoints that are intentionally shared include supply return, converter +5 V and ground, chopper ground, and the protection positive and negative inputs. The internal motor-enable, guard, bank-protection and chopper contacts are generated in the layout. W16 routes measured state to the controller separately from the bank power paths.
- The A/B/Z encoder produces 80 decoded counts per revolution from simulated shaft motion. The level uses the campaign evaluator and retains the first five verified milestones during feedback loss.

| Symptom | Check |
|---|---|
| First W-number remains on screen | Verify both endpoint IDs in the wiring sheet and remove extra wires. |
| Cannot start | Close the guard, verify the 5 V rails, and calibrate at rest. |
| Efficiency below target | Discharge the bank, select low current, and repeat the comparison. |
| Reset unavailable | Restore W16 and let the flywheel stop. |
| Still 5/6 | Recalibrate, rebuild the 90 RPM reference, and run Final Certification. |
