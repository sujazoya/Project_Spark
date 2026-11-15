#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.EditorTools
{
    /// <summary>Level 75 original 3D geometry placement only. Does NOT implement 106 player wires,
    /// 164 SparkTerminals, ion implantation physics, or six authoritative objectives.</summary>
    public static class SparkLevel75ArtPlacementEditor
    {
        private const string ArtRoot = "L75_HD_VISUAL_ART";
        private static readonly string[] ModelNames = {
            "00_L75_IndustrialIonImplantationBase",
            "01_L75_TransparentBeamlineSafetyEnclosure",
            "02_L75_PrecisionIonSourceAssembly",
            "03_L75_VirtualDopantSourceCartridge",
            "04_L75_IonSourcePlasmaChamber",
            "05_L75_IonSourceEmissionModule",
            "06_L75_IonExtractionElectrodeAssembly",
            "07_L75_VirtualHighVoltagePowerCabinet",
            "08_L75_MagneticMassAnalyzer",
            "09_L75_MassResolvingAperture",
            "10_L75_BeamEnergySelectionModule",
            "11_L75_CeramicIonAccelerationColumn",
            "12_L75_NeutralParticleSeparationModule",
            "13_L75_QuadrupoleBeamFocusingLens",
            "14_L75_ElectromagneticBeamSteeringAssembly",
            "15_L75_ProgrammableXYBeamScanner",
            "16_L75_FastBeamBlankingGate",
            "17_L75_PrimaryBeamCurrentMonitor",
            "18_L75_PrecisionFaradayCupDosimeter",
            "19_L75_FourCornerBeamUniformityCups",
            "20_L75_WaferChargeNeutralizationModule",
            "21_L75_TransparentWaferEndstation",
            "22_L75_ElectrostaticWaferHoldingChuck",
            "23_L75_WaferTiltRotationStage",
            "24_L75_PatternedSiliconWafer",
            "25_L75_DevelopedPhotoresistImplantMask",
            "26_L75_ImplantedDopantRegionVisualization",
            "27_L75_HolographicDopantDepthProfile",
            "28_L75_AutomatedWaferTransferRobot",
            "29_L75_WaferCarrierCassette",
            "30_L75_VacuumWaferLoadLock",
            "31_L75_LoadLockGateValve",
            "32_L75_TurbomolecularVacuumPump",
            "33_L75_VacuumBackingPump",
            "34_L75_AutomaticVacuumThrottleValve",
            "35_L75_PrecisionVacuumPressureGauge",
            "36_L75_ResidualGasDiagnosticModule",
            "37_L75_WaferTemperatureSensor",
            "38_L75_WaferCoolingSystem",
            "39_L75_ProtectedBeamDumpCollector",
            "40_L75_BeamlineSafetyInterlockRelay",
            "41_L75_BeamProfileInspectionScanner",
            "42_L75_IonSpeciesPurityAnalyzer",
            "43_L75_IntegratedIonDoseController",
            "44_L75_WaferDoseUniformityAnalyzer",
            "45_L75_WaferSurfacePotentialSensor",
            "46_L75_CrystalLatticeDamageAnalyzer",
            "47_L75_RapidThermalActivationChamber",
            "48_L75_MultiZoneActivationHeater",
            "49_L75_ActivationTemperatureSensor",
            "50_L75_SheetResistanceMetrologyProbe",
            "51_L75_DopantJunctionDepthScanner",
            "52_L75_WaferRegistrationCamera",
            "53_L75_IonImplantationPLCRack",
            "54_L75_BeamPowerIsolationController",
            "55_L75_EmergencyImplantationSafetyStation",
            "56_L75_HolographicIonImplantationSCADA",
            "57_L75_Regulated5VControlSupply"
        };

        [MenuItem("Tools/Project Spark/Levels/74/Place HD Art Under Selected Layout")]
        public static void Place()
        {
            GameObject selected = Selection.activeGameObject;
            if (selected == null)
            {
                EditorUtility.DisplayDialog("Level 75 Art", "Select your Level 75 layout root.", "OK");
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
                    EditorUtility.DisplayDialog("Level 75 FBX missing",
                        ModelNames[i] + ".fbx was not imported.\nImport all 58 numbered Level 75 FBXs before placing art. No changes made.", "OK");
                    return;
                }
            }

            Transform oldArt = selected.transform.Find(ArtRoot);
            if (oldArt != null && !EditorUtility.DisplayDialog("Replace Level 75 art?",
                "Replace only the " + ArtRoot + " child? Existing gameplay objects remain unchanged.", "Replace Art", "Cancel"))
                return;

            int undoGroup = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Place Level 75 lithography art");
            if (oldArt != null) Undo.DestroyObjectImmediate(oldArt.gameObject);
            GameObject holder = new GameObject(ArtRoot);
            Undo.RegisterCreatedObjectUndo(holder, "Level 75 Art Root");
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
            Debug.Log("[L75] Placed " + prefabs.Length + " FBXs. Art only. Check approx 19.2m workstation width and FBX cm-to-m import scale.");
        }
    }
}
#endif
