#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace ProjectSpark.EditorTools
{
    /// <summary>Imports visual modular Level 90 FBXs. Does not implement gameplay, SparkTerminals or simulation.</summary>
    public static class SparkLevel90ArtPlacementEditor
    {
        public const string Folder = "Assets/ProjectSpark/Level90/Models/";
        public const string VisualRoot = "L90_HD_VISUAL_ART";
        public static readonly string[] ModelNames = new string[]
        {
            "L90_OrbitalPropulsionQualificationDeck",
            "L90_EarthPanoramicObservationDome",
            "L90_ExteriorOrbitalStationTruss",
            "L90_Aurora1VehicleIntegrationStand",
            "L90_DeepSpaceDepartureHologram",
            "L90_ThrusterPlasmaTestChamber",
            "L90_PropulsionBayThermalContainment",
            "L90_InspectionServiceGantry",
            "L90_PropulsionControlWalkway",
            "L90_MissionControlHologramWall",
            "L90_SpacecraftDockingReleaseFixture",
            "L90_ProtectedVentAndInterlockGate",
            "L90_PropulsionBayRailArray",
            "L90_OrbitalLightingArc",
            "L90_EarthMoonTransferDisplay",
            "L90_FinalQualificationBeacon",
            "L90_Aurora1MainFuselage",
            "L90_Aurora1ForwardAvionicsNose",
            "L90_Aurora1PayloadServiceRing",
            "L90_Aurora1PortSolarArray",
            "L90_Aurora1StarboardSolarArray",
            "L90_Aurora1PortArrayHinge",
            "L90_Aurora1StarboardArrayHinge",
            "L90_Aurora1SolarArrayTrackingController",
            "L90_Aurora1ThermalRadiatorPort",
            "L90_Aurora1ThermalRadiatorStarboard",
            "L90_Aurora1PropellantTankSupportFrame",
            "L90_Aurora1ThrusterMountingTruss",
            "L90_Aurora1AttitudeAndNavigationSuite",
            "L90_Aurora1ServiceComputer",
            "L90_Aurora1FlightSafetyComputer",
            "L90_Aurora1PropulsionHarnessTrays",
            "L90_HallThrusterA",
            "L90_HallThrusterB",
            "L90_HallThrusterC",
            "L90_HallThrusterD",
            "L90_AnnularDischargeChamberCutaway",
            "L90_XenonAnodeDistributor",
            "L90_MagneticCoilAssemblyA",
            "L90_MagneticCoilAssemblyB",
            "L90_MagneticCoilAssemblyC",
            "L90_MagneticCoilAssemblyD",
            "L90_HollowCathodeNeutralizerA",
            "L90_HollowCathodeNeutralizerB",
            "L90_HollowCathodeNeutralizerC",
            "L90_HollowCathodeNeutralizerD",
            "L90_ThrusterGimbalActuatorBank",
            "L90_ThrustVectorCalibrationFixture",
            "L90_XenonStorageTankA",
            "L90_XenonStorageTankB",
            "L90_XenonHighPressureFillInterface",
            "L90_XenonFeedIsolationValveA",
            "L90_XenonFeedIsolationValveB",
            "L90_XenonPressureRegulatorA",
            "L90_XenonPressureRegulatorB",
            "L90_XenonFourWayDistributionManifold",
            "L90_XenonFlowControllerA",
            "L90_XenonFlowControllerB",
            "L90_XenonFlowControllerC",
            "L90_XenonFlowControllerD",
            "L90_XenonMassFlowSensorArray",
            "L90_XenonPropellantInventoryAnalyzer",
            "L90_PPUChannelA",
            "L90_PPUChannelB",
            "L90_PPUChannelC",
            "L90_PPUChannelD",
            "L90_ProtectedPropulsionDCBus",
            "L90_PPUPrechargeContactor",
            "L90_PPUCurrentShuntMonitor",
            "L90_PPUVoltageTransducer",
            "L90_MagnetCurrentDriverRack",
            "L90_CathodeKeeperPowerModule",
            "L90_PlasmaIgnitionSequencerPLC",
            "L90_NeutralizerCurrentTransducer",
            "L90_PropulsionSafetyPLC",
            "L90_Regulated5VControlSupply",
            "L90_ThermalControlPumpRack",
            "L90_ThrusterHeatRejectionRadiator",
            "L90_OpticalPlumeSpectrometer",
            "L90_FaradayPlumeDiagnosticProbe",
            "L90_HallDischargeOscillationRecorder",
            "L90_ThrusterThrustCalibrationBalance",
            "L90_QuadEngineThrustAllocationComputer",
            "L90_DeepSpaceTrajectoryGuidanceComputer",
            "L90_AutonomousDepartureSequencingPLC",
            "L90_StarTrackerTrajectoryMonitor",
            "L90_GimbalAlignmentFeedbackRack",
            "L90_XenonFlowRestrictionFaultInjector",
            "L90_MagnetDriverOvertemperatureFaultInjector",
            "L90_ThrustVectorMisalignmentFaultInjector",
            "L90_MissionPropulsionSCADA",
            "L90_PlasmaFieldVectorHologram",
            "L90_W157CriticalNeutralizerPatchStation",
            "L90_DeepSpaceMissionEventRecorder",
        };
        [MenuItem("Tools/Project Spark/Levels/90/Place HD Art Under Selected Layout")]
        public static void Place()
        {
            GameObject parent = Selection.activeGameObject;
            if (parent == null)
            {
                EditorUtility.DisplayDialog("Level 90", "Select the Level 90 layout root GameObject in the Hierarchy first.", "OK");
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
                Debug.LogError("[Level90Art] Missing FBX: "+string.Join("; ",missing));
                EditorUtility.DisplayDialog("Level 90", "Import all 94 numbered FBX models first. Missing: "+missing.Count, "OK");
                return;
            }
            if (!EditorUtility.DisplayDialog("Level 90 Modular Art", 
                    "Replace ONLY the generated visual root '"+VisualRoot+"'? Gameplay objects outside this root remain intact.",
                    "Place Visual Art", "Cancel")) return;
            Transform old = parent.transform.Find(VisualRoot);
            if (old != null) Undo.DestroyObjectImmediate(old.gameObject);
            var newArt = new GameObject(VisualRoot);
            Undo.RegisterCreatedObjectUndo(newArt, "Create Level 90 visual root");
            Undo.SetTransformParent(newArt.transform,parent.transform,"Parent Level 90 visual root");
            newArt.transform.localPosition=Vector3.zero;
            newArt.transform.localRotation=Quaternion.identity;
            newArt.transform.localScale=Vector3.one;
            int placed=0;
            for (int i=0; i<ModelNames.Length; i++)
            {
                GameObject inst=PrefabUtility.InstantiatePrefab(source[i],newArt.transform) as GameObject;
                if(inst==null) { Debug.LogError("[Level90Art] Cannot instantiate "+ModelNames[i]);continue; }
                Undo.RegisterCreatedObjectUndo(inst,"Place Level 90 model");
                inst.name=ModelNames[i];
                inst.transform.localPosition=Vector3.zero;
                inst.transform.localRotation=Quaternion.identity;
                inst.transform.localScale=Vector3.one;
                placed++;
            }
            EditorUtility.SetDirty(parent);
            if (parent.scene.IsValid()) EditorSceneManager.MarkSceneDirty(parent.scene);
            Selection.activeGameObject=newArt;
            Debug.Log("[Level90Art] Placed "+placed+"/94 modular models. Check FBX centimetre-to-metre conversion EXACTLY ONCE. No gameplay generated.");
        }
    }
}
#endif
