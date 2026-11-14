# Level 71 — Art package vs planned gameplay

This pack contains **50 actual 3D equipment FBX meshes** and matching GLBs, not a playable Unity level.

## Approved conceptual target
- 90 player-created wires — **NOT INCLUDED**
- 140 unique functional `SparkTerminal` anchors — **NOT INCLUDED**
- Six authoritative objectives — **NOT IMPLEMENTED**
- Process pressure, virtual gas flow, RF matching, plasma, wafer film, OES and safety — **NOT IMPLEMENTED**

The file `Gameplay_Wiring_Budget.csv` allocates 90 future player wires across eight design groups, **not** exact W01-W90 endpoint definitions. Do not treat it as a functional manifest.

## Artwork only
The FBX/GLB surfaces and material labels are procedural illustration geometry. Pivots in `Animation_Rigs.csv` are reference points for artist/Unity rigging; no animated skeleton or clips are included. Check FBX scale and model hierarchy before attaching gameplay components.

## Future Unity integrations
Inspect actual project APIs for SparkTerminal, SparkElectricalSolver, SparkLevelEvaluator and LevelGamePlayManager before writing runtime code. The illustrative vapor-vacuum / RF scenario is a *virtual educational simulator* and is not a recipe or hardware control design for real reactive plasma equipment.
