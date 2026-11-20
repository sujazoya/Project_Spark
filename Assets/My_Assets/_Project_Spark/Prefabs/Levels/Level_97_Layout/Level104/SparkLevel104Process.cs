using System;using System.Collections.Generic;using System.Linq;using System.IO;
using AAAUI.VFX;using ProjectSpark.Circuit;using ProjectSpark.PCBVirtualStudio;using ProjectSpark.VoltageGuardian;
using UnityEngine;using UnityEngine.UI;
namespace ProjectSpark.Gameplay
{
 public sealed partial class SparkLevel104Process:MonoBehaviour,ISparkPCBRepairAuthority
 {
  public static readonly string[] Gates={"L104_Assembly","L104_Routing","L104_Startup","L104_Loads","L104_Repairs","L104_Recovery"};
  [Serializable]public sealed class Part {public GameObject art;public SparkVirtualPCBTarget target;public SparkSwitch[] joints;public SparkElectricalComponent[] devices;}
  [Serializable]public sealed class Segment {public SparkTerminal from,to;public int layer=1;public LineRenderer line;}
  [Serializable]public sealed class Route {public Segment[] segments;public int[] vias=Array.Empty<int>();}
  [Serializable]public sealed class Via {public SparkTerminal top,bottom;public SparkSwitch barrel;public SparkSwitch[] layerTaps=Array.Empty<SparkSwitch>();public Renderer art;}
  public SparkLevel104Design design;public SparkLevelLayout layout;public Transform arena,art;
  public Part[] parts;public Route[] routes;public Via[] vias;
  public SparkPowerSupply certification,supply,mainStage,secondaryStage,coreStage,supervisor;
  public SparkResistor feedbackTop,inputDraw,load;public SparkSwitch safeLoad;
  public SparkTerminal ground,input,mainRail,secondaryRail,coreRail,trustedInput;
  public SparkLevel104Effects effects;public SparkVirtualPCBWorkstation workstation;
  public Text heading,readings,detail;
  public SparkVoltageGuardianPlant plant=new();public List<SparkLevel104Evidence> evidence=new();
  private LevelGamePlayManager manager;private SparkLevelLayoutLoader loader;private SparkLevel104BoardView view;
  private SignalWireBuilder wire;private SparkToolController tools;private UnityEngine.EventSystems.PhysicsRaycaster ownRaycaster;
  private readonly List<SparkCircuitConnection> graph=new();
  private bool[] placed,soldered,flux,hot;private bool[] repaired=new bool[3];
  private int selectedPart,selectedFootprint,rotation,selectedRoute,selectedLayer=1,stage,revision,fault=-1,pendingRepair=-1;
  private bool diagnosed,unapproved,armed,lossObserved,resetObserved,running,xray,calibrated;
  private int loadCase,sampleCount;private double testStart,lastIndependent=-100,stableSince=-1;
  private float nextUI,nextElectrical;private int runRevision;private string notice="Select a footprint. FIT, FLUX and SOLDER each part.";
  private GuardianSample measured;
  public int Stage=>stage;public int InstalledCount=>placed?.Count(x=>x)??0;public int SolderedCount=>soldered?.Count(x=>x)??0;
  public int RoutedCount=>routes==null?0:Enumerable.Range(0,66).Count(RouteReady);public int ViaCount=>vias?.Count(v=>v.barrel.IsClosed)??0;
  public bool Running=>running;public int SelectedRoute=>selectedRoute;public GuardianSample Measured=>measured;
  public bool CanEnableSecondary => !Isolated && plant.CanEnableSecondary;
  public bool CanEnableCore => !Isolated && plant.CanEnableCore;
  private bool Active=>manager&&loader&&loader.ActiveLayout==layout;
  private void Start()
  {
   manager=FindFirstObjectByType<LevelGamePlayManager>();loader=FindFirstObjectByType<SparkLevelLayoutLoader>();
   if(!Active){gameObject.SetActive(false);return;}
   if(!design.IsResolved(out notice)){enabled=false;return;}
   placed=new bool[parts.Length];soldered=new bool[parts.Length];flux=new bool[parts.Length];hot=new bool[parts.Length];
   using(manager.ElectricalSolver.BeginConfigurationBatch())for(int i=0;i<parts.Length;i++)
   {
    var p=parts[i];
    var label=p.art.transform.Find(design.parts[i].reference+" label");
    if(label && label.TryGetComponent<TextMesh>(out var referenceLabel))referenceLabel.characterSize=.0034f;
    p.art.SetActive(false);foreach(var j in p.joints)j.SetState(SparkSwitchState.Open);
    foreach(var device in p.devices)device.SetElectricalEnabled(false);
    workstation.AddTarget(p.target);p.target.SetWorkstation(workstation);
   }
   workstation.BindAuthority(this);
   SparkLevel104BoardView.DockControls(workstation.GetComponent<Canvas>(), layout.transform, heading, readings, detail);
   if(Camera.main&&!Camera.main.GetComponent<UnityEngine.EventSystems.PhysicsRaycaster>())
   {ownRaycaster=Camera.main.gameObject.AddComponent<UnityEngine.EventSystems.PhysicsRaycaster>();ownRaycaster.eventMask=1<<LayerMask.NameToLayer("UI");}
   tools=FindFirstObjectByType<SparkGameplayController>()?.ToolController;
   if(tools){tools.ActiveToolChanged+=ToolChanged;ToolChanged(null,tools.ActiveTool);}
   wire=FindFirstObjectByType<SignalWireBuilder>();
   if(wire){wire.TerminalPickerOverride=PickPad;wire.TerminalWireConnected+=ManualWire;}
   view=gameObject.AddComponent<SparkLevel104BoardView>();view.Initialize(arena);BoardView();ReadGraph();Present();
  }
  private void OnDisable()
  {
   if(tools)tools.ActiveToolChanged-=ToolChanged;if(ownRaycaster)Destroy(ownRaycaster);
   if(!wire)return;wire.TerminalWireConnected-=ManualWire;
   if(wire.TerminalPickerOverride==PickPad){if(wire.IsDrawing)wire.CancelWire();wire.TerminalPickerOverride=null;}
  }
  private void ToolChanged(SparkTool old,SparkTool current){if(ownRaycaster)ownRaycaster.enabled=!current||current.ToolType!=SparkToolType.Wire;}
  public void WireMode(){tools?.OnWireButton();if(ownRaycaster)ownRaycaster.enabled=false;notice="Drag amber contact to cyan contact on the selected layer. HOME returns to board.";}
  public void InspectMode(){tools?.OnInspectButton();if(ownRaycaster)ownRaycaster.enabled=true;workstation.SelectTool(SparkVirtualTool.Inspect);}
  private bool Edge(SparkTerminal a,SparkTerminal b)=>graph.Exists(e=>e.Kind==SparkConnectionKind.Wire&&Pair(e.A,e.B,a,b));
  private static bool Pair(SparkTerminal a,SparkTerminal b,SparkTerminal x,SparkTerminal y)=>(a==x&&b==y)||(a==y&&b==x);
  private bool RouteReady(int index)=>routes[index].segments.All(s=>Edge(s.from,s.to))&&routes[index].vias.All(v=>vias[v].barrel.IsClosed);
  private void ReadGraph()
  {
   foreach(var v in vias)foreach(var tap in v.layerTaps)tap.SetState(v.barrel.IsClosed?SparkSwitchState.Closed:SparkSwitchState.Open); graph.Clear();manager.CircuitSystem.CopyConnections(graph);
   unapproved=graph.Any(e=>e.Kind==SparkConnectionKind.Wire&&(e.A.transform.IsChildOf(arena)||e.B.transform.IsChildOf(arena))&&!routes.Any(r=>r.segments.Any(s=>Pair(e.A,e.B,s.from,s.to))));
   foreach(var r in routes)foreach(var s in r.segments)s.line.enabled=Edge(s.from,s.to)&&(xray||s.layer==selectedLayer); foreach(var renderer in art.GetComponentsInChildren<Renderer>(true))renderer.forceRenderingOff=xray||selectedLayer!=1; foreach(var v in vias){v.top.GetComponent<Renderer>().enabled=xray||selectedLayer==1;v.bottom.GetComponent<Renderer>().enabled=xray||selectedLayer==4;}
  }
  private bool Isolated=>!supply.IsOutputActive;
  public bool TryFitPart(string component,string footprint,out string reason)
  {
   int i=Array.FindIndex(design.parts,p=>p.reference==component);reason="Isolate power before assembly.";
   if(!Active||!Isolated)return false;
   if(i<0||component!=footprint){reason="Wrong component identity for "+footprint;return false;}
   if(rotation!=0){reason="Pin 1 / polarity does not match the footprint.";return false;}
   if(placed[i]){reason="Already installed.";return false;}
   placed[i]=true;parts[i].art.SetActive(true);revision++;reason=component+" fitted. Flux and solder its fixed contacts.";effects.Emit(parts[i].target.transform.position,0);return true;
  }
  public void FitPart()=>TryFitPart(design.parts[selectedPart].reference,design.parts[selectedFootprint].reference,out notice);
  public void NextPart(){for(int n=1;n<=parts.Length;n++){int i=(selectedPart+n)%parts.Length;if(!soldered[i]){selectedPart=i;break;}}selectedFootprint=selectedPart;rotation=0;}
  public void RotatePart(){rotation=(rotation+1)%4;}
  public void FluxPart()=>TryApplyRepair(parts[selectedPart].target,SparkVirtualTool.Flux,out notice);
  public void SolderPart()=>TryApplyRepair(parts[selectedPart].target,SparkVirtualTool.Solder,out notice);
  public void NextRoute(){selectedRoute=(selectedRoute+1)%67;SelectPendingLayer();}
  public void PreviousRoute(){selectedRoute=(selectedRoute+66)%67;SelectPendingLayer();}
  private void SelectPendingLayer(){var s=routes[selectedRoute].segments.FirstOrDefault(s=>!Edge(s.from,s.to));if(s!=null)selectedLayer=s.layer;}
  public void LayerNext(){selectedLayer=selectedLayer%4+1;ReadGraph();notice="Layer L"+selectedLayer+(selectedLayer==2?" — ground routing and factory return branches":selectedLayer==3?" — isolated power routing":" — player copper");}
  public void ToggleXray(){xray=!xray;ReadGraph();}
  public void PlateVia()
  {
   if(!Active||!Isolated||stage<1){notice="Finish assembly and isolate power first.";return;}
   int i=Array.Find(routes[selectedRoute].vias,v=>!vias[v].barrel.IsClosed);
   if(routes[selectedRoute].vias.Length==0||!routes[selectedRoute].vias.Any(v=>!vias[v].barrel.IsClosed))i=-1;
   if(i<0){notice="No missing via in this task.";return;}
   vias[i].barrel.SetState(SparkSwitchState.Closed);revision++;effects.Emit(vias[i].top.transform.position,0);notice=design.vias[i].id+" plated L1 to L4.";
  }
  private Segment Pending()
  {
   if(!Active||stage<1||!Isolated&&selectedRoute!=66)return null;
   var r=routes[selectedRoute];if(r.vias.Any(v=>!vias[v].barrel.IsClosed))return null;
   return r.segments.FirstOrDefault(s=>!Edge(s.from,s.to));
  }
  public void RouteCopper()
  {
   ReadGraph();var s=Pending();if(s==null){notice="Plate both vias, then route the pending segment.";return;}
   if(s.layer!=selectedLayer&&!xray){notice="Select L"+s.layer+" before drawing this segment.";return;}
   if(manager.CircuitSystem.TryCreateConnection(s.from,s.to,SparkConnectionKind.Wire,SparkConnectionDirection.Bidirectional,out _))CompleteRoute();
  }
  private void CompleteRoute()
  {
   revision++;ReadGraph();effects.Emit(routes[selectedRoute].segments[^1].to.transform.position,1);
   if(RouteReady(selectedRoute)&&selectedRoute<66)for(int n=1;n<=67;n++){int i=(selectedRoute+n)%67;if(!RouteReady(i)){selectedRoute=i;break;}}
   SelectPendingLayer();
  }
  public void UndoCopper()
  {
   if(!Active||!Isolated&&selectedRoute!=66)return;ReadGraph();
   foreach(var edge in graph.ToArray())if(edge.Kind==SparkConnectionKind.Wire&&routes[selectedRoute].segments.Any(s=>Pair(edge.A,edge.B,s.from,s.to)))RemoveWire(edge.Id);
   revision++;ReadGraph();
  }
  private void RemoveWire(ulong id){if(!wire||!wire.RemoveConnectedWire(id))manager.CircuitSystem.TryRemoveConnection(id,out _);}
  private SparkTerminal PickPad(Vector2 pointer,SparkTerminal start)
  {
   var s=Pending();if(s==null||!Camera.main||s.layer!=selectedLayer&&!xray)return null;
   SparkTerminal best=null;float distance=Mathf.Pow(Mathf.Clamp(Screen.height*.017f,14,27),2);
   foreach(var p in new[]{s.from,s.to}){var screen=Camera.main.WorldToScreenPoint(p.transform.position);float d=((Vector2)screen-pointer).sqrMagnitude;if(p!=start&&screen.z>0&&d<distance){best=p;distance=d;}}
   return best;
  }
  private void ManualWire(SparkTerminal a,SparkTerminal b,SignalPath path)
  {
   if(routes[selectedRoute].segments.Any(s=>Pair(a,b,s.from,s.to)))CompleteRoute();
  }
  private void Update()
  {
   if(!Active||placed==null)return;ReadGraph();
   bool topology=SolderedCount==50&&RoutedCount==66&&ViaCount==14&&!unapproved;
   if(stage==0&&SolderedCount==50)Award("50 source BOM identities and solder contacts verified");
   if(stage==1&&topology&&RouteReady(66)&&design.drcVerified)Award("66 owned routes / 14 L1-L4 plated vias / W067 in native graph");
   plant.wrongFeedback=Math.Abs(feedbackTop.Resistance-52500)>1;plant.openSupplyVia=!vias[4].barrel.IsClosed;
   double vin=Voltage(input);if(!double.IsFinite(vin))vin=0;
   plant.Advance(Time.deltaTime,vin,topology,!Isolated);
   if(Time.unscaledTime>=nextElectrical){nextElectrical=Time.unscaledTime+.02f;DriveNative(topology);}
   measured=ReadNativeSample();
   bool good=measured.valid&&Math.Abs(measured.main-5)<=.15;
   bool connected=RouteReady(66);bool high=good&&Voltage(trustedInput)>2.5;
   if(!connected&&(armed||plant.feedback==TrustedFeedback.High)){if(armed)lossObserved=true;lastIndependent=-100;calibrated=false;resetObserved=false;plant.ObserveTrusted(false,false,true);notice="W067 MISSING / UNKNOWN. Loads inhibited; physical rails retained. Reconnect, probe rails, reset, then qualify.";}
   else if(connected&&good)plant.ObserveTrusted(true,high,true);
   if(stage<5&&!lossObserved&&connected&&high)plant.ResetSafety(true);
   bool rails=RailCheck();stableSince=rails?(stableSince<0?plant.clock:stableSince):-1;
   if(stage==2&&calibrated&&stableSince>=0&&plant.clock-stableSince>.5)Award("Deliberate 12 V > 5 V > 3V3 > 1V8 startup; independent native node readings stable");
   if(running)TickQualification();
   if(Time.unscaledTime>=nextUI){nextUI=Time.unscaledTime+.15f;Present();}
  }
  private double Voltage(SparkTerminal pin)
  {
   var result=manager.ElectricalSolver.LatestSolveResult;
   return result!=null&&result.TryGetVoltage(pin,ground,out float value)?value:double.NaN;
  }
  private GuardianSample ReadNativeSample()
  {
   var sample = plant.latest;
   sample.main = Voltage(mainRail);
   sample.secondary = Voltage(secondaryRail);
   sample.core = Voltage(coreRail);
   sample.current = Math.Abs(load.ElectricalState.Current);
   sample.inputCurrent = Math.Abs(supply.ElectricalState.Current);
   double delivered = sample.main * sample.current + sample.secondary * .012 + sample.core * .008;
   double consumed = supply.OutputVoltage * sample.inputCurrent;
   sample.efficiency = consumed > 1e-6 ? delivered / consumed : 0;
   sample.valid &= double.IsFinite(sample.main) && double.IsFinite(sample.secondary) && double.IsFinite(sample.core);
   return sample;
  }
  private void DriveNative(bool topology)
  {
   using(manager.ElectricalSolver.BeginConfigurationBatch())
   {
    Drive(mainStage,plant.output,!Isolated&&topology&&plant.enableMain);
    Drive(secondaryStage,plant.secondary,!Isolated&&topology&&plant.enableSecondary);
    Drive(coreStage,plant.core,!Isolated&&topology&&plant.enableCore);
    Drive(supervisor,3.3,!Isolated&&topology&&plant.PowerGood);
    safeLoad.SetState(plant.LoadsEnabled&&!Isolated?SparkSwitchState.Closed:SparkSwitchState.Open);
    load.SetResistance((float)plant.LoadOhms);
    double inputVolts=Math.Max(1,plant.latest.input);double power=plant.latest.inputCurrent*inputVolts;
    inputDraw.SetResistance((float)(power>.001?inputVolts*inputVolts/power:1000000));
   }
  }
  private static void Drive(SparkPowerSupply source,double voltage,bool on)
  {
   float v=(float)Math.Max(.001,voltage);
   if(Mathf.Abs(source.OutputVoltage-v)>.002f){source.SetOutput(false,default);source.TryConfigure(v,3,out _);}
   source.SetOutput(on&&source.ElectricalEnabled,default);
  }
  private bool RailCheck()=>measured.valid&&Math.Abs(measured.main-5)<=.15&&Math.Abs(measured.secondary-3.3)<=.099&&Math.Abs(measured.core-1.8)<=.054&&!unapproved;
  public void TogglePower()
  {
   if(!Isolated){supply.SetOutput(false,default);plant.Isolate();plant.LoadOhms=plant.configuration.lightOhms;running=false;calibrated=false;stableSince=-1;notice="Input isolated. Stored output energy decays through the bleed/load model.";return;}
   if(stage<2||SolderedCount!=50||RoutedCount!=66||ViaCount!=14||unapproved){notice="Complete assembly, copper and via checks first.";return;}
   supply.SetOutput(true,default);notice="12 V applied. Enable 5 V, wait stable, then 3V3, then 1V8.";
  }
  public void EnableMain()
  {
   notice = !Isolated && plant.TryEnable(0) ? "5 V soft-start enabled." : "Apply 12 V first.";
  }
  public void EnableSecondary()
  {
   notice = CanEnableSecondary && plant.TryEnable(1) ? "3V3 soft-start enabled." : "Wait for the 5 V rail to stabilize.";
  }
  public void EnableCore()
  {
   notice = CanEnableCore && plant.TryEnable(2) ? "1V8 soft-start enabled." : "Wait for the 3V3 rail to stabilize.";
  }
  public void CheckRails()
  {
   ReadGraph();
   if (Isolated || !manager.ElectricalSolver.SolveNow())
   {
    notice = "A live board solve is required to measure the rails.";
    return;
   }
   measured = ReadNativeSample();
   if(!RailCheck()){notice="Independent native rail readings are not within tolerance.";return;}
   lastIndependent=plant.clock;calibrated=true;notice="Fresh independent 5 V / 3V3 / 1V8 measurements recorded.";
  }
  public void TestBoard()
  {
   if(stage<3||Isolated||!RailCheck()||!calibrated||plant.safetyLatched||plant.feedback!=TrustedFeedback.High||fault>=0)
   {notice="Check rails, clear faults and reset trusted safety before load qualification.";return;}
   running=true;loadCase=0;sampleCount=0;runRevision=revision;testStart=plant.clock;plant.LoadOhms=plant.configuration.lightOhms;
   notice="LIGHT > NOMINAL > PEAK qualification running; edits or invalid samples cancel the window.";
  }
  private void TickQualification()
  {
   double age=plant.clock-testStart;
   if(runRevision!=revision||!RailCheck()||plant.safetyLatched||unapproved){running=false;notice="Qualification canceled: topology, rails or safety changed.";return;}
   if(age<plant.configuration.settlingSeconds)return;
   if(!plant.Qualified(measured,loadCase==1)){running=false;notice="Qualification failed: ripple / efficiency / temperature / load authority.";return;}
   sampleCount++;
   if(age<plant.configuration.windowSeconds)return;
   if(++loadCase<3){plant.LoadOhms=loadCase==1?plant.configuration.nominalOhms:plant.configuration.peakOhms;testStart=plant.clock;return;}
   running=false;
   if(stage==3)Award("Fresh light / nominal / peak measured traces passed",sampleCount,testStart);
   else if(pendingRepair>=0){repaired[pendingRepair]=true;pendingRepair=-1;if(repaired.All(x=>x)&&stage==4)Award("R7 / V05 / Q1 corrected and separately requalified",sampleCount,testStart);}
   else if(stage==5&&armed&&lossObserved&&resetObserved&&plant.clock-lastIndependent<30)Award("Controlled W067 loss, safe inhibition, reconnect, fresh independent reset and new full qualification",sampleCount,testStart);
   notice="Qualification passed. "+(stage==4?"Inject and diagnose each fault in turn.":stage==5?"Arm the controlled W067 recovery test.":"Evidence recorded.");
  }
  public void FaultFeedback()=>InjectFault(0);public void FaultVia()=>InjectFault(1);public void FaultLoss()=>InjectFault(2);
  public bool InjectFault(int which)
  {
   if(stage!=4||fault>=0||pendingRepair>=0||which<0||which>2||repaired[which])return false;
   fault=which;diagnosed=false;running=false;calibrated=false;revision++;
   if(which==0)feedbackTop.SetResistance(68000);if(which==1)vias[4].barrel.SetState(SparkSwitchState.Open);if(which==2)plant.highLoss=true;
   notice = which == 2
       ? "Q1 fault inserted. Use the thermal camera after the temperature rises."
       : which == 1
           ? "V05 fault inserted. Isolate 12 V, then run continuity across the via."
           : "R7 fault inserted. Isolate 12 V, then measure R7 with the multimeter.";
   effects.Emit(parts[Array.FindIndex(design.parts,p=>p.reference==FaultTarget())].target.transform.position,2);
   return true;
  }
  private string FaultTarget()=>fault==0?"R7":fault==1?"L1":"Q1";
  private SparkVirtualTool FaultTool() => fault switch
  {
   0 => SparkVirtualTool.Multimeter,
   1 => SparkVirtualTool.Continuity,
   _ => SparkVirtualTool.ThermalCamera
  };

  public void Diagnose()
  {
   if (fault < 0) return;
   int index = Array.FindIndex(design.parts, part => part.reference == FaultTarget());
   TryReadProbe(parts[index].target, FaultTool(), out notice);
  }
  public void Repair()
  {
   if(fault<0)return;var target=parts[Array.FindIndex(design.parts,p=>p.reference==FaultTarget())].target;
   if(fault!=1)TryApplyRepair(target,SparkVirtualTool.HotAir,out _);
   TryApplyRepair(target,fault==1?SparkVirtualTool.ViaRepair:SparkVirtualTool.ReplaceComponent,out notice);
  }
  public bool TryReadProbe(SparkVirtualPCBTarget target,SparkVirtualTool tool,out string reading)
  {
   int index=Array.FindIndex(design.parts,p=>p.reference==target.TargetId);if(index<0){reading="Unknown PCB target.";return false;}
   selectedFootprint=index;selectedPart=index;
   if (fault >= 0 && target.TargetId == FaultTarget() && tool == FaultTool())
   {
    bool found = ReadFaultEvidence(parts[index], out reading);
    diagnosed |= found;
    return found;
   }

   reading=target.TargetId+" / "+design.parts[index].value+" / "+(soldered[index]?"SOLDERED":"OPEN")+" / t="+plant.clock.ToString("F3");
   if(tool==SparkVirtualTool.ThermalCamera && target.TargetId=="Q1")reading+=" / Q1 "+plant.temperature.ToString("F1")+" C";
   else{var pin=GetComponentsInChildren<SparkTerminal>(true).FirstOrDefault(t=>t.name==design.contacts.First(c=>c.reference==target.TargetId).id);double v=pin?Voltage(pin):double.NaN;reading+=" / "+(double.IsFinite(v)?v.ToString("F5")+" V":"UNKNOWN");}
   if (fault >= 0 && target.TargetId == FaultTarget()) reading += " / Use the " + SparkPCBToolCatalog.DisplayName(FaultTool()) + " for diagnosis.";
   return true;
  }

  private bool ReadFaultEvidence(Part part, out string reading)
  {
   if (fault == 2)
   {
    double temperature = plant.temperature;
    double threshold = plant.configuration.ambient + 12;
    reading = $"Q1 thermal reading {temperature:F1} °C. Fault threshold {threshold:F1} °C.";
    return double.IsFinite(temperature) && temperature >= threshold;
   }

   if (!Isolated)
   {
    reading = "Isolate the 12 V source before a resistance or continuity check.";
    return false;
   }

   if (fault == 1)
   {
    // The probe is across the plated barrel itself, independent of other board paths.
    var via = vias[4];
    bool closed = via.barrel.IsConducting;
    reading = $"{via.top.name} to {via.bottom.name} / {(closed ? "CONTINUOUS" : "OPEN")}.";
    return !closed;
   }

   double expected = plant.configuration.feedbackTop;
   double measuredResistance = feedbackTop.EffectiveResistance;
   if (!part.joints.All(joint => joint.IsConducting)
       || !feedbackTop.CanConductBetween(feedbackTop.TerminalA, feedbackTop.TerminalB)
       || !double.IsFinite(measuredResistance))
   {
    reading = "R7 resistance unavailable. Check its solder contacts.";
    return false;
   }
   reading = $"R7 resistance {measuredResistance / 1000:F2} kΩ. Expected {expected / 1000:F2} kΩ.";
   return Math.Abs(measuredResistance - expected) > 1;
  }
  public bool TryApplyRepair(SparkVirtualPCBTarget target,SparkVirtualTool tool,out string message)
  {
   message="";int i=Array.FindIndex(design.parts,p=>p.reference==target.TargetId);if(i<0)return false;
   if(SparkPCBToolCatalog.IsDiagnostic(tool))return TryReadProbe(target,tool,out message);
   if(!Isolated){message="Isolate the 12 V source before soldering, removal or repair.";return false;}
   if(tool==SparkVirtualTool.Flux){flux[i]=true;message="Flux applied.";return true;}
   if(tool==SparkVirtualTool.HotAir){hot[i]=true;message="Removal prepared.";return true;}
   if(tool==SparkVirtualTool.Solder||tool==SparkVirtualTool.Reflow)
   {
    if(!placed[i]||!flux[i]){message="Fit and flux the part first.";return false;}
    using(manager.ElectricalSolver.BeginConfigurationBatch()){foreach(var device in parts[i].devices)device.SetElectricalEnabled(true);foreach(var j in parts[i].joints)j.SetState(SparkSwitchState.Closed);}
    soldered[i]=true;revision++;effects.Emit(workstation.CurrentContactPoint??target.transform.position,0);NextPart();message=target.TargetId+" contacts soldered.";return true;
   }
   if(tool==SparkVirtualTool.Desolder)
   {
    using(manager.ElectricalSolver.BeginConfigurationBatch())
    {
     foreach(var joint in parts[i].joints)joint.SetState(SparkSwitchState.Open);
     foreach(var device in parts[i].devices)device.SetElectricalEnabled(false);
    }
    soldered[i]=false;revision++;message="Contacts opened.";return true;
   }
   bool correct=fault>=0&&target.TargetId==FaultTarget()&&diagnosed&&(fault==1&&tool==SparkVirtualTool.ViaRepair||fault!=1&&tool==SparkVirtualTool.ReplaceComponent&&hot[i]);
   if(!correct){message="Diagnose the defect and choose its matching repair tool.";return false;}
   pendingRepair=fault;
   if(fault==0)feedbackTop.SetResistance(52500);if(fault==1)vias[4].barrel.SetState(SparkSwitchState.Closed);if(fault==2)plant.highLoss=false;
   fault=-1;calibrated=false;revision++;message="Correct replacement fitted and reattached. Restart all rails, measure and repeat all three loads.";return true;
  }
  public void ArmW067()
  {
   if(stage!=5||!RailCheck()||!RouteReady(66))return;
   armed=true;selectedRoute=66;selectedLayer=routes[66].segments[0].layer;
   notice="Controlled test armed. UNDO removes W067; observe unknown feedback and load inhibition.";
  }
  public void ResetSafety()
  {
   ReadGraph();
   resetObserved=armed&&lossObserved&&!Isolated&&RouteReady(66)&&RailCheck()&&plant.clock-lastIndependent<2&&plant.ResetSafety(true);
   notice=resetObserved?"Safety reset accepted. Run a NEW three-load qualification.":"Reconnect W067 and obtain fresh independent rail measurements first.";
  }
  private void Award(string proof,int samples=0,double began=0)
  {
   if(stage>=6||!loader.ActiveBindings.SetRuntimeComponentBinding(Gates[stage],certification))return;
   evidence.Add(new(){objective=stage+1,revision=revision,samples=samples,timestamp=plant.clock,started=began,proof=proof});stage++;manager.QueueEvaluation();
  }
  public void BoardView(){if(!view)return;var board=art.Find("00_L97_VGR5_Empty_PCB_160x110mm");var rs=board.GetComponentsInChildren<Renderer>();var b=rs[0].bounds;foreach(var r in rs)b.Encapsulate(r.bounds);view.UseSocketView(b.center-arena.right*b.extents.x*1.4f-arena.forward,b.center+arena.right*b.extents.x*1.4f-arena.forward);}
  public void PadView(){var s=Pending();if(s!=null)view.UseSocketView(s.from.transform.position,s.to.transform.position);else view.UseSocketView(parts[selectedPart].target.transform.position-arena.right*.15f,parts[selectedPart].target.transform.position+arena.right*.15f);}
  private void Present()
  {
   heading.text=$"VOLTAGE GUARDIAN  /  {stage}/6 OBJECTIVES";
   readings.text=$"{SolderedCount}/50 soldered    {RoutedCount}/66 copper    {ViaCount}/14 vias\n"
       +$"5 V {measured.main:F3}    3V3 {measured.secondary:F3}    1V8 {measured.core:F3}    PG {plant.feedback}\n"
       +$"{measured.current:F3} A    Ripple {measured.ripple*1000:F1} mVpp    η {measured.efficiency*100:F1}%    Q1 {plant.temperature:F1} °C";
   string nextRail = CanEnableCore && !plant.enableCore ? "Ready to enable 1V8. "
       : CanEnableSecondary && !plant.enableSecondary ? "Ready to enable 3V3. " : "";
   detail.text=(stage==0?$"{design.parts[selectedPart].reference} / {design.parts[selectedPart].value} / rotation {rotation*90}°":$"{design.routes[selectedRoute].id}: {design.routes[selectedRoute].from} > {design.routes[selectedRoute].to} / L{selectedLayer}")+"\n"+(unapproved?"UNAUTHORIZED COPPER. ":"")+nextRail+notice;
  }
  private void OnGUI()
  {
   if(!Active||!Camera.main||Event.current.type!=EventType.Repaint)return;
   GUI.Label(new Rect(14,Screen.height-27,650,24),"HOME overview | Middle drag pan | Right drag orbit | Select route, plate vias, draw highlighted contacts");
   if(stage==0){var v=Camera.main.WorldToScreenPoint(parts[selectedPart].target.transform.position);if(v.z>0){var color=GUI.color;GUI.color=Color.cyan;GUI.DrawTexture(new Rect(v.x-6,Screen.height-v.y-6,12,12),Texture2D.whiteTexture);GUI.Label(new Rect(v.x-100,Screen.height-v.y-32,250,24),design.parts[selectedPart].reference+" / ALIGN PIN 1");GUI.color=color;}return;} var s=Pending();if(s==null)return;
   var old=GUI.color;foreach(var p in new[]{s.from,s.to}){var v=Camera.main.WorldToScreenPoint(p.transform.position);if(v.z<=0)continue;GUI.color=p==s.from?new Color(1,.7f,.2f):Color.cyan;GUI.DrawTexture(new Rect(v.x-5,Screen.height-v.y-5,10,10),Texture2D.whiteTexture);GUI.Label(new Rect(v.x-90,Screen.height-v.y-28,250,22),p.name);}GUI.color=old;
  }
 }
}






