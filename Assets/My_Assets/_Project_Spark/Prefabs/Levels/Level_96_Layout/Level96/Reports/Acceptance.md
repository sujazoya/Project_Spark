# Level 103 — final acceptance

Finalized 18 November 2026 in Unity 6000.3.19f1. Campaign identity: `level_103` / Sensor Matrix. Source kit: Level-96/For Creator. Approved educational schematic: SMX4-EDU-1.

## Verified outcome

**84 PASS / 0 FAIL / 0 NOT RUN** in the numbered Unity Edit Mode / Play Mode audit. **6/6** was confirmed by the authoritative live level manager. Unity returned to Edit Mode.

`Acceptance_84.csv` maps every Level 103 test to the original L96-AT identifier, requirement, status and observed evidence. `Acceptance_84.txt` contains the execution log. `Level103_Complete.png` captures the completed live board. Run again by creating `Logs/SparkLevel103Audit84.request` while MainMenu is open in Edit Mode; the runner is `Editor/Level103Acceptance84.cs` with its Edit and Play partial files.

The audit covers exact model/GLB imports, repeated builds, physical scale, placement/orientation rejection, open solder contacts, both routing layers and all vias, native wire input, partial-via persistence, quantitative sensor behavior, ADC quantization/throughput/settling, retained raw history, stale and clipped samples, live probes, touch-position effects, three repairs, W059 latching/recovery, legacy save migration, all six objectives and an existing-Level-102 handoff.

Existing scene files and other level-definition assets remained byte-identical through the audit. The Level 102 gameplay layout loaded after Level 103 unloaded and owned its wire picker correctly. This is representative runtime regression coverage, not a claim that every prior level was replayed.

## Final fixes

- A scene-placed Level 103 preview now deactivates during Play Mode unless it is the campaign-owned layout. It cannot duplicate live UI, camera or wire ownership.
- Master UI binds Current Level Controls specifically to the active layout. The audit verifies close/open against the actual board canvas.
- Component identity must match the selected footprint; wrong orientation and unprepared/energized soldering are rejected.
- Required unresolved/placeholder schematic domains and unauthorized copper cannot certify the board.
- ADC history retains 256 timestamped raw samples per channel, including physical voltage, reference, code and status. A modeled mux slot must settle within half an LSB before code acceptance. Reference drift remains observable in raw codes while engineering qualification is inhibited.
- Save/resume retains partial copper, plating, assembly preparation, selections, calibration context, fault state and historical evidence. Older v1 saves receive the default mux setting, and restored measurements start stale.
- Accepted physical repair effects use the touched contact position through the existing workstation authority.

## Delivery and play

Assets: `Assets/ProjectSpark/Level103`. Prefab/definition: `Prefabs/Level_103_Layout.prefab` and `Prefabs/Level_103.asset`. Menu: Tools / Project Spark / Levels / Create Level 103.

Select Sensor Matrix in MainMenu. Open Master UI / Current Level Controls. Fit, flux and solder the named parts. Select Wire Mode and connect the highlighted amber/cyan pads; plate vias before routing both layers. Inspect Mode returns to physical component inspection. Home returns from Pad View to the board overview. Check reference, capture zero/span standards and run acquisition, then follow the fault and W059 recovery instructions.

Source FBXs imported without geometry, so recovered FBXs were generated from the matching supplied GLBs; originals remain preserved. All 62 modular prefabs are present. The source-contact geometry table records 186 mesh-aligned contacts and one explicitly added RV1 wiper land. J1/J2 artwork pins beyond the two modeled contacts remain decorative NC. Manufacturing coordinates retain the 140 x 95 x 1.6 mm FR4 board, while the visual root fits the existing large table.

## Fidelity boundaries

This is the user-approved generic educational schematic, not a manufacturer pin-compatible hardware design. The 128 Hz converter samples the latest held native Unity electrical solution; front ends, filtering, noise and mux settling are educational equivalents rather than transistor-level continuous-time SPICE. The existing wire-current bridge remains fail-closed for unsupported device-current visualization. No current is fabricated for those paths.

Passing the 84 listed software cases does not establish manufacturing clearance/DRC, exhaustive testing of every possible wrong-part combination, hardware validation, or an Android player build. Those were not performed. Copper artwork represents the authoritative endpoint/layer topology rather than manufacturing-ready PCB routing.
