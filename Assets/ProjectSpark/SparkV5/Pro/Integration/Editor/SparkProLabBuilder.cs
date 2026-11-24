using System;
using ProjectSpark.Circuit;
using ProjectSpark.Electrical;
using ProjectSpark.Gameplay;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine;

namespace ProjectSpark.SparkV5.Pro.Editor
{
    internal static class SparkProLabBuilder
    {
        private const string Prefab = "Assets/ProjectSpark/SparkV5/Pro/Resources/SparkProIsolatedLab.prefab";
        private const string Registry = "Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Electrical/Solvers/Solver Assets/SparkElectricalSolverRegistry.asset";

        internal static void Ensure()
        {
            if (!AssetDatabase.LoadAssetAtPath<SparkProLabCircuit>(Prefab)) Build();
        }

        [MenuItem("Project Spark/Spark V5 Pro/Cortex/Build Isolated Teaching Lab")]
        internal static void Build()
        {
            var registry = AssetDatabase.LoadAssetAtPath<SparkElectricalSolverRegistry>(Registry);
            if (!registry) throw new InvalidOperationException("The project solver registry is missing.");
            var active = SceneManager.GetActiveScene();
            var temporary = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            SceneManager.SetActiveScene(temporary);
            var root = new GameObject("Spark Pro Isolated Teaching Lab");
            try
            {
                root.SetActive(false);
                var lab = root.AddComponent<SparkProLabCircuit>();
                lab.circuit = root.AddComponent<SparkCircuitSystem>();
                lab.solver = root.AddComponent<SparkElectricalSolver>();
                Set(lab.circuit, ("reportInvalidConnectionsToAI", false));
                Set(lab.solver, ("circuit", lab.circuit), ("solverRegistry", registry), ("componentSearchRoot", root.transform),
                    ("simulateTransientDevices", false));
                var supply = Node<SparkPowerSupply>("5 V training supply");
                lab.supplyPositive = Pin(supply, "Supply +");
                lab.supplyReturn = Pin(supply, "Supply return");
                Set(supply, ("positiveTerminal", lab.supplyPositive), ("negativeTerminal", lab.supplyReturn),
                    ("outputVoltage", 5f), ("currentLimit", .1f), ("state", SparkPowerSupplyState.Active));
                lab.control = Node<SparkSwitch>("Series switch");
                lab.switchIn = Pin(lab.control, "Switch in");
                lab.switchOut = Pin(lab.control, "Switch out");
                Set(lab.control, ("inputTerminal", lab.switchIn), ("outputTerminal", lab.switchOut));
                lab.load = Node<SparkResistor>("1 kΩ test load");
                lab.resistorIn = Pin(lab.load, "Load in");
                lab.resistorOut = Pin(lab.load, "Load return");
                Set(lab.load, ("terminalA", lab.resistorIn), ("terminalB", lab.resistorOut), ("resistance", 1000f));
                root.SetActive(true);
                if (!PrefabUtility.SaveAsPrefabAsset(root, Prefab))
                    throw new InvalidOperationException("Could not persist the isolated teaching lab.");

                T Node<T>(string name) where T : Component
                {
                    var go = new GameObject(name);
                    go.transform.SetParent(root.transform, false);
                    return go.AddComponent<T>();
                }
            }
            finally
            {
                SceneManager.SetActiveScene(active);
                EditorSceneManager.CloseScene(temporary, true);
            }
        }

        private static SparkTerminal Pin(SparkElectronicObject owner, string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(owner.transform, false);
            var pin = go.AddComponent<SparkTerminal>();
            Set(pin, ("owner", owner), ("acceptsInput", true), ("providesOutput", true), ("maxConnections", 2));
            return pin;
        }

        private static void Set(UnityEngine.Object target, params (string name, object value)[] fields)
        {
            var serialized = new SerializedObject(target);
            foreach (var (name, value) in fields)
            {
                var field = serialized.FindProperty(name) ?? throw new InvalidOperationException(name);
                switch (value)
                {
                    case UnityEngine.Object reference: field.objectReferenceValue = reference; break;
                    case bool flag: field.boolValue = flag; break;
                    case float number: field.floatValue = number; break;
                    case int number: field.intValue = number; break;
                    case Enum mode: field.enumValueIndex = Convert.ToInt32(mode); break;
                    default: throw new InvalidOperationException(name);
                }
            }
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
