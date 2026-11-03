# Project Spark — Level 13 Creation Process

**Level:** `level_13` — **Loaded Voltage Divider Challenge**  
**Unity:** existing Project Spark single-scene loader, shelf, circuit solver and universal workstation bridge  
**Import file:** `Level_13_LoadedVoltageDivider_Import.txt`

## 1. What this package does

The TXT replaces the data inside the **original** `Level_13.asset` (same GUID, path, JSON field structure and target count). It **does not create or modify a Unity prefab**, place resistors, connect cables or assign bindings. Perform the following steps in the editor.

## 2. Circuit to build

```text
Battery +5 V -- SW1 -- R1 220 ohm --+--- R2 330 ohm ---+--- Battery -
                                    |                  |
                                    +--- R3 470 ohm ---+
                                    |                  |
                                    LCD A              LCD B
                                   (Vout)             (GND)
```

R1 is **in series with the entire parallel load** (R2 || R3). Do not place R1 parallel with the other resistors as in Level 12. The LCD probes go across the **load**: LCD A to Vout and LCD B to negative/GND. Do not put the LCD in series.

## 3. Calculated expected results (ideal 5 V supply)

| Measurement | Correct 330 ohm R2 | Wrong 150 ohm R2 |
|---|---:|---:|
| Effective parallel load (R2 || R3) | 193.875 ohm | 113.710 ohm |
| LCD Vout | 2.342 V | 1.704 V |
| R1 / supply current | 12.081 mA | 14.983 mA |
| R2 current | 7.098 mA | 11.358 mA |
| R3 current | 4.983 mA | 3.625 mA |

Small deviations from wire/switch resistance are expected. **Inspect the SparkResistor's actual `Resistance` field** for each physical component; do not trust the label. Level 11 previously failed when a resistor named 470 ohm had 330 ohm behavior.

## 4. Import the Level 13 definition

1. Back up the existing `Level_13.asset` and `Level_13_Layout` prefab.
2. Exit Play Mode. Select the **original** `Assets/My_Assets/_Project_Spark/Prefabs/Levels/Level_13_Layout/Level_13.asset`.
3. In your editable TXT importer, import `Level_13_LoadedVoltageDivider_Import.txt` into the selected asset.
4. Confirm `levelId=level_13`, display name **Loaded Voltage Divider Challenge**, four targets and 5 V nominal source.
5. Save assets. The import alone cannot make the topology correct.

## 5. Create the Level 13 prefab

1. Duplicate your existing working level-layout prefab as `Level_13_Layout`, or edit your already-created original Level 13 prefab. Keep its `SparkLevelLayout`, **one primary** `SparkLevelSceneBindings`, battery, switch, LCD and level-workstation infrastructure.
2. Assign `Level_13.asset` to the prefab's `SparkLevelLayout.Level Definition`.
3. Position a fixed R1 **220 ohm** resistor between SW1 output and Vout. Configure its component resistance to 220 ohm.
4. Position a fixed R3 **470 ohm** resistor between Vout and negative/GND. Confirm its actual resistance is 470 ohm.
5. Provide **one replaceable R2 workstation slot** in the R2 parallel branch; its `SparkShelfSlot.Kind` must be **Workstation**, not Storage. Make sure it is registered with the shelf when the layout loads.
6. Use a 150 ohm resistor as the initial *incorrect* installed R2 if you want the challenge to start assembled; otherwise let the player first install it. The correct choice is a 330 ohm shelf resistor.
7. If the prefab came from an older level, remove/retire its obsolete level-specific disposable bindings without deleting permanent battery/switch/LCD references.

## 6. Connect the wires exactly

| # | From | To |
|---|---|---|
| 1 | `Battery_positive` | `SwitchInput` |
| 2 | `SwitchOutput` | fixed `Resistor_220_Input` (R1) |
| 3 | fixed `Resistor_220_Output` (R1) | R2 workstation **input** (Vout) |
| 4 | fixed `Resistor_220_Output` (R1) | fixed `Resistor_470_Input` (R3) |
| 5 | R2 workstation **output** | `Battery_negative` |
| 6 | fixed `Resistor_470_Output` (R3) | `Battery_negative` |
| 7 | `LCD_Terminal_A` | Vout at R2 input / R3 input |
| 8 | `LCD_Terminal_B` | negative/GND at R2 output / R3 output |

The terminal labels in the table are **logical references**; use the real objects, and ensure the physical graph connections exist. The circuit solver should show R1 current equal to the **sum** of R2 and R3 branch currents.

## 7. Configure authoritative scene bindings

**Primary fixed bindings on `Level_13_Layout`:** `Battery_positive`, `Battery_negative`, `PowerSupply_01`, `Resistor_220_Ω`, `Resistor_220_Input`, `Resistor_220_Output`, `Resistor_470_Ω`, `Resistor_470_Input`, `Resistor_470_Output`. Keep any existing permanent switch/LCD binding IDs required by your systems. Create or rename fixed IDs only to match actual objects; Unity names alone do not establish binding IDs.

**Declared runtime IDs:** `R2_Component`, `R2_Input`, `R2_Output`. These are logical IDs and **must not point to a specific shelf resistor prefab**.

On the **original Level 13 layout prefab root**, configure exactly one `SparkLevelWorkstationBindingBridge` rule:

| Bridge field | Value |
|---|---|
| Slot Id | The actual active Level 13 R2 **workstation** `SparkShelfSlot.SlotId` (for example `Register_Slot_2` only if this is actually the one) |
| Component Binding Id | `R2_Component` |
| Input Terminal Binding Id | `R2_Input` |
| Output Terminal Binding Id | `R2_Output` |
| Electrical Type Name | `SparkResistor` |
| Input Terminal Name | `_Input` |
| Output Terminal Name | `_Output` |
| Exact Terminal Names | unchecked |

Click **DECLARE / CHECK RUNTIME IDS IN PRIMARY BINDINGS** in **Edit Mode on the original prefab**, verify three declarations, and save the prefab. Runtime item association then happens automatically. **Do not use storage slot `Placement_01_04` as your workstation slot.**

## 8. Level target configuration

| Target ID | Type | Component / terminals | Pass range |
|---|---|---|---|
| `loaded_divider_r1_conducting` | ComponentConducting | `Resistor_220_Ω` | conducting |
| `loaded_divider_r2_current` | CurrentRange | `R2_Component` | 0.0068–0.0074 A |
| `loaded_divider_output_voltage` | VoltageRange | `R2_Input` vs `R2_Output` | 2.25–2.45 V |
| `loaded_divider_source_current` | CurrentRange | `PowerSupply_01` | 0.0118–0.0124 A |

The output voltage target uses `R2_Input` and `R2_Output`, which are the same two nodes as the R3 load. Your evaluator measures absolute terminal-pair voltage. Source current has `Require Conduction=false`, consistent with the tested Level 11 asset; the actual current-range check remains enforced.

## 9. Test in Unity

- [ ] Open the original Level 13 prefab, validate fixed binding references, save.
- [ ] Verify the Layout Loader is registered to use this prefab for the original Level 13 asset.
- [ ] In Play Mode verify `Level_13_Layout(Clone)` exists and the active level is `level_13`.
- [ ] Verify the R2 bridge log says `source=SLOT_OCCUPANT` and `bound=Resistor_330...` after installing 330 ohm.
- [ ] Inspect resistor actual configured resistances: R1=220, R2=330, R3=470 ohm.
- [ ] Verify R1 in series with the parallel R2/R3 load, not in parallel with them.
- [ ] Verify LCD about 2.342 V, R2 about 7.10 mA, supply about 12.08 mA.
- [ ] Check `LevelGamePlayManager.TargetResults` for all four `true`.
- [ ] Verify current limits on source and no overload/short reported.
- [ ] Save original assets outside Play Mode. Don't save state from a runtime `(Clone)`.

## 10. Troubleshooting without editing source code

- **`SLOT_MISSING`:** bridge `Slot Id` points to a storage slot or an unregistered workstation; inspect `SparkShelfSlot.Kind`, `SlotId`, registration.
- **`item=EMPTY`:** R2 resistor is not installed in the workstation or shelf occupancy is not settled; inspect `Occupant`, `CurrentSlot`, `IsAtWorkbench`.
- **R2 aliases unresolved:** check original prefab runtime ID declarations, exact spelling and active primary bindings.
- **Wrong voltage/current:** check the real resistance values first, then circuit topology and wire connections. Never force the target thresholds to compensate.
- **3/4 targets:** log target result IDs; inspect the actual source current through the source-current audit rather than assuming the first target's 220 ohm current is source current.
- **No level clone:** verify `CanLoadLevel`, primary binding validation, loader registration and progression access. Do not change the solver.

**Scope:** This is an asset-data import and a manual build guide; it does not automatically generate a Unity scene or prefab. No C# changes are needed if the existing universal bridge and evaluator work as they did for Levels 11–12.
