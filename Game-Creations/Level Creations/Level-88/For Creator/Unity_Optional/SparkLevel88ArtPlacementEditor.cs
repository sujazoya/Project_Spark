#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace ProjectSpark.EditorTools
{
    /// <summary>Imports visual modular Level 88 FBXs. Does not implement gameplay, SparkTerminals or simulation.</summary>
    public static class SparkLevel88ArtPlacementEditor
    {
        public const string Folder = "Assets/ProjectSpark/Level88/Models/";
        public const string VisualRoot = "L88_HD_VISUAL_ART";
        public static readonly string[] ModelNames = new string[]
        {
            "L88_HabitatAtmosphereQualificationArenaBase",
            "L88_PanoramicOrbitalGardenViewport",
            "L88_HabitatRingGlassBioDome",
            "L88_HabitatSectorA_Crew",
            "L88_HabitatSectorB_Research",
            "L88_HabitatSectorC_Garden",
            "L88_HabitatSectorD_Habitat",
            "L88_FourCompartmentAtmosphereDigitalTwin",
            "L88_PressurizedHabitatServiceBulkhead",
            "L88_AtmosphereAirlockIsolationHatch",
            "L88_LifeSupportEquipmentServiceWalkway",
            "L88_ProcessGasOverheadPipeNetwork",
            "L88_DualAdsorberTowerA",
            "L88_DualAdsorberTowerB",
            "L88_AdsorberAFlowDistributor",
            "L88_AdsorberBFlowDistributor",
            "L88_AdsorbentCartridgeArrayA",
            "L88_AdsorbentCartridgeArrayB",
            "L88_AdsorberRegenerationHeaterA",
            "L88_AdsorberRegenerationHeaterB",
            "L88_AdsorberCycleSelectorValveManifold",
            "L88_AdsorberInletCO2Analyzer",
            "L88_AdsorberOutletCO2Analyzer",
            "L88_AdsorptionCapacityMonitor",
            "L88_RegenerationExhaustCondenser",
            "L88_CO2StorageIsolationCanister",
            "L88_CO2BreakthroughFaultInjector",
            "L88_CO2RegenerationControllerPLC",
            "L88_O2WaterElectrolysisChamber",
            "L88_O2CellStackA",
            "L88_O2CellStackB",
            "L88_OxygenGasSeparator",
            "L88_HydrogenByproductIsolationModule",
            "L88_OxygenBufferStorageTank",
            "L88_OxygenOutputFlowController",
            "L88_OxygenPartialPressureAnalyzer",
            "L88_OxygenOutputQualityMonitor",
            "L88_O2GenerationPowerConverter",
            "L88_OxygenGeneratorThermalCoolingLoop",
            "L88_WaterFeedPumpToElectrolyzer",
            "L88_OxygenDeliveryRegulatorManifold",
            "L88_OxygenEmergencyBackupReserve",
            "L88_ElectrolyzerCellMonitoringConsole",
            "L88_OxygenGenerationProcessPLC",
            "L88_HumidityCondensingHeatExchanger",
            "L88_CoolingCoilSerpentineAssembly",
            "L88_CondensateWaterSeparator",
            "L88_CondensateCollectionReservoir",
            "L88_ReclaimedWaterPump",
            "L88_WaterQualityMonitoringStation",
            "L88_FrozenCondenserFaultInjector",
            "L88_HumidityRecoveryPLC",
            "L88_VaporHumidityProbeArray",
            "L88_CoolingValveActuator",
            "L88_RecoveredWaterFiltrationRack",
            "L88_WaterResourceBudgetAnalyzer",
            "L88_VentilationMainCirculationBlower",
            "L88_VentilationSecondaryCirculationBlower",
            "L88_VariableFrequencyFanDriveA",
            "L88_VariableFrequencyFanDriveB",
            "L88_PrimaryAirDuctDistributionManifold",
            "L88_FourZoneMotorizedDamperBank",
            "L88_HabitatA_ZonePressureSensor",
            "L88_HabitatB_ZonePressureSensor",
            "L88_HabitatC_ZonePressureSensor",
            "L88_HabitatD_ZonePressureSensor",
            "L88_CabinPressurePrimaryTransducer",
            "L88_IndependentCabinPressureSensor",
            "L88_DifferentialPressureMonitorArray",
            "L88_CabinLeakIsolationManifold",
            "L88_CabinPressureRegulator",
            "L88_HabitatMicroLeakFaultInjector",
            "L88_TraceContaminantFilterArray",
            "L88_AirFlowVelocityMeasurementRack",
            "L88_GasMixingAndSamplingStation",
            "L88_HabitatPressureVesselSafetyRelief",
            "L88_AirQualityEnvironmentalPLC",
            "L88_HabitatLifeSupportSafetyPLC",
            "L88_Protected5VControlSupply",
            "L88_OxygenCO2TelemetryGateway",
            "L88_ECLSSMasterPLC",
            "L88_WaterAndEnergySafetyInterlock",
            "L88_HabitatEnvironmentalSCADA",
            "L88_AtmosphericDigitalFlowHologram",
            "L88_AirSystemDiagnosticsRecorder",
            "L88_MissionOccupancyDemandSimulator",
            "L88_W145CriticalPressureFeedbackPatchStation",
            "L88_EmergencyAtmosphereIsolationConsole",
            "L88_AtmosphereMissionCertificationBeacon",
            "L88_EnvironmentalInspectionCamera",
        };
        [MenuItem("Tools/Project Spark/Levels/88/Place HD Art Under Selected Layout")]
        public static void Place()
        {
            GameObject parent = Selection.activeGameObject;
            if (parent == null)
            {
                EditorUtility.DisplayDialog("Level 88", "Select the Level 88 layout root GameObject in the Hierarchy first.", "OK");
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
                Debug.LogError("[Level88Art] Missing FBX: "+string.Join("; ",missing));
                EditorUtility.DisplayDialog("Level 88", "Import all 90 numbered FBX models first. Missing: "+missing.Count, "OK");
                return;
            }
            if (!EditorUtility.DisplayDialog("Level 88 Modular Art", 
                    "Replace ONLY the generated visual root '"+VisualRoot+"'? Gameplay objects outside this root remain intact.",
                    "Place Visual Art", "Cancel")) return;
            Transform old = parent.transform.Find(VisualRoot);
            if (old != null) Undo.DestroyObjectImmediate(old.gameObject);
            var newArt = new GameObject(VisualRoot);
            Undo.RegisterCreatedObjectUndo(newArt, "Create Level 88 visual root");
            Undo.SetTransformParent(newArt.transform,parent.transform,"Parent Level 88 visual root");
            newArt.transform.localPosition=Vector3.zero;
            newArt.transform.localRotation=Quaternion.identity;
            newArt.transform.localScale=Vector3.one;
            int placed=0;
            for (int i=0; i<ModelNames.Length; i++)
            {
                GameObject inst=PrefabUtility.InstantiatePrefab(source[i],newArt.transform) as GameObject;
                if(inst==null) { Debug.LogError("[Level88Art] Cannot instantiate "+ModelNames[i]);continue; }
                Undo.RegisterCreatedObjectUndo(inst,"Place Level 88 model");
                inst.name=ModelNames[i];
                inst.transform.localPosition=Vector3.zero;
                inst.transform.localRotation=Quaternion.identity;
                inst.transform.localScale=Vector3.one;
                placed++;
            }
            EditorUtility.SetDirty(parent);
            if (parent.scene.IsValid()) EditorSceneManager.MarkSceneDirty(parent.scene);
            Selection.activeGameObject=newArt;
            Debug.Log("[Level88Art] Placed "+placed+"/90 modular models. Check FBX centimetre-to-metre conversion EXACTLY ONCE. No gameplay generated.");
        }
    }
}
#endif
