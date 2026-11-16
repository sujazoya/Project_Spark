using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectSpark.Visuals
{
    /// <summary>
    /// ART ONLY: Level 81 digital mission reliability animation. All incoming values must come
    /// from actual game simulation. This script neither performs SECDED nor awards objectives.
    /// Does not create SparkTerminals, player wires, radiation faults or safety logic.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkLevel81VisualRig : MonoBehaviour
    {
        [Header("Optional actual-state lights")]
        [SerializeField] private Light faultEventLight;
        [SerializeField] private Light eccCorrectedLight;
        [SerializeField] private Light uncorrectableErrorLight;
        [SerializeField] private Light tmrQualifiedLight;
        [SerializeField] private Light tmrDisagreementLight;
        [SerializeField] private Light criticalECCFaultLight;
        [SerializeField] private Light trustedECCFeedbackLight;
        [SerializeField] private Light missionReadyLight;
        [SerializeField] private ParticleSystem fictionalBitstormHologram;
        [SerializeField] private ParticleSystem correctionPulseVFX;

        public Transform art;
        public Transform[] pivots;
        private readonly Dictionary<string, Transform> pivotLookup = new Dictionary<string, Transform>();
        private readonly Dictionary<string, Vector3> initialPositions = new Dictionary<string, Vector3>();
        private readonly Dictionary<string, Quaternion> initialRotations = new Dictionary<string, Quaternion>();

        private float stormIntensity01, eventAzimuthDeg, eventDirectorX01, eventDirectorY01;
        private float memoryCartridgeA01, memoryCartridgeB01, scrubberHead01, quarantineGate01;
        private float checkpointSlide01, coreProbe01, voterGate01, watchdogAngleDeg;
        private float commandGate01, telemetryDock01, powerIsolator01, emergencyHoldButton01;
        private float missionConfidence01, correctionStrength01, uncorrectableSeverity01;
        private float tmrDisagreement01, packetActivity01, packetRetransmit01;
        private bool eccTrustedFeedback, criticalFaultLatched, missionSafeHold;
        private bool eccJustCorrected, eventActive, majorityVotingSafe, packetsVerified;
        private float faultChamberLid01, injectionGuard01;

        private void Awake()
        {
            foreach (Transform t in pivots != null && pivots.Length > 0 ? pivots : GetComponentsInChildren<Transform>(true))
            {
                if (t == null) continue;
                int offset=t.name.IndexOf("ANIM_",StringComparison.Ordinal);
                if(offset<0)continue;
                string key=t.name.Substring(offset);
                if (pivotLookup.ContainsKey(key))
                {
                    Debug.LogWarning("[Level81VisualRig] Duplicate pivot " + t.name, this);
                    continue;
                }
                pivotLookup.Add(key, t);
                initialPositions.Add(key, t.localPosition);
                initialRotations.Add(key, t.localRotation);
            }
            if (pivotLookup.Count != 52)
                Debug.LogWarning("[Level81VisualRig] Found " + pivotLookup.Count + "/52 actual named FBX pivotLookup; ensure 70 numbered FBXs imported.", this);
        }

        /// <summary>Actual fictional fault environment, never synthetic truth from HUD alone.</summary>
        public void SetActualFaultEnvironment(float actualStormIntensity01, float actualProjectorAzimuthDeg,
            float actualDirectorX01, float actualDirectorY01, float actualChamberDoor01,
            float actualGuardOpen01, bool actualEventActive)
        {
            stormIntensity01 = Mathf.Clamp01(actualStormIntensity01);
            eventAzimuthDeg = actualProjectorAzimuthDeg;
            eventDirectorX01 = Mathf.Clamp01(actualDirectorX01);
            eventDirectorY01 = Mathf.Clamp01(actualDirectorY01);
            faultChamberLid01 = Mathf.Clamp01(actualChamberDoor01);
            injectionGuard01 = Mathf.Clamp01(actualGuardOpen01);
            eventActive = actualEventActive;
        }

        /// <summary>Validated SECDED state from memory model and scrubber, not imagined pass flags.</summary>
        public void SetActualECCState(bool trustedSyndromeFeedbackValid, float actualCorrection01,
            float actualUncorrectableSeverity01, float actualScrubPosition01,
            float actualQuarantineGate01, float actualCheckpointSlide01,
            float actualMemoryBankACartridge01, float actualMemoryBankBCartridge01,
            bool actualCorrectionOccurred)
        {
            eccTrustedFeedback = trustedSyndromeFeedbackValid;
            correctionStrength01 = Mathf.Clamp01(actualCorrection01);
            uncorrectableSeverity01 = Mathf.Clamp01(actualUncorrectableSeverity01);
            scrubberHead01 = Mathf.Clamp01(actualScrubPosition01);
            quarantineGate01 = Mathf.Clamp01(actualQuarantineGate01);
            checkpointSlide01 = Mathf.Clamp01(actualCheckpointSlide01);
            memoryCartridgeA01 = Mathf.Clamp01(actualMemoryBankACartridge01);
            memoryCartridgeB01 = Mathf.Clamp01(actualMemoryBankBCartridge01);
            eccJustCorrected = actualCorrectionOccurred;
        }

        /// <summary>Actual three-lane compute/voting results evaluated by the game, not the rig.</summary>
        public void SetActualRedundancy(bool actualVotedResultQualified, float measuredDisagreement01,
            float actualVoterGate01, float actualHealthProbe01, float actualWatchdogHandAngle,
            float actualSafeCommandGate01)
        {
            majorityVotingSafe = actualVotedResultQualified;
            tmrDisagreement01 = Mathf.Clamp01(measuredDisagreement01);
            voterGate01 = Mathf.Clamp01(actualVoterGate01);
            coreProbe01 = Mathf.Clamp01(actualHealthProbe01);
            watchdogAngleDeg = actualWatchdogHandAngle;
            commandGate01 = Mathf.Clamp01(actualSafeCommandGate01);
        }

        public void SetActualPacketIntegrity(float measuredPacketActivity01,
            float actualRetransmissionProgress01, bool actualCRCAndFreshnessVerified,
            float actualTelemetryDock01)
        {
            packetActivity01 = Mathf.Clamp01(measuredPacketActivity01);
            packetRetransmit01 = Mathf.Clamp01(actualRetransmissionProgress01);
            packetsVerified = actualCRCAndFreshnessVerified;
            telemetryDock01 = Mathf.Clamp01(actualTelemetryDock01);
        }

        /// <summary>Trusted W094 feedback must be valid in the actual Spark electrical graph.</summary>
        public void SetActualMissionSafety(bool actualCriticalFaultLatched, bool trustedECCFeedbackValid,
            bool actualMissionSafeHold, float actualPowerIsolator01,
            float actualEmergencyHoldButton01, float actualMissionConfidence01)
        {
            criticalFaultLatched = actualCriticalFaultLatched;
            eccTrustedFeedback = trustedECCFeedbackValid;
            missionSafeHold = actualMissionSafeHold;
            powerIsolator01 = Mathf.Clamp01(actualPowerIsolator01);
            emergencyHoldButton01 = Mathf.Clamp01(actualEmergencyHoldButton01);
            missionConfidence01 = Mathf.Clamp01(actualMissionConfidence01);
        }

        public void BindEffects(Light ecc,Light voter,Light telemetry,Light alarm)
        {
            fittedLights=new[]{ecc,voter,telemetry,alarm};
            foreach(var lamp in fittedLights)lamp.enabled=false;
        }
        private Light[] fittedLights;
        private int previousCorrections;
        private float correctionFlash;
        public void Apply(ProjectSpark.Gameplay.SparkLevel81Plant p,bool trusted,int phase,float elapsed,float hold,bool fault,bool armed)
        {
            if(previousCorrections!=p.Corrections){previousCorrections=p.Corrections;correctionFlash=1;}
            correctionFlash=Mathf.MoveTowards(correctionFlash,0,Time.deltaTime*.8f);
            SetActualFaultEnvironment(p.Storm,elapsed*32%360,p.Storm,p.Storm*.74f,0,armed?1:0,p.Storm>.02f);
            SetActualECCState(trusted,correctionFlash,p.QuarantineCount>0?1:0,(p.Inspected%256)/255f,
                p.QuarantineCount>0?1:0,p.Recovery,p.Initialized?1:0,p.Initialized?1:0,correctionFlash>.01f);
            SetActualRedundancy(p.VoterValid,p.FailedLane>=0?.55f:0,p.VoterValid?1:0,p.Watchdog?1:0,elapsed*288%360,phase>0?1-hold:0);
            bool packets=phase>0&&p.LastPacket==ProjectSpark.Gameplay.SparkLevel81Plant.Packet.Accepted;
            SetActualPacketIntegrity(packets?.68f:0,p.Recovery,packets,p.Initialized?1:0);
            SetActualMissionSafety(fault,trusted,hold>.01f,hold,hold,phase/3f);
            if(fittedLights==null)return;
            fittedLights[0].enabled=correctionFlash>.01f||trusted;
            fittedLights[1].enabled=p.VoterValid&&phase>0&&!fault;
            fittedLights[2].enabled=packets&&!fault;
            fittedLights[3].enabled=fault;
        }

        private void Translate(string name, Vector3 localOffset)
        {
            if (pivotLookup.TryGetValue(name, out Transform t))
                t.localPosition = initialPositions[name] + (art!=null?t.parent.InverseTransformVector(art.TransformVector(new Vector3(-localOffset.x,localOffset.y,-localOffset.z)*.096f)):localOffset);
        }
        private void Turn(string name, Vector3 euler)
        {
            if (pivotLookup.TryGetValue(name, out Transform t))
                t.localRotation = initialRotations[name] * Quaternion.Euler(euler);
        }
        private static void Lit(Light lamp, bool enabled, float brightness = 1f)
        {
            if (lamp == null) return;
            lamp.enabled = enabled;
            lamp.intensity = Mathf.Max(0f, brightness);
        }

        private void LateUpdate()
        {
            Turn("ANIM_BITSTORM_PROJECTOR_ROTATION", new Vector3(0f, eventAzimuthDeg, 0f));
            Translate("ANIM_EVENT_DIRECTOR_X", new Vector3(.32f * (eventDirectorX01 - .5f), 0f, 0f));
            Translate("ANIM_EVENT_DIRECTOR_Y", new Vector3(0f, .22f * (eventDirectorY01 - .5f), 0f));
            Turn("ANIM_FAULT_INJECTION_CHAMBER_DOOR", new Vector3(70f * faultChamberLid01, 0f, 0f));
            Turn("ANIM_FAULT_CONSOLE_GUARD", new Vector3(65f * injectionGuard01, 0f, 0f));
            Translate("ANIM_MEMORY_BANK_A_CARTRIDGE", new Vector3(0f, 0f, .16f * memoryCartridgeA01));
            Translate("ANIM_MEMORY_BANK_B_CARTRIDGE", new Vector3(0f, 0f, .16f * memoryCartridgeB01));
            Translate("ANIM_SCRUBBER_SCAN_HEAD", new Vector3(.57f * (scrubberHead01 - .5f), 0f, 0f));
            Translate("ANIM_PAGE_QUARANTINE_GATE", new Vector3(0f, .48f * quarantineGate01, 0f));
            Translate("ANIM_GOLDEN_CHECKPOINT_RESTORE", new Vector3(0f, 0f, .24f * checkpointSlide01));
            Translate("ANIM_COMPUTE_HEALTH_PROBE", new Vector3(0f, .18f * coreProbe01, 0f));
            Translate("ANIM_TMR_VOTER_GATE", new Vector3(0f, .38f * voterGate01, 0f));
            Turn("ANIM_WATCHDOG_TIMER", new Vector3(0f, 0f, watchdogAngleDeg));
            Translate("ANIM_SAFE_COMMAND_GATE", new Vector3(0f, .55f * commandGate01, 0f));
            Translate("ANIM_OPTICAL_TELEMETRY_DOCK", new Vector3(.28f * telemetryDock01, 0f, 0f));
            Translate("ANIM_POWER_DISTRIBUTION_ISOLATOR", new Vector3(0f, .28f * powerIsolator01, 0f));
            Translate("ANIM_EMERGENCY_SAFE_HOLD", new Vector3(0f, 0f, .12f * emergencyHoldButton01));

            Lit(faultEventLight, eventActive, Mathf.Max(.2f, stormIntensity01 * 2.2f));
            Lit(eccCorrectedLight, eccTrustedFeedback && eccJustCorrected, .30f + correctionStrength01 * 2.2f);
            Lit(uncorrectableErrorLight, uncorrectableSeverity01 > .05f, uncorrectableSeverity01 * 2.0f);
            Lit(tmrQualifiedLight, majorityVotingSafe && !missionSafeHold);
            Lit(tmrDisagreementLight, tmrDisagreement01 > .05f, .3f + tmrDisagreement01 * 1.7f);
            Lit(criticalECCFaultLight, criticalFaultLatched);
            Lit(trustedECCFeedbackLight, eccTrustedFeedback && !criticalFaultLatched);
            Lit(missionReadyLight, missionConfidence01 > .95f && packetsVerified && !criticalFaultLatched);
            if (fictionalBitstormHologram != null)
            {
                var emission = fictionalBitstormHologram.emission;
                emission.rateOverTime = stormIntensity01 * 36f;
            }
            if (correctionPulseVFX != null)
            {
                var emission = correctionPulseVFX.emission;
                emission.rateOverTime = eccJustCorrected ? 16f : 0f;
            }
        }
    }
}

