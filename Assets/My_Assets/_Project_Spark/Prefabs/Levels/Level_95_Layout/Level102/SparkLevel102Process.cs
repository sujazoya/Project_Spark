using System;
using System.Collections.Generic;
using AAAUI.VFX;
using ProjectSpark.Circuit;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.Gameplay
{
    public sealed partial class SparkLevel102Process : MonoBehaviour
    {
        public static readonly string[] Gates = {"L102_Assembly","L102_Network","L102_Clocks","L102_Duty","L102_Repair","L102_Recovery"};

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
        public SparkLevel102Effects effects;
        public Text heading, detail, readings;
        private LevelGamePlayManager manager;
        private SparkLevelLayoutLoader loader;
        private SparkLevel102BoardView cameraView;
        private SignalWireBuilder wireBuilder;
        private GUIStyle padLabel;
        private SparkLevel102Instruments instruments;
        private Button challengeButton;
        private Text challengeLabel;
        private bool challengeMode;
        private static readonly string[] PreparationControls = {"INSPECT NET", "NEXT PART", "ROTATE 90°", "FIT PART"};
        private readonly List<SparkCircuitConnection> edges = new();
        private readonly HashSet<int> inspected = new();
        private readonly bool[] repairs = new bool[3];
        private int stage, part, orientation, route, fault = -1, routed, run;
        private bool armed, lost, reset, measured, running, validRail, trusted, load;
        private float shuntVoltage, clock;
        private string notice = "Inspect the nets, then fit the board components.";

        private bool Active => loader && manager && loader.ActiveLayout == layout && manager.CircuitSystem && manager.ElectricalSolver;

        private void Start()
        {
            loader = FindFirstObjectByType<SparkLevelLayoutLoader>();
            manager = FindFirstObjectByType<LevelGamePlayManager>();
            cameraView = gameObject.AddComponent<SparkLevel102BoardView>();
            cameraView.Initialize(arena);
            BoardView();
            wireBuilder = FindFirstObjectByType<SignalWireBuilder>();
            if (wireBuilder)
            {
                wireBuilder.TerminalPickerOverride = PickPad;
                wireBuilder.TerminalWireConnected += OnManualWire;
            }
            CreateChallengeControl();
            instruments = SparkLevel102Instruments.Create(this,arena,heading);
            SetChallengeMode(false);
        }

        private void CreateChallengeControl()
        {
            var panel = heading.rectTransform.parent;
            var node = new GameObject("Challenge Mode", typeof(RectTransform), typeof(Image), typeof(Button));
            var rect = (RectTransform)node.transform;
            rect.SetParent(panel, false);
            rect.anchorMin = rect.anchorMax = new Vector2(.5f, 0f);
            rect.pivot = new Vector2(.5f, 0f);
            rect.anchoredPosition = new Vector2(481f, 213f);
            rect.sizeDelta = new Vector2(164f, 29f);
            var image = node.GetComponent<Image>();
            image.color = new Color(.15f, .35f, .71f);
            challengeButton = node.GetComponent<Button>();
            challengeButton.targetGraphic = image;
            challengeButton.onClick.AddListener(ToggleChallengeMode);

            var labelNode = new GameObject("Mode label", typeof(RectTransform), typeof(Text));
            var labelRect = (RectTransform)labelNode.transform;
            labelRect.SetParent(rect, false);
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = labelRect.offsetMax = Vector2.zero;
            challengeLabel = labelNode.GetComponent<Text>();
            challengeLabel.font = heading.font;
            challengeLabel.fontSize = 13;
            challengeLabel.alignment = TextAnchor.MiddleCenter;
            challengeLabel.color = Color.white;
            challengeLabel.raycastTarget = false;
        }

        private bool CanSwitchChallengeMode()
        {
            if (stage > 1 || (wireBuilder && wireBuilder.IsDrawing)) return false;
            foreach (var item in routes)
            {
                if (item.via)
                {
                    if (item.via.IsClosed || Edge(item.from, item.viaTop) || Edge(item.viaBottom, item.to))
                        return false;
                }
                else if (Edge(item.from, item.to))
                    return false;
            }
            return true;
        }

        public void ToggleChallengeMode()
        {
            if (!Active) return;
            ReadGraph();
            if (CanSwitchChallengeMode()) SetChallengeMode(!challengeMode);
        }

        private void SetChallengeMode(bool enabled)
        {
            challengeMode = enabled;
            stage = part = orientation = route = 0;
            inspected.Clear();
            foreach (var component in parts)
                component.SetActive(!enabled);
            foreach (string name in PreparationControls)
            {
                var control = heading.transform.parent.Find(name);
                if (control && control.TryGetComponent<Button>(out var button))
                    button.interactable = enabled;
            }
            challengeLabel.text = enabled ? "CHALLENGE MODE: ON" : "CHALLENGE MODE: OFF";
            if (enabled)
            {
                loader.ActiveBindings.ClearRuntimeComponentBinding(Gates[0]);

                notice = "Inspect the nets, then fit the board components.";
            }
            else
            {
                Award(false);
                notice = "Parts preinstalled. Select the Wire tool and drag between the highlighted pads.";
            }
            manager.QueueEvaluation();
        }

        private void OnDisable()
        {
            if (!wireBuilder) return;
            wireBuilder.TerminalWireConnected -= OnManualWire;
            if (wireBuilder.TerminalPickerOverride != PickPad) return;
            if (wireBuilder.IsDrawing) wireBuilder.CancelWire();
            wireBuilder.TerminalPickerOverride = null;
        }

        private bool CanRoute => Active && stage is >= 1 and < 6 && (!supply.IsOutputActive || route == 48);

        private bool PendingPads(out SparkTerminal a, out SparkTerminal b)
        {
            a = null;
            b = null;
            if (!CanRoute) return false;
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
            if (ProjectSpark.UIManagement.SparkUIMonitorDashboard.IsConsoleVisible) return;
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
            foreach (var renderer in path.GetComponentsInChildren<Renderer>(true)) renderer.forceRenderingOff = true;
            ReadGraph();
            if (RouteConnected(routes[route]))
                FinishRoute(to);
            else
            {
                effects.Emit(to.transform.position, 1);
                notice = "First segment connected. Drag between the next highlighted pads.";
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
                item.copper.enabled=connected;
                if(item.backCopper) item.backCopper.enabled=connected;
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
            if (!Active || stage!=0) return;
            inspected.Add(route);
            notice=$"Inspected {inspected.Count}/48 source routes.";
            if (inspected.Count == 48) notice = "All nets inspected. Fit all fixed components.";
            else NextRoute();
        }

        public void NextPart()
        {
            if (!challengeMode || stage != 0 || inspected.Count < 48) return;
            for(int i=1;i<=parts.Length;i++)
            {
                int next=(part+i)%parts.Length;
                if(!parts[next].activeSelf){part=next;break;}
            }
            orientation=part%4;
        }

        public void RotatePart()
        {
            if(challengeMode && stage==0 && inspected.Count==48) orientation=(orientation+1)%4;
        }

        public void FitPart()
        {
            if(!Active || !challengeMode || stage!=0 || inspected.Count<48 || parts[part].activeSelf) return;
            if(orientation!=0)
            {
                notice="Align the part’s pin 1 mark with the footprint.";
                effects.Emit(parts[part].transform.TransformPoint(new Vector3(0,1.27f,0)),2);
                return;
            }
            parts[part].SetActive(true);
            var pivot=Array.Find(parts[part].GetComponentsInChildren<Transform>(),t=>t.name.StartsWith("ANIM_",StringComparison.Ordinal));
            effects.Emit(pivot ? pivot.position : board.position,0);
            if(Array.TrueForAll(parts,p=>p.activeSelf)) { route=0; Award(); notice="Select the Wire tool and drag between the highlighted pads."; }
            else NextPart();
        }

        public void NextRoute()
        {
            route=(route+1)%(stage>=1 ? 49 : 48);
        }

        public void PreviousRoute()
        {
            int total=stage>=1 ? 49 : 48;
            route=(route+total-1)%total;
        }

        public void FitVia()
        {
            if(!Active || supply.IsOutputActive || stage<1 || !routes[route].via) return;
            var item=routes[route];
            if(item.via.IsClosed) return;
            item.via.SetState(SparkSwitchState.Closed);
            effects.Emit(item.viaTop.transform.position,0);
            notice=$"{item.viaName} plated. Drag between the highlighted pads for each segment.";
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

        public void RouteCopper()
        {
            if (!CanRoute) return;
            ReadGraph();
            var item=routes[route];
            if(RouteConnected(item)) return;
            if(item.via && !item.via.IsClosed){notice=$"Plate {item.viaName} before routing.";return;}
            ulong second=0;
            if(!Connect(item.from,item.via ? item.viaTop : item.to,out ulong first))
            {notice="Copper connection rejected by the circuit graph.";return;}
            if(item.via && !Connect(item.viaBottom,item.to,out second))
            {
                Remove(first);
                notice="The second copper segment could not connect.";
                return;
            }
            ReadGraph();
            FinishRoute(item.to);
        }

        public void UndoCopper()
        {
            if (!CanRoute) return;
            ReadGraph();
            var item = routes[route];
            RemoveEdge(item.from, item.via ? item.viaTop : item.to);
            if (item.via) RemoveEdge(item.viaBottom, item.to);
            ReadGraph();
        }

        public void ArmJumper()
        {
            if(!Active || stage!=5 || !trusted || running || lost) return;
            route=48;
            armed=true;
            notice="Remove W049 with UNDO COPPER or Wire Manager. The raw clock stays available.";
        }

        public void ResetSafety()
        {
            if(!lost || !measured || !trusted) return;
            lost=false;
            reset=true;
            notice="Safety latch cleared. Start a fresh qualification.";
        }

        public void BoardView()
        {
            if(!cameraView) return;
            var substrate=arena.Find("L102_ART/L95_FR4PrecisionBoardCore");
            var surfaces=substrate ? substrate.GetComponentsInChildren<Renderer>() : System.Array.Empty<Renderer>();
            if(surfaces.Length==0){cameraView.UseOverview();return;}
            var bounds=surfaces[0].bounds;
            foreach(var surface in surfaces) bounds.Encapsulate(surface.bounds);
            var reach=Vector3.right*bounds.extents.x*1.49f;
            var focus=bounds.center;
            cameraView.UseSocketView(focus-reach,focus+reach);
        }
        public void PadView()
        {
            if(!cameraView) return;
            var item=routes[route];
            if (PendingPads(out var a, out var b)) cameraView.UseSocketView(a.transform.position,b.transform.position);
            else cameraView.UseSocketView(item.from.transform.position,item.to.transform.position);
        }
        public void Showcase(){if(cameraView)cameraView.UseShowcaseView();}

        private void Award(bool announce = true)
        {
            if(stage>=Gates.Length || !loader.ActiveBindings.SetRuntimeComponentBinding(Gates[stage],certification)) return;
            stage++;
            manager.QueueEvaluation();
            if (!announce) return;
            notice=$"Objective {stage}/6 recorded.";
            effects.Emit(board.TransformPoint(new Vector3(0,1.3f,0)),3);
        }

        private void Present()
        {
            challengeButton.interactable = CanSwitchChallengeMode();
            heading.text=$"PULSE ARCHITECT   /   LEVEL 102   /   {stage}/6   /   COPPER {routed}/48";
            var item=routes[route];
            detail.text=challengeMode && stage==0 && inspected.Count==48 ? $"FOOTPRINT {references[part]}  ·  {values[part]}  ·  PIN 1 ANGLE {orientation*90}°    |    {notice}" :
                $"W{route+1:000}  {item.from.name} → {item.to.name}  [{item.net} {item.viaName}]    |    {notice}";
            readings.text=$"INPUT {(supply.IsOutputActive?"ON":"OFF")}   /   3.3 V RAIL {(validRail?"VALID":"OFF")}   /   W049 {(trusted?"TRUSTED":"INVALID")}   /   CLOCK {rawHz:F0} / {dividedHz:F0} / {slowHz:F2} Hz   /   RV1 {requestedDuty}%";
        }
    }
}

