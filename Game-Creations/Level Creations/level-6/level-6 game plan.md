# PROJECT SPARK
# LEVEL 6 — CIRCUIT FAULT DETECTIVE

**Development focus:** Level gameplay, electrical simulation, objectives, pass/fail conditions, and visual feedback.

**AI integration:** Deferred until the level is fully functional.

**Difficulty:** Intermediate  
**Supply:** 5 V DC  
**Circuit:** Two parallel LED branches  
**Objectives:** 5  
**Primary challenge:** Detect an open connection and replace an incorrectly sized resistor.

---

## 1. Level Overview

### Level title
**Circuit Fault Detective**

### Player briefing
> A parallel LED circuit has two faults. One branch has a missing connection, and the other contains an unsuitable resistor. Inspect the circuit, repair the connection, select the correct resistor, and bring both LEDs into their safe operating ranges.

### Learning goals
- Recognize an open circuit.
- Understand current in parallel branches.
- Recognize excessive current caused by low resistance.
- Understand resistor power dissipation and thermal failure.
- Replace an unsuitable resistor with a safe one.
- Verify the circuit using live current measurements.

### Success condition
Both LEDs operate correctly, their branch currents remain within the required ranges, the source current is safe, and no installed resistor is burned.

---

## 2. Scene Layout

### Existing workbench components

Reuse the assets and systems already built for Levels 1–5.

| Component | Quantity | Starting configuration |
|---|---:|---|
| DC power supply | 1 | 5 V |
| Power switch | 1 | Initially OFF |
| LED | 2 | Parallel branches |
| Resistor — Branch 1 | 1 | 330 Ω |
| Resistor — Branch 2 | 1 | 100 Ω |
| Wire connections | As needed | One return wire missing |
| Resistor shelf | 1 | 100, 150, 220, 330, 470, 1 kΩ |
| Current measurement displays | 3 | Branch 1, Branch 2, total |
| Objective display | 1 | 5 objectives |
| Reset control | Existing | Restart level |
| SparkResistorBurn | On applicable resistors | Thermal monitoring enabled |

### Circuit topology

```text
                        SWITCH
5 V (+) ──────────────────o/ o─────┬──────────────┐
                                  │              │
                                R1 330Ω        R2 100Ω
                                  │              │
                                LED 1          LED 2
                                  │              │
                                  │          OPEN WIRE
                                  │              X
5 V (−) ──────────────────────────┴──────────────┘
```

This is the **initial fault state**, not the completed circuit.

### Physical arrangement

**Left:** Power supply and switch.

**Center:** Two LED branches with resistors installed on the workbench.

**Upper shelf:** Replacement resistors, clearly marked with resistance values.

**Right:** Circuit status and current measurement panel.

**Existing HUD:** Level title, objective progress, hints, and reset.

---

## 3. Initial Faults

### Fault A — Missing return connection

Branch 2's return wire is absent.

**Expected state:**
- Branch 1 works after switching ON.
- Branch 2 remains OFF.
- Branch 2 current is approximately zero.
- The level cannot be completed.

**Player action:** Connect the missing wire.

### Fault B — Incorrect 100 Ω resistor

Once Branch 2 is reconnected, it conducts excessive current relative to the level's required operating range.

**Expected state:**
- LED 2 may illuminate.
- Branch 2 current is too high.
- The total source current may exceed the target.
- The level remains incomplete.
- Prolonged operation may cause thermal failure if the resistor is configured to be genuinely overloaded.

**Player action:** Replace the 100 Ω resistor with an appropriate value.

### Recommended solutions

**220 Ω:** Recommended, giving approximately 13.6 mA.

**330 Ω:** Also valid, giving approximately 9.1 mA.

These values assume a nominal LED forward voltage of 2 V and a 5 V supply. The actual ProjectSpark electrical solver remains authoritative.

---

## 4. Level 6 Objectives

Use `SparkLevelDefinition.CompletionMode.AllTargets`.

| # | Objective | Target type | Pass condition |
|---|---|---|---|
| 1 | Restore LED 1 | LEDOn | LED 1 is ON |
| 2 | Restore LED 2 | LEDOn | LED 2 is ON |
| 3 | Safe Branch 1 current | CurrentRange | 6–15 mA |
| 4 | Safe Branch 2 current | CurrentRange | 6–15 mA |
| 5 | Safe total current | CurrentRange | 12–30 mA |

### Target binding rules

- Objective 1 → Branch 1 LED component.
- Objective 2 → Branch 2 LED component.
- Objective 3 → Branch 1 LED component.
- Objective 4 → Branch 2 LED component.
- Objective 5 → Power supply component, provided its solved `ElectricalState.Current` represents the total output current.

Use stable LED and source bindings so replacing a resistor does not invalidate the objectives.

### Additional completion condition

**Require Healthy Installed Resistors = true.**

Before completing Level 6, the evaluator checks installed resistors for burned or electrically disabled states.

The required healthy resistor IDs array may remain empty if all relevant resistors are replaceable shelf items and the installed-component check is enabled.

---

## 5. Electrical Values

Assume a 5 V source and nominal 2 V LED forward voltage.

| Resistor | Approx. branch current | Approx. resistor power | Current target |
|---|---:|---:|---|
| 100 Ω | 30.0 mA | 90.0 mW | Fail |
| 150 Ω | 20.0 mA | 60.0 mW | Fail |
| 220 Ω | 13.6 mA | 40.9 mW | Pass |
| 330 Ω | 9.1 mA | 27.3 mW | Pass |
| 470 Ω | 6.4 mA | 19.1 mW | Pass |
| 1 kΩ | 3.0 mA | 9.0 mW | Fail |

**Important:** A 470 Ω resistor also satisfies the nominal 6–15 mA branch-current objective. If you want only 220 Ω and 330 Ω to qualify, tighten the range or add an explicit resistance objective. For the current design, 470 Ω is an acceptable solution if the actual solver reading remains within range.

### Expected circuit measurements

| Circuit condition | Branch 1 | Branch 2 | Total |
|---|---:|---:|---:|
| Branch 2 disconnected | 9.1 mA | 0 mA | 9.1 mA |
| Branch 2 with 100 Ω | 9.1 mA | 30.0 mA | 39.1 mA |
| Branch 2 with 220 Ω | 9.1 mA | 13.6 mA | 22.7 mA |
| Branch 2 with 330 Ω | 9.1 mA | 9.1 mA | 18.2 mA |
| Branch 2 resistor burned open | 9.1 mA | 0 mA | 9.1 mA |

All values are design estimates, not hardcoded display values.

---

## 6. Resistor Burning Behavior

The existing `SparkResistorBurn` component controls this behavior.

### Thermal sequence

**Normal**
- Original resistor materials visible.
- No flames or smoke.
- Current is measured normally.

**Hot**
- Temperature exceeds `hotC`.
- Heat lighting increases.
- Resistor continues conducting.

**Smoking**
- Temperature exceeds `smokeC`.
- Smoke particles or VFX Graph become active.
- The player can still repair the circuit before failure.

**Flaming**
- Temperature exceeds `flameC`.
- `VFX_ResistorFlame` receives increasing `FireRate` and `Heat`.
- The resistor continues operating until its failure threshold.

**Failed**
- Temperature exceeds `failureC` for the specified exposure period.
- `IsFailed` becomes true.
- Burned material replaces the original materials.
- `MeshBurnProgress` animates the surface damage.
- Failure sparks, debris, flash, and audio play.
- `SparkResistor` requests electrical reevaluation.
- The resistor becomes an open circuit.

**Repaired**
- Original materials are restored.
- Temperature and burn progress reset.
- The circuit reevaluates.

### Burn timing

Do not configure a universal fixed burn time independent of electrical power.

With the default thermal model, a 100 Ω resistor dissipating approximately 90 mW will not burn because its equilibrium temperature is too low.

For an intentional Level 6 thermal demonstration, configure a lower resistor power rating and calibrate thermal capacity, cooling, and failure thresholds so the circuit's actual dissipated power produces failure.

The resistor should provide enough warning time for the player to respond.

**Recommended gameplay target:** Several seconds of visible heating before failure, followed by progressive charring after the electrical failure.

---

## 7. Current Measurement System

Use the previously specified `SparkLevel6CurrentDisplay` script and existing TextMeshPro objects.

### Displays

**Branch 1 Current**
- Reads `branch1LED.ElectricalState.Current`.
- Displays mA.

**Branch 2 Current**
- Reads `branch2LED.ElectricalState.Current`.
- Displays mA.

**Total Current**
- Prefer the solved supply output current.
- If the circuit contains only the two intended branches, summing both branch currents is a reasonable display fallback.
- Do not assume the sum is authoritative if additional loads or bypasses exist.

### Current display states

**0.00 mA:** Branch is disconnected, disabled, or nonconducting.

**Below 6 mA:** Below required operating range.

**6–15 mA:** Safe target range.

**Above 15 mA:** Excess current warning.

### Burned resistor measurement

After a real open-circuit failure, the affected branch current should become approximately zero after the electrical solver recalculates.

The current display must reflect the new solved state automatically.

### Display behavior

Use green/cyan for safe currents, amber for excessive currents, and neutral for a disconnected branch.

Color is supplemental: the textual current and status remain readable without depending on color alone.

---

## 8. Pass and Fail Conditions

### Pass

The level passes only when:

1. LED 1 is ON.
2. LED 2 is ON.
3. Branch 1 current is within range.
4. Branch 2 current is within range.
5. Source current is within range.
6. All required installed resistors are healthy.
7. The required electrical return path is valid.
8. No rejected source short, target short, overload, or invalid connection is detected.

On pass:
- Show level completion.
- Record permanent victory.
- Unlock Level 7 according to progression settings.
- Do not automatically revoke completion if the player changes wiring afterward.

### Incomplete — Repairable conditions

- Missing return wire.
- LED not powered.
- Branch current too low.
- Branch current too high.
- Total current outside target range.
- Burned resistor awaiting replacement.

The player remains in the level and can continue making repairs.

### Failed evaluation — Electrical hazards

The current `SparkLevelEvaluator` returns `Failed` for:
- Source short circuit.
- Target short circuit.
- Power source overload.
- Invalid connection or polarity.

The level manager's response to `Failed` should follow its existing behavior. Do not add permanent game-over mechanics until the manager's actual failure handling is reviewed.

---

## 9. Unity Level Definition Inspector

Create or duplicate the Level 5 ScriptableObject.

### Identity

- Level ID: `Level_06`
- Display Name: `Circuit Fault Detective`
- Description: `Repair the disconnected branch and replace the unsafe resistor to restore safe parallel operation.`

### Level rules

- Completion Mode: `AllTargets`
- Require Closed Return: enabled
- Reject Short Circuit: enabled
- Reject Target Short: enabled
- Allow Intermediate Connections: enabled

### Power

- Require Minimum Voltage: disabled for the initial implementation
- Allow Any Configured Power Source: enabled
- Nominal Voltage: 5 V
- Positive/negative terminal IDs: copy the valid IDs from the duplicated Level 5 scene and verify the new scene bindings

### Objectives

Configure the five targets from Section 4.

### Fault Detective Rules

- Required Healthy Resistor IDs: empty if all required resistors are replaceable
- Require Healthy Installed Resistors: enabled

### Progression

- Unlocked By Default: disabled
- Auto Unlock Next Level: enabled
- Auto Advance On Completion: disabled

---

## 10. Required Scene Hierarchy

```text
Level_06_CircuitFaultDetective
│
├── GameplaySystems
│   ├── LevelGamePlayManager
│   ├── SparkCircuitSystem
│   └── SparkLevelSceneBindings
│
├── PowerSystem
│   ├── PowerSupply_5V
│   └── MainSwitch
│
├── CircuitWorkbench
│   ├── Branch1
│   │   ├── ResistorSlot_1
│   │   ├── LED_1
│   │   └── Wires_Branch1
│   │
│   └── Branch2
│       ├── ResistorSlot_2
│       ├── LED_2
│       └── Wires_Branch2
│
├── ResistorShelf
│   ├── Resistor_100
│   ├── Resistor_150
│   ├── Resistor_220
│   ├── Resistor_330
│   ├── Resistor_470
│   └── Resistor_1000
│
└── Level6UI
    ├── ObjectivePanel
    ├── CurrentPanel
    │   ├── Branch1CurrentText
    │   ├── Branch2CurrentText
    │   ├── TotalCurrentText
    │   └── CircuitStatusText
    └── ExistingLevelControls
```

This is a suggested organizational hierarchy. Preserve your actual component locations and prefab structure if they already work.

---

## 11. Implementation Order

### Phase A — Initial circuit

- Duplicate the Level 5 scene.
- Set the new level definition.
- Install Branch 1 with 330 Ω.
- Install Branch 2 with 100 Ω.
- Disconnect one Branch 2 return connection.
- Verify all resistor terminals are correctly positioned.
- Verify sufficient terminal connection capacity.

### Phase B — Objectives

- Configure five targets.
- Verify individual LED states.
- Verify individual branch current readings.
- Verify total source current.
- Test that unsafe currents do not count as complete.

### Phase C — Resistor replacement

- Remove the initial 100 Ω resistor.
- Install a 220 Ω resistor.
- Check that the new resistor participates in the electrical solver.
- Check that the old shelf resistor is no longer treated as installed.
- Check that all objectives read stable components rather than the removed resistor.

### Phase D — Thermal failure

- Verify `SparkResistorBurn` receives dissipated power.
- Verify heating, smoke, and flame VFX.
- Verify actual failure opens the circuit.
- Verify Branch 2 current falls to zero.
- Verify burned materials progress visually.
- Verify a burned installed resistor prevents victory.

### Phase E — Victory

- Confirm 5/5 objectives.
- Confirm all health rules pass.
- Confirm Level 6 completes once.
- Confirm progression unlocks Level 7.
- Confirm subsequent circuit changes do not erase the permanent win.

---

## 12. Play Mode Test Checklist

- [ ] Level starts with the intended two faults.
- [ ] Branch 1 works after switching ON.
- [ ] Branch 2 stays OFF when disconnected.
- [ ] Reconnecting Branch 2 produces a measurable current.
- [ ] 100 Ω produces excessive current under the nominal circuit assumptions.
- [ ] Incorrect branch current blocks victory.
- [ ] Replacing with 220 Ω can satisfy the branch-current target.
- [ ] Replacing with 330 Ω can satisfy the branch-current target.
- [ ] Source current matches the solved circuit.
- [ ] Thermal system responds to actual resistor power.
- [ ] Real resistor failure opens the branch.
- [ ] Current display updates after failure.
- [ ] Burned installed components block completion.
- [ ] Repaired safe circuit reaches 5/5.
- [ ] Level 6 completes and unlocks Level 7.
- [ ] Permanent victory behavior is preserved.

---

## 13. Deferred Features

Do not implement these while building the initial level:

- Ask Spark automated fault diagnosis.
- AI-generated explanations.
- AI hint improvements.
- AI tutorial changes.
- AI-guided resistor selection.
- Automated AI circuit inspection.

These will be integrated after gameplay, measurements, burning, and completion are verified.

---

## 14. Definition of Done

**Level 6 is complete when a player can start with two faults, diagnose them using the circuit and measurements, replace the incorrect resistor, repair the missing wire, achieve all five objectives, and permanently complete the level without relying on Ask Spark AI.**

The electrical solver remains the single source of truth for voltage, current, and power. The level evaluator remains the single authority for objective completion.

**Next development task:** Verify the actual Level 6 scene bindings and the five `SparkLevelTarget` configurations against the existing resistor replacement workflow.