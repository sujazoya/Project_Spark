# Level 94 AAA-inspired polish & lighting specification

## Visual composition

- Board is the hero object; instruments occupy the outer edges, not the center.
- Board PCB mask: emerald green, polished ENIG gold contacts, white silkscreen and miniature lead-free solder terminations.
- Surrounding hardware: dark anodized graphite, brushed titanium, ESD-safe antistatic rubber and cyan edge lighting.
- Restrained amber accents communicate safety/active load; green power and blue status LEDs remain clearly distinct.
- Use one readable PCB-scanning light above the populated board with subtle reflections, not excessive particle effects.

## Recommended native Unity setup

- Use one primary neutral inspection light (shadowed), dim warm amber rim lighting and cool cyan accents.
- Control display/glass smoothness and use a low-bloom setting for legible traces and status LEDs.
- High-quality anti-aliasing is particularly important for the chip pins and solder joints; enable MSAA or TAA as appropriate.
- Bake large static bench geometry; use real-time lighting for player-movable component parts and the microscope focus stage.
- Create optional close-up LOD1 meshes for common components and LOD2 simplified board view only after target-platform profiling.
- Maintain separate physically accurate PCB dimensions (100×70×1.6 mm) while using 14× visual scale for player interaction.

## Still required for final AAA production

Hand-authored UV islands for specialized close-up parts, high-resolution branded silkscreen graphics, realistic subtle wear/decal work, baked maps from a high-poly surface pipeline, optimized colliders, full LOD authoring, final shading calibration in the shipping Unity render pipeline and Play Mode validation. The procedural texture maps are high-resolution references but are not hand-baked production textures.
