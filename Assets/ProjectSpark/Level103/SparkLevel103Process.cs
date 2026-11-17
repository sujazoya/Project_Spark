using System;
using System.Collections.Generic;
using System.Linq;
using AAAUI.VFX;
using ProjectSpark.Circuit;
using ProjectSpark.PCBVirtualStudio;
using ProjectSpark.SensorMatrix;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.Gameplay
{
    public sealed partial class SparkLevel103Process : MonoBehaviour,ISparkPCBRepairAuthority
    {
        public static readonly string[] Gates={"L103_Assembly","L103_Routing","L103_Calibration","L103_Acquisition","L103_Repairs","L103_Recovery"};
        [Serializable] public sealed class Part
        {
            public GameObject art;
            public SparkVirtualPCBTarget target;
            public SparkSwitch[] joints;
            public SparkElectricalComponent[] devices;
        }
        [Serializable] public sealed class Route
        {
            public SparkTerminal from,to,viaTop,viaBottom;
            public SparkSwitch via;
            public LineRenderer top,bottom;
        }
        public SparkLevel103Design design;
        public SparkLevelLayout layout;
        public Transform arena,art;
        public Part[] parts;
        public Route[] routes;
        public SparkPowerSupply certification,supply,logic,reference,supervisor,safetyLamp;
        public SparkPowerSupply[] excitations,sensorOutputs,frontEnds;
        public SparkTerminal ground,protectedInput,logicInput,referenceInput,supervisorInput,trustedInput;
        public SparkTerminal[] sensorInputs,minusInputs,adcInputs,componentReturns;
        public SparkResistor opticalFeedback;
        public SparkLevel103Effects effects;
        public Text heading,readings,detail;
        public SparkVirtualPCBWorkstation workstation;
        public SparkSensorMatrixAcquisition acquisition=new();
        public List<SparkLevel103Evidence> evidence=new();

        private LevelGamePlayManager manager;
        private SparkLevelLayoutLoader loader;
        private SparkLevel103BoardView cameraView;
        private SignalWireBuilder wireBuilder;
        private readonly List<SparkCircuitConnection> connections=new();
        private bool[] placed,soldered,flux,hot;
        private bool[] repairProved=new bool[3];
        private int selectedPart,selectedRoute,orientation,stage,revision,fault=-1,pendingRepair=-1;
        private bool diagnosed,armed,lossObserved,latched,recoveryReset,running;
        private int routed,viaCount,runSamples,runRevision;
        private double elapsedRun,runStarted,referenceGoodSince=-1;
        private string notice="Fit a component to its named footprint, then solder its contacts.";
        private float nextPresentation;
        private bool Active=>manager && loader && loader.ActiveLayout==layout;
        public int Stage=>stage;
        public int InstalledCount=>placed==null?0:placed.Count(value=>value);
        public int SolderedCount=>soldered==null?0:soldered.Count(value=>value);
        public int RoutedCount=>routed;
        public int ViaCount=>viaCount;
        public bool SafetyLatched=>latched;
        public bool Trusted=>trusted;
        public bool Running=>running;
        public int SelectedRoute=>selectedRoute;

        private void Start()
        {
            manager=FindFirstObjectByType<LevelGamePlayManager>();
            loader=FindFirstObjectByType<SparkLevelLayoutLoader>();
            placed=new bool[parts.Length];soldered=new bool[parts.Length];
            flux=new bool[parts.Length];hot=new bool[parts.Length];
            foreach(var part in parts)
            {
                part.art.SetActive(false);
                foreach(var joint in part.joints)joint.SetState(SparkSwitchState.Open);
                foreach(var device in part.devices)device.SetElectricalEnabled(false);
                workstation.AddTarget(part.target);
                part.target.SetWorkstation(workstation);
            }
            workstation.BindAuthority(this);
            cameraView=gameObject.AddComponent<SparkLevel103BoardView>();
            cameraView.Initialize(arena);
            BoardView();
            wireBuilder=FindFirstObjectByType<SignalWireBuilder>();
            if(wireBuilder)
            {
                wireBuilder.TerminalPickerOverride=PickPad;
                wireBuilder.TerminalWireConnected+=OnManualWire;
            }
            Present();
        }

        private void OnDisable()
        {
            if(!wireBuilder)return;
            wireBuilder.TerminalWireConnected-=OnManualWire;
            if(wireBuilder.TerminalPickerOverride!=PickPad)return;
            if(wireBuilder.IsDrawing)wireBuilder.CancelWire();
            wireBuilder.TerminalPickerOverride=null;
        }

        private bool Edge(SparkTerminal from,SparkTerminal to)=>connections.Exists(edge=>
            edge.Kind==SparkConnectionKind.Wire && (edge.A==from && edge.B==to || edge.A==to && edge.B==from));

        private bool RouteReady(int index)
        {
            var route=routes[index];
            return route.via ? route.via.IsClosed && Edge(route.from,route.viaTop) && Edge(route.viaBottom,route.to) :
                Edge(route.from,route.to);
        }

        private void ReadGraph()
        {
            connections.Clear();
            manager.CircuitSystem.CopyConnections(connections);
            routed=viaCount=0;
            for(int i=0;i<routes.Length;i++)
            {
                bool connected=RouteReady(i);
                if(i<58 && connected)routed++;
                if(routes[i].via && routes[i].via.IsClosed)viaCount++;
                routes[i].top.enabled=connected;
                if(routes[i].bottom)routes[i].bottom.enabled=connected;
            }
        }

        public void FitPart()
        {
            if(!Active || supply.IsOutputActive || placed[selectedPart])return;
            if(orientation!=0){notice="Pin 1 orientation is wrong. Rotate until the mark aligns.";return;}
            placed[selectedPart]=true;
            parts[selectedPart].art.SetActive(true);
            revision++;
            notice=design.parts[selectedPart].reference+" placed. Apply flux and solder its contacts.";
            effects.Emit(parts[selectedPart].target.transform.position,0);
        }

        public void NextPart()
        {
            for(int i=1;i<=parts.Length;i++)
            {
                int next=(selectedPart+i)%parts.Length;
                if(!soldered[next]){selectedPart=next;break;}
            }
            orientation=0;
            Highlight();
        }

        public void RotatePart(){orientation=(orientation+1)%4;notice="Pin 1 rotation "+orientation*90+"°";}
        public void FluxPart()=>TryApplyRepair(parts[selectedPart].target,SparkVirtualTool.Flux,out notice);
        public void SolderPart()=>TryApplyRepair(parts[selectedPart].target,SparkVirtualTool.Solder,out notice);

        private void Highlight()
        {
            if(cameraView && stage==0)
                cameraView.UseSocketView(parts[selectedPart].target.transform.position-Vector3.right*.16f,
                    parts[selectedPart].target.transform.position+Vector3.right*.16f);
        }

        public void NextRoute(){selectedRoute=(selectedRoute+1)%routes.Length;}
        public void PreviousRoute(){selectedRoute=(selectedRoute+routes.Length-1)%routes.Length;}
        public void PlateVia()
        {
            if(!Active || supply.IsOutputActive || stage<1)return;
            var route=routes[selectedRoute];
            if(!route.via){notice="This route stays on the front layer.";return;}
            route.via.SetState(SparkSwitchState.Closed);
            revision++;
            effects.Emit(route.viaTop.transform.position,0);
            notice=design.routes[selectedRoute].via+" plated. Route both layer segments.";
        }

        private bool Connect(SparkTerminal from,SparkTerminal to,out ulong id)
        {
            id=0;
            if(Edge(from,to))return true;
            if(!manager.CircuitSystem.TryCreateConnection(from,to,SparkConnectionKind.Wire,
                SparkConnectionDirection.Bidirectional,out var edge))return false;
            id=edge.Id;
            return true;
        }

        public void RouteCopper()
        {
            if(!Active || stage<1 || supply.IsOutputActive && selectedRoute!=58)
            {notice="Isolate bench power before changing copper.";return;}
            ReadGraph();
            var route=routes[selectedRoute];
            if(RouteReady(selectedRoute))return;
            if(route.via && !route.via.IsClosed){notice="Plate "+design.routes[selectedRoute].via+" first.";return;}
            bool existing=Edge(route.from,route.via?route.viaTop:route.to);
            if(!Connect(route.from,route.via?route.viaTop:route.to,out ulong first))
            {notice="The circuit rejected the first copper segment.";return;}
            if(route.via && !Connect(route.viaBottom,route.to,out _))
            {
                if(!existing && first!=0)manager.CircuitSystem.RemoveConnection(first);
                notice="The second segment failed. New first segment rolled back.";return;
            }
            CompleteRoute();
        }

        private void CompleteRoute()
        {
            revision++;
            ReadGraph();
            effects.Emit(routes[selectedRoute].to.transform.position,1);
            notice=design.routes[selectedRoute].id+" electrically connected.";
            if(selectedRoute<58)
                for(int i=1;i<=59;i++)
                {
                    int next=(selectedRoute+i)%59;
                    if(!RouteReady(next)){selectedRoute=next;break;}
                }
        }

        public void UndoCopper()
        {
            if(!Active || supply.IsOutputActive && selectedRoute!=58)return;
            ReadGraph();
            var route=routes[selectedRoute];
            foreach(var edge in connections.ToArray())
                if(edge.Kind==SparkConnectionKind.Wire &&
                    (edge.A==route.from && edge.B==(route.via?route.viaTop:route.to) ||
                     edge.B==route.from && edge.A==(route.via?route.viaTop:route.to) ||
                     route.via && (edge.A==route.viaBottom && edge.B==route.to || edge.B==route.viaBottom && edge.A==route.to)))
                    manager.CircuitSystem.RemoveConnection(edge.Id);
            revision++;
            ReadGraph();
        }

        private bool Pending(out SparkTerminal from,out SparkTerminal to)
        {
            from=to=null;
            if(!Active || stage<1 || supply.IsOutputActive && selectedRoute!=58)return false;
            var route=routes[selectedRoute];
            if(route.via && !route.via.IsClosed)return false;
            from=route.via && Edge(route.from,route.viaTop)?route.viaBottom:route.from;
            to=from==route.viaBottom?route.to:route.via?route.viaTop:route.to;
            return !Edge(from,to);
        }

        private SparkTerminal PickPad(Vector2 pointer,SparkTerminal start)
        {
            if(!Pending(out var from,out var to) || !Camera.main)return null;
            float radius=Mathf.Clamp(Screen.height*.017f,14,27);
            var a=Camera.main.WorldToScreenPoint(from.transform.position);
            var b=Camera.main.WorldToScreenPoint(to.transform.position);
            float first=a.z>0?((Vector2)a-pointer).sqrMagnitude:float.PositiveInfinity;
            float second=b.z>0?((Vector2)b-pointer).sqrMagnitude:float.PositiveInfinity;
            if(Mathf.Min(first,second)>radius*radius)return null;
            var found=first<=second?from:to;
            return found==start?null:found;
        }

        private void OnManualWire(SparkTerminal from,SparkTerminal to,SignalPath path)
        {
            var route=routes[selectedRoute];
            if((from==route.from && (to==route.to || to==route.viaTop)) ||
                (to==route.from && (from==route.to || from==route.viaTop)) ||
                route.via && (from==route.viaBottom && to==route.to || to==route.viaBottom && from==route.to))
                CompleteRoute();
        }

        private void OnGUI()
        {
            if(Event.current.type!=EventType.Repaint || !Pending(out var from,out var to) || !Camera.main)return;
            var color=GUI.color;
            foreach(var pin in new[]{from,to})
            {
                var point=Camera.main.WorldToScreenPoint(pin.transform.position);
                if(point.z<=0)continue;
                GUI.color=pin==from?new Color(1,.72f,.2f):Color.cyan;
                GUI.DrawTexture(new Rect(point.x-6,Screen.height-point.y-6,12,12),Texture2D.whiteTexture);
                GUI.Label(new Rect(point.x-100,Screen.height-point.y-34,200,24),pin.name);
            }
            GUI.color=color;
        }

        public void BoardView()
        {
            if(!cameraView)return;
            var board=art.Find("L96_SMX4_PCB_140x95mm_Empty");
            var renderers=board.GetComponentsInChildren<Renderer>();
            var bounds=renderers[0].bounds;
            foreach(var item in renderers)bounds.Encapsulate(item.bounds);
            cameraView.UseSocketView(bounds.center-Vector3.right*bounds.extents.x*1.4f,
                bounds.center+Vector3.right*bounds.extents.x*1.4f);
        }
        public void PadView(){if(cameraView)cameraView.UseSocketView(routes[selectedRoute].from.transform.position,routes[selectedRoute].to.transform.position);}
        public void Showcase(){if(cameraView)cameraView.UseShowcaseView();}

        public bool TryReadProbe(SparkVirtualPCBTarget target,SparkVirtualTool tool,out string reading)
        {
            int part=Array.FindIndex(design.parts,item=>item.reference==target.TargetId);
            int channel=part>=0 && design.parts[part].reference.StartsWith("S")?int.Parse(design.parts[part].reference.Substring(1))-1:-1;
            reading=channel>=0 ? ChannelText(channel) : target.TargetId+" / "+(part>=0&&soldered[part]?"SOLDERED":"CONTACT OPEN");
            if(target.TargetId=="U6")reading=$"Independent reference probe {measuredReference:F5} V; nominal 2.50000 V.";
            if(fault>=0 && target.TargetId==(fault==0?"U6":fault==1?"U3":"U4"))
            {
                diagnosed=true;
                reading+=" Diagnosis "+new[]{"reference drift","V02 open continuity","optical gain saturation"}[fault]+".";
            }
            return true;
        }

        public bool TryApplyRepair(SparkVirtualPCBTarget target,SparkVirtualTool tool,out string message)
        {
            int index=Array.FindIndex(design.parts,item=>item.reference==target.TargetId);
            message="";
            if(index<0){message="Unrecognized board target.";return false;}
            if(SparkPCBToolCatalog.IsDiagnostic(tool))
            {
                TryReadProbe(target,tool,out message);
                notice=message;
                return true;
            }
            if(supply.IsOutputActive){message="Isolate the 5 V supply before soldering or rework.";notice=message;return false;}
            if(tool==SparkVirtualTool.Flux){flux[index]=true;message="Flux applied at "+target.TargetId+".";return true;}
            if(tool==SparkVirtualTool.HotAir){hot[index]=true;message="Removal preparation accepted.";return true;}
            if(tool==SparkVirtualTool.Solder || tool==SparkVirtualTool.Reflow)
            {
                if(!placed[index]||!flux[index]){message="Place the part and apply flux first.";return false;}
                soldered[index]=true;
                foreach(var device in parts[index].devices)device.SetElectricalEnabled(true);
                foreach(var joint in parts[index].joints)joint.SetState(SparkSwitchState.Closed);
                revision++;NextPart();
                message=target.TargetId+" solder joints electrically closed.";
                effects.Emit(target.transform.position,0);
                notice=message;
                return true;
            }
            if(tool==SparkVirtualTool.Desolder)
            {
                foreach(var joint in parts[index].joints)joint.SetState(SparkSwitchState.Open);
                foreach(var device in parts[index].devices)device.SetElectricalEnabled(false);
                soldered[index]=false;revision++;
                message="Contacts physically isolated.";return true;
            }
            bool correctTarget=fault>=0 && target.TargetId==(fault==0?"U6":fault==1?"U3":"U4");
            bool correctTool=fault==0 && tool==SparkVirtualTool.ReplaceComponent && hot[index] ||
                fault==1 && tool==SparkVirtualTool.ViaRepair || fault==2 && tool==SparkVirtualTool.ReplaceComponent && hot[index];
            if(!correctTarget||!correctTool||!diagnosed){message="Diagnose the active defect and use its matching repair tool.";return false;}
            pendingRepair=fault;
            if(fault==1)routes[31].via.SetState(SparkSwitchState.Closed);
            if(fault==2)opticalFeedback.SetResistance(47000);
            fault=-1;revision++;acquisition.InvalidateCalibration();
            message="Physical defect repaired. Recheck reference, recalibrate, and run fresh qualification.";
            effects.Emit(target.transform.position,0);
            notice=message;
            return true;
        }

        public void Diagnose()
        {
            if(fault<0){notice="No injected fault. Use the probe on a component.";return;}
            var target=parts[Array.FindIndex(design.parts,item=>item.reference==(fault==0?"U6":fault==1?"U3":"U4"))].target;
            TryReadProbe(target,SparkVirtualTool.Multimeter,out notice);
        }
        public void Repair()
        {
            if(fault<0)return;
            int index=Array.FindIndex(design.parts,item=>item.reference==(fault==0?"U6":fault==1?"U3":"U4"));
            if(fault!=1)TryApplyRepair(parts[index].target,SparkVirtualTool.HotAir,out _);
            TryApplyRepair(parts[index].target,fault==1?SparkVirtualTool.ViaRepair:SparkVirtualTool.ReplaceComponent,out notice);
        }

        private void Award(string proof,int samples=0,double began=0)
        {
            if(stage>=6)return;
            var record=new SparkLevel103Evidence {objective=stage+1,topologyRevision=manager.CircuitSystem.TopologyVersion,
                systemRevision=revision,samples=samples,timestamp=acquisition.clock,intervalStart=began,proof=proof};
            if(!loader.ActiveBindings.SetRuntimeComponentBinding(Gates[stage],certification))return;
            evidence.Add(record);stage++;manager.QueueEvaluation();
            effects.Emit(parts[0].target.transform.position,3);
        }

        private string ChannelText(int index)
        {
            var c=acquisition.channels[index];
            string value=c.status==SensorSampleStatus.Valid?c.value.ToString("F2")+" "+c.unit:"—";
            return $"CH{index} {value} | ADC {c.rawCode} | {c.voltage:F4} V | {c.status}";
        }

        private void Present()
        {
            heading.text=$"SENSOR MATRIX / LEVEL 103 / {stage}/6   |   {InstalledCount}/44 PARTS   {SolderedCount}/44 SOLDERED   {routed}/58 COPPER   {viaCount}/12 VIAS";
            readings.text=string.Join("     ",Enumerable.Range(0,4).Select(ChannelText));
            string task=stage==0?$"{design.parts[selectedPart].reference}  {design.parts[selectedPart].value}  PIN 1 {orientation*90}°":
                $"{design.routes[selectedRoute].id}: {routes[selectedRoute].from.name} > {routes[selectedRoute].to.name} [{design.routes[selectedRoute].via}]";
            detail.text=$"{task}\nREF {measuredReference:F5} V   TRUST {(trusted?"VALID":"INVALID")}   SAFETY {(latched?"LATCHED":"READY")}   {notice}";
        }
    }
}