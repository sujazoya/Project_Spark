#if UNITY_EDITOR
using ProjectSpark.UI.VisualTeaching;
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.EditorTools
{
    public static class SparkLevel44DrawingAssetCreator
    {
        private static readonly Color Ladder = new Color(.24f,.87f,.80f);
        private static readonly Color Digital = new Color(.99f,.73f,.31f);
        private static readonly Color Return = new Color(.28f,.69f,1f);
        private static readonly Color Fault = new Color(.98f,.35f,.44f);

        [MenuItem("Tools/Project Spark/Level 44/Create Teaching Drawing")]
        public static void CreateAsset()
        {
            SparkLevel44Builder.EnsureFolder();
            string path=SparkLevel44Builder.Folder+"/Level_44_Drawing.asset";
            if (AssetDatabase.LoadMainAssetAtPath(path)!=null) return;
            var drawing=ScriptableObject.CreateInstance<SparkLevelDrawingAsset>();
            drawing.levelId="level_44";
            drawing.levelTitle="Find Home";
            drawing.pages=new[]
            {
                Page("01 / Wire the encoder","A AND B  •  Z INDEX  •  5 V",
                    "W01 supply + to decoder +. W02 supply return to decoder return. W03 ENC Z to DEC Z. W04 ENC A to DEC A. W05 ENC B to DEC B. Fit the A/B wires last so you can swap them later.",
                    "A → A", "B → B", false),
                Page("02 / Find home and decode direction","80 COUNTS PER TURN  •  4.5° PER COUNT",
                    "Home rotates clockwise until the unique Z pulse resets the count. Clockwise gives 00 → 10 → 11 → 01 → 00, so A leads. Counterclockwise reverses that sequence. Station A is +20 counts and Station B is +52.",
                    "A LEADS CW", "B LEADS CCW", false),
                Page("03 / Repair crossed A and B","TABLE CW  •  COUNT FALLING",
                    "Swap the A and B leads at the decoder. TEST DIRECTION compares commanded clockwise travel with the decoded count. It stops after three backward counts. Repair both leads and re-home.",
                    "A → B", "B → A", true),
                Page("04 / Recover the reference","PHYSICAL ANGLE STAYS  •  COUNT REFERENCE IS LOST",
                    "Move to B and cycle decoder power. The platter stays at 234°. After power-up, HOME restores a known zero. Move to B again and check that the position repeats.",
                    "HOME ON Z", "B = +52", false)
            };
            AssetDatabase.CreateAsset(drawing,path);
            AssetDatabase.SaveAssets();
        }

        private static SparkLevelDrawingPage Page(string title,string caption,string explanation,string a,string b,bool crossed)
        {
            return new SparkLevelDrawingPage
            {
                title=title, caption=caption, explanation=explanation,
                parts=new[]
                {
                    Part("source",SparkSchematicKind.Generic,.12f,.80f,"5 V + RETURN",Return),
                    Part("encoder",SparkSchematicKind.Generic,.14f,.48f,"ENCODER",Ladder),
                    Part("decoder",SparkSchematicKind.Generic,.82f,.48f,"DECODER",Digital),
                    Part("phaseA",SparkSchematicKind.Generic,.47f,.64f,a,crossed ? Fault : Ladder),
                    Part("phaseB",SparkSchematicKind.Generic,.47f,.30f,b,crossed ? Fault : Return),
                    Part("index",SparkSchematicKind.Generic,.47f,.48f,"Z INDEX",Digital)
                },
                wires=new[]
                {
                    Wire(Return,P(.12f,.80f),P(.82f,.80f),P(.82f,.57f)),
                    Wire(crossed ? Fault : Ladder,P(.18f,.60f),P(.33f,.60f),P(.67f,crossed ? .34f : .60f),P(.78f,crossed ? .34f : .60f)),
                    Wire(crossed ? Fault : Return,P(.18f,.34f),P(.33f,.34f),P(.67f,crossed ? .60f : .34f),P(.78f,crossed ? .60f : .34f)),
                    Wire(Digital,P(.18f,.48f),P(.78f,.48f))
                }
            };
        }

        private static SparkSchematicPart Part(string id,SparkSchematicKind kind,float x,float y,string label,Color color) =>
            new SparkSchematicPart { id=id,kind=kind,position=P(x,y),label=label,color=color };

        private static SparkSchematicWire Wire(Color color,params Vector2[] points) =>
            new SparkSchematicWire { color=color,points=points };

        private static Vector2 P(float x,float y) => new Vector2(x,y);
    }
}
#endif
