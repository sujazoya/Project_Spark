# Project Spark Level 93 — Unity gameplay implementation plan

This is an art-only source package. Inspect actual Project Spark classes and integrate using their real APIs, rather than assuming helper methods exist.

1. Use only meshes/materials under a dedicated `L93_HD_VISUAL_ART` root; import models at a shared origin without altering previous levels or LevelGamePlayManager.
2. Provide actual `SparkTerminal` components for the 194 planned wire routes. Reconcile 388 logical endpoints with 272 decorative visual positions. Never join different signal or power domains just because their decorative art sockets coincide.
3. Model one authoritative finite KHEPRI-7 geological core composition and internal structure. Samples consumed for mass analysis must remain consumed after retries; reserve a fresh fragment for post-repair tests.
4. Implement protected CT with realistic finite-stage movement and scan coverage; Raman/IR/XRF with calibrated and noisy spectral measurements; mass-spectrometry/thermal vacuum pressure and resource constraints; specimen custody and contamination witness.
5. Separate real-world process values, sensor readings, controller commands, safety-qualified conditions, displays and animation. A green UI indicator or successfully played animation cannot certify an objective.
6. Provide real three-fault recovery: inert purge failure, CT stage encoder slip and mass detector gain drift.
7. W179 needs actual wire topology loss, time-invalid pressure signal, protected heating/analysis shutdown, reconnection, fresh calibration, explicit safety reset and new qualification run ID.
8. Award six objectives using independently validated evidence and `LevelGamePlayManager`. Preserve levels 1–92 and do not use fake objective-completion flags.
9. Add real occlusion/colliders/LODs, audio, URP/HDRP lighting, baked occlusion and lightmaps, VFX authoring and professional art tuning.
10. Test a new Unity project import before runtime; a structural FBX/GLB check is NOT a Unity compile or Play Mode test.

### Suggested Editor command
`Tools/Project Spark/Levels/Create Level 93` — to be implemented by the gameplay team; the package only includes the `Levels/93/1...4` optional ART menus.
