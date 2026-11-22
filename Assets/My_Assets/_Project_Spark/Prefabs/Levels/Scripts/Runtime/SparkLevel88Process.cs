using System.Collections.Generic;
using AAAUI.VFX;
using ProjectSpark.Circuit;
using ProjectSpark.Visuals;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.Gameplay
{
    public sealed class SparkLevel88Process : MonoBehaviour
    {
        public static readonly string[] Gates = { "L88_Network", "L88_Calibrated", "L88_Commissioned", "L88_Endurance", "L88_Repairs", "L88_Requalified" };
        private const int Feedback = 144, Backup = 126;
        public SparkLevelLayout layout;
        public SparkPowerSupply source, primary, secondary;
        public SparkTerminal ground;
        public SparkTerminal[] pins;
        public TextMesh[] labels;
        public string[] endpoints, purposes;
        public Transform arena;
        public SparkLevel88VisualRig visuals;
        public Text status, measurements, instructions;
        private readonly SparkLevel88Atmosphere plant = new();
        private readonly Dictionary<SparkTerminal, int> index = new();
        private readonly List<SparkCircuitConnection> connections = new();
        private readonly bool[] connected = new bool[164];
        private readonly bool[] repaired = new bool[4];
        private LevelGamePlayManager manager;
        private SparkLevelLayoutLoader loader;
        private SignalWireBuilder wireTool;
        private SparkLevel88ArenaView view;
        private int version = -1, missing = -1, count, stage, fault, runId;
        private int commissionedBeds;
        private bool extra, trusted, backup, latched, armed, witnessed, recalibrated, reset, running, faultProven;
        private float elapsed, stable, accumulator, faultAge, recovery;
        private float lastPrimary = -1, lastSecondary = -1, sensorTime;
        private string message = "Connect the highlighted endpoints.";
        private GUIStyle terminalCaption;

        private void Start()
        {
            manager = FindFirstObjectByType<LevelGamePlayManager>();
            loader = FindFirstObjectByType<SparkLevelLayoutLoader>();
            wireTool = FindFirstObjectByType<SignalWireBuilder>();
            for (int i = 0; i < pins.Length; i++) index.Add(pins[i], i);
            view = gameObject.AddComponent<SparkLevel88ArenaView>();
            view.Initialize(arena);
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

            if (armed && !connected[Feedback] && count == 163 && !extra)
            {
                latched = witnessed = true;
                armed = running = recalibrated = reset = false;
                message = "W145 removed. Primary feedback invalid. Backup protection is monitoring.";
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
            sensorTime -= Time.deltaTime;
            if (sensorTime <= 0)
            {
                sensorTime = .24f;
                float pressure = plant.Zones[1].Pressure;
                Drive(primary, Rail(7), Mathf.Clamp(2.5f + (pressure - 99)*.12f,.7f,4.3f), ref lastPrimary);
                Drive(secondary, Rail(8), Mathf.Clamp(2.5f + (pressure - 99)*.1f,.7f,4.3f), ref lastSecondary);
            }
            trusted = primary.IsOutputActive && ActualSample(Feedback);
            backup = secondary.IsOutputActive && ActualSample(Backup) && connected[125];
            accumulator += Mathf.Min(Time.deltaTime,.15f) * 2.5f;
            while (accumulator >= .05f)
            {
                accumulator -= .05f;
                Tick(.05f);
            }
            Present();
        }

        private void Tick(float step)
        {
            bool electrical = !extra && missing < 0 && Powered();
            if (stage == 0 && electrical && trusted && backup) Award();
            if (stage == 0) return;
            bool support = Powered() && (connected[102] && connected[103]) && stage >= 1;
            bool regulation = stage >= 2 && electrical && trusted && !latched;
            bool commissioning = regulation && (stage == 2 || running || fault != 0 || recovery > 0 || !plant.Healthy);
            float demand = !running ? .25f : elapsed < 8 ? .7f : elapsed < 18 ? 1.8f : .9f;
            plant.Tick(step, demand, support, commissioning, commissioning, commissioning,
                regulation && connected[122], fault);

            if (stage == 2 && commissioning && plant.Ventilated && plant.OxygenFlow > .08f && plant.Condenser > .9f)
            {
                commissionedBeds |= 1 << plant.ActiveBed;
                if (commissionedBeds == 3 && plant.Hydrogen > .1f && plant.CapturedCarbon > .01f) Award();
            }

            if (fault != 0)
            {
                faultAge += step;
                faultProven |= fault switch
                {
                    1 => plant.Zones[0].CarbonPressure > .52f,
                    2 => plant.Condenser < .05f,
                    3 => plant.Zones[1].Pressure < 97.5f,
                    _ => false
                };
            }
            if (recovery > 0 && plant.Healthy && fault == 0) recovery = Mathf.Max(0,recovery-step);

            if (!running) return;
            elapsed += step;
            if (electrical && trusted && !latched && plant.Healthy && plant.Ventilated) stable += step;
            if (elapsed < 27) return;
            running = false;
            if (stable < 26.8f || plant.Water <= 1 || plant.Energy <= 1)
            {
                message = "Run failed. Check atmosphere, airflow and reserves before retrying.";
                return;
            }
            if (stage == 3 || stage == 4 && repaired[1] && repaired[2] && repaired[3] ||
                stage == 5 && witnessed && recalibrated && reset) Award();
            else message = "Endurance passed. Complete the remaining recovery steps.";
        }

        private void Award()
        {
            if (stage >= Gates.Length || !loader.ActiveBindings.SetRuntimeComponentBinding(Gates[stage],source)) return;
            stage++;
            manager.QueueEvaluation();
            message = $"Objective {stage}/6 recorded.";
        }

        public void Power()
        {
            if (running) running = false;
            source.SetOutput(!source.IsOutputActive,default);
        }

        public void Calibrate()
        {
            if (missing >= 0 || extra || !Powered() || !trusted || !backup || !plant.Ventilated || !plant.Healthy)
            {
                message = "Calibration requires valid primary and backup feedback, a healthy atmosphere and steady airflow.";
                return;
            }
            if (stage == 1) Award();
            else if (latched && witnessed)
            {
                recalibrated = true;
                message = "Fresh pressure sample validated. Select RESET SAFETY.";
            }
        }

        public bool CanRun => stage is >= 3 and < 6 && !running && !armed && !latched && fault == 0 &&
            recovery <= 0 && trusted && missing < 0 && !extra && plant.Healthy && plant.Ventilated;

        public void Run()
        {
            if (!CanRun)
            {
                message = "Run requires all wiring, a healthy atmosphere, settled repairs and valid feedback.";
                return;
            }
            elapsed = stable = 0;
            runId++;
            running = true;
            message = "Endurance running: Low demand, peak demand, then recovery.";
        }

        private void Inject(int type)
        {
            if (stage != 4 || fault != 0 || running || recovery > 0 || repaired[type]) return;
            fault = type;
            faultProven = false;
            faultAge = 0;
            message = "Disturbance active. Watch the measured response, then select REPAIR.";
        }
        public void Breakthrough() => Inject(1);
        public void Freeze() => Inject(2);
        public void Leak() => Inject(3);
        public void Repair()
        {
            if (!faultProven || fault == 0 || faultAge < 1) { message = "Wait for a measurable fault before repairing."; return; }
            repaired[fault] = true;
            fault = 0;
            recovery = 2;
            message = "Repair made. Wait for the atmosphere to recover, then run endurance.";
        }
        public void Arm()
        {
            if (stage != 5 || !trusted || !backup || running || witnessed) return;
            armed = true;
            message = "Use Wire Manager to physically remove W145, then reconnect its two highlighted endpoints.";
        }
        public void ResetSafety()
        {
            if (!latched || !recalibrated || !trusted || !backup || !plant.Healthy || missing >= 0) return;
            latched = false;
            reset = true;
            message = "Pressure control restored. Start a fresh endurance run.";
        }
        public void ArenaView() { if (view) view.UseOverview(); }
        public void SocketView()
        {
            if (!view) return;
            int route = armed ? Feedback : missing;
            if (route < 0) view.UseSocketView();
            else view.UseSocketView(pins[route*2].transform.position, pins[route*2+1].transform.position);
        }
        public void ShowcaseView() { if (view) view.UseShowcaseView(); }

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

            void Marker(int index,Color color)
            {
                Vector3 p = camera.WorldToScreenPoint(pins[index].transform.position);
                if (p.z <= 0 || p.x < 0 || p.x > Screen.width || p.y < 0 || p.y > Screen.height) return;
                float y = Screen.height-p.y;
                GUI.color = color;
                GUI.DrawTexture(new Rect(p.x-10,y-10,20,2),Texture2D.whiteTexture);
                GUI.DrawTexture(new Rect(p.x-10,y+8,20,2),Texture2D.whiteTexture);
                GUI.DrawTexture(new Rect(p.x-10,y-10,2,20),Texture2D.whiteTexture);
                GUI.DrawTexture(new Rect(p.x+8,y-10,2,20),Texture2D.whiteTexture);
                float captionX = p.x + ((index & 1) == 0 ? -37 : 37);
                var caption = new Rect(captionX-45,y-36,90,22);
                GUI.color = new Color(.035f,.085f,.13f,.96f);
                GUI.DrawTexture(caption,Texture2D.whiteTexture);
                GUI.color = color;
                GUI.Label(caption,labels[index].text,terminalCaption);
            }

            if (route >= 0)
            {
                Marker(route*2,new Color(1,.68f,.25f));
                Marker(route*2+1,new Color(.25f,.88f,1));
            }
            if (wireTool && wireTool.HoveredTerminal && index.TryGetValue(wireTool.HoveredTerminal,out int hovered) &&
                hovered/2 != route && !SignalWireBuilder.AnyWireDrawing)
                Marker(hovered,Color.white);
            GUI.color = previous;
        }
        private void Present()
        {
            int wire = armed ? Feedback : missing;

            status.text = $"THE BREATHING RING   •   {stage}/6   |   {count}/164 WIRES   |   W145 {(trusted ? "LIVE" : "INVALID")}   |   BACKUP {(backup ? "LIVE" : "OFFLINE")}";
            var z = plant.Zones[1];
            measurements.text = $"PRESSURE A/B/C/D  {plant.Zones[0].Pressure:F1} / {z.Pressure:F1} / {plant.Zones[2].Pressure:F1} / {plant.Zones[3].Pressure:F1}   |   B: O₂ {z.OxygenPressure:F1}  CO₂ {z.CarbonPressure:F2}  H₂O {z.Vapor:F2}  T {z.Temperature:F1}   |   WATER {plant.Water:F0}  ENERGY {plant.Energy:F0}";
            instructions.text = extra ? "Remove the incorrect wire." : wire >= 0 ?
                $"{endpoints[wire*2]} → {endpoints[wire*2+1]}   |   {purposes[wire]}" :
                running ? $"RUN {runId}: {elapsed:F1}/27 s   |   Sustained healthy atmosphere {stable:F1} s" : message;

            visuals.SetActualVentilation(plant.Fan,0,plant.Dampers[0],plant.Dampers[1],plant.Dampers[2],plant.Dampers[3]);
            visuals.SetActualCO2Processing(plant.Beds[0]/1.6f,plant.Beds[1]/1.6f,plant.ActiveBed*90);
            visuals.SetActualOxygenProcess(plant.OxygenFlow,plant.OxygenFlow*90);
            visuals.SetActualWaterAndHumidity(plant.Condenser,plant.Condenser,0);
            visuals.SetActualSafety(trusted,latched,plant.Healthy && trusted,z.OxygenPressure < 18,false,plant.Regulator*90);
        }
    }
}




