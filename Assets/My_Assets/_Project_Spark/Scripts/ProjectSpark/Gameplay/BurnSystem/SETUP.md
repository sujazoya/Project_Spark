# ProjectSpark — One-script cinematic resistor burn

## Replace and compatibility

1. Back up your old `SparkResistorBurn.cs`. Replace it with the new `SparkResistorBurn.cs`, **not an additional script**. The class retains `SetDissipatedPower`, `SetElectricalState`, `SetDeenergized`, `ResetBurn`, `TemperatureC`, `PowerWatts`, `IsFailed`, `RatedWatts`, `ElectricalFailureMode`, `State`, and `Failed`.
2. Keep the existing `SparkResistor.cs` with `[RequireComponent(typeof(SparkResistorBurn))]` and its `Failed` event handler. The solver must use `resistor.EffectiveResistance` to treat a failed resistor as an open circuit.
3. Turn off/remove previous `SparkBurnCinematicVFX`, `SparkResistorSmokeGraphDriver`, `SparkResistorFailureBurstVFX`, and demo components; they otherwise compete for the same VFX properties or duplicate bursts. Do not remove `SparkResistorBurn` itself.
4. All systems are created and assigned **manually in the Unity Editor**. This script never creates GameObjects, materials, UI, or ParticleSystems at runtime.

## Manually authored prefab hierarchy

ResistorPrefab (SparkResistor, SparkResistorBurn)
  Body (MeshRenderer with cinematic char material)
  BurnFX (Transform around center of resistor body)
    Flames (ParticleSystem)
    Embers (ParticleSystem, optional)
    Sparks (ParticleSystem)
    Flash (ParticleSystem)
    Debris (ParticleSystem)
    Smoke (VisualEffect, optional; or a ParticleSystem)
    HeatLight (Point Light, optional)
    FlashLight (Point Light, optional)

Assign all fields on **the same `SparkResistorBurn` component**. Leave unused effects null. Prefer VFX Graph smoke and leave `Smoke Particles` empty to avoid duplicates.

## Mesh burning

1. Create a new material. Shader: `ProjectSpark/Resistor Cinematic Char URP` (included).
2. Apply it to the resistor **body mesh only**. The material supports `_Heat` and `_Char`, which the controller drives. Its built-in mask generates black scorching and ember patterns from the mesh UVs.
3. Assign the **Body Renderer** to `Heated Renderers` (one element); exclude wire and terminal renderers.
4. Enable HDR bloom in your URP volume if desired, tune the shader's Glow Strength and Ember Color. For ordinary URP Lit body materials, the script darkens `_BaseColor` as a fallback, but patterned char is shader-dependent.

## Flames (key to non-basic visual quality)

1. Create a ParticleSystem `Flames` manually; Play On Awake ON is OK because the script sets emission to zero on start, but disable **Emission Bursts** to avoid startup flashes.
2. Recommended: Looping ON; simulation space Local, lifetime 0.25–0.5 s; speed 0.05–0.18; start size roughly 0.02–0.05 world units for a small resistor; Shape Box small, centered just above the resistor body, positive Y velocity, Color over Lifetime pale-yellow → orange → red/transparent; Size over Lifetime starts narrow, peaks, then vanishes.
3. Create a particle material using a URP transparent/unlit particle-capable shader and `PS_FlameFlipbook_4x4.png`; use Alpha Blending or Additive for flame core. Particle System > Texture Sheet Animation: Grid; Tiles X=4 Y=4; Animation Whole Sheet; Frame over Time linear 0..1. Soft particles and distortion are optional.
4. Set the `Flame Particles` Inspector reference on `SparkResistorBurn`; the script controls emission from temperature and makes flames appear briefly after failure. **No flame particles = no visible fire**.

## Sparks / debris / flash

Create `Sparks`, `Flash`, and `Debris` ParticleSystems in Editor. Set Play On Awake OFF, Emission Rate Over Time=0, no configured bursts, and Loop OFF. The script calls Emit() once on failure. Sparks: orange/yellow, lifetime 0.2–0.6 s, speed 0.6–1.8, size 0.005–0.015, gravity 0.5, trails. Flash: one larger bright billboard, lifetime 0.1 s, speed 0. Debris: dark rectangular fragments, lifetime 0.4–0.9 s, speed 0.25–1, gravity 0.8. Parent all to an existing pivot near the resistor center.

Create the Point Lights manually; set `Heat Light` and `Failure Flash Light` references. Do not use very large ranges for a small workbench resistor (start range about 0.3–0.7 Unity units and tune to scene scale).

## Smoke Graph

Use the `VFX_ResistorSmoke` graph with exposed float `SmokeRate` linked to Constant Spawn Rate, default **0**. Assign its VisualEffect to `Smoke Vfx`. The script drives the rate; do not also use `SparkResistorSmokeGraphDriver`.

## Suggested thermal preset (gameplay testing, not a physical guarantee)

Ambient=25°C; Hot=85°C; Smoke=120°C; Flame=170°C; Failure=225°C; Capacity=0.06 J/°C; Cooling=0.002 W/°C; Failure Exposure=0.3 s. At constant power P, equilibrium temperature approximately `25 + P/0.002` °C: 0.09 W → 70°C (no burn), 0.5 W → 275°C (eventual failure). Heating duration depends on capacity and current solver output. To make an overpowered resistor burn faster, lower Capacity; to raise equilibrium temperature, lower Cooling. Do not force all normal 5V LED resistors to burn.

**FailureMode:** use OpenCircuit. The existing SparkResistor/Solver does not implement ShortCircuit failure semantics.

## Testing

Enter Play Mode, select resistor > SparkResistorBurn component context menu:
- `Debug / Preview Cinematic VFX Only (Play Mode)` — flash, sparks, debris, flame and smoke without electrical failure. The visual-only preview temporarily chars the mesh, then returns it to normal.
- `Debug / Force Burn (Play Mode)` — actual resistor failure and solver re-evaluation via your existing SparkResistor event subscription. Leaves resistor blackened until repaired.
- `Debug / Reset Burn` — resets visuals and failure state. In gameplay, prefer `SparkResistor.RepairResistor()` because it also requests electrical re-evaluation.

## Unity integration notes

- Keep all visual controllers consolidated here. The shader and flipbook are visual assets, not C# controllers.
- Class compiles against Unity 6 with Visual Effect Graph package installed, as in your supplied source. Real Play Mode testing is still necessary.
- Avoid very high particle emission rates and large light ranges; believable AAA results require well-configured textures, material, size curves and HDR bloom, not more scripts.
