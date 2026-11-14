# Level 63 — Catch the Frequency

Open MainMenu and choose Level 63. To install in another checkout, merge the package's Assets directory, run **Tools → Project Spark → Levels → Create Level 63**, and save MainMenu. The supplied kit, manifest and previews are included under Reference.

1. Connect W01–W58 using the illustrated guide. Wire W58 last for the encoder-loss test.
2. Select **Main On** and **Calibrate**. Enable VSM, droop and damping. Select **Charge Rotor** and wait for a stable frequency and sufficient kinetic reserve.
3. Apply **Generator Trip**. Watch the generator breaker open, rotor energy decrease and frequency recover. The required nadir is 49.50 Hz or higher.
4. Wait for the flywheel to recharge, then run **Motor Step** and **Renewable Loss** in order. Cyan plots frequency, amber plots rate of change of frequency, and the red line marks the nadir limit.
5. Select **W58 Test** and remove **ENC OUT → ENC IN**. The flywheel support path opens and the main grid stays connected.
6. Reconnect W58, reset the fault, recalibrate and recharge. Repeat the generator trip and certify.

The level uses Project Spark's 5 V solver and circuit graph for the controls, wire validation and breaker contact topology. A scaled model integrates one grid frequency, a torque-driven flywheel, converter limits and a finite battery reserve. The VSM, droop and damping controls use preset gains. All 34 modules use a uniform workbench fit.
