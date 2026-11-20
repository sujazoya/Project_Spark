# Level 99 · Silicon Brain

SBC7-EDU-1 is the fictional training circuit used by the playable Level 99 prefab. The original 190 × 130 mm board and component positions come from the SBC-7 source kit. The board substrate scales to 10.13 × 6.93 Unity units on the 15.79 × 8.54-unit workbench. The original source manifests remain available alongside the gameplay route map.

[Training route map](../Data/SBC7_EDU_1_Training_Routes.csv) records W001–W082 and their 18 via spans, plus the independent W083 connection. Coordinates are in millimetres in the board plane. The Unity circuit system tracks player-added connections and native contacts. Firmware programming uses a curated virtual image, readback verification, and accelerated startup timing for the lesson.

| Objective | Evidence used by the level |
| --- | --- |
| Assemble the board | The 62 matching components are seated and their solder contacts closed. |
| Route and verify the board | All 82 route pairs, 18 via barrels, W083, and six solver continuity witnesses pass. |
| Commission the MCU | U1 has an approved image and valid solved power, clock, and reset conditions at boot. |
| Qualify peripherals | A single boot completes the GPIO, timer, external SPI, I²C, and watchdog checks. |
| Repair the board | The player reproduces each fault, isolates power, repairs the diagnosed target, and verifies a fresh boot and diagnostic pass. |
| Prove W083 recovery | Opening the U1–U8 link inhibits the protected output while U1 keeps running. Recovery requires reconnecting, reacquiring the heartbeat, resetting U8, and passing a new boot and diagnostics before the output returns. |

The training faults are C1 crystal loading, an open V11 barrel in W045 on the external SPI bus, and the R18 reset pull-up. The `Run Gameplay Acceptance` command under `Tools > Project Spark > Levels > 99` exercises the campaign flow in Unity from MainMenu and writes `Logs/Level99Acceptance.txt`.
