# Level 117 — The Packet Citadel (NFX-25)

**Source delivery:** 272 modular OBJ/GLB pairs + separate 273rd assembled reference, 220 installable component source models, 52 noninstallable/diagnostic visual source models, 32 original procedural PBR material families, 26 layer reference art models, 304 UNASSIGNED routing tasks, 108 UNASSIGNED via tasks, W305 schematic-validation-required.

This package imports **models and editor authoring tools only**. Do not claim that actual optical networking or LevelGamePlayManager objectives were verified. Preview is explicitly visual-only. Preserve previous Levels 1–116.

## Installation

1. Back up Unity project and close Unity.
2. Extract the **complete** package outside Unity `Assets`.
3. From the folder containing `Install_Level117_AAA_AutoLayout.ps1` and `ProjectSpark_Level117_NFX25/`, run:

```powershell
powershell -ExecutionPolicy Bypass -File ".\Install_Level117_AAA_AutoLayout.ps1" -UnityProjectPath "D:\Project_Spark_Master_Project\Project_Spark"
```

4. Open Unity 6.3 URP and wait for OBJ import. Project path inside Assets: `Assets/ProjectSpark/Level117/NFX25`.
5. `Tools > Project Spark > Automatic Level Generator > 117 > 3. Validate Sources and Report Blockers`.
6. `117 > 4. Rebuild URP PBR Materials`.
7. `117 > 1. Build FULL Level_117_Layout` (isolated scene and visual prefabs).
8. Select EXISTING `Table_top`; `117 > 2. Place assembled VISUAL preview on SELECTED tabletop` (no electrical effect).

If using only the **generator upgrade**, first install the complete source kit. The installer refuses to overwrite changed source geometry, data or textures; changed editor scripts are backed up outside `Assets` while preserving `.meta` GUIDs.

## Known electrical blockers

All 304 route nets/pads unassigned; all 108 via nets/spans TBD; 220 footprints unverified; U35/U36 and W305 physical pin map unverified; switch/PHY/optical parameters not yet approved. The demonstration class `SparkL117PacketTeachingDemo` is illustrative only and cannot certify objectives. 100 acceptance tests are recorded as NOT_RUN.

## Visual scale

Physical board: 0.460 x 0.320 metres at 1:1. A separate visual-only enlarged preview fits ~10.13 x 7.05 Unity units on the user's existing tabletop, leaving electrical metric measurements at original scale.

## QA

Run `Documentation/validate_level117_source.py` with Python 3 to verify source consistency and OBJ bounds before Unity import. Unity compilation, models rendered in URP, project-specific API compatibility and Play Mode need actual editor testing; no such proof is included.
