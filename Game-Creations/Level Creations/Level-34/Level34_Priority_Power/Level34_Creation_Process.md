# Level 34 — Priority Power

Wire a shared bus. Use the demand bank to cause a sag, watch the auxiliary fan shed, and restore it manually after the critical pump has recovered.

## Generate and register

1. If the scripts are not yet installed, copy the runtime script into a runtime folder under `Assets` and both editor scripts into an `Editor` folder. Let Unity compile.
2. With Play Mode stopped, run **Tools → Project Spark → Level 34 → Create Level Assets**. It creates `Level_34.asset`, `Level_34_Priority_Power_Layout.prefab`, `Level_34_Drawing.asset`, and materials in `Assets/My_Assets/_Project_Spark/Prefabs/Levels/Level_34_Layout/`. The workbench uses Level 33's placement, with Level 32 as a fallback.
3. Pair the definition and prefab in the active **Level Layout Catalog**. Append the definition after Level 33 in **LevelGamePlayManager → Levels**. Keep **Auto Evaluate** enabled. Use the game's test unlock or complete Level 33 to unlock Level 34.
4. Add the drawing to **Spark Draw Current Level → Drawings**. Its four pages cover the feeder, branch wiring, sag and recovery, and the six checks.
5. In Play Mode, use the existing Wire tool and connect the preinstalled components. The layout has 14 labeled sockets and fixed bindings.

## Exact wiring

| Wire | From | To |
|---|---|---|
| W01 | `Battery_positive` | `FEED_IN` |
| W02 | `FEED_OUT` | `Pump_A` |
| W03 | `FEED_OUT` | `AUX_IN` |
| W04 | `FEED_OUT` | `DEMAND_IN` |
| W05 | `AUX_OUT` | `Fan_A` |
| W06 | `DEMAND_OUT` | `R_DEMAND_IN` |
| W07 | `Pump_B` | `Battery_negative` |
| W08 | `Fan_B` | `Battery_negative` |
| W09 | `R_DEMAND_OUT` | `Battery_negative` |

Three wires share **FEED OUT**, which is BUS. Three wires share **Battery −**. IN and A are the left sockets. OUT and B are the right sockets.

## Six checks

| Progress | Action | Nominal reading |
|---|---|---|
| **1/6** | Finish wiring. The critical pump powers up by itself. | BUS ≈ 4.12 V, pump ≈ 88 mA. |
| **2/6** | Press **CONNECT AUX**. | BUS ≈ 3.51 V. Pump and fan ≈ 75 mA each. |
| **3/6** | Press **APPLY DEMAND**. | The recorded SAG reads ≈ 3.05 V. Below 3.25 V, the controller opens the fan contact. |
| **4/6** | Leave demand connected until recovery is verified. | BUS ≈ 3.51 V. Pump and demand ≈ 75 mA each. Fan below 1 mA. |
| **5/6** | Press **REMOVE DEMAND**. | BUS ≈ 4.12 V. AUX remains shed and fan current stays below 1 mA. |
| **6/6** | Press **RESTORE AUX**. | The fan resumes and BUS settles near 3.51 V. |

The **SAG** meter retains the lowest pre-cutoff reading used by the trip, so you can compare it with the recovered BUS voltage. **RESTART TEST** opens both controlled branches and resets the sequence. Editing the wiring also resets the sequence.

## Electrical settings and validation

- 5 V source with a 0.35 A current limit. The 10 Ω feeder drops more voltage as total branch current rises. It is rated 1 W. The 47 Ω demand resistor is rated 0.5 W.
- Both motors use 47 Ω windings and the existing signed motor solver, with a 2.70 V startup threshold. The pump can continue turning during the sag, while its voltage recovers when the fan is shed.
- The cutoff latches at a measured bus voltage below 3.25 V. Manual restore requires the demand contact open and BUS at least 3.90 V. The controller queues the level evaluator whenever an objective is registered.
- If the pump never runs, check W01, W02, and W07. If the fan keeps running after cutoff, look for a wire that bypasses AUX. If voltage never sags below 3.25 V, confirm the 10 Ω feeder and the 47 Ω demand branch are wired as shown.
- If the motor solver is missing, check the active registry for `Level30_MotorSolver.asset`. If wiring clicks miss, check that the wire tool includes the `Pin` layer. The generated panel uses the existing EventSystem.
