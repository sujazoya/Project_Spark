using System;
using System.Collections.Generic;
using AAAUI.VFX;
using ProjectSpark.UI.VisualTeaching;
using ProjectSpark.Circuit;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.Gameplay
{
    public sealed partial class SparkLevel94Process : MonoBehaviour, ISparkAILevelTeachingFocus
    {
        public static readonly string[] Gates = {"L94_Inspect","L94_Assembly","L94_Copper","L94_Commission","L94_Repair","L94_Recovery"};

        [Serializable] public sealed class Route
        {
            public SparkTerminal from, to, viaTop, viaBottom;
            public SparkSwitch via;
            public LineRenderer copper, backCopper;
            public string net, viaName;

        }

        public SparkLevelLayout layout;
        public Transform arena, board;
        public GameObject[] parts;
        public string[] references, values;
        public SparkTerminal supplyReturn, protectedInput, rail, feedbackInput, shunt;
        public SparkPowerSupply supply, regulator, feedback, certification;
        public SparkSwitch loadGate, bridge;
        public SparkResistor pullup;
        public Route[] routes;
        public GameObject jumperArt;
        public SparkLevel94Effects effects;
        public Text heading, detail, readings;
        private LevelGamePlayManager manager;
        private SparkLevelLayoutLoader loader;
        private SparkLevel94BoardView cameraView;
        private SignalWireBuilder wireBuilder;
        private GUIStyle padLabel;
        private SparkElectricalComponent[][] assemblyDevices;
        private int partPick;
        private bool fitted, backLayer, calibrated, hasShuntSample, followRoute, focusPending;
        private float railVoltage, feedbackVoltage, inputVoltage;
        private const int CopperCount = 34;
        private int JumperIndex => routes.Length - 1;
        private readonly List<SparkCircuitConnection> edges = new();
        private readonly HashSet<int> inspected = new();
        private readonly bool[] repairs = new bool[3];
        private int stage, part, orientation, route, fault = -1, routed, run;
        private bool armed, lost, reset, measured, running, validRail, trusted, load;
        private float liveFor, shuntVoltage, lastShunt, clock;
        private string notice = "Inspect the nets, then fit the board components.";

        private bool Active => loader && manager && loader.ActiveLayout == layout &&
            manager.ActiveLevel == layout.LevelDefinition && manager.CircuitSystem && manager.ElectricalSolver;

        private void Start()
        {
            loader = FindFirstObjectByType<SparkLevelLayoutLoader>();
            manager = FindFirstObjectByType<LevelGamePlayManager>();
            cameraView = gameObject.AddComponent<SparkLevel94BoardView>();
            cameraView.Initialize(arena);
            BoardView();
            wireBuilder = FindFirstObjectByType<SignalWireBuilder>();
            if (wireBuilder)
            {
                wireBuilder.TerminalPickerOverride = PickPad;
                wireBuilder.TerminalWireConnected += OnManualWire;
            }
            PrepareAssembly();
            float copperWidth = Mathf.Clamp(arena.lossyScale.x * .0035f, .012f, .038f);
            foreach (var item in routes)
            {
                item.copper.widthMultiplier = copperWidth;
                if (item.backCopper) item.backCopper.widthMultiplier = copperWidth * .88f;
            }
            BuildConsole();
            BoardView();
        }

        private void PrepareAssembly()
        {
            var topology = arena.Find("Board topology");
            var devices = topology.GetComponentsInChildren<SparkElectricalComponent>(true);
            assemblyDevices = Array.ConvertAll(references, reference => Array.FindAll(devices, device =>
                device.name == reference || device.name.StartsWith(reference + " ", StringComparison.Ordinal)));
            for (int i = 0; i < parts.Length; i++) SetInstalled(i, false);
        }

        private void SetInstalled(int index, bool installed)
        {
            parts[index].SetActive(installed);
            foreach (var device in assemblyDevices[index]) device.SetElectricalEnabled(installed);
            if (references[index] == "U2") regulator.SetElectricalEnabled(installed);
            if (references[index] == "U4") feedback.SetElectricalEnabled(installed);
            if (references[index] == "Q1")
            {
                loadGate.SetElectricalEnabled(installed);
                bridge.SetElectricalEnabled(installed);
            }
        }

        private int InstalledCount => Array.FindAll(parts, item => item.activeSelf).Length;

        public bool TryGetTeachingFocus(out Transform target, out string instruction)
        {
            instruction = notice;
            target = null;
            if (!Active || stage == Gates.Length) return false;
            if (stage == 1)
            {
                target = PartAnchor(part);
                return target;
            }
            if (PendingPads(out var pad, out _)) target = pad.transform;
            else if (stage is 0 or 2) target = routes[route].from.transform;
            else target = (stage >= 5 ? routes[JumperIndex].from : rail).transform;
            return target;
        }

        private Transform PartAnchor(int index)
        {
            var pivot = Array.Find(parts[index].GetComponentsInChildren<Transform>(true),
                candidate => candidate.name.StartsWith("ANIM_", StringComparison.Ordinal));
            if (pivot && pivot.gameObject.activeInHierarchy) return pivot;
            var terminal = Array.Find(arena.GetComponentsInChildren<SparkTerminal>(),
                candidate => candidate.name.StartsWith(references[index] + ".", StringComparison.Ordinal));
            return terminal ? terminal.transform : board;
        }

        private void OnDisable()
        {
            FlushCheckpoint();
            RestoreGeneralCoach();
            if (!wireBuilder) return;
            wireBuilder.TerminalWireConnected -= OnManualWire;
            if (wireBuilder.TerminalPickerOverride != PickPad) return;
            if (wireBuilder.IsDrawing) wireBuilder.CancelWire();
            wireBuilder.TerminalPickerOverride = null;
        }

        private bool CanRoute => Active && stage is >= 2 and < 6 &&
            (!supply.IsOutputActive || route == JumperIndex);

        private bool NeedsBackLayer => routes[route].via && Edge(routes[route].from, routes[route].viaTop);

        private bool PendingPads(out SparkTerminal a, out SparkTerminal b)
        {
            a = null;
            b = null;
            if (!CanRoute || backLayer != NeedsBackLayer) return false;
            var item = routes[route];
            if (!item.via)
            {
                a = item.from;
                b = item.to;
            }
            else if (!item.via.IsClosed)
            {
                return false;
            }
            else if (!Edge(item.from, item.viaTop))
            {
                a = item.from;
                b = item.viaTop;
            }
            else
            {
                a = item.viaBottom;
                b = item.to;
            }
            return !Edge(a, b);
        }

        private SparkTerminal PickPad(Vector2 pointer, SparkTerminal start)
        {
            var camera = Camera.main;
            if (!camera || !PendingPads(out var a, out var b)) return null;
            var pointA = camera.WorldToScreenPoint(a.transform.position);
            var pointB = camera.WorldToScreenPoint(b.transform.position);
            float radius = Mathf.Clamp(Screen.height * .017f, 12f, 26f);
            float distanceA = pointA.z > 0 ? ((Vector2)pointA - pointer).sqrMagnitude : float.PositiveInfinity;
            float distanceB = pointB.z > 0 ? ((Vector2)pointB - pointer).sqrMagnitude : float.PositiveInfinity;
            if (Mathf.Min(distanceA, distanceB) > radius * radius) return null;
            var terminal = distanceA <= distanceB ? a : b;
            return terminal == start ? null : terminal;
        }

        private void OnGUI()
        {
            if (ProjectSpark.UIManagement.SparkUIManager.SuppressWorkbenchAnnotations) return;
            if (Event.current.type != EventType.Repaint || !PendingPads(out var a, out var b)) return;
            var camera = Camera.main;
            if (!camera) return;
            padLabel ??= new GUIStyle(GUI.skin.label)
                { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold, fontSize = 15 };
            var color = GUI.color;
            DrawPad(camera, a, new Color(1f, .74f, .29f), -38f);
            DrawPad(camera, b, new Color(.25f, .9f, 1f), 38f);
            GUI.color = color;
        }

        private void DrawPad(Camera camera, SparkTerminal terminal, Color color, float captionOffset)
        {
            var point = camera.WorldToScreenPoint(terminal.transform.position);
            if (point.z <= 0f || point.x < 0 || point.x > Screen.width || point.y < 0 || point.y > Screen.height) return;
            float y = Screen.height - point.y;
            var texture = Texture2D.whiteTexture;
            GUI.color = color;
            GUI.DrawTexture(new Rect(point.x - 8, y - 8, 16, 2), texture);
            GUI.DrawTexture(new Rect(point.x - 8, y + 6, 16, 2), texture);
            GUI.DrawTexture(new Rect(point.x - 8, y - 8, 2, 16), texture);
            GUI.DrawTexture(new Rect(point.x + 6, y - 8, 2, 16), texture);
            var caption = new Rect(point.x + captionOffset - 64, y - 32, 128, 22);
            GUI.color = new Color(.025f, .063f, .09f, .95f);
            GUI.DrawTexture(caption, texture);
            GUI.color = color;
            GUI.Label(caption, terminal.name, padLabel);
        }

        private void OnManualWire(SparkTerminal from, SparkTerminal to, SignalPath path)
        {
            if (!CanRoute) return;
            // The gesture creates native topology. Its final visual is the authored
            // PCB trace at the terminal anchors rather than a raised cable mesh.
            if (path) path.gameObject.SetActive(false);
            ReadGraph();
            if (RouteConnected(routes[route]))
                FinishRoute(to);
            else
            {
                effects.Emit(to.transform.position, 1);
                notice = "Front trace confirmed. Select Back copper to route the second segment.";
                QueueCheckpoint();
            }
        }

        private void FinishRoute(SparkTerminal end)
        {
            effects.Emit(end.transform.position, 1);
            backLayer = false;
            notice = route == JumperIndex ? "W035 connected. Check live feedback before qualification." :
                $"W{route + 1:000} routed. Drag between the next highlighted pads.";
            QueueCheckpoint();
            if (stage == 2 && routed == CopperCount)
            {
                route = JumperIndex;
                focusPending = true;
                Award();
                notice = "Copper passed. Drag between the highlighted J3 pads to fit W035.";
            }
            else if (stage == 2)
            {
                for (int i = 0; i < CopperCount; i++)
                {
                    NextRoute();
                    if (!RouteConnected(routes[route])) break;
                }
                if (routes[route].via && !routes[route].via.IsClosed)
                    notice = $"Select PLATE VIA for {routes[route].viaName} before dragging the two segments.";
            }
        }

        private void Update()
        {
            if (!Active) return;
            ReadGraph();
            var solver = manager.ElectricalSolver.LatestSolveResult;
            bool Measure(SparkTerminal pin,out float voltage)
            {
                voltage=0;
                return solver != null && solver.TryGetVoltage(pin,supplyReturn,out voltage);
            }
            bool protectedPower = supply.IsOutputActive && !supply.IsOverloaded &&
                Measure(protectedInput, out inputVoltage) && inputVoltage is > 3.65f and < 5.15f;
            if (!supply.IsOutputActive) inputVoltage = 0;
            Output(regulator, protectedPower);
            bool solvedRail = Measure(rail, out railVoltage);
            validRail = protectedPower && solvedRail && railVoltage is > 3.16f and < 3.44f;
            Output(feedback, validRail);
            bool solvedFeedback = Measure(feedbackInput, out feedbackVoltage);
            trusted = validRail && solvedFeedback && feedbackVoltage is > 2.6f and < 3.5f &&
                RouteConnected(routes[JumperIndex]);
            clock += Time.deltaTime;
            bool nextLoad = trusted && !lost && fault < 0 && clock%0.5f < .25f;
            if (nextLoad != load)
            {
                load = nextLoad;
                loadGate.SetState(load ? SparkSwitchState.Closed : SparkSwitchState.Open);
            }
            hasShuntSample = supply.IsOutputActive && trusted && !lost && fault < 0 &&
                Measure(shunt, out shuntVoltage);
            if (hasShuntSample && load && shuntVoltage > .00008f) lastShunt = shuntVoltage;

            if (armed && !RouteConnected(routes[JumperIndex]))
            {
                armed = false;
                lost = true;
                measured = reset = running = calibrated = false;
                hasShuntSample = false;
                load = false;
                loadGate.SetState(SparkSwitchState.Open);
                QueueCheckpoint();
                notice = "W035 open. Feedback invalid and load inhibited. Refit W035 and measure.";
                effects.Emit(routes[JumperIndex].from.transform.position,2);
            }
            if (running)
            {
                if (!validRail || !trusted || !AllCopper() || fault >= 0 || lost)
                {
                    running=false;
                    notice="Qualification stopped. Inspect 3.3 V, W035 and the copper network.";
                }
                else if ((liveFor += Time.deltaTime) >= 2f)
                {
                    running=false;
                    if (lastShunt <= .00008f) notice="Qualification requires a solved load-current sample.";
                    else if (stage == 3 || stage == 5 && reset) Award();
                    else if (stage == 4 && repairedPending >= 0)
                    {
                        repairs[repairedPending] = true;
                        repairedPending = -1;
                        if (Array.TrueForAll(repairs, value => value)) Award();
                        else notice = "Repair qualification passed. Select the next fault case.";
                        QueueCheckpoint();
                    }
                    else notice="Board measured. Finish the required repair or feedback exercise.";
                }
            }
            Present();
            if (followRoute && focusPending && !SignalWireBuilder.AnyWireDrawing)
            {
                focusPending = false;
                FocusCurrent();
            }
            if (Time.unscaledTime >= checkpointDue) FlushCheckpoint();
        }

        private static void Output(SparkPowerSupply device,bool enable)
        {
            if (device.IsOutputActive != enable) device.SetOutput(enable,default);
        }

        private bool Edge(SparkTerminal a,SparkTerminal b)
        {
            return edges.Exists(edge=>edge.Kind==SparkConnectionKind.Wire &&
                (edge.A==a && edge.B==b || edge.A==b && edge.B==a));
        }

        private bool RouteConnected(Route item)
        {
            return item.via ? item.via.IsClosed && Edge(item.from,item.viaTop) && Edge(item.viaBottom,item.to) : Edge(item.from,item.to);
        }

        private void ReadGraph()
        {
            edges.Clear();
            manager.CircuitSystem.CopyConnections(edges);
            routed=0;
            for(int i=0;i<routes.Length;i++)
            {
                var item=routes[i];
                bool connected=RouteConnected(item);
                if (i < CopperCount && connected) routed++;
                bool showFront = stage != 2 || !backLayer;
                bool showBack = stage != 2 || backLayer;
                item.copper.enabled = showFront && (item.via ? Edge(item.from, item.viaTop) : connected);
                if (item.backCopper) item.backCopper.enabled = showBack && item.via.IsClosed && Edge(item.viaBottom, item.to);
            }
            bool linked = RouteConnected(routes[JumperIndex]);
            if (jumperArt.activeSelf != linked) jumperArt.SetActive(linked);
        }

        private bool AllCopper()
        {
            ReadGraph();
            return routed == CopperCount;
        }

        public void InspectNet()
        {
            if (!Active || stage!=0) return;
            inspected.Add(route);
            notice=$"Inspected {inspected.Count}/{CopperCount} source routes.";
            if(inspected.Count == CopperCount)
            {
                Award();
                notice = "Choose the matching part, align pin 1, then fit and solder it.";
            }
            else NextRoute();
            QueueCheckpoint();
        }

        private void ResetPartSelection()
        {
            orientation = 1 + part % 3;
            fitted = false;
        }

        public void NextPart()
        {
            if (stage != 1) return;
            for (int i = 1; i <= parts.Length; i++)
            {
                int next = (part + i) % parts.Length;
                if (parts[next].activeSelf) continue;
                part = next;
                ResetPartSelection();
                return;
            }
        }

        public void NextStockPart()
        {
            if (stage != 1) return;
            partPick = (partPick + 1) % parts.Length;
            ResetPartSelection();
        }

        public void MatchPart()
        {
            if (stage != 1) return;
            partPick = part;
            fitted = false;
            notice = "Matching stock selected. Rotate the part to align pin 1.";
        }

        public void RotatePart()
        {
            if (stage != 1) return;
            orientation = (orientation + 1) % 4;
            fitted = false;
        }

        public void FitPart()
        {
            if (!Active || stage != 1 || parts[part].activeSelf) return;
            if (partPick != part)
            {
                notice = $"Wrong part for {references[part]}. Use the matching stock value and reference.";
                return;
            }
            if (orientation != 0)
            {
                notice = "Align pin 1 before fitting this part.";
                return;
            }
            fitted = true;
            notice = $"{references[part]} aligned in its footprint. Solder to commit the placement.";
        }

        public void SolderPart()
        {
            if (!Active || stage != 1 || !fitted || parts[part].activeSelf) return;
            SetInstalled(part, true);
            fitted = false;
            effects.Emit(PartAnchor(part).position, 0);
            QueueCheckpoint();
            if (InstalledCount == parts.Length)
            {
                route = 0;
                Award();
                notice = "Assembly passed. Open Wire tool and drag between the highlighted native pads.";
            }
            else
            {
                notice = $"{references[part]} soldered. Select stock for the next footprint.";
                NextPart();
            }
        }

        public void NextRoute() => SelectRoute(1);
        public void PreviousRoute() => SelectRoute(-1);

        private void SelectRoute(int direction)
        {
            if (stage is 1 or >= 6) return;
            int count = stage >= 3 ? routes.Length : CopperCount;
            route = (route + direction + count) % count;
            backLayer = false;
            focusPending = true;
        }

        public void FrontCopper() => SelectLayer(false);
        public void BackCopper() => SelectLayer(true);

        private void SelectLayer(bool back)
        {
            if (stage != 2) return;
            backLayer = back;
            focusPending = true;
            notice = back ? "Back copper selected. Route the second segment of a plated via." :
                "Front copper selected. Start at the highlighted source pad.";
        }

        public void FitVia()
        {
            if (!Active || supply.IsOutputActive || stage != 2 || !routes[route].via) return;
            if (backLayer)
            {
                notice = "Select Front copper and plate the via barrel first.";
                return;
            }
            var item = routes[route];
            if (item.via.IsClosed) return;
            item.via.SetState(SparkSwitchState.Closed);
            effects.Emit(item.viaTop.transform.position, 0);
            notice = $"{item.viaName} plated. Wire the front segment, then switch to Back copper.";
            QueueCheckpoint();
        }

        private bool Connect(SparkTerminal a,SparkTerminal b,out ulong id)
        {
            id=0;
            if(Edge(a,b)) return true;
            if(!manager.CircuitSystem.TryCreateConnection(a,b,SparkConnectionKind.Wire,
                SparkConnectionDirection.Bidirectional,out SparkCircuitConnection edge)) return false;
            id=edge.Id;
            return true;
        }

        private void Remove(ulong id)
        {
            if (id != 0 && (!wireBuilder || !wireBuilder.RemoveConnectedWire(id)))
                manager.CircuitSystem.TryRemoveConnection(id, out _);
        }

        private void RemoveEdge(SparkTerminal a, SparkTerminal b)
        {
            int index = edges.FindIndex(edge => edge.Kind == SparkConnectionKind.Wire &&
                (edge.A == a && edge.B == b || edge.A == b && edge.B == a));
            if (index >= 0) Remove(edges[index].Id);
        }

        public void UndoCopper()
        {
            if (!CanRoute) return;
            ReadGraph();
            var item = routes[route];
            RemoveEdge(item.from, item.via ? item.viaTop : item.to);
            if (item.via) RemoveEdge(item.viaBottom, item.to);
            ReadGraph();
            calibrated = false;
            QueueCheckpoint();
        }

        public void TogglePower()
        {
            if(!Active || stage<3) return;
            Output(supply,!supply.IsOutputActive);
            calibrated = running = false;
            hasShuntSample = false;
            notice = supply.IsOutputActive ? "Protected input enabled. Measure the rail and W035 feedback." : "Protected input isolated.";
        }

        public void TestBoard()
        {
            if (!Active || stage is < 3 or > 5 || running || !calibrated ||
                !validRail || !trusted || !AllCopper() || fault >= 0 || lost)
            {
                notice = "Qualify after a fresh rail measurement, complete copper, and trusted W035 feedback.";
                return;
            }
            running=true;
            liveFor=0;
            lastShunt=0;
            run++;
            notice=$"Run {run}: acquiring a live load-current sample.";
        }

        private int repairedPending = -1;

        public void InjectVia() => Inject(0);
        public void InjectPullup() => Inject(1);
        public void InjectBridge() => Inject(2);
        private void Inject(int kind)
        {
            if (!Active || stage != 4 || running || fault >= 0 || repairedPending >= 0 ||
                !supply.IsOutputActive || kind != Array.FindIndex(repairs, passed => !passed)) return;
            fault = kind;
            measured = calibrated = false;
            if(kind==0) routes[29].via.SetState(SparkSwitchState.Open);
            else if(kind==1) pullup.SetResistance(47000);
            else bridge.SetState(SparkSwitchState.Closed);
            notice = "Fault seeded. Measure the indicated circuit, isolate power, and repair it.";
            QueueCheckpoint();
            effects.Emit(board.TransformPoint(new Vector3(0,1.3f,0)),2);
        }

        public void Measure()
        {
            if(!Active) return;
            ReadGraph();
            if(fault==0 && !RouteConnected(routes[29])) {measured=true;notice="V05 is open on W030.";}
            else if(fault==1 && pullup.Resistance>45000) {measured=true;notice="R2 reads 47 kΩ. Fit the specified 4.7 kΩ part.";}
            else if(fault==2 && bridge.IsClosed) {measured=true;notice="Q1 gate-to-drain bridge detected.";}
            else if (lost && validRail && trusted)
            {
                measured = true;
                notice = "Fresh feedback measured after reconnection. Reset the safety latch.";
            }
            else if (validRail && trusted && AllCopper())
            {
                calibrated = true;
                notice = $"Live rail {railVoltage:F2} V and feedback {feedbackVoltage:F2} V recorded. Qualify a fresh load sample.";
            }
            else notice = "Waiting for solved rail and feedback measurements.";
        }

        public void Repair()
        {
            if (!Active || fault < 0 || !measured) return;
            if (supply.IsOutputActive)
            {
                notice = "Isolate the protected bench before touching the repair.";
                return;
            }
            if(fault==0) routes[29].via.SetState(SparkSwitchState.Closed);
            else if(fault==1) pullup.SetResistance(4700);
            else bridge.SetState(SparkSwitchState.Open);
            repairedPending = fault;
            fault = -1;
            measured = calibrated = false;
            notice = "Repair applied. Re-enable power, measure the rail, and qualify a fresh run.";
            QueueCheckpoint();
            effects.Emit(board.TransformPoint(new Vector3(0,1.3f,0)),0);
        }

        public void ArmJumper()
        {
            if(!Active || stage!=5 || !trusted || running || lost) return;
            route = JumperIndex;
            backLayer = false;
            armed = true;
            notice="Use UNDO COPPER to remove the W035 service jumper during the live test.";
        }

        public void ResetSafety()
        {
            if(!lost || !measured || !trusted) return;
            lost = false;
            reset = calibrated = true;
            notice = "Safety latch cleared. Start a fresh qualification.";
            QueueCheckpoint();
        }

        public void BoardView()
        {
            if(!cameraView) return;
            followRoute = focusPending = false;
            var substrate=arena.Find("L94_ART/L94_FR4BoardSubstrate");
            var surfaces=substrate ? substrate.GetComponentsInChildren<Renderer>() : System.Array.Empty<Renderer>();
            if(surfaces.Length==0){cameraView.UseOverview();return;}
            var bounds=surfaces[0].bounds;
            foreach(var surface in surfaces) bounds.Encapsulate(surface.bounds);
            var reach=Vector3.right*bounds.extents.x*1.1f;
            Vector3 focus = bounds.center;
            cameraView.UseSocketView(focus-reach,focus+reach);
        }
        public void PadView()
        {
            if(!cameraView) return;
            var item=routes[route];
            cameraView.UseSocketView(item.from.transform.position,item.to.transform.position);
        }
        public void Showcase()
        {
            followRoute = focusPending = false;
            if (cameraView) cameraView.UseShowcaseView();
        }

        private bool Award(bool announce = true)
        {
            if(stage>=Gates.Length || !loader.ActiveBindings.SetRuntimeComponentBinding(Gates[stage],certification)) return false;
            stage++;
            if (stage == 1) ResetPartSelection();
            calibrated = false;
            QueueCheckpoint();
            manager.QueueEvaluation();
            if (!announce) return true;
            notice=$"Objective {stage}/6 recorded.";
            effects.Emit(board.TransformPoint(new Vector3(0,1.3f,0)),3);
            return true;
        }

    }
}
