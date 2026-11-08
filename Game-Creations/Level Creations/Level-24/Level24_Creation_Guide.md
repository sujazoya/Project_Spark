# PROJECT SPARK — LEVEL 24 CREATION PROCESS
## Opposite Directions — Diode Challenge

**Game:** Project Spark | **Level:** `level_24` | **Circuit:** 5 V, 330 Ω, two opposing 1N4007 diodes, green LED | **Targets:** 6/6

> This guide uses the uploaded and already prepared `Level_24_Opposite_Diodes_Replacement.txt`. It assumes your corrected resistance-based `SparkDiodeSolverDefinition`, diode Smart Slot category fix, and established level loader remain in place. No global solver changes are needed merely to assemble this level.

## 1 — Understand the required circuit

The diode pair is **anti-parallel**, not in series. The forward diode D1 carries current from the resistor to the green LED. The reverse diode D2 bridges the same two electrical nodes in the opposite direction; it must remain blocked.

```text
BATTERY (+5 V) -- SW1 -- R1 330 Ω --+-- D1 ANODE ->|-- D1 CATHODE --+-- LED ANODE ->|-- LED CATHODE -- BATTERY (-)
                                    |                               |
                                    +-- D2 CATHODE --|<- D2 ANODE ---+
```

**Important:** Both diode branches share exactly two electrical nodes, but D2 connects to them *with reversed polarity*. The two branches are not additional power supplies. Do not attach a bypass wire directly from the resistor output to the LED anode; that would bypass both diodes.

## 2 — Create the Level 24 prefab

1. Back up your completed Level 23 prefab and Level 23 definition.
2. Duplicate `Level_23_Layout` into `Level_24_Layout`; make the duplicate an independent prefab asset.
3. Rename the duplicate's level layout identity to `level_24` in the relevant layout component/loader catalog.
4. Retain `Power_Sources/PowerSupply_01`, the existing power switch, workbench, fixed green LED, and the physical resistor slot.
5. Keep **two** diode workstations, renumbered Slot_02 and Slot_03. Remove the unused third diode workstation (former Slot_04) and any old runtime binding declaration for it.
6. Make sure the layout loader catalog includes the Level 24 prefab and the gameplay manager has a Level 24 entry. Existing progression may keep Level 24 locked until Level 23 completion.
7. Save the **original prefab** outside Play Mode. Do not make persistent changes to `Level_24_Layout(Clone)`.

Recommended hierarchy:

```text
Level_24_Layout
├── Power_Sources
│   └── PowerSupply_01
├── Switch
│   └── Power_Switch
├── Slot_01
│   └── Resistor_330 Ω
├── Slot_02
│   └── Diode_1n4007_01   (D1: forward)
├── Slot_03
│   └── Diode_1n4007_02   (D2: reverse)
└── LED
    └── led                 (fixed SparkLED)
```

## 3 — Configure each SparkShelfSlot (Smart Runtime Bindings)

On the original prefab, enable `Enable Smart Binding` for all three workstation slots and confirm `Category` and `Kind = Workstation`:

| Slot ID | Category | Runtime Component ID | Runtime Terminal A ID | Runtime Terminal B ID |
|---|---|---|---|---|
| `Slot_01` | Resistor | `R_LIMIT_Component` | `R_LIMIT_Input` | `R_LIMIT_Output` |
| `Slot_02` | Diode | `D_FORWARD_Component` | `D_FORWARD_Anode` | `D_FORWARD_Cathode` |
| `Slot_03` | Diode | `D_REVERSE_Component` | `D_REVERSE_Anode` | `D_REVERSE_Cathode` |

**Physical terminal object names** inside both diode prefabs are `Diode_Anode` and `Diode_Cathode` (the same physical names can occur in both diode prefab instances). The resistor prefab uses `Register_Input` and `Register_Output`.

In `SparkLevelWorkstationBindingBridge`, keep **Use Smart Slots = ON**. Smart Slot diode matching must use suffixes `_Anode` and `_Cathode` rather than `_Input`/`_Output`. The smart slot's IDs take priority over legacy workstation rules. Avoid duplicate IDs or duplicate slot IDs. Make sure each diode's `SparkDiode.AnodeTerminal` and `.CathodeTerminal` are assigned to the matching terminal components.

## 4 — Declare runtime IDs in SparkLevelSceneBindings

Under **Runtime Component IDs**, declare exactly:

- `R_LIMIT_Component`
- `D_FORWARD_Component`
- `D_REVERSE_Component`

Under **Runtime Terminal IDs**, declare exactly:

- `R_LIMIT_Input`, `R_LIMIT_Output`
- `D_FORWARD_Anode`, `D_FORWARD_Cathode`
- `D_REVERSE_Anode`, `D_REVERSE_Cathode`

Keep the existing **fixed** bindings for `PowerSupply_01`, `Battery_positive`, `Battery_negative`, and the green LED. The imported definition uses fixed LED component `led`, terminal IDs `LED_Anode` and `LED_Cathode`. Ensure your prefab uses those exact IDs (do not silently replace with `LED_INDICATOR_*`). Do not declare the same alias as both fixed and dynamically owned.

## 5 — Correct 7 external wire connections

Use the **actual SparkTerminal objects** while drawing wires in game:

| Wire | From | To |
|---:|---|---|
| 1 | `Battery_positive` | Switch input |
| 2 | Switch output | `Register_Input` |
| 3 | `Register_Output` | D1 `Diode_Anode` |
| 4 | `Register_Output` | D2 `Diode_Cathode` |
| 5 | D1 `Diode_Cathode` | `LED_Anode` |
| 6 | D2 `Diode_Anode` | `LED_Anode` |
| 7 | `LED_Cathode` | `Battery_negative` |

The resistor output and LED anode each require **two wire connections**. Check that `SparkCircuitSystem` and your connection rules permit multi-attachment to a terminal; if not, do not force this topology without adapting that policy. A wire from D2 cathode to the resistor output and from D2 anode to LED anode is a reverse branch; D2 should not illuminate or pass appreciable forward current.

## 6 — Import Level_24 definition

1. Select the original `Level_24.asset` in the Project window.
2. Import `Level_24_Opposite_Diodes_Replacement.txt` with your custom editable ScriptableObject importer.
3. Keep its existing GUID and source object references. Do not create an unrelated asset by pasting the JSON.
4. Confirm `levelId = level_24`, name `Opposite Directions - Diode Challenge`, `requiredTargetCount = 6`, `requireClosedReturn = true`, `rejectShortCircuit = true`, `rejectTargetShort = true`.
5. Inspect target IDs against the table below before testing.

### Six imported objectives

| Target ID | Type | Required binding | Requirement |
|---|---|---|---|
| `level24_supply` | VoltageRange | `PowerSupply_01` | 4.8–5.2 V |
| `level24_d1_forward` | VoltageRange | `D_FORWARD_Component`; forward anode/cathode | 0.45–1.0 V |
| `level24_d2_blocked` | CurrentRange | `D_REVERSE_Component`; reverse anode/cathode | 0–0.00005 A |
| `level24_led_on` | LEDOn | `led`; `LED_Anode`, `LED_Cathode` | ON |
| `level24_safe_current` | CurrentRange | `R_LIMIT_Component` | 0.005–0.008 A |
| `level24_resistor_voltage` | VoltageRange | `R_LIMIT_Component` | 2.0–2.6 V |

**Critical evaluator validation:** The reverse branch is expected to have zero current. Some evaluator implementations reject an entirely nonconducting target or fail to resolve its current. Test `level24_d2_blocked` independently. If it cannot pass at 0 A, add a purpose-built reverse-blocked predicate in the evaluator *only after confirming the evaluator's actual code*; don't falsely mark the level complete or change diode physics to make the target pass.

## 7 — Test order

1. Start Level 24, verify `Active layout: True` and all three workstations are registered.
2. Press **Synchronize AND REPORT**: all 3 component IDs and all 6 terminal IDs must show bound to the right instances.
3. Close SW1 and connect the exact seven wires.
4. Run a solve and check `SolveNow` success, source output, resistor, LED and both diode states.
5. Expected approximate values: supply 5 V; resistor ~2.3 V, ~6.9 mA; D1 +0.7 V, ~6.9 mA; D2 -0.7 V, approximately 0 A; LED ~2.0 V, lit. Values depend on your actual solved device models.
6. Verify `Closed Return = YES`, `Source Short = NO`, `Target Short = NO`.
7. Verify 6/6 targets individually; if only 5/6 passes, inspect the failed target before changing thresholds.
8. Save prefab and level definition outside Play Mode, retest starting from the chapter/level manager.

## 8 — Troubleshooting

| Symptom | Likely next check |
|---|---|
| `D_FORWARD_*` / `D_REVERSE_*` UNBOUND | Slot category Diode; Smart Slots ON; binding IDs declared; bridge `_Anode`/`_Cathode` suffix matching |
| Both diodes pass current | Check D2 polarity and both branch endpoints; make sure D2 cathode connects to resistor output |
| Source current 0 A | Switch closed, connected return, diode orientation, solver convergence |
| Only one branch has voltage | Graph terminal registration and multi-attachment capability; inspect measured terminal voltage on both sides |
| D2 blocked, yet level 5/6 | Verify evaluator accepts valid 0 A for CurrentRange and that `D_REVERSE_Component` resolves |
| Resistor too much/too little current | Check actual resistor is 330 Ω, supply is 5 V, LED model voltage, and no bypass wire |
| Logs flood Console | Remove temporary per-iteration diode logs and diagnostic scans after testing |

## 9 — Drawing pages (optional visual teaching)

1. **Introduction:** Forward vs reverse bias and two opposing paths.
2. **Wiring:** Seven correctly numbered connections; show the two shared nodes.
3. **Operation:** D1 conducts; D2 blocks. Explain signed voltage across D2.
4. **Success:** Expected measurements and six objectives; actual completion must come from the level evaluator.

**Preserve:** Your working Level 21 transient driver, corrected `AddForwardDrop()` resistance stamp, current diode hysteresis/convergence fixes, and Smart Slot diode-category support. Do not reintroduce global continuous solving.
