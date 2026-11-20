# Transient Shield · PIS6-EDU-1

Level 98 uses the original PIS-6 board art and a separate fictional training schematic. The original concept manifests retain their unassigned status. The installed design uses the source component positions and the board’s converted Y-up geometry.

The training route table `Data/PIS6_EDU_1_Training_Routes.csv` enumerates 74 route pairs, 16 plated spans and the separate U8-to-U9 W075 link. Coordinates are source-scale millimeters in the tabletop X/Z plane. The first 56 construction contacts correspond to the 56 source BOM placements, including W075 once. Players connect the W075 link during routing.

## Objective flow

- Fit and solder the 56 BOM parts, route the highlighted copper pairs, and plate the 16 prescribed via barrels.
- Apply the 12 V bench input, wait for the 5 V, 3.3 V and 1.8 V rails, and calibrate the load-voltage and current readings.
- Run a scope capture through three controlled pulses. The traces report solved load voltage and current. Finite line impedance, native capacitor storage and series ESR give the training rail a measurable step response.
- Diagnose and repair the fictional CP2 ESR fault, the V16 return barrel at W056, and W074's Kelvin sense edge. Confirm each repair with a new capture.
- Open W075 under power. U9 opens the load relay while bench diagnostics remain energized. Reconnect the link, recalibrate, reset U9 and run a fresh scope capture.

`SparkCircuitSystem` owns player connections, `SparkElectricalSolver` supplies the scope samples, and `LevelGamePlayManager` evaluates the six objective proofs. The effective capacitor and impedance values are scaled for an observable educational demonstration. The local checkpoint makes resumed power and calibration start isolated.

The **Create Level 98** and **Run Gameplay Acceptance** actions are under **Tools / Project Spark / Levels**. Source art and electrical planning remain available for inspection.
