#if UNITY_EDITOR
using ProjectSpark.UI.VisualTeaching;
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.EditorTools
{
    public static class SparkLevel38DrawingAssetCreator
    {
        private static readonly Color Power = new Color(.98f,.35f,.42f);
        private static readonly Color Raw = new Color(.99f,.73f,.31f);
        private static readonly Color Filtered = new Color(.24f,.87f,.80f);
        private static readonly Color Return = new Color(.28f,.69f,1f);

        [MenuItem("Tools/Project Spark/Level 38/Create Teaching Drawing")]
        public static void CreateAsset()
        {
            SparkLevel38Builder.EnsureFolder();
            string path = SparkLevel38Builder.Folder + "/Level_38_Drawing.asset";
            if (AssetDatabase.LoadMainAssetAtPath(path) != null) return;
            var drawing = ScriptableObject.CreateInstance<SparkLevelDrawingAsset>();
            drawing.levelId = "level_38";
            drawing.levelTitle = "One Press, One Count";
            drawing.pages = new[]
            {
                CircuitPage(),
                TracePage("02 / Watch the raw overcount","RAW TEST  •  THREE RISING EDGES",
                    "Run RAW TEST. One press closes, opens, closes, opens, and closes again. Count the three upward steps. The test ends with a clean release and a short wait for the capacitor to discharge.",
                    new[] {P(.10f,.25f),P(.18f,.25f),P(.18f,.76f),P(.25f,.76f),P(.25f,.25f),P(.32f,.25f),P(.32f,.76f),P(.39f,.76f),P(.39f,.25f),P(.46f,.25f),P(.46f,.76f),P(.89f,.76f)}, null),
                TracePage("03 / Reject a glitch and accept a press","FILTERED MODE  •  3.0 V PRESS THRESHOLD",
                    "Select FILTERED MODE, then inject a 100 ms glitch. The filtered voltage stays under 3.0 V, so the count stays at zero. Run BOUNCED PRESS. The contact settles closed and the filtered voltage crosses 3.0 V once.",
                    new[] {P(.10f,.25f),P(.22f,.25f),P(.22f,.76f),P(.29f,.76f),P(.29f,.25f),P(.49f,.25f),P(.49f,.76f),P(.54f,.76f),P(.54f,.25f),P(.59f,.25f),P(.59f,.76f),P(.89f,.76f)},
                    new[] {P(.10f,.25f),P(.22f,.25f),P(.29f,.33f),P(.39f,.28f),P(.49f,.26f),P(.59f,.37f),P(.69f,.56f),P(.79f,.66f),P(.89f,.70f)}),
                TracePage("04 / Release and rearm","2.0 V REARM THRESHOLD  •  TWO COMPLETE CYCLES",
                    "Run BOUNCED RELEASE. The counter stays at one while FILTERED falls below 2.0 V and rearms. Run SECOND CYCLE. The filtered signal crosses 3.0 V once on the way up and 2.0 V once on the way down. The count finishes at two.",
                    new[] {P(.10f,.76f),P(.21f,.76f),P(.21f,.25f),P(.28f,.25f),P(.28f,.76f),P(.34f,.76f),P(.34f,.25f),P(.89f,.25f)},
                    new[] {P(.10f,.70f),P(.21f,.70f),P(.34f,.64f),P(.47f,.53f),P(.61f,.44f),P(.75f,.38f),P(.89f,.34f)})
            };
            AssetDatabase.CreateAsset(drawing,path);
            AssetDatabase.SaveAssets();
        }

        private static SparkLevelDrawingPage CircuitPage() => new SparkLevelDrawingPage
        {
            title = "01 / Wire the debounce circuit",
            caption = "5 V SUPPLY  •  RAW AT FEED OUT  •  FILTERED AT CAP +",
            explanation = "W01 Battery_positive → SW_IN. W02 SW_OUT → FEED_IN. W03 FEED_OUT → PULLDOWN_IN. W04 FEED_OUT → FILTER_IN. W05 PULLDOWN_OUT → Battery_negative. W06 FILTER_OUT → CAP_PLUS. W07 CAP_MINUS → Battery_negative.",
            parts = new[]
            {
                Part("positive",SparkSchematicKind.Generic,.08f,.77f,"BATTERY +",Power),
                Part("switch",SparkSchematicKind.Generic,.25f,.77f,"PUSHBUTTON",Power),
                Part("feed",SparkSchematicKind.Resistor,.43f,.77f,"FEED 1 kΩ",Raw),
                Part("pull",SparkSchematicKind.Resistor,.58f,.48f,"PULL-DOWN 10 kΩ",Raw),
                Part("filter",SparkSchematicKind.Resistor,.70f,.77f,"FILTER 10 kΩ",Filtered),
                Part("cap",SparkSchematicKind.Generic,.89f,.55f,"47 µF  + / −",Filtered),
                Part("negative",SparkSchematicKind.Generic,.74f,.23f,"BATTERY −",Return)
            },
            wires = new[]
            {
                Wire(Power,P(.08f,.77f),P(.205f,.77f)),
                Wire(Power,P(.295f,.77f),P(.385f,.77f)),
                Wire(Raw,P(.475f,.77f),P(.52f,.77f),P(.52f,.48f),P(.535f,.48f)),
                Wire(Raw,P(.475f,.77f),P(.655f,.77f)),
                Wire(Return,P(.625f,.48f),P(.67f,.48f),P(.67f,.23f),P(.74f,.23f)),
                Wire(Filtered,P(.745f,.77f),P(.81f,.77f),P(.81f,.55f),P(.845f,.55f)),
                Wire(Return,P(.935f,.55f),P(.96f,.55f),P(.96f,.23f),P(.74f,.23f))
            }
        };

        private static SparkLevelDrawingPage TracePage(string title,string caption,string explanation,Vector2[] raw,Vector2[] filtered)
        {
            var reference = new Color(.38f,.52f,.60f);
            return new SparkLevelDrawingPage
            {
                title=title, caption=caption, explanation=explanation,
                parts=filtered == null
                    ? new[] {Part("raw",SparkSchematicKind.Generic,.50f,.88f,"RAW",Raw)}
                    : new[] {Part("raw",SparkSchematicKind.Generic,.34f,.88f,"RAW",Raw),
                        Part("filtered",SparkSchematicKind.Generic,.66f,.88f,"FILTERED",Filtered)},
                wires=filtered == null
                    ? new[] {Wire(reference,P(.10f,.25f),P(.92f,.25f)),Wire(Raw,raw)}
                    : new[] {Wire(reference,P(.10f,.25f),P(.92f,.25f)),
                        Wire(reference,P(.10f,.55f),P(.92f,.55f)),
                        Wire(reference,P(.10f,.45f),P(.92f,.45f)),Wire(Raw,raw),Wire(Filtered,filtered)}
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
