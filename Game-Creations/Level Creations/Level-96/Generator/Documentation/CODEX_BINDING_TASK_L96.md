# Codex Unity Project Spark — Level 96 binding task

Project: `D:\Project_Spark_Master_Project\Project_Spark` (Unity 6.3 URP).

1. Inspect the actual declarations and integration APIs in the project for `LevelGamePlayManager`, `SparkCircuitSystem`, `SparkTerminal`, component placement, existing PCB routing/via tools, inventory/shelf/shop, saved game state, and world-space virtual instrumentation.
2. Compile the new `SparkAutoL96*` builder/records in-place under the existing generator, inspect `Level_96_Layout.prefab` and validate all 44 placements, 187 contact records, 58 routes, 12 vias, 3 faults, 6 objectives.
3. Verify OBJ import and material lookups; ensure all original 14 PBR material families are assigned and apparent source dimensions are correct before accepting art QA.
4. Review source data rather than inventing connections. A pin-level schematic for all 187 contacts is **not supplied**. All 58 conceptual routing tasks and 12 vias remain **NOT_CERTIFIABLE** until approved endpoints/nets are provided. W059 is logical feedback U8→U9, not the 2.500V reference supply.
5. Bind only through real gameplay/electrical APIs; do not count renderers, colliders, labels or visual-only repair effects as electrical success.
6. Implement four sensor measurement models, calibration and ADC where needed, but do not claim demonstrated Play Mode accuracy without tests.
7. Run EditMode and PlayMode regression tests, save/load checks, initial 0/44, 0/58, 0/12 and objectives 0/6, W059 disconnected; write exact errors and blockers.
8. Preserve existing levels and users' tabletop and environment; no new scene furniture or physical instruments.
