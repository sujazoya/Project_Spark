#!/usr/bin/env python3
"""Grounded Level 81 art-to-game planning. This DOES NOT create SparkTerminal or live wires."""
from pathlib import Path
from collections import defaultdict
import json,csv
R=Path('/mnt/data/ProjectSpark_Level81_HD_AAA')
manifest=json.loads((R/'Model_Manifest.json').read_text())
names=[x['name'] for x in manifest['models']]
def S(i):return names[int(i)]
# 130 conceptual player electrical wires; DATA/CONTROL connections are electrically distinct from virtual memory state.
G=[
('Control power / returns', '''
58,55,5V mission safety PLC feed
58,56,5V trusted ECC supervisor feed
58,9,5V virtual fault controller feed
58,19,5V ECC syndrome controller feed
58,22,5V memory scrubber feed
58,32,5V redundant compute sequencer feed
58,40,5V command gate control feed
58,45,5V optical telemetry bridge feed
58,59,5V guarded fault console feed
58,61,5V SCADA control feed
'''),
('Clock, reset and mission scheduling', '''
37,32,Mission task ready strobe
37,29,Core A mission clock request
37,30,Core B mission clock request
37,31,Core C mission clock request
37,38,Watchdog cycle synchronization
38,37,Watchdog timer status
39,38,Watchdog challenge reply
55,37,Mission scheduler safety permit
37,17,Memory address cycle command
17,15,Memory bank A address bus
17,16,Memory bank B address bus
32,37,Triple lane lockstep state
36,32,Reference compute readiness
55,40,Safe command gate enable
52,37,Guidance computation request
37,55,Mission deadline status
'''),
('Memory ECC encoding, correction and scrubbing', '''
17,18,Data word read for SECDED encoding
18,15,ECC codeword bank A write
18,16,ECC codeword bank B write
15,19,Memory bank A ECC syndrome input
16,19,Memory bank B ECC syndrome input
19,20,Correctable error position output
20,15,Corrected codeword writeback A
20,16,Corrected codeword writeback B
19,21,Detected double bit error event
21,25,Uncorrectable page quarantine request
23,22,Memory scrub interval command
22,17,Scrub address cycle selection
22,19,Scrub syndrome evaluation request
19,22,Scrub correction result
24,23,Scrubber bandwidth availability
22,24,Scrubber utilization feedback
25,26,Validated checkpoint selection request
26,27,Trusted backup data transfer authorization
27,25,Recovered page verification result
67,27,Checkpoint integrity reference confirmation
'''),
('Redundant computing and TMR voting', '''
32,29,Core A lockstep input
32,30,Core B lockstep input
32,31,Core C lockstep input
29,33,Core A actual calculated output
30,33,Core B actual calculated output
31,33,Core C actual calculated output
33,34,Voter disagreement telemetry
34,32,Faulty lane identification
35,29,Core A independent health status
35,30,Core B independent health status
35,31,Core C independent health status
36,33,Independent reference result compare
53,34,Correlated common mode alert
34,40,Voter qualified command permit
33,40,Majority voted command result
36,40,Independent validation gate
40,52,Validated guidance command
52,51,Authorized fictional spacecraft actuation
'''),
('Optical telemetry and packet integrity', '''
49,42,Synthetic sensor data packet source
50,42,Navigation state packet source
42,45,CRC encoded telemetry frame
45,43,Optical receiver reconstructed frame
43,44,CRC checked frame into sequence monitor
44,47,Verified packet order result
43,47,Packet CRC validity result
44,46,Missing sequence retransmission request
46,45,Retransmitted packet data
47,48,Qualified telemetry to protected buffer
48,50,Validated navigation telemetry data
45,47,Optical link receiver validity status
47,55,Telemetry integrity mission permission
45,44,Optical stream packet age
43,46,Reject bad frame signal
44,48,Replay or duplicate quarantine status
48,52,Sensor freshness proof for command validator
47,61,Telemetry integrity dashboard reporting
'''),
('Fault injection / monitoring / ECC trust', '''
59,9,Operator fictional bit fault injection command
9,8,Seeded fault distribution command
8,15,Memory A virtual upset event
8,16,Memory B virtual upset event
9,29,Core A controlled transient event
9,30,Core B controlled transient event
9,31,Core C controlled transient event
8,53,Fault correlation raw event feed
53,54,Correlated fault pattern result
54,55,Fault propagation risk to mission safety
21,56,Double bit ECC alarm into supervisory diagnostics
19,56,CRITICAL ECC syndrome feedback and heartbeat
56,55,Trusted ECC feedback health to mission safety
19,59,Non-authoritative ECC inspection display
54,9,Fault event trace diagnostic readout
53,32,Common mode computation inhibit
9,45,Packet corruption injection signal
9,61,Fault injection telemetry for SCADA
'''),
('Watchdog and recovery supervision', '''
38,55,Watchdog expired event
39,55,Watchdog challenge confidence status
55,40,Mission safe hold command
55,27,Trusted recovery approval
27,67,Restore checksum reference request
67,27,Backup checksum comparison feedback
25,55,Memory quarantine status
22,56,Scrubber inspected word freshness
56,55,ECC trusted sample freshness
56,27,ECC recertification request
27,55,Memory recovery verified result
33,55,Majority voter overall confidence
36,55,Independent compute validation status
47,55,Packet continuity qualification
55,59,Guarded fault console safe enable
59,55,Explicit manual reset request
'''),
('Safety PLC, SCADA and certification', '''
57,55,Protected controls distribution status
58,57,Educational 5V supply status
55,60,Emergency mission hold actuator command
60,55,Emergency hold physical feedback
55,59,Fault injection permission interlock
55,61,Safety PLC state telemetry
56,61,Trusted ECC monitoring telemetry
25,61,Quarantined page count telemetry
22,61,Memory scrubber progress telemetry
33,61,Triple modular voter status telemetry
37,61,Mission timing and task health telemetry
47,61,Telemetry packet integrity status
55,65,Qualified mission health status
55,69,Final 6 objective certification status
''')]
assert [len([x for x in s.strip().splitlines() if x.strip()]) for _,s in G]==[10,16,20,18,18,18,16,14]
sockets=list(csv.DictReader((R/'Visual_Terminal_Sockets_200.csv').open(newline='',encoding='utf8')))
assert len(sockets)==200
by_model=defaultdict(list)
for r in sockets:by_model[r['Model']].append(r['AnchorID'])
rot=defaultdict(int)
rows=[]
for category,records in G:
 for rec in records.strip().splitlines():
  a,b,description=rec.strip().split(',',2)
  a=int(a);b=int(b);src=S(a);dst=S(b)
  if not by_model[src] or not by_model[dst]:raise RuntimeError(('No real art socket anchor',rec))
  rot[src]+=1;rot[dst]+=1
  src_anchor=by_model[src][(rot[src]-1)%len(by_model[src])]
  dst_anchor=by_model[dst][(rot[dst]-1)%len(by_model[dst])]
  idx=len(rows)+1
  kind='POWER' if idx<=10 else 'ANALOG_FEEDBACK' if any(word in description.lower() for word in ('feedback','actual','syndrome','health','status','measurement','utilization','temperature','value')) else 'DIGITAL_COMMAND_OR_STATUS'
  rows.append({'WireID':f'W{idx:03d}','Group':category,'SourceModel':src,'SourceVisualAnchor':src_anchor,'TargetModel':dst,'TargetVisualAnchor':dst_anchor,'SignalPurpose':description,'SignalKind':kind,'Critical':'YES' if idx==94 else 'NO','Status':'CONCEPT_ONLY_NO_PLAYER_WIRE'})
assert len(rows)==130
assert (rows[93]['SourceModel'],rows[93]['TargetModel'])==('L81_ECCSyndromeDetectionEngine','L81_CriticalECCFeedbackSupervisor')
with (R/'Gameplay_Wiring_Concept_W001_W130.csv').open('w',newline='',encoding='utf8') as f:
 w=csv.DictWriter(f,fieldnames=rows[0]);w.writeheader();w.writerows(rows)
with (R/'Gameplay_Wiring_Budget.csv').open('w',newline='',encoding='utf8') as f:
 w=csv.writer(f);w.writerow(['Group','FirstWire','LastWire','PlannedWires','Status'])
 a=1
 for group,block in G:
  n=len(block.strip().splitlines());w.writerow([group,f'W{a:03d}',f'W{a+n-1:03d}',n,'CONCEPT_ONLY']);a+=n
# Keep legacy expected naming consistent with Level 79/80 workflow.
(R/'Terminal_Anchor_Plan_200.csv').write_text((R/'Visual_Terminal_Sockets_200.csv').read_text(encoding='utf8'),encoding='utf8')
objectives=[
('1','Build the Mission Reliability Network','All 130 player wires constructed and tested; 200 real SparkTerminals registered; 5V control, ECC, watchdog, PLC, TMR and W094 are valid'),
('2','Commission and Prove Error Correction','Actual SECDED encodes protected words, single-bit fault produces correct syndrome and verified correction, scrubber writes back validated codeword'),
('3','Build Fault-Tolerant Computing','Three computing lanes commissioned; genuine majority vote, independent reference validation and watchdog-backed safe command authorization'),
('4','Survive the Virtual Radiation Mission','Run navigation acquisition, fictional maneuvering and data return with valid ECC state, safe command outcomes and CRC/sequence checks'),
('5','Defeat Complex Digital Failures','Recover double-bit ECC fault using verified checkpoint, correlated TMR failure and corrupted/replayed telemetry; fresh complete mission passes'),
('6','Restore Critical ECC Supervision','Real W094 unplug invalidates trusted syndrome status and latches safe hold; reconnect, fresh samples, diagnostics and reset then fresh three-phase mission certifies')]
with (R/'Objective_Certification_Plan_6.csv').open('w',newline='',encoding='utf8') as f:
 w=csv.writer(f);w.writerow(['Objective','Title','AuthoritativeEvidence','Status'])
 for r in objectives:w.writerow([*r,'DESIGN_ONLY_NOT_IMPLEMENTED'])
(R/'W094_Critical_Feedback_Recovery.md').write_text('''# Level 81 — W094: critical ECC syndrome feedback path

## Actual planned end points
- Wire ID: **W094**
- Source model: `L81_ECCSyndromeDetectionEngine`
- Proposed source terminal: `ECC_SYNDROME_STATUS_OUT`
- Destination model: `L81_CriticalECCFeedbackSupervisor`
- Proposed destination terminal: `CRITICAL_ECC_SYNDROME_IN`
- Visual art socket anchors: see row W094 in `Gameplay_Wiring_Concept_W001_W130.csv`.

## Trusted heartbeat semantics
This line must carry periodically refreshed ECC syndrome/health data, including a valid timestamp/sequence. Absence of an ECC error **is not** equivalent to an absent wire. A stale last-good frame cannot certify memory health. W093 and W095 diagnostic/telemetry wires must not bypass W094.

## Required gameplay sequence (not provided by this art package)
1. Complete and historically certify Objectives 1–5.
2. Physically unplug player-created W094, changing the *actual* Spark electrical topology.
3. Supervisory feedback becomes stale or invalid. Local ECC hardware may continue to detect/correct; the supervisor can no longer certify it.
4. Safety PLC inhibits unverified critical command commits, permits only safe processing, enters controlled mission-safe hold; record and latch fault.
5. Reconnect W094, acquire fresh messages, run diagnostic interface qualification; verify outstanding ECC faults, memory quarantine and checkpoint restoration.
6. Explicit operator safety reset; run new memory validation, three-lane majority/reference test, and fresh three-part fictional mission with CRC/sequence verification.
7. Objective 6 awarded *only* after real LevelGamePlayManager evidence reaches 6/6; preserve historical 1–5.

No actual SparkTerminal components, player wires, fault simulator or LevelGamePlayManager objectives are included in the FBX/GLB geometry. 
''',encoding='utf8')
(R/'Gameplay_Integration_Plan.md').write_text('''# Level 81 — Unity gameplay integration plan

## Deliverable is visual art and planning only
- 70 original procedural modular FBXs and 70 corresponding GLBs, plus assembled preview in each format.
- 52 real named hierarchy pivot parents in FBX/GLB (see `Animation_Rigs.csv`).
- 200 visually modeled terminal sockets with matching world coordinates (`Terminal_Anchor_Plan_200.csv`). **These are not functional SparkTerminals.**
- 130 player-wire concepts with source/destination model + proposed visual socket anchors; neither wire topology nor terminals are connected to a live Project Spark electrical graph.

## Four interacting simulation domains to implement
1. **Electrical** — 5V control, PLC, real player wiring, monitored disconnects, watchdog I/O.
2. **Digital storage** — 72-bit SECDED codewords carrying 64-bit data; actual syndrome/overall parity, single-bit correction and double-bit detection, scrubber and page quarantine.
3. **Computing/mission** — Three truly distinct deterministic compute lanes, TMR voter, independent reference comparison, mission tasks, watchdog, safe output gate.
4. **Communication** — Level 80 qualified optical link transport, actual packet CRC/sequence checks, retransmission and safety-certified data freshness.

## Safety and fault injection
- Fictional, seeded bit/register event generator **only**. No real radiation source or hazardous test procedure.
- Three disturbances: accumulated uncorrectable memory fault, correlated two-lane compute error, corrupted/replayed telemetry.
- W094 trusted ECC syndrome/heartbeat is a critical supervisory wire: broken/stale feedback must latch mission safe hold and require repair/calibration/reset and fresh full mission run.

## Six objective evidence
See `Objective_Certification_Plan_6.csv`. Real LevelGamePlayManager integration is pending; no objective is currently implemented.

## Art placement
Use `Tools/Project Spark/Levels/81/Place HD Art Under Selected Layout`; keep game objects outside `L81_HD_VISUAL_ART` because re-placing art may replace that root. Use `SparkLevel81VisualRig` exclusively as a **consumer of actual simulation values**; it cannot award certificates.
''',encoding='utf8')
(R/'AAA_Art_Delivery_Notes.md').write_text('''# Level 81 — HD / AAA-inspired original 3D asset collection

## What is included
- True modular binary FBX 7400 geometry and GLB geometry, common-origin Y-up assembly.
- Unique modeled ECC bit-wall, memory modules, 4-chiplet computer, fictional particle storm apparatus, transparent observatory, 3 redundant computing towers, physical voting cube, recovery vault, command gate, CRC chain, mission screen and safety equipment.
- All 52 named pivots are **real mesh-owning transform parents**, not a CSV-only fiction.
- 200 gold-rimmed terminal sockets are visual meshes with accurate CSV art positions; do not mistake them for working SparkTerminal objects.
- Procedural preview PBR textures and material upgrade helper; these are not final baked artist-authored surface textures.
- Unity art-placement, material and validator helpers; a visual rig with simulation-driven methods (no actual gameplay logic).

## Limitations
- 130-wire CSV is a connection concept and terminal sharing/compatibility must be resolved through the actual Project Spark electrical API.
- No actual SECDED codec, memory fault physics, TMR supervisor, radiation simulation, mission gameplay, safety interlocks or 6/6 evaluator is included.
- High individual mesh-part count: optimize/LOD/batch before shipping on constrained devices.
- FBX numeric geometry is centimetres; GLB numeric geometry is metres. Unity import scale conversion must be applied exactly once, then verify 24-metre total model width.
- Simulated particles and binary-error effects are fictional holograms, not real radiation emitters.
- Do not instantiate the assembled showcase together with 70 modular models.
''',encoding='utf8')
print('Level81 planning ready: ',len(rows),'wires',len(sockets),'visual socket anchors',len(objectives),'objectives')
