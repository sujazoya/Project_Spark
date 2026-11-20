# Level 104 — Voltage Guardian

Source: the user's Level-97 VGR-5 board kit. Runtime identity: `level_104`.

The user explicitly authorized the generic **VGR5-EDU-1** educational schematic. The original route and via CSVs remain unchanged. Resolved pin/net records, layer escapes and manufacturing assumptions are separate generated files under `Data/`. These assignments do not claim compatibility with a commercial regulator IC.

## What the player does

1. Start with the empty board. Select the named footprint, fit its matching component, apply flux and solder. Align pin 1/polarity. Complete all 50 parts; W067 counts once.
2. Select each W001–W066 task. Plate its two source through-vias when requested, then draw each highlighted segment or use **Route Segment**. The pending layer is selected automatically. Layer/cutaway controls expose inner copper. Install the separate W067 link for commissioning.
3. Apply **12 V Power**, then enable **5 V**. Wait for the panel to show that **3V3** is ready, enable it, then repeat for **1V8**. Use **Measure Rails** for a fresh solver reading.
4. Run **Test 3 Loads**. Light, nominal and peak loads must pass voltage, ripple, efficiency and thermal checks. Edits and unsafe readings cancel the window.
5. Inject each fault separately. Isolate input before measuring R7 with the multimeter or checking continuity across the V05 barrel. For Q1, take a thermal reading after the temperature rises, then isolate input. Repair, restart the full sequence, measure and run all three loads after each repair.
6. Arm W067 recovery. Remove W067 using **Undo / W067**. Confirm missing/unknown trusted feedback and inhibited loads while the physical rail remains. Reconnect, measure rails again, explicitly reset safety and run a new qualification.

The **Current Level Controls** panel stays at the lower left in Board View and Pad View. **Home** returns to the board. Middle drag pans, right drag orbits, and the wheel zooms. **Pad View** focuses the current contacts. **Wire Mode** enables native terminal dragging. **Inspect Mode** restores component and tool picking. The colored contact markers identify the pending terminals.

## Source discrepancies and explicit adaptations

- Source includes 68 modular models plus one assembled reference: 69 FBXs and 69 GLBs. The reference is never the gameplay graph.
- Unity imports the original FBXs without renderers. Visible Unity-compatible derivatives were recovered from the matching GLBs in `Models_Rebuilt`; user source files are preserved.
- GLBs contain vertex colors rather than material-family slots. `Recovered_Material_Map.csv` matches those colors exactly against the source palette. Generated URP materials use albedo, normal, packed metallic/smoothness, AO and restrained LED emission.
- Actual textures: 21 PBR families × five maps = 105, plus five board/layer maps = 110. The README's 20-family claim differs from the palette and summary.
- Board remains 160 × 110 × 1.6 mm in manufacturing coordinates. A separate visual transform fits the existing room table. Electrical values and the engineering clock do not scale with it.
- Source component placement Y differs between manifest and README. Contact reference is normalized to 0.00092 m and aligned to recovered solder/contact geometry.
- C1, C2 and C8 have no exported leads. Separate documented educational radial-capacitor lands were added at their footprints.
- Twenty/14-pin IC positions are generic educational positions. Unused pins are explicitly NC. MOSFET assignments are educational G/D/S positions anchored to actual exported legs.
- All 66 source route rows are unverified placeholders; all 14 via nets are originally unassigned. The original records are retained beside the resolved educational records.
- L2/L3 factory blind via-in-pad escapes and isolated routing channels were authored because a single top-layer layout failed clearance checks. They are factory geometry, not additional player via tasks. The 14 supplied L1–L4 through-vias remain separately plated tasks. Their internal lands follow the actual plating state. V05 remains in the main supply path.
- Player trace centerlines undergo an independent unlike-net polyline clearance check. This educational check is not a complete commercial PCB fabrication/EMC qualification; ideal factory plane branches and component internals are explicitly modeled separately.

## Authority and simulation

`SparkCircuitSystem` owns wire connectivity. Native terminals, solder-contact switches, via contacts, sources, resistors, capacitors, diodes and LEDs participate in `SparkElectricalSolver`. Native node readings feed the objectives. A separate SI-unit averaged LC converter models duty, inductor current, capacitor voltage, finite secondary startup, feedback resistance, ripple, input-power-equivalent loss and Q1 thermal response. R7, V05 and Q1 change real model parameters or graph edges.

W067 carries trusted logic, never physical load current. A missing link invalidates its quality and latches safety; the load switch opens while independent rails remain available. Reconnection alone cannot complete recovery. Objective history is timestamped, and fresh live qualification remains separate from history.

Solder/repair sparks and generated audio cues present accepted actions only. They cannot change circuit or objective authority. Model rendering is combined by verified material family while original mesh/contact records remain inspectable.

Save/load rebuilds physical contacts, route edges, via states and faults. It rejects malformed state and invalidates live measurement and safety-reset evidence after reload.

## Unity acceptance

The 19 November 2026 run in Unity 6000.3.19f1 recorded **161 passing checks and zero failures**. The live level manager confirmed **6/6**, and Unity returned to Edit Mode after the campaign handed the active layout and wire picker to Level 103 and then Level 102.

The run exercised the startup dwell, native copper placement, partial-route save/resume, all three load cases, instrument-gated repairs, and W067 loss and recovery. The native load switch opened and measured load current fell below 1 mA while the independent 5 V rail stayed available. The controls were checked in Pad View and the completed Board View was visually inspected. An Android player build was outside this run.

All 180 prior-level baseline files matched their recorded content hashes, including the Level 103 definition at its new path under the shared prefab tree.

The [acceptance log](../../../../Logs/SparkLevel104Acceptance.txt) and [completion screenshot](../../../../Logs/SparkLevel104Complete.png) capture the run. The numerical plant, electrical-rule, and copper-clearance reports are in this directory.
