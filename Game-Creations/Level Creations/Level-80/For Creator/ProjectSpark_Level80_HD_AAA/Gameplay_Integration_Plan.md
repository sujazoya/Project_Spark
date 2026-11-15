# Project Spark Level 80 — Unity gameplay integration plan

**Level:** `level_80` — Command the Light

## Scope
The art package includes 68 procedural modular binary FBXs, 68 matching GLBs, assembled references and 50 genuine named animation parent transforms. It does **not** include functional gameplay components, working optical graph or live electrical simulation.

## Distinct interacting simulation domains
1. **Electrical domain**: the user's W001–W126 control, analog, digital and supply wires, real SparkTerminals and monitored disconnects. A conceptual CSV is provided; convert it through the actual Project Spark electrical API.
2. **Optical domain**: source optical output, safety shutter, attenuator, coupling efficiency, photonic guide/fiber losses, four resonators, WDM multiplexer/demultiplexer, drop-port power and photodiode conversion.
3. **Thermal/control domain**: four actual ring-heater temperatures with coupled thermal response, source wavelength stabilization and modulation bias.
4. **Data domain**: separate Logic, Memory, I/O and Sensor streams, modulation, clock recovery, modeled eye, jitter, BER/FEC and routing evidence.
5. **Safety domain**: protected laser source, independent shutter, disabled transmit/training on missing trusted W078 feedback, latched reset, maintenance/calibration.

## Calibration scenario targets (fictional educational parameters)
Four carrier channels with individually tuned resonances, sufficient per-channel RX power, safe bounded laser output, low crosstalk and a stable qualification period; no specific real industrial laser safety levels or recipes are supplied.

## Objective evidence
Consult `Objective_Certification_Plan_6.csv`. All six are **unimplemented** in this art pack. An actual Unity Level 80 gameplay builder and an authoritative 6/6 evaluation remain to be built.

## W078
Consult `W078_Critical_Feedback_Recovery.md`. W078 is the receiver optical power monitor's measurement output into the optical link supervisor. SCADA data and diagnostic health must never silently bypass it.

## Art rig
Attach `SparkLevel80VisualRig` to a stable parent of `L80_HD_VISUAL_ART` and call only its `SetActual...` methods from the authoritative optical/thermal/data process state. The rig never creates or approves a measurement by itself.

## Electrical graph precautions
External optical fibers and on-chip waveguides are **not** electrical `SparkTerminal` wires. Actual player connections control or monitor optoelectronic components. Use a separate optical network representation, coupled through source/modulator/photodiode devices.
