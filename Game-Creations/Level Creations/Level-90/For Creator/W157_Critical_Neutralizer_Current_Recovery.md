# Project Spark Level 90: W157 critical neutralizer feedback recovery

**Source-authored concept route:** `L90_NeutralizerCurrentTransducer` (`P214`) → `L90_PropulsionSafetyPLC` (`P216`), `ANALOG_FEEDBACK`, `Critical=YES`.
Proposed functional contacts `TRUSTED_NEUTRALIZER_CURRENT_OUT` → `TRUSTED_NEUTRALIZER_CURRENT_IN`.

1. Require Objectives 1–5 to have historical certificates, not decorative UI badges.
2. Run a supervised safe test in fictional game simulation; player physically disconnects live W157. This ZIP contains **no actual SparkTerminal/wire objects**.
3. Validity and timestamp of *trusted* neutralizer current are invalidated immediately. **Do not treat missing feedback as zero current** or assume the actual ion discharge ceases instantly.
4. Withdraw normal discharge/ignition authorization and command safe bounded shutdown of the relevant engines. Observe actual current/power decay in the separate educational plasma plant. Maintain independent electrical and thermal protection.
5. Preserve real tank inventory, spacecraft velocity, pending maneuver and historical Objectives 1–5. An uncontrolled thrust/propellant state is reported honestly rather than silently normalized.
6. Explicitly prevent W157 replacement by display-only telemetry, cached SCADA values, or a separate status indicator.
7. Player reconnects the correct functional wire; validate electrical graph, measurement power/return, polarity, range, calibration, timestamp, safety and independent shutdown confirmation.
8. Require explicit operator safety reset; new ignition cannot start until all safety interlocks pass.
9. Start a fresh post-repair test run ID. Recommission all four thrusters, execute a complete actual safe departure/low-thrust profile and validate propellant, power, thermal and trajectory constraints.
10. Only the authoritative `LevelGamePlayManager` may record Objective 6 and real **6/6**, never on reconnection or a timer alone.

Fictional educational game simulation, **not** spacecraft propulsion qualification or safe real-world plasma setup instructions.
