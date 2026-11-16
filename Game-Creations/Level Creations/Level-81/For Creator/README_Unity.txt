PROJECT SPARK — LEVEL 81: OUTLAST THE BITSTORM
HD / AAA-INSPIRED 3D ART ASSET COLLECTION
===============================================

THIS IS REAL MODULAR 3D GEOMETRY, NOT AN IMAGE.
This archive is NOT yet a playable Unity level. No actual Project Spark SparkTerminal
components, player wires, ECC gameplay, safety logic or LevelGamePlayManager objectives
are delivered. The gameplay integration plan and CSVs are designs for implementation.

DELIVERY
- Models/                  70 independent binary FBX 7400 models + assembled FBX.
- GLB/                     70 matching independent GLBs + assembled GLB.
- Textures/                12 procedural material sets x 3 maps = 36 PNG images.
- Unity_Optional/          3 optional Unity Editor helpers and 1 runtime art-only rig.
- Model_Manifest.json      Exact model filenames, bounds, mesh and pivot counts.
- Animation_Rigs.csv       52 actual mesh-owning pivot parents and xyz positions.
- Art_Assembly.csv         Common-origin placement plan.
- Visual_Terminal_Sockets_200.csv, Terminal_Anchor_Plan_200.csv
                            200 visual gold-rimmed socket world locations.
- Gameplay_Wiring_Concept_W001_W130.csv
                            130 proposed source-destination electrical connections.
- Objective_Certification_Plan_6.csv
                            Six proposed objectives. NOT implemented.
- W094_Critical_Feedback_Recovery.md
                            Critical ECC syndrome heartbeat and safe hold specification.
- Source/                  Reproducible original procedural asset exporter, planning,
                            textures, packaging, structural verification and logs.
- Verification_Report.json
                            Independent binary FBX+GLB structure test details.

UNITY SETUP
1. Unzip the package.
2. Import the 70 numbered .fbx assets from Models/ into your Project Spark project.
3. Copy the optional *Editor.cs scripts into any Unity `Editor` folder.
4. Copy SparkLevel81VisualRig.cs into a normal runtime script folder.
5. Select the empty Level 81 layout GameObject in the Hierarchy.
6. Run Tools/Project Spark/Levels/81/Place HD Art Under Selected Layout.
   It places 70 models under L81_HD_VISUAL_ART at local (0,0,0), rotation (0,0,0), scale (1,1,1).
7. Apply / inspect materials via:
   Tools/Project Spark/Levels/81/Create & Apply HD Materials To Selected Art.
8. Inspect import scale via:
   Tools/Project Spark/Levels/81/Validate Imported HD Art.
9. Check that the combined X width is approximately 24.00 Unity metres.
10. Bind SparkLevel81VisualRig to the parent of the visual art root and feed
    its SetActual... methods ONLY from real Project Spark gameplay simulation.

FBX UNIT CONVENTION
- FBX data uses centimetre numeric vertices with UnitScaleFactor=1.
- GLB data uses metre numeric coordinates.
- Up axis = Y; X = left-right; Z = front-back.
- Imported 24 m wide scene MUST NOT become 2400 Unity units wide.
- Common assembly origin (0,0,0) for all 70 models.
- DO NOT place Level81_Complete_ArtKit_Showcase alongside modular assets; it
  is an alternative assembled preview and will duplicate the whole workstation.

MATERIAL NOTES
- Procedural texture maps are preview assets, not artist-baked AAA production maps.
- PNG _MetalRoughAO stores metallic in R, roughness in G, ambient occlusion in B.
  Remap to your active Unity render pipeline's correct channels before shipping.
- Models have basic material colors. Advanced UV unwrapping, custom shaders and
  texture baking may be required to use the maps fully; do not assume UV-ready PBR.
- Transparent glass may require manual URP/HDRP shader alpha/sorting setup.
- Holographic radiation tracks are fictional false-color visualization.

RIG AND SOCKET NOTES
- The 52 ANIM_* parents actually own mesh geometry in both FBX and GLB.
- No baked animation clips are included; the C# rig has typed simulation-driven
  methods for physical movements, event-state indicators, warning lights and VFX.
- The 200 terminal socket meshes are NOT SparkTerminal components, although
  their visual positions are provided in CSV. Actual wiring must be authored
  against Project Spark's verified electrical API. Shared/bus endpoints in the
  130-wire concept require proper topology and terminal compatibility rules.

CRITICAL FINAL FAULT
W094: L81_ECCSyndromeDetectionEngine (ECC_SYNDROME_STATUS_OUT)
   -> L81_CriticalECCFeedbackSupervisor (CRITICAL_ECC_SYNDROME_IN).
Lose trusted ECC heartbeat/status -> mark samples stale -> protected command
hold with fault latch -> actual wire reconnect -> diagnostic checks -> explicit
reset -> fresh three-phase fictional mission to earn authoritative 6/6.
Other monitoring/SCADA signals cannot silently bypass W094.

VERIFICATION
- 71 binary FBXs independently scanned: 70 modular + 1 assembled.
- 71 GLBs loaded by trimesh: 70 modular + 1 assembled.
- Pivot names, bounds, triangle counts and mesh counts verified.
- 52 true pivot parent hierarchies validated in both formats.
- 200 art sockets / 130 conceptual wires / W094 endpoints checked.
- Zip CRC integrity and SHA-256 manifest created by packager.
- Unity Editor C# was NOT compiled in Unity; Play Mode was NOT tested.

NO IMAGE-GENERATION OUTPUT IS INCLUDED AS THE PRIMARY DELIVERABLE.
