#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;
using ProjectSpark.UI.VisualTeaching;

namespace ProjectSpark.EditorTools
{
    /// <summary>Creates Level 26 four-page teaching asset. No wiring, solver, or prefab changes.</summary>
    public static class SparkLevel26DrawingAssetCreator
    {
        private const string Menu = "Tools/Project Spark/Visual Teaching/Create Level 26 Drawing Asset";
        private static readonly Color Red = new Color(.98f,.29f,.35f,1f);
        private static readonly Color Blue = new Color(.19f,.74f,1f,1f);
        private static readonly Color Gold = new Color(1f,.78f,.28f,1f);
        private static readonly Color Green = new Color(.30f,.98f,.59f,1f);
        private static readonly Color Cyan = new Color(.28f,.93f,.98f,1f);

        [MenuItem(Menu)]
        public static void CreateAsset()
        {
            string path = EditorUtility.SaveFilePanelInProject(
                "Create Level 26 Drawing", "Level_26_Drawing", "asset",
                "Save the four-page Level 26 Visual Teaching asset.");
            if (string.IsNullOrEmpty(path)) return;
            if (AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path) != null)
            {
                EditorUtility.DisplayDialog("Level 26 Drawing", "An asset already exists here. Choose another name to avoid overwriting authored pages.", "OK");
                return;
            }
            SparkLevelDrawingAsset drawing = ScriptableObject.CreateInstance<SparkLevelDrawingAsset>();
            drawing.levelId = "level_26";
            drawing.levelTitle = "Bridge Defender - Four Diode Rectifier";
            drawing.initialPage = 0;
            drawing.pages = new SparkLevelDrawingPage[4];
            for (int i = 0; i < drawing.pages.Length; i++) drawing.pages[i] = BuildPage(i);
            AssetDatabase.CreateAsset(drawing, path);
            EditorUtility.SetDirty(drawing);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Selection.activeObject = drawing;
            EditorGUIUtility.PingObject(drawing);
            Debug.Log("[Level 26 Drawing] Saved 4 pages: " + path);
        }
        private static Vector2 P(float x, float y) { return new Vector2(x, y); }
        private static SparkSchematicKind KindOrGeneric(string value)
        {
            SparkSchematicKind kind;
            return Enum.TryParse(value, true, out kind) &&
                Enum.IsDefined(typeof(SparkSchematicKind), kind)
                ? kind : SparkSchematicKind.Generic;
        }
        private static SparkSchematicPart Part(string id, SparkSchematicKind kind,
            float x, float y, string label, Color color)
        {
            return new SparkSchematicPart { id=id, kind=kind, position=P(x,y), label=label, color=color };
        }
        private static SparkSchematicWire Wire(Color c, params Vector2[] points)
        {
            return new SparkSchematicWire { color=c, energized=false, points=points };
        }
        private static SparkLevelDrawingPage BuildPage(int page)
        {
            // Teaching representation of four bridge NODES:
            // A = switch output, B = battery negative,
            // P = resistor input, N = LED cathode.
            // D1: A -> P; D2: B -> P; D3: N -> A; D4: N -> B.
            // Normal input polarity conducts through D1 and D4.
            SparkSchematicPart[] parts = {
                Part("battery", SparkSchematicKind.Battery, .09f,.48f,"5 V",Red),
                Part("sw1", SparkSchematicKind.Switch,.25f,.76f,"SW1",Gold),
                Part("d1",KindOrGeneric("Diode"),.43f,.84f,"D1 A >| K  ON",Green),
                Part("d2",KindOrGeneric("Diode"),.63f,.84f,"D2 A >| K  OFF",Cyan),
                Part("d3",KindOrGeneric("Diode"),.43f,.39f,"D3 K |< A  OFF",Cyan),
                Part("d4",KindOrGeneric("Diode"),.63f,.39f,"D4 K |< A  ON",Green),
                Part("r1",SparkSchematicKind.Resistor,.85f,.75f,"R1 220 Ohm",Gold),
                Part("led1",KindOrGeneric("LED"),.85f,.51f,"LED GREEN",Green),
                Part("gnd",SparkSchematicKind.Ground,.15f,.14f,"Battery negative",Blue)
            };
            // Schematic wires are visual, not SparkCircuitConnection objects.
            // Connect explicit bridge nodes using multi-point polylines.
            SparkSchematicWire[] wires = {
                Wire(Red,P(.09f,.63f),P(.09f,.76f),P(.19f,.76f)), // battery -> switch
                Wire(Red,P(.31f,.76f),P(.35f,.76f),P(.35f,.84f),P(.37f,.84f)), // A -> D1 anode
                Wire(Red,P(.35f,.76f),P(.35f,.39f),P(.37f,.39f)), // A -> D3 cathode
                Wire(Blue,P(.09f,.33f),P(.15f,.33f),P(.15f,.91f),P(.57f,.91f),P(.57f,.84f)), // B -> D2 anode
                Wire(Blue,P(.15f,.33f),P(.57f,.33f),P(.57f,.39f)), // B -> D4 cathode
                Wire(Red,P(.49f,.84f),P(.79f,.84f),P(.79f,.75f)), // D1 cathode -> load+
                Wire(Red,P(.69f,.84f),P(.79f,.84f)), // D2 cathode -> load+
                Wire(Red,P(.85f,.69f),P(.85f,.57f)), // resistor -> LED anode
                Wire(Blue,P(.85f,.45f),P(.76f,.45f),P(.76f,.39f),P(.49f,.39f)), // LED cathode -> D3 anode
                Wire(Blue,P(.76f,.45f),P(.69f,.39f)) // LED cathode -> D4 anode
            };
            string title, caption, explanation;
            switch(page)
            {
                case 0:
                    title="LEVEL 26 | Bridge Defender | Four Diodes";
                    caption="PAGE 1/4 | 5 V | FOUR 1N4007 | 220 OHM | GREEN LED";
                    explanation="A bridge rectifier uses four diodes to keep the load polarity consistent. The DC-input teaching solution follows: battery positive -> switch -> D1 anode -> D1 cathode -> R1 -> green LED -> D4 anode -> D4 cathode -> battery negative. D2 and D3 are reverse-biased and block. The visual layout is illustrative, not live solver output.";
                    break;
                case 1:
                    title="LEVEL 26 | Exact Ten Wire Connections";
                    caption="PAGE 2/4 | WIRE NODES A, B, P, N | SHARED JUNCTIONS";
                    explanation="CONNECT 10 EXTERNAL WIRES: (1) Battery_positive -> SwitchInput; (2) SwitchOutput -> D_BRIDGE_1_Anode; (3) SwitchOutput -> D_BRIDGE_3_Cathode; (4) Battery_negative -> D_BRIDGE_2_Anode; (5) Battery_negative -> D_BRIDGE_4_Cathode; (6) D_BRIDGE_1_Cathode -> R_LIMIT_Input; (7) D_BRIDGE_2_Cathode -> R_LIMIT_Input; (8) R_LIMIT_Output -> LED_Anode; (9) LED_Cathode -> D_BRIDGE_3_Anode; (10) LED_Cathode -> D_BRIDGE_4_Anode. Wire terminal to terminal; do not confuse diode orientation with drawing position.";
                    break;
                case 2:
                    title="LEVEL 26 | Which Diodes Conduct?";
                    caption="PAGE 3/4 | D1 + D4 ON | D2 + D3 OFF";
                    explanation="Normal polarity: input A=SwitchOutput is positive with respect to input B=Battery_negative. D1 runs from A to bridge output P; D4 returns from bridge output N to B. D2 runs from B to P and D3 runs from N to A, so both block. With a reversed DC input, the conducting pair swaps. This level evaluates only normal input polarity; do not reverse the battery in gameplay unless the level supports it.";
                    break;
                default:
                    title="LEVEL 26 | Correct Solution and Six Targets";
                    caption="PAGE 4/4 | 5 V | D1/D4 0.45-1 V | LED ON | 6-9 mA";
                    explanation="DESIGN ESTIMATES, NOT MEASURED: 5 V supply; approximately 0.7 V across each conducting diode D1 and D4; about 2.0 V across green LED; about 1.6 V across 220 Ohm resistor; current about 7.3 mA. Targets: supply 4.8-5.2 V, D1 drop 0.45-1 V, D4 drop 0.45-1 V, LED ON, resistor current 6-9 mA, resistor drop 1.3-1.9 V. Declare runtime IDs D_BRIDGE_1 through D_BRIDGE_4 and R_LIMIT; fixed LED IDs remain led, LED_Anode, LED_Cathode. Only the level evaluator awards 6/6.";
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
