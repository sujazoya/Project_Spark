using System;
using System.Collections.Generic;
using AAAUI.VFX;
using ProjectSpark.Circuit;
using ProjectSpark.UI.VisualTeaching;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.Gameplay
{
    public sealed partial class SparkLevel95Process : MonoBehaviour, ISparkAILevelTeachingFocus
    {
        public static readonly string[] Gates = {"L95_Assembly","L95_Network","L95_Clocks","L95_Duty","L95_Repair","L95_Recovery"};

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
        public SparkPowerSupply supply, regulator, certification;
        public SparkSwitch loadGate, bridge;

        public Route[] routes;
        public GameObject jumperArt;
        public SparkLevel95Effects effects;
        public Text heading, detail, readings;
        private LevelGamePlayManager manager;
        private SparkLevelLayoutLoader loader;
        private SparkLevel95BoardView cameraView;
        private SignalWireBuilder wireBuilder;
        private GUIStyle padLabel;
        private SparkLevel95Instruments instruments;
        private SparkElectricalComponent[][] assemblyDevices;
        private int partPick;
        private bool fitted, backLayer, followRoute, focusPending;
        private const int CopperCount = 48;
        private int JumperIndex => routes.Length - 1;
        private bool InAssembly => stage == 0 && inspected.Count == CopperCount;
        private readonly List<SparkCircuitConnection> edges = new();
        private readonly HashSet<int> inspected = new();
        private readonly bool[] repairs = new bool[3];
        private int stage, part, orientation, route, fault = -1, routed, run;
        private bool armed, lost, reset, measured, running, validRail, trusted, load;
        private float shuntVoltage, clock;
        private string notice = "Inspect the nets, then fit the board components.";

        private bool Active => loader && manager && loader.ActiveLayout == layout && manager.ActiveLevel == layout.LevelDefinition && manager.CircuitSystem && manager.ElectricalSolver;

        private void Start()
        {
            loader = FindFirstObjectByType<SparkLevelLayoutLoader>();
            manager = FindFirstObjectByType<LevelGamePlayManager>();
            cameraView = gameObject.AddComponent<SparkLevel95BoardView>();
            cameraView.Initialize(arena);
            BoardView();
            wireBuilder = FindFirstObjectByType<SignalWireBuilder>();
            if (wireBuilder)
            {
                wireBuilder.TerminalPickerOverride = PickPad;
                wireBuilder.TerminalWireConnected += OnManualWire;
            }
            PrepareAssembly();
            float width = Mathf.Clamp(arena.lossyScale.x * .0035f, .012f, .038f);
            foreach (var item in routes)
            {
                item.copper.widthMultiplier = width;
                if (item.backCopper) item.backCopper.widthMultiplier = width * .88f;
            }
            BuildConsole();
            instruments = SparkLevel95Instruments.Create(this, heading);
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
            if (references[index] == "U5") regulator.SetElectricalEnabled(installed);
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
            if (InAssembly) target = PartAnchor(part);
            else if (PendingPads(out var pad, out _)) target = pad.transform;
            else if (stage <= 1 || stage == 5) target = routes[route].from.transform;
            else target = rail.transform;
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

        private bool CanRoute => Active && stage is >= 1 and < 6 && (!supply.IsOutputActive || route == 48);

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
            if (path) path.gameObject.SetActive(false);
            ReadGraph();
            if (RouteConnected(routes[route]))
                FinishRoute(to);
            else
            {
                effects.Emit(to.transform.position, 1);
                notice = "Front segment complete. Select Back copper for the next pair.";
                QueueCheckpoint();
            }
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
                if(i<48 && connected) routed++;
                bool front = stage != 1 || !backLayer;
                bool back = stage != 1 || backLayer;
                item.copper.enabled = front && (item.via ? Edge(item.from, item.viaTop) : connected);
                if (item.backCopper) item.backCopper.enabled = back && item.via.IsClosed && Edge(item.viaBottom, item.to);
            }
            jumperArt.SetActive(RouteConnected(routes[48]));
        }

        private bool AllCopper()
        {
            ReadGraph();
            return routed==48;
        }

        public void InspectNet()
        {
            if (!Active || stage != 0 || InAssembly) return;
            inspected.Add(route);
            notice=$"Inspected {inspected.Count}/48 source routes.";
            if (InAssembly)
            {
                ResetPartSelection();
                notice = "Choose the matching stock and align it before soldering.";
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
            if (!InAssembly) return;
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
            if (!InAssembly) return;
            partPick = (partPick + 1) % parts.Length;
            ResetPartSelection();
        }

        public void MatchPart()
        {
            if (!InAssembly) return;
            partPick = part;
            fitted = false;
            notice = "Matching stock selected. Rotate the part to align pin 1.";
        }

        public void RotatePart()
        {
            if (!InAssembly) return;
            orientation = (orientation + 1) % 4;
            fitted = false;
        }

        public void FitPart()
        {
            if (!Active || !InAssembly || parts[part].activeSelf) return;
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
            if (!Active || !InAssembly || !fitted || parts[part].activeSelf) return;
            SetInstalled(part, true);
            fitted = false;
            effects.Emit(PartAnchor(part).position, 0);
            QueueCheckpoint();
            if (InstalledCount == parts.Length)
            {
                route = 0;
                Award();
                notice = "Assembly passed. Route the native pads, then install the separate W049 jumper.";
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
            if (InAssembly || stage >= 6) return;
            int count = stage >= 2 || stage == 1 && routed == CopperCount ? routes.Length : CopperCount;
            route = (route + direction + count) % count;
            backLayer = false;
            focusPending = true;
        }

        public void FrontCopper() => SelectLayer(false);
        public void BackCopper() => SelectLayer(true);

        private void SelectLayer(bool back)
        {
            if (stage != 1) return;
            backLayer = back;
            focusPending = true;
            notice = back ? "Back copper selected. Route the second segment of a plated via." :
                "Front copper selected. Start at the highlighted source pad.";
        }

        public void FitVia()
        {
            if (!Active || supply.IsOutputActive || stage != 1 || !routes[route].via) return;
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
            QueueCheckpoint();
        }

        public void ArmJumper()
        {
            if(!Active || stage!=5 || !trusted || running || lost) return;
            route = JumperIndex;
            backLayer = false;
            armed = true;
            notice="Remove W049 with UNDO COPPER or Wire Manager. The raw clock stays available.";
        }

        public void ResetSafety()
        {
            if(!lost || !measured || !trusted) return;
            lost=false;
            reset=true;
            QueueCheckpoint();
            notice="Safety latch cleared. Start a fresh qualification.";
        }

        public void BoardView()
        {
            if(!cameraView) return;
            followRoute = focusPending = false;
            var substrate=arena.Find("L95_ART/L95_FR4PrecisionBoardCore");
            var surfaces=substrate ? substrate.GetComponentsInChildren<Renderer>() : System.Array.Empty<Renderer>();
            if(surfaces.Length==0){cameraView.UseOverview();return;}
            var bounds=surfaces[0].bounds;
            foreach(var surface in surfaces) bounds.Encapsulate(surface.bounds);
            cameraView.UseBoardView(bounds);
        }
        public void PadView()
        {
            if(!cameraView) return;
            var item=routes[route];
            if (PendingPads(out var a, out var b)) cameraView.UseSocketView(a.transform.position,b.transform.position);
            else cameraView.UseSocketView(item.from.transform.position,item.to.transform.position);
        }
        public void Showcase() { followRoute = focusPending = false; if (cameraView) cameraView.UseShowcaseView(); }

        private bool Award(bool announce = true)
        {
            if (stage >= Gates.Length || !loader.ActiveBindings.SetRuntimeComponentBinding(Gates[stage], certification))
                return false;
            stage++;
            QueueCheckpoint();
            manager.QueueEvaluation();
            if (!announce) return true;
            notice = $"Objective {stage}/6 recorded.";
            effects.Emit(board.TransformPoint(new Vector3(0, 1.3f, 0)), 3);
            return true;
        }
    }
}
