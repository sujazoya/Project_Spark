#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.EditorTools
{
    /// <summary>Only visual FBX placement. Does NOT create SparkTerminal sockets, 86 wires,
    /// environmental controls, HEPA filtration, air flow physics, or gameplay objectives.</summary>
    public static class SparkLevel70ArtPlacementEditor
    {
        private const string ArtRoot = "L70_HD_VISUAL_ART";
        private static readonly string[] Models = {
            "00_L70_IndustrialCleanroomBase",
            "01_L70_TransparentSemiconductorCleanroom",
            "02_L70_TransparentCleanroomAirlock",
            "03_L70_IndustrialCorridorFacade",
            "04_L70_SemiconductorInspectionBench",
            "05_L70_SiliconWaferCarrierRack",
            "06_L70_AutomatedWaferTransferRobot",
            "07_L70_IndustrialAirHandlingUnit",
            "08_L70_AirIntakePrefilter",
            "09_L70_ChilledWaterCoolingCoil",
            "10_L70_ElectricReheatCoil",
            "11_L70_PrecisionAirHumidifier",
            "12_L70_CondensateDrainAssembly",
            "13_L70_SupplyAirBlowerMotor",
            "14_L70_ReturnAirBlowerMotor",
            "15_L70_ExhaustAirBlowerMotor",
            "16_L70_CeilingFanFilterUnitA",
            "17_L70_CeilingFanFilterUnitB",
            "18_L70_ReplaceableHEPACartridgeA",
            "19_L70_ReplaceableHEPACartridgeB",
            "20_L70_LowLevelReturnAirGrille",
            "21_L70_TransparentSupplyDuctNetwork",
            "22_L70_TransparentReturnDuctNetwork",
            "23_L70_FreshAirMixingDamper",
            "24_L70_CleanroomSupplyDamperA",
            "25_L70_AirlockSupplyDamperB",
            "26_L70_MotorizedPressureReliefDamper",
            "27_L70_CleanroomDifferentialPressureSensor",
            "28_L70_AirlockDifferentialPressureSensor",
            "29_L70_AirVelocityMeasurementProbe",
            "30_L70_CleanroomParticleCounter",
            "31_L70_AirlockParticleCounter",
            "32_L70_CleanroomTemperatureHumiditySensor",
            "33_L70_AirlockTemperatureHumiditySensor",
            "34_L70_AirDewPointSensor",
            "35_L70_FilterDifferentialPressureTransmitter",
            "36_L70_IndustrialAirShowerModule",
            "37_L70_AirlockInnerSafetyDoor",
            "38_L70_AirlockOuterSafetyDoor",
            "39_L70_HEPASealFailureSimulator",
            "40_L70_EmergencyStopAndDoorSafetyStation",
            "41_L70_AirQualityDisturbanceSimulator",
            "42_L70_EnvironmentalControlPLCRack",
            "43_L70_EnvironmentalDigitalIOModule",
            "44_L70_CascadeAirPressurePIDController",
            "45_L70_PsychrometricControlModule",
            "46_L70_HolographicCleanroomSCADAMonitor",
            "47_L70_Regulated5VControlSupply"
        };

        [MenuItem("Tools/Project Spark/Levels/70/Place HD Art Under Selected Layout")]
        public static void Place()
        {
            GameObject destination = Selection.activeGameObject;
            if (destination == null)
            {
                EditorUtility.DisplayDialog("Level 70 Visual Art", "Select the Level 70 layout root in the hierarchy.", "OK");
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
                    EditorUtility.DisplayDialog("Missing Level 70 FBX", Models[i] + ".fbx is missing.\nImport all 48 numbered FBXs. No changes made.", "OK");
                    return;
                }
            }
            Transform existing = destination.transform.Find(ArtRoot);
            if (existing != null && !EditorUtility.DisplayDialog("Replace Level 70 art?",
                "Replace only the " + ArtRoot + " child? Gameplay objects outside this child are not touched.", "Replace Art", "Cancel")) return;
            Undo.IncrementCurrentGroup();
            int group = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Place Project Spark Level 70 HD Visual Assets");
            if (existing != null) Undo.DestroyObjectImmediate(existing.gameObject);
            var root = new GameObject(ArtRoot);
            Undo.RegisterCreatedObjectUndo(root, "Create Level 70 art root");
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
            Debug.Log("[Project Spark L70] Placed 48 model prefabs at common origin under " + ArtRoot + ". Actual gameplay is separate.");
        }
    }
}
#endif
