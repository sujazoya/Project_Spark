# Level 82 — Complete gameplay integration plan

## Status
The asset pack contains original procedural 72-FBX and matching GLB visual artwork, 54 mesh-owning pivot groups, 206 modeled gold-rimmed socket meshes, 134 planned electrical connections, and Unity art helpers. **It does not contain working Project Spark gameplay, electrical circuits or six objectives.**

## Authoritative domains
- **Electrical:** real `SparkTerminal` components, 134 player-created wires, 5V logic supply, analog sensors, actuator commands, source/return verification, independent safety power domain.
- **EMC field model:** bounded educational spectral sources, cable common-mode and differential currents, frequency-dependent shield leakage, antenna orientation/height/polarization, filter attenuation and susceptibility; no full-wave Maxwell solution claimed.
- **Machine motion:** real turntable angular dynamics, calibrated antenna mast lift and polarization, 3D near-field probe positions, door mechanics. The `SparkLevel82VisualRig` accepts physical state only.
- **Mission monitoring:** Level 81 EUT sensor/clock/optical telemetry workload, qualified command gate, tolerance and thermal limits.
- **RF safety:** trusted field feedback W119, chamber door, independent emergency stop, RF source disable with measured confirmation, latched fault and explicit reset.

## Disturbances
1. Wrong common-mode harness routing/return current creates conducted/radiated peak.
2. Open conductive shield gasket increases seam radiation.
3. Controlled immunity RF couples into analog input and causes navigation sensor error; repair must not cause excess latency.

## Objective sequence
See `Objective_Certification_Plan_6.csv`. Must be connected to real `LevelGamePlayManager` and prove 6/6 through **actual** gameplay evidence.

## Art import
Import FBX models once, not assembled showcase simultaneously, apply centimetre-to-metre conversion once, set all modular model transforms to identity, keep gameplay objects outside replaceable `L82_HD_VISUAL_ART`.
