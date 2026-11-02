#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using ProjectSpark.UI.VisualTeaching;

namespace ProjectSpark.EditorTools
{
    public static class SparkLevelDrawingEditorTools
    {
        private static readonly Color Red = new Color(.99f,.30f,.40f,1f);
        private static readonly Color Blue = new Color(.21f,.70f,1f,1f);
        private static readonly Color Gold = new Color(1f,.79f,.32f,1f);
        private static readonly Color Cyan = new Color(.28f,.95f,.98f,1f);
        private static Vector2 P(float x,float y) => new Vector2(x,y);
        private static SparkSchematicPart Part(string id,SparkSchematicKind kind,float x,float y,string label,Color color)
            => new SparkSchematicPart {id=id,kind=kind,position=P(x,y),label=label,color=color};
        private static SparkSchematicWire Wire(Color color,params Vector2[] points)
            => new SparkSchematicWire {color=color,energized=false,points=points};

        [MenuItem("Tools/Project Spark/Visual Teaching/Install Current Level Drawer")]
        public static void Install()
        {
            SparkDrawCurrentLevel drawer = Object.FindFirstObjectByType<SparkDrawCurrentLevel>(FindObjectsInactive.Include);
            if (drawer == null)
            {
                var manager = Object.FindFirstObjectByType<SparkVisualTeachingManager>(FindObjectsInactive.Include);
                if (manager == null) { EditorUtility.DisplayDialog("Level Drawer","Place SparkVisualTeachingManager in your gameplay scene first.","OK"); return; }
                drawer = Undo.AddComponent<SparkDrawCurrentLevel>(manager.gameObject);
            }
            Selection.activeGameObject = drawer.gameObject;
            EditorGUIUtility.PingObject(drawer);
            Debug.Log("[Level Draw] Installed. Assign level drawing assets to the Drawings array in SparkDrawCurrentLevel Inspector.");
        }

        [MenuItem("Assets/Create/Project Spark/Visual Teaching/Level 8 Sample (4 Pages)")]
        public static void CreateLevel8Sample()
        {
            var asset = ScriptableObject.CreateInstance<SparkLevelDrawingAsset>();
            asset.levelId="level_08";
            asset.levelTitle="Level 8 | Parallel Current Divider";
            asset.pages = new SparkLevelDrawingPage[4];
            string[] headings = {"Overview","Exact Wiring","Measurements","Solution"};
            string[] captions = {
                "PAGE 1/4 | 5 V TWO-BRANCH PARALLEL NETWORK",
                "PAGE 2/4 | SIX CONNECTIONS AFTER SWITCH INPUT",
                "PAGE 3/4 | R1 15.15mA | R2 10.64mA | TOTAL 25.79mA",
                "PAGE 4/4 | REPLACE R2 WITH 470 OHM | VERIFY 4/4"
            };
            string[] explanations = {
                "Two branches share 5 V. Fixed R1 = 330 Ohm; replaceable R2 begins at 220 Ohm and must be 470 Ohm.",
                "1 SwitchOutput -> R1 Register_Input; 2 SwitchOutput -> R2 Register_Input; 3 R1 Register_Output -> Battery_negative; 4 R2 Register_Output -> Battery_negative; 5 LCD_Terminal_A -> SwitchOutput; 6 LCD_Terminal_B -> Battery_negative. Battery positive -> switch input is prewired.",
                "Expected at 5 V: R1 330 Ohm = 15.15 mA; R2 470 Ohm = 10.64 mA; source = 25.79 mA. LCD component-current mode displays selected R1 current, not automatic source total.",
                "Replace initial 220 Ohm with 470 Ohm; confirm separate low-resistance returns, healthy installed resistors, ~5 V per branch, ~25.79 mA source, and 4/4 objectives."
            };
            for (int i=0;i<4;i++)
            {
                asset.pages[i]=new SparkLevelDrawingPage {
                    title="LEVEL 8 | " + headings[i],caption=captions[i],explanation=explanations[i],
                    parts=new [] {
                        Part("battery",SparkSchematicKind.Battery,.11f,.50f,"5 V",Red),
                        Part("switch",SparkSchematicKind.Switch,.27f,.82f,"SWITCH",Gold),
                        Part("positive",SparkSchematicKind.Generic,.41f,.82f,"",Red),
                        Part("r1",SparkSchematicKind.Resistor,.55f,.66f,"R1 330 Ohm",Gold),
                        Part("r2",SparkSchematicKind.Resistor,.55f,.39f,"R2 470 Ohm",Gold),
                        Part("return1",SparkSchematicKind.Generic,.72f,.66f,"",Blue),
                        Part("return2",SparkSchematicKind.Generic,.72f,.39f,"",Blue),
                        Part("lcd",SparkSchematicKind.Meter,.87f,.52f,"LCD A+ B-",Cyan),
                        Part("gnd",SparkSchematicKind.Ground,.66f,.16f,"GND",Blue)
                    },
                    wires=new [] {
                        Wire(Red,P(.11f,.64f),P(.11f,.82f),P(.21f,.82f)),
                        Wire(Red,P(.33f,.82f),P(.41f,.82f),P(.41f,.39f)),
                        Wire(Red,P(.41f,.66f),P(.49f,.66f)),Wire(Red,P(.41f,.39f),P(.49f,.39f)),
                        Wire(Blue,P(.61f,.66f),P(.72f,.66f)),Wire(Blue,P(.61f,.39f),P(.72f,.39f)),
                        Wire(Blue,P(.72f,.66f),P(.72f,.16f),P(.11f,.16f),P(.11f,.36f)),
                        Wire(Red,P(.41f,.82f),P(.87f,.82f),P(.87f,.64f)),
                        Wire(Blue,P(.87f,.40f),P(.87f,.16f),P(.72f,.16f))
                    }
                };
            }
            string folder=AssetDatabase.GetAssetPath(Selection.activeObject);
            if (!AssetDatabase.IsValidFolder(folder)) folder="Assets";
            string path=AssetDatabase.GenerateUniqueAssetPath(folder + "/Level_08_Drawing.asset");
            AssetDatabase.CreateAsset(asset,path); AssetDatabase.SaveAssets();
            Selection.activeObject=asset; EditorGUIUtility.PingObject(asset);
        }
    }
}
#endif
