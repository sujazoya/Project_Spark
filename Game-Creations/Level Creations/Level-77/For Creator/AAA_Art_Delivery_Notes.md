# Level 77 art-production notes

## Actual deliverables

- 62 unique numbered procedural 3D workstation parts, each exported as binary FBX 7400 and corresponding loadable GLB.
- One complete assembled binary FBX and matching GLB reference (already includes all 62 modules).
- 44 actual named pivot parent objects inside the individual FBX and GLB hierarchies (not just a CSV naming plan).
- Simple preview material regions with colors including copper, steel, ceramic, pad, cyan, amber, violet, glass, slurry and display illumination. A Unity editor helper assigns basic preview materials.
- Common world origin so 62 imported models align at position (0,0,0) after correct scale conversion.
- Source generators, manifests, SHA-256 checksums, structural verification reports, 114-wire conceptual plan and 176 planned anchor locations.

## Distinct Stage A/B/C art

- A (x -5.35 m): amber copper bulk-removal platen, grooved pad, carrier, lift, downforce instrument, wear conditioner, fluid equipment and eddy-current endpoint.
- B (x 0 m): cyan soft-landing platen, fine pad, carrier, force regulator and finishing-fluid nozzle.
- C (x +5.35 m): violet barrier-clear platen, pad, carrier, optical endpoint spectrometer and transparent window, tomography and selective fluid modules.
- Above: transfer gantry, inspection holograms and rear SCADA display.
- Around front: wafer infeed, optical/tomography metrology, cleaning, rinse, dry, interline leakage and Kelvin-probe cabinets.

## Important engineering/art limitations

- True 3D geometry exports were structurally validated outside Unity; Maya/Blender/Unity importer behavior and render-pipeline transparency were **not** tested.
- This is original procedural HD/AAA-inspired geometry; it does not contain hand-authored high-quality normal/roughness maps, UV atlases, baked animation clips, texture LODs, physically authored slurry simulations or ready-to-play electrical gameplay.
- Multiple material colors/emissions are placeholder visual materials rather than complete artist-authored PBR.
- Elaborate polish slurry spray and wafer layer transitions require scene-specific Unity VFX/Shader Graph integration; the visual rig accepts actual gameplay state but is not the simulation itself.
- Do not use the assembled showcase alongside all 62 modular parts; that doubles the workstation visually.
