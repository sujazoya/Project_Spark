#if UNITY_EDITOR
using ProjectSpark.UI.VisualTeaching;
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.EditorTools
{
    public static class SparkLevel68DrawingAssetCreator
    {
        [MenuItem("Tools/Project Spark/Level 68/Create Teaching Drawing")]
        public static void CreateAsset()
        {
            SparkLevel68Builder.EnsureFolder();
            string path = SparkLevel68Builder.Folder + "/Level_68_Drawing.asset";
            if (AssetDatabase.LoadMainAssetAtPath(path) != null) return;
            var drawing = ScriptableObject.CreateInstance<SparkLevelDrawingAsset>();
            drawing.levelId = "level_68";
            drawing.levelTitle = "Tame the Heat";
            drawing.pages = new[] {
                Page("01 / Wire the thermal network","78 WIRES  •  122 SOCKETS","Route heater and pump drive permissions, RTDs, valves and safety. Install W78 last.","POWER","SENSORS","SAFETY"),
                Page("02 / Commission and tune","65°C PRODUCT  •  3 LOOPS","Calibrate temperature, flow and pressure. Start the three pumps, enable heat and tune the two cascade loops.","CALIBRATE","CIRCULATE","TUNE"),
                Page("03 / Recover waste heat","PREHEAT  •  UTILITY  •  BYPASS","Compare stable heating duty before and after closing the regenerative bypass. Test inlet temperature, surge and fouling.","BASELINE","RECOVER","VERIFY"),
                Page("04 / Restore the outlet RTD","W78  •  INVALID  •  SAFE","Remove W78 and verify heater isolation. Reconnect, calibrate, reset, recommission and repeat a disturbance before certification.","ISOLATE","RECONNECT","CERTIFY"),
            };
            AssetDatabase.CreateAsset(drawing, path);
            AssetDatabase.SaveAssets();
        }

        private static SparkLevelDrawingPage Page(string title,string caption,string explanation,string first,string middle,string last)
        {
            Color cyan = new(.18f,.86f,.94f);
            Color amber = new(1f,.68f,.31f);
            SparkSchematicPart Part(string id,float x,string label,Color color) =>
                new() {id=id, kind=SparkSchematicKind.Generic, position=new Vector2(x,.68f), label=label, color=color};
            return new SparkLevelDrawingPage {
                title=title, caption=caption, explanation=explanation,
                parts=new[] {Part("input",.18f,first,cyan),Part("tracking",.68f,middle,amber),Part("output",.82f,last,cyan)},
                wires=new[] {
                    new SparkSchematicWire {color=cyan,points=new[] {new Vector2(.25f,.68f),new Vector2(.43f,.68f)}},
                    new SparkSchematicWire {color=amber,points=new[] {new Vector2(.68f,.68f),new Vector2(.75f,.68f)}}
                }
            };
        }
    }
}
#endif
