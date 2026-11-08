# Project Spark - Level 26 Complete Unity Creation Process

## Level specification
**Level ID:** `level_26`  
**Name:** Bridge Defender - Four Diode Rectifier  
**Source:** 5 V DC  
**Devices:** one switch, four 1N4007 diodes, one 220 ohm resistor, one green LED  
**Completion:** all six targets, normal DC input polarity only.

## Safety/compatibility
Do not edit `SparkElectricalSolver.cs`, the Level 21 transient driver, or diode equations. Reuse the already corrected `SparkDiodeSolverDefinition` (`AddForwardDrop` receives resistance and internally stamps conductance). This pack does **not** test reversed battery polarity automatically.

## Step 1 - Duplicate a working prefab
In Project window duplicate your working `Level_25_Layout` prefab to `Level_26_Layout`. Keep its power source, fixed LED, standard switch, `SparkLevelSceneBindings`, `SparkLevelWorkstationBindingBridge`, and shelf systems. Remove the extra Level 25 switch and its old wires. Add two more diode workstations, so the new layout has 1 resistor workstation and 4 diode workstations. Save original prefab outside Play Mode.

## Step 2 - Set up the hierarchy
```text
Level_26_Layout
  Power_Sources/PowerSupply_01
  Switch/Power_Switch
  Slot_01/Resistor_220 Ohm
  Slot_02/Diode_1n4007_01 (D1)
  Slot_03/Diode_1n4007_02 (D2)
  Slot_04/Diode_1n4007_03 (D3)
  Slot_05/Diode_1n4007_04 (D4)
  LED/led
```

## Step 3 - Smart Slot bindings
All slots should be Category `Resistor` or `Diode`, Kind `Workstation`, `Enable Smart Binding = ON`, and each SlotId unique. The bridge must have `Use Smart Slots = ON`.

| Slot ID | Component Binding ID | Terminal A | Terminal B |
|---|---|---|---|
| Slot_01 | R_LIMIT_Component | R_LIMIT_Input | R_LIMIT_Output |
| Slot_02 | D_BRIDGE_1_Component | D_BRIDGE_1_Anode | D_BRIDGE_1_Cathode |
| Slot_03 | D_BRIDGE_2_Component | D_BRIDGE_2_Anode | D_BRIDGE_2_Cathode |
| Slot_04 | D_BRIDGE_3_Component | D_BRIDGE_3_Anode | D_BRIDGE_3_Cathode |
| Slot_05 | D_BRIDGE_4_Component | D_BRIDGE_4_Anode | D_BRIDGE_4_Cathode |

Diode physical terminal GameObject names stay `Diode_Anode` and `Diode_Cathode`. Slot mapping IDs are distinct from GameObject names. The diode Smart Slot bridge must support the `_Anode` and `_Cathode` suffixes (your Level 23/24 fix).

## Step 4 - Declare IDs in original prefab Scene Bindings
In `SparkLevelSceneBindings` runtime declarations, enter **5 component IDs** and **10 terminal IDs** exactly as shown above. Leave fixed source `PowerSupply_01`/`Battery_positive`/`Battery_negative` and LED `led`/`LED_Anode`/`LED_Cathode` bindings untouched. No duplicate IDs. **If any rule is undeclared, the entire bridge fails closed**, potentially leaving all IDs `UNBOUND`.

## Step 5 - Place diodes and connect ten external wires
**Electrical bridge nets:** A = SwitchOutput; B = Battery_negative; P = R_LIMIT_Input; N = LED_Cathode. Diode directions: D1 A→P; D2 B→P; D3 N→A; D4 N→B.

| Wire # | From | To |
|---:|---|---|
| 1 | `Battery_positive` | `SwitchInput` |
| 2 | `SwitchOutput` | `D_BRIDGE_1_Anode` |
| 3 | `SwitchOutput` | `D_BRIDGE_3_Cathode` |
| 4 | `Battery_negative` | `D_BRIDGE_2_Anode` |
| 5 | `Battery_negative` | `D_BRIDGE_4_Cathode` |
| 6 | `D_BRIDGE_1_Cathode` | `R_LIMIT_Input` |
| 7 | `D_BRIDGE_2_Cathode` | `R_LIMIT_Input` |
| 8 | `R_LIMIT_Output` | `LED_Anode` |
| 9 | `LED_Cathode` | `D_BRIDGE_3_Anode` |
| 10 | `LED_Cathode` | `D_BRIDGE_4_Anode` |

**Important:** Each shared junction requires two wire connections to the same `SparkTerminal` (SwitchOutput, Battery_negative, R_LIMIT_Input, LED_Cathode). Confirm your wire system accepts multiple connections to one terminal. Do not connect the diode paths in a single series chain. The net schematic PNG is authoritative; the scene-style overview is only a spatial placement guide.

## Step 6 - Import Level Definition
Select the **original** `Level_26.asset` in Unity; import `Level_26_Bridge_Rectifier_FIXED.txt` through your existing ScriptableObject TXT importer (do not create a new asset with the same GUID). The exported GUID and section markers have been preserved. Set `LevelGamePlayManager.levels[25]` to this definition, register Level 26 in your layout catalog/loader, and unlock it for testing using your normal progression workflow.

## Step 7 - Create the Visual Teaching asset
Put `SparkLevel26DrawingAssetCreator.cs` into an `Assets/Editor/` folder. Wait for Unity compilation. Run `Tools > Project Spark > Visual Teaching > Create Level 26 Drawing Asset`, choose `Level_26_Drawing.asset`, and assign it in your Visual Teaching UI/catalogue. Pages: overview; 10 exact wires; conduction pair; measurements and objectives.

## Step 8 - Verify six targets
| Target ID | Type | Range / state |
|---|---|---|
| level26_supply | VoltageRange | 4.8 to 5.2 V |
| level26_diode1_forward | VoltageRange | D1: 0.45 to 1.0 V |
| level26_diode4_forward | VoltageRange | D4: 0.45 to 1.0 V |
| level26_led_on | LEDOn | ON |
| level26_safe_current | CurrentRange | 0.006 to 0.009 A |
| level26_resistor_voltage | VoltageRange | 1.3 to 1.9 V |

Nominal expected: D1=+0.7 V, D4=+0.7 V, D2/D3 reverse-biased and 0 A, resistor about 1.6 V, LED about 2.0 V, current about 7.3 mA. **These are estimates**; compare with your solved-current audit before finalizing level tolerances.

## Step 9 - Acceptance checklist
- [ ] Level 26 layout clone is active and the switch is closed
- [ ] The solver discovers four distinct SparkDiode instances and applies correct definitions
- [ ] `SMART SLOT BINDINGS` has all 15 runtime aliases BOUND (five components plus ten terminals)
- [ ] All four diode anode/cathode terminal references are assigned correctly
- [ ] D1+D4 conduct; D2+D3 block
- [ ] Green LED is ON; current is in 6-9 mA range
- [ ] `Closed Return=YES`, `Source Short=NO`, `Target Short=NO`
- [ ] Level 26 reaches 6/6 targets

## Troubleshooting
- **Undeclared component/terminal:** Add the missing runtime ID to the **original prefab** Scene Bindings. An invalid slot prevents all rules from binding.
- **All IDs valid but diode UNBOUND:** Verify Smart Slot category `Diode`, suffix matching `_Anode/_Cathode`, and that each installed shelf item contains one `SparkDiode` and both `SparkTerminal` children.
- **Only 1 diode passes current:** Review the four net directions; only D1 and D4 should conduct in normal polarity.
- **InvalidTarget with 4/6 or 5/6:** Inspect the individual target result; confirm D4 ID and current/voltage tolerances.
- **Solver fails:** Check terminal graph inclusion and nonlinear diode hysteresis already verified on Level 23; do not change the shared solver blindly.
- **Resistor too hot/current out of range:** Ensure 220 ohm installed, not Level 25’s 330 ohm.

## Note on polarity reversal
This deliverable evaluates only the normal 5 V DC input orientation. Reversing DC input and recording the other conduction pair would require a separate controlled test state and likely a level-specific tracker; do not assume the six simultaneous Level Definition targets enforce this test.
