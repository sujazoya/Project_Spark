using System;
using System.Linq;
using ProjectSpark.SensorMatrix;
using UnityEngine;

namespace ProjectSpark.Gameplay
{
    public sealed partial class SparkLevel103Process
    {
        private bool validPower,trusted,previousJumper;
        private double measuredReference;
        private readonly double[] adcVoltages=new double[4];
        private readonly bool[] channelConnected=new bool[4];

        private double Voltage(SparkTerminal terminal)
        {
            if(!terminal || !manager || manager.ElectricalSolver.LatestSolveResult==null)return double.NaN;
            return manager.ElectricalSolver.LatestSolveResult.TryGetVoltage(terminal,ground,out float value)?value:double.NaN;
        }

        private static void Drive(SparkPowerSupply device,double voltage,bool enabled)
        {
            if(!device)return;
            if(double.IsFinite(voltage) && Math.Abs(device.OutputVoltage-voltage)>.00001)
                {
                if(device.IsOutputActive)device.SetOutput(false,default);
                device.TryConfigure((float)Math.Clamp(voltage,0,5.1),device.CurrentLimit,out _);
            }
            bool on=enabled && device.ElectricalEnabled;
            if(device.IsOutputActive!=on)device.SetOutput(on,default);
        }

        private void Update()
        {
            if(!Active || placed==null)return;
            using(manager.ElectricalSolver.BeginConfigurationBatch())
            {
            if(running)acquisition.DynamicStimulus(acquisition.clock-runStarted);
            ReadGraph();
            validPower=supply.IsOutputActive && !supply.IsOverloaded && Voltage(protectedInput) is >3.5 and <5.15;
            Drive(logic,3.3,validPower);
            bool logicValid=Voltage(logicInput) is >3.15 and <3.45;
            Drive(reference,fault==0?2.37:2.5,validPower && logicValid);
            measuredReference=Voltage(reference.PositiveTerminal);
            double adcReference=Voltage(referenceInput);
            bool referenceHealthy=schematicResolved && !unauthorizedCopper && validPower && logicValid && double.IsFinite(measuredReference) &&
                Math.Abs(measuredReference-2.5)<.015 && double.IsFinite(adcReference) &&
                Math.Abs(adcReference-measuredReference)<.008 && RouteReady(10) && RouteReady(11) && RouteReady(12);
            Drive(supervisor,3.3,referenceHealthy);
            Drive(bridgeCommon,double.IsFinite(measuredReference)?measuredReference*.5:1.25,validPower && logicValid);
            bool jumper=RouteReady(58);
            trusted=referenceHealthy && jumper && Voltage(trustedInput) is >3.05 and <3.45;
            if(trusted && referenceGoodSince<0)referenceGoodSince=acquisition.clock;
            if(!trusted)referenceGoodSince=-1;
            if((armed || stage>=2 && previousJumper) && !jumper)
            {
                armed=false;latched=true;lossObserved=true;recoveryReset=false;running=false;
                acquisition.InvalidateCalibration();revision++;
                notice="W059 continuity lost. Physical sensor states retained; trusted samples invalid and safety latched.";
                effects.Emit(routes[58].from.transform.position,2);
            }
            previousJumper=jumper;

            for(int channel=0;channel<4;channel++)
            {
                int offset=18+channel*8;
                bool connected=validPower;
                for(int i=offset;i<offset+8;i++)connected &= RouteReady(i);
                channelConnected[channel]=connected;
                double excitation=channel==1 && double.IsFinite(measuredReference)?measuredReference:2.5;
                Drive(excitations[channel],excitation,validPower && logicValid);
                Drive(sensorOutputs[channel],acquisition.SensorVoltage(channel,excitation),connected);
            }
            if(manager.ElectricalSolver.IsDirty)manager.ElectricalSolver.SolveNow();
            for(int channel=0;channel<4;channel++)
            {
                double excitation=channel==1 && double.IsFinite(measuredReference)?measuredReference:2.5;
                bool connected=channelConnected[channel];
                double input=Voltage(sensorInputs[channel]);
                double minus=channel==1?Voltage(minusInputs[channel]):0;
                double front=double.IsFinite(input) && double.IsFinite(minus) ?
                    acquisition.FrontEndVoltage(channel,input,minus,excitation,opticalFeedback.Resistance):0;
                Drive(frontEnds[channel],front,connected);
                double measured=Voltage(adcInputs[channel]);
                adcVoltages[channel]=double.IsFinite(measured)?measured:0;
                channelConnected[channel] &= double.IsFinite(measured);
            }
            if(manager.ElectricalSolver.IsDirty)manager.ElectricalSolver.SolveNow();
            for(int channel=0;channel<4;channel++)
            {
                double measured=Voltage(adcInputs[channel]);
                adcVoltages[channel]=double.IsFinite(measured)?measured:0;
            }
            acquisition.Step(Time.deltaTime,adcVoltages,channelConnected,trusted,adcReference,
                !manager.ElectricalSolver.IsSolving && !manager.ElectricalSolver.IsDirty);
            bool healthy=trusted && !latched && acquisition.AllValid && fault<0;
            Drive(safetyLamp,3.3,healthy);
            if(stage==0 && placed.All(value=>value) && soldered.All(value=>value))
                Award("44 correct packages placed at their footprints and all 187 solder contact switches closed.");
            if(stage==1 && routed==58 && viaCount==12 && trusted)
                Award("58 player copper tasks, 12 plated vias, live rails and actual W059 continuity verified.");
            if(stage==2 && acquisition.AllCalibrated && acquisition.referenceChecked && trusted)
                Award("Independent reference verification and eight measured zero/span standards recorded.",acquisition.TotalSamples);

            if(running)UpdateRun();
            if(Time.unscaledTime>=nextPresentation){nextPresentation=Time.unscaledTime+.12f;Present();}
            }
        }

        public void TogglePower()
        {
            if(!Active)return;
            supply.SetOutput(!supply.IsOutputActive,default);
            running=false;
            notice=supply.IsOutputActive?"5 V supply enabled. Wait for settling and verify reference.":"5 V supply isolated. Rework is permitted.";
        }

        public void CheckReference()
        {
            if(!trusted || referenceGoodSince<0 || acquisition.clock-referenceGoodSince<.65 ||
                double.IsNaN(measuredReference)||Math.Abs(measuredReference-2.5)>.015)
            {notice="Reference requires trusted W059 continuity and 0.65 seconds of fresh settled 2.500 V evidence.";return;}
            acquisition.referenceChecked=true;
            notice=$"Independent 2.500 V reference checked: {measuredReference:F5} V, tolerance ±0.015 V. Select ZERO STANDARD.";
        }
        public void ZeroStandard(){running=false;acquisition.SelectStandard(0);notice="Known standards 10 °C / 0 N / 0 lux / 0 A applied. Wait 0.65 s then CAPTURE.";}
        public void SpanStandard(){running=false;acquisition.SelectStandard(1);notice="Known standards 70 °C / 100 N / 1000 lux / 2 A applied. Wait 0.65 s then CAPTURE.";}
        public void Capture(){acquisition.CaptureStandard(out notice);}

        public void TestBoard()
        {
            if(!Active || stage<3 || !schematicResolved || unauthorizedCopper || !trusted || latched || fault>=0 || !acquisition.AllCalibrated ||
                !acquisition.referenceChecked || routed!=58 || viaCount!=12)
            {notice="Qualification needs all copper/vias, trusted reference, calibration and a reset safety latch.";return;}
            runStarted=acquisition.clock;elapsedRun=0;runSamples=acquisition.TotalSamples;
            runRevision=manager.CircuitSystem.TopologyVersion;
            running=true;
            notice="Four-channel 6-second acquisition qualification running.";
        }

        private void UpdateRun()
        {
            elapsedRun=acquisition.clock-runStarted;

            // Allow the analog filters to settle from the preceding calibration standard.
            if(elapsedRun>.65 && (!trusted || latched || fault>=0 || routed!=58 ||
                manager.CircuitSystem.TopologyVersion!=runRevision || !acquisition.WithinUncertainty()))
            {
                running=false;
                notice="Qualification rejected at "+elapsedRun.ToString("F3")+" s; graph "+manager.CircuitSystem.TopologyVersion+"/"+runRevision+" valid="+acquisition.AllValid+"; "+string.Join("; ",acquisition.channels.Select(c=>$"{c.name} {c.value:F3}/{c.stimulus:F3} {c.status}"));
                return;
            }
            if(elapsedRun<6.2)return;
            running=false;
            int samples=acquisition.TotalSamples-runSamples;
            if(samples<4*128*5){notice="Acquisition throughput below 128 samples/s/channel.";return;}
            if(stage==3)Award("Fresh four-channel dynamic sweep within 4.5% full-scale educational uncertainty.",samples,runStarted);
            else if(stage==4 && pendingRepair>=0)
            {
                repairProved[pendingRepair]=true;pendingRepair=-1;
                if(repairProved.All(value=>value))Award("F1, F2 and F3 each diagnosed, physically repaired, independently calibrated and requalified.",samples,runStarted);
            }
            else if(stage==5 && lossObserved && recoveryReset)
                Award("W059 open detected; trusted reference reacquired, recalibrated, explicitly reset and qualified again.",samples,runStarted);
            notice="Measured acquisition passed. "+(stage==4?"Select the next fault scenario.":stage==5?"Arm the supervised W059 interruption.":"");
        }

        private void Inject(int kind)
        {
            if(!Active || stage!=4 || fault>=0 || repairProved[kind])return;
            running=false;fault=kind;diagnosed=false;revision++;pendingRepair=-1;
            if(kind==1)routes[31].via.SetState(SparkSwitchState.Open);
            if(kind==2)
            {
                opticalFeedback.SetResistance(470000);
                acquisition.channels[2].stimulus=1000;
            }
            acquisition.InvalidateCalibration();
            notice=new[]{"F1 reference drift injected.","F2 bridge via V02 physically open.","F3 optical gain x10 causes ADC saturation."}[kind]+" Diagnose before rework.";
            effects.Emit(parts[Array.FindIndex(design.parts,item=>item.reference==(kind==0?"U6":kind==1?"U3":"U4"))].target.transform.position,2);
        }
        public void FaultReference()=>Inject(0);
        public void FaultVia()=>Inject(1);
        public void FaultOptical()=>Inject(2);
        public void ArmW059()
        {
            if(stage!=5 || !trusted || latched)return;
            armed=true;selectedRoute=58;
            notice="Supervised W059 test armed. Remove the actual patch with UNDO or Wire Manager.";
        }
        public void ResetSafety()
        {
            if(!lossObserved || !latched || !trusted || !acquisition.AllCalibrated || !acquisition.referenceChecked)
            {notice="Reinstall W059, verify reference and capture new zero/span calibration before reset.";return;}
            latched=false;recoveryReset=true;revision++;
            notice="Safety latch reset. Start a complete fresh four-channel qualification.";
        }
    }
}



