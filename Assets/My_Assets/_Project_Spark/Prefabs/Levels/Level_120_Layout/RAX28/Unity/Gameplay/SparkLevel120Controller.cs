using System;
using UnityEngine;

namespace ProjectSpark.Gameplay.AxisForge
{
    public sealed partial class SparkLevel120Process
    {
        public SparkTerminal mainRail, monitorSupply, prechargePin, dcLinkPin, stoA, stoB, phaseB, protectedFeed;
        public SparkTerminal gateWitness, resolverWitness, currentWitness, brakeWitness;
        public SparkTerminal[] drivePins, encoderPins, thermalPins;
        public SparkSwitch bench, monitorGate, prechargeGate, contactor, stoGateA, stoGateB, phaseGate, heartbeat, workload;
        public SparkSwitch[] driveGates, encoderGates, thermalGates;
        public Renderer[] axisMarkers;
        public GameObject assemblyArt;
        public int gatePart, encoderPart, thermalPart, encoderViaIndex;

        public enum AxisFailure { None, PhaseB, EncoderD, ThermalF, Workspace, Protection, Tracking }
        private enum Motion { Idle, Home, Service, Protected }
        private readonly SparkLevel120Robot robot = new();
        private readonly bool[] drives = new bool[6], encoders = new bool[6], thermals = new bool[6];
        private Motion motion;
        private float time, accumulator, traceAt, nextHeartbeat, lastRise = -1f, linkChargedAt = -1f;
        private bool driverHeartbeat, highFeedback, stoProofA, stoProofB, homed, workspaceProof;
        private bool faultWitnessed, reworked, armed, interrupted, safetyReset;
        private int epoch, qualifiedEpoch = -1, runEpoch = -1, minimumEpoch, feedbackIntervals;
        private int activeFault = -1, repaired, repairTarget = -1, viaInspectionEpoch = -1, spare, selectedAxis;
        private string evidence = "", benchState = "BENCH ISOLATED";

        public bool ChassisReady => powered && Live(mainRail) && Live(monitorSupply);
        public bool LinkReady => ChassisReady && Live(dcLinkPin) && robot.Bus > 10.85f;
        public bool StoA => Live(stoA);
        public bool StoB => Live(stoB);
        public bool BothStoTested => stoProofA && stoProofB;
        public bool Homed => homed;
        public bool WorkspaceProof => workspaceProof;
        public bool Running => motion != Motion.Idle;
        public bool ProtectedRunning => motion == Motion.Protected;
        public bool TorqueOn => robot.TorqueOn;
        public bool Tripped => interrupted;
        public bool Qualifies => qualifiedEpoch == epoch && LinkReady && StoA && StoB &&
            BothStoTested && homed && workspaceProof && ReadyChannels && Live(phaseB);
        public bool Fresh => linked && Qualifies && feedbackIntervals >= 2 && time - lastRise < .49f;
        public int Repaired => repaired;
        public int Epoch => epoch;
        public int ReadyAxes => CountReady();
        public float Bus => robot.Bus;
        public float Speed => robot.PeakSpeed;
        public float PeakError => robot.PeakError;
        public float ToolErrorMm => robot.PeakToolErrorMm;
        public float IkResidualMm => robot.MaxIkResidualMm;
        public float IkOrientationDeg => robot.MaxIkOrientationDeg;
        public float PeakCurrent => robot.PeakCurrent;
        public float PeakTemperature => robot.PeakTemperature;
        public float MinimumBus => robot.MinimumBus;
        public float BrakeEnergy => robot.BrakeEnergy;
        public float RegenEnergy => robot.RegenEnergy;
        public int CartesianSamples => robot.CartesianSamples;
        public int PhaseClips => robot.ClippedPhaseSamples;
        public int EncoderMisses => robot.EncoderMisses;
        public float PhaseImbalance => robot.PeakPhaseImbalance;
        public float EncoderDisagreement => robot.EncoderDisagreement;
        public float DerateSeconds => robot.ThermalDerateTime;
        public float TripSpeed { get; private set; }
        public float ProtectedToolError { get; private set; }
        public AxisFailure Failure { get; private set; }
        public string Evidence => evidence;
        public float Travel(int axis) => robot.Joints[axis].Range;

        private Route EncoderVia => routes[encoderViaIndex];
        private int ExpectedPart => activeFault switch { 0 => gatePart, 1 => encoderPart, 2 => thermalPart, _ => -1 };
        private bool FaultLive(int index) => activeFault == index && !reworked;
        private bool Live(SparkTerminal pin) => Voltage(pin) > 10.8f;
        private void Solve() => manager.ElectricalSolver.SolveNow();
        private bool ReadyChannels => CountReady() == 6 && Live(gateWitness) && Live(resolverWitness) &&
            Live(currentWitness) && Live(brakeWitness);

        private int CountReady()
        {
            if (!LinkReady) return 0;
            int count = 0;
            for (int axis = 0; axis < 6; axis++)
                if (Live(drivePins[axis]) && Live(encoderPins[axis]) && Live(thermalPins[axis])) count++;
            return count;
        }

        public bool TogglePower()
        {
            if (stage < 2 || stage >= 6 || !Construction || !linked && !interrupted)
                return Reject("Complete the native continuity check before enabling the virtual bench.");
            if (powered)
            {
                IsolatePower();
                return true;
            }
            powered = true;
            using (manager.ElectricalSolver.BeginConfigurationBatch())
            {
                SetContact(bench, true);
                SetContact(monitorGate, true);
                SetContact(stoGateA, true);
                SetContact(stoGateB, true);
                for (int axis = 0; axis < 6; axis++)
                {
                    SetContact(encoderGates[axis], true);
                    SetContact(thermalGates[axis], true);
                }
            }
            message = "Start shared-link precharge. Close the contactor after the voltage settles.";
            return true;
        }

        public bool BeginPrecharge()
        {
            if (!ChassisReady || contactor.IsClosed || Running)
                return Reject("Enable bench power before precharge.");
            SetContact(prechargeGate, true);
            Solve();
            linkChargedAt = -1f;
            message = "Precharging the virtual shared link. Wait for a stable 11.1 V.";
            return true;
        }

        public bool CloseContactor()
        {
            if (!ChassisReady || !Live(prechargePin) || linkChargedAt < 0f || time - linkChargedAt < .16f)
                return Reject("Precharge has not settled at 11.1 V.");
            using (manager.ElectricalSolver.BeginConfigurationBatch())
            {
                SetContact(contactor, true);
                SetContact(prechargeGate, false);
                foreach (var drive in driveGates) SetContact(drive, true);
                SetContact(phaseGate, !FaultLive(0));
            }
            message = "Prove STO A and B independently, then home the six joints.";
            return true;
        }

        public bool ProveSto(int channel)
        {
            if (!LinkReady || Running || channel is < 0 or > 1 || !StoA || !StoB)
                return Reject("Close the charged contactor and stop motion before probing STO.");
            var contact = channel == 0 ? stoGateA : stoGateB;
            SetContact(workload, false);
            SetContact(contact, false);
            Solve();
            bool independent = (channel == 0 ? !StoA && StoB : StoA && !StoB) && Live(monitorSupply);
            robot.CutTorque();
            SetContact(contact, true);
            Solve();
            if (!independent || !StoA || !StoB) return Reject("Independent STO drop or restore measurement failed.");
            if (channel == 0) stoProofA = true;
            else stoProofB = true;
            evidence = $"STO {(channel == 0 ? "A" : "B")} opened alone. The other channel and U48 stayed live.";
            message = BothStoTested ? "Both safety channels proved independently. Home all six joints." : "Prove the other STO channel.";
            return true;
        }

        public bool HomeAxes()
        {
            if (!LinkReady || !ExerciseInterlocksReady() || !ReadyChannels || !Live(phaseB) ||
                Running || Speed >= 5f)
                return Reject("Close the charged link, prove both STO channels and verify all feedback at standstill.");
            homed = workspaceProof = false;
            qualifiedEpoch = -1;
            BeginMotion(Motion.Home);
            message = "Homing six virtual joints and comparing each encoder with its independent position reference.";
            return true;
        }

        public bool TestEnvelope()
        {
            if (!homed || !LinkReady || Running || !StoA || !StoB || TorqueOn)
                return Reject("Home all joints and stop motion before checking the protected envelope.");
            workspaceProof = robot.RejectsForbiddenTarget();
            if (!workspaceProof) return Reject("Workspace check did not reject the below-floor target.");
            evidence = "Reachable inverse solution rejected below the workcell floor. Actuator torque stayed off.";
            message = "Workspace boundary proved. Run the joint and Cartesian path.";
            return true;
        }

        public bool RunTrajectory()
        {
            if (stage is < 3 or > 5 || !ExerciseInterlocksReady() || !homed || !workspaceProof || Running || Speed >= 5f)
                return Reject("Home all axes and verify the workspace before running the service path.");
            qualifiedEpoch = runEpoch = -1;
            BeginMotion(Motion.Service);
            message = "Isolated service path · coordinated joint move, Cartesian interpolation and return.";
            return true;
        }

        public bool StartProtectedMotion()
        {
            if (stage != 5 || Running || !Fresh || Inhibited || interrupted && !safetyReset)
                return Reject("U48 needs fresh coordinated-motion evidence and its solved permit.");
            armed = true;
            BeginMotion(Motion.Protected);
            message = interrupted ? "Running the freshly authorized six-axis path." :
                "Protected motion active. Open W353 while one or more joints are moving.";
            return true;
        }

        private bool ExerciseInterlocksReady() => LinkReady && linked && StoA && StoB && BothStoTested &&
            Live(gateWitness) && Live(resolverWitness) && Live(currentWitness) && Live(brakeWitness);

        private void BeginMotion(Motion kind)
        {
            motion = kind;
            traceAt = kind == Motion.Home ? .32f : FaultLive(1) ? .18f : .85f;
            robot.Begin();
            scope?.ResetTrace();
            Failure = AxisFailure.None;
        }

        private void IsolatePower()
        {
            powered = false;
            using (manager.ElectricalSolver.BeginConfigurationBatch())
            {
                foreach (var contact in new[] { bench, monitorGate, prechargeGate, contactor, stoGateA, stoGateB, phaseGate, heartbeat, workload })
                    SetContact(contact, false);
                for (int axis = 0; axis < 6; axis++)
                {
                    SetContact(driveGates[axis], false);
                    SetContact(encoderGates[axis], false);
                    SetContact(thermalGates[axis], false);
                }
                InvalidateDiagnostics();
            }
            linkChargedAt = -1f;
            message = "Bench isolated. Wait for link discharge and standstill before rework.";
        }

        private void TickController(float dt)
        {
            time += dt;
            bool driveTrial = motion == Motion.Service;
            bool encoderD = powered && (!driveTrial || !FaultLive(1) || robot.Elapsed is < .43f or >= .56f);
            bool temperatureF = powered && (!driveTrial || !FaultLive(2) || robot.Elapsed < .16f ||
                (int)(robot.Elapsed * 10f) % 3 == 0);
            if (encoderGates[3].IsClosed != encoderD || thermalGates[5].IsClosed != temperatureF)
            {
                SetContact(encoderGates[3], encoderD);
                SetContact(thermalGates[5], temperatureF);
                Solve();
            }
            UpdateSupervisor();
            for (int axis = 0; axis < 6; axis++)
            {
                drives[axis] = Live(drivePins[axis]);
                encoders[axis] = Live(encoderPins[axis]);
                thermals[axis] = Live(thermalPins[axis]);
            }
            bool enabled = ExerciseInterlocksReady() && (motion != Motion.Protected || !Inhibited);
            float feed = Voltage(mainRail);
            bool charge = powered && Live(prechargePin), engaged = powered && Live(dcLinkPin), high = Live(phaseB);
            accumulator += dt;
            while (accumulator >= SparkLevel120Robot.Dt)
            {
                accumulator -= SparkLevel120Robot.Dt;
                robot.Advance(feed, charge, engaged, Running, motion == Motion.Home, enabled, high, drives, encoders, thermals);
                if (!Running || robot.Elapsed < traceAt || robot.Elapsed >= (motion == Motion.Home ? 1.12f : 1.65f)) continue;
                var axis = robot.Joints[selectedAxis];
                scope?.Record(.5f + axis.CurrentU / (2f * CurrentScale), .5f + axis.CurrentV / (2f * CurrentScale),
                    .5f + axis.CurrentW / (2f * CurrentScale), .5f + (axis.Target - axis.Position) / (2f * TrackingScale));
                traceAt += .010f;
            }
            if (charge && robot.Bus >= 11.1f)
            {
                if (linkChargedAt < 0f) linkChargedAt = time;
            }
            else linkChargedAt = -1f;
            if (Running && (robot.EncoderTrip || robot.EnvelopeTrip || robot.ProtectionTrip ||
                robot.Elapsed >= (motion == Motion.Home ? SparkLevel120Robot.HomeSeconds : SparkLevel120Robot.PathSeconds)))
                FinishMotion();
            benchState = stage == 6 ? "RAX-28 QUALIFIED" : !powered ? "BENCH ISOLATED" :
                motion == Motion.Home ? "SIX-AXIS HOMING" : motion == Motion.Protected ? "PROTECTED MOTION" :
                motion == Motion.Service ? "ISOLATED COORDINATED PATH" : !engaged ? "SHARED-LINK PRECHARGE" :
                interrupted && !safetyReset ? "U48 RECOVERY LATCH" : "SIX-AXIS BENCH READY";
        }

        private void FinishMotion()
        {
            Motion finished = motion;
            motion = Motion.Idle;
            robot.CutTorque();
            if (finished == Motion.Home)
            {
                homed = robot.Homed;
                if (!homed) { Reject("One or more joints failed home alignment or encoder plausibility."); return; }
                evidence = "Six independent encoder references agree at home.";
                if (stage == 2) Award();
                message = "Home proved. Check the workspace, then run the synchronized path.";
                return;
            }
            if (finished == Motion.Protected)
            {
                armed = false;
                if (interrupted && safetyReset && !Inhibited && Fresh && robot.Qualified && Award())
                {
                    ProtectedToolError = ToolErrorMm;
                    message = "W353 recovery verified. U48 authorized the complete six-axis trajectory.";
                    evidence = $"Protected path · {PeakError:F2}° peak joint error · {ToolErrorMm:F1} mm peak tool error.";
                }
                else if (!interrupted) message = "Start another protected path and open W353 while moving.";
                return;
            }
            runEpoch = epoch;
            Failure = robot.EncoderTrip && EncoderMisses > 0 ? AxisFailure.EncoderD :
                robot.ClippedPhaseSamples > 0 ? AxisFailure.PhaseB :
                robot.ThermalDerateTime > .05f ? AxisFailure.ThermalF :
                robot.EnvelopeTrip ? AxisFailure.Workspace : robot.ProtectionTrip ? AxisFailure.Protection :
                robot.Qualified ? AxisFailure.None : AxisFailure.Tracking;
            evidence = $"Joint {PeakError:F2}° · tool {ToolErrorMm:F1} mm · IK {IkResidualMm:F2} mm\n" +
                       $"Link {MinimumBus:F2} V min · brake {BrakeEnergy:F2} J";
            if (Failure != AxisFailure.None)
            {
                message = Failure switch
                {
                    AxisFailure.PhaseB => "Axis C phase-B current distorted. Probe its high-side contact.",
                    AxisFailure.EncoderD => "Encoder D samples went stale and every joint lost torque. Probe the plated span.",
                    AxisFailure.ThermalF => "Axis F repeatedly derated as temperature feedback went stale. Probe U34.",
                    _ => "The trajectory exceeded a tracking, protection or workspace limit."
                };
                return;
            }
            qualifiedEpoch = epoch;
            if (stage == 3 && Award()) message = "Joint and Cartesian path qualified. Start the fault trials.";
            else if (stage == 4 && activeFault >= 0 && reworked)
            {
                repaired++;
                activeFault = -1;
                faultWitnessed = reworked = false;
                QueueCheckpoint();
                message = repaired == 3 && Award() ?
                    "All repairs qualified. Start a protected path and open W353 while moving." :
                    "Repair requalified. Begin the next fault trial.";
            }
            else if (stage == 4) message = "New motion proof recorded. Begin the next fault trial.";
            else if (stage == 5)
                message = interrupted ? "Fresh path qualified. Wait for U48's heartbeat and reset the latch." :
                    "Fresh path qualified. Wait for U48's permit and start protected motion.";
        }

        public bool BeginFaultTrial()
        {
            if (stage != 4 || activeFault >= 0 || repaired >= 3 || !Qualifies || Running)
                return Reject("Complete a healthy coordinated path before staging a fault.");
            activeFault = repaired;
            faultWitnessed = reworked = false;
            repairTarget = viaInspectionEpoch = -1;
            spare = 0;
            qualifiedEpoch = runEpoch = -1;
            epoch++;
            SetContact(phaseGate, !FaultLive(0));
            Solve();
            SelectAxis(activeFault == 0 ? 2 : activeFault == 1 ? 3 : 5);
            message = "A seeded fault is active. Run the isolated service path to capture its signature.";
            QueueCheckpoint();
            return true;
        }

        public bool ProbeFault()
        {
            if (!powered || runEpoch != epoch || activeFault < 0 || reworked)
                return Reject("Obtain fresh fault evidence first.");
            bool measured = activeFault switch
            {
                0 => Failure == AxisFailure.PhaseB && PhaseClips > 0 && !Live(phaseB) && Live(drivePins[2]),
                1 => Failure == AxisFailure.EncoderD && EncoderMisses > 0 && Live(resolverWitness) && !TorqueOn,
                2 => Failure == AxisFailure.ThermalF && DerateSeconds > .10f && Live(drivePins[5]),
                _ => false
            };
            if (!measured) return Reject("The measured motion signature does not establish the staged fault.");
            faultWitnessed = true;
            footprint = ExpectedPart;
            SelectAxis(activeFault == 0 ? 2 : activeFault == 1 ? 3 : 5);
            evidence = activeFault switch
            {
                0 => $"U18 B-high {Voltage(phaseB):F2} V · {PhaseClips} clipped samples.",
                1 => $"{EncoderVia.viaId}: {EncoderMisses} missing D samples · {EncoderDisagreement:F2}° mismatch.",
                _ => $"U34 feedback stale · {DerateSeconds:F2} s of axis F derating."
            };
            message = $"Isolate power. Select {parts[ExpectedPart].label} on the board and its matching spare.";
            QueueCheckpoint();
            return true;
        }

        public bool InspectEncoderVia()
        {
            if (activeFault != 1 || !faultWitnessed || powered)
                return Reject("Observe encoder D and isolate bench power first.");
            if (selectedRoute != encoderViaIndex || layer != EncoderVia.layer)
            {
                SelectRoute(encoderViaIndex);
                PadView();
                return Reject($"Inspect {EncoderVia.viaId} on entry layer L{EncoderVia.layer:D2}.");
            }
            viaInspectionEpoch = epoch;
            message = $"Measured {EncoderVia.viaId} from L{EncoderVia.layer:D2} to L{EncoderVia.exitLayer:D2}.";
            return true;
        }

        public void SelectSpare()
        {
            spare = (spare + 1) % 4;
            message = spare switch
            {
                1 => "Spare · axis C phase-B isolator",
                2 => "Spare · encoder D plated barrel",
                3 => "Spare · axis F thermal contact",
                _ => "No spare selected."
            };
        }

        public bool Repair()
        {
            if (stage != 4 || activeFault < 0 || !faultWitnessed || reworked || powered || Bus > .6f || Speed > 2f)
                return Reject("Diagnose, isolate and wait for discharge and standstill before rework.");
            if (spare != activeFault + 1 || repairTarget != ExpectedPart ||
                activeFault == 1 && (viaInspectionEpoch != epoch || layer != EncoderVia.layer))
                return Reject("Match the spare to the selected site and inspect the entry span when required.");
            reworked = true;
            InvalidateDiagnostics();
            QueueCheckpoint();
            BoardView();
            message = "Repair seated. Acquire new STO, home, workspace and coordinated-path proof.";
            return true;
        }

        public bool OpenW353()
        {
            if (stage != 5 || !linked || interrupted || !ProtectedRunning || !TorqueOn || Speed < 10f)
                return Reject("Open W353 while the protected path is driving moving joints.");
            if (!RemoveWire(jumper)) return Reject("The native registry could not open the W353 edge.");
            Solve();
            return true;
        }

        public bool RestoreW353()
        {
            if (stage != 5 || !interrupted || linked)
                return Reject("Open W353 during the active challenge before restoring it.");
            if (!CreateWire(jumper)) return Reject("The native graph rejected the W353 training contact pair.");
            ReadGraph();
            message = "W353 restored. The U48 latch still requires new STO, home and full path evidence.";
            return true;
        }

        public bool ResetSafety()
        {
            if (stage != 5 || !interrupted || safetyReset || !Fresh || epoch < minimumEpoch || Running)
                return Reject("U48 requires fresh post-repair safety and trajectory evidence.");
            safetyReset = true;
            UpdateSupervisor();
            message = "U48 reset. Run the complete protected six-axis path.";
            return true;
        }

        private void AbortProtectedMotion()
        {
            TripSpeed = Speed;
            motion = Motion.Idle;
            robot.CutTorque();
            armed = safetyReset = false;
            SetContact(workload, false);
            minimumEpoch = epoch + 1;
            Solve();
        }

        private void InvalidateDiagnostics()
        {
            epoch++;
            qualifiedEpoch = runEpoch = -1;
            stoProofA = stoProofB = homed = workspaceProof = false;
            motion = Motion.Idle;
            robot.CutTorque();
            feedbackIntervals = 0;
            lastRise = -1f;
            highFeedback = false;
            SetContact(workload, false);
        }

        private void UpdateSupervisor()
        {
            if (time >= nextHeartbeat)
            {
                nextHeartbeat = time + .14f;
                driverHeartbeat = !driverHeartbeat && ChassisReady && Qualifies;
                if (heartbeat.IsClosed != driverHeartbeat)
                {
                    SetContact(heartbeat, driverHeartbeat);
                    Solve();
                }
                bool high = linked && Live(jumper.to);
                if (high && !highFeedback)
                {
                    if (lastRise >= 0 && time - lastRise is > .21f and < .49f) feedbackIntervals++;
                    else feedbackIntervals = 0;
                    lastRise = time;
                }
                highFeedback = high;
            }
            bool permit = stage >= 5 && Fresh && (!interrupted || safetyReset);
            if (workload.IsClosed == permit) return;
            SetContact(workload, permit);
            if (!permit && ProtectedRunning) robot.CutTorque();
            Solve();
        }
    }
}
