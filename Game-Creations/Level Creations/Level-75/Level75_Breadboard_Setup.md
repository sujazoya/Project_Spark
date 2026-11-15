# Level 75 — Write the Silicon

Open **MainMenu** and select Level 75 after completing Level 74. In another Project Spark checkout, merge the package's **Assets** folder into the project and run **Tools → Project Spark → Levels → Create Level 75** in Edit Mode. The art-only placement, material and validation tools are under **Tools → Project Spark → Levels → 75**.

## Play sequence

1. Use the exact wiring drawing for W001–W106. **Skip W067 until the other 105 wires are installed.** W067 connects **FARADAY_CUP_CURRENT_OUT → ION_DOSE_CONTROLLER_BEAM_CURRENT_IN**. Placing it last makes it the wire removed by **Undo Last Wire**. Turn on **5 V Control**.
2. **Cal Vacuum**, start the backing pump, then start the turbo pump. When vacuum permits, **Load Wafer**, **Verify Mask** and **Clamp Wafer**. Enable **Wafer Cool** and **Neutralize**.
3. Enable **Virtual Source**, then select **Cal Species → Tune Analyzer → Select Energy → Focus / Steer → Profile Beam → Cal Faraday**. Qualification waits for measured current, purity, beam width and cup validity.
4. Select **Implant 16**. The controller integrates measured Faraday current across the raster. Wait for the gate to close, then **Unload → Activate → Depth Metro → Sheet Metro**. The wafer must pass dose, uniformity, mask leakage, junction depth and sheet resistance.
5. Load another wafer. Trigger **Analyzer Drift** and observe the purity decrease. **Restore → Reset Safety → Profile Beam**. Run **Focus Fault**, restore, reset and profile again. Start implanting and select **Charge Fault** once useful beam is present. Restore neutralization and reset. Discard the interrupted wafer, then load, requalify, implant and activate a fresh wafer and pass both inspections.
6. At 5/6, load another test wafer and start implanting. Select **W067 Test** during the raster. Use **Undo Last Wire**, then observe the Faraday measurement go stale and the physical blanker close. Reconnect W067, choose **Recal Cup**, wait for **Verify Dose**, and select **Reset Safety**. Discard the interrupted wafer, load and requalify a fresh wafer, then complete implantation, activation and both metrology checks. Select **Certify**.

## Workbench

The 58 supplied equipment models retain their proportions under a common fit. The assembled reference and source assets remain in the original kit. The 38 authored pivot groups are bound to the runtime visual rig. The dose map shows all 16 regions and highlights the active raster region.

Project Spark's electrical solver handles five 5 V control supply and return branches, the emergency-stop loop and the beam gate permit. Other process signals use the validated wire graph and the level's fictional process model. Vacuum, species selection, charge, wafer temperature, depth and activation have separate evolving states. This game uses normalized training values.
