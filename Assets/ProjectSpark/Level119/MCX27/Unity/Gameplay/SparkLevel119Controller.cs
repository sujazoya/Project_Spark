using UnityEngine;

namespace ProjectSpark.Gameplay.MotionCitadel
{
    public sealed partial class SparkLevel119Process
    {
        public SparkTerminal mainRail, monitorSupply, prechargePin, dcLinkPin, stoA, stoB, phaseU, phaseV, phaseW;
        public SparkTerminal gateWitness, encoderPin, resolverWitness, brakePin, currentWitness, protectedFeed;
        public SparkSwitch bench, monitorGate, prechargeGate, contactor, stoGateA, stoGateB, gateU, gateV, gateW;
        public SparkSwitch encoderGate, brakeGate, heartbeat, workload;
        public GameObject assemblyArt;
        public int gatePart, encoderPart, brakePart, encoderViaIndex;

        public enum MotorFailure { None, PhaseV, EncoderVia, BrakeSense, Performance }
        private enum Motion { None, Service, Protected }
        private readonly SparkLevel119Motor motor = new();
        private Motion motion;
        private float time, accumulator, nextTrace, nextHeartbeat, lastRise = -1f, linkChargedAt = -1f;
        private bool driverHeartbeat, highFeedback, stoProofA, stoProofB, aligned;
        private bool faultWitnessed, reworked, armed, interrupted, safetyReset;
        private int epoch, qualifiedEpoch = -1, runEpoch = -1, minimumEpoch, feedbackIntervals;
        private int activeFault = -1, repaired, repairTarget = -1, viaInspectionEpoch = -1, spare;
        private string evidence = "", benchState = "BENCH ISOLATED";

        public bool ChassisReady => powered && Live(mainRail) && Live(monitorSupply);
        public bool LinkReady => ChassisReady && Live(dcLinkPin) && motor.Bus > 10.85f;
        public bool StoA => Live(stoA);
        public bool StoB => Live(stoB);
        public bool Alignment => aligned;
        public bool BothStoTested => stoProofA && stoProofB;
        public bool Qualifies => qualifiedEpoch == epoch && LinkReady && StoA && StoB && BothStoTested && aligned &&
            Live(phaseV) && Live(encoderPin) && Live(brakePin);
        public bool Fresh => linked && Qualifies && feedbackIntervals >= 2 && time - lastRise < .49f;
        public bool Running => motion != Motion.None;
        public bool ProtectedRunning => motion == Motion.Protected;
        public bool Pwm => motor.Pwm;
        public bool Tripped => interrupted;
        public int Repaired => repaired;
        public int Epoch => epoch;
        public float Bus => motor.Bus;
        public float RotorRpm => motor.Speed * 9.549296f;
        public float Torque => motor.Torque;
        public float Energy => motor.BrakeEnergy;
        public float RegenEnergy => motor.RegenEnergy;
        public float PeakBus => motor.PeakBus;
        public float CruiseRpm => motor.CruiseSpeed * 9.549296f;
        public float CruiseTorque => motor.CruiseTorque;
        public float PhaseSpread => motor.CruiseRmsSpread;
        public float PeakCurrent => motor.PeakCurrent;
        public int ReducedPhaseSteps => motor.PhaseVClipped;
        public int EncoderDropouts => motor.EncoderOutages;
        public float EncoderError => motor.EncoderError;
        public float BrakeTemperature => motor.BrakeTemp;
        public float TripSpeedRpm { get; private set; }
        public MotorFailure Failure { get; private set; }
        public string Evidence => evidence;

        private Route EncoderVia => routes[encoderViaIndex];
        private int ExpectedPart => activeFault switch { 0 => gatePart, 1 => encoderPart, 2 => brakePart, _ => -1 };
        private bool FaultLive(int index) => activeFault == index && !reworked;
        private bool Live(SparkTerminal pin) => Voltage(pin) > 10.8f;
        private void Solve() => manager.ElectricalSolver.SolveNow();

        public bool TogglePower()
        {
            if (stage < 2 || stage >= 6 || !Construction || !linked && !interrupted)
                return Reject("Finish native copper continuity before enabling the MCX-27 bench.");
            if (powered)
            {
                IsolatePower();
                return true;
            }
            if (motor.Overvoltage && motor.Bus >= .6f) return Reject("Discharge the virtual link below 0.6 V before re-enabling.");
            motor.ClearDischargedLatch();
            powered = true;
            SetContact(bench, true);
            SetContact(monitorGate, true);
            SetContact(stoGateA, true);
            SetContact(stoGateB, true);
            SetContact(encoderGate, true);
            SetContact(brakeGate, !FaultLive(2));
            Solve();
            message = "Start precharge. Close the contactor only after the link is charged.";
            return true;
        }

        public bool BeginPrecharge()
        {
            if (!ChassisReady || contactor.IsClosed || Running)
                return Reject("Enable bench power before precharge.");
            SetContact(prechargeGate, true);
            Solve();
            linkChargedAt = -1f;
            message = "Precharging the virtual DC link. Wait for at least 11.1 V and the stability check.";
            return true;
        }

        public bool CloseContactor()
        {
            if (!ChassisReady || !Live(prechargePin) || linkChargedAt < 0f || time - linkChargedAt < .16f)
                return Reject("Precharge has not settled at 11.1 V.");
            SetContact(contactor, true);
            SetContact(prechargeGate, false);
            SetContact(gateU, true);
            SetContact(gateV, !FaultLive(0));
            SetContact(gateW, true);
            Solve();
            message = "Prove STO A and B one at a time, then align the encoder.";
            return true;
        }

        public bool ProveSto(int channel)
        {
            if (!LinkReady || Running || channel is < 0 or > 1 || !StoA || !StoB)
                return Reject("Close the charged contactor and stop motion before a two-channel test.");
            var contact = channel == 0 ? stoGateA : stoGateB;
            SetContact(workload, false);
            SetContact(contact, false);
            Solve();
            bool independent = (channel == 0 ? !StoA && StoB : StoA && !StoB) && Live(monitorSupply);
            motor.CutPwm();
            SetContact(contact, true);
            Solve();
            if (!independent || !StoA || !StoB) return Reject("STO independence or restore measurement failed.");
            if (channel == 0) stoProofA = true;
            else stoProofB = true;
            evidence = $"STO {(channel == 0 ? "A" : "B")} opened alone; the other input and U40 stayed live.";
            message = BothStoTested ? "Both channels independently proved. Align encoder." : "Prove the other STO channel.";
            return true;
        }

        public bool AlignEncoder()
        {
            if (!LinkReady || Running || !BothStoTested || !StoA || !StoB || !Live(encoderPin) ||
                !Live(resolverWitness) || Mathf.Abs(motor.Speed) > 2f)
                return Reject("With both STO channels proved, wait for standstill and verify both position paths.");
            aligned = true;
            evidence = "Primary encoder escape and independent resolver agree at the stationary reference.";
            if (stage == 2 && !Award()) return false;
            message = "Run the bounded service speed and regeneration profile.";
            return true;
        }

        public void SelectTuning()
        {
            if (Running || armed || stage >= 6) return;
            motor.DampedTuning = !motor.DampedTuning;
            qualifiedEpoch = -1;
            message = motor.DampedTuning ? "Damped speed-loop preset selected. Run a fresh profile." :
                "Balanced speed-loop preset selected. Run a fresh profile.";
        }

        public bool RunProfile()
        {
            if (stage is < 3 or > 5 || !TestReady() || !linked || Running || motor.Overvoltage)
                return Reject("Commission both channels and align the encoder before running the service profile.");
            BeginMotion(Motion.Service);
            return true;
        }

        public bool StartProtectedMotion()
        {
            if (stage != 5 || Running || !Fresh || Inhibited || interrupted && !safetyReset)
                return Reject("U40 needs fresh qualified motion evidence and an active permit.");
            armed = true;
            BeginMotion(Motion.Protected);
            return true;
        }

        private bool TestReady() => LinkReady && BothStoTested && StoA && StoB && aligned && Live(gateWitness) &&
            Live(currentWitness) && Live(resolverWitness) && Live(phaseU) && Live(phaseW) && Mathf.Abs(motor.Speed) < 5f;

        private void BeginMotion(Motion kind)
        {
            motion = kind;
            nextTrace = 1.42f;
            motor.Begin();
            scope?.ResetTrace();
            Failure = MotorFailure.None;
            message = kind == Motion.Service ? "Service profile · accelerate, hold speed, then regenerate into the brake." :
                interrupted ? "Protected recovery profile running on fresh motion evidence." :
                "Protected motion running. Open W337 once the rotor is above 150 rpm.";
        }

        private void IsolatePower()
        {
            powered = false;
            motion = Motion.None;
            motor.CutPwm();
            foreach (var contact in new[] { bench, monitorGate, prechargeGate, contactor, stoGateA, stoGateB,
                gateU, gateV, gateW, encoderGate, brakeGate, heartbeat, workload })
                SetContact(contact, false);
            linkChargedAt = -1f;
            InvalidateDiagnostics();
            Solve();
            message = "Bench isolated. Let the DC link discharge and the rotor stop before touching a fault site.";
        }

        private void TickController(float dt)
        {
            time += dt;
            bool encoderExpected = powered && (!FaultLive(1) || !Running || ((int)(motor.Elapsed * 18f) % 3) != 1);
            if (encoderGate.IsClosed != encoderExpected)
            {
                SetContact(encoderGate, encoderExpected);
                Solve();
            }
            UpdateSupervisor();
            bool active = Running;
            bool enable = powered && linked && StoA && StoB && Live(phaseU) && Live(phaseW) &&
                (motion != Motion.Protected || !Inhibited);
            bool encoder = Live(encoderPin), phaseVValid = Live(phaseV), brake = Live(brakePin);
            float supply = Voltage(mainRail);
            bool charge = powered && Live(prechargePin), engaged = powered && Live(dcLinkPin);
            accumulator += dt;
            while (accumulator >= SparkLevel119Motor.Step)
            {
                motor.Advance(supply, charge, engaged, active, enable, phaseVValid, encoder, brake);
                accumulator -= SparkLevel119Motor.Step;
                // Keep the four traces on the same short, high-rate cruise window.
                if (!active || motor.Elapsed < nextTrace || motor.Elapsed >= 1.74f) continue;
                scope?.Record(.5f + motor.CurrentU / 9f, .5f + motor.CurrentV / 9f,
                    .5f + motor.CurrentW / 9f, .5f + motor.QCurrent / 9f);
                nextTrace += .004f;
            }
            if (charge && motor.Bus >= 11.1f)
            {
                if (linkChargedAt < 0f) linkChargedAt = time;
            }
            else linkChargedAt = -1f;

            if (active && (motor.Overvoltage || motor.Elapsed >= SparkLevel119Motor.RunLength)) FinishMotion();

            benchState = stage == 6 ? "MCX-27 QUALIFIED" : !powered ? "BENCH ISOLATED" :
                motor.Overvoltage ? "OVERVOLTAGE LATCHED" : motion == Motion.Protected ? "PROTECTED MOTION" :
                motion == Motion.Service ? "ISOLATED SERVICE PROFILE" : !engaged ? "DC-LINK PRECHARGE" :
                interrupted && !safetyReset ? "U40 RECOVERY LATCH" : "SERVO BENCH READY";
        }

        private void FinishMotion()
        {
            Motion finished = motion;
            motion = Motion.None;
            motor.CutPwm();
            if (finished == Motion.Protected)
            {
                if (interrupted && safetyReset && !Inhibited && Fresh && motor.Qualified && Award())
                {
                    message = "W337 recovery verified. MCX-27 completed the protected speed and braking profile.";
                    evidence = $"U40 permit live · fresh protected brake energy {Energy:F2} J.";
                }
                else if (!interrupted) message = "Protected profile ended. Start another run and open W337 above 150 rpm.";
                return;
            }
            runEpoch = epoch;
            Failure = motor.Overvoltage && !Live(brakePin) ? MotorFailure.BrakeSense :
                motor.EncoderOutages > 0 ? MotorFailure.EncoderVia : motor.PhaseVClipped > 0 ? MotorFailure.PhaseV :
                motor.Qualified ? MotorFailure.None : MotorFailure.Performance;
            evidence = $"Cruise {CruiseRpm:F0} rpm · {CruiseTorque:F2} N·m · peak {PeakCurrent:F2} A\n" +
                       $"Link {PeakBus:F2} V · regen {RegenEnergy:F2} J · brake {Energy:F2} J";
            if (Failure != MotorFailure.None)
            {
                qualifiedEpoch = -1;
                message = Failure switch
                {
                    MotorFailure.PhaseV => "Phase V high-side duty clipped. Compare the phase waveforms and probe the fault.",
                    MotorFailure.EncoderVia => "Encoder samples dropped while resolver continuity stayed valid. Probe the fault.",
                    MotorFailure.BrakeSense => "Brake feedback under-read and the DC link tripped. Probe the brake path.",
                    _ => "Profile outside the speed, current, braking or thermal limits. Review the measured evidence."
                };
                return;
            }
            qualifiedEpoch = epoch;
            if (stage == 3 && Award()) message = "Speed, torque and regeneration qualified. Begin a three-fault trial.";
            else if (stage == 4 && activeFault >= 0 && reworked)
            {
                repaired++;
                activeFault = -1;
                faultWitnessed = reworked = false;
                QueueCheckpoint();
                if (repaired == 3 && Award())
                    message = "Three repairs qualified. Start protected motion and deliberately open W337.";
                else message = "Repair requalified with fresh evidence. Begin the next fault trial.";
            }
            else if (stage == 5)
                message = interrupted ? "Fresh service profile passed. Wait for U40 heartbeat, then reset the latch." :
                    "Fresh motion evidence established. Wait for U40 permission, then start protected motion.";
        }

        public bool BeginFaultTrial()
        {
            if (stage != 4 || activeFault >= 0 || repaired >= 3 || !Qualifies || Running)
                return Reject("Complete a fresh successful profile before staging the next fault.");
            activeFault = repaired;
            faultWitnessed = reworked = false;
            repairTarget = viaInspectionEpoch = -1;
            spare = 0;
            qualifiedEpoch = runEpoch = -1;
            epoch++;
            SetContact(gateV, !FaultLive(0));
            SetContact(brakeGate, !FaultLive(2));
            Solve();
            message = "A seeded drive fault is active. Run the service profile and compare the measurements.";
            QueueCheckpoint();
            return true;
        }

        public bool ProbeFault()
        {
            if (!powered || runEpoch != epoch || activeFault < 0 || reworked) return Reject("Obtain fresh fault evidence first.");
            bool measured = activeFault switch
            {
                0 => Failure == MotorFailure.PhaseV && ReducedPhaseSteps > 0 && !Live(phaseV) && Live(gateWitness),
                1 => Failure == MotorFailure.EncoderVia && EncoderDropouts > 0 && Live(resolverWitness),
                2 => Failure == MotorFailure.BrakeSense && !Live(brakePin) && PeakBus >= 14.6f && Energy < .2f,
                _ => false
            };
            if (!measured) return Reject("The fault signature has not been observed.");
            faultWitnessed = true;
            footprint = ExpectedPart;
            var part = parts[ExpectedPart];
            evidence = activeFault switch
            {
                0 => $"U12 phase-V high side reads {Voltage(phaseV):F2} V · {ReducedPhaseSteps} clipped PWM samples.",
                1 => $"{EncoderVia.viaId} missed {EncoderDropouts} samples · peak position disagreement {EncoderError:F1}°.",
                _ => $"U14 brake sensor {Voltage(brakePin):F2} V · link peak {PeakBus:F2} V · brake {Energy:F2} J."
            };
            message = $"Isolate the bench. Select {part.label}, then the matching repair kit.";
            QueueCheckpoint();
            return true;
        }

        public bool InspectEncoderVia()
        {
            if (activeFault != 1 || !faultWitnessed || powered)
                return Reject("Probe the encoder fault and isolate the bench first.");
            if (selectedRoute != encoderViaIndex || layer != EncoderVia.layer)
            {
                SelectRoute(encoderViaIndex);
                PadView();
                message = $"Inspect {EncoderVia.viaId} at the entry layer L{EncoderVia.layer:D2}.";
                return false;
            }
            viaInspectionEpoch = epoch;
            message = $"Verified {EncoderVia.viaId} from L{EncoderVia.layer:D2} to L{EncoderVia.exitLayer:D2}.";
            return true;
        }

        public void SelectSpare()
        {
            spare = (spare + 1) % 4;
            message = spare switch
            {
                1 => "Repair kit · phase V gate isolator",
                2 => "Repair kit · encoder via barrel",
                3 => "Repair kit · brake thermal sensor",
                _ => "No repair kit selected."
            };
        }

        public bool Repair()
        {
            if (stage != 4 || activeFault < 0 || !faultWitnessed || reworked || powered ||
                motor.Bus > .6f || Mathf.Abs(motor.Speed) > 2f)
                return Reject("Diagnose the fault, isolate the bench and wait for discharge and standstill.");
            if (spare != activeFault + 1 || repairTarget != ExpectedPart ||
                activeFault == 1 && (viaInspectionEpoch != epoch || layer != EncoderVia.layer))
                return Reject("Match the spare to the selected board site and inspect the marked entry layer if needed.");
            reworked = true;
            InvalidateDiagnostics();
            QueueCheckpoint();
            message = "Repair installed. Precharge, prove both STO channels and rerun the full profile.";
            BoardView();
            return true;
        }

        public bool OpenW337()
        {
            if (stage != 5 || !linked || interrupted || !ProtectedRunning || !motor.Pwm || RotorRpm < 150f)
                return Reject("Start protected motion and open W337 above 150 rpm.");
            if (!RemoveWire(jumper)) return Reject("The native wire registry could not open W337.");
            Solve();
            return true;
        }

        public bool RestoreW337()
        {
            if (stage != 5 || !interrupted || linked) return Reject("Open W337 during protected motion before reworking it.");
            if (!CreateWire(jumper)) return Reject("The native circuit rejected the W337 pad pair.");
            ReadGraph();
            message = "W337 restored. The latch stays closed to torque until fresh STO, alignment and service evidence are collected.";
            return true;
        }

        public bool ResetSafety()
        {
            if (stage != 5 || !interrupted || safetyReset || !Fresh || epoch < minimumEpoch || Running)
                return Reject("U40 requires fresh post-repair STO, encoder and service evidence before reset.");
            safetyReset = true;
            UpdateSupervisor();
            message = "U40 reset. Run the protected motion and braking profile once more.";
            return true;
        }

        private void AbortProtectedMotion()
        {
            TripSpeedRpm = RotorRpm;
            motion = Motion.None;
            motor.CutPwm();
            armed = safetyReset = false;
            SetContact(workload, false);
            minimumEpoch = epoch + 1;
            Solve();
        }

        private void InvalidateDiagnostics()
        {
            epoch++;
            qualifiedEpoch = runEpoch = -1;
            stoProofA = stoProofB = aligned = false;
            feedbackIntervals = 0;
            lastRise = -1f;
            highFeedback = false;
            SetContact(workload, false);
        }

        private void UpdateSupervisor()
        {
            // U40 consumes a solved W337 input and sees both STO channels separately.
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
            if (!permit && motion == Motion.Protected) motor.CutPwm();
            Solve();
        }
    }
}
