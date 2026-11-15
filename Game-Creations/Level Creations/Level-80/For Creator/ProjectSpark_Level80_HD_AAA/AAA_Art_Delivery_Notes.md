# Level 80 HD / AAA-inspired art delivery — limitations

- Original generated procedural 3D geometry, **not** a raster image.
- 68 unique modular binary FBX 7.4 exports and 68 matching GLBs.
- One assembled FBX+GLB as an alternate art preview; do **not** place it alongside the individual models.
- The FBX writer exports centimetre numeric vertices with UnitScaleFactor=1; matching GLB vertices are in metres. Set Unity importer scale once and check assembled width ~23.2 m.
- 50 actual named pivot hierarchy groups are exported. Their animation data is **not baked clips**; the included C# visual rig drives transforms from actual data when connected by gameplay code.
- Preview vertex colors / basic shading, not baked production PBR texture sets. A Unity material upgrade helper is supplied.
- Photonic infrared wavelengths are depicted in false-color cyan/green/amber/violet **for education**. They are not literally visible waveguides.
- High geometry and part count can benefit from optimization/LOD and instance batching before shipping.
- No authored interactions, optical physics, functional electrical terminals, safety interlocks, W078 monitoring or objective 6/6 are included. See planning files.
