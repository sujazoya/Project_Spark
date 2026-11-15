# Project Spark — Level 72 Gameplay Integration Plan

## What is in this package
- 52 **actual independent modular binary FBX** models and 52 matching GLB geometry assets.
- One assembled FBX and GLB reference. Do not instantiate them together with the 52 individual art assets.
- Unity art placement, material preview/upgrading, and a visual-only rig adapter.
- World-space pivot references, material palette, assembled placements, geometry export sources, asset verification.

## NOT INCLUDED: gameplay systems
- **94 player wires**: design budget only; no W01-W94 terminal endpoints created.
- **146 actual `SparkTerminal` sockets**: not included; must be created and validated against existing Project Spark source.
- Physics: pumpdown, inert purge, self-limiting surface reactions, nanofilm, sensor signal, temperature and pressure PID: not implemented.
- Authoritative 6/6 objectives: not implemented.

## Proposed process storyline
**Level 72 — LAYER BY LAYER**: an educational fictitious ALD machine, A dose / purge / B dose / purge. Process targets: 12 nm film, ~0.10 nm nominal per valid cycle, within 0.3 nm; <=3% proposed nonuniformity, >=95% bottom/top trench conformality. All are fictional balance targets, not real chemical instructions.

A cleanroom wafer is loaded; the player commissions vacuum, wafer temperature, and fictional precursor flow, executes a safe four-stage ALD cycle, deposits a conformal coating, recovers from purge restriction/heater-zone drift/stuck A valve, then physically disconnects a purge-flow feedback wire, repairs, recalibrates, resets, requalifies and reaches authoritative `LevelGamePlayManager` 6/6.

## Build and scale
FBX numeric geometry is centimetres with UnitScaleFactor=1; GLBs use metres. Verify Unity import dimensions (~16.1 m platform width), avoid double scaling. Use 52 numbered FBXs at a **common origin**, no manually offsetting each model.

The animation pivot CSV contains location **references**, not joints, clips or guaranteed separately rigged moving meshes. Rig components in Unity before animating parts. Artwork materials have color regions but need production shader/PBR work. Unity import and runtime not tested.
