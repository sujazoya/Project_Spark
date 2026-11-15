PROJECT SPARK — LEVEL 80: COMMAND THE LIGHT
HD / AAA-inspired original silicon photonics art package (models and integration plans, NOT complete gameplay)

ART:
  68 independent binary FBX 7.4 modular models in Models/
  68 matching GLB alternatives in GLB/
  1 assembled reference FBX + 1 assembled reference GLB
  50 real named ANIM_* transform parents (visible geometry parented to pivots)
  nominal width 23.2 m, Y-up, X width, Z front
  FBX numeric vertex coordinates are centimetres (UnitScaleFactor=1), GLB geometry in metres

UNITY IMPORT:
  1. Extract this ZIP into a convenient folder outside the Unity project.
  2. Copy Models/ with all 68 numbered FBXs to Assets/ProjectSpark/Level80/Models.
  3. Set importer scale so the assembled geometry would measure approximately 23.2 Unity metres wide; do NOT convert cm to m twice.
  4. Optional: copy Textures/*.png to Assets/ProjectSpark/Level80/Textures for 9 original 1024x1024 procedural PBR map sets.
     The preview material tool assigns base/normal maps when available; the R/G/B metallic-roughness-AO maps require manual shader-specific packing.
  5. Copy three *Editor.cs files to Assets/ProjectSpark/Level80/Editor/.
  6. Copy SparkLevel80VisualRig.cs to Assets/ProjectSpark/Level80/Scripts/ (outside Editor).
  7. Select the Level 80 layout parent in the Hierarchy. Run:
     Tools/Project Spark/Levels/80/Place HD Art Under Selected Layout
     This creates L80_HD_VISUAL_ART with 68 common-origin instantiated modular FBXs.
  8. Select L80_HD_VISUAL_ART and run:
     Tools/Project Spark/Levels/80/Create & Apply HD Materials To Selected Art
  9. Validate:
     Tools/Project Spark/Levels/80/Validate Imported HD Art
  10. Attach SparkLevel80VisualRig to the parent containing all ANIM_ groups. Bind actual authored Lights / ParticleSystems if available.
 11. Connect the rig's public SetActual...() methods to real optical/electrical/control gameplay state.

CAUTION:
 - The art-placement helper REPLACES the L80_HD_VISUAL_ART child on rerun after confirmation. Do not put gameplay objects beneath that disposable root.
 - Never instantiate both the numbered models and assembled showcase simultaneously (double geometry).
 - Preview materials are procedural vertex-color based; not final artist-authored PBR textures and not ship-ready LODs.
 - Cyan/green/amber/violet waveguides are educational false-color illustrations of infrared signals.
 - W001-W126 and P001-P194 are CONCEPTUAL data only, not pre-installed SparkTerminal objects or connected wires.
 - Laser/shutter controls, optical transfer, microring thermal tuning, W078 feedback loss, industrial safety and objective 6/6 require runtime gameplay implementation.
 - This deliverable has NOT been Unity-compiled or Play Mode tested.

KEY FILES:
  Model_Manifest.json
  Animation_Rigs.csv
  Art_Assembly.csv
  Component_Roles.csv
  Gameplay_Wiring_Budget.csv
  Gameplay_Wiring_Concept_W001_W126.csv
  Terminal_Anchor_Plan_194.csv
  Objective_Certification_Plan_6.csv
  W078_Critical_Feedback_Recovery.md
  Gameplay_Integration_Plan.md
  AAA_Art_Delivery_Notes.md
  Unity_Optional/SparkLevel80ArtPlacementEditor.cs
  Unity_Optional/SparkLevel80MaterialUpgradeEditor.cs
  Unity_Optional/SparkLevel80AssetValidatorEditor.cs
  Unity_Optional/SparkLevel80VisualRig.cs
  Source/build_level80.py — original reproducible geometry generator
  Source/make_level80_planning.py — reproducible plans
  Verification_Report.json — structural checks
  SHA256SUMS.txt — checksums

No image generation is included in this asset pack.
