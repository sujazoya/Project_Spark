#if UNITY_EDITOR
using System;
using TMPro;
using UnityEditor;
using UnityEngine;
using ProjectSpark.Electrical;
using ProjectSpark.Gameplay;

namespace ProjectSpark.EditorTools
{
    /// <summary>Scene-only installer: never rewrites your existing electrical scripts.</summary>
    public static class SparkLCDMeterInstaller
    {
        private const string MenuRoot = "Tools/Project Spark/Electrical/LCD Meter/";
        private const string DefinitionAsset =
            "Assets/ProjectSpark_LCD_Meter/SparkLCDMeterSolverDefinition.asset";

        [MenuItem(MenuRoot + "Install on Selected LCD")]
        private static void Install()
        {
            GameObject root = Selection.activeGameObject;
            if (root == null)
            {
                EditorUtility.DisplayDialog("Spark LCD Meter",
                    "Select an LCD.fbx instance in your scene Hierarchy, then run the installer.", "OK");
                return;
            }
            if (EditorUtility.IsPersistent(root))
            {
                EditorUtility.DisplayDialog("Spark LCD Meter",
                    "Drag LCD.fbx into the SCENE first. Select that scene instance, not the Project asset.", "OK");
                return;
            }
            Undo.RegisterFullObjectHierarchyUndo(root, "Install Spark LCD Meter");
            SparkLCDMeter meter = root.GetComponent<SparkLCDMeter>();
            if (meter == null) meter = Undo.AddComponent<SparkLCDMeter>(root);
            SparkLCDMeterDisplay display = root.GetComponent<SparkLCDMeterDisplay>();
            if (display == null) display = Undo.AddComponent<SparkLCDMeterDisplay>(root);

            SparkTerminal a = FindOrCreateTerminal(root.transform, "LCD_Terminal_A",
                new Vector3(-0.08f, -0.04f, -0.005f));
            SparkTerminal b = FindOrCreateTerminal(root.transform, "LCD_Terminal_B",
                new Vector3(+0.08f, -0.04f, -0.005f));
            meter.ConfigureTerminals(a, b);

            TMP_Text text = null;
            Transform screen = FindChildRecursive(root.transform, "scr");
            if (screen == null) screen = root.transform;
            Transform existing = FindChildRecursive(root.transform, "Spark_LCD_1602_Text");
            if (existing != null) text = existing.GetComponent<TMP_Text>();
            if (text == null)
            {
                GameObject go = new GameObject("Spark_LCD_1602_Text");
                Undo.RegisterCreatedObjectUndo(go, "Create Spark LCD text");
                go.transform.SetParent(screen, false);
                // LCD.fbx meshes have varied local scales/orientations; these are STARTING
                // values only. Align the text with the `scr` face in Scene view.
                go.transform.localPosition = new Vector3(0f, 0f, -0.002f);
                go.transform.localRotation = Quaternion.identity;
                go.transform.localScale = Vector3.one * 0.006f;
                TextMeshPro tmp = Undo.AddComponent<TextMeshPro>(go);
                tmp.text = "SPARK LCD READY\nCONNECT A AND B";
                tmp.fontSize = 4.5f;
                tmp.alignment = TextAlignmentOptions.MidlineLeft;
                tmp.enableWordWrapping = false;
                tmp.color = new Color(0.84f, 0.96f, 1f, 1f);
                tmp.rectTransform.sizeDelta = new Vector2(25f, 9f);
                text = tmp;
            }

            SparkElectricalSolver solver =
                UnityEngine.Object.FindFirstObjectByType<SparkElectricalSolver>();
            display.AssignSceneReferences(text, meter, solver);
            EditorUtility.SetDirty(meter);
            EditorUtility.SetDirty(display);
            if (PrefabUtility.IsPartOfPrefabInstance(root))
            {
                PrefabUtility.RecordPrefabInstancePropertyModifications(meter);
                PrefabUtility.RecordPrefabInstancePropertyModifications(display);
            }
            Selection.activeGameObject = root;

            string warning = "";
            if (solver != null)
            {
                SerializedObject serializedSolver = new SerializedObject(solver);
                SerializedProperty rootProp = serializedSolver.FindProperty("componentSearchRoot");
                Transform search = rootProp != null ? rootProp.objectReferenceValue as Transform : null;
                if (search != null && !root.transform.IsChildOf(search) && root.transform != search)
                    warning = "\n\nWARNING: LCD is OUTSIDE the solver Component Search Root. " +
                        "Move it beneath your ElectronicObjectRoot/search root.";
            }
            else warning = "\n\nNo scene SparkElectricalSolver found; assign it in LCD Display.";
            Debug.Log("[Spark LCD] Installed scene LCD meter. Terminals, TMP screen, and " +
                "display created. MUST add the LCD Meter Solver Definition asset to your existing Solver Registry." + warning,
                root);
            EditorUtility.DisplayDialog("Spark LCD Meter",
                "Installed LCD meter on the selected scene object.\n\n" +
                "1. Move A and B terminal spheres to your preferred two connection pins.\n" +
                "2. Align Spark_LCD_1602_Text over the visible LCD screen.\n" +
                "3. Create/register the LCD Solver Definition from the next menu.\n" +
                "4. Assign source bindings on SparkLCDMeterDisplay.\n" +
                "5. Connect terminals using your existing wire tool." + warning, "OK");
        }

        private static SparkTerminal FindOrCreateTerminal(
            Transform root, string name, Vector3 localPosition)
        {
            Transform existing = root.Find(name);
            GameObject go;
            if (existing != null) go = existing.gameObject;
            else
            {
                go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                Undo.RegisterCreatedObjectUndo(go, "Create LCD terminal");
                go.name = name;
                go.transform.SetParent(root, false);
                go.transform.localPosition = localPosition;
                go.transform.localRotation = Quaternion.identity;
                go.transform.localScale = Vector3.one * 0.018f;
            }
            SparkTerminal terminal = go.GetComponent<SparkTerminal>();
            if (terminal == null) terminal = Undo.AddComponent<SparkTerminal>(go);
            if (go.GetComponent<Collider>() == null)
                Undo.AddComponent<SphereCollider>(go);
            return terminal;
        }

        private static Transform FindChildRecursive(Transform root, string wanted)
        {
            if (root.name.Equals(wanted, StringComparison.OrdinalIgnoreCase)) return root;
            foreach (Transform child in root)
            {
                Transform found = FindChildRecursive(child, wanted);
                if (found != null) return found;
            }
            return null;
        }

        [MenuItem(MenuRoot + "Create Solver Definition Asset")]
        private static void CreateDefinition()
        {
            if (!AssetDatabase.IsValidFolder("Assets/ProjectSpark_LCD_Meter"))
                AssetDatabase.CreateFolder("Assets", "ProjectSpark_LCD_Meter");
            SparkLCDMeterSolverDefinition asset =
                AssetDatabase.LoadAssetAtPath<SparkLCDMeterSolverDefinition>(DefinitionAsset);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<SparkLCDMeterSolverDefinition>();
                AssetDatabase.CreateAsset(asset, DefinitionAsset);
                AssetDatabase.SaveAssets();
            }
            Selection.activeObject = asset;
            EditorGUIUtility.PingObject(asset);
            EditorUtility.DisplayDialog("Spark LCD Solver Definition",
                "Created/selected:\n" + DefinitionAsset + "\n\n" +
                "Add THIS asset to the existing SparkElectricalSolverRegistry definitions list. " +
                "The menu does not change the registry automatically because your registry may be customized.",
                "OK");
        }
    }
}
#endif
