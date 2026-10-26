#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using ProjectSpark.AI.Tutorial;

namespace ProjectSpark.AI.Tutorial.Editor
{
    public static class SparkTutorialSampleCreator
    {
        [MenuItem("Tools/ProjectSpark/AI/Create Getting Started Tutorial")]
        public static void Create()
        {
            string selectedFolder = "Assets";
            foreach (Object selection in Selection.GetFiltered(typeof(Object), SelectionMode.Assets))
            {
                string p = AssetDatabase.GetAssetPath(selection);
                if (AssetDatabase.IsValidFolder(p)) { selectedFolder = p; break; }
            }
            var tutorial = ScriptableObject.CreateInstance<SparkTutorialDefinition>();
            tutorial.tutorialId = "project_spark_first_circuit";
            tutorial.displayName = "Build Your First Circuit";
            tutorial.steps.Add(Step("welcome", "Meet your AI lab instructor", "Welcome to ProjectSpark. I'll guide you through the tools, explain what the components do, and let you build the circuit yourself.", "You can repeat explanations with F2 and request help with F1.", SparkTutorialAdvance.ContinueButton, "", SparkTutorialFocus.None, ""));
            tutorial.steps.Add(Step("resistor_intro", "What is a resistor?", "A resistor limits electric current. Its resistance is measured in ohms. A 220-ohm resistor usually passes less current than a 100-ohm resistor at the same voltage.", "The colored bands help identify its resistance.", SparkTutorialAdvance.ContinueButton, "", SparkTutorialFocus.WorldTransform, "shelf.resistor"));
            tutorial.steps.Add(Step("select", "Pick a component", "Click a resistor in your storage shelf to select it. You can also drag it to the workstation.", "Choose an available resistor from the shelf.", SparkTutorialAdvance.GameplaySignal, "shelf.item_selected", SparkTutorialFocus.WorldTransform, "shelf.resistor"));
            tutorial.steps.Add(Step("workstation", "Place it on the workstation", "Place the selected resistor into the resistor workstation slot. The shelf manager will verify that the slot is compatible and available.", "Click the empty resistor slot, or drag the resistor over it and release.", SparkTutorialAdvance.GameplaySignal, "shelf.item_placed", SparkTutorialFocus.WorldTransform, "workstation.resistor"));
            tutorial.steps.Add(Step("replace", "Move or replace parts", "To change a resistor, disconnect attached wires first. Drag the resistor back to storage, then place a different value in the empty slot.", "Powered or connected parts may be protected from unsafe moves.", SparkTutorialAdvance.ContinueButton, "", SparkTutorialFocus.WorldTransform, "workstation.resistor"));
            tutorial.steps.Add(Step("switch", "How a switch works", "A switch opens or closes a conductive path. A closed switch can allow current through a complete circuit. An open switch interrupts that path.", "A switch alone doesn't make a circuit work; the full path must be complete.", SparkTutorialAdvance.ContinueButton, "", SparkTutorialFocus.WorldTransform, "device.switch"));
            tutorial.steps.Add(Step("wire", "Connect the circuit", "Click one terminal, then the destination terminal, to create a wire. Follow the terminals highlighted by your level instructions.", "Only valid terminal combinations will connect.", SparkTutorialAdvance.GameplaySignal, "circuit.connection_created", SparkTutorialFocus.WorldTransform, "terminal.start"));
            tutorial.steps.Add(Step("current", "Voltage, current and power", "Voltage is electrical potential difference. Current is charge flow. For an ideal resistor, I equals V divided by R, and dissipated power is I squared times R.", "Too much dissipated power can overheat and damage a resistor.", SparkTutorialAdvance.ContinueButton, "", SparkTutorialFocus.None, ""));
            tutorial.steps.Add(Step("finish", "You're ready to experiment", "You know how to select, place and replace parts, what resistors and switches do, and how to begin wiring. Try the level objective. Ask the AI instructor for help whenever needed.", "You can revisit this tutorial from your Help menu.", SparkTutorialAdvance.ContinueButton, "", SparkTutorialFocus.None, ""));
            string path = AssetDatabase.GenerateUniqueAssetPath(selectedFolder + "/Tutorial_GettingStarted.asset");
            AssetDatabase.CreateAsset(tutorial, path);
            AssetDatabase.SaveAssets();
            Selection.activeObject = tutorial;
            EditorGUIUtility.PingObject(tutorial);
            Debug.Log("[Tutorial] Created " + path);
        }
        private static SparkTutorialStep Step(string id, string title, string text, string hint, SparkTutorialAdvance advance, string signal, SparkTutorialFocus focus, string locator)
        {
            return new SparkTutorialStep { id = id, title = title, explanation = text, hint = hint, advance = advance, requiredSignal = signal, focus = focus, locatorKey = locator, autoHintAfterSeconds = 16f };
        }
    }
}
#endif
