# Level 101 · The Hidden Layers

MLX9-EDU-1 is the fictional training schematic for the packaged 230 × 160 mm, six-layer MLX-9. It uses all 76 original component poses. The board display scales to about 10.13 × 7.05 Unity units on the 15.79 × 8.54-unit workbench.

| Source item | Training assignment |
| --- | --- |
| W001–W100 and V01–V24 | The [training route map](../Data/MLX9_EDU_1_Training_Routes.csv) records named native terminals, layers, and coordinates in millimetres. |
| W101 monitor | U7 `TRUSTED_LINK_HEALTH_OUT`, following the BOM's `LINK_MON` role. |
| W101 supervisor | U8 `TRUSTED_LINK_HEALTH_IN`, following the BOM's `SAFETY` role. |
| F1 · Internal open | W035 uses the V09 L1–L3 signal barrel for U1 to U2 SPI clock. Rework restores continuity and requires a fresh flash/RAM test. |
| F2 · Return discontinuity | W073 measures the L5 stitch through a 180 Ω excitation. Opening the stitch forces the alternate return and produces a measurable increase in resistance and modeled ringing. |
| F3 · Pair skew | Reworking W053 removes the added Lane A meander while retaining the native endpoints. The capture calculates delay from the changed route geometry. |

The original W101 file uses U10 and U11. The BOM identifies them as the LDO and interface. The fictional circuit uses U7 and U8, and retains the original JSON and unassigned netlist tables as source references.

The six objectives verify assembly, routing, MCU and memory commissioning, signal integrity, all three repairs, and W101 recovery. The native solver reports rail voltages, SPI clock levels, return voltage and differential receiver polarity. The link instrument samples the modeled TDR and eye and records observed errors over a 4,096-bit training interval per lane. The final W101 test requires actual disconnection, fresh pulses and a qualifying capture after reconnection, explicit U8 reset, and a new training epoch and capture.

The campaign/save ID is `level_101_hidden_layers` so the older `level_101` definition has a separate progress record. The `Tools > Project Spark > Levels > 101 > Run Gameplay Acceptance` editor command exercises the playable MainMenu entry and writes its report and screenshots under `Logs`.
