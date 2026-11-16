PROJECT SPARK — LEVEL 90 — THE LONG BURN
ORIGINAL EDITABLE MODULAR 3D ART ASSET DELIVERY

WHAT YOU HAVE
- 94 individual binary FBX 7400 files in Models/, 94 matching glTF GLB 2.0 in GLB/.
- One assembled showcase FBX and GLB for reference ONLY; do not instantiate alongside all 94 modular files.
- 80 named pivot-parent groups ANIM_*, 254 decorative gold visual socket markers.
- 12 procedural preview material sets / 36 PNG reference maps.
- Authored 176 conceptual wire definitions (W001..W176), 352 proposed independent logical ports, six objective design records.
- Optional Unity C# art placer, material palette upgrade, structure validator, and read-only visual rig.

IMPORT
1. Extract the ZIP, copy Models/ to Assets/ProjectSpark/Level90/Models/.
2. Copy optional Unity_Optional/*Editor.cs to a Unity Editor folder (e.g. Assets/ProjectSpark/Level90/Editor/).
3. Put SparkLevel90VisualRig.cs in a runtime scripts directory outside Editor/.
4. Select an existing/new Level 90 layout GameObject.
5. Tools/Project Spark/Levels/90/Place HD Art Under Selected Layout.
6. Tools/Project Spark/Levels/90/Create & Apply HD Materials To Selected Art.
7. Tools/Project Spark/Levels/90/Validate Imported HD Art.

SCALING IMPORTANT
FBX numeric vertices and animation pivot positions are in CENTIMETRES with FBX UnitScaleFactor=1;
GLB numeric geometry and node positions are in METRES. Ensure Unity's FBX importer converts cm -> metres EXACTLY ONCE.
All modular models were authored with their equipment geometry already laid out at common assembly origin (0,0,0).
Instantiate each numbered FBX once under L90_HD_VISUAL_ART at identity local transforms.
Do not also instantiate Models/Level90_Complete_ArtKit_Showcase.fbx.

NOT INCLUDED / NOT CLAIMED
- No actual SparkTerminal components or player-created physical wires; 254 SOCKET_P... are decorative.
- No circuit solver or PPU/plasma/xenon/trajectory safety process model.
- No authoritative 6/6 gameplay, fault recovery or scene builder.
- Unity compilation, import and Play Mode have NOT been tested.
- Procedural reference textures need UV unwrap, baking, packing, LOD and production shading.
- Generated emitter/plasma shapes are only visual geometry, not VFX or physical ion propulsion.

CRITICAL W157
L90_NeutralizerCurrentTransducer P214 -> L90_PropulsionSafetyPLC P216.
See W157_Critical_Neutralizer_Current_Recovery.md. Never confuse the decorative anchor with a functional SparkTerminal.

This package supplies the art and authoring plan for a future Unity implementation only.
