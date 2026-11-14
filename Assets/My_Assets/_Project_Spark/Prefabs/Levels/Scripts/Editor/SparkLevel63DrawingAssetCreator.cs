#if UNITY_EDITOR
using ProjectSpark.UI.VisualTeaching;
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.EditorTools
{
    public static class SparkLevel63DrawingAssetCreator
    {
        [MenuItem("Tools/Project Spark/Level 63/Create Teaching Drawing")]
        public static void CreateAsset()
        {
            SparkLevel63Builder.EnsureFolder();
            string path = SparkLevel63Builder.Folder + "/Level_63_Drawing.asset";
            if (AssetDatabase.LoadMainAssetAtPath(path) != null) return;
            var drawing = ScriptableObject.CreateInstance<SparkLevelDrawingAsset>();
            drawing.levelId = "level_63";
            drawing.levelTitle = "Catch the Frequency";
            drawing.pages = new[] {
                Page("01 / Build the inertia lab","58 WIRES  •  90 SOCKETS","Wire the sources, battery, bidirectional converter and frequency feedback. Install W58 last.","GRID","STORAGE","FEEDBACK"),
                Page("02 / Commission the reserve","KINETIC ENERGY  •  VIRTUAL INERTIA","Calibrate sensors, tune the response and charge the rotor before enabling disturbances.","CALIBRATE","ACCELERATE","RESERVE"),
                Page("03 / Arrest frequency loss","49.50 HZ NADIR  •  50 HZ RECOVERY","Trip the generator. Watch kinetic and battery support, then apply the motor and renewable disturbances.","GEN TRIP","MOTOR STEP","RENEWABLE LOSS"),
                Page("04 / Repair and certify","W58  •  SPEED ENCODER","Open the encoder link. Reconnect, reset, recalibrate and repeat an inertia response test.","ISOLATE","REPAIR","CERTIFY"),
            };
            AssetDatabase.CreateAsset(drawing, path);
            AssetDatabase.SaveAssets();
        }

        private static SparkLevelDrawingPage Page(string title,string caption,string explanation,string first,string middle,string last)
        {
            Color cyan = new(.18f,.86f,.94f);
            Color amber = new(1f,.68f,.31f);
            SparkSchematicPart Part(string id,float x,string label,Color color) =>
                new() {id=id, kind=SparkSchematicKind.Generic, position=new Vector2(x,.63f), label=label, color=color};
            return new SparkLevelDrawingPage {
                title=title, caption=caption, explanation=explanation,
                parts=new[] {Part("input",.18f,first,cyan),Part("tracking",.63f,middle,amber),Part("output",.82f,last,cyan)},
                wires=new[] {
                    new SparkSchematicWire {color=cyan,points=new[] {new Vector2(.25f,.63f),new Vector2(.43f,.63f)}},
                    new SparkSchematicWire {color=amber,points=new[] {new Vector2(.63f,.63f),new Vector2(.75f,.63f)}}
                }
            };
        }
    }
}
#endif
