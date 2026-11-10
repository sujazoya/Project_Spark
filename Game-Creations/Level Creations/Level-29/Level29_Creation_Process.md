# Level 29 — Automatic Emergency Backup

This replaces the earlier dual-brightness proposal. Level 28 required manual SPDT transfer. Level 29 combines diode source isolation, capacitor storage and automatic transfer, then verifies the full failure/recovery sequence.

## Package contents and scope

- Level_29_AUTOMATIC_Backup_Editable.txt: updated definition retaining your Level 29 GUID.
- Runtime/SparkLevel29AutomaticBackup.cs: new level-local sequence observer and transient solve scheduler.
- Editor/SparkLevel29Setup.cs: adds declarations and the observer to a selected Level 29 layout.
- Editor/SparkLevel29DrawingAssetCreator.cs: replacement for the previous Level 29 drawing creator.
- Level29_Automatic_Backup_Wiring.png: exact wiring diagram.
- Level29_Exact_Wiring.csv and Level29_Slot_Bindings.csv: terminal and slot tables.
- Nominal_Calculations.txt and VALIDATION.md: calculation/check results and limits.

No shared solver or level manager is replaced. No existing project file has been edited by generating this package. Existing capacitor/diode solver definitions, smart-slot bridge and drawing UI are prerequisites.

## Circuit and the third diode correction

Three diodes are required here. The two initially proposed source-selection diodes alone would leave a possible reverse-feed path through a directly connected charging resistor. D_CHARGE blocks that path.

MAIN output splits into:

1. D_MAIN anode -> cathode -> R_LED -> LED -> negative.
2. D_CHARGE anode -> cathode -> R_CHARGE -> capacitor positive.

Capacitor positive also connects to D_BACKUP anode. D_BACKUP cathode joins D_MAIN cathode at R_LED input. Capacitor negative goes to battery negative.

MAIN ON: D_MAIN powers the LED; D_CHARGE charges the capacitor. MAIN OFF: D_BACKUP automatically powers the LED from the capacitor. Restore MAIN: normal supply takes over and charging resumes. There is no SPDT or bleed resistor.

## 1. Preserve and replace the old Level 29 content

Exit Play Mode. Back up Level_29.asset and Level_29_Layout.prefab. Remove the old Level 29 brightness-specific drawings from the drawing list (do not remove other levels). Do not import either old Level_29_FIXED_Editable.txt brightness file or the original Level_29_Editable.txt copied from Level 28.

Copy the new Runtime script into a new Level29 folder under your ProjectSpark scripts. Copy the two Editor scripts into an Editor subfolder. If SparkLevel29DrawingAssetCreator.cs already exists, REPLACE that file in place; do not leave two classes with the same name. Preserve the old file's .meta GUID. These files require no changes to shared scripts.

Select your original Level_29.asset and import Level_29_AUTOMATIC_Backup_Editable.txt with your existing editable-SO importer. Verify GUID 2e4bc1d5117b4e744ab6e32d6970c3a9, levelId level_29, six sequence targets, AllTargets, requiredTargetCount 6, Closed Return enabled, short rejection enabled, and Auto Advance off.

## 2. Rebuild the six slots in Level_29_Layout

Remove the old SPDT assembly, SparkSpdtSlotBinding, its two electrical switches and obsolete wires from this layout only. Disable/remove inherited Level 27/28 transient drivers or phase observers from Level 29. Do not run multiple explicit capacitor schedulers on this level.

Keep one fixed 5 V source, main switch, and LED. Keep the existing electrical fixed wire Battery_positive -> SwitchInput. Verify it points to this layout's terminals. All six generic workstations now have two terminals:

| Slot | Part | Component ID | Terminal A ID | Terminal B ID |
|---|---|---|---|---|
| Slot_01 | 1N4007 main diode | D_MAIN_Component | D_MAIN_Anode | D_MAIN_Cathode |
| Slot_02 | 1N4007 charging diode | D_CHARGE_Component | D_CHARGE_Anode | D_CHARGE_Cathode |
| Slot_03 | 220 ohm charge resistor | R_CHARGE_Component | R_CHARGE_Input | R_CHARGE_Output |
| Slot_04 | 10000 uF capacitor | C_STORE_Component | C_STORE_Positive | C_STORE_Negative |
| Slot_05 | 330 ohm LED resistor | R_LED_Component | R_LED_Input | R_LED_Output |
| Slot_06 | 1N4007 backup diode | D_BACKUP_Component | D_BACKUP_Anode | D_BACKUP_Cathode |

These NEW slot numbers supersede BOTH previous Level 28 and brightness-level slot tables. Set each slot Kind = Workstation, matching part Category, Smart Binding enabled, and the table's IDs. Use occupied-at-start if starting with installed parts. Ensure the shelf controller permits these six part types and the new capacitor variant. A slot label does not set the actual electrical value.

Part values:

- All three SparkDiode components: forwardVoltage 0.7 V, normal 1N4007 model, electrically enabled.
- R_CHARGE: 220 ohm, maximum power at least 0.25 W.
- R_LED: 330 ohm, maximum power at least 0.25 W.
- C_STORE: capacitanceFarads = **0.01**, voltageRating = 16. This is **10000 uF**, not 1000 or 4700 uF. Duplicate/create a matching shelf prefab variant if needed; update its displayed name and capacitor display too.
- LED: forwardVoltage 2 V, maximumForwardCurrent 0.02 A, visual assigned. Verify visible emission down to 2 mA.
- Source: 5 V; main switch initially OFF. Keep source enabled during the failure test; turn OFF the MAIN switch only.

Set actual resistor TerminalA to its named Input and TerminalB to Output. Diode anode/cathode assignments and capacitor positive/negative must be correct; the observer checks actual component terminals, not labels alone.

## 3. Declare bindings and attach the observer

Open Level_29_Layout in Prefab Mode. Root SparkLevelLayout must reference Level_29.asset and root must have SparkLevelSceneBindings. Select the ROOT and run:

Tools > Project Spark > Level 29 > Prepare Selected Layout Gates

This adds missing runtime IDs and SparkLevel29AutomaticBackup. It is idempotent and does not wire or populate the slots. Save the prefab. Remove stale SPDT/brightness runtime declarations that are no longer used, and fixed entries referring to deleted components. Keep the existing workstation binding bridge enabled.

Fixed IDs must resolve within the active layout:

- PowerSupply_01 -> actual SparkPowerSupply.
- Power_Switch -> main SparkSwitch.
- led -> actual SparkLED.
- Battery_positive, Battery_negative, SwitchInput, SwitchOutput, LED_Anode, LED_Cathode -> actual terminals.

Six runtime **component** gate IDs are declared automatically:
L29_Wiring, L29_Charged, L29_Isolated, L29_Backup, L29_Restored, L29_Recharged.

Do not assign these gate IDs to physical slot components or static binding entries. The observer owns them. It exposes a gate by binding it to the REAL LED after the corresponding recorded check passes, and clears them on invalid attempts. No fabricated voltage/current is written. The level definition uses LEDOn targets for these gates, so the final LED must also really be on. Without the observer or declarations, the level stays incomplete.

The observer automatically finds the active loader, solver and circuit. Assign the intended instances explicitly if your scene has multiples. Attach exactly one observer to the active layout root, not the persistent manager. Do not call CompleteCurrentLevel from UI events for this lesson.

## 4. Wire 10 player wires plus the fixed wire

| Wire | From | To |
|---|---|---|
| F0 | Battery_positive | SwitchInput |
| W1 | SwitchOutput | D_MAIN_Anode |
| W2 | D_MAIN_Cathode | R_LED_Input |
| W3 | SwitchOutput | D_CHARGE_Anode |
| W4 | D_CHARGE_Cathode | R_CHARGE_Input |
| W5 | R_CHARGE_Output | C_STORE_Positive |
| W6 | C_STORE_Positive | D_BACKUP_Anode |
| W7 | D_BACKUP_Cathode | R_LED_Input |
| W8 | R_LED_Output | LED_Anode |
| W9 | LED_Cathode | Battery_negative |
| W10 | C_STORE_Negative | Battery_negative |

SwitchOutput, C_STORE_Positive, R_LED_Input and Battery_negative each need capacity for at least two external wires. Wire joins through junctions are allowed if their terminals share the same electrical graph node. Do not connect component input directly to output with a wire.

The observer checks seven distinct required wire-nets, preventing diode/resistor/capacitor bypasses between those nets. It verifies the required network, not every unrelated object elsewhere in the scene. Keep extra supplies/loads disconnected from this circuit.

## 5. Solver timing and capacitor state

Enable the existing solver's Simulate Transient Devices. Keep Time Step at 1/60 s (or another positive value <=0.1 s); verify capacitor and diode solver definitions are registered. The observer requests fresh solves on due frames without changing solver settings or electrical history.

Durations are **accepted simulated seconds**, accumulated from each successful solve's TimeStep. They are not guaranteed wall-clock seconds on a slow computer or when other systems request extra solves. Do not claim frame-rate-independent real-time timing. Unity timeScale 0 pauses observer scheduling and progression. A backup sampling gap above 0.25 game-time seconds resets the attempt. A failed solve resets it too.

Use the existing level restart to reset capacitor history for a fresh run. The observer deliberately does not inject a charge or discharge value. If starting fully charged without having observed a 0.30 V rise, discharge using the actual circuit or restart the layout; do not bypass the test with preloaded state.

## 6. Test the required sequence

1. **Wiring validated:** required parts active, correct values, healthy, required nets present and separated. Gate 1.
2. **Charge:** MAIN ON, LED on through D_MAIN, backup path essentially idle. Observe capacitor rise >=0.30 V and reach >=4.10 V. Gate 2. Follow the status prompt before switching OFF.
3. **Automatic isolation:** MAIN OFF, LED supplied through D_BACKUP. Source, D_MAIN, D_CHARGE and charge-resistor current magnitudes each <=0.10 mA. LED current 2–9 mA. Gate 3.
4. **Sustained backup:** keep MAIN OFF; LED stays in range for 2 accepted simulated seconds, capacitor loses >=0.20 V. Gate 4. Follow BACKUP PASSED prompt.
5. **Restore:** MAIN ON, D_MAIN powers LED, D_BACKUP becomes idle, positive charge-resistor current >0.20 mA. Gate 5.
6. **Recover:** capacitor rises >=0.15 V from the restoration sample and reaches 4.10 V. Keep normal operation for 0.5 accepted simulated seconds. Gate 6; existing manager awards completion.

Early restoration, loss of required current during backup, failed solves, wiring changes, installed-part replacements or electrical configuration changes clear the attempt. Switching MAIN is allowed and expected. Earlier gate indicators still depend on the live LED, so a dark LED may temporarily reduce the displayed target count even though recorded history exists. The observer status is the authoritative phase prompt.

The default small status overlay appears near the bottom. Disable Show Status only after connecting another UI to the public Status and Stage properties; otherwise the player loses phase instructions.

## 7. Drawing and progression

Run Tools > Project Spark > Visual Teaching > Create Level 29 Drawing Asset. Save a NEW Automatic Backup drawing asset. Replace the old level_29 entry in SparkDrawCurrentLevel.Drawings with this asset (one drawing per level ID). Keep other levels intact. Register Level_29.asset and Level_29_Layout in your existing progression after Level 28.

The four-page drawing is a labelled functional map with an explanatory caption; the PNG and CSV are the authoritative terminal-level wiring guide. Drawing lines never create gameplay connections.

## 8. Nominal electrical expectations

Assuming 0.7 V diode drops and a 2 V LED:

- Normal load rail: approximately 4.3 V; LED current approximately 6.97 mA.
- Charge asymptote: approximately 4.3 V; RC time constant 220 * 0.01 = 2.2 s.
- Initial charge current: approximately 19.5 mA; resistor power approximately 84 mW.
- Reach 4.10 V from zero: approximately 6.8 simulated seconds.
- After 2 seconds of backup starting at 4.10 V: capacitor approximately 3.47 V, LED current approximately 2.32 mA.

These estimates omit small on-resistances. Actual Unity nonlinear solver acceptance must be tested. The script does not fake results when the solver cannot support an operating state.

## 9. Acceptance checks

- Correct sequence completes only after recovery; initial LED illumination never completes.
- Turn MAIN OFF before charge threshold: no transfer credit.
- Restore MAIN before 2 seconds: attempt resets.
- Reverse D_BACKUP: cannot sustain backup.
- Bypass D_MAIN or D_CHARGE: required-net separation fails.
- Remove capacitor: unresolved part prevents progress.
- Install 1000 or 4700 uF: value check prevents progress; install 10000 uF.
- Use wrong resistor: value check prevents progress.
- Reverse capacitor/LED: topology or electrical-state checks prevent progress.
- Remove the return or short any required nets: no completion.
- Pause during backup: paused time does not count.
- Failed solver result: no stale-success progression.
- Replace a component or change wiring mid-test: recorded sequence resets.
- Stop/restart level: no retained gate bindings.
- Disable observer: its OnDisable clears gates; static targets alone cannot complete.
- Verify final success uses the existing level-completion/unlock path.

## Remaining validation boundary

The code compiles against the installed project assemblies and nominal RC calculations support the default thresholds. No Unity Play Mode run or prefab import was performed in this delivery. Existing real-project transient solving, graph registration, slot insertion and UI must pass the tests above before publishing. No additional shared solver changes are prescribed.
