using System.Collections.Generic;
using AAAUI.VFX;
using ProjectSpark.Circuit;
using ProjectSpark.Visuals;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.Gameplay
{
    public sealed class SparkLevel89Process : MonoBehaviour
    {
        public static readonly string[] Gates = { "L89_Network", "L89_Calibrated", "L89_Commissioned", "L89_Endurance", "L89_Repairs", "L89_Requalified" };
        private const int Feedback = 148, Backup = 131;
        public SparkLevelLayout layout;
        public SparkPowerSupply source, primary, secondary;
        public SparkTerminal ground;
        public SparkTerminal[] pins;
        public TextMesh[] labels;
        public string[] endpoints, purposes;
        public Transform arena;
        public SparkLevel89VisualRig visuals;
        public Text status, measurements, instructions;
        private readonly SparkLevel89Plant plant = new();
        private readonly Dictionary<SparkTerminal, int> index = new();
        private readonly List<SparkCircuitConnection> connections = new();
        private readonly bool[] connected = new bool[170];
        private readonly bool[] repaired = new bool[4];
        private LevelGamePlayManager manager;
        private SparkLevelLayoutLoader loader;
        private SignalWireBuilder wireTool;
        private SparkLevel88ArenaView view;
        private int version = -1, missing = -1, count, stage, fault, runId;
        private int sampleEpoch, recoveryEpoch;
        private bool extra, trusted, backup, latched, armed, witnessed, recalibrated, reset, running, faultProven;
        private float elapsed, accumulator, faultAge, recovery, dose;
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
            view.Initialize(arena, arena.Find("L89_HD_VISUAL_ART/L89_HeliosRadiationDefenseArenaBase"));
        }

        private void ReadWires(SparkCircuitSystem circuit)
        {
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

            if (armed && !connected[Feedback] && count == 169 && !extra)
            {
                latched = witnessed = true; recoveryEpoch = sampleEpoch;
                armed = running = recalibrated = reset = false;
                message = "W149 removed. Trusted shelter dose invalid. Shelter is in protective hold.";
            }
            else if (running && (extra || missing >= 0))
            {
                running = false;
                message = "Qualification interrupted by a wiring change. Restore the network and start a fresh run.";
            }
        }

        private bool Rail(int wire)
        {
            var solve = manager.ElectricalSolver.LatestSolveResult;
            return connected[wire] && solve != null && solve.TryGetVoltage(pins[wire*2+1], ground, out float volts) &&
                volts is > 4.6f and < 5.3f;
        }

        private bool Powered()
        {
            if (!source.IsOutputActive || source.IsOverloaded) return false;
            for (int i = 0; i < 10; i++) if (!Rail(i)) return false;
            return connected[10] && connected[11];
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
                Drive(primary, Rail(6) && plant.DetectorsInRange, EncodeDose(plant.DoseRate), ref lastPrimary);
                Drive(secondary, Rail(7), EncodeDose(plant.DoseRate), ref lastSecondary);
            }

            trusted = sampleAge < .5f && plant.DetectorsInRange && primary.IsOutputActive && ActualSample(Feedback);
            backup = sampleAge < .5f && secondary.IsOutputActive && ActualSample(Backup) && connected[41];
            accumulator += Mathf.Min(Time.deltaTime, .15f) * 2f;
            while (accumulator >= .05f)
            {
                accumulator -= .05f;
                Tick(.05f);
            }

            Present();
        }

        private static float EncodeDose(float doseRate) => Mathf.Clamp(.8f + doseRate * .42f, .8f, 4.2f);

        private bool NetworkReady => !extra && missing < 0 && Powered();
        private bool CanCertify => NetworkReady && trusted && backup && !latched && plant.Ready;

        private void Tick(float step)
        {
            bool powered = Powered();
            bool essential = powered && connected[4] && connected[114] && connected[122] && connected[132];
            bool motion = powered && !latched && trusted && backup && connected[68] && connected[69] &&
                connected[101] && connected[128] && connected[136];

            if (running) elapsed += step;
            plant.Tick(step, essential, motion, running ? elapsed : -1f, fault, repaired[3]);
            if (stage == 0 && NetworkReady && trusted && backup) Award();
            if (stage == 2 && CanCertify) Award();

            if (fault != 0)
            {
                faultAge += step;
                faultProven |= fault switch
                {
                    1 => !plant.DetectorsInRange && !trusted && backup,
                    2 => plant.Positions[2] < .8f && !plant.Ready,
                    3 => plant.HighEnergy > 1.6f && plant.DoseRate > .22f,
                    _ => false
                };
            }
            if (fault == 0 && plant.Ready) recovery = Mathf.Max(0, recovery - step);
            if (!running) return;
            dose += plant.DoseRate * step;
            if (!CanCertify || dose > SparkLevel89Plant.DoseBudget)
            {
                running = false;
                message = "Storm qualification failed. Restore coverage, trusted feedback and essential service before retrying.";
                return;
            }
            if (elapsed < SparkLevel89Plant.StormDuration) return;

            running = false;
            Award();
        }

        private void Award()
        {
            if (stage >= Gates.Length || !loader.ActiveBindings.SetRuntimeComponentBinding(Gates[stage], source)) return;
            stage++;
            manager.QueueEvaluation();
            message = stage switch
            {
                1 => "Network verified. Calibrate the observatory.",
                2 => "Observatory calibrated. Seal the shelter and wait for the water walls and mechanical locks.",
                3 => "Shelter commissioned. Run the first complete storm.",
                4 => "First storm passed. Inject and repair each of the three disturbances, then run another storm.",
                5 => "Recovery storm passed. Arm W149 and remove that wire using Wire Manager.",
                _ => "Helios Storm qualified. All six objectives recorded."
            };
        }

        public void Power()
        {
            running = false;
            source.SetOutput(!source.IsOutputActive, default);
        }

        public void Calibrate()
        {
            if (!NetworkReady || !trusted || !backup || !plant.DetectorsInRange)
            {
                message = "Calibration requires an intact network and fresh, in-range primary and independent readings.";
                return;
            }
            if (stage == 1) Award();
            else if (stage == 5 && witnessed && latched && sampleEpoch > recoveryEpoch)
            {
                recalibrated = true;
                message = "Fresh W149 and independent readings validated. Select RESET SAFETY.";
            }
        }

        public void Seal()
        {
            if (stage < 2 || running || latched || !NetworkReady || !trusted || !backup) return;
            plant.RequestSeal();
            message = "Closing the shelter and transferring water. Check actual coverage and lock feedback.";
        }

        public void Run()
        {
            bool eligible = stage == 3 || stage == 4 && repaired[1] && repaired[2] && repaired[3] ||
                stage == 5 && witnessed && recalibrated && reset;
            if (!eligible || running || fault != 0 || recovery > 0 || !CanCertify)
            {
                message = "Run requires a qualified shelter and completion of this stage's recovery steps.";
                return;
            }
            elapsed = dose = 0f;
            runId++;
            running = true;
        }

        private void Inject(int type)
        {
            if (stage != 4 || fault != 0 || running || recovery > 0 || repaired[type] || !CanCertify) return;
            fault = type;
            faultProven = false;
            faultAge = 0;
            if (type == 2) plant.BeginJam();
            message = "Disturbance active. Observe its measured effect before selecting REPAIR.";
        }

        public void Saturate() => Inject(1);
        public void Jam() => Inject(2);
        public void Surge() => Inject(3);

        public void Repair()
        {
            if (!faultProven || fault == 0 || faultAge < 1f)
            {
                message = "Wait for the measured fault before repairing.";
                return;
            }
            repaired[fault] = true;
            fault = 0;
            recovery = 1.5f;
            message = "Repair applied. Wait for protection to stabilize, then continue.";
        }

        public void Arm()
        {
            if (stage != 5 || !CanCertify || running || witnessed) return;
            armed = true;
            message = "Remove W149 in Wire Manager, then reconnect its highlighted endpoints.";
        }

        public void ResetSafety()
        {
            if (!latched || !recalibrated || !NetworkReady || !trusted || !backup || !plant.Ready) return;
            latched = false;
            reset = true;
            message = "Safety reset accepted. Start a fresh complete storm.";
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
            string safety = latched ? "PROTECTIVE HOLD" : plant.Ready ? "SEALED" : "COMMISSIONING";
            status.text = $"THE HELIOS STORM     {stage}/6  |  {count}/170 WIRES  |  W149 {trust}  |  {safety}";
            measurements.text = $"COVERAGE {plant.Coverage:P0}   LOCK {(plant.Locked ? "ENGAGED" : "OPEN")}   WATER RESERVE {plant.Reserve:F0}   ENERGY {plant.Energy:F0}   HIGH BIN {plant.HighEnergy:F2}   DOSE {dose:F2}/{SparkLevel89Plant.DoseBudget:F1}";
            instructions.text = extra ? "Remove the incorrect wire." : wire >= 0 ?
                $"{endpoints[wire * 2]} → {endpoints[wire * 2 + 1]}  |  {purposes[wire]}" :
                running ? $"STORM {runId}  •  {plant.Phase.ToUpperInvariant()}  •  {elapsed:F1}/{SparkLevel89Plant.StormDuration:F0}s" :
                recovery > 0 ? "Repair applied. Waiting for protection to stabilize." : message;

            var p = plant.Positions;
            var w = plant.Water;
            visuals.SetActualShieldMechanisms(p[0], p[1], p[2], p[3], plant.LockEngagement, plant.DoorClosed);
            visuals.SetActualSubsystemMotion(p[0], p[1], p[2], p[3], plant.Airflow, plant.StormIntensity * 70f);
            visuals.SetActualWaterAndPumps(w[0], w[1], w[2], w[3], plant.PumpRate, plant.PumpRate);
            visuals.SetActualRadiationAndSafety(plant.StormIntensity, plant.Coverage,
                dose / SparkLevel89Plant.DoseBudget, trusted, latched, stage == Gates.Length);
        }
    }
}

