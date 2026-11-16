#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace ProjectSpark.EditorTools
{
    /// <summary>Level 86 visual-art importer ONLY. Does not generate playable circuits,
    /// actual SparkTerminals, servo dynamics, objectives or thermal physics.</summary>
    public static class SparkLevel86ArtPlacementEditor
    {
        public const string Folder = "Assets/ProjectSpark/Level86/Models/";
        public const string VisualRoot = "L86_HD_VISUAL_ART";
        public static readonly string[] ModelNames = new string[]
        {
            "L86_OrbitalForgeArenaBase",
            "L86_PanoramicEarthMaintenanceViewport",
            "L86_StationExteriorServiceTruss",
            "L86_StationRobotServiceBayFrame",
            "L86_DockedCargoSpacecraft",
            "L86_CargoServiceBayHatch",
            "L86_RobotLinearTraverseRail",
            "L86_SevenAxisRobotBase",
            "L86_RobotJoint1BaseYaw",
            "L86_RobotJoint2Shoulder",
            "L86_RobotJoint3UpperArm",
            "L86_RobotJoint4Elbow",
            "L86_RobotJoint5Forearm",
            "L86_RobotJoint6WristPitch",
            "L86_RobotJoint7WristRoll",
            "L86_RobotCounterbalanceModule",
            "L86_JointServoDriveRack",
            "L86_JointEncoderFeedbackArray",
            "L86_JointTorqueCurrentSensorRack",
            "L86_RobotBrakeAndTorqueLimiter",
            "L86_SevenAxisReachEnvelopeHologram",
            "L86_RoboticHomingCalibrationFixture",
            "L86_StereoMachineVisionCameraLeft",
            "L86_StereoMachineVisionCameraRight",
            "L86_StructuredLightDepthScanner",
            "L86_OpticalFiducialTargetArray",
            "L86_PrecisionVisionCalibrationBoard",
            "L86_LaserAlignmentMetrologyUnit",
            "L86_SixAxisWristForceTorqueSensor",
            "L86_ForceTorqueCalibrationFixture",
            "L86_CompliantInsertionController",
            "L86_SixAxisContactForceHologram",
            "L86_RobotJointPathPlanningComputer",
            "L86_InverseKinematicsSolverDisplay",
            "L86_CollisionProximityScanner",
            "L86_RobotTrajectorySafetySupervisor",
            "L86_FlexiblePayloadModalSensor",
            "L86_ActiveVibrationDampingController",
            "L86_AdaptiveRadiatorHandlingGripper",
            "L86_AutomaticEndEffectorToolChanger",
            "L86_PrecisionTorqueFasteningDriver",
            "L86_CaptiveFastenerAndBoltRack",
            "L86_VacuumCompatibleInspectionProbe",
            "L86_RoboticToolDockAndCalibrationNest",
            "L86_DualJawGripperPressureSensorArray",
            "L86_RadiatorGrappleHandleFixture",
            "L86_FailedThermalRadiatorModule",
            "L86_ReplacementThermalRadiatorModule",
            "L86_RadiatorMountingSpaceTruss",
            "L86_RadiatorThermalInterfacePlate",
            "L86_CoolantQuickDisconnectA",
            "L86_CoolantQuickDisconnectB",
            "L86_CoolantValveManifold",
            "L86_CoolantPressureTransducer",
            "L86_CoolantFlowMeter",
            "L86_RadiatorTemperatureSensorArray",
            "L86_StationAvionicsHeatLoadEmulator",
            "L86_ThermalLoopPumpSimulator",
            "L86_RadiatorLeakTestController",
            "L86_CoolantIsolationAndVentStation",
            "L86_RadiatorFastenerActuatorDriver",
            "L86_StationThermalTrendHologram",
            "L86_RadiatorThermalPerformanceAnalyzer",
            "L86_FailedRadiatorDisposalCradle",
            "L86_PayloadVibrationFaultInjector",
            "L86_JointEncoderDriftFaultInjector",
            "L86_AsymmetricGripperFaultInjector",
            "L86_RoboticContactSafetyPLC",
            "L86_ManipulatorEmergencyStopStation",
            "L86_Protected5VControlSupply",
            "L86_RobotMotorPowerDistributionUnit",
            "L86_IndependentContactSafetyRelay",
            "L86_SevenAxisJointServoController",
            "L86_ContactForceSignalConditioner",
            "L86_StereoVisionFusionComputer",
            "L86_RadiatorServiceSequencingPLC",
            "L86_HolographicOrbitalRoboticsSCADA",
            "L86_W129CriticalFeedbackPatchStation",
            "L86_HardwareInterlockDiagnosticRack",
            "L86_ServicingMissionCertificationBeacon",
            "L86_ExternalInspectionCameraGantry",
            "L86_RoboticServicingEventRecorder",
        };

        [MenuItem("Tools/Project Spark/Levels/86/Place HD Art Under Selected Layout")]
        private static void PlaceArt()
        {
            GameObject parent = Selection.activeGameObject;
            if (parent == null)
            {
                EditorUtility.DisplayDialog("Level 86", "Select the existing Level 86 layout root GameObject in the Hierarchy first.", "OK");
                return;
            }
            var source = new GameObject[ModelNames.Length];
            var missing = new List<string>();
            for (int i = 0; i < source.Length; i++)
            {
                string path = Folder + i.ToString("00") + "_" + ModelNames[i] + ".fbx";
                source[i] = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (source[i] == null) missing.Add(path);
            }
            if (missing.Count > 0)
            {
                Debug.LogError("[Level86Art] Missing FBX: " + string.Join("; ", missing));
                EditorUtility.DisplayDialog("Level 86", "Missing " + missing.Count + " FBX files. Import all 82 numbered FBXs first.", "OK");
                return;
            }
            if (!EditorUtility.DisplayDialog("Level 86 Modular Art",
                "Replace ONLY the art root '" + VisualRoot + "' under selected layout? Actual gameplay children outside it remain intact.",
                "Place Art", "Cancel")) return;
            var oldRoot = parent.transform.Find(VisualRoot);
            if (oldRoot != null) Undo.DestroyObjectImmediate(oldRoot.gameObject);
            var visual = new GameObject(VisualRoot);
            Undo.RegisterCreatedObjectUndo(visual, "Create Level 86 art root");
            Undo.SetTransformParent(visual.transform, parent.transform, "Parent Level 86 art root");
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localRotation = Quaternion.identity;
            visual.transform.localScale = Vector3.one;
            int placed = 0;
            for (int i = 0; i < source.Length; i++)
            {
                GameObject inst = PrefabUtility.InstantiatePrefab(source[i], visual.transform) as GameObject;
                if (inst == null)
                {
                    Debug.LogError("[Level86Art] Could not instantiate " + ModelNames[i]);
                    continue;
                }
                Undo.RegisterCreatedObjectUndo(inst, "Place Level 86 modular model");
                inst.name = ModelNames[i];
                inst.transform.localPosition = Vector3.zero;
                inst.transform.localRotation = Quaternion.identity;
                inst.transform.localScale = Vector3.one;
                placed++;
            }
            EditorUtility.SetDirty(parent);
            if (parent.scene.IsValid()) EditorSceneManager.MarkSceneDirty(parent.scene);
            Debug.Log("[Level86Art] Placed " + placed + "/82 common-origin modular FBXs. No gameplay was generated. Check metres-scale bounds before playing.");
            Selection.activeGameObject = visual;
        }
    }
}
#endif
