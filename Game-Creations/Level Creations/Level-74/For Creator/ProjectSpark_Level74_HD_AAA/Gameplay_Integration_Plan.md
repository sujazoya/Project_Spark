# LEVEL 74 — UNITY GAMEPLAY INTEGRATION PLAN

**Art only; this document is not a gameplay implementation.**

Level: `level_74` — Align the Light.
Gameplay target: **102** player-created wires; **158** unique `SparkTerminal` anchors; **6** authoritative objectives.

## Six objectives
1. Build lithography control network (102 actual wires, 158 terminals, safety and PLC connectivity).
2. Spin-coat fictional photoresist, measure coating uniformity and complete controlled soft bake.
3. Align wafer and reticle, verify 4:1 reduction scan trajectory, focus map and interferometer position.
4. Expose 16 wafer fields, post-bake, develop, rinse/dry and certify CD, overlay and pattern defects.
5. Recover from resist nozzle restriction, active vibration isolation disturbance and lens thermal drift.
6. Disconnect real interferometer signal wire -> invalidate measured position -> close exposure shutter and stop unsafe scan -> reconnect, recalibrate, explicit reset, fresh 16-field exposure and verified metrology.

## Physics/logic not in this package
- Real servo stages: X/Y/Z, acceleration, jerk, following error, reticle/wafer 4:1 trajectory.
- Exposure: spatial photon-dose integration, virtual reticle patterns, defocus, latent resist image.
- Fictional photoresist: coating, bake kinetics, development, rinse/dry and defects.
- Sensor states: actual position, sampled interferometer position with validity and timestamps.
- Safety: enclosure and shutter interlock, motion fault, source inhibit.
- Authoritative LevelGamePlayManager must certify 6/6; historical 1–5 persist through final deliberate fault.

## Integration discipline
Inspect actual Unity Project Spark types and APIs before implementing components. Do not fabricate existing methods.
No gameplay SparkTerminal objects, circuit topology or physics were generated in these FBX/GLB models.
102-wire budget is provided in Gameplay_Wiring_Budget.csv only; W01–W102 is NOT implemented or positioned yet.
