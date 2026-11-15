# Level 77 — Reveal the Circuit

In **MainMenu**, select Level 77 after completing Level 76. To install this package in another copy of Project Spark, merge its **Assets** folder into the project and run **Tools → Project Spark → Levels → Create Level 77** in Edit Mode.

## Play sequence

1. Enable **5 V Control**. Use the wiring drawing to connect W001–W114. **Connect W055 last** so **Undo Last Wire** can remove it during the final test. On the breadboard its labels are **BARR SPEC OUT** and **CMP END IN**.
2. Select **Prime Flow**, **Check Drives**, **Qualify Pads**, **Cal Copper** and **Cal Optics**. **Load Wafer → Inspect Incoming**. The first two objectives verify the full wiring and the ready workstation.
3. Run **A · Bulk**. Wait for the carrier to retract, then choose **Transfer B → B · Soft Land**. The third objective verifies bulk copper removal and soft landing. Choose **Transfer C → C · Barrier** and wait for the optical endpoint and carrier retraction.
4. Select **Unload → Surface Protect → Brush → Megasonic → Rinse → Spin Dry → Geometry → Electrical**. Inspect the 16-region map, remaining conductive film, via continuity and isolation to reach 4/6.
5. Use separate wafers for each disturbance. Run to Stage C and select **Viewport Foul** while polishing. Wait for the safety hold, then select **Restore → Recal Optics → Verify Optics → Reset Safety → Unload**. On the next wafer run to Stage C and select **Selectivity Drift**, then **Restore → Reset Safety → Unload**. On another wafer finish Stage C, unload and select **Corrosion** before cleaning. Wait for the hold and select **Restore → Reset Safety**. Process and inspect a fresh wafer to reach 5/6.
6. Start Stage C on a new wafer. When **Arm W055 Test** becomes available, select it to slow the test, then choose **Undo Last Wire**. W055 should become stale and the carrier should retract. Reconnect W055, then **Recal Optics → Verify Optics → Reset Safety → Unload**. Process and inspect one more fresh wafer, then select **Certify**.

## Equipment and measurements

The 62 art modules retain their source proportions, with a uniform fit to the Unity workbench. All 44 motion anchors come from the supplied art kit. The on-board labels use a local scale of 0.20. The terminal CSV supplies full binding IDs where the breadboard displays abbreviations.

The incoming wafer uses a deterministic training preset for material previously plated in Level 76. Level 76's runtime has no persistent wafer data API. The modeled properties include blanket copper, field barrier, patterned-conductor loss through dishing, dielectric loss, particles, moisture, residue and corrosion. The displayed electrical metrology derives from those properties.

Project Spark solves the 5 V rails, E-stop loop and polishing permit. The gameplay controller validates every physical player connection and the process model supplies motion, optical and metrology values. W055 carries the barrier endpoint permission independently of the optical readiness and display routes. Process values are fictional training values.

Unity rejected the encoding of the original modular FBX files. This build uses Unity-compatible exports of the supplied art with its named pivot groups preserved. Original assets and the authoring references are kept separately.

