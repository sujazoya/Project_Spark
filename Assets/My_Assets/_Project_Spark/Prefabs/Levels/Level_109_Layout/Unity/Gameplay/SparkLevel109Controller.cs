using System;
using UnityEngine;

namespace ProjectSpark.Gameplay.ThermalEndurance
{
    public sealed partial class SparkLevel109Process
    {
        [Serializable]
        public sealed class Phase
        {
            public SparkResistor mosfet, load;
        }

        public GameObject heatsink, spreader;
        public Renderer[] thermalMarkers;
        public SparkTerminal standby, aux, cpuRail, ddrRail, fanSense, memorySense, pcieSense, monitorSupply, protectedFeed;
        public SparkSwitch bench, auxGate, cpuGate, ddrGate, fanDrive, memoryContact, heartbeat, workload;
        public SparkResistor cpuLoad, memoryA, memoryB;
        public Phase[] phases;

        private enum Profile { Idle, Nominal, Peak, Recovery, Burst }
        private static readonly float[] CpuDemand = { 3.15f, .78f, .29f, 1.70f, .33f };
        private static readonly float[] MemoryDemand = { 3.10f, 1.23f, .46f, 2.87f, .42f };
        private static readonly float[] PhaseDemand = { 23f, 11.2f, 6.4f, 18f, 6.7f };
        private readonly SparkLevel109ThermalModel thermal = new();
        private readonly SparkLevel109Endurance endurance = new();
        private readonly float[] currents = new float[4], losses = new float[4];
        private Profile profile;
        private float time, powerAt, accumulator, ambient = 25f, pwm;
        private float calibrationAt, reworkUntil, heartbeatAt, lastRise = -1f, lastThermalSample;
        private bool sinkMounted, spreaderMounted, freshTim, calibrating, calibrateLow, testing, derating;
        private bool armed, interrupted, safetyReset, heartbeatDrive, highFeedback, faultWitnessed, reworked;
        private int epoch, calibrationEpoch = -1, enduranceEpoch = -1, requiredEpoch, captures;
        private int activeFault = -1, repaired, repairTarget = -1, feedbackIntervals;
        private string evidence = "", benchState = "BENCH ISOLATED";

        public bool CoolingMounted => sinkMounted && spreaderMounted;
        public bool RailsReady => powered && Voltage(standby) > 4.7f && Voltage(aux) > 3.05f &&
            Voltage(cpuRail) is >= 1.00f and <= 1.10f && Voltage(ddrRail) is >= 1.14f and <= 1.26f;
        public bool Calibrated => RailsReady && CoolingMounted && calibrationEpoch == epoch &&
            time - lastThermalSample < .24f && Voltage(fanSense) > 3f;
        public bool Calibrating => calibrating;
        public bool Testing => testing;
        public bool Reworking => reworkUntil > time;
        public bool FaultWitnessed => faultWitnessed;
        public bool Qualifies => Calibrated && enduranceEpoch == epoch && endurance.Healthy && thermal.Cpu < 85f;
        public bool Fresh => linked && Qualifies && feedbackIntervals >= 2 && time - lastRise < .53f;
        public int Captures => captures;
        public int Epoch => epoch;
        public int Repaired => repaired;
        public int Bits => endurance.Bits;
        public int Errors => endurance.Errors;
        public int HotErrors => endurance.HotErrors;
        public int ColdErrors => endurance.ColdErrors;
        public int HotOpenings => endurance.HotOpenings;
        public int ColdRecoveries => endurance.ColdRecoveries;
        public float PeakCpu => endurance.PeakCpu;
        public float PeakDelta => endurance.PeakDelta;
        public float PeakPhaseSpread => endurance.PeakPhaseSpread;
        public float PeakPhaseLossRatio => endurance.PeakPhaseLossRatio;
        public float PeakMemory => endurance.PeakMemory;
        public float CpuTemperature => thermal.Cpu;
        public float SinkTemperature => thermal.Heatsink;
        public float VrmTemperature(int phase) => thermal.Phase(phase);
        public string Evidence => evidence;

        private void Solve() => manager.ElectricalSolver.SolveNow();
        private bool FaultLive(int fault) => activeFault == fault && !reworked;
        private int ExpectedPart => activeFault == 0 ? mosfetPart : activeFault == 1 ? -2 : activeFault == 2 ? memoryPart : -1;

        public bool ToggleCooling(bool cpu)
        {
            if (installed != PartCount || powered || Reworking)
                return Reject("Complete the board assembly and isolate power before handling cooling hardware.");
            bool remove = cpu ? sinkMounted : spreaderMounted;
            SetCooling(cpu ? !remove : sinkMounted, cpu ? spreaderMounted : !remove);
            InvalidateDiagnostics();
            if (cpu && sinkMounted && FaultLive(1) && freshTim && faultWitnessed)
            {
                reworked = true;
                message = "New TIM and retention verified. Calibrate cooling and retest the original workload.";
            }
            else message = remove ? "Cooling assembly removed. Inspect the contact before remounting."
                : CoolingMounted ? "Both cooling assemblies are locked down." : "Cooling assembly mounted. Fit the other assembly.";
            QueueCheckpoint();
            return true;
        }

        public void InspectCooling(bool cpu)
        {
            footprint = cpu ? 0 : mosfetPart;
            if (stage == 4 && faultWitnessed) repairTarget = cpu ? -2 : mosfetPart;
            FocusPart();
            message = cpu ? $"CPU junction {thermal.Cpu:F1} °C · sink {thermal.Heatsink:F1} °C · retention {(sinkMounted ? "engaged" : "open")}."
                : $"VRM spreader {thermal.VrmSpreader:F1} °C. Compare phase currents and losses under the same load.";
        }

        private void SetCooling(bool sink, bool vrm)
        {
            sinkMounted = sink;
            spreaderMounted = vrm;
            heatsink.SetActive(sink);
            spreader.SetActive(vrm);
        }

        public bool TogglePower()
        {
            if (powered)
            {
                IsolatePower();
                message = "Bench isolated. Cooling and measured repairs are available.";
                return true;
            }
            if (stage < 2 || !Construction || Reworking || !linked && !interrupted)
                return Reject("Complete copper, both cooling assemblies and W165 before energizing the bench.");
            powered = true;
            InvalidateDiagnostics();
            powerAt = time;
            SetContact(bench, true);
            ApplyProfile(Profile.Idle);
            Solve();
            benchState = "POWER SEQUENCE";
            message = "Bench powered. Wait for the auxiliary, CPU and DDR rails before calibration.";
            QueueCheckpoint();
            return true;
        }

        private void IsolatePower()
        {
            powered = false;
            SetContact(bench, false);
            SetContact(auxGate, false);
            SetContact(cpuGate, false);
            SetContact(ddrGate, false);
            SetContact(fanDrive, false);
            InvalidateDiagnostics();
            ApplyProfile(Profile.Idle);
            Solve();
            benchState = "BENCH ISOLATED";
            QueueCheckpoint();
        }

        private void InvalidateDiagnostics()
        {
            calibrating = testing = false;
            calibrationEpoch = enduranceEpoch = -1;
            ClearHeartbeat();
        }

        public bool CalibrateCooling()
        {
            if (!RailsReady || !CoolingMounted || testing || calibrating || thermal.Cpu > 58f)
                return Reject("Use both cooling mounts and the stable powered idle rails. Wait for the CPU to cool below 58 °C.");
            InvalidateDiagnostics();
            epoch++;
            ApplyProfile(Profile.Idle);
            SetContact(fanDrive, false);
            Solve();
            calibrateLow = Voltage(fanSense) < .25f;
            calibrating = true;
            calibrationAt = time + .24f;
            benchState = "CALIBRATING IDLE SENSORS";
            message = "Sampling the idle junction and the fan response across the calibration pulse.";
            return true;
        }

        public bool RunEndurance()
        {
            if (!Calibrated || testing || calibrating || Reworking)
                return Reject("Calibrate cooling on the current hardware revision before starting endurance.");
            endurance.Begin();
            testing = true;
            enduranceEpoch = -1;
            derating = false;
            ClearHeartbeat();
            scope?.ResetTrace();
            evidence = "";
            benchState = "THERMAL ENDURANCE";
            message = "Running a 48-second simulated cold, peak, recovery, burst and cooldown cycle.";
            return true;
        }

        private void TickController(float dt)
        {
            time += dt;
            if (reworkUntil > 0 && !Reworking)
            {
                reworkUntil = 0;
                reworked = true;
                ConfigureFaultDevices();
                Solve();
                message = "U3 rework complete. Repeat the cold and hot memory cycle.";
                QueueCheckpoint();
            }
            if (powered)
            {
                if (!auxGate.IsClosed && time - powerAt >= .14f && Voltage(standby) > 4.7f)
                {
                    SetContact(auxGate, true);
                    Solve();
                }
                if (!ddrGate.IsClosed && time - powerAt >= .27f && Voltage(aux) > 3f)
                {
                    SetContact(cpuGate, true);
                    SetContact(ddrGate, true);
                    Solve();
                    benchState = "COOLING CHECK PENDING";
                }
                if (calibrating) TickCalibration();
            }

            accumulator += dt * 10f;
            for (int count = 0; count < 16 && accumulator >= SparkLevel109ThermalModel.StepSeconds; count++)
            {
                accumulator -= SparkLevel109ThermalModel.StepSeconds;
                StepThermal();
            }
            if (!powered) return;
            if (stage == 2 && Calibrated && Award())
                message = "Idle sensors and fan response qualified. Run the endurance cycle.";
            if (stage == 3 && Qualifies && Award())
                message = "Endurance qualified. Start the first controlled reliability fault.";
            if (stage == 4 && reworked && Qualifies)
            {
                repaired++;
                activeFault = repairTarget = -1;
                faultWitnessed = reworked = freshTim = false;
                QueueCheckpoint();
                if (repaired == 3)
                {
                    if (Award()) message = "All three repairs qualified. Wait for the W165 heartbeat, then open the jumper.";
                }
                else message = "Repair verified under the original workload. Start the next fault trial.";
            }
            TickSupervisor();
        }

        private void TickCalibration()
        {
            if (time >= calibrationAt - .09f && !fanDrive.IsClosed)
            {
                SetContact(fanDrive, true);
                Solve();
            }
            if (time < calibrationAt) return;
            calibrating = false;
            bool valid = RailsReady && calibrateLow && Voltage(fanSense) > 3f &&
                Mathf.Abs(thermal.Pcb - ambient) < 15f && time - lastThermalSample < .24f;
            if (valid)
            {
                calibrationEpoch = epoch;
                evidence = $"Idle calibration · CPU {thermal.Cpu:F1} °C · sink {thermal.Heatsink:F1} °C\nU5 fan response {Voltage(fanSense):F2} V";
                benchState = "COOLING CALIBRATED";
                message = "Cold-idle sensors and fan response verified. Run endurance.";
            }
            else
            {
                benchState = "CALIBRATION INCOMPLETE";
                message = "Fan, rail or idle baseline check failed. Allow cooldown and measure again.";
            }
        }

        private void ApplyProfile(Profile requested)
        {
            if (profile == requested) return;
            profile = requested;
            int index = (int)requested;
            cpuLoad.SetResistance(CpuDemand[index]);
            memoryA.SetResistance(MemoryDemand[index] + .06f);
            memoryB.SetResistance(MemoryDemand[index]);
            foreach (var phase in phases) phase.load.SetResistance(PhaseDemand[index]);
            Solve();
        }

        private void StepThermal()
        {
            float elapsed = endurance.Tick * SparkLevel109ThermalModel.StepSeconds;
            ambient = testing && (elapsed is >= 8f and < 18f || elapsed is >= 27f and < 42f) ? 36f : 25f;
            if (testing)
            {
                Profile requested = SelectProfile(elapsed);
                if (thermal.Cpu >= 85f && requested is Profile.Peak or Profile.Burst)
                {
                    requested = Profile.Nominal;
                    derating = true;
                }
                ApplyProfile(requested);
            }
            for (int i = 0; i < phases.Length; i++)
            {
                currents[i] = Mathf.Abs(phases[i].mosfet.Current);
                losses[i] = Mathf.Abs(phases[i].mosfet.PowerWatts);
            }
            pwm = Voltage(fanSense) > 3f ? Mathf.Clamp(.24f + (thermal.Cpu - 30f) * .021f, .24f, .93f) : 0f;
            float tim = !sinkMounted ? 0f : FaultLive(1) ? .064f : .48f;
            thermal.Step(ambient, pwm, tim, spreaderMounted, cpuLoad.PowerWatts, memoryA.PowerWatts,
                memoryB.PowerWatts, currents, losses);
            lastThermalSample = time;
            if (FaultLive(2))
            {
                bool shouldClose = memoryContact.IsClosed ? thermal.MemoryB < 49f : thermal.MemoryB <= 44f;
                if (memoryContact.IsClosed != shouldClose)
                {
                    SetContact(memoryContact, shouldClose);
                    if (testing) endurance.RecordContact(shouldClose);
                    Solve();
                }
            }
            if (!testing) return;
            endurance.Sample(thermal, losses, RailsReady && Voltage(fanSense) > 3f,
                Voltage(pcieSense) > 3f, Voltage(memorySense) > 1.05f, derating);
            if (endurance.Tick % 12 == 0) scope?.Record(thermal.Cpu, thermal.Heatsink, thermal.Phase(2), thermal.MemoryB);
            if (endurance.Tick >= SparkLevel109Endurance.TotalTicks) FinishEndurance();
        }

        private static Profile SelectProfile(float elapsed)
        {
            if (elapsed < 3f || elapsed >= 42f) return Profile.Idle;
            if (elapsed < 8f) return Profile.Nominal;
            if (elapsed < 18f) return Profile.Peak;
            if (elapsed < 27f) return Profile.Recovery;
            return Mathf.Repeat(elapsed - 27f, 2f) < 1.55f ? Profile.Burst : Profile.Nominal;
        }

        private void FinishEndurance()
        {
            testing = false;
            ApplyProfile(Profile.Idle);
            enduranceEpoch = epoch;
            captures++;
            benchState = endurance.Healthy ? "ENDURANCE VERIFIED" : "ENDURANCE OUT OF SPEC";
            evidence = $"Run {captures} · CPU peak {endurance.PeakCpu:F1} °C · TIM Δ {endurance.PeakDelta:F1} °C\n" +
                $"Phase 3 Δ {endurance.PeakPhaseSpread:F1} °C · {endurance.Errors:N0}/{endurance.Bits:N0} observed bit errors";
            message = endurance.Healthy ? "Idle-to-peak, ambient cycling, rails and data passed."
                : "Compare phase loss, junction-to-sink delta and warm memory results to locate the failure.";
        }

        public bool BeginFaultTrial()
        {
            if (stage != 4 || activeFault >= 0 || repaired >= 3 || !Qualifies)
                return Reject("Pass a complete endurance cycle before starting the next fault trial.");
            activeFault = repaired;
            repairTarget = -1;
            reworked = faultWitnessed = freshTim = false;
            evidence = "";
            InvalidateDiagnostics();
            ConfigureFaultDevices();
            Solve();
            benchState = "RELIABILITY TRIAL";
            message = "Calibrate cooling again, run the original endurance profile and use its measurements to locate the fault.";
            QueueCheckpoint();
            return true;
        }

        private void ConfigureFaultDevices()
        {
            phases[2].mosfet.SetResistance(FaultLive(0) ? 2.3f : .18f);
            SetContact(memoryContact, !FaultLive(2) || thermal.MemoryB < 49f);
        }

        public bool LocateFault()
        {
            bool observed = powered && enduranceEpoch == epoch && (activeFault switch
            {
                0 => endurance.PeakPhaseSpread >= 5f && endurance.PeakPhaseLossRatio >= 2.4f,
                1 => endurance.PeakDelta >= 18f && sinkMounted,
                2 => endurance.HotOpenings >= 1 && endurance.ColdRecoveries >= 1 && endurance.HotErrors > 0 && endurance.ColdErrors == 0,
                _ => false
            });
            if (!observed || reworked) return Reject("Acquire the failed test’s actual phase, interface or warm-memory evidence first.");
            faultWitnessed = true;
            repairTarget = ExpectedPart;
            footprint = activeFault == 1 ? 0 : repairTarget;
            FocusPart();
            message = activeFault switch
            {
                0 => $"Q05 phase 3 shows {endurance.PeakPhaseLossRatio:F1}× peer loss. Isolate and replace the measured MOSFET.",
                1 => $"CPU-to-sink delta reached {endurance.PeakDelta:F1} °C. Isolate, remove the heatsink, renew TIM, then remount.",
                _ => $"U3 opened warm and recovered cold. Isolate and rework its diagnosed BGA contact."
            };
            QueueCheckpoint();
            return true;
        }

        public bool Repair()
        {
            if (stage != 4 || powered || !faultWitnessed || reworked || Reworking || repairTarget != ExpectedPart)
                return Reject("Isolate bench power and select the diagnosed repair target.");
            if (activeFault == 1)
            {
                if (sinkMounted) return Reject("Remove the CPU heatsink before changing the thermal interface.");
                freshTim = true;
                message = "Thermal interface renewed. Remount the CPU heatsink to engage retention.";
            }
            else if (activeFault == 2)
            {
                reworkUntil = time + 1.15f;
                benchState = "ISOLATED BGA REWORK";
                message = "Executing the isolated BGA rework cycle. Bench power is locked out.";
            }
            else
            {
                reworked = true;
                ConfigureFaultDevices();
                Solve();
                message = "Q05 replaced. Calibrate and rerun the same phase workload.";
            }
            QueueCheckpoint();
            return true;
        }

        public bool OpenW165()
        {
            if (stage != 5 || !armed || !Fresh || interrupted)
                return Reject("Wait for healthy cadence and an enabled peak workload relay.");
            requiredEpoch = epoch + 1;
            if (!RemoveWire(jumper)) return Reject("Could not open the native W165 connection.");
            Solve();
            return true;
        }

        public bool RestoreW165()
        {
            if (stage != 5 || !interrupted || linked) return Reject("First record the supervised physical W165 opening.");
            SelectRoute(routes.Length);
            if (!CreateWire(jumper)) return Reject("W165 terminals rejected the connection.");
            ReadGraph();
            Solve();
            message = "W165 restored. Calibrate cooling and repeat endurance before resetting U19.";
            return true;
        }

        public bool ResetSafety()
        {
            if (stage != 5 || !interrupted || safetyReset || epoch < requiredEpoch || !Fresh || !Inhibited ||
                Voltage(monitorSupply) < 3f)
                return Reject("Reconnect W165 and obtain a fresh thermal cycle and heartbeat before resetting U19.");
            safetyReset = true;
            requiredEpoch = epoch + 1;
            InvalidateDiagnostics();
            Solve();
            message = "U19 reset accepted. Obtain a new calibration and endurance result to restore the peak workload relay.";
            return true;
        }

        private void ClearHeartbeat()
        {
            feedbackIntervals = 0;
            lastRise = -1;
            highFeedback = heartbeatDrive = false;
            heartbeatAt = time;
            SetContact(heartbeat, false);
            SetContact(workload, false);
        }

        private void TickSupervisor()
        {
            if (Qualifies && time >= heartbeatAt)
            {
                heartbeatDrive = !heartbeatDrive;
                heartbeatAt = time + .15f;
                SetContact(heartbeat, heartbeatDrive);
                Solve();
                bool high = Voltage(jumper.to) > 2.8f;
                if (high && !highFeedback)
                {
                    float period = time - lastRise;
                    feedbackIntervals = lastRise >= 0 && period is >= .23f and <= .46f ? feedbackIntervals + 1 : 0;
                    lastRise = time;
                }
                highFeedback = high;
            }
            bool permit = Fresh && Voltage(monitorSupply) > 3f && (!interrupted || safetyReset && epoch >= requiredEpoch);
            if (workload.IsClosed != permit)
            {
                SetContact(workload, permit);
                Solve();
            }
            if (!permit || Voltage(protectedFeed) <= 3f || stage != 5) return;
            if (!interrupted) armed = true;
            else if (safetyReset && Award())
            {
                benchState = "THERMAL ENDURANCE CERTIFIED";
                message = "Fresh thermal evidence restored the independent W165 peak workload permit.";
            }
        }
    }
}
