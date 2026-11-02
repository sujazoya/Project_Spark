# PROJECT SPARK — LEVEL 8 COMPLETE CREATION CHECKLIST

**Level ID:** `level_08`  
**Title:** Parallel Current Divider  
**Difficulty:** Intermediate  
**Supply:** 5 V DC  
**Completion mode:** `AllTargets`  
**Required targets:** 4  
**Purpose:** Learn parallel resistance and current distribution by replacing an unsuitable branch resistor with 470 Ω.

> **Implementation scope:** Reuse the existing single Unity gameplay scene, electrical solver, shelf, wiring tools, LCD meter, objective evaluator, and level progression. Do not create another solver or duplicate the gameplay scene.

## 1. Create the Level 8 assets

- [ ] Duplicate `Level_07_Layout` to `Level_08_Layout` (prefab/layout, **not** the entire gameplay scene).
- [ ] Create or duplicate a `SparkLevelDefinition` asset named `Level_08`.
- [ ] Set **Level ID** to `level_08`.
- [ ] Set **Display Name** to `Parallel Current Divider`.
- [ ] Set **Description** to `Complete two parallel resistor branches and replace the unsuitable resistor to reach the target source current.`
- [ ] Register the layout and definition with the existing level-loading/progression configuration.
- [ ] Confirm the runtime `SparkLevelSceneBindings` resolves all fixed components and terminals used by the objectives.

## 2. Arrange the workbench components

- [ ] Keep one existing 5 V supply and power switch.
- [ ] Install **R1 = 330 Ω** as the *fixed* Branch 1 resistor.
- [ ] Set up Branch 2's replacement slot with **R2 = 220 Ω** initially (intentionally incorrect).
- [ ] Make **470 Ω** available on the resistor shelf as the correct replacement.
- [ ] Reuse the LCD electronic meter with two real terminals, A and B.
- [ ] Keep the existing resistor shelf, wire tool, HUD, and reset mechanism.
- [ ] Ensure the shelf hierarchy is beneath the electrical solver's component search root.
- [ ] Confirm inactive shelf stock is not treated as electrically installed.

## 3. Create the actual parallel circuit

```
                         SWITCH
Battery +5V ──────────────o/ o──────┬─────────────┐
                                   │             │
                                R1 330Ω       R2 470Ω
                                 FIXED        REPLACEABLE
                                   │             │
Battery − / GND ────────────────────┴─────────────┘

LCD Terminal A → positive bus after the switch
LCD Terminal B → negative / ground bus
```

**Power supply positive → switch input** should already be connected as in previous levels.

### Six exact player connections

- [ ] **1:** `SwitchOutput` → R1 `Register_Input`.
- [ ] **2:** `SwitchOutput` → R2 `Register_Input`.
- [ ] **3:** R1 `Register_Output` → `Battery_negative`.
- [ ] **4:** R2 `Register_Output` → `Battery_negative`.
- [ ] **5:** `LCD_Terminal_A` → `SwitchOutput` (positive bus after switch).
- [ ] **6:** `LCD_Terminal_B` → `Battery_negative`.
- [ ] Verify the two resistor branches are **parallel**, not series.
- [ ] Verify electrical `SparkCircuitSystem` connections exist, not only visible cable meshes.
- [ ] Verify that terminal connection capacities permit both load branches and LCD wiring.

**Important:** The LCD in voltage/analysis mode is high impedance and must not be used as either resistor's main return path. Never connect the LCD in `InlineCurrent` mode directly across the 5 V supply.

## 4. Configure the LCD

- [ ] Set `SparkLCDMeter.MeasurementMode` to `AutoVoltageAndComponentCurrent`.
- [ ] Assign the LCD's **Terminal A** and **Terminal B** to their real `SparkTerminal` components.
- [ ] Connect Terminal A to the switched positive bus and B to GND.
- [ ] In `SparkLCDMeterDisplay`, choose the **fixed 330 Ω R1** as the component-current source (use its stable binding or direct reference).
- [ ] Confirm voltage input resistance is approximately **10 MΩ**.
- [ ] Register `SparkLCDMeterSolverDefinition` in the existing `SparkElectricalSolverRegistry`.
- [ ] Verify the LCD displays approximately **5.00 V** across A/B and **15.15 mA** for fixed R1 after a valid solve.
- [ ] For a *total source current* readout, switch to `SourceCurrent` and assign the 5 V source, subject to its current-sign convention.

## 5. Configure the four objectives

Set `SparkLevelDefinition.CompletionMode` to **AllTargets** and required target count to **4**.

| # | Display name | Target type | Stable target component | Pass condition |
|---|---|---|---|---|
| 1 | Complete Parallel Branch | `ComponentConducting` | Fixed 330 Ω R1 | R1 conducting |
| 2 | Maintain Branch Voltage | `VoltageRange` | Fixed 330 Ω R1 | 4.90–5.10 V |
| 3 | Verify Branch 1 Current | `CurrentRange` | Fixed 330 Ω R1 | 0.014–0.016 A |
| 4 | Calibrate Total Source Current | `CurrentRange` | `PowerSupply_01` | 0.025–0.027 A |

- [ ] **Objective 1:** `ComponentConducting` points to the fixed R1 component binding; require electrical enabled and conducting.
- [ ] **Objective 2:** `VoltageRange` targets fixed R1, minimum `4.90`, maximum `5.10`, and **Use Maximum Voltage = true**.
- [ ] **Objective 3:** `CurrentRange` targets fixed R1, minimum `0.014 A`, maximum `0.016 A`, and **Use Maximum Current = true**.
- [ ] **Objective 4:** `CurrentRange` targets `PowerSupply_01`, minimum `0.025 A`, maximum `0.027 A`, and **Use Maximum Current = true**.
- [ ] Verify `Target Component Id` strings exactly match the active `SparkLevelSceneBindings` IDs.
- [ ] Confirm fixed-R1 voltage and current signs agree with the evaluator; if required, correct bindings/orientation instead of masking bad wiring.
- [ ] Confirm `PowerSupply_01.ElectricalState.Current` actually reports delivered total source current with the evaluator's expected sign. If not, correct the objective's measurement source before shipping.
- [ ] Do **not** bind objectives to replaceable R2; its instance changes during replacement.

## 6. Level rules and progression

| Setting | Value |
|---|---|
| Level ID | `level_08` |
| Display Name | `Parallel Current Divider` |
| Completion Mode | `AllTargets` |
| Required Target Count | `4` |
| Require Closed Return | `true` |
| Reject Short Circuit | `true` |
| Reject Target Short | `true` |
| Allow Intermediate Connections | `true` |
| Require Healthy Installed Resistors | `true` |
| Unlocked By Default | `false` |
| Auto Unlock Next Level | `true` |
| Auto Advance On Completion | `false` |

- [ ] Enter the values above in the Level 8 ScriptableObject.
- [ ] Verify source terminal IDs (`Battery_positive`, `Battery_negative`) in Level 8 bindings.
- [ ] Keep source voltage nominally 5 V.
- [ ] Verify Level 8 unlocks after Level 7 according to existing progression configuration.
- [ ] Verify Level 9 unlocks after a successful Level 8 completion.

## 7. Nominal electrical calculations

The following values assume an ideal 5.00 V source with two resistor branches only. **The live solver remains authoritative.**

| R2 choice | R1 330 Ω current | R2 current | Total source current | Expected objective result |
|---|---:|---:|---:|---|
| 100 Ω | 15.15 mA | 50.00 mA | 65.15 mA | Fail |
| 150 Ω | 15.15 mA | 33.33 mA | 48.48 mA | Fail |
| 220 Ω (initial) | 15.15 mA | 22.73 mA | 37.88 mA | Fail |
| 330 Ω | 15.15 mA | 15.15 mA | 30.30 mA | Fail |
| **470 Ω (correct)** | **15.15 mA** | **10.64 mA** | **25.79 mA** | **Pass** |
| 1 kΩ | 15.15 mA | 5.00 mA | 20.15 mA | Fail |

**With R2 = 470 Ω:**
- Voltage across each branch: approximately **5.00 V**.
- R1 dissipated power: approximately **75.8 mW**.
- R2 dissipated power: approximately **53.2 mW**.
- Equivalent parallel resistance: approximately **193.9 Ω**.
- LCD auto display (referencing R1): `VAB +5.00 V` and `R1 I 15.15 mA`.

## 8. Play Mode test sequence

### Initial faulty state
- [ ] Load Level 8 with R2 = 220 Ω and switch OFF.
- [ ] Turn switch ON after connecting the intended circuit.
- [ ] Verify R1 ≈ 15.15 mA.
- [ ] Verify R2 ≈ 22.73 mA.
- [ ] Verify source current ≈ 37.88 mA.
- [ ] Confirm Level 8 **does not** pass the 25–27 mA source-current target.

### Correct resistor replacement
- [ ] Switch OFF before replacing the resistor.
- [ ] Remove R2 = 220 Ω from Branch 2.
- [ ] Install R2 = 470 Ω from the shelf.
- [ ] Confirm shelf resistor is inside the solver's component search root.
- [ ] Confirm the old 220 Ω resistor is no longer registered as an installed branch component.
- [ ] Confirm the new 470 Ω resistor is discovered by the solver.
- [ ] Verify both branches are electrically connected and a fresh solve succeeds.
- [ ] Turn switch ON and confirm R1 ≈ 15.15 mA and R2 ≈ 10.64 mA.
- [ ] Confirm source current ≈ 25.79 mA.
- [ ] Confirm both branch voltages ≈ 5.00 V.

### Failure/edge conditions
- [ ] Removing either branch return connection makes that branch nonconducting.
- [ ] A short circuit is rejected according to the existing evaluator rules.
- [ ] Incorrectly connecting the branches in series does not pass.
- [ ] A failed/burned installed resistor blocks victory if required by the level rules.
- [ ] LCD remains a measurement branch; it never substitutes for a missing resistor return.
- [ ] Switching the LCD to inline-current mode is not part of the intended solution.

### Victory and progression
- [ ] Check all **4/4 objectives** pass with the correct 470 Ω resistor.
- [ ] Confirm no required installed resistor is burned.
- [ ] Confirm Level 8 completes once and displays the existing completion feedback.
- [ ] Confirm Level 9 unlocks.
- [ ] Confirm permanent completion persists after restarting/reloading the level.

## 9. Definition of Done

Level 8 is complete when the player can wire two parallel resistor branches, measure their common voltage and differing currents, replace the unsuitable 220 Ω shelf resistor with 470 Ω, achieve all four objectives based on **actual solver results**, and permanently finish the level without adding new electrical solver systems or AI dependencies.

---

**Project Spark note:** The schematic, values, and objectives here are design targets. Validate actual IDs, wiring, solver current conventions, and completed gameplay in Unity Play Mode.
