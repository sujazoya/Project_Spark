# Level 69 — Keep It Cold

Open MainMenu and select Level 69 after completing Level 68. In another checkout, merge the package's Assets folder into Project Spark and run **Tools → Project Spark → Levels → Create Level 69** in Edit Mode.

## Play sequence

1. Turn on **5 V Control** and connect all 82 wires. Connect **W82 last** so the Wire tool can remove it during the final recovery test.
2. Run **Safety Check**, **Cal Pressure**, **Cal Temp**, and **Cal Mass Flow**. Enable the condenser fan group, the evaporator fans, and both EEVs. Start the compressor. Watch the pressure split, mass flow, and live pressure–enthalpy plot.
3. Calibrate **Superheat A** and **Superheat B** while refrigerant is flowing. Set both gain controls to **Tuned**, keep Chamber A at **4°C** and Chamber B at **2°C**, then enable **Dual-Zone Auto**. Wait for air and cargo temperatures to settle.
4. Apply **Frost Test** to Evaporator B. Use **Request Defrost** once the frost threshold is reached. The controller closes EEV B and stops its fans before applying heat. Wait through the drip delay and allow Chamber B to recover.
5. Run **Door Test** and wait for stable cooling. Once nominal COP is recorded, apply **Fan Fault**. Observe high-side pressure trip the compressor. Wait for pressure and shaft speed to fall, select **Reset**, restart the compressor, and select **Optimize** after the two zones settle.
6. Select **W82 Test** and use **Undo Last Wire** to remove **EVAP_A_TEMP_OUT → SH_A_TEMP_IN**. Confirm invalid superheat and a de-energized compressor permit. Reconnect the wire. After the shaft stops, select **Recal Evap A**, wait for valid feedback and select **Reset**. Restart, repeat **Door Test**, and select **Certify**.

## Check the workbench

- The status line names the first missing connection or the active protection latch.
- An invalid Evaporator A signal interrupts the optimization and closes both EEV commands. Fresh evidence is required to regain the final target.
- Frost g, drain g, pressure in absolute kPa, mass flow in g/s, RPM and kW are from the running model. The pressure–enthalpy plot clears when its required feedback is unavailable.
- The measured COP compares integrated useful cooling and electrical energy over verified windows. Allow both zones to settle before running the next trial.

## Model and assembly

The simulation uses a fictional educational fluid, PS-69, with one saturation curve and consistent liquid, vapor, and two-phase enthalpy relationships. The compressor transports vapor mass and raises its enthalpy. The condenser rejects heat through a finite thermal store. EEV throttling is isenthalpic, and the chamber models track separate air and cargo temperatures. The mass check covers the receiver, low and high vapor spaces, and accumulator. The reported cycle closure is for the four plotted state points. The model uses lumped evaporators and a receiver mass balance. Pressure drops, oil return, frost and heat transfer are reduced-order teaching approximations.

The supplied 46 models are uniformly fitted to the workbench, with the lower chamber moved forward for visibility. The original models, rig references and kit preview are included. The Project Spark solver powers the control rails and the normally closed E-Stop and compressor permit paths. The rest of the instrumentation is validated through real gameplay connections and read from the simulation.
