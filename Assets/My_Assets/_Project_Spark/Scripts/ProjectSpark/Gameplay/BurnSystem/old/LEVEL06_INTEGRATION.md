# ProjectSpark Level 06 — Circuit Fault Detective: burn-system integration

## Status of supplied BurnSystem.zip
The supplied package already contains a substantial thermal/VFX system. This is an integration guide, not a claim that the full ProjectSpark Unity scene was compiled or play-tested.

## Recommended Level 06 (working with current evaluator)
- Duplicate the verified Level 05 parallel circuit layout as `Level06_CircuitFaultDetective`.
- Fault 1: intentionally leave branch 1 resistor-to-LED wire unconnected.
- Fault 2: install a low-value resistor in branch 2, configured so branch current is outside the safe band. Avoid assuming this causes thermal failure: 100 ohms with ~3 V across the resistor gives ~0.09 W, insufficient to reach the supplied thermal model's failure temperature with default settings.
- Target IDs (unique): led_1_restored (LEDOn), led_2_operates (LEDOn), branch_1_safe_current (CurrentRange 0.006 to 0.015 A), branch_2_safe_current (CurrentRange same), total_current_safe (CurrentRange 0 to 0.030 A).
- Verify the last target measures supply output current, not one branch.
- Normal mode: completed achievement stays completed. Post-win changes are for a separate inspection evaluation, not a reversible gameplay state.

## Existing integration from your pasted SparkResistor.cs
- `[RequireComponent(typeof(SparkResistorBurn))]` exists.
- `ApplyElectricalState()` calls `burnSystem.SetDissipatedPower(Mathf.Max(0f,state.Power))`.
- Resistor listens to `burnSystem.Failed` and requests electrical reevaluation.
- `EffectiveResistance` returns infinity for burned resistors, so the existing solver treats failure as OPEN CIRCUIT.
- `RepairResistor()` calls `ResetBurn()` and requests reevaluation.

## Critical compatibility rule
Keep `Electrical Failure Mode = OpenCircuit` on every `SparkResistorBurn` in this configuration. Although the burn class exposes `ShortCircuit`, the pasted resistor implementation *always* models burning as an open circuit. Do not select ShortCircuit unless the resistor conductance, path checks, state calculations, and evaluation have been explicitly updated and tested for that failure mode.

## Thermal tuning
Thermal steady-state rise, for constant power, is approximately `P / coolingWattsPerC` degrees Celsius above ambient. With defaults `P=0.09 W`, cooling `0.018 W/C`, ambient `25 C`, equilibrium is approximately `30 C` — nowhere near `failureC=230 C`.
If you want actual smoke/burning in later levels, tune from solver-measured dissipated power and use reasonable accelerated gameplay thermal settings. Check smoke threshold, failure threshold, and time-to-failure together; ratedWatts is metadata in this implementation and does not itself trigger failure.

## Unity Inspector (existing objects only)
1. Add `SparkResistorBurn` to each resistor prefab if not already present.
2. Set Failure Mode to OpenCircuit.
3. Assign only ceramic body renderers to Heated Renderers. Choose a compatible URP burn material/shader.
4. Either assign existing VisualEffect Graph components (Smoke VFX / Failure VFX) or enable Generate Runtime VFX. Avoid enabling both if you do not want doubled particles.
5. If using the supplied VisualEffect Graph file, confirm exposed property names `SmokeRate`, `Heat`, and event `OnBurn` match the script settings; fix actual graph if needed in Unity Editor.
6. On Level 06 reset, call the existing resistor repair/reset logic only where gameplay requires it; do not auto-repair during inspection.
7. Ensure replaced shelf resistors update the target component binding to the currently installed instance.

## QA scenarios
- Disconnect a branch: that branch's LED and current targets go pending.
- Reconnect: measure branch current and confirm objectives update.
- Choose a low-value resistor: current exceeds max and corresponding target fails.
- Choose 330 ohms: verify solver measurement, LED operation, and current targets.
- Force a burn with `Debug / Force Burn` context menu in Play mode: the resistor becomes open circuit, solved current falls on reevaluation, and its target fails.
- Repair with existing `RepairResistor()`: restored component can conduct after reevaluation.
- Test shelf replacement, two connections from the switch output, and both return paths.
- Verify normal-mode victory persists, and inspection-mode diagnostics do not change progression.

## No-new-UI rule
All player-facing UI stays in manually configured Unity Canvas/TextMeshPro objects. The burn system's optional particle child objects are VFX, not UI.
