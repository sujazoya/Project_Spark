#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;
using ProjectSpark.UI.VisualTeaching;

namespace ProjectSpark.EditorTools
{
    /// <summary>
    /// Generates four Level 9 authored teaching pages into a SparkLevelDrawingAsset.
    /// No scene component or additional teaching MonoBehaviour is required.
    /// </summary>
    public static class SparkLevel09DrawingAssetCreator
    {
        [MenuItem("Tools/Project Spark/Visual Teaching/Create Level 09 Drawing Asset")]
        public static void CreateAsset()
        {
            string path = EditorUtility.SaveFilePanelInProject(
                "Create Level 09 Drawing",
                "Level_09_Drawing",
                "asset",
                "Choose where to save the Level 9 drawing asset.");
            if (string.IsNullOrEmpty(path)) return;

            if (AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path) != null)
            {
                EditorUtility.DisplayDialog("Level 9 Drawing",
                    "An asset already exists at this path. Choose a new path to avoid overwriting authored pages.",
                    "OK");
                return;
            }

            var drawing = ScriptableObject.CreateInstance<SparkLevelDrawingAsset>();
            drawing.levelId = "level_09";
            drawing.levelTitle = "Series-Parallel Circuit Master";
            drawing.initialPage = 0;
            drawing.pages = new SparkLevelDrawingPage[4];
            for (int i = 0; i < 4; i++)
                drawing.pages[i] = BuildPage(i);

            AssetDatabase.CreateAsset(drawing, path);
            EditorUtility.SetDirty(drawing);
            AssetDatabase.SaveAssets();
            Selection.activeObject = drawing;
            EditorGUIUtility.PingObject(drawing);
            Debug.Log("[Level 9 Drawing] Created four-page asset at " + path);
        }

        private static readonly Color Supply = new Color(.99f, .30f, .40f, 1f);
        private static readonly Color Ground = new Color(.21f, .70f, 1f, 1f);
        private static readonly Color Gold = new Color(1f, .79f, .32f, 1f);
        private static readonly Color Junction = new Color(.25f, .99f, .77f, 1f);
        private static readonly Color Cyan = new Color(.28f, .95f, .98f, 1f);

        private static Vector2 P(float x, float y) => new Vector2(x, y);
        private static SparkSchematicPart Part(string id, SparkSchematicKind kind,
            float x, float y, string label, Color color) =>
            new SparkSchematicPart { id = id, kind = kind, position = P(x, y), label = label, color = color };
        private static SparkSchematicWire Wire(Color color, params Vector2[] points) =>
            new SparkSchematicWire { color = color, energized = false, points = points };

        private static SparkLevelDrawingPage BuildPage(int page)
        {
            // Circuit: Battery+ -> switch -> R1 220 -> VOUT -> { R2 330 || R3 470 } -> Battery-.
            // LCD is a separate 10 MOhm (voltage/auto mode) sensing branch across VOUT and ground.
            // Positions are normalized and use the renderer's horizontal symbol tips (~0.06).
            var parts = new[]
            {
                Part("battery", SparkSchematicKind.Battery, .11f, .53f, "5 V", Supply),
                Part("switch", SparkSchematicKind.Switch, .27f, .83f, "SWITCH", Gold),
                Part("r1", SparkSchematicKind.Resistor, .46f, .83f, "R1 220 Ohm", Gold),
                Part("vout", SparkSchematicKind.Generic, .61f, .83f, "", Junction),
                Part("r2", SparkSchematicKind.Resistor, .70f, .64f, "R2 330 Ohm", Gold),
                Part("r3", SparkSchematicKind.Resistor, .70f, .38f, "R3 470 Ohm", Gold),
                Part("return", SparkSchematicKind.Generic, .84f, .38f, "", Ground),
                Part("lcd", SparkSchematicKind.Meter, .42f, .39f, "LCD A+ B-", Cyan),
                Part("gnd", SparkSchematicKind.Ground, .60f, .14f, "GND", Ground)
            };

            var wires = new[]
            {
                // Main supply and fixed series resistor.
                Wire(Supply, P(.11f,.67f), P(.11f,.83f), P(.21f,.83f)),
                Wire(Supply, P(.33f,.83f), P(.40f,.83f)),
                Wire(Supply, P(.52f,.83f), P(.61f,.83f)),
                // Split into two independent parallel paths at VOUT.
                Wire(Junction, P(.61f,.83f), P(.61f,.64f), P(.64f,.64f)),
                Wire(Junction, P(.61f,.64f), P(.61f,.38f), P(.64f,.38f)),
                // Rejoin resistor outputs at the actual return bus.
                Wire(Ground, P(.76f,.64f), P(.84f,.64f)),
                Wire(Ground, P(.76f,.38f), P(.84f,.38f)),
                Wire(Ground, P(.84f,.64f), P(.84f,.14f), P(.60f,.14f)),
                Wire(Ground, P(.60f,.14f), P(.11f,.14f), P(.11f,.39f)),
                // LCD high-impedance measurement branch, NOT a substitute for ground return.
                Wire(Junction, P(.61f,.83f), P(.57f,.83f), P(.57f,.50f), P(.42f,.50f), P(.42f,.45f)),
                Wire(Ground, P(.42f,.33f), P(.42f,.14f))
            };

            string title, caption, explanation;
            switch (page)
            {
                case 0:
                    title = "LEVEL 9 | Series-Parallel Circuit | Overview";
                    caption = "PAGE 1/4 | R1 SERIES WITH (R2 PARALLEL R3) | 5 V";
                    explanation = "ILLUSTRATIVE / NOT LIVE VERIFIED. Fixed R1 = 220 Ohm is in series with two parallel return branches: fixed R2 = 330 Ohm and replaceable R3. Start R3 at unsuitable 220 Ohm; correct R3 is 470 Ohm. LCD is high-impedance across VOUT and Battery_negative, not a current return.";
                    break;
                case 1:
                    title = "LEVEL 9 | Exact Seven Wiring Connections";
                    caption = "PAGE 2/4 | SEVEN WIRES AFTER BATTERY+ TO SWITCH INPUT";
                    explanation = "ILLUSTRATIVE / NOT LIVE VERIFIED. 1 SwitchOutput -> R1 Register_Input. 2 R1 Register_Output -> R2 Register_Input. 3 R1 Register_Output -> R3 Register_Input. 4 R2 Register_Output -> Battery_negative. 5 R3 Register_Output -> Battery_negative. 6 LCD_Terminal_A -> R1 Register_Output (VOUT). 7 LCD_Terminal_B -> Battery_negative. Battery_positive -> switch input is assumed prewired. VOUT is a shared node; ensure connection capacity and actual SparkCircuitSystem wires.";
                    break;
                case 2:
                    title = "LEVEL 9 | Voltage & Current Measurements";
                    caption = "PAGE 3/4 | VOUT 2.342 V | TOTAL 12.081 mA";
                    explanation = "NOMINAL DESIGN VALUES / NOT LIVE READINGS. At 5 V, R1 220 Ohm and (R2 330 Ohm || R3 470 Ohm): parallel equivalent = 193.875 Ohm, total resistance = 413.875 Ohm; total / R1 current = 12.081 mA. VOUT / R2 / R3 voltage = 2.342 V; R1 drop = 2.658 V. R2 current = 7.098 mA, R3 current = 4.983 mA. LCD AutoVoltageAndComponentCurrent displays A-B voltage and the current of whichever fixed component is assigned in SparkLCDMeterDisplay; it does not automatically report total current.";
                    break;
                default:
                    title = "LEVEL 9 | Correct Circuit & Completion";
                    caption = "PAGE 4/4 | R3 470 Ohm | 2.342 V | 12.081 mA | 4 TARGETS";
                    explanation = "ILLUSTRATIVE / NOT LIVE VERIFIED. Replace shelf R3 220 Ohm with R3 470 Ohm. R1 220 and R2 330 stay fixed. Targets: R1 conducting; fixed R2 voltage between 2.25 and 2.45 V; fixed R2 current between 6.7 and 7.5 mA; supply current between 11.8 and 12.4 mA. Confirm both R2 and R3 have their own direct electrical return to Battery_negative, valid source-current polarity, no shorts, and healthy installed resistors. Only the existing SparkLevelEvaluator awards 4/4 and unlocks Level 10.";
                    break;
            }

            return new SparkLevelDrawingPage
            {
                title = title,
                explanation = explanation,
                caption = caption,
                parts = parts,
                wires = wires
            };
        }
    }
}
#endif
