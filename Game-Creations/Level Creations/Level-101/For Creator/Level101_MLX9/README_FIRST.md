# Project Spark Level 101 — MLX-9 The Hidden Layers

BOARD-ONLY PROCEDURAL 3D ASSET KIT. No room, table, physical instruments or UI model assets.

- 96 individually exported FBX + matching GLB models: 76 installable components, 20 PCB/layer/repair modules.
- 230 x 160 mm fictional PCB with six copper layers. All mesh positions expressed in metres, Y = thickness axis.
- One assembled reference FBX + GLB in Showcase, NOT for playable puzzle state.
- Shared 12 material families, 60 procedural PBR maps and a 2048-pixel silkscreen reference. These are reference-grade textures, not artist-authored AAA bakes.
- Gameplay starts with all 76 components missing, 100 routes missing, 24 vias missing, and W101 disconnected.
- CSV copper/via data are UNVERIFIED DESIGN PLACEHOLDERS, NOT A FUNCTIONAL PIN-LEVEL SCHEMATIC. Do not mark any route complete based on decorative traces.
- FBX uses binary 7400 and centimetre coordinates (physical units originally metres). Unity FBX import scaling should be checked against known 0.230 x 0.160 m board dimensions.
- Unity helper is art-only. It does not add electrical gameplay, solder functionality, or objective certification.

**Suggested install:** Place all folders under Assets/ProjectSpark/Level101 and run Tools > Project Spark > Levels > 101 > Build Empty MLX-9 Board.

Realism: modeled IC metal leads, component packages, PCB metal details, six-layer visualization. Manufacturer-accurate mechanical footprints and fully qualified Unity materials still require a production pass.
