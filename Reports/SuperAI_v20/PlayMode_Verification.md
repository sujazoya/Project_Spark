# Unity verification

The historical v19 reports preserve Level 1 (3/3 connections, 1/1 objective) and Level 20 (10/10 connections, 5/5 objectives) as baselines. The v20 Level 48 fixture writes its own result to `Logs/SuperAI_v20/Level48PlayMode.txt`.

Campaign asset scans and draft validation establish structural coverage only. No Android or Windows player packaging is part of this work.

- Unity 6000.3.19f1 compiled the runtime and Editor assemblies after the final approach change.
- The independent wire-10 fixture registered its native connection. Both full Level 48 attempts registered the first four pairs and stopped before wire 5. They did not reach official objective verification.
- The latest Unity audit produced 130 unique entries and 66 structurally validated wire recipes. The payload excludes authorization and never changes a circuit.
- Levels 1 and 20 retain their historical baseline evidence. The final v20 flight changes were not replayed on those two levels in this pass.

The final isolated wire-10 rerun passed after retaining the safe ground transfer for fixed-reach pickups. The destination flight scan found 2,829 clear routes. Its native connection registered and Unity returned to Edit Mode. An earlier experiment that forced airborne carry hit the source pin and is retained in Level48Wire10WideCarryAttempt.txt. The last full sequence ran before that routing guard and stopped at wire 5.

## v20.1 Play Mode

- Level 1 passed the native 3/3 wiring and 1/1 evaluator fixture.
- Level 20 registered 10/10 reviewed wires, honored pause/resume, passed standing switch OFF and ON, and reached all 5/5 objectives. The same suite later failed a separate controlled test using relocated sockets and an artificially limited 1.15 m reach. It reported cable overstretch before completing that walking fixture.
- These two runs exercised the alternate-takeoff implementation. They preceded the later original-hand flight change to use calibrated limb length.
