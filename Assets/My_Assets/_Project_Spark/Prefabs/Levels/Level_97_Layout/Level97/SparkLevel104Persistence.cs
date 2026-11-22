using System;using System.Linq;using UnityEngine;
namespace ProjectSpark.Gameplay
{
 public sealed partial class SparkLevel104Process
 {
  private const string SaveKey="ProjectSpark.Level104.Board.v1";
  [Serializable]private sealed class SavedBoard
  {
   public string schema="VGR5-EDU-1";public bool[] placed,soldered,flux,vias,segments,repaired;
   public int stage,fault,pendingRepair,selectedPart,selectedRoute,layer;
   public bool armed,lossObserved;public double temperature,clock;public SparkLevel104Evidence[] evidence;
  }
  public void SaveBoard()
  {
   if(!Active)return;ReadGraph();
   var record=new SavedBoard {placed=placed,soldered=soldered,flux=flux,vias=vias.Select(v=>v.barrel.IsClosed).ToArray(),segments=routes.SelectMany(r=>r.segments).Select(s=>Edge(s.from,s.to)).ToArray(),repaired=repaired,stage=stage,fault=fault,pendingRepair=pendingRepair,selectedPart=selectedPart,selectedRoute=selectedRoute,layer=selectedLayer,armed=armed,lossObserved=lossObserved,temperature=plant.temperature,clock=plant.clock,evidence=evidence.ToArray()};
   PlayerPrefs.SetString(SaveKey,JsonUtility.ToJson(record));PlayerPrefs.Save();notice="Board topology and timestamped history saved. Live measurements will require a fresh run after loading.";
  }
  public bool LoadBoard()
  {
   if(!Active||!Isolated){notice="Isolate 12 V before loading board topology.";return false;}
   SavedBoard saved;
   try{saved=JsonUtility.FromJson<SavedBoard>(PlayerPrefs.GetString(SaveKey,""));}catch{notice="Invalid board save.";return false;}
   bool Size(bool[] values,int count)=>values!=null&&values.Length==count;
   if(saved==null||saved.schema!=design.revision||!Size(saved.placed,50)||!Size(saved.soldered,50)||!Size(saved.flux,50)||!Size(saved.vias,14)||!Size(saved.segments,routes.Sum(r=>r.segments.Length))||!Size(saved.repaired,3)||saved.stage<0||saved.stage>6||saved.fault< -1||saved.fault>2||saved.pendingRepair< -1||saved.pendingRepair>2||saved.evidence==null||saved.evidence.Length!=saved.stage||!double.IsFinite(saved.temperature)||saved.temperature<0||saved.temperature>150||!double.IsFinite(saved.clock)||saved.clock<0)
   {notice="Save schema or authoritative state is invalid; nothing changed.";return false;}
   for(int i=0;i<50;i++)if(saved.soldered[i]&&!saved.placed[i]){notice="Save contains solder without component occupancy.";return false;}
   for(int i=0;i<saved.evidence.Length;i++)if(saved.evidence[i]==null||saved.evidence[i].objective!=i+1||!double.IsFinite(saved.evidence[i].timestamp)||string.IsNullOrEmpty(saved.evidence[i].proof)){notice="Save history is malformed.";return false;}
   using(manager.ElectricalSolver.BeginConfigurationBatch())
   {
    ReadGraph();foreach(var edge in graph.ToArray())if(edge.Kind==SparkConnectionKind.Wire&&(edge.A.transform.IsChildOf(arena)||edge.B.transform.IsChildOf(arena)))RemoveWire(edge.Id);
    placed=saved.placed;soldered=saved.soldered;flux=saved.flux;hot=new bool[50];
    for(int i=0;i<50;i++){parts[i].art.SetActive(placed[i]);foreach(var j in parts[i].joints)j.SetState(soldered[i]?SparkSwitchState.Closed:SparkSwitchState.Open);foreach(var device in parts[i].devices)device.SetElectricalEnabled(soldered[i]);}
    for(int i=0;i<14;i++)vias[i].barrel.SetState(saved.vias[i]?SparkSwitchState.Closed:SparkSwitchState.Open);
    int n=0;foreach(var segment in routes.SelectMany(r=>r.segments))if(saved.segments[n++])manager.CircuitSystem.TryCreateConnection(segment.from,segment.to,SparkConnectionKind.Wire,SparkConnectionDirection.Bidirectional,out _);
   }
   repaired=saved.repaired;fault=saved.fault;pendingRepair=saved.pendingRepair;diagnosed=false;armed=saved.armed;lossObserved=saved.lossObserved;
   selectedPart=Mathf.Clamp(saved.selectedPart,0,49);selectedFootprint=selectedPart;selectedRoute=Mathf.Clamp(saved.selectedRoute,0,66);selectedLayer=Mathf.Clamp(saved.layer,1,4);
   evidence=saved.evidence.ToList();stage=Math.Min(saved.stage,5);if(evidence.Count>stage)evidence.RemoveRange(stage,evidence.Count-stage);
   plant=new ProjectSpark.VoltageGuardian.SparkVoltageGuardianPlant{clock=saved.clock,temperature=saved.temperature,highLoss=fault==2,safetyLatched=true};
   feedbackTop.SetResistance(fault==0?68000:52500);calibrated=resetObserved=running=false;lastIndependent=-100;stableSince=-1;revision++;
   for(int i=0;i<6;i++){if(i<stage)loader.ActiveBindings.SetRuntimeComponentBinding(Gates[i],certification);else loader.ActiveBindings.ClearRuntimeComponentBinding(Gates[i]);}
   ReadGraph();manager.QueueEvaluation();notice="Board restored and isolated. Fresh measurements, safety reset and full qualification are required.";return true;
  }
  public void ResumeBoard()=>LoadBoard();
 }
}
