PROJECT SPARK — LEVEL 82: SILENCE THE SPECTRUM
HD / AAA-INSPIRED MODULAR FBX + GLB 3D ASSET COLLECTION
==========================================================

ACTUAL BINARY 3D MESHES — NOT ANOTHER IMAGE.
This is an EMC qualification *art and planning pack*, NOT a fully playable Unity level.
No live SparkTerminal components, player electrical wires, real EMC field solver, RF
safety PLC gameplay, fault recovery gameplay, or LevelGamePlayManager objectives are included.

INCLUDED
- Models/                  72 modular FBX 7400 binary files, plus assembled FBX.
- GLB/                     72 corresponding modular GLBs, plus assembled GLB.
- Textures/                12 original procedural PBR-style material sets, 36 PNG maps.
- Unity_Optional/          3 optional art Editor scripts and 1 visual runtime rig.
- Model_Manifest.json      Exact filenames, bounds, geometry counts and pivot groups.
- Animation_Rigs.csv       54 actual named mesh-owning FBX/GLB pivot parent transforms.
- Art_Assembly.csv         All modular pieces at common local origin.
- Visual_Terminal_Sockets_206.csv and Terminal_Anchor_Plan_206.csv
                            Coordinates for 206 modeled gold-rimmed socket meshes.
- Gameplay_Wiring_Concept_W001_W134.csv
                            134 proposed electrical signal connections and visual socket IDs.
- Objective_Certification_Plan_6.csv
                            Six designed objectives, not active in Unity.
- W119_Critical_Feedback_Recovery.md
                            Trust boundary and safe recovery of RF field monitor.
- Gameplay_Integration_Plan.md
                            Recommended simulation and Unity integration architecture.
- AAA_Art_Delivery_Notes.md
                            Geometry, visual and production notes.
- Source/                  Reproducible FBX/GLB mesh builder, PBR map generator,
                            planning generator, structure verifier and packager.
- Verification_Report.json
                            Independent FBX and GLB structure checks.
- Checksums_SHA256.txt     SHA-256 of files at packaging time.

UNITY SETUP
1. Unzip the Level82 complete ZIP.
2. Import the 72 *numbered* FBX models from Models/ into your existing Project Spark project.
3. Place SparkLevel82*Editor.cs files inside a Unity Editor/ directory.
4. Place SparkLevel82VisualRig.cs outside any Editor folder for runtime compilation.
5. Select an empty Level 82 layout GameObject in the Hierarchy.
6. Menu: Tools/Project Spark/Levels/82/Place HD Art Under Selected Layout.
   This creates L82_HD_VISUAL_ART and places all 72 modular FBX models at local
   position zero, local rotation identity and scale one after importer conversion.
7. Copy Textures/ into Assets/ProjectSpark/Level82/Textures, then run:
   Tools/Project Spark/Levels/82/Create & Apply HD Materials To Selected Art.
8. Run: Tools/Project Spark/Levels/82/Validate Imported HD Art.
9. Confirm assembled visible width approximately 25.4 Unity units (metres).
10. Attach SparkLevel82VisualRig to a parent containing all 72 model/pivot hierarchies,
    and drive SetActual... methods only with actual EMC system state.

IMPORTANT UNIT CONVENTIONS
- Y is up; X is horizontal; Z is depth.
- Binary FBX numeric vertex coordinates represent centimetres, UnitScaleFactor=1.
- GLB geometry uses metres.
- Apply the Unity FBX import scale conversion **exactly once**.
- All numbered modular FBXs share the same origin; do not manually arrange each.
- Do not instantiate Level82_Complete_ArtKit_Showcase alongside modular models.
  The assembled model is an alternate preview and will duplicate equipment.

ART DETAILS
- Actual absorber pyramid wall/ceiling tessellation, RF test chamber support frame,
  rotating EUT stage, four-chiplet avionics computer, antenna mast, near-field gantry,
  cable/current probes, metal shield modules, ferrites and filter bench, RF source,
  field probe, safety PLC, spectrum holograms and emergency shutdown station.
- All 54 ANIM_* parents are real transform hierarchy groups and contain meshes.
- 206 art socket meshes are visible with corresponding CSV positions; they are not
  functioning SparkTerminal components until implemented through existing game code.
- PBR maps are *procedural previews*, not baked UV-specific high-end art textures.
  Packed map channels: R=metallic, G=roughness, B=ambient occlusion. Convert channels
  appropriately for URP/HDRP. UV unwrapping/baking and LOD work still recommended.
- Optical/RF field glow colors are fictional visualization only.

CRITICAL W119 GAMEPLAY DESIGN — NOT YET FUNCTIONING
- Real future W119 signal: L82_TriaxialRFFieldStrengthProbe (RF_FIELD_MEASURED_OUT)
  -> L82_RFImmunitySafetyPLC (RF_SAFE_FIELD_FEEDBACK_IN).
- Actual art anchors: P119 -> P126, as allocated in the wiring concept CSV.
- The final gameplay must disconnect the *actual* wire and invalidate stale field samples.
  PLC disables the simulated RF amplifier, confirms shutdown, latches critical fault.
  Player repairs wire, recalibrates probe, explicitly resets safety, and runs a fresh
  conducted+radiated emissions and protected RF immunity qualification to earn 6/6.
- W120 telemetry and W125 calibration health cannot bypass missing W119.

VALIDATION SCOPE
- 73 FBXs independently parsed (72 modular + assembled).
- 73 GLBs imported and checked (72 modular + assembled).
- Verified mesh counts, triangle counts, pivot groups, position/bounds and 206 sockets.
- Verified 134 conceptual wire endpoints, W119 source/destination and six objectives.
- ZIP integrity and SHA-256 digests are validated by the packaging script.
- Unity C# compilation, Unity import and Play Mode NOT performed here.

NO IMAGE-GENERATION ARTWORK IS SUPPLIED AS THE PRIMARY DELIVERABLE.
