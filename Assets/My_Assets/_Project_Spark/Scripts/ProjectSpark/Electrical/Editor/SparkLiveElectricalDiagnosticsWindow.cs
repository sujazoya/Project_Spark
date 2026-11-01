#if UNITY_EDITOR
using System;
using System.IO;
using ProjectSpark.Electrical;
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.UI.VisualTeaching.Editor
{
    /// <summary>
    /// Read-only live testing console. Installs only the observer, not new circuit systems.
    /// </summary>
    public sealed class SparkLiveElectricalDiagnosticsWindow : EditorWindow
    {
        private SparkLiveElectricalDiagnostics observer;
        private Vector2 scroll;

        [MenuItem("Tools/Project Spark/Electrical/Live Integration Diagnostics")]
        private static void Open()
        {
            var window = GetWindow<SparkLiveElectricalDiagnosticsWindow>("Spark Electrical Test");
            window.minSize = new Vector2(560, 410);
            window.Resolve();
            window.Show();
        }

        [MenuItem("Tools/Project Spark/Electrical/Attach Read-Only Diagnostics")]
        private static void Attach()
        {
            var solver = Selection.activeGameObject != null
                ? Selection.activeGameObject.GetComponent<SparkElectricalSolver>() : null;
            if (solver == null)
                solver = UnityEngine.Object.FindFirstObjectByType<SparkElectricalSolver>(FindObjectsInactive.Include);
            if (solver == null)
            {
                EditorUtility.DisplayDialog("Spark Diagnostics", "Open the shared gameplay scene containing SparkElectricalSolver.", "OK");
                return;
            }
            var observer = solver.GetComponent<SparkLiveElectricalDiagnostics>();
            if (observer == null)
                observer = Undo.AddComponent<SparkLiveElectricalDiagnostics>(solver.gameObject);
            var serialized = new SerializedObject(observer);
            AssignIfEmpty(serialized, "solver", solver);
            AssignIfEmpty(serialized, "loader", UnityEngine.Object.FindFirstObjectByType<ProjectSpark.Gameplay.SparkLevelLayoutLoader>(FindObjectsInactive.Include));
            AssignIfEmpty(serialized, "bridge", UnityEngine.Object.FindFirstObjectByType<SparkSolverLiveMeasurementBridge>(FindObjectsInactive.Include));
            AssignIfEmpty(serialized, "publisher", UnityEngine.Object.FindFirstObjectByType<SparkVerifiedElectricalPublisher>(FindObjectsInactive.Include));
            AssignIfEmpty(serialized, "nodeFeed", UnityEngine.Object.FindFirstObjectByType<SparkMeterVerifiedNodeFeed>(FindObjectsInactive.Include));
            AssignIfEmpty(serialized, "virtualMeter", UnityEngine.Object.FindFirstObjectByType<SparkVirtualMultimeter>(FindObjectsInactive.Include));
            serialized.ApplyModifiedProperties();
            Selection.activeObject = observer;
            EditorGUIUtility.PingObject(observer);
            var window = GetWindow<SparkLiveElectricalDiagnosticsWindow>("Spark Electrical Test");
            window.observer = observer;
            window.Show();
            Debug.Log("[Project Spark] Read-only electrical diagnostics attached. Save the scene and test in Play Mode.", observer);
        }

        private static void AssignIfEmpty(SerializedObject serialized, string field, UnityEngine.Object value)
        {
            var property = serialized.FindProperty(field);
            if (property != null && property.objectReferenceValue == null && value != null)
                property.objectReferenceValue = value;
        }

        private void OnEnable() => EditorApplication.playModeStateChanged += OnPlayModeChanged;
        private void OnDisable() => EditorApplication.playModeStateChanged -= OnPlayModeChanged;
        private void OnPlayModeChanged(PlayModeStateChange _) => observer = null;
        private void OnInspectorUpdate() => Repaint();

        private void Resolve()
        {
            if (observer == null)
                observer = UnityEngine.Object.FindFirstObjectByType<SparkLiveElectricalDiagnostics>(FindObjectsInactive.Include);
        }

        private void OnGUI()
        {
            Resolve();
            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("PROJECT SPARK / LIVE ELECTRICAL DIAGNOSTICS", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Read-only: does not solve circuits, publish measurements, change mappings or move probes. " +
                "Start a level, change a component, then switch levels to test the signal pipeline.", MessageType.Info);
            if (observer == null)
            {
                EditorGUILayout.HelpBox("No SparkLiveElectricalDiagnostics is attached.", MessageType.Warning);
                if (GUILayout.Button("Attach Read-Only Diagnostics")) Attach();
                return;
            }
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.ObjectField("Observer", observer, typeof(SparkLiveElectricalDiagnostics), true);
                if (GUILayout.Button("Select", GUILayout.Width(65))) Selection.activeObject = observer;
            }
            EditorGUILayout.LabelField("Mode", EditorApplication.isPlaying ? "PLAY MODE / LIVE" : "EDIT MODE / REFERENCES ONLY");
            EditorGUILayout.LabelField("Level", observer.ActiveLevelName);
            EditorGUILayout.LabelField("Circuit ID", observer.LatestCircuitId);
            EditorGUILayout.LabelField("Revision", observer.LatestRevision.ToString());
            EditorGUILayout.LabelField("Solver completed / failed", observer.SuccessfulSolves + " / " + observer.FailedSolves);
            EditorGUILayout.LabelField("Component / node frames", observer.ComponentFramesObserved + " / " + observer.NodeFramesObserved);
            EditorGUILayout.LabelField("Recorded issues", observer.ViolationsObserved.ToString());
            EditorGUILayout.Space(6);
            scroll = EditorGUILayout.BeginScrollView(scroll);
            foreach (var check in observer.GetChecks())
            {
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    EditorGUILayout.LabelField(check.state.ToString().ToUpperInvariant() + " / " + check.name, EditorStyles.boldLabel);
                    EditorGUILayout.LabelField(check.detail, EditorStyles.wordWrappedLabel);
                }
            }
            EditorGUILayout.EndScrollView();
            EditorGUILayout.Space(8);
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Export Markdown Report", GUILayout.Height(30))) ExportReport();
                if (GUILayout.Button("Clear Observations", GUILayout.Height(30)))
                    observer.ResetObservationHistory();
            }
        }

        private void ExportReport()
        {
            string path = EditorUtility.SaveFilePanel("Save Spark Electrical Diagnostic Report", "",
                "Spark_Live_Electrical_Diagnostics_" + DateTime.Now.ToString("yyyyMMdd_HHmmss"), "md");
            if (string.IsNullOrEmpty(path)) return;
            try
            {
                File.WriteAllText(path, observer.GetReportMarkdown());
                Debug.Log("[Project Spark] Electrical integration report saved: " + path, observer);
                EditorUtility.RevealInFinder(path);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                EditorUtility.DisplayDialog("Save Failed", exception.Message, "OK");
            }
        }
    }
}
#endif
