#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace ProjectSpark.EditorTools
{
    /// <summary>Imports visual modular Level 91 FBXs. Does not implement gameplay, SparkTerminals or simulation.</summary>
    public static class SparkLevel91ArtPlacementEditor
    {
        public const string Folder = "Assets/ProjectSpark/Level91/Models/";
        public const string VisualRoot = "L91_HD_VISUAL_ART";
        public static readonly string[] ModelNames = new string[]
        {
            "L91_CosmicNavigationQualificationDeck",
            "L91_PanoramicDeepSpaceObservationDome",
            "L91_StellarSceneryStructuralFrame",
            "L91_Aurora1DeepSpaceSpacecraft",
            "L91_PulsarVectorHologram",
            "L91_FourPulsarNavigationProjectionStage",
            "L91_NavigationCommandWorkstation",
            "L91_XRayObservatoryServicePlatform",
            "L91_DeepSpaceStarfieldVault",
            "L91_PulsarPhaseReferenceDisplay",
            "L91_ExteriorInspectionRail",
            "L91_PhotonSafetyShieldEnclosure",
            "L91_ClockTimingLaboratoryBase",
            "L91_TimingInstrumentAccessBridge",
            "L91_DeepSpaceMissionCertificationArch",
            "L91_DeepSpaceMissionSafetyBarrier",
            "L91_XRayPulsarTelescopeA",
            "L91_XRayPulsarTelescopeB",
            "L91_XRayPulsarTelescopeC",
            "L91_XRayPulsarTelescopeD",
            "L91_TelescopeGimbalA",
            "L91_TelescopeGimbalB",
            "L91_TelescopeGimbalC",
            "L91_TelescopeGimbalD",
            "L91_XRayConcentratorOpticsA",
            "L91_XRayConcentratorOpticsB",
            "L91_XRayConcentratorOpticsC",
            "L91_XRayConcentratorOpticsD",
            "L91_XRayPhotonDetectorA",
            "L91_XRayPhotonDetectorB",
            "L91_XRayPhotonDetectorC",
            "L91_XRayPhotonDetectorD",
            "L91_DetectorSunshadeArray",
            "L91_TelescopeCalibrationTarget",
            "L91_PrimaryAtomicClockReference",
            "L91_RedundantAtomicClockReference",
            "L91_UltraStableOscillator",
            "L91_PrecisionTimingDistributionRack",
            "L91_PrimaryTimeTagger",
            "L91_SecondaryTimeTagger",
            "L91_PhotonEventBufferA",
            "L91_PhotonEventBufferB",
            "L91_PhotonEventBufferC",
            "L91_PhotonEventBufferD",
            "L91_ClockComparisonMonitor",
            "L91_ClockThermalRegulationUnit",
            "L91_ClockVibrationIsolationPlatform",
            "L91_ClockBiasEstimator",
            "L91_TimeReferenceSwitchingPanel",
            "L91_PhaseErrorOscilloscope",
            "L91_FrequencyCounterRack",
            "L91_TimingCalibrationPulseSource",
            "L91_EventProcessorA",
            "L91_EventProcessorB",
            "L91_EventProcessorC",
            "L91_EventProcessorD",
            "L91_EventCoincidenceFilter",
            "L91_BackgroundRejectionComputer",
            "L91_PulseProfileFoldingEngine",
            "L91_PulsarCatalogStorageArray",
            "L91_EphemerisIntegrityValidator",
            "L91_CelestialTimeBarycenterComputer",
            "L91_PulsarCorrelationProcessor",
            "L91_TimingResidualAnalyzer",
            "L91_NavigationMeasurementFusionUnit",
            "L91_NavigationCovarianceEstimator",
            "L91_PositionVelocityStateEstimator",
            "L91_FourPulsarGeometryAnalyzer",
            "L91_PulseAmbiguityResolver",
            "L91_NavigationUncertaintyHologram",
            "L91_PrecisionTelescopePointingPLC",
            "L91_GuidanceNavigationComputer",
            "L91_TrajectoryCorrectionPlanner",
            "L91_PropulsionCommandSupervisor",
            "L91_Aurora1ThrusterControlLink",
            "L91_AutonomousFlightSafetyPLC",
            "L91_NavigationIntegritySafetyPLC",
            "L91_FaultEventRecorder",
            "L91_NavigationSafetyInterlockStation",
            "L91_Protected5VControlSupply",
            "L91_NavigationSCADA",
            "L91_MissionNavigationCertificate",
            "L91_DetectorHighBackgroundFaultInjector",
            "L91_ClockThermalDriftFaultInjector",
            "L91_EphemerisMismatchFaultInjector",
            "L91_TelescopeGimbalJamTest",
            "L91_DetectorCoolingSystem",
            "L91_ObservatoryThermalRadiator",
            "L91_SpectralCalibrationReference",
            "L91_AttitudeMonitor",
            "L91_OpticalStarTrackerCrosscheck",
            "L91_DeepSpaceCommunicationsBlackoutSimulator",
            "L91_W163CriticalTimePatchStation",
            "L91_EStopStation",
            "L91_EquipmentPowerDistribution",
            "L91_MissionSummaryConsole",
        };
        [MenuItem("Tools/Project Spark/Levels/90/Place HD Art Under Selected Layout")]
        public static void Place()
        {
            GameObject parent = Selection.activeGameObject;
            if (parent == null)
            {
                EditorUtility.DisplayDialog("Level 91", "Select the Level 91 layout root GameObject in the Hierarchy first.", "OK");
                return;
            }
            var source = new GameObject[ModelNames.Length];
            var missing = new List<string>();
            for (int i=0; i<ModelNames.Length; i++)
            {
                string path = Folder+i.ToString("00")+"_"+ModelNames[i]+".fbx";
                source[i] = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (source[i] == null) missing.Add(path);
            }
            if (missing.Count > 0)
            {
                Debug.LogError("[Level91Art] Missing FBX: "+string.Join("; ",missing));
                EditorUtility.DisplayDialog("Level 91", "Import all 96 numbered FBX models first. Missing: "+missing.Count, "OK");
                return;
            }
            if (!EditorUtility.DisplayDialog("Level 91 Modular Art", 
                    "Replace ONLY the generated visual root '"+VisualRoot+"'? Gameplay objects outside this root remain intact.",
                    "Place Visual Art", "Cancel")) return;
            Transform old = parent.transform.Find(VisualRoot);
            if (old != null) Undo.DestroyObjectImmediate(old.gameObject);
            var newArt = new GameObject(VisualRoot);
            Undo.RegisterCreatedObjectUndo(newArt, "Create Level 91 visual root");
            Undo.SetTransformParent(newArt.transform,parent.transform,"Parent Level 91 visual root");
            newArt.transform.localPosition=Vector3.zero;
            newArt.transform.localRotation=Quaternion.identity;
            newArt.transform.localScale=Vector3.one;
            int placed=0;
            for (int i=0; i<ModelNames.Length; i++)
            {
                GameObject inst=PrefabUtility.InstantiatePrefab(source[i],newArt.transform) as GameObject;
                if(inst==null) { Debug.LogError("[Level91Art] Cannot instantiate "+ModelNames[i]);continue; }
                Undo.RegisterCreatedObjectUndo(inst,"Place Level 91 model");
                inst.name=ModelNames[i];
                inst.transform.localPosition=Vector3.zero;
                inst.transform.localRotation=Quaternion.identity;
                inst.transform.localScale=Vector3.one;
                placed++;
            }
            EditorUtility.SetDirty(parent);
            if (parent.scene.IsValid()) EditorSceneManager.MarkSceneDirty(parent.scene);
            Selection.activeGameObject=newArt;
            Debug.Log("[Level91Art] Placed "+placed+"/96 modular models. Check FBX centimetre-to-metre conversion EXACTLY ONCE. No gameplay generated.");
        }
    }
}
#endif
