# Codex: Level 99 full gameplay integration (source-aware, no guessed APIs)

Project root: `D:\Project_Spark_Master_Project\Project_Spark`.

1. Open the actual Project Spark Unity source; inspect and document signatures for `SparkTerminal`, `SparkCircuitSystem`, network/solver, PCB component/trace/via systems, `LevelGamePlayManager`, shop/shelf, World-Space Canvas virtual repair tools and saving. Detect namespaces/asmdefs. Never introduce duplicate global services.
2. Compile the Level 99 generator/visual scene. Verify scale, axes, material slot remapping, component poses and the reference board's disabled state. Do not claim Unity-tested results without actual compilation logs.
3. Produce an **explicitly proposed and reviewed** MCU/PCB pin-level schematic and footprint map. Reconcile all 82 W001–W082 route endpoints, V01–V18 positions/nets/layers and W083 physical pins. Keep original source manifests unmodified. Report `BLOCKED_UNVERIFIED_NETLIST` until approved.
4. Bind 62 component visual targets to real install/solder transactions and actual part identity; keep visual enlargement distinct from solver units. Implement real DRC/topology for factory and player copper.
5. Implement finite, deterministic virtual power/clock/reset (protected 5 V in, regulated 3.3 V, nominal 8 MHz reference, 48 MHz configured PLL), curated firmware integrity/SWD programming and GPIO/timer/I2C/SPI/watchdog behavioral diagnostics. Do not claim full MCU instruction-set emulation.
6. Implement physical fault F01 oscillator capacitor Y1/C1, F02 SPI V11 open and F03 `R_RESET` pull-up. The source does not resolve the particular R_RESET BOM ref; require schematic review before assigning it.
7. Implement W083 independently as a digital heartbeat monitor: install, qualify edges, supervised disconnection, protected output inhibition, fault latch, physical reconnect, fresh transitions, explicit reset, new boot/diagnostics. No fake rail-off effect on lost heartbeat.
8. Bind existing real Project Spark click/touch tools; only authority changes may drive VFX/telemetry. Add persistence and level objectives with fresh evidence; run all source-proposed AT99-001...086 tests, report PASS/FAIL/SKIP with evidence and regression checks for Levels 1–98.

Do not award objectives from a scene preview, the complete-board reference, speculative netlists or UI buttons. All real project integrations and Play Mode certification remain pending until implemented and tested.
