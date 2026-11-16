# Level 81 — Unity gameplay integration plan

## Deliverable is visual art and planning only
- 70 original procedural modular FBXs and 70 corresponding GLBs, plus assembled preview in each format.
- 52 real named hierarchy pivot parents in FBX/GLB (see `Animation_Rigs.csv`).
- 200 visually modeled terminal sockets with matching world coordinates (`Terminal_Anchor_Plan_200.csv`). **These are not functional SparkTerminals.**
- 130 player-wire concepts with source/destination model + proposed visual socket anchors; neither wire topology nor terminals are connected to a live Project Spark electrical graph.

## Four interacting simulation domains to implement
1. **Electrical** — 5V control, PLC, real player wiring, monitored disconnects, watchdog I/O.
2. **Digital storage** — 72-bit SECDED codewords carrying 64-bit data; actual syndrome/overall parity, single-bit correction and double-bit detection, scrubber and page quarantine.
3. **Computing/mission** — Three truly distinct deterministic compute lanes, TMR voter, independent reference comparison, mission tasks, watchdog, safe output gate.
4. **Communication** — Level 80 qualified optical link transport, actual packet CRC/sequence checks, retransmission and safety-certified data freshness.

## Safety and fault injection
- Fictional, seeded bit/register event generator **only**. No real radiation source or hazardous test procedure.
- Three disturbances: accumulated uncorrectable memory fault, correlated two-lane compute error, corrupted/replayed telemetry.
- W094 trusted ECC syndrome/heartbeat is a critical supervisory wire: broken/stale feedback must latch mission safe hold and require repair/calibration/reset and fresh full mission run.

## Six objective evidence
See `Objective_Certification_Plan_6.csv`. Real LevelGamePlayManager integration is pending; no objective is currently implemented.

## Art placement
Use `Tools/Project Spark/Levels/81/Place HD Art Under Selected Layout`; keep game objects outside `L81_HD_VISUAL_ART` because re-placing art may replace that root. Use `SparkLevel81VisualRig` exclusively as a **consumer of actual simulation values**; it cannot award certificates.
