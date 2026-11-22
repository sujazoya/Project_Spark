using System;

namespace ProjectSpark.VoltageGuardian
{
    // Averaged educational converter. Units are SI; scene/art scale never enters this model.
    [Serializable] public sealed class GuardianConfiguration
    {
        public double inductance=100e-6,capacitance=470e-6,esr=.035,winding=.035;
        public double switchingHz=100003,reference=.8,feedbackTop=52500,feedbackBottom=10000;
        public double mosfetResistance=.045,switchingLoss=.065,ambient=25,thermalResistance=36,thermalSeconds=2.8;
        public double tolerance=.03,rippleLimit=.060,minimumEfficiency=.85,maximumTemperature=80;
        public double lightOhms=33,nominalOhms=10,peakOhms=5,windowSeconds=1.6,settlingSeconds=.4;
        public double maximumCurrent=2.2,overvoltage=5.6,undervoltage=9;
    }
    public enum TrustedFeedback { Missing, Low, High, Stale }
    [Serializable] public struct GuardianSample
    {
        public double time,input,main,secondary,core,current,inputCurrent,ripple,efficiency,temperature,duty,inductorCurrent;
        public bool valid,loadsEnabled;
        public string protection;
    }
    [Serializable] public sealed class SparkVoltageGuardianPlant
    {
        private const double RailStabilitySeconds = .12;

        public GuardianConfiguration configuration=new();
        public double clock,output,secondary,core,inductorCurrent,duty,temperature=25;
        public bool enableMain,enableSecondary,enableCore,highLoss,wrongFeedback,openSupplyVia,safetyLatched;
        public TrustedFeedback feedback=TrustedFeedback.Missing;
        public GuardianSample latest;
        public GuardianSample[] history=new GuardianSample[4096];
        public int historyCount,historyHead;
        private double integral,accumulator,sampleAccumulator,mainStable,secondaryStable,rampedTarget;
        private bool protectionLatched,secondaryStarted,coreStarted;
        private string protection="OFF";
        public bool PowerGood=>latest.valid && Math.Abs(latest.main-5)<=5*configuration.tolerance;
        public bool CanEnableSecondary => PowerGood && mainStable > RailStabilitySeconds;
        public bool CanEnableCore => latest.valid && enableSecondary && secondaryStable > RailStabilitySeconds;
        public bool StableRails=>PowerGood && Math.Abs(latest.secondary-3.3)<=3.3*configuration.tolerance && Math.Abs(latest.core-1.8)<=1.8*configuration.tolerance;
        public bool LoadsEnabled=>feedback==TrustedFeedback.High && !safetyLatched && !protectionLatched;
        public double LoadOhms { get; set; }=33;
        public void Advance(double seconds,double input,bool topologyValid,bool loadRequested)
        {
            if(seconds<0 || !double.IsFinite(seconds))throw new ArgumentOutOfRangeException(nameof(seconds));
            accumulator+=Math.Min(seconds,.25);
            const double step=.0001;
            while(accumulator>=step){Tick(step,input,topologyValid,loadRequested);accumulator-=step;}
        }
        private void Tick(double dt,double input,bool topologyValid,bool loadRequested)
        {
            var c=configuration; clock+=dt;
            bool powered=topologyValid && input>=c.undervoltage && input<=14;
            if(!powered){protectionLatched=false;integral=0;protection=input<0?"REVERSE_POLARITY":input>14?"INPUT_OVERVOLTAGE":"OFF";}
            double resistance=highLoss?2.5:c.mosfetResistance;
            bool drive=powered && enableMain && !protectionLatched;
            double setpoint=c.reference*(1+(wrongFeedback?68000:c.feedbackTop)/c.feedbackBottom);
            rampedTarget=drive?Math.Min(setpoint,rampedTarget+dt*18):0;
            double error=rampedTarget-output;
            if(drive)integral=Clamp(integral+error*dt*45,-.4,.4);
            duty=drive?Clamp(rampedTarget/input+error*.12+integral-inductorCurrent*.018,0,.92):0;
            if(temperature>70)duty*=Clamp((85-temperature)/15,0,1);
            bool loadOn=loadRequested && LoadsEnabled;
            double loadCurrent=output/10000+(loadOn?output/Math.Max(1,LoadOhms):0);
            // Semi-implicit integration of inductor energy and output capacitor charge.
            double vl=duty*input-output-inductorCurrent*(resistance+c.winding);
            inductorCurrent=Math.Max(0,inductorCurrent+vl/c.inductance*dt);
            if(!drive)inductorCurrent=Math.Max(0,inductorCurrent-output/c.inductance*dt);
            double capacitorCurrent=inductorCurrent-loadCurrent;
            output=Math.Max(0,output+capacitorCurrent/c.capacitance*dt);
            if(output>c.overvoltage){protectionLatched=true;protection="OUTPUT_OVERVOLTAGE";}
            if(inductorCurrent>c.maximumCurrent){protectionLatched=true;protection="OVERCURRENT";}
            if(temperature>85){protectionLatched=true;protection="THERMAL_SHUTDOWN";}
            double rail=openSupplyVia?0:output;
            mainStable=Math.Abs(rail-5)<.15?mainStable+dt:0;
            bool secondaryAvailable=powered&&enableSecondary&&(secondaryStarted||mainStable>RailStabilitySeconds)&&rail>3.48;
            secondaryStarted=secondaryAvailable;
            secondary=Approach(secondary,secondaryAvailable?Math.Min(3.3,Math.Max(0,rail-.18)):0,dt,.075);
            secondaryStable=Math.Abs(secondary-3.3)<.099?secondaryStable+dt:0;
            bool coreAvailable=powered&&enableCore&&(coreStarted||secondaryStable>RailStabilitySeconds)&&secondary>1.95;
            coreStarted=coreAvailable;
            core=Approach(core,coreAvailable?Math.Min(1.8,Math.Max(0,secondary-.12)):0,dt,.09);
            double switching=drive?(highLoss?1.8:c.switchingLoss):0;
            double loss=inductorCurrent*inductorCurrent*(resistance+c.winding)+switching+(powered?.035:0);
            temperature=Approach(temperature,c.ambient+loss*c.thermalResistance,dt,c.thermalSeconds);
            double pOut=rail*rail*(loadOn?1/Math.Max(1,LoadOhms):0)+secondary*.012+core*.008;
            double pIn=pOut+loss;
            double deltaI=drive?Math.Max(0,(input-output)*duty/(c.inductance*c.switchingHz)):0;
            double ripple=deltaI*(1/(8*c.switchingHz*c.capacitance)+c.esr);
            if(powered && !protectionLatched)protection="NONE";
            latest=new GuardianSample {time=clock,input=input,main=rail,secondary=secondary,core=core,
                current=loadOn?rail/Math.Max(1,LoadOhms):0,inputCurrent=powered?pIn/input:0,
                ripple=ripple,efficiency=pIn>1e-9?pOut/pIn:0,temperature=temperature,duty=duty,inductorCurrent=inductorCurrent,
                valid=powered && drive && !protectionLatched,loadsEnabled=loadOn,protection=protection};
            sampleAccumulator+=dt;
            if(sampleAccumulator>=.001)
            {
                sampleAccumulator-=.001;
                var sample=latest;
                sample.main+=.5*ripple*Math.Sin(2*Math.PI*c.switchingHz*clock);
                history[historyHead]=sample;historyHead=(historyHead+1)%history.Length;historyCount=Math.Min(historyCount+1,history.Length);
            }
        }
        public void ObserveTrusted(bool connected,bool high,bool fresh)
        {
            feedback=!connected?TrustedFeedback.Missing:!fresh?TrustedFeedback.Stale:high?TrustedFeedback.High:TrustedFeedback.Low;
            if(feedback is TrustedFeedback.Missing or TrustedFeedback.Stale)safetyLatched=true;
        }
        public bool ResetSafety(bool independentMeasurementsFresh)
        {
            if(!independentMeasurementsFresh || feedback!=TrustedFeedback.High || !StableRails)return false;
            safetyLatched=false;return true;
        }
        public bool TryEnable(int rail)
        {
            if(rail==0){enableMain=true;return true;}
            if(rail==1 && CanEnableSecondary){enableSecondary=true;return true;}
            if(rail==2 && CanEnableCore){enableCore=true;return true;}
            return false;
        }
        public void Isolate()
        {
            enableMain = enableSecondary = enableCore = false;
            mainStable = secondaryStable = 0;
        }
        public bool Qualified(in GuardianSample sample,bool nominal)
        {
            var c=configuration;
            return sample.valid && sample.loadsEnabled && sample.protection=="NONE" &&
                Math.Abs(sample.main-5)<=5*c.tolerance && Math.Abs(sample.secondary-3.3)<=3.3*c.tolerance &&
                Math.Abs(sample.core-1.8)<=1.8*c.tolerance && sample.ripple<c.rippleLimit &&
                sample.efficiency<=1.02 && sample.temperature<c.maximumTemperature && (!nominal || sample.efficiency>=c.minimumEfficiency);
        }
        private static double Clamp(double x,double low,double high)=>Math.Max(low,Math.Min(high,x));
        private static double Approach(double x,double target,double dt,double tau)=>x+(target-x)*(1-Math.Exp(-dt/tau));
    }
}

