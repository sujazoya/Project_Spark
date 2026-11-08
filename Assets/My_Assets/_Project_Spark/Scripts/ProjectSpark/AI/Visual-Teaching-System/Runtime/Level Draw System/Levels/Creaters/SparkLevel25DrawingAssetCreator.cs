#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;
using ProjectSpark.UI.VisualTeaching;

namespace ProjectSpark.EditorTools
{
    /// <summary>
    /// Four Visual Teaching pages for Level 25. Native SparkLevelDrawingAsset schema.
    /// Diagram is explanatory; does not create actual SparkCircuitConnections.
    /// </summary>
    public static class SparkLevel25DrawingAssetCreator
    {
        private const string Menu = "Tools/Project Spark/Visual Teaching/Create Level 25 Drawing Asset";
        private static readonly Color Red = new Color(.98f, .29f, .35f, 1f);
        private static readonly Color Blue = new Color(.19f, .74f, 1f, 1f);
        private static readonly Color Gold = new Color(1f, .78f, .28f, 1f);
        private static readonly Color Green = new Color(.30f, .98f, .59f, 1f);
        private static readonly Color Cyan = new Color(.28f, .93f, .98f, 1f);

        [MenuItem(Menu)]
        public static void CreateAsset()
        {
            string path = EditorUtility.SaveFilePanelInProject(
                "Create Level 25 Drawing", "Level_25_Drawing", "asset",
                "Save the native four-page Visual Teaching drawing asset.");
            if (string.IsNullOrEmpty(path)) return;
            if (AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path) != null)
            {
                EditorUtility.DisplayDialog("Level 25 Drawing", "An asset already exists at this path. Choose another filename.", "OK");
                return;
            }

            var drawing = ScriptableObject.CreateInstance<SparkLevelDrawingAsset>();
            drawing.levelId = "level_25";
            drawing.levelTitle = "Dual Path Power - Diode OR Gate";
            drawing.initialPage = 0;
            drawing.pages = new SparkLevelDrawingPage[4];
            for (int i = 0; i < drawing.pages.Length; i++) drawing.pages[i] = BuildPage(i);
            AssetDatabase.CreateAsset(drawing, path);
            EditorUtility.SetDirty(drawing);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Selection.activeObject = drawing;
            EditorGUIUtility.PingObject(drawing);
            Debug.Log("[Level 25 Drawing] Created four pages: " + path);
        }

        private static Vector2 P(float x, float y) => new Vector2(x, y);
        private static SparkSchematicPart Part(string id, SparkSchematicKind kind,
            float x, float y, string label, Color color) => new SparkSchematicPart
            { id = id, kind = kind, position = P(x, y), label = label, color = color };

        private static SparkSchematicKind KindOrGeneric(string name)
        {
            SparkSchematicKind kind;
            return Enum.TryParse(name, true, out kind) &&
                Enum.IsDefined(typeof(SparkSchematicKind), kind)
                ? kind : SparkSchematicKind.Generic;
        }

        private static SparkSchematicWire Wire(Color color, params Vector2[] points) =>
            new SparkSchematicWire { color = color, energized = false, points = points };

        private static SparkLevelDrawingPage BuildPage(int index)
        {
            // Positive battery feed branches UP to SW A and DOWN to SW B.
            // Both diode cathodes merge at R1 input. R1 output -> LED anode -> battery minus.
            // Part half-width is approximately 0.06 in normalized renderer coordinates.
            var parts = new[]
            {
                Part("battery", SparkSchematicKind.Battery, .09f, .48f, "5 V", Red),
                Part("swa", SparkSchematicKind.Switch, .31f, .78f, "SW A", Gold),
                Part("swb", SparkSchematicKind.Switch, .31f, .37f, "SW B", Gold),
                Part("d1", KindOrGeneric("Diode"), .53f, .78f, "D1 A >| K", Cyan),
                Part("d2", KindOrGeneric("Diode"), .53f, .37f, "D2 A >| K", Cyan),
                Part("r1", SparkSchematicKind.Resistor, .78f, .61f, "R1 330 Ohm", Gold),
                Part("led", KindOrGeneric("LED"), .89f, .32f, "GREEN LED A >| K", Green),
                Part("gnd", SparkSchematicKind.Ground, .46f, .12f, "BATTERY - / GND", Blue)
            };
            // Eight external wires are represented as 8 distinct schematic polylines.
            var wires = new[]
            {
                Wire(Red, P(.09f,.62f),P(.09f,.78f),P(.25f,.78f)), // 1 Battery+ -> SW A in
                Wire(Red, P(.09f,.62f),P(.18f,.62f),P(.18f,.37f),P(.25f,.37f)), // 2 Battery+ -> SW B in
                Wire(Red, P(.37f,.78f),P(.47f,.78f)), // 3 SW A out -> D1 anode
                Wire(Red, P(.37f,.37f),P(.47f,.37f)), // 4 SW B out -> D2 anode
                Wire(Red, P(.59f,.78f),P(.68f,.78f),P(.68f,.61f),P(.72f,.61f)), // 5 D1 cathode -> R1 in
                Wire(Red, P(.59f,.37f),P(.68f,.37f),P(.68f,.61f),P(.72f,.61f)), // 6 D2 cathode -> R1 in
                Wire(Red, P(.84f,.61f),P(.83f,.61f),P(.83f,.32f)), // 7 R1 out -> LED anode
                Wire(Blue, P(.95f,.26f),P(.95f,.12f),P(.09f,.12f),P(.09f,.34f)) // 8 LED cathode -> Battery-
            };
            string title, caption, explanation;
            switch (index)
            {
                case 0:
                    title = "LEVEL 25 | Dual Path Power | Diode OR Gate";
                    caption = "PAGE 1/4 | EITHER SWITCH POWERS THE LED";
                    explanation = "Two positive-fed input branches, each with a switch and a forward diode, join at the input of one 330 Ohm resistor. " +
                        "The common resistor drives one green LED, returning to Battery_negative. Diode cathode bands face the resistor. " +
                        "These are teaching drawings, not live electrical results.";
                    break;
                case 1:
                    title = "LEVEL 25 | Eight External Connections";
                    caption = "PAGE 2/4 | BATTERY POSITIVE SPLITS TO BOTH SWITCHES";
                    explanation = "CONNECT 8 WIRES: (1) Battery_positive to SwitchA_Input; (2) Battery_positive to SwitchB_Input; " +
                        "(3) SwitchA_Output to D_OR_A_Anode; (4) SwitchB_Output to D_OR_B_Anode; " +
                        "(5) D_OR_A_Cathode to R_LIMIT_Input; (6) D_OR_B_Cathode to R_LIMIT_Input; " +
                        "(7) R_LIMIT_Output to LED_Anode; (8) LED_Cathode to Battery_negative. " +
                        "Both positive branches share Battery_positive, never Battery_negative. No diode bypass.";
                    break;
                case 2:
                    title = "LEVEL 25 | Diode OR Truth Table";
                    caption = "PAGE 3/4 | A OR B | 00 OFF, 10 ON, 01 ON, 11 ON";
                    explanation = "Switch A OFF / B OFF: LED OFF. A ON / B OFF: LED ON through D1. " +
                        "A OFF / B ON: LED ON through D2. A ON / B ON: LED ON. " +
                        "With both closed, the two diode paths may share current; the common resistor still limits LED current. " +
                        "The default Level Definition completes for A ON and B OFF; other truth-table states are manual tests.";
                    break;
                default:
                    title = "LEVEL 25 | Verification & Five Targets";
                    caption = "PAGE 4/4 | 5 V | D1 0.45-1.0 V | LED ON | 5-8 mA";
                    explanation = "BASE COMPLETION STATE: close Switch A, open Switch B. Verify source voltage 4.8-5.2 V; " +
                        "D1 forward drop 0.45-1.0 V; green LED ON; shared resistor current 5-8 mA; " +
                        "resistor drop 2.0-2.6 V. Use actual measured solver values for confirmation. " +
                        "Runtime IDs: R_LIMIT_Component/Input/Output, D_OR_A_Component/Anode/Cathode, " +
                        "D_OR_B_Component/Anode/Cathode. Fixed LED uses led / LED_Anode / LED_Cathode. " +
                        "Only SparkLevelEvaluator awards 5/5; the extra truth-table combinations are not auto-validated by this asset.";
                    break;
            }
            return new SparkLevelDrawingPage
            { title = title, caption = caption, explanation = explanation, parts = parts, wires = wires };
        }
    }
}
#endif
