# Level 58 — Isolate the Fault

## Install and play

Open Project Spark’s MainMenu scene. The level layout, definition, and four-page teaching drawing are included. In another checkout, merge the package's Assets directory into your project, then use **Tools → Project Spark → Levels → Create Level 58** and save MainMenu. The installer adds the existing assets to the level manager, layout catalog, and teaching drawer. The package includes the source art archive and its companion files under Reference.

1. Wire W01–W38 from the illustrated wiring guide. Connect W16 last so Undo Last Wire can remove it for the final test.
2. With the guard closed and 5 V on, select **Calibrate CTs**. Choose 0.34 A feeder pickup, Standard Inverse, TMS 0.075, and 1.55 A high-set. Verify settings, arm the relays, then close Q0–Q3.
3. Run **A Overload**. Watch Q1 open while Q0, Q2, and Q3 stay closed. Clear the test, reset the latch and reclose.
4. Run **B Short**. Watch Q2 open while the healthy feeders stay on. Clear, reset and reclose.
5. Run **Q2 Failure**. Wait for the failed Q2 request and delayed Q0 backup. Clear the test, service Q2, reset and reclose.
6. Start **W16 Test**. Remove CTB OUT → R2 I and wait for Q2 to open. Reconnect, reset, recalibrate, verify, arm and reclose. Run B Short again, then clear, reset, reclose and certify.

## Simulation and fit

The Project Spark solver supplies the 5 V control rails. The player wiring uses the shared circuit graph and the four breakers actuate twelve real SparkSwitch contacts. The level models feeder RMS with a 2.4 V phase reference, 50 Hz, 0.15 Ω shared equivalent source impedance and independent resistive branches. The CT ratio is 20:1, with 0.25 A equivalent secondary saturation and filtered primary measurements. Loads and faults use the corresponding branch impedances. The main backup waits 0.42 s after detecting persistent current and closed Q2 feedback. Breaker opening takes a further 0.085 s. The simulated RMS network is a teaching approximation with internal fixed module connections. W01–W38 are the external wires.

The installer fits all 24 models as one proportion-preserving assembly. It seats the playable sockets on the existing breadboard, and applies 0.20 local scale to TextMesh labels. Source art stays available in its original package.
