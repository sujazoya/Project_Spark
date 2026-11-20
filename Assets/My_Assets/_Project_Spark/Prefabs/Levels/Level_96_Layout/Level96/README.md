# Level 103 — Sensor Matrix

Source kit: Level-96/For Creator. Campaign identity: level_103. Educational schematic revision: SMX4-EDU-1, authored with explicit user authorization. Generic IC pin roles are educational definitions, not manufacturer-compatible pinouts.

## Play
Open MainMenu and select Sensor Matrix / Level 103. Open Current Level Controls through the wall Master panel. Start with the bare board. FIT PART, FLUX and SOLDER each named footprint; SOLDER selects the next unfinished part. ROTATE checks pin-1 orientation. Keep bench power off during assembly and copper work.

Press WIRE MODE to activate the shared Wire tool; INSPECT MODE returns to physical component inspection. Press HOME at any time outside an active drag to return from a close-up to the board overview. For each named route, plate its via if present, then connect the highlighted endpoints using the existing Wire tool or ROUTE COPPER. PAD VIEW brings the selected route closer. Complete 58 copper tasks, 12 vias and the W059 patch. Enable 5 V POWER and wait for reference trust.

CHECK REFERENCE after settling. Apply ZERO STANDARD, wait at least 0.65 seconds and CAPTURE; repeat with SPAN STANDARD. RUN ACQUISITION performs a six-second dynamic qualification.

Inject F1, F2 and F3 separately. DIAGNOSE, isolate 5 V, REPAIR, restore power, check reference, recalibrate and run again after each repair. Finally ARM W059, UNDO its actual connection, reconnect, check reference, recalibrate, RESET SAFETY and run a new qualification. SAVE BOARD and RESUME BOARD retain topology and historical evidence; loading requires power off and fresh reference acquisition.

## Engineering model
187 contacts distinguish physical pad and internal package contact through solder switches. SparkCircuitSystem owns player copper; SparkElectricalSolver owns voltages, rails, native resistors, capacitors, diode, LED, supplies and continuity switches. CSV mappings in Data resolve the supplied conceptual placeholders. No decorative copper certifies a route.

The educational transducer model drives native source equivalents. RTD uses a 0.5 mA PT1000 transfer; bridge uses 2.5 V excitation and 4 mV/V full-scale sensitivity with differential gain 200; optical uses 40 nA/lux and 47 kohm feedback; Hall uses 0.25 V zero and 0.8 V/A. The ADC quantizes measured pin voltages against the measured 2.500 V reference to 16 bits. Its 128 Hz clock samples the latest held Unity electrical solution; this is not a transistor-level or continuous-time SPICE simulation. Filtering, deterministic noise, clipping and stale/untrusted states remain explicit. Dynamic acceptance uses a documented educational 4.5% full-scale uncertainty.

W059 carries reference-valid feedback, not the physical reference voltage. Disconnecting it invalidates trust and calibration and latches safety without erasing physical stimulus or earned evidence. Certification bindings are published only after each corresponding measured state transition.

## Art recovery and scale
Original supplied FBXs imported without mesh geometry. Models_Rebuilt contains FBXs recovered from the matching supplied GLBs through the installed Blender. Original FBXs/GLBs remain preserved. Import scale is corrected explicitly; the builder rejects a board width inconsistent with 140 mm. Manufacturing coordinates remain millimetres; a visual root fits the existing large table. Existing room and permanently wall-mounted Master/Workshop monitors are reused.

## Build and validation
Menu: Tools/Project Spark/Levels/Create Level 103. Logs/SparkLevel103.txt records build status. Editor/Level103Check.cs drives the actual MainMenu campaign through Play Mode; Logs/SparkLevel103Check.txt records only executed outcomes and failures. Screenshots are written to Logs. The separate numbered audit has passed all 84 listed cases, including the authoritative 6/6 Play Mode scenario; see Reports/Acceptance_84.csv and Reports/Acceptance.md.

Native initialization and acquisition use SparkElectricalSolver.BeginConfigurationBatch to avoid repeated solves for each contact/source mutation. Verbose solver diagnostics are opt-in. See Reports/Acceptance.md for executed evidence and remaining fidelity/coverage limits.


Contact positions come from the recovered models' conductive lead, fillet, sensor-contact and connector meshes. Resolved_Contact_Geometry.csv records every assignment. J1/J2 use the first two modeled pins; their remaining artwork pins are decorative NC. The supplied RV1 art has only two end contacts, so the educational three-terminal schematic adds an accessible fixed gold wiper land.

Finalization adds raw ADC history and mux-settling checks, wrong-footprint rejection, active-layout UI ownership, preview isolation and legacy save migration. Current Level Controls belongs to the campaign-loaded instance; scene-placed preview copies do not run gameplay. See Reports/Acceptance.md for final evidence and fidelity boundaries.

