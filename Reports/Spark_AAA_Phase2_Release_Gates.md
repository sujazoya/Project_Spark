# Project Spark Phase 2 release gates

| Gate | Evidence still required |
| --- | --- |
| Physical connector fit | Authored mating frames on the actual socket and plug assets, verified roll or keyway clearance, measured mesh penetration checks, contact-pad placement, and a normal-versus-misaligned physical insertion replay. |
| Hand coordination | Full-body visual review during walking and flight. The right hand owns the procedural cable and the left stays clear. Optional stabilization requires a demonstrated safe support point and release choreography before activation. |
| Cable collision | Segment-level clearance of the final rendered tube across representative obstacles and release, including a path blocked while already walking and a flight interruption during cable carry. |
| Platform readiness | Fresh player build logs and Level 20 native wiring results. Android hardware, touch, voice permissions, microphone input and thermally stable profiling require a device. The Unity-bundled ADB inventory had no connected devices. |
| Visual sign-off | Inspect actual renders for original finger articulation, telescopic joint continuity, connector clearance and release. Numeric checks apply to rig pivots and sampled cable controls. |

For physical socket authoring, measure the narrowest radial clearance and insertion depth on the intended hardware mesh. Set positional tolerance within that clearance and check angular tolerance against the insertion depth and keyed parts. Use the Level 20 measurements as references for its procedural tube and standing switch only. Track `Spark.NativeHand.Pose` and `Spark.NativeCable.Solve` separately in the Unity Profiler alongside total frame time, frame GC and memory in a development player.

Maintain the Engineering Control v14 ownership, power and exam gates. All completion claims must come from the native evaluator.
