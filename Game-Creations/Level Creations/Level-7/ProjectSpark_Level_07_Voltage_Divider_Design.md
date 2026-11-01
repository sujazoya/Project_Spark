# PROJECT SPARK — LEVEL 7: VOLTAGE DIVIDER CHALLENGE

**Development scope:** Gameplay and existing solver/evaluator integration only. No AI, new electrical solver, shop, new panels, or unnecessary scripts.

**Difficulty:** Intermediate → Advanced  
**Supply:** 5 V DC  
**New concept:** Voltage division / selecting a resistor to calibrate an unloaded DC output  
**Objectives:** 4  
**Initial problem:** Output voltage below specification because upper divider resistor is too large  
**Correct intended fix:** Replace upper 330 Ω resistor with 220 Ω from the existing shelf.

## 1. Player briefing

> A sensor input needs a stable 3.00 V reference from a 5 V power supply. The installed divider produces the wrong output. Use the multimeter to measure the output, choose the correct resistor from the shelf, and calibrate the circuit without causing a short circuit.

### Learning goals
- Understand that two series resistors divide a supply voltage.
- Distinguish voltage measured across one resistor from the full supply voltage.
- Understand how the upper resistor changes the midpoint voltage.
- Check current and resistor power using verified simulated values.
- Replace a shelf resistor without leaving old electrical connections or stale registrations.

## 2. Circuit

```text
                       Main Switch
Battery +5 V -------------o/ o-----[ R1: replaceable ]------o------ VOUT
                                                       midpoint |
                                                                |
                                                        [ R2: 330 Ω ]
                                                                |
Battery − / 0 V -----------------------------------------------o
```

- **R1 upper resistor:** Start with 330 Ω. Place on workbench from the shelf. Intended final R1 is 220 Ω.
- **R2 lower resistor:** Fixed 330 Ω in the level layout; not replaceable.
- **VOUT:** The junction between R1 and R2. Meter RED on VOUT and BLACK on battery negative / bottom of R2.
- **Supply:** 5 V DC. Existing switch initially OFF.
- **No LED or other load on VOUT:** A load would change the expected divider voltage and require different targets.
- **Only one return path:** Both resistors connected in series, no extra parallel loads or bypasses.

## 3. Player actions and states

1. Switch ON and measure VOUT: about **2.50 V** (wrong).
2. Inspect the upper 330 Ω resistor. Remove / return it with the existing shelf system.
3. Choose and install a 220 Ω resistor from the existing shelf, preserving its two series connections.
4. Switch ON if necessary and remeasure VOUT: about **3.00 V**.
5. Verify divider current and safe electrical evaluation. Finish with all objectives satisfied.

No automatic repair and no AI hints are required.

## 4. Calculated values (unloaded ideal divider)

For R1 upper and R2 = 330 Ω lower:

`I = 5 / (R1 + 330)` amperes; `VOUT = I × 330` volts.

| Upper R1 | VOUT | Series current | R1 power | Result |
|---:|---:|---:|---:|---|
| 100 Ω | 3.837 V | 11.63 mA | 13.5 mW | FAIL — output too high |
| 150 Ω | 3.438 V | 10.42 mA | 16.3 mW | FAIL — output too high |
| **220 Ω** | **3.000 V** | **9.09 mA** | **18.2 mW** | **PASS** |
| 330 Ω | 2.500 V | 7.58 mA | 18.9 mW | FAIL — initial setup |
| 470 Ω | 2.063 V | 6.25 mA | 18.4 mW | FAIL — output too low |
| 1 kΩ | 1.241 V | 3.76 mA | 14.1 mW | FAIL — output too low |

At 220 Ω, the bottom 330 Ω resistor dissipates about **27.3 mW**, and total source power is about **45.5 mW**. These are estimates; use the live electrical solver as authoritative. Differences from parasitic resistances, device models, or non-ideal 5 V source should be considered during testing.

### Do not make this a burn challenge
These power values are normally safe for 0.25 W resistors. Leave the Level 7 thermal settings normal. Level 6 already teaches overheating and failure.

## 5. Existing Level Target setup — four objectives

Use `SparkLevelDefinition.CompletionMode.AllTargets`. The known target types below already appear in Project Spark's training dashboard integration; verify their actual inspector behavior and underlying evaluator before publishing.

| # | Objective | Existing target type | Stable binding | Pass condition |
|---:|---|---|---|---|
| 1 | Establish divider current | `ComponentConducting` | Fixed R2 (`SparkResistor`) | R2 is electrically conducting |
| 2 | Calibrate output voltage | `VoltageRange` | Fixed R2 (`SparkResistor`) | **2.90–3.10 V** across R2 |
| 3 | Safe divider current | `CurrentRange` | Fixed R2 (`SparkResistor`) | **0.008–0.010 A** |
| 4 | Correct source output | `CurrentRange` | Existing power supply (`SparkPowerSupply`) | **0.008–0.010 A**, if source state reports delivered current magnitude |

- **Critical binding rule:** Do not bind completion targets to R1. The shelf component is replaced during play, making that reference unstable.
- **For objective 2:** The physical voltage across R2 equals VOUT only if its upper terminal is connected to the midpoint and its lower terminal to 0 V. Confirm polarity/sign and evaluator behavior in Play Mode. If it evaluates signed voltage, orient the R2 terminals appropriately.
- **For objective 4:** Reuse the same supply-current semantics already validated in Level 6. If the evaluator does not produce a correct positive source-current magnitude, do not fake a total; verify the source model or use only the first three stable objectives until corrected.
- **Source of truth:** Never hardcode `3.00 V` or `9.09 mA` into a live display. Values must come from solved states.

## 6. Scene architecture (single-scene Project Spark)

**Do not duplicate the Unity gameplay scene.** Create `Level_07_Layout` as the next loadable layout prefab / `SparkLevelLayout` and a `Level_07` ScriptableObject definition. Keep the persistent solver, HUD, switch/wire system, shelf and instruments.

Suggested level layout contents:

```text
Level_07_Layout
├── SparkLevelLayout
├── SparkLevelSceneBindings
├── PowerSupply_5V       (reuse existing level power component)
├── MainSwitch           (reuse existing switch)
├── DividerWorkbench
│   ├── UpperResistorSlot  (receives shelf R1)
│   ├── MidpointTerminal  (VOUT)
│   ├── LowerResistor_330 (fixed stable R2)
│   └── DividerWires
└── ExistingObjectiveAndCurrentUI
```

The shared `Component_Shelf` remains **under the solver's actual ElectronicObjectRoot / Component Search Root**, as already corrected for Levels 3–6. Installed shelf resistors must be discovered; storage components must not behave as wired devices.

### Proposed binding IDs (examples, assign actual stable IDs in inspector)

- `divider_bottom` → the fixed 330 Ω `SparkResistor` component.
- `power_supply` → this level's actual `SparkPowerSupply` component.
- `divider_midpoint` → `SparkTerminal` at junction / top terminal of fixed R2, for optional probe assistance.
- `battery_negative` → actual negative terminal binding.

Do not create a stable objective binding to a specific physical shelf R1 instance.

## 7. Level definition rules

- Level ID: `Level_07`
- Display name: `Voltage Divider Challenge`
- Description: `Calibrate a 3.00 V output from a 5 V supply by selecting the correct upper resistor.`
- Completion Mode: `AllTargets`
- Require Closed Return: enabled.
- Reject Short Circuit: enabled.
- Reject Target Short: enabled.
- Allow Intermediate Connections: enabled.
- Require Healthy Installed Resistors: enabled, if already supported by the active Level Definition.
- Supply nominal voltage: 5 V.
- Unlocked By Default: disabled.
- Auto Unlock Next Level: enabled (Level 8).
- Auto Advance On Completion: disabled.
- Preserve permanent completion on later wiring changes, per the existing progression system.

No new evaluation types, extra AI panels, or bespoke level managers are required for the intended implementation.

## 8. Pass, incomplete, and failure behavior

**Pass:** All four objectives satisfied on a completed solve, expected return path valid, no rejected shorts or invalid connections, installed resistors healthy. Level 8 unlocks using existing progression handling.

**Incomplete:** Switch OFF; missing upper resistor; wrong upper resistor; VOUT outside 2.90–3.10 V; current out of range; missing return wire. Player can correct without resetting the scene.

**Failed evaluation:** Source short, target short, polarity or connection hazards already rejected by existing `SparkLevelEvaluator`. Do not add new permanent game-over mechanics.

## 9. Player-facing feedback

- Existing multimeter displays **VOUT measured relative to battery −**.
- Existing component telemetry may select fixed R2 to show voltage/current/power.
- Existing objective HUD displays `TARGETS 0/4` through `4/4`.
- A small static label near the midpoint may read `VOUT — TARGET 3.00 V` (existing TextMeshPro / world label, no new HUD system).
- When output is low or high, existing objective display remains incomplete. No hardcoded fake measured values.

## 10. Build and Play Mode checklist

- [ ] `Level_07` definition and active `Level_07_Layout` load in the single gameplay scene.
- [ ] All bindings resolve to current Level 7 layout objects rather than a previous level.
- [ ] Upper starting 330 Ω resistor is electrically discovered under the solver search root.
- [ ] With switch ON, initial VOUT is near **2.50 V**; total current near **7.58 mA**.
- [ ] Both wire connections for removed resistor are correctly removed / reset by existing shelf mechanics.
- [ ] Replacing 330 Ω with 220 Ω leads to a fresh valid solve.
- [ ] Fixed R2 remains bound to all its objectives after R1 replacement.
- [ ] VOUT reads near **3.00 V** and current near **9.09 mA**.
- [ ] `VoltageRange` evaluates the voltage across fixed R2 with the correct sign / magnitude.
- [ ] Existing supply-current objective reads meaningful delivered current (or is withheld pending verification).
- [ ] Other shelf options produce incomplete objectives: 100, 150, 330, 470, 1k Ω.
- [ ] Stored resistors are not included as active wired components; removed old resistor doesn't retain links.
- [ ] Short-circuit / invalid-connection checks continue to work.
- [ ] No thermal failure is expected during correct normal operation.
- [ ] All configured objectives reach pass and Level 8 unlocks.
- [ ] Permanent victory persists, following the existing level progression system.

## 11. Definition of done

Level 7 is complete when players can start from 2.50 V, measure VOUT, replace the upper resistor with a 220 Ω part from the shelf, observe a verified output near 3.00 V, satisfy the existing objective evaluator, and permanently complete Level 7 without any new solver or AI integrations.

**Next development task:** Build the `Level_07_Layout` and configure four targets against actual fixed-R2 and power-source bindings. Then test the resistor-swap workflow in Play Mode.
