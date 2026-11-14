#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.EditorTools
{
    /// <summary>Only visual FBX placement. Does NOT create SparkTerminal sockets, 90 wires,
    /// plasma process controls, vacuum / RF / wafer physics, or gameplay objectives.</summary>
    public static class SparkLevel71ArtPlacementEditor
    {
        private const string ArtRoot = "L71_HD_VISUAL_ART";
        private static readonly string[] Models = {
            "00_L71_IndustrialPlasmaFabricationBase",
            "01_L71_TransparentPlasmaEtchReactor",
            "02_L71_ReactorServiceLid",
            "03_L71_QuartzPlasmaObservationViewport",
            "04_L71_InductivelyCoupledRFCoil",
            "05_L71_PrecisionGasDistributionShowerhead",
            "06_L71_WaferBiasElectrode",
            "07_L71_ElectrostaticWaferChuck",
            "08_L71_PrecisionWaferLiftAssembly",
            "09_L71_SiliconWafer",
            "10_L71_WaferThinFilmLayer",
            "11_L71_PatternedWaferMask",
            "12_L71_WaferCarrierCassette",
            "13_L71_VacuumLoadLockChamber",
            "14_L71_LoadLockGateValve",
            "15_L71_AutomatedWaferTransferRobot",
            "16_L71_TurbomolecularVacuumPump",
            "17_L71_VacuumBackingPump",
            "18_L71_IndustrialVacuumForeline",
            "19_L71_VacuumIsolationValve",
            "20_L71_AutomaticVacuumThrottleValve",
            "21_L71_CapacitancePressureManometer",
            "22_L71_RoughVacuumPressureGauge",
            "23_L71_HighVacuumPressureGauge",
            "24_L71_ResidualGasAnalyzer",
            "25_L71_VirtualProcessGasSupplyA",
            "26_L71_VirtualProcessGasSupplyB",
            "27_L71_MassFlowControllerA",
            "28_L71_MassFlowControllerB",
            "29_L71_VacuumGasMixingManifold",
            "30_L71_HighFrequencyRFGenerator",
            "31_L71_AutomaticRFMatchingNetwork",
            "32_L71_RFVariableCapacitorA",
            "33_L71_RFVariableCapacitorB",
            "34_L71_ForwardReflectedPowerCoupler",
            "35_L71_RFVoltageCurrentSensor",
            "36_L71_RFArcProtectionModule",
            "37_L71_WaferTemperatureController",
            "38_L71_PrecisionWaferChiller",
            "39_L71_EmergencyPlasmaSafetyStation",
            "40_L71_OpticalEmissionSpectrometer",
            "41_L71_OpticalFiberViewportModule",
            "42_L71_AutomaticEtchEndpointController",
            "43_L71_PlasmaUniformityAnalyzer",
            "44_L71_ThinFilmThicknessMetrology",
            "45_L71_VacuumPressurePIDController",
            "46_L71_PlasmaProcessPLCRack",
            "47_L71_PlasmaDigitalIOModule",
            "48_L71_HolographicPlasmaSCADAMonitor",
            "49_L71_Regulated5VControlSupply"
        };

        [MenuItem("Tools/Project Spark/Levels/71/Place HD Art Under Selected Layout")]
        public static void Place()
        {
            GameObject destination = Selection.activeGameObject;
            if (destination == null)
            {
                EditorUtility.DisplayDialog("Level 71 Visual Art", "Select the Level 71 layout root in the hierarchy.", "OK");
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
                    EditorUtility.DisplayDialog("Missing Level 71 FBX", Models[i] + ".fbx is missing.\nImport all 50 numbered FBXs. No changes made.", "OK");
                    return;
                }
            }
            Transform existing = destination.transform.Find(ArtRoot);
            if (existing != null && !EditorUtility.DisplayDialog("Replace Level 71 art?",
                "Replace only the " + ArtRoot + " child? Gameplay objects outside this child are not touched.", "Replace Art", "Cancel")) return;
            Undo.IncrementCurrentGroup();
            int group = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Place Project Spark Level 71 HD Visual Assets");
            if (existing != null) Undo.DestroyObjectImmediate(existing.gameObject);
            var root = new GameObject(ArtRoot);
            Undo.RegisterCreatedObjectUndo(root, "Create Level 71 art root");
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
            Debug.Log("[Project Spark L71] Placed 50 model prefabs at common origin under " + ArtRoot + ". Actual gameplay is separate.");
        }
    }
}
#endif
