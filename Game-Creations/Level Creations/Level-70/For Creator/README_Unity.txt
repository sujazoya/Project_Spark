PROJECT SPARK / LEVEL 70 - GUARD THE AIR
HD PROCEDURAL CLEANROOM 3D GEOMETRY — ACTUAL 3D FILES, NOT SPRITES

WHAT IS INSIDE
- Models/: 48 independent binary FBX 7400 art assets + 1 assembled reference FBX.
- GLB/: 48 GLB backups in meters + 1 assembled GLB reference.
- Animation_Rigs.csv: 19 spatial pivot references (NOT completed submesh joint rigs).
- Art_Assembly.csv: one row per equipment model, identity local transforms.
- Model_Manifest.json: model names, triangle/part counts, bounds, declared pivots.
- Material_Palette.json: suggested colors / metallic / roughness.
- Previews/: real software-rasterized geometry previews (only auxiliary preview files).
- Unity_Optional/SparkLevel70ArtPlacementEditor.cs: non-destructive visual assembly helper.
- Unity_Optional/SparkLevel70MaterialUpgradeEditor.cs: optional PBR-like material creation/remap.
- Unity_Optional/SparkLevel70VisualRig.cs: optional VISUAL-ONLY animation adapter (requires separated moving meshes).
- Source/build_level70.py: procedural generation/re-export source.
- Source/verify_level70.py: detailed binary FBX and GLB validation script.
- Verification_Report.json: export integrity report.

UNITY INSTALLATION
1. Extract the ZIP. Import ONLY 48 numbered Models/*.fbx into Assets/ProjectSpark/Level70/Models.
2. Put SparkLevel70ArtPlacementEditor.cs and SparkLevel70MaterialUpgradeEditor.cs into a Unity Editor folder, e.g. Assets/Editor/ProjectSpark/.
3. Select the Level 70 layout root in the Unity Hierarchy.
4. Use Tools/Project Spark/Levels/70/Place HD Art Under Selected Layout.
5. The helper creates L70_HD_VISUAL_ART under the selected transform and instantiates all 48 models with local identity transforms.
6. Optionally select that root and choose Tools/Project Spark/Levels/70/Create & Apply HD Materials To Selected Art.
7. Do NOT also instantiate Models/Level70_Complete_ArtKit_Showcase.fbx. It is only a composition reference and causes duplicate overlapping geometry.
8. For animated machinery, assign submeshes and pivot transforms to the visual rig, and call it from real gameplay/simulation.

COORDINATES & SCALE
X=workstation width, Y=up, Z=front. Authored layout spans ~15.6 m width.
GLB numeric units are meters. FBX vertices use numeric centimeters and FBX UnitScaleFactor 1.
Verify Unity FBX importer scaling: the 15.6-m wide workbench must import as approximately 15.6 Unity units.
If 100x too large, adjust the model import scale (e.g. 0.01) once. Do not double-apply conversion.

MODEL QUALITY / LIMITATIONS
- These are ORIGINAL procedurally modeled, textured-by-material-color, HD/AAA-INSPIRED geometry assets, not artisan-authored film-quality AAA assets.
- No baked normal maps, hand-painted PBR atlas, LOD pipeline, final collision meshes, UV texture atlas, or skeletal animation clips are included.
- Glass materials may require pipeline-specific alpha/surface settings in Unity.
- Animation_Rigs.csv provides art pivot reference positions only; FBX meshes are not automatically joint-rigged.
- Assets contain NO SparkTerminal components, 86 wire paths, 134 gameplay sockets, HEPA/particle physics, PID controller, PLC gameplay, or six objectives.
- The separate helper only places art and optionally changes render materials; it does not create the playable Level_70.asset or Layout.prefab.
- The optional runtime visual rig has no authority over HVAC control, pressure, particle concentration or interlocks.
- Unity native FBX importer and Play Mode have NOT been tested in this container.
- All equipment is conceptual educational visual art, not certified industrial engineering equipment.
