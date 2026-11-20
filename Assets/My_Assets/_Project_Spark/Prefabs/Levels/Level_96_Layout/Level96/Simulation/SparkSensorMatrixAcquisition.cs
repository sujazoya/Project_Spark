using System;
using UnityEngine;

namespace ProjectSpark.SensorMatrix
{
    public enum SensorSampleStatus { Valid, Stale, Saturated, OpenCircuit, ReferenceUntrusted, Uncalibrated, OutOfRange }

    // Engineering-domain model. Input voltages come from SparkElectricalSolver.
    // The fixed clock samples the latest held electrical solution at 128 Hz per channel.
    [Serializable]
    public sealed class SparkSensorMatrixAcquisition
    {
        [Serializable] public struct RawSample
        { public double timestamp,voltage,reference;public int code;public SensorSampleStatus status; }
        [Serializable] public sealed class Channel
        {
            public string name,unit;
            public double low,high,stimulus,filtered,voltage,value,timestamp;
            public int rawCode,samples,historyWrite,historyCount;
            public RawSample[] history=new RawSample[256];
            public void Record(double time,double vref){if(history==null||history.Length!=256)history=new RawSample[256];history[historyWrite]=new RawSample{timestamp=time,voltage=voltage,reference=vref,code=rawCode,status=status};historyWrite=(historyWrite+1)%history.Length;historyCount=Math.Min(historyCount+1,history.Length);}
            public double zeroCode,spanCode;
            public bool zeroCaptured,calibrated;
            public SensorSampleStatus status=SensorSampleStatus.OpenCircuit;
        }

        public Channel[] channels={
            new(){name="RTD TEMPERATURE",unit="°C",low=10,high=70,stimulus=40},
            new(){name="BRIDGE FORCE",unit="N",low=0,high=100,stimulus=50},
            new(){name="OPTICAL LIGHT",unit="lux",low=0,high=1000,stimulus=400},
            new(){name="HALL CURRENT",unit="A",low=0,high=2,stimulus=1}
        };
        public double clock,reference=2.5,standardStarted;
        public int standard=-1;
        public bool referenceChecked;
        public const double SamplePeriod=1d/128d;
        [SerializeField] private double accumulated,muxHold;
        public double muxSettlingSeconds=.0014;
        public const double MuxTimeConstant=.0001;
        [SerializeField] private uint random=103;
        public bool AllCalibrated => Array.TrueForAll(channels,c=>c.calibrated);
        public bool AllValid => Array.TrueForAll(channels,c=>c.status==SensorSampleStatus.Valid && clock-c.timestamp<.1);
        public int TotalSamples => channels[0].samples+channels[1].samples+channels[2].samples+channels[3].samples;

        public void SelectStandard(int selected)
        {
            standard=selected;
            standardStarted=clock;
            for(int i=0;i<channels.Length;i++)channels[i].stimulus=selected==0?channels[i].low:channels[i].high;
        }

        public void InvalidateCalibration()
        {
            referenceChecked=false;
            foreach(var c in channels){c.calibrated=false;c.zeroCaptured=false;}
        }

        public double SensorVoltage(int index,double excitation)
        {
            var value=channels[index].stimulus;
            return index switch {
                0 => .0005*1000*(1+.00385*value)+.000001, // 0.5 mA RTD, bounded self heating.
                1 => excitation*.5+excitation*.004*value/100,
                2 => 40e-9*value*47000+1e-9*47000, // Photodiode Norton/Thevenin equivalent.
                _ => .25+.8*value
            };
        }

        public double FrontEndVoltage(int index,double input,double minus,double referenceVoltage,double opticalResistance)
        {
            double output=index switch {
                0 => .2+(input-.51925)*(2d/.1155),
                1 => .08*referenceVoltage+200*(input-minus),
                2 => .2+input*(opticalResistance/47000d),
                _ => input
            };
            return Math.Clamp(output,0,3.15);
        }

        public void Step(double dt,double[] volts,bool[] connected,bool trusted,double vref,bool solvedFresh)
        {
            if(dt<=0 || double.IsNaN(dt))return;
            accumulated+=Math.Min(dt,.25);
            reference=double.IsFinite(vref)?vref:0;
            while(accumulated>=SamplePeriod)
            {
                accumulated-=SamplePeriod;
                clock+=SamplePeriod;
                for(int i=0;i<4;i++)
                {
                    var channel=channels[i];
                    if(!connected[i]){channel.status=SensorSampleStatus.OpenCircuit;continue;}
                    if(!solvedFresh || !double.IsFinite(volts[i])){channel.status=SensorSampleStatus.Stale;continue;}
                    if(!double.IsFinite(vref)||vref<.05){channel.status=SensorSampleStatus.ReferenceUntrusted;continue;}
                    double alpha=1-Math.Exp(-SamplePeriod/.018);
                    channel.filtered+=alpha*(volts[i]-channel.filtered);
                    double budget=Math.Clamp(muxSettlingSeconds,0,SamplePeriod/4);
                    muxHold=channel.filtered+(muxHold-channel.filtered)*Math.Exp(-budget/MuxTimeConstant);
                    bool settled=Math.Abs(muxHold-channel.filtered)<vref/65535d*.5;
                    double noise=(NextNoise()-.5)*.000012;
                    channel.voltage=channel.filtered;
                    channel.rawCode=(int)Math.Clamp(Math.Round((muxHold+noise)/vref*65535),0,65535);
                    channel.timestamp=clock;
                    if(!settled){channel.status=SensorSampleStatus.Stale;channel.Record(clock,vref);continue;}
                    if(!trusted || vref<2.475 || vref>2.525)
                    {channel.status=SensorSampleStatus.ReferenceUntrusted;channel.Record(clock,vref);continue;}
                    channel.samples++;
                    if(volts[i]>=vref-.001 || volts[i]<-.005){channel.status=SensorSampleStatus.Saturated;channel.Record(clock,vref);continue;}
                    if(!channel.calibrated){channel.status=SensorSampleStatus.Uncalibrated;channel.Record(clock,vref);continue;}
                    double fraction=(channel.rawCode-channel.zeroCode)/(channel.spanCode-channel.zeroCode);
                    channel.value=channel.low+fraction*(channel.high-channel.low);
                    double margin=(channel.high-channel.low)*.012;
                    channel.status=channel.value<channel.low-margin || channel.value>channel.high+margin ?
                        SensorSampleStatus.OutOfRange : SensorSampleStatus.Valid;
                    channel.Record(clock,vref);
                }
            }
        }

        public bool CaptureStandard(out string reason)
        {
            reason="";
            if(!referenceChecked){reason="Verify the independent 2.500 V reference first.";return false;}
            if(standard<0 || clock-standardStarted<.65){reason="Hold the known zero/span standard for at least 0.65 seconds.";return false;}
            if(Array.Exists(channels,c=>clock-c.timestamp>.1 || c.status is SensorSampleStatus.OpenCircuit or
                SensorSampleStatus.ReferenceUntrusted or SensorSampleStatus.Saturated or SensorSampleStatus.Stale))
            {reason="Every channel needs a fresh, unclipped, connected ADC sample.";return false;}
            if(standard==1 && Array.Exists(channels,c=>!c.zeroCaptured || c.rawCode-c.zeroCode<100))
            {reason="Capture a valid zero standard before the span standard.";return false;}
            foreach(var c in channels)
            {
                if(standard==0){c.zeroCode=c.rawCode;c.zeroCaptured=true;c.calibrated=false;}
                else{c.spanCode=c.rawCode;c.calibrated=true;}
            }
            reason=standard==0?"Known zero standards recorded. Select and capture span.":"All four zero/span calibrations recorded from ADC codes.";
            return true;
        }

        public void DynamicStimulus(double elapsed)
        {
            standard=-1;
            double fraction=Math.Clamp(elapsed/6d,0,1);
            // Reproducible full-range sweep contains independent low/high checks.
            double normalized=.5-.5*Math.Cos(fraction*Math.PI*2);
            foreach(var channel in channels)
                channel.stimulus=channel.low+(channel.high-channel.low)*normalized;
        }

        public bool WithinUncertainty()
        {
            if(!AllValid)return false;
            foreach(var c in channels)
                if(Math.Abs(c.value-c.stimulus)>Math.Max(.005,(c.high-c.low)*.045))return false;
            return true;
        }

        private double NextNoise()
        {
            if(random==0)random=103; // Legacy saves did not retain the PRNG state.
            random^=random<<13;random^=random>>17;random^=random<<5;
            return random/(double)uint.MaxValue;
        }
    }
}

