#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.EditorTools
{
    /// <summary>Level 66 visual geometry only. Does NOT create SparkTerminal, wiring, simulation or objectives.</summary>
    public static class SparkLevel66ArtPlacementEditor
    {
        private const string ArtRootName = "L66_HD_VISUAL_ART";
        private static readonly string[] FbxNames =
        {
            "00_L66_IndustrialPredictiveMaintenanceBase",
            "01_L66_IndustrialThreePhaseMotor",
            "02_L66_MotorStatorCutaway",
            "03_L66_MotorRotorShaft",
            "04_L66_IndustrialGearboxHousing",
            "05_L66_GearboxGearAssembly",
            "06_L66_DriveShaftCoupling",
            "07_L66_DriveEndBearingCartridge",
            "08_L66_NonDriveEndBearingCartridge",
            "09_L66_DamagedBearingCartridge",
            "10_L66_HealthyReplacementBearing",
            "11_L66_IndustrialConveyorAssembly",
            "12_L66_ConveyorDriveRoller",
            "13_L66_VariableFrequencyMotorDrive",
            "14_L66_MotorCurrentSensorBank",
            "15_L66_ThreeAxisVibrationSensor",
            "16_L66_SecondaryVibrationSensor",
            "17_L66_PrecisionShaftEncoder",
            "18_L66_BearingTemperatureProbe",
            "19_L66_MotorTemperatureProbe",
            "20_L66_InfraredThermalCamera",
            "21_L66_LubricationMonitoringModule",
            "22_L66_ShaftAlignmentSensor",
            "23_L66_GearWearSensorModule",
            "24_L66_VibrationFFTAnalyzer",
            "25_L66_ConditionMonitoringController",
            "26_L66_PredictiveMaintenanceAIUnit",
            "27_L66_IndustrialDigitalTwinDisplay",
            "28_L66_MotorProtectionRelay",
            "29_L66_MaintenancePLCRack",
            "30_L66_DigitalInputOutputModule",
            "31_L66_BearingRemovalTool",
            "32_L66_PrecisionAlignmentGauge",
            "33_L66_IndustrialTorqueTool",
            "34_L66_MaintenancePartsStorageRack",
            "35_L66_ActiveCoolingAssembly",
            "36_L66_EmergencyStopAndLockoutPanel",
            "37_L66_HolographicDiagnosticMonitor",
            "38_L66_TransparentMachineSafetyEnclosure",
            "39_L66_Regulated5VControlSupply",
        };

        [MenuItem("Tools/Project Spark/Levels/66/Place HD Art Under Selected Layout")]
        public static void Place()
        {
            GameObject destination = Selection.activeGameObject;
            if (destination == null)
            {
                EditorUtility.DisplayDialog("Level 66 Art", "Select an existing Level 66 layout root in the Hierarchy first.", "OK");
                return;
            }

            // Resolve all exact FBXs before modifying the Hierarchy.
            GameObject[] prefabs = new GameObject[FbxNames.Length];
            for (int i = 0; i < FbxNames.Length; i++)
            {
                string exactPath = null;
                foreach (string guid in AssetDatabase.FindAssets(FbxNames[i] + " t:Model"))
                {
                    string candidate = AssetDatabase.GUIDToAssetPath(guid);
                    if (Path.GetExtension(candidate).Equals(".fbx", StringComparison.OrdinalIgnoreCase)
                        && Path.GetFileNameWithoutExtension(candidate).Equals(FbxNames[i], StringComparison.Ordinal))
                    {
                        exactPath = candidate;
                        break;
                    }
                }
                if (exactPath == null || (prefabs[i] = AssetDatabase.LoadAssetAtPath<GameObject>(exactPath)) == null)
                {
                    EditorUtility.DisplayDialog("Missing Level 66 FBX", FbxNames[i] + ".fbx was not found. Import all 40 models. No changes made.", "OK");
                    return;
                }
            }

            Transform oldRoot = destination.transform.Find(ArtRootName);
            if (oldRoot != null && !EditorUtility.DisplayDialog("Replace Level 66 generated art?",
                "Replace ONLY " + ArtRootName + " under the selected layout?", "Replace", "Cancel"))
                return;

            Undo.IncrementCurrentGroup();
            int undoGroup = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Place Level 66 HD Visual Art");
            if (oldRoot != null) Undo.DestroyObjectImmediate(oldRoot.gameObject);

            GameObject root = new GameObject(ArtRootName);
            Undo.RegisterCreatedObjectUndo(root, "Create L66 art root");
            root.transform.SetParent(destination.transform, false);
            root.transform.localPosition = Vector3.zero;
            root.transform.localRotation = Quaternion.identity;
            root.transform.localScale = Vector3.one;
            for (int i = 0; i < prefabs.Length; i++)
            {
                GameObject model = PrefabUtility.InstantiatePrefab(prefabs[i], root.transform) as GameObject;
                if (model == null) throw new InvalidOperationException("Could not instantiate " + FbxNames[i]);
                Undo.RegisterCreatedObjectUndo(model, "Place Level 66 model");
                model.name = FbxNames[i];
                model.transform.localPosition = Vector3.zero;
                model.transform.localRotation = Quaternion.identity;
                model.transform.localScale = Vector3.one;
            }
            Undo.CollapseUndoOperations(undoGroup);
            Selection.activeGameObject = root;
            Debug.Log("[LEVEL 66 ART] Placed 40 FBX models under " + ArtRootName +
                      ". Art only: no gameplay terminals, player wires, physics or objective creation.", root);
        }
    }
}
#endif
