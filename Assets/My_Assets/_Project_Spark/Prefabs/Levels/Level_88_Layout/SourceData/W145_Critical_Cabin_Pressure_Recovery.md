# Level 88 — W145 Trusted Cabin Pressure Feedback Recovery

Planned critical row: `W145`, `L88_CabinPressurePrimaryTransducer` (`P134`) → `L88_HabitatLifeSupportSafetyPLC` (`P180`), `ANALOG_FEEDBACK`. Functional contacts: `TRUSTED_CABIN_PRESSURE_OUT` → `TRUSTED_CABIN_PRESSURE_IN`.

1. Historical Objectives 1–5 must be certified in the real LevelGamePlayManager first.
2. In a supervised fault exercise, player unplugs the real W145 runtime wire (art kit ships no connected gameplay wires), mutating the Spark electrical circuit graph.
3. Treat the trusted pressure sample as **invalid**, not zero pressure. Actual pressure continues via mass balance, P*V=n*R*T; do not teleport gas mass.
4. Withdraw normal pressure-regulation authorization; restrict unverified actuator commands and enter a protected safe hold with independent pressure relief and backup physical monitoring still effective. A severe actual gas fault can still cause unsafe atmosphere—do not claim immortal protection.
5. Latch critical safety fault and log event, preserve historical 1–5 objective records. SCADA cached readings, backup diagnostic pressure and W145 health bits cannot impersonate trusted feedback.
6. Repair correct live wire, verify 5 V source and return, acquire fresh range-valid sensor samples, calibrate primary pressure measurement and independently cross-check safety protection.
7. Require operator **explicit reset** only after qualified preconditions; start a **new runID** and perform full gas/humidity/CO2/O2 and four-compartment endurance test.
8. Only after a complete clean post-fault test may actual LevelGamePlayManager certify 6/6; reconnection alone can never complete Objective 6.

All units/limits are configurable fictional educational scenarios, not real spacecraft ECLSS safety specifications. These instructions are design requirements, not existing gameplay scripts.
