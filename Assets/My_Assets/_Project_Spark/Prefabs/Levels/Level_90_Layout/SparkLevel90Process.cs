using System.Collections.Generic;
using AAAUI.VFX;
using ProjectSpark.Circuit;
using ProjectSpark.Visuals;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.Gameplay
{
    public sealed class SparkLevel90Process : MonoBehaviour
    {
        public static readonly string[] Gates = { "L90_Network", "L90_Calibrated", "L90_Commissioned", "L90_Endurance", "L90_Repairs", "L90_Requalified" };
        private const int Feedback = 156, Backup = 138;
        public SparkLevelLayout layout;
        public SparkPowerSupply source, primary, secondary;
        public SparkTerminal ground;
        public SparkTerminal[] pins;
        public TextMesh[] labels;
        public string[] endpoints, purposes;
        public Transform arena;
        public SparkLevel90VisualRig visuals;
        public Text status, measurements, instructions;
        private readonly SparkLevel90Plant plant = new();
        private readonly Dictionary<SparkTerminal, int> index = new();
        private readonly List<SparkCircuitConnection> connections = new();
        private readonly bool[] connected = new bool[176];
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
            view.Initialize(arena, arena.Find("L90_HD_VISUAL_ART/L90_OrbitalPropulsionQualificationDeck"));
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

            if (!connected[Feedback] && (armed || stage >= 3 && trusted))
            {
                witnessed |= armed && count == 175 && !extra;
                latched = true;
                recoveryEpoch = sampleEpoch;
                plant.StopIgnition();
                armed = running = recalibrated = reset = false;
                message = "W157 removed. Neutralizer feedback invalid. Discharge is inhibited.";
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
                Drive(primary, Rail(6) && plant.NeutralizerAvailable, EncodeCurrent(plant.NeutralizerCurrent), ref lastPrimary);
                Drive(secondary, Rail(0), EncodeCurrent(plant.IndependentCurrent), ref lastSecondary);
            }

            trusted = sampleAge < .5f && plant.NeutralizerAvailable && primary.IsOutputActive && ActualSample(Feedback);
            backup = sampleAge < .5f && secondary.IsOutputActive && ActualSample(Backup);
            accumulator += Mathf.Min(Time.deltaTime, .15f) * 2f;
            while (accumulator >= .05f)
            {
                accumulator -= .05f;
                Tick(.05f);
            }

            Present();
        }

        private static float EncodeCurrent(float current) => .8f + Mathf.Clamp01(current) * 3.2f;

        private bool NetworkReady => !extra && missing < 0 && Powered();
        private bool CanCertify => NetworkReady && trusted && backup && !latched && plant.Stable;

        private void Tick(float step)
        {
            bool controls = Powered() && connected[141] && connected[142] && connected[158];
            bool permit = NetworkReady && controls && trusted && backup && !latched;
            if (running) elapsed += step;
            plant.Tick(step, controls, permit, running, fault);

            if (stage == 0 && permit) Award();
            if (stage == 1 && plant.Calibrated && plant.Commissioned) Award();
            if (stage == 2 && plant.Stable) Award();

            if (fault != 0)
            {
                faultAge += step;
                faultProven |= fault switch
                {
                    1 => plant.Flow[0] < .35f && plant.Discharge[0] < .4f,
                    2 => plant.Heat[1] > .82f && plant.Discharge[1] < .4f,
                    3 => Mathf.Abs(plant.Yaw[2]) > 7f,
                    _ => false
                };
            }
            if (fault == 0 && plant.Stable) recovery = Mathf.Max(0, recovery - step);
            if (!running) return;
            if (!CanCertify)
            {
                running = false;
                message = "Departure aborted. Restore power, flow, pointing and safety authorization.";
                return;
            }
            if (elapsed < SparkLevel90Plant.DepartureDuration) return;
            running = false;
            if (plant.DeltaV >= SparkLevel90Plant.TargetDeltaV && plant.Separation >= 12f) Award();
            else message = "Departure missed its velocity or separation checkpoint. Recheck propulsion and retry.";
        }

        private void Award()
        {
            if (stage >= Gates.Length || !loader.ActiveBindings.SetRuntimeComponentBinding(Gates[stage], source)) return;
            stage++;
            manager.QueueEvaluation();
            message = stage switch
            {
                1 => "Network verified. Calibrate xenon flow and precharge the four PPUs.",
                2 => "Xenon and PPUs commissioned. Ignite the four thrusters.",
                3 => "Plasma stable on all four channels. Run the first departure.",
                4 => "Departure passed. Diagnose and repair the three disturbances, then run another departure.",
                5 => "Recovery departure passed. Arm W157 and remove that wire using Wire Manager.",
                _ => "The Long Burn qualified. All six objectives recorded."
            };
        }

        public void Power()
        {
            running = false;
            source.SetOutput(!source.IsOutputActive, default);
        }

        public void Calibrate()
        {
            if (!NetworkReady || !trusted || !backup || !plant.NeutralizerAvailable)
            {
                message = "Calibration requires an intact network and fresh, in-range primary and independent readings.";
                return;
            }
            if (stage == 1) { plant.Calibrate(); message = "Calibration accepted. Verifying regulator flow and protected precharge."; }
            else if (latched && sampleEpoch > recoveryEpoch)
            {
                recalibrated = true;
                message = "Fresh W157 and independent readings validated. Select RESET SAFETY.";
            }
        }

        public void Ignite()
        {
            if (stage < 2 || running || latched || !NetworkReady || !trusted || !backup) return;
            plant.RequestIgnition();
            message = "Ignition requested. Verify measured discharge and neutralization.";
        }

        public void Run()
        {
            bool eligible = stage == 3 || stage == 4 && repaired[1] && repaired[2] && repaired[3] ||
                stage == 5 && witnessed && recalibrated && reset;
            if (!eligible || running || fault != 0 || recovery > 0 || !CanCertify)
            {
                message = "Departure requires stable discharge on all four thrusters and completion of this stage's recovery steps.";
                return;
            }
            elapsed = 0f;
            plant.BeginDeparture();
            runId++;
            running = true;
        }

        private void Inject(int type)
        {
            if (stage != 4 || fault != 0 || running || recovery > 0 || repaired[type] || !CanCertify) return;
            fault = type;
            faultProven = false;
            faultAge = 0;

            message = "Disturbance active. Observe its measured effect before selecting REPAIR.";
        }

        public void RestrictFlow() => Inject(1);
        public void Overheat() => Inject(2);
        public void OffsetVector() => Inject(3);

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
            message = "Remove W157 in Wire Manager, then reconnect its highlighted endpoints.";
        }

        public void ResetSafety()
        {
            if (!latched || !recalibrated || !NetworkReady || !trusted || !backup || !plant.Commissioned || !plant.DischargeIsOff) return;
            latched = false;
            reset = true;
            message = "Safety reset accepted. Ignite, stabilize and start a fresh departure.";
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
            string safety = latched ? "DISCHARGE INHIBITED" : plant.Stable ? "PLASMA STABLE" : "STANDBY";
            status.text = $"THE LONG BURN  {stage}/6  |  {count}/176 WIRES  |  W157 {trust}  |  BACKUP {(backup ? "LIVE" : "OFFLINE")}  |  {safety}";
            measurements.text = $"XENON A/B {plant.Tanks[0]:F1}/{plant.Tanks[1]:F1}   ENERGY {plant.Energy:F0}   THRUST {plant.Thrust:F2}   DELTA V {plant.DeltaV:F2}   SEPARATION {plant.Separation:F1}";
            instructions.text = extra ? "Remove the incorrect wire." : wire >= 0 ?
                $"{endpoints[wire * 2]} → {endpoints[wire * 2 + 1]}  |  {purposes[wire]}" :
                running ? $"DEPARTURE {runId}  |  {plant.Phase}  |  {elapsed:F1}/{SparkLevel90Plant.DepartureDuration:F0}s" :
                recovery > 0 ? "Repair applied. Waiting for flow, heat and pointing to stabilize." :
                fault != 0 ? $"A FLOW {plant.Flow[0]:F2}  |  B COIL HEAT {plant.Heat[1]:F2}  |  C YAW {plant.Yaw[2]:F1}°  |  {message}" : message;
            var d = plant.Discharge;
            var p = plant.Precharge;
            var y = plant.Yaw;
            visuals.SetActualEngineStates(d[0], d[1], d[2], d[3], p[0], p[1], p[2], p[3]);
            visuals.SetActualGimbals(y[0], 0f, y[1], 0f, y[2], 0f, y[3], 0f);
            visuals.SetActualPropellantAndMechanisms(plant.Tanks[0] / 48f, plant.Tanks[1] / 48f,
                plant.Solar, plant.Solar, plant.Coolant);
            visuals.SetActualMissionAndSafety(plant.Separation / 20f, plant.DeltaV / SparkLevel90Plant.TargetDeltaV,
                trusted, latched, stage == Gates.Length);
        }
    }
}

