#if UNITY_EDITOR
using ProjectSpark.UI.VisualTeaching;
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.EditorTools
{
    public static class SparkLevel83DrawingAssetCreator
    {
        [MenuItem("Tools/Project Spark/Level 83/Create Teaching Drawing")]
        public static void CreateAsset()
        {
            SparkLevel83Builder.EnsureFolder();
            string path = SparkLevel83Builder.Folder + "/Level_83_Drawing.asset";
            if (AssetDatabase.LoadMainAssetAtPath(path) != null) return;
            var drawing = ScriptableObject.CreateInstance<SparkLevelDrawingAsset>();
            drawing.levelId = "level_83";
            drawing.levelTitle = "Beyond the Shadow";
            drawing.pages = new[] {
                Page("01 / Wire the orbital workstation","138 WIRES  â€¢  269 FUNCTIONAL SOCKETS","Connect the numbered pairs and the ten 5 V feeds. Connect W104 last. Test emergency isolation and the independent undervoltage channel.","WIRE","POWER","TEST"),
                Page("02 / Capture sunlight","DEPLOY  â€¢  HOME  â€¢  TRACK","Deploy and home both wings. Measure both PV curves, enable MPPT, precharge, then close the main contactor. Calibrate the main bus transducer.","DEPLOY","SWEEP","REGULATE"),
                Page("03 / Survive eclipse","CHARGE  â€¢  PLAN  â€¢  SURVIVE","Commission both battery packs, scan cells and balance them. Plan mission loads and finish a complete orbit. Diagnose and repair each of the three disturbances.","STORE","SURVIVE","RECOVER"),
                Page("04 / Restore the trusted bus measurement","W104  â€¢  CALIBRATE  â€¢  RESET","Arm the test under load and use Undo Last to remove W104. Reconnect the pair. Calibrate, verify independent safety and reset. Run a new complete orbit.","LATCH","RESET","QUALIFY"),
            };
            AssetDatabase.CreateAsset(drawing, path);
            AssetDatabase.SaveAssets();
        }

        private static SparkLevelDrawingPage Page(string title,string caption,string explanation,string first,string middle,string last)
        {
            Color cyan = new(.18f,.86f,.94f);
            Color amber = new(1f,.71f,.31f);
            SparkSchematicPart Part(string id,float x,string label,Color color) =>
                new() {id=id, kind=SparkSchematicKind.Generic, position=new Vector2(x,.71f), label=label, color=color};
            return new SparkLevelDrawingPage {
                title=title, caption=caption, explanation=explanation,
                parts=new[] {Part("input",.18f,first,cyan),Part("tracking",.71f,middle,amber),Part("output",.82f,last,cyan)},
                wires=new[] {
                    new SparkSchematicWire {color=cyan,points=new[] {new Vector2(.25f,.71f),new Vector2(.43f,.71f)}},
                    new SparkSchematicWire {color=amber,points=new[] {new Vector2(.71f,.71f),new Vector2(.75f,.71f)}}
                }
            };
        }
    }
}
#endif
