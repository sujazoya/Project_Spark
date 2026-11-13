# Level 42 — Read the Analog Signal

Wire a 3-bit successive-approximation ADC, convert two unknown signals, diagnose reversed comparator inputs, and verify all eight input bands. The level uses 13 fitted modules, 19 breadboard sockets, and 11 player wires.

## Install

1. Extract the ZIP into the Project Spark Unity project and merge the supplied Assets folder with its metadata.
2. In MainMenu Edit Mode, run **Tools → Project Spark → Level 42 → Install and Register**.
3. Save MainMenu if it already had unsaved changes. The installer registers the definition and teaching drawing in MainMenu and the prefab in the layout catalog.
4. Start Level 42 from the campaign. Level 41 unlocks it in normal progression.

## Play

1. Follow the wiring image or CSV with the Wire tool. Add W10 and W11 last. **DAC TAP** is the B2 branch output. **RET** is the reference return.
2. Press **Test Next Bit**. Keep the trial bit when the sensor is at or above the DAC. Clear it when the sensor is below the DAC. Repeat for B2, B1 and B0, then submit. The input voltage appears after submission.
3. Select **Load Sensor B**, repeat the conversion, then select **Load Fault Test**.
4. Use **Undo Last** twice. Connect **Sensor Out → DAC −** and **DAC Tap → Sensor +**. Test 100 and observe **BELOW**.
5. Undo both crossed leads and restore W10 and W11. Test 100 again for **ABOVE**, then test 110 for **BELOW**.
6. Select **Run Sweep**. It checks the midpoint of each of the eight 0.625 V bands.

| Signal | Input revealed after submit | Trial sequence | Final code |
| --- | ---: | --- | --- |
| A | 3.200 V | 100 keep, 110 clear, 101 keep | 101 |
| B | 1.100 V | 100 clear, 010 clear, 001 keep | 001 |

The level reads the existing solver's DAC and comparator voltages. The four whiteboard pages cover wiring, bit decisions, reversed inputs, and the sweep.
