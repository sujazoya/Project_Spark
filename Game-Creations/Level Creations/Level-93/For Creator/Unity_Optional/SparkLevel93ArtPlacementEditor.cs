#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
namespace ProjectSpark.Level93.EditorTools
{
    /// <summary>Art only. Keeps visual FBX instances isolated; never changes gameplay/electrical objects.</summary>
    public static class SparkLevel93ArtPlacementEditor
    {
        public const string ArtRootName = "L93_HD_VISUAL_ART";
        public static readonly string[] ModelNames = new string[]
        {
            "L93_Aurora1ScienceLaboratoryDeck",
            "L93_PanoramicObservationWindow",
            "L93_TitaniumWallModuleLeft",
            "L93_TitaniumWallModuleRight",
            "L93_CeilingLabLightingArray",
            "L93_LabSafetyRailSet",
            "L93_CentralArchiveDais",
            "L93_PristineSampleVaultOuterShell",
            "L93_PristineSampleVaultInspectionGlass",
            "L93_Khepri7AsteroidCoreArchive",
            "L93_SampleCoreDisplayCradle",
            "L93_ElementalHolographicArchive",
            "L93_AsteroidCompositionWallDisplay",
            "L93_CleanRoomAirlock",
            "L93_ScienceMissionControlDesk",
            "L93_ContaminationFreeFloorGrid",
            "L93_LaboratoryUtilityBridge",
            "L93_ArchiveQualificationBeacon",
            "L93_XRayCTMainGantry",
            "L93_XRayCTRotatingBore",
            "L93_CTSourceShieldedHousing",
            "L93_CTBeamCollimator",
            "L93_CTDetectorArc",
            "L93_CTPrecisionRotationStage",
            "L93_CTStageServoDrive",
            "L93_CTAngleEncoder",
            "L93_CTSampleCarrier",
            "L93_CTGantryCoolingManifold",
            "L93_CTReconstructionComputer",
            "L93_CTGeometryCalibrationPhantom",
            "L93_CTInterlockDoor",
            "L93_CTPowerIsolationPanel",
            "L93_CTReferenceFiducialSet",
            "L93_CTProjectionHologram",
            "L93_CTDiagnosticRack",
            "L93_RamanMicroscopeBody",
            "L93_RamanLaserHead",
            "L93_RamanOpticalTable",
            "L93_RamanWavelengthCalibrator",
            "L93_RamanSpectralDetector",
            "L93_InfraredSpectrometer",
            "L93_IRInterferometer",
            "L93_IRSampleStage",
            "L93_IRDetectorCoolingHead",
            "L93_XRFEnergyAnalyzer",
            "L93_XRFShieldedSourceModule",
            "L93_XRFDetectorHead",
            "L93_XRFSamplePositioner",
            "L93_SpectralFusionProcessor",
            "L93_MineralReferenceLibrary",
            "L93_OpticalSafetyInterlock",
            "L93_SpectrumHologramProjector",
            "L93_ThermalVacuumChamber",
            "L93_ThermalVacuumAccessHatch",
            "L93_MicroSampleHeater",
            "L93_HeaterPrecisionThermocouple",
            "L93_VacuumTurbomolecularPump",
            "L93_VacuumBackingPump",
            "L93_VacuumIsolationManifold",
            "L93_AnalyticalVacuumPressureTransducer",
            "L93_ResidualGasAnalyzer",
            "L93_QuadrupoleMassSpectrometer",
            "L93_MassIonSource",
            "L93_MassFilterRodArray",
            "L93_MassDetectorMultiplier",
            "L93_SampleGasCaptureTrap",
            "L93_VolatileCollectionReservoir",
            "L93_VacuumVentAndPurgePanel",
            "L93_ThermalProcessController",
            "L93_MassSpectrumReadoutRack",
            "L93_InertGloveboxMain",
            "L93_InertGloveboxInspectionWindow",
            "L93_DualAccessGlovePorts",
            "L93_CleanSampleTransferArm",
            "L93_CoreCartridgeA",
            "L93_CoreCartridgeB",
            "L93_MicroSampleAblationStage",
            "L93_SampleTransferCarousel",
            "L93_InertPurgeGasManifold",
            "L93_AtmosphereContaminationWitness",
            "L93_PrecisionMicrobalance",
            "L93_SampleSealCompressionTester",
            "L93_SampleCustodyTrackingTerminal",
            "L93_PristineArchiveReturnLatch",
            "L93_AnalyticalPowerDistribution",
            "L93_AnalyticalSafetyPLC",
            "L93_SCADASystemRack",
            "L93_OpticalInstrumentController",
            "L93_CTSafetyInterlockPLC",
            "L93_VacuumSafetyPowerRelay",
            "L93_InertEnvironmentController",
            "L93_ThermalVacuumFaultInjector",
            "L93_TomographyEncoderFaultInjector",
            "L93_MassDetectorGainFaultInjector",
            "L93_W179CriticalPressurePatchStation",
            "L93_ScienceEvidenceProcessor",
            "L93_ArchiveObjectivePanel",
            "L93_Khepri7GeochemistryReportStation",
            "L93_EmergencyStopMaster",
            "L93_Level93MissionSuccessConsole"
        };
        [MenuItem("Tools/Project Spark/Levels/93/2. Place 100 Modular FBX Models Under Selected Layout")]
        public static void Build()
        {
            var selected = Selection.activeGameObject;
            if (selected == null) { Debug.LogError("[L93] Select the Level 93 layout GameObject first."); return; }
            var matching = new Dictionary<string,string>(StringComparer.OrdinalIgnoreCase);
            foreach (string guid in AssetDatabase.FindAssets("t:Model"))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (!path.EndsWith(".fbx",StringComparison.OrdinalIgnoreCase)) continue;
                string filename = Path.GetFileNameWithoutExtension(path);
                int underscore = filename.IndexOf("_L93_",StringComparison.OrdinalIgnoreCase);
                if (underscore < 0) continue;
                string cleanName = filename.Substring(underscore + 1);
                if (!matching.ContainsKey(cleanName)) matching[cleanName]=path;
            }
            int missing=0; var fail = new List<string>();
            foreach (string model in ModelNames) if (!matching.ContainsKey(model)) { missing++; fail.Add(model); }
            if (missing>0)
            {
                Debug.LogError("[L93] Missing "+missing+" FBX models. Import Models/ before placement. Example: "+String.Join(", ",fail.Take(6).ToArray()));
                return;
            }
            Undo.RegisterFullObjectHierarchyUndo(selected,"Build Level 93 visual art");
            Transform old = selected.transform.Find(ArtRootName);
            if (old != null) Undo.DestroyObjectImmediate(old.gameObject);
            GameObject artRoot=new GameObject(ArtRootName);
            Undo.RegisterCreatedObjectUndo(artRoot,"Create Level 93 art root");
            artRoot.transform.SetParent(selected.transform,false);
            artRoot.transform.localPosition=Vector3.zero;
            artRoot.transform.localRotation=Quaternion.identity;
            artRoot.transform.localScale=Vector3.one;
            int count=0;
            foreach (string name in ModelNames)
            {
                var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(matching[name]);
                if (prefab==null) { Debug.LogError("[L93] Failed loading "+matching[name]); continue; }
                GameObject go=(GameObject)PrefabUtility.InstantiatePrefab(prefab);
                if (go==null) { Debug.LogError("[L93] Could not instantiate "+name); continue; }
                go.transform.SetParent(artRoot.transform,false);
                go.transform.localPosition=Vector3.zero;
                go.transform.localRotation=Quaternion.identity;
                go.transform.localScale=Vector3.one;
                go.name=name;
                count++;
            }
            SparkLevel93MaterialUpgradeEditor.ApplyGeneratedMaterials(artRoot);
            Selection.activeGameObject=artRoot;
            Debug.Log("[L93] Placed "+count+"/100 real FBX models (visual-only, no gameplay simulation). All artwork uses common scene origin.");
        }
    }
}
#endif
