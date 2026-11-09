#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;
using ProjectSpark.UI.VisualTeaching;

namespace ProjectSpark.EditorTools
{
    /// <summary>
    /// Creates four native SparkLevelDrawingAsset teaching pages for Level 23.
    /// Diagram only: does not modify circuit topology, prefabs, solver, bindings, or levels.
    /// Based on the Level 22 drawing creator's native Visual Teaching asset schema.
    /// </summary>
    public static class SparkLevel23DrawingAssetCreator
    {
        private const string Menu =
            "Tools/Project Spark/Visual Teaching/Create Level 23 Drawing Asset";

        private static readonly Color Red = new Color(.98f, .29f, .35f, 1f);
        private static readonly Color Blue = new Color(.19f, .74f, 1f, 1f);
        private static readonly Color Gold = new Color(1f, .78f, .28f, 1f);
        private static readonly Color Green = new Color(.30f, .98f, .59f, 1f);
        private static readonly Color Cyan = new Color(.28f, .93f, .98f, 1f);

        [MenuItem(Menu)]
        public static void CreateAsset()
        {
            string path = EditorUtility.SaveFilePanelInProject(
                "Create Level 23 Drawing", "Level_23_Drawing", "asset",
                "Choose where to save the four-page Level 23 teaching asset.");
            if (string.IsNullOrEmpty(path)) return;

            if (AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path) != null)
            {
                EditorUtility.DisplayDialog("Level 23 Drawing",
                    "An asset already exists at this path. Choose another name to preserve existing pages.",
                    "OK");
                return;
            }

            SparkLevelDrawingAsset drawing =
                ScriptableObject.CreateInstance<SparkLevelDrawingAsset>();
            drawing.levelId = "level_23";
            drawing.levelTitle = "Triple Barrier - Three Diodes in Series";
            drawing.initialPage = 0;
            drawing.pages = new SparkLevelDrawingPage[4];
            for (int i = 0; i < drawing.pages.Length; i++)
                drawing.pages[i] = BuildPage(i);

            AssetDatabase.CreateAsset(drawing, path);
            EditorUtility.SetDirty(drawing);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Selection.activeObject = drawing;
            EditorGUIUtility.PingObject(drawing);
            Debug.Log("[Level 23 Drawing] Created four teaching pages at " + path);
        }

        private static Vector2 P(float x, float y) { return new Vector2(x, y); }

        private static SparkSchematicKind KindOrGeneric(string name)
        {
            SparkSchematicKind kind;
            return Enum.TryParse(name, true, out kind) &&
                   Enum.IsDefined(typeof(SparkSchematicKind), kind)
                ? kind : SparkSchematicKind.Generic;
        }

        private static SparkSchematicPart Part(
            string id, SparkSchematicKind kind,
            float x, float y, string label, Color color)
        {
            return new SparkSchematicPart
            {
                id = id,
                kind = kind,
                position = P(x, y),
                label = label,
                color = color
            };
        }

        private static SparkSchematicWire Wire(Color color, params Vector2[] points)
        {
            return new SparkSchematicWire
            {
                color = color,
                energized = false,
                points = points
            };
        }

        private static SparkLevelDrawingPage BuildPage(int page)
        {
            // Native normalized drawing coordinates.
            // Each horizontal symbol is treated as approximately center +/- .045.
            // Ordered seven connection segments, no parallel bypass path:
            // Battery+ -> Switch -> R1 -> D1 -> D2 -> D3 -> LED -> Battery-.
            SparkSchematicPart[] parts =
            {
                Part("battery", SparkSchematicKind.Battery, .07f, .53f, "5 V", Red),
                Part("sw1", SparkSchematicKind.Switch, .20f, .79f, "SW1", Gold),
                Part("r1", SparkSchematicKind.Resistor, .33f, .79f, "R1 150 Ohm", Gold),
                Part("d1", KindOrGeneric("Diode"), .46f, .79f, "D1 A >| K", Cyan),
                Part("d2", KindOrGeneric("Diode"), .59f, .79f, "D2 A >| K", Cyan),
                Part("d3", KindOrGeneric("Diode"), .72f, .79f, "D3 A >| K", Cyan),
                Part("led1", KindOrGeneric("LED"), .87f, .79f, "GREEN LED A >| K", Green),
                Part("gnd", SparkSchematicKind.Ground, .50f, .17f, "BATTERY NEGATIVE / GND", Blue)
            };

            SparkSchematicWire[] wires =
            {
                // 1: supply positive to switch input.
                Wire(Red, P(.07f,.66f), P(.07f,.79f), P(.155f,.79f)),
                // 2: switch output to resistor input.
                Wire(Red, P(.245f,.79f), P(.285f,.79f)),
                // 3: resistor output to first diode anode.
                Wire(Red, P(.375f,.79f), P(.415f,.79f)),
                // 4: diode 1 cathode to diode 2 anode.
                Wire(Red, P(.505f,.79f), P(.545f,.79f)),
                // 5: diode 2 cathode to diode 3 anode.
                Wire(Red, P(.635f,.79f), P(.675f,.79f)),
                // 6: diode 3 cathode to LED anode.
                Wire(Red, P(.765f,.79f), P(.825f,.79f)),
                // 7: LED cathode to supply negative via bottom return.
                Wire(Blue, P(.915f,.79f), P(.96f,.79f), P(.96f,.17f),
                    P(.50f,.17f), P(.07f,.17f), P(.07f,.40f))
            };

            string title, caption, explanation;
            switch (page)
            {
                case 0:
                    title = "LEVEL 23 | Triple Barrier | Overview";
                    caption = "PAGE 1/4 | 5 V | SWITCH | 150 OHM | THREE DIODES | LED";
                    explanation =
                        "TEACHING SCHEMATIC, NOT LIVE MEASUREMENTS. Wire one continuous series path: " +
                        "Battery positive -> switch -> 150 Ohm resistor -> D1 -> D2 -> D3 -> " +
                        "green LED -> battery negative. Point the banded cathode end of all " +
                        "three diodes toward the LED. Do not bypass any diode.";
                    break;
                case 1:
                    title = "LEVEL 23 | Seven Electrical Wire Connections";
                    caption = "PAGE 2/4 | CONNECT IN ORDER | THREE FORWARD DIODES";
                    explanation =
                        "THE SEVEN WIRES: (1) Battery_positive -> SwitchInput. " +
                        "(2) SwitchOutput -> R_LIMIT_Input. " +
                        "(3) R_LIMIT_Output -> D_SERIES_1_Anode. " +
                        "(4) D_SERIES_1_Cathode -> D_SERIES_2_Anode. " +
                        "(5) D_SERIES_2_Cathode -> D_SERIES_3_Anode. " +
                        "(6) D_SERIES_3_Cathode -> LED_Anode. " +
                        "(7) LED_Cathode -> Battery_negative. " +
                        "These are logical binding IDs; connect the corresponding SparkTerminal " +
                        "objects in the scene. If a source-to-switch wire is preinstalled, the " +
                        "player only needs to place the remaining wires.";
                    break;
                case 2:
                    title = "LEVEL 23 | Combined Diode Voltage Drop";
                    caption = "PAGE 3/4 | D1 + D2 + D3 APPROXIMATELY 2.1 V";
                    explanation =
                        "Each forward-biased 1N4007 has an approximate 0.7 V drop, for a " +
                        "combined diode drop of 0.7 + 0.7 + 0.7 = 2.1 V. " +
                        "With an approximately 2.1 V green LED drop and a 5 V supply, " +
                        "the 150 Ohm resistor has approximately 0.8 V and the series " +
                        "current is approximately 5.33 mA. Actual results depend on " +
                        "the configured diode and LED models.";
                    break;
                default:
                    title = "LEVEL 23 | Solution and Six Objectives";
                    caption = "PAGE 4/4 | 5 V | D1/D2/D3 FORWARD | LED ON | SAFE CURRENT";
                    explanation =
                        "TARGETS 6/6: supply 4.8-5.2 V; D1 forward voltage 0.45-1.0 V; " +
                        "D2 forward voltage 0.45-1.0 V; D3 forward voltage 0.45-1.0 V; " +
                        "green LED ON; resistor series current 4.5-6.5 mA. " +
                        "Use 150 Ohm in Slot_01. Diode binding IDs: D_SERIES_1_Component, " +
                        "D_SERIES_1_Anode, D_SERIES_1_Cathode; D_SERIES_2_Component, " +
                        "D_SERIES_2_Anode, D_SERIES_2_Cathode; D_SERIES_3_Component, " +
                        "D_SERIES_3_Anode, D_SERIES_3_Cathode. " +
                        "Fixed LED IDs: led, LED_Anode, LED_Cathode. " +
                        "Only SparkLevelEvaluator can award 6/6 after a valid solve; " +
                        "the drawing itself does not validate wiring.";
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
