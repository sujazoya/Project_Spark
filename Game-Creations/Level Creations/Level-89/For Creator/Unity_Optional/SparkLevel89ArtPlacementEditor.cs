#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace ProjectSpark.EditorTools
{
    /// <summary>Imports visual modular Level 89 FBXs. Does not implement gameplay, SparkTerminals or simulation.</summary>
    public static class SparkLevel89ArtPlacementEditor
    {
        public const string Folder = "Assets/ProjectSpark/Level89/Models/";
        public const string VisualRoot = "L89_HD_VISUAL_ART";
        public static readonly string[] ModelNames = new string[]
        {
            "L89_HeliosRadiationDefenseArenaBase",
            "L89_SolarStormPanoramicObservatory",
            "L89_OrbitalStationHabitatExterior",
            "L89_StormShelterPressureHull",
            "L89_ShelterEntranceShieldedBulkhead",
            "L89_SolarWeatherHologramDome",
            "L89_EarthAndHabitatObservationWindow",
            "L89_StormShelterInteriorSupportDeck",
            "L89_WaterWallStructuralContainmentFrame",
            "L89_ShelterEmergencyEgressHatch",
            "L89_OuterExposureInstrumentPlatform",
            "L89_StationShieldWaterUtilitySkid",
            "L89_SolarEnergeticParticleTelescopeA",
            "L89_SolarEnergeticParticleTelescopeB",
            "L89_ChargedParticleSpectrometer",
            "L89_ProtonEnergyBinAnalyzer",
            "L89_HeavyIonChannelDetector",
            "L89_SolarVectorTrackingGimbal",
            "L89_ParticleCollimatorHead",
            "L89_PrimaryParticleCounter",
            "L89_SecondaryParticleCounter",
            "L89_DetectorPulseShaper",
            "L89_SpectralCalibrationSource",
            "L89_ParticleDetectorThermalShroud",
            "L89_SpaceWeatherForecastConsole",
            "L89_StormArrivalPredictionHologram",
            "L89_ExternalDosimeterArray",
            "L89_HabitatDosimeterArray",
            "L89_ShelterReferenceDosimeter",
            "L89_IndependentShelterSafetyDosimeter",
            "L89_TriaxialRadiationDoseProbe",
            "L89_DoseRateTransientRecorder",
            "L89_DosimeterCalibrationFrame",
            "L89_DetectorSaturationFaultInjector",
            "L89_DoseAccumulationAnalyzer",
            "L89_DistributedDetectorNetworkGateway",
            "L89_RadiationHeatmapHologram",
            "L89_DetectorRedundancyComparator",
            "L89_ShelterDoseLimitEvaluator",
            "L89_WaterWallTankPortA",
            "L89_WaterWallTankPortB",
            "L89_WaterWallTankStarboardA",
            "L89_WaterWallTankStarboardB",
            "L89_WaterReservoirMain",
            "L89_WaterTankLevelSensorArray",
            "L89_WaterTransferPumpA",
            "L89_WaterTransferPumpB",
            "L89_WaterDistributionValveManifold",
            "L89_WaterMassAllocationController",
            "L89_ShieldArealDensityAnalyzer",
            "L89_ThermalWaterResourceGuard",
            "L89_WaterWallLeakMonitor",
            "L89_ShieldPanelTop",
            "L89_ShieldPanelBottom",
            "L89_ShieldPanelPort",
            "L89_ShieldPanelStarboard",
            "L89_ShieldCassetteRailArray",
            "L89_ServoShieldActuatorA",
            "L89_ServoShieldActuatorB",
            "L89_ServoShieldActuatorC",
            "L89_ServoShieldActuatorD",
            "L89_ShieldLockingPinArray",
            "L89_ShieldPositionEncoderRack",
            "L89_ShieldMotorDriveCabinet",
            "L89_ShieldJammingFaultInjector",
            "L89_ShelterDoorActualInterlock",
            "L89_ShelterRadiationShieldQualityMonitor",
            "L89_ShieldCoverageDigitalTwin",
            "L89_ShelterEnvironmentalAirSupply",
            "L89_ShelterOxygenBufferInterface",
            "L89_ShelterThermalAirReclaimer",
            "L89_ShelterProtectedLifeSupportPLC",
            "L89_ShelterReservePowerDistribution",
            "L89_WaterPowerPriorityScheduler",
            "L89_HabitatCrewShelterOccupancyEmulator",
            "L89_StormIntensityProfileGenerator",
            "L89_HighEnergySurgeFaultInjector",
            "L89_ShieldingScenarioSafetyController",
            "L89_RadiationEmergencyIsolationStation",
            "L89_Protected5VControlSupply",
            "L89_HeliosSafetyPLC",
            "L89_HeliosMissionSequencingPLC",
            "L89_RadiationDefenseSCADA",
            "L89_SolarStorm3DParticleHologram",
            "L89_ExteriorParticleFieldEmitter",
            "L89_ShelterRadiationContingencyConsole",
            "L89_W149TrustedDoseFeedbackPatchStation",
            "L89_RadiationScenarioDiagnosticRecorder",
            "L89_StormShelterCertificationBeacon",
            "L89_EmergencyShelterCommunications",
            "L89_EquipmentMaintenanceInspectionCamera",
            "L89_SolarStormSurveyAntennaArray",
        };
        [MenuItem("Tools/Project Spark/Levels/89/Place HD Art Under Selected Layout")]
        public static void Place()
        {
            GameObject parent = Selection.activeGameObject;
            if (parent == null)
            {
                EditorUtility.DisplayDialog("Level 89", "Select the Level 89 layout root GameObject in the Hierarchy first.", "OK");
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
                Debug.LogError("[Level89Art] Missing FBX: "+string.Join("; ",missing));
                EditorUtility.DisplayDialog("Level 89", "Import all 92 numbered FBX models first. Missing: "+missing.Count, "OK");
                return;
            }
            if (!EditorUtility.DisplayDialog("Level 89 Modular Art", 
                    "Replace ONLY the generated visual root '"+VisualRoot+"'? Gameplay objects outside this root remain intact.",
                    "Place Visual Art", "Cancel")) return;
            Transform old = parent.transform.Find(VisualRoot);
            if (old != null) Undo.DestroyObjectImmediate(old.gameObject);
            var newArt = new GameObject(VisualRoot);
            Undo.RegisterCreatedObjectUndo(newArt, "Create Level 89 visual root");
            Undo.SetTransformParent(newArt.transform,parent.transform,"Parent Level 89 visual root");
            newArt.transform.localPosition=Vector3.zero;
            newArt.transform.localRotation=Quaternion.identity;
            newArt.transform.localScale=Vector3.one;
            int placed=0;
            for (int i=0; i<ModelNames.Length; i++)
            {
                GameObject inst=PrefabUtility.InstantiatePrefab(source[i],newArt.transform) as GameObject;
                if(inst==null) { Debug.LogError("[Level89Art] Cannot instantiate "+ModelNames[i]);continue; }
                Undo.RegisterCreatedObjectUndo(inst,"Place Level 89 model");
                inst.name=ModelNames[i];
                inst.transform.localPosition=Vector3.zero;
                inst.transform.localRotation=Quaternion.identity;
                inst.transform.localScale=Vector3.one;
                placed++;
            }
            EditorUtility.SetDirty(parent);
            if (parent.scene.IsValid()) EditorSceneManager.MarkSceneDirty(parent.scene);
            Selection.activeGameObject=newArt;
            Debug.Log("[Level89Art] Placed "+placed+"/92 modular models. Check FBX centimetre-to-metre conversion EXACTLY ONCE. No gameplay generated.");
        }
    }
}
#endif
