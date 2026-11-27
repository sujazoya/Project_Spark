# Workflows awaiting native automation

The campaign layouts below contain bound board controllers. Their listed methods are native player operations confirmed in source. Each board still requires a reviewed sequence with its own placement, copper, via, power, and diagnostic prerequisites before Auto Draw can dispatch it.

| Level | Board controller | Authored routes / parts | Native controls identified |
| --- | --- | --- | --- |
| 94 | `SparkLevel94Process` | 35 / 27 | FitPart, FitVia, Repair, RotatePart, SolderPart |
| 95 | `SparkLevel95Process` | 49 / 35 | FitPart, FitVia, Repair, RotatePart, SolderPart |
| 96 | `SparkLevel103Process` | 59 / 44 | Diagnose, FitPart, PlateVia, Repair, RotatePart, RouteCopper, SolderPart |
| 98 | `SparkLevel98Process` | 74 / 56 | BeginFaultTrial, FitPart, LocateFault, PlateVia, Repair, RotatePart, SelectFootprint, SelectPart, SelectRoute, Solder |
| 99 | `SparkLevel99Process` | 82 / 62 | BeginFaultTrial, FitPart, LocateFault, PlateVia, Repair, RotatePart, SelectFootprint, SelectPart, SelectRoute, Solder |
| 100 | `SparkLevel100Process` | 92 / 70 | BeginFaultTrial, FitPart, LocateFault, PlateVia, Repair, RotatePart, SelectFootprint, SelectPart, SelectRoute, Solder |
| 106 | `SparkLevel106Process` | 140 / 106 | BeginFaultTrial, FitPart, LocateFault, PlateVia, Repair, RotatePart, SelectFootprint, SelectPart, SelectRoute, Solder |
| 107 | `SparkLevel107Process` | 148 / 112 | BeginCutoverTrial, FitPart, PlateVia, RepairContact, SelectFootprint, SelectPart, SelectRoute, Solder |
| 108 | `SparkLevel108Process` | 156 / 118 | FitPart, LocateRepair, PlateVia, Repair, SelectFootprint, SelectPart, SelectRoute, Solder |
| 110 | `SparkLevel110Process` | 172 / 130 | Diagnose, Fit, PlateVia, Repair, RotatePart, SelectFootprint, SelectInventory, SelectRoute, Solder |

Level 96 uses a saved layout whose native controller is named SparkLevel103Process. The audit follows the binding from the campaign catalog.

The remaining rows in the final coverage CSV identify missing or incomplete wiring data. Level 20’s reviewed plan requires the existing shelf operation to populate the four resistor workstations before Auto Draw starts.

No board operation is dispatched from an unreviewed recipe. Physical poses, solver readings, and objective results must be verified by the authoritative player tools and level evaluator.
