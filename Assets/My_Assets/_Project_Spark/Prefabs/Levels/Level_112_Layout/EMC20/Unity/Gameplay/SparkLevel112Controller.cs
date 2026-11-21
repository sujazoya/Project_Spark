using System;
using UnityEngine;

namespace ProjectSpark.Gameplay.EmcGauntlet
{
    public sealed partial class SparkLevel112Process
    {
        [Serializable]
        public sealed class Lane
        {
            public SparkSwitch[] drive = new SparkSwitch[2];
            public SparkTerminal[] sense = new SparkTerminal[2];
        }

        public GameObject[] shields;
        public Renderer[] shieldSurfaces;
        public Lane[] lanes;
        public SparkTerminal standby, aux, clockP, clockN, fixtureSense, shieldSense, chokeSense, returnSense, ditherSense;
        public SparkTerminal monitorSupply, protectedFeed;
        public SparkSwitch bench, auxGate, clockDriveP, clockDriveN, fixtureGate, shieldBond, chokeShunt, ditherGate, heartbeat, workload;
        public SparkResistor stimulus, shieldExcitation;

        private readonly SparkLevel112Emissions.Band[] baseline = new SparkLevel112Emissions.Band[SparkLevel112Emissions.BandCount];
        private readonly SparkLevel112Emissions.Band[] spectrum = new SparkLevel112Emissions.Band[SparkLevel112Emissions.BandCount];
        private readonly SparkLevel112Packets packets = new();
        private readonly float[] eyes = new float[4], sampledEye = new float[4];
        private float time, powerAt, testAt, heartbeatAt, lastRise = -1f, fixtureGain, clockPositive, clockNegative;
        private float peakConducted, peakRadiated, insertionLoss, measuredBond, measuredChoke, measuredCurrent;
        private int shieldMask, epoch, calibrationEpoch = -1, baselineEpoch = -1, spectrumEpoch = -1, packetEpoch = -1;
        private int requiredEpoch, captures, activeFault = -1, repaired, repairTarget = -1, feedbackIntervals;
        private bool testing, stressCaptured, measuredReturnOpen, armed, interrupted, safetyReset;
        private bool faultWitnessed, reworked, replacementSeam, highFeedback, heartbeatDrive;
        private string evidence = "", benchState = "BENCH ISOLATED";

        public bool ShieldsMounted => shieldMask == 7;
        public bool RailsReady => powered && Voltage(standby) > 4.7f && Voltage(aux) > 3.1f;
        public bool Calibrated => RailsReady && calibrationEpoch == epoch && fixtureGain is >= .96f and <= 1.04f;
        public bool BaselineReady => Calibrated && baselineEpoch == epoch;
        public bool Testing => testing;
        public bool FaultWitnessed => faultWitnessed;
        public bool Qualifies => BaselineReady && spectrumEpoch == epoch && packetEpoch == epoch && packets.Errors == 0 &&
            ShieldsMounted && !measuredReturnOpen && measuredBond < .060f && measuredChoke > 2.85f &&
            insertionLoss >= 14f && Array.TrueForAll(spectrum, band => band.InsideLimits) && Array.TrueForAll(eyes, eye => eye >= 320f);
        public bool Fresh => linked && Qualifies && feedbackIntervals >= 2 && time - lastRise < .53f;
        public int Captures => captures;
        public int Epoch => epoch;
        public int Repaired => repaired;
        public int Packets => packets.Packets;
        public int PayloadBytes => packets.Bytes;
        public int Errors => packets.Errors;
        public int FirstErrorLane => packets.FirstLane;
        public float PeakConducted => peakConducted;
        public float PeakRadiated => peakRadiated;
        public float MeasuredBond => measuredBond;
        public float MeasuredChoke => measuredChoke;
        public float MeasuredCurrent => measuredCurrent;
        public float InsertionLoss => insertionLoss;
        public float MinimumEye => Mathf.Min(Mathf.Min(eyes[0], eyes[1]), Mathf.Min(eyes[2], eyes[3]));
        public bool ReturnOpen => measuredReturnOpen;
        public bool SpreadSpectrum => Voltage(ditherSense) > 2.7f;
        public float BandConducted(int band) => spectrum[band].ConductedDb;
        public string Evidence => evidence;

        private void Solve() => manager.ElectricalSolver.SolveNow();
        private bool FaultLive(int fault) => activeFault == fault && !reworked;
        private int ExpectedPart => activeFault == 0 ? -2 : activeFault == 1 ? chokePart : -1;

        public bool ToggleShield(int index)
        {
            if (index < 0 || index >= shields.Length || installed != PartCount || powered)
                return Reject("Complete the component placements and isolate bench power before handling shields.");
            bool mounting = (shieldMask & (1 << index)) == 0;
            SetShields(shieldMask ^ (1 << index));
            if (mounting && index == 0 && FaultLive(0) && replacementSeam && faultWitnessed)
            {
                reworked = true;
                ConfigureFaultDevices();
                message = "Can 1 remounted and the native seam bond restored. Calibrate and repeat the spectrum and packet tests.";
            }
            else message = mounting ? $"Shield can {index + 1} is seated." : $"Shield can {index + 1} removed with power isolated.";
            InvalidateDiagnostics();
            Solve();
            QueueCheckpoint();
            return true;
        }

        public void InspectShield(int index)
        {
            if (index < 0 || index >= shields.Length) return;
            if (stage == 4 && faultWitnessed) repairTarget = index == 0 ? -2 : -3;
            FocusTarget(shields[index].transform.position);
            message = $"Shield can {index + 1} · {((shieldMask & 1 << index) != 0 ? "mounted" : "removed")}.";
            if (index == 0 && measuredBond > 0) message += $" Last measured bond {measuredBond * 1000f:F1} mΩ.";
        }

        private void SetShields(int mask)
        {
            shieldMask = mask;
            for (int i = 0; i < shields.Length; i++) shields[i].SetActive((mask & (1 << i)) != 0);
            SetContact(shieldBond, ShieldsMounted && !FaultLive(0));
        }

        public bool TogglePower()
        {
            if (powered)
            {
                IsolatePower();
                message = "Bench isolated. Use the diagnosed target for any repair.";
                return true;
            }
            if (stage < 2 || !TrialTopology || !linked && !interrupted)
                return Reject("Complete the shield mounts, routing and W209 before energizing the bench.");
            powered = true;
            InvalidateDiagnostics();
            powerAt = time;
            SetContact(bench, true);
            Solve();
            benchState = "POWER SEQUENCE";
            message = "Waiting for the 3.3 V auxiliary rail. Calibrate the two reference states and the fixture probe.";
            QueueCheckpoint();
            return true;
        }

        private void IsolatePower()
        {
            powered = false;
            SetContact(bench, false);
            SetContact(auxGate, false);
            SetContact(fixtureGate, false);
            SetContact(clockDriveP, false);
            SetContact(clockDriveN, false);
            InvalidateDiagnostics();
            Solve();
            benchState = "BENCH ISOLATED";
            QueueCheckpoint();
        }

        private void InvalidateDiagnostics()
        {
            testing = stressCaptured = false;
            calibrationEpoch = baselineEpoch = spectrumEpoch = packetEpoch = -1;
            stimulus.SetResistance(20f);
            SetContact(chokeShunt, false);
            ClearHeartbeat();
        }

        public bool CalibrateFixture()
        {
            if (!RailsReady || testing) return Reject("Calibrate on a stable powered bench with the stress test paused.");
            InvalidateDiagnostics();
            epoch++;
            SetContact(clockDriveP, true);
            SetContact(clockDriveN, false);
            SetContact(fixtureGate, false);
            Solve();
            clockPositive = Voltage(clockP) - Voltage(clockN);
            float floor = Voltage(fixtureSense);
            SetContact(clockDriveP, false);
            SetContact(clockDriveN, true);
            SetContact(fixtureGate, true);
            Solve();
            clockNegative = Voltage(clockP) - Voltage(clockN);
            fixtureGain = (Voltage(fixtureSense) - floor) / 1.65f;
            if (clockPositive < 3f || clockNegative > -3f || floor > .10f || fixtureGain is < .96f or > 1.04f ||
                Mathf.Abs(stimulus.Current) < .16f)
                return Reject("One reference or stimulus witness is outside the fixture window. Check the power and clock paths.");
            calibrationEpoch = epoch;
            evidence = $"REFCLK {clockPositive:+0.00;-0.00}/{clockNegative:+0.00;-0.00} V · fixture gain {fixtureGain:F3}\n50 Ω port · 0.25 m probe · ±0.8 dB window";
            benchState = "FIXTURE CALIBRATED";
            message = "Capture the idle conducted baseline and radiated proxy.";
            return true;
        }

        public bool CaptureBaseline()
        {
            if (!Calibrated || testing) return Reject("Calibrate the fixture before capturing the idle emissions baseline.");
            float current = Mathf.Abs(stimulus.Current);
            if (current is < .17f or > .38f)
                return Reject("Capture the baseline at the idle stimulus current.");
            SparkLevel112Emissions.Capture(baseline, current, fixtureGain, Voltage(chokeSense),
                ActiveBondOhms(), Voltage(returnSense) > 1f, SpreadSpectrum);
            baselineEpoch = epoch;
            plotted = baseline;
            nearField = false;
            evidence = $"Idle 24 MHz · CM {baseline[4].CommonMicroamps:F2} µA · DM {baseline[4].DifferentialMicroamps:F2} µA\n" +
                $"{baseline[4].ConductedDb:F1} dBµV port · {baseline[4].RadiatedDb:F1} dBµV/m proxy";
            benchState = "IDLE BASELINE CAPTURED";
            message = "Baseline sampled at all eight detector bands. Run the stress, suppression and packet pass.";
            return true;
        }

        public bool ToggleSpread()
        {
            if (!RailsReady || testing) return Reject("Pause the test and power the board before changing the spread setting.");
            bool enable = !SpreadSpectrum;
            SetContact(ditherGate, enable);
            InvalidateDiagnostics();
            Solve();
            message = $"Spread spectrum {(enable ? "enabled" : "disabled")}. Recalibrate and measure in this new configuration.";
            return true;
        }

        public bool RunSuppression()
        {
            if (!BaselineReady || testing) return Reject("Capture a fresh idle baseline before stressing the board.");
            testing = true;
            stressCaptured = false;
            spectrumEpoch = packetEpoch = -1;
            packets.Begin();
            for (int i = 0; i < eyes.Length; i++) eyes[i] = float.MaxValue;
            ClearHeartbeat();
            stimulus.SetResistance(5.6f);
            Solve();
            testAt = time + .20f;
            benchState = "LIVE · SUPPRESSION TEST";
            message = "Measuring the loaded fixture, then comparing packets in all four lanes.";
            return true;
        }

        private float ActiveBondOhms()
        {
            float amps = Mathf.Abs(shieldExcitation.Current);
            return amps > .0001f ? Mathf.Abs(Voltage(shieldSense)) / amps : 1f;
        }

        private void TickController(float elapsed)
        {
            time += elapsed;
            if (!powered) return;
            if (!auxGate.IsClosed && time - powerAt >= .16f && Voltage(standby) > 4.7f)
            {
                SetContact(auxGate, true);
                Solve();
                benchState = "FIXTURE CHECK PENDING";
            }
            if (testing && time >= testAt) TickSuppression();
            if (stage == 2 && BaselineReady && Award())
                message = "Baseline registered. Verify suppression, return continuity and an independent packet pass under load.";
            if (stage == 3 && Qualifies && Award())
                message = "Suppression and packet eye qualified. Start the first controlled fault trial.";
            if (stage == 4 && reworked && Qualifies)
            {
                repaired++;
                activeFault = repairTarget = -1;
                faultWitnessed = reworked = replacementSeam = false;
                QueueCheckpoint();
                if (repaired == 3)
                {
                    if (Award()) message = "Three repairs verified. Wait for W209’s trusted cadence and open the jumper.";
                }
                else message = "Repair verified through fresh measurements. Start the next fault trial.";
            }
            TickSupervisor();
        }

        private void TickSuppression()
        {
            testAt = time + .090f;
            if (!stressCaptured)
            {
                measuredCurrent = Mathf.Abs(stimulus.Current);
                if (measuredCurrent < .65f || !RailsReady)
                {
                    testing = false;
                    stimulus.SetResistance(20f);
                    Solve();
                    message = "Stress current is outside the fixture range. Check powered continuity.";
                    return;
                }
                SetContact(chokeShunt, FaultLive(1) && measuredCurrent > .55f);
                Solve();
                measuredBond = ActiveBondOhms();
                measuredChoke = Voltage(chokeSense);
                measuredReturnOpen = Voltage(returnSense) > 1f;
                SparkLevel112Emissions.Capture(spectrum, measuredCurrent, fixtureGain, measuredChoke,
                    measuredBond, measuredReturnOpen, SpreadSpectrum);
                peakConducted = peakRadiated = 0f;
                foreach (var band in spectrum)
                {
                    peakConducted = Mathf.Max(peakConducted, band.ConductedDb);
                    peakRadiated = Mathf.Max(peakRadiated, band.RadiatedDb);
                }
                insertionLoss = spectrum[4].InsertionLossDb;
                spectrumEpoch = epoch;
                plotted = spectrum;
                nearField = false;
                stressCaptured = true;
                return;
            }
            if (!RailsReady)
            {
                InvalidateDiagnostics();
                Solve();
                message = "The bench lost power before the packet comparison completed.";
                return;
            }
            int polarity = packets.Bursts % 2;
            foreach (var lane in lanes)
            {
                SetContact(lane.drive[polarity], true);
                SetContact(lane.drive[1 - polarity], false);
            }
            Solve();
            for (int i = 0; i < lanes.Length; i++)
            {
                float swing = Voltage(lanes[i].sense[polarity]) - Voltage(lanes[i].sense[1 - polarity]);
                sampledEye[i] = Mathf.Max(0f, swing * 198f - Mathf.Max(0f, peakConducted - 30f) * 2.3f -
                    (measuredReturnOpen ? 365f : 0f) - i * 7f);
                eyes[i] = Mathf.Min(eyes[i], sampledEye[i]);
            }
            packets.CompareBurst(sampledEye);
            if (packets.Bursts < SparkLevel112Packets.BurstCount) return;

            testing = false;
            stimulus.SetResistance(20f);
            SetContact(chokeShunt, false);
            Solve();
            packetEpoch = epoch;
            captures++;
            evidence = $"Run {captures} · {peakConducted:F1} dBµV port · {peakRadiated:F1} dBµV/m proxy\n" +
                $"24 MHz loss {insertionLoss:F1} dB · {packets.Errors}/{packets.Packets} packet errors";
            benchState = Qualifies ? "EMC TRAINING LIMITS MET" : "SUPPRESSION OUT OF SPEC";
            message = Qualifies ? "Suppression, shield bond, return continuity and packet eye passed."
                : "Compare the out-of-limit bands, filter loss, shield bond and return eye to diagnose the fault.";
        }

        public bool BeginFaultTrial()
        {
            if (stage != 4 || activeFault >= 0 || repaired >= 3 || !Qualifies)
                return Reject("Verify suppression and data before starting the next fault trial.");
            activeFault = repaired;
            repairTarget = -1;
            faultWitnessed = reworked = replacementSeam = false;
            InvalidateDiagnostics();
            ConfigureFaultDevices();
            ReadGraph();
            Solve();
            benchState = "CONTROLLED FAULT TRIAL";
            message = "Recalibrate, capture idle and repeat the full stress and packet test to diagnose the failure.";
            QueueCheckpoint();
            return true;
        }

        private void ConfigureFaultDevices()
        {
            SetContact(shieldBond, ShieldsMounted && !FaultLive(0));
            SetContact(chokeShunt, false);
            if (activeFault == 2) SetContact(routes[returnRoute].barrel, !FaultLive(2));
        }

        public bool LocateFault()
        {
            bool diagnosed = powered && spectrumEpoch == epoch && packetEpoch == epoch && (activeFault switch
            {
                0 => measuredBond > .25f && peakRadiated + SparkLevel112Emissions.DetectorUncertainty >= SparkLevel112Emissions.RadiatedLimit,
                1 => measuredCurrent > .65f && measuredChoke < 1.4f && insertionLoss < 14f,
                2 => measuredReturnOpen && packets.Errors > 0 && MinimumEye < 320f,
                _ => false
            });
            if (!diagnosed || reworked) return Reject("Capture the failing loaded measurement before choosing the repair.");
            faultWitnessed = true;
            repairTarget = ExpectedPart;
            if (activeFault == 0)
            {
                InspectShield(0);
                nearField = true;
                message = $"Can 1 shows {measuredBond * 1000f:F0} mΩ and excess radiation. Isolate, remove and rebond the seam.";
            }
            else if (activeFault == 1)
            {
                footprint = chokePart;
                FocusPart();
                message = $"L1 loses attenuation at {measuredCurrent:F2} A. Isolate and replace the measured choke.";
            }
            else
            {
                SelectRoute(returnRoute);
                PadView();
                message = "V44 opens the reference plane and collapses the packet eye. Isolate and replate on L06.";
            }
            QueueCheckpoint();
            return true;
        }

        public bool Repair()
        {
            if (stage != 4 || powered || !faultWitnessed || reworked || repairTarget != ExpectedPart ||
                activeFault == 2 && (Selected != routes[returnRoute] || layer != Selected.layer))
                return Reject("Isolate the bench and select the diagnosed target on its authorized layer.");
            if (activeFault == 0)
            {
                if ((shieldMask & 1) != 0) return Reject("Remove shield can 1 before reworking its seam.");
                replacementSeam = true;
                message = "Seam rebonded. Refit shield can 1 to close its native contact.";
            }
            else
            {
                reworked = true;
                ConfigureFaultDevices();
                ReadGraph();
                Solve();
                BoardView();
                message = "Native path restored. Capture a fresh idle baseline and loaded qualification.";
            }
            QueueCheckpoint();
            return true;
        }

        public bool OpenW209()
        {
            if (stage != 5 || !armed || !Fresh || interrupted || !linked)
                return Reject("Wait for a current qualified heartbeat and enabled protected relay.");
            requiredEpoch = epoch + 1;
            if (!RemoveWire(jumper)) return Reject("Could not open the physical W209 link.");
            Solve();
            return true;
        }

        public bool RestoreW209()
        {
            if (stage != 5 || !interrupted || linked) return Reject("Record the supervised W209 opening first.");
            SelectRoute(routes.Length);
            if (!CreateWire(jumper)) return Reject("W209’s native terminals rejected the connection.");
            ReadGraph();
            Solve();
            message = "W209 restored. Recalibrate the fixture and repeat suppression and packet testing before resetting U9.";
            return true;
        }

        public bool ResetSafety()
        {
            if (stage != 5 || !interrupted || safetyReset || epoch < requiredEpoch || !Fresh || !Inhibited ||
                Voltage(monitorSupply) < 3f)
                return Reject("Restore W209 and acquire new spectrum, packet and heartbeat evidence before resetting U9.");
            safetyReset = true;
            requiredEpoch = epoch + 1;
            InvalidateDiagnostics();
            Solve();
            message = "U9 reset accepted. Repeat fixture, baseline and loaded qualification to restore the protected relay.";
            return true;
        }

        private void ClearHeartbeat()
        {
            feedbackIntervals = 0;
            lastRise = -1f;
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
                benchState = "EMC GAUNTLET CERTIFIED";
                message = "Fresh spectrum and packet evidence restored the independent W209 workload permit.";
            }
        }
    }
}
