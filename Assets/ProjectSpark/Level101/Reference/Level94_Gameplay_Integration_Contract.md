# Level 94 gameplay integration contract

## Board data and verification authority

| Source file | Intended use |
|---|---|
| `Data/Component_BOM_27.csv` | Part value, footprint and initial uninstalled state |
| `Data/Component_Placement_27.csv` | Geometry-space placement anchor |
| `Data/Logical_Pad_Locations.csv` | 127 physical PCB pad anchors; convert to actual SparkTerminal or compatible pad objects |
| `Data/Copper_Trace_Tasks_W001_W034.csv` | 34 initially absent, player-routed connection requirements |
| `Data/Required_Via_Placements_V01_V06.csv` | Six real through-layer requirements, initially absent |
| `Data/Factory_Existing_Nets.csv` | Allowed permanent PCB copper segments, preexisting without player edits |
| `Data/Critical_W035_Feedback.csv` | One detachable safety-feedback jumper, initially disconnected |
| `Data/Three_Fault_Repair_Scenarios.csv` | Real open via, wrong resistor and MOSFET solder bridge scenarios |
| `Data/Objective_Certification_Plan_6.csv` | Actual objective evidence requirements |
| `Data/Engineering_Acceptance_Tests_56.csv` | 56 acceptance-test *definitions*, not pass results |

## Required Unity integration

1. Inspect the actual Project Spark `SparkElectricalSolver`, `SparkCircuitSystem`, `SparkTerminal`, player wiring, item purchase/placement and `LevelGamePlayManager` APIs. Do not invent signatures.
2. Instantiate **27 real part placements** from the BOM; components start in a shelf or uninstalled inventory, not preconnected in the correct pads. A part placement commits real pin-to-pad relationships only after proper orientation and value validation.
3. Instantiate 34 missing traces and 6 missing vias from source data, not from the decorative gold pad geometry.
4. The copper net graph requires real node IDs and layer-specific routing checks. Player trace intersections on different layers are insulated unless there is a valid via; crossing incompatible nets on one layer is forbidden.
5. Simulate protected 5 V input, 3.3 V regulator, actual microcontroller 2 Hz diagnostic response, I2C temperature sensor, MOSFET load and shunt feedback. Instruments must consume real values and timesteps from the electrical model, not scripted blinking animations.
6. W035 fault: physically break trusted U4 -> U1 monitoring path through J3; external diagnostic display may remain powered, but load drive is inhibited. Never represent an invalid current sample as zero.
7. Preserve saved objective evidence 1–5 after a fault, but require **fresh post-repair test evidence** for Objective 6. Only the real `LevelGamePlayManager` can mark 6/6.
8. Level 94 scene must not change Levels 1–93. Its source art can be projected as **mesh-only** visual copies in the futuristic game-room table.

## Engineering data limitations

The defined netlist is a **gameplay planning schema**, not independently validated KiCad manufacturing data. Microcontroller and analog behaviors are to be modeled using the project's educational simulation. The 14× visual presentation scale never changes the physical PCB's dimensions or electrical behaviors. Inputs are protected virtual 5 V, and gameplay does not instruct the player to construct or use a mains-voltage circuit.

## Source-art caveats

The original geometry and UVs are procedural. Real import dimensions, material matching and pivots require Unity inspection. The supplied C# is optional art/import helper code, and **no existing project's compiled gameplay scripts are being patched**.
