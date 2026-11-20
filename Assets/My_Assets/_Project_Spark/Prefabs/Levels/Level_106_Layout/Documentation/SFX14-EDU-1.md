# First Boot · SFX14-EDU-1

Level 106 uses the supplied SFX-14 board art and the fictional training schematic approved for this level. The source blueprint and unassigned source manifests retain their original status. The training route table in `Data/SFX14_EDU_1_Training_Routes.csv` records all 140 distinct copper tasks, 42 via transitions and the W141 endpoints. Its positions are in source-scale millimeters in the workbench X/Z plane. These are educational service contacts and simplified layers for gameplay.

The existing workshop layout centers the PCB at 10.13 × 7.51 Unity units on the 15.79 × 8.54 tabletop. The active parts use the matching original XY meshes so their board-facing axes match the bare board. The assembled reference stays hidden. The 106th placement provisions the W141 jumper; the player still has to connect it during routing. U14 is a dedicated fictional daughterboard, and U15 uses the original supervisor footprint.

## How the level works

1. Match the source components to their footprints, align pin one and solder.
2. Plate each specified via and connect each pair of highlighted pads on its indicated layer. The native Wire tool and the Connect Pads control both use the same circuit graph.
3. Power up standby and run POST. The console reports solved rail and contact voltages alongside the virtual POST stage. The simplified CPU and memory exercise checks rail tolerances, reset timing and four DDR4 byte lanes.
4. POST checks a game-owned SPI firmware fixture, a modeled PCIe endpoint and a checksummed virtual eMMC boot image. These represent classroom boot stages.
5. Reproduce premature reset, a bridged SPI contact and an open eMMC contact. Isolate power before each repair and run a fresh POST to prove it.
6. Open W141 while the protected output is energized. Reconnect the actual edge, rerun POST, wait for fresh U14 pulses and authorize U15 reset.

`SparkCircuitSystem` owns player wiring and `SparkElectricalSolver` owns voltage results. The six objective evidence bindings are evaluated by `LevelGamePlayManager`. The checkpoint is local to SFX14-EDU-1 and restores construction and repair history with power isolated. A resumed safety trial always requires new boot and heartbeat evidence.

The Unity menus **Install First Boot Training Level** and **Run Gameplay Acceptance** are under **Tools / Project Spark / Levels / 106**. The integration run writes `Logs/Level106Acceptance.txt`. The older Automatic Level Generator menu builds the source art preview.
