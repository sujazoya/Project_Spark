using UnityEngine;

namespace ProjectSpark.Gameplay
{
    // Fictional WDM training model with four independent carriers and data streams.
    public sealed class SparkLevel80Plant
    {
        public const float Step=.025f;
        public enum Disturbance { None, Drift, Thermal, Bend }
        private static readonly float[] Setpoint={.45f,.53f,.59f,.49f};
        private readonly uint[] patterns={0xACE1,0x239D,0xF207,0x3471};
        private readonly uint[] impairment={0x90193,0x48539,0xBC073,0x12377};
        private readonly int[] blockBits=new int[4],blockBad=new int[4],blockOnes=new int[4];
        public readonly float[] Heater=new float[4],Temperature=new float[4],Detune=new float[4],Rx=new float[4],
            Current=new float[4],Eye=new float[4],Jitter=new float[4],Clock=new float[4];
        public readonly int[] Bits=new int[4],RawErrors=new int[4],CorrectedErrors=new int[4],RouteBits=new int[4];
        public readonly bool[] Locked=new bool[4];
        public float Launch {get;private set;}
        public float MainClosed {get;private set;}=1;
        public float EmergencyClosed {get;private set;}=1;
        public float SourceTemperature {get;private set;}
        public float WavelengthOffset {get;private set;}
        public float Coupling {get;private set;}
        public float BendLoss {get;private set;}
        public float CrossTalk {get;private set;}
        public float SpectrumGap {get;private set;}
        public float ThroughPower=>Launch*(1-MainClosed)*(1-EmergencyClosed)*Coupling*(1-BendLoss);
        public float AverageRx=>(Rx[0]+Rx[1]+Rx[2]+Rx[3])*.25f;
        public bool AllLocked=>Locked[0]&&Locked[1]&&Locked[2]&&Locked[3]&&SpectrumGap>.88f;
        public bool ClocksReady=>Clock[0]>.95f&&Clock[1]>.95f&&Clock[2]>.95f&&Clock[3]>.95f;
        public bool SafelyClosed=>MainClosed>.99f&&EmergencyClosed>.99f&&Launch<.025f;
        public float RawBer(int i)=>Bits[i]>0?(float)RawErrors[i]/Bits[i]:1;
        public float FinalBer(int i)=>Bits[i]>0?(float)CorrectedErrors[i]/Bits[i]:1;

        public void ClearData()
        {
            for(int i=0;i<4;i++)Bits[i]=RawErrors[i]=CorrectedErrors[i]=RouteBits[i]=blockBits[i]=blockBad[i]=blockOnes[i]=0;
        }
        public bool BudgetGood()
        {
            for(int i=0;i<4;i++)if(Rx[i] is <.38f or >.72f)return false;
            return Coupling>.9f&&BendLoss<.045f;
        }
        public bool PassData()
        {
            for(int i=0;i<4;i++)
                if(Bits[i]<6500||RouteBits[i]<3000||Eye[i]<.65f||Jitter[i]>.15f||RawBer(i)>.022f||FinalBer(i)>.002f)return false;
            return true;
        }
        public void Tick(float dt,bool powered,bool docked,bool[] tune,bool compensate,bool calibratingSafety,bool proofOpen,
            bool sourceCommand,bool normalShutter,bool emergencyShutter,bool receiver,bool sending,bool fec,Disturbance fault)
        {
            SourceTemperature=Mathf.MoveTowards(SourceTemperature,powered?.49f:0,dt*.92f);
            WavelengthOffset=Mathf.MoveTowards(WavelengthOffset,fault==Disturbance.Drift?.33f:0,dt*.57f);
            BendLoss=Mathf.MoveTowards(BendLoss,fault==Disturbance.Bend?.62f:0,dt*.93f);
            Coupling=Mathf.MoveTowards(Coupling,docked?.965f:0,dt*.82f);
            Launch=Mathf.MoveTowards(Launch,sourceCommand?.84f:0,dt*(sourceCommand?1.37f:.88f));
            MainClosed=Mathf.MoveTowards(MainClosed,(normalShutter||calibratingSafety&&proofOpen)?0:1,dt*2.72f);
            EmergencyClosed=Mathf.MoveTowards(EmergencyClosed,(emergencyShutter||calibratingSafety&&proofOpen)?0:1,dt*2.35f);

            float heat=fault==Disturbance.Thermal?.50f:.12f,cross=0,gap=1;
            for(int i=0;i<4;i++)
            {
                float adjacent=((i>0?Temperature[i-1]:0)+(i<3?Temperature[i+1]:0))*.5f;
                float induced=heat*adjacent;
                cross=Mathf.Max(cross,induced);
                float command=tune[i]?Setpoint[i]-(compensate?induced:0):0;
                Heater[i]=Mathf.MoveTowards(Heater[i],Mathf.Clamp01(command),dt*1.08f);
                Temperature[i]=Mathf.MoveTowards(Temperature[i],Heater[i]+induced,dt*.73f);
                Detune[i]=Temperature[i]-Setpoint[i]+WavelengthOffset*(.81f+.12f*i);
                Locked[i]=tune[i]&&Launch>.76f&&MainClosed<.04f&&EmergencyClosed<.04f&&Mathf.Abs(Detune[i])<.052f;
                if(i>0)gap=Mathf.Min(gap,1+(Detune[i]-Detune[i-1])*.68f);
                float resonance=Mathf.Exp(-10*Detune[i]*Detune[i]);
                Rx[i]=ThroughPower*(.755f-i*.019f)*resonance;
                Current[i]=Rx[i]*(.68f-i*.025f);
                float clockTarget=receiver&&Locked[i]&&Rx[i]>.36f?1:0;
                Clock[i]=Mathf.MoveTowards(Clock[i],clockTarget,dt*(clockTarget>0?1.34f:2.4f));
                Jitter[i]=.041f+Mathf.Abs(Detune[i])*.48f+induced*.36f+(1-Clock[i])*.28f;
                float contrast=Mathf.Clamp01(Current[i]*3.2f);
                Eye[i]=receiver?Mathf.Clamp01(contrast*(1-Jitter[i]-.21f*BendLoss)*Clock[i]):0;
                if(sending&&Clock[i]>.94f)Transmit(i,fec);
            }
            CrossTalk=cross;SpectrumGap=gap;
        }
        private void Transmit(int channel,bool fec)
        {
            uint prbs=patterns[channel],rng=impairment[channel];
            float probability=.0028f+.16f*(1-Eye[channel])*(1-Eye[channel])+Jitter[channel]*.017f;
            for(int j=0;j<64;j++)
            {
                uint bit=prbs&1;
                prbs=(prbs>>1)|(((prbs^(prbs>>2)^(prbs>>3)^(prbs>>5))&1)<<15);
                rng^=rng<<13;rng^=rng>>17;rng^=rng<<5;
                uint received=bit^((rng&65535)<probability*65536?1u:0u);
                int error=received!=bit?1:0;
                Bits[channel]++;RawErrors[channel]+=error;
                if(!fec){CorrectedErrors[channel]+=error;RouteBits[channel]+=(int)received*(1-error);continue;}
                blockBad[channel]+=error;blockOnes[channel]+=(int)bit;
                if(++blockBits[channel]<15)continue;
                if(blockBad[channel]>2)CorrectedErrors[channel]+=blockBad[channel];
                else RouteBits[channel]+=blockOnes[channel];
                blockBits[channel]=blockBad[channel]=blockOnes[channel]=0;
            }
            patterns[channel]=prbs;impairment[channel]=rng;
        }
    }
}

