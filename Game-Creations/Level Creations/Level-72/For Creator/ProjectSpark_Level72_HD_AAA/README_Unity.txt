PROJECT SPARK — LEVEL 72: LAYER BY LAYER
==========================================

ART PACKAGE: 52 modular binary FBX + 52 GLB matching backup models; assembled FBX and GLB reference.

1. Extract this ZIP and import Models/*.fbx (the 52 numbered FBXs) into your Unity Assets folder.
2. Put *ArtPlacementEditor.cs and *MaterialUpgradeEditor.cs into Assets/.../Editor/ (or another Editor-only asmdef).
3. Select the existing Level 72 layout root GameObject in the Unity hierarchy.
4. Run Tools > Project Spark > Levels > 72 > Place HD Art Under Selected Layout.
5. The helper creates L72_HD_VISUAL_ART and instantiates the 52 numbered FBXs at (0,0,0), preserving the shared origin.
6. Do not also instantiate Level72_Complete_ArtKit_Showcase.fbx; that model is an assembled reference.
7. Run Tools > Project Spark > Levels > 72 > Create & Apply HD Materials To Selected Art if desired. Check shader compatibility with your built-in/URP/HDRP project.
8. VisualRig.cs is optional, visual-only, and expects explicitly bound child moving parts. The pivot CSV lists world-space REFERENCES, not baked skeletons.
9. Mesh authoring: X width, Y up, Z front. Construction in metres; GLB numeric metres, binary FBX numeric centimetres with UnitScaleFactor=1. Check FBX importer: 16.1 m workstation width expected in Unity.

NOTE: Geometry is procedural HD / AAA-inspired but not handmade AAA PBR textured. Material regions are imported; artistic PBR maps and LODs are not included. Unity compilation / Play Mode import was NOT tested.

IMPORTANT: This is ART ONLY. No 94 functional player wires, 146 SparkTerminal objects, ALD physics, four-stage sequencer, valid purge feedback or LevelGamePlayManager 6/6 objectives exist in this pack. Gameplay_Wiring_Budget.csv only allocates proposed groups, it is not an actual wire endpoint manifest.

DEVELOPMENT: For code integration, inspect the actual Unity Project Spark source and APIs (SparkTerminal, SparkElectricalSolver, SparkLevelEvaluator, LevelGamePlayManager) before creating gameplay controllers. Keep 5V educational controls separate from simulated heater and pump energy. The precursors A/B are fictional nonhazardous virtual chemicals.

Files: Model_Manifest.json, Animation_Rigs.csv, Art_Assembly.csv, Component_Roles.csv, Material_Palette.json, Gameplay_Wiring_Budget.csv, Gameplay_Integration_Plan.md, Verification_Report.json, Source/.
