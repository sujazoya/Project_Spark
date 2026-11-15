#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.EditorTools
{
    /// <summary>Visual placement only for Level 69. Does not add SparkTerminal, player wires, electrical/refrigerant physics, or objectives.</summary>
    public static class SparkLevel69ArtPlacementEditor
    {
        private const string ArtRoot = "L69_HD_VISUAL_ART";
        private static readonly string[] Models = {
            "00_L69_IndustrialColdChainBase",
            "01_L69_VariableSpeedRefrigerationCompressor",
            "02_L69_CompressorCutawayAssembly",
            "03_L69_CompressorMotorDrive",
            "04_L69_AirCooledCondenser",
            "05_L69_CondenserFanBank",
            "06_L69_HighPressureLiquidReceiver",
            "07_L69_LiquidLineFilterDrier",
            "08_L69_RefrigerantSightGlass",
            "09_L69_ElectronicExpansionValveA",
            "10_L69_ElectronicExpansionValveB",
            "11_L69_EvaporatorCoilA",
            "12_L69_EvaporatorCoilB",
            "13_L69_EvaporatorFanAssemblyA",
            "14_L69_EvaporatorFanAssemblyB",
            "15_L69_TransparentColdStorageChamber",
            "16_L69_TransparentRapidCoolingChamber",
            "17_L69_TemperatureSensitiveCargoRack",
            "18_L69_SuctionAccumulator",
            "19_L69_CompressorOilSeparator",
            "20_L69_CopperRefrigerantPipelineNetwork",
            "21_L69_SuctionPressureTransmitter",
            "22_L69_DischargePressureTransmitter",
            "23_L69_SuctionTemperatureSensor",
            "24_L69_DischargeTemperatureSensor",
            "25_L69_LiquidLineTemperatureSensor",
            "26_L69_EvaporatorOutletSensorA",
            "27_L69_EvaporatorOutletSensorB",
            "28_L69_ColdChamberTemperatureSensor",
            "29_L69_RapidCoolingTemperatureSensor",
            "30_L69_RefrigerantMassFlowMeter",
            "31_L69_RefrigerationEnergyAnalyzer",
            "32_L69_PressureEnthalpyCycleDisplay",
            "33_L69_DualZoneTemperaturePIDController",
            "34_L69_ElectronicSuperheatController",
            "35_L69_AdaptiveDefrostController",
            "36_L69_EvaporatorDefrostHeaterBank",
            "37_L69_CondensateDrainAssembly",
            "38_L69_CompressorProtectionRelay",
            "39_L69_ColdChainSafetyPLC",
            "40_L69_IndustrialRefrigerationPLCRack",
            "41_L69_RefrigerationDigitalIOModule",
            "42_L69_ThermalLoadDisturbanceModule",
            "43_L69_HolographicRefrigerationSCADA",
            "44_L69_TransparentRefrigerationSafetyEnclosure",
            "45_L69_Regulated5VControlSupply"
        };

        [MenuItem("Tools/Project Spark/Levels/69/Place HD Art Under Selected Layout")]
        public static void Place()
        {
            GameObject destination = Selection.activeGameObject;
            if (destination == null)
            {
                EditorUtility.DisplayDialog("Level 69 Art", "Select your Level 69 layout root before placing visuals.", "OK");
                return;
            }
            var prefabs = new GameObject[Models.Length];
            for (int i = 0; i < Models.Length; i++)
            {
                string selectedPath = null;
                foreach (string guid in AssetDatabase.FindAssets(Models[i] + " t:Model"))
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    if (Path.GetExtension(path).Equals(".fbx", StringComparison.OrdinalIgnoreCase) &&
                        Path.GetFileNameWithoutExtension(path).Equals(Models[i], StringComparison.Ordinal))
                    { selectedPath = path; break; }
                }
                if (selectedPath == null || (prefabs[i] = AssetDatabase.LoadAssetAtPath<GameObject>(selectedPath)) == null)
                {
                    EditorUtility.DisplayDialog("Missing Level 69 Model", Models[i] + ".fbx missing. Import all 46 individual models. No changes made.", "OK");
                    return;
                }
            }
            Transform previous = destination.transform.Find(ArtRoot);
            if (previous != null && !EditorUtility.DisplayDialog("Replace generated Level 69 visuals?",
                "Replace only the " + ArtRoot + " child? Other layout objects remain untouched.", "Replace", "Cancel")) return;
            Undo.IncrementCurrentGroup();
            int group = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Place Level 69 HD Art");
            if (previous != null) Undo.DestroyObjectImmediate(previous.gameObject);
            GameObject root = new GameObject(ArtRoot);
            Undo.RegisterCreatedObjectUndo(root, "Create Level 69 art root");
            root.transform.SetParent(destination.transform, false);
            root.transform.localPosition = Vector3.zero;
            root.transform.localRotation = Quaternion.identity;
            root.transform.localScale = Vector3.one;
            for (int i = 0; i < prefabs.Length; i++)
            {
                var instance = PrefabUtility.InstantiatePrefab(prefabs[i], root.transform) as GameObject;
                if (instance == null) throw new InvalidOperationException("Could not instantiate " + Models[i]);
                Undo.RegisterCreatedObjectUndo(instance, "Place " + Models[i]);
                instance.name = Models[i];
                instance.transform.localPosition = Vector3.zero;
                instance.transform.localRotation = Quaternion.identity;
                instance.transform.localScale = Vector3.one;
            }
            Undo.CollapseUndoOperations(group);
            EditorUtility.SetDirty(root);
            Debug.Log("[ProjectSpark L69 Art] Placed " + Models.Length + " visual FBX assets under " + ArtRoot + ". Gameplay must be built separately.");
        }
    }
}
#endif
