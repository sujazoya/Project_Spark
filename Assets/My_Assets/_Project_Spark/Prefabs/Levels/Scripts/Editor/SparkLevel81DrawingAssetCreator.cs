#if UNITY_EDITOR
using ProjectSpark.UI.VisualTeaching;
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.EditorTools
{
    public static class SparkLevel81DrawingAssetCreator
    {
        [MenuItem("Tools/Project Spark/Level 81/Create Teaching Drawing")]
        public static void CreateAsset()
        {
            SparkLevel81Builder.EnsureFolder();
            string path = SparkLevel81Builder.Folder + "/Level_81_Drawing.asset";
            if (AssetDatabase.LoadMainAssetAtPath(path) != null) return;
            var drawing = ScriptableObject.CreateInstance<SparkLevelDrawingAsset>();
            drawing.levelId = "level_81";
            drawing.levelTitle = "Outlast the Bitstorm";
            drawing.pages = new[] {
                Page("01 / Wire the mission computer","130 WIRES  •  200 SOCKETS","Wire power, both ECC banks, the compute lanes, optical data and safety. Connect W094 last.","POWER","ECC","SAFETY"),
                Page("02 / Prove ECC and voting","72-BIT SECDED  •  THREE LANES","Flip one memory bit, scrub and check the corrected codeword. Prove that one wrong compute lane is masked.","INJECT","CORRECT","VOTE"),
                Page("03 / Fly the simulated mission","NAVIGATION  •  MANEUVER  •  DATA RETURN","Verify packet CRC and freshness and authorize commands only with clean memory, a qualified vote, and a healthy watchdog.","NAVIGATION","MANEUVER","DATA"),
                Page("04 / Restore ECC supervision","W094  •  MISSION SAFE HOLD","Disconnect W094 during a live mission. Reconnect, diagnose, revalidate, explicitly reset and fly a fresh mission.","HOLD","CHECK","CERTIFY"),
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
