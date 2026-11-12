#if UNITY_EDITOR
using ProjectSpark.UI.VisualTeaching;
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.EditorTools
{
    public static class SparkLevel36DrawingAssetCreator
    {
        private static readonly Color Positive = new Color(.98f, .35f, .42f);
        private static readonly Color Return = new Color(.28f, .69f, 1f);
        private static readonly Color Sense = new Color(.24f, .87f, .80f);
        private static readonly Color Heat = new Color(.99f, .71f, .28f);
        private static readonly Color Fault = new Color(1f, .49f, .25f);

        [MenuItem("Tools/Project Spark/Level 36/Create Teaching Drawing")]
        public static void CreateAsset()
        {
            SparkLevel36Builder.EnsureFolder();
            string path = SparkLevel36Builder.Folder + "/Level_36_Drawing.asset";
            if (AssetDatabase.LoadMainAssetAtPath(path) != null) return;
            var drawing = ScriptableObject.CreateInstance<SparkLevelDrawingAsset>();
            drawing.levelId = "level_36";
            drawing.levelTitle = "Thermal Interlock";
            drawing.pages = new[]
            {
                SensorPage("01 / Wire the sensor divider", "5 V → 10 kΩ → SENSE → NTC → RETURN",
                    "W01 Battery_positive → PULLUP_IN. W02 PULLUP_OUT → SENSOR_IN. W03 SENSOR_RETURN → Battery_negative. Cool NTC is 15 kΩ and SENSE is about 3.00 V. The pull-up limits sensor current even in the short test.", "NTC · 15 kΩ", "SENSE 3.00 V"),
                OutputsPage(),
                SensorPage("03 / Test both sides of the warm band", "FAN ON BELOW 2.00 V  •  FAN OFF ABOVE 2.70 V",
                    "HOT reads 1.60 V and starts cooling. WARM reads 2.25 V and keeps cooling on. COOL reads 3.00 V and stops cooling. WARM again reads 2.25 V with cooling off. The same voltage keeps whichever state was already active.", "NTC · 8.2 kΩ", "SENSE 2.25 V"),
                SensorPage("04 / Test the fault latch", "OPEN ≈ 5 V  •  SHORT ≈ 0 V",
                    "Both faults cut the heater, run the fan, and light the fault LED. Restore the sensor after each fault and watch the warning stay on. RESET FAULT is available after a valid cool reading returns. Finish both tests and verify normal outputs.", "NTC · OPEN / SHORT", "SENSE → FAULT")
            };
            AssetDatabase.CreateAsset(drawing, path);
            AssetDatabase.SaveAssets();
        }

        private static SparkLevelDrawingPage SensorPage(string title,string caption,string explanation,string sensor,string sense)
        {
            return new SparkLevelDrawingPage
            {
                title = title, caption = caption, explanation = explanation,
                parts = new[]
                {
                    Part("positive", SparkSchematicKind.Generic, .09f, .56f, "BATTERY +", Positive),
                    Part("pullup", SparkSchematicKind.Resistor, .31f, .56f, "10 kΩ PULL-UP", Sense),
                    Part("sense", SparkSchematicKind.Generic, .50f, .71f, sense, Sense),
                    Part("ntc", SparkSchematicKind.Resistor, .69f, .56f, sensor, Sense),
                    Part("negative", SparkSchematicKind.Generic, .91f, .56f, "BATTERY −", Return)
                },
                wires = new[]
                {
                    Wire(Positive, P(.09f,.56f), P(.265f,.56f)),
                    Wire(Sense, P(.355f,.56f), P(.645f,.56f)),
                    Wire(Sense, P(.50f,.56f), P(.50f,.68f)),
                    Wire(Return, P(.735f,.56f), P(.91f,.56f))
                }
            };
        }

        private static SparkLevelDrawingPage OutputsPage()
        {
            return new SparkLevelDrawingPage
            {
                title = "02 / Wire the three outputs", caption = "FAN, HEATER, AND FAULT LED BRANCHES",
                explanation = "W04 + → FAN_SW_IN, W05 FAN_SW_OUT → FAN_A, W06 FAN_B → −. W07 + → HEAT_SW_IN, W08 HEAT_SW_OUT → HEATER_IN, W09 HEATER_OUT → −. W10 + → FAULT_SW_IN, W11 FAULT_SW_OUT → LED_R_IN, W12 LED_R_OUT → LED_A, W13 LED_K → −.",
                parts = new[]
                {
                    Part("positive",SparkSchematicKind.Generic,.09f,.54f,"BATTERY +",Positive),
                    Part("fan-contact",SparkSchematicKind.Generic,.29f,.78f,"FAN CONTACT",Sense),
                    Part("heat-contact",SparkSchematicKind.Generic,.29f,.54f,"HEATER CONTACT",Heat),
                    Part("fault-contact",SparkSchematicKind.Generic,.29f,.30f,"FAULT CONTACT",Fault),
                    Part("fan",SparkSchematicKind.Generic,.62f,.78f,"FAN MOTOR",Sense),
                    Part("heater",SparkSchematicKind.Resistor,.62f,.54f,"HEATER 100 Ω",Heat),
                    Part("limiter",SparkSchematicKind.Resistor,.51f,.30f,"330 Ω",Fault),
                    Part("led",SparkSchematicKind.Generic,.71f,.30f,"FAULT LED",Fault),
                    Part("negative",SparkSchematicKind.Generic,.91f,.54f,"BATTERY −",Return)
                },
                wires = new[]
                {
                    Wire(Positive,P(.09f,.54f),P(.09f,.78f),P(.245f,.78f)),
                    Wire(Positive,P(.09f,.54f),P(.245f,.54f)),
                    Wire(Positive,P(.09f,.54f),P(.09f,.30f),P(.245f,.30f)),
                    Wire(Sense,P(.335f,.78f),P(.575f,.78f)),
                    Wire(Heat,P(.335f,.54f),P(.575f,.54f)),
                    Wire(Fault,P(.335f,.30f),P(.465f,.30f)),
                    Wire(Fault,P(.555f,.30f),P(.665f,.30f)),
                    Wire(Return,P(.665f,.78f),P(.91f,.78f),P(.91f,.54f)),
                    Wire(Return,P(.665f,.54f),P(.91f,.54f)),
                    Wire(Return,P(.755f,.30f),P(.91f,.30f),P(.91f,.54f))
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
