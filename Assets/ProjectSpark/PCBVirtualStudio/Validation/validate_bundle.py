from pathlib import Path
import re,json,zipfile,hashlib
root=Path(__file__).parents[1]
runtime=root/'Assets/ProjectSpark/PCBVirtualStudio/Runtime'
editor=root/'Assets/ProjectSpark/PCBVirtualStudio/Editor'
shader=root/'Assets/ProjectSpark/PCBVirtualStudio/Shaders/AAA_Hologram_UI.shader'
checks=[]
def check(title,condition):
    checks.append((title,bool(condition)))
    if not condition: print('FAIL:',title)
    else: print('PASS:',title)
check('Runtime scripts >= 10',len(list(runtime.glob('*.cs')))>=10)
check('Editor tool scripts >= 2',len(list(editor.glob('*.cs')))>=2)
check('Hologram shader exists and named',shader.exists() and 'ProjectSpark/PCB/AAA Hologram UI' in shader.read_text())
factory=(runtime/'SparkPCBStudioFactory.cs').read_text()
types=(runtime/'SparkPCBVirtualTypes.cs').read_text()
work=(runtime/'SparkPCBVirtualWorkstation.cs').read_text()
vfx=(runtime/'SparkPCBTouchVFX.cs').read_text()
bridge=(runtime/'SparkPCBPhysicalClickBridge.cs').read_text()
check('World-space Canvas builder','RenderMode.WorldSpace' in factory and 'GraphicRaycaster' in factory)
check('All 21 enum tools',len(re.findall(r'\b(?:Inspect|Multimeter|Continuity|Oscilloscope|LogicAnalyzer|ThermalCamera|NetTracer|FaultScanner|Microscope|Flux|Solder|Desolder|Reflow|HotAir|TracePatch|ViaRepair|ReplaceComponent|Clean|BGAReball|SolderMask|Calibrate)\b',types.split('public enum SparkVirtualTool')[1].split('}')[0])) ==21)
check('16 default board targets',len(re.findall(r'new TargetDefinition\(',factory))==16)
check('UI touch and click via Unity Button','button.onClick.AddListener' in (runtime/'SparkVirtualPCBTarget.cs').read_text())
check('Physical PCB click bridge','IPointerClickHandler' in bridge and 'PhysicsRaycaster' in bridge)
check('VFX at target click positions','SpawnParticles' in vfx and 'PlayAtWorld' in vfx and 'SpawnRing' in vfx)
check('Live authority interface', 'interface ISparkPCBRepairAuthority' in types and 'TryApplyRepair' in types and 'TryReadProbe' in types)
check('No automatic electrical claims', 'No ISparkPCBRepairAuthority' in work and 'DEMO TRAINING' in work and 'NOT circuit-certified' in (runtime/'SparkVirtualPCBTarget.cs').read_text())
check('Sample bridge blocks unverified changes','return false;' in (runtime/'SparkPCBAuthorityAdapterExample.cs').read_text())
check('Toolbar group 3 categories', '"SCAN", "REPAIR", "ADVANCED"' in factory)
check('Procedural oscillator graph', (runtime/'SparkPCBWaveformGraphic.cs').exists() and 'OnPopulateMesh' in (runtime/'SparkPCBWaveformGraphic.cs').read_text())
check('Optional persistent shader menu','AssetDatabase.CreateAsset' in (editor/'SparkPCBStudioShaderInstaller.cs').read_text())
check('Documentation present',len(list((root/'Documentation').glob('*.md')))>=4)
check('No FBX or GLB models included', not list(root.rglob('*.fbx')) and not list(root.rglob('*.glb')))
for f in root.rglob('*.cs'):
    text=f.read_text()
    # simplistic balanced delimiter parse, ignoring comments and quoted text
    stripped=re.sub(r'(?s)/\*.*?\*/|//[^\n]*|@?"(?:\\.|[^"\\])*"|\'(?:\\.|[^\'\\])*\'','',text)
    for op,cl in [('(',')'),('{','}'),('[',']')]:
        if stripped.count(op)!=stripped.count(cl):
            print('UNBALANCED',f.name,op,cl)
            checks.append((f'{f.name} structural delimiters',False))
check('All scripts basic source syntax closure',not any(c[0].endswith('structural delimiters') for c in checks))
result={'project':'ProjectSpark AAA Virtual PCB UI','status':'PASS' if all(v for _,v in checks) else 'FAIL',
        'offline_checks':len(checks),'passed':sum(v for _,v in checks),'failed':sum(not v for _,v in checks),
        'unity_compilation_tested':False, 'real_solver_bound':False,
        'details':[{'name':k,'passed':v} for k,v in checks]}
(root/'Validation'/'Validation_Result.json').write_text(json.dumps(result,indent=2))
print(json.dumps({k:result[k] for k in ['status','offline_checks','passed','failed']},indent=2))
if result['status']!='PASS': raise SystemExit(1)
