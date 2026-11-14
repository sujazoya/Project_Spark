# Level 55 — Visual Rig and Unity Inspector Guide

The kit contains visual-only FBX animation pivots. Nothing here implements magnetic force, a DC motor, a sensor reading or an authoritative level target.

## Steps

1. Instantiate the twenty individual FBXs as children of one `L55_HD_VISUAL_ART` object at local `(0,0,0)`.
2. Attach `SparkLevel55VisualRig` to an independent art controller.
3. Link `rotorSpin` to `L55_LevitationRotor / ANIM_ROTOR_SPIN` and `encoderDisc` to `L55_EncoderABZ / ANIM_ENCODER_DISC`.
4. For `rotorArtRoot`, assign the rotor assembly transform only if the authoritative gameplay wants visual shaft-translation. Do not also move static sensor or socket art.
5. Link `safetyDoor` to `L55_SafetyEnclosure / ANIM_SAFETY_DOOR`; link `disturbancePlunger` to the corresponding transform under `L55_DisturbanceActuator`.
6. Call `ApplyState(shaftAngleDegrees, leftXmm, leftYmm, rightXmm, rightYmm, doorOpen01, disturbance01)` with values measured by the actual magnetic-bearing simulator. The shaft is nominally oriented along X, with X/Y bearing displacement mapped to local Z/Y.
7. Adjust `positionAmplification` for visibility while keeping the real millimeter values unchanged on the diagnostic display.
8. Use `Terminal_Anchors.csv` to align real `SparkTerminal` prefabs with the visual jack rings. Do not turn the art-only empty locators into circuit elements unless assigned and tested through the actual Level 55 binding architecture.

## Level 55 checkpoint sequence

Build 28 wires → calibrate four gap sensors → levitate → 45 RPM no-contact run → left/right disturbance tests → 75 RPM high-speed test → disconnect W13 (right X sensor) → protected stop/touchdown → reconnect → reset → relift → final verified run.

The gameplay evaluator and `LevelGamePlayManager` must record 6/6 authoritatively. The art helper never writes completion state.
