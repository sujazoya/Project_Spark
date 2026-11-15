# Project Spark — Level 76: FORGE THE CONNECTIONS

## Scope and status

This is original modular 3D visual art **only**. Geometry is not a Unity gameplay implementation. The existing files provide 60 original binary FBX modular meshes, 60 GLB backups, assembled workstation FBX/GLB, named pivots, equipment naming, educational wiring/terminal plans, and optional Unity art helpers. No Unity Play Mode test was performed.

## Existing asset hierarchy

- Art root `L76_HD_VISUAL_ART`.
- 60 numbered FBXs share one origin. Keep each at position `(0,0,0)` rotation zero scale one after importer conversion.
- Approximate workstation width 19.6 metres. FBX coordinates are numeric centimeters with `UnitScaleFactor=1`; GLB coordinates are numeric metres.
- Do not place the assembled reference FBX alongside the 60 modular files.
- Named pivots from `Animation_Rigs.csv` are actual FBX/GLB parent nodes, no baked animations.

## Critical Unity systems still required

1. Inspect existing Project Spark Unity circuit, wiring, shelf, level and objective APIs.
2. Create exactly **170 functional `SparkTerminal` objects** and **110 player wires** using the planning CSVs. Respect real junction/shared-terminal rules. No playable terminals or wires are inside the art files.
3. Model a fictional nonhazardous electrolyte, reservoir inventory, flow, filtration, temperature, prewetting and bubbles.
4. Implement cell rectifier with distinct actual current, cell voltage and measured cathode potential; include finite response and fail-safe shutdown.
5. Implement Faraday-law inspired copper deposition and true regional trench/via fill geometry: seed continuity, bottom and sidewall growth, voids, seams, overburden and electrical measurement.
6. Implement current-density distribution with finite multi-zone anode controller; fictional accelerator/suppressor/leveler balance.
7. Implement wafer cleaning, spin drying, thickness/void/topography, and pre-CMP electrical test-structure metrology.
8. Implement equipment disturbances: wafer contact degradation; manifold flow restriction; additive dosing drift.
9. Implement the critical **W076** real reference-electrode feedback wire. On disconnection, invalidate potential sensing, inhibit plating, safely disable the rectifier through simulated actuator dynamics, latch protection and require real wire reconnection, recalibration, explicit reset, and a fresh certified wafer.
10. Integrate the existing Project Spark objective system. Objectives 1–5 stay historically certified during the final intentional disconnection; objective 6 only completes after full repair and successful new wafer; `LevelGamePlayManager` must reach 6/6.

## Planned objectives

1. Build electrochemical network — 110 player connections / 170 functional terminals.
2. Prepare electrolyte and wafer — circulation, temperature, prewetting, seed continuity, clamp and bubble checks.
3. Master current and electrochemical control — calibrate current / potential / anode current distribution.
4. Achieve void-free copper filling — regional deposition, fill, overburden, cleaning and metrology.
5. Survive three disturbances — contact resistance, flow manifold, virtual additives.
6. Restore critical reference-electrode feedback — W076 real wire loss, safe disable, reconnection, recalibration, fresh wafer certification.

## Important manufacturing continuity

The prepared incoming wafer is assumed to have a patterned dielectric, barrier and seed layer after intervening process steps. This level does not claim to plate bare implanted silicon directly. The electroplated overburden remains ready for a later copper CMP level.

## Safety and verification limitations

All plating electrolyte and additives are fictional and nonhazardous, with normalized educational settings; this art pack is not an operational chemical recipe. Source art does not implement circuit physics, plating chemistry, safety PLC, objectives, LODs, baked clips, final PBR textures, or tested Unity gameplay.
