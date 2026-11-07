using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using ProjectSpark.Electrical;

namespace ProjectSpark.Gameplay
{
    /// <summary>
    /// Read-only live voltage dashboard for the currently loaded Project Spark level.
    /// Samples every completed electrical solve and periodically checks for changes.
    /// Never calls SolveNow(), MarkDirty(), or changes any electrical state.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkLevelVoltageMonitor : MonoBehaviour
    {
        [Header("References (auto-resolved if empty)")]
        [SerializeField] private SparkLevelLayoutLoader layoutLoader;
        [SerializeField] private SparkElectricalSolver electricalSolver;

        [Header("UI")]
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text summaryText;
        [SerializeField] private TMP_Text readingsText;
        [SerializeField] private TMP_Text historyText;
        [SerializeField] private TMP_Text pauseButtonText;

        [Header("Monitoring")]
        [Tooltip("Absolute voltage change required to record an event, in volts.")]
        [SerializeField, Min(0.000001f)] private float changeThresholdVolts = 0.001f;
        [Tooltip("UI refresh rate. Sampling also occurs on every solver completion.")]
        [SerializeField, Range(1f, 30f)] private float displayRefreshHz = 10f;
        [SerializeField, Min(10)] private int maxHistoryEntries = 80;
        [SerializeField] private bool includeComponentVoltages = true;
        [SerializeField] private bool includeInactiveObjects = false;

        private sealed class Reading
        {
            public SparkTerminal terminal;
            public SparkElectricalComponent component;
            public string label;
            public float voltage;
            public float previousVoltage;
            public bool hasValue;
            public bool active;
            public bool changed;
        }

        private readonly List<Reading> readings = new List<Reading>(64);
        private readonly Queue<string> history = new Queue<string>(80);
        private readonly StringBuilder builder = new StringBuilder(4096);
        private SparkLevelLayout observedLayout;
        private SparkElectricalSolver subscribedSolver;
        private bool paused;
        private bool needsRepaint = true;
        private float nextUiTime;
        private float nextFallbackSampleTime;
        private int totalChanges;

        private void OnEnable()
        {
            ResolveReferences();
            RebindSolverEvent();
            SyncLevel();
            needsRepaint = true;
        }

        private void OnDisable()
        {
            if (subscribedSolver != null)
                subscribedSolver.SolveCompleted -= OnSolveCompleted;
            subscribedSolver = null;
        }

        private void Update()
        {
            if (layoutLoader == null || electricalSolver == null)
            {
                ResolveReferences();
                RebindSolverEvent();
            }

            SyncLevel();

            // Fallback for a scene that updates electrical readings without
            // publishing SolveCompleted. Never requests an electrical solve.
            if (!paused && Time.unscaledTime >= nextFallbackSampleTime)
            {
                nextFallbackSampleTime = Time.unscaledTime + 0.1f;
                Sample();
            }

            if (needsRepaint && Time.unscaledTime >= nextUiTime)
            {
                nextUiTime = Time.unscaledTime +
                    (1f / Mathf.Max(1f, displayRefreshHz));
                Render();
                needsRepaint = false;
            }
        }

        private void ResolveReferences()
        {
            if (layoutLoader == null)
                layoutLoader = FindFirstObjectByType<SparkLevelLayoutLoader>();
            if (electricalSolver == null)
                electricalSolver = FindFirstObjectByType<SparkElectricalSolver>();
        }

        private void RebindSolverEvent()
        {
            if (subscribedSolver == electricalSolver)
                return;

            if (subscribedSolver != null)
                subscribedSolver.SolveCompleted -= OnSolveCompleted;

            subscribedSolver = electricalSolver;
            if (subscribedSolver != null)
                subscribedSolver.SolveCompleted += OnSolveCompleted;
        }

        private void OnSolveCompleted()
        {
            // The solver publishes this after terminal states are applied.
            if (!paused)
            {
                SyncLevel();
                Sample();
            }
        }

        private void SyncLevel()
        {
            SparkLevelLayout active = layoutLoader != null
                ? layoutLoader.ActiveLayout
                : null;

            if (active == observedLayout)
                return;

            observedLayout = active;
            readings.Clear();
            history.Clear();
            totalChanges = 0;
            needsRepaint = true;

            if (active == null)
                return;

            // Only objects inside the active layout are monitored.
            SparkTerminal[] terminals =
                active.GetComponentsInChildren<SparkTerminal>(true);

            for (int i = 0; i < terminals.Length; i++)
            {
                SparkTerminal terminal = terminals[i];
                if (terminal == null)
                    continue;

                readings.Add(new Reading
                {
                    terminal = terminal,
                    label = "T  " + RelativePath(active.transform, terminal.transform)
                });
            }

            if (includeComponentVoltages)
            {
                SparkElectricalComponent[] components =
                    active.GetComponentsInChildren<SparkElectricalComponent>(true);

                for (int i = 0; i < components.Length; i++)
                {
                    SparkElectricalComponent component = components[i];
                    if (component == null)
                        continue;

                    readings.Add(new Reading
                    {
                        component = component,
                        label = "C  " + RelativePath(active.transform, component.transform)
                    });
                }
            }

            readings.Sort((a, b) =>
                string.Compare(a.label, b.label, StringComparison.OrdinalIgnoreCase));
            Sample();
        }

        private static string RelativePath(Transform root, Transform item)
        {
            if (item == root)
                return item.name;

            var names = new List<string>(8);
            Transform cursor = item;
            while (cursor != null && cursor != root)
            {
                names.Add(cursor.name);
                cursor = cursor.parent;
            }
            names.Reverse();
            return string.Join("/", names);
        }

        private void Sample()
        {
            if (observedLayout == null)
                return;

            bool anyChange = false;

            for (int i = 0; i < readings.Count; i++)
            {
                Reading entry = readings[i];
                bool isTerminal = entry.terminal != null;
                bool exists = isTerminal || entry.component != null;

                if (!exists)
                    continue;

                bool isActive = isTerminal
                    ? entry.terminal.isActiveAndEnabled && entry.terminal.IsElectricalEnabled
                    : entry.component.isActiveAndEnabled && entry.component.ElectricalEnabled;

                if (!isActive && !includeInactiveObjects)
                {
                    if (entry.active)
                        anyChange = true;
                    entry.active = false;
                    entry.hasValue = false; // Do not compare stale readings on reactivation.
                    continue;
                }

                float voltage = isTerminal
                    ? entry.terminal.ElectricalState.Voltage
                    : entry.component.ElectricalState.Voltage;

                if (float.IsNaN(voltage) || float.IsInfinity(voltage))
                    continue;

                bool becameActive = !entry.active;
                entry.active = isActive;

                if (!entry.hasValue)
                {
                    entry.voltage = voltage;
                    entry.previousVoltage = voltage;
                    entry.hasValue = true;
                    anyChange = true;
                    continue;
                }

                float delta = voltage - entry.voltage;
                if (Mathf.Abs(delta) < changeThresholdVolts)
                {
                    if (becameActive)
                        anyChange = true;
                    continue;
                }

                entry.previousVoltage = entry.voltage;
                entry.voltage = voltage;
                entry.changed = true;
                totalChanges++;
                anyChange = true;

                string sign = delta >= 0f ? "+" : string.Empty;
                string message = string.Format(
                    "{0,6:F2}s  {1}  {2:F3} V  ({3}{4:F3} V)",
                    Time.unscaledTime, entry.label, voltage, sign, delta);

                history.Enqueue(message);
                while (history.Count > Mathf.Max(10, maxHistoryEntries))
                    history.Dequeue();
            }

            if (anyChange)
                needsRepaint = true;
        }

        private void Render()
        {
            string levelName = observedLayout != null &&
                               observedLayout.LevelDefinition != null
                ? observedLayout.LevelDefinition.DisplayName
                : "No level loaded";

            if (titleText != null)
                titleText.text = "VOLTAGE MONITOR  <color=#40D6FA>LIVE</color>";

            if (summaryText != null)
            {
                summaryText.text = string.Format(
                    "{0}\n{1} channels   |   {2} changes   |   {3}",
                    Escape(levelName), readings.Count, totalChanges,
                    paused ? "PAUSED" : "READ ONLY");
            }

            if (readingsText != null)
            {
                builder.Clear();
                if (observedLayout == null)
                {
                    builder.Append("Load a level to see terminal and component voltages.");
                }
                else
                {
                    for (int i = 0; i < readings.Count; i++)
                    {
                        Reading entry = readings[i];
                        if (!entry.active && !includeInactiveObjects)
                            continue;
                        if (!entry.hasValue)
                            continue;

                        string color = entry.changed ? "#78F6B4" : "#D6EBF6";
                        builder.Append("<color=").Append(color).Append(">")
                            .Append(Escape(entry.label))
                            .Append("</color>\n    <color=#58CBF2>")
                            .Append(entry.voltage.ToString("F4"))
                            .Append(" V</color>");

                        if (entry.changed)
                        {
                            float delta = entry.voltage - entry.previousVoltage;
                            builder.Append("   <color=#83E8A7>")
                                .Append(delta >= 0f ? "+" : string.Empty)
                                .Append(delta.ToString("F4"))
                                .Append(" V</color>");
                        }

                        builder.Append("\n\n");
                        entry.changed = false;
                    }
                    if (builder.Length == 0)
                        builder.Append("No active voltage channels found.");
                }
                readingsText.text = builder.ToString();
            }

            if (historyText != null)
            {
                builder.Clear();
                if (history.Count == 0)
                    builder.Append("No voltage changes detected yet.");
                else
                    foreach (string item in history)
                        builder.Append(Escape(item)).Append('\n');
                historyText.text = builder.ToString();
            }

            if (pauseButtonText != null)
                pauseButtonText.text = paused ? "RESUME" : "PAUSE";
        }

        private static string Escape(string value)
        {
            return string.IsNullOrEmpty(value) ? string.Empty :
                value.Replace("&", "&amp;").Replace("<", "&lt;")
                    .Replace(">", "&gt;");
        }

        public void TogglePause()
        {
            paused = !paused;
            needsRepaint = true;
            if (!paused)
                Sample();
        }

        public void ClearHistory()
        {
            history.Clear();
            totalChanges = 0;
            needsRepaint = true;
        }
    }
}
