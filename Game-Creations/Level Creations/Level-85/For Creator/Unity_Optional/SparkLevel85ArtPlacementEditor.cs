#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace ProjectSpark.EditorTools
{
    /// <summary>
    /// Optional Level 85 ART-ONLY importer. Does not create SparkTerminals, player wires,
    /// a circuit solver, spacecraft dynamics, six objectives, or a playable level.
    /// Import all numbered FBXs under Assets/ProjectSpark/Level85/Models first.
    /// </summary>
    public static class SparkLevel85ArtPlacementEditor
    {
        private const string Base = "Assets/ProjectSpark/Level85/Models/";
        private const string RootName = "L85_HD_VISUAL_ART";
        private const string MobileName = "L85_DYNAMIC_SERVICE_SPACECRAFT";
        private static readonly string[] Names = new string[]
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

        [MenuItem("Tools/Project Spark/Levels/85/Place HD Art Under Selected Layout")]
        public static void PlaceArt()
        {
            var selected = Selection.activeGameObject;
            if (selected == null)
            {
                EditorUtility.DisplayDialog("Level 85 Art", "Select an existing Level 85 scene/layout parent GameObject.", "OK");
                return;
            }
            if (!EditorUtility.DisplayDialog("Level 85 Modular Art", "Replace the visual-only L85_HD_VISUAL_ART child? Gameplay objects outside that child will not be touched.", "Place", "Cancel")) return;
            var prefabs = new GameObject[Names.Length];
            var missing = new List<string>();
            for (int i = 0; i < Names.Length; i++)
            {
                var path = Base + i.ToString("00") + "_" + Names[i] + ".fbx";
                prefabs[i] = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefabs[i] == null) missing.Add(path);
            }
            if (missing.Count != 0)
            {
                Debug.LogError("[Level85Art] Missing " + missing.Count + " FBX files. First missing: " + missing[0]);
                EditorUtility.DisplayDialog("Level 85 Art", "Missing " + missing.Count + " FBX models. Please import all 78 numbered models to the documented path before placing.", "OK");
                return;
            }
            var previous = selected.transform.Find(RootName);
            if (previous != null) Undo.DestroyObjectImmediate(previous.gameObject);
            var art = new GameObject(RootName);
            Undo.RegisterCreatedObjectUndo(art, "Create Level 85 visual art");
            art.transform.SetParent(selected.transform, false);
            art.transform.localPosition = Vector3.zero;
            art.transform.localRotation = Quaternion.identity;
            art.transform.localScale = Vector3.one;
            // Geometry in all modular FBXs was authored at common world origin, NOT centered per file.
            var rootList = new GameObject[Names.Length];
            for (int i = 0; i < prefabs.Length; i++)
            {
                var obj = PrefabUtility.InstantiatePrefab(prefabs[i]) as GameObject;
                if (obj == null) continue;
                Undo.RegisterCreatedObjectUndo(obj, "Place Level 85 modular model");
                obj.name = Names[i];
                obj.transform.SetParent(art.transform, false);
                obj.transform.localPosition = Vector3.zero;
                obj.transform.localRotation = Quaternion.identity;
                obj.transform.localScale = Vector3.one;
                rootList[i] = obj;
            }
            // One movable vehicle root preserves the original world-space art alignment.
            // Include ship (18-31) + hull-mounted navigation sensors (35-42), not bench racks (32-34).
            var mobile = new GameObject(MobileName);
            Undo.RegisterCreatedObjectUndo(mobile, "Group service spacecraft art");
            mobile.transform.SetParent(art.transform, false);
            mobile.transform.localPosition = new Vector3(-4.8f, 5.05f, -1.45f);
            mobile.transform.localRotation = Quaternion.identity;
            for (int i = 18; i <= 42; i++)
            {
                if (i >= 32 && i <= 34) continue;
                if (rootList[i] != null) rootList[i].transform.SetParent(mobile.transform, true);
            }
            Selection.activeGameObject = art;
            EditorUtility.SetDirty(selected);
            Debug.Log("[Level85Art] Placed 78 origin-aligned modular FBX models, grouped the movable service spacecraft. Art-only: no gameplay implemented.");
        }
    }
}
#endif
