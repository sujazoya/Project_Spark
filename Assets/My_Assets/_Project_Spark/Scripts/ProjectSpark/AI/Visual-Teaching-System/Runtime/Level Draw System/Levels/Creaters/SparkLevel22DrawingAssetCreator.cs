#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;
using ProjectSpark.UI.VisualTeaching;

namespace ProjectSpark.EditorTools
{
    /// <summary>
    /// Creates four native SparkLevelDrawingAsset teaching pages for Level 22.
    /// Uses the existing schematic renderer. Does not touch electrical wiring,
    /// solver states, runtime bindings, prefabs or level definitions.
    /// </summary>
    public static class SparkLevel22DrawingAssetCreator
    {
        private const string Menu = "Tools/Project Spark/Visual Teaching/Create Level 22 Drawing Asset";
        private static readonly Color Red = new Color(.98f, .29f, .35f, 1f);
        private static readonly Color Blue = new Color(.19f, .74f, 1f, 1f);
        private static readonly Color Gold = new Color(1f, .78f, .28f, 1f);
        private static readonly Color Green = new Color(.30f, .98f, .59f, 1f);
        private static readonly Color Cyan = new Color(.28f, .93f, .98f, 1f);

        [MenuItem(Menu)]
        public static void CreateAsset()
        {
            string path = EditorUtility.SaveFilePanelInProject(
                "Create Level 22 Drawing", "Level_22_Drawing", "asset",
                "Choose where to save the four-page Level 22 teaching asset.");
            if (string.IsNullOrEmpty(path)) return;

            if (AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path) != null)
            {
                EditorUtility.DisplayDialog("Level 22 Drawing",
                    "An asset already exists here. Choose a different name to preserve authored pages.", "OK");
                return;
            }

            SparkLevelDrawingAsset drawing = ScriptableObject.CreateInstance<SparkLevelDrawingAsset>();
            drawing.levelId = "level_22";
            drawing.levelTitle = "One Way Only - Diode Direction";
            drawing.initialPage = 0;
            drawing.pages = new SparkLevelDrawingPage[4];
            for (int i = 0; i < drawing.pages.Length; i++) drawing.pages[i] = BuildPage(i);

            AssetDatabase.CreateAsset(drawing, path);
            EditorUtility.SetDirty(drawing);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Selection.activeObject = drawing;
            EditorGUIUtility.PingObject(drawing);
            Debug.Log("[Level 22 Drawing] Created 4 teaching pages at " + path);
        }

        private static Vector2 P(float x, float y) { return new Vector2(x, y); }

        private static SparkSchematicPart Part(string id, SparkSchematicKind kind,
            float x, float y, string label, Color color)
        {
            return new SparkSchematicPart
            {
                id = id, kind = kind, position = P(x, y), label = label, color = color
            };
        }

        // The uploaded Level 09 sample confirms Battery, Switch, Resistor,
        // Generic and Ground. Later renderer versions may add Diode / LED.
        // Use them if present; otherwise fall back to labeled generic symbols
        // so this script compiles against the existing Project Spark enum.
        private static SparkSchematicKind KindOrGeneric(string kindName)
        {
            SparkSchematicKind kind;
            return Enum.TryParse(kindName, true, out kind)
                && Enum.IsDefined(typeof(SparkSchematicKind), kind)
                ? kind : SparkSchematicKind.Generic;
        }

        private static SparkSchematicWire Wire(Color color, params Vector2[] points)
        {
            return new SparkSchematicWire
            {
                color = color, energized = false, points = points
            };
        }

        private static SparkLevelDrawingPage BuildPage(int page)
        {
            // NORMALIZED SCHEMATIC PLACEMENT, TOP POSITIVE RAIL TO BOTTOM RETURN:
            // Battery+ -> SW1 -> R1 (330 Ohm) -> D1 ANODE -> D1 CATHODE
            // -> LED ANODE -> LED CATHODE -> Battery-.
            // Horizontal schematic part tips approximately center +/- 0.06.
            // The return wire does not bypass the LED or diode.
            SparkSchematicPart[] parts =
            {
                Part("battery", SparkSchematicKind.Battery, .10f, .52f, "5 V", Red),
                Part("sw1", SparkSchematicKind.Switch, .27f, .80f, "SW1", Gold),
                Part("r1", SparkSchematicKind.Resistor, .45f, .80f, "R1 330 Ohm", Gold),
                Part("d1", KindOrGeneric("Diode"), .64f, .80f, "D1 1N4007  A >| K", Cyan),
                Part("led1", KindOrGeneric("LED"), .82f, .80f, "LED1 GREEN  A >| K", Green),
                Part("gnd", SparkSchematicKind.Ground, .58f, .17f, "BATTERY NEGATIVE / GND", Blue)
            };

            SparkSchematicWire[] wires =
            {
                // 1. Battery positive -> switch input.
                Wire(Red, P(.10f,.66f), P(.10f,.80f), P(.21f,.80f)),
                // 2. Switch output -> 330-Ohm resistor input.
                Wire(Red, P(.33f,.80f), P(.39f,.80f)),
                // 3. Resistor output -> diode anode (NO bypass).
                Wire(Red, P(.51f,.80f), P(.58f,.80f)),
                // 4. Diode cathode (banded end) -> LED anode.
                Wire(Red, P(.70f,.80f), P(.76f,.80f)),
                // 5. LED cathode -> battery negative.
                Wire(Blue, P(.88f,.80f), P(.94f,.80f), P(.94f,.17f),
                    P(.58f,.17f), P(.10f,.17f), P(.10f,.38f))
            };

            string title;
            string caption;
            string explanation;
            switch (page)
            {
                case 0:
                    title = "LEVEL 22 | One Way Only | Circuit Overview";
                    caption = "PAGE 1/4 | 5 V | SWITCH | 330 OHM | DIODE | GREEN LED";
                    explanation =
                        "ILLUSTRATIVE / NOT LIVE VERIFIED. One series current path: Battery_positive -> SW1 -> " +
                        "R1 330 Ohm -> D1 anode -> D1 banded cathode -> green LED anode -> green LED cathode " +
                        "-> Battery_negative. The diode permits forward current and blocks reverse current. " +
                        "No direct parallel bypass wire should connect across the diode or LED.";
                    break;
                case 1:
                    title = "LEVEL 22 | Exact Five Electrical Wires";
                    caption = "PAGE 2/4 | WIRE TERMINAL TO TERMINAL | DIODE BAND TOWARD LED";
                    explanation =
                        "CONNECT THESE FIVE WIRES: (1) Battery_positive -> Switch Input. " +
                        "(2) Switch Output -> R_LIMIT_Input. (3) R_LIMIT_Output -> D_PROTECT_Anode. " +
                        "(4) D_PROTECT_Cathode -> LED_INDICATOR_Anode. " +
                        "(5) LED_INDICATOR_Cathode -> Battery_negative. " +
                        "These are runtime binding IDs; use the actual SparkTerminal GameObjects when drawing wires. " +
                        "The banded diode cathode points toward the LED anode. " +
                        "If Battery_positive -> switch is prewired, the player places only the four remaining wires.";
                    break;
                case 2:
                    title = "LEVEL 22 | Forward vs Reverse Bias";
                    caption = "PAGE 3/4 | FORWARD = GREEN LED ON | REVERSE = OFF";
                    explanation =
                        "FORWARD TEST: D1 anode toward the 330 Ohm resistor and banded cathode toward LED anode. " +
                        "With switch closed, conventional current flows and green LED illuminates. " +
                        "REVERSE TEST: Power off before swapping D1 wires. Connect resistor to D1 cathode and " +
                        "D1 anode to LED anode; current should be blocked and LED should stay off. " +
                        "The schematic depicts the forward-bias SOLUTION; reverse bias is an experiment, not " +
                        "an additional simultaneous electrical branch.";
                    break;
                default:
                    title = "LEVEL 22 | Correct Solution & Four Targets";
                    caption = "PAGE 4/4 | 5 V | DIODE 0.45-1.0 V | LED ON | 5-8 mA";
                    explanation =
                        "DESIGN ESTIMATES / NOT LIVE SOLVER READINGS. With approximately 0.7 V diode drop, " +
                        "2.1 V green LED drop and 330 Ohm resistor, series current is approximately 6.67 mA. " +
                        "Targets: 5 V supply between 4.8 and 5.2 V; forward diode drop 0.45 to 1.0 V; " +
                        "green LED ON; resistor current 5 to 8 mA. " +
                        "Required runtime bindings: D_PROTECT_Component / D_PROTECT_Anode / D_PROTECT_Cathode; " +
                        "R_LIMIT_Component / R_LIMIT_Input / R_LIMIT_Output; " +
                        "LED_INDICATOR_Component / LED_INDICATOR_Anode / LED_INDICATOR_Cathode. " +
                        "Only SparkLevelEvaluator awards 4/4; verify actual currents with the electrical audit.";
                    break;
            }

            return new SparkLevelDrawingPage
            {
                title = title,
                caption = caption,
                explanation = explanation,
                parts = parts,
                wires = wires
            };
        }
    }
}
#endif
