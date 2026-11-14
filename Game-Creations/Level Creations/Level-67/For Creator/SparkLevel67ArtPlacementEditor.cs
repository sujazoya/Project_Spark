#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.EditorTools
{
    /// <summary>Level 67 visual geometry only. Does NOT create SparkTerminal, wiring, simulation or objectives.</summary>
    public static class SparkLevel67ArtPlacementEditor
    {
        private const string ArtRootName = "L67_HD_VISUAL_ART";
        private static readonly string[] FbxNames =
        {
            "00_L67_IndustrialProcessControlBase",
            "01_L67_TransparentSupplyTankA",
            "02_L67_TransparentSupplyTankB",
            "03_L67_CentralMixingVessel",
            "04_L67_FinishedProductTank",
            "05_L67_CentrifugalPumpA",
            "06_L67_CentrifugalPumpB",
            "07_L67_ProductTransferPump",
            "08_L67_PumpMotorA",
            "09_L67_PumpMotorB",
            "10_L67_TransferPumpMotor",
            "11_L67_VariableFrequencyDriveBank",
            "12_L67_ThreeChannelFlowMeterAssembly",
            "13_L67_FlowTransmitterA",
            "14_L67_FlowTransmitterB",
            "15_L67_ProductFlowTransmitter",
            "16_L67_UltrasonicLevelSensorA",
            "17_L67_UltrasonicLevelSensorB",
            "18_L67_ProductTankLevelSensor",
            "19_L67_PressureTransmitterA",
            "20_L67_PressureTransmitterB",
            "21_L67_MotorizedControlValveA",
            "22_L67_MotorizedControlValveB",
            "23_L67_ProductOutletControlValve",
            "24_L67_EmergencyIsolationValve",
            "25_L67_TransparentProcessPipelineNetwork",
            "26_L67_IndustrialMixingImpeller",
            "27_L67_MixingMotorDrive",
            "28_L67_LiquidRatioQualitySensor",
            "29_L67_CascadePIDController",
            "30_L67_FlowRatioController",
            "31_L67_TankLevelController",
            "32_L67_PumpProtectionRelay",
            "33_L67_CavitationDetectionSensor",
            "34_L67_ProcessSafetyPLC",
            "35_L67_IndustrialProcessPLCRack",
            "36_L67_ProcessDigitalIOModule",
            "37_L67_DownstreamDemandSimulator",
            "38_L67_HolographicSCADAMonitor",
            "39_L67_EmergencyStopAndAlarmStation",
            "40_L67_TransparentProcessSafetyEnclosure",
            "41_L67_Regulated5VControlSupply",
        };

        [MenuItem("Tools/Project Spark/Levels/66/Place HD Art Under Selected Layout")]
        public static void Place()
        {
            GameObject destination = Selection.activeGameObject;
            if (destination == null)
            {
                EditorUtility.DisplayDialog("Level 67 Art", "Select an existing Level 67 layout root in the Hierarchy first.", "OK");
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
                    EditorUtility.DisplayDialog("Missing Level 67 FBX", FbxNames[i] + ".fbx was not found. Import all 42 models. No changes made.", "OK");
                    return;
                }
            }

            Transform oldRoot = destination.transform.Find(ArtRootName);
            if (oldRoot != null && !EditorUtility.DisplayDialog("Replace Level 67 generated art?",
                "Replace ONLY " + ArtRootName + " under the selected layout?", "Replace", "Cancel"))
                return;

            Undo.IncrementCurrentGroup();
            int undoGroup = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Place Level 67 HD Visual Art");
            if (oldRoot != null) Undo.DestroyObjectImmediate(oldRoot.gameObject);

            GameObject root = new GameObject(ArtRootName);
            Undo.RegisterCreatedObjectUndo(root, "Create L67 art root");
            root.transform.SetParent(destination.transform, false);
            root.transform.localPosition = Vector3.zero;
            root.transform.localRotation = Quaternion.identity;
            root.transform.localScale = Vector3.one;
            for (int i = 0; i < prefabs.Length; i++)
            {
                GameObject model = PrefabUtility.InstantiatePrefab(prefabs[i], root.transform) as GameObject;
                if (model == null) throw new InvalidOperationException("Could not instantiate " + FbxNames[i]);
                Undo.RegisterCreatedObjectUndo(model, "Place Level 67 model");
                model.name = FbxNames[i];
                model.transform.localPosition = Vector3.zero;
                model.transform.localRotation = Quaternion.identity;
                model.transform.localScale = Vector3.one;
            }
            Undo.CollapseUndoOperations(undoGroup);
            Selection.activeGameObject = root;
            Debug.Log("[LEVEL 67 ART] Placed 42 FBX models under " + ArtRootName +
                      ". Art only: no gameplay terminals, player wires, physics or objective creation.", root);
        }
    }
}
#endif
