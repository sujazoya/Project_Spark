#if UNITY_EDITOR
using System.Collections.Generic;
using ProjectSpark.UI.VisualTeaching;
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.EditorTools
{
    public static class SparkLevel39DrawingAssetCreator
    {
        private static readonly Color Power = new Color(.98f,.35f,.42f);
        private static readonly Color Tx = new Color(.99f,.73f,.31f);
        private static readonly Color Rx = new Color(.24f,.87f,.80f);
        private static readonly Color TxReturn = new Color(.28f,.69f,1f);
        private static readonly Color RxReturn = new Color(.65f,.53f,.99f);

        [MenuItem("Tools/Project Spark/Level 39/Create Teaching Drawing")]
        public static void CreateAsset()
        {
            SparkLevel39Builder.EnsureFolder();
            string path = SparkLevel39Builder.Folder + "/Level_39_Drawing.asset";
            if (AssetDatabase.LoadMainAssetAtPath(path) != null) return;
            var drawing = ScriptableObject.CreateInstance<SparkLevelDrawingAsset>();
            drawing.levelId = "level_39";
            drawing.levelTitle = "Across the Isolation Gap";
            drawing.pages = new[]
            {
                Page("01 / Wire the isolated circuits","SEPARATE 5 V SUPPLIES  •  SEPARATE RETURNS",
                    "W01 TX_POS → SEND_IN. W02 SEND_OUT → TX_R_IN. W03 TX_R_OUT → OPTO_A. W04 OPTO_K → TX_RETURN. W05 RX_POS → PULLUP_IN. W06 PULLUP_OUT → OPTO_C. W07 OPTO_E → RX_RETURN. Leave the returns separate.",
                    "TX OFF","RX HIGH",false),
                Page("02 / Send and release","ACTIVE-LOW OUTPUT  •  SEND ON MEANS RX LOW",
                    "Turn SEND on. The transmitter LED draws about 6.4 mA. The receiver contact closes and sinks about 5 mA through the 1 kΩ pull-up. RX drops near 0 V. Turn SEND off and the pull-up returns RX to about 5 V.",
                    "TX LED ON","RX LOW",false),
                Page("03 / Prove independent receiver power","THE RECEIVER HAS ITS OWN POWER SUPPLY",
                    "Turn SEND on again, then turn RX POWER off. The transmitter LED stays on and the receiver shows UNPOWERED. Restore RX POWER and confirm RX goes LOW again while SEND remains on.",
                    "TX LED ON","RX UNPOWERED",false),
                Page("04 / Detect and remove the bridge","THE OUTPUT CAN WORK WHILE ISOLATION IS LOST",
                    "Add the temporary W08 wire between TX_RETURN and RX_RETURN. RX may still read LOW while the indicator changes to BRIDGED. Remove W08, turn SEND off to see HIGH, then turn SEND on to verify LOW with isolation restored.",
                    "TX LED ON","RX LOW",true)
            };
            AssetDatabase.CreateAsset(drawing,path);
            AssetDatabase.SaveAssets();
        }

        private static SparkLevelDrawingPage Page(string title,string caption,string explanation,string txState,string rxState,bool bridge)
        {
            var wires = new List<SparkSchematicWire>
            {
                Wire(Power,P(.10f,.78f),P(.225f,.78f)),
                Wire(Tx,P(.315f,.78f),P(.385f,.78f)),
                Wire(Tx,P(.43f,.78f),P(.43f,.48f),P(.375f,.48f)),
                Wire(TxReturn,P(.285f,.48f),P(.16f,.48f),P(.16f,.27f)),
                Wire(Power,P(.90f,.78f),P(.775f,.78f)),
                Wire(Rx,P(.685f,.78f),P(.60f,.78f),P(.60f,.48f),P(.625f,.48f)),
                Wire(RxReturn,P(.715f,.48f),P(.84f,.48f),P(.84f,.27f))
            };
            if (bridge) wires.Add(Wire(Power,P(.16f,.27f),P(.16f,.19f),P(.84f,.19f),P(.84f,.27f)));
            return new SparkLevelDrawingPage
            {
                title=title, caption=caption, explanation=explanation,
                parts=new[]
                {
                    Part("tx-supply",SparkSchematicKind.Generic,.10f,.78f,"TX +5 V",Power),
                    Part("send",SparkSchematicKind.Generic,.27f,.78f,"SEND",Tx),
                    Part("limit",SparkSchematicKind.Resistor,.43f,.78f,"470 Ω",Tx),
                    Part("emitter",SparkSchematicKind.Generic,.33f,.48f,txState,Tx),
                    Part("gap",SparkSchematicKind.Generic,.50f,.48f,"LIGHT ONLY",Tx),
                    Part("receiver",SparkSchematicKind.Generic,.67f,.48f,rxState,Rx),
                    Part("pull-up",SparkSchematicKind.Resistor,.73f,.78f,"1 kΩ",Rx),
                    Part("rx-supply",SparkSchematicKind.Generic,.90f,.78f,"RX +5 V",Power),
                    Part("tx-return",SparkSchematicKind.Generic,.16f,.27f,"TX RETURN",TxReturn),
                    Part("rx-return",SparkSchematicKind.Generic,.84f,.27f,"RX RETURN",RxReturn)
                },
                wires=wires.ToArray()
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
