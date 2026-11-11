using System;
using System.Collections.Generic;
using ProjectSpark.Electrical;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.Gameplay
{
    [DisallowMultipleComponent]
    public sealed class SparkLevel32OpticalSorter : MonoBehaviour
    {
        public static readonly string[] Gates = {
            "L32_Powered", "L32_Calibrated", "L32_Dark", "L32_Bright", "L32_ReviewGlare", "L32_MixedBatch"
        };

        private enum Bin { Review, Dark, Bright }
        private readonly struct Sample
        {
            public readonly string Name;
            public readonly float Light;
            public readonly Bin Expected;
            public readonly bool Glare;

            public Sample(string name, float light, Bin expected, bool glare = false)
            { Name = name; Light = light; Expected = expected; Glare = glare; }
        }

        private static readonly Sample[][] Trials = {
            new[] { new Sample("Dark", .15f, Bin.Dark) },
            new[] { new Sample("Bright", .90f, Bin.Bright) },
            new[] { new Sample("Borderline", .50f, Bin.Review), new Sample("Dark with glare", .15f, Bin.Dark, true) },
            new[] { new Sample("Bright", .86f, Bin.Bright), new Sample("Borderline", .48f, Bin.Review),
                new Sample("Dark with glare", .19f, Bin.Dark, true), new Sample("Dark", .13f, Bin.Dark),
                new Sample("Bright", .82f, Bin.Bright), new Sample("Borderline", .58f, Bin.Review) }
        };

        public SparkLevelLayout layout;
        public SparkPowerSupply source;
        public SparkSwitch main;
        public SparkLDR sensor;
        public SparkResistor divider;
        public SparkMotor motor;
        public Transform[] beltSlats;
        public Transform shaft, gate, part;
        public Renderer partRenderer;
        public Material darkPart, brightPart, reviewPart;
        public Light inspectionLight;
        public Text statusText, meterText, countText;
        public Image mainIndicator;
        public Button darkReference, brightReference, runTrial;

        private const float DarkLimit = 1.5f, BrightLimit = 3.5f, StabilityTime = .24f;
        private const float InspectStart = .65f, InspectEnd = 1.25f, GateArrival = 1.75f, BinArrival = 2.50f, SampleEnd = 2.80f;
        private SparkLevelLayoutLoader loader;
        private SparkElectricalSolver solver;
        private SparkLevelSceneBindings bindings;
        private LevelGamePlayManager levelManager;
        private SparkElectricalSolveResult checkedResult;
        private string wiringProblem, retry;
        private int stage, reference, sampleIndex, darkCount, brightCount, reviewCount;
        private bool ready, darkCalibrated, brightCalibrated, locked;
        private float timer, stableTime, beltOffset, referenceTime;
        private Sample[] activeTrial;
        private Bin candidate, route;

        public void ToggleMain()
        {
            if (main == null || bindings == null || stage == Gates.Length) return;
            if (main.IsClosed) { main.SetState(SparkSwitchState.Open); ResetProgress(); }
            else if (ready) main.SetState(SparkSwitchState.Closed);
        }

        public void UseDarkReference() => SetReference(1, .15f);
        public void UseBrightReference() => SetReference(2, .90f);

        private void SetReference(int tile, float light)
        {
            if (!ready || stage != 1 || !main.IsClosed) return;
            reference = tile;
            referenceTime = 0;
            SetIllumination(light);
            ShowPart(tile == 1 ? Bin.Dark : Bin.Bright, new Vector3(-.08f, .12f, 0));
        }

        public void RunTrial()
        {
            if (!ready || !main.IsClosed || stage < 2 || stage >= Gates.Length || activeTrial != null) return;
            activeTrial = Trials[stage - 2];
            sampleIndex = darkCount = brightCount = reviewCount = 0;
            retry = null;
            BeginSample();
        }

        private void Update()
        {
            if (loader == null) loader = FindFirstObjectByType<SparkLevelLayoutLoader>();
            if (layout == null || loader == null || loader.ActiveLayout != layout) return;
            if (solver == null) solver = FindFirstObjectByType<SparkElectricalSolver>();
            if (levelManager == null) levelManager = FindFirstObjectByType<LevelGamePlayManager>();
            if (bindings != loader.ActiveBindings)
            {
                ResetProgress();
                bindings = loader.ActiveBindings;
                checkedResult = null;
            }
            UpdateControls();
            if (solver == null || bindings == null || Array.Exists(Gates, id => !bindings.IsComponentIdDeclared(id)))
            { SetStatus("Waiting for the solver and six Level 32 objective bindings."); return; }
            if (solver.SolverRegistry == null || !Available(source) || !Available(main) || !Available(sensor) ||
                !Available(divider) || !Available(motor) || !solver.SolverRegistry.TryGetDefinition(sensor, out _) ||
                !solver.SolverRegistry.TryGetDefinition(motor, out _))
            { Invalidate("Check the LDR and motor solver registrations and enable the five bench components."); return; }

            if (solver.IsSolving) return;
            if (solver.IsDirty || solver.LatestSolveResult == null) solver.SolveNow();
            var result = solver.LatestSolveResult;
            if (solver.IsDirty || solver.IsSolving || result == null) return;
            if (!ReferenceEquals(checkedResult, result))
            {
                checkedResult = result;
                wiringProblem = CheckWiring(result.Context.Graph);
            }
            if (wiringProblem != null) { Invalidate(wiringProblem); return; }
            float supply = source.ElectricalState.Voltage;
            if (!source.IsOutputActive || supply < 4.8f || supply > 5.2f || source.IsOverloaded || motor.IsOverCurrent)
            { Invalidate("Check the 5 V supply and make sure the motor branch is clear of shorts."); return; }
            ready = true;
            if (!main.IsClosed)
            {
                if (stage > 0) ResetProgress();
                SetMeter("SENSE  —   |   MAIN OFF");
                SetStatus("Wiring verified. Turn MAIN on.");
                return;
            }
            if (!motor.IsRunning || motor.ElectricalState.Current < .02f ||
                !result.TryGetVoltage(divider.TerminalA, source.NegativeTerminal, out float sense) ||
                float.IsNaN(sense) || float.IsInfinity(sense))
            { Invalidate("Check motor polarity and the SENSE connection."); return; }

            float dt = Mathf.Min(Time.deltaTime, .05f);
            AnimateBelt(dt);
            SetMeter($"SENSE  {sense:0.00} V   |   {Classify(sense).ToString().ToUpperInvariant()}");
            switch (stage)
            {
                case 0: Award(); break;
                case 1: Calibrate(sense, dt); break;
                case 6:
                    SetStatus(levelManager != null && levelManager.SceneBindings == bindings && levelManager.IsCompleted
                        ? "Quality test passed. All six parts reached the correct bins."
                        : "All six parts sorted correctly. Waiting for level completion.");
                    break;
                default:
                    if (activeTrial != null) AdvanceSample(sense, dt);
                    else SetStatus(retry ?? TrialPrompt());
                    break;
            }
        }

        private void Calibrate(float sense, float dt)
        {
            bool accepted = reference == 1 && sense >= .55f && sense <= 1.20f ||
                            reference == 2 && sense >= 3.95f && sense <= 4.60f;
            referenceTime = accepted ? referenceTime + dt : 0;
            if (referenceTime >= StabilityTime)
            {
                if (reference == 1) darkCalibrated = true;
                else brightCalibrated = true;
            }
            if (darkCalibrated && brightCalibrated)
            {
                if (Award()) HidePart();
                return;
            }
            SetStatus($"Calibrate the references. DARK {(darkCalibrated ? "OK" : "pending")}   BRIGHT {(brightCalibrated ? "OK" : "pending")}" +
                (reference != 0 && !accepted ? "   Check the 10 kΩ divider and LDR wiring." : ""));
        }

        private void BeginSample()
        {
            timer = stableTime = 0;
            route = candidate = Bin.Review;
            locked = false;
            SetIllumination(.50f);
            var sample = activeTrial[sampleIndex];
            ShowPart(sample.Expected, new Vector3(-.54f,.12f,0));
        }

        private void AdvanceSample(float sense, float dt)
        {
            var sample = activeTrial[sampleIndex];
            bool inspecting = timer >= InspectStart && timer < InspectEnd;
            bool glare = sample.Glare && timer < InspectStart + .12f;
            if (SetIllumination(inspecting ? (glare ? .98f : sample.Light) : .50f)) return;
            if (inspecting && !locked)
            {
                var reading = Classify(sense);
                if (reading != candidate) { candidate = reading; stableTime = 0; }
                stableTime += dt;
                if (stableTime >= StabilityTime) { route = candidate; locked = true; }
            }
            timer += dt;
            if (timer >= InspectEnd) locked = true;
            SetStatus($"Part {sampleIndex + 1}/{activeTrial.Length}  •  {sample.Name}  •  " +
                (timer >= BinArrival ? $"Arrived in {route.ToString().ToUpperInvariant()}" :
                    locked ? $"Locked to {route.ToString().ToUpperInvariant()}" : inspecting ? "Measuring" : "Approaching sensor"));
            MovePart();
            if (timer < SampleEnd) return;
            if (route == Bin.Dark) darkCount++;
            else if (route == Bin.Bright) brightCount++;
            else reviewCount++;

            if (route != sample.Expected)
            {
                retry = $"{sample.Name} reached {route}. Expected {sample.Expected}. Check SENSE and retry.";
                EndTrial();
                return;
            }
            sampleIndex++;
            if (sampleIndex < activeTrial.Length) { BeginSample(); return; }
            if (!Award()) retry = "The level could not record the objective. Check the Level 32 bindings and retry.";
            EndTrial();
        }

        private void MovePart()
        {
            float binZ = route == Bin.Dark ? -.30f : route == Bin.Bright ? .30f : 0;
            float alongBelt = Mathf.InverseLerp(0, GateArrival, timer);
            float toBin = Mathf.InverseLerp(GateArrival, BinArrival, timer);
            if (part != null) part.localPosition = new Vector3(Mathf.Lerp(-.54f,.35f,alongBelt) + .36f * toBin,
                .12f - .04f * toBin, binZ * toBin);
            if (gate != null) gate.localRotation = Quaternion.Euler(0, -binZ * 115f * Mathf.InverseLerp(InspectEnd,GateArrival,timer), 0);
        }

        private void AnimateBelt(float dt)
        {
            if (inspectionLight != null) inspectionLight.enabled = true;
            if (shaft != null) shaft.Rotate(Vector3.right, 720f * dt, Space.Self);
            if (beltSlats == null || beltSlats.Length == 0) return;
            beltOffset = Mathf.Repeat(beltOffset + dt * .50f, .94f);
            for (int i = 0; i < beltSlats.Length; i++)
            {
                if (beltSlats[i] == null) continue;
                var p = beltSlats[i].localPosition;
                p.x = Mathf.Repeat(i * .94f / beltSlats.Length + beltOffset, .94f) - .54f;
                beltSlats[i].localPosition = p;
            }
        }

        private void ShowPart(Bin appearance, Vector3 position)
        {
            if (partRenderer != null) partRenderer.sharedMaterial = appearance == Bin.Dark ? darkPart : appearance == Bin.Bright ? brightPart : reviewPart;
            if (part == null) return;
            part.localPosition = position;
            part.gameObject.SetActive(true);
        }

        private void HidePart()
        {
            if (part != null) part.gameObject.SetActive(false);
            if (gate != null) gate.localRotation = Quaternion.identity;
        }

        private bool SetIllumination(float value)
        {
            if (sensor == null || Mathf.Approximately(sensor.Illumination,value)) return false;
            sensor.SetIllumination(value);
            return true;
        }

        private static Bin Classify(float sense) => sense <= DarkLimit ? Bin.Dark : sense >= BrightLimit ? Bin.Bright : Bin.Review;
        private bool Award()
        {
            if (!bindings.SetRuntimeComponentBinding(Gates[stage], source)) return false;
            stage++;
            // Runtime bindings do not emit an electrical or topology event.
            QueueEvaluation();
            return true;
        }

        private void QueueEvaluation()
        {
            if (levelManager != null && levelManager.SceneBindings == bindings)
                levelManager.QueueEvaluation();
        }

        private string TrialPrompt() => stage == 2 ? "Run the dark part test." : stage == 3 ? "Run the bright part test." :
            stage == 4 ? "Run the borderline and brief glare test." : "Run the six-part mixed batch.";

        private void EndTrial() { activeTrial = null; HidePart(); SetIllumination(.50f); }

        private void ResetProgress()
        {
            if (bindings != null) foreach (string id in Gates) bindings.ClearRuntimeComponentBinding(id);
            if (stage > 0) QueueEvaluation();
            stage = reference = darkCount = brightCount = reviewCount = 0;
            darkCalibrated = brightCalibrated = false;
            retry = null;
            EndTrial();
            if (inspectionLight != null) inspectionLight.enabled = false;
        }

        private void Invalidate(string reason)
        {
            if (ready || stage > 0) ResetProgress();
            ready = false;
            if (main != null && main.IsClosed) main.SetState(SparkSwitchState.Open);
            SetMeter("SENSE  —");
            SetStatus(reason);
        }

        private void OnDisable() { ResetProgress(); bindings = null; checkedResult = null; ready = false; }
        private void SetStatus(string text) { if (statusText != null && statusText.text != text) statusText.text = text; }
        private void SetMeter(string text) { if (meterText != null && meterText.text != text) meterText.text = text; }

        private void UpdateControls()
        {
            bool powered = ready && main != null && main.IsClosed;
            if (mainIndicator != null) mainIndicator.color = powered ? new Color(.12f,.50f,.34f) : new Color(.22f,.29f,.37f);
            if (darkReference != null) darkReference.interactable = powered && stage == 1;
            if (brightReference != null) brightReference.interactable = powered && stage == 1;
            if (runTrial != null) runTrial.interactable = powered && stage >= 2 && stage < Gates.Length && activeTrial == null;
            string count = $"DARK {darkCount}   |   BRIGHT {brightCount}   |   REVIEW {reviewCount}";
            if (countText != null && countText.text != count) countText.text = count;
        }

        private static bool Available(SparkElectricalComponent component) => component != null && component.isActiveAndEnabled && component.ElectricalEnabled;

        private string CheckWiring(SparkElectricalNetworkGraph graph)
        {
            if (graph == null) return "Waiting for the solved circuit graph.";
            var wires = new Dictionary<int,int>();
            int Root(int node)
            {
                while (wires.TryGetValue(node, out int parent)) node = parent;
                return node;
            }
            foreach (var entry in graph.ComponentTerminals)
            {
                if (!(entry.Key is SparkSingleWire wire) || !Available(wire) ||
                    !graph.TryGetPrimaryComponentTerminals(wire,out var a,out var b) ||
                    !graph.TryGetNode(a,out int an) || !graph.TryGetNode(b,out int bn)) continue;
                int aRoot = Root(an), bRoot = Root(bn);
                if (aRoot != bRoot) wires[bRoot] = aRoot;
            }
            var nets = new[] {
                new[] { source.PositiveTerminal, main.InputTerminal },
                new[] { main.OutputTerminal, sensor.TerminalA, motor.PositiveTerminal },
                new[] { sensor.TerminalB, divider.TerminalA },
                new[] { source.NegativeTerminal, divider.TerminalB, motor.NegativeTerminal }
            };
            var used = new Dictionary<int,string>();
            foreach (var net in nets)
            {
                if (net[0] == null || !graph.TryGetNode(net[0], out int first)) return "A required terminal is missing from the solver graph.";
                int root = Root(first);
                if (used.TryGetValue(root,out string other)) return $"Separate {net[0].name} from {other}. Check for a bypass wire.";
                used.Add(root,net[0].name);
                foreach (var pin in net)
                {
                    if (pin == null || !graph.TryGetNode(pin,out int node)) return $"A terminal is missing on the {net[0].name} path.";
                    if (Root(node) != root) return $"Connect {net[0].name} to {pin.name}. See the wiring diagram.";
                }
            }
            return null;
        }
    }
}
