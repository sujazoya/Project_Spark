# Material and Shader Reference

This kit includes 17 shared PBR material sets and 2 original hologram shaders.

## Unity native PBR material generation

The Editor installer creates real `.mat` assets based on **native Lit / Standard shaders**:

- Universal Render Pipeline: `Universal Render Pipeline/Lit`
- High Definition Render Pipeline: `HDRP/Lit`
- Built-in Render Pipeline: `Standard`

Material slots in the FBX meshes are named `PS_graphite`, `PS_titanium`, `PS_cyan`, etc. The room builder remaps them to the shared generated materials. Texture maps and emissions remain editable in Unity.

## Original shaders

- `PS_RoomHologramURP.shader`: URP translucent scan/Fresnel shader with time-varying emissive highlighting.
- `PS_RoomHologramBuiltin.shader`: equivalent for Unity Built-in rendering.

HDRP uses `HDRP/Unlit` for the cyan hologram under the material installer; the remaining assets use HDRP Lit. This is a deliberate compatibility fallback rather than an unverified promise of a custom native HDRP hologram shader.

## Packed channels

| Map | R | G | B | A |
|---|---|---|---|---|
| UnityMaskMap | Metallic | AO | Unused/Detail | Smoothness |
| MetalRoughAO | Metallic | Roughness | AO | Optional |
| Normal | Encoded tangent-space X | Encoded tangent-space Y | Encoded tangent-space Z | N/A |

For normal maps, use `Normal Map` texture type and trilinear mipmaps. For mask maps, disable sRGB. For basecolor and emissive, enable sRGB.

## Visual target

Brushed dark titanium and graphite, satin bronze hardware, ceramic accent panels, glossy smart glass, carefully limited cyan and amber emissive trim, soft room lighting, and anti-aliased reflections. The intent is a premium understated engineering room, **not** a cluttered sci-fi warehouse.
