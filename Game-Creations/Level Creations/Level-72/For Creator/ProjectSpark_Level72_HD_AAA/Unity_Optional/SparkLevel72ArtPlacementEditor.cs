#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.EditorTools
{
    /// <summary>Only visual FBX placement. Does NOT create SparkTerminal sockets, 94 wires,
    /// ALD cycle process controls, vacuum / gas / film physics, or gameplay objectives.</summary>
    public static class SparkLevel72ArtPlacementEditor
    {
        private const string ArtRoot = "L72_HD_VISUAL_ART";
        private static readonly string[] Models = {
            "00_L72_IndustrialAtomicDepositionBase",
            "01_L72_TransparentALDReactorChamber",
            "02_L72_ReactorServiceLid",
            "03_L72_QuartzObservationViewport",
            "04_L72_MultiZoneHeatedWaferChuck",
            "05_L72_PrecisionSiliconWafer",
            "06_L72_AtomicThinFilmVisualization",
            "07_L72_HighAspectRatioTrenchSample",
            "08_L72_WaferLiftPinAssembly",
            "09_L72_WaferCarrierCassette",
            "10_L72_TransparentVacuumLoadLock",
            "11_L72_LoadLockGateValve",
            "12_L72_AutomatedWaferTransferRobot",
            "13_L72_VirtualPrecursorSupplyA",
            "14_L72_VirtualPrecursorSupplyB",
            "15_L72_InertPurgeGasSupply",
            "16_L72_HighSpeedPulseValveA",
            "17_L72_HighSpeedPulseValveB",
            "18_L72_HighSpeedPurgeValve",
            "19_L72_MassFlowControllerA",
            "20_L72_MassFlowControllerB",
            "21_L72_PurgeMassFlowTransmitter",
            "22_L72_SequentialGasDistributionManifold",
            "23_L72_PrecisionDepositionShowerhead",
            "24_L72_HeatedGasDeliveryPipeline",
            "25_L72_TurbomolecularVacuumPump",
            "26_L72_VacuumBackingPump",
            "27_L72_AutomaticVacuumThrottleValve",
            "28_L72_VacuumIsolationValve",
            "29_L72_IndustrialVacuumForeline",
            "30_L72_CapacitancePressureManometer",
            "31_L72_RoughVacuumPressureGauge",
            "32_L72_HeatedDeliveryLineSensor",
            "33_L72_WaferChuckTemperatureSensor",
            "34_L72_MultiZoneHeaterPowerBank",
            "35_L72_QuartzCrystalMicrobalance",
            "36_L72_InSituOpticalEllipsometer",
            "37_L72_OpticalThicknessViewport",
            "38_L72_ConformalFilmMetrologyScanner",
            "39_L72_SurfaceCoverageMonitor",
            "40_L72_ResidualVaporAnalyzer",
            "41_L72_ReactantOverlapMonitor",
            "42_L72_GrowthPerCycleAnalyzer",
            "43_L72_FourStageALDCycleSequencer",
            "44_L72_AtomicDepositionPLCRack",
            "45_L72_WaferTemperaturePIDController",
            "46_L72_VacuumPressurePIDController",
            "47_L72_PurgeInterlockProtectionRelay",
            "48_L72_VirtualExhaustTreatmentModule",
            "49_L72_EmergencyDepositionSafetyStation",
            "50_L72_HolographicAtomicDepositionSCADA",
            "51_L72_Regulated5VControlSupply"
        };

        [MenuItem("Tools/Project Spark/Levels/72/Place HD Art Under Selected Layout")]
        public static void Place()
        {
            GameObject destination = Selection.activeGameObject;
            if (destination == null)
            {
                EditorUtility.DisplayDialog("Level 72 Visual Art", "Select the Level 72 layout root in the hierarchy.", "OK");
                return;
            }
            GameObject[] prefabs = new GameObject[Models.Length];
            for (int i = 0; i < Models.Length; i++)
            {
                string found = null;
                foreach (string guid in AssetDatabase.FindAssets(Models[i] + " t:Model"))
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    if (Path.GetExtension(path).Equals(".fbx", StringComparison.OrdinalIgnoreCase) &&
                        Path.GetFileNameWithoutExtension(path).Equals(Models[i], StringComparison.Ordinal))
                    {
                        found = path;
                        break;
                    }
                }
                if (found == null || (prefabs[i] = AssetDatabase.LoadAssetAtPath<GameObject>(found)) == null)
                {
                    EditorUtility.DisplayDialog("Missing Level 72 FBX", Models[i] + ".fbx is missing.\nImport all 52 numbered FBXs. No changes made.", "OK");
                    return;
                }
            }
            Transform existing = destination.transform.Find(ArtRoot);
            if (existing != null && !EditorUtility.DisplayDialog("Replace Level 72 art?",
                "Replace only the " + ArtRoot + " child? Gameplay objects outside this child are not touched.", "Replace Art", "Cancel")) return;
            Undo.IncrementCurrentGroup();
            int group = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Place Project Spark Level 72 HD Visual Assets");
            if (existing != null) Undo.DestroyObjectImmediate(existing.gameObject);
            var root = new GameObject(ArtRoot);
            Undo.RegisterCreatedObjectUndo(root, "Create Level 72 art root");
            root.transform.SetParent(destination.transform, false);
            root.transform.localPosition = Vector3.zero;
            root.transform.localRotation = Quaternion.identity;
            root.transform.localScale = Vector3.one;
            for (int i = 0; i < Models.Length; i++)
            {
                var instance = PrefabUtility.InstantiatePrefab(prefabs[i], root.transform) as GameObject;
                if (instance == null) throw new InvalidOperationException("Failed to instantiate " + Models[i]);
                Undo.RegisterCreatedObjectUndo(instance, "Place " + Models[i]);
                instance.name = Models[i];
                instance.transform.localPosition = Vector3.zero;
                instance.transform.localRotation = Quaternion.identity;
                instance.transform.localScale = Vector3.one;
            }
            Undo.CollapseUndoOperations(group);
            EditorUtility.SetDirty(root);
            Debug.Log("[Project Spark L72] Placed 52 model prefabs at common origin under " + ArtRoot + ". Actual gameplay is separate.");
        }
    }
}
#endif
