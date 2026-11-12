#if UNITY_EDITOR
using System.Collections.Generic;
using ProjectSpark.UI.VisualTeaching;
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.EditorTools
{
    public static class SparkLevel37DrawingAssetCreator
    {
        private static readonly Color Positive = new Color(.98f, .35f, .42f);
        private static readonly Color Force = new Color(.99f, .73f, .31f);
        private static readonly Color Return = new Color(.28f, .69f, 1f);
        private static readonly Color Sense = new Color(.24f, .87f, .80f);

        [MenuItem("Tools/Project Spark/Level 37/Create Teaching Drawing")]
        public static void CreateAsset()
        {
            SparkLevel37Builder.EnsureFolder();
            string path = SparkLevel37Builder.Folder + "/Level_37_Drawing.asset";
            if (AssetDatabase.LoadMainAssetAtPath(path) != null) return;
            var drawing = ScriptableObject.CreateInstance<SparkLevelDrawingAsset>();
            drawing.levelId = "level_37";
            drawing.levelTitle = "Kelvin Test";
            drawing.pages = new[]
            {
                Page("01 / Measure the FORCE loop", "TWO-WIRE = 10 Ω + 5 Ω + 10 Ω = 25 Ω",
                    "W01 Battery_positive → LIMIT_IN. W02 LIMIT_OUT → LEAD_PLUS_IN. W03 LEAD_PLUS_OUT → CONNECTOR_PLUS. W04 CONNECTOR_MINUS → LEAD_RETURN_IN. W05 LEAD_RETURN_OUT → Battery_negative. Press READ LOOP. The 100 Ω limiter sets the test current and sits outside the two-wire voltage measurement.",
                    10, 5, false),
                Page("02 / Connect the Kelvin meter", "KELVIN = CONNECTOR VOLTAGE ÷ TEST CURRENT",
                    "W06 SENSE_PLUS → CONNECTOR_PLUS. W07 SENSE_MINUS → CONNECTOR_MINUS. Leave the FORCE wires connected to the same connector terminals. At 40.0 mA, 0.20 V across the connector gives 5 Ω. Press READ KELVIN.",
                    10, 5, true),
                Page("03 / Diagnose both faults", "COMPARE THE TWO-WIRE AND KELVIN READINGS",
                    "Load the first case. Two-wire rises to 35 Ω and Kelvin stays at 5 Ω. Choose LEAD FAULT. Load the second case. Both readings rise to 42 Ω and 12 Ω. Choose CONNECTOR FAULT. The drawing shows the second case.",
                    20, 12, true),
                Page("04 / Verify the repair", "VERIFY THE CONNECTOR, THEN THE FIXTURE",
                    "Press FIT CONNECTOR. Kelvin returns to 5 Ω while two-wire still reads 35 Ω. Press REPLACE LEAD. The readings return to 25 Ω and 5 Ω. The drawing shows the intermediate state after the connector repair.",
                    20, 5, true)
            };
            AssetDatabase.CreateAsset(drawing,path);
            AssetDatabase.SaveAssets();
        }

        private static SparkLevelDrawingPage Page(string title, string caption, string explanation,
            int lead, int connector, bool showSense)
        {
            var parts = new List<SparkSchematicPart>
            {
                Part("positive", SparkSchematicKind.Generic, .075f,.66f, "BATTERY +", Positive),
                Part("limiter", SparkSchematicKind.Resistor, .24f,.66f, "LIMITER 100 Ω", Force),
                Part("lead", SparkSchematicKind.Resistor, .405f,.66f, $"LEAD {lead} Ω", Force),
                Part("connector", SparkSchematicKind.Resistor, .57f,.66f, $"CONNECTOR {connector} Ω", Force),
                Part("return-lead", SparkSchematicKind.Resistor, .735f,.66f, "RETURN 10 Ω", Force),
                Part("negative", SparkSchematicKind.Generic, .915f,.66f, "BATTERY −", Return)
            };
            var wires = new List<SparkSchematicWire>
            {
                Wire(Positive,P(.075f,.66f),P(.195f,.66f)),
                Wire(Force,P(.285f,.66f),P(.360f,.66f)),
                Wire(Force,P(.450f,.66f),P(.525f,.66f)),
                Wire(Force,P(.615f,.66f),P(.690f,.66f)),
                Wire(Return,P(.780f,.66f),P(.915f,.66f))
            };
            if (showSense)
            {
                parts.Add(Part("sense", SparkSchematicKind.Generic, .57f,.34f, "KELVIN SENSE", Sense));
                wires.Add(Wire(Sense,P(.525f,.66f),P(.505f,.66f),P(.505f,.34f),P(.525f,.34f)));
                wires.Add(Wire(Sense,P(.615f,.66f),P(.635f,.66f),P(.635f,.34f),P(.615f,.34f)));
            }
            return new SparkLevelDrawingPage
            {
                title=title, caption=caption, explanation=explanation, parts=parts.ToArray(), wires=wires.ToArray()
            };
        }

        private static SparkSchematicPart Part(string id,SparkSchematicKind kind,float x,float y,string label,Color color) =>
            new SparkSchematicPart { id=id,kind=kind,position=P(x,y),label=label,color=color };

        private static SparkSchematicWire Wire(Color color,params Vector2[] points) =>
            new SparkSchematicWire { color=color,points=points };

        private static Vector2 P(float x,float y) => new Vector2(x,y);
    }
}
#endif
