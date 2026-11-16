# Level 90 Read-only Visual Rig Notes

SparkLevel90VisualRig.cs provides these *art-only* entry points:
- SetActualEngineStates(A, B, C, D, measured PPU powers A–D)
- SetActualGimbals(actual yaw/pitch of each A–D engine)
- SetActualPropellantAndMechanisms(tank A/B fractions, port/starboard solar deploy, coolant pump speed)
- SetActualMissionAndSafety(departure progress, measured deltaV normalized for display, trusted W157 validity, fault latch, mission qualification)

Pivots are looked up from the imported 80 ANIM_* nodes. They drive mechanical pieces or decorative indicators. The visual rig never calculates thrust, plasma current, xenon flow, mission success, fault reset, nor manipulates the electrical solver.

Dedicated adapter required in actual Project Spark: physical state -> properly scaled visual parameters. Unity compilation and assembly import remain unverified.
