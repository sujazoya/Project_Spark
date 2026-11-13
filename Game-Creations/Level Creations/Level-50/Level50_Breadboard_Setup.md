# Level 50 — Zero Defect Factory

## Install and inspect

Merge the package's Assets folder into Project Spark with its Unity metadata. Open MainMenu in Edit Mode and run **Tools → Project Spark → Levels → Create Level 50**. The creator installs the layout, definition and four teaching pages, validates the bindings and adds the campaign entries. If MainMenu has pending edits, review and save the scene to persist its registration.

The factory root contains the scene bindings, level controller, visual rig and controls. The breadboard holds 16 fitted model modules, 38 player sockets and the mechanical pivots. The blade pivot is parented to the moving carriage. The creator assigns the references automatically. The terminal and animation manifests map the supplied art to the playable assembly.

## Play the six objectives

1. Connect W01–W22 with the normal Wire tool. The +5 V and return sockets each support four wires. Check the safety input on W22.
2. Press **HOME** and wait for the diverter contact and cutter home position. Press **CALIBRATE**. The line measures two genuine optical marks at 18 RPM. Wait for the web to stop.
3. Press **START** in AUTO at 30 RPM. Follow the product IDs through printing, cutting, inspection and delivery. The GOOD bin must confirm three consecutive good products.
4. Select **60 RPM** and **MIXED BATCH**. The test assigns specific print defects to specific products. Verify at least two confirmed GOOD and two confirmed REJECT deliveries.
5. With multiple products in flight, run **SPEED TEST** and **FALSE MARK**. Wait for the speed dip and recovery, one rejected false pulse and two subsequent good deliveries.
6. While running, press **EMERGENCY STOP**. Restore safety after the line stops, explicitly reset, home and calibrate again. Press START and deliver four new GOOD products.

## Simulation and inspection

The source, transport and diverter motor windings, safety contact and home contact use the existing electrical solver. Encoder phases, optical events, station triggers, inspection and collection pulses are level-local signals carried over the validated wire topology. The diverter takes time to travel and uses the solved motor current on its powered stroke.

The measuring roller is 20 mm in radius and has 80 decoded counts per revolution. Genuine marks are 160 counts apart. Print, cut, inspection and sorting are at offsets of 80, 160, 240 and 320 counts. Printing predicts the 75 ms actuator stroke from web velocity. The cutting carriage has a 25 mm tracking window and a 6 mm/s synchronization tolerance. Inspection checks the simulated print and cut records against ±2 mm limits. The camera and indicators display those results.

| Symptom | Check |
|---|---|
| 0/6 | Follow the first displayed W-number. Check all four 5 V branches and W22. |
| HOME unavailable | Close the safety circuit and wait for the machine to stop. Check W19. |
| Mixed batch stays pending | Select 60 RPM and wait for the products to reach their bins. Inspect W14–W21 if the controller reports a sorting or feedback fault. |
| 4/6 stays visible | Trigger both tests. Wait for confirmed GOOD deliveries after the speed recovery. |
| 5/6 after reset | Home, recalibrate and press START. Wait for four new GOOD deliveries. |

The Play Mode report records the checks that ran in Unity. The optional verification script belongs in the project's Editor folder if replaying the test.
