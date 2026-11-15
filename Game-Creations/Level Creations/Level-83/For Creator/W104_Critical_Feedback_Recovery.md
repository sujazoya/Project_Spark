# Level 83 — W104 critical DC bus feedback loss and power-safe recovery

## Proposed real electrical endpoints
- **W104** (actual player-created wire in future game)
- Source: `L83_BusVoltageTransducer` terminal `MAIN_DC_BUS_VOLTAGE_OUT`, visual anchor `P075`
- Destination: `L83_MainBusEnergyManagementController` terminal `EPS_TRUSTED_BUS_VOLTAGE_IN`, visual anchor `P195`
- Signal: trusted, continuously sampled DC bus voltage with calibration, range validity and timestamp.

## Fault semantics
- Physically disconnect W104: update actual circuit topology; trusted bus feedback invalidates as samples age. The real bus continues to obey energy conservation and does not instantly drop to zero. Last-known voltage/SCADA telemetry, W105 diagnostic health, and the separate independent protector do **not** substitute for W104 closed-loop qualification.
- Power safety PLC withdraws normal high-power permits, sheds optional loads and commands protected fallback. Maintain essential power only if the available physical energy permits. Separate independent undervoltage protector remains active. Latch critical fault.
- Reconnect W104, reacquire fresh samples, verify calibration, independent protection, converter/current limits, and explicit operator reset.
- Run **fresh full sunlight → eclipse → sunlight** qualification, PV/MPPT, pack temperature/current/SOC, load priorities, bus stability and mission deadlines. Only then certify Objective 6 (actual LevelGamePlayManager 6/6). Objectives 1–5 remain historically certified.

## Scope
This package contains **art plus conceptual wiring** only: no actual Unity `SparkTerminal`, circuit solver bindings, EPS physics, playable fault state machine or objectives.
