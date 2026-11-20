from pathlib import Path
import csv,json,re,sys
source=Path(__file__).resolve().parents[2]/'ProjectSpark_Level110_PWR18_VoltageDominion_AssetKit'
if not source.is_dir():
    print('Source asset kit not in neighboring folder; skipping external audit')
    sys.exit(0)
d=source/'Data'
get=lambda name:list(csv.DictReader((d/name).open(newline='',encoding='utf-8-sig')))
parts=get('L110_Models_168.csv')
routes=get('L110_Routes_172_PLANNING.csv')
vias=get('L110_Vias_56_PLANNING.csv')
assert len(parts)==168 and sum(x['installable']=='True' for x in parts)==130
assert len(routes)==172 and len(vias)==56
assert len(json.loads((d/'L110_Objectives_6.json').read_text()))==6
assert len(json.loads((d/'L110_Faults_3.json').read_text()))==3
print('PASS: Level 110 source records and counts validated (NOT Unity compilation)')
