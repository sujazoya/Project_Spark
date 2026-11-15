PROJECT SPARK — LEVEL 69: KEEP IT COLD
HD PROCEDURAL REFRIGERATION LAB / REAL 3D ART FILES

ASSETS
- 46 individual binary FBX 7400 meshes under Models/
- 46 GLB backups under GLB/
- Assembled reference FBX and GLB (do not instantiate together with individual assets)
- Model_Manifest.json (exact names, counts, declared pivots)
- Animation_Rigs.csv (14 spatial pivot references; NOT complete animation rigs)
- Unity_Optional/SparkLevel69ArtPlacementEditor.cs
- Previews/ hero assembled and individual model contact sheet
- Verification_Report.json

UNITY
1. Extract ZIP, import Models/ individual numbered FBXs into Assets/ProjectSpark/Level69/Models.
2. Put SparkLevel69ArtPlacementEditor.cs under Assets/Editor or other Unity Editor folder.
3. Select existing Level 69 layout root in hierarchy.
4. Run Tools/Project Spark/Levels/69/Place HD Art Under Selected Layout.
5. The helper creates L69_HD_VISUAL_ART and instantiates all 46 individual models with local identity transforms.
6. Use only the individual FBXs for modular gameplay, not the assembled showcase.

COORDINATES / SCALE
X = width; Y = up; Z = front. GLB authored in meters. FBX numeric vertices are centimeters with UnitScaleFactor 1.
Unity FBX normally auto-converts centimeters to meters; verify model Import Settings and one known base dimension (13.8 meters) first.
If the imported scene looks 100x larger, set Model Import Scale Factor = 0.01 and confirm visual size.

IMPORTANT LIMITATIONS
- This is a modeled geometry collection with material color regions, not a hand-authored PBR AAA pack.
- The 14 pivot positions are engineering references, not completed nested mechanical rigs or keyframe animations.
- Actual chamber doors, fans, compressor internals, frost overlays, refrigerant phase effects and safety interactions require Unity rigging.
- No SparkTerminal components, W01-W82 connection manifest, 128 socket anchors, refrigerant phase model, PLC code, safety logic or six gameplay objectives are included.
- Unity FBX import and Play Mode are not tested in this environment.
- All models are educational visual designs, not certified engineering equipment.
