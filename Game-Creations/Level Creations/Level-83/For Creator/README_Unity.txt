PROJECT SPARK — LEVEL 83: BEYOND THE SHADOW — UNITY ART INSTALL

1. Extract the complete archive and import all 74 numbered FBX models from Models into Assets/ProjectSpark/Level83/Models.
2. Import scale: FBX numeric coordinates are CENTIMETRES; the GLBs are METRES. Configure Unity to convert exactly once so workstation bounds are about 26.20 m wide x 9.29 m tall. One Unity unit = one metre.
3. Put the three *Editor.cs scripts in an Assets/**/Editor/ folder; put SparkLevel83VisualRig.cs in any runtime folder. Copy Textures/ to Assets/ProjectSpark/Level83/Textures.
4. Select an empty Level 83 layout root. Run Tools/Project Spark/Levels/83/Place HD Art Under Selected Layout. This creates L83_HD_VISUAL_ART with the 74 common-origin modular models.
5. Select L83_HD_VISUAL_ART and run Tools/Project Spark/Levels/83/Create & Apply HD Materials To Selected Art.
6. Run Tools/Project Spark/Levels/83/Validate Imported HD Art; confirm 74 models, 56 pivot parents and expected world dimensions.
7. Optional: use Level83_Complete_ArtKit_Showcase.fbx or .glb for an alternate static showcase, NOT alongside all modular models.
8. The visual rig is an art-only adapter. The 138 wiring CSV entries and 212 socket art anchors are design plans, not instantiated SparkTerminals or gameplay circuits. Build the EPS and power safety simulation using your actual Project Spark APIs.
9. W104 future trusted bus-voltage connection: L83_BusVoltageTransducer -> L83_MainBusEnergyManagementController. Do not bypass with SCADA or stale samples. See W104_Critical_Feedback_Recovery.md.
10. Preview PBR maps: R metallic, G roughness, B ambient occlusion. Map separately to your URP/HDRP shader channel expectations. Transparent regions and thousands of cell meshes require LOD and draw-call optimization.
11. No Unity compilation, Play Mode testing, battery safety testing or real spacecraft flight certification has been performed.
