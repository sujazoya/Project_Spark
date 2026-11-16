#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace ProjectSpark.EditorTools
{
    /// <summary>Level 87 visual-art importer ONLY. Does not generate playable circuits,
    /// actual SparkTerminals, bearing physics, ring dynamics or gameplay objectives.</summary>
    public static class SparkLevel87ArtPlacementEditor
    {
        public const string Folder = "Assets/ProjectSpark/Level87/Models/";
        public const string VisualRoot = "L87_HD_VISUAL_ART";
        public static readonly string[] ModelNames = new string[]
        {
            "L87_GravityQualificationArenaBase",
            "L87_PanoramicOrbitalObservationViewport",
            "L87_RotatingHabitatRingMainRotor",
            "L87_RotatingHabitatSpokeAssembly",
            "L87_StationaryGravityHub",
            "L87_HabitatSegmentA",
            "L87_HabitatSegmentB",
            "L87_HabitatSegmentC",
            "L87_HabitatSegmentD",
            "L87_HabitatSegmentE",
            "L87_HabitatSegmentF",
            "L87_HabitatPressureShellRing",
            "L87_GravityRingStructuralGuideTrack",
            "L87_GravityStationFixedSupportFrame",
            "L87_GravityStationIsolationTruss",
            "L87_HabitatObservationDeck",
            "L87_MagneticBearingStationA",
            "L87_MagneticBearingStationB",
            "L87_MagneticBearingStationC",
            "L87_MagneticBearingStationD",
            "L87_MagneticBearingStationE",
            "L87_MagneticBearingStationF",
            "L87_MagneticBearingStationG",
            "L87_MagneticBearingStationH",
            "L87_PrimaryRadialGapSensor",
            "L87_SecondaryRadialGapSensor",
            "L87_AxialGapDisplacementSensor",
            "L87_MagneticBearingCoilCurrentMonitor",
            "L87_MagneticFluxVisualizationHologram",
            "L87_EmergencyTouchdownBearing",
            "L87_BearingPowerAmplifierCabinet",
            "L87_BearingGapCalibrationStage",
            "L87_SegmentedRimMotorStator",
            "L87_RimDriveRotorMagnetArray",
            "L87_MotorInverterDriveA",
            "L87_MotorInverterDriveB",
            "L87_MotorPhaseCurrentSensor",
            "L87_MotorTorqueEstimator",
            "L87_RotorSpeedOpticalEncoder",
            "L87_SafeSpinUpSequencer",
            "L87_MotorPrechargeContactor",
            "L87_DriveThermalCoolingManifold",
            "L87_MotorAccelerationProfileConsole",
            "L87_RingAngularMomentumCompensationUnit",
            "L87_MultiAxisVibrationSensorArray",
            "L87_VibrationFrequencySpectrumAnalyzer",
            "L87_RotorShaftOrbitProbe",
            "L87_RotorImbalanceVectorDisplay",
            "L87_AdaptiveVibrationDampingController",
            "L87_PrecisionBalanceWeightActuatorA",
            "L87_PrecisionBalanceWeightActuatorB",
            "L87_RotorMassDistributionScanner",
            "L87_ResonanceSweepTestFixture",
            "L87_BearingHousingTemperatureSensor",
            "L87_DynamicRotorHealthRecorder",
            "L87_PrecisionGapAndVibrationSCADA",
            "L87_VibrationFaultCalibrationInjector",
            "L87_ContactlessRotaryPowerCouplerPrimary",
            "L87_ContactlessRotaryPowerCouplerBackup",
            "L87_RotaryOpticalTelemetryLink",
            "L87_RotaryPowerIsolationContactor",
            "L87_HabitatElectricalLoadEmulator",
            "L87_HabitatEmergencyLightingLoad",
            "L87_HabitatThermalControlLoad",
            "L87_RotatingDataQualityMonitor",
            "L87_RotaryPowerTransferMonitor",
            "L87_HabitatLoadDistributionSimulator",
            "L87_GravityHabitatEnvironmentHologram",
            "L87_OrbitalRingPowerBudgetAnalyzer",
            "L87_GravitySafetySupervisoryPLC",
            "L87_MagneticBearingSafetyPLC",
            "L87_IndependentOverspeedProtector",
            "L87_RingEmergencyStopStation",
            "L87_Protected5VControlSupply",
            "L87_MagneticBearingControlPLC",
            "L87_RotorMotorControlPLC",
            "L87_HabitatPowerDistributionPLC",
            "L87_RadialSensorDriftFaultInjector",
            "L87_BearingAmplifierHeatFaultInjector",
            "L87_MassImbalanceFaultInjector",
            "L87_W137CriticalFeedbackPatchStation",
            "L87_GravityEngineeringMasterSCADA",
            "L87_RingCertificationBeacon",
            "L87_GravityMachineEventRecorder",
            "L87_GravityEngineInspectionCamera",
            "L87_StationSafetyIsolationRelay",
        };

        [MenuItem("Tools/Project Spark/Levels/87/Place HD Art Under Selected Layout")]
        private static void PlaceArt()
        {
            GameObject parent = Selection.activeGameObject;
            if (parent == null)
            {
                EditorUtility.DisplayDialog("Level 87", "Select the existing Level 87 layout root GameObject in the Hierarchy first.", "OK");
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
                Debug.LogError("[Level87Art] Missing FBX: " + string.Join("; ", missing));
                EditorUtility.DisplayDialog("Level 87", "Missing " + missing.Count + " FBX files. Import all 86 numbered FBXs first.", "OK");
                return;
            }
            if (!EditorUtility.DisplayDialog("Level 87 Modular Art",
                "Replace ONLY the art root '" + VisualRoot + "' under selected layout? Actual gameplay children outside it remain intact.",
                "Place Art", "Cancel")) return;
            var oldRoot = parent.transform.Find(VisualRoot);
            if (oldRoot != null) Undo.DestroyObjectImmediate(oldRoot.gameObject);
            var visual = new GameObject(VisualRoot);
            Undo.RegisterCreatedObjectUndo(visual, "Create Level 87 art root");
            Undo.SetTransformParent(visual.transform, parent.transform, "Parent Level 87 art root");
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localRotation = Quaternion.identity;
            visual.transform.localScale = Vector3.one;
            int placed = 0;
            for (int i = 0; i < source.Length; i++)
            {
                GameObject inst = PrefabUtility.InstantiatePrefab(source[i], visual.transform) as GameObject;
                if (inst == null)
                {
                    Debug.LogError("[Level87Art] Could not instantiate " + ModelNames[i]);
                    continue;
                }
                Undo.RegisterCreatedObjectUndo(inst, "Place Level 87 modular model");
                inst.name = ModelNames[i];
                inst.transform.localPosition = Vector3.zero;
                inst.transform.localRotation = Quaternion.identity;
                inst.transform.localScale = Vector3.one;
                placed++;
            }
            EditorUtility.SetDirty(parent);
            if (parent.scene.IsValid()) EditorSceneManager.MarkSceneDirty(parent.scene);
            Debug.Log("[Level87Art] Placed " + placed + "/86 common-origin modular FBXs. No gameplay was generated. Check metres-scale bounds before playing.");
            Selection.activeGameObject = visual;
        }
    }
}
#endif
