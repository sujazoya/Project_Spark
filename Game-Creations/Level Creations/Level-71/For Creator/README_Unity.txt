PROJECT SPARK / LEVEL 71 — ETCH THE FUTURE
ACTUAL PROCEDURAL HD/AAA-INSPIRED SEMICONDUCTOR FABRICATION 3D ASSET COLLECTION

CONTENTS
- Models/: 50 individual binary FBX 7400 equipment models + 1 assembled art-only showcase FBX.
- GLB/: 50 matching GLB backups in meters + 1 assembled showcase GLB.
- Previews/: auxiliary 3D software-rendered geometry hero, closeup and catalog PNGs.
- Animation_Rigs.csv: 21 scene-space pivot REFERENCE positions (not pre-rigged mesh joints).
- Art_Assembly.csv: 50 model placements in common coordinates (local identity transforms).
- Model_Manifest.json: 50 model files and exact triangle counts, mesh parts and bounds.
- Component_Roles.csv: model index, name, file and complexity.
- Material_Palette.json: original material-color regions and suggested appearance properties.
- Gameplay_Wiring_Budget.csv: design budget of 90 future player wires; NOT actual wiring endpoints.
- Gameplay_Integration_Plan.md: visual art vs planned gameplay boundaries.
- Unity_Optional/SparkLevel71ArtPlacementEditor.cs: art-only scene placement helper (Undo, rerunnable).
- Unity_Optional/SparkLevel71MaterialUpgradeEditor.cs: art material creation/remapping.
- Unity_Optional/SparkLevel71VisualRig.cs: runtime VISUAL-ONLY adapter for already separated moving meshes.
- Source/build_level71.py: complete procedural 3D generation and binary FBX/GLB exporter source.
- Source/render_level71.py: software 3D scene preview renderer.
- Source/verify_level71.py: binary FBX + GLB structural and triangle-count verification.
- Verification_Report.json: actual export verification results and limitations.

UNITY QUICK START
1. Import only the 50 NUMBERED Models/*.fbx into Assets/ProjectSpark/Level71/Models/.
2. Place BOTH Editor C# files in a Unity Editor folder, e.g. Assets/Editor/ProjectSpark/.
3. Select your Level 71 layout root in the Unity Hierarchy.
4. Run Tools/Project Spark/Levels/71/Place HD Art Under Selected Layout.
5. The helper creates L71_HD_VISUAL_ART and places 50 FBX prefabs with local identity transforms.
6. Optionally select that art root and use Tools/Project Spark/Levels/71/Create & Apply HD Materials To Selected Art.
7. Do NOT also instantiate Models/Level71_Complete_ArtKit_Showcase.fbx: it duplicates the whole scene.
8. For moving equipment, separate the moving FBX mesh parts in a 3D editor or Unity as appropriate, place pivots according to Animation_Rigs.csv and assign them to SparkLevel71VisualRig.
9. The plasma light and optional plasma VFX are assigned manually. The script DOES NOT generate plasma physics or any visual effect automatically.

SCALE + COORDINATES
- X = workstation width, Y = up, Z = front. Construction dimensions in meters.
- Platform nominal width = 15.8 m.
- GLB vertex positions are numerically in meters.
- FBX vertices are stored numerically in centimeters, with UnitScaleFactor = 1.0.
- Check Unity FBX ModelImporter scaling: the imported workstation should span approximately 15.8 Unity units (at 1 unit/m).
- If it is 100 times too large, correct the FBX import scale once (e.g. factor .01). Do not double-apply conversion.
- All assets share the SAME world-origin frame; place with localPosition = (0,0,0), localRotation identity, localScale 1.

QUALITY / HONEST LIMITATIONS
- These are real original, scene-aligned, procedurally modeled HD/AAA-INSPIRED geometries, not hand-authored film/AAA-production-quality textured assets.
- No baked PBR maps, authored UV/normal atlases, LOD authoring, tested physics colliders, animation clips or skeleton bindings are included.
- Glass/plasma translucency requires shader configuration in Unity for the chosen render pipeline.
- Pivot CSV contains REFERENCE positions only; FBXs are not automatically joint-rigged.
- The Unity editor helper creates VISUAL ART ONLY, not Level_71.asset, Level_71_Layout.prefab, SparkTerminals or gameplay systems.
- The planned 90 player wires, 140 functional sockets and six objectives are NOT implemented in this art collection.
- No vacuum equations, RF matching, plasma chemistry, etch simulation, actual endpoints or safety PLC are included.
- The model names and appearance are fictional educational representations, not real-world manufacturing instructions or recipes.
- Unity native import, compilation and Play Mode have NOT been tested in this environment.
