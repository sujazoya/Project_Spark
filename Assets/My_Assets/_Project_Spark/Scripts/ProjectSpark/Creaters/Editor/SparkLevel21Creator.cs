#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;
using ProjectSpark.Gameplay;
using ProjectSpark.UI.VisualTeaching;

namespace ProjectSpark.EditorTools
{
    /// <summary>
    /// Authors Level 21 without touching existing levels, scene circuits, or solver assets.
    /// A prefab scaffold is intentionally UNWIRED: real SparkTerminal prefabs must be installed.
    /// </summary>
    public static class SparkLevel21Creator
    {
        private const string Root = "Assets/ProjectSparkGenerated/Level_21";
        private static readonly Color Supply = new Color(.99f,.30f,.40f,1f);
        private static readonly Color Ground = new Color(.21f,.70f,1f,1f);
        private static readonly Color Gold = new Color(1f,.79f,.32f,1f);
        private static readonly Color Cyan = new Color(.28f,.95f,.98f,1f);

        [MenuItem("Tools/Project Spark/Levels/Create Level 21 - Capacitor Charge")]
        public static void Create()
        {
            if (!AssetDatabase.IsValidFolder("Assets/ProjectSparkGenerated"))
                AssetDatabase.CreateFolder("Assets", "ProjectSparkGenerated");
            if (!AssetDatabase.IsValidFolder(Root))
                AssetDatabase.CreateFolder("Assets/ProjectSparkGenerated", "Level_21");
            string levelPath = Root + "/Level_21.asset";
            string drawingPath = Root + "/Level_21_Drawing.asset";
            string prefabPath = Root + "/Level_21_Layout.prefab";
            if (AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(levelPath) != null ||
                AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(drawingPath) != null ||
                AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(prefabPath) != null)
            {
                EditorUtility.DisplayDialog("Level 21 exists", "One or more Level 21 assets already exist. No files were overwritten. Move or rename them if you want to generate another set.", "OK");
                return;
            }

            var definition = ScriptableObject.CreateInstance<SparkLevelDefinition>();
            SerializedObject so = new SerializedObject(definition);
            Set(so, "levelId", "level_21");
            Set(so, "displayName", "Capacitor Charge - Store the Energy");
            Set(so, "description", "Build a 5 V RC charging circuit. Observe the 1000 uF capacitor charging through a 1 kOhm resistor, and verify both capacitor voltage and charging current.");
            Set(so, "completionMode", 0);
            Set(so, "requiredTargetCount", 3);
            Set(so, "requireClosedReturn", true);
            Set(so, "rejectShortCircuit", true);
            Set(so, "rejectTargetShort", true);
            Set(so, "allowIntermediateConnections", true);
            Set(so, "requireMinimumVoltage", false);
            Set(so, "allowAnyConfiguredPowerSource", true);
            Set(so, "autoUnlockNextLevel", true);
            Set(so, "autoAdvanceOnCompletion", false);
            Set(so, "requireHealthyInstalledResistors", true);
            SerializedProperty sources = so.FindProperty("powerSources");
            sources.arraySize = 1;
            var src = sources.GetArrayElementAtIndex(0);
            Set(src, "sourceName", "PowerSupply_01");
            Set(src, "positiveTerminalId", "Battery_positive");
            Set(src, "negativeTerminalId", "Battery_negative");
            Set(src, "nominalVoltage", 5f);

            SerializedProperty targets = so.FindProperty("targets");
            targets.arraySize = 3;
            Target(targets.GetArrayElementAtIndex(0), "level21_supply", "5 V Supply", "Verify a healthy 5 V source.",
                5, "PowerSupply_01", "Battery_positive", "Battery_negative", 4.8f, 5.2f, false);
            Target(targets.GetArrayElementAtIndex(1), "level21_capacitor_voltage", "Capacitor Charging Voltage", "Measure voltage rise across the 1000 uF capacitor (a transient, time-dependent target).",
                5, "C_STORE_Component", "C_STORE_Input", "C_STORE_Output", 3f, 4.8f, false);
            Target(targets.GetArrayElementAtIndex(2), "level21_charge_current", "Safe Charge Current", "Observe nonzero current through the 1 kOhm charging resistor during charging.",
                6, "R_CHARGE_Component", "R_CHARGE_Input", "R_CHARGE_Output", .00015f, .0022f, true);
            so.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.CreateAsset(definition, levelPath);

            var drawing = ScriptableObject.CreateInstance<SparkLevelDrawingAsset>();
            drawing.levelId = "level_21";
            drawing.levelTitle = "Capacitor Charge - Store the Energy";
            drawing.initialPage = 0;
            drawing.pages = new SparkLevelDrawingPage[4];
            for (int i=0; i<4; i++) drawing.pages[i] = BuildPage(i);
            AssetDatabase.CreateAsset(drawing, drawingPath);

            // Prefab shell only: no copied old level items/wires, so there is no accidental Level 20 wiring.
            var root = new GameObject("Level_21_Layout");
            var layout = root.AddComponent<SparkLevelLayout>();
            var bindings = root.AddComponent<SparkLevelSceneBindings>();
            var layoutSO = new SerializedObject(layout);
            Set(layoutSO, "levelDefinition", definition);
            Set(layoutSO, "sceneBindings", bindings);
            layoutSO.ApplyModifiedPropertiesWithoutUndo();
            var bindingSO = new SerializedObject(bindings);
            SetArray(bindingSO, "runtimeComponentIds", "R_CHARGE_Component", "C_STORE_Component");
            SetArray(bindingSO, "runtimeTerminalIds", "R_CHARGE_Input", "R_CHARGE_Output", "C_STORE_Input", "C_STORE_Output");
            bindingSO.ApplyModifiedPropertiesWithoutUndo();
            PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            UnityEngine.Object.DestroyImmediate(root);

            AssetDatabase.SaveAssets();
            Selection.activeObject = definition;
            EditorGUIUtility.PingObject(definition);
            Debug.Log("[Level21] Created Level Definition, four-page Drawing Asset and UNWIRED Layout prefab at " + Root + ". Author the actual workbench and bind the fixed battery terminals before playing.");
            EditorUtility.DisplayDialog("Level 21 scaffolding created", "3 assets created at " + Root + ". The layout is UNWIRED by design. Follow the included build guide before Play Mode.", "OK");
        }

        private static void Set(SerializedObject so, string key, string value) { var p=so.FindProperty(key); if(p==null) throw new Exception("Missing property " + key); p.stringValue=value; }
        private static void Set(SerializedObject so, string key, bool value) { var p=so.FindProperty(key); if(p==null) throw new Exception("Missing property " + key); p.boolValue=value; }
        private static void Set(SerializedObject so, string key, int value) { var p=so.FindProperty(key); if(p==null) throw new Exception("Missing property " + key); p.intValue=value; }
        private static void Set(SerializedProperty p, string key, string value) { var c=p.FindPropertyRelative(key); if(c==null) throw new Exception("Missing field " + key); c.stringValue=value; }
        private static void Set(SerializedProperty p, string key, bool value) { var c=p.FindPropertyRelative(key); if(c==null) throw new Exception("Missing field " + key); c.boolValue=value; }
        private static void Set(SerializedProperty p, string key, int value) { var c=p.FindPropertyRelative(key); if(c==null) throw new Exception("Missing field " + key); c.intValue=value; }
        private static void Set(SerializedProperty p, string key, float value) { var c=p.FindPropertyRelative(key); if(c==null) throw new Exception("Missing field " + key); c.floatValue=value; }
        private static void SetArray(SerializedObject obj, string key, params string[] vals) { var p=obj.FindProperty(key); if(p==null) throw new Exception("Missing property " + key); p.arraySize=vals.Length; for(int i=0;i<vals.Length;i++)p.GetArrayElementAtIndex(i).stringValue=vals[i]; }
        private static void Target(SerializedProperty t, string id, string name, string info, int type, string component, string a, string b, float min, float max, bool current)
        {
            Set(t,"targetId",id); Set(t,"displayName",name); Set(t,"description",info); Set(t,"targetType",type);
            Set(t,"targetComponentId",component); Set(t,"requiredPositiveTerminalId",a); Set(t,"requiredNegativeTerminalId",b);
            Set(t,"requireElectricalEnabled",true); Set(t,"requireConduction",false);
            if(current) { Set(t,"minimumCurrent",min); Set(t,"useMaximumCurrent",true); Set(t,"maximumCurrent",max); }
            else { Set(t,"minimumVoltage",min); Set(t,"useMaximumVoltage",true); Set(t,"maximumVoltage",max); }
        }
        private static Vector2 P(float x,float y) => new Vector2(x,y);
        private static SparkSchematicPart Part(string id,SparkSchematicKind k,float x,float y,string label,Color c) =>
            new SparkSchematicPart { id=id,kind=k,position=P(x,y),label=label,color=c };
        private static SparkSchematicWire Wire(Color c, params Vector2[] points) => new SparkSchematicWire {color=c,energized=false,points=points};
        private static SparkLevelDrawingPage BuildPage(int i)
        {
            var parts = new [] {
                Part("battery",SparkSchematicKind.Battery,.12f,.50f,"5 V",Supply),
                Part("switch",SparkSchematicKind.Switch,.29f,.80f,"SW1",Gold),
                Part("resistor",SparkSchematicKind.Resistor,.49f,.80f,"R 1 kOhm",Gold),
                // Generic capacitor symbol is used here because older schematic renderers may lack Capacitor kind.
                Part("capacitor",SparkSchematicKind.Generic,.76f,.55f,"C 1000 uF",Cyan),
                Part("lcd",SparkSchematicKind.Meter,.54f,.36f,"LCD Vcap",Cyan),
                Part("ground",SparkSchematicKind.Ground,.46f,.15f,"GND",Ground)
            };
            var wires = new [] {
                Wire(Supply,P(.12f,.64f),P(.12f,.80f),P(.23f,.80f)),
                Wire(Supply,P(.35f,.80f),P(.43f,.80f)),
                Wire(Supply,P(.55f,.80f),P(.76f,.80f),P(.76f,.62f)),
                Wire(Ground,P(.76f,.48f),P(.76f,.15f),P(.46f,.15f),P(.12f,.15f),P(.12f,.36f)),
                Wire(Cyan,P(.76f,.80f),P(.63f,.80f),P(.63f,.36f),P(.60f,.36f)),
                Wire(Ground,P(.48f,.36f),P(.46f,.36f),P(.46f,.15f))
            };
            string[] titles={"LEVEL 21 | Capacitor Charging | Overview","LEVEL 21 | Exact Wiring Guide","LEVEL 21 | Voltage Rise & Charge Current","LEVEL 21 | Correct Circuit & 3 Objectives"};
            string[] captions={"PAGE 1/4 | 5 V | R 1 kOhm | C 1000 uF","PAGE 2/4 | SERIES RC + LCD ACROSS CAPACITOR","PAGE 3/4 | TIME CONSTANT 1 SECOND","PAGE 4/4 | LIVE MEASUREMENT REQUIRED"};
            string[] descriptions={
                "A capacitor stores energy in an electric field. With SW1 closed, it charges through the 1 kOhm resistor. The LCD is only a high-impedance voltmeter across capacitor terminals; it is not the return wire.",
                "Wire battery positive to SW1 input; SW1 output to R_CHARGE_Input; R_CHARGE_Output to C_STORE_Input; C_STORE_Output to battery negative. LCD A to C_STORE_Input, LCD B to C_STORE_Output. Set C_STORE capacitance to 0.001 F (1000 uF), with voltage rating >= 5 V.",
                "Ideal estimates, not live measurements: R=1000 Ohm, C=0.001 F, tau=RC=1 s. Starting uncharged: Vcap(1 s)=3.16 V, Vcap(2 s)=4.32 V, Vcap(3 s)=4.75 V. Initial resistor current 5 mA decays toward zero.",
                "Three target windows: supply 4.8-5.2 V; capacitor 3.0-4.8 V; charging resistor current 0.15-2.2 mA. All three overlap during the transient, so switch the source ON and watch. Only SparkLevelEvaluator awards completion. If solver updates do not satisfy targets together, adjust after live testing."
            };
            return new SparkLevelDrawingPage {title=titles[i],caption=captions[i],explanation=descriptions[i],parts=parts,wires=wires};
        }
    }
}
#endif
