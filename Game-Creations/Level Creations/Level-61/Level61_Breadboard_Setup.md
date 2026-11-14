# Level 61 — Clean the Wave

In another checkout, merge the package's Assets directory into the Unity project. Open MainMenu, use **Tools → Project Spark → Levels → Create Level 61**, then save the scene. The package includes the original art kit, preview and placement reference under Reference.

1. Connect W01–W50 from the wiring guide. Install W50 last so Undo Last Wire can remove it during the sensor test.
2. Select **Main On**, **Calibrate**, **Nonlinear Loads**, and **Lock PLL**. Record the THD baseline when the PLL locks. Amber shows load current and cyan shows source current. The spectrum columns show the 5th, 7th, 11th and 13th orders from left to right.
3. Enable the filter after the DC link charges. Cycle gain to 97% and wait for THD below 5%.
4. Select two capacitor stages and wait for displacement power factor to reach 0.98. Apply the industrial load step and wait for the filter to settle.
5. Select **W50 Test** and remove **FILTER CT OUT → APF CT IN**. The filter opens while the main contactor remains closed.
6. Reconnect W50, reset the fault, recalibrate and lock the PLL. Restart the filter and certify after THD and displacement PF recover.

All 30 art modules retain their proportions under a uniform workbench fit. TextMesh labels use a 0.20 local scale. The 5 V rails and player wiring are solved and registered in Project Spark. The educational model calculates RMS harmonic sums, continuous compensation current, DC-link energy, temperature and both power factors. Waveforms and spectra are reconstructed from the modeled harmonic components.
