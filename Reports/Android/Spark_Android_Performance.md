# Spark Android performance

| Area | Status | Evidence |
| --- | --- | --- |
| Android quality profile | PASS | Serialized settings select the existing Mobile URP asset on Android. That asset uses 0.8 render scale, 1024 main-light shadows, one shadow cascade and no additional-light shadows. The mobile renderer differs from High in decal and screen-space occlusion features. Phone readability is a device check. |
| Cable quality budget | PASS | Runtime code limits mobile cable geometry to 16 segments and four solver iterations through the existing native cable path. |
| FPS and frame time | NOT TESTED | No Android device frames were sampled. Both 30 FPS and 60 FPS targets remain unverified. |
| GC, memory, loading and latency | NOT TESTED | The Android development fixture can report elapsed native wiring time, CPU and GC counters, and peak used memory when the device exposes them. |
| GPU, textures, thermal and battery | NOT TESTED | Needs an actual device, stable test conditions and profiler support. |

Profile `Spark.NativeCable.Solve` and `Spark.NativeHand.Pose` alongside total frames during Level 20. Capture device model, refresh rate, graphics API and Android version. Compare cold launch and sustained play separately. Editor timing from Phase 2 does not measure Android performance.
