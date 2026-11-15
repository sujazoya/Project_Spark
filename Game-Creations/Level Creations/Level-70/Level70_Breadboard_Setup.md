# Level 70 — Guard the Air

Open MainMenu and select Level 70 after completing Level 69. To install in another checkout, merge the archive's Assets folder into Project Spark and run **Tools → Project Spark → Levels → Create Level 70** in Edit Mode.

## Play sequence

1. Turn on **5 V Control** and connect all 86 wires. Connect **W86 last** for the pressure-feedback test.
2. Calibrate pressure, airflow, particles, temperature and humidity. Start the supply, return and exhaust blowers and enable FFUs A and B.
3. Select **Tuned** on both pressure and airflow gains and enable **Auto Env**. The nominal targets are **+15 Pa** in the cleanroom, **+8 Pa** in the airlock, **22°C** and **45% RH**.
4. Wait for stable pressure and three clean particle samples. Run two **Wafer Tests** and verify accepted exposure.
5. Run **Airlock Test**. Watch the outer-door pressure drop, the particle pulse, and the air shower. Wait for recovery and then apply **Filter Load**. Select **Confirm Load** after measured HEPA A pressure drop rises. Apply **HEPA Leak** and observe the contamination hold.
6. Select **Request Service**, then **Isolate A**. Wait for branch flow to fall and FFU A to stop. Replace HEPA A and select **Verify Seal**. Let the room purge before choosing **Reset Safety**.
7. Select **W86 Test** and use **Undo Last Wire** to remove **ROOM_DP_OUT → CASCADE_DP_IN**. The reading goes invalid, the robot permit drops, and fallback ventilation remains active. Reconnect W86, select **Recal Room DP**, wait for fresh feedback, then reset safety and re-enable Auto Env.
8. Run a fresh airlock test and wafer inspection. Select **Certify** once both pass.

## Workbench checks

- The status line reports the first missing circuit, the current service step, or the active fault.
- A wafer cycle requires a sustained safe pressure and particle window and closed doors. A loss of any permit stops robot motion.
- A filter change requires measured low branch flow and a stopped FFU A. The seal test restores branch flow and checks the new pressure drop.
- The main gauge displays INVALID if W86 is open. The room still exchanges air through the modeled fan, return, exhaust and leakage paths.

## Simulation and assembly

All 48 supplied equipment models use a common workbench scale. The ceiling panels are translucent to keep the wafer robot visible. The supplied 19 animation references are bound to the level. The two second FFU impellers also have separate pivots.

The reduced-order air network uses volumetric flow, signed leakage flow, and pneumatic compliance in the cleanroom and airlock. The pressure outer loop requests airflow and the inner loop controls fan speed. Filter resistance and penetration affect airflow and transported particle concentration. The two counters integrate air samples over finite windows. Wafer inspection integrates exposure over the physical robot cycle.

Air temperature and water content evolve separately. Cooling and condensation, finite reheat, humidification and drainage determine the air state. The dashboard calculates RH and dew point from that state. The cleanliness thresholds are educational gameplay limits.

Project Spark solves the 5 V module rails, the E-Stop circuit and the robot-enable path. The remaining environmental channels are validated through the real wire graph and read from the level's model.
