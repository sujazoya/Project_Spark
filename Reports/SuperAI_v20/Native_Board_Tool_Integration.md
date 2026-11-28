# Native board integration

Levels 94, 95, 96, 98, 99, 100, 106, 107, 108, and 110 have authored board routes and part slots. Their process components own board power, placement checks, soldering, layer selection, vias, routing, and diagnostics. The existing Level 100 Play Mode evidence covers one rejected wrong part and one native fit and solder action.

The next implementation task is a typed adapter against the Level 100 process methods and live part and route IDs. Board setup must verify the correct part and orientation, use the player's native fit and solder actions, and verify return values and registered state. The current wire executor cannot execute those board recipes. Level 120 certification remains behind the existing authored hardware and assessment checks.

## Authored controllers

| Level | Native process | Routes | Part slots | Observed player APIs |
| --- | --- | ---: | ---: | --- |
| 94 | SparkLevel94Process.cs | 35 | 27 | FitPart, FitVia, InspectNet, Measure, SolderPart, TestBoard |
| 95 | SparkLevel95Process.cs | 49 | 35 | FitPart, FitVia, InspectNet, SolderPart |
| 96 | SparkLevel103Process.cs | 59 | 44 | FitPart, InspectMode, PlateVia, SolderPart |
| 98 | SparkLevel98Process.cs | 74 | 56 | ConnectPads, FitPart, PlateVia, SelectRoute, Solder |
| 99 | SparkLevel99Process.cs | 82 | 62 | ConnectPads, FitPart, PlateVia, SelectRoute, Solder |
| 100 | SparkLevel100Process.cs | 92 | 70 | ConnectPads, FitPart, PlateVia, SelectRoute, Solder |
| 106 | SparkLevel106Process.cs | 140 | 106 | ConnectPads, FitPart, PlateVia, SelectRoute, Solder |
| 107 | SparkLevel107Process.cs | 148 | 112 | ConnectPads, FitPart, PlateVia, SelectRoute, Solder |
| 108 | SparkLevel108Process.cs | 156 | 118 | ConnectPads, FitPart, PlateVia, SelectRoute, Solder |
| 110 | SparkLevel110Process.cs | 172 | 130 | ConnectRoute, Fit, PlateVia, SelectRoute, Solder |

## v20.1 Level 100 adapter boundary

The native process owns selected footprint, part, orientation, and circuit phase. It can verify fit and solder through its existing guarded methods and expose evidence through the installed count and the authored part contact. The existing ConnectPads method changes the circuit directly through SparkCircuitSystem. A robot adapter that draws a physical lead must use the native wire port and observe the board owner's route acceptance, rather than using ConnectPads as a substitute for motion. The assembly, routing, and diagnostic stages still require an approved action graph.
