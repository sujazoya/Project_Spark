# Level 62 — Hold the Voltage

Open MainMenu and choose Level 62. In another checkout, merge the package's Assets directory into the project, then use **Tools → Project Spark → Levels → Create Level 62** and save MainMenu. The package includes the original art archive, assembled reference and all supplied companion files.

1. Wire W01–W54 using the illustrated guide. Connect W54 last so **Undo Last Wire** can remove it during the sensor test.
2. Select **Main On**, **Calibrate**, **Check Storage**, and **Precharge**. Lock the PLL and enable the DVR. Enabling the DVR opens bypass before closing the three injection contactors.
3. Apply **Deep Sag**. Compare 0.65 pu incoming voltage with the restored output. Apply **Swell** and watch injection reverse polarity, then test **Unbalance**.
4. Start the **Heavy Motor**. Compare the three incoming and protected voltages during inrush.
5. Select **W54 Test**, then disconnect **INJ V OUT → DVR V IN**. Confirm that injection opens and the protected circuit transfers to bypass. Reconnect W54, reset the fault, recalibrate and lock the PLL. Re-enable the DVR, repeat the sag and certify.

The waveform panel displays source in amber and protected voltage in cyan, with phases A, B and C from bottom to top. The 5 V controls use the shared solver and player wiring. A scaled, per-phase RMS model calculates signed series injection, RC link precharge, capacitor stored energy, conversion losses, motor inrush and temperature. The tests use a 0.95–1.05 pu protected-voltage window. All art modules retain their original relative proportions under the uniform workbench fit.
