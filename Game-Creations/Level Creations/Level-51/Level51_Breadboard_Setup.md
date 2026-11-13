# Level 51 — Predict the Failure

## Install

Merge the package's Assets folder into Project Spark. Open MainMenu in Edit Mode and run **Tools → Project Spark → Levels → Create Level 51**. The creator fits the supplied art, validates the sockets, generates four teaching pages and registers the level. If MainMenu already has unsaved changes, review and save it to persist the registration.

The layout root contains the diagnostic controller, scene bindings, supplied visual rig and control panel. The breadboard holds the 15 fitted modules. The 32 terminals are fixed bindings. The rotor, shaft, encoder, eccentric weight, guard and service cartridge have dedicated visual references. The eccentric weight is parented to the rotor.

## Play the six objectives

1. Connect W01–W19. The four powered modules share the two supply sockets. Place W06 and W07 in series with the motor and connect both Kelvin sense leads W15 and W16.
2. Press **START** at 45 RPM and wait for stable samples. Press **BASELINE**.
3. Stop, wait for the rotor, install **IMBALANCE TEST**, and restart. Select **ROTOR IMBALANCE** using the diagnosis button and press **ANALYZE**. Look for the 1× shaft-frequency vibration.
4. Stop, install **BEARING TEST**, and restart. Select **BEARING WEAR** and press **ANALYZE** once irregular vibration, load current and heating are present.
5. Stop, load **WEAR TEST**, and restart. Watch the temperature trend and predicted thermal limit. When **MAINTENANCE REQUIRED** appears, use **PREVENTIVE STOP** and wait for the shaft to stop.
6. Open the guard. Remove the bearing and then install the healthy spare using the same service control. Correct the balance weight, close the guard and press **RESET FAULT**. Start at 45 RPM and press **VERIFY HEALTH** after the sample window fills.

## Measurement model

- The normal solver calculates the series circuit and the 0.22 Ω shunt. The controller uses the solved difference across the Kelvin inputs divided by the shunt resistance.
- A level-local averaged drive adjusts series impedance to control solved motor current. Shaft torque and drag drive the rotor model. Encoder A/B are routed through the wired graph at 80 decoded counts per turn.
- A 50 Hz, four-second sample window measures RMS vibration, the shaft-synchronous component and the remaining irregular component. Bearing friction increases load and heat, while the eccentric test weight increases shaft-synchronous vibration. Temperature uses a first-order heating and cooling model.
- Thermal prediction divides the remaining margin to the 61 °C modeled limit by a positive measured trend. At critical wear or temperature, the protective stop cannot earn the preventive objective. After cooling below 38 °C, load the wear test again.

| Symptom | Check |
|---|---|
| Start is blocked | Follow the displayed W-number and close the guard. W19 carries solved safety feedback. |
| Shunt reads zero | Check W06–W08 and the separate W15/W16 sense leads. |
| Analyze is unavailable | Wait for the four-second history to fill and speed to settle. |
| Diagnosis is rejected | Compare 1× vibration, irregular vibration, current and temperature trend. |
| Reset is blocked | Install the healthy bearing, correct the balance, close the guard and let the bearing cool. |

See the Play Mode report for the verified test sequence.
