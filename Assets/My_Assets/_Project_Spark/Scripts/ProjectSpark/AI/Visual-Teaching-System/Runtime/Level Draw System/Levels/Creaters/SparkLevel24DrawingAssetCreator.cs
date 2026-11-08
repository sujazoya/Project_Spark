#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;
using ProjectSpark.UI.VisualTeaching;

namespace ProjectSpark.EditorTools
{
    /// <summary>
    /// Creates four native SparkLevelDrawingAsset pages for Level 24.
    /// Uses the existing Project Spark schematic renderer.
    /// Teaching artwork only: never changes wiring or electrical state.
    /// </summary>
    public static class SparkLevel24DrawingAssetCreator
    {
        private const string Menu =
            "Tools/Project Spark/Visual Teaching/Create Level 24 Drawing Asset";

        private static readonly Color Red   = new Color(.98f, .29f, .35f, 1f);
        private static readonly Color Blue  = new Color(.19f, .74f, 1f, 1f);
        private static readonly Color Gold  = new Color(1f, .78f, .28f, 1f);
        private static readonly Color Green = new Color(.30f, .98f, .59f, 1f);
        private static readonly Color Cyan  = new Color(.28f, .93f, .98f, 1f);
        private static readonly Color Amber = new Color(1f, .62f, .25f, 1f);

        [MenuItem(Menu)]
        public static void CreateAsset()
        {
            string path = EditorUtility.SaveFilePanelInProject(
                "Create Level 24 Drawing",
                "Level_24_Drawing",
                "asset",
                "Choose where to save the four-page Level 24 teaching drawing asset.");

            if (string.IsNullOrEmpty(path))
                return;

            if (AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path) != null)
            {
                EditorUtility.DisplayDialog(
                    "Level 24 Drawing",
                    "An asset already exists at that path. Choose a new filename to avoid overwriting authored pages.",
                    "OK");
                return;
            }

            SparkLevelDrawingAsset drawing =
                ScriptableObject.CreateInstance<SparkLevelDrawingAsset>();

            drawing.levelId = "level_24";
            drawing.levelTitle = "Opposite Directions - Diode Challenge";
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

            Debug.Log("[Level 24 Drawing] Created 4 teaching pages at " + path);
        }

        private static Vector2 P(float x, float y) => new Vector2(x, y);

        private static SparkSchematicPart Part(
            string id, SparkSchematicKind kind, float x, float y,
            string label, Color color)
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

        // Older Project Spark schematic enums may not have Diode/LED.
        // Generic provides a compatible labeled symbol in that case.
        private static SparkSchematicKind KindOrGeneric(string kindName)
        {
            SparkSchematicKind kind;
            return Enum.TryParse(kindName, true, out kind) &&
                   Enum.IsDefined(typeof(SparkSchematicKind), kind)
                ? kind
                : SparkSchematicKind.Generic;
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
            // NORMALIZED CONCEPTUAL TOPOLOGY:
            // Battery+ -> SW1 -> R1 -> SPLIT (left junction)
            //   Upper: D1 anode -> D1 cathode -> MERGE (right junction)
            //   Lower: D2 cathode -> D2 anode -> MERGE (right junction)
            // MERGE -> green LED anode -> LED cathode -> Battery-.
            //
            // Seven external electrical connections. The branching drawn here
            // must be supported by the runtime wire/terminal connection rules.
            //
            // D2 is intentionally Generic so no forward-facing diode symbol
            // accidentally implies the opposite polarity. Its label defines
            // K (left) and A (right) unambiguously.
            SparkSchematicPart[] parts =
            {
                Part("battery", SparkSchematicKind.Battery,
                    .09f, .52f, "5 V", Red),
                Part("sw1", SparkSchematicKind.Switch,
                    .26f, .78f, "SW1", Gold),
                Part("r1", SparkSchematicKind.Resistor,
                    .44f, .78f, "R1 330 Ohm", Gold),
                Part("d1", KindOrGeneric("Diode"),
                    .67f, .78f, "D1 FORWARD  A >| K", Cyan),
                Part("d2", SparkSchematicKind.Generic,
                    .67f, .54f, "D2 REVERSE  K |< A", Amber),
                Part("led1", KindOrGeneric("LED"),
                    .90f, .78f, "LED1 GREEN  A >| K", Green),
                Part("gnd", SparkSchematicKind.Ground,
                    .55f, .17f, "BATTERY NEGATIVE / GND", Blue)
            };

            SparkSchematicWire[] wires =
            {
                // 1. Battery positive -> switch input.
                Wire(Red, P(.09f,.66f), P(.09f,.78f), P(.20f,.78f)),
                // 2. Switch output -> resistor input.
                Wire(Red, P(.32f,.78f), P(.38f,.78f)),
                // 3. Resistor output -> D1 anode (upper branch).
                Wire(Red, P(.50f,.78f), P(.55f,.78f), P(.61f,.78f)),
                // 4. Resistor output junction -> D2 cathode (lower branch).
                Wire(Amber, P(.55f,.78f), P(.55f,.54f), P(.61f,.54f)),
                // 5. D1 cathode -> LED anode, with merge at x=.79.
                Wire(Red, P(.73f,.78f), P(.79f,.78f), P(.84f,.78f)),
                // 6. D2 anode -> same LED anode junction (lower branch).
                Wire(Amber, P(.73f,.54f), P(.79f,.54f), P(.79f,.78f)),
                // 7. LED cathode -> battery negative.
                Wire(Blue, P(.96f,.78f), P(.98f,.78f), P(.98f,.17f),
                    P(.55f,.17f), P(.09f,.17f), P(.09f,.38f))
            };

            string title, caption, explanation;
            switch (page)
            {
                case 0:
                    title = "LEVEL 24 | Opposite Directions | Circuit Overview";
                    caption = "PAGE 1/4 | 5 V | SWITCH | 330 OHM | 2 OPPOSING DIODES | GREEN LED";
                    explanation =
                        "ILLUSTRATIVE / NOT LIVE VERIFIED. One 5 V supply and switch feed R1 330 Ohm, " +
                        "then split into two parallel branches. The upper branch is D1 forward: " +
                        "anode at the resistor side, banded cathode toward the green LED. " +
                        "The lower branch is D2 reverse: banded cathode at the resistor side, " +
                        "anode toward the LED. Both paths join before the LED. " +
                        "The LED cathode returns to Battery_negative. D1 conducts; D2 blocks. " +
                        "Never bypass the resistor, LED, or both diodes with a direct wire.";
                    break;

                case 1:
                    title = "LEVEL 24 | Exact Seven Electrical Connections";
                    caption = "PAGE 2/4 | TWO BRANCHES | SHARED START + SHARED END";
                    explanation =
                        "CONNECT SEVEN WIRES: (1) Battery_positive -> SwitchInput. " +
                        "(2) SwitchOutput -> R_LIMIT_Input. " +
                        "(3) R_LIMIT_Output -> D_FORWARD_Anode. " +
                        "(4) R_LIMIT_Output -> D_REVERSE_Cathode. " +
                        "(5) D_FORWARD_Cathode -> LED_Anode. " +
                        "(6) D_REVERSE_Anode -> LED_Anode. " +
                        "(7) LED_Cathode -> Battery_negative. " +
                        "Runtime binding IDs identify the physical SparkTerminals. " +
                        "The circuit requires multiple connections at the branch terminals, " +
                        "or explicit junctions supported by the wire system. " +
                        "If the first battery-to-switch wire is preinstalled, the player adds six.";
                    break;

                case 2:
                    title = "LEVEL 24 | Forward Conducts, Reverse Blocks";
                    caption = "PAGE 3/4 | D1 A >| K | D2 K |< A";
                    explanation =
                        "The two diode branches are in parallel between the same pair of nodes. " +
                        "D1 points from resistor toward LED: forward bias, approximately +0.7 V " +
                        "from D_FORWARD_Anode to D_FORWARD_Cathode, and positive branch current. " +
                        "D2 points the opposite way: D_REVERSE_Cathode at the high-potential side, " +
                        "D_REVERSE_Anode at the low-potential side, approximately -0.7 V signed " +
                        "anode-to-cathode voltage and near-zero reverse current. " +
                        "The reverse Generic schematic symbol is deliberately labeled K |< A " +
                        "rather than drawing a possibly forward-facing built-in diode icon.";
                    break;

                default:
                    title = "LEVEL 24 | Correct Solution & Six Targets";
                    caption = "PAGE 4/4 | D1 ON | D2 BLOCKED | GREEN LED ON | 5-8 mA";
                    explanation =
                        "DESIGN ESTIMATES / NOT LIVE SOLVER READINGS: Supply approximately 5 V; " +
                        "R1 330 Ohm drop around 2.3 V; D1 around +0.7 V; D2 around -0.7 V " +
                        "signed anode-to-cathode voltage; LED around 2.0 V; current about 6.9 mA. " +
                        "Level targets: supply 4.8-5.2 V, D1 voltage 0.45-1.0 V, " +
                        "D2 current 0-0.05 mA, green LED ON, resistor current 5-8 mA, " +
                        "resistor voltage 2.0-2.6 V. " +
                        "Workstation bindings: R_LIMIT_Component / R_LIMIT_Input / R_LIMIT_Output; " +
                        "D_FORWARD_Component / D_FORWARD_Anode / D_FORWARD_Cathode; " +
                        "D_REVERSE_Component / D_REVERSE_Anode / D_REVERSE_Cathode. " +
                        "Fixed LED binding: led / LED_Anode / LED_Cathode. " +
                        "Verify that evaluator CurrentRange accepts zero current for D2. " +
                        "Only SparkLevelEvaluator can award 6/6; drawings do not certify wiring.";
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
