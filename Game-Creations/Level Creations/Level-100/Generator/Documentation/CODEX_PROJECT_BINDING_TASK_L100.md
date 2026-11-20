# Level 100 — Connect Generated Layout to Existing Project Spark (Codex Task)

Project root: `D:\Project_Spark_Master_Project\Project_Spark`

First inspect real definitions of `SparkTerminal`, `SparkCircuitSystem`, `LevelGamePlayManager`, current PCB pad/routing/via components, existing Level 99 firmware and virtual repair UI, level registration and save formats. Do not invent their APIs. Keep all new integration classes under the Level 100-specific namespace/assemblies.

Do not write electrical edges from unresolved source `UNASSIGNED` pads/nets/layers. Reconcile a reviewed pin-level schematic/footprint table and create verified netlist documents separately from original data. Map all W001–W092 route IDs, V01–V20 via IDs and W093 physical pin contacts. Validate DRC/ERC and prevent factory art from bypassing player-owned required copper.

After validation, implement real component placement/solder authority, I2C rise-time and three-device transactions, two SPI device paths, UART 115200 8N1 bidirectional tests, firmware/clock/reset, F01/F02/F03 repairs, independent W093 health supervisor with fault latch/explicit reset and fresh evidence, and save/load. Connect objective evaluator to the actual LevelGamePlayManager; perform Unity Edit/Play Mode tests, including Levels 1–99 regressions. Never report 6/6 without actual Play Mode evidence.
