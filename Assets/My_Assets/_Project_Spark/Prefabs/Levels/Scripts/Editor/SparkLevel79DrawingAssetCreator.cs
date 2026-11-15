#if UNITY_EDITOR
using ProjectSpark.UI.VisualTeaching;
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.EditorTools
{
    public static class SparkLevel79DrawingAssetCreator
    {
        [MenuItem("Tools/Project Spark/Level 79/Create Teaching Drawing")]
        public static void CreateAsset()
        {
            SparkLevel79Builder.EnsureFolder();
            string path = SparkLevel79Builder.Folder + "/Level_79_Drawing.asset";
            if (AssetDatabase.LoadMainAssetAtPath(path) != null) return;
            var drawing = ScriptableObject.CreateInstance<SparkLevelDrawingAsset>();
            drawing.levelId = "level_79";
            drawing.levelTitle = "Bond the Worlds";
            drawing.pages = new[] {
                Page("01 / Wire the bonding cell","122 WIRES  •  188 SOCKETS","Wire the motion, laser, vision, force, metrology and safety systems. Connect W037 last.","MOTION","LASER","SAFETY"),
                Page("02 / Align the first pair","LOGIC  •  MEMORY","Home the axes, calibrate both sensors, prepare the surfaces and bond the first two chiplets.","PREPARE","ALIGN","BOND"),
                Page("03 / Finish and test","I/O  •  SENSOR  •  LINK","After scan acceptance of all four placements, consolidate and test the vertical contacts and chiplet links.","SCAN","CONSOLIDATE","TEST"),
                Page("04 / Recover precision motion","W037  •  MOTION HOLD","Disconnect W037 mid-approach, reconnect, home, calibrate, verify and reset, then build a fresh assembly.","HOLD","RECALIBRATE","CERTIFY"),
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
