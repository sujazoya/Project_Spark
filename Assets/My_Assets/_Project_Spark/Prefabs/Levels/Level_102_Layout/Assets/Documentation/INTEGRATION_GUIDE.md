# PROJECT SPARK — LEVEL 102 SIGNAL PHANTOM

## Scope
Board-only procedural 3D mesh assets, not a complete Unity level. No environment, instrument FBXs, or UI meshes.

## Source of truth
- ModelManifest.csv contains 104 modular models; 82 are component placements.
- ComponentBOM.csv and ComponentPlacements.csv supply local-space placement references. Board physical size = **250 x 175 mm**. All 3D coordinates use metres.
- Assembled_Reference model is **showcase only**. Do not include it in the playable board.
- Start with 82 uninstalled parts, 108 incomplete copper routes, 28 vias missing, W109 detached.
- The 108 route/via tasks are planning placeholders, NOT a pin-level electrical schematic. Reconcile authentic nets, package pins and pad IDs before permitting gameplay certification.
- W109 carries a freshness-checked link-health validity signal, NOT physical differential data.

## Unity
Import FBXs. Invoke `Tools > Project Spark > Levels > 102 > 1. Build EMPTY HSX-10 Board`. Run material tool as a starting point, then assign lit materials based on imported material slots. The textures are reference PBR maps; actual mask packing and FBX mapping need validation. For accurate scale ensure Unity treats the FBX metre coordinate values as metres.

## Required gameplay logic (not supplied)
Real pad/net topology, 108 player copper routes, 28 vias, device dynamics, actual two differential channels, TDR, eye analyzer, BER qualification, impedance and parasitic model, separate safety supervisory loop, fault repair authority, virtual PCB repair UI integration, six objectives via `LevelGamePlayManager`.

## Quality limits
Procedurally detailed packages, leads, pads, connectors, reference copper, textures. Not manufacturer-accurate land patterns or artist-baked AAA material maps. FBX binary structure is generated and checked offline, but Unity import and shader compilation are not verified.
