# Level 110 — Voltage Dominion

Level 110 uses **PWR18-EDU-1**, the fictional training schematic authorized for this game level. The PWR-18 kit's original physical route, via and W173 records remain marked unassigned. The generated [training routes](../Data/PWR18_EDU_1_Training_Routes.csv) are the separate educational mapping. They use virtual training pads, authored coordinates, 12 display layers and 56 via spans. Seven native series continuity circuits, with a shared bench feed and return, exercise the source task groups. The independent W173 health receiver has its own native signal edge and the workload has its own native interlock. The four-phase transient model belongs to the training fixture and uses the native bench input and verified path state. The design has no manufacturing or commercial pinout certification.

## Play the level

Open **MainMenu** and use campaign navigation to reach **Voltage Dominion** after **Voltage Guardian**. The level begins with the empty board, 130 available parts, unwired copper and a disconnected W173.

1. Match a shelf part to its footprint. Align the reference mark, fit, flux and solder. Use **Mount cooling** after the last solder joint.
2. Open the selected route. Plate its via when indicated. Use **Connect pads** or the native **Wire tool** with the highlighted pad pair. Complete the 172 routes and connect W173.
3. Apply **12 V input**. Wait for PGOOD, select **Calibrate**, and run **Capture step**. The oscilloscope displays sampled VCORE. Compare the load step and release against the displayed training limits.
4. Use **Inject fault**, capture the fault, and use **Diagnose** for instrument evidence. Discharge the rail before the capacitor ESR check and before rework. Select the diagnosed footprint, repair it, then power up, calibrate and capture again. Repeat for the capacitor, phase-current sense and feedback faults.
5. Run **Protection** to exercise the four protection cases in the diagnostic bench. Capture another passing workload on the repaired board.
6. Break W173 with the rail live, verify that protected workload is isolated, then reconnect W173. Take a fresh qualifying capture, use **Safety reset**, and take a protected load capture to finish.

**Save / Resume** saves placement, solder joints, copper, vias and recorded repair state. On a fresh layout it restores the saved board, then requires new electrical and safety evidence.

## Assets and tooling

- The playable campaign asset is `Level_110_Layout.prefab`, with the adjacent `Level_110.asset` definition.
- The playable meshes are generated from the supplied modular GLBs. The completed model is a separate inactive visual reference.
- The bare-board dimensions are 310 × 230 mm. The runtime fits the visual board to 88% of the limiting tabletop dimension while retaining the design dimensions and native signal topology.
- **Tools → Project Spark → Levels → Create Level 110** rebuilds the playable assets. **Run Level 110 Acceptance** reconstructs the level using ordinary runtime actions and verifies outcomes through the live level manager.

## Verified Unity result

Unity 6000.3.19f1 compiled the project and recorded **79 passing checks** on 19 November 2026. The live manager awarded **6/6** after the final protected load capture. Level 104 loaded afterward, and Unity returned to Edit Mode.

The run verified 130 soldered parts plus cooling hardware, 172 copper routes, 56 plated vias, seven native continuity readings, a native pad-to-pad wire gesture, assembly save and resume, the four-phase VRM, all three faults and repairs, and W173 dropout, reconnection and reset. The live board measured 10.13 × 7.51 scene units on a 15.79 × 8.54 tabletop. The scope visualization retained the measured transient peaks and dips.

The [Unity acceptance log](../../../../../../../../Logs/Level110Acceptance.txt) and [completed board screenshot](../../../../../../../../Logs/Level110Complete.png) record the result. Testing used the Unity editor. Android device testing, player builds and the full release matrix in the supplied brief are outside this run.
