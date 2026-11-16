# Project Spark Level 80 — W078 critical RX optical power feedback recovery

## Electrical path (proposal, NOT an implemented SparkCircuitSystem connection)
- Wire ID: `W078`
- Source: `L80_ReceiverOpticalPowerMonitor`
- Destination: `L80_OpticalLinkSupervisoryController`
- Proposed source terminal: `RX_OPTICAL_POWER_SIGNAL_OUT`
- Proposed destination terminal: `LINK_SUPERVISOR_RX_POWER_IN`
- Source file: `Gameplay_Wiring_Concept_W001_W126.csv`, row W078.

## Explicit trust boundary
- W075 optical spectrum and W076 power-budget measurements are **not** trusted substitutes for W078.
- W074 receiver power SCADA telemetry is not W078.
- W079 receiver-power instrument diagnostic health to safety is not W078.
- No stored, stale or display reading can certify optical safety/receiver power.

## Authoritative sequence required in gameplay implementation
1. Objectives 1–5 are historically certified.
2. Player removes the real W078 wire; circuit graph updates and trusted RX-power sample invalidates.
3. Samples are marked stale; supervisor withdraws power qualification; stops data-link training/certification.
4. Independent safety pathway commands controlled laser output disable and protective shutter closure; optical power state decays according to source and shutter dynamics. Fault latches and is recorded.
5. Player physically reconnects W078. New validly routed samples are acquired, but measurement is not immediately calibrated.
6. Player calibrates RX power monitor, verifies safe shutter and source, explicitly resets protection.
7. All four wavelength channels are reacquired and re-locked; fresh optical power/BER/eye/data-route qualification passes.
8. Only then Objective 6 is certified by the real `LevelGamePlayManager`; Objectives 1–5 retain historical completion.

The delivered files contain **art and a plan**. No functional SparkTerminals, player-wiring system or safety logic is implemented in these meshes.
