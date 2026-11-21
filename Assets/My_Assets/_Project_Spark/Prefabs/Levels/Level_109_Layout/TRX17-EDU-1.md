# Level 109 · Thermal Endurance

TRX17-EDU-1 is the fictional campaign circuit for the supplied 300 × 220 mm board. The original unassigned route, via and W165 records document the source art kit. The [training map](Unity/TRX17_EDU_1_Training_Routes.csv) provides explicit playable pad pairs and spans, including the U18-to-U19 W165 connection.

The prefab uses the source’s renderable OBJ geometry and URP materials. The CPU sink and VRM spreader are separate service parts outside the 124-component BOM. The 10.13 × 7.43 Unity-unit board keeps its physical aspect ratio on the 15.79 × 8.54 game table.

| Objective | Required evidence |
| --- | --- |
| Assemble | Verify all 124 placements and both mechanical cooling assemblies. |
| Connect | Install 164 native wires and plate 54 vias across ten layers. Connect W165 independently. |
| Calibrate | Check the powered rails, cold idle sensors and the virtual fan control’s low-to-high response. |
| Endurance | Run a 48-second simulated workload and ambient cycle. Verify the phase, CPU, interface, memory and continuity measurements. |
| Repair | Diagnose excess phase 3 loss at Q05, renew the CPU TIM and remount the heatsink, and service U3’s warm BGA contact. Repeat the same workload after each repair. |
| Restore W165 | Observe protected peak-load inhibition with the diagnostic rails up. Reconnect, requalify, reset U19 and earn a fresh relay permit. |

The thermal RC model advances in fixed 50 ms simulation steps and reads power and current from the existing electrical solver. Its temperatures, workload profiles, and fault thresholds apply to this educational scenario. The finite DDR check reports observed bit errors.

Open **Thermal Endurance** from the Main Menu. **Tools → Project Spark → Levels → Create Level 109** rebuilds the campaign prefab. Run **Levels → 109 → Run Gameplay Acceptance** with MainMenu active to record a Play Mode report and captures under `Logs`.
