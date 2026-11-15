PROJECT SPARK — LEVEL 84: DEFY THE TUMBLE
========================================

ART ASSETS ONLY — NOT A COMPLETED UNITY GAMEPLAY LEVEL

Package:
- 76 individual true binary Autodesk FBX 7.4 mesh files under Models/.
- 76 matching glTF 2.0 GLB mesh files under GLB/.
- One assembled FBX and one assembled GLB (reference alternatives, not to duplicate with modular art).
- 58 named ANIM_* mesh-owning parent nodes in the FBX and GLB hierarchies.
- 218 named SOCKET_P001..SOCKET_P218 mesh-owning art socket nodes.
- 12 procedurally generated 1024x1024 PBR reference sets, 36 PNGs.
- Four optional C# art placement/material/validator/runtime visual rig helper files.
- W001..W142 conceptual wire plan, socket anchor plan and six-objective proposal.

Unity import:
1. Import all 76 numbered FBXs into Assets/ProjectSpark/Level84/Models.
2. Copy the 3 Editor C# scripts to an Editor folder and the VisualRig to a runtime scripts folder.
3. Select an empty Level 84 layout parent GameObject.
4. Tools/Project Spark/Levels/84/Place HD Art Under Selected Layout
5. This creates L84_HD_VISUAL_ART from the 76 origin-aligned FBX prefabs.
6. Optional: Tools/Project Spark/Levels/84/Create & Apply HD Materials To Selected Art
7. Tools/Project Spark/Levels/84/Validate Imported HD Art
8. Add SparkLevel84VisualRig to L84_HD_VISUAL_ART (or existing suitable parent).
9. Bind the optional runtime Light and ParticleSystem references. They are NOT preinstalled.
10. Connect the visual rig only to the authoritative measured values from your future ADCS gameplay scripts.

Scale:
- FBX numeric coordinates are centimetres with UnitScaleFactor 1.
- GLB vertices are in metres.
- Expected imported assembled workstation: approximately 27.0 metres wide, 10.47 metres high.
- Verify FBX Unity import factor; do not apply centimetre/metre conversions twice.
- X is width, Y is vertical, Z is front. All modular models share common origin.

Important:
- No real SparkTerminal components, electrical player wires, Project Spark circuit-solving code,
  spacecraft attitude solver, real Unity level definition, six objective implementation or
  Unity Play Mode test is in this art pack.
- Concept wire allocation uses valid-looking source/target equipment but must be reconciled
  with the actual Project Spark signal and fanout rules. SOCKET_* nodes are visual anchors only.
- W097 is the reserved critical primary-star-tracker quaternion feedback, from
  L84_PrimaryStarTracker STAR_TRACKER_QUATERNION_OUT to
  L84_AttitudeStateEstimator ADCS_ABSOLUTE_ATTITUDE_IN.
- A closed-loop ADCS estimator must invalidate stale W097 messages and hold/limit unsafe commands.
- The large three-axis rig visually represents orientation; it must NEVER apply free flight torque.
- Textures are reference procedural PBR maps. Final art should get UVs, LODs, shader work,
  baked maps and draw-call optimization for full production AAA quality.
- Unity compilation/Play Mode testing has not been performed in this environment.
