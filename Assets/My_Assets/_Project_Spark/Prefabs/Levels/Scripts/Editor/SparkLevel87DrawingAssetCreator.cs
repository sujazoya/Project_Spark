#if UNITY_EDITOR
using ProjectSpark.UI.VisualTeaching;
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.EditorTools
{
    public static class SparkLevel87DrawingAssetCreator
    {
        [MenuItem("Tools/Project Spark/Level 87/Create Teaching Drawing")]
        public static void CreateAsset()
        {
            SparkLevel87Builder.EnsureFolder();
            string path = SparkLevel87Builder.Folder + "/Level_87_Drawing.asset";
            if (AssetDatabase.LoadMainAssetAtPath(path) != null) return;
            var drawing = ScriptableObject.CreateInstance<SparkLevelDrawingAsset>();
            drawing.levelId = "level_87";
            drawing.levelTitle = "The Gravity Engine";
            drawing.pages = new[] {


                Page("01 / Wire the control network","158 DIRECT WIRES","Connect the numbered ports on their equipment. Verify protected 5 V, the independent relay, E-stop and the primary W137 sensor.","WIRE","POWER","CHECK"),
                Page("02 / Levitate and spin","8 BEARING STATIONS","Calibrate the radial and axial gap channels. Lift off the touchdown bearings, precharge the drives, then perform a low-speed trial and resonance scan.","CAL","LIFT","SCAN"),
                Page("03 / Hold artificial gravity","3 RPM  •  0.453 g","The physical habitat scenario uses a 45 m radius. Balance the rotor, reach 3 RPM and maintain habitat power, cooling and optical data under changing load.","BALANCE","SPIN","LOAD"),
                Page("04 / Recover trusted feedback","W137  •  P019 → P135","During the supervised low-speed trial remove W137. Watch the controlled rundown. Reconnect, recalibrate and reset, then qualify a fresh complete run.","STOP","RESET","QUALIFY"),
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
