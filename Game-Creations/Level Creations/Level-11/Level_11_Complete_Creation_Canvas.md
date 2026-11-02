# PROJECT SPARK — LEVEL 11 COMPLETE CREATION CANVAS

## Power Distribution Challenge · Safe Parallel Loads

**Status:** Build blueprint (not Unity Play Mode verified). **Level ID:** `level_11` · **Supply:** 5 V DC · **Objectives:** 4 · **Completion:** All Targets.

> **Architecture:** Reuse your persistent gameplay scene and existing `LevelGamePlayManager`, `SparkLevelLayoutLoader`, `SparkLevelSceneBindings`, electrical solver, shelf, LCD, resistor burnout and `SparkLevelDrawingAsset`. Make a new level prefab + definition, not a second gameplay scene. Do not create new teaching MonoBehaviours.

## 1 · Learning objective and win condition

Player begins with **100 Ω** installed as the replaceable middle resistor (R2). They must switch power off, remove it, and install **220 Ω**. The circuit remains parallel; R2 current drops from 50.00 mA to 22.73 mA and its dissipation from 0.250 W to about 0.114 W. All four electrical targets must pass.

```text
                          Switched +5 V bus
                                  |
                 +----------------+----------------+
                 |                |                |
          R1 = 330 Ω FIXED   R2 = 100 Ω WRONG   R3 = 470 Ω FIXED
                 |           replace -> 220 Ω         |
                 |                |                |
                 +----------------+----------------+
                                  |
                           Battery_negative

          LCD A -> R2 input (top bus)
          LCD B -> Battery_negative (DCVoltage mode)
```

Each branch must return **independently** to battery negative. The LCD is a high-impedance measurement, **not** a current return. No LED or other load is required for this design.

## 2 · Calculations (ideal 5 V supply)

| Measurement | Initial wrong 100 Ω R2 | Correct 220 Ω R2 |
|---|---:|---:|
| R1 current, 330 Ω | 15.152 mA | 15.152 mA |
| R2 current | 50.000 mA | 22.727 mA |
| R3 current, 470 Ω | 10.638 mA | 10.638 mA |
| Total source current | 75.790 mA | 48.517 mA |
| R2 voltage | 5.000 V | 5.000 V |
| R2 dissipation | 0.250 W | 0.11364 W |

Use a **0.15 W R2 rating** if the specific burnout implementation respects wattage; correct 220 Ω stays below it but with a modest margin. Your earlier `SparkResistorBurn` version did **not** use `ratedWatts` directly in its thermal equation, so heat/cooling must be calibrated separately. A component can burn before objectives complete; ensure wrong initial resistor does not burn instantly. LCD's finite input impedance can make source readings differ slightly.

## 3 · Asset and scene setup

- [ ] Duplicate the working Level 10 layout prefab as **`Level_11_Layout`** (or create equivalent using the current prefabs). Remove all Level 10 Wheatstone bridge objects/wires; do **not** retain its midpoint circuitry.
- [ ] Create/select the new **`Level_11.asset`** definition, `levelId = level_11`.
- [ ] Import `Level_11_Editable_REPLACEMENT.txt` into the **original Level 11 asset only**, after replacing all placeholder IDs as in Section 7. This file preserves your Level 11 GUID; do **not** import into Level 10.
- [ ] In `SparkLevelLayout`, point **LevelDefinition** to `Level_11.asset`.
- [ ] Register `Level_11_Layout` in `SparkLevelLayoutCatalog`, mapped to the same Level 11 definition.
- [ ] Add `Level_11.asset` to `LevelGamePlayManager.levels` at **index 10** (zero-based) when Levels 1–10 are at 0–9; do not replace an existing element.
- [ ] Keep one authoritative gameplay manager and layout loader; ensure `SparkDrawCurrentLevel` references them.
- [ ] Use the active layout's `SparkLevelSceneBindings` for all ids; avoid sharing stale Level 10 bindings.

Suggested prefab hierarchy (adapt names to your actual prefabs):

```text
Level_11_Layout  [SparkLevelLayout]
 ├─ SparkLevelSceneBindings
 ├─ PowerSupply_01 (existing 5 V supply prefab)
 ├─ Switch
 ├─ R1_330_Fixed
 ├─ R2_Replaceable_Slot  -> starts with 100 Ω resistor
 ├─ R3_470_Fixed
 ├─ LCD_Meter
 └─ (existing world-space layout/decoration as desired)
```

**Important:** `SparkLevelSceneBindings` might be located on your layout root rather than a child. Preserve the structure expected by `SparkLevelLayout.SceneBindings`.

## 4 · Place and configure components

| Item | Required setup | Role |
|---|---|---|
| Supply | Existing `PowerSupply_01`, nominal 5 V; `Battery_positive` / `Battery_negative` | Main source |
| Power switch | OFF initially | Safe replacement workflow |
| R1 | Fixed 330 Ω | Stable branch 1, conducting target |
| R2 | Shelf-replaceable position, **100 Ω initially** | Faulty current-heavy branch; correct shelf replacement = **220 Ω** |
| R3 | Fixed 470 Ω | Stable branch 3 |
| LCD | `SparkLCDMeasurementMode.DCVoltage`, high impedance | A to positive bus, B to negative bus |
| Shelf inventory | At least 100 Ω, 150 Ω, 220 Ω, 330 Ω | Provide player choices |

- [ ] Ensure fixed/installed resistor components are discoverable by the existing electrical solver's active root.
- [ ] Verify the replaceable position and installed resistor share stable **scene binding IDs** across swaps. If your shelf replacement process changes bound component IDs, configure targets according to the stable binding mechanism already used in Levels 7–9.
- [ ] Keep shelf inventory outside active topology until installed.
- [ ] Configure `SparkResistorBurn` **for R2**: use a nominal rating of 0.15 W (if exposed) and tune heat capacity/cooling/thresholds from real solver wattage (Section 2). Never assume `ratedWatts` alone enforces thermal behavior.
- [ ] Check that the **fixed 330 Ω R1** is not confused with the former bridge circuit's fixed 330 Ω element in scene bindings.

## 5 · Exact physical wire checklist

If the power supply is not prewired to the switch, make the initial connection too.

| Wire | From | To | Explanation |
|---|---|---|---|
| 0 | `Battery_positive` | Switch input | Supply positive — only if not already prewired |
| 1 | Switch output | R1 330 Ω input | Parallel +5 V rail |
| 2 | Switch output | R2 replaceable input | Parallel +5 V rail |
| 3 | Switch output | R3 470 Ω input | Parallel +5 V rail |
| 4 | R1 330 Ω output | `Battery_negative` | Branch 1 return |
| 5 | R2 replaceable output | `Battery_negative` | Branch 2 return |
| 6 | R3 470 Ω output | `Battery_negative` | Branch 3 return |
| 7 | `LCD_Terminal_A` | R2 input / switched +5 V rail | Voltage probe positive |
| 8 | `LCD_Terminal_B` | `Battery_negative` | Voltage probe negative |

**Do not** connect resistor outputs in series; all three branch inputs share the positive rail, all branch outputs share battery negative. **Do not** put LCD in `InlineCurrent` mode across 5 V (its shunt is a low-resistance path).

## 6 · LevelDefinition values

```text
levelId = level_11
displayName = Power Distribution Challenge
description = A three-branch 5 V power network is overloaded. Switch off power, replace the middle 100 ohm resistor with 220 ohm, and restore safe distribution.
completionMode = AllTargets (0)
requiredTargetCount = 4
requireClosedReturn = true
rejectShortCircuit = true
rejectTargetShort = true
allowIntermediateConnections = true
requireMinimumVoltage = false
allowAnyConfiguredPowerSource = true
powerSources[0] = PowerSupply_01, Battery_positive, Battery_negative, 5 V
failureMode = None (0)
autoAdvanceOnCompletion = false
```

## 7 · Four gameplay targets — bind exact scene IDs

The downloadable replacement TXT uses the following objectives. **The component/terminal IDs are not verified for your newly built Level 11 scene; replace placeholders before import.**

| Index | Target ID | Type | Component / acceptance |
|---|---|---|---|
| 0 | `distribution_r1_conducting` | Component Conducting (4) | Fixed 330 Ω R1 must conduct |
| 1 | `distribution_r2_current` | Current Range (6) | Replaceable R2 current **0.021–0.024 A** |
| 2 | `distribution_r2_voltage` | Voltage Range (5) | Replaceable R2 voltage **4.90–5.10 V** |
| 3 | `distribution_source_current` | Current Range (6) | `PowerSupply_01` total current **0.047–0.050 A** |

**Replace these six TXT placeholders consistently:**

```text
R1_330_COMPONENT_ID
R1_330_INPUT_TERMINAL_ID
R1_330_OUTPUT_TERMINAL_ID
R2_REPLACEABLE_COMPONENT_ID
R2_INPUT_TERMINAL_ID
R2_OUTPUT_TERMINAL_ID
```

These are **binding keys**, not necessarily GameObject names. Get their exact spelling from the Level 11 active layout's `SparkLevelSceneBindings` and the stable replaceable R2 binding. The 3rd target uses `useMaximumVoltage = true`, maximum 5.10, and **`useMaximumCurrent = false`**. The 2nd and 4th are current targets with `useMaximumCurrent = true`. Verify polarity and current sign with `LevelGamePlayManager.TargetResults`. Do not guess alternate target IDs.

**Design limitation:** These four electrical targets validate the correct 220 Ω behavior (via current ranges); they do not directly evaluate actual `SparkResistorBurn` temperature/health. You may separately enable healthy-resistor rules only after verifying IDs and runtime behavior. Keep the base challenge playable with the existing evaluator first.

## 8 · Teaching drawing asset (`SparkLevelDrawingAsset`)

- [ ] Create **`Level_11_Drawing.asset`** (`levelId = level_11`).
- [ ] Page 1: **Overview** — three branches in parallel, why too much current is unsafe.
- [ ] Page 2: **Wiring** — supply, switch, three independent returns, LCD across R2.
- [ ] Page 3: **Measurements** — wrong 100 Ω vs corrected 220 Ω, Ohm's Law and `P = V²/R`.
- [ ] Page 4: **Solution** — replace R2, total current ≈ 48.52 mA, four objectives.
- [ ] Add the asset to existing **`SparkDrawCurrentLevel.drawings`** list.
- [ ] No `SparkLevel11CompleteTeaching` MonoBehaviour required; keep the existing shared teaching canvas.

## 9 · Play Mode acceptance test

1. [ ] Start Level 11 (manager level array index 10). Confirm HUD says **Power Distribution Challenge** and active layout is Level 11, not the prior bridge.
2. [ ] Verify initial R2 = 100 Ω; other values R1 = 330 Ω and R3 = 470 Ω.
3. [ ] Turn on switch briefly. Expected nominal R2 ≈ 50 mA, total ≈ 75.79 mA, R2 power ≈ 0.25 W, LCD ≈ 5 V. Challenge **must not** already satisfy all 4 targets.
4. [ ] Observe burner warning behavior; power off before damage. Verify level is restartable if R2 burns.
5. [ ] Replace R2 using the shelf with 220 Ω and reconnect its same two electrical nodes.
6. [ ] Switch on. Expected R1 ≈ 15.15 mA, R2 ≈ 22.73 mA, R3 ≈ 10.64 mA, source ≈ 48.52 mA, LCD ≈ 5 V.
7. [ ] Verify no source or target shorts, both returns closed, no erroneous overload.
8. [ ] Read `LevelGamePlayManager.TargetResults`: all four target IDs must show **Satisfied=True**.
9. [ ] If stuck at 3/4, check the failing target's real component ID, terminal IDs, sign, enabled maximum-range flags, and the source-current binding. Do not change the working wiring arbitrarily.
10. [ ] Check restart respawns fresh 100 Ω initial configuration, auto-evaluation, progression unlock, and Level 11 teaching pages.

## 10 · Import workflow and safety

This package includes a **Level 11 editable TXT replacement** copied from the earlier generated replacement (not a new GUID). The original header must contain:

```text
Asset Name: Level_11
Asset GUID: 75dca85951b518543a34f15d67e5654f
```

1. Back up your `Level_11.asset` (importer should create a backup too).
2. Open the TXT; substitute all six binding placeholders with actual Level 11 IDs.
3. Select the **original** `Level_11.asset` in Unity Project view.
4. Use **Tools → Project Spark → TXT Round Trip → Import TXT Into Selected Asset**.
5. Confirm updated Inspector values; test in Play Mode. Import does not generate the layout prefab, wires, scene bindings or teaching drawing asset.

**No Unity Play Mode validation was performed here.** This is a build/checklist document, and actual topology and runtime IDs must be verified in the user project.
