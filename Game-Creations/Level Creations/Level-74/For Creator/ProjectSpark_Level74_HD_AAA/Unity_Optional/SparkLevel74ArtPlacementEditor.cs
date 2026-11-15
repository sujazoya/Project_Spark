#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.EditorTools
{
    /// <summary>Level 74 original 3D geometry placement only. Does NOT implement 102 player wires,
    /// 158 SparkTerminals, photolithography physics, or six authoritative objectives.</summary>
    public static class SparkLevel74ArtPlacementEditor
    {
        private const string ArtRoot = "L74_HD_VISUAL_ART";
        private static readonly string[] ModelNames = {
            "00_L74_IndustrialLithographyWorkstationBase",
            "01_L74_TransparentLithographySafetyEnclosure",
            "02_L74_AutomatedPhotoresistTrack",
            "03_L74_PhotoresistSpinCoatingBowl",
            "04_L74_PrecisionSpinChuckDrive",
            "05_L74_VirtualPhotoresistSupplyCartridge",
            "06_L74_PhotoresistMeteringPump",
            "07_L74_PrecisionPhotoresistNozzle",
            "08_L74_ResistThicknessMeasurementSensor",
            "09_L74_SoftBakeHotplateModule",
            "10_L74_SoftBakeTemperatureSensor",
            "11_L74_SiliconWaferCarrierCassette",
            "12_L74_AutomatedWaferHandlingRobot",
            "13_L74_WaferPrealignmentStation",
            "14_L74_PrecisionProjectionScannerFrame",
            "15_L74_VirtualDUVLightSource",
            "16_L74_OpticalIlluminationHomogenizer",
            "17_L74_HighSpeedExposureShutter",
            "18_L74_PrecisionReticleStorageCassette",
            "19_L74_HighResolutionCircuitReticle",
            "20_L74_PrecisionReticleMotionStage",
            "21_L74_ReticleStageLinearEncoder",
            "22_L74_ProjectionOpticsLensBarrel",
            "23_L74_ActiveOpticalFocusCorrector",
            "24_L74_NumericalApertureSelector",
            "25_L74_NanometreWaferXYStage",
            "26_L74_DualAxisLinearServoMotorAssembly",
            "27_L74_WaferZTiltFocusStage",
            "28_L74_WaferStageLaserInterferometer",
            "29_L74_WaferAlignmentMarkCamera",
            "30_L74_ElectrostaticWaferChuck",
            "31_L74_PlanarizedSiliconWafer",
            "32_L74_WaferOverlayAlignmentMarks",
            "33_L74_PhotosensitiveResistLayer",
            "34_L74_WaferTopographyFocusMap",
            "35_L74_WaferHeightLevelingSensor",
            "36_L74_ActiveVibrationIsolationPlatform",
            "37_L74_SixAxisVibrationMeasurementSensor",
            "38_L74_ProjectionLensThermalDriftSensor",
            "39_L74_ExposureDosePhotodiode",
            "40_L74_PrecisionScanSlitAssembly",
            "41_L74_LithographyEmergencySafetyStation",
            "42_L74_PostExposureBakeModule",
            "43_L74_PostExposureBakeTemperatureSensor",
            "44_L74_VirtualPhotoresistDeveloperModule",
            "45_L74_DeveloperDispensingNozzle",
            "46_L74_DeveloperFlowTransmitter",
            "47_L74_WaferRinseSpinDryer",
            "48_L74_CriticalDimensionInspectionScanner",
            "49_L74_OverlayAlignmentMetrologySystem",
            "50_L74_ReticleDefectInspectionModule",
            "51_L74_PhotoresistPatternDefectCamera",
            "52_L74_LithographyProcessPLCRack",
            "53_L74_StageSynchronizationController",
            "54_L74_HolographicLithographySCADAMonitor",
            "55_L74_Regulated5VControlSupply"
        };

        [MenuItem("Tools/Project Spark/Levels/74/Place HD Art Under Selected Layout")]
        public static void Place()
        {
            GameObject selected = Selection.activeGameObject;
            if (selected == null)
            {
                EditorUtility.DisplayDialog("Level 74 Art", "Select your Level 74 layout root.", "OK");
                return;
            }
            // Find and validate *all* FBXs before touching an existing scene child.
            GameObject[] prefabs = new GameObject[ModelNames.Length];
            for (int i = 0; i < ModelNames.Length; i++)
            {
                string found = null;
                foreach (string guid in AssetDatabase.FindAssets(ModelNames[i] + " t:Model"))
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    if (Path.GetExtension(path).Equals(".fbx", StringComparison.OrdinalIgnoreCase) &&
                        Path.GetFileNameWithoutExtension(path).Equals(ModelNames[i], StringComparison.Ordinal))
                    { found = path; break; }
                }
                if (found == null || (prefabs[i] = AssetDatabase.LoadAssetAtPath<GameObject>(found)) == null)
                {
                    EditorUtility.DisplayDialog("Level 74 FBX missing",
                        ModelNames[i] + ".fbx was not imported.\nImport all 56 numbered Level 74 FBXs before placing art. No changes made.", "OK");
                    return;
                }
            }

            Transform oldArt = selected.transform.Find(ArtRoot);
            if (oldArt != null && !EditorUtility.DisplayDialog("Replace Level 74 art?",
                "Replace only the " + ArtRoot + " child? Existing gameplay objects remain unchanged.", "Replace Art", "Cancel"))
                return;

            int undoGroup = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Place Level 74 lithography art");
            if (oldArt != null) Undo.DestroyObjectImmediate(oldArt.gameObject);
            GameObject holder = new GameObject(ArtRoot);
            Undo.RegisterCreatedObjectUndo(holder, "Level 74 Art Root");
            holder.transform.SetParent(selected.transform, false);
            holder.transform.localPosition = Vector3.zero;
            holder.transform.localRotation = Quaternion.identity;
            holder.transform.localScale = Vector3.one;
            for (int i = 0; i < prefabs.Length; i++)
            {
                GameObject go = PrefabUtility.InstantiatePrefab(prefabs[i], holder.transform) as GameObject;
                if (go == null) throw new InvalidOperationException("Could not instantiate " + ModelNames[i]);
                Undo.RegisterCreatedObjectUndo(go, "Place " + ModelNames[i]);
                go.name = ModelNames[i];
                go.transform.localPosition = Vector3.zero;
                go.transform.localRotation = Quaternion.identity;
                go.transform.localScale = Vector3.one;
            }
            Undo.CollapseUndoOperations(undoGroup);
            Debug.Log("[L74] Placed " + prefabs.Length + " FBXs. Art only. Check approx 18.2m workstation width and FBX cm-to-m import scale.");
        }
    }
}
#endif
