#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;
namespace ProjectSpark.EditorTools
{
    /// <summary>Places Level 73 FBX art only; does not construct 98 player wires, 152 SparkTerminals,
    /// CMP physics, six objectives, or an authoritative LevelGamePlayManager.</summary>
    public static class SparkLevel73ArtPlacementEditor
    {
        private const string ArtRoot = "L73_HD_VISUAL_ART";
        private static readonly string[] Models = {
            "00_L73_IndustrialCMPWorkstationBase",
            "01_L73_TransparentCMPSafetyEnclosure",
            "02_L73_RotatingPolishingPlaten",
            "03_L73_GroovedPolishingPad",
            "04_L73_PlatenServoMotorDrive",
            "05_L73_PrecisionWaferCarrierHead",
            "06_L73_CarrierGimbalAssembly",
            "07_L73_MultiZonePressureBladder",
            "08_L73_WaferRetainingRing",
            "09_L73_SiliconWaferSubstrate",
            "10_L73_PatternedThinFilmLayer",
            "11_L73_MicroscopicSurfaceTopography",
            "12_L73_WaferCarrierCassette",
            "13_L73_AutomatedWaferTransferRobot",
            "14_L73_WaferLiftAndAlignmentAssembly",
            "15_L73_VirtualSlurrySupplyCabinet",
            "16_L73_AbrasiveSlurryMixingTank",
            "17_L73_PrecisionSlurryMeteringPump",
            "18_L73_SlurryMassFlowTransmitter",
            "19_L73_MotorizedSlurryDispensingArm",
            "20_L73_PrecisionSlurryNozzle",
            "21_L73_SlurryConcentrationSensor",
            "22_L73_SlurryTemperatureSensor",
            "23_L73_PadConditioningSwingArm",
            "24_L73_PadConditionerAbrasiveDisc",
            "25_L73_PadConditionerMotorDrive",
            "26_L73_PadWearMeasurementSensor",
            "27_L73_PadGrooveInspectionScanner",
            "28_L73_CarrierDownforceLoadCell",
            "29_L73_CarrierPressureRegulator",
            "30_L73_PlatenSpeedEncoder",
            "31_L73_CarrierSpeedEncoder",
            "32_L73_MotorTorqueCurrentAnalyzer",
            "33_L73_PolishingVibrationSensor",
            "34_L73_AcousticEmissionSensor",
            "35_L73_InterfaceTemperatureProbe",
            "36_L73_PolishingCoolingLoop",
            "37_L73_TransparentSlurrySplashGuard",
            "38_L73_WasteSlurryCollectionModule",
            "39_L73_CMPEmergencySafetyStation",
            "40_L73_PostCMPRinseChamber",
            "41_L73_WaferBrushCleaningAssembly",
            "42_L73_SpinRinseDryer",
            "43_L73_InSituOpticalEndpointSensor",
            "44_L73_EddyCurrentThicknessSensor",
            "45_L73_OpticalSurfaceProfilometer",
            "46_L73_WaferDefectInspectionCamera",
            "47_L73_DishingErosionAnalyzer",
            "48_L73_MultiZonePressurePIDController",
            "49_L73_PlatenCarrierMotionController",
            "50_L73_CMPProcessPLCRack",
            "51_L73_CMPDigitalIOModule",
            "52_L73_HolographicCMPSCADA",
            "53_L73_Regulated5VControlSupply"
        };
        [MenuItem("Tools/Project Spark/Levels/73/Place HD Art Under Selected Layout")]
        public static void Place()
        {
            var destination = Selection.activeGameObject;
            if (destination == null)
            {
                EditorUtility.DisplayDialog("Level 73 Art", "Select the Level 73 layout root first.", "OK");
                return;
            }
            GameObject[] prefabs = new GameObject[Models.Length];
            for (int i=0; i<Models.Length; i++)
            {
                string found = null;
                foreach (string guid in AssetDatabase.FindAssets(Models[i]+" t:Model"))
                {
                    string path=AssetDatabase.GUIDToAssetPath(guid);
                    if (Path.GetExtension(path).Equals(".fbx",StringComparison.OrdinalIgnoreCase) &&
                        Path.GetFileNameWithoutExtension(path).Equals(Models[i],StringComparison.Ordinal))
                    { found=path; break; }
                }
                if (found==null || (prefabs[i]=AssetDatabase.LoadAssetAtPath<GameObject>(found))==null)
                {
                    EditorUtility.DisplayDialog("Level 73 FBX missing",Models[i]+".fbx not imported.
Import all 54 numbered FBX models. No changes made.","OK");
                    return;
                }
            }
            Transform previous=destination.transform.Find(ArtRoot);
            if (previous != null && !EditorUtility.DisplayDialog("Replace Level 73 visual art?",
                "Replace only the "+ArtRoot+" child? Other gameplay objects are preserved.","Replace Visual Art","Cancel")) return;
            Undo.IncrementCurrentGroup();int g=Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Place Project Spark Level 73 CMP art");
            if (previous!=null) Undo.DestroyObjectImmediate(previous.gameObject);
            GameObject root=new GameObject(ArtRoot);
            Undo.RegisterCreatedObjectUndo(root,"Create Level 73 art root");
            root.transform.SetParent(destination.transform,false);
            root.transform.localPosition=Vector3.zero;
            root.transform.localRotation=Quaternion.identity;
            root.transform.localScale=Vector3.one;
            for (int i=0;i<Models.Length;i++)
            {
                GameObject o=PrefabUtility.InstantiatePrefab(prefabs[i],root.transform) as GameObject;
                if(o==null) throw new InvalidOperationException("Cannot instantiate "+Models[i]);
                Undo.RegisterCreatedObjectUndo(o,"Place "+Models[i]);
                o.name=Models[i];
                o.transform.localPosition=Vector3.zero;
                o.transform.localRotation=Quaternion.identity;
                o.transform.localScale=Vector3.one;
            }
            Undo.CollapseUndoOperations(g);
            EditorUtility.SetDirty(root);
            Debug.Log("[Level 73 Art] Placed "+Models.Length+" independent FBXs under "+ArtRoot+". Art only. Verify cm-to-m import conversion (workstation ~17.15m wide).");
        }
    }
}
#endif
