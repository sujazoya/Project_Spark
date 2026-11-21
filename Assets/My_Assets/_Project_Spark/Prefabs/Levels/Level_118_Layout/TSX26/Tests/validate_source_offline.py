"""Offline source-only checks; does NOT compile Unity, certify electronics or play game."""
from pathlib import Path
import sys, csv, json, struct, hashlib

def check(root):
    root = Path(root).resolve()
    get=lambda n:list(csv.DictReader(open(root/'Data'/n,encoding='utf-8-sig',newline='')))
    models=get('ModelManifest.csv')
    parts=get('ComponentBOM.csv')
    placements=get('ComponentPlacements.csv')
    routes=get('CopperRoutes_320_REQUIRES_SCHEMATIC.csv')
    vias=get('ViaTasks_116_REQUIRES_SCHEMATIC.csv')
    assertions=[]
    def assert_that(label,condition):
        assertions.append({'name':label,'passed':bool(condition)})
        if not condition: print('FAIL', label)
    assert_that('288 manifest models',len(models)==288)
    assert_that('236 BOM and placement rows',len(parts)==len(placements)==236)
    assert_that('320 route rows',len(routes)==320)
    assert_that('116 via rows',len(vias)==116)
    assert_that('unique model IDs',len(set(r['asset_id'] for r in models))==288)
    assert_that('unique BOM refs',len(set(r['ref'] for r in parts))==236)
    assert_that('correct component ownership',all(r['board']=='TSX-26' for r in parts+placements))
    assert_that('valid OBJ filename field',all(r['obj'].endswith('.obj') and r['obj']!='NOT_INCLUDED' for r in models))
    assert_that('288 matching OBJ paths',all((root/'Models_OBJ'/r['obj']).is_file() for r in models))
    assert_that('288 matching MTL paths',all((root/'Models_OBJ'/r['obj'].replace('.obj','.mtl')).is_file() for r in models))
    assert_that('288 matching GLB paths',all((root/'Models_GLB'/r['glb']).is_file() for r in models))
    assert_that('exactly 289 OBJ / GLB including reference',len(list((root/'Models_OBJ').glob('*.obj')))==len(list((root/'Models_GLB').glob('*.glb')))==289)
    assert_that('289th assembled visual pair',all((root/p/('289_L118_AssembledReference'+ext)).is_file() for p,ext in [('Models_OBJ','.obj'),('Models_GLB','.glb')]))
    assert_that('236 installable assets',sum(r['installable']=='TRUE' for r in models)==236)
    assert_that('52 structural/inspection assets',sum(r['installable']=='FALSE' for r in models)==52)
    assert_that('BOM/ref/asset joins',set(r['ref'] for r in parts)==set(r['ref'] for r in placements) and all(any(x['asset_id']==r['asset_id'] and x['installable']=='TRUE' for x in models) for r in placements))
    assert_that('all uninstalled unverified',all(r['installed_at_start']==r['footprint_verified']=='FALSE' for r in placements))
    assert_that('route IDs and authority blocked',all(r['id']==f'W{i:03d}' and r['source_pad']==r['dest_pad']==r['net_id']=='UNASSIGNED' and r['certifiable']=='FALSE' for i,r in enumerate(routes,1)))
    assert_that('via IDs and authority blocked',all(r['id']==f'V{i:03d}' and r['net_id']=='UNASSIGNED' and r['start_layer']==r['end_layer']=='TBD' and r['verified']=='FALSE' for i,r in enumerate(vias,1)))
    assert_that('34 four-map texture families',len(list((root/'Textures').glob('*_BaseColor.png')))==34 and len(list((root/'Textures').glob('*_Normal.png')))==34 and len(list((root/'Textures').glob('*_AO.png')))==34 and len(list((root/'Textures').glob('*_MetalRoughAO.png')))==34)
    assert_that('34 URP packed maps',len(list((root/'URP_Packed').glob('*_URP_MetallicSmoothness.png')))==34)
    assert_that('W321 OPEN with no approved pins',json.loads((root/'Data'/'W321_Recovery.json').read_text())['initial']=='OPEN' and json.loads((root/'Data'/'W321_Recovery.json').read_text())['physical_pins']=='UNASSIGNED')
    assert_that('100 acceptance tests NOT_RUN',len(get('AcceptanceTests_100.csv'))==100 and all(r['status']=='NOT_RUN' for r in get('AcceptanceTests_100.csv')))
    assert_that('Unity code present',all((root/'Unity'/p).exists() for p in ['Editor/SparkL118FullLayoutBuilder.cs','Editor/SparkL118URPPBRMaterialBuilder.cs','Runtime/SparkL118LayoutRecords.cs','Runtime/SparkL118TSNTeachingDemo.cs']))
    shader=(root/'Unity/Editor/SparkL118FullLayoutBuilder.cs').read_text()
    assert_that('builder expects correct count and paths','models.Count != 288' in shader and 'parts.Count != 236' in shader and 'm[5].EndsWith(".obj"' in shader and 'NOT_INCLUDED' in shader and 'L118_PCB_Main' in shader)
    # Board GLB: glTF 2.0 header and vertex bounds from OBJ reliable across readers.
    p=next(root/'Models_OBJ'/r['obj'] for r in models if r['asset_id']=='L118_PCB_Main')
    minx=minz=float('inf');maxx=maxz=float('-inf');miny=float('inf');maxy=float('-inf')
    count=0
    for line in p.open():
        if line.startswith('v '):
            v=line.split();x,y,z=map(float,v[1:4]);minx=min(minx,x);maxx=max(maxx,x);minz=min(minz,z);maxz=max(maxz,z);miny=min(miny,y);maxy=max(maxy,y);count+=1
    assert_that('PCB OBJ has source mesh vertices',count>100)
    assert_that('board exact 480x340mm',abs((maxx-minx)-.48)<.0001 and abs((maxz-minz)-.34)<.0001)
    assert_that('PCB material substrate >=1.8mm',(maxy-miny)>.00179)
    summary={'source_kind':'OFFLINE_FILE_AND_GEOMETRY_ONLY','unity_compiled':False,'playmode_tested':False,'approved_schematic':False,'checks_passed':sum(a['passed'] for a in assertions),'checks_total':len(assertions),'all_pass':all(a['passed'] for a in assertions),'checks':assertions}
    (root/'Documentation'/'OFFLINE_VALIDATION.json').write_text(json.dumps(summary,indent=2),encoding='utf-8')
    print(json.dumps({k:v for k,v in summary.items() if k!='checks'},indent=2))
    return summary['all_pass']
if __name__=='__main__':sys.exit(0 if check(Path(__file__).parent.parent) else 1)
