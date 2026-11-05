# PROJECT SPARK — LEVEL 19 CREATION PROCESS
## The Loading Effect (Loaded Voltage Divider)

**Design status:** Implementation specification, not an import-ready `SparkLevelDefinition` asset. To produce an editable Unity asset file, export your original `Level_19.asset` with the Project Spark SO TXT exporter and update its serialized JSON while preserving asset identity.

## 1. Learning goal
Build upon Level 18's 5 V, 330 Ω / 470 Ω divider by connecting a 1 kΩ load *in parallel* with the 470 Ω lower resistor. Observe the output decrease from ~2.9375 V to ~2.4605 V and the source current increase from 6.25 mA to ~7.6955 mA.

## 2. Circuit topology — exact connections
```
PowerSupply (+) → Power_Switch.Input
Power_Switch.Output → R_TOP_Input
R_TOP_Output → VOUT junction
VOUT junction → R_BOTTOM_Input
VOUT junction → R_LOAD_Input
R_BOTTOM_Output → PowerSupply (−) / GND junction
R_LOAD_Output → PowerSupply (−) / GND junction
LCD Terminal A → VOUT junction
LCD Terminal B → GND junction
```
The LCD must measure voltage with sufficiently high impedance. Never connect VOUT to ground through an ordinary wire; that shorts the output. The 1 kΩ load is a resistor, not an ordinary wire.

## 3. Theory (ideal)
- R_BOTTOM || R_LOAD = (470 × 1000)/(470 + 1000) = 319.727891 Ω.
- Total equivalent resistance = 330 + 319.727891 = 649.727891 Ω.
- Supply and top resistor current = 5/649.727891 = 0.007695529 A = 7.6955 mA.
- VOUT = 2.460475 V.
- Top resistor voltage = 2.539525 V.
- Bottom resistor current = 2.460475/470 = 0.005235054 A = 5.2351 mA.
- Load resistor current = 2.460475/1000 = 0.002460475 A = 2.4605 mA.
- Check: 5.2351 + 2.4605 = 7.6955 mA.

## 4. Level definition
- Level ID: `level_19`
- Display name: `The Loading Effect`
- Description: `Build a 5 V voltage divider with 330 Ω on top and 470 Ω at the bottom. Connect a 1 kΩ load in parallel with the bottom resistor. Measure approximately 2.46 V at the midpoint and verify the branch currents.`
- Completion mode: all required targets (existing completionMode 0, after verifying enum in your project)
- RequiredTargetCount: 5
- RequireClosedReturn: true
- RejectShortCircuit: true
- RejectTargetShort: true
- AllowIntermediateConnections: true
- RequireMinimumVoltage: false
- AllowAnyConfiguredPowerSource: true
- RequiredHealthyResistorIds: []
- RequireHealthyInstalledResistors: false (unless the intended evaluator behavior is verified across levels)
- UnlockNextLevelOnCompletion: true
- AutoUnlockNextLevel: true
- AutoAdvanceOnCompletion: false
- Source: `PowerSupply_01`, terminals `Battery_positive` and `Battery_negative`, nominal 5 V.

## 5. Five proposed SparkLevelTarget entries
These use the existing `VoltageRange=5` and `CurrentRange=6` target types. `RequireElectricalEnabled=true`; `RequireConduction=true` for resistor currents. For voltage, set `RequireConduction=false` unless the target is configured to read the component directly. `useMaximumVoltage/useMaximumCurrent=true` on each corresponding target.

| Target ID | Type | Component ID | Terminal pair | Minimum | Maximum |
|---|---|---|---|---:|---:|
| `level19_loaded_output_voltage` | VoltageRange (5) | `R_BOTTOM_Component` | `R_BOTTOM_Input` / `R_BOTTOM_Output` | 2.35 V | 2.57 V |
| `level19_top_current` | CurrentRange (6) | `R_TOP_Component` | `R_TOP_Input` / `R_TOP_Output` | 0.0072 A | 0.0082 A |
| `level19_bottom_current` | CurrentRange (6) | `R_BOTTOM_Component` | `R_BOTTOM_Input` / `R_BOTTOM_Output` | 0.0049 A | 0.0056 A |
| `level19_load_current` | CurrentRange (6) | `R_LOAD_Component` | `R_LOAD_Input` / `R_LOAD_Output` | 0.0022 A | 0.0028 A |
| `level19_source_current` | CurrentRange (6) | `PowerSupply_01` | `Battery_positive` / `Battery_negative` | 0.0072 A | 0.0082 A |

These numeric conditions demonstrate loading but do not independently guarantee exact resistor nominal values. If necessary, add resistance-specific tests to your evaluator.

## 6. Unity prefab creation
1. Exit Play Mode.
2. Duplicate the original `Level_18_Layout` prefab and save as `Level_19_Layout`.
3. Duplicate/export the `SparkLevelDefinition` asset as `Level_19.asset`, ensuring unique level ID `level_19` and correct metadata. Do NOT import Level 18's TXT into the Level 19 asset unmodified.
4. Reuse the 5 V supply, switch, LCD, R_TOP 330 Ω workstation and R_BOTTOM 470 Ω workstation.
5. Add a third resistor workstation `Register_Slot_LOAD`, set `Category=Resistor`, `Kind=Workstation`, `Accept Any Category=false`.
6. Put the load station physically next to the bottom resistor with sufficient room for two wires branching from VOUT and merging at GND.
7. Configure a 1 kΩ `SparkShelfItem` for the load (or leave the slot empty at level start and require the player to install it).
8. Ensure each installed item's `CurrentSlot` and its slot's `Occupant` agree and `IsAtWorkbench=True` when installed.
9. Connect the circuit precisely as in Section 2; initially turn the supply OFF.

## 7. Runtime scene binding declarations
**On the original Level 19 prefab's `SparkLevelSceneBindings`:**

Runtime Component IDs:
```
R_TOP_Component
R_BOTTOM_Component
R_LOAD_Component
```
Runtime Terminal IDs:
```
R_TOP_Input
R_TOP_Output
R_BOTTOM_Input
R_BOTTOM_Output
R_LOAD_Input
R_LOAD_Output
```
No runtime electronic-object IDs are required for resistor-only slots.

## 8. Workstation mapping (critical)
If using **Smart Slots**, configure the `SparkShelfSlot` component itself. Smart-slot rules override legacy manual bridge entries for the same SlotId.

| Slot ID | Component ID | Terminal A ID | Terminal B ID |
|---|---|---|---|
| `Register_Slot_RED` | `R_TOP_Component` | `R_TOP_Input` | `R_TOP_Output` |
| `Register_Slot_GREEN` | `R_BOTTOM_Component` | `R_BOTTOM_Input` | `R_BOTTOM_Output` |
| `Register_Slot_LOAD` | `R_LOAD_Component` | `R_LOAD_Input` | `R_LOAD_Output` |

Choose ONE consistent strategy:
- Smart Slots ON: `SparkLevelWorkstationBindingBridge.UseSmartSlots=true`, and enable/configure smart binding on each slot.
- Legacy mappings: `UseSmartSlots=false`, with three manual Workstation entries matching the above IDs and the actual terminals' `_Input` / `_Output` suffixes.

Do not leave Smart Slots ON with outdated IDs on `SparkShelfSlot`: the bridge ignores matching legacy entries.

## 9. Player objectives
- Bring the loaded output into 2.35–2.57 V.
- Achieve 7.2–8.2 mA through R_TOP.
- Achieve 4.9–5.6 mA through R_BOTTOM.
- Achieve 2.2–2.8 mA through R_LOAD.
- Achieve 7.2–8.2 mA source current.

## 10. Play-mode verification
1. Before adding load, verify unloaded output ~2.94 V, source current ~6.25 mA; loaded targets should not all pass.
2. Power OFF and install/connect 1 kΩ across VOUT and ground.
3. Power ON and run `SparkSourceCurrentAudit`. Look for R_TOP ~7.696 mA, R_BOTTOM ~5.235 mA, R_LOAD ~2.460 mA, source ~7.696 mA; LCD ~2.460 V.
4. In the active level's `SparkLevelWorkstationBindingBridge`, run `Project Spark / Synchronize AND REPORT`.
5. Confirm all three components and their six terminal IDs are BOUND/RESOLVED.
6. Enable `[LEVEL TARGET RESULT]` diagnostics briefly. Each of five targets should pass.
7. Confirm closed return YES, source short NO, target short NO, Targets 5/5, Completed.
8. Disable verbose diagnostics after testing to prevent Console spam.

## 11. Guardrails
- Never edit Level 18 to create Level 19. Preserve previous levels.
- The fixed `SparkLevelEvaluator` already compares magnitude for current and voltage range targets; do not change its shared behavior without regression tests.
- Use component/terminal bindings, not the physical GameObject names, in the target entries.
- Meter input is a voltage measurement, not a conductive return path.
- `SparkSourceCurrentAudit`'s old Level 11 theoretical check is irrelevant to this circuit.
