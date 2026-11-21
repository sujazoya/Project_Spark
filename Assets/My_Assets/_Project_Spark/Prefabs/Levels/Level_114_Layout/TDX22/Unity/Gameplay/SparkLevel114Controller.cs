using System;
using UnityEngine;

namespace ProjectSpark.Gameplay.ThermalDominion
{
    public sealed partial class SparkLevel114Process
    {
        public SparkTerminal standby, aux, cpuRail, acceleratorRail, fanSense, monitorSupply, protectedFeed;
        public SparkTerminal[] channelFeeds, sensorFeeds;
        public SparkSwitch bench, auxGate, cpuGate, acceleratorGate, vrmGate, fanDrive, heartbeat, workload;
        public SparkResistor cpuLoad, acceleratorLoad, vrmLoad;
        public Renderer[] thermalMarkers;
        public Transform fanRotor;
        public int[] coolingParts;
        public int vrmPart, thermalViaIndex;

        private static readonly float[] CpuDemand = { 5.8f, 1.36f, .53f, .62f };
        private static readonly float[] AcceleratorDemand = { 8.2f, 2.05f, .87f, .93f };
        private static readonly float[] VrmDemand = { 180f, 51f, 26f, 30f };
        private enum Profile { Idle, Nominal, Burst, Sustained }
        private readonly SparkLevel114ThermalPlant thermal = new();
        private SparkLevel114HeatRun run = new();
        private readonly float[] offsets = new float[8];
        private readonly bool[] coolingRetained = new bool[5];
        private Profile profile;
        private float time, accumulator, fanRequest, calibrationAt, lowUntil, fanLow, calibratedFlow, heartbeatAt, lastRise = -1f;
        private bool calibrating, testing, derating, heartbeatDrive, highFeedback, faultWitnessed, reworked;
        private bool armed, interrupted, safetyReset, replacementPrepared;
        private int epoch, calibrationEpoch = -1, qualificationEpoch = -1, runEpoch = -1, requiredEpoch, feedbackIntervals;
        private int activeFault = -1, repaired, repairTarget = -1, inspectedViaEpoch = -1;
        private string benchState = "BENCH ISOLATED", evidence = "";

        public bool CoolingMounted => Array.TrueForAll(coolingParts, Mounted);
        public bool RailsReady => powered && Voltage(standby) > 4.7f && Voltage(aux) > 3.05f &&
            Voltage(cpuRail) > 1.0f && Voltage(acceleratorRail) > 1.09f &&
            Array.TrueForAll(sensorFeeds, pin => Voltage(pin) > 4.7f);
        public bool Calibrated => RailsReady && CoolingMounted && calibrationEpoch == epoch;
        public bool Testing => testing;
        public bool Qualifies => Calibrated && qualificationEpoch == epoch && run.Healthy &&
            thermal.Cpu < 75f && thermal.Accelerator < 74f && thermal.Vrm < 68f;
        public bool Fresh => linked && Qualifies && feedbackIntervals >= 2 && time - lastRise < .51f;
        public bool FaultWitnessed => faultWitnessed;
        public int Epoch => epoch;
        public int Repaired => repaired;
        public float PeakCpu => run.PeakCpu;
        public float PeakAccelerator => run.PeakAccelerator;
        public float PeakVrm => run.PeakVrm;
        public float PeakDelta => run.PeakDelta;
        public float PeakVrmDrop => run.PeakVrmDrop;
        public float MeasuredAirflow => run.SustainedFlow;
        public float TachDisagreement => run.TachError;
        public string Evidence => evidence;

        private Route ThermalVia => routes[thermalViaIndex];
        private int ExpectedPart => activeFault switch { 0 => coolingParts[0], 1 => coolingParts[4], 2 => vrmPart, _ => -1 };
        private bool Mounted(int index)
        {
            int cooling = Array.IndexOf(coolingParts, index);
            return cooling >= 0 && coolingRetained[cooling] && parts[index].contact.IsClosed;
        }

        private void SetMounted(int index, bool retained)
        {
            coolingRetained[Array.IndexOf(coolingParts, index)] = retained;
            parts[index].model.SetActive(retained);
        }
        private bool FaultLive(int index) => activeFault == index && !reworked;
        private void Solve() => manager.ElectricalSolver.SolveNow();

        public bool TogglePower()
        {
            if (powered) { IsolatePower(); return true; }
            if (stage < 2 || !Construction || (!linked && !interrupted))
                return Reject("Complete the native circuit and mount both heatsinks, heatpipes and FAN1.");
            powered = true;
            SetContact(bench, true);
            SetContact(auxGate, true);
            Solve();
            SetContact(cpuGate, Voltage(channelFeeds[0]) > 4.7f);
            SetContact(acceleratorGate, Voltage(channelFeeds[1]) > 4.7f);
            SetContact(vrmGate, Voltage(channelFeeds[2]) > 4.7f);
            SetContact(fanDrive, Voltage(channelFeeds[3]) > 4.7f);
            Solve();
            benchState = "CALIBRATION REQUIRED";
            message = "Calibrate TS1–TS8 against the reference probe and sweep the fan.";
            return true;
        }

        private void IsolatePower()
        {
            powered = false;
            foreach (var contact in new[] { bench, auxGate, cpuGate, acceleratorGate, vrmGate, fanDrive })
                SetContact(contact, false);
            InvalidateDiagnostics();
            Solve();
            benchState = "BENCH ISOLATED";
            message = "Power isolated. The selected assembly can be serviced.";
        }

        private void InvalidateDiagnostics()
        {
            epoch++;
            calibrating = testing = false;
            calibrationEpoch = qualificationEpoch = -1;
            ApplyProfile(Profile.Idle);
            ClearHeartbeat();
            scope?.ResetTrace();
        }

        public bool CalibrateCooling()
        {
            if (!RailsReady || !CoolingMounted || testing || calibrating)
                return Reject("Stable rails, all sensor feeds and every cooling assembly are required.");
            lowUntil = time + .40f;
            calibrationAt = time + .86f;
            fanLow = -1f;
            calibrating = true;
            calibrationEpoch = qualificationEpoch = -1;
            ClearHeartbeat();
            benchState = "CALIBRATING SENSORS";
            message = "Comparing independent thermometry and measuring low and high airflow.";
            return true;
        }

        private void FinishCalibration()
        {
            calibrating = false;
            calibratedFlow = thermal.Airflow - Mathf.Max(0f, fanLow);
            if (!RailsReady || fanLow < 0f || calibratedFlow < .075f || (stage == 2 && calibratedFlow < .45f))
            {
                benchState = "CALIBRATION INCOMPLETE";
                message = "The fan sweep or a sensor feed failed. Check the cooling and wiring.";
                return;
            }
            // An independent reference measures each physical node. The simulated
            // TS channel has its own offset and a solved continuity witness.
            for (int i = 0; i < offsets.Length; i++) offsets[i] = RawSensor(i) - thermal.Sensor(i);
            calibrationEpoch = epoch;
            evidence = $"TS1–TS8 calibrated · airflow rise {calibratedFlow:P0}\nU1 {Sensor(0):F1} °C · U2 {Sensor(1):F1} °C · VRM {Sensor(2):F1} °C";
            benchState = "INSTRUMENTS CALIBRATED";
            message = "Start the idle, nominal, burst and sustained heat profile.";
        }

        private float RawSensor(int i) => thermal.Sensor(i) + (i % 3 - 1) * .37f + .14f;
        private float Sensor(int i) => RawSensor(i) - offsets[i];

        public bool RunEndurance()
        {
            if (!Calibrated || testing || calibrating)
                return Reject("Calibrate the current revision before running the heat profile.");
            run = new SparkLevel114HeatRun();
            runEpoch = epoch;
            testing = true;
            derating = false;
            qualificationEpoch = -1;
            ClearHeartbeat();
            scope?.ResetTrace();
            evidence = "";
            benchState = "THERMAL LOAD PROFILE";
            message = "Measuring all three zones through the 48-second simulated workload.";
            return true;
        }

        private void TickController(float dt)
        {
            time += dt;
            accumulator += dt * 9f;
            while (accumulator >= SparkLevel114ThermalPlant.Step)
            {
                accumulator -= SparkLevel114ThermalPlant.Step;
                SamplePlant();
            }
            if (!powered) return;
            if (calibrating && time >= calibrationAt) FinishCalibration();
            if (stage == 2 && Calibrated && calibratedFlow >= .45f && Award())
                message = "Sensors and independent airflow verified. Start the heat profile.";
            if (stage == 3 && Qualifies && Award())
                message = "Both dies and the VRM passed. Start the first thermal fault trial.";
            if (stage == 4 && reworked && Qualifies)
            {
                repaired++;
                activeFault = repairTarget = -1;
                faultWitnessed = reworked = replacementPrepared = false;
                QueueCheckpoint();
                if (repaired == 3)
                {
                    if (Award()) message = "All repairs passed. Wait for the W257 permit, then open the jumper.";
                }
                else message = "Repair verified under the original load. Start the next fault trial.";
            }
            TickSupervisor();
        }

        private void SamplePlant()
        {
            float elapsed = run.Count * SparkLevel114ThermalPlant.Step;
            Profile request = !testing || elapsed < 4f || elapsed >= 42f ? Profile.Idle :
                elapsed < 11f ? Profile.Nominal : elapsed < 15f ? Profile.Burst : Profile.Sustained;
            if (testing && (thermal.Cpu >= 83f || thermal.Accelerator >= 82f || thermal.Vrm >= 87f))
            {
                request = Profile.Nominal;
                derating = true;
            }
            ApplyProfile(request);
            bool regulatedFan = powered && Voltage(fanSense) > 3f && Mounted(coolingParts[4]);
            if (calibrating && fanLow < 0f && time >= lowUntil) fanLow = thermal.Airflow;
            fanRequest = !regulatedFan ? 0f : calibrating ? time < lowUntil ? .22f : .90f :
                testing && request != Profile.Idle ? .89f : .35f;
            thermal.Advance(powered ? cpuLoad.PowerWatts * 14f : 0f,
                powered ? acceleratorLoad.PowerWatts * 16f : 0f, powered ? vrmLoad.PowerWatts * 11.5f : 0f,
                fanRequest, Mounted(coolingParts[0]), Mounted(coolingParts[1]),
                Mounted(coolingParts[2]), Mounted(coolingParts[3]), FaultLive(0), FaultLive(1), FaultLive(2));
            if (fanRotor && fanRotor.gameObject.activeInHierarchy)
                fanRotor.Rotate(Vector3.up, thermal.Airflow * 67f, Space.Self);
            if (!testing) return;
            bool sensorsAgree = true;
            for (int i = 0; i < offsets.Length; i++) sensorsAgree &= Mathf.Abs(Sensor(i) - thermal.Sensor(i)) < .5f;
            run.Record(thermal, RailsReady && sensorsAgree, derating);
            if (run.Count % 12 == 0) scope?.Record(Sensor(0), Sensor(1), Sensor(2), Sensor(3));
            if (run.Complete) FinishRun();
        }

        private void ApplyProfile(Profile target)
        {
            if (profile == target) return;
            profile = target;
            int index = (int)target;
            cpuLoad.SetResistance(CpuDemand[index]);
            acceleratorLoad.SetResistance(AcceleratorDemand[index]);
            vrmLoad.SetResistance(VrmDemand[index]);
            Solve();
        }

        private void FinishRun()
        {
            testing = false;
            if (run.Healthy && runEpoch == epoch && Calibrated) qualificationEpoch = epoch;
            benchState = Qualifies ? "LOAD PROFILE PASSED" : "THERMAL PROFILE FAILED";
            evidence = $"Peaks U1 {run.PeakCpu:F1}° · U2 {run.PeakAccelerator:F1}° · VRM {run.PeakVrm:F1}°\n" +
                $"TIM rise {run.PeakDelta:F1}° · VRM rise {run.PeakVrmDrop:F1}° · flow {run.SustainedFlow:P0}";
            message = Qualifies ? "Thermal margins and airflow verified across all four workload states." :
                "Compare independent probe readings to locate the failing heat path.";
        }

        public bool BeginFaultTrial()
        {
            if (stage != 4 || !Qualifies || activeFault >= 0 || repaired >= 3)
                return Reject("Complete a healthy heat profile before starting the next fault trial.");
            activeFault = repaired;
            faultWitnessed = reworked = replacementPrepared = false;
            repairTarget = -1;
            InvalidateDiagnostics();
            benchState = "THERMAL FAULT TRIAL";
            message = "A fault was introduced. Recalibrate and repeat the heat profile to measure it.";
            QueueCheckpoint();
            return true;
        }

        public bool LocateFault()
        {
            bool observed = run.Complete && runEpoch == epoch && run.RailsStable && !run.Healthy &&
                (activeFault switch
                {
                    0 => run.PeakDelta >= 18f,
                    1 => run.TachError > .5f && run.SustainedFlow < .35f,
                    2 => run.PeakVrmDrop >= SparkLevel114HeatRun.VrmRiseLimit && ThermalVia.barrel.IsClosed,
                    _ => false
                });
            if (!observed || reworked) return Reject("Capture a failed profile on the current fault revision.");
            faultWitnessed = true;
            footprint = repairTarget = ExpectedPart;
            FocusPart();
            message = activeFault switch
            {
                0 => $"U1-to-HS1 rises {run.PeakDelta:F1} °C. Isolate, remove HS1, renew TIM and remount.",
                1 => $"Tach claims speed with only {run.SustainedFlow:P0} airflow. Isolate and replace FAN1.",
                _ => $"VRM-to-plane rises {run.PeakVrmDrop:F1} °C with continuity intact. Inspect the isolated thermal span."
            };
            QueueCheckpoint();
            return true;
        }

        public bool ToggleService()
        {
            if (powered || stage is < 2 or > 5 || Array.IndexOf(coolingParts, repairTarget) < 0)
                return Reject("Isolate power and select HS1, HS2, HP1, HP2 or FAN1.");
            bool mounting = !Mounted(repairTarget);
            SetMounted(repairTarget, mounting);
            if (mounting && replacementPrepared && activeFault is 0 or 1 && repairTarget == ExpectedPart)
            {
                reworked = true;
                replacementPrepared = false;
            }
            InvalidateDiagnostics();
            message = mounting ? $"{parts[repairTarget].label} seated and retained." :
                $"{parts[repairTarget].label} removed from its original mount.";
            QueueCheckpoint();
            return true;
        }

        public bool InspectThermalVia()
        {
            if (stage != 4 || powered || activeFault != 2 || !faultWitnessed || repairTarget != vrmPart)
                return Reject("Locate the VRM fault and isolate power before inspecting the thermal span.");
            SelectRoute(thermalViaIndex);
            PadView();
            inspectedViaEpoch = epoch;
            message = $"{ThermalVia.viaId} still conducts. The separate heat-transfer sleeve spans " +
                $"L{ThermalVia.layer:00}–L{ThermalVia.exitLayer:00}. Retain its entry layer for repair.";
            return true;
        }

        public bool Repair()
        {
            if (stage != 4 || powered || !faultWitnessed || reworked || repairTarget != ExpectedPart)
                return Reject("Isolate power and select the measured fault target.");
            if (activeFault is 0 or 1)
            {
                if (Mounted(repairTarget)) return Reject("Remove the diagnosed assembly before replacement.");
                replacementPrepared = true;
                message = activeFault == 0 ? "Fresh TIM applied. Remount HS1 to restore clamping." :
                    "Replacement rotor and tach assembly ready. Remount FAN1.";
            }
            else
            {
                if (inspectedViaEpoch != epoch || layer != ThermalVia.layer || !ThermalVia.barrel.IsClosed)
                    return Reject("Inspect the diagnosed thermal span and select its entry layer first.");
                reworked = true;
                message = "Thermal sleeve rebuilt. Electrical plating remains intact. Repeat the heat profile.";
            }
            QueueCheckpoint();
            return true;
        }

        public bool OpenW257()
        {
            if (stage != 5 || !armed || !Fresh || interrupted)
                return Reject("Wait for fresh W257 cadence and a live protected workload.");
            requiredEpoch = epoch + 1;
            if (!RemoveWire(jumper)) return Reject("The native circuit could not open W257.");
            Solve();
            return true;
        }

        public bool RestoreW257()
        {
            if (stage != 5 || !interrupted || linked)
                return Reject("Record the supervised W257 opening before reconnecting it.");
            SelectRoute(routes.Length);
            if (!CreateWire(jumper)) return Reject("The native circuit rejected W257.");
            ReadGraph();
            Solve();
            message = "W257 restored. Calibrate and qualify a fresh heat profile before resetting U10.";
            return true;
        }

        public bool ResetSafety()
        {
            if (stage != 5 || !interrupted || safetyReset || epoch < requiredEpoch || !Fresh || !Inhibited ||
                Voltage(monitorSupply) < 3f)
                return Reject("U10 needs W257 restored with new calibration and heat evidence before reset.");
            safetyReset = true;
            requiredEpoch = epoch + 1;
            InvalidateDiagnostics();
            Solve();
            message = "U10 reset accepted. Recalibrate and repeat the heat profile to restore protected load.";
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
                    float interval = time - lastRise;
                    feedbackIntervals = lastRise >= 0 && interval is >= .23f and <= .46f ? feedbackIntervals + 1 : 0;
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
                benchState = "TRAINING BOARD QUALIFIED";
                message = "U10 restored protected load after the W257 recovery and full requalification.";
            }
        }
    }
}
