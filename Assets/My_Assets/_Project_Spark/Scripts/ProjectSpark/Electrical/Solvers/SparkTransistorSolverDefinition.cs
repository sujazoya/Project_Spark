using System;
using UnityEngine;
using ProjectSpark.Gameplay;
namespace ProjectSpark.Electrical
{
    /// <summary>Educational voltage-controlled NPN/PNP switch approximation.
    /// Not a semiconductor SPICE model: base current, beta, saturation and thermal behavior
    /// are not represented. Uses solver iteration voltages, not previous-frame terminal state.</summary>
    [CreateAssetMenu(fileName="SparkTransistorSolverDefinition",menuName="Project Spark/Electrical/Solver Definitions/Transistor (Switch Approximation)")]
    public sealed class SparkTransistorSolverDefinition : SparkElectricalSolverDefinition
    {
        [SerializeField,Min(0.001f)] private float offResistanceOhms = 100000000f;
        public override Type SupportedComponentType => typeof(SparkTransistor);
        public override bool CanHandle(SparkElectricalComponent c) => c is SparkTransistor;
        public override bool TryGetTerminals(SparkElectricalComponent c,SparkElectricalNetworkGraph graph,out SparkTerminal a,out SparkTerminal b)
        { a=null;b=null;if (!(c is SparkTransistor t)) return false;
          a=t.CollectorTerminal;b=t.EmitterTerminal;return a!=null&&b!=null&&t.BaseTerminal!=null; }
        private bool Driven(SparkTransistor t,SparkElectricalSolveContext context)
        { if(!context.Graph.TryGetNode(t.BaseTerminal,out int bn) || !context.Graph.TryGetNode(t.EmitterTerminal,out int en)) return false;
          float v=context.GetIterationNodeVoltage(bn)-context.GetIterationNodeVoltage(en);
          return t.Type==SparkTransistorType.NPN ? v>=t.BaseThreshold : v<=-t.BaseThreshold; }
        public override void Stamp(SparkElectricalComponent c,SparkElectricalSolveContext context)
        { if(!(c is SparkTransistor t)||context==null||!t.ElectricalEnabled) return;
          if(!TryGetTerminals(t,context.Graph,out var a,out var b) ||
             !context.Graph.ContainsTerminal(a)||!context.Graph.ContainsTerminal(b)||!context.Graph.ContainsTerminal(t.BaseTerminal)) return;
          bool on=Driven(t,context);context.SetNonlinearState(t,on);
          context.AddConductance(a,b,1.0/Mathf.Max(0.000001f,on?t.OnResistance:offResistanceOhms)); }
        public override void ApplySolvedState(SparkElectricalComponent c,SparkElectricalSolveResult result)
        { if(!(c is SparkTransistor t)||result==null)return;
          if(!t.ElectricalEnabled||!TryGetTerminals(t,result.Context.Graph,out var a,out var b)||!result.TryGetVoltage(a,b,out float v))
          {t.ApplyElectricalState(new SparkElectricalState(0,0,0,SparkConductionState.NonConducting));return;}
          bool on=result.GetNonlinearState(t);float resistance=Mathf.Max(0.000001f,on?t.OnResistance:offResistanceOhms);
          float i=v/resistance;t.ApplyElectricalState(new SparkElectricalState(v,i,v*i,
              on && Mathf.Abs(i)>0.000001f?SparkConductionState.Conducting:SparkConductionState.NonConducting)); }
        public override bool TryCalculateCurrent(SparkElectricalComponent c,SparkElectricalSolveResult result,out float i)
        {i=0;if(!(c is SparkTransistor t)||result==null)return false;
         if(!t.ElectricalEnabled)return true;
         if(!TryGetTerminals(t,result.Context.Graph,out var a,out var b)||!result.TryGetVoltage(a,b,out float v))return false;
         i=v/Mathf.Max(0.000001f,result.GetNonlinearState(t)?t.OnResistance:offResistanceOhms);return true;}
    }
}
