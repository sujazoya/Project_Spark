using System.Collections.Generic;
using AAAUI.VFX;
using ProjectSpark.Circuit;
using ProjectSpark.Visuals;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.Gameplay
{
    public sealed class SparkLevel92Process : MonoBehaviour
    {
        public static readonly string[] Gates = { "L92_Network", "L92_Calibrated", "L92_Commissioned", "L92_Endurance", "L92_Repairs", "L92_Requalified" };
        private const int Feedback = 170, Backup = 158;
        public SparkLevelLayout layout;
        public SparkPowerSupply source, primary, secondary;
        public SparkTerminal ground;
        public SparkTerminal[] pins;
        public TextMesh[] labels;
        public string[] endpoints, purposes;
        public Transform arena;
        public SparkLevel92VisualRig visuals;
        public Text status, measurements, instructions;
        private readonly SparkLevel92Plant plant = new();
        private readonly Dictionary<SparkTerminal, int> index = new();
        private readonly List<SparkCircuitConnection> connections = new();
        private readonly bool[] connected = new bool[188];
        private readonly bool[] repaired = new bool[4];
        private LevelGamePlayManager manager;
        private SparkLevelLayoutLoader loader;
        private SignalWireBuilder wireTool;
        private SparkLevel88ArenaView view;
        private int version = -1, missing = -1, count, stage, fault, runId;
        private int sampleEpoch, recoveryEpoch;
        private bool extra, trusted, backup, latched, armed, witnessed, recalibrated, reset, running, faultProven;
        private float elapsed, accumulator, faultAge, recovery;
        private float lastPrimary = -1, lastSecondary = -1, sensorTime, sampleAge;
        private string message = "Connect the highlighted endpoints.";
        private GUIStyle terminalCaption;

        private void Start()
        {
            manager = FindFirstObjectByType<LevelGamePlayManager>();
            loader = FindFirstObjectByType<SparkLevelLayoutLoader>();
            wireTool = FindFirstObjectByType<SignalWireBuilder>();
            for (int i = 0; i < pins.Length; i++) index.Add(pins[i], i);
            view = gameObject.AddComponent<SparkLevel88ArenaView>();
            view.Initialize(arena, arena.Find("L92_HD_VISUAL_ART/L92_AsteroidSamplingMissionDeck"));
        }

        private void ReadWires(SparkCircuitSystem circuit)
        {
            bool previousFeedback = connected[Feedback];
            version = circuit.TopologyVersion;
            connections.Clear();
            circuit.CopyConnections(connections);
            System.Array.Clear(connected, 0, connected.Length);
            extra = false;
            foreach (var edge in connections)
            {
                if (edge.Kind != SparkConnectionKind.Wire) continue;
                bool hasA = index.TryGetValue(edge.A, out int a);
                bool hasB = index.TryGetValue(edge.B, out int b);
                if (!hasA && !hasB) continue;
                if (!hasA || !hasB || a/2 != b/2 || a == b) extra = true;
                else connected[a/2] = true;
            }

            count = 0;
            missing = -1;
            for (int i = 0; i < connected.Length; i++)
                if (connected[i]) count++;
                else if (missing < 0) missing = i;

            if (latched && !previousFeedback && connected[Feedback]) recoveryEpoch = sampleEpoch;
            if (!connected[Feedback] && (armed || stage >= 3 && trusted))
            {
                witnessed |= armed && count == 187 && !extra;
                latched = true;
                recoveryEpoch = sampleEpoch;
                plant.StopRun();
                armed = running = recalibrated = reset = false;
                message = "W171 removed. Trusted anchor load lost. Drill inhibited.";
            }
            else if (running && (extra || missing >= 0))
            {
                running = false;
                plant.StopRun();
                message = "Qualification interrupted by a wiring change. Restore the network and start a fresh run.";
            }
        }

        private bool Rail(int wire)
        {
            var solve = manager.ElectricalSolver.LatestSolveResult;
            return connected[wire] && solve != null && solve.TryGetVoltage(pins[wire*2+1], ground, out float volts) &&
                (wire is 3 or 7 or 11 ? Mathf.Abs(volts) < .15f : volts is > 4.6f and < 5.3f);
        }

        private bool Powered()
        {
            if (!source.IsOutputActive || source.IsOverloaded) return false;
            for (int i = 0; i < 12; i++) if (!Rail(i)) return false;
            return true;
        }

        private bool ActualSample(int wire)
        {
            var solve = manager.ElectricalSolver.LatestSolveResult;
            return connected[wire] && solve != null && solve.TryGetVoltage(pins[wire*2+1], ground, out float volts) &&
                volts is > .6f and < 4.4f;
        }

        private static void Drive(SparkPowerSupply transmitter, bool enabled, float voltage, ref float previous)
        {
            if (enabled && transmitter.IsOutputActive && Mathf.Abs(previous-voltage)<.02f) return;
            if (transmitter.IsOutputActive) transmitter.SetOutput(false, default);
            previous = -1;
            if (enabled && transmitter.TryConfigure(voltage,.01f,out _))
            {
                transmitter.SetOutput(true, default);
                previous = voltage;
            }
        }

        private void Update()
        {
            if (!loader || !manager || loader.ActiveLayout != layout || !loader.ActiveBindings ||
                !manager.CircuitSystem || !manager.ElectricalSolver) return;

            var circuit = manager.CircuitSystem;
            if (version != circuit.TopologyVersion) ReadWires(circuit);

            sampleAge += Time.deltaTime;
            sensorTime -= Time.deltaTime;
            if (sensorTime <= 0)
            {
                sensorTime = .22f;
                sampleAge = 0;
                sampleEpoch++;
                Drive(primary, Rail(6), EncodeLoad(plant.AnchorLoad), ref lastPrimary);
                Drive(secondary, Rail(1), EncodeLoad(plant.Tether), ref lastSecondary);
            }

            trusted = sampleAge < .5f && primary.IsOutputActive && ActualSample(Feedback);
            backup = sampleAge < .5f && secondary.IsOutputActive && ActualSample(Backup);
            accumulator += Mathf.Min(Time.deltaTime, .15f) * 2f;
            while (accumulator >= .05f)
            {
                accumulator -= .05f;
                Tick(.05f);
            }

            Present();
        }

        private static float EncodeLoad(float load) => 1f + Mathf.Clamp01(load) * 3f;

        private bool NetworkReady => !extra && missing < 0 && Powered();
        private bool SamplingPermit => NetworkReady && trusted && backup && !latched && connected[147] && connected[160];

        private void Tick(float step)
        {
            bool permit = SamplingPermit;
            plant.Tick(step, Powered(), permit, fault);
            if (stage == 0 && permit) Award();
            if (stage == 1 && permit && plant.Surveyed) Award();
            if (stage == 2 && permit && plant.Stable) Award();

            if (fault != 0)
            {
                faultAge += step;
                faultProven |= fault switch
                {
                    1 => plant.Grip[0] < .2f && plant.Margin < .55f,
                    2 => plant.Rpm < 110f && plant.Torque > .9f && plant.Heat > .7f,
                    3 => plant.Seal < .55f && plant.Leak > .2f,
                    _ => false
                };
            }
            if (fault == 0 && plant.Stable && plant.Heat < .62f)
                recovery = Mathf.Max(0f, recovery - step);

            if (!running) return;
            elapsed += step;
            if (!permit || !plant.Stable || plant.Heat > .82f || elapsed > SparkLevel92Plant.MissionDuration)
            {
                running = false;
                plant.StopRun();
                message = "Sampling aborted. Verify anchor margin, bit temperature and cartridge condition.";
            }
            else if (plant.Sealed)
            {
                running = false;
                plant.StopRun();
                Award();
            }
        }

        private void Award()
        {
            if (stage >= Gates.Length || !loader.ActiveBindings.SetRuntimeComponentBinding(Gates[stage], source)) return;
            stage++;
            manager.QueueEvaluation();
            message = stage switch
            {
                1 => "Network verified. Calibrate and scan the geological survey grid.",
                2 => "Survey qualified. Deploy the lander and verify six-leg grip under load.",
                3 => "Anchor proof passed. Extract, retract and seal a core.",
                4 => "First core qualified. Diagnose and repair all three disturbances, then extract again.",
                5 => "Recovery qualified. Arm W171, then remove it in Wire Manager.",
                _ => "Silent Core qualified. All six objectives recorded."
            };
        }

        public void Power()
        {
            running = false;
            plant.StopRun();
            source.SetOutput(!source.IsOutputActive, default);
        }

        public void Calibrate()
        {
            if (!NetworkReady || !trusted || !backup) return;
            if (stage == 1)
            {
                plant.BeginSurvey();
                message = "Survey calibrated. Measuring layered echoes and site uncertainty.";
            }
            else if (latched && sampleEpoch > recoveryEpoch && plant.Stable)
            {
                recalibrated = true;
                message = "Fresh anchor load validated against independent tether protection. Select RESET SAFETY.";
            }
        }

        public void Deploy()
        {
            if (stage != 2 || !SamplingPermit || !plant.Surveyed) return;
            plant.Deploy();
            message = "Lander deploying. Verify all six feet and the load proof.";
        }

        public void Run()
        {
            bool eligible = stage == 3 || stage == 4 && repaired[1] && repaired[2] && repaired[3] ||
                stage == 5 && witnessed && recalibrated && reset;
            if (!eligible || running || fault != 0 || recovery > 0f || !SamplingPermit || !plant.Stable || !plant.MotorOff)
            {
                message = "Extraction requires a surveyed site, stable anchors, and an idle drill.";
                return;
            }
            elapsed = 0f;
            plant.BeginExtraction();
            runId++;
            running = true;
        }

        private void Inject(int type)
        {
            if (stage != 4 || fault != 0 || running || recovery > 0f || repaired[type] || !SamplingPermit) return;
            fault = type;
            faultProven = false;
            faultAge = 0f;
            message = "Disturbance active. Confirm its measured signature before selecting REPAIR.";
        }

        public void SlipAnchor() => Inject(1);
        public void StallBit() => Inject(2);
        public void FailSeal() => Inject(3);

        public void Repair()
        {
            if (!faultProven || fault == 0 || faultAge < 1f) return;
            repaired[fault] = true;
            fault = 0;
            recovery = 1.5f;
            message = "Repair applied. Waiting for mechanical conditions to settle.";
        }

        public void Arm()
        {
            if (stage != 5 || !SamplingPermit || running || witnessed) return;
            armed = true;
            message = "Remove W171 in Wire Manager, then reconnect its highlighted endpoints.";
        }

        public void ResetSafety()
        {
            if (!latched || !recalibrated || !NetworkReady || !trusted || !backup || !plant.Stable || !plant.MotorOff) return;
            latched = false;
            reset = true;
            message = "Safety reset accepted. Extract and qualify a fresh sealed core.";
        }

        public void ArenaView() { if (view) view.UseOverview(); }
        public void SocketView()
        {
            if (!view) return;
            int route = armed ? Feedback : missing;
            if (route < 0) view.UseSocketView();
            else view.UseSocketView(pins[route*2].transform.position, pins[route*2+1].transform.position);
        }

        private void OnGUI()
        {
            if (ProjectSpark.UIManagement.SparkUIManager.SuppressWorkbenchAnnotations) return;
            if (Event.current.type != EventType.Repaint || !loader || loader.ActiveLayout != layout) return;
            var camera = Camera.main;
            if (!camera) return;
            int route = armed ? Feedback : missing;
            var previous = GUI.color;
            if (terminalCaption == null)
                terminalCaption = new GUIStyle(GUI.skin.label) { alignment=TextAnchor.MiddleCenter, fontStyle=FontStyle.Bold, fontSize=15 };

            void Marker(int index, Color color, float captionOffset = 0f, float captionLift = 0f)
            {
                Vector3 p = camera.WorldToScreenPoint(pins[index].transform.position);
                if (p.z <= 0 || p.x < 0 || p.x > Screen.width || p.y < 0 || p.y > Screen.height) return;
                float y = Screen.height-p.y;
                GUI.color = color;
                GUI.DrawTexture(new Rect(p.x-10,y-10,20,2),Texture2D.whiteTexture);
                GUI.DrawTexture(new Rect(p.x-10,y+8,20,2),Texture2D.whiteTexture);
                GUI.DrawTexture(new Rect(p.x-10,y-10,2,20),Texture2D.whiteTexture);
                GUI.DrawTexture(new Rect(p.x+8,y-10,2,20),Texture2D.whiteTexture);
                float captionX = Mathf.Clamp(p.x + captionOffset, 45f, Screen.width - 45f);
                var caption = new Rect(captionX - 45f, Mathf.Max(0f, y - 36f - captionLift), 90f, 22f);
                GUI.color = new Color(.035f,.085f,.13f,.96f);
                GUI.DrawTexture(caption,Texture2D.whiteTexture);
                GUI.color = color;
                GUI.Label(caption,labels[index].text,terminalCaption);
            }

            if (route >= 0)
            {
                int output = route * 2;
                int input = output + 1;
                float dx = camera.WorldToScreenPoint(pins[input].transform.position).x -
                    camera.WorldToScreenPoint(pins[output].transform.position).x;
                float direction = dx >= 0f ? 1f : -1f;
                Marker(output, new Color(1f, .68f, .25f), -48f * direction);
                Marker(input, new Color(.25f, .88f, 1f), 48f * direction, Mathf.Abs(dx) < 12f ? 24f : 0f);
            }
            if (wireTool && wireTool.HoveredTerminal && index.TryGetValue(wireTool.HoveredTerminal,out int hovered) &&
                hovered/2 != route && !SignalWireBuilder.AnyWireDrawing)
                Marker(hovered,Color.white);
            GUI.color = previous;
        }
        private void Present()
        {
            int wire = armed ? Feedback : missing;
            string trust = trusted ? "LIVE" : "INVALID";
            string safety = latched ? "DRILL INHIBITED" : plant.MotorOff ? "STANDBY" : "DRILL ACTIVE";
            status.text = $"SILENT CORE  {stage}/6  |  {count}/188 WIRES  |  W171 {trust}  |  BACKUP {(backup ? "LIVE" : "OFFLINE")}  |  {safety}";
            measurements.text = $"SURVEY {plant.Coverage:P0}   SITE {plant.Site}   UNCERTAINTY {plant.Uncertainty:F2}   ANCHOR MARGIN {plant.Margin:F2}   DEPTH {plant.Depth:F2}   CORE {plant.SampleMass:F2}   LEAK {plant.Leak:F3}";
            instructions.text = extra ? "Remove the incorrect wire." : wire >= 0 ?
                $"{endpoints[wire * 2]} → {endpoints[wire * 2 + 1]}  |  {purposes[wire]}" :
                running ? $"CORE RUN {runId}  |  {plant.Phase}  |  BIT {plant.Rpm:F0} RPM  |  TORQUE {plant.Torque:F2}  |  HEAT {plant.Heat:F2}" :
                fault != 0 ? $"FOOT A GRIP {plant.Grip[0]:F2}  |  BIT {plant.Rpm:F0} RPM  |  HEAT {plant.Heat:F2}  |  SEAL LEAK {plant.Leak:F3}" : message;

            for (int i = 0; i < 6; i++)
            {
                float extension = plant.Extension[i];
                visuals.SetActualLegPose(i, extension * 36f, extension * -28f, plant.Grip[i] * 18f,
                    extension, plant.Grip[i]);
            }
            visuals.SetActualMechanicalState(plant.Rpm, plant.Depth, plant.Tether < .8f ? plant.Tether * 80f : 0f,
                plant.Seal * 60f, plant.Torque, Mathf.Clamp01(plant.Margin));
            visuals.SetActualSafetyState(trusted, latched, stage == Gates.Length);
        }
    }
}
