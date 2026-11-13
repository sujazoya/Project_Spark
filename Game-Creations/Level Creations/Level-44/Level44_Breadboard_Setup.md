# Level 44 — Find Home

The indexing table uses 80 decoded counts per revolution. Clockwise quadrature advances the count by one at each A/B transition, or 4.5° per count. A single Z pulse sets the zero reference. The prewired motor uses the existing SparkMotor solver. The level generates encoder pulses from simulated shaft position and measures their solved voltages at the decoder jacks.

## Install

1. Merge the package's Assets folder into Project Spark. Preserve its asset metadata.
2. In MainMenu Edit Mode, run **Tools → Project Spark → Level 44 → Install and Register**.
3. Save MainMenu if it already contained unsaved changes. The menu registers the definition and teaching drawing in MainMenu and the prefab in the layout catalog.
4. Complete Level 43 to unlock Level 44. The active electrical solver registry needs the existing SparkMotor definition used by Level 30.

## Play

1. Use the Wire tool to place all five wires. Add **W04 and W05 last**.
2. Press **Home** and wait for the Z index to set count zero.
3. Press **Jog CW**, then **Jog CCW**. The count rises and falls. A leads in CW and B leads in CCW.
4. Press **Move A** for +20 counts (90°), then **Move B** for +52 counts (234°).
5. Undo the last two wires. Connect **ENC A → DEC B** and **ENC B → DEC A**. Press **Test Direction**. The table turns clockwise while the displayed count falls. The drive stops after three backward counts.
6. Undo both crossed wires, restore A → A and B → B, and press **Home** again.
7. Press **Move B**, then **Power Off** and **Power On**. The platter stays at B, and the decoder loses its zero reference. Home again and return to B to complete the level.

The four whiteboard pages cover wiring, quadrature and the Z pulse, reversed channels, and recovery after a power cycle.
