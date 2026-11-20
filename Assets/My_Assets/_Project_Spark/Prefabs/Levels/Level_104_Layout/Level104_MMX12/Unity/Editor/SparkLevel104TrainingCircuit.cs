using System;
using System.Collections.Generic;
using System.Linq;
using ProjectSpark.Gameplay;
using ProjectSpark.Gameplay.MemoryMatrix;
using SparkLevel104Process = ProjectSpark.Gameplay.MemoryMatrix.SparkLevel104Process;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ProjectSpark.EditorTools.MemoryMatrix
{
    internal static partial class SparkLevel104Builder
    {
        private static readonly string[] BytePaths =
        {
            "W039", "W040", "W041", "W042", "W063", "W064", "W065", "W066"
        };

        private static void ConfigureBoard(SparkLevel104Process board, Circuit graph, string[][] plans,
            IEnumerable<string> barrelIds, Dictionary<string, Material> mats, List<string> csv)
        {
            board.input = graph.Pin("SUPPLY_POS");
            board.ground = graph.Pin("SUPPLY_RET");
            board.source = graph.Supply("TRAINING_INPUT", board.input, board.ground, 5f);
            board.ground.transform.SetParent(board.source.transform, true);
            Set(board.ground, ("owner", board.source));
            graph.Load("Continuity reference", board.input, board.ground, 10000f);
            board.aux = graph.Pin("U7.CONTROLLER_3V3");
            board.vdd = graph.Pin("U4.DDR4_VDD");
            board.vpp = graph.Pin("U5.DDR4_VPP");
            board.vtt = graph.Pin("U6.DDR4_VTT");
            board.lanes = Enumerable.Range(0, 4).Select(_ => new SparkLevel104Process.Lane()).ToArray();

            var remaining = new Queue<string>(barrelIds.Where(id => id != "V19"));
            var groups = plans.GroupBy(row => row[1]).ToArray();
            var routes = new List<SparkLevel104Process.Route>();
            var witnesses = new List<SparkTerminal>();
            var pads = new List<Vector3>();
            var sites = board.parts.Select(p => graph.Root.InverseTransformPoint(p.footprint.position)).ToArray();
            Vector3 Site(string id) => sites[Array.FindIndex(board.parts, part => part.label == id)];

            for (int group = 0; group < groups.Length; group++)
            {
                int viasLeft = group < 3 ? 6 : 5;
                SparkTerminal previous = board.input;
                foreach (var task in groups[group])
                {
                    string id = task[0];
                    int index = routes.Count;
                    int path = Array.IndexOf(BytePaths, id);
                    int laneIndex = path / 2;
                    bool bytePath = path >= 0;
                    bool strobe = bytePath && path % 2 == 0;
                    bool specialVia = path == 4;
                    bool command = id == "W021";
                    bool clock = id is "W019" or "W020";
                    bool functional = clock || command || bytePath;
                    string memory = laneIndex < 2 ? "U2" : "U3";
                    string net = bytePath ? "BYTE" + laneIndex + (strobe ? "_DQS" : "_DQ") :
                        command ? "CA_TERMINATED" : clock ? id == "W019" ? "CK_P" : "CK_N" : "CHECK_" + (group + 1);
                    Vector3 site = sites[index % sites.Length];
                    Vector3 sourceSite = functional ? Site(clock ? "U8" : "U1") : site;
                    Vector3 targetSite = bytePath ? Site(memory) : command ? Site("RN1") : clock ? Site("U2") : site;
                    var entry = FindPad(sourceSite, functional ? index % 8 : 0, pads);
                    var destination = FindPad(targetSite, functional ? (index + 3) % 8 : 2, pads);
                    var a = graph.Pin(functional ? net + ".TX" : id + ".A", entry, true);
                    var b = graph.Pin(functional ? net + (specialVia ? ".V19_TAP" : ".PAD") : id + ".B", destination, true);
                    var route = new SparkLevel104Process.Route
                    {
                        id = id, group = groups[group].Key, net = net, entry = entry, destination = destination,
                        layer = functional ? 1 : (index + group) % 8 + 1,
                        from = a, to = b, copper = Copper(graph.Root, mats[functional ? "gold" : "copper"], id, entry, destination)
                    };
                    var assemblyOut = functional ? graph.Pin(id + ".ASSEMBLY_CHECK") : a;
                    var contact = graph.SwitchedOutput(id + " native solder contact", previous, assemblyOut, index >= board.parts.Length);
                    if (index < board.parts.Length) board.parts[index].contact = contact;
                    previous = functional ? assemblyOut : b;
                    if (functional)
                    {
                        var receiver = b;
                        if (specialVia)
                        {
                            board.strobeRoute = index;
                            route.viaId = "V19";
                            route.exitLayer = 3;
                            receiver = graph.Pin("U3.BYTE2_DQS_RX", destination);
                            route.barrel = graph.SwitchedOutput("V19 lane 2 DQS barrel", b, receiver);
                        }
                        else if (path == 7)
                        {
                            receiver = graph.Pin("U3.BYTE3_DQ0_BALL", destination);
                            board.bgaBall = graph.SwitchedOutput("U3 lane 3 DQ0 BGA contact", b, receiver, true);
                        }

                        SparkSwitch driver;
                        if (command)
                        {
                            var drive = graph.Pin("U1 CA driver output");
                            driver = graph.SwitchedOutput("U1 CA driver", board.vdd, drive);
                            graph.Load("CA series termination witness", drive, a, 10f);
                            board.commandDrive = driver;
                            board.commandSense = receiver;
                            board.termination = graph.Load("RN1 CA termination", receiver, board.vtt, 47f);
                        }
                        else
                        {
                            driver = graph.SwitchedOutput(net + " driver", board.vdd, a);
                            graph.Load(net + " receiver bias", receiver, board.ground, 10000f);
                            if (clock)
                            {
                                if (id == "W019") { board.ckP = receiver; board.ckPDrive = driver; }
                                else { board.ckN = receiver; board.ckNDrive = driver; }
                            }
                            else
                            {
                                var lane = board.lanes[laneIndex];
                                if (strobe) { lane.strobe = receiver; lane.strobeDrive = driver; lane.strobeRoute = index; }
                                else { lane.data = receiver; lane.dataDrive = driver; lane.dataRoute = index; }
                            }
                        }
                    }
                    else if (viasLeft > 0)
                    {
                        viasLeft--;
                        route.viaId = remaining.Dequeue();
                        route.layer = (index + group) % 7 + 1;
                        route.exitLayer = 8;
                        previous = graph.Pin(route.viaId + ".L8", destination);
                        route.barrel = graph.SwitchedOutput(route.viaId + " plated barrel", b, previous);
                    }
                    routes.Add(route);
                    csv.Add(Row(route));
                }
                graph.Load(groups[group].Key + " continuity witness", previous, board.ground, 1000f);
                witnesses.Add(previous);
            }
            if (remaining.Count != 0 || routes.Count != SparkLevel104Process.RouteCount ||
                routes.Count(route => route.barrel) != SparkLevel104Process.ViaCount ||
                routes.Select(route => route.layer).Distinct().Count() != 8 || witnesses.Count != 6 ||
                board.parts.Any(part => !part.contact) || routes[board.strobeRoute].viaId != "V19" ||
                board.lanes.Any(lane => !lane.strobe || !lane.data))
                throw new InvalidOperationException("MMX-12 eight-layer route or byte-lane assignment is incomplete.");
            board.routes = routes.ToArray();
            board.witnesses = witnesses.ToArray();
        }

        private static void ConfigureController(SparkLevel104Process board, Circuit graph,
            Dictionary<string, Material> mats, List<string> csv)
        {
            board.standby = graph.Pin("PROTECTED_5V");
            board.bench = graph.SwitchedOutput("Protected input isolation", board.input, board.standby);
            graph.Load("Protected 5 V monitor", board.standby, board.ground, 330f);

            SparkSwitch Regulate(string id, float voltage, SparkTerminal pin)
            {
                var supply = graph.Pin(id + ".SOURCE");
                graph.Supply(id + " training regulator", supply, board.ground, voltage);
                graph.Load(id + " rail witness", pin, board.ground, 150f);
                return graph.SwitchedOutput(id + " enable", supply, pin);
            }
            board.logicGate = Regulate("U7 controller auxiliary", 3.3f, board.aux);
            board.vppGate = Regulate("U5 DDR4 wordline supply", 2.5f, board.vpp);
            board.vddGate = Regulate("U4 DDR4 data supply", 1.2f, board.vdd);
            board.vttGate = Regulate("U6 DDR4 termination supply", .6f, board.vtt);
            board.vref = graph.Pin("DDR4.VREF_DATA");
            var refFeed = graph.Load("VREF top divider", board.vdd, board.vref, 1200f);
            board.vref.transform.SetParent(refFeed.transform, true);
            graph.Load("VREF lower divider", board.vref, board.ground, 1200f);

            Vector3 Pad(string id, Vector3 offset) =>
                graph.Root.InverseTransformPoint(board.parts.Single(part => part.label == id).footprint.position) + offset;
            Vector3 from = Pad("U1", new Vector3(.009f, .0030f, .005f));
            Vector3 to = Pad("U9", new Vector3(-.008f, .0030f, -.004f));
            var sender = graph.Pin("U1.TRUSTED_MEMORY_READY_OUT", from, true);
            var receiver = graph.Pin("U9.TRUSTED_MEMORY_READY_IN", to, true);
            board.heartbeat = graph.SwitchedOutput("U1 validity pulse driver", board.aux, sender);
            var bias = graph.Load("U9 receiver bias", receiver, board.ground, 10000f);
            receiver.transform.SetParent(bias.transform, true);
            board.jumper = new SparkLevel104Process.Route
            {
                id = "W125", net = "TRUSTED_MEMORY_READY", group = "Independent safety", layer = 1,
                from = sender, to = receiver, entry = from, destination = to,
                copper = Copper(graph.Root, mats["gold"], "W125", from, to)
            };
            csv.Add(Row(board.jumper));
            foreach (var point in new[] { from, to })
                Primitive(graph.Root, "W125 contact", PrimitiveType.Cylinder, point,
                    new Vector3(.0025f, .00022f, .0025f), mats["gold"]);

            board.monitorSupply = graph.Pin("U9.INDEPENDENT_3V3");
            var supervisorFeed = graph.Load("Independent supervisor supply", board.aux, board.monitorSupply, 1f);
            board.monitorSupply.transform.SetParent(supervisorFeed.transform, true);
            graph.Load("Independent supervisor bias", board.monitorSupply, board.ground, 1000f);
            board.protectedFeed = graph.Pin("U9.PROTECTED_TX");
            board.workload = graph.SwitchedOutput("U9 independent transmission permit", board.monitorSupply, board.protectedFeed);
            graph.Load("Protected TX permit witness", board.protectedFeed, board.ground, 330f);

            board.startMarker = Marker(graph.Root, "Source locator", mats["gold"], .0026f);
            board.endMarker = Marker(graph.Root, "Destination locator", mats["green"], .0026f);
            board.locationMarker = Marker(graph.Root, "Placement locator", mats["red"], .0049f);
            board.statusLamp = Primitive(graph.Root, "U9 permit lamp", PrimitiveType.Cube,
                to + new Vector3(-.003f, .001f, 0), new Vector3(.0018f, .0006f, .0012f), mats["green"]);
            var risks = new[] { board.routes[board.strobeRoute].destination, Pad("RN1", Vector3.zero), Pad("U3", Vector3.zero) };
            board.faultOverlays = risks.Select((point, index) =>
            {
                var marker = Marker(graph.Root, "Fault locator " + (index + 1), mats["red"], .0050f);
                marker.transform.localPosition = point + Vector3.up * .0040f;
                marker.enabled = true;
                marker.gameObject.SetActive(false);
                return marker.gameObject;
            }).ToArray();
            BoardText(graph.Root, "MMX-12  /  MEMORY MATRIX", new Vector3(-.081f, .0032f, -.078f), .00075f, new Color(.87f, .92f, 1f));
            BoardText(graph.Root, "U9 / W125", to + new Vector3(-.006f, .0021f, .006f), .00040f, Color.white);
            var silk = new Color(.71f, .81f, .91f);
            BoardText(graph.Root, "U1  MEMORY CTRL", Pad("U1", new Vector3(0, .0024f, -.018f)), .00062f, silk);
            BoardText(graph.Root, "U2  DDR4 / BYTE 0–1", Pad("U2", new Vector3(0, .0024f, .015f)), .00062f, silk);
            BoardText(graph.Root, "U3  DDR4 / BYTE 2–3", Pad("U3", new Vector3(0, .0024f, -.016f)), .00062f, silk);
            BoardText(graph.Root, "RN1  CA TERM", Pad("RN1", new Vector3(.006f, .0024f, -.010f)), .00053f, silk);
            WorkLight(graph.Root, "Bench key", new Vector3(-.072f, .071f, -.038f), new Color(1f, .91f, .79f), 9.2f);
            WorkLight(graph.Root, "Lane fill", new Vector3(.072f, .073f, .039f), new Color(.66f, .84f, 1f), 8.7f);
        }

        private static Vector3 FindPad(Vector3 site, int corner, List<Vector3> used)
        {
            for (int ring = 0; ring < 8; ring++)
            {
                for (int attempt = 0; attempt < 8; attempt++)
                {
                    float angle = (corner + attempt) * Mathf.PI / 4f;
                    float radius = .0042f + ring * .0019f;
                    var candidate = new Vector3(site.x + radius * Mathf.Cos(angle), .0032f, site.z + radius * Mathf.Sin(angle));
                    if (Mathf.Abs(candidate.x) > .125f || Mathf.Abs(candidate.z) > .089f ||
                        used.Exists(point => Vector2.SqrMagnitude(new Vector2(point.x - candidate.x, point.z - candidate.z)) < .0000022f)) continue;
                    used.Add(candidate);
                    return candidate;
                }
            }
            throw new InvalidOperationException("No room for MMX-12 training contacts at " + site);
        }

        private static Transform Child(Transform parent, string name, Vector3 position = default)
        {
            var node = new GameObject(name).transform;
            node.SetParent(parent, false);
            node.localPosition = position;
            return node;
        }

        private static Renderer Primitive(Transform parent, string name, PrimitiveType kind, Vector3 position, Vector3 scale, Material mat)
        {
            var go = GameObject.CreatePrimitive(kind);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            go.transform.localScale = scale;
            Object.DestroyImmediate(go.GetComponent<Collider>());
            var renderer = go.GetComponent<Renderer>();
            renderer.sharedMaterial = mat;
            return renderer;
        }

        private static Renderer Marker(Transform parent, string name, Material material, float diameter)
        {
            var renderer = Primitive(parent, name, PrimitiveType.Cylinder, Vector3.zero,
                new Vector3(diameter, .00024f, diameter), material);
            renderer.enabled = false;
            return renderer;
        }

        private static void WorkLight(Transform root, string name, Vector3 position, Color color, float intensity)
        {
            var node = Child(root, name, position);
            node.localRotation = Quaternion.Euler(90f, 0, 0);
            var light = node.gameObject.AddComponent<Light>();
            light.type = LightType.Spot;
            light.range = 7.5f;
            light.spotAngle = 103f;
            light.innerSpotAngle = 68f;
            light.shadows = LightShadows.None;
            light.color = color;
            light.intensity = intensity;
        }

        private static void BoardText(Transform parent, string content, Vector3 position, float size, Color color)
        {
            var node = Child(parent, content, position);
            node.localRotation = Quaternion.Euler(90f, 0, 0);
            var text = node.gameObject.AddComponent<TextMesh>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            node.GetComponent<MeshRenderer>().sharedMaterial = text.font.material;
            text.text = content;
            text.anchor = TextAnchor.MiddleCenter;
            text.characterSize = size;
            text.fontSize = 48;
            text.color = color;
        }

        private static LineRenderer Copper(Transform parent, Material material, string name, Vector3 a, Vector3 b)
        {
            var line = Child(parent, name + " copper").gameObject.AddComponent<LineRenderer>();
            line.useWorldSpace = false;
            line.sharedMaterial = material;
            line.widthMultiplier = name == "W125" ? .041f : .023f;
            line.positionCount = 3;
            line.SetPosition(0, a);
            line.SetPosition(1, new Vector3(b.x, Mathf.Max(a.y, b.y) + (name == "W125" ? .005f : 0f), a.z));
            line.SetPosition(2, b);
            line.enabled = false;
            return line;
        }

        private sealed class Circuit
        {
            public Transform Root { get; }
            private readonly SparkElectronicObject padOwner;

            public Circuit(Transform root)
            {
                Root = root;
                padOwner = Child(root, "Native training contacts").gameObject.AddComponent<SparkElectronicObject>();
            }

            public SparkTerminal Pin(string name, Vector3 position = default, bool clickable = false)
            {
                var node = Child(Root, name, position).gameObject;
                if (clickable)
                {
                    node.layer = LayerMask.NameToLayer("Pin");
                    node.AddComponent<SphereCollider>().radius = .0011f;
                }
                var pin = node.AddComponent<SparkTerminal>();
                Set(pin, ("owner", padOwner), ("maxConnections", 12), ("providesOutput", true), ("acceptsInput", true));
                return pin;
            }

            public SparkPowerSupply Supply(string name, SparkTerminal positive, SparkTerminal negative, float voltage)
            {
                var device = Child(Root, name).gameObject.AddComponent<SparkPowerSupply>();
                Set(device, ("positiveTerminal", positive), ("negativeTerminal", negative), ("outputVoltage", voltage),
                    ("currentLimit", 4f), ("state", SparkPowerSupplyState.Active));
                positive.transform.SetParent(device.transform, true);
                Set(positive, ("owner", device));
                return device;
            }

            public SparkSwitch Contact(string name, SparkTerminal a, SparkTerminal b, bool closed = false)
            {
                var contact = Child(Root, name).gameObject.AddComponent<SparkSwitch>();
                Set(contact, ("inputTerminal", a), ("outputTerminal", b), ("closedResistance", .001f),
                    ("state", closed ? SparkSwitchState.Closed : SparkSwitchState.Open));
                return contact;
            }

            public SparkSwitch SwitchedOutput(string name, SparkTerminal a, SparkTerminal b, bool closed = false)
            {
                var contact = Contact(name, a, b, closed);
                b.transform.SetParent(contact.transform, true);
                return contact;
            }

            public SparkCapacitor Cap(string name, SparkTerminal positive, SparkTerminal negative, float farads)
            {
                var device = Child(Root, name).gameObject.AddComponent<SparkCapacitor>();
                Set(device, ("positiveTerminal", positive), ("negativeTerminal", negative), ("capacitanceFarads", farads), ("voltageRating", 6.3f));
                positive.transform.SetParent(device.transform, true);
                return device;
            }

            public SparkResistor Load(string name, SparkTerminal a, SparkTerminal b, float ohms)
            {
                var resistor = Child(Root, name).gameObject.AddComponent<SparkResistor>();
                Set(resistor, ("terminalA", a), ("terminalB", b), ("resistance", ohms), ("maximumPowerWatts", 120f));
                Set(resistor.GetComponent<SparkResistorBurn>(), ("ratedWatts", 120f),
                    ("thermalCapacityJPerC", .8f), ("coolingWattsPerC", .35f));
                return resistor;
            }
        }
    }
}
