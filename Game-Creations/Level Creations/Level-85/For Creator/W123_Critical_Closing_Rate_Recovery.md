# LEVEL 85 — CRITICAL W123 TRUSTED CLOSING-RATE FEEDBACK

This is a **design proposal**, not a prebuilt Unity wire.

- Wire: **W123**, critical `ANALOG_FEEDBACK`.
- Source: `L85_RelativeNavigationFusionComputer`.
- Source logical terminal: `TRUSTED_RANGE_RATE_OUT`.
- Source 3D visual socket: `P148`.
- Target: `L85_DockingSafetyPLC`.
- Target logical terminal: `TRUSTED_RANGE_RATE_IN`.
- Target 3D visual socket: `P046`.

The authoritative continuous feed must include signed/filtered closing speed, sequence number, timestamp, confidence and sensor validity, not just a frozen floating-point display. W124 is **display telemetry only** and W121 is sensor diagnostic health, neither may substitute for W123.

A player must first create the real electrical graph connection. On a deliberate final disconnection after Objectives 1–5, real graph continuity changes, W123 age expires, automated docking/capture permission is withdrawn, and the independent safety PLC commands a controlled HOLD or ABORT through modeled actuators and available fuel. Do not teleport the vehicle to zero velocity, and do not undock an already safely latched vehicle. Latch the fault and log causal evidence. On reconnection require fresh calibrated measurement and timestamp, cross-check radar/LiDAR consistency, perform explicit operator reset, and run a completely new full orbital rendezvous and dock sequence. Historical first five certificates persist; Objective 6 earns 6/6 only after fresh evidence.

The art/CSV package has no real player wires, actual SparkTerminal components, spacecraft physics, pressure simulation, safety PLC state machine or implemented LevelGamePlayManager objectives. Build them in the user's existing Unity project using verified APIs.
