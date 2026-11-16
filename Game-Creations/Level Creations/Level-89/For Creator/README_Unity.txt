PROJECT SPARK — LEVEL 89: THE HELIOS STORM
ORIGINAL PROCEDURAL 3D ASSET KIT — EDITABLE FBX / GLB MESHES

DELIVERED ART AND SOURCE PLANNING
- 92 separately importable numbered binary FBX 7400 scenes under Models/ and 92 corresponding GLB scenes under GLB/
- Assembled reference FBX and GLB (DO NOT instantiate alongside the 92 individual numbered models)
- 76 uniquely named ANIM_ mesh-owning parent pivot groups, including shield cassettes, pump rotors and instrument status pivots
- 248 geometry-modeled gold-rimmed visual socket nodes, not active SparkTerminals
- 170 proposed wiring entries with 340 separate logical endpoints, 68 visually shared anchors needing reconciliation
- 12 procedural texture reference sets / 36 PNGs at 1024x1024, not baked artist-authored UV maps
- Four optional Unity art-import/material/validation/actual-state-only visual rig scripts
- Manifest, assembly map, objective design, W149 planned critical feedback, production plans and structural test report

UNITY IMPORT
1. Create Assets/ProjectSpark/Level89/Models/ and copy Models/00_*.fbx through 91_*.fbx, excluding the assembled showcase FBX.
2. Copy Textures/ into Assets/ProjectSpark/Level89/Textures/ if using the reference palette workflow.
3. Put SparkLevel89*Editor.cs in an `Editor` folder. Put SparkLevel89VisualRig.cs outside all Editor folders.
4. Select the layout GameObject, then run `Tools/Project Spark/Levels/89/Place HD Art Under Selected Layout`.
5. Run `Tools/Project Spark/Levels/89/Create & Apply HD Materials To Selected Art` on the generated art root.
6. Run `Tools/Project Spark/Levels/89/Validate Imported HD Art`. Confirm all 92 FBX models, 76 pivots, 248 decorative socket transforms.
7. Verify imported scale EXACTLY ONCE: FBX numeric vertices are authored in centimetres with UnitScaleFactor=1; GLB vertices are metres. Y up.
8. All modular equipment models are in common-origin global assembly coordinates. Set each placed instance localPosition=(0,0,0), localRotation=identity, localScale=(1,1,1), *after correct importer unit handling*.
9. Keep real SparkTerminals, circuit solvers, PLC logic and gameplay outside L89_HD_VISUAL_ART so replacing visuals cannot erase playability.
10. Use Gameplay_Wiring_Concept_W001_W170.csv, Terminal_Logical_Port_Reconciliation_340.csv, Objective_Certification plan and W149 doc for building real gameplay separately.

QUALITY AND HONEST LIMITATIONS
Delivered original hard-surface procedural technical-art preview with distinct physical mesh details and transform pivots.
NOT INCLUDED: final AAA UV unwrap/baked texture production, final art-directed lighting, colliders and authored LOD meshes,
real runtime wire circuits, a radiation transport solver, dose validation, safety PLC gameplay, LevelGamePlayManager 6/6,
Unity compilation, Unity FBX import/Play Mode tests. Structural GLB and binary header validation is separately reported.
These assets are original game fiction, not qualified flight hardware, radiation protection or equipment design.
