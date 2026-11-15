#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.EditorTools
{
    public static class SparkLevel85AssetValidatorEditor
    {
        private static readonly string[] ExpectedNames = new string[]
        {
            "L85_OrbitalDockingArenaBase",
            "L85_PanoramicEarthAndStarfieldViewport",
            "L85_StationOrbitalTrussSpine",
            "L85_StationPrimaryDockingPort",
            "L85_DockingOuterGuideRing",
            "L85_DockingPassiveCaptureCone",
            "L85_StationOpticalTargetMarkerArray",
            "L85_StationSolarWingModule",
            "L85_StationThermalRadiatorWing",
            "L85_StationApproachBeaconCluster",
            "L85_StationServiceAirlockModule",
            "L85_StationDockingPowerInterface",
            "L85_StationCaptureStructuralFrame",
            "L85_ThreeDimensionalApproachCorridorHologram",
            "L85_RelativeOrbitTrajectoryHologram",
            "L85_EarthOrbitalReferenceHologram",
            "L85_ExternalInspectionCameraGantry",
            "L85_StationApproachIlluminationRig",
            "L85_AutonomousServiceSpacecraftBody",
            "L85_ServiceSpacecraftExtendableDockingProbe",
            "L85_ProtectedServiceVehicleAvionics",
            "L85_ServiceSpacecraftPortSolarWing",
            "L85_ServiceSpacecraftStarboardSolarWing",
            "L85_ReactionControlPropellantManifold",
            "L85_RCSPlusXThruster",
            "L85_RCSMinusXThruster",
            "L85_RCSPlusYThruster",
            "L85_RCSMinusYThruster",
            "L85_RCSPlusZThruster",
            "L85_RCSMinusZThruster",
            "L85_PropellantTankA",
            "L85_PropellantTankB",
            "L85_ThrusterValveDriverRack",
            "L85_ThrusterFlowAndPressureMonitor",
            "L85_PrecisionThrusterCalibrationFixture",
            "L85_SpacecraftInertialMeasurementUnit",
            "L85_StereoNavigationCameraLeft",
            "L85_StereoNavigationCameraRight",
            "L85_PrimaryScanningLiDAR",
            "L85_SecondaryScanningLiDAR",
            "L85_ShortRangeRendezvousRadar",
            "L85_DockingBeaconReceiver",
            "L85_OpticalTargetPatternTracker",
            "L85_RelativePositionVelocityEstimator",
            "L85_RelativeNavigationFusionComputer",
            "L85_NavigationUncertaintyVolumeDisplay",
            "L85_PrecisionFlightTimeReferenceClock",
            "L85_RelativeTrajectoryPlanningComputer",
            "L85_ProximityOperationsGuidanceComputer",
            "L85_SixDegreeFreedomFlightController",
            "L85_ThrusterForceAllocationComputer",
            "L85_DockingCorridorSafetyGuardian",
            "L85_CollisionAvoidancePredictionMonitor",
            "L85_FuelReserveAndAbortPredictor",
            "L85_ProtectedAutonomousAbortController",
            "L85_ActiveSoftCaptureRing",
            "L85_MotorizedCaptureGuidePetals",
            "L85_SixActuatorDockingLatchArray",
            "L85_SixAxisContactForceSensor",
            "L85_CompliantContactDampingActuator",
            "L85_DockingElectricalUmbilicalCoupler",
            "L85_PressureInterfaceSealGasket",
            "L85_PressureLeakQualificationController",
            "L85_HardDockStructuralLoadMonitor",
            "L85_DockingMechanismSequencingPLC",
            "L85_PrecisionDockingCameraMonitor",
            "L85_LiDARBlindSpotFaultEmulator",
            "L85_StuckThrusterValveFaultEmulator",
            "L85_AsymmetricLatchFaultEmulator",
            "L85_CriticalClosingRateFeedbackStation",
            "L85_DockingSafetyPLC",
            "L85_Protected5VControlSupply",
            "L85_DockingPowerDistributionCabinet",
            "L85_HolographicRendezvousSCADA",
            "L85_ElectricalTerminalDiagnosticRack",
            "L85_EmergencyApproachHoldStation",
            "L85_DockingMissionCertificationBeacon",
            "L85_PrecisionFlightEventDataRecorder"
        };
        [MenuItem("Tools/Project Spark/Levels/85/Validate Imported HD Art")]
        public static void Validate()
        {
            var selected = Selection.activeGameObject;
            if (selected == null) { Debug.LogError("[Level85ArtValidator] Select visual art root or layout parent."); return; }
            var art = selected.transform.name == "L85_HD_VISUAL_ART" ? selected.transform : selected.transform.Find("L85_HD_VISUAL_ART");
            if (art == null) { Debug.LogError("[Level85ArtValidator] Cannot find L85_HD_VISUAL_ART."); return; }
            var uniquePivots = new HashSet<string>(); var uniqueSockets = new HashSet<string>();
            int meshCount = 0, triCount = 0;
            foreach (var tr in art.GetComponentsInChildren<Transform>(true))
            {
                if (tr.name.StartsWith("ANIM_")) uniquePivots.Add(tr.name);
                if (tr.name.StartsWith("SOCKET_P")) uniqueSockets.Add(tr.name);
            }
            foreach (var meshFilter in art.GetComponentsInChildren<MeshFilter>(true))
            {
                if (meshFilter.sharedMesh == null) continue;
                meshCount++; triCount += meshFilter.sharedMesh.triangles.Length / 3;
            }
            var actualModels = new HashSet<string>();
            foreach (var tr in art.GetComponentsInChildren<Transform>(true))
                if (tr.name.StartsWith("L85_") && !tr.name.StartsWith("L85_HD_VISUAL") && !tr.name.StartsWith("L85_DYNAMIC"))
                    actualModels.Add(tr.name);
            int matchedModels = 0;
            foreach (string model in ExpectedNames)
                if (actualModels.Contains(model)) matchedModels++;
            bool pivOK = uniquePivots.Count == 60 && matchedModels == 78;
            bool sockOK = uniqueSockets.Count == 224;
            string message = "Imported Level85 art: distinct model roots=" + matchedModels + "/78, mesh filters=" + meshCount + ", rendered triangles=" + triCount +
                ", unique ANIM pivots=" + uniquePivots.Count + "/60, unique visual socket nodes=" + uniqueSockets.Count + "/224." +
                " This validates scene art only, not FBX scale, SparkTerminals, gameplay, or the objective system.";
            if (pivOK && sockOK) Debug.Log("[Level85ArtValidator] " + message);
            else Debug.LogWarning("[Level85ArtValidator] MISSING ART PIVOTS OR SOCKETS. " + message);
            EditorUtility.DisplayDialog("Level 85 Structural Art Check", message, "OK");
        }
    }
}
#endif
