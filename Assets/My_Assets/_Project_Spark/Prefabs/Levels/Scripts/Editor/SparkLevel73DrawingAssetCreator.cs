#if UNITY_EDITOR
using ProjectSpark.UI.VisualTeaching;
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.EditorTools
{
    public static class SparkLevel73DrawingAssetCreator
    {
        [MenuItem("Tools/Project Spark/Level 73/Create Teaching Drawing")]
        public static void CreateAsset()
        {
            SparkLevel73Builder.EnsureFolder();
            string path = SparkLevel73Builder.Folder + "/Level_73_Drawing.asset";
            if (AssetDatabase.LoadMainAssetAtPath(path) != null) return;
            var drawing = ScriptableObject.CreateInstance<SparkLevelDrawingAsset>();
            drawing.levelId = "level_73";
            drawing.levelTitle = "Polish to Perfection";
            drawing.pages = new[] {
                Page("01 / Wire the CMP workbench","98 WIRES  •  152 SOCKETS","Connect drives, force, slurry, cleaning and measurement. Install downforce feedback W98 last.","POWER","PROCESS","SAFETY"),
                Page("02 / Establish controlled contact","SPEED  •  DOWNFORCE  •  SLURRY","Calibrate, condition the pad, spin both drives, clamp a wafer and enable the zone controller.","COMMISSION","CLAMP","ENGAGE"),
                Page("03 / Planarize and verify","ENDPOINT  •  RINSE  •  METROLOGY","Verify endpoint, rinse, brush and dry before checking wafer flatness, removal uniformity and defects.","POLISH","CLEAN","SCAN"),
                Page("04 / Restore downforce","W98  •  CARRIER RETRACTION","Disconnect W98 during contact. Reconnect, calibrate and verify the unloaded sensor. Then certify a fresh wafer.","RETRACT","RECALIBRATE","CERTIFY"),
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
