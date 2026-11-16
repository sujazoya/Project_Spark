#!/usr/bin/env python3
"""Level80 art-linked gameplay integration concepts; NOT live Unity electrical/optical gameplay."""
import csv,json, math
from pathlib import Path
R=Path('/mnt/data/ProjectSpark_Level80_HD_AAA')
manifest=json.loads((R/'Model_Manifest.json').read_text())
N=[x['name'] for x in manifest['models']]
S=lambda i:N[i]
G=[
('Educational 5 V control power and returns',[
(66,57,'PLC control power'),(66,56,'Optical link supervisor logic power'),(66,61,'SCADA control power'),(66,58,'Emergency shutter logic power'),(66,11,'Laser driver logic power'),(66,32,'Microring heater DAC logic power'),(66,38,'Photodiode conditioning supply'),(66,41,'Receiver optical power monitoring interface supply'),(66,44,'Eye diagram analyzer logic power'),(66,45,'Bit error tester logic power')]),
('Laser source shutter and power control',[
(57,11,'Protected laser source enable'),(11,9,'Laser source drive command'),(9,16,'Launch power tap monitor'),(16,56,'Measured launched optical power'),(57,13,'Safety shutter open command'),(13,58,'Actual shutter closed feedback'),(57,18,'Optical attenuator setpoint'),(18,56,'Optical attenuator actual transmission'),(10,56,'Laser comb wavelength lock'),(12,56,'Laser temperature measured'),(56,12,'Laser temperature regulation request'),(19,56,'Spectral reference calibration'),(63,58,'Stray light protection feedback'),(56,14,'Optical isolator status request'),(14,56,'Isolator ready feedback'),(17,56,'Polarization conditioning state'),(15,56,'Optical tap splitter health'),(16,61,'Laser power telemetry to SCADA')]),
('Modulator transmitter and chiplet interfaces',[
(56,20,'Transmit chain enable'),(20,21,'Four-channel transmitter drive'),(22,21,'Modulator bias control'),(21,22,'Actual modulator optical response feedback'),(51,20,'Logic chiplet electrical traffic'),(50,20,'Memory chiplet electrical traffic'),(52,20,'I/O chiplet electrical traffic'),(53,20,'Sensor chiplet electrical traffic'),(57,48,'Pseudorandom test pattern start'),(48,20,'Reference data stream'),(20,56,'Transmitter actual enable feedback'),(56,23,'WDM multiplexer permission'),(23,56,'Multiplexer health feedback'),(21,56,'Modulator array calibration status'),(56,49,'Chiplet data routing request'),(49,56,'Chiplet traffic assignment status'),(20,61,'Transmitter drive telemetry'),(21,61,'Modulator bias telemetry')]),
('Four microring tuning and thermal control',[
(56,32,'Microring heater control enable'),(32,31,'Four-zone heater DAC output'),(33,32,'Four-zone ring temperature sensing'),(34,56,'Measured inter-ring thermal crosstalk'),(32,27,'Ring A microheater command'),(27,33,'Ring A measured temperature'),(32,28,'Ring B microheater command'),(28,33,'Ring B measured temperature'),(32,29,'Ring C microheater command'),(29,33,'Ring C measured temperature'),(32,30,'Ring D microheater command'),(30,33,'Ring D measured temperature'),(27,56,'Ring A spectral lock feedback'),(28,56,'Ring B spectral lock feedback'),(29,56,'Ring C spectral lock feedback'),(30,56,'Ring D spectral lock feedback'),(34,32,'Thermal crosstalk compensation request'),(36,56,'Demultiplexer resonance health feedback'),(33,61,'Ring actual temperature telemetry'),(56,26,'Photonic thermal stage operating request')]),
('Receiver and optical measurement feedback',[
(56,35,'WDM demultiplexer configuration command'),(35,56,'Demultiplexer configured status'),(56,38,'Photodiode receiver enable'),(38,39,'Photodiode analog current outputs'),(39,40,'Recovered analog receiver waveform'),(40,56,'Receiver clock/data lock status'),(38,41,'Optical receiver calibrated power tap'),(41,61,'Receiver power SCADA telemetry (not trusted control feedback)'),(42,56,'Optical spectrum analyzer spectral scan'),(43,56,'Link optical power budget estimate'),(37,38,'Drop port array optical health status'),
(41,56,'CRITICAL receiver optical power feedback: trusted measurement path'), # W078
(41,58,'Receiver power instrument diagnostic health only (cannot bypass W078)'),(56,39,'TIA gain request'),(39,56,'TIA actual gain/status'),(38,56,'Receiver photodiode array readiness'),(42,61,'Measured wavelength spectrum SCADA telemetry'),(43,61,'Optical link loss telemetry')]),
('BER clock recovery and signal testing',[
(57,45,'Four-channel BER test enable'),(48,45,'PRBS expected pattern reference'),(40,45,'Recovered test-bit stream'),(45,56,'Measured BER qualification'),(44,56,'Eye-opening result'),(46,56,'PAM4 sample quality result'),(47,56,'FEC decoder status'),(45,47,'Raw errored bit stream'),(47,49,'Corrected data test stream'),(56,40,'Clock recovery lock request'),(40,56,'Recovered clock status'),(49,56,'Four-chiplet data route pass'),(50,56,'Memory optical response result'),(51,56,'Logic optical transmitter status'),(52,56,'I/O optical output status'),(53,56,'Sensor optical telemetry result')]),
('Fiber loss and diagnostics',[
(54,56,'Optical fiber bend-loss monitor'),(55,56,'Controlled optical fault state'),(56,55,'Fictional optical fault injection enable'),(62,56,'Photonic waveguide defect microscope result'),(64,56,'Optical calibration coupon acceptance'),(65,57,'ESD guarded fixture state'),(7,56,'Fiber dock seated status'),(6,56,'Photonic edge coupler alignment status'),(8,56,'Fiber cassette identity and inventory'),(60,61,'Thermal crosstalk map telemetry'),(5,61,'Waveguide map visualization state'),(24,61,'Combined wavelength hologram telemetry'),(67,61,'Routing hologram display state'),(56,61,'Aggregated optical diagnostics')]),
('PLC safety and SCADA',[
(58,57,'Emergency shutter interlock'),(57,58,'Independent laser/shutter safety override'),(56,57,'Link supervisor optical safety permit'),(57,56,'PLC operating-state authorization'),(56,61,'Four-channel qualification telemetry'),(61,56,'SCADA operator command'),(56,57,'Critical feedback loss fault request'),(57,11,'Laser safety disable'),(57,13,'Safety shutter fail-closed command'),(56,61,'W078 critical measurement validity state'),(56,61,'Objective evidence and historical certification'),(57,61,'Safety status and critical alarm')])]
assert [len(rows) for _,rows in G]==[10,18,18,20,18,16,14,12]
with (R/'Gameplay_Wiring_Concept_W001_W126.csv').open('w',newline='',encoding='utf8') as f:
    w=csv.writer(f);w.writerow(['WireID','Group','SourceModel','TargetModel','SignalPurpose','SignalKind','Critical','Status'])
    j=0
    for name,rows in G:
        for a,b,description in rows:
            j+=1
            typ='POWER' if j<=10 else 'ANALOG_FEEDBACK' if any(s in description.lower() for s in ('measured','feedback','temperature','power tap','current outputs','waveform','spectrum')) else 'DIGITAL_OR_COMMAND'
            w.writerow([f'W{j:03}',name,S(a),S(b),description,typ,'YES' if j==78 else 'NO','CONCEPT_ONLY_NO_PLAYER_WIRE'])
assert j==126
assert G[4][1][11][:2]==(41,56)
with (R/'Gameplay_Wiring_Budget.csv').open('w',newline='',encoding='utf8') as f:
    w=csv.writer(f);w.writerow(['Group','FirstWire','LastWire','PlannedPlayerWires','Scope'])
    n=1
    for name,rows in G:
        w.writerow([name,f'W{n:03}',f'W{n+len(rows)-1:03}',len(rows),'Conceptual electrical controls; not operational Unity wires'])
        n+=len(rows)
# A plausible practical terminal socket palette, evenly distributed among 68 equipment groups.
counts=[2]*68
# 58 more sockets, favor controllers, power distribution, sensing and source/ring stations.
priority=[56,57,61,32,31,33,11,13,20,21,38,39,40,41,42,43,44,45,47,48,49,50,51,52,53,58,66,27,28,29,30,35,37,23,9,10,12,54,55,7,6,16,18,19,34,59,60,62,63,64,65,67,1,4,3,22,46,24]
assert len(priority)==58 and len(set(priority))==58
for i in priority:counts[i]+=1
assert sum(counts)==194
with (R/'Terminal_Anchor_Plan_194.csv').open('w',newline='',encoding='utf8') as f:
    w=csv.writer(f);w.writerow(['PlannedAnchorID','Model','WorldX_m','WorldY_m','WorldZ_m','Purpose','Status'])
    j=0
    for i,m in enumerate(manifest['models']):
        lo,hi=m['bbox_min'],m['bbox_max'];cx=(lo[0]+hi[0])/2;cy=(lo[1]+hi[1])/2
        width=max(.2,hi[0]-lo[0]);height=max(.2,hi[1]-lo[1]);top=hi[2]+.065
        for n in range(counts[i]):
            j+=1
            xx=cx+width*.32*((n%3)-1) if counts[i]>2 else cx+width*.26*(2*n-1)
            yy=cy+height*.14*(1 if n%2 else -1)
            purpose=('Instrument/logic socket' if i not in (66,57,56,41) else 'Safety, power or metrology terminal')
            w.writerow([f'P{j:03}',m['name'],round(xx,5),round(yy,5),round(top,5),purpose,'PROPOSED_ONLY_NO_SPARKTERMINAL_OBJECT'])
assert j==194
objectives=[
('1','Build the Photonic Control Network','Create all W001-W126 player connections and 194 functional terminals; validate supply/returns/signal topology and laser safety'),
('2','Commission the Optical Engine','Verify Level79 four-chiplet package, separate photonic engine, protected laser/shutter, power meters, coupling and receiver calibration'),
('3','Lock the Four Wavelengths','Establish all four microring resonances with finite heater dynamics, stability interval, measured spectral spacing and crosstalk margin'),
('4','Transmit Four Independent Data Streams','Modulators/photodiodes/clock and data recovery active; valid eye, BER/FEC and chiplet route evidence on all four wavelength channels'),
('5','Survive Optical Communication Disturbances','Recover laser drift, coupled ring heater instability, and fiber microbend loss; qualify a fresh complete four-channel transmission'),
('6','Restore Critical Optical Power Feedback','Physically unplug W078; invalidate measurement; protected laser/shutter shutdown; reconnect/calibrate/reset; independently requalify all channels and fresh data tests')]
with (R/'Objective_Certification_Plan_6.csv').open('w',newline='',encoding='utf8') as f:
    w=csv.writer(f);w.writerow(['Objective','Title','AuthoritativeEvidence','Status'])
    for i,name,desc in objectives:w.writerow([i,name,desc,'PROPOSED_ONLY_NOT_IMPLEMENTED'])
(R/'W078_Critical_Feedback_Recovery.md').write_text('''# Project Spark Level 80 — W078 critical RX optical power feedback recovery

## Electrical path (proposal, NOT an implemented SparkCircuitSystem connection)
- Wire ID: `W078`
- Source: `L80_ReceiverOpticalPowerMonitor`
- Destination: `L80_OpticalLinkSupervisoryController`
- Proposed source terminal: `RX_OPTICAL_POWER_SIGNAL_OUT`
- Proposed destination terminal: `LINK_SUPERVISOR_RX_POWER_IN`
- Source file: `Gameplay_Wiring_Concept_W001_W126.csv`, row W078.

## Explicit trust boundary
- W075 optical spectrum and W076 power-budget measurements are **not** trusted substitutes for W078.
- W074 receiver power SCADA telemetry is not W078.
- W079 receiver-power instrument diagnostic health to safety is not W078.
- No stored, stale or display reading can certify optical safety/receiver power.

## Authoritative sequence required in gameplay implementation
1. Objectives 1–5 are historically certified.
2. Player removes the real W078 wire; circuit graph updates and trusted RX-power sample invalidates.
3. Samples are marked stale; supervisor withdraws power qualification; stops data-link training/certification.
4. Independent safety pathway commands controlled laser output disable and protective shutter closure; optical power state decays according to source and shutter dynamics. Fault latches and is recorded.
5. Player physically reconnects W078. New validly routed samples are acquired, but measurement is not immediately calibrated.
6. Player calibrates RX power monitor, verifies safe shutter and source, explicitly resets protection.
7. All four wavelength channels are reacquired and re-locked; fresh optical power/BER/eye/data-route qualification passes.
8. Only then Objective 6 is certified by the real `LevelGamePlayManager`; Objectives 1–5 retain historical completion.

The delivered files contain **art and a plan**. No functional SparkTerminals, player-wiring system or safety logic is implemented in these meshes.
''',encoding='utf8')
(R/'Gameplay_Integration_Plan.md').write_text('''# Project Spark Level 80 — Unity gameplay integration plan

**Level:** `level_80` — Command the Light

## Scope
The art package includes 68 procedural modular binary FBXs, 68 matching GLBs, assembled references and 50 genuine named animation parent transforms. It does **not** include functional gameplay components, working optical graph or live electrical simulation.

## Distinct interacting simulation domains
1. **Electrical domain**: the user's W001–W126 control, analog, digital and supply wires, real SparkTerminals and monitored disconnects. A conceptual CSV is provided; convert it through the actual Project Spark electrical API.
2. **Optical domain**: source optical output, safety shutter, attenuator, coupling efficiency, photonic guide/fiber losses, four resonators, WDM multiplexer/demultiplexer, drop-port power and photodiode conversion.
3. **Thermal/control domain**: four actual ring-heater temperatures with coupled thermal response, source wavelength stabilization and modulation bias.
4. **Data domain**: separate Logic, Memory, I/O and Sensor streams, modulation, clock recovery, modeled eye, jitter, BER/FEC and routing evidence.
5. **Safety domain**: protected laser source, independent shutter, disabled transmit/training on missing trusted W078 feedback, latched reset, maintenance/calibration.

## Calibration scenario targets (fictional educational parameters)
Four carrier channels with individually tuned resonances, sufficient per-channel RX power, safe bounded laser output, low crosstalk and a stable qualification period; no specific real industrial laser safety levels or recipes are supplied.

## Objective evidence
Consult `Objective_Certification_Plan_6.csv`. All six are **unimplemented** in this art pack. An actual Unity Level 80 gameplay builder and an authoritative 6/6 evaluation remain to be built.

## W078
Consult `W078_Critical_Feedback_Recovery.md`. W078 is the receiver optical power monitor's measurement output into the optical link supervisor. SCADA data and diagnostic health must never silently bypass it.

## Art rig
Attach `SparkLevel80VisualRig` to a stable parent of `L80_HD_VISUAL_ART` and call only its `SetActual...` methods from the authoritative optical/thermal/data process state. The rig never creates or approves a measurement by itself.

## Electrical graph precautions
External optical fibers and on-chip waveguides are **not** electrical `SparkTerminal` wires. Actual player connections control or monitor optoelectronic components. Use a separate optical network representation, coupled through source/modulator/photodiode devices.
''',encoding='utf8')
(R/'AAA_Art_Delivery_Notes.md').write_text('''# Level 80 HD / AAA-inspired art delivery — limitations

- Original generated procedural 3D geometry, **not** a raster image.
- 68 unique modular binary FBX 7.4 exports and 68 matching GLBs.
- One assembled FBX+GLB as an alternate art preview; do **not** place it alongside the individual models.
- The FBX writer exports centimetre numeric vertices with UnitScaleFactor=1; matching GLB vertices are in metres. Set Unity importer scale once and check assembled width ~23.2 m.
- 50 actual named pivot hierarchy groups are exported. Their animation data is **not baked clips**; the included C# visual rig drives transforms from actual data when connected by gameplay code.
- Preview vertex colors / basic shading, not baked production PBR texture sets. A Unity material upgrade helper is supplied.
- Photonic infrared wavelengths are depicted in false-color cyan/green/amber/violet **for education**. They are not literally visible waveguides.
- High geometry and part count can benefit from optimization/LOD and instance batching before shipping.
- No authored interactions, optical physics, functional electrical terminals, safety interlocks, W078 monitoring or objective 6/6 are included. See planning files.
''',encoding='utf8')
print('Planning complete',j,'terminal anchors',len(objectives),'objectives')
