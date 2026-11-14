#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;
namespace ProjectSpark.EditorTools
{
    /// <summary>Level 65 visual art placement only; no terminals, wiring, PLC, physics, or objectives.</summary>
    public static class SparkLevel65ArtPlacementEditor
    {
        private const string RootName = "L65_HD_VISUAL_ART";
        private static readonly string[] Names = {
            "00_L65_IndustrialSmartFactoryBase",
            "01_L65_MainFeedConveyor",
            "02_L65_ConveyorMotorDrive",
            "03_L65_ConveyorRotaryEncoder",
            "04_L65_ProductFeederMagazine",
            "05_L65_OpticalEntrySensor",
            "06_L65_VisionInspectionCamera",
            "07_L65_InspectionLightingModule",
            "08_L65_VisionProcessingController",
            "09_L65_SixAxisIndustrialRobot",
            "10_L65_RobotServoDriveBank",
            "11_L65_RobotJointEncoderModule",
            "12_L65_VacuumGripperAssembly",
            "13_L65_VacuumPumpModule",
            "14_L65_VacuumPressureSensor",
            "15_L65_RobotPickupPlatform",
            "16_L65_SortingConveyorA",
            "17_L65_SortingConveyorB",
            "18_L65_SortingConveyorC",
            "19_L65_PneumaticRejectGate",
            "20_L65_PneumaticSolenoidValve",
            "21_L65_AirPressureRegulator",
            "22_L65_RejectGatePositionSensors",
            "23_L65_ProductCollectionBinA",
            "24_L65_ProductCollectionBinB",
            "25_L65_ProductCollectionBinC",
            "26_L65_DefectiveProductRejectBin",
            "27_L65_IndustrialPLCRack",
            "28_L65_DigitalInputOutputModule",
            "29_L65_RobotMotionController",
            "30_L65_ConveyorSynchronizationController",
            "31_L65_SafetyPLCModule",
            "32_L65_EmergencyStopStation",
            "33_L65_SafetyLightCurtain",
            "34_L65_HolographicFactoryMonitor",
            "35_L65_ProductQualityScanner",
            "36_L65_TransparentRobotSafetyEnclosure",
            "37_L65_Regulated5VControlSupply",
        };
        [MenuItem("Tools/Project Spark/Levels/65/Place HD Art Under Selected Layout")]
        public static void Place()
        {
            GameObject destination = Selection.activeGameObject;
            if (!destination) { EditorUtility.DisplayDialog("Level 65 Art", "Select the Level 65 layout root.", "OK"); return; }
            var prefabs = new GameObject[Names.Length];
            for (int i=0; i<Names.Length; i++)
            {
                string exact = null;
                foreach (string guid in AssetDatabase.FindAssets(Names[i] + " t:Model"))
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    if (Path.GetExtension(path).Equals(".fbx", StringComparison.OrdinalIgnoreCase) &&
                        Path.GetFileNameWithoutExtension(path).Equals(Names[i], StringComparison.Ordinal))
                    { exact=path; break; }
                }
                if (exact == null || !(prefabs[i] = AssetDatabase.LoadAssetAtPath<GameObject>(exact)))
                { EditorUtility.DisplayDialog("Missing Level 65 FBX", Names[i]+".fbx missing. Import all 38 models first; nothing changed.", "OK"); return; }
            }
            var existing = destination.transform.Find(RootName);
            if (existing && !EditorUtility.DisplayDialog("Replace generated art?", "Replace only " + RootName + " under the selected layout?", "Replace", "Cancel")) return;
            Undo.IncrementCurrentGroup(); int group=Undo.GetCurrentGroup(); Undo.SetCurrentGroupName("Place Project Spark Level 65 HD Art");
            if (existing) Undo.DestroyObjectImmediate(existing.gameObject);
            var root = new GameObject(RootName); Undo.RegisterCreatedObjectUndo(root, "Create Level 65 art root"); root.transform.SetParent(destination.transform,false);
            for (int i=0; i<prefabs.Length; i++)
            {
                var instance = PrefabUtility.InstantiatePrefab(prefabs[i],root.transform) as GameObject;
                if (!instance) throw new InvalidOperationException("Failed to instantiate " + Names[i]);
                Undo.RegisterCreatedObjectUndo(instance,"Place Level 65 model"); instance.name=Names[i];
                instance.transform.localPosition=Vector3.zero;
                instance.transform.localRotation=Quaternion.identity;
                instance.transform.localScale=Vector3.one;
            }
            Undo.CollapseUndoOperations(group); Selection.activeGameObject=root;
            Debug.Log("[LEVEL 65 ART] Placed 38 independent FBX models. Gameplay wiring, real terminals, PLC, robotics, and objectives NOT created.", root);
        }
    }
}
#endif
