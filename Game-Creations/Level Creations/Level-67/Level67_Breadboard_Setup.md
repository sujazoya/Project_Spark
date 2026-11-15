# Level 67 — Master the Flow

Open MainMenu and select Level 67 after finishing Level 66. In another checkout, merge the package's Assets folder into Project Spark, then use **Tools → Project Spark → Levels → Create Level 67** in Edit Mode. This registers the layout, definition and teaching drawing.

## Play sequence

1. Turn on **5 V Control** and connect all 74 wires using the exact wiring sheet. Connect **W74 last** so **Undo Last Wire** can disconnect Flow B during the fault test.
2. **Calibrate**, then **Prime A**, **Prime B**, **Start A**, **Start B** and enable **Mixer**. Prime the transfer pump once the mix vessel holds enough liquid.
3. **Start Transfer**, enable **Cascade**, select **Tuned** for **Level Gains** and **Flow Gains**, and open **Product Outlet**. The default setpoints are 3 L in the finished tank and 20 L/min total flow.
4. Enable **Ratio Control** and **Production** at **70% A**. Wait for actual on-spec product to reach the finished tank.
5. Run **Demand Surge**, **Pressure Loss**, then **Pump B Failure**. Watch the measured response each time. When Pump B has stopped and release is isolated, use **Reset Fault** and restart A, B and transfer.
6. Select **W74 Test** and use **Undo Last Wire** to remove **FLOW_B_OUT → RATIO_B_IN**. Verify invalid Flow B and drive isolation. Reconnect W74, wait for the pumps to stop, choose **Recal Flow B**, wait for valid feedback and **Reset Fault**. Restart all three pumps, repeat **Demand Surge** and **Certify**.

## Controls and troubleshooting

- Use the workbench wiring sheet to locate each terminal. The first missing wire appears on the control panel.
- A rail check requires approximately 5 V on each powered module and a closed emergency stop circuit.
- When no fluid is moving, confirm that the tanks have inventory, each pump is primed and enabled, and the transfer pump has adequate suction.
- During commissioning the plant starts with an initial wet inventory. In production, release requires measured flow from both sources, a running mixer and on-spec liquid. Monitor both volume and blend rather than setpoints alone.
- If a disturbance fails its check, let the trend settle before retrying. With the emergency stop latched, release the stop and wait for the pumps to stop before resetting.
- Flow measurements are in L/min, tank readings are in L and pressure is in kPa. The process trend covers recent simulated time.

## Process configuration

The hydraulic plant advances at 0.05 s model steps. A, B, mix and product capacities are 58, 42, 8 and 8 L. Their starting volumes are 41, 29, 0.80 and 2.95 L. The mix and product start with explicitly documented 70/30 wet inventory. Supplier volume falls whenever material transfers, and downstream withdrawal is tracked as an inventory sink. A and B are modeled as equal-density liquids. Blend fraction comes from constituent inventories.

The three pumps use bounded 1,800 RPM shaft dynamics, head curves and valve-dependent branch losses. Tank head and available suction affect flow and cavitation protection. Flow/level readings use a filtered sample. The outer PID turns product level error into a bounded transfer flow request, and the inner PID commands transfer speed. Both loop gain controls provide low, tuned and high profiles with filtered derivative and integral clamping. Recipe control divides the inlet request and corrects from the measured A/B flows.

The existing Project Spark circuit supplies the 5 V rails, electrical return, emergency stop and switched drive permit. The remaining process signals use validated wire topology and the level-local process model. Pump currents displayed by that model are simulated drive loads. This is a lumped process training simulation.

## Installation evidence

The wiring CSV and terminal bindings give the wiring list and fan-out. The component binding sheet documents the fixed contacts and runtime targets. The archive includes the original art kit, the fitted models, animation reference, teaching asset and Play Mode results.
