# Project Spark — Level 73 CMP Gameplay Integration Plan

## Scope: actual binary FBX/GLB artwork, **not** functional Unity gameplay

This pack provides 54 individually positioned CMP equipment models, matching GLBs, one assembled reference model in each format, actual parent pivot hierarchy names for 35 moving/inspection subassemblies, material regions, Unity helper scripts, geometry export sources and structural verification.

The pack **does not** implement 98 playable `SparkWire` connections, 152 unique `SparkTerminal` anchors, circuit solver, CMP physical model, shop, PLC or six authoritative objectives.

## Workstation industrial process

- Primary polishing axis at roughly X=0, Z=-0.55. Polishing platen and grooved pad rotate independently as mesh groups, driven by the same physical platen RPM. The wafer carrier head and spindle have their own motor and vertical force actuator.
- Slurry supplied from a cabinet and transparent mixing tank, metered by a pump and applied by an articulated nozzle. Model finite slurry inventory, flow, concentration, lubrication and waste.
- Conditioning arm and disc may restore consumable pad surface properties within service-life bounds. Pad glazing, groove wear and conditioning are separate state variables.
- Wafer regional topography should be generated from an authoritative height/thickness grid; `MicroscopicSurfaceTopography` is a sample visualization, not a working shader.
- The post-CMP station contains separate rinse, brush and spin-dry components.

## Approved gameplay targets (design only)

- Player-created wires: **98** (see Gameplay_Wiring_Budget.csv).
- Unique registered functional terminals: **152**.
- Objectives: **six**, integrated into existing Project Spark LevelDefinition / LevelEvaluator and `LevelGamePlayManager`.
- Main measurements: platen/carrier RPM, contact downforce, zone pressure, slurry flow, pad wear, temperature, motor torque, optical/eddy current endpoint, regional wafer surface heights.
- Critical final feedback path: `CARRIER_DOWNFORCE_SENSOR_OUT → MULTIZONE_PRESSURE_PID_FORCE_FEEDBACK_IN` (actual W-number not yet assigned). Remove the real signal wire; measurement becomes invalid while the physical force still exists; enter safe hold, repair, recalibrate and pass a new wafer certification.

## Physics guidance (reduced-order educational system)

For wafer region i: MRR_i = Kp_i * Pressure_i * RelativeVelocity_i * f_slurry * f_pad * f_temperature.
Evaluate contact geometry and relative velocity from separate finite-inertia motors; pressure based on measured load and contact zones. Integrate removal only with proper mechanical contact, slurry and valid safety. Apply separate patterned features to track dishing, erosion, residual film and scratches. Use measurement-based endpoint detection; post-process profilometry/inspection validates wafer quality. Do not fabricate results.

## Six objectives

1. **Build the Precision Polishing Network** — wire/validate actual 98 wires and 152 terminals.
2. **Commission the CMP Workstation** — calibrate sensors, start independent motors, verify pad/slurry and load wafer safely.
3. **Master Pressure and Material Removal** — actual pressure control and finite material removal; control torque, vibration and thermal state.
4. **Achieve Perfect Planarization** — detect actual endpoint, clean/rinse/dry wafer, verify height variation, thickness, ≤3% nonuniformity and configured dishing/erosion/defect targets.
5. **Survive the Polishing Disturbances** — pad glazing, slurry restriction and edge-pressure drift; recover and certify a fresh wafer.
6. **Restore Critical Downforce Feedback** — physically disconnect feedback wire, invalidate measurement, safe hold, reconnect, recalibrate, explicit fault reset and successful fresh processing.

## Target objective state

Objectives 1–5 remain historically certified through the deliberate final fault; actual authoritative `LevelGamePlayManager` reaches 6/6 only after legitimate Objective 6 certification.

## Integration caution

The animation-hierarchy Null names are embedded in the FBX and GLB, but there are no baked animation clips. Use the bundled `SparkLevel73VisualRig` as an optional **visual-only consumer** of actual motor and actuator states. Verify correct 17.15-m imported model width before placing colliders and functional terminals. No claimed Unity Play Mode test.
