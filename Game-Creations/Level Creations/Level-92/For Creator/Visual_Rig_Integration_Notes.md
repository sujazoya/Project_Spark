# Level 92 visual rig — authoritative data adapter

`SparkLevel92VisualRig` is in `Unity_Optional/SparkLevel92VisualRig.cs` (runtime folder, never under Editor). Its public methods consume measured feedback:

- `RebindArtPivots()` binds every imported `ANIM_*` once after art placement.
- `SetActualLegPose(index, hipDegrees, kneeDegrees, footDegrees, deployed01, latch01)` for each of six legs; do not feed unachieved commands.
- `SetActualMechanicalState(drillRPM, feedDepthM, winchRPM, carouselDeg, drillTorqueNormalized, anchorConfidenceNormalized)` for measured plant values.
- `SetActualSafetyState(trustedW171Valid, actualLatchedFault, actualMissionQualified)` for actual certified state.

The visual rig does not implement inverse kinematics, collision detection, electrical graph, real six-leg support forces, drill physics, UI, safety PLC or objective completion. It uses named pivot transform modifications and indicator lights only. Do not attach interactions directly to replaceable visual nodes; keep gameplay components under stable layout roots and address art via an adapter.

Unity import conversion: FBX geometric vertices in centimeters; convert to meters once using the selected model importer units. Verify pivot centers and local origin alignment before any locomotion animation.
