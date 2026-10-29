# ProjectSpark Cinematic Resistor Burn — One Controller + One Shader

## What this upgrades
- Progressive **mesh char / scorching** and orange ember cracks (requires material made from supplied shader)
- Local flame particles that grow as the resistor overheats and fade after failure
- One-shot sparks, flash, and flying debris
- Existing VFX Graph SmokeRate with a smoke tail
- Short point-light burst
- No runtime GameObject creation; no electrical or level state changes.

## Import
1. Put `SparkBurnCinematicVFX.cs` in your Gameplay scripts folder.
2. Put `ProjectSpark_ResistorChar_URP.shader` in Assets/Shaders.
3. Create a **Material** in the Unity Editor using `ProjectSpark/Resistor Cinematic Char URP`.
4. Assign the resistor's existing body texture to **Resistor Body Texture**; match its starting tint. Assign that material to the body mesh's desired material slot. Preserve a backup of the old material.
5. On existing resistor / FailureVFX object, add `SparkBurnCinematicVFX`. Assign **Burn** and **Resistor Body**. If using a multi-material renderer, set Material Index appropriately.
6. Assign existing `VFX_ResistorSmoke` VisualEffect and `Sparks`, `Flash`, `Debris` ParticleSystems, and a manually created `FlashLight` Point Light.
7. Create **Flames** ParticleSystem by duplicating your existing emitter in the **Editor** (not at runtime), then configure below and assign it.

## Flame emitter — main settings
- Play On Awake: OFF; Looping: ON; Duration: 1 s; Start Lifetime: 0.15–0.4 s; Start Speed: 0.05–0.18; Start Size: 0.02–0.055 for a physically small resistor; Simulation Space: Local.
- Emission: Rate over Time 0 (script controls it); no bursts.
- Shape: Cone pointing upward, angle 9–16 degrees, radius 0.012–0.02 (adapt to resistor mesh), positioned on top of resistor body.
- Velocity over Lifetime: Y 0.15–0.35. Noise: strength 0.06–0.15, frequency 2–4.
- Color over Lifetime: yellow-white at birth -> hot orange -> deep red -> transparent. Size over Lifetime: grows then shrinks to 0. Render: billboard with soft flame flipbook texture, additive or alpha-blended URP particle material; enable bloom.
- If you use billboard without a flame texture, the result will look like round particles. A proper flame flipbook and material are the most important visual quality additions.

## Flash Light
- Create an existing (Editor-authored) Point Light at resistor center. Intensity 0 initially, Range ~0.5–1.5 scene units; adjust for scale. Assign to controller.

## CRITICAL: disable duplicate drivers
The existing `SparkResistorBurn` already writes shader `_Heat`, `_Char`, VFX `SmokeRate`, heat light and bursts. To prevent conflicts:
- On `SparkResistorBurn`: empty **Heated Renderers**, **Smoke Vfx**, **Failure Vfx**, **Heat Light**, and ParticleSystem references that this new controller owns. Set **Emit Internal Failure Burst = false**.
- Disable/remove `SparkResistorSmokeGraphDriver` and `SparkResistorFailureBurstVFX` from this resistor while testing this controller.
- Keep the thermal and electrical logic of `SparkResistorBurn` active. Do not change the solver or shelf.
- If the upgraded burn inspector uses serialized arrays, set Heated Renderers array Size=0; do not remove your mesh.

## Demo
- In Play Mode choose component context menu `VFX / Visual-Only Failure Preview (Play Mode)`. It triggers sparks, flash, char, flames and smoke without burning the electrical resistor.
- For real failure: use `SparkResistorBurn` context menu `Debug / Force Burn`. It raises `Failed` and triggers the exact same visuals plus electrical failure.
- If smoke still runs at idle, set VisualEffect's overridden `SmokeRate` to 0 and confirm no other driver controls it.

## Notes
- This is deliberately an addon, not a rewrite of thermal simulation. Simple architecture: 1 existing burn script + 1 visual controller + 1 shader + manually authored emitter assets.
- `ProjectSpark/Resistor Cinematic Char URP` is a lightweight stylized URP shader. It requires URP and valid mesh UVs for the paint/char pattern.
- Render quality depends heavily on the actual authored flame texture/flipbook, materials, post-processing bloom and particle tuning.

## Supplied flame flipbook (major visual improvement)
- `PS_FlameFlipbook_4x4.png` is a 4 x 4 (16-frame) procedural fire texture atlas with transparency.
- Import it into Unity. In Texture Import Settings, choose Alpha Is Transparency and clamp wrapping. Keep filtering bilinear.
- Create a material using a compatible URP particle shader with transparent blending (for example URP/Particles/Unlit), and assign this texture as the base map.
- Assign that material to the `Flames` ParticleSystem Renderer.
- In `Flames` -> **Texture Sheet Animation**: enable, Mode Grid, Tiles X=4, Y=4, Animation Whole Sheet, Frame over Time 0→1, Start Frame 0. Use randomized start frame if preferred.
- For the best look use a small emission cone along the resistor, soft particles if supported, low transparency near edges and Bloom. Tune in Game view at actual camera distance.
