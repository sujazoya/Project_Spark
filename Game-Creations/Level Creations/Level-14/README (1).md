# Project Spark - Level 14 LED Protection & Current Limiting

## Contents
- Level_14_LED_Protection_Import.txt: editable serialized Unity import, retaining original Level_14 asset path/GUID and all non-target configuration.
- Level_14_Creation_Guide.pdf: wiring, prefab, binding, validation, and expected-value checklist.

## Crucial requirements before Unity import
The uploaded Level_14 file is a copy of Level 13's loaded voltage divider; this package converts it to the LED challenge.
Target enum values retained from the existing asset: 4=ComponentConducting, 5=VoltageRange, 6=CurrentRange. Objective 1 tests LED component conduction, NOT LED visual On status. Confirm LED1 is a SparkElectricalComponent in your project, or replace objective 1 with a verified supported LED-specific target.

IDs LED1, LED1_Anode, LED1_Cathode, R_LIMIT_Input, R_LIMIT_Output, R_LIMIT_Component, Resistor_470_Input/Output are proposed Level 14 IDs, NOT discovered existing Unity object references. Match these exactly when authoring Level_14_Layout. The power supply keeps the uploaded IDs.

Electrical predictions assume a forward-conducting LED with fixed 2.0 V drop. Actual diode model may produce different values. Verify solver before publishing. If solver does not represent an LED voltage drop, do NOT use these numeric targets unmodified.

## Import
1. Back up original Level_14.asset.
2. Select ORIGINAL Level_14.asset in Unity Project view.
3. Use your existing editable-TXT importer on Level_14_LED_Protection_Import.txt.
4. Save and inspect targets and prefab bindings.
5. Run and audit solver current/voltage.

## Primary runtime declarations
runtimeComponentIds: R_LIMIT_Component
runtimeTerminalIds: R_LIMIT_Input; R_LIMIT_Output

Do not add runtime IDs to fixed scene reference lists.
