using System.Collections.Generic;
using AAAUI.VFX;
using ProjectSpark.Circuit;
using ProjectSpark.Visuals;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.Gameplay
{
    public sealed class SparkLevel91Process : MonoBehaviour
    {
        public static readonly string[] Gates = { "L91_Network", "L91_Calibrated", "L91_Commissioned", "L91_Endurance", "L91_Repairs", "L91_Requalified" };
        private const int Feedback = 162, Backup = 63;
        public SparkLevelLayout layout;
        public SparkPowerSupply source, primary, secondary;
        public SparkTerminal ground;
        public SparkTerminal[] pins;
        public TextMesh[] labels;
        public string[] endpoints, purposes;
        public Transform arena;
        public SparkLevel91VisualRig visuals;
        public Text status, measurements, instructions;
        private readonly SparkLevel91Plant plant = new();
        private readonly Dictionary<SparkTerminal, int> index = new();
        private readonly List<SparkCircuitConnection> connections = new();
        private readonly bool[] connected = new bool[182];
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
            view.Initialize(arena, arena.Find("L91_HD_VISUAL_ART/L91_CosmicNavigationQualificationDeck"));
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
                witnessed |= armed && count == 181 && !extra;
                latched = true;
                recoveryEpoch = sampleEpoch;
                plant.StopRun();
                armed = running = recalibrated = reset = false;
                message = "W163 removed. Trusted timing lost. Course correction inhibited.";
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
                volts is > 4.6f and < 5.3f;
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
                Drive(primary, Rail(6) && plant.ClockAvailable, EncodeClock(plant.ClockResidual), ref lastPrimary);
                Drive(secondary, Rail(1), EncodeClock(plant.BackupBias), ref lastSecondary);
            }

            trusted = sampleAge < .5f && plant.ClockAvailable && primary.IsOutputActive && ActualSample(Feedback);
            backup = sampleAge < .5f && secondary.IsOutputActive && ActualSample(Backup);
            accumulator += Mathf.Min(Time.deltaTime, .15f) * 2f;
            while (accumulator >= .05f)
            {
                accumulator -= .05f;
                Tick(.05f);
            }

            Present();
        }

        private static float EncodeClock(float bias) => 2.5f + Mathf.Clamp(bias, -.2f, .2f) * 7f;

        private bool NetworkReady => !extra && missing < 0 && Powered();
        private bool TimingPermit => NetworkReady && trusted && backup && !latched && connected[155] && connected[161];

        private void Tick(float step)
        {
            bool permit = TimingPermit;
            plant.Tick(step, Powered(), permit, fault);

            if (stage == 0 && permit) Award();
            if (stage == 1 && permit && plant.Calibrated && plant.Homed) Award();
            if (stage == 2 && permit && plant.ObservationReady) Award();

            if (fault != 0)
            {
                faultAge += step;
                faultProven |= fault switch
                {
                    1 => plant.Background > 1.5f,
                    2 => plant.ClockTemp > .72f && Mathf.Abs(plant.ClockResidual) > .07f,
                    3 => plant.EphemerisError > .065f,
                    _ => false
                };
            }
            if (fault == 0 && plant.Healthy) recovery = Mathf.Max(0f, recovery - step);
            if (!running) return;
            if (!permit || !plant.Healthy)
            {
                running = false;
                plant.StopRun();
                message = "Correction aborted. Restore trusted timing and instrument health.";
                return;
            }

            elapsed += step;
            if (elapsed < SparkLevel91Plant.MissionDuration) return;
            running = false;
            plant.StopRun();
            if (plant.Corrected) Award();
            else message = "Correction missed its navigation or actuation checkpoint. Start a fresh run.";
        }

        private void Award()
        {
            if (stage >= Gates.Length || !loader.ActiveBindings.SetRuntimeComponentBinding(Gates[stage], source)) return;
            stage++;
            manager.QueueEvaluation();
            message = stage switch
            {
                1 => "Network verified. Calibrate the clock and home the four telescopes.",
                2 => "Timing calibrated and telescopes homed. Acquire four pulsar profiles.",
                3 => "Four-pulsar navigation solution verified. Run a fresh protected correction.",
                4 => "Correction qualified. Diagnose and repair all three disturbances, then run another correction.",
                5 => "Recovery qualified. Arm W163 and remove the wire using Wire Manager.",
                _ => "The Cosmic Clock qualified. All six objectives recorded."
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
            if (!NetworkReady || !trusted || !backup)
            {
                message = "Calibration requires fresh primary and independent timing feedback.";
                return;
            }
            if (stage == 1)
            {
                plant.Calibrate();
                message = "Timing calibration recorded. Waiting for measured telescope home positions.";
            }
            else if (latched && sampleEpoch > recoveryEpoch)
            {
                plant.Calibrate();
                recalibrated = true;
                message = "Fresh W163 time reference calibrated. Select RESET SAFETY.";
            }
        }

        public void Acquire()
        {
            if (stage != 2 || !TimingPermit || !plant.Healthy) return;
            plant.BeginObservation(false);
            message = "Acquiring individual photon events and fitting four pulse profiles.";
        }

        public void Run()
        {
            bool eligible = stage == 3 || stage == 4 && repaired[1] && repaired[2] && repaired[3] ||
                stage == 5 && witnessed && recalibrated && reset;
            if (!eligible || running || fault != 0 || recovery > 0f || !TimingPermit || !plant.Healthy)
            {
                message = "Correction needs healthy optics, trusted timing and the current recovery steps.";
                return;
            }
            elapsed = 0f;
            plant.BeginObservation(true);
            runId++;
            running = true;
        }

        private void Inject(int type)
        {
            if (stage != 4 || fault != 0 || running || recovery > 0f || repaired[type] || !TimingPermit) return;
            fault = type;
            faultProven = false;
            faultAge = 0f;
            message = "Disturbance active. Observe the measured change before selecting REPAIR.";
        }

        public void RaiseBackground() => Inject(1);
        public void DriftClock() => Inject(2);
        public void CorruptEphemeris() => Inject(3);

        public void Repair()
        {
            if (!faultProven || fault == 0 || faultAge < 1f)
            {
                message = "Wait for a measured fault signature before repairing.";
                return;
            }
            repaired[fault] = true;
            fault = 0;
            recovery = 1.5f;
            message = "Repair applied. Waiting for timing and optics to settle.";
        }

        public void Arm()
        {
            if (stage != 5 || !TimingPermit || running || witnessed) return;
            armed = true;
            message = "Remove W163 in Wire Manager, then reconnect its highlighted endpoints.";
        }

        public void ResetSafety()
        {
            if (!latched || !recalibrated || !NetworkReady || !trusted || !backup || !plant.Healthy) return;
            latched = false;
            reset = true;
            message = "Safety reset accepted. Run a fresh four-pulsar acquisition and correction.";
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
            string safety = latched ? "BURN INHIBITED" : plant.BurnActive ? "CORRECTION ACTIVE" : "STANDBY";
            status.text = $"THE COSMIC CLOCK  {stage}/6  |  {count}/182 WIRES  |  W163 {trust}  |  BACKUP {(backup ? "LIVE" : "OFFLINE")}  |  {safety}";
            var n = plant.Photons;
            measurements.text = $"PHOTONS {n[0]}/{n[1]}/{n[2]}/{n[3]}   COV {plant.Covariance:F5}   POSITION ERROR {plant.CourseError:F3}   DELTA V {plant.DeltaV:F3}   FUEL {plant.Fuel:F1}";
            instructions.text = extra ? "Remove the incorrect wire." : wire >= 0 ?
                $"{endpoints[wire * 2]} → {endpoints[wire * 2 + 1]}  |  {purposes[wire]}" :
                running ? $"CORRECTION {runId}  |  {plant.Phase}  |  {elapsed:F1}/{SparkLevel91Plant.MissionDuration:F0}s" :
                recovery > 0f ? message :
                fault != 0 ? $"BACKGROUND {plant.Background:F2}   CLOCK TEMP {plant.ClockTemp:F2}   EPHEMERIS ERROR {plant.EphemerisError:F3}  |  {message}" : message;

            var az = plant.Azimuth;
            var el = plant.Elevation;
            var q = plant.Quality;
            float shade = Mathf.InverseLerp(.14f, 2f, plant.Background);
            visuals.SetActualTelescopeAngles(az[0], el[0], az[1], el[1], az[2], el[2], az[3], el[3]);
            visuals.SetActualAcquisition(q[0], q[1], q[2], q[3], shade, shade, shade, shade);
            visuals.SetActualTimingAndNavigation(1f - Mathf.Clamp01(Mathf.Abs(plant.ClockResidual) * 5f),
                plant.ObservationReady ? 1f - Mathf.Clamp01(plant.Covariance * 600f) : 0f,
                plant.Cooling, trusted, latched, stage == Gates.Length);
        }
    }
}

