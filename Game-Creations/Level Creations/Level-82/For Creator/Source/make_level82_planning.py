#!/usr/bin/env python3
"""Level 82 original art-to-game wiring proposal. Does NOT instantiate SparkTerminal or gameplay wires."""
from pathlib import Path
from collections import defaultdict
import csv,json
R=Path('/mnt/data/ProjectSpark_Level82_HD_AAA')
man=json.loads((R/'Model_Manifest.json').read_text());names=[r['name'] for r in man['models']]
def S(i):return names[int(i)]
G=[
('5 V educational power and returns','''
68,67,5V sequencing PLC logic feed
68,48,5V immunity safety PLC logic feed
68,29,5V EMI receiver control feed
68,30,5V spectrum analyzer logic feed
68,44,5V RF sweep controller logic feed
68,46,5V RF field probe signal conditioning
68,27,5V near-field scanning controller feed
68,4,5V turntable servo control interface
68,61,5V fault diagnostic router logic feed
68,66,5V EMC SCADA logic feed
'''),
('Turntable antenna and scan motion','''
67,4,Turntable azimuth command
4,36,Turntable actual angle sensing
36,67,Measured turntable azimuth feedback
67,5,Turntable gearbox run command
5,67,Turntable gearbox ready status
67,34,Antenna mast vertical position request
34,67,Antenna mast actual-height feedback
67,35,Antenna polarization rotation command
35,67,Antenna actual polarization feedback
67,33,Antenna position/coupling readiness request
33,67,Antenna system measured status
67,27,Nearfield gantry XY scan command
27,67,Nearfield XY actual position feedback
67,28,Nearfield scan Z lift setpoint
28,67,Nearfield Z actual position feedback
27,25,Nearfield magnetic probe carriage control
27,26,Nearfield electric probe carriage control
67,36,Turntable sweep home calibration request
'''),
('Conducted emissions and bonding','''
67,41,DC power artificial-network safe enable
41,42,Conducted emissions source measurement
42,29,Conducted noise spectrum receiver path
39,42,Common-mode cable clamp measured current
40,42,Differential cable current measured value
41,39,Common-mode clamp calibration request
41,40,Differential clamp calibration request
13,42,Chassis bonding monitor status
22,13,Shield termination to bonding grid status
14,39,Harness common-mode current monitor
15,40,Differential harness response measurement
18,42,Common-mode choke insertion attenuation telemetry
19,42,Differential LC filter loss telemetry
17,41,Feedthrough filter DC path measurement
55,41,Switch-mode converter conducted noise proxy
42,67,Conducted scan passfail and margin
29,42,Receiver detector bandwidth setup
32,29,Spectral reference receiver calibration
42,66,Conducted spectrum SCADA telemetry
67,42,Conducted emission scan initiate
'''),
('Radiated emissions and metrology','''
67,29,Radiated emissions receiver acquisition request
33,29,Emission antenna sensed RF input
29,30,Measured EMI spectrum into frequency analyzer
30,66,Radiated spectrum telemetry to SCADA
32,30,Instrument spectral reference calibration
31,29,Preselector actual attenuation and tuning
29,31,Receiver front-end filter request
34,29,Antenna mast height association
35,29,Antenna polarization association
36,29,Turntable measured azimuth association
25,29,Magnetic nearfield probe emission measurement
26,29,Electric nearfield probe emission measurement
24,29,Shield seam leakage measurement
29,67,Emission worst-case result validity
30,67,Spectral peak and detector certificate
67,32,Calibration source enable request
29,66,Field heatmap instrumentation telemetry
67,25,Nearfield probe head measurement enable
'''),
('Shielding filters and harness repairs','''
61,23,Shield gasket compression test request
23,61,Measured gasket compression and closure status
61,22,360 degree cable shield clamp command
22,61,Measured shield termination contact state
61,18,Common-mode choke installation request
18,61,Common-mode choke installation confirmation
61,19,Differential LC filter installation request
19,61,LC filter installed condition
61,20,Ferrite latch installation request
20,61,Ferrite component fitted feedback
61,17,DC feedthrough filter selection request
17,61,Feedthrough filter qualified status
61,21,Harness routing tray position request
21,61,Harness routing actual state
61,13,Chassis bonding continuity measurement request
13,61,Chassis bonding integrity feedback
61,57,Sensor input filter commissioning request
57,61,Sensor input filter response telemetry
'''),
('Controlled RF immunity testing','''
67,44,Protected immunity sweep generator request
44,45,Programmed RF test waveform to amplifier
48,45,Immunity safety output authorization
45,48,Actual RF amplifier state feedback
46,48,Field monitoring auxiliary validity
67,47,Field uniformity frame calibration request
47,46,Field probe calibration reference state
46,67,Calibrated RF field measurement telemetry
44,67,RF sweep frequency and modulation status
45,43,Amplified RF stimulus to coupling fixture supervisory state
43,67,RF coupling fixture readiness
48,43,RF coupling fixture inhibit command
67,51,Sensor emulator qualification during RF test
67,53,Optical link functional check request
67,54,Clock stability functional check request
67,60,Functional mission workload test request
'''),
('Mission function monitoring and faults','''
51,52,Sensor emulator sampled analog input
52,60,Measured ADC deviation mission test result
53,60,Optical link and telemetry jitter measurement
54,60,Precision mission-clock interference measurement
55,52,Power converter induced ADC disturbance coupling
56,54,Spread spectrum timing profile monitor
57,52,Filtered sensor signal actual response
58,60,Infrared thermal enclosure health measurement
60,67,Mission workload functional passfail status
60,48,Mission command safety validity
62,61,Common-mode cable injection fault report
63,61,Ground and return fault actuator report
64,61,Seam-gap actuator fault report
65,61,RF desense virtual fault state
61,67,Diagnostic sourcepathreceiver determination
61,66,EMC fault diagnosis and repair telemetry
'''),
('PLC safety feedback and SCADA','''
67,48,RF exposure safety permit request
49,48,Chamber door actual interlock status
46,48,CRITICAL trusted measured RF field strength feedback
46,66,Non-authoritative field telemetry for SCADA
48,45,Protected RF amplifier enable command
48,45,Independent RF shutdown override
71,48,Emergency RF stop hardwired status
45,48,Actual amplifier disabled confirmation
46,48,Field probe calibration diagnostic health
48,67,Measured RF overlimit alarm status
48,4,Turntable exposure position authorization
34,48,Antenna mast ready for exposure
60,48,Mission function validity for RF exposure
49,67,Chamber door and seal qualified status
48,67,Latched critical RF safety fault
67,48,Operator explicit RF safety reset request
67,66,Authoritative six-objective status telemetry
48,67,Final conductedradiatedimmunity certification status
''')]
assert [len([x for x in block.strip().splitlines() if x.strip()]) for _,block in G]==[10,18,20,18,18,16,16,18]
sockets=list(csv.DictReader((R/'Visual_Terminal_Sockets_206.csv').open(newline='',encoding='utf8')))
assert len(sockets)==206
by_model=defaultdict(list)
for s in sockets:by_model[s['Model']].append(s['AnchorID'])
rot=defaultdict(int);rows=[]
for group,block in G:
 for rec in block.strip().splitlines():
  a,b,desc=rec.strip().split(',',2);a=int(a);b=int(b);src=S(a);dst=S(b)
  if not by_model[src] or not by_model[dst]:raise ValueError(('Missing visual socket',desc,src,dst))
  rot[src]+=1;rot[dst]+=1
  srcanchor=by_model[src][(rot[src]-1)%len(by_model[src])];dstanchor=by_model[dst][(rot[dst]-1)%len(by_model[dst])]
  idx=len(rows)+1
  kind='POWER' if idx<=10 else 'ANALOG_FEEDBACK' if any(w in desc.lower() for w in ('feedback','measured','measurement','actual','sensor','status','field','current','spectrum')) else 'DIGITAL_COMMAND_OR_STATUS'
  rows.append({'WireID':f'W{idx:03d}','Group':group,'SourceModel':src,'SourceVisualAnchor':srcanchor,'TargetModel':dst,'TargetVisualAnchor':dstanchor,'SignalPurpose':desc,'SignalKind':kind,'Critical':'YES' if idx==119 else 'NO','Status':'CONCEPT_ONLY_NO_PLAYER_WIRE'})
assert len(rows)==134
assert (rows[118]['SourceModel'],rows[118]['TargetModel'])==('L82_TriaxialRFFieldStrengthProbe','L82_RFImmunitySafetyPLC')
with (R/'Gameplay_Wiring_Concept_W001_W134.csv').open('w',newline='') as f:
 w=csv.DictWriter(f,fieldnames=rows[0].keys());w.writeheader();w.writerows(rows)
with (R/'Gameplay_Wiring_Budget.csv').open('w',newline='') as f:
 w=csv.writer(f);w.writerow(['Group','FirstWire','LastWire','PlannedWires','Status']);a=1
 for g,bl in G:
  n=len(bl.strip().splitlines());w.writerow([g,f'W{a:03d}',f'W{a+n-1:03d}',n,'CONCEPT_ONLY']);a+=n
(R/'Terminal_Anchor_Plan_206.csv').write_text((R/'Visual_Terminal_Sockets_206.csv').read_text())
objectives=[
('1','Build the EMC Qualification Network','134 player-created electrical wires connected in real Spark graph; 206 SparkTerminals registered; 5V power, EMC instruments and independent RF safety healthy'),
('2','Calibrate the Blackstar Chamber','Chamber door interlock; turntable; antenna mast and polarization; EMI receiver; near-field probes; conducted instruments and trusted field probe calibrated'),
('3','Silence Conducted and Radiated Emissions','Source/path diagnosis and physical repairs; complete required rotational antenna measurements; conducted and radiated margins pass without breaking mission functionality'),
('4','Prove RF Immunity','W119 trusted field feedback and independent interlocks verified; safe fictional exposure sweeps pass with valid sensors clocks telemetry and zero unsafe commands'),
('5','Survive Three EMC Failures','Common-mode cable fault; shielding seam leakage; RF-induced sensor susceptibility all diagnosed and repaired; fresh complete EMC and mission-function qualification passes'),
('6','Restore Critical RF Field Feedback','Player unplugs real W119, PLC sees invalid stale field measurements and inhibits RF, fault latches; reconnect and recalibrate then explicitly reset and run fresh comprehensive emissionsimmunity certification')]
with (R/'Objective_Certification_Plan_6.csv').open('w',newline='') as f:
 w=csv.writer(f);w.writerow(['Objective','Title','AuthoritativeEvidence','Status'])
 for row in objectives:w.writerow([*row,'DESIGN_ONLY_NOT_IMPLEMENTED'])
w119=rows[118]
(R/'W119_Critical_Feedback_Recovery.md').write_text(f'''# Level 82 — W119 critical RF field-strength feedback recovery\n\n## Planned real electrical endpoints\n- **W119** is a real player-created analog measurement wire in the future Unity implementation.\n- Source model: `{w119['SourceModel']}`\n- Proposed terminal: `RF_FIELD_MEASURED_OUT`\n- Source visual socket: `{w119['SourceVisualAnchor']}`\n- Destination model: `{w119['TargetModel']}`\n- Proposed terminal: `RF_SAFE_FIELD_FEEDBACK_IN`\n- Target visual socket: `{w119['TargetVisualAnchor']}`\n\n## Critical feedback semantics\nThe field probe models actual RF field magnitude with timestamp, validity, measurement range and calibration. W119 must be continuously sampled; when unplugged, the last good value must expire. W120 SCADA telemetry and W125 diagnostic calibration health are **not** substitutes. The independent emergency RF stop, amplifier-disable path and door interlocks remain effective.\n\n## Required final challenge (NOT included in art)\n1. After historically certifying Objectives 1–5, player physically disconnects actual W119; the electrical topology changes and trusted field measurement becomes invalid or stale.\n2. RF safety PLC removes amplifier permit, commands safe shutdown and confirms actual amplifier-disabled state; latch critical fault; preserve chamber and EUT state.\n3. Player reconnects W119, receives fresh valid samples, performs field probe calibration and verifies other interlocks.\n4. Explicit operator reset clears latch only when all safety prerequisites pass.\n5. Run **fresh complete** conducted and radiated emissions tests, protected RF immunity exposure, and Level 81 mission-computer functional monitoring. Objective 6 then certifies real `LevelGamePlayManager` 6/6. Historical Objectives 1–5 remain certified.\n\n**Package limitation:** this art collection contains only conceptual wiring and socket geometry; no actual `SparkTerminal`, electrical solver connection or safety PLC gameplay is implemented.\n''')
(R/'Gameplay_Integration_Plan.md').write_text('''# Level 82 — Complete gameplay integration plan\n\n## Status\nThe asset pack contains original procedural 72-FBX and matching GLB visual artwork, 54 mesh-owning pivot groups, 206 modeled gold-rimmed socket meshes, 134 planned electrical connections, and Unity art helpers. **It does not contain working Project Spark gameplay, electrical circuits or six objectives.**\n\n## Authoritative domains\n- **Electrical:** real `SparkTerminal` components, 134 player-created wires, 5V logic supply, analog sensors, actuator commands, source/return verification, independent safety power domain.\n- **EMC field model:** bounded educational spectral sources, cable common-mode and differential currents, frequency-dependent shield leakage, antenna orientation/height/polarization, filter attenuation and susceptibility; no full-wave Maxwell solution claimed.\n- **Machine motion:** real turntable angular dynamics, calibrated antenna mast lift and polarization, 3D near-field probe positions, door mechanics. The `SparkLevel82VisualRig` accepts physical state only.\n- **Mission monitoring:** Level 81 EUT sensor/clock/optical telemetry workload, qualified command gate, tolerance and thermal limits.\n- **RF safety:** trusted field feedback W119, chamber door, independent emergency stop, RF source disable with measured confirmation, latched fault and explicit reset.\n\n## Disturbances\n1. Wrong common-mode harness routing/return current creates conducted/radiated peak.\n2. Open conductive shield gasket increases seam radiation.\n3. Controlled immunity RF couples into analog input and causes navigation sensor error; repair must not cause excess latency.\n\n## Objective sequence\nSee `Objective_Certification_Plan_6.csv`. Must be connected to real `LevelGamePlayManager` and prove 6/6 through **actual** gameplay evidence.\n\n## Art import\nImport FBX models once, not assembled showcase simultaneously, apply centimetre-to-metre conversion once, set all modular model transforms to identity, keep gameplay objects outside replaceable `L82_HD_VISUAL_ART`.\n''')
(R/'AAA_Art_Delivery_Notes.md').write_text('''# Project Spark Level 82 — Silence the Spectrum — HD AAA-inspired art notes\n\nOriginal procedural EMC qualification environment. Actual FBX 7400 binary geometry and matching meter-unit GLB. 72 modular assets + assembled showpieces in both formats; physically modeled RF absorber pyramids, EUT rotation stage, near-field gantry, measurement mast and log-periodic style antenna, shielded avionics computer, cable/filter repair bench, protected immunity equipment, spectrum displays, 206 gold-rimmed visual sockets and 54 mesh-owning ANIM pivots.\n\nMaterials are preview PBR maps, NOT final authored game-ready baked textures. Many mesh parts: use static batching, instancing and appropriate LODs in Unity before shipping.\n\nNo true electromagnetics solver, safety hardware, gameplay electrical wires, SparkTerminals, playable objectives, Unity build or Play Mode testing provided. No actual hazardous RF output; a simulated educational game.\n\nWorkstation nominal width 25.4 meters, actual bounds in Model_Manifest.json. FBX numeric geometry in centimeters, GLB geometry in meters, Y-up. Do not instantiate assembled and modular models together.\n''')
print('PLAN COMPLETE',len(rows),'real wire concepts',len(sockets),'visual sockets','critical',w119)
