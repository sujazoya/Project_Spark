#if UNITY_EDITOR
using ProjectSpark.UI.VisualTeaching;
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.EditorTools
{
    public static class SparkLevel70DrawingAssetCreator
    {
        [MenuItem("Tools/Project Spark/Level 70/Create Teaching Drawing")]
        public static void CreateAsset()
        {
            SparkLevel70Builder.EnsureFolder();
            string path = SparkLevel70Builder.Folder + "/Level_70_Drawing.asset";
            if (AssetDatabase.LoadMainAssetAtPath(path) != null) return;
            var drawing = ScriptableObject.CreateInstance<SparkLevelDrawingAsset>();
            drawing.levelId = "level_70";
            drawing.levelTitle = "Guard the Air";
            drawing.pages = new[] {
                Page("01 / Wire the cleanroom","86 WIRES  •  134 SOCKETS","Connect blowers, FFUs, sensors, dampers, doors and safety. Install W86 last.","POWER","SENSORS","SAFETY"),
                Page("02 / Commission and stabilize","+15 Pa  /  +8 Pa  •  22°C  •  45% RH","Calibrate, start all five fans, tune pressure and airflow, then enable automatic environmental control.","FLOW","PRESSURE","CLIMATE"),
                Page("03 / Protect the wafer","PARTICLE SAMPLES  •  FILTER DP","Inspect wafer exposure, exercise the airlock, verify loading and isolate the failed HEPA cartridge before replacement.","WAFER","SERVICE","PURGE"),
                Page("04 / Restore pressure feedback","W86  •  SAFE VENTILATION  •  RECERTIFY","Disconnect the actual pressure feedback wire. Restore and recalibrate the channel, reset, then repeat the door and wafer tests.","ISOLATE","RECONNECT","CERTIFY"),
            };
            AssetDatabase.CreateAsset(drawing, path);
            AssetDatabase.SaveAssets();
        }

        private static SparkLevelDrawingPage Page(string title,string caption,string explanation,string first,string middle,string last)
        {
            Color cyan = new(.18f,.86f,.94f);
            Color amber = new(1f,.70f,.31f);
            SparkSchematicPart Part(string id,float x,string label,Color color) =>
                new() {id=id, kind=SparkSchematicKind.Generic, position=new Vector2(x,.70f), label=label, color=color};
            return new SparkLevelDrawingPage {
                title=title, caption=caption, explanation=explanation,
                parts=new[] {Part("input",.18f,first,cyan),Part("tracking",.70f,middle,amber),Part("output",.82f,last,cyan)},
                wires=new[] {
                    new SparkSchematicWire {color=cyan,points=new[] {new Vector2(.25f,.70f),new Vector2(.43f,.70f)}},
                    new SparkSchematicWire {color=amber,points=new[] {new Vector2(.70f,.70f),new Vector2(.75f,.70f)}}
                }
            };
        }
    }
}
#endif
