using System;
using System.Collections.Generic;
using System.Linq;
using ProjectSpark.Gameplay;
using ProjectSpark.Gameplay.HiddenLayers;
using SparkLevel101Process = ProjectSpark.Gameplay.HiddenLayers.SparkLevel101Process;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ProjectSpark.EditorTools.HiddenLayers
{
    internal static partial class SparkLevel101Builder
    {
        private static readonly string[] PairTasks = { "W053", "W054", "W055", "W056" };
        private static readonly string[] PairNets = { "LANE_A_P", "LANE_A_N", "LANE_B_P", "LANE_B_N" };

        private static void ConfigureBoard(SparkLevel101Process board, Circuit graph, string[][] plans,
            IEnumerable<string> barrelIds, Dictionary<string, Material> mats, List<string> csv)
        {
            board.input = graph.Pin("SUPPLY_POS");
            board.ground = graph.Pin("SUPPLY_RET");
            board.source = graph.Supply("TRAINING_INPUT", board.input, board.ground, 5f);
            board.ground.transform.SetParent(board.source.transform, true);
            Set(board.ground, ("owner", board.source));
            graph.Load("Continuity reference", board.input, board.ground, 10000f);
            board.vdd = graph.Pin("U4.LOGIC_3V3");
            board.laneRail = graph.Pin("U10.TRANSCEIVER_1V8");
            board.lanes = new[] { new SparkLevel101Process.Lane(), new SparkLevel101Process.Lane() };
            var remaining = new Queue<string>(barrelIds.Where(id => id != "V09"));
            var groups = plans.GroupBy(row => row[1]).ToArray();
            var routes = new List<SparkLevel101Process.Route>();
            var witnesses = new List<SparkTerminal>();
            var pads = new List<Vector3>();
            var placements = board.parts.Select(p => graph.Root.InverseTransformPoint(p.footprint.position)).ToArray();
            Vector3 Site(string id) => placements[Array.FindIndex(board.parts, part => part.label == id)];
            for (int group = 0; group < groups.Length; group++)
            {
                int viasLeft = group == 5 ? 3 : 4;
                SparkTerminal previous = board.input;
                foreach (var task in groups[group])
                {
                    int index = routes.Count;
                    string id = task[0];
                    int pair = Array.IndexOf(PairTasks, id);
                    bool signal = pair >= 0;
                    bool flash = id == "W035";
                    bool stitch = id == "W073";
                    bool deviceRoute = signal || flash || stitch;
                    Vector3 entry, destination;
                    if (signal)
                    {
                        var launch = Site(pair < 2 ? "U6" : "U9");
                        var receive = Site(pair < 2 ? "U7" : "U11");
                        float offset = (pair % 2) * .0022f;
                        entry = new Vector3(launch.x + .008f + offset, .0022f, launch.z + .013f + offset);
                        destination = new Vector3(receive.x - .008f + offset, .0022f, receive.z + .013f + offset);
                        pads.Add(entry);
                        pads.Add(destination);
                    }
                    else
                    {
                        entry = FindPad(flash ? Site("U1") : placements[index % placements.Length], index >= placements.Length ? 3 : 0, pads);
                        destination = FindPad(flash ? Site("U2") : placements[index % placements.Length], 2, pads);
                    }
                    string net = signal ? PairNets[pair] : flash ? "SPI_SCK" : stitch ? "L5_RETURN_STITCH" : "CHECK_" + (group + 1);
                    var a = graph.Pin(signal ? net + ".TX" : flash ? "U1.SPI_SCK" : stitch ? "TP.L5_RETURN" : id + ".A", entry, true);
                    var b = graph.Pin(signal ? net + ".RX" : flash ? "V09.SPI_SCK_ENTRY" : stitch ? "L5.STITCH_LAND" : id + ".B", destination, true);
                    var route = new SparkLevel101Process.Route
                    {
                        id = id, group = groups[group].Key, net = net, entry = entry, destination = destination,
                        layer = signal || flash ? 1 : stitch ? 5 : (index + group) % 6 + 1,
                        from = a, to = b, copper = Copper(graph.Root, mats[signal ? "Signal_Cyan" : "Copper_Gold"], id, entry, destination)
                    };
                    var assemblyOut = deviceRoute ? graph.Pin(id + ".ASSEMBLY_CHECK") : a;
                    var contact = graph.SwitchedOutput(id + " assembly solder contact", previous, assemblyOut, index >= board.parts.Length);
                    if (index < board.parts.Length) board.parts[index].contact = contact;
                    previous = deviceRoute ? assemblyOut : b;
                    if (signal)
                    {
                        var lane = board.lanes[pair / 2];
                        var high = graph.Contact(net + " high driver", board.laneRail, a);
                        var low = graph.Contact(net + " low driver", a, board.ground);
                        graph.Load(net + " receiver bias", b, board.ground, 10000f);
                        if (pair == 0) board.skewRoute = index;
                        if (pair % 2 == 0)
                        {
                            lane.positive = index;
                            lane.receiveP = b;
                            lane.pHigh = high;
                            lane.pLow = low;
                        }
                        else
                        {
                            lane.negative = index;
                            lane.receiveN = b;
                            lane.nHigh = high;
                            lane.nLow = low;
                        }
                    }
                    else if (flash)
                    {
                        board.memoryRoute = index;
                        board.flashTx = a;
                        board.flashRx = graph.Pin("U2.SCK_L3", destination);
                        route.viaId = "V09";
                        route.exitLayer = 3;
                        route.barrel = graph.SwitchedOutput("V09 memory clock barrel", b, board.flashRx);
                        board.flashClock = graph.SwitchedOutput("U1 SPI clock driver", board.vdd, a);
                        graph.Load("U1 SPI driver bias", a, board.ground, 10000f);
                        graph.Load("U2 SPI receiver bias", board.flashRx, board.ground, 10000f);
                    }
                    else if (stitch)
                    {
                        board.returnRoute = index;
                        board.returnProbe = a;
                        graph.Load("L5 calibration excitation 180 ohm", board.vdd, a, 180f);
                        var returnLand = graph.Pin("L5.NATIVE_GROUND_STITCH");
                        board.groundStitch = graph.SwitchedOutput("W073 L5 ground stitch", b, returnLand, true);
                        graph.Load("L5 short return", returnLand, board.ground, .12f);
                        graph.Load("L5 alternate long return", b, board.ground, 39f);
                    }
                    else if (viasLeft > 0)
                    {
                        viasLeft--;
                        route.viaId = remaining.Dequeue();
                        route.layer = (index + group) % 5 + 1;
                        route.exitLayer = 6;
                        previous = graph.Pin(route.viaId + ".L6", destination);
                        route.barrel = graph.SwitchedOutput(route.viaId + " plated barrel", b, previous);
                    }
                    routes.Add(route);
                    csv.Add(Row(route));
                }
                graph.Load(groups[group].Key + " continuity witness", previous, board.ground, 1000f);
                witnesses.Add(previous);
            }
            if (remaining.Count != 0 || routes.Count != SparkLevel101Process.RouteCount ||
                routes.Count(route => route.barrel) != SparkLevel101Process.ViaCount ||
                routes.Select(route => route.layer).Distinct().Count() != 6 || witnesses.Count != 6 ||
                board.parts.Any(part => !part.contact) || routes[board.memoryRoute].viaId != "V09")
                throw new InvalidOperationException("MLX-9 route or six-layer design is incomplete.");
            board.routes = routes.ToArray();
            board.witnesses = witnesses.ToArray();
            board.lanes[0].termination = graph.Load("Lane A differential termination", board.lanes[0].receiveP, board.lanes[0].receiveN, 100f);
            board.lanes[1].termination = graph.Load("Lane B differential termination", board.lanes[1].receiveP, board.lanes[1].receiveN, 100f);
        }

        private static void ConfigureController(SparkLevel101Process board, Circuit graph,
            Dictionary<string, Material> mats, List<string> csv)
        {
            board.standby = graph.Pin("PROTECTED_5V_IN");
            board.bench = graph.SwitchedOutput("J1 guarded bench disconnect", board.input, board.standby);
            graph.Load("5 V input witness", board.standby, board.ground, 220f);
            var regulatorSource = graph.Pin("U4_3V3_REGULATOR");
            graph.Supply("U4 logic regulator", regulatorSource, board.ground, 3.3f);
            var ramp = graph.Pin("U4_SERIES_OUTPUT");
            board.regulator = graph.SwitchedOutput("U4 protected regulator gate", regulatorSource, ramp);
            var series = graph.Load("3V3 source impedance", ramp, board.vdd, .045f);
            board.vdd.transform.SetParent(series.transform, true);
            graph.Cap("MCU rail effective decoupling", board.vdd, board.ground, .11f);
            graph.Load("MCU rail bleed", board.vdd, board.ground, 120f);
            var transceiverSource = graph.Pin("U6 regulated output");
            graph.Supply("U10 training transceiver regulator", transceiverSource, board.ground, 1.8f);
            var laneFeed = graph.Pin("U6 protected output");
            board.transceiverRegulator = graph.SwitchedOutput("U10 enable gate", transceiverSource, laneFeed);
            graph.Load("U6 source impedance", laneFeed, board.laneRail, .05f);
            graph.Cap("1.8 V effective rail decoupling", board.laneRail, board.ground, .059f);
            graph.Load("1.8 V return witness", board.laneRail, board.ground, 100f);

            var clockInput = graph.Pin("U5_STARTUP_FEED");
            board.clockDrive = graph.SwitchedOutput("U5 clock bias enable", board.vdd, clockInput);
            board.clockSense = graph.Pin("U5.REFERENCE_ENVELOPE");
            var clockBias = graph.Load("U5 bias and source impedance", clockInput, board.clockSense, 1f);
            board.clockSense.transform.SetParent(clockBias.transform, true);
            graph.Load("U5 load bleed", board.clockSense, board.ground, 100f);
            var clockDrain = graph.Pin("U5.RESET_DISCHARGE");
            board.clockDischarge = graph.SwitchedOutput("U5 discharge contact", board.clockSense, clockDrain, true);
            graph.Load("U5 discharge impedance", clockDrain, board.ground, .47f);
            var clockStorage = graph.Pin("U5.STARTUP_STORAGE");
            graph.Contact("U5 load contact", board.clockSense, clockStorage, true);
            graph.Cap("U5 effective startup capacitance", clockStorage, board.ground, .067f);

            var resetIn = graph.Pin("RESET_PULLUP_INPUT");
            board.resetRelease = graph.SwitchedOutput("Independent reset supervisor", board.vdd, resetIn);
            board.resetSense = graph.Pin("U1.RESET_N");
            var resetPullup = graph.Load("Controller reset pull-up", resetIn, board.resetSense, 180f);
            board.resetSense.transform.SetParent(resetPullup.transform, true);
            graph.Load("Reset input pulldown", board.resetSense, board.ground, 1120f);

            Vector3 Pad(string id, Vector3 offset) =>
                graph.Root.InverseTransformPoint(board.parts.Single(part => part.label == id).footprint.position) + offset;
            Vector3 from = Pad("U7", new Vector3(.009f, .0030f, .005f));
            Vector3 to = Pad("U8", new Vector3(-.008f, .0030f, -.004f));
            var sender = graph.Pin("U7.TRUSTED_LINK_HEALTH_OUT", from, true);
            var receiver = graph.Pin("U8.TRUSTED_LINK_HEALTH_IN", to, true);
            board.heartbeat = graph.SwitchedOutput("U7 validity pulse driver", board.vdd, sender);
            var bias = graph.Load("U8 receiver bias", receiver, board.ground, 10000f);
            receiver.transform.SetParent(bias.transform, true);
            board.jumper = new SparkLevel101Process.Route
            {
                id = "W101", net = "TRUSTED_LINK_HEALTH", group = "Independent safety", layer = 1,
                from = sender, to = receiver, entry = from, destination = to,
                copper = Copper(graph.Root, mats["Copper_Gold"], "W101", from, to)
            };
            csv.Add(Row(board.jumper));
            foreach (var point in new[] { from, to })
                Primitive(graph.Root, "W101 contact", PrimitiveType.Cylinder, point,
                    new Vector3(.0025f, .00022f, .0025f), mats["Copper_Gold"]);

            board.monitorSupply = graph.Pin("U8.INDEPENDENT_3V3");
            var supervisorFeed = graph.Load("Independent supervisor supply", board.vdd, board.monitorSupply, 1f);
            board.monitorSupply.transform.SetParent(supervisorFeed.transform, true);
            graph.Load("Independent supervisor bias", board.monitorSupply, board.ground, 1000f);
            board.protectedFeed = graph.Pin("U8.PROTECTED_TX");
            board.workload = graph.SwitchedOutput("U8 independent transmission permit", board.monitorSupply, board.protectedFeed);
            graph.Load("Protected TX permit witness", board.protectedFeed, board.ground, 330f);

            board.startMarker = Marker(graph.Root, "Source locator", mats["Copper_Gold"], .0026f);
            board.endMarker = Marker(graph.Root, "Destination locator", mats["LED_Green"], .0026f);
            board.locationMarker = Marker(graph.Root, "Placement locator", mats["Fault_Amber"], .0049f);
            board.statusLamp = Primitive(graph.Root, "U8 permit lamp", PrimitiveType.Cube,
                to + new Vector3(-.003f, .001f, 0), new Vector3(.0018f, .0006f, .0012f), mats["LED_Green"]);
            var risks = new[] { board.routes[board.memoryRoute].destination, board.routes[board.returnRoute].destination,
                Vector3.Lerp(board.routes[board.skewRoute].entry, board.routes[board.skewRoute].destination, .5f) };
            board.faultOverlays = risks.Select((point, index) =>
            {
                var marker = Marker(graph.Root, "Fault locator " + (index + 1), mats["Fault_Amber"], .005f);
                marker.transform.localPosition = point + Vector3.up * .0028f;
                marker.enabled = true;
                marker.gameObject.SetActive(false);
                return marker.gameObject;
            }).ToArray();
            BoardText(graph.Root, "MLX-9  /  THE HIDDEN LAYERS", new Vector3(-.082f, .0027f, -.069f), .00075f, new Color(.87f, .92f, 1f));
            BoardText(graph.Root, "U8 / W101", to + new Vector3(-.006f, .0021f, .006f), .00040f, Color.white);
            WorkLight(graph.Root, "Bench key", new Vector3(-.072f, .071f, -.038f), new Color(1f, .91f, .79f), 7.0f);
            WorkLight(graph.Root, "Lane fill", new Vector3(.072f, .073f, .039f), new Color(.66f, .84f, 1f), 6.8f);
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
                    if (Mathf.Abs(candidate.x) > .111f || Mathf.Abs(candidate.z) > .076f ||
                        used.Exists(point => Vector2.SqrMagnitude(new Vector2(point.x - candidate.x, point.z - candidate.z)) < .0000022f)) continue;
                    used.Add(candidate);
                    return candidate;
                }
            }
            throw new InvalidOperationException("No room for DNX-8 training contacts at " + site);
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
            line.widthMultiplier = name == "W101" ? .041f : .023f;
            line.positionCount = 3;
            line.SetPosition(0, a);
            line.SetPosition(1, new Vector3(b.x, Mathf.Max(a.y, b.y) + (name == "W101" ? .005f : 0f), a.z));
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
