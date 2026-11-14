# Level 60 — The Grid Must Survive

## Install and play

The Project Spark MainMenu scene registers Level 60 after Level 59. In another checkout, merge the package's Assets folder, open MainMenu, run **Tools → Project Spark → Levels → Create Level 60**, and save the scene. The original models, assembled reference, placement script, and art previews are under Reference.

1. Connect W01–W46 from the wiring diagram. Include the safety interlock W46.
2. Calibrate the sensors. Select a 35% reserve, 0.66 W generator startup margin, and AUX → MOTOR → PRI shedding order. Enable AUTO and let the baseline settle.
3. Apply **Cloud Cover**. Watch PV power fall and the utility supply more power. When that test passes, apply **Utility Outage**. QGRID opens and the inverter forms the island.
4. Wait for the backup generator, then apply **Motor Surge**. After the generator contributes power, trigger **Gen Failure**. Verify that QGEN trips, nonessential loads shed and the essential feeder stays powered.
5. Wait for the rotor to stop. Repair the generator and reset the fault. Once the generator resynchronizes, select **Restore Utility** and wait for QGRID to close.
6. Restore motor then auxiliary. After all four loads stabilize, select **Certify**.

## Simulation and layout

The 5 V control rails and player wiring use the Project Spark solver and circuit graph. An educational RMS model calculates 50 Hz bus behavior, measured solar input, MPPT response, bidirectional converter losses, battery discharge and charge, source limits, timed synchronization and load shedding. Reserve time is estimated from usable battery energy and present discharge rate. The battery begins at 76%, with a 19% physical minimum. The generator and utility close after a 0.65 s match within 0.20 Hz, 5 degrees, and 5% voltage. The AC model uses scaled units and a lumped RMS load, with a 2.4 V phase reference.

The workbench fits all 28 kit pieces uniformly. The assembled showcase serves as a reference. TextMesh labels use 0.20 local scale and real sockets snap to the breadboard deck.
