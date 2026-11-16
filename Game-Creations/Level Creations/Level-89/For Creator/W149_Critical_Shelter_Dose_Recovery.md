# Project Spark Level 89 — W149 trusted shelter dose feedback recovery

**Source-grounded proposed concept route:** `L89_ShelterReferenceDosimeter` (`P072`) → `L89_HeliosSafetyPLC` (`P222`). Signal: `ANALOG_FEEDBACK`.

Functional port identities: `TRUSTED_SHELTER_DOSE_RATE_OUT` → `TRUSTED_SHELTER_DOSE_RATE_IN`.

1. Complete Objectives 1–5 with actual historical certification, not simply UI toggles.
2. During a protected training test, player physically unplugs actual live W149. It must change the real Spark circuit topology. **This art ZIP does not implement such a wire.**
3. Trusted shelter dose-rate sample becomes invalid/stale, **not zero radiation**. Storm particle environment and physical shielding keep evolving in the authoritative plant.
4. Safety PLC withdraws permission to open the shelter or certify exposure. Preserve existing locked shielding and independent safety/ventilation/power where physically feasible. Latch critical fault.
5. Diagnostic W149 status bits, SCADA cached measurements and independent sensors must not be accepted as substitute for W149 normal certification. Independent emergency shutdown remains active.
6. Repair correct real W149 wire. Confirm 5 V conditioning, returns, valid physical topology and new calibrated within-range timestamped measurements.
7. Require operator **explicit safety reset** after cross-checking independent shelter protection. Issue brand-new post-repair `runId`.
8. Complete full simulated quiet→rise→peak→decay storm with finite protection resources, valid trusted dose-rate, acceptable accumulated fictional dose, essential-systems service and no critical fault.
9. Only the actual `LevelGamePlayManager` may authorize Objective 6 and `6/6`. Never certify on mere reconnection or timer.

Educational game scenario, not a radiation qualification procedure or statement of spacecraft safety.
