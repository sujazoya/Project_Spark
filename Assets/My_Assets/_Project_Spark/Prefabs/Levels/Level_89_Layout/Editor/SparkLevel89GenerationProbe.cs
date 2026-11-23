using System;
using System.IO;
using System.Linq;
using ProjectSpark.Gameplay;
using ProjectSpark.Circuit;
using ProjectSpark.Electrical;
using UnityEditor.SceneManagement;
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.EditorTools
{
    internal static class SparkLevel89GenerationProbe
    {

        [MenuItem("Tools/Project Spark/Levels/89/Verify Layout and Wiring")]
        private static void Verify()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode) return;
            try
            {
                SparkLevel89Builder.BuildLayoutOnly();
                const string path = "Assets/My_Assets/_Project_Spark/Prefabs/Levels/Level_89_Layout/Level_89_Layout.prefab";
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                var process = prefab.GetComponent<SparkLevel89Process>();
                var art = process.arena.Find("L89_HD_VISUAL_ART");
                var modelChildren = art.Cast<Transform>().Count(t => t.GetComponentsInChildren<MeshRenderer>(true).Length > 0);
                var transforms = art.GetComponentsInChildren<Transform>(true);
                var sockets = transforms.Count(t => t.name.StartsWith("SOCKET_P", StringComparison.Ordinal));
                var pivots = transforms.Count(t => t.name.StartsWith("ANIM_", StringComparison.Ordinal));
                if (modelChildren != 92 || process.pins.Length != 340 || sockets != 248 || pivots != 76 ||
                    process.pins.Distinct().Count() != 340 || process.GetComponent<SparkLevelLayout>() == null)
                    throw new InvalidOperationException($"Children {string.Join(", ", art.Cast<Transform>().Take(5).Select(t => t.name))}. Unexpected assets: {modelChildren} models, {process.pins.Length} pins, {sockets} sockets, {pivots} pivots.");
                if (!art.Find("L89_HeliosRadiationDefenseArenaBase") || !process.source || !process.primary ||
                    !process.secondary || !process.ground || !process.visuals || process.labels.Length != 340)
                    throw new InvalidOperationException("Missing serialized Level 89 layout references.");
                CheckWiring(prefab);
                var plant = new SparkLevel89Plant();
                plant.RequestSeal();
                for (int step = 0; step < 120; step++) plant.Tick(.05f, true, true, -1f, 0, false);
                if (!plant.Ready) throw new InvalidOperationException("Plant failed to commission.");
                float dose = 0;
                for (int step = 1; step <= 480; step++)
                {
                    plant.Tick(.05f, true, true, step * .05f, 0, false);
                    dose += plant.DoseRate * .05f;
                    if (!plant.Ready) throw new InvalidOperationException("Essential service or protection lost during nominal storm.");
                }
                if (dose <= 0f || dose > SparkLevel89Plant.DoseBudget) throw new InvalidOperationException($"Invalid nominal dose {dose}.");
                plant.Tick(.05f, true, true, -1f, 1, false);
                if (plant.DetectorsInRange || plant.DoseRate <= 0f) throw new InvalidOperationException("Saturation invalidated physical dose.");
                plant.Tick(.05f, true, true, -1f, 3, false);
                if (plant.HighEnergy <= 1.6f || plant.DoseRate <= .22f) throw new InvalidOperationException("Surge was unobservable.");
                plant.BeginJam();
                plant.Tick(.05f, true, true, -1f, 2, false);
                if (plant.Ready || plant.Positions[2] >= .8f) throw new InvalidOperationException("Jam did not remove protection.");
                for (int step = 0; step < 120; step++) plant.Tick(.05f, true, true, -1f, 0, true);
                if (!plant.Ready) throw new InvalidOperationException("Jam repair did not restore readiness.");
                string report = $"PASS: {modelChildren} modular models, {sockets} sockets, {pivots} pivots, {process.pins.Length} active contacts. All 170 actual graph edges and W149 loss/recovery verified. Commissioning, nominal storm dose {dose:F3}, three disturbance effects and jam recovery verified in Edit Mode.";
                File.WriteAllText("Logs/SparkLevel89Probe.txt", report);
                Debug.Log(report);
            }
            catch (Exception exception)
            {
                File.WriteAllText("Logs/SparkLevel89Probe.txt", exception.ToString());
                Debug.LogException(exception);
            }
        }
        private static void CheckWiring(GameObject prefab)
        {
            var sceneSolver = UnityEngine.Object.FindFirstObjectByType<SparkElectricalSolver>(FindObjectsInactive.Include);
            var registry = new SerializedObject(sceneSolver).FindProperty("solverRegistry").objectReferenceValue;
            var scene = EditorSceneManager.NewPreviewScene();
            try
            {
                var root = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
                var process = root.GetComponent<SparkLevel89Process>();
                var circuit = root.AddComponent<SparkCircuitSystem>();
                var solver = root.AddComponent<SparkElectricalSolver>();
                var serialized = new SerializedObject(solver);
                serialized.FindProperty("circuit").objectReferenceValue = circuit;
                serialized.FindProperty("solverRegistry").objectReferenceValue = registry;
                serialized.FindProperty("componentSearchRoot").objectReferenceValue = root.transform;
                serialized.ApplyModifiedPropertiesWithoutUndo();
                SparkCircuitConnection critical = default;
                for (int wire = 0; wire < 170; wire++)
                {
                    if (!circuit.TryCreateConnection(process.pins[wire * 2], process.pins[wire * 2 + 1],
                        SparkConnectionKind.Wire, SparkConnectionDirection.OutputToInput, out var edge))
                        throw new InvalidOperationException($"The circuit rejected W{wire + 1:000}.");
                    if (wire == 148) critical = edge;
                }

                process.source.SetOutput(true, default);
                process.primary.SetOutput(true, default);
                process.secondary.SetOutput(true, default);
                float Read(int wire)
                {
                    solver.MarkTopologyDirty();
                    solver.SolveNow();
                    var result = solver.LatestSolveResult;
                    return result != null && result.TryGetVoltage(process.pins[wire * 2 + 1], process.ground, out float voltage) ? voltage : 0f;
                }

                if (Read(0) < 4.6f || Read(148) < .6f || Read(131) < .6f)
                    throw new InvalidOperationException("Expected solved control and dose rails were missing.");
                if (!circuit.TryRemoveConnection(critical.Id, out _) || Read(148) >= .6f || Read(131) < .6f)
                    throw new InvalidOperationException("W149 removal did not isolate trusted feedback while preserving independent feedback.");
                if (!circuit.TryCreateConnection(critical.A, critical.B, critical.Kind, critical.Direction, out _) || Read(148) < .6f)
                    throw new InvalidOperationException("W149 reconnect did not restore the solved receiver voltage.");
            }
            finally { EditorSceneManager.ClosePreviewScene(scene); }
        }
    }
}




