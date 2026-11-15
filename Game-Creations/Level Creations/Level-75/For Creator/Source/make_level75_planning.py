#!/usr/bin/env python3
"""Generate clearly nonfunctional Level75 conceptual wire and terminal-anchor planning CSVs."""
from pathlib import Path
import json,csv,math
ROOT=Path('/mnt/data/ProjectSpark_Level75_HD_AAA')
MAN=json.loads((ROOT/'Model_Manifest.json').read_text())
models=MAN['models']
# Pairs refer to actual *model* IDs, not existing SparkTerminal IDs (these must be implemented in Unity).
groups=[
('Control power / returns',[
(57,53,'Main PLC 5V supply'),(57,54,'Power isolator 5V logic'),(57,43,'Dose controller 5V supply'),(57,40,'Beam safety relay 5V supply'),(57,56,'SCADA 5V logic'),(57,17,'Beam sensor 5V supply'),(57,18,'Faraday sensor 5V supply'),(57,35,'Pressure instrument 5V supply'),(57,42,'Species analyzer 5V logic'),(57,55,'Emergency station control supply')]),
('Ion source / beam-power',[
(53,2,'Virtual source enable'),(54,7,'Beam power domain enable'),(7,6,'Extraction power request'),(3,2,'Virtual source cartridge available'),(4,2,'Source chamber readiness'),(5,2,'Plasma emission feedback'),(6,53,'Extraction status feedback'),(2,53,'Source current feedback'),(54,5,'Emission permission'),(40,54,'Safety domain permission'),(2,7,'Source power feedback'),(53,5,'Source intensity command'),(7,53,'Power domain fault'),(2,17,'Transport entrance state'),(53,6,'Extraction stage enable'),(7,40,'High-power isolation status')]),
('Mass analyzer / species',[
(53,8,'Analyzer magnetic field command'),(8,42,'Analyzer field feedback'),(42,53,'Species purity measurement'),(53,9,'Resolving aperture command'),(9,42,'Aperture position feedback'),(10,42,'Energy selection response'),(53,10,'Energy selector command'),(12,42,'Neutral fraction diagnostic'),(42,43,'Selected species quality'),(42,40,'Species interlock'),(17,8,'Selected beam entrance current'),(8,17,'Analyzer transmitted current'),(53,42,'Species calibration request'),(42,56,'Purity status telemetry')]),
('Beam focusing / scanning',[
(53,13,'Focusing optics enable'),(53,14,'Beam steer command'),(13,41,'Focus quality response'),(14,41,'Beam centroid response'),(53,15,'XY scan enable'),(15,44,'Actual scan position'),(53,16,'Beam blanking command'),(16,40,'Blanker position feedback'),(41,53,'Profile scan valid'),(17,43,'Beam current estimate'),(53,41,'Profiler scan request'),(41,56,'Beam shape telemetry'),(54,16,'Isolation override to blanker'),(15,43,'Scan dwell integral')]),
('Wafer endstation / handling',[
(53,28,'Wafer robot motion request'),(28,53,'Wafer robot ready'),(53,30,'Loadlock chamber control'),(53,31,'Gate valve command'),(31,40,'Gate position interlock'),(53,22,'Wafer clamp enable'),(22,40,'Wafer clamp status'),(53,23,'Wafer angle command'),(23,44,'Wafer tilt feedback'),(52,53,'Wafer registration measurement'),(25,52,'Patterned mask presence'),(20,40,'Neutralization readiness')]),
('Dosimetry / metrology',[
(18,43,'CRITICAL Faraday cup current feedback'),(17,43,'Upstream beam current'),(19,44,'Corner cups uniformity'),(43,53,'Dose reached event'),(44,53,'Wafer dose uniformity result'),(41,44,'Beam profile map'),(42,43,'Species-corrected beam dose'),(45,40,'Wafer surface potential'),(46,53,'Crystal damage warning'),(50,53,'Sheet resistance result'),(51,53,'Implant depth profile result'),(53,51,'Depth metrology start'),(53,50,'Electrical metrology start'),(43,56,'Measured dose telemetry')]),
('Vacuum / thermal / activation',[
(53,33,'Backing pump enable'),(53,32,'Turbomolecular pump enable'),(35,53,'Chamber pressure feedback'),(53,34,'Vacuum throttle position command'),(36,53,'Residual gas health'),(37,40,'Wafer temperature safety'),(53,38,'Wafer cooling request'),(53,47,'Activation chamber start'),(48,53,'Heater power status'),(49,53,'Activation temperature feedback')]),
('PLC / safety / SCADA',[
(40,53,'Safety-ready permission'),(55,40,'Emergency stop signal'),(53,55,'Emergency station status command'),(54,53,'Power isolation ready'),(53,54,'Power isolation request'),(53,56,'PLC machine telemetry'),(56,53,'Operator command'),(43,40,'Dose controller inhibit signal'),(44,40,'Dose-uniformity interlock'),(45,53,'Charge sensor diagnostic'),(46,56,'Damage telemetry'),(47,53,'Thermal activation result'),(49,40,'Activation temperature overlimit'),(51,56,'Junction depth metrology telemetry'),(18,40,'Faraday current validity for safety'),(40,16,'Safety-driven beam blank request')])
]
assert [len(rows) for _,rows in groups]==[10,16,14,14,12,14,10,16]
with (ROOT/'Gameplay_Wiring_Concept_W01_W106.csv').open('w',newline='',encoding='utf-8') as f:
    w=csv.writer(f);w.writerow(['WireID','Group','SourceModel','TargetModel','DesignSignal','Status','CriticalNote'])
    i=0
    for g,rows in groups:
        for src,dst,desc in rows:
            i+=1
            w.writerow([f'W{i:03d}',g,models[src]['name'],models[dst]['name'],desc,
             'DESIGN_ONLY_NOT_WORKING_WIRE',
             'Actual Source/Target SparkTerminal IDs, signal compatibility and topology MUST be implemented in Unity'])
assert i==106
# 164 nonfunctional art-space anchor location proposals, distributing among 56 functional models.
# They do NOT claim 164 terminal GameObjects are present in the FBX exports.
indices=list(range(2,58))
termCounts={i:2 for i in indices}
for i in [2,7,8,13,14,15,16,17,18,19,20,21,22,23,32,33,34,35,37,40,42,43,44,45,47,48,49,50,51,52,53,54,55,56,57,3,5,6,9,10,11,12,25,28,30,31,36,39,41,46,24,26]:
    termCounts[i]+=1
assert sum(termCounts.values())==164,sum(termCounts.values())
with (ROOT/'Terminal_Anchor_Plan_164.csv').open('w',newline='',encoding='utf-8') as f:
    w=csv.writer(f);w.writerow(['PlannedAnchorID','Model','WorldX_m','WorldY_m','WorldZ_m','Purpose','Status'])
    seq=0
    for idx in indices:
        m=models[idx];lo=m['bbox_min'];hi=m['bbox_max'];count=termCounts[idx]
        # Place anchors on a design-guide strip above the front-facing, accessible portion of each bbox.
        # Unity developer must reposition to safe physically accessible equipment panels.
        x0=(lo[0]+hi[0])/2;top=hi[1];front=hi[2]
        span=min(max((hi[0]-lo[0])*.58,.18),1.15)
        for j in range(count):
            seq+=1
            x=x0+(j-(count-1)/2)*span/max(count,1)
            y=min(top-.04,max(lo[1]+.12, (lo[1]+hi[1])*.5))
            z=front+.095
            w.writerow([f'P{seq:03d}',m['name'],round(x,4),round(y,4),round(z,4),
                'Suggested terminal location on accessible panel; replace with verified terminal design',
                'PROPOSED_ONLY_NO_SPARKTERMINAL_OBJECT'])
assert seq==164
print('Wiring concept 106 rows; anchor plan 164 rows, NOT gameplay')
