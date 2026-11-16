# W129 — Trusted Contact Wrench Feedback — Level 86

**SOURCE-CONCEPT ROW (NOT LIVE PLAYER WIRE)**

- Source: `L86_SixAxisWristForceTorqueSensor` at `P045`, logical port `W129_OUT_ANALOG_FEEDBACK`.
- Destination: `L86_RoboticContactSafetyPLC` at `P119`, logical port `W129_IN_ANALOG_FEEDBACK`.
- The real Unity implementation must use a player-created, physically detachable Spark wire and actual solver topology.

## Fault and recovery

When Objectives 1–5 are historically complete, a deliberate actual W129 disconnect invalidates fresh trusted contact wrench. The independent contact safety PLC must prohibit further contact motion, preserve gripper support if holding a payload, withdraw unsafe joint/fastener authorizations and latch a critical fault. No current can become zero by UI convention; previously valid measurements cannot be reused.

After a real wire reconnection, verify power and returns, acquire fresh calibrated six-axis force+torque samples, check overload/clearance/gripper and thermal isolation conditions, then require an **explicit** operator reset. Run a genuinely fresh, complete manipulation, interface and cooling qualification using a replaceable service cassette, recording new `runId`, evidence and event timestamps. Only then may objective 6 certify in the actual `LevelGamePlayManager`.

Objectives 1–5 retain historical certification even when live machine state becomes unsafe. Diagnostic SCADA feedback may never substitute for W129. No Unity runtime, SparkTerminal or actual safety PLC is delivered by this art package.
