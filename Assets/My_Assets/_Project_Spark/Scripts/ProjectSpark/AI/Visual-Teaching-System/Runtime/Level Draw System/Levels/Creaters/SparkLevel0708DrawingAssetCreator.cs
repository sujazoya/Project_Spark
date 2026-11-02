#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using ProjectSpark.UI.VisualTeaching;

namespace ProjectSpark.EditorTools
{
    /// <summary>Author Level 7 and 8 ScriptableObject teaching assets, no scene components.</summary>
    public static class SparkLevel0708DrawingAssetCreator
    {
        [MenuItem("Tools/Project Spark/Visual Teaching/Create Level 07 Drawing Asset")]
        public static void CreateLevel7() => Create("level_07", "Voltage Divider Challenge", Build7);

        [MenuItem("Tools/Project Spark/Visual Teaching/Create Level 08 Drawing Asset")]
        public static void CreateLevel8() => Create("level_08", "Parallel Current Divider", Build8);

        private static void Create(string id, string title, System.Func<int, SparkLevelDrawingPage> build)
        {
            string path = EditorUtility.SaveFilePanelInProject("Create " + title,
                id == "level_07" ? "Level_07_Drawing" : "Level_08_Drawing", "asset",
                "Choose a destination for the level drawing asset.");
            if (string.IsNullOrEmpty(path)) return;
            if (AssetDatabase.LoadAssetAtPath<Object>(path) != null)
            {
                EditorUtility.DisplayDialog("Asset already exists", "Choose a new filename; existing drawing assets are never overwritten.", "OK");
                return;
            }
            var asset = ScriptableObject.CreateInstance<SparkLevelDrawingAsset>();
            asset.levelId = id;
            asset.levelTitle = title;
            asset.initialPage = 0;
            asset.pages = new SparkLevelDrawingPage[4];
            for (int i = 0; i < 4; i++) asset.pages[i] = build(i);
            AssetDatabase.CreateAsset(asset, path);
            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();
            Selection.activeObject = asset;
            EditorGUIUtility.PingObject(asset);
            Debug.Log("[Spark Level Drawing] Created " + id + " at " + path);
        }

        private static readonly Color Supply = new Color(.99f, .30f, .40f, 1f);
        private static readonly Color Ground = new Color(.21f, .70f, 1f, 1f);
        private static readonly Color Gold = new Color(1f, .79f, .32f, 1f);
        private static readonly Color Junction = new Color(.25f, .99f, .77f, 1f);
        private static readonly Color Cyan = new Color(.28f, .95f, .98f, 1f);
        private static Vector2 P(float x, float y) => new Vector2(x, y);
        private static SparkSchematicPart Part(string id, SparkSchematicKind kind, float x, float y, string label, Color color)
            => new SparkSchematicPart { id=id, kind=kind, position=P(x,y), label=label, color=color };
        private static SparkSchematicWire Wire(Color color, params Vector2[] points)
            => new SparkSchematicWire { color=color, energized=false, points=points };

        private static SparkLevelDrawingPage Build7(int page)
        {
            var parts = new[]
            {
                Part("battery", SparkSchematicKind.Battery, .11f, .51f, "5 V SUPPLY", Supply),
                Part("switch", SparkSchematicKind.Switch, .29f, .79f, "SWITCH", Gold),
                Part("r1", SparkSchematicKind.Resistor, .46f, .79f, "R1 220 Ohm", Gold),
                Part("vout", SparkSchematicKind.Generic, .585f, .79f, "", Junction),
                Part("r2", SparkSchematicKind.Resistor, .73f, .79f, "R2 330 Ohm", Gold),
                Part("lcd", SparkSchematicKind.Meter, .65f, .36f, "LCD 3.00 V", Cyan),
                Part("ground", SparkSchematicKind.Ground, .44f, .18f, "GND", Ground)
            };
            var wires = new[]
            {
                // Main current path, independent of meter.
                Wire(Supply, P(.11f,.65f), P(.11f,.79f), P(.23f,.79f)),
                Wire(Supply, P(.35f,.79f), P(.40f,.79f)),
                Wire(Supply, P(.52f,.79f), P(.585f,.79f)),
                Wire(Junction, P(.585f,.79f), P(.67f,.79f)),
                Wire(Ground, P(.79f,.79f), P(.89f,.79f), P(.89f,.18f), P(.44f,.18f)),
                Wire(Ground, P(.44f,.18f), P(.11f,.18f), P(.11f,.37f)),
                // High impedance LCD sense connections A=midpoint and B=GND.
                // Route A on left side of LCD to avoid crossing the body of R2.
                Wire(Junction, P(.585f,.79f), P(.585f,.54f), P(.59f,.54f), P(.59f,.36f)),
                Wire(Ground, P(.71f,.36f), P(.83f,.36f), P(.83f,.18f))
            };
            string title, caption, explanation;
            switch (page)
            {
                case 0:
                    title = "LEVEL 7 | Voltage Divider | Overview";
                    caption = "PAGE 1/4 | SERIES 220 + 330 Ohm | 5 V SUPPLY";
                    explanation = "Correct completed circuit: Battery+ -> Switch -> replaceable upper R1 220 Ohm -> VOUT -> fixed lower R2 330 Ohm -> Battery_negative. Initial R1 is 330 Ohm; replace with 220 Ohm to target VOUT=3.00 V. LCD measures across R2 and never substitutes for the real ground return.";
                    break;
                case 1:
                    title = "LEVEL 7 | Exact Five Wiring Connections";
                    caption = "PAGE 2/4 | FIVE WIRES AFTER BATTERY+ TO SWITCH INPUT";
                    explanation = "1 SwitchOutput -> R1 Register_Input. 2 R1 Register_Output -> R2 Register_Input (VOUT). 3 R2 Register_Output -> Battery_negative. 4 LCD_Terminal_A -> shared VOUT node. 5 LCD_Terminal_B -> Battery_negative. Battery positive to switch input is assumed prewired. R1 and R2 need an actual conducting series return independent of the LCD.";
                    break;
                case 2:
                    title = "LEVEL 7 | Expected Electrical Measurements";
                    caption = "PAGE 3/4 | VOUT 3.00 V | I 9.09 mA | 5 V";
                    explanation = "NOMINAL ESTIMATES, NOT LIVE MEASUREMENTS. At 5 V, upper R1 220 Ohm and lower R2 330 Ohm: total I=5/(220+330)=9.09 mA; drop across R1=2.00 V; across R2 and VOUT to ground=3.00 V. AutoVoltageAndComponentCurrent: LCD A=VOUT, B=GND, component current reference=fixed R2.";
                    break;
                default:
                    title = "LEVEL 7 | Solution & Four Objectives";
                    caption = "PAGE 4/4 | 220 + 330 Ohm | VOUT 2.90-3.10 V";
                    explanation = "Replace upper R1 330 Ohm with 220 Ohm. Check 1 fixed R2 conducting; 2 R2 voltage between 2.90 and 3.10 V; 3 R2 current between 8 and 10 mA; 4 verified source current between 8 and 10 mA. Use maximum-value toggles. Source current orientation must match evaluator rules. Existing level evaluator remains sole victory authority.";
                    break;
            }
            return new SparkLevelDrawingPage
            {
                title=title, caption=caption, explanation=explanation, parts=parts, wires=wires
            };
        }

        private static SparkLevelDrawingPage Build8(int page)
        {
            var parts = new[]
            {
                Part("battery", SparkSchematicKind.Battery, .11f,.50f, "5 V", Supply),
                Part("switch", SparkSchematicKind.Switch,.27f,.82f, "SWITCH", Gold),
                Part("positive", SparkSchematicKind.Generic,.41f,.82f,"",Supply),
                Part("r1", SparkSchematicKind.Resistor,.55f,.66f,"R1 330 Ohm",Gold),
                Part("r2", SparkSchematicKind.Resistor,.55f,.39f,"R2 470 Ohm",Gold),
                Part("return1", SparkSchematicKind.Generic,.72f,.66f,"",Ground),
                Part("return2", SparkSchematicKind.Generic,.72f,.39f,"",Ground),
                Part("lcd", SparkSchematicKind.Meter,.87f,.52f,"LCD A+ B-",Cyan),
                Part("gnd", SparkSchematicKind.Ground,.66f,.16f,"GND",Ground)
            };

            // Each stroke has endpoints at the actual resistor/meter symbol tips.
            var wires = new[]
            {
                Wire(Supply,P(.11f,.64f),P(.11f,.82f),P(.21f,.82f)),
                Wire(Supply,P(.33f,.82f),P(.41f,.82f),P(.41f,.39f)),
                Wire(Supply,P(.41f,.66f),P(.49f,.66f)),
                Wire(Supply,P(.41f,.39f),P(.49f,.39f)),
                Wire(Ground,P(.61f,.66f),P(.72f,.66f)),
                Wire(Ground,P(.61f,.39f),P(.72f,.39f)),
                Wire(Ground,P(.72f,.66f),P(.72f,.16f),P(.11f,.16f),P(.11f,.36f)),
                Wire(Supply,P(.41f,.82f),P(.87f,.82f),P(.87f,.64f)),
                Wire(Ground,P(.87f,.40f),P(.87f,.16f),P(.72f,.16f))
            };

            string title, caption, explanation;
            switch (page)
            {
                case 0:
                    title="LEVEL 8 | Parallel Current Divider | Overview";
                    caption="PAGE 1/4 | TWO PARALLEL BRANCHES | 5 V EACH";
                    explanation="R1 fixed 330 Ohm and R2 replaceable, starting 220 Ohm. Correct R2=470 Ohm. Each resistor independently spans SwitchOutput to Battery_negative. LCD is a high-impedance sense branch, not a return wire.";
                    break;
                case 1:
                    title="LEVEL 8 | Exact Six Wiring Connections";
                    caption="PAGE 2/4 | SIX WIRES AFTER BATTERY+ TO SWITCH INPUT";
                    explanation="1 SwitchOutput -> R1 Register_Input. 2 SwitchOutput -> R2 Register_Input. 3 R1 Register_Output -> Battery_negative. 4 R2 Register_Output -> Battery_negative. 5 LCD_Terminal_A -> SwitchOutput. 6 LCD_Terminal_B -> Battery_negative. Battery+ to switch input is assumed already wired. Both resistors require their own true return path.";
                    break;
                case 2:
                    title="LEVEL 8 | Expected Measurements";
                    caption="PAGE 3/4 | 5.00 V EACH | 15.15 + 10.64 = 25.79 mA";
                    explanation="NOMINAL ESTIMATES, NOT LIVE DATA. 5 V across fixed R1=330 Ohm gives 15.15 mA. 5 V across corrected R2=470 Ohm gives 10.64 mA. Total source current about 25.79 mA. LCD AutoVoltageAndComponentCurrent displays A-to-B voltage and selected component current; SourceCurrent mode requires a configured source reference.";
                    break;
                default:
                    title="LEVEL 8 | Solution & Four Objectives";
                    caption="PAGE 4/4 | REPLACE R2 WITH 470 Ohm | TOTAL 25.79 mA";
                    explanation="Replace incorrect R2=220 Ohm with 470 Ohm. Check fixed R1 conducting, R1 voltage 4.90-5.10 V, R1 current 14-16 mA, and source current 25-27 mA. Enable maximum bounds and require healthy installed resistors. The level evaluator decides completion; this authored drawing is not live validation.";
                    break;
            }
            return new SparkLevelDrawingPage
            {
                title=title, caption=caption, explanation=explanation, parts=parts, wires=wires
            };
        }
    }
}
#endif
