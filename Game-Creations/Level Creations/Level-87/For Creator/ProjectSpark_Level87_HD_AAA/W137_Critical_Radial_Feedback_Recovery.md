# Level 87 — W137 critical trusted radial displacement feedback

**VERIFIED SOURCE PLANNED WIRE:** `W137`: `L87_PrimaryRadialGapSensor` `(P019)` → `L87_MagneticBearingSafetyPLC` `(P135)`; `ANALOG_FEEDBACK`.

Source logical port: `TRUSTED_RADIAL_GAP_OUT`. Destination logical port: `TRUSTED_RADIAL_GAP_IN`.

1. After historical Objectives 1–5 certify, arm supervised low-speed exercise with independent touchdown/overspeed protection available.
2. Player must disconnect an **actual runtime** W137 SparkCircuitSystem wire; source art contains no connected wire.
3. Safety PLC invalidates *new and cached* primary radial-gap samples; does not interpret missing as zero displacement.
4. Withdraw normal high-speed and contactless-bearing control authorization; command controlled rundown if redundant sensors, energy, motor torque and mechanisms permit; otherwise protective response, retained independent overspeed and touchdown systems.
5. Latch critical safety fault and record event evidence; old Objectives 1–5 remain historically valid. Never magically stop angular velocity or fully disable a physically spinning ring.
6. Player reconnects correct compatible port, restores actual electrical continuity, receives genuinely fresh calibrated samples; verify backup sensor, overspeed relay, bearing temperatures, power and touchdown readiness.
7. Explicit operator reset **after** preconditions; create new full run ID, recommission magnetic levitation, accelerate, qualify sustained spin and habitat loads.
8. Only the real LevelGamePlayManager may reach 6/6 after fresh complete certification. W137 reconnect by itself can never count.

Non-authoritative SCADA, patch monitor and redundant diagnostic display never replace missing trusted W137 feedback. These steps are proposed implementation requirements, not present in the art asset files.
