#!/usr/bin/env python3
"""Manufacturing-style planning data for Project Spark Level 94. This is source data, not compiled circuit logic."""
import json,csv,hashlib,math
from pathlib import Path
import build_level94 as b
R=Path(__file__).resolve().parents[1];D=R/'Data'; D.mkdir(exist_ok=True)
def out(name, rows):
 assert rows
 with (D/name).open('w',newline='',encoding='utf8') as f:
  w=csv.DictWriter(f,fieldnames=list(rows[0]));w.writeheader();w.writerows(rows)
# physical 100 x 70 mm, 2-layer PCB. 14x display scale is limited to artwork.
physical={'Level':94,'BoardName':'Board Diagnostic Controller','BoardWidthMM':100,'BoardLengthMM':70,'ThicknessMM':1.6,'Layers':['F.Cu','B.Cu'],'InputV':5,'LogicV':3.3,'LogicHz':2.0,'ArtScaleMultiplier':14.0,'ArtIsGameplayAuthority':False,'DeclaredCopperTasks':34,'DeclaredVias':6,'RequiredComponentPlacements':27,'CriticalDisconnect':'W035','ElectricalNotes':'Power supply is a virtual protected 5 V source. Higher-energy real tools and hardware are not exposed.'}
(D/'PCB_Physical_Design.json').write_text(json.dumps(physical,indent=2))
# Reconciled wiring: 34 selected routable copper connections plus physical removable diagnostic W035.
arr=[
('J1.5V_IN','D1.A','VIN_RAW','power','F.Cu'),
('D1.K','U2.VIN','+5V_PROT','power','F.Cu'),
('U2.VOUT','U1.VDD','+3V3','power','F.Cu'),
('J1.GND','U2.GND','GND','ground','F.Cu'),
('U2.VOUT','C2.POS','+3V3','power','F.Cu'),
('C2.NEG','J1.GND','GND','ground','F.Cu'),
('U2.VIN','C1.POS','+5V_PROT','power','F.Cu'),
('C1.NEG','J1.GND','GND','ground','F.Cu'),
('U1.VDD','C3.POS','+3V3','power','F.Cu'),
('C3.NEG','J1.GND','GND','ground','F.Cu'),
('U3.VDD','U1.VDD','+3V3','power','F.Cu'),
('U3.GND','J1.GND','GND','ground','F.Cu'),
('U1.SCL','U3.SCL','I2C_SCL','digital','F.Cu'),
('U1.SDA','U3.SDA','I2C_SDA','digital','F.Cu+B.Cu'),
('U1.SCL','R1.A','I2C_SCL','digital','F.Cu'),
('R1.B','U2.VOUT','+3V3','power','F.Cu'),
('U1.SDA','R2.A','I2C_SDA','digital','F.Cu'),
('R2.B','U2.VOUT','+3V3','power','F.Cu+B.Cu'),
('U1.GPIO_LOAD','R3.A','LOAD_GATE_DRIVE','digital','F.Cu'),
('R3.B','Q1.G','MOSFET_GATE','digital','F.Cu'),
('Q1.G','R4.A','MOSFET_GATE','digital','F.Cu+B.Cu'),
('R4.B','J1.GND','GND','ground','F.Cu'),
('Q1.S','R8.B','LOAD_SHUNT','analog','F.Cu'),
('R8.A','J1.GND','GND','ground','F.Cu'),
('Q1.D','LED3.C','LOAD_SWITCHED','power','F.Cu+B.Cu'),
('LED3.A','R7.B','LOAD_DRIVE','power','F.Cu'),
('R7.A','D1.K','+5V_PROT','power','F.Cu'),
('LED1.A','R5.B','PWR_LED_SERIES','power','F.Cu'),
('R5.A','U2.VOUT','+3V3','power','F.Cu'),
('LED1.C','J1.GND','GND','ground','F.Cu+B.Cu'),
('U1.GPIO_STAT','R6.A','STATUS_DRIVE','digital','F.Cu'),
('R6.B','LED2.A','STATUS_LED_SERIES','power','F.Cu'),
('LED2.C','J1.GND','GND','ground','F.Cu'),
('R8.B','U4.SENSE_POS','SHUNT_SENSE_POS','analog','F.Cu+B.Cu')]
assert len(arr)==34
vias={14:('V01',-42,-24),18:('V02',-20,-12),21:('V03',0,25),25:('V04',21,9),30:('V05',41,-18),34:('V06',12,-30)}
rows=[]
for i,(source,target,net,typ,layer) in enumerate(arr,1):
 vr=vias.get(i)
 rows.append(dict(WireID=f'W{i:03d}',FromPad=source,ToPad=target,Net=net,SignalType=typ,RequiredLayer=layer,
 ViaID=vr[0] if vr else '',RequiredClearanceMM='.2',TargetWidthMM='.35' if typ in ('power','ground') else '.2',
 InitialState='MISSING',SimulationAuthority='SparkPCBNetlistValidator+SparkElectricalSolver',QualificationStage='03 Copper Routing'))
out('Copper_Trace_Tasks_W001_W034.csv',rows)
vrrows=[]
for route,(via,x,z) in vias.items():vrrows.append(dict(ViaID=via,RouteID=f'W{route:03d}',Xmm=x,Zmm=z,TopLayer='F.Cu',BottomLayer='B.Cu',HoleDiameterMM=.35,PadDiameterMM=.75,MustBePlayerPlaced='YES',InitialState='MISSING'))
out('Required_Via_Placements_V01_V06.csv',vrrows)
critical=[dict(WireID='W035',Type='DetachableServiceJumper',FromComponent='U4',FromPort='CURRENT_OK_OUT',ThroughPadA='J3.A',ThroughPadB='J3.B',ToComponent='U1',ToPort='CURRENT_OK_IN',VisualModel='L94_W035DetachableFeedbackJumper',InitialState='DISCONNECTED',FaultTest='Disconnect after objectives 1-5 certified',Signal='TRUSTED_CURRENT_OK',SafeState='LOAD_GATE_DISABLED',Reset='Fresh measurement + explicit operator reset',Safety='Never interpret open-feedback as zero current')]
out('Critical_W035_Feedback.csv',critical)
# A defined set of preexisting factory copper connections provides realistic hidden topology.
fixed=[('U4.VDD','+3V3'),('U4.GND','GND'),('U4.SENSE_NEG','GND'),('U4.CURRENT_OK_OUT','J3.A'),('J3.B','U1.CURRENT_OK_IN'),('U3.DEVICE_ADDR','GND'),('U1.VSS','GND'),('U1.RESET_N','SW1.NO1'),('SW1.NO2','GND'),('U1.SWDIO','J2.SWDIO'),('U1.SWCLK','J2.SWCLK'),('J2.GND','GND'),('J2.3V3','+3V3'),('C4.POS','+3V3'),('C4.NEG','GND'),('C5.POS','+3V3'),('C5.NEG','GND'),('C6.POS','U1.RESET_N'),('C6.NEG','GND'),('C7.POS','U1.ADC_IN'),('C7.NEG','GND'),('LED2.C','GND'),('U1.VDDIO','+3V3')]
out('Factory_Existing_Nets.csv',[dict(EndpointA=a,EndpointB=b,Status='PREFABRICATED',Gameplay='NOT A PLAYER ROUTING TASK') for a,b in fixed])
# Net names and circuit domains
netvals={}
for row in rows:netvals[row['Net']]=row['SignalType']
for name,kind in [('TRUSTED_CURRENT_OK','digital'),('U1.RESET_N','digital'),('U1.ADC_IN','analog'),('VDDIO','power')]:netvals[name]=kind
out('Electrical_Net_Domains.csv',[dict(Net=n,SignalKind=k,ControlVoltage='5V' if n.startswith('+5') else '3.3V' if n=='+3V3' else '0V' if n=='GND' else 'SIGNAL',ProtectedDomain='YES' if k=='power' else 'NO') for n,k in sorted(netvals.items())])
# 27-component BOM: true model ownership, designated installation value and orientation.
bom=[]
for r in b.PX.values():
 role='Mandatory component placed by player'
 bom.append(dict(Ref=r['Reference'],PackageKind=r['Kind'],Value=r['Value'],Model=r['Model'],FootprintWidthMM=r['WidthMM'],FootprintLengthMM=r['DepthMM'],Xmm=r['Xmm'],Zmm=r['Zmm'],RotationDegrees=0,InitialInstalled='NO',CorrectPolarityRequired='YES' if r['Kind'] in ('diode','led','ic','soic','ldo','sot') else 'NO',PlacementKind='SnapToFootprint',Qualification='Inspect value+orientation+joint quality'))
out('Component_BOM_27.csv',bom)
# exact pad footprints with a physical pin-position table and logical signal aliases
pin_names={
'U1':{1:'VDD',2:'VSS',3:'SCL',4:'SDA',5:'GPIO_LOAD',6:'GPIO_STAT',7:'CURRENT_OK_IN',8:'RESET_N',9:'SWDIO',10:'SWCLK',11:'ADC_IN',12:'VDDIO',13:'GND'},
'U2':{1:'VIN',2:'GND',3:'VOUT',4:'TAB_GND'},
'U3':{1:'SCL',2:'SDA',3:'VDD',4:'GND',5:'DEVICE_ADDR'},
'U4':{1:'VDD',2:'GND',3:'SENSE_POS',4:'SENSE_NEG',5:'CURRENT_OK_OUT'},
'Q1':{1:'G',2:'S',3:'D'},'D1':{1:'A',2:'K'},
'LED1':{1:'A',2:'C'},'LED2':{1:'A',2:'C'},'LED3':{1:'A',2:'C'},
'SW1':{1:'NO1',2:'NO2',3:'NC1',4:'NC2'},'J1':{1:'5V_IN',2:'GND'},
'J2':{1:'SWDIO',2:'SWCLK',3:'GND',4:'3V3',5:'RESET',6:'UART_TX',7:'UART_RX',8:'KEY',9:'GND2',10:'VREF'}}
for ref in b.PX:
 if ref[0] in ('R','C'):
  pin_names[ref]={1:'A' if ref[0]=='R' else 'POS',2:'B' if ref[0]=='R' else 'NEG'}
counts={'mcu':48,'ldo':4,'soic':8,'sot':3,'diode':2,'led':2,'res':2,'cap':2,'switch':4,'terminal':2,'header':10}
anchors=[]
for ref,r in b.PX.items():
 n=counts[r['Kind']];x=float(r['Xmm']);z=float(r['Zmm']);w=float(r['WidthMM']);h=float(r['DepthMM']);kind=r['Kind']
 for p in range(1,n+1):
  if kind=='mcu':
   side=(p-1)//12;j=(p-1)%12;d=(j-5.5)/11
   xx=x+((-w*.48 if side==0 else w*.48) if side in (0,2) else d*w*.78)
   zz=z+((d*h*.78) if side in (0,2) else (-h*.48 if side==1 else h*.48))
  elif kind=='soic':xx=x+(-w*.48 if p<=4 else w*.48);zz=z+(((p-1)%4)-1.5)*h*.26
  elif kind=='header':xx=x+(-w*.25 if p<=5 else w*.25);zz=z+(((p-1)%5)-2)*h*.16
  elif kind=='switch':xx=x+(-1 if p in (1,3) else 1)*w*.4;zz=z+(-1 if p in (1,2) else 1)*h*.4
  else:xx=x+(-w*.42 if p%2 else w*.42);zz=z
  sig=pin_names.get(ref,{}).get(p,'NC_'+str(p))
  anchors.append(dict(PadID=f'{ref}.{sig}',Ref=ref,PadNumber=p,SignalName=sig,Layer='F.Cu',Xmm=round(xx,4),Zmm=round(zz,4),
   WorldX=round(xx*.014,6),WorldY=round(b.Y+.013,6),WorldZ=round(zz*.014,6),OwnerModel=r['Model'],IsDecorativeMeshOnly='YES',LogicalType='FUNCTIONAL_PAD_TO_CREATE_IN_UNITY'))
for n,(sig,x,z) in enumerate([('A',38,-9),('B',44,-9)],1):anchors.append(dict(PadID=f'J3.{sig}',Ref='J3',PadNumber=n,SignalName=sig,Layer='F.Cu',Xmm=x,Zmm=z,WorldX=x*.014,WorldY=b.Y+.013,WorldZ=z*.014,OwnerModel='L94_W035DetachableFeedbackJumper',IsDecorativeMeshOnly='YES',LogicalType='SERVICE_JUMPER_CONTACT'))
out('Logical_Pad_Locations.csv',anchors)
# trace routing and test criteria
steps=[
('01','Inspect PCB schematic and identify all nets','A valid schematic/netlist inspection state is recorded and no routing is pre-awarded'),
('02','Install all 27 parts','27 real parts in matched footprint, correct value/orientation/polarity; no fake placement'),
('03','Route copper tasks W001-W034 and vias V01-V06','Valid graph connectivity, 2-layer DRC, no shorted nets or open required route'),
('04','Commission board and test','Protected 5 V, valid 3.3 V regulator, microcontroller clock 2 Hz, sensor I2C, load driver and current samples'),
('05','Repair 3 injected faults','Open ground via, incorrect R2 value, solder bridge on Q1; then independent full circuit retest'),
('06','Disconnect and restore critical W035','Real topology break, load inhibit, new current measurement, explicit reset, new full production qualification')]
out('Objective_Certification_Plan_6.csv',[dict(ObjectiveID=i,Title=t,RealEvidence=e,Authority='LevelGamePlayManager via Project Spark actual evaluator',InitialStatus='INCOMPLETE') for i,t,e in steps])
errors=[dict(FaultID='F01',Name='Open Ground Via',Location='V05, W030 return path',ElectricalEffect='Open return / false LED operation',Repair='Rebuild actual via connection',Validation='Net continuity + powered test'),
 dict(FaultID='F02',Name='Incorrect Resistor Value',Location='R2 SDA pull-up',ElectricalEffect='47K instead of required 4K7; slower I2C rise time',Repair='Replace with correct 4K7 component',Validation='Measure I2C edges and acknowledge'),
 dict(FaultID='F03',Name='MOSFET Solder Bridge',Location='Q1 Gate-to-Drain neighboring pads',ElectricalEffect='Unwanted coupling and load-control malfunction',Repair='Remove bridged solder defect, run DRC',Validation='Correct controlled switching and safety cutoff')]
out('Three_Fault_Repair_Scenarios.csv',errors)
# 56 acceptance tests defined, not executed in Unity.
accept=[]
checks=['FBX model imports with nonempty meshes','GLB alternative opens without missing meshes','Material slots mapped to native Lit material','Normal maps are imported as normal maps','Copper pad positions match logical pad CSV','Board display scale does not change physical dimensions','PCB layer controls affect visuals only','Component prefab snaps to correct footprint','Wrong part value rejected','Wrong polarized part orientation rejected','Missing part prevents objective','Pad-to-pad connection creates real solver graph edge','Invalid circuit-net connection rejected','Same-layer incompatible trace crossing rejected','Via joins intended layers only','Missing via invalidates route','Each route begins unconnected','All 34 routes have distinct IDs','All 6 via IDs exist','W035 is a detachable real graph edge','Open W035 invalidates trusted feedback','Open W035 never reports zero current','Open W035 inhibits load enable','W035 repair requires fresh sample','W035 repair requires explicit safety reset','5 V protected input monitors real state','3.3 V regulator responds to load','MCU 2 Hz output reflects power conditions','U3 I2C exchange requires real SDA/SCL','R1/R2 pull-ups affect rise time','Q1 MOSFET control changes load current','R8 current measurement updates from circuit','Voltmeter probe reflects actual circuit','Scope measures signal, not animation','Logic analyzer captures digital edges','Open ground via actually breaks net','Incorrect R2 value degrades signal','Q1 bridge produces physical short','Three faults have unique recovery evidence','Objective 1 inspection evidence validated','Objective 2 placement evidence validated','Objective 3 routing evidence validated','Objective 4 power test evidence validated','Objective 5 fault repair evidence validated','Objective 6 fresh test evidence validated','Only LevelGamePlayManager awards 6/6','Historical certifications retained after final fault','Unity room table preview clones only visual meshes','Preview never clones SparkTerminal or solvers','Gameplay layout preserves Levels 1-93','No fake preconnected wires on level start','Safety interlocks remain effective after fault','Runtime materials load in URP','Runtime materials load in HDRP or documented fallback','Runtime materials load in Built-in','Save/load retains actual component and route states']
assert len(checks)==56
for i,c in enumerate(checks,1):accept.append(dict(TestID=f'L94-AT-{i:03d}',Requirement=c,Result='NOT TESTED - UNITY IMPLEMENTATION REQUIRED'))
out('Engineering_Acceptance_Tests_56.csv',accept)
# minimal selected animation/placement records
(D/'Circuit_Graph_Interface_Contract.json').write_text(json.dumps({'required_interfaces':['SparkTerminal','SparkCircuitSystem','SparkElectricalSolver','LevelGamePlayManager'],'source_status':'INTEGRATION PLAN ONLY - inspect actual user Project Spark assemblies before compiling','nodes':len(anchors),'player_required_routes':34,'physical_critical_jumper':'W035','shared_nets':'Connections on same logical net may join only through compatible solver rules','do_not_use':['decorative wire as circuit','gameplay state from renderers','fake 6/6 result'],'instruments':['DMM','Oscilloscope','Logic Analyzer','Protected Supply'],'safety':['No hot work simulation','Protected 5 V educational input','Independent feedback validation']},indent=2))
print('DATA COMPLETE',json.dumps({'component_placements':len(bom),'trace_tasks':len(rows),'vias':len(vrrows),'pad_anchors':len(anchors),'preexisting_topology_edges':len(fixed),'acceptance_test_definitions':len(accept)},indent=2))
