using System;
using System.Collections.Generic;
using ProjectSpark.Circuit;
using ProjectSpark.Electrical;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.Gameplay
{
    [DisallowMultipleComponent]
    public sealed class SparkLevel38OnePress : MonoBehaviour
    {
        public static readonly string[] Gates =
        {
            "L38_Wired", "L38_RawBounce", "L38_GlitchRejected",
            "L38_OnePress", "L38_Released", "L38_TwoParts"
        };

        private const float RawThreshold = 2.5f;
        private const float PressThreshold = 3f;
        private const float RearmThreshold = 2f;
        private const float RestVoltage = .12f;
        private const int SampleCount = 129;

        private static readonly Pulse[] RawTest =
        {
            new(true,.08f), new(false,.08f), new(true,.08f), new(false,.08f),
            new(true,1.20f), new(false,4f)
        };
        private static readonly Pulse[] GlitchTest = { new(true,.10f), new(false,2.6f) };
        private static readonly Pulse[] PressTest =
        {
            new(true,.08f), new(false,.08f), new(true,.08f), new(false,.08f), new(true,1.20f)
        };
        private static readonly Pulse[] ReleaseTest =
        {
            new(false,.08f), new(true,.08f), new(false,.08f), new(true,.08f), new(false,1.45f)
        };
        private static readonly Pulse[] CycleTest =
        {
            new(true,.08f), new(false,.08f), new(true,.08f), new(false,.08f), new(true,1.20f),
            new(false,.08f), new(true,.08f), new(false,.08f), new(true,.08f), new(false,1.45f)
        };

        private readonly struct Pulse
        {
            public readonly bool Closed;
            public readonly float Seconds;
            public Pulse(bool closed, float seconds)
            {
                Closed = closed;
                Seconds = seconds;
            }
        }

        public SparkLevelLayout layout;
        public SparkPowerSupply source;
        public SparkSwitch contact;
        public SparkResistor feed, pullDown, filter;
        public SparkCapacitor capacitor;
        public Text statusText, meterText;
        public Button rawTestButton, filteredModeButton, glitchButton, pressButton, releaseButton, cycleButton, restartButton;
        public RectTransform rawTrace, filteredTrace;
        public GameObject countIndicator;

        private SparkLevelLayoutLoader loader;
        private LevelGamePlayManager manager;
        private SparkElectricalSolver solver;
        private SparkCircuitSystem circuit;
        private SparkLevelSceneBindings bindings;
        private SparkElectricalNetworkGraph checkedGraph;
        private string wiringIssue, feedback;
        private int topology = -1;
        private int stage, pulseIndex, rawEdges, filteredEdges, partCount;
        private bool ready, filteredMode, rawHigh, filteredHigh, failed;
        private float accumulator, pulseElapsed, sampleElapsed, rawVolts, filteredVolts, filteredPeak;
        private Pulse[] sequence;
        private readonly List<Vector2> samples = new List<Vector2>(SampleCount);
        private RectTransform[] rawLines, filteredLines;

        public void RunRawTest()
        {
            if (CanStart(1) && !filteredMode) Begin(RawTest);
        }

        public void SelectFiltered()
        {
            if (!CanStart(2)) return;
            filteredMode = true;
            partCount = 0;
        }

        public void RunGlitch()
        {
            if (CanStart(2) && filteredMode) Begin(GlitchTest);
        }

        public void RunPress()
        {
            if (CanStart(3)) Begin(PressTest);
        }

        public void RunRelease()
        {
            if (CanStart(4)) Begin(ReleaseTest);
        }

        public void RunCycle()
        {
            if (CanStart(5)) Begin(CycleTest);
        }

        public void Restart()
        {
            if (ready && sequence == null && topology == circuit.TopologyVersion) ResetTrial();
        }

        private bool CanStart(int expectedStage) =>
            ready && !failed && sequence == null && stage == expectedStage &&
            loader.ActiveLayout == layout && topology == circuit.TopologyVersion &&
            (stage > 2 || filteredVolts <= RestVoltage && !rawHigh);

        private void Begin(Pulse[] waveform)
        {
            sequence = waveform;
            accumulator = pulseElapsed = sampleElapsed = 0f;
            pulseIndex = rawEdges = filteredEdges = 0;
            filteredPeak = filteredVolts;
            feedback = null;
            samples.Clear();
            PlotSample();
        }

        private void Update()
        {
            if (loader == null) loader = FindFirstObjectByType<SparkLevelLayoutLoader>();
            if (layout == null || loader == null || loader.ActiveLayout != layout) return;
            if (manager == null) manager = FindFirstObjectByType<LevelGamePlayManager>();
            if (solver == null) solver = FindFirstObjectByType<SparkElectricalSolver>();
            if (circuit == null) circuit = FindFirstObjectByType<SparkCircuitSystem>();
            if (solver == null || circuit == null)
            {
                Show("Waiting for the circuit and solver.");
                return;
            }
            if (bindings != loader.ActiveBindings || topology != circuit.TopologyVersion)
            {
                ResetTrial();
                bindings = loader.ActiveBindings;
                topology = circuit.TopologyVersion;
                checkedGraph = null;
            }
            if (bindings == null || Array.Exists(Gates, gate => !bindings.IsComponentIdDeclared(gate)))
            {
                Invalidate("Check the six Level 38 objective bindings.");
                return;
            }
            if (!Available(source) || !Available(contact) || !Available(feed) || !Available(pullDown) ||
                !Available(filter) || !Available(capacitor) || feed.IsBurned || pullDown.IsBurned || filter.IsBurned)
            {
                Invalidate("Check that all six bench components are active and healthy.");
                return;
            }
            if (solver.IsSolving) return;
            if ((solver.IsDirty || solver.LatestSolveResult == null) && !solver.SolveNow())
            {
                Invalidate("The electrical solver could not read the circuit.");
                return;
            }
            var result = solver.LatestSolveResult;
            if (result == null || solver.IsDirty) return;
            if (!ReferenceEquals(result.Context.Graph, checkedGraph))
            {
                checkedGraph = result.Context.Graph;
                wiringIssue = CheckWiring(checkedGraph);
            }
            if (wiringIssue != null)
            {
                Invalidate(wiringIssue);
                return;
            }
            float step = result.Context.TimeStep;
            if (!result.Context.SimulateTransientDevices || !(step > 0f && step <= .05f))
            {
                Invalidate("Enable transient simulation with a solver step of 50 ms or less.");
                return;
            }
            if (!ReadVoltages(result))
            {
                Invalidate("Check the 5 V supply and the RAW and FILTERED nodes.");
                return;
            }
            ready = true;
            if (stage == 0 && !contact.IsClosed && rawVolts < .2f && filteredVolts <= RestVoltage)
            {
                Award();
                samples.Clear();
                PlotSample();
            }

            bool draining = sequence == null && stage <= 2 && filteredVolts > RestVoltage;
            bool returnToRest = sequence == null && stage <= 2 && contact.IsClosed;
            if (!failed && (sequence != null || draining || returnToRest))
            {
                accumulator = Mathf.Min(accumulator + Time.unscaledDeltaTime, .25f);
                for (int i = 0; i < 8 && accumulator >= step; i++)
                {
                    accumulator -= step;
                    if (!AdvanceSignal(step)) break;
                }
            }
            else accumulator = 0f;
            Show(feedback ?? Prompt());
        }

        private bool AdvanceSignal(float step)
        {
            bool running = sequence != null;
            bool closed = running && sequence[pulseIndex].Closed;
            var previous = solver.LatestSolveResult;
            SetContact(closed);
            // A contact change can solve immediately. Reuse that step when it does.
            if (ReferenceEquals(previous, solver.LatestSolveResult))
            {
                solver.MarkDirty();
                if (!solver.SolveNow())
                {
                    FailTrial("The solver failed during the pulse. Restart the trial.");
                    return false;
                }
            }
            var result = solver.LatestSolveResult;
            if (result == null || !ReadVoltages(result))
            {
                FailTrial("A voltage sample was unavailable. Restart the trial.");
                return false;
            }
            bool nextRaw = rawVolts >= RawThreshold;
            if (running && !rawHigh && nextRaw)
            {
                rawEdges++;
                if (!filteredMode) partCount++;
            }
            rawHigh = nextRaw;
            if (!filteredHigh && filteredVolts >= PressThreshold)
            {
                filteredHigh = true;
                if (running)
                {
                    filteredEdges++;
                    if (filteredMode) partCount++;
                }
            }
            else if (filteredHigh && filteredVolts <= RearmThreshold) filteredHigh = false;

            if (!running) return filteredVolts > RestVoltage;
            filteredPeak = Mathf.Max(filteredPeak, filteredVolts);
            sampleElapsed += step;
            if (sampleElapsed >= .05f - .00001f)
            {
                sampleElapsed -= .05f;
                PlotSample();
            }
            pulseElapsed += step;
            if (pulseElapsed + step * .5f < sequence[pulseIndex].Seconds) return true;
            pulseElapsed = 0f;
            pulseIndex++;
            if (pulseIndex < sequence.Length) return true;
            PlotSample();
            sequence = null;
            VerifyTrial();
            return false;
        }

        private void VerifyTrial()
        {
            bool passed = stage switch
            {
                1 => rawEdges == 3 && partCount == 3 && !rawHigh && filteredVolts <= RestVoltage,
                2 => rawEdges == 1 && filteredEdges == 0 && partCount == 0 && filteredPeak < PressThreshold,
                3 => rawEdges == 3 && filteredEdges == 1 && partCount == 1 && filteredHigh,
                4 => rawEdges == 2 && filteredEdges == 0 && partCount == 1 && !filteredHigh && !rawHigh,
                5 => rawEdges == 5 && filteredEdges == 1 && partCount == 2 && !filteredHigh && !rawHigh,
                _ => false
            };
            if (passed) Award();
            else FailTrial("The waveform or count was outside the expected range. Restart the trial.");
        }

        private bool ReadVoltages(SparkElectricalSolveResult result)
        {
            float rail = source.ElectricalState.Voltage;
            return source.IsOutputActive && !source.IsOverloaded && rail >= 4.8f && rail <= 5.2f &&
                result.TryGetVoltage(feed.TerminalB, source.NegativeTerminal, out rawVolts) &&
                result.TryGetVoltage(capacitor.PositiveTerminal, source.NegativeTerminal, out filteredVolts) &&
                IsFinite(rawVolts) && IsFinite(filteredVolts);
        }

        private static bool IsFinite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);

        private void SetContact(bool closed)
        {
            if (contact != null && contact.IsClosed != closed)
                contact.SetState(closed ? SparkSwitchState.Closed : SparkSwitchState.Open);
        }

        private string Prompt()
        {
            if (sequence != null) return "Test running. Watch the two voltage traces and the part count.";
            return stage switch
            {
                0 => "Waiting for the released circuit to settle near 0 V.",
                1 => "Press RAW TEST to see one deliberate press count three times.",
                2 => filteredMode ? "Press 100 ms GLITCH. The filtered count should stay at zero."
                    : "RAW counted three edges. Select FILTERED MODE to reset the count.",
                3 => "Press BOUNCED PRESS. The filtered counter should advance once.",
                4 => "Press BOUNCED RELEASE and wait for the input to rearm.",
                5 => "Run SECOND CYCLE. Verify exactly two parts for two deliberate presses.",
                _ => manager != null && manager.SceneBindings == bindings && manager.IsCompleted
                    ? "Counter verified. Two deliberate presses produced two parts."
                    : "Six checks recorded. Waiting for level completion."
            };
        }

        private void Show(string message)
        {
            if (statusText != null) statusText.text = message;
            if (meterText != null) meterText.text = ready
                ? $"RAW {rawVolts:0.00} V   |   FILTERED {filteredVolts:0.00} V   |   RAW EDGES {rawEdges}   |   PART COUNT {partCount}   |   {(filteredHigh ? "HELD" : "ARMED")}"
                : "RAW —   |   FILTERED —   |   RAW EDGES —   |   PART COUNT —";
            if (countIndicator != null) countIndicator.SetActive(ready && partCount > 0);
            SetButton(rawTestButton, CanStart(1));
            SetButton(filteredModeButton, CanStart(2) && !filteredMode);
            SetButton(glitchButton, CanStart(2) && filteredMode);
            SetButton(pressButton, CanStart(3));
            SetButton(releaseButton, CanStart(4));
            SetButton(cycleButton, CanStart(5));
            SetButton(restartButton, ready && sequence == null && stage > 0 && stage < Gates.Length);
        }

        private static void SetButton(Button button, bool enabled)
        {
            if (button != null) button.interactable = enabled;
        }

        private void Award()
        {
            if (!bindings.SetRuntimeComponentBinding(Gates[stage], source))
            {
                FailTrial("Could not record the Level 38 objective. Restart the trial.");
                return;
            }
            stage++;
            QueueEvaluation();
        }

        private void QueueEvaluation()
        {
            if (manager != null && manager.SceneBindings == bindings) manager.QueueEvaluation();
        }

        private void ClearGates()
        {
            if (bindings != null)
                foreach (string gate in Gates) bindings.ClearRuntimeComponentBinding(gate);
            if (stage > 0) QueueEvaluation();
        }

        private void ResetTrial()
        {
            ClearGates();
            stage = rawEdges = filteredEdges = partCount = 0;
            ready = filteredMode = rawHigh = filteredHigh = failed = false;
            sequence = null;
            accumulator = 0f;
            feedback = null;
            samples.Clear();
            RefreshTrace();
            SetContact(false);
        }

        private void FailTrial(string reason)
        {
            sequence = null;
            failed = true;
            feedback = reason;
            SetContact(false);
        }

        private void Invalidate(string reason)
        {
            if (ready || stage > 0) ResetTrial();
            Show(reason);
        }

        private void OnDisable()
        {
            if (gameObject.activeInHierarchy) ResetTrial();
            else ClearGates();
            bindings = null;
            ready = false;
        }

        private void PlotSample()
        {
            if (samples.Count == SampleCount) samples.RemoveAt(0);
            samples.Add(new Vector2(rawVolts, filteredVolts));
            RefreshTrace();
        }

        private void RefreshTrace()
        {
            DrawTrace(rawTrace, ref rawLines, true, new Color(1f,.72f,.30f));
            DrawTrace(filteredTrace, ref filteredLines, false, new Color(.31f,.92f,.86f));
        }

        private void DrawTrace(RectTransform plot, ref RectTransform[] segments, bool raw, Color color)
        {
            if (plot == null) return;
            if (segments == null)
            {
                if (samples.Count < 2) return;
                segments = new RectTransform[SampleCount - 1];
                for (int i = 0; i < segments.Length; i++)
                {
                    var line = new GameObject(raw ? "RAW sample" : "FILTERED sample", typeof(RectTransform), typeof(Image));
                    var rect = line.GetComponent<RectTransform>();
                    rect.SetParent(plot, false);
                    rect.anchorMin = rect.anchorMax = Vector2.zero;
                    rect.pivot = new Vector2(0f, .5f);
                    var image = line.GetComponent<Image>();
                    image.color = color;
                    image.raycastTarget = false;
                    segments[i] = rect;
                }
            }
            Vector2 size = plot.rect.size;
            int horizontalSamples = Mathf.Max(40, samples.Count - 1);
            for (int i = 0; i < segments.Length; i++)
            {
                var line = segments[i];
                bool visible = i + 1 < samples.Count;
                if (line.gameObject.activeSelf != visible) line.gameObject.SetActive(visible);
                if (!visible) continue;
                float start = raw ? samples[i].x : samples[i].y;
                float end = raw ? samples[i + 1].x : samples[i + 1].y;
                Vector2 from = new Vector2(size.x * i / horizontalSamples, size.y * Mathf.Clamp01(start / 5.2f));
                Vector2 to = new Vector2(size.x * (i + 1) / horizontalSamples, size.y * Mathf.Clamp01(end / 5.2f));
                Vector2 delta = to - from;
                line.anchoredPosition = from;
                line.sizeDelta = new Vector2(delta.magnitude + 1f, 2f);
                line.localRotation = Quaternion.Euler(0, 0, Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg);
            }
        }

        private static bool Available(SparkElectricalComponent component) =>
            component != null && component.isActiveAndEnabled && component.ElectricalEnabled;

        private string CheckWiring(SparkElectricalNetworkGraph graph)
        {
            if (graph == null) return "Waiting for the solved circuit graph.";
            var parents = new Dictionary<int, int>();
            int Root(int node)
            {
                while (parents.TryGetValue(node, out int parent)) node = parent;
                return node;
            }
            foreach (var component in graph.ComponentTerminals.Keys)
            {
                if (!(component is SparkSingleWire wire) || !Available(wire) ||
                    !graph.TryGetPrimaryComponentTerminals(wire, out var a, out var b) ||
                    !graph.TryGetNode(a, out int an) || !graph.TryGetNode(b, out int bn)) continue;
                int aRoot = Root(an);
                int bRoot = Root(bn);
                if (aRoot != bRoot) parents[bRoot] = aRoot;
            }
            var nets = new[]
            {
                new[] { source.PositiveTerminal, contact.InputTerminal },
                new[] { contact.OutputTerminal, feed.TerminalA },
                new[] { feed.TerminalB, pullDown.TerminalA, filter.TerminalA },
                new[] { filter.TerminalB, capacitor.PositiveTerminal },
                new[] { source.NegativeTerminal, pullDown.TerminalB, capacitor.NegativeTerminal }
            };
            var used = new Dictionary<int, string>();
            foreach (var net in nets)
            {
                if (!graph.TryGetNode(net[0], out int first)) return "A required terminal is missing from the circuit graph.";
                int root = Root(first);
                if (used.TryGetValue(root, out string other)) return $"Separate {net[0].name} from {other}.";
                used.Add(root, net[0].name);
                for (int i = 1; i < net.Length; i++)
                {
                    if (!graph.TryGetNode(net[i], out int node)) return "A required terminal is missing from the circuit graph.";
                    if (Root(node) != root) return $"Connect {net[0].name} to {net[i].name}.";
                }
            }
            return null;
        }
    }
}
