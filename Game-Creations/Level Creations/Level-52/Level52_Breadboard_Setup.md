# Level 52 — Cancel the Shake

Merge the package's Assets folder into Project Spark with its metadata. Open MainMenu in Edit Mode and run **Tools → Project Spark → Levels → Create Level 52**. The creator fits the supplied art to the breadboard, validates the terminal references, creates the teaching drawing and registers the level. If MainMenu already has unsaved edits, review and save it to persist its registration.

## Play all six objectives

1. Connect W01–W19 with the Wire tool. The supply sockets each take four wires. Connect **W19 last** so Undo Last Wire can remove only the encoder index.
2. Press **START** at 45 RPM. Leave the masses opposite each other, wait for five valid revolutions and press **RECORD BASELINE**.
3. Press **RUN TRIAL**. Wait for both masses to settle and the trial measurement to finish, then press **CALCULATE**.
4. Switch to **AUTO**, press **AUTO BALANCE**, then press **VERIFY BALANCE** once both masses settle. The residual must be at most 30% of baseline.
5. Select **60 RPM** while running and verify again after the speed stabilizes. The level checks at least four complete revolutions.
6. Press **PHASE FAULT TEST**, use **Undo Last Wire**, then wait for safe stop. Reconnect W19 from ENCODER Z to Z IN. Press **RESET FAULT**, **START**, and **VERIFY BALANCE** after the index returns.

## Measurement and setup

The shared solver supplies motor voltage and current and verifies the guard and power rails. A level-local averaged drive varies the series impedance, and the solved motor current supplies the rotor model. The ring uses a separate solved load and moves only while powered, with valid safety and encoder feedback. A/B decode 80 counts per turn, and Z anchors the angle once per revolution. The vibration model projects the unbalance and correction vectors onto the rotating sensor axis. The controller derives the complex influence from a known trial and its measured change in vibration, then solves the two equal-mass angles. The dashboard plots the last 2.4 seconds of simulated vibration and shows baseline and residual vectors alongside the two correction masses. The measured vibration and mass vectors use separate full scales on the polar plot.

The editor creator assigns the motion references. Both mass pivots are children of the common ring pivot and each rotates independently in rotor coordinates. See the included terminal and animation manifests for the source model mapping.

| Symptom | Check |
|---|---|
| Start is blocked | Follow the first missing wire shown. Check all four supply pairs and guard W15. |
| Baseline or trial is unavailable | Wait for stable speed and valid Z revolutions. Check W12–W14 and W19. |
| Correction masses stay still | Check W16–W18 and confirm the guard is closed. |
| VERIFY is unavailable | Wait for both masses and speed to settle, then check that the correct speed is selected. |
| Reset is blocked after index loss | Restore W19, close the guard and wait for the rotor to stop. |

The included Play Mode report lists the checks run in Unity.
