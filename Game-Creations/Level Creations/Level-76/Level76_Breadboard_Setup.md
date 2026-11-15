# Level 76 — Forge the Connections

Open **MainMenu** and select Level 76 after completing Level 75. To install in another Project Spark checkout, merge the package's **Assets** folder into Unity and run **Tools → Project Spark → Levels → Create Level 76** in Edit Mode. The art-only placement, material and validation commands are under **Tools → Project Spark → Levels → 76**.

## Play sequence

1. Use the exact wiring drawing for W001–W110. **Connect W076 last** even though it is numbered in the middle of the list. It connects **REFERENCE_ELECTRODE_POTENTIAL_OUT → ELECTROCHEMICAL_CONTROLLER_REFERENCE_FEEDBACK_IN**. It must be the wire removed by **Undo Last Wire** during the final test. Enable **5 V Control**.
2. **Cal Flow**, then enable **Recirculate**, **Filter**, **Degas**, **Condition Bath** and **Add Additives**. **Load Wafer**, **Inspect Seed**, **Prewet** and **Clamp Wafer**. Wait until flow, bath temperature and wetting reach their operating windows.
3. Select **Cal Current**, **Cal Reference**, **Pulse Mode** and **Qualify Zones**. Select **Plate** and watch the copper-fill map. The current drives copper deposition. The controller stops the rectifier when fill and charge reach target.
4. After current runs down, **Unload → Rinse → Spin Dry → Fill Metro → Electrical**. Inspection checks fill, voids, thickness nonuniformity, overburden and isolated test-structure resistance.
5. On separate sacrificial wafers, start plating and run **Contact Fault**, **Flow Restrict**, then **Additive Drift**. For each, wait for the protection hold, select **Restore**, wait for safe conditions, select **Reset Safety**, and discard the interrupted wafer. Requalify the zones before resuming. Plate and inspect a fresh wafer to reach 5/6.
6. Start plating a test wafer and select **W076 Test** when live current is present. Use **Undo Last Wire**. The reference becomes stale, the rectifier permit drops and physical current decays. W084 remains connected as a status route. Reconnect W076, select **Recal Ref → Verify Ref → Reset Safety**, discard the interrupted wafer, then prepare, plate and inspect a fresh wafer. Select **Certify**.

## Workbench and process

The level starts with a prepared wafer that already has dielectric features, a barrier and a conductive seed layer. The final metrology checks a plated wafer before copper CMP and uses isolated electrical test structures. All process values are fictional training values.

All 60 supplied models retain their proportions under a common fit. The runtime visual rig uses the supplied motion API and binds all 42 authored pivots. The map tracks copper fill and voids over 16 regions.

Project Spark solves the five 5 V supply and return branches, the E-stop loop and the rectifier permit. Remaining process signals use the validated wire graph and the level's normalized electrochemical model.
