# ProjectSpark — Resistor Burn System

## Requirements
- Unity with Universal Render Pipeline (URP).
- Visual Effect Graph package installed and supported by the active render pipeline.
- Existing ProjectSpark circuit solver and resistor component.

## Files
- `SparkResistorBurn.cs`: thermal simulation, material parameters, VFX hooks and failure event.
- `SparkResistorBurn_URP.shader`: simple URP unlit heat/char shader; for final production use Shader Graph Lit with texture maps, normal maps and emission.

## Unity wiring
1. Add `SparkResistorBurn` to the actual resistor GameObject (the one moved by SparkObjectShelf), not to the workstation slot.
2. Make a material with shader `ProjectSpark/ResistorBurnURP`; apply to resistor ceramic-body renderer(s) only. Assign those renderers to `Heated Renderers`.
3. Set `Rated Watts` to the resistor's actual power rating (e.g. 0.25 W). Note: rated watts is a metadata rating; heating is driven by measured dissipated power, not by this threshold alone.
4. Create child GameObjects `SmokeVFX`, `FailureVFX`, and optionally `HeatLight`. Assign components in Inspector. Disable heat light initially.
5. For each solver update call `burn.SetElectricalState(voltageAcrossResistorRms, currentThroughResistorRms, energized)` or `burn.SetDissipatedPower(actualResistorPowerWatts)`. When disconnected call `burn.SetDeenergized()`.
6. Subscribe to `burn.Failed` and change the authoritative solver resistor to an OPEN CIRCUIT (default) or short circuit as configured. Recalculate immediately. Do not simply change visual material; electrical failure must be modeled by solver.
7. On level reset or component replacement call `burn.ResetBurn()` and reset solver state. For permanently damaged gameplay, do not reset automatically.

## VFX Graph: smoke
1. Create `Visual Effects > Visual Effect Graph` asset `VFX_ResistorSmoke`.
2. Add exposed Blackboard **float** properties `SmokeRate` (default 0) and `Heat` (default 0).
3. Spawn Context: `Constant Spawn Rate` bound to `SmokeRate`.
4. Initialize Particle: lifetime random 0.7–1.8 seconds, size random 0.015–0.06 meters, initial velocity upward (0, 0.25, 0) m/s with slight random spread.
5. Update Particle: add gravity/drag/turbulence as appropriate; use noise to curl smoke upward.
6. Output Particle Quad: soft smoke flipbook texture, alpha blend, gray-to-dark-gray gradient over lifetime, fade alpha in and out; enable soft particles if supported.
7. Attach VFX component to `SmokeVFX` child and assign graph.

## VFX Graph: failure burst
1. Create `VFX_ResistorFailure` graph.
2. Add Event Context named `OnBurn` connected to a single burst spawn (e.g. 16–30 particles).
3. Initialize: lifetime 0.12–0.5 s, size 0.006–0.018 m, random sphere velocity 0.6–2 m/s.
4. Update: gravity, drag.
5. Output: additive glowing orange sparks, fade intensity over lifetime.
6. Optionally add a second `OnBurn` spawn for a brief gray smoke puff.
7. Assign VFX component to `FailureVFX` child.

## Shader Graph production upgrade
Create URP **Lit Shader Graph** `SG_ResistorBurnLit` with Blackboard floats `_Heat`, `_Char`, colors Base, Char, Glow. Lerp original resistor albedo texture to char texture with `_Char`; multiply emission glow by `Pow(_Heat, 3)` and `(1-_Char)`; connect emission to Fragment Emission. Enable HDR emission/bloom. Keep the original resistor color-band texture so resistance bands remain readable before failure.

## Electrical/thermal tuning
Example illustrative values: ambient 25 C, hot 90 C, smoke 145 C, fail 230 C, thermal capacity 1.2 J/C, cooling 0.018 W/C. At 0.25 W, steady-state temperature rise is ~13.9 C, so no burn; at 5 W, temperature rises substantially and can eventually fail. Tune by physical resistor package, rating, and gameplay time scale.

## Important limitations
- This script is an integration component, not a direct patch to your unknown SparkResistor / SparkCircuitSystem API. You must connect the solver result and the failure event to those APIs.
- URP shader is deliberately simple and unlit; production PBR should use the Shader Graph setup above.
- VFX Graph assets are created in the Unity editor following the graph instructions; they are not generated as `.vfx` assets here.
- No random burning; no thermal failure without solver-fed power.
