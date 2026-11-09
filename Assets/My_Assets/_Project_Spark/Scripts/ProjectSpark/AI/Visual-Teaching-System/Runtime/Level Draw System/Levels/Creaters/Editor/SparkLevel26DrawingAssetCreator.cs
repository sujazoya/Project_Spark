#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;
using ProjectSpark.UI.VisualTeaching;

namespace ProjectSpark.EditorTools
{
    /// <summary>
    /// Corrected Level 26 teaching drawing, arranged like the workshop:
    /// source/switch left, four diodes in diamond, R220 + LED right.
    /// Visual only: does not make SparkCircuitConnections.
    /// </summary>
    public static class SparkLevel26DrawingAssetCreator
    {
        private const string Menu = "Tools/Project Spark/Visual Teaching/Create Level 26 Drawing Asset";
        private static readonly Color Red = new Color(.98f,.27f,.31f,1f);
        private static readonly Color Blue = new Color(.21f,.72f,1f,1f);
        private static readonly Color Gold = new Color(1f,.78f,.31f,1f);
        private static readonly Color Green = new Color(.30f,.98f,.59f,1f);
        private static readonly Color Cyan = new Color(.28f,.90f,1f,1f);

        [MenuItem(Menu)]
        public static void CreateAsset()
        {
            string path = EditorUtility.SaveFilePanelInProject(
                "Create / Update Level 26 Drawing", "Level_26_Drawing", "asset",
                "Choose the Level 26 drawing asset. Selecting an existing asset updates it in place.");
            if (string.IsNullOrEmpty(path)) return;

            SparkLevelDrawingAsset drawing = AssetDatabase.LoadAssetAtPath<SparkLevelDrawingAsset>(path);
            if (drawing == null)
            {
                if (AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path) != null)
                {
                    EditorUtility.DisplayDialog("Level 26", "The selected path contains another asset type.", "OK");
                    return;
                }
                drawing = ScriptableObject.CreateInstance<SparkLevelDrawingAsset>();
                AssetDatabase.CreateAsset(drawing, path);
            }
            else
            {
                if (!EditorUtility.DisplayDialog("Update Level 26 Drawing",
                    "Replace the existing drawing's four pages? Its asset GUID will be preserved.",
                    "Replace pages", "Cancel")) return;
                Undo.RecordObject(drawing, "Update Level 26 schematic pages");
            }

            drawing.levelId = "level_26";
            drawing.levelTitle = "Bridge Defender - Four Diode Rectifier";
            drawing.initialPage = 0;
            drawing.pages = new SparkLevelDrawingPage[4];
            for (int i = 0; i < 4; i++) drawing.pages[i] = BuildPage(i);
            EditorUtility.SetDirty(drawing);
            AssetDatabase.SaveAssets();
            Selection.activeObject = drawing;
            EditorGUIUtility.PingObject(drawing);
            Debug.Log("[Level 26 Drawing] Saved corrected four-page workshop schematic: " + path);
        }

        private static Vector2 P(float x, float y) => new Vector2(x,y);
        private static SparkSchematicKind Kind(string text)
        {
            return Enum.TryParse(text, true, out SparkSchematicKind kind) &&
                Enum.IsDefined(typeof(SparkSchematicKind),kind)
                ? kind : SparkSchematicKind.Generic;
        }
        private static SparkSchematicPart Part(string id, SparkSchematicKind kind,
            float x, float y, string label, Color color) =>
            new SparkSchematicPart { id=id, kind=kind, position=P(x,y), label=label, color=color };
        private static SparkSchematicWire Wire(Color color, params Vector2[] points) =>
            new SparkSchematicWire { color=color, energized=false, points=points };

        private static SparkLevelDrawingPage BuildPage(int page)
        {
            // Project Spark wire graph in a diamond; numbers correspond to
            // 10 external physical wires, not hypothetical component-internal wires.
            // A (left)  = SwitchOutput, D1 anode, D3 cathode.
            // B (right) = Battery_negative, D2 anode, D4 cathode.
            // P (top)   = D1 cathode, D2 cathode, R input.
            // N (bottom)= D3 anode, D4 anode, LED cathode.
            // Current at normal polarity: A -> D1 -> P -> R -> LED -> N -> D4 -> B.

            SparkSchematicPart[] parts = {
                Part("battery", SparkSchematicKind.Battery,.075f,.53f,"5 V",Red),
                Part("switch", SparkSchematicKind.Switch,.22f,.54f,"POWER SWITCH",Gold),
                Part("d1",Kind("Diode"),.44f,.69f,"D1  A >| K  ON",Green),
                Part("d2",Kind("Diode"),.62f,.69f,"D2  K |< A  OFF",Cyan),
                Part("d3",Kind("Diode"),.44f,.36f,"D3  K |< A  OFF",Cyan),
                Part("d4",Kind("Diode"),.62f,.36f,"D4  A >| K  ON",Green),
                Part("r1",SparkSchematicKind.Resistor,.85f,.71f,"R1  220 OHM",Gold),
                Part("led",Kind("LED"),.86f,.48f,"LED GREEN",Green),
                Part("nodeA",SparkSchematicKind.Generic,.345f,.54f,"A / DC INPUT +",Red),
                Part("nodeB",SparkSchematicKind.Generic,.715f,.54f,"B / DC INPUT -",Blue),
                Part("nodeP",SparkSchematicKind.Generic,.53f,.84f,"P / LOAD +",Red),
                Part("nodeN",SparkSchematicKind.Generic,.53f,.19f,"N / LOAD -",Blue)
            };

            // Exact connections expressed as separate polyline strokes. Each diode's
            // two contacts are visually separated. The four net-junction labels
            // show which contacts share the same electrical node.
            SparkSchematicWire[] wires = {
                Wire(Red,P(.10f,.59f),P(.16f,.59f),P(.16f,.54f)), // 1 battery+ -> switch input
                Wire(Red,P(.28f,.54f),P(.345f,.54f),P(.385f,.64f)), // 2 switch -> D1 A
                Wire(Red,P(.345f,.54f),P(.385f,.40f)), // 3 switch -> D3 K
                Wire(Blue,P(.075f,.45f),P(.075f,.11f),P(.715f,.11f),P(.715f,.54f),P(.675f,.64f)), // 4 battery- -> D2 A
                Wire(Blue,P(.715f,.54f),P(.675f,.40f)), // 5 battery- -> D4 K
                Wire(Red,P(.495f,.74f),P(.53f,.84f),P(.85f,.84f),P(.85f,.76f)), // 6 D1 K -> R input
                Wire(Red,P(.565f,.74f),P(.53f,.84f)), // 7 D2 K -> R input
                Wire(Red,P(.85f,.65f),P(.86f,.54f)), // 8 R output -> LED anode
                Wire(Blue,P(.86f,.42f),P(.86f,.19f),P(.53f,.19f),P(.385f,.30f)), // 9 LED cathode -> D3 A
                Wire(Blue,P(.53f,.19f),P(.565f,.30f)) // 10 LED cathode -> D4 A
            };

            string title, caption, explanation;
            switch (page)
            {
                case 0:
                    title = "LEVEL 26 | ORIGINAL BENCH LAYOUT | BRIDGE";
                    caption = "PAGE 1/4 | SOURCE + SWITCH LEFT | FOUR DIODES CENTER | R + LED RIGHT";
                    explanation = "The four diodes make a diamond on the workbench. Junction A is switch output, B is battery negative, P is load positive, N is load negative. Red and blue lines show separate nets; crossings do not connect unless they meet at a marked junction. The physical position of a diode is not its electrical ID: read D_BRIDGE_1..4 from Smart Slot bindings.";
                    break;
                case 1:
                    title = "LEVEL 26 | NUMBERED WIRING | 10 CONNECTIONS";
                    caption = "PAGE 2/4 | FOLLOW TERMINAL NAMES, NOT VISUAL ORIENTATION";
                    explanation = "1 Battery_positive -> SwitchInput; 2 SwitchOutput -> D_BRIDGE_1_Anode; 3 SwitchOutput -> D_BRIDGE_3_Cathode; 4 Battery_negative -> D_BRIDGE_2_Anode; 5 Battery_negative -> D_BRIDGE_4_Cathode; 6 D_BRIDGE_1_Cathode -> R_LIMIT_Input; 7 D_BRIDGE_2_Cathode -> R_LIMIT_Input; 8 R_LIMIT_Output -> LED_Anode; 9 LED_Cathode -> D_BRIDGE_3_Anode; 10 LED_Cathode -> D_BRIDGE_4_Anode. In your last reported layout D1=Slot_02, D3=Slot_03, D2=Slot_04, D4=Slot_05.";
                    break;
                case 2:
                    title = "LEVEL 26 | CONDUCTING DIODES";
                    caption = "PAGE 3/4 | NORMAL 5 V POLARITY | D1 AND D4 CONDUCT";
                    explanation = "Correct path: battery+ -> switch -> D1 -> 220 ohm resistor -> green LED -> D4 -> battery-. D2 and D3 are reverse-biased. Do not directly wire battery- to LED cathode or switch output to battery-. Those connections bypass the intended bridge load.";
                    break;
                default:
                    title = "LEVEL 26 | VERIFIED CIRCUIT MEASUREMENTS";
                    caption = "PAGE 4/4 | SOURCE 5.000 V | CURRENT 7.230 mA | NO OVERLOAD";
                    explanation = "Observed in your solved-source audit: R220 voltage 1.590594 V and current 7.230 mA; LED 2.007230 V and 7.230 mA; D1 and D4 around 0.700723 V and conducting; D2 and D3 reverse-blocking. The level evaluator alone decides target completion.";
                    break;
            }
            return new SparkLevelDrawingPage {
                title=title, caption=caption, explanation=explanation,
                parts=parts, wires=wires
            };
        }
    }
}
#endif
