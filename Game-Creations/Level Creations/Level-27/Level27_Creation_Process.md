# Project Spark — Level 27 Creation Process

## Scope and compatibility

**Level ID:** `level_27`  
**Title:** Charge the Reservoir - Capacitor Smoothing  
**Electrical supply:** 5 V DC (existing `PowerSupply_01`)  
**Concept:** diode-isolated charged capacitor supplying a parallel LED/load branch.  
**Important:** A 5 V DC simulation cannot demonstrate AC ripple suppression. This deliverable is a DC reservoir lesson. Real half-wave AC/ripple evaluation requires a voltage waveform source and time-series evaluator, which have not been supplied.

The uploaded `Level_27_Editable.txt` actually contains copied `level26_*` targets and four-diode bridge descriptions. The fixed TXT replaces those with six Level 27-specific targets while preserving the export wrapper and asset GUID. This is an editable export for your existing Project Spark importer, **not** a native `.asset` file.

## Scene placement (matches Project Spark bench convention)

Use the same black breadboard with the 5 V supply and power switch on the left, replaceable workstation slots in the middle, green LED toward the right, and blue battery-negative return wire. This visual is a **proposed Level 27 layout**, not a verified screenshot of an actual Level 27 prefab.

```
Level_27_Layout
  Power_Sources/PowerSupply_01 (Battery_positive, Battery_negative)
  Switch/Power_Switch (SwitchInput, SwitchOutput)
  Slot_01  -> D_ISOLATE_Component (SparkDiode; physical terminals Diode_Anode, Diode_Cathode)
  Slot_02  -> R_CHARGE_Component (SparkResistor, 220 ohm; Register_Input, Register_Output)
  Slot_03  -> C_RESERVOIR_Component (SparkCapacitor, 1000 uF)
  Slot_04  -> R_LED_Component (SparkResistor, 330 ohm; Register_Input, Register_Output)
  LED/led  -> fixed LED component `led` (LED_Anode, LED_Cathode)
  SparkLevelSceneBindings (fixed IDs and runtime declarations)
  SparkLevelWorkstationBindingBridge (Use Smart Slots = ON)
  SparkLevel27ReservoirDriver (Level 27 local transient solves)
```

**Capacitor terminal role:** assign the actual positive/negative `SparkTerminal` references in the Smart Slot rule; do not assume `_Input` / `_Output` resolve automatically for capacitor category. Your current runtime bridge's `BuildRules()` special-cases only LED; a capacitor may need legacy rule for Slot_03 with **Use Smart Slots OFF on Slot_03 only**, while the bridge remains smart-enabled globally. Set legacy terminal patterns to exact physical capacitor terminal names, or add a capacitor category mapping in the bridge after inspecting its prefab. The declaration importer must include the legacy rule for Slot_03.

## Runtime IDs required

| Slot | Component ID | Terminal A | Terminal B |
|---|---|---|---|
| Slot_01 | `D_ISOLATE_Component` | `D_ISOLATE_Anode` | `D_ISOLATE_Cathode` |
| Slot_02 | `R_CHARGE_Component` | `R_CHARGE_Input` | `R_CHARGE_Output` |
| Slot_03 | `C_RESERVOIR_Component` | `C_RESERVOIR_Positive` | `C_RESERVOIR_Negative` |
| Slot_04 | `R_LED_Component` | `R_LED_Input` | `R_LED_Output` |

The `SparkLevelSceneBindings` prefab must declare **4 runtime component IDs and 8 runtime terminal IDs**. Keep fixed IDs for `PowerSupply_01`, `Battery_positive`, `Battery_negative`, `SwitchInput`, `SwitchOutput`, `led`, `LED_Anode`, and `LED_Cathode`.

## Numbered external wiring — exactly eight connections

| # | From | To |
|---|---|---|
| 1 | `Battery_positive` | `SwitchInput` |
| 2 | `SwitchOutput` | `D_ISOLATE_Anode` |
| 3 | `D_ISOLATE_Cathode` | `R_CHARGE_Input` |
| 4 | `R_CHARGE_Output` | `C_RESERVOIR_Positive` |
| 5 | `R_CHARGE_Output` | `R_LED_Input` |
| 6 | `R_LED_Output` | `LED_Anode` |
| 7 | `LED_Cathode` | `Battery_negative` |
| 8 | `C_RESERVOIR_Negative` | `Battery_negative` |

**Three conductive nets:** switch output feeds diode anode; charge resistor output is the common capacitor-positive / LED-resistor-input rail; capacitor-negative and LED cathode return to supply negative. Never connect battery negative directly to the output positive rail.

## Target definition (six targets)

| Target | Expected condition |
|---|---|
| 1 | Source voltage 4.8–5.2 V |
| 2 | D1 forward voltage 0.45–1.0 V |
| 3 | C1 loaded DC voltage 3.1–3.7 V |
| 4 | Green LED ON |
| 5 | 330 Ω LED branch current 3–6 mA |
| 6 | 220 Ω charge resistor voltage drop 0.7–1.2 V |

Approximate DC steady point using a 0.7 V protection diode and a 2 V LED: reservoir rail ~3.38 V, load current ~4.18 mA, 220 Ω drop ~0.92 V. Values are design estimates, not measured engine output. Your diode and LED models may move readings; adjust ranges after a successful source-current audit, not blindly.

## Import the fixed Level Definition

1. Back up the original `Level_27.asset`.
2. Import `Definition/Level_27_FIXED.txt` using your Project Spark **editable ScriptableObject import tool**, selecting the original Level 27 asset; this is not Unity's standard text import.
3. Verify `levelId = level_27`, `requiredTargetCount = 6`, `Targets = 6`, and the new Level 27 IDs.
4. Set the Level 27 layout catalog entry to the actual `Level_27_Layout.prefab` and the `LevelGamePlayManager` entry to the corrected `Level_27.asset`.
5. Ensure progression access for test purposes without changing earlier level saves.

## Build and test

1. Duplicate a safe layout base, **do not reuse Level 26's bridge target IDs**.
2. Place diode, 220Ω resistor, 1000µF capacitor and 330Ω LED resistor in Slots 01–04.
3. Configure runtime Smart Slot names, declaration arrays and fixed LED/supply bindings.
4. Add `SparkLevel27ReservoirDriver` to this Level 27 prefab only. Its default 20 Hz timed solves restart after topology and level-component configuration changes.
5. The existing capacitor solver definition must be registered in `SparkElectricalSolverRegistry`. Configure capacitance and transient timestep consistently; your shared solver already has `Simulate Transient Devices` and `Time Step` fields.
6. Attach only the eight numbered wires. Switch ON.
7. Run `Synchronize AND REPORT` then `Solved Source Current Audit` and confirm diode, both resistors, LED, capacitor all resolve.
8. Observe capacitor voltage rising and eventually a stable state; verify all six target results. Your evaluator tests a static voltage window, **not ripple**.

## Drawing creator

Run **Tools > Project Spark > Visual Teaching > Create Level 27 Drawing Asset (from template)**. Select an existing working `SparkLevelDrawingAsset` first. The creator clones the asset safely and modifies known metadata. It deliberately **does not assume the unknown serialized page/part/wire schema**. Replace the cloned template pages with Level 27 content from the included numbered PNG or SVG. To generate all four pages automatically, the exact `SparkLevelDrawingAsset` / `SparkSchematicPart` / `SparkSchematicWire` data definitions or a working Level 26 creator must be supplied.

Suggested pages: (1) Mission overview, (2) numbered eight-wire connections, (3) capacitor charge curve & RC explanation, (4) six completion targets.

## Troubleshooting

- `UNBOUND`: verify runtime declarations, smart-slot enablement, installed shelf item and capacitor terminal match rules.
- 0 A despite LEDs connected: check diode direction, switch and the eight connections.
- Overload: check accidental positive-to-negative external wire or LED branch missing 330Ω resistor.
- Capacitor voltage stays 0: check solver registry capacitor definition, terminal binding and level-only transient driver.
- Targets 1/6 only: use target diagnostics in `SparkLevelEvaluator` to identify exact unresolved target IDs.
- In the editor, a legacy-only button can report `Rules checked: 0` with Smart Slots active. Use Smart Slot-aware declaration tooling instead.
