# PROJECT SPARK — LEVEL 10 COMPLETE CREATION CANVAS

## Bridge Balance Challenge · Wheatstone Bridge

**Status:** Build blueprint; not runtime-verified. **Level ID:** `level_10` · **Supply:** 5 V DC · **Objectives:** 4 · **Completion:** All Targets · **Difficulty:** 4/5.

> **Architecture:** One persistent gameplay scene; create a new `SparkLevelLayout` prefab, a `SparkLevelDefinition` asset, and one `SparkLevelDrawingAsset`. Reuse existing solver, shelf, LCD, UI, evaluators, and the `SparkLevelLayoutLoader`. Do not add a new gameplay scene or teaching MonoBehaviour.

## 1 · Success condition

The player replaces the incorrect **220 Ω upper-right R3** with **100 Ω**. The two divider midpoint voltages become equal and the DC-voltage LCD connected between the midpoints reads approximately **0.000 V**. Four existing evaluator targets validate the correct circuit without requiring a new bridge-specific evaluator.

### Intended topology

```text
                           +5V BUS (after power switch)
                                      |
                        +-------------+-------------+
                        |                           |
                  R1 220Ω FIXED              R3 100Ω SHELF
                        |                           |
                    LEFT MID                    RIGHT MID
                        |                           |
                  R2 330Ω FIXED              R4 150Ω FIXED
                        |                           |
                        +-------------+-------------+
                                      |
                              Battery_negative

                LCD A --> LEFT MID      LCD B --> RIGHT MID
                         (DCVoltage, high impedance)
```

**IMPORTANT:** There is NO direct wire between LEFT MID and RIGHT MID: only the LCD's high-impedance input connects them. Both R2 and R4 independently return to battery negative. Both upper resistors connect to the switch output.

## 2 · Nominal calculations

| Measurement | Correct R3 100 Ω | Wrong R3 220 Ω |
|---|---:|---:|
| Left midpoint, `VL` | 3.000 V | 3.000 V |
| Right midpoint, `VR` | 3.000 V | 2.027 V |
| LCD `VL − VR` | ≈ 0.000 V | ≈ +0.973 V |
| Left-branch current | 9.091 mA | 9.091 mA |
| Right-branch current | 20.000 mA | 13.514 mA |
| Total supply current | 29.091 mA | 22.604 mA |

`VL = 5 × 330/(220+330)`. `VR = 5 × 150/(R3+150)`. Balanced condition `R1/R2 = R3/R4 = 2/3`. Nominal numbers assume an ideal 5 V source and high-impedance LCD; the electrical solver's results are authoritative.

## 3 · Prepare assets and hierarchy

- [ ] Duplicate your working `Level_09_Layout` **prefab** as `Level_10_Layout`, or create a new `SparkLevelLayout` prefab with identical script structure.
- [ ] Create or duplicate a `SparkLevelDefinition` asset named **`Level_10.asset`**; do not edit Level 9 in place.
- [ ] Set `levelId = level_10`, display name **Bridge Balance Challenge**, and the description from Section 7.
- [ ] Set the prefab's `SparkLevelLayout.LevelDefinition` reference to the new Level 10 asset.
- [ ] Register the new prefab/definition mapping in `SparkLevelLayoutCatalog`.
- [ ] Append `Level_10.asset` to the `LevelGamePlayManager.levels` array at index **9** (zero-based) if Levels 1–9 occupy indices 0–8.
- [ ] Verify Level 10's active layout and scene bindings switch together via `LevelGamePlayManager.StartLevel(9)`.
- [ ] Leave the gameplay manager, circuit system, and solver on the persistent scene.

## 4 · Place components

| Component | Initial value | Lifetime / role |
|---|---:|---|
| `PowerSupply_01` | 5 V | Level's configured power source |
| Switch | OFF | Source switching |
| R1 upper-left | 220 Ω | Fixed, conducting target possible |
| R2 lower-left | 330 Ω | Fixed |
| R3 upper-right | **220 Ω initially** | Shelf-replaceable; correct part **100 Ω** |
| R4 lower-right | 150 Ω | Fixed; voltage/current target |
| Universal LCD | `DCVoltage` | A to LEFT MID, B to RIGHT MID; 10 MΩ input preferable |

- [ ] Ensure all installed/bench components are under the electrical solver's discovery root.
- [ ] Ensure shelf *inventory* resistors are not included in the active network until installed.
- [ ] Keep **R4 fixed** so its component/terminal binding IDs remain stable.
- [ ] R3 uses its existing replaceable resistor slot; ensure both terminals connect after installation.

## 5 · Exact physical wires

**Wire 0, if not built in:** `Battery_positive → Switch input`.

| Wire | FROM | TO | Electrical node |
|---|---|---|---|
| 1 | Switch output | R1 input | +5 V switched bus |
| 2 | Switch output | R3 input | +5 V switched bus |
| 3 | R1 output | R2 input | LEFT MID |
| 4 | R3 output | R4 input | RIGHT MID |
| 5 | R2 output | `Battery_negative` | Ground return |
| 6 | R4 output | `Battery_negative` | Ground return |
| 7 | `LCD_Terminal_A` | R1 output (LEFT MID) | LCD + sense |
| 8 | `LCD_Terminal_B` | R3 output (RIGHT MID) | LCD − sense |

**Bind actual terminal IDs:** Your previous level used `Register_Input`/`Register_Output` for some resistors, while Level 9's asset used `Resistor_Input`/`Resistor_Output` for another. Do not assume these names are universal: take each final ID from `SparkLevelSceneBindings` for R1, R2, R3 and R4. The TXT template deliberately labels R4 bindings as needing verification.

## 6 · Level 10 evaluator targets (existing target types)

**Completion Mode:** All Targets. Require Closed Return: true. Reject Short Circuit: true. Reject Target Short: true. Allow Intermediate Connections: true. Require Minimum Voltage: false. Failure Mode: None.

| Index | Target ID | Type | Bound component | Acceptance |
|---|---|---|---|---|
| 0 | `bridge_r1_conducting` | Component Conducting (4) | Fixed 220 Ω R1 | `requireElectricalEnabled = true`, `requireConduction = true` |
| 1 | `bridge_r4_voltage` | Voltage Range (5) | Fixed 150 Ω R4 | 2.90–3.10 V |
| 2 | `bridge_r4_current` | Current Range (6) | Fixed 150 Ω R4 | 0.019–0.021 A |
| 3 | `bridge_source_current` | Current Range (6) | `PowerSupply_01` | 0.028–0.030 A |

For target 1 (R4 voltage), **`useMaximumCurrent = false`**. For target 2 and 3, `useMaximumCurrent = true` and maximum currents as above. Keep `inverted = false` unless testing confirms that the fixed R4 terminal orientation is reversed. The evaluator may treat sign or polarity differently; verify using its `TargetResults` diagnostics before changing target polarity.

**Reference IDs in the starter TXT:** R1 uses verified prior `Resistor_220_Ω`, `Resistor_220_Input`, `Resistor_220_Output` as a *starting assumption*. R4 IDs are placeholders `R4_COMPONENT_ID`, `R4_INPUT_TERMINAL_ID`, `R4_OUTPUT_TERMINAL_ID`; replace all of them with your *actual* Level 10 scene binding identifiers before testing. Do not use the template unchanged.

## 7 · LevelDefinition settings

```text
levelId = level_10
displayName = Bridge Balance Challenge
description = Balance two voltage dividers on a 5 V supply. Replace the upper-right resistor with 100 Ω until the LCD measures approximately 0.00 V between their midpoint nodes.
completionMode = AllTargets (0)
requiredTargetCount = 4
requireClosedReturn = true
rejectShortCircuit = true
rejectTargetShort = true
allowIntermediateConnections = true
requireMinimumVoltage = false
allowAnyConfiguredPowerSource = true
PowerSources[0] = PowerSupply_01, Battery_positive, Battery_negative, 5 V
failureMode = None (0)
autoAdvanceOnCompletion = false
```

## 8 · Create Level 10 teaching drawing asset

- [ ] Generate a new `Level_10_Drawing.asset` using the existing `SparkLevelDrawingAsset` workflow.
- [ ] Page 1 **Overview** — two divider branches, bridge balance idea.
- [ ] Page 2 **Wiring** — eight wires, two separate midpoint nodes, LCD between them.
- [ ] Page 3 **Measurements** — compare 3 V left, 3 V right, approx 0 V LCD; total current 29.091 mA.
- [ ] Page 4 **Solution** — swap 220 Ω upper-right for 100 Ω; show all 4 target ranges.
- [ ] Register `Level_10_Drawing.asset` in the `SparkDrawCurrentLevel.drawings` array; its `levelId` must equal `level_10`.
- [ ] Do not attach another teaching script to `SparkVisualTeachingManager`.

## 9 · Test procedure / acceptance gate

1. [ ] Launch Level 10; verify HUD title is **Bridge Balance Challenge**, not copied Level 9 text.
2. [ ] At the initial wrong R3 220 Ω, switch on. LCD should show roughly +0.97 V, and all 4 targets must **not** pass.
3. [ ] Remove incorrect upper-right R3 and install 100 Ω from the shelf.
4. [ ] Confirm the switched +5 V bus reaches *both* top resistors and both lower resistors reach battery negative.
5. [ ] Switch on. Check `VL ≈ 3.0 V`, `VR ≈ 3.0 V`, LCD `≈ 0.0 V`.
6. [ ] Check R4 voltage approx 3.0 V; R4 current approx 20 mA; source current approx 29.09 mA.
7. [ ] Check each `LevelGamePlayManager.TargetResults` entry: all 4 `satisfied = true`.
8. [ ] If stuck at 3/4, fix the **specific failing target** ID/polarity rather than changing resistor values arbitrarily.
9. [ ] Confirm level completes without false short-circuit/overload. Confirm restart rebuilds Level 10 fresh.
10. [ ] Confirm drawing button selects `Level_10_Drawing` rather than prior Level 9 asset.

## 10 · Import / editable TXT workflow

**Important:** The attached `Level_10_DEFINITION_TEMPLATE.txt` contains the same JSON structure as your round-trip exporter, but it **cannot be imported directly**, because Level 10's actual `.asset` GUID does not yet exist here. The importer deliberately validates original GUID and type.

**Safe procedure:** (1) Create `Level_10.asset` inside Unity. (2) Select it and run **Tools → Project Spark → TXT Round Trip → Export Selected Asset (Editable TXT)**. (3) Copy **only the JSON fields under `MonoBehaviour`** from the supplied Level 10 definition template into the new export's JSON, retaining the new export's real header/GUID/path and Unity's generated object metadata. (4) Replace R4 binding placeholders with the actual IDs. (5) Re-import the *newly exported* TXT while the new `Level_10.asset` is selected. Keep backup and verify the Inspector values.

Alternatively, use the accompanying `Level_10_DefinitionData.json` as the source of target/field values for the custom Level Definition Inspector. **Never change Level 9's GUID or import this Level 10 data into Level 9.**
