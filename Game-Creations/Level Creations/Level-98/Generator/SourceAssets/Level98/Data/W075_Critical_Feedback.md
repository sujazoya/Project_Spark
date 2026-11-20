# W075 — trusted load rail validity

L98_LoadRailSupervisor_U8.TRUSTED_LOAD_RAIL_VALID_OUT -> detachable W075 -> L98_PowerIntegritySafety_U9.TRUSTED_LOAD_RAIL_VALID_IN.

W075 is a logical feedback jumper, never the power rail itself. Its geometry is included as a separately placeable component. Initial game state: uninstalled. Objective 6: after prior normal certification remove W075, reject stale measurements, inhibit load steps, latch fault, physically reconnect, recalibrate, explicitly reset and complete fresh transient qualification.

No supplied file certifies a real pin-level circuit. Implement a verified netlist and safety state machine before enabling objectives.
