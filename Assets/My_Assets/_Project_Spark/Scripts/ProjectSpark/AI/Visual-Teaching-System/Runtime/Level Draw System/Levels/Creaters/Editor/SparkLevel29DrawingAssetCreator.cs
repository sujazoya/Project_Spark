#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using ProjectSpark.UI.VisualTeaching;

namespace ProjectSpark.EditorTools
{
    /// <summary>Visual teaching only. Does not change the circuit or award completion.</summary>
    public static class SparkLevel29DrawingAssetCreator
    {
        private static readonly Color Red = new Color(1f, .3f, .36f);
        private static readonly Color Blue = new Color(.2f, .68f, 1f);
        private static readonly Color Gold = new Color(1f, .72f, .25f);
        private static readonly Color Green = new Color(.3f, .95f, .58f);
        private static readonly Color Cyan = new Color(.2f, .9f, .95f);

        [MenuItem("Tools/Project Spark/Visual Teaching/Create Level 29 Drawing Asset")]
        public static void CreateAsset()
        {
            string path = EditorUtility.SaveFilePanelInProject("Create Level 29 Drawing",
                "Level_29_Drawing", "asset", "Save four Level 29 teaching pages.");
            if (string.IsNullOrEmpty(path)) return;
            if (AssetDatabase.LoadAssetAtPath<Object>(path) != null)
            {
                EditorUtility.DisplayDialog("Level 29", "Choose an unused filename to preserve the existing drawing.", "OK");
                return;
            }
            var asset = ScriptableObject.CreateInstance<SparkLevelDrawingAsset>();
            asset.levelId = "level_29";
            asset.levelTitle = "Power Budget - Dual-Brightness LED";
            asset.initialPage = 0;
            asset.pages = new SparkLevelDrawingPage[4];
            for (int i = 0; i < asset.pages.Length; i++) asset.pages[i] = BuildPage(i);
            AssetDatabase.CreateAsset(asset, path);
            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();
            Selection.activeObject = asset;
            EditorGUIUtility.PingObject(asset);
            Debug.Log("[Level 29] Created four teaching pages. Add this asset to SparkDrawCurrentLevel.Drawings. " + path);
        }

        private static Vector2 P(float x, float y) => new Vector2(x, y);
        private static SparkSchematicPart Node(string id, float x, float y, string label, Color c) =>
            new SparkSchematicPart { id = id, kind = SparkSchematicKind.Generic,
                position = P(x, y), label = label, color = c };
        private static SparkSchematicWire W(Color c, params Vector2[] points) =>
            new SparkSchematicWire { color = c, energized = false, points = points };

        private static SparkLevelDrawingPage BuildPage(int index)
        {
            // All symbols use normalized vector strokes. This avoids the existing renderer's
            // fixed-pixel symbol terminals becoming detached from normalized wires on resize.
            bool bright = index == 1;
            var parts = new[] {
                Node("positive", .08f,.73f,"BATTERY +5V",Red),
                Node("negative", .08f,.18f,"BATTERY -",Blue),
                Node("main_in", .22f,.73f,"MAIN IN",Cyan),
                Node("main_out", .33f,.73f,"MAIN OUT",Cyan),
                Node("a", .51f,.67f,"A / BRIGHT",Gold),
                Node("b", .65f,.67f,"B / ECO",Green),
                Node("com", .58f,.48f,"S06 COM",Red),
                Node("bright_in", .76f,.57f,"S04 330 OHM",Gold),
                Node("bright_out", .86f,.57f,"OUT",Gold),
                Node("eco_in", .74f,.26f,"S05 1k OHM",Green),
                Node("eco_out", .84f,.26f,"OUT",Green),
                Node("led_a", .94f,.44f,"LED ANODE",Green),
                Node("led_k", .94f,.30f,"LED CATHODE",Blue)
            };
            var wires = new List<SparkSchematicWire> {
                W(Red,P(.08f,.73f),P(.22f,.73f)), // fixed battery positive -> main
                W(Cyan,P(.22f,.73f),P(.33f,.73f)), // main ON illustration
                W(Red,P(.33f,.73f),P(.38f,.73f),P(.38f,.48f),P(.58f,.48f)), // W1
                W(Gold,P(.51f,.67f),P(.51f,.86f),P(.71f,.86f),P(.71f,.57f),P(.76f,.57f)), // W2
                W(Gold,P(.86f,.57f),P(.94f,.57f),P(.94f,.44f)), // W3
                W(Green,P(.65f,.67f),P(.69f,.67f),P(.69f,.26f),P(.74f,.26f)), // W4
                W(Green,P(.84f,.26f),P(.89f,.26f),P(.89f,.44f),P(.94f,.44f)), // W5
                W(Blue,P(.94f,.30f),P(.98f,.30f),P(.98f,.10f),P(.08f,.10f),P(.08f,.18f)), // W6
                W(bright ? Gold : Green,P(.58f,.48f),bright ? P(.51f,.67f) : P(.65f,.67f)),
                W(Green,P(.94f,.44f),P(.94f,.41f)),
                W(Green,P(.925f,.41f),P(.955f,.41f),P(.94f,.35f),P(.925f,.41f)),
                W(Green,P(.92f,.35f),P(.96f,.35f)),
                W(Green,P(.94f,.35f),P(.94f,.30f))
            };
            Resistor(wires,.76f,.86f,.57f,Gold);
            Resistor(wires,.74f,.84f,.26f,Green);
            string[] titles = { "Workbench Wiring / ECO", "Test BRIGHT", "Select ECO", "Six Completion Targets" };
            string[] captions = {
                "S04: 330 OHM | S05: 1k OHM | S06: SPDT | MAIN ON",
                "COM-A CLOSED | COM-B OPEN | EXPECT ABOUT 9 mA",
                "COM-B CLOSED | COM-A OPEN | EXPECT ABOUT 3 mA",
                "FINAL ECO STATE | HEALTHY PARTS | CLOSED RETURN | NO SHORTS"
            };
            string[] explanations = {
                "Keep the fixed Battery_positive -> SwitchInput wire. Add six wires: SwitchOutput -> SPDT_COM; " +
                "SPDT_A -> R_BRIGHT_Input; R_BRIGHT_Output -> LED_Anode; SPDT_B -> R_ECO_Input; " +
                "R_ECO_Output -> LED_Anode; LED_Cathode -> Battery_negative. Remove old capacitor/diode wiring. " +
                "Two resistor outputs join at LED anode. Never join SPDT A and B. Colours identify branches, not live voltage.",
                "MAIN ON, select A. Current flows through the 330 Ohm resistor in Slot_04 and LED. " +
                "For a 2 V LED: (5-2)/330 = 9.09 mA; source power approximately 45.5 mW. " +
                "Slot_05 carries approximately zero current. BRIGHT is a learning step, not the completion state.",
                "Keep MAIN ON and select B. Current flows through the 1k Ohm resistor in Slot_05 and LED. " +
                "For a 2 V LED: (5-2)/1000 = 3 mA; source power approximately 15 mW, about 67 percent less. " +
                "Slot_04 carries approximately zero current. The LED remains on with lower brightness. No capacitor is used.",
                "All six targets must pass: source 4.8-5.2 V; LED ON; ECO resistor current 2.5-3.5 mA; " +
                "BRIGHT resistor current at most 0.05 mA; ECO resistor voltage 2.7-3.3 V; source current 2.5-3.5 mA. " +
                "A valid source return and healthy installed resistors are required. Static targets validate the final state, " +
                "not a BRIGHT-then-ECO sequence or exact wiring topology. Drawings do not award completion."
            };
            return new SparkLevelDrawingPage { title = "LEVEL 29 | " + titles[index],
                caption = "PAGE " + (index + 1) + "/4 | " + captions[index],
                explanation = explanations[index], parts = parts, wires = wires.ToArray() };
        }

        private static void Resistor(List<SparkSchematicWire> wires, float x0, float x1, float y, Color c)
        {
            float d = (x1-x0)/8f;
            wires.Add(W(c,P(x0,y),P(x0+d,y),P(x0+2*d,y+.012f),P(x0+3*d,y-.012f),
                P(x0+4*d,y+.012f),P(x0+5*d,y-.012f),P(x0+6*d,y+.012f),P(x0+7*d,y),P(x1,y)));
        }
    }
}
#endif
