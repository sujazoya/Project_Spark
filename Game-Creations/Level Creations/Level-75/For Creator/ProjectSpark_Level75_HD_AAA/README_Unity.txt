PROJECT SPARK / LEVEL 75 — WRITE THE SILICON
ACTUAL MODULAR 3D FBX/GLB ASSET PACK — VISUAL ART ONLY

CONTENTS
- Models/: 58 individual REAL binary Kaydara FBX 7.4 geometry models + 1 assembled FBX.
- GLB/: 58 individual GLB backup models + 1 assembled GLB.
- Model_Manifest.json: actual mesh counts, bounds, pivot names and declared scope.
- Animation_Rigs.csv: named FBX/GLB parent Null group positions and motion type.
- Art_Assembly.csv / Component_Roles.csv: common-origin model list and geometric roles.
- Material_Palette.json: suggested material color/metallic/transparency settings.
- Gameplay_Wiring_Budget.csv: 106 PLANNED player-created circuit wires.
- Gameplay_Wiring_Concept_W01_W106.csv: intended equipment pairings (NOT real SparkTerminal connectivity).
- Terminal_Anchor_Plan_164.csv: 164 PLANNED terminal positions (NOT FBX components or Unity SparkTerminals).
- Gameplay_Integration_Plan.md: six objectives, actual simulation requirements and final fault.
- Unity_Optional/SparkLevel75ArtPlacementEditor.cs: place modular FBX art under selected root.
- Unity_Optional/SparkLevel75MaterialUpgradeEditor.cs: configure basic pipeline-compatible material previews.
- Unity_Optional/SparkLevel75VisualRig.cs: visual-only motion / optional beam VFX adapter.
- Unity_Optional/SparkLevel75AssetValidatorEditor.cs: check that 58 FBX slots exist and confirm placed scale/pivots.
- Source/build_level75.py: original procedural geometry and FBX/GLB exporter, self-contained.
- Source/make_level75_planning.py: concept wiring and terminal-anchor guide generator.
- Verification_Report.json: real structural checks after validation.

GEOMETRY IMPORT
- All models share a single common world origin, centered on 19.2-m-wide X-axis base.
- X=left/right, Y=up, Z=front; original authored geometry uses metres.
- FBX stored in numeric CENTIMETERS, UnitScaleFactor=1; GLB stored in numeric METERS.
- Check Unity ModelImporter scale (common source of 100x mismatches).
- The imported base should be approximately 19.2 UNITY UNITS WIDE at 1 Unity unit=1 meter.
- Import only 58 numbered FBXs into the playable scene; the assembled FBX is for reference,
  not an extra module. Avoid doubling geometry by importing assembled + individual together.

UNITY STEPS
1. Import all numbered Models/*.fbx into Assets.
2. Put all three Editor scripts in an Assets/**/Editor directory.
3. Put SparkLevel75VisualRig.cs in a runtime scripts directory.
4. Select your Level 75 scene/layout root.
5. Click Tools/Project Spark/Levels/75/Place HD Art Under Selected Layout.
6. It builds L75_HD_VISUAL_ART, with all 58 model prefabs at common local origin.
7. Select the art, click Tools/Project Spark/Levels/75/Create & Apply HD Materials To Selected Art.
8. Verify visual rig pivot names and import scale; connect actual simulation state to its methods.

NOT A WORKING LEVEL YET
- No real Unity project or LevelGamePlayManager was available to compile or run here.
- The pack DOES NOT implement 106 player wires, 164 SparkTerminal instances, ion implantation
  physics, vacuum dynamics, dose control, safety PLC, six objectives, or automated scene builder.
- Art placement menu is NOT a full playable level creator.
- Gameplay_Wiring_Concept and Terminal_Anchor_Plan are explicit DESIGN-ONLY documents.
- ALL beam current, thermal, mask, dose and diagnostic visual values must be supplied by
  your actual Project Spark simulation. Do not treat optional visual rig as physics.
- No hand-authored photorealistic PBR maps, baked motion clips, advanced lighting, colliders,
  Unity LODs or working particle systems are included. Materials are palette starting points.

EDUCATIONAL SAFETY
- Fictitious source species, normalized fictional dose and high-power abstraction.
- Do not interpret the artwork or gameplay plan as operational ion implantation instructions.
- Control circuitry is 5 V; beam, turbo, thermal and actuator domains need separate virtual power.

DOWNLOAD/VERIFY
The ZIP contains all source and geometry. Verification_Report.json and SHA256SUMS.txt
summarize the actual exported files; Unity visual/play-mode testing must be performed separately.
