# PROJECT SPARK — LEVEL 28 COMPLETE DESIGN

## Identity and learning goal
- **ID:** `level_28`
- **Name:** Emergency Power Backup — Charge / Backup Selector
- **Chapter:** Capacitor Energy Storage and Power Failover
- **Core concept:** Isolate a capacitor while charging it through a diode/resistor, then use an SPDT selector to power a load from the charged capacitor.
- **Prerequisite:** Level 27 charge the reservoir.
- **State:** **Implementation-ready design specification, not a compiled or Play-Mode-verified level.**

## Scene and slots
| Location | Game object | Value | Runtime ID |
|---|---|---|---|
| Fixed | PowerSupply_01 | 5 V DC | `Battery_positive`, `Battery_negative` |
| Fixed | Main power switch | Existing | `SwitchInput`, `SwitchOutput` |
| Slot_01 | Schottky diode 1N5819 | forward drop configurable (suggest 0.25–0.45 V) | `D_PROTECT_Component`, `D_PROTECT_Anode`, `D_PROTECT_Cathode` |
| Slot_02 | Charging resistor | 220 Ω | `R_CHARGE_Component`, `R_CHARGE_Input`, `R_CHARGE_Output` |
| Slot_03 | Electrolytic capacitor | 4700 µF / 16 V | `C_BACKUP_Component`, `C_BACKUP_Positive`, `C_BACKUP_Negative` |
| Slot_04 | LED series resistor | 330 Ω | `R_LED_Component`, `R_LED_Input`, `R_LED_Output` |
| Slot_05 | Parallel bleed/load resistor | 1 kΩ | `R_BLEED_Component`, `R_BLEED_Input`, `R_BLEED_Output` |
| Slot_06 | SPDT ON-ON break-before-make selector | positions CHARGE/BACKUP | `SPDT_Component`, `SPDT_A`, `SPDT_COM`, `SPDT_B` |
| Fixed | Green LED | existing | `led`, `LED_Anode`, `LED_Cathode` |

**Placement:** Battery left, fixed ON/OFF switch just right of battery. Diode above left; 220 Ω next. SPDT prominently center with clearly marked A(top-left), COM(bottom center), B(top-right). Capacitor beneath selector. LED load 330 Ω→green LED on right. Bleed 1 kΩ on the lower-right branch. Blue common return bus across board bottom.

## Exact eleven external wires
| # | From | To | Meaning |
|---|---|---|---|
| 1 | `Battery_positive` | `SwitchInput` | Main power feed |
| 2 | `SwitchOutput` | `D_PROTECT_Anode` | Schottky diode input |
| 3 | `D_PROTECT_Cathode` | `R_CHARGE_Input` | Diode into charge resistor |
| 4 | `R_CHARGE_Output` | `SPDT_A` | Charge input (throw A) |
| 5 | `SPDT_COM` | `C_BACKUP_Positive` | Selector common to capacitor + |
| 6 | `C_BACKUP_Negative` | `Battery_negative` | Capacitor ground return |
| 7 | `SPDT_B` | `R_LED_Input` | Backup LED branch |
| 8 | `R_LED_Output` | `LED_Anode` | LED current-limiter output |
| 9 | `LED_Cathode` | `Battery_negative` | LED ground return |
| 10 | `SPDT_B` | `R_BLEED_Input` | Backup bleeder branch |
| 11 | `R_BLEED_Output` | `Battery_negative` | Bleeder ground return |

**Electrical topology:** the selector common is **only** tied to capacitor positive. CHARGE position closes **A↔COM** and leaves B open. BACKUP position closes **COM↔B** and leaves A open. Do not connect all three SPDT terminals at once. Choose a break-before-make transition. Capacitor negative, LED cathode, bleed output, and battery negative are one common return net.

## Challenge sequence
1. **Build:** Install 1N5819, 220 Ω, 4700 µF, 330 Ω, 1 kΩ, and SPDT in six specified slots, attach eleven wires.
2. **Charge:** Main power ON, selector CHARGE. Source feeds diode → 220 Ω → selector A → COM → capacitor+. LED/bleed output are disconnected, and the capacitor charges toward approximately 4.6–4.8 V for a 0.2–0.4 V diode drop (model-dependent).
3. **Isolate:** Main power OFF. Capacitor voltage should remain approximately stable with only leakage (simulated as configured), and the LED remains OFF.
4. **Backup:** Selector BACKUP. Capacitor supplies 330 Ω+LED and 1 kΩ bleed branch in parallel. LED lights briefly; the capacitor voltage falls. Battery/source is disconnected by the switch and selector.
5. **Finish:** Observe a minimum backup illumination duration and eventual LED-off after voltage decreases. Award completion once phase events are recorded, **not** when all phase-specific conditions are simultaneously true.

## Proposed six latched objectives (level-local temporal evaluator)
1. All six required component categories/values installed with all 11 external wires and no prohibited short.
2. Main power ON and SPDT CHARGE: capacitor reaches **≥4.4 V** (calibrate to configured diode model).
3. Main power OFF in CHARGE: capacitor retains **≥4.2 V** for at least 0.2 s (leakage/model dependent).
4. Main power OFF and selector BACKUP: the green LED becomes energized **from the capacitor**.
5. Hold-up: LED remains ON for **≥0.35 s** after moving to BACKUP (tune against actual 4700 µF transient implementation).
6. Safe decay: capacitor voltage decreases by **≥1.0 V** after entering BACKUP and LED eventually turns OFF; no overcurrent or short.

**Do not author these as six simultaneous `SparkLevelDefinition.targets` checks.** The current static Level evaluator measures instantaneous quantities. Level 28 needs a local **state machine** that latches evidence from CHARGE, ISOLATED and BACKUP phases and signals completion once all goals pass. Avoid assuming the enum offers a temporal target type.

## Engineering / Unity implementation
- Duplicate **Level_27_Layout** to `Level_28_Layout` to retain fixed source, switch and fixed LED IDs, but replace and add slots as listed. Do not reuse Level 27 level ID or targets.
- Declare runtime IDs for all six slots in `SparkLevelSceneBindings`.
- Smart-slot matching: diode uses `_Anode/_Cathode`; resistors use `_Input/_Output`; capacitor uses `_Positive/_Negative` (check physical prefab names); **SPDT has 3 terminals**, unlike the existing 2-terminal `Rule` model.
- For SPDT **extend per-level wiring/binding** rather than modifying two-terminal global assumptions blindly. Bind A and COM via a legacy/manual alias if supported, and register B separately with a level-specific binding. A visual FBX alone cannot change circuit topology.
- Use a level-specific three-terminal SPDT component implementing `A–COM` in CHARGE and `B–COM` in BACKUP. Never stamp both closed at once; provide a neutral/open interval if the switch implementation supports break-before-make. Disconnect/un-stamp stale edges on throw changes.
- The 1N5819 is a **Schottky variant of a diode**, not a plain 1N4007. Configure forward drop to the device's current-dependent model; suggested 0.25–0.45 V is a teaching range rather than a guaranteed reading.
- Implement capacitor charge/discharge in `SparkLevel28BackupDriver` or equivalent level-local transient integration using the project's Level 21/27 transient architecture; preserve stored voltage when the source/selector changes. The static DC solver cannot alone model hold-up time.
- LED state should derive from instantaneous capacitor-derived current; clear it as stored voltage drops below the LED's conduction threshold.
- `SparkLevelDefinition` should keep safeguards enabled (`requireClosedReturn` where applicable, `rejectShortCircuit`, `rejectTargetShort`) but must not demand source-connected return while the selector is in isolated backup mode unless the evaluator supports that topology. The temporal driver owns phase completion.
- Create the visual teaching asset with the included Editor script. It creates **four visual-only pages** and makes no physical circuit wires.

## Visual teaching pages
1. Full workbench + six slots and 11 connections.
2. CHARGE: A↔COM (main ON); blue return; LED OFF.
3. ISOLATE & BACKUP: source OFF, selector COM↔B; LED ON from capacitor.
4. Six targets + safe switch sequence; demonstrate capacitor voltage decay.

## Validation / acceptance testing
- CHARGE and BACKUP contacts must be mutually exclusive on each simulator update.
- Negative capacitor terminal must never be connected to battery positive.
- 220 Ω charging resistor and Schottky diode must remain in series on the source side.
- In CHARGE, load branch must be disconnected and LED OFF.
- In BACKUP with main source OFF, LED must receive positive current only from the stored capacitor and eventually turn OFF.
- Never report 6/6 until both phases have been visited and validated.
- Test empty slot, reversed diode, reversed polarized capacitor, incorrect SPDT position, miswired bleed resistor, circuit short, and switching repeatedly.

## Timing estimates (illustrative, not simulator readings)
Initial BACKUP current if C≈4.6 V and LED≈2.0 V: (4.6−2.0)/330 ≈ 7.9 mA through LED plus 4.6/1000 ≈ 4.6 mA through bleed. With 4700 µF, initial voltage decay rate ≈ 2.65 V/s; LED hold-up is typically order of a second, depending on device models and state update. Adjust targets after Play Mode measurements.
