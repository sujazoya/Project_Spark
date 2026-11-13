# Project Spark — Level 53: The Hidden Couple — Visual rig

These FBXs are **art only**. No electrical component scripts, signal transport, gameplay evaluator, or motor dynamics are included.

## Install

1. Import individual FBXs from `Models` (not the assembled reference) into a Level 53 asset folder.
2. Create `Level_53_Layout` using your actual level/prefab creation pipeline.
3. Use `showcase_positions` in `Model_Manifest.json` for initial art-relative placements; rescale the complete art uniformly and align to the workbench before binding electrical terminals.
4. Attach `SparkLevel53VisualRig` to the level art root, and drag the exact named transforms from the FBX instances.

| Serialized visual reference | FBX transform group |
|---|---|
| `motorShaftRotateX` | `01_L53_DCMotor / MOTOR_SHAFT_ROTATE_X` |
| `longRotorShaftRotateX` | `02_L53_LongRotorShaft / LONG_ROTOR_SHAFT_ROTATE_X` |
| `planeARotorRotateX` | `03_L53_CorrectionPlaneA / PLANE_A_ROTOR_ROTATE_X` |
| `planeBRotorRotateX` | `04_L53_CorrectionPlaneB / PLANE_B_ROTOR_ROTATE_X` |
| `planeACorrectionWeightOrbitX` | `03_L53_CorrectionPlaneA / PLANE_A_CORRECTION_WEIGHT_ORBIT_X` |
| `planeBCorrectionWeightOrbitX` | `04_L53_CorrectionPlaneB / PLANE_B_CORRECTION_WEIGHT_ORBIT_X` |
| `encoderABZDiscRotateX` | `09_L53_EncoderABZ / ENCODER_ABZ_DISC_ROTATE_X` |
| `safetyFrontGuardDoorHingeY` | `14_L53_SafetyEnclosure / SAFETY_FRONT_GUARD_DOOR_HINGE_Y` |

## Critical animation note

Each balancing plane has its rotating disc and counterweight as **separate sibling transform groups**. Do not reparent them casually. The counterweight receives shaft phase plus its individually measured angular correction. The static authored angles are Plane A **35°** and Plane B **225°**; inspector defaults match them.

Connect real runtime data, e.g.:

```csharp
visualRig.SetMeasuredRPM(actualMotorRPM, actualEncoderRPM);
visualRig.SetMeasuredShaftAngle(actualShaftAngleDegrees); // optional but preferred
visualRig.SetCorrectionAngles(actualPlaneAAngle, actualPlaneBAngle);
visualRig.SetSensorValidity(actualLeftSensorValid, actualRightSensorValid);
visualRig.SetGuardOpen(actualGuardOpen);
visualRig.SetSafetyCondition(actualSafetyInterlockSatisfied);
```

**Never use these visual values as the evaluator's measured vibration or proof that an objective passed.** The 2x2 complex influence matrix, vibration amplitude/phase, fault and safe stop behavior must be handled by actual authoritative Unity components.

## Wiring/gameplay

`Exact_Wiring.csv` defines 24 requested wire routes and `Terminal_Anchors.csv` provides 40 visible art jack positions. These are not actual `SparkTerminal` or `SparkElectricalComponent` instances. Use your existing circuit graph, scene binding IDs, physical terminal placement and signal architecture. Snap each real terminal onto a workbench hole and visually align its art jack; the manifest positions are not exact world-space Unity terminal placements.

**Install W15 last**, even though its ID is W15, so `Undo Last` can be used for the deliberate right-vibration-sensor fault exercise.
