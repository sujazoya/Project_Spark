#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.AI.Editor
{
    /// <summary>
    /// Editor utility for creating the initial Project Spark
    /// Level 1 electronics knowledge database.
    ///
    /// Editor-only.
    /// Does not run during gameplay.
    /// </summary>
    public static class SparkAIKnowledgeLevel1Editor
    {
        private const string AssetFolder =
            "Assets/My_Assets/_Project_Spark/Data/AI";

        private const string AssetPath =
            AssetFolder + "/SparkAIKnowledge.asset";

       [MenuItem("Project Spark/AI/Create Level 1 Knowledge")]
        public static void CreateLevel1Knowledge()
        {
            SparkAIKnowledge knowledge =
                AssetDatabase.LoadAssetAtPath<SparkAIKnowledge>(
                    AssetPath);

            if (knowledge == null)
            {
                EnsureFolderExists();

                knowledge =
                    ScriptableObject.CreateInstance<SparkAIKnowledge>();

                AssetDatabase.CreateAsset(
                    knowledge,
                    AssetPath);
            }

            Undo.RecordObject(
                knowledge,
                "Populate Project Spark Level 1 Knowledge");

            SerializedObject serializedKnowledge =
                new SerializedObject(knowledge);

            SerializedProperty entries =
                serializedKnowledge.FindProperty("entries");

            entries.ClearArray();

            AddCircuit(entries);
            AddElectricalSource(entries);
            AddCircuitPath(entries);
            AddPolarity(entries);
            AddOpenCircuit(entries);
            AddClosedCircuit(entries);
            AddShortCircuit(entries);
            AddWire(entries);
            AddLED(entries);

            serializedKnowledge.ApplyModifiedProperties();

            EditorUtility.SetDirty(knowledge);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log(
                "[SPARK AI KNOWLEDGE] Level 1 knowledge created. " +
                "Entries: 9");
        }

        // ============================================================
        // ENTRIES
        // ============================================================

        private static void AddCircuit(
            SerializedProperty entries)
        {
            AddEntry(
                entries,
                "circuit",
                "Circuit",
                "Circuit",
                new[]
                {
                    "electrical circuit",
                    "basic circuit",
                    "circuit basics"
                },
                "An electrical circuit is a connected path containing " +
                "a source and electrical components that allows electrical " +
                "energy to be transferred through the circuit.",
                "A basic circuit needs a source and a continuous " +
                "electrical path through the connected components and " +
                "back to the other terminal of the source.",
                "A battery connected to an LED through suitable wiring " +
                "and a current-limiting resistor forms a basic electrical " +
                "circuit when the path returns to the battery.",
                new[]
                {
                    "A circuit needs an electrical source.",
                    "A circuit needs connected electrical paths.",
                    "A complete circuit provides a path between the source terminals.",
                    "Components must be electrically connected, not merely physically close.",
                    "An open path prevents normal current flow."
                },
                "A group of components is not automatically a working " +
                "circuit. The components must form a usable electrical path.",
                "Use the low-voltage Project Spark environment while " +
                "learning circuit fundamentals. Never experiment with mains electricity.");
        }

        private static void AddElectricalSource(
            SerializedProperty entries)
        {
            AddEntry(
                entries,
                "electrical_source",
                "Electrical Source",
                "Source",
                new[]
                {
                    "source",
                    "power source",
                    "voltage source",
                    "battery",
                    "power supply"
                },
                "An electrical source provides a potential difference " +
                "that can drive current through a suitable electrical circuit.",
                "A source establishes an electrical potential difference " +
                "between its terminals. When a suitable closed path is " +
                "connected, this potential difference can drive current " +
                "through the circuit.",
                "A battery or laboratory DC power supply can provide " +
                "voltage to a circuit.",
                new[]
                {
                    "A source provides electrical potential difference.",
                    "A source has electrical terminals.",
                    "Voltage can exist without current flowing.",
                    "Current requires a suitable electrical path.",
                    "A source does not automatically cause current when its circuit is open."
                },
                "Having voltage at a source does not mean that current " +
                "is automatically flowing. Current depends on the connected circuit.",
                "Use low-voltage training sources in Project Spark. " +
                "Never connect Project Spark circuits to mains electricity.");
        }

        private static void AddCircuitPath(
            SerializedProperty entries)
        {
            AddEntry(
                entries,
                "circuit_path",
                "Circuit Path",
                "Circuit",
                new[]
                {
                    "path",
                    "electrical path",
                    "current path",
                    "circuit route"
                },
                "A circuit path is the connected electrical route " +
                "between the terminals of a source through the circuit components.",
                "For current to flow in a basic circuit, there must be " +
                "a continuous electrical path from one source terminal " +
                "through the connected components and back to the other source terminal.",
                "A wire from a battery positive terminal to an LED, " +
                "followed by a return connection from the LED to the " +
                "battery negative terminal, creates a complete path.",
                new[]
                {
                    "Current requires a suitable electrical path.",
                    "A path can contain multiple components.",
                    "A broken connection creates an open circuit.",
                    "The complete path normally connects both source terminals through the circuit."
                },
                "Connecting only one side of a component to a source " +
                "does not create a complete circuit.",
                "Build and test circuit paths using Project Spark's " +
                "low-voltage training environment.");
        }

        private static void AddPolarity(
            SerializedProperty entries)
        {
            AddEntry(
                entries,
                "polarity",
                "Polarity",
                "Fundamentals",
                new[]
                {
                    "positive",
                    "negative",
                    "positive terminal",
                    "negative terminal",
                    "anode",
                    "cathode"
                },
                "Polarity identifies the positive and negative sides " +
                "of an electrical source or polarized component.",
                "Polarity tells us which terminals have positive and " +
                "negative electrical relationships. Polarized components " +
                "such as LEDs and diodes can require their terminals to " +
                "be connected with the correct polarity.",
                "An LED has an anode and cathode. Connecting the source " +
                "with the appropriate polarity allows the LED to operate " +
                "when the rest of the circuit is complete.",
                new[]
                {
                    "Positive and negative terminals have different polarity.",
                    "LEDs are polarized components.",
                    "An LED has an anode and a cathode.",
                    "Polarity can determine whether a polarized component operates.",
                    "Polarity does not by itself guarantee current flow."
                },
                "Positive and negative terminals are not interchangeable " +
                "on polarized components.",
                "Use low-voltage Project Spark circuits when learning polarity.");
        }

        private static void AddOpenCircuit(
            SerializedProperty entries)
        {
            AddEntry(
                entries,
                "open_circuit",
                "Open Circuit",
                "Circuit",
                new[]
                {
                    "broken circuit",
                    "disconnected circuit",
                    "broken path",
                    "interrupted circuit"
                },
                "An open circuit is a circuit with a broken or interrupted " +
                "electrical path that prevents normal current flow through that path.",
                "An open switch, disconnected wire, or missing connection " +
                "can interrupt the circuit. Voltage may still exist across " +
                "the open portion even though current through that path is effectively zero.",
                "Opening a switch in a simple battery and lamp circuit " +
                "interrupts the path and stops normal current flow.",
                new[]
                {
                    "An open circuit interrupts the electrical path.",
                    "An ideal open circuit has zero current.",
                    "Voltage can exist across an open circuit.",
                    "An open switch can create an open circuit.",
                    "A disconnected wire can create an open circuit."
                },
                "An open circuit does not necessarily mean that there " +
                "is no voltage anywhere in the circuit.",
                "Use Project Spark's low-voltage environment when " +
                "experimenting with open and closed circuits.");
        }

        private static void AddClosedCircuit(
            SerializedProperty entries)
        {
            AddEntry(
                entries,
                "closed_circuit",
                "Closed Circuit",
                "Circuit",
                new[]
                {
                    "complete circuit",
                    "completed circuit",
                    "closed path",
                    "complete path"
                },
                "A closed circuit has a continuous electrical path that " +
                "allows current to flow when a source and suitable components are present.",
                "A closed circuit provides a complete path between the " +
                "source terminals through the connected components. Whether " +
                "significant current flows depends on the source and the " +
                "electrical properties of the circuit.",
                "Closing a switch in a battery and lamp circuit completes " +
                "the path and allows current to flow through the lamp.",
                new[]
                {
                    "A closed path can allow current flow.",
                    "A closed switch normally provides a conducting path.",
                    "A closed circuit still needs a source to produce current.",
                    "Circuit resistance affects current.",
                    "A closed circuit does not mean unlimited current."
                },
                "A closed circuit does not automatically mean that a large " +
                "current is flowing. The source and circuit resistance determine the current.",
                "Use low-voltage Project Spark circuits when learning about closed circuits.");
        }

        private static void AddShortCircuit(
            SerializedProperty entries)
        {
            AddEntry(
                entries,
                "short_circuit",
                "Short Circuit",
                "Safety",
                new[]
                {
                    "short",
                    "electrical short",
                    "shorted circuit"
                },
                "A short circuit is an unintended low-resistance path " +
                "that can allow excessive current to flow.",
                "A short circuit can bypass a component or load with a " +
                "very low-resistance path. The resulting current can become " +
                "much larger than intended and may damage components or the power source.",
                "Connecting the positive and negative terminals of a source " +
                "through a very low-resistance path can create a short circuit.",
                new[]
                {
                    "A short circuit has very low resistance.",
                    "A short circuit can produce excessive current.",
                    "A short can bypass intended components.",
                    "Short circuits can damage electrical components.",
                    "Current depends on voltage and circuit resistance."
                },
                "A short circuit is not simply a circuit that works very " +
                "well. It is an unintended low-resistance path that can cause excessive current.",
                "Never intentionally create short circuits with real batteries, " +
                "power supplies, or mains electricity. Use Project Spark's simulated environment.");
        }

        private static void AddWire(
            SerializedProperty entries)
        {
            AddEntry(
                entries,
                "wire",
                "Wire",
                "Component",
                new[]
                {
                    "cable",
                    "conductor",
                    "connection wire",
                    "electrical wire"
                },
                "A wire is a conductive connection used to electrically " +
                "connect terminals and components in a circuit.",
                "A wire provides a relatively low-resistance path between " +
                "connected electrical terminals. Wires allow components to " +
                "be connected together to form circuit paths.",
                "A wire can connect a power supply terminal to a component " +
                "terminal and provide the electrical path needed by the circuit.",
                new[]
                {
                    "A wire provides an electrical connection.",
                    "An ideal wire has approximately zero resistance.",
                    "Real wires have some resistance.",
                    "Wires can connect components in series or parallel arrangements.",
                    "A disconnected wire can break a circuit."
                },
                "A wire only creates an electrical connection when its " +
                "conductive ends are actually connected to the intended terminals.",
                "Use the simulated wiring system in Project Spark while learning electrical connections.");
        }

        private static void AddLED(
            SerializedProperty entries)
        {
            AddEntry(
                entries,
                "led",
                "LED",
                "Component",
                new[]
                {
                    "light emitting diode",
                    "light-emitting diode",
                    "diode",
                    "indicator LED",
                    "light"
                },
                "An LED, or light-emitting diode, is a semiconductor diode " +
                "that emits light when current flows through it in the appropriate direction.",
                "An LED is a polarized semiconductor device. Its anode and " +
                "cathode must be connected with appropriate polarity, and the " +
                "circuit must provide suitable current for normal operation.",
                "A low-voltage source can drive an LED through a suitable " +
                "current-limiting resistor when the LED polarity and circuit " +
                "connections are correct.",
                new[]
                {
                    "An LED is a diode.",
                    "An LED has polarity.",
                    "An LED has an anode and cathode.",
                    "Forward operation requires appropriate polarity.",
                    "Current must flow through the LED for normal light emission.",
                    "An LED normally requires current limiting.",
                    "Excessive current can damage an LED.",
                    "Voltage alone does not guarantee that an LED will light."
                },
                "An LED does not light simply because voltage is present. " +
                "It needs appropriate polarity, a complete current path, and suitable current.",
                "Use low-voltage Project Spark circuits when learning about LEDs. " +
                "Do not connect an LED directly to an uncontrolled voltage source " +
                "without appropriate current limiting.");
        }

        // ============================================================
        // GENERIC ENTRY CREATION
        // ============================================================

        private static void AddEntry(
            SerializedProperty entries,
            string id,
            string title,
            string category,
            string[] aliases,
            string definition,
            string explanation,
            string example,
            string[] importantFacts,
            string misconception,
            string safety)
        {
            int index =
                entries.arraySize;

            entries.InsertArrayElementAtIndex(index);

            SerializedProperty entry =
                entries.GetArrayElementAtIndex(index);

            SetString(
                entry,
                "id",
                id);

            SetString(
                entry,
                "title",
                title);

            SetString(
                entry,
                "category",
                category);

            SetStringList(
                entry,
                "aliases",
                aliases);

            SetString(
                entry,
                "definition",
                definition);

            SetString(
                entry,
                "explanation",
                explanation);

            SetString(
                entry,
                "example",
                example);

            SetStringList(
                entry,
                "importantFacts",
                importantFacts);

            SetString(
                entry,
                "misconception",
                misconception);

            SetString(
                entry,
                "safety",
                safety);
        }

        // ============================================================
        // SERIALIZED HELPERS
        // ============================================================

        private static void SetString(
            SerializedProperty parent,
            string propertyName,
            string value)
        {
            SerializedProperty property =
                parent.FindPropertyRelative(propertyName);

            if (property != null)
                property.stringValue = value;
        }

        private static void SetStringList(
            SerializedProperty parent,
            string propertyName,
            string[] values)
        {
            SerializedProperty property =
                parent.FindPropertyRelative(propertyName);

            if (property == null)
                return;

            property.ClearArray();

            if (values == null)
                return;

            for (int i = 0; i < values.Length; i++)
            {
                property.InsertArrayElementAtIndex(i);

                SerializedProperty element =
                    property.GetArrayElementAtIndex(i);

                element.stringValue =
                    values[i];
            }
        }

        // ============================================================
        // FOLDER
        // ============================================================

        private static void EnsureFolderExists()
        {
            string[] folders =
            {
                "Assets/My_Assets",
                "Assets/My_Assets/_Project_Spark",
                "Assets/My_Assets/_Project_Spark/Data",
                AssetFolder
            };

            for (int i = 0; i < folders.Length; i++)
            {
                string folder =
                    folders[i];

                if (AssetDatabase.IsValidFolder(folder))
                    continue;

                int slash =
                    folder.LastIndexOf('/');

                string parent =
                    folder.Substring(0, slash);

                string name =
                    folder.Substring(slash + 1);

                AssetDatabase.CreateFolder(
                    parent,
                    name);
            }
        }
    }
}

#endif