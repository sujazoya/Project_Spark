# PROJECT SPARK — LEVEL 09 CREATION CANVAS

## Series–Parallel Circuit Master

**Purpose:** Build, configure, test, and ship Level 9 inside the existing single-scene Project Spark architecture.  
**Difficulty:** Intermediate → Advanced  
**Supply:** 5 V DC  
**Completion mode:** `AllTargets`  
**Objectives:** 4  
**Initial incorrect part:** Replaceable R3 = 220 Ω  
**Correct replacement:** R3 = 470 Ω  
**Target output:** approximately 2.342 V  
**Target source current:** approximately 12.081 mA

> **Scope:** Reuse the existing level loader, SparkElectricalSolver, resistor shelf, terminal/wire system, LCD meter, and level evaluator. Do not create a new scene, solver, or AI feature. This canvas is a design/build checklist, not a live scene verification.

---

## 1. Circuit topology

```text
                       POWER SWITCH        R1 = 220 Ω (FIXED)
Battery +5 V ─────────────o/ o────────────────[ R1 ]─────● VOUT
                                                       │
                                  ┌────────────────────┴─────────────────────┐
                                  │                                          │
                          R2 = 330 Ω (FIXED)                     R3 = 470 Ω (SHELF)
                               [ R2 ]                                     [ R3 ]
                                  │                                          │
Battery − / GND ───────────────────┴──────────────────────────────────────────┘

LCD Terminal A (+) → VOUT (junction after R1)
LCD Terminal B (−) → Battery_negative / GND
LCD is a high-impedance measuring branch, not the return conductor.
```

**Correct electrical structure:** `R1 + (R2 || R3)`. R1 carries the full source current; R2 and R3 carry different branch currents at the same VOUT voltage.

### Design calculation (ideal 5 V, nominal resistors)

- `Rparallel = 330 × 470 / (330 + 470) = 193.875 Ω`
- `Rtotal = 220 + 193.875 = 413.875 Ω`
- `Isource = 5 / 413.875 = 0.0120809 A ≈ 12.08 mA`
- `VOUT = Isource × 193.875 ≈ 2.342 V`
- `I_R2 = VOUT / 330 ≈ 7.098 mA`
- `I_R3 = VOUT / 470 ≈ 4.983 mA`
- `V_R1 = 5 − VOUT ≈ 2.658 V`

**These are expected ideal values only; the actual solved readings are authoritative.**

---

## 2. Create the level asset and layout

- [ ] Duplicate `Level_08_Layout` as `Level_09_Layout` (prefab/layout, **not** a new gameplay scene).
- [ ] Duplicate the prior `SparkLevelDefinition` asset and name it `Level_09`.
- [ ] Set Level ID to **`level_09`**.
- [ ] Set Display Name to **`Series–Parallel Circuit Master`**.
- [ ] Set description: **`Calibrate a combined series–parallel resistor network by replacing the incorrect branch resistor.`**
- [ ] Register the new layout and definition with your existing loader/level catalogue.
- [ ] Confirm the active level loads as `Level_09_Layout(Clone)` and has the correct `SparkLevelSceneBindings`.
- [ ] Keep persistent shelf, solver, LCD infrastructure, and HUD in the shared scene.

---

## 3. Set up workbench components

| Component | Quantity | Initial state | Notes |
|---|---:|---|---|
| 5 V power supply | 1 | Existing | Reuse bindings `Battery_positive`, `Battery_negative` |
| Power switch | 1 | OFF | Reuse `SwitchOutput` |
| R1 | 1 | 220 Ω fixed | Total-current path |
| R2 | 1 | 330 Ω fixed | Stable binding for voltage and branch-current targets |
| R3 | 1 | 220 Ω **incorrect** | Replaceable from shelf; 470 Ω is correct |
| Universal LCD meter | 1 | High impedance | A to VOUT, B to GND |
| Resistor shelf | Existing | 100/150/220/330/470/1k Ω | R3 replacement inventory |

- [ ] Place fixed R1 = **220 Ω** inside the solver's component search root.
- [ ] Place fixed R2 = **330 Ω** inside the solver's component search root.
- [ ] Configure a replacement placement slot for R3; install incorrect **220 Ω** initially.
- [ ] Confirm shelf-placed R3 is actually inside the solver's effective discovery hierarchy when placed.
- [ ] Ensure stored inventory components are not energized or considered installed loads.
- [ ] Verify no unintended branches or cross-connections exist.

---

## 4. Exact wiring — seven connections

The **Battery_positive → Switch Input** connection is assumed to be already provided in the level prefab. Verify it separately.

| Wire | From | To | Purpose |
|---|---|---|---|
| 1 | `SwitchOutput` | R1 `Register_Input` | Supply into series resistor |
| 2 | R1 `Register_Output` | R2 `Register_Input` | Split into fixed parallel branch |
| 3 | R1 `Register_Output` | R3 `Register_Input` | Split into replaceable parallel branch |
| 4 | R2 `Register_Output` | `Battery_negative` | R2 physical ground return |
| 5 | R3 `Register_Output` | `Battery_negative` | R3 physical ground return |
| 6 | `LCD_Terminal_A` | R1 `Register_Output` (VOUT) | Measure output node |
| 7 | `LCD_Terminal_B` | `Battery_negative` | Ground-referenced LCD measurement |

> **Connection capacity:** The VOUT electrical node joins R1 output, R2 input, R3 input and LCD A. If your `SparkTerminal` accepts only one direct wire, create equivalent electrically connected junction terminals using the existing wiring system rather than bypassing capacity restrictions.

- [ ] Check all seven electrical connections appear in `SparkCircuitSystem`, not merely as visible cable meshes.
- [ ] Confirm R2 and R3 return to battery negative **directly**, not through the LCD input impedance.
- [ ] Confirm LCD mode is **not `InlineCurrent`** when wired across VOUT and GND.
- [ ] Confirm the level's polarity validator accepts the configured `Register_Input` and `Register_Output` orientation.

---

## 5. Configure LCD

| Inspector field | Value |
|---|---|
| `SparkLCDMeter.MeasurementMode` | `AutoVoltageAndComponentCurrent` |
| `SparkLCDMeter.TerminalA` | Physical `LCD_Terminal_A` at VOUT |
| `SparkLCDMeter.TerminalB` | Physical `LCD_Terminal_B` at battery negative |
| `Voltage Input Resistance` | 10,000,000 Ω |
| `SparkLCDMeterDisplay` component current source | Fixed R2 = 330 Ω |
| `Solver` | Existing active `SparkElectricalSolver` |

Expected LCD at completed circuit:

```text
VOUT  +2.34 V
R2 I   7.10 mA
```

- [ ] Verify `SparkLCDMeterSolverDefinition` asset is in the existing solver registry.
- [ ] Verify LCD appears in `SparkElectricalSolver.ElectricalComponents`.
- [ ] Confirm A/B terminal bindings are distinct and connected.
- [ ] Confirm LCD reports a fresh applied solver result.
- [ ] Confirm the current source is fixed R2, not the replaceable R3 reference.

---

## 6. Configure all four objectives

**Level completion mode:** `AllTargets`; **Required Target Count:** `4`.

| # | Objective | Target type | Stable component binding | Min | Max |
|---|---|---|---|---:|---:|
| 1 | Complete Series–Parallel Circuit | `ComponentConducting` | Fixed R1 (220 Ω) | Conducting | — |
| 2 | Calibrate Parallel Voltage | `VoltageRange` | Fixed R2 (330 Ω) | 2.25 V | 2.45 V |
| 3 | Maintain Safe Branch Current | `CurrentRange` | Fixed R2 (330 Ω) | 0.0067 A | 0.0075 A |
| 4 | Calibrate Total Source Current | `CurrentRange` | `PowerSupply_01` | 0.0118 A | 0.0124 A |

### Objective 1
- [ ] Target Type = `ComponentConducting`.
- [ ] Target Component ID = stable binding for fixed R1.
- [ ] Require Electrical Enabled = true.
- [ ] Require Conduction = true.

### Objective 2
- [ ] Target Type = `VoltageRange`.
- [ ] Target Component ID = stable binding for fixed R2.
- [ ] Minimum Voltage = `2.25`.
- [ ] Maximum Voltage = `2.45`.
- [ ] **Use Maximum Voltage = true**.
- [ ] Verify the expected signed voltage direction in Play Mode before setting required positive/negative terminal IDs.

### Objective 3
- [ ] Target Type = `CurrentRange`.
- [ ] Target Component ID = stable binding for fixed R2.
- [ ] Minimum Current = `0.0067`.
- [ ] Maximum Current = `0.0075`.
- [ ] **Use Maximum Current = true**.

### Objective 4
- [ ] Target Type = `CurrentRange`.
- [ ] Target Component ID = `PowerSupply_01` (or the exact bound source ID).
- [ ] Minimum Current = `0.0118`.
- [ ] Maximum Current = `0.0124`.
- [ ] **Use Maximum Current = true**.
- [ ] Verify whether source current is signed positive or negative and whether the evaluator expects magnitude.

**Never bind completion objectives to replaceable R3**, whose GameObject instance changes when replaced.

---

## 7. Level rules and progression

- [ ] Completion Mode = `AllTargets`.
- [ ] Required Target Count = `4`.
- [ ] Require Closed Return = **true**.
- [ ] Reject Short Circuit = **true**.
- [ ] Reject Target Short = **true**.
- [ ] Allow Intermediate Connections = **true**.
- [ ] Require Healthy Installed Resistors = **true**.
- [ ] Unlocked By Default = **false**.
- [ ] Auto Unlock Next Level = **true**.
- [ ] Auto Advance On Completion = **false**.
- [ ] Verify successful completion remains permanent and Level 10 unlocks.

---

## 8. Resistor replacement experiment

Approximate results with R1 = 220 Ω, R2 = 330 Ω, supply = 5 V:

| Installed R3 | VOUT | Total source current | Intended result |
|---|---:|---:|---|
| 100 Ω | 1.293 V | 16.850 mA | Fail |
| 150 Ω | 1.596 V | 15.474 mA | Fail |
| 220 Ω (starting) | 1.875 V | 14.205 mA | Fail |
| 330 Ω | 2.143 V | 12.987 mA | Fail |
| **470 Ω (correct)** | **2.342 V** | **12.081 mA** | **Pass** |
| 1 kΩ | 2.650 V | 10.681 mA | Fail |

The actual solver includes LCD loading and any other modeled elements; slight deviations are normal.

- [ ] Start with R3 = 220 Ω and confirm the level does **not** pass.
- [ ] Remove old R3 from the workbench and ensure it is excluded from active electrical stamping.
- [ ] Install R3 = 470 Ω from the shelf.
- [ ] Reconnect both R3 terminals to the appropriate electrical nodes.
- [ ] Confirm replacement R3 is discovered by the solver and solver topology is refreshed.
- [ ] Verify R2/R3 share VOUT and GND nodes.

---

## 9. Play Mode verification

### Electrical registration
- [ ] All installed resistors are found by the solver.
- [ ] Fixed R1, fixed R2 and placed R3 have both valid graph terminals.
- [ ] No old or stored shelf resistor is being counted as an active load.
- [ ] Solver has a fresh successfully applied result.

### Measurements
- [ ] Supply voltage ≈ 5.00 V.
- [ ] R1 voltage drop ≈ 2.658 V.
- [ ] VOUT / R2 voltage ≈ 2.342 V.
- [ ] R1 current ≈ 12.081 mA.
- [ ] R2 current ≈ 7.098 mA.
- [ ] R3 current ≈ 4.983 mA.
- [ ] Total source current ≈ 12.081 mA.
- [ ] LCD shows ≈ 2.34 V and ≈ 7.10 mA.
- [ ] No spurious invalid-polarity or open-return evaluation after correct wiring.

### Victory
- [ ] All four objectives show complete.
- [ ] No installed resistor is failed or burned.
- [ ] Victory is recorded once and remains permanent.
- [ ] Level 10 unlocks.
- [ ] Reset/reload restores intended starting state without stale bindings.

---

## 10. Troubleshooting quick reference

| Symptom | Check first |
|---|---|
| LCD reads ≈ 5 V with tiny current | Physical R2/R3 ground return missing; LCD is not a return conductor |
| Resistor reports 0 A but wires look connected | Solver discovery root, `SparkCircuitSystem` connections, graph node IDs |
| Wrong voltage at VOUT | Wrong resistor order/value or missing parallel branch |
| Invalid polarity | Bound terminal orientation / level evaluator's required positive-negative IDs |
| 3/4 objectives | `Use Maximum ...` toggles, target bindings, source-current sign |
| Current changes when R3 is removed but not installed | Replacement slot discovery/refresh and stale component references |

---

## Definition of done

Level 9 starts with the wrong R3 resistor and a combined series–parallel circuit. The player replaces R3 with **470 Ω**, obtains approximately **2.342 V at VOUT** and **12.081 mA from the source**, passes **all four evaluator objectives**, and permanently unlocks Level 10—all using the existing Project Spark systems.
