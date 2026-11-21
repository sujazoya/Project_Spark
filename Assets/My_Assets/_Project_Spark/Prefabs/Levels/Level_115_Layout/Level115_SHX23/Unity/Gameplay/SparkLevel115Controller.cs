using System;
using System.Globalization;
using System.Security.Cryptography;
using UnityEngine;

namespace ProjectSpark.Gameplay.SecureEnclave
{
    public sealed partial class SparkLevel115Process
    {
        public SparkTerminal standby, aux, secure, clock, reset, flash, meshSense, monitorSupply, protectedFeed;
        public SparkTerminal railWitness, clockWitness;
        public SparkSwitch bench, auxGate, secureContact, clockGate, resetGate, flashContact, heartbeat, workload;
        public SparkSwitch[] shieldSeams;
        public int[] shieldParts;
        public int railPart, flashPart, flashViaIndex;

        private enum BootStep { Off, Bench, Power, ResetHeld, Rom, Read, Digest, Signature, Version, Authenticated, Attested, Blocked }
        public enum BootFailure { None, Supply, ResetClock, Flash, Digest, Signature, Rollback, Tamper, Verification }
        private const string CapsuleIdentity = "SHX23-EDU-1";
        private readonly bool[] shieldRetained = new bool[4];
        private SparkLevel115Attestor attestor;
        private SparkLevel115Firmware.Image image;
        private BootStep bootStep;
        private string digest, challenge;
        private float time, nextStep, restoreAt, heartbeatAt, lastRise = -1f, scopeAt;
        private bool heartbeatDrive, highFeedback, tamperLatched, faultWitnessed, reworked, replacementPrepared;
        private bool armed, interrupted, safetyReset;
        private int epoch, bootEpoch, trustedEpoch = -1, trustedBoot = -1, minimumBoot, versionFloor, imageProfile;
        private int activeFault = -1, repaired, repairTarget = -1, flashInspectionEpoch = -1, feedbackIntervals;
        private string evidence = "", benchState = "BENCH ISOLATED";

        public bool ShieldsMounted => Array.TrueForAll(shieldParts, Mounted);
        public bool RailsReady => powered && Voltage(standby) > 4.7f && Voltage(aux) > 3.05f && Voltage(secure) > 1.69f;
        public bool Booting => bootStep is >= BootStep.Power and <= BootStep.Version;
        public bool BootAuthenticated => bootStep is BootStep.Authenticated or BootStep.Attested;
        public bool Trusted => bootStep == BootStep.Attested && trustedEpoch == epoch && trustedBoot == bootEpoch &&
            RailsReady && Voltage(clock) > 3f && Voltage(reset) > 3f && Voltage(flash) > 4.6f && MeshIntact && !tamperLatched;
        public bool Fresh => linked && Trusted && feedbackIntervals >= 2 && time - lastRise < .51f;
        public bool TamperLatched => tamperLatched;
        public int Epoch => epoch;
        public int BootEpoch => bootEpoch;
        public int Repaired => repaired;
        public int VersionFloor => versionFloor;
        public string Evidence => evidence;
        public string ChallengeId => challenge;
        public BootFailure Failure { get; private set; }
        public float FailureReading { get; private set; }

        private bool MeshIntact => Voltage(meshSense) > 3.02f;
        private Route FlashVia => routes[flashViaIndex];
        private int ExpectedPart => activeFault switch { 0 => railPart, 1 => flashPart, 2 => shieldParts[2], _ => -1 };
        private bool FaultLive(int index) => activeFault == index && !reworked;
        private void Solve() => manager.ElectricalSolver.SolveNow();

        private bool Mounted(int index)
        {
            int shield = Array.IndexOf(shieldParts, index);
            return shield >= 0 && shieldRetained[shield] && parts[index].contact.IsClosed;
        }

        private void SetMounted(int index, bool retained)
        {
            int shield = Array.IndexOf(shieldParts, index);
            shieldRetained[shield] = retained;
            parts[index].model.SetActive(retained);
            SetContact(shieldSeams[shield], retained && (shield != 2 || !FaultLive(2)));
        }

        public bool TogglePower()
        {
            if (powered) { IsolatePower(); return true; }
            if (stage < 2 || !Construction || (!linked && !interrupted))
                return Reject("Complete the circuit, shield seams and initial W273 connection before enabling the bench.");
            powered = true;
            SetContact(bench, true);
            SetContact(auxGate, true);
            Solve();
            SetContact(secureContact, Voltage(railWitness) > 4.7f);
            SetContact(clockGate, Voltage(clockWitness) > 4.7f);
            SetContact(flashContact, true);
            SetContact(resetGate, false);
            Solve();
            bootStep = BootStep.Bench;
            benchState = "RESET HELD";
            message = "Probe protected rails, the clock witness and hold state.";
            return true;
        }

        private void IsolatePower()
        {
            powered = false;
            foreach (var contact in new[] { bench, auxGate, secureContact, clockGate, flashContact, resetGate })
                SetContact(contact, false);
            InvalidateDiagnostics();
            Solve();
            benchState = "BENCH ISOLATED";
            message = "Bench power isolated for rework.";
        }

        public bool ProbeCommissioning()
        {
            if (!powered) return Reject("Enable the diagnostic bench first.");
            Solve();
            float five = Voltage(standby), diagnostic = Voltage(aux), secureRail = Voltage(secure);
            float osc = Voltage(clock), release = Voltage(reset), shield = Voltage(meshSense);
            evidence = $"5 V {five:F2} · DIAG {diagnostic:F2} · U2 {secureRail:F2} V\nCLK {osc:F2} · POR {release:F2} · MESH {shield:F2} V";
            if (stage > 2) { message = "Independent native probe readings refreshed."; return true; }
            if (!RailsReady || osc < 3f || release > .15f || !MeshIntact || Voltage(flash) < 4.6f)
                return Reject("Commissioning requires stable rails, clock, held reset, flash and shield continuity.");
            if (!Award()) return false;
            message = "Commissioned. Cold-boot the signed image, issue a nonce, then verify attestation.";
            return true;
        }

        public void NextImage() => SelectImage((imageProfile + 1) % SparkLevel115Firmware.Profiles.Length);

        public bool SelectImage(int profile)
        {
            if (profile < 0 || profile >= SparkLevel115Firmware.Profiles.Length || !powered || stage is < 3 or > 5 || Booting)
                return Reject("Select an available study image between boot attempts.");
            if (profile == imageProfile) return true;
            imageProfile = profile;
            InvalidateDiagnostics();
            message = "Selected image " + SparkLevel115Firmware.Profiles[profile] + ". Start a new cold boot.";
            return true;
        }

        public bool BeginBoot()
        {
            if (stage is < 3 or > 5 || !RailsReady || !ShieldsMounted || Booting || tamperLatched || !MeshIntact)
                return Reject("Boot requires stable rails, all shield seams and an explicitly reset tamper latch.");
            InvalidateDiagnostics();
            bootEpoch++;
            image = SparkLevel115Firmware.Select(imageProfile);
            Failure = BootFailure.None;
            FailureReading = 0f;
            scope?.ResetTrace();
            scope?.Record(Voltage(secure), Voltage(flash), Voltage(meshSense), Voltage(reset));
            bootStep = BootStep.Power;
            benchState = "SUPPLY QUALIFICATION";
            message = $"Boot {bootEpoch} · {SparkLevel115Firmware.Profiles[imageProfile]}";
            nextStep = time + .18f;
            scopeAt = time;
            return true;
        }

        private void TickController(float dt)
        {
            time += dt;
            if (!powered) return;
            if (restoreAt > 0f && time >= restoreAt)
            {
                restoreAt = 0f;
                SetContact(secureContact, true);
                SetContact(flashContact, true);
                Solve();
            }

            if (!MeshIntact && !tamperLatched)
            {
                tamperLatched = true;
                Fail(BootFailure.Tamper, Voltage(meshSense), "U10 detected an open shield mesh. U8 latched inhibit.");
                QueueCheckpoint();
            }
            if (Booting && time >= nextStep) AdvanceBoot();
            if ((Booting || bootStep == BootStep.Blocked && time < restoreAt + .12f) && time >= scopeAt)
            {
                scope?.Record(Voltage(secure), Voltage(flash), Voltage(meshSense), Voltage(reset));
                scopeAt = time + .034f;
            }
            if (BootAuthenticated && !RailsReady)
                Fail(BootFailure.Supply, Voltage(secure), "U2 secure rail is outside the training tolerance.");
            if (BootAuthenticated && (Voltage(reset) < 3f || Voltage(clock) < 3f))
                Fail(BootFailure.ResetClock, Voltage(reset), "The current boot lost its clock or released reset.");
            if (BootAuthenticated && Voltage(flash) < 4.6f)
                Fail(BootFailure.Flash, Voltage(flash), "The current boot lost its flash continuity witness.");
            TickSupervisor();
        }

        private void AdvanceBoot()
        {
            nextStep = time + .18f;
            switch (bootStep)
            {
                case BootStep.Power:
                    if (!RailsReady) { Fail(BootFailure.Supply, Voltage(secure), "Secure rail qualification failed."); return; }
                    bootStep = BootStep.ResetHeld;
                    benchState = "RESET HELD";
                    break;
                case BootStep.ResetHeld:
                    if (Voltage(reset) > .15f || Voltage(clock) < 3f)
                    { Fail(BootFailure.ResetClock, Voltage(clock), "Reset hold or clock witness failed."); return; }
                    SetContact(resetGate, true);
                    Solve();
                    bootStep = BootStep.Rom;
                    benchState = "BOOT ROM";
                    break;
                case BootStep.Rom:
                    if (FaultLive(0))
                    {
                        SetContact(secureContact, false);
                        Solve();
                        restoreAt = time + .25f;
                    }
                    if (!RailsReady) { Fail(BootFailure.Supply, Voltage(secure), "Q1 secure feed dipped during ROM entry."); return; }
                    if (Voltage(reset) < 3f || Voltage(clock) < 3f)
                    { Fail(BootFailure.ResetClock, Voltage(reset), "The released reset or clock reading failed."); return; }
                    bootStep = BootStep.Read;
                    benchState = "READING SIGNED FLASH";
                    break;
                case BootStep.Read:
                    if (FaultLive(1))
                    {
                        SetContact(flashContact, false);
                        Solve();
                        restoreAt = time + .23f;
                    }
                    if (Voltage(flash) < 4.6f)
                    { Fail(BootFailure.Flash, Voltage(flash), "U4 lost read continuity during the native via cycle."); return; }
                    bootStep = BootStep.Digest;
                    benchState = "MEASURING SHA-256";
                    break;
                case BootStep.Digest:
                    digest = SparkLevel115Firmware.Digest(image.Payload);
                    if (digest != image.Digest) { Fail(BootFailure.Digest, 0f, "Image bytes do not match the signed manifest digest."); return; }
                    bootStep = BootStep.Signature;
                    benchState = "VERIFYING SIGNATURE";
                    break;
                case BootStep.Signature:
                    try
                    {
                        if (!SparkLevel115Firmware.VerifySignature(image))
                        { Fail(BootFailure.Signature, 0f, "The image signature is not trusted by the pinned workshop root."); return; }
                    }
                    catch (CryptographicException)
                    { Fail(BootFailure.Verification, 0f, "The workshop signature verifier could not complete its check."); return; }
                    bootStep = BootStep.Version;
                    benchState = "CHECKING VERSION POLICY";
                    break;
                case BootStep.Version:
                    if (image.Version < Mathf.Max(versionFloor, SparkLevel115Firmware.MinVersion))
                    { Fail(BootFailure.Rollback, image.Version, "Publisher authenticated, but the version violates the rollback floor."); return; }
                    bootStep = BootStep.Authenticated;
                    evidence = $"Boot {bootEpoch} · version {image.Version} · policy {SparkLevel115Firmware.PolicyRevision}\nSHA-256 {digest.Substring(0, 12)}… · RSA signature verified";
                    benchState = "BOOT AUTHENTICATED";
                    message = "Request a fresh challenge from the independent verifier.";
                    break;
            }
        }

        public bool IssueChallenge()
        {
            if (bootStep != BootStep.Authenticated || !RailsReady || !MeshIntact || tamperLatched)
                return Reject("Authenticate the current boot before requesting a challenge.");
            try
            {
                attestor ??= new SparkLevel115Attestor();
                challenge = attestor.Challenge(AttestationSnapshot());
            }
            catch (CryptographicException)
            { return Reject("The simulated device could not sign the challenge."); }
            message = $"Challenge {challenge.Substring(0, 10)}… signed by the simulated device. Verify the response.";
            return true;
        }

        private string AttestationSnapshot() => FormattableString.Invariant(
            $"{CapsuleIdentity}|{epoch}|{bootEpoch}|{image.Version}|{digest}|{SparkLevel115Firmware.PolicyRevision}|{MeshIntact && !tamperLatched}");

        public bool VerifyAttestation()
        {
            if (bootStep != BootStep.Authenticated || challenge == null || !RailsReady || !MeshIntact || tamperLatched)
                return Reject("A fresh, unconsumed nonce and current authenticated boot are required.");
            try
            {
                if (!attestor.Verify(AttestationSnapshot()))
                { Fail(BootFailure.Verification, 0f, "The quote did not match the current board and nonce."); return false; }
            }
            catch (CryptographicException)
            { Fail(BootFailure.Verification, 0f, "The independent verifier rejected the response."); return false; }
            bootStep = BootStep.Attested;
            trustedBoot = bootEpoch;
            trustedEpoch = epoch;
            versionFloor = Mathf.Max(versionFloor, image.Version);
            evidence = $"Signed nonce {challenge.Substring(0, 10)}… · boot {bootEpoch}\nVersion {image.Version} · policy {SparkLevel115Firmware.PolicyRevision} · mesh clear";
            QueueCheckpoint();
            benchState = "ATTESTED";
            message = "Fresh boot attestation verified.";
            if (stage == 3)
            {
                if (Award()) message = "Trusted boot proven. Begin the first hardware fault trial.";
            }
            else if (stage == 4 && activeFault >= 0 && reworked)
            {
                repaired++;
                activeFault = repairTarget = -1;
                faultWitnessed = reworked = replacementPrepared = false;
                QueueCheckpoint();
                if (repaired == 3)
                {
                    if (Award()) message = "All repairs requalified. Wait for the W273 permit and open the jumper.";
                }
                else message = "A fresh authenticated boot verified this repair. Start the next fault trial.";
            }
            return true;
        }

        private void Fail(BootFailure failure, float value, string description)
        {
            Failure = failure;
            FailureReading = value;
            bootStep = BootStep.Blocked;
            trustedEpoch = trustedBoot = -1;
            attestor?.Clear();
            challenge = null;
            SetContact(resetGate, false);
            ClearHeartbeat();
            Solve();
            benchState = "BOOT INHIBITED";
            evidence = $"Boot {bootEpoch} · {failure} · {value:F2}\n{description}";
            message = description;
        }

        private void InvalidateDiagnostics()
        {
            epoch++;
            bootStep = powered ? BootStep.Bench : BootStep.Off;
            challenge = digest = null;
            trustedEpoch = trustedBoot = -1;
            attestor?.Clear();
            SetContact(resetGate, false);
            ClearHeartbeat();
            Solve();
        }

        public bool BeginFaultTrial()
        {
            if (stage != 4 || !Trusted || activeFault >= 0 || repaired >= 3)
                return Reject("Attest the healthy board before starting the next fault trial.");
            activeFault = repaired;
            repairTarget = -1;
            faultWitnessed = reworked = replacementPrepared = false;
            InvalidateDiagnostics();
            if (FaultLive(2)) SetContact(shieldSeams[2], false);
            Solve();
            message = activeFault == 2 ? "Observe the independent shield monitor and locate the open seam." :
                "Boot again and inspect the captured supply or flash discontinuity.";
            QueueCheckpoint();
            return true;
        }

        public bool LocateFault()
        {
            bool observed = powered && (activeFault switch
            {
                0 => Failure == BootFailure.Supply && FailureReading < 1.69f,
                1 => Failure == BootFailure.Flash && FailureReading < 4.6f,
                2 => Failure == BootFailure.Tamper && tamperLatched && !MeshIntact,
                _ => false
            });
            if (!observed || reworked) return Reject("Measure the current failure before selecting a repair target.");
            faultWitnessed = true;
            footprint = repairTarget = ExpectedPart;
            FocusPart();
            message = activeFault switch
            {
                0 => $"Q1 dipped to {FailureReading:F2} V. Isolate and renew the secure feed contact.",
                1 => $"U4 read dropped to {FailureReading:F2} V. Isolate and inspect the flash via.",
                _ => "SH3 opens the native tamper loop. Isolate, lift the shield and repair its seam."
            };
            QueueCheckpoint();
            return true;
        }

        public bool InspectFlashSpan()
        {
            if (stage != 4 || powered || !faultWitnessed || activeFault != 1 || repairTarget != flashPart)
                return Reject("Locate U4's read fault and isolate the bench before inspecting the via.");
            SelectRoute(flashViaIndex);
            PadView();
            flashInspectionEpoch = epoch;
            message = $"{FlashVia.viaId} spans L{FlashVia.layer:00}–L{FlashVia.exitLayer:00}. Retain its entry layer for repair.";
            return true;
        }

        public bool ToggleService()
        {
            if (powered || stage is < 2 or > 5 || Array.IndexOf(shieldParts, repairTarget) < 0)
                return Reject("Isolate the bench and select a shield can.");
            bool mounting = !Mounted(repairTarget);
            if (mounting && activeFault == 2 && repairTarget == ExpectedPart && replacementPrepared)
            {
                reworked = true;
                replacementPrepared = false;
            }
            SetMounted(repairTarget, mounting);
            InvalidateDiagnostics();
            message = $"{parts[repairTarget].label} {(mounting ? "retained on its seam." : "lifted from its mount.")}";
            QueueCheckpoint();
            return true;
        }

        public bool Repair()
        {
            if (stage != 4 || powered || !faultWitnessed || reworked || repairTarget != ExpectedPart)
                return Reject("Isolate the bench and select the measured repair target.");
            if (activeFault == 1 && (flashInspectionEpoch != epoch || layer != FlashVia.layer || !FlashVia.barrel.IsClosed))
                return Reject("Inspect the diagnosed flash span and select its entry layer.");
            if (activeFault == 2)
            {
                if (Mounted(repairTarget)) return Reject("Lift SH3 before repairing the seam.");
                replacementPrepared = true;
                message = "SH3 seam repaired. Remount the shield and confirm loop continuity before reset.";
            }
            else
            {
                reworked = true;
                message = activeFault == 0 ? "Q1 secure feed renewed. Restore power and cold boot." :
                    "Boot flash via continuity rebuilt. Restore power and cold boot.";
            }
            QueueCheckpoint();
            return true;
        }

        public bool ResetTamper()
        {
            if (!powered || !tamperLatched || !ShieldsMounted || !MeshIntact || FaultLive(2))
                return Reject("Restore the physical tamper loop and verify it under diagnostic power before clearing the latch.");
            tamperLatched = false;
            InvalidateDiagnostics();
            message = "Tamper latch cleared. Cold boot and attest the newly measured state.";
            QueueCheckpoint();
            return true;
        }

        public bool OpenW273()
        {
            if (stage != 5 || !armed || !Fresh || interrupted) return Reject("Wait for an active, fresh protected workload permit.");
            minimumBoot = bootEpoch + 1;
            if (!RemoveWire(jumper)) return Reject("The native circuit could not open W273.");
            Solve();
            return true;
        }

        public bool RestoreW273()
        {
            if (stage != 5 || !interrupted || linked) return Reject("Record the supervised W273 opening first.");
            SelectRoute(routes.Length);
            if (!CreateWire(jumper)) return Reject("The native circuit rejected W273.");
            ReadGraph();
            Solve();
            message = "W273 restored. Cold boot and attest a fresh nonce before resetting U8.";
            return true;
        }

        public bool ResetSafety()
        {
            if (stage != 5 || !interrupted || safetyReset || bootEpoch < minimumBoot || !Fresh || !Inhibited ||
                Voltage(monitorSupply) < 3f)
                return Reject("U8 needs a reconnected link, a newer boot and fresh nonce-bound attestation before reset.");
            safetyReset = true;
            ClearHeartbeat();
            Solve();
            message = "U8 reset accepted. Waiting for the verified heartbeat to reauthorize workload.";
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
            if (Trusted && time >= heartbeatAt)
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
            bool permit = Fresh && Voltage(monitorSupply) > 3f && (!interrupted || safetyReset && bootEpoch >= minimumBoot);
            if (workload.IsClosed != permit)
            {
                SetContact(workload, permit);
                Solve();
            }
            if (!permit || Voltage(protectedFeed) < 3f || stage != 5) return;
            if (!interrupted) armed = true;
            else if (safetyReset && Award())
            {
                benchState = "TRAINING BOARD AUTHORIZED";
                message = "U8 reauthorized the protected workload after W273 recovery and fresh signed boot evidence.";
            }
        }

        private void DisposeAttestor() { attestor?.Dispose(); attestor = null; }
    }
}
