# Level 91 — Read-only visual rig notes

- `Unity_Optional/SparkLevel91VisualRig.cs` can bind `ANIM_*` children and animate visual telescope positions, sunshades, photon quality indicators and actual fault/lock lights.
- It is a façade only: it does not measure actual photon events, synchronize time, create motion physics, certify navigation, detect a wire disconnect or advance an objective.
- `SetActualTelescopeAngles` must receive actual measured gimbal pose, not user intent.
- `SetActualAcquisition` must receive measured normalized photon signal and actual sunshade extension (not just bright UI).
- `SetActualTimingAndNavigation` must receive authoritative clock/position confidence and independently certified W163 safety state. The light flags must not alter authoritative safety.
- Preserve each pivot's local initial orientation and position. If gimbal yaw/elevation should be physically serial, attach a separate gameplay transform hierarchy and map named art groups to that hierarchy rather than assuming groups are nested.
