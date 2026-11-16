# Level 91: W163 trusted atomic clock feedback recovery

**Concept route:** `L91_PrimaryAtomicClockReference` / `P104` → `L91_NavigationIntegritySafetyPLC` / `P221`. `SignalKind=TIME_REFERENCE_FEEDBACK`, `Critical=YES`.

Functional source contact: `TRUSTED_TIME_REFERENCE_OUT`. Functional destination contact: `TRUSTED_TIME_REFERENCE_IN`.

1. Only after Objectives 1–5 have real historical certificates, enter a protected fault test.
2. The player physically removes W163 through the existing wire tool; this archive does not itself contain a playable wire.
3. The circuit graph shows an open edge; time-reference measurement validity is false and last-good sample timestamps remain stale. Missing timing is NOT zero clock bias.
4. Inhibit new autonomous propulsion correction burns while real spacecraft position and velocity continue evolving, and while independent spacecraft protections remain available.
5. Latch the critical navigation fault. SCADA, backup clock and previous ephemeris samples cannot pretend W163 is continuous.
6. Reconnect actual W163; check electrical domain, reference power/return, direction, integrity, sampling rate, calibration and independent safety conditions.
7. Recalibrate real modeled clock bias/frequency and require explicit operator safety reset.
8. Start a NEW post-repair runId. Reacquire four independent pulsars, fit fresh pulse timing, estimate valid navigation covariance and execute the fresh measured correction profile.
9. Historical objective records 1–5 remain awarded; Objective 6 only after genuine full post-repair mission and `LevelGamePlayManager` reaches 6/6.

The entire scenario uses fictional bounded educational parameters; not flight-qualified spacecraft hardware or real X-ray instrumentation.
