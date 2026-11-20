# Critical W083 — application heartbeat

**U1.APPLICATION_HEARTBEAT_OUT → detachable W083 jumper → U8.TRUSTED_HEARTBEAT_IN**. This is a *conceptual functional* link; assign verified package contacts after completing the schematic.

Start with W083 uninstalled, install for first commissioning. During final supervised test disconnect it, observe trusted heartbeat invalidation and independent safety output inhibition. Reconnect, obtain multiple fresh valid transitions (not merely steady HIGH), explicitly reset latched protection and finish a new complete boot and diagnostics qualification. Do not alter the actual processor power rail or invent false clock errors.
