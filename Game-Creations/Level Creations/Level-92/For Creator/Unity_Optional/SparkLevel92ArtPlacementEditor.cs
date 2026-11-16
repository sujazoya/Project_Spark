#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace ProjectSpark.EditorTools
{
    /// <summary>Imports visual modular Level 92 FBXs. Does not implement gameplay, SparkTerminals or simulation.</summary>
    public static class SparkLevel92ArtPlacementEditor
    {
        public const string Folder = "Assets/ProjectSpark/Level92/Models/";
        public const string VisualRoot = "L92_HD_VISUAL_ART";
        public static readonly string[] ModelNames = new string[]
        {
            "L92_AsteroidSamplingMissionDeck",
            "L92_PanoramicKhepri7ObservationWindow",
            "L92_CarbonaceousAsteroidSurface",
            "L92_Aurora1AsteroidOrbitStage",
            "L92_Khepri7GeologicalHologram",
            "L92_SamplingMissionControlConsole",
            "L92_ExteriorLanderDeploymentBay",
            "L92_AsteroidBoulderField",
            "L92_ZeroGravityScienceLabBase",
            "L92_SubsurfaceTomographyProjection",
            "L92_SamplingServiceGantry",
            "L92_ExternalRockCraterWall",
            "L92_LanderSurfaceContactArena",
            "L92_CoreTransferAccessBridge",
            "L92_MissionQualificationBeacon",
            "L92_AsteroidOperationsSafetyRail",
            "L92_SixLegMicrogravityLanderPlatform",
            "L92_LanderCentralAvionicsBody",
            "L92_ArticulatedLandingLegA",
            "L92_ArticulatedLandingLegB",
            "L92_ArticulatedLandingLegC",
            "L92_ArticulatedLandingLegD",
            "L92_ArticulatedLandingLegE",
            "L92_ArticulatedLandingLegF",
            "L92_AdaptiveMicrospineFootA",
            "L92_AdaptiveMicrospineFootB",
            "L92_AdaptiveMicrospineFootC",
            "L92_AdaptiveMicrospineFootD",
            "L92_AdaptiveMicrospineFootE",
            "L92_AdaptiveMicrospineFootF",
            "L92_AnchorLoadCellArray",
            "L92_AnchorLoadDistributionPLC",
            "L92_LanderAttitudeStabilizer",
            "L92_LanderLegCalibrationRig",
            "L92_RotaryPercussiveCoringDrill",
            "L92_DrillSpindleAssembly",
            "L92_TungstenCarbideCoringBit",
            "L92_AxialFeedBallscrew",
            "L92_DrillingMotorAndGearbox",
            "L92_PercussionActuator",
            "L92_BitThermalProbe",
            "L92_DrillTorqueTransducer",
            "L92_AxialForceLoadCell",
            "L92_DrillDepthEncoder",
            "L92_DrillVibrationAnalyzer",
            "L92_DrillMotorDrive",
            "L92_DrillFeedServoController",
            "L92_DrillToolGuideFrame",
            "L92_DrillCuttingsCollector",
            "L92_BitJamRecoveryActuator",
            "L92_BitWearInspectionCamera",
            "L92_CoreRetentionSleeve",
            "L92_DrillThermalManagementUnit",
            "L92_DrillCommissioningConsole",
            "L92_GroundPenetratingRadarA",
            "L92_GroundPenetratingRadarB",
            "L92_SurfaceLidarScanner",
            "L92_RegolithPorosityProbe",
            "L92_SeismicContactGeophoneA",
            "L92_SeismicContactGeophoneB",
            "L92_SubsurfaceLayerFusionComputer",
            "L92_GeologicalUncertaintyEstimator",
            "L92_RockCohesionTestProbe",
            "L92_DrillSitePlanner",
            "L92_TerrainStereoCameraA",
            "L92_TerrainStereoCameraB",
            "L92_RadarCalibrationReference",
            "L92_LayeredMaterialCrossSectionHologram",
            "L92_CoreSampleCaptureAssembly",
            "L92_SealedCoreCartridgeA",
            "L92_SealedCoreCartridgeB",
            "L92_SampleCarouselIndexer",
            "L92_CoreSealCompressionMechanism",
            "L92_CartridgeLeakIntegrityTester",
            "L92_CoreMassMetrology",
            "L92_CoreOpticalInspectionStation",
            "L92_SampleReturnTransferArm",
            "L92_TetherRetrievalWinch",
            "L92_TetherForceTransducer",
            "L92_LanderTetherCableGuide",
            "L92_SampleTransportContainer",
            "L92_VacuumSampleAirlock",
            "L92_SamplingSafetyPLC",
            "L92_Protected5VControlSupply",
            "L92_DrillingPowerDistributionCabinet",
            "L92_SamplingMissionSCADA",
            "L92_LanderSafetyInterlockStation",
            "L92_AnchorSlipFaultInjector",
            "L92_DrillBitStallFaultInjector",
            "L92_CartridgeSealFaultInjector",
            "L92_FaultDiagnosisAndRepairRouter",
            "L92_IndependentTetherEmergencyControl",
            "L92_W171CriticalAnchorFeedbackPatchStation",
            "L92_MechanicalSampleCertificationHologram",
            "L92_EStopStation",
            "L92_LanderEquipmentPowerCabinet",
            "L92_EnvironmentalProcessRecorder",
            "L92_MissionCompletionCertificateConsole",
        };
        [MenuItem("Tools/Project Spark/Levels/92/Place HD Art Under Selected Layout")]
        public static void Place()
        {
            GameObject parent = Selection.activeGameObject;
            if (parent == null)
            {
                EditorUtility.DisplayDialog("Level 92", "Select the Level 92 layout root GameObject in the Hierarchy first.", "OK");
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
                Debug.LogError("[Level92Art] Missing FBX: "+string.Join("; ",missing));
                EditorUtility.DisplayDialog("Level 92", "Import all 98 numbered FBX models first. Missing: "+missing.Count, "OK");
                return;
            }
            if (!EditorUtility.DisplayDialog("Level 92 Modular Art", 
                    "Replace ONLY the generated visual root '"+VisualRoot+"'? Gameplay objects outside this root remain intact.",
                    "Place Visual Art", "Cancel")) return;
            Transform old = parent.transform.Find(VisualRoot);
            if (old != null) Undo.DestroyObjectImmediate(old.gameObject);
            var newArt = new GameObject(VisualRoot);
            Undo.RegisterCreatedObjectUndo(newArt, "Create Level 92 visual root");
            Undo.SetTransformParent(newArt.transform,parent.transform,"Parent Level 92 visual root");
            newArt.transform.localPosition=Vector3.zero;
            newArt.transform.localRotation=Quaternion.identity;
            newArt.transform.localScale=Vector3.one;
            int placed=0;
            for (int i=0; i<ModelNames.Length; i++)
            {
                GameObject inst=PrefabUtility.InstantiatePrefab(source[i],newArt.transform) as GameObject;
                if(inst==null) { Debug.LogError("[Level92Art] Cannot instantiate "+ModelNames[i]);continue; }
                Undo.RegisterCreatedObjectUndo(inst,"Place Level 92 model");
                inst.name=ModelNames[i];
                inst.transform.localPosition=Vector3.zero;
                inst.transform.localRotation=Quaternion.identity;
                inst.transform.localScale=Vector3.one;
                placed++;
            }
            EditorUtility.SetDirty(parent);
            if (parent.scene.IsValid()) EditorSceneManager.MarkSceneDirty(parent.scene);
            Selection.activeGameObject=newArt;
            Debug.Log("[Level92Art] Placed "+placed+"/98 modular models. Check FBX centimetre-to-metre conversion EXACTLY ONCE. No gameplay generated.");
        }
    }
}
#endif
