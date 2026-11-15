# Level 78 — W083 critical electrometer feedback fault

W083 is a **conceptual** player-connection plan, not an existing Unity wire. Exact pair:

- Source: `L78_DielectricLeakageCurrentElectrometer`
- Destination: `L78_DielectricReliabilityController`
- Signal: `CRITICAL dielectric leakage-current electrometer feedback`

The eventual Project Spark implementation MUST create actual compatible SparkTerminal endpoints with a physically removable W083 wire. On removal the circuit graph must change, the leakage-current measurement becomes INVALID and prior samples STALE. The test source must be inhibited by safety control and the test fixture's stored energy discharged through a modeled safe path. The source and physical electric field must NOT vanish instantaneously merely because the sensor is disconnected. Log and latch the fault; prevent a fake leakage PASS.

After player reconnection require fresh samples, electrometer zero/calibration and explicit reset. Then require a **new** fully processed protective cap and low-k wafer, UV cure, wafer metrology and valid monitor-coupon leakage/reliability screening. Preserve Objectives 1–5 historically certified. Only real SparkLevelEvaluator/LevelGamePlayManager should award 6/6.

Do not treat W084 (electrometer health) or any SCADA status as a substitute for the missing W083 analog feedback path. Electrical testing should use fictional low-energy protected instrumentation, without real hazardous high-voltage procedures.
