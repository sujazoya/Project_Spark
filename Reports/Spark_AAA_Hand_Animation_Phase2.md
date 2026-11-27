# Project Spark Phase 2 hand animation

## Workspace and baseline

Unity was open on the project on drive E. The corresponding project on drive D was absent. The fresh pre-change Level 20 replay finished at 10/10 approved wires and 5/5 powered objectives. Its standing switch gaps were 6.9 mm OFF and 3.2 mm ON. The isolated Bootstrap and chapter-load results from Phase 1 are retained with the verification records.

## Original rig and motion

Inspected `SparkProCharacter`, `SparkTelescopicArms`, `SparkTelescopicNativeWireBridge`, `SparkNativeWireCinematic`, the generated Spark V5 Pro rig prefab, and the native cable and terminal owners. Spark retains its original shoulder, elbow, wrist, four pairs of finger joints, and thumb. The original right hand now opens during pickup, closes progressively after the native lead is available, holds during carry, opens after the native connection, and retracts. The cable path follows the original hand. The arm uses the previous wrist frame for limited-speed orientation and a continuous elbow pole. The left hand stays available and clear during the original-hand native lead path.

Grip closure uses the existing end mesh radius and measures the aperture between the original thumb and index pivots. The aperture check limits closure against the modeled diameter. Surface penetration, contact-pad placement and rigid connector roll require close-up art and QA sign-off.

## Manual close-up review

1. Load Level 20, populate the four reviewed resistor slots, leave virtual power off and select Assisted mode.
2. Suggest and approve the next reviewed wire. Watch the original right wrist as it approaches and closes. Check that the left arm remains clear of the connector and torso.
3. Review the pickup, carry and insertion captures from the side and along the approach direction. Check the finger surfaces against the tube and socket rim. Compare any visible gap with the telemetry for that frame.
4. Confirm the wire exists in the native registry, then watch Spark open the fingers and return the original joints to rest. Use native Undo and confirm that the mesh and circuit link are both removed.
5. With power still off, approve OFF and ON for the physical switch. Review the standing-switch clip and the close-ups at contact in both directions.

## Connector authoring

Create **Project Spark > Native Wire > Connector Motion Profile**. Give a terminal a **Spark Native Connector Contact** component with the profile and an authored frame. The frame's forward direction points into the socket and its origin coincides with the native terminal within the profile's contact tolerance. Orient that frame from measured art. The profile controls insertion stroke, positional tolerance, approach-axis tolerance and post-release retreat. An invalid or moved frame stops the action. The native tool still commits and verifies the connection.

The Level 20 art exposes native terminal origins and visible tube ends. The level currently uses that endpoint workflow. Asset-by-asset rigid mating frames and actual angular clearance are pending.

## Inspector controls

**Spark Telescopic Arms** has **Grip Transition Seconds** defaulting to 0.22 and **Grip Wrist Speed** defaulting to 200 degrees per second. **Spark Native Connector Profile** defaults to a 0.035-unit insertion stroke, 0.005-unit contact tolerance, 10-degree approach-axis tolerance and 0.025-unit release travel. Use measured asset values when authoring actual profiles.

See the Phase 2 test report for capture commands, logs and rollback.
