using System.Collections.Generic;
using AAAUI.VFX;
using ProjectSpark.Circuit;
using ProjectSpark.Level93.Visuals;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.Gameplay
{
    public sealed class SparkLevel93Process : MonoBehaviour
    {
        public static readonly string[] Gates = { "L93_Network", "L93_Calibrated", "L93_Commissioned", "L93_Endurance", "L93_Repairs", "L93_Requalified" };
        private const int Feedback = 178, Backup = 171;
        public SparkLevelLayout layout;
        public SparkPowerSupply source, primary, secondary;
        public SparkTerminal ground;
        public SparkTerminal[] pins;
        public TextMesh[] labels;
        public string[] endpoints, purposes;
        public Transform arena;
        public SparkLevel93VisualRig visuals;
        public Text status, measurements, instructions;
        private readonly SparkLevel93Plant plant = new();
        private readonly Dictionary<SparkTerminal, int> index = new();
        private readonly List<SparkCircuitConnection> connections = new();
        private readonly bool[] connected = new bool[194];
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
            view.Initialize(arena, arena.Find("L93_HD_VISUAL_ART/L93_Aurora1ScienceLaboratoryDeck"));
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
                witnessed |= armed && count == 193 && !extra;
                latched = true;
                recoveryEpoch = sampleEpoch;
                plant.StopRun();
                armed = running = recalibrated = reset = false;
                message = "W179 removed. Trusted chamber pressure lost. Heating inhibited.";
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
                Drive(primary, Rail(6), EncodePressure(plant.Pressure), ref lastPrimary);
                Drive(secondary, Rail(1), EncodePressure(plant.BackupPressure), ref lastSecondary);
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

        private static float EncodePressure(float pressure) => 1f + Mathf.Clamp01(pressure) * 3f;

        private bool NetworkReady => !extra && missing < 0 && Powered();
        private bool AnalysisPermit => NetworkReady && trusted && backup && !latched && connected[156] && connected[177];

        private void Tick(float step)
        {
            bool permit = AnalysisPermit;
            plant.Tick(step, Powered(), permit, fault);
            if (stage == 0 && permit) Award();
            if (stage == 1 && permit && plant.Calibrated) Award();
            if (stage == 2 && permit && plant.Mapped) Award();

            if (fault != 0)
            {
                faultAge += step;
                faultProven |= fault switch
                {
                    1 => plant.Contamination > .55f,
                    2 => Mathf.Abs(plant.EncoderError) > 18f,
                    3 => Mathf.Abs(plant.Gain - 1f) > .3f,
                    _ => false
                };
            }
            if (fault == 0 && plant.Calibrated && plant.HeaterOff)
                recovery = Mathf.Max(0f, recovery - step);

            if (!running) return;
            elapsed += step;
            if (!permit || !plant.Calibrated || elapsed > 22f)
            {
                running = false;
                plant.StopRun();
                message = "Analysis aborted. Verify custody, purge, calibration and chamber feedback.";
            }
            else if (plant.Qualified)
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
                1 => "Network verified. Commission inert handling and calibrate the lab.",
                2 => "Laboratory qualified. Map the mineral archive.",
                3 => "Projection and spectral measurements agree. Analyze a fresh fragment.",
                4 => "Chemistry qualified. Diagnose all three faults and repeat the complete analysis.",
                5 => "Recovery qualified. Arm W179, then remove it in Wire Manager.",
                _ => "The Elemental Archive qualified. All six objectives recorded."
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
                plant.Commission();
                message = "Transferring the core into inert custody and closing the vacuum chamber.";
            }
            else if (latched && sampleEpoch > recoveryEpoch && plant.Calibrated)
            {
                recalibrated = true;
                message = "Fresh chamber pressure validated against independent protection. Select RESET SAFETY.";
            }
        }

        public void Map()
        {
            if (stage != 2 || !AnalysisPermit || !plant.Calibrated) return;
            plant.BeginMap();
            message = "Acquiring CT projections and comparing spectral measurements.";
        }

        public void Run()
        {
            bool eligible = stage == 3 || stage == 4 && repaired[1] && repaired[2] && repaired[3] ||
                stage == 5 && witnessed && recalibrated && reset;
            if (!eligible || running || fault != 0 || recovery > 0f || !AnalysisPermit || !plant.Calibrated || !plant.HeaterOff)
            {
                message = "Analysis requires valid lab calibration, trusted pressure and an idle heater.";
                return;
            }
            if (!plant.BeginAnalysis())
            {
                message = "Archive reserve reached. Restart the mission to use a fresh specimen.";
                return;
            }
            elapsed = 0f;
            runId++;
            running = true;
        }

        private void Inject(int type)
        {
            if (stage != 4 || fault != 0 || running || recovery > 0f || repaired[type] || !AnalysisPermit) return;
            fault = type;
            faultProven = false;
            faultAge = 0f;
            message = "Disturbance active. Confirm its measured signature before selecting REPAIR.";
        }

        public void FailPurge() => Inject(1);
        public void SlipEncoder() => Inject(2);
        public void DriftGain() => Inject(3);

        public void Repair()
        {
            if (!faultProven || fault == 0 || faultAge < 1f) return;
            repaired[fault] = true;
            fault = 0;
            recovery = 1.5f;
            message = "Repair applied. Waiting for purge, encoder and gain calibration to settle.";
        }

        public void Arm()
        {
            if (stage != 5 || !AnalysisPermit || running || witnessed) return;
            armed = true;
            message = "Remove W179 in Wire Manager, then reconnect its highlighted endpoints.";
        }

        public void ResetSafety()
        {
            if (!latched || !recalibrated || !NetworkReady || !trusted || !backup || !plant.Calibrated || !plant.HeaterOff) return;
            latched = false;
            reset = true;
            message = "Safety reset accepted. Map and analyze a fresh fragment.";
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
            string safety = latched ? "HEAT INHIBITED" : plant.HeaterOff ? "STANDBY" : "HEATER ACTIVE";
            status.text = $"ELEMENTAL ARCHIVE  {stage}/6  |  {count}/194 WIRES  |  W179 {trust}  |  BACKUP {(backup ? "LIVE" : "OFFLINE")}  |  {safety}";
            measurements.text = $"CT {plant.Coverage:P0}   UNCERTAINTY {plant.Uncertainty:F3}   SPECTRAL ERROR {plant.SpectralError:F3}   PRESSURE {plant.Pressure:F3}   RESERVE {plant.Reserve:P0}";
            instructions.text = extra ? "Remove the incorrect wire." : wire >= 0 ?
                $"{endpoints[wire * 2]} → {endpoints[wire * 2 + 1]}  |  {purposes[wire]}" :
                running ? $"ANALYSIS {runId}  |  {(plant.Mapped ? "THERMAL / MASS ANALYSIS" : "PROJECTION / SPECTRAL SCAN")}  |  HEAT {plant.Heat:F2}  |  SAMPLE USED {plant.Consumed:P0}" :
                fault != 0 ? $"CONTAMINATION {plant.Contamination:F2}  |  ENCODER ERROR {plant.EncoderError:F0}°  |  DETECTOR GAIN {plant.Gain:F2}" : message;

            visuals.SetActualAngleDegrees("ANIM_CT_ROTATING_BORE", plant.Angle);
            visuals.SetActualAngleDegrees("ANIM_CT_STAGE_ENCODER", plant.Angle + plant.EncoderError);
            visuals.SetActualAngleDegrees("ANIM_MASS_DETECTOR_GAIN", (plant.Gain - 1f) * 90f);
            visuals.SetActualAngleDegrees("ANIM_CLEAN_TRANSFER_ARM", plant.Transfer * 65f);
            visuals.SetActualAngleDegrees("ANIM_THERMAL_CONTROL_READOUT", plant.Heat * 85f);
            visuals.SetActualAngleDegrees("ANIM_MISSION_BEACON", stage == Gates.Length ? 60f : 0f);
        }
    }
}


