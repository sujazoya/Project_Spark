# LEVEL 84 — W097 CRITICAL STAR-TRACKER FEEDBACK RECOVERY

## Exact proposed connection
- Wire: **W097** (planned player-created connection, not an existing `SparkTerminal` wire).
- Source model: `L84_PrimaryStarTracker`.
- Source proposed terminal: `STAR_TRACKER_QUATERNION_OUT`.
- Source art socket: `P139`.
- Destination model: `L84_AttitudeStateEstimator`.
- Destination proposed terminal: `ADCS_ABSOLUTE_ATTITUDE_IN`.
- Destination art socket: `P064`.
- Signal: timestamped absolute-attitude quaternion, calibration, star-match confidence, validity and heartbeat.

## Safety and trust
The star-tracker signal is continuously refreshed, not a change-only event. On actual electrical disconnection, stop fresh trusted attitude updates; age/expire cached samples; propagate estimator covariance; withdraw fine optical-pointing authorization; preserve physical angular momentum and attitude evolution. Coarse gyro/Sun-sensor protection may maintain bounded safe pointing; the independent safety station still enforces wheel and actuator limits. Unqualified secondary diagnostic tracker or SCADA telemetry must never bypass W097.

## Recovery
The player physically reconnects W097, verifies the graph, obtains fresh star observations and catalog matches, checks gyroscope agreement, recalibrates tracker-to-body alignment and estimator covariance, explicitly resets the safety latch, re-establishes fine control and **completes a fresh two-pass optical pointing mission**. Keep Objectives 1–5 historically certified; Objective 6 requires new qualification evidence, not reconnection alone.

## Integration status
This package contains *art and conceptual wiring only*. Unity project circuit solver, attitude dynamics, sensors, safe modes, six objectives, and real electrical terminals must be implemented in the host Unity project.
