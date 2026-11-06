# PROJECT SPARK — LEVEL 22 CREATION PROCESS
## One Way Only — Diode Direction

**Goal:** Create Level 22 as an independent 5 V diode-direction lesson. The accompanying drawings are a schematic and an *illustrative* workbench layout, not an exported Unity prefab or proof of exact physical terminal positions.

### 0. Before editing
1. Back up the project and check that `SparkDiode`, `SparkDiodeSolverDefinition`, `SparkLED`, `SparkResistor`, `SparkElectricalSolver` and `SparkElectricalSolverRegistry` compile.
2. Keep Level 21 unchanged. Work on the ORIGINAL `Level_22_Layout` prefab in Prefab Mode; do not configure a `Level_22_Layout(Clone)` during Play Mode.
3. Select the ORIGINAL `Level_22.asset`, then import the previously generated `Level_22_Diode_Direction_Fixed.txt`. Confirm `LevelId=level_22`, four targets, and the source IDs.

### 1. Author the physical layout
Suggested hierarchy (only names, not a mandatory automation contract):
```
Level_22_Layout
  Power_Sources/PowerSupply_01
  Switch/Power_Switch
  Register_Slot_LIMIT/Resistor_330ohm
  Diode_Slot_01/Diode_1N4007
  LED/LED_Green
  LCD_Display (optional probes, high impedance)
```
Assign the existing `SparkLevelLayout`, `SparkLevelSceneBindings`, and `SparkLevelWorkstationBindingBridge` on the level layout as your earlier levels require. Reuse the 5 V supply and switch prefabs. Make the resistor, diode and LED terminals visible and wire-selectable.

### 2. Configure components
- **Power supply:** 5.0 V, enabled, positive terminal `Battery_positive`, negative terminal `Battery_negative`.
- **Switch:** its real input and output terminals must switch the circuit, not just animate.
- **R1:** 330 Ω, healthy and electrically enabled; terminal owners should be the resistor, not an unrelated prefab.
- **D1:** `SparkDiode` with actual ANODE and CATHODE terminals. Position the *banded cathode* toward LED1. Verify actual field/property names in the installed diode class; do not substitute resistor names.
- **LED1:** green LED; its ANODE faces D1 cathode and its CATHODE faces the battery return.
- **Shelf:** `SparkShelfSlot.Kind = Workstation`, relevant category, `SparkShelfItem` matching component category, and correctly registered occupant for replaceable components.

### 3. Exact six electrical wire connections
| Wire | From | To |
|---|---|---|
| W1 | `Battery_positive` | `Power_Switch` input |
| W2 | `Power_Switch` output | `R1` input |
| W3 | `R1` output | `Diode_1N4007` ANODE |
| W4 | `Diode_1N4007` CATHODE (banded end) | `LED_Green` ANODE |
| W5 | `LED_Green` CATHODE | `Battery_negative` |

Five **wire segments**, one series path. The sixth logical electrical connection is within the switch/device models; do not directly wire a device's own terminals together. Actual internal terminal GameObject names may differ; use the terminal references on the installed components.

### 4. Runtime binding IDs (case-sensitive)
**Runtime Component IDs:** `D_PROTECT_Component`, `R_LIMIT_Component`, `LED_INDICATOR_Component` (if LED is a fixed object; declare if used by its target).
**Runtime Terminal IDs:** `D_PROTECT_Anode`, `D_PROTECT_Cathode`, `R_LIMIT_Input`, `R_LIMIT_Output`, `LED_INDICATOR_Anode`, `LED_INDICATOR_Cathode`.
**Source IDs:** `PowerSupply_01`, `Battery_positive`, `Battery_negative`.

**Diode workstation:** SlotId `Diode_Slot_01`, ComponentBindingId `D_PROTECT_Component`, InputBindingId `D_PROTECT_Anode`, OutputBindingId `D_PROTECT_Cathode`, ElectricalTypeName `SparkDiode`, and *actual* terminal GameObject names for ANODE/CATHODE.
**Resistor workstation:** SlotId `Register_Slot_LIMIT`, ComponentBindingId `R_LIMIT_Component`, InputBindingId `R_LIMIT_Input`, OutputBindingId `R_LIMIT_Output`, ElectricalTypeName `SparkResistor`, and correct terminal names (`_Input`/`_Output` if your prefab uses those).
**Fixed LED:** map directly in `SparkLevelSceneBindings` to `LED_INDICATOR_Component`, `LED_INDICATOR_Anode`, `LED_INDICATOR_Cathode` if the LED is not a shelf-installed workstation component.

Choose one mechanism per workstation: `Use Smart Slots = true` with configured smart slot IDs, **or** `Use Smart Slots = false` with validated legacy rules. If the diode has UNBOUND aliases, inspect the bridge's electrical-type discovery support and terminal names. Do not hide failures by changing evaluator thresholds.

### 5. Level definition / completion
- LevelId: `level_22`; display: `One Way Only - Diode Direction`.
- RequireClosedReturn: `true`; rejectShortCircuit: `true`; rejectTargetShort: `true`.
- RequiredTargetCount: `4`.
- `level22_supply` — VoltageRange: 4.8–5.2 V.
- `level22_diode_forward` — VoltageRange across D1: 0.45–1.0 V (nominal; verify solver).
- `level22_led_on` — LEDOn: green LED lit.
- `level22_safe_current` — CurrentRange through 330 Ω: 0.005–0.008 A.
- Plan: source 5 V, diode ~0.7 V, green LED ~2.1 V, R1 ~2.2 V, I ~6.7 mA. These are example nominal values, **not** verified solver outcomes.

### 6. Play Mode acceptance test
1. Enter Play Mode with switch open: LED OFF; inspect polarity and bridge aliases.
2. Close SW1: source 5 V; diode should conduct forward; green LED ON; current ideally near 6.7 mA.
3. Open switch and reverse D1 **only while unpowered**, then close: diode reverse blocks; LED OFF. Restore D1 orientation before completion.
4. Run `SparkLevelWorkstationBindingBridge > Synchronize + Report Now`; inspect `D_PROTECT_*` and `R_LIMIT_*` for BOUND and fixed LED IDs for RESOLVED.
5. Inspect `[LEVEL TARGET RESULT]` for all four targets. Require all `Passed=True`, correct component and terminal pair resolution, and no source/target shorts. Tune nominal ranges only after a valid electrical audit.
6. Save the original prefab; verify Level 22 loads through the existing level catalog and progression.

### 7. Common blockers
- **1/4 or 2/4:** source passes but diode or LED aliases are UNRESOLVED: fix runtime declarations, bridge type names, slot occupant registration and terminal discovery.
- **Diode 0 V or LED OFF:** check switch, diode band direction, LED orientation, wiring, and diode solver definition registration.
- **LED never OFF when D1 reversed:** inspect whether wire endpoints bypass the diode or the diode solver's reverse branch behavior.
- **Wrong current:** inspect actual diode/LED I-V models and the installed 330 Ω resistor before changing target thresholds.
- **Repeated solver logs:** gate routine Debug.Log statements; keep errors and warnings. No transient scheduler should be introduced for a DC diode circuit solely for animation.

### 8. Whiteboard tutorial plan
Page 1: Anode and cathode, identify the band. Page 2: exact five wire segments. Page 3: forward versus reverse bias, LED ON/OFF. Page 4: measured current/voltage and four goals.

### Deliverables / limitations
This package contains a PDF guide, editable Markdown guide, vector schematic SVG, a PNG of the schematic and an illustrated workbench PNG. It does NOT contain a compiled Unity prefab, an imported .unity scene, executable diode mechanics or a working download button in the concept artwork.
