# Project Spark Level 52 — Animation / Inspector Setup

Attach `SparkLevel52VisualRig` to the Level 52 layout root (or a suitable art coordinator). Assign:

| Visual rig reference | FBX object group |
|---|---|
| `motorShaftRotateX` | `01_L52_DriveMotor/MOTOR_SHAFT_ROTATE_X` |
| `globalDrivetrainShaftRotateX` | `00_L52_IndustrialBase/GLOBAL_DRIVETRAIN_SHAFT_ROTATE_X` |
| `testRotorRotateX` | `02_L52_TestRotor/TEST_ROTOR_ROTATE_X` |
| `balanceRingRotateX` | `04_L52_ActiveBalanceRing/RING_ROTOR_ROTATE_X` |
| `encoderAbzDiscRotateX` | `06_L52_EncoderABZ/ENCODER_ABZ_DISC_ROTATE_X` |
| `correctionMassAOrbitX` | `04_L52_ActiveBalanceRing/CORRECTION_MASS_A_ORBIT_X` |
| `correctionMassBOrbitX` | `04_L52_ActiveBalanceRing/CORRECTION_MASS_B_ORBIT_X` |
| `frontSafetyDoorHingeY` | `12_L52_SafetyGuard/FRONT_SAFETY_DOOR_HINGE_Y` |
| `installedImbalanceTestWeight` | Mounted instance of `03_L52_ImbalanceWeight/IMBALANCE_TEST_WEIGHT_DETACH` |

All transform paths depend on the FBX importer and may include automatically generated model-root prefixes. Find the **exact named group** in the hierarchy rather than matching a guessed path.

Use valid external system values:

```csharp
visualRig.SetMeasuredRPM(actualMotorRPM, actualEncoderRPM);
visualRig.SetMeasuredCorrectionAngles(actualAngleA, actualAngleB);
visualRig.SetSafetyGuardOpen(guardReallyOpen);
visualRig.SetCondition(actualWarning, actualFault);
visualRig.SetImbalanceWeightInstalled(weightReallyInstalled);
```

These methods alter **visuals only**. They do not implement active balancing calculations, no measurement or objective passes automatically, and no electrical components are stamped. For the Z fault, rely on actual wiring and encoder signal-validity checks, not whether a lamp happens to illuminate.
