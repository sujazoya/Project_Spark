# Level 40 | The Long Run

Build a 4–20 mA series loop and compare the actual tank level with the level reported by the 250 Ω receiver. Follow the top row from left to right, down to the line break, and back through the receiver.

## Wire the breadboard

| Wire | From | To |
| --- | --- | --- |
| W01 | 12 V Supply LOOP + | Transmitter TX IN |
| W02 | Transmitter TX OUT | Cable IN |
| W03 | Cable OUT | Line Break IN |
| W04 | Line Break OUT | Receiver RX IN |
| W05 | Receiver RX OUT | 12 V Supply RETURN |

Follow the printed **IN** and **OUT** labels. The bottom row carries current from right to left, so the line break and receiver each have their IN socket on the right.

## Play to 6/6

| Progress | Action | Expected result |
| --- | --- | --- |
| 1/6 | Finish the five wires. | Tank 0%, loop 4.00 mA, receiver 1.00 V. |
| 2/6 | Press **50% TANK**. | Tank and receiver both show 50%. Loop 12.00 mA, receiver 3.00 V. |
| 3/6 | Press **FIT LONG CABLE**. | Cable becomes 350 Ω. At 50%, the receiver still reads 3.00 V. |
| 4/6 | Press **100% TANK**. | The transmitter reaches its headroom limit. The tank reads 100%, while the receiver reports 79% at 4.17 V. |
| 5/6 | Press **FIT SHORT CABLE**. | Cable returns to 50 Ω. The receiver reaches 100% at 5.00 V and the loop reaches 20.00 mA. |
| 6/6 | Press **OPEN LINE**, then **CLOSE LINE**. | The lever and lamp show the break. Receiver voltage falls to 0.00 V, then returns to 5.00 V when the line is restored. |

## Install in another Project Spark checkout

Merge the included `Assets` folder into the project and keep the Unity `.meta` files. Use the existing Level 40 campaign entry, layout catalog entry, and teaching drawing. If the MainMenu breadboard geometry changes, open MainMenu in Edit Mode and run **Tools → Project Spark → Level 40 → Fit Complete Display Assembly**.
