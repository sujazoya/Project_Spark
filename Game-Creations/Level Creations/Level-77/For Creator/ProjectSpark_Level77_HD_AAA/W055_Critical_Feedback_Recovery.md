# Level 77 — W055 optical barrier endpoint feedback: Unity implementation and safety sequence

**CONCEPTUAL WIRE:** W055 (`L77_DualWavelengthBarrierEndpointSpectrometer` → `L77_ThreeStageCMPProcessController`).

**Suggested real SparkTerminal endpoint IDs** after inspecting Project Spark APIs:

- `BARRIER_ENDPOINT_SPECTROMETER_SIGNAL_OUT`
- `THREE_STAGE_CMP_CONTROLLER_ENDPOINT_IN`

These names are proposed; neither functional terminal nor its player wire has yet been created.

## Critical fault progression

1. After objectives 1–5 are historically certified, the player physically removes the actual W055 player-created wire, using the existing Project Spark wire system.
2. A real circuit graph mutation removes the analog optical measurement path. Mark the received spectral endpoint signal INVALID and previous samples STALE, even if the physical spectrometer retains power.
3. The CMP controller must not reuse cached pre-fault endpoint values or treat SCADA visualization as measurement evidence.
4. The controller enters a protective safe stop: inhibit new polishing, command platen and carrier motors to decelerate through actual plant dynamics, ramp down downforce, safely retract the polishing carrier, suspend wafer processing and latch the critical fault.
5. Update process historian, safety station and SCADA from the authoritative state. **Do not** instantaneously reset all motor RPM or layer thickness to zero just because a wire disconnects.
6. The player reconnects the real W055 wire. Verify signal topology and electronics power. Acquire fresh optical samples with proper timestamp/validity but leave calibration invalid until performed.
7. Explicitly run spectrometer/viewport calibration; measure spectral reference response; verify signal confidence; require operator fault reset after verifying safety conditions.
8. Recommission all three stations, load a **fresh** copper-plated wafer, perform bulk copper removal, soft landing, selective barrier clearing, cleaning and drying; pass residue, copper dishing, dielectric erosion, line isolation, via continuity, corrosion and surface defect tests.
9. Certify objective 6 only when the existing `LevelGamePlayManager` objectively reaches 6/6; preserve historical objectives 1–5 through the deliberate test.

## Physics distinction

Measured optical feedback is an instrument signal. The polished wafer's copper/barrier/dielectric geometry evolves due to *actual process contact, pressure, platen speed and material removal*; no amount of SCADA status text can remove a material layer. The fault triggers a safe stop, not a fabricated finished wafer.

## Unimplemented in this pack

The included W055 CSV is a wiring plan. This package contains only real art geometry, pivot groups and optional Editor/art animation helpers. No working SparkTerminals, PLC, circuit, CMP physics, faults or objectives are shipped.
