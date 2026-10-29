# PROJECT SPARK
# LEVEL 05 — PARALLEL PATHS
**Complete Level Design & Unity Implementation Reference**

**Level Number:** 05  
**Level Name:** Parallel Paths  
**Difficulty:** Intermediate  
**Category:** Parallel Circuits, Current Distribution & Electrical Safety  
**Previous Level:** Level 04 — The Power Challenge  
**Next Level:** Level 06 — To Be Designed  
**Status:** Design Specification

---

## 1. LEVEL OVERVIEW

### Main Concept

Level 5 introduces **parallel electrical circuits**.

In previous levels, players learned how to power components, connect a return path, select resistors, limit current, and understand electrical power.

Now the player must build a circuit with **two independent LED branches**, both powered from the same source.

Each branch contains:

- One current-limiting resistor
- One LED
- Connecting wires

Both branches share a common positive supply connection and a common negative return connection.

### Mission Statement

**"Build two parallel LED circuits. Make both LEDs glow while keeping the current in each branch within safe limits and preventing excessive total current draw."**

### Player Learning Outcomes

By the end of this level, the player should understand:

1. What a parallel circuit is.
2. How parallel circuits differ from series circuits.
3. Why both branches share the same supply voltage.
4. Why current divides between parallel branches.
5. How resistor values affect branch current.
6. Why each LED needs its own current-limiting resistor.
7. Why total supply current equals the sum of branch currents.
8. How to diagnose a branch that is not working.

---

## 2. LEVEL ENVIRONMENT

### Scene / Layout Name

`Level05_ParallelPaths`

### Visual Theme

Use the existing ProjectSpark futuristic electronics workshop.

Keep the established visual language:

- Dark electronics workbench
- Glowing red positive wiring
- Glowing blue negative return wiring
- Cyan component labels
- Existing resistor shelf
- Workshop computer showing level objectives and progress
- Ask Spark AI interface

### Workbench Arrangement

**Left side**
- 5 V power supply
- Main power switch

**Center**
- Branch 1 resistor installation position
- Branch 2 resistor installation position
- Separate wire connection points

**Right side**
- LED 1, preferably red
- LED 2, preferably green

**Shared connections**
- Positive rail after the main switch
- Negative return rail connected to supply negative

The two branches should be visibly separated so players can understand the circuit structure.

---

## 3. REQUIRED COMPONENTS

| Component | Quantity | Purpose |
|---|---:|---|
| DC power supply | 1 | Provides 5 V |
| Main switch | 1 | Enables/disables both branches |
| LED 1 | 1 | First electrical load |
| LED 2 | 1 | Second electrical load |
| Resistor | 2 installed | Controls branch current |
| Electrical wires | As needed | Connects both branches |
| Resistor shelf | Existing system | Allows selecting resistor values |

### Available Resistor Options

Use the existing shelf values:

- 100 Ω
- 150 Ω
- 220 Ω
- 330 Ω
- 470 Ω
- 1 kΩ

The recommended initial passing combination is **330 Ω + 330 Ω**, assuming approximately 2 V forward voltage for each LED.

Actual success must be verified using the ProjectSpark electrical solver.

---

## 4. ELECTRICAL CIRCUIT DESIGN

### Correct Circuit Structure

```text
                   MAIN SWITCH
  POWER +5 V  ───────/ ──────┬──────────────┐
                             │              │
                            R1             R2
                             │              │
                           LED 1          LED 2
                             │              │
  POWER −   ─────────────────┴──────────────┘
```

### Branch 1

`+5 V → Switch → Resistor 1 → LED 1 → Supply Negative`

### Branch 2

`+5 V → Switch → Resistor 2 → LED 2 → Supply Negative`

### Important Wiring Rules

- Both branches must connect to the same switched positive rail.
- Both branches must return to the negative supply rail.
- The LEDs must have correct polarity.
- Each LED needs its own series resistor.
- LED 1 and LED 2 must not be connected in series with each other.
- Connecting positive directly to negative must be treated as a short circuit.
- A branch without a valid return path must not operate.

### Why Two Resistors?

Each branch must control its own current.

If both LEDs shared only one resistor before splitting into parallel branches, current division could become dependent on the LED characteristics. The intended safe design is one resistor in series with each LED.

---

## 5. LEVEL OBJECTIVES

The player sees three mission goals.

### Objective 1 — Both LEDs Operate

**Player-facing instruction:**  
"Make LED 1 and LED 2 glow."

Requirements:

- LED 1 is operating.
- LED 2 is operating.
- Both have correct polarity.
- Both receive sufficient electrical current.

**Failure examples:**

- LED 2 is disconnected.
- One LED is reversed.
- The return path is open.
- A resistor prevents sufficient current.
- The supply or switch is off.

### Objective 2 — Control Branch Current

**Player-facing instruction:**  
"Keep the current through each LED branch between 6 mA and 15 mA."

Requirements:

- Branch 1 current: 6–15 mA.
- Branch 2 current: 6–15 mA.

Both branches must satisfy their ranges.

**Failure examples:**

- A 100 Ω resistor allows excessive current.
- A 1 kΩ resistor produces insufficient current.
- One branch is open, producing zero current.
- A component is damaged or disabled.

These examples assume the approximate 5 V and 2 V LED model; actual behavior depends on the solver.

### Objective 3 — Limit Total Supply Current

**Player-facing instruction:**  
"Keep the total current drawn from the power supply at or below 30 mA."

Requirements:

- Combined supply current is no greater than 30 mA.
- Both individual branch requirements also pass.

The current delivered by the source should approximately equal the sum of the two branch currents when no additional paths are present.

---

## 6. ELECTRICAL CALCULATIONS

### Basic Equations

For a branch containing a resistor and an LED:

**Branch current:**

`I = (Vsupply − VLED) / R`

**Total current:**

`Itotal = Ibranch1 + Ibranch2`

**Resistor power:**

`P = I² × R`

### Example with 330 Ω Resistors

Assumptions:

- Supply voltage = 5 V
- LED forward voltage ≈ 2 V
- Branch 1 resistor = 330 Ω
- Branch 2 resistor = 330 Ω

Voltage across each resistor:

`5 V − 2 V = 3 V`

Branch 1 current:

`3 / 330 = 0.00909 A ≈ 9.09 mA`

Branch 2 current:

`3 / 330 = 0.00909 A ≈ 9.09 mA`

Total current:

`9.09 + 9.09 = 18.18 mA`

Power in each resistor:

`0.00909² × 330 ≈ 0.02727 W`

`≈ 27.27 mW`

### Example Result

| Measurement | Approximate Value | Requirement | Result |
|---|---:|---|---|
| Branch 1 current | 9.09 mA | 6–15 mA | Pass |
| Branch 2 current | 9.09 mA | 6–15 mA | Pass |
| Total source current | 18.18 mA | ≤30 mA | Pass |
| Resistor 1 power | 27.27 mW | Informational | — |
| Resistor 2 power | 27.27 mW | Informational | — |

**Note:** These are approximate design calculations, not guaranteed runtime measurements. The electrical solver and component configurations determine the actual result.

---

## 7. RESISTOR SELECTION GAMEPLAY

Players should experiment with resistor combinations.

### Example Combinations

Assuming both LEDs have approximately 2 V forward voltage:

| R1 | R2 | Branch 1 | Branch 2 | Total | Current Objectives |
|---|---|---:|---:|---:|---|
| 100 Ω | 100 Ω | 30.00 mA | 30.00 mA | 60.00 mA | Fail |
| 150 Ω | 330 Ω | 20.00 mA | 9.09 mA | 29.09 mA | Fail: Branch 1 |
| 220 Ω | 220 Ω | 13.64 mA | 13.64 mA | 27.27 mA | Pass |
| 330 Ω | 330 Ω | 9.09 mA | 9.09 mA | 18.18 mA | Pass |
| 330 Ω | 470 Ω | 9.09 mA | 6.38 mA | 15.47 mA | Pass |
| 470 Ω | 1 kΩ | 6.38 mA | 3.00 mA | 9.38 mA | Fail: Branch 2 |

These multiple passing combinations encourage experimentation rather than forcing a single correct resistor choice.

### Key Design Principle

The player must learn that:

**Making both LEDs glow is not enough. The electrical measurements must also meet the requirements.**

---

## 8. UNITY LEVEL DEFINITION

Create the Level 5 asset using the existing `SparkLevelDefinition` system.

### Suggested Values

| Property | Value |
|---|---|
| Level ID | `level_05` |
| Display Name | `Parallel Paths` |
| Description | `Power two LEDs in parallel while controlling branch and total current.` |
| Supply voltage | 5 V |
| Required return path | Enabled |
| Short-circuit rejection | Enabled |
| Target short rejection | Enabled |
| Progression dependency | Level 4 completion |

Use the actual serialized fields exposed by your current `SparkLevelDefinition` asset and gameplay manager.

---

## 9. UNITY EVALUATOR TARGET CONFIGURATION

Although the player sees **three learning goals**, your current `SparkLevelTarget` system evaluates one target at a time.

Therefore, use **five evaluator targets**.

### Target 1 — LED 1 On

- `TargetId`: `led_1_on`
- `Type`: `LEDOn`
- `TargetComponentId`: Actual LED 1 binding ID
- Require electrical enabled: Yes
- Require conduction: Yes

### Target 2 — LED 2 On

- `TargetId`: `led_2_on`
- `Type`: `LEDOn`
- `TargetComponentId`: Actual LED 2 binding ID
- Require electrical enabled: Yes
- Require conduction: Yes

### Target 3 — Branch 1 Current

- `TargetId`: `branch_1_current`
- `Type`: `CurrentRange`
- `TargetComponentId`: Actual branch 1 resistor binding ID
- `MinimumCurrent`: `0.006`
- `UseMaximumCurrent`: `true`
- `MaximumCurrent`: `0.015`
- Require electrical enabled: Yes
- Require conduction: Yes

### Target 4 — Branch 2 Current

- `TargetId`: `branch_2_current`
- `Type`: `CurrentRange`
- `TargetComponentId`: Actual branch 2 resistor binding ID
- `MinimumCurrent`: `0.006`
- `UseMaximumCurrent`: `true`
- `MaximumCurrent`: `0.015`
- Require electrical enabled: Yes
- Require conduction: Yes

### Target 5 — Total Supply Current

- `TargetId`: `total_supply_current`
- `Type`: `CurrentRange`
- `TargetComponentId`: A measurement component representing total supply output current
- `MinimumCurrent`: `0`
- `UseMaximumCurrent`: `true`
- `MaximumCurrent`: `0.030`

**Critical implementation requirement:** The total-current target must measure the complete source output, not just one LED branch. Verify that the chosen electrical state exposes combined current correctly.

### Target ID Rules

Every `TargetId` must be unique.

A `TargetId` identifies the objective. A `TargetComponentId` identifies the runtime component being evaluated.

Never assume that a shelf item's ID or slot ID automatically equals the level's target component binding ID.

---

## 10. RUNTIME SCENE BINDINGS

In the Level 5 layout, ensure these bindings resolve correctly:

- Power supply
- Main switch
- LED 1
- LED 2
- Branch 1 resistor
- Branch 2 resistor
- Required source terminals
- Required LED terminals
- Appropriate total-current measurement component

If resistors can be replaced from the shelf, confirm the active binding points to the **currently installed resistor** after replacement.

This is particularly important because ProjectSpark currently distinguishes shelf placement from level objective bindings.

The level should not accidentally evaluate an old or uninstalled resistor.

---

## 11. PLAYER EXPERIENCE

### Phase A — Starting the Level

The player enters Level 5.

Initial state:

- Power supply present.
- Two LEDs visible.
- Resistors available on the shelf.
- Both branches incomplete.
- Level objectives displayed.

The workshop computer introduces the mission:

**"Two lights. Two paths. One power source. Build a safe parallel circuit."**

### Phase B — Build the First Branch

The player connects:

- Supply positive through switch
- First resistor
- LED 1
- Negative return

Expected outcome:

- LED 1 may light.
- LED 2 remains off.
- The level stays incomplete.

Ask Spark can explain that one working branch does not satisfy the mission.

### Phase C — Build the Second Branch

The player constructs another resistor-and-LED path between the same positive and negative rails.

Expected outcome:

- Both LEDs can operate.
- The evaluator checks current in both branches.

### Phase D — Optimize the Values

The player experiments with shelf resistor values.

The two branches may carry different currents.

Ask Spark should describe the actual current values rather than simply saying "wrong resistor."

### Phase E — Complete the Mission

The level completes when all five evaluator targets pass and the circuit has no disqualifying fault.

Progression can then unlock Level 6.

---

## 12. ASK SPARK AI BEHAVIOR

The existing offline Ask Spark system should remain global and work with Level 5 through its level definitions and bindings.

### INSPECT

When the player inspects a resistor:

- Show component name.
- Show resistance.
- Show measured voltage.
- Show measured current.
- Show measured power.
- Show conducting/disabled state if available.

When the player inspects an LED:

- Show operating state.
- Show relevant electrical measurements.
- Explain the component's role in its branch.

### EXPLAIN

The `SparkAILiveTroubleshooter` should report:

- Active level
- Objective completion count
- Short-circuit or return-path issues
- Pending objective names
- Measured electrical values
- Configured target ranges

Example:

**CIRCUIT STATUS**

Level: Parallel Paths | Objectives: 3/5

LED 1: PASS | LED 2: PASS

Branch 1 current: 20.0 mA | Required: 6–15 mA

**Diagnosis:** Branch 1 exceeds its maximum current. Inspect the corresponding resistor and compare the available resistor choices.

### HINT SYSTEM

**Hint 1 — Conceptual**

"Both LEDs need complete current paths from the positive supply to the negative return."

**Hint 2 — Directional**

"Build two separate resistor-and-LED branches. Connect both branches across the same supply rails."

**Hint 3 — Evidence-Based**

"Branch 1 current is outside its required range. Inspect the installed resistor and compare its measured current with the configured minimum and maximum."

Hints should always be driven by current evaluator results where possible.

### ASK

Possible player questions:

- What is a parallel circuit?
- Why does only one LED glow?
- Why do I need two resistors?
- What happens if I use 100 Ω?
- Why is my total current high?
- Why is the level not complete?
- What is the difference between series and parallel circuits?

The offline AI should explain the concept and refer to live circuit values when available.

---

## 13. COMMON PLAYER MISTAKES

### Mistake 1 — LEDs Connected in Series

The player connects:

`Supply → R1 → LED 1 → R2 → LED 2 → Return`

Why this is incorrect:

- The two LEDs do not have independent branches.
- They share one series current path.
- The connection does not demonstrate the intended parallel topology.

### Mistake 2 — One LED Reversed

Expected result:

- The affected branch may fail to conduct.
- Its LED objective remains pending.

### Mistake 3 — Missing Branch Return

Expected result:

- The incomplete branch carries no normal current.
- The corresponding LED fails to light.

### Mistake 4 — Resistor Too Small

Expected result:

- Branch current may exceed the configured maximum.
- LED operation alone does not complete the level.

### Mistake 5 — Resistor Too Large

Expected result:

- Branch current falls below its configured minimum.

### Mistake 6 — One Shared Current-Limiting Resistor

The player places one resistor before the circuit splits.

Why this is undesirable:

- The branches do not have independently selected current limits.
- LED current distribution may be unequal.
- The circuit differs from the intended design.

---

## 14. IMPORTANT EVALUATOR LIMITATIONS

Your current `SparkLevelEvaluator` supports:

- `LEDOn`
- `CurrentRange`
- `VoltageRange`
- `PowerRange`
- Electrical conduction checks
- Circuit fault checks

However, individual target checks alone may not prove that the LEDs are wired as **two separate parallel branches**.

A circuit could potentially satisfy some numerical conditions without matching the desired topology.

For strict Level 5 validation, a future topology-specific check should confirm:

1. There are two distinct LED-resistor paths.
2. Both originate from the switched positive rail.
3. Both return to the negative rail.
4. Each LED has its own series resistor.
5. The branches are not accidentally connected in series.
6. Probe-only connections do not count as conducting paths.

Do not implement this by guessing wire IDs. Use `SparkCircuitSystem` connectivity and the actual terminal/component conduction rules.

Until topology validation is implemented, treat Level 5 as a **measurement-driven level**, and manually test whether unintended configurations can pass.

---

## 15. WORKSHOP COMPUTER DISPLAY

### Level Header

**LEVEL 05 — PARALLEL PATHS**

### Mission Description

"Power two LEDs in parallel and keep all currents within their limits."

### Suggested Progress Display

The current evaluator may display:

`TARGETS 0/5`

As the player advances:

`TARGETS 1/5 → 2/5 → 3/5 → 4/5 → 5/5`

When all requirements pass:

**LEVEL COMPLETE**

### Suggested Live Measurements

Show where supported:

- Supply voltage
- Branch 1 current
- Branch 2 current
- Total source current

If the existing workshop computer only displays aggregate electrical values, keep that functionality unchanged initially. Additional per-branch measurements can be added later through manually created UI.

---

## 16. UNITY BUILD CHECKLIST

### Level Asset
- [ ] Create Level 5 definition.
- [ ] Set unique level ID.
- [ ] Add five evaluator targets.
- [ ] Set current minimums and maximums.
- [ ] Verify unique target IDs.

### Layout
- [ ] Create `Level05_ParallelPaths` layout.
- [ ] Place one 5 V supply.
- [ ] Place main switch.
- [ ] Place two LEDs.
- [ ] Provide two resistor slots.
- [ ] Establish positive and negative connections.
- [ ] Connect layout to level-loading system.

### Runtime Bindings
- [ ] Confirm both LED binding IDs.
- [ ] Confirm both resistor binding IDs.
- [ ] Confirm total-current measurement source.
- [ ] Confirm shelf replacements update the evaluated components.

### Electrical Tests
- [ ] Switch off prevents normal LED operation.
- [ ] First completed branch works.
- [ ] Second completed branch works.
- [ ] Reversed LED is detected.
- [ ] Missing return is detected.
- [ ] 100 Ω overcurrent example fails where expected.
- [ ] 1 kΩ undercurrent example fails where expected.
- [ ] 330 Ω + 330 Ω passes when solver readings satisfy requirements.
- [ ] Source current equals the sum of branch currents within solver tolerances.
- [ ] Direct source short is rejected.
- [ ] Unintended series topology does not win if strict topology validation is enabled.

### Ask Spark
- [ ] INSPECT identifies each component.
- [ ] EXPLAIN lists pending targets.
- [ ] HINT identifies the failing branch.
- [ ] ASK explains parallel circuit principles.
- [ ] Answers use the compact TextMeshPro layout.
- [ ] No UI is created dynamically.

### Progression
- [ ] Level 4 completion unlocks Level 5.
- [ ] Restart clears the previous layout.
- [ ] Level 5 success is recorded.
- [ ] Next-level flow works.

---

## 17. COMPLETION MESSAGE

**LEVEL 05 COMPLETE — PARALLEL PATHS**

"Excellent work! You successfully built two parallel LED branches and controlled the electrical current in each one."

**Skills Unlocked:**

- Parallel circuit design
- Branch current calculations
- Independent current limiting
- Total current management
- Multi-objective circuit troubleshooting

**Next Challenge:** More advanced circuit networks and fault diagnosis.

---

## 18. FINAL DESIGN DECISIONS

| Item | Decision |
|---|---|
| Level name | Parallel Paths |
| Core concept | Parallel circuits |
| Supply | 5 V DC |
| LED count | 2 |
| Resistor count | 2 installed |
| Switch count | 1 |
| Suggested current range | 6–15 mA per branch |
| Suggested total current limit | 30 mA |
| Example passing resistors | 330 Ω + 330 Ω |
| Player-facing mission goals | 3 |
| Evaluator targets | 5 |
| AI system | Existing offline Ask Spark |
| Runtime UI creation | None |
| Progression | Unlock after Level 4 |

**Implementation principle:** Reuse ProjectSpark's existing electrical solver, level manager, shelf system, inspector bindings, and manually designed UI. Add specialized functionality only where existing systems cannot validate or explain the new parallel-circuit behavior.

---

**END OF LEVEL 05 DESIGN DOCUMENT**
