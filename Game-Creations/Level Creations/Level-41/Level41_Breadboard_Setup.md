# Level 41 | Build the Analog Output

Build a 3-bit R–2R ladder and compare the three bit weights. The 5 V reference feeds the bit drivers internally. Gold sockets carry the bit signals, mint sockets carry ladder connections, and blue sockets mark the return.

## Wire the breadboard

| Wire | From | To |
| --- | --- | --- |
| W01 | B2 Driver | B2 Branch IN |
| W02 | B1 Driver | B1 Branch IN |
| W03 | B0 Driver | B0 Branch IN |
| W04 | B2 Branch OUT | Link 21 IN |
| W05 | Link 21 OUT | B1 Branch OUT |
| W06 | B1 Branch OUT | Link 10 IN |
| W07 | Link 10 OUT | B0 Branch OUT |
| W08 | B0 Branch OUT | Termination IN |
| W09 | Termination OUT | 5 V Reference RET |

Each branch IN is below its resistor. Each branch OUT is above it. B1 OUT and B0 OUT each take two wires.

## Play to 6/6

| Progress | Action | Expected output |
| --- | --- | --- |
| 1/6 | Finish all nine wires. | Default code **000** reads **0.000 V**. |
| 2/6 | Select **001** and press **APPLY CODE**. | **0.625 V**. |
| 3/6 | Apply **010** and **100**, in either order. | **1.250 V** and **2.500 V**. |
| 4/6 | Apply **101**. | **3.125 V**. |
| 5/6 | Apply **111**, press **OPEN LINK**, then choose **OPEN TERMINATION**. | The same code rises from **4.375 V** to about **5.000 V**. |
| 6/6 | Press **RESTORE LINK**, then **RUN SWEEP**. | All eight codes verify in **0.625 V** steps. The final output is **4.375 V**. |

The model rockers and bargraph show the applied signal. The on-screen bit buttons let you choose the next code before pressing **APPLY CODE**.

## Install in another Project Spark checkout

Merge the included `Assets` folder into the existing project and keep the Unity `.meta` files. Use the existing Level 41 definition, layout catalog entry, campaign entry after Level 40, and teaching drawing. If the MainMenu breadboard geometry changes, run **Tools → Project Spark → Level 41 → Fit Breadboard Assembly** in Edit Mode.
