# Level 68 — Tame the Heat

Open MainMenu and select Level 68 after completing Level 67. For another checkout, merge the package's Assets folder into Project Spark and run **Tools → Project Spark → Levels → Create Level 68** in Edit Mode. This registers the level, layout and teaching drawing.

## Play sequence

1. Turn on **5 V Control** and connect all 78 wires. Connect **W78 last** for the outlet RTD test.
2. Select **Cal Temp**, **Cal Flow** and **Cal Pressure**. Start the hot-water, cooling-water and product pumps. Wait for circulation, then select **Enable Heater**. Confirm that hot-water temperature rises and heat reaches the product.
3. Select **Tuned** on both gain controls, keep the outlet target at **65°C**, and enable **Cascade**. The outer loop requests hot-water temperature. The inner loop commands heater power. Wait for stable product temperature and the matched baseline.
4. Select **Regeneration**, toggle **Bypass** closed, and select **Product Transfer**. The hot product warms the incoming product across the regenerative exchanger. Watch preheating reduce the trim exchanger's load.
5. Run **Cold Inlet**, then **Flow Surge**. Wait for recovery after each. Select **Fouling Test** and compare differential pressure and exchanger effectiveness before choosing **Confirm Fouling**.
6. Select **W78 Test**, then **Undo Last Wire** to remove **OUTLET_RTD_OUT → PID_PRODUCT_TEMP_IN**. Watch the outlet reading report INVALID and the heater permit open. Reconnect W78, wait for the pumps to stop, choose **Recal Outlet RTD**, wait for a fresh reading and **Reset Fault**. Restart all three pumps, re-enable heat and cascade, repeat **Cold Inlet**, and **Certify**.

## Troubleshoot the bench

- Check the first missing wire shown on the control panel. The main network requires roughly 5 V at the control rails and the normally closed stop contact.
- The heater interlock requires validated temperature feedback and measured circulation on all three loops. An invalid outlet RTD drops the electrically solved heater-enable path.
- The safety latch keeps the plant isolated until circulation stops, valid feedback returns and temperatures fall within reset limits. Release E-Stop before resetting.
- The outlet RTD measures the heated product before the regenerative and final cooling stages. Only valid, in-band product that meets the discharge limit can enter the finished-product total.
- If a disturbance is inconclusive, allow the temperature and utility trend to stabilize before reapplying it.

## Model and comparison

The level uses a lumped water-process model with a 0.05 s fixed step. The heater tank holds 5.5 L, the cooling reservoir 6 L, the thermal buffer 7 L, and the product holding chamber 1.15 L. The one-way product feed starts with 180 L. Hot and cooling loop inventories recirculate. Product withdrawn from feed is accounted for as accepted or diverted volume.

The heater is capped at 62 kW in the simulation. Each driven pump responds through finite shaft inertia and valve travel. Heat exchangers apply an effectiveness model and subtract what they transfer from the donating stream. The regenerative plate pack and trim wall store heat with capacities of 1.20 and 1.55 kJ/K respectively. The energy check accounts for heater energy, product inlet and discharge enthalpy, cooling rejection, standing losses and changes in stored heat.

The regeneration comparison uses the same outlet target and closely matched flow. It adjusts utility duty for hot-loop and buffer energy changes and requires measured heater power to settle near that duty. The dashboard computes the percentage from the matched run.

Project Spark solves the control supply, the stop contact, and the switched **SAFE_HEAT → HEAT_EN** permit. The remaining instrumentation channels are validated by the gameplay wire graph and driven by the thermal model. Pump currents and high-power heater energy are simulated process values.
